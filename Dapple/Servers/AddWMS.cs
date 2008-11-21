using System;
using System.Windows.Forms;
using WorldWind;
using Dapple.LayerGeneration;

namespace Dapple
{
   public partial class AddWMS : Form
   {
		private static string DEFAULT_TEXT = "http://";

      WMSCatalogBuilder m_oParent;

      public AddWMS(WorldWind.WorldWindow worldWindow, WMSCatalogBuilder oParent)
      {
         m_oParent = oParent;
         InitializeComponent();
      }

      public string WmsURL
      {
         get
         {
            return txtWmsURL.Text;
         }
      }

      private void butOK_Click(object sender, EventArgs e)
      {
         Uri oServerUrl = null;
			while (txtWmsURL.Text.EndsWith("&")) txtWmsURL.Text = txtWmsURL.Text.Substring(0, txtWmsURL.Text.Length - 1);

			if (txtWmsURL.Text.Equals(DEFAULT_TEXT, StringComparison.InvariantCultureIgnoreCase))
			{
				Program.ShowMessageBox(
					"Please enter a valid URL.",
					"Add WMS Server",
					MessageBoxButtons.OK,
					MessageBoxDefaultButton.Button1,
					MessageBoxIcon.Error);
				DialogResult = DialogResult.None;
				return;
			}
			if (!(Uri.TryCreate(txtWmsURL.Text, UriKind.Absolute, out oServerUrl) || Uri.TryCreate("http://" + txtWmsURL.Text, UriKind.Absolute, out oServerUrl)))
			{
				Program.ShowMessageBox(
					"Please enter a valid URL.",
					"Add WMS Server",
					MessageBoxButtons.OK,
					MessageBoxDefaultButton.Button1,
					MessageBoxIcon.Error);
            DialogResult = DialogResult.None;
            return;
         }
         if (m_oParent.ContainsServer(new WMSServerUri(oServerUrl.ToString())))
         {
				Program.ShowMessageBox(
					"The specified server has already been added.",
					"Add WMS Server",
					MessageBoxButtons.OK,
					MessageBoxDefaultButton.Button1,
					MessageBoxIcon.Information);
            DialogResult = DialogResult.None;
            return;
         }

			if (!oServerUrl.Scheme.Equals("http"))
			{
				Program.ShowMessageBox(
					"Only web urls are permitted (must start with \"http://\")",
					"Add WMS Server",
					MessageBoxButtons.OK,
					MessageBoxDefaultButton.Button1,
					MessageBoxIcon.Error);
				DialogResult = DialogResult.None;
				return;
			}

			txtWmsURL.Text = oServerUrl.ToString();
      }

      private void linkLabelHelpWMS_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
      {
         MainForm.BrowseTo(MainForm.WMSWebsiteHelpUrl);
      }

      private void AddWMS_Load(object sender, EventArgs e)
      {
         this.txtWmsURL.SelectionStart = this.txtWmsURL.Text.Length;
      }
   }
}