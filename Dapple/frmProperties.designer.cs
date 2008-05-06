namespace Dapple
{
   partial class frmProperties
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmProperties));
			this.c_pgProperties = new System.Windows.Forms.PropertyGrid();
			this.c_pButtons = new System.Windows.Forms.Panel();
			this.c_bCancel = new System.Windows.Forms.Button();
			this.c_bOK = new System.Windows.Forms.Button();
			this.c_pButtons.SuspendLayout();
			this.SuspendLayout();
			// 
			// c_pgProperties
			// 
			this.c_pgProperties.Dock = System.Windows.Forms.DockStyle.Fill;
			this.c_pgProperties.Location = new System.Drawing.Point(0, 0);
			this.c_pgProperties.Name = "c_pgProperties";
			this.c_pgProperties.Size = new System.Drawing.Size(570, 353);
			this.c_pgProperties.TabIndex = 0;
			// 
			// c_pButtons
			// 
			this.c_pButtons.BackColor = System.Drawing.Color.Transparent;
			this.c_pButtons.Controls.Add(this.c_bCancel);
			this.c_pButtons.Controls.Add(this.c_bOK);
			this.c_pButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.c_pButtons.Location = new System.Drawing.Point(0, 353);
			this.c_pButtons.Name = "c_pButtons";
			this.c_pButtons.Size = new System.Drawing.Size(570, 29);
			this.c_pButtons.TabIndex = 1;
			// 
			// c_bCancel
			// 
			this.c_bCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.c_bCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.c_bCancel.Location = new System.Drawing.Point(483, 3);
			this.c_bCancel.Name = "c_bCancel";
			this.c_bCancel.Size = new System.Drawing.Size(75, 23);
			this.c_bCancel.TabIndex = 1;
			this.c_bCancel.Text = "&Cancel";
			this.c_bCancel.UseVisualStyleBackColor = true;
			this.c_bCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// c_bOK
			// 
			this.c_bOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.c_bOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.c_bOK.Location = new System.Drawing.Point(402, 3);
			this.c_bOK.Name = "c_bOK";
			this.c_bOK.Size = new System.Drawing.Size(75, 23);
			this.c_bOK.TabIndex = 0;
			this.c_bOK.Text = "&OK";
			this.c_bOK.UseVisualStyleBackColor = true;
			this.c_bOK.Click += new System.EventHandler(this.btnOk_Click);
			// 
			// frmProperties
			// 
			this.AcceptButton = this.c_bOK;
			this.CancelButton = this.c_bCancel;
			this.ClientSize = new System.Drawing.Size(570, 382);
			this.Controls.Add(this.c_pgProperties);
			this.Controls.Add(this.c_pButtons);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "frmProperties";
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Properties";
			this.c_pButtons.ResumeLayout(false);
			this.ResumeLayout(false);

      }

      #endregion

      private System.Windows.Forms.PropertyGrid c_pgProperties;
      private System.Windows.Forms.Panel c_pButtons;
      private System.Windows.Forms.Button c_bCancel;
      private System.Windows.Forms.Button c_bOK;
   }
}

