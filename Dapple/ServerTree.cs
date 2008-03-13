using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Windows.Forms;
using Dapple.LayerGeneration;
using dappleview;
using Altova.Types;

using Geosoft.GX.DAPGetData;

using Geosoft.Dap;
using Geosoft.Dap.Common;
using Geosoft.DotNetTools;

using WorldWind.PluginEngine;
using System.Collections;
using System.Drawing;

namespace Dapple
{
	/// <summary>
	/// Derived tree that not only contains DAP servers but also WMS and image tile servers
	/// </summary>
   public class ServerTree : Geosoft.GX.DAPGetData.ServerTree
	{
		#region Members
		protected string m_strSearch = String.Empty;
		protected WorldWind.GeographicBoundingBox m_filterExtents;

		protected TreeNode m_hRootNode;
		protected TreeNode m_hTileRootNode;
		protected TreeNode m_hVERootNode;
		protected TreeNode m_hWMSRootNode;
      protected TreeNode m_hArcIMSRootNode;

		//TreeNodeSorter m_TreeSorter;

		protected MainForm m_oParent;
		protected LayerList m_activeLayers;
      protected ServerList m_oServerListControl;
		protected List<ServerBuilder> m_wmsServers = new List<ServerBuilder>();
      private ToolStripMenuItem cMenuItem_AddServer;
      protected List<ServerBuilder> m_oArcIMSServers = new List<ServerBuilder>();
      private ToolStripMenuItem cMenuItem_SetDefault;

      private String m_szDefaultServer = String.Empty;

      public event MainForm.ViewMetadataHandler ViewMetadata;
		#endregion

      #region Delegates
      public delegate void LoadFinishedCallbackHandler();
      #endregion

      #region Constructor/Disposal
      /// <summary>
		/// Constructor
		/// </summary>
		/// <param name="strCacheDir"></param>
      public ServerTree(ImageList oImageList, string strCacheDir, MainForm oParent, LayerList activeLayers, ServerList oServerList)
			: base(oImageList, strCacheDir)
		{
         InitializeComponent();

         m_oParent = oParent;
			m_activeLayers = activeLayers;
         m_oServerListControl = oServerList;
         m_activeLayers.ActiveLayersChanged += new LayerList.ActiveLayersChangedHandler(UpdateTreeNodeColors);
			this.SupportDatasetSelection = false;
			this.AfterCollapse += new TreeViewEventHandler(OnAfterCollapse);

         m_hRootNode = new TreeNode("Available Servers", Dapple.MainForm.ImageListIndex("dapple"), Dapple.MainForm.ImageListIndex("dapple"));
			this.Nodes.Add(m_hRootNode);
			m_hRootNode.ToolTipText = "It is possible to double-click on layers in\nhere to add them to the current layers.\nSingle-click browses tree.";

         m_hDAPRootNode = new TreeNode("DAP Servers", Dapple.MainForm.ImageListIndex("dap"), Dapple.MainForm.ImageListIndex("dap"));
			m_hRootNode.Nodes.Add(m_hDAPRootNode);

         m_hTileRootNode = new TreeNode("Image Tile Servers", Dapple.MainForm.ImageListIndex("tile"), Dapple.MainForm.ImageListIndex("tile"));
			m_hRootNode.Nodes.Add(m_hTileRootNode);

         VETileSetBuilder veDir = new VETileSetBuilder("Virtual Earth", null, false);
			m_hVERootNode = m_hRootNode.Nodes.Add("Virtual Earth");
         m_hVERootNode.SelectedImageIndex = m_hVERootNode.ImageIndex = Dapple.MainForm.ImageListIndex("live");
			m_hVERootNode.Tag = veDir;

         VEQuadLayerBuilder m_VEMapQTB = new VEQuadLayerBuilder("Virtual Earth Map", WorldWind.VirtualEarthMapType.road, m_oParent.WorldWindow, true, veDir);
         VEQuadLayerBuilder m_VESatQTB = new VEQuadLayerBuilder("Virtual Earth Satellite", WorldWind.VirtualEarthMapType.aerial, m_oParent.WorldWindow, true, veDir);
         VEQuadLayerBuilder m_VEMapAndSatQTB = new VEQuadLayerBuilder("Virtual Earth Map & Satellite", WorldWind.VirtualEarthMapType.hybrid, m_oParent.WorldWindow, true, veDir);
			veDir.LayerBuilders.Add(m_VEMapQTB);
			veDir.LayerBuilders.Add(m_VESatQTB);
			veDir.LayerBuilders.Add(m_VEMapAndSatQTB);

         WMSCatalogBuilder wmsBuilder = new WMSCatalogBuilder("WMS Servers", m_oParent.WorldWindow, null);
			wmsBuilder.LoadFinished += new LoadFinishedCallbackHandler(OnLoadFinished);

         m_hWMSRootNode = new TreeNode("WMS Servers", Dapple.MainForm.ImageListIndex("wms"), Dapple.MainForm.ImageListIndex("wms"));
			m_hWMSRootNode.Tag = wmsBuilder;
			m_hRootNode.Nodes.Add(m_hWMSRootNode);

         ArcIMSCatalogBuilder arcIMSBuilder = new ArcIMSCatalogBuilder("ArcIMS Servers", m_oParent.WorldWindow, null);
         arcIMSBuilder.LoadFinished += new LoadFinishedCallbackHandler(OnLoadFinished);

         m_hArcIMSRootNode = new TreeNode("ArcIMS Servers", Dapple.MainForm.ImageListIndex("arcims"), Dapple.MainForm.ImageListIndex("arcims"));
         m_hArcIMSRootNode.Tag = arcIMSBuilder;
         m_hRootNode.Nodes.Add(m_hArcIMSRootNode);

			//m_TreeSorter = new TreeNodeSorter(this);
         this.TreeViewNodeSorter = new TreeNodeSorter(this);

         this.MouseMove += new MouseEventHandler(this.HandleMouseMove);
         this.MouseDown += new MouseEventHandler(this.HandleMouseDown);
         this.AllowDrop = false;
         this.BorderStyle = BorderStyle.FixedSingle;
		}
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
		}
		#endregion

      #region Event Handlers

      private System.Drawing.Point m_oDragDropStartPoint = System.Drawing.Point.Empty;

      private TreeNode m_oDragSource = null;
      private void HandleMouseDown(Object oSender, MouseEventArgs oArgs)
      {
         TreeNode oNodeOver = this.GetNodeAt(oArgs.X, oArgs.Y);

         if (oNodeOver != null && (oNodeOver.Tag is DataSet || oNodeOver.Tag is LayerBuilder))
         {
            m_oDragDropStartPoint = new System.Drawing.Point(oArgs.X, oArgs.Y);
            m_oDragSource = oNodeOver;
         }
      }

      private void HandleMouseMove(Object oSender, MouseEventArgs oArgs)
      {
         //Check that the user is doing a click-hold AND a drag.  If you don't, you'll make the event thread behave strangely.
         if ((oArgs.Button & MouseButtons.Left) == MouseButtons.Left && (oArgs.X != m_oDragDropStartPoint.X || oArgs.Y != m_oDragDropStartPoint.Y))
         {
            if (m_oDragSource == null) return;
            List<LayerBuilder> oDragData = new List<LayerBuilder>();

            if (m_oDragSource.Tag is LayerBuilder)
            {
               oDragData.Add(m_oDragSource.Tag as LayerBuilder);
            }
            else if (m_oDragSource.Tag is DataSet)
            {
               // --- Get the DataSet's Server parent node ---
               TreeNode oServerNode = this.SelectedNode;
               while (!(oServerNode.Tag is Server)) oServerNode = oServerNode.Parent;

               oDragData.Add(new DAPQuadLayerBuilder(m_oDragSource.Tag as DataSet, m_oParent.WorldWindow, oServerNode.Tag as Server, null));
            }

            if (oDragData.Count > 0)
            {
               DragDropEffects dropEffect = this.DoDragDrop(oDragData, DragDropEffects.All);
               this.SelectedNode = m_oDragSource;
            }

            m_oDragSource = null;
            m_oDragDropStartPoint = System.Drawing.Point.Empty;
         }
      }

      #endregion

      #region Properties

      public WMSCatalogBuilder WMSCatalog
      {
         get { return m_hWMSRootNode.Tag as WMSCatalogBuilder; }
      }

      public ArcIMSCatalogBuilder ArcIMSCatalog
      {
         get { return m_hArcIMSRootNode.Tag as ArcIMSCatalogBuilder; }
      }

		/// <summary>
		/// The root node collection to use where WMS servers are kept
		/// </summary>
		public TreeNodeCollection WMSRootNodes
		{
			get
			{
				return m_hWMSRootNode.Nodes;
			}
		}

      /// <summary>
      /// The root node collection to use where ArcIMS servers are kept.
      /// </summary>
      public TreeNodeCollection ArcIMSRootNodes
      {
         get
         {
            return m_hArcIMSRootNode.Nodes;
         }
      }

