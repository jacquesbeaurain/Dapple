using Microsoft.DirectX.Direct3D;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Xml.Serialization;
using WorldWind.Configuration;

namespace WorldWind
{

	public enum MeasureMode
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
		public int menuBarBackgroundColor = Color.FromArgb(128,128,128,128).ToArgb();

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

		internal int downloadTerrainRectangleColor = Color.FromArgb(180,0,0,255).ToArgb();
		internal int downloadProgressColor = Color.FromArgb(180,255,0,0).ToArgb();
		internal int downloadLogoColor = Color.FromArgb(180, 255, 255, 255).ToArgb();
		internal int menuBackColor = Color.FromArgb(170, 40, 40, 40).ToArgb();
		internal int menuOutlineColor = Color.FromArgb(150,160,160,160).ToArgb();
		internal int scrollbarColor = System.Drawing.Color.FromArgb(170, 100, 100, 100).ToArgb();
		internal int scrollbarHotColor = System.Drawing.Color.FromArgb(170, 255, 255, 255).ToArgb();
		internal int toolBarBackColor = System.Drawing.Color.FromArgb(100, 255, 255, 255).ToArgb();
		internal bool showDownloadRectangles = false;
		internal bool outlineText = false;
		internal bool showCompass = true;
      internal bool advancedMode = false;

		[Browsable(true),Category("UI")]
		[Description("Show Compass Indicator.")]
		public bool ShowCompass 
		{
			get { return showCompass; }
			set { showCompass = value; }
		}


		[Browsable(true),Category("UI")]
		[Description("Draw outline around text to improve visibility.")]
		public bool OutlineText
		{
			get { return outlineText; }
			set { outlineText = value; }
		}

		[Browsable(true),Category("UI")]
		[Description("Display download rectangles in 3D view.")]
		public bool ShowDownloadRectangles
		{
			get { return showDownloadRectangles; }
			set { showDownloadRectangles = value; }
		}

      [XmlIgnore]
		[Browsable(true),Category("UI")]
		[Editor(typeof(ColorEditor),typeof(UITypeEditor))]
		[Description("Toolbar Background color.")]
		public Color ToolBarBackColor
		{
			get { return Color.FromArgb(toolBarBackColor); }
			set { toolBarBackColor = value.ToArgb(); }
		}

		[XmlIgnore]
		[Browsable(true),Category("UI")]
		[Editor(typeof(ColorEditor),typeof(UITypeEditor))]
		[Description("Color of scrollbar when scrolling.")]
		public Color ScrollbarHotColor
		{
			get { return Color.FromArgb(scrollbarHotColor); }
			set { scrollbarHotColor = value.ToArgb(); }
		}

		[XmlIgnore]
		[Browsable(true),Category("UI")]
		[Editor(typeof(ColorEditor),typeof(UITypeEditor))]
		[Description("Color of scrollbar.")]
		public Color ScrollbarColor
		{
			get { return Color.FromArgb(scrollbarColor); }
			set { scrollbarColor = value.ToArgb(); }
		}

		[XmlIgnore]
		[Browsable(true),Category("UI")]
		[Editor(typeof(ColorEditor),typeof(UITypeEditor))]
		[Description("Menu border color.")]
		public Color MenuOutlineColor
		{
			get { return Color.FromArgb(menuOutlineColor); }
			set { menuOutlineColor = value.ToArgb(); }
		}

		[XmlIgnore]
		[Browsable(true),Category("UI")]
		[Editor(typeof(ColorEditor),typeof(UITypeEditor))]
		[Description("Background color of the menu.")]
		public Color MenuBackColor
		{
			get { return Color.FromArgb(menuBackColor); }
			set { menuBackColor = value.ToArgb(); }
		}

		[XmlIgnore]
		[Browsable(true),Category("UI")]
		[Editor(typeof(ColorEditor),typeof(UITypeEditor))]
		[Description("Color/transparency of the download progress icon.")]
		public Color DownloadLogoColor 
		{
			get { return Color.FromArgb(downloadLogoColor); }
			set { downloadLogoColor = value.ToArgb(); }
		}

		[XmlIgnore]
		[Browsable(true),Category("UI")]
		[Editor(typeof(ColorEditor),typeof(UITypeEditor))]
		[Description("Color of the download progress bar.")]
		public Color DownloadProgressColor 
		{
			get { return Color.FromArgb(downloadProgressColor); }
			set { downloadProgressColor = value.ToArgb(); }
		}

