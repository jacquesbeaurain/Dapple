using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Dapple.LayerGeneration
{
   class VETileSetBuilder : BuilderDirectory
   {
      public VETileSetBuilder(string name, IBuilder parent, bool something, int iLayerImageIndex, int iDirectoryImageIndex)
         : base(name, parent, something, iLayerImageIndex, iDirectoryImageIndex)
      {
      }
   }
}
