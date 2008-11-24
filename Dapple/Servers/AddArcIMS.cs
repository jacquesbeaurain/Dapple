using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using WorldWind;
using Dapple.LayerGeneration;

namespace Dapple
{
   public partial class AddArcIMS : Form
   {
		private static string DEFAULT_TEXT = "http://";

      public const String DEFAULT_ARCIMS_PATH = "/servlet/com.esri.esrimap.Esrimap";

      public AddArcIMS()
      {
         InitializeComponent();
      }

      public string URL
      {
         get
         {
				return txtArcIMSURL.Text;
         }
      }

      private void butOK_Click(object sender, EventArgs e)
      {
         Uri oServerUrl = null;
			while (txtArcIMSURL.Text.EndsWith("&")) txtArcIMSURL.Text = txtArcIMSURL.Text.Substring(0, txtArcIMSURL.Text.Length - 1);

			if (txtArcIMSURL.Text.Equals(DEFAULT_TEXT, StringComparison.InvariantCultureIgnoreCase))
			{
				Program.ShowMessageBox(
					"Please enter a valid URL.",
					"Add ArcIMS Server",
					MessageBoxButtons.OK,
					MessageBoxDefaultButton.Button1,
					MessageBoxIcon.Error);
				DialogResult = DialogResult.None;
				return;
			}
			if (!(Uri.TryCreate(txtArcIMSURL.Text, UriKind.Absolute, out oServerUrl) || Uri.TryCreate("http://" + txtArcIMSURL.Text, UriKind.Absolute, out oServerUrl)))
			{
				Program.ShowMessageBox(
					"Please enter a valid URL.",
					"Add ArcIMS Server",
					MessageBoxButtons.OK,
					MessageBoxDefaultButton.Button1,
					MessageBoxIcon.Error);
				DialogResult = DialogResult.None;
				return;
			}

			if (!oServerUrl.Scheme.Equals("http"))
			{
				Program.ShowMessageBox(
					"Only web urls are permitted (must start with \"http://\")",
					"Add ArcIMS Server",
					MessageBoxButtons.OK,
					MessageBoxDefaultButton.Button1,
					MessageBoxIcon.Error);
				DialogResult = DialogResult.None;
				return;
			}

			if (oServerUrl.AbsolutePath.Equals("/"))
			{
				String szUpdatedPath = oServerUrl.ToString();
				szUpdatedPath = szUpdatedPath.Substring(0, szUpdatedPath.Length - 1);
				oServerUrl = new Uri(szUpdatedPath + DEFAULT_ARCIMS_PATH);
			}

			txtArcIMSURL.Text = oServerUrl.ToString();
      }

      private void AddArcIMS_Load(object sender, EventArgs e)
      {
         this.txtArcIMSURL.SelectionStart = this.txtArcIMSURL.Text.Length;
      }
   }
}