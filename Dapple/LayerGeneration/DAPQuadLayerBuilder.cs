using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Xml;
using System.Xml.XPath;

using WorldWind;
using WorldWind.Renderable;

using Geosoft;
using Geosoft.Dap.Common;
using Geosoft.GX.DAPGetData;

using Dapple.DAP;

namespace Dapple.LayerGeneration
{
   public class DAPQuadLayerBuilder : LayerBuilder
	{
		#region Static

		public static readonly string URISchemeName = "gxdap";
		public static readonly string CacheSubDir = "DAPImages";

      public const int DAP_TILE_SIZE = 256;
      public const double DAP_TILE_LZTS = 22.5;

		#endregion

      #region Member variables

      private QuadTileSet m_layer;
		public int m_iHeight;
		public int m_iTextureSizePixels;
		public DataSet m_hDataSet;
		private Server m_oServer;
		private int m_iLevels;
		private double m_dLevelZeroTileSizeDegrees;

      #endregion

		#region Constructors

		public DAPQuadLayerBuilder(DataSet dataSet, WorldWindow worldWindow, Server server, IBuilder parent)
			:
		   this(dataSet, worldWindow,  server, parent, 0, 256, 0, 0)
		{
		}

		public DAPQuadLayerBuilder(DataSet dataSet, WorldWindow worldWindow, Server server, IBuilder parent, int height, int size, double lvl0tilesize, int levels)
         :base(dataSet.Title, worldWindow, parent)
		{
			m_hDataSet = dataSet;
			m_oServer = server;

			m_iHeight = height;
			m_iTextureSizePixels = size;
			this.LevelZeroTileSize = lvl0tilesize;
         m_iLevels = levels;

         if (server.MajorVersion >= 11)
         {
            m_iTextureSizePixels = DAP_TILE_SIZE;
            m_dLevelZeroTileSizeDegrees = DAP_TILE_LZTS;
				m_iLevels = DappleUtils.TileLevels(m_oServer.Command, dataSet);
         }
		}

		#endregion

		#region Properties

		[System.ComponentModel.Category("Dapple")]
		[System.ComponentModel.Browsable(true)]
		[System.ComponentModel.Description("Whether Dapple can display metadata for this data layer")]
		public override bool SupportsMetaData
		{
			get
			{
				return true;
			}
		}

		[System.ComponentModel.Category("Dapple")]
		[System.ComponentModel.Browsable(true)]
		[System.ComponentModel.Description("The opacity of the image (255 = opaque, 0 = transparent)")]
		public override byte Opacity
		{
			get
			{
				if (m_layer != null)
					return m_layer.Opacity;
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
				if (m_layer != null && m_layer.Opacity != value)
				{
					m_layer.Opacity = value;
					bChanged = true;
				}
				if (bChanged)
					SendBuilderChanged(BuilderChangeType.OpacityChanged);
			}
		}

		[System.ComponentModel.Category("Dapple")]
		[System.ComponentModel.Browsable(true)]
		[System.ComponentModel.Description("Whether this data layer is visible on the globe")]
		public override bool Visible
		{
			get
			{
				if (m_layer != null)
					return m_layer.IsOn;
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
				if (m_layer != null && m_layer.IsOn != value)
				{
					m_layer.IsOn = value;
					bChanged = true;
				}

				if (bChanged)
					SendBuilderChanged(BuilderChangeType.VisibilityChanged);
			}
		}

		[System.ComponentModel.Category("Dapple")]
		[System.ComponentModel.Browsable(true)]
		[System.ComponentModel.ReadOnly(true)]
		[System.ComponentModel.Description("The tile size, in degrees, of the topmost level")]
		private double LevelZeroTileSize
		{
			get
			{
				if (m_dLevelZeroTileSizeDegrees == 0)
					// Shared code in DappleUtils of dapxmlclient
					m_dLevelZeroTileSizeDegrees = DappleUtils.LevelZeroTileSize(m_hDataSet);
				return m_dLevelZeroTileSizeDegrees;
			}
			set
			{
				m_dLevelZeroTileSizeDegrees = value;
			}
		}

