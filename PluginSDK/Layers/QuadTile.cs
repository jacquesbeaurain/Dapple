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

      public virtual void ComputeChildren(DrawArgs drawArgs)
      {
         double tileSize = 0.5 * (North - South);

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
                  if (northEastChild == null || northWestChild == null || southEastChild == null || southWestChild == null)
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
            bool bChildren = false;

            if (!isInitialized || texture == null)
               return;
            if (!drawArgs.WorldCamera.ViewFrustum.Intersects(BoundingBox))
               return;

            if (northWestChild != null && northWestChild.isInitialized)
            {
               northWestChild.ExportProcess(drawArgs, expInfo);
               bChildren = true;
            }

            if (northEastChild != null && northEastChild.isInitialized)
            {
               northEastChild.ExportProcess(drawArgs, expInfo);
               bChildren = true;
            }

            if (southWestChild != null && southWestChild.isInitialized)
            {
               southWestChild.ExportProcess(drawArgs, expInfo);
               bChildren = true;
            }

            if (southEastChild != null && southEastChild.isInitialized)
            {
               southEastChild.ExportProcess(drawArgs, expInfo);
               bChildren = true;
            }

            if (!bChildren && texture != null)
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
}
