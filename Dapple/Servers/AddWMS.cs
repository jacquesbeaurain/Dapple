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
   public partial class AddWMS : Form
   {
      WorldWindow m_worldWind;
      WMSCatalogBuilder m_oParent;
      string m_WmsURL;

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
            return m_WmsURL;
         }
      }

      private void butOK_Click(object sender, EventArgs e)
      {
         m_WmsURL = txtWmsURL.Text;
         WMSCatalogBuilder.TrimCapabilitiesURL(ref m_WmsURL);

         if (!txtWmsURL.Text.StartsWith("http://") || txtWmsURL.Text.Length <= "http://".Length)
         {
            MessageBox.Show(this, "Please enter a valid URL", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            DialogResult = DialogResult.None;
            return;
         }

         WMSList oServer = m_oParent.FindServer(m_WmsURL);
         if (oServer != null)
         {
            MessageBox.Show(this, "This WMS Server has already been added", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            DialogResult = DialogResult.None;
            return;
         }        
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