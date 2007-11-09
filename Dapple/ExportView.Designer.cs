namespace Dapple
{
   partial class ExportView
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

      #region Windows Form Designer generated code

      /// <summary>
      /// Required method for Designer support - do not modify
      /// the contents of this method with the code editor.
      /// </summary>
      private void InitializeComponent()
      {
         System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExportView));
         this.labelName = new System.Windows.Forms.Label();
         this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
         this.btnCancel = new System.Windows.Forms.Button();
         this.btnOK = new System.Windows.Forms.Button();
         this.cFilenameControl = new Geosoft.OpenGX.UtilityForms.FEditControl();
         this.SuspendLayout();
         // 
         // labelName
         // 
         this.labelName.AutoSize = true;
         this.labelName.Location = new System.Drawing.Point(12, 16);
         this.labelName.Name = "labelName";
         this.labelName.Size = new System.Drawing.Size(76, 13);
         this.labelName.TabIndex = 13;
         this.labelName.Text = "GeoTiff Name:";
         this.labelName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
         // 
         // btnCancel
         // 
         this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
         this.btnCancel.CausesValidation = false;
         this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
         this.btnCancel.Location = new System.Drawing.Point(232, 46);
         this.btnCancel.Name = "btnCancel";
         this.btnCancel.Size = new System.Drawing.Size(75, 23);
         this.btnCancel.TabIndex = 30;
         this.btnCancel.Text = "C&ancel";
         this.btnCancel.UseVisualStyleBackColor = true;
         // 
         // btnOK
         // 
         this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
         this.btnOK.Location = new System.Drawing.Point(151, 46);
         this.btnOK.Name = "btnOK";
         this.btnOK.Size = new System.Drawing.Size(75, 23);
         this.btnOK.TabIndex = 29;
         this.btnOK.Text = "&OK";
         this.btnOK.UseVisualStyleBackColor = true;
         this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
         // 
         // cFilenameControl
         // 
         this.cFilenameControl.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
         this.cFilenameControl.BrowseQuery = "Save Snapshot As...";
         this.cFilenameControl.FileName = "";
         this.cFilenameControl.FileOpenSave = Geosoft.OpenGX.UtilityForms.FileOpenSaveEnum.Save;
         this.cFilenameControl.FilterIndex = 0;
         this.cFilenameControl.Filters = "GeoTIFF (*.tif)|*.tif|BMP (*.bmp)|*.bmp|PNG (*.png)|*.png|GIF (*.gif)|*.gif";
         this.cFilenameControl.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
         this.cFilenameControl.InitialDirectory = "\\\\Geostore\\Document Root\\chrismac\\My Documents";
         this.cFilenameControl.Location = new System.Drawing.Point(94, 12);
         this.cFilenameControl.MaxHistory = 0;
         this.cFilenameControl.MaximumSize = new System.Drawing.Size(400, 21);
         this.cFilenameControl.MinimumSize = new System.Drawing.Size(50, 21);
         this.cFilenameControl.Name = "cFilenameControl";
         this.cFilenameControl.Required = true;
         this.cFilenameControl.Size = new System.Drawing.Size(213, 21);
         this.cFilenameControl.TabIndex = 31;
         // 
         // ExportView
         // 
         this.AcceptButton = this.btnOK;
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(319, 81);
         this.Controls.Add(this.cFilenameControl);
         this.Controls.Add(this.btnCancel);
         this.Controls.Add(this.btnOK);
         this.Controls.Add(this.labelName);
         this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
         this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
         this.MaximizeBox = false;
         this.MinimizeBox = false;
         this.Name = "ExportView";
         this.ShowInTaskbar = false;
         this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
         this.Text = "Create GeoTIFF Snapshot";
         this.Load += new System.EventHandler(this.ExportView_Load);
         this.ResumeLayout(false);
         this.PerformLayout();

      }

      #endregion

      private System.Windows.Forms.Label labelName;
      private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
      private System.Windows.Forms.Button btnCancel;
      private System.Windows.Forms.Button btnOK;
      private Geosoft.OpenGX.UtilityForms.FEditControl cFilenameControl;
   }
}