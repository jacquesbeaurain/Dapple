using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using WorldWind;
using Dapple.LayerGeneration;
using System.Security.Cryptography.X509Certificates;

namespace Dapple
{
   public partial class AddWMS : Form
   {
      WorldWindow m_worldWind;
      WMSCatalogBuilder m_oParent;

      public AddWMS(WorldWind.WorldWindow worldWindow, WMSCatalogBuilder oParent)
      {
         m_worldWind = worldWindow;
         m_oParent = oParent;
         InitializeComponent();
         this.Icon = new System.Drawing.Icon(@"app.ico");
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
			if (!(Uri.TryCreate(txtWmsURL.Text, UriKind.Absolute, out oServerUrl) || Uri.TryCreate("http://" + txtWmsURL.Text, UriKind.Absolute, out oServerUrl)))
			{
            MessageBox.Show(this, "Unable to parse URL.", "Invalid URL", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            DialogResult = DialogResult.None;
            return;
         }
         if (m_oParent.ContainsServer(new WMSServerUri(oServerUrl.ToString())))
         {
            MessageBox.Show(this, "This WMS Server has already been added", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
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