using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Text;
using System.Web;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml;
using WorldWind;
using WorldWind.Net;
using WorldWind.Renderable;
using Dapple.LayerGeneration;
using Geosoft.DotNetTools;
using ConfigurationWizard;
using dappleview;
using DM.SharedMemory;
using Microsoft.Win32;
using Altova.Types;


namespace Dapple
{
   public partial class MainForm : Form
   {
      [DllImport("User32.dll")]
      private static extern UInt32 RegisterWindowMessageW(String strMessage);


      #region Statics

      public static UInt32 OpenViewMessage = RegisterWindowMessageW("Dapple.OpenViewMessage");
      public const string ViewExt = ".dapple";
      public const string LastView = "lastview" + ViewExt;
      public const string HomeView = "home" + ViewExt;
      public const string DefaultView = "default" + ViewExt;
      public const string ViewFileDescr = "Dapple View"; 
      public const string WebsiteUrl = "http://dapple.geosoft.com/";
      public const string VersionFile = "version.txt";
      public const string LicenseWebsiteUrl = "http://dapple.geosoft.com/license.asp";
      public const string CreditsWebsiteUrl = "http://dapple.geosoft.com/credits.asp";
      public const string WMSWebsiteHelpUrl = "http://dapple.geosoft.com/help/wms.asp";
      public const string DAPWebsiteHelpUrl = "http://dapple.geosoft.com/help/dap.asp";
      public static string UserPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DappleData");

      /// <summary>
      /// Try to open url in web browser
      /// </summary>
      /// <param name="url">The url to open in browser</param>
      public static void BrowseTo(string url)
      {
         try
         {
            System.Diagnostics.Process.Start(url);
         }
         catch
         {
            try
            {
               System.Diagnostics.Process.Start("IExplore.exe", url);
            }
            catch
            {
            }
         }
      }

      #endregion

      #region Private Members

      private ImageList iconImageList = new ImageList();
      private TreeView treeViewServers;
      private TreeView treeViewServerBackup;
      private TriStateTreeView triStateTreeViewLayers;
      private string m_executablePath;
      private WorldWindow m_worldWindow = new WorldWindow();
      private NASA.Plugins.BMNG m_BMNGForm;
      private WorldWind.OverviewControl m_overviewCtl;
      private string openView = "";
      private string openGeoTiff = "";
      private string lastView = "";

      private bool rightmouse_context = false;
      private bool checked_context = false;

      private LayerBuilderList m_ActiveLayers;

      Murris.Plugins.Compass m_oCompass;

      private object lockCatalogUpdates = new object();
      private int iLastTransparency = 255; // It breaks the message loop to access the actual property during paint
      private DappleToolStripRenderer toolStripRenderer;
      /* private int iServerPanelLastMinSize, iServerPanelLastPos;
      private int iLayerPanelLastMinSize, iLayerPanelLastPos;
      private int iOverviewPanelLastMinSize, iOverviewPanelLastPos;
      
      */
      #endregion

      #region Properties

      private LayerBuilderContainer layerBuilder = null;
      private IBuilder serverBuilder = null;

      LayerBuilderContainer LayerBuilderItem
      {
         get
         {
            if (this.layerBuilder != null)
               return this.layerBuilder;
            else
               return null;
         }
         set
         {
            this.layerBuilder = value;

            this.toolStripMenuItemgoTo.Visible = true;
            this.toolStripSeparatorLayerGoto.Visible = true;
            this.toolStripMenuItemRemoveAll.Visible = true;
            this.toolStripMenuItemremove.Visible = true;
            this.toolStripMenuItemremoveAllButThis.Visible = true;
            this.toolStripSeparatorLayerRemove.Visible = true;
            this.toolStripSeparatorLayerZOrder.Visible = true;
            this.toolStripMenuItemproperties.Visible = true;

            this.toolStripMenuItemviewMetadata.Enabled = false;
            this.toolStripMenuItemGetLegend.Enabled = false;
            this.toolStripMenuItemproperties.Enabled = false;

            this.toolStripMenuItemputOnTop.Enabled = false;
            this.toolStripMenuItemmoveUp.Enabled = false;
            this.toolStripMenuItemmoveDown.Enabled = false;
            this.toolStripMenuItemputAtBottom.Enabled = false;

            this.toolStripButtonGoTo.Enabled = false;
            this.toolStripButtonDelete.Enabled = false;
            this.toolStripButtonBottom.Enabled = false;
            this.toolStripButtonTop.Enabled = false;
            this.toolStripButtonUp.Enabled = false;
            this.toolStripButtonDown.Enabled = false;
            if (this.layerBuilder != null)
            {
               if (this.layerBuilder.Visible)
                  this.trackBarTransp.Enabled = true;
               this.trackBarTransp.Value = this.layerBuilder.Opacity;
               this.toolStripButtonGoTo.Enabled = true;
               this.toolStripButtonDelete.Enabled = true;

               if (!m_ActiveLayers.IsTop(this.layerBuilder))
               {
                  this.toolStripButtonTop.Enabled = true;
                  this.toolStripButtonUp.Enabled = true;
                  this.toolStripMenuItemputOnTop.Enabled = true;
                  this.toolStripMenuItemmoveUp.Enabled = true;
               }

               if (!m_ActiveLayers.IsBottom(this.layerBuilder))
               {
                  this.toolStripButtonBottom.Enabled = true;
                  this.toolStripButtonDown.Enabled = true;
                  this.toolStripMenuItemmoveDown.Enabled = true;
                  this.toolStripMenuItemputAtBottom.Enabled = true;
               }

               if (this.layerBuilder.Builder != null)
               {
                  this.toolStripMenuItemproperties.Enabled = true;
                  this.toolStripMenuItemviewMetadata.Enabled = this.layerBuilder.Builder.SupportsMetaData;
                  this.toolStripMenuItemGetLegend.Enabled = this.layerBuilder.Builder.SupportsLegend;
               }
            }
            else
            {
               // After the else to prevent flicker
               this.trackBarTransp.Enabled = false;
               this.trackBarTransp.Value = 128; 
            }
         }
      }
      IBuilder ServerBuilderItem
      {
         get
         {
            return this.serverBuilder;
         }
         set
         {
            this.serverBuilder = value;

            this.toolStripMenuItemAddLayer.Visible = true;
            this.toolStripMenuItemaddServer.Text = "Add Server";
            this.toolStripMenuItemaddServer.Visible = true;
            this.toolStripSeparatorServerAdd.Visible = true;
            this.toolStripMenuItemgoToServer.Visible = true;
            this.toolStripSeparatorServerGoto.Visible = true;
            this.toolStripMenuItemremoveServer.Visible = true;
            this.toolStripSeparatorServerRemove.Visible = true;
            this.toolStripMenuItemviewMetadataServer.Visible = false;
            this.toolStripMenuItemServerLegend.Visible = false;
            this.toolStripMenuItemviewMetadataServer.Enabled = false;
            this.toolStripMenuItemServerLegend.Enabled = false;
            this.toolStripMenuItempropertiesServer.Visible = true;
            this.toolStripSeparatorRefreshCatalog.Visible = false;
            this.toolStripMenuItemRefreshCatalog.Visible = false;

            if (this.serverBuilder != null)
            {
               if (this.serverBuilder is DAPCatalogBuilder)
               {
                  this.toolStripMenuItemaddServer.Text = "Add DAP Server";
               }
               //else if (this.serverBuilder is TileSetSet)
               //{
               //   this.toolStripMenuItemaddServer.Text = "Add Image Tile Server";
               //}
               else if (this.serverBuilder is WMSCatalogBuilder)
               {
                  this.toolStripMenuItemaddServer.Text = "Add WMS Server";
               }
               else
                  this.toolStripMenuItemaddServer.Visible = false;
               
               if (!(this.serverBuilder is LayerBuilder))
                  this.toolStripMenuItemAddLayer.Visible = false;

               if (!this.toolStripMenuItemaddServer.Visible && !this.toolStripMenuItemAddLayer.Visible)
                  this.toolStripSeparatorServerAdd.Visible = false;

               if (this.serverBuilder is DAPQuadLayerBuilder || this.serverBuilder is VEQuadLayerBuilder || this.serverBuilder is DAPQuadLayerBuilder ||
                  this.serverBuilder is WMSQuadLayerBuilder || (this.serverBuilder is BuilderDirectory && !(this.serverBuilder as BuilderDirectory).Removable))
               {
                  this.toolStripMenuItemremoveServer.Visible = false;
                  this.toolStripSeparatorServerRemove.Visible = false;
               }
               if (this.serverBuilder is DAPServerBuilder || this.serverBuilder is WMSServerBuilder ||
                   this.serverBuilder is BuilderDirectory)
               {
                  this.toolStripMenuItemgoToServer.Visible = false;
                  this.toolStripSeparatorServerGoto.Visible = false;
               }
               if (this.serverBuilder is DAPServerBuilder || this.serverBuilder is WMSServerBuilder)
               {
                  this.toolStripSeparatorRefreshCatalog.Visible = true;
                  this.toolStripMenuItemRefreshCatalog.Visible = true;
               }

               this.toolStripMenuItemviewMetadataServer.Visible = this.serverBuilder.SupportsMetaData || this.serverBuilder is LayerBuilder;
               this.toolStripMenuItemServerLegend.Visible = this.serverBuilder is LayerBuilder;
               this.toolStripMenuItemviewMetadataServer.Enabled = this.serverBuilder.SupportsMetaData;
               this.toolStripMenuItemServerLegend.Enabled = (this.serverBuilder is LayerBuilder) && (this.serverBuilder as LayerBuilder).SupportsLegend;
            }
         }
      }


      #endregion

      #region Constructor

      public MainForm(string strView, string strGeoTiff, string strLastView)
      {
         if (String.Compare(Path.GetExtension(strView), ViewExt, true) == 0 && File.Exists(strView))
            this.openView = strView;
         this.openGeoTiff = strGeoTiff;
         this.lastView = strLastView;

         m_executablePath = Path.GetDirectoryName(Application.ExecutablePath);
         InitializeComponent();
         this.Icon = new System.Drawing.Icon(@"app.ico");
         this.toolStripRenderer = new DappleToolStripRenderer();
         toolStripServers.Renderer = this.toolStripRenderer;
         toolStripLayerLabel.Renderer = this.toolStripRenderer;
         toolStripOverview.Renderer = this.toolStripRenderer;

         string strConfigDir = Path.Combine(UserPath, "Config");
         Directory.CreateDirectory(strConfigDir);
         Directory.CreateDirectory(Path.Combine(UserPath, "Cache"));

         // Copy/Update any configuration files if needed now
         string[] cfgFiles = Directory.GetFiles(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "Config"), "*.xml");
         foreach (string strCfgFile in cfgFiles)
         {
            string strUserCfg = Path.Combine(strConfigDir, Path.GetFileName(strCfgFile));
            if (!File.Exists(strUserCfg))
               File.Copy(strCfgFile, strUserCfg);
         }

         WWSettingsCtl.SettingsPath = UserPath;
         
         #region Plugin + World Init.

         // register handler for extension 

         Registry.SetValue(Registry.CurrentUser + "\\Software\\Classes\\" + ViewExt, "" , "Dapple View");
         Registry.SetValue(Registry.CurrentUser + "\\Software\\Classes\\Dapple View", "", "Dapple View");
         Registry.SetValue(Registry.CurrentUser + "\\Software\\Classes\\Dapple View\\Shell\\Open", "", "Open &" + ViewFileDescr);
         Registry.SetValue(Registry.CurrentUser + "\\Software\\Classes\\Dapple View\\Shell\\Open\\Command", "", "\"" + Application.ExecutablePath + "\" \"%1\"");
         Registry.SetValue(Registry.CurrentUser + "\\Software\\Classes\\Dapple View\\DefaultIcon", "", Path.Combine(m_executablePath, "app.ico"));

         m_worldWindow.WorldWindSettingsComponent = WWSettingsCtl;
         WorldSettings settings = new WorldSettings();
         settings = (WorldSettings)WorldSettings.LoadFromPath(settings, WWSettingsCtl.ConfigPath);
         World.Settings = settings;

         if (WWSettingsCtl.ConfigurationWizardAtStartup)
         {
            Wizard frm = new Wizard(WWSettingsCtl);
            frm.ShowDialog(this);
            WWSettingsCtl.ConfigurationWizardAtStartup = false;
         }

         if (WWSettingsCtl.NewCachePath.Length > 0)
         {
            Utility.FileSystem.DeleteFolderGUI(this, WWSettingsCtl.CachePath);
            WWSettingsCtl.CachePath = WWSettingsCtl.NewCachePath;
            WWSettingsCtl.NewCachePath = "";
         }

         WorldWind.Terrain.TerrainTileService terrainTileService = new WorldWind.Terrain.TerrainTileService("http://worldwind25.arc.nasa.gov/tile/tile.aspx", "100", 20, 150, "bil", 8, Path.Combine(WWSettingsCtl.CachePath, "Earth\\TerrainAccessor\\SRTM"));
         WorldWind.Terrain.TerrainAccessor terrainAccessor = new WorldWind.Terrain.NltTerrainAccessor("Earth", -180, -90, 180, 90, terrainTileService, null);

         WorldWind.World world = new WorldWind.World("Earth",
             new Vector3d(0, 0, 0), new Quaternion4d(0, 0, 0, 0),
             (float)6378137,
             System.IO.Path.Combine(m_worldWindow.Cache.CacheDirectory, "Earth"),
             terrainAccessor);

         this.m_worldWindow.CurrentWorld = world;

         NASA.Plugins.BmngLoader bmng = new NASA.Plugins.BmngLoader(WWSettingsCtl.WorldWindDirectory);
         Atmosphere.Plugin.Atmosphere atmo = new Atmosphere.Plugin.Atmosphere(WWSettingsCtl.WorldWindDirectory);
         Stars3D.Plugin.Stars3D stars = new Stars3D.Plugin.Stars3D(WWSettingsCtl.WorldWindDirectory);
         m_oCompass = new Murris.Plugins.Compass();

         this.m_worldWindow.AddPlugin(bmng, m_executablePath);
         this.m_worldWindow.AddPlugin(atmo, m_executablePath);
         this.m_worldWindow.AddPlugin(stars, m_executablePath);
         this.m_worldWindow.AddPlugin(m_oCompass, m_executablePath);

         m_BMNGForm = bmng.BMNGForm;

         splitContainerMain.Panel2.Controls.Add(m_worldWindow);
         m_worldWindow.Dock = DockStyle.Fill;

         #endregion

         float[] verticalExaggerationMultipliers = { 0.0f, 1.0f, 1.5f, 2.0f, 3.0f, 5.0f, 7.0f, 10.0f };
         foreach (float multiplier in verticalExaggerationMultipliers)
         {
            ToolStripMenuItem curItem = new ToolStripMenuItem(multiplier.ToString("f1", System.Threading.Thread.CurrentThread.CurrentCulture) + "x", null, new EventHandler(menuItemVerticalExaggerationChange));
            toolStripMenuItemverticalExagerration.DropDownItems.Add(curItem);
            curItem.CheckOnClick = true;
            if (Math.Abs(multiplier - World.Settings.VerticalExaggeration) < 0.1f)
               curItem.Checked = true;
         }

         toolStripMenuItemcompass.Checked = World.Settings.ShowCompass;
         toolStripMenuItemtileActivity.Checked = World.Settings.ShowDownloadRectangles;
         toolStripCrossHairs.Checked = World.Settings.ShowCrosshairs;
         toolStripMenuItemshowPosition.Checked = World.Settings.ShowPosition;
         toolStripMenuItemshowGridLines.Checked = World.Settings.ShowLatLonLines;

         #region OverviewPanel

         int i;
         for (i = 0; i < m_worldWindow.CurrentWorld.RenderableObjects.Count; i++)
         {
            if (((RenderableObject)m_worldWindow.CurrentWorld.RenderableObjects.ChildObjects[i]).Name == "4 - The Blue Marble")
               break;
         }

         m_overviewCtl = new OverviewControl(WWSettingsCtl.DataPath + @"\Earth\BmngBathy\world.topo.bathy.200407.jpg", this.m_worldWindow, panelOverview);
         m_overviewCtl.Dock = DockStyle.Fill;
         panelOverview.Controls.Add(m_overviewCtl);

         #endregion

         m_worldWindow.MouseEnter += new EventHandler(m_worldWindow_MouseEnter);
         m_worldWindow.MouseLeave += new EventHandler(m_worldWindow_MouseLeave);

         m_worldWindow.ClearDevice();

         TriStateTreeView layerTree = GetNewLayerTree();
         InitializeTrees(GetNewServerTree(), layerTree, new LayerBuilderList(this, layerTree, m_worldWindow));

#if !DEBUG
         Application.ThreadException += new ThreadExceptionEventHandler(OnThreadException);
#endif
      }

      #endregion

      #region Updates

      private void checkForUpdatesToolStripMenuItem_Click(object sender, EventArgs e)
      {
         CheckForUpdates(true);
      }