		/// <summary>
		/// The root node collection to use where tile servers are kept
		/// </summary>
		public TreeNodeCollection TileRootNodes
		{
			get
			{
				return m_hTileRootNode.Nodes;
			}
		}

		/// <summary>
		/// The root node collection to use where Virtual Earth is kept
		/// </summary>
		public TreeNodeCollection VERootNodes
		{
			get
			{
				return m_hVERootNode.Nodes;
			}
		}

		public TreeNode RootNode
		{
			get
			{
				return m_hRootNode;
			}
		}

      public Object SelectedServer
      {
         get
         {
            TreeNode iter = this.SelectedOrRoot;
            while (iter != this.RootNode)
            {
               if (iter.Tag is Server || iter.Tag is ServerBuilder) return iter.Tag;

               iter = iter.Parent;
            }
            return null;
         }
         set
         {
				if (value == this.SelectedServer) return; // Don't do anything if it's the same server

            if (value is Server)
            {
               this.SelectedNode = m_hDAPRootNode;
               foreach (TreeNode oNode in DAPRootNodes)
               {
                  if (oNode.Tag.Equals(value))
                  {
                     this.SelectedNode = oNode;
                     break;
                  }
               }
            }
            else if (value is ArcIMSServerBuilder)
            {
               this.SelectedNode = m_hArcIMSRootNode;
               foreach (TreeNode oNode in ArcIMSRootNodes)
               {
                  if (oNode.Tag.Equals(value))
                  {
                     this.SelectedNode = oNode;
                     break;
                  }
               }
            }
            else if (value is WMSServerBuilder)
            {
               this.SelectedNode = m_hWMSRootNode;
               foreach (TreeNode oNode in WMSRootNodes)
               {
                  if (oNode.Tag.Equals(value))
                  {
                     this.SelectedNode = oNode;
                     break;
                  }
               }
            }
         }
      }

		public bool SelectedIsFavorite
		{
			get
			{
				if (SelectedNode.Tag is Server)
				{
					return ((Server)SelectedNode.Tag).Url.Equals(m_szDefaultServer);
				}
				else if (SelectedNode.Tag is ServerBuilder)
				{
					return ((ServerBuilder)SelectedNode.Tag).Uri.ToString().Equals(m_szDefaultServer);
				}
				else
				{
					return false;
				}
			}
		}

		#endregion

		#region Catalog Load Handler

      void OnLoadFinished()
      {
         if (!this.IsDisposed)
         {
            if (InvokeRequired)
               this.BeginInvoke(new LoadFinishedCallbackHandler(LoadFinished));
            else
               LoadFinished();
         }
      }

      void LoadFinished()
      {
         m_oServerListControl.Servers = getServerList();

			ReconstructTree();
      }

		/// <summary>
		/// Rebuilds tree when no change has been made to the selected node.
		/// </summary>
		private void ReconstructTree()
		{
			oPreNode = SelectedOrRoot;
			oPostNode = SelectedOrRoot;
			RebuildTreeAfterChange();
		}

		#endregion

		#region Methods

		public XmlNode GetCurrentDAPMetaData(XmlDocument oDoc)
		{
			if (this.SelectedDAPDataset != null)
			{
				XmlDocument responseDoc = m_oCurServer.Command.GetMetaData(SelectedDAPDataset, null);
				XmlNode oNode = responseDoc.DocumentElement.FirstChild.FirstChild.FirstChild;
				XmlNode metaNode = oDoc.CreateElement("dapmeta");
				XmlNode nameNode = oDoc.CreateElement("name");
				nameNode.InnerText = this.SelectedDAPDataset.Title;
				metaNode.AppendChild(nameNode);
				XmlNode geoMetaNode = oDoc.CreateElement(oNode.Name);
				geoMetaNode.InnerXml = oNode.InnerXml;
				metaNode.AppendChild(geoMetaNode);
				return metaNode;
			}
			else
				return null;
		}

		public void AddCurrentDataset()
		{
			if (this.SelectedDAPDataset != null)
			{
				DAPQuadLayerBuilder layerBuilder = new DAPQuadLayerBuilder(this.SelectedDAPDataset, m_oParent.WorldWindow, m_oCurServer, null);
				m_oParent.AddLayerBuilder(layerBuilder);
			}
			else if (this.SelectedNode != null && SelectedNode.Tag is LayerBuilder)
				m_oParent.AddLayerBuilder(SelectedNode.Tag as LayerBuilder);
		}

      /// <summary>
      /// Remove and re-add a server, causing its data to be re-downloaded.
      /// </summary>
		public void RefreshCurrentServer()
		{
         Object oCurrentServer = this.SelectedServer;
			if (oCurrentServer  == null) return;

			SuspendLayout();

         if (oCurrentServer is WMSServerBuilder)
			{
            WMSServerBuilder serverBuilder = oCurrentServer as WMSServerBuilder;
            if (serverBuilder.Enabled)
            {
               RemoveCurrentServer();
               AddWMSServer(serverBuilder.Uri.ToBaseUri(), true, serverBuilder.Enabled, true);
               foreach (TreeNode oNode in m_hWMSRootNode.Nodes)
               {
                  if (((WMSServerBuilder)oNode.Tag).Uri.Equals(serverBuilder.Uri))
                  {
                     this.SelectedNode = oNode;
                     break;
                  }
               }
            }
			}
         else if (oCurrentServer is ArcIMSServerBuilder)
         {
            ArcIMSServerBuilder serverBuilder = oCurrentServer as ArcIMSServerBuilder;
            if (serverBuilder.Enabled)
            {
               RemoveCurrentServer();
               AddArcIMSServer(serverBuilder.Uri as ArcIMSServerUri, true, serverBuilder.Enabled, true);
               foreach (TreeNode oNode in m_hArcIMSRootNode.Nodes)
               {
                  if (((ArcIMSServerBuilder)oNode.Tag).Uri.Equals(serverBuilder.Uri))
                  {
                     this.SelectedNode = oNode;
                     break;
                  }
               }
            }
         }
         else if (oCurrentServer is Server)
         {
            if (((Server)oCurrentServer).Enabled)
            {
					// GetCatalogHierarchy will UpdateConfiguration if necessary, shouldn't need it here.
               //(oCurrentServer as Server).UpdateConfiguration();
               ClearCatalog();
               GetCatalogHierarchy();
               GetDatasetCount(oCurrentServer as Server);
               RefreshTreeNodeText();
            }
         }

			ResumeLayout();
		}

		public void RemoveCurrentServer()
		{
			Object oCurrentServer = this.SelectedServer;

         if (oCurrentServer == null)
				return;

         try
         {
            if (oCurrentServer is Server)
            {
               RemoveServer(oCurrentServer as Server);
					m_oParent.CmdUpdateHomeView(MainForm.UpdateHomeViewType.RemoveServer, new Object[] { (oCurrentServer as Server).Url, "DAP" });
               return;
            }
            else if (oCurrentServer is WMSServerBuilder)
            {
               WMSServerBuilder serverBuilder = oCurrentServer as WMSServerBuilder;
               m_wmsServers.Remove(serverBuilder);
					m_oParent.CmdUpdateHomeView(MainForm.UpdateHomeViewType.RemoveServer, new Object[] { (oCurrentServer as WMSServerBuilder).Uri.ToString(), "WMS" });
               ((WMSCatalogBuilder)m_hWMSRootNode.Tag).UncacheServer(serverBuilder.Uri as WMSServerUri);
					this.SelectedNode = m_hWMSRootNode;
            }
            else if (oCurrentServer is ArcIMSServerBuilder)
            {
               ArcIMSServerBuilder serverBuilder = oCurrentServer as ArcIMSServerBuilder;
               m_oArcIMSServers.Remove(serverBuilder);
					m_oParent.CmdUpdateHomeView(MainForm.UpdateHomeViewType.RemoveServer, new Object[] { (oCurrentServer as ArcIMSServerBuilder).Uri.ToString(), "ArcIMS" });
               ((ArcIMSCatalogBuilder)m_hArcIMSRootNode.Tag).UncacheServer(serverBuilder.Uri as ArcIMSServerUri);
					this.SelectedNode = m_hArcIMSRootNode;
            }
         }
         finally
         {
            m_oServerListControl.Servers = getServerList();
         }
		}

		public bool AddWMSServer(string strCapUrl, bool bUpdateTree, bool blEnabled, bool blUpdateHomeView)
		{
			WMSCatalogBuilder wmsBuilder = m_hWMSRootNode.Tag as WMSCatalogBuilder;
			WMSServerUri oUri = new WMSServerUri(strCapUrl);
         if (wmsBuilder.ContainsServer(oUri))
				return false;// Don't add a server multiple times

         WMSServerBuilder builder = wmsBuilder.AddServer(oUri, blEnabled) as WMSServerBuilder;
			if (builder != null && blUpdateHomeView)
			{
				m_oParent.CmdUpdateHomeView(MainForm.UpdateHomeViewType.AddServer, new Object[] { builder.Uri.ToString(), "WMS" });
			}
			m_wmsServers.Add(builder);

			if (bUpdateTree)
			{
            this.SelectedServer = builder;
			}
         else if (m_szDefaultServer.Equals(strCapUrl))
         {
            this.SelectedServer = builder;
         }
         return true;
		}

