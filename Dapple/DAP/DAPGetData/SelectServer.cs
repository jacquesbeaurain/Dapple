using System;
using System.Text;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

#if !ALMAAK
using Geosoft.GXNet;
#endif

namespace Geosoft.GX.DAPGetData
{
	/// <summary>
	/// Summary description for SelectServer.
	/// </summary>
	public class SelectServer : System.Windows.Forms.Form
	{
      #region Constants
      protected const Int32   UNSUPPORTED_SERVER_VERSION = 3;
      protected const Int32   DISABLED_SERVER = 2;      
      protected const Int32   OFFLINE_SERVER = 1;
      protected const Int32   ONLINE_SERVER = 0;
      
      #endregion

      #region Member Variables
      protected Server  m_hOriginalSelectedServer;
#if !ALMAAK
      protected string  m_strUrl = string.Empty;
#endif
      #endregion

      private System.Windows.Forms.Panel pButtons;
      private System.Windows.Forms.ListView lvServers;
      private System.Windows.Forms.Panel pCommands;
      private System.Windows.Forms.ColumnHeader chName;
      private System.Windows.Forms.ColumnHeader chUrl;
      private System.Windows.Forms.Button bRemove;
      private System.Windows.Forms.Button bAdd;
      private System.Windows.Forms.ImageList ilServerIcons;
      private System.Windows.Forms.Button bDisable;
      private System.Windows.Forms.Button bSetAsActive;
      private System.Windows.Forms.Button bEnable;
      private System.Windows.Forms.ColumnHeader chActive;
      private System.Windows.Forms.Button bOK;
      private System.Windows.Forms.Label label1;
      private System.ComponentModel.IContainer components;

      #region Events
      /// <summary>
      /// Define the select server event
      /// </summary>
      public event ServerSelectHandler  ServerSelect;

      /// <summary>
      /// Invoke the delegatae registered with the select server event
      /// </summary>
      protected virtual void OnSelectServer(ServerSelectArgs e) 
      {
         if (ServerSelect != null) 
         {
            ServerSelect(this, e);
         }
      }

      /// <summary>
      /// Define the remove server event
      /// </summary>
      public event RemoveServerHandler  RemoveServer;

      /// <summary>
      /// Invoke the delegatae registered with the select server event
      /// </summary>
      protected virtual void OnRemoveServer(ServerSelectArgs e) 
      {
         if (RemoveServer != null) 
         {
            RemoveServer(this, e);
         }
      }
      #endregion      

      #region Constructor/Destructor
      /// <summary>
      /// Default constructor
      /// </summary>
		public SelectServer()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

#if !ALMAAK
         Constant.GetSelectedServerInSettingsMeta(out m_strUrl);
#endif

