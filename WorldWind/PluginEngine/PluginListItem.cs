using System;
using System.Windows.Forms;

namespace WorldWind.PluginEngine
{
	/// <summary>
	/// Listview item in the plugin dialog.
	/// </summary>
	internal class PluginListItem : ListViewItem
	{
		PluginInfo pluginInfo;

		/// <summary>
		/// Plugin information container.
		/// </summary>
		internal PluginInfo PluginInfo
		{
			get
			{
				return pluginInfo;
			}
		}

		/// <summary>
		/// Plugin name.
		/// </summary>
		internal new string Name
		{
			get
			{
				return pluginInfo.Name;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.PluginEngine.PluginListItem"/> class.
		/// </summary>
		internal PluginListItem(PluginInfo pi)
		{
			this.pluginInfo = pi;
			this.Text = pi.Name;	
			this.Checked = pi.IsLoadedAtStartup;
		}
	}
}
