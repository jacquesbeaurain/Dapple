using Microsoft.DirectX.Direct3D;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Xml.Serialization;
using WorldWind.Configuration;

namespace WorldWind
{

   internal enum MeasureMode
   {
      Single,
      Multi
   }

   /// <summary>
   /// World user configurable settings
   /// TODO: Group settings
   /// </summary>
	public class WorldSettings : SettingsBase
   {
      #region Atmosphere
      internal bool enableAtmosphericScattering = false;

      [Browsable(true), Category("Atmosphere")]
      [Description("Enable Atmospheric Scattering")]
		public bool EnableAtmosphericScattering
      {
         get { return enableAtmosphericScattering; }
			set { enableAtmosphericScattering = value; }
      }

      internal bool forceCpuAtmosphere = true;
      [Browsable(true), Category("Atmosphere")]
      [Description("Forces CPU calculation instead of GPU for Atmospheric Scattering")]
      internal bool ForceCpuAtmosphere
      {
         get { return forceCpuAtmosphere; }
			private set { forceCpuAtmosphere = value; }
      }

      #endregion

      #region UI

      /// <summary>
      /// Show the top tool button bar
      /// </summary>
      internal bool showToolbar = true;

      /// <summary>
      /// Display the layer manager window
      /// </summary>
      internal bool showLayerManager = false;

      /// <summary>
      /// Display cross-hair symbol on screen
      /// </summary>
      internal bool showCrosshairs = false;
      internal int crosshairColor = Color.Beige.ToArgb();
      internal int crosshairSize = 10;

      /// <summary>
      /// Font name for the default font used in UI
      /// </summary>
      internal string defaultFontName = "Tahoma";

      /// <summary>
      /// Font size (em) for the default font used in UI
      /// </summary>
      internal float defaultFontSize = 9.0f;

      /// <summary>
      /// Font style for the default font used in UI
      /// </summary>
      internal FontStyle defaultFontStyle = FontStyle.Regular;

      /// <summary>
      /// Font name used in the toolbar 
      /// </summary>
      internal string toolbarFontName = "Tahoma";

      /// <summary>
      /// Font size (em) for the font used in UI
      /// </summary>
      internal float toolbarFontSize = 8;

      /// <summary>
      /// Font style for the font used in UI
      /// </summary>
      internal FontStyle toolbarFontStyle = FontStyle.Bold;

      /// <summary>
      /// Menu bar background color
      /// </summary>
      internal int menuBarBackgroundColor = Color.FromArgb(128, 128, 128, 128).ToArgb();

      /// <summary>
      /// Font name used in the layer manager 
      /// </summary>
      internal string layerManagerFontName = "Tahoma";

      /// <summary>
      /// Font size (em) for the font used in UI
      /// </summary>
      internal float layerManagerFontSize = 9;

      /// <summary>
      /// Font style for the font used in layer manager
      /// </summary>
      internal FontStyle layerManagerFontStyle = FontStyle.Regular;

      /// <summary>
      /// Layer manager width (pixels)
      /// </summary>
      internal int layerManagerWidth = 200;

      /// <summary>
      /// Draw anti-aliased text
      /// </summary>
      internal bool antiAliasedText = false;

      /// <summary>
      /// Maximum frames-per-second setting
      /// </summary>
      internal int throttleFpsHz = 50;

      /// <summary>
      /// Vsync on/off (Wait for vertical retrace)
      /// </summary>
      internal bool vSync = true;

      /// <summary>
      /// Rapid Fire MODIS icon size
      /// </summary>
      internal int modisIconSize = 60;

      internal int m_FpsFrameCount = 300;
      internal bool m_ShowFpsGraph = false;

      internal int downloadTerrainRectangleColor = Color.FromArgb(50, 0, 0, 255).ToArgb();
      internal int downloadProgressColor = Color.FromArgb(50, 255, 0, 0).ToArgb();
      internal int downloadLogoColor = Color.FromArgb(180, 255, 255, 255).ToArgb();
      internal int menuBackColor = Color.FromArgb(170, 40, 40, 40).ToArgb();
      internal int menuOutlineColor = Color.FromArgb(150, 160, 160, 160).ToArgb();
      internal int widgetBackgroundColor = Color.FromArgb(0, 0, 0, 255).ToArgb();
      internal int scrollbarColor = System.Drawing.Color.FromArgb(170, 100, 100, 100).ToArgb();
      internal int scrollbarHotColor = System.Drawing.Color.FromArgb(170, 255, 255, 255).ToArgb();
      internal int toolBarBackColor = System.Drawing.Color.FromArgb(100, 255, 255, 255).ToArgb();
      internal bool showDownloadIndicator = true;
      internal bool outlineText = false;
      internal bool showCompass = false;
      internal WFSNameColors nameColors = WFSNameColors.Default;
      internal float nameSizeMultiplier = 1.0f;

      internal bool browserVisible = false;
      internal bool browserOrientationHorizontal = false;
      internal int browserSize = 300;
      internal bool useInternalBrowser = true;
      internal bool useOfflineSearch = false;

