namespace Dapple.CustomControls
{
	partial class Overview
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
			this.c_cbAOIs = new System.Windows.Forms.ComboBox();
			this.c_pOverview = new System.Windows.Forms.Panel();
			this.SuspendLayout();
			// 
			// c_cbAOIs
			// 
			this.c_cbAOIs.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
							| System.Windows.Forms.AnchorStyles.Right)));
			this.c_cbAOIs.DisplayMember = "Key";
			this.c_cbAOIs.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.c_cbAOIs.FormattingEnabled = true;
			this.c_cbAOIs.Location = new System.Drawing.Point(3, 3);
			this.c_cbAOIs.Name = "c_cbAOIs";
			this.c_cbAOIs.Size = new System.Drawing.Size(144, 21);
			this.c_cbAOIs.TabIndex = 0;
			this.c_cbAOIs.SelectedIndexChanged += new System.EventHandler(this.c_cbAOIs_SelectedIndexChanged);
			// 
			// c_pOverview
			// 
			this.c_pOverview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
							| System.Windows.Forms.AnchorStyles.Left)
							| System.Windows.Forms.AnchorStyles.Right)));
			this.c_pOverview.Location = new System.Drawing.Point(0, 30);
			this.c_pOverview.Name = "c_pOverview";
			this.c_pOverview.Size = new System.Drawing.Size(150, 120);
			this.c_pOverview.TabIndex = 1;
			this.c_pOverview.Resize += new System.EventHandler(this.c_pOverview_Resize);
			// 
			// Overview
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.c_pOverview);
			this.Controls.Add(this.c_cbAOIs);
			this.Name = "Overview";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ComboBox c_cbAOIs;
		private System.Windows.Forms.Panel c_pOverview;
	}
}
