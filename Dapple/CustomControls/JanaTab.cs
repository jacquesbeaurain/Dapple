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
   /// A tab pane that doesn't look like a tab pane, just for Jana.
   /// </summary>
   public partial class JanaTab : UserControl
   {
      #region Statics

      const int NUM_PAGES = 2;

      #endregion

      #region Memeber variables

      private int m_iCurrentPage = 0;
      private Control[] m_cControls = new Control[NUM_PAGES];

      #endregion

      #region Constructor

      public JanaTab()
      {
         InitializeComponent();

         cTabToolbar.ButtonPressed += new TabToolStrip.TabToolbarButtonDelegate(PageChanged);
      }

      #endregion

      #region Properties

      public int SelectedIndex
      {
         get
         {
            return m_iCurrentPage;
         }
      }

      #endregion

      #region Events

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
         m_cControls[iPage].Location = new Point(0, 0);
         m_cControls[iPage].Size = new Size(this.Width, this.Height - cTabToolbar.Height);
         m_cControls[iPage].TabIndex = 0;

         this.ResumeLayout();
      }

      public void SetImage(int iPageIndex, Image oValue)
      {
         cTabToolbar.SetImage(iPageIndex, oValue);
      }

      #endregion
   }
}
