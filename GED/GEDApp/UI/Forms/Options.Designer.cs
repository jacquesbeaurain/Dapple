namespace GED.App.UI.Forms
{
	partial class Options
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
			this.c_gbCache = new System.Windows.Forms.GroupBox();
			this.c_bCacheDirectory = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.c_tbCacheDirectory = new System.Windows.Forms.TextBox();
			this.c_gbKML = new System.Windows.Forms.GroupBox();
			this.label2 = new System.Windows.Forms.Label();
			this.c_cbKmlFormat = new System.Windows.Forms.ComboBox();
			this.c_bOk = new System.Windows.Forms.Button();
			this.c_bCancel = new System.Windows.Forms.Button();
			this.c_gbSearchProvider = new System.Windows.Forms.GroupBox();
			this.label4 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.c_cbSearchProviderType = new System.Windows.Forms.ComboBox();
			this.c_tbSearchProviderURL = new System.Windows.Forms.TextBox();
			this.c_gbCache.SuspendLayout();
			this.c_gbKML.SuspendLayout();
			this.c_gbSearchProvider.SuspendLayout();
			this.SuspendLayout();
			// 
			// c_gbCache
			// 
			this.c_gbCache.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
							| System.Windows.Forms.AnchorStyles.Right)));
			this.c_gbCache.Controls.Add(this.c_bCacheDirectory);
			this.c_gbCache.Controls.Add(this.label1);
			this.c_gbCache.Controls.Add(this.c_tbCacheDirectory);
			this.c_gbCache.Location = new System.Drawing.Point(12, 12);
			this.c_gbCache.Name = "c_gbCache";
			this.c_gbCache.Size = new System.Drawing.Size(588, 48);
			this.c_gbCache.TabIndex = 0;
			this.c_gbCache.TabStop = false;
			this.c_gbCache.Text = "Cache";
			// 
			// c_bCacheDirectory
			// 
			this.c_bCacheDirectory.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.c_bCacheDirectory.Location = new System.Drawing.Point(556, 19);
			this.c_bCacheDirectory.Name = "c_bCacheDirectory";
			this.c_bCacheDirectory.Size = new System.Drawing.Size(26, 23);
			this.c_bCacheDirectory.TabIndex = 2;
			this.c_bCacheDirectory.Text = "...";
			this.c_bCacheDirectory.UseVisualStyleBackColor = true;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(6, 24);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(84, 13);
			this.label1.TabIndex = 1;
			this.label1.Text = "Cache directory:";
			// 
			// c_tbCacheDirectory
			// 
			this.c_tbCacheDirectory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
							| System.Windows.Forms.AnchorStyles.Right)));
			this.c_tbCacheDirectory.Location = new System.Drawing.Point(96, 21);
			this.c_tbCacheDirectory.Name = "c_tbCacheDirectory";
			this.c_tbCacheDirectory.Size = new System.Drawing.Size(454, 20);
			this.c_tbCacheDirectory.TabIndex = 0;
			// 
			// c_gbKML
			// 
			this.c_gbKML.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
							| System.Windows.Forms.AnchorStyles.Right)));
			this.c_gbKML.Controls.Add(this.label2);
			this.c_gbKML.Controls.Add(this.c_cbKmlFormat);
			this.c_gbKML.Location = new System.Drawing.Point(12, 66);
			this.c_gbKML.Name = "c_gbKML";
			this.c_gbKML.Size = new System.Drawing.Size(588, 46);
			this.c_gbKML.TabIndex = 1;
			this.c_gbKML.TabStop = false;
			this.c_gbKML.Text = "KML";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(6, 22);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(64, 13);
			this.label2.TabIndex = 1;
			this.label2.Text = "KML format:";
			// 
			// c_cbKmlFormat
			// 
			this.c_cbKmlFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.c_cbKmlFormat.FormattingEnabled = true;
			this.c_cbKmlFormat.Location = new System.Drawing.Point(96, 19);
			this.c_cbKmlFormat.Name = "c_cbKmlFormat";
			this.c_cbKmlFormat.Size = new System.Drawing.Size(121, 21);
			this.c_cbKmlFormat.TabIndex = 0;
			// 
			// c_bOk
			// 
			this.c_bOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.c_bOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.c_bOk.Location = new System.Drawing.Point(444, 173);
			this.c_bOk.Name = "c_bOk";
			this.c_bOk.Size = new System.Drawing.Size(75, 23);
			this.c_bOk.TabIndex = 2;
			this.c_bOk.Text = "OK";
			this.c_bOk.UseVisualStyleBackColor = true;
			this.c_bOk.Click += new System.EventHandler(this.c_bOk_Click);
			// 
			// c_bCancel
			// 
			this.c_bCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.c_bCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.c_bCancel.Location = new System.Drawing.Point(525, 173);
			this.c_bCancel.Name = "c_bCancel";
			this.c_bCancel.Size = new System.Drawing.Size(75, 23);
			this.c_bCancel.TabIndex = 3;
			this.c_bCancel.Text = "Cancel";
			this.c_bCancel.UseVisualStyleBackColor = true;
			// 
			// c_gbSearchProvider
			// 
			this.c_gbSearchProvider.Controls.Add(this.label4);
			this.c_gbSearchProvider.Controls.Add(this.label3);
			this.c_gbSearchProvider.Controls.Add(this.c_cbSearchProviderType);
			this.c_gbSearchProvider.Controls.Add(this.c_tbSearchProviderURL);
			this.c_gbSearchProvider.Location = new System.Drawing.Point(12, 118);
			this.c_gbSearchProvider.Name = "c_gbSearchProvider";
			this.c_gbSearchProvider.Size = new System.Drawing.Size(588, 45);
			this.c_gbSearchProvider.TabIndex = 4;
			this.c_gbSearchProvider.TabStop = false;
			this.c_gbSearchProvider.Text = "Search Provider";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(421, 22);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(34, 13);
			this.label4.TabIndex = 3;
			this.label4.Text = "Type:";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(6, 22);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(32, 13);
			this.label3.TabIndex = 2;
			this.label3.Text = "URL:";
			// 
			// c_cbSearchProviderType
			// 
			this.c_cbSearchProviderType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.c_cbSearchProviderType.FormattingEnabled = true;
			this.c_cbSearchProviderType.Location = new System.Drawing.Point(461, 19);
			this.c_cbSearchProviderType.Name = "c_cbSearchProviderType";
			this.c_cbSearchProviderType.Size = new System.Drawing.Size(121, 21);
			this.c_cbSearchProviderType.TabIndex = 1;
			// 
			// c_tbSearchProviderURL
			// 
			this.c_tbSearchProviderURL.Location = new System.Drawing.Point(96, 19);
			this.c_tbSearchProviderURL.Name = "c_tbSearchProviderURL";
			this.c_tbSearchProviderURL.Size = new System.Drawing.Size(319, 20);
			this.c_tbSearchProviderURL.TabIndex = 0;
			// 
			// Options
			// 
			this.AcceptButton = this.c_bOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.c_bCancel;
			this.ClientSize = new System.Drawing.Size(612, 208);
			this.ControlBox = false;
			this.Controls.Add(this.c_gbSearchProvider);
			this.Controls.Add(this.c_bCancel);
			this.Controls.Add(this.c_bOk);
			this.Controls.Add(this.c_gbKML);
			this.Controls.Add(this.c_gbCache);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "Options";
			this.Text = "GEDapple Options";
			this.c_gbCache.ResumeLayout(false);
			this.c_gbCache.PerformLayout();
			this.c_gbKML.ResumeLayout(false);
			this.c_gbKML.PerformLayout();
			this.c_gbSearchProvider.ResumeLayout(false);
			this.c_gbSearchProvider.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.GroupBox c_gbCache;
		private System.Windows.Forms.GroupBox c_gbKML;
		private System.Windows.Forms.Button c_bOk;
		private System.Windows.Forms.Button c_bCancel;
		private System.Windows.Forms.Button c_bCacheDirectory;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox c_tbCacheDirectory;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ComboBox c_cbKmlFormat;
		private System.Windows.Forms.GroupBox c_gbSearchProvider;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.ComboBox c_cbSearchProviderType;
		private System.Windows.Forms.TextBox c_tbSearchProviderURL;
	}
}