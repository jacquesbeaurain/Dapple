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
      bool enableAtmosphericScattering;

      [Browsable(true), Category("Atmosphere")]
      [Description("Enable Atmospheric Scattering")]
      public bool EnableAtmosphericScattering
      {
         get { return enableAtmosphericScattering; }
         set { enableAtmosphericScattering = value; }
      }

      bool forceCpuAtmosphere = true;
      [Browsable(true), Category("Atmosphere")]
      [Description("Forces CPU calculation instead of GPU for Atmospheric Scattering")]
      internal bool ForceCpuAtmosphere
      {
         get { return forceCpuAtmosphere; }
      }

      #endregion

      #region UI

      /// <summary>
      /// Show the top tool button bar
      /// </summary>
      bool showToolbar = true;

      /// <summary>
      /// Display the layer manager window
      /// </summary>
      bool showLayerManager = false;

      /// <summary>
      /// Display cross-hair symbol on screen
      /// </summary>
      bool showCrosshairs;
      int crosshairColor = Color.Beige.ToArgb();
      int crosshairSize = 10;

      /// <summary>
      /// Font name for the default font used in UI
      /// </summary>
      string defaultFontName = "Tahoma";

      /// <summary>
      /// Font size (em) for the default font used in UI
      /// </summary>
      float defaultFontSize = 9.0f;

      /// <summary>
      /// Layer manager width (pixels)
      /// </summary>
      int layerManagerWidth = 200;

      /// <summary>
      /// Draw anti-aliased text
      /// </summary>
      bool antiAliasedText = false;

      /// <summary>
      /// Vsync on/off (Wait for vertical retrace)
      /// </summary>
      bool vSync = true;

      int m_FpsFrameCount = 300;
      bool m_ShowFpsGraph = false;

      int downloadTerrainRectangleColor = Color.FromArgb(50, 0, 0, 255).ToArgb();
      int downloadProgressColor = Color.FromArgb(50, 255, 0, 0).ToArgb();
      int downloadLogoColor = Color.FromArgb(180, 255, 255, 255).ToArgb();
      int widgetBackgroundColor = Color.FromArgb(0, 0, 0, 255).ToArgb();
      bool showDownloadIndicator = true;
      bool outlineText = false;
      bool showCompass;
      WFSNameColors nameColors = WFSNameColors.Default;
      float nameSizeMultiplier = 1.0f;

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
      }

      [Browsable(true), Category("UI")]
      [Description("Change name colors for visibility.")]
      internal WFSNameColors WFSNameColors
      {
         get { return nameColors; }
      }

      [Browsable(true), Category("UI")]
      [Description("Factor by which default text size will be multiplied")]
      internal float WFSNameSizeMultiplier
      {
         get { return nameSizeMultiplier; }
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
      [Description("Widget background color.")]
      public Color WidgetBackgroundColor
      {
         get { return Color.FromArgb(widgetBackgroundColor); }
      }

      [XmlIgnore]
      [Browsable(true), Category("UI")]
      [Editor(typeof(ColorEditor), typeof(UITypeEditor))]
      [Description("Color/transparency of the download progress icon.")]
      internal Color DownloadLogoColor
      {
         get { return Color.FromArgb(downloadLogoColor); }
      }

      [XmlIgnore]
      [Browsable(true), Category("UI")]
      [Editor(typeof(ColorEditor), typeof(UITypeEditor))]
      [Description("Color of the download progress bar.")]
      public Color DownloadProgressColor
      {
         get { return Color.FromArgb(downloadProgressColor); }
      }

      [XmlIgnore]
      [Browsable(true), Category("UI")]
      [Editor(typeof(ColorEditor), typeof(UITypeEditor))]
      [Description("Color of the terrain download in progress rectangle.")]
      internal Color DownloadTerrainRectangleColor
      {
         get { return Color.FromArgb(downloadTerrainRectangleColor); }
      }

      [Browsable(true), Category("UI")]
      [Description("Show the top tool button bar.")]
      public bool ShowToolbar
      {
         get { return showToolbar; }
      }

      [Browsable(true), Category("UI")]
      [Description("Display the layer manager window.")]
      internal bool ShowLayerManager
      {
         get { return showLayerManager; }
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
      }

      [Browsable(true), Category("UI")]
      [Description("Size of cross-hair.")]
      public int CrosshairSize
      {
         get { return crosshairSize; }
      }

      [Browsable(true), Category("UI")]
      [Description("Font name for the default font used in UI.")]
      internal string DefaultFontName
      {
         get { return defaultFontName; }
      }

      [Browsable(true), Category("UI")]
      [Description("Font size for the default font used in UI.")]
      internal float DefaultFontSize
      {
         get { return defaultFontSize; }
      }

      [Browsable(true), Category("UI")]
      [Description("Layer manager width (pixels)")]
      internal int LayerManagerWidth
      {
         get { return layerManagerWidth; }
      }

      /// <summary>
      /// Draw anti-aliased text
      /// </summary>
      [Browsable(true), Category("UI")]
      [Description("Enable anti-aliased text rendering. Change active only after program restart.")]
      internal bool AntiAliasedText
      {
         get { return antiAliasedText; }
      }

      [Browsable(true), Category("UI")]
      [Description("Synchronize render buffer swaps with the monitor's refresh rate (vertical retrace). Change active only after program restart.")]
      public bool VSync
      {
         get { return vSync; }
      }

      [Browsable(true), Category("UI")]
      [Description("Enables the Frames Per Second Graph")]
      public bool ShowFpsGraph
      {
         get { return m_ShowFpsGraph; }
      }

      [Browsable(true), Category("UI")]
      [Description("Changes length of the Fps Graph History")]
      public int FpsFrameCount
      {
         get { return m_FpsFrameCount; }
      }

      #endregion

      #region Grid

      /// <summary>
      /// Display the latitude/longitude grid
      /// </summary>
      bool showLatLonLines;

      /// <summary>
      /// The color of the latitude/longitude grid
      /// </summary>
      int latLonLinesColor = System.Drawing.Color.FromArgb(200, 160, 160, 160).ToArgb();

      /// <summary>
      /// The color of the equator latitude line
      /// </summary>
      int equatorLineColor = System.Drawing.Color.FromArgb(160, 64, 224, 208).ToArgb();

      /// <summary>
      /// Display the tropic of capricorn/cancer lines
      /// </summary>
      bool showTropicLines = true;

      /// <summary>
      /// The color of the latitude/longitude grid
      /// </summary>
      int tropicLinesColor = System.Drawing.Color.FromArgb(160, 176, 224, 230).ToArgb();

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
      }

      [XmlIgnore]
      [Browsable(true), Category("Grid Lines")]
      [Editor(typeof(ColorEditor), typeof(UITypeEditor))]
      [Description("The color of the equator latitude line.")]
      internal Color EquatorLineColor
      {
         get { return Color.FromArgb(equatorLineColor); }
      }

      [Browsable(true), Category("Grid Lines")]
      [Description("Display the tropic latitude lines.")]
      internal bool ShowTropicLines
      {
         get { return showTropicLines; }
      }

      [XmlIgnore]
      [Browsable(true), Category("Grid Lines")]
      [Description("The color of the latitude/longitude grid.")]
      [Editor(typeof(ColorEditor), typeof(UITypeEditor))]
      internal Color TropicLinesColor
      {
         get { return Color.FromArgb(tropicLinesColor); }
      }

      #endregion

      #region World

      /// <summary>
      /// Whether to display the planet axis line (through poles)
      /// </summary>
      bool showPlanetAxis = false;

      /// <summary>
      /// Whether place name labels should display
      /// </summary>
      bool showPlacenames = true;

      /// <summary>
      /// Displays coordinates of current position
      /// </summary>
      bool showPosition;

		/// <summary>
		/// Whether to display the scale bar in the globe.
		/// </summary>
		bool m_blShowScaleBar;


      /// <summary>
      /// Color of the sky at sea level
      /// </summary>
      int skyColor = Color.FromArgb(115, 155, 185).ToArgb();

      [XmlIgnore]
      [Browsable(true), Category("UI")]
      [Editor(typeof(ColorEditor), typeof(UITypeEditor))]
      [Description("Color of the sky at sea level.")]
      public Color SkyColor
      {
         get { return Color.FromArgb(skyColor); }
         set { skyColor = value.ToArgb(); }
      }

      [Browsable(true), Category("World")]
      [Description("Whether to display the planet axis line (through poles).")]
      internal bool ShowPlanetAxis
      {
         get { return showPlanetAxis; }
      }

      bool showClouds;
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

      #endregion

      #region Camera

      bool cameraResetsAtStartup = true;
      Angle cameraLatitude = Angle.FromDegrees(0.0);
      Angle cameraLongitude = Angle.FromDegrees(0.0);
      double cameraAltitudeMeters = 20000000;
      Angle cameraHeading = Angle.FromDegrees(0.0);
      Angle cameraTilt = Angle.FromDegrees(0.0);

      bool cameraIsPointGoto = true;
      bool cameraSmooth = true;
      bool cameraHasMomentum = false;
      bool cameraTwistLock = true;
      bool cameraBankLock = true;
      float cameraSlerpInertia = 0.25f;

      // Set to either Inertia or Standard slerp value
      float cameraSlerpPercentage = 0.25f;

		internal float CameraSlerpPercentage
		{
			get { return cameraSlerpPercentage; }
		}

      Angle cameraFov = Angle.FromRadians(Math.PI * 0.25f);
      Angle cameraFovMin = Angle.FromDegrees(5);
      Angle cameraFovMax = Angle.FromDegrees(150);
      float cameraZoomStepFactor = 0.015f;
      float cameraZoomAcceleration = 10f;
      float cameraZoomAnalogFactor = 1f;
      float cameraZoomStepKeyboard = 0.15f;
      float cameraRotationSpeed = 3.5f;
      bool elevateCameraLookatPoint = true;
      bool allowNegativeAltitude = false;

      [Browsable(true), Category("Camera")]
      internal bool ElevateCameraLookatPoint
      {
         get { return elevateCameraLookatPoint; }
      }

      [Browsable(true), Category("Camera")]
      [Description("Allow camera to go below sea level - experimental.")]
      internal bool AllowNegativeAltitude
      {
         get { return allowNegativeAltitude; }
      }

      [Browsable(true), Category("Camera")]
      public bool CameraResetsAtStartup
      {
         get { return cameraResetsAtStartup; }
      }

      //[Browsable(true),Category("Camera")]
      public Angle CameraLatitude
      {
         get { return cameraLatitude; }
         set { cameraLatitude = value; }
      }

      //[Browsable(true),Category("Camera")]
      public Angle CameraLongitude
      {
         get { return cameraLongitude; }
         set { cameraLongitude = value; }
      }

      public double CameraAltitude
      {
         get { return cameraAltitudeMeters; }
         set { cameraAltitudeMeters = value; }
      }

      //[Browsable(true),Category("Camera")]
      public Angle CameraHeading
      {
         get { return cameraHeading; }
         set { cameraHeading = value; }
      }

      public Angle CameraTilt
      {
         get { return cameraTilt; }
         set { cameraTilt = value; }
      }

      [Browsable(true), Category("Camera")]
      internal bool CameraIsPointGoto
      {
         get { return cameraIsPointGoto; }
      }

      [Browsable(true), Category("Camera")]
      [Description("Smooth camera movement.")]
      public bool CameraSmooth
      {
         get { return cameraSmooth; }
         set { cameraSmooth = value; }
      }

      [Browsable(true), Category("Camera")]
      public bool CameraHasMomentum
      {
         get { return cameraHasMomentum; }
      }

      [Browsable(true), Category("Camera")]
      public bool CameraTwistLock
      {
         get { return cameraTwistLock; }
      }

      [Browsable(true), Category("Camera")]
      public bool CameraBankLock
      {
         get { return cameraBankLock; }
      }

      [Browsable(true), Category("Camera")]
      [Description("Responsiveness of movement when inertia is enabled.")]
      public float CameraSlerpInertia
      {
         get { return cameraSlerpInertia; }
      }

      [Browsable(true), Category("Camera")]
      internal Angle CameraFov
      {
         get { return cameraFov; }
      }

      [Browsable(true), Category("Camera")]
      internal Angle CameraFovMin
      {
         get { return cameraFovMin; }
      }

      [Browsable(true), Category("Camera")]
      internal Angle CameraFovMax
      {
         get { return cameraFovMax; }
      }

      [Browsable(true), Category("Camera")]
      public float CameraZoomStepFactor
      {
         get { return cameraZoomStepFactor; }
      }

      [Browsable(true), Category("Camera")]
      internal float CameraZoomAcceleration
      {
         get { return cameraZoomAcceleration; }
      }

      [Browsable(true), Category("Camera")]
      [Description("Analog zoom factor (Mouse LMB+RMB)")]
      public float CameraZoomAnalogFactor
      {
         get { return cameraZoomAnalogFactor; }
      }

      [Browsable(true), Category("Camera")]
      public float CameraZoomStepKeyboard
      {
         get { return cameraZoomStepKeyboard; }
      }

      float m_cameraDoubleClickZoomFactor = 2.0f;
      [Browsable(true), Category("Camera")]
      public float CameraDoubleClickZoomFactor
      {
         get { return m_cameraDoubleClickZoomFactor; }
      }

      [Browsable(true), Category("Camera")]
      public float CameraRotationSpeed
      {
         get { return cameraRotationSpeed; }
      }

      #endregion

      #region 3D

      private Format textureFormat = Format.Dxt3;
      private bool m_UseBelowNormalPriorityUpdateThread = false;
      private bool m_AlwaysRenderWindow = false;

      [Browsable(true), Category("3D settings")]
      [Description("Always Renders the 3D window even form is unfocused.")]
      public bool AlwaysRenderWindow
      {
         get
         {
            return m_AlwaysRenderWindow;
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
      }

      private bool m_enableSunShading;
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

      private bool m_sunSynchedWithTime;
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
      }

      [Browsable(true), Category("3D settings")]
      [Description("Use lower priority update thread to allow smoother rendering at the expense of data update frequency.")]
      public bool UseBelowNormalPriorityUpdateThread
      {
         get
         {
            return m_UseBelowNormalPriorityUpdateThread;
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
      }
      #endregion

      int downloadQueuedColor = Color.FromArgb(50, 128, 168, 128).ToArgb();

      [XmlIgnore]
      [Browsable(true), Category("UI")]
      [Editor(typeof(ColorEditor), typeof(UITypeEditor))]
      [Description("Color of queued for download image tile rectangles.")]
      internal Color DownloadQueuedColor
      {
         get { return Color.FromArgb(downloadQueuedColor); }
      }

      #region Layers
      System.Collections.ArrayList loadedLayers = new System.Collections.ArrayList();
      bool useDefaultLayerStates = true;
      int maxSimultaneousDownloads = 1;

      [Browsable(true), Category("Layers")]
      internal bool UseDefaultLayerStates
      {
         get { return useDefaultLayerStates; }
      }

      [Browsable(true), Category("Layers")]
      internal int MaxSimultaneousDownloads
      {
         get { return maxSimultaneousDownloads; }
      }

      [Browsable(true), Category("Layers")]
      internal System.Collections.ArrayList LoadedLayers
      {
         get { return loadedLayers; }
      }
      #endregion

      [Browsable(true), Category("Logging")]
      public bool Log404Errors
      {
         get { return WorldWind.Net.WebDownload.Log404Errors; }
      }

      // comment out ToString() to have namespace+class name being used as filename
      public override string ToString()
      {
         return "World";
      }
   }
}
