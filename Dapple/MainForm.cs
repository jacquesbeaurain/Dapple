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
using Geosoft.GX.DAPGetData;
using Dapple.CustomControls;

namespace Dapple
{
	public partial class MainForm : MainApplication
	{
		#region Win32 DLLImports

		[DllImport("User32.dll")]
		private static extern UInt32 RegisterWindowMessageW(String strMessage);

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

		#endregion

		#region Delegates

		/// <summary>
		/// Called when this control selects a layer to tell others to load Metadata for it.
		/// </summary>
		/// <param name="?"></param>
		public delegate void ViewMetadataHandler(IBuilder oBuilder);

		#endregion

		#region Constants

		public const int MAX_MRU_TERMS = 8;
		public const int DOWNLOAD_TIMEOUT = 30000;
		public const string ViewExt = ".dapple";
		public const string FavouriteServerExt = ".dapple_serverlist";
		public const string LastView = "lastview" + ViewExt;
		public const string DefaultView = "default" + ViewExt;
		public const string HomeView = "homeview_2.0.0" + ViewExt;
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
		public const string NEW_SERVER_GATEWAY = "AddNewServer.aspx";
		public const string SEARCH_XML_GATEWAY = "SearchInterfaceXML.aspx";
		public const string NO_SEARCH = "--- Enter keyword(s) ---";
		public static readonly UInt32 OpenViewMessage = RegisterWindowMessageW("Dapple.OpenViewMessage");
		public static readonly string UserPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DappleData");

		#endregion

		#region Private Members

		private Splash splashScreen;
		private ServerTree c_oServerTree;
		private ServerList c_oServerList;
		private JanaTab cServerViewsTab;
		private DappleSearchList c_oDappleSearch;
		private static WorldWindow c_oWorldWindow;

		public static WorldWindow WorldWindowSingleton
		{
			get { return c_oWorldWindow; }
		}

		private NASA.Plugins.ScaleBarLegend scalebarPlugin;

		private Murris.Plugins.Compass compassPlugin;
		private Murris.Plugins.GlobalClouds cloudsPlugin;
		private Murris.Plugins.SkyGradient skyPlugin;

		private Stars3D.Plugin.Stars3D starsPlugin;
		private ThreeDconnexion.Plugin.TDxWWInput threeDConnPlugin;

		private RenderableObjectList placeNames;

		private string openView = "";
		private string openGeoTiff = "";
		private string openGeoTiffName = "";
		private bool openGeoTiffTmp = false;
		private string lastView = "";
		private string metaviewerDir = "";

		private MetadataDisplayThread m_oMetadataDisplay;

		private static ImageList m_oImageList = new ImageList();
		private static RemoteInterface m_oMontajRemoteInterface;
		private static Dapple.Extract.Options.Client.ClientType m_eClientType;
		private static GeographicBoundingBox m_oOMMapExtentWGS84;
		private static GeographicBoundingBox m_oOMMapExtentNative;
		private static string m_strAoiCoordinateSystem;
		private static string m_strOpenMapFileName = string.Empty;
		private Dictionary<String, GeographicBoundingBox> m_oCountryAOIs;

		private string m_szLastSearchString = String.Empty;
		private GeographicBoundingBox m_oLastSearchROI = null;
		#endregion

		#region Properties

