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
      public bool KeepLayers;
      public string OutputName;
      public string Folder;
      public int Resolution;
      public string OutputFormat;

      private FEditControl m_fEditControl;
      private string m_strConfigDir;

      public ExportView(string strConfigDir)
      {
         InitializeComponent();
         m_strConfigDir = strConfigDir;

         m_fEditControl = null;
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
            m_fEditControl = new Geosoft.OpenGX.UtilityForms.FEditControl();
            m_fEditControl.BrowseQuery = "Save export as";
            m_fEditControl.FileOpenSave = Geosoft.OpenGX.UtilityForms.FileOpenSaveEnum.Save;
            m_fEditControl.FilterIndex = 1;
            m_fEditControl.Filters = "GeoTIFF (*.tif)|*.tif|BMP (*.bmp)|*.bmp|PNG (*.png)|*.png|GIF (*.gif)|*.gif";
         }

         m_fEditControl.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
         m_fEditControl.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
         m_fEditControl.Location = new System.Drawing.Point(89, 11);
         m_fEditControl.MaximumSize = new System.Drawing.Size(400, 21);
         m_fEditControl.MinimumSize = new System.Drawing.Size(50, 21);
         m_fEditControl.Name = "fEditControl";
         m_fEditControl.Size = new System.Drawing.Size(198, 21);
         m_fEditControl.TabIndex = 26;
         m_fEditControl.FEditTextChanged += new Geosoft.OpenGX.UtilityForms.FEditControl.FEditTextChangedEventHandler(FEditTextChanged);
         m_fEditControl.MaxHistory = 0;
         m_fEditControl.Required = true;
         this.Controls.Add(m_fEditControl);

         string strPath = "";
         m_fEditControl.GetFilePath(ref strPath);
         if (strPath.Length > 0 && strPath[strPath.Length-1] != Path.DirectorySeparatorChar)
            this.btnOK.Enabled = true;
         else
            this.btnOK.Enabled = false;
      }

      private void ExportView_Load(object sender, EventArgs e)
      {
         cmbRes.SelectedIndex = 1;
      }

      private void FEditTextChanged(object sender)
      {
         string strPath = "";
         m_fEditControl.GetFilePath(ref strPath);
         if (strPath.Length > 0 && strPath[strPath.Length - 1] != Path.DirectorySeparatorChar)
            this.btnOK.Enabled = true;
         else
            this.btnOK.Enabled = false;
      }

      private void btnOK_Click(object sender, EventArgs e)
      {
         string strError = "";
         if (m_fEditControl.bIsValid(ref strError))
         {
            bool bStripExtension = true;
            string strPath = "";

            m_fEditControl.GetFilePath(ref strPath);

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

            KeepLayers = checkKeepLayers.Checked;
            Folder = Path.GetDirectoryName(strPath);
            if (!Directory.Exists(Folder))
               Directory.CreateDirectory(Folder);
            else if (KeepLayers)
            {

               string[] arrExists = Directory.GetFiles(Folder, OutputName + "_*" + OutputFormat, SearchOption.TopDirectoryOnly);
               if (arrExists.Length > 0 && MessageBox.Show(this, "The individual layers option may cause some existing files to be overwritten in\n" + Folder + "\nContinue?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                  return;
            }

            if (cmbRes.SelectedIndex == 0)
               Resolution = 1024;
            else if (cmbRes.SelectedIndex == 1)
               Resolution = 1600;
            else if (cmbRes.SelectedIndex == 2)
               Resolution = 2048;
            else if (cmbRes.SelectedIndex == 3)
               Resolution = -1;

            m_fEditControl.Serialize(Path.Combine(m_strConfigDir, "exporthistory.cfg"));
            DialogResult = DialogResult.OK;
            Close();
         }
      }
   }
}