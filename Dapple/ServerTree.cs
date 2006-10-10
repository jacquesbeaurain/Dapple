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
      protected TreeNode m_hRootNode;
      protected TreeNode m_hTileRootNode;
      protected TreeNode m_hWMSRootNode;
      protected MainForm m_oParent;
      protected TriStateTreeView m_layerTree;
      protected LayerBuilderList m_activeLayers;
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
         this.ShowPlusMinus = true;
         this.TreeViewNodeSorter = new TreeNodeSorter();
         this.Scrollable = true;

         m_hRootNode = new TreeNode("Available Servers", iImageListIndex("dapple"), iImageListIndex("dapple"));
         this.Nodes.Add(m_hRootNode);
         m_hRootNode.ToolTipText = "Double-click on layers in here to\nadd them to the current layers.";

         m_hDAPRootNode = new TreeNode("DAP Servers", iImageListIndex("dap"), iImageListIndex("dap"));
         m_hRootNode.Nodes.Add(m_hDAPRootNode);

         m_hTileRootNode = new TreeNode("Image Tile Servers", iImageListIndex("tile"), iImageListIndex("tile"));
         m_hRootNode.Nodes.Add(m_hTileRootNode);

         WMSCatalogBuilder wmsBuilder = new WMSCatalogBuilder(m_oParent.Settings.WorldWindDirectory, m_oParent.WorldWindowControl, "WMS Servers", null);
         wmsBuilder.LoadingCompleted += new LayerGeneration.LoadingCompletedCallbackHandler(OnCatalogLoaded);
         wmsBuilder.LoadingFailed += new LoadingFailedCallbackHandler(OnCatalogFailed);

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
      #endregion

      #region Catalog Loaded/Failed Handlers

      delegate void InvokeLoadedCatalog(BuilderDirectory directory);
      delegate void InvokeFailedCatalog(BuilderDirectory directory, string message);

      void OnCatalogLoaded(BuilderDirectory directory)
      {
         if (!this.IsDisposed)
         {
            if (this.InvokeRequired)
               this.BeginInvoke(new InvokeLoadedCatalog(LoadCatalog), new object[] { directory });
            else
               LoadCatalog(directory);
         }
      }

      void OnCatalogFailed(BuilderDirectory directory, string message)
      {
         if (!this.IsDisposed)
         {
            if (this.InvokeRequired)
               this.BeginInvoke(new InvokeFailedCatalog(CatalogFailed), new object[] { directory, message });
            else
               CatalogFailed(directory, message);
         }
      }

      void CatalogFailed(BuilderDirectory directory, string message)
      {
         TreeNode treeNode = TreeUtils.FindNodeBFS(directory, m_hRootNode.Nodes);
         if (treeNode != null)
         {
            treeNode.Nodes.Clear();
            treeNode.Text = "Failed: " + directory.Name + ": " + message;
            treeNode.SelectedImageIndex = treeNode.ImageIndex = iImageListIndex("offline");
         }
      }

      void ProcessCatalogDirectory(BuilderDirectory dir, TreeNode parent)
      {
         if (parent.Tag is WMSServerBuilder && dir.SubList.Count == 1)
         {
            BuilderDirectory builders = (BuilderDirectory)dir.SubList[0];
            ProcessCatalogDirectory(builders, parent);
         }
         else
         {
            foreach (BuilderDirectory childDir in dir.SubList)
            {
               TreeNode treeNode = parent.Nodes.Add(childDir.Name);
               treeNode.SelectedImageIndex = treeNode.ImageIndex = iImageListIndex("folder");
               treeNode.Tag = childDir;
               ProcessCatalogDirectory(childDir, treeNode);
            }
         }
         foreach (LayerBuilder builder in dir.LayerBuilders)
         {
            TreeNode treeNode;
            if (builder is DAPQuadLayerBuilder)
            {
               int iImage;
               DAPQuadLayerBuilder dapbuilder = (DAPQuadLayerBuilder)builder;

               iImage = iImageIndex(dapbuilder.DAPType.ToLower());
               if (iImage == -1)
                  iImage = iImageListIndex("layer");
               treeNode = parent.Nodes.Add(builder.Name, builder.Name, iImage, iImage);
            }
            else
               treeNode = parent.Nodes.Add(builder.Name, builder.Name, iImageListIndex("layer"), iImageListIndex("layer"));
            treeNode.Tag = builder;
         }
      }

      void LoadCatalog(BuilderDirectory directory)
      {
         this.BeginUpdate();

         TreeNode treeNode = TreeUtils.FindNodeBFS(directory, m_hRootNode.Nodes);

         if (treeNode == null)
         {
            TreeNode treeParentNode = TreeUtils.FindNodeBFS(directory.Parent, m_hRootNode.Nodes);

            if (treeParentNode == null)
               return;

            if (directory is WMSServerBuilder)
            {
               (treeParentNode.Tag as BuilderDirectory).SubList.Add(directory);
               treeNode = treeParentNode.Nodes.Add(directory.Name);
               treeNode.SelectedImageIndex = treeNode.ImageIndex = iImageListIndex("enserver");
               treeNode.Tag = directory;
            }
            else
            {
               treeNode = treeParentNode.Nodes.Add(directory.Name);
               treeNode.SelectedImageIndex = treeNode.ImageIndex = iImageListIndex("folder");
               treeNode.Tag = directory;
            }
         }
         else
         {
            treeNode.Text = directory.Name;
         }

         treeNode.SelectedImageIndex = treeNode.ImageIndex = (directory is WMSServerBuilder ? iImageListIndex("enserver") : iImageListIndex("layer"));
         ProcessCatalogDirectory(directory, treeNode);
         // Remove the nodes (we can restore them using the IBuilder parent/client relations ships from here on)
         treeNode.Nodes.Clear();
         this.Sort();
         this.EndUpdate();

         m_layerTree.BeginUpdate();
         if (directory is WMSServerBuilder)
         {
            // Find provider in parents first
            IBuilder parentCatalog = directory.Parent;

            while (parentCatalog != null && !(parentCatalog is WMSCatalogBuilder))
               parentCatalog = parentCatalog.Parent;

            if (parentCatalog != null)
            {
               WMSServerBuilder wmsserver = directory as WMSServerBuilder;
               WMSCatalogBuilder provider = parentCatalog as WMSCatalogBuilder;

               foreach (LayerBuilderContainer container in m_activeLayers)
               {
                  if (container.Uri.StartsWith(WMSQuadLayerBuilder.URLProtocolName) && wmsserver.URL == WMSQuadLayerBuilder.ServerURLFromURI(container.Uri))
                  {
                     LayerBuilder builder = WMSQuadLayerBuilder.GetBuilderFromURI(container.Uri, provider, m_oParent.WorldWindowControl, wmsserver);
                     if (builder != null)
                        m_activeLayers.RefreshFromSource(container, builder);
                  }
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
            nameNode.InnerText = Name;
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
            // Need to have a catalog builder in treenode's parents
            IBuilder parentCatalog = this.SelectedNode.Tag as IBuilder;

            while (parentCatalog != null && !(parentCatalog is WMSCatalogBuilder))
               parentCatalog = parentCatalog.Parent;

            if (parentCatalog != null)
            {
               WMSCatalogBuilder wmsBuilder = parentCatalog as WMSCatalogBuilder;
               wmsBuilder.RemoveServer(serverBuilder.URL);

               BuilderDirectory wmsDir = wmsBuilder.AddServer(serverBuilder.URL, serverBuilder.Parent as BuilderDirectory);
               wmsBuilder.SubList.Add(wmsDir);

               this.SelectedNode.Nodes.Clear();
               this.SelectedNode.Text = "Loading: " + serverBuilder.URL;
               this.SelectedNode.SelectedImageIndex = this.SelectedNode.ImageIndex = iImageListIndex("time");
               this.SelectedNode.Tag = wmsDir;
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
               // Need to have a catalog builder in treenode's parents
               IBuilder parentCatalog = serverBuilder as IBuilder;

               while (parentCatalog != null && !(parentCatalog is WMSCatalogBuilder))
                  parentCatalog = parentCatalog.Parent;

               if (parentCatalog != null && parentCatalog is WMSCatalogBuilder)
                  (parentCatalog as WMSCatalogBuilder).RemoveServer(serverBuilder.URL);
            }

            (treeNode.Parent.Tag as BuilderDirectory).SubList.Remove(treeNode.Tag as BuilderDirectory);
         }

         treeNode.Parent.Nodes.Remove(treeNode);
      }

      public bool AddWMSServer(string strCapUrl)
      {
         WMSCatalogBuilder wmsBuilder = m_hWMSRootNode.Tag as WMSCatalogBuilder;
         WorldWind.WMSList wmsList = wmsBuilder.FindServer(strCapUrl);
         if (wmsList == null)
         {
            TreeNode treeNode = m_hWMSRootNode.Nodes.Add(strCapUrl);
            treeNode.SelectedImageIndex = treeNode.ImageIndex = iImageListIndex("time");
            treeNode.Tag = wmsBuilder.AddServer(strCapUrl, wmsBuilder);
            wmsBuilder.SubList.Add(treeNode.Tag as BuilderDirectory);
            treeNode.Text = (treeNode.Tag as BuilderDirectory).Name;
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
                  LoadBuilderEntryIntoNode(entry, m_hRootNode);
               }
            }

            AsyncFilterChanged(ServerTree.SearchModeEnum.All, null, null, false, false);
         }
         finally
         {
            // Always expand root node
            m_hRootNode.Expand();
            this.Sort();

            // Remove the first level nodes (we can restore them using the IBuilder parent/client relations ships from here on)
            foreach (TreeNode node in m_hTileRootNode.Nodes)
               node.Nodes.Clear();
            foreach (TreeNode node in m_hWMSRootNode.Nodes)
               node.Nodes.Clear();
            this.EndUpdate();
         }
      }

      public void SaveToView(DappleView oView)
      {
         serversType servers = oView.View.Newservers();

         foreach (TreeNode node in m_hRootNode.Nodes)
         {
            builderentryType entry = servers.Newbuilderentry();
            PopulateEntry(entry, node);
            servers.Addbuilderentry(entry);
         }
         oView.View.Addservers(servers);
      }

      protected void PopulateEntry(builderentryType entry, TreeNode node)
      {
         if (node.Tag is Server)
         {
            dapcatalogType dap = entry.Newdapcatalog();
            dap.Addurl(new SchemaString((node.Tag as Server).Url));
            entry.Adddapcatalog(dap);
         }
         else 
         {
            IBuilder builder = null;
            
            if (node.Tag is IBuilder)
               builder = node.Tag as IBuilder;

            if (builder is WMSServerBuilder)
            {
               wmscatalogType wms = entry.Newwmscatalog();
               wms.Addcapabilitiesurl(new SchemaString((builder as WMSServerBuilder).URL));
               entry.Addwmscatalog(wms);
            }
            else if (builder is VETileSetBuilder)
            {
               virtualearthType ve = entry.Newvirtualearth();
               ve.Addname(new SchemaString(builder.Name));
               entry.Addvirtualearth(ve);
            }
            else if (builder is TileSetSet)
            {
               tilelayersType layers = null;
               tileserversetType set = entry.Newtileserverset();
               set.Addname(new SchemaString(builder.Name));
               foreach (TreeNode subnode in node.Nodes)
               {
                  QuadLayerBuilder layer = subnode.Tag as QuadLayerBuilder;
                  if (layer != null)
                  {
                     if (layers == null)
                        layers = set.Newtilelayers();

                     tilelayerType tilelayer = layers.Newtilelayer();
                     tilelayer.Addname(new SchemaString(subnode.Text));
                     tilelayer.Adddataset(new SchemaString(layer.TileServerDatasetName));
                     tilelayer.Addimageextension(new SchemaString(layer.ImageFileExtension));
                     tilelayer.Addurl(new SchemaString(layer.TileServerURL));
                     tilelayer.Addlevels(new SchemaInt(layer.Levels));
                     tilelayer.Addlevelzerotilesize(new SchemaDouble((double)layer.LevelZeroTileSize));
                     tilelayer.Addtilepixelsize(new SchemaInt(layer.ImagePixelSize));

                     boundingboxType bounds = tilelayer.Newboundingbox();
                     bounds.Addmaxlat(new SchemaDouble(layer.Extents.North));
                     bounds.Addminlat(new SchemaDouble(layer.Extents.South));
                     bounds.Addmaxlon(new SchemaDouble(layer.Extents.East));
                     bounds.Addminlon(new SchemaDouble(layer.Extents.West));
                     tilelayer.Addboundingbox(bounds);
                     layers.Addtilelayer(tilelayer);
                  }
               }
               if (layers != null)
                  set.Addtilelayers(layers);

               entry.Addtileserverset(set);
            }
            else if (builder == null || builder is BuilderDirectory)
            {
               builderdirectoryType dir = entry.Newbuilderdirectory();
               dir.Addname(new SchemaString(node.Text));

               if (builder == null)
               {
                  if (node == m_hDAPRootNode)
                     dir.Addspecialcontainer(new SpecialDirectoryType("DAPServers"));
                  else if (node == m_hWMSRootNode)
                     dir.Addspecialcontainer(new SpecialDirectoryType("WMSServers"));
                  else if (node == m_hTileRootNode)
                     dir.Addspecialcontainer(new SpecialDirectoryType("ImageServers"));
               }
               foreach (TreeNode subnode in node.Nodes)
               {
                  builderentryType subentry = dir.Newbuilderentry();
                  PopulateEntry(subentry, subnode);
                  dir.Addbuilderentry(subentry);
               }

               entry.Addbuilderdirectory(dir);
            }
         }
      }

      void LoadBuilderEntryIntoNode(builderentryType entry, TreeNode serverNode)
      {
         int i, j;
         IBuilder Parent = null;
         BuilderDirectory dir;
         TreeNodeCollection serverNodes;
         TreeNode newServerNode;
         TreeNode newServerChildNode;
         TreeNode newServerChildSubNode;

         if (serverNode != null)
         {
            Parent = (IBuilder)serverNode.Tag;
            serverNodes = serverNode.Nodes;
         }
         else
         {
            serverNodes = m_hRootNode.Nodes;
         }


         if (entry.Hasbuilderdirectory())
         {
            if (entry.builderdirectory.Hasspecialcontainer())
            {
               if (entry.builderdirectory.specialcontainer.Value == "ImageServers")
                  newServerNode = m_hTileRootNode;
               else if (entry.builderdirectory.specialcontainer.Value == "DAPServers")
                  newServerNode = m_hDAPRootNode;
               else if (entry.builderdirectory.specialcontainer.Value == "WMSServers")
                  newServerNode = m_hWMSRootNode;
               else
                  return;
            }
            else
            {
               newServerNode = serverNodes.Add(entry.builderdirectory.name.Value);
               newServerNode.SelectedImageIndex = newServerNode.ImageIndex = iImageListIndex("local");
               dir = new BuilderDirectory(newServerNode.Text, Parent, false);
               newServerNode.Tag = dir;
            }
            
            for (i = 0; i < entry.builderdirectory.builderentryCount; i++)
               LoadBuilderEntryIntoNode(entry.builderdirectory.GetbuilderentryAt(i), newServerNode);
         }
         else if (entry.Hasdapcatalog())
         {
            Geosoft.GX.DAPGetData.Server dapServer;
            AddDAPServer(entry.dapcatalog.url.Value, out dapServer);
         }
         else if (entry.Haswmscatalog())
            AddWMSServer(entry.wmscatalog.capabilitiesurl.Value);
         else if (entry.Hastileserverset())
         {
            TileSetSet tileDir = new TileSetSet(entry.tileserverset.name.Value, Parent, false);
            newServerChildNode = serverNodes.Add(entry.tileserverset.name.Value);
            newServerChildNode.SelectedImageIndex = newServerChildNode.ImageIndex = iImageListIndex("tile");
            newServerChildNode.Tag = tileDir;
            if (entry.tileserverset.Hastilelayers())
            {
               for (j = 0; j < entry.tileserverset.tilelayers.tilelayerCount; j++)
               {
                  tilelayerType tile = entry.tileserverset.tilelayers.GettilelayerAt(j);
                  newServerChildSubNode = newServerChildNode.Nodes.Add(tile.name.Value);
                  newServerChildSubNode.SelectedImageIndex = newServerChildSubNode.ImageIndex = iImageListIndex("layer");

                  int iDistance = tile.Hasdistanceabovesurface() ? tile.distanceabovesurface.Value : Convert.ToInt32(tilelayerType.GetdistanceabovesurfaceDefault());
                  int iPixelSize = tile.Hastilepixelsize() ? tile.tilepixelsize.Value : Convert.ToInt32(tilelayerType.GettilepixelsizeDefault());
                  WorldWind.ImageTileService tileService = new WorldWind.ImageTileService(tile.dataset.Value, tile.url.Value);
                  QuadLayerBuilder quadBuilder = new QuadLayerBuilder(tile.name.Value, iDistance, true, new WorldWind.GeographicBoundingBox(tile.boundingbox.maxlat.Value, tile.boundingbox.minlat.Value, tile.boundingbox.minlon.Value, tile.boundingbox.maxlon.Value), (decimal)tile.levelzerotilesize.Value, tile.levels.Value, iPixelSize, tileService,
                                                          tile.imageextension.Value, 255, m_oParent.WorldWindowControl.CurrentWorld, m_oParent.Settings.CachePath, m_oParent.Settings.CachePath, tileDir);
                  newServerChildSubNode.Tag = quadBuilder;
               }
            }
         }
         else if (entry.Hasvirtualearth())
         {
            VETileSetBuilder veDir = new VETileSetBuilder(entry.virtualearth.name.Value, Parent, false);
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
      #endregion

      #region Search

      public void ClearSearch()
      {
      }
      public void Search(bool bInterSect, WorldWind.GeographicBoundingBox extents, string strText)
      {
        // FilterTreeNodes(null, filterTree.Nodes, GeographicBoundingBox.FromQuad(m_oParent.WorldWindowControl.GetViewBox()), bIntersect, "");
      }
      protected void FilterTreeNodes(TreeNode node, TreeNodeCollection col, WorldWind.GeographicBoundingBox filterExtents, bool bIntersect, string filterText)
      {
         /*
         List<TreeNode> nodeList = new List<TreeNode>();
         foreach (TreeNode treeNode in col)
            nodeList.Add(treeNode);

         foreach (TreeNode treeNode in nodeList)
         {
            if (treeNode.Tag is ImageBuilder)
            {
               ImageBuilder builder = treeNode.Tag as ImageBuilder;
               if ((filterText.Length > 0 && treeNode.Text.IndexOf(filterText, 0, StringComparison.InvariantCultureIgnoreCase) == -1) || (filterExtents != null && ((bIntersect && !filterExtents.IntersectsWith(builder.Extents)) || (!bIntersect && !filterExtents.Contains(builder.Extents)))))
                  treeNode.Remove();
               else
                  treeNode.SelectedImageIndex = treeNode.ImageIndex;
            }
            /*TODO:else if (treeNode.Tag is DAPServerBuilder && filterExtents != null)
            {
               // Poke the DAP server again to get a filtered catalog from it.

               DAPServerBuilder serverBuilder = treeNode.Tag as DAPServerBuilder;
               // Need to have a catalog builder in treenode's parents
               IBuilder parentCatalog = ServerBuilderItem as IBuilder;

               while (parentCatalog != null && !(parentCatalog is DAPCatalogBuilder))
                  parentCatalog = parentCatalog.Parent;

               if (parentCatalog != null)
               {
                  DAPCatalogBuilder dapBuilder = (DAPCatalogBuilder)(parentCatalog as DAPCatalogBuilder).Clone();
                  dapBuilder.RemoveServer(serverBuilder.Server.Url);
                  dapBuilder.ServerTree = treeNode.TreeView;

                     Geosoft.GX.DAPGetData.Server dapServer = new Geosoft.GX.DAPGetData.Server(serverBuilder.Server.Url, m_oParent.Settings.CachePath);
                     BuilderDirectory dapDir = dapBuilder.AddDapServer(dapServer, new Geosoft.Dap.Common.BoundingBox(filterExtents.East, filterExtents.North, filterExtents.West, filterExtents.South), serverBuilder.Parent);
                     dapBuilder.SubList.Add(dapDir);

                     treeNode.Nodes.Clear();
                     treeNode.Text = "Loading: " + serverBuilder.Server.Url;
                     treeNode.SelectedImageIndex = treeNode.ImageIndex = iImageListIndex("time");
                     treeNode.Tag = dapDir;
               }
            }
            else if (treeNode.Tag is BuilderDirectory)
               // Recurse
               FilterTreeNodes(treeNode, treeNode.Nodes, filterExtents, bIntersect, filterText);
         }

         // Remove empty folders
         if (node != null && node.Tag is BuilderDirectory)
         {
            int iDatasets = 0;
            CountTreeNodeDatasets(col, ref iDatasets);
            if (iDatasets == 0)
            {
               if (node.Parent != null)
                  node.Remove();
               else
               {
                  if (node.ImageIndex == iImageListIndex("dap"))
                     node.ImageIndex = node.SelectedImageIndex = iImageListIndex("dap_gray");
                  else if (node.ImageIndex == iImageListIndex("wms"))
                     node.ImageIndex = node.SelectedImageIndex = iImageListIndex("wms_gray");
                  else if (node.ImageIndex == iImageListIndex("tile"))
                     node.ImageIndex = node.SelectedImageIndex = iImageListIndex("tile_gray");
               }
            }
         }*/
      }

      #endregion

      #region Event Handlers
      /// <summary>
      /// Modify catalog browsing tree
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      protected override void OnAfterSelect(object sender, TreeViewEventArgs e)
      {
         base.OnAfterSelect(sender, e);
         if (m_bSelect)
         {
            TreeNode hCurrentNode;

            m_bSelect = false;

            hCurrentNode = e.Node;

            m_bSelect = true;
         }
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

   // Create a node sorter that implements the IComparer interface that puts directories in front of layer builders.
   internal class TreeNodeSorter : System.Collections.IComparer
   {
      public int Compare(object x, object y)
      {
         TreeNode tx = x as TreeNode;
         TreeNode ty = y as TreeNode;

         // Don't sort these
         if (tx.Tag is Server || ty.Tag is Server || tx.Tag is DataSet || ty.Tag is DataSet ||
            tx.Parent == null || ty.Parent == null)
            return 0;

         if (tx.Tag is BuilderDirectory && !(ty.Tag is BuilderDirectory))
            return -1;
         else if (ty.Tag is BuilderDirectory && !(ty.Tag is BuilderDirectory))
            return 1;

         // Exception, we want "Virtual Earth" on top and "Map & Satellite" at bottom
         if (tx.Text == "Virtual Earth")
            return -1;
         else if (ty.Text == "Virtual Earth")
            return 1;
         if (tx.Text == "Map & Satellite")
            return 1;
         else if (ty.Text == "Map & Satellite")
            return -1;

         // If they are the same length, call Compare.
         return string.Compare(tx.Text, ty.Text);
      }
   }
}
