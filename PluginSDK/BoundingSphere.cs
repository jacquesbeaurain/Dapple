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
	}
}
