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
			m_oModel.AddArcIMSServer();
		}

		#endregion


		#region Properties

		public override bool ShowAllChildren
		{
			get { return UseShowAllChildren; }
		}


		public override String DisplayText
		{
			get
			{
				ModelNode[] cache = FilteredChildren;
				return String.Format("ArcIMS Servers [{0} server{1}]", cache.Length, cache.Length != 1 ? "s" : String.Empty);
			}
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
					new ToolStripMenuItem("Add ArcIMS Server...", IconKeys.ImageList.Images[IconKeys.AddArcIMSServerMenuItem], new EventHandler(c_miAddArcIMSServer_Click))
				};
			}
		}

		#endregion


		#region Public Methods

		public ArcIMSServerModelNode GetServer(ArcIMSServerUri oUri)
		{
			foreach (ArcIMSServerModelNode oServer in UnfilteredChildren)
			{
				if (oServer.Uri.Equals(oUri))
				{
					return oServer;
				}
			}

			return null;
		}

		public ArcIMSServerModelNode AddServer(ArcIMSServerUri oUri, bool blEnabled)
		{
			ArcIMSServerModelNode result = new ArcIMSServerModelNode(m_oModel, oUri, blEnabled);
			AddChild(result);
			if (blEnabled)
			{
				result.BeginLoad();
			}
			return result;
		}

		public ServerModelNode SetFavouriteServer(String strUri)
		{
			ServerModelNode result = null;

			foreach (ArcIMSServerModelNode oServer in UnfilteredChildren)
			{
				if (oServer.UpdateFavouriteStatus(strUri))
				{
					result = oServer;
				}
			}

			return result;
		}

		public void SaveToView(dappleview.builderdirectoryType oArcDir)
		{
			foreach (ArcIMSServerModelNode oChild in UnfilteredChildren)
			{
				dappleview.builderentryType oChildEntry = oArcDir.Newbuilderentry();
				dappleview.arcimscatalogType oChildCatalog = oChildEntry.Newarcimscatalog();

				oChildCatalog.Addcapabilitiesurl(new Altova.Types.SchemaString(oChild.Uri.ToBaseUri()));
				oChildCatalog.Addenabled(new Altova.Types.SchemaBoolean(oChild.Enabled));

				oChildEntry.Addarcimscatalog(oChildCatalog);
				oArcDir.Addbuilderentry(oChildEntry);
			}
		}

		#endregion


		#region Helper Methods

		protected override ModelNode[] Load()
		{
			throw new ApplicationException(ErrLoadedBadNode);
		}

		#endregion
	}


	public class ArcIMSServerModelNode : ServerModelNode, IFilterableModelNode
	{
		#region Member Variables

		private ArcIMSServerUri m_oUri;

		#endregion


		#region Constructors

		public ArcIMSServerModelNode(DappleModel oModel, ArcIMSServerUri oUri, bool blEnabled)
			: base(oModel, blEnabled)
		{
			m_oUri = oUri;
		}

		#endregion


		#region Properties

		public override string DisplayText
		{
			get { return m_oUri.ServerTreeDisplayName; }
		}

		public override ServerUri Uri
		{
			get { return m_oUri; }
		}

		public override ServerModelNode.ServerType Type
		{
			get { return ServerType.ArcIMS; }
		}

		public int FilteredChildCount
		{
			get
			{
				int result = 0;

				foreach (ModelNode oChild in FilteredChildren)
				{
					if (oChild is IFilterableModelNode)
					{
						result += (oChild as IFilterableModelNode).FilteredChildCount;
					}
				}

				return result;
			}
		}

		public bool PassesFilter
		{
			get
			{
				// --- Don't remove ArcIMS servers that are still loading their services ---

				foreach (ModelNode oChild in UnfilteredChildren)
				{
					if (oChild.LoadState == LoadState.Unloaded || oChild.LoadState == LoadState.Loading)
					{
						return true;
					}
				}

				return FilteredChildCount > 0;
			}
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

			result.Sort();

			return result.ToArray();
		}

		#endregion
	}


	public class ArcIMSServiceModelNode : ModelNode, IFilterableModelNode
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

		public int FilteredChildCount
		{
			get
			{
				if (LoadState != LoadState.LoadSuccessful)
				{
					return 0;
				}

				int result = 0;

				foreach (ModelNode oChild in FilteredChildren)
				{
					if (oChild is IFilterableModelNode)
					{
						result += (oChild as IFilterableModelNode).FilteredChildCount;
					}
				}

				return result;
			}
		}

		public bool PassesFilter
		{
			get { return LoadState != LoadState.LoadSuccessful || FilteredChildCount > 0; }
		}

		public String ServiceName
		{
			get { return m_strServiceName; }
		}

		#endregion


		#region Helper Methods

		protected override ModelNode[] Load()
		{
			String strServiceFilename = @"c:\c\arcims" + Parent.Parent.GetIndex(Parent) + "-" + m_strServiceName + ".xml";

			ArcIMSServiceDownload oServiceDownload = new ArcIMSServiceDownload((Parent as ServerModelNode).Uri as ArcIMSServerUri, m_strServiceName, 0);
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

			result.Sort();

			return result.ToArray();
		}

		#endregion
	}


	public class ArcIMSLayerModelNode : LayerModelNode, IFilterableModelNode
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

			MarkLoaded();
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

		public int FilteredChildCount
		{
			get
			{
				return PassesFilter ? 1 : 0;
			}
		}

		public bool PassesFilter
		{
			get
			{
				bool result = true;

				if (m_oModel.SearchBoundsSet)
				{
					result &= m_oBounds.Intersects(m_oModel.SearchBounds_Geo);
				}

				if (m_oModel.SearchKeywordSet)
				{
					result &= DisplayText.Contains(m_oModel.SearchKeyword);
				}

				return result;
			}
		}

		#endregion


		#region Public Methods

		[Obsolete("This should get removed with the rest of the LayerBuilder/ServerTree stuff")]
		public override LayerBuilder ConvertToLayerBuilder()
		{
			return new ArcIMSQuadLayerBuilder(
				(Parent.Parent as ArcIMSServerModelNode).Uri as ArcIMSServerUri,
				(Parent as ArcIMSServiceModelNode).ServiceName,
				m_strTitle,
				m_strID,
				m_oBounds,
				Dapple.MainForm.WorldWindowSingleton,
				null,
				null,
				m_dMinScale,
				m_dMaxScale,
				m_oCultureInfo
				);
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