      [Browsable(true), Category("UI")]
      [Description("Show Compass Indicator.")]
		public bool ShowCompass
      {
         get { return showCompass; }
			set { showCompass = value; }
      }


      [Browsable(true), Category("UI")]
      [Description("Draw outline around WFS text to improve visibility.")]
      internal bool WFSOutlineText
      {
         get { return outlineText; }
			private set { outlineText = value; }
      }

      [Browsable(true), Category("UI")]
      [Description("Change name colors for visibility.")]
      internal WFSNameColors WFSNameColors
      {
         get { return nameColors; }
			private set { nameColors = value; }
      }

      [Browsable(true), Category("UI")]
      [Description("Factor by which default text size will be multiplied")]
      internal float WFSNameSizeMultiplier
      {
         get { return nameSizeMultiplier; }
			private set
         {
            if (value < 0.1f || value > 10f)
               throw new ArgumentException("WFSNameSize out of range: " + value);
            else
               nameSizeMultiplier = value;
         }
      }

      [Browsable(true), Category("UI")]
      [Description("Display download progress and rectangles.")]
		public bool ShowDownloadIndicator
      {
         get { return showDownloadIndicator; }
			set { showDownloadIndicator = value; }
      }

      [XmlIgnore]
      [Browsable(true), Category("UI")]
      [Editor(typeof(ColorEditor), typeof(UITypeEditor))]
      [Description("Toolbar Background color.")]
      internal Color ToolBarBackColor
      {
         get { return Color.FromArgb(toolBarBackColor); }
			private set { toolBarBackColor = value.ToArgb(); }
      }

      [XmlIgnore]
      [Browsable(true), Category("UI")]
      [Editor(typeof(ColorEditor), typeof(UITypeEditor))]
      [Description("Color of scrollbar when scrolling.")]
      internal Color ScrollbarHotColor
      {
         get { return Color.FromArgb(scrollbarHotColor); }
			private set { scrollbarHotColor = value.ToArgb(); }
      }

      [XmlIgnore]
      [Browsable(true), Category("UI")]
      [Editor(typeof(ColorEditor), typeof(UITypeEditor))]
      [Description("Color of scrollbar.")]
      internal Color ScrollbarColor
      {
         get { return Color.FromArgb(scrollbarColor); }
			private set { scrollbarColor = value.ToArgb(); }
      }

      [XmlIgnore]
      [Browsable(true), Category("UI")]
      [Editor(typeof(ColorEditor), typeof(UITypeEditor))]
      [Description("Menu border color.")]
      internal Color MenuOutlineColor
      {
         get { return Color.FromArgb(menuOutlineColor); }
			private set { menuOutlineColor = value.ToArgb(); }
      }

      [XmlIgnore]
      [Browsable(true), Category("UI")]
      [Editor(typeof(ColorEditor), typeof(UITypeEditor))]
      [Description("Widget background color.")]
		public Color WidgetBackgroundColor
      {
         get { return Color.FromArgb(widgetBackgroundColor); }
			private set { widgetBackgroundColor = value.ToArgb(); }
      }

      [XmlIgnore]
      [Browsable(true), Category("UI")]
      [Editor(typeof(ColorEditor), typeof(UITypeEditor))]
      [Description("Background color of the menu.")]
      internal Color MenuBackColor
      {
         get { return Color.FromArgb(menuBackColor); }
			private set { menuBackColor = value.ToArgb(); }
      }

      [XmlIgnore]
      [Browsable(true), Category("UI")]
      [Editor(typeof(ColorEditor), typeof(UITypeEditor))]
      [Description("Color/transparency of the download progress icon.")]
      internal Color DownloadLogoColor
      {
         get { return Color.FromArgb(downloadLogoColor); }
			private set { downloadLogoColor = value.ToArgb(); }
      }

      [XmlIgnore]
      [Browsable(true), Category("UI")]
      [Editor(typeof(ColorEditor), typeof(UITypeEditor))]
      [Description("Color of the download progress bar.")]
		public Color DownloadProgressColor
      {
         get { return Color.FromArgb(downloadProgressColor); }
			private set { downloadProgressColor = value.ToArgb(); }
      }

      [XmlIgnore]
      [Browsable(true), Category("UI")]
      [Editor(typeof(ColorEditor), typeof(UITypeEditor))]
      [Description("Color of the terrain download in progress rectangle.")]
      internal Color DownloadTerrainRectangleColor
      {
         get { return Color.FromArgb(downloadTerrainRectangleColor); }
			private set { downloadTerrainRectangleColor = value.ToArgb(); }
      }

      [Browsable(true), Category("UI")]
      [Description("Show the top tool button bar.")]
		public bool ShowToolbar
      {
         get { return showToolbar; }
			private set { showToolbar = value; }
      }

      [Browsable(true), Category("UI")]
      [Description("Display the layer manager window.")]
      internal bool ShowLayerManager
      {
         get { return showLayerManager; }
			private set { showLayerManager = value; }
      }

