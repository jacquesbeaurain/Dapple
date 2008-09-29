using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using WorldWind;
using Dapple.LayerGeneration;
using Geosoft.GX.DAPGetData;

namespace Dapple
{
   public partial class AddDAP : Form
   {
		private static string DEFAULT_TEXT = "http://";

      public AddDAP()
      {
         InitializeComponent();
      }

      public string Url
      {
         get
         {
            return txtDapURL.Text;
         }
      }

      private void linkLabelHelpDAP_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
      {
         MainForm.BrowseTo(MainForm.DAPWebsiteHelpUrl);
      }

      private void AddDAP_Load(object sender, EventArgs e)
      {
         this.txtDapURL.SelectionStart = this.txtDapURL.Text.Length;
      }

      private void butOK_Click(object sender, EventArgs e)
      {
			Uri oServerUrl = null;
			while (txtDapURL.Text.EndsWith("&")) txtDapURL.Text = txtDapURL.Text.Substring(0, txtDapURL.Text.Length - 1);

			if (txtDapURL.Text.Equals(DEFAULT_TEXT, StringComparison.InvariantCultureIgnoreCase))
			{
				Program.ShowMessageBox(
					"Please enter a valid URL.",
					"Add DAP Server",
					MessageBoxButtons.OK,
					MessageBoxDefaultButton.Button1,
					MessageBoxIcon.Error);
				DialogResult = DialogResult.None;
				return;
			}
			if (!(Uri.TryCreate(txtDapURL.Text, UriKind.Absolute, out oServerUrl) || Uri.TryCreate("http://" + txtDapURL.Text, UriKind.Absolute, out oServerUrl)))
			{
				Program.ShowMessageBox(
					"Please enter a valid URL.",
					"Add DAP Server",
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
					"Add DAP Server",
					MessageBoxButtons.OK,
					MessageBoxDefaultButton.Button1,
					MessageBoxIcon.Error);
				DialogResult = DialogResult.None;
				return;
			}

			txtDapURL.Text = oServerUrl.ToString();
      }
   }
}