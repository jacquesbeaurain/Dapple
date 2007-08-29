using System;
using System.Collections.Generic;
using System.Text;
using WorldWind.Renderable;
using WorldWind;
using System.IO;
using WorldWind.PluginEngine;

namespace Dapple.LayerGeneration
{
   class ArcIMSQuadLayerBuilder : LayerBuilder
   {
      #region Member variables

      private QuadTileSet m_oQuadTileSet = null;
      private bool m_blnIsChanged = true;
      private GeographicBoundingBox m_oEnvelope;
      private ArcIMSServerUri m_oUri;
      private double m_dLevelZeroTileSizeDegrees = 0;
      private int m_iLevels = 15;

      #endregion

      #region Static

      public static readonly string TypeName = "ArcIMSQuadLayer";
      public static readonly string URLProtocolName = "gxarcims://";
      public static readonly string CacheSubDir = "ArcIMSImages";

      #endregion

      #region Constructor

      public ArcIMSQuadLayerBuilder(ArcIMSServerUri oUri, String strServiceName, GeographicBoundingBox oEnvelope, WorldWindow oWorldWindow, IBuilder oParent)
         :base(strServiceName, oWorldWindow, oParent)
      {
         m_oEnvelope = oEnvelope;
         m_oUri = oUri;

         // Determine the needed levels (function of tile size and resolution, for which we just use ~5 meters because it is not available with WMS)
         double dRes = 5.0 / 100000.0;
         if (dRes > 0)
         {
            double dTileSize = LevelZeroTileSize;
            m_iLevels = 1;
            while (dTileSize / Convert.ToDouble(256) > dRes / 4.0)
            {
               m_iLevels++;
               dTileSize /= 2;
            }
         }
      }

      #endregion

      #region ImageBuilder Implementations

      public override GeographicBoundingBox Extents
      {
         get { return m_oEnvelope; }
      }

      public override byte Opacity
      {
         get
         {
            if (m_oQuadTileSet != null)
               return m_oQuadTileSet.Opacity;
            return m_bOpacity;
         }
         set
         {
            bool bChanged = false;
            if (m_bOpacity != value)
            {
               m_bOpacity = value;
               bChanged = true;
            }
            if (m_oQuadTileSet != null && m_oQuadTileSet.Opacity != value)
            {
               m_oQuadTileSet.Opacity = value;
               bChanged = true;
            }
            if (bChanged)
               SendBuilderChanged(BuilderChangeType.OpacityChanged);
         }
      }

      public override bool Visible
      {
         get
         {
            if (m_oQuadTileSet != null)
               return m_oQuadTileSet.IsOn;
            return m_IsOn;
         }
         set
         {
            bool bChanged = false;
            if (m_IsOn != value)
            {
               m_IsOn = value;
               bChanged = true;
            }
            if (m_oQuadTileSet != null && m_oQuadTileSet.IsOn != value)
            {
               m_oQuadTileSet.IsOn = value;
               bChanged = true;
            }

            if (bChanged)
               SendBuilderChanged(BuilderChangeType.VisibilityChanged);
         }
      }

      public override string Type
      {
         get { return TypeName; }
      }

      public override bool IsChanged
      {
         get { return m_blnIsChanged; }
      }

      public override string ServerTypeIconKey
      {
         get { return "arcims"; }
      }

      public override string LayerTypeIconKey
      {
         get { return "layer"; }
      }

      public override bool bIsDownloading(out int iBytesRead, out int iTotalBytes)
      {
         if (m_oQuadTileSet != null)
            return m_oQuadTileSet.bIsDownloading(out iBytesRead, out iTotalBytes);
         else
         {
            iBytesRead = 0;
            iTotalBytes = 0;
            return false;
         }
      }

      public override string ServiceType
      {
         get { return "ArcIMS Server"; }
      }

      public override RenderableObject GetLayer()
      {
         if (m_blnIsChanged)
         {
            ImageStore[] aImageStore = new ImageStore[1];
            aImageStore[0] = new ArcIMSImageStore(m_strName, m_oUri);
            aImageStore[0].DataDirectory = null;
            aImageStore[0].LevelZeroTileSizeDegrees = LevelZeroTileSize;
            aImageStore[0].LevelCount = m_iLevels;
            aImageStore[0].ImageExtension = ".png";
            aImageStore[0].CacheDirectory = GetCachePath();
            aImageStore[0].TextureFormat = World.Settings.TextureFormat;
            aImageStore[0].TextureSizePixels = 256;

            m_oQuadTileSet = new QuadTileSet(m_strName, m_oWorldWindow.CurrentWorld, 0,
               m_oEnvelope.North, m_oEnvelope.South, m_oEnvelope.West, m_oEnvelope.East,
               true, aImageStore);
            m_oQuadTileSet.AlwaysRenderBaseTiles = true;
            m_oQuadTileSet.IsOn = m_IsOn;
            m_oQuadTileSet.Opacity = m_bOpacity;
            m_blnIsChanged = false;
         }
         return m_oQuadTileSet;
      }

      public override string GetURI()
      {
         return m_oUri.ToServiceUri(m_strName).Replace("http://", URLProtocolName) + String.Format("&minx={0}&miny={1}&maxx={2}&maxy={3}", m_oEnvelope.West, m_oEnvelope.South, m_oEnvelope.East, m_oEnvelope.North);
      }

      public override string GetCachePath()
      {
         return Path.Combine(Path.Combine(Path.Combine(m_strCacheRoot, CacheSubDir), m_oUri.ToCacheDirectory()), m_strName);
      }

      protected override void CleanUpLayer(bool bFinal)
      {
         if (m_oQuadTileSet != null)
            m_oQuadTileSet.Dispose();
         m_oQuadTileSet = null;
         m_blnIsChanged = true;
      }

      public override object CloneSpecific()
      {
         return new ArcIMSQuadLayerBuilder(m_oUri, m_strName, m_oEnvelope, m_oWorldWindow, m_Parent);
      }

      public override bool Equals(object obj)
      {
         if (!(obj is ArcIMSQuadLayerBuilder)) return false;
         ArcIMSQuadLayerBuilder castObj = obj as ArcIMSQuadLayerBuilder;

         // -- Equal if they're the same service from the same server --
         return m_oUri.Equals(castObj.m_oUri) && m_strName.Equals(castObj.m_strName);
      }

      #endregion

      #region Private Members

      // Copied shamelessly from WMS
      private double LevelZeroTileSize
      {
         get
         {
            if (m_dLevelZeroTileSizeDegrees == 0)
            {
               // Round to ceiling of four decimals (>~ 10 meter resolution)
               // Empirically determined as pretty good tile size choice for small data sets
               double dLevelZero = Math.Ceiling(10000.0 * Math.Max(m_oEnvelope.North - m_oEnvelope.South, m_oEnvelope.West - m_oEnvelope.East)) / 10000.0;

               // Optimum tile alignment when this is 180/(2^n), the first value is 180/2^3
               m_dLevelZeroTileSizeDegrees = 22.5;
               while (dLevelZero < m_dLevelZeroTileSizeDegrees)
                  m_dLevelZeroTileSizeDegrees /= 2;
            }
            return m_dLevelZeroTileSizeDegrees;
         }
      }

      #endregion
   }
}
