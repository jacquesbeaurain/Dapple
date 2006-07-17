using WorldWind;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace WorldWind.Renderable
{
   public class DownloadQueue : IDisposable
   {
      public static int MaxConcurrentDownloads = 2;

      int _numberRetries;
      Hashtable m_downloadRequests = new Hashtable();
      int _maxQueueSize = 200;
      Hashtable _wmsQueue = new Hashtable();
      Queue _recentSelectedThreads = new Queue();

      GeoSpatialDownloadRequest[] m_activeDownloads = new GeoSpatialDownloadRequest[MaxConcurrentDownloads];
      DateTime[] m_downloadStarted = new DateTime[MaxConcurrentDownloads];
      TimeSpan m_connectionWaitTime = TimeSpan.FromMinutes(2);
      DateTime m_connectionWaitStart;
      bool m_isConnectionWaiting;
      WorldWind.Camera.CameraBase m_camera;

      public int NumberRetries
      {
         get
         {
            return this._numberRetries;
         }
         set
         {
            this._numberRetries = value;
         }
      }

      public DateTime ConnectionWaitStart
      {
         get
         {
            return m_connectionWaitStart;
         }
      }

      public bool IsConnectionWaiting
      {
         get
         {
            return m_isConnectionWaiting;
         }
      }

      /// <summary>
      /// Tiles in the request for download queue
      /// </summary>
      public Hashtable DownloadRequests
      {
         get
         {
            return m_downloadRequests;
         }
      }

      /// <summary>
      /// Currently downloading tiles 
      /// </summary>
      public GeoSpatialDownloadRequest[] ActiveDownloads
      {
         get
         {
            return m_activeDownloads;
         }
      }

      public virtual void AddToDownloadQueue(DrawArgs drawArgs, GeoSpatialDownloadRequest newRequest)
      {
         string key = newRequest.ToString();
         lock (m_downloadRequests.SyncRoot)
         {
            if (m_downloadRequests.Contains(key))
               return;

            m_downloadRequests.Add(key, newRequest);

            if (m_downloadRequests.Count >= this._maxQueueSize)
            {
               //remove spatially farthest request
               GeoSpatialDownloadRequest farthestRequest = null;
               Angle curDistance = Angle.Zero;
               Angle farthestDistance = Angle.Zero;
               foreach (GeoSpatialDownloadRequest curRequest in m_downloadRequests.Values)
               {
                  curDistance = MathEngine.SphericalDistance(
                     curRequest.CenterLatitude,
                     curRequest.CenterLongitude,
                     drawArgs.WorldCamera.Latitude,
                     drawArgs.WorldCamera.Longitude);

                  if (curDistance > farthestDistance)
                  {
                     farthestRequest = curRequest;
                     farthestDistance = curDistance;
                  }
               }

               m_downloadRequests.Remove(farthestRequest.ToString());
            }
         }

         ServiceDownloadQueue();
      }

      /// <summary>
      /// Removes a request from the download queue.
      /// </summary>
      public virtual void RemoveFromDownloadQueue(GeoSpatialDownloadRequest removeRequest)
      {
         lock (m_downloadRequests.SyncRoot)
         {
            string key = removeRequest.ToString();
            GeoSpatialDownloadRequest request = (GeoSpatialDownloadRequest)m_downloadRequests[key];
            if (request != null)
            {
               m_downloadRequests.Remove(key);
               //if (request.QuadTile != null)
               //    request.QuadTile.DownloadRequest = null;
            }
         }
      }

      /// <summary>
      /// The camera controlling the layers update logic
      /// </summary>
      public WorldWind.Camera.CameraBase Camera
      {
         get
         {
            return m_camera;
         }
         set
         {
            m_camera = value;
         }
      }

      /// <summary>
      /// Starts downloads when there are threads available
      /// </summary>
      public virtual void ServiceDownloadQueue()
      {
         lock (m_downloadRequests.SyncRoot)
         {
            for (int i = 0; i < MaxConcurrentDownloads; i++)
            {
               if (m_activeDownloads[i] != null)
               {
                  if (!m_activeDownloads[i].IsComplete)
                  {
                     if ( DateTime.Now.Subtract(m_downloadStarted[i]).Seconds >= 40 )
                     {
                        m_activeDownloads[i].Cancel();
                        m_activeDownloads[i].StartDownload();
                        m_downloadStarted[i] = DateTime.Now;
                     }
                     continue;
                  }
                  else
                  {
                     m_activeDownloads[i].Cancel();
                     m_activeDownloads[i].Dispose();
                  }
               }
               m_activeDownloads[i] = GetClosestDownloadRequest();
               if (m_activeDownloads[i] != null)
               {
                  m_downloadStarted[i] = DateTime.Now;
                  m_activeDownloads[i].StartDownload();
               }
            }

            if (NumberRetries >= 5 || m_isConnectionWaiting)
            {
               // Anti hammer in effect
               if (!m_isConnectionWaiting)
               {
                  m_connectionWaitStart = DateTime.Now;
                  m_isConnectionWaiting = true;
               }

               if (DateTime.Now.Subtract(m_connectionWaitTime) > m_connectionWaitStart)
               {
                  NumberRetries = 0;
                  m_isConnectionWaiting = false;
               }
               return;
            }

            // Queue new downloads
            //for (int i = 0; i < MaxConcurrentDownloads; i++)
            //{
            //    if (m_activeDownloads[i] != null)
            //        continue;

            //    if (m_downloadRequests.Count <= 0)
            //        continue;

            //    m_activeDownloads[i] = GetClosestDownloadRequest();
            //    if (m_activeDownloads[i] != null)
            //    {
            //        m_downloadStarted[i] = DateTime.Now;
            //        m_activeDownloads[i].StartDownload();
            //    }
            //}
         }
      }

      ArrayList deletionList = new ArrayList();

      /// <summary>
      /// Finds the "best" tile from queue
      /// </summary>
      public virtual GeoSpatialDownloadRequest GetClosestDownloadRequest()
      {
         GeoSpatialDownloadRequest closestRequest = null;
         GeoSpatialDownloadRequest firstRequest = null;
         double largestArea = double.MinValue;

         lock (m_downloadRequests.SyncRoot)
         {
            foreach (GeoSpatialDownloadRequest curRequest in m_downloadRequests.Values)
            {
               if (curRequest.IsDownloading)
                  continue;

               BoundingBox bb = new BoundingBox((float)curRequest.Boundary.South,
                   (float)curRequest.Boundary.North,
                   (float)curRequest.Boundary.West,
                   (float)curRequest.Boundary.East,
                   (float)m_camera.WorldRadius,
                   (float)m_camera.WorldRadius + 300000f);
               if (!m_camera.ViewFrustum.Intersects(bb))
               {
                  deletionList.Add(curRequest);
                  continue;
               }

               double screenArea = bb.CalcRelativeScreenArea(m_camera);
               if (screenArea > largestArea)
               {
                  largestArea = screenArea;
                  closestRequest = curRequest;
               }
               if (firstRequest != null)
               {
                  firstRequest = curRequest;
               }
            }
         }

         // Remove requests that point to invisible tiles
         foreach (GeoSpatialDownloadRequest req in deletionList)
         {
            m_downloadRequests.Remove(req.ToString());
            //if (req.QuadTile != null)
            //    req.QuadTile.DownloadRequest = null;
         }
         deletionList.Clear();

         if (closestRequest == null && firstRequest != null)
         {
            closestRequest = firstRequest;
         }

         return closestRequest;
      }

      public virtual void RemoveWMSFromQueue(WorldWind.Net.WMSDownload oldRequest)
      {
         try
         {
            if (this._wmsQueue.Contains(oldRequest.SavedFilePath))
            {
               lock (this._wmsQueue.SyncRoot)
               {
                  this._wmsQueue.Remove(oldRequest.SavedFilePath);
               }
            }
         }
         catch (Exception caught)
         {
            Utility.Log.Write(caught);
         }
      }

      public void ClearDownloadRequests()
      {
         lock (m_downloadRequests.SyncRoot)
         {
            m_downloadRequests.Clear();
         }
      }

      public bool Contains(GeoSpatialDownloadRequest request)
      {
         bool result = m_downloadRequests.Contains(request);
         if (!result)
         {
            for (int i = 0; i < MaxConcurrentDownloads; i++)
            {
               if (m_activeDownloads[i] == request)
               {
                  result = true;
                  break;
               }
            }
         }
         return result;
      }

      #region IDisposable Members

      public void Dispose()
      {
         for (int i = 0; i < MaxConcurrentDownloads; i++)
         {
            if (m_activeDownloads[i] != null)
            {
               m_activeDownloads[i].Dispose();
               m_activeDownloads[i] = null;
            }
         }
      }

      #endregion
   }
}