      bool m_bUpdateFromMenu;
      delegate void InvokeNotifyUpdate(string strVersion);
      void NotifyUpdate(string strVersion)
      {
         if (MessageBox.Show(this, "There is a new update for Dapple (Version " + strVersion + ") available.\nDo you want to visit the Dapple web site to downlad the latest version?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
            MainForm.BrowseTo(MainForm.WebsiteUrl);
      }

      delegate void InvokeNotifyUpdateNotAvailable();
      void NotifyUpdateNotAvailable()
      {
         MessageBox.Show(this, "There is no update for Dapple available at this time.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
      }

      private void UpdateDownloadComplete(WebDownload downloadInfo)
      {
         // First compare the file to the one we have
         bool bUpdate = false;
         string strVersion;
         string strTemp = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
         try
         {
            string[] tokens;
            int iHaveVer1, iHaveVer2, iHaveVer3;
            int iCurVer1, iCurVer2, iCurVer3;
            downloadInfo.Verify();
            downloadInfo.SaveMemoryDownloadToFile(strTemp);

            using (StreamReader sr = new StreamReader(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), VersionFile)))
            {
               strVersion = sr.ReadLine();
               tokens = strVersion.Split('.');
               iHaveVer1 = Convert.ToInt32(tokens[0]);
               iHaveVer2 = Convert.ToInt32(tokens[1]);
               iHaveVer3 = Convert.ToInt32(tokens[2]);
            }

            using (StreamReader sr = new StreamReader(strTemp))
            {
               strVersion = sr.ReadLine();
               tokens = strVersion.Split('.');
               iCurVer1 = Convert.ToInt32(tokens[0]);
               iCurVer2 = Convert.ToInt32(tokens[1]);
               iCurVer3 = Convert.ToInt32(tokens[2]);
            }

            if (iCurVer1 > iHaveVer1 || (iCurVer1 == iHaveVer1 && iCurVer2 > iHaveVer2) ||
               (iCurVer1 == iHaveVer1 && iCurVer2 == iHaveVer2 && iCurVer3 > iHaveVer3))
            {
               this.BeginInvoke(new InvokeNotifyUpdate(NotifyUpdate), new object[] { strVersion });
               bUpdate = true;
            }
               
            File.Delete(strTemp);
         }
         catch (System.Net.WebException caught)
         {
         }
         catch
         {
         }
         finally
         {
            if (!bUpdate && m_bUpdateFromMenu)
               this.BeginInvoke(new InvokeNotifyUpdateNotAvailable(NotifyUpdateNotAvailable));
         }
      }

      void CheckForUpdates(bool bFromMenu)
      {
         m_bUpdateFromMenu = bFromMenu;
         WebDownload download = new WebDownload(WebsiteUrl + VersionFile);
         download.DownloadType = DownloadType.Unspecified;
         download.CompleteCallback += new DownloadCompleteHandler(UpdateDownloadComplete);
         download.BackgroundDownloadMemory();
      }
      #endregion

      #region TreeView Initialization

      private void ReplaceServerTree(TreeView serverTree)
      {
         this.SuspendLayout();
         this.panelServer.SuspendLayout();
         this.panelServer.Controls.Clear();
         this.treeViewServers = serverTree;
         this.panelServer.Controls.Add(this.treeViewServers);
         this.panelServer.ResumeLayout(false);
         this.ResumeLayout(false);
         this.panelServer.PerformLayout();
         this.PerformLayout();

         // Expand the first level
         foreach (TreeNode treeNode in this.treeViewServers.Nodes)
            treeNode.Expand();
      }

      private void InitializeTrees(TreeView serverTree, TriStateTreeView layerTree, LayerBuilderList activeList)
      {
         this.treeViewServerBackup = null;
         this.SuspendLayout();
         if (m_ActiveLayers != null && m_ActiveLayers != activeList)
         {
            m_ActiveLayers.RemoveAll();
         }
         m_ActiveLayers = activeList;
         this.panelServer.SuspendLayout();
         this.panelLayers.SuspendLayout();
         this.panelServer.Controls.Clear();
         this.panelLayers.Controls.Clear();
         this.treeViewServers = serverTree;
         this.triStateTreeViewLayers = layerTree;

         this.panelServerTreeTemp.Controls.Remove(this.treeViewServers);
         this.panelLayerTreeTemp.Controls.Remove(this.triStateTreeViewLayers);
         this.panelServer.Controls.Add(this.treeViewServers);
         this.panelLayers.Controls.Add(this.triStateTreeViewLayers);

         this.panelServer.ResumeLayout(false);
         this.panelLayers.ResumeLayout(false);
         this.ResumeLayout(false);
         this.panelServer.PerformLayout();
         this.panelLayers.PerformLayout();
         this.PerformLayout();

         // Expand the first level
         foreach (TreeNode treeNode in this.treeViewServers.Nodes)
            treeNode.Expand();

         this.treeViewServerBackup = null;
         this.toolStripButtonClearFilter.Enabled = false;
         m_strLastSearchText = "";
         this.toolStripFilterText.ForeColor = SystemColors.GrayText;
      }

      TreeView GetNewServerTree()
      {
         TreeView tree = new TreeView();

         tree.ContextMenuStrip = this.contextMenuStripServers;
         tree.Dock = System.Windows.Forms.DockStyle.Fill;
         tree.ImageIndex = 0;
         tree.ImageList = this.iconImageList;
         tree.Location = new System.Drawing.Point(0, 0);
         tree.Name = "treeViewServers";
         tree.SelectedImageIndex = 0;
         tree.ShowLines = false;
         tree.ShowNodeToolTips = true;
         tree.Sorted = true;
         tree.TreeViewNodeSorter = new TreeNodeSorter();
         tree.Size = new System.Drawing.Size(245, 240);
         tree.TabIndex = 0;
         tree.Scrollable = true;
         tree.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.treeViewServers_MouseDoubleClick);
         tree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewServers_AfterSelect);
         tree.MouseDown += new System.Windows.Forms.MouseEventHandler(this.treeViewServers_MouseDown);


         // Add temporarily to invisible panel to be sure that the control is created/will reparent later
         this.panelServerTreeTemp.Controls.Add(tree);
         tree.CreateControl();

