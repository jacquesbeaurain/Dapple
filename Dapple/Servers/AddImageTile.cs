using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using WorldWind;
using Geosoft.GX.DAPGetData;

namespace Dapple
{
   public partial class AddImageTile : Form
   {
      public enum ServerType
      {
         DapServer = 2,
         WmsServer = 3,
         TileServer = 1
      }

      List<TabPage> m_tabPages = new List<TabPage>();
      int m_intLastTab;
      int m_intCurrentTab;
      WorldWindow m_worldWind;
      Server m_oServer;
      LayerGeneration.QuadLayerBuilder m_oTileServer;
      LayerGeneration.IBuilder m_oParent;
      string m_WmsURL;

      public AddImageTile(WorldWind.WorldWindow worldWindow, LayerGeneration.IBuilder oParent)
      {
         m_worldWind = worldWindow;
         m_oParent = oParent;
         InitializeComponent();

         // collect the pages and leave only the start page
         for (int i = 0; i < tabCtl.TabPages.Count; i++)
         {
            m_tabPages.Add(tabCtl.TabPages[i]);
         }
         tabCtl.TabPages.Clear();
         tabCtl.TabPages.Add(m_tabPages[0]);
      }

      public Server DapServer
      {
         get
         {
            return m_oServer;
         }
      }

      public LayerGeneration.QuadLayerBuilder TileServer
      {
         get
         {
            return m_oTileServer;
         }
      }

      public string WmsURL
      {
         get
         {
            return m_WmsURL;
         }
      }

      private void butCancel_Click(object sender, EventArgs e)
      {
         Close();
      }

      private void butNext_Click(object sender, EventArgs e)
      {
         if (ValidateChildren(ValidationConstraints.Enabled) && Validate())
         {
            // if there are screens to go to....
            if (butNext.Text.StartsWith("&N"))
            {
               tabCtl.TabPages.Clear();
               tabCtl.TabPages.Add(m_tabPages[m_intCurrentTab]);
            }
            // otherwise
            else
            {
               // Make result available and close
               if (m_intCurrentTab == 3)
               {
                  m_oServer = null;
                  m_oTileServer = null;
                  m_WmsURL = txtWmsServeURL.Text;
               }
               else if (m_intCurrentTab == 2)
               {
                  //DAP
                  m_oTileServer = null;
                  m_WmsURL = "";
               }
               else if (m_intCurrentTab == 1)
               {
                  m_oServer = null;
                  m_WmsURL = "";

                  string cacheDir = System.IO.Path.Combine(m_worldWind.WorldWindSettings.CachePath, txtName.Text);
                  ImageTileService imageTileService = new ImageTileService(txtDatabaseName.Text, txtServerURL.Text, txtServerURL.Text);

                  GeographicBoundingBox geoBox = new GeographicBoundingBox(
                     Math.Round(Convert.ToDouble(numN.Value), 0),
                     Math.Round(Convert.ToDouble(numS.Value), 0),
                     Math.Round(Convert.ToDouble(numW.Value), 0),
                     Math.Round(Convert.ToDouble(numE.Value), 0));
                  m_oTileServer = new LayerGeneration.QuadLayerBuilder(
                     txtName.Text,
                     Convert.ToInt32(numHeight.Value),
                     chkTileServerUseTerrainMap.Checked,
                     geoBox, numTileSize.Value, Convert.ToInt32(numLevels.Value), Convert.ToInt32(numImagePixelSize.Value),
                     imageTileService,cmbTileServerFileExtension.Text, Convert.ToByte(chkShowOnAdd.Checked ? 255 : 0),
                     m_worldWind.CurrentWorld, m_worldWind.WorldWindSettings.CachePath, m_worldWind.WorldWindSettings.CachePath, m_oParent);
               }

               Close();
            }
         }
         // Change next to finish for those screens that end the wizard
         if (m_intCurrentTab != 0)
         {
            butNext.Text = "&OK";
         }
      }

      private void butBack_Click(object sender, EventArgs e)
      {
         tabCtl.TabPages.Clear();
         tabCtl.TabPages.Add(m_tabPages[m_intLastTab]);
      }

      public DialogResult ShowDialog(IWin32Window owner, ServerType serverType)
      {
         m_intCurrentTab = (int)serverType;
         tabCtl.TabPages.Clear();
         tabCtl.TabPages.Add(m_tabPages[m_intCurrentTab]);
         this.Text = m_tabPages[m_intCurrentTab].Text;
         butNext.Text = "Finish";
         return base.ShowDialog(owner);
      }

      #region Validating Event Handlers

