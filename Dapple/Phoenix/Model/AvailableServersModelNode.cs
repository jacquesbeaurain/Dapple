using System;

namespace NewServerTree
{
	public class AvailableServersModelNode : ModelNode
	{
		#region Member Variables

		private WMSRootModelNode m_oWMSRootNode;
		private ArcIMSRootModelNode m_oArcIMSRootNode;
		private DapServerRootModelNode m_oDAPRootNode;
		private ImageTileSetRootModelNode m_oTileRootNode;
		private VERootModelNode m_oVERootNode;
		private PersonalDapServerModelNode m_oPersonalDAPServer;

		#endregion


		#region Constructors

		public AvailableServersModelNode(DappleModel oModel)
			: base(oModel)
		{
			m_oDAPRootNode = new DapServerRootModelNode(m_oModel);
			AddChildSilently(m_oDAPRootNode);

			if (PersonalDapServerModelNode.PersonalDapRunning)
			{
				m_oPersonalDAPServer = new PersonalDapServerModelNode(m_oModel);
				AddChildSilently(m_oPersonalDAPServer);
				m_oPersonalDAPServer.BeginLoad();
			}

			m_oTileRootNode = new ImageTileSetRootModelNode(m_oModel);
			AddChildSilently(m_oTileRootNode);

			m_oVERootNode = new VERootModelNode(m_oModel);
			AddChildSilently(m_oVERootNode);

			m_oWMSRootNode = new WMSRootModelNode(m_oModel);
			AddChildSilently(m_oWMSRootNode);

			m_oArcIMSRootNode = new ArcIMSRootModelNode(m_oModel);
			AddChildSilently(m_oArcIMSRootNode);

			MarkLoaded();
		}

		#endregion


		#region Properties

		public override bool ShowAllChildren
		{
			get { return UseShowAllChildren; }
		}

		public override String DisplayText
		{
			get { return "Available Servers"; }
		}

		public override string IconKey
		{
			get { return IconKeys.AvailableServers; }
		}

		public DapServerRootModelNode DAPServers
		{
			get { return m_oDAPRootNode; }
		}

		public ImageTileSetRootModelNode ImageTileSets
		{
			get { return m_oTileRootNode; }
		}

		public WMSRootModelNode WMSServers
		{
			get { return m_oWMSRootNode; }
		}

		public ArcIMSRootModelNode ArcIMSServers
		{
			get { return m_oArcIMSRootNode; }
		}

		public PersonalDapServerModelNode PersonalDapServer
		{
			get { return m_oPersonalDAPServer; }
		}

		#endregion


		#region Public Methods

		public void Clear()
		{
			m_oDAPRootNode.ClearSilently();
			m_oTileRootNode.ClearSilently();
			m_oWMSRootNode.ClearSilently();
			m_oArcIMSRootNode.ClearSilently();
		}

		public ServerModelNode SetFavouriteServer(String strUri)
		{
			ServerModelNode temp, result = null;

			temp = m_oDAPRootNode.SetFavouriteServer(strUri);
			if (temp != null) result = temp;
			temp = m_oWMSRootNode.SetFavouriteServer(strUri);
			if (temp != null) result = temp;
			temp = m_oArcIMSRootNode.SetFavouriteServer(strUri);
			if (temp != null) result = temp;

			return result;
		}

		#region Saving and Loading old Dapple Views

		public void SaveToView(Dapple.DappleView oView)
		{
			dappleview.serversType oServers = oView.View.Newservers();

			m_oDAPRootNode.SaveToView(oServers);
			m_oTileRootNode.SaveToView(oServers);
			m_oVERootNode.SaveToView(oServers);

			dappleview.builderentryType oWMSBuilder = oServers.Newbuilderentry();
			dappleview.builderdirectoryType oWMSDir = oWMSBuilder.Newbuilderdirectory();
			oWMSDir.Addname(new Altova.Types.SchemaString("WMS Servers"));
			oWMSDir.Addspecialcontainer(new dappleview.SpecialDirectoryType("WMSServers"));

			m_oWMSRootNode.SaveToView(oWMSDir);
			m_oArcIMSRootNode.SaveToView(oWMSDir);


			oWMSBuilder.Addbuilderdirectory(oWMSDir);
			oServers.Addbuilderentry(oWMSBuilder);

			oView.View.Addservers(oServers);
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
}