		private String SearchKeyword
		{
			get
			{
				String result = c_tbSearchKeywords.Text.Trim();
				if (result.Equals(NO_SEARCH)) result = String.Empty;

				return result;
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
			get
			{
				return m_oMontajRemoteInterface;
			}
		}

		/// <summary>
		/// Get the client
		/// </summary>
		public static Dapple.Extract.Options.Client.ClientType Client
		{
			get { return m_eClientType; }
		}

		/// <summary>
		/// Get the open map area of interest
		/// </summary>
		public static GeographicBoundingBox MapAoi
		{
			get { return m_oOMMapExtentNative; }
		}

		/// <summary>
		/// Get the open map coordinate system
		/// </summary>
		public static string MapAoiCoordinateSystem
		{
			get { return m_strAoiCoordinateSystem; }
		}

		/// <summary>
		/// Get the name of the open map
		/// </summary>
		public static string MapFileName
		{
			get { return m_strOpenMapFileName; }
		}

		#region Blue Marble

		private RenderableObject GetBMNG()
		{
			for (int i = 0; i < c_oWorldWindow.CurrentWorld.RenderableObjects.Count; i++)
			{
				if (((RenderableObject)c_oWorldWindow.CurrentWorld.RenderableObjects.ChildObjects[i]).Name == "4 - The Blue Marble")
					return c_oWorldWindow.CurrentWorld.RenderableObjects.ChildObjects[i] as RenderableObject;
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

		#endregion

		#region Constructor

		public MainForm(string strView, string strGeoTiff, string strGeotiffName, bool bGeotiffTmp, string strLastView, Dapple.Extract.Options.Client.ClientType eClientType, RemoteInterface oMRI, GeographicBoundingBox oAoi, string strAoiCoordinateSystem, string strMapFileName)
		{
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
			System.Threading.Thread.CurrentThread.Name = ThreadNames.EventDispatch;

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

			// --- Set up a new user's favorites list and home view ---

			/*if (!File.Exists(Path.Combine(CurrentSettingsDirectory, "user.dapple_serverlist")))
			{
				File.Copy(Path.Combine(Path.Combine(DirectoryPath, "Data"), "default.dapple_serverlist"), Path.Combine(CurrentSettingsDirectory, "user.dapple_serverlist"));
			}*/
			if (!File.Exists(Path.Combine(CurrentSettingsDirectory, HomeView)))
			{
				File.Copy(Path.Combine(Path.Combine(DirectoryPath, "Data"), "default.dapple"), Path.Combine(CurrentSettingsDirectory, HomeView));
			}

			InitSettings();

			if (Settings.NewCachePath.Length > 0)
			{
				try
				{
					// We want to make sure the new cache path is writable
					Directory.CreateDirectory(Settings.NewCachePath);
					if (Directory.Exists(Settings.CachePath))
						Utility.FileSystem.DeleteFolderGUI(this, Settings.CachePath, "Deleting Existing Cache");
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

			//#if !DEBUG
			using (this.splashScreen = new Splash())
			{
				this.splashScreen.Owner = this;
				this.splashScreen.Show();
				this.splashScreen.SetText("Initializing...");

				Application.DoEvents();
				//#endif

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
				m_oImageList.Images.Add("dap_generic", Resources.dap_map);
				m_oImageList.Images.Add("dap_picture", Resources.dap_picture);
				m_oImageList.Images.Add("dap_picturesection", Resources.dap_picture);
				m_oImageList.Images.Add("dap_point", Resources.dap_point);
				m_oImageList.Images.Add("dap_spf", Resources.dap_spf);
				m_oImageList.Images.Add("dap_voxel", Resources.dap_voxel);
				m_oImageList.Images.Add("dap_imageserver", Resources.arcims);
				m_oImageList.Images.Add("dap_gridsection", Resources.dap_grid);
				m_oImageList.Images.Add("folder", Resources.folder);
				m_oImageList.Images.Add("folder_open", Resources.folder_open);
				m_oImageList.Images.Add("loading", Resources.loading);
				m_oImageList.Images.Add("dap_arcgis", global::Dapple.Properties.Resources.dap_arcgis);
				m_oImageList.Images.Add("dap_imageserver", Resources.nasa);
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
				m_oImageList.Images.Add("imageservice", Dapple.Properties.Resources.layers_top);
				m_oImageList.Images.Add("blue_marble", Dapple.Properties.Resources.blue_marble);


				c_oWorldWindow = new WorldWindow();
#if !DEBUG
				Utility.AbortUtility.ProgramAborting += new MethodInvoker(c_oWorldWindow.KillD3DAndWorkerThread);
#endif
				c_oWorldWindow.AllowDrop = true;
				c_oWorldWindow.DragOver += new DragEventHandler(c_oWorldWindow_DragOver);
				c_oWorldWindow.DragDrop += new DragEventHandler(c_oWorldWindow_DragDrop);
				InitializeComponent();
				this.SuspendLayout();

/*#if DEBUG
				// --- Make the server tree HOOGE ---
				this.splitContainerMain.SplitterDistance = 400;
				this.splitContainerLeftMain.SplitterDistance = 400;
#endif*/

				this.Icon = new System.Drawing.Icon(@"app.ico");
				DappleToolStripRenderer oTSR = new DappleToolStripRenderer();
				c_tsSearch.Renderer = oTSR;
				c_tsLayers.Renderer = oTSR;
				c_tsOverview.Renderer = oTSR;
				c_tsMetadata.Renderer = oTSR;

				c_tsNavigation.Renderer = new BorderlessToolStripRenderer();

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
				c_oWorldWindow.Cache = new Cache(
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

				WorldWind.Terrain.TerrainTileService terrainTileService = new WorldWind.Terrain.TerrainTileService("http://worldwind25.arc.nasa.gov/wwelevation/wwelevation.aspx", "srtm30pluszip", 20, 150, "bil", 12, Path.Combine(Settings.CachePath, "Earth\\TerrainAccessor\\SRTM"), TimeSpan.FromMinutes(30), "Int16");
				WorldWind.Terrain.TerrainAccessor terrainAccessor = new WorldWind.Terrain.NltTerrainAccessor("SRTM", -180, -90, 180, 90, terrainTileService, null);

				WorldWind.World world = new WorldWind.World("Earth",
					new Point3d(0, 0, 0), Quaternion4d.RotationYawPitchRoll(0, 0, 0),
					(float)6378137,
					System.IO.Path.Combine(c_oWorldWindow.Cache.CacheDirectory, "Earth"),
					terrainAccessor);

				c_oWorldWindow.CurrentWorld = world;
				c_oWorldWindow.DrawArgs.WorldCamera.CameraChanged += new EventHandler(c_oWorldWindow_CameraChanged);

				string strPluginsDir = Path.Combine(DirectoryPath, "Plugins");

				this.scalebarPlugin = new NASA.Plugins.ScaleBarLegend();
				this.scalebarPlugin.PluginLoad(this, strPluginsDir);
				this.scalebarPlugin.IsVisible = World.Settings.ShowScaleBar;

				this.starsPlugin = new Stars3D.Plugin.Stars3D();
				this.starsPlugin.PluginLoad(this, Path.Combine(strPluginsDir, "Stars3D"));

				this.compassPlugin = new Murris.Plugins.Compass();
				this.compassPlugin.PluginLoad(this, Path.Combine(strPluginsDir, "Compass"));

				String szGlobalCloudsCacheDir = Path.Combine(Settings.CachePath, @"Plugins\GlobalClouds");
				Directory.CreateDirectory(szGlobalCloudsCacheDir);
				String szGlobalCloudsPluginDir = Path.Combine(CurrentSettingsDirectory, @"Plugins\GlobalClouds");
				Directory.CreateDirectory(szGlobalCloudsPluginDir);

				if (!File.Exists(Path.Combine(szGlobalCloudsPluginDir, Murris.Plugins.GlobalCloudsLayer.serverListFileName)))
				{
					File.Copy(Path.Combine(Path.Combine(strPluginsDir, "GlobalClouds"), Murris.Plugins.GlobalCloudsLayer.serverListFileName), Path.Combine(szGlobalCloudsPluginDir, Murris.Plugins.GlobalCloudsLayer.serverListFileName));
				}

				this.cloudsPlugin = new Murris.Plugins.GlobalClouds(szGlobalCloudsCacheDir);
				this.cloudsPlugin.PluginLoad(this, szGlobalCloudsPluginDir);

				this.skyPlugin = new Murris.Plugins.SkyGradient();
				this.skyPlugin.PluginLoad(this, Path.Combine(strPluginsDir, "SkyGradient"));

				this.threeDConnPlugin = new ThreeDconnexion.Plugin.TDxWWInput();
				this.threeDConnPlugin.PluginLoad(this, Path.Combine(strPluginsDir, "3DConnexion"));

				ThreadPool.QueueUserWorkItem(LoadPlacenames);

				c_scWorldMetadata.Panel1.Controls.Add(c_oWorldWindow);
				c_oWorldWindow.Dock = DockStyle.Fill;

				#endregion

				float[] verticalExaggerationMultipliers = { 0.0f, 1.0f, 1.5f, 2.0f, 3.0f, 5.0f, 7.0f, 10.0f };
				foreach (float multiplier in verticalExaggerationMultipliers)
				{
					ToolStripMenuItem curItem = new ToolStripMenuItem(multiplier.ToString("f1", System.Threading.Thread.CurrentThread.CurrentCulture) + "x", null, new EventHandler(menuItemVerticalExaggerationChange));
					c_miVertExaggeration.DropDownItems.Add(curItem);
					curItem.CheckOnClick = true;
					if (Math.Abs(multiplier - World.Settings.VerticalExaggeration) < 0.1f)
						curItem.Checked = true;
				}

				this.c_miShowCompass.Checked = World.Settings.ShowCompass;
				this.c_miShowDLProgress.Checked = World.Settings.ShowDownloadIndicator;
				this.c_miShowCrosshair.Checked = World.Settings.ShowCrosshairs;
				this.c_miShowInfoOverlay.Checked = World.Settings.ShowPosition;
				this.c_miShowGridlines.Checked = World.Settings.ShowLatLonLines;
				this.c_miShowGlobalClouds.Checked = World.Settings.ShowClouds;
				if (World.Settings.EnableSunShading)
				{
					if (!World.Settings.SunSynchedWithTime)
						this.c_miSunshadingEnabled.Checked = true;
					else
						this.c_miSunshadingSync.Checked = true;
				}
				else
					this.c_miSunshadingDisabled.Checked = true;
				this.c_miShowAtmoScatter.Checked = World.Settings.EnableAtmosphericScattering;

				this.c_miAskLastViewAtStartup.Checked = Settings.AskLastViewAtStartup;
				if (!Settings.AskLastViewAtStartup)
					this.c_miOpenLastViewAtStartup.Checked = Settings.LastViewAtStartup;


				#region OverviewPanel

				// Fix: earlier versions of Dapple set the DataPath as an absolute reference, so if Dapple was uninstalled, OMapple could not find
				// the file for the overview control.  To fix this, switch the variable to a relative reference if the absolute one doesn't resolve.
				// Dapple will still work; the relative reference will be from whatever directory Dapple is being run.
				if (!Directory.Exists(Settings.DataPath)) Settings.DataPath = "Data";

				#endregion


				c_oWorldWindow.MouseEnter += new EventHandler(this.c_oWorldWindow_MouseEnter);
				c_oWorldWindow.MouseLeave += new EventHandler(this.c_oWorldWindow_MouseLeave);

				c_oWorldWindow.ClearDevice();

				c_oOverview.AOISelected += new Overview.AOISelectedDelegate(c_oOverview_AOISelected);

				#region Search view setup

				this.c_oServerList = new ServerList();
				this.c_oServerTree = new ServerTree(m_oImageList, Settings.CachePath, this, c_oLayerList, c_oServerList);
				c_oServerList.ImageList = this.c_oServerTree.ImageList;
				c_oLayerList.ImageList = this.c_oServerTree.ImageList;
				c_oLayerList.ServerTree = c_oServerTree;
				c_oLayerList.LayerSelectionChanged += new EventHandler(c_oLayerList_LayerSelectionChanged);

				m_oMetadataDisplay = new MetadataDisplayThread(this);
				m_oMetadataDisplay.AddBuilder(null);
				c_oServerList.LayerList = c_oLayerList;
				c_oLayerList.GoTo += new LayerList.GoToHandler(this.GoTo);

				c_oLayerList.ViewMetadata += new ViewMetadataHandler(m_oMetadataDisplay.AddBuilder);
				c_oServerTree.ViewMetadata += new ViewMetadataHandler(m_oMetadataDisplay.AddBuilder);
				c_oServerList.ViewMetadata += new ViewMetadataHandler(m_oMetadataDisplay.AddBuilder);
				c_oServerList.LayerSelectionChanged += new EventHandler(c_oServerList_LayerSelectionChanged);

				this.c_oServerTree.AfterSelect += new TreeViewEventHandler(this.c_oServerTree_AfterSelected);
				this.c_oServerTree.Dock = System.Windows.Forms.DockStyle.Fill;
				this.c_oServerTree.ImageIndex = 0;
				this.c_oServerTree.Location = new System.Drawing.Point(0, 0);
				this.c_oServerTree.Name = "treeViewServers";
				this.c_oServerTree.SelectedImageIndex = 0;
				this.c_oServerTree.TabIndex = 0;

				this.cServerViewsTab = new JanaTab();
				this.cServerViewsTab.SetImage(0, Resources.tab_tree);
				this.cServerViewsTab.SetImage(1, Resources.tab_list);
				this.cServerViewsTab.SetToolTip(0, "Server tree view");
				this.cServerViewsTab.SetToolTip(1, "Server list view");
				this.cServerViewsTab.SetNameAndText(0, "TreeView");
				this.cServerViewsTab.SetNameAndText(1, "ListView");
				this.cServerViewsTab.SetPage(0, this.c_oServerTree);
				this.cServerViewsTab.SetPage(1, this.c_oServerList);
				cServerViewsTab.PageChanged += new JanaTab.PageChangedDelegate(ServerPageChanged);

				c_oDappleSearch = new DappleSearchList();
				c_oDappleSearch.LayerSelectionChanged += new EventHandler(c_oDappleSearch_LayerSelectionChanged);
				c_oDappleSearch.ServerTree = c_oServerTree;
				c_oDappleSearch.LayerList = c_oLayerList;

				c_tcSearchViews.TabPages[0].Controls.Add(cServerViewsTab);
				cServerViewsTab.Dock = DockStyle.Fill;
				c_tcSearchViews.TabPages[1].Controls.Add(c_oDappleSearch);
				c_oDappleSearch.Dock = DockStyle.Fill;

				c_oLayerList.SetBaseLayer(new BlueMarbleBuilder());

				this.ResumeLayout(false);

				#endregion

				this.PerformLayout();

				while (!this.splashScreen.IsDone)
					System.Threading.Thread.Sleep(50);

				// Force initial render to avoid showing random contents of frame buffer to user.
				c_oWorldWindow.Render();
				WorldWindow.Focus();

				#region OM Forked Process configuration

				if (IsMontajChildProcess)
				{
					c_oLayerList.OMFeaturesEnabled = true;
					this.MinimizeBox = false;

					if (oAoi != null && !string.IsNullOrEmpty(strAoiCoordinateSystem))
					{
						m_oOMMapExtentNative = oAoi;
						m_strAoiCoordinateSystem = strAoiCoordinateSystem;
						m_strOpenMapFileName = strMapFileName;

						m_oOMMapExtentWGS84 = m_oOMMapExtentNative.Clone() as GeographicBoundingBox;
						m_oMontajRemoteInterface.ProjectBoundingRectangle(strAoiCoordinateSystem, ref m_oOMMapExtentWGS84.West, ref m_oOMMapExtentWGS84.South, ref m_oOMMapExtentWGS84.East, ref m_oOMMapExtentWGS84.North, Dapple.Extract.Resolution.WGS_84);
					}
					m_eClientType = eClientType;

					c_miLastView.Enabled = false;
					c_miLastView.Visible = false;
					c_miDappleHelp.Visible = false;
					c_miDappleHelp.Enabled = false;
					toolStripSeparator10.Visible = false;
					c_miOpenImage.Visible = false;
					c_miOpenImage.Enabled = false;
					c_miOpenKeyhole.Visible = false;
					c_miOpenKeyhole.Enabled = false;

					// Hide and disable the file menu
					c_miFile.Visible = false;
					c_miFile.Enabled = false;
					c_miOpenSavedView.Visible = false;
					c_miOpenSavedView.Enabled = false;
					c_miOpenHomeView.Visible = false;
					c_miOpenHomeView.Enabled = false;
					c_miSetHomeView.Visible = false;
					c_miSetHomeView.Enabled = false;
					c_miSaveView.Visible = false;
					c_miSaveView.Enabled = false;
					c_miSendViewTo.Visible = false;
					c_miSendViewTo.Enabled = false;
					c_miOpenKeyhole.Visible = false;
					c_miOpenKeyhole.Enabled = false;

					// Show the OM help menu
					c_miGetDatahelp.Enabled = true;
					c_miGetDatahelp.Visible = true;

					// Don't let the user check for updates.  EVER.
					c_miCheckForUpdates.Visible = false;
					c_miCheckForUpdates.Enabled = false;
				}
				else
				{
					c_miExtractLayers.Visible = false;
					c_miExtractLayers.Enabled = false;
				}

				#endregion

				loadCountryList();
				populateAoiComboBox();
				LoadMRUList();
				CenterNavigationToolStrip();
				//#if !DEBUG

				c_tbSearchKeywords.Text = NO_SEARCH;
			}
			//#endif
		}

		#endregion

		#region Updates

		bool m_bUpdateFromMenu;
		delegate void NotifyUpdateDelegate(string strVersion);

		private void NotifyUpdate(string strVersion)
		{
			UpdateDialog dlg = new UpdateDialog(strVersion);

			if (dlg.ShowDialog(this) == DialogResult.Yes)
				MainForm.BrowseTo(MainForm.WebsiteUrl);
		}

		private void NotifyUpdateNotAvailable()
		{
			Program.ShowMessageBox(
				"Your version of Dapple is up-to-date.",
				"Check For Updates",
				MessageBoxButtons.OK,
				MessageBoxDefaultButton.Button1,
				MessageBoxIcon.Information);
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

				using (StreamReader sr = new StreamReader(Path.Combine(DirectoryPath, VersionFile)))
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
					this.BeginInvoke(new NotifyUpdateDelegate(NotifyUpdate), new object[] { strVersion });
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
					this.BeginInvoke(new MethodInvoker(NotifyUpdateNotAvailable));
			}
		}

		private void CheckForUpdates(bool bFromMenu)
		{
			m_bUpdateFromMenu = bFromMenu;
			WebDownload download = new WebDownload(WebsiteUrl + VersionFile);
			download.DownloadType = DownloadType.Unspecified;
			download.CompleteCallback += new DownloadCompleteHandler(UpdateDownloadComplete);
			download.BackgroundDownloadMemory();
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

		private delegate void UpdateDownloadIndicatorsDelegate(bool bDownloading, int iPos, int iTotal, List<ActiveDownload> newList);
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
							this.toolStripStatusLabel6.Image = this.c_oServerTree.ImageList.Images["marble"];
						}
						else
						{
							this.toolStripStatusLabel6.ToolTipText = m_downloadList[5].builder.Title;
							this.toolStripStatusLabel6.Image = this.c_oServerTree.ImageList.Images[m_downloadList[5].builder.ServerTypeIconKey];
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
							this.toolStripStatusLabel5.ToolTipText = m_downloadList[4].builder.Title;
							this.toolStripStatusLabel5.Image = this.c_oServerTree.ImageList.Images[m_downloadList[4].builder.ServerTypeIconKey];
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
							this.toolStripStatusLabel4.ToolTipText = m_downloadList[3].builder.Title;
							this.toolStripStatusLabel4.Image = this.c_oServerTree.ImageList.Images[m_downloadList[3].builder.ServerTypeIconKey];
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
							this.toolStripStatusLabel3.ToolTipText = m_downloadList[2].builder.Title;
							this.toolStripStatusLabel3.Image = this.c_oServerTree.ImageList.Images[m_downloadList[2].builder.ServerTypeIconKey];
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
							this.toolStripStatusLabel2.ToolTipText = m_downloadList[1].builder.Title;
							this.toolStripStatusLabel2.Image = this.c_oServerTree.ImageList.Images[m_downloadList[1].builder.ServerTypeIconKey];
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
							this.toolStripStatusLabel1.ToolTipText = m_downloadList[0].builder.Title;
							this.toolStripStatusLabel1.Image = this.c_oServerTree.ImageList.Images[m_downloadList[0].builder.ServerTypeIconKey];
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
				return c_oWorldWindow;
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

		#region Metadata displayer

		public delegate void StringParamDelegate(String szString);
		public delegate void LoadMetadataDelegate(IBuilder oBuilder);

		private void DisplayMetadataMessage(String szMessage)
		{
			if (InvokeRequired)
				this.Invoke(new StringParamDelegate(DisplayMetadataMessage), new Object[] { szMessage });
			else
			{
				c_wbMetadata.Visible = false;
				c_lMetadata.Text = szMessage;
			}
		}

		private void DisplayMetadataDocument(String szMessage)
		{
			c_wbMetadata.Visible = true;
			Uri metaUri = new Uri(szMessage);
			if (!metaUri.Equals(c_wbMetadata.Url))
			{
				// --- Delete the file we were pointing to before ---
				if (c_wbMetadata.Url != null && c_wbMetadata.Url.Scheme.Equals("file"))
				{
					File.Delete(c_wbMetadata.Url.LocalPath);
				}
				c_wbMetadata.Url = metaUri;
			}
		}
		
		private void LoadMetadata(IBuilder oBuilder)
		{
			if (InvokeRequired)
				this.Invoke(new LoadMetadataDelegate(LoadMetadata), new Object[] { oBuilder });
			else
			{
				try
				{
					XmlDocument oDoc = new XmlDocument();
					oDoc.AppendChild(oDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes"));
					XmlNode oNode = null;
					string strStyleSheet = null;

					if ((oBuilder is AsyncBuilder) && (oBuilder as AsyncBuilder).LoadingErrorOccurred)
					{
						DisplayMetadataMessage("The selected object failed to load.");
						return;
					}
					else if (oBuilder.SupportsMetaData)
					{
						oNode = oBuilder.GetMetaData(oDoc);
						if (oNode == null)
						{
							DisplayMetadataMessage("You do not have permission to view the metadata for this data layer.");
							return;
						}
						strStyleSheet = oBuilder.StyleSheetName;
						DisplayMetadataMessage("Loading metadata for layer " + oBuilder.Title + "...");
					}
					else
					{
						DisplayMetadataMessage("Metadata for the selected object is unsupported.");
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

			public void AddBuilder(IBuilder oBuilder)
			{
				lock (LOCK)
				{
					m_hLayerToLoad.Add(oBuilder);
					m_oSignaller.Set();
				}
			}

			public void Abort()
			{
				m_hThread.Abort();
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

					if (oCurrentBuilder == null)
					{
						m_hOwner.DisplayMetadataMessage("Select a dataset or server to view its associated metadata.");
						oLastBuilder = oCurrentBuilder;
					}
					else
					{
						if (oCurrentBuilder is AsyncBuilder)
							((AsyncBuilder)oCurrentBuilder).WaitUntilLoaded();

						if (!oCurrentBuilder.Equals(oLastBuilder))
						{
							m_hOwner.LoadMetadata(oCurrentBuilder);
							oLastBuilder = oCurrentBuilder;
						}
					}
				}
			}
		}

		#endregion

		#region Menu Item Event Handlers

		private void c_miCheckForUpdates_Click(object sender, EventArgs e)
		{
			CheckForUpdates(true);
		}

		private void c_miOpenImage_Click(object sender, EventArgs e)
		{
			string strLastFolderCfg = Path.Combine(Path.Combine(UserPath, Settings.ConfigPath), "opengeotif.cfg");

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

		private void c_miAddDAPServer_Click(object sender, EventArgs e)
		{
			AddDAPServer();
		}

		private void c_miAddWMSServer_Click(object sender, EventArgs e)
		{
			AddWMSServer();
		}

		private void c_miAddArcIMSServer_Click(object sender, EventArgs e)
		{
			AddArcIMSServer();
		}

		private void c_miAskLastViewAtStartup_Click(object sender, EventArgs e)
		{
			c_miOpenLastViewAtStartup.Checked = false;
			Settings.AskLastViewAtStartup = c_miAskLastViewAtStartup.Checked;
		}

		private void c_miOpenLastViewAtStartup_Click(object sender, EventArgs e)
		{
			Settings.AskLastViewAtStartup = false;
			this.c_miAskLastViewAtStartup.Checked = false;
			Settings.LastViewAtStartup = c_miOpenLastViewAtStartup.Checked;
		}

		private void c_miAdvancedSettings_Click(object sender, EventArgs e)
		{
			Wizard wiz = new Wizard(Settings);
			wiz.ShowDialog(this);
		}

		/// <summary>
		/// Handler for a change in the selection of the vertical exaggeration from the main menu
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void menuItemVerticalExaggerationChange(object sender, EventArgs e)
		{
			foreach (ToolStripMenuItem item in c_miVertExaggeration.DropDownItems)
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
			c_oWorldWindow.Invalidate();
		}

		private void c_miShowCompass_Click(object sender, EventArgs e)
		{
			World.Settings.ShowCompass = c_miShowCompass.Checked;
			this.compassPlugin.Layer.IsOn = World.Settings.ShowCompass;
		}

		private void c_miShowDLProgress_Click(object sender, EventArgs e)
		{
			World.Settings.ShowDownloadIndicator = c_miShowDLProgress.Checked;
		}

		private void c_miShowCrosshair_Click(object sender, EventArgs e)
		{
			World.Settings.ShowCrosshairs = c_miShowCrosshair.Checked;
		}

		private void c_miOpenSavedView_Click(object sender, EventArgs e)
		{
			ViewOpenDialog dlgtest = new ViewOpenDialog(Path.Combine(UserPath, Settings.ConfigPath));
			DialogResult res = dlgtest.ShowDialog(this);
			if (dlgtest.ViewFile != null)
			{
				if (res == DialogResult.OK)
					OpenView(dlgtest.ViewFile, true, true);
			}
		}

		private void c_miOpenHomeView_Click(object sender, EventArgs e)
		{
			CmdLoadHomeView();
		}

		private void c_miShowGridLines_Click(object sender, EventArgs e)
		{
			World.Settings.ShowLatLonLines = c_miShowGridlines.Checked;
			foreach (RenderableObject oRO in c_oWorldWindow.CurrentWorld.RenderableObjects.ChildObjects)
			{
				if (oRO.Name == "1 - Grid Lines")
				{
					oRO.IsOn = c_miShowGridlines.Checked;
					break;
				}
			}
		}

		private void c_miShowInfoOverlay_Click(object sender, EventArgs e)
		{
			World.Settings.ShowPosition = c_miShowInfoOverlay.Checked;
			c_oWorldWindow.Invalidate();
		}

		private void c_miSaveView_Click(object sender, EventArgs e)
		{
			string tempViewFile = Path.ChangeExtension(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()), ViewExt);
			string tempFile = Path.ChangeExtension(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()), ".jpg");
			SaveCurrentView(tempViewFile, tempFile, "");
			Image img = Image.FromFile(tempFile);
			SaveViewForm form = new SaveViewForm(Path.Combine(UserPath, Settings.ConfigPath), img);

			if (form.ShowDialog(this) == DialogResult.OK)
			{
				if (File.Exists(form.OutputPath))
					File.Delete(form.OutputPath);

				XmlDocument oDoc = new XmlDocument();
				oDoc.Load(tempViewFile);
				XmlNode oRoot = oDoc.DocumentElement;
				XmlNode oNode = oDoc.CreateElement("notes");
				oNode.InnerText = form.Notes;
				oRoot.AppendChild(oNode);
				oDoc.Save(form.OutputPath);
			}

			img.Dispose();
			if (File.Exists(tempFile)) File.Delete(tempFile);
			if (File.Exists(tempViewFile)) File.Delete(tempViewFile);
		}

		private void c_miSendViewTo_Click(object sender, EventArgs e)
		{
			string tempBodyFile = Path.ChangeExtension(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()), ".txt");
			string tempJpgFile = Path.ChangeExtension(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()), ".jpg");
			string tempViewFile = Path.ChangeExtension(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()), ViewExt);
			string strMailApp = Path.Combine(Path.Combine(DirectoryPath, "System"), "mailer.exe");

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
					// Let the screen draw so it doesn't look damaged.
					Application.DoEvents();
					p.WaitForExit();
				}
			}
			catch (Exception ex)
			{
				Program.ShowMessageBox(
					"An unexpected error occurred sending the view:\n" + ex.Message,
					"Send View To",
					MessageBoxButtons.OK,
					MessageBoxDefaultButton.Button1,
					MessageBoxIcon.Error);
			}

			File.Delete(tempBodyFile);
			File.Delete(tempJpgFile);
			File.Delete(tempViewFile);
		}

		private void c_miShowPlaceNames_Click(object sender, EventArgs e)
		{
			if (this.placeNames == null) return;

			World.Settings.ShowPlacenames = !World.Settings.ShowPlacenames;
			this.c_miShowPlaceNames.Checked = World.Settings.ShowPlacenames;
			this.placeNames.IsOn = World.Settings.ShowPlacenames;
		}

		private void c_miShowScaleBar_Click(object sender, EventArgs e)
		{
			this.scalebarPlugin.IsVisible = !this.scalebarPlugin.IsVisible;
			World.Settings.ShowScaleBar = this.scalebarPlugin.IsVisible;
			this.c_miShowScaleBar.Checked = this.scalebarPlugin.IsVisible;
		}

		private void c_miSunshadingEnabled_Click(object sender, EventArgs e)
		{
			World.Settings.EnableSunShading = true;
			World.Settings.SunSynchedWithTime = false;
			this.c_miSunshadingEnabled.Checked = true;
			this.c_miSunshadingSync.Checked = false;
			this.c_miSunshadingDisabled.Checked = false;
		}

		private void c_miSunshadingSync_Click(object sender, EventArgs e)
		{
			World.Settings.EnableSunShading = true;
			World.Settings.SunSynchedWithTime = true;
			this.c_miSunshadingEnabled.Checked = false;
			this.c_miSunshadingSync.Checked = true;
			this.c_miSunshadingDisabled.Checked = false;
		}

		private void c_miSunshadingDisabled_Click(object sender, EventArgs e)
		{
			World.Settings.EnableSunShading = false;
			World.Settings.SunSynchedWithTime = false;
			this.c_miSunshadingEnabled.Checked = false;
			this.c_miSunshadingSync.Checked = false;
			this.c_miSunshadingDisabled.Checked = true;
		}

		private void c_miShowAtmoScatter_Click(object sender, EventArgs e)
		{
			World.Settings.EnableAtmosphericScattering = !World.Settings.EnableAtmosphericScattering;
			this.c_miShowAtmoScatter.Checked = World.Settings.EnableAtmosphericScattering;
		}

		private void c_miShowGlobalClouds_Click(object sender, EventArgs e)
		{
			World.Settings.ShowClouds = !World.Settings.ShowClouds;
			this.c_miShowGlobalClouds.Checked = World.Settings.ShowClouds;
			this.cloudsPlugin.layer.IsOn = World.Settings.ShowClouds;
		}

		private void c_miExit_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void c_miView_DropDownOpening(object sender, EventArgs e)
		{
			this.c_miShowScaleBar.Checked = this.scalebarPlugin.IsVisible;
		}

		private void c_miHelpAbout_Click(object sender, EventArgs e)
		{
			AboutDialog dlg = new AboutDialog();
			dlg.ShowDialog(this);
		}

		private void c_miHelpHomepage_Click(object sender, EventArgs e)
		{
			MainForm.BrowseTo(MainForm.WebsiteUrl);
		}

		private void c_miHelpForums_Click(object sender, EventArgs e)
		{
			MainForm.BrowseTo(MainForm.WebsiteForumsHelpUrl);
		}

		private void c_miHelpWebDocs_Click(object sender, EventArgs e)
		{
			MainForm.BrowseTo(MainForm.WebsiteHelpUrl);
		}

		private void c_miAddLayer_Click(object sender, EventArgs e)
		{
			AddDatasetAction();
		}

		private void c_miExtractLayers_Click(object sender, EventArgs e)
		{
			c_oLayerList.CmdExtractVisibleLayers();
		}

		private void c_miSearch_Click(object sender, EventArgs e)
		{
			doSearch();
		}

		private void c_miViewProperties_Click(object sender, EventArgs e)
		{
			this.c_oServerTree.CmdServerProperties();
		}

		private void c_miTakeSnapshot_Click(object sender, EventArgs e)
		{
			c_oLayerList.CmdTakeSnapshot();
		}

		private void c_miRemoveLayer_Click(object sender, EventArgs e)
		{
			c_oLayerList.CmdRemoveSelectedLayers();
		}

		private void c_miRefreshServer_Click(object sender, EventArgs e)
		{
			this.c_oServerTree.RefreshCurrentServer();
		}

		private void c_miRemoveServer_Click(object sender, EventArgs e)
		{
			this.c_oServerTree.RemoveCurrentServer();
		}

		private void c_miSetHomeView_Click(object sender, EventArgs e)
		{
			CmdSaveHomeView();
		}

		private void c_oDappleSearch_LayerSelectionChanged(object sender, EventArgs e)
		{
			CmdSetupToolsMenu();
		}

		private void c_oLayerList_LayerSelectionChanged(object sender, EventArgs e)
		{
			c_miRemoveLayer.Enabled = c_oLayerList.RemoveAllowed;
		}

		private void c_tcSearchViews_SelectedIndexChanged(object sender, EventArgs e)
		{
			CmdSetupServersMenu();
			CmdSetupToolsMenu();
		}

		private void c_miAddBrowserMap_Click(object sender, EventArgs e)
		{
			c_oServerTree.CmdAddBrowserMap();
		}

		private void c_miGetDataHelp_Click(object sender, EventArgs e)
		{
			CmdDisplayOMHelp();
		}

		private void c_miSetFavouriteServer_Click(object sender, EventArgs e)
		{
			c_oServerTree.CmdSetFavoriteServer();
		}

		private void c_miToggleServerStatus_Click(object sender, EventArgs e)
		{
			c_oServerTree.CmdToggleServerEnabled();
		}

		private void c_miOpenKeyhole_Click(object sender, EventArgs e)
		{
			OpenFileDialog bob = new OpenFileDialog();
			bob.Filter = "KML/KMZ Files|*.kml;*.kmz";
			bob.FilterIndex = 1;
			bob.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			bob.Title = "Select Keyhole File to Open...";
			bob.Multiselect = false;

			if (bob.ShowDialog() == DialogResult.OK && File.Exists(bob.FileName))
			{
				AddLayerBuilder(new Dapple.KML.KMLLayerBuilder(bob.FileName, WorldWindowSingleton, null));
			}
		}

		#endregion

		#region MainForm Event Handlers

		private void MainForm_Shown(object sender, EventArgs e)
		{
			if (IsMontajChildProcess)
			{
				try
				{
					m_oMontajRemoteInterface.StartConnection();
				}
				catch (System.Runtime.Remoting.RemotingException ex)
				{
					throw new System.Runtime.Remoting.RemotingException("A communication error occurred between Dapple and OM during startup", ex);
				}
			}

			// Render once to not just show the atmosphere at startup (looks better) ---
			c_oWorldWindow.SafeRender();


			//tvServers.LoadFavoritesList(Path.Combine(Path.Combine(UserPath, "Config"), "user.dapple_serverlist"));

			try
			{
				// --- Draw the screen, so it doesn't look damaged ---
				UseWaitCursor = true;
				Application.DoEvents();

				if (this.openView.Length > 0)
					OpenView(this.openView, this.openGeoTiff.Length == 0, true);
				else if (!IsMontajChildProcess && File.Exists(Path.Combine(Path.Combine(UserPath, Settings.ConfigPath), LastView)))
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
						OpenView(Path.Combine(Path.Combine(UserPath, Settings.ConfigPath), LastView), true, true);
					else
						CmdLoadHomeView();
				}
				else
				{
					CmdLoadHomeView();
				}

				if (this.openGeoTiff.Length > 0)
					AddGeoTiff(this.openGeoTiff, this.openGeoTiffName, this.openGeoTiffTmp, true);


				// Check for updates daily
				if (IsMontajChildProcess == false && Settings.UpdateCheckDate.Date != System.DateTime.Now.Date)
					CheckForUpdates(false);
				Settings.UpdateCheckDate = System.DateTime.Now;

				foreach (RenderableObject oRO in c_oWorldWindow.CurrentWorld.RenderableObjects.ChildObjects)
				{
					if (oRO.Name == "1 - Grid Lines")
					{
						oRO.IsOn = World.Settings.ShowLatLonLines;
						break;
					}
				}

				if (m_oOMMapExtentWGS84 != null)
				{
					doSearch(String.Empty, m_oOMMapExtentWGS84);
					GoTo(m_oOMMapExtentWGS84, false);
				}

				c_oOverview.StartRenderTimer();
			}
			finally
			{
				UseWaitCursor = false;
			}
		}

		bool m_bSizing = false;
		private void MainForm_ResizeBegin(object sender, EventArgs e)
		{
			m_bSizing = true;
			c_oWorldWindow.Visible = false;
		}

		private void MainForm_ResizeEnd(object sender, EventArgs e)
		{
			m_bSizing = false;
			c_oWorldWindow.Visible = true;
			c_oWorldWindow.SafeRender();
		}

		private void MainForm_Resize(object sender, EventArgs e)
		{
			c_oWorldWindow.Visible = false;
		}

		private void MainForm_SizeChanged(object sender, EventArgs e)
		{
			if (!m_bSizing)
			{
				c_oWorldWindow.Visible = true;
				c_oWorldWindow.SafeRender();
			}
		}

		private void MainForm_Load(object sender, EventArgs e)
		{

			c_oWorldWindow.IsRenderDisabled = false;

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
			c_oWorldWindow.Updated += new WorldWindow.UpdatedDelegate(c_oWorldWindow_Updated);

			this.c_oServerTree.Load();
		}

		private void MainForm_Closing(object sender, CancelEventArgs e)
		{
			// Turn off the metadata display thread and background search thread
			m_oMetadataDisplay.Abort();

			this.threeDConnPlugin.Unload();

			// Turning off the layers will set this
			bool bSaveGridLineState = World.Settings.ShowLatLonLines;

			this.WindowState = FormWindowState.Minimized;

			SaveLastView();
			//tvServers.SaveFavoritesList(Path.Combine(Path.Combine(UserPath, "Config"), "user.dapple_serverlist"));

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

			// Don't force-dispose the WorldWindow (or, really, anything), just let .NET free it up for us.
			// Should kill the background worker thread though.
			c_oWorldWindow.KillWorkerThread();

			// --- Delete all the temporary XML files the metadata viewer has been pumping out ---

			foreach (String bob in Directory.GetFiles(metaviewerDir, "*.xml"))
			{
				try
				{
					File.Delete(bob);
				}
				catch (System.IO.IOException)
				{
					// Couldn't delete a temp file?  Not the end of the world.
				}
			}

			SaveMRUList();
		}

		private void MainForm_Deactivate(object sender, EventArgs e)
		{
			c_oWorldWindow.IsRenderDisabled = true;
		}

		private void MainForm_Activated(object sender, EventArgs e)
		{
			c_oWorldWindow.IsRenderDisabled = false;
		}

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
						OpenView(strView, strGeoTiff.Length == 0, true);
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

		#region World Window Event Handlers

		private void c_oWorldWindow_MouseLeave(object sender, EventArgs e)
		{
			c_scMain.Panel1.Select();
		}

		private void c_oWorldWindow_MouseEnter(object sender, EventArgs e)
		{
			c_oWorldWindow.Select();
		}

		private void c_oWorldWindow_Updated()
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

			foreach (LayerBuilder oBuilder in c_oLayerList.AllLayers)
			{
				if (oBuilder.bIsDownloading(out iBuilderPos, out iBuilderTotal))
				{
					bDownloading = true;
					iPos += iBuilderPos;
					iTotal += iBuilderTotal;
					ActiveDownload dl = new ActiveDownload();
					dl.builder = oBuilder;
					dl.iPos = iBuilderPos;
					dl.iTotal = iBuilderTotal;
					dl.bOn = true;
					dl.bRead = false;
					currentList.Add(dl);
				}
			}

			// In rare cases, the WorldWindow's background worker thread might start sending back updates before the
			// MainForm's window handle has been created.  Don't let it, or you'll cascade the system into failure.
			if (this.IsHandleCreated)
			{
				this.BeginInvoke(new UpdateDownloadIndicatorsDelegate(UpdateDownloadIndicators), new object[] { bDownloading, iPos, iTotal, currentList });
			}
		}

		private void c_oWorldWindow_DragOver(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(typeof(List<LayerBuilder>)))
			{
				e.Effect = DragDropEffects.Copy;
			}
		}

