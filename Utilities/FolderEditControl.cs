using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using Geosoft;

namespace Geosoft.OpenGX.UtilityForms
{
   /// <summary>
   /// Reusable Folder Browsing Edit control for use in Geosoft GX.Net code.
   /// </summary>
   [ToolboxItem(true)]
   //[DesignerAttribute(typeof(EditControlDesigner))]
   [ToolboxBitmap(typeof(FolderEditControl), "FolderEditControl.bmp")]
   public class FolderEditControl : Control, IRequirable
   {
      #region Member Variables
      private string m_strTitle = "";
      #endregion

      #region Forms Variables
      private System.Windows.Forms.TextBox m_tbFolderName;
      private Button btnBrowse;
      private FolderBrowserDialog m_folderBrowserDialog;
		private bool m_blRequired = false;

		public event EventHandler RequiredChanged;
      #endregion

      #region Construction/Destruction
      /// <summary> 
      /// Default constructor.
      /// </summary>
      public FolderEditControl()
      {
         // This call is required by the Windows.Forms Form Designer.
         InitializeComponent();
      }

      /// <summary> 
      /// Clean up any resources being used.
      /// </summary>
      protected override void Dispose(bool disposing)
      {
         if (disposing)
         {
         }
         base.Dispose(disposing);
      }
      #endregion

      #region Component Designer generated code
      /// <summary> 
      /// Required method for Designer support - do not modify 
      /// the contents of this method with the code editor.
      /// </summary>
      private void InitializeComponent()
      {
			this.m_tbFolderName = new System.Windows.Forms.TextBox();
			this.btnBrowse = new System.Windows.Forms.Button();
			this.m_folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
			this.SuspendLayout();
			// 
			// m_tbFolderName
			// 
			this.m_tbFolderName.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
							| System.Windows.Forms.AnchorStyles.Left)
							| System.Windows.Forms.AnchorStyles.Right)));
			this.m_tbFolderName.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Append;
			this.m_tbFolderName.Location = new System.Drawing.Point(0, 0);
			this.m_tbFolderName.Margin = new System.Windows.Forms.Padding(0);
			this.m_tbFolderName.MinimumSize = new System.Drawing.Size(50, 21);
			this.m_tbFolderName.Name = "m_tbFolderName";
			this.m_tbFolderName.Size = new System.Drawing.Size(155, 21);
			this.m_tbFolderName.TabIndex = 0;
			// 
			// btnBrowse
			// 
			this.btnBrowse.BackColor = System.Drawing.SystemColors.Control;
			this.btnBrowse.Dock = System.Windows.Forms.DockStyle.Right;
			this.btnBrowse.Font = new System.Drawing.Font("Tahoma", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnBrowse.Location = new System.Drawing.Point(159, 0);
			this.btnBrowse.Margin = new System.Windows.Forms.Padding(2, 10, 2, 5);
			this.btnBrowse.MaximumSize = new System.Drawing.Size(30, 21);
			this.btnBrowse.MinimumSize = new System.Drawing.Size(21, 21);
			this.btnBrowse.Name = "btnBrowse";
			this.btnBrowse.Size = new System.Drawing.Size(21, 21);
			this.btnBrowse.TabIndex = 2;
			this.btnBrowse.Text = "...";
			this.btnBrowse.UseCompatibleTextRendering = true;
			this.btnBrowse.UseVisualStyleBackColor = true;
			this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
			// 
			// FolderEditControl
			// 
			this.Controls.Add(this.btnBrowse);
			this.Controls.Add(this.m_tbFolderName);
			this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Size = new System.Drawing.Size(180, 21);
			this.ResumeLayout(false);
			this.PerformLayout();

      }
      #endregion

      #region Properties Code
      /// <summary>
      /// Gets or sets the value for the directory edit control's path.
      /// </summary>
      [Browsable(true), Category("GX_Parameters")]
      [Description("Sets the value")]
      public String Value
      {
         get
         {
            return m_tbFolderName.Text;
         }
         set
         {
            m_tbFolderName.Text = value;
         }
      }
      /// <summary>
      /// Gets or sets the title to be dispalayed above the folder tree.
      /// </summary>
      [Browsable(true), Category("GX_Parameters")]
      [Description("The title of the browse dialog launched using the browse button.")]
      public String BrowseQuery
      {
         get
         {
            return m_strTitle;
         }
         set
         {
            m_strTitle = value;
         }
      }

		[Browsable(true), Category("GX_Parameters")]
		[Description("Whether this field is required.")]
		public bool Required
		{
			get
			{
				return m_blRequired;
			}
			set
			{
				m_blRequired = value;
				if (RequiredChanged != null)
					RequiredChanged(this, new EventArgs());
			}
		}
      #endregion

      #region Methods
      /// <summary>
      /// Check to see if the current folder path passes validation tests.
      /// </summary>
      /// <remarks></remarks>
      /// <param name="strError">The returned error string if the control does not validate.</param>
      /// <returns>true if validation succeeds</returns>
      public bool bIsValid(ref string strError)
      {
         string strPath = "";
         return (bValidate(ref strPath, ref strError, true));
      }
      #endregion

      #region Private Methods
      private bool bValidate(ref string strPath, ref string strError, bool bGetCreatePermission)
      {
         string strLocalPath = m_tbFolderName.Text;

         if (strLocalPath == String.Empty)
            strLocalPath = "." + Path.DirectorySeparatorChar;

         if (!Directory.Exists(strLocalPath))
         {
            if (bGetCreatePermission)
            {
               // --- Ask the user for overwrite permission ---

               string strMessage = "Directory \"" + strLocalPath + "\" does not exist. Do you want to create it?";
               if (DialogResult.Yes == MessageBox.Show(strMessage, "Directory does not exist", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Question))
               {
                  try
                  {
                     Directory.CreateDirectory(strLocalPath);
                     return true;
                  }
                  catch (Exception e)
                  {
                     strError = e.Message;
                     return false;
                  }
               }
            }
            strError = "Directory \"" + strLocalPath + "\" does not exist.";
            return false;
         }
         else
         {
            strError = "";
            strPath = strLocalPath;
            return true;
         }
      }
      #endregion

      #region Event Handlers
      private void btnBrowse_Click(object sender, EventArgs e)
      {
			m_folderBrowserDialog.Description = BrowseQuery;
			m_folderBrowserDialog.RootFolder = Environment.SpecialFolder.Desktop;
			m_folderBrowserDialog.SelectedPath = Value;
			m_folderBrowserDialog.ShowNewFolderButton = true;
         /*string strReturn = "";
         if (PathUI.bDirectoryOpen(ParentForm, m_strTitle, m_tbFolderName.Text, ref strReturn))
            m_tbFolderName.Text = strReturn;*/
			if (m_folderBrowserDialog.ShowDialog() == DialogResult.OK)
				m_tbFolderName.Text = m_folderBrowserDialog.SelectedPath;
      }
      #endregion
   }

}
