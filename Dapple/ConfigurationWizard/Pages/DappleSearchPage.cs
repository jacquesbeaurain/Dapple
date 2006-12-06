using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace ConfigurationWizard
{
   public partial class DappleSearchPage : WizardPage
   {
      public DappleSearchPage()
      {
         InitializeComponent();
      }

      protected override void OnValidating(System.ComponentModel.CancelEventArgs e)
      {
         try
         {
            Wizard.Settings.DappleSearchURL = this.DappleSearchURLTextbox.Text;
         }
         catch (Exception caught)
         {
            MessageBox.Show(caught.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            e.Cancel = true;
         }
      }

      protected override void OnLoad(EventArgs e)
      {
         this.DappleSearchURLTextbox.Text = Wizard.Settings.DappleSearchURL;
         this.DappleSearchURLTextbox.Enabled =
            (Wizard.Settings.DappleSearchURL != null && !Wizard.Settings.DappleSearchURL.Equals(String.Empty));
         this.UseDappleSearchCheckBox.Checked = this.DappleSearchURLTextbox.Enabled;
      }

      private void UseDappleSearchCheckBox_CheckedChanged(object sender, EventArgs e)
      {
         if (UseDappleSearchCheckBox.Checked)
         {
            DappleSearchURLTextbox.Enabled = true;
         }
         else
         {
            DappleSearchURLTextbox.Text = String.Empty;
            DappleSearchURLTextbox.Enabled = false;
         }
      }
   }
}