      [Browsable(true), Category("UI")]
      [Description("Display cross-hair symbol on screen.")]
		public bool ShowCrosshairs
      {
         get { return showCrosshairs; }
			set { showCrosshairs = value; }
      }

      [XmlIgnore]
      [Browsable(true), Category("UI")]
      [Editor(typeof(ColorEditor), typeof(UITypeEditor))]
      [Description("Cross-hair symbol color.")]
		public Color CrosshairColor
      {
         get { return Color.FromArgb(crosshairColor); }
			private set { crosshairColor = value.ToArgb(); }
      }

      [Browsable(true), Category("UI")]
      [Description("Size of cross-hair.")]
		public int CrosshairSize
      {
         get { return crosshairSize; }
			private set { crosshairSize = value; }
      }

      [Browsable(true), Category("UI")]
      [Description("Font name for the default font used in UI.")]
      internal string DefaultFontName
      {
         get { return defaultFontName; }
			private set { defaultFontName = value; }
      }

      [Browsable(true), Category("UI")]
      [Description("Font size for the default font used in UI.")]
      internal float DefaultFontSize
      {
         get { return defaultFontSize; }
			private set { defaultFontSize = value; }
      }

      [Browsable(true), Category("UI")]
      [Description("Font style for the default font used in UI.")]
      internal FontStyle DefaultFontStyle
      {
         get { return defaultFontStyle; }
			private set { defaultFontStyle = value; }
      }

      [Browsable(true), Category("UI")]
      [Description("Font name for the toolbar font used in UI.")]
      internal string ToolbarFontName
      {
         get { return toolbarFontName; }
			private set { toolbarFontName = value; }
      }

      [Browsable(true), Category("UI")]
      [Description("Font size (em) for the toolbar font used in UI.")]
      internal float ToolbarFontSize
      {
         get { return toolbarFontSize; }
			private set { toolbarFontSize = value; }
      }

      [Browsable(true), Category("UI")]
      [Description("Font style for the toolbar font used in UI.")]
      internal FontStyle ToolbarFontStyle
      {
         get { return toolbarFontStyle; }
			private set { toolbarFontStyle = value; }
      }

      [XmlIgnore]
      [Browsable(true), Category("UI")]
      [Editor(typeof(ColorEditor), typeof(UITypeEditor))]
      [Description("Menu bar background color.")]
      internal Color MenuBarBackgroundColor
      {
         get { return Color.FromArgb(menuBarBackgroundColor); }
			private set { menuBarBackgroundColor = value.ToArgb(); }
      }

      [Browsable(true), Category("UI")]
      [Description("Font name for the layer manager font.")]
      internal string LayerManagerFontName
      {
         get { return layerManagerFontName; }
			private set { layerManagerFontName = value; }
      }

      [Browsable(true), Category("UI")]
      [Description("Font size for the layer manager font.")]
      internal float LayerManagerFontSize
      {
         get { return layerManagerFontSize; }
			private set { layerManagerFontSize = value; }
      }

      [Browsable(true), Category("UI")]
      [Description("Font style for the layer manager font used in UI.")]
      internal FontStyle LayerManagerFontStyle
      {
         get { return layerManagerFontStyle; }
			private set { layerManagerFontStyle = value; }
      }

      [Browsable(true), Category("UI")]
      [Description("Layer manager width (pixels)")]
      internal int LayerManagerWidth
      {
         get { return layerManagerWidth; }
			private set { layerManagerWidth = value; }
      }

      /// <summary>
      /// Draw anti-aliased text
      /// </summary>
      [Browsable(true), Category("UI")]
      [Description("Enable anti-aliased text rendering. Change active only after program restart.")]
      internal bool AntiAliasedText
      {
         get { return antiAliasedText; }
			private set { antiAliasedText = value; }
      }

      [Browsable(true), Category("UI")]
      [Description("Maximum frames-per-second setting. Optionally throttles the frame rate (to get consistent frame rates or reduce CPU usage. 0 = Disabled")]
      internal int ThrottleFpsHz
      {
         get { return throttleFpsHz; }
			private set { throttleFpsHz = value; }
      }

      [Browsable(true), Category("UI")]
      [Description("Synchronize render buffer swaps with the monitor's refresh rate (vertical retrace). Change active only after program restart.")]
		public bool VSync
      {
         get { return vSync; }
			private set { vSync = value; }
      }

      [Browsable(true), Category("UI")]
      [Description("Changes the size of the Rapid Fire Modis icons.")]
      internal int ModisIconSize
      {
         get { return modisIconSize; }
			private set { modisIconSize = value; }
      }

      [Browsable(true), Category("UI")]
      [Description("Enables the Frames Per Second Graph")]
		public bool ShowFpsGraph
      {
         get { return m_ShowFpsGraph; }
			private set { m_ShowFpsGraph = value; }
      }

