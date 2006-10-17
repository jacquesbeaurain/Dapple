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
      public const string DefaultView = "default" + ViewExt;
      public const string ViewFileDescr = "Dapple View"; 
      public const string WebsiteUrl = "http://dapple.geosoft.com/";
      public const string VersionFile = "version.txt";
      public const string LicenseWebsiteUrl = "http://dapple.geosoft.com/license.asp";
      public const string CreditsWebsiteUrl = "http://dapple.geosoft.com/credits.asp";
      public const string ReleaseNotesWebsiteUrl = "http://dapple.geosoft.com/releasenotes.asp";
      public const string WebsiteHelpUrl = "http://dapple.geosoft.com/help/";
      public const string WebsiteForumsHelpUrl = "https://dappleforums.geosoft.com/";
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

      private ServerTree tvServers;
      private TriStateTreeView tvLayers;
      //private List<TreeNode> triStateTreeViewLayerNodes = new List<TreeNode>();
      //private TreeNode lastLayerNode, firstLayerNode;
      private WorldWindow worldWindow = new WorldWindow();
      private NASA.Plugins.BMNG bmngPlugin;
      private WorldWind.OverviewControl overviewCtl;
      private string openView = "";
      private string openGeoTiff = "";
      private string openGeoTiffName = "";
      private bool openGeoTiffTmp = false;
      private string lastView = "";
      private string metaviewerDir = "";

      private bool rightmouse_context = false;
      private bool checked_context = false;

      private LayerBuilderList activeLayers;

      Geosoft.GX.DAPGetData.GetDapError dapErrors;

      Murris.Plugins.Compass compassPlugin;

      private int iLastTransparency = 255; // It breaks the message loop to access the actual property during paint
      private DappleToolStripRenderer toolStripRenderer;
      /* private int iServerPanelLastMinSize, iServerPanelLastPos;
      private int iLayerPanelLastMinSize, iLayerPanelLastPos;
      private int iOverviewPanelLastMinSize, iOverviewPanelLastPos;
      
      */
      #endregion

      #region Properties

      private LayerBuilderContainer layerBuilder = null;

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

               if (!this.activeLayers.IsTop(this.layerBuilder))
               {
                  this.toolStripButtonTop.Enabled = true;
                  this.toolStripButtonUp.Enabled = true;
                  this.toolStripMenuItemputOnTop.Enabled = true;
                  this.toolStripMenuItemmoveUp.Enabled = true;
               }

               if (!this.activeLayers.IsBottom(this.layerBuilder))
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

      public WorldWindow WorldWindowControl
      {
         get
         {
            return this.worldWindow;
         }
      }

      public WorldWind.WorldWindSettingsComponent Settings
      {
         get
         {
            return this.WWSettingsCtl;
         }
      }

      #endregion

      #region Constructor

      public MainForm(string strView, string strGeoTiff, string strGeotiffName, bool bGeotiffTmp, string strLastView)
      {
         if (String.Compare(Path.GetExtension(strView), ViewExt, true) == 0 && File.Exists(strView))
            this.openView = strView;
         this.openGeoTiff = strGeoTiff;
         this.openGeoTiffName = strGeotiffName;
         this.openGeoTiffTmp = bGeotiffTmp;
         this.lastView = strLastView;

         string executablePath = Path.GetDirectoryName(Application.ExecutablePath);

         InitializeComponent();
         this.SuspendLayout();
         this.Icon = new System.Drawing.Icon(@"app.ico");
         this.toolStripRenderer = new DappleToolStripRenderer();
         toolStripServers.Renderer = this.toolStripRenderer;
         toolStripLayerLabel.Renderer = this.toolStripRenderer;
         toolStripOverview.Renderer = this.toolStripRenderer;

         // Copy/Update any configuration files and other files if needed now
         string strConfigDir = Path.Combine(UserPath, "Config");
         Directory.CreateDirectory(strConfigDir);
         Directory.CreateDirectory(Path.Combine(UserPath, "Cache"));
         this.metaviewerDir = Path.Combine(UserPath, "Metadata");
         Directory.CreateDirectory(this.metaviewerDir);
         string[] cfgFiles = Directory.GetFiles(Path.Combine(executablePath, "Config"), "*.xml");
         foreach (string strCfgFile in cfgFiles)
         {
            string strUserCfg = Path.Combine(strConfigDir, Path.GetFileName(strCfgFile));
            if (!File.Exists(strUserCfg))
               File.Copy(strCfgFile, strUserCfg);
         }
         string[] metaFiles = Directory.GetFiles(Path.Combine(Path.Combine(executablePath, "Data"), "MetaViewer"), "*.*");
         foreach (string strMetaFile in metaFiles)
         {
            string strUserMeta = Path.Combine(this.metaviewerDir, Path.GetFileName(strMetaFile));
            File.Copy(strMetaFile, strUserMeta, true);
         }

         WWSettingsCtl.SettingsPath = UserPath;
         
         #region Plugin + World Init.

         // register handler for extension 

         Registry.SetValue(Registry.CurrentUser + "\\Software\\Classes\\" + ViewExt, "" , "Dapple View");
         Registry.SetValue(Registry.CurrentUser + "\\Software\\Classes\\Dapple View", "", "Dapple View");
         Registry.SetValue(Registry.CurrentUser + "\\Software\\Classes\\Dapple View\\Shell\\Open", "", "Open &" + ViewFileDescr);
         Registry.SetValue(Registry.CurrentUser + "\\Software\\Classes\\Dapple View\\Shell\\Open\\Command", "", "\"" + executablePath + "\" \"%1\"");
         Registry.SetValue(Registry.CurrentUser + "\\Software\\Classes\\Dapple View\\DefaultIcon", "", Path.Combine(executablePath, "app.ico"));

         this.worldWindow.WorldWindSettingsComponent = WWSettingsCtl;
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
            if (Directory.Exists(WWSettingsCtl.CachePath))
               Utility.FileSystem.DeleteFolderGUI(this, WWSettingsCtl.CachePath);
            WWSettingsCtl.CachePath = WWSettingsCtl.NewCachePath;
            WWSettingsCtl.NewCachePath = "";
         }

         this.dapErrors = new Geosoft.GX.DAPGetData.GetDapError(Path.Combine(WWSettingsCtl.CachePath, "DapErrors.log"));

         WorldWind.Terrain.TerrainTileService terrainTileService = new WorldWind.Terrain.TerrainTileService("http://worldwind25.arc.nasa.gov/tile/tile.aspx", "100", 20, 150, "bil", 8, Path.Combine(WWSettingsCtl.CachePath, "Earth\\TerrainAccessor\\SRTM"));
         WorldWind.Terrain.TerrainAccessor terrainAccessor = new WorldWind.Terrain.NltTerrainAccessor("Earth", -180, -90, 180, 90, terrainTileService, null);

         WorldWind.World world = new WorldWind.World("Earth",
             new Vector3d(0, 0, 0), new Quaternion4d(0, 0, 0, 0),
             (float)6378137,
             System.IO.Path.Combine(this.worldWindow.Cache.CacheDirectory, "Earth"),
             terrainAccessor);

         this.worldWindow.CurrentWorld = world;

         NASA.Plugins.BmngLoader bmng = new NASA.Plugins.BmngLoader(WWSettingsCtl.WorldWindDirectory);
         Atmosphere.Plugin.Atmosphere atmo = new Atmosphere.Plugin.Atmosphere(WWSettingsCtl.WorldWindDirectory);
         Stars3D.Plugin.Stars3D stars = new Stars3D.Plugin.Stars3D(WWSettingsCtl.WorldWindDirectory);
         this.compassPlugin = new Murris.Plugins.Compass();

         this.worldWindow.AddPlugin(bmng, executablePath);
         this.worldWindow.AddPlugin(atmo, executablePath);
         this.worldWindow.AddPlugin(stars, executablePath);
         this.worldWindow.AddPlugin(this.compassPlugin, executablePath);

         this.bmngPlugin = bmng.BMNGForm;

         this.splitContainerMain.Panel2.Controls.Add(this.worldWindow);
         this.worldWindow.Dock = DockStyle.Fill;

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

         this.toolStripMenuItemcompass.Checked = World.Settings.ShowCompass;
         this.toolStripMenuItemtileActivity.Checked = World.Settings.ShowDownloadRectangles;
         this.toolStripCrossHairs.Checked = World.Settings.ShowCrosshairs;
         this.toolStripMenuItemshowPosition.Checked = World.Settings.ShowPosition;
         this.toolStripMenuItemshowGridLines.Checked = World.Settings.ShowLatLonLines;

         this.toolStripMenuItemAskAtStartup.Checked = this.WWSettingsCtl.AskLastViewAtStartup;
         if (!this.WWSettingsCtl.AskLastViewAtStartup)
            this.toolStripMenuItemLoadLastView.Checked = this.WWSettingsCtl.LastViewAtStartup;


         #region OverviewPanel

         int i;
         for (i = 0; i < this.worldWindow.CurrentWorld.RenderableObjects.Count; i++)
         {
            if (((RenderableObject)this.worldWindow.CurrentWorld.RenderableObjects.ChildObjects[i]).Name == "4 - The Blue Marble")
               break;
         }

         this.overviewCtl = new OverviewControl(WWSettingsCtl.DataPath + @"\Earth\BmngBathy\world.topo.bathy.200407.jpg", this.worldWindow, panelOverview);
         this.overviewCtl.Dock = DockStyle.Fill;
         this.panelOverview.Controls.Add(this.overviewCtl);

         #endregion


         this.worldWindow.MouseEnter += new EventHandler(this.worldWindow_MouseEnter);
         this.worldWindow.MouseLeave += new EventHandler(this.worldWindow_MouseLeave);

         this.worldWindow.ClearDevice();


         #region Tree Views

         this.tvLayers = new TriStateTreeView();

         this.tvLayers.ContextMenuStrip = this.contextMenuStripLayers;
         this.tvLayers.Dock = System.Windows.Forms.DockStyle.Fill;
         this.tvLayers.HideSelection = false;
         this.tvLayers.ImageIndex = 0;
         this.tvLayers.Location = new System.Drawing.Point(0, 0);
         this.tvLayers.Name = "tvLayers";
         this.tvLayers.SelectedImageIndex = 0;
         this.tvLayers.ShowLines = false;
         this.tvLayers.ShowNodeToolTips = true;
         this.tvLayers.ShowRootLines = false;
         this.tvLayers.Size = new System.Drawing.Size(245, 182);
         this.tvLayers.TabIndex = 1;
         this.tvLayers.Scrollable = true;
         //tree.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.tvLayers_BeforeSelect);
         this.tvLayers.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvLayers_AfterSelect);
         this.tvLayers.KeyUp += new System.Windows.Forms.KeyEventHandler(this.tvLayers_KeyUp);
         this.tvLayers.TreeNodeChecked += new Geosoft.DotNetTools.TreeNodeCheckedEventHandler(this.tvLayers_TreeNodeChecked);
         this.tvLayers.MouseDown += new System.Windows.Forms.MouseEventHandler(this.tvLayers_MouseDown);

         this.activeLayers = new LayerBuilderList(this, this.tvLayers, this.worldWindow);

         this.tvServers = new ServerTree(WWSettingsCtl.CachePath, this, this.tvLayers, this.activeLayers);
         this.tvServers.RMBContextMenuStrip = this.contextMenuStripServers;
         this.tvServers.Dock = System.Windows.Forms.DockStyle.Fill;
         this.tvServers.ImageIndex = 0;
         this.tvServers.Location = new System.Drawing.Point(0, 0);
         this.tvServers.Name = "treeViewServers";
         this.tvServers.SelectedImageIndex = 0;
         this.tvServers.Size = new System.Drawing.Size(245, 240);
         this.tvServers.TabIndex = 0;
         
         this.tvLayers.ImageList = this.tvServers.ImageList;

         this.panelServer.SuspendLayout();
         this.panelLayers.SuspendLayout();

         this.panelServer.Controls.Add(this.tvServers);
         this.panelLayers.Controls.Add(this.tvLayers);

         this.panelServer.ResumeLayout(false);
         this.panelLayers.ResumeLayout(false);
         this.ResumeLayout(false);
         this.panelServer.PerformLayout();
         this.panelLayers.PerformLayout();


         #endregion


         this.PerformLayout();
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
         UpdateDialog dlg = new UpdateDialog(strVersion);

         if (dlg.ShowDialog(this) == DialogResult.Yes)
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
         WebDownload download = new WebDownload(WebsiteUrl + VersionFile, false);
         download.DownloadType = DownloadType.Unspecified;
         download.CompleteCallback += new DownloadCompleteHandler(UpdateDownloadComplete);
         download.BackgroundDownloadMemory();
      }
      #endregion

      #region World Window Events

      void worldWindow_MouseLeave(object sender, EventArgs e)
      {
         splitContainerMain.Panel1.Select();
      }

      void worldWindow_MouseEnter(object sender, EventArgs e)
      {
         this.worldWindow.Select();
      }

      #endregion

      #region Context menus and buttons

      #region Opening 

      private void contextMenuStripLayers_Opening(object sender, CancelEventArgs e)
      {
         e.Cancel = false;
         if (LayerBuilderItem == null || this.checked_context)
         {
            this.checked_context = false;
            e.Cancel = true;
         }
      }

      private void contextMenuStripServers_Opening(object sender, CancelEventArgs e)
      {
         e.Cancel = false;
         if (this.tvServers.SelectedNode == null)
         {
            e.Cancel = true;
            return;
         }

         this.toolStripMenuItemAddLayer.Visible = false;
         this.toolStripMenuItemaddServer.Text = "Add Server";
         this.toolStripMenuItemaddServer.Visible = false;
         this.toolStripSeparatorServerAdd.Visible = false;
         this.toolStripMenuItemgoToServer.Visible = false;
         this.toolStripSeparatorServerGoto.Visible = false;
         this.toolStripMenuItemremoveServer.Visible = false;
         this.toolStripSeparatorServerRemove.Visible = false;
         this.toolStripMenuItemviewMetadataServer.Visible = false;
         this.toolStripMenuItemServerLegend.Visible = false;
         this.toolStripMenuItemviewMetadataServer.Enabled = false;
         this.toolStripMenuItemServerLegend.Enabled = false;
         this.toolStripMenuItempropertiesServer.Visible = false;
         this.toolStripSeparatorRefreshCatalog.Visible = false;
         this.toolStripMenuItemRefreshCatalog.Visible = false;

         if (this.tvServers.SelectedNode.Nodes == this.tvServers.DAPRootNodes)
         {
            this.toolStripMenuItemaddServer.Text = "Add DAP Server";
            this.toolStripMenuItemaddServer.Visible = true;
         }
         else if (this.tvServers.SelectedNode.Nodes == this.tvServers.WMSRootNodes)
         {
            this.toolStripMenuItemaddServer.Text = "Add WMS Server";
            this.toolStripMenuItemaddServer.Visible = true;
         }
         else if (this.tvServers.SelectedNode.Nodes == this.tvServers.TileRootNodes)
            e.Cancel = true;
         else if (this.tvServers.SelectedNode.Tag is Geosoft.GX.DAPGetData.Server)
         {
            this.toolStripMenuItemRefreshCatalog.Visible = true;
            this.toolStripMenuItemremoveServer.Visible = true;
            this.toolStripSeparatorServerRemove.Visible = true;
         }
         else if (this.tvServers.SelectedNode.Tag != null)
         {
            bool bCancel = true;
            IBuilder builder = null;
            Geosoft.Dap.Common.DataSet dapDataset = null;

            if (this.tvServers.SelectedNode.Tag is IBuilder)
               builder = this.tvServers.SelectedNode.Tag as IBuilder;
            if (this.tvServers.SelectedNode.Tag is Geosoft.Dap.Common.DataSet)
               dapDataset = this.tvServers.SelectedDAPDataset;

            if (builder is LayerBuilder || dapDataset != null)
            {
               this.toolStripMenuItemAddLayer.Visible = true;
               this.toolStripMenuItemgoToServer.Visible = true;
               this.toolStripSeparatorServerGoto.Visible = true;
               bCancel = false;
            }
            else
            {
               this.toolStripMenuItemAddLayer.Visible = false;
               this.toolStripMenuItemgoToServer.Visible = false;
               this.toolStripSeparatorServerGoto.Visible = false;
            }

            if (!this.toolStripMenuItemaddServer.Visible && !this.toolStripMenuItemAddLayer.Visible)
               this.toolStripSeparatorServerAdd.Visible = false;
            else
            {
               this.toolStripSeparatorServerAdd.Visible = true;
               bCancel = false;
            }

            if (dapDataset != null || builder == null || builder is WMSQuadLayerBuilder || builder is VEQuadLayerBuilder || builder is QuadLayerBuilder || (builder is BuilderDirectory && !(builder as BuilderDirectory).Removable))
            {
               this.toolStripMenuItemremoveServer.Visible = false;
               this.toolStripSeparatorServerRemove.Visible = false;
            }
            else
            {
               this.toolStripMenuItemremoveServer.Visible = true;
               this.toolStripSeparatorServerRemove.Visible = true;
               bCancel = false;
            }

            if (builder is WMSServerBuilder)
            {
               this.toolStripSeparatorRefreshCatalog.Visible = true;
               this.toolStripMenuItemRefreshCatalog.Visible = true;
               bCancel = false;
            }

            if (builder != null)
            {
               this.toolStripMenuItemviewMetadataServer.Visible = builder.SupportsMetaData || builder is LayerBuilder;
               this.toolStripMenuItemServerLegend.Visible = builder is LayerBuilder;
               this.toolStripMenuItemviewMetadataServer.Enabled = builder.SupportsMetaData;
               this.toolStripMenuItemServerLegend.Enabled = (builder is LayerBuilder) && (builder as LayerBuilder).SupportsLegend;
               bCancel = !builder.SupportsMetaData && !(builder is LayerBuilder) && !((builder is LayerBuilder) && (builder as LayerBuilder).SupportsLegend);
            }

            if (dapDataset != null)
            {
               this.toolStripMenuItemviewMetadataServer.Visible = true;
               this.toolStripMenuItemviewMetadataServer.Enabled = true;
               bCancel = false;
            }
            
            if (this.toolStripMenuItemviewMetadataServer.Visible && !this.toolStripMenuItemServerLegend.Visible && !this.toolStripMenuItempropertiesServer.Visible)
               this.toolStripSeparatorRefreshCatalog.Visible = false;

            if (bCancel)
               e.Cancel = true;
         }
         else
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
         this.worldWindow.DrawArgs.WorldCamera.SlerpPercentage = World.Settings.CameraSlerpInertia;
         this.worldWindow.DrawArgs.WorldCamera.SetPosition(
                  this.worldWindow.Latitude,
                  this.worldWindow.Longitude,
                   this.worldWindow.DrawArgs.WorldCamera.Heading.Degrees,
                   this.worldWindow.DrawArgs.WorldCamera.Altitude,
                   0);

      }

      private void toolStripButtonRestoreNorth_Click(object sender, EventArgs e)
      {
         this.worldWindow.DrawArgs.WorldCamera.SlerpPercentage = World.Settings.CameraSlerpInertia;
         this.worldWindow.DrawArgs.WorldCamera.SetPosition(
                  this.worldWindow.Latitude,
                  this.worldWindow.Longitude,
                   0,
                   this.worldWindow.DrawArgs.WorldCamera.Altitude,
                   this.worldWindow.DrawArgs.WorldCamera.Tilt.Degrees);
      }

      private void toolStripButtonResetCamera_Click(object sender, EventArgs e)
      {
         this.worldWindow.DrawArgs.WorldCamera.SlerpPercentage = World.Settings.CameraSlerpInertia;
         this.worldWindow.DrawArgs.WorldCamera.Reset();
      }


      private void timerNavigation_Tick(object sender, EventArgs e)
      {
         this.bNavTimer = true;
         this.worldWindow.DrawArgs.WorldCamera.SlerpPercentage = 1.0;
         switch (this.eNavMode)
         {
            case NavMode.ZoomIn:
               this.worldWindow.DrawArgs.WorldCamera.Zoom(0.2f);
               return;
            case NavMode.ZoomOut:
               this.worldWindow.DrawArgs.WorldCamera.Zoom(-0.2f);
               return;
            case NavMode.RotateLeft:
               Angle rotateClockwise = Angle.FromRadians(-0.01f);
               this.worldWindow.DrawArgs.WorldCamera.Heading += rotateClockwise;
               this.worldWindow.DrawArgs.WorldCamera.RotationYawPitchRoll(Angle.Zero, Angle.Zero, rotateClockwise);
               return;
            case NavMode.RotateRight:
               Angle rotateCounterclockwise = Angle.FromRadians(0.01f);
               this.worldWindow.DrawArgs.WorldCamera.Heading += rotateCounterclockwise;
               this.worldWindow.DrawArgs.WorldCamera.RotationYawPitchRoll(Angle.Zero, Angle.Zero, rotateCounterclockwise);
               return;
            case NavMode.TiltUp:
               this.worldWindow.DrawArgs.WorldCamera.Tilt += Angle.FromDegrees(-1.0f);
               return;
            case NavMode.TiltDown:
               this.worldWindow.DrawArgs.WorldCamera.Tilt += Angle.FromDegrees(1.0f);
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
            this.worldWindow.DrawArgs.WorldCamera.SlerpPercentage = World.Settings.CameraSlerpInertia;
            this.worldWindow.DrawArgs.WorldCamera.Zoom(2.0f);
         }
         else
            this.bNavTimer = false;
      }

      private void toolStripButtonZoomOut_Click(object sender, EventArgs e)
      {
         this.timerNavigation.Enabled = false;
         if (!this.bNavTimer)
         {
            this.worldWindow.DrawArgs.WorldCamera.SlerpPercentage = World.Settings.CameraSlerpInertia;
            this.worldWindow.DrawArgs.WorldCamera.Zoom(-2.0f);
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
            this.worldWindow.DrawArgs.WorldCamera.SlerpPercentage = World.Settings.CameraSlerpInertia;
            this.worldWindow.DrawArgs.WorldCamera.Heading += rotateClockwise;
            this.worldWindow.DrawArgs.WorldCamera.RotationYawPitchRoll(Angle.Zero, Angle.Zero, rotateClockwise);
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
            this.worldWindow.DrawArgs.WorldCamera.SlerpPercentage = World.Settings.CameraSlerpInertia;
            this.worldWindow.DrawArgs.WorldCamera.Heading += rotateCounterclockwise;
            this.worldWindow.DrawArgs.WorldCamera.RotationYawPitchRoll(Angle.Zero, Angle.Zero, rotateCounterclockwise);
         }
         else
            this.bNavTimer = false;
      }

      private void toolStripButtonTiltUp_Click(object sender, EventArgs e)
      {
         this.timerNavigation.Enabled = false;
         if (!this.bNavTimer)
         {
            this.worldWindow.DrawArgs.WorldCamera.SlerpPercentage = World.Settings.CameraSlerpInertia;
            this.worldWindow.DrawArgs.WorldCamera.Tilt += Angle.FromDegrees(-10.0f);
         }
         else
            this.bNavTimer = false;
      }

      private void toolStripButtonTiltDown_Click(object sender, EventArgs e)
      {
         this.timerNavigation.Enabled = false;
         if (!this.bNavTimer)
         {
            this.worldWindow.DrawArgs.WorldCamera.SlerpPercentage = World.Settings.CameraSlerpInertia;
            this.worldWindow.DrawArgs.WorldCamera.Tilt += Angle.FromDegrees(10.0f);
         }
         else
            this.bNavTimer = false;
      }

      #endregion

      #region Add Layers

      private void toolStripMenuItemOpen_Click(object sender, EventArgs e)
      {
         string strLastFolderCfg = Path.Combine(this.worldWindow.WorldWindSettings.ConfigPath, "opengeotif.cfg");

         OpenFileDialog openFileDialog = new OpenFileDialog();
         openFileDialog.Filter = "GeoTIFF Files|*.tif;*.tiff";
         openFileDialog.Title = "Open GeoTIFF File in Current View...";
         openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
         openFileDialog.RestoreDirectory = true;
         if (File.Exists(strLastFolderCfg))
         {
            try
            {
               using (StreamReader sr = new StreamReader(strLastFolderCfg))
               {
                  string strDir = sr.ReadLine();
                  if (Directory.Exists(strDir))
                     openFileDialog.InitialDirectory = strDir;
               }
            }
            catch
            {
            }
         }

         if (openFileDialog.ShowDialog(this) == DialogResult.OK)
         {
            AddGeoTiff(openFileDialog.FileName, "", false, true);
            try
            {
               using (StreamWriter sw = new StreamWriter(strLastFolderCfg))
               {
                  sw.WriteLine(Path.GetDirectoryName(openFileDialog.FileName));
               }
            }
            catch
            {
            }
         }
      }

      void AddGeoTiff(string strGeoTiff, string strGeoTiffName, bool bTmp, bool bGoto)
      {
         LayerBuilder builder = new GeorefImageLayerBuilder(WWSettingsCtl.CachePath, strGeoTiff, bTmp, this.worldWindow.CurrentWorld, null);
         
         Cursor = Cursors.WaitCursor;
         if (builder.GetLayer() != null)
         {
            Cursor = Cursors.Default;

            // If the file is already there remove it 
            foreach (LayerBuilderContainer container in this.activeLayers)
            {
               if (container.Builder != null && container.Builder is GeorefImageLayerBuilder)
               {
                  if (String.Compare((container.Builder as GeorefImageLayerBuilder).FileName, strGeoTiff, true) == 0)
                  {
                     this.activeLayers.RemoveContainer(container);
                     break;
                  }
               }
            }

            // If there is already a layer by that name find unique name
            if (strGeoTiffName.Length > 0)
            {
               int iCount = 0;
               string strNewName = strGeoTiffName;
               bool bExist = true;
               while (bExist)
               {
                  bExist = false;
                  foreach (LayerBuilderContainer container in this.activeLayers)
                  {
                     if (container.Name == strNewName)
                     {
                        bExist = true;
                        break;
                     }
                  }

                  if (bExist)
                  {
                     iCount++;
                     strNewName = strGeoTiffName + "_" + iCount.ToString();
                  }
               }
               strGeoTiffName = strNewName;
            }

            if (strGeoTiffName.Length > 0)
               this.activeLayers.Add(strGeoTiffName, builder, true, 255, true, bTmp);
            else
               this.activeLayers.Add(builder.Name, builder, true, 255, true, bTmp);
            if (bGoto)
               GoTo(builder as ImageBuilder);
         }
         else
         {
            Cursor = Cursors.Default;
            string strMessage = "Error adding the file: '" + strGeoTiff + "'.\nOnly WGS 84 geographic images can be displayed at this time.";
            string strGeoInfo = GeorefImageLayerBuilder.GetGeorefInfoFromGeotif(strGeoTiff);
            if (strGeoInfo.Length > 0)
               strMessage += "\nThis image is:\n\n" + strGeoInfo;
            MessageBox.Show(this, strMessage, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
         }
      }

      private void toolStripMenuItemAddLayer_Click(object sender, EventArgs e)
      {
         this.tvServers.AddCurrentDataset();
      }

      public bool bContainsDAPLayer(Geosoft.GX.DAPGetData.Server oServer, Geosoft.Dap.Common.DataSet oDataset)
      {
         foreach (LayerBuilderContainer container in this.activeLayers)
         {
            if (container.Builder is DAPQuadLayerBuilder)
            {
               DAPQuadLayerBuilder dapLayerBuilder = container.Builder as DAPQuadLayerBuilder;

               if (dapLayerBuilder.DAPServerURL == oServer.Url && oDataset.Name == dapLayerBuilder.DatasetName)
                  return true;
            }
         }
         return false;
      }

      public void AddLayerBuilder(LayerBuilder oLayer)
      {
         this.activeLayers.Add(oLayer.Name, oLayer, true, oLayer.Opacity, true);

         if (tvLayers.SelectedNode != null)
            LayerBuilderItem = tvLayers.SelectedNode.Tag as LayerBuilderContainer;
         else
            LayerBuilderItem = null;

         SaveLastView();
      }
      #endregion

      #region Add Servers

      private void AddDAPServer()
      {
         AddDAP dlg = new AddDAP();
         if (dlg.ShowDialog(this) == DialogResult.OK)
         {
            Geosoft.GX.DAPGetData.Server oServer;
            try
            {
               this.tvServers.AddDAPServer(dlg.Url, out oServer);
            }
            catch (Exception except)
            {
               MessageBox.Show(this, "Error adding server \"" + dlg.Url + "\" (" + except.Message + ")", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            SaveLastView();
         }
      }

      private void AddWMSServer()
      {
         TreeNode treeNode = TreeUtils.FindNodeOfTypeBFS(typeof(WMSCatalogBuilder), this.tvServers.Nodes);

         if (treeNode != null)
         {
            AddWMS dlg = new AddWMS(this.worldWindow, treeNode.Tag as WMSCatalogBuilder);
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
               try
               {
                  this.tvServers.AddWMSServer(dlg.WmsURL, true);
               }
               catch (Exception except)
               {
                  MessageBox.Show(this, "Error adding server \"" + dlg.WmsURL + "\" (" + except.Message + ")", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
               }
               SaveLastView();
            }
         }
      }

      private void toolStripMenuItemAddDAP_Click(object sender, EventArgs e)
      {
         AddDAPServer();
      }

      private void toolStripMenuItemAddWMS_Click(object sender, EventArgs e)
      {
         AddWMSServer();
      }

      void toolStripMenuItemaddServer_Click(object sender, EventArgs e)
      {
         if (this.tvServers.SelectedNode.Nodes == this.tvServers.DAPRootNodes)
            AddDAPServer();
         else if (this.tvServers.SelectedNode.Nodes == this.tvServers.WMSRootNodes)
            AddWMSServer();
      }

      #endregion

      #region Remove Items

      private void toolStripMenuItemremoveServer_Click(object sender, EventArgs e)
      {
         this.tvServers.RemoveCurrentServer();
         SaveLastView();
      }

      private void toolStripMenuItemremoveAllButThis_Click(object sender, EventArgs e)
      {
         if (LayerBuilderItem == null)
            return;

         this.activeLayers.RemoveOthers(LayerBuilderItem);

         if (tvLayers.SelectedNode != null)
            LayerBuilderItem = tvLayers.SelectedNode.Tag as LayerBuilderContainer;
         else
            LayerBuilderItem = null;
      }

      private void toolStripMenuButtonItemremove_Click(object sender, EventArgs e)
      {
         if (LayerBuilderItem == null)
            return;

         LayerBuilderContainer current = LayerBuilderItem;
         LayerBuilderItem = null;
         this.activeLayers.RemoveContainer(current);

         if (tvLayers.SelectedNode != null)
            LayerBuilderItem = tvLayers.SelectedNode.Tag as LayerBuilderContainer;
         else
            LayerBuilderItem = null;
      }

      private void toolStripMenuItemRemoveAll_Click(object sender, EventArgs e)
      {
         this.activeLayers.RemoveAll();
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
         if (this.tvServers.SelectedNode != null)
         {
            if (this.tvServers.SelectedNode.Tag is ImageBuilder)
               GoTo(this.tvServers.SelectedNode.Tag as ImageBuilder);
            if (this.tvServers.SelectedNode.Tag is Geosoft.Dap.Common.DataSet)
            {
               Geosoft.Dap.Common.DataSet dataSet = this.tvServers.SelectedNode.Tag as Geosoft.Dap.Common.DataSet;
               GeographicBoundingBox extents =
                  new GeographicBoundingBox(dataSet.Boundary.MaxY,
                dataSet.Boundary.MinY,
                dataSet.Boundary.MinX,
                dataSet.Boundary.MaxX);
               GoTo(extents, -1.0);
            }
         }
      }

      void GoTo(ImageBuilder builder)
      {
         GoTo(builder.Extents, (builder is QuadLayerBuilder) ? (double) (builder as QuadLayerBuilder).LevelZeroTileSize : -1.0);
      }

      void GoTo(GeographicBoundingBox extents, double dLevelZeroTileSize)
      {
         double latitude, longitude;
         long overviewCameraAlt = 12000000;

         if (extents.North > 89 &&
            extents.South < -89 &&
            extents.West < -179 &&
            extents.East > 179)
         {
            latitude = this.worldWindow.Latitude;
            longitude = this.worldWindow.Longitude;
         }
         else
         {
            latitude = extents.South + (extents.North - extents.South) / 2.0;
            longitude = extents.West + (extents.East - extents.West) / 2.0;
         }


         double fov = 2.0 * Math.Max(extents.North - extents.South, extents.East - extents.West);

         if (dLevelZeroTileSize > 0.0)
            fov = Math.Min(fov, 2 * dLevelZeroTileSize);
         if (fov < 180.0)
            this.worldWindow.GotoLatLonHeadingViewRange(latitude, longitude, 0, fov);
         else
         {
            this.worldWindow.GotoLatLonAltitude(latitude, longitude, overviewCameraAlt);
         }
      }

      #endregion

      #region Render Order

      private void toolStripMenuItemButtonAtBottom_Click(object sender, EventArgs e)
      {
         if (LayerBuilderItem != null && !this.activeLayers.IsBottom(LayerBuilderItem))
         {
            this.activeLayers.MoveBottom(LayerBuilderItem);
            if (tvLayers.SelectedNode != null)
               LayerBuilderItem = tvLayers.SelectedNode.Tag as LayerBuilderContainer;
            else
               LayerBuilderItem = null;
         }
      }

      private void toolStripMenuItemButtonMoveDown_Click(object sender, EventArgs e)
      {
         if (LayerBuilderItem != null && !this.activeLayers.IsBottom(LayerBuilderItem))
         {
            this.activeLayers.MoveDown(LayerBuilderItem);
            if (tvLayers.SelectedNode != null)
               LayerBuilderItem = tvLayers.SelectedNode.Tag as LayerBuilderContainer;
            else
               LayerBuilderItem = null;
         }
      }

      private void toolStripMenuItemButtonMoveUp_Click(object sender, EventArgs e)
      {
         if (LayerBuilderItem != null && !this.activeLayers.IsTop(LayerBuilderItem))
         {
            this.activeLayers.MoveUp(LayerBuilderItem);
            if (tvLayers.SelectedNode != null)
               LayerBuilderItem = tvLayers.SelectedNode.Tag as LayerBuilderContainer;
            else
               LayerBuilderItem = null;
         }
      }

      private void toolStripMenuItemButtonOnTop_Click(object sender, EventArgs e)
      {
         if (LayerBuilderItem != null && !this.activeLayers.IsTop(LayerBuilderItem))
         {
            this.activeLayers.MoveTop(LayerBuilderItem);
            if (tvLayers.SelectedNode != null)
               LayerBuilderItem = tvLayers.SelectedNode.Tag as LayerBuilderContainer;
            else
               LayerBuilderItem = null;
         }
      }

      #endregion

      #region Refresh
      private void toolStripMenuItemRefresh_Click(object sender, EventArgs e)
      {
         if (LayerBuilderItem != null && LayerBuilderItem.Builder != null)
            this.activeLayers.RefreshBuilder(LayerBuilderItem.Builder);
      }

      private void toolStripMenuItemClearRefresh_Click(object sender, EventArgs e)
      {
         if (LayerBuilderItem != null && LayerBuilderItem.Builder != null)
         {
            Utility.FileSystem.DeleteFolderGUI(this, LayerBuilderItem.Builder.GetCachePath());
            this.activeLayers.RefreshBuilder(LayerBuilderItem.Builder);
         }
      }

      private void toolStripMenuItemRefreshCatalog_Click(object sender, EventArgs e)
      {
         this.tvServers.RefreshCurrentServer();
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
         if (this.tvServers.SelectedNode != null && this.tvServers.SelectedNode.Tag is LayerBuilder)
         {
            if ((this.tvServers.SelectedNode.Tag as LayerBuilder).SupportsLegend)
            {
               string[] strLegendArr = (this.tvServers.SelectedNode.Tag as LayerBuilder).GetLegendURLs();
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
         if (this.tvServers.SelectedNode != null && this.tvServers.SelectedNode.Tag is IBuilder)
            ViewMetadata(this.tvServers.SelectedNode.Tag as IBuilder);
         else
            ViewMetadata(null);
      }

      void ViewMetadata(IBuilder builder)
      {
         try
         {
            MetaDataForm form = new MetaDataForm();
            XmlDocument oDoc = new XmlDocument();
            oDoc.AppendChild(oDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes"));
            string strStyleSheet = null;
            XmlNode oNode;

            if (builder == null)
            {
               oNode = this.tvServers.GetCurrentDAPMetaData(oDoc);
               strStyleSheet = "dap_dataset.xsl";
            }
            else
            {
               oNode = builder.GetMetaData(oDoc);
               strStyleSheet = builder.StyleSheetName; 
            }
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
            if (strStyleSheet != null)
            {
               XmlNode oRef = oDoc.CreateProcessingInstruction("xml-stylesheet", "type='text/xsl' href='" + Path.Combine(this.metaviewerDir, strStyleSheet) + "'");
               oDoc.InsertBefore(oRef, oDoc.DocumentElement);
            }

            string filePath = Path.Combine(this.metaviewerDir, Path.GetRandomFileName());
            oDoc.Save(filePath);
            form.WindowState = FormWindowState.Maximized;
            form.ShowDialog(this, filePath);
            System.IO.File.Delete(filePath);
         }
         catch (Exception e)
         {
            MessageBox.Show(this, "Could not display metadata because invalid data was received from server (" + e.Message + ").", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
         }
      }

      private void toolStripMenuItemproperties_Click(object sender, EventArgs e)
      {
         if (LayerBuilderItem != null && LayerBuilderItem.Builder != null)
            ViewProperties(LayerBuilderItem.Builder);
      }

      private void toolStripMenuItempropertiesServer_Click(object sender, EventArgs e)
      {
         if (this.tvServers.SelectedNode != null && this.tvServers.SelectedNode.Tag is IBuilder)
            ViewProperties(this.tvServers.SelectedNode.Tag as IBuilder);
      }

      void ViewProperties(IBuilder builder)
      {
         frmProperties form = new frmProperties();
         form.SetObject = builder;
         form.ShowDialog(this);
         if (builder.IsChanged && builder is LayerBuilder && (builder as LayerBuilder).IsAdded)
         {
            this.activeLayers.RefreshBuilder(builder as LayerBuilder);
            if (tvLayers.SelectedNode != null)
               LayerBuilderItem = tvLayers.SelectedNode.Tag as LayerBuilderContainer;
            else
               LayerBuilderItem = null;
         }
      }

      #endregion

      #endregion

      #region Main Menu Item Click events

      private void toolStripMenuItemAskAtStartup_Click(object sender, EventArgs e)
      {
         toolStripMenuItemLoadLastView.Checked = false;
         this.WWSettingsCtl.AskLastViewAtStartup = toolStripMenuItemAskAtStartup.Checked;
      }

      private void toolStripMenuItemLoadLastView_Click(object sender, EventArgs e)
      {
         this.WWSettingsCtl.AskLastViewAtStartup = false;
         this.toolStripMenuItemAskAtStartup.Checked = false;
         this.WWSettingsCtl.LastViewAtStartup = toolStripMenuItemLoadLastView.Checked;
      }

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
         this.worldWindow.Invalidate();
      }

      private void toolStripMenuItemcompass_Click(object sender, EventArgs e)
      {
         World.Settings.ShowCompass = toolStripMenuItemcompass.Checked;
         this.compassPlugin.Layer.IsOn = World.Settings.ShowCompass;
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
         ViewOpenDialog dlgtest = new ViewOpenDialog(this.worldWindow.WorldWindSettings.ConfigPath);
         DialogResult res = dlgtest.ShowDialog(this);
         if (dlgtest.ViewFile != null)
         {
            if (res == DialogResult.OK)
               OpenView(dlgtest.ViewFile, true);
         }
      }

      private void toolStripMenuItemResetDefaultView_Click(object sender, EventArgs e)
      {
         OpenView(Path.Combine(this.worldWindow.WorldWindSettings.DataPath, DefaultView), true);
      }

      private void MainForm_Shown(object sender, EventArgs e)
      {
         // This will ensure that the buttons is enabled correctly
         LayerBuilderItem = null;

         if (this.openView.Length > 0)
            OpenView(openView, this.openGeoTiff.Length == 0);
         else if (this.openGeoTiff.Length == 0 && File.Exists(Path.Combine(WWSettingsCtl.ConfigPath, LastView)))
         {
            if (WWSettingsCtl.AskLastViewAtStartup)
            {
               Utils.MessageBoxExLib.MessageBoxEx msgBox = Utils.MessageBoxExLib.MessageBoxExManager.CreateMessageBox(null);
               msgBox.AllowSaveResponse = true;
               msgBox.SaveResponseText = "Don't ask me again";
               msgBox.Caption = this.Text;
               msgBox.Icon = Utils.MessageBoxExLib.MessageBoxExIcon.Question;
               msgBox.AddButtons(MessageBoxButtons.YesNo);
               msgBox.Text = "Would you like to open your last View?";
               msgBox.Font = this.Font;
               WWSettingsCtl.LastViewAtStartup = msgBox.Show() == Utils.MessageBoxExLib.MessageBoxExResult.Yes;
               if (msgBox.SaveResponse)
                  WWSettingsCtl.AskLastViewAtStartup = false;
            }

            if (WWSettingsCtl.LastViewAtStartup)
               OpenView(Path.Combine(WWSettingsCtl.ConfigPath, LastView), this.openGeoTiff.Length == 0);
            else
               OpenView(Path.Combine(WWSettingsCtl.ConfigPath, LastView), false, false);
         }
         else
            OpenView(Path.Combine(this.worldWindow.WorldWindSettings.DataPath, DefaultView), this.openGeoTiff.Length == 0);

         if (this.openGeoTiff.Length > 0)
            AddGeoTiff(this.openGeoTiff, this.openGeoTiffName, this.openGeoTiffTmp, true);


         // Check for updates daily
         if (WWSettingsCtl.UpdateCheckDate.Date != DateTime.Now.Date)
            CheckForUpdates(false);
         WWSettingsCtl.UpdateCheckDate = DateTime.Now;

         foreach (RenderableObject oRO in this.worldWindow.CurrentWorld.RenderableObjects.ChildObjects)
         {
            if (oRO.Name == "1 - Grid Lines")
            {
               oRO.IsOn = World.Settings.ShowLatLonLines;
               break;
            }
         }
         
      }

      bool m_bSizing = false;
      private void MainForm_ResizeBegin(object sender, EventArgs e)
      {
         m_bSizing = true;
         this.worldWindow.Visible = false;
      }

      private void MainForm_ResizeEnd(object sender, EventArgs e)
      {
         m_bSizing = false;
         this.worldWindow.Visible = true;
         this.worldWindow.SafeRender();
      }

      private void MainForm_Resize(object sender, EventArgs e)
      {
         this.worldWindow.Visible = false;
      }

      private void MainForm_SizeChanged(object sender, EventArgs e)
      {
         if (!m_bSizing)
         {
            this.worldWindow.Visible = true;
            this.worldWindow.SafeRender();
         }
      }

      private void splitContainerMain_SplitterMoving(object sender, SplitterCancelEventArgs e)
      {
         this.worldWindow.Visible = false;
      }

      private void splitContainerMain_SplitterMoved(object sender, SplitterEventArgs e)
      {
         if (!m_bSizing)
         {
            this.worldWindow.Visible = true;
            this.worldWindow.SafeRender();
            this.overviewCtl.Refresh();
         }
      }


      private void toolStripMenuItemeditBlueMarble_Click(object sender, EventArgs e)
      {
         this.bmngPlugin.ShowDialog(this);
      }

      private void toolStripMenuItemshowGridLines_Click(object sender, EventArgs e)
      {
         World.Settings.ShowLatLonLines = toolStripMenuItemshowGridLines.Checked;
         foreach (RenderableObject oRO in this.worldWindow.CurrentWorld.RenderableObjects.ChildObjects)
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
         this.worldWindow.Invalidate();
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

      #region Current Layer Panel

      private void tvLayers_TreeNodeChecked(object sender, Geosoft.DotNetTools.TreeNodeCheckedEventArgs e)
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

      private void tvLayers_KeyUp(object sender, KeyEventArgs e)
      {
         switch (e.KeyCode)
         {
            case Keys.Delete:
               toolStripMenuButtonItemremove_Click(sender, e);
               break;
         }
      }

      // The multiple selection code loosely based on http://www.codeproject.com/cs/miscctrl/treeviewms.asp
      /*
      private void tvLayers_MouseDown(object sender, MouseEventArgs e)
      {
         bool bControl = (ModifierKeys == Keys.Control);
         bool bShift = (ModifierKeys == Keys.Shift);

         if (e.Button == MouseButtons.Left && !bControl && !bShift && tvLayers.HitTest(e.Location).Node == null)
         {
            tvLayers.SelectedNode = null;
            tvLayers_RemovePaintFromNodes();
            this.triStateTreeViewLayerNodes.Clear();
            LayerBuilderItem = null;
         }

         tvLayers.SelectedNode = null;

         if (e.Button == MouseButtons.Right)
            this.rightmouse_context = true;
         else
            this.rightmouse_context = false;
      }

      private void tvLayers_PaintSelectedNodes()
      {
         foreach (TreeNode n in this.triStateTreeViewLayerNodes)
         {
            n.BackColor = SystemColors.Highlight;
            n.ForeColor = SystemColors.HighlightText;
         }
      }

      private void tvLayers_RemovePaintFromNodes()
      {
         if (this.triStateTreeViewLayerNodes.Count == 0) return;

         TreeNode n0 = (TreeNode)this.triStateTreeViewLayerNodes[0];
         Color back = n0.TreeView.BackColor;
         Color fore = n0.TreeView.ForeColor;

         foreach (TreeNode n in this.triStateTreeViewLayerNodes)
         {
            n.BackColor = back;
            n.ForeColor = fore;
         }
      }
      
      private void tvLayers_BeforeSelect(object sender, TreeViewCancelEventArgs e)
      {
         bool bControl = (ModifierKeys == Keys.Control);
         bool bShift = (ModifierKeys == Keys.Shift);

         // selecting twice the node while pressing CTRL ?
         if (bControl && this.triStateTreeViewLayerNodes.Contains(e.Node))
         {
            // unselect it
            // (let framework know we don't want selection this time)
            e.Cancel = true;

            // update nodes
            tvLayers_RemovePaintFromNodes();
            this.triStateTreeViewLayerNodes.Remove(e.Node);
            tvLayers_PaintSelectedNodes();
            return;
         }

         this.lastLayerNode = e.Node;
         if (!bShift) this.firstLayerNode = e.Node; // store begin of shift sequence
      }

      private void tvLayers_AfterSelect(object sender, TreeViewEventArgs e)
      {
         bool bControl = (ModifierKeys == Keys.Control);
         bool bShift = (ModifierKeys == Keys.Shift);
         
         if (bControl)
         {
            if (!this.triStateTreeViewLayerNodes.Contains(e.Node)) // new node ?
            {
               this.triStateTreeViewLayerNodes.Add(e.Node);
            }
            else  // not new, remove it from the collection
            {
               tvLayers_RemovePaintFromNodes();
               this.triStateTreeViewLayerNodes.Remove(e.Node);
            }
            tvLayers_PaintSelectedNodes();
         }
         else
         {
            if (bShift)
            {
               Queue<TreeNode> myQueue = new Queue<TreeNode>();

               TreeNode uppernode = this.firstLayerNode;
               TreeNode bottomnode = e.Node;

               // case 1 : begin and end nodes are parent
               bool bParent = TreeUtils.isParent(this.firstLayerNode, e.Node);
               if (!bParent)
               {
                  bParent = TreeUtils.isParent(bottomnode, uppernode);
                  if (bParent) // swap nodes
                  {
                     TreeNode t = uppernode;
                     uppernode = bottomnode;
                     bottomnode = t;
                  }
               }
               if (bParent)
               {
                  TreeNode n = bottomnode;
                  while (n != uppernode.Parent)
                  {
                     if (!this.triStateTreeViewLayerNodes.Contains(n)) // new node ?
                        myQueue.Enqueue(n);

                     n = n.Parent;
                  }
               }
               // case 2 : nor the begin nor the
               // end node are descendant one another
               else
               {
                  // are they siblings ?                 

                  if ((uppernode.Parent == null && bottomnode.Parent == null)
                        || (uppernode.Parent != null &&
                        uppernode.Parent.Nodes.Contains(bottomnode)))
                  {
                     int nIndexUpper = uppernode.Index;
                     int nIndexBottom = bottomnode.Index;
                     if (nIndexBottom < nIndexUpper) // reversed?
                     {
                        TreeNode t = uppernode;
                        uppernode = bottomnode;
                        bottomnode = t;
                        nIndexUpper = uppernode.Index;
                        nIndexBottom = bottomnode.Index;
                     }

                     TreeNode n = uppernode;
                     while (nIndexUpper <= nIndexBottom)
                     {
                        if (!this.triStateTreeViewLayerNodes.Contains(n)) // new node ?
                           myQueue.Enqueue(n);

                        n = n.NextNode;

                        nIndexUpper++;
                     } // end while

                  }
                  else
                  {
                     if (!this.triStateTreeViewLayerNodes.Contains(uppernode))
                        myQueue.Enqueue(uppernode);
                     if (!this.triStateTreeViewLayerNodes.Contains(bottomnode))
                        myQueue.Enqueue(bottomnode);
                  }

               }

               this.triStateTreeViewLayerNodes.AddRange(myQueue);

               tvLayers_PaintSelectedNodes();
               // let us chain several SHIFTs if we like it
               this.firstLayerNode = e.Node;

            } // end if m_bShift
            else
            {
               // in the case of a simple click, just add this item
               if (this.triStateTreeViewLayerNodes != null && this.triStateTreeViewLayerNodes.Count > 0)
               {
                  tvLayers_RemovePaintFromNodes();
                  this.triStateTreeViewLayerNodes.Clear();
               }
               this.triStateTreeViewLayerNodes.Add(e.Node);
            }
         }

         if (e.Node != null && e.Node.Tag != null)
            LayerBuilderItem = e.Node.Tag as LayerBuilderContainer;
         else
            LayerBuilderItem = null;
      }
      */

      private void tvLayers_AfterSelect(object sender, TreeViewEventArgs e)
      {
         if (e.Node != null && e.Node.Tag != null)
            LayerBuilderItem = e.Node.Tag as LayerBuilderContainer;
         else
            LayerBuilderItem = null;
      }

      private void tvLayers_MouseDown(object sender, MouseEventArgs e)
      {
         tvLayers.SelectedNode = tvLayers.HitTest(e.Location).Node;

         if (tvLayers.SelectedNode == null)
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
         
         foreach (LayerBuilderContainer container in this.activeLayers)
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
         foreach (LayerBuilderContainer container in this.activeLayers)
         {
            if (container.Visible && container.Builder != null)
            {
               RenderableObject ro = container.Builder.GetLayer();
               if (ro != null)
               {
                  RenderableObject.ExportInfo expinfo = new RenderableObject.ExportInfo();
                  ro.InitExportInfo(this.worldWindow.DrawArgs, expinfo);

                  if (expinfo.iPixelsX > 0 && expinfo.iPixelsY > 0)
                     expList.Add(new ExportEntry(container, ro, expinfo));
               }
            }
         }
         if (roBMNG != null && roBMNG.IsOn)
         {
            RenderableObject.ExportInfo expinfo = new RenderableObject.ExportInfo();
            roBMNG.InitExportInfo(this.worldWindow.DrawArgs, expinfo);
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

         WorldWind.Camera.MomentumCamera camera = this.worldWindow.DrawArgs.WorldCamera as WorldWind.Camera.MomentumCamera;
         if (camera.Tilt.Degrees > 5.0)
         {
            MessageBox.Show(this, "It is not possible to export a tilted view. Reset the tilt using the navigation buttons\nor by using Right-Mouse-Button and drag and try again.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
         }

         try
         {
            ExportView dlg = new ExportView(this.worldWindow.WorldWindSettings.ConfigPath);
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
               Cursor = Cursors.WaitCursor;

               // Stop the camera
               camera.SetPosition(camera.Latitude.Degrees, camera.Longitude.Degrees, camera.Heading.Degrees, camera.Altitude, camera.Tilt.Degrees);
               
               // Determine output parameters
               GeographicBoundingBox geoExtent = GeographicBoundingBox.FromQuad(this.worldWindow.GetViewBox());
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
                              exp.RO.ExportProcess(this.worldWindow.DrawArgs, exp.Info);

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
         return this.tvServers.iImageListIndex(strKey);
      }

      /// <summary>
      /// Returns imagelist index from dap dataset type
      /// </summary>
      /// <param name="strType"></param>
      /// <returns></returns>
      public int ImageIndex(string strType)
      {
         return this.tvServers.iImageIndex(strType);
      }

      private void AddTreeNode(TreeView tree, LayerBuilder builder, TreeNode parent)
      {
         int iImageIndex;
         TreeNode treeNode = null;
         if (builder is DAPQuadLayerBuilder)
         {
            DAPQuadLayerBuilder dapbuilder = (DAPQuadLayerBuilder)builder;

            iImageIndex = ImageIndex(dapbuilder.DAPType.ToLower());
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

         WorldWind.Camera.MomentumCamera camera = this.worldWindow.DrawArgs.WorldCamera as WorldWind.Camera.MomentumCamera;

         //stop the camera
         camera.SetPosition(camera.Latitude.Degrees, camera.Longitude.Degrees, camera.Heading.Degrees, camera.Altitude, camera.Tilt.Degrees);

         //store the servers
         this.tvServers.SaveToView(view);
         
         // store the current layers
         if (this.activeLayers.Count > 0)
         {
            activelayersType lyrs = view.View.Newactivelayers();
            foreach (LayerBuilderContainer container in this.activeLayers)
            {
               if (!container.Temporary)
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
         //this.worldWindow.SaveScreenshot(picFileName);
         //this.worldWindow.Render();

         using (Image img = TakeSnapshot(this.worldWindow.Handle))
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

      bool OpenView(string filename, bool bGoto)
      {
         return OpenView(filename, bGoto, true);
      }

      bool OpenView(string filename, bool bGoto, bool bLoadLayers)
      {
         bool bFinishUpdate = false;
         bool bOldView = false;
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

               if (bGoto && view.View.Hascameraorientation())
               {
                  cameraorientationType orient = view.View.cameraorientation;
                  this.worldWindow.DrawArgs.WorldCamera.SlerpPercentage = World.Settings.CameraSlerpInertia;
                  this.worldWindow.DrawArgs.WorldCamera.SetPosition(orient.lat.Value, orient.lon.Value, orient.heading.Value, orient.altitude.Value, orient.tilt.Value);
               }

               this.tvLayers.BeginUpdate();
               bFinishUpdate = true;
               this.activeLayers.RemoveAll();

               this.tvServers.LoadFromView(Path.GetFileNameWithoutExtension(filename), view);

               if (bLoadLayers && view.View.Hasactivelayers())
               {
                  for (i = 0; i < view.View.activelayers.datasetCount; i++)
                  {
                     datasetType dataset = view.View.activelayers.GetdatasetAt(i);

                     this.activeLayers.AddUsingUri(dataset.name.Value, dataset.uri.Value, dataset.Hasinvisible() ? !dataset.invisible.Value : true, (byte)dataset.opacity.Value, false, this.tvServers, ref bOldView);
                  }
               }
            }
         }
         catch (Exception e)
         {
            if (bFinishUpdate)
               this.tvLayers.EndUpdate();
            bFinishUpdate = false;
            if (MessageBox.Show(this, "Error loading view from " + filename + "\n(" + e.Message + ")\nDo you want to open the Dapple default view?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.Yes)
            {
               return OpenView(Path.Combine(this.worldWindow.WorldWindSettings.DataPath, DefaultView), true);
            }
         }

         if (bFinishUpdate)
            this.tvLayers.EndUpdate();
         if (bOldView)
            MessageBox.Show(this, "The view " + filename + " contained some layers from an earlier version\nwhich could not be retrieved. We apologize for the inconvenience.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
         this.toolStripButtonClearFilter.Enabled = false;
         m_strLastSearchText = "";
         this.toolStripFilterText.ForeColor = SystemColors.GrayText;
         return true;
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
         this.toolStripButtonFilterText.Enabled = false;
         this.toolStripButtonClearFilter.Enabled = false;

         this.worldWindow.IsRenderDisabled = false;

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
         this.toolStripStatusSpin6.Alignment = ToolStripItemAlignment.Right; 
         this.worldWindow.Updated += new WorldWindow.UpdatedDelegate(OnUpdated);
      }

      void MainForm_Closing(object sender, CancelEventArgs e)
      {
         // Turning off the layers will set this
         bool bSaveGridLineState = World.Settings.ShowLatLonLines;

         this.WindowState = FormWindowState.Minimized;

         SaveLastView();
         
         // Ensure cleanup
         this.activeLayers.RemoveAll();
         for (int i = 0; i < this.worldWindow.CurrentWorld.RenderableObjects.Count; i++)
         {
            RenderableObject oRO = (RenderableObject)this.worldWindow.CurrentWorld.RenderableObjects.ChildObjects[i];
            oRO.IsOn = false;
            oRO.Dispose();
         }

         World.Settings.ShowLatLonLines = bSaveGridLineState;

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

         this.worldWindow.Dispose();

         this.panelServer.Controls.Remove(this.tvServers);
         this.tvServers.Dispose();
      }

      private void MainForm_Deactivate(object sender, EventArgs e)
      {
         this.worldWindow.IsRenderDisabled = true;
      }

      private void MainForm_Activated(object sender, EventArgs e)
      {
         this.worldWindow.IsRenderDisabled = false;
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
               string strGeoTiffName = strData[2];
               bool bGeotiffTmp = strData[3] == "YES";
               this.lastView = strData[4];

               if (strView.Length > 0)
                  OpenView(strView, strGeoTiff.Length == 0);
               if (strGeoTiff.Length > 0)
                  AddGeoTiff(strGeoTiff, strGeoTiffName, bGeotiffTmp, true);
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
         
         foreach (LayerBuilderContainer container in this.activeLayers)
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

         this.BeginInvoke(new WorldWindow.UpdatedDelegate(Updated));
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
                  this.toolStripStatusLabel6.Image = this.tvServers.ImageList.Images["marble"];
               }
               else
               {
                  this.toolStripStatusLabel6.ToolTipText = m_downloadList[5].builder.Name;
                  this.toolStripStatusLabel6.Image = this.tvServers.ImageList.Images[m_downloadList[5].builder.LogoKey];
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
                  this.toolStripStatusLabel5.Image = global::Dapple.Properties.Resources.marble_icon.ToBitmap();
               }
               else
               {
                  this.toolStripStatusLabel5.ToolTipText = m_downloadList[4].builder.Name;
                  this.toolStripStatusLabel5.Image = this.tvServers.ImageList.Images[m_downloadList[4].builder.LogoKey];
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
                  this.toolStripStatusLabel4.Image = global::Dapple.Properties.Resources.marble_icon.ToBitmap();
               }
               else
               {
                  this.toolStripStatusLabel4.ToolTipText = m_downloadList[3].builder.Name;
                  this.toolStripStatusLabel4.Image = this.tvServers.ImageList.Images[m_downloadList[3].builder.LogoKey];
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
                  this.toolStripStatusLabel3.Image = global::Dapple.Properties.Resources.marble_icon.ToBitmap();
               }
               else
               {
                  this.toolStripStatusLabel3.ToolTipText = m_downloadList[2].builder.Name;
                  this.toolStripStatusLabel3.Image = this.tvServers.ImageList.Images[m_downloadList[2].builder.LogoKey];
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
                  this.toolStripStatusLabel2.Image = global::Dapple.Properties.Resources.marble_icon.ToBitmap();
               }
               else
               {
                  this.toolStripStatusLabel2.ToolTipText = m_downloadList[1].builder.Name;
                  this.toolStripStatusLabel2.Image = this.tvServers.ImageList.Images[m_downloadList[1].builder.LogoKey];
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
                  this.toolStripStatusLabel1.Image = global::Dapple.Properties.Resources.marble_icon.ToBitmap();
               }
               else
               {
                  this.toolStripStatusLabel1.ToolTipText = m_downloadList[0].builder.Name;
                  this.toolStripStatusLabel1.Image = this.tvServers.ImageList.Images[m_downloadList[0].builder.LogoKey];
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
      private GeographicBoundingBox m_extentsLastSearch = null;
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

      private void FilterServersToView(bool bIntersect)
      {
         m_extentsLastSearch = GeographicBoundingBox.FromQuad(this.worldWindow.GetViewBox());
         this.tvServers.Search(bIntersect, m_extentsLastSearch, "");
         this.toolStripButtonClearFilter.Enabled = true;
         m_strLastSearchText = "";
         this.toolStripFilterText.ForeColor = SystemColors.GrayText;
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
         this.tvServers.ClearSearch();
         this.toolStripButtonClearFilter.Enabled = false;
         m_strLastSearchText = "";
         m_extentsLastSearch = null;
         this.toolStripFilterText.ForeColor = SystemColors.GrayText;
      }

      private void toolStripFilterText_KeyDown(object sender, KeyEventArgs e)
      {
         if (e.KeyCode == Keys.Return)
            toolStripButtonFilterText_Click(sender, null);
      }

      private void toolStripButtonFilterText_Click(object sender, EventArgs e)
      {
         this.tvServers.Search(m_extentsLastSearch != null, m_extentsLastSearch, this.toolStripFilterText.Text);
         this.toolStripButtonClearFilter.Enabled = true;
         m_strLastSearchText = this.toolStripFilterText.Text;
         this.toolStripFilterText.ForeColor = SystemColors.WindowText;
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
         for (int i = 0; i < this.worldWindow.CurrentWorld.RenderableObjects.Count; i++)
         {
            if (((RenderableObject)this.worldWindow.CurrentWorld.RenderableObjects.ChildObjects[i]).Name == "4 - The Blue Marble")
               return this.worldWindow.CurrentWorld.RenderableObjects.ChildObjects[i] as RenderableObject;
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

      #region Help

      private void toolStripMenuItemWeb_Click(object sender, EventArgs e)
      {
         MainForm.BrowseTo(MainForm.WebsiteUrl);
      }

      private void toolStripMenuItemWebForums_Click(object sender, EventArgs e)
      {
         MainForm.BrowseTo(MainForm.WebsiteForumsHelpUrl);
      }

      private void toolStripMenuItemWebDoc_Click(object sender, EventArgs e)
      {
         MainForm.BrowseTo(MainForm.WebsiteHelpUrl);
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
}
