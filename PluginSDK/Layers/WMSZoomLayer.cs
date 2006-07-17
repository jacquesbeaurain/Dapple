using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.IO;
using WorldWind;
using WorldWind.Renderable;
using WorldWind.Terrain;
using WorldWind.Net;
using WorldWind.Net.Wms;
using WorldWind.VisualControl;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;


namespace WorldWind.Renderable
{
   public class WMSZoomLayer : Renderable.RenderableObject
   {
      private WMSZoomLayer m_oChild;
      private ImageLayer m_oInitial;
      private decimal m_decZoomPercent;
      private decimal m_decMinViewRange;

      private World m_oWorld;
      private double m_dblHeight;
      private WMSLayer m_oWMSLayer;
      private TerrainAccessor m_oTerrainAccessor;
      private int m_intPixelSize;
      private string m_strCachePath;

      private decimal m_decMaxLat;
      private decimal m_decMaxLon;
      private decimal m_decMinLat;
      private decimal m_decMinLon;

      private int m_intLevel;

      private bool m_bFailed = false;
      private WMSDownload m_oWMSDownload;
      WorldWind.Net.WebDownload m_oDownload;
      public float ProgressPercent;
      private DrawArgs m_oDrawArgs;

      Texture m_iconTexture;
      Sprite sprite;
      Rectangle m_spriteSize;
      ProgressBar progressBar;
      Texture m_serverLogo;


      private bool renderchild = false;

      public delegate void LoadFailedHandler(RenderableObject oRO, string message);
      public event LoadFailedHandler LoadFailed;

      public WMSZoomLayer(string name,
         World parentWorld,
         double distanceAboveSurface,
         WMSLayer wmsLayer,
         decimal minLatitude,
         decimal maxLatitude,
         decimal minLongitude,
         decimal maxLongitude,
         int level,
         int size,
         byte opacity,
         bool ison,
         string cachePath,
         TerrainAccessor terrainAccessor,
         decimal zoomPercent)
         : base(name, parentWorld.Position, parentWorld.Orientation)
      {
         this.Opacity = opacity;
         this.IsOn = ison;
         m_intLevel = level;
         m_oWorld = parentWorld;
         m_decZoomPercent = zoomPercent;
         m_dblHeight = distanceAboveSurface;
         m_oTerrainAccessor = terrainAccessor;
         m_intPixelSize = size;
         m_oWMSLayer = wmsLayer;
         if (maxLongitude < minLongitude)
            throw new ApplicationException("Need to make sure this is sane!!!");
         if (level == 0)
         {
            m_decMinViewRange = Math.Max(Math.Min(maxLatitude - minLatitude, 180), Math.Min(maxLongitude - minLongitude, 180));
         }
         else
         {
            m_decMinViewRange = m_decZoomPercent * Math.Max(maxLatitude - minLatitude, maxLongitude - minLongitude);
         }
         
         // Round the ranges to the nearest larger area of integer multiples of half the minimum view range around center of layer
         decimal dblLatCent =  m_oWMSLayer.South + (m_oWMSLayer.North - m_oWMSLayer.South) / 2;
         decimal dblLonCent = m_oWMSLayer.West + (m_oWMSLayer.East - m_oWMSLayer.West) / 2;
         m_decMaxLat = Math.Min( m_oWMSLayer.North, dblLatCent + (m_decMinViewRange / 2) * Math.Ceiling((maxLatitude - dblLatCent) / (m_decMinViewRange / 2)));
         m_decMinLat = Math.Max( m_oWMSLayer.South, dblLatCent + (m_decMinViewRange / 2) * Math.Floor((minLatitude - dblLatCent) / (m_decMinViewRange / 2)));
         m_decMaxLon = Math.Min( m_oWMSLayer.East, dblLonCent + (m_decMinViewRange / 2) * Math.Ceiling((maxLongitude - dblLonCent) / (m_decMinViewRange / 2)));
         m_decMinLon = Math.Max( m_oWMSLayer.West, dblLonCent + (m_decMinViewRange / 2) * Math.Floor((minLongitude - dblLonCent) / (m_decMinViewRange / 2)));

         m_strCachePath = cachePath;
         m_decZoomPercent = zoomPercent;
      }

