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
      public AddDAP()
      {
         InitializeComponent();
         this.Icon = new System.Drawing.Icon(@"app.ico");
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
			if (!(Uri.TryCreate(txtDapURL.Text, UriKind.Absolute, out oServerUrl) || Uri.TryCreate("http://" + txtDapURL.Text, UriKind.Absolute, out oServerUrl)))
			{
				MessageBox.Show(this, "Unable to parse URL.", "Invalid URL", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				DialogResult = DialogResult.None;
				return;
			}
			txtDapURL.Text = oServerUrl.ToString();
      }
   }
}