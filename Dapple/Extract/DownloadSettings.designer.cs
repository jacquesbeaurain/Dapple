namespace Dapple.Extract
{
   partial class DownloadSettings
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
			this.lCoordinateSystem = new System.Windows.Forms.Label();
			this.rbReproject = new System.Windows.Forms.RadioButton();
			this.rbCSNative = new System.Windows.Forms.RadioButton();
			this.pSettings = new System.Windows.Forms.Panel();
			this.lvDatasets = new System.Windows.Forms.ListView();
			this.chName = new System.Windows.Forms.ColumnHeader();
			this.pBottom = new System.Windows.Forms.Panel();
			this.bCancel = new System.Windows.Forms.Button();
			this.bDownload = new System.Windows.Forms.Button();
			this.cFolderControl = new Geosoft.OpenGX.UtilityForms.FolderEditControl();
			this.cErrorProvider = new System.Windows.Forms.ErrorProvider(this.components);
			this.lDestination = new Geosoft.OpenGX.UtilityForms.LabelControl();
			this.pBottom.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.cErrorProvider)).BeginInit();
			this.SuspendLayout();
			// 
			// lCoordinateSystem
			// 
			this.lCoordinateSystem.AutoSize = true;
			this.lCoordinateSystem.Location = new System.Drawing.Point(13, 40);
			this.lCoordinateSystem.Name = "lCoordinateSystem";
			this.lCoordinateSystem.Size = new System.Drawing.Size(98, 13);
			this.lCoordinateSystem.TabIndex = 3;
			this.lCoordinateSystem.Text = "Coordinate System:";
			// 
			// rbReproject
			// 
			this.rbReproject.AutoSize = true;
			this.rbReproject.Location = new System.Drawing.Point(179, 38);
			this.rbReproject.Name = "rbReproject";
			this.rbReproject.Size = new System.Drawing.Size(142, 17);
			this.rbReproject.TabIndex = 5;
			this.rbReproject.Text = "Reproject to original map";
			this.rbReproject.UseVisualStyleBackColor = true;
			// 
			// rbCSNative
			// 
			this.rbCSNative.AutoSize = true;
			this.rbCSNative.Checked = true;
			this.rbCSNative.Location = new System.Drawing.Point(117, 38);
			this.rbCSNative.Name = "rbCSNative";
			this.rbCSNative.Size = new System.Drawing.Size(56, 17);
			this.rbCSNative.TabIndex = 4;
			this.rbCSNative.TabStop = true;
			this.rbCSNative.Text = "Native";
			this.rbCSNative.UseVisualStyleBackColor = true;
			// 
			// pSettings
			// 
			this.pSettings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
							| System.Windows.Forms.AnchorStyles.Right)));
			this.pSettings.AutoScroll = true;
			this.pSettings.Location = new System.Drawing.Point(16, 214);
			this.pSettings.Name = "pSettings";
			this.pSettings.Size = new System.Drawing.Size(511, 226);
			this.pSettings.TabIndex = 1;
			// 
			// lvDatasets
			// 
			this.lvDatasets.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
							| System.Windows.Forms.AnchorStyles.Left)
							| System.Windows.Forms.AnchorStyles.Right)));
			this.lvDatasets.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.chName});
			this.lvDatasets.FullRowSelect = true;
			this.lvDatasets.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.lvDatasets.HideSelection = false;
			this.lvDatasets.Location = new System.Drawing.Point(16, 61);
			this.lvDatasets.MultiSelect = false;
			this.lvDatasets.Name = "lvDatasets";
			this.lvDatasets.Size = new System.Drawing.Size(511, 147);
			this.lvDatasets.TabIndex = 0;
			this.lvDatasets.UseCompatibleStateImageBehavior = false;
			this.lvDatasets.View = System.Windows.Forms.View.Details;
			this.lvDatasets.SelectedIndexChanged += new System.EventHandler(this.lvDatasets_SelectedIndexChanged);
			// 
			// chName
			// 
			this.chName.Text = "Data Layers";
			this.chName.Width = 439;
			// 
			// pBottom
			// 
			this.pBottom.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
							| System.Windows.Forms.AnchorStyles.Right)));
			this.pBottom.Controls.Add(this.bCancel);
			this.pBottom.Controls.Add(this.bDownload);
			this.pBottom.Location = new System.Drawing.Point(0, 446);
			this.pBottom.Name = "pBottom";
			this.pBottom.Size = new System.Drawing.Size(547, 52);
			this.pBottom.TabIndex = 13;
			// 
			// bCancel
			// 
			this.bCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.bCancel.CausesValidation = false;
			this.bCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.bCancel.Location = new System.Drawing.Point(452, 17);
			this.bCancel.Name = "bCancel";
			this.bCancel.Size = new System.Drawing.Size(75, 23);
			this.bCancel.TabIndex = 1;
			this.bCancel.Text = "Cancel";
			this.bCancel.UseVisualStyleBackColor = true;
			// 
			// bDownload
			// 
			this.bDownload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.bDownload.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.bDownload.Location = new System.Drawing.Point(371, 17);
			this.bDownload.Name = "bDownload";
			this.bDownload.Size = new System.Drawing.Size(75, 23);
			this.bDownload.TabIndex = 0;
			this.bDownload.Text = "Download";
			this.bDownload.UseVisualStyleBackColor = true;
			this.bDownload.Click += new System.EventHandler(this.bDownload_Click);
			// 
			// cFolderControl
			// 
			this.cFolderControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
							| System.Windows.Forms.AnchorStyles.Right)));
			this.cFolderControl.BrowseQuery = "";
			this.cFolderControl.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.cFolderControl.Location = new System.Drawing.Point(117, 12);
			this.cFolderControl.Name = "cFolderControl";
			this.cFolderControl.Required = true;
			this.cFolderControl.Size = new System.Drawing.Size(410, 21);
			this.cFolderControl.TabIndex = 0;
			this.cFolderControl.Text = "folderEditControl1";
			this.cFolderControl.Value = "";
			this.cFolderControl.Validating += new System.ComponentModel.CancelEventHandler(this.cFolderControl_Validating);
			// 
			// cErrorProvider
			// 
			this.cErrorProvider.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
			this.cErrorProvider.ContainerControl = this;
			// 
			// lDestination
			// 
			this.lDestination.AutoSize = true;
			this.lDestination.BuddyControl = this.cFolderControl;
			this.lDestination.Location = new System.Drawing.Point(13, 16);
			this.lDestination.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
			this.lDestination.Name = "lDestination";
			this.lDestination.Size = new System.Drawing.Size(92, 13);
			this.lDestination.TabIndex = 14;
			this.lDestination.Text = "Destination folder:";
			// 
			// DownloadSettings
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.bCancel;
			this.ClientSize = new System.Drawing.Size(547, 498);
			this.Controls.Add(this.lDestination);
			this.Controls.Add(this.cFolderControl);
			this.Controls.Add(this.lvDatasets);
			this.Controls.Add(this.pSettings);
			this.Controls.Add(this.rbReproject);
			this.Controls.Add(this.lCoordinateSystem);
			this.Controls.Add(this.rbCSNative);
			this.Controls.Add(this.pBottom);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "DownloadSettings";
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Extract Data Layers...";
			this.Shown += new System.EventHandler(this.DownloadSettings_Shown);
			this.pBottom.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.cErrorProvider)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

      }

      #endregion

		private System.Windows.Forms.Label lCoordinateSystem;
      private System.Windows.Forms.RadioButton rbReproject;
      private System.Windows.Forms.RadioButton rbCSNative;
      private System.Windows.Forms.Panel pBottom;
      private System.Windows.Forms.ListView lvDatasets;
      private System.Windows.Forms.ColumnHeader chName;
      private System.Windows.Forms.Button bCancel;
      private System.Windows.Forms.Button bDownload;
      private System.Windows.Forms.Panel pSettings;
		private Geosoft.OpenGX.UtilityForms.FolderEditControl cFolderControl;
		private System.Windows.Forms.ErrorProvider cErrorProvider;
		private Geosoft.OpenGX.UtilityForms.LabelControl lDestination;
   }
}

