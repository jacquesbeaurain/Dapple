namespace Dapple.Extract
{
   partial class HyperMAP
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
         this.oResolution = new Dapple.Extract.Resolution();
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
         // oResolution
         // 
         this.oResolution.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                     | System.Windows.Forms.AnchorStyles.Right)));
         this.oResolution.Location = new System.Drawing.Point(0, 55);
         this.oResolution.Name = "oResolution";
         this.oResolution.Size = new System.Drawing.Size(200, 105);
         this.oResolution.TabIndex = 4;
         // 
         // HyperMAP
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.Controls.Add(this.oResolution);
         this.Controls.Add(this.tbGroupName);
         this.Controls.Add(this.lGroupName);
         this.Controls.Add(this.tbFilename);
         this.Controls.Add(this.lFileName);
         this.Name = "HyperMAP";
         this.Size = new System.Drawing.Size(200, 300);
         this.ResumeLayout(false);
         this.PerformLayout();

      }

      #endregion

      private System.Windows.Forms.Label lFileName;
      private System.Windows.Forms.TextBox tbFilename;
      private System.Windows.Forms.Label lGroupName;
      private System.Windows.Forms.TextBox tbGroupName;
      private Resolution oResolution;
   }
}
