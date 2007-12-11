namespace Dapple.Extract
{
   partial class DatasetDisclaimer
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
			this.pBottom = new System.Windows.Forms.Panel();
			this.bCancel = new System.Windows.Forms.Button();
			this.bAccept = new System.Windows.Forms.Button();
			this.lvDatasets = new System.Windows.Forms.ListView();
			this.chName = new System.Windows.Forms.ColumnHeader();
			this.wbDisclaimer = new System.Windows.Forms.WebBrowser();
			this.lDisclaimer = new System.Windows.Forms.Label();
			this.pBottom.SuspendLayout();
			this.SuspendLayout();
			// 
			// pBottom
			// 
			this.pBottom.Controls.Add(this.bCancel);
			this.pBottom.Controls.Add(this.bAccept);
			this.pBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.pBottom.Location = new System.Drawing.Point(0, 402);
			this.pBottom.Name = "pBottom";
			this.pBottom.Size = new System.Drawing.Size(457, 40);
			this.pBottom.TabIndex = 13;
			// 
			// bCancel
			// 
			this.bCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.bCancel.Location = new System.Drawing.Point(375, 6);
			this.bCancel.Name = "bCancel";
			this.bCancel.Size = new System.Drawing.Size(75, 23);
			this.bCancel.TabIndex = 1;
			this.bCancel.Text = "Cancel";
			this.bCancel.UseVisualStyleBackColor = true;
			this.bCancel.Click += new System.EventHandler(this.bCancel_Click);
			// 
			// bAccept
			// 
			this.bAccept.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.bAccept.Location = new System.Drawing.Point(294, 6);
			this.bAccept.Name = "bAccept";
			this.bAccept.Size = new System.Drawing.Size(75, 23);
			this.bAccept.TabIndex = 0;
			this.bAccept.Text = "Accept";
			this.bAccept.UseVisualStyleBackColor = true;
			this.bAccept.Click += new System.EventHandler(this.bAccept_Click);
			// 
			// lvDatasets
			// 
			this.lvDatasets.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.chName});
			this.lvDatasets.Dock = System.Windows.Forms.DockStyle.Top;
			this.lvDatasets.FullRowSelect = true;
			this.lvDatasets.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.lvDatasets.HideSelection = false;
			this.lvDatasets.Location = new System.Drawing.Point(0, 0);
			this.lvDatasets.MultiSelect = false;
			this.lvDatasets.Name = "lvDatasets";
			this.lvDatasets.Size = new System.Drawing.Size(457, 148);
			this.lvDatasets.TabIndex = 14;
			this.lvDatasets.UseCompatibleStateImageBehavior = false;
			this.lvDatasets.View = System.Windows.Forms.View.Details;
			this.lvDatasets.SelectedIndexChanged += new System.EventHandler(this.lvDatasets_SelectedIndexChanged);
			// 
			// chName
			// 
			this.chName.Text = "Datasets";
			this.chName.Width = 439;
			// 
			// wbDisclaimer
			// 
			this.wbDisclaimer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.wbDisclaimer.Location = new System.Drawing.Point(0, 161);
			this.wbDisclaimer.MinimumSize = new System.Drawing.Size(20, 20);
			this.wbDisclaimer.Name = "wbDisclaimer";
			this.wbDisclaimer.Size = new System.Drawing.Size(457, 241);
			this.wbDisclaimer.TabIndex = 15;
			// 
			// lDisclaimer
			// 
			this.lDisclaimer.AutoSize = true;
			this.lDisclaimer.Dock = System.Windows.Forms.DockStyle.Top;
			this.lDisclaimer.Location = new System.Drawing.Point(0, 148);
			this.lDisclaimer.Name = "lDisclaimer";
			this.lDisclaimer.Size = new System.Drawing.Size(176, 13);
			this.lDisclaimer.TabIndex = 16;
			this.lDisclaimer.Text = "Copyright / Restrictions / Disclaimer";
			// 
			// DatasetDisclaimer
			// 
			this.AcceptButton = this.bAccept;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.bCancel;
			this.ClientSize = new System.Drawing.Size(457, 442);
			this.Controls.Add(this.wbDisclaimer);
			this.Controls.Add(this.lDisclaimer);
			this.Controls.Add(this.lvDatasets);
			this.Controls.Add(this.pBottom);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "DatasetDisclaimer";
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Copyright / Restrictions / Disclaimer";
			this.pBottom.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

      }

      #endregion

      private System.Windows.Forms.Panel pBottom;
      private System.Windows.Forms.Button bCancel;
      private System.Windows.Forms.Button bAccept;
      private System.Windows.Forms.ListView lvDatasets;
      private System.Windows.Forms.ColumnHeader chName;
      private System.Windows.Forms.WebBrowser wbDisclaimer;
      private System.Windows.Forms.Label lDisclaimer;
   }
}

