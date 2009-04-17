using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using WorldWind;
using Dapple.LayerGeneration;
using System.Xml;
using System.IO;
using Dapple;
using WorldWind.Net;
using System.Net;
using WorldWind.PluginEngine;
using System.Globalization;

namespace NewServerTree
{
	#region Event Args

	internal class NodeLoadEventArgs : EventArgs
	{
		private ModelNode m_oNode;

		internal NodeLoadEventArgs(ModelNode oNode)
		{
			m_oNode = oNode;
		}

		internal ModelNode Node
		{
			get { return m_oNode; }
		}
	}

	internal class NodeAddedEventArgs : EventArgs
	{
		private ModelNode m_oParentNode;
		private ModelNode m_oChildNode;

		internal NodeAddedEventArgs(ModelNode parent, ModelNode child)
		{
			m_oParentNode = parent;
			m_oChildNode = child;
		}

		internal ModelNode Parent
		{
			get { return m_oParentNode; }
		}

		internal ModelNode Child
		{
			get { return m_oChildNode; }
		}
	}

	internal class NodeRemovedEventArgs : EventArgs
	{
		private ModelNode m_oParentNode;
		private ModelNode m_oChildNode;

		internal NodeRemovedEventArgs(ModelNode parent, ModelNode child)
		{
			m_oParentNode = parent;
			m_oChildNode = child;
		}

		internal ModelNode Parent
		{
			get { return m_oParentNode; }
		}

		internal ModelNode Child
		{
			get { return m_oChildNode; }
		}
	}

	internal class NodeDisplayUpdatedEventArgs : EventArgs
	{
		private ModelNode m_oNode;

		internal NodeDisplayUpdatedEventArgs(ModelNode oNode)
		{
			m_oNode = oNode;
		}

		internal ModelNode Node
		{
			get { return m_oNode; }
		}
	}

	internal class NodeUnloadedEventArgs : EventArgs
	{
		private ModelNode m_oNode;

		internal NodeUnloadedEventArgs(ModelNode oNode)
		{
			m_oNode = oNode;
		}

		internal ModelNode Node
		{
			get { return m_oNode; }
		}
	}

	#endregion

	internal class DappleModel
	{
		#region Member Variables

		private AvailableServersModelNode m_oRootNode;
		private ModelNode m_oSelectedNode;
		private Object m_oLock = new Object();
		private String m_strSearchKeyword = String.Empty;
		private GeographicBoundingBox m_oSearchBounds = null;
		private ServerModelNode m_oFavouriteServer = null;
		private ViewedDatasetsModel m_oViewedDatasets;

		#endregion


		#region Events

		internal event EventHandler SelectedNodeChanged;
		protected void OnSelectedNodeChanged(EventArgs e)
		{
			if (SelectedNodeChanged != null)
			{
				SelectedNodeChanged(this, e);
			}
		}

		internal event EventHandler<NodeLoadEventArgs> NodeLoaded;
		protected void OnNodeLoaded(NodeLoadEventArgs e)
		{
			if (NodeLoaded != null)
			{
				NodeLoaded(this, e);
			}
		}

		internal event EventHandler<NodeAddedEventArgs> NodeAdded;
		protected void OnNodeAdded(NodeAddedEventArgs e)
		{
			if (NodeAdded != null)
			{
				NodeAdded(this, e);
			}
		}

		internal event EventHandler<NodeRemovedEventArgs> NodeRemoved;
		protected void OnNodeRemoved(NodeRemovedEventArgs e)
		{
			if (NodeRemoved != null)
			{
				NodeRemoved(this, e);
			}
		}

		internal event EventHandler<NodeDisplayUpdatedEventArgs> NodeDisplayUpdated;
		protected void OnNodeDisplayUpdated(NodeDisplayUpdatedEventArgs e)
		{
			if (NodeDisplayUpdated != null)
			{
				NodeDisplayUpdated(this, e);
			}
		}

		internal event EventHandler<NodeUnloadedEventArgs> NodeUnloaded;
		protected void OnNodeUnloaded(NodeUnloadedEventArgs e)
		{
			if (NodeUnloaded != null)
			{
				NodeUnloaded(this, e);
			}
		}

