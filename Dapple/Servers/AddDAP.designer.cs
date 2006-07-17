namespace Dapple
{
   partial class AddDAP
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
         this.butOK = new System.Windows.Forms.Button();
         this.butCancel = new System.Windows.Forms.Button();
         this.label11 = new System.Windows.Forms.Label();
         this.txtDapURL = new System.Windows.Forms.TextBox();
         this.linkLabelHelpDAP = new System.Windows.Forms.LinkLabel();
         this.SuspendLayout();
         // 
         // butOK
         // 
         this.butOK.DialogResult = System.Windows.Forms.DialogResult.OK;
         this.butOK.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
         this.butOK.Location = new System.Drawing.Point(210, 61);
         this.butOK.Name = "butOK";
         this.butOK.Size = new System.Drawing.Size(75, 23);
         this.butOK.TabIndex = 2;
         this.butOK.Text = "&OK";
         this.butOK.UseVisualStyleBackColor = true;
         this.butOK.Click += new System.EventHandler(this.butOK_Click);
         // 
         // butCancel
         // 
         this.butCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
         this.butCancel.Location = new System.Drawing.Point(291, 61);
         this.butCancel.Name = "butCancel";
         this.butCancel.Size = new System.Drawing.Size(75, 23);
         this.butCancel.TabIndex = 3;
         this.butCancel.Text = "C&ancel";
         this.butCancel.UseVisualStyleBackColor = true;
         // 
         // label11
         // 
         this.label11.AutoSize = true;
         this.label11.Location = new System.Drawing.Point(12, 9);
         this.label11.Name = "label11";
         this.label11.Size = new System.Drawing.Size(307, 13);
         this.label11.TabIndex = 0;
         this.label11.Text = "Please enter the URL for the DAP Server you would like to add:";
         // 
         // txtDapURL
         // 
         this.txtDapURL.Location = new System.Drawing.Point(15, 35);
         this.txtDapURL.Name = "txtDapURL";
         this.txtDapURL.Size = new System.Drawing.Size(351, 20);
         this.txtDapURL.TabIndex = 1;
         this.txtDapURL.Text = "http://";
         // 
         // linkLabelHelpDAP
         // 
         this.linkLabelHelpDAP.AutoSize = true;
         this.linkLabelHelpDAP.Location = new System.Drawing.Point(12, 66);
         this.linkLabelHelpDAP.Name = "linkLabelHelpDAP";
         this.linkLabelHelpDAP.Size = new System.Drawing.Size(117, 13);
         this.linkLabelHelpDAP.TabIndex = 4;
         this.linkLabelHelpDAP.TabStop = true;
         this.linkLabelHelpDAP.Text = "What is a DAP Server?";
         this.linkLabelHelpDAP.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelHelpDAP_LinkClicked);
         // 
         // AddDAP
         // 
         this.AcceptButton = this.butOK;
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.CancelButton = this.butCancel;
         this.ClientSize = new System.Drawing.Size(377, 92);
         this.Controls.Add(this.linkLabelHelpDAP);
         this.Controls.Add(this.label11);
         this.Controls.Add(this.butOK);
         this.Controls.Add(this.txtDapURL);
         this.Controls.Add(this.butCancel);
         this.MaximizeBox = false;
         this.MinimizeBox = false;
         this.Name = "AddDAP";
         this.ShowInTaskbar = false;
         this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
         this.Text = "Add a DAP Server";
         this.ResumeLayout(false);
         this.PerformLayout();

      }

      #endregion

      private System.Windows.Forms.Button butOK;
      private System.Windows.Forms.Button butCancel;
      private System.Windows.Forms.Label label11;
      private System.Windows.Forms.TextBox txtDapURL;
      private System.Windows.Forms.LinkLabel linkLabelHelpDAP;
   }
}