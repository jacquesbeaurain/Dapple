namespace Dapple.Extract
{
	partial class ExtractDebug
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
			this.c_bDone = new System.Windows.Forms.Button();
			this.c_bExecute = new System.Windows.Forms.Button();
			this.c_wbExtract = new System.Windows.Forms.WebBrowser();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// c_bDone
			// 
			this.c_bDone.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.c_bDone.Location = new System.Drawing.Point(369, 341);
			this.c_bDone.Name = "c_bDone";
			this.c_bDone.Size = new System.Drawing.Size(75, 23);
			this.c_bDone.TabIndex = 1;
			this.c_bDone.Text = "Close";
			this.c_bDone.UseVisualStyleBackColor = true;
			this.c_bDone.Click += new System.EventHandler(this.c_bDone_Click);
			// 
			// c_bExecute
			// 
			this.c_bExecute.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.c_bExecute.Location = new System.Drawing.Point(288, 341);
			this.c_bExecute.Name = "c_bExecute";
			this.c_bExecute.Size = new System.Drawing.Size(75, 23);
			this.c_bExecute.TabIndex = 2;
			this.c_bExecute.Text = "Execute";
			this.c_bExecute.UseVisualStyleBackColor = true;
			this.c_bExecute.Click += new System.EventHandler(this.c_bExecute_Click);
			// 
			// c_wbExtract
			// 
			this.c_wbExtract.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
							| System.Windows.Forms.AnchorStyles.Left)
							| System.Windows.Forms.AnchorStyles.Right)));
			this.c_wbExtract.Location = new System.Drawing.Point(12, 49);
			this.c_wbExtract.MinimumSize = new System.Drawing.Size(20, 20);
			this.c_wbExtract.Name = "c_wbExtract";
			this.c_wbExtract.Size = new System.Drawing.Size(432, 255);
			this.c_wbExtract.TabIndex = 3;
			// 
			// label1
			// 
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
							| System.Windows.Forms.AnchorStyles.Right)));
			this.label1.Location = new System.Drawing.Point(12, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(432, 37);
			this.label1.TabIndex = 4;
			this.label1.Text = "This is a debugging dialog. If you see this, but are not debugging Dapple, you sh" +
				 "ould report this to Geosoft at dapple@geosoft.com.";
			// 
			// label2
			// 
			this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
							| System.Windows.Forms.AnchorStyles.Right)));
			this.label2.Location = new System.Drawing.Point(12, 307);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(432, 31);
			this.label2.TabIndex = 5;
			this.label2.Text = "Hit close to close this dialog and let the extract dialog perform extraction. Hit" +
				 " execute to perform extraction but leave this dialog open.";
			// 
			// ExtractDebug
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(456, 376);
			this.ControlBox = false;
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.c_wbExtract);
			this.Controls.Add(this.c_bExecute);
			this.Controls.Add(this.c_bDone);
			this.MinimumSize = new System.Drawing.Size(400, 200);
			this.Name = "ExtractDebug";
			this.Text = "Extraction Debugging Dialog";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button c_bDone;
		private System.Windows.Forms.Button c_bExecute;
		private System.Windows.Forms.WebBrowser c_wbExtract;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;

	}
}