		internal event EventHandler Loaded;
		protected void OnLoaded(EventArgs e)
		{
			if (Loaded != null)
			{
				Loaded(this, e);
			}
		}

		internal event EventHandler SearchFilterChanged;
		protected void OnSearchFilterChanged(EventArgs e)
		{
			if (SearchFilterChanged != null)
			{
				SearchFilterChanged(this, e);
			}
		}

		internal event EventHandler FavouriteServerChanged;
		protected void OnFavouriteServerChanged(EventArgs e)
		{
			if (FavouriteServerChanged != null)
			{
				FavouriteServerChanged(this, e);
			}
		}

		internal event EventHandler ServerToggled;
		protected void OnServerToggled(EventArgs e)
		{
			if (ServerToggled != null)
			{
				ServerToggled(this, e);
			}
		}

		internal event EventHandler ServerAdded;
		protected void OnServerAdded(EventArgs e)
		{
			if (ServerAdded != null)
			{
				ServerAdded(this, e);
			}
		}

		#endregion


		#region Constructors

		internal DappleModel(LayerList oTarget)
		{
			m_oViewedDatasets = new ViewedDatasetsModel(oTarget);
			m_oRootNode = new AvailableServersModelNode(this);
			m_oSelectedNode = m_oRootNode;
		}

		#endregion


		#region Properties

		internal ModelNode SelectedNode
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
		internal ServerModelNode SelectedServer
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

		internal PersonalDapServerModelNode PersonalDapServer
		{
			get
			{
				return m_oRootNode.PersonalDapServer;
			}
		}

		internal bool SearchKeywordSet
		{
			get { return !String.IsNullOrEmpty(m_strSearchKeyword); }
		}

		internal String SearchKeyword
		{
			get { return m_strSearchKeyword; }
		}

		internal bool SearchBoundsSet
		{
			get { return m_oSearchBounds != null; }
		}

		internal GeographicBoundingBox SearchBounds_Geo
		{
			get { return m_oSearchBounds; }
		}

		internal Geosoft.Dap.Common.BoundingBox SearchBounds_DAP
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

		internal bool SearchFilterSet
		{
			get { return SearchKeywordSet || SearchBoundsSet; }
		}

		internal ViewedDatasetsModel ViewedDatasets
		{
			get { return m_oViewedDatasets; }
		}

		internal List<ServerModelNode> ListableServers
		{
			get
			{
				List<ServerModelNode> result = new List<ServerModelNode>();

				foreach (ServerModelNode oServer in m_oRootNode.DAPServers.UnfilteredChildren)
				{
					result.Add(oServer);
				}

				if (m_oRootNode.PersonalDapServer != null)
				{
					result.Add(m_oRootNode.PersonalDapServer);
				}

				foreach (ServerModelNode oServer in m_oRootNode.WMSServers.UnfilteredChildren)
				{
					result.Add(oServer);
				}

				foreach (ServerModelNode oServer in m_oRootNode.ArcIMSServers.UnfilteredChildren)
				{
					result.Add(oServer);
				}

				return result;
			}
		}

		#endregion


		#region Public Methods

		#region Saving and Loading old Dapple Views

		internal void SaveToView(Dapple.DappleView oView)
		{
			m_oRootNode.SaveToView(oView);

			if (m_oFavouriteServer != null)
			{
				oView.View.Addfavouriteserverurl(new Altova.Types.SchemaString(m_oFavouriteServer.Uri.ToString()));
			}
		}