      public bool AddArcIMSServer(ArcIMSServerUri serverUri, bool bUpdateTree, bool blEnabled, bool blUpdateHomeView)
      {
         ArcIMSCatalogBuilder arcimsBuilder = m_hArcIMSRootNode.Tag as ArcIMSCatalogBuilder;
         if (arcimsBuilder.ContainsServer(serverUri)) return false; // Don't add multiple times

         ArcIMSServerBuilder builderEntry = arcimsBuilder.AddServer(serverUri, blEnabled) as ArcIMSServerBuilder;
			if (builderEntry != null && blUpdateHomeView)
			{
				m_oParent.CmdUpdateHomeView(MainForm.UpdateHomeViewType.AddServer, new Object[] { builderEntry.Uri.ToString(), "ArcIMS" });
			}
         m_oArcIMSServers.Add(builderEntry);

         if (bUpdateTree)
         {
            this.SelectedServer = builderEntry;
         }
         else if (m_szDefaultServer.Equals(serverUri.ToString()))
         {
            this.SelectedServer = builderEntry;
         }
         return true;
      }

		public void LoadFromView(DappleView oView)
		{
			this.BeginUpdate();

			try
			{
				// Clear Tree and WMS servers too
				/*WMSCatalogBuilder wmsBuilder = m_hWMSRootNode.Tag as WMSCatalogBuilder;
            wmsBuilder.cancelDownloads();
				wmsBuilder.LoadFinished -= new LoadFinishedCallbackHandler(OnLoadFinished);
            wmsBuilder = new WMSCatalogBuilder("WMS Servers", m_oParent.WorldWindow, null);
				m_hWMSRootNode.Tag = wmsBuilder;
				wmsBuilder.LoadFinished += new LoadFinishedCallbackHandler(OnLoadFinished);

            ArcIMSCatalogBuilder arcIMSBuilder = m_hArcIMSRootNode.Tag as ArcIMSCatalogBuilder;
            arcIMSBuilder.cancelDownloads();
            arcIMSBuilder.LoadFinished -= new LoadFinishedCallbackHandler(OnLoadFinished);
            arcIMSBuilder = new ArcIMSCatalogBuilder("ArcIMS Servers", m_oParent.WorldWindow, null);
            m_hArcIMSRootNode.Tag = arcIMSBuilder;
            arcIMSBuilder.LoadFinished += new LoadFinishedCallbackHandler(OnLoadFinished);*/

				/*foreach (TreeNode node in m_hRootNode.Nodes)
					node.Nodes.Clear();*/
				m_hTileRootNode.Nodes.Clear();

				// Reset First
				/*m_bEntireCatalogMode = false;
				m_bAOIFilter = false;
				m_bPrevAOIFilter = false;
				m_bTextFilter = false;
				m_bPrevTextFilter = false;
				m_bSelect = true;
				m_strSearchString = string.Empty;
				m_strCurSearchString = string.Empty;
				m_eMode = SearchModeEnum.All;
				m_ePrevMode = SearchModeEnum.All;
				m_oCurServer = null;
				m_hCurServerTreeNode = null;
				m_wmsServers.Clear();
            m_oArcIMSServers.Clear();
				m_oServerList.Clear();
				m_oFullServerList.Clear();
				m_oValidServerList.Clear();
				m_bSelect = false;
				this.SelectedNode = m_hRootNode;
				m_bSelect = true;*/
				// m_hRootNode.Text = strName;
				this.SelectedNode = m_hRootNode;

				//if (view.View.Hasnotes())
				//  m_hRootNode.ToolTipText = view.View.notes.Value;
				// else
				//   this.lblNotes.Text = strName;            

            if (oView.View.Hasfavouriteserverurl())
            {
               m_szDefaultServer = oView.View.favouriteserverurl.Value;
            }
            else
            {
               m_szDefaultServer = String.Empty;
            }
 
				if (oView.View.Hasservers())
				{
					for (int i = 0; i < oView.View.servers.builderentryCount; i++)
					{
						builderentryType entry = oView.View.servers.GetbuilderentryAt(i);
						LoadServers(entry);
					}
				}

				ClearSearch();
			}
			finally
			{
            if (String.IsNullOrEmpty(m_szDefaultServer))
            {
               // Always expand root node
               m_hRootNode.Expand();

               // Collapse the first level nodes and clean the subnodes (we can restore them using the IBuilder parent/client relations ships from here on)
               foreach (TreeNode node in m_hRootNode.Nodes)
               {
						if (node == m_hDAPRootNode)
						{
							m_blAllowCollapse = true;
							node.Collapse();
							m_blAllowCollapse = false;
						}
						else
							node.Nodes.Clear();
                  foreach (TreeNode subNode in node.Nodes)
                     subNode.Nodes.Clear();
               }
            }
				UpdateCounts();
				this.EndUpdate();
			}
		}

