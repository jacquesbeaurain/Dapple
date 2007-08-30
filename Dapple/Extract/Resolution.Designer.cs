namespace Dapple.Extract
{
   partial class Resolution
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
         this.lMaxResolution = new System.Windows.Forms.Label();
         this.lMinResolution = new System.Windows.Forms.Label();
         this.lSize = new System.Windows.Forms.Label();
         this.label1 = new System.Windows.Forms.Label();
         this.lUnit = new System.Windows.Forms.Label();
         this.tbRes = new System.Windows.Forms.TextBox();
         this.tbResolution = new System.Windows.Forms.TrackBar();
         this.lResolution = new System.Windows.Forms.Label();
         ((System.ComponentModel.ISupportInitialize)(this.tbResolution)).BeginInit();
         this.SuspendLayout();
         // 
         // lMaxResolution
         // 
         this.lMaxResolution.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
         this.lMaxResolution.Location = new System.Drawing.Point(208, 79);
         this.lMaxResolution.Name = "lMaxResolution";
         this.lMaxResolution.Size = new System.Drawing.Size(113, 13);
         this.lMaxResolution.TabIndex = 6;
         this.lMaxResolution.Text = "10";
         this.lMaxResolution.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
         // 
         // lMinResolution
         // 
         this.lMinResolution.AutoSize = true;
         this.lMinResolution.Location = new System.Drawing.Point(86, 79);
         this.lMinResolution.Name = "lMinResolution";
         this.lMinResolution.Size = new System.Drawing.Size(19, 13);
         this.lMinResolution.TabIndex = 5;
         this.lMinResolution.Text = "10";
         this.lMinResolution.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
         // 
         // lSize
         // 
         this.lSize.AutoSize = true;
         this.lSize.Location = new System.Drawing.Point(185, 97);
         this.lSize.Name = "lSize";
         this.lSize.Size = new System.Drawing.Size(19, 13);
         this.lSize.TabIndex = 4;
         this.lSize.Text = "10";
         // 
         // label1
         // 
         this.label1.AutoSize = true;
         this.label1.Location = new System.Drawing.Point(86, 97);
         this.label1.Name = "label1";
         this.label1.Size = new System.Drawing.Size(93, 13);
         this.label1.TabIndex = 3;
         this.label1.Text = "Estimated file size:";
         // 
         // lUnit
         // 
         this.lUnit.AutoSize = true;
         this.lUnit.Location = new System.Drawing.Point(156, 8);
         this.lUnit.Name = "lUnit";
         this.lUnit.Size = new System.Drawing.Size(15, 13);
         this.lUnit.TabIndex = 2;
         this.lUnit.Text = "m";
         // 
         // tbRes
         // 
         this.tbRes.Location = new System.Drawing.Point(89, 5);
         this.tbRes.Name = "tbRes";
         this.tbRes.Size = new System.Drawing.Size(61, 20);
         this.tbRes.TabIndex = 1;
         this.tbRes.TextChanged += new System.EventHandler(this.tbRes_TextChanged);
         // 
         // tbResolution
         // 
         this.tbResolution.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                     | System.Windows.Forms.AnchorStyles.Right)));
         this.tbResolution.Location = new System.Drawing.Point(89, 31);
         this.tbResolution.Maximum = 100;
         this.tbResolution.Name = "tbResolution";
         this.tbResolution.Size = new System.Drawing.Size(242, 45);
         this.tbResolution.TabIndex = 0;
         this.tbResolution.TickFrequency = 5;
         this.tbResolution.Scroll += new System.EventHandler(this.tbResolution_Scroll);
         // 
         // lResolution
         // 
         this.lResolution.AutoSize = true;
         this.lResolution.Location = new System.Drawing.Point(-3, 8);
         this.lResolution.Name = "lResolution";
         this.lResolution.Size = new System.Drawing.Size(60, 13);
         this.lResolution.TabIndex = 7;
         this.lResolution.Text = "Resolution:";
         // 
         // Resolution
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.Controls.Add(this.lResolution);
         this.Controls.Add(this.lMaxResolution);
         this.Controls.Add(this.lMinResolution);
         this.Controls.Add(this.tbResolution);
         this.Controls.Add(this.tbRes);
         this.Controls.Add(this.lUnit);
         this.Controls.Add(this.lSize);
         this.Controls.Add(this.label1);
         this.Name = "Resolution";
         this.Size = new System.Drawing.Size(334, 124);
         ((System.ComponentModel.ISupportInitialize)(this.tbResolution)).EndInit();
         this.ResumeLayout(false);
         this.PerformLayout();

      }

      #endregion

      private System.Windows.Forms.Label label1;
      private System.Windows.Forms.Label lUnit;
      private System.Windows.Forms.TextBox tbRes;
      private System.Windows.Forms.TrackBar tbResolution;
      private System.Windows.Forms.Label lSize;
      private System.Windows.Forms.Label lMaxResolution;
      private System.Windows.Forms.Label lMinResolution;
      private System.Windows.Forms.Label lResolution;
   }
}
