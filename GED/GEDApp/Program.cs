using System;
using System.Threading;
using System.Windows.Forms;
using GED.App.UI.Forms;
using GED.App.Properties;
using GED.App.Core;

namespace GED.App
{
	public static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		public static void Main(String[] args)
		{
			MaintainAndApplyUserSettings();

			GoogleEarth.Init();
			GED.WebService.ControlPanel.Start();

			try
			{
				// --- Show the Google Earth Window ---
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				Application.Run(new MainForm());
			}
			finally
			{
				// --- Deinit program context ---
				GED.WebService.ControlPanel.Stop();
			}
		}

		/// <summary>
		/// Upgrade the user settings for this program from a previous version, and apply
		/// the settings to the application.
		/// </summary>
		private static void MaintainAndApplyUserSettings()
		{
			// --- One of the settings in the user settings is 'UpgradeSettingsRequired',
			// --- with default value true.  Whenever the application runs and there is no
			// --- user settings file present, it is assumed that this is the first run of
			// --- a new version of the application, and so it will attempt to upgrade the
			// --- settings from a previous version. Following that, it will set the flag
			// --- to false and save the settings, so the settings won't get upgraded again
			// --- for this application version.

			if (Settings.Default.UpgradeSettingsRequired)
			{
				Settings.Default.Upgrade();
				Settings.Default.UpgradeSettingsRequired = false;
				Settings.Default.Save();
			}


			// --- Once all the setting migration is taken care of, apply the CachePath
			// --- property to CacheUtils

			if (!String.IsNullOrEmpty(Settings.Default.CustomCachePath))
			{
				GED.Core.CacheUtils.CacheRoot = Settings.Default.CustomCachePath;
			}
		}
	}
}
