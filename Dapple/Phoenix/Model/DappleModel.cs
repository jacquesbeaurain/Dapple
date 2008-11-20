using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using Dapple.LayerGeneration;
using Dapple;
using dappleview;
using WorldWind;

namespace NewServerTree
{
	#region Event Args

	public class NodeLoadEventArgs : EventArgs
	{
		private ModelNode m_oNode;

		public NodeLoadEventArgs(ModelNode oNode)
		{
			m_oNode = oNode;
		}

		public ModelNode Node
		{
			get { return m_oNode; }
		}
	}

	public class NodeAddedEventArgs : EventArgs
	{
		private ModelNode m_oParentNode;
		private ModelNode m_oChildNode;

		public NodeAddedEventArgs(ModelNode parent, ModelNode child)
		{
			m_oParentNode = parent;
			m_oChildNode = child;
		}

		public ModelNode Parent
		{
			get { return m_oParentNode; }
		}

		public ModelNode Child
		{
			get { return m_oChildNode; }
		}
	}

	public class NodeRemovedEventArgs : EventArgs
	{
		private ModelNode m_oParentNode;
		private ModelNode m_oChildNode;

		public NodeRemovedEventArgs(ModelNode parent, ModelNode child)
		{
			m_oParentNode = parent;
			m_oChildNode = child;
		}

		public ModelNode Parent
		{
			get { return m_oParentNode; }
		}

		public ModelNode Child
		{
			get { return m_oChildNode; }
		}
	}

	public class NodeDisplayUpdatedEventArgs : EventArgs
	{
		private ModelNode m_oNode;

		public NodeDisplayUpdatedEventArgs(ModelNode oNode)
		{
			m_oNode = oNode;
		}

		public ModelNode Node
		{
			get { return m_oNode; }
		}
	}

	public class NodeUnloadedEventArgs : EventArgs
	{
		private ModelNode m_oNode;

		public NodeUnloadedEventArgs(ModelNode oNode)
		{
			m_oNode = oNode;
		}

		public ModelNode Node
		{
			get { return m_oNode; }
		}
	}

	#endregion

	public class DappleModel
	{
		#region Member Variables

		private AvailableServersModelNode m_oRootNode;
		private ModelNode m_oSelectedNode;
		private Object m_oLock = new Object();
		private String m_strSearchKeyword = String.Empty;
		private GeographicBoundingBox m_oSearchBounds = null;
		private ServerModelNode m_oFavouriteServer = null;

		#endregion


		#region Events

		public event EventHandler SelectedNodeChanged;
		protected void OnSelectedNodeChanged(EventArgs e)
		{
			if (SelectedNodeChanged != null)
			{
				SelectedNodeChanged(this, e);
			}
		}

		public event EventHandler<NodeLoadEventArgs> NodeLoaded;
		protected void OnNodeLoaded(NodeLoadEventArgs e)
		{
			if (NodeLoaded != null)
			{
				NodeLoaded(this, e);
			}
		}

		public event EventHandler<NodeAddedEventArgs> NodeAdded;
		protected void OnNodeAdded(NodeAddedEventArgs e)
		{
			if (NodeAdded != null)
			{
				NodeAdded(this, e);
			}
		}

		public event EventHandler<NodeRemovedEventArgs> NodeRemoved;
		protected void OnNodeRemoved(NodeRemovedEventArgs e)
		{
			if (NodeRemoved != null)
			{
				NodeRemoved(this, e);
			}
		}

		public event EventHandler<NodeDisplayUpdatedEventArgs> NodeDisplayUpdated;
		protected void OnNodeDisplayUpdated(NodeDisplayUpdatedEventArgs e)
		{
			if (NodeDisplayUpdated != null)
			{
				NodeDisplayUpdated(this, e);
			}
		}

		public event EventHandler<NodeUnloadedEventArgs> NodeUnloaded;
		protected void OnNodeUnloaded(NodeUnloadedEventArgs e)
		{
			if (NodeUnloaded != null)
			{
				NodeUnloaded(this, e);
			}
		}

		public event EventHandler Loaded;
		protected void OnLoaded(EventArgs e)
		{
			if (Loaded != null)
			{
				Loaded(this, e);
			}
		}

		public event EventHandler SearchFilterChanged;
		protected void OnSearchFilterChanged(EventArgs e)
		{
			if (SearchFilterChanged != null)
			{
				SearchFilterChanged(this, e);
			}
		}

		public event EventHandler FavouriteServerChanged;
		protected void OnFavouriteServerChanged(EventArgs e)
		{
			if (FavouriteServerChanged != null)
			{
				FavouriteServerChanged(this, e);
			}
		}

