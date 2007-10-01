namespace Dapple.CustomControls
{
   partial class JanaTab
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
         this.cTabToolbar = new Dapple.CustomControls.TabToolStrip();
         this.SuspendLayout();
         // 
         // cTabToolbar
         // 
         this.cTabToolbar.Dock = System.Windows.Forms.DockStyle.Bottom;
         this.cTabToolbar.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
         this.cTabToolbar.Location = new System.Drawing.Point(0, 125);
         this.cTabToolbar.Name = "cTabToolbar";
         this.cTabToolbar.Size = new System.Drawing.Size(150, 25);
         this.cTabToolbar.TabIndex = 1;
         this.cTabToolbar.TabStop = true;
         this.cTabToolbar.Text = "tabToolbar1";
         // 
         // JanaTab
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.Controls.Add(this.cTabToolbar);
         this.Name = "JanaTab";
         this.ResumeLayout(false);
         this.PerformLayout();

      }

      #endregion

      private TabToolStrip cTabToolbar;
   }
}
