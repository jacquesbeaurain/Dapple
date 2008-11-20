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
			this.serverTree2 = new NewServerTree.View.ServerTree();
			this.button1 = new System.Windows.Forms.Button();
			this.button2 = new System.Windows.Forms.Button();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.bSearch = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// serverTree1
			// 
			this.serverTree1.Location = new System.Drawing.Point(12, 12);
			this.serverTree1.Name = "serverTree1";
			this.serverTree1.Size = new System.Drawing.Size(395, 417);
			this.serverTree1.TabIndex = 0;
			// 
			// serverTree2
			// 
			this.serverTree2.Location = new System.Drawing.Point(413, 12);
			this.serverTree2.Name = "serverTree2";
			this.serverTree2.Size = new System.Drawing.Size(370, 417);
			this.serverTree2.TabIndex = 2;
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(12, 447);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(75, 23);
			this.button1.TabIndex = 3;
			this.button1.Text = "Test";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// button2
			// 
			this.button2.Location = new System.Drawing.Point(93, 447);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(75, 23);
			this.button2.TabIndex = 4;
			this.button2.Text = "Last View";
			this.button2.UseVisualStyleBackColor = true;
			this.button2.Click += new System.EventHandler(this.button2_Click);
			// 
			// textBox1
			// 
			this.textBox1.Location = new System.Drawing.Point(683, 450);
			this.textBox1.Name = "textBox1";
			this.textBox1.Size = new System.Drawing.Size(100, 20);
			this.textBox1.TabIndex = 5;
			// 
			// bSearch
			// 
			this.bSearch.Location = new System.Drawing.Point(602, 447);
			this.bSearch.Name = "bSearch";
			this.bSearch.Size = new System.Drawing.Size(75, 23);
			this.bSearch.TabIndex = 6;
			this.bSearch.Text = "Search";
			this.bSearch.UseVisualStyleBackColor = true;
			this.bSearch.Click += new System.EventHandler(this.bSearch_Click);
			// 
			// ServerTreeTest
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(795, 482);
			this.Controls.Add(this.bSearch);
			this.Controls.Add(this.textBox1);
			this.Controls.Add(this.button2);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.serverTree2);
			this.Controls.Add(this.serverTree1);
			this.Name = "ServerTreeTest";
			this.Text = "ServerTreeTest";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private global::NewServerTree.View.ServerTree serverTree1;
		private ServerTree serverTree2;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.Button bSearch;

	}
}