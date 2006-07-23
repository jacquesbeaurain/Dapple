using System;
using System.Collections.Generic;
using System.Text;

namespace WorldWind
{
   public struct Viewport2d
   {
      public int Height;
      public double MaxZ;
      public double MinZ;
      public int Width;
      public int X;
      public int Y;

      public Viewport2d(int height, double maxZ, double minZ, int width, int x, int y) 
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
