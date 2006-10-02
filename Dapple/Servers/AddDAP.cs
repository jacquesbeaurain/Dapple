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
      WorldWindow m_worldWind;
      Server m_oServer;
      DAPCatalogBuilder m_oParent;

      public AddDAP(WorldWind.WorldWindow worldWindow, DAPCatalogBuilder oParent)
      {
         m_worldWind = worldWindow;
         m_oParent = oParent;
         InitializeComponent();
         this.Icon = new System.Drawing.Icon(@"app.ico");
      }

      public Server DapServer
      {
         get
         {
            return m_oServer;
         }
      }


      private void butOK_Click(object sender, EventArgs e)
      {
         if (!txtDapURL.Text.StartsWith("http://") || txtDapURL.Text.Length <= "http://".Length)
         {
            MessageBox.Show(this, "Please enter a valid URL", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            DialogResult = DialogResult.None;
            return;
         }

         Geosoft.GX.DAPGetData.Server oServer = m_oParent.FindServer(txtDapURL.Text);
         if (oServer != null)
         {
            MessageBox.Show(this, "This DAP Server has already been added", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            DialogResult = DialogResult.None;
            return;
         }
         m_oServer = new Server(txtDapURL.Text, m_worldWind.WorldWindSettings.CachePath);

         if (m_oServer.Name == null)
         {
            MessageBox.Show(this, "The URL you entered is not a valid DAP Server", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            DialogResult = DialogResult.None;
            return;
         }
      }

      private void linkLabelHelpDAP_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
      {
         MainForm.BrowseTo(MainForm.DAPWebsiteHelpUrl);
      }
   }
}