      [Browsable(true), Category("UI")]
      [Description("Changes length of the Fps Graph History")]
		public int FpsFrameCount
      {
         get { return m_FpsFrameCount; }
			private set { m_FpsFrameCount = value; }
      }

      [Browsable(true), Category("UI")]
      [Description("Initial visiblity of browser.")]
      internal bool BrowserVisible
      {
         get { return browserVisible; }
			private set { browserVisible = value; }
      }

      [Browsable(true), Category("UI")]
      [Description("Browser orientation.")]
      internal bool BrowserOrientationHorizontal
      {
         get { return browserOrientationHorizontal; }
			private set { browserOrientationHorizontal = value; }
      }

      [Browsable(true), Category("UI")]
      [Description("Size of browser panel.")]
      internal int BrowserSize
      {
         get { return browserSize; }
			private set { browserSize = value; }
      }

      [Browsable(true), Category("UI")]
      [Description("Use Internal Browser?")]
      internal bool UseInternalBrowser
      {
         get { return useInternalBrowser; }
			private set { useInternalBrowser = value; }
      }

      [Browsable(true), Category("UI")]
      [Description("Use Offline Placename Search?")]
      internal bool UseOfflineSearch
      {
         get { return useOfflineSearch; }
			private set { useOfflineSearch = value; }
      }



      #endregion

      #region Grid

      /// <summary>
      /// Display the latitude/longitude grid
      /// </summary>
      internal bool showLatLonLines = false;

      /// <summary>
      /// The color of the latitude/longitude grid
      /// </summary>
      internal int latLonLinesColor = System.Drawing.Color.FromArgb(200, 160, 160, 160).ToArgb();

      /// <summary>
      /// The color of the equator latitude line
      /// </summary>
      internal int equatorLineColor = System.Drawing.Color.FromArgb(160, 64, 224, 208).ToArgb();

      /// <summary>
      /// Display the tropic of capricorn/cancer lines
      /// </summary>
      internal bool showTropicLines = true;

      /// <summary>
      /// The color of the latitude/longitude grid
      /// </summary>
      internal int tropicLinesColor = System.Drawing.Color.FromArgb(160, 176, 224, 230).ToArgb();

      [Browsable(true), Category("Grid Lines")]
      [Description("Display the latitude/longitude grid.")]
		public bool ShowLatLonLines
      {
         get { return showLatLonLines; }
			set { showLatLonLines = value; }
      }

      [XmlIgnore]
      [Browsable(true), Category("Grid Lines")]
      [Editor(typeof(ColorEditor), typeof(UITypeEditor))]
      [Description("The color of the latitude/longitude grid.")]
      internal Color LatLonLinesColor
      {
         get { return Color.FromArgb(latLonLinesColor); }
			private set { latLonLinesColor = value.ToArgb(); }
      }

      [XmlIgnore]
      [Browsable(true), Category("Grid Lines")]
      [Editor(typeof(ColorEditor), typeof(UITypeEditor))]
      [Description("The color of the equator latitude line.")]
      internal Color EquatorLineColor
      {
         get { return Color.FromArgb(equatorLineColor); }
			private set { equatorLineColor = value.ToArgb(); }
      }

      [Browsable(true), Category("Grid Lines")]
      [Description("Display the tropic latitude lines.")]
      internal bool ShowTropicLines
      {
         get { return showTropicLines; }
			private set { showTropicLines = value; }
      }

      [XmlIgnore]
      [Browsable(true), Category("Grid Lines")]
      [Description("The color of the latitude/longitude grid.")]
      [Editor(typeof(ColorEditor), typeof(UITypeEditor))]
      internal Color TropicLinesColor
      {
         get { return Color.FromArgb(tropicLinesColor); }
			private set { tropicLinesColor = value.ToArgb(); }
      }

      #endregion

      #region World

      /// <summary>
      /// Index of blue marble version to show
      /// </summary>
      internal int bmngVersion = 1;

      /// <summary>
      /// Whether to display the planet axis line (through poles)
      /// </summary>
      internal bool showPlanetAxis = false;

      /// <summary>
      /// Whether place name labels should display
      /// </summary>
      internal bool showPlacenames = true;

      /// <summary>
      /// Whether country borders and other boundaries should display
      /// </summary>
      internal bool showBoundaries = false;

      /// <summary>
      /// Displays coordinates of current position
      /// </summary>
      internal bool showPosition = false;

		/// <summary>
		/// Whether to display the scale bar in the globe.
		/// </summary>
		protected bool m_blShowScaleBar = false;


      /// <summary>
      /// Color of the sky at sea level
      /// </summary>
      internal int skyColor = Color.FromArgb(115, 155, 185).ToArgb();

      [XmlIgnore]
      [Browsable(true), Category("UI")]
      [Editor(typeof(ColorEditor), typeof(UITypeEditor))]
      [Description("Color of the sky at sea level.")]
		public Color SkyColor
      {
         get { return Color.FromArgb(skyColor); }
			set { skyColor = value.ToArgb(); }
      }

