using System;
using EARTHLib;

namespace GED.Core
{
	/// <summary>
	/// Contains static methods to interact with the Google Earth COM API.
	/// </summary>
	public static class GoogleEarth
	{
		private static ApplicationGEClass s_oGEClass;

		/// <summary>
		/// Gets the currently running instance of Google Earth.
		/// </summary>
		public static ApplicationGEClass Instance
		{
			get { return s_oGEClass; }
		}

		/// <summary>
		/// Initializes the Google Earth interface.
		/// </summary>
		public static void Init()
		{
			s_oGEClass = new ApplicationGEClass();
		}

		/// <summary>
		/// Performs a miniscule translation of the Google Earth camera. This will cause any
		/// features which refresh on view changes to refresh.
		/// </summary>
		public static void TriggerRefresh()
		{
			CameraInfoGE oCamera = s_oGEClass.GetCamera(Convert.ToInt32(false));
			CameraInfoGE oNudgedCamera = new CameraInfoGEClass();
			oNudgedCamera.Azimuth = oCamera.Azimuth;
			oNudgedCamera.FocusPointAltitude = oCamera.FocusPointAltitude;
			oNudgedCamera.FocusPointAltitudeMode = oCamera.FocusPointAltitudeMode;
			oNudgedCamera.FocusPointLatitude = oCamera.FocusPointLatitude;
			oNudgedCamera.FocusPointLongitude = oCamera.FocusPointLongitude;
			oNudgedCamera.Range = oCamera.Range - 0.0001;
			oNudgedCamera.Tilt = oCamera.Tilt;

			s_oGEClass.SetCamera(oNudgedCamera, 100000.0);
		}
	}
}