      public override byte Opacity
      {
         get
         {
            return base.Opacity;
         }
         set
         {
            if (m_oInitial != null)
            {
               m_oInitial.Opacity = value;
            }
            if (m_oChild != null)
            {
               m_oChild.Opacity = value;
            }
            base.Opacity = value;
         }
      }

      public override bool IsOn
      {
         get
         {
            return base.IsOn;
         }
         set
         {
            if (m_oInitial != null)
            {
               m_oInitial.IsOn = value;
            }
            if (m_oChild != null)
            {
               m_oChild.IsOn = value;
            }
            base.IsOn = value;
         }
      }

      public decimal MaxLat
      {
         get
         {
            return m_decMaxLat;
         }
      }

      public decimal MinLat
      {
         get
         {
            return m_decMinLat;
         }
      }

      public decimal MaxLon
      {
         get
         {
            return m_decMaxLon;
         }
      }

      public decimal MinLon
      {
         get
         {
            return m_decMinLon;
         }
      }

      public bool Ready
      {
         get
         {
            return m_oInitial != null && m_oInitial.isInitialized;
         }
      }

      public override void Initialize(DrawArgs drawArgs)
      {
         decimal north, south, east, west;
         uint size;

         north = Convert.ToDecimal(m_decMaxLat);
         south = Convert.ToDecimal(m_decMinLat);
         west = Convert.ToDecimal(m_decMinLon);
         east = Convert.ToDecimal(m_decMaxLon);
         size = Convert.ToUInt32(m_intPixelSize);

         string dateString = string.Empty;
         if (m_oWMSLayer.Dates != null && m_oWMSLayer.Dates.Length > 0)
         {
            dateString = m_oWMSLayer.Dates[0];
         }

         string dlPath = Path.Combine(m_strCachePath, m_oWMSLayer.GetWMSDownloadFile(dateString, null, north, south, west, east, size));

         if (!Directory.Exists(Path.GetDirectoryName(dlPath)))
            Directory.CreateDirectory(Path.GetDirectoryName(dlPath));

         m_oDrawArgs = drawArgs;

         if (File.Exists(dlPath + ".wwi") && File.Exists(dlPath))
         {
            try
            {
               WMSImageLayerInfo imageLayerInfo = WMSImageLayerInfo.FromFile(dlPath + ".wwi");
               m_oInitial = new ImageLayer(imageLayerInfo.Description,
                                           m_oWorld,
                                           0,
                                           dlPath,
                                           imageLayerInfo.South,
                                           imageLayerInfo.North,
                                           imageLayerInfo.West,
                                           imageLayerInfo.East,
                                           this.Opacity, m_oTerrainAccessor);
               m_oInitial.IsOn = this.IsOn;
               return;
            }
            // Cached file not readable for some reason - reload
            catch (IOException) { }
            catch (FormatException) { }
         }

         m_oWMSDownload = m_oWMSLayer.GetWmsRequest( dateString, null, north, south, west, east, size, m_strCachePath);
         m_oDownload = new WorldWind.Net.WebDownload(m_oWMSDownload.Url);
         m_oDownload.BackgroundDownloadMemory(new WorldWind.Net.DownloadCompleteHandler(DownloadCompleteCallbackHandler));
         m_oDownload.ProgressCallback += new DownloadProgressHandler(UpdateProgress);
      }

      protected override void FreeResources()
      {
         if (m_oChild != null)
         {
            m_oChild.Dispose();
            m_oChild = null;
         }
         if (m_oInitial != null)
         {
            m_oInitial.Dispose();
            m_oInitial = null;
         }
      }