      /// <summary>
      /// Keep the original (unconverted) NASA SVS image files on disk (in addition to converted files). 
      /// </summary>
      internal bool keepOriginalSvsImages = false;

      [Browsable(true), Category("World")]
      [Description("Whether to display the planet axis line (through poles).")]
      internal bool ShowPlanetAxis
      {
         get { return showPlanetAxis; }
			private set { showPlanetAxis = value; }
      }

      internal bool showClouds = false;
      [Browsable(true), Category("World")]
      [Description("Whether to show clouds.")]
		public bool ShowClouds
      {
         get { return showClouds; }
			set { showClouds = value; }
      }

      [Browsable(true), Category("World")]
      [Description("Whether place name labels should display")]
		public bool ShowPlacenames
      {
         get { return showPlacenames; }
			set { showPlacenames = value; }
      }

      [Browsable(true), Category("World")]
      [Description("Whether country borders and other boundaries should display")]
      internal bool ShowBoundaries
      {
         get { return showBoundaries; }
			private set { showBoundaries = value; }
      }

      [Browsable(true), Category("World")]
      [Description("Displays coordinates of current position.")]
		public bool ShowPosition
      {
         get { return showPosition; }
			set { showPosition = value; }
      }

		[Browsable(true), Category("World")]
		[Description("Whether to display the scale bar over the globe")]
		public bool ShowScaleBar
		{
			get { return m_blShowScaleBar; }
			set { m_blShowScaleBar = value; }
		}

      [Browsable(true), Category("World")]
      [Description("Keep the original (unconverted) NASA SVS image files on disk (in addition to converted files). ")]
      internal bool KeepOriginalSvsImages
      {
         get { return keepOriginalSvsImages; }
			private set { keepOriginalSvsImages = value; }
      }

      [Browsable(false), Category("World")]
      [Description("Index of Blue Marble version to show.")]
		public int BmngVersion
      {
         get { return bmngVersion; }
			set { bmngVersion = value; }
      }

      #endregion

      #region Camera

      internal bool cameraResetsAtStartup = true;
      internal Angle cameraLatitude = Angle.FromDegrees(0.0);
      internal Angle cameraLongitude = Angle.FromDegrees(0.0);
      internal double cameraAltitudeMeters = 20000000;
      internal Angle cameraHeading = Angle.FromDegrees(0.0);
      internal Angle cameraTilt = Angle.FromDegrees(0.0);

      internal bool cameraIsPointGoto = true;
      internal bool cameraHasInertia = true;
      internal bool cameraSmooth = true;
      internal bool cameraHasMomentum = false;
      internal bool cameraTwistLock = true;
      internal bool cameraBankLock = true;
      internal float cameraSlerpStandard = 0.35f;
      internal float cameraSlerpInertia = 0.25f;

      // Set to either Inertia or Standard slerp value
      internal float cameraSlerpPercentage = 0.25f;

      internal Angle cameraFov = Angle.FromRadians(Math.PI * 0.25f);
      internal Angle cameraFovMin = Angle.FromDegrees(5);
      internal Angle cameraFovMax = Angle.FromDegrees(150);
      internal float cameraZoomStepFactor = 0.015f;
      internal float cameraZoomAcceleration = 10f;
      internal float cameraZoomAnalogFactor = 1f;
      internal float cameraZoomStepKeyboard = 0.15f;
      internal float cameraRotationSpeed = 3.5f;
      internal bool elevateCameraLookatPoint = true;
      internal bool allowNegativeAltitude = false;

      [Browsable(true), Category("Camera")]
      internal bool ElevateCameraLookatPoint
      {
         get { return elevateCameraLookatPoint; }
			private set { elevateCameraLookatPoint = value; }
      }

      [Browsable(true), Category("Camera")]
      [Description("Allow camera to go below sea level - experimental.")]
      internal bool AllowNegativeAltitude
      {
         get { return allowNegativeAltitude; }
			private set { allowNegativeAltitude = value; }
      }

      [Browsable(true), Category("Camera")]
		public bool CameraResetsAtStartup
      {
         get { return cameraResetsAtStartup; }
			private set { cameraResetsAtStartup = value; }
      }

      //[Browsable(true),Category("Camera")]
		public Angle CameraLatitude
      {
         get { return cameraLatitude; }
			private set { cameraLatitude = value; }
      }

      //[Browsable(true),Category("Camera")]
		public Angle CameraLongitude
      {
         get { return cameraLongitude; }
			private set { cameraLongitude = value; }
      }

		public double CameraAltitude
      {
         get { return cameraAltitudeMeters; }
         private set { cameraAltitudeMeters = value; }
      }

      //[Browsable(true),Category("Camera")]
		public Angle CameraHeading
      {
         get { return cameraHeading; }
			private set { cameraHeading = value; }
      }

		public Angle CameraTilt
      {
         get { return cameraTilt; }
			private set { cameraTilt = value; }
      }

      [Browsable(true), Category("Camera")]
      internal bool CameraIsPointGoto
      {
         get { return cameraIsPointGoto; }
			private set { cameraIsPointGoto = value; }
      }

