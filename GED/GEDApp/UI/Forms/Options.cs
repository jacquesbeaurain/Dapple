using System;
using System.IO;
using System.Windows.Forms;
using GED.App.Core;
using GED.App.Properties;
using GED.Core;

namespace GED.App.UI.Forms
{
	public enum SearchProtocol
	{
		DappleSearch,
		DapServer
	}

	public partial class Options : Form
	{
		public Options()
		{
			InitializeComponent();

			c_cbKmlFormat.Items.Add(RootKmlFormat.Tiled);
			c_cbKmlFormat.Items.Add(RootKmlFormat.SingleImage);

			c_cbSearchProviderType.Items.Add(SearchProtocol.DappleSearch);
			c_cbSearchProviderType.Items.Add(SearchProtocol.DapServer);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			c_tbCacheDirectory.Text = CacheUtils.CacheRoot;
			c_cbKmlFormat.SelectedItem = Settings.Default.KmlFormat;

			c_tbSearchProviderURL.Text = Settings.Default.SearchProviderURL;
			c_cbSearchProviderType.SelectedItem= Settings.Default.SearchProviderType;
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
			Settings.Default.SearchProviderType = (SearchProtocol)c_cbSearchProviderType.SelectedItem;
			Settings.Default.SearchProviderURL = c_tbSearchProviderURL.Text;
			Settings.Default.Save();
		}
	}
}
