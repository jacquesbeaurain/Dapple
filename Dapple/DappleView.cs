using System;
using System.Collections.Generic;
using System.Text;
using dappleview;

namespace Dapple
{
   internal class DappleView
   {
      private dappleviewDoc doc;
      private dappleviewType dappleView;

      internal DappleView()
      {
         this.doc = new dappleviewDoc();
         this.dappleView = new dappleviewType(doc.CreateRootElement("", "dappleview"));
      }

      internal DappleView(string strFile)
      {
         this.doc = new dappleviewDoc();
         this.dappleView = new dappleviewType(doc.Load(strFile));
      }
      
      internal void Save(string strFile)
      {
         this.doc = new dappleviewDoc();
         doc.Save(strFile, this.dappleView);
      }

      internal dappleviewType View
      {
         get
         {
            return this.dappleView;
         }
      }
   }
}
