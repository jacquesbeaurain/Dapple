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

namespace Dapple
{
	/// <summary>
	/// Derived tree that not only contains DAP servers but also WMS and image tile servers
	/// </summary>
	public class ServerTree : Geosoft.GX.DAPGetData.ServerTree
	{
		#region Members
		protected string m_strSearch;
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
		protected TriStateTreeView m_layerTree;
		protected LayerBuilderList m_activeLayers;
		protected List<ServerBuilder> m_wmsServers = new List<ServerBuilder>();
      protected List<ServerBuilder> m_oArcIMSServers = new List<ServerBuilder>();

      TreeNode m_oLastSelectedTreeNode;
		#endregion

      #region Delegates
      public delegate void LoadFinishedCallbackHandler();
      #endregion

      #region Constructor/Disposal
      /// <summary>
		/// Constructor
		/// </summary>
		/// <param name="strCacheDir"></param>
		public ServerTree(string strCacheDir, MainForm oParent, TriStateTreeView tvLayer, LayerBuilderList activeLayers)
			: base(strCacheDir)
		{
			m_oParent = oParent;
			m_layerTree = tvLayer;
			m_activeLayers = activeLayers;

			this.SupportDatasetSelection = false;
			this.AfterCollapse += new TreeViewEventHandler(OnAfterCollapse);
			this.MouseDoubleClick += new MouseEventHandler(OnMouseDoubleClick);

			// Extra icons
			base.ImageList.Images.Add("dapple", global::Dapple.Properties.Resources.dapple);
			base.ImageList.Images.Add("dap_gray", global::Dapple.Properties.Resources.dap_gray);
			base.ImageList.Images.Add("error", global::Dapple.Properties.Resources.error);
			base.ImageList.Images.Add("folder_gray", global::Dapple.Properties.Resources.folder_gray);
			base.ImageList.Images.Add("layer", global::Dapple.Properties.Resources.layer);
			base.ImageList.Images.Add("live", global::Dapple.Properties.Resources.live);
			base.ImageList.Images.Add("tile", global::Dapple.Properties.Resources.tile);
			base.ImageList.Images.Add("tile_gray", global::Dapple.Properties.Resources.tile_gray);
			base.ImageList.Images.Add("georef_image", global::Dapple.Properties.Resources.georef_image);
			base.ImageList.Images.Add("time", global::Dapple.Properties.Resources.time_icon);
			base.ImageList.Images.Add("wms", global::Dapple.Properties.Resources.wms);
			base.ImageList.Images.Add("wms_gray", global::Dapple.Properties.Resources.wms_gray);
			base.ImageList.Images.Add("nasa", global::Dapple.Properties.Resources.nasa);
			base.ImageList.Images.Add("usgs", global::Dapple.Properties.Resources.usgs);
			base.ImageList.Images.Add("worldwind_central", global::Dapple.Properties.Resources.worldwind_central);
         base.ImageList.Images.Add("arcims", global::Dapple.Properties.Resources.arcims);

			m_hRootNode = new TreeNode("Available Servers", iImageListIndex("dapple"), iImageListIndex("dapple"));
			this.Nodes.Add(m_hRootNode);
			m_hRootNode.ToolTipText = "It is possible to double-click on layers in\nhere to add them to the current layers.\nSingle-click browses tree.";

			m_hDAPRootNode = new TreeNode("DAP Servers", iImageListIndex("dap"), iImageListIndex("dap"));
			m_hRootNode.Nodes.Add(m_hDAPRootNode);

			m_hTileRootNode = new TreeNode("Image Tile Servers", iImageListIndex("tile"), iImageListIndex("tile"));
			m_hRootNode.Nodes.Add(m_hTileRootNode);
			BuilderDirectory tileDir = new BuilderDirectory("Image Tile Servers", null, false, 0, 0);

         VETileSetBuilder veDir = new VETileSetBuilder("Virtual Earth", null, false, iImageListIndex("live"), 0);
			m_hVERootNode = m_hRootNode.Nodes.Add("Virtual Earth");
			m_hVERootNode.SelectedImageIndex = m_hVERootNode.ImageIndex = iImageListIndex("live");
			m_hVERootNode.Tag = veDir;

			m_VEMapQTB = new VEQuadLayerBuilder("Virtual Earth Map", WorldWind.VirtualEarthMapType.road, m_oParent, true, veDir);
			m_VESatQTB = new VEQuadLayerBuilder("Virtual Earth Satellite", WorldWind.VirtualEarthMapType.aerial, m_oParent, true, veDir);
			m_VEMapAndSatQTB = new VEQuadLayerBuilder("Virtual Earth Map & Satellite", WorldWind.VirtualEarthMapType.hybrid, m_oParent, true, veDir);
			veDir.LayerBuilders.Add(m_VEMapQTB);
			veDir.LayerBuilders.Add(m_VESatQTB);
			veDir.LayerBuilders.Add(m_VEMapAndSatQTB);

         WMSCatalogBuilder wmsBuilder = new WMSCatalogBuilder("WMS Servers", m_oParent.WorldWindow, null, 0, iImageListIndex("enserver"), iImageListIndex("layer"), iImageListIndex("folder"));
			wmsBuilder.LoadFinished += new LoadFinishedCallbackHandler(OnLoadFinished);

			m_hWMSRootNode = new TreeNode("WMS Servers", iImageListIndex("wms"), iImageListIndex("wms"));
			m_hWMSRootNode.Tag = wmsBuilder;
			m_hRootNode.Nodes.Add(m_hWMSRootNode);

         ArcIMSCatalogBuilder arcIMSBuilder = new ArcIMSCatalogBuilder("ArcIMS Servers", m_oParent.WorldWindow, null, 0, iImageListIndex("enserver"), iImageListIndex("layer"), iImageListIndex("folder"));
         arcIMSBuilder.LoadFinished += new LoadFinishedCallbackHandler(OnLoadFinished);

         m_hArcIMSRootNode = new TreeNode("ArcIMS Servers", iImageListIndex("arcims"), iImageListIndex("arcims"));
         m_hArcIMSRootNode.Tag = arcIMSBuilder;
         m_hRootNode.Nodes.Add(m_hArcIMSRootNode);

			//m_TreeSorter = new TreeNodeSorter(this);
         this.TreeViewNodeSorter = new TreeNodeSorter(this);

         m_oLastSelectedTreeNode = RootNode;
		}
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
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
				if (!m_oParent.bContainsDAPLayer(m_oCurServer, this.SelectedDAPDataset))
				{
					DAPQuadLayerBuilder layerBuilder = new DAPQuadLayerBuilder(this.SelectedDAPDataset, m_oParent.WorldWindow.CurrentWorld, /*m_strCacheDir,*/ m_oCurServer, null);
					m_oParent.AddLayerBuilder(layerBuilder);
				}
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
				treeNode.SelectedImageIndex = treeNode.ImageIndex = iImageListIndex("disserver");
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
            treeNode.SelectedImageIndex = treeNode.ImageIndex = iImageListIndex("disserver");
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
				wmsBuilder.LoadFinished -= new LoadFinishedCallbackHandler(OnLoadFinished);
            wmsBuilder = new WMSCatalogBuilder("WMS Servers", m_oParent.WorldWindow, null, 0, iImageListIndex("enserver"), iImageListIndex("layer"), iImageListIndex("folder"));
				m_hWMSRootNode.Tag = wmsBuilder;
				wmsBuilder.LoadFinished += new LoadFinishedCallbackHandler(OnLoadFinished);