      public void SaveFavoritesList(String szFilename)
      {
         XmlDocument oDocument = new XmlDocument();
         bool blValidDefaultServer = false;

         XmlElement oRootElement = oDocument.CreateElement("favorites_list", oDocument.NamespaceURI);
         oDocument.AppendChild(oRootElement);

         XmlElement oDapElement = oDocument.CreateElement("dap_servers", oDocument.NamespaceURI);
         oRootElement.AppendChild(oDapElement);
         foreach (Server oServer in this.ServerList.Values)
         {
            XmlElement oServerElement = oDocument.CreateElement("dap_server", oDocument.NamespaceURI);
            oServerElement.SetAttribute("url", oServer.Url);

            oDapElement.AppendChild(oServerElement);

            if (oServer.Url.Equals(m_szDefaultServer))
            {
               blValidDefaultServer = true;
            }
         }

         XmlElement oWmsElement = oDocument.CreateElement("wms_servers", oDocument.NamespaceURI);
         oRootElement.AppendChild(oWmsElement);
         foreach (WMSServerBuilder oBuilder in ((WMSCatalogBuilder)m_hWMSRootNode.Tag).SubList)
         {
            XmlElement oServerElement = oDocument.CreateElement("wms_server", oDocument.NamespaceURI);
            oServerElement.SetAttribute("url", oBuilder.Uri.ToString());

            oWmsElement.AppendChild(oServerElement);

            if (oBuilder.Uri.ToString().Equals(m_szDefaultServer))
            {
               blValidDefaultServer = true;
            }
         }

         XmlElement oArcIMSElement = oDocument.CreateElement("arcims_servers", oDocument.NamespaceURI);
         oRootElement.AppendChild(oArcIMSElement);
         foreach (ArcIMSServerBuilder oBuilder in ((ArcIMSCatalogBuilder)m_hArcIMSRootNode.Tag).SubList)
         {
            XmlElement oServerElement = oDocument.CreateElement("arcims_server", oDocument.NamespaceURI);
            oServerElement.SetAttribute("url", oBuilder.Uri.ToString());

            oArcIMSElement.AppendChild(oServerElement);

            if (oBuilder.Uri.ToString().Equals(m_szDefaultServer))
            {
               blValidDefaultServer = true;
            }
         }

         /*XmlElement oTileElement = oDocument.CreateElement("tile_servers", oDocument.NamespaceURI);
         oRootElement.AppendChild(oTileElement);
         foreach (BuilderDirectory oDir in ((BuilderDirectory)m_hTileRootNode.Tag).SubList)
         {
            XmlElement oTileSetSetElement = oDocument.CreateElement("server_set");
            oTileSetSetElement.SetAttribute("name", oDir.Name);
            oTileElement.AppendChild(oTileSetSetElement);

            foreach (NltQuadLayerBuilder oBuilder in oDir.LayerBuilders)
            {
               XmlElement oServerElement = oDocument.CreateElement("tile_server");
               oBuilder.SaveToXml(oServerElement);
               oTileSetSetElement.AppendChild(oServerElement);
            }
         }*/

         if (blValidDefaultServer)
         {
            XmlElement oDefaultElement = oDocument.CreateElement("default_server", oDocument.NamespaceURI);
            oDefaultElement.SetAttribute("url", m_szDefaultServer);
            oRootElement.AppendChild(oDefaultElement);
         }

         oDocument.Save(szFilename);
      }
      /*
      public void LoadFavoritesList(String szFilename)
      {
         try
         {
            XmlDocument oDocument = new XmlDocument();
            oDocument.Load(szFilename);

            String szDefaultServer = String.Empty;

            XmlElement oDefaultElement = oDocument.SelectSingleNode("favorites_list/default_server") as XmlElement;
            if (oDefaultElement != null)
            {
               szDefaultServer = oDefaultElement.GetAttribute("url");
            }

            foreach (XmlElement oServerNode in oDocument.SelectNodes("/favorites_list/dap_servers/dap_server"))
            {
               Server oIgnored;
               this.AddDAPServer(oServerNode.GetAttribute("url"), out oIgnored);

               if (!String.IsNullOrEmpty(szDefaultServer) && oServerNode.GetAttribute("url").Equals(szDefaultServer))
               {
                  this.SelectedServer = oIgnored;
                  m_szDefaultServer = szDefaultServer;
               }
            }

            foreach (XmlElement oServerNode in oDocument.SelectNodes("/favorites_list/wms_servers/wms_server"))
            {
               this.AddWMSServer(oServerNode.GetAttribute("url"), false);

               if (!String.IsNullOrEmpty(szDefaultServer) && oServerNode.GetAttribute("url").Equals(szDefaultServer))
               {
                  WMSServerBuilder oBuilder = ((WMSCatalogBuilder)m_hWMSRootNode.Tag).GetServer(new WMSServerUri(oServerNode.GetAttribute("url")));
                  this.SelectedServer = oBuilder;

                  m_szDefaultServer = szDefaultServer;
               }
            }

            foreach (XmlElement oServerNode in oDocument.SelectNodes("/favorites_list/arcims_servers/arcims_server"))
            {
               this.AddArcIMSServer(new ArcIMSServerUri(oServerNode.GetAttribute("url")), false);

               if (!String.IsNullOrEmpty(szDefaultServer) && oServerNode.GetAttribute("url").Equals(szDefaultServer))
               {
                  ArcIMSServerBuilder oBuilder = ((ArcIMSCatalogBuilder)m_hArcIMSRootNode.Tag).GetServer(new ArcIMSServerUri(oServerNode.GetAttribute("url")));
                  this.SelectedServer = oBuilder;

                  m_szDefaultServer = szDefaultServer;
               }
            }

            if (String.IsNullOrEmpty(m_szDefaultServer))
            {
               this.SelectedNode = RootNode;
            }
         }
         catch (XmlException e)
         {
            MessageBox.Show(e.Message);
         }
      }
      */
		public void SaveToView(DappleView oView)
		{
			builderentryType entry;
			builderdirectoryType dir;
			serversType servers = oView.View.Newservers();

			entry = servers.Newbuilderentry();
			dir = entry.Newbuilderdirectory();
			dir.Addname(new SchemaString(m_hDAPRootNode.Text));
			dir.Addspecialcontainer(new SpecialDirectoryType("DAPServers"));
			foreach (string strDapUrl in m_oFullServerList.Keys)
			{
				builderentryType subentry = servers.Newbuilderentry();
				dapcatalogType dap = subentry.Newdapcatalog();
				dap.Addurl(new SchemaString(strDapUrl));
            dap.Addenabled(new SchemaBoolean(m_oFullServerList[strDapUrl].Enabled));
				subentry.Adddapcatalog(dap);
				dir.Addbuilderentry(subentry);
			}
			entry.Addbuilderdirectory(dir);
			servers.Addbuilderentry(entry);

			entry = servers.Newbuilderentry();
			entry.Addbuilderdirectory(m_hTileRootNode.Tag as builderdirectoryType);
			servers.Addbuilderentry(entry);
			entry = servers.Newbuilderentry();
			virtualearthType ve = entry.Newvirtualearth();
			ve.Addname(new SchemaString("Virtual Earth"));

			entry = servers.Newbuilderentry();
			dir = entry.Newbuilderdirectory();
			dir.Addname(new SchemaString(m_hWMSRootNode.Text));
			dir.Addspecialcontainer(new SpecialDirectoryType("WMSServers"));
			foreach (ServerBuilder builderEntry in m_wmsServers)
			{
				builderentryType subentry = servers.Newbuilderentry();
				wmscatalogType wms = subentry.Newwmscatalog();
				wms.Addcapabilitiesurl(new SchemaString(builderEntry.Uri.ToBaseUri()));
            wms.Addenabled(new SchemaBoolean(builderEntry.Enabled));
				subentry.Addwmscatalog(wms);
				dir.Addbuilderentry(subentry);
			}
         foreach (ServerBuilder builderEntry in m_oArcIMSServers)
         {
            builderentryType subentry = servers.Newbuilderentry();
            arcimscatalogType arcims = subentry.Newarcimscatalog();
            arcims.Addcapabilitiesurl(new SchemaString(builderEntry.Uri.ToBaseUri()));
            arcims.Addenabled(new SchemaBoolean(builderEntry.Enabled));
            subentry.Addarcimscatalog(arcims);
            dir.Addbuilderentry(subentry);
         }
			entry.Addbuilderdirectory(dir);
			servers.Addbuilderentry(entry);

			oView.View.Addservers(servers);

         if (!String.IsNullOrEmpty(m_szDefaultServer))
         {
            oView.View.Addfavouriteserverurl(new SchemaString(m_szDefaultServer));
         }
		}


		void LoadTileServerSetIntoNode(tileserversetType tileServerSet, TreeNode serverNode)
		{
         BuilderDirectory tileDir = new BuilderDirectory(tileServerSet.name.Value, null, false);
			if (tileServerSet.Hastilelayers())
			{
				for (int count = 0; count < tileServerSet.tilelayers.tilelayerCount; count++)
				{
					tilelayerType tile = tileServerSet.tilelayers.GettilelayerAt(count);
					TreeNode newServerChildNode = serverNode.Nodes.Add(tile.name.Value);
               newServerChildNode.SelectedImageIndex = newServerChildNode.ImageIndex = Dapple.MainForm.ImageListIndex("layer");

					int iDistance = tile.Hasdistanceabovesurface() ? tile.distanceabovesurface.Value : Convert.ToInt32(tilelayerType.GetdistanceabovesurfaceDefault());
					int iPixelSize = tile.Hastilepixelsize() ? tile.tilepixelsize.Value : Convert.ToInt32(tilelayerType.GettilepixelsizeDefault());
					NltQuadLayerBuilder quadBuilder = new NltQuadLayerBuilder(tile.name.Value, iDistance, true, new WorldWind.GeographicBoundingBox(tile.boundingbox.maxlat.Value, tile.boundingbox.minlat.Value, tile.boundingbox.minlon.Value, tile.boundingbox.maxlon.Value), tile.levelzerotilesize.Value, tile.levels.Value, iPixelSize, tile.url.Value,
															tile.dataset.Value, tile.imageextension.Value, 255, m_oParent.WorldWindow, tileDir);
					newServerChildNode.Tag = quadBuilder;
				}
			}
		}

		void LoadBuilderEntryIntoNode(builderentryType entry, TreeNode serverNode)
		{
			if (entry.Hasbuilderdirectory())
			{
				if (entry.builderdirectory.Hasspecialcontainer()) return;
				else
				{
					TreeNode newNode = serverNode.Nodes.Add(entry.builderdirectory.name.Value);
               newNode.SelectedImageIndex = newNode.ImageIndex = Dapple.MainForm.ImageListIndex("local");
					newNode.Tag = entry.builderdirectory;
				}
			}
			else if (entry.Hastileserverset())
			{
            TreeNode newNode = serverNode.Nodes.Add(entry.tileserverset.name.Value);
            newNode.SelectedImageIndex = newNode.ImageIndex = Dapple.MainForm.ImageListIndex("tile");
				newNode.Tag = entry.tileserverset;
			}
		}

		void LoadServers(builderentryType entry)
		{
			int i;

         if (entry.Hasbuilderdirectory())
         {
            if (entry.builderdirectory.Hasspecialcontainer())
            {
               if (entry.builderdirectory.specialcontainer.Value == "ImageServers")
                  m_hTileRootNode.Tag = entry.builderdirectory;
            }

            for (i = 0; i < entry.builderdirectory.builderentryCount; i++)
               LoadServers(entry.builderdirectory.GetbuilderentryAt(i));
         }
         else if (entry.Hasdapcatalog())
         {
            Geosoft.GX.DAPGetData.Server dapServer;
            AddDAPServer(entry.dapcatalog.url.Value, out dapServer, entry.dapcatalog.Hasenabled() ? entry.dapcatalog.enabled.Value : true, false);
         }
         else if (entry.Haswmscatalog())
            AddWMSServer(entry.wmscatalog.capabilitiesurl.Value, false, entry.wmscatalog.Hasenabled() ? entry.wmscatalog.enabled.Value : true, false);
         else if (entry.Hasarcimscatalog())
            AddArcIMSServer(new ArcIMSServerUri(entry.arcimscatalog.capabilitiesurl.Value), false, entry.arcimscatalog.Hasenabled() ? entry.arcimscatalog.enabled.Value : true, false);
      }
		#endregion

		#region Search