      [Browsable(true), Category("Camera")]
      [Description("Smooth camera movement.")]
		public bool CameraSmooth
      {
         get { return cameraSmooth; }
			set { cameraSmooth = value; }
      }

      [Browsable(true), Category("Camera")]
      [Description("See CameraSlerp settings for responsiveness adjustment.")]
      internal bool CameraHasInertia
      {
         get { return cameraHasInertia; }
			private set
         {
            cameraHasInertia = value;
            cameraSlerpPercentage = cameraHasInertia ? cameraSlerpInertia : cameraSlerpStandard;
         }
      }

      [Browsable(true), Category("Camera")]
		public bool CameraHasMomentum
      {
         get { return cameraHasMomentum; }
			private set { cameraHasMomentum = value; }
      }

      [Browsable(true), Category("Camera")]
		public bool CameraTwistLock
      {
         get { return cameraTwistLock; }
			private set { cameraTwistLock = value; }
      }

      [Browsable(true), Category("Camera")]
      public bool CameraBankLock
      {
         get { return cameraBankLock; }
			private set { cameraBankLock = value; }
      }

      [Browsable(true), Category("Camera")]
      [Description("Responsiveness of movement when inertia is enabled.")]
		public float CameraSlerpInertia
      {
         get { return cameraSlerpInertia; }
			private set
         {
            cameraSlerpInertia = value;
            if (cameraHasInertia)
               cameraSlerpPercentage = cameraSlerpInertia;
         }
      }

      [Browsable(true), Category("Camera")]
      [Description("Responsiveness of movement when inertia is disabled.")]
      internal float CameraSlerpStandard
      {
         get { return cameraSlerpStandard; }
			private set
         {
            cameraSlerpStandard = value;
            if (!cameraHasInertia)
               cameraSlerpPercentage = cameraSlerpStandard;
         }
      }

      [Browsable(true), Category("Camera")]
      internal Angle CameraFov
      {
         get { return cameraFov; }
			private set { cameraFov = value; }
      }

      [Browsable(true), Category("Camera")]
      internal Angle CameraFovMin
      {
         get { return cameraFovMin; }
			private set { cameraFovMin = value; }
      }

      [Browsable(true), Category("Camera")]
      internal Angle CameraFovMax
      {
         get { return cameraFovMax; }
			private set { cameraFovMax = value; }
      }

      [Browsable(true), Category("Camera")]
      internal float CameraZoomStepFactor
      {
         get { return cameraZoomStepFactor; }
			private set
         {
            const float maxValue = 0.3f;
            const float minValue = 1e-4f;

            if (value >= maxValue)
               value = maxValue;
            if (value <= minValue)
               value = minValue;
            cameraZoomStepFactor = value;
         }
      }

      [Browsable(true), Category("Camera")]
      internal float CameraZoomAcceleration
      {
         get { return cameraZoomAcceleration; }
			private set
         {
            const float maxValue = 50f;
            const float minValue = 1f;

            if (value >= maxValue)
               value = maxValue;
            if (value <= minValue)
               value = minValue;

            cameraZoomAcceleration = value;
         }
      }

      [Browsable(true), Category("Camera")]
      [Description("Analog zoom factor (Mouse LMB+RMB)")]
		public float CameraZoomAnalogFactor
      {
         get { return cameraZoomAnalogFactor; }
			private set { cameraZoomAnalogFactor = value; }
      }

      [Browsable(true), Category("Camera")]
		public float CameraZoomStepKeyboard
      {
         get { return cameraZoomStepKeyboard; }
			private set
         {
            const float maxValue = 0.3f;
            const float minValue = 1e-4f;

            if (value >= maxValue)
               value = maxValue;
            if (value <= minValue)
               value = minValue;

            cameraZoomStepKeyboard = value;
         }
      }

      float m_cameraDoubleClickZoomFactor = 2.0f;
      [Browsable(true), Category("Camera")]
		public float CameraDoubleClickZoomFactor
      {
         get { return m_cameraDoubleClickZoomFactor; }
			private set
         {
            m_cameraDoubleClickZoomFactor = value;
         }
      }

      [Browsable(true), Category("Camera")]
		public float CameraRotationSpeed
      {
         get { return cameraRotationSpeed; }
			private set { cameraRotationSpeed = value; }
      }

      #endregion

      #region Time

      [Browsable(true), Category("Time")]
      [Description("Controls the time multiplier for the Time Keeper.")]
      [XmlIgnore]
      internal float TimeMultiplier
      {
         get { return TimeKeeper.TimeMultiplier; }
			private set { TimeKeeper.TimeMultiplier = value; }
      }

      #endregion

      #region 3D

      private Format textureFormat = Format.Dxt3;
      private bool m_UseBelowNormalPriorityUpdateThread = false;
      private bool m_AlwaysRenderWindow = false;

