using System;
using Dapple.LayerGeneration;
using System.Collections.Generic;
using System.Xml;
using System.Globalization;
using WorldWind;
using System.Windows.Forms;

namespace NewServerTree
{
	public class ArcIMSRootModelNode : ModelNode, IContextModelNode
	{
		#region Constructors

		public ArcIMSRootModelNode(DappleModel oModel)
			: base(oModel)
		{
			MarkLoaded();
		}

		#endregion


		#region Event Handlers

		protected void c_miAddArcIMSServer_Click(object sender, EventArgs e)
		{
			throw new NotImplementedException();
		}

		#endregion


		#region Properties

		public override bool ShowAllChildren
		{
			get { return UseShowAllChildren; }
		}

		public override string DisplayText
		{
			get { return "ArcIMS Servers"; }
		}

		public override string IconKey
		{
			get { return IconKeys.ArcIMSRoot; }
		}

		public ToolStripMenuItem[] MenuItems
		{
			get
			{
				return new ToolStripMenuItem[] {
					new ToolStripMenuItem("Add ArcIMS Server...", null, new EventHandler(c_miAddArcIMSServer_Click))
				};
			}
		}

		#endregion


		#region Public Methods

		public ArcIMSServerModelNode AddServer(ArcIMSServerUri oUri)
		{
			ArcIMSServerModelNode result = new ArcIMSServerModelNode(m_oModel, oUri);
			result.BeginLoad();
			AddChild(result);
			return result;
		}

		#endregion


		#region Helper Methods

		protected override ModelNode[] Load()
		{
			throw new ApplicationException(ErrLoadedBadNode);
		}

		#endregion
	}


	public class ArcIMSServerModelNode : ServerModelNode
	{
		#region Member Variables

		private ArcIMSServerUri m_oUri;

		#endregion


		#region Constructors

		public ArcIMSServerModelNode(DappleModel oModel, ArcIMSServerUri oUri)
			: base(oModel)
		{
			m_oUri = oUri;
		}

		#endregion


		#region Properties

		public override string DisplayText
		{
			get { return m_oUri.ToBaseUri(); }
		}

		public ArcIMSServerUri Uri
		{
			get { return m_oUri; }
		}

		#endregion


		#region Helper Methods

		protected override ModelNode[] Load()
		{
			String strCapFilename = @"c:\c\arcims" + Parent.GetIndex(this) + ".xml";

			ArcIMSCatalogDownload oCatalogDownload = new ArcIMSCatalogDownload(m_oUri, 0);
			oCatalogDownload.DownloadFile(strCapFilename);

			XmlDocument oCatalogXML = new XmlDocument();
			oCatalogXML.Load(strCapFilename);

			XmlNodeList oServiceList = oCatalogXML.SelectNodes("/ARCXML/RESPONSE/SERVICES/SERVICE[@type=\"ImageServer\" and @access=\"PUBLIC\" and @status=\"ENABLED\"]");

			List<ModelNode> result = new List<ModelNode>();

			foreach (XmlNode oServiceNode in oServiceList)
			{
				String strServiceName = ((XmlElement)oServiceNode).GetAttribute("name");
				XmlElement oLocaleNode = oServiceNode.SelectSingleNode("ENVIRONMENT/LOCALE") as XmlElement;
				CultureInfo oCultureInfo = CultureInfo.CurrentCulture;
				if (oLocaleNode != null && oLocaleNode.HasAttribute("language") && oLocaleNode.HasAttribute("country"))
				{
					String strLanguage = oLocaleNode.GetAttribute("language");
					String strCountry = oLocaleNode.GetAttribute("country");
					try
					{
						oCultureInfo = new CultureInfo(String.Format("{0}-{1}", strLanguage.ToLowerInvariant(), strCountry.ToUpperInvariant()));
					}
					catch (ArgumentException)
					{
						try
						{
							// --- A server returned en-IT, which Windows didn't understand.  If we can't process the
							// --- language and country code, use the neutral language instead
							oCultureInfo = new CultureInfo(strLanguage.ToLowerInvariant());
						}
						catch (ArgumentException)
						{
							// --- If they sent back something truly bizarre, then just use invariant.  If it doesn't work,
							// --- then that's the server's problem, not ours.
							oCultureInfo = CultureInfo.InvariantCulture;
						}
					}

				}

				result.Add(new ArcIMSServiceModelNode(m_oModel, strServiceName, oCultureInfo));
			}

			return result.ToArray();
		}