		[XmlIgnore]
		[Browsable(true),Category("UI")]
		[Editor(typeof(ColorEditor),typeof(UITypeEditor))]
		[Description("Color of the terrain download in progress rectangle.")]
		public Color DownloadTerrainRectangleColor 
		{
			get { return Color.FromArgb(downloadTerrainRectangleColor); }
			set { downloadTerrainRectangleColor = value.ToArgb(); }
		}

		[Browsable(true),Category("UI")]
		[Description("Show the top tool button bar.")]
		public bool ShowToolbar 
		{
			get { return showToolbar; }
			set { showToolbar = value; }
		}

		[Browsable(true),Category("UI")]
		[Description("Display the layer manager window.")]
		public bool ShowLayerManager
		{
			get { return showLayerManager; }
			set { showLayerManager = value; }
		}

		[Browsable(true),Category("UI")]
		[Description("Display cross-hair symbol on screen.")]
		public bool ShowCrosshairs 
		{
			get { return showCrosshairs; }
			set { showCrosshairs = value; }
		}

		[Browsable(true),Category("UI")]
		[Description("Font name for the default font used in UI.")]
		public string DefaultFontName 
		{
			get { return defaultFontName; }
			set { defaultFontName = value; }
		}

		[Browsable(true),Category("UI")]
		[Description("Font size for the default font used in UI.")]
		public float DefaultFontSize 
		{
			get { return defaultFontSize; }
			set { defaultFontSize = value; }
		}

		[Browsable(true),Category("UI")]
		[Description("Font style for the default font used in UI.")]
		public FontStyle DefaultFontStyle 
		{
			get { return defaultFontStyle; }
			set { defaultFontStyle = value; }
		}

		[Browsable(true),Category("UI")]
		[Description("Font name for the toolbar font used in UI.")]
		public string ToolbarFontName 
		{
			get { return toolbarFontName; }
			set { toolbarFontName = value; }
		}

		[Browsable(true),Category("UI")]
		[Description("Font size (em) for the toolbar font used in UI.")]
		public float ToolbarFontSize 
		{
			get { return toolbarFontSize; }
			set { toolbarFontSize = value; }
		}

		[Browsable(true),Category("UI")]
		[Description("Font style for the toolbar font used in UI.")]
		public FontStyle ToolbarFontStyle 
		{
			get { return toolbarFontStyle; }
			set { toolbarFontStyle = value; }
		}

		[XmlIgnore]
		[Browsable(true),Category("UI")]
		[Editor(typeof(ColorEditor),typeof(UITypeEditor))]
		[Description("Menu bar background color.")]
		public Color MenuBarBackgroundColor 
		{
			get { return Color.FromArgb(menuBarBackgroundColor); }
			set { menuBarBackgroundColor = value.ToArgb(); }
		}

		[Browsable(true),Category("UI")]
		[Description("Font name for the layer manager font.")]
		public string LayerManagerFontName 
		{
			get { return layerManagerFontName; }
			set { layerManagerFontName = value; }
		}

		[Browsable(true),Category("UI")]
		[Description("Font size for the layer manager font.")]
		public float LayerManagerFontSize 
		{
			get { return layerManagerFontSize; }
			set { layerManagerFontSize = value; }
		}

		[Browsable(true),Category("UI")]
		[Description("Font style for the layer manager font used in UI.")]
		public FontStyle LayerManagerFontStyle 
		{
			get { return layerManagerFontStyle; }
			set { layerManagerFontStyle = value; }
		}

		[Browsable(true),Category("UI")]
		[Description("Layer manager width (pixels)")]
		public int LayerManagerWidth 
		{
			get { return layerManagerWidth; }
			set { layerManagerWidth = value; }
		}

		/// <summary>
		/// Draw anti-aliased text
		/// </summary>
		[Browsable(true),Category("UI")]
		[Description("Enable anti-aliased text rendering. Change active only after program restart.")]
		public bool AntiAliasedText 
		{
			get { return antiAliasedText; }
			set { antiAliasedText = value; }
		}

		[Browsable(true),Category("UI")]
		[Description("Maximum frames-per-second setting. Optionally throttles the frame rate (to get consistent frame rates or reduce CPU usage. 0 = Disabled")]
		public int ThrottleFpsHz 
		{
			get { return throttleFpsHz; }
			set { throttleFpsHz = value; }
		}

		[Browsable(true),Category("UI")]
		[Description("Synchronize render buffer swaps with the monitor's refresh rate (vertical retrace). Change active only after program restart.")]
		public bool VSync
		{
			get { return vSync; }
			set { vSync = value; }
		}

