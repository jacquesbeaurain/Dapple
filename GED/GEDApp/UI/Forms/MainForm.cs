using System;
using System.Windows.Forms;
using EARTHLib;
using GED.App.UI.Controls;
using Geosoft.Dap.Common;
using GED.App.Core;
using GED.App.Properties;

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
			ViewExtentsGE oExtents = GoogleEarth.Instance.ViewExtents;
			dappleSearchList1.SetSearchParameters(textBox1.Text, new BoundingBox(oExtents.East, oExtents.North, oExtents.West, oExtents.South));
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
					GoogleEarth.Instance.OpenKmlFile(strFilename, 1);
				}
			}
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