      [Browsable(true), Category("3D settings")]
      [Description("This feature is not supported in Dapple and will always return false.")]
      internal bool ConvertDownloadedImagesToDds
      {
         get
         {
            return false;
         }
			private set
         {
         }
      }

      [Browsable(true), Category("3D settings")]
      [Description("Always Renders the 3D window even form is unfocused.")]
      public bool AlwaysRenderWindow
      {
         get
         {
            return m_AlwaysRenderWindow;
         }
         private set
         {
            m_AlwaysRenderWindow = value;
         }
      }

      [Browsable(true), Category("3D settings")]
      [Description("In-memory texture format.  Also used for converted files on disk when image conversion is enabled.")]
		public Format TextureFormat
      {
         get
         {
            //	return Format.Dxt3;
            return textureFormat;
         }
			private set
         {
            textureFormat = value;
         }
      }

      private bool m_enableSunShading = false;
      [Browsable(true), Category("3D settings")]
      [Description("Shade the Earth according to the Sun's position at a certain time.")]
		public bool EnableSunShading
      {
         get
         {
            return m_enableSunShading;
         }
			set
         {
            m_enableSunShading = value;
         }
      }

      private bool m_sunSynchedWithTime = false;
      [Browsable(true), Category("3D settings")]
      [Description("Sun position is computed according to time.")]
		public bool SunSynchedWithTime
      {
         get
         {
            return m_sunSynchedWithTime;
         }
			set
         {
            m_sunSynchedWithTime = value;
         }
      }

      private double m_sunElevation = Math.PI / 4;
      [Browsable(true), Category("3D settings")]
      [Description("Sun elevation when not synched to time.")]
		public double SunElevation
      {
         get
         {
            return m_sunElevation;
         }
			set
         {
            m_sunElevation = value;
         }
      }

      private double m_sunHeading = -Math.PI / 4;
      [Browsable(true), Category("3D settings")]
      [Description("Sun direction when not synched to time.")]
		public double SunHeading
      {
         get
         {
            return m_sunHeading;
         }
			set
         {
            m_sunHeading = value;
         }
      }

      private double m_sunDistance = 150000000000;
      [Browsable(true), Category("3D settings")]
      [Description("Sun distance in meter.")]
      internal double SunDistance
      {
         get
         {
            return m_sunDistance;
         }
			private set
         {
            m_sunDistance = value;
         }
      }
      private int m_LightColor = System.Drawing.Color.FromArgb(255, 255, 255).ToArgb();
      [Browsable(true), Category("3D settings")]
      [Description("The light color when sun shading is enabled.")]
      [XmlIgnore]
      internal System.Drawing.Color LightColor
      {
         get
         {
            return System.Drawing.Color.FromArgb(m_LightColor);
         }
			private set
         {
            m_LightColor = value.ToArgb();
         }
      }

      private int m_shadingAmbientColor = System.Drawing.Color.FromArgb(50, 50, 50).ToArgb();
      [Browsable(true), Category("3D settings")]
      [Description("The background ambient color when sun shading is enabled.")]
      [XmlIgnore]
      internal System.Drawing.Color ShadingAmbientColor
      {
         get
         {
            return System.Drawing.Color.FromArgb(m_shadingAmbientColor);
         }
			private set
         {
            m_shadingAmbientColor = value.ToArgb();
         }
      }

      private int m_standardAmbientColor = System.Drawing.Color.FromArgb(64, 64, 64).ToArgb();
      [Browsable(true), Category("3D settings")]
      [Description("The background ambient color only ambient lighting is used.")]
      [XmlIgnore]
		public System.Drawing.Color StandardAmbientColor
      {
         get
         {
            return System.Drawing.Color.FromArgb(m_standardAmbientColor);
         }
			private set
         {
            m_standardAmbientColor = value.ToArgb();
         }
      }

      [Browsable(true), Category("3D settings")]
      [Description("Use lower priority update thread to allow smoother rendering at the expense of data update frequency.")]
		public bool UseBelowNormalPriorityUpdateThread
      {
         get
         {
            return m_UseBelowNormalPriorityUpdateThread;
         }
			private set
         {
            m_UseBelowNormalPriorityUpdateThread = value;
         }
      }

      #endregion

      #region Terrain

      private float minSamplesPerDegree = 3.0f;

      [Browsable(true), Category("Terrain")]
      [Description("Sets the minimum samples per degree for which elevation is applied.")]
      internal float MinSamplesPerDegree
      {
         get
         {
            return minSamplesPerDegree;
         }
			private set
         {
            minSamplesPerDegree = value;
         }
      }

      private bool useWorldSurfaceRenderer = true;

      [Browsable(true), Category("Terrain")]
      [Description("Use World Surface Renderer for the visualization of multiple terrain-mapped layers.")]
      internal bool UseWorldSurfaceRenderer
      {
         get
         {
            return useWorldSurfaceRenderer;
         }
			private set
         {
            useWorldSurfaceRenderer = value;
         }
      }

      private float verticalExaggeration = 3.0f;

