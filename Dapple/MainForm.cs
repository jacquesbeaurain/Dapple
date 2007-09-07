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
using System.Net;

using WorldWind.Configuration;
using WorldWind.PluginEngine;
using Utility;
using Dapple.Properties;
using MontajRemote;
using System.Collections;

namespace Dapple
{
	public partial class MainForm : MainApplication
	{
		[DllImport("User32.dll")]
		private static extern UInt32 RegisterWindowMessageW(String strMessage);

      #region Delegates

      /// <summary>
      /// Called when this control selects a layer to tell others to load Metadata for it.
      /// </summary>
      /// <param name="?"></param>
      public delegate void ViewMetadataHandler(IBuilder oBuilder);

      #endregion

      #region Statics

      public static UInt32 OpenViewMessage = RegisterWindowMessageW("Dapple.OpenViewMessage");
		public const string ViewExt = ".dapple";
		public const string LinkExt = ".dapple_datasetlink";
		public const string LastView = "lastview" + ViewExt;
		public const string DefaultView = "default" + ViewExt;
		public const string ViewFileDescr = "Dapple View";
		public const string LinkFileDescr = "Dapple Link";
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

		private Splash splashScreen;
		private ServerTree tvServers;
		private static WorldWindow worldWindow;

      public static WorldWindow WorldWindowSingleton
      {
         get { return worldWindow; }
      }

		private NASA.Plugins.BMNG bmngPlugin;
		private NASA.Plugins.BmngLoader bmngLoader;
		private NASA.Plugins.ScaleBarLegend scalebarPlugin;

		private Murris.Plugins.Compass compassPlugin;
		private Murris.Plugins.GlobalClouds cloudsPlugin;
		private Murris.Plugins.SkyGradient skyPlugin;

		private Stars3D.Plugin.Stars3D starsPlugin;
		private ThreeDconnexion.Plugin.TDxWWInput threeDConnPlugin;

		private KMLPlugin.KMLImporter kmlPlugin;

		//private MeasureToolNewgen.Plugins.MeasureToolNG measurePlugin;

		private RenderableObjectList placeNames;

		private WorldWind.OverviewControl overviewCtl;
		private string openView = "";
		private string openGeoTiff = "";
		private string openGeoTiffName = "";
		private bool openGeoTiffTmp = false;
		private string lastView = "";
		private string metaviewerDir = "";
		private string strLayerToLoad = String.Empty;

		private MetadataDisplayThread m_oMetadataDisplay;

		Geosoft.GX.DAPGetData.GetDapError dapErrors;

		private DappleToolStripRenderer toolStripRenderer;
		/* private int iServerPanelLastMinSize, iServerPanelLastPos;
		private int iLayerPanelLastMinSize, iLayerPanelLastPos;
		private int iOverviewPanelLastMinSize, iOverviewPanelLastPos;
      
		*/

      private static ImageList m_oImageList = new ImageList();
      private static RemoteInterface m_oMontajRemoteInterface;
      private static GeographicBoundingBox m_oAoi;
      private Dictionary<String, GeographicBoundingBox> m_oCountryAOIs;
		#endregion

		#region Properties

		public MenuStrip MenuStrip
		{
			get
			{
				return this.menuStrip;
			}
		}

      private static bool IsMontajChildProcess
      {
         get { return m_oMontajRemoteInterface != null; }
      }

      public static ImageList DataTypeImageList
      {
         get { return m_oImageList; }
      }

      public static RemoteInterface MontajInterface
      {
         get { return m_oMontajRemoteInterface; }
      }

      /// <summary>
      /// Get the open map area of interest
      /// </summary>
      public static GeographicBoundingBox MapAoi
      {
         get { return m_oAoi; }
      }
		#endregion

		#region Constructor

		public MainForm(string strView, string strGeoTiff, string strGeotiffName, bool bGeotiffTmp, string strLastView, string strDatasetLink, RemoteInterface oMRI, GeographicBoundingBox oAoi)
		{
         worldWindow = new WorldWindow();
			if (String.Compare(Path.GetExtension(strView), ViewExt, true) == 0 && File.Exists(strView))
				this.openView = strView;
			this.openGeoTiff = strGeoTiff;
			this.openGeoTiffName = strGeotiffName;
			this.openGeoTiffTmp = bGeotiffTmp;
			this.lastView = strLastView;         
         m_oMontajRemoteInterface = oMRI;

			// Establish the version number string used for user display,
			// such as the Splash and Help->About screens.
			// To change the Application.ProductVersion make the
			// changes in \WorldWind\AssemblyInfo.cs
			// For alpha/beta versions, include " alphaN" or " betaN"
			// at the end of the format string.
			Version ver = new Version(Application.ProductVersion);
			Release = string.Format("{0}.{1}.{2}", ver.Major, ver.Minor, ver.Build);
			if (ver.Build % 2 != 0)
				Release += " (BETA)";

			// Name the main thread.
			System.Threading.Thread.CurrentThread.Name = "Main Thread";

			// Copy/Update any configuration files and other files if needed now
			CurrentSettingsDirectory = Path.Combine(UserPath, "Config");
			Directory.CreateDirectory(CurrentSettingsDirectory);
			Settings.CachePath = Path.Combine(UserPath, "Cache");
			Directory.CreateDirectory(Settings.CachePath);
			this.metaviewerDir = Path.Combine(UserPath, "Metadata");
			Directory.CreateDirectory(this.metaviewerDir);
			string[] cfgFiles = Directory.GetFiles(Path.Combine(DirectoryPath, "Config"), "*.xml");
			foreach (string strCfgFile in cfgFiles)
			{
				string strUserCfg = Path.Combine(CurrentSettingsDirectory, Path.GetFileName(strCfgFile));
				if (!File.Exists(strUserCfg))
					File.Copy(strCfgFile, strUserCfg);
			}
			string[] metaFiles = Directory.GetFiles(Path.Combine(Path.Combine(DirectoryPath, "Data"), "MetaViewer"), "*.*");
			foreach (string strMetaFile in metaFiles)
			{
				string strUserMeta = Path.Combine(this.metaviewerDir, Path.GetFileName(strMetaFile));
				File.Copy(strMetaFile, strUserMeta, true);
			}

			InitSettings();

			if (Settings.NewCachePath.Length > 0)
			{
				try
				{
					// We want to make sure the new cache path is writable
					Directory.CreateDirectory(Settings.NewCachePath);
					if (Directory.Exists(Settings.CachePath))
						Utility.FileSystem.DeleteFolderGUI(this, Settings.CachePath);
					Settings.CachePath = Settings.NewCachePath;
				}
				catch
				{
				}
				Settings.NewCachePath = "";
			}


			if (Settings.ConfigurationWizardAtStartup)
			{
				Wizard frm = new Wizard(Settings);
				frm.ShowDialog(this);
				Settings.ConfigurationWizardAtStartup = false;
			}

			if (Settings.ConfigurationWizardAtStartup)
			{
				// If the settings file doesn't exist, then we are using the
				// default settings, and the default is to show the Configuration
				// Wizard at startup. We only want that to happen the first time
				// World Wind is started, so change the setting to false(the user
				// can change it to true if they want).
				if (!File.Exists(Settings.FileName))
				{
					Settings.ConfigurationWizardAtStartup = false;
				}
				ConfigurationWizard.Wizard wizard = new ConfigurationWizard.Wizard(Settings);
				wizard.TopMost = true;
				wizard.ShowInTaskbar = true;
				wizard.ShowDialog();
				// TODO: should settings be saved now, in case of program crashes,
				//	   and so that XML file on disk matches in-memory settings?
			}

         Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);