		private void c_oWorldWindow_DragDrop(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(typeof(List<LayerBuilder>)))
			{
				List<LayerBuilder> oDropData = e.Data.GetData(typeof(List<LayerBuilder>)) as List<LayerBuilder>;
				c_oLayerList.AddLayers(oDropData);
			}
		}

		void c_oWorldWindow_CameraChanged(object sender, EventArgs e)
		{
			if (m_oLastSearchROI != null)
			{
				SetSearchable(!GeographicBoundingBox.FromQuad(c_oWorldWindow.GetSearchBox()).Equals(m_oLastSearchROI));
			}
		}

		#endregion

		#region Other Event Handlers

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

		private void c_bResetTilt_Click(object sender, EventArgs e)
		{
			c_oWorldWindow.DrawArgs.WorldCamera.SlerpPercentage = World.Settings.CameraSlerpInertia;
			c_oWorldWindow.DrawArgs.WorldCamera.SetPosition(
					 c_oWorldWindow.Latitude,
					 c_oWorldWindow.Longitude,
					  c_oWorldWindow.DrawArgs.WorldCamera.Heading.Degrees,
					  c_oWorldWindow.DrawArgs.WorldCamera.Altitude,
					  0);

		}

		private void c_bResetRotation_Click(object sender, EventArgs e)
		{
			c_oWorldWindow.DrawArgs.WorldCamera.SlerpPercentage = World.Settings.CameraSlerpInertia;
			c_oWorldWindow.DrawArgs.WorldCamera.SetPosition(
					 c_oWorldWindow.Latitude,
					 c_oWorldWindow.Longitude,
					  0,
					  c_oWorldWindow.DrawArgs.WorldCamera.Altitude,
					  c_oWorldWindow.DrawArgs.WorldCamera.Tilt.Degrees);
		}

