using System;
using System.ComponentModel;
using System.Net;
using System.Globalization;
using System.Windows.Forms;
using System.IO;
using System.Configuration;
using System.Xml;
using System.Xml.Serialization;
using WorldWind.Configuration;
using WorldWind;

namespace WorldWind
{
	/// <summary>
	/// World Wind persisted settings.
	/// </summary>
   public class WorldWindSettings : WorldWind.Configuration.SettingsBase
	{
		public WorldWindSettings() : base()
		{
			
		}
		#region Proxy

		// Proxy settings
      private Net.WebDownload.HttpProtoVersion useHttpProtoVersion = Net.WebDownload.HttpProtoVersion.HTTP1_1;
		private bool useWindowsDefaultProxy = true;
		private string proxyUrl = "";
		private bool useDynamicProxy = false;
		private string proxyUsername = "";
		private string proxyPassword = "";

      [Browsable(true), Category("Proxy")]
      [Description("Which HTTP protocol to use in communicating with servers.")]
      public Net.WebDownload.HttpProtoVersion UseHTTPProtocol
      {
         get
         {
            return useHttpProtoVersion;
         }
         set
         {
            this.useHttpProtoVersion = value;
            UpdateProxySettings();
         }
      }


		[Browsable(true),Category("Proxy")]
		[Description("Whether to use Internet Explorer proxy settings (disables url).")]
		public bool UseWindowsDefaultProxy
		{
			get
			{
				return useWindowsDefaultProxy;
			}
			set
			{
				this.useWindowsDefaultProxy = value;
				UpdateProxySettings();
			}
		}

		[Browsable(true),Category("Proxy")]
		[Description("The address of the proxy, or a proxy script if UseDynamicProxy is enabled.")]
		public string ProxyUrl
		{
			get
			{
				return proxyUrl;
			}
			set
			{
				this.proxyUrl = value;
				UpdateProxySettings();
			}
		}

		[Browsable(true),Category("Proxy")]
		[Description("Select if your proxy is determined by a script. If you leave ProxyScriptUrl empty, autodiscovery will be attempted")]
		public bool UseDynamicProxy
		{
			get 
			{
				return this.useDynamicProxy;
			}
			set 
			{
				this.useDynamicProxy = value;
				UpdateProxySettings();
			}         
		}

		[Browsable(true),Category("Proxy")]
		[Description("The user name to use if your proxy requires authentication")]
		public string ProxyUsername
		{
			get 
			{
				return this.proxyUsername;
			}
			set 
			{
				this.proxyUsername = value;
				UpdateProxySettings();
			}         
		}

		[Browsable(true),Category("Proxy")]
		[Description("The password to use if your proxy requires authentication")]
		public string ProxyPassword
		{
			get 
			{
				return this.proxyPassword;
			}
			set 
			{
				this.proxyPassword = value;
				UpdateProxySettings();
			}         
		}

		#endregion

		#region Cache

		// Cache settings
		private string cachePath = "";
      private string newCachePath = "";
		private int cacheSizeGigaBytes = 10;
		private TimeSpan cacheCleanupInterval = TimeSpan.FromMinutes(60);
		public static readonly DateTime ApplicationStartTime = DateTime.Now;
		private TimeSpan totalRunTime = TimeSpan.Zero;

		[Browsable(false),Category("Cache")]
		[Description("Directory to use for caching Image and Terrain files.")]
		public string CachePath
		{
			get
			{
				return cachePath;
			}
			set
			{
				cachePath = value;
			}
		}

      [Browsable(false), Category("Cache")]
      [Description("Directory to use for caching Image and Terrain files.")]
      public string NewCachePath
      {
         get
         {
            return newCachePath;
         }
         set
         {
            newCachePath = value;
         }
      }

		[Browsable(true),Category("Cache")]
		[Description("Upper limit for amount of disk space to allow cache to use (GigaBytes).")]
		public int CacheSizeGigaBytes
		{
			get
			{
				return cacheSizeGigaBytes;
			}
			set
			{
				cacheSizeGigaBytes = value;
			}
		}

		[Browsable(true),Category("Cache")]
		[Description("Controls the frequency of cache cleanup.")]
		[XmlIgnore]
		public TimeSpan CacheCleanupInterval
		{
			get
			{
				return cacheCleanupInterval;
			}
			set
			{
				TimeSpan minimum = TimeSpan.FromMinutes(1);
				if(value < minimum)
					value = minimum;
				cacheCleanupInterval = value;
			}
		}

		/// <summary>
		/// Because Microsoft forgot to implement TimeSpan in their xml serializer.
		/// </summary>
		[Browsable(false)]
		[XmlElement("CacheCleanupInterval", DataType="duration")] 
		public string CacheCleanupIntervalXml 
		{     
			get     
			{         
				if(cacheCleanupInterval < TimeSpan.FromSeconds(1) )
					return null;

				return XmlConvert.ToString(cacheCleanupInterval);         
			}     
			set     
			{
				if(String.IsNullOrEmpty(value))
					return;         

				cacheCleanupInterval = XmlConvert.ToTimeSpan(value);
			} 
		} 

