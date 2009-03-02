using System;

namespace ConfigurationWizard
{
	/// <summary>
	/// Summary description for FinalPage .
	/// </summary>
	internal class FinalPage : WizardPage
	{
      private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.Label label1;

		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FinalPage));
			this.label1 = new System.Windows.Forms.Label();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(48, 96);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(448, 136);
			this.label1.TabIndex = 0;
			this.label1.Text = "Congratulations, you have succesfully configured Dapple!\r\n\r\nRemember you can retu" +
				 "rn to this wizard at any time by clicking\r\nSettings->Advanced Settings...";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// pictureBox1
			// 
			this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
			this.pictureBox1.Location = new System.Drawing.Point(428, 220);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(96, 125);
			this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
			this.pictureBox1.TabIndex = 2;
			this.pictureBox1.TabStop = false;
			// 
			// FinalPage
			// 
			this.Controls.Add(this.pictureBox1);
			this.Controls.Add(this.label1);
			this.Name = "FinalPage";
			this.SubTitle = "Dapple configuration is complete";
			this.Title = "All done!";
			this.Controls.SetChildIndex(this.label1, 0);
			this.Controls.SetChildIndex(this.pictureBox1, 0);
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
	
		/// <summary>
		/// Initializes a new instance of the <see cref= "T:ConfigurationWizard.FinalPage"/> class.
		/// </summary>
		internal FinalPage()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call
		}
	}
}