		#endregion


		#region Constructors

		public DappleModel()
		{
			m_oRootNode = new AvailableServersModelNode(this);
			m_oSelectedNode = m_oRootNode;
		}

		#endregion


		#region Properties

		public ModelNode SelectedNode
		{
			get { return m_oSelectedNode; }
			set
			{
				if (value == null) Debugger.Break();

				if (m_oSelectedNode != value)
				{
					m_oSelectedNode = value;
					OnSelectedNodeChanged(EventArgs.Empty);
				}
			}
		}

		/// <summary>
		/// The server that the currently selected ModelNode is from, or null if no
		/// server is currently selected, for example, if the current ModelNode
		/// is one of the root nodes.
		/// </summary>
		public ServerModelNode SelectedServer
		{
			get
			{
				ModelNode result = m_oSelectedNode;
				while (result != null)
				{
					if (result is ServerModelNode) return result as ServerModelNode;
					result = result.Parent;
				}

				return null;
			}
		}

		public bool SearchKeywordSet
		{
			get { return !String.IsNullOrEmpty(m_strSearchKeyword); }
		}

		public String SearchKeyword
		{
			get { return m_strSearchKeyword; }
		}

		public bool SearchBoundsSet
		{
			get { return m_oSearchBounds != null; }
		}

		public GeographicBoundingBox SearchBounds_Geo
		{
			get { return m_oSearchBounds; }
		}

		public Geosoft.Dap.Common.BoundingBox SearchBounds_DAP
		{
			get
			{
				if (m_oSearchBounds == null)
				{
					return null;
				}

				return new Geosoft.Dap.Common.BoundingBox(m_oSearchBounds.East, m_oSearchBounds.North, m_oSearchBounds.West, m_oSearchBounds.South);
			}
		}

		public bool SearchFilterSet
		{
			get { return SearchKeywordSet || SearchBoundsSet; }
		}

		#endregion


		#region Public Methods

		#region Adding Servers

		public ServerModelNode AddArcIMSServer(ArcIMSServerUri oUri, bool blEnabled)
		{
			lock (m_oLock)
			{
				// --- Don't add the server if it's already in the model ---

				foreach (ArcIMSServerModelNode oArcIMSServer in m_oRootNode.ArcIMSServers.UnfilteredChildren)
				{
					if (oArcIMSServer.ServerUri.Equals(oUri))
					{
						return oArcIMSServer;
					}
				}

				// --- Add the server ---

				return m_oRootNode.ArcIMSServers.AddServer(oUri, blEnabled);
			}
		}

		public ServerModelNode AddWMSServer(WMSServerUri oUri, bool blEnabled)
		{
			lock (m_oLock)
			{
				// --- Don't add the server if it's already in the model ---

				foreach (WMSServerModelNode oWMSServer in m_oRootNode.WMSServers.UnfilteredChildren)
				{
					if (oWMSServer.ServerUri.Equals(oUri))
					{
						return oWMSServer;
					}
				}

				// --- Add the server ---

				return m_oRootNode.WMSServers.AddServer(oUri, blEnabled);
			}
		}

		public ServerModelNode AddDAPServer(DapServerUri oUri, bool blEnabled)
		{
			lock (m_oLock)
			{
				// --- Don't add the server if it's already in the model ---

				foreach (DapServerModelNode oDAPServer in m_oRootNode.DAPServers.UnfilteredChildren)
				{
					if (oDAPServer.ServerUri.Equals(oUri))
					{
						return oDAPServer;
					}
				}

				// --- Add the server ---

				return m_oRootNode.DAPServers.AddServer(oUri, blEnabled);
			}
		}

		public void AddImageTileLayer(String strTileSetName, ImageTileLayerModelNode oLayer)
		{
			lock (m_oLock)
			{
				// --- Get the tileset to add to ---

				ImageTileSetModelNode oSet = m_oRootNode.ImageTileSets.GetImageTileSet(strTileSetName);

				// --- Add the tileset ---

				oSet.AddLayer(oLayer);
			}
		}

		public void Save()
		{
			throw new NotImplementedException();
		}

		public void Load(DappleView oSource)
		{
			lock (m_oLock)
			{
				ClearModel();

				if (oSource.View.Hasservers())
				{
					for (int i = 0; i < oSource.View.servers.builderentryCount; i++)
					{
						builderentryType entry = oSource.View.servers.GetbuilderentryAt(i);
						LoadBuilderEntryType(entry);
					}
				}

				OnLoaded(EventArgs.Empty);
			}
		}

