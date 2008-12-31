using System;
using System.Threading;
using Dapple.LayerGeneration;
using Geosoft.GX.DAPGetData;
using System.IO;
using System.Collections.Generic;
using Geosoft.Dap.Common;
using System.Windows.Forms;
using Geosoft.Dap;
using System.ComponentModel;
namespace NewServerTree
{
	public class DapServerRootModelNode : ModelNode, IContextModelNode
	{
		#region Statics

		public static readonly string DAPSecureToken;

		static DapServerRootModelNode()
		{
			GeoSecureClient.CGeoSecureInterfaceClass tokenGenerator = new GeoSecureClient.CGeoSecureInterfaceClass();
			tokenGenerator.CreateSecureToken(out DAPSecureToken);
		}

		#endregion


		#region Constructors

		public DapServerRootModelNode(DappleModel oModel)
			: base(oModel)
		{
			MarkLoaded();
		}

		#endregion


		#region Properties

		[Browsable(false)]
		public override bool ShowAllChildren
		{
			get { return UseShowAllChildren; }
		}

		public override String DisplayText
		{
			get { return "DAP Servers"; }
		}

		public override string Annotation
		{
			get { return String.Format("[{0}]", FilteredChildren.Length); }
		}

		[Browsable(false)]
		public override string IconKey
		{
			get { return IconKeys.DapRoot; }
		}

		[Browsable(false)]
		public ToolStripMenuItem[] MenuItems
		{
			get
			{
				return new ToolStripMenuItem[] {
					new ToolStripMenuItem("Add DAP Server...", IconKeys.ImageList.Images[IconKeys.AddDAPServerMenuItem], new EventHandler(c_miAddDAPServer_Click))
				};
			}
		}

		#endregion


		#region Event Hanlders

		protected void c_miAddDAPServer_Click(object sender, EventArgs e)
		{
			m_oModel.AddDAPServer();
		}

		#endregion


		#region Public Methods

		public DapServerModelNode GetServer(DapServerUri oUri)
		{
			foreach (DapServerModelNode oServer in UnfilteredChildren)
			{
				if (oServer.Uri.Equals(oUri))
				{
					return oServer;
				}
			}

			return null;
		}

		public DapServerModelNode AddServer(DapServerUri oUri, bool blEnabled)
		{
			DapServerModelNode result = new DapServerModelNode(m_oModel, oUri, blEnabled);
			AddChild(result);
			if (blEnabled)
			{
				result.BeginLoad();
			}
			return result;
		}

		public void SearchFilterChanged()
		{
			foreach (DapServerModelNode oServer in UnfilteredChildren)
			{
				oServer.SearchFilterChanged();
			}
		}

		public ServerModelNode SetFavouriteServer(String strUri)
		{
			ServerModelNode result = null;

			foreach (DapServerModelNode oServer in UnfilteredChildren)
			{
				if (oServer.UpdateFavouriteStatus(strUri))
				{
					result = oServer;
				}
			}

			return result;
		}

		#region Saving and Loading old Dapple Views

