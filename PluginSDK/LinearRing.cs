using System;

namespace WorldWind
{
	/// <summary>
	/// Summary description for LinearRing.
	/// </summary>
	public class LinearRing
	{
		internal Point3d[] Points = null;

		public LinearRing(Point3d[] points)
		{
			Points = points;
		}
	}
}
