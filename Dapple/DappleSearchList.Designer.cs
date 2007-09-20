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
         this.cResultListBox = new System.Windows.Forms.ListBox();
         this.cNavigator = new Dapple.PageNavigator();
         this.cTabToolbar = new Dapple.CustomControls.TabToolStrip();
         this.SuspendLayout();
         // 
         // cResultListBox
         // 
         this.cResultListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                     | System.Windows.Forms.AnchorStyles.Left)
                     | System.Windows.Forms.AnchorStyles.Right)));
         this.cResultListBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
         this.cResultListBox.FormattingEnabled = true;
         this.cResultListBox.IntegralHeight = false;
         this.cResultListBox.Location = new System.Drawing.Point(0, 0);
         this.cResultListBox.Margin = new System.Windows.Forms.Padding(0);
         this.cResultListBox.Name = "cResultListBox";
         this.cResultListBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
         this.cResultListBox.Size = new System.Drawing.Size(200, 151);
         this.cResultListBox.TabIndex = 0;
         this.cResultListBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.cResultListBox_DrawItem);
         this.cResultListBox.MeasureItem += new System.Windows.Forms.MeasureItemEventHandler(this.cResultListBox_MeasureItem);
         this.cResultListBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.cResultListBox_MouseMove);
         this.cResultListBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.cResultListBox_MouseDown);
         // 
         // cNavigator
         // 
         this.cNavigator.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                     | System.Windows.Forms.AnchorStyles.Right)));
         this.cNavigator.Location = new System.Drawing.Point(0, 151);
         this.cNavigator.Margin = new System.Windows.Forms.Padding(0);
         this.cNavigator.Name = "cNavigator";
         this.cNavigator.Size = new System.Drawing.Size(200, 24);
         this.cNavigator.TabIndex = 1;
         // 
         // cTabToolbar
         // 
         this.cTabToolbar.Dock = System.Windows.Forms.DockStyle.Bottom;
         this.cTabToolbar.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
         this.cTabToolbar.Location = new System.Drawing.Point(0, 175);
         this.cTabToolbar.Name = "cTabToolbar";
         this.cTabToolbar.Size = new System.Drawing.Size(200, 25);
         this.cTabToolbar.TabIndex = 2;
         this.cTabToolbar.Text = "tabToolbar1";
         // 
         // DappleSearchList
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.Controls.Add(this.cTabToolbar);
         this.Controls.Add(this.cNavigator);
         this.Controls.Add(this.cResultListBox);
         this.Name = "DappleSearchList";
         this.Size = new System.Drawing.Size(200, 200);
         this.ResumeLayout(false);
         this.PerformLayout();

      }

      #endregion

      private System.Windows.Forms.ListBox cResultListBox;
      private PageNavigator cNavigator;
      private TabToolStrip cTabToolbar;
   }
}
