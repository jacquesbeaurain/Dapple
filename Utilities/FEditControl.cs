using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Geosoft.OpenGX.UtilityForms
{
   // This defines the possible values for the FEditControl's FileOpenSaveEnum property.
	public enum FileOpenSaveEnum
   {
      Open = 0,
      Save = 1
   }

   /// <summary>
   /// Summary description for FEditControl.
   /// </summary>
   [Serializable]
	public class FEditControl : System.Windows.Forms.UserControl, ISerializable, IRequirable
   {
      #region Member Variables
      protected string m_strDirectory = "";
      protected string m_strTitle = "Choose File";
      protected FileOpenSaveEnum m_eOpenSave = FileOpenSaveEnum.Open;
      protected string m_strOverwriteFile = "";
      protected bool m_bRequired = true;
      protected bool m_bShowExtension = true; 
      protected bool m_bSaveHistory = true;
      protected int m_iMaxHistory = 0;
      protected List<string> m_hLST = null;
      #endregion
      private IContainer components;
		public delegate void FEditTextChangedEventHandler(object sender);
		public event FEditTextChangedEventHandler FEditTextChanged;
		/// <summary>
		/// Fired when the <see cref="Required"/> property changes
		/// </summary>
		public event EventHandler RequiredChanged;

      #region Forms Variables

      private System.Windows.Forms.TextBox m_tbFileName;
      private System.Windows.Forms.Button btnBrowse;
      private System.Windows.Forms.SaveFileDialog m_saveFileDialog1;
      private System.Windows.Forms.OpenFileDialog m_openFileDialog1;
      private System.Windows.Forms.ComboBox m_cmbFileName;
      private System.Windows.Forms.ErrorProvider m_errorProvider;
      #endregion

      #region Construction/Serialization
      /// <summary> 
      /// Default constructor.
      /// </summary>
		public FEditControl()
      {
         // This call is required by the Windows.Forms Form Designer.
         InitializeComponent();

         Filters = "All files (*.*)|*.*";
         m_strDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
      }

		public FEditControl(SerializationInfo info, StreamingContext context)
			: this()
      {
         if (info == null)
            throw new System.ArgumentNullException("info");

         m_strDirectory = (string)info.GetValue("Directory", typeof(string));
         if (!Directory.Exists(m_strDirectory))
            m_strDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
         this.Filters = (string)info.GetValue("Filters", typeof(string));
         this.FilterIndex = (int)info.GetValue("FilterIndex", typeof(int));
         this.BrowseQuery = (string)info.GetValue("Title", typeof(string));
         this.FileOpenSave = (FileOpenSaveEnum)info.GetValue("FileOpenSave", typeof(FileOpenSaveEnum));
         this.Required = (bool)info.GetValue("Required", typeof(bool));

         ClearHistory();
         try
         {
            this.MaxHistory = (int)info.GetValue("MaxHistory", typeof(int));
         }
         catch
         {
            // The following to support previous versions which used SaveHistory
            if ((bool)info.GetValue("SaveHistory", typeof(bool)))
               this.MaxHistory = 10;
            else
               this.MaxHistory = 0;
         }
         if (this.MaxHistory > 0)
         {
            int iHistory = (int)info.GetValue("History", typeof(int));
				for (int i = 0; i < iHistory; i++)
				{
					String szHistory = (string)info.GetValue("History_" + i.ToString(), typeof(string));
					if (File.Exists(szHistory))
						m_hLST.Add(szHistory);
				}
         }

			String szLastFilename = (string)info.GetValue("File", typeof(string));
			if (!File.Exists(Path.Combine(m_strDirectory, szLastFilename)))
			{
				if (m_hLST.Count > 0)
				{
					szLastFilename = Path.GetFileName(m_hLST[0]);
					m_strDirectory = Path.GetDirectoryName(m_hLST[0]);
					m_hLST.RemoveAt(0);
				}
				else
					szLastFilename = String.Empty;
			}

         if (m_hLST != null && m_hLST.Count > 0)
         {
            FillHistory();
            m_cmbFileName.Text = szLastFilename;
         }
         else
            m_tbFileName.Text = szLastFilename;
      }

      [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public virtual void GetObjectData(
      SerializationInfo info, StreamingContext context)
      {
         if (info == null)
            throw new System.ArgumentNullException("info");

         info.AddValue("Directory", m_strDirectory);
         info.AddValue("Filters", this.Filters);
         info.AddValue("FilterIndex", this.FilterIndex);
         info.AddValue("Title", this.BrowseQuery);
         info.AddValue("FileOpenSave", this.FileOpenSave);
         info.AddValue("Required", this.Required);
         info.AddValue("MaxHistory", this.MaxHistory);
         
         string strFile;
         if (m_hLST != null && m_hLST.Count > 0)
            strFile = m_cmbFileName.Text;
         else
            strFile = m_tbFileName.Text;

         // --- Add the default extension for the first file filter ---
         if (!Path.HasExtension(strFile) && strFile.Substring(strFile.Length - 1, 1) != ".")
         {
            string strExt = "";
            int iIndex = Filters.IndexOf("|*.");
            if (iIndex != -1)
            {
               char[] caAny = new char[] { '|', ';' };
               strExt = this.Filters.Substring(iIndex + 3);
               iIndex = strExt.IndexOfAny(caAny);
               if (iIndex != -1)
                  strExt = strExt.Substring(0, iIndex);
               if (strExt.IndexOf('*') == -1)
                  strExt = "." + strExt;
            }
            strFile += strExt;
         }
         info.AddValue("File", strFile);
         if (m_hLST != null && m_hLST.Count > 0)
         {
            for (int i = 0; i < m_hLST.Count; i++)
            {
               if (m_hLST[i].Equals(Path.Combine(m_strDirectory, strFile), StringComparison.InvariantCultureIgnoreCase))
               {
                  m_hLST.RemoveAt(i);
                  break;
               }
            }
         } else
            m_hLST = new List<string>();
         
         m_hLST.Insert(0, Path.Combine(m_strDirectory, strFile));
         info.AddValue("History", m_hLST.Count);
         for (int i = 0; i < Math.Min(m_iMaxHistory, m_hLST.Count); i++)
            info.AddValue("History_" + i.ToString(), m_hLST[i]);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="strFile"></param>
		public void Serialize(string strFile)
      {
         BinaryFormatter binaryFmt = new BinaryFormatter();
         using (FileStream fs = new FileStream(strFile, FileMode.Create))
         {
            binaryFmt.Serialize(fs, this);
            fs.Close();
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="strFile"></param>
      /// <param name="fEdit"></param>
		public static FEditControl DeSerialize(string strFile)
      {
         BinaryFormatter binaryFmt = new BinaryFormatter();
         FEditControl fEdit;
         using (FileStream fs = new FileStream(strFile, FileMode.Open))
         {
            fEdit = (FEditControl)binaryFmt.Deserialize(fs);
            fs.Close();
         }
         return fEdit;
      }

      /// <summary> 
      /// Clean up any resources being used.
      /// </summary>
      protected override void Dispose(bool disposing)
      {
         if (disposing)
         {
            if (components != null)
            {
               components.Dispose();
            }
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
			this.components = new System.ComponentModel.Container();
			this.m_tbFileName = new System.Windows.Forms.TextBox();
			this.m_cmbFileName = new System.Windows.Forms.ComboBox();
			this.btnBrowse = new System.Windows.Forms.Button();
			this.m_saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
			this.m_openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
			this.m_errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
			((System.ComponentModel.ISupportInitialize)(this.m_errorProvider)).BeginInit();
			this.SuspendLayout();
			// 
			// m_tbFileName
			// 
			this.m_tbFileName.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
							| System.Windows.Forms.AnchorStyles.Left)
							| System.Windows.Forms.AnchorStyles.Right)));
			this.m_tbFileName.Location = new System.Drawing.Point(0, 0);
			this.m_tbFileName.MinimumSize = new System.Drawing.Size(50, 21);
			this.m_tbFileName.Name = "m_tbFileName";
			this.m_tbFileName.Size = new System.Drawing.Size(156, 21);
			this.m_tbFileName.TabIndex = 0;
			this.m_tbFileName.TextChanged += new System.EventHandler(this.m_tbFileName_TextChanged);
			// 
			// m_cmbFileName
			// 
			this.m_cmbFileName.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
							| System.Windows.Forms.AnchorStyles.Left)
							| System.Windows.Forms.AnchorStyles.Right)));
			this.m_cmbFileName.FormattingEnabled = true;
			this.m_cmbFileName.Location = new System.Drawing.Point(0, 0);
			this.m_cmbFileName.Margin = new System.Windows.Forms.Padding(0);
			this.m_cmbFileName.MinimumSize = new System.Drawing.Size(50, 0);
			this.m_cmbFileName.Name = "m_cmbFileName";
			this.m_cmbFileName.Size = new System.Drawing.Size(165, 21);
			this.m_cmbFileName.TabIndex = 1;
			this.m_cmbFileName.SelectedIndexChanged += new System.EventHandler(this.m_cmbFileName_SelectedIndexChanged);
			this.m_cmbFileName.TextUpdate += new System.EventHandler(this.m_cmbFileName_TextUpdate);
			// 
			// btnBrowse
			// 
			this.btnBrowse.Dock = System.Windows.Forms.DockStyle.Right;
			this.btnBrowse.Font = new System.Drawing.Font("Tahoma", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnBrowse.Location = new System.Drawing.Point(167, 0);
			this.btnBrowse.Margin = new System.Windows.Forms.Padding(0);
			this.btnBrowse.MaximumSize = new System.Drawing.Size(21, 21);
			this.btnBrowse.MinimumSize = new System.Drawing.Size(21, 21);
			this.btnBrowse.Name = "btnBrowse";
			this.btnBrowse.Size = new System.Drawing.Size(21, 21);
			this.btnBrowse.TabIndex = 2;
			this.btnBrowse.Text = "...";
			this.btnBrowse.UseCompatibleTextRendering = true;
			this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
			// 
			// m_errorProvider
			// 
			this.m_errorProvider.ContainerControl = this;
			// 
			// FEditControl
			// 
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.Controls.Add(this.btnBrowse);
			this.Controls.Add(this.m_cmbFileName);
			this.Controls.Add(this.m_tbFileName);
			this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.MaximumSize = new System.Drawing.Size(400, 21);
			this.MinimumSize = new System.Drawing.Size(50, 21);
			this.Name = "FEditControl";
			this.Size = new System.Drawing.Size(188, 21);
			((System.ComponentModel.ISupportInitialize)(this.m_errorProvider)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

      }
      #endregion

      #region Properties Code

      /// <summary>
      /// Gets or sets the File Open/Save status.
      /// </summary>
      [Browsable(true), Category("GX_Parameters")]
      [Description("Determines whether to open or save the file.")]
		public FileOpenSaveEnum FileOpenSave
      {
         get
         {
            return m_eOpenSave;
         }
         set
         {
            m_eOpenSave = value;
         }
      }

      /// <summary>
      /// If true, then the field may not be left blank (dummy).
      /// </summary>
      [Browsable(true), Category("GX_Parameters")]
      [Description("If true, then the field may not be left blank (dummy).")]
		public bool Required
      {
         get
         {
            return m_bRequired;
         }
         set
         {
            m_bRequired = value;
				if (RequiredChanged != null)
					RequiredChanged(this, new EventArgs());
         }
      }

      /// <summary>
      /// If >0, then a dropdown box will be displayed showing a history of N previously selected files
      /// at the time this control was last serialized"</summary>
      [Browsable(true), Category("GX_Parameters")]
      [Description("If >0, then a dropdown box will be displayed showing a history of N previously selected files at the time this control was last serialized.")]
		public int MaxHistory
      {
         get
         {
            return m_iMaxHistory;
         }
         set
         {
            m_iMaxHistory = value;
            if (m_iMaxHistory == 0)
               ClearHistory();
            else
            {
               if (m_hLST == null)
                  m_hLST = new List<string>(m_iMaxHistory);
               else
               {
                  if (m_hLST.Count > m_iMaxHistory)
                     m_hLST.RemoveRange(m_iMaxHistory, m_hLST.Count);
               }
               FillHistory();
            }
         }
      }

      /// <summary>
      /// The current number of items in the history"</summary>
      [Browsable(true), Category("GX_Parameters")]
      [Description("The current number of items in the history")]
      internal int History
      {
         get
         {
            if (m_hLST == null)
               return 0;
            else 
               return m_hLST.Count;
         }
      }

      /// <summary>
      /// Gets or sets the title of the browse dialog launched using the browse button.
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


      /// <summary>
      /// Gets or sets the file filters. Example: "Image Files(*.BMP;*.JPG;*.GIF)|*.BMP;*.JPG;*.GIF|All files (*.*)|*.*"
      /// </summary>
      [Browsable(true), Category("GX_Parameters")]
      [Description("Gets or sets the file filters. Example: \"Image Files(*.BMP;*.JPG;*.GIF)|*.BMP;*.JPG;*.GIF|All files (*.*)|*.*\"")]
		public String Filters
      {
         get
         {
            if (m_eOpenSave == FileOpenSaveEnum.Open)
               return m_openFileDialog1.Filter;
            else
               return m_saveFileDialog1.Filter;
         }
         set
         {
            m_saveFileDialog1.Filter = value;
            m_openFileDialog1.Filter = value;
            m_saveFileDialog1.FilterIndex = 0;
            m_openFileDialog1.FilterIndex = 0;
         }
      }

      /// <summary>
      /// Gets or sets the file filter index"
      /// </summary>
      [Browsable(true), Category("GX_Parameters")]
      [Description("Gets or sets the file filter index.")]
		public int FilterIndex
      {
         get
         {
            if (m_eOpenSave == FileOpenSaveEnum.Open)
               return m_openFileDialog1.FilterIndex;
            else
               return m_saveFileDialog1.FilterIndex;
         }
         set
         {
            m_saveFileDialog1.FilterIndex = Math.Max(value, 1);
            m_openFileDialog1.FilterIndex = Math.Max(value, 1);
         }
      }

      [Browsable(false), Category("GX_Parameters")]
      [Description("Gets or sets the default file name.")]
		public string FileName
      {
         get 
         {
            if (m_hLST != null && m_hLST.Count > 0)
               return m_cmbFileName.Text ;
            else
               return m_tbFileName.Text;
         }
         set 
         {
            if (m_hLST != null && m_hLST.Count > 0)
               m_cmbFileName.Text = value;
            else
               m_tbFileName.Text = value;
         }
      }

      [Browsable(false), Category("GX_Parameters")]
      [Description("Gets or sets the default directory.")]
		public string InitialDirectory
      {
         get 
         {
            return m_strDirectory;
         }
         set 
         {
            m_strDirectory = value;
         }
      }
      #endregion

      #region Public Methods

      
      /// <summary>
      /// Set the file path value in an FEdit control.
      /// </summary>
      /// <remarks>
      /// The path is automatically split, so that only the file name in the text edit box.
      /// </remarks>
      /// <param name="strPath">The input path string.</param>
      internal void SetFilePath(string strPath)
      {
         // --- Set the volume, directory and edit box text ---

         string strFile = Path.GetFileName(strPath);
         m_strDirectory = Path.GetDirectoryName(strPath);
         if (m_hLST != null && m_hLST.Count > 0)
            m_cmbFileName.Text = strFile;
         else
            m_tbFileName.Text = strFile;
      }


      /// <summary>
      /// Get the file path value from an FEdit control.
      /// </summary>
      /// 
      /// <remarks>
      /// Folder with trailing directory separator returned for empty text. No validation is done on the retrieved path. For validation, call FEdit.bIsValid().
      /// </remarks>
      /// 
      /// <param name="strPath">The returned path </param>
      /// 
      /// <example>
      /// <code>
      /// 
      ///       private bool bValidateDialog()
      ///       {
      ///          bool bValid = true;
      ///       
      ///       
      ///          // --- Validate and register any errors in the dialog ---
      ///       
      ///          if (!m_FEdit.bIsValid || ...)
      ///              bValid = false;
      ///
      ///          return bValid;
      ///        }
      ///
      ///        private void btnOK_Click(object sender, System.EventArgs e)
      ///        {  
      ///           string strFile;
      /// 
      ///           try
      ///           {
      ///              if (!bValidateDialog(true))
      ///              { 
      ///                 DialogResult = DialogResult.None;
      ///                 return;
      ///              }
      ///
      ///              m_FEdit.GetFilePath(this.strFile);
      ///              
      ///              // --- Execute ---
      ///
      ///              Execute();
      ///              this.Close();
      ///           } 
      /// </code>
      /// </example>
		public void GetFilePath(ref string strPath)
      {
         // --- Return the current path, without validation ---

         string strError = "";
         bValidate(ref strPath, ref strError, false);
      }
      /// <summary>
      /// Check to see if the current file path passes validation tests.
      /// </summary>
      /// <remarks>The following validation is performed:
      /// <list type="bullet"><item><description>The file name(s) must not contain illegal characters.</description></item>
      /// <item><description>If the control is opened using FileOpenSaveEnum.Open, the the file(s) must exist.</description></item>
      /// <item><description>If the control is opened using FileOpenSaveEnum.Save, permission must have been given for overwriting.</description></item>
      /// <item><description>If the item is required, it must be defined.</description></item></list>
      /// </remarks>
      /// <param name="strError">The returned error string if the control does not validate.</param>
      /// <returns>true if validation succeeds</returns>
		public bool bIsValid(ref string strError)
      {
         string strPath = "";
         return (bValidate(ref strPath, ref strError, true));
      }
      #endregion

      #region Private Methods

      private void btnBrowse_Click(object sender, EventArgs e)
      {
         if (m_eOpenSave == FileOpenSaveEnum.Open)
            this.OpenFileDialog();
         else
            this.SaveFileDialog();
      }

      private void OpenFileDialog()
      {
         string strDefault;
         string strFile;

         // --- Combine the edit box part with the current volume and directory to make the
         //     default string ---

         if (m_hLST != null && m_hLST.Count > 0)
            strDefault = m_cmbFileName.Text;
         else
            strDefault = m_tbFileName.Text;

         m_openFileDialog1.FileName = strDefault;
         m_openFileDialog1.InitialDirectory = m_strDirectory;
         m_openFileDialog1.RestoreDirectory = true;
         m_openFileDialog1.Title = m_strTitle;

         if (m_openFileDialog1.ShowDialog() != DialogResult.OK)
            return;

         strFile = Path.GetFileName(m_openFileDialog1.FileName);
         m_strDirectory = Path.GetDirectoryName(m_openFileDialog1.FileName);

         if (m_hLST != null && m_hLST.Count > 0)
            m_cmbFileName.Text = strFile;
         else
            m_tbFileName.Text = strFile;

         if (FEditTextChanged != null)
            FEditTextChanged(this);
      }

      private void SaveFileDialog()
      {
         string strDefault;
         string strFile;


         // --- Combine the edit box part with the current volume and directory to make the
         //     default string ---

         if (m_hLST != null && m_hLST.Count > 0)
            strDefault = m_cmbFileName.Text;
         else
            strDefault = m_tbFileName.Text;

         m_saveFileDialog1.FileName = strDefault;
         m_saveFileDialog1.InitialDirectory = m_strDirectory;
         m_saveFileDialog1.RestoreDirectory = true;
         m_saveFileDialog1.Title = m_strTitle;
         m_saveFileDialog1.OverwritePrompt = true;

         if (m_saveFileDialog1.ShowDialog() != DialogResult.OK)
            return;

         strFile = Path.GetFileName(m_saveFileDialog1.FileName);
         m_strDirectory = Path.GetDirectoryName(m_saveFileDialog1.FileName);
               
         if (m_hLST != null && m_hLST.Count > 0)
            m_cmbFileName.Text = strFile;
         else
            m_tbFileName.Text = strFile;

         m_strOverwriteFile = Path.Combine(m_strDirectory, strFile);    // Warning will NOT be issued on this path again.

         if (FEditTextChanged != null)
            FEditTextChanged(this);
      }

      private bool bValidate(ref string strPath, ref string strError, bool bGetOverwritePermission)
      {
         string strText = m_hLST != null && m_hLST.Count > 0 ? m_cmbFileName.Text : m_tbFileName.Text;


         // --- Init to no errors, empty path, just folder---

         strError = "";
         strPath = ""; 
         strPath = m_strDirectory + Path.DirectorySeparatorChar;


         // --- Has a value been entered? ---

         if (strText.Length == 0)
         {
            if (m_bRequired)
            {
               // --- Defined? ---

               strError = "A file name is required.";
               return false;
            }
         }
         else
         {
            // Valid file name? ---

            bool bUpdate = false;
            string strP = Path.Combine(m_strDirectory, strText);

            // --- Get existing file parts: name, extension and qualifers ---

            string strName = Path.GetFileNameWithoutExtension(strP);
            string strExtension = Path.GetExtension(strP);

            // --- Default extension part. Get and add the default extension unless we are opening the
            //     file and the file does not exist, in which case it is better just to leave it and
            //     register an error. ---

            if (strExtension.Length == 0)
            {
               string strExt = "";

               // --- If the file name ends in a period ".", it
               //     indicates that no extension is desired, so just recreate the name/ext part without it.

               if (strP.Substring(strP.Length - 1, 1) != ".")
               {
                  // --- Add the default extension for the first file filter ---
                  int iIndex = Filters.IndexOf("|*.");
                  if (iIndex != -1)
                  {
                     char[] caAny = new char[] { '|', ';' };
                     strExt = Filters.Substring(iIndex + 3);
                     iIndex = strExt.IndexOfAny(caAny);
                     if (iIndex != -1)
                        strExt = strExt.Substring(0, iIndex);
                     if (strExt.IndexOf('*') != -1)
                        strExt = "";
                     else
                        strExt = "." + strExt;
                  }
               }

               // --- Update the extension if the extended file exists, or if it is being saved ---

               string strTestFile = Path.Combine(m_strDirectory, strName + strExt);
               if (m_eOpenSave == FileOpenSaveEnum.Save || File.Exists(strTestFile))
               {
                  if (!strExt.Equals(strExtension, StringComparison.InvariantCultureIgnoreCase))
                  {
                     strExtension = strExt;
                     bUpdate = true;
                  }
               }
            }


            // --- Re-create the path if the extension has changed. ---

            if (bUpdate)
            {
               strText = strName + strExtension;
               strP = Path.Combine(m_strDirectory, strText);
            }

            if (m_eOpenSave == FileOpenSaveEnum.Open)
            {
               if (!File.Exists(strP))
               {
                  strError = "The specified file does not exist.";
                  return false;
               }
            }
            else  // --- Overwrite warning (only on final validation) ---
            {
               // --- Warn only if the file or files already exists, and the warning has not already been issued.
               //     If the user has used the "Save file dialog" with this file, then the warning will 
               //     have been given already. ---

               if (File.Exists(strP) && !strP.Equals(m_strOverwriteFile, StringComparison.InvariantCultureIgnoreCase))
               {
                  string strMessage = "";

                  if (bGetOverwritePermission)
                  {
                     // --- Ask the user for overwrite permission ---

                     strMessage = "Overwrite the \"" + strP + "\" file?";
                     
                     if (DialogResult.Yes == MessageBox.Show(strMessage, "File Exists", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Question))
                        m_strOverwriteFile = strP;
                  }

                  if (!strP.Equals(m_strOverwriteFile, StringComparison.InvariantCultureIgnoreCase))
                  {
                     strError = "The specified file will be overwritten: " + strP;
                     strPath = strP;
                     return false;
                  }
               }
            }
            strPath = strP;
         }
         
         // --- Return true for successful validation ---

         return true;
      }

      private void FillHistory()
      {
         m_cmbFileName.Items.Clear();
         for (int i = 0; i < m_hLST.Count; i++)
            m_cmbFileName.Items.Add(Path.GetFileName(m_hLST[i]));
         
         // --- Switch to the combo box ---

         if (m_hLST.Count > 0)
         {
            m_tbFileName.Hide();
            m_cmbFileName.Show();
         }
         else
         {
            m_tbFileName.Show();
            m_cmbFileName.Hide();
         }
      }

      private void ClearHistory()
      {
         m_hLST = null;
         m_cmbFileName.Items.Clear();
         
         // --- Switch to the combo box ---

         m_tbFileName.Show();
         m_cmbFileName.Hide();
      }

      private void m_cmbFileName_SelectedIndexChanged(object sender, EventArgs e)
      {
         m_strDirectory = Path.GetDirectoryName(m_hLST[m_cmbFileName.SelectedIndex]);
         if (FEditTextChanged != null)
            FEditTextChanged(this);
      }

      private void m_cmbFileName_TextUpdate(object sender, EventArgs e)
      {
         if (FEditTextChanged != null)
            FEditTextChanged(this);
      }

      private void m_tbFileName_TextChanged(object sender, EventArgs e)
      {
         if (FEditTextChanged != null)
            FEditTextChanged(this);
      }
      #endregion

   }
}