		private void c_bResetCamera_Click(object sender, EventArgs e)
		{
			c_oWorldWindow.DrawArgs.WorldCamera.SlerpPercentage = World.Settings.CameraSlerpInertia;
			c_oWorldWindow.DrawArgs.WorldCamera.Reset();
		}

		private void timerNavigation_Tick(object sender, EventArgs e)
		{
			this.bNavTimer = true;
			c_oWorldWindow.DrawArgs.WorldCamera.SlerpPercentage = 1.0;
			switch (this.eNavMode)
			{
				case NavMode.ZoomIn:
					c_oWorldWindow.DrawArgs.WorldCamera.Zoom(0.2f);
					return;
				case NavMode.ZoomOut:
					c_oWorldWindow.DrawArgs.WorldCamera.Zoom(-0.2f);
					return;
				case NavMode.RotateLeft:
					Angle rotateClockwise = Angle.FromRadians(-0.01f);
					c_oWorldWindow.DrawArgs.WorldCamera.Heading += rotateClockwise;
					c_oWorldWindow.DrawArgs.WorldCamera.RotationYawPitchRoll(Angle.Zero, Angle.Zero, rotateClockwise);
					return;
				case NavMode.RotateRight:
					Angle rotateCounterclockwise = Angle.FromRadians(0.01f);
					c_oWorldWindow.DrawArgs.WorldCamera.Heading += rotateCounterclockwise;
					c_oWorldWindow.DrawArgs.WorldCamera.RotationYawPitchRoll(Angle.Zero, Angle.Zero, rotateCounterclockwise);
					return;
				case NavMode.TiltUp:
					c_oWorldWindow.DrawArgs.WorldCamera.Tilt += Angle.FromDegrees(-1.0f);
					return;
				case NavMode.TiltDown:
					c_oWorldWindow.DrawArgs.WorldCamera.Tilt += Angle.FromDegrees(1.0f);
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

		private void c_bRotateRight_MouseDown(object sender, MouseEventArgs e)
		{
			this.timerNavigation.Enabled = true;
			this.bNavTimer = false;
			this.eNavMode = NavMode.RotateLeft;
		}

		private void c_bRotateLeft_MouseDown(object sender, MouseEventArgs e)
		{
			this.timerNavigation.Enabled = true;
			this.bNavTimer = false;
			this.eNavMode = NavMode.RotateRight;
		}

		private void c_bTiltDown_MouseDown(object sender, MouseEventArgs e)
		{
			this.timerNavigation.Enabled = true;
			this.bNavTimer = false;
			this.eNavMode = NavMode.TiltDown;
		}

		private void c_bTiltUp_MouseDown(object sender, MouseEventArgs e)
		{
			this.timerNavigation.Enabled = true;
			this.bNavTimer = false;
			this.eNavMode = NavMode.TiltUp;
		}

		private void c_bZoomOut_MouseDown(object sender, MouseEventArgs e)
		{
			this.timerNavigation.Enabled = true;
			this.bNavTimer = false;
			this.eNavMode = NavMode.ZoomOut;
		}

		private void c_bZoomIn_MouseDown(object sender, MouseEventArgs e)
		{
			this.timerNavigation.Enabled = true;
			this.bNavTimer = false;
			this.eNavMode = NavMode.ZoomIn;
		}

		private void c_bZoomIn_Click(object sender, EventArgs e)
		{
			this.timerNavigation.Enabled = false;
			if (!this.bNavTimer)
			{
				c_oWorldWindow.DrawArgs.WorldCamera.SlerpPercentage = World.Settings.CameraSlerpInertia;
				c_oWorldWindow.DrawArgs.WorldCamera.Zoom(2.0f);
			}
			else
				this.bNavTimer = false;
		}

		private void c_bZoomOut_Click(object sender, EventArgs e)
		{
			this.timerNavigation.Enabled = false;
			if (!this.bNavTimer)
			{
				c_oWorldWindow.DrawArgs.WorldCamera.SlerpPercentage = World.Settings.CameraSlerpInertia;
				c_oWorldWindow.DrawArgs.WorldCamera.Zoom(-2.0f);
			}
			else
				this.bNavTimer = false;
		}

		private void c_bRotateRight_Click(object sender, EventArgs e)
		{
			this.timerNavigation.Enabled = false;
			if (!this.bNavTimer)
			{
				Angle rotateClockwise = Angle.FromRadians(-0.2f);
				c_oWorldWindow.DrawArgs.WorldCamera.SlerpPercentage = World.Settings.CameraSlerpInertia;
				c_oWorldWindow.DrawArgs.WorldCamera.Heading += rotateClockwise;
				c_oWorldWindow.DrawArgs.WorldCamera.RotationYawPitchRoll(Angle.Zero, Angle.Zero, rotateClockwise);
			}
			else
				this.bNavTimer = false;
		}

		private void c_bRotateLeft_Click(object sender, EventArgs e)
		{
			this.timerNavigation.Enabled = false;
			if (!this.bNavTimer)
			{
				Angle rotateCounterclockwise = Angle.FromRadians(0.2f);
				c_oWorldWindow.DrawArgs.WorldCamera.SlerpPercentage = World.Settings.CameraSlerpInertia;
				c_oWorldWindow.DrawArgs.WorldCamera.Heading += rotateCounterclockwise;
				c_oWorldWindow.DrawArgs.WorldCamera.RotationYawPitchRoll(Angle.Zero, Angle.Zero, rotateCounterclockwise);
			}
			else
				this.bNavTimer = false;
		}

		private void c_bTiltUp_Click(object sender, EventArgs e)
		{
			this.timerNavigation.Enabled = false;
			if (!this.bNavTimer)
			{
				c_oWorldWindow.DrawArgs.WorldCamera.SlerpPercentage = World.Settings.CameraSlerpInertia;
				c_oWorldWindow.DrawArgs.WorldCamera.Tilt += Angle.FromDegrees(-10.0f);
			}
			else
				this.bNavTimer = false;
		}

		private void c_bTiltDown_Click(object sender, EventArgs e)
		{
			this.timerNavigation.Enabled = false;
			if (!this.bNavTimer)
			{
				c_oWorldWindow.DrawArgs.WorldCamera.SlerpPercentage = World.Settings.CameraSlerpInertia;
				c_oWorldWindow.DrawArgs.WorldCamera.Tilt += Angle.FromDegrees(10.0f);
			}
			else
				this.bNavTimer = false;
		}

		#endregion

		private void ServerPageChanged(int iIndex)
		{
			if (iIndex == 0) // Changed to tree view
			{
				c_oServerTree.SelectedServer = c_oServerList.SelectedServer;
			}
			else if (iIndex == 1) // Changed to list view
			{
				c_oServerList.SelectedServer = c_oServerTree.SelectedServer;
			}

			CmdSetupToolsMenu();
			CmdSetupServersMenu();
		}

		private void c_scWorldMetadata_Panel1_Resize(object sender, EventArgs e)
		{
			CenterNavigationToolStrip();
		}

		private void c_oOverview_AOISelected(object sender, GeographicBoundingBox bounds)
		{
			GoTo(bounds, false);
		}

		private void c_tbSearchKeywords_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar.Equals((char)13))
			{
				doSearch();
			}
		}