		[System.ComponentModel.Category("Common")]
		[System.ComponentModel.Browsable(true)]
		[System.ComponentModel.Description("The extents of this data layer, in WGS 84")]
		public override GeographicBoundingBox Extents
		{
			get
			{
				if (m_layer != null)
				{
					return new GeographicBoundingBox(m_layer.North,
				m_layer.South,
				m_layer.West,
					 m_layer.East);
				}
				return new GeographicBoundingBox(m_hDataSet.Boundary.MaxY,
					m_hDataSet.Boundary.MinY,
					m_hDataSet.Boundary.MinX,
					m_hDataSet.Boundary.MaxX);
			}
		}

		[System.ComponentModel.Category("Common")]
		[System.ComponentModel.Browsable(true)]
		[System.ComponentModel.Description("The server providing this data layer")]
		public string ServerURL
		{
			get
			{
				return m_hDataSet.Url;
			}
		}

		[System.ComponentModel.Category("DAP")]
		[System.ComponentModel.Browsable(true)]
		[System.ComponentModel.Description("The dataset name used to access this data layer on the DAP server")]
		public string DatasetName
		{
			get
			{
				return m_hDataSet.Name;
			}
		}

		[System.ComponentModel.Category("DAP")]
		[System.ComponentModel.Browsable(true)]
		[System.ComponentModel.Description("The DAP type of this data layer")]
		public string DAPType
		{
			get
			{
				return m_hDataSet.Type;
			}
		}

		[System.ComponentModel.Browsable(false)]
		public override bool IsChanged
		{
			get { return m_layer == null; }
		}

		[System.ComponentModel.Browsable(false)]
		public override string ServerTypeIconKey
		{
			get { return "dap"; }
		}

		[System.ComponentModel.Browsable(false)]
		public override string DisplayIconKey
		{
			get { return "dap_" + m_hDataSet.Type.ToLower(); }
		}

		[System.ComponentModel.Browsable(false)]
		public override string StyleSheetName
		{
			get
			{
				return "dap_dataset.xsl";
			}
		}

		[System.ComponentModel.Browsable(false)]
		public int ServerMajorVersion
		{
			get
			{
				return m_oServer.MajorVersion;
			}
		}

		#endregion

		#region ImageBuilder Implementations

      public override bool bIsDownloading(out int iBytesRead, out int iTotalBytes)
      {
         if (m_layer != null)
            return m_layer.bIsDownloading(out iBytesRead, out iTotalBytes);
         else
         {
            iBytesRead = 0;
            iTotalBytes = 0;
            return false;
         }
      }
      
      public override RenderableObject GetLayer()
      {
         return GetQuadTileSet();
      }

      public override string GetURI()
      {
         NameValueCollection queryColl = new NameValueCollection();

         queryColl.Add("datasetname", m_hDataSet.Name);
         queryColl.Add("height", m_iHeight.ToString());
         queryColl.Add("size", m_iTextureSizePixels.ToString());
         queryColl.Add("type", m_hDataSet.Type);
         queryColl.Add("title", m_hDataSet.Title);
         queryColl.Add("edition", m_hDataSet.Edition);
         queryColl.Add("hierarchy", m_hDataSet.Hierarchy);
         queryColl.Add("north", m_hDataSet.Boundary.MaxY.ToString(System.Globalization.CultureInfo.InvariantCulture));
         queryColl.Add("east", m_hDataSet.Boundary.MaxX.ToString(System.Globalization.CultureInfo.InvariantCulture));
         queryColl.Add("south", m_hDataSet.Boundary.MinY.ToString(System.Globalization.CultureInfo.InvariantCulture));
         queryColl.Add("west", m_hDataSet.Boundary.MinX.ToString(System.Globalization.CultureInfo.InvariantCulture));
         queryColl.Add("levels", m_iLevels.ToString());
         queryColl.Add("lvl0tilesize", m_dLevelZeroTileSizeDegrees.ToString(System.Globalization.CultureInfo.InvariantCulture));

         string strHost = "";
         string strPath = "";
         Utility.URI.ParseURI("http", m_oServer.Url, ref strHost, ref strPath);

         return Utility.URI.CreateURI(URISchemeName, strHost, strPath, queryColl);
      }

      public override string GetCachePath()
      {
         return Path.Combine(Path.Combine(Path.Combine(m_strCacheRoot, CacheSubDir), m_oServer.Url.Replace("http://", "")), m_hDataSet.GetHashCode().ToString());
      }

      protected override void CleanUpLayer(bool bFinal)
      {
         if (m_layer != null)
            m_layer.Dispose();
         m_layer = null;
      }

