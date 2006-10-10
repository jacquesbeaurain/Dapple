using System;
using System.Collections.Generic;
using System.Text;
using dappleview;

namespace Dapple
{
   public class DappleView
   {
      private dappleviewDoc doc;
      private dappleviewType dappleView;

      public DappleView()
      {
         this.doc = new dappleviewDoc();
         this.dappleView = new dappleviewType(doc.CreateRootElement("", "dappleview"));
      }

      public DappleView(string strFile)
      {
         this.doc = new dappleviewDoc();
         this.dappleView = new dappleviewType(doc.Load(strFile));
      }
      
      public void Save(string strFile)
      {
         this.doc = new dappleviewDoc();
         doc.Save(strFile, this.dappleView);
      }

      public dappleviewType View
      {
         get
         {
            return this.dappleView;
         }
      }
   }
}
