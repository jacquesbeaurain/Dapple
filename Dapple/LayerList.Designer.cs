namespace Dapple
{
   partial class LayerList
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
			System.Windows.Forms.ToolStripSeparator cSliderMenuSeparator;
			System.Windows.Forms.ToolStrip c_tsControls;
			this.c_bGoToLayer = new System.Windows.Forms.ToolStripButton();
			this.c_RemoveLayer = new System.Windows.Forms.ToolStripButton();
			this.c_bExtract = new System.Windows.Forms.ToolStripButton();
			this.c_Snapshot = new System.Windows.Forms.ToolStripButton();
			this.c_lvLayers = new System.Windows.Forms.ListView();
			this.c_chLayers = new System.Windows.Forms.ColumnHeader();
			this.c_msContext = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.c_miGoToLayer = new System.Windows.Forms.ToolStripMenuItem();
			this.c_miProperties = new System.Windows.Forms.ToolStripMenuItem();
			this.c_miViewLegend = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.c_miRefreshLayer = new System.Windows.Forms.ToolStripMenuItem();
			this.c_miClearLayerChache = new System.Windows.Forms.ToolStripMenuItem();
			this.c_miRemoveLayer = new System.Windows.Forms.ToolStripMenuItem();
			this.c_miAddOrGoToServer = new System.Windows.Forms.ToolStripMenuItem();
			this.c_tbTransparency = new Dapple.TrackBarWithPaint();
			cSliderMenuSeparator = new System.Windows.Forms.ToolStripSeparator();
			c_tsControls = new System.Windows.Forms.ToolStrip();
			c_tsControls.SuspendLayout();
			this.c_msContext.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.c_tbTransparency)).BeginInit();
			this.SuspendLayout();
			// 
			// cSliderMenuSeparator
			// 
			cSliderMenuSeparator.Name = "cSliderMenuSeparator";
			cSliderMenuSeparator.Size = new System.Drawing.Size(6, 25);
			// 
			// c_tsControls
			// 
			c_tsControls.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
							| System.Windows.Forms.AnchorStyles.Right)));
			c_tsControls.AutoSize = false;
			c_tsControls.Dock = System.Windows.Forms.DockStyle.None;
			c_tsControls.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			c_tsControls.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            cSliderMenuSeparator,
            this.c_bGoToLayer,
            this.c_RemoveLayer,
            this.c_bExtract,
            this.c_Snapshot});
			c_tsControls.Location = new System.Drawing.Point(46, 0);
			c_tsControls.Name = "c_tsControls";
			c_tsControls.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
			c_tsControls.Size = new System.Drawing.Size(151, 25);
			c_tsControls.TabIndex = 1;
			c_tsControls.TabStop = true;
			c_tsControls.Text = "toolStrip1";
			// 
			// c_bGoToLayer
			// 
			this.c_bGoToLayer.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.c_bGoToLayer.Image = global::Dapple.Properties.Resources.layers_goto;
			this.c_bGoToLayer.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.c_bGoToLayer.Name = "c_bGoToLayer";
			this.c_bGoToLayer.Size = new System.Drawing.Size(23, 22);
			this.c_bGoToLayer.Text = "Go To Button";
			this.c_bGoToLayer.ToolTipText = "Go to";
			this.c_bGoToLayer.Click += new System.EventHandler(this.cGoToButton_Click);
			// 
			// c_RemoveLayer
			// 
			this.c_RemoveLayer.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.c_RemoveLayer.Image = global::Dapple.Properties.Resources.layers_remove;
			this.c_RemoveLayer.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.c_RemoveLayer.Name = "c_RemoveLayer";
			this.c_RemoveLayer.Size = new System.Drawing.Size(23, 22);
			this.c_RemoveLayer.Text = "Remove Layer button";
			this.c_RemoveLayer.ToolTipText = "Remove data layers";
			this.c_RemoveLayer.Click += new System.EventHandler(this.cRemoveLayerButton_Click);
			// 
			// c_bExtract
			// 
			this.c_bExtract.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.c_bExtract.Image = global::Dapple.Properties.Resources.layers_download;
			this.c_bExtract.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.c_bExtract.Name = "c_bExtract";
			this.c_bExtract.Size = new System.Drawing.Size(23, 22);
			this.c_bExtract.Text = "Extract Button";
			this.c_bExtract.ToolTipText = "Extract data layers...";
			this.c_bExtract.Click += new System.EventHandler(this.cExtractButton_Click);
			// 
			// c_Snapshot
			// 
			this.c_Snapshot.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.c_Snapshot.Image = global::Dapple.Properties.Resources.snapshot;
			this.c_Snapshot.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.c_Snapshot.Name = "c_Snapshot";
			this.c_Snapshot.Size = new System.Drawing.Size(23, 22);
			this.c_Snapshot.Text = "Take Snapshot Button";
			this.c_Snapshot.ToolTipText = "Create GeoTIFF snapshot...";
			this.c_Snapshot.Click += new System.EventHandler(this.cExportButton_Click);
			// 
			// c_lvLayers
			// 
			this.c_lvLayers.AllowDrop = true;
			this.c_lvLayers.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
							| System.Windows.Forms.AnchorStyles.Left)
							| System.Windows.Forms.AnchorStyles.Right)));
			this.c_lvLayers.CheckBoxes = true;
			this.c_lvLayers.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.c_chLayers});
			this.c_lvLayers.ContextMenuStrip = this.c_msContext;
			this.c_lvLayers.ForeColor = System.Drawing.Color.ForestGreen;
			this.c_lvLayers.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.c_lvLayers.HideSelection = false;
			this.c_lvLayers.Location = new System.Drawing.Point(0, 25);
			this.c_lvLayers.Name = "c_lvLayers";
			this.c_lvLayers.Size = new System.Drawing.Size(197, 96);
			this.c_lvLayers.TabIndex = 2;
			this.c_lvLayers.UseCompatibleStateImageBehavior = false;
			this.c_lvLayers.View = System.Windows.Forms.View.Details;
			this.c_lvLayers.SelectedIndexChanged += new System.EventHandler(this.cLayerList_SelectedIndexChanged);
			this.c_lvLayers.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.cLayerList_ItemCheck);
			this.c_lvLayers.DragDrop += new System.Windows.Forms.DragEventHandler(this.cLayerList_DragDrop);
			this.c_lvLayers.MouseMove += new System.Windows.Forms.MouseEventHandler(this.cLayerList_MouseMove);
			this.c_lvLayers.MouseDown += new System.Windows.Forms.MouseEventHandler(this.c_lvLayers_MouseDown);
			this.c_lvLayers.DragEnter += new System.Windows.Forms.DragEventHandler(this.cLayerList_DragEnter);
			this.c_lvLayers.KeyUp += new System.Windows.Forms.KeyEventHandler(this.cLayerList_KeyUp);
			this.c_lvLayers.DragLeave += new System.EventHandler(this.cLayerList_DragLeave);
			this.c_lvLayers.DragOver += new System.Windows.Forms.DragEventHandler(this.cLayerList_DragOver);
			// 
			// c_msContext
			// 
			this.c_msContext.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.c_miGoToLayer,
            this.c_miProperties,
            this.c_miViewLegend,
            this.toolStripSeparator1,
            this.c_miRefreshLayer,
            this.c_miClearLayerChache,
            this.c_miRemoveLayer,
            this.c_miAddOrGoToServer});
			this.c_msContext.Name = "cLayerListContextMenu";
			this.c_msContext.Size = new System.Drawing.Size(158, 164);
			this.c_msContext.Opening += new System.ComponentModel.CancelEventHandler(this.cLayerListContextMenu_Opening);
			// 
			// c_miGoToLayer
			// 
			this.c_miGoToLayer.Image = global::Dapple.Properties.Resources.layers_goto;
			this.c_miGoToLayer.Name = "c_miGoToLayer";
			this.c_miGoToLayer.Size = new System.Drawing.Size(157, 22);
			this.c_miGoToLayer.Text = "Go To";
			this.c_miGoToLayer.Click += new System.EventHandler(this.cGoToToolStripMenuItem_Click);
			// 
			// c_miProperties
			// 
			this.c_miProperties.Image = global::Dapple.Properties.Resources.properties;
			this.c_miProperties.Name = "c_miProperties";
			this.c_miProperties.Size = new System.Drawing.Size(157, 22);
			this.c_miProperties.Text = "Properties...";
			this.c_miProperties.Click += new System.EventHandler(this.cViewPropertiesToolStripMenuItem_Click);
			// 
			// c_miViewLegend
			// 
			this.c_miViewLegend.Image = global::Dapple.Properties.Resources.legend;
			this.c_miViewLegend.Name = "c_miViewLegend";
			this.c_miViewLegend.Size = new System.Drawing.Size(157, 22);
			this.c_miViewLegend.Text = "View Legend...";
			this.c_miViewLegend.Click += new System.EventHandler(this.cViewLegendToolStripMenuItem_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(154, 6);
			// 
			// c_miRefreshLayer
			// 
			this.c_miRefreshLayer.Image = global::Dapple.Properties.Resources.refresh;
			this.c_miRefreshLayer.Name = "c_miRefreshLayer";
			this.c_miRefreshLayer.Size = new System.Drawing.Size(157, 22);
			this.c_miRefreshLayer.Text = "Refresh";
			this.c_miRefreshLayer.Click += new System.EventHandler(this.cRefreshToolStripMenuItem_Click);
			// 
			// c_miClearLayerChache
			// 
			this.c_miClearLayerChache.Image = global::Dapple.Properties.Resources.refresh_cache;
			this.c_miClearLayerChache.Name = "c_miClearLayerChache";
			this.c_miClearLayerChache.Size = new System.Drawing.Size(157, 22);
			this.c_miClearLayerChache.Text = "Clear Cache";
			this.c_miClearLayerChache.Click += new System.EventHandler(this.cClearCacheToolStripMenuItem_Click);
			// 
			// c_miRemoveLayer
			// 
			this.c_miRemoveLayer.Image = global::Dapple.Properties.Resources.layers_remove;
			this.c_miRemoveLayer.Name = "c_miRemoveLayer";
			this.c_miRemoveLayer.Size = new System.Drawing.Size(157, 22);
			this.c_miRemoveLayer.Text = "Remove";
			this.c_miRemoveLayer.Click += new System.EventHandler(this.cRemoveToolStripMenuItem_Click);
			// 
			// c_miAddOrGoToServer
			// 
			this.c_miAddOrGoToServer.Name = "c_miAddOrGoToServer";
			this.c_miAddOrGoToServer.Size = new System.Drawing.Size(157, 22);
			this.c_miAddOrGoToServer.Text = "Add/GoTo Server";
			this.c_miAddOrGoToServer.Click += new System.EventHandler(this.c_miAddOrGoToServer_Click);
			// 
			// c_tbTransparency
			// 
			this.c_tbTransparency.AutoSize = false;
			this.c_tbTransparency.LargeChange = 25;
			this.c_tbTransparency.Location = new System.Drawing.Point(0, 0);
			this.c_tbTransparency.Margin = new System.Windows.Forms.Padding(0);
			this.c_tbTransparency.Maximum = 255;
			this.c_tbTransparency.Name = "c_tbTransparency";
			this.c_tbTransparency.Size = new System.Drawing.Size(46, 25);
			this.c_tbTransparency.SmallChange = 5;
			this.c_tbTransparency.TabIndex = 0;
			this.c_tbTransparency.TickStyle = System.Windows.Forms.TickStyle.None;
			this.c_tbTransparency.Paint += new System.Windows.Forms.PaintEventHandler(this.cTransparencySlider_Paint);
			this.c_tbTransparency.ValueChanged += new System.EventHandler(this.cTransparencySlider_ValueChanged);
			// 
			// LayerList
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.c_tbTransparency);
			this.Controls.Add(c_tsControls);
			this.Controls.Add(this.c_lvLayers);
			this.Name = "LayerList";
			this.Size = new System.Drawing.Size(197, 121);
			this.Load += new System.EventHandler(this.LayerList_Load);
			c_tsControls.ResumeLayout(false);
			c_tsControls.PerformLayout();
			this.c_msContext.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.c_tbTransparency)).EndInit();
			this.ResumeLayout(false);

      }

      #endregion

      private System.Windows.Forms.ListView c_lvLayers;
      private System.Windows.Forms.ColumnHeader c_chLayers;
      private System.Windows.Forms.ToolStripButton c_bGoToLayer;
      private System.Windows.Forms.ToolStripButton c_RemoveLayer;
      private System.Windows.Forms.ToolStripButton c_bExtract;
      private TrackBarWithPaint c_tbTransparency;
      private System.Windows.Forms.ContextMenuStrip c_msContext;
      private System.Windows.Forms.ToolStripMenuItem c_miGoToLayer;
      private System.Windows.Forms.ToolStripMenuItem c_miProperties;
      private System.Windows.Forms.ToolStripMenuItem c_miRemoveLayer;
      private System.Windows.Forms.ToolStripMenuItem c_miRefreshLayer;
      private System.Windows.Forms.ToolStripMenuItem c_miClearLayerChache;
      private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
      private System.Windows.Forms.ToolStripButton c_Snapshot;
		private System.Windows.Forms.ToolStripMenuItem c_miViewLegend;
		private System.Windows.Forms.ToolStripMenuItem c_miAddOrGoToServer;
   }
}
