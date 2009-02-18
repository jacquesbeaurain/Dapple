using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Xml;
using System.Windows.Forms;
using Geosoft.OpenGX.UtilityForms;

namespace Dapple
{
   public partial class ViewOpenDialog : Form
   {
      #region Private Members
      private string m_strConfigDir;
      private string m_strViewFile = null;
      private Image m_viewImage = null;
      private FEditControl m_fEditControl;
      #endregion

      #region Constructor
      public ViewOpenDialog(string strConfigDir)
      {
         InitializeComponent();
         this.Icon = new System.Drawing.Icon(@"app.ico");
         m_strConfigDir = strConfigDir;

         m_fEditControl = null;
         try
         {
            if (File.Exists(Path.Combine(strConfigDir, "viewhistory.cfg")))
               m_fEditControl = FEditControl.DeSerialize(Path.Combine(strConfigDir, "viewhistory.cfg"));
         }
         catch
         {
         }
         if (m_fEditControl == null)
         {
            m_fEditControl = new Geosoft.OpenGX.UtilityForms.FEditControl();
            m_fEditControl.BrowseQuery = "Open Dapple View";
            m_fEditControl.FileOpenSave = Geosoft.OpenGX.UtilityForms.FileOpenSaveEnum.Open;
            m_fEditControl.FilterIndex = 1;
            m_fEditControl.Filters = "Dapple View files (*.dapple)|*.dapple";
         }
         
         m_fEditControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
         m_fEditControl.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
         m_fEditControl.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
         m_fEditControl.Location = new System.Drawing.Point(68, 456);
         m_fEditControl.MaximumSize = new System.Drawing.Size(400, 21);
         m_fEditControl.MinimumSize = new System.Drawing.Size(50, 21);
         m_fEditControl.Name = "fEditControl";
         m_fEditControl.Size = new System.Drawing.Size(180, 21);
         m_fEditControl.TabIndex = 8;
         m_fEditControl.MaxHistory = 10;
         m_fEditControl.Required = true;
         m_fEditControl.FEditTextChanged += new Geosoft.OpenGX.UtilityForms.FEditControl.FEditTextChangedEventHandler(FEditTextChanged);
         this.Controls.Add(m_fEditControl);
         
         string strPath = "";
         m_fEditControl.GetFilePath(ref strPath);
         if (strPath.Length > 0)
            ValidateView(strPath);
      }
      #endregion

      #region Properties
      public string ViewFile
      {
         get
         {
            return m_strViewFile;
         }
      }
      #endregion

      #region Events

      private void ValidateView(string strView)
      {
         try
         {
            DappleView view = new DappleView(strView);

            if (view.View.Haspreview())
            {
               // dont use the property from Altova it validates the bytes one by one and takes forever
               MemoryStream ms = new MemoryStream(Convert.FromBase64String(view.View.GetStartingpreviewCursor().InnerText));
               m_viewImage = Image.FromStream(ms);
            }
            else
               m_viewImage = null;

            if (view.View.Hasnotes())
               this.c_lNotes.Text = view.View.notes.Value;
            else
               this.c_lNotes.Text = "";
            m_strViewFile = strView;
            this.c_bOK.Enabled = true;
         }
         catch
         {
            this.c_bOK.Enabled = false;
            m_strViewFile = null;
            m_viewImage = null;
            this.c_lNotes.Text = "";
         }
      }

      private void pictureBox_Paint(object sender, PaintEventArgs e)
      {
         var format = new StringFormat
                               	{
                               		LineAlignment = StringAlignment.Center,
                               		Alignment = StringAlignment.Center
                               	};

      	if (m_viewImage != null)
         {
         	if (c_pbPreview.ClientRectangle.Width <= 0 || c_pbPreview.ClientRectangle.Height <= 0 ||
                m_viewImage.Width <= 0 || m_viewImage.Height <= 0)
               return;

            double scaleH = (double)m_viewImage.Height / (double)c_pbPreview.ClientRectangle.Height;
            double scaleW = (double)m_viewImage.Width / (double)c_pbPreview.ClientRectangle.Width;
            double scale = Math.Max(scaleH, scaleW);

            int x = (int)Math.Round((double)c_pbPreview.ClientRectangle.Width / 2.0 - (double)m_viewImage.Width / (scale * 2.0));
            int y = (int) Math.Round((double)c_pbPreview.ClientRectangle.Height / 2.0 - (double)m_viewImage.Height / (scale * 2.0));

            e.Graphics.Clear(Color.Black);
            e.Graphics.DrawImage(m_viewImage, new Rectangle(x, y, (int)Math.Round((double)m_viewImage.Width / scale), (int)Math.Round((double)m_viewImage.Height / scale)));
         }
         else if (!string.IsNullOrEmpty(m_strViewFile))
            e.Graphics.DrawString("No preview available in this view", Font, Brushes.Black, c_pbPreview.ClientRectangle, format);
         else
            e.Graphics.DrawString("Select Dapple view for preview", Font, Brushes.Black, c_pbPreview.ClientRectangle, format);

         e.Graphics.DrawRectangle(Pens.Black, c_pbPreview.ClientRectangle.Left, c_pbPreview.ClientRectangle.Top, c_pbPreview.ClientRectangle.Width - 1, c_pbPreview.ClientRectangle.Height - 1);
      }

      private void pictureBox_SizeChanged(object sender, EventArgs e)
      {
         this.c_pbPreview.Invalidate();
      }
      
      private void butOK_Click(object sender, EventArgs e)
      {
         m_fEditControl.Serialize(Path.Combine(m_strConfigDir, "viewhistory.cfg"));
      }

      private void FEditTextChanged(object sender)
      {
         string strPath = "";
         m_fEditControl.GetFilePath(ref strPath);
         if (strPath.Length > 0 && (String.IsNullOrEmpty(m_strViewFile) || !m_strViewFile.Equals(strPath)))
            ValidateView(strPath);
         this.c_pbPreview.Invalidate();
      }
      #endregion
   }
}