using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GED.Core;
using GED.App.Properties;
using GED.App.Core;
using System.IO;

namespace GED.App.UI.Forms
{
	public partial class Options : Form
	{
		public Options()
		{
			InitializeComponent();

			c_cbKmlFormat.Items.Add(RootKmlFormat.Tiled);
			c_cbKmlFormat.Items.Add(RootKmlFormat.SingleImage);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			c_tbCacheDirectory.Text = CacheUtils.CacheRoot;
			c_cbKmlFormat.SelectedItem = Settings.Default.KmlFormat;
		}

		private void c_bOk_Click(object sender, EventArgs e)
		{
			CacheUtils.CacheRoot = Path.GetFullPath(c_tbCacheDirectory.Text);
			if (CacheUtils.IsCacheRootDefault)
			{
				Settings.Default.CustomCachePath = String.Empty;
			}
			else
			{
				Settings.Default.CustomCachePath = c_tbCacheDirectory.Text;
			}
			Settings.Default.KmlFormat = (RootKmlFormat)c_cbKmlFormat.SelectedItem;
			Settings.Default.Save();
		}
	}
}
