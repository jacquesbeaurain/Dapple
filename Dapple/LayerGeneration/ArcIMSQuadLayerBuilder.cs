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

      private String m_szServiceName;
      private QuadTileSet m_oQuadTileSet = null;
      private bool m_blnIsChanged = true;
      private GeographicBoundingBox m_oEnvelope;
      private ArcIMSServerUri m_oServerUri;
      private double m_dLevelZeroTileSizeDegrees = 0;
      private int m_iLevels = 15;
      private int m_iPixels = 256;
      private String m_szLayerID;
      private double m_dMinScale, m_dMaxScale;

      #endregion

      #region Static

      public static readonly string URLProtocolName = "gxarcims://";
      public static readonly string CacheSubDir = "ArcIMSImages";

      #endregion

      #region Constructor

      public ArcIMSQuadLayerBuilder(ArcIMSServerUri oServerUri, String strServiceName, String szLayerTitle, String szLayerID, GeographicBoundingBox oEnvelope, WorldWindow oWorldWindow, IBuilder oParent, double dMinScale, double dMaxScale)
         :base(szLayerTitle, oWorldWindow, oParent)
      {
         m_oEnvelope = oEnvelope;
         m_szLayerID = szLayerID;
         m_szServiceName = strServiceName;
         m_oServerUri = oServerUri;
         m_dMinScale = dMinScale;
         m_dMaxScale = dMaxScale;

         m_dLevelZeroTileSizeDegrees = 22.5;
         m_iLevels = 1;
         m_iPixels = 256;

         while ((m_dLevelZeroTileSizeDegrees / m_iPixels) > m_dMaxScale)
         {
            m_dLevelZeroTileSizeDegrees /= 2.0;
         }

         if (m_dLevelZeroTileSizeDegrees / m_iPixels < m_dMinScale)
         {
            double dMidLZTS = (m_dMinScale + m_dMaxScale) * m_iPixels / 2.0;
            double dCompressRatio = m_dLevelZeroTileSizeDegrees / dMidLZTS;
            m_iPixels = (int)(m_iPixels * dCompressRatio);
         }
         else
         {
            double dIter = m_dLevelZeroTileSizeDegrees / 2.0;

            while (dIter / m_iPixels > m_dMinScale && dIter > 0.35)
            {
               dIter /= 2.0;
               m_iLevels++;
            }
         }

         // --- Check the calculations ---
         if (m_dLevelZeroTileSizeDegrees > m_dMaxScale * m_iPixels) throw new InvalidDataException("LZTS is wrong");
         if (m_dLevelZeroTileSizeDegrees < m_dMinScale * m_iPixels * Math.Pow(2.0, m_iLevels - 1)) throw new InvalidDataException("Levels is wrong");
      }

      #endregion

      [System.ComponentModel.Browsable(true)]
      public double MinMapUnitsPerPixel { get { return m_dMinScale; } }

      [System.ComponentModel.Browsable(true)]
      public double MaxMapUnitsPerPixel { get { return m_dMaxScale; } }

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

      [System.ComponentModel.Browsable(false)]
      public override bool IsChanged
      {
         get { return m_blnIsChanged; }
      }

      public override string ServerTypeIconKey
      {
         get { return "arcims"; }
      }

      public override string DisplayIconKey
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

      public override RenderableObject GetLayer()
      {
         Console.WriteLine(m_dLevelZeroTileSizeDegrees + "," + m_iPixels + "," + m_iLevels);

         if (m_blnIsChanged)
         {
            ImageStore[] aImageStore = new ImageStore[1];
            aImageStore[0] = new ArcIMSImageStore(m_szServiceName, m_szLayerID, m_oServerUri, m_iPixels);
            aImageStore[0].DataDirectory = null;
            aImageStore[0].LevelZeroTileSizeDegrees = m_dLevelZeroTileSizeDegrees;
            aImageStore[0].LevelCount = m_iLevels;
            aImageStore[0].ImageExtension = ".png";
            aImageStore[0].CacheDirectory = GetCachePath();
            aImageStore[0].TextureFormat = World.Settings.TextureFormat;

            m_oQuadTileSet = new QuadTileSet(m_szTreeNodeText, m_oWorldWindow.CurrentWorld, 0,
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
         return m_oServerUri.ToServiceUri(m_szTreeNodeText).Replace("http://", URLProtocolName) + String.Format("&minx={0}&miny={1}&maxx={2}&maxy={3}", m_oEnvelope.West, m_oEnvelope.South, m_oEnvelope.East, m_oEnvelope.North);
      }

      public override string GetCachePath()
      {
         return Path.Combine(Path.Combine(Path.Combine(m_strCacheRoot, CacheSubDir), m_oServerUri.ToCacheDirectory()), m_szServiceName + " - " + m_szLayerID);
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
         return new ArcIMSQuadLayerBuilder(m_oServerUri, m_szServiceName, this.m_szTreeNodeText, m_szLayerID, m_oEnvelope, m_oWorldWindow, m_Parent, m_dMinScale, m_dMaxScale);
      }

      public override bool Equals(object obj)
      {
         if (!(obj is ArcIMSQuadLayerBuilder)) return false;
         ArcIMSQuadLayerBuilder castObj = obj as ArcIMSQuadLayerBuilder;

         // -- Equal if they're the same service from the same server --
         return m_oServerUri.Equals(castObj.m_oServerUri) && m_szTreeNodeText.Equals(castObj.m_szTreeNodeText);
      }

      #endregion

      #region Private Members

      /*// Copied shamelessly from WMS
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
      }*/

      public override void GetOMMetadata(out String szDownloadType, out String szServerURL, out String szLayerId)
      {
         szDownloadType = "arcims";
         szServerURL = m_oServerUri.ToBaseUri();
         szLayerId = m_szLayerID;
      }

      #endregion
   }
}
