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
         System.Windows.Forms.SplitContainer splitContainer1;
         System.Windows.Forms.Label label1;
         this.cServersComboBox = new System.Windows.Forms.ComboBox();
         this.cLayersListView = new System.Windows.Forms.ListView();
         this.cLayerNameColumnHeader = new System.Windows.Forms.ColumnHeader();
         this.cLayerContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
         this.addToLayersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.cPrevButton = new System.Windows.Forms.Button();
         this.cNextButton = new System.Windows.Forms.Button();
         this.cPageLabel = new System.Windows.Forms.Label();
         splitContainer1 = new System.Windows.Forms.SplitContainer();
         label1 = new System.Windows.Forms.Label();
         splitContainer1.Panel1.SuspendLayout();
         splitContainer1.Panel2.SuspendLayout();
         splitContainer1.SuspendLayout();
         this.cLayerContextMenu.SuspendLayout();
         this.SuspendLayout();
         // 
         // splitContainer1
         // 
         splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
         splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
         splitContainer1.IsSplitterFixed = true;
         splitContainer1.Location = new System.Drawing.Point(0, 0);
         splitContainer1.Name = "splitContainer1";
         splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
         // 
         // splitContainer1.Panel1
         // 
         splitContainer1.Panel1.Controls.Add(this.cServersComboBox);
         splitContainer1.Panel1.Controls.Add(label1);
         // 
         // splitContainer1.Panel2
         // 
         splitContainer1.Panel2.Controls.Add(this.cLayersListView);
         splitContainer1.Panel2.Controls.Add(this.cPrevButton);
         splitContainer1.Panel2.Controls.Add(this.cNextButton);
         splitContainer1.Panel2.Controls.Add(this.cPageLabel);
         splitContainer1.Size = new System.Drawing.Size(150, 150);
         splitContainer1.SplitterDistance = 27;
         splitContainer1.TabIndex = 0;
         // 
         // cServersComboBox
         // 
         this.cServersComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                     | System.Windows.Forms.AnchorStyles.Right)));
         this.cServersComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
         this.cServersComboBox.FormattingEnabled = true;
         this.cServersComboBox.Location = new System.Drawing.Point(50, 3);
         this.cServersComboBox.Name = "cServersComboBox";
         this.cServersComboBox.Size = new System.Drawing.Size(100, 21);
         this.cServersComboBox.TabIndex = 1;
         this.cServersComboBox.SelectedIndexChanged += new System.EventHandler(this.cServersComboBox_SelectedIndexChanged);
         // 
         // label1
         // 
         label1.AutoSize = true;
         label1.Location = new System.Drawing.Point(3, 6);
         label1.Name = "label1";
         label1.Size = new System.Drawing.Size(41, 13);
         label1.TabIndex = 0;
         label1.Text = "Server:";
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
         this.cLayersListView.Location = new System.Drawing.Point(0, 3);
         this.cLayersListView.Name = "cLayersListView";
         this.cLayersListView.Size = new System.Drawing.Size(150, 84);
         this.cLayersListView.TabIndex = 4;
         this.cLayersListView.UseCompatibleStateImageBehavior = false;
         this.cLayersListView.View = System.Windows.Forms.View.Details;
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
            this.addToLayersToolStripMenuItem});
         this.cLayerContextMenu.Name = "cLayerContextMenu";
         this.cLayerContextMenu.Size = new System.Drawing.Size(144, 26);
         this.cLayerContextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.cLayerContextMenu_Opening);
         // 
         // addToLayersToolStripMenuItem
         // 
         this.addToLayersToolStripMenuItem.Image = global::Dapple.Properties.Resources.layers_add;
         this.addToLayersToolStripMenuItem.Name = "addToLayersToolStripMenuItem";
         this.addToLayersToolStripMenuItem.Size = new System.Drawing.Size(143, 22);
         this.addToLayersToolStripMenuItem.Text = "Add To Layers";
         this.addToLayersToolStripMenuItem.Click += new System.EventHandler(this.addToLayersToolStripMenuItem_Click);
         // 
         // cPrevButton
         // 
         this.cPrevButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
         this.cPrevButton.Location = new System.Drawing.Point(3, 93);
         this.cPrevButton.Name = "cPrevButton";
         this.cPrevButton.Size = new System.Drawing.Size(24, 23);
         this.cPrevButton.TabIndex = 1;
         this.cPrevButton.Text = "<-";
         this.cPrevButton.UseVisualStyleBackColor = true;
         this.cPrevButton.Click += new System.EventHandler(this.cPrevButton_Click);
         // 
         // cNextButton
         // 
         this.cNextButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
         this.cNextButton.Location = new System.Drawing.Point(123, 93);
         this.cNextButton.Name = "cNextButton";
         this.cNextButton.Size = new System.Drawing.Size(24, 23);
         this.cNextButton.TabIndex = 2;
         this.cNextButton.Text = "->";
         this.cNextButton.UseVisualStyleBackColor = true;
         this.cNextButton.Click += new System.EventHandler(this.cNextButton_Click);
         // 
         // cPageLabel
         // 
         this.cPageLabel.Dock = System.Windows.Forms.DockStyle.Bottom;
         this.cPageLabel.Location = new System.Drawing.Point(0, 93);
         this.cPageLabel.Name = "cPageLabel";
         this.cPageLabel.Size = new System.Drawing.Size(150, 26);
         this.cPageLabel.TabIndex = 3;
         this.cPageLabel.Text = "Page X of X+n";
         this.cPageLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
         // 
         // ServerList
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.Controls.Add(splitContainer1);
         this.Name = "ServerList";
         this.Load += new System.EventHandler(this.ServerList_Load);
         splitContainer1.Panel1.ResumeLayout(false);
         splitContainer1.Panel1.PerformLayout();
         splitContainer1.Panel2.ResumeLayout(false);
         splitContainer1.ResumeLayout(false);
         this.cLayerContextMenu.ResumeLayout(false);
         this.ResumeLayout(false);

      }

      #endregion

      private System.Windows.Forms.ComboBox cServersComboBox;
      private System.Windows.Forms.Button cNextButton;
      private System.Windows.Forms.Button cPrevButton;
      private System.Windows.Forms.Label cPageLabel;
      private System.Windows.Forms.ListView cLayersListView;
      private System.Windows.Forms.ColumnHeader cLayerNameColumnHeader;
      private System.Windows.Forms.ContextMenuStrip cLayerContextMenu;
      private System.Windows.Forms.ToolStripMenuItem addToLayersToolStripMenuItem;
   }
}
