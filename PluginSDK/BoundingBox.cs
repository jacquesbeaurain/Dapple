using System;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using WorldWind.Camera;

namespace WorldWind
{
	/// <summary>
	/// The closed volume that completely contains a set of objects.
	/// </summary>
	public class BoundingBox
	{
      internal readonly Point3d[] corners = new Point3d[8];
      internal readonly BoundingSphere boundsphere = new BoundingSphere(new Point3d(0.0,0.0,0.0), 0.0);

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.BoundingSphere"/> class
		/// from a set of lat/lon values (degrees)
		/// </summary>
		/// <param name="south"></param>
		/// <param name="north"></param>
		/// <param name="west"></param>
		/// <param name="east"></param>
		/// <param name="radius1"></param>
		/// <param name="radius2"></param>
		public BoundingBox(double south, double north, double west, double east, double radius1, double radius2) 
		{
         Update(south, north, west, east, radius1, radius2);
		}
      internal void Update(double south, double north, double west, double east, double radius1, double radius2)
      {
         double scale = radius2 / radius1;
         MathEngine.SphericalToCartesian(south, west, radius1, ref this.corners[0]);
         this.corners[1] = this.corners[0];
         this.corners[1].scale(scale);
         MathEngine.SphericalToCartesian(south, east, radius1, ref this.corners[2]);
         this.corners[3] = this.corners[2];
         this.corners[3].scale(scale); 
         MathEngine.SphericalToCartesian(north, west, radius1, ref this.corners[4]);
         this.corners[5] = this.corners[4];
         this.corners[5].scale(scale); 
         MathEngine.SphericalToCartesian(north, east, radius1, ref this.corners[6]);
         this.corners[7] = this.corners[6];
         this.corners[7].scale(scale);

         ComputeBoundSphere();
      }

      internal void ComputeBoundSphere()
      {
         this.boundsphere.Center.X = 0.0;
         this.boundsphere.Center.Y = 0.0;
         this.boundsphere.Center.Z = 0.0;
         this.boundsphere.RadiusSq = 0.0;

         //Find the center.  In this case, we'll simply average the coordinates. 
         foreach (Point3d v in this.corners)
            this.boundsphere.Center += v;
         this.boundsphere.Center *= 1.0 / 8.0;

         //Loop through the coordinates and find the maximum distance from the center.  This is the radius.		
         foreach (Point3d v in this.corners)
         {
            double distSq = (v - this.boundsphere.Center).LengthSq;
            if (distSq > this.boundsphere.RadiusSq)
               this.boundsphere.RadiusSq = distSq;
         }
      }

		public override string ToString()
      {
         return String.Format("corner(0):\n{0}\ncorner(1):\n{1}\ncorner(2):\n{2}\ncorner(3):\n{3}\ncorner(4):\n{4}\ncorner(5):\n{5}\ncorner(6):\n{6}\ncorner(7):\n{7}\n",
            corners[0], corners[1], corners[2], corners[3], corners[4], corners[5], corners[6], corners[7]);
      }
	}
}
