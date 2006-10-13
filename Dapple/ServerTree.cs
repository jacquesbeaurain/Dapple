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
      protected TreeNode m_hVERootNode;
      protected TreeNode m_hWMSRootNode;

      TreeNodeSorter m_TreeSorter;

      VEQuadLayerBuilder m_VEMapQTB;
      VEQuadLayerBuilder m_VESatQTB;
      VEQuadLayerBuilder m_VEMapAndSatQTB;

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

         m_hRootNode = new TreeNode("Available Servers", iImageListIndex("dapple"), iImageListIndex("dapple"));
         this.Nodes.Add(m_hRootNode);
         m_hRootNode.ToolTipText = "It is possible to double-click on layers in\nhere to add them to the current layers.\nSingle-click browses tree.";

         m_hDAPRootNode = new TreeNode("DAP Servers", iImageListIndex("dap"), iImageListIndex("dap"));
         m_hRootNode.Nodes.Add(m_hDAPRootNode);

         m_hTileRootNode = new TreeNode("Image Tile Servers", iImageListIndex("tile"), iImageListIndex("tile"));
         m_hRootNode.Nodes.Add(m_hTileRootNode);
         BuilderDirectory tileDir = new BuilderDirectory("Image Tile Servers", null, false);

         VETileSetBuilder veDir = new VETileSetBuilder("Virtual Earth", null, false);
         m_hVERootNode = m_hRootNode.Nodes.Add("Virtual Earth");
         m_hVERootNode.SelectedImageIndex = m_hVERootNode.ImageIndex = iImageListIndex("live");
         m_hVERootNode.Tag = veDir;

         m_VEMapQTB = new VEQuadLayerBuilder("Virtual Earth Map", VEQuadLayerBuilder.VirtualEarthMapType.road, m_oParent.WorldWindowControl, true, m_oParent.WorldWindowControl.CurrentWorld, m_oParent.WorldWindowControl.WorldWindSettings.CachePath, veDir);
         m_VESatQTB = new VEQuadLayerBuilder("Virtual Earth Satellite", VEQuadLayerBuilder.VirtualEarthMapType.aerial, m_oParent.WorldWindowControl, true, m_oParent.WorldWindowControl.CurrentWorld, m_oParent.WorldWindowControl.WorldWindSettings.CachePath, veDir);
         m_VEMapAndSatQTB = new VEQuadLayerBuilder("Virtual Earth Map & Satellite", VEQuadLayerBuilder.VirtualEarthMapType.hybrid, m_oParent.WorldWindowControl, true, m_oParent.WorldWindowControl.CurrentWorld, m_oParent.WorldWindowControl.WorldWindSettings.CachePath, veDir);
         veDir.LayerBuilders.Add(m_VEMapQTB);
         veDir.LayerBuilders.Add(m_VESatQTB);
         veDir.LayerBuilders.Add(m_VEMapAndSatQTB);

         WMSCatalogBuilder wmsBuilder = new WMSCatalogBuilder(m_oParent.Settings.WorldWindDirectory, m_oParent.WorldWindowControl, "WMS Servers", null);
         wmsBuilder.LoadingCompleted += new WMSCatalogBuilder.LoadingCompletedCallbackHandler(OnWMSCatalogLoaded);
         wmsBuilder.LoadingFailed += new WMSCatalogBuilder.LoadingFailedCallbackHandler(OnWMSCatalogFailed);

         m_hWMSRootNode = new TreeNode("WMS Servers", iImageListIndex("wms"), iImageListIndex("wms"));
         m_hWMSRootNode.Tag = wmsBuilder;
         m_hRootNode.Nodes.Add(m_hWMSRootNode);

         m_TreeSorter = new TreeNodeSorter(this);
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

      void AddWMSLayers(BuilderDirectory dir, TreeNode treeNode)
      {
         TreeNode subTreeNode;
         TreeNode subSubTreeNode;

         if (dir is WMSServerBuilder && dir.SubList.Count == 1)
         {
            foreach (BuilderDirectory childDir in dir.SubList[0].SubList)
            {
               subTreeNode = treeNode.Nodes.Add(childDir.Name);
               subTreeNode.SelectedImageIndex = subTreeNode.ImageIndex = iImageListIndex("folder");
               subTreeNode.Tag = childDir;
            }
            foreach (LayerBuilder builder in dir.SubList[0].LayerBuilders)
            {
               subTreeNode = treeNode.Nodes.Add(builder.Name, builder.Name, iImageListIndex("layer"), iImageListIndex("layer"));
               subTreeNode.Tag = builder;
            }
         }
         else
         {
            foreach (BuilderDirectory childDir in dir.SubList)
            {
               subTreeNode = treeNode.Nodes.Add(childDir.Name);
               subTreeNode.SelectedImageIndex = subTreeNode.ImageIndex = iImageListIndex("folder");
               subTreeNode.Tag = childDir;
            }

            foreach (LayerBuilder builder in dir.LayerBuilders)
            {
               subTreeNode = treeNode.Nodes.Add(builder.Name, builder.Name, iImageListIndex("layer"), iImageListIndex("layer"));
               subTreeNode.Tag = builder;
            }
         }
      }

      bool bFillWMSCatalogEntriesInTreeNode(BuilderDirectory dir)
      {
         TreeNode treeNode;
         TreeNode treeSelectedNode = this.SelectedNode;
         TreeNode treeWMSNode = this.SelectedNode;
         TreeNode treeWMSRootNode = this.SelectedNode == m_hWMSRootNode ? m_hWMSRootNode : null;

         // Determine if this is even in the WMS servers
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

         // If so, lets do it
         if (treeWMSRootNode != null)
         {
            bool bLoadingOrError = false;
            this.BeginUpdate();

            // Determine which nodes to clear first
            List<TreeNode> cutList = new List<TreeNode>();
            if (treeWMSNode != treeWMSRootNode)
            {
               foreach (TreeNode cutNode in m_hWMSRootNode.Nodes)
               {
                  if (cutNode != treeWMSNode && treeWMSNode != null && treeWMSNode.Tag is WMSServerBuilder)
                     cutList.Add(cutNode);
                  else if (treeWMSNode != null && treeWMSNode != treeWMSRootNode)
                  {
                     foreach (BuilderEntry entry in m_wmsServers)
                     {
                        if (entry.Builder == treeWMSNode.Tag && (entry.Loading || entry.Error))
                        {
                           bLoadingOrError = true;
                           break;
                        }
                     }
                     if (!bLoadingOrError)
                        treeWMSNode.Nodes.Clear();
                  }
               }
               if (treeWMSNode != null && !(treeWMSNode.Tag is WMSServerBuilder) && treeWMSNode.Tag is BuilderDirectory)
               {
                  // different cutlist for folders
                  foreach (TreeNode cutNode in treeWMSNode.Parent.Nodes)
                  {
                     if (cutNode != treeWMSNode)
                        cutList.Add(cutNode);
                  }
               }
            }
            else
               treeWMSRootNode.Nodes.Clear();

            if (!bLoadingOrError)
            {
               if (treeWMSNode != treeWMSRootNode)
                  foreach (TreeNode cutNode in cutList)
                     cutNode.Remove();
            }
            foreach (BuilderEntry entry in m_wmsServers)
            {
               treeNode = null;

               if (treeWMSNode != treeWMSRootNode)
               {
                  if (treeWMSNode != null && treeWMSNode.Tag is WMSServerBuilder && treeWMSNode.Tag == entry.Builder) 
                  {
                     treeNode = treeWMSNode;
                     treeNode.Text = entry.Builder.Name;
                  }
                  else
                     continue;
               }
               else if (entry.Loading || entry.Error || (!m_bAOIFilter && String.IsNullOrEmpty(m_strSearch)) ||
                  (entry.Builder as BuilderDirectory).iGetLayerCount(m_bAOIFilter, m_filterExtents, m_strSearch) > 0)
                  treeNode = m_hWMSRootNode.Nodes.Add(entry.Builder.Name);

               if (treeNode != null)
               {
                  if (entry.Loading)
                  {
                     if (!bLoadingOrError)
                     {
                        treeNode.SelectedImageIndex = treeNode.ImageIndex = iImageListIndex("disserver");

                        // --- updating in progress ---

                        TreeNode hTempNode;
                        hTempNode = new TreeNode("Retrieving Datasets...", iImageListIndex("loading"), iImageListIndex("loading"));
                        hTempNode.Tag = null;
                        treeNode.Nodes.Add(hTempNode);
                     }
                  }
                  else
                  {
                     if (entry.Error)
                     {
                        if (entry.ErrorString != string.Empty)
                           treeNode.Text += " (" + entry.ErrorString + ")";
                        treeNode.SelectedImageIndex = treeNode.ImageIndex = iImageListIndex("offline");
                     }
                     else
                        treeNode.SelectedImageIndex = treeNode.ImageIndex = iImageListIndex("enserver");
                  }
                  treeNode.Tag = entry.Builder;
               }
            }

            if (treeWMSNode != null && treeWMSNode != treeWMSRootNode && treeWMSNode.Tag != null && treeWMSNode.Tag is BuilderDirectory)
               AddWMSLayers(dir, treeWMSNode);
                  
            this.Sorted = true;
            this.TreeViewNodeSorter = m_TreeSorter;
            this.Sort();
            this.TreeViewNodeSorter = null;
            this.Sorted = false;
            m_hWMSRootNode.ExpandAll();

            this.SelectedNode = treeSelectedNode;
            this.EndUpdate();
            this.Refresh();

            return true;
         }
         else
            return false;
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
               DAPQuadLayerBuilder layerBuilder = new DAPQuadLayerBuilder(this.SelectedDAPDataset, m_oParent.WorldWindowControl.CurrentWorld, m_strCacheDir, m_oCurServer, null);
               m_oParent.AddLayerBuilder(layerBuilder);
            }
         }
         else if (this.SelectedNode != null && SelectedNode.Tag is LayerBuilder)
            m_oParent.AddLayerBuilder(SelectedNode.Tag as LayerBuilder);
      }

      public void RefreshCurrentServer()
      {
         if (this.SelectedNode == null)
            return;

         TreeNode treeNodeSel = this.SelectedNode;
         treeNodeSel.Nodes.Clear();

         // --- updating in progress ---

         TreeNode hTempNode;
         hTempNode = new TreeNode("Retrieving Datasets...", iImageListIndex("loading"), iImageListIndex("loading"));
         hTempNode.Tag = null;
         treeNodeSel.Nodes.Add(hTempNode);
         this.Refresh();

         if (treeNodeSel.Tag is WMSServerBuilder)
         {
            WMSServerBuilder serverBuilder = treeNodeSel.Tag as WMSServerBuilder;
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
               IBuilder parentCatalog = treeNodeSel.Tag as IBuilder;

               while (parentCatalog != null && !(parentCatalog is WMSCatalogBuilder))
                  parentCatalog = parentCatalog.Parent;

               if (parentCatalog != null)
               {
                  WMSCatalogBuilder wmsBuilder = parentCatalog as WMSCatalogBuilder;
                  wmsBuilder.RemoveServer(serverBuilder.URL);
                  treeNodeSel.SelectedImageIndex = treeNodeSel.ImageIndex = iImageListIndex("disserver");
                  treeNodeSel.Tag = builderEntry.Builder = wmsBuilder.AddServer(serverBuilder.URL, serverBuilder.Parent as BuilderDirectory);
                  wmsBuilder.SubList.Add(builderEntry.Builder as BuilderDirectory);
               }
            }
         }
         else if (treeNodeSel.Tag is Server)
         {
            (treeNodeSel.Tag as Server).UpdateConfiguration();
            ClearCatalog();
            GetCatalogHierarchy();
            GetDatasetCount(treeNodeSel.Tag as Server);
            RefreshTreeNodeText();
         }
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
            TreeNode treeNode = null;
            BuilderEntry builderEntry;
            m_wmsServers.Add(builderEntry = new BuilderEntry(wmsBuilder.AddServer(strCapUrl, wmsBuilder), false, true, String.Empty));
            if (bUpdateTree)
            {
               this.BeginUpdate();
               m_hWMSRootNode.Nodes.Clear();
               treeNode = WMSRootNodes.Add(strCapUrl);
               treeNode.SelectedImageIndex = treeNode.ImageIndex = iImageListIndex("disserver");
               treeNode.Tag = builderEntry.Builder;
               this.EndUpdate();
            }
            wmsBuilder.SubList.Add(builderEntry.Builder as BuilderDirectory);

            if (treeNode != null)
               this.SelectedNode = treeNode;

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
            m_wmsServers.Clear();
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

      protected void UpdateNodeCounts(TreeNode node)
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

      protected override void RefreshTreeNodeText()
      {
         base.RefreshTreeNodeText();

         // Just count the servers with data in the DAP tree
         int iCount = 0;
         foreach (TreeNode treeNode in m_hDAPRootNode.Nodes)
         {
            if ((treeNode.Tag as Server).DatasetCount > 0)
               iCount++;
         }
         m_hDAPRootNode.Text = "DAP Servers (" + iCount.ToString() + ")";
             
        
         // WMS Servers 
         // First just count the servers
         iCount = 0;
         foreach (BuilderEntry entry in m_wmsServers)
         {
            int iDatasetCount = (entry.Builder as BuilderDirectory).iGetLayerCount(m_bAOIFilter, m_filterExtents, m_strSearch);

            if (entry.Loading || entry.Error || iDatasetCount > 0)
               iCount++;

            foreach (TreeNode treeNode in m_hWMSRootNode.Nodes)
            {
               if (treeNode.Tag == entry.Builder && !entry.Loading && !entry.Error)
                  treeNode.Text = entry.Builder.Name + " (" + iDatasetCount.ToString() + ")";
            }
         }
         m_hWMSRootNode.Text = "WMS Servers (" + iCount.ToString() + ")";
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
            }
            if (treeNode.Tag is BuilderDirectory && !(treeNode.Tag is WMSCatalogBuilder) && 
               (treeNode.Tag as BuilderDirectory).iGetLayerCount(m_bAOIFilter, m_filterExtents, m_strSearch) == 0)
               treeNode.Remove();
         }
         
         // Update counts accross the board
         UpdateCounts();
      }

      #endregion

      #region Event Handlers
      /// <summary>
      /// Modify catalog browsing tree
      /// </summary>
      /// <param name="node"></param>
      protected override void AfterSelected(TreeNode treeNode)
      {
         base.AfterSelected(treeNode);

         this.BeginUpdate();

         // Cleanup everything but myself
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
               // Just collapse and clear the subnodes with the DAP servers
               if (node == m_hDAPRootNode)
               {
                  node.Collapse();
                  foreach (TreeNode subNode in node.Nodes)
                     subNode.Nodes.Clear();
               }
               else
                  node.Nodes.Clear();
            }
         }
         if (!(treeNode.Tag is VEQuadLayerBuilder))
            m_hVERootNode.Nodes.Clear();
         if (treeNode.Tag is VETileSetBuilder)
         {
            TreeNode treeSubNode = treeNode.Nodes.Add("Map", "Map", iImageListIndex("live"), iImageListIndex("live"));
            treeSubNode.Tag = m_VEMapQTB;
            treeSubNode = treeNode.Nodes.Add("Satellite", "Satellite", iImageListIndex("live"), iImageListIndex("live"));
            treeSubNode.Tag = m_VESatQTB;
            treeSubNode = treeNode.Nodes.Add("Map & Satellite", "Map & Satellite", iImageListIndex("live"), iImageListIndex("live"));
            treeSubNode.Tag = m_VEMapAndSatQTB;
            treeNode.ExpandAll();
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
         this.EndUpdate();
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

         public int Compare(object x, object y)
         {
            TreeNode tx = x as TreeNode;
            TreeNode ty = y as TreeNode;
            ServerTree tree = tx.TreeView as ServerTree;

            // Put unresolved loading servers at bottom
            if (tx.Text.StartsWith("http://"))
               return int.MaxValue;
            else if (ty.Text.StartsWith("http://"))
               return int.MinValue;

            // Sort Root Nodes
            if (tx.Nodes == m_ServerTree.DAPRootNodes)
               return -2;
            if (tx.Nodes == m_ServerTree.TileRootNodes)
               return -1;
            if (tx.Nodes == m_ServerTree.VERootNodes)
               return 1;
            if (tx.Nodes == m_ServerTree.WMSRootNodes)
               return 2;

            if (ty.Nodes == m_ServerTree.DAPRootNodes)
               return 2;
            if (ty.Nodes == m_ServerTree.TileRootNodes)
               return 1;
            if (ty.Nodes == m_ServerTree.VERootNodes)
               return -1;
            if (ty.Nodes == m_ServerTree.WMSRootNodes)
               return -2;

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
