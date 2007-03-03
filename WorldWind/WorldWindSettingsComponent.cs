using System;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.IO;
using WorldWind;
using WorldWind.Configuration;

namespace WorldWind
{
   public partial class WorldWindSettingsComponent : Component
   {
      private string m_strSettingsPath;
      private WorldWindSettings m_oSettings = new WorldWindSettings();

      public WorldWindSettingsComponent()
      {
         InitializeComponent();
      }

      public WorldWindSettingsComponent(IContainer container)
      {
         container.Add(this);

         InitializeComponent();
      }

      public WorldWindSettings WorldWindSettings
      {
         get
         {
            return m_oSettings;
         }
      }

      public string SettingsPath
      {
         get
         {
            return m_strSettingsPath;
         }
         set
         {
            if (value == null)
               return;
            try
            {
               bool bFirstStartup;

               ConfigPath = System.IO.Path.Combine(value, "Config");
               DataPath = System.IO.Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "Data");

               string fileName = System.IO.Path.Combine(ConfigPath, m_oSettings.DefaultName());
               m_strSettingsPath = value;
               if (System.IO.File.Exists(fileName))
               {
                  m_oSettings = (WorldWindSettings)SettingsBase.LoadFromPath(m_oSettings, ConfigPath);
                  bFirstStartup = m_oSettings.ConfigurationWizardAtStartup;
               }
               else
               {
                  m_oSettings.FileName = fileName;
                  bFirstStartup = true;
               }

               // Force these in Dapple
               ConfigPath = System.IO.Path.Combine(value, "Config");
               DataPath = System.IO.Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "Data");
               
               // These should be in setup on first run 
               if (bFirstStartup)
                  CachePath = Path.Combine(value, "Cache");
            }
            catch (Exception)
            {
            }
         }
      }

      [Browsable(true)]
      [Category("Misc")]
      public TimeSpan CacheCleanupInterval
      {
         get
         {
            return m_oSettings.CacheCleanupInterval;
         }
         set
         {
            m_oSettings.CacheCleanupInterval = value;
         }
      }

      [Browsable(true)]
      [Category("Misc")]
      public string CacheCleanupIntervalXml
      {
         get
         {
            return m_oSettings.CacheCleanupIntervalXml;
         }
         set
         {
            m_oSettings.CacheCleanupIntervalXml = value;
         }
      }

      [Browsable(false)]
      public string CachePath
      {
         get
         {
            if (!Path.IsPathRooted(m_oSettings.CachePath))
               return Path.Combine(SettingsPath, m_oSettings.CachePath);

            return m_oSettings.CachePath;
         }
         set
         {
            m_oSettings.CachePath = value;
         }
      }

      [Browsable(false)]
      public string NewCachePath
      {
         get
         {
            return m_oSettings.NewCachePath;
         }
         set
         {
            m_oSettings.NewCachePath = value;
         }
      }

      [Browsable(true)]
      [Category("Misc")]
      public int CacheSizeGigaBytes
      {
         get
         {
            return m_oSettings.CacheSizeGigaBytes;
         }
         set
         {
            m_oSettings.CacheSizeGigaBytes = value;
         }
      }

      public string ConfigPath
      {
         get
         {
            if (!Path.IsPathRooted(m_oSettings.ConfigPath))
               return Path.Combine(SettingsPath, m_oSettings.ConfigPath);

            return m_oSettings.ConfigPath;
         }
         set
         {
            m_oSettings.ConfigPath = value;
         }
      }
      [Browsable(true)]
      [Category("Misc")]
      public bool ConfigurationWizardAtStartup
      {
         get
         {
            return m_oSettings.ConfigurationWizardAtStartup;
         }
         set
         {
            m_oSettings.ConfigurationWizardAtStartup = value;
         }
      }

      [Browsable(true)]
      [Category("Misc")]
      public DateTime UpdateCheckDate
      {
         get
         {
            return m_oSettings.UpdateCheckDate;
         }
         set
         {
            m_oSettings.UpdateCheckDate = value;
         }
      }

      public string DataPath
      {
         get
         {
            if (!Path.IsPathRooted(m_oSettings.DataPath))
               return Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), m_oSettings.DataPath);

            return m_oSettings.DataPath;
         }
         set
         {
            m_oSettings.DataPath = value;
         }
      }
      [Browsable(true)]
      [Category("Misc")]
      public string DefaultWorld
      {
         get
         {
            return m_oSettings.DefaultWorld;
         }
         set
         {
            m_oSettings.DefaultWorld = value;
         }
      }
      [Browsable(true)]
      [Category("Misc")]
      public string FormatVersion
      {
         get
         {
            return m_oSettings.FormatVersion;
         }
         set
         {
            m_oSettings.FormatVersion = value;
         }
      }

      public System.Collections.ArrayList PluginsLoadedOnStartup
      {
         get
         {
            return m_oSettings.PluginsLoadedOnStartup;
         }
      }
      [Browsable(true)]
      [Category("Misc")]
      public string ProxyPassword
      {
         get
         {
            return m_oSettings.ProxyPassword;
         }
         set
         {
            m_oSettings.ProxyPassword = value;
         }
      }
      [Browsable(true)]
      [Category("Misc")]
      public string ProxyUrl
      {
         get
         {
            return m_oSettings.ProxyUrl;
         }
         set
         {
            m_oSettings.ProxyUrl = value;
         }
      }
      [Browsable(true)]
      [Category("Misc")]
      public string ProxyUsername
      {
         get
         {
            return m_oSettings.ProxyUsername;
         }
         set
         {
            m_oSettings.ProxyUsername = value;
         }
      }
      [Browsable(true)]
      [Category("Misc")]
      public TimeSpan TotalRunTime
      {
         get
         {
            return m_oSettings.TotalRunTime;
         }
         set
         {
            m_oSettings.TotalRunTime = value;
         }
      }
      [Browsable(true)]
      [Category("Misc")]
      public string TotalRunTimeXml
      {
         get
         {
            return m_oSettings.TotalRunTimeXml;
         }
         set
         {
            m_oSettings.TotalRunTimeXml = value;
         }
      }
      [Browsable(true)]
      [Category("Misc")]
      public bool UseDynamicProxy
      {
         get
         {
            return m_oSettings.UseDynamicProxy;
         }
         set
         {
            m_oSettings.UseDynamicProxy = value;
         }
      }
      [Browsable(true)]
      [Category("Misc")]
      public bool UseWindowsDefaultProxy
      {
         get
         {
            return m_oSettings.UseWindowsDefaultProxy;
         }
         set
         {
            m_oSettings.UseWindowsDefaultProxy = value;
         }
      }

      [Browsable(true)]
      [Category("Misc")]
      public Net.WebDownload.HttpProtoVersion UseHTTPProtocol
      {
         get
         {
            return m_oSettings.UseHTTPProtocol;
         }
         set
         {
            m_oSettings.UseHTTPProtocol = value;
         }
      }

      [Browsable(true), Category("Misc")]
      [Description("Load last view on program startup.")]
      public bool AskLastViewAtStartup
      {
         get
         {
            return m_oSettings.AskLastViewAtStartup;
         }
         set
         {
            m_oSettings.AskLastViewAtStartup = value;
         }
      }

      [Browsable(true), Category("Misc")]
      [Description("Load last view on program startup.")]
      public bool LastViewAtStartup
      {
         get
         {
            return m_oSettings.LastViewAtStartup;
         }
         set
         {
            m_oSettings.LastViewAtStartup = value;
         }
      }
      
      public string WorldWindDirectory
      {
         get
         {
            return m_oSettings.WorldWindDirectory;
         }
      }
      
      public string DappleSearchURL
      {
         get
         {
            return m_oSettings.DappleSearchURL;
         }
         set
         {
            m_oSettings.DappleSearchURL = value;
         }
      }

   }
}
