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
               middleRow = MathEngine.GetRowFromLatitude(South + (North - South)/2.0, (double)QuadTileArgs.ImageAccessor.LevelZeroTileSizeDegrees);
               middleCol = MathEngine.GetColFromLongitude(West + (East - West)/2.0, (double)QuadTileArgs.ImageAccessor.LevelZeroTileSizeDegrees);
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



   public class QuadTile : IDisposable
   {
#if DEBUG
      static object lockalloc = new object();
      static int lockcount = 0;
#endif

      /// <summary>
      /// Child tile location
      /// </summary>
      public enum ChildLocation
      {
         NorthWest,
         SouthWest,
         NorthEast,
         SouthEast
      }

      private bool disposed = false;
      public QuadTileArgs QuadTileArgs;
      public double West;
      public double East;
      public double North;
      public double South;
      public Angle CenterLatitude;
      public Angle CenterLongitude;
      public double LatitudeSpan;
      public double LongitudeSpan;

      public bool isInitialized;
      public BoundingBox BoundingBox;
      public GeoSpatialDownloadRequest DownloadRequest;

      protected Texture texture;

      /// <summary>
      /// Number of points in child flat mesh grid (times 2)
      /// </summary>
      protected static int vertexCount = 40;

      /// <summary>
      /// Number of points in child terrain mesh grid (times 2)
      /// </summary>
      protected static int vertexCountElevated = 32;
      protected int level;

      protected QuadTile northWestChild;
      protected QuadTile southWestChild;
      protected QuadTile northEastChild;
      protected QuadTile southEastChild;

      protected QuadTileSubset northWestChildSubset;
      protected QuadTileSubset southWestChildSubset;
      protected QuadTileSubset northEastChildSubset;
      protected QuadTileSubset southEastChildSubset;

      protected CustomVertex.PositionColoredTextured[] northWestVertices;
      protected CustomVertex.PositionColoredTextured[] southWestVertices;
      protected CustomVertex.PositionColoredTextured[] northEastVertices;
      protected CustomVertex.PositionColoredTextured[] southEastVertices;
      protected short[] vertexIndices;

      /// <summary>
      /// The vertical exaggeration the tile mesh was computed for
      /// </summary>
      float verticalExaggeration;

      CustomVertex.PositionColored[] downloadRectangle = new CustomVertex.PositionColored[5];


      public QuadTile NorthWestChild
      {
         get
         {
            return northWestChild;
         }
      }

      public QuadTile NorthEastChild
      {
         get
         {
            return northEastChild;
         }
      }

      public QuadTile SouthWestChild
      {
         get
         {
            return southWestChild;
         }
      }

      public QuadTile SouthEastChild
      {
         get
         {
            return southEastChild;
         }
      }

      public byte Opacity
      {
         get
         {
            return m_CurrentOpacity;
         }
         set
         {
            m_CurrentOpacity = value;
            int opacity = value << 24;
            if (northEastVertices != null)
            {
               for (int i = 0; i < northEastVertices.Length; i++)
               {
                  northEastVertices[i].Color = opacity;
               }
            }
            if (northWestVertices != null)
            {
               for (int i = 0; i < northWestVertices.Length; i++)
               {
                  northWestVertices[i].Color = opacity;
               }
            }
            if (southEastVertices != null)
            {
               for (int i = 0; i < southEastVertices.Length; i++)
               {
                  southEastVertices[i].Color = opacity;
               }
            }
            if (southWestVertices != null)
            {
               for (int i = 0; i < southWestVertices.Length; i++)
               {
                  southWestVertices[i].Color = opacity;
               }
            }
            if (northEastChild != null)
            {
               northEastChild.Opacity = value;
            }
            if (northWestChild != null)
            {
               northWestChild.Opacity = value;
            }
            if (southWestChild != null)
            {
               southWestChild.Opacity = value;
            }
            if (southEastChild != null)
            {
               southEastChild.Opacity = value;
            }
         }
      }
      /// <summary>
      /// Initializes a new instance of the <see cref= "T:WorldWind.Renderable.QuadTile"/> class.
      /// </summary>
      /// <param name="south"></param>
      /// <param name="north"></param>
      /// <param name="west"></param>
      /// <param name="east"></param>
      /// <param name="level"></param>
      /// <param name="quadTileArgs"></param>
      public QuadTile(double south, double north, double west, double east, int level, QuadTileArgs quadTileArgs)
      {
#if DEBUG
         lock (lockalloc)
         {
            lockcount++;
            System.Diagnostics.Debug.WriteLine("Allocating QuadTile " + lockcount.ToString());
         }
#endif

         this.South = south;
         this.North = north;
         this.West = west;
         this.East = east;
         CenterLatitude = Angle.FromDegrees(0.5f * (North + South));
         CenterLongitude = Angle.FromDegrees(0.5f * (West + East));
         LatitudeSpan = Math.Abs(North - South);
         LongitudeSpan = Math.Abs(East - West);

         m_TileOffset = MathEngine.SphericalToCartesian(
            Math.Round(CenterLatitude.Degrees, 4), Math.Round(CenterLongitude.Degrees, 4), quadTileArgs.LayerRadius);

         this.level = level;
         QuadTileArgs = quadTileArgs;

         BoundingBox = new BoundingBox((float)south, (float)north, (float)west, (float)east,
            (float)QuadTileArgs.LayerRadius, (float)QuadTileArgs.LayerRadius + 10000 * World.Settings.VerticalExaggeration);

         int row = MathEngine.GetRowFromLatitude(South, North - South);
         int col = MathEngine.GetColFromLongitude(West, North - South);
      }

      /// <summary>
      /// Returns the QuadTile for specified location if available.
      /// Tries to queue a download if not available.
      /// </summary>
      /// <returns>Initialized QuadTile if available locally, else null.</returns>
      private QuadTile ComputeChild(DrawArgs drawArgs, double childSouth, double childNorth, double childWest, double childEast, double tileSize)
      {
         int row = MathEngine.GetRowFromLatitude(childSouth, tileSize);
         int col = MathEngine.GetColFromLongitude(childWest, tileSize);

         if (QuadTileArgs.ImageAccessor.LevelCount <= level + 1)
            return null;

         QuadTile child = new QuadTile(
            childSouth,
            childNorth,
            childWest,
            childEast,
            this.level + 1,
            QuadTileArgs);

         return child;
      }

      private QuadTileSubset ComputeChildSubset(DrawArgs drawArgs, double childSouth, double childNorth, double childWest, double childEast, double tileSize)
      {
         int row = MathEngine.GetRowFromLatitude(childSouth, tileSize);
         int col = MathEngine.GetColFromLongitude(childWest, tileSize);

         QuadTileSubset child = new QuadTileSubset(
            childSouth,
            childNorth,
            childWest,
            childEast,
            this.level + 1,
            QuadTileArgs,
            texture,
            North,
            South,
            West,
            East);

         return child;
      }

      public virtual void ComputeChildren(DrawArgs drawArgs)
      {
         double tileSize = 0.5 * (North - South);

         double CenterLat = 0.5f * (South + North);
         double CenterLon = 0.5f * (East + West);

         if (level + 1 >= QuadTileArgs.ImageAccessor.LevelCount)
         {
            if (northWestChildSubset == null)
            {
               northWestChildSubset = ComputeChildSubset(drawArgs, CenterLat, North, West, CenterLon, tileSize);
            }
            if (northEastChildSubset == null)
            {
               northEastChildSubset = ComputeChildSubset(drawArgs, CenterLat, North, CenterLon, East, tileSize);
            }
            if (southWestChildSubset == null)
            {
               southWestChildSubset = ComputeChildSubset(drawArgs, South, CenterLat, West, CenterLon, tileSize);
            }
            if (southEastChildSubset == null)
            {
               southEastChildSubset = ComputeChildSubset(drawArgs, South, CenterLat, CenterLon, East, tileSize);
            }
            return;
         }

         if (northWestChild == null)
         {
            northWestChild = ComputeChild(drawArgs, CenterLat, North, West, CenterLon, tileSize);
         }

         if (northEastChild == null)
         {
            northEastChild = ComputeChild(drawArgs, CenterLat, North, CenterLon, East, tileSize);
         }

         if (southWestChild == null)
         {
            southWestChild = ComputeChild(drawArgs, South, CenterLat, West, CenterLon, tileSize);
         }

         if (southEastChild == null)
         {
            southEastChild = ComputeChild(drawArgs, South, CenterLat, CenterLon, East, tileSize);
         }
      }

      #region IDisposable Implementation
      protected virtual void Dispose(bool disposing)
      {
         if (!disposed)
         {
#if DEBUG
            lock (lockalloc)
            {
               System.Diagnostics.Debug.WriteLine("Disposing QuadTile " + lockcount.ToString());
               lockcount--;
            }
#endif
            try
            {
               this.isInitialized = false;
               if (this.texture != null)
               {
                  this.texture.Dispose();
                  texture = null;
               }
            }
            catch
            {
            }
            DisposeChildren();
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

      ~QuadTile()
      {
         Dispose(false);
      }
      #endregion

      public virtual void Initialize(DrawArgs drawArgs)
      {
         try
         {
            GeographicBoundingBox geoBox = new GeographicBoundingBox(North, South, West, East);
            string strImagePath = QuadTileArgs.ImageAccessor.GetImagePath(geoBox, level);

            if (DownloadRequest != null && (DownloadRequest.IsDownloading || !DownloadRequest.IsComplete))
            {
               // Sort of a hack
               if (!QuadTileArgs.ImageAccessor.DownloadQueue.Contains(DownloadRequest))
               {
                  GeographicBoundingBox geoBox1 = new GeographicBoundingBox(North, South, West, East);
                  DownloadRequest = QuadTileArgs.ImageAccessor.RequestTexture(drawArgs, geoBox1, this.level);
               }
               // Waiting for download
               return;
            }

            if (texture != null)
            {
               texture.Dispose();
               texture = null;
            }

            if (DownloadRequest != null && DownloadRequest.IsComplete)
            {
               texture = QuadTileArgs.ImageAccessor.GetTexture(drawArgs, geoBox, level);
               //if (texture == null)
               //{
               //    DownloadRequest = QuadTileArgs.ImageAccessor.RequestTexture(drawArgs, geoBox, level);
               //}
            }
            else if (DownloadRequest == null)
            {
               DownloadRequest = QuadTileArgs.ImageAccessor.RequestTexture(drawArgs, geoBox, level);
               if (DownloadRequest.IsComplete)
               {
                  texture = QuadTileArgs.ImageAccessor.GetTexture(drawArgs, geoBox, level);
               }
               else
               {
                  return;
               }
            }

            if (texture == null)
            {
               return;
            }

            m_CurrentOpacity = QuadTileArgs.Opacity;
            CreateTileMesh();
            drawArgs.Repaint = true;
         }
         catch (Microsoft.DirectX.Direct3D.Direct3DXException)
         {
            // Texture load failed.
         }
         isInitialized = true;
      }

      public virtual void Update(DrawArgs drawArgs)
      {
         try
         {
            double tileSize = North - South;

            if (!isInitialized)
            {
               if ((drawArgs.WorldCamera.ViewRange * 0.5f < Angle.FromDegrees(QuadTileArgs.TileDrawDistance * tileSize)
                  && MathEngine.SphericalDistance(CenterLatitude, CenterLongitude,
                  drawArgs.WorldCamera.Latitude, drawArgs.WorldCamera.Longitude) < Angle.FromDegrees(QuadTileArgs.TileDrawSpread * tileSize * 1.25f)
                  && drawArgs.WorldCamera.ViewFrustum.Intersects(BoundingBox)) || (this.level == 0 && QuadTileArgs.AlwaysRenderBaseTiles))
                  this.Initialize(drawArgs);
            }
            else
            {
               if (!(drawArgs.WorldCamera.ViewRange * 0.5f < Angle.FromDegrees(QuadTileArgs.TileDrawDistance * tileSize)
                  && MathEngine.SphericalDistance(CenterLatitude, CenterLongitude,
                  drawArgs.WorldCamera.Latitude, drawArgs.WorldCamera.Longitude) < Angle.FromDegrees(QuadTileArgs.TileDrawSpread * tileSize * 1.25f)
                  && drawArgs.WorldCamera.ViewFrustum.Intersects(BoundingBox)
                  ) && (this.level != 0 || (this.level == 0 && !QuadTileArgs.AlwaysRenderBaseTiles)))
               {
                  this.Dispose();
                  return;
               }
            }
            
            // if the vertical exaggeration or opacity have changed, recreate the tile mesh
            if (isInitialized &&
               (World.Settings.VerticalExaggeration != verticalExaggeration || m_CurrentOpacity != QuadTileArgs.Opacity))
            {
               m_CurrentOpacity = QuadTileArgs.Opacity;
               CreateTileMesh();
               drawArgs.Repaint = true;
            }

            if (isInitialized)
            {
               if (drawArgs.WorldCamera.ViewRange < Angle.FromDegrees(QuadTileArgs.TileDrawDistance * tileSize)
                  && MathEngine.SphericalDistance(CenterLatitude, CenterLongitude,
                  drawArgs.WorldCamera.Latitude, drawArgs.WorldCamera.Longitude) < Angle.FromDegrees(QuadTileArgs.TileDrawSpread * tileSize)
                  && drawArgs.WorldCamera.ViewFrustum.Intersects(BoundingBox)
                  )
               {
                  if ((northEastChild == null && northEastChildSubset == null) ||
                     (northWestChild == null && northWestChildSubset == null) ||
                     (southEastChild == null && southEastChildSubset == null) ||
                     (southWestChild == null && southWestChildSubset == null)
                     )
                  {
                     ComputeChildren(drawArgs);
                  }

                  UpdateChildren(drawArgs);
               }
               else
               {
                  DisposeChildren();
               }
            }
         }
         catch (Exception ex)
         {
            Utility.Log.Write(ex);
         }
      }


      public void InitExportInfo(DrawArgs drawArgs, RenderableObject.ExportInfo info)
      {
         if (isInitialized)
         {
            info.dMaxLat = Math.Max(info.dMaxLat, this.North);
            info.dMinLat = Math.Min(info.dMinLat, this.South);
            info.dMaxLon = Math.Max(info.dMaxLon, this.East);
            info.dMinLon = Math.Min(info.dMinLon, this.West);

            info.iPixelsY = Math.Max(info.iPixelsY, (int)Math.Round((info.dMaxLat - info.dMinLat) / (this.North - this.South)) * QuadTileArgs.ImageAccessor.TextureSizePixels);
            info.iPixelsX = Math.Max(info.iPixelsX, (int)Math.Round((info.dMaxLon - info.dMinLon) / (this.East - this.West)) * QuadTileArgs.ImageAccessor.TextureSizePixels);
         }

         if (northWestChild != null && northWestChild.isInitialized)
            northWestChild.InitExportInfo(drawArgs, info);
         if (northEastChild != null && northEastChild.isInitialized)
            northEastChild.InitExportInfo(drawArgs, info);
         if (southWestChild != null && southWestChild.isInitialized)
            southWestChild.InitExportInfo(drawArgs, info);
         if (southEastChild != null && southEastChild.isInitialized)
            southEastChild.InitExportInfo(drawArgs, info);
      }

      public void ExportProcess(DrawArgs drawArgs, RenderableObject.ExportInfo expInfo)
      {
         try
         {
            if (texture != null)
            {
               Image img = null;

               try
               {
                  int iWidth, iHeight, iX, iY;

                  GeographicBoundingBox geoBox = new GeographicBoundingBox(this.North, this.South, this.West, this.East);
                  img = Image.FromFile(QuadTileArgs.ImageAccessor.GetImagePath(geoBox, level));

                  iWidth = (int)Math.Round((this.East - this.West) * (double)expInfo.iPixelsX / (expInfo.dMaxLon - expInfo.dMinLon));
                  iHeight = (int)Math.Round((this.North - this.South) * (double)expInfo.iPixelsY / (expInfo.dMaxLat - expInfo.dMinLat));
                  iX = (int)Math.Round((this.West - expInfo.dMinLon) * (double)expInfo.iPixelsX / (expInfo.dMaxLon - expInfo.dMinLon));
                  iY = (int)Math.Round((expInfo.dMaxLat - this.North) * (double)expInfo.iPixelsY / (expInfo.dMaxLat - expInfo.dMinLat));
                  expInfo.gr.DrawImage(img, new Rectangle(iX, iY, iWidth, iHeight));
               }
               catch
               {
#if DEBUG
                  System.Diagnostics.Debug.WriteLine("Thrown in image export");
#endif
               }
               finally
               {
                  if (img != null)
                     img.Dispose();
               }
            }

            if (northWestChild != null && northWestChild.isInitialized)
               northWestChild.ExportProcess(drawArgs, expInfo);

            if (northEastChild != null && northEastChild.isInitialized)
               northEastChild.ExportProcess(drawArgs, expInfo);

            if (southWestChild != null && southWestChild.isInitialized)
               southWestChild.ExportProcess(drawArgs, expInfo);

            if (southEastChild != null && southEastChild.isInitialized)
               southEastChild.ExportProcess(drawArgs, expInfo);
         }
         catch
         {
         }
      }

      private void UpdateChildren(DrawArgs drawArgs)
      {
         if (northEastChild != null)
         {
            northEastChild.Update(drawArgs);
         }

         if (northWestChild != null)
         {
            northWestChild.Update(drawArgs);
         }

         if (southEastChild != null)
         {
            southEastChild.Update(drawArgs);
         }

         if (southWestChild != null)
         {
            southWestChild.Update(drawArgs);
         }

         if (northEastChildSubset != null)
         {
            northEastChildSubset.Update(drawArgs);
         }
         if (northWestChildSubset != null)
         {
            northWestChildSubset.Update(drawArgs);
         }
         if (southEastChildSubset != null)
         {
            southEastChildSubset.Update(drawArgs);
         }
         if (southWestChildSubset != null)
         {
            southWestChildSubset.Update(drawArgs);
         }
      }
      public void DisposeChildren()
      {
         if (northWestChild != null)
         {
            northWestChild.Dispose();
            northWestChild = null;
         }

         if (northEastChild != null)
         {
            northEastChild.Dispose();
            northEastChild = null;
         }

         if (southEastChild != null)
         {
            southEastChild.Dispose();
            southEastChild = null;
         }

         if (southWestChild != null)
         {
            southWestChild.Dispose();
            southWestChild = null;
         }
         if (northWestChildSubset != null)
         {
            northWestChildSubset.Dispose();
            northWestChildSubset = null;
         }
         if (northEastChildSubset != null)
         {
            northEastChildSubset.Dispose();
            northEastChildSubset = null;
         }
         if (southWestChildSubset != null)
         {
            southWestChildSubset.Dispose();
            southWestChildSubset = null;
         }
         if (southEastChildSubset != null)
         {
            southEastChildSubset.Dispose();
            southEastChildSubset = null;
         }
      }

      /// <summary>
      /// Builds flat or terrain mesh for current tile
      /// </summary>
      public virtual void CreateTileMesh()
      {
         verticalExaggeration = World.Settings.VerticalExaggeration;
         CreateElevatedMesh();
      }

      // Create indice list (PM)
      private short[] CreateTriangleIndicesBTT(CustomVertex.PositionColoredTextured[] ElevatedVertices, int VertexDensity, int Margin, double LayerRadius)
      {
         BinaryTriangleTree Tree = new BinaryTriangleTree(ElevatedVertices, VertexDensity, Margin, LayerRadius);
         Tree.BuildTree(7); // variance 0 = best fit
         Tree.BuildIndices();
         return Tree.Indices.ToArray();
      }

      private short[] CreateTriangleIndicesRegular(CustomVertex.PositionColoredTextured[] ElevatedVertices, int VertexDensity, int Margin, double LayerRadius)
      {
         short[] indices;
         int thisVertexDensityElevatedPlus2 = (VertexDensity + (Margin * 2));
         indices = new short[2 * thisVertexDensityElevatedPlus2 * thisVertexDensityElevatedPlus2 * 3];

         for (int i = 0; i < thisVertexDensityElevatedPlus2; i++)
         {
            int elevated_idx = (2 * 3 * i * thisVertexDensityElevatedPlus2);
            for (int j = 0; j < thisVertexDensityElevatedPlus2; j++)
            {
               indices[elevated_idx] = (short)(i * (thisVertexDensityElevatedPlus2 + 1) + j);
               indices[elevated_idx + 1] = (short)((i + 1) * (thisVertexDensityElevatedPlus2 + 1) + j);
               indices[elevated_idx + 2] = (short)(i * (thisVertexDensityElevatedPlus2 + 1) + j + 1);

               indices[elevated_idx + 3] = (short)(i * (thisVertexDensityElevatedPlus2 + 1) + j + 1);
               indices[elevated_idx + 4] = (short)((i + 1) * (thisVertexDensityElevatedPlus2 + 1) + j);
               indices[elevated_idx + 5] = (short)((i + 1) * (thisVertexDensityElevatedPlus2 + 1) + j + 1);

               elevated_idx += 6;
            }
         }
         return indices;
      }

      byte m_CurrentOpacity = 255;

      /// <summary>
      /// Build the elevated terrain mesh
      /// </summary>
      protected virtual void CreateElevatedMesh()
      {
         TerrainTile tile = null;
         if (QuadTileArgs.TerrainAccessor != null)
            tile = QuadTileArgs.TerrainAccessor.GetElevationArray((float)North, (float)South, (float)West, (float)East, vertexCountElevated + 1);

         int vertexCountElevatedPlus3 = vertexCountElevated / 2 + 3;
         int totalVertexCount = vertexCountElevatedPlus3 * vertexCountElevatedPlus3;
         northWestVertices = new CustomVertex.PositionColoredTextured[totalVertexCount];
         southWestVertices = new CustomVertex.PositionColoredTextured[totalVertexCount];
         northEastVertices = new CustomVertex.PositionColoredTextured[totalVertexCount];
         southEastVertices = new CustomVertex.PositionColoredTextured[totalVertexCount];
         float layerRadius = (float)QuadTileArgs.LayerRadius;
         double scaleFactor = 1f / vertexCountElevated;

         float meshBaseRadius = (float)QuadTileArgs.LayerRadius;

         if (tile != null)
         {
            // Calculate mesh base radius (bottom vertices)
            float minimumElevation = float.MaxValue;
            float maximumElevation = float.MinValue;

            // Find minimum elevation to account for possible bathymetry
            foreach (float height in tile.ElevationData)
            {
               if (height < minimumElevation)
                  minimumElevation = height;
               if (height > maximumElevation)
                  maximumElevation = height;
            }
            minimumElevation *= verticalExaggeration;
            maximumElevation *= verticalExaggeration;

            if (minimumElevation > maximumElevation)
            {
               // Compensate for negative vertical exaggeration
               float tmp = minimumElevation;
               minimumElevation = maximumElevation;
               maximumElevation = minimumElevation;
            }

            float overlap = 500 * verticalExaggeration; // 500m high tiles

            // Radius of mesh bottom grid
            meshBaseRadius = layerRadius + minimumElevation - overlap;
         }

         if (tile != null)
         {
            CreateElevatedMesh(ChildLocation.NorthWest, ref northWestVertices, meshBaseRadius, ref tile.ElevationData, vertexCountElevated + 1);
            CreateElevatedMesh(ChildLocation.SouthWest, ref southWestVertices, meshBaseRadius, ref tile.ElevationData, vertexCountElevated + 1);
            CreateElevatedMesh(ChildLocation.NorthEast, ref northEastVertices, meshBaseRadius, ref tile.ElevationData, vertexCountElevated + 1);
            CreateElevatedMesh(ChildLocation.SouthEast, ref southEastVertices, meshBaseRadius, ref tile.ElevationData, vertexCountElevated + 1);
         }
         else
         {
            List<float> nullRef = null;
            CreateElevatedMesh(ChildLocation.NorthWest, ref northWestVertices, meshBaseRadius, ref nullRef, vertexCountElevated + 1);
            CreateElevatedMesh(ChildLocation.SouthWest, ref southWestVertices, meshBaseRadius, ref nullRef, vertexCountElevated + 1);
            CreateElevatedMesh(ChildLocation.NorthEast, ref northEastVertices, meshBaseRadius, ref nullRef, vertexCountElevated + 1);
            CreateElevatedMesh(ChildLocation.SouthEast, ref southEastVertices, meshBaseRadius, ref nullRef, vertexCountElevated + 1);
         }

         BoundingBox = new BoundingBox((float)South, (float)North, (float)West, (float)East,
            (float)layerRadius, (float)layerRadius + 10000 * this.verticalExaggeration);

         m_NwIndices = CreateTriangleIndicesBTT(northWestVertices, (int)vertexCountElevated / 2, 1, layerRadius);
         m_NeIndices = CreateTriangleIndicesBTT(northEastVertices, (int)vertexCountElevated / 2, 1, layerRadius);
         m_SwIndices = CreateTriangleIndicesBTT(southWestVertices, (int)vertexCountElevated / 2, 1, layerRadius);
         m_SeIndices = CreateTriangleIndicesBTT(southEastVertices, (int)vertexCountElevated / 2, 1, layerRadius);

         QuadTileArgs.IsDownloadingElevation = false;

         // Build common set of indices for the 4 child meshes
         int vertexCountElevatedPlus2 = vertexCountElevated / 2 + 2;
         vertexIndices = new short[2 * vertexCountElevatedPlus2 * vertexCountElevatedPlus2 * 3];

         int elevated_idx = 0;
         for (int i = 0; i < vertexCountElevatedPlus2; i++)
         {
            for (int j = 0; j < vertexCountElevatedPlus2; j++)
            {
               vertexIndices[elevated_idx++] = (short)(i * vertexCountElevatedPlus3 + j);
               vertexIndices[elevated_idx++] = (short)((i + 1) * vertexCountElevatedPlus3 + j);
               vertexIndices[elevated_idx++] = (short)(i * vertexCountElevatedPlus3 + j + 1);

               vertexIndices[elevated_idx++] = (short)(i * vertexCountElevatedPlus3 + j + 1);
               vertexIndices[elevated_idx++] = (short)((i + 1) * vertexCountElevatedPlus3 + j);
               vertexIndices[elevated_idx++] = (short)((i + 1) * vertexCountElevatedPlus3 + j + 1);
            }
         }
      }

      short[] m_NwIndices;
      short[] m_NeIndices;
      short[] m_SwIndices;
      short[] m_SeIndices;

      Vector3d m_TileOffset = Vector3d.Empty;

      /// <summary>
      /// Create child tile terrain mesh
      /// </summary>
      protected void CreateElevatedMesh(ChildLocation corner, ref CustomVertex.PositionColoredTextured[] vertices,
         float meshBaseRadius, ref List<float> heightData, int samples)
      {
         // Figure out child lat/lon boundaries (radians)
         double north = MathEngine.DegreesToRadians(North);
         double west = MathEngine.DegreesToRadians(West);

         // Texture coordinate offsets
         float TuOffset = 0;
         float TvOffset = 0;

         switch (corner)
         {
            case ChildLocation.NorthWest:
               // defaults are all good
               break;
            case ChildLocation.NorthEast:
               west = MathEngine.DegreesToRadians(0.5 * (West + East));
               TuOffset = 0.5f;
               break;
            case ChildLocation.SouthWest:
               north = MathEngine.DegreesToRadians(0.5 * (North + South));
               TvOffset = 0.5f;
               break;
            case ChildLocation.SouthEast:
               north = MathEngine.DegreesToRadians(0.5 * (North + South));
               west = MathEngine.DegreesToRadians(0.5 * (West + East));
               TuOffset = 0.5f;
               TvOffset = 0.5f;
               break;
         }

         double latitudeRadianSpan = MathEngine.DegreesToRadians(LatitudeSpan);
         double longitudeRadianSpan = MathEngine.DegreesToRadians(LongitudeSpan);

         float layerRadius = (float)QuadTileArgs.LayerRadius;
         double scaleFactor = 1f / vertexCountElevated;
         int terrainLongitudeIndex = (int)(TuOffset * vertexCountElevated);
         int terrainLatitudeIndex = (int)(TvOffset * vertexCountElevated);

         int vertexCountElevatedPlus1 = vertexCountElevated / 2 + 1;

         float radius = 0;
         int vertexIndex = 0;
         for (int latitudeIndex = -1; latitudeIndex <= vertexCountElevatedPlus1; latitudeIndex++)
         {
            int latitudePoint = latitudeIndex;
            if (latitudePoint < 0)
               latitudePoint = 0;
            else if (latitudePoint >= vertexCountElevatedPlus1)
               latitudePoint = vertexCountElevatedPlus1 - 1;

            double latitudeFactor = latitudePoint * scaleFactor;
            double latitude = north - latitudeFactor * latitudeRadianSpan;

            // Cache trigonometric values
            double cosLat = Math.Cos(latitude);
            double sinLat = Math.Sin(latitude);

            for (int longitudeIndex = -1; longitudeIndex <= vertexCountElevatedPlus1; longitudeIndex++)
            {
               int longitudePoint = longitudeIndex;
               if (longitudePoint < 0)
                  longitudePoint = 0;
               else if (longitudePoint >= vertexCountElevatedPlus1)
                  longitudePoint = vertexCountElevatedPlus1 - 1;

               if (longitudeIndex != longitudePoint || latitudeIndex != latitudePoint)
               {

                  if (heightData != null && heightData.Count > 0)
                  {
                     radius = layerRadius +
                        heightData[terrainLatitudeIndex + latitudePoint + (terrainLongitudeIndex + longitudePoint) * samples]
                        * verticalExaggeration;
                  }
                  else
                  {
                     radius = meshBaseRadius;
                  }
               }
               else
               {
                  if (heightData != null && heightData.Count > 0)
                  {
                     // Top of mesh (real terrain)
                     radius = layerRadius +
                        heightData[terrainLatitudeIndex + latitudeIndex + (terrainLongitudeIndex + longitudeIndex) * samples]
                        * verticalExaggeration;
                  }
                  else
                  {
                     radius = meshBaseRadius;
                  }
               }

               double longitudeFactor = longitudePoint * scaleFactor;

               // Texture coordinates
               vertices[vertexIndex].Tu = TuOffset + (float)longitudeFactor;
               vertices[vertexIndex].Tv = TvOffset + (float)latitudeFactor;

               // Convert from spherical (radians) to cartesian
               double longitude = west + longitudeFactor * longitudeRadianSpan;
               double radCosLat = radius * cosLat;

               vertices[vertexIndex].X = (float)(radCosLat * Math.Cos(longitude) - m_TileOffset.X);
               vertices[vertexIndex].Y = (float)(radCosLat * Math.Sin(longitude) - m_TileOffset.Y);
               vertices[vertexIndex].Z = (float)(radius * sinLat - m_TileOffset.Z);

               vertices[vertexIndex].Color = m_CurrentOpacity << 24;

               vertexIndex++;
            }
         }
      }

      /// <summary>
      /// Render a rectangle around an image tile in the specified color
      /// </summary>
      public void RenderDownloadRectangle(DrawArgs drawArgs, int color)
      {
         // Render terrain download rectangle
         Vector3d northWestV = MathEngine.SphericalToCartesian((float)North, (float)West, QuadTileArgs.LayerRadius);
         Vector3d southWestV = MathEngine.SphericalToCartesian((float)South, (float)West, QuadTileArgs.LayerRadius);
         Vector3d northEastV = MathEngine.SphericalToCartesian((float)North, (float)East, QuadTileArgs.LayerRadius);
         Vector3d southEastV = MathEngine.SphericalToCartesian((float)South, (float)East, QuadTileArgs.LayerRadius);

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

      public virtual void Render(DrawArgs drawArgs)
      {
         try
         {
            if (!isInitialized || texture == null)
               return;
            if (!drawArgs.WorldCamera.ViewFrustum.Intersects(BoundingBox))
               return;

            if (isInitialized && DownloadRequest != null)
            {
               DownloadRequest = null;
            }


            Matrix curWorld = drawArgs.device.Transform.World;

            drawArgs.device.VertexFormat = CustomVertex.PositionColoredTextured.Format;
            drawArgs.device.TextureState[0].AlphaOperation = TextureOperation.Modulate;
            drawArgs.device.TextureState[0].ColorOperation = TextureOperation.Add;
            drawArgs.device.TextureState[0].AlphaArgument1 = TextureArgument.TextureColor;

            // NORTH WEST Quarter rendering
            if (northWestChild != null && northWestChild.isInitialized)
            {
               northWestChild.Render(drawArgs);
            }
            else if (northWestChildSubset != null && northWestChildSubset.isInitialized)
            {
               northWestChildSubset.Render(drawArgs);
            }
            else if (texture != null && !texture.Disposed)
            {
               drawArgs.device.Transform.World *= Matrix.Translation((float)m_TileOffset.X, (float)m_TileOffset.Y, (float)m_TileOffset.Z);
               drawArgs.device.SetTexture(0, texture);
               drawArgs.device.DrawIndexedUserPrimitives(
                  PrimitiveType.TriangleList, 0, northWestVertices.Length,
                  (m_NwIndices != null ? m_NwIndices.Length / 3 : vertexIndices.Length / 3),
                  (m_NwIndices != null ? m_NwIndices : vertexIndices),
                  true, northWestVertices);
               drawArgs.device.Transform.World = curWorld;
               drawArgs.numberTilesDrawn++;
            }

            if (northEastChild != null && northEastChild.isInitialized)
            {
               northEastChild.Render(drawArgs);
            }
            else if (northEastChildSubset != null && northEastChildSubset.isInitialized)
            {
               northEastChildSubset.Render(drawArgs);
            }
            else if (texture != null && !texture.Disposed)
            {
               drawArgs.device.Transform.World *= Matrix.Translation((float)m_TileOffset.X, (float)m_TileOffset.Y, (float)m_TileOffset.Z);

               drawArgs.device.SetTexture(0, texture);
               drawArgs.device.DrawIndexedUserPrimitives(
                  PrimitiveType.TriangleList, 0, northEastVertices.Length,
                  (m_NeIndices != null ? m_NeIndices.Length / 3 : vertexIndices.Length / 3),
                  (m_NeIndices != null ? m_NeIndices : vertexIndices),
                  true, northEastVertices);
               drawArgs.device.Transform.World = curWorld;
               drawArgs.numberTilesDrawn++;
            }

            if (southWestChild != null && southWestChild.isInitialized)
            {
               southWestChild.Render(drawArgs);
            }
            else if (southWestChildSubset != null && southWestChildSubset.isInitialized)
            {
               southWestChildSubset.Render(drawArgs);
            }
            else if (texture != null && !texture.Disposed)
            {
               drawArgs.device.Transform.World *= Matrix.Translation((float)m_TileOffset.X, (float)m_TileOffset.Y, (float)m_TileOffset.Z);

               drawArgs.device.SetTexture(0, texture);
               drawArgs.device.DrawIndexedUserPrimitives(
                  PrimitiveType.TriangleList, 0, southWestVertices.Length,
                  (m_SwIndices != null ? m_SwIndices.Length / 3 : vertexIndices.Length / 3),
                  (m_SwIndices != null ? m_SwIndices : vertexIndices),
                  true, southWestVertices);
               drawArgs.device.Transform.World = curWorld;
               drawArgs.numberTilesDrawn++;
            }

            if (southEastChild != null && southEastChild.isInitialized)
            {
               southEastChild.Render(drawArgs);
            }
            else if (southEastChildSubset != null && southEastChildSubset.isInitialized)
            {
               southEastChildSubset.Render(drawArgs);
            }
            else if (texture != null && !texture.Disposed)
            {
               drawArgs.device.Transform.World *= Matrix.Translation((float)m_TileOffset.X, (float)m_TileOffset.Y, (float)m_TileOffset.Z);

               drawArgs.device.SetTexture(0, texture);
               drawArgs.device.DrawIndexedUserPrimitives(
                  PrimitiveType.TriangleList, 0, southEastVertices.Length,
                  (m_SeIndices != null ? m_SeIndices.Length / 3 : vertexIndices.Length),
                  (m_SeIndices != null ? m_SeIndices : vertexIndices),
                  true, southEastVertices);
               drawArgs.device.Transform.World = curWorld;
               drawArgs.numberTilesDrawn++;
            }
         }
         catch
         {
         }
      }
   }

   public class QuadTileSubset : IDisposable
   {
#if DEBUG
      static object lockalloc = new object();
      static int lockcount = 0;
#endif

      /// <summary>
      /// Child tile location
      /// </summary>
      public enum ChildLocation
      {
         NorthWest,
         SouthWest,
         NorthEast,
         SouthEast
      }

      public QuadTileArgs QuadTileArgs;
      public double West;
      public double East;
      public double North;
      public double South;
      public Angle CenterLatitude;
      public Angle CenterLongitude;
      public double LatitudeSpan;
      public double LongitudeSpan;

      public bool isInitialized;
      public BoundingBox BoundingBox;
      public GeoSpatialDownloadRequest DownloadRequest;
      byte m_CurrentOpacity = 255;

      protected Texture texture;

      /// <summary>
      /// Number of points in child flat mesh grid (times 2)
      /// </summary>
      protected static int vertexCount = 40;

      /// <summary>
      /// Number of points in child terrain mesh grid (times 2)
      /// </summary>
      protected static int vertexCountElevated = 32;
      protected int level;

      protected QuadTileSubset northWestChild;
      protected QuadTileSubset southWestChild;
      protected QuadTileSubset northEastChild;
      protected QuadTileSubset southEastChild;

      protected CustomVertex.PositionColoredTextured[] northWestVertices;
      protected CustomVertex.PositionColoredTextured[] southWestVertices;
      protected CustomVertex.PositionColoredTextured[] northEastVertices;
      protected CustomVertex.PositionColoredTextured[] southEastVertices;
      protected short[] vertexIndices;

      double m_ParentTextureNorth;
      double m_ParentTextureSouth;
      double m_ParentTextureWest;
      double m_ParentTextureEast;

      /// <summary>
      /// The vertical exaggeration the tile mesh was computed for
      /// </summary>
      float verticalExaggeration;

      Vector3d m_TileOffset;
      CustomVertex.PositionColored[] downloadRectangle = new CustomVertex.PositionColored[5];

      private bool disposed = false;

      /// <summary>
      /// Initializes a new instance of the <see cref= "T:WorldWind.Renderable.QuadTile"/> class.
      /// </summary>
      /// <param name="south"></param>
      /// <param name="north"></param>
      /// <param name="west"></param>
      /// <param name="east"></param>
      /// <param name="level"></param>
      /// <param name="quadTileArgs"></param>
      public QuadTileSubset(
         double south,
         double north,
         double west,
         double east,
         int level,
         QuadTileArgs quadTileArgs,
         Texture parentTexture,
         double parentTextureNorth,
         double parentTextureSouth,
         double parentTextureWest,
         double parentTextureEast)
      {
#if DEBUG
         lock (lockalloc)
         {
            lockcount++;
            System.Diagnostics.Debug.WriteLine("Allocating QuadTileSubset " + lockcount.ToString());
         }
#endif

         South = south;
         North = north;
         West = west;
         East = east;
         CenterLatitude = Angle.FromDegrees(0.5f * (North + South));
         CenterLongitude = Angle.FromDegrees(0.5f * (West + East));
         LatitudeSpan = Math.Abs(North - South);
         LongitudeSpan = Math.Abs(East - West);

         this.level = level;
         texture = parentTexture;
         m_ParentTextureNorth = parentTextureNorth;
         m_ParentTextureSouth = parentTextureSouth;
         m_ParentTextureWest = parentTextureWest;
         m_ParentTextureEast = parentTextureEast;
         m_TileOffset = MathEngine.SphericalToCartesian(
            //Math.Round(CenterLatitude.Degrees, 4), Math.Round(CenterLongitude.Degrees, 4), quadTileArgs.LayerRadius);
            CenterLatitude.Degrees, CenterLongitude.Degrees, quadTileArgs.LayerRadius);

         QuadTileArgs = quadTileArgs;

         BoundingBox = new BoundingBox((float)south, (float)north, (float)west, (float)east,
            (float)QuadTileArgs.LayerRadius, (float)QuadTileArgs.LayerRadius + 10000 * World.Settings.VerticalExaggeration);

         int row = MathEngine.GetRowFromLatitude(South, North - South);
         int col = MathEngine.GetColFromLongitude(West, North - South);
      }

      /// <summary>
      /// Returns the QuadTile for specified location if available.
      /// Tries to queue a download if not available.
      /// </summary>
      /// <returns>Initialized QuadTile if available locally, else null.</returns>
      private QuadTileSubset ComputeChild(DrawArgs drawArgs, double childSouth, double childNorth, double childWest, double childEast, double tileSize)
      {
         int row = MathEngine.GetRowFromLatitude(childSouth, tileSize);
         int col = MathEngine.GetColFromLongitude(childWest, tileSize);

         QuadTileSubset child = new QuadTileSubset(
            childSouth,
            childNorth,
            childWest,
            childEast,
            this.level + 1,
            QuadTileArgs,
            texture,
            m_ParentTextureNorth,
            m_ParentTextureSouth,
            m_ParentTextureWest,
            m_ParentTextureEast);

         return child;
      }

      public virtual void ComputeChildren(DrawArgs drawArgs)
      {
         float tileSize = (float)(0.5 * (North - South));

         double CenterLat = 0.5f * (South + North);
         double CenterLon = 0.5f * (East + West);
         if (northWestChild == null)
         {
            northWestChild = ComputeChild(drawArgs, CenterLat, North, West, CenterLon, tileSize);
         }

         if (northEastChild == null)
         {
            northEastChild = ComputeChild(drawArgs, CenterLat, North, CenterLon, East, tileSize);
         }

         if (southWestChild == null)
         {
            southWestChild = ComputeChild(drawArgs, South, CenterLat, West, CenterLon, tileSize);
         }

         if (southEastChild == null)
         {
            southEastChild = ComputeChild(drawArgs, South, CenterLat, CenterLon, East, tileSize);
         }
      }

      #region IDisposable Implementation
      protected virtual void Dispose(bool disposing)
      {
         if (!disposed)
         {
#if DEBUG
            lock (lockalloc)
            {
               System.Diagnostics.Debug.WriteLine("Disposing QuadTileSubSet " + lockcount.ToString());
               lockcount--;
            }
#endif
            this.isInitialized = false;
            DisposeChildren();
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

      ~QuadTileSubset()
      {
         Dispose(false);
      }
      #endregion

      private void DisposeChildren()
      {
         if (northWestChild != null)
         {
            northWestChild.Dispose();
            northWestChild = null;
         }
         if (southWestChild != null)
         {
            southWestChild.Dispose();
            southWestChild = null;
         }
         if (northEastChild != null)
         {
            northEastChild.Dispose();
            northEastChild = null;
         }
         if (southEastChild != null)
         {
            southEastChild.Dispose();
            southEastChild = null;
         }
      }

      public virtual void Initialize(DrawArgs drawArgs)
      {
         try
         {
            m_CurrentOpacity = QuadTileArgs.ParentQuadTileSet.Opacity;
            CreateTileMesh();
            drawArgs.Repaint = true;
         }
         catch (Microsoft.DirectX.Direct3D.Direct3DXException)
         {
            // Texture load failed.
         }
         isInitialized = true;
      }

      public virtual void Update(DrawArgs drawArgs)
      {
         try
         {
            double tileSize = North - South;

            if (!isInitialized)
            {
               if (drawArgs.WorldCamera.ViewRange * 0.5f < Angle.FromDegrees(QuadTileArgs.TileDrawDistance * tileSize)
                  && MathEngine.SphericalDistance(CenterLatitude, CenterLongitude,
                  drawArgs.WorldCamera.Latitude, drawArgs.WorldCamera.Longitude) < Angle.FromDegrees(QuadTileArgs.TileDrawSpread * tileSize * 1.25f)
                  && drawArgs.WorldCamera.ViewFrustum.Intersects(BoundingBox)
                  )
                  this.Initialize(drawArgs);
            }
            else
            {
               if (!(drawArgs.WorldCamera.ViewRange * 0.5f < Angle.FromDegrees(QuadTileArgs.TileDrawDistance * tileSize)
                  && MathEngine.SphericalDistance(CenterLatitude, CenterLongitude,
                  drawArgs.WorldCamera.Latitude, drawArgs.WorldCamera.Longitude) < Angle.FromDegrees(QuadTileArgs.TileDrawSpread * tileSize * 1.25f)
                  && drawArgs.WorldCamera.ViewFrustum.Intersects(BoundingBox)
                  ) && (this.level != 0 || (this.level == 0 && !QuadTileArgs.AlwaysRenderBaseTiles)))
               {
                  this.Dispose();
                  return;
               }
            }

            if (isInitialized &&
              (World.Settings.VerticalExaggeration != verticalExaggeration || m_CurrentOpacity != QuadTileArgs.ParentQuadTileSet.Opacity)
              )
            {
               m_CurrentOpacity = QuadTileArgs.ParentQuadTileSet.Opacity;
               CreateTileMesh();
               drawArgs.Repaint = true;
            }

            if (isInitialized)
            {
               if (drawArgs.WorldCamera.ViewRange < Angle.FromDegrees(QuadTileArgs.TileDrawDistance * tileSize)
                  && MathEngine.SphericalDistance(CenterLatitude, CenterLongitude,
                  drawArgs.WorldCamera.Latitude, drawArgs.WorldCamera.Longitude) < Angle.FromDegrees(QuadTileArgs.TileDrawSpread * tileSize)
                  && drawArgs.WorldCamera.ViewFrustum.Intersects(BoundingBox)
                  )
               {
                  if (northEastChild == null ||
                     northWestChild == null ||
                     southEastChild == null ||
                     southWestChild == null)
                  {
                     ComputeChildren(drawArgs);
                  }

                  if (northEastChild != null)
                  {
                     northEastChild.Update(drawArgs);
                  }

                  if (northWestChild != null)
                  {
                     northWestChild.Update(drawArgs);
                  }

                  if (southEastChild != null)
                  {
                     southEastChild.Update(drawArgs);
                  }

                  if (southWestChild != null)
                  {
                     southWestChild.Update(drawArgs);
                  }
               }
               else
                  DisposeChildren();
            }
         }
         catch (Exception ex)
         {
            Utility.Log.Write(ex);
         }
      }

      /// <summary>
      /// Builds flat or terrain mesh for current tile
      /// </summary>
      public virtual void CreateTileMesh()
      {
         verticalExaggeration = World.Settings.VerticalExaggeration;
         CreateElevatedMesh();
      }

      short[] m_NwIndices;
      short[] m_NeIndices;
      short[] m_SwIndices;
      short[] m_SeIndices;

      /// <summary>
      /// Build the elevated terrain mesh
      /// </summary>
      protected virtual void CreateElevatedMesh()
      {
         TerrainTile tile = null;
         
         if (QuadTileArgs.TerrainAccessor != null)
            tile = QuadTileArgs.TerrainAccessor.GetElevationArray((float)North, (float)South, (float)West, (float)East, vertexCountElevated + 1);
         
         int vertexCountElevatedPlus3 = vertexCountElevated / 2 + 3;
         int totalVertexCount = vertexCountElevatedPlus3 * vertexCountElevatedPlus3;
         northWestVertices = new CustomVertex.PositionColoredTextured[totalVertexCount];
         southWestVertices = new CustomVertex.PositionColoredTextured[totalVertexCount];
         northEastVertices = new CustomVertex.PositionColoredTextured[totalVertexCount];
         southEastVertices = new CustomVertex.PositionColoredTextured[totalVertexCount];
         float layerRadius = (float)QuadTileArgs.LayerRadius;
         double scaleFactor = 1f / vertexCountElevated;

         float meshBaseRadius = layerRadius;
         if (tile != null)
         {
            // Calculate mesh base radius (bottom vertices)
            // Find minimum elevation to account for possible bathymetry
            float minimumElevation = float.MaxValue;
            float maximumElevation = float.MinValue;
            foreach (float height in tile.ElevationData)
            {
               if (height < minimumElevation)
                  minimumElevation = height;
               if (height > maximumElevation)
                  maximumElevation = height;
            }
            minimumElevation *= verticalExaggeration;
            maximumElevation *= verticalExaggeration;

            if (minimumElevation > maximumElevation)
            {
               // Compensate for negative vertical exaggeration
               float tmp = minimumElevation;
               minimumElevation = maximumElevation;
               maximumElevation = minimumElevation;
            }

            float overlap = 500 * verticalExaggeration; // 500m high tiles

            // Radius of mesh bottom grid
            meshBaseRadius = layerRadius + minimumElevation - overlap;
         }

         if (tile != null)
         {
            CreateElevatedMesh(ChildLocation.NorthWest, ref northWestVertices, meshBaseRadius, ref tile.ElevationData, vertexCountElevated + 1);
            CreateElevatedMesh(ChildLocation.SouthWest, ref southWestVertices, meshBaseRadius, ref tile.ElevationData, vertexCountElevated + 1);
            CreateElevatedMesh(ChildLocation.NorthEast, ref northEastVertices, meshBaseRadius, ref tile.ElevationData, vertexCountElevated + 1);
            CreateElevatedMesh(ChildLocation.SouthEast, ref southEastVertices, meshBaseRadius, ref tile.ElevationData, vertexCountElevated + 1);
         }
         else
         {
            List<float> nullRef = null;

            CreateElevatedMesh(ChildLocation.NorthWest, ref northWestVertices, meshBaseRadius, ref nullRef, vertexCountElevated + 1);
            CreateElevatedMesh(ChildLocation.SouthWest, ref southWestVertices, meshBaseRadius, ref nullRef, vertexCountElevated + 1);
            CreateElevatedMesh(ChildLocation.NorthEast, ref northEastVertices, meshBaseRadius, ref nullRef, vertexCountElevated + 1);
            CreateElevatedMesh(ChildLocation.SouthEast, ref southEastVertices, meshBaseRadius, ref nullRef, vertexCountElevated + 1);
         }

         BoundingBox = new BoundingBox((float)South, (float)North, (float)West, (float)East,
            (float)layerRadius, (float)layerRadius + 10000 * this.verticalExaggeration);


         m_NwIndices = CreateTriangleIndicesBTT(northWestVertices, (int)vertexCountElevated / 2, 1, layerRadius);
         m_NeIndices = CreateTriangleIndicesBTT(northEastVertices, (int)vertexCountElevated / 2, 1, layerRadius);
         m_SwIndices = CreateTriangleIndicesBTT(southWestVertices, (int)vertexCountElevated / 2, 1, layerRadius);
         m_SeIndices = CreateTriangleIndicesBTT(southEastVertices, (int)vertexCountElevated / 2, 1, layerRadius);

         QuadTileArgs.IsDownloadingElevation = false;

         // Build common set of indices for the 4 child meshes
         int vertexCountElevatedPlus2 = vertexCountElevated / 2 + 2;
         vertexIndices = new short[2 * vertexCountElevatedPlus2 * vertexCountElevatedPlus2 * 3];

         int elevated_idx = 0;
         for (int i = 0; i < vertexCountElevatedPlus2; i++)
         {
            for (int j = 0; j < vertexCountElevatedPlus2; j++)
            {
               vertexIndices[elevated_idx++] = (short)(i * vertexCountElevatedPlus3 + j);
               vertexIndices[elevated_idx++] = (short)((i + 1) * vertexCountElevatedPlus3 + j);
               vertexIndices[elevated_idx++] = (short)(i * vertexCountElevatedPlus3 + j + 1);

               vertexIndices[elevated_idx++] = (short)(i * vertexCountElevatedPlus3 + j + 1);
               vertexIndices[elevated_idx++] = (short)((i + 1) * vertexCountElevatedPlus3 + j);
               vertexIndices[elevated_idx++] = (short)((i + 1) * vertexCountElevatedPlus3 + j + 1);
            }
         }
      }

      // Create indice list (PM)
      private short[] CreateTriangleIndicesBTT(CustomVertex.PositionColoredTextured[] ElevatedVertices, int VertexDensity, int Margin, double LayerRadius)
      {
         BinaryTriangleTree Tree = new BinaryTriangleTree(ElevatedVertices, VertexDensity, Margin, LayerRadius);
         Tree.BuildTree(7); // variance 0 = best fit
         Tree.BuildIndices();
         return Tree.Indices.ToArray();
      }

      private short[] CreateTriangleIndicesRegular(CustomVertex.PositionColoredTextured[] ElevatedVertices, int VertexDensity, int Margin, double LayerRadius)
      {
         short[] indices;
         int thisVertexDensityElevatedPlus2 = (VertexDensity + (Margin * 2));
         indices = new short[2 * thisVertexDensityElevatedPlus2 * thisVertexDensityElevatedPlus2 * 3];

         for (int i = 0; i < thisVertexDensityElevatedPlus2; i++)
         {
            int elevated_idx = (2 * 3 * i * thisVertexDensityElevatedPlus2);
            for (int j = 0; j < thisVertexDensityElevatedPlus2; j++)
            {
               indices[elevated_idx] = (short)(i * (thisVertexDensityElevatedPlus2 + 1) + j);
               indices[elevated_idx + 1] = (short)((i + 1) * (thisVertexDensityElevatedPlus2 + 1) + j);
               indices[elevated_idx + 2] = (short)(i * (thisVertexDensityElevatedPlus2 + 1) + j + 1);

               indices[elevated_idx + 3] = (short)(i * (thisVertexDensityElevatedPlus2 + 1) + j + 1);
               indices[elevated_idx + 4] = (short)((i + 1) * (thisVertexDensityElevatedPlus2 + 1) + j);
               indices[elevated_idx + 5] = (short)((i + 1) * (thisVertexDensityElevatedPlus2 + 1) + j + 1);

               elevated_idx += 6;
            }
         }
         return indices;
      }

      /// <summary>
      /// Create child tile terrain mesh
      /// </summary>
      protected void CreateElevatedMesh(ChildLocation corner, ref CustomVertex.PositionColoredTextured[] vertices,
         float meshBaseRadius, ref List<float> heightData, int samples)
      {
         // Figure out child lat/lon boundaries (radians)
         double north = MathEngine.DegreesToRadians(North);
         double west = MathEngine.DegreesToRadians(West);

         // Texture coordinate offsets
         float TuOffset = 0;
         float TvOffset = 0;

         float uOffset = 0;
         float vOffset = 0;

         double parentLongitudeSpan = Math.Abs(m_ParentTextureEast - m_ParentTextureWest);
         double parentLatitudeSpan = Math.Abs(m_ParentTextureNorth - m_ParentTextureSouth);

         switch (corner)
         {
            case ChildLocation.NorthWest:
               TuOffset = (float)(West - m_ParentTextureWest) / (float)parentLongitudeSpan;
               TvOffset = (float)(m_ParentTextureNorth - North) / (float)parentLatitudeSpan;
               break;
            case ChildLocation.NorthEast:
               west = MathEngine.DegreesToRadians(0.5 * (West + East));
               TuOffset = (float)(0.5 * (West + East) - m_ParentTextureWest) / (float)parentLongitudeSpan;
               TvOffset = (float)(m_ParentTextureNorth - North) / (float)parentLatitudeSpan;
               uOffset = 0.5f;
               break;
            case ChildLocation.SouthWest:
               north = MathEngine.DegreesToRadians(0.5 * (North + South));
               TuOffset = (float)(West - m_ParentTextureWest) / (float)parentLongitudeSpan;
               TvOffset = (float)(m_ParentTextureNorth - 0.5 * (North + South)) / (float)parentLatitudeSpan;
               vOffset = 0.5f;
               break;
            case ChildLocation.SouthEast:
               north = MathEngine.DegreesToRadians(0.5 * (North + South));
               west = MathEngine.DegreesToRadians(0.5 * (West + East));
               TuOffset = (float)(0.5 * (West + East) - m_ParentTextureWest) / (float)parentLongitudeSpan;
               TvOffset = (float)(m_ParentTextureNorth - 0.5 * (North + South)) / (float)parentLatitudeSpan;
               uOffset = 0.5f;
               vOffset = 0.5f;
               break;
         }

         double parentLatitudeRadianSpan = MathEngine.DegreesToRadians(parentLatitudeSpan);
         double parentLongitudeRadianSpan = MathEngine.DegreesToRadians(parentLongitudeSpan);

         double latitudeRadianSpan = MathEngine.DegreesToRadians(LatitudeSpan);
         double longitudeRadianSpan = MathEngine.DegreesToRadians(LongitudeSpan);

         double tuFactor = (longitudeRadianSpan / parentLongitudeRadianSpan) / (double)vertexCountElevated;
         double tvFactor = (latitudeRadianSpan / parentLatitudeRadianSpan) / (double)vertexCountElevated;

         float layerRadius = (float)QuadTileArgs.LayerRadius;
         double scaleFactor = 1f / vertexCountElevated;
         int terrainLongitudeIndex = (int)(uOffset * vertexCountElevated);
         int terrainLatitudeIndex = (int)(vOffset * vertexCountElevated);

         int vertexCountElevatedPlus1 = vertexCountElevated / 2 + 1;

         float radius = 0;
         int vertexIndex = 0;
         for (int latitudeIndex = -1; latitudeIndex <= vertexCountElevatedPlus1; latitudeIndex++)
         {
            int latitudePoint = latitudeIndex;
            if (latitudePoint < 0)
               latitudePoint = 0;
            else if (latitudePoint >= vertexCountElevatedPlus1)
               latitudePoint = vertexCountElevatedPlus1 - 1;

            double latitudeFactor = latitudePoint * scaleFactor;
            double latitude = north - latitudeFactor * latitudeRadianSpan;

            // Cache trigonometric values
            double cosLat = Math.Cos(latitude);
            double sinLat = Math.Sin(latitude);

            for (int longitudeIndex = -1; longitudeIndex <= vertexCountElevatedPlus1; longitudeIndex++)
            {
               int longitudePoint = longitudeIndex;
               if (longitudePoint < 0)
                  longitudePoint = 0;
               else if (longitudePoint >= vertexCountElevatedPlus1)
                  longitudePoint = vertexCountElevatedPlus1 - 1;

               if (longitudeIndex != longitudePoint || latitudeIndex != latitudePoint)
               {
                  if (heightData != null && heightData.Count > 0)
                  {
                     // Mesh base (flat)
                     radius = layerRadius +
                        heightData[terrainLatitudeIndex + latitudePoint + (terrainLongitudeIndex + longitudePoint) * samples]
                        * verticalExaggeration;
                  }
                  else
                  {
                     radius = meshBaseRadius;
                  }
               }
               else
               {
                  if (heightData != null && heightData.Count > 0)
                  {
                     // Top of mesh (real terrain)
                     radius = layerRadius +
                        heightData[terrainLatitudeIndex + latitudeIndex + (terrainLongitudeIndex + longitudeIndex) * samples]
                        * verticalExaggeration;
                  }
                  else
                  {
                     radius = meshBaseRadius;
                  }
               }

               double longitudeFactor = longitudePoint * scaleFactor;

               // Texture coordinates
               vertices[vertexIndex].Tu = TuOffset + longitudePoint * (float)tuFactor;//(float)longitudeFactor;
               vertices[vertexIndex].Tv = TvOffset + latitudePoint * (float)tvFactor;//(float)latitudeFactor;

               // Convert from spherical (radians) to cartesian
               double longitude = west + longitudeFactor * longitudeRadianSpan;
               double radCosLat = radius * cosLat;
               vertices[vertexIndex].X = (float)(radCosLat * Math.Cos(longitude) - m_TileOffset.X);
               vertices[vertexIndex].Y = (float)(radCosLat * Math.Sin(longitude) - m_TileOffset.Y);
               vertices[vertexIndex].Z = (float)(radius * sinLat - m_TileOffset.Z);
               vertices[vertexIndex].Color = m_CurrentOpacity << 24;
               vertexIndex++;
            }
         }
      }

      /// <summary>
      /// Render a rectangle around an image tile in the specified color
      /// </summary>
      public void RenderDownloadRectangle(DrawArgs drawArgs, int color)
      {
         // Render terrain download rectangle
         Vector3d northWestV = MathEngine.SphericalToCartesian((float)North, (float)West, QuadTileArgs.LayerRadius);
         Vector3d southWestV = MathEngine.SphericalToCartesian((float)South, (float)West, QuadTileArgs.LayerRadius);
         Vector3d northEastV = MathEngine.SphericalToCartesian((float)North, (float)East, QuadTileArgs.LayerRadius);
         Vector3d southEastV = MathEngine.SphericalToCartesian((float)South, (float)East, QuadTileArgs.LayerRadius);

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

      public virtual void Render(DrawArgs drawArgs)
      {
         try
         {
            if (!isInitialized || texture == null)
               return;

            if (!drawArgs.WorldCamera.ViewFrustum.Intersects(BoundingBox))
               return;

            Matrix curWorld = drawArgs.device.Transform.World;

            if (northWestChild != null && northWestChild.isInitialized)
            {
               northWestChild.Render(drawArgs);
            }
            else if (texture != null && !texture.Disposed)
            {
               drawArgs.device.Transform.World *= Matrix.Translation((float)m_TileOffset.X, (float)m_TileOffset.Y, (float)m_TileOffset.Z);
               drawArgs.device.SetTexture(0, texture);
               drawArgs.device.DrawIndexedUserPrimitives(
                  PrimitiveType.TriangleList, 0, northWestVertices.Length,
                  (m_NwIndices != null ? m_NwIndices.Length / 3 : vertexIndices.Length / 3),
                  (m_NwIndices != null ? m_NwIndices : vertexIndices),
                  true, northWestVertices);
               //drawArgs.device.Transform.World *= Matrix.Translation(-m_TileOffset.X, -m_TileOffset.Y, -m_TileOffset.Z);
               drawArgs.device.Transform.World = curWorld;
               drawArgs.numberTilesDrawn++;
            }

            if (northEastChild != null && northEastChild.isInitialized)
            {
               northEastChild.Render(drawArgs);
            }
            else if (texture != null && !texture.Disposed)
            {
               drawArgs.device.Transform.World *= Matrix.Translation((float)m_TileOffset.X, (float)m_TileOffset.Y, (float)m_TileOffset.Z);
               drawArgs.device.SetTexture(0, texture);
               drawArgs.device.DrawIndexedUserPrimitives(
                  PrimitiveType.TriangleList, 0, northEastVertices.Length,
                  (m_NeIndices != null ? m_NeIndices.Length / 3 : vertexIndices.Length / 3),
                  (m_NeIndices != null ? m_NeIndices : vertexIndices),
                  true, northEastVertices);
               //drawArgs.device.Transform.World *= Matrix.Translation(-m_TileOffset.X, -m_TileOffset.Y, -m_TileOffset.Z);
               drawArgs.device.Transform.World = curWorld;
               drawArgs.numberTilesDrawn++;
            }

            if (southWestChild != null && southWestChild.isInitialized)
            {
               southWestChild.Render(drawArgs);
            }
            else if (texture != null && !texture.Disposed)
            {
               drawArgs.device.Transform.World *= Matrix.Translation((float)m_TileOffset.X, (float)m_TileOffset.Y, (float)m_TileOffset.Z);
               drawArgs.device.SetTexture(0, texture);
               drawArgs.device.DrawIndexedUserPrimitives(
                  PrimitiveType.TriangleList, 0, southWestVertices.Length,
                  (m_SwIndices != null ? m_SwIndices.Length / 3 : vertexIndices.Length / 3),
                  (m_SwIndices != null ? m_SwIndices : vertexIndices),
                  true, southWestVertices);
               //drawArgs.device.Transform.World *= Matrix.Translation(-m_TileOffset.X, -m_TileOffset.Y, -m_TileOffset.Z);
               drawArgs.device.Transform.World = curWorld;
               drawArgs.numberTilesDrawn++;
            }

            if (southEastChild != null && southEastChild.isInitialized)
            {
               southEastChild.Render(drawArgs);
            }
            else if (texture != null && !texture.Disposed)
            {
               drawArgs.device.Transform.World *= Matrix.Translation((float)m_TileOffset.X, (float)m_TileOffset.Y, (float)m_TileOffset.Z);
               drawArgs.device.SetTexture(0, texture);
               drawArgs.device.DrawIndexedUserPrimitives(
                  PrimitiveType.TriangleList, 0, southEastVertices.Length,
                  (m_SeIndices != null ? m_SeIndices.Length / 3 : vertexIndices.Length),
                  (m_SeIndices != null ? m_SeIndices : vertexIndices),
                  true, southEastVertices);
               //drawArgs.device.Transform.World *= Matrix.Translation(-m_TileOffset.X, -m_TileOffset.Y, -m_TileOffset.Z);
               drawArgs.device.Transform.World = curWorld;
               drawArgs.numberTilesDrawn++;
            }
         }
         catch
         {
         }
      }
   }


   public class QuadTileArgs : IDisposable
   {

      #region Private Members

      QuadTileSet m_ParentQuadTileSet;
      double _layerRadius;
      bool _alwaysRenderBaseTiles;
      float _tileDrawSpread;
      float _tileDrawDistance;

      //int m_iTextureSize;

      int m_TransparentColor = 0;
      bool m_RenderTileFileNames = false;
      byte m_opacity = 255;
      bool _isDownloadingElevation;

      TerrainAccessor _terrainAccessor;
      IImageAccessor _imageAccessor;

      #endregion

      public GeographicBoundingBox Boundary;

      #region Properties

      public int TransparentColor
      {
         get
         {
            return m_TransparentColor;
         }
         set
         {
            m_TransparentColor = value;
         }
      }
      public QuadTileSet ParentQuadTileSet
      {
         get
         {
            return m_ParentQuadTileSet;
         }
      }
      public byte Opacity
      {
         get
         {
            return m_opacity;
         }
         set
         {
            m_opacity = value;
         }
      }

      public double LayerRadius
      {
         get
         {
            return this._layerRadius;
         }
         set
         {
            this._layerRadius = value;
         }
      }

      public bool AlwaysRenderBaseTiles
      {
         get
         {
            return this._alwaysRenderBaseTiles;
         }
         set
         {
            this._alwaysRenderBaseTiles = value;
         }
      }

      public float TileDrawSpread
      {
         get
         {
            return this._tileDrawSpread;
         }
         set
         {
            this._tileDrawSpread = value;
         }
      }

      public float TileDrawDistance
      {
         get
         {
            return this._tileDrawDistance;
         }
         set
         {
            this._tileDrawDistance = value;
         }
      }

      public bool IsDownloadingElevation
      {
         get
         {
            return this._isDownloadingElevation;
         }
         set
         {
            this._isDownloadingElevation = value;
         }
      }

      public bool RenderTileFileNames
      {
         get
         {
            return m_RenderTileFileNames;
         }
         set
         {
            m_RenderTileFileNames = value;
         }
      }

      public TerrainAccessor TerrainAccessor
      {
         get
         {
            return this._terrainAccessor;
         }
         set
         {
            this._terrainAccessor = value;
         }
      }

      public IImageAccessor ImageAccessor
      {
         get
         {
            return this._imageAccessor;
         }
      }


      #endregion

      /// <summary>
      /// Initializes a new instance of the <see cref= "T:WorldWind.Renderable.QuadTileArgs"/> class.
      /// </summary>
      /// <param name="layerRadius"></param>
      /// <param name="terrainAccessor"></param>
      /// <param name="imageAccessor"></param>
      public QuadTileArgs(
         double layerRadius,
        QuadTileSet parentQuadTileSet,
         TerrainAccessor terrainAccessor,
         IImageAccessor imageAccessor,
         bool alwaysRenderBaseTiles)
      {
         this._layerRadius = layerRadius;
         m_ParentQuadTileSet = parentQuadTileSet;
         this._tileDrawDistance = 3.5f;
         this._tileDrawSpread = 2.9f;
         this._imageAccessor = imageAccessor;
         this._terrainAccessor = terrainAccessor;
         this._alwaysRenderBaseTiles = alwaysRenderBaseTiles;
      }

      public void Dispose()
      {
         _imageAccessor.DownloadQueue.ClearDownloadRequests();
      }
   }

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