		internal void LoadFromView(Dapple.DappleView oSource)
		{
			lock (m_oLock)
			{
				ClearModel();

				ServerModelNode oFavouriteServer = null;

				// --- Create favourite server Uri ---

				Uri oFavouriteServerUri = null;
				if (oSource.View.Hasfavouriteserverurl() && !String.IsNullOrEmpty(oSource.View.favouriteserverurl.Value))
				{
					try
					{
						oFavouriteServerUri = new Uri(oSource.View.favouriteserverurl.Value);
					}
					catch (UriFormatException)
					{
						// --- The favourite server is invalid. Default to no favourite server ---
					}
				}


				// --- Load the servers ---

				if (oSource.View.Hasservers())
				{
					for (int i = 0; i < oSource.View.servers.builderentryCount; i++)
					{
						dappleview.builderentryType entry = oSource.View.servers.GetbuilderentryAt(i);
						ServerModelNode temp = LoadBuilderEntryType(entry, oFavouriteServerUri);
						if (temp != null) oFavouriteServer = temp;
					}
				}

				if (oFavouriteServer != null)
				{
					SetFavouriteServer(oFavouriteServer, false);
				}
				else if (oSource.View.Hasfavouriteserverurl() && PersonalDapServerModelNode.PersonalDapRunning && oSource.View.favouriteserverurl.Value.Equals(Geosoft.GX.DAPGetData.Server.PERSONAL_DAP_URI))
				{
					SetFavouriteServer(PersonalDapServer, false);
				}

				OnLoaded(EventArgs.Empty);
			}
		}

		private ServerModelNode LoadBuilderEntryType(dappleview.builderentryType entry, Uri favouriteServerUri)
		{
			bool DontUpdateHomeView = false;
			bool DontSubmitToDappleSearch = false;
			ServerModelNode result = null;

			if (entry.Hasbuilderdirectory())
				for (int i = 0; i < entry.builderdirectory.builderentryCount; i++)
				{
					ServerModelNode newServer = LoadBuilderEntryType(entry.builderdirectory.GetbuilderentryAt(i), favouriteServerUri);
					if (newServer != null)
					{
						result = newServer;
					}
				}
			else if (entry.Haswmscatalog())
			{
				ServerModelNode newServer = AddWMSServer(new WMSServerUri(entry.wmscatalog.capabilitiesurl.Value), entry.wmscatalog.Hasenabled() ? entry.wmscatalog.enabled.Value : true, DontUpdateHomeView, DontSubmitToDappleSearch);
				if (favouriteServerUri != null && newServer.Uri.ToBaseUri().Equals(favouriteServerUri.ToString()))
				{
					result = newServer;
				}
			}
			else if (entry.Hasarcimscatalog())
			{
				ServerModelNode newServer = AddArcIMSServer(new ArcIMSServerUri(entry.arcimscatalog.capabilitiesurl.Value), entry.arcimscatalog.Hasenabled() ? entry.arcimscatalog.enabled.Value : true, DontUpdateHomeView, DontSubmitToDappleSearch);
				if (favouriteServerUri != null && newServer.Uri.ToBaseUri().Equals(favouriteServerUri.ToString()))
				{
					result = newServer;
				}
			}
			else if (entry.Hasdapcatalog())
			{
				DapServerUri oUri = new DapServerUri(entry.dapcatalog.url.Value);
				if (!oUri.IsForPersonalDAP)
				{
					ServerModelNode newServer = AddDAPServer(oUri, entry.dapcatalog.Hasenabled() ? entry.dapcatalog.enabled.Value : true, DontUpdateHomeView, DontSubmitToDappleSearch);
					if (favouriteServerUri != null && newServer.Uri.ToBaseUri().Equals(favouriteServerUri.ToString()))
					{
						result = newServer;
					}
				}
			}
			else if (entry.Hastileserverset())
			{
				LoadTileServerSet(entry.tileserverset);
			}

			return result;
		}

