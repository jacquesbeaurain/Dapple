using System;

namespace WorldWind
{
	/// <summary>
	/// Bounding sphere.  The tightest sphere that will fit the bounded object, 
	/// that is, the smallest radius sphere that all points lie within. 
	/// </summary>
	internal class BoundingSphere
	{
		internal Point3d Center;
		internal double RadiusSq;

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.BoundingSphere"/> class
		/// from a center point and a radius.
		/// </summary>
		internal BoundingSphere(Point3d center, double radiussq)
		{
			this.Center = center;
			this.RadiusSq = radiussq;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.BoundingSphere"/> class
		/// from a set of lat/lon values (degrees)
		/// </summary>
      internal BoundingSphere(double south, double north, double west, double east, double radius1, double radius2)
		{
			// Compute the points in world coordinates
			Point3d[] corners = new Point3d[8];

         double scale = radius2 / radius1;
			corners[0] = MathEngine.SphericalToCartesian(south, west, radius1);
			corners[1] = corners[0] * scale;
			corners[2] = MathEngine.SphericalToCartesian(south, east, radius1);
			corners[3] = corners[2] * scale;
			corners[4] = MathEngine.SphericalToCartesian(north, west, radius1);
			corners[5] = corners[4] * scale;
			corners[6] = MathEngine.SphericalToCartesian(north, east, radius1);
			corners[7] = corners[6] * scale;

			//Find the center.  In this case, we'll simply average the coordinates. 
			foreach(Point3d v in corners)
				Center += v;
			Center *= 1.0 / 8.0;

			//Loop through the coordinates and find the maximum distance from the center.  This is the radius.		
			foreach(Point3d v in corners)
			{
            double distSq = (v - Center).LengthSq;
				if (distSq > RadiusSq)
					RadiusSq = distSq;
			}
		}		
	}
}
