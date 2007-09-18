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

		VEQuadLayerBuilder m_VEMapQTB;
		VEQuadLayerBuilder m_VESatQTB;
		VEQuadLayerBuilder m_VEMapAndSatQTB;

		protected MainForm m_oParent;
		protected LayerList m_activeLayers;
		protected List<AsyncBuilder> m_wmsServers = new List<AsyncBuilder>();
      protected List<AsyncBuilder> m_oArcIMSServers = new List<AsyncBuilder>();

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
		public ServerTree(ImageList oImageList, string strCacheDir, MainForm oParent, LayerList activeLayers)
			: base(oImageList, strCacheDir)
		{
			m_oParent = oParent;
			m_activeLayers = activeLayers;
         m_activeLayers.ActiveLayersChanged += new LayerList.ActiveLayersChangedHandler(UpdateTreeNodeColors);

			this.SupportDatasetSelection = false;
			this.AfterCollapse += new TreeViewEventHandler(OnAfterCollapse);
			this.MouseDoubleClick += new MouseEventHandler(OnMouseDoubleClick);

         m_hRootNode = new TreeNode("Available Servers", Dapple.MainForm.ImageListIndex("dapple"), Dapple.MainForm.ImageListIndex("dapple"));
			this.Nodes.Add(m_hRootNode);
			m_hRootNode.ToolTipText = "It is possible to double-click on layers in\nhere to add them to the current layers.\nSingle-click browses tree.";

         m_hDAPRootNode = new TreeNode("DAP Servers", Dapple.MainForm.ImageListIndex("dap"), Dapple.MainForm.ImageListIndex("dap"));
			m_hRootNode.Nodes.Add(m_hDAPRootNode);

         m_hTileRootNode = new TreeNode("Image Tile Servers", Dapple.MainForm.ImageListIndex("tile"), Dapple.MainForm.ImageListIndex("tile"));
			m_hRootNode.Nodes.Add(m_hTileRootNode);
			BuilderDirectory tileDir = new BuilderDirectory("Image Tile Servers", null, false, 0, 0);

         VETileSetBuilder veDir = new VETileSetBuilder("Virtual Earth", null, false, Dapple.MainForm.ImageListIndex("live"), 0);
			m_hVERootNode = m_hRootNode.Nodes.Add("Virtual Earth");
         m_hVERootNode.SelectedImageIndex = m_hVERootNode.ImageIndex = Dapple.MainForm.ImageListIndex("live");
			m_hVERootNode.Tag = veDir;

			m_VEMapQTB = new VEQuadLayerBuilder("Virtual Earth Map", WorldWind.VirtualEarthMapType.road, m_oParent.WorldWindow, true, veDir);
         m_VESatQTB = new VEQuadLayerBuilder("Virtual Earth Satellite", WorldWind.VirtualEarthMapType.aerial, m_oParent.WorldWindow, true, veDir);
         m_VEMapAndSatQTB = new VEQuadLayerBuilder("Virtual Earth Map & Satellite", WorldWind.VirtualEarthMapType.hybrid, m_oParent.WorldWindow, true, veDir);
			veDir.LayerBuilders.Add(m_VEMapQTB);
			veDir.LayerBuilders.Add(m_VESatQTB);
			veDir.LayerBuilders.Add(m_VEMapAndSatQTB);

         WMSCatalogBuilder wmsBuilder = new WMSCatalogBuilder("WMS Servers", m_oParent.WorldWindow, null, 0, Dapple.MainForm.ImageListIndex("enserver"), Dapple.MainForm.ImageListIndex("layer"), Dapple.MainForm.ImageListIndex("folder"));
			wmsBuilder.LoadFinished += new LoadFinishedCallbackHandler(OnLoadFinished);

         m_hWMSRootNode = new TreeNode("WMS Servers", Dapple.MainForm.ImageListIndex("wms"), Dapple.MainForm.ImageListIndex("wms"));
			m_hWMSRootNode.Tag = wmsBuilder;
			m_hRootNode.Nodes.Add(m_hWMSRootNode);

         ArcIMSCatalogBuilder arcIMSBuilder = new ArcIMSCatalogBuilder("ArcIMS Servers", m_oParent.WorldWindow, null, 0, Dapple.MainForm.ImageListIndex("enserver"), Dapple.MainForm.ImageListIndex("layer"), Dapple.MainForm.ImageListIndex("folder"));
         arcIMSBuilder.LoadFinished += new LoadFinishedCallbackHandler(OnLoadFinished);

         m_hArcIMSRootNode = new TreeNode("ArcIMS Servers", Dapple.MainForm.ImageListIndex("arcims"), Dapple.MainForm.ImageListIndex("arcims"));
         m_hArcIMSRootNode.Tag = arcIMSBuilder;
         m_hRootNode.Nodes.Add(m_hArcIMSRootNode);

			//m_TreeSorter = new TreeNodeSorter(this);
         this.TreeViewNodeSorter = new TreeNodeSorter(this);

         this.MouseMove += new MouseEventHandler(this.HandleMouseMove);
         this.MouseDown += new MouseEventHandler(this.HandleMouseDown);
         this.AllowDrop = false;
		}
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
		}
		#endregion

      #region Event Handlers

      private TreeNode m_oDragSource = null;
      private void HandleMouseDown(Object oSender, MouseEventArgs oArgs)
      {
         TreeNode oNodeOver = this.GetNodeAt(oArgs.X, oArgs.Y);

         if (oNodeOver != null && (oNodeOver.Tag is DataSet || oNodeOver.Tag is LayerBuilder))
         {
            m_oDragSource = oNodeOver;
         }
      }

      private void HandleMouseMove(Object oSender, MouseEventArgs oArgs)
      {
         if ((oArgs.Button & MouseButtons.Left) == MouseButtons.Left)
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
         // Rebuild the tree, only the selected node hasn't changed
         oPreNode = SelectedOrRoot;
         oPostNode = SelectedOrRoot;
         CMRebuildTree();
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
			if (this.SelectedNode == null)
				return;

			if (this.SelectedNode.Tag is WMSServerBuilder)
			{
            WMSServerBuilder serverBuilder = this.SelectedNode.Tag as WMSServerBuilder;
            RemoveCurrentServer();
            AddWMSServer(serverBuilder.Uri.ToBaseUri(), true);
			}
         else if (this.SelectedNode.Tag is ArcIMSServerBuilder)
         {
            ArcIMSServerBuilder serverBuilder = this.SelectedNode.Tag as ArcIMSServerBuilder;
            RemoveCurrentServer();
            AddArcIMSServer(serverBuilder.Uri as ArcIMSServerUri, true);
         }
         else if (this.SelectedNode.Tag is Server)
         {
            (this.SelectedNode.Tag as Server).UpdateConfiguration();
            ClearCatalog();
            GetCatalogHierarchy();
            GetDatasetCount(this.SelectedNode.Tag as Server);
            RefreshTreeNodeText();
         }
		}

		public void RemoveCurrentServer()
		{
			if (this.SelectedNode == null)
				return;

         if (this.SelectedNode.Tag == null || this.SelectedNode.Tag is DataSet || this.SelectedNode.Tag is WMSQuadLayerBuilder || this.SelectedNode.Tag is ArcIMSQuadLayerBuilder)
				return;

         if (this.SelectedNode.Tag is LayerBuilder)
			{
            (this.SelectedNode.Parent.Tag as BuilderDirectory).LayerBuilders.Remove(this.SelectedNode.Tag as LayerBuilder);
			}
			else
			{
            if (this.SelectedNode.Tag is Server)
				{
               RemoveServer(this.SelectedNode.Tag as Server);
					return;
				}
            else if (this.SelectedNode.Tag is WMSServerBuilder)
				{
               WMSServerBuilder serverBuilder = this.SelectedNode.Tag as WMSServerBuilder;
				   m_wmsServers.Remove(serverBuilder);
               ((WMSCatalogBuilder)m_hWMSRootNode.Tag).UncacheServer(serverBuilder.Uri as WMSServerUri);
				}
            else if (this.SelectedNode.Tag is ArcIMSServerBuilder)
            {
               ArcIMSServerBuilder serverBuilder = this.SelectedNode.Tag as ArcIMSServerBuilder;
               m_oArcIMSServers.Remove(serverBuilder);
               ((ArcIMSCatalogBuilder)m_hArcIMSRootNode.Tag).UncacheServer(serverBuilder.Uri as ArcIMSServerUri);
            }
            (this.SelectedNode.Parent.Tag as BuilderDirectory).SubList.Remove(this.SelectedNode.Tag as BuilderDirectory);
			}

         this.SelectedNode.Parent.Nodes.Remove(this.SelectedNode);
		}

		public bool AddWMSServer(string strCapUrl, bool bUpdateTree)
		{
			WMSCatalogBuilder wmsBuilder = m_hWMSRootNode.Tag as WMSCatalogBuilder;
         if (wmsBuilder.ContainsServer(new WMSServerUri(strCapUrl))) return false;// Don't add a server multiple times

			TreeNode treeNode = null;
         WMSServerBuilder builder = wmsBuilder.AddServer(new WMSServerUri(strCapUrl)) as WMSServerBuilder;
			m_wmsServers.Add(builder);
			if (bUpdateTree)
			{
				this.BeginUpdate();
				treeNode = m_hWMSRootNode.Nodes.Add(strCapUrl);
            treeNode.SelectedImageIndex = treeNode.ImageIndex = Dapple.MainForm.ImageListIndex("disserver");
				treeNode.Tag = builder;
				this.EndUpdate();
            //this.AfterSelected(this.SelectedNode);
            this.SelectedNode = treeNode;
			}
         return true;
		}

      public bool AddArcIMSServer(ArcIMSServerUri serverUri, bool bUpdateTree)
      {
         ArcIMSCatalogBuilder arcimsBuilder = m_hArcIMSRootNode.Tag as ArcIMSCatalogBuilder;
         if (arcimsBuilder.ContainsServer(serverUri)) return false; // Don't add multiple times

         ArcIMSServerBuilder builderEntry = arcimsBuilder.AddServer(serverUri) as ArcIMSServerBuilder;
         m_oArcIMSServers.Add(builderEntry);
         if (bUpdateTree)
         {
            this.BeginUpdate();
            TreeNode treeNode = m_hArcIMSRootNode.Nodes.Add(serverUri.ToString());
            treeNode.SelectedImageIndex = treeNode.ImageIndex = Dapple.MainForm.ImageListIndex("disserver");
            treeNode.Tag = builderEntry;
            this.EndUpdate();
            //this.AfterSelected(this.SelectedNode);
            this.SelectedNode = treeNode;
         }
         return true;
      }

		public void LoadFromView(DappleView oView)
		{
			this.BeginUpdate();

			try
			{
				// Clear Tree and WMS servers too
				WMSCatalogBuilder wmsBuilder = m_hWMSRootNode.Tag as WMSCatalogBuilder;
            wmsBuilder.cancelDownloads();
				wmsBuilder.LoadFinished -= new LoadFinishedCallbackHandler(OnLoadFinished);
            wmsBuilder = new WMSCatalogBuilder("WMS Servers", m_oParent.WorldWindow, null, 0, Dapple.MainForm.ImageListIndex("enserver"), Dapple.MainForm.ImageListIndex("layer"), Dapple.MainForm.ImageListIndex("folder"));
				m_hWMSRootNode.Tag = wmsBuilder;
				wmsBuilder.LoadFinished += new LoadFinishedCallbackHandler(OnLoadFinished);

            ArcIMSCatalogBuilder arcIMSBuilder = m_hArcIMSRootNode.Tag as ArcIMSCatalogBuilder;
            arcIMSBuilder.cancelDownloads();
            arcIMSBuilder.LoadFinished -= new LoadFinishedCallbackHandler(OnLoadFinished);
            arcIMSBuilder = new ArcIMSCatalogBuilder("ArcIMS Servers", m_oParent.WorldWindow, null, 0, Dapple.MainForm.ImageListIndex("enserver"), Dapple.MainForm.ImageListIndex("layer"), Dapple.MainForm.ImageListIndex("folder"));
            m_hArcIMSRootNode.Tag = arcIMSBuilder;
            arcIMSBuilder.LoadFinished += new LoadFinishedCallbackHandler(OnLoadFinished);

				foreach (TreeNode node in m_hRootNode.Nodes)
					node.Nodes.Clear();

				// Reset First
				m_bEntireCatalogMode = false;
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
				m_bSelect = true;
				// m_hRootNode.Text = strName;

				/*if (view.View.Hasnotes())
				   m_hRootNode.ToolTipText = view.View.notes.Value;
				else
				   this.lblNotes.Text = strName;
				*/
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
				// Always expand root node
				m_hRootNode.Expand();

				// Collapse the first level nodes and clean the subnodes (we can restore them using the IBuilder parent/client relations ships from here on)
				foreach (TreeNode node in m_hRootNode.Nodes)
				{
					if (node == m_hDAPRootNode)
						node.Collapse();
					else
						node.Nodes.Clear();
					foreach (TreeNode subNode in node.Nodes)
						subNode.Nodes.Clear();
				}
				UpdateCounts();
				this.EndUpdate();
			}
		}

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
			foreach (AsyncBuilder builderEntry in m_wmsServers)
			{
				builderentryType subentry = servers.Newbuilderentry();
				wmscatalogType wms = subentry.Newwmscatalog();
				wms.Addcapabilitiesurl(new SchemaString(builderEntry.Uri.ToBaseUri()));
				subentry.Addwmscatalog(wms);
				dir.Addbuilderentry(subentry);
			}
         foreach (AsyncBuilder builderEntry in m_oArcIMSServers)
         {
            builderentryType subentry = servers.Newbuilderentry();
            arcimscatalogType arcims = subentry.Newarcimscatalog();
            arcims.Addcapabilitiesurl(new SchemaString(builderEntry.Uri.ToBaseUri()));
            subentry.Addarcimscatalog(arcims);
            dir.Addbuilderentry(subentry);
         }
			entry.Addbuilderdirectory(dir);
			servers.Addbuilderentry(entry);

			oView.View.Addservers(servers);
		}


		void LoadTileServerSetIntoNode(tileserversetType tileServerSet, TreeNode serverNode)
		{
         BuilderDirectory tileDir = new BuilderDirectory(tileServerSet.name.Value, null, false, 0, 0);
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
            AddDAPServer(entry.dapcatalog.url.Value, out dapServer);
         }
         else if (entry.Haswmscatalog())
            AddWMSServer(entry.wmscatalog.capabilitiesurl.Value, false);
         else if (entry.Hasarcimscatalog())
            AddArcIMSServer(new ArcIMSServerUri(entry.arcimscatalog.capabilitiesurl.Value), false);
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

      /// <summary>
      /// Removes the child nodes of any TreeNode not in the path from the selected node to the root node.
      /// </summary>
      /// <remarks>
      /// Doesn't remove children from Dap root node.  That makes it not work.  Just remove its grandchildren
      /// and collapses it.
      /// </remarks>
      /// <param name="focus">The node to remove children from if not in openNodes.</param>
      /// <param name="openNodes">The list of nodes in the path from the root node to the currently selected node.</param>
      private void PruneClosedNodes(TreeNode focus, List<TreeNode> openNodes)
      {
         foreach (TreeNode child in focus.Nodes)
         {
            if (openNodes.Contains(child))
               PruneClosedNodes(child, openNodes);
            else
            {
               // Don't clear the DAP root node, just close it and clear it's subchildren
               if (child == m_hDAPRootNode)
               {
                  child.Collapse();
                  foreach (TreeNode subNode in child.Nodes)
                     subNode.Nodes.Clear();
               }
               else
                  child.Nodes.Clear();
            }
         }
         if (focus != m_hRootNode && focus != m_hDAPRootNode) openNodes[0].Nodes.Clear();
      }

      protected override void UpdateTreeNodeColors()
      {
         SuspendLayout();
         UpdateTreeNodeColors(RootNode);
         ResumeLayout();
      }

      private void UpdateTreeNodeColors(TreeNode oNode)
      {
         if (oNode.Tag is LayerBuilder && m_activeLayers.ContainsLayerBuilder(oNode.Tag as LayerBuilder))
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

            if (m_activeLayers.ContainsLayerBuilder(new DAPQuadLayerBuilder(oNode.Tag as DataSet, MainForm.WorldWindowSingleton, iter.Tag as Server, null)))
            {
               oNode.ForeColor = System.Drawing.Color.Green;
            }
            else
            {
               oNode.ForeColor = this.ForeColor;
            }
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

      private void CMRebuildTree()
      {
         if (oPreNode == null) throw new ArgumentNullException("Preselect node unset");
         if (oPostNode == null) throw new ArgumentNullException("Postselect node unset");

         this.BeginUpdate();

         if (!(oPostNode.Tag is LayerBuilder))
         {
            TreeNode iterator = oPreNode;
            while (iterator != RootNode)
            {
               if (iterator == oPostNode.Parent)
               {
                  if (!PruneChildNodes(iterator)) iterator.Nodes.Add(oPostNode);
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
            oNode.Collapse();
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
         CMRebuildTree();

         FireViewMetadataEvent();
		}

		TreeNode m_nodeLastCollapsed = null; // I'm never used?
		protected void OnAfterCollapse(object sender, TreeViewEventArgs e)
		{
			// Never collapse root
			if (e.Node == null || e.Node == m_hRootNode)
				m_hRootNode.Expand();

			if (m_bSelect)
			{
				// Populate parent on collapse, but keep me selected
				// this makes for better keyboard/double click navigation
				// in the new tree infrastructure but should only happen in servers

				m_nodeLastCollapsed = null;
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
								m_nodeLastCollapsed = this.SelectedNode = node;
								m_bSelect = true;
								break;
							}
						}
					}
					this.EndUpdate();
				}
			}
		}

		protected void OnMouseDoubleClick(object sender, MouseEventArgs e)
		{
         AddCurrentDataset();
		}
		#endregion

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
            if (((Server)oNode.Tag).Status == Server.ServerStatus.OnLine)
               result.Add(oNode.Tag);
         }

         // --- WMS servers ---
         foreach (WMSServerBuilder oServer in ((WMSCatalogBuilder)m_hWMSRootNode.Tag).GetServers())
         {
            if (oServer.IsLoadedSuccessfully)
               result.Add(oServer);
         }

         // --- ArcIMS servers ---
         foreach (ArcIMSServerBuilder oServer in ((ArcIMSCatalogBuilder)m_hArcIMSRootNode.Tag).GetServers())
         {
            if (oServer.IsLoadedSuccessfully)
               result.Add(oServer);
         }

         return result;
      }
	}
}
