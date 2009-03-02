using System;
using System.Collections.Generic;
using System.Text;

namespace WorldWind
{
   internal struct Viewport2d
   {
      internal int Height;
      internal double MaxZ;
      internal double MinZ;
      internal int Width;
      internal int X;
      internal int Y;

      internal Viewport2d(int height, double maxZ, double minZ, int width, int x, int y) 
      {
         Height = height; 
         MaxZ = maxZ; 
         MinZ = minZ; 
         Width = width; 
         X = x; 
         Y = y;
      }
   }
}
