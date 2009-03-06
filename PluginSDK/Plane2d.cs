using System;
using System.Collections.Generic;
using System.Text;

namespace WorldWind
{
   internal struct Plane2d
   {
      internal double A;
      internal double B;
      internal double C;
      internal double D;

		internal Plane2d(double ai, double bi, double ci, double di)	
		{
         A = ai; B = bi; C = ci; D = di;
		}

      internal void Normalize()
      {
         double dNormSize = (new Point3d(A, B, C)).Length;
         double dInverseLength = dNormSize != 0.0 ? 1.0 / dNormSize : 1.0;

         this.A *= dInverseLength; this.B *= dInverseLength; this.C *= dInverseLength; this.D *= dInverseLength; 
      }

		public override string ToString()
      {
         return "(" + A.ToString() + ", " + B.ToString() + ", " + C.ToString() + ", " + D.ToString() + ")";
      }
   }
}
