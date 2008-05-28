namespace Dapple
{
   partial class ServerList
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
			System.Windows.Forms.Label label1;
			this.c_cbServers = new System.Windows.Forms.ComboBox();
			this.c_lvLayers = new System.Windows.Forms.ListView();
			this.c_chLayers = new System.Windows.Forms.ColumnHeader();
			this.c_msContext = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.c_miAddLayer = new System.Windows.Forms.ToolStripMenuItem();
			this.c_miViewLegend = new System.Windows.Forms.ToolStripMenuItem();
			this.c_pBase = new System.Windows.Forms.Panel();
			this.c_oPageNavigator = new Dapple.PageNavigator();
			label1 = new System.Windows.Forms.Label();
			this.c_msContext.SuspendLayout();
			this.c_pBase.SuspendLayout();
			this.SuspendLayout();
			// 
			// label1
			// 
			label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			label1.AutoSize = true;
			label1.Location = new System.Drawing.Point(3, 177);
			label1.Name = "label1";
			label1.Size = new System.Drawing.Size(41, 13);
			label1.TabIndex = 0;
			label1.Text = "Server:";
			// 
			// c_cbServers
			// 
			this.c_cbServers.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
							| System.Windows.Forms.AnchorStyles.Right)));
			this.c_cbServers.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
			this.c_cbServers.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.c_cbServers.FormattingEnabled = true;
			this.c_cbServers.Location = new System.Drawing.Point(49, 174);
			this.c_cbServers.Name = "c_cbServers";
			this.c_cbServers.Size = new System.Drawing.Size(146, 21);
			this.c_cbServers.TabIndex = 1;
			this.c_cbServers.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.c_cbServers_DrawItem);
			this.c_cbServers.SelectedIndexChanged += new System.EventHandler(this.c_cbServers_SelectedIndexChanged);
			// 
			// c_lvLayers
			// 
			this.c_lvLayers.Alignment = System.Windows.Forms.ListViewAlignment.Left;
			this.c_lvLayers.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
							| System.Windows.Forms.AnchorStyles.Left)
							| System.Windows.Forms.AnchorStyles.Right)));
			this.c_lvLayers.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.c_chLayers});
			this.c_lvLayers.ContextMenuStrip = this.c_msContext;
			this.c_lvLayers.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.c_lvLayers.Location = new System.Drawing.Point(3, 32);
			this.c_lvLayers.Name = "c_lvLayers";
			this.c_lvLayers.Size = new System.Drawing.Size(192, 136);
			this.c_lvLayers.TabIndex = 2;
			this.c_lvLayers.UseCompatibleStateImageBehavior = false;
			this.c_lvLayers.View = System.Windows.Forms.View.Details;
			this.c_lvLayers.Resize += new System.EventHandler(this.cLayersListView_Resize);
			this.c_lvLayers.SelectedIndexChanged += new System.EventHandler(this.cLayersListView_SelectedIndexChanged);
			this.c_lvLayers.DoubleClick += new System.EventHandler(this.cLayersListView_DoubleClick);
			this.c_lvLayers.MouseMove += new System.Windows.Forms.MouseEventHandler(this.cLayersListView_MouseMove);
			this.c_lvLayers.MouseDown += new System.Windows.Forms.MouseEventHandler(this.cLayersListView_MouseDown);
			// 
			// c_chLayers
			// 
			this.c_chLayers.Text = "Layer";
			// 
			// c_msContext
			// 
			this.c_msContext.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.c_miAddLayer,
            this.c_miViewLegend});
			this.c_msContext.Name = "cLayerContextMenu";
			this.c_msContext.Size = new System.Drawing.Size(170, 48);
			this.c_msContext.Opening += new System.ComponentModel.CancelEventHandler(this.cLayerContextMenu_Opening);
			// 
			// c_miAddLayer
			// 
			this.c_miAddLayer.Image = global::Dapple.Properties.Resources.layers_add;
			this.c_miAddLayer.Name = "c_miAddLayer";
			this.c_miAddLayer.Size = new System.Drawing.Size(169, 22);
			this.c_miAddLayer.Text = "Add To Data Layers";
			this.c_miAddLayer.Click += new System.EventHandler(this.addToLayersToolStripMenuItem_Click);
			// 
			// c_miViewLegend
			// 
			this.c_miViewLegend.Image = global::Dapple.Properties.Resources.legend;
			this.c_miViewLegend.Name = "c_miViewLegend";
			this.c_miViewLegend.Size = new System.Drawing.Size(169, 22);
			this.c_miViewLegend.Text = "View Legend...";
			this.c_miViewLegend.Click += new System.EventHandler(this.viewLegendToolStripMenuItem_Click);
			// 
			// c_pBase
			// 
			this.c_pBase.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.c_pBase.Controls.Add(this.c_oPageNavigator);
			this.c_pBase.Controls.Add(this.c_lvLayers);
			this.c_pBase.Controls.Add(label1);
			this.c_pBase.Controls.Add(this.c_cbServers);
			this.c_pBase.Dock = System.Windows.Forms.DockStyle.Fill;
			this.c_pBase.Location = new System.Drawing.Point(0, 0);
			this.c_pBase.Name = "c_pBase";
			this.c_pBase.Size = new System.Drawing.Size(200, 200);
			this.c_pBase.TabIndex = 4;
			// 
			// c_oPageNavigator
			// 
			this.c_oPageNavigator.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
							| System.Windows.Forms.AnchorStyles.Right)));
			this.c_oPageNavigator.Location = new System.Drawing.Point(3, 3);
			this.c_oPageNavigator.Name = "c_oPageNavigator";
			this.c_oPageNavigator.Size = new System.Drawing.Size(192, 23);
			this.c_oPageNavigator.TabIndex = 3;
			// 
			// ServerList
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.c_pBase);
			this.Name = "ServerList";
			this.Size = new System.Drawing.Size(200, 200);
			this.Load += new System.EventHandler(this.ServerList_Load);
			this.c_msContext.ResumeLayout(false);
			this.c_pBase.ResumeLayout(false);
			this.c_pBase.PerformLayout();
			this.ResumeLayout(false);

      }

      #endregion

      private System.Windows.Forms.ComboBox c_cbServers;
      private System.Windows.Forms.ListView c_lvLayers;
      private System.Windows.Forms.ColumnHeader c_chLayers;
      private System.Windows.Forms.ContextMenuStrip c_msContext;
      private System.Windows.Forms.ToolStripMenuItem c_miAddLayer;
      private PageNavigator c_oPageNavigator;
      private System.Windows.Forms.Panel c_pBase;
      private System.Windows.Forms.ToolStripMenuItem c_miViewLegend;
   }
}
