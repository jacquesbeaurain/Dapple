using Microsoft.DirectX.Direct3D;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Xml.Serialization;
using WorldWind.Configuration;
using System.Collections;

namespace WorldWind
{

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

      #endregion

      #region UI

      /// <summary>
      /// Display cross-hair symbol on screen
      /// </summary>
      bool showCrosshairs;

      bool showDownloadIndicator = true;
      bool showCompass;

      [Browsable(true), Category("UI")]
      [Description("Show Compass Indicator.")]
      public bool ShowCompass
      {
         get { return showCompass; }
         set { showCompass = value; }
      }

		[Browsable(true), Category("UI")]
      [Description("Display download progress and rectangles.")]
      public bool ShowDownloadIndicator
      {
         get { return showDownloadIndicator; }
         set { showDownloadIndicator = value; }
      }

		[XmlIgnore]
		public Color DownloadProgressColor = Color.FromArgb(50, 255, 0, 0);

      internal Color DownloadTerrainRectangleColor = Color.FromArgb(50, 0, 0, 255);

      [Browsable(true), Category("UI")]
      [Description("Display cross-hair symbol on screen.")]
      public bool ShowCrosshairs
      {
         get { return showCrosshairs; }
         set { showCrosshairs = value; }
      }

      #endregion

      #region Grid

      /// <summary>
      /// Display the latitude/longitude grid
      /// </summary>
      bool showLatLonLines;

      [Browsable(true), Category("Grid Lines")]
      [Description("Display the latitude/longitude grid.")]
      public bool ShowLatLonLines
      {
         get { return showLatLonLines; }
         set { showLatLonLines = value; }
      }

      #endregion

      #region World

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

      Angle cameraLatitude = Angle.FromDegrees(0.0);
      Angle cameraLongitude = Angle.FromDegrees(0.0);
      double cameraAltitudeMeters = 20000000;
      Angle cameraHeading = Angle.FromDegrees(0.0);
      Angle cameraTilt = Angle.FromDegrees(0.0);

      bool cameraSmooth = true;

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
      [Description("Smooth camera movement.")]
      public bool CameraSmooth
      {
         get { return cameraSmooth; }
         set { cameraSmooth = value; }
      }

		[XmlIgnore]
		public const float CameraSlerpInertia = 0.25f;

      internal Angle CameraFov = Angle.FromRadians(Math.PI * 0.25f);

      internal Angle CameraFovMin = Angle.FromDegrees(5);

      internal Angle CameraFovMax = Angle.FromDegrees(150);

		[XmlIgnore]
		public const float CameraRotationSpeed = 3.5f;

      #endregion

      #region 3D

		[XmlIgnore]
		public const Format TextureFormat = Format.Dxt3;

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

      internal Color ShadingAmbientColor = Color.FromArgb(50, 50, 50);

		[XmlIgnore]
		public Color StandardAmbientColor = Color.FromArgb(64, 64, 64);

      #endregion

      #region Terrain

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

      #region Layers

      internal ArrayList LoadedLayers = new ArrayList();

      #endregion

      // comment out ToString() to have namespace+class name being used as filename
      public override string ToString()
      {
         return "World";
      }
   }
}
