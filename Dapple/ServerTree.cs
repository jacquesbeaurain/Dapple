using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Dapple.LayerGeneration;

namespace Dapple
{
   /// <summary>
   /// Derived tree that not only contains DAP servers but also WMS and image tile servers
   /// </summary>
   public class ServerTree : Geosoft.GX.DAPGetData.ServerTree
   {
      #region Members
      protected TreeNode m_hTileRootNode;
      protected TreeNode m_hWMSRootNode;
      protected MainForm m_oParent;
      #endregion

      #region Constructor/Disposal
      /// <summary>
      /// Constructor
      /// </summary>
      /// <param name="strCacheDir"></param>
      public ServerTree(string strCacheDir, MainForm oParent)
         : base(strCacheDir)
      {
         m_oParent = oParent;

         // Extra icons
         m_hNodeimageList.Images.Add("dap_gray", global::Dapple.Properties.Resources.dap_gray);
         m_hNodeimageList.Images.Add("error", global::Dapple.Properties.Resources.error);
         m_hNodeimageList.Images.Add("folder_gray", global::Dapple.Properties.Resources.folder_gray);
         m_hNodeimageList.Images.Add("layer", global::Dapple.Properties.Resources.layer);
         m_hNodeimageList.Images.Add("live", global::Dapple.Properties.Resources.live);
         m_hNodeimageList.Images.Add("tile", global::Dapple.Properties.Resources.tile);
         m_hNodeimageList.Images.Add("tile_gray", global::Dapple.Properties.Resources.tile_gray);
         m_hNodeimageList.Images.Add("georef_image", global::Dapple.Properties.Resources.georef_image);
         m_hNodeimageList.Images.Add("time", global::Dapple.Properties.Resources.time_icon);
         m_hNodeimageList.Images.Add("wms", global::Dapple.Properties.Resources.wms);
         m_hNodeimageList.Images.Add("wms_gray", global::Dapple.Properties.Resources.wms_gray);
         m_hNodeimageList.Images.Add("marble", global::Dapple.Properties.Resources.marble_icon);
         m_hNodeimageList.Images.Add("nasa", global::Dapple.Properties.Resources.nasa);
         m_hNodeimageList.Images.Add("usgs", global::Dapple.Properties.Resources.usgs);
         m_hNodeimageList.Images.Add("worldwind_central", global::Dapple.Properties.Resources.worldwind_central);

         m_hTileRootNode = new TreeNode("Image Tile Servers", iImageListIndex("tile"), iImageListIndex("tile"));
         this.Nodes.Add(m_hTileRootNode);

         m_hWMSRootNode = new TreeNode("WMS Servers", iImageListIndex("wms"), iImageListIndex("wms"));
         this.Nodes.Add(m_hWMSRootNode);

         this.MouseDoubleClick += new MouseEventHandler(OnMouseDoubleClick);
      }
      protected override void Dispose(bool disposing)
      {
         base.Dispose(disposing);
      }
      #endregion

      #region Properties
      #endregion

      #region Methods
      public bool AddWMSServer(string strCapUrl, string strHierarchy, string strCacheDir)
      {
         return true;
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
         if (this.SelectedDAPDataset != null)
         {
            if (!m_oParent.bContainsDAPLayer(m_oCurServer, this.SelectedDAPDataset))
            {
               DAPQuadLayerBuilder layerBuilder = new DAPQuadLayerBuilder(this.SelectedDAPDataset, m_oParent.WorldWindowControl.CurrentWorld, m_strCacheDir, m_oCurServer, null);
               m_oParent.AddLayerBuilder(layerBuilder);
            }
         }
      }
      #endregion
   }
}
