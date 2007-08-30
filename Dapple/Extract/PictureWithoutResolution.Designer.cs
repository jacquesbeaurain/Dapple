namespace Dapple.Extract
{
   partial class PictureWithoutResolution
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
         this.lDisplayOptions = new System.Windows.Forms.Label();
         this.cbDisplayOptions = new System.Windows.Forms.ComboBox();
         this.lSize = new System.Windows.Forms.Label();
         this.cbSize = new System.Windows.Forms.ComboBox();
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
         this.lOptions.Location = new System.Drawing.Point(3, 59);
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
         this.cbDownloadOptions.Location = new System.Drawing.Point(95, 56);
         this.cbDownloadOptions.Name = "cbDownloadOptions";
         this.cbDownloadOptions.Size = new System.Drawing.Size(196, 21);
         this.cbDownloadOptions.TabIndex = 6;
         this.cbDownloadOptions.SelectedIndexChanged += new System.EventHandler(this.cbDownloadOptions_SelectedIndexChanged);
         // 
         // lDisplayOptions
         // 
         this.lDisplayOptions.AutoSize = true;
         this.lDisplayOptions.Location = new System.Drawing.Point(3, 89);
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
         this.cbDisplayOptions.Location = new System.Drawing.Point(95, 83);
         this.cbDisplayOptions.Name = "cbDisplayOptions";
         this.cbDisplayOptions.Size = new System.Drawing.Size(195, 21);
         this.cbDisplayOptions.TabIndex = 8;
         // 
         // lSize
         // 
         this.lSize.AutoSize = true;
         this.lSize.Location = new System.Drawing.Point(3, 32);
         this.lSize.Name = "lSize";
         this.lSize.Size = new System.Drawing.Size(30, 13);
         this.lSize.TabIndex = 9;
         this.lSize.Text = "Size:";
         // 
         // cbSize
         // 
         this.cbSize.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                     | System.Windows.Forms.AnchorStyles.Right)));
         this.cbSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
         this.cbSize.FormattingEnabled = true;
         this.cbSize.Location = new System.Drawing.Point(95, 29);
         this.cbSize.Name = "cbSize";
         this.cbSize.Size = new System.Drawing.Size(196, 21);
         this.cbSize.TabIndex = 10;
         // 
         // PictureWithoutResolution
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.Controls.Add(this.cbSize);
         this.Controls.Add(this.lSize);
         this.Controls.Add(this.cbDisplayOptions);
         this.Controls.Add(this.lDisplayOptions);
         this.Controls.Add(this.cbDownloadOptions);
         this.Controls.Add(this.lOptions);
         this.Controls.Add(this.tbFilename);
         this.Controls.Add(this.lFileName);
         this.Name = "PictureWithoutResolution";
         this.Size = new System.Drawing.Size(299, 117);
         this.ResumeLayout(false);
         this.PerformLayout();

      }

      #endregion

      private System.Windows.Forms.Label lFileName;
      private System.Windows.Forms.TextBox tbFilename;
      private System.Windows.Forms.Label lOptions;
      private System.Windows.Forms.ComboBox cbDownloadOptions;
      private System.Windows.Forms.Label lDisplayOptions;
      private System.Windows.Forms.ComboBox cbDisplayOptions;
      private System.Windows.Forms.Label lSize;
      private System.Windows.Forms.ComboBox cbSize;
   }
}
