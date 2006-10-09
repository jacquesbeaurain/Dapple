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
   }
}