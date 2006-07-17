namespace Dapple
{
   partial class SaveViewForm
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
         this.btnOK = new System.Windows.Forms.Button();
         this.picPreview = new System.Windows.Forms.PictureBox();
         this.btnCancel = new System.Windows.Forms.Button();
         this.txtNotes = new System.Windows.Forms.TextBox();
         this.labelNotes = new System.Windows.Forms.Label();
         this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
         this.labelName = new System.Windows.Forms.Label();
         ((System.ComponentModel.ISupportInitialize)(this.picPreview)).BeginInit();
         this.SuspendLayout();
         // 
         // btnOK
         // 
         this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
         this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
         this.btnOK.Location = new System.Drawing.Point(263, 405);
         this.btnOK.Name = "btnOK";
         this.btnOK.Size = new System.Drawing.Size(75, 23);
         this.btnOK.TabIndex = 2;
         this.btnOK.Text = "&OK";
         this.btnOK.UseVisualStyleBackColor = true;
         this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
         // 
         // picPreview
         // 
         this.picPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                     | System.Windows.Forms.AnchorStyles.Left)
                     | System.Windows.Forms.AnchorStyles.Right)));
         this.picPreview.Location = new System.Drawing.Point(1, 1);
         this.picPreview.Name = "picPreview";
         this.picPreview.Size = new System.Drawing.Size(429, 347);
         this.picPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
         this.picPreview.TabIndex = 3;
         this.picPreview.TabStop = false;
         this.picPreview.Paint += new System.Windows.Forms.PaintEventHandler(this.picPreview_Paint);
         // 
         // btnCancel
         // 
         this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
         this.btnCancel.CausesValidation = false;
         this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
         this.btnCancel.Location = new System.Drawing.Point(344, 405);
         this.btnCancel.Name = "btnCancel";
         this.btnCancel.Size = new System.Drawing.Size(75, 23);
         this.btnCancel.TabIndex = 3;
         this.btnCancel.Text = "C&ancel";
         this.btnCancel.UseVisualStyleBackColor = true;
         // 
         // txtNotes
         // 
         this.txtNotes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                     | System.Windows.Forms.AnchorStyles.Right)));
         this.txtNotes.Location = new System.Drawing.Point(47, 383);
         this.txtNotes.Multiline = true;
         this.txtNotes.Name = "txtNotes";
         this.txtNotes.Size = new System.Drawing.Size(202, 45);
         this.txtNotes.TabIndex = 1;
         // 
         // labelNotes
         // 
         this.labelNotes.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
         this.labelNotes.AutoSize = true;
         this.labelNotes.Location = new System.Drawing.Point(7, 386);
         this.labelNotes.Name = "labelNotes";
         this.labelNotes.Size = new System.Drawing.Size(35, 13);
         this.labelNotes.TabIndex = 3;
         this.labelNotes.Text = "Notes";
         // 
         // labelName
         // 
         this.labelName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
         this.labelName.AutoSize = true;
         this.labelName.Location = new System.Drawing.Point(7, 361);
         this.labelName.Name = "labelName";
         this.labelName.Size = new System.Drawing.Size(35, 13);
         this.labelName.TabIndex = 0;
         this.labelName.Text = "Name";
         // 
         // SaveViewForm
         // 
         this.AcceptButton = this.btnOK;
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.CancelButton = this.btnCancel;
         this.ClientSize = new System.Drawing.Size(431, 434);
         this.Controls.Add(this.labelName);
         this.Controls.Add(this.labelNotes);
         this.Controls.Add(this.txtNotes);
         this.Controls.Add(this.btnCancel);
         this.Controls.Add(this.picPreview);
         this.Controls.Add(this.btnOK);
         this.MaximizeBox = false;
         this.MinimizeBox = false;
         this.Name = "SaveViewForm";
         this.ShowInTaskbar = false;
         this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
         this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
         this.Text = "Save Current View";
         ((System.ComponentModel.ISupportInitialize)(this.picPreview)).EndInit();
         this.ResumeLayout(false);
         this.PerformLayout();

      }

      #endregion

      private System.Windows.Forms.Button btnOK;
      private System.Windows.Forms.PictureBox picPreview;
      private System.Windows.Forms.Button btnCancel;
      private System.Windows.Forms.TextBox txtNotes;
      private System.Windows.Forms.Label labelNotes;
      private System.Windows.Forms.SaveFileDialog saveFileDialog;
      private System.Windows.Forms.Label labelName;
   }
}