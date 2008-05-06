namespace Dapple
{
   partial class PageNavigator
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
			this.c_lStatusMessage = new System.Windows.Forms.Label();
			this.c_bForward = new System.Windows.Forms.Button();
			this.c_bBack = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// c_lStatusMessage
			// 
			this.c_lStatusMessage.Dock = System.Windows.Forms.DockStyle.Fill;
			this.c_lStatusMessage.Location = new System.Drawing.Point(0, 0);
			this.c_lStatusMessage.Name = "c_lStatusMessage";
			this.c_lStatusMessage.Size = new System.Drawing.Size(200, 23);
			this.c_lStatusMessage.TabIndex = 1;
			this.c_lStatusMessage.Text = "Results 1-10 of 2000";
			this.c_lStatusMessage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// c_bForward
			// 
			this.c_bForward.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.c_bForward.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.c_bForward.Image = global::Dapple.Properties.Resources.next;
			this.c_bForward.Location = new System.Drawing.Point(177, 0);
			this.c_bForward.Name = "c_bForward";
			this.c_bForward.Size = new System.Drawing.Size(23, 23);
			this.c_bForward.TabIndex = 2;
			this.c_bForward.UseVisualStyleBackColor = true;
			this.c_bForward.Click += new System.EventHandler(this.cForwardButton_Click);
			// 
			// c_bBack
			// 
			this.c_bBack.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.c_bBack.Image = global::Dapple.Properties.Resources.previous;
			this.c_bBack.Location = new System.Drawing.Point(0, 0);
			this.c_bBack.Name = "c_bBack";
			this.c_bBack.Size = new System.Drawing.Size(23, 23);
			this.c_bBack.TabIndex = 0;
			this.c_bBack.UseVisualStyleBackColor = true;
			this.c_bBack.Click += new System.EventHandler(this.cBackButton_Click);
			// 
			// PageNavigator
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.c_bBack);
			this.Controls.Add(this.c_bForward);
			this.Controls.Add(this.c_lStatusMessage);
			this.Name = "PageNavigator";
			this.Size = new System.Drawing.Size(200, 23);
			this.ResumeLayout(false);

      }

      #endregion

      private System.Windows.Forms.Label c_lStatusMessage;
      private System.Windows.Forms.Button c_bForward;
      private System.Windows.Forms.Button c_bBack;
   }
}