         return tree;
      }

      TriStateTreeView GetNewLayerTree()
      {
         TriStateTreeView tree = new TriStateTreeView();

         tree.ContextMenuStrip = this.contextMenuStripLayers;
         tree.Dock = System.Windows.Forms.DockStyle.Fill;
         tree.HideSelection = false;
         tree.ImageIndex = 0;
         tree.ImageList = this.iconImageList;
         tree.Location = new System.Drawing.Point(0, 0);
         tree.Name = "triStateTreeViewLayers";
         tree.SelectedImageIndex = 0;
         tree.ShowLines = false;
         tree.ShowNodeToolTips = true;
         tree.ShowRootLines = false;
         tree.Size = new System.Drawing.Size(245, 182);
         tree.TabIndex = 1;
         tree.Scrollable = true;
         tree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.triStateTreeViewLayers_AfterSelect);
         tree.KeyUp += new System.Windows.Forms.KeyEventHandler(this.triStateTreeViewLayers_KeyUp);
         tree.TreeNodeChecked += new Geosoft.DotNetTools.TreeNodeCheckedEventHandler(this.triStateTreeViewLayers_TreeNodeChecked);
         tree.MouseDown += new System.Windows.Forms.MouseEventHandler(this.triStateTreeViewLayers_MouseDown);

         // Add temporarily to invisible panel to be sure that the control is created/will reparent later
         this.panelLayerTreeTemp.Controls.Add(tree);
         tree.CreateControl();

         return tree;
      }



      #endregion

      #region World Window Events

      void m_worldWindow_MouseLeave(object sender, EventArgs e)
      {
         splitContainerMain.Panel1.Select();
      }

      void m_worldWindow_MouseEnter(object sender, EventArgs e)
      {
         m_worldWindow.Select();
      }

      #endregion

      #region Context menus and buttons

      #region Opening 

      private void contextMenuStripLayers_Opening(object sender, CancelEventArgs e)
      {
         if (LayerBuilderItem == null || this.checked_context)
         {
            this.checked_context = false;
            e.Cancel = true;
         }
      }

      private void contextMenuStripServers_Opening(object sender, CancelEventArgs e)
      {
         if (ServerBuilderItem == null || this.checked_context)
            e.Cancel = true;
      }

      #endregion

      #region Nav Strip Buttons

      enum NavMode
      {
         None,
         ZoomIn,
         ZoomOut,
         RotateLeft,
         RotateRight,
         TiltUp,
         TiltDown
      }

      private NavMode eNavMode = NavMode.None;
      private bool bNavTimer = false;

      private void toolStripButtonRestoreTilt_Click(object sender, EventArgs e)
      {
         m_worldWindow.DrawArgs.WorldCamera.SlerpPercentage = World.Settings.CameraSlerpInertia;
         m_worldWindow.DrawArgs.WorldCamera.SetPosition(
                  m_worldWindow.Latitude,
                  m_worldWindow.Longitude,
                   m_worldWindow.DrawArgs.WorldCamera.Heading.Degrees,
                   m_worldWindow.DrawArgs.WorldCamera.Altitude,
                   0);

      }

      private void toolStripButtonRestoreNorth_Click(object sender, EventArgs e)
      {
         m_worldWindow.DrawArgs.WorldCamera.SlerpPercentage = World.Settings.CameraSlerpInertia;
         m_worldWindow.DrawArgs.WorldCamera.SetPosition(
                  m_worldWindow.Latitude,
                  m_worldWindow.Longitude,
                   0,
                   m_worldWindow.DrawArgs.WorldCamera.Altitude,
                   m_worldWindow.DrawArgs.WorldCamera.Tilt.Degrees);
      }

      private void toolStripButtonResetCamera_Click(object sender, EventArgs e)
      {
         m_worldWindow.DrawArgs.WorldCamera.SlerpPercentage = World.Settings.CameraSlerpInertia;
         m_worldWindow.DrawArgs.WorldCamera.Reset();
      }


      private void timerNavigation_Tick(object sender, EventArgs e)
      {
         this.bNavTimer = true;
         m_worldWindow.DrawArgs.WorldCamera.SlerpPercentage = 1.0;
         switch (this.eNavMode)
         {
            case NavMode.ZoomIn:
               m_worldWindow.DrawArgs.WorldCamera.Zoom(0.2f);
               return;
            case NavMode.ZoomOut:
               m_worldWindow.DrawArgs.WorldCamera.Zoom(-0.2f);
               return;
            case NavMode.RotateLeft:
               Angle rotateClockwise = Angle.FromRadians(-0.01f);
               m_worldWindow.DrawArgs.WorldCamera.Heading += rotateClockwise;
               m_worldWindow.DrawArgs.WorldCamera.RotationYawPitchRoll(Angle.Zero, Angle.Zero, rotateClockwise);
               return;
            case NavMode.RotateRight:
               Angle rotateCounterclockwise = Angle.FromRadians(0.01f);
               m_worldWindow.DrawArgs.WorldCamera.Heading += rotateCounterclockwise;
               m_worldWindow.DrawArgs.WorldCamera.RotationYawPitchRoll(Angle.Zero, Angle.Zero, rotateCounterclockwise);
               return;
            case NavMode.TiltUp:
               m_worldWindow.DrawArgs.WorldCamera.Tilt += Angle.FromDegrees(-1.0f);
               return;
            case NavMode.TiltDown:
               m_worldWindow.DrawArgs.WorldCamera.Tilt += Angle.FromDegrees(1.0f);
               return;
            default:
               return;
         }
      }
   
      private void toolStripNavButton_MouseRemoveCapture(object sender, EventArgs e)
      {
         this.timerNavigation.Enabled = true;   
         this.timerNavigation.Enabled = false;
         this.eNavMode = NavMode.None;
      }

      private void toolStripButtonRotLeft_MouseDown(object sender, MouseEventArgs e)
      {
         this.timerNavigation.Enabled = true;
         this.bNavTimer = false;
         this.eNavMode = NavMode.RotateLeft;
      }

      private void toolStripButtonRotRight_MouseDown(object sender, MouseEventArgs e)
      {
         this.timerNavigation.Enabled = true;
         this.bNavTimer = false;
         this.eNavMode = NavMode.RotateRight;
      }

      private void toolStripButtonTiltDown_MouseDown(object sender, MouseEventArgs e)
      {
         this.timerNavigation.Enabled = true;   
         this.bNavTimer = false;
         this.eNavMode = NavMode.TiltDown;
      }

      private void toolStripButtonTiltUp_MouseDown(object sender, MouseEventArgs e)
      {
         this.timerNavigation.Enabled = true;   
         this.bNavTimer = false;
         this.eNavMode = NavMode.TiltUp;
      }

      private void toolStripButtonZoomOut_MouseDown(object sender, MouseEventArgs e)
      {
         this.timerNavigation.Enabled = true;   
         this.bNavTimer = false;
         this.eNavMode = NavMode.ZoomOut;
      }

      private void toolStripButtonZoomIn_MouseDown(object sender, MouseEventArgs e)
      {
         this.timerNavigation.Enabled = true;   
         this.bNavTimer = false;
         this.eNavMode = NavMode.ZoomIn;
      }

      private void toolStripButtonZoomIn_Click(object sender, EventArgs e)
      {
         this.timerNavigation.Enabled = false;
         if (!this.bNavTimer)
         {
            m_worldWindow.DrawArgs.WorldCamera.SlerpPercentage = World.Settings.CameraSlerpInertia;
            m_worldWindow.DrawArgs.WorldCamera.Zoom(2.0f);
         }
         else
            this.bNavTimer = false;
      }

      private void toolStripButtonZoomOut_Click(object sender, EventArgs e)
      {
         this.timerNavigation.Enabled = false;
         if (!this.bNavTimer)
         {
            m_worldWindow.DrawArgs.WorldCamera.SlerpPercentage = World.Settings.CameraSlerpInertia;
            m_worldWindow.DrawArgs.WorldCamera.Zoom(-2.0f);
         }
         else
            this.bNavTimer = false;
      }

      private void toolStripButtonRotLeft_Click(object sender, EventArgs e)
      {
         this.timerNavigation.Enabled = false;
         if (!this.bNavTimer)
         {
            Angle rotateClockwise = Angle.FromRadians(-0.2f);
            m_worldWindow.DrawArgs.WorldCamera.SlerpPercentage = World.Settings.CameraSlerpInertia;
            m_worldWindow.DrawArgs.WorldCamera.Heading += rotateClockwise;
            m_worldWindow.DrawArgs.WorldCamera.RotationYawPitchRoll(Angle.Zero, Angle.Zero, rotateClockwise);
         }
         else
            this.bNavTimer = false;
      }

      private void toolStripButtonRotRight_Click(object sender, EventArgs e)
      {
         this.timerNavigation.Enabled = false;
         if (!this.bNavTimer)
         {
            Angle rotateCounterclockwise = Angle.FromRadians(0.2f);
            m_worldWindow.DrawArgs.WorldCamera.SlerpPercentage = World.Settings.CameraSlerpInertia;
            m_worldWindow.DrawArgs.WorldCamera.Heading += rotateCounterclockwise;
            m_worldWindow.DrawArgs.WorldCamera.RotationYawPitchRoll(Angle.Zero, Angle.Zero, rotateCounterclockwise);
         }
         else
            this.bNavTimer = false;
      }

      private void toolStripButtonTiltUp_Click(object sender, EventArgs e)
      {
         this.timerNavigation.Enabled = false;
         if (!this.bNavTimer)
         {
            m_worldWindow.DrawArgs.WorldCamera.SlerpPercentage = World.Settings.CameraSlerpInertia;
            m_worldWindow.DrawArgs.WorldCamera.Tilt += Angle.FromDegrees(-10.0f);
         }
         else
            this.bNavTimer = false;
      }

      private void toolStripButtonTiltDown_Click(object sender, EventArgs e)
      {
         this.timerNavigation.Enabled = false;
         if (!this.bNavTimer)
         {
            m_worldWindow.DrawArgs.WorldCamera.SlerpPercentage = World.Settings.CameraSlerpInertia;
            m_worldWindow.DrawArgs.WorldCamera.Tilt += Angle.FromDegrees(10.0f);
         }
         else
            this.bNavTimer = false;
      }

      #endregion

      #region Add Layers

      private void toolStripMenuItemOpen_Click(object sender, EventArgs e)
      {
         string strLastFolderCfg = Path.Combine(m_worldWindow.WorldWindSettings.ConfigPath, "opengeotif.cfg");

         this.openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
         this.openFileDialog.RestoreDirectory = true;
         if (File.Exists(strLastFolderCfg))
         {
            try
            {
               using (StreamReader sr = new StreamReader(strLastFolderCfg))
               {
                  string strDir = sr.ReadLine();
                  if (Directory.Exists(strDir))
                     this.openFileDialog.InitialDirectory = strDir;
               }
            }
            catch
            {
            }
         }

         if (this.openFileDialog.ShowDialog() == DialogResult.OK)
         {
            AddGeoTiff(this.openFileDialog.FileName, true);
            try
            {
               using (StreamWriter sw = new StreamWriter(strLastFolderCfg))
               {
                  sw.WriteLine(Path.GetDirectoryName(this.openFileDialog.FileName));
               }
            }
            catch
            {
            }
         }
      }

      void AddGeoTiff(string strGeoTiff, bool bGoto)
      {
         LayerBuilder builder = new GeorefImageLayerBuilder(WWSettingsCtl.CachePath, strGeoTiff, m_worldWindow.CurrentWorld, null);

         Cursor = Cursors.WaitCursor;
         if (builder.GetLayer() != null)
         {
            Cursor = Cursors.Default;
            m_ActiveLayers.Add(builder.Name, builder, true, 255, true);
            if (bGoto)
               GoTo(builder as ImageBuilder);
         }
         else
         {
            Cursor = Cursors.Default;
            string strMessage = "Error adding the file: '" + strGeoTiff + "'.\nOnly WGS 84 geographic images can be displayed.";
            string strGeoInfo = GeorefImageLayerBuilder.GetGeorefInfoFromGeotif(strGeoTiff);
            if (strGeoInfo.Length > 0)
               strMessage += "\nThis image is:\n\n" + strGeoInfo;
            MessageBox.Show(this, strMessage, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
         }
      }

      private void toolStripMenuItemAddLayer_Click(object sender, EventArgs e)
      {
         AddCurrentToActiveLayers();
      }

      private void treeViewServers_MouseDoubleClick(object sender, MouseEventArgs e)
      {
         AddCurrentToActiveLayers();
      }

      void AddCurrentToActiveLayers()
      {
         if (!(ServerBuilderItem is LayerBuilder))
            return;

         m_ActiveLayers.Add(ServerBuilderItem.Name, ServerBuilderItem as LayerBuilder, true, ServerBuilderItem.Opacity, true);

         if (triStateTreeViewLayers.SelectedNode != null)
            LayerBuilderItem = triStateTreeViewLayers.SelectedNode.Tag as LayerBuilderContainer;
         else
            LayerBuilderItem = null;

         SaveLastView();
      }
      #endregion

      #region Add Servers

      private void toolStripMenuItemAddDAP_Click(object sender, EventArgs e)
      {
         TreeNode treeNode = TreeUtils.FindNodeOfTypeBFS(typeof(DAPCatalogBuilder), this.treeViewServers.Nodes);
         if (treeNode != null)
            AddServer(treeNode.Tag as BuilderDirectory, treeNode.Nodes);
      }

      private void toolStripMenuItemAddWMS_Click(object sender, EventArgs e)
      {
         TreeNode treeNode = TreeUtils.FindNodeOfTypeBFS(typeof(WMSCatalogBuilder), this.treeViewServers.Nodes);
         if (treeNode != null)
            AddServer(treeNode.Tag as BuilderDirectory, treeNode.Nodes);
      }

      void toolStripMenuItemaddServer_Click(object sender, EventArgs e)
      {
         if (ServerBuilderItem == null)
            return;

         if (treeViewServerBackup != null)
         {
            MessageBox.Show(this, "It is not possible to add servers to a filtered view.\nClear the results by using the \"Show all layers\" button first.", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
         }

         if (ServerBuilderItem is BuilderDirectory)
         {
            TreeNode treeNode = TreeUtils.FindNodeBFS(ServerBuilderItem, this.treeViewServers.Nodes);
            AddServer(ServerBuilderItem as BuilderDirectory, treeNode.Nodes);
         }
      }

      void AddServer(BuilderDirectory serverDir, TreeNodeCollection nodes)
      {
         /*DialogResult res = DialogResult.Cancel;

         }
         else if (serverDir is WMSCatalogBuilder)
            serverType = LayerAddWizard.ServerType.WmsServer;
         else if (serverDir is TileSetSet)
            serverType = LayerAddWizard.ServerType.TileServer;
         else
            return;
         */
         if (serverDir is DAPCatalogBuilder)
         {
            AddDAP dlg = new AddDAP(m_worldWindow, serverDir as DAPCatalogBuilder);
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {         
               lock (lockCatalogUpdates)
               {
                  DAPCatalogBuilder dapBuilder = serverDir as DAPCatalogBuilder;
                  BuilderDirectory dir = dapBuilder.AddDapServer(dlg.DapServer, new Geosoft.Dap.Common.BoundingBox(180, 90, -180, -90), serverDir);
                  serverDir.SubList.Add(dir);
                  TreeNode treeNode = nodes.Add(dir.Name);
                  treeNode.SelectedImageIndex = treeNode.ImageIndex = ImageListIndex("time");
                  treeNode.Tag = dir;
                  RefreshTreeHelper(treeViewServers, treeNode, dir);
               }
               SaveLastView();
            }
         }
         else if (serverDir is WMSCatalogBuilder)
         {
            AddWMS dlg = new AddWMS(m_worldWindow, serverDir as WMSCatalogBuilder);
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {         
               lock (lockCatalogUpdates)
               {
                  WMSCatalogBuilder wmsBuilder = serverDir as WMSCatalogBuilder;
                  BuilderDirectory dir = wmsBuilder.AddServer(dlg.WmsURL, wmsBuilder);
                  serverDir.SubList.Add(dir);
                  TreeNode treeNode = nodes.Add(dir.Name);
                  treeNode.SelectedImageIndex = treeNode.ImageIndex = ImageListIndex("time");
                  treeNode.Tag = dir;
                  RefreshTreeHelper(treeViewServers, treeNode, dir);
               }
               SaveLastView();
            }
         }/*
            else if (serverType == LayerAddWizard.ServerType.TileServer && wiz.TileServer != null)
            {
               TreeNode treeNode = nodes.Add(wiz.TileServer.Name);
               treeNode.SelectedImageIndex = treeNode.ImageIndex = ImageListIndex("tile");
               treeNode.Tag = wiz.TileServer;
               serverDir.LayerBuilders.Add(wiz.TileServer);
            }
         }*/
         
      }
      #endregion

      #region Remove Items

      private void toolStripMenuItemremoveServer_Click(object sender, EventArgs e)
      {
         if (ServerBuilderItem == null)
            return;

         if (treeViewServerBackup != null)
         {
            MessageBox.Show(this, "It is not possible to remove servers from a filtered view.\nClear the results by using the \"Show all layers\" button first.", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
         }

         TreeNode treeNode = TreeUtils.FindNodeBFS(ServerBuilderItem, treeViewServers.Nodes);
         if (treeNode != null && treeNode.Parent != null)
         {
            // Check the item isn'treeNode part of catalog
            TreeNode parent = treeNode.Parent;
            while (parent != null)
            {
               if (parent.Tag is ServerBuilder)
                  return;
               parent = parent.Parent;
            }

            if (treeNode.Tag is DAPQuadLayerBuilder || treeNode.Tag is WMSQuadLayerBuilder)
               return;

            if (treeNode.Tag is LayerBuilder)
            {
               (treeNode.Parent.Tag as BuilderDirectory).LayerBuilders.Remove(treeNode.Tag as LayerBuilder);
            }
            else
            {
               if (treeNode.Tag is DAPServerBuilder)
               {
                  DAPServerBuilder serverBuilder = treeNode.Tag as DAPServerBuilder;
                  // Need to have a catalog builder in treenode's parents
                  IBuilder parentCatalog = ServerBuilderItem as IBuilder;

                  while (parentCatalog != null && !(parentCatalog is DAPCatalogBuilder))
                     parentCatalog = parentCatalog.Parent;

                  if (parentCatalog != null && parentCatalog is DAPCatalogBuilder)
                     (parentCatalog as DAPCatalogBuilder).RemoveServer(serverBuilder.URL);
               }
               else if (treeNode.Tag is WMSServerBuilder)
               {
                  WMSServerBuilder serverBuilder = treeNode.Tag as WMSServerBuilder;
                  // Need to have a catalog builder in treenode's parents
                  IBuilder parentCatalog = ServerBuilderItem as IBuilder;

                  while (parentCatalog != null && !(parentCatalog is WMSCatalogBuilder))
                     parentCatalog = parentCatalog.Parent;

                  if (parentCatalog != null && parentCatalog is WMSCatalogBuilder)
                     (parentCatalog as WMSCatalogBuilder).RemoveServer(serverBuilder.URL);
               }

               (treeNode.Parent.Tag as BuilderDirectory).SubList.Remove(treeNode.Tag as BuilderDirectory);
            }
         
            treeNode.Parent.Nodes.Remove(treeNode);
            SaveLastView();
         }
      }

      private void toolStripMenuItemremoveAllButThis_Click(object sender, EventArgs e)
      {
         if (LayerBuilderItem == null)
            return;

         m_ActiveLayers.RemoveOthers(LayerBuilderItem);

         if (triStateTreeViewLayers.SelectedNode != null)
            LayerBuilderItem = triStateTreeViewLayers.SelectedNode.Tag as LayerBuilderContainer;
         else
            LayerBuilderItem = null;
      }

      private void toolStripMenuButtonItemremove_Click(object sender, EventArgs e)
      {
         if (LayerBuilderItem == null)
            return;

         LayerBuilderContainer current = LayerBuilderItem;
         LayerBuilderItem = null;
         m_ActiveLayers.RemoveContainer(current);

         if (triStateTreeViewLayers.SelectedNode != null)
            LayerBuilderItem = triStateTreeViewLayers.SelectedNode.Tag as LayerBuilderContainer;
         else
            LayerBuilderItem = null;
      }

      private void toolStripMenuItemRemoveAll_Click(object sender, EventArgs e)
      {
         m_ActiveLayers.RemoveAll();
         LayerBuilderItem = null;
      }


      #endregion

      #region Go To

      private void toolStripMenuButtonItemGoTo_Click(object sender, EventArgs e)
      {
         if (LayerBuilderItem.Builder is ImageBuilder)
            GoTo(LayerBuilderItem.Builder as ImageBuilder);
      }
      private void toolStripMenuItemgoToServer_Click(object sender, EventArgs e)
      {
         if (ServerBuilderItem is ImageBuilder)
            GoTo(ServerBuilderItem as ImageBuilder);
      }

      void GoTo(ImageBuilder builder)
      {
         double latitude, longitude;
         long overviewCameraAlt = 12000000;

         if (builder.Extents.North > 89 &&
            builder.Extents.South < -89 &&
            builder.Extents.West < -179 &&
            builder.Extents.East > 179)
         {
            latitude = m_worldWindow.Latitude;
            longitude = m_worldWindow.Longitude;
         }
         else
         {
            latitude = builder.Extents.South + (builder.Extents.North - builder.Extents.South) / 2.0;
            longitude = builder.Extents.West + (builder.Extents.East - builder.Extents.West) / 2.0;
         }


         double fov = 2.0 * Math.Max(builder.Extents.North - builder.Extents.South, builder.Extents.East - builder.Extents.West);

         if (builder is QuadLayerBuilder)
            fov = Math.Min(fov, (double) (2*(builder as QuadLayerBuilder).LevelZeroTileSize));
         if (fov < 180.0)
            m_worldWindow.GotoLatLonHeadingViewRange(latitude, longitude, 0, fov);
         else
         {
            m_worldWindow.GotoLatLonAltitude(latitude, longitude, overviewCameraAlt);
         }
      }

      #endregion

      #region Render Order

      private void toolStripMenuItemButtonAtBottom_Click(object sender, EventArgs e)
      {
         if (LayerBuilderItem != null && !m_ActiveLayers.IsBottom(LayerBuilderItem))
         {
            m_ActiveLayers.MoveBottom(LayerBuilderItem);
            if (triStateTreeViewLayers.SelectedNode != null)
               LayerBuilderItem = triStateTreeViewLayers.SelectedNode.Tag as LayerBuilderContainer;
            else
               LayerBuilderItem = null;
         }
      }

      private void toolStripMenuItemButtonMoveDown_Click(object sender, EventArgs e)
      {
         if (LayerBuilderItem != null && !m_ActiveLayers.IsBottom(LayerBuilderItem))
         {
            m_ActiveLayers.MoveDown(LayerBuilderItem);
            if (triStateTreeViewLayers.SelectedNode != null)
               LayerBuilderItem = triStateTreeViewLayers.SelectedNode.Tag as LayerBuilderContainer;
            else
               LayerBuilderItem = null;
         }
      }

      private void toolStripMenuItemButtonMoveUp_Click(object sender, EventArgs e)
      {
         if (LayerBuilderItem != null && !m_ActiveLayers.IsTop(LayerBuilderItem))
         {
            m_ActiveLayers.MoveUp(LayerBuilderItem);
            if (triStateTreeViewLayers.SelectedNode != null)
               LayerBuilderItem = triStateTreeViewLayers.SelectedNode.Tag as LayerBuilderContainer;
            else
               LayerBuilderItem = null;
         }
      }

      private void toolStripMenuItemButtonOnTop_Click(object sender, EventArgs e)
      {
         if (LayerBuilderItem != null && !m_ActiveLayers.IsTop(LayerBuilderItem))
         {
            m_ActiveLayers.MoveTop(LayerBuilderItem);
            if (triStateTreeViewLayers.SelectedNode != null)
               LayerBuilderItem = triStateTreeViewLayers.SelectedNode.Tag as LayerBuilderContainer;
            else
               LayerBuilderItem = null;
         }
      }

      #endregion

      #region Refresh
      private void toolStripMenuItemRefresh_Click(object sender, EventArgs e)
      {
         if (LayerBuilderItem != null && LayerBuilderItem.Builder != null)
            m_ActiveLayers.RefreshBuilder(LayerBuilderItem.Builder);
      }

      private void toolStripMenuItemClearRefresh_Click(object sender, EventArgs e)
      {
         if (LayerBuilderItem != null && LayerBuilderItem.Builder != null)
         {
            Utility.FileSystem.DeleteFolderGUI(this, LayerBuilderItem.Builder.GetCachePath());
            m_ActiveLayers.RefreshBuilder(LayerBuilderItem.Builder);
         }
      }

      private void toolStripMenuItemRefreshCatalog_Click(object sender, EventArgs e)
      {
         if (ServerBuilderItem == null)
            return;

         if (treeViewServerBackup != null)
         {
            MessageBox.Show(this, "It is not possible to refresh catalogs in a filtered view.\nClear the results by using the \"Show all layers\" button first.", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
         }

         TreeNode serverNode  = TreeUtils.FindNodeBFS(ServerBuilderItem, treeViewServers.Nodes);
         if (serverNode != null)
         {
            if (serverNode.Tag is DAPServerBuilder)
            {
               DAPServerBuilder serverBuilder = serverNode.Tag as DAPServerBuilder;
               // Need to have a catalog builder in treenode's parents
               IBuilder parentCatalog = ServerBuilderItem as IBuilder;

               while (parentCatalog != null && !(parentCatalog is DAPCatalogBuilder))
                  parentCatalog = parentCatalog.Parent;

               if (parentCatalog != null)
               {
                  DAPCatalogBuilder dapBuilder = parentCatalog as DAPCatalogBuilder;
                  dapBuilder.RemoveServer(serverBuilder.Server.Url);

                  lock (lockCatalogUpdates)
                  {
                     Geosoft.GX.DAPGetData.Server dapServer = new Geosoft.GX.DAPGetData.Server(serverBuilder.Server.Url, WWSettingsCtl.CachePath);
                     BuilderDirectory dapDir = dapBuilder.AddDapServer(dapServer, new Geosoft.Dap.Common.BoundingBox(180, 90, -180, -90), serverBuilder.Parent);
                     dapBuilder.SubList.Add(dapDir);

                     serverNode.Nodes.Clear();
                     serverNode.Text = "Loading: " + serverBuilder.Server.Url;
                     serverNode.SelectedImageIndex = serverNode.ImageIndex = ImageListIndex("time");
                     serverNode.Tag = dapDir;
                  }
               }
            }
            else if (serverNode.Tag is WMSServerBuilder)
            {
               WMSServerBuilder serverBuilder = serverNode.Tag as WMSServerBuilder;
               // Need to have a catalog builder in treenode's parents
               IBuilder parentCatalog = ServerBuilderItem as IBuilder;

               while (parentCatalog != null && !(parentCatalog is WMSCatalogBuilder))
                  parentCatalog = parentCatalog.Parent;

               if (parentCatalog != null)
               {
                  WMSCatalogBuilder wmsBuilder = parentCatalog as WMSCatalogBuilder;
                  wmsBuilder.RemoveServer(serverBuilder.URL);

                  lock (lockCatalogUpdates)
                  {
                     BuilderDirectory wmsDir = wmsBuilder.AddServer(serverBuilder.URL, serverBuilder.Parent as BuilderDirectory);
                     wmsBuilder.SubList.Add(wmsDir);

                     serverNode.Nodes.Clear();
                     serverNode.Text = "Loading: " + serverBuilder.URL;
                     serverNode.SelectedImageIndex = serverNode.ImageIndex = ImageListIndex("time");
                     serverNode.Tag = wmsDir;
                  }
               }
            }
         }
      }

      #endregion

      #region Metadata, legend and properties

      private void toolStripMenuItemGetLegend_Click(object sender, EventArgs e)
      {
         if (LayerBuilderItem != null && LayerBuilderItem.Builder != null)
         {
            if (LayerBuilderItem.Builder.SupportsLegend)
            {
               string[] strLegendArr = LayerBuilderItem.Builder.GetLegendURLs();
               foreach (string strLegend in strLegendArr)
               {
                  if (!String.IsNullOrEmpty(strLegend))
                     MainForm.BrowseTo(strLegend);
               }
            }
         }
      }

      private void toolStripMenuItemServerLegend_Click(object sender, EventArgs e)
      {
         if (ServerBuilderItem != null && ServerBuilderItem is LayerBuilder)
         {
            if ((ServerBuilderItem as LayerBuilder).SupportsLegend)
            {
               string[] strLegendArr = (ServerBuilderItem as LayerBuilder).GetLegendURLs();
               foreach (string strLegend in strLegendArr)
               {
                  if (!String.IsNullOrEmpty(strLegend))
                     MainForm.BrowseTo(strLegend);
               }
            }
         }
      }

      private void toolStripMenuItemviewMetadata_Click(object sender, EventArgs e)
      {
         if (LayerBuilderItem != null && LayerBuilderItem.Builder != null)
            ViewMetadata(LayerBuilderItem.Builder);
      }

      private void toolStripMenuItemviewMetadataServer_Click(object sender, EventArgs e)
      {
         if (ServerBuilderItem != null)
            ViewMetadata(ServerBuilderItem);
      }

      void ViewMetadata(IBuilder builder)
      {
         MetaDataForm form = new MetaDataForm();
         XmlDocument oDoc = new XmlDocument();
         oDoc.AppendChild(oDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes"));
         XmlNode oNode = builder.GetMetaData(oDoc);

         if (oNode == null)
            return;

         if (oNode is XmlDocument)
         {
            oDoc = oNode as XmlDocument;
         }
         else
         {
            oDoc.AppendChild(oNode);
         }
         if (builder.StyleSheetPath != null)
         {
            XmlNode oRef = oDoc.CreateProcessingInstruction("xml-stylesheet", "type='text/xsl' href='" + builder.StyleSheetPath + "'");
            oDoc.InsertBefore(oRef, oDoc.DocumentElement);
         }

         string filePath = Path.Combine(Path.GetDirectoryName(builder.StyleSheetPath), Path.GetRandomFileName());
         oDoc.Save(filePath);
         form.ShowDialog(this, filePath);
         System.IO.File.Delete(filePath);
      }

      private void toolStripMenuItemproperties_Click(object sender, EventArgs e)
      {
         if (LayerBuilderItem != null && LayerBuilderItem.Builder != null)
            ViewProperties(LayerBuilderItem.Builder);
      }

      private void toolStripMenuItempropertiesServer_Click(object sender, EventArgs e)
      {
         if (ServerBuilderItem != null)
            ViewProperties(ServerBuilderItem);
      }

      void ViewProperties(IBuilder builder)
      {
         frmProperties form = new frmProperties();
         form.SetObject = builder;
         form.ShowDialog(this);
         if (builder.IsChanged && builder is LayerBuilder && (builder as LayerBuilder).IsAdded)
         {
            m_ActiveLayers.RefreshBuilder(builder as LayerBuilder);
            if (triStateTreeViewLayers.SelectedNode != null)
               LayerBuilderItem = triStateTreeViewLayers.SelectedNode.Tag as LayerBuilderContainer;
            else
               LayerBuilderItem = null;
         }
      }

      #endregion

      #endregion

      #region Main Menu Item Click events

      private void toolStripMenuItemadvancedSettings_Click(object sender, EventArgs e)
      {
         Wizard wiz = new Wizard(WWSettingsCtl);
         wiz.ShowDialog(this);
      }

      /// <summary>
      /// Handler for a change in the selection of the vertical exaggeration from the main menu
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void menuItemVerticalExaggerationChange(object sender, EventArgs e)
      {
         foreach (ToolStripMenuItem item in toolStripMenuItemverticalExagerration.DropDownItems)
         {
            if (item != sender)
            {
               item.Checked = false;
            }
            else
            {
               World.Settings.VerticalExaggeration = Convert.ToSingle(item.Text.Replace("x", string.Empty));
            }
         }
         m_worldWindow.Invalidate();
      }

      private void toolStripMenuItemcompass_Click(object sender, EventArgs e)
      {
         World.Settings.ShowCompass = toolStripMenuItemcompass.Checked;
         m_oCompass.Layer.IsOn = World.Settings.ShowCompass;
      }

      private void toolStripMenuItemtileActivity_Click(object sender, EventArgs e)
      {
         World.Settings.ShowDownloadRectangles = toolStripMenuItemtileActivity.Checked;
      }

      private void toolStripCrossHairs_Click(object sender, EventArgs e)
      {
         World.Settings.ShowCrosshairs = toolStripCrossHairs.Checked;
      }

      private void toolStripMenuItemOpenSaved_Click(object sender, EventArgs e)
      {
         ViewOpenDialog dlgtest = new ViewOpenDialog(m_worldWindow.WorldWindSettings.ConfigPath);
         DialogResult res = dlgtest.ShowDialog(this);
         if (dlgtest.ViewFile != null)
         {
            if (res == DialogResult.OK)
               OpenView(dlgtest.ViewFile, true);
         }
      }

      private void toolStripMenuItemResetDefaultView_Click(object sender, EventArgs e)
      {
         OpenView(Path.Combine(m_worldWindow.WorldWindSettings.DataPath, DefaultView), true);
      }

      private void toolStripMenuItemHomeView_Click(object sender, EventArgs e)
      {
         string strHome = Path.Combine(m_worldWindow.WorldWindSettings.ConfigPath, HomeView);
         if (File.Exists(strHome))
            OpenView(strHome, true);
         else
            OpenView(Path.Combine(m_worldWindow.WorldWindSettings.DataPath, DefaultView), true);
      }

      private void toolStripMenuItemSetHomeView_Click(object sender, EventArgs e)
      {
         if (MessageBox.Show(this, "Make the current view your default start-up Home View?", Text, MessageBoxButtons.YesNo) == DialogResult.Yes)
         {
            string tempFile = Path.ChangeExtension(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()), ".jpg");
            SaveCurrentView(Path.Combine(WWSettingsCtl.ConfigPath, HomeView), tempFile, string.Empty);
            File.Delete(tempFile);
         }
      }

      private void MainForm_Shown(object sender, EventArgs e)
      {
         // This will ensure that the buttons is enabled correctly
         ServerBuilderItem = null; 
         LayerBuilderItem = null;

         string strHome = Path.Combine(m_worldWindow.WorldWindSettings.ConfigPath, HomeView);
         if (this.openView.Length > 0)
            OpenView(openView, this.openGeoTiff.Length == 0);
         else if (this.openGeoTiff.Length == 0 && File.Exists(Path.Combine(WWSettingsCtl.ConfigPath, LastView)) &&
            MessageBox.Show(this, "Would you like to open your last View?", Text, MessageBoxButtons.YesNo) == DialogResult.Yes)
            OpenView(Path.Combine(WWSettingsCtl.ConfigPath, LastView), this.openGeoTiff.Length == 0);
         else if (File.Exists(strHome))
            OpenView(strHome, this.openGeoTiff.Length == 0);
         else
            OpenView(Path.Combine(m_worldWindow.WorldWindSettings.DataPath, DefaultView), this.openGeoTiff.Length == 0);

         if (this.openGeoTiff.Length > 0)
            AddGeoTiff(this.openGeoTiff, true);


         // Check for updates daily
         if (WWSettingsCtl.UpdateCheckDate.Date != DateTime.Now.Date)
            CheckForUpdates(false);
         WWSettingsCtl.UpdateCheckDate = DateTime.Now;
      }

      bool m_bSizing = false;
      private void MainForm_ResizeBegin(object sender, EventArgs e)
      {
         m_bSizing = true;
         m_worldWindow.Visible = false;
      }

      private void MainForm_ResizeEnd(object sender, EventArgs e)
      {
         m_bSizing = false;
         m_worldWindow.Visible = true;
         m_worldWindow.SafeRender();
      }

      private void MainForm_Resize(object sender, EventArgs e)
      {
         m_worldWindow.Visible = false;
      }

      private void MainForm_SizeChanged(object sender, EventArgs e)
      {
         if (!m_bSizing)
         {
            m_worldWindow.Visible = true;
            m_worldWindow.SafeRender();
         }
      }

      private void splitContainerMain_SplitterMoving(object sender, SplitterCancelEventArgs e)
      {
         m_worldWindow.Visible = false;
      }

      private void splitContainerMain_SplitterMoved(object sender, SplitterEventArgs e)
      {
         if (!m_bSizing)
         {
            m_worldWindow.Visible = true;
            m_worldWindow.SafeRender();
         }
      }


      private void toolStripMenuItemeditBlueMarble_Click(object sender, EventArgs e)
      {
         m_BMNGForm.ShowDialog(this);
      }

      private void toolStripMenuItemshowGridLines_Click(object sender, EventArgs e)
      {
         World.Settings.ShowLatLonLines = toolStripMenuItemshowGridLines.Checked;
         foreach (RenderableObject oRO in m_worldWindow.CurrentWorld.RenderableObjects.ChildObjects)
         {
            if (oRO.Name == "1 - Grid Lines")
            {
               oRO.IsOn = toolStripMenuItemshowGridLines.Checked;
               break;
            }
         }
      }

      private void toolStripMenuItemshowPosition_Click(object sender, EventArgs e)
      {
         World.Settings.ShowPosition = toolStripMenuItemshowPosition.Checked;
         m_worldWindow.Invalidate();
      }

      private void toolStripMenuItemsave_Click(object sender, EventArgs e)
      {
         string tempViewFile = Path.ChangeExtension(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()), ViewExt);
         string tempFile = Path.ChangeExtension(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()), ".jpg");
         SaveCurrentView(tempViewFile, tempFile, "");
         Image img = Image.FromFile(tempFile);
         SaveViewForm form = new SaveViewForm(WWSettingsCtl.ConfigPath, img);
         if (form.ShowDialog(this) == DialogResult.OK)
         {
            if (File.Exists(form.OutputPath))
               File.Delete(form.OutputPath);
            File.Move(tempViewFile, form.OutputPath);

            XmlDocument oDoc = new XmlDocument();
            oDoc.Load(form.OutputPath);
            XmlNode oRoot = oDoc.DocumentElement;
            XmlNode oNode = oDoc.CreateElement("notes");
            oNode.InnerText = form.Notes;
            oRoot.AppendChild(oNode);
            oDoc.Save(form.OutputPath);
         }
         else
         {
            File.Delete(tempViewFile);
         }
         img.Dispose();
         File.Delete(tempFile);
      }

      private void toolStripMenuItemsend_Click(object sender, EventArgs e)
      {
         string tempBodyFile = Path.ChangeExtension(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()), ".txt");
         string tempJpgFile = Path.ChangeExtension(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()), ".jpg");
         string tempViewFile = Path.ChangeExtension(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()), ViewExt);
         string strMailApp = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "System");
