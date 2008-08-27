namespace GED.App.UI.Forms
{
	partial class MainForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			this.c_bSearch = new System.Windows.Forms.Button();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.dappleSearchList1 = new GED.App.UI.Controls.DappleSearchList();
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.c_miFile = new System.Windows.Forms.ToolStripMenuItem();
			this.c_miExit = new System.Windows.Forms.ToolStripMenuItem();
			this.c_miTools = new System.Windows.Forms.ToolStripMenuItem();
			this.c_miOptions = new System.Windows.Forms.ToolStripMenuItem();
			this.c_miEdit = new System.Windows.Forms.ToolStripMenuItem();
			this.c_miRefreshView = new System.Windows.Forms.ToolStripMenuItem();
			this.menuStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// c_bSearch
			// 
			this.c_bSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.c_bSearch.Image = global::GED.App.Properties.Resources.search;
			this.c_bSearch.Location = new System.Drawing.Point(501, 27);
			this.c_bSearch.Name = "c_bSearch";
			this.c_bSearch.Size = new System.Drawing.Size(25, 25);
			this.c_bSearch.TabIndex = 1;
			this.c_bSearch.UseVisualStyleBackColor = true;
			this.c_bSearch.Click += new System.EventHandler(this.c_bSearch_Click);
			// 
			// textBox1
			// 
			this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
							| System.Windows.Forms.AnchorStyles.Right)));
			this.textBox1.Location = new System.Drawing.Point(12, 30);
			this.textBox1.Name = "textBox1";
			this.textBox1.Size = new System.Drawing.Size(483, 20);
			this.textBox1.TabIndex = 2;
			this.textBox1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox1_KeyDown);
			// 
			// dappleSearchList1
			// 
			this.dappleSearchList1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
							| System.Windows.Forms.AnchorStyles.Left)
							| System.Windows.Forms.AnchorStyles.Right)));
			this.dappleSearchList1.Location = new System.Drawing.Point(12, 58);
			this.dappleSearchList1.Name = "dappleSearchList1";
			this.dappleSearchList1.Size = new System.Drawing.Size(514, 612);
			this.dappleSearchList1.TabIndex = 0;
			this.dappleSearchList1.LayerAddRequested += new System.EventHandler<GED.App.UI.Controls.DappleSearchList.LayerAddArgs>(this.dappleSearchList1_LayerAddRequested);
			// 
			// menuStrip1
			// 
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.c_miFile,
            this.c_miEdit,
            this.c_miTools});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(538, 24);
			this.menuStrip1.TabIndex = 3;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// c_miFile
			// 
			this.c_miFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.c_miExit});
			this.c_miFile.Name = "c_miFile";
			this.c_miFile.Size = new System.Drawing.Size(35, 20);
			this.c_miFile.Text = "&File";
			// 
			// c_miExit
			// 
			this.c_miExit.Name = "c_miExit";
			this.c_miExit.Size = new System.Drawing.Size(152, 22);
			this.c_miExit.Text = "E&xit";
			this.c_miExit.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
			// 
			// c_miTools
			// 
			this.c_miTools.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.c_miOptions});
			this.c_miTools.Name = "c_miTools";
			this.c_miTools.Size = new System.Drawing.Size(44, 20);
			this.c_miTools.Text = "&Tools";
			// 
			// c_miOptions
			// 
			this.c_miOptions.Name = "c_miOptions";
			this.c_miOptions.Size = new System.Drawing.Size(152, 22);
			this.c_miOptions.Text = "&Options";
			this.c_miOptions.Click += new System.EventHandler(this.optionsToolStripMenuItem_Click);
			// 
			// c_miEdit
			// 
			this.c_miEdit.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.c_miRefreshView});
			this.c_miEdit.Name = "c_miEdit";
			this.c_miEdit.Size = new System.Drawing.Size(37, 20);
			this.c_miEdit.Text = "&Edit";
			// 
			// c_miRefreshView
			// 
			this.c_miRefreshView.Name = "c_miRefreshView";
			this.c_miRefreshView.Size = new System.Drawing.Size(174, 22);
			this.c_miRefreshView.Text = "&Trigger View Refresh";
			this.c_miRefreshView.Click += new System.EventHandler(this.c_miRefreshView_Click);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(538, 682);
			this.Controls.Add(this.textBox1);
			this.Controls.Add(this.c_bSearch);
			this.Controls.Add(this.dappleSearchList1);
			this.Controls.Add(this.menuStrip1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MainMenuStrip = this.menuStrip1;
			this.Name = "MainForm";
			this.Text = "GEDapple";
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private GED.App.UI.Controls.DappleSearchList dappleSearchList1;
		private System.Windows.Forms.Button c_bSearch;
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem c_miFile;
		private System.Windows.Forms.ToolStripMenuItem c_miExit;
		private System.Windows.Forms.ToolStripMenuItem c_miTools;
		private System.Windows.Forms.ToolStripMenuItem c_miOptions;
		private System.Windows.Forms.ToolStripMenuItem c_miEdit;
		private System.Windows.Forms.ToolStripMenuItem c_miRefreshView;

	}
}

