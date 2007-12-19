namespace Dapple.Extract
{
	partial class Disabled
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
			this.lNoOptions = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// lNoOptions
			// 
			this.lNoOptions.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
							| System.Windows.Forms.AnchorStyles.Left)
							| System.Windows.Forms.AnchorStyles.Right)));
			this.lNoOptions.Location = new System.Drawing.Point(3, 0);
			this.lNoOptions.Name = "lNoOptions";
			this.lNoOptions.Size = new System.Drawing.Size(163, 300);
			this.lNoOptions.TabIndex = 0;
			this.lNoOptions.Text = "This data layer will not be extracted.";
			// 
			// Disabled
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.lNoOptions);
			this.Name = "Disabled";
			this.Size = new System.Drawing.Size(169, 300);
			this.ResumeLayout(false);

      }

      #endregion

      private System.Windows.Forms.Label lNoOptions;
   }
}