#if DEBUG
         strMailApp = Path.Combine(strMailApp, "mailerd.exe");
#else
         strMailApp = Path.Combine(strMailApp, "mailer.exe");
#endif

         SaveCurrentView(tempViewFile, tempJpgFile, "");


         using (StreamWriter sw = new StreamWriter(tempBodyFile))
         {
            sw.WriteLine();
            sw.WriteLine();
            sw.WriteLine("Get Dapple to view the attachment: " + WebsiteUrl + ".");
         }

         try
         {
            ProcessStartInfo psi = new ProcessStartInfo(strMailApp);
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            psi.Arguments = String.Format(CultureInfo.InvariantCulture,
               " \"{0}\" \"{1}\" \"{2}\" \"{3}\" \"{4}\" \"{5}\" \"{6}\" \"{7}\"",
               ViewFileDescr, "", "",
               tempViewFile, ViewFileDescr + ViewExt,
               tempJpgFile, ViewFileDescr + ".jpg", tempBodyFile);
            using (Process p = Process.Start(psi))
            {
               p.WaitForExit();
            }
         }
         catch
         {
         }
         
         File.Delete(tempBodyFile);
         File.Delete(tempJpgFile);
         File.Delete(tempViewFile);
      }


      private void toolStripMenuItemexit_Click(object sender, EventArgs e)
      {
         Close();
      }

      #endregion

      #region Expand/collapse Panels

     /* private void toolStripButtonCollapseOverview_Click(object sender, EventArgs e)
      {
         if (this.splitContainerOverview.Panel2Collapsed)
         {
            this.splitContainerOverview.Panel2Collapsed = false;
            this.toolStripButtonCollapseOverview.Image = global::Dapple.Properties.Resources.collapse;
         }
         else
         {
            splitContainerOverview.Panel2Collapsed = true;
            this.toolStripButtonCollapseOverview.Image = global::Dapple.Properties.Resources.expand;
         }
      }
      private void toolStripButtonCollapseLayers_Click(object sender, EventArgs e)
      {
         if (this.splitContainerLayers.Panel2Collapsed)
         {
            this.splitContainerLayers.Panel2Collapsed = false;
            this.toolStripButtonCollapseLayers.Image = global::Dapple.Properties.Resources.collapse;
            splitContainerLeft.SplitterWidth = 4;
            splitContainerLeft.IsSplitterFixed = false;
            splitContainerLeft.SplitterDistance = this.iLayerPanelLastPos;
            splitContainerLeft.Panel1MinSize = this.iLayerPanelLastMinSize;
         }
         else
         {
            this.iLayerPanelLastPos = splitContainerLeft.SplitterDistance;
            this.iLayerPanelLastMinSize = splitContainerLeft.Panel1MinSize;
            splitContainerLeft.Panel1MinSize = toolStripLayers.Height;
            splitContainerLeft.SplitterDistance = toolStripLayers.Height;
            splitContainerLeft.SplitterWidth = 1;
            splitContainerLeft.IsSplitterFixed = true; 
            this.splitContainerLayers.Panel2Collapsed = true;
            this.toolStripButtonCollapseLayers.Image = global::Dapple.Properties.Resources.expand;
         }
      }

      private void toolStripButtonCollapseServers_Click(object sender, EventArgs e)
      {
         if (this.splitContainerServers.Panel2Collapsed)
         {
            this.splitContainerServers.Panel2Collapsed = false;
            this.toolStripButtonCollapseServers.Image = global::Dapple.Properties.Resources.collapse;
            splitContainerLeftMain.SplitterWidth = 4;
            splitContainerLeftMain.IsSplitterFixed = false;
            splitContainerLeftMain.SplitterDistance = this.iServerPanelLastPos;
            splitContainerLeftMain.Panel1MinSize = this.iServerPanelLastMinSize;
         }
         else
         {
            this.iServerPanelLastPos = splitContainerLeftMain.SplitterDistance;
            this.iServerPanelLastMinSize = splitContainerLeftMain.Panel1MinSize;
            splitContainerLeftMain.Panel1MinSize = toolStripServers.Height;
            splitContainerLeftMain.SplitterDistance = toolStripServers.Height;
            splitContainerLeftMain.SplitterWidth = 1;
            splitContainerLeftMain.IsSplitterFixed = true;
            this.splitContainerServers.Panel2Collapsed = true;
            this.toolStripButtonCollapseServers.Image = global::Dapple.Properties.Resources.expand;
         }
      }
      */
      #endregion

      #region Servers Panel

      private void treeViewServers_AfterSelect(object sender, TreeViewEventArgs e)
      {
         if (e.Node != null && e.Node.Tag != null)
            ServerBuilderItem = e.Node.Tag as IBuilder;
         else
            ServerBuilderItem = null;
      }

      private void treeViewServers_MouseDown(object sender, MouseEventArgs e)
      {
         treeViewServers.SelectedNode = treeViewServers.HitTest(e.Location).Node;

         if (treeViewServers.SelectedNode == null)
            ServerBuilderItem = null;
      }
      #endregion

      #region Current Layer Panel

      private void triStateTreeViewLayers_AfterSelect(object sender, TreeViewEventArgs e)
      {
         if (e.Node != null && e.Node.Tag != null)
            LayerBuilderItem = e.Node.Tag as LayerBuilderContainer;
         else
            LayerBuilderItem = null;
      }

      private void triStateTreeViewLayers_TreeNodeChecked(object sender, Geosoft.DotNetTools.TreeNodeCheckedEventArgs e)
      {
         if (e.Node == null || e.Node.Tag == null)
            return;

         if (this.rightmouse_context)
         {
            this.rightmouse_context = false;
            this.checked_context = true;
         }

         (e.Node.Tag as LayerBuilderContainer).Visible = e.State == Geosoft.DotNetTools.TriStateTreeView.CheckBoxState.Checked;
         if (LayerBuilderItem == e.Node.Tag as LayerBuilderContainer)
         {
            if (LayerBuilderItem.Visible)
               this.trackBarTransp.Enabled = true;
            else
               this.trackBarTransp.Enabled = false;
         }
      }

      private void triStateTreeViewLayers_KeyUp(object sender, KeyEventArgs e)
      {
         switch (e.KeyCode)
         {
            case Keys.Delete:
               toolStripMenuButtonItemremove_Click(sender, e);
               break;
         }
      }

      private void triStateTreeViewLayers_MouseDown(object sender, MouseEventArgs e)
      {
         if (e.Button == MouseButtons.Right)
             this.rightmouse_context = true;
         else
             this.rightmouse_context = false;

         triStateTreeViewLayers.SelectedNode = triStateTreeViewLayers.HitTest(e.Location).Node;
         if (triStateTreeViewLayers.SelectedNode == null)
            LayerBuilderItem = null;
      }

      #endregion

      #region Transparency Slider Related Events

      private void trackBarTransp_Paint(object sender, PaintEventArgs e)
      {
         // Be careful accessing any properties in here (e.g. Value) as it may cause native messages to be sent in which case the loop may be broken

         Graphics g = e.Graphics;
         int iPos = 8 + iLastTransparency * (trackBarTransp.Bounds.Width - 26) / 255;

         // Attempt to match the toolstrip next to it by drawing with same colors, bottom line and separator
         g.FillRectangle(SystemBrushes.ButtonFace, trackBarTransp.Bounds.X, trackBarTransp.Bounds.Y, trackBarTransp.Bounds.Width, trackBarTransp.Bounds.Height);
         g.DrawLine(Pens.LightGray, trackBarTransp.Bounds.X, trackBarTransp.Bounds.Y + trackBarTransp.Bounds.Height - 2, trackBarTransp.Bounds.X + trackBarTransp.Bounds.Width, trackBarTransp.Bounds.Y + trackBarTransp.Bounds.Height - 2);
         g.DrawLine(Pens.White, trackBarTransp.Bounds.X, trackBarTransp.Bounds.Y + trackBarTransp.Bounds.Height - 1, trackBarTransp.Bounds.X + trackBarTransp.Bounds.Width, trackBarTransp.Bounds.Y + trackBarTransp.Bounds.Height - 1);
         Rectangle rect = new Rectangle(trackBarTransp.Bounds.X + 3, trackBarTransp.Bounds.Y + trackBarTransp.Bounds.Height / 2 - 8, trackBarTransp.Bounds.Width - 7, 16);
         if (trackBarTransp.Enabled)
         {
            using (LinearGradientBrush lgb = new LinearGradientBrush(rect, SystemColors.ButtonFace, SystemColors.ControlDarkDark, LinearGradientMode.Horizontal))
               g.FillRectangle(lgb, rect);
            g.DrawRectangle(SystemPens.ActiveCaption, rect);
            g.FillRectangle(SystemBrushes.ControlDarkDark, trackBarTransp.Bounds.X + 9, trackBarTransp.Bounds.Y + trackBarTransp.Bounds.Height / 2 - 1, trackBarTransp.Bounds.Width - 17, 2);
            g.DrawImage(global::Dapple.Properties.Resources.trackbutton, trackBarTransp.Bounds.X + iPos, trackBarTransp.Bounds.Y + 1, global::Dapple.Properties.Resources.trackbutton.Width, trackBarTransp.Bounds.Height - 3);
         }
         else
         {
            using (LinearGradientBrush lgb = new LinearGradientBrush(rect, SystemColors.ButtonFace, SystemColors.ControlDark, LinearGradientMode.Horizontal))
               g.FillRectangle(lgb, rect);
            g.DrawRectangle(SystemPens.ControlDark, rect);
            g.FillRectangle(SystemBrushes.ControlDark, trackBarTransp.Bounds.X + 9, trackBarTransp.Bounds.Y + trackBarTransp.Bounds.Height / 2 - 1, trackBarTransp.Bounds.Width - 17, 2);
            g.DrawImage(global::Dapple.Properties.Resources.trackbutton_disable, trackBarTransp.Bounds.X + iPos, trackBarTransp.Bounds.Y + 1, global::Dapple.Properties.Resources.trackbutton.Width, trackBarTransp.Bounds.Height - 3);
         }
         if (e.ClipRectangle.X > 0)
            trackBarTransp.Invalidate();
      }

      private void trackBarTransp_ValueChanged(object sender, EventArgs e)
      {
         iLastTransparency = trackBarTransp.Value;
         if (LayerBuilderItem != null && this.trackBarTransp.Enabled)
            LayerBuilderItem.Opacity = Convert.ToByte(trackBarTransp.Value);
         
         // Invalidate to make sure our custom paintjob is fresh
         trackBarTransp.Invalidate();
      }

      #endregion

      #region Export

      private void SaveGeoImage(Bitmap bitMap, string strName, string strFolder, string strFormat, string strGeotiff)
      {
         ImageFormat format = ImageFormat.Tiff;
         if (strFormat == ".bmp")
            format = ImageFormat.Bmp;
         else if (strFormat == ".png")
            format = ImageFormat.Png;
         else if (strFormat == ".gif")
            format = ImageFormat.Gif;

         foreach (char c in Path.GetInvalidFileNameChars())
            strName = strName.Replace(c, '_');

         if (format == ImageFormat.Tiff)
         {
            string strTempBM = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + strFormat);
            bitMap.Save(strTempBM, format);

            ProcessStartInfo psi = new ProcessStartInfo(Path.GetDirectoryName(Application.ExecutablePath) + @"\System\geotifcp.exe");
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            psi.Arguments = "-g \"" + strGeotiff + "\" \"" + strTempBM + "\" \"" + Path.Combine(strFolder, strName + strFormat) + "\"";

            using (Process p = Process.Start(psi))
               p.WaitForExit();

            try
            {
               File.Delete(strTempBM);
            }
            catch
            {
            }
         }
         else
            bitMap.Save(Path.Combine(strFolder, strName + strFormat), format);
      }

      private bool bIsDownloading()
      {
         int iRead, iTotal;
         RenderableObject roBMNG = GetActiveBMNG();

         if (roBMNG != null && roBMNG.IsOn && ((QuadTileSet)((RenderableObjectList)roBMNG).ChildObjects[1]).bIsDownloading(out iRead, out iTotal))
            return true;
         
         foreach (LayerBuilderContainer container in m_ActiveLayers)
         {
            if (container.Visible && container.Builder != null && container.Builder.bIsDownloading(out iRead, out iTotal))
               return true;
         }

         return false;
      }

      private class ExportEntry
      {
         public LayerBuilderContainer Container;
         public RenderableObject RO;
         public RenderableObject.ExportInfo Info;

         public ExportEntry(LayerBuilderContainer container, RenderableObject ro, RenderableObject.ExportInfo expInfo)
         {
            Container = container;
            RO = ro;
            Info = expInfo;
         }
      }

      private void toolStripMenuItemExport_Click(object sender, EventArgs e)
      {
         string strGeotiff = null;
         List<ExportEntry> expList = new List<ExportEntry>();
         RenderableObject roBMNG = GetActiveBMNG();


         // Gather info first
         foreach (LayerBuilderContainer container in m_ActiveLayers)
         {
            if (container.Visible && container.Builder != null)
            {
               RenderableObject ro = container.Builder.GetLayer();
               if (ro != null)
               {
                  RenderableObject.ExportInfo expinfo = new RenderableObject.ExportInfo();
                  ro.InitExportInfo(m_worldWindow.DrawArgs, expinfo);

                  if (expinfo.iPixelsX > 0 && expinfo.iPixelsY > 0)
                     expList.Add(new ExportEntry(container, ro, expinfo));
               }
            }
         }
         if (roBMNG != null && roBMNG.IsOn)
         {
            RenderableObject.ExportInfo expinfo = new RenderableObject.ExportInfo();
            roBMNG.InitExportInfo(m_worldWindow.DrawArgs, expinfo);
            expList.Add(new ExportEntry(null, roBMNG, expinfo));
         }
         
         // Reverse the list to do render order right
         expList.Reverse();

         if (expList.Count == 0)
         {
            MessageBox.Show(this, "There are no visible layers to export.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
         }

         if (bIsDownloading())
         {
            MessageBox.Show(this, "It is not possible to export a view while there are still downloads in progress.\nPlease wait for the downloads to complete and try again.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
         }

         WorldWind.Camera.MomentumCamera camera = m_worldWindow.DrawArgs.WorldCamera as WorldWind.Camera.MomentumCamera;
         if (camera.Tilt.Degrees > 5.0)
         {
            MessageBox.Show(this, "It is not possible to export a tilted view. Reset the tilt using the navigation buttons\nor by using Right-Mouse-Button and drag and try again.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
         }

         try
         {
            ExportView dlg = new ExportView(m_worldWindow.WorldWindSettings.ConfigPath);
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
               Cursor = Cursors.WaitCursor;

               // Stop the camera
               camera.SetPosition(camera.Latitude.Degrees, camera.Longitude.Degrees, camera.Heading.Degrees, camera.Altitude, camera.Tilt.Degrees);
               
               // Determine output parameters
               GeographicBoundingBox geoExtent = GeographicBoundingBox.FromQuad(m_worldWindow.GetViewBox());
               int iResolution = dlg.Resolution;
               int iExportPixelsX, iExportPixelsY;

               // Minimize the estimated extents to what is available
               GeographicBoundingBox geoExtentLayers = new GeographicBoundingBox(double.MinValue, double.MaxValue, double.MaxValue, double.MinValue);
               foreach (ExportEntry exp in expList)
               {
                  geoExtentLayers.North = Math.Max(geoExtentLayers.North, exp.Info.dMaxLat);
                  geoExtentLayers.South = Math.Min(geoExtentLayers.South, exp.Info.dMinLat);
                  geoExtentLayers.East = Math.Max(geoExtentLayers.East, exp.Info.dMaxLon);
                  geoExtentLayers.West = Math.Min(geoExtentLayers.West, exp.Info.dMinLon);
               }
               if (geoExtent.East > geoExtentLayers.East)
                  geoExtent.East = geoExtentLayers.East;
               if (geoExtent.North > geoExtentLayers.North)
                  geoExtent.North = geoExtentLayers.North;
               if (geoExtent.West < geoExtentLayers.West)
                  geoExtent.West = geoExtentLayers.West;
               if (geoExtent.South < geoExtentLayers.South)
                  geoExtent.South = geoExtentLayers.South;

               // Determine the maximum resolution based on the highest res in layers
               if (iResolution == -1)
               {
                  double dXRes, dYRes;
                  foreach (ExportEntry exp in expList)
                  {
                     dXRes = (double)exp.Info.iPixelsX / (exp.Info.dMaxLon - exp.Info.dMinLon);
                     dYRes = (double)exp.Info.iPixelsY / (exp.Info.dMaxLat - exp.Info.dMinLat);
                     iResolution = Math.Max(iResolution, (int)Math.Round(Math.Max(dXRes * (geoExtent.East - geoExtent.West), dYRes * (geoExtent.North - geoExtent.South))));
                  }
               }
               if (iResolution <= 0)
                  return;

               if (geoExtent.North - geoExtent.South > geoExtent.East - geoExtent.West)
               {
                  iExportPixelsY = iResolution;
                  iExportPixelsX = (int)Math.Round((double)iResolution * (geoExtent.East - geoExtent.West) / (geoExtent.North - geoExtent.South));
               }
               else
               {
                  iExportPixelsX = iResolution;
                  iExportPixelsY = (int)Math.Round((double)iResolution * (geoExtent.North - geoExtent.South) / (geoExtent.East - geoExtent.West));
               }


               // Make geotiff metadata file to use for georeferencing images
               strGeotiff = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
               using (StreamWriter sw = new StreamWriter(strGeotiff, false))
               {
                  sw.WriteLine("Geotiff_Information:");
                  sw.WriteLine("Version: 1");
                  sw.WriteLine("Key_Revision: 1.0");
                  sw.WriteLine("Tagged_Information:");
                  sw.WriteLine("ModelTiepointTag (2,3):");
                  sw.WriteLine("0 0 0");
                  sw.WriteLine(geoExtent.West.ToString() + " " + geoExtent.North.ToString() + " 0");
                  sw.WriteLine("ModelPixelScaleTag (1,3):");
                  sw.WriteLine(((geoExtent.East - geoExtent.West) / (double)iExportPixelsX).ToString() + " " + ((geoExtent.North - geoExtent.South) / (double)iExportPixelsY).ToString() + " 0");
                  sw.WriteLine("End_Of_Tags.");
                  sw.WriteLine("Keyed_Information:");
                  sw.WriteLine("GTModelTypeGeoKey (Short,1): ModelTypeGeographic");
                  sw.WriteLine("GTRasterTypeGeoKey (Short,1): RasterPixelIsArea");
                  sw.WriteLine("GeogAngularUnitsGeoKey (Short,1): Angular_Degree");
                  sw.WriteLine("GeographicTypeGeoKey (Short,1): GCS_WGS_84");
                  sw.WriteLine("End_Of_Keys.");
                  sw.WriteLine("End_Of_Geotiff.");
               }

               // Export image(s)
               using (Bitmap bitMapMain = new Bitmap(iExportPixelsX, iExportPixelsY))
               {
                  using (Graphics gr = Graphics.FromImage(bitMapMain))
                  {
                     gr.FillRectangle(Brushes.White, new Rectangle(0, 0, iExportPixelsX, iExportPixelsY));
                     foreach (ExportEntry exp in expList)
                     {
                        using (Bitmap bitMap = new Bitmap(exp.Info.iPixelsX, exp.Info.iPixelsY))
                        {
                           int iOffsetX, iOffsetY;
                           int iWidth, iHeight;

                           using (exp.Info.gr = Graphics.FromImage(bitMap))
                              exp.RO.ExportProcess(m_worldWindow.DrawArgs, exp.Info);

                           iOffsetX = (int)Math.Round((exp.Info.dMinLon - geoExtent.West) * (double)iExportPixelsX / (geoExtent.East - geoExtent.West));
                           iOffsetY = (int)Math.Round((geoExtent.North - exp.Info.dMaxLat) * (double)iExportPixelsY / (geoExtent.North - geoExtent.South));
                           iWidth = (int)Math.Round((exp.Info.dMaxLon - exp.Info.dMinLon) * (double)iExportPixelsX / (geoExtent.East - geoExtent.West));
                           iHeight = (int)Math.Round((exp.Info.dMaxLat - exp.Info.dMinLat) * (double)iExportPixelsY / (geoExtent.North - geoExtent.South));

                           if (exp.Container != null)
                           {
                              ImageAttributes imgAtt = new ImageAttributes();
                              float[][] fMat = { 
                                    new float[] {1.0f, 0.0f, 0.0f, 0.0f, 0.0f},
                                    new float[] {0.0f, 1.0f, 0.0f, 0.0f, 0.0f},
                                    new float[] {0.0f, 0.0f, 1.0f, 0.0f, 0.0f},
                                    new float[] {0.0f, 0.0f, 0.0f, (float)exp.Container.Opacity/255.0f, 0.0f},
                                    new float[] {0.0f, 0.0f, 0.0f, 0.0f, 1.0f}
                                 };
                              ColorMatrix clrMatrix = new ColorMatrix(fMat);
                              imgAtt.SetColorMatrix(clrMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                              gr.DrawImage(bitMap, new Rectangle(iOffsetX, iOffsetY, iWidth, iHeight), 0, 0, exp.Info.iPixelsX, exp.Info.iPixelsY, GraphicsUnit.Pixel, imgAtt);
                           }
                           else
                           {
                              gr.DrawImage(bitMap, new Rectangle(iOffsetX, iOffsetY, iWidth, iHeight), 0, 0, exp.Info.iPixelsX, exp.Info.iPixelsY, GraphicsUnit.Pixel);
                           }
                           if (dlg.KeepLayers)
                           {
                              using (Bitmap bitMapLayer = new Bitmap(iExportPixelsX, iExportPixelsY))
                              {
                                 using (Graphics grl = Graphics.FromImage(bitMapLayer))
                                 {
                                    grl.FillRectangle(Brushes.White, new Rectangle(0, 0, iExportPixelsX, iExportPixelsY));
                                    grl.DrawImage(bitMap, new Rectangle(iOffsetX, iOffsetY, iWidth, iHeight), 0, 0, exp.Info.iPixelsX, exp.Info.iPixelsY, GraphicsUnit.Pixel);
                                 }

                                 if (exp.Container != null)
                                    SaveGeoImage(bitMapLayer, dlg.OutputName + "_" + exp.Container.Name, dlg.Folder, dlg.OutputFormat, strGeotiff);
                                 else
                                    SaveGeoImage(bitMapLayer, dlg.OutputName + "_Blue Marble(" + exp.RO.Name + ")", dlg.Folder, dlg.OutputFormat, strGeotiff);
                              }
                           }
                        }
                     }
                  }
                  SaveGeoImage(bitMapMain, dlg.OutputName, dlg.Folder, dlg.OutputFormat, strGeotiff);
               }
            }
         }
         catch (Exception exc)
         {
            MessageBox.Show(this, "Export failed!\n" + exc.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            Cursor = Cursors.Default;
         }
         finally
         {
            if (strGeotiff != null && File.Exists(strGeotiff))
               File.Delete(strGeotiff);

            Cursor = Cursors.Default;
         }
      }

      #endregion
     
      #region Private (Helper) Methods

      /// <summary>
      /// Returns imagelist index from key name
      /// </summary>
      /// <param name="strKey"></param>
      /// <returns></returns>
      public int ImageListIndex(string strKey)
      {
         return this.iconImageList.Images.IndexOfKey(strKey);
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
               treeNode.SelectedImageIndex = treeNode.ImageIndex = ImageListIndex("folder");
               treeNode.Tag = childDir;
               ProcessCatalogDirectory(childDir, treeNode);
            }
         }
         foreach (LayerBuilder builder in dir.LayerBuilders)
         {
            TreeNode treeNode;
            if (builder is DAPQuadLayerBuilder)
            {
               int iImageIndex;
               DAPQuadLayerBuilder dapbuilder = (DAPQuadLayerBuilder)builder;

               iImageIndex = ImageListIndex("dap_" + dapbuilder.DAPType.ToLower());
               if (iImageIndex == -1)
                  ImageListIndex("layer");
               treeNode = parent.Nodes.Add(builder.Name, builder.Name, iImageIndex, iImageIndex);
            }
            else
               treeNode = parent.Nodes.Add(builder.Name, builder.Name, ImageListIndex("layer"), ImageListIndex("layer"));
            treeNode.Tag = builder;
         }
      }

      /// <summary>
      /// Recursive helper method to RefreshTree()
      /// </summary>
      /// <param name="parent"></param>
      /// <param name="listing"></param>
      private void RefreshTreeHelper(TreeView tree, TreeNode parent, BuilderDirectory listing)
      {
         foreach (BuilderDirectory dir in listing.SubList)
         {
            TreeNode treeNode = new TreeNode(dir.Name);
            treeNode.SelectedImageIndex = treeNode.ImageIndex = ImageListIndex("layer");
            treeNode.Tag = dir;
            if (parent != null)
               parent.Nodes.Add(treeNode);
            else
               tree.Nodes.Add(treeNode);
            RefreshTreeHelper(tree, treeNode, dir);
         }
         foreach (LayerBuilder builder in listing.LayerBuilders)
         {
            AddTreeNode(tree, builder, parent);
         }
      }

      private void AddTreeNode(TreeView tree, LayerBuilder builder, TreeNode parent)
      {
         int iImageIndex;
         TreeNode treeNode = null;
         if (builder is DAPQuadLayerBuilder)
         {
            DAPQuadLayerBuilder dapbuilder = (DAPQuadLayerBuilder)builder;

            iImageIndex = ImageListIndex("dap_" + dapbuilder.DAPType.ToLower());
            if (iImageIndex == -1)
               ImageListIndex("layer");
         }
         else if (builder is VEQuadLayerBuilder)
            iImageIndex = ImageListIndex("live");
         else
            iImageIndex = ImageListIndex("layer");

         treeNode = parent.Nodes.Add(builder.Name, builder.Name, iImageIndex, iImageIndex);
         treeNode.Tag = builder;
      }

      #endregion

      #region Open/Save View Methods

      void SaveLastView()
      {
         string tempFile = Path.ChangeExtension(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()), ".jpg");
         if (this.lastView.Length == 0)
            SaveCurrentView(Path.Combine(WWSettingsCtl.ConfigPath, LastView), tempFile, string.Empty);
         else
            SaveCurrentView(this.lastView, tempFile, string.Empty);
         File.Delete(tempFile);
      }

      void PopulateEntry(builderentryType entry, TreeNode node)
      {
         IBuilder builder = node.Tag as IBuilder;
         if (builder is DAPServerBuilder)
         {
            dapcatalogType dap = entry.Newdapcatalog();
            dap.Addurl(new SchemaString((builder as DAPServerBuilder).URL));
            entry.Adddapcatalog(dap);
         }
         else if (builder is WMSServerBuilder)
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
                  tilelayer.Addlevelzerotilesize(new SchemaDouble((double) layer.LevelZeroTileSize));
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
         else if (builder is BuilderDirectory)
         {
            builderdirectoryType dir = entry.Newbuilderdirectory();
            dir.Addname(new SchemaString(node.Text));
            if (builder is DAPCatalogBuilder)
               dir.Addspecialcontainer(new SpecialDirectoryType("DAPServers"));
            else if (builder is WMSCatalogBuilder)
               dir.Addspecialcontainer(new SpecialDirectoryType("WMSServers"));
            else if (builder is TileSetBuilder)
               dir.Addspecialcontainer(new SpecialDirectoryType("ImageServers"));
            foreach (TreeNode subnode in node.Nodes)
            {
               builderentryType subentry = dir.Newbuilderentry();
               PopulateEntry(subentry, subnode);
               dir.Addbuilderentry(subentry);
            }

            entry.Addbuilderdirectory(dir);
         }
      }

      /// <summary>
      /// Saves the current view to an xml file, 
      /// this requires that worldWindow was created in the same thread as the caller
      /// </summary>
      /// <param name="fileName"></param>
      /// <param name="notes"></param>
      void SaveCurrentView(string fileName, string picFileName, string notes)
      {
         DappleView view = new DappleView();

         // blue marble
         RenderableObject roBMNG = GetBMNG();
         if (roBMNG != null)
            view.View.Addshowbluemarble(new SchemaBoolean(roBMNG.IsOn));

         WorldWind.Camera.MomentumCamera camera = m_worldWindow.DrawArgs.WorldCamera as WorldWind.Camera.MomentumCamera;

         //stop the camera
         camera.SetPosition(camera.Latitude.Degrees, camera.Longitude.Degrees, camera.Heading.Degrees, camera.Altitude, camera.Tilt.Degrees);

         //store the servers

         TreeView tree = treeViewServerBackup != null ? treeViewServerBackup : treeViewServers;
         if (tree.Nodes.Count > 0)
         {
            serversType servers = view.View.Newservers();
            foreach (TreeNode node in tree.Nodes)
            {
               builderentryType entry = servers.Newbuilderentry();
               PopulateEntry(entry, node);
               servers.Addbuilderentry(entry);
            }
            view.View.Addservers(servers);
         }

         // store the current layers
         if (m_ActiveLayers.Count > 0)
         {
            activelayersType lyrs = view.View.Newactivelayers();
            foreach (LayerBuilderContainer container in m_ActiveLayers)
            {
               datasetType dataset = lyrs.Newdataset();
               dataset.Addname(new SchemaString(container.Name));
               opacityType op = dataset.Newopacity();
               op.Value = container.Opacity;
               dataset.Addopacity(op);
               dataset.Adduri(new SchemaString(container.Uri));
               dataset.Addinvisible(new SchemaBoolean(!container.Visible));
               lyrs.Adddataset(dataset);
            }
            view.View.Addactivelayers(lyrs);
         }

         // store the camera information
         cameraorientationType cameraorient = view.View.Newcameraorientation();
         cameraorient.Addlat(new SchemaDouble(camera.Latitude.Degrees));
         cameraorient.Addlon(new SchemaDouble(camera.Longitude.Degrees));
         cameraorient.Addaltitude(new SchemaDouble(camera.Altitude));
         cameraorient.Addheading(new SchemaDouble(camera.Heading.Degrees));
         cameraorient.Addtilt(new SchemaDouble(camera.Tilt.Degrees));
         view.View.Addcameraorientation(cameraorient);
         
         if (notes.Length > 0)
            view.View.Addnotes(new SchemaString(notes));
         
         // Save screen capture (The regular WorldWind method crashes some systems, use interop)
         //m_worldWindow.SaveScreenshot(picFileName);
         //m_worldWindow.Render();

         using (Image img = TakeSnapshot(m_worldWindow.Handle))
            img.Save(picFileName, System.Drawing.Imaging.ImageFormat.Jpeg);

         FileStream fs = new FileStream(picFileName, FileMode.Open);
         BinaryReader br = new BinaryReader(fs);
         byte[] buffer = new byte[fs.Length];
         br.Read(buffer, 0, Convert.ToInt32(fs.Length));
         br.Close();
         fs.Close();
         view.View.Addpreview(new SchemaBase64Binary(buffer));

         view.Save(fileName);
      }

      private struct RECT
      {
         public int left;
         public int top;
         public int right;
         public int bottom;

         public static implicit operator Rectangle(RECT rect)
         {
            return new Rectangle(rect.left, rect.top, rect.right - rect.left,
            rect.bottom - rect.top);
         }
      }

      private const int DCX_WINDOW = 0x00000001;
      private const int DCX_CACHE = 0x00000002;
      private const int DCX_LOCKWINDOWUPDATE = 0x00000400;
      private const int SRCCOPY = 0x00CC0020;
      private const int CAPTUREBLT = 0x40000000;


      [System.Runtime.InteropServices.DllImport("user32.dll")]
      private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

      [System.Runtime.InteropServices.DllImport("user32.dll")]
      private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

      [System.Runtime.InteropServices.DllImport("user32.dll")]
      private static extern IntPtr GetDCEx(IntPtr hWnd, IntPtr hrgnClip, int
      flags);

      [System.Runtime.InteropServices.DllImport("user32.dll")]
      private static extern IntPtr GetDC(IntPtr hWnd);

      [System.Runtime.InteropServices.DllImport("user32.dll")]
      private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDc);

      [System.Runtime.InteropServices.DllImport("gdi32.dll")]
      private static extern bool BitBlt(
      IntPtr hdcDest,
      int nXDest,
      int nYDest,
      int nWidth,
      int nHeight,
      IntPtr hdcSrc,
      int nXSrc,
      int nYSrc,
      int dwRop
      );

      public static Image TakeSnapshot(IntPtr handle)
      {
         RECT tempRect;
         GetWindowRect(handle, out tempRect);
         Rectangle windowRect = tempRect;
         IntPtr formDC = GetDCEx(handle, IntPtr.Zero, DCX_CACHE | DCX_WINDOW);
         Graphics grfx = Graphics.FromHdc(formDC);

         Bitmap bmp = new Bitmap(windowRect.Width, windowRect.Height, grfx);
         using (grfx = Graphics.FromImage(bmp))
         {
            IntPtr bmpDC = grfx.GetHdc();

            BitBlt(bmpDC, 0, 0, bmp.Width, bmp.Height, formDC, 0, 0, CAPTUREBLT |
            SRCCOPY);
            grfx.ReleaseHdc(bmpDC);
            ReleaseDC(handle, formDC);
         }
         return bmp;
      }

      void LoadBuilderEntryIntoNode(builderentryType entry, TreeView serverTree, TriStateTreeView layerTree, TreeNode serverNode, LayerBuilderList activeList)
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
            serverNodes = serverTree.Nodes;
         }

         
         if (entry.Hasbuilderdirectory())
         {
            newServerNode = serverNodes.Add(entry.builderdirectory.name.Value);

            if (entry.builderdirectory.Hasspecialcontainer())
            {
               if (entry.builderdirectory.specialcontainer.Value == "DAPServers")
               {
                  DAPCatalogBuilder dapBuilder = new DAPCatalogBuilder(WWSettingsCtl.WorldWindDirectory, WWSettingsCtl.CachePath, m_worldWindow.CurrentWorld, entry.builderdirectory.name.Value, Parent, serverTree, layerTree, activeList);
                  dapBuilder.LoadingCompleted += new LoadingCompletedCallbackHandler(OnCatalogLoaded);
                  dapBuilder.LoadingFailed += new LoadingFailedCallbackHandler(OnCatalogFailed);

                  newServerNode.SelectedImageIndex = newServerNode.ImageIndex = ImageListIndex("dap");

                  dir = dapBuilder;
               }
               else if (entry.builderdirectory.specialcontainer.Value == "ImageServers")
               {
                  TileSetBuilder tileDir = new TileSetBuilder(newServerNode.Text, Parent, false);
                  newServerNode.SelectedImageIndex = newServerNode.ImageIndex = ImageListIndex("tile");
                  dir = tileDir;
               }
               else if (entry.builderdirectory.specialcontainer.Value == "WMSServers")
               {
                  WMSCatalogBuilder wmsBuilder = new WMSCatalogBuilder(WWSettingsCtl.WorldWindDirectory, m_worldWindow, entry.builderdirectory.name.Value, Parent, serverTree, layerTree, activeList);
                  wmsBuilder.LoadingCompleted += new LayerGeneration.LoadingCompletedCallbackHandler(OnCatalogLoaded);
                  wmsBuilder.LoadingFailed += new LoadingFailedCallbackHandler(OnCatalogFailed);

                  newServerNode.SelectedImageIndex = newServerNode.ImageIndex = ImageListIndex("wms");
                  dir = wmsBuilder;
               }
               else
                  return;
            }
            else
            {
               newServerNode.SelectedImageIndex = newServerNode.ImageIndex = ImageListIndex("local");
               dir = new BuilderDirectory(newServerNode.Text, Parent, false);
            }
            newServerNode.Tag = dir;
            for (i = 0; i < entry.builderdirectory.builderentryCount; i++)
               LoadBuilderEntryIntoNode(entry.builderdirectory.GetbuilderentryAt(i), serverTree, layerTree, newServerNode, activeList); 
         }
         else if (entry.Hasdapcatalog())
         {
            // Need to have a catalog builder in treenode's parents
            IBuilder parentCatalog = Parent as IBuilder;

            while (parentCatalog != null && !(parentCatalog is DAPCatalogBuilder))
               parentCatalog = parentCatalog.Parent;

            if (parentCatalog != null)
            {
               DAPCatalogBuilder dapBuilder = parentCatalog as DAPCatalogBuilder;
               Geosoft.GX.DAPGetData.Server dapServer = dapBuilder.FindServer(entry.dapcatalog.url.Value);
               if (dapServer == null)
               {
                  lock (lockCatalogUpdates)
                  {
                     dapServer = new Geosoft.GX.DAPGetData.Server(entry.dapcatalog.url.Value, WWSettingsCtl.CachePath);
                     BuilderDirectory dapDir = dapBuilder.AddDapServer(dapServer, new Geosoft.Dap.Common.BoundingBox(180, 90, -180, -90), Parent);
                     dapBuilder.SubList.Add(dapDir);
                     newServerChildNode = serverNodes.Add("Loading: " + entry.dapcatalog.url.Value);
                     newServerChildNode.SelectedImageIndex = newServerChildNode.ImageIndex = ImageListIndex("time");
                     newServerChildNode.Tag = dapDir;
                  }
               }
            }
         }
         else if (entry.Haswmscatalog())
         {
            // Need to have a catalog builder in treenode's parents
            IBuilder parentCatalog = Parent as IBuilder;

            while (parentCatalog != null && !(parentCatalog is WMSCatalogBuilder))
               parentCatalog = parentCatalog.Parent;

            if (parentCatalog != null)
            {
               WMSCatalogBuilder wmsBuilder = parentCatalog as WMSCatalogBuilder;
               WMSList wmsList = wmsBuilder.FindServer(entry.wmscatalog.capabilitiesurl.Value);
               if (wmsList == null)
               {
                  lock (lockCatalogUpdates)
                  {
                     BuilderDirectory wmsDir = wmsBuilder.AddServer(entry.wmscatalog.capabilitiesurl.Value, Parent as BuilderDirectory);
                     wmsBuilder.SubList.Add(wmsDir);
                     newServerChildNode = serverNodes.Add("Loading: " + entry.wmscatalog.capabilitiesurl.Value);
                     newServerChildNode.SelectedImageIndex = newServerChildNode.ImageIndex = ImageListIndex("time");
                     newServerChildNode.Tag = wmsDir;
                  }
               }
            }
         }
         else if (entry.Hastileserverset())
         {
            TileSetSet tileDir = new TileSetSet(entry.tileserverset.name.Value, Parent, false);
            newServerChildNode = serverNodes.Add(entry.tileserverset.name.Value);
            newServerChildNode.SelectedImageIndex = newServerChildNode.ImageIndex = ImageListIndex("tile");
            newServerChildNode.Tag = tileDir;
            if (entry.tileserverset.Hastilelayers())
            {
               for (j = 0; j < entry.tileserverset.tilelayers.tilelayerCount; j++)
               {
                  tilelayerType tile = entry.tileserverset.tilelayers.GettilelayerAt(j);
                  newServerChildSubNode = newServerChildNode.Nodes.Add(tile.name.Value);
                  newServerChildSubNode.SelectedImageIndex = newServerChildSubNode.ImageIndex = ImageListIndex("layer");

                  int iDistance = tile.Hasdistanceabovesurface() ? tile.distanceabovesurface.Value : Convert.ToInt32(tilelayerType.GetdistanceabovesurfaceDefault());
                  int iPixelSize = tile.Hastilepixelsize() ? tile.tilepixelsize.Value : Convert.ToInt32(tilelayerType.GettilepixelsizeDefault());
                  ImageTileService tileService = new ImageTileService(tile.dataset.Value, tile.url.Value);
                  QuadLayerBuilder quadBuilder = new QuadLayerBuilder(tile.name.Value, iDistance, true, new GeographicBoundingBox(tile.boundingbox.maxlat.Value, tile.boundingbox.minlat.Value, tile.boundingbox.minlon.Value, tile.boundingbox.maxlon.Value), (decimal) tile.levelzerotilesize.Value, tile.levels.Value, iPixelSize, tileService,
                                                          tile.imageextension.Value, 255, m_worldWindow.CurrentWorld, WWSettingsCtl.CachePath, WWSettingsCtl.CachePath, tileDir);
                  newServerChildSubNode.Tag = quadBuilder;
               }
            }
         }
         else if (entry.Hasvirtualearth())
         {
            VETileSetBuilder veDir = new VETileSetBuilder(entry.virtualearth.name.Value, Parent, false);
            newServerChildNode = serverNodes.Add(entry.virtualearth.name.Value);
            newServerChildNode.SelectedImageIndex = newServerChildNode.ImageIndex = ImageListIndex("live");
            newServerChildNode.Tag = veDir;

            VEQuadLayerBuilder q = new VEQuadLayerBuilder("Virtual Earth Map", VEQuadLayerBuilder.VirtualEarthMapType.road, m_worldWindow, true, m_worldWindow.CurrentWorld, m_worldWindow.WorldWindSettings.CachePath, veDir);
            newServerChildSubNode = newServerChildNode.Nodes.Add("Map", "Map", ImageListIndex("live"), ImageListIndex("live"));
            newServerChildSubNode.Tag = q;
            veDir.LayerBuilders.Add(q);
            q = new VEQuadLayerBuilder("Virtual Earth Satellite", VEQuadLayerBuilder.VirtualEarthMapType.aerial, m_worldWindow, true, m_worldWindow.CurrentWorld, m_worldWindow.WorldWindSettings.CachePath, veDir);
            newServerChildSubNode = newServerChildNode.Nodes.Add("Satellite", "Satellite", ImageListIndex("live"), ImageListIndex("live"));
            newServerChildSubNode.Tag = q;
            veDir.LayerBuilders.Add(q);
            q = new VEQuadLayerBuilder("Virtual Earth Map & Satellite", VEQuadLayerBuilder.VirtualEarthMapType.hybrid, m_worldWindow, true, m_worldWindow.CurrentWorld, m_worldWindow.WorldWindSettings.CachePath, veDir);
            newServerChildSubNode = newServerChildNode.Nodes.Add("Map & Satellite", "Map & Satellite", ImageListIndex("live"), ImageListIndex("live"));
            newServerChildSubNode.Tag = q;
            veDir.LayerBuilders.Add(q);
         }
      }

      bool OpenView(string filename, bool bGoto)
      {
         try
         {
            if (File.Exists(filename))
            {
               int i;
               DappleView view = new DappleView(filename);
               bool bShowBlueMarble = true;

               if (view.View.Hasshowbluemarble())
                  bShowBlueMarble = view.View.showbluemarble.Value;

               RenderableObject roBMNG = GetBMNG();
               if (roBMNG != null)
               {
                  if (bShowBlueMarble)
                     this.toolStripButtonBMNG.Image = global::Dapple.Properties.Resources.blue_marble_checked;
                  else
                     this.toolStripButtonBMNG.Image = global::Dapple.Properties.Resources.blue_marble_unchecked;
                  this.toolStripButtonBMNG.Checked = roBMNG.IsOn = bShowBlueMarble;
                  this.toolStripButtonBMNG.Enabled = true;
               }
               else
               {
                  this.toolStripButtonBMNG.Image = global::Dapple.Properties.Resources.blue_marble_unchecked;
                  this.toolStripButtonBMNG.Checked = false;
                  this.toolStripButtonBMNG.Enabled = false;
               }

               if (view.View.Hascameraorientation())
               {
                  cameraorientationType orient = view.View.cameraorientation;
                  m_worldWindow.DrawArgs.WorldCamera.SlerpPercentage = World.Settings.CameraSlerpInertia;
                  m_worldWindow.DrawArgs.WorldCamera.SetPosition(orient.lat.Value, orient.lon.Value, orient.heading.Value, orient.altitude.Value, orient.tilt.Value);
               }

               TreeView serverTree = GetNewServerTree();
               TriStateTreeView layerTree = GetNewLayerTree();

               LayerBuilderList activeList = new LayerBuilderList(this, layerTree, m_worldWindow);

               if (view.View.Hasactivelayers())
               {
                  for (i = 0; i < view.View.activelayers.datasetCount; i++)
                  {
                     datasetType dataset = view.View.activelayers.GetdatasetAt(i);

                     activeList.AddUsingUri(dataset.name.Value, dataset.uri.Value, dataset.Hasinvisible() ? !dataset.invisible.Value : true, (byte) dataset.opacity.Value, false);
                  }
               }

               if (view.View.Hasservers())
               {
                  for (i = 0; i < view.View.servers.builderentryCount; i++)
                  {
                     builderentryType entry = view.View.servers.GetbuilderentryAt(i);
                     LoadBuilderEntryIntoNode(entry, serverTree, layerTree, null, activeList);
                  }
               }
               InitializeTrees(serverTree, layerTree, activeList);
            }
            return true;
         }
         catch (Exception e)
         {
            MessageBox.Show(this, e.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
         }
      }

      #endregion

      #region MainForm Events
      private void toolStripMenuItemabout_Click(object sender, EventArgs e)
      {
         AboutDialog dlg = new AboutDialog();
         dlg.ShowDialog(this);
      }

      private void splitContainerLeftMain_SizeChanged(object sender, EventArgs e)
      {
         if (splitContainerLeftMain.SplitterWidth == 1)
            splitContainerLeftMain.SplitterDistance = toolStripServers.Height;
      }

      private void splitContainerLeft_SizeChanged(object sender, EventArgs e)
      {
         if (splitContainerLeft.SplitterWidth == 1)
            splitContainerLeft.SplitterDistance = toolStripOverview.Height;
      }

      private void MainForm_Load(object sender, EventArgs e)
      {
         this.treeViewServerBackup = null;
         this.toolStripButtonFilterText.Enabled = false;
         this.toolStripButtonClearFilter.Enabled = false;

         this.iconImageList.Images.Add("dap", global::Dapple.Properties.Resources.dap);
         this.iconImageList.Images.Add("dap_gray", global::Dapple.Properties.Resources.dap_gray);
         this.iconImageList.Images.Add("dap_database", global::Dapple.Properties.Resources.dap_database);
         this.iconImageList.Images.Add("dap_document", global::Dapple.Properties.Resources.dap_document);
         this.iconImageList.Images.Add("dap_grid", global::Dapple.Properties.Resources.dap_grid);
         this.iconImageList.Images.Add("dap_map", global::Dapple.Properties.Resources.dap_map);
         this.iconImageList.Images.Add("dap_picture", global::Dapple.Properties.Resources.dap_picture);
         this.iconImageList.Images.Add("dap_point", global::Dapple.Properties.Resources.dap_point);
         this.iconImageList.Images.Add("dap_spf", global::Dapple.Properties.Resources.dap_spf);
         this.iconImageList.Images.Add("dap_voxel", global::Dapple.Properties.Resources.dap_voxel);
         this.iconImageList.Images.Add("error", global::Dapple.Properties.Resources.error);
         this.iconImageList.Images.Add("folder", global::Dapple.Properties.Resources.folder);
         this.iconImageList.Images.Add("folder_gray", global::Dapple.Properties.Resources.folder_gray);
         this.iconImageList.Images.Add("layer", global::Dapple.Properties.Resources.layer);
         this.iconImageList.Images.Add("live", global::Dapple.Properties.Resources.live);
         this.iconImageList.Images.Add("tile", global::Dapple.Properties.Resources.tile);
         this.iconImageList.Images.Add("tile_gray", global::Dapple.Properties.Resources.tile_gray);
         this.iconImageList.Images.Add("georef_image", global::Dapple.Properties.Resources.georef_image);
         this.iconImageList.Images.Add("time", global::Dapple.Properties.Resources.time_icon);
         this.iconImageList.Images.Add("wms", global::Dapple.Properties.Resources.wms);
         this.iconImageList.Images.Add("wms_gray", global::Dapple.Properties.Resources.wms_gray);
         this.iconImageList.Images.Add("marble", global::Dapple.Properties.Resources.marble_icon);
         this.iconImageList.Images.Add("nasa", global::Dapple.Properties.Resources.nasa);
         this.iconImageList.Images.Add("usgs", global::Dapple.Properties.Resources.usgs);
         this.iconImageList.Images.Add("worldwind_central", global::Dapple.Properties.Resources.worldwind_central);

         m_worldWindow.IsRenderDisabled = false;

         this.toolStripProgressBar.Visible = false;
         this.toolStripStatusLabel1.Visible = false;
         this.toolStripStatusLabel1.Alignment = ToolStripItemAlignment.Right;
         this.toolStripStatusLabel2.Visible = false;
         this.toolStripStatusLabel2.Alignment = ToolStripItemAlignment.Right; 
         this.toolStripStatusLabel3.Visible = false;
         this.toolStripStatusLabel3.Alignment = ToolStripItemAlignment.Right; 
         this.toolStripStatusLabel4.Visible = false;
         this.toolStripStatusLabel4.Alignment = ToolStripItemAlignment.Right; 
         this.toolStripStatusLabel5.Visible = false;
         this.toolStripStatusLabel5.Alignment = ToolStripItemAlignment.Right; 
         this.toolStripStatusLabel6.Visible = false;
         this.toolStripStatusLabel6.Alignment = ToolStripItemAlignment.Right; 
         this.toolStripStatusSpin1.Visible = false;
         this.toolStripStatusSpin1.Alignment = ToolStripItemAlignment.Right; 
         this.toolStripStatusSpin2.Visible = false;
         this.toolStripStatusSpin2.Alignment = ToolStripItemAlignment.Right; 
         this.toolStripStatusSpin3.Visible = false;
         this.toolStripStatusSpin3.Alignment = ToolStripItemAlignment.Right; 
         this.toolStripStatusSpin4.Visible = false;
         this.toolStripStatusSpin4.Alignment = ToolStripItemAlignment.Right; 
         this.toolStripStatusSpin5.Visible = false;
         this.toolStripStatusSpin5.Alignment = ToolStripItemAlignment.Right; 
         this.toolStripStatusSpin6.Visible = false;
         this.m_worldWindow.Updated += new WorldWindow.UpdatedDelegate(OnUpdated);
      }

      void MainForm_Closing(object sender, CancelEventArgs e)
      {
         this.WindowState = FormWindowState.Minimized;

         SaveLastView();
         
         // Ensure cleanup
         for (int i = 0; i < m_worldWindow.CurrentWorld.RenderableObjects.Count; i++)
         {
            RenderableObject oRO = (RenderableObject)m_worldWindow.CurrentWorld.RenderableObjects.ChildObjects[i];
            oRO.IsOn = false;
            oRO.Dispose();
         }

         // Register the cache location to make it easy for uninstall to clear the cache for at least the current user
         try
         {
            RegistryKey keySW = Registry.CurrentUser.CreateSubKey("Software");
            RegistryKey keyDapple = keySW.CreateSubKey("Dapple");
            keyDapple.SetValue("CachePathForUninstall", WWSettingsCtl.CachePath);
         }
         catch
         {
         }

         // Save changed settings
         World.Settings.Save();
         WWSettingsCtl.WorldWindSettings.Save();

         m_worldWindow.Dispose();
      }

      private void MainForm_Deactivate(object sender, EventArgs e)
      {
         m_worldWindow.IsRenderDisabled = true;
      }

      private void MainForm_Activated(object sender, EventArgs e)
      {
         m_worldWindow.IsRenderDisabled = false;
      }

      #endregion

      #region Catalog Loaded/Failed Handlers

      delegate void InvokeLoadedCatalog(BuilderDirectory directory, TreeView serverTree, TriStateTreeView layerTree, LayerBuilderList activeList);
      delegate void InvokeFailedCatalog(BuilderDirectory directory, string message, TreeView serverTree, TriStateTreeView layerTree, LayerBuilderList activeList);

      void OnCatalogLoaded(BuilderDirectory directory, TreeView serverTree, TriStateTreeView layerTree, LayerBuilderList activeList)
      {
         lock (lockCatalogUpdates)
         {
            if (serverTree != null && !serverTree.IsDisposed && layerTree != null && !layerTree.IsDisposed)
               serverTree.BeginInvoke(new InvokeLoadedCatalog(LoadCatalog), new object[] { directory, serverTree, layerTree, activeList });
         }
      }

      void OnCatalogFailed(BuilderDirectory directory, string message, TreeView serverTree, TriStateTreeView layerTree, LayerBuilderList activeList)
      {
         lock (lockCatalogUpdates)
         {
            if (serverTree != null && !serverTree.IsDisposed && layerTree != null && !layerTree.IsDisposed)
               serverTree.BeginInvoke(new InvokeFailedCatalog(CatalogFailed), new object[] { directory, message, serverTree, layerTree, activeList });
         }
      }

      void CatalogFailed(BuilderDirectory directory, string message, TreeView serverTree, TriStateTreeView layerTree, LayerBuilderList activeList)
      {
         TreeNode treeNode = TreeUtils.FindNodeBFS(directory, serverTree.Nodes);
         if (treeNode != null)
         {
            treeNode.Text = "Failed: " + directory.Name + ": " + message;
            treeNode.SelectedImageIndex = treeNode.ImageIndex = ImageListIndex("error");
         }
      }

      void LoadCatalog(BuilderDirectory directory, TreeView serverTree, TriStateTreeView layerTree, LayerBuilderList activeList)
      {
         serverTree.BeginUpdate();

         TreeNode treeNode = TreeUtils.FindNodeBFS(directory, serverTree.Nodes);

         if (treeNode == null)
         {
            TreeNode treeParentNode = TreeUtils.FindNodeBFS(directory.Parent, serverTree.Nodes);

            if (treeParentNode == null)
               return;

            if (directory is DAPServerBuilder)
            {
               (treeParentNode.Tag as BuilderDirectory).SubList.Add(directory);
               treeNode = treeParentNode.Nodes.Add(directory.Name);
               treeNode.SelectedImageIndex = treeNode.ImageIndex = ImageListIndex("dap");
               treeNode.Tag = directory;
            }
            else if (directory is WMSServerBuilder)
            {
               (treeParentNode.Tag as BuilderDirectory).SubList.Add(directory);
               treeNode = treeParentNode.Nodes.Add(directory.Name);
               treeNode.SelectedImageIndex = treeNode.ImageIndex = ImageListIndex("wms");
               treeNode.Tag = directory;
            }
            else
            {
               treeNode = treeParentNode.Nodes.Add(directory.Name);
               treeNode.SelectedImageIndex = treeNode.ImageIndex = ImageListIndex("folder");
               treeNode.Tag = directory;
            }
         }
         else
         {
            treeNode.Text = directory.Name;
         }

         treeNode.SelectedImageIndex = treeNode.ImageIndex = (directory is DAPServerBuilder ? ImageListIndex("dap") : (directory is WMSServerBuilder ? ImageListIndex("wms") : ImageListIndex("layer")));
         ProcessCatalogDirectory(directory, treeNode);
         serverTree.Sort();
         serverTree.EndUpdate();

         layerTree.BeginUpdate();
         if (directory is DAPServerBuilder)
         {
            // Find provider in parents first
            IBuilder parentCatalog = directory.Parent;

            while (parentCatalog != null && !(parentCatalog is DAPCatalogBuilder))
               parentCatalog = parentCatalog.Parent;

            if (parentCatalog != null)
            {
               DAPServerBuilder dapserver = directory as DAPServerBuilder;
               DAPCatalogBuilder provider = parentCatalog as DAPCatalogBuilder;

               foreach (LayerBuilderContainer container in activeList)
               {
                  if (container.Uri.StartsWith(DAPQuadLayerBuilder.URLProtocolName) && dapserver.URL == DAPQuadLayerBuilder.ServerURLFromURI(container.Uri))
                  {
                     LayerBuilder builder = DAPQuadLayerBuilder.GetBuilderFromURI(container.Uri, provider, m_worldWindow, dapserver);
                     if (builder != null)
                        activeList.RefreshFromSource(container, builder);
                  }
               }
            }
         }
         else if (directory is WMSServerBuilder)
         {
            // Find provider in parents first
            IBuilder parentCatalog = directory.Parent;

            while (parentCatalog != null && !(parentCatalog is WMSCatalogBuilder))
               parentCatalog = parentCatalog.Parent;

            if (parentCatalog != null)
            {
               WMSServerBuilder wmsserver = directory as WMSServerBuilder;
               WMSCatalogBuilder provider = parentCatalog as WMSCatalogBuilder;
               
               foreach (LayerBuilderContainer container in activeList)
               {
                  if (container.Uri.StartsWith(WMSQuadLayerBuilder.URLProtocolName) && wmsserver.URL == WMSQuadLayerBuilder.ServerURLFromURI(container.Uri))
                  {
                     LayerBuilder builder = WMSQuadLayerBuilder.GetBuilderFromURI(container.Uri, provider, m_worldWindow, wmsserver);
                     if (builder != null)
                        activeList.RefreshFromSource(container, builder);
                  }
               }
            }
         }
         layerTree.EndUpdate();
      }

      #endregion

      #region Base Overrides
      protected override void WndProc(ref System.Windows.Forms.Message m)
      {
         if (m.Msg == OpenViewMessage)
         {
            try
            {
               Segment s = new Segment("Dapple.OpenView", SharedMemoryCreationFlag.Attach, 0);

               string[] strData = (string[])s.GetData();

               string strView = strData[0];
               string strGeoTiff = strData[1];
               this.lastView = strData[2];

               OpenView(strView, strGeoTiff.Length == 0);
               if (strGeoTiff.Length > 0)
                  AddGeoTiff(strGeoTiff, true);
            }
            catch
            {
            }
         }
         base.WndProc(ref m);
      }
      #endregion

      #region Download Progress

      private class ActiveDownload
      {
         public LayerBuilder builder; // null indicates Blue Marble
         public bool bOn;
         public bool bRead;
         public int iPos;
         public int iTotal;

         public static int Compare(ActiveDownload x, ActiveDownload y)
         {
            return (y.iTotal - y.iPos).CompareTo(x.iTotal - x.iPos);
         }
      }
      List<ActiveDownload> m_downloadList = new List<ActiveDownload>();
      int m_iPos = 0, m_iTotal = 0;
      bool m_bDownloading = false;

      void OnUpdated()
      {
         int iBuilderPos, iBuilderTotal;
         // Do the work in the update thread and just invoke to update the GUI

         m_iPos = 0;
         m_iTotal = 0;
         m_bDownloading = false;
         List<ActiveDownload> currentList = new List<ActiveDownload>();
         RenderableObject roBMNG = GetActiveBMNG();

         if (roBMNG != null && roBMNG.IsOn && ((QuadTileSet)((RenderableObjectList)roBMNG).ChildObjects[1]).bIsDownloading(out iBuilderPos, out iBuilderTotal))
         {
            m_bDownloading = true;
            m_iPos += iBuilderPos;
            m_iTotal += iBuilderTotal;
            ActiveDownload dl = new ActiveDownload();
            dl.builder = null;
            dl.iPos = iBuilderPos;
            dl.iTotal = iBuilderTotal;
            dl.bOn = true;
            dl.bRead = false;
            currentList.Add(dl);
         }
         
         foreach (LayerBuilderContainer container in m_ActiveLayers)
         {
            if (container.Builder != null && container.Builder.bIsDownloading(out iBuilderPos, out iBuilderTotal))
            {
               m_bDownloading = true;
               m_iPos += iBuilderPos;
               m_iTotal += iBuilderTotal;
               ActiveDownload dl = new ActiveDownload();
               dl.builder = container.Builder;
               dl.iPos = iBuilderPos;
               dl.iTotal = iBuilderTotal;
               dl.bOn = true;
               dl.bRead = false;
               currentList.Add(dl);
            }
         }


         if (m_bDownloading)
         {
            // Add new or update information for previous downloads
            for (int i = 0; i < currentList.Count; i++)
            {
               int iFound = -1;
               for (int j = 0; j < m_downloadList.Count; j++)
               {
                  if (currentList[i].builder == m_downloadList[j].builder)
                  {
                     iFound = j;
                     break;
                  }
               }
               if (iFound != -1)
               {
                  if (m_downloadList[iFound].bRead)
                  {
                     // This for simple flashing led animation
                     currentList[i].bOn = !m_downloadList[iFound].bOn;
                     currentList[i].bRead = false;
                  }
               }
            }

            // Sorting to put largest dl first, also causes some animation which indicates "busyness"
            // currentList.Sort(ActiveDownload.Compare); 
            m_downloadList = currentList;
         }
         else
            m_downloadList.Clear();

         this.Invoke(new WorldWindow.UpdatedDelegate(Updated));
      }

      private void Updated()
      {
         if (!m_bDownloading)
         {
            if (this.toolStripProgressBar.Visible)
               this.toolStripProgressBar.Value = 100;
            this.toolStripProgressBar.Visible = false;
            this.toolStripStatusLabel1.Visible = false;
            this.toolStripStatusSpin1.Visible = false;
            this.toolStripStatusLabel2.Visible = false;
            this.toolStripStatusSpin2.Visible = false;
            this.toolStripStatusLabel3.Visible = false;
            this.toolStripStatusSpin3.Visible = false;
            this.toolStripStatusLabel4.Visible = false;
            this.toolStripStatusSpin4.Visible = false;
            this.toolStripStatusLabel5.Visible = false;
            this.toolStripStatusSpin5.Visible = false;
            this.toolStripStatusLabel6.Visible = false;
            this.toolStripStatusSpin6.Visible = false;
         }
         else
         {
            this.toolStripProgressBar.Visible = true;
            this.toolStripProgressBar.Value = m_iTotal > 0 ? Math.Max(5, Math.Min(100 * m_iPos / m_iTotal, 100)) : 0;
            this.toolStripProgressBar.ToolTipText = "";// (m_iPos / 1024).ToString() + "KB from an estimated " + (m_iTotal / 1024).ToString() + "KB completed.";
            if (m_downloadList.Count >= 6)
            {
               this.toolStripStatusLabel6.Text = "";
               if (m_downloadList[5].builder == null)
               {
                  this.toolStripStatusLabel6.ToolTipText = "Base Image";
                  this.toolStripStatusLabel6.Image = this.iconImageList.Images["marble"];
               }
               else
               {
                  this.toolStripStatusLabel6.ToolTipText = m_downloadList[5].builder.Name;
                  this.toolStripStatusLabel6.Image = this.iconImageList.Images[m_downloadList[5].builder.LogoKey];
               }
               this.toolStripStatusLabel6.Visible = true;
               this.toolStripStatusSpin6.Text = "";
               this.toolStripStatusSpin6.Image = m_downloadList[5].bOn ? global::Dapple.Properties.Resources.led_on : global::Dapple.Properties.Resources.led_off; //GetSpinImage(m_downloadList[5].iSpin);
               this.toolStripStatusSpin6.Visible = true;
               m_downloadList[5].bRead = true;
            }
            else
            {
               this.toolStripStatusLabel6.Visible = false;
               this.toolStripStatusSpin6.Visible = false;
            }

            if (m_downloadList.Count >= 5)
            {
               this.toolStripStatusLabel5.Text = "";
               if (m_downloadList[4].builder == null)
               {
                  this.toolStripStatusLabel5.ToolTipText = "Base Image";
                  this.toolStripStatusLabel5.Image = this.iconImageList.Images["marble"];
               }
               else
               {
                  this.toolStripStatusLabel5.ToolTipText = m_downloadList[4].builder.Name;
                  this.toolStripStatusLabel5.Image = this.iconImageList.Images[m_downloadList[4].builder.LogoKey];
               }
               this.toolStripStatusLabel5.Visible = true;
               this.toolStripStatusSpin5.Text = "";
               this.toolStripStatusSpin5.Image = m_downloadList[4].bOn ? global::Dapple.Properties.Resources.led_on : global::Dapple.Properties.Resources.led_off; //GetSpinImage(m_downloadList[4].iSpin);
               this.toolStripStatusSpin5.Visible = true;
               m_downloadList[4].bRead = true;
            }
            else
            {
               this.toolStripStatusLabel5.Visible = false;
               this.toolStripStatusSpin5.Visible = false;
            }

            if (m_downloadList.Count >= 4)
            {
               this.toolStripStatusLabel4.Text = "";
               if (m_downloadList[3].builder == null)
               {
                  this.toolStripStatusLabel4.ToolTipText = "Base Image";
                  this.toolStripStatusLabel4.Image = this.iconImageList.Images["marble"];
               }
               else
               {
                  this.toolStripStatusLabel4.ToolTipText = m_downloadList[3].builder.Name;
                  this.toolStripStatusLabel4.Image = this.iconImageList.Images[m_downloadList[3].builder.LogoKey];
               }
               this.toolStripStatusLabel4.Visible = true;
               this.toolStripStatusSpin4.Text = "";
               this.toolStripStatusSpin4.Image = m_downloadList[3].bOn ? global::Dapple.Properties.Resources.led_on : global::Dapple.Properties.Resources.led_off; //GetSpinImage(m_downloadList[3].iSpin);
               this.toolStripStatusSpin4.Visible = true;
               m_downloadList[3].bRead = true;
            }
            else
            {
               this.toolStripStatusLabel4.Visible = false;
               this.toolStripStatusSpin4.Visible = false;
            }

            if (m_downloadList.Count >= 3)
            {
               this.toolStripStatusLabel3.Text = "";
               if (m_downloadList[2].builder == null)
               {
                  this.toolStripStatusLabel3.ToolTipText = "Base Image";
                  this.toolStripStatusLabel3.Image = this.iconImageList.Images["marble"];
               }
               else
               {
                  this.toolStripStatusLabel3.ToolTipText = m_downloadList[2].builder.Name;
                  this.toolStripStatusLabel3.Image = this.iconImageList.Images[m_downloadList[2].builder.LogoKey];
               }
               this.toolStripStatusLabel3.Visible = true;
               this.toolStripStatusSpin3.Text = "";
               this.toolStripStatusSpin3.Image = m_downloadList[2].bOn ? global::Dapple.Properties.Resources.led_on : global::Dapple.Properties.Resources.led_off; //GetSpinImage(m_downloadList[2].iSpin);
               this.toolStripStatusSpin3.Visible = true;
               m_downloadList[2].bRead = true;
            }
            else
            {
               this.toolStripStatusLabel3.Visible = false;
               this.toolStripStatusSpin3.Visible = false;
            }

            if (m_downloadList.Count >= 2)
            {
               this.toolStripStatusLabel2.Text = "";
               if (m_downloadList[1].builder == null)
               {
                  this.toolStripStatusLabel2.ToolTipText = "Base Image";
                  this.toolStripStatusLabel2.Image = this.iconImageList.Images["marble"];
               }
               else
               {
                  this.toolStripStatusLabel2.ToolTipText = m_downloadList[1].builder.Name;
                  this.toolStripStatusLabel2.Image = this.iconImageList.Images[m_downloadList[1].builder.LogoKey];
               }
               this.toolStripStatusLabel2.Visible = true;
               this.toolStripStatusSpin2.Text = "";
               this.toolStripStatusSpin2.Image = m_downloadList[1].bOn ? global::Dapple.Properties.Resources.led_on : global::Dapple.Properties.Resources.led_off; //GetSpinImage(m_downloadList[1].iSpin);
               this.toolStripStatusSpin2.Visible = true;
               m_downloadList[1].bRead = true;
            }
            else
            {
               this.toolStripStatusLabel2.Visible = false;
               this.toolStripStatusSpin2.Visible = false;
            }

            if (m_downloadList.Count >= 1)
            {
               this.toolStripStatusLabel1.Text = "";
               if (m_downloadList[0].builder == null)
               {
                  this.toolStripStatusLabel1.ToolTipText = "Base Image";
                  this.toolStripStatusLabel1.Image = this.iconImageList.Images["marble"];
               }
               else
               {
                  this.toolStripStatusLabel1.ToolTipText = m_downloadList[0].builder.Name;
                  this.toolStripStatusLabel1.Image = this.iconImageList.Images[m_downloadList[0].builder.LogoKey];
               }
               this.toolStripStatusLabel1.Visible = true;
               this.toolStripStatusSpin1.Text = "";
               this.toolStripStatusSpin1.Image = m_downloadList[0].bOn ? global::Dapple.Properties.Resources.led_on : global::Dapple.Properties.Resources.led_off; //GetSpinImage(m_downloadList[0].iSpin);
               this.toolStripStatusSpin1.Visible = true;
               m_downloadList[0].bRead = true;
            }
            else
            {
               this.toolStripStatusLabel1.Visible = false;
               this.toolStripStatusSpin1.Visible = false;
            }
         }
      }
      #endregion

      #region Spatial and Text Filtering

      private string m_strLastSearchText = "";

      public static void CountTreeNodeDatasets(TreeNodeCollection col, ref int iDatasets)
      {
         foreach (TreeNode treeNode in col)
         {
            if (!(treeNode.Tag is BuilderDirectory))
               iDatasets++;
            else
               CountTreeNodeDatasets(treeNode.Nodes, ref iDatasets);
         }
      }

      public void FilterTreeNodes(TreeNode node, TreeNodeCollection col, GeographicBoundingBox filterExtents, bool bIntersect, string filterText)
      {
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
            else if (treeNode.Tag is DAPServerBuilder && filterExtents != null)
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

                  lock (lockCatalogUpdates)
                  {
                     Geosoft.GX.DAPGetData.Server dapServer = new Geosoft.GX.DAPGetData.Server(serverBuilder.Server.Url, WWSettingsCtl.CachePath);
                     BuilderDirectory dapDir = dapBuilder.AddDapServer(dapServer, new Geosoft.Dap.Common.BoundingBox(filterExtents.East, filterExtents.North, filterExtents.West, filterExtents.South), serverBuilder.Parent);
                     dapBuilder.SubList.Add(dapDir);

                     treeNode.Nodes.Clear();
                     treeNode.Text = "Loading: " + serverBuilder.Server.Url;
                     treeNode.SelectedImageIndex = treeNode.ImageIndex = ImageListIndex("time");
                     treeNode.Tag = dapDir;
                  }
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
                  if (node.ImageIndex == ImageListIndex("dap"))
                     node.ImageIndex = node.SelectedImageIndex = ImageListIndex("dap_gray");
                  else if (node.ImageIndex == ImageListIndex("wms"))
                     node.ImageIndex = node.SelectedImageIndex = ImageListIndex("wms_gray");
                  else if (node.ImageIndex == ImageListIndex("tile"))
                     node.ImageIndex = node.SelectedImageIndex = ImageListIndex("tile_gray");
               }
            }
         }
      }

      private void FilterServersToView(bool bIntersect)
      {
         if (this.treeViewServerBackup == null)
            this.treeViewServerBackup = this.treeViewServers;

         TreeView filterTree = GetNewServerTree();

         // Make exact copy of tree first
         foreach (TreeNode treeNode in this.treeViewServerBackup.Nodes)
            filterTree.Nodes.Add((TreeNode)treeNode.Clone());

         lock (lockCatalogUpdates)
         {
            FilterTreeNodes(null, filterTree.Nodes, GeographicBoundingBox.FromQuad(m_worldWindow.GetViewBox()), bIntersect, "");
            ReplaceServerTree(filterTree);
            this.toolStripButtonClearFilter.Enabled = true;
            m_strLastSearchText = "";
            this.toolStripFilterText.ForeColor = SystemColors.GrayText;
         }
      }

      private void toolStripButtonFilterSpatial_Click(object sender, EventArgs e)
      {
         FilterServersToView(true);
      }

      private void toolStripButtonFilterSpatialInside_Click(object sender, EventArgs e)
      {
         FilterServersToView(false);
      }

      private void toolStripButtonClearFilter_Click(object sender, EventArgs e)
      {
         if (this.treeViewServerBackup != null)
            ReplaceServerTree(this.treeViewServerBackup);
         this.treeViewServerBackup = null;
         this.toolStripButtonClearFilter.Enabled = false;
         m_strLastSearchText = "";
         this.toolStripFilterText.ForeColor = SystemColors.GrayText;
      }

      private void toolStripFilterText_KeyDown(object sender, KeyEventArgs e)
      {
         if (e.KeyCode == Keys.Return)
            toolStripButtonFilterText_Click(sender, null);
      }

      private void toolStripButtonFilterText_Click(object sender, EventArgs e)
      {
         TreeView filterTree = GetNewServerTree();

         // Are we filtering the results or the original tree
         if (this.treeViewServerBackup == null)
         {
            this.treeViewServerBackup = this.treeViewServers;
            foreach (TreeNode treeNode in this.treeViewServerBackup.Nodes)
               filterTree.Nodes.Add((TreeNode)treeNode.Clone());
         }
         else
         {
            foreach (TreeNode treeNode in this.treeViewServers.Nodes)
               filterTree.Nodes.Add((TreeNode)treeNode.Clone());
         }

         lock (lockCatalogUpdates)
         {
            FilterTreeNodes(null, filterTree.Nodes, null, false, this.toolStripFilterText.Text);
            ReplaceServerTree(filterTree);
            this.toolStripButtonClearFilter.Enabled = true;
            m_strLastSearchText = this.toolStripFilterText.Text;
            this.toolStripFilterText.ForeColor = SystemColors.WindowText;
         }
      }

      private void toolStripFilterText_TextChanged(object sender, EventArgs e)
      {
         if (this.toolStripFilterText.Text.Length > 0)
            this.toolStripButtonFilterText.Enabled = true;
         else
            this.toolStripButtonFilterText.Enabled = false;
         if (this.toolStripFilterText.Text == m_strLastSearchText)
            this.toolStripFilterText.ForeColor = SystemColors.WindowText;
         else
            this.toolStripFilterText.ForeColor = SystemColors.GrayText;
      }

      #endregion

      #region Blue Marble

      private void toolStripButtonBMNG_Click(object sender, EventArgs e)
      {
         this.toolStripButtonBMNG.Checked = !toolStripButtonBMNG.Checked;

         if (this.toolStripButtonBMNG.Checked)
            this.toolStripButtonBMNG.Image = global::Dapple.Properties.Resources.blue_marble_checked;
         else
            this.toolStripButtonBMNG.Image = global::Dapple.Properties.Resources.blue_marble_unchecked;
                  

         RenderableObject roBMNG = GetBMNG();
         if (roBMNG != null)
            roBMNG.IsOn = this.toolStripButtonBMNG.Checked;
      }

      private RenderableObject GetBMNG()
      {
         for (int i = 0; i < m_worldWindow.CurrentWorld.RenderableObjects.Count; i++)
         {
            if (((RenderableObject)m_worldWindow.CurrentWorld.RenderableObjects.ChildObjects[i]).Name == "4 - The Blue Marble")
               return m_worldWindow.CurrentWorld.RenderableObjects.ChildObjects[i] as RenderableObject;
         }
         return null;
      }

      private RenderableObject GetActiveBMNG()
      {
         RenderableObject roBMNG = GetBMNG();
         if (roBMNG != null && roBMNG.IsOn)
            return GetActiveBMNG(roBMNG);
         else
            return null;
      }

      private RenderableObjectList GetActiveBMNG(RenderableObject roBMNG)
      {
         if (roBMNG is RenderableObjectList)
         {
            if ((roBMNG as RenderableObjectList).ChildObjects.Count == 2 && roBMNG.isInitialized)
               return roBMNG as RenderableObjectList;
            for (int i = 0; i < (roBMNG as RenderableObjectList).Count; i++)
            {
               RenderableObject ro = (RenderableObject)(roBMNG as RenderableObjectList).ChildObjects[i];
               if (ro is RenderableObjectList)
               {
                  if ((ro as RenderableObjectList).ChildObjects.Count != 2)
                  {
                     for (int j = 0; j < (ro as RenderableObjectList).Count; j++)
                     {
                        RenderableObjectList roRet = GetActiveBMNG((RenderableObject)(ro as RenderableObjectList).ChildObjects[j]);
                        if (roRet != null)
                           return roRet;
                     }
                  }
                  else if (ro.isInitialized)
                     return ro as RenderableObjectList;
               }
            }
         }
         return null;
      }

      #endregion

      private void toolStripNavButton_MouseRemoveCapture(object sender, MouseEventArgs e)
      {

      }

#if !DEBUG
      public void OnThreadException(object o, ThreadExceptionEventArgs e)
      {
         Utility.AbortUtility.Abort(e.Exception, Thread.CurrentThread);
      }
#endif
   }

   // Create a node sorter that implements the IComparer interface that puts directories in front of layer builders.
   public class TreeNodeSorter : System.Collections.IComparer
   {
      public int Compare(object x, object y)
      {
         TreeNode tx = x as TreeNode;
         TreeNode ty = y as TreeNode;

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
