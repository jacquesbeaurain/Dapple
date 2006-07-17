using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX;

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
         Plane p = new Plane((float)this.A, (float)this.B, (float)this.C, (float)this.D);
         p.Normalize();
         this.A = p.A; this.B = p.B; this.C = p.C; this.D = p.D; 
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
