using System;
using System.IO;
using System.Globalization;
using System.Diagnostics;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Utility
{
	/// <summary>
	/// Summary description for ErrorDisplay.
	/// </summary>
	public class ErrorDisplay : System.Windows.Forms.Form
	{
		private System.Windows.Forms.TextBox errorText;
		private System.Windows.Forms.Button copyButton;
		private System.Windows.Forms.Button exitButton;
		private System.Windows.Forms.Label errorLabel;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:Utility.ErrorDisplay"/> class 
		/// with default data.
		/// </summary>
		public ErrorDisplay()
		{
			InitializeComponent();
         this.Icon = new System.Drawing.Icon(@"app.ico");
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		
		public void errorMessages(string errorMessages)
		{
			this.errorText.Text = errorMessages;
		}

		private void exitButton_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		private void copyButton_Click(object sender, System.EventArgs e)
		{
         string strVersion = Application.ProductVersion;
         string tempBodyFile = Path.ChangeExtension(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()), ".txt");
         string tempAbortFile = Path.ChangeExtension(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()), ".log");

         using (StreamWriter sw = new StreamWriter(tempAbortFile, false))
         {
				String szTemp = "DAPPLE " + strVersion + " ABORT" + Environment.NewLine +
					Environment.NewLine + 
					this.errorText.Text;
            sw.Write(szTemp);
         }

         using (StreamWriter sw = new StreamWriter(tempBodyFile))
         {
            sw.WriteLine("Thank you very much for helping us to improve Dapple.");
            sw.WriteLine();
            sw.WriteLine("Please describe what you were doing, or what menu item you were using when the system stopped.");
            sw.WriteLine();
         }

         string strMailApp = Path.Combine(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "System"), "mailer.exe");

         this.TopMost = false;

         ProcessStartInfo psi = new ProcessStartInfo(strMailApp);
         psi.UseShellExecute = false;
         psi.CreateNoWindow = true;
         psi.Arguments = String.Format(CultureInfo.InvariantCulture,
            " \"{0}\" \"{1}\" \"{2}\" \"{3}\" \"{4}\" \"{5}\"",
            "Dapple(" + strVersion + ") Abort Log Report", "", "dapple@geosoft.com",
            tempAbortFile, "abort.log", tempBodyFile);
         using (Process p = Process.Start(psi))
         {
            p.WaitForExit();
         }

         File.Delete(tempBodyFile);
         File.Delete(tempAbortFile);
         
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
         this.exitButton = new System.Windows.Forms.Button();
         this.errorLabel = new System.Windows.Forms.Label();
         this.errorText = new System.Windows.Forms.TextBox();
         this.copyButton = new System.Windows.Forms.Button();
         this.SuspendLayout();
         // 
         // exitButton
         // 
         this.exitButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
         this.exitButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
         this.exitButton.Location = new System.Drawing.Point(8, 336);
         this.exitButton.Name = "exitButton";
         this.exitButton.Size = new System.Drawing.Size(120, 23);
         this.exitButton.TabIndex = 0;
         this.exitButton.Text = "E&xit";
         this.exitButton.Click += new System.EventHandler(this.exitButton_Click);
         // 
         // errorLabel
         // 
         this.errorLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
         this.errorLabel.Location = new System.Drawing.Point(8, 8);
         this.errorLabel.Name = "errorLabel";
         this.errorLabel.Size = new System.Drawing.Size(168, 23);
         this.errorLabel.TabIndex = 2;
         this.errorLabel.Tag = "A Fatal Error has occurred:";
         this.errorLabel.Text = "A Fatal Error has occurred:";
         // 
         // errorText
         // 
         this.errorText.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                     | System.Windows.Forms.AnchorStyles.Left)
                     | System.Windows.Forms.AnchorStyles.Right)));
         this.errorText.HideSelection = false;
         this.errorText.Location = new System.Drawing.Point(8, 24);
         this.errorText.Multiline = true;
         this.errorText.Name = "errorText";
         this.errorText.ReadOnly = true;
         this.errorText.ScrollBars = System.Windows.Forms.ScrollBars.Both;
         this.errorText.Size = new System.Drawing.Size(606, 304);
         this.errorText.TabIndex = 3;
         // 
         // copyButton
         // 
         this.copyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
         this.copyButton.Location = new System.Drawing.Point(134, 336);
         this.copyButton.Name = "copyButton";
         this.copyButton.Size = new System.Drawing.Size(200, 23);
         this.copyButton.TabIndex = 1;
         this.copyButton.Text = "&Send us a mail message";
         this.copyButton.Click += new System.EventHandler(this.copyButton_Click);
         // 
         // ErrorDisplay
         // 
         this.AcceptButton = this.exitButton;
         this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
         this.CancelButton = this.exitButton;
         this.ClientSize = new System.Drawing.Size(622, 366);
         this.Controls.Add(this.copyButton);
         this.Controls.Add(this.errorText);
         this.Controls.Add(this.errorLabel);
         this.Controls.Add(this.exitButton);
         this.MinimumSize = new System.Drawing.Size(400, 200);
         this.Name = "ErrorDisplay";
         this.Text = "Dapple Error";
         this.TopMost = true;
         this.ResumeLayout(false);
         this.PerformLayout();

		}
		#endregion

	}
}