      void DownloadCompleteCallbackHandler(WorldWind.Net.WebDownload download)
      {
         try
         {
            if (m_oWMSDownload == null)
            {
               //TODO Investigate this weirdness
               download.Dispose();
               return;
            }
            download.Verify();

            if (download.ContentStream.Length == 0)
               throw new System.Net.WebException("Server returned no data.");

            download.ContentStream.Seek(0, SeekOrigin.Begin);
            Image img = Image.FromStream(download.ContentStream);
            img.Save(m_oWMSDownload.SavedFilePath);
         }
         catch
         {
            if (m_oWMSDownload != null)
            {
               try
               {
                  string fileName = m_oWMSDownload.SavedFilePath + ".xml";
                  download.SaveMemoryDownloadToFile(fileName);
                  if (LoadFailed != null)
                  {
                     LoadFailed(this, "Image download failed");
                  }
               }
               catch
               {

               }
            }
            download.Dispose();
            m_bFailed = true;
            m_oWMSDownload = null;
            if (m_oDownload != null)
            {
               m_oDownload.Dispose();
               m_oDownload = null;
            }            
         }

         if (m_oWMSDownload == null)
            m_bFailed = true;

         if (m_bFailed)
            return;

         WMSImageLayerInfo imageLayerInfo = new WMSImageLayerInfo(m_oWMSDownload);
         imageLayerInfo.Save(m_oWMSDownload.SavedFilePath + ".wwi");
         m_oInitial = new ImageLayer(imageLayerInfo.Description,
             m_oWorld,
             0,
             m_oWMSDownload.SavedFilePath,
             imageLayerInfo.South,
             imageLayerInfo.North,
             imageLayerInfo.West,
             imageLayerInfo.East,
             this.Opacity, m_oTerrainAccessor);
         m_oInitial.IsOn = this.IsOn;
      }

      public override void Update(DrawArgs drawArgs)
      {
         // Don't do anything outside layer extents! (TODO: May need refinement with tilt etc.)
         if (drawArgs.WorldCamera.Latitude.Degrees - drawArgs.WorldCamera.ViewRange.Degrees / 2.0 > (double)MaxLat ||
             drawArgs.WorldCamera.Latitude.Degrees + drawArgs.WorldCamera.ViewRange.Degrees / 2.0 < (double)MinLat ||
             drawArgs.WorldCamera.Longitude.Degrees - drawArgs.WorldCamera.ViewRange.Degrees / 2.0 > (double)MaxLon ||
             drawArgs.WorldCamera.Longitude.Degrees + drawArgs.WorldCamera.ViewRange.Degrees / 2.0 < (double)MinLon)
            return;

         // Decide whether to create/render a child
         if (drawArgs.WorldCamera.ViewRange.Degrees < (double)m_decMinViewRange)
         {
            renderchild = true;
         }
         else
            renderchild = false;

         if (m_oChild != null)
         {
            // Decide whether the child layer needs to be moved
            if (Math.Min((double)m_oWMSLayer.North, drawArgs.WorldCamera.Latitude.Degrees + drawArgs.WorldCamera.ViewRange.Degrees * (double)m_decZoomPercent) > (double)m_oChild.MaxLat ||
                Math.Max((double)m_oWMSLayer.South, drawArgs.WorldCamera.Latitude.Degrees - drawArgs.WorldCamera.ViewRange.Degrees * (double)m_decZoomPercent) < (double)m_oChild.MinLat ||
                Math.Min((double)m_oWMSLayer.East, drawArgs.WorldCamera.Longitude.Degrees + drawArgs.WorldCamera.ViewRange.Degrees * (double)m_decZoomPercent) > (double)m_oChild.MaxLon ||
                Math.Max((double)m_oWMSLayer.West, drawArgs.WorldCamera.Longitude.Degrees - drawArgs.WorldCamera.ViewRange.Degrees * (double)m_decZoomPercent) < (double)m_oChild.MinLon)
            {
               m_oChild.Dispose();
               m_oChild = null;
            }
         }

         if (m_oChild == null && renderchild)
            m_oChild = CreateChild(drawArgs);

         if (m_oChild != null && renderchild)
            m_oChild.Update(drawArgs);
         
         if (m_oInitial == null && !m_bFailed)
            Initialize(drawArgs);

         if (m_oInitial != null)
            m_oInitial.Update(drawArgs);
      }

      private WMSZoomLayer CreateChild(DrawArgs drawArgs)
      {
         decimal lat = Math.Min((m_decMinViewRange) * m_decZoomPercent, 90);
         decimal lon = Math.Min((m_decMinViewRange) * m_decZoomPercent, 89);

         return new WMSZoomLayer(m_oWMSLayer.Title, m_oWorld, m_dblHeight, m_oWMSLayer,
                  (decimal)drawArgs.WorldCamera.Latitude.Degrees - lat, (decimal) drawArgs.WorldCamera.Latitude.Degrees + lat,
                  (decimal)drawArgs.WorldCamera.Longitude.Degrees - lon, (decimal) drawArgs.WorldCamera.Longitude.Degrees + lon,
                  m_intLevel + 1, m_intPixelSize, this.Opacity, this.IsOn,
                  m_strCachePath, m_oTerrainAccessor, m_decZoomPercent);
      }

