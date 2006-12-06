namespace ConfigurationWizard
{
   partial class DappleSearchPage
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
         System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DappleSearchPage));
         this.DappleSearchURLTextbox = new System.Windows.Forms.TextBox();
         this.label1 = new System.Windows.Forms.Label();
         this.label2 = new System.Windows.Forms.Label();
         this.UseDappleSearchCheckBox = new System.Windows.Forms.CheckBox();
         this.SuspendLayout();
         // 
         // DappleSearchURLTextbox
         // 
         this.DappleSearchURLTextbox.Location = new System.Drawing.Point(160, 148);
         this.DappleSearchURLTextbox.Name = "DappleSearchURLTextbox";
         this.DappleSearchURLTextbox.Size = new System.Drawing.Size(347, 20);
         this.DappleSearchURLTextbox.TabIndex = 1;
         // 
         // label1
         // 
         this.label1.AutoSize = true;
         this.label1.Location = new System.Drawing.Point(17, 63);
         this.label1.Name = "label1";
         this.label1.Size = new System.Drawing.Size(436, 52);
         this.label1.TabIndex = 2;
         this.label1.Text = resources.GetString("label1.Text");
         // 
         // label2
         // 
         this.label2.AutoSize = true;
         this.label2.Location = new System.Drawing.Point(17, 151);
         this.label2.Name = "label2";
         this.label2.Size = new System.Drawing.Size(137, 13);
         this.label2.TabIndex = 3;
         this.label2.Text = "DappleSearch Server URL:";
         // 
         // UseDappleSearchCheckBox
         // 
         this.UseDappleSearchCheckBox.AutoSize = true;
         this.UseDappleSearchCheckBox.Location = new System.Drawing.Point(20, 131);
         this.UseDappleSearchCheckBox.Name = "UseDappleSearchCheckBox";
         this.UseDappleSearchCheckBox.Size = new System.Drawing.Size(116, 17);
         this.UseDappleSearchCheckBox.TabIndex = 4;
         this.UseDappleSearchCheckBox.Text = "Use DappleSearch";
         this.UseDappleSearchCheckBox.UseVisualStyleBackColor = true;
         this.UseDappleSearchCheckBox.CheckedChanged += new System.EventHandler(this.UseDappleSearchCheckBox_CheckedChanged);
         // 
         // DappleSearchPage
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.Controls.Add(this.UseDappleSearchCheckBox);
         this.Controls.Add(this.label2);
         this.Controls.Add(this.label1);
         this.Controls.Add(this.DappleSearchURLTextbox);
         this.Name = "DappleSearchPage";
         this.SubTitle = "Adjust DappleSearch settings";
         this.Title = "DappleSearch";
         this.Controls.SetChildIndex(this.DappleSearchURLTextbox, 0);
         this.Controls.SetChildIndex(this.label1, 0);
         this.Controls.SetChildIndex(this.label2, 0);
         this.Controls.SetChildIndex(this.UseDappleSearchCheckBox, 0);
         this.ResumeLayout(false);
         this.PerformLayout();

      }

      #endregion

      private System.Windows.Forms.TextBox DappleSearchURLTextbox;
      private System.Windows.Forms.Label label1;
      private System.Windows.Forms.Label label2;
      private System.Windows.Forms.CheckBox UseDappleSearchCheckBox;
   }
}
