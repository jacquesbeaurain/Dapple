using System;
using System.Threading;
using Dapple.LayerGeneration;
using WorldWind.Net;
using WorldWind;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.ComponentModel;

namespace NewServerTree
{
	internal class WMSRootModelNode : ModelNode, IContextModelNode
	{
		#region Constructors

		internal WMSRootModelNode(DappleModel oModel)
			: base(oModel)
		{
			MarkLoaded();
		}

		#endregion


		#region Event Handlers

		protected void c_miAddWMSServer_Click(object sender, EventArgs e)
		{
			m_oModel.AddWMSServer();
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
			get { return "WMS Servers"; }
		}

		internal override string Annotation
		{
			get { return String.Format("[{0}]", FilteredChildren.Length); }
		}

		[Browsable(false)]
		internal override string IconKey
		{
			get { return IconKeys.WMSRoot; }
		}

		[Browsable(false)]
		public ToolStripMenuItem[] MenuItems
		{
			get
			{
				return new ToolStripMenuItem[] {
					new ToolStripMenuItem("Add WMS Server...", IconKeys.ImageList.Images[IconKeys.AddWMSServerMenuItem], new EventHandler(c_miAddWMSServer_Click))
				};
			}
		}

		#endregion


		#region Public Methods

		internal WMSServerModelNode GetServer(WMSServerUri oUri)
		{
			foreach (WMSServerModelNode oServer in UnfilteredChildren)
			{
				if (oServer.Uri.Equals(oUri))
				{
					return oServer;
				}
			}

			return null;
		}

		internal WMSServerModelNode AddServer(WMSServerUri oUri, bool blEnabled)
		{
			WMSServerModelNode result = new WMSServerModelNode(m_oModel, oUri, blEnabled);
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

			foreach (WMSServerModelNode oServer in UnfilteredChildren)
			{
				if (oServer.UpdateFavouriteStatus(strUri))
				{
					result = oServer;
				}
			}

			return result;
		}

		/// <summary>
		/// Sorts WMS folders before WMS layers.
		/// </summary>
		/// <param name="first"></param>
		/// <param name="second"></param>
		/// <returns></returns>
		internal static int SortWMSChildNodes(ModelNode first, ModelNode second)
		{
			if (first is WMSFolderModelNode)
			{
				if (second is WMSLayerModelNode)
				{
					return -1;
				}
				else if (second is WMSFolderModelNode)
				{
					return first.CompareTo(second);
				}
				else
				{
					throw new ApplicationException("Unexpected model node '" + second.GetType().ToString() + "'in WMS subree");
				}
			}
			else if (first is WMSLayerModelNode)
			{
				if (second is WMSLayerModelNode)
				{
					return first.CompareTo(second);
				}
				else if (second is WMSFolderModelNode)
				{
					return 1;
				}
				else
				{
					throw new ApplicationException("Unexpected model node '" + second.GetType().ToString() + "'in WMS subree");
				}
			}
			else
			{
				throw new ApplicationException("Unexpected model node '" + first.GetType().ToString() + "'in WMS subree");
			}
		}

		#region Saving and Loading old Dapple Views

		internal void SaveToView(dappleview.builderdirectoryType oWMSDir)
		{
			foreach (WMSServerModelNode oChild in UnfilteredChildren)
			{
				dappleview.builderentryType oChildEntry = oWMSDir.Newbuilderentry();
				dappleview.wmscatalogType oChildCatalog = oChildEntry.Newwmscatalog();

				oChildCatalog.Addcapabilitiesurl(new Altova.Types.SchemaString(oChild.Uri.ToBaseUri()));
				oChildCatalog.Addenabled(new Altova.Types.SchemaBoolean(oChild.Enabled));

				oChildEntry.Addwmscatalog(oChildCatalog);
				oWMSDir.Addbuilderentry(oChildEntry);
			}
		}

		#endregion

		#endregion


		#region Helper Methods

		protected override ModelNode[] Load()
		{
			throw new ApplicationException(ErrLoadedBadNode);
		}

		#endregion
	}


	internal class WMSServerModelNode : ServerModelNode, IFilterableModelNode
	{
		#region Member Variables

		private WMSServerUri m_oUri;
		private String m_strTitle;

		#endregion


		#region Constructors

		internal WMSServerModelNode(DappleModel oModel, WMSServerUri oUri, bool blEnabled)
			: base(oModel, blEnabled)
		{
			m_oUri = oUri;
			m_strTitle = oUri.ServerTreeDisplayName;
		}

		#endregion


		#region Properties

