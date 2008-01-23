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
			this.components = new System.ComponentModel.Container();
			this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
			this.btnCancel = new System.Windows.Forms.Button();
			this.btnOK = new System.Windows.Forms.Button();
			this.labelControl1 = new Geosoft.OpenGX.UtilityForms.LabelControl();
			this.cFilenameControl = new Geosoft.OpenGX.UtilityForms.FEditControl();
			this.cFilenameErrorProvider = new System.Windows.Forms.ErrorProvider(this.components);
			((System.ComponentModel.ISupportInitialize)(this.cFilenameErrorProvider)).BeginInit();
			this.SuspendLayout();
			// 
			// btnCancel
			// 
			this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnCancel.CausesValidation = false;
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(224, 46);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(75, 23);
			this.btnCancel.TabIndex = 2;
			this.btnCancel.Text = "C&ancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			// 
			// btnOK
			// 
			this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnOK.Location = new System.Drawing.Point(143, 46);
			this.btnOK.Name = "btnOK";
			this.btnOK.Size = new System.Drawing.Size(75, 23);
			this.btnOK.TabIndex = 1;
			this.btnOK.Text = "&OK";
			this.btnOK.UseVisualStyleBackColor = true;
			this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
			// 
			// labelControl1
			// 
			this.labelControl1.AutoSize = true;
			this.labelControl1.BuddyControl = this.cFilenameControl;
			this.labelControl1.Location = new System.Drawing.Point(16, 18);
			this.labelControl1.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
			this.labelControl1.Name = "labelControl1";
			this.labelControl1.Size = new System.Drawing.Size(81, 13);
			this.labelControl1.TabIndex = 32;
			this.labelControl1.Text = "GeoTIFF name:";
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
			this.cFilenameControl.Location = new System.Drawing.Point(103, 14);
			this.cFilenameControl.MaxHistory = 0;
			this.cFilenameControl.MaximumSize = new System.Drawing.Size(400, 21);
			this.cFilenameControl.MinimumSize = new System.Drawing.Size(50, 21);
			this.cFilenameControl.Name = "cFilenameControl";
			this.cFilenameControl.Required = true;
			this.cFilenameControl.Size = new System.Drawing.Size(196, 21);
			this.cFilenameControl.TabIndex = 0;
			this.cFilenameControl.Validating += new System.ComponentModel.CancelEventHandler(this.cFilenameControl_Validating);
			// 
			// cFilenameErrorProvider
			// 
			this.cFilenameErrorProvider.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
			this.cFilenameErrorProvider.ContainerControl = this.cFilenameControl;
			// 
			// ExportView
			// 
			this.AcceptButton = this.btnOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(319, 81);
			this.Controls.Add(this.labelControl1);
			this.Controls.Add(this.cFilenameControl);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOK);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ExportView";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Create GeoTIFF Snapshot";
			((System.ComponentModel.ISupportInitialize)(this.cFilenameErrorProvider)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

      }

      #endregion

		private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
      private System.Windows.Forms.Button btnCancel;
      private System.Windows.Forms.Button btnOK;
      private Geosoft.OpenGX.UtilityForms.FEditControl cFilenameControl;
		private System.Windows.Forms.ErrorProvider cFilenameErrorProvider;
		private Geosoft.OpenGX.UtilityForms.LabelControl labelControl1;
   }
}