		public void ClearSearch()
		{
			this.BeginUpdate();

			AsyncFilterChanged(ServerTree.SearchModeEnum.All, null, string.Empty, false, false);
			m_filterExtents = null;
			m_strSearch = string.Empty;

			if (this.SelectedNode != null)
			{
				string strText = this.SelectedNode != null ? this.SelectedNode.Text : string.Empty;
				object tag = this.SelectedNode != null ? this.SelectedNode.Tag : null;
				this.SelectedNode = this.SelectedNode.Parent;
				TreeNode treeNode = TreeUtils.FindNodeDFS(m_hRootNode.Nodes, strText, tag);
				if (treeNode != null)
					this.SelectedNode = treeNode;
			}
			else
			{
				this.SelectedNode = m_hRootNode;
			}

			this.EndUpdate();
		}

      public void Search(WorldWind.GeographicBoundingBox extents, string strSearch)
      {
         Search(extents != null, extents, strSearch);
      }

		public void Search(bool bInterSect, WorldWind.GeographicBoundingBox extents, string strSearch)
		{
			this.BeginUpdate();

			AsyncFilterChanged(ServerTree.SearchModeEnum.All, bInterSect ? new BoundingBox(extents.East, extents.North, extents.West, extents.South) : null, strSearch, bInterSect, strSearch != string.Empty);
			m_filterExtents = extents;
			m_strSearch = strSearch;

			// Reselect parent node and then the node to refresh the results
			if (this.SelectedNode != null)
			{
				string strText = this.SelectedNode != null ? this.SelectedNode.Text : string.Empty;
				object tag = this.SelectedNode != null ? this.SelectedNode.Tag : null;
				this.SelectedNode = this.SelectedNode.Parent;
				TreeNode treeNode = TreeUtils.FindNodeDFS(m_hRootNode.Nodes, strText, tag);
				if (treeNode != null)
					this.SelectedNode = treeNode;
			}
			else
			{
				this.SelectedNode = m_hRootNode;
			}
			this.EndUpdate();
		}

		protected override void RefreshTreeNodeText()
		{
         // TODO: Switched from n^2 algorithm to doing filtering twice per server.  Is it faster to just use n^2?
			base.RefreshTreeNodeText();

			// Count Dap servers.
			int iCount = 0;
			foreach (TreeNode treeNode in m_hDAPRootNode.Nodes)
			{
				if ((treeNode.Tag as Server).DatasetCount > 0)
					iCount++;
			}
			m_hDAPRootNode.Text = "DAP Servers (" + iCount.ToString() + ")";

			// Count WMS servers.
			iCount = 0;
			foreach (WMSServerBuilder entry in m_wmsServers)
			{
				if (entry.IsLoading || entry.LoadingErrorOccurred || entry.iGetLayerCount(m_bAOIFilter, m_filterExtents, m_strSearch) > 0)
					iCount++;
			}
			m_hWMSRootNode.Text = "WMS Servers (" + iCount + ")";

         // Annotate the tree for loading WMS servers.
         foreach (TreeNode treeNode in m_hWMSRootNode.Nodes)
         {
            ((AsyncBuilder)treeNode.Tag).updateTreeNode(treeNode, this, m_bAOIFilter, m_filterExtents, m_strSearch);
         }

         iCount = 0;
         foreach (ArcIMSServerBuilder entry in m_oArcIMSServers)
         {
            if (entry.IsLoading || entry.LoadingErrorOccurred || entry.iGetLayerCount(m_bAOIFilter, m_filterExtents, m_strSearch) > 0)
               iCount++;
         }
         m_hArcIMSRootNode.Text = "ArcIMS Servers (" + iCount + ")";

         // Annotate tree for loading ArcIMS servers.
         foreach (TreeNode treeNode in m_hArcIMSRootNode.Nodes)
         {
            ((AsyncBuilder)treeNode.Tag).updateTreeNode(treeNode, this, m_bAOIFilter, m_filterExtents, m_strSearch);
         }
		}

		protected void FilterTreeNodes(TreeNode node)
		{
			if (m_strSearch == string.Empty && (m_filterExtents == null || !m_bAOIFilter))
				return;

			List<TreeNode> nodeList = new List<TreeNode>();
			foreach (TreeNode treeNode in node.Nodes)
				nodeList.Add(treeNode);

			foreach (TreeNode treeNode in nodeList)
			{
				if (treeNode.Tag is LayerBuilder)
				{
					LayerBuilder builder = treeNode.Tag as LayerBuilder;
					if ((m_strSearch != string.Empty && treeNode.Text.IndexOf(m_strSearch, 0, StringComparison.InvariantCultureIgnoreCase) == -1) ||
					  (m_filterExtents != null && m_bAOIFilter && !m_filterExtents.Intersects(builder.Extents) && !m_filterExtents.Contains(builder.Extents)))
						treeNode.Remove();
				}
				if (treeNode.Tag is BuilderDirectory && !(treeNode.Tag is WMSCatalogBuilder) && !(treeNode.Tag is ArcIMSCatalogBuilder) &&
				  (treeNode.Tag as BuilderDirectory).iGetLayerCount(m_bAOIFilter, m_filterExtents, m_strSearch) == 0)
				{
               // Don't remove loading or busted servers
               if (treeNode.Tag is AsyncBuilder && !((AsyncBuilder)treeNode.Tag).IsLoadedSuccessfully)
                  continue;
					
					treeNode.Remove();
				}
			}

			// Update counts accross the board
			UpdateCounts();
		}

		#endregion

		#region Event Handlers

      protected override void UpdateTreeNodeColors()
      {
         SuspendLayout();
         UpdateTreeNodeColors(RootNode);
         ResumeLayout();
      }

      private void UpdateTreeNodeColors(TreeNode oNode)
      {
         if (oNode.Tag is LayerBuilder && m_activeLayers.AllLayers.Contains(oNode.Tag as LayerBuilder))
         {
            oNode.ForeColor = System.Drawing.Color.Green;
         }
         else if (oNode.Tag is DataSet)
         {
            TreeNode iter = oNode;
            while (!(iter.Tag is Server))
            {
               iter = iter.Parent;
            }

            if (m_activeLayers.AllLayers.Contains(new DAPQuadLayerBuilder(oNode.Tag as DataSet, MainForm.WorldWindowSingleton, iter.Tag as Server, null)))
            {
               oNode.ForeColor = System.Drawing.Color.Green;
            }
            else
            {
               oNode.ForeColor = this.ForeColor;
            }
         }
			else if (oNode.Tag is ServerBuilder)
			{
				oNode.ForeColor = ((ServerBuilder)oNode.Tag).Enabled ? this.ForeColor : System.Drawing.Color.Gray;
				oNode.NodeFont = ((ServerBuilder)oNode.Tag).Uri.ToString().Equals(m_szDefaultServer) ? new Font(this.Font, FontStyle.Bold) : this.Font;
			}
			else if (oNode.Tag is Server)
			{
				oNode.ForeColor = ((Server)oNode.Tag).Enabled ? this.ForeColor : System.Drawing.Color.Gray;
				oNode.NodeFont = ((Server)oNode.Tag).Url.Equals(m_szDefaultServer) ? new Font(this.Font, FontStyle.Bold) : this.Font;
			}
         else
         {
            oNode.ForeColor = this.ForeColor;
         }

         foreach (TreeNode oChildNode in oNode.Nodes)
         {
            UpdateTreeNodeColors(oChildNode);
         }
      }

      private void RebuildTreeAfterChange()
      {
         if (oPreNode == null) throw new ArgumentNullException("Preselect node unset");
         if (oPostNode == null) throw new ArgumentNullException("Postselect node unset");

			TreeNode oIter = oPreNode;
         while (oIter != null)
         {
            if (oIter == m_hDAPRootNode)
            {
               break;
            }
            oIter = oIter.Parent;
         }
         oIter = oPostNode;
         while (oIter != null)
         {
            if (oIter == m_hDAPRootNode)
            {
               break;
            }
            oIter = oIter.Parent;
         }


         this.BeginUpdate();

         if (!(oPostNode.Tag is LayerBuilder) && oPostNode.Parent != oPreNode)
         {
            TreeNode iterator = oPreNode;
            while (iterator != RootNode)
            {
               if (iterator == oPostNode.Parent)
               {
                  //if (!PruneChildNodes(iterator)) iterator.Nodes.Add(oPostNode);
                  break;
               }
               if (iterator == oPostNode)
               {
                  PruneChildNodes(oPostNode);
                  break;
               }

               PruneChildNodes(iterator);
               iterator = iterator.Parent;
            }
         }

         if (oPostNode.Tag is IBuilder)
            oPostNode.Nodes.AddRange(((IBuilder)oPostNode.Tag).getChildTreeNodes());
         else if (oPostNode.Tag is builderdirectoryType)
            for (int i = 0; i < ((builderdirectoryType)oPostNode.Tag).builderentryCount; i++)
               LoadBuilderEntryIntoNode(((builderdirectoryType)oPostNode.Tag).GetbuilderentryAt(i), oPostNode);
         else if (oPostNode.Tag is tileserversetType)
            LoadTileServerSetIntoNode(oPostNode.Tag as tileserversetType, oPostNode);

         oPostNode.Expand();
         RefreshTreeNodeText();
         this.FilterTreeNodes(oPostNode);
         UpdateTreeNodeColors();
         this.EndUpdate();
      }

