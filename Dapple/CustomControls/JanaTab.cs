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
   internal partial class JanaTab : UserControl
   {
      #region Statics

      const int NUM_PAGES = 2;

      #endregion

		#region Events

		internal delegate void PageChangedDelegate(int iPage);
		internal event PageChangedDelegate PageChanged;

		#endregion

		#region Memeber variables

		private int m_iCurrentPage = 0;
      private Control[] m_cControls = new Control[NUM_PAGES];

      #endregion

      #region Constructor

      internal JanaTab()
      {
         InitializeComponent();

         cTabToolbar.ButtonPressed += new TabToolStrip.TabToolbarButtonDelegate(OnPageChanged);
      }

      #endregion

      #region Properties

      internal int SelectedIndex
      {
         get
         {
            return m_iCurrentPage;
         }
      }

      #endregion

      #region Event Handlers

      void OnPageChanged(int iIndex)
      {
         if (m_iCurrentPage == iIndex) return;

         if (m_cControls[iIndex] != null) m_cControls[iIndex].Visible = true;

         m_iCurrentPage = iIndex;

         for (int count = 0; count < NUM_PAGES; count++)
         {
            if (m_cControls[count] != null && count != m_iCurrentPage)
               m_cControls[count].Visible = false;
         }

			if (PageChanged != null) PageChanged(iIndex);
      }

      #endregion

      #region Public Methods

      internal void SetPage(int iPage, Control oControl)
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

      internal void SetImage(int iPageIndex, Image oValue)
      {
         cTabToolbar.SetImage(iPageIndex, oValue);
      }

      internal void SetToolTip(int iPageIndex, String szToolTipText)
      {
         cTabToolbar.SetToolTip(iPageIndex, szToolTipText);
      }

		internal void SetNameAndText(int iPageIndex, String szName)
		{
			cTabToolbar.SetNameAndText(iPageIndex, szName);
		}

      #endregion
   }
}