		private void LoadTileServerSet(dappleview.tileserversetType entry)
		{
			if (entry.Hastilelayers())
			{
				for (int i = 0; i < entry.tilelayers.tilelayerCount; i++)
				{
					dappleview.tilelayerType oLayer = entry.tilelayers.GettilelayerAt(i);
					dappleview.boundingboxType oBoundsData = oLayer.boundingbox;
					GeographicBoundingBox oBounds = new GeographicBoundingBox(
						oBoundsData.maxlat.Value,
						oBoundsData.minlat.Value,
						oBoundsData.minlon.Value,
						oBoundsData.maxlon.Value);


					ImageTileLayerModelNode oNode = new ImageTileLayerModelNode(
						this,
						oLayer.name.Value,
						new Uri(oLayer.url.Value),
						oLayer.imageextension.Value,
						oLayer.levelzerotilesize.Value,
						oLayer.dataset.Value,
						oLayer.levels.Value,
						oBounds,
						oLayer.Hasdistanceabovesurface() ? oLayer.distanceabovesurface.Value : Convert.ToInt32(dappleview.tilelayerType.GetdistanceabovesurfaceDefault(), CultureInfo.InvariantCulture),
						oLayer.Hastilepixelsize() ? oLayer.tilepixelsize.Value : Convert.ToInt32(dappleview.tilelayerType.GettilepixelsizeDefault(), CultureInfo.InvariantCulture)
						);

					this.AddImageTileLayer(entry.name.Value, oNode);
				}
			}
		}

		#endregion

		#region Adding Servers

		internal void AddArcIMSServer()
		{
			AddArcIMS oDialog = new AddArcIMS();

			if (oDialog.ShowDialog() == DialogResult.OK)
			{
				ModelNode result = AddArcIMSServer(new ArcIMSServerUri(oDialog.URL), true, true, true);
				SelectedNode = result;
			}
		}

		internal ServerModelNode AddArcIMSServer(ArcIMSServerUri oUri, bool blEnabled, bool blUpdateHomeView, bool blSubmitToDappleSearch)
		{
			lock (m_oLock)
			{
				// --- Don't add the server if it's already in the model ---

				ServerModelNode result = m_oRootNode.ArcIMSServers.GetServer(oUri);
				if (result != null)
				{
					return result;
				}

				// --- Add the server ---

				result = m_oRootNode.ArcIMSServers.AddServer(oUri, blEnabled);

				// --- Update home view if necessary ---

				if (blUpdateHomeView)
				{
					result.AddToHomeView();
				}

				// --- Submit to DappleSearch if necessary ---

				if (blSubmitToDappleSearch)
				{
					SubmitServerToSearchEngine(oUri.ToBaseUri(), ServerModelNode.ServerType.ArcIMS);
				}

				OnServerAdded(EventArgs.Empty);
				return result;
			}
		}

		internal void AddWMSServer()
		{
			AddWMS oDialog = new AddWMS();

			if (oDialog.ShowDialog() == DialogResult.OK)
			{
				ModelNode result = AddWMSServer(new WMSServerUri(oDialog.WmsURL), true, true, true);
				SelectedNode = result;
			}
		}

		internal ServerModelNode AddWMSServer(WMSServerUri oUri, bool blEnabled, bool blUpdateHomeView, bool blSubmitToDappleSearch)
		{
			lock (m_oLock)
			{
				// --- Don't add the server if it's already in the model ---

				ServerModelNode result = m_oRootNode.WMSServers.GetServer(oUri);
				if (result != null)
				{
					return result;
				}

				// --- Add the server ---

				result = m_oRootNode.WMSServers.AddServer(oUri, blEnabled);

				// --- Update home view if necessary ---

				if (blUpdateHomeView)
				{
					result.AddToHomeView();
				}

				// --- Submit to DappleSearch if necessary ---

				if (blSubmitToDappleSearch)
				{
					SubmitServerToSearchEngine(oUri.ToBaseUri(), ServerModelNode.ServerType.WMS);
				}

				OnServerAdded(EventArgs.Empty);
				return result;
			}
		}

		internal void AddDAPServer()
		{
			AddDAP oDialog = new AddDAP();

			if (oDialog.ShowDialog() == DialogResult.OK)
			{
				ModelNode result = AddDAPServer(new DapServerUri(oDialog.Url), true, true, true);
				SelectedNode = result;
			}
		}

