namespace NewServerTree.View
{
	partial class ServerTree
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
			this.c_tvView = new System.Windows.Forms.TreeView();
			this.SuspendLayout();
			// 
			// c_tvView
			// 
			this.c_tvView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.c_tvView.HideSelection = false;
			this.c_tvView.Location = new System.Drawing.Point(0, 0);
			this.c_tvView.Name = "c_tvView";
			this.c_tvView.ShowPlusMinus = false;
			this.c_tvView.ShowRootLines = false;
			this.c_tvView.Size = new System.Drawing.Size(150, 150);
			this.c_tvView.TabIndex = 0;
			this.c_tvView.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.c_tvView_NodeMouseDoubleClick);
			this.c_tvView.BeforeCollapse += new System.Windows.Forms.TreeViewCancelEventHandler(this.c_tvView_BeforeCollapse);
			this.c_tvView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.c_tvView_AfterSelect);
			this.c_tvView.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.c_tvView_NodeMouseClick);
			this.c_tvView.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.c_tvView_BeforeSelect);
			// 
			// ServerTree
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.c_tvView);
			this.Name = "ServerTree";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TreeView c_tvView;
	}
}
