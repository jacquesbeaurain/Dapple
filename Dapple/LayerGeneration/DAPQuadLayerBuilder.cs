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
using System.Globalization;

namespace Dapple.LayerGeneration
{
   internal class DAPQuadLayerBuilder : LayerBuilder
	{
		#region Static

		internal static readonly string URISchemeName = "gxdap";
		internal static readonly string CacheSubDir = "DAPImages";
		internal static readonly string StylesheetCacheSubDir = "DAP Stylesheets";

      internal const int DAP_TILE_SIZE = 256;
      internal const double DAP_TILE_LZTS = 22.5;

		#endregion

      #region Member variables

      private QuadTileSet m_layer;
		internal int m_iHeight;
		internal int m_iTextureSizePixels;
		internal DataSet m_hDataSet;
		private Server m_oServer;
		private int m_iLevels;
		private double m_dLevelZeroTileSizeDegrees;
		private bool m_blUseXMLMeta = true;

      #endregion

		#region Constructors

		internal DAPQuadLayerBuilder(DataSet dataSet, WorldWindow worldWindow, Server server, IBuilder parent)
			:
		   this(dataSet, worldWindow,  server, parent, 0, 256, 0, 0)
		{
		}

		internal DAPQuadLayerBuilder(DataSet dataSet, WorldWindow worldWindow, Server server, IBuilder parent, int height, int size, double lvl0tilesize, int levels)
         :base(dataSet.Title, worldWindow, parent)
		{
			m_hDataSet = dataSet;
			m_oServer = server;

			m_iHeight = height;
			m_iTextureSizePixels = size;
			this.LevelZeroTileSize = lvl0tilesize;
         m_iLevels = levels;

			m_blUseXMLMeta = !String.IsNullOrEmpty(m_hDataSet.Stylesheet);
		}

