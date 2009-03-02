using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using WorldWind;

namespace Dapple.CustomControls
{
	internal partial class Overview : UserControl
	{
		#region Member Variables

		private WorldWind.OverviewControl c_Overview;

		#endregion

		#region Events

		internal delegate void AOISelectedDelegate(Object sender, GeographicBoundingBox bounds);
		internal event AOISelectedDelegate AOISelected;

		#endregion

		#region Constructor

		internal Overview()
		{
			InitializeComponent();

			c_Overview = new OverviewControl(MainForm.Settings.DataPath + @"\Earth\BmngBathy\world.topo.bathy.200407.jpg", MainForm.WorldWindowSingleton, c_pOverview);
			c_Overview.Dock = DockStyle.Fill;
			c_Overview.TabStop = false;
			c_pOverview.Controls.Add(c_Overview);
		}

		#endregion

		#region Public Methods

		internal void SetAOIList(List<KeyValuePair<String, GeographicBoundingBox>> oNewList)
		{
			c_cbAOIs.BeginUpdate();
			c_cbAOIs.Items.Clear();

			foreach (KeyValuePair<String, GeographicBoundingBox> oAOI in oNewList)
			{
				c_cbAOIs.Items.Add(oAOI);
			}

			c_cbAOIs.SelectedIndex = 0;
			c_cbAOIs.EndUpdate();
		}

		internal void StartRenderTimer()
		{
			c_Overview.StartTimer();
		}

		#endregion

		#region Event Handlers

		private void c_cbAOIs_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (AOISelected != null && ((KeyValuePair<String, GeographicBoundingBox>)c_cbAOIs.SelectedItem).Value != null)
			{
				AOISelected(this, ((KeyValuePair<String, GeographicBoundingBox>)c_cbAOIs.SelectedItem).Value);
			}
		}

		private void c_pOverview_Resize(object sender, EventArgs e)
		{
			if (c_Overview != null)
			{
				c_Overview.Refresh();
			}
		}

		#endregion
	}
}
