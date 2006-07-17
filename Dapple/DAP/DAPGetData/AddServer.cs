using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Geosoft.GX.DAPGetData
{
	/// <summary>
	/// Summary description for AddServer.
	/// </summary>
	public class AddServer : System.Windows.Forms.Form
	{
      #region Member Variables
      protected string m_strServerUrl;
      #endregion

      private System.Windows.Forms.Label lServerUrl;
      private System.Windows.Forms.Button bOk;
      private System.Windows.Forms.Button bCancel;
      private System.Windows.Forms.TextBox tbServerUrl;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

      #region Properties
      /// <summary>
      /// Get/Set the server url
      /// </summary>
      public string ServerUrl
      {
         get { return m_strServerUrl; }
         set { m_strServerUrl = value; }
      }
      #endregion

      #region Constructor/Destructor
      /// <summary>
      /// Default constructor
      /// </summary>
		public AddServer()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
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
      #endregion

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
         this.lServerUrl = new System.Windows.Forms.Label();
         this.tbServerUrl = new System.Windows.Forms.TextBox();
         this.bOk = new System.Windows.Forms.Button();
         this.bCancel = new System.Windows.Forms.Button();
         this.SuspendLayout();
         // 
         // lServerUrl
         // 
         this.lServerUrl.Location = new System.Drawing.Point(-24, 8);
         this.lServerUrl.Name = "lServerUrl";
         this.lServerUrl.Size = new System.Drawing.Size(112, 24);
         this.lServerUrl.TabIndex = 0;
         this.lServerUrl.Text = "DAP Server URL";
         this.lServerUrl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
         // 
         // tbServerUrl
         // 
         this.tbServerUrl.Location = new System.Drawing.Point(88, 8);
         this.tbServerUrl.Name = "tbServerUrl";
         this.tbServerUrl.Size = new System.Drawing.Size(224, 20);
         this.tbServerUrl.TabIndex = 1;
         this.tbServerUrl.Text = "";
         // 
         // bOk
         // 
         this.bOk.Location = new System.Drawing.Point(160, 32);
         this.bOk.Name = "bOk";
         this.bOk.TabIndex = 2;
         this.bOk.Text = "Ok";
         this.bOk.Click += new System.EventHandler(this.bOk_Click);
         // 
         // bCancel
         // 
         this.bCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
         this.bCancel.Location = new System.Drawing.Point(240, 32);
         this.bCancel.Name = "bCancel";
         this.bCancel.TabIndex = 3;
         this.bCancel.Text = "Cancel";
         this.bCancel.Click += new System.EventHandler(this.bCancel_Click);
         // 
         // AddServer
         // 
         this.AcceptButton = this.bOk;
         this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
         this.CancelButton = this.bCancel;
         this.ClientSize = new System.Drawing.Size(320, 64);
         this.ControlBox = false;
         this.Controls.Add(this.bCancel);
         this.Controls.Add(this.bOk);
         this.Controls.Add(this.tbServerUrl);
         this.Controls.Add(this.lServerUrl);
         this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
         this.MaximizeBox = false;
         this.MinimizeBox = false;
         this.Name = "AddServer";
         this.ShowInTaskbar = false;
         this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
         this.Text = "Add Server";
         this.ResumeLayout(false);

      }
		#endregion

      #region Event Handlers
      /// <summary>
      /// Set the new server url
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void bOk_Click(object sender, System.EventArgs e)
      {
         if (tbServerUrl.Text.Length == 0) 
         {
            MessageBox.Show("Please enter a dap server URL.", "Invalid URL");
         }
         else 
         {
            ServerUrl = tbServerUrl.Text;
            DialogResult = DialogResult.OK;
            Close();
         }
      }

      /// <summary>
      /// Cancel adding a new server
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void bCancel_Click(object sender, System.EventArgs e)
      {
         DialogResult = DialogResult.Cancel;
      }
      #endregion
	}
}
