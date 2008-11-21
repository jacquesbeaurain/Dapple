using System;
using System.Threading;
using Dapple.LayerGeneration;
using WorldWind.Net;
using WorldWind;
using System.Collections.Generic;
using System.Windows.Forms;

namespace NewServerTree
{
	public class WMSRootModelNode : ModelNode, IContextModelNode
	{
		#region Constructors

		public WMSRootModelNode(DappleModel oModel)
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

		public override bool ShowAllChildren
		{
			get { return UseShowAllChildren; }
		}

		public override String DisplayText
		{
			get
			{
				ModelNode[] cache = FilteredChildren;
				return String.Format("WMS Servers [{0} server{1}]", cache.Length, cache.Length != 1 ? "s" : String.Empty);
			}
		}

		public override string IconKey
		{
			get { return IconKeys.WMSRoot; }
		}

		public ToolStripMenuItem[] MenuItems
		{
			get
			{
				return new ToolStripMenuItem[] {
					new ToolStripMenuItem("Add WMS Server...", null, new EventHandler(c_miAddWMSServer_Click))
				};
			}
		}

		#endregion


		#region Public Methods

		public WMSServerModelNode GetServer(WMSServerUri oUri)
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

		public WMSServerModelNode AddServer(WMSServerUri oUri, bool blEnabled)
		{
			WMSServerModelNode result = new WMSServerModelNode(m_oModel, oUri, blEnabled);
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
		public static int SortWMSChildNodes(ModelNode first, ModelNode second)
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

		public void SaveToView(dappleview.builderdirectoryType oWMSDir)
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


	public class WMSServerModelNode : ServerModelNode, IFilterableModelNode
	{
		#region Member Variables

		private WMSServerUri m_oUri;
		private String m_strTitle;

		#endregion


		#region Constructors

		public WMSServerModelNode(DappleModel oModel, WMSServerUri oUri, bool blEnabled)
			: base(oModel, blEnabled)
		{
			m_oUri = oUri;
			m_strTitle = oUri.ServerTreeDisplayName;
		}

		#endregion


		#region Properties

		public override string DisplayText
		{
			get
			{
				if (LoadState == LoadState.LoadSuccessful)
				{
					return String.Format("{0} [{1} datasets]", m_strTitle, FilteredChildCount);
				}
				else
				{
					return m_strTitle;
				}
			}
		}

		public override ServerUri Uri
		{
			get { return m_oUri; }
		}

		public override ServerModelNode.ServerType Type
		{
			get { return ServerType.WMS; }
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
			get
			{
				return LoadState != LoadState.LoadSuccessful || FilteredChildCount > 0;
			}
		}

		#endregion


		#region Helper Methods

		protected override ModelNode[] Load()
		{
			String strCapFilename = @"c:\c\wms" + Parent.GetIndex(this) + ".xml";

			WebDownload oCatalogDownload = new WebDownload(m_oUri.ToCapabilitiesUri(), true);
			oCatalogDownload.DownloadFile(strCapFilename);

			WMSList oCatalog = new WMSList(m_oUri.ToCapabilitiesUri(), strCapFilename);
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


	public class WMSFolderModelNode : ModelNode, IFilterableModelNode
	{
		#region Member Variables

		private WMSLayer m_oData;

		#endregion


		#region Constructors

		public WMSFolderModelNode(DappleModel oModel, WMSLayer oData)
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

		public override string DisplayText
		{
			get { return m_oData.Title; }
		}

		public override string IconKey
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
				return FilteredChildCount > 0;
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


	public class WMSLayerModelNode : LayerModelNode, IContextModelNode, IFilterableModelNode
	{
		#region Member Variables

		private WMSLayer m_oData;
		private GeographicBoundingBox m_oBounds;

		#endregion


		#region Constructors

		public WMSLayerModelNode(DappleModel oModel, WMSLayer oData)
			: base(oModel)
		{
			m_oData = oData;
			m_oBounds = new GeographicBoundingBox((double)m_oData.North, (double)m_oData.South, (double)m_oData.West, (double)m_oData.East);

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
			get { return m_oData.Title; }
		}

		public override string IconKey
		{
			get { return IconKeys.WMSLayer; }
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

		#endregion


		#region Helper Methods

		protected override ModelNode[] Load()
		{
			throw new ApplicationException(ErrLoadedLeafNode);
		}

		#endregion
	}
}