			using (this.splashScreen = new Splash())
			{
				this.splashScreen.Owner = this;
				this.splashScreen.Show();
				this.splashScreen.SetText("Initializing...");

				Application.DoEvents();

            // --- setup the list of images used for the different datatypes ---

            m_oImageList.ColorDepth = ColorDepth.Depth32Bit;
            m_oImageList.ImageSize = new Size(16, 16);
            m_oImageList.TransparentColor = Color.Transparent;

            m_oImageList.Images.Add("enserver", Resources.enserver);
            m_oImageList.Images.Add("disserver", Resources.disserver);
            m_oImageList.Images.Add("offline", Resources.offline);
            m_oImageList.Images.Add("dap", Resources.dap);
            m_oImageList.Images.Add("dap_database", Resources.dap_database);
            m_oImageList.Images.Add("dap_document", Resources.dap_document);
            m_oImageList.Images.Add("dap_grid", Resources.dap_grid);
            m_oImageList.Images.Add("dap_map", Resources.dap_map);
            m_oImageList.Images.Add("dap_picture", Resources.dap_picture);
            m_oImageList.Images.Add("dap_point", Resources.dap_point);
            m_oImageList.Images.Add("dap_spf", Resources.dap_spf);
            m_oImageList.Images.Add("dap_voxel", Resources.dap_voxel);
            m_oImageList.Images.Add("folder", Resources.folder);
            m_oImageList.Images.Add("folder_open", Resources.folder_open);
            m_oImageList.Images.Add("loading", Resources.loading);
            m_oImageList.Images.Add("kml", Resources.kml);
            m_oImageList.Images.Add("dapple", global::Dapple.Properties.Resources.dapple);
            m_oImageList.Images.Add("dap_gray", global::Dapple.Properties.Resources.dap_gray);
            m_oImageList.Images.Add("error", global::Dapple.Properties.Resources.error);
            m_oImageList.Images.Add("folder_gray", global::Dapple.Properties.Resources.folder_gray);
            m_oImageList.Images.Add("layer", global::Dapple.Properties.Resources.layer);
            m_oImageList.Images.Add("live", global::Dapple.Properties.Resources.live);
            m_oImageList.Images.Add("tile", global::Dapple.Properties.Resources.tile);
            m_oImageList.Images.Add("tile_gray", global::Dapple.Properties.Resources.tile_gray);
            m_oImageList.Images.Add("georef_image", global::Dapple.Properties.Resources.georef_image);
            m_oImageList.Images.Add("time", global::Dapple.Properties.Resources.time_icon);
            m_oImageList.Images.Add("wms", Resources.wms);
            m_oImageList.Images.Add("wms_gray", global::Dapple.Properties.Resources.wms_gray);
            m_oImageList.Images.Add("nasa", global::Dapple.Properties.Resources.nasa);
            m_oImageList.Images.Add("usgs", global::Dapple.Properties.Resources.usgs);
            m_oImageList.Images.Add("worldwind_central", global::Dapple.Properties.Resources.worldwind_central);
            m_oImageList.Images.Add("arcims", global::Dapple.Properties.Resources.arcims);


				InitializeComponent();
				this.SuspendLayout();
				this.Icon = new System.Drawing.Icon(@"app.ico");
				this.toolStripRenderer = new DappleToolStripRenderer();
				toolStripServers.Renderer = this.toolStripRenderer;
				toolStripLayerLabel.Renderer = this.toolStripRenderer;
				toolStripOverview.Renderer = this.toolStripRenderer;
            cToolStripMetadata.Renderer = this.toolStripRenderer;

				// set Upper and Lower limits for Cache size control, in bytes
				long CacheUpperLimit = (long)Settings.CacheSizeGigaBytes * 1024L * 1024L * 1024L;
				long CacheLowerLimit = (long)Settings.CacheSizeGigaBytes * 768L * 1024L * 1024L;	//75% of upper limit

				try
				{
					Directory.CreateDirectory(Settings.CachePath);
				}
				catch
				{
					// We get here when people used a cache drive that since dissappeared (e.g. USB flash)
					// Revert to default cache directory in this case

					Settings.CachePath = Path.Combine(UserPath, "Cache");
					Directory.CreateDirectory(Settings.CachePath);
				}

				//Set up the cache
				worldWindow.Cache = new Cache(
					Settings.CachePath,
					CacheLowerLimit,
					CacheUpperLimit,
					Settings.CacheCleanupInterval,
					Settings.TotalRunTime);

				WorldWind.Net.WebDownload.Log404Errors = World.Settings.Log404Errors;

				#region Plugin + World Init.

				// register handler for extension 

				Registry.SetValue(Registry.CurrentUser + "\\Software\\Classes\\" + ViewExt, "", "Dapple View");
				Registry.SetValue(Registry.CurrentUser + "\\Software\\Classes\\Dapple View", "", "Dapple View");
				Registry.SetValue(Registry.CurrentUser + "\\Software\\Classes\\Dapple View\\Shell\\Open", "", "Open &" + ViewFileDescr);
				Registry.SetValue(Registry.CurrentUser + "\\Software\\Classes\\Dapple View\\Shell\\Open\\Command", "", "\"" + DirectoryPath + "\" \"%1\"");
				Registry.SetValue(Registry.CurrentUser + "\\Software\\Classes\\Dapple View\\DefaultIcon", "", Path.Combine(DirectoryPath, "app.ico"));

				this.dapErrors = new Geosoft.GX.DAPGetData.GetDapError(Path.Combine(Settings.CachePath, "DapErrors.log"));

				WorldWind.Terrain.TerrainTileService terrainTileService = new WorldWind.Terrain.TerrainTileService("http://worldwind25.arc.nasa.gov/wwelevation/wwelevation.aspx", "srtm30pluszip", 20, 150, "bil", 12, Path.Combine(Settings.CachePath, "Earth\\TerrainAccessor\\SRTM"), TimeSpan.FromMinutes(30), "Int16");
				WorldWind.Terrain.TerrainAccessor terrainAccessor = new WorldWind.Terrain.NltTerrainAccessor("SRTM", -180, -90, 180, 90, terrainTileService, null);

				WorldWind.World world = new WorldWind.World("Earth",
					new Point3d(0, 0, 0), Quaternion4d.RotationYawPitchRoll(0, 0, 0),
					(float)6378137,
					System.IO.Path.Combine(worldWindow.Cache.CacheDirectory, "Earth"),
					terrainAccessor);

				worldWindow.CurrentWorld = world;

				string strPluginsDir = Path.Combine(DirectoryPath, "Plugins");
				this.bmngLoader = new NASA.Plugins.BmngLoader();
				this.bmngLoader.PluginLoad(this, Path.Combine(strPluginsDir, "BlueMarble"));
				this.bmngPlugin = bmngLoader.BMNGForm;

				this.scalebarPlugin = new NASA.Plugins.ScaleBarLegend();
				this.scalebarPlugin.PluginLoad(this, strPluginsDir);

				this.starsPlugin = new Stars3D.Plugin.Stars3D();
				this.starsPlugin.PluginLoad(this, Path.Combine(strPluginsDir, "Stars3D"));

				this.compassPlugin = new Murris.Plugins.Compass();
				this.compassPlugin.PluginLoad(this, Path.Combine(strPluginsDir, "Compass"));

				this.cloudsPlugin = new Murris.Plugins.GlobalClouds();
				this.cloudsPlugin.PluginLoad(this, Path.Combine(strPluginsDir, "GlobalClouds"));

				this.skyPlugin = new Murris.Plugins.SkyGradient();
				this.skyPlugin.PluginLoad(this, Path.Combine(strPluginsDir, "SkyGradient"));

				this.threeDConnPlugin = new ThreeDconnexion.Plugin.TDxWWInput();
				this.threeDConnPlugin.PluginLoad(this, Path.Combine(strPluginsDir, "3DConnexion"));

				this.kmlPlugin = new KMLPlugin.KMLImporter();
				this.kmlPlugin.PluginLoad(this, strPluginsDir);

				//this.measurePlugin = new MeasureToolNewgen.Plugins.MeasureToolNG();
				//this.measurePlugin.PluginLoad(this, strPluginsDir); 
				
				try
				{
					this.placeNames = ConfigurationLoader.getRenderableFromLayerFile(Path.Combine(CurrentSettingsDirectory, "^Placenames.xml"), this.WorldWindow.CurrentWorld, this.WorldWindow.Cache, true, null);
				}
				catch
				{
					this.placeNames = null;
					this.showPlaceNamesToolStripMenuItem.Visible = false;
				}
				finally
				{
					this.placeNames.IsOn = World.Settings.ShowPlacenames;
					this.placeNames.RenderPriority = RenderPriority.Placenames;
					worldWindow.CurrentWorld.RenderableObjects.Add(this.placeNames);
					this.showPlaceNamesToolStripMenuItem.Checked = World.Settings.ShowPlacenames;
				}

				this.WorldResultsSplitPanel.Panel1.Controls.Add(worldWindow);
				worldWindow.Dock = DockStyle.Fill;

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
				this.toolStripMenuItemtileActivity.Checked = World.Settings.ShowDownloadIndicator;
				this.toolStripCrossHairs.Checked = World.Settings.ShowCrosshairs;
				this.toolStripMenuItemshowPosition.Checked = World.Settings.ShowPosition;
				this.toolStripMenuItemshowGridLines.Checked = World.Settings.ShowLatLonLines;
				this.globalCloudsToolStripMenuItem.Checked = World.Settings.ShowClouds;
				if (World.Settings.EnableSunShading)
				{
					if (!World.Settings.SunSynchedWithTime)
						this.enableSunShadingToolStripMenuItem.Checked = true;
					else
						this.syncSunShadingToTimeToolstripMenuItem.Checked = true;
				}
				else
					this.disableSunShadingToolStripMenuItem.Checked = true;
				this.atmosphericEffectsToolStripMenuItem.Checked = World.Settings.EnableAtmosphericScattering;

				this.toolStripMenuItemAskAtStartup.Checked = Settings.AskLastViewAtStartup;
				if (!Settings.AskLastViewAtStartup)
					this.toolStripMenuItemLoadLastView.Checked = Settings.LastViewAtStartup;


				#region OverviewPanel

				int i;
				for (i = 0; i < worldWindow.CurrentWorld.RenderableObjects.Count; i++)
				{
					if (((RenderableObject)worldWindow.CurrentWorld.RenderableObjects.ChildObjects[i]).Name == "4 - The Blue Marble")
						break;
				}

				this.overviewCtl = new OverviewControl(Settings.DataPath + @"\Earth\BmngBathy\world.topo.bathy.200407.jpg", worldWindow, panelOverview);
				this.overviewCtl.Dock = DockStyle.Fill;
				this.panelOverview.Controls.Add(this.overviewCtl);

				#endregion


				worldWindow.MouseEnter += new EventHandler(this.worldWindow_MouseEnter);
				worldWindow.MouseLeave += new EventHandler(this.worldWindow_MouseLeave);

				worldWindow.ClearDevice();

            #region Views

            this.tvServers = new ServerTree(m_oImageList, Settings.CachePath, this, cLayerList);
            cServerListControl.ImageList = this.tvServers.ImageList;
            cLayerList.ImageList = this.tvServers.ImageList;

            m_oMetadataDisplay = new MetadataDisplayThread(this);
            cServerListControl.LayerList = cLayerList;
            cLayerList.ViewMetadata += new ViewMetadataHandler(m_oMetadataDisplay.addBuilder);
            cLayerList.GoTo += new LayerList.GoToHandler(this.GoTo);
            tvServers.ViewMetadata += new ViewMetadataHandler(m_oMetadataDisplay.addBuilder);
            cServerListControl.ViewMetadata += new ViewMetadataHandler(m_oMetadataDisplay.addBuilder);

            this.tvServers.AfterSelect += new TreeViewEventHandler(this.ServerTreeAfterSelected);
				this.tvServers.RMBContextMenuStrip = this.contextMenuStripServers;
				this.tvServers.Dock = System.Windows.Forms.DockStyle.Fill;
				this.tvServers.ImageIndex = 0;
				this.tvServers.Location = new System.Drawing.Point(0, 0);
				this.tvServers.Name = "treeViewServers";
				this.tvServers.SelectedImageIndex = 0;
				this.tvServers.Size = new System.Drawing.Size(245, 240);
				this.tvServers.TabIndex = 0;

				this.cServerTreeView.SuspendLayout();

            this.cServerTreeView.Controls.Add(this.tvServers);

            this.cServerTreeView.ResumeLayout(false);
				this.ResumeLayout(false);
            this.cServerTreeView.PerformLayout();

				#endregion

				this.PerformLayout();
				strLayerToLoad = strDatasetLink;


				while (!this.splashScreen.IsDone)
					System.Threading.Thread.Sleep(50);

				// Force initial render to avoid showing random contents of frame buffer to user.
				worldWindow.Render();
				WorldWindow.Focus();

            #region OM Forked Process configuration

            if (IsMontajChildProcess)
            {
               cLayerList.OMFeaturesEnabled = true;
               this.Text = "Find Data";
               m_oAoi = oAoi;

               // Put the view menu after the first two
               menuStrip.Items.Remove(this.toolStripMenuItemoptions);
               menuStrip.Items.Add(this.toolStripMenuItemoptions);
               cOMToolsMenu.Visible = true;
               cOMServerMenu.Visible = true;
               toolStripMenuItemfile.Visible = false;
               toolStripMenuItemedit.Visible = false;
               toolStripMenuItemhelp.Visible = false;
            }
            else
            {
               downloadToolStripMenuItem.Enabled = false;

               // register handler for dataset links

               Registry.SetValue(Registry.CurrentUser + "\\Software\\Classes\\Mime\\Database\\Content Type\\text/Geosoft_datasetlink", "Extension", LinkExt);
               Registry.SetValue(Registry.CurrentUser + "\\Software\\Classes\\" + LinkExt, "", "Dapple DataSetLink");
               Registry.SetValue(Registry.CurrentUser + "\\Software\\Classes\\Dapple DataSetLink", "", "Geosoft Dapple Dataset Link XML");
               Registry.SetValue(Registry.CurrentUser + "\\Software\\Classes\\Dapple DataSetLink\\Shell\\Open", "", "Open Dapple Dataset Link");
               Registry.SetValue(Registry.CurrentUser + "\\Software\\Classes\\Dapple DataSetLink\\Shell\\Open\\Command", "", "\"" + AppDomain.CurrentDomain.BaseDirectory + "Dapple.exe\" -datasetlink=\"%1\"");
               Registry.SetValue(Registry.CurrentUser + "\\Software\\Classes\\Dapple DataSetLink\\DefaultIcon", "", Path.Combine(DirectoryPath, "app.ico"));
            }

            #endregion

            loadCountryList();
            populateAoiComboBox();
         }
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

				using (StreamReader sr = new StreamReader(Path.Combine(Path.GetDirectoryName(DirectoryPath), VersionFile)))
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
			catch (System.Net.WebException)
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

		#region World Window Events

		void worldWindow_MouseLeave(object sender, EventArgs e)
		{
			splitContainerMain.Panel1.Select();
		}

		void worldWindow_MouseEnter(object sender, EventArgs e)
		{
			worldWindow.Select();
		}

		#endregion

		#region Context menus and buttons

		#region Opening

