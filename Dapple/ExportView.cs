using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Geosoft.OpenGX.UtilityForms;

namespace Dapple
{
   public partial class ExportView : Form
   {
      public string OutputName;
      public string Folder;
      public string OutputFormat;

      private string m_strConfigDir;

      public ExportView(string szConfigDir): this(szConfigDir, Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments))
      {
      }

      public ExportView(string strConfigDir, string strInitialFolder)
      {
         InitializeComponent();
         m_strConfigDir = strConfigDir;

         FEditControl m_fEditControl = null;
         try
         {
            if (File.Exists(Path.Combine(strConfigDir, "exporthistory.cfg")))
               m_fEditControl = FEditControl.DeSerialize(Path.Combine(strConfigDir, "exporthistory.cfg"));
         }
         catch
         {
         }
         if (m_fEditControl == null)
         {
            cFilenameControl.FileName = String.Empty;
				cFilenameControl.InitialDirectory = strInitialFolder;
         }
         else
         {
            cFilenameControl.FileName = m_fEditControl.FileName;
            cFilenameControl.InitialDirectory = m_fEditControl.InitialDirectory;
         }

         cFilenameControl.Name = "fEditControl";
      }

      private void btnOK_Click(object sender, EventArgs e)
      {
         string strError = "";
         if (cFilenameControl.bIsValid(ref strError))
         {
            bool bStripExtension = true;
            string strPath = "";

            cFilenameControl.GetFilePath(ref strPath);

            OutputFormat = Path.GetExtension(strPath);
            if (!OutputFormat.Equals(".tif", StringComparison.InvariantCultureIgnoreCase) &&
               !OutputFormat.Equals(".bmp", StringComparison.InvariantCultureIgnoreCase) &&
               !OutputFormat.Equals(".gif", StringComparison.InvariantCultureIgnoreCase) &&
               !OutputFormat.Equals(".png", StringComparison.InvariantCultureIgnoreCase))
            {
               OutputFormat = ".tif";
               bStripExtension = false;
            }

            if (bStripExtension)
               OutputName = Path.GetFileNameWithoutExtension(strPath);
            else
               OutputName = Path.GetFileName(strPath);

            Folder = Path.GetDirectoryName(strPath);
            if (!Directory.Exists(Folder)) Directory.CreateDirectory(Folder);

            cFilenameControl.Serialize(Path.Combine(m_strConfigDir, "exporthistory.cfg"));
            DialogResult = DialogResult.OK;
            Close();
         }
      }

		private void cFilenameControl_Validating(object sender, CancelEventArgs e)
		{
			string strPath = "";
			cFilenameControl.GetFilePath(ref strPath);
			if (strPath.Length <= 0 || strPath[strPath.Length - 1] == Path.DirectorySeparatorChar)
			{
				cFilenameErrorProvider.SetError(cFilenameControl, "A file name is required.");
				e.Cancel = true;
			}
			else
			{
				cFilenameErrorProvider.SetError(cFilenameControl, "");
			}
		}
   }
}