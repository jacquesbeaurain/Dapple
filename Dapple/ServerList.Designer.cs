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
         this.cServersComboBox = new System.Windows.Forms.ComboBox();
         this.cLayersListView = new System.Windows.Forms.ListView();
         this.cLayerNameColumnHeader = new System.Windows.Forms.ColumnHeader();
         this.cLayerContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
         this.addToLayersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.viewLegendToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.panel1 = new System.Windows.Forms.Panel();
         this.cPageNavigator = new Dapple.PageNavigator();
         label1 = new System.Windows.Forms.Label();
         this.cLayerContextMenu.SuspendLayout();
         this.panel1.SuspendLayout();
         this.SuspendLayout();
         // 
         // label1
         // 
         label1.AutoSize = true;
         label1.Location = new System.Drawing.Point(2, 6);
         label1.Name = "label1";
         label1.Size = new System.Drawing.Size(41, 13);
         label1.TabIndex = 0;
         label1.Text = "Server:";
         // 
         // cServersComboBox
         // 
         this.cServersComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                     | System.Windows.Forms.AnchorStyles.Right)));
         this.cServersComboBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
         this.cServersComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
         this.cServersComboBox.FormattingEnabled = true;
         this.cServersComboBox.Location = new System.Drawing.Point(49, 3);
         this.cServersComboBox.Name = "cServersComboBox";
         this.cServersComboBox.Size = new System.Drawing.Size(146, 21);
         this.cServersComboBox.TabIndex = 1;
         this.cServersComboBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.cServersComboBox_DrawItem);
         this.cServersComboBox.SelectedIndexChanged += new System.EventHandler(this.cServersComboBox_SelectedIndexChanged);
         // 
         // cLayersListView
         // 
         this.cLayersListView.Alignment = System.Windows.Forms.ListViewAlignment.Left;
         this.cLayersListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                     | System.Windows.Forms.AnchorStyles.Left)
                     | System.Windows.Forms.AnchorStyles.Right)));
         this.cLayersListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.cLayerNameColumnHeader});
         this.cLayersListView.ContextMenuStrip = this.cLayerContextMenu;
         this.cLayersListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
         this.cLayersListView.Location = new System.Drawing.Point(3, 30);
         this.cLayersListView.Name = "cLayersListView";
         this.cLayersListView.Size = new System.Drawing.Size(192, 136);
         this.cLayersListView.TabIndex = 2;
         this.cLayersListView.UseCompatibleStateImageBehavior = false;
         this.cLayersListView.View = System.Windows.Forms.View.Details;
         this.cLayersListView.DoubleClick += new System.EventHandler(this.cLayersListView_DoubleClick);
         this.cLayersListView.Resize += new System.EventHandler(this.cLayersListView_Resize);
         this.cLayersListView.SelectedIndexChanged += new System.EventHandler(this.cLayersListView_SelectedIndexChanged);
         this.cLayersListView.MouseMove += new System.Windows.Forms.MouseEventHandler(this.cLayersListView_MouseMove);
         this.cLayersListView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.cLayersListView_MouseDown);
         // 
         // cLayerNameColumnHeader
         // 
         this.cLayerNameColumnHeader.Text = "Layer";
         // 
         // cLayerContextMenu
         // 
         this.cLayerContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addToLayersToolStripMenuItem,
            this.viewLegendToolStripMenuItem});
         this.cLayerContextMenu.Name = "cLayerContextMenu";
         this.cLayerContextMenu.Size = new System.Drawing.Size(147, 48);
         this.cLayerContextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.cLayerContextMenu_Opening);
         // 
         // addToLayersToolStripMenuItem
         // 
         this.addToLayersToolStripMenuItem.Image = global::Dapple.Properties.Resources.layers_add;
         this.addToLayersToolStripMenuItem.Name = "addToLayersToolStripMenuItem";
         this.addToLayersToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
         this.addToLayersToolStripMenuItem.Text = "Add To Layers";
         this.addToLayersToolStripMenuItem.Click += new System.EventHandler(this.addToLayersToolStripMenuItem_Click);
         // 
         // viewLegendToolStripMenuItem
         // 
         this.viewLegendToolStripMenuItem.Image = global::Dapple.Properties.Resources.legend;
         this.viewLegendToolStripMenuItem.Name = "viewLegendToolStripMenuItem";
         this.viewLegendToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
         this.viewLegendToolStripMenuItem.Text = "View Legend...";
         this.viewLegendToolStripMenuItem.Click += new System.EventHandler(this.viewLegendToolStripMenuItem_Click);
         // 
         // panel1
         // 
         this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
         this.panel1.Controls.Add(this.cPageNavigator);
         this.panel1.Controls.Add(this.cLayersListView);
         this.panel1.Controls.Add(label1);
         this.panel1.Controls.Add(this.cServersComboBox);
         this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
         this.panel1.Location = new System.Drawing.Point(0, 0);
         this.panel1.Name = "panel1";
         this.panel1.Size = new System.Drawing.Size(200, 200);
         this.panel1.TabIndex = 4;
         // 
         // cPageNavigator
         // 
         this.cPageNavigator.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                     | System.Windows.Forms.AnchorStyles.Right)));
         this.cPageNavigator.Location = new System.Drawing.Point(3, 172);
         this.cPageNavigator.Name = "cPageNavigator";
         this.cPageNavigator.Size = new System.Drawing.Size(192, 23);
         this.cPageNavigator.TabIndex = 3;
         // 
         // ServerList
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.Controls.Add(this.panel1);
         this.Name = "ServerList";
         this.Size = new System.Drawing.Size(200, 200);
         this.Load += new System.EventHandler(this.ServerList_Load);
         this.cLayerContextMenu.ResumeLayout(false);
         this.panel1.ResumeLayout(false);
         this.panel1.PerformLayout();
         this.ResumeLayout(false);

      }

      #endregion

      private System.Windows.Forms.ComboBox cServersComboBox;
      private System.Windows.Forms.ListView cLayersListView;
      private System.Windows.Forms.ColumnHeader cLayerNameColumnHeader;
      private System.Windows.Forms.ContextMenuStrip cLayerContextMenu;
      private System.Windows.Forms.ToolStripMenuItem addToLayersToolStripMenuItem;
      private PageNavigator cPageNavigator;
      private System.Windows.Forms.Panel panel1;
      private System.Windows.Forms.ToolStripMenuItem viewLegendToolStripMenuItem;
   }
}