		private void contextMenuStripServers_Opening(object sender, CancelEventArgs e)
		{
			e.Cancel = false;
			if (this.tvServers.SelectedNode == null || this.tvServers.SelectedNode.Nodes == this.tvServers.TileRootNodes)
			{
				e.Cancel = true;
				return;
			}

			this.toolStripMenuItemAddLayer.Tag = false;
			this.toolStripMenuItemaddServer.Tag = false;
			this.toolStripMenuItemgoToServer.Tag = false;
			this.toolStripMenuItemremoveServer.Tag = false;
			this.toolStripMenuItemviewMetadataServer.Tag = false;
			this.toolStripMenuItemServerLegend.Tag = false;
			this.toolStripMenuItemviewMetadataServer.Tag = false;
			this.toolStripMenuItemServerLegend.Tag = false;
			this.toolStripMenuItempropertiesServer.Tag = false;
			this.toolStripMenuItemRefreshCatalog.Tag = false;

			if (this.tvServers.SelectedNode.Nodes == this.tvServers.DAPRootNodes)
			{
				this.toolStripMenuItemaddServer.Text = "Add DAP Server";
				this.toolStripMenuItemaddServer.Tag = true;
			}
         else if (this.tvServers.SelectedNode.Nodes == this.tvServers.WMSRootNodes)
			{
				this.toolStripMenuItemaddServer.Text = "Add WMS Server";
				this.toolStripMenuItemaddServer.Tag = true;
			}
         else if (this.tvServers.SelectedNode.Nodes == this.tvServers.ArcIMSRootNodes)
         {
            this.toolStripMenuItemaddServer.Text = "Add ArcIMS Server";
            this.toolStripMenuItemaddServer.Tag = true;
         }
         else if (this.tvServers.SelectedNode.Tag is Geosoft.GX.DAPGetData.Server)
         {
            this.toolStripMenuItemRefreshCatalog.Tag = true;
            this.toolStripMenuItemremoveServer.Tag = true;
         }
         else if (this.tvServers.SelectedNode.Tag != null)
         {
            IBuilder builder = null;
            Geosoft.Dap.Common.DataSet dapDataset = null;

            if (this.tvServers.SelectedNode.Tag is IBuilder)
               builder = this.tvServers.SelectedNode.Tag as IBuilder;
            if (this.tvServers.SelectedNode.Tag is Geosoft.Dap.Common.DataSet)
               dapDataset = this.tvServers.SelectedDAPDataset;

            if (builder is LayerBuilder || dapDataset != null)
            {
               this.toolStripMenuItemAddLayer.Tag = true;
               this.toolStripMenuItemgoToServer.Tag = true;
            }

            if (!(dapDataset != null || builder == null || builder is WMSQuadLayerBuilder || builder is VEQuadLayerBuilder || builder is NltQuadLayerBuilder || builder is ArcIMSQuadLayerBuilder || (builder is BuilderDirectory && !(builder as BuilderDirectory).Removable)))
               this.toolStripMenuItemremoveServer.Tag = true;

            if (builder != null)
            {
               this.toolStripMenuItemviewMetadataServer.Tag = builder.SupportsMetaData || builder is LayerBuilder;
               this.toolStripMenuItemServerLegend.Tag = builder is LayerBuilder;
               this.toolStripMenuItemviewMetadataServer.Enabled = builder.SupportsMetaData;
               this.toolStripMenuItemServerLegend.Enabled = (builder is LayerBuilder) && (builder as LayerBuilder).SupportsLegend;

               if (builder is WMSServerBuilder || builder is ArcIMSServerBuilder)
                  this.toolStripMenuItemRefreshCatalog.Tag = true;
            }

            if (dapDataset != null)
            {
               this.toolStripMenuItemviewMetadataServer.Tag = true;
               this.toolStripMenuItemviewMetadataServer.Enabled = true;
            }
         }

			// Filter Separators
			bool bAnyVisible = false;
			int iTotalVisible = 0;
			ToolStripSeparator lastSeparator = null;

			this.toolStripSeparatorServerAdd.Visible = true;
			this.toolStripSeparatorServerGoto.Visible = true;
			this.toolStripSeparatorServerRemove.Visible = true;
			this.toolStripSeparatorRefreshCatalog.Visible = true;


			for (int i = 0; i < this.contextMenuStripServers.Items.Count; i++)
			{
				if (this.contextMenuStripServers.Items[i] is ToolStripSeparator)
				{
					if (!bAnyVisible)
						this.contextMenuStripServers.Items[i].Visible = false;
					lastSeparator = this.contextMenuStripServers.Items[i] as ToolStripSeparator;
					bAnyVisible = false;
				}
				else
				{
					if ((bool)this.contextMenuStripServers.Items[i].Tag)
					{
						iTotalVisible++;
						this.contextMenuStripServers.Items[i].Visible = true;
						bAnyVisible = true;
					}
					else
						this.contextMenuStripServers.Items[i].Visible = false;
				}
			}
			if (iTotalVisible > 0)
			{
				if (iTotalVisible == 1)
				{
					// Hide all separators
					for (int i = 0; i < this.contextMenuStripServers.Items.Count; i++)
					{
						if (this.contextMenuStripServers.Items[i] is ToolStripSeparator)
							this.contextMenuStripServers.Items[i].Visible = false;
					}
				}
				else if (lastSeparator != null && !bAnyVisible)
					lastSeparator.Visible = false;
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
			worldWindow.DrawArgs.WorldCamera.SlerpPercentage = World.Settings.CameraSlerpInertia;
			worldWindow.DrawArgs.WorldCamera.SetPosition(
					 worldWindow.Latitude,
					 worldWindow.Longitude,
					  worldWindow.DrawArgs.WorldCamera.Heading.Degrees,
					  worldWindow.DrawArgs.WorldCamera.Altitude,
					  0);

		}

		private void toolStripButtonRestoreNorth_Click(object sender, EventArgs e)
		{
			worldWindow.DrawArgs.WorldCamera.SlerpPercentage = World.Settings.CameraSlerpInertia;
			worldWindow.DrawArgs.WorldCamera.SetPosition(
					 worldWindow.Latitude,
					 worldWindow.Longitude,
					  0,
					  worldWindow.DrawArgs.WorldCamera.Altitude,
					  worldWindow.DrawArgs.WorldCamera.Tilt.Degrees);
		}

		private void toolStripButtonResetCamera_Click(object sender, EventArgs e)
		{
			worldWindow.DrawArgs.WorldCamera.SlerpPercentage = World.Settings.CameraSlerpInertia;
			worldWindow.DrawArgs.WorldCamera.Reset();
		}


		private void timerNavigation_Tick(object sender, EventArgs e)
		{
			this.bNavTimer = true;
			worldWindow.DrawArgs.WorldCamera.SlerpPercentage = 1.0;
			switch (this.eNavMode)
			{
				case NavMode.ZoomIn:
					worldWindow.DrawArgs.WorldCamera.Zoom(0.2f);
					return;
				case NavMode.ZoomOut:
					worldWindow.DrawArgs.WorldCamera.Zoom(-0.2f);
					return;
				case NavMode.RotateLeft:
					Angle rotateClockwise = Angle.FromRadians(-0.01f);
					worldWindow.DrawArgs.WorldCamera.Heading += rotateClockwise;
					worldWindow.DrawArgs.WorldCamera.RotationYawPitchRoll(Angle.Zero, Angle.Zero, rotateClockwise);
					return;
				case NavMode.RotateRight:
					Angle rotateCounterclockwise = Angle.FromRadians(0.01f);
					worldWindow.DrawArgs.WorldCamera.Heading += rotateCounterclockwise;
					worldWindow.DrawArgs.WorldCamera.RotationYawPitchRoll(Angle.Zero, Angle.Zero, rotateCounterclockwise);
					return;
				case NavMode.TiltUp:
					worldWindow.DrawArgs.WorldCamera.Tilt += Angle.FromDegrees(-1.0f);
					return;
				case NavMode.TiltDown:
					worldWindow.DrawArgs.WorldCamera.Tilt += Angle.FromDegrees(1.0f);
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
				worldWindow.DrawArgs.WorldCamera.SlerpPercentage = World.Settings.CameraSlerpInertia;
				worldWindow.DrawArgs.WorldCamera.Zoom(2.0f);
			}
			else
				this.bNavTimer = false;
		}

		private void toolStripButtonZoomOut_Click(object sender, EventArgs e)
		{
			this.timerNavigation.Enabled = false;
			if (!this.bNavTimer)
			{
				worldWindow.DrawArgs.WorldCamera.SlerpPercentage = World.Settings.CameraSlerpInertia;
				worldWindow.DrawArgs.WorldCamera.Zoom(-2.0f);
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
				worldWindow.DrawArgs.WorldCamera.SlerpPercentage = World.Settings.CameraSlerpInertia;
				worldWindow.DrawArgs.WorldCamera.Heading += rotateClockwise;
				worldWindow.DrawArgs.WorldCamera.RotationYawPitchRoll(Angle.Zero, Angle.Zero, rotateClockwise);
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
				worldWindow.DrawArgs.WorldCamera.SlerpPercentage = World.Settings.CameraSlerpInertia;
				worldWindow.DrawArgs.WorldCamera.Heading += rotateCounterclockwise;
				worldWindow.DrawArgs.WorldCamera.RotationYawPitchRoll(Angle.Zero, Angle.Zero, rotateCounterclockwise);
			}
			else
				this.bNavTimer = false;
		}

		private void toolStripButtonTiltUp_Click(object sender, EventArgs e)
		{
			this.timerNavigation.Enabled = false;
			if (!this.bNavTimer)
			{
				worldWindow.DrawArgs.WorldCamera.SlerpPercentage = World.Settings.CameraSlerpInertia;
				worldWindow.DrawArgs.WorldCamera.Tilt += Angle.FromDegrees(-10.0f);
			}
			else
				this.bNavTimer = false;
		}

		private void toolStripButtonTiltDown_Click(object sender, EventArgs e)
		{
			this.timerNavigation.Enabled = false;
			if (!this.bNavTimer)
			{
				worldWindow.DrawArgs.WorldCamera.SlerpPercentage = World.Settings.CameraSlerpInertia;
				worldWindow.DrawArgs.WorldCamera.Tilt += Angle.FromDegrees(10.0f);
			}
			else
				this.bNavTimer = false;
		}

		#endregion

		#region Add Layers

		private void toolStripMenuItemOpen_Click(object sender, EventArgs e)
		{
			string strLastFolderCfg = Path.Combine(Settings.ConfigPath, "opengeotif.cfg");

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
			LayerBuilder builder = new GeorefImageLayerBuilder(strGeoTiffName, strGeoTiff, bTmp, worldWindow, null);

			Cursor = Cursors.WaitCursor;
			if (builder.GetLayer() != null)
			{
				Cursor = Cursors.Default;

				// If the file is already there remove it 
            cLayerList.CmdRemoveGeoTiff(strGeoTiff);

				// If there is already a layer by that name find unique name
				if (strGeoTiffName.Length > 0)
				{
					int iCount = 0;
					string strNewName = strGeoTiffName;
					bool bExist = true;
					while (bExist)
					{
						bExist = false;
						foreach (LayerBuilderContainer container in cLayerList.AllLayers)
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

            builder.Opacity = 255;
            builder.Visible = true;
            builder.Temporary = bTmp;

            cLayerList.AddLayer(builder);

				if (bGoto)
					GoTo(builder);
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
         AddDatasetAction();
		}

		public void AddLayerBuilder(LayerBuilder oLayer)
		{
         cLayerList.AddLayer(oLayer);

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
               if (this.tvServers.AddDAPServer(dlg.Url, out oServer))
               {
                  Thread t = new Thread(new ParameterizedThreadStart(submitServerToSearchEngine));
                  t.Name = "Background add server thread";
                  t.Start(new String[] { dlg.Url, "DAP" });
               }
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
			   AddWMS dlg = new AddWMS(worldWindow, treeNode.Tag as WMSCatalogBuilder);
			   if (dlg.ShowDialog(this) == DialogResult.OK)
			   {
				  try
				  {
                 if (this.tvServers.AddWMSServer(dlg.WmsURL, true))
                 {
                    Thread t = new Thread(new ParameterizedThreadStart(submitServerToSearchEngine));
                    t.Name = "Background add server thread";
                    t.Start(new String[] { dlg.WmsURL, "WMS" });
                 }
				  }
				  catch (Exception except)
				  {
					 MessageBox.Show(this, "Error adding server \"" + dlg.WmsURL + "\" (" + except.Message + ")", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
				  }
				  SaveLastView();
			   }
			}
		}

      private void AddArcIMSServer()
      {
         TreeNode treeNode = TreeUtils.FindNodeOfTypeBFS(typeof(ArcIMSCatalogBuilder), this.tvServers.Nodes);

         if (treeNode != null)
         {
            AddArcIMS dlg = new AddArcIMS(worldWindow, treeNode.Tag as ArcIMSCatalogBuilder);
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
               try
               {
                  if (this.tvServers.AddArcIMSServer(new ArcIMSServerUri(dlg.URL), true))
                  {
                     Thread t = new Thread(new ParameterizedThreadStart(submitServerToSearchEngine));
                     t.Name = "Background add server thread";
                     t.Start(new String[] { dlg.URL, "ArcIMS" });
                  }
               }
               catch (Exception except)
               {
                  MessageBox.Show(this, "Error adding server \"" + dlg.URL + "\" (" + except.Message + ")", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
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

      private void addAnArcIMSServerToolStripMenuItem_Click(object sender, EventArgs e)
      {
         AddArcIMSServer();
      }

		void toolStripMenuItemaddServer_Click(object sender, EventArgs e)
		{
         if (this.tvServers.SelectedNode.Nodes == this.tvServers.DAPRootNodes)
            AddDAPServer();
         else if (this.tvServers.SelectedNode.Nodes == this.tvServers.WMSRootNodes)
            AddWMSServer();
         else if (this.tvServers.SelectedNode.Nodes == this.tvServers.ArcIMSRootNodes)
            AddArcIMSServer();
		}

		#endregion

		#region Remove Items

		private void toolStripMenuItemremoveServer_Click(object sender, EventArgs e)
		{
			this.tvServers.RemoveCurrentServer();
			SaveLastView();
		}


		#endregion

		#region Go To

		private void toolStripMenuItemgoToServer_Click(object sender, EventArgs e)
		{
			if (this.tvServers.SelectedNode != null)
			{
				if (this.tvServers.SelectedNode.Tag is LayerBuilder)
					GoTo(this.tvServers.SelectedNode.Tag as LayerBuilder);
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

      void GoTo(LayerBuilder builder)
		{
			GoTo(builder.Extents, (builder is NltQuadLayerBuilder) ? (builder as NltQuadLayerBuilder).LevelZeroTileSize : -1.0);
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
				latitude = worldWindow.Latitude;
				longitude = worldWindow.Longitude;
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
				worldWindow.GotoLatLonHeadingViewRange(latitude, longitude, 0, fov);
			else
			{
				worldWindow.GotoLatLonAltitude(latitude, longitude, overviewCameraAlt);
			}
		}

		#endregion

		#region Refresh

		private void toolStripMenuItemRefreshCatalog_Click(object sender, EventArgs e)
		{
			this.tvServers.RefreshCurrentServer();
		}

		#endregion

		#region Metadata, legend and properties

		/*private void toolStripMenuItemGetLegend_Click(object sender, EventArgs e)
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
		}*/

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

		/*private void toolStripMenuItemviewMetadataServer_Click(object sender, EventArgs e)
		{
			if (this.tvServers.SelectedNode != null && this.tvServers.SelectedNode.Tag is IBuilder)
				ViewMetadata(this.tvServers.SelectedNode.Tag as IBuilder);
			else
				ViewMetadata(null);
		}*/

		/*private void toolStripMenuItempropertiesServer_Click(object sender, EventArgs e)
		{
			if (this.tvServers.SelectedNode != null && this.tvServers.SelectedNode.Tag is IBuilder)
				ViewProperties(this.tvServers.SelectedNode.Tag as IBuilder);
		}*/

		#endregion

		#endregion

		#region Main Menu Item Click events

		private void toolStripMenuItemAskAtStartup_Click(object sender, EventArgs e)
		{
			toolStripMenuItemLoadLastView.Checked = false;
			Settings.AskLastViewAtStartup = toolStripMenuItemAskAtStartup.Checked;
		}

		private void toolStripMenuItemLoadLastView_Click(object sender, EventArgs e)
		{
			Settings.AskLastViewAtStartup = false;
			this.toolStripMenuItemAskAtStartup.Checked = false;
			Settings.LastViewAtStartup = toolStripMenuItemLoadLastView.Checked;
		}

		private void toolStripMenuItemadvancedSettings_Click(object sender, EventArgs e)
		{
			Wizard wiz = new Wizard(Settings);
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
			worldWindow.Invalidate();
		}

		private void toolStripMenuItemcompass_Click(object sender, EventArgs e)
		{
			World.Settings.ShowCompass = toolStripMenuItemcompass.Checked;
			this.compassPlugin.Layer.IsOn = World.Settings.ShowCompass;
		}

		private void toolStripMenuItemtileActivity_Click(object sender, EventArgs e)
		{
			World.Settings.ShowDownloadIndicator = toolStripMenuItemtileActivity.Checked;
		}

		private void toolStripCrossHairs_Click(object sender, EventArgs e)
		{
			World.Settings.ShowCrosshairs = toolStripCrossHairs.Checked;
		}

		private void toolStripMenuItemOpenSaved_Click(object sender, EventArgs e)
		{
			ViewOpenDialog dlgtest = new ViewOpenDialog(Settings.ConfigPath);
			DialogResult res = dlgtest.ShowDialog(this);
			if (dlgtest.ViewFile != null)
			{
				if (res == DialogResult.OK)
					OpenView(dlgtest.ViewFile, true);
			}
		}

		private void toolStripMenuItemResetDefaultView_Click(object sender, EventArgs e)
		{
			OpenView(Path.Combine(Settings.DataPath, DefaultView), true);
		}

		private void MainForm_Shown(object sender, EventArgs e)
		{
			// Render once to not just show the atmosphere at startup (looks better) ---
			worldWindow.SafeRender();

         if (IsMontajChildProcess)
            OpenView(Path.Combine(Settings.DataPath, DefaultView), false, false);
         else if (this.openView.Length > 0)
            OpenView(openView, this.openGeoTiff.Length == 0);
         else if (this.openGeoTiff.Length == 0 && File.Exists(Path.Combine(Settings.ConfigPath, LastView)))
         {
            if (Settings.AskLastViewAtStartup)
            {
               Utils.MessageBoxExLib.MessageBoxEx msgBox = Utils.MessageBoxExLib.MessageBoxExManager.CreateMessageBox(null);
               msgBox.AllowSaveResponse = true;
               msgBox.SaveResponseText = "Don't ask me again";
               msgBox.Caption = this.Text;
               msgBox.Icon = Utils.MessageBoxExLib.MessageBoxExIcon.Question;
               msgBox.AddButtons(MessageBoxButtons.YesNo);
               msgBox.Text = "Would you like to open your last View?";
               msgBox.Font = this.Font;
               Settings.LastViewAtStartup = msgBox.Show() == Utils.MessageBoxExLib.MessageBoxExResult.Yes;
               if (msgBox.SaveResponse)
                  Settings.AskLastViewAtStartup = false;
            }

            if (Settings.LastViewAtStartup)
               OpenView(Path.Combine(Settings.ConfigPath, LastView), true);
            else
               OpenView(Path.Combine(Settings.ConfigPath, LastView), false, false);
         }
         else
            OpenView(Path.Combine(Settings.DataPath, DefaultView), this.openGeoTiff.Length == 0);

			if (this.openGeoTiff.Length > 0)
				AddGeoTiff(this.openGeoTiff, this.openGeoTiffName, this.openGeoTiffTmp, true);


			// Check for updates daily
			if (Settings.UpdateCheckDate.Date != System.DateTime.Now.Date)
				CheckForUpdates(false);
			Settings.UpdateCheckDate = System.DateTime.Now;

			foreach (RenderableObject oRO in worldWindow.CurrentWorld.RenderableObjects.ChildObjects)
			{
				if (oRO.Name == "1 - Grid Lines")
				{
					oRO.IsOn = World.Settings.ShowLatLonLines;
					break;
				}
			}

			// Load datasetlink, now that everything is laid out
			if (strLayerToLoad.Length > 0)
				OpenDatasetLink(strLayerToLoad);

         if (m_oAoi != null)
            GoTo(m_oAoi, 0);
		}

		bool m_bSizing = false;
		private void MainForm_ResizeBegin(object sender, EventArgs e)
		{
			m_bSizing = true;
			worldWindow.Visible = false;
		}

		private void MainForm_ResizeEnd(object sender, EventArgs e)
		{
			m_bSizing = false;
			worldWindow.Visible = true;
			worldWindow.SafeRender();
		}

		private void MainForm_Resize(object sender, EventArgs e)
		{
			worldWindow.Visible = false;
		}

		private void MainForm_SizeChanged(object sender, EventArgs e)
		{
			if (!m_bSizing)
			{
				worldWindow.Visible = true;
				worldWindow.SafeRender();
			}
		}

		private void splitContainerMain_SplitterMoving(object sender, SplitterCancelEventArgs e)
		{
			worldWindow.Visible = false;
		}

		private void splitContainerMain_SplitterMoved(object sender, SplitterEventArgs e)
		{
			if (!m_bSizing)
			{
				worldWindow.Visible = true;
				worldWindow.SafeRender();
				if (this.overviewCtl != null)
					this.overviewCtl.Refresh();
			}
		}

      private void cWorldMetadataSplitter_SplitterMoving(object sender, SplitterCancelEventArgs e)
      {
         worldWindow.Visible = false;
      }

      private void cWorldMetadataSplitter_SplitterMoved(object sender, SplitterEventArgs e)
      {
         worldWindow.Visible = true;
         worldWindow.SafeRender();
      }

      private void WorldResultsSplitPanel_Panel1_Resize(object sender, EventArgs e)
      {
         CenterNavigationToolStrip();
      }


		private void toolStripMenuItemeditBlueMarble_Click(object sender, EventArgs e)
		{
			this.bmngPlugin.ShowDialog(this);
		}

		private void toolStripMenuItemshowGridLines_Click(object sender, EventArgs e)
		{
			World.Settings.ShowLatLonLines = toolStripMenuItemshowGridLines.Checked;
			foreach (RenderableObject oRO in worldWindow.CurrentWorld.RenderableObjects.ChildObjects)
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
			worldWindow.Invalidate();
		}

		private void toolStripMenuItemsave_Click(object sender, EventArgs e)
		{
			string tempViewFile = Path.ChangeExtension(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()), ViewExt);
			string tempFile = Path.ChangeExtension(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()), ".jpg");
			SaveCurrentView(tempViewFile, tempFile, "");
			Image img = Image.FromFile(tempFile);
			SaveViewForm form = new SaveViewForm(Settings.ConfigPath, img);
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
			string strMailApp = Path.Combine(Path.Combine(Path.GetDirectoryName(DirectoryPath), "System"), "mailer.exe");

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

		private void showPlaceNamesToolStripMenuItem_Click(object sender, EventArgs e)
		{
			World.Settings.ShowPlacenames = !World.Settings.ShowPlacenames;
			this.showPlaceNamesToolStripMenuItem.Checked = World.Settings.ShowPlacenames;
			this.placeNames.IsOn = World.Settings.ShowPlacenames;
		}

		private void scaleBarToolStripMenuItem_Click(object sender, EventArgs e)
		{
			this.scalebarPlugin.IsVisible = !this.scalebarPlugin.IsVisible;
			this.scaleBarToolStripMenuItem.Checked = this.scalebarPlugin.IsVisible;
		}

		private void enableSunShadingToolStripMenuItem_Click(object sender, EventArgs e)
		{
			World.Settings.EnableSunShading = true;
			World.Settings.SunSynchedWithTime = false;
			this.enableSunShadingToolStripMenuItem.Checked = true;
			this.syncSunShadingToTimeToolstripMenuItem.Checked = false;
			this.disableSunShadingToolStripMenuItem.Checked = false;
		}

		private void syncSunShadingToTimeToolstripMenuItem_Click(object sender, EventArgs e)
		{
			World.Settings.EnableSunShading = true;
			World.Settings.SunSynchedWithTime = true;
			this.enableSunShadingToolStripMenuItem.Checked = false;
			this.syncSunShadingToTimeToolstripMenuItem.Checked = true;
			this.disableSunShadingToolStripMenuItem.Checked = false;
		}

		private void disableSunShadingToolStripMenuItem_Click(object sender, EventArgs e)
		{
			World.Settings.EnableSunShading = false;
			World.Settings.SunSynchedWithTime = false;
			this.enableSunShadingToolStripMenuItem.Checked = false;
			this.syncSunShadingToTimeToolstripMenuItem.Checked = false;
			this.disableSunShadingToolStripMenuItem.Checked = true;
		}

		private void atmosphericEffectsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			World.Settings.EnableAtmosphericScattering = !World.Settings.EnableAtmosphericScattering;
			this.atmosphericEffectsToolStripMenuItem.Checked = World.Settings.EnableAtmosphericScattering;
		}

		private void globalCloudsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			World.Settings.ShowClouds = !World.Settings.ShowClouds;
			this.globalCloudsToolStripMenuItem.Checked = World.Settings.ShowClouds;
			this.cloudsPlugin.layer.IsOn = World.Settings.ShowClouds;
		}

		private void toolStripMenuItemexit_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void toolStripMenuItemoptions_DropDownOpening(object sender, EventArgs e)
		{
			this.scaleBarToolStripMenuItem.Checked = this.scalebarPlugin.IsVisible;
		}

      void ServerTreeAfterSelected(object sender, TreeViewEventArgs e)
      {
         populateAoiComboBox();
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

				ProcessStartInfo psi = new ProcessStartInfo(Path.GetDirectoryName(DirectoryPath) + @"\System\geotifcp.exe");
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

			foreach (LayerBuilderContainer container in cLayerList.AllLayers)
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
			foreach (LayerBuilderContainer container in cLayerList.AllLayers)
			{
				if (container.Visible && container.Builder != null)
				{
					RenderableObject ro = container.Builder.GetLayer();
					if (ro != null)
					{
						RenderableObject.ExportInfo expinfo = new RenderableObject.ExportInfo();
						ro.InitExportInfo(worldWindow.DrawArgs, expinfo);

						if (expinfo.iPixelsX > 0 && expinfo.iPixelsY > 0)
							expList.Add(new ExportEntry(container, ro, expinfo));
					}
				}
			}
			if (roBMNG != null && roBMNG.IsOn)
			{
				RenderableObject.ExportInfo expinfo = new RenderableObject.ExportInfo();
				roBMNG.InitExportInfo(worldWindow.DrawArgs, expinfo);
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

			WorldWind.Camera.MomentumCamera camera = worldWindow.DrawArgs.WorldCamera as WorldWind.Camera.MomentumCamera;
			if (camera.Tilt.Degrees > 5.0)
			{
				MessageBox.Show(this, "It is not possible to export a tilted view. Reset the tilt using the navigation buttons\nor by using Right-Mouse-Button and drag and try again.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			try
			{
				ExportView dlg = new ExportView(Settings.ConfigPath);
				if (dlg.ShowDialog(this) == DialogResult.OK)
				{
					Cursor = Cursors.WaitCursor;

					// Stop the camera
					camera.SetPosition(camera.Latitude.Degrees, camera.Longitude.Degrees, camera.Heading.Degrees, camera.Altitude, camera.Tilt.Degrees);

					// Determine output parameters
					GeographicBoundingBox geoExtent = GeographicBoundingBox.FromQuad(worldWindow.GetViewBox(true));
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
								// Limit info for layer to area we are looking at

								double dExpXRes = (double)exp.Info.iPixelsX / (exp.Info.dMaxLon - exp.Info.dMinLon);
								double dExpYRes = (double)exp.Info.iPixelsY / (exp.Info.dMaxLat - exp.Info.dMinLat);
								int iExpOffsetX = (int)Math.Round((geoExtent.West - exp.Info.dMinLon) * dExpXRes);
								int iExpOffsetY = (int)Math.Round((exp.Info.dMaxLat - geoExtent.North) * dExpYRes);
								exp.Info.iPixelsX = (int)Math.Round((geoExtent.East - geoExtent.West) * dExpXRes);
								exp.Info.iPixelsY = (int)Math.Round((geoExtent.North - geoExtent.South) * dExpYRes);
								exp.Info.dMinLon = exp.Info.dMinLon + (double)iExpOffsetX / dExpXRes;
								exp.Info.dMaxLat = exp.Info.dMaxLat - (double)iExpOffsetY / dExpYRes;
								exp.Info.dMaxLon = exp.Info.dMinLon + (double)exp.Info.iPixelsX / dExpXRes;
								exp.Info.dMinLat = exp.Info.dMaxLat - (double)exp.Info.iPixelsY / dExpYRes;

								using (Bitmap bitMap = new Bitmap(exp.Info.iPixelsX, exp.Info.iPixelsY))
								{
									int iOffsetX, iOffsetY;
									int iWidth, iHeight;

									using (exp.Info.gr = Graphics.FromImage(bitMap))
										exp.RO.ExportProcess(worldWindow.DrawArgs, exp.Info);

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
		public static int ImageListIndex(string strKey)
		{
         return m_oImageList.Images.IndexOfKey(strKey);
		}

		/// <summary>
		/// Returns imagelist index from dap dataset type
		/// </summary>
		/// <param name="strType"></param>
		/// <returns></returns>
      public static int ImageIndex(string strType)
		{
         Int32 iRet = 3;

         switch (strType.ToLower())
         {
            case "database":
               iRet = ImageListIndex("dap_database");
               break;
            case "document":
               iRet = ImageListIndex("dap_document");
               break;
            case "generic":
               iRet = ImageListIndex("dap_map");
               break;
            case "grid":
               iRet = ImageListIndex("dap_grid");
               break;
            case "map":
               iRet = ImageListIndex("dap_map");
               break;
            case "picture":
               iRet = ImageListIndex("dap_picture");
               break;
            case "point":
               iRet = ImageListIndex("dap_point");
               break;
            case "spf":
               iRet = ImageListIndex("dap_spf");
               break;
            case "voxel":
               iRet = ImageListIndex("dap_voxel");
               break;
         }
         return iRet;         
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

      private void loadCountryList()
      {
         if (m_oCountryAOIs == null)
         {
            m_oCountryAOIs = new Dictionary<string, GeographicBoundingBox>();
            String[] straCountries = Resources.aoi_region.Split(new String[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            for (int count = 1; count < straCountries.Length - 1; count++)
            {
               String[] data = straCountries[count].Split(new char[] { ',' });
               m_oCountryAOIs.Add(data[0], new GeographicBoundingBox(double.Parse(data[4]), double.Parse(data[2]), double.Parse(data[1]), double.Parse(data[3])));
            }
         }
      }

      private void populateAoiComboBox()
      {
         cAoiList.BeginUpdate();

         cAoiList.Items.Clear();
         cAoiList.Items.Add(new KeyValuePair<String, GeographicBoundingBox>("--- Go to specific AOI ---", null));
         cAoiList.Items.Add(new KeyValuePair<String, GeographicBoundingBox>("Whole world", new GeographicBoundingBox()));
         if (IsMontajChildProcess && m_oAoi != null) cAoiList.Items.Add(new KeyValuePair<String, GeographicBoundingBox>("Area of interest", m_oAoi));

         if (this.tvServers.SelectedNode != null && this.tvServers.SelectedNode.Tag is Geosoft.GX.DAPGetData.Server)
         {
            if (((Geosoft.GX.DAPGetData.Server)this.tvServers.SelectedNode.Tag).Status == Geosoft.GX.DAPGetData.Server.ServerStatus.OnLine)
            {
               cAoiList.Items.Add(new KeyValuePair<String, GeographicBoundingBox>("--- AOIs from DAP Server ---", null));
               ArrayList aAOIs = ((Geosoft.GX.DAPGetData.Server)this.tvServers.SelectedNode.Tag).ServerConfiguration.GetAreaList();
               foreach (String strAOI in aAOIs)
               {
                  double minX, minY, maxX, maxY;
                  String strCoord;
                  ((Geosoft.GX.DAPGetData.Server)this.tvServers.SelectedNode.Tag).ServerConfiguration.GetBoundingBox(strAOI, out minX, out minY, out maxX, out maxY, out strCoord);
                  if (strCoord.Equals("WGS 84"))
                  {
                     GeographicBoundingBox oBox = new GeographicBoundingBox(maxY, minY, minX, maxX);
                     cAoiList.Items.Add(new KeyValuePair<String, GeographicBoundingBox>(strAOI, oBox));
                  }
               }
            }
         }
         else
         {
            cAoiList.Items.Add(new KeyValuePair<String, GeographicBoundingBox>("--- Countries of the World ---", null));
            foreach (KeyValuePair<String, GeographicBoundingBox> country in m_oCountryAOIs)
            {
               cAoiList.Items.Add(country);
            }
         }

         cAoiList.SelectedIndex = 0;
         cAoiList.DisplayMember = "Key";

         cAoiList.EndUpdate();
      }

      private void cAoiList_SelectedIndexChanged(object sender, EventArgs e)
      {
         if (((KeyValuePair<String, GeographicBoundingBox>)cAoiList.SelectedItem).Value != null)
         {
            GoTo(((KeyValuePair<String, GeographicBoundingBox>)cAoiList.SelectedItem).Value, 0.0);
         }
      }

		#endregion

		#region Open/Save View Methods

		void SaveLastView()
		{
         // --- Don't save views when we're running inside OM ---
         if (IsMontajChildProcess) return;

			string tempFile = Path.ChangeExtension(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()), ".jpg");
			if (this.lastView.Length == 0)
				SaveCurrentView(Path.Combine(Settings.ConfigPath, LastView), tempFile, string.Empty);
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

			WorldWind.Camera.MomentumCamera camera = worldWindow.DrawArgs.WorldCamera as WorldWind.Camera.MomentumCamera;

			//stop the camera
			camera.SetPosition(camera.Latitude.Degrees, camera.Longitude.Degrees, camera.Heading.Degrees, camera.Altitude, camera.Tilt.Degrees);

			//store the servers
			this.tvServers.SaveToView(view);

			// store the current layers
			if (cLayerList.AllLayers.Count > 0)
			{
				activelayersType lyrs = view.View.Newactivelayers();
				foreach (LayerBuilderContainer container in cLayerList.AllLayers)
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

			using (Image img = TakeSnapshot(worldWindow.Handle))
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
			bool bOldView = false;
			try
			{
				if (File.Exists(filename))
				{
					DappleView view = new DappleView(filename);
					bool bShowBlueMarble = true;

					if (view.View.Hasshowbluemarble())
						bShowBlueMarble = view.View.showbluemarble.Value;

					/*RenderableObject roBMNG = GetBMNG();
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
					}*/

					if (bGoto && view.View.Hascameraorientation())
					{
						cameraorientationType orient = view.View.cameraorientation;
						worldWindow.DrawArgs.WorldCamera.SlerpPercentage = World.Settings.CameraSlerpInertia;
						worldWindow.DrawArgs.WorldCamera.SetPosition(orient.lat.Value, orient.lon.Value, orient.heading.Value, orient.altitude.Value, orient.tilt.Value);
					}

               this.tvServers.LoadFromView(view);
               if (bLoadLayers && view.View.Hasactivelayers())
               {
                  bOldView = cLayerList.LoadFromView(view, tvServers);
               }
				}
			}
			catch (Exception e)
			{
				if (MessageBox.Show(this, "Error loading view from " + filename + "\n(" + e.Message + ")\nDo you want to open the Dapple default view?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.Yes)
				{
					return OpenView(Path.Combine(Settings.DataPath, DefaultView), true);
				}
			}

         if (bOldView)
            MessageBox.Show(this, "The view " + filename + " contained some layers from an earlier version\nwhich could not be retrieved. We apologize for the inconvenience.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

			worldWindow.IsRenderDisabled = false;

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
			worldWindow.Updated += new WorldWindow.UpdatedDelegate(OnUpdated);

			this.tvServers.Load();

			// --- Configure DappleSearch if it's enabled ---

         if (!IsMontajChildProcess)
         {
            if (Settings.DappleSearchURL == null) Settings.DappleSearchURL = "http://dapplesearch.geosoft.com/";

            if (!Settings.DappleSearchURL.Equals(String.Empty))
            {
               m_strDappleSearchServerURL = Settings.DappleSearchURL;
               DappleSearchToolbar.Visible = true;
               Thread t = new Thread(new ThreadStart(BackgroundSearchThreadMain));
               t.Name = "Background search thread";
               t.Start();
            }
         }
		}

		void MainForm_Closing(object sender, CancelEventArgs e)
		{
			this.threeDConnPlugin.Unload();

			// Turning off the layers will set this
			bool bSaveGridLineState = World.Settings.ShowLatLonLines;

			this.WindowState = FormWindowState.Minimized;

			SaveLastView();

			// Ensure cleanup
         cLayerList.CmdRemoveAllLayers();
			for (int i = 0; i < worldWindow.CurrentWorld.RenderableObjects.Count; i++)
			{
				RenderableObject oRO = (RenderableObject)worldWindow.CurrentWorld.RenderableObjects.ChildObjects[i];
				oRO.IsOn = false;
				oRO.Dispose();
			}

			World.Settings.ShowLatLonLines = bSaveGridLineState;

			// Register the cache location to make it easy for uninstall to clear the cache for at least the current user
			try
			{
				RegistryKey keySW = Registry.CurrentUser.CreateSubKey("Software");
				RegistryKey keyDapple = keySW.CreateSubKey("Dapple");
				keyDapple.SetValue("CachePathForUninstall", Settings.CachePath);
			}
			catch
			{
			}

			FinalizeSettings();

			worldWindow.Dispose();

			this.cServerTreeView.Controls.Remove(this.tvServers);
			this.tvServers.Dispose();

         if (cMetadataBrowser.Url != null && cMetadataBrowser.Url.Scheme.Equals("file"))
         {
            File.Delete(HttpUtility.UrlDecode(cMetadataBrowser.Url.AbsolutePath));
         }
		}

		private void MainForm_Deactivate(object sender, EventArgs e)
		{
			worldWindow.IsRenderDisabled = true;
		}

		private void MainForm_Activated(object sender, EventArgs e)
		{
			worldWindow.IsRenderDisabled = false;
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
					string strDatasetLink = strData[5];

					if (strView.Length > 0)
						OpenView(strView, strGeoTiff.Length == 0);
					if (strGeoTiff.Length > 0)
						AddGeoTiff(strGeoTiff, strGeoTiffName, bGeotiffTmp, true);
					if (strDatasetLink.Length > 0)
						OpenDatasetLink(strDatasetLink);
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
		bool m_bDownloadUpdating = false;
		List<ActiveDownload> m_downloadList = new List<ActiveDownload>();
		int m_iPos = 0, m_iTotal = 0;
		bool m_bDownloading = false;

		void OnUpdated()
		{
			int iBuilderPos, iBuilderTotal;
			// Do the work in the update thread and just invoke to update the GUI


			int iPos = 0;
			int iTotal = 0;
			bool bDownloading = false;
			List<ActiveDownload> currentList = new List<ActiveDownload>();
			RenderableObject roBMNG = GetActiveBMNG();

			if (roBMNG != null && roBMNG.IsOn && ((QuadTileSet)((RenderableObjectList)roBMNG).ChildObjects[1]).bIsDownloading(out iBuilderPos, out iBuilderTotal))
			{
				bDownloading = true;
				iPos += iBuilderPos;
				iTotal += iBuilderTotal;
				ActiveDownload dl = new ActiveDownload();
				dl.builder = null;
				dl.iPos = iBuilderPos;
				dl.iTotal = iBuilderTotal;
				dl.bOn = true;
				dl.bRead = false;
				currentList.Add(dl);
			}

			foreach (LayerBuilderContainer container in cLayerList.AllLayers)
			{
				if (container.Builder != null && container.Builder.bIsDownloading(out iBuilderPos, out iBuilderTotal))
				{
					bDownloading = true;
					iPos += iBuilderPos;
					iTotal += iBuilderTotal;
					ActiveDownload dl = new ActiveDownload();
					dl.builder = container.Builder;
					dl.iPos = iBuilderPos;
					dl.iTotal = iBuilderTotal;
					dl.bOn = true;
					dl.bRead = false;
					currentList.Add(dl);
				}
			}


			this.BeginInvoke(new UpdateDownloadIndicatorsHandler(UpdateDownloadIndicators), new object[] { bDownloading, iPos, iTotal, currentList });
		}

		private delegate void UpdateDownloadIndicatorsHandler(bool bDownloading, int iPos, int iTotal, List<ActiveDownload> newList);

		private void UpdateDownloadIndicators(bool bDownloading, int iPos, int iTotal, List<ActiveDownload> newList)
		{
			// --- This always happens in main thread (but protect it anyway) ---
			if (!m_bDownloadUpdating)
			{
				m_bDownloadUpdating = true;
				m_downloadList = newList;
				m_bDownloading = bDownloading;
				m_iPos = iPos;
				m_iTotal = iTotal;
				if (m_bDownloading)
				{
					// Add new or update information for previous downloads
					for (int i = 0; i < newList.Count; i++)
					{
						int iFound = -1;
						for (int j = 0; j < m_downloadList.Count; j++)
						{
							if (newList[i].builder == m_downloadList[j].builder)
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
								newList[i].bOn = !m_downloadList[iFound].bOn;
								newList[i].bRead = false;
							}
						}
					}
					m_downloadList = newList;
				}
				else
					m_downloadList.Clear();

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
							this.toolStripStatusLabel6.Image = this.tvServers.ImageList.Images[m_downloadList[5].builder.ServerTypeIconKey];
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
							this.toolStripStatusLabel5.Image = this.tvServers.ImageList.Images[m_downloadList[4].builder.ServerTypeIconKey];
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
							this.toolStripStatusLabel4.Image = this.tvServers.ImageList.Images[m_downloadList[3].builder.ServerTypeIconKey];
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
							this.toolStripStatusLabel3.Image = this.tvServers.ImageList.Images[m_downloadList[2].builder.ServerTypeIconKey];
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
							this.toolStripStatusLabel2.Image = this.tvServers.ImageList.Images[m_downloadList[1].builder.ServerTypeIconKey];
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
							this.toolStripStatusLabel1.Image = this.tvServers.ImageList.Images[m_downloadList[0].builder.ServerTypeIconKey];
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
				m_bDownloadUpdating = false;
			}
		}
		#endregion

		#region Blue Marble

		/*private void toolStripButtonBMNG_Click(object sender, EventArgs e)
		{
			this.toolStripButtonBMNG.Checked = !toolStripButtonBMNG.Checked;

			if (this.toolStripButtonBMNG.Checked)
				this.toolStripButtonBMNG.Image = global::Dapple.Properties.Resources.blue_marble_checked;
			else
				this.toolStripButtonBMNG.Image = global::Dapple.Properties.Resources.blue_marble_unchecked;


			RenderableObject roBMNG = GetBMNG();
			if (roBMNG != null)
				roBMNG.IsOn = this.toolStripButtonBMNG.Checked;
		}*/

		private RenderableObject GetBMNG()
		{
			for (int i = 0; i < worldWindow.CurrentWorld.RenderableObjects.Count; i++)
			{
				if (((RenderableObject)worldWindow.CurrentWorld.RenderableObjects.ChildObjects[i]).Name == "4 - The Blue Marble")
					return worldWindow.CurrentWorld.RenderableObjects.ChildObjects[i] as RenderableObject;
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

		#region MainApplication Implementation
		/// <summary>
		/// MainApplication's System.Windows.Forms.Form
		/// </summary>
		public override System.Windows.Forms.Form Form
		{
			get
			{
				return this;
			}
		}

		/// <summary>
		/// MainApplication's globe window
		/// </summary>
		public override WorldWindow WorldWindow
		{
			get
			{
				return worldWindow;
			}
		}

		/// <summary>
		/// The splash screen dialog.
		/// </summary>
		public override WorldWind.Splash SplashScreen
		{
			get
			{
				return splashScreen;
			}
		}
		#endregion

		#region DappleSearch code

		private enum SearchMode { Text, ROI, Dual };
		private SearchMode searchMode = SearchMode.Dual;
		private String SEARCH_HTML_GATEWAY = "SearchInterfaceHTML.aspx";
      private String NEW_SERVER_GATEWAY = "AddNewServer.aspx";
		private String SEARCH_XML_GATEWAY = "SearchInterfaceXML.aspx";
		private String m_strDappleSearchServerURL = null;

		private void DappleSearchKeyword_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (((ushort)e.KeyChar) == 13) ViewSearchResults(); // enter key pressed
		}

		private void textToolStripMenuItem_Click(object sender, EventArgs e)
		{
			searchMode = SearchMode.Text;
			DappleSearchGoButton.Image = textToolStripMenuItem.Image;
			DappleSearchKeyword.Enabled = true;
		}

		private void rOIToolStripMenuItem_Click(object sender, EventArgs e)
		{
			searchMode = SearchMode.ROI;
			DappleSearchGoButton.Image = rOIToolStripMenuItem.Image;
			DappleSearchKeyword.Enabled = false;
		}

		private void dualToolStripMenuItem_Click(object sender, EventArgs e)
		{
			searchMode = SearchMode.Dual;
			DappleSearchGoButton.Image = dualToolStripMenuItem.Image;
			DappleSearchKeyword.Enabled = true;
		}

		private void DappleSearchGoButton_ButtonClick(object sender, EventArgs e)
		{
			ViewSearchResults();
		}


		private void DappleSearchBasicGoButton_Click(object sender, EventArgs e)
		{
			ViewSearchResults();
		}

		private void ViewSearchResults()
		{
			openSearchResults();
			displayResultsPanel();
		}

		private void CloseSearchResultsButton_Click(object sender, EventArgs e)
		{
			hideSearchResults();
		}

		private void displayResultsPanel()
		{
			CloseSearchResultsButton.Enabled = true;
			worldWindow.Visible = false;
			WorldResultsSplitPanel.SplitterDistance = WorldResultsSplitPanel.Width - 400;
			WorldResultsSplitPanel.Panel2Collapsed = false;
			worldWindow.Visible = true;
			worldWindow.SafeRender();
		}

		private void hideSearchResults()
		{
			CloseSearchResultsButton.Enabled = false;
			worldWindow.Visible = false;
			WorldResultsSplitPanel.Panel2Collapsed = true;
			worldWindow.Visible = true;
			worldWindow.SafeRender();
		}

		private void openSearchResults()
		{
			if (m_strDappleSearchServerURL == null) return;

			StringBuilder resultsURL = new StringBuilder(m_strDappleSearchServerURL + SEARCH_HTML_GATEWAY + "?");
			if (searchMode != SearchMode.ROI)
			{
				resultsURL.Append("&keyword=");
				resultsURL.Append(HttpUtility.UrlEncode(DappleSearchKeyword.Text));
			}
			if (searchMode != SearchMode.Text)
			{
				GeographicBoundingBox AoI = GeographicBoundingBox.FromQuad(worldWindow.GetViewBox(false));
				resultsURL.Append("&usebbox=true");
				resultsURL.Append("&minx=");
				resultsURL.Append(AoI.West);
				resultsURL.Append("&miny=");
				resultsURL.Append(AoI.South);
				resultsURL.Append("&maxx=");
				resultsURL.Append(AoI.East);
				resultsURL.Append("&maxy=");
				resultsURL.Append(AoI.North);
			}

			SearchResultsBrowser.Navigate(resultsURL.ToString());
		}

		private void BackgroundSearchThreadMain()
		{
			if (m_strDappleSearchServerURL == null) return;

			GeographicBoundingBox lastAoI = GeographicBoundingBox.FromQuad(worldWindow.GetViewBox(false));
			GeographicBoundingBox currentAoI = null;
			String keyword = DappleSearchKeyword.Text;
			SearchMode mode = searchMode;

			Boolean searchCompleted = false;

			while (true)
			{
				try { Thread.Sleep(1000); }
				catch (ThreadAbortException) { return; }
				catch (ThreadInterruptedException) { return; }

            if (this.IsDisposed) return;

				try
				{
					currentAoI = GeographicBoundingBox.FromQuad(worldWindow.GetViewBox(false));
				}
				catch (NullReferenceException)
            {
               return;// Assume because program is shutting down, so thread does too.
            } 

				if (
				   searchMode == mode
				   &&
				   (searchMode == SearchMode.Text || lastAoI.North == currentAoI.North && lastAoI.East == currentAoI.East && lastAoI.South == currentAoI.South && lastAoI.West == currentAoI.West)
				   &&
				   (searchMode == SearchMode.ROI || keyword.Equals(DappleSearchKeyword.Text))
				   )
				{
					if (!searchCompleted)
					{
						setDappleSearchResultsLabelText("Hits: <searching...>");
						int hits, error;

						doBackgroundSearch(out hits,
							out error,
							searchMode == SearchMode.ROI ? null : keyword,
							searchMode == SearchMode.Text ? null : currentAoI);

						if (error == 0)
							setDappleSearchResultsLabelText("Hits: " + hits);
						else if (error > 0)
							setDappleSearchResultsLabelText("Hits: <ERROR " + error + ">");
						else
						{
							setDappleSearchResultsLabelText("ERROR CONTACTING SEARCH SERVER");
							DappleSearchGoButton.Enabled = false;
						}
						searchCompleted = true;
					}
				}
				else
				{
					searchCompleted = false;
					setDappleSearchResultsLabelText("Hits: <waiting...>");
					lastAoI = currentAoI;
					keyword = DappleSearchKeyword.Text;
					mode = searchMode;
				}
			}
		}

      private void setDappleSearchResultsLabelText(String text)
      {
         if (InvokeRequired)
         {
            Invoke(new UpdateTextDelegate(__setDappleSearchResultsLabelText), new Object[] { text });
         }
         else
         {
            __setDappleSearchResultsLabelText(text);
         }
      }

      delegate void UpdateTextDelegate(String text);
      /// <summary>
      /// DO NOT CALL THIS METHOD DIRECTLY.  Call the non-underscored one instead.  It is event-thread-safe.
      /// </summary>
      /// <param name="text"></param>
      private void __setDappleSearchResultsLabelText(String text)
      {
         DappleSearchResultsLabel.Text = text;
      }

		private void doBackgroundSearch(out int hits, out int error, String keywords, GeographicBoundingBox ROI)
		{
			XmlDocument query = new XmlDocument();
			XmlElement geoRoot = query.CreateElement("geosoft_xml");
			query.AppendChild(geoRoot);
			XmlElement root = query.CreateElement("search_request");
			root.SetAttribute("version", "1.0");
			root.SetAttribute("handle", "cheese");
			root.SetAttribute("maxcount", "0");
			root.SetAttribute("offset", "0");
			geoRoot.AppendChild(root);

			if (ROI != null)
			{
				XmlElement boundingBox = query.CreateElement("bounding_box");
				boundingBox.SetAttribute("minx", ROI.West.ToString());
				boundingBox.SetAttribute("miny", ROI.South.ToString());
				boundingBox.SetAttribute("maxx", ROI.East.ToString());
				boundingBox.SetAttribute("maxy", ROI.North.ToString());
				boundingBox.SetAttribute("crs", "WSG84");
				root.AppendChild(boundingBox);
			}

			if (keywords != null)
			{
				XmlElement keyword = query.CreateElement("text_filter");
				keyword.InnerText = keywords;
				root.AppendChild(keyword);
			}

			// --- Do the request ---

         try
         {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(m_strDappleSearchServerURL + SEARCH_XML_GATEWAY);
            request.Headers["GeosoftMapSearchRequest"] = query.InnerXml;
            WebResponse response = request.GetResponse();

            XmlDocument responseXML = new XmlDocument();
            responseXML.Load(response.GetResponseStream());

            System.Xml.XPath.XPathNavigator navcom = responseXML.CreateNavigator();
            if (navcom.MoveToFollowing("error", ""))
            {
               String errorString = navcom.GetAttribute("code", "");
               hits = -1;
               error = Int32.Parse(errorString);
            }
            else
            {
               navcom.MoveToFollowing("search_result", "");
               String countString = navcom.GetAttribute("totalcount", "");
               hits = Int32.Parse(countString);
               error = 0;
            }
         }
         catch (Exception e)
         {
            StringBuilder dump = new StringBuilder();
            dump.Append("================================================================================" + Environment.NewLine);
            dump.Append("Exception caused by background search thread at " + System.DateTime.Now + Environment.NewLine);
            Exception iter = e;
            while (e != null)
            {
               dump.Append("--------------------------------------------------------------------------------" + Environment.NewLine);
               dump.Append("Type: " + e.GetType().ToString() + Environment.NewLine);
               dump.Append("Message: " + e.Message + Environment.NewLine);
               dump.Append("StackTrace:" + Environment.NewLine);
               dump.Append(e.StackTrace + Environment.NewLine);

               e = e.InnerException;
            }

            dump.Append("================================================================================" + Environment.NewLine);

            File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "errordump.txt"), dump.ToString());

            hits = 0; error = -1; return;
         }
		}

		private void WorldResultsSplitPanel_SplitterMoving(object sender, SplitterCancelEventArgs e)
		{
			worldWindow.Visible = false;
		}

		private void WorldResultsSplitPanel_SplitterMoved(object sender, SplitterEventArgs e)
		{
			worldWindow.Visible = true;
			worldWindow.SafeRender();
		}

		private void OpenDatasetLink(String linkFilename)
		{
			if (!File.Exists(linkFilename)) return;
			XmlDocument linkData = new XmlDocument();
			linkData.Load(linkFilename);

			XmlElement searchElement = (XmlElement)linkData.SelectSingleNode("//geosoft_xml/search_params");

			if (searchElement != null)
			{
				StringBuilder resultsURL = new StringBuilder(m_strDappleSearchServerURL + SEARCH_HTML_GATEWAY + "?");
				if (!searchElement.GetAttribute("keyword").Equals(String.Empty))
				{
					resultsURL.Append("&keyword=");
					resultsURL.Append(searchElement.GetAttribute("keyword"));
				}
				if (!searchElement.GetAttribute("minx").Equals(String.Empty))
				{
					resultsURL.Append("&usebbox=true");
					resultsURL.Append("&minx=");
					resultsURL.Append(searchElement.GetAttribute("minx"));
					resultsURL.Append("&miny=");
					resultsURL.Append(searchElement.GetAttribute("miny"));
					resultsURL.Append("&maxx=");
					resultsURL.Append(searchElement.GetAttribute("maxx"));
					resultsURL.Append("&maxy=");
					resultsURL.Append(searchElement.GetAttribute("maxy"));
				}
				if (!searchElement.GetAttribute("page").Equals(String.Empty))
				{
					resultsURL.Append("&page=");
					resultsURL.Append(searchElement.GetAttribute("page"));
				}
				if (!searchElement.GetAttribute("numresults").Equals(String.Empty))
				{
					resultsURL.Append("&numresults=");
					resultsURL.Append(searchElement.GetAttribute("numresults"));
				}
				SearchResultsBrowser.Navigate(resultsURL.ToString());
				displayResultsPanel();
			}

			XmlElement displayMapElement = (XmlElement)linkData.SelectSingleNode("//geosoft_xml/display_map");

			String serverType = displayMapElement.GetAttribute("type");
			String serverURL = displayMapElement.GetAttribute("server");
         if (!serverURL.Contains("?")) serverURL += "?";
			String layerTitle = displayMapElement.GetAttribute("layertitle");
			float minx = Single.Parse(displayMapElement.GetAttribute("minx"));
			float miny = Single.Parse(displayMapElement.GetAttribute("miny"));
			float maxx = Single.Parse(displayMapElement.GetAttribute("maxx"));
			float maxy = Single.Parse(displayMapElement.GetAttribute("maxy"));

			if (serverType.Equals("DAP"))
			{
				String layerName = displayMapElement.GetAttribute("datasetname");
				String strHeight = displayMapElement.GetAttribute("height");
				String strSize = displayMapElement.GetAttribute("size");
				String datasetType = displayMapElement.GetAttribute("datasettype");
				String edition = displayMapElement.GetAttribute("edition");
				String hierarchy = displayMapElement.GetAttribute("hierarchy");
				String strLevels = displayMapElement.GetAttribute("levels");
				String strLevelZeroTilesize = displayMapElement.GetAttribute("levelzerotilesize");

				String strUri = "gxdap://" + serverURL +
				   "&datasetname=" + HttpUtility.UrlEncode(layerName) +
				   "&height=" + strHeight +
				   "&size=" + strSize +
				   "&type=" + datasetType +
				   "&title=" + HttpUtility.UrlEncode(layerTitle) +
				   "&edition=" + HttpUtility.UrlEncode(edition) +
				   "&hierarchy=" + HttpUtility.UrlEncode(hierarchy) +
				   "&north=" + maxy +
				   "&east=" + maxx +
				   "&south=" + miny +
				   "&west=" + minx +
				   "&levels=" + strLevels +
				   "&lvl0tilesize=" + strLevelZeroTilesize;

				cLayerList.AddLayer(LayerUri.create(strUri).getBuilder(this.WorldWindow, tvServers));
			}
			else if (serverType.Equals("WMS"))
			{
            String layerName = displayMapElement.GetAttribute("layername");
            String strUri = "gxwms://" + serverURL + "&layer=" + layerName + "&pixelsize=256";

            cLayerList.AddLayer(LayerUri.create(strUri).getBuilder(this.WorldWindow, tvServers));
			}
         else if (serverType.Equals("ArcIMS"))
         {
            String serviceName = displayMapElement.GetAttribute("servicename");
            String strUri = "gxarcims://" + serverURL +
               "&servicename=" + serviceName +
               "&minx=" + minx.ToString() +
               "&miny=" + miny.ToString() +
               "&maxx=" + maxx.ToString() +
               "&maxy=" + maxy.ToString();

            cLayerList.AddLayer(LayerUri.create(strUri).getBuilder(this.WorldWindow, tvServers));
         }
         else
         {
            MessageBox.Show("Unable to view layer: unknown server type '" + serverType + "'", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
         }
		}

      private void submitServerToSearchEngine(object param)
      {
         try
         {
            XmlDocument query = new XmlDocument();
            XmlElement geoRoot = query.CreateElement("geosoft_xml");
            query.AppendChild(geoRoot);
            XmlElement root = query.CreateElement("add_server");
            root.SetAttribute("url", ((String[])param)[0]);
            root.SetAttribute("type", ((String[])param)[1]);
            geoRoot.AppendChild(root);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(m_strDappleSearchServerURL + NEW_SERVER_GATEWAY);
            request.Headers["GeosoftAddServerRequest"] = query.InnerXml;
            WebResponse response = request.GetResponse();
            response.Close();
         }
         catch
         {
            // Never crash, just silently fail to send the data to the server.
         }
      }

		#endregion

		#region Temporary KML Code

		/*private void tvLayers_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				GeographicBoundingBox extents = null;
				TreeNode node = this.tvLayers.HitTest(e.Location).Node;

				Type tp = node.Tag.GetType();

				if (node.Tag is WorldWind.Renderable.Icon)
				{
					WorldWind.Renderable.Icon icon = node.Tag as WorldWind.Renderable.Icon;

					worldWindow.GotoLatLon(icon.Latitude, icon.Longitude);
				}
				else if (node.Tag is ImageLayer)
				{
					ImageLayer imageLayer = node.Tag as ImageLayer;

					extents = new GeographicBoundingBox(imageLayer.MaxLat, imageLayer.MinLat, imageLayer.MinLon, imageLayer.MaxLon);
				}
				else if (node.Tag is LineFeature)
				{
					LineFeature line = node.Tag as LineFeature;
					extents = new GeographicBoundingBox(double.MinValue, double.MaxValue, double.MaxValue, double.MinValue);
					foreach (Point3d p in line.Points)
					{
						extents.North = Math.Max(p.Y, extents.North);
						extents.South = Math.Min(p.Y, extents.South);
						extents.East = Math.Max(p.X, extents.East);
						extents.West = Math.Min(p.X, extents.West);
					}
				} 
				else if (node.Tag is PolygonFeature)
				{
					Point3d pSph;
					PolygonFeature pFeat = node.Tag as PolygonFeature;
					extents = new GeographicBoundingBox(double.MinValue, double.MaxValue, double.MaxValue, double.MinValue);
					foreach (Point3d p in pFeat.BoundingBox.corners)
					{
						pSph = MathEngine.CartesianToSpherical(p.X, p.Y, p.Z);
						pSph.Y = MathEngine.RadiansToDegrees(pSph.Y);
						pSph.Z = MathEngine.RadiansToDegrees(pSph.Z);
						extents.North = Math.Max(pSph.Y, extents.North);
						extents.South = Math.Min(pSph.Y, extents.South);
						extents.East = Math.Max(pSph.Z, extents.East);
						extents.West = Math.Min(pSph.Z, extents.West);
					}
				}

				if (extents != null)
					GoTo(extents, -1.0);
			}
		}

		void UpdateKMLNodes(TreeNode parentNode, RenderableObjectList objectList)
		{
			TreeNode node;
			int iImage = ImageListIndex("kml");
			foreach (RenderableObject ro in objectList.ChildObjects)
			{
				node = this.tvLayers.Add(parentNode, ro.Name, iImage, iImage, this.kmlPlugin.KMLIcons.IsOn ? TriStateTreeView.CheckBoxState.Checked : TriStateTreeView.CheckBoxState.Unchecked);
				node.Tag = ro;
				if (ro is RenderableObjectList)
					UpdateKMLNodes(node, ro as RenderableObjectList);
			}
		}

		void UpdateKMLIcons()
		{
			this.tvLayers.BeginUpdate();

			if (this.kmlNode != null)
				this.tvLayers.Nodes.Remove(this.kmlNode);

			if (this.kmlPlugin.KMLIcons.ChildObjects.Count > 0)
			{
				int iImage = ImageListIndex("kml");
				this.kmlNode = this.tvLayers.AddTop(null, this.kmlPlugin.KMLIcons.Name, iImage, iImage, this.kmlPlugin.KMLIcons.IsOn ? TriStateTreeView.CheckBoxState.Checked : TriStateTreeView.CheckBoxState.Unchecked);
				this.kmlNode.Tag = this.kmlPlugin.KMLIcons;
				UpdateKMLNodes(this.kmlNode, this.kmlPlugin.KMLIcons);
			}
			this.tvLayers.EndUpdate();
		}

		private void toolStripMenuItemOpenKML_Click(object sender, EventArgs e)
		{
			System.Windows.Forms.OpenFileDialog fileDialog = new OpenFileDialog();
			fileDialog.CheckFileExists = true;
			fileDialog.Filter = "KML/KMZ files (*.kml *.kmz)|*.kml;*.kmz";
			fileDialog.Multiselect = false;
			fileDialog.RestoreDirectory = true;
			DialogResult result = fileDialog.ShowDialog();

			if (result == DialogResult.OK)
			{
				this.kmlPlugin.KMLIcons.ChildObjects.Clear();
				if (this.kmlNode != null)
					this.tvLayers.Nodes.Remove(this.kmlNode);
				int iImage = ImageListIndex("kml");
				this.kmlNode = this.tvLayers.AddTop(null, "Please wait, loading KML file...", iImage, iImage, TriStateTreeView.CheckBoxState.None);
				this.kmlPlugin.LoadDiskKM(fileDialog.FileName, new MethodInvoker(UpdateKMLIcons));
			}
		}*/

		#endregion

      /// <summary>
		/// Occurs when an un-trapped thread exception is thrown, typically in UI event handlers.
		/// </summary>
		private static void Application_ThreadException( object sender, System.Threading.ThreadExceptionEventArgs e )
		{
#if DEBUG
         Log.Write(e.Exception);
#else
			Log.Write( e.Exception );

			//HACK
			if (e.Exception is NullReferenceException)
				return;

			Utility.AbortUtility.Abort(e.Exception, Thread.CurrentThread);
#endif
		}

      private void cSearchTextComboBox_KeyPress(object sender, KeyPressEventArgs e)
      {
         if (e.KeyChar.Equals((char)13))
         {
            doSearch();
         }
      }

      private void cSearchButton_Click(object sender, EventArgs e)
      {
         doSearch();
      }

      private void doSearch()
      {
         cSearchTextComboBox.Text = cSearchTextComboBox.Text.Trim();

         if (!cSearchTextComboBox.Text.Equals(String.Empty) && !cSearchTextComboBox.Items.Contains(cSearchTextComboBox.Text))
         {
            cSearchTextComboBox.Items.Add(cSearchTextComboBox.Text);
         }

         GeographicBoundingBox oAoi = GeographicBoundingBox.FromQuad(worldWindow.GetViewBox(false));
         String strText = cSearchTextComboBox.Text;
         this.tvServers.Search(oAoi, strText);
         this.cServerListControl.setSearchCriteria(strText, oAoi);
      }

      private void cServerTabControl_SelectedIndexChanged(object sender, EventArgs e)
      {
         if (cServerTabControl.SelectedIndex == 1)
         {
            cServerListControl.Servers = this.tvServers.getServerList();
            cServerListControl.SelectedServer = this.tvServers.SelectedServer;
         }
         else if (cServerTabControl.SelectedIndex == 0)
         {
            tvServers.SelectedServer = cServerListControl.SelectedServer;
         }
      }

      private void CenterNavigationToolStrip()
      {
         Point newLocation = new Point((WorldResultsSplitPanel.Panel1.Width - toolStripNavigation.Width) / 2, WorldResultsSplitPanel.Height - toolStripNavigation.Height);
         toolStripNavigation.Location = newLocation;
      }

      private void DisplayMetadataMessage(String szMessage)
      {
         if (InvokeRequired) this.Invoke(new UpdateTextDelegate(__DisplayMetadataMessage), new Object[] { szMessage });
         else __DisplayMetadataMessage(szMessage);
      }

      private void DisplayMetadataDocument(String szMessage)
      {
         if (InvokeRequired) this.Invoke(new UpdateTextDelegate(__DisplayMetadataDocument), new Object[] { szMessage });
         else __DisplayMetadataDocument(szMessage);
      }

      private void __DisplayMetadataMessage(String szMessage)
      {
         cMetadataBrowser.Visible = false;
         cMetadataLoadingLabel.Text = szMessage;
         cWorldMetadataSplitter.Panel2.Refresh();
      }

      private void __DisplayMetadataDocument(String szUri)
      {
         lock (cMetadataBrowser)
         {
            cMetadataBrowser.Visible = true;
            Uri metaUri = new Uri(szUri);
            if (!metaUri.Equals(cMetadataBrowser.Url))
            {
               // --- Delete the file we were pointing to before ---
               if (cMetadataBrowser.Url != null && cMetadataBrowser.Url.Scheme.Equals("file"))
               {
                  File.Delete(HttpUtility.UrlDecode(cMetadataBrowser.Url.AbsolutePath));
               }
               cMetadataBrowser.Url = metaUri;
               cMetadataBrowser.Refresh(WebBrowserRefreshOption.Completely);
            }
            cWorldMetadataSplitter.Panel2.Refresh();
         }
      }

      public void LoadMetadata(Object oParams)
      {
         IBuilder oBuilder = ((Object[])oParams)[0] as IBuilder;

         try
         {
            XmlDocument oDoc = new XmlDocument();
            oDoc.AppendChild(oDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes"));
            XmlNode oNode = null;
            string strStyleSheet = null;

            if (oBuilder.SupportsMetaData)
            {
               oNode = oBuilder.GetMetaData(oDoc);
               strStyleSheet = oBuilder.StyleSheetName;
               DisplayMetadataMessage("Loading metadata for layer " + oBuilder.Name);
            }
            else
            {
               DisplayMetadataMessage("Metadata for the selected layer type is unsupported");
               return;
            }

            if (oNode is XmlDocument)
            {
               oDoc = oNode as XmlDocument;
            }
            else if (oNode is XmlElement)
            {
               oDoc.AppendChild(oNode);
            }
            if (strStyleSheet != null)
            {
               XmlNode oRef = oDoc.CreateProcessingInstruction("xml-stylesheet", "type='text/xsl' href='" + Path.Combine(this.metaviewerDir, strStyleSheet) + "'");
               oDoc.InsertBefore(oRef, oDoc.DocumentElement);
            }

            string filePath = Path.Combine(this.metaviewerDir, Path.GetRandomFileName());
            filePath = Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath) + ".xml");
            oDoc.Save(filePath);
            DisplayMetadataDocument(filePath);
         }
         catch (Exception e)
         {
            DisplayMetadataMessage("An error occurred while accessing metadata: " + e.Message);
         }
      }

      private void cAddLayerButton_Click(object sender, EventArgs e)
      {
         AddDatasetAction();
      }

      private void DumpServerTree()
      {
         DumpTreeNode(this.tvServers.RootNode, String.Empty);
      }

      private void DumpTreeNode(TreeNode oNode, String strPrepend)
      {
         Console.Write(strPrepend + oNode.Text);
         if (oNode.Tag != null)
            Console.Write(" (" + oNode.Tag.GetType() + ")");
         else
            Console.Write(" (null)");
         Console.WriteLine();

         foreach (TreeNode oChild in oNode.Nodes)
         {
            DumpTreeNode(oChild, strPrepend + "  -");
         }
      }

      #region UI Actions

      private void AddDatasetAction()
      {
         if (cServerTabControl.SelectedIndex == 0)
         {
            this.tvServers.AddCurrentDataset();
         }
         else
         {
            List<LayerBuilder> oBuilderList = cServerListControl.SelectedLayers;
            oBuilderList.Reverse();
            cLayerList.AddLayers(oBuilderList);
         }
      }

      #endregion

      private void exitToolStripMenuItem_Click(object sender, EventArgs e)
      {
         Close();
      }

      private void addToLayersToolStripMenuItem_Click(object sender, EventArgs e)
      {
         AddDatasetAction();
      }

      private void WorldResultsSplitPanel_Panel1_DragOver(object sender, DragEventArgs e)
      {
         if (e.Data.GetDataPresent(typeof(List<LayerBuilder>)))
         {
            e.Effect = DragDropEffects.Copy;
         }
      }

      private void WorldResultsSplitPanel_Panel1_DragDrop(object sender, DragEventArgs e)
      {
         if (e.Data.GetDataPresent(typeof(List<LayerBuilder>)))
         {
            List<LayerBuilder> oDropData = e.Data.GetData(typeof(List<LayerBuilder>)) as List<LayerBuilder>;
            cLayerList.AddLayers(oDropData);
         }
      }

      private void downloadToolStripMenuItem_Click(object sender, EventArgs e)
      {
         cLayerList.CmdDownloadActiveLayers();
      }
   }

   /// <summary>
   /// Synchronization class which handles displaying the metadata for a layer.  Prevents loading a layer
   /// multiple times, and supresses multiple loads when more than one layer change occurs in a short
   /// time period.
   /// </summary>
   class MetadataDisplayThread
   {
      private Object LOCK = new Object();
      private ManualResetEvent m_oSignaller = new ManualResetEvent(false);
      private List<IBuilder> m_hLayerToLoad = new List<IBuilder>();
      private Thread m_hThread;
      private MainForm m_hOwner;

      public MetadataDisplayThread(MainForm hOwner)
      {
         m_hOwner = hOwner;

         m_hThread = new Thread(new ThreadStart(ThreadMain));
         m_hThread.IsBackground = true;
         m_hThread.Start();
      }

      public void addBuilder(IBuilder oBuilder)
      {
         lock (LOCK)
         {
            m_hLayerToLoad.Add(oBuilder);
            m_oSignaller.Set();
         }
      }

      private void ThreadMain()
      {
         IBuilder oCurrentBuilder = null;
         IBuilder oLastBuilder = null;

         while (true)
         {
            m_oSignaller.WaitOne();

            lock (LOCK)
            {
               oCurrentBuilder = m_hLayerToLoad[m_hLayerToLoad.Count - 1];
               m_hLayerToLoad.Clear();
               m_oSignaller.Reset();
            }

            if (!oCurrentBuilder.Equals(oLastBuilder))
            {
               m_hOwner.LoadMetadata(new Object[] { oCurrentBuilder });
               oLastBuilder = oCurrentBuilder;
            }
         }
      }
   }
}