      /// <summary>
      /// Removes all the children of a node in a safe way (doesn't touch DAP branch or root)
      /// </summary>
      /// <param name="oNode">The node to prune.</param>
      /// <returns>True if the node is "special"</returns>
      private bool PruneChildNodes(TreeNode oNode)
      {
         // --- Don't nerf the root node ---
         if (oNode == RootNode) return true;

         // --- Only collapse the DAP root node ---
         if (oNode == m_hDAPRootNode)
         {
				m_blAllowCollapse = true;
				oNode.Collapse();
				m_blAllowCollapse = false;
            foreach (TreeNode oSubNode in oNode.Nodes)
               oSubNode.Nodes.Clear();

            return true;
         }

         // --- If I'm in the DAP branch, don't touch anything ---
         TreeNode iterator = oNode.Parent;
         while (iterator != RootNode)
         {
            if (iterator == m_hDAPRootNode) return true;
            iterator = iterator.Parent;
         }

         // --- Ok to nerf it ---
         oNode.Nodes.Clear();
         return false;
      }

      TreeNode oPreNode = null;
      TreeNode oPostNode = null;

      private TreeNode SelectedOrRoot
      {
         get
         {
            if (this.SelectedNode == null) return this.RootNode;
            return this.SelectedNode;
         }
      }

      protected override void OnBeforeSelect(TreeViewCancelEventArgs e)
      {
         base.OnBeforeSelect(e);

         if (e.Node.Tag is TempNodeTag)
         {
            e.Cancel = true;
            return;
         }

         oPreNode = SelectedOrRoot;
      }

      protected override void FireViewMetadataEvent()
      {
         if (SelectedNode != null && ViewMetadata != null)
         {
            if (SelectedNode.Tag is IBuilder)
            {
               ViewMetadata(SelectedNode.Tag as IBuilder);
            }
            if (SelectedNode.Tag is DataSet)
            {
               TreeNode iter = SelectedNode;
               while (!(iter.Tag is Server))
               {
                  iter = iter.Parent;
               }

               ViewMetadata(new DAPQuadLayerBuilder(SelectedNode.Tag as DataSet, MainForm.WorldWindowSingleton, iter.Tag as Server, null));
            }
         }
      }

		/// <summary>
		/// Modify catalog browsing tree
		/// </summary>
		/// <param name="node">The currently selected node.</param>
      protected override void AfterSelected(TreeNode oSelectedNode)
		{
         base.AfterSelected(oSelectedNode);

         oPostNode = SelectedOrRoot;
         FireViewMetadataEvent();
         RebuildTreeAfterChange();
		}
		
		protected void OnAfterCollapse(object sender, TreeViewEventArgs e)
		{
         TreeNode oNodeLastCollapsed = null; // I'm never used?

			// Never collapse root
			if (e.Node == null || e.Node == m_hRootNode)
				m_hRootNode.Expand();

			if (m_bSelect)
			{
				// Populate parent on collapse, but keep me selected
				// this makes for better keyboard/double click navigation
				// in the new tree infrastructure but should only happen in servers

				oNodeLastCollapsed = null;
				TreeNode serverNode = e.Node;
				while (serverNode != null && serverNode.Parent != null)
				{
					if (serverNode.Parent == m_hWMSRootNode || serverNode.Parent == m_hDAPRootNode)
						break;
					serverNode = serverNode.Parent;
				}

				if (serverNode != null && e.Node != null)
				{
					object tag = e.Node.Tag;
					string strText = e.Node.Text;

					this.BeginUpdate();
					this.SelectedNode = e.Node.Parent;
					if (this.SelectedNode != null)
					{
						foreach (TreeNode node in this.SelectedNode.Nodes)
						{
							if (node.Tag == tag && node.Text == strText)
							{
								m_bSelect = false;
								oNodeLastCollapsed = this.SelectedNode = node;
								m_bSelect = true;
								break;
							}
						}
					}
					this.EndUpdate();
				}
			}
		}

      protected override void OnNodeMouseClick(TreeNodeMouseClickEventArgs e)
      {
         base.OnNodeMouseClick(e);

         if ((e.Button & MouseButtons.Right) == MouseButtons.Right)
         {
            this.SelectedNode = e.Node;

            if (SelectedNode == m_hDAPRootNode || SelectedNode == m_hWMSRootNode || SelectedNode == m_hArcIMSRootNode)
            {
               cContextMenu_Add.Show(this, e.Location.X, e.Node.Bounds.Y + e.Node.Bounds.Height / 2);
            }
            else if (SelectedNode.Tag is Server || SelectedNode.Tag is ServerBuilder)
            {
               cContextMenu_Server.Show(this, e.Location.X, e.Node.Bounds.Y + e.Node.Bounds.Height / 2);
            }
            else if (SelectedNode.Tag is DataSet || SelectedNode.Tag is LayerBuilder)
            {
               cContextMenu_Layer.Show(this, e.Location.X, e.Node.Bounds.Y + e.Node.Bounds.Height / 2);
            }
         }
      }

      protected override void OnMouseDoubleClick(MouseEventArgs e)
      {
         base.OnMouseDoubleClick(e);

         AddCurrentDataset();
      }

      private void cContextMenu_Layer_Opening(object sender, System.ComponentModel.CancelEventArgs e)
      {
         cMenuItem_ViewLegend.Enabled = SelectedNode.Tag is LayerBuilder && ((LayerBuilder)SelectedNode.Tag).SupportsLegend;
      }

      private void cContextMenu_Server_Opening(object sender, System.ComponentModel.CancelEventArgs e)
      {

         bool blServerEnabled = false;
         if (SelectedNode.Tag is ServerBuilder) blServerEnabled = ((ServerBuilder)SelectedNode.Tag).Enabled;
         if (SelectedNode.Tag is Server)  blServerEnabled = ((Server)SelectedNode.Tag).Enabled;

         if (blServerEnabled)
         {
            cMenuItem_ToggleServerEnabled.Text = "Disable";
            cMenuItem_ToggleServerEnabled.Image = Dapple.Properties.Resources.disserver;
         }
         else
         {
            cMenuItem_ToggleServerEnabled.Text = "Enable";
            cMenuItem_ToggleServerEnabled.Image = Dapple.Properties.Resources.enserver;
         }

			if (SelectedNode.Tag is AsyncBuilder)
			{
				cMenuItem_Refresh.Enabled = !((AsyncBuilder)SelectedNode.Tag).IsLoading;
			}
			else
			{
				cMenuItem_Refresh.Enabled = blServerEnabled;
			}

         if (SelectedNode.Tag is Server)
         {
            cMenuItem_SetDefault.Enabled = blServerEnabled && !(m_szDefaultServer.Equals(((Server)SelectedNode.Tag).Url));
         }
         else if (SelectedNode.Tag is ServerBuilder)
         {
				cMenuItem_SetDefault.Enabled = blServerEnabled && !(m_szDefaultServer.Equals(((ServerBuilder)SelectedNode.Tag).Uri.ToString()));
         }

			cMenuItem_AddBrowserMap.Enabled = blServerEnabled && SelectedNode.Tag is Server;
      }

      private void cContextMenu_Add_Opening(object sender, System.ComponentModel.CancelEventArgs e)
      {
         if (SelectedNode == m_hDAPRootNode)
         {
            cMenuItem_AddServer.Text = "Add DAP Server...";
            cMenuItem_AddServer.Tag = new AddDAP();
         }
         else if (SelectedNode == m_hWMSRootNode)
         {
            cMenuItem_AddServer.Text = "Add WMS Server...";
            cMenuItem_AddServer.Tag = new AddWMS(MainForm.WorldWindowSingleton, m_hWMSRootNode.Tag as WMSCatalogBuilder);
         }
         else if (SelectedNode == m_hArcIMSRootNode)
         {
            cMenuItem_AddServer.Text = "Add ArcIMS Server...";
            cMenuItem_AddServer.Tag = new AddArcIMS(MainForm.WorldWindowSingleton, m_hArcIMSRootNode.Tag as ArcIMSCatalogBuilder);
         }
         else
         {
            cMenuItem_AddServer.Text = "Add X Server...";
            cMenuItem_AddServer.Tag = null;
         }
      }

		#endregion

      public bool AddDAPServer(string strUrl, out Server hRetServer, bool blEnabled, bool blUpdateHomeView)
      {
         bool result = base.AddDAPServer(strUrl, out hRetServer, blEnabled);
			if (result && blUpdateHomeView)
			{
				m_oParent.CmdUpdateHomeView(MainForm.UpdateHomeViewType.AddServer, new Object[] { hRetServer.Url, "DAP" });
			}

         this.m_oServerListControl.Servers = getServerList();
         if (hRetServer != null && hRetServer.Url.Equals(m_szDefaultServer))
         {
            this.SelectedServer = hRetServer;
         }
			if (result)
			{
				UpdateTreeNodeColors();
			}
         return result;
      }

