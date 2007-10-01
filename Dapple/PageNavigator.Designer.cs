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
         this.cPageLabel = new System.Windows.Forms.Label();
         this.cForwardButton = new System.Windows.Forms.Button();
         this.cBackButton = new System.Windows.Forms.Button();
         this.SuspendLayout();
         // 
         // cPageLabel
         // 
         this.cPageLabel.Dock = System.Windows.Forms.DockStyle.Fill;
         this.cPageLabel.Location = new System.Drawing.Point(0, 0);
         this.cPageLabel.Name = "cPageLabel";
         this.cPageLabel.Size = new System.Drawing.Size(200, 23);
         this.cPageLabel.TabIndex = 1;
         this.cPageLabel.Text = "Results 1-10 of 2000";
         this.cPageLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
         // 
         // cForwardButton
         // 
         this.cForwardButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
         this.cForwardButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
         this.cForwardButton.Image = global::Dapple.Properties.Resources.next;
         this.cForwardButton.Location = new System.Drawing.Point(177, 0);
         this.cForwardButton.Name = "cForwardButton";
         this.cForwardButton.Size = new System.Drawing.Size(23, 23);
         this.cForwardButton.TabIndex = 2;
         this.cForwardButton.UseVisualStyleBackColor = true;
         this.cForwardButton.Click += new System.EventHandler(this.cForwardButton_Click);
         // 
         // cBackButton
         // 
         this.cBackButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
         this.cBackButton.Image = global::Dapple.Properties.Resources.previous;
         this.cBackButton.Location = new System.Drawing.Point(0, 0);
         this.cBackButton.Name = "cBackButton";
         this.cBackButton.Size = new System.Drawing.Size(23, 23);
         this.cBackButton.TabIndex = 0;
         this.cBackButton.UseVisualStyleBackColor = true;
         this.cBackButton.Click += new System.EventHandler(this.cBackButton_Click);
         // 
         // PageNavigator
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.Controls.Add(this.cBackButton);
         this.Controls.Add(this.cForwardButton);
         this.Controls.Add(this.cPageLabel);
         this.Name = "PageNavigator";
         this.Size = new System.Drawing.Size(200, 23);
         this.ResumeLayout(false);

      }

      #endregion

      private System.Windows.Forms.Label cPageLabel;
      private System.Windows.Forms.Button cForwardButton;
      private System.Windows.Forms.Button cBackButton;
   }
}
