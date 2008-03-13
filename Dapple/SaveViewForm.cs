using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using Geosoft.OpenGX.UtilityForms;

namespace Dapple
{
   public partial class SaveViewForm : Form
   {
      #region Private Members
      Image m_imgPreview = null;
      string m_strNotes = null;
      string m_strOutputPath = null;
      private string m_strConfigDir;
      private FEditControl m_fOpenEditControl;
      private FEditControl m_fEditControl;
      #endregion

      #region Constructor
      public SaveViewForm(string strConfigDir, Image preview)
      {
         InitializeComponent();
         m_strConfigDir = strConfigDir;

         m_fEditControl = null;
         try
         {
            // We pick up and serialize the path to the same cfg file as for the open dialog
            if (File.Exists(Path.Combine(strConfigDir, "viewhistory.cfg")))
               m_fOpenEditControl = FEditControl.DeSerialize(Path.Combine(strConfigDir, "viewhistory.cfg"));
            else
            {
               m_fOpenEditControl = new Geosoft.OpenGX.UtilityForms.FEditControl();
               m_fOpenEditControl.BrowseQuery = "Open Dapple View";
               m_fOpenEditControl.FileOpenSave = Geosoft.OpenGX.UtilityForms.FileOpenSaveEnum.Open;
               m_fOpenEditControl.FilterIndex = 1;
               m_fOpenEditControl.Filters = "Dapple View files (*.dapple)|*.dapple";
            }
         }
         catch
         {
				// m_fOpenEditControl is most likely null!
         }
         m_fEditControl = new Geosoft.OpenGX.UtilityForms.FEditControl();
         m_fEditControl.FilterIndex = 1;
         m_fEditControl.BrowseQuery = "Save Dapple View as...";
         m_fEditControl.FileOpenSave = Geosoft.OpenGX.UtilityForms.FileOpenSaveEnum.Save;
         m_fEditControl.Filters = "Dapple View files (*.dapple)|*.dapple";
         m_fEditControl.Required = true;
         m_fEditControl.MaxHistory = 0;
         m_fEditControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                     | System.Windows.Forms.AnchorStyles.Right)));
         m_fEditControl.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
         m_fEditControl.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
         m_fEditControl.Location = new System.Drawing.Point(47, 356);
         m_fEditControl.MaximumSize = new System.Drawing.Size(400, 21);
         m_fEditControl.MinimumSize = new System.Drawing.Size(50, 21);
         m_fEditControl.Name = "fEditControl";
         m_fEditControl.Size = new System.Drawing.Size(202, 21);
         m_fEditControl.TabIndex = 0;

         if (m_fOpenEditControl != null)
            m_fEditControl.InitialDirectory = m_fOpenEditControl.InitialDirectory;
         this.Controls.Add(m_fEditControl);
         
         
         this.Icon = new System.Drawing.Icon(@"app.ico");
         m_imgPreview = preview;
      }
      #endregion

      #region Properties

      public string OutputPath
      {
         get
         {
            return m_strOutputPath;
         }
      }

      public string Notes
      {
         get
         {
            return m_strNotes;
         }
      }
      #endregion

      #region Event Handlers
      private void btnOK_Click(object sender, EventArgs e)
      {
         string strError = "";
         if (m_fEditControl.bIsValid(ref strError))
         {
            m_fEditControl.GetFilePath(ref m_strOutputPath);
            m_strNotes = this.txtNotes.Text;
				if (m_fOpenEditControl != null)
				{
					m_fOpenEditControl.InitialDirectory = m_fEditControl.InitialDirectory;
					m_fOpenEditControl.FileName = m_fEditControl.FileName;
					m_fOpenEditControl.Serialize(Path.Combine(m_strConfigDir, "viewhistory.cfg"));
				}
            this.DialogResult = DialogResult.OK;
            Close();
         } else
            this.DialogResult = DialogResult.None;
      }

      private void picPreview_Paint(object sender, PaintEventArgs e)
      {
         double scale;
         int x, y;

         if (this.picPreview.ClientRectangle.Width <= 0 || this.picPreview.ClientRectangle.Height <= 0 ||
             m_imgPreview.Width <= 0 || m_imgPreview.Height <= 0)
            return;

         double scaleH = (double)m_imgPreview.Height / (double)this.picPreview.ClientRectangle.Height;
         double scaleW = (double)m_imgPreview.Width / (double)this.picPreview.ClientRectangle.Width;
         scale = Math.Max(scaleH, scaleW);

         x = (int)Math.Round((double)this.picPreview.ClientRectangle.Width / 2.0 - (double)m_imgPreview.Width / (scale * 2.0));
         y = (int)Math.Round((double)this.picPreview.ClientRectangle.Height / 2.0 - (double)m_imgPreview.Height / (scale * 2.0));

         e.Graphics.Clear(Color.Black);
         e.Graphics.DrawImage(m_imgPreview, new Rectangle(x, y, (int)Math.Round((double)m_imgPreview.Width / scale), (int)Math.Round((double)m_imgPreview.Height / scale)));
      }
      #endregion
   }
}