      public void CmdServerProperties()
      {
         if (this.SelectedServer != null)
            frmProperties.DisplayForm(this.SelectedServer);
      }

		#region WMS TreeNode Sorter
		// Create a node sorter that implements the IComparer interface that puts directories in front of layer builders.
		internal class TreeNodeSorter : System.Collections.IComparer
		{
			ServerTree m_ServerTree;

			public TreeNodeSorter(ServerTree serverTree)
			{
				m_ServerTree = serverTree;
			}

			public int Compare(object x, object y) //  think x - y
			{
				TreeNode tx = x as TreeNode;
				TreeNode ty = y as TreeNode;
				ServerTree tree = tx.TreeView as ServerTree;

				// Put unresolved loading servers at bottom
				if (tx.Text.StartsWith("http://"))
					return int.MaxValue;
				else if (ty.Text.StartsWith("http://"))
					return int.MinValue;

            // Sort root nodes
            if (tx.Nodes == m_ServerTree.ArcIMSRootNodes)
               return 1;
            if (ty.Nodes == m_ServerTree.ArcIMSRootNodes)
               return -1;
            if (tx.Nodes == m_ServerTree.WMSRootNodes)
               return 1;
            if (ty.Nodes == m_ServerTree.WMSRootNodes)
               return -1;
            if (tx.Nodes == m_ServerTree.VERootNodes)
               return 1;
            if (ty.Nodes == m_ServerTree.VERootNodes)
               return -1;
            if (tx.Nodes == m_ServerTree.TileRootNodes)
               return 1;
            if (ty.Nodes == m_ServerTree.TileRootNodes)
               return -1;
            if (tx.Nodes == m_ServerTree.DAPRootNodes)
               return 1;
            if (ty.Nodes == m_ServerTree.DAPRootNodes)
               return -1;

				if (tx.Tag is BuilderDirectory && !(ty.Tag is BuilderDirectory))
					return -1;
				else if (ty.Tag is BuilderDirectory && !(ty.Tag is BuilderDirectory))
					return 1;

				// If they are the same type, call Compare.
				return string.Compare(tx.Text, ty.Text);
			}
		}
		#endregion


      /// <summary>
      /// Get a list of all the servers currently in the tree.
      /// </summary>
      /// <returns></returns>
      public ArrayList getServerList()
      {
         ArrayList result = new ArrayList();

         // --- Dap servers ---
         foreach (TreeNode oNode in m_hDAPRootNode.Nodes)
         {
            if (((Server)oNode.Tag).Status == Server.ServerStatus.OnLine && ((Server)oNode.Tag).Enabled)
               result.Add(oNode.Tag);
         }

         // --- WMS servers ---
         foreach (WMSServerBuilder oServer in ((WMSCatalogBuilder)m_hWMSRootNode.Tag).GetServers())
         {
            if (oServer.IsLoadedSuccessfully && oServer.Enabled)
               result.Add(oServer);
         }

         // --- ArcIMS servers ---
         foreach (ArcIMSServerBuilder oServer in ((ArcIMSCatalogBuilder)m_hArcIMSRootNode.Tag).GetServers())
         {
            if (oServer.IsLoadedSuccessfully && oServer.Enabled)
               result.Add(oServer);
         }

         return result;
      }

      #region Windows Form Designer generated code

      private ContextMenuStrip cContextMenu_Server;
      private System.ComponentModel.IContainer components;
      private ToolStripMenuItem cMenuItem_Properties;
      private ToolStripMenuItem cMenuItem_AddBrowserMap;
      private ToolStripMenuItem cMenuItem_Refresh;
      private ToolStripMenuItem cMenuItem_ToggleServerEnabled;
      private ToolStripMenuItem cMenuItem_RemoveServer;
      private ContextMenuStrip cContextMenu_Add;
      private ContextMenuStrip cContextMenu_Layer;
      private ToolStripMenuItem cMenuItem_ViewLegend;
      private ToolStripMenuItem cMenuItem_AddLayer;