		/// <summary>
		/// For DAP 11 and later servers, switch to using the tile interface, updating the texture
		/// info appropriately.
		/// </summary>
		private void SwitchToUseTiles()
		{
			if (m_oServer.MajorVersion >= 11)
			{
				m_iTextureSizePixels = DAP_TILE_SIZE;
				m_dLevelZeroTileSizeDegrees = DAP_TILE_LZTS;
				m_iLevels = DappleUtils.TileLevels(m_oServer.Command, m_hDataSet);
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
		public double LevelZeroTileSize
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

		[System.ComponentModel.Category("DAP")]
		[System.ComponentModel.Browsable(true)]
		[System.ComponentModel.Description("The location on your hard drive of this dataset, for datasets which are hosted on your Personal DAP server")]
		public string LocalFilename
		{
			get
			{
				if (IsFromPersonalDapServer)
				{
					try
					{
						String result;
						m_oServer.Command.GetDataSetFileName(m_hDataSet, out result);
						return result;
					}
					catch (Exception)
					{
						return "Error contacting Personal DAP server.";
					}
				}
				else
				{
					return "This dataset is from a remote server.";
				}
			}
		}

		[System.ComponentModel.Browsable(false)]
		public override bool IsChanged
		{
			get { return m_layer == null; }
		}

		[System.ComponentModel.Browsable(false)]
		internal override string ServerTypeIconKey
		{
			get
			{
				if (this.IsFromPersonalDapServer)
				{
					return "desktopcataloger";
				}
				else
				{
					return "dap";
				}
			}
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
				if (!m_blUseXMLMeta)
				{
					return "dap_dataset.xsl";
				}
				else
				{
					try
					{
						String strStylesheetCRC = m_oServer.StyleSheets[m_hDataSet.Stylesheet];
						String szFilename = Path.Combine(GetStyleSheetCachePath(), Server.GetStyleSheetFilename(m_hDataSet.Stylesheet, strStylesheetCRC));

						if (!File.Exists(Path.Combine(GetStyleSheetCachePath(), szFilename)))
						{
							Directory.CreateDirectory(Path.GetDirectoryName(szFilename));
							XmlDocument oStyleSheet = m_oServer.Command.GetStylesheet(m_hDataSet.Stylesheet);
							oStyleSheet.Save(szFilename);
						}

						return szFilename;
					}
					catch (Geosoft.Dap.DapException)
					{
						// --- Probably talking to an old server that doesn't understand this command, default to the ugly sheet ---
						return "dap_dataset.xsl";
					}
				}
			}
		}

		/// <summary>
		/// The name of the stylesheet, as found in the server's GetStylesheets response.
		/// </summary>
		internal String StyleSheetID
		{
			get
			{
				return m_hDataSet.Stylesheet;
			}
		}

		[System.ComponentModel.Browsable(false)]
		internal int ServerMajorVersion
		{
			get { return m_oServer.MajorVersion; }
		}

		[System.ComponentModel.Browsable(false)]
		internal override bool LayerFromSupportedServer
		{
			get { return true; }
		}

		[System.ComponentModel.Browsable(false)]
		internal bool IsFromPersonalDapServer
		{
			get { return m_oServer.IsPersonal; }
		}

		[System.ComponentModel.Browsable(false)]
		internal override bool ServerIsInHomeView
		{
			get
			{
				return NewServerTree.HomeView.ContainsServer(new DapServerUri(this.m_oServer.Url));
			}
		}

		#endregion

		#region ImageBuilder Implementations

      internal override bool bIsDownloading(out int iBytesRead, out int iTotalBytes)
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
      
      internal override RenderableObject GetLayer()
      {
         return GetQuadTileSet();
      }

      internal override string GetURI()
      {
         NameValueCollection queryColl = new NameValueCollection();

         queryColl.Add("datasetname", m_hDataSet.Name);
			queryColl.Add("height", m_iHeight.ToString(CultureInfo.InvariantCulture));
			queryColl.Add("size", m_iTextureSizePixels.ToString(CultureInfo.InvariantCulture));
         queryColl.Add("type", m_hDataSet.Type);
         queryColl.Add("title", m_hDataSet.Title);
         queryColl.Add("edition", m_hDataSet.Edition);
         queryColl.Add("hierarchy", m_hDataSet.Hierarchy);
         queryColl.Add("north", m_hDataSet.Boundary.MaxY.ToString(CultureInfo.InvariantCulture));
         queryColl.Add("east", m_hDataSet.Boundary.MaxX.ToString(CultureInfo.InvariantCulture));
         queryColl.Add("south", m_hDataSet.Boundary.MinY.ToString(CultureInfo.InvariantCulture));
         queryColl.Add("west", m_hDataSet.Boundary.MinX.ToString(CultureInfo.InvariantCulture));
			queryColl.Add("levels", m_iLevels.ToString(CultureInfo.InvariantCulture));
         queryColl.Add("lvl0tilesize", m_dLevelZeroTileSizeDegrees.ToString(CultureInfo.InvariantCulture));

         string strHost = "";
         string strPath = "";
         Utility.URI.ParseURI("http", m_oServer.Url, ref strHost, ref strPath);

         return Utility.URI.CreateURI(URISchemeName, strHost, strPath, queryColl);
      }

      internal override string GetCachePath()
      {
			return Path.Combine(Path.Combine(Path.Combine(s_strCacheRoot, CacheSubDir), Utility.FileSystem.SanitizeFilename(m_oServer.Url.Replace("http://", ""))), m_hDataSet.GetHashCode().ToString(CultureInfo.InvariantCulture));
      }

		private String GetStyleSheetCachePath()
		{
			return Path.Combine(Path.Combine(s_strCacheRoot, StylesheetCacheSubDir), Utility.FileSystem.SanitizeFilename(m_oServer.Url.Replace("http://", "")));
		}

      protected override void CleanUpLayer(bool bFinal)
      {
         if (m_layer != null)
            m_layer.Dispose();
         m_layer = null;
      }

      internal override object CloneSpecific()
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

		public override int GetHashCode()
		{
			return m_hDataSet.UniqueName.GetHashCode();
		}

      #endregion

      #region ImageBuilder Overrides

		public override XmlNode GetMetaData(XmlDocument oDoc)
		{
			if (m_blUseXMLMeta)
				{
					try
					{
						XmlDocument oMetaDoc = m_oServer.Command.GetXMLMetaData(m_hDataSet);

						// --- Remove all xml-stylesheet processing instructions in the metadata. This is
						// --- necessary in cases where someone catalogs extracted data, and there would
						// --- otherwise be two processing instructions, where the first one most likely
						// --- pointed to a file that can't be found

						for (int count = oMetaDoc.ChildNodes.Count - 1; count >= 0; count--)
						{
							if (oMetaDoc.ChildNodes[count].NodeType == XmlNodeType.ProcessingInstruction && oMetaDoc.ChildNodes[count].Name.Equals("xml-stylesheet"))
							{
								oMetaDoc.RemoveChild(oMetaDoc.ChildNodes[count]);
							}
						}

						return oMetaDoc;
					}
					catch (Geosoft.Dap.DapException)
					{
						// Dataset doesn't have XML, default to the ugly type of metadata.
						m_blUseXMLMeta = false;
					}
				}

				// --- Doctor the metadata slightly to fit the default stylesheet ---
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

		#region Public Methods

		public override string ToString()
		{
			return String.Format(CultureInfo.InvariantCulture, "DAPQuadLayerBuilder, Title=\"{0}\", UniqueName=\"{1}\"", m_hDataSet.Title, m_hDataSet.UniqueName);
		}
		#endregion


		#region Private Members

		private QuadTileSet GetQuadTileSet()
      {
         if (m_layer == null)
         {
				SwitchToUseTiles();

				string strCachePath = Path.Combine(GetCachePath(), LevelZeroTileSize.ToString(CultureInfo.InvariantCulture));
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
            imageStores[0] = new DAPImageStore(m_hDataSet, m_oServer)
                             	{
                             		DataDirectory = null,
                             		LevelZeroTileSizeDegrees = LevelZeroTileSize,
                             		LevelCount = m_iLevels,
                             		ImageExtension = ".png",
                             		CacheDirectory = strCachePath,
                             		TextureFormat = WorldSettings.TextureFormat,
                             		TextureSizePixels = m_iTextureSizePixels
                             	};

         	m_layer = new QuadTileSet(m_hDataSet.Title, m_oWorldWindow.CurrentWorld, 0, m_hDataSet.Boundary.MaxY, m_hDataSet.Boundary.MinY,
               m_hDataSet.Boundary.MinX, m_hDataSet.Boundary.MaxX, true, imageStores)
                      	{
                      		AlwaysRenderBaseTiles = true,
                      		IsOn = m_IsOn,
                      		Opacity = m_bOpacity
                      	};
         }
         return m_layer;
      }

      // Note: in practice this will never get called, as DAP layers use a separate metadata handling process.
      internal override void GetOMMetadata(out String szDownloadType, out String szServerURL, out String szLayerId)
      {
         szDownloadType = "dap";
         szServerURL = m_oServer.Url;
         szLayerId = m_hDataSet.Name;
      }

      #endregion
   }
}
