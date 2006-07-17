using System;
using System.Collections.Generic;
using System.Text;

namespace Dapple.LayerGeneration
{
   class TileSetBuilder : BuilderDirectory
   {
      public TileSetBuilder(string name, IBuilder parent, bool removable) : base(name, parent, removable) 
      {
      }
   }

   class TileSetSet : BuilderDirectory
   {
      public TileSetSet(string name, IBuilder parent, bool removable)
         : base(name, parent, removable)
      {
      }
   }
}
