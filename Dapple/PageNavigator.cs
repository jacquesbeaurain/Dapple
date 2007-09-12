using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace Dapple
{
   public partial class PageNavigator : UserControl, IPageNavigator
   {
      #region Events

      /// <summary>
      /// Invoked when the back button is pressed.
      /// </summary>
      public event ThreadStart PageBack;

      /// <summary>
      /// Invoked when the forward button is pressed.
      /// </summary>
      public event ThreadStart PageForward;

      #endregion

      #region Constructors

      public PageNavigator()
      {
         InitializeComponent();
      }

      #endregion

      #region Event Handlers

      private void cBackButton_Click(object sender, EventArgs e)
      {
         if (PageBack != null) PageBack();
      }

      private void cForwardButton_Click(object sender, EventArgs e)
      {
         if (PageForward != null) PageForward();
      }

      #endregion

      #region Display methods

      public void SetState(int iPage, int iNumResults)
      {
         int iNumPages = PagesFromResults(iNumResults);
         SetState(String.Format("Results {0}-{1} of {2}", iPage * ResultsPerPage + 1, Math.Min((iPage + 1) * PageNavigator.ResultsPerPage, iNumResults), iNumResults), iPage > 0, iPage < iNumPages - 1);
      }

      public void SetState(String szMessage)
      {
         SetState(szMessage, false, false);
      }

      private delegate void SetStateDelegate(String szMessage, bool blCanBack, bool blCanForward);
      public void SetState(String szMessage, bool blCanBack, bool blCanForward)
      {
         if (InvokeRequired)
         {
            Invoke(new SetStateDelegate(SetState), new object[] { szMessage, blCanBack, blCanForward });
         }
         else
         {
            cBackButton.Enabled = blCanBack;
            cForwardButton.Enabled = blCanForward;
            cPageLabel.Text = szMessage;
         }
      }

      #endregion

      #region Statics

      public static int ResultsPerPage = 10;

      public static int PagesFromResults(int iNumResults)
      {
         int result = iNumResults / ResultsPerPage;
         if (iNumResults % ResultsPerPage != 0) result++;
         return result;
      }

      #endregion
   }
}
