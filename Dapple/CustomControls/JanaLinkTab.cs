using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Dapple.CustomControls
{
   /// <summary>
   /// Another Jana tab view, this time using LinkLabels.
   /// </summary>
   public partial class JanaLinkTab : UserControl
   {
      #region Statics

      const int NUM_PAGES = 2;

      #endregion

      #region Memeber variables

      private int m_iCurrentPage = 0;
      private Control[] m_cControls = new Control[NUM_PAGES];
      private LinkLabel[] m_cLinks = new LinkLabel[NUM_PAGES];

      #endregion

      #region Constructor

      public JanaLinkTab()
      {
         for (int count = 0; count < NUM_PAGES; count++)
         {
            m_cLinks[count] = new LinkLabel();
            m_cLinks[count].Font = new Font("Arial", 8, FontStyle.Bold);
            m_cLinks[count].Text = "Link " + count;
            m_cLinks[count].Padding = new Padding(3);
            m_cLinks[count].Tag = count;
            m_cLinks[count].DisabledLinkColor = SystemColors.ControlText;
            m_cLinks[count].LinkClicked += new LinkLabelLinkClickedEventHandler(LinkClicked);
            this.Controls.Add(m_cLinks[count]);
         }
         m_cLinks[0].Enabled = false;

         InitializeComponent();
      }

      #endregion

      #region Events

      void LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
      {
         foreach (LinkLabel oLink in m_cLinks)
         {
            oLink.Enabled = true;
         }
         ((LinkLabel)sender).Enabled = false;

         PageChanged((int)((LinkLabel)sender).Tag);
      }

      protected override void OnResize(EventArgs e)
      {
         base.OnResize(e);

         PositionLabels();
      }

      private void PositionLabels()
      {
         int iFreeSpace = this.Width;

         foreach (LinkLabel oLabel in m_cLinks)
         {
            iFreeSpace -= (oLabel.PreferredWidth + oLabel.Padding.Horizontal);
         }
         
         int iIncrement = iFreeSpace / (m_cLinks.Length + 1);

         int iCurrPosition = iIncrement;

         foreach (LinkLabel oLabel in m_cLinks)
         {
            oLabel.Location = new Point(iCurrPosition, oLabel.Padding.Top);
            iCurrPosition += (oLabel.PreferredWidth + oLabel.Padding.Horizontal + iIncrement);
         }
      }

      void PageChanged(int iIndex)
      {
         if (m_iCurrentPage == iIndex) return;

         if (m_cControls[iIndex] != null) m_cControls[iIndex].Visible = true;

         m_iCurrentPage = iIndex;

         for (int count = 0; count < NUM_PAGES; count++)
         {
            if (m_cControls[count] != null && count != m_iCurrentPage)
               m_cControls[count].Visible = false;
         }
      }

      #endregion

      #region Public Methods

      public void SetPage(int iPage, Control oControl)
      {
         this.SuspendLayout();

         if (m_cControls[iPage] != null) this.Controls.Remove(m_cControls[iPage]);

         m_cControls[iPage] = oControl;

         this.Controls.Add(m_cControls[iPage]);
         m_cControls[iPage].Visible = m_iCurrentPage == iPage;
         m_cControls[iPage].Anchor = AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Top;

         int iOffset = m_cLinks[0].PreferredHeight + m_cLinks[0].Padding.Vertical;
         m_cControls[iPage].Location = new Point(0, iOffset);
         m_cControls[iPage].Size = new Size(this.Width, this.Height - iOffset);

         this.ResumeLayout();
      }

      public void SetText(int iLinkIndex, String szText)
      {
         m_cLinks[iLinkIndex].Text = szText;
         m_cLinks[iLinkIndex].Size = m_cLinks[iLinkIndex].PreferredSize;
         PositionLabels();
      }

      #endregion
   }
}