		private void LoadBuilderEntryType(builderentryType entry)
		{
			if (entry.Hasbuilderdirectory())
				for (int i = 0; i < entry.builderdirectory.builderentryCount; i++)
					LoadBuilderEntryType(entry.builderdirectory.GetbuilderentryAt(i));
			else if (entry.Haswmscatalog())
				AddWMSServer(new WMSServerUri(entry.wmscatalog.capabilitiesurl.Value), entry.wmscatalog.Hasenabled() ? entry.wmscatalog.enabled.Value : true);
			else if (entry.Hasarcimscatalog())
				AddArcIMSServer(new ArcIMSServerUri(entry.arcimscatalog.capabilitiesurl.Value), entry.arcimscatalog.Hasenabled() ? entry.arcimscatalog.enabled.Value : true);
			else if (entry.Hasdapcatalog())
				AddDAPServer(new DapServerUri(entry.dapcatalog.url.Value), entry.dapcatalog.Hasenabled() ? entry.dapcatalog.enabled.Value : true);
			else if (entry.Hastileserverset())
				LoadTileServerSet(entry.tileserverset);
		}

		private void LoadTileServerSet(tileserversetType entry)
		{
			if (entry.Hastilelayers())
			{
				for (int i = 0; i < entry.tilelayers.tilelayerCount; i++)
				{
					tilelayerType oLayer = entry.tilelayers.GettilelayerAt(i);

					ImageTileLayerModelNode oNode = new ImageTileLayerModelNode(
						this,
						oLayer.name.Value,
						new Uri(oLayer.url.Value),
						oLayer.imageextension.Value,
						oLayer.levelzerotilesize.Value,
						oLayer.dataset.Value,
						oLayer.levels.Value);

					this.AddImageTileLayer(entry.name.Value, oNode);
				}
			}
		}

		public void LoadTestView()
		{
			lock (m_oLock)
			{
				ClearModel();

				bool blHalfEnabled = false;

				AddDAPServer(new DapServerUri("http://dap.geosoft.com"), true);
				AddDAPServer(new DapServerUri("http://gdrdap.agg.nrcan.gc.ca"), blHalfEnabled);

				AddWMSServer(new WMSServerUri("http://gdr.ess.nrcan.gc.ca/wmsconnector/com.esri.wms.Esrimap/gdr_e"), blHalfEnabled);
				AddWMSServer(new WMSServerUri("http://atlas.gc.ca/cgi-bin/atlaswms_en?VERSION=1.1.1"), true);
				AddWMSServer(new WMSServerUri("http://apps1.gdr.nrcan.gc.ca/cgi-bin/canmin_en-ca_ows"), blHalfEnabled);
				AddWMSServer(new WMSServerUri("http://www.ga.gov.au/bin/getmap.pl"), true);
				AddWMSServer(new WMSServerUri("http://apps1.gdr.nrcan.gc.ca/cgi-bin/worldmin_en-ca_ows"), blHalfEnabled);
				AddWMSServer(new WMSServerUri("http://gisdata.usgs.net/servlet/com.esri.wms.Esrimap"), true);
				AddWMSServer(new WMSServerUri("http://maps.customweather.com/image"), blHalfEnabled);
				AddWMSServer(new WMSServerUri("http://cgkn.net/cgi-bin/cgkn_wms"), true);
				AddWMSServer(new WMSServerUri("http://wms.jpl.nasa.gov/wms.cgi"), blHalfEnabled);

				AddImageTileLayer("NASA Landsat Imagery", new ImageTileLayerModelNode(this, "NLT Landsat7 (Visible Color)", new Uri("http://worldwind25.arc.nasa.gov/tile/tile.aspx"), "jpg", 2.25, "105", 5));
				AddImageTileLayer("USGS Imagery", new ImageTileLayerModelNode(this, "USGS Digital Ortho", new Uri("http://worldwind25.arc.nasa.gov/tile/tile.aspx"), "jpg", 0.8, "101", 8));

				AddArcIMSServer(new ArcIMSServerUri("http://www.geographynetwork.com/servlet/com.esri.esrimap.Esrimap"), blHalfEnabled);
				AddArcIMSServer(new ArcIMSServerUri("http://gisdata.usgs.gov/servlet/com.esri.esrimap.Esrimap"), blHalfEnabled);
				AddArcIMSServer(new ArcIMSServerUri("http://map.ngdc.noaa.gov/servlet/com.esri.esrimap.Esrimap"), true);
				AddArcIMSServer(new ArcIMSServerUri("http://mrdata.usgs.gov/servlet/com.esri.esrimap.Esrimap"), blHalfEnabled);
				AddArcIMSServer(new ArcIMSServerUri("http://gdw.apfo.usda.gov/servlet/com.esri.esrimap.Esrimap"), true);

				SetFavouriteServer("http://gisdata.usgs.net/servlet/com.esri.wms.Esrimap");

				OnLoaded(EventArgs.Empty);
			}
		}