      public override object CloneSpecific()
      {
         DataSet hDSCopy = new DataSet();
         hDSCopy.Edition = m_hDataSet.Edition;
         hDSCopy.Boundary = m_hDataSet.Boundary;
         hDSCopy.Hierarchy = m_hDataSet.Hierarchy;
         hDSCopy.Name = m_hDataSet.Name;
         hDSCopy.Title = m_hDataSet.Title;
         hDSCopy.Type = m_hDataSet.Type;
         hDSCopy.Url = m_hDataSet.Url;

         return new DAPQuadLayerBuilder(hDSCopy, m_oWorldWindow, m_oServer, m_Parent, m_iHeight, m_iTextureSizePixels, this.LevelZeroTileSize, m_iLevels);
      }

      public override bool Equals(object obj)
      {
         if (!(obj is DAPQuadLayerBuilder)) return false;
         DAPQuadLayerBuilder castObj = obj as DAPQuadLayerBuilder;

         // -- Equal if their unique names are --
         return this.m_hDataSet.UniqueName.Equals(castObj.m_hDataSet.UniqueName);
      }

      #endregion

      #region ImageBuilder Overrides

		public override XmlNode GetMetaData(XmlDocument oDoc)
		{
			XmlDocument responseDoc = m_oServer.Command.GetMetaData(m_hDataSet, null);
			XmlNode oNode = responseDoc.DocumentElement.FirstChild.FirstChild.FirstChild;
			if (oNode == null) return null;
			XmlNode metaNode = oDoc.CreateElement("dapmeta");
			XmlNode nameNode = oDoc.CreateElement("name");
			nameNode.InnerText = Title;
			metaNode.AppendChild(nameNode);
			XmlNode geoMetaNode = oDoc.CreateElement(oNode.Name);
			geoMetaNode.InnerXml = oNode.InnerXml;
			metaNode.AppendChild(geoMetaNode);
			return metaNode;
		}

		#endregion

      #region Private Members

      private QuadTileSet GetQuadTileSet()
      {
         if (m_layer == null)
         {
            string strCachePath = Path.Combine(GetCachePath(), LevelZeroTileSize.ToString());
            System.IO.Directory.CreateDirectory(strCachePath);

            // Determine the needed levels (function of tile size and resolution if available)
            // Shared code in DappleUtils of dapxmlclient
            try
            {
               if (m_iLevels == 0)
                  m_iLevels = DappleUtils.Levels(m_oServer.Command, m_hDataSet);
            }
            catch
            {
               m_iLevels = 15;
            }

            ImageStore[] imageStores = new ImageStore[1];
            imageStores[0] = new DAPImageStore(m_hDataSet, m_oServer);
            imageStores[0].DataDirectory = null;
            imageStores[0].LevelZeroTileSizeDegrees = LevelZeroTileSize;
            imageStores[0].LevelCount = m_iLevels;
            imageStores[0].ImageExtension = ".png";
            imageStores[0].CacheDirectory = strCachePath;
            imageStores[0].TextureFormat = World.Settings.TextureFormat;
            imageStores[0].TextureSizePixels = m_iTextureSizePixels;

            GeographicBoundingBox box = new WorldWind.GeographicBoundingBox(m_hDataSet.Boundary.MaxY,
               m_hDataSet.Boundary.MinY, m_hDataSet.Boundary.MinX, m_hDataSet.Boundary.MaxX);

            m_layer = new QuadTileSet(m_hDataSet.Title, m_oWorldWindow.CurrentWorld, 0, m_hDataSet.Boundary.MaxY, m_hDataSet.Boundary.MinY,
               m_hDataSet.Boundary.MinX, m_hDataSet.Boundary.MaxX, true, imageStores);
            m_layer.AlwaysRenderBaseTiles = true;
            m_layer.IsOn = m_IsOn;
            m_layer.Opacity = m_bOpacity;
         }
         return m_layer;
      }

      // Note: in practice this will never get called, as DAP layers use a separate metadata handling process.
      public override void GetOMMetadata(out String szDownloadType, out String szServerURL, out String szLayerId)
      {
         szDownloadType = "dap";
         szServerURL = m_oServer.Url;
         szLayerId = m_hDataSet.Name;
      }

      #endregion
   }
}
