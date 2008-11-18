using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using Dapple.LayerGeneration;

namespace NewServerTree
{
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

	public class DappleModel
	{
		#region Member Variables

		private AvailableServersModelNode m_oRootNode;
		private ModelNode m_oSelectedNode;
		private Object m_oLock = new Object();

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

		public ModelNode PersonalDapNode
		{
			get { throw new NotImplementedException(); }
		}

		#endregion


		#region Public Methods

		#region Adding Servers

		public ServerModelNode AddArcIMSServer(ArcIMSServerUri oUri)
		{
			lock (m_oLock)
			{
				// --- Don't add the server if it's already in the model ---

				foreach (ArcIMSServerModelNode oArcIMSServer in m_oRootNode.ArcIMSServers.Children)
				{
					if (oArcIMSServer.Uri.Equals(oUri))
					{
						return oArcIMSServer;
					}
				}

				// --- Add the server ---

				return m_oRootNode.ArcIMSServers.AddServer(oUri);
			}
		}

		public ServerModelNode AddWMSServer(WMSServerUri oUri)
		{
			lock (m_oLock)
			{
				// --- Don't add the server if it's already in the model ---

				foreach (WMSServerModelNode oWMSServer in m_oRootNode.WMSServers.Children)
				{
					if (oWMSServer.Uri.Equals(oUri))
					{
						return oWMSServer;
					}
				}

				// --- Add the server ---

				return m_oRootNode.WMSServers.AddServer(oUri);
			}
		}

		public ServerModelNode AddDAPServer(DapServerUri oUri)
		{
			lock (m_oLock)
			{
				// --- Don't add the server if it's already in the model ---

				foreach (DapServerModelNode oDAPServer in m_oRootNode.DAPServers.Children)
				{
					if (oDAPServer.Uri.Equals(oUri))
					{
						return oDAPServer;
					}
				}

				// --- Add the server ---

				return m_oRootNode.DAPServers.AddServer(oUri);
			}
		}

		public void RefreshServer(ModelNode oServer)
		{
			throw new NotImplementedException();
		}

		public void RemoveServer(ModelNode oServer)
		{
			throw new NotImplementedException();
		}

		public void Save()
		{
			throw new NotImplementedException();
		}

		internal void Load()
		{
			AddDAPServer(new DapServerUri("http://dap.geosoft.com"));
			AddDAPServer(new DapServerUri("http://gdrdap.agg.nrcan.gc.ca"));

			AddWMSServer(new WMSServerUri("http://gdr.ess.nrcan.gc.ca/wmsconnector/com.esri.wms.Esrimap/gdr_e"));
			AddWMSServer(new WMSServerUri("http://atlas.gc.ca/cgi-bin/atlaswms_en?VERSION=1.1.1"));
			AddWMSServer(new WMSServerUri("http://apps1.gdr.nrcan.gc.ca/cgi-bin/canmin_en-ca_ows"));
			AddWMSServer(new WMSServerUri("http://www.ga.gov.au/bin/getmap.pl"));
			AddWMSServer(new WMSServerUri("http://apps1.gdr.nrcan.gc.ca/cgi-bin/worldmin_en-ca_ows"));
			AddWMSServer(new WMSServerUri("http://gisdata.usgs.net/servlet/com.esri.wms.Esrimap"));
			AddWMSServer(new WMSServerUri("http://maps.customweather.com/image"));
			AddWMSServer(new WMSServerUri("http://cgkn.net/cgi-bin/cgkn_wms"));
			AddWMSServer(new WMSServerUri("http://wms.jpl.nasa.gov/wms.cgi"));

			AddArcIMSServer(new ArcIMSServerUri("http://www.geographynetwork.com/servlet/com.esri.esrimap.Esrimap"));
			AddArcIMSServer(new ArcIMSServerUri("http://gisdata.usgs.gov/servlet/com.esri.esrimap.Esrimap"));
			AddArcIMSServer(new ArcIMSServerUri("http://map.ngdc.noaa.gov/servlet/com.esri.esrimap.Esrimap"));
			AddArcIMSServer(new ArcIMSServerUri("http://mrdata.usgs.gov/servlet/com.esri.esrimap.Esrimap"));
			AddArcIMSServer(new ArcIMSServerUri("http://gdw.apfo.usda.gov/servlet/com.esri.esrimap.Esrimap"));
		}

		public void SetFilter(Object keywords, Object bounds)
		{
			throw new NotImplementedException();
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

		#endregion
	}
}