		internal ServerModelNode AddDAPServer(DapServerUri oUri, bool blEnabled, bool blUpdateHomeView, bool blSubmitToDappleSearch)
		{
			lock (m_oLock)
			{
				// --- Don't add 
				if (oUri.IsForPersonalDAP)
				{
					return m_oRootNode.PersonalDapServer;
				}

				// --- Don't add the server if it's already in the model ---

				ServerModelNode result = m_oRootNode.DAPServers.GetServer(oUri);
				if (result != null)
				{
					return result;
				}

				// --- Add the server ---

				result = m_oRootNode.DAPServers.AddServer(oUri, blEnabled);

				// --- Update home view if necessary ---

				if (blUpdateHomeView)
				{
					result.AddToHomeView();
				}

				// --- Submit to DappleSerach if necessary ---

				if (blSubmitToDappleSearch)
				{
					SubmitServerToSearchEngine(oUri.ToBaseUri(), ServerModelNode.ServerType.DAP);
				}


				OnServerAdded(EventArgs.Empty);
				return result;
			}
		}

		internal void AddImageTileLayer(String strTileSetName, ImageTileLayerModelNode oLayer)
		{
			lock (m_oLock)
			{
				// --- Get the tileset to add to ---

				ImageTileSetModelNode oSet = m_oRootNode.ImageTileSets.GetImageTileSet(strTileSetName);

				// --- Add the tileset ---

				oSet.AddLayer(oLayer);
			}
		}

		internal void SetSearchFilter(String strKeyword, GeographicBoundingBox oBounds)
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
					if (m_oRootNode.PersonalDapServer != null)
					{
						m_oRootNode.PersonalDapServer.SearchFilterChanged();
					}


