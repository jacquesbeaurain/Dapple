using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Dapple
{
   class FixedCollapseSplitContainer : SplitContainer
   {
      private bool m_bInvertSplitterFix = false;

      protected override void OnLayout(LayoutEventArgs e)
      {
         base.OnLayout(e);
         if (!m_bInvertSplitterFix && this.SplitterDistance != this.Panel1MinSize)
            this.SplitterDistance = this.Panel1MinSize;
         else if (m_bInvertSplitterFix & this.Height - this.SplitterDistance != this.Panel2MinSize)
            this.SplitterDistance = this.Height - this.Panel2MinSize;
      }

      [Browsable(true)]
      [Category("FixedCollapseSplitContainer")]
      internal bool InvertSplitterFix
      {
         set
         {
            m_bInvertSplitterFix = value;
         }
      }
   }
}