		public void SaveToView(dappleview.serversType oServers)
		{
			dappleview.builderentryType oDAPBuilder = oServers.Newbuilderentry();
			dappleview.builderdirectoryType oDAPDir = oDAPBuilder.Newbuilderdirectory();
			oDAPDir.Addname(new Altova.Types.SchemaString("DAP Servers"));
			oDAPDir.Addspecialcontainer(new dappleview.SpecialDirectoryType("DAPServers"));

			foreach (DapServerModelNode oChild in UnfilteredChildren)
			{
				dappleview.builderentryType oChildEntry = oDAPDir.Newbuilderentry();
				dappleview.dapcatalogType oChildCatalog = oChildEntry.Newdapcatalog();

				oChildCatalog.Addurl(new Altova.Types.SchemaString(oChild.Uri.ToBaseUri()));
				oChildCatalog.Addenabled(new Altova.Types.SchemaBoolean(oChild.Enabled));

				oChildEntry.Adddapcatalog(oChildCatalog);
				oDAPDir.Addbuilderentry(oChildEntry);
			}

			oDAPBuilder.Addbuilderdirectory(oDAPDir);
			oServers.Addbuilderentry(oDAPBuilder);
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


	public class DapServerModelNode : ServerModelNode, IFilterableModelNode
	{
		#region Constants

		private const int MAX_SEARCH_RESULTS = 1000;

		#endregion


		#region Statics

		public static CatalogCacheManager s_oCCM = new CatalogCacheManager(null, Dapple.MainForm.Settings.CachePath);

		#endregion


		#region Member Variables

		private DapServerUri m_oUri;
		private String m_strTitle;
		private Server m_oServer;
		private bool m_blEntireCatalogMode;
		private CatalogFolder m_oFolder;
		private bool m_blBrowserMapAvailable;

		#endregion


		#region Constructors

		public DapServerModelNode(DappleModel oModel, DapServerUri oUri, bool blEnabled)
			: base(oModel, blEnabled)
		{
			m_oUri = oUri;
			m_strTitle = m_oUri.ServerTreeDisplayName;
		}

		#endregion


		#region Properites

		public override string DisplayText
		{
			get
			{
				return m_strTitle;
			}
		}

		public override string Annotation
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
		public override string ServerTypeIconKey
		{
			get { return IconKeys.DapRoot; }
		}

		[Browsable(true)]
		[Category("Server")]
		[Description("The URI for this server.")]
		public override ServerUri Uri
		{
			get { return m_oUri; }
		}

		public Server Server
		{
			get { return m_oServer; }
		}

		[Browsable(true)]
		[Category("Server")]
		[Description("What type of server (DAP, WMS, ArcIMS) this server is.")]
		public override ServerModelNode.ServerType Type
		{
			get { return ServerType.DAP; }
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

				int result = m_oServer.DatasetCount;
				if (m_blBrowserMapAvailable) result++;

				return result;
			}
		}

		[Browsable(false)]
		public bool PassesFilter
		{
			get
			{
				return m_oServer == null || FilteredChildCount > 0;
			}
		}

		#endregion


		#region Public Methods

		public void SearchFilterChanged()
		{
			if (this.Enabled)
			{
				UnloadSilently();
				BeginLoad();
			}
		}

		[Obsolete("This should get removed with the rest of the LayerBuilder/ServerTree stuff")]
		public override List<LayerBuilder> GetBuildersInternal()
		{
			this.WaitForLoad();

			List<LayerBuilder> result = new List<LayerBuilder>();
			System.Collections.ArrayList oDapDatasets;
			m_oServer.Command.GetCatalog(null, 0, 0, 1000, m_oModel.SearchKeyword, m_oModel.SearchBounds_DAP, out oDapDatasets);

			if (!m_oModel.SearchBoundsSet || this.m_oServer.ServerExtents.Intersects(m_oModel.SearchBounds_DAP))
			{
				result.Add(new DAPBrowserMapBuilder(Dapple.MainForm.WorldWindowSingleton, this.m_oServer, null));
			}

			foreach (Geosoft.Dap.Common.DataSet oDataSet in oDapDatasets)
			{
				result.Add(new DAPQuadLayerBuilder(oDataSet, Dapple.MainForm.WorldWindowSingleton, m_oServer, null));
			}

			return result;
		}

		public override void AddToHomeView()
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
			String strCacheDir = Dapple.MainForm.Settings.CachePath;
			Directory.CreateDirectory(strCacheDir);
			m_oServer = new Server(m_oUri.ToBaseUri(), strCacheDir, DapServerRootModelNode.DAPSecureToken, true);

			if (m_oServer.Status != Server.ServerStatus.OnLine)
			{
				throw new DapException("Server is " + m_oServer.Status.ToString());
			}

			m_oServer.GetDatasetCount(m_oModel.SearchBounds_DAP, m_oModel.SearchKeyword);

			DapBrowserMapModelNode oBrowserMap = new DapBrowserMapModelNode(m_oModel, m_oServer);
			m_blBrowserMapAvailable = oBrowserMap.PassesFilter;

			m_strTitle = m_oServer.Name;
			m_blEntireCatalogMode = m_oServer.MajorVersion < 6 || (m_oServer.MajorVersion == 6 && m_oServer.MinorVersion < 3);

			List<ModelNode> result = new List<ModelNode>();

			if (m_blBrowserMapAvailable)
			{
				result.Add(oBrowserMap);
			}

			String strEdition;
			m_oFolder = s_oCCM.GetCatalogHierarchyRoot(m_oServer, m_oModel.SearchBounds_DAP, m_oModel.SearchBoundsSet, m_oModel.SearchKeywordSet, m_oModel.SearchKeyword, out m_blEntireCatalogMode, out strEdition);
			foreach (CatalogFolder oSubFolder in m_oFolder.Folders)
			{
				result.Add(new DapDirectoryModelNode(m_oModel, oSubFolder));
			}

			while (!DapServerModelNode.s_oCCM.bGetDatasetList(m_oServer, m_oFolder.Hierarchy, m_oFolder.Timestamp, m_oModel.SearchBounds_DAP, m_oModel.SearchBoundsSet, m_oModel.SearchKeywordSet, m_oModel.SearchKeyword)) { }

			FolderDatasetList oDatasets = DapServerModelNode.s_oCCM.GetDatasets(m_oServer, m_oFolder, m_oModel.SearchBounds_DAP, m_oModel.SearchBoundsSet, m_oModel.SearchKeywordSet, m_oModel.SearchKeyword);

			foreach (DataSet oDataset in oDatasets.Datasets)
			{
				result.Add(new DapDatasetModelNode(m_oModel, oDataset));
			}

			return result.ToArray();
		}

