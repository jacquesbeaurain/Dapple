namespace Dapple
{
   partial class ViewOpenDialog
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
			this.c_bCancel = new System.Windows.Forms.Button();
			this.c_bOK = new System.Windows.Forms.Button();
			this.c_pbPreview = new System.Windows.Forms.PictureBox();
			this.c_gbNotes = new System.Windows.Forms.GroupBox();
			this.c_lNotes = new System.Windows.Forms.Label();
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.c_lView = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.c_pbPreview)).BeginInit();
			this.c_gbNotes.SuspendLayout();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.SuspendLayout();
			// 
			// c_bCancel
			// 
			this.c_bCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.c_bCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.c_bCancel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.c_bCancel.Location = new System.Drawing.Point(386, 456);
			this.c_bCancel.Name = "c_bCancel";
			this.c_bCancel.Size = new System.Drawing.Size(75, 23);
			this.c_bCancel.TabIndex = 10;
			this.c_bCancel.Text = "&Cancel";
			// 
			// c_bOK
			// 
			this.c_bOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.c_bOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.c_bOK.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.c_bOK.Location = new System.Drawing.Point(305, 456);
			this.c_bOK.Name = "c_bOK";
			this.c_bOK.Size = new System.Drawing.Size(75, 23);
			this.c_bOK.TabIndex = 9;
			this.c_bOK.Text = "&OK";
			this.c_bOK.Click += new System.EventHandler(this.butOK_Click);
			// 
			// c_pbPreview
			// 
			this.c_pbPreview.Dock = System.Windows.Forms.DockStyle.Fill;
			this.c_pbPreview.Location = new System.Drawing.Point(0, 0);
			this.c_pbPreview.Name = "c_pbPreview";
			this.c_pbPreview.Size = new System.Drawing.Size(470, 359);
			this.c_pbPreview.TabIndex = 0;
			this.c_pbPreview.TabStop = false;
			this.c_pbPreview.Paint += new System.Windows.Forms.PaintEventHandler(this.pictureBox_Paint);
			this.c_pbPreview.SizeChanged += new System.EventHandler(this.pictureBox_SizeChanged);
			// 
			// c_gbNotes
			// 
			this.c_gbNotes.Controls.Add(this.c_lNotes);
			this.c_gbNotes.Dock = System.Windows.Forms.DockStyle.Fill;
			this.c_gbNotes.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.c_gbNotes.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.c_gbNotes.Location = new System.Drawing.Point(0, 0);
			this.c_gbNotes.Name = "c_gbNotes";
			this.c_gbNotes.Size = new System.Drawing.Size(470, 87);
			this.c_gbNotes.TabIndex = 13;
			this.c_gbNotes.TabStop = false;
			this.c_gbNotes.Text = "Notes";
			// 
			// c_lNotes
			// 
			this.c_lNotes.Dock = System.Windows.Forms.DockStyle.Fill;
			this.c_lNotes.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.c_lNotes.Location = new System.Drawing.Point(3, 17);
			this.c_lNotes.Name = "c_lNotes";
			this.c_lNotes.Size = new System.Drawing.Size(464, 67);
			this.c_lNotes.TabIndex = 1;
			this.c_lNotes.Text = "Some notes";
			// 
			// splitContainer1
			// 
			this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
							| System.Windows.Forms.AnchorStyles.Left)
							| System.Windows.Forms.AnchorStyles.Right)));
			this.splitContainer1.Location = new System.Drawing.Point(1, 1);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.c_pbPreview);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.c_gbNotes);
			this.splitContainer1.Size = new System.Drawing.Size(470, 450);
			this.splitContainer1.SplitterDistance = 359;
			this.splitContainer1.TabIndex = 14;
			// 
			// c_lView
			// 
			this.c_lView.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.c_lView.AutoSize = true;
			this.c_lView.Location = new System.Drawing.Point(32, 460);
			this.c_lView.Name = "c_lView";
			this.c_lView.Size = new System.Drawing.Size(30, 13);
			this.c_lView.TabIndex = 16;
			this.c_lView.Text = "View";
			this.c_lView.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// ViewOpenDialog
			// 
			this.AcceptButton = this.c_bOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.c_bCancel;
			this.ClientSize = new System.Drawing.Size(473, 482);
			this.Controls.Add(this.c_lView);
			this.Controls.Add(this.splitContainer1);
			this.Controls.Add(this.c_bCancel);
			this.Controls.Add(this.c_bOK);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(481, 516);
			this.Name = "ViewOpenDialog";
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Open View";
			((System.ComponentModel.ISupportInitialize)(this.c_pbPreview)).EndInit();
			this.c_gbNotes.ResumeLayout(false);
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

      }

      #endregion

      private System.Windows.Forms.PictureBox c_pbPreview;
      private System.Windows.Forms.GroupBox c_gbNotes;
      private System.Windows.Forms.Label c_lNotes;
      private System.Windows.Forms.Button c_bCancel;
      private System.Windows.Forms.Button c_bOK;
      private System.Windows.Forms.SplitContainer splitContainer1;
      private System.Windows.Forms.Label c_lView;
   }
}