      public override void Render(DrawArgs drawArgs)
      {
         // Don't do anything outside layer extents! (TODO: May need refinement with tilt etc.)
         if (drawArgs.WorldCamera.Latitude.Degrees - drawArgs.WorldCamera.ViewRange.Degrees / 2.0 > (double)MaxLat ||
             drawArgs.WorldCamera.Latitude.Degrees + drawArgs.WorldCamera.ViewRange.Degrees / 2.0 < (double)MinLat ||
             drawArgs.WorldCamera.Longitude.Degrees - drawArgs.WorldCamera.ViewRange.Degrees / 2.0 > (double)MaxLon ||
             drawArgs.WorldCamera.Longitude.Degrees + drawArgs.WorldCamera.ViewRange.Degrees / 2.0 < (double)MinLon)
            return;

         if (m_oChild != null && renderchild)
         {
            if (m_oChild.Ready)
            {
               m_oChild.Render(drawArgs);
               return;
            }
            else
            {
               if (World.Settings.ShowDownloadRectangles)
                  m_oChild.RenderDownloadRectangle(drawArgs);
               if (World.Settings.ShowDownloadProgress)
                  m_oChild.RenderDownloadIndicator(drawArgs);
            }
         }

         if (Ready)
            m_oInitial.Render(drawArgs);

         if (m_oChild != null && renderchild)
         {
               if (!m_oChild.Ready)
               {
                  if (World.Settings.ShowDownloadRectangles)
                     m_oChild.RenderDownloadRectangle(drawArgs);
                  if (World.Settings.ShowDownloadProgress)
                     m_oChild.RenderDownloadIndicator(drawArgs);
               }
         }

         if (!Ready)
         {
            if (World.Settings.ShowDownloadRectangles)
               RenderDownloadRectangle(drawArgs);
            if (World.Settings.ShowDownloadProgress)
               RenderDownloadIndicator(drawArgs);
         }
      }

      public void RenderDownloadRectangle(DrawArgs drawArgs)
      {
         int color = System.Drawing.Color.Red.ToArgb();
         double rad = m_dblHeight + m_oWorld.EquatorialRadius;
         Microsoft.DirectX.Direct3D.CustomVertex.PositionColored[] downloadRectangle = new Microsoft.DirectX.Direct3D.CustomVertex.PositionColored[5];
         Microsoft.DirectX.Vector3 northWestV = ConvertDX.FromVector3d(MathEngine.SphericalToCartesian((float)m_decMaxLat, (float)m_decMinLon, rad));
         Microsoft.DirectX.Vector3 southWestV = ConvertDX.FromVector3d(MathEngine.SphericalToCartesian((float)m_decMinLat, (float)m_decMinLon, rad));
         Microsoft.DirectX.Vector3 northEastV = ConvertDX.FromVector3d(MathEngine.SphericalToCartesian((float)m_decMaxLat, (float)m_decMaxLon, rad));
         Microsoft.DirectX.Vector3 southEastV = ConvertDX.FromVector3d(MathEngine.SphericalToCartesian((float)m_decMinLat, (float)m_decMaxLon, rad));

         downloadRectangle[0].X = northWestV.X;
         downloadRectangle[0].Y = northWestV.Y;
         downloadRectangle[0].Z = northWestV.Z;
         downloadRectangle[0].Color = color;

         downloadRectangle[1].X = southWestV.X;
         downloadRectangle[1].Y = southWestV.Y;
         downloadRectangle[1].Z = southWestV.Z;
         downloadRectangle[1].Color = color;

         downloadRectangle[2].X = southEastV.X;
         downloadRectangle[2].Y = southEastV.Y;
         downloadRectangle[2].Z = southEastV.Z;
         downloadRectangle[2].Color = color;

         downloadRectangle[3].X = northEastV.X;
         downloadRectangle[3].Y = northEastV.Y;
         downloadRectangle[3].Z = northEastV.Z;
         downloadRectangle[3].Color = color;

         downloadRectangle[4].X = downloadRectangle[0].X;
         downloadRectangle[4].Y = downloadRectangle[0].Y;
         downloadRectangle[4].Z = downloadRectangle[0].Z;
         downloadRectangle[4].Color = color;

         drawArgs.device.RenderState.ZBufferEnable = false;
         drawArgs.device.VertexFormat = Microsoft.DirectX.Direct3D.CustomVertex.PositionColored.Format;
         drawArgs.device.TextureState[0].ColorOperation = Microsoft.DirectX.Direct3D.TextureOperation.Disable;
         drawArgs.device.DrawUserPrimitives(Microsoft.DirectX.Direct3D.PrimitiveType.LineStrip, 4, downloadRectangle);
         drawArgs.device.TextureState[0].ColorOperation = Microsoft.DirectX.Direct3D.TextureOperation.SelectArg1;
         drawArgs.device.VertexFormat = Microsoft.DirectX.Direct3D.CustomVertex.PositionTextured.Format;
         drawArgs.device.RenderState.ZBufferEnable = true;
      }