		[Browsable(true),Category("Cache")]
		[Description("Total amount of time the application has been running.")]
		[XmlIgnore]
		public TimeSpan TotalRunTime
		{
			get
			{
				return totalRunTime + DateTime.Now.Subtract(ApplicationStartTime);
			}
			set
			{
				value = totalRunTime;
			}
		}

		/// <summary>
		/// Because Microsoft forgot to implement TimeSpan in their xml serializer.
		/// </summary>
		[Browsable(false)]
		[XmlElement("TotalRunTime", DataType="duration")] 
		public string TotalRunTimeXml 
		{     
			get     
			{         
				if(TotalRunTime < TimeSpan.FromSeconds(1) )
					return null;

				return XmlConvert.ToString(TotalRunTime);         
			}     
			set     
			{         
				if(String.IsNullOrEmpty(value))
					return;         

				totalRunTime = XmlConvert.ToTimeSpan(value);
			} 
		} 

		#endregion

		#region Plugin

		private System.Collections.ArrayList pluginsLoadedOnStartup = new System.Collections.ArrayList();

		[Browsable(true),Category("Plugin")]
		[Description("List of plugins loaded at startup.")]
		public System.Collections.ArrayList PluginsLoadedOnStartup
		{
			get
			{
				return pluginsLoadedOnStartup;
			}
		}

		#endregion

      #region DappleSearch settings
      private String dappleSearchURL = "http://dapplesearch.geosoft.com/";

      public string DappleSearchURL
      {
         get
         {
            return dappleSearchURL;
         }
         set
         {
            dappleSearchURL = value;

            if (dappleSearchURL.Trim().Equals(String.Empty)) dappleSearchURL = String.Empty;

            if (!Uri.IsWellFormedUriString(dappleSearchURL, UriKind.Absolute))
            {
               dappleSearchURL = String.Empty;
            }
            else
            {
               Uri parse = new Uri(dappleSearchURL);
               dappleSearchURL = "http://" + parse.Authority + "/";
            }
         }
      }
      #endregion

      #region Miscellaneous settings

      public bool askLastViewAtStartup = true;
      public bool lastViewAtStartup = true;

		// Misc
		private string defaultWorld = "Earth";
		// default is to show the Configuration Wizard at startup
		private bool configurationWizardAtStartup = true;
      private DateTime updateCheckDate = DateTime.MinValue;

		[Browsable(true),Category("Miscellaneous")]
		[Description("World to load on startup.")]
		public string DefaultWorld
		{
			get
			{
				return defaultWorld;
			}
			set
			{
				defaultWorld = value;
			}
		}

		[Browsable(true),Category("Miscellaneous")]
		[Description("Show Configuration Wizard on program startup.")]
      public bool ConfigurationWizardAtStartup
      {
         get
         {
            return configurationWizardAtStartup;
         }
         set
         {
            configurationWizardAtStartup = value;
         }
      }

       [Browsable(true), Category("Miscellaneous")]
       [Description("Update check date. Wil check the website for update only once per day, store last check time.")]
      public DateTime UpdateCheckDate
       {
           get
           {
              return updateCheckDate;
           }
           set
           {
              updateCheckDate = value;
           }
       }

      [Browsable(true), Category("Miscellaneous")]
      [Description("Load last view on program startup.")]
      public bool AskLastViewAtStartup
      {
         get
         {
            return askLastViewAtStartup;
         }
         set
         {
            askLastViewAtStartup = value;
         }
      }

      [Browsable(true), Category("Miscellaneous")]
      [Description("Load last view on program startup.")]
      public bool LastViewAtStartup
      {
         get
         {
            return lastViewAtStartup;
         }
         set
         {
            lastViewAtStartup = value;
         }
      }
		#endregion

		#region File System Settings

		// File system settings
		private string configPath = "Config";
		private string dataPath = "Data";
		
		[Browsable(true),Category("FileSystem")]
		[Description("Location where configuration files are stored.")]
		public string ConfigPath
		{
			get
			{
				return configPath;
			}
			set
			{
				configPath = value;
			}
		}

		[Browsable(true),Category("FileSystem")]
		[Description("Location where data files are stored.")]
		public string DataPath
		{
			get
			{
				return dataPath;
			}
			set
			{
				dataPath = value;
			}
		}

		/// <summary>
		/// World Wind application base directory ("C:\Program Files\NASA\Worldwind v1.2\") 
		/// </summary>
		public readonly string WorldWindDirectory = Path.GetDirectoryName(Application.ExecutablePath);

		#endregion

		/// <summary>
		/// Propagate proxy-related settings to statics in WebDownload class
		/// </summary>
		void UpdateProxySettings()
		{
         WorldWind.Net.WebDownload.useProto               = this.useHttpProtoVersion;
			WorldWind.Net.WebDownload.useWindowsDefaultProxy = this.useWindowsDefaultProxy;
			WorldWind.Net.WebDownload.useDynamicProxy        = this.useDynamicProxy;
			WorldWind.Net.WebDownload.proxyUrl               = this.proxyUrl;
			WorldWind.Net.WebDownload.proxyUserName          = this.proxyUsername;
			WorldWind.Net.WebDownload.proxyPassword          = this.proxyPassword;
		}

		// comment out ToString() to have namespace+class name being used as filename
		public override string ToString()
		{
			return "WorldWind";
		}
  }
}
