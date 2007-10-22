namespace Dapple
{
   partial class AddWMS
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
         this.label11 = new System.Windows.Forms.Label();
         this.butOK = new System.Windows.Forms.Button();
         this.txtWmsURL = new System.Windows.Forms.TextBox();
         this.butCancel = new System.Windows.Forms.Button();
         this.linkLabelHelpWMS = new System.Windows.Forms.LinkLabel();
         this.SuspendLayout();
         // 
         // label11
         // 
         this.label11.AutoSize = true;
         this.label11.Location = new System.Drawing.Point(12, 9);
         this.label11.Name = "label11";
         this.label11.Size = new System.Drawing.Size(368, 13);
         this.label11.TabIndex = 0;
         this.label11.Text = "Please enter the Capabilities URL for the WMS Server you would like to add:";
         // 
         // butOK
         // 
         this.butOK.DialogResult = System.Windows.Forms.DialogResult.OK;
         this.butOK.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
         this.butOK.Location = new System.Drawing.Point(224, 61);
         this.butOK.Name = "butOK";
         this.butOK.Size = new System.Drawing.Size(75, 23);
         this.butOK.TabIndex = 3;
         this.butOK.Text = "&OK";
         this.butOK.UseVisualStyleBackColor = true;
         this.butOK.Click += new System.EventHandler(this.butOK_Click);
         // 
         // txtWmsURL
         // 
         this.txtWmsURL.Location = new System.Drawing.Point(15, 35);
         this.txtWmsURL.Name = "txtWmsURL";
         this.txtWmsURL.Size = new System.Drawing.Size(365, 20);
         this.txtWmsURL.TabIndex = 1;
         this.txtWmsURL.Text = "http://";
         // 
         // butCancel
         // 
         this.butCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
         this.butCancel.Location = new System.Drawing.Point(305, 61);
         this.butCancel.Name = "butCancel";
         this.butCancel.Size = new System.Drawing.Size(75, 23);
         this.butCancel.TabIndex = 4;
         this.butCancel.Text = "C&ancel";
         this.butCancel.UseVisualStyleBackColor = true;
         // 
         // linkLabelHelpWMS
         // 
         this.linkLabelHelpWMS.AutoSize = true;
         this.linkLabelHelpWMS.Location = new System.Drawing.Point(12, 66);
         this.linkLabelHelpWMS.Name = "linkLabelHelpWMS";
         this.linkLabelHelpWMS.Size = new System.Drawing.Size(169, 13);
         this.linkLabelHelpWMS.TabIndex = 2;
         this.linkLabelHelpWMS.TabStop = true;
         this.linkLabelHelpWMS.Text = "What is a WMS Capabilities URL?";
         this.linkLabelHelpWMS.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelHelpWMS_LinkClicked);
         // 
         // AddWMS
         // 
         this.AcceptButton = this.butOK;
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.CancelButton = this.butCancel;
         this.ClientSize = new System.Drawing.Size(388, 90);
         this.ControlBox = false;
         this.Controls.Add(this.linkLabelHelpWMS);
         this.Controls.Add(this.label11);
         this.Controls.Add(this.butOK);
         this.Controls.Add(this.txtWmsURL);
         this.Controls.Add(this.butCancel);
         this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
         this.Name = "AddWMS";
         this.ShowInTaskbar = false;
         this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
         this.Text = "Add a WMS Server";
         this.Load += new System.EventHandler(this.AddWMS_Load);
         this.ResumeLayout(false);
         this.PerformLayout();

      }

      #endregion

      private System.Windows.Forms.Label label11;
      private System.Windows.Forms.Button butOK;
      private System.Windows.Forms.TextBox txtWmsURL;
      private System.Windows.Forms.Button butCancel;
      private System.Windows.Forms.LinkLabel linkLabelHelpWMS;
   }
}