using System;
using System.IO;
using System.Windows.Forms;

namespace ConfigurationWizard
{
	/// <summary>
	/// Summary description for CachePage.
	/// </summary>
	internal class CachePage : WizardPage
	{
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.NumericUpDown cacheSizeGigaBytes;
      private FolderBrowserDialog folderBrowserDialog;
      private Button buttonBrowse;
      private TextBox textBoxCacheLocation;
      private Label label2;
      private Label label3;
      private Button butClearCache;
		private System.Windows.Forms.Label label4;
      private string strCacheFolder = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DappleData"), "Cache");

		private void InitializeComponent()
		{
         System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CachePage));
         this.label1 = new System.Windows.Forms.Label();
         this.cacheSizeGigaBytes = new System.Windows.Forms.NumericUpDown();
         this.label4 = new System.Windows.Forms.Label();
         this.label6 = new System.Windows.Forms.Label();
         this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
         this.buttonBrowse = new System.Windows.Forms.Button();
         this.textBoxCacheLocation = new System.Windows.Forms.TextBox();
         this.label2 = new System.Windows.Forms.Label();
         this.label3 = new System.Windows.Forms.Label();
         this.butClearCache = new System.Windows.Forms.Button();
         ((System.ComponentModel.ISupportInitialize)(this.cacheSizeGigaBytes)).BeginInit();
         this.SuspendLayout();
         // 
         // label1
         // 
         this.label1.Location = new System.Drawing.Point(24, 96);
         this.label1.Name = "label1";
         this.label1.Size = new System.Drawing.Size(120, 16);
         this.label1.TabIndex = 0;
         this.label1.Text = "Maximum cache size:";
         // 
         // cacheSizeGigaBytes
         // 
         this.cacheSizeGigaBytes.Location = new System.Drawing.Point(136, 96);
         this.cacheSizeGigaBytes.Maximum = new decimal(new int[] {
            4096,
            0,
            0,
            0});
         this.cacheSizeGigaBytes.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
         this.cacheSizeGigaBytes.Name = "cacheSizeGigaBytes";
         this.cacheSizeGigaBytes.Size = new System.Drawing.Size(80, 20);
         this.cacheSizeGigaBytes.TabIndex = 0;
         this.cacheSizeGigaBytes.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
         this.cacheSizeGigaBytes.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
         // 
         // label4
         // 
         this.label4.Location = new System.Drawing.Point(216, 96);
         this.label4.Name = "label4";
         this.label4.Size = new System.Drawing.Size(205, 16);
         this.label4.TabIndex = 6;
         this.label4.Text = "GigaBytes (1 Gigabyte = 1024 MB)";
         // 
         // label6
         // 
         this.label6.Location = new System.Drawing.Point(24, 72);
         this.label6.Name = "label6";
         this.label6.Size = new System.Drawing.Size(496, 16);
         this.label6.TabIndex = 8;
         this.label6.Text = "Once this folder reaches it\'s maximum size the cached files that are the oldest w" +
             "ill be removed.";
         this.label6.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
         // 
         // buttonBrowse
         // 
         this.buttonBrowse.Location = new System.Drawing.Point(479, 290);
         this.buttonBrowse.Name = "buttonBrowse";
         this.buttonBrowse.Size = new System.Drawing.Size(31, 20);
         this.buttonBrowse.TabIndex = 2;
         this.buttonBrowse.Text = "...";
         this.buttonBrowse.UseVisualStyleBackColor = true;
         this.buttonBrowse.Click += new System.EventHandler(this.buttonBrowse_Click);
         // 
         // textBoxCacheLocation
         // 
         this.textBoxCacheLocation.Location = new System.Drawing.Point(132, 291);
         this.textBoxCacheLocation.Name = "textBoxCacheLocation";
         this.textBoxCacheLocation.ReadOnly = true;
         this.textBoxCacheLocation.Size = new System.Drawing.Size(347, 20);
         this.textBoxCacheLocation.TabIndex = 1;
         // 
         // label2
         // 
         this.label2.Location = new System.Drawing.Point(21, 186);
         this.label2.Name = "label2";
         this.label2.Size = new System.Drawing.Size(496, 101);
         this.label2.TabIndex = 11;
         this.label2.Text = resources.GetString("label2.Text");
         this.label2.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
         // 
         // label3
         // 
         this.label3.AutoSize = true;
         this.label3.Location = new System.Drawing.Point(45, 294);
         this.label3.Name = "label3";
         this.label3.Size = new System.Drawing.Size(81, 13);
         this.label3.TabIndex = 13;
         this.label3.Text = "Cache location:";
         // 
         // butClearCache
         // 
         this.butClearCache.Location = new System.Drawing.Point(354, 316);
         this.butClearCache.Name = "butClearCache";
         this.butClearCache.Size = new System.Drawing.Size(156, 24);
         this.butClearCache.TabIndex = 3;
         this.butClearCache.Text = "Clear all cached information";
         this.butClearCache.UseVisualStyleBackColor = true;
         this.butClearCache.Click += new System.EventHandler(this.butClearCache_Click);
         // 
         // CachePage
         // 
         this.Controls.Add(this.butClearCache);
         this.Controls.Add(this.label2);
         this.Controls.Add(this.label3);
         this.Controls.Add(this.label6);
         this.Controls.Add(this.textBoxCacheLocation);
         this.Controls.Add(this.buttonBrowse);
         this.Controls.Add(this.label4);
         this.Controls.Add(this.cacheSizeGigaBytes);
         this.Controls.Add(this.label1);
         this.Name = "CachePage";
         this.SubTitle = "Adjust Dapple\'s Cache settings";
         this.Title = "Cache";
         this.Load += new System.EventHandler(this.CachePage_Load);
         this.Controls.SetChildIndex(this.label1, 0);
         this.Controls.SetChildIndex(this.cacheSizeGigaBytes, 0);
         this.Controls.SetChildIndex(this.label4, 0);
         this.Controls.SetChildIndex(this.buttonBrowse, 0);
         this.Controls.SetChildIndex(this.textBoxCacheLocation, 0);
         this.Controls.SetChildIndex(this.label6, 0);
         this.Controls.SetChildIndex(this.label3, 0);
         this.Controls.SetChildIndex(this.label2, 0);
         this.Controls.SetChildIndex(this.butClearCache, 0);
         ((System.ComponentModel.ISupportInitialize)(this.cacheSizeGigaBytes)).EndInit();
         this.ResumeLayout(false);
         this.PerformLayout();

		}
	
		/// <summary>
		/// Initializes a new instance of the <see cref= "T:ConfigurationWizard.CachePage"/> class.
		/// </summary>
		internal CachePage()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();
		}

		protected override void OnValidating(System.ComponentModel.CancelEventArgs e)
		{
			Wizard.Settings.CacheSizeGigaBytes = (int)this.cacheSizeGigaBytes.Value;
		}

		private void CachePage_Load(object sender, System.EventArgs e)
		{
			this.cacheSizeGigaBytes.Value = Wizard.Settings.CacheSizeGigaBytes;

         if (String.Compare(Wizard.Settings.CachePath.TrimEnd(Path.PathSeparator), this.strCacheFolder, true) == 0)
            this.textBoxCacheLocation.Text = "";
         else
            this.textBoxCacheLocation.Text = Wizard.Settings.CachePath;
		}

      private void buttonBrowse_Click(object sender, EventArgs e)
      {
         if (this.textBoxCacheLocation.Text.Length == 0)
            this.folderBrowserDialog.SelectedPath = Wizard.Settings.CachePath;
         else
            this.folderBrowserDialog.SelectedPath = this.textBoxCacheLocation.Text;
         if (this.folderBrowserDialog.ShowDialog(this) == DialogResult.OK)
         {
            if (String.Compare(Wizard.Settings.CachePath, this.folderBrowserDialog.SelectedPath, true) != 0)
            {
               Wizard.Settings.NewCachePath = this.folderBrowserDialog.SelectedPath;
               if (String.Compare(Wizard.Settings.NewCachePath, this.strCacheFolder, true) == 0)
                  this.textBoxCacheLocation.Text = "";
               else
                  this.textBoxCacheLocation.Text = Wizard.Settings.NewCachePath;
            } 
            else
            {
               Wizard.Settings.NewCachePath = "";
               if (String.Compare(Wizard.Settings.CachePath.TrimEnd(Path.PathSeparator), this.strCacheFolder, true) == 0)
                  this.textBoxCacheLocation.Text = "";
               else
                  this.textBoxCacheLocation.Text = Wizard.Settings.CachePath;
            }
         }
      }


      private void butClearCache_Click(object sender, EventArgs e)
      {
         Utility.FileSystem.DeleteFolderGUI(this, Wizard.Settings.CachePath, "Deleting Cached Data");
      }
	}
}