					OnSearchFilterChanged(EventArgs.Empty);
				}
			}
		}

		internal void SetFavouriteServer(ServerModelNode oServer, bool blUpdateHomeView)
		{
			lock (m_oLock)
			{
				if (m_oFavouriteServer == null || !m_oFavouriteServer.Uri.ToBaseUri().Equals(oServer.Uri.ToBaseUri()))
				{
					m_oFavouriteServer = m_oRootNode.SetFavouriteServer(oServer.Uri.ToBaseUri());

					if (blUpdateHomeView)
					{
						oServer.AddToHomeView();
						HomeView.SetFavourite(m_oFavouriteServer.Uri);
					}

					m_oSelectedNode = m_oFavouriteServer;
					OnFavouriteServerChanged(EventArgs.Empty);
				}
			}
		}

		internal void ToggleServer(ServerModelNode oServer, bool blUpdateHomeView)
		{
			lock (m_oLock)
			{
				oServer.ToggleEnabled();

				if (blUpdateHomeView)
				{
					oServer.AddToHomeView();
					HomeView.SetServerEnabled(oServer.Uri, oServer.Enabled);
				}

				OnServerToggled(EventArgs.Empty);
			}
		}

		internal void RemoveServer(ServerModelNode oServer, bool blUpdateHomeView)
		{
			lock (m_oLock)
			{
				oServer.Parent.RemoveChild(oServer);

				if (blUpdateHomeView)
				{
					if (oServer.Favourite)
					{
						HomeView.ClearFavourite();
					}

					HomeView.RemoveServer(oServer.Uri);
				}
			}
		}

		#endregion

		internal void DoWithLock(MethodInvoker oOperation)
		{
			lock (m_oLock)
			{
				oOperation();
			}
		}

		internal void DoWithLock(WaitCallback oOperation, Object oParam)
		{
			lock (m_oLock)
			{
				oOperation(oParam);
			}
		}

		internal void ModelNodeLoaded(ModelNode oLoadedNode)
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

		internal void ModelNodeAdded(ModelNode oParentNode, ModelNode oChildNode)
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

		internal void ModelNodeRemoved(ModelNode oParentNode, ModelNode oChildNode)
		{
			if (IsSelectedOrAncestor(oChildNode))
			{
				SelectedNode = oParentNode;
			}

			OnNodeRemoved(new NodeRemovedEventArgs(oParentNode, oChildNode));
		}

		internal void ModelNodeDisplayUpdated(ModelNode oUpdatedNode)
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

		internal void ModelNodeUnloaded(ModelNode oNode)
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

		internal bool IsSelectedOrAncestor(ModelNode oNode)
		{
			for (ModelNode iter = m_oSelectedNode; iter != null; iter = iter.Parent)
			{
				if (iter == oNode) return true;
			}

			return false;
		}

		#endregion


		#region Helper Methods
		private void ClearModel()
		{
			lock (m_oLock)
			{
				m_oRootNode.Clear();
			}

			m_oSelectedNode = m_oRootNode;
		}

		/// <remarks>
		/// TODO: Move this into a separate DappleSearchInterface class, along with the communications in the
		/// DappleSearch list.
		/// </remarks>
		private static void SubmitServerToSearchEngine(String strUrl, NewServerTree.ServerModelNode.ServerType eType)
		{
#if !DEBUG
			if (!MainApplication.Settings.UseDappleSearch) return;

			DappleSearchAddServerDownload oSubmission = new DappleSearchAddServerDownload(strUrl, eType);
			oSubmission.BackgroundDownloadMemory();
#endif
		}

		private class DappleSearchAddServerDownload : WebDownload
		{
			#region Member Variables

			private String m_strNewServerUrl;
			private NewServerTree.ServerModelNode.ServerType m_eType;

			#endregion


			#region Constructor

			internal DappleSearchAddServerDownload(String strUrl, NewServerTree.ServerModelNode.ServerType eType)
			{
				m_strNewServerUrl = strUrl;
				m_eType = eType;

				this.Url = MainForm.Settings.DappleSearchURL + "AddNewServer.aspx";
				this.CompleteCallback = new DownloadCompleteHandler(DownloadComplete);
			}

			#endregion


			#region WebDownload Overrides

			protected override void Download()
			{
				// --- Ensure that this download only happens asynchronously ---
				throw new ApplicationException("Don't perform a DappleSearchAddServerDownload download synchronously");
			}

			protected override HttpWebRequest BuildRequest()
			{
				HttpWebRequest result = base.BuildRequest();

				request.Headers["GeosoftAddServerRequest"] = CreateAddServerXML();

				return result;
			}

			#endregion


			#region Helper Methods

			private string CreateAddServerXML()
			{
				return "<geosoft_xml><add_server url=\"" + m_strNewServerUrl + "\" type=\"" + m_eType.ToString() + "\"/></geosoft_xml>";
			}

			private void DownloadComplete(WebDownload oDownload)
			{
				// --- Sending it is what matters, we don't care what comes back ---
				oDownload.Dispose();
			}

			#endregion
		}
		#endregion
	}

	/// <summary>
	/// Class representing the datasets that are visible on the globe.
	/// </summary>
	/// <remarks>
	/// This is currently a proxy for converting ModelNodes to LayerBuilders
	/// to isolate the model from the rest of the system. 
	/// </remarks>
	internal class ViewedDatasetsModel
	{
		#region Member Variables

		internal LayerList m_oLayerList;

		#endregion


		#region Events

		internal event EventHandler LayersRemoved;
		protected void OnLayersRemoved(EventArgs e)
		{
			if (LayersRemoved != null)
			{
				LayersRemoved(this, e);
			}
		}

		internal event EventHandler LayersAdded;
		protected void OnLayersAdded(EventArgs e)
		{
			if (LayersAdded != null)
			{
				LayersAdded(this, e);
			}
		}

		#endregion


		#region Constructors

		internal ViewedDatasetsModel(LayerList oTarget)
		{
			m_oLayerList = oTarget;
		}

		#endregion


		#region Public Methods

		internal void Add(LayerModelNode oNewLayer)
		{
#pragma warning disable 618
			m_oLayerList.AddLayer(oNewLayer.ConvertToLayerBuilder());
#pragma warning restore 618
			OnLayersAdded(EventArgs.Empty);
		}

		internal bool Contains(LayerModelNode oLayer)
		{
#pragma warning disable 618
			return m_oLayerList.AllLayers.Contains(oLayer.ConvertToLayerBuilder());
#pragma warning restore 618
		}

		internal void Remove(List<LayerModelNode> oLayersToRemove)
		{
			OnLayersRemoved(EventArgs.Empty);
		}

		#endregion
	}
}
