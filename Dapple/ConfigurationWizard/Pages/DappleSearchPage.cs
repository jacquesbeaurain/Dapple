using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace ConfigurationWizard
{
   internal partial class DappleSearchPage : WizardPage
   {
      internal DappleSearchPage()
      {
         InitializeComponent();
      }

      protected override void OnValidating(System.ComponentModel.CancelEventArgs e)
      {
			// --- Add http:// if necessary --- 

			if (!Uri.IsWellFormedUriString(DappleSearchURLTextbox.Text, UriKind.Absolute) &&
					Uri.IsWellFormedUriString("http://" + DappleSearchURLTextbox.Text, UriKind.Absolute))
			{
				DappleSearchURLTextbox.Text = "http://" + DappleSearchURLTextbox.Text;
			}

			// --- Cancel if the URL isn't well-formed ---

			if (!Uri.IsWellFormedUriString(DappleSearchURLTextbox.Text, UriKind.Absolute))
			{
				MessageBox.Show("The URL entered for DappleSearch is invalid", "Invalid URL", MessageBoxButtons.OK, MessageBoxIcon.Error);
				e.Cancel = true;
				return;
			}

         try
         {
            Wizard.Settings.DappleSearchURL = this.DappleSearchURLTextbox.Text;
				Wizard.Settings.UseDappleSearch = UseDappleSearchCheckBox.Checked;
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
			this.DappleSearchURLTextbox.Enabled = Wizard.Settings.UseDappleSearch;
			this.UseDappleSearchCheckBox.Checked = Wizard.Settings.UseDappleSearch;
      }

      private void UseDappleSearchCheckBox_CheckedChanged(object sender, EventArgs e)
      {
         if (UseDappleSearchCheckBox.Checked)
         {
            DappleSearchURLTextbox.Enabled = true;
         }
         else
         {
            DappleSearchURLTextbox.Enabled = false;
         }
      }
   }
}
