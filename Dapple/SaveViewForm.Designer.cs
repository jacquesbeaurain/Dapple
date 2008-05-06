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
			this.c_bOK = new System.Windows.Forms.Button();
			this.c_pbPreview = new System.Windows.Forms.PictureBox();
			this.c_bCancel = new System.Windows.Forms.Button();
			this.c_tbNotes = new System.Windows.Forms.TextBox();
			this.c_lNotes = new System.Windows.Forms.Label();
			this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
			this.c_lName = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.c_pbPreview)).BeginInit();
			this.SuspendLayout();
			// 
			// c_bOK
			// 
			this.c_bOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.c_bOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.c_bOK.Location = new System.Drawing.Point(263, 405);
			this.c_bOK.Name = "c_bOK";
			this.c_bOK.Size = new System.Drawing.Size(75, 23);
			this.c_bOK.TabIndex = 2;
			this.c_bOK.Text = "&OK";
			this.c_bOK.UseVisualStyleBackColor = true;
			this.c_bOK.Click += new System.EventHandler(this.btnOK_Click);
			// 
			// c_pbPreview
			// 
			this.c_pbPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
							| System.Windows.Forms.AnchorStyles.Left)
							| System.Windows.Forms.AnchorStyles.Right)));
			this.c_pbPreview.Location = new System.Drawing.Point(0, 0);
			this.c_pbPreview.Name = "c_pbPreview";
			this.c_pbPreview.Size = new System.Drawing.Size(429, 347);
			this.c_pbPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.c_pbPreview.TabIndex = 3;
			this.c_pbPreview.TabStop = false;
			this.c_pbPreview.Paint += new System.Windows.Forms.PaintEventHandler(this.picPreview_Paint);
			// 
			// c_bCancel
			// 
			this.c_bCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.c_bCancel.CausesValidation = false;
			this.c_bCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.c_bCancel.Location = new System.Drawing.Point(344, 405);
			this.c_bCancel.Name = "c_bCancel";
			this.c_bCancel.Size = new System.Drawing.Size(75, 23);
			this.c_bCancel.TabIndex = 3;
			this.c_bCancel.Text = "C&ancel";
			this.c_bCancel.UseVisualStyleBackColor = true;
			// 
			// c_tbNotes
			// 
			this.c_tbNotes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
							| System.Windows.Forms.AnchorStyles.Right)));
			this.c_tbNotes.Location = new System.Drawing.Point(47, 383);
			this.c_tbNotes.Multiline = true;
			this.c_tbNotes.Name = "c_tbNotes";
			this.c_tbNotes.Size = new System.Drawing.Size(202, 45);
			this.c_tbNotes.TabIndex = 1;
			// 
			// c_lNotes
			// 
			this.c_lNotes.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.c_lNotes.AutoSize = true;
			this.c_lNotes.Location = new System.Drawing.Point(7, 386);
			this.c_lNotes.Name = "c_lNotes";
			this.c_lNotes.Size = new System.Drawing.Size(35, 13);
			this.c_lNotes.TabIndex = 3;
			this.c_lNotes.Text = "Notes";
			// 
			// c_lName
			// 
			this.c_lName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.c_lName.AutoSize = true;
			this.c_lName.Location = new System.Drawing.Point(7, 361);
			this.c_lName.Name = "c_lName";
			this.c_lName.Size = new System.Drawing.Size(35, 13);
			this.c_lName.TabIndex = 0;
			this.c_lName.Text = "Name";
			// 
			// SaveViewForm
			// 
			this.AcceptButton = this.c_bOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.c_bCancel;
			this.ClientSize = new System.Drawing.Size(431, 434);
			this.Controls.Add(this.c_pbPreview);
			this.Controls.Add(this.c_lName);
			this.Controls.Add(this.c_lNotes);
			this.Controls.Add(this.c_tbNotes);
			this.Controls.Add(this.c_bCancel);
			this.Controls.Add(this.c_bOK);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "SaveViewForm";
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Save Current View";
			((System.ComponentModel.ISupportInitialize)(this.c_pbPreview)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

      }

      #endregion

      private System.Windows.Forms.Button c_bOK;
      private System.Windows.Forms.PictureBox c_pbPreview;
      private System.Windows.Forms.Button c_bCancel;
      private System.Windows.Forms.TextBox c_tbNotes;
      private System.Windows.Forms.Label c_lNotes;
      private System.Windows.Forms.SaveFileDialog saveFileDialog;
      private System.Windows.Forms.Label c_lName;
   }
}