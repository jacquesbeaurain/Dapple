using System;
using System.Windows.Forms;
using EARTHLib;
using GED.App.Core;
using GED.App.UI.Controls;
using GED.Core;
using Geosoft.Dap.Common;

namespace GED.App.UI.Forms
{
	internal partial class MainForm : Form
	{
		#region Constructors

		public MainForm()
		{
			InitializeComponent();
		}

		#endregion


		#region Event Handlers

		private void c_bSearch_Click(object sender, EventArgs e)
		{
			dappleSearchList1.SetSearchParameters(textBox1.Text, GoogleEarth.ViewedExtents);
		}

		private void textBox1_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				c_bSearch_Click(this, EventArgs.Empty);
			}
		}

		private void dappleSearchList1_LayerAddRequested(object sender, DappleSearchList.LayerAddArgs e)
		{
			foreach (SearchResult oResult in e.Layers)
			{
				if (oResult.ServerType.Equals("DAP", StringComparison.InvariantCultureIgnoreCase))
				{
					String strFilename = RootKmlFiles.CreateKmlFile(oResult);
					GoogleEarth.OpenKmlFile(strFilename, true);
				}
			}
		}

		private void c_miRefreshView_Click(object sender, EventArgs e)
		{
			GoogleEarth.TriggerRefresh();
		}

		private void exitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Options oOptions = new Options();
			oOptions.ShowDialog();
		}

		#endregion
	}
}