         PopulateServerList();         
#if ALMAAK
         m_hOriginalSelectedServer = GetDapData.Instance.CurServer;
#endif
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
         this.components = new System.ComponentModel.Container();
         System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(SelectServer));
         this.pButtons = new System.Windows.Forms.Panel();
         this.label1 = new System.Windows.Forms.Label();
         this.bOK = new System.Windows.Forms.Button();
         this.lvServers = new System.Windows.Forms.ListView();
         this.chActive = new System.Windows.Forms.ColumnHeader();
         this.chName = new System.Windows.Forms.ColumnHeader();
         this.chUrl = new System.Windows.Forms.ColumnHeader();
         this.ilServerIcons = new System.Windows.Forms.ImageList(this.components);
         this.bRemove = new System.Windows.Forms.Button();
         this.pCommands = new System.Windows.Forms.Panel();
         this.bEnable = new System.Windows.Forms.Button();
         this.bSetAsActive = new System.Windows.Forms.Button();
         this.bDisable = new System.Windows.Forms.Button();
         this.bAdd = new System.Windows.Forms.Button();
         this.pButtons.SuspendLayout();
         this.pCommands.SuspendLayout();
         this.SuspendLayout();
         // 
         // pButtons
         // 
         this.pButtons.Controls.Add(this.label1);
         this.pButtons.Controls.Add(this.bOK);
         this.pButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
         this.pButtons.Location = new System.Drawing.Point(0, 302);
         this.pButtons.Name = "pButtons";
         this.pButtons.Size = new System.Drawing.Size(416, 32);
         this.pButtons.TabIndex = 0;
         // 
         // label1
         // 
         this.label1.Location = new System.Drawing.Point(8, 8);
         this.label1.Name = "label1";
         this.label1.TabIndex = 1;
         this.label1.Text = "* Default Server";
         // 
         // bOK
         // 
         this.bOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
         this.bOK.DialogResult = System.Windows.Forms.DialogResult.OK;
         this.bOK.Location = new System.Drawing.Point(336, 0);
         this.bOK.Name = "bOK";
         this.bOK.Size = new System.Drawing.Size(72, 23);
         this.bOK.TabIndex = 0;
         this.bOK.Text = "OK";
         // 
         // lvServers
         // 
         this.lvServers.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                                                                                    this.chActive,
                                                                                    this.chName,
                                                                                    this.chUrl});
         this.lvServers.Dock = System.Windows.Forms.DockStyle.Fill;
         this.lvServers.FullRowSelect = true;
         this.lvServers.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
         this.lvServers.HideSelection = false;
         this.lvServers.Location = new System.Drawing.Point(0, 0);
         this.lvServers.MultiSelect = false;
         this.lvServers.Name = "lvServers";
         this.lvServers.Size = new System.Drawing.Size(328, 302);
         this.lvServers.SmallImageList = this.ilServerIcons;
         this.lvServers.TabIndex = 1;
         this.lvServers.View = System.Windows.Forms.View.Details;
         this.lvServers.SelectedIndexChanged += new System.EventHandler(this.lvServers_SelectedIndexChanged);
         // 
         // chActive
         // 
         this.chActive.Text = "Default";
         this.chActive.Width = 50;
         // 
         // chName
         // 
         this.chName.Text = "Name";
         this.chName.Width = 129;
         // 
         // chUrl
         // 
         this.chUrl.Text = "Url";
         this.chUrl.Width = 134;
         // 
         // ilServerIcons
         // 
         this.ilServerIcons.ImageSize = new System.Drawing.Size(16, 16);
         this.ilServerIcons.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ilServerIcons.ImageStream")));
         this.ilServerIcons.TransparentColor = System.Drawing.Color.Transparent;
         // 
         // bRemove
         // 
         this.bRemove.Location = new System.Drawing.Point(8, 40);
         this.bRemove.Name = "bRemove";
         this.bRemove.Size = new System.Drawing.Size(72, 23);
         this.bRemove.TabIndex = 4;
         this.bRemove.Text = "Remove";
         this.bRemove.Click += new System.EventHandler(this.bRemove_Click);
         // 
         // pCommands
         // 
         this.pCommands.Controls.Add(this.bEnable);
         this.pCommands.Controls.Add(this.bSetAsActive);
         this.pCommands.Controls.Add(this.bDisable);
         this.pCommands.Controls.Add(this.bAdd);
         this.pCommands.Controls.Add(this.bRemove);
         this.pCommands.Dock = System.Windows.Forms.DockStyle.Right;
         this.pCommands.Location = new System.Drawing.Point(328, 0);
         this.pCommands.Name = "pCommands";
         this.pCommands.Size = new System.Drawing.Size(88, 302);
         this.pCommands.TabIndex = 5;
         // 
         // bEnable
         // 
         this.bEnable.Location = new System.Drawing.Point(8, 104);
         this.bEnable.Name = "bEnable";
         this.bEnable.Size = new System.Drawing.Size(72, 23);
         this.bEnable.TabIndex = 8;
         this.bEnable.Text = "Enable";
         this.bEnable.Click += new System.EventHandler(this.bEnable_Click);
         // 
         // bSetAsActive
         // 
         this.bSetAsActive.Location = new System.Drawing.Point(8, 200);
         this.bSetAsActive.Name = "bSetAsActive";
         this.bSetAsActive.Size = new System.Drawing.Size(72, 40);
         this.bSetAsActive.TabIndex = 7;
         this.bSetAsActive.Text = "Set As \nDefault";
         this.bSetAsActive.Click += new System.EventHandler(this.bSetAsActive_Click);
         // 
         // bDisable
         // 
         this.bDisable.Location = new System.Drawing.Point(8, 136);
         this.bDisable.Name = "bDisable";
         this.bDisable.Size = new System.Drawing.Size(72, 23);
         this.bDisable.TabIndex = 6;
         this.bDisable.Text = "Disable";
         this.bDisable.Click += new System.EventHandler(this.bDisable_Click);
         // 
         // bAdd
         // 
         this.bAdd.Location = new System.Drawing.Point(8, 8);
         this.bAdd.Name = "bAdd";
         this.bAdd.Size = new System.Drawing.Size(72, 23);
         this.bAdd.TabIndex = 5;
         this.bAdd.Text = "Add";
         this.bAdd.Click += new System.EventHandler(this.bAdd_Click);
         // 
         // SelectServer
         // 
         this.AcceptButton = this.bOK;
         this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
         this.CancelButton = this.bOK;
         this.ClientSize = new System.Drawing.Size(416, 334);
         this.Controls.Add(this.lvServers);
         this.Controls.Add(this.pCommands);
         this.Controls.Add(this.pButtons);
         this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
         this.MaximizeBox = false;
         this.MinimizeBox = false;
         this.Name = "SelectServer";
         this.ShowInTaskbar = false;
         this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
         this.Text = "DAP Server Manager";
         this.Closing += new System.ComponentModel.CancelEventHandler(this.SelectServer_Closing);
         this.pButtons.ResumeLayout(false);
         this.pCommands.ResumeLayout(false);
         this.ResumeLayout(false);

      }
		#endregion

      #region Protected Members
      /// <summary>
      /// Populate the list of servers
      /// </summary>
      protected void PopulateServerList()
      {
         bool  bSelected = false;
         
         lvServers.Items.Clear();         

         for (Int32 iIndex = 0; iIndex < GetDapData.Instance.FullServerList.Count; iIndex++)
         {
            Server   hServer = (Server)GetDapData.Instance.FullServerList.GetByIndex(iIndex);            

            ListViewItem   hItem = new ListViewItem();
            hItem.Tag = hServer;
            hItem.Text = string.Empty;
            hItem.SubItems.Add(hServer.Name);
            hItem.SubItems.Add(hServer.Url);

            if (hServer.MajorVersion < 6 || hServer.MajorVersion == 6 && hServer.MinorVersion < 2)
            {
               hItem.ImageIndex = UNSUPPORTED_SERVER_VERSION;
               hItem.ForeColor = System.Drawing.Color.Red;
            }
            else 
            {
               if (hServer.Status == Server.ServerStatus.OffLine) 
               {
                  hItem.ImageIndex = OFFLINE_SERVER;
                  hItem.ForeColor = System.Drawing.Color.Gray;
               }
               else if (hServer.Status == Server.ServerStatus.Disabled)
               {
                  hItem.ImageIndex = DISABLED_SERVER;
                  hItem.ForeColor = System.Drawing.Color.Gray;
               }
               else
               {
                  hItem.ImageIndex = ONLINE_SERVER;
                  hItem.ForeColor = System.Drawing.Color.Black;
               }
            }

#if !ALMAAK
            if (string.Compare(hServer.Url, m_strUrl) == 0) 
            {
               hItem.Text = "*";
               hItem.Selected = true;
               bSelected = true;
               m_hOriginalSelectedServer = hServer;
            }
#else
            if (hServer == GetDapData.Instance.CurServer) 
            {
               hItem.Text = "*";
               hItem.Selected = true;
               bSelected = true;
            }
#endif

            lvServers.Items.Add(hItem);
         }

         if (!bSelected)
         {
            foreach (ListViewItem oItem in lvServers.Items)
            {
               Server hServer = (Server)oItem.Tag;

               if (hServer.Status == Server.ServerStatus.OnLine || hServer.Status == Server.ServerStatus.Maintenance)
               {
                  SetAsActive(oItem);
               }
            }
         }
      }

      /// <summary>
      /// Set this list view item as the active one
      /// </summary>
      /// <param name="oItem"></param>
      protected void SetAsActive(ListViewItem oItem)
      {
         if (oItem.ImageIndex == UNSUPPORTED_SERVER_VERSION)
         {
            MessageBox.Show("Only dap servers version 6.2 or greater are supported through this interface. Please use the old getdapdata dialog to retrieve data from this server.", "Unsupported dap server", MessageBoxButtons.OK, MessageBoxIcon.Information);
         }
         else if (oItem.ImageIndex == OFFLINE_SERVER)
         {
            MessageBox.Show("The selected dap server is presently not available. Please try again later.", "Selected dap server is unavailable", MessageBoxButtons.OK, MessageBoxIcon.Information);
         }
         else if (oItem.ImageIndex == DISABLED_SERVER)
         {
            MessageBox.Show("Only enabled servers can be set as active. Please choose another server.", "Selected dap server is disabled", MessageBoxButtons.OK, MessageBoxIcon.Information);
         }
         else 
         {            
            // --- clear the currently active server ---
            
            foreach (ListViewItem oTemp in lvServers.Items)
            {
               oTemp.Text = string.Empty;
            }

            oItem.Text = "*";
         }      
      }
      #endregion

      #region Event Handlers               
      /// <summary>
      /// Add a new server to the list
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void bAdd_Click(object sender, System.EventArgs e)
      {
         Server hServer;
         bool   bError = false;
  
         if (!GetDapData.Instance.AddServer(out hServer))
         {
            string strUrl = string.Empty;
            if (hServer != null) strUrl = hServer.Url;

            MessageBox.Show("Error adding dap server " + strUrl + " to the list.\n\r", "Error Adding Dap Server", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            bError = true;
         } 
         PopulateServerList();

         // --- we just added our first item, make it the active one ---

         if (!bError)
         {
            if (lvServers.Items.Count == 1)
            {
               ListViewItem hItem = lvServers.Items[0];
               SetAsActive(hItem);
            } 
            else 
            {
               bRemove.Enabled = true;
            }
         }
      }

      /// <summary>
      /// Remove the selected servers from the list
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void bRemove_Click(object sender, System.EventArgs e)
      {
         Int32 iCount = lvServers.SelectedItems.Count;
         bool  bFound = true;

         if (lvServers.SelectedItems.Count > 0)
         {
            ListViewItem hItem = lvServers.SelectedItems[0];

            // --- if this is a selected server then we must choose a new one ---

            if (hItem.Text == "*") 
            {
               bFound = false;
               foreach (ListViewItem oItem in lvServers.Items)
               {
                  if (oItem != hItem && oItem.ImageIndex == ONLINE_SERVER) 
                  {
                     SetAsActive(oItem);
                     bFound = true;
                     break;
                  }
               }
            }
            
            if (bFound)
            {
               OnRemoveServer(new ServerSelectArgs(hItem.Index));
               lvServers.Items.RemoveAt(hItem.Index);            
            } 
            else 
            {
               MessageBox.Show("Unable to disable server as it is the last one in the list.", "Only available server", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
         }               
      }

      /// <summary>
      /// Enable a disabled server
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void bEnable_Click(object sender, System.EventArgs e)
      {
         if (lvServers.SelectedItems.Count > 0)
         {
            ListViewItem hItem = lvServers.SelectedItems[0];

            if (hItem.ImageIndex == DISABLED_SERVER)
            {
               hItem.ImageIndex = ONLINE_SERVER;
               hItem.ForeColor = System.Drawing.Color.Black;

               GetDapData.Instance.EnableServer((Server)hItem.Tag);

               bDisable.Enabled = true;
               bEnable.Enabled = false;
               bSetAsActive.Enabled = true;
            }
            else if (hItem.ImageIndex == UNSUPPORTED_SERVER_VERSION || hItem.ImageIndex == OFFLINE_SERVER)
            {
               Server   oServer = (Server)hItem.Tag;
               Server   oRetServer = null;

               // --- attempt to add this server again ---
               
               if (GetDapData.Instance.AddServer(oServer.Url, out oRetServer) && oRetServer != null) 
               {
                  hItem.Tag = oRetServer;
                  bDisable.Enabled = true;
                  bEnable.Enabled = false;
                  bSetAsActive.Enabled = true;
               }
               else 
               {
                  MessageBox.Show("The selected dap server is presently not available or is of a previous version. Please try again later or with the pre 6.2 getdapdata dialog.", "Selected dap server is unavailable", MessageBoxButtons.OK, MessageBoxIcon.Information);
               }
            }            
         }
      }

      /// <summary>
      /// Disable an enabled server
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void bDisable_Click(object sender, System.EventArgs e)
      {
         bool bFound = true;
         if (lvServers.SelectedItems.Count > 0)
         {            
            ListViewItem hItem = lvServers.SelectedItems[0];

            if (hItem.ImageIndex == ONLINE_SERVER)
            {
               // --- if this is a selected server then we must choose a new one ---

               if (hItem.Text == "*") 
               {
                  bFound = false;
                  foreach (ListViewItem oItem in lvServers.Items)
                  {
                     if (oItem != hItem && oItem.ImageIndex == ONLINE_SERVER) 
                     {
                        SetAsActive(oItem);
                        bFound = true;
                        break;
                     }
                  }
               }
            
               if (bFound)
               {
                  hItem.ImageIndex = DISABLED_SERVER;
                  hItem.ForeColor = System.Drawing.Color.Gray;
                  GetDapData.Instance.DisableServer((Server)hItem.Tag);

                  bDisable.Enabled = false;
                  bEnable.Enabled = true;
                  bSetAsActive.Enabled = false;
               } 
               else 
               {
                  MessageBox.Show("Unable to disable server as it is the last one in the list.", "Only available server", MessageBoxButtons.OK, MessageBoxIcon.Information);
               }
            }
            else if (hItem.ImageIndex == UNSUPPORTED_SERVER_VERSION)
            {
               MessageBox.Show("Only dap servers version 6.2 or greater are supported through this interface. Please use the old getdapdata dialog to retrieve data from this server.", "Unsupported dap server", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else if (hItem.ImageIndex == OFFLINE_SERVER)
            {
               MessageBox.Show("The selected dap server is presently not available. Please try again later.", "Selected dap server is unavailable", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }            
         }
      }

      /// <summary>
      /// Set the selected server as active
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void bSetAsActive_Click(object sender, System.EventArgs e)
      {         
         if (lvServers.SelectedItems.Count > 0)
         {            
            ListViewItem hItem = lvServers.SelectedItems[0];

            SetAsActive(hItem);
         }
      }      

      /// <summary>
      /// Active the currently selected server
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void SelectServer_Closing(object sender, System.ComponentModel.CancelEventArgs e)
      {
         bool  bNewServer = false;
         int   iIndex = 0;

         foreach (ListViewItem oItem in lvServers.Items)
         {
            if (oItem.Text == "*") 
            {
               if (oItem.Tag != m_hOriginalSelectedServer) 
               {
                  bNewServer = true;
                  iIndex = oItem.Index;
               }
            }             
         }

         if (bNewServer)
            OnSelectServer(new ServerSelectArgs(iIndex));
      }      

      /// <summary>
      /// The selected index has changed adjust the state of the buttons accordingly
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void lvServers_SelectedIndexChanged(object sender, System.EventArgs e)
      {
         if (lvServers.SelectedIndices.Count == 0)
         {
            bRemove.Enabled = false;
            bEnable.Enabled = false;
            bDisable.Enabled = false;
            bSetAsActive.Enabled = false;
         } 
         else 
         {
            Server oServer = (Server)lvServers.SelectedItems[0].Tag;

            if (lvServers.Items.Count > 1) 
               bRemove.Enabled = true;
            else
               bRemove.Enabled = false;

            if (oServer.Status == Server.ServerStatus.Disabled)
            {
               bDisable.Enabled = false;
               bEnable.Enabled = true;
               bSetAsActive.Enabled = false;
            } 
            else if (oServer.Status == Server.ServerStatus.OnLine || oServer.Status == Server.ServerStatus.Maintenance)
            {
               bDisable.Enabled = true;
               bEnable.Enabled = false;
               bSetAsActive.Enabled = true;
            }
            else
            {
               bDisable.Enabled = false;
               bEnable.Enabled = true;
               bSetAsActive.Enabled = false;
            }
         }
      }
      #endregion
	}
}
