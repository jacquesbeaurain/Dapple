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
			throw new NotImplementedException();
		}

		#endregion


		#region Properties

		public override bool ShowAllChildren
		{
			get { return UseShowAllChildren; }
		}

		public override String DisplayText
		{
			get { return "WMS Servers"; }
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

		public WMSServerModelNode AddServer(WMSServerUri oUri)
		{
			WMSServerModelNode result = new WMSServerModelNode(m_oModel, oUri);
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


	public class WMSServerModelNode : ServerModelNode
	{
		#region Member Variables

		private WMSServerUri m_oUri;
		private String m_strTitle;

		#endregion


		#region Constructors

		public WMSServerModelNode(DappleModel oModel, WMSServerUri oUri)
			: base(oModel)
		{
			m_oUri = oUri;
			m_strTitle = oUri.ToBaseUri();
		}

		#endregion


		#region Public Methods

		public override string DisplayText
		{
			get { return m_strTitle; }
		}

		public WMSServerUri Uri
		{
			get { return m_oUri; }
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

			foreach (WMSLayer oLayer in oCatalog.Layers)
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

			return result.ToArray();
		}

		#endregion
	}


	public class WMSFolderModelNode : ModelNode
	{
		#region Member Variables

		private WMSLayer m_oData;

		#endregion


		#region Constructors

		public WMSFolderModelNode(DappleModel oModel, WMSLayer oData)
			: base(oModel)
		{
			m_oData = oData;
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

		#endregion


		#region Helper Methods

		protected override ModelNode[] Load()
		{
			List<ModelNode> result = new List<ModelNode>();

			foreach (WMSLayer oLayer in m_oData.ChildLayers)
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

			return result.ToArray();
		}

		#endregion
	}


	public class WMSLayerModelNode : LayerModelNode, IContextModelNode
	{
		#region Member Variables

		private WMSLayer m_oData;

		#endregion


		#region Constructors

		public WMSLayerModelNode(DappleModel oModel, WMSLayer oData)
			: base(oModel)
		{
			m_oData = oData;

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

		#endregion


		#region Helper Methods

		protected override ModelNode[] Load()
		{
			throw new NotImplementedException(ErrLoadedLeafNode);
		}

		#endregion
	}
}
