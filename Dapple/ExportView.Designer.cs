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
         this.checkKeepLayers = new System.Windows.Forms.CheckBox();
         this.labelName = new System.Windows.Forms.Label();
         this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
         this.btnCancel = new System.Windows.Forms.Button();
         this.btnOK = new System.Windows.Forms.Button();
         this.label2 = new System.Windows.Forms.Label();
         this.cmbRes = new System.Windows.Forms.ComboBox();
         this.SuspendLayout();
         // 
         // checkKeepLayers
         // 
         this.checkKeepLayers.AutoSize = true;
         this.checkKeepLayers.Location = new System.Drawing.Point(89, 68);
         this.checkKeepLayers.Name = "checkKeepLayers";
         this.checkKeepLayers.Size = new System.Drawing.Size(128, 17);
         this.checkKeepLayers.TabIndex = 28;
         this.checkKeepLayers.Text = "Keep individual layers";
         this.checkKeepLayers.UseVisualStyleBackColor = true;
         // 
         // labelName
         // 
         this.labelName.AutoSize = true;
         this.labelName.Location = new System.Drawing.Point(51, 15);
         this.labelName.Name = "labelName";
         this.labelName.Size = new System.Drawing.Size(35, 13);
         this.labelName.TabIndex = 13;
         this.labelName.Text = "Name";
         this.labelName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
         // 
         // btnCancel
         // 
         this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
         this.btnCancel.CausesValidation = false;
         this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
         this.btnCancel.Location = new System.Drawing.Point(213, 93);
         this.btnCancel.Name = "btnCancel";
         this.btnCancel.Size = new System.Drawing.Size(75, 23);
         this.btnCancel.TabIndex = 30;
         this.btnCancel.Text = "C&ancel";
         this.btnCancel.UseVisualStyleBackColor = true;
         // 
         // btnOK
         // 
         this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
         this.btnOK.Location = new System.Drawing.Point(132, 93);
         this.btnOK.Name = "btnOK";
         this.btnOK.Size = new System.Drawing.Size(75, 23);
         this.btnOK.TabIndex = 29;
         this.btnOK.Text = "&OK";
         this.btnOK.UseVisualStyleBackColor = true;
         this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
         // 
         // label2
         // 
         this.label2.AutoSize = true;
         this.label2.Location = new System.Drawing.Point(29, 45);
         this.label2.Name = "label2";
         this.label2.Size = new System.Drawing.Size(57, 13);
         this.label2.TabIndex = 26;
         this.label2.Text = "Resolution";
         this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
         // 
         // cmbRes
         // 
         this.cmbRes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                     | System.Windows.Forms.AnchorStyles.Right)));
         this.cmbRes.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Append;
         this.cmbRes.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
         this.cmbRes.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
         this.cmbRes.FormattingEnabled = true;
         this.cmbRes.Items.AddRange(new object[] {
            "Low (1024x1024)",
            "Medium (1600x1600)",
            "High (2048x2048)",
            "Full (highest detail in view)"});
         this.cmbRes.Location = new System.Drawing.Point(89, 41);
         this.cmbRes.Name = "cmbRes";
         this.cmbRes.Size = new System.Drawing.Size(198, 21);
         this.cmbRes.TabIndex = 27;
         // 
         // ExportView
         // 
         this.AcceptButton = this.btnOK;
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(300, 119);
         this.Controls.Add(this.label2);
         this.Controls.Add(this.cmbRes);
         this.Controls.Add(this.btnCancel);
         this.Controls.Add(this.btnOK);
         this.Controls.Add(this.checkKeepLayers);
         this.Controls.Add(this.labelName);
         this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
         this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
         this.MaximizeBox = false;
         this.MinimizeBox = false;
         this.Name = "ExportView";
         this.ShowInTaskbar = false;
         this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
         this.Text = "Export Current Dapple View";
         this.Load += new System.EventHandler(this.ExportView_Load);
         this.ResumeLayout(false);
         this.PerformLayout();

      }

      #endregion

      private System.Windows.Forms.CheckBox checkKeepLayers;
      private System.Windows.Forms.Label labelName;
      private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
      private System.Windows.Forms.Button btnCancel;
      private System.Windows.Forms.Button btnOK;
      private System.Windows.Forms.Label label2;
      private System.Windows.Forms.ComboBox cmbRes;
   }
}