      [Browsable(true), Category("Terrain")]
      [Description("Terrain height multiplier.")]
		public float VerticalExaggeration
      {
         get
         {
            return verticalExaggeration;
         }
			set
         {
            if (value > 20)
               throw new ArgumentException("Vertical exaggeration out of range: " + value);
            if (value <= 0)
               verticalExaggeration = Single.Epsilon;
            else
               verticalExaggeration = value;
         }
      }

      #endregion

      #region Measure tool

      internal MeasureMode measureMode;

      internal bool measureShowGroundTrack;

      internal int measureLineGroundColor = Color.FromArgb(222, 0, 255, 0).ToArgb();
      internal int measureLineLinearColor = Color.FromArgb(255, 255, 0, 0).ToArgb();

      [XmlIgnore]
      [Browsable(true), Category("UI")]
      [Editor(typeof(ColorEditor), typeof(UITypeEditor))]
      [Description("Color of the linear distance measure line.")]
      internal Color MeasureLineLinearColor
      {
         get { return Color.FromArgb(measureLineLinearColor); }
			private set { measureLineLinearColor = value.ToArgb(); }
      }

      [Browsable(false)]
		public int MeasureLineLinearColorXml
      {
         get { return measureLineLinearColor; }
			private set { measureLineLinearColor = value; }
      }

      [XmlIgnore]
      [Browsable(true), Category("UI")]
      [Editor(typeof(ColorEditor), typeof(UITypeEditor))]
      [Description("Color of the ground track measure line.")]
      internal Color MeasureLineGroundColor
      {
         get { return Color.FromArgb(measureLineGroundColor); }
			private set { measureLineGroundColor = value.ToArgb(); }
      }

      [Browsable(false)]
      internal int MeasureLineGroundColorXml
      {
         get { return measureLineGroundColor; }
			private set { measureLineGroundColor = value; }
      }

      [Browsable(true), Category("UI")]
      [Description("Display the ground track column in the measurement statistics table.")]
      internal bool MeasureShowGroundTrack
      {
         get { return measureShowGroundTrack; }
			private set { measureShowGroundTrack = value; }
      }

      [Browsable(true), Category("UI")]
      [Description("Measure tool operation mode.")]
      internal MeasureMode MeasureMode
      {
         get { return measureMode; }
			private set { measureMode = value; }
      }

      #endregion

      #region Units
      private Units m_displayUnits = Units.Metric;
      [Browsable(true), Category("Units")]
      [Description("The target display units for measurements.")]
		public Units DisplayUnits
      {
         get
         {
            return m_displayUnits;
         }
			private set
         {
            m_displayUnits = value;
         }
      }
      #endregion

      private TimeSpan terrainTileRetryInterval = TimeSpan.FromMinutes(30);

      [Browsable(true), Category("Terrain")]
      [Description("Retry Interval for missing terrain tiles.")]
      [XmlIgnore]
      internal TimeSpan TerrainTileRetryInterval
      {
         get
         {
            return terrainTileRetryInterval;
         }
			private set
         {
            TimeSpan minimum = TimeSpan.FromMinutes(1);
            if (value < minimum)
               value = minimum;
            terrainTileRetryInterval = value;
         }
      }

      internal int downloadQueuedColor = Color.FromArgb(50, 128, 168, 128).ToArgb();

      [XmlIgnore]
      [Browsable(true), Category("UI")]
      [Editor(typeof(ColorEditor), typeof(UITypeEditor))]
      [Description("Color of queued for download image tile rectangles.")]
      internal Color DownloadQueuedColor
      {
         get { return Color.FromArgb(downloadQueuedColor); }
			private set { downloadQueuedColor = value.ToArgb(); }
      }

      #region Layers
      internal System.Collections.ArrayList loadedLayers = new System.Collections.ArrayList();
      internal bool useDefaultLayerStates = true;
      internal int maxSimultaneousDownloads = 1;

      [Browsable(true), Category("Layers")]
      internal bool UseDefaultLayerStates
      {
         get { return useDefaultLayerStates; }
			private set { useDefaultLayerStates = value; }
      }

      [Browsable(true), Category("Layers")]
      internal int MaxSimultaneousDownloads
      {
         get { return maxSimultaneousDownloads; }
			private set
         {
            if (value > 20)
               maxSimultaneousDownloads = 20;
            else if (value < 1)
               maxSimultaneousDownloads = 1;
            else
               maxSimultaneousDownloads = value;
         }
      }

      [Browsable(true), Category("Layers")]
      internal System.Collections.ArrayList LoadedLayers
      {
         get { return loadedLayers; }
			private set { loadedLayers = value; }
      }
      #endregion

      [Browsable(true), Category("Logging")]
		public bool Log404Errors
      {
         get { return WorldWind.Net.WebDownload.Log404Errors; }
			private set { WorldWind.Net.WebDownload.Log404Errors = value; }
      }

      // comment out ToString() to have namespace+class name being used as filename
		public override string ToString()
      {
         return "World";
      }
   }
}
