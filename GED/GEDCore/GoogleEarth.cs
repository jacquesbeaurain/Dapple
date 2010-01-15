using System;
using EARTHLib;
using Geosoft.Dap.Common;

namespace GED.Core
{
	/// <summary>
	/// Contains static methods to interact with the Google Earth COM API.
	/// </summary>
	public static class GoogleEarth
	{
		private static ApplicationGEClass s_oGEClass;

		/// <summary>
		/// Initializes the Google Earth interface.
		/// </summary>
		public static void Init()
		{
			s_oGEClass = new ApplicationGEClass();
		}

		/// <summary>
		/// The current view extents inside of Google Earth.
		/// </summary>
		public static BoundingBox ViewedExtents
		{
			get
			{
				if (s_oGEClass == null)
					return new BoundingBox(179, 89, -179, -89);

				ViewExtentsGE extents = s_oGEClass.ViewExtents;
				return new BoundingBox(extents.East, extents.North, extents.West, extents.South);
			}
		}

		/// <summary>
		/// Instruct Google Earth to open a KML file.
		/// </summary>
		/// <param name="filename">The filename to open.</param>
		/// <param name="suppressMessages">Whether to suppress messages. Check GE API docs for possible meaning.</param>
		public static void OpenKmlFile(String filename, bool suppressMessages)
		{
			s_oGEClass.OpenKmlFile(filename, suppressMessages ? 1 : 0);
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
