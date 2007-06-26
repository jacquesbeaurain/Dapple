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
   /// <summary>
   /// Main class for image tile rendering.  Uses the Terrain Manager to query height values for 3D 
   /// terrain rendering.
   /// Relies on an Update thread to refresh the "tiles" based on lat/lon/view range
   /// </summary>
   public class QuadTileSet : WorldWind.Renderable.RenderableObject
   {
#if DEBUG
      static object lockalloc = new object();
      static int lockcount = 0;
#endif
      #region public members

      public QuadTileArgs QuadTileArgs;

      #endregion

      #region Private Members
      private bool disposed = false;
      OrderedDictionary m_updateTiles = new OrderedDictionary();
      Hashtable m_topmostTiles = new Hashtable();
      List<long> m_deleteTiles = new List<long>();
      protected double m_north;
      protected double m_south;
      protected double m_west;
      protected double m_east;
      BoundingBox m_TestBB = new BoundingBox(0, 1, 0, 1, 1, 2);
      #endregion

      public System.Collections.Hashtable TopmostTiles
      {
         get
         {
            return m_topmostTiles;
         }
      }

      public int TransparentColor
      {
         get
         {
            return QuadTileArgs.ImageAccessor.TransparentColor;
         }
         set
         {
            QuadTileArgs.ImageAccessor.TransparentColor = value;
         }
      }

      /// <summary>
      /// Initializes a new instance of the <see cref= "T:WorldWind.Renderable.QuadTileSet"/> class.
      /// </summary>
      /// <param name="name"></param>
      /// <param name="parentWorld"></param>
      /// <param name="distanceAboveSurface"></param>
      /// <param name="north"></param>
      /// <param name="south"></param>
      /// <param name="west"></param>
      /// <param name="east"></param>
      /// <param name="terrainAccessor"></param>
      /// <param name="imageAccessor"></param>

      public QuadTileSet(
          string title,
          GeographicBoundingBox boundary,
         World parentWorld,
         double distanceAboveSurface,
         TerrainAccessor terrainAccessor,
         IImageAccessor imageAccessor,
         byte opacity,
         bool alwaysRenderBaseTiles
         )
         : base(title, parentWorld.Position, Quaternion4d.EulerToQuaternion(0, 0, 0))//TODOVERIFY
      {
#if DEBUG
         lock (lockalloc)
         {
            lockcount++;
            System.Diagnostics.Debug.WriteLine("Allocating QuadTileSet " + lockcount.ToString());
         }
#endif

         float layerRadius = (float)(parentWorld.EquatorialRadius + distanceAboveSurface);
         QuadTileArgs = new QuadTileArgs(
            layerRadius,
            this,
            terrainAccessor,
            imageAccessor,
            alwaysRenderBaseTiles);
         QuadTileArgs.Opacity = opacity;

         // Round to ceiling of four decimals (>~ 10 meter tile squares)
         m_north = Math.Ceiling(10000.0 * Math.Abs(boundary.North)) * Math.Sign(boundary.North) / 10000.0;
         m_south = Math.Ceiling(10000.0 * Math.Abs(boundary.South)) * Math.Sign(boundary.South) / 10000.0;
         m_west = Math.Ceiling(10000.0 * Math.Abs(boundary.West)) * Math.Sign(boundary.West) / 10000.0;
         m_east = Math.Ceiling(10000.0 * Math.Abs(boundary.East)) * Math.Sign(boundary.East) / 10000.0;
         QuadTileArgs.Boundary = boundary;
      }

      /// <summary>
      /// Initializes a new instance of the <see cref= "T:WorldWind.Renderable.QuadTileSet"/> class.
      /// </summary>
      /// <param name="name"></param>
      /// <param name="parentWorld"></param>
      /// <param name="distanceAboveSurface"></param>
      /// <param name="north"></param>
      /// <param name="south"></param>
      /// <param name="west"></param>
      /// <param name="east"></param>
      /// <param name="terrainAccessor"></param>
      /// <param name="imageAccessor"></param>
      /// <param name="tileDrawDistanceFactor"></param>
      /// <param name="tileSpreadFactor"></param>
      public QuadTileSet(
          string title,
          GeographicBoundingBox boundary,
         World parentWorld,
         double distanceAboveSurface,
         TerrainAccessor terrainAccessor,
         IImageAccessor imageAccessor,
         float tileDrawDistanceFactor,
         float tileSpreadFactor,
         byte opacity,
         bool alwaysRenderBaseTiles
         )
         : this(title, boundary, parentWorld, distanceAboveSurface, terrainAccessor, imageAccessor, opacity, alwaysRenderBaseTiles)
      {
         QuadTileArgs.TileDrawDistance = tileDrawDistanceFactor;
         QuadTileArgs.TileDrawSpread = tileSpreadFactor;
      }

      public override void Initialize(DrawArgs drawArgs)
      {
         QuadTileArgs.ImageAccessor.Camera = drawArgs.WorldCamera;

         try
         {
            lock (m_topmostTiles.SyncRoot)
            {
               foreach (QuadTile qt in m_topmostTiles.Values)
                  qt.Initialize(drawArgs);
            }
         }
         catch
         {
         }
         isInitialized = true;
      }

      protected override void FreeResources()
      {
         try
         {
            lock (m_topmostTiles.SyncRoot)
            {
               foreach (QuadTile qt in m_topmostTiles.Values)
                  qt.DisposeChildren();
            }
         }
         catch
         {
         }
      }

      public bool RenderTileFileNames
      {
         get
         {
            return QuadTileArgs.RenderTileFileNames;
         }
         set
         {
            QuadTileArgs.RenderTileFileNames = value;
         }
      }

      /// <summary>
      /// North bound for this QuadTileSet
      /// </summary>
      public double North
      {
         get
         {
            return m_north;
         }
      }

      /// <summary>
      /// West bound for this QuadTileSet
      /// </summary>
      public double West
      {
         get
         {
            return m_west;
         }
      }

      /// <summary>
      /// South bound for this QuadTileSet
      /// </summary>
      public double South
      {
         get
         {
            return m_south;
         }
      }

      /// <summary>
      /// East bound for this QuadTileSet
      /// </summary>
      public double East
      {
         get
         {
            return m_east;
         }
      }

      public override bool PerformSelectionAction(DrawArgs drawArgs)
      {
         return false;
      }

      public override byte Opacity
      {
         get
         {
            return QuadTileArgs.Opacity;
         }
         set
         {
            QuadTileArgs.Opacity = value;
            foreach (QuadTile q in m_topmostTiles.Values)
            {
               if (q.Opacity != value)
                  q.Opacity = value;
            }
         }
      }

      public override void Update(DrawArgs drawArgs)
      {
         if (!isInitialized)
            Initialize(drawArgs);

         if (this.QuadTileArgs.ImageAccessor.LevelZeroTileSizeDegrees < 180)
         {
            // Check for layer outside view
            double vrd = drawArgs.WorldCamera.ViewRange.Degrees;
            double latitudeMax = drawArgs.WorldCamera.Latitude.Degrees + vrd;
            double latitudeMin = drawArgs.WorldCamera.Latitude.Degrees - vrd;
            double longitudeMax = drawArgs.WorldCamera.Longitude.Degrees + vrd;
            double longitudeMin = drawArgs.WorldCamera.Longitude.Degrees - vrd;
            if (latitudeMax < m_south || latitudeMin > m_north || longitudeMax < m_west || longitudeMin > m_east)
               return;
         }

         if (!QuadTileArgs.AlwaysRenderBaseTiles && drawArgs.WorldCamera.ViewRange * 0.5f >
            Angle.FromDegrees(QuadTileArgs.TileDrawDistance * (double)QuadTileArgs.ImageAccessor.LevelZeroTileSizeDegrees))
         {
            // Used to be that if the camera is too far away, don't render anything (clear the tiles)
            // Now keep these topmost tiles alive, the user gets to see the area he was looking at.
            /* lock (m_topmostTiles.SyncRoot)
             {
                foreach (QuadTile qt in m_topmostTiles.Values)
                   qt.Dispose();
                m_topmostTiles.Clear();
                //QuadTileArgs.ClearDownloadRequests();
             }*/

            return;
         }

         try
         {
            // 'Spiral' from the centre tile outward adding tiles that's in the view
            // Defer the updates to after the loop to prevent tiles from updating twice
            // If the tilespread is huge we are likely looking at a small dataset in the view 
            // so just test all the tiles in the dataset.
            int tileSpread = Math.Max(5, (int)Math.Ceiling(drawArgs.WorldCamera.TrueViewRange.Degrees / (2.0 * (double)QuadTileArgs.ImageAccessor.LevelZeroTileSizeDegrees)));

            int middleRow;
            int middleCol;

            if (tileSpread > 10)
            {
               tileSpread = Math.Max(5, (int)Math.Ceiling(Math.Max(North - South, East - West) / (2.0 * (double)QuadTileArgs.ImageAccessor.LevelZeroTileSizeDegrees)));
               middleRow = MathEngine.GetRowFromLatitude(South + (North - South) / 2.0, (double)QuadTileArgs.ImageAccessor.LevelZeroTileSizeDegrees);
               middleCol = MathEngine.GetColFromLongitude(West + (East - West) / 2.0, (double)QuadTileArgs.ImageAccessor.LevelZeroTileSizeDegrees);
            }
            else
            {
               middleRow = MathEngine.GetRowFromLatitude(drawArgs.WorldCamera.Latitude, (double)QuadTileArgs.ImageAccessor.LevelZeroTileSizeDegrees);
               middleCol = MathEngine.GetColFromLongitude(drawArgs.WorldCamera.Longitude, (double)QuadTileArgs.ImageAccessor.LevelZeroTileSizeDegrees);
            }
            double middleSouth = -90.0f + middleRow * (double)QuadTileArgs.ImageAccessor.LevelZeroTileSizeDegrees;
            double middleNorth = -90.0f + middleRow * (double)QuadTileArgs.ImageAccessor.LevelZeroTileSizeDegrees + (double)QuadTileArgs.ImageAccessor.LevelZeroTileSizeDegrees;
            double middleWest = -180.0f + middleCol * (double)QuadTileArgs.ImageAccessor.LevelZeroTileSizeDegrees;
            double middleEast = -180.0f + middleCol * (double)QuadTileArgs.ImageAccessor.LevelZeroTileSizeDegrees + (double)QuadTileArgs.ImageAccessor.LevelZeroTileSizeDegrees;

            double middleCenterLat = 0.5f * (middleNorth + middleSouth);
            double middleCenterLon = 0.5f * (middleWest + middleEast);

            m_updateTiles.Clear();
            for (int i = 0; i < tileSpread; i++)
            {
               for (double j = middleCenterLat - i * (double)QuadTileArgs.ImageAccessor.LevelZeroTileSizeDegrees; j < middleCenterLat + i * (double)QuadTileArgs.ImageAccessor.LevelZeroTileSizeDegrees; j += (double)QuadTileArgs.ImageAccessor.LevelZeroTileSizeDegrees)
               {
                  for (double k = middleCenterLon - i * (double)QuadTileArgs.ImageAccessor.LevelZeroTileSizeDegrees; k < middleCenterLon + i * (double)QuadTileArgs.ImageAccessor.LevelZeroTileSizeDegrees; k += (double)QuadTileArgs.ImageAccessor.LevelZeroTileSizeDegrees)
                  {
                     int curRow = MathEngine.GetRowFromLatitude(Angle.FromDegrees(j), (double)QuadTileArgs.ImageAccessor.LevelZeroTileSizeDegrees);
                     int curCol = MathEngine.GetColFromLongitude(Angle.FromDegrees(k), (double)QuadTileArgs.ImageAccessor.LevelZeroTileSizeDegrees);
                     long key = ((long)curRow << 32) + curCol;

                     QuadTile qt = (QuadTile)m_topmostTiles[key];
                     if (qt != null)
                     {
                        if (!m_updateTiles.Contains(key))
                           m_updateTiles.Add(key, qt);
                        continue;
                     }

                     // Check for tile outside layer boundaries
                     double west = -180.0 + curCol * (double)QuadTileArgs.ImageAccessor.LevelZeroTileSizeDegrees;
                     if (west > m_east)
                        continue;

                     double east = west + (double)QuadTileArgs.ImageAccessor.LevelZeroTileSizeDegrees;
                     if (east < m_west)
                        continue;

                     double south = -90.0 + curRow * (double)QuadTileArgs.ImageAccessor.LevelZeroTileSizeDegrees;
                     if (south > m_north)
                        continue;

                     double north = south + (double)QuadTileArgs.ImageAccessor.LevelZeroTileSizeDegrees;
                     if (north < m_south)
                        continue;

                     m_TestBB.Update((float)south, (float)north, (float)west, (float)east, (float)QuadTileArgs.LayerRadius, (float)QuadTileArgs.LayerRadius + 10000 * World.Settings.VerticalExaggeration);
                     if (drawArgs.WorldCamera.ViewFrustum.Intersects(m_TestBB))
                     {
                        qt = new QuadTile(south, north, west, east, 0, QuadTileArgs);
                        lock (m_topmostTiles.SyncRoot)
                           m_topmostTiles.Add(key, qt);
                        m_updateTiles.Add(key, qt);
                     }
                  }
               }
            }

            m_deleteTiles.Clear();
            lock (m_topmostTiles.SyncRoot)
            {
               foreach (long key in m_topmostTiles.Keys)
               {
                  QuadTile qt = (QuadTile)m_topmostTiles[key];
                  if (!drawArgs.WorldCamera.ViewFrustum.Intersects(qt.BoundingBox))
                  {
                     if (m_updateTiles.Contains(key))
                        m_updateTiles.Remove(key);
                     m_deleteTiles.Add(key);
                  }
               }
            }

            // Do updates before cleanup for performance reasons.
            foreach (long key in m_updateTiles.Keys)
               ((QuadTile)m_topmostTiles[key]).Update(drawArgs);
            lock (m_topmostTiles.SyncRoot)
            {
               foreach (long key in m_deleteTiles)
               {
                  QuadTile qt = (QuadTile)m_topmostTiles[key];
                  if (qt != null)
                  {
                     m_topmostTiles.Remove(key);
                     qt.Dispose();
                  }
               }
            }
         }
         catch (System.Threading.ThreadAbortException)
         {
         }
         catch (Exception caught)
         {
            Utility.Log.Write(caught);
         }
      }

      public bool bIsDownloading(out int iBytesRead, out int iTotalBytes)
      {
         bool bIsDownloading = false;

         iBytesRead = 0; iTotalBytes = 0;
         DownloadQueue dlq = QuadTileArgs.ImageAccessor.DownloadQueue;
         for (int i = 0; i < dlq.ActiveDownloads.GetLength(0); i++)
         {
            if (dlq.ActiveDownloads[i] != null)
            {
               iBytesRead += dlq.ActiveDownloads[i].iDownloadPos;
               iTotalBytes += dlq.ActiveDownloads[i].iDownloadTotal;
               bIsDownloading = true;
            }
         }

         return bIsDownloading;
      }

      public override void InitExportInfo(DrawArgs drawArgs, ExportInfo info)
      {
         Update(drawArgs);
         lock (m_topmostTiles.SyncRoot)
         {
            foreach (long key in m_topmostTiles.Keys)
            {
               QuadTile qt = (QuadTile)m_topmostTiles[key];
               qt.InitExportInfo(drawArgs, info);
            }
         }
      }

      public override void ExportProcess(DrawArgs drawArgs, ExportInfo expInfo)
      {
         Update(drawArgs);
         lock (m_topmostTiles.SyncRoot)
         {
            foreach (long key in m_topmostTiles.Keys)
            {
               QuadTile qt = (QuadTile)m_topmostTiles[key];
               qt.ExportProcess(drawArgs, expInfo);
            }
         }
      }

      public override void Render(DrawArgs drawArgs)
      {
         try
         {
            lock (m_topmostTiles.SyncRoot)
            {
               if (m_topmostTiles.Count <= 0)
                  return;

               // Set the render states for rendering of quad tiles.  
               // Any quad tile rendering code that adjusts the state should restore it to below values afterwards.
               drawArgs.device.VertexFormat = CustomVertex.PositionColoredTextured.Format;
               drawArgs.device.TextureState[0].ColorOperation = TextureOperation.SelectArg1;
               drawArgs.device.TextureState[0].AlphaArgument1 = TextureArgument.TextureColor;
               drawArgs.device.TextureState[0].AlphaOperation = TextureOperation.SelectArg1;
               drawArgs.device.Clear(ClearFlags.ZBuffer, 0, 1.0f, 0);
               drawArgs.device.RenderState.ZBufferEnable = true;
               foreach (QuadTile qt in m_topmostTiles.Values)
               {
                  qt.Render(drawArgs);
               }
            }

            // Update download status
            DownloadQueue dlq = QuadTileArgs.ImageAccessor.DownloadQueue;
            for (int i = 0; i < dlq.ActiveDownloads.GetLength(0); i++)
            {
               if (dlq.ActiveDownloads[i] != null)
               {
                  if (World.Settings.ShowDownloadRectangles)
                     RenderDownloadRectangle(drawArgs, dlq.ActiveDownloads[i].Boundary, World.Settings.DownloadProgressColor.ToArgb());
               }

               if (dlq.IsConnectionWaiting)
               {
                  if (DateTime.Now.Subtract(TimeSpan.FromSeconds(15)) < dlq.ConnectionWaitStart)
                  {
                     //        string s = "Problem connecting to server...Trying again in 2 minutes.\n";
                     //        drawArgs.UpperLeftCornerText += s;
                  }
               }
            }
         }
         catch (Exception caught)
         {
            Utility.Log.DebugWrite(caught);
         }
      }

      /// <summary>
      /// Render a rectangle around an image tile in the specified color
      /// </summary>
      public void RenderDownloadRectangle(DrawArgs drawArgs, GeographicBoundingBox boundary, int color)
      {
         CustomVertex.PositionColored[] downloadRectangle = new CustomVertex.PositionColored[5];

         // Render terrain download rectangle
         Vector3d northWestV = MathEngine.SphericalToCartesian((float)boundary.North, (float)boundary.West, QuadTileArgs.LayerRadius);
         Vector3d southWestV = MathEngine.SphericalToCartesian((float)boundary.South, (float)boundary.West, QuadTileArgs.LayerRadius);
         Vector3d northEastV = MathEngine.SphericalToCartesian((float)boundary.North, (float)boundary.East, QuadTileArgs.LayerRadius);
         Vector3d southEastV = MathEngine.SphericalToCartesian((float)boundary.South, (float)boundary.East, QuadTileArgs.LayerRadius);

         downloadRectangle[0].X = (float)northWestV.X;
         downloadRectangle[0].Y = (float)northWestV.Y;
         downloadRectangle[0].Z = (float)northWestV.Z;
         downloadRectangle[0].Color = color;

         downloadRectangle[1].X = (float)southWestV.X;
         downloadRectangle[1].Y = (float)southWestV.Y;
         downloadRectangle[1].Z = (float)southWestV.Z;
         downloadRectangle[1].Color = color;

         downloadRectangle[2].X = (float)southEastV.X;
         downloadRectangle[2].Y = (float)southEastV.Y;
         downloadRectangle[2].Z = (float)southEastV.Z;
         downloadRectangle[2].Color = color;

         downloadRectangle[3].X = (float)northEastV.X;
         downloadRectangle[3].Y = (float)northEastV.Y;
         downloadRectangle[3].Z = (float)northEastV.Z;
         downloadRectangle[3].Color = color;

         downloadRectangle[4].X = downloadRectangle[0].X;
         downloadRectangle[4].Y = downloadRectangle[0].Y;
         downloadRectangle[4].Z = downloadRectangle[0].Z;
         downloadRectangle[4].Color = color;

         drawArgs.device.RenderState.ZBufferEnable = false;
         drawArgs.device.VertexFormat = CustomVertex.PositionColored.Format;
         drawArgs.device.TextureState[0].ColorOperation = TextureOperation.Disable;
         drawArgs.device.DrawUserPrimitives(PrimitiveType.LineStrip, 4, downloadRectangle);
         drawArgs.device.TextureState[0].ColorOperation = TextureOperation.SelectArg1;
         drawArgs.device.VertexFormat = CustomVertex.PositionColoredTextured.Format;
         drawArgs.device.RenderState.ZBufferEnable = true;
      }

      #region IDisposable Implementation
      protected virtual void Dispose(bool disposing)
      {
         if (!disposed)
         {
#if DEBUG
            lock (lockalloc)
            {
               System.Diagnostics.Debug.WriteLine("Disposing QuadTileSet " + lockcount.ToString());
               lockcount--;
            }
#endif

            isInitialized = false;

            lock (m_topmostTiles.SyncRoot)
            {
               foreach (QuadTile qt in m_topmostTiles.Values)
                  qt.Dispose();
               m_topmostTiles.Clear();
            }

            if (QuadTileArgs != null)
            {
               QuadTileArgs.Dispose();
               QuadTileArgs = null;
            }

            disposed = true;

            // Suppress finalization of this disposed instance.
            if (disposing)
            {
               GC.SuppressFinalize(this);
            }
         }
      }

      public sealed override void Dispose()
      {
         Dispose(true);
      }

      ~QuadTileSet()
      {
         Dispose(false);
      }
      #endregion
   }

}