		#endregion
	}


	public class ArcIMSServiceModelNode : ModelNode
	{
		#region Member Variables

		private CultureInfo m_oCultureInfo;
		private String m_strServiceName;

		#endregion


		#region Constructors

		public ArcIMSServiceModelNode(DappleModel oModel, String strServiceName, CultureInfo oCultureInfo)
			: base(oModel)
		{
			m_strServiceName = strServiceName;
			m_oCultureInfo = oCultureInfo;
		}

		#endregion


		#region Properties

		public override string DisplayText
		{
			get { return m_strServiceName; }
		}

		public override string IconKey
		{
			get { return IconKeys.ArcIMSService; }
		}

		#endregion


		#region Helper Methods

		protected override ModelNode[] Load()
		{
			String strServiceFilename = @"c:\c\arcims" + Parent.Parent.GetIndex(Parent) + "-" + m_strServiceName + ".xml";

			ArcIMSServiceDownload oServiceDownload = new ArcIMSServiceDownload((Parent as ArcIMSServerModelNode).Uri, m_strServiceName, 0);
			oServiceDownload.DownloadFile(strServiceFilename);


			// --- Parse the XML document downloaded ---

			XmlDocument oServiceXML = new XmlDocument();
			oServiceXML.Load(strServiceFilename);


			bool blRecognizedCRS = false;

			XmlElement oFeatureCoordSys = oServiceXML.SelectSingleNode("/ARCXML/RESPONSE/SERVICEINFO/PROPERTIES/FEATURECOORDSYS") as XmlElement;
			if (oFeatureCoordSys != null)
			{
				String szCRSID = "EPSG:" + oFeatureCoordSys.GetAttribute("id");
				blRecognizedCRS = Utility.GCSMappings.WMSWGS84Equivalents.Contains(szCRSID);
			}

			GeographicBoundingBox oServiceBounds = new GeographicBoundingBox();

			if (blRecognizedCRS)
			{
				XmlElement oServiceEnvelope = oServiceXML.SelectSingleNode("/ARCXML/RESPONSE/SERVICEINFO/PROPERTIES/ENVELOPE") as XmlElement;
				if (oServiceEnvelope != null)
				{
					GeographicBoundingBox oRealServiceBounds = new GeographicBoundingBox();
					bool blValid = true;
					blValid &= Double.TryParse(oServiceEnvelope.GetAttribute("minx"), NumberStyles.Any, m_oCultureInfo, out oRealServiceBounds.West);
					blValid &= Double.TryParse(oServiceEnvelope.GetAttribute("miny"), NumberStyles.Any, m_oCultureInfo, out oRealServiceBounds.South);
					blValid &= Double.TryParse(oServiceEnvelope.GetAttribute("maxx"), NumberStyles.Any, m_oCultureInfo, out oRealServiceBounds.East);
					blValid &= Double.TryParse(oServiceEnvelope.GetAttribute("maxy"), NumberStyles.Any, m_oCultureInfo, out oRealServiceBounds.North);

					if (blValid)
						oServiceBounds = oRealServiceBounds;
				}
			}


			List<ModelNode> result = new List<ModelNode>();

			XmlNodeList oNodeList = oServiceXML.SelectNodes("/ARCXML/RESPONSE/SERVICEINFO/LAYERINFO");
			foreach (XmlElement nLayerElement in oNodeList)
			{
				String szID = nLayerElement.GetAttribute("id");
				String szTitle = nLayerElement.GetAttribute("name");
				if (String.IsNullOrEmpty(szTitle)) szTitle = "LayerID " + szID;

				String szMinScale = nLayerElement.GetAttribute("minscale");
				double dMinScale = ArcIMSQuadLayerBuilder.DefaultMinScale;
				if (!String.IsNullOrEmpty(szMinScale))
				{
					Double.TryParse(szMinScale, NumberStyles.Any, CultureInfo.InvariantCulture, out dMinScale);
				}

				String szMaxScale = nLayerElement.GetAttribute("maxscale");
				double dMaxScale = ArcIMSQuadLayerBuilder.DefaultMaxScale;
				if (!String.IsNullOrEmpty(szMaxScale))
				{
					Double.TryParse(szMaxScale, NumberStyles.Any, CultureInfo.InvariantCulture, out dMaxScale);
				}

				if (dMaxScale > dMinScale || dMinScale > 1.0)
				{
					// --- If the server sends back weird values, ignore them ---
					dMaxScale = ArcIMSQuadLayerBuilder.DefaultMaxScale;
					dMinScale = ArcIMSQuadLayerBuilder.DefaultMinScale;
				}

				GeographicBoundingBox oLayerBounds = oServiceBounds.Clone() as GeographicBoundingBox;

				if (blRecognizedCRS)
				{
					XmlElement oLayerEnvelope = null;
					if (nLayerElement.GetAttribute("type").Equals("image"))
						oLayerEnvelope = nLayerElement.SelectSingleNode("ENVELOPE") as XmlElement;
					if (nLayerElement.GetAttribute("type").Equals("featureclass"))
						oLayerEnvelope = nLayerElement.SelectSingleNode("FCLASS/ENVELOPE") as XmlElement;
					if (oLayerEnvelope != null)
					{
						GeographicBoundingBox oRealLayerBounds = new GeographicBoundingBox();
						bool blValid = true;
						blValid &= Double.TryParse(oLayerEnvelope.GetAttribute("minx"), NumberStyles.Any, m_oCultureInfo, out oRealLayerBounds.West);
						blValid &= Double.TryParse(oLayerEnvelope.GetAttribute("miny"), NumberStyles.Any, m_oCultureInfo, out oRealLayerBounds.South);
						blValid &= Double.TryParse(oLayerEnvelope.GetAttribute("maxx"), NumberStyles.Any, m_oCultureInfo, out oRealLayerBounds.East);
						blValid &= Double.TryParse(oLayerEnvelope.GetAttribute("maxy"), NumberStyles.Any, m_oCultureInfo, out oRealLayerBounds.North);
						if (blValid)
							oLayerBounds = oRealLayerBounds;
					}
				}

				result.Add(new ArcIMSLayerModelNode(m_oModel, szTitle, szID, oLayerBounds, dMinScale, dMaxScale, m_oCultureInfo));
			}

			return result.ToArray();
		}

		#endregion
	}


	public class ArcIMSLayerModelNode : LayerModelNode
	{
		#region Member Variables

		private String m_strTitle, m_strID;
		private GeographicBoundingBox m_oBounds;
		private double m_dMinScale, m_dMaxScale;
		private CultureInfo m_oCultureInfo;

		#endregion


		#region Constructors

		public ArcIMSLayerModelNode(DappleModel oModel, String strTitle, String strID, GeographicBoundingBox oBounds, double dMinScale, double dMaxScale, CultureInfo oCultureInfo)
			: base(oModel)
		{
			m_strTitle = strTitle;
			m_strID = strID;
			m_oBounds = oBounds;
			m_dMinScale = dMinScale;
			m_dMaxScale = dMaxScale;
			m_oCultureInfo = oCultureInfo;
		}

		#endregion


		#region Properties

		public override bool IsLeaf
		{
			get { return true; }
		}

		public override string DisplayText
		{
			get { return m_strTitle; }
		}

		public override string IconKey
		{
			get { return IconKeys.ArcIMSLayer; }
		}

		#endregion


		#region Helper Methods

		protected override ModelNode[] Load()
		{
			throw new ApplicationException(ErrLoadedLeafNode);
		}

		#endregion
	}
}
