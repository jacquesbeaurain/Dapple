using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Geosoft.GX.DAPGetData
{
   /// <summary>
   /// Summary description for Login.
   /// </summary>
   public class Login : System.Windows.Forms.Form
   {
      private System.Windows.Forms.TextBox tbUserName;
      private System.Windows.Forms.TextBox tbPassword;
      private System.Windows.Forms.Label lUserName;
      private System.Windows.Forms.Label lPassword;
      private System.Windows.Forms.Button bCancel;
      private System.Windows.Forms.Button bOK;
      private Server m_oServer;

      /// <summary>
      /// Required designer variable.
      /// </summary>
      private System.ComponentModel.Container components = null;

      /// <summary>
      /// Get the user name
      /// </summary>
      public string UserName 
      { 
         get {return tbUserName.Text; }
      }             
      
      /// <summary>
      /// Get the password
      /// </summary>
      public string Password
      { 
         get { return tbPassword.Text; }
      }

      /// <summary>
      /// Default constructor
      /// </summary>
		public Login(Server oServer)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

         m_oServer = oServer;
         Text = string.Format("Dap Server Login - {0}", m_oServer.Name);

         
         // --- get the user name from the project ---

         string strUserName = string.Empty;

#if !DAPPLE
         Geosoft.GXNet.CSYS.GtString("DAPGETDATA", "USER_NAME", ref strUserName);
#endif
         tbUserName.Text = strUserName;         
		}

      /// <summary>
      /// Default constructor
      /// </summary>
      /// <param name="strUserName"></param>
      /// <param name="strPassword"></param>
      public Login(string strUserName, string strPassword, Server oServer) : this(oServer)
      {
         tbUserName.Text = strUserName;
         tbPassword.Text = strPassword;
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

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
         this.tbUserName = new System.Windows.Forms.TextBox();
         this.tbPassword = new System.Windows.Forms.TextBox();
         this.lUserName = new System.Windows.Forms.Label();
         this.lPassword = new System.Windows.Forms.Label();
         this.bCancel = new System.Windows.Forms.Button();
         this.bOK = new System.Windows.Forms.Button();
         this.SuspendLayout();
         // 
         // tbUserName
         // 
         this.tbUserName.Location = new System.Drawing.Point(80, 8);
         this.tbUserName.Name = "tbUserName";
         this.tbUserName.Size = new System.Drawing.Size(224, 20);
         this.tbUserName.TabIndex = 0;
         this.tbUserName.Text = "";
         // 
         // tbPassword
         // 
         this.tbPassword.Location = new System.Drawing.Point(80, 32);
         this.tbPassword.Name = "tbPassword";
         this.tbPassword.PasswordChar = '*';
         this.tbPassword.Size = new System.Drawing.Size(224, 20);
         this.tbPassword.TabIndex = 1;
         this.tbPassword.Text = "";
         // 
         // lUserName
         // 
         this.lUserName.Location = new System.Drawing.Point(8, 8);
         this.lUserName.Name = "lUserName";
         this.lUserName.Size = new System.Drawing.Size(64, 23);
         this.lUserName.TabIndex = 2;
         this.lUserName.Text = "User Name";
         this.lUserName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
         // 
         // lPassword
         // 
         this.lPassword.Location = new System.Drawing.Point(8, 32);
         this.lPassword.Name = "lPassword";
         this.lPassword.Size = new System.Drawing.Size(64, 23);
         this.lPassword.TabIndex = 3;
         this.lPassword.Text = "Password";
         this.lPassword.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
         // 
         // bCancel
         // 
         this.bCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
         this.bCancel.Location = new System.Drawing.Point(248, 56);
         this.bCancel.Name = "bCancel";
         this.bCancel.Size = new System.Drawing.Size(56, 24);
         this.bCancel.TabIndex = 3;
         this.bCancel.Text = "&Cancel";
         // 
         // bOK
         // 
         this.bOK.Location = new System.Drawing.Point(184, 56);
         this.bOK.Name = "bOK";
         this.bOK.Size = new System.Drawing.Size(56, 24);
         this.bOK.TabIndex = 2;
         this.bOK.Text = "&OK";
         this.bOK.Click += new System.EventHandler(this.bOK_Click);
         // 
         // Login
         // 
         this.AcceptButton = this.bOK;
         this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
         this.CancelButton = this.bCancel;
         this.ClientSize = new System.Drawing.Size(314, 87);
         this.ControlBox = false;
         this.Controls.Add(this.bOK);
         this.Controls.Add(this.bCancel);
         this.Controls.Add(this.lPassword);
         this.Controls.Add(this.lUserName);
         this.Controls.Add(this.tbPassword);
         this.Controls.Add(this.tbUserName);
         this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
         this.MaximizeBox = false;
         this.MinimizeBox = false;
         this.Name = "Login";
         this.ShowInTaskbar = false;
         this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
         this.Text = "Dap Server Login";
         this.Load += new System.EventHandler(this.Login_Load);
         this.ResumeLayout(false);

      }
		#endregion

      #region Event Handlers
      /// <summary>
      /// Set the focus correctly
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void Login_Load(object sender, System.EventArgs e)
      {
         // --- ensure the form is visible, so we can set the control focus ---

         this.Visible = true;

         if (tbUserName.Text != string.Empty && tbPassword.CanFocus)
            tbPassword.Focus();
      }

      /// <summary>
      /// Check to see if this login information is valid for the current server
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void bOK_Click(object sender, System.EventArgs e)
      {
         if (tbUserName.Text.Length == 0)
         {
            MessageBox.Show(this, "Please enter a user name.", "Invalid user name", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            tbUserName.Focus();
            return;
         }

         // --- see if this is a valid username/password pair for the current server ---

         try
         {
            if (!m_oServer.Command.AuthenticateUser(tbUserName.Text, tbPassword.Text, null))
            {
               MessageBox.Show(this, "Invalid user name and/or password.", "User login failed", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
               tbPassword.Focus();
               return;
            }
         } 
         catch (Exception ex)
         {
            MessageBox.Show(this, string.Format("An error occurred while attempting to authenticate you. {0}", ex.Message), "User login failed", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            return;
         }

         DialogResult = DialogResult.OK;

#if !DAPPLE
         Geosoft.GXNet.CSYS.SetString("DAPGETDATA", "USER_NAME", tbUserName.Text);
#endif
         Close();      
      }
      #endregion      
	}
}
