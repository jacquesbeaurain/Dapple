using System;
using Dapple.LayerGeneration;
using System.Collections.Generic;
using System.Xml;
using System.Globalization;
using WorldWind;
using System.Windows.Forms;
using System.ComponentModel;
using System.IO;

namespace NewServerTree
{
	internal class ArcIMSRootModelNode : ModelNode, IContextModelNode
	{
		#region Constructors

		internal ArcIMSRootModelNode(DappleModel oModel)
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

		[Browsable(false)]
		internal override bool ShowAllChildren
		{
			get { return UseShowAllChildren; }
		}


		internal override String DisplayText
		{
			get { return "ArcIMS Servers"; }
		}

		internal override string Annotation
		{
			get { return String.Format(CultureInfo.InvariantCulture, "[{0}]", FilteredChildren.Length); }
		}

		[Browsable(false)]
		internal override string IconKey
		{
			get { return IconKeys.ArcIMSRoot; }
		}

		[Browsable(false)]
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

		internal ArcIMSServerModelNode GetServer(ArcIMSServerUri oUri)
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

		internal ArcIMSServerModelNode AddServer(ArcIMSServerUri oUri, bool blEnabled)
		{
			ArcIMSServerModelNode result = new ArcIMSServerModelNode(m_oModel, oUri, blEnabled);
			AddChild(result);
			if (blEnabled)
			{
				result.BeginLoad();
			}
			return result;
		}

		internal ServerModelNode SetFavouriteServer(String strUri)
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

		internal void SaveToView(dappleview.builderdirectoryType oArcDir)
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


	internal class ArcIMSServerModelNode : ServerModelNode, IFilterableModelNode
	{
		#region Member Variables

		private ArcIMSServerUri m_oUri;

		#endregion


		#region Constructors

		internal ArcIMSServerModelNode(DappleModel oModel, ArcIMSServerUri oUri, bool blEnabled)
			: base(oModel, blEnabled)
		{
			m_oUri = oUri;
		}

		#endregion


		#region Properties

		internal override string DisplayText
		{
			get { return m_oUri.ServerTreeDisplayName; }
		}

		internal override string Annotation
		{
			get
			{
				switch (LoadState)
				{
					case LoadState.LoadSuccessful:
						{
							int cache = FilteredChildCount;
							return String.Format(CultureInfo.InvariantCulture, "[{0} dataset{1}]", cache, cache != 1 ? "s" : String.Empty);
						}
					case LoadState.Loading:
						{
							return "[Loading...]";
						}
					case LoadState.LoadFailed:
						{
							return "[Unable to contact server]";
						}
					case LoadState.Unloaded:
						{
							return String.Empty;
						}
					default:
						throw new ApplicationException("Missing enum case statement");
				}
			}
		}

		[Browsable(false)]
		internal override string ServerTypeIconKey
		{
			get { return IconKeys.ArcIMSRoot; }
		}

		[Browsable(true)]
		[Category("Server")]
		[Description("The URI for this server.")]
		public override ServerUri Uri
		{
			get { return m_oUri; }
		}

		[Browsable(true)]
		[Category("Server")]
		[Description("What type of server (DAP, WMS, ArcIMS) this server is.")]
		public override ServerModelNode.ServerType Type
		{
			get { return ServerType.ArcIMS; }
		}

		[Browsable(false)]
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

		[Browsable(false)]
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

		internal String CapabilitiesFilename
		{
			get
			{
				return Path.Combine(WorldWind.PluginEngine.MainApplication.Settings.CachePath, Path.Combine("ArcIMS Catalog Cache", Path.Combine(m_oUri.ToCacheDirectory(), "serviceinfo.xml")));
			}
		}

		#endregion


		#region Public Methods

		[Obsolete("This should get removed with the rest of the LayerBuilder/ServerTree stuff")]
		internal override List<LayerBuilder> GetBuildersInternal()
		{
			return new List<LayerBuilder>();
		}

		internal override void AddToHomeView()
		{
			if (!HomeView.ContainsServer(m_oUri))
			{
				HomeView.AddServer(m_oUri);
			}
		}

		#endregion


		#region Helper Methods

