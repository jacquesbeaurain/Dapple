using System;
using System.Threading;
using Dapple.LayerGeneration;
using Geosoft.GX.DAPGetData;
using System.IO;
using System.Collections.Generic;
using Geosoft.Dap.Common;
using System.Windows.Forms;
using Geosoft.Dap;

namespace NewServerTree
{
	public class DapServerRootModelNode : ModelNode, IContextModelNode
	{
		public DapServerRootModelNode(DappleModel oModel)
			: base(oModel)
		{
			MarkLoaded();
		}

		public override ModelNode[] Load()
		{
			throw new ApplicationException(ErrLoadedBadNode);
		}

		public DapServerModelNode AddServer(DapServerUri oUri)
		{
			DapServerModelNode result = new DapServerModelNode(m_oModel, oUri);
			result.BeginLoad();
			AddChild(result);
			return result;
		}

		public override String DisplayText
		{
			get
			{
				return "DAP Servers";
			}
		}

		public override bool ShowAllChildren
		{
			get
			{
				return UseShowAllChildren;
			}
		}

		public ToolStripMenuItem[] MenuItems
		{
			get
			{
				return new ToolStripMenuItem[] {
					new ToolStripMenuItem("Add DAP Server...", null, new EventHandler(c_miAddDAPServer_Click))
				};
			}
		}

		protected void c_miAddDAPServer_Click(object sender, EventArgs e)
		{
			throw new NotImplementedException();
		}

		public override string IconKey
		{
			get { return IconKeys.DapRoot; }
		}
	}

	public class DapServerModelNode : ServerModelNode
	{
		public static CatalogCacheManager s_oCCM = new CatalogCacheManager(null, @"c:\c\ccm\");

		private DapServerUri m_oUri;
		private String m_strTitle;
		private Server m_oServer;
		private bool m_blEntireCatalogMode;
		private CatalogFolder m_oFolder;

		public DapServerModelNode(DappleModel oModel, DapServerUri oUri)
			: base(oModel)
		{
			m_oUri = oUri;
			m_strTitle = m_oUri.ToBaseUri();
		}

		public override ModelNode[] Load()
		{
			String strCacheDir = @"C:\c\dap\" + m_oUri.GetHashCode() + @"\";
			Directory.CreateDirectory(strCacheDir);
			m_oServer = new Server(m_oUri.ToBaseUri(), strCacheDir, String.Empty, true);

			if (m_oServer.Status != Server.ServerStatus.OnLine)
			{
				throw new DapException("Server is " + m_oServer.Status.ToString());
			}

			m_strTitle = m_oServer.Name;
			m_blEntireCatalogMode = m_oServer.MajorVersion < 6 || (m_oServer.MajorVersion == 6 && m_oServer.MinorVersion < 3);

			List<ModelNode> result = new List<ModelNode>();

			String strEdition;
			m_oFolder = s_oCCM.GetCatalogHierarchyRoot(m_oServer, null, false, false, null, out m_blEntireCatalogMode, out strEdition);
			foreach (CatalogFolder oSubFolder in m_oFolder.Folders)
			{
				result.Add(new DapDirectoryModelNode(m_oModel, oSubFolder));
			}

			while (!DapServerModelNode.s_oCCM.bGetDatasetList(m_oServer, m_oFolder.Hierarchy, m_oFolder.Timestamp, null, false, false, null)) { }

			FolderDatasetList oDatasets = DapServerModelNode.s_oCCM.GetDatasets(m_oServer, m_oFolder, null, false, false, null);

			foreach (DataSet oDataset in oDatasets.Datasets)
			{
				result.Add(new DapDatasetModelNode(m_oModel, oDataset));
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

		public DapServerUri Uri
		{
			get { return m_oUri; }
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

		public Server Server
		{
			get { return m_oServer; }
		}
	}

	public class PersonalDapServerModelNode : DapServerModelNode
	{
		public PersonalDapServerModelNode(DappleModel oModel)
			: base(oModel, new DapServerUri("http://localhost:10205/"))
		{
		}

		public override string DisplayText
		{
			get
			{
				return "Whatever Title Marketing Decides to Use for Personal Dap";
			}
		}

		public override string IconKey
		{
			get
			{
				return IconKeys.PersonalDAPServer;
			}
		}
	}

	public class DapDirectoryModelNode : ModelNode
	{
		private CatalogFolder m_oFolder;

		public DapDirectoryModelNode(DappleModel oModel, CatalogFolder oFolder)
			: base(oModel)
		{
			m_oFolder = oFolder;

			foreach (CatalogFolder oSubFolder in m_oFolder.Folders)
			{
				AddChildSilently(new DapDirectoryModelNode(m_oModel, oSubFolder));
			}
		}

		public override ModelNode[] Load()
		{
			while (!DapServerModelNode.s_oCCM.bGetDatasetList(GetServer(), m_oFolder.Hierarchy, m_oFolder.Timestamp, null, false, false, null)) { }

			FolderDatasetList oDatasets = DapServerModelNode.s_oCCM.GetDatasets(GetServer(), m_oFolder, null, false, false, null);

			List<ModelNode> result = new List<ModelNode>();

			foreach (DataSet oDataset in oDatasets.Datasets)
			{
				result.Add(new DapDatasetModelNode(m_oModel, oDataset));
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
				return m_oFolder.Name;
			}
		}

		private Server GetServer()
		{
			ModelNode oServerNode = this;
			while (oServerNode != null && !(oServerNode is DapServerModelNode))
			{
				oServerNode = oServerNode.Parent;
			}

			if (oServerNode == null) throw new ApplicationException("Orphaned DAP folder node");

			return (oServerNode as DapServerModelNode).Server;
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

	public class DapDatasetModelNode : LayerModelNode
	{
		DataSet m_oDataSet;

		public DapDatasetModelNode(DappleModel oModel, DataSet oDataSet)
			: base(oModel)
		{
			m_oDataSet = oDataSet;

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
				return m_oDataSet.Title;
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
			get { return IconKeys.DapLayerPrefix + m_oDataSet.Type.ToLowerInvariant(); }
		}
	}
}