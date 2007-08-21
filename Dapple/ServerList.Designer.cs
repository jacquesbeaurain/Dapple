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
         this.splitContainer1 = new System.Windows.Forms.SplitContainer();
         this.cServersComboBox = new System.Windows.Forms.ComboBox();
         this.label1 = new System.Windows.Forms.Label();
         this.cNextButton = new System.Windows.Forms.Button();
         this.cPrevButton = new System.Windows.Forms.Button();
         this.cLayersListBox = new System.Windows.Forms.ListBox();
         this.splitContainer1.Panel1.SuspendLayout();
         this.splitContainer1.Panel2.SuspendLayout();
         this.splitContainer1.SuspendLayout();
         this.SuspendLayout();
         // 
         // splitContainer1
         // 
         this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
         this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
         this.splitContainer1.IsSplitterFixed = true;
         this.splitContainer1.Location = new System.Drawing.Point(0, 0);
         this.splitContainer1.Name = "splitContainer1";
         this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
         // 
         // splitContainer1.Panel1
         // 
         this.splitContainer1.Panel1.Controls.Add(this.cServersComboBox);
         this.splitContainer1.Panel1.Controls.Add(this.label1);
         // 
         // splitContainer1.Panel2
         // 
         this.splitContainer1.Panel2.Controls.Add(this.cNextButton);
         this.splitContainer1.Panel2.Controls.Add(this.cPrevButton);
         this.splitContainer1.Panel2.Controls.Add(this.cLayersListBox);
         this.splitContainer1.Size = new System.Drawing.Size(150, 150);
         this.splitContainer1.SplitterDistance = 27;
         this.splitContainer1.TabIndex = 0;
         // 
         // cServersComboBox
         // 
         this.cServersComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                     | System.Windows.Forms.AnchorStyles.Right)));
         this.cServersComboBox.FormattingEnabled = true;
         this.cServersComboBox.Location = new System.Drawing.Point(50, 3);
         this.cServersComboBox.Name = "cServersComboBox";
         this.cServersComboBox.Size = new System.Drawing.Size(100, 21);
         this.cServersComboBox.TabIndex = 1;
         // 
         // label1
         // 
         this.label1.AutoSize = true;
         this.label1.Location = new System.Drawing.Point(3, 6);
         this.label1.Name = "label1";
         this.label1.Size = new System.Drawing.Size(41, 13);
         this.label1.TabIndex = 0;
         this.label1.Text = "Server:";
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
         // 
         // cLayersListBox
         // 
         this.cLayersListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                     | System.Windows.Forms.AnchorStyles.Left)
                     | System.Windows.Forms.AnchorStyles.Right)));
         this.cLayersListBox.FormattingEnabled = true;
         this.cLayersListBox.Location = new System.Drawing.Point(3, 3);
         this.cLayersListBox.Name = "cLayersListBox";
         this.cLayersListBox.Size = new System.Drawing.Size(144, 82);
         this.cLayersListBox.TabIndex = 0;
         // 
         // ServerList
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.Controls.Add(this.splitContainer1);
         this.Name = "ServerList";
         this.splitContainer1.Panel1.ResumeLayout(false);
         this.splitContainer1.Panel1.PerformLayout();
         this.splitContainer1.Panel2.ResumeLayout(false);
         this.splitContainer1.ResumeLayout(false);
         this.ResumeLayout(false);

      }

      #endregion

      private System.Windows.Forms.SplitContainer splitContainer1;
      private System.Windows.Forms.ComboBox cServersComboBox;
      private System.Windows.Forms.Label label1;
      private System.Windows.Forms.Button cNextButton;
      private System.Windows.Forms.Button cPrevButton;
      private System.Windows.Forms.ListBox cLayersListBox;
   }
}
