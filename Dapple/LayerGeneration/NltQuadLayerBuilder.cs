using System;
using System.Globalization;
using System.Collections.Generic;
using System.Text;
using System.IO;
using WorldWind;
using WorldWind.Renderable;
using System.Xml;

namespace Dapple.LayerGeneration
{
   public class NltQuadLayerBuilder : LayerBuilder
	{
		#region Statics

		public static readonly string URLProtocolName = "gxtile://";

		public static readonly string CacheSubDir = "Images";

		#endregion

      #region Member Variables

      QuadTileSet m_oQuadTileSet;
		
		int distAboveSurface = 0;
		bool terrainMapped = false;
		GeographicBoundingBox m_hBoundary = new GeographicBoundingBox(0, 0, 0, 0);

		bool m_blnIsChanged = true;

		private string m_strServerUrl;
		private string m_strDatasetName;
		private string m_strImageExt;
      private string m_strWorldName;

		public int m_iTextureSizePixels;

		private int m_iLevels;
		private double m_dLevelZeroTileSizeDegrees;

      #endregion

      #region Constructor

      public NltQuadLayerBuilder(string name, int height, bool isTerrainMapped, GeographicBoundingBox boundary,
		   double levelZeroTileSize, int levels, int textureSize, string serverURL, string dataSetName, string imageExtension,
		   byte opacity, WorldWindow worldWindow, IBuilder parent)
         :base(name, worldWindow, parent)
		{
			distAboveSurface = height;
			terrainMapped = isTerrainMapped;
			m_hBoundary = boundary;
			m_bOpacity = opacity;
         m_strWorldName = worldWindow.CurrentWorld.Name;
			m_iLevels = levels;
			m_iTextureSizePixels = textureSize;
			m_dLevelZeroTileSizeDegrees = levelZeroTileSize;
			m_strServerUrl = serverURL;
			m_strDatasetName = dataSetName;
			m_strImageExt = imageExtension;
      }

      #endregion

      #region ImageBuilder Implementations

      public override GeographicBoundingBox Extents
      {
         get { return m_hBoundary; }
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
         get { return "tile"; }
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
         return GetQuadLayer();
      }

      public override string GetURI()
      {
         string formatString = "{0}?datasetname={1}&name={2}&height={3}&north={4}&east={5}&south={6}&west={7}&size={8}&levels={9}&lvl0tilesize={10}&terrainMapped={11}&imgfileext={12}";

         return string.Format(CultureInfo.InvariantCulture, formatString,
            m_strServerUrl.Replace("http://", URLProtocolName),
            m_strDatasetName,
            System.Web.HttpUtility.UrlEncode(m_szTreeNodeText),
            distAboveSurface.ToString(),
            m_hBoundary.North,
            m_hBoundary.East,
            m_hBoundary.South,
            m_hBoundary.West,
            m_iTextureSizePixels,
            m_iLevels,
            LevelZeroTileSize,
            terrainMapped,
            m_strImageExt);
      }

      public override string GetCachePath()
      {
         return String.Format("{0}{1}{4}{1}{2}{1}{3}", m_strCacheRoot, Path.DirectorySeparatorChar, CacheSubDir, GetBuilderPathString(this), m_strWorldName);
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
         return new NltQuadLayerBuilder(m_szTreeNodeText, distAboveSurface, terrainMapped, m_hBoundary, m_dLevelZeroTileSizeDegrees, m_iLevels, m_iTextureSizePixels, m_strServerUrl, m_strDatasetName, m_strImageExt, Opacity, m_oWorldWindow, m_Parent);
      }

      public override bool Equals(object obj)
      {
         if (!(obj is NltQuadLayerBuilder)) return false;
         NltQuadLayerBuilder castObj = obj as NltQuadLayerBuilder;

         // -- Equal if they're the same dataset from the same server --
         return this.m_strServerUrl.Equals(castObj.m_strServerUrl) && this.m_strDatasetName.Equals(castObj.m_strDatasetName);
      }

