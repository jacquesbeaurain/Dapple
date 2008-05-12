namespace Dapple.CustomControls
{
   partial class DappleSearchList
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

      #region Component Designer generated code

      /// <summary> 
      /// Required method for Designer support - do not modify 
      /// the contents of this method with the code editor.
      /// </summary>
      private void InitializeComponent()
      {
			this.components = new System.ComponentModel.Container();
			this.c_lbResults = new System.Windows.Forms.ListBox();
			this.c_msContext = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.c_miAddLayer = new System.Windows.Forms.ToolStripMenuItem();
			this.c_pBase = new System.Windows.Forms.Panel();
			this.c_oPageNavigator = new Dapple.PageNavigator();
			this.c_tsTabToolstrip = new Dapple.CustomControls.TabToolStrip();
			this.c_msContext.SuspendLayout();
			this.c_pBase.SuspendLayout();
			this.SuspendLayout();
			// 
			// c_lbResults
			// 
			this.c_lbResults.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
							| System.Windows.Forms.AnchorStyles.Left)
							| System.Windows.Forms.AnchorStyles.Right)));
			this.c_lbResults.ContextMenuStrip = this.c_msContext;
			this.c_lbResults.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
			this.c_lbResults.FormattingEnabled = true;
			this.c_lbResults.HorizontalScrollbar = true;
			this.c_lbResults.IntegralHeight = false;
			this.c_lbResults.Location = new System.Drawing.Point(3, 32);
			this.c_lbResults.Name = "c_lbResults";
			this.c_lbResults.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
			this.c_lbResults.Size = new System.Drawing.Size(192, 138);
			this.c_lbResults.TabIndex = 0;
			this.c_lbResults.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.cResultListBox_MouseDoubleClick);
			this.c_lbResults.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.cResultListBox_DrawItem);
			this.c_lbResults.MeasureItem += new System.Windows.Forms.MeasureItemEventHandler(this.cResultListBox_MeasureItem);
			this.c_lbResults.SelectedIndexChanged += new System.EventHandler(this.c_lbResults_SelectedIndexChanged);
			this.c_lbResults.MouseMove += new System.Windows.Forms.MouseEventHandler(this.cResultListBox_MouseMove);
			this.c_lbResults.MouseDown += new System.Windows.Forms.MouseEventHandler(this.cResultListBox_MouseDown);
			// 
			// c_msContext
			// 
			this.c_msContext.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.c_miAddLayer});
			this.c_msContext.Name = "cContextMenu";
			this.c_msContext.Size = new System.Drawing.Size(168, 26);
			this.c_msContext.Opening += new System.ComponentModel.CancelEventHandler(this.cContextMenu_Opening);
			// 
			// c_miAddLayer
			// 
			this.c_miAddLayer.Image = global::Dapple.Properties.Resources.layers_add;
			this.c_miAddLayer.Name = "c_miAddLayer";
			this.c_miAddLayer.Size = new System.Drawing.Size(167, 22);
			this.c_miAddLayer.Text = "Add to Data Layers";
			this.c_miAddLayer.Click += new System.EventHandler(this.addLayerToolStripMenuItem_Click);
			// 
			// c_pBase
			// 
			this.c_pBase.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
							| System.Windows.Forms.AnchorStyles.Left)
							| System.Windows.Forms.AnchorStyles.Right)));
			this.c_pBase.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.c_pBase.Controls.Add(this.c_lbResults);
			this.c_pBase.Controls.Add(this.c_oPageNavigator);
			this.c_pBase.Location = new System.Drawing.Point(0, 0);
			this.c_pBase.Name = "c_pBase";
			this.c_pBase.Size = new System.Drawing.Size(200, 175);
			this.c_pBase.TabIndex = 3;
			// 
			// c_oPageNavigator
			// 
			this.c_oPageNavigator.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
							| System.Windows.Forms.AnchorStyles.Right)));
			this.c_oPageNavigator.Location = new System.Drawing.Point(3, 3);
			this.c_oPageNavigator.Name = "c_oPageNavigator";
			this.c_oPageNavigator.Size = new System.Drawing.Size(192, 23);
			this.c_oPageNavigator.TabIndex = 1;
			// 
			// c_tsTabToolstrip
			// 
			this.c_tsTabToolstrip.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.c_tsTabToolstrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.c_tsTabToolstrip.Location = new System.Drawing.Point(0, 175);
			this.c_tsTabToolstrip.Name = "c_tsTabToolstrip";
			this.c_tsTabToolstrip.Size = new System.Drawing.Size(200, 25);
			this.c_tsTabToolstrip.TabIndex = 2;
			this.c_tsTabToolstrip.TabStop = true;
			this.c_tsTabToolstrip.Text = "tabToolbar1";
			// 
			// DappleSearchList
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.c_pBase);
			this.Controls.Add(this.c_tsTabToolstrip);
			this.Name = "DappleSearchList";
			this.Size = new System.Drawing.Size(200, 200);
			this.c_msContext.ResumeLayout(false);
			this.c_pBase.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

      }

      #endregion

      private System.Windows.Forms.ListBox c_lbResults;
      private PageNavigator c_oPageNavigator;
      private TabToolStrip c_tsTabToolstrip;
      private System.Windows.Forms.Panel c_pBase;
      private System.Windows.Forms.ContextMenuStrip c_msContext;
      private System.Windows.Forms.ToolStripMenuItem c_miAddLayer;
   }
}
