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
      protected TreeNode m_hWMSRootNode;
      protected MainForm m_oParent;
      protected TriStateTreeView m_layerTree;
      protected LayerBuilderList m_activeLayers;
      protected List<BuilderEntry> m_wmsServers = new List<BuilderEntry>();
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

         this.ShowLines = true;
         this.ShowRootLines = false;
         this.ShowNodeToolTips = true;
         this.Sorted = true;
         this.ShowPlusMinus = false;
         //this.TreeViewNodeSorter = new TreeNodeSorter();
         this.Scrollable = true;

         m_hRootNode = new TreeNode("Available Servers", iImageListIndex("dapple"), iImageListIndex("dapple"));
         this.Nodes.Add(m_hRootNode);
         m_hRootNode.ToolTipText = "It is possible to double-click on layers in\nhere to add them to the current layers.\nSingle-click browses tree.";

         m_hDAPRootNode = new TreeNode("DAP Servers", iImageListIndex("dap"), iImageListIndex("dap"));
         m_hRootNode.Nodes.Add(m_hDAPRootNode);

         m_hTileRootNode = new TreeNode("Image Tile Servers", iImageListIndex("tile"), iImageListIndex("tile"));
         m_hRootNode.Nodes.Add(m_hTileRootNode);
         BuilderDirectory tileDir = new BuilderDirectory("Image Tile Servers", null, false);
        
         WMSCatalogBuilder wmsBuilder = new WMSCatalogBuilder(m_oParent.Settings.WorldWindDirectory, m_oParent.WorldWindowControl, "WMS Servers", null);
         wmsBuilder.LoadingCompleted += new WMSCatalogBuilder.LoadingCompletedCallbackHandler(OnWMSCatalogLoaded);
         wmsBuilder.LoadingFailed += new WMSCatalogBuilder.LoadingFailedCallbackHandler(OnWMSCatalogFailed);

         m_hWMSRootNode = new TreeNode("WMS Servers", iImageListIndex("wms"), iImageListIndex("wms"));
         m_hWMSRootNode.Tag = wmsBuilder;
         m_hRootNode.Nodes.Add(m_hWMSRootNode);
         
         this.MouseDoubleClick += new MouseEventHandler(OnMouseDoubleClick);
      }
      protected override void Dispose(bool disposing)
      {
         base.Dispose(disposing);
      }
      #endregion

      #region Properties
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
      /// The root node collection to use where tile servers are kept
      /// </summary>
      public TreeNodeCollection TileRootNodes
      {
         get
         {
            return m_hTileRootNode.Nodes;
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

      #region Catalog Loaded/Failed Handlers

      void OnWMSCatalogLoaded(WMSServerBuilder builder)
      {
         if (!this.IsDisposed)
         {
            if (this.InvokeRequired)
               this.BeginInvoke(new WMSCatalogBuilder.LoadingCompletedCallbackHandler(LoadWMSCatalog), new object[] { builder });
            else
               LoadWMSCatalog(builder);
         }
      }

      void OnWMSCatalogFailed(WMSServerBuilder builder, string message)
      {
         if (!this.IsDisposed)
         {
            if (this.InvokeRequired)
               this.BeginInvoke(new WMSCatalogBuilder.LoadingFailedCallbackHandler(WMSCatalogFailed), new object[] { builder, message });
            else
               WMSCatalogFailed(builder, message);
         }
      }

      void WMSCatalogFailed(WMSServerBuilder builder, string message)
      {
         BuilderEntry builderEntry = null;
         foreach (BuilderEntry entry in m_wmsServers)
         {
            if (entry.Builder == builder)
            {
               entry.Error = true;
               entry.ErrorString = message;
               entry.Loading = false;
               builderEntry = entry;
               break;
            }
         }

         if (builderEntry != null)
            bFillWMSCatalogEntriesInTreeNode(builderEntry.Builder as BuilderDirectory);
      }

      bool bFillWMSCatalogEntriesInTreeNode(BuilderDirectory dir)
      {
         TreeNode treeNode;

         TreeNode treeWMSNode = (this.SelectedNode != null && (this.SelectedNode.Tag as BuilderDirectory) == dir) ? this.SelectedNode : null;
         TreeNode treeWMSRootNode = this.SelectedNode == m_hWMSRootNode ? m_hWMSRootNode : null;

         if (treeWMSNode != null)
         {
            treeNode = treeWMSNode;
            while (treeWMSRootNode == null && treeNode.Parent != null)
            {
               if (treeNode.Parent == m_hWMSRootNode)
               {
                  treeWMSRootNode = m_hWMSRootNode;
                  break;
               }
               treeNode = treeNode.Parent;
            }
         }
        
         if (treeWMSRootNode != null)
         {
            this.BeginUpdate();

            if (treeWMSRootNode == this.SelectedNode)
            {
               treeWMSRootNode.Nodes.Clear();
               foreach (BuilderEntry entry in m_wmsServers)
               {
                  if (entry.Loading)
                  {
                     treeNode = treeWMSRootNode.Nodes.Add("Loading: " + entry.Builder.Name);
                     treeNode.SelectedImageIndex = treeNode.ImageIndex = iImageListIndex("time");
                  }
                  else
                  {
                     treeNode = treeWMSRootNode.Nodes.Add(entry.Builder.Name);
                     if (entry.Error)
                     {
                        if (entry.ErrorString != string.Empty)
                           treeNode.Text += "(" + entry.ErrorString + ")";
                        treeNode.SelectedImageIndex = treeNode.ImageIndex = iImageListIndex("offline");
                     }
                     else
                        treeNode.SelectedImageIndex = treeNode.ImageIndex = iImageListIndex("enserver");
                  }
                  treeNode.Tag = entry.Builder;
               }
               treeWMSRootNode.Expand();
            }
            else
            {
               List<TreeNode> cutList = new List<TreeNode>();
               foreach (TreeNode cutNode in treeWMSRootNode.Nodes)
               {
                  if (cutNode != treeWMSNode)
                     cutList.Add(cutNode);
               }
               foreach (TreeNode cutNode in cutList)
                  cutNode.Remove();

               if (dir is WMSServerBuilder && dir.SubList.Count == 1)
               {
                  foreach (BuilderDirectory childDir in dir.SubList[0].SubList)
                  {
                     treeNode = treeWMSNode.Nodes.Add(childDir.Name);
                     treeNode.SelectedImageIndex = treeNode.ImageIndex = iImageListIndex("folder");
                     treeNode.Tag = childDir;
                  }
                  foreach (LayerBuilder builder in dir.SubList[0].LayerBuilders)
                  {
                     treeNode = treeWMSNode.Nodes.Add(builder.Name, builder.Name, iImageListIndex("layer"), iImageListIndex("layer"));
                     treeNode.Tag = builder;
                  }
               }
               else
               {
                  foreach (BuilderDirectory childDir in dir.SubList)
                  {
                     treeNode = treeWMSNode.Nodes.Add(childDir.Name);
                     treeNode.SelectedImageIndex = treeNode.ImageIndex = iImageListIndex("folder");
                     treeNode.Tag = childDir;
                  }
               }
               foreach (LayerBuilder builder in dir.LayerBuilders)
               {
                  treeNode = treeWMSNode.Nodes.Add(builder.Name, builder.Name, iImageListIndex("layer"), iImageListIndex("layer"));
                  treeNode.Tag = builder;
               }
               treeWMSNode.Expand();
            }
            this.EndUpdate();
            return true;
         }
         else
            return false;
      }

      void LoadWMSCatalog(WMSServerBuilder wmsbuilder)
      {
         BuilderEntry builderEntry = null;
         foreach (BuilderEntry entry in m_wmsServers)
         {
            if (entry.Builder == wmsbuilder)
            {
               entry.Error = false;
               entry.Loading = false;
               builderEntry = entry;
               break;
            }
         }

         if (builderEntry != null)
            bFillWMSCatalogEntriesInTreeNode(builderEntry.Builder as BuilderDirectory);

         m_layerTree.BeginUpdate();

         // Find provider in parents first
         IBuilder parentCatalog = wmsbuilder;

         while (parentCatalog != null && !(parentCatalog is WMSCatalogBuilder))
            parentCatalog = parentCatalog.Parent;

         if (parentCatalog != null)
         {
            WMSCatalogBuilder provider = parentCatalog as WMSCatalogBuilder;

            foreach (LayerBuilderContainer container in m_activeLayers)
            {
               if (container.Uri.StartsWith(WMSQuadLayerBuilder.URLProtocolName) && wmsbuilder.URL == WMSQuadLayerBuilder.ServerURLFromURI(container.Uri))
               {
                  LayerBuilder builder = WMSQuadLayerBuilder.GetBuilderFromURI(container.Uri, provider, m_oParent.WorldWindowControl, wmsbuilder);
                  if (builder != null)
                     m_activeLayers.RefreshFromSource(container, builder);
               }
            }
         }
         m_layerTree.EndUpdate();
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

      public void AddCurrentDAPDataSet()
      {
         if (this.SelectedDAPDataset != null)
         {
            if (!m_oParent.bContainsDAPLayer(m_oCurServer, this.SelectedDAPDataset))
            {
               DAPQuadLayerBuilder layerBuilder = new DAPQuadLayerBuilder(this.SelectedDAPDataset, m_oParent.WorldWindowControl.CurrentWorld, m_strCacheDir, m_oCurServer, null);
               m_oParent.AddLayerBuilder(layerBuilder);
            }
         }
      }

      public void RefreshCurrentServer()
      {
         if (this.SelectedNode == null)
            return;

         if (this.SelectedNode.Tag is WMSServerBuilder)
         {
            WMSServerBuilder serverBuilder = this.SelectedNode.Tag as WMSServerBuilder;
            BuilderEntry builderEntry = null;
            foreach (BuilderEntry entry in m_wmsServers)
            {
               if (entry.Builder == serverBuilder)
               {
                  builderEntry = entry;
                  break;
               }
            }


            if (builderEntry != null)
            {
               builderEntry.Error = false;
               builderEntry.Loading = true;

               // Need to have a catalog builder in treenode's parents
               IBuilder parentCatalog = this.SelectedNode.Tag as IBuilder;

               while (parentCatalog != null && !(parentCatalog is WMSCatalogBuilder))
                  parentCatalog = parentCatalog.Parent;

               if (parentCatalog != null)
               {
                  WMSCatalogBuilder wmsBuilder = parentCatalog as WMSCatalogBuilder;
                  wmsBuilder.RemoveServer(serverBuilder.URL);

                  this.SelectedNode.Nodes.Clear();
                  this.SelectedNode.Text = "Loading: " + serverBuilder.URL;
                  this.SelectedNode.SelectedImageIndex = this.SelectedNode.ImageIndex = iImageListIndex("time");
                  this.SelectedNode.Tag = builderEntry.Builder = wmsBuilder.AddServer(serverBuilder.URL, serverBuilder.Parent as BuilderDirectory);
                  wmsBuilder.SubList.Add(builderEntry.Builder as BuilderDirectory);
               }
            }
         }
         else if (this.SelectedNode.Tag is Server)
            (this.SelectedNode.Tag as Server).UpdateConfiguration();
      }

      public void RemoveCurrentServer()
      {
         if (this.SelectedNode == null)
            return;

         TreeNode treeNode = this.SelectedNode;

         if (treeNode.Tag == null || treeNode.Tag is DataSet || treeNode.Tag is WMSQuadLayerBuilder)
            return;

         if (treeNode.Tag is LayerBuilder)
         {
            (treeNode.Parent.Tag as BuilderDirectory).LayerBuilders.Remove(treeNode.Tag as LayerBuilder);
         }
         else
         {
            if (treeNode.Tag is Server)
            {
               RemoveServer(treeNode.Tag as Server);
               return;
            }
            else if (treeNode.Tag is WMSServerBuilder)
            {
               WMSServerBuilder serverBuilder = treeNode.Tag as WMSServerBuilder;
               BuilderEntry builderEntry = null;
               foreach (BuilderEntry entry in m_wmsServers)
               {
                  if (entry.Builder == serverBuilder)
                  {
                     builderEntry = entry;
                     break;
                  }
               }

               if (builderEntry != null)
               {
                  m_wmsServers.Remove(builderEntry);

                  // Need to have a catalog builder in treenode's parents
                  IBuilder parentCatalog = serverBuilder as IBuilder;

                  while (parentCatalog != null && !(parentCatalog is WMSCatalogBuilder))
                     parentCatalog = parentCatalog.Parent;

                  if (parentCatalog != null && parentCatalog is WMSCatalogBuilder)
                     (parentCatalog as WMSCatalogBuilder).RemoveServer(serverBuilder.URL);
               }
               else
                  return;
            }
            (treeNode.Parent.Tag as BuilderDirectory).SubList.Remove(treeNode.Tag as BuilderDirectory);
         }

         treeNode.Parent.Nodes.Remove(treeNode);
      }

      public bool AddWMSServer(string strCapUrl, bool bUpdateTree)
      {
         WMSCatalogBuilder wmsBuilder = m_hWMSRootNode.Tag as WMSCatalogBuilder;
         WorldWind.WMSList wmsList = wmsBuilder.FindServer(strCapUrl);
         if (wmsList == null)
         {
            BuilderEntry builderEntry;
            m_wmsServers.Add(builderEntry = new BuilderEntry(wmsBuilder.AddServer(strCapUrl, wmsBuilder), false, true, String.Empty));
            if (bUpdateTree)
            {
               this.BeginUpdate();
               m_hWMSRootNode.Nodes.Clear();
               TreeNode treeNode = WMSRootNodes.Add("Loading: " + strCapUrl);
               treeNode.SelectedImageIndex = treeNode.ImageIndex = iImageListIndex("time");
               treeNode.Tag = builderEntry.Builder;
               this.EndUpdate();
            }
            wmsBuilder.SubList.Add(builderEntry.Builder as BuilderDirectory);
            
            return true;
         }
         else
            return false;
      }

      public void LoadFromView(string strName, DappleView oView)
      {
         this.BeginUpdate();

         try
         {
            // Clear Tree and WMS servers too
            WMSCatalogBuilder wmsBuilder = m_hWMSRootNode.Tag as WMSCatalogBuilder;
            wmsBuilder.LoadingCompleted -= new WMSCatalogBuilder.LoadingCompletedCallbackHandler(OnWMSCatalogLoaded);
            wmsBuilder.LoadingFailed -= new WMSCatalogBuilder.LoadingFailedCallbackHandler(OnWMSCatalogFailed);
            wmsBuilder = new WMSCatalogBuilder(m_oParent.Settings.WorldWindDirectory, m_oParent.WorldWindowControl, "WMS Servers", null);
            m_hWMSRootNode.Tag = wmsBuilder;
            wmsBuilder.LoadingCompleted += new WMSCatalogBuilder.LoadingCompletedCallbackHandler(OnWMSCatalogLoaded);
            wmsBuilder.LoadingFailed += new WMSCatalogBuilder.LoadingFailedCallbackHandler(OnWMSCatalogFailed);

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
         dir = entry.Newbuilderdirectory();
         dir.Addname(new SchemaString(m_hWMSRootNode.Text));
         dir.Addspecialcontainer(new SpecialDirectoryType("WMSServers"));
         foreach (BuilderEntry builderEntry in m_wmsServers)
         {
            builderentryType subentry = servers.Newbuilderentry();
            WMSServerBuilder wmsServer = builderEntry.Builder as WMSServerBuilder;
            wmscatalogType wms = subentry.Newwmscatalog();
            wms.Addcapabilitiesurl(new SchemaString(wmsServer.URL));
            subentry.Addwmscatalog(wms);
            dir.Addbuilderentry(subentry);
         }
         entry.Addbuilderdirectory(dir);
         servers.Addbuilderentry(entry);

         oView.View.Addservers(servers);
      }

      void LoadBuilderEntryIntoNode(tileserversetType tileServerSet, TreeNode serverNode)
      {
         TreeNode newServerChildNode;
         int j;

         TileSetSet tileDir = new TileSetSet(tileServerSet.name.Value, null, false);
         if (tileServerSet.Hastilelayers())
         {
            for (j = 0; j < tileServerSet.tilelayers.tilelayerCount; j++)
            {
               tilelayerType tile = tileServerSet.tilelayers.GettilelayerAt(j);
               newServerChildNode = serverNode.Nodes.Add(tile.name.Value);
               newServerChildNode.SelectedImageIndex = newServerChildNode.ImageIndex = iImageListIndex("layer");

               int iDistance = tile.Hasdistanceabovesurface() ? tile.distanceabovesurface.Value : Convert.ToInt32(tilelayerType.GetdistanceabovesurfaceDefault());
               int iPixelSize = tile.Hastilepixelsize() ? tile.tilepixelsize.Value : Convert.ToInt32(tilelayerType.GettilepixelsizeDefault());
               WorldWind.ImageTileService tileService = new WorldWind.ImageTileService(tile.dataset.Value, tile.url.Value);
               QuadLayerBuilder quadBuilder = new QuadLayerBuilder(tile.name.Value, iDistance, true, new WorldWind.GeographicBoundingBox(tile.boundingbox.maxlat.Value, tile.boundingbox.minlat.Value, tile.boundingbox.minlon.Value, tile.boundingbox.maxlon.Value), (decimal)tile.levelzerotilesize.Value, tile.levels.Value, iPixelSize, tileService,
                                                       tile.imageextension.Value, 255, m_oParent.WorldWindowControl.CurrentWorld, m_oParent.Settings.CachePath, m_oParent.Settings.CachePath, tileDir);
               newServerChildNode.Tag = quadBuilder;
            }
         }
      }

      void LoadBuilderEntryIntoNode(builderentryType entry, TreeNode serverNode)
      {
         TreeNodeCollection serverNodes;
         TreeNode newServerNode;
         TreeNode newServerChildNode;
         TreeNode newServerChildSubNode;

         serverNodes = serverNode.Nodes;
         
         if (entry.Hasbuilderdirectory())
         {
            if (entry.builderdirectory.Hasspecialcontainer())
               return;
            else
            {
               newServerNode = serverNodes.Add(entry.builderdirectory.name.Value);
               newServerNode.SelectedImageIndex = newServerNode.ImageIndex = iImageListIndex("local");
               newServerNode.Tag = entry.builderdirectory;
            }
         }
         else if (entry.Hastileserverset())
         {
            newServerChildNode = serverNodes.Add(entry.tileserverset.name.Value);
            newServerChildNode.SelectedImageIndex = newServerChildNode.ImageIndex = iImageListIndex("tile");
            newServerChildNode.Tag = entry.tileserverset;            
         }
         else if (entry.Hasvirtualearth())
         {
            VETileSetBuilder veDir = new VETileSetBuilder(entry.virtualearth.name.Value, null, false);
            newServerChildNode = serverNodes.Add(entry.virtualearth.name.Value);
            newServerChildNode.SelectedImageIndex = newServerChildNode.ImageIndex = iImageListIndex("live");
            newServerChildNode.Tag = veDir;

            VEQuadLayerBuilder q = new VEQuadLayerBuilder("Virtual Earth Map", VEQuadLayerBuilder.VirtualEarthMapType.road, m_oParent.WorldWindowControl, true, m_oParent.WorldWindowControl.CurrentWorld, m_oParent.WorldWindowControl.WorldWindSettings.CachePath, veDir);
            newServerChildSubNode = newServerChildNode.Nodes.Add("Map", "Map", iImageListIndex("live"), iImageListIndex("live"));
            newServerChildSubNode.Tag = q;
            veDir.LayerBuilders.Add(q);
            q = new VEQuadLayerBuilder("Virtual Earth Satellite", VEQuadLayerBuilder.VirtualEarthMapType.aerial, m_oParent.WorldWindowControl, true, m_oParent.WorldWindowControl.CurrentWorld, m_oParent.WorldWindowControl.WorldWindSettings.CachePath, veDir);
            newServerChildSubNode = newServerChildNode.Nodes.Add("Satellite", "Satellite", iImageListIndex("live"), iImageListIndex("live"));
            newServerChildSubNode.Tag = q;
            veDir.LayerBuilders.Add(q);
            q = new VEQuadLayerBuilder("Virtual Earth Map & Satellite", VEQuadLayerBuilder.VirtualEarthMapType.hybrid, m_oParent.WorldWindowControl, true, m_oParent.WorldWindowControl.CurrentWorld, m_oParent.WorldWindowControl.WorldWindSettings.CachePath, veDir);
            newServerChildSubNode = newServerChildNode.Nodes.Add("Map & Satellite", "Map & Satellite", iImageListIndex("live"), iImageListIndex("live"));
            newServerChildSubNode.Tag = q;
            veDir.LayerBuilders.Add(q);
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
      }
      #endregion

      #region Search

      public void ClearSearch()
      {
         AsyncFilterChanged(ServerTree.SearchModeEnum.All, null, string.Empty, false, false);
         m_filterExtents = null;
         m_strSearch = string.Empty;

         // Reselect current node
         TreeNode treeNode = this.SelectedNode;
         this.SelectedNode = null;
         this.SelectedNode = treeNode;
      }
      public void Search(bool bInterSect, WorldWind.GeographicBoundingBox extents, string strText)
      {
         AsyncFilterChanged(ServerTree.SearchModeEnum.All, bInterSect ? new BoundingBox(extents.East, extents.North, extents.West, extents.South) : null, strText, bInterSect, strText != string.Empty);
         m_filterExtents = extents;
         m_strSearch = strText;

         // Reselect current node
         TreeNode treeNode = this.SelectedNode;
         this.SelectedNode = null;
         this.SelectedNode = treeNode;
      }

      protected void FilterTreeNodes(TreeNode node)
      {
         List<TreeNode> nodeList = new List<TreeNode>();
         foreach (TreeNode treeNode in node.Nodes)
            nodeList.Add(treeNode);

         foreach (TreeNode treeNode in nodeList)
         {
            if (treeNode.Tag is ImageBuilder)
            {
               ImageBuilder builder = treeNode.Tag as ImageBuilder;
               if ((m_strSearch != string.Empty && treeNode.Text.IndexOf(m_strSearch, 0, StringComparison.InvariantCultureIgnoreCase) == -1) ||
                  (m_filterExtents != null && m_bAOIFilter && !m_filterExtents.IntersectsWith(builder.Extents) && !m_filterExtents.Contains(builder.Extents)))
                  treeNode.Remove();
               else
                  treeNode.SelectedImageIndex = treeNode.ImageIndex;
            }
         }            
      }

      #endregion

      #region Event Handlers
      /// <summary>
      /// Modify catalog browsing tree
      /// </summary>
      /// <param name="node"></param>
      protected override void AfterSelected(TreeNode treeNode)
      {
         foreach (TreeNode node in m_hRootNode.Nodes)
         {
            TreeNode tnTemp = this.SelectedNode;
            while (tnTemp != null)
            {
               if (tnTemp == node)
                  break;
               tnTemp = tnTemp.Parent;
            }
            if (tnTemp == null)
            {
               node.Collapse();
               foreach (TreeNode subNode in node.Nodes)
                  subNode.Nodes.Clear();
            }
         }
         if (treeNode.Tag is builderdirectoryType || treeNode.Tag is tileserversetType)
         {
            if (treeNode != m_hTileRootNode)
            {
               List<TreeNode> cutList = new List<TreeNode>();
               foreach (TreeNode cutNode in treeNode.Parent.Nodes)
               {
                  if (cutNode != treeNode)
                     cutList.Add(cutNode);
               }
               foreach (TreeNode cutNode in cutList)
                  cutNode.Remove();
            }
            treeNode.Nodes.Clear();

            if (treeNode.Tag is builderdirectoryType)
            {
               builderdirectoryType dir = treeNode.Tag as builderdirectoryType;
               for (int i = 0; i < dir.builderentryCount; i++)
                  LoadBuilderEntryIntoNode(dir.GetbuilderentryAt(i), treeNode);
            }
            else if (treeNode.Tag is tileserversetType)
               LoadBuilderEntryIntoNode(treeNode.Tag as tileserversetType, treeNode);
            
            treeNode.ExpandAll();
         }
         else if (!(treeNode.Tag is BuilderDirectory && bFillWMSCatalogEntriesInTreeNode(treeNode.Tag as BuilderDirectory)))
         {
            if (treeNode == m_hDAPRootNode)
            {
               m_oCurServer = null;
               m_hCurServerTreeNode = null;
               PopulateServerList();
               m_hDAPRootNode.Expand();
            }
            else if (treeNode == m_hTileRootNode)
               m_hTileRootNode.Expand();
         }
         FilterTreeNodes(treeNode);
      }

      protected void OnMouseDoubleClick(object sender, MouseEventArgs e)
      {
         if (this.SelectedNode == m_hRootNode)
            // Never collapse this
            m_hRootNode.Expand();
         else
            AddCurrentDAPDataSet();
      }
      #endregion
   }

   #region TreeNode Sorter
   // Create a node sorter that implements the IComparer interface that puts directories in front of layer builders.
   internal class TreeNodeSorter : System.Collections.IComparer
   {
      public int Compare(object x, object y)
      {
         TreeNode tx = x as TreeNode;
         TreeNode ty = y as TreeNode;

         if (tx.Tag is BuilderDirectory && !(ty.Tag is BuilderDirectory))
            return -1;
         else if (ty.Tag is BuilderDirectory && !(ty.Tag is BuilderDirectory))
            return 1;

         // DAP Sorting
         if (tx.Tag is DataSet && !(ty.Tag is DataSet))
            return 1;
         if (ty.Tag is DataSet && !(tx.Tag is DataSet))
            return -1;
         // Exception, we want "Virtual Earth" on top and "Map & Satellite" at bottom
         if (tx.Text == "Virtual Earth")
            return -1;
         else if (ty.Text == "Virtual Earth")
            return 1;
         if (tx.Text == "Map & Satellite")
            return 1;
         else if (ty.Text == "Map & Satellite")
            return -1;

         // If they are the same type, call Compare.
         return string.Compare(tx.Text, ty.Text);
      }
   }
   #endregion
}