		protected override ModelNode[] Load()
		{
			String strCapFilename = CapabilitiesFilename;

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
				CultureInfo oCultureInfo = CultureInfo.InvariantCulture;
				if (oLocaleNode != null && oLocaleNode.HasAttribute("language") && oLocaleNode.HasAttribute("country"))
				{
					String strLanguage = oLocaleNode.GetAttribute("language");
					String strCountry = oLocaleNode.GetAttribute("country");
					try
					{
						oCultureInfo = new CultureInfo(strLanguage.ToLowerInvariant() + '-' + strCountry.ToUpperInvariant());
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

					// --- We can't parse strings with neutral cultures, so if we get one, then default
					// --- to the invariant culture.
					if (oCultureInfo.IsNeutralCulture)
					{
						oCultureInfo = CultureInfo.InvariantCulture;
					}
				}

				result.Add(new ArcIMSServiceModelNode(m_oModel, strServiceName, oCultureInfo));
			}

			result.Sort();

			return result.ToArray();
		}

		protected override void OnLoadCompleted()
		{
			if (LoadState != LoadState.LoadSuccessful)
				return;

			foreach (ModelNode child in UnfilteredChildren)
				if (child is ArcIMSServiceModelNode)
					child.BeginLoad();
		}

		#endregion
	}


	internal class ArcIMSServiceModelNode : ModelNode, IFilterableModelNode
	{
		#region Member Variables

		private CultureInfo m_oCultureInfo;
		private String m_strServiceName;

		#endregion


		#region Constructors

		internal ArcIMSServiceModelNode(DappleModel oModel, String strServiceName, CultureInfo oCultureInfo)
			: base(oModel)
		{
			m_strServiceName = strServiceName;
			m_oCultureInfo = oCultureInfo;
		}

		#endregion


		#region Properties

		internal override string DisplayText
		{
			get { return m_strServiceName; }
		}

		[Browsable(false)]
		internal override string IconKey
		{
			get
			{
				if (LoadState == LoadState.Loading)
					return IconKeys.ArcIMSServiceLoading;
				else if (LoadState == LoadState.LoadFailed)
					return IconKeys.ArcIMSServiceLoadFailed;
				else
					return IconKeys.ArcIMSService;
			}
		}

		[Browsable(false)]
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

		[Browsable(false)]
		public bool PassesFilter
		{
			get { return LoadState != LoadState.LoadSuccessful || FilteredChildCount > 0; }
		}

		internal String ServiceName
		{
			get { return m_strServiceName; }
		}

		internal String CapabilitiesFilename
		{
			get
			{
				return Path.Combine(WorldWind.PluginEngine.MainApplication.Settings.CachePath, Path.Combine("ArcIMS Catalog Cache", Path.Combine(((this.Parent as ArcIMSServerModelNode).Uri as ArcIMSServerUri).ToCacheDirectory(), m_strServiceName.GetHashCode().ToString("X8", CultureInfo.InvariantCulture) + ".xml")));
			}
		}

		#endregion


		#region Helper Methods

		protected override ModelNode[] Load()
		{
			String strServiceFilename = CapabilitiesFilename;

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


	internal class ArcIMSLayerModelNode : LayerModelNode, IFilterableModelNode
	{
		#region Member Variables

		private String m_strTitle, m_strID;
		private GeographicBoundingBox m_oBounds;
		private double m_dMinScale, m_dMaxScale;
		private CultureInfo m_oCultureInfo;

		#endregion


		#region Constructors

		internal ArcIMSLayerModelNode(DappleModel oModel, String strTitle, String strID, GeographicBoundingBox oBounds, double dMinScale, double dMaxScale, CultureInfo oCultureInfo)
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

		[Browsable(false)]
		internal override bool IsLeaf
		{
			get { return true; }
		}

		internal override string DisplayText
		{
			get { return m_strTitle; }
		}

		[Browsable(false)]
		internal override string IconKey
		{
			get { return IconKeys.ArcIMSLayer; }
		}

		[Browsable(false)]
		public int FilteredChildCount
		{
			get
			{
				return PassesFilter ? 1 : 0;
			}
		}

		[Browsable(false)]
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
					result &= DisplayText.ToLowerInvariant().Contains(m_oModel.SearchKeyword.ToLowerInvariant());
				}

				return result;
			}
		}

		#endregion


		#region Public Methods

		[Obsolete("This should get removed with the rest of the LayerBuilder/ServerTree stuff")]
		internal override LayerBuilder ConvertToLayerBuilder()
		{
			return new ArcIMSQuadLayerBuilder(
				(Parent.Parent as ArcIMSServerModelNode).Uri as ArcIMSServerUri,
				(Parent as ArcIMSServiceModelNode).ServiceName,
				m_strTitle,
				m_strID,
				m_oBounds,
				Dapple.MainForm.WorldWindowSingleton,
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
