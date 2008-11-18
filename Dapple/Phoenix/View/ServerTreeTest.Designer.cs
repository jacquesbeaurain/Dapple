namespace NewServerTree.View
{
	partial class ServerTreeTest
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
			this.serverTree1 = new NewServerTree.View.ServerTree();
			this.c_bAddWMS = new System.Windows.Forms.Button();
			this.serverTree2 = new NewServerTree.View.ServerTree();
			this.SuspendLayout();
			// 
			// serverTree1
			// 
			this.serverTree1.Location = new System.Drawing.Point(12, 12);
			this.serverTree1.Name = "serverTree1";
			this.serverTree1.Size = new System.Drawing.Size(395, 417);
			this.serverTree1.TabIndex = 0;
			// 
			// c_bAddWMS
			// 
			this.c_bAddWMS.Enabled = false;
			this.c_bAddWMS.Location = new System.Drawing.Point(12, 447);
			this.c_bAddWMS.Name = "c_bAddWMS";
			this.c_bAddWMS.Size = new System.Drawing.Size(98, 23);
			this.c_bAddWMS.TabIndex = 1;
			this.c_bAddWMS.Text = "Add WMS server";
			this.c_bAddWMS.UseVisualStyleBackColor = true;
			this.c_bAddWMS.Click += new System.EventHandler(this.c_bAddWMS_Click);
			// 
			// serverTree2
			// 
			this.serverTree2.Location = new System.Drawing.Point(413, 12);
			this.serverTree2.Name = "serverTree2";
			this.serverTree2.Size = new System.Drawing.Size(370, 417);
			this.serverTree2.TabIndex = 2;
			// 
			// ServerTreeTest
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(795, 482);
			this.Controls.Add(this.serverTree2);
			this.Controls.Add(this.c_bAddWMS);
			this.Controls.Add(this.serverTree1);
			this.Name = "ServerTreeTest";
			this.Text = "ServerTreeTest";
			this.ResumeLayout(false);

		}

		#endregion

		private global::NewServerTree.View.ServerTree serverTree1;
		private System.Windows.Forms.Button c_bAddWMS;
		private ServerTree serverTree2;

	}
}