using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Dapple.LayerGeneration
{
   class VETileSetBuilder : BuilderDirectory
   {
      public VETileSetBuilder(string name, IBuilder parent, bool removable, int iLayerImageIndex, int iDirectoryImageIndex)
         : base(name, parent, removable, iLayerImageIndex, iDirectoryImageIndex)
      {
      }
   }
}
