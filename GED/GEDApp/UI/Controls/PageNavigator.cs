using System;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;

namespace GED.App.UI.Controls
{
   public partial class PageNavigator : UserControl
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
         SetState(String.Format(CultureInfo.CurrentCulture, "Results {0}-{1} of {2}", iPage * ResultsPerPage + 1, Math.Min((iPage + 1) * PageNavigator.ResultsPerPage, iNumResults), iNumResults), iPage > 0, iPage < iNumPages - 1);
      }

      public void SetState(String strMessage)
      {
         SetState(strMessage, false, false);
      }

      private delegate void SetStateDelegate(String strMessage, bool blCanBack, bool blCanForward);
      public void SetState(String strMessage, bool blCanBack, bool blCanForward)
      {
         if (InvokeRequired)
         {
            Invoke(new SetStateDelegate(SetState), new object[] { strMessage, blCanBack, blCanForward });
         }
         else
         {
            c_bBack.Enabled = blCanBack;
            c_bForward.Enabled = blCanForward;
            c_lStatusMessage.Text = strMessage;
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
