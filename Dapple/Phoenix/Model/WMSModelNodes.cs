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
		public WMSRootModelNode(DappleModel oModel)
			: base(oModel)
		{
			MarkLoaded();
		}

		public override ModelNode[] Load()
		{
			throw new ApplicationException(ErrLoadedBadNode);
		}

		public WMSServerModelNode AddServer(WMSServerUri oUri)
		{
			WMSServerModelNode result = new WMSServerModelNode(m_oModel, oUri);
			result.BeginLoad();
			AddChild(result);
			return result;
		}

		public override bool LoadSynchronously
		{
			get
			{
				return false;
			}
		}

		public override String DisplayText
		{
			get
			{
				return "WMS Servers";
			}
		}

		public override bool ShowAllChildren
		{
			get
			{
				return UseShowAllChildren;
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

		protected void c_miAddWMSServer_Click(object sender, EventArgs e)
		{
			throw new NotImplementedException();
		}
	}

	public class WMSServerModelNode : ServerModelNode
	{
		private WMSServerUri m_oUri;
		private String m_strTitle;

		public WMSServerModelNode(DappleModel oModel, WMSServerUri oUri)
			: base(oModel)
		{
			m_oUri = oUri;
			m_strTitle = oUri.ToBaseUri();
		}

		public override ModelNode[] Load()
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

		public override bool LoadSynchronously
		{
			get
			{
				return false;
			}
		}

		public override string DisplayText
		{
			get
			{
				return m_strTitle;
			}
		}

		public override bool Enabled
		{
			get { return true; }
			set { throw new NotImplementedException(); }
		}

		public override bool Favourite
		{
			get { return false; }
			set { throw new NotImplementedException(); }
		}

		public WMSServerUri Uri
		{
			get { return m_oUri; }
		}
	}

	public class WMSFolderModelNode : ModelNode
	{
		private WMSLayer m_oData;

		public WMSFolderModelNode(DappleModel oModel, WMSLayer oData)
			: base(oModel)
		{
			m_oData = oData;
		}

		public override ModelNode[] Load()
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

		public override bool LoadSynchronously
		{
			get
			{
				return true;
			}
		}

		public override string DisplayText
		{
			get
			{
				return m_oData.Title;
			}
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
	}

	public class WMSLayerModelNode : LayerModelNode, IContextModelNode
	{
		private WMSLayer m_oData;

		public WMSLayerModelNode(DappleModel oModel, WMSLayer oData)
			: base(oModel)
		{
			m_oData = oData;

			MarkLoaded();
		}

		public override ModelNode[] Load()
		{
			throw new NotImplementedException(ErrLoadedLeafNode);
		}

		public override string DisplayText
		{
			get
			{
				return m_oData.Title;
			}
		}

		public override bool IsLeaf
		{
			get
			{
				return true;
			}
		}

		public override string IconKey
		{
			get { return IconKeys.WMSLayer; }
		}
	}
}