		private void c_bSearch_Click(object sender, EventArgs e)
		{
			doSearch();
		}

		private bool m_blSupressSearchSelectedIndexChanged = false;
		private void c_tbSearchKeywords_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (m_blSupressSearchSelectedIndexChanged) return;
			doSearch();
		}

		#region Globe visibility disabling

		private void c_scMain_SplitterMoving(object sender, SplitterCancelEventArgs e)
		{
			c_oWorldWindow.Visible = false;
		}

		private void c_scMain_SplitterMoved(object sender, SplitterEventArgs e)
		{
			if (!m_bSizing)
			{
				c_oWorldWindow.Visible = true;
				c_oWorldWindow.SafeRender();
			}
		}

		private void c_scWorldMetadata_SplitterMoving(object sender, SplitterCancelEventArgs e)
		{
			c_oWorldWindow.Visible = false;
		}

		private void c_scWorldMetadata_SplitterMoved(object sender, SplitterEventArgs e)
		{
			if (!m_bSizing)
			{
				c_oWorldWindow.Visible = true;
				c_oWorldWindow.SafeRender();
			}
		}

		#endregion

		private void c_oServerTree_AfterSelected(object sender, TreeViewEventArgs e)
		{
			populateAoiComboBox();
			CmdSetupToolsMenu();
			CmdSetupServersMenu();
		}

		private void c_oServerList_LayerSelectionChanged(object sender, EventArgs e)
		{
			CmdSetupToolsMenu();
		}

		private void c_tbSearchKeywords_Enter(object sender, EventArgs e)
		{
			if (c_tbSearchKeywords.Text.Equals(NO_SEARCH))
			{
				c_tbSearchKeywords.Text = String.Empty;
			}
			c_tbSearchKeywords.ForeColor = SystemColors.ControlText;
		}

		private void c_tbSearchKeywords_Leave(object sender, EventArgs e)
		{
			if (c_tbSearchKeywords.Text.Equals(String.Empty))
			{
				c_tbSearchKeywords.Text = NO_SEARCH;
				c_tbSearchKeywords.ForeColor = SystemColors.GrayText;
			}
		}

		private void c_bClearSearch_Click(object sender, EventArgs e)
		{
			clearSearch();
		}

		private void c_tbSearchKeywords_TextUpdate(object sender, EventArgs e)
		{
			if (!SearchKeyword.Equals(m_szLastSearchString))
			{
				SetSearchable(true);
			}
		}

		#endregion

		#region Commands

		#region ImageList Queries

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
			switch (strType.ToLower())
			{
				case "database":
					return ImageListIndex("dap_database");
				case "document":
					return ImageListIndex("dap_document");
				case "generic":
					return ImageListIndex("dap_map");
				case "grid":
					return ImageListIndex("dap_grid");
				case "gridsection":
					return ImageListIndex("dap_grid");
				case "map":
					return ImageListIndex("dap_map");
				case "picture":
					return ImageListIndex("dap_picture");
				case "picturesection":
					return ImageListIndex("dap_picture");
				case "point":
					return ImageListIndex("dap_point");
				case "spf":
					return ImageListIndex("dap_spf");
				case "voxel":
					return ImageListIndex("dap_voxel");
				case "imageserver":
					return ImageListIndex("arcims");
				case "arcgis":
					return ImageListIndex("dap_arcgis");
				default:
					return 3;
			}
		}

		#endregion

		#region AOIs

		private void loadCountryList()
		{
			if (m_oCountryAOIs == null)
			{
				m_oCountryAOIs = new Dictionary<string, GeographicBoundingBox>();
				String[] straCountries = Resources.aoi_region.Split(new String[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
				for (int count = 1; count < straCountries.Length - 1; count++)
				{
					String[] data = straCountries[count].Split(new char[] { ',' });
					double minX, minY, maxX, maxY;
					if (!Double.TryParse(data[1], NumberStyles.Any, CultureInfo.InvariantCulture, out minX)) continue;
					if (!Double.TryParse(data[2], NumberStyles.Any, CultureInfo.InvariantCulture, out minY)) continue;
					if (!Double.TryParse(data[3], NumberStyles.Any, CultureInfo.InvariantCulture, out maxX)) continue;
					if (!Double.TryParse(data[4], NumberStyles.Any, CultureInfo.InvariantCulture, out maxY)) continue;

					m_oCountryAOIs.Add(data[0], new GeographicBoundingBox(maxY, minY, minX, maxX));
				}
			}
		}

		private void populateAoiComboBox()
		{
			List<KeyValuePair<String, GeographicBoundingBox>> oAois = new List<KeyValuePair<string, GeographicBoundingBox>>();

			oAois.Add(new KeyValuePair<String, GeographicBoundingBox>("--- Select a specific region ---", null));

			if (this.c_oServerTree.SelectedNode != null && this.c_oServerTree.SelectedNode.Tag is Geosoft.GX.DAPGetData.Server)
			{
				Server oServer = this.c_oServerTree.SelectedNode.Tag as Server;
				if (oServer.Status == Server.ServerStatus.OnLine)
				{
					oAois.Add(new KeyValuePair<String, GeographicBoundingBox>("Server extent", new GeographicBoundingBox(oServer.ServerExtents.MaxY, oServer.ServerExtents.MinY, oServer.ServerExtents.MinX, oServer.ServerExtents.MaxX)));
				}
			}

			if (IsMontajChildProcess && m_oOMMapExtentWGS84 != null) oAois.Add(new KeyValuePair<String, GeographicBoundingBox>("Original map extent", m_oOMMapExtentWGS84));

			oAois.Add(new KeyValuePair<String, GeographicBoundingBox>("-----------------------------", null));

			if (this.c_oServerTree.SelectedNode != null && this.c_oServerTree.SelectedNode.Tag is Geosoft.GX.DAPGetData.Server)
			{
				if (((Geosoft.GX.DAPGetData.Server)this.c_oServerTree.SelectedNode.Tag).Status == Geosoft.GX.DAPGetData.Server.ServerStatus.OnLine &&
					((Geosoft.GX.DAPGetData.Server)this.c_oServerTree.SelectedNode.Tag).Enabled)
				{
					ArrayList aAOIs = ((Geosoft.GX.DAPGetData.Server)this.c_oServerTree.SelectedNode.Tag).ServerConfiguration.GetAreaList();
					foreach (String strAOI in aAOIs)
					{
						double minX, minY, maxX, maxY;
						String strCoord;
						((Geosoft.GX.DAPGetData.Server)this.c_oServerTree.SelectedNode.Tag).ServerConfiguration.GetBoundingBox(strAOI, out maxX, out maxY, out minX, out minY, out strCoord);
						if (strCoord.Equals("WGS 84"))
						{
							GeographicBoundingBox oBox = new GeographicBoundingBox(maxY, minY, minX, maxX);
							oAois.Add(new KeyValuePair<String, GeographicBoundingBox>(strAOI, oBox));
						}
					}
				}
			}
			else
			{
				foreach (KeyValuePair<String, GeographicBoundingBox> country in m_oCountryAOIs)
				{
					oAois.Add(country);
				}
			}

			c_oOverview.SetAOIList(oAois);
		}

		#endregion

		#region Placename Loading

		private void LoadPlacenames(Object oParams)
		{
			this.placeNames = ConfigurationLoader.getRenderableFromLayerFile(Path.Combine(CurrentSettingsDirectory, "^Placenames.xml"), this.WorldWindow.CurrentWorld, this.WorldWindow.Cache, true, null);
			try
			{
				if (!this.IsDisposed)
					Invoke(new MethodInvoker(LoadPlacenamesCallback));
			}
			catch (ObjectDisposedException)
			{
				// --- The user closed the form before the placenames were loaded.  Ignore, since we're shutting down anyway. ---
			}
		}

		private void LoadPlacenamesCallback()
		{
			if (this.placeNames != null)
			{
				this.placeNames.IsOn = World.Settings.ShowPlacenames;
				this.placeNames.RenderPriority = RenderPriority.Placenames;
				c_oWorldWindow.CurrentWorld.RenderableObjects.Add(this.placeNames);
				this.c_miShowPlaceNames.Enabled = true;
			}
			this.c_miShowPlaceNames.Checked = World.Settings.ShowPlacenames;
		}

		#endregion

		#region Open/Save View Methods

		private void SaveLastView()
		{
			// --- Don't save views when we're running inside OM ---
			if (IsMontajChildProcess) return;

			string tempFile = Path.ChangeExtension(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()), ".jpg");
			if (this.lastView.Length == 0)
				SaveCurrentView(Path.Combine(UserPath, Path.Combine(Settings.ConfigPath, LastView)), tempFile, string.Empty);
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
		private void SaveCurrentView(string fileName, string picFileName, string notes)
		{
			DappleView view = new DappleView();

			// blue marble
			RenderableObject roBMNG = GetBMNG();
			if (roBMNG != null)
				view.View.Addshowbluemarble(new SchemaBoolean(roBMNG.IsOn));

			WorldWind.Camera.MomentumCamera camera = c_oWorldWindow.DrawArgs.WorldCamera as WorldWind.Camera.MomentumCamera;

			//stop the camera
			camera.SetPosition(camera.Latitude.Degrees, camera.Longitude.Degrees, camera.Heading.Degrees, camera.Altitude, camera.Tilt.Degrees);

			//store the servers
			this.c_oServerTree.SaveToView(view);

			// store the current layers
			if (c_oLayerList.AllLayers.Count > 0)
			{
				activelayersType lyrs = view.View.Newactivelayers();
				foreach (LayerBuilder container in c_oLayerList.AllLayers)
				{
					if (!container.Temporary)
					{
						datasetType dataset = lyrs.Newdataset();
						dataset.Addname(new SchemaString(container.Title));
						opacityType op = dataset.Newopacity();
						op.Value = container.Opacity;
						dataset.Addopacity(op);
						dataset.Adduri(new SchemaString(container.GetURI()));
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

			using (Image img = TakeSnapshot(c_oWorldWindow.Handle))
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

		private static Image TakeSnapshot(IntPtr handle)
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

		private bool OpenView(string filename, bool bGoto, bool bLoadLayers)
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

					if (bGoto && view.View.Hascameraorientation())
					{
						cameraorientationType orient = view.View.cameraorientation;
						c_oWorldWindow.DrawArgs.WorldCamera.SlerpPercentage = World.Settings.CameraSlerpInertia;
						c_oWorldWindow.DrawArgs.WorldCamera.SetPosition(orient.lat.Value, orient.lon.Value, orient.heading.Value, orient.altitude.Value, orient.tilt.Value);
					}

					this.c_oServerTree.LoadFromView(view);
					if (bLoadLayers && view.View.Hasactivelayers())
					{
						bOldView = c_oLayerList.CmdLoadFromView(view, c_oServerTree);
					}
				}
			}
			catch (Exception e)
			{
				if (MessageBox.Show(this, "Error loading view from " + filename + "\n(" + e.Message + ")\nDo you want to open the Dapple default view?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.Yes)
				{
					return OpenView(Path.Combine(Settings.DataPath, DefaultView), true, true);
				}
			}

			if (bOldView)
				MessageBox.Show(this, "The view " + filename + " contained some layers from an earlier version\nwhich could not be retrieved. We apologize for the inconvenience.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
			return true;
		}

		#endregion

		#region Add Layers

		private void AddGeoTiff(string strGeoTiff, string strGeoTiffName, bool bTmp, bool bGoto)
		{
			LayerBuilder builder = new GeorefImageLayerBuilder(strGeoTiffName, strGeoTiff, bTmp, c_oWorldWindow, null);

			Cursor = Cursors.WaitCursor;
			if (builder.GetLayer() != null)
			{
				Cursor = Cursors.Default;

				// If the file is already there remove it 
				c_oLayerList.CmdRemoveGeoTiff(strGeoTiff);

				// If there is already a layer by that name find unique name
				if (strGeoTiffName.Length > 0)
				{
					int iCount = 0;
					string strNewName = strGeoTiffName;
					bool bExist = true;
					while (bExist)
					{
						bExist = false;
						foreach (LayerBuilder container in c_oLayerList.AllLayers)
						{
							if (container.Title == strNewName)
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

				if (bTmp) builder.Opacity = 128;
				else builder.Opacity = 255;
				builder.Visible = true;
				builder.Temporary = bTmp;

				c_oLayerList.AddLayer(builder);

				if (bGoto)
					GoTo(builder, false);
			}
			else
			{
				Cursor = Cursors.Default;
				string strMessage = "Error adding the file: '" + strGeoTiff + "'.\nOnly WGS 84 geographic images can be displayed at this time.";
				string strGeoInfo = GeorefImageLayerBuilder.GetGeorefInfoFromGeotif(strGeoTiff);
				if (strGeoInfo.Length > 0)
				{
					strMessage += "\nThis image is:\n\n" + strGeoInfo;
				}

				Program.ShowMessageBox(
					strMessage,
					"Open GeoTIFF Image",
					MessageBoxButtons.OK,
					MessageBoxDefaultButton.Button1,
					MessageBoxIcon.Error);
			}
		}

		public void AddLayerBuilder(LayerBuilder oLayer)
		{
			c_oLayerList.AddLayer(oLayer);

			SaveLastView();
		}

		#endregion

		#region Go To

		void GoTo(LayerBuilder builder)
		{
			GoTo(builder.Extents, false);
		}

		void GoTo(LayerBuilder builder, bool blImmediate)
		{
			GoTo(builder.Extents, blImmediate);
		}

		void GoTo(GeographicBoundingBox extents, bool blImmediate)
		{
			c_oWorldWindow.GotoBoundingbox(extents.West, extents.South, extents.East, extents.North, blImmediate);
		}

		#endregion

		#region Add Servers

		public void AddDAPServer()
		{
			AddDAP dlg = new AddDAP();
			if (dlg.ShowDialog(this) == DialogResult.OK)
			{
				Geosoft.GX.DAPGetData.Server oServer;
				try
				{
					if (this.c_oServerTree.AddDAPServer(dlg.Url, out oServer, true, true))
					{
						ThreadPool.QueueUserWorkItem(new WaitCallback(submitServerToSearchEngine), new Object[] { dlg.Url, "DAP" });
					}
				}
				catch (Exception except)
				{
					Program.ShowMessageBox(
						"Error adding server \"" + dlg.Url + "\":\n" + except.Message,
						"Add DAP Server",
						MessageBoxButtons.OK,
						MessageBoxDefaultButton.Button1,
						MessageBoxIcon.Error);
				}
				SaveLastView();
			}
		}

		public void AddWMSServer()
		{
			TreeNode treeNode = TreeUtils.FindNodeOfTypeBFS(typeof(WMSCatalogBuilder), this.c_oServerTree.Nodes);

			if (treeNode != null)
			{
				AddWMS dlg = new AddWMS(c_oWorldWindow, treeNode.Tag as WMSCatalogBuilder);
				if (dlg.ShowDialog(this) == DialogResult.OK)
				{
					try
					{
						if (this.c_oServerTree.AddWMSServer(dlg.WmsURL, true, true, true))
						{
							ThreadPool.QueueUserWorkItem(new WaitCallback(submitServerToSearchEngine), new Object[] { dlg.WmsURL, "WMS" });
						}
					}
					catch (Exception except)
					{
						Program.ShowMessageBox(
							"Error adding server \"" + dlg.WmsURL + "\":\n" + except.Message,
							"Add WMS Server",
							MessageBoxButtons.OK,
							MessageBoxDefaultButton.Button1,
							MessageBoxIcon.Error);
					}
					SaveLastView();
				}
			}
		}

		public void AddArcIMSServer()
		{
			TreeNode treeNode = TreeUtils.FindNodeOfTypeBFS(typeof(ArcIMSCatalogBuilder), this.c_oServerTree.Nodes);

			if (treeNode != null)
			{
				AddArcIMS dlg = new AddArcIMS(c_oWorldWindow, treeNode.Tag as ArcIMSCatalogBuilder);
				if (dlg.ShowDialog(this) == DialogResult.OK)
				{
					try
					{
						if (this.c_oServerTree.AddArcIMSServer(new ArcIMSServerUri(dlg.URL), true, true, true))
						{
							ThreadPool.QueueUserWorkItem(new WaitCallback(submitServerToSearchEngine), new Object[] { dlg.URL, "ArcIMS" });
						}
					}
					catch (Exception except)
					{
						Program.ShowMessageBox(
							"Error adding server \"" + dlg.URL + "\":\n" + except.Message,
							"Add ArcIMS Server",
							MessageBoxButtons.OK,
							MessageBoxDefaultButton.Button1,
							MessageBoxIcon.Error);
					}
					SaveLastView();
				}
			}
		}

		#endregion

		#region Updating the home view

		public enum UpdateHomeViewType
		{
			AddServer,
			RemoveServer,
			ToggleServer,
			ChangeFavorite
		};

		private Object LOCK = new object();
		/// <summary>
		/// This command modifies the home view, but doesn't do a full save.  Useful for changing the server list.
		/// </summary>
		/// <param name="eType"></param>
		/// <param name="szServer"></param>
		public void CmdUpdateHomeView(UpdateHomeViewType eType, Object[] data)
		{
			lock (LOCK)
			{
				XmlDocument oHomeViewDoc = new XmlDocument();
				oHomeViewDoc.Load(Path.Combine(Path.Combine(UserPath, Settings.ConfigPath), HomeView));

				switch (eType)
				{
					case UpdateHomeViewType.AddServer:
						{
							String szUrl = data[0] as String;
							String szType = data[1] as String;

							if (szType.Equals("DAP", StringComparison.InvariantCultureIgnoreCase))
							{
								XmlElement oDAPRoot = oHomeViewDoc.SelectSingleNode("/dappleview/servers/builderentry/builderdirectory[@specialcontainer=\"DAPServers\"]") as XmlElement;
								XmlElement oBuilderEntry = oHomeViewDoc.CreateElement("builderentry");
								oDAPRoot.AppendChild(oBuilderEntry);
								XmlElement oDapCatalog = oHomeViewDoc.CreateElement("dapcatalog");
								oDapCatalog.SetAttribute("url", szUrl);
								oDapCatalog.SetAttribute("enabled", "true");
								oBuilderEntry.AppendChild(oDapCatalog);
							}
							else if (szType.Equals("WMS", StringComparison.InvariantCultureIgnoreCase))
							{
								XmlElement oWMSRoot = oHomeViewDoc.SelectSingleNode("/dappleview/servers/builderentry/builderdirectory[@specialcontainer=\"WMSServers\"]") as XmlElement;
								XmlElement oBuilderEntry = oHomeViewDoc.CreateElement("builderentry");
								oWMSRoot.AppendChild(oBuilderEntry);
								XmlElement oDapCatalog = oHomeViewDoc.CreateElement("wmscatalog");
								oDapCatalog.SetAttribute("capabilitiesurl", szUrl);
								oDapCatalog.SetAttribute("enabled", "true");
								oBuilderEntry.AppendChild(oDapCatalog);
							}
							else if (szType.Equals("ArcIMS", StringComparison.InvariantCultureIgnoreCase))
							{
								XmlElement oWMSRoot = oHomeViewDoc.SelectSingleNode("/dappleview/servers/builderentry/builderdirectory[@specialcontainer=\"WMSServers\"]") as XmlElement;
								XmlElement oBuilderEntry = oHomeViewDoc.CreateElement("builderentry");
								oWMSRoot.AppendChild(oBuilderEntry);
								XmlElement oDapCatalog = oHomeViewDoc.CreateElement("arcimscatalog");
								oDapCatalog.SetAttribute("capabilitiesurl", szUrl);
								oDapCatalog.SetAttribute("enabled", "true");
								oBuilderEntry.AppendChild(oDapCatalog);
							}
						}
						break;
					case UpdateHomeViewType.ChangeFavorite:
						{
							String szUrl = data[0] as String;

							XmlElement oDocRoot = oHomeViewDoc.SelectSingleNode("/dappleview") as XmlElement;
							oDocRoot.SetAttribute("favouriteserverurl", szUrl);
						}
						break;
					case UpdateHomeViewType.RemoveServer:
						{
							String szUrl = data[0] as String;
							String szType = data[1] as String;

							if (szType.Equals("DAP", StringComparison.InvariantCultureIgnoreCase))
							{
								foreach (XmlElement oDapCatalog in oHomeViewDoc.SelectNodes("/dappleview/servers/builderentry/builderdirectory/builderentry/dapcatalog"))
								{
									if (oDapCatalog.GetAttribute("url").Equals(szUrl))
									{
										oDapCatalog.ParentNode.ParentNode.RemoveChild(oDapCatalog.ParentNode);
									}
								}
							}
							else if (szType.Equals("WMS", StringComparison.InvariantCultureIgnoreCase))
							{
								foreach (XmlElement oDapCatalog in oHomeViewDoc.SelectNodes("/dappleview/servers/builderentry/builderdirectory/builderentry/wmscatalog"))
								{
									if (oDapCatalog.GetAttribute("capabilitiesurl").Equals(szUrl))
									{
										oDapCatalog.ParentNode.ParentNode.RemoveChild(oDapCatalog.ParentNode);
									}
								}
							}
							else if (szType.Equals("ArcIMS", StringComparison.InvariantCultureIgnoreCase))
							{
								foreach (XmlElement oDapCatalog in oHomeViewDoc.SelectNodes("/dappleview/servers/builderentry/builderdirectory/builderentry/arcimscatalog"))
								{
									if (oDapCatalog.GetAttribute("capabilitiesurl").Equals(szUrl))
									{
										oDapCatalog.ParentNode.ParentNode.RemoveChild(oDapCatalog.ParentNode);
									}
								}
							}
						}
						break;
					case UpdateHomeViewType.ToggleServer:
						{
							String szUrl = data[0] as String;
							String szType = data[1] as String;
							bool blStatus = (bool)data[2];

							if (szType.Equals("DAP", StringComparison.InvariantCultureIgnoreCase))
							{
								foreach (XmlElement oDapCatalog in oHomeViewDoc.SelectNodes("/dappleview/servers/builderentry/builderdirectory/builderentry/dapcatalog"))
								{
									if (oDapCatalog.GetAttribute("url").Equals(szUrl))
									{
										oDapCatalog.SetAttribute("enabled", blStatus.ToString());
									}
								}
							}
							else if (szType.Equals("WMS", StringComparison.InvariantCultureIgnoreCase))
							{
								foreach (XmlElement oDapCatalog in oHomeViewDoc.SelectNodes("/dappleview/servers/builderentry/builderdirectory/builderentry/wmscatalog"))
								{
									if (oDapCatalog.GetAttribute("capabilitiesurl").Equals(szUrl))
									{
										oDapCatalog.SetAttribute("enabled", blStatus.ToString());
									}
								}
							}
							else if (szType.Equals("ArcIMS", StringComparison.InvariantCultureIgnoreCase))
							{
								foreach (XmlElement oDapCatalog in oHomeViewDoc.SelectNodes("/dappleview/servers/builderentry/builderdirectory/builderentry/arcimscatalog"))
								{
									if (oDapCatalog.GetAttribute("capabilitiesurl").Equals(szUrl))
									{
										oDapCatalog.SetAttribute("enabled", blStatus.ToString());
									}
								}
							}
						}
						break;
				}

				oHomeViewDoc.Save(Path.Combine(Path.Combine(UserPath, Settings.ConfigPath), HomeView));
			}
		}

		#endregion

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

		private void AddDatasetAction()
		{
			if (c_tcSearchViews.SelectedIndex == 0)
			{
				if (cServerViewsTab.SelectedIndex == 0)
				{
					c_oServerTree.AddCurrentDataset();
				}
				else if (cServerViewsTab.SelectedIndex == 1)
				{
					c_oLayerList.AddLayers(c_oServerList.SelectedLayers);
				}
			}
			else if (c_tcSearchViews.SelectedIndex == 1)
			{
				c_oDappleSearch.CmdAddSelected();
			}
		}

		private void submitServerToSearchEngine(object param)
		{
			if (!Settings.UseDappleSearch) return;

			try
			{
				XmlDocument query = new XmlDocument();
				XmlElement geoRoot = query.CreateElement("geosoft_xml");
				query.AppendChild(geoRoot);
				XmlElement root = query.CreateElement("add_server");
				root.SetAttribute("url", ((Object[])param)[0] as String);
				root.SetAttribute("type", ((Object[])param)[1] as String);
				geoRoot.AppendChild(root);

				// --- This non-WebDownload download is permitted because this method is called from a threadpool thread ---
				HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Settings.DappleSearchURL + NEW_SERVER_GATEWAY);
				request.Headers["GeosoftAddServerRequest"] = query.InnerXml;
				WebResponse response = request.GetResponse();
				response.Close();
			}
			catch
			{
				// Never crash, just silently fail to send the data to the server.
			}
		}

		public void CmdSaveHomeView()
		{
			SaveCurrentView(Path.Combine(Path.Combine(UserPath, Settings.ConfigPath), HomeView), Path.ChangeExtension(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()), ".jpg"), String.Empty);
		}

		public void CmdLoadHomeView()
		{
			OpenView(Path.Combine(Path.Combine(UserPath, Settings.ConfigPath), HomeView), true, true);
		}

		private void CmdDisplayOMHelp()
		{
			try
			{
				MontajInterface.DisplayHelp();
			}
			catch (System.Runtime.Remoting.RemotingException)
			{
				Program.ShowMessageBox(
					"Connection to " + Utility.EnumUtils.GetDescription(MainForm.Client) + " lost, unable to display help.",
					"Extract Layers",
					MessageBoxButtons.OK,
					MessageBoxDefaultButton.Button1,
					MessageBoxIcon.Error);
				c_miGetDatahelp.Enabled = false;
			}
		}

		private void SaveMRUList()
		{
			StreamWriter oOutput = null;
			try
			{
				oOutput = new StreamWriter(Path.Combine(CurrentSettingsDirectory, "MRU.txt"), false);

				foreach (String szMRU in c_tbSearchKeywords.Items)
				{
					oOutput.WriteLine(szMRU);
				}
			}
			catch (IOException)
			{
				// Do nothing, if a minor bug borks the MRU list, it's not the end of the world.
			}
			finally
			{
				if (oOutput != null) oOutput.Close();
			}
		}

		private void LoadMRUList()
		{
			if (!File.Exists(Path.Combine(CurrentSettingsDirectory, "MRU.txt"))) return;

			try
			{
				String[] aMRUs = File.ReadAllLines(Path.Combine(CurrentSettingsDirectory, "MRU.txt"));

				for (int count = 0; count < aMRUs.Length && count < MAX_MRU_TERMS; count++)
				{
					c_tbSearchKeywords.Items.Add(aMRUs[count]);
				}
			}
			catch (IOException)
			{
				// Do nothing, if we can't read the MRU list, it's not the end of the world.
			}
		}

		private void SetSearchable(bool blValue)
		{
			c_miSearch.Enabled = blValue;
			c_bSearch.Enabled = blValue;
		}

		private void SetSearchClearable(bool blValue)
		{
			c_bClearSearch.Enabled = blValue;
		}

		private void CenterNavigationToolStrip()
		{
			Point newLocation = new Point((c_tsNavigation.Parent.Width - c_tsNavigation.Width) / 2, c_tsNavigation.Parent.Height - c_tsNavigation.Height);
			c_tsNavigation.Location = newLocation;
		}

		/// <summary>
		/// Do a search programmatically.
		/// </summary>
		/// <remarks>
		/// Doesn't update MRU list or supress repeated searches.
		/// </remarks>
		/// <param name="szKeywords"></param>
		/// <param name="oAoI"></param>
		private void doSearch(String szKeywords, GeographicBoundingBox oAoI)
		{
			SetSearchable(false);
			SetSearchClearable(true);

			m_oLastSearchROI = oAoI;
			m_szLastSearchString = szKeywords;

			applySearchCriteria();
		}

		private void doSearch()
		{
			// --- Cancel if the search parameters are unchanged ---
			GeographicBoundingBox oCurrSearchROI = GeographicBoundingBox.FromQuad(c_oWorldWindow.GetSearchBox());
			String szCurrSearchString = SearchKeyword;
			if (oCurrSearchROI.Equals(m_oLastSearchROI) && szCurrSearchString.Equals(m_szLastSearchString)) return;

			// --- Reorder the MRU list.  Supress index changed is important because removing the current MRU will raise the event again ---
			m_blSupressSearchSelectedIndexChanged = true;
			c_tbSearchKeywords.SuspendLayout();
			if (!SearchKeyword.Equals(String.Empty))
			{
				c_tbSearchKeywords.Items.Remove(szCurrSearchString);

				while (c_tbSearchKeywords.Items.Count >= MAX_MRU_TERMS)
				{
					c_tbSearchKeywords.Items.RemoveAt(c_tbSearchKeywords.Items.Count - 1);
				}
				c_tbSearchKeywords.Items.Insert(0, szCurrSearchString);
				c_tbSearchKeywords.Text = szCurrSearchString;
			}
			c_tbSearchKeywords.ResumeLayout();
			m_blSupressSearchSelectedIndexChanged = false;

			// --- Mop up and move out ---

			SetSearchable(false);
			SetSearchClearable(true);

			m_oLastSearchROI = GeographicBoundingBox.FromQuad(c_oWorldWindow.GetSearchBox());
			m_szLastSearchString = SearchKeyword;

			applySearchCriteria();
		}

		private void clearSearch()
		{
			SetSearchable(true);
			SetSearchClearable(false);

			m_oLastSearchROI = null;
			m_szLastSearchString = String.Empty;

			c_tbSearchKeywords.Text = NO_SEARCH;
			c_tbSearchKeywords.ForeColor = SystemColors.GrayText;

			applySearchCriteria();
		}

		private void applySearchCriteria()
		{
			this.UseWaitCursor = true;
			Application.DoEvents();
			this.c_oServerTree.Search(m_oLastSearchROI, m_szLastSearchString);
			this.c_oServerList.setSearchCriteria(m_szLastSearchString, m_oLastSearchROI);
			this.c_oDappleSearch.SetSearchParameters(m_szLastSearchString, m_oLastSearchROI);
			this.UseWaitCursor = false;
			Application.DoEvents();
		}

		private void CmdSetupServersMenu()
		{
			bool blServerSelected = c_oServerTree.SelectedNode != null &&
						(c_oServerTree.SelectedNode.Tag is Server || c_oServerTree.SelectedNode.Tag is ServerBuilder);
			blServerSelected &= c_tcSearchViews.SelectedIndex == 0;
			blServerSelected &= cServerViewsTab.SelectedIndex == 0;

			bool blDAPServerSelected = blServerSelected && c_oServerTree.SelectedNode.Tag is Server;

			c_miViewProperties.Enabled = blServerSelected;
			c_miRefreshServer.Enabled = blServerSelected;
			c_miRemoveServer.Enabled = blServerSelected;
			c_miSetFavouriteServer.Enabled = blServerSelected && !c_oServerTree.SelectedIsFavorite;
			c_miAddBrowserMap.Enabled = blDAPServerSelected;

			if (blServerSelected == false)
			{
				c_miToggleServerStatus.Text = "Disable";
				c_miToggleServerStatus.Image = Resources.disserver;
				c_miToggleServerStatus.Enabled = false;
			}
			else
			{
				bool blServerEnabled = true;
				if (c_oServerTree.SelectedNode.Tag is Server)
					blServerEnabled = ((Server)c_oServerTree.SelectedNode.Tag).Enabled;
				else if (c_oServerTree.SelectedNode.Tag is ServerBuilder)
					blServerEnabled = ((ServerBuilder)c_oServerTree.SelectedNode.Tag).Enabled;

				if (blServerEnabled)
				{
					c_miToggleServerStatus.Text = "Disable";
					c_miToggleServerStatus.Image = Resources.disserver;
				}
				else
				{
					c_miToggleServerStatus.Text = "Enable";
					c_miToggleServerStatus.Image = Resources.enserver;
				}
				c_miToggleServerStatus.Enabled = true;
			}
		}

		private void CmdSetupToolsMenu()
		{
			if (c_tcSearchViews.SelectedIndex == 0)
			{
				if (cServerViewsTab.SelectedIndex == 0)
				{
					// --- Active is server tree ---
					c_miAddLayer.Enabled = c_oServerTree.SelectedNode != null && (c_oServerTree.SelectedNode.Tag is LayerBuilder || c_oServerTree.SelectedNode.Tag is Geosoft.Dap.Common.DataSet);
				}
				else if (cServerViewsTab.SelectedIndex == 1)
				{
					// --- Active is server list ---
					c_miAddLayer.Enabled = c_oServerList.SelectedLayers.Count > 0;
				} 
			}
			else if (c_tcSearchViews.SelectedIndex == 1)
			{
				// --- Active is dapple search list ---
				c_miAddLayer.Enabled = c_oDappleSearch.HasLayersSelected;
			}
		}

		#endregion
	}
}

#region Temporary KML Code
/*
		private void cKMLTree_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				GeographicBoundingBox extents = null;
				TreeNode node = cKMLTree.HitTest(e.Location).Node;

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
					double dNorth = double.MinValue;
					double dSouth = double.MaxValue;
					double dEast = double.MinValue;
					double dWest = double.MaxValue;
					foreach (Point3d p in line.Points)
					{
						dNorth = Math.Max(p.Y, dNorth);
						dSouth = Math.Min(p.Y, dSouth);
						dEast = Math.Max(p.X, dEast);
						dWest = Math.Min(p.X, dWest);
					}
					extents = new GeographicBoundingBox(dNorth, dSouth, dWest, dEast);
				}
				else if (node.Tag is PolygonFeature)
				{
					Point3d pSph;
					PolygonFeature pFeat = node.Tag as PolygonFeature;
					double dNorth = double.MinValue;
					double dSouth = double.MaxValue;
					double dEast = double.MinValue;
					double dWest = double.MaxValue;
					foreach (Point3d p in pFeat.BoundingBox.corners)
					{
						pSph = MathEngine.CartesianToSpherical(p.X, p.Y, p.Z);
						pSph.Y = MathEngine.RadiansToDegrees(pSph.Y);
						pSph.Z = MathEngine.RadiansToDegrees(pSph.Z);
						dNorth = Math.Max(pSph.Y, dNorth);
						dSouth = Math.Min(pSph.Y, dSouth);
						dEast = Math.Max(pSph.Z, dEast);
						dWest = Math.Min(pSph.Z, dWest);
					}
					extents = new GeographicBoundingBox(dNorth, dSouth, dWest, dEast);
				}

				if (extents != null)
					GoTo(extents, false);
			}
		}

		void UpdateKMLNodes(TreeNode parentNode, RenderableObjectList objectList)
		{
			TreeNode node;
			int iImage = ImageListIndex("kml");
			foreach (RenderableObject ro in objectList.ChildObjects)
			{
				node = parentNode.Nodes.Add(null, ro.Name, iImage, iImage);
				node.Checked = ro.IsOn;
				//node = this.tvLayers.Add(parentNode, ro.Name, iImage, iImage, this.kmlPlugin.KMLIcons.IsOn ? TriStateTreeView.CheckBoxState.Checked : TriStateTreeView.CheckBoxState.Unchecked);
				node.Tag = ro;
				if (ro is RenderableObjectList)
					UpdateKMLNodes(node, ro as RenderableObjectList);
			}
		}

		void UpdateKMLIcons()
		{
			cKMLTree.BeginUpdate();
			//this.tvLayers.BeginUpdate();

			if (cKMLTree.Nodes.Count > 0)
				cKMLTree.Nodes.Clear();
			//if (this.kmlNode != null)
			//	this.tvLayers.Nodes.Remove(this.kmlNode);

			if (this.kmlPlugin.KMLIcons.ChildObjects.Count > 0)
			{
				int iImage = ImageListIndex("kml");
				TreeNode oRootNode = cKMLTree.Nodes.Add(null, kmlPlugin.KMLIcons.Name, iImage, iImage);
				//this.kmlNode = this.tvLayers.AddTop(null, this.kmlPlugin.KMLIcons.Name, iImage, iImage, this.kmlPlugin.KMLIcons.IsOn ? TriStateTreeView.CheckBoxState.Checked : TriStateTreeView.CheckBoxState.Unchecked);
				oRootNode.Checked = kmlPlugin.KMLIcons.IsOn; // new
				oRootNode.Tag = this.kmlPlugin.KMLIcons;
				//this.kmlNode.Tag = this.kmlPlugin.KMLIcons;
				UpdateKMLNodes(oRootNode, this.kmlPlugin.KMLIcons);
			}

			cKMLTree.EndUpdate();
			//this.tvLayers.EndUpdate();
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
				if (cKMLTree.Nodes.Count > 0)
					cKMLTree.Nodes.Clear();
				//if (this.kmlNode != null)
				//	this.tvLayers.Nodes.Remove(this.kmlNode);
				int iImage = ImageListIndex("kml");
				cKMLTree.Nodes.Add(null, "Please wait, loading KML file...", iImage, iImage);
				//this.kmlNode = this.tvLayers.AddTop(null, "Please wait, loading KML file...", iImage, iImage, TriStateTreeView.CheckBoxState.None);
				this.kmlPlugin.LoadDiskKM(fileDialog.FileName, new MethodInvoker(UpdateKMLIcons));
			}
		}

		private void cKMLTree_AfterCheck(object sender, TreeViewEventArgs e)
		{
			if (e.Node.Tag != null) ((RenderableObject)e.Node.Tag).IsOn = e.Node.Checked;
			foreach (TreeNode oChildNode in e.Node.Nodes)
			{
				oChildNode.Checked = e.Node.Checked;
			}
		}
		*/
#endregion