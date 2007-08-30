namespace Dapple.Extract
{
   partial class Picture
   {
      /// <summary> 
      /// Required designer variable.
      /// </summary>
      private System.ComponentModel.IContainer components = null;

      /// <summary> 
      /// Clean up any resources being used.
      /// </summary>
      /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
      protected override void Dispose(bool disposing)
      {
         if (disposing && (components != null))
         {
            components.Dispose();
         }
         base.Dispose(disposing);
      }

      #region Component Designer generated code

      /// <summary> 
      /// Required method for Designer support - do not modify 
      /// the contents of this method with the code editor.
      /// </summary>
      private void InitializeComponent()
      {
         this.lFileName = new System.Windows.Forms.Label();
         this.tbFilename = new System.Windows.Forms.TextBox();
         this.lOptions = new System.Windows.Forms.Label();
         this.cbDownloadOptions = new System.Windows.Forms.ComboBox();
         this.oResolution = new Dapple.Extract.Resolution();
         this.lDisplayOptions = new System.Windows.Forms.Label();
         this.cbDisplayOptions = new System.Windows.Forms.ComboBox();
         this.SuspendLayout();
         // 
         // lFileName
         // 
         this.lFileName.AutoSize = true;
         this.lFileName.Location = new System.Drawing.Point(3, 6);
         this.lFileName.Name = "lFileName";
         this.lFileName.Size = new System.Drawing.Size(55, 13);
         this.lFileName.TabIndex = 0;
         this.lFileName.Text = "File name:";
         // 
         // tbFilename
         // 
         this.tbFilename.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                     | System.Windows.Forms.AnchorStyles.Right)));
         this.tbFilename.Location = new System.Drawing.Point(95, 3);
         this.tbFilename.Name = "tbFilename";
         this.tbFilename.Size = new System.Drawing.Size(196, 20);
         this.tbFilename.TabIndex = 1;
         // 
         // lOptions
         // 
         this.lOptions.AutoSize = true;
         this.lOptions.Location = new System.Drawing.Point(3, 150);
         this.lOptions.Name = "lOptions";
         this.lOptions.Size = new System.Drawing.Size(95, 13);
         this.lOptions.TabIndex = 5;
         this.lOptions.Text = "Download options:";
         // 
         // cbDownloadOptions
         // 
         this.cbDownloadOptions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                     | System.Windows.Forms.AnchorStyles.Right)));
         this.cbDownloadOptions.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
         this.cbDownloadOptions.FormattingEnabled = true;
         this.cbDownloadOptions.Items.AddRange(new object[] {
            "PNG",
            "JPG",
            "TIFF",
            "Native",
            "Default"});
         this.cbDownloadOptions.Location = new System.Drawing.Point(95, 147);
         this.cbDownloadOptions.Name = "cbDownloadOptions";
         this.cbDownloadOptions.Size = new System.Drawing.Size(196, 21);
         this.cbDownloadOptions.TabIndex = 6;
         this.cbDownloadOptions.SelectedIndexChanged += new System.EventHandler(this.cbDownloadOptions_SelectedIndexChanged);
         // 
         // oResolution
         // 
         this.oResolution.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                     | System.Windows.Forms.AnchorStyles.Right)));
         this.oResolution.Location = new System.Drawing.Point(6, 29);
         this.oResolution.Name = "oResolution";
         this.oResolution.Size = new System.Drawing.Size(285, 112);
         this.oResolution.TabIndex = 4;
         // 
         // lDisplayOptions
         // 
         this.lDisplayOptions.AutoSize = true;
         this.lDisplayOptions.Location = new System.Drawing.Point(3, 180);
         this.lDisplayOptions.Name = "lDisplayOptions";
         this.lDisplayOptions.Size = new System.Drawing.Size(81, 13);
         this.lDisplayOptions.TabIndex = 7;
         this.lDisplayOptions.Text = "Display options:";
         // 
         // cbDisplayOptions
         // 
         this.cbDisplayOptions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                     | System.Windows.Forms.AnchorStyles.Right)));
         this.cbDisplayOptions.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
         this.cbDisplayOptions.FormattingEnabled = true;
         this.cbDisplayOptions.Items.AddRange(new object[] {
            "Download and display",
            "Do not display"});
         this.cbDisplayOptions.Location = new System.Drawing.Point(95, 174);
         this.cbDisplayOptions.Name = "cbDisplayOptions";
         this.cbDisplayOptions.Size = new System.Drawing.Size(195, 21);
         this.cbDisplayOptions.TabIndex = 8;
         // 
         // Picture
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.Controls.Add(this.cbDisplayOptions);
         this.Controls.Add(this.lDisplayOptions);
         this.Controls.Add(this.cbDownloadOptions);
         this.Controls.Add(this.lOptions);
         this.Controls.Add(this.oResolution);
         this.Controls.Add(this.tbFilename);
         this.Controls.Add(this.lFileName);
         this.Name = "Picture";
         this.Size = new System.Drawing.Size(299, 201);
         this.ResumeLayout(false);
         this.PerformLayout();

      }

      #endregion

      private System.Windows.Forms.Label lFileName;
      private System.Windows.Forms.TextBox tbFilename;
      private Resolution oResolution;
      private System.Windows.Forms.Label lOptions;
      private System.Windows.Forms.ComboBox cbDownloadOptions;
      private System.Windows.Forms.Label lDisplayOptions;
      private System.Windows.Forms.ComboBox cbDisplayOptions;
   }
}