		internal override string DisplayText
		{
			get
			{
				return m_strTitle;
			}
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
							return String.Format("[{0} dataset{1}]", cache, cache != 1 ? "s" : String.Empty);
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
			get { return IconKeys.WMSRoot; }
		}

		[Browsable(true)]
		[Category("Server")]
		[Description("The URI for this server.")]
		internal override ServerUri Uri
		{
			get { return m_oUri; }
		}

		[Browsable(true)]
		[Category("Server")]
		[Description("What type of server (DAP, WMS, ArcIMS) this server is.")]
		internal override ServerModelNode.ServerType Type
		{
			get { return ServerType.WMS; }
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
			get
			{
				return LoadState != LoadState.LoadSuccessful || FilteredChildCount > 0;
			}
		}

		internal String CapabilitiesFilename
		{
			get
			{
				return Path.Combine(WorldWind.PluginEngine.MainApplication.Settings.CachePath, Path.Combine("WMS Catalog Cache", Path.Combine(m_oUri.ToCacheDirectory(), "capabilities.xml")));
			}
		}

		#endregion


		#region Public Methods

		internal WMSLayerModelNode GetLayer(String strLayerName)
		{
			foreach (ModelNode oChild in UnfilteredChildren)
			{
				if (oChild is WMSLayerModelNode)
				{
					WMSLayerModelNode oLayer = oChild as WMSLayerModelNode;

					if (oLayer.LayerData.Name.Equals(strLayerName)) return oLayer;
				}
				if (oChild is WMSFolderModelNode)
				{
					WMSFolderModelNode oFolder = oChild as WMSFolderModelNode;
					WMSLayerModelNode oLayer = oFolder.GetLayer(strLayerName);
					if (oLayer != null) return oLayer;
				}
			}

			return null;
		}

		[Obsolete("This should get removed with the rest of the LayerBuilder/ServerTree stuff")]
		internal override List<LayerBuilder> GetBuildersInternal()
		{
			List<LayerBuilder> result = new List<LayerBuilder>();

			foreach (ModelNode oNode in FilteredChildren)
			{
				if (oNode is WMSFolderModelNode)
				{
					result.AddRange((oNode as WMSFolderModelNode).GetBuilders());
				}
				else if (oNode is WMSLayerModelNode)
				{
					result.Add((oNode as WMSLayerModelNode).ConvertToLayerBuilder());
				}
			}

			return result;
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
			WebDownload oCatalogDownload = new WebDownload(m_oUri.ToCapabilitiesUri(), true);
			oCatalogDownload.DownloadFile(CapabilitiesFilename);

			WMSList oCatalog = new WMSList(m_oUri.ToCapabilitiesUri(), CapabilitiesFilename);
			m_strTitle = oCatalog.Name;

			List<ModelNode> result = new List<ModelNode>();

			foreach (WMSLayer oLayer in oCatalog.Layers[0].ChildLayers)
			{
				if (oLayer.ChildLayers == null)
				{
					result.Add(new WMSLayerModelNode(m_oModel, oLayer));
				}
				else
				{
					result.Add(new WMSFolderModelNode(m_oModel, oLayer));
				}
			}

			result.Sort(new Comparison<ModelNode>(WMSRootModelNode.SortWMSChildNodes));

			return result.ToArray();
		}

		#endregion
	}


	internal class WMSFolderModelNode : ModelNode, IFilterableModelNode
	{
		#region Member Variables

		private WMSLayer m_oData;

		#endregion


		#region Constructors

		internal WMSFolderModelNode(DappleModel oModel, WMSLayer oData)
			: base(oModel)
		{
			m_oData = oData;

			foreach (WMSLayer oLayer in m_oData.ChildLayers)
			{
				if (oLayer.ChildLayers == null)
				{
					AddChildSilently(new WMSLayerModelNode(m_oModel, oLayer));
				}
				else
				{
					AddChildSilently(new WMSFolderModelNode(m_oModel, oLayer));
				}
			}

			MarkLoaded();
		}

		#endregion


		#region Properties

		internal override string DisplayText
		{
			get { return m_oData.Title; }
		}

		[Browsable(false)]
		internal override string IconKey
		{
			get
			{
				if (m_oModel.IsSelectedOrAncestor(this))
				{
					return IconKeys.OpenFolder;
				}
				else
				{
					return IconKeys.ClosedFolder;
				}
			}
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
				return FilteredChildCount > 0;
			}
		}

		#endregion


		#region Public Methods