      #endregion

      #region Other Public Members

      public double LevelZeroTileSize
      {
         get
         {
            return m_dLevelZeroTileSizeDegrees;
         }
      }

      public int Levels
      {
         get { return m_iLevels; }
      }

      public String Server
      {
         get { return m_strServerUrl; }
      }

      public String DatasetName
      {
         get { return m_strDatasetName; }
      }

      #endregion

      #region Private Members

      private static string getRenderablePathString(RenderableObject renderable)
      {
         if (renderable.ParentList == null)
         {
            return renderable.Name;
         }
         else
         {
            return getRenderablePathString(renderable.ParentList) + Path.DirectorySeparatorChar + renderable.Name;
         }
      }

      private static string GetBuilderPathString(IBuilder builder)
      {
         if (builder.Parent == null)
         {
            string strRet = builder.Name;
            foreach (char cInvalid in Path.GetInvalidPathChars())
               strRet = strRet.Replace(cInvalid, '_');
            return strRet;
         }
         else
         {
            return GetBuilderPathString(builder.Parent) + Path.DirectorySeparatorChar + builder.Name;
         }
      }

      private QuadTileSet GetQuadLayer()
      {
         if (m_blnIsChanged)
         {
            NltImageStore[] imageStores = new NltImageStore[1];
            imageStores[0] = new NltImageStore(m_strDatasetName, m_strServerUrl);
            imageStores[0].DataDirectory = null;
            imageStores[0].LevelZeroTileSizeDegrees = LevelZeroTileSize;
            imageStores[0].LevelCount = m_iLevels;
            imageStores[0].ImageExtension = m_strImageExt;
            imageStores[0].CacheDirectory = GetCachePath();
            imageStores[0].TextureFormat = World.Settings.TextureFormat;
            imageStores[0].TextureSizePixels = m_iTextureSizePixels;

            m_oQuadTileSet = new QuadTileSet(m_szTreeNodeText,
               m_oWorldWindow.CurrentWorld,
               distAboveSurface,
               90, -90, -180, 180, terrainMapped, imageStores);
            m_oQuadTileSet.IsOn = m_IsOn;
            m_oQuadTileSet.Opacity = m_bOpacity;


            m_oQuadTileSet.AlwaysRenderBaseTiles = true;
            m_oQuadTileSet.IsOn = m_IsOn;
            m_oQuadTileSet.Opacity = m_bOpacity;
            m_blnIsChanged = false;
         }
         return m_oQuadTileSet;
      }

      #endregion

      internal void SaveToXml(XmlElement oServerElement)
      {
         oServerElement.SetAttribute("name", Name);
         oServerElement.SetAttribute("url", m_strServerUrl);
         oServerElement.SetAttribute("dataset", m_strDatasetName);
         oServerElement.SetAttribute("image_extension", m_strImageExt);

         XmlElement oBoundsElement = oServerElement.OwnerDocument.CreateElement("bounding_box");
         oBoundsElement.SetAttribute("minx", m_hBoundary.West.ToString());
         oBoundsElement.SetAttribute("miny", m_hBoundary.South.ToString());
         oBoundsElement.SetAttribute("maxx", m_hBoundary.East.ToString());
         oBoundsElement.SetAttribute("maxy", m_hBoundary.North.ToString());

         XmlElement oResElement = oServerElement.OwnerDocument.CreateElement("resolution");
         oResElement.SetAttribute("level_zero_tilesize", LevelZeroTileSize.ToString());
         oResElement.SetAttribute("levels", Levels.ToString());
         oResElement.SetAttribute("tile_size", m_iTextureSizePixels.ToString());
      }

      public override void GetOMMetadata(out String szDownloadType, out String szServerURL, out String szLayerId)
      {
         szDownloadType = "tile";
         szServerURL = m_strServerUrl;
         szLayerId = m_strDatasetName;
      }
   }
}
