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
         this.tcMain = new System.Windows.Forms.TabControl();
         this.tpGeneral = new System.Windows.Forms.TabPage();
         this.pResolution = new System.Windows.Forms.Panel();
         this.rbCustom = new System.Windows.Forms.RadioButton();
         this.lResolution = new System.Windows.Forms.Label();
         this.rbDefault = new System.Windows.Forms.RadioButton();
         this.rbOriginal = new System.Windows.Forms.RadioButton();
         this.pClip = new System.Windows.Forms.Panel();
         this.lClip = new System.Windows.Forms.Label();
         this.rbNone = new System.Windows.Forms.RadioButton();
         this.rbClipViewedArea = new System.Windows.Forms.RadioButton();
         this.rbClipMapExtent = new System.Windows.Forms.RadioButton();
         this.pCS = new System.Windows.Forms.Panel();
         this.lCoordinateSystem = new System.Windows.Forms.Label();
         this.rbReproject = new System.Windows.Forms.RadioButton();
         this.rbCSNative = new System.Windows.Forms.RadioButton();
         this.bBrowseDestination = new System.Windows.Forms.Button();
         this.tbDestination = new System.Windows.Forms.TextBox();
         this.lDestination = new System.Windows.Forms.Label();
         this.tpIndividual = new System.Windows.Forms.TabPage();
         this.pSettings = new System.Windows.Forms.Panel();
         this.lvDatasets = new System.Windows.Forms.ListView();
         this.chName = new System.Windows.Forms.ColumnHeader();
         this.pBottom = new System.Windows.Forms.Panel();
         this.bCancel = new System.Windows.Forms.Button();
         this.bDownload = new System.Windows.Forms.Button();
         this.tcMain.SuspendLayout();
         this.tpGeneral.SuspendLayout();
         this.pResolution.SuspendLayout();
         this.pClip.SuspendLayout();
         this.pCS.SuspendLayout();
         this.tpIndividual.SuspendLayout();
         this.pBottom.SuspendLayout();
         this.SuspendLayout();
         // 
         // tcMain
         // 
         this.tcMain.Controls.Add(this.tpGeneral);
         this.tcMain.Controls.Add(this.tpIndividual);
         this.tcMain.Dock = System.Windows.Forms.DockStyle.Fill;
         this.tcMain.Location = new System.Drawing.Point(0, 0);
         this.tcMain.Name = "tcMain";
         this.tcMain.SelectedIndex = 0;
         this.tcMain.Size = new System.Drawing.Size(457, 402);
         this.tcMain.TabIndex = 0;
         // 
         // tpGeneral
         // 
         this.tpGeneral.Controls.Add(this.pResolution);
         this.tpGeneral.Controls.Add(this.pClip);
         this.tpGeneral.Controls.Add(this.pCS);
         this.tpGeneral.Controls.Add(this.bBrowseDestination);
         this.tpGeneral.Controls.Add(this.tbDestination);
         this.tpGeneral.Controls.Add(this.lDestination);
         this.tpGeneral.Location = new System.Drawing.Point(4, 22);
         this.tpGeneral.Name = "tpGeneral";
         this.tpGeneral.Padding = new System.Windows.Forms.Padding(3);
         this.tpGeneral.Size = new System.Drawing.Size(449, 376);
         this.tpGeneral.TabIndex = 0;
         this.tpGeneral.Text = "General";
         this.tpGeneral.UseVisualStyleBackColor = true;
         // 
         // pResolution
         // 
         this.pResolution.Controls.Add(this.rbCustom);
         this.pResolution.Controls.Add(this.lResolution);
         this.pResolution.Controls.Add(this.rbDefault);
         this.pResolution.Controls.Add(this.rbOriginal);
         this.pResolution.Location = new System.Drawing.Point(8, 192);
         this.pResolution.Name = "pResolution";
         this.pResolution.Size = new System.Drawing.Size(492, 79);
         this.pResolution.TabIndex = 15;
         // 
         // rbCustom
         // 
         this.rbCustom.AutoSize = true;
         this.rbCustom.Location = new System.Drawing.Point(117, 54);
         this.rbCustom.Name = "rbCustom";
         this.rbCustom.Size = new System.Drawing.Size(60, 17);
         this.rbCustom.TabIndex = 13;
         this.rbCustom.Text = "Custom";
         this.rbCustom.UseVisualStyleBackColor = true;
         this.rbCustom.CheckedChanged += new System.EventHandler(this.Resolution_Changed);
         // 
         // lResolution
         // 
         this.lResolution.AutoSize = true;
         this.lResolution.Location = new System.Drawing.Point(12, 10);
         this.lResolution.Name = "lResolution";
         this.lResolution.Size = new System.Drawing.Size(60, 13);
         this.lResolution.TabIndex = 10;
         this.lResolution.Text = "Resolution:";
         // 
         // rbDefault
         // 
         this.rbDefault.AutoSize = true;
         this.rbDefault.Checked = true;
         this.rbDefault.Location = new System.Drawing.Point(117, 8);
         this.rbDefault.Name = "rbDefault";
         this.rbDefault.Size = new System.Drawing.Size(59, 17);
         this.rbDefault.TabIndex = 11;
         this.rbDefault.TabStop = true;
         this.rbDefault.Text = "Default";
         this.rbDefault.UseVisualStyleBackColor = true;
         this.rbDefault.CheckedChanged += new System.EventHandler(this.Resolution_Changed);
         // 
         // rbOriginal
         // 
         this.rbOriginal.AutoSize = true;
         this.rbOriginal.Location = new System.Drawing.Point(117, 31);
         this.rbOriginal.Name = "rbOriginal";
         this.rbOriginal.Size = new System.Drawing.Size(60, 17);
         this.rbOriginal.TabIndex = 12;
         this.rbOriginal.Text = "Original";
         this.rbOriginal.UseVisualStyleBackColor = true;
         this.rbOriginal.CheckedChanged += new System.EventHandler(this.Resolution_Changed);
         // 
         // pClip
         // 
         this.pClip.Controls.Add(this.lClip);
         this.pClip.Controls.Add(this.rbNone);
         this.pClip.Controls.Add(this.rbClipViewedArea);
         this.pClip.Controls.Add(this.rbClipMapExtent);
         this.pClip.Location = new System.Drawing.Point(8, 104);
         this.pClip.Name = "pClip";
         this.pClip.Size = new System.Drawing.Size(492, 82);
         this.pClip.TabIndex = 14;
         // 
         // lClip
         // 
         this.lClip.AutoSize = true;
         this.lClip.Location = new System.Drawing.Point(12, 10);
         this.lClip.Name = "lClip";
         this.lClip.Size = new System.Drawing.Size(27, 13);
         this.lClip.TabIndex = 6;
         this.lClip.Text = "Clip:";
         // 
         // rbNone
         // 
         this.rbNone.AutoSize = true;
         this.rbNone.Location = new System.Drawing.Point(117, 8);
         this.rbNone.Name = "rbNone";
         this.rbNone.Size = new System.Drawing.Size(51, 17);
         this.rbNone.TabIndex = 7;
         this.rbNone.Text = "None";
         this.rbNone.UseVisualStyleBackColor = true;
         // 
         // rbClipViewedArea
         // 
         this.rbClipViewedArea.AutoSize = true;
         this.rbClipViewedArea.Checked = true;
         this.rbClipViewedArea.Location = new System.Drawing.Point(117, 31);
         this.rbClipViewedArea.Name = "rbClipViewedArea";
         this.rbClipViewedArea.Size = new System.Drawing.Size(99, 17);
         this.rbClipViewedArea.TabIndex = 8;
         this.rbClipViewedArea.TabStop = true;
         this.rbClipViewedArea.Text = "To viewed area";
         this.rbClipViewedArea.UseVisualStyleBackColor = true;
         // 
         // rbClipMapExtent
         // 
         this.rbClipMapExtent.AutoSize = true;
         this.rbClipMapExtent.Location = new System.Drawing.Point(117, 54);
         this.rbClipMapExtent.Name = "rbClipMapExtent";
         this.rbClipMapExtent.Size = new System.Drawing.Size(129, 17);
         this.rbClipMapExtent.TabIndex = 9;
         this.rbClipMapExtent.Text = "To original map extent";
         this.rbClipMapExtent.UseVisualStyleBackColor = true;
         // 
         // pCS
         // 
         this.pCS.Controls.Add(this.lCoordinateSystem);
         this.pCS.Controls.Add(this.rbReproject);
         this.pCS.Controls.Add(this.rbCSNative);
         this.pCS.Location = new System.Drawing.Point(8, 38);
         this.pCS.Name = "pCS";
         this.pCS.Size = new System.Drawing.Size(492, 60);
         this.pCS.TabIndex = 13;
         // 
         // lCoordinateSystem
         // 
         this.lCoordinateSystem.AutoSize = true;
         this.lCoordinateSystem.Location = new System.Drawing.Point(12, 13);
         this.lCoordinateSystem.Name = "lCoordinateSystem";
         this.lCoordinateSystem.Size = new System.Drawing.Size(98, 13);
         this.lCoordinateSystem.TabIndex = 3;
         this.lCoordinateSystem.Text = "Coordinate System:";
         // 
         // rbReproject
         // 
         this.rbReproject.AutoSize = true;
         this.rbReproject.Location = new System.Drawing.Point(117, 34);
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
         this.rbCSNative.Location = new System.Drawing.Point(117, 11);
         this.rbCSNative.Name = "rbCSNative";
         this.rbCSNative.Size = new System.Drawing.Size(56, 17);
         this.rbCSNative.TabIndex = 4;
         this.rbCSNative.TabStop = true;
         this.rbCSNative.Text = "Native";
         this.rbCSNative.UseVisualStyleBackColor = true;
         // 
         // bBrowseDestination
         // 
         this.bBrowseDestination.FlatStyle = System.Windows.Forms.FlatStyle.System;
         this.bBrowseDestination.Location = new System.Drawing.Point(423, 12);
         this.bBrowseDestination.Name = "bBrowseDestination";
         this.bBrowseDestination.Size = new System.Drawing.Size(20, 20);
         this.bBrowseDestination.TabIndex = 2;
         this.bBrowseDestination.Text = "...";
         this.bBrowseDestination.UseVisualStyleBackColor = true;
         this.bBrowseDestination.Click += new System.EventHandler(this.bBrowseDestination_Click);
         // 
         // tbDestination
         // 
         this.tbDestination.Location = new System.Drawing.Point(125, 12);
         this.tbDestination.Name = "tbDestination";
         this.tbDestination.Size = new System.Drawing.Size(292, 20);
         this.tbDestination.TabIndex = 1;
         // 
         // lDestination
         // 
         this.lDestination.AutoSize = true;
         this.lDestination.Location = new System.Drawing.Point(20, 17);
         this.lDestination.Name = "lDestination";
         this.lDestination.Size = new System.Drawing.Size(63, 13);
         this.lDestination.TabIndex = 0;
         this.lDestination.Text = "Destination:";
         // 
         // tpIndividual
         // 
         this.tpIndividual.Controls.Add(this.pSettings);
         this.tpIndividual.Controls.Add(this.lvDatasets);
         this.tpIndividual.Location = new System.Drawing.Point(4, 22);
         this.tpIndividual.Name = "tpIndividual";
         this.tpIndividual.Padding = new System.Windows.Forms.Padding(3);
         this.tpIndividual.Size = new System.Drawing.Size(449, 376);
         this.tpIndividual.TabIndex = 1;
         this.tpIndividual.Text = "Individual";
         this.tpIndividual.UseVisualStyleBackColor = true;
         // 
         // pSettings
         // 
         this.pSettings.AutoScroll = true;
         this.pSettings.Dock = System.Windows.Forms.DockStyle.Fill;
         this.pSettings.Location = new System.Drawing.Point(3, 173);
         this.pSettings.Name = "pSettings";
         this.pSettings.Size = new System.Drawing.Size(443, 200);
         this.pSettings.TabIndex = 1;
         // 
         // lvDatasets
         // 
         this.lvDatasets.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.chName});
         this.lvDatasets.Dock = System.Windows.Forms.DockStyle.Top;
         this.lvDatasets.FullRowSelect = true;
         this.lvDatasets.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
         this.lvDatasets.HideSelection = false;
         this.lvDatasets.Location = new System.Drawing.Point(3, 3);
         this.lvDatasets.Name = "lvDatasets";
         this.lvDatasets.Size = new System.Drawing.Size(443, 170);
         this.lvDatasets.TabIndex = 0;
         this.lvDatasets.UseCompatibleStateImageBehavior = false;
         this.lvDatasets.View = System.Windows.Forms.View.Details;
         this.lvDatasets.SelectedIndexChanged += new System.EventHandler(this.lvDatasets_SelectedIndexChanged);
         // 
         // chName
         // 
         this.chName.Text = "Datasets";
         this.chName.Width = 439;
         // 
         // pBottom
         // 
         this.pBottom.Controls.Add(this.bCancel);
         this.pBottom.Controls.Add(this.bDownload);
         this.pBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
         this.pBottom.Location = new System.Drawing.Point(0, 402);
         this.pBottom.Name = "pBottom";
         this.pBottom.Size = new System.Drawing.Size(457, 40);
         this.pBottom.TabIndex = 13;
         // 
         // bCancel
         // 
         this.bCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
         this.bCancel.Location = new System.Drawing.Point(375, 6);
         this.bCancel.Name = "bCancel";
         this.bCancel.Size = new System.Drawing.Size(75, 23);
         this.bCancel.TabIndex = 1;
         this.bCancel.Text = "Cancel";
         this.bCancel.UseVisualStyleBackColor = true;
         // 
         // bDownload
         // 
         this.bDownload.Location = new System.Drawing.Point(294, 6);
         this.bDownload.Name = "bDownload";
         this.bDownload.Size = new System.Drawing.Size(75, 23);
         this.bDownload.TabIndex = 0;
         this.bDownload.Text = "Download";
         this.bDownload.UseVisualStyleBackColor = true;
         this.bDownload.Click += new System.EventHandler(this.bDownload_Click);
         // 
         // DownloadSettings
         // 
         this.AcceptButton = this.bDownload;
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.CancelButton = this.bCancel;
         this.ClientSize = new System.Drawing.Size(457, 442);
         this.Controls.Add(this.tcMain);
         this.Controls.Add(this.pBottom);
         this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
         this.MaximizeBox = false;
         this.MinimizeBox = false;
         this.Name = "DownloadSettings";
         this.ShowIcon = false;
         this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
         this.Text = "Download Settings";
         this.tcMain.ResumeLayout(false);
         this.tpGeneral.ResumeLayout(false);
         this.tpGeneral.PerformLayout();
         this.pResolution.ResumeLayout(false);
         this.pResolution.PerformLayout();
         this.pClip.ResumeLayout(false);
         this.pClip.PerformLayout();
         this.pCS.ResumeLayout(false);
         this.pCS.PerformLayout();
         this.tpIndividual.ResumeLayout(false);
         this.pBottom.ResumeLayout(false);
         this.ResumeLayout(false);

      }

      #endregion

      private System.Windows.Forms.TabControl tcMain;
      private System.Windows.Forms.TabPage tpGeneral;
      private System.Windows.Forms.Button bBrowseDestination;
      private System.Windows.Forms.TextBox tbDestination;
      private System.Windows.Forms.Label lDestination;
      private System.Windows.Forms.TabPage tpIndividual;
      private System.Windows.Forms.Label lCoordinateSystem;
      private System.Windows.Forms.Label lClip;
      private System.Windows.Forms.RadioButton rbReproject;
      private System.Windows.Forms.RadioButton rbCSNative;
      private System.Windows.Forms.Panel pBottom;
      private System.Windows.Forms.RadioButton rbOriginal;
      private System.Windows.Forms.RadioButton rbDefault;
      private System.Windows.Forms.Label lResolution;
      private System.Windows.Forms.RadioButton rbClipMapExtent;
      private System.Windows.Forms.RadioButton rbClipViewedArea;
      private System.Windows.Forms.RadioButton rbNone;
      private System.Windows.Forms.ListView lvDatasets;
      private System.Windows.Forms.ColumnHeader chName;
      private System.Windows.Forms.Button bCancel;
      private System.Windows.Forms.Button bDownload;
      private System.Windows.Forms.Panel pSettings;
      private System.Windows.Forms.Panel pResolution;
      private System.Windows.Forms.Panel pClip;
      private System.Windows.Forms.Panel pCS;
      private System.Windows.Forms.RadioButton rbCustom;
   }
}