		#endregion

	}


	public class PersonalDapServerModelNode : DapServerModelNode
	{
		#region Constructors

		public PersonalDapServerModelNode(DappleModel oModel)
			: base(oModel, new DapServerUri("http://localhost:10205/"), true)
		{
		}

		#endregion


		#region Properties

		[Browsable(false)]
		public override string IconKey
		{
			get { return IconKeys.PersonalDAPServer; }
		}

		public static bool PersonalDapRunning
		{
			get
			{
				foreach (System.Diagnostics.Process oProcess in System.Diagnostics.Process.GetProcesses())
				{
					if (string.Compare(oProcess.ProcessName, "geosoft.dap.server", true) == 0)
					{
						return true;
					}
				}

				return false;
			}
		}

		[Browsable(false)]
		public override ToolStripMenuItem[] MenuItems
		{
			get
			{
				return new ToolStripMenuItem[] {
					m_oProperties,
					m_oRefresh,
					m_oSetFavourite
				};
			}
		}

		[Browsable(false)]
		public override string ServerTypeIconKey
		{
			get { return IconKeys.PersonalDAPServer; }
		}

		#endregion


		#region Public Methods

		public override void AddToHomeView()
		{
			// --- This space intentionally left blank ---
		}

		#endregion
	}


	public class DapDirectoryModelNode : ModelNode
	{
		#region Member Variables

		private CatalogFolder m_oFolder;

		#endregion


		#region Constructors

		public DapDirectoryModelNode(DappleModel oModel, CatalogFolder oFolder)
			: base(oModel)
		{
			m_oFolder = oFolder;

			foreach (CatalogFolder oSubFolder in m_oFolder.Folders)
			{
				AddChildSilently(new DapDirectoryModelNode(m_oModel, oSubFolder));
			}
		}

		#endregion


		#region Properties

		public override string DisplayText
		{
			get { return m_oFolder.Name; }
		}

