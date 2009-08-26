using System;
using System.Collections.Generic;
using System.Text;
using WorldWind.Renderable;
using WorldWind;
using System.IO;
using WorldWind.PluginEngine;
using System.Globalization;
using System.Xml;

namespace Dapple.LayerGeneration
{
	class ArcIMSQuadLayerBuilder : LayerBuilder
	{
		#region Constants

		private const double LevelZeroTileSize = 22.5;
		private const int TileSize = 256;

		#endregion

		#region Member variables

		private String m_szServiceName;
		private QuadTileSet m_oQuadTileSet = null;
		private bool m_blnIsChanged = true;
		private GeographicBoundingBox m_oEnvelope;
		private GeographicBoundingBox m_oUnprojectedEnvelope;
		private ArcIMSServerUri m_oServerUri;
		private int m_iLevels;
		private String m_szLayerID;
		private double m_dMinScale, m_dMaxScale;
		private CultureInfo m_oCultureInfo;
		private ArcIMSFeatureCoordSys m_oProjection;

		#endregion

		#region Static

		internal static readonly string URLProtocolName = "gxarcims://";
		internal static readonly string CacheSubDir = "ArcIMSImages";
		internal static readonly double DefaultMinScale = 0.0;
		internal static readonly double DefaultMaxScale = 1.0;

		#endregion

		#region Constructor

		internal ArcIMSQuadLayerBuilder(ArcIMSServerUri oServerUri, String strServiceName, String szLayerTitle, String szLayerID, GeographicBoundingBox oEnvelope, ArcIMSFeatureCoordSys oProjection, WorldWindow oWorldWindow, IBuilder oParent, double dMinScale, double dMaxScale, CultureInfo oInfo)
			: base(szLayerTitle, oWorldWindow, oParent)
		{
			m_oServerUri = oServerUri;
			m_oCultureInfo = oInfo;
			m_oUnprojectedEnvelope = oEnvelope;
			m_oProjection = oProjection;
			m_szLayerID = szLayerID;
			m_szServiceName = strServiceName;
			m_dMinScale = dMinScale;
			if (m_dMinScale < DefaultMinScale)
				m_dMinScale = DefaultMinScale;
			m_dMaxScale = dMaxScale;
			if (m_dMaxScale > DefaultMaxScale)
				m_dMaxScale = DefaultMaxScale;
			if (m_dMaxScale < m_dMinScale)
			{
				// --- Weird scale values, ignore them and hope for the best ---
				m_dMaxScale = DefaultMaxScale;
				m_dMinScale = DefaultMinScale;
			}

			CalculateLevels();
		}

		private void CalculateLevels()
		{
			m_iLevels = 1;
			double iter = LevelZeroTileSize;
			while (iter / (TileSize * Math.Pow(2, m_iLevels)) > m_dMinScale)
			{
				m_iLevels += 1;
				iter /= 2;
			}
			m_iLevels += 2;
		}

		#endregion

		#region Properties

		[System.ComponentModel.Category("Dapple")]
		[System.ComponentModel.Browsable(true)]
		[System.ComponentModel.Description("The opacity of the image (255 = opaque, 0 = transparent)")]
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

		[System.ComponentModel.Category("Dapple")]
		[System.ComponentModel.Browsable(true)]
		[System.ComponentModel.Description("Whether this data layer is visible on the globe")]
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

		[System.ComponentModel.Category("Common")]
		[System.ComponentModel.Browsable(true)]
		[System.ComponentModel.Description("The extents of this data layer, in WGS 84")]
		public override GeographicBoundingBox Extents
		{
			get
			{
				if (m_oEnvelope == null)
					m_oEnvelope = m_oProjection.ReprojectToWGS84(m_oServerUri, m_szServiceName, m_oUnprojectedEnvelope);

				return m_oEnvelope;
			}
		}

		[System.ComponentModel.Category("Common")]
		[System.ComponentModel.Browsable(true)]
		[System.ComponentModel.Description("The server providing this data layer")]
		public string ServerURL
		{
			get { return m_oServerUri.ToBaseUri(); }
		}

		[System.ComponentModel.Browsable(false)]
		public override bool IsChanged
		{
			get { return m_blnIsChanged; }
		}

		[System.ComponentModel.Browsable(false)]
		internal override string ServerTypeIconKey
		{
			get { return MainForm.ArcImsIconKey; }
		}

		[System.ComponentModel.Browsable(false)]
		public override string DisplayIconKey
		{
			get { return MainForm.ArcImsIconKey; }
		}

		[System.ComponentModel.Browsable(false)]
		internal override bool LayerFromSupportedServer
		{
			get { return true; }
		}

		[System.ComponentModel.Browsable(false)]
		internal override bool ServerIsInHomeView
		{
			get
			{
				return NewServerTree.HomeView.ContainsServer(m_oServerUri);
			}
		}