		internal WMSLayerModelNode GetLayer(String strLayerName)
		{
			foreach (ModelNode oChild in UnfilteredChildren)
			{
				if (oChild is WMSLayerModelNode)
				{
					WMSLayerModelNode oLayer = oChild as WMSLayerModelNode;

					if (oLayer.LayerData.Name.Equals(strLayerName)) return oLayer;
				}
				if (oChild is WMSFolderModelNode)
				{
					WMSFolderModelNode oFolder = oChild as WMSFolderModelNode;
					WMSLayerModelNode oLayer = oFolder.GetLayer(strLayerName);
					if (oLayer != null) return oLayer;
				}
			}

			return null;
		}

		[Obsolete("This should get removed with the rest of the LayerBuilder/ServerTree stuff")]
		internal List<LayerBuilder> GetBuilders()
		{
			List<LayerBuilder> result = new List<LayerBuilder>();

			foreach (ModelNode oNode in FilteredChildren)
			{
				if (oNode is WMSFolderModelNode)
				{
					result.AddRange((oNode as WMSFolderModelNode).GetBuilders());
				}
				else if (oNode is WMSLayerModelNode)
				{
					result.Add((oNode as WMSLayerModelNode).ConvertToLayerBuilder());
				}
			}

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


	internal class WMSLayerModelNode : LayerModelNode, IContextModelNode, IFilterableModelNode
	{
		#region Member Variables

		private WMSLayer m_oData;
		private GeographicBoundingBox m_oBounds;

		private ToolStripMenuItem m_oViewLegend;

		#endregion


		#region Constructors

		internal WMSLayerModelNode(DappleModel oModel, WMSLayer oData)
			: base(oModel)
		{
			m_oData = oData;
			m_oBounds = new GeographicBoundingBox((double)m_oData.North, (double)m_oData.South, (double)m_oData.West, (double)m_oData.East);

			m_oViewLegend = new ToolStripMenuItem("View Legend...", IconKeys.ImageList.Images[IconKeys.ViewLegendMenuItem], new EventHandler(c_miViewLegend_Click));

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
			get { return m_oData.Title; }
		}

		[Browsable(false)]
		internal override string IconKey
		{
			get { return IconKeys.WMSLayer; }
		}

		public override ToolStripMenuItem[] MenuItems
		{
			get
			{
				ToolStripMenuItem[] baseItems = base.MenuItems;
				ToolStripMenuItem[] result = new ToolStripMenuItem[baseItems.Length + 1];

				Array.Copy(baseItems, result, baseItems.Length);

				result[result.Length - 1] = m_oViewLegend;
				m_oViewLegend.Enabled = m_oData.HasLegend;

				return result;
			}
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
				bool blResult = true;

				if (m_oModel.SearchKeywordSet)
				{
					blResult &= (m_oData.Description != null && m_oData.Description.Contains(m_oModel.SearchKeyword)) || (m_oData.Title != null && m_oData.Title.Contains(m_oModel.SearchKeyword));
				}

				if (m_oModel.SearchBoundsSet)
				{
					blResult &= m_oBounds.Intersects(m_oModel.SearchBounds_Geo);
				}

				return blResult;
			}
		}

		internal WMSLayer LayerData
		{
			get { return m_oData; }
		}

		#endregion


		#region Event Handlers

		private void c_miViewLegend_Click(object sender, EventArgs e)
		{
			List<String> oLegendUrls = new List<String>();

			foreach (WMSLayerStyle oStyle in m_oData.Styles)
			{
				if (oStyle.legendURL != null && oStyle.legendURL.Length > 0)
					foreach (WMSLayerStyleLegendURL oUrl in oStyle.legendURL)
					{
						oLegendUrls.Add(oUrl.href);
					}
			}

			foreach (String strLegendUrl in oLegendUrls)
			{
				Dapple.MainForm.BrowseTo(strLegendUrl);
			}
		}

		#endregion


		#region Public Methods

		[Obsolete("This should get removed with the rest of the LayerBuilder/ServerTree stuff")]
		internal override LayerBuilder ConvertToLayerBuilder()
		{
			WMSServerModelNode oSMN = GetServer();
			WMSServerBuilder oServer = new WMSServerBuilder(null, oSMN.Uri as WMSServerUri, oSMN.CapabilitiesFilename, true);
			return new WMSQuadLayerBuilder(m_oData, Dapple.MainForm.WorldWindowSingleton, oServer, null);
		}

		#endregion


		#region Helper Methods

		protected override ModelNode[] Load()
		{
			throw new ApplicationException(ErrLoadedLeafNode);
		}

		private WMSServerModelNode GetServer()
		{
			ModelNode iter = this;

			while (iter != null && !(iter is WMSServerModelNode))
			{
				iter = iter.Parent;
			}

			if (iter == null) throw new ApplicationException("Orphaned WMS layer node");

			return iter as WMSServerModelNode;
		}

		#endregion
	}
}
