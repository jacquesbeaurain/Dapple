using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using WorldWind;
using System.Globalization;

namespace Dapple
{
   internal class UpdateDialog : System.Windows.Forms.Form
   {
      private LinkLabel linkLabelWhatNew;
      private Label labelMessage;
      private Button buttonNo;
      private Button buttonYes;

      /// <summary>
      /// Initializes a new instance of the <see cref= "T:WorldWind.UpdateDialog"/> class.
      /// </summary>
      /// <param name="ww"></param>
      internal UpdateDialog(string strVersion)
      {
         InitializeComponent();
         Icon = new System.Drawing.Icon(@"app.ico");

         this.labelMessage.Text = String.Format(CultureInfo.InvariantCulture, this.labelMessage.Text, strVersion);
      }

      #region Windows Form Designer generated code
      /// <summary>
      /// Required method for Designer support - do not modify
      /// the contents of this method with the code editor.
      /// </summary>
      private void InitializeComponent()
      {
         this.linkLabelWhatNew = new System.Windows.Forms.LinkLabel();
         this.labelMessage = new System.Windows.Forms.Label();
         this.buttonNo = new System.Windows.Forms.Button();
         this.buttonYes = new System.Windows.Forms.Button();
         this.SuspendLayout();
         // 
         // linkLabelWhatNew
         // 
         this.linkLabelWhatNew.AutoSize = true;
         this.linkLabelWhatNew.Location = new System.Drawing.Point(12, 45);
         this.linkLabelWhatNew.Name = "linkLabelWhatNew";
         this.linkLabelWhatNew.Size = new System.Drawing.Size(136, 13);
         this.linkLabelWhatNew.TabIndex = 3;
         this.linkLabelWhatNew.TabStop = true;
         this.linkLabelWhatNew.Text = "What\'s new in this version?";
         this.linkLabelWhatNew.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelWhatNew_LinkClicked);
         // 
         // labelMessage
         // 
         this.labelMessage.AutoSize = true;
         this.labelMessage.Location = new System.Drawing.Point(12, 9);
         this.labelMessage.Name = "labelMessage";
         this.labelMessage.Size = new System.Drawing.Size(341, 26);
         this.labelMessage.TabIndex = 2;
         this.labelMessage.Text = "There is a new update for Dapple (Version {0}) available.\r\nDo you want to visit t" +
             "he Dapple web site to downlad the latest version?";
         // 
         // buttonNo
         // 
         this.buttonNo.DialogResult = System.Windows.Forms.DialogResult.No;
         this.buttonNo.Location = new System.Drawing.Point(287, 75);
         this.buttonNo.Name = "buttonNo";
         this.buttonNo.Size = new System.Drawing.Size(75, 23);
         this.buttonNo.TabIndex = 1;
         this.buttonNo.Text = "No";
         this.buttonNo.UseVisualStyleBackColor = true;
         // 
         // buttonYes
         // 
         this.buttonYes.DialogResult = System.Windows.Forms.DialogResult.Yes;
         this.buttonYes.Location = new System.Drawing.Point(206, 75);
         this.buttonYes.Name = "buttonYes";
         this.buttonYes.Size = new System.Drawing.Size(75, 23);
         this.buttonYes.TabIndex = 0;
         this.buttonYes.Text = "Yes";
         this.buttonYes.UseVisualStyleBackColor = true;
         // 
         // UpdateDialog
         // 
         this.AcceptButton = this.buttonYes;
         this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
         this.CancelButton = this.buttonNo;
         this.ClientSize = new System.Drawing.Size(369, 104);
         this.Controls.Add(this.buttonYes);
         this.Controls.Add(this.buttonNo);
         this.Controls.Add(this.labelMessage);
         this.Controls.Add(this.linkLabelWhatNew);
         this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
         this.KeyPreview = true;
         this.MaximizeBox = false;
         this.MinimizeBox = false;
         this.Name = "UpdateDialog";
         this.ShowInTaskbar = false;
         this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
         this.Text = "Dapple Update Available";
         this.ResumeLayout(false);
         this.PerformLayout();

      }
      #endregion

      private void linkLabelWhatNew_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
      {
         MainForm.BrowseTo(MainForm.ReleaseNotesWebsiteUrl);
      }

   }
}
