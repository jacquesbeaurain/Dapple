using System;
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;
using System.Windows.Forms;

namespace WorldWind.Configuration
{
	public class SettingsBase
	{
		private string m_fileName; // location where settings will be stored / were loaded from
		// Filename property, do not serialize
		[XmlIgnore]
		[Browsable(false)]
		public string FileName 
		{
			get { return m_fileName; }
			set { m_fileName = value; }
		}

		private string m_formatVersion; // Version of application that created file
		[Browsable(false)]
		public string FormatVersion 
		{
			get { return m_formatVersion; }
			set { m_formatVersion = value; }
		}

		// types of location supported
		public enum LocationType 
		{
			User = 0,       // regular, roaming user - settings will move
			UserLocal,      // local user - settings will be stored on local machine
			UserCommon,     // location common to all users
			Application,    // application - settings will be saved in appdir
		}


		// Return the default filename (without path) to be used when saving
		// this class's data(e.g. via serialization).
		// Always add the ".xml" file extension.
		// If ToString is not overridden, the default filename will be the
		// class name.
		public string DefaultName()
		{
			return String.Format("{0}.xml", this.ToString());
		}

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.Configuration.SettingsBase"/> class.
		/// A default constructor is required for serialization.
		/// </summary>
		public SettingsBase()
		{
			// experimental: store app version
			m_formatVersion = Application.ProductVersion;
		}


		// Save settings to XML file given specifically by name
		// Note: the FileName property will stay unchanged
		public virtual void Save(string fileName) 
		{
			XmlSerializer ser = null;

			try 
			{
				ser = new XmlSerializer(this.GetType());
				using(TextWriter tw = new StreamWriter(fileName)) 
				{
					ser.Serialize(tw, this);
				}
			}
			catch(Exception ex) 
			{
				throw new System.Exception(String.Format("Saving settings class '{0}' to {1} failed", this.GetType().ToString(), fileName), ex);
			}
		}

		// Save to default name
		public virtual void Save()
		{
			try
			{
				Save(m_fileName);
			}
			catch(Exception caught)
			{
				Utility.Log.Write(caught);
			}
		}

		// load settings from a given file (full path and name)
		public static SettingsBase Load(SettingsBase defaultSettings, string fileName) 
		{
			// remember where we loaded from for a later save
			defaultSettings.m_fileName = fileName;

			// return the default instance if the file does not exist
			if(!File.Exists(fileName)) 
			{
				return defaultSettings;
			}

			// start out with the default instance
			SettingsBase settings = defaultSettings;
			try 
			{
				XmlSerializer ser = new XmlSerializer(defaultSettings.GetType());

				using(TextReader tr = new StreamReader(fileName)) 
				{
					settings = (SettingsBase)ser.Deserialize(tr);
					settings.m_fileName = fileName; // remember where we loaded from for a later save
				}
			}
			catch(Exception ex) 
			{
				throw new System.Exception(String.Format("Loading settings from file '{1}' to {0} failed", 
					defaultSettings.GetType().ToString(), fileName), ex);
			}
         
			return settings;
		}

		// Load settings from specified location using specified path and default filename
		public static SettingsBase LoadFromPath(SettingsBase defaultSettings, string path)
		{
			string fileName = Path.Combine(path, defaultSettings.DefaultName());
			return Load(defaultSettings, fileName);
		}

		public string SettingsFilePath
		{
			get
			{
				return m_fileName;
			}
		}	
	}
}

