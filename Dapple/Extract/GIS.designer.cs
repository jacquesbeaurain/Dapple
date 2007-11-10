namespace Dapple.Extract
{
   partial class GIS
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
         this.lGroupName = new System.Windows.Forms.Label();
         this.tbGroupName = new System.Windows.Forms.TextBox();
         this.lOptions = new System.Windows.Forms.Label();
         this.cbOptions = new System.Windows.Forms.ComboBox();
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
         this.tbFilename.Location = new System.Drawing.Point(104, 3);
         this.tbFilename.Name = "tbFilename";
         this.tbFilename.Size = new System.Drawing.Size(93, 20);
         this.tbFilename.TabIndex = 1;
         // 
         // lGroupName
         // 
         this.lGroupName.AutoSize = true;
         this.lGroupName.Location = new System.Drawing.Point(3, 32);
         this.lGroupName.Name = "lGroupName";
         this.lGroupName.Size = new System.Drawing.Size(68, 13);
         this.lGroupName.TabIndex = 2;
         this.lGroupName.Text = "Group name:";
         // 
         // tbGroupName
         // 
         this.tbGroupName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                     | System.Windows.Forms.AnchorStyles.Right)));
         this.tbGroupName.Location = new System.Drawing.Point(104, 29);
         this.tbGroupName.Name = "tbGroupName";
         this.tbGroupName.Size = new System.Drawing.Size(93, 20);
         this.tbGroupName.TabIndex = 3;
         // 
         // lOptions
         // 
         this.lOptions.AutoSize = true;
         this.lOptions.Location = new System.Drawing.Point(3, 58);
         this.lOptions.Name = "lOptions";
         this.lOptions.Size = new System.Drawing.Size(95, 13);
         this.lOptions.TabIndex = 5;
         this.lOptions.Text = "Download options:";
         // 
         // cbOptions
         // 
         this.cbOptions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                     | System.Windows.Forms.AnchorStyles.Right)));
         this.cbOptions.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
         this.cbOptions.FormattingEnabled = true;
         this.cbOptions.Location = new System.Drawing.Point(104, 55);
         this.cbOptions.Name = "cbOptions";
         this.cbOptions.Size = new System.Drawing.Size(93, 21);
         this.cbOptions.TabIndex = 6;
         this.cbOptions.SelectedIndexChanged += new System.EventHandler(this.cbOptions_SelectedIndexChanged);
         // 
         // GIS
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.Controls.Add(this.cbOptions);
         this.Controls.Add(this.lOptions);
         this.Controls.Add(this.tbGroupName);
         this.Controls.Add(this.lGroupName);
         this.Controls.Add(this.tbFilename);
         this.Controls.Add(this.lFileName);
         this.Name = "GIS";
         this.Size = new System.Drawing.Size(200, 300);
         this.ResumeLayout(false);
         this.PerformLayout();

      }

      #endregion

      private System.Windows.Forms.Label lFileName;
      private System.Windows.Forms.TextBox tbFilename;
      private System.Windows.Forms.Label lGroupName;
      private System.Windows.Forms.TextBox tbGroupName;
      private System.Windows.Forms.Label lOptions;
      private System.Windows.Forms.ComboBox cbOptions;
   }
}
