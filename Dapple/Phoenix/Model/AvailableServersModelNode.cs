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

		#endregion


		#region Constructors

		public AvailableServersModelNode(DappleModel oModel)
			: base(oModel)
		{
			m_oDAPRootNode = new DapServerRootModelNode(m_oModel);
			AddChildSilently(m_oDAPRootNode);
			PersonalDapServerModelNode oPDNode = new PersonalDapServerModelNode(m_oModel);
			oPDNode.BeginLoad();
			AddChildSilently(oPDNode);
			m_oTileRootNode = new ImageTileSetRootModelNode(m_oModel);
			AddChildSilently(m_oTileRootNode);
			AddChildSilently(new VERootModelNode(m_oModel));
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

		#endregion


		#region Helper Methods

		protected override ModelNode[] Load()
		{
			throw new ApplicationException(ErrLoadedBadNode);
		}

		#endregion
	}
}