      private void InitializeComponent()
      {
         this.components = new System.ComponentModel.Container();
         this.cContextMenu_Server = new System.Windows.Forms.ContextMenuStrip(this.components);
         this.cMenuItem_Properties = new System.Windows.Forms.ToolStripMenuItem();
         this.cMenuItem_SetDefault = new System.Windows.Forms.ToolStripMenuItem();
         this.cMenuItem_AddBrowserMap = new System.Windows.Forms.ToolStripMenuItem();
         this.cMenuItem_Refresh = new System.Windows.Forms.ToolStripMenuItem();
         this.cMenuItem_ToggleServerEnabled = new System.Windows.Forms.ToolStripMenuItem();
         this.cMenuItem_RemoveServer = new System.Windows.Forms.ToolStripMenuItem();
         this.cContextMenu_Add = new System.Windows.Forms.ContextMenuStrip(this.components);
         this.cMenuItem_AddServer = new System.Windows.Forms.ToolStripMenuItem();
         this.cContextMenu_Layer = new System.Windows.Forms.ContextMenuStrip(this.components);
         this.cMenuItem_AddLayer = new System.Windows.Forms.ToolStripMenuItem();
         this.cMenuItem_ViewLegend = new System.Windows.Forms.ToolStripMenuItem();
         this.cContextMenu_Server.SuspendLayout();
         this.cContextMenu_Add.SuspendLayout();
         this.cContextMenu_Layer.SuspendLayout();
         this.SuspendLayout();
         // 
         // cContextMenu_Server
         // 
         this.cContextMenu_Server.ImageScalingSize = new System.Drawing.Size(18, 18);
         this.cContextMenu_Server.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cMenuItem_Properties,
            this.cMenuItem_SetDefault,
            this.cMenuItem_AddBrowserMap,
            this.cMenuItem_Refresh,
            this.cMenuItem_ToggleServerEnabled,
            this.cMenuItem_RemoveServer});
         this.cContextMenu_Server.Name = "contextMenuStripServers";
         this.cContextMenu_Server.Size = new System.Drawing.Size(235, 148);
         this.cContextMenu_Server.Opening += new System.ComponentModel.CancelEventHandler(this.cContextMenu_Server_Opening);
         // 
         // cMenuItem_Properties
         // 
         this.cMenuItem_Properties.Image = global::Dapple.Properties.Resources.properties;
         this.cMenuItem_Properties.Name = "cMenuItem_Properties";
         this.cMenuItem_Properties.Size = new System.Drawing.Size(234, 24);
         this.cMenuItem_Properties.Text = "Properties...";
         this.cMenuItem_Properties.Click += new System.EventHandler(this.cMenuItem_Properties_Click);
         // 
         // cMenuItem_SetDefault
         // 
         this.cMenuItem_SetDefault.Image = global::Dapple.Properties.Resources.server_favourite;
         this.cMenuItem_SetDefault.Name = "cMenuItem_SetDefault";
         this.cMenuItem_SetDefault.Size = new System.Drawing.Size(234, 24);
         this.cMenuItem_SetDefault.Text = "Set as Favourite";
         this.cMenuItem_SetDefault.Click += new System.EventHandler(this.cMenuItem_SetDefault_Click);
         // 
         // cMenuItem_AddBrowserMap
         // 
         this.cMenuItem_AddBrowserMap.Image = global::Dapple.Properties.Resources.layers_bottom;
         this.cMenuItem_AddBrowserMap.Name = "cMenuItem_AddBrowserMap";
         this.cMenuItem_AddBrowserMap.Size = new System.Drawing.Size(234, 24);
         this.cMenuItem_AddBrowserMap.Text = "Add Browser Map to Data Layers";
         this.cMenuItem_AddBrowserMap.Click += new System.EventHandler(this.cMenuItem_AddBrowserMap_Click);
         // 
         // cMenuItem_Refresh
         // 
         this.cMenuItem_Refresh.Image = global::Dapple.Properties.Resources.server_refresh;
         this.cMenuItem_Refresh.Name = "cMenuItem_Refresh";
         this.cMenuItem_Refresh.Size = new System.Drawing.Size(234, 24);
         this.cMenuItem_Refresh.Text = "Refresh";
         this.cMenuItem_Refresh.Click += new System.EventHandler(this.cMenuItem_Refresh_Click);
         // 
         // cMenuItem_ToggleServerEnabled
         // 
         this.cMenuItem_ToggleServerEnabled.Image = global::Dapple.Properties.Resources.disserver;
         this.cMenuItem_ToggleServerEnabled.Name = "cMenuItem_ToggleServerEnabled";
         this.cMenuItem_ToggleServerEnabled.Size = new System.Drawing.Size(234, 24);
         this.cMenuItem_ToggleServerEnabled.Text = "Disable";
         this.cMenuItem_ToggleServerEnabled.Click += new System.EventHandler(this.cMenuItem_ToggleServerEnabled_Click);
         // 
         // cMenuItem_RemoveServer
         // 
         this.cMenuItem_RemoveServer.Image = global::Dapple.Properties.Resources.server_remove;
         this.cMenuItem_RemoveServer.Name = "cMenuItem_RemoveServer";
         this.cMenuItem_RemoveServer.Size = new System.Drawing.Size(234, 24);
         this.cMenuItem_RemoveServer.Text = "Remove";
         this.cMenuItem_RemoveServer.Click += new System.EventHandler(this.cMenuItem_RemoveServer_Click);
         // 
         // cContextMenu_Add
         // 
         this.cContextMenu_Add.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cMenuItem_AddServer});
         this.cContextMenu_Add.Name = "cContextMenu_Add";
         this.cContextMenu_Add.Size = new System.Drawing.Size(150, 26);
         this.cContextMenu_Add.Opening += new System.ComponentModel.CancelEventHandler(this.cContextMenu_Add_Opening);
         // 
         // cMenuItem_AddServer
         // 
         this.cMenuItem_AddServer.Image = global::Dapple.Properties.Resources.addserver;
         this.cMenuItem_AddServer.Name = "cMenuItem_AddServer";
         this.cMenuItem_AddServer.Size = new System.Drawing.Size(149, 22);
         this.cMenuItem_AddServer.Text = "Add X Server...";
         this.cMenuItem_AddServer.Click += new System.EventHandler(this.cMenuItem_AddServer_Click);
         // 
         // cContextMenu_Layer
         // 
         this.cContextMenu_Layer.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cMenuItem_AddLayer,
            this.cMenuItem_ViewLegend});
         this.cContextMenu_Layer.Name = "cContextMenu_Layer";
         this.cContextMenu_Layer.Size = new System.Drawing.Size(168, 48);
         this.cContextMenu_Layer.Opening += new System.ComponentModel.CancelEventHandler(this.cContextMenu_Layer_Opening);
         // 
         // cMenuItem_AddLayer
         // 
         this.cMenuItem_AddLayer.Image = global::Dapple.Properties.Resources.layers_add;
         this.cMenuItem_AddLayer.Name = "cMenuItem_AddLayer";
         this.cMenuItem_AddLayer.Size = new System.Drawing.Size(167, 22);
         this.cMenuItem_AddLayer.Text = "Add to Data Layers";
         this.cMenuItem_AddLayer.Click += new System.EventHandler(this.cMenuItem_AddLayer_Click);
         // 
         // cMenuItem_ViewLegend
         // 
         this.cMenuItem_ViewLegend.Image = global::Dapple.Properties.Resources.legend;
         this.cMenuItem_ViewLegend.Name = "cMenuItem_ViewLegend";
         this.cMenuItem_ViewLegend.Size = new System.Drawing.Size(167, 22);
         this.cMenuItem_ViewLegend.Text = "View Legend...";
         this.cMenuItem_ViewLegend.Click += new System.EventHandler(this.cMenuItem_ViewLegend_Click);
         // 
         // ServerTree
         // 
         this.LineColor = System.Drawing.Color.Black;
         this.cContextMenu_Server.ResumeLayout(false);
         this.cContextMenu_Add.ResumeLayout(false);
         this.cContextMenu_Layer.ResumeLayout(false);
         this.ResumeLayout(false);

      }

      void cMenuItem_AddServer_Click(object sender, EventArgs e)
      {
         Form oAddForm = cMenuItem_AddServer.Tag as Form;

         if (oAddForm != null)
         {
            if (oAddForm is AddDAP)
            {
					m_oParent.AddDAPServer();
            }
            else if (oAddForm is AddWMS)
            {
					m_oParent.AddWMSServer();
            }
            else if (oAddForm is AddArcIMS)
            {
					m_oParent.AddArcIMSServer();
            }
            else
            {
               throw new InvalidOperationException("Unknown server type");
            }
         }
      }

      void cMenuItem_AddLayer_Click(object sender, EventArgs e)
      {
         AddCurrentDataset();
      }

      void cMenuItem_ViewLegend_Click(object sender, EventArgs e)
      {
         if (SelectedNode != null && SelectedNode.Tag is LayerBuilder && ((LayerBuilder)SelectedNode.Tag).SupportsLegend)
         {
            string[] aLegends = (SelectedNode.Tag as LayerBuilder).GetLegendURLs();
            foreach (string szLegend in aLegends)
            {
               if (!String.IsNullOrEmpty(szLegend)) MainForm.BrowseTo(szLegend);
            }
         }
      }

      void cMenuItem_RemoveServer_Click(object sender, EventArgs e)
      {
         RemoveCurrentServer();
      }

      void cMenuItem_ToggleServerEnabled_Click(object sender, EventArgs e)
      {
         CmdToggleServerEnabled();
      }

      void cMenuItem_Refresh_Click(object sender, EventArgs e)
      {
         RefreshCurrentServer();
      }

      void cMenuItem_AddBrowserMap_Click(object sender, EventArgs e)
      {
         CmdAddBrowserMap();
      }

      void cMenuItem_Properties_Click(object sender, EventArgs e)
      {
         CmdServerProperties();
      }

      void cMenuItem_SetDefault_Click(object sender, EventArgs e)
      {
         CmdSetFavoriteServer();
      }

      public void CmdAddBrowserMap()
      {
         if (SelectedNode != null && SelectedNode.Tag is Server)
         {
            m_activeLayers.AddLayer(new DAPBrowserMapBuilder(MainForm.WorldWindowSingleton, SelectedNode.Tag as Server, null));
         }
      }

      public void CmdSetFavoriteServer()
      {
         if (SelectedNode != null)
         {
            if (SelectedNode.Tag is Server)
            {
               m_szDefaultServer = ((Server)SelectedNode.Tag).Url;
            }
            if (SelectedNode.Tag is ServerBuilder)
            {
               m_szDefaultServer = ((ServerBuilder)SelectedNode.Tag).Uri.ToString();
            }
         }
			ReconstructTree();

			m_oParent.CmdUpdateHomeView(MainForm.UpdateHomeViewType.ChangeFavorite, new Object[] { m_szDefaultServer });
      }

      public void CmdToggleServerEnabled()
      {
			if (SelectedNode.Tag is ServerBuilder)
			{
				if (SelectedIsFavorite && ((ServerBuilder)SelectedNode.Tag).Enabled)
				{
					if (MessageBox.Show(this.TopLevelControl, "Disabling a favourite server will remove the favourite setting.\nAre you sure you want to disable this server?", "Disabling Server", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
						return;

					m_szDefaultServer = String.Empty;
					m_oParent.CmdUpdateHomeView(MainForm.UpdateHomeViewType.ChangeFavorite, new Object[] { String.Empty });
				}

				((ServerBuilder)SelectedNode.Tag).Enabled ^= true; // Toggle it.
				if (SelectedNode.Tag is WMSServerBuilder)
				{
					m_oParent.CmdUpdateHomeView(MainForm.UpdateHomeViewType.ToggleServer, new Object[] { ((ServerBuilder)SelectedNode.Tag).Uri.ToString(), "WMS", ((ServerBuilder)SelectedNode.Tag).Enabled });
				}
				else if (SelectedNode.Tag is ArcIMSServerBuilder)
				{
					m_oParent.CmdUpdateHomeView(MainForm.UpdateHomeViewType.ToggleServer, new Object[] { ((ServerBuilder)SelectedNode.Tag).Uri.ToString(), "ArcIMS", ((ServerBuilder)SelectedNode.Tag).Enabled });
				}
				
				SelectedNode.ForeColor = ((ServerBuilder)SelectedNode.Tag).Enabled ? System.Drawing.SystemColors.WindowText : System.Drawing.Color.Gray;

				LoadFinished();
			}
			else if (SelectedNode.Tag is Server)
			{
				if (SelectedIsFavorite && ((Server)SelectedNode.Tag).Enabled)
				{
					if (MessageBox.Show(this.TopLevelControl, "Disabling a favourite server will remove the favourite setting.\nAre you sure you want to disable this server?", "Disabling Server", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
						return;

					m_szDefaultServer = String.Empty;
					m_oParent.CmdUpdateHomeView(MainForm.UpdateHomeViewType.ChangeFavorite, new Object[] { String.Empty });
				}

				((Server)SelectedNode.Tag).Enabled ^= true;
				m_oParent.CmdUpdateHomeView(MainForm.UpdateHomeViewType.ToggleServer, new Object[] { ((Server)SelectedNode.Tag).Url, "DAP", ((Server)SelectedNode.Tag).Enabled });
				SelectedNode.ForeColor = ((Server)SelectedNode.Tag).Enabled ? System.Drawing.SystemColors.WindowText : System.Drawing.Color.Gray;
				GetCatalogHierarchy();

				LoadFinished();
			}
			else
				throw new ApplicationException("Don't know how to toggle server status of a " + SelectedNode.Tag.GetType().ToString());
      }

      #endregion

      /// <summary>
      /// If this is the Tag of a TreeNode, it is only temporary; don't let the user select it.  See OnBeforeSelect.
      /// </summary>
      public class TempNodeTag { }
   }
}
