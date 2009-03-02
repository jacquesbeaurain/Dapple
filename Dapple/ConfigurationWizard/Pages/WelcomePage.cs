using System;
using System.IO;

namespace ConfigurationWizard
{
	/// <summary>
	/// Summary description for WelcomePage.
	/// </summary>
	internal class WelcomePage : WizardPage
	{
      private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.Label label1;

		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WelcomePage));
			this.label1 = new System.Windows.Forms.Label();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(65, 63);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(408, 163);
			this.label1.TabIndex = 0;
			this.label1.Text = "This wizard will guide you through setting up Dapple. \r\n\r\n You can return to this" +
				 " wizard at any time by clicking\r\nSettings ->Advanced Settings...";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// pictureBox1
			// 
			this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
			this.pictureBox1.Location = new System.Drawing.Point(426, 220);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(96, 125);
			this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
			this.pictureBox1.TabIndex = 1;
			this.pictureBox1.TabStop = false;
			// 
			// WelcomePage
			// 
			this.Controls.Add(this.pictureBox1);
			this.Controls.Add(this.label1);
			this.Name = "WelcomePage";
			this.SubTitle = "Welcome to Dapple Advanced Settings";
			this.Title = "Advanced Settings";
			this.Controls.SetChildIndex(this.label1, 0);
			this.Controls.SetChildIndex(this.pictureBox1, 0);
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
	
		/// <summary>
		/// Initializes a new instance of the <see cref= "T:ConfigurationWizard.WelcomePage"/> class.
		/// </summary>
		internal WelcomePage()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();
		}

	}
}
