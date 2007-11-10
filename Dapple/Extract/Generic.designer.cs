namespace Dapple.Extract
{
   partial class Generic
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
         this.lNoOptions.AutoSize = true;
         this.lNoOptions.Location = new System.Drawing.Point(3, 0);
         this.lNoOptions.Name = "lNoOptions";
         this.lNoOptions.Size = new System.Drawing.Size(60, 13);
         this.lNoOptions.TabIndex = 0;
         this.lNoOptions.Text = "No Options";
         // 
         // Generic
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.Controls.Add(this.lNoOptions);
         this.Name = "Generic";
         this.Size = new System.Drawing.Size(200, 300);
         this.ResumeLayout(false);
         this.PerformLayout();

      }

      #endregion

      private System.Windows.Forms.Label lNoOptions;
   }
}