            // CMTODO: Handle ArcIMS catalog

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
			foreach (ServerBuilder builderEntry in m_wmsServers)
			{
				builderentryType subentry = servers.Newbuilderentry();
				wmscatalogType wms = subentry.Newwmscatalog();
				wms.Addcapabilitiesurl(new SchemaString(builderEntry.Uri.ToBaseUri()));
				subentry.Addwmscatalog(wms);
				dir.Addbuilderentry(subentry);
			}
         foreach (ServerBuilder builderEntry in m_oArcIMSServers)
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
					newServerChildNode.SelectedImageIndex = newServerChildNode.ImageIndex = iImageListIndex("layer");

					int iDistance = tile.Hasdistanceabovesurface() ? tile.distanceabovesurface.Value : Convert.ToInt32(tilelayerType.GetdistanceabovesurfaceDefault());
					int iPixelSize = tile.Hastilepixelsize() ? tile.tilepixelsize.Value : Convert.ToInt32(tilelayerType.GettilepixelsizeDefault());
					NltQuadLayerBuilder quadBuilder = new NltQuadLayerBuilder(tile.name.Value, iDistance, true, new WorldWind.GeographicBoundingBox(tile.boundingbox.maxlat.Value, tile.boundingbox.minlat.Value, tile.boundingbox.minlon.Value, tile.boundingbox.maxlon.Value), tile.levelzerotilesize.Value, tile.levels.Value, iPixelSize, tile.url.Value,
															tile.dataset.Value, tile.imageextension.Value, 255, m_oParent.WorldWindow.CurrentWorld, /*MainApplication.Settings.CachePath,*/ tileDir);
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
					newNode.SelectedImageIndex = newNode.ImageIndex = iImageListIndex("local");
					newNode.Tag = entry.builderdirectory;
				}
			}
			else if (entry.Hastileserverset())
			{
            TreeNode newNode = serverNode.Nodes.Add(entry.tileserverset.name.Value);
				newNode.SelectedImageIndex = newNode.ImageIndex = iImageListIndex("tile");
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
            ((ServerBuilder)treeNode.Tag).updateTreeNode(treeNode, this, m_bAOIFilter, m_filterExtents, m_strSearch);
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
            ((ServerBuilder)treeNode.Tag).updateTreeNode(treeNode, this, m_bAOIFilter, m_filterExtents, m_strSearch);
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
				if (treeNode.Tag is ImageBuilder)
				{
					ImageBuilder builder = treeNode.Tag as ImageBuilder;
					if ((m_strSearch != string.Empty && treeNode.Text.IndexOf(m_strSearch, 0, StringComparison.InvariantCultureIgnoreCase) == -1) ||
					  (m_filterExtents != null && m_bAOIFilter && !m_filterExtents.Intersects(builder.Extents) && !m_filterExtents.Contains(builder.Extents)))
						treeNode.Remove();
				}
				if (treeNode.Tag is BuilderDirectory && !(treeNode.Tag is WMSCatalogBuilder) && !(treeNode.Tag is ArcIMSCatalogBuilder) &&
				  (treeNode.Tag as BuilderDirectory).iGetLayerCount(m_bAOIFilter, m_filterExtents, m_strSearch) == 0)
				{
               // Don't remove loading or busted servers
               if (treeNode.Tag is ServerBuilder && !((ServerBuilder)treeNode.Tag).IsLoadedSuccessfully)
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

      public void CMRebuildTree()
      {
         if (m_oLastSelectedTreeNode == null) throw new ArgumentNullException("oSelectedNode");

         this.BeginUpdate();

         // Get the list of TreeNodes in the path from m_hRootNode to the currently selected node
         List<TreeNode> oSelectedPath = new List<TreeNode>();
         TreeNode oWalker = m_oLastSelectedTreeNode;
         if (oWalker == null) oWalker = m_hRootNode; // Never clear the children of the root node
         do
         {
            oSelectedPath.Add(oWalker);
            oWalker = oWalker.Parent;
         } while (oWalker != null);

         // Remove all the children of nodes not in the path
         PruneClosedNodes(m_hRootNode, oSelectedPath);

         // Add the new children of the selected node
         if (m_oLastSelectedTreeNode.Tag is IBuilder)
            m_oLastSelectedTreeNode.Nodes.AddRange(((IBuilder)m_oLastSelectedTreeNode.Tag).getChildTreeNodes());
         else if (m_oLastSelectedTreeNode.Tag is builderdirectoryType)
            for (int i = 0; i < ((builderdirectoryType)m_oLastSelectedTreeNode.Tag).builderentryCount; i++)
               LoadBuilderEntryIntoNode(((builderdirectoryType)m_oLastSelectedTreeNode.Tag).GetbuilderentryAt(i), m_oLastSelectedTreeNode);
         else if (m_oLastSelectedTreeNode.Tag is tileserversetType)
            LoadTileServerSetIntoNode(m_oLastSelectedTreeNode.Tag as tileserversetType, m_oLastSelectedTreeNode);

         m_oLastSelectedTreeNode.Expand();
         RefreshTreeNodeText();
         this.FilterTreeNodes(m_oLastSelectedTreeNode);
         this.Sort();
         this.EndUpdate();
      }

		/// <summary>
		/// Modify catalog browsing tree
		/// </summary>
		/// <param name="node">The currently selected node.</param>
		protected override void AfterSelected(TreeNode oSelectedNode)
		{
         base.AfterSelected(oSelectedNode);

         m_oLastSelectedTreeNode = oSelectedNode;
         CMRebuildTree();
		}

		TreeNode m_nodeLastCollapsed = null;
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

	}
}
