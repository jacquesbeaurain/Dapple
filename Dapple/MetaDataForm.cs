using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Dapple
{
   public partial class MetaDataForm : Form
   {
      public MetaDataForm()
      {
         InitializeComponent();
         this.Icon = new System.Drawing.Icon(@"app.ico");
      }

      public DialogResult ShowDialog(IWin32Window owner, string location)
      {
         this.webBrowser1.Url = new Uri(location);
         return base.ShowDialog(owner);
      }
   }
}