		[Browsable(true),Category("UI")]
		[Description("Changes the size of the Rapid Fire Modis icons.")]
		public int ModisIconSize
		{
			get { return modisIconSize; }
			set { modisIconSize = value; }
		}

		[Browsable(true), Category("UI")]
      [Description("Toggles Advanced Mode")]
      public bool AdvancedMode
      {
         get { return advancedMode; }
         set { advancedMode = value; }
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
		public int latLonLinesColor = System.Drawing.Color.FromArgb(200,160,160,160).ToArgb();

		/// <summary>
		/// The color of the equator latitude line
		/// </summary>
		public int equatorLineColor = System.Drawing.Color.FromArgb(160,64,224,208).ToArgb();

		/// <summary>
		/// Display the tropic of capricorn/cancer lines
		/// </summary>
		internal bool showTropicLines = true;

		/// <summary>
		/// The color of the latitude/longitude grid
		/// </summary>
		public int tropicLinesColor = System.Drawing.Color.FromArgb(160,176,224,230).ToArgb();

		[Browsable(true),Category("Grid Lines")]
		[Description("Display the latitude/longitude grid.")]
		public bool ShowLatLonLines 
		{
			get { return showLatLonLines; }
			set { showLatLonLines = value; }
		}

		[XmlIgnore]
		[Browsable(true),Category("Grid Lines")]
		[Editor(typeof(ColorEditor),typeof(UITypeEditor))]
		[Description("The color of the latitude/longitude grid.")]
		public Color LatLonLinesColor 
		{
			get { return Color.FromArgb(latLonLinesColor); }
			set { latLonLinesColor = value.ToArgb(); }
		}

		[XmlIgnore]
		[Browsable(true),Category("Grid Lines")]
		[Editor(typeof(ColorEditor),typeof(UITypeEditor))]
		[Description("The color of the equator latitude line.")]
		public Color EquatorLineColor
		{
			get { return Color.FromArgb(equatorLineColor); }
			set { equatorLineColor = value.ToArgb(); }
		}

		[Browsable(true),Category("Grid Lines")]
		[Description("Display the tropic latitude lines.")]
		public bool ShowTropicLines
		{
			get { return showTropicLines; }
			set { showTropicLines = value; }
		}

		[XmlIgnore]
		[Browsable(true),Category("Grid Lines")]
		[Description("The color of the latitude/longitude grid.")]
		[Editor(typeof(ColorEditor),typeof(UITypeEditor))]
		public Color TropicLinesColor 
		{
			get { return Color.FromArgb(tropicLinesColor); }
			set { tropicLinesColor = value.ToArgb(); }
		}

		#endregion

		#region World

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
		/// Color of the sky at sea level
		/// </summary>
		internal int skyColor = Color.FromArgb(115, 155, 185).ToArgb();
		
		[XmlIgnore]
		[Browsable(true),Category("UI")]
		[Editor(typeof(ColorEditor),typeof(UITypeEditor))]
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

		[Browsable(true),Category("World")]
		[Description("Whether to display the planet axis line (through poles).")]
		public bool ShowPlanetAxis
		{
			get { return showPlanetAxis; }
			set { showPlanetAxis = value; }
		}

		internal bool showClouds = true;
		[Browsable(true),Category("World")]
		[Description("Whether to show clouds.")]
		public bool ShowClouds
		{
			get { return showClouds; }
			set { showClouds = value; }
		}

		[Browsable(true),Category("World")]
		[Description("Whether place name labels should display")]
		public bool ShowPlacenames 
		{
			get { return showPlacenames; }
			set { showPlacenames = value; }
		}

		[Browsable(true),Category("World")]
		[Description("Whether country borders and other boundaries should display")]
		public bool ShowBoundaries 
		{
			get { return showBoundaries; }
			set { showBoundaries = value; }
		}

		[Browsable(true),Category("World")]
		[Description("Displays coordinates of current position.")]
		public bool ShowPosition
		{
			get { return showPosition; }
			set { showPosition = value; }
		}

		[Browsable(true),Category("World")]
		[Description("Keep the original (unconverted) NASA SVS image files on disk (in addition to converted files). ")]
		public bool KeepOriginalSvsImages
		{
			get { return keepOriginalSvsImages; }
			set { keepOriginalSvsImages = value; }
		}

		#endregion

		#region Camera

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

		internal Angle cameraFov = Angle.FromRadians( Math.PI * 0.25f );
		internal Angle cameraFovMin = Angle.FromDegrees( 5 );
		internal Angle cameraFovMax = Angle.FromDegrees( 150 );
		internal float cameraZoomStepFactor = 0.015f;
		internal float cameraZoomAcceleration = 10f;
		internal float cameraZoomAnalogFactor = 1f;
		internal float cameraZoomStepKeyboard = 0.15f;
		internal float cameraRotationSpeed =  3.5f;


		[Browsable(true),Category("Camera")]
		public bool CameraIsPointGoto
		{
			get { return cameraIsPointGoto; }
			set { cameraIsPointGoto = value; }
		}

		[Browsable(true),Category("Camera")]
		[Description("Smooth camera movement.")]
		public bool CameraSmooth
		{
			get { return cameraSmooth; }
			set { cameraSmooth = value; }
		}

		[Browsable(true),Category("Camera")]
		[Description("See CameraSlerp settings for responsiveness adjustment.")]
		public bool CameraHasInertia
		{
			get { return cameraHasInertia; }
			set { 
				cameraHasInertia = value; 
				cameraSlerpPercentage = cameraHasInertia ? cameraSlerpInertia : cameraSlerpStandard;
			}
		}

		[Browsable(true),Category("Camera")]
		public bool CameraHasMomentum 
		{
			get { return cameraHasMomentum; }
			set { cameraHasMomentum = value; }
		}

		[Browsable(true),Category("Camera")]
		public bool CameraTwistLock
		{
			get { return cameraTwistLock; }
			set { cameraTwistLock = value; }
		}

		[Browsable(true),Category("Camera")]
		public bool CameraBankLock 
		{
			get { return cameraBankLock; }
			set { cameraBankLock = value; }
		}

		[Browsable(true),Category("Camera")]
		[Description("Responsiveness of movement when inertia is enabled.")]
		public float CameraSlerpInertia
		{
			get { return cameraSlerpInertia; }
			set { 
				cameraSlerpInertia = value; 
				if(cameraHasInertia)
					cameraSlerpPercentage = cameraSlerpInertia;
			}
		}

		[Browsable(true),Category("Camera")]
		[Description("Responsiveness of movement when inertia is disabled.")]
		public float CameraSlerpStandard
		{
			get { return cameraSlerpStandard; }
			set { 
				cameraSlerpStandard = value; 
				if(!cameraHasInertia)
					cameraSlerpPercentage = cameraSlerpStandard;
			}
		}

		[Browsable(true),Category("Camera")]
		public Angle CameraFov 
		{
			get { return cameraFov; }
			set { cameraFov = value; }
		}

		[Browsable(true),Category("Camera")]
		public Angle CameraFovMin 
		{
			get { return cameraFovMin; }
			set { cameraFovMin = value; }
		}

		[Browsable(true),Category("Camera")]
		public Angle CameraFovMax 
		{
			get { return cameraFovMax; }
			set { cameraFovMax = value; }
		}

		[Browsable(true),Category("Camera")]
		public float CameraZoomStepFactor 
		{
			get { return cameraZoomStepFactor; }
			set { 
				const float maxValue = 0.3f;
				const float minValue = 1e-4f;

				if(value>=maxValue)
					value = maxValue;
				if (value <=minValue)
					value = minValue;
				cameraZoomStepFactor = value; 
			}
		}

		[Browsable(true),Category("Camera")]
		public float CameraZoomAcceleration
		{
			get { return cameraZoomAcceleration; }
			set { 
				const float maxValue = 50f;
				const float minValue = 1f;

				if(value>=maxValue)
					value = maxValue;
				if (value <=minValue)
					value = minValue;

				cameraZoomAcceleration = value; 
			}
		}

		[Browsable(true),Category("Camera")]
		[Description("Analog zoom factor (Mouse LMB+RMB)")]
		public float CameraZoomAnalogFactor 
		{
			get { return cameraZoomAnalogFactor; }
			set { cameraZoomAnalogFactor = value; }
		}

		[Browsable(true),Category("Camera")]
		public float CameraZoomStepKeyboard
		{
			get { return cameraZoomStepKeyboard; }
			set { 
				const float maxValue = 0.3f;
				const float minValue = 1e-4f;

				if(value>=maxValue)
					value = maxValue;
				if (value <=minValue)
					value = minValue;

				cameraZoomStepKeyboard = value; 
			}
		}

		[Browsable(true),Category("Camera")]
		public float CameraRotationSpeed
		{
			get { return cameraRotationSpeed; }
			set { cameraRotationSpeed = value; }
		}

		#endregion

		#region 3D

		private Format textureFormat = Format.Dxt1;
		private bool convertDownloadedImagesToDds = false;
		private bool m_AllowPureDevice = false;
		private bool m_UseBelowNormalPriorityUpdateThread = false;
		private bool m_UseHardwareBuffers = false;
		private bool m_AlwaysRenderWindow = false;

		[Browsable(true),Category("3D settings")]
		[Description("Always Renders the 3D window even form is unfocused.")]
		public bool AlwaysRenderWindow
		{
			get
			{
				return m_AlwaysRenderWindow;
			}
			set
			{
				m_AlwaysRenderWindow = value;
			}
		}

		[Browsable(true),Category("3D settings")]
		[Description("Enables Hardware memory buffers for increased rendering performance at the cost of additional graphics memory.")]
		public bool UseHardwareBuffers
		{
			get
			{
				return m_UseHardwareBuffers;
			}
			set
			{
				m_UseHardwareBuffers = value;
			}
		}

		[Browsable(true),Category("3D settings")]
		[Description("Enables image conversion to DDS files when loading images. TextureFormat controls the sub-format of the DDS file.")]
		public bool ConvertDownloadedImagesToDds
		{
			get
			{
				return convertDownloadedImagesToDds;
			}
			set
			{
				convertDownloadedImagesToDds = value;
			}
		}

		[Browsable(true),Category("3D settings")]
		[Description("In-memory texture format.  Also used for converted files on disk when image conversion is enabled.")]
		public Format TextureFormat
		{
			get
			{
				return textureFormat;
			}
			set
			{
				textureFormat = value;
			}
		}

		[Browsable(true),Category("3D settings")]
		[Description("Allows use of the Pure Device, which allows faster rendering, but might break older World Wind plugins that need some updating.  Requires restart of application.")]
		public bool AllowPureDevice
		{
			get
			{
				return m_AllowPureDevice;
			}
			set
			{
				m_AllowPureDevice = value;
			}
		}
		
		[Browsable(true),Category("3D settings")]
		[Description("Use lower priority update thread to allow smoother rendering at the expense of data update frequency.")]
		public bool UseBelowNormalPriorityUpdateThread
		{
			get
			{
				return m_UseBelowNormalPriorityUpdateThread;
			}
			set
			{
				m_UseBelowNormalPriorityUpdateThread = value;
			}
		}

		#endregion

		#region Terrain

		private float verticalExaggeration = 3.0f;

		[Browsable(true),Category("Terrain")]
		[Description("Terrain height multiplier.")]
		public float VerticalExaggeration
		{
			get
			{
				return verticalExaggeration;
			}
			set
			{
				if(value>20)
					throw new ArgumentException("Vertical exaggeration out of range: "+value);
				if(value <= 0)
					verticalExaggeration = Single.Epsilon;
				else 
					verticalExaggeration = value;
			}
		}

		#endregion

		#region Measure tool

		internal MeasureMode measureMode;

		internal bool measureShowGroundTrack;

		internal int measureLineGroundColor = Color.FromArgb(222,0,255,0).ToArgb();
		internal int measureLineLinearColor = Color.FromArgb(255,255,0,0).ToArgb();

		[XmlIgnore]
		[Browsable(true),Category("UI")]
		[Editor(typeof(ColorEditor),typeof(UITypeEditor))]
		[Description("Color of the linear distance measure line.")]
		public Color MeasureLineLinearColor 
		{
			get { return Color.FromArgb(measureLineLinearColor); }
			set { measureLineLinearColor = value.ToArgb(); }
		}

		[Browsable(false)]
		public int MeasureLineLinearColorXml
		{
			get { return measureLineLinearColor; }
			set { measureLineLinearColor = value; }
		}

		[XmlIgnore]
		[Browsable(true),Category("UI")]
		[Editor(typeof(ColorEditor),typeof(UITypeEditor))]
		[Description("Color of the ground track measure line.")]
		public Color MeasureLineGroundColor 
		{
			get { return Color.FromArgb(measureLineGroundColor); }
			set { measureLineGroundColor = value.ToArgb(); }
		}

		[Browsable(false)]
		public int MeasureLineGroundColorXml
		{
			get { return measureLineGroundColor; }
			set { measureLineGroundColor = value; }
		}

		[Browsable(true),Category("UI")]
		[Description("Display the ground track column in the measurement statistics table.")]
		public bool MeasureShowGroundTrack
		{
			get { return measureShowGroundTrack; }
			set { measureShowGroundTrack = value; }
		}

		[Browsable(true),Category("UI")]
		[Description("Measure tool operation mode.")]
		public MeasureMode MeasureMode
		{
			get { return measureMode; }
			set { measureMode = value; }
		}

		#endregion

		// comment out ToString() to have namespace+class name being used as filename
		public override string ToString() 
		{
			return "World";
		}
  }
}
