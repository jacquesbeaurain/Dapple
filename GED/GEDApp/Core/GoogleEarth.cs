using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EARTHLib;

namespace GED.App.Core
{
	static class GoogleEarth
	{
		private static ApplicationGEClass s_oGEClass;

		public static ApplicationGEClass Instance
		{
			get { return s_oGEClass; }
		}

		public static void Init()
		{
			s_oGEClass = new ApplicationGEClass();
		}

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
