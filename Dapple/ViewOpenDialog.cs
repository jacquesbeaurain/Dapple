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
      private XmlDocument m_xmlDoc = new XmlDocument();
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
               this.lblNotes.Text = view.View.notes.Value;
            else
               this.lblNotes.Text = "";
            m_strViewFile = strView;
            this.butOK.Enabled = true;
         }
         catch
         {
            this.butOK.Enabled = false;
            m_strViewFile = null;
            m_viewImage = null;
            this.lblNotes.Text = "";
         }
      }

      private void pictureBox_Paint(object sender, PaintEventArgs e)
      {
         StringFormat format = new StringFormat();
         System.Drawing.Imaging.ImageAttributes imageAttr = new System.Drawing.Imaging.ImageAttributes();

         format.LineAlignment = StringAlignment.Center;
         format.Alignment = StringAlignment.Center;

         if (m_viewImage != null)
         {
            double scale;
            int    x, y;

            if (this.pictureBox.ClientRectangle.Width <= 0 || this.pictureBox.ClientRectangle.Height <= 0 ||
                m_viewImage.Width <= 0 || m_viewImage.Height <= 0)
               return;

            double scaleH = (double)m_viewImage.Height / (double)pictureBox.ClientRectangle.Height;
            double scaleW = (double)m_viewImage.Width / (double)this.pictureBox.ClientRectangle.Width;
            scale = Math.Max(scaleH, scaleW);

            x = (int)Math.Round((double)this.pictureBox.ClientRectangle.Width / 2.0 - (double)m_viewImage.Width / (scale * 2.0));
            y = (int) Math.Round((double)this.pictureBox.ClientRectangle.Height / 2.0 - (double)m_viewImage.Height / (scale * 2.0));

            e.Graphics.Clear(Color.Black);
            e.Graphics.DrawImage(m_viewImage, new Rectangle(x, y, (int)Math.Round((double)m_viewImage.Width / scale), (int)Math.Round((double)m_viewImage.Height / scale)));
         }
         else if (m_strViewFile != null && m_strViewFile.Length > 0)
            e.Graphics.DrawString("No preview available in this view", this.Font, Brushes.Black, this.pictureBox.ClientRectangle, format);
         else
            e.Graphics.DrawString("Select Dapple view for preview", this.Font, Brushes.Black, this.pictureBox.ClientRectangle, format);

         e.Graphics.DrawRectangle(Pens.Black, this.pictureBox.ClientRectangle.Left, this.pictureBox.ClientRectangle.Top, this.pictureBox.ClientRectangle.Width - 1, this.pictureBox.ClientRectangle.Height - 1);
      }

      private void pictureBox_SizeChanged(object sender, EventArgs e)
      {
         this.pictureBox.Invalidate();
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
         this.pictureBox.Invalidate();
      }
      #endregion
   }
}