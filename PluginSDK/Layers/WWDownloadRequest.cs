using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using WorldWind;
using WorldWind.Camera;
using WorldWind.Net;
using WorldWind.Terrain;
using WorldWind.VisualControl;

namespace WorldWind.Renderable
{
   public abstract class GeoSpatialDownloadRequest : IDisposable
   {
      public int iDownloadPos = 0;
      public int iDownloadTotal = 2048;
      protected WebDownload m_oDownload;
      protected string m_strImagePath;
      protected DownloadQueue m_oQueue;
      protected GeographicBoundingBox m_oGeoBoundary;
      protected IImageAccessor m_oImageAcessor;
      protected Texture m_oTexture;
      protected bool m_isStarted = false;
      private bool disposed = false;

      public delegate void LoadingComlpetedCallbackHandler();
      public LoadingComlpetedCallbackHandler LoadingCompleted = null;

      /// <summary>
      /// Initializes a new instance of the <see cref= "T:WorldWind.Renderable.GeoSpatialDownloadRequest"/> class.
      /// </summary>
      /// <param name="quadTile"></param>
      public GeoSpatialDownloadRequest(GeographicBoundingBox geoBox, string imagePath, DownloadQueue queue, IImageAccessor imageAcessor)
      {
         m_strImagePath = imagePath;
         m_oQueue = queue;
         m_oImageAcessor = imageAcessor;

         m_oGeoBoundary = geoBox;
      }

      #region Properties

      /// <summary>
      /// Whether the request is currently being downloaded
      /// </summary>
      public bool IsDownloading
      {
         get
         {
            return (m_oDownload != null);
         }
      }

      public bool IsComplete
      {
         get
         {
            if (m_oDownload == null)
               return m_isStarted;
            return m_oDownload.IsComplete;
         }
      }

      public double TileWidth
      {
         get
         {
            return Boundary.East - Boundary.West;
         }
      }

      public Angle CenterLatitude
      {
         get
         {
            return Angle.FromDegrees((Boundary.North + Boundary.South) / 2);
         }
      }

      public Angle CenterLongitude
      {
         get
         {
            return Angle.FromDegrees((Boundary.East + Boundary.West) / 2);
         }
      }

      public GeographicBoundingBox Boundary
      {
         get
         {
            return m_oGeoBoundary;
         }
      }

      public Texture Texture
      {
         get
         {
            return m_oTexture;
         }
      }

      public DateTime StartTime
      {
         get
         {
            if (m_oDownload == null)
               return DateTime.MinValue;
            return m_oDownload.DownloadStartTime;
         }
      }

      #endregion

      private void DownloadComplete(WorldWind.Net.WebDownload downloadInfo)
      {
         try
         {
            downloadInfo.Verify();
            CompletedDownload(downloadInfo);
            m_oQueue.NumberRetries = 0;
         }
         catch
         {
            DownloadFailed(downloadInfo);
            m_oQueue.NumberRetries++;
         }
         finally
         {
            if (m_oDownload != null)
               m_oDownload.IsComplete = true;
            m_oQueue.RemoveFromDownloadQueue(this);

            //Immediately queue next download
            m_oQueue.ServiceDownloadQueue();
            m_oDownload = null;
         }
      }

      public virtual void StartDownload()
      {
         m_isStarted = true;
         m_oDownload = BeginDownload();
         m_oDownload.ProgressCallback += new DownloadProgressHandler(UpdateProgress);
         m_oDownload.CompleteCallback += new DownloadCompleteHandler(DownloadComplete);
         m_oDownload.BackgroundDownloadFile();
      }

      /// <summary>
      /// Sets the specific download object required for the donwload
      /// e.g. 
      ///  WebDownload GetDownloadObject()
      ///  {
      ///     return new WebDownload(url);
      ///  }
      /// </summary>
      /// <returns>A WebDownload based object</returns>
      protected abstract WebDownload BeginDownload();
      /// <summary>
      /// Actions required when a download is completed
      /// e.g. moving from the temp file to the final file
      /// </summary>
      protected abstract void CompletedDownload(WorldWind.Net.WebDownload downloadInfo);
      /// <summary>
      /// Actions required if the download throws an exception
      /// </summary>
      protected abstract void DownloadFailed(WorldWind.Net.WebDownload downloadInfo);

      void UpdateProgress(int pos, int total)
      {
         iDownloadPos = pos;

         if (total < pos)
         {
            // Provide rolling progress (starts with 2048 estimate)
            while (iDownloadTotal < pos)
               iDownloadTotal *= 2;
         }
         else
            iDownloadTotal = total;
      }

      public virtual void Cancel()
      {
         if (m_oDownload != null)
            m_oDownload.Cancel();
      }

      public override string ToString()
      {
         return m_strImagePath;
      }

      #region IDisposable Implementation
      protected virtual void Dispose(bool disposing)
      {
         if (!disposed)
         {
            if (m_oDownload != null)
            {
               m_oDownload.Dispose();
               m_oDownload = null;
            }
            disposed = true;

            // Suppress finalization of this disposed instance.
            if (disposing)
            {
               GC.SuppressFinalize(this);
            }
         }
      }

      public void Dispose()
      {
         Dispose(true);
      }

      ~GeoSpatialDownloadRequest()
      {
         Dispose(false);
      }
      #endregion
   }

   public class WWDownloadRequest : GeoSpatialDownloadRequest
   {
      WebDownload m_Download;
      string uri;

      public WWDownloadRequest(GeographicBoundingBox geoBox, ImageTileInfo imageTileInfo, DownloadQueue queue, IImageAccessor imageAcessor)
         : base(geoBox, imageTileInfo.ImagePath, queue, imageAcessor)
      {
         uri = imageTileInfo.Uri;
         if (File.Exists(m_strImagePath))
         {
            // cause it to believe that the download is complete if the file exists
            m_isStarted = true;
         }
      }

      protected override WebDownload BeginDownload()
      {
         m_Download = new WebDownload(uri, false);
         m_Download.SavedFilePath = m_strImagePath + ".tmp";
         return m_Download;
      }

      protected override void CompletedDownload(WorldWind.Net.WebDownload downloadInfo)
      {
         File.Delete(this.m_strImagePath);
         File.Move(downloadInfo.SavedFilePath, this.m_strImagePath);
      }

      protected override void DownloadFailed(WorldWind.Net.WebDownload downloadInfo)
      {
         if (!File.Exists(m_strImagePath + ".fail"))
         {
            using (File.Create(m_strImagePath + ".fail"))
            {
            }
         }
         if (File.Exists(downloadInfo.SavedFilePath))
         {
            try
            {
               File.Delete(downloadInfo.SavedFilePath);
            }
            catch { }
         }
      }
   }
}
