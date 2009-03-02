using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using WorldWind;

namespace Dapple
{
   /// <summary>
   /// World Wind Help->About
   /// </summary>
   internal class AboutDialog : System.Windows.Forms.Form
   {
      private System.Windows.Forms.Button buttonClose;
      private System.Windows.Forms.Label labelVersion;
      private System.Windows.Forms.Label labelVersionNumber;
      private System.Windows.Forms.PictureBox pictureBox;
      private Label label1;
      private Label label2;
      private LinkLabel linkLabelLicense;
      private LinkLabel linkLabelCredits;
      private LinkLabel linkLabelWebSite;
      private System.Windows.Forms.Label labelProductVersion;

      /// <summary>
      /// Initializes a new instance of the <see cref= "T:WorldWind.AboutDialog"/> class.
      /// </summary>
      /// <param name="ww"></param>
      internal AboutDialog()
      {
         InitializeComponent();
         Icon = global::Dapple.Properties.Resources.dapple;

         this.labelVersionNumber.Text = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(4);
      }

      #region Windows Form Designer generated code
      /// <summary>
      /// Required method for Designer support - do not modify
      /// the contents of this method with the code editor.
      /// </summary>
      private void InitializeComponent()
      {
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AboutDialog));
			this.buttonClose = new System.Windows.Forms.Button();
			this.pictureBox = new System.Windows.Forms.PictureBox();
			this.labelVersion = new System.Windows.Forms.Label();
			this.labelVersionNumber = new System.Windows.Forms.Label();
			this.labelProductVersion = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.linkLabelLicense = new System.Windows.Forms.LinkLabel();
			this.linkLabelCredits = new System.Windows.Forms.LinkLabel();
			this.linkLabelWebSite = new System.Windows.Forms.LinkLabel();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
			this.SuspendLayout();
			// 
			// buttonClose
			// 
			this.buttonClose.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonClose.Location = new System.Drawing.Point(267, 458);
			this.buttonClose.Name = "buttonClose";
			this.buttonClose.Size = new System.Drawing.Size(96, 32);
			this.buttonClose.TabIndex = 0;
			this.buttonClose.Text = "Close";
			// 
			// pictureBox
			// 
			this.pictureBox.Cursor = System.Windows.Forms.Cursors.Hand;
			this.pictureBox.ErrorImage = null;
			this.pictureBox.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox.Image")));
			this.pictureBox.InitialImage = null;
			this.pictureBox.Location = new System.Drawing.Point(12, 14);
			this.pictureBox.Name = "pictureBox";
			this.pictureBox.Size = new System.Drawing.Size(350, 370);
			this.pictureBox.TabIndex = 1;
			this.pictureBox.TabStop = false;
			this.pictureBox.Click += new System.EventHandler(this.pictureBox_Click);
			// 
			// labelVersion
			// 
			this.labelVersion.Location = new System.Drawing.Point(12, 387);
			this.labelVersion.Name = "labelVersion";
			this.labelVersion.Size = new System.Drawing.Size(84, 18);
			this.labelVersion.TabIndex = 2;
			this.labelVersion.Text = "Dapple Version:";
			// 
			// labelVersionNumber
			// 
			this.labelVersionNumber.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelVersionNumber.Location = new System.Drawing.Point(102, 387);
			this.labelVersionNumber.Name = "labelVersionNumber";
			this.labelVersionNumber.Size = new System.Drawing.Size(165, 18);
			this.labelVersionNumber.TabIndex = 3;
			// 
			// labelProductVersion
			// 
			this.labelProductVersion.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelProductVersion.Location = new System.Drawing.Point(424, 278);
			this.labelProductVersion.Name = "labelProductVersion";
			this.labelProductVersion.Size = new System.Drawing.Size(96, 24);
			this.labelProductVersion.TabIndex = 6;
			this.labelProductVersion.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 405);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(155, 13);
			this.label1.TabIndex = 8;
			this.label1.Text = "Copyright (C) 2008 Geosoft Inc.";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(12, 426);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(327, 39);
			this.label2.TabIndex = 9;
			this.label2.Text = "Copyright (C) 2001 United States Government as represented by the\r\nAdministrator " +
				 "of the national Aeronautics and Space Administration.\r\nAll rights reserved.";
			// 
			// linkLabelLicense
			// 
			this.linkLabelLicense.AutoSize = true;
			this.linkLabelLicense.Location = new System.Drawing.Point(273, 405);
			this.linkLabelLicense.Name = "linkLabelLicense";
			this.linkLabelLicense.Size = new System.Drawing.Size(44, 13);
			this.linkLabelLicense.TabIndex = 10;
			this.linkLabelLicense.TabStop = true;
			this.linkLabelLicense.Text = "License";
			this.linkLabelLicense.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelLicense_LinkClicked);
			// 
			// linkLabelCredits
			// 
			this.linkLabelCredits.AutoSize = true;
			this.linkLabelCredits.Location = new System.Drawing.Point(323, 405);
			this.linkLabelCredits.Name = "linkLabelCredits";
			this.linkLabelCredits.Size = new System.Drawing.Size(39, 13);
			this.linkLabelCredits.TabIndex = 11;
			this.linkLabelCredits.TabStop = true;
			this.linkLabelCredits.Text = "Credits";
			this.linkLabelCredits.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelCredits_LinkClicked);
			// 
			// linkLabelWebSite
			// 
			this.linkLabelWebSite.AutoSize = true;
			this.linkLabelWebSite.Location = new System.Drawing.Point(231, 387);
			this.linkLabelWebSite.Name = "linkLabelWebSite";
			this.linkLabelWebSite.Size = new System.Drawing.Size(131, 13);
			this.linkLabelWebSite.TabIndex = 12;
			this.linkLabelWebSite.TabStop = true;
			this.linkLabelWebSite.Text = "http://dapple.geosoft.com";
			this.linkLabelWebSite.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelWebSite_LinkClicked);
			// 
			// AboutDialog
			// 
			this.AcceptButton = this.buttonClose;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(374, 499);
			this.Controls.Add(this.linkLabelWebSite);
			this.Controls.Add(this.linkLabelCredits);
			this.Controls.Add(this.linkLabelLicense);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.labelProductVersion);
			this.Controls.Add(this.labelVersionNumber);
			this.Controls.Add(this.labelVersion);
			this.Controls.Add(this.pictureBox);
			this.Controls.Add(this.buttonClose);
			this.Controls.Add(this.label2);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
			this.KeyPreview = true;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "AboutDialog";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "About Dapple";
			((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

      }
      #endregion

      protected override void OnKeyUp(System.Windows.Forms.KeyEventArgs e)
      {
         switch (e.KeyCode)
         {
            case Keys.Escape:
               Close();
               e.Handled = true;
               break;
            case Keys.F4:
               if (e.Modifiers == Keys.Control)
               {
                  Close();
                  e.Handled = true;
               }
               break;
         }

         base.OnKeyUp(e);
      }

      private void pictureBox_Click(object sender, System.EventArgs e)
      {
         MainForm.BrowseTo(MainForm.WebsiteUrl);
      }

      private void linkLabelLicense_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
      {
         MainForm.BrowseTo(MainForm.LicenseWebsiteUrl);
      }

      private void linkLabelCredits_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
      {
         MainForm.BrowseTo(MainForm.CreditsWebsiteUrl);
      }

      private void linkLabelWebSite_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
      {
         MainForm.BrowseTo(MainForm.WebsiteUrl);
      }
   }
}
