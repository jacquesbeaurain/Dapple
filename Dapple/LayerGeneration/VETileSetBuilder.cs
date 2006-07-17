using System;
using System.Collections.Generic;
using System.Text;

namespace Dapple.LayerGeneration
{
   class VETileSetBuilder : BuilderDirectory
   {
      public VETileSetBuilder(string name, IBuilder parent, bool removable)
         : base(name, parent, removable)
      {
      }
   }
}
