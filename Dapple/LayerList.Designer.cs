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
         System.Windows.Forms.ToolStrip cToolStrip;
         this.cGoToButton = new System.Windows.Forms.ToolStripButton();
         this.cRemoveLayerButton = new System.Windows.Forms.ToolStripButton();
         this.cExtractButton = new System.Windows.Forms.ToolStripButton();
         this.cExportButton = new System.Windows.Forms.ToolStripButton();
         this.cLayerList = new System.Windows.Forms.ListView();
         this.cColumnHeader = new System.Windows.Forms.ColumnHeader();
         this.cLayerListContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
         this.cGoToToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.cViewPropertiesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
         this.cRefreshToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.cClearCacheToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.cRemoveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.cTransparencySlider = new Dapple.TrackBarWithPaint();
         cSliderMenuSeparator = new System.Windows.Forms.ToolStripSeparator();
         cToolStrip = new System.Windows.Forms.ToolStrip();
         cToolStrip.SuspendLayout();
         this.cLayerListContextMenu.SuspendLayout();
         ((System.ComponentModel.ISupportInitialize)(this.cTransparencySlider)).BeginInit();
         this.SuspendLayout();
         // 
         // cSliderMenuSeparator
         // 
         cSliderMenuSeparator.Name = "cSliderMenuSeparator";
         cSliderMenuSeparator.Size = new System.Drawing.Size(6, 25);
         // 
         // cToolStrip
         // 
         cToolStrip.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                     | System.Windows.Forms.AnchorStyles.Right)));
         cToolStrip.AutoSize = false;
         cToolStrip.Dock = System.Windows.Forms.DockStyle.None;
         cToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
         cToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            cSliderMenuSeparator,
            this.cGoToButton,
            this.cRemoveLayerButton,
            this.cExtractButton,
            this.cExportButton});
         cToolStrip.Location = new System.Drawing.Point(46, 0);
         cToolStrip.Name = "cToolStrip";
         cToolStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
         cToolStrip.Size = new System.Drawing.Size(151, 25);
         cToolStrip.TabIndex = 1;
         cToolStrip.TabStop = true;
         cToolStrip.Text = "toolStrip1";
         // 
         // cGoToButton
         // 
         this.cGoToButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
         this.cGoToButton.Image = global::Dapple.Properties.Resources.layers_goto;
         this.cGoToButton.ImageTransparentColor = System.Drawing.Color.Magenta;
         this.cGoToButton.Name = "cGoToButton";
         this.cGoToButton.Size = new System.Drawing.Size(23, 22);
         this.cGoToButton.Text = "toolStripButton1";
         this.cGoToButton.ToolTipText = "Go to layer";
         this.cGoToButton.Click += new System.EventHandler(this.cGoToButton_Click);
         // 
         // cRemoveLayerButton
         // 
         this.cRemoveLayerButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
         this.cRemoveLayerButton.Image = global::Dapple.Properties.Resources.layers_remove;
         this.cRemoveLayerButton.ImageTransparentColor = System.Drawing.Color.Magenta;
         this.cRemoveLayerButton.Name = "cRemoveLayerButton";
         this.cRemoveLayerButton.Size = new System.Drawing.Size(23, 22);
         this.cRemoveLayerButton.Text = "toolStripButton2";
         this.cRemoveLayerButton.ToolTipText = "Remove layer from visible layers";
         this.cRemoveLayerButton.Click += new System.EventHandler(this.cRemoveLayerButton_Click);
         // 
         // cExtractButton
         // 
         this.cExtractButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
         this.cExtractButton.Image = global::Dapple.Properties.Resources.layers_download;
         this.cExtractButton.ImageTransparentColor = System.Drawing.Color.Magenta;
         this.cExtractButton.Name = "cExtractButton";
         this.cExtractButton.Size = new System.Drawing.Size(23, 22);
         this.cExtractButton.Text = "toolStripButton3";
         this.cExtractButton.ToolTipText = "Download visible layers...";
         this.cExtractButton.Click += new System.EventHandler(this.cExtractButton_Click);
         // 
         // cExportButton
         // 
         this.cExportButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
         this.cExportButton.Image = global::Dapple.Properties.Resources.export;
         this.cExportButton.ImageTransparentColor = System.Drawing.Color.Magenta;
         this.cExportButton.Name = "cExportButton";
         this.cExportButton.Size = new System.Drawing.Size(23, 22);
         this.cExportButton.Text = "toolStripButton1";
         this.cExportButton.ToolTipText = "Export to GeoTiff";
         this.cExportButton.Click += new System.EventHandler(this.cExportButton_Click);
         // 
         // cLayerList
         // 
         this.cLayerList.AllowDrop = true;
         this.cLayerList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                     | System.Windows.Forms.AnchorStyles.Left)
                     | System.Windows.Forms.AnchorStyles.Right)));
         this.cLayerList.CheckBoxes = true;
         this.cLayerList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.cColumnHeader});
         this.cLayerList.ContextMenuStrip = this.cLayerListContextMenu;
         this.cLayerList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
         this.cLayerList.HideSelection = false;
         this.cLayerList.Location = new System.Drawing.Point(0, 25);
         this.cLayerList.Name = "cLayerList";
         this.cLayerList.Size = new System.Drawing.Size(197, 96);
         this.cLayerList.TabIndex = 2;
         this.cLayerList.UseCompatibleStateImageBehavior = false;
         this.cLayerList.View = System.Windows.Forms.View.Details;
         this.cLayerList.DragEnter += new System.Windows.Forms.DragEventHandler(this.cLayerList_DragEnter);
         this.cLayerList.DragDrop += new System.Windows.Forms.DragEventHandler(this.cLayerList_DragDrop);
         this.cLayerList.Resize += new System.EventHandler(this.cLayerList_Resize);
         this.cLayerList.DragOver += new System.Windows.Forms.DragEventHandler(this.cLayerList_DragOver);
         this.cLayerList.SelectedIndexChanged += new System.EventHandler(this.cLayerList_SelectedIndexChanged);
         this.cLayerList.DragLeave += new System.EventHandler(this.cLayerList_DragLeave);
         this.cLayerList.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.cLayerList_ItemCheck);
         this.cLayerList.MouseMove += new System.Windows.Forms.MouseEventHandler(this.cLayerList_MouseMove);
         this.cLayerList.KeyUp += new System.Windows.Forms.KeyEventHandler(this.cLayerList_KeyUp);
         // 
         // cLayerListContextMenu
         // 
         this.cLayerListContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cGoToToolStripMenuItem,
            this.cViewPropertiesToolStripMenuItem,
            this.toolStripSeparator1,
            this.cRefreshToolStripMenuItem,
            this.cClearCacheToolStripMenuItem,
            this.cRemoveToolStripMenuItem});
         this.cLayerListContextMenu.Name = "cLayerListContextMenu";
         this.cLayerListContextMenu.Size = new System.Drawing.Size(136, 120);
         this.cLayerListContextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.cLayerListContextMenu_Opening);
         // 
         // cGoToToolStripMenuItem
         // 
         this.cGoToToolStripMenuItem.Image = global::Dapple.Properties.Resources.layers_goto;
         this.cGoToToolStripMenuItem.Name = "cGoToToolStripMenuItem";
         this.cGoToToolStripMenuItem.Size = new System.Drawing.Size(135, 22);
         this.cGoToToolStripMenuItem.Text = "Go To";
         this.cGoToToolStripMenuItem.Click += new System.EventHandler(this.cGoToToolStripMenuItem_Click);
         // 
         // cViewPropertiesToolStripMenuItem
         // 
         this.cViewPropertiesToolStripMenuItem.Image = global::Dapple.Properties.Resources.properties;
         this.cViewPropertiesToolStripMenuItem.Name = "cViewPropertiesToolStripMenuItem";
         this.cViewPropertiesToolStripMenuItem.Size = new System.Drawing.Size(135, 22);
         this.cViewPropertiesToolStripMenuItem.Text = "Properties...";
         this.cViewPropertiesToolStripMenuItem.Click += new System.EventHandler(this.cViewPropertiesToolStripMenuItem_Click);
         // 
         // toolStripSeparator1
         // 
         this.toolStripSeparator1.Name = "toolStripSeparator1";
         this.toolStripSeparator1.Size = new System.Drawing.Size(132, 6);
         // 
         // cRefreshToolStripMenuItem
         // 
         this.cRefreshToolStripMenuItem.Image = global::Dapple.Properties.Resources.refresh;
         this.cRefreshToolStripMenuItem.Name = "cRefreshToolStripMenuItem";
         this.cRefreshToolStripMenuItem.Size = new System.Drawing.Size(135, 22);
         this.cRefreshToolStripMenuItem.Text = "Refresh";
         this.cRefreshToolStripMenuItem.Click += new System.EventHandler(this.cRefreshToolStripMenuItem_Click);
         // 
         // cClearCacheToolStripMenuItem
         // 
         this.cClearCacheToolStripMenuItem.Image = global::Dapple.Properties.Resources.refresh_cache;
         this.cClearCacheToolStripMenuItem.Name = "cClearCacheToolStripMenuItem";
         this.cClearCacheToolStripMenuItem.Size = new System.Drawing.Size(135, 22);
         this.cClearCacheToolStripMenuItem.Text = "Clear Cache";
         this.cClearCacheToolStripMenuItem.Click += new System.EventHandler(this.cClearCacheToolStripMenuItem_Click);
         // 
         // cRemoveToolStripMenuItem
         // 
         this.cRemoveToolStripMenuItem.Image = global::Dapple.Properties.Resources.layers_remove;
         this.cRemoveToolStripMenuItem.Name = "cRemoveToolStripMenuItem";
         this.cRemoveToolStripMenuItem.Size = new System.Drawing.Size(135, 22);
         this.cRemoveToolStripMenuItem.Text = "Remove";
         this.cRemoveToolStripMenuItem.Click += new System.EventHandler(this.cRemoveToolStripMenuItem_Click);
         // 
         // cTransparencySlider
         // 
         this.cTransparencySlider.AutoSize = false;
         this.cTransparencySlider.LargeChange = 25;
         this.cTransparencySlider.Location = new System.Drawing.Point(0, 0);
         this.cTransparencySlider.Margin = new System.Windows.Forms.Padding(0);
         this.cTransparencySlider.Maximum = 255;
         this.cTransparencySlider.Name = "cTransparencySlider";
         this.cTransparencySlider.Size = new System.Drawing.Size(46, 25);
         this.cTransparencySlider.SmallChange = 5;
         this.cTransparencySlider.TabIndex = 0;
         this.cTransparencySlider.TickStyle = System.Windows.Forms.TickStyle.None;
         this.cTransparencySlider.ValueChanged += new System.EventHandler(this.cTransparencySlider_ValueChanged);
         this.cTransparencySlider.Paint += new System.Windows.Forms.PaintEventHandler(this.cTransparencySlider_Paint);
         // 
         // LayerList
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.Controls.Add(this.cTransparencySlider);
         this.Controls.Add(cToolStrip);
         this.Controls.Add(this.cLayerList);
         this.Name = "LayerList";
         this.Size = new System.Drawing.Size(197, 121);
         this.Load += new System.EventHandler(this.LayerList_Load);
         cToolStrip.ResumeLayout(false);
         cToolStrip.PerformLayout();
         this.cLayerListContextMenu.ResumeLayout(false);
         ((System.ComponentModel.ISupportInitialize)(this.cTransparencySlider)).EndInit();
         this.ResumeLayout(false);

      }

      #endregion

      private System.Windows.Forms.ListView cLayerList;
      private System.Windows.Forms.ColumnHeader cColumnHeader;
      private System.Windows.Forms.ToolStripButton cGoToButton;
      private System.Windows.Forms.ToolStripButton cRemoveLayerButton;
      private System.Windows.Forms.ToolStripButton cExtractButton;
      private TrackBarWithPaint cTransparencySlider;
      private System.Windows.Forms.ContextMenuStrip cLayerListContextMenu;
      private System.Windows.Forms.ToolStripMenuItem cGoToToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem cViewPropertiesToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem cRemoveToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem cRefreshToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem cClearCacheToolStripMenuItem;
      private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
      private System.Windows.Forms.ToolStripButton cExportButton;
   }
}