      Texture GetLogo(DrawArgs drawArgs)
      {
         string strServerLogo = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + "\\Data\\Icons\\Interface\\wms.png";
         if (m_serverLogo == null && File.Exists(strServerLogo))
         {
            m_serverLogo = ImageHelper.LoadTexture(drawArgs.device, strServerLogo);
         }

         return m_serverLogo;
      }

      void UpdateProgress(int pos, int total)
      {
         if (total == 0)
            // When server doesn't provide content-length, use this dummy value to at least show some progress.
            total = 50 * 1024;
         pos = pos % (total + 1);
         ProgressPercent = (float)pos / total;
      }

      protected virtual void RenderDownloadIndicator(DrawArgs drawArgs)
      {
         if (m_oDownload != null)
         {
            int halfIconHeight = 24;
            int halfIconWidth = 24;

            Vector3 projectedPoint = new Vector3(drawArgs.screenWidth - 10 - halfIconWidth, drawArgs.screenHeight - 34, 0.5f);

            // Render progress bar
            if (progressBar == null)
               progressBar = new ProgressBar(40, 4);
            progressBar.Draw(drawArgs, projectedPoint.X, projectedPoint.Y + 24, ProgressPercent, World.Settings.DownloadProgressColor.ToArgb());
            drawArgs.device.RenderState.ZBufferEnable = true;


            m_iconTexture = GetLogo(drawArgs);

            if (m_iconTexture == null || m_iconTexture.Disposed)
               return;

            if (sprite == null)
            {
               using (Surface s = m_iconTexture.GetSurfaceLevel(0))
               {
                  SurfaceDescription desc = s.Description;
                  m_spriteSize = new Rectangle(0, 0, desc.Width, desc.Height);
               }

               this.sprite = new Sprite(drawArgs.device);
            }

            float scaleWidth = (float)2.0f * halfIconWidth / m_spriteSize.Width;
            float scaleHeight = (float)2.0f * halfIconHeight / m_spriteSize.Height;


            this.sprite.Begin(SpriteFlags.AlphaBlend);
            this.sprite.Transform = Matrix.Transformation2D(new Vector2(0.0f, 0.0f), 0.0f, new Vector2(scaleWidth, scaleHeight),
               new Vector2(0, 0),
               0.0f, new Vector2(projectedPoint.X, projectedPoint.Y));

            this.sprite.Draw(m_iconTexture, m_spriteSize,
               new Vector3(1.32f * 48, 1.32f * 48, 0),
               new Vector3(0, 0, 0),
               World.Settings.downloadLogoColor);
            this.sprite.End();
         }
      }

      public override void Dispose()
      {
         if (m_oChild != null)
         {
            m_oChild.Dispose();
            m_oChild = null;
         }
         if (m_oInitial != null)
         {
            m_oInitial.Dispose();
            m_oInitial = null;
         }
         m_oWMSDownload = null;
         if (m_oDownload != null)
         {
            m_oDownload.Dispose();
            m_oDownload = null;
         }
      }

      public override bool PerformSelectionAction(DrawArgs drawArgs)
      {
         return false;
      }
   }
}
