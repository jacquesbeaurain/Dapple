namespace Dapple.Extract
{
   partial class ArcGIS
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
         this.lDownload = new System.Windows.Forms.Label();
         this.cbDownload = new System.Windows.Forms.ComboBox();
         this.tbFilename = new System.Windows.Forms.TextBox();
         this.lFileName = new System.Windows.Forms.Label();
         this.SuspendLayout();
         // 
         // lDownload
         // 
         this.lDownload.AutoSize = true;
         this.lDownload.Location = new System.Drawing.Point(4, 33);
         this.lDownload.Name = "lDownload";
         this.lDownload.Size = new System.Drawing.Size(95, 13);
         this.lDownload.TabIndex = 2;
         this.lDownload.Text = "Download options:";
         // 
         // cbDownload
         // 
         this.cbDownload.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                     | System.Windows.Forms.AnchorStyles.Right)));
         this.cbDownload.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
         this.cbDownload.FormattingEnabled = true;
         this.cbDownload.Items.AddRange(new object[] {
            "Download And Open",
            "Download Only"});
         this.cbDownload.Location = new System.Drawing.Point(105, 30);
         this.cbDownload.Name = "cbDownload";
         this.cbDownload.Size = new System.Drawing.Size(186, 21);
         this.cbDownload.TabIndex = 3;
         // 
         // tbFilename
         // 
         this.tbFilename.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                     | System.Windows.Forms.AnchorStyles.Right)));
         this.tbFilename.Location = new System.Drawing.Point(105, 3);
         this.tbFilename.Name = "tbFilename";
         this.tbFilename.Size = new System.Drawing.Size(186, 20);
         this.tbFilename.TabIndex = 1;
         // 
         // lFileName
         // 
         this.lFileName.AutoSize = true;
         this.lFileName.Location = new System.Drawing.Point(3, 6);
         this.lFileName.Name = "lFileName";
         this.lFileName.Size = new System.Drawing.Size(68, 13);
         this.lFileName.TabIndex = 0;
         this.lFileName.Text = "Folder name:";
         // 
         // ArcGIS
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.Controls.Add(this.cbDownload);
         this.Controls.Add(this.lDownload);
         this.Controls.Add(this.tbFilename);
         this.Controls.Add(this.lFileName);
         this.Name = "ArcGIS";
         this.Size = new System.Drawing.Size(299, 59);
         this.ResumeLayout(false);
         this.PerformLayout();

      }

      #endregion

      private System.Windows.Forms.Label lDownload;
      private System.Windows.Forms.ComboBox cbDownload;
      private System.Windows.Forms.TextBox tbFilename;
      private System.Windows.Forms.Label lFileName;
   }
}
