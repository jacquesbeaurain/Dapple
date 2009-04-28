using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Dapple
{
   class FixedCollapseSplitContainer : SplitContainer
   {
      protected override void OnLayout(LayoutEventArgs e)
      {
         base.OnLayout(e);
         if (this.SplitterDistance != this.Panel1MinSize)
            this.SplitterDistance = this.Panel1MinSize;
      }
   }
}
