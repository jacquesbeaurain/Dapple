using System;
using System.Collections.Generic;
using System.Text;

namespace WorldWind
{
   public struct Plane2d
   {
      public double A;
      public double B;
      public double C;
      public double D;

		public Plane2d(double ai, double bi, double ci, double di)	
		{
         A = ai; B = bi; C = ci; D = di;
		}

      public void Normalize()
      {
         //Microsoft.DirectX.Plane test = new Microsoft.DirectX.Plane((float)this.A, (float)this.B, (float)this.C, (float)this.D);
         //test.Normalize();

         double dNormSize = (new Vector3d(A, B, C)).Length;
         double dInverseLength = dNormSize != 0.0 ? 1.0 / dNormSize : 1.0;

         this.A *= dInverseLength; this.B *= dInverseLength; this.C *= dInverseLength; this.D *= dInverseLength; 
      }

      public double DistanceToPoint(Vector3d p)
      {
         return this.A * p.X + this.B * p.Y + this.C * p.Z + this.D;
      }

      public override string ToString()
      {
         return "(" + A.ToString() + ", " + B.ToString() + ", " + C.ToString() + ", " + D.ToString() + ")";
      }
   }
}