      void cmbTileServerFileExtension_Validating(object sender, System.ComponentModel.CancelEventArgs e)
      {
         if (cmbTileServerFileExtension.SelectedIndex < 0)
         {
            errProvider.SetError(cmbTileServerFileExtension, "Please choose a file extension from the list");
            
         }
         else
         {
            errProvider.SetError(cmbTileServerFileExtension, string.Empty);
         }
      }

      void cmbTypeSelect_Validating(object sender, System.ComponentModel.CancelEventArgs e)
      {
         if (cmbTypeSelect.SelectedIndex < 0)
         {
            errProvider.SetError(cmbTypeSelect, "Please select an item from the list");
            
            return;
         }
         // TODO: update
         errProvider.SetError(cmbTypeSelect, string.Empty);
         m_intLastTab = 0;
         m_intCurrentTab = cmbTypeSelect.SelectedIndex + 1;
      }

      void txtServerURL_Validating(object sender, System.ComponentModel.CancelEventArgs e)
      {
         if (txtServerURL.Text.StartsWith("http://"))
         {
            errProvider.SetError(txtServerURL, string.Empty);
         }
         else
         {
            errProvider.SetError(txtServerURL, "Please enter a valid URL");
            
            return;
         }
      }

      void txtDatabaseName_Validating(object sender, System.ComponentModel.CancelEventArgs e)
      {
         if (txtDatabaseName.Text.Length > 0)
         {
            errProvider.SetError(txtDatabaseName, string.Empty);
         }
         else
         {
            errProvider.SetError(txtDatabaseName, "A database name must be entered.");
            
         }
      }

      void txtLogoPath_Validating(object sender, System.ComponentModel.CancelEventArgs e)
      {
         if (txtLogoPath.Text.Length > 0 && !System.IO.File.Exists(txtLogoPath.Text))
         {
            errProvider.SetError(txtLogoPath, "If a logo path is to be entered, the path must exist.");
            
            return;
         }
         errProvider.SetError(txtLogoPath, string.Empty);
      }

      void grpExtents_Validating(object sender, System.ComponentModel.CancelEventArgs e)
      {
         if (numN.Value > numS.Value)
         {
            errProvider.SetError(grpExtents, string.Empty);
         }
         else
         {
            errProvider.SetError(grpExtents, "The value in degrees for North must be greater than that for South.");
            
            return;
         }
         if (numE.Value > numW.Value)
         {
            errProvider.SetError(grpExtents, string.Empty);
         }
         else
         {
            errProvider.SetError(grpExtents, "The value in degrees for East must be greater than that for West.");
            
         }
      }

      void txtDapURL_Validating(object sender, System.ComponentModel.CancelEventArgs e)
      {
         if (!txtDapURL.Text.StartsWith("http://"))
         {
            errProvider.SetError(txtDapURL, "The URL you entered must begin with 'http://'");
            
            return;
         }
         // CHECK that the server exists + display info
         Server oServer = new Server(txtDapURL.Text, 
            m_worldWind.WorldWindSettings.CachePath);
         if (oServer.Name == null)
         {
            errProvider.SetError(txtDapURL, "The URL you entered is not a valid DAP Server");
            
            return;
         }
         else
         {
            errProvider.SetError(txtDapURL, string.Empty);
            m_oServer = oServer;
         }
      }

      void txtName_Validating(object sender, System.ComponentModel.CancelEventArgs e)
      {
         if (txtName.Text.Length > 0)
         {
            errProvider.SetError(txtName, string.Empty);
         }
         else
         {
            errProvider.SetError(txtName, "A name must be entered for the Tile Server.");
            
            return;
         }
      }


      void txtWmsServeURL_Validating(object sender, System.ComponentModel.CancelEventArgs e)
      {
         if (txtWmsServeURL.Text.StartsWith("http://"))
         {
            errProvider.SetError(txtWmsServeURL, string.Empty);
         }
         else
         {
            errProvider.SetError(txtWmsServeURL, "Please enter a valid URL");
            
            return;
         }
      }

      #endregion

      private void butLogoPathBrowse_Click(object sender, EventArgs e)
      {

      }

      private void chkTileServerUseTerrainMap_CheckedChanged(object sender, EventArgs e)
      {

      }

      private void cmbTileServerFileExtension_SelectedIndexChanged(object sender, EventArgs e)
      {

      }

      private void txtServerURL_TextChanged(object sender, EventArgs e)
      {

      }

      private void txtDatabaseName_TextChanged(object sender, EventArgs e)
      {

      }

      private void txtLogoPath_TextChanged(object sender, EventArgs e)
      {

      }

      private void butLogoPathBrowse_Click_1(object sender, EventArgs e)
      {

      }

      private void linkLabelHelpTile_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
      {

      }

      private void butOK_Click(object sender, EventArgs e)
      {

      }

      private void butCancel_Click_1(object sender, EventArgs e)
      {

      }
   }
}