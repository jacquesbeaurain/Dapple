using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using WorldWind.Configuration;
using Utility;

namespace WorldWind.PluginEngine
{
	public class MainApplication : Form
   {
      // The value of Release should be set in constructor
		public static string Release;
		public static string CurrentSettingsDirectory;
		public static WorldWindSettings Settings = new WorldWindSettings();
		public static readonly string DirectoryPath = Path.GetDirectoryName(Application.ExecutablePath);

      /// <summary>
      /// MainApplication's System.Windows.Forms.Form
      /// </summary>
		public virtual System.Windows.Forms.Form Form
      {
			get
			{
				return null;
			}
      }

      /// <summary>
      /// MainApplication's globe window
      /// </summary>
		public virtual WorldWindow WorldWindow
      {
			get
			{
				return null;
			}
      }

      /// <summary>
      /// The splash screen dialog.
      /// </summary>
		public virtual Splash SplashScreen
      {
			get
			{
				return null;
			}
      }

      /// <summary>
      /// MainApplication's main menu (drop-down)
      /// </summary>
      internal virtual MainMenu MainMenu
      {
			get
			{
				return null;
			}
      }

      /// <summary>
      /// MainApplication's Tools menu (drop-down)
      /// </summary>
      internal virtual MenuItem ToolsMenu
      {
			get
			{
				return null;
			}
      }

      /// <summary>
      /// MainApplication's View menu (drop-down)
      /// </summary>
      internal virtual MenuItem ViewMenu
      {
			get
			{
				return null;
			}
      }

      /// <summary>
      /// MainApplication's Plugins menu (drop-down)
      /// </summary>
      internal virtual MenuItem PluginsMenu
      {
			get
			{
				return null;
			}
      }


      #region Various loading methods

      /// <summary>
      /// Run at startup to load settings
      /// </summary>
      protected void InitSettings()
      {
         if (CurrentSettingsDirectory == null)
         {
            // load program settings from default directory
            LoadSettings();
            World.LoadSettings();
         }
         else
         {
            LoadSettings(CurrentSettingsDirectory);
            World.LoadSettings(CurrentSettingsDirectory);
         }
      }

      /// <summary>
      /// Run at shutdown to save settings
      /// </summary>
      protected void FinalizeSettings()
      {
         // Save World settings
         World.Settings.Save();

         // Encrypt encoded user credentials before saving program settings
         DataProtector dp = new DataProtector(DataProtector.Store.USE_USER_STORE);
         Settings.ProxyUsername = dp.TransparentEncrypt(Settings.ProxyUsername);
         Settings.ProxyPassword = dp.TransparentEncrypt(Settings.ProxyPassword);

         // Save program settings
         Settings.Save();
      }

      /// <summary>
      /// Deserializes and optionally decrypts settings
      /// </summary>
      protected virtual void LoadSettings()
      {
         try
         {
            Settings = (WorldWindSettings)SettingsBase.Load(Settings, SettingsBase.LocationType.User);

            // decrypt encoded user credentials
            DataProtector dp = new DataProtector(DataProtector.Store.USE_USER_STORE);

            if (Settings.ProxyUsername.Length > 0) Settings.ProxyUsername = dp.TransparentDecrypt(Settings.ProxyUsername);
            if (Settings.ProxyPassword.Length > 0) Settings.ProxyPassword = dp.TransparentDecrypt(Settings.ProxyPassword);
         }
         catch (Exception caught)
         {
            Log.Write(caught);
         }
      }

      /// <summary>
      /// Deserializes and optionally decrypts settings, using specified location
      /// </summary>
      protected virtual void LoadSettings(string directory)
      {
         try
         {
            Settings = (WorldWindSettings)SettingsBase.LoadFromPath(Settings, directory);

            // decrypt encoded user credentials
            DataProtector dp = new DataProtector(DataProtector.Store.USE_USER_STORE);

            if (Settings.ProxyUsername.Length > 0) Settings.ProxyUsername = dp.TransparentDecrypt(Settings.ProxyUsername);
            if (Settings.ProxyPassword.Length > 0) Settings.ProxyPassword = dp.TransparentDecrypt(Settings.ProxyPassword);
         }
         catch (Exception caught)
         {
            Log.Write(caught);
         }
      }
      #endregion
   }
}
