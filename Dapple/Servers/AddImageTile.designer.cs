namespace Dapple
{
   partial class AddImageTile
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
         this.errProvider = new System.Windows.Forms.ErrorProvider(this.components);
         this.numS = new System.Windows.Forms.NumericUpDown();
         this.numE = new System.Windows.Forms.NumericUpDown();
         this.numW = new System.Windows.Forms.NumericUpDown();
         this.numN = new System.Windows.Forms.NumericUpDown();
         this.label12 = new System.Windows.Forms.Label();
         this.cmbTileServerFileExtension = new System.Windows.Forms.ComboBox();
         this.butLogoPathBrowse = new System.Windows.Forms.Button();
         this.numHeight = new System.Windows.Forms.NumericUpDown();
         this.txtName = new System.Windows.Forms.TextBox();
         this.txtLogoPath = new System.Windows.Forms.TextBox();
         this.txtDatabaseName = new System.Windows.Forms.TextBox();
         this.txtServerURL = new System.Windows.Forms.TextBox();
         this.chkTileServerUseTerrainMap = new System.Windows.Forms.CheckBox();
         this.numTileSize = new System.Windows.Forms.NumericUpDown();
         this.numLevels = new System.Windows.Forms.NumericUpDown();
         this.numImagePixelSize = new System.Windows.Forms.NumericUpDown();
         this.grpExtents = new System.Windows.Forms.GroupBox();
         this.label10 = new System.Windows.Forms.Label();
         this.label9 = new System.Windows.Forms.Label();
         this.label8 = new System.Windows.Forms.Label();
         this.label7 = new System.Windows.Forms.Label();
         this.label6 = new System.Windows.Forms.Label();
         this.label5 = new System.Windows.Forms.Label();
         this.label4 = new System.Windows.Forms.Label();
         this.label2 = new System.Windows.Forms.Label();
         this.label1 = new System.Windows.Forms.Label();
         this.linkLabelHelpTile = new System.Windows.Forms.LinkLabel();
         this.butOK = new System.Windows.Forms.Button();
         this.butCancel = new System.Windows.Forms.Button();
         ((System.ComponentModel.ISupportInitialize)(this.errProvider)).BeginInit();
         ((System.ComponentModel.ISupportInitialize)(this.numS)).BeginInit();
         ((System.ComponentModel.ISupportInitialize)(this.numE)).BeginInit();
         ((System.ComponentModel.ISupportInitialize)(this.numW)).BeginInit();
         ((System.ComponentModel.ISupportInitialize)(this.numN)).BeginInit();
         ((System.ComponentModel.ISupportInitialize)(this.numHeight)).BeginInit();
         ((System.ComponentModel.ISupportInitialize)(this.numTileSize)).BeginInit();
         ((System.ComponentModel.ISupportInitialize)(this.numLevels)).BeginInit();
         ((System.ComponentModel.ISupportInitialize)(this.numImagePixelSize)).BeginInit();
         this.grpExtents.SuspendLayout();
         this.SuspendLayout();
         // 
         // errProvider
         // 
         this.errProvider.ContainerControl = this;
         // 
         // numS
         // 
         this.numS.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
         this.numS.Location = new System.Drawing.Point(45, 71);
         this.numS.Maximum = new decimal(new int[] {
            90,
            0,
            0,
            0});
         this.numS.Minimum = new decimal(new int[] {
            90,
            0,
            0,
            -2147483648});
         this.numS.Name = "numS";
         this.numS.Size = new System.Drawing.Size(38, 20);
         this.numS.TabIndex = 3;
         this.numS.Value = new decimal(new int[] {
            90,
            0,
            0,
            -2147483648});
         // 
         // numE
         // 
         this.numE.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
         this.numE.Location = new System.Drawing.Point(65, 45);
         this.numE.Maximum = new decimal(new int[] {
            180,
            0,
            0,
            0});
         this.numE.Minimum = new decimal(new int[] {
            180,
            0,
            0,
            -2147483648});
         this.numE.Name = "numE";
         this.numE.Size = new System.Drawing.Size(41, 20);
         this.numE.TabIndex = 2;
         this.numE.Value = new decimal(new int[] {
            180,
            0,
            0,
            0});
         // 
         // numW
         // 
         this.numW.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
         this.numW.Location = new System.Drawing.Point(12, 45);
         this.numW.Maximum = new decimal(new int[] {
            180,
            0,
            0,
            0});
         this.numW.Minimum = new decimal(new int[] {
            180,
            0,
            0,
            -2147483648});
         this.numW.Name = "numW";
         this.numW.Size = new System.Drawing.Size(47, 20);
         this.numW.TabIndex = 1;
         this.numW.Value = new decimal(new int[] {
            180,
            0,
            0,
            -2147483648});
         // 
         // numN
         // 
         this.numN.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
         this.numN.Location = new System.Drawing.Point(45, 19);
         this.numN.Maximum = new decimal(new int[] {
            90,
            0,
            0,
            0});
         this.numN.Minimum = new decimal(new int[] {
            90,
            0,
            0,
            -2147483648});
         this.numN.Name = "numN";
         this.numN.Size = new System.Drawing.Size(38, 20);
         this.numN.TabIndex = 0;
         this.numN.Value = new decimal(new int[] {
            90,
            0,
            0,
            0});
         // 
         // label12
         // 
         this.label12.AutoSize = true;
         this.label12.Location = new System.Drawing.Point(12, 9);
         this.label12.Name = "label12";
         this.label12.Size = new System.Drawing.Size(295, 13);
         this.label12.TabIndex = 0;
         this.label12.Text = "Please enter as much of the following information as possible.";
         // 
         // cmbTileServerFileExtension
         // 
         this.cmbTileServerFileExtension.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
         this.cmbTileServerFileExtension.FormattingEnabled = true;
         this.cmbTileServerFileExtension.Items.AddRange(new object[] {
            ".bmp",
            ".dds",
            ".dib",
            ".hdr",
            ".jpg",
            ".jpeg",
            ".pfm",
            ".png",
            ".ppm",
            ".tga",
            ".gif",
            ".tif"});
         this.cmbTileServerFileExtension.Location = new System.Drawing.Point(161, 148);
         this.cmbTileServerFileExtension.Name = "cmbTileServerFileExtension";
         this.cmbTileServerFileExtension.Size = new System.Drawing.Size(66, 21);
         this.cmbTileServerFileExtension.TabIndex = 13;
         this.cmbTileServerFileExtension.SelectedIndexChanged += new System.EventHandler(this.cmbTileServerFileExtension_SelectedIndexChanged);
         // 
         // butLogoPathBrowse
         // 
         this.butLogoPathBrowse.Location = new System.Drawing.Point(334, 212);
         this.butLogoPathBrowse.Name = "butLogoPathBrowse";
         this.butLogoPathBrowse.Size = new System.Drawing.Size(75, 23);
         this.butLogoPathBrowse.TabIndex = 20;
         this.butLogoPathBrowse.Text = "Browse...";
         this.butLogoPathBrowse.UseVisualStyleBackColor = true;
         this.butLogoPathBrowse.Click += new System.EventHandler(this.butLogoPathBrowse_Click_1);
         // 
         // numHeight
         // 
         this.numHeight.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
         this.numHeight.Location = new System.Drawing.Point(180, 59);
         this.numHeight.Maximum = new decimal(new int[] {
            25000,
            0,
            0,
            0});
         this.numHeight.Name = "numHeight";
         this.numHeight.Size = new System.Drawing.Size(47, 20);
         this.numHeight.TabIndex = 5;
         // 
         // txtName
         // 
         this.txtName.Location = new System.Drawing.Point(94, 35);
         this.txtName.Name = "txtName";
         this.txtName.Size = new System.Drawing.Size(133, 20);
         this.txtName.TabIndex = 2;
         // 
         // txtLogoPath
         // 
         this.txtLogoPath.Location = new System.Drawing.Point(161, 215);
         this.txtLogoPath.Name = "txtLogoPath";
         this.txtLogoPath.Size = new System.Drawing.Size(167, 20);
         this.txtLogoPath.TabIndex = 19;
         this.txtLogoPath.TextChanged += new System.EventHandler(this.txtLogoPath_TextChanged);
         // 
         // txtDatabaseName
         // 
         this.txtDatabaseName.Location = new System.Drawing.Point(134, 192);
         this.txtDatabaseName.Name = "txtDatabaseName";
         this.txtDatabaseName.Size = new System.Drawing.Size(275, 20);
         this.txtDatabaseName.TabIndex = 17;
         this.txtDatabaseName.TextChanged += new System.EventHandler(this.txtDatabaseName_TextChanged);
         // 
         // txtServerURL
         // 
         this.txtServerURL.Location = new System.Drawing.Point(122, 170);
         this.txtServerURL.Name = "txtServerURL";
         this.txtServerURL.Size = new System.Drawing.Size(287, 20);
         this.txtServerURL.TabIndex = 15;
         this.txtServerURL.TextChanged += new System.EventHandler(this.txtServerURL_TextChanged);
         // 
         // chkTileServerUseTerrainMap
         // 
         this.chkTileServerUseTerrainMap.AutoSize = true;
         this.chkTileServerUseTerrainMap.Location = new System.Drawing.Point(299, 39);
         this.chkTileServerUseTerrainMap.Name = "chkTileServerUseTerrainMap";
         this.chkTileServerUseTerrainMap.Size = new System.Drawing.Size(105, 17);
         this.chkTileServerUseTerrainMap.TabIndex = 3;
         this.chkTileServerUseTerrainMap.Text = "Use Terrain Map";
         this.chkTileServerUseTerrainMap.UseVisualStyleBackColor = true;
         this.chkTileServerUseTerrainMap.CheckedChanged += new System.EventHandler(this.chkTileServerUseTerrainMap_CheckedChanged);
         // 
         // numTileSize
         // 
         this.numTileSize.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
         this.numTileSize.Location = new System.Drawing.Point(180, 82);
         this.numTileSize.Maximum = new decimal(new int[] {
            45,
            0,
            0,
            0});
         this.numTileSize.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            65536});
         this.numTileSize.Name = "numTileSize";
         this.numTileSize.Size = new System.Drawing.Size(47, 20);
         this.numTileSize.TabIndex = 7;
         this.numTileSize.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
         // 
         // numLevels
         // 
         this.numLevels.Location = new System.Drawing.Point(180, 104);
         this.numLevels.Maximum = new decimal(new int[] {
            20,
            0,
            0,
            0});
         this.numLevels.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
         this.numLevels.Name = "numLevels";
         this.numLevels.Size = new System.Drawing.Size(47, 20);
         this.numLevels.TabIndex = 9;
         this.numLevels.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
         // 
         // numImagePixelSize
         // 
         this.numImagePixelSize.Increment = new decimal(new int[] {
            128,
            0,
            0,
            0});
         this.numImagePixelSize.Location = new System.Drawing.Point(180, 126);
         this.numImagePixelSize.Maximum = new decimal(new int[] {
            4096,
            0,
            0,
            0});
         this.numImagePixelSize.Minimum = new decimal(new int[] {
            128,
            0,
            0,
            0});
         this.numImagePixelSize.Name = "numImagePixelSize";
         this.numImagePixelSize.Size = new System.Drawing.Size(47, 20);
         this.numImagePixelSize.TabIndex = 11;
         this.numImagePixelSize.Value = new decimal(new int[] {
            512,
            0,
            0,
            0});
         // 
         // grpExtents
         // 
         this.grpExtents.Controls.Add(this.numS);
         this.grpExtents.Controls.Add(this.numE);
         this.grpExtents.Controls.Add(this.numW);
         this.grpExtents.Controls.Add(this.numN);
         this.grpExtents.Location = new System.Drawing.Point(289, 62);
         this.grpExtents.Name = "grpExtents";
         this.grpExtents.Size = new System.Drawing.Size(120, 102);
         this.grpExtents.TabIndex = 21;
         this.grpExtents.TabStop = false;
         this.grpExtents.Text = "Boundaries";
         // 
         // label10
         // 
         this.label10.AutoSize = true;
         this.label10.Location = new System.Drawing.Point(12, 196);
         this.label10.Name = "label10";
         this.label10.Size = new System.Drawing.Size(75, 13);
         this.label10.TabIndex = 16;
         this.label10.Text = "Dataset Name";
         // 
         // label9
         // 
         this.label9.AutoSize = true;
         this.label9.Location = new System.Drawing.Point(12, 219);
         this.label9.Name = "label9";
         this.label9.Size = new System.Drawing.Size(102, 13);
         this.label9.TabIndex = 18;
         this.label9.Text = "Logo Path (optional)";
         // 
         // label8
         // 
         this.label8.AutoSize = true;
         this.label8.Location = new System.Drawing.Point(12, 129);
         this.label8.Name = "label8";
         this.label8.Size = new System.Drawing.Size(94, 13);
         this.label8.TabIndex = 10;
         this.label8.Text = "Image Size (pixels)";
         // 
         // label7
         // 
         this.label7.AutoSize = true;
         this.label7.Location = new System.Drawing.Point(12, 151);
         this.label7.Name = "label7";
         this.label7.Size = new System.Drawing.Size(104, 13);
         this.label7.TabIndex = 12;
         this.label7.Text = "Image File Extension";
         // 
         // label6
         // 
         this.label6.AutoSize = true;
         this.label6.Location = new System.Drawing.Point(12, 174);
         this.label6.Name = "label6";
         this.label6.Size = new System.Drawing.Size(63, 13);
         this.label6.TabIndex = 14;
         this.label6.Text = "Server URL";
         // 
         // label5
         // 
         this.label5.AutoSize = true;
         this.label5.Location = new System.Drawing.Point(12, 107);
         this.label5.Name = "label5";
         this.label5.Size = new System.Drawing.Size(38, 13);
         this.label5.TabIndex = 8;
         this.label5.Text = "Levels";
         // 
         // label4
         // 
         this.label4.AutoSize = true;
         this.label4.Location = new System.Drawing.Point(12, 85);
         this.label4.Name = "label4";
         this.label4.Size = new System.Drawing.Size(121, 13);
         this.label4.TabIndex = 6;
         this.label4.Text = "Initial Tile Size (degrees)";
         // 
         // label2
         // 
         this.label2.AutoSize = true;
         this.label2.Location = new System.Drawing.Point(12, 62);
         this.label2.Name = "label2";
         this.label2.Size = new System.Drawing.Size(55, 13);
         this.label2.TabIndex = 4;
         this.label2.Text = "Height (m)";
         // 
         // label1
         // 
         this.label1.AutoSize = true;
         this.label1.Location = new System.Drawing.Point(12, 38);
         this.label1.Name = "label1";
         this.label1.Size = new System.Drawing.Size(35, 13);
         this.label1.TabIndex = 1;
         this.label1.Text = "Name";
         // 
         // linkLabelHelpTile
         // 
         this.linkLabelHelpTile.AutoSize = true;
         this.linkLabelHelpTile.Location = new System.Drawing.Point(12, 246);
         this.linkLabelHelpTile.Name = "linkLabelHelpTile";
         this.linkLabelHelpTile.Size = new System.Drawing.Size(126, 13);
         this.linkLabelHelpTile.TabIndex = 22;
         this.linkLabelHelpTile.TabStop = true;
         this.linkLabelHelpTile.Text = "What does all this mean?";
         this.linkLabelHelpTile.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelHelpTile_LinkClicked);
         // 
         // butOK
         // 
         this.butOK.DialogResult = System.Windows.Forms.DialogResult.OK;
         this.butOK.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
         this.butOK.Location = new System.Drawing.Point(253, 241);
         this.butOK.Name = "butOK";
         this.butOK.Size = new System.Drawing.Size(75, 23);
         this.butOK.TabIndex = 23;
         this.butOK.Text = "&OK";
         this.butOK.UseVisualStyleBackColor = true;
         this.butOK.Click += new System.EventHandler(this.butOK_Click);
         // 
         // butCancel
         // 
         this.butCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
         this.butCancel.Location = new System.Drawing.Point(334, 241);
         this.butCancel.Name = "butCancel";
         this.butCancel.Size = new System.Drawing.Size(75, 23);
         this.butCancel.TabIndex = 24;
         this.butCancel.Text = "C&ancel";
         this.butCancel.UseVisualStyleBackColor = true;
         this.butCancel.Click += new System.EventHandler(this.butCancel_Click_1);
         // 
         // AddImageTile
         // 
         this.AcceptButton = this.butOK;
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.CancelButton = this.butCancel;
         this.ClientSize = new System.Drawing.Size(416, 269);
         this.Controls.Add(this.linkLabelHelpTile);
         this.Controls.Add(this.butOK);
         this.Controls.Add(this.butCancel);
         this.Controls.Add(this.label12);
         this.Controls.Add(this.cmbTileServerFileExtension);
         this.Controls.Add(this.butLogoPathBrowse);
         this.Controls.Add(this.numHeight);
         this.Controls.Add(this.txtName);
         this.Controls.Add(this.txtLogoPath);
         this.Controls.Add(this.txtDatabaseName);
         this.Controls.Add(this.txtServerURL);
         this.Controls.Add(this.chkTileServerUseTerrainMap);
         this.Controls.Add(this.numTileSize);
         this.Controls.Add(this.numLevels);
         this.Controls.Add(this.numImagePixelSize);
         this.Controls.Add(this.grpExtents);
         this.Controls.Add(this.label10);
         this.Controls.Add(this.label9);
         this.Controls.Add(this.label8);
         this.Controls.Add(this.label7);
         this.Controls.Add(this.label6);
         this.Controls.Add(this.label5);
         this.Controls.Add(this.label4);
         this.Controls.Add(this.label2);
         this.Controls.Add(this.label1);
         this.MaximizeBox = false;
         this.MinimizeBox = false;
         this.Name = "AddImageTile";
         this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
         this.Text = "Add a Server";
         ((System.ComponentModel.ISupportInitialize)(this.errProvider)).EndInit();
         ((System.ComponentModel.ISupportInitialize)(this.numS)).EndInit();
         ((System.ComponentModel.ISupportInitialize)(this.numE)).EndInit();
         ((System.ComponentModel.ISupportInitialize)(this.numW)).EndInit();
         ((System.ComponentModel.ISupportInitialize)(this.numN)).EndInit();
         ((System.ComponentModel.ISupportInitialize)(this.numHeight)).EndInit();
         ((System.ComponentModel.ISupportInitialize)(this.numTileSize)).EndInit();
         ((System.ComponentModel.ISupportInitialize)(this.numLevels)).EndInit();
         ((System.ComponentModel.ISupportInitialize)(this.numImagePixelSize)).EndInit();
         this.grpExtents.ResumeLayout(false);
         this.ResumeLayout(false);
         this.PerformLayout();

      }

      #endregion

      private System.Windows.Forms.ErrorProvider errProvider;
      private System.Windows.Forms.Label label12;
      private System.Windows.Forms.ComboBox cmbTileServerFileExtension;
      private System.Windows.Forms.Button butLogoPathBrowse;
      private System.Windows.Forms.NumericUpDown numHeight;
      private System.Windows.Forms.TextBox txtName;
      private System.Windows.Forms.TextBox txtLogoPath;
      private System.Windows.Forms.TextBox txtDatabaseName;
      private System.Windows.Forms.TextBox txtServerURL;
      private System.Windows.Forms.CheckBox chkTileServerUseTerrainMap;
      private System.Windows.Forms.NumericUpDown numTileSize;
      private System.Windows.Forms.NumericUpDown numLevels;
      private System.Windows.Forms.NumericUpDown numImagePixelSize;
      private System.Windows.Forms.GroupBox grpExtents;
      private System.Windows.Forms.NumericUpDown numS;
      private System.Windows.Forms.NumericUpDown numE;
      private System.Windows.Forms.NumericUpDown numW;
      private System.Windows.Forms.NumericUpDown numN;
      private System.Windows.Forms.Label label10;
      private System.Windows.Forms.Label label9;
      private System.Windows.Forms.Label label8;
      private System.Windows.Forms.Label label7;
      private System.Windows.Forms.Label label6;
      private System.Windows.Forms.Label label5;
      private System.Windows.Forms.Label label4;
      private System.Windows.Forms.Label label2;
      private System.Windows.Forms.Label label1;
      private System.Windows.Forms.Button butOK;
      private System.Windows.Forms.Button butCancel;
      private System.Windows.Forms.LinkLabel linkLabelHelpTile;
   }
}