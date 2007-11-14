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
      WorldWindow m_worldWind;
      ArcIMSCatalogBuilder m_oParent;

      public AddArcIMS(WorldWind.WorldWindow worldWindow, ArcIMSCatalogBuilder oParent)
      {
         m_worldWind = worldWindow;
         m_oParent = oParent;
         InitializeComponent();
         this.Icon = new System.Drawing.Icon(@"app.ico");
      }

      public string URL
      {
         get
         {
				if (txtWmsURL.Text.EndsWith("/servlet/com.esri.esrimap.Esrimap"))
					return txtWmsURL.Text;
				else
					return txtWmsURL.Text + "/servlet/com.esri.esrimap.Esrimap";
         }
      }

      private void butOK_Click(object sender, EventArgs e)
      {
         if (!txtWmsURL.Text.StartsWith("http://") || txtWmsURL.Text.Length <= "http://".Length)
         {
            MessageBox.Show(this, "Please enter a valid URL (including http://)", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            DialogResult = DialogResult.None;
            return;
         }

         if (m_oParent.ContainsServer(new ArcIMSServerUri(txtWmsURL.Text)))
         {
            MessageBox.Show(this, "This ArcIMS Server has already been added", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            DialogResult = DialogResult.None;
            return;
         }        
      }

      private void AddArcIMS_Load(object sender, EventArgs e)
      {
         this.txtWmsURL.SelectionStart = this.txtWmsURL.Text.Length;
      }
   }
}