		[Browsable(false)]
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
			while (!DapServerModelNode.s_oCCM.bGetDatasetList(GetServer().Server, m_oFolder.Hierarchy, m_oFolder.Timestamp, m_oModel.SearchBounds_DAP, m_oModel.SearchBoundsSet, m_oModel.SearchKeywordSet, m_oModel.SearchKeyword)) { }

			FolderDatasetList oDatasets = DapServerModelNode.s_oCCM.GetDatasets(GetServer().Server, m_oFolder, m_oModel.SearchBounds_DAP, m_oModel.SearchBoundsSet, m_oModel.SearchKeywordSet, m_oModel.SearchKeyword);

			List<ModelNode> result = new List<ModelNode>();

			foreach (DataSet oDataset in oDatasets.Datasets)
			{
				result.Add(new DapDatasetModelNode(m_oModel, oDataset));
			}

			return result.ToArray();
		}

		private DapServerModelNode GetServer()
		{
			ModelNode oServerNode = this;
			while (oServerNode != null && !(oServerNode is DapServerModelNode))
			{
				oServerNode = oServerNode.Parent;
			}

			if (oServerNode == null) throw new ApplicationException("Orphaned DAP folder node");

			return oServerNode as DapServerModelNode;
		}

		#endregion
	}


	public class DapDatasetModelNode : LayerModelNode
	{
		#region Member Variables

		DataSet m_oDataSet;

		#endregion


		#region Constructors

		public DapDatasetModelNode(DappleModel oModel, DataSet oDataSet)
			: base(oModel)
		{
			m_oDataSet = oDataSet;

			MarkLoaded();
		}

		#endregion


		#region Properties

		[Browsable(false)]
		public override bool IsLeaf
		{
			get { return true; }
		}

		public override string DisplayText
		{
			get { return m_oDataSet.Title; }
		}

		[Browsable(false)]
		public override string IconKey
		{
			get { return IconKeys.DapLayerPrefix + m_oDataSet.Type.ToLowerInvariant(); }
		}

		#endregion


		#region Public Methods

		[Obsolete("This should get removed with the rest of the LayerBuilder/ServerTree stuff")]
		public override LayerBuilder ConvertToLayerBuilder()
		{
			return new DAPQuadLayerBuilder(m_oDataSet, Dapple.MainForm.WorldWindowSingleton, GetServer().Server, null);
		}

		#endregion


		#region Helper Methods

		protected override ModelNode[] Load()
		{
			throw new ApplicationException(ErrLoadedLeafNode);
		}

		private DapServerModelNode GetServer()
		{
			ModelNode oServerNode = this;
			while (oServerNode != null && !(oServerNode is DapServerModelNode))
			{
				oServerNode = oServerNode.Parent;
			}

			if (oServerNode == null) throw new ApplicationException("Orphaned DAP dataset node");

			return oServerNode as DapServerModelNode;
		}

		#endregion
	}


	public class DapBrowserMapModelNode : LayerModelNode, IFilterableModelNode
	{
		#region Member Variables

		Server m_oData;

		#endregion


		#region Constructors

		public DapBrowserMapModelNode(DappleModel oModel, Server oData)
			: base(oModel)
		{
			m_oData = oData;
		}

		#endregion


		#region Properties

		[Browsable(false)]
		public override bool IsLeaf
		{
			get { return true; }
		}

		public override string DisplayText
		{
			get { return "Browser Map"; }
		}

		[Browsable(false)]
		public override string IconKey
		{
			get { return IconKeys.DapBrowserMapLayer; }
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
				return !m_oModel.SearchBoundsSet || m_oModel.SearchBounds_DAP.Intersects(m_oData.ServerExtents);
			}
		}

		#endregion


		#region Public Methods

		[Obsolete("This should get removed with the rest of the LayerBuilder/ServerTree stuff")]
		public override LayerBuilder ConvertToLayerBuilder()
		{
			return new DAPBrowserMapBuilder(Dapple.MainForm.WorldWindowSingleton, m_oData, null);
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