		[System.ComponentModel.Category("ArcIMS")]
		[System.ComponentModel.Browsable(true)]
		[System.ComponentModel.Description("The minimum scale that this dataset is visible at")]
		public double MinScale
		{
			get { return m_dMinScale; }
		}

		[System.ComponentModel.Category("ArcIMS")]
		[System.ComponentModel.Browsable(true)]
		[System.ComponentModel.Description("The maximum scale that this dataset is visible at")]
		public double MaxScale
		{
			get { return m_dMaxScale; }
		}

		[System.ComponentModel.Category("ArcIMS")]
		[System.ComponentModel.Browsable(true)]
		[System.ComponentModel.Description("The name of the service on the server that serves this dataset")]
		public string ServiceName
		{
			get { return m_szServiceName; }
		}

		[System.ComponentModel.Category("ArcIMS")]
		[System.ComponentModel.Browsable(true)]
		[System.ComponentModel.Description("The unique identifier of this dataset in its associated service")]
		public string LayerID
		{
			get { return m_szLayerID; }
		}

		#endregion

		#region ImageBuilder Implementations

		internal override bool bIsDownloading(out int iBytesRead, out int iTotalBytes)
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

		internal override RenderableObject GetLayer()
		{
			if (m_blnIsChanged)
			{
				ImageStore[] aImageStore = new ImageStore[1];
				aImageStore[0] = new ArcIMSImageStore(m_szServiceName, m_szLayerID, m_oServerUri as ArcIMSServerUri, TileSize, m_oCultureInfo, m_dMinScale, m_dMaxScale);
				aImageStore[0].DataDirectory = null;
				aImageStore[0].LevelZeroTileSizeDegrees = LevelZeroTileSize;
				aImageStore[0].LevelCount = m_iLevels;
				aImageStore[0].ImageExtension = ".png";
				aImageStore[0].CacheDirectory = GetCachePath();

				m_oQuadTileSet = new QuadTileSet(m_szTreeNodeText, m_oWorldWindow.CurrentWorld, 0,
					Extents.North, Extents.South, Extents.West, Extents.East,
					true, aImageStore);
				m_oQuadTileSet.AlwaysRenderBaseTiles = true;
				m_oQuadTileSet.IsOn = m_IsOn;
				m_oQuadTileSet.Opacity = m_bOpacity;
				m_blnIsChanged = false;
			}
			return m_oQuadTileSet;
		}

		internal override string GetURI()
		{
			return m_oServerUri.ToBaseUri().Replace("http://", URLProtocolName)
				+ String.Format(System.Globalization.CultureInfo.InvariantCulture, "&minx={0}&miny={1}&maxx={2}&maxy={3}&minscale={4}&maxscale={5}&layerid={6}&title={7}&servicename={8}",
				Extents.West,
				Extents.South,
				Extents.East,
				Extents.North,
				m_dMinScale,
				m_dMaxScale,
				System.Web.HttpUtility.UrlEncode(m_szLayerID),
				System.Web.HttpUtility.UrlEncode(this.Title),
				System.Web.HttpUtility.UrlEncode(m_szServiceName));
		}

		private String LayerCacheFolder
		{
			get
			{
				return Path.Combine(m_szServiceName.GetHashCode().ToString("X8", CultureInfo.InvariantCulture),
					m_szLayerID.GetHashCode().ToString("X8", CultureInfo.InvariantCulture));
			}
		}

		internal override string GetCachePath()
		{
			return Path.Combine(Path.Combine(Path.Combine(s_strCacheRoot, CacheSubDir), m_oServerUri.ToCacheDirectory()), this.LayerCacheFolder);
		}

		protected override void CleanUpLayer(bool bFinal)
		{
			if (m_oQuadTileSet != null)
				m_oQuadTileSet.Dispose();
			m_oQuadTileSet = null;
			m_blnIsChanged = true;
		}

		internal override object CloneSpecific()
		{
			return new ArcIMSQuadLayerBuilder(m_oServerUri, m_szServiceName, this.m_szTreeNodeText, m_szLayerID, m_oUnprojectedEnvelope, m_oProjection, m_oWorldWindow, m_Parent, m_dMinScale, m_dMaxScale, m_oCultureInfo);
		}

		public override bool Equals(object obj)
		{
			if (!(obj is ArcIMSQuadLayerBuilder)) return false;
			ArcIMSQuadLayerBuilder castObj = obj as ArcIMSQuadLayerBuilder;

			// -- Equal if they're the same service from the same server --
			return m_oServerUri.Equals(castObj.m_oServerUri) && m_szServiceName.Equals(castObj.m_szServiceName) && m_szLayerID.Equals(castObj.m_szLayerID);
		}

		public override int GetHashCode()
		{
			return m_oServerUri.ToString().GetHashCode() ^ m_szServiceName.GetHashCode() ^ m_szLayerID.GetHashCode();
		}

		[System.ComponentModel.Browsable(false)]
		public override string MetadataDisplayMessage
		{
			get { return "The selected layer does not publish metadata."; }
		}

		#endregion

		#region Private Members

		internal override void GetOMMetadata(out String szDownloadType, out String szServerURL, out String szLayerId)
		{
			szDownloadType = "arcims";
			szServerURL = m_oServerUri.ToBaseUri();
			szLayerId = m_szLayerID;
		}

		#endregion

		#region Public Methods

		public override string ToString()
		{
			return "ArcIMSQuadLayerBuilder, Server=\"" + m_oServerUri.ToBaseUri() + "\", Service=\"" + m_szServiceName + "\", Layer=\"" + m_szLayerID + "\"";
		}

		#endregion
	}

	internal class ArcIMSFeatureCoordSys
	{
		private String idAttribute;
		private String stringAttribute;
		private bool defined;
		private CultureInfo culture;

		public ArcIMSFeatureCoordSys(CultureInfo culture)
		{
			this.culture = culture;
			defined = false;
		}

		public ArcIMSFeatureCoordSys(CultureInfo culture, System.Xml.XmlElement featureCoordSysElement)
		{
			this.culture = culture;
			idAttribute = featureCoordSysElement.GetAttribute("id");
			stringAttribute = featureCoordSysElement.GetAttribute("string");
			defined = !String.IsNullOrEmpty(idAttribute) || !String.IsNullOrEmpty(stringAttribute);
		}

		public bool IsWGS84
		{
			get
			{
				return defined && Utility.GCSMappings.WMSWGS84Equivalents.Contains(idAttribute);
			}
		}

		public GeographicBoundingBox ReprojectToWGS84(ArcIMSServerUri uri, String serviceName, GeographicBoundingBox source)
		{
			if (IsWGS84)
				return source;

			if (!defined)
				return new GeographicBoundingBox(89.1, -89.1, -180, 180);

			if (source == null)
				return new GeographicBoundingBox(89.2, -89.2, -180, 180);

			ArcIMSReprojectDownload download = new ArcIMSReprojectDownload(uri, 0, source, this, serviceName);
			try
			{
				download.DownloadMemory();
				XmlDocument response = new XmlDocument();
				response.Load(download.ContentStream);
				return ProcessResponse(response);
			}
			catch (Exception)
			{
				return new GeographicBoundingBox(89.3, -89.3, -180, 180);
			}
		}

		public XmlDocument MakeArcXmlRequest(GeographicBoundingBox source)
		{
			XmlDocument result = new XmlDocument();

			result.AppendChild(result.CreateXmlDeclaration("1.0", "UTF-8", null));

			XmlElement arcNode = result.CreateElement("ARCXML");
			arcNode.SetAttribute("version", "1.1");
			{
				XmlNode requestNode = result.CreateElement("REQUEST");
				{
					XmlElement getProjectElement = result.CreateElement("GET_PROJECT");
					getProjectElement.SetAttribute("envelope", "true");
					{
						XmlElement fromCoordSysElement = result.CreateElement("FROMCOORDSYS");
						if (!String.IsNullOrEmpty(stringAttribute))
							fromCoordSysElement.SetAttribute("string", stringAttribute);
						else
							fromCoordSysElement.SetAttribute("id", idAttribute);
						getProjectElement.AppendChild(fromCoordSysElement);

						XmlElement toCoordSysElement = result.CreateElement("TOCOORDSYS");
						toCoordSysElement.SetAttribute("id", "4326");
						getProjectElement.AppendChild(toCoordSysElement);

						XmlElement envelopeElement = result.CreateElement("ENVELOPE");
						envelopeElement.SetAttribute("minx", source.West.ToString(culture));
						envelopeElement.SetAttribute("miny", source.South.ToString(culture));
						envelopeElement.SetAttribute("maxx", source.East.ToString(culture));
						envelopeElement.SetAttribute("maxy", source.North.ToString(culture));
						getProjectElement.AppendChild(envelopeElement);
					}
					requestNode.AppendChild(getProjectElement);
				}
				arcNode.AppendChild(requestNode);
			}
			result.AppendChild(arcNode);

			return result;
		}

		private GeographicBoundingBox ProcessResponse(XmlDocument document)
		{
			XmlElement envelopeElement = document.SelectSingleNode("/ARCXML/RESPONSE/PROJECT/ENVELOPE") as XmlElement;
			if (envelopeElement != null)
			{
				GeographicBoundingBox result = new GeographicBoundingBox();

				bool success = true;
				success &= Double.TryParse(envelopeElement.GetAttribute("minx"), NumberStyles.Float, culture, out result.West);
				success &= Double.TryParse(envelopeElement.GetAttribute("miny"), NumberStyles.Float, culture, out result.South);
				success &= Double.TryParse(envelopeElement.GetAttribute("maxx"), NumberStyles.Float, culture, out result.East);
				success &= Double.TryParse(envelopeElement.GetAttribute("maxy"), NumberStyles.Float, culture, out result.North);

				if (success)
					return result;
			}

			return new GeographicBoundingBox(89.4, -89.4, -180, 180);
		}
	}
}