		public void SetSearchFilter(String strKeyword, GeographicBoundingBox oBounds)
		{
			lock (m_oLock)
			{
				if (strKeyword != m_strSearchKeyword || oBounds != m_oSearchBounds)
				{
					if (SelectedServer != null)
						m_oSelectedNode = SelectedServer;

					m_strSearchKeyword = strKeyword;
					m_oSearchBounds = oBounds;
					m_oRootNode.DAPServers.SearchFilterChanged();

					OnSearchFilterChanged(EventArgs.Empty);
				}
			}
		}

		public void SetFavouriteServer(String strUri)
		{
			if (m_oFavouriteServer == null || !m_oFavouriteServer.ServerUri.ToString().Equals(strUri))
			{
				m_oRootNode.SetFavouriteServer(strUri);

				OnFavouriteServerChanged(EventArgs.Empty);
			}
		}

		#endregion

		public void DoWithLock(MethodInvoker oOperation)
		{
			lock (m_oLock)
			{
				oOperation();
			}
		}

		public void DoWithLock(WaitCallback oOperation, Object oParam)
		{
			lock (m_oLock)
			{
				oOperation(oParam);
			}
		}

		public void ModelNodeLoaded(ModelNode oLoadedNode)
		{
			bool blVisible = false;
			for (ModelNode iter = m_oSelectedNode; iter != null; iter = iter.Parent)
			{
				if (iter == oLoadedNode.Parent)
				{
					blVisible = true;
					break;
				}
			}

			if (blVisible) OnNodeLoaded(new NodeLoadEventArgs(oLoadedNode));
		}

		public void ModelNodeAdded(ModelNode oParentNode, ModelNode oChildNode)
		{
			bool blVisible = false;
			for (ModelNode iter = m_oSelectedNode; iter != null; iter = iter.Parent)
			{
				if (iter == oParentNode)
				{
					blVisible = true;
					break;
				}
			}

			if (blVisible) OnNodeAdded(new NodeAddedEventArgs(oParentNode, oChildNode));
		}

		public void ModelNodeRemoved(ModelNode oParentNode, ModelNode oChildNode)
		{
			if (IsSelectedOrAncestor(oChildNode))
			{
				SelectedNode = oParentNode;
			}

			OnNodeRemoved(new NodeRemovedEventArgs(oParentNode, oChildNode));
		}

		public void ModelNodeDisplayUpdated(ModelNode oUpdatedNode)
		{
			bool blVisible = false;
			for (ModelNode iter = m_oSelectedNode; iter != null; iter = iter.Parent)
			{
				if (iter == oUpdatedNode.Parent)
				{
					blVisible = true;
					break;
				}
			}

			if (blVisible) OnNodeDisplayUpdated(new NodeDisplayUpdatedEventArgs(oUpdatedNode));
		}

		public void ModelNodeUnloaded(ModelNode oNode)
		{
			for (ModelNode iter = m_oSelectedNode; iter != null; iter = iter.Parent)
			{
				if (iter == oNode.Parent)
				{
					OnNodeUnloaded(new NodeUnloadedEventArgs(oNode));
					return;
				}
				else if (iter == oNode && oNode != m_oSelectedNode)
				{
					m_oSelectedNode = oNode;
					OnSelectedNodeChanged(EventArgs.Empty);
					return;
				}
			}
		}

		public bool IsSelectedOrAncestor(ModelNode oNode)
		{
			for (ModelNode iter = m_oSelectedNode; iter != null; iter = iter.Parent)
			{
				if (iter == oNode) return true;
			}

			return false;
		}

		#endregion


		#region Helper Methods

		private bool NodeIsOpened(ModelNode oNode)
		{
			ModelNode iter = m_oSelectedNode;
			while (iter != null)
			{
				if (iter == oNode) return true;
				iter = iter.Parent;
			}

			return false;
		}

		private void ClearModel()
		{
			lock (m_oLock)
			{
				m_oRootNode.Clear();
			}

			m_oSelectedNode = m_oRootNode;
		}

		#endregion
	}
}
