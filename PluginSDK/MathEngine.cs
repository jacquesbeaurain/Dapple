using System;

namespace WorldWind
{
	/// <summary>
	/// Commonly used mathematical functions.
	/// </summary>
	public sealed class MathEngine
	{
		/// <summary>
		/// This class has only static methods.
		/// </summary>
		private MathEngine()
		{
		}

		/// <summary>
		/// Converts position in spherical coordinates (lat/lon/altitude) to cartesian (XYZ) coordinates.
		/// </summary>
		/// <param name="latitude">Latitude in decimal degrees</param>
		/// <param name="longitude">Longitude in decimal degrees</param>
		/// <param name="radius">Radius (OBS: not altitude)</param>
		/// <returns>Coordinates converted to cartesian (XYZ)</returns>
		internal static void SphericalToCartesian(
			double latitude,
			double longitude,
			double radius,
         ref Point3d v
			)
		{
			latitude *= System.Math.PI / 180.0;
			longitude *= System.Math.PI /180.0;

			double radCosLat = radius * Math.Cos(latitude);

			v.X = radCosLat * Math.Cos(longitude);
			v.Y = radCosLat * Math.Sin(longitude);
			v.Z = radius * Math.Sin(latitude);
		}
		public static Point3d SphericalToCartesian(
         double latitude,
         double longitude,
         double radius
         )
      {
         Point3d v = Point3d.Empty;
         SphericalToCartesian(latitude, longitude, radius, ref v);
         return v;   
      }

      /// <summary>
		/// Converts position in spherical coordinates (lat/lon/altitude) to cartesian (XYZ) coordinates.
		/// </summary>
		/// <param name="latitude">Latitude (Angle)</param>
		/// <param name="longitude">Longitude (Angle)</param>
		/// <param name="radius">Radius (OBS: not altitude)</param>
		/// <returns>Coordinates converted to cartesian (XYZ)</returns>
      internal static void SphericalToCartesian(
			Angle latitude,
			Angle longitude,
			double radius,
         ref Point3d v)
		{
			double latRadians = latitude.Radians;
			double lonRadians = longitude.Radians;

			double radCosLat = radius * Math.Cos(latRadians);

			v.X = radCosLat * Math.Cos(lonRadians);
			v.Y = radCosLat * Math.Sin(lonRadians);
			v.Z = radius * Math.Sin(latRadians);
		}
		public static Point3d SphericalToCartesian(
         Angle latitude,
         Angle longitude,
         double radius)
      {
         Point3d v = Point3d.Empty;
         SphericalToCartesian(latitude, longitude, radius, ref v);
         return v;   
      }

		/// <summary>
		/// Converts position in cartesian coordinates (XYZ) to spherical (lat/lon/radius) coordinates in radians.
		/// </summary>
		/// <returns>Coordinates converted to spherical coordinates.  X=radius, Y=latitude (radians), Z=longitude (radians).</returns>
		public static Point3d CartesianToSpherical(double x, double y, double z)
		{
			double rho = Math.Sqrt((x * x + y * y + z * z));
			double longitude = Math.Atan2(y,x);
			double latitude = (Math.Asin(z / rho));

         return new Point3d(rho, latitude, longitude);
		}
		
		/// <summary>
		/// Converts an angle in decimal degrees to angle in radians
		/// </summary>
		/// <param name="degrees">Angle in decimal degrees (0-360)</param>
		/// <returns>Angle in radians (0-2*Pi)</returns>
		public static double DegreesToRadians(double degrees)
		{
			return Math.PI * degrees / 180.0;
		}

		/// <summary>
		/// Converts an angle in radians to angle in decimal degrees 
		/// </summary>
		/// <param name="radians">Angle in radians (0-2*Pi)</param>
		/// <returns>Angle in decimal degrees (0-360)</returns>
		internal static double RadiansToDegrees(double radians)
		{
			return  radians * 180.0 / Math.PI;
		}

		/// <summary>
		/// Computes the angle (seen from the center of the sphere) between 2 sets of latitude/longitude values.
		/// </summary>
		/// <param name="latA">Latitude of point 1 (decimal degrees)</param>
		/// <param name="lonA">Longitude of point 1 (decimal degrees)</param>
		/// <param name="latB">Latitude of point 2 (decimal degrees)</param>
		/// <param name="lonB">Longitude of point 2 (decimal degrees)</param>
		/// <returns>Angle in decimal degrees</returns>
		internal static double SphericalDistanceDegrees(double latA, double lonA, double latB, double lonB)
		{
         double radLatA = MathEngine.DegreesToRadians(latA);
         double radLatB = MathEngine.DegreesToRadians(latB);
         double radLonA = MathEngine.DegreesToRadians(lonA);
         double radLonB = MathEngine.DegreesToRadians(lonB);

         return MathEngine.RadiansToDegrees(
            Math.Acos(Math.Cos(radLatA) * Math.Cos(radLatB) * Math.Cos(radLonA - radLonB) + Math.Sin(radLatA) * Math.Sin(radLatB)));
      }

      /// <summary>
		/// Computes the angular distance between two pairs of lat/longs.
		/// Fails for distances (on earth) smaller than approx. 2km. (returns 0)
		/// </summary>
		internal static Angle SphericalDistance(Angle latA, Angle lonA, Angle latB, Angle lonB)
		{
			double radLatA = latA.Radians;
			double radLatB = latB.Radians;
			double radLonA = lonA.Radians;
			double radLonB = lonB.Radians;

			return Angle.FromRadians( Math.Acos(
				Math.Cos(radLatA)*Math.Cos(radLatB)*Math.Cos(radLonA-radLonB)+
				Math.Sin(radLatA)*Math.Sin(radLatB)) );
		}

		/// Compute the tile number (used in file names) for given latitude and tile size.
		/// </summary>
		/// <param name="latitude">Latitude (decimal degrees)</param>
		/// <param name="tileSize">Tile size  (decimal degrees)</param>
		/// <returns>The tile number</returns>
		internal static int GetRowFromLatitude(double latitude, double tileSize)
		{
			return (int)System.Math.Truncate((System.Math.Abs(-90.0 - latitude) % 180) / tileSize);
		}

		/// <summary>
		/// Compute the tile number (used in file names) for given latitude and tile size.
		/// </summary>
		/// <param name="latitude">Latitude (decimal degrees)</param>
		/// <param name="tileSize">Tile size  (decimal degrees)</param>
		/// <returns>The tile number</returns>
		internal static int GetRowFromLatitude(Angle latitude, double tileSize)
		{
			return (int)System.Math.Truncate((System.Math.Abs(-90.0 - latitude.Degrees) % 180) / tileSize);
		}

		/// <summary>
		/// Compute the tile number (used in file names) for given longitude and tile size.
		/// </summary>
		/// <param name="longitude">Longitude (decimal degrees)</param>
		/// <param name="tileSize">Tile size  (decimal degrees)</param>
		/// <returns>The tile number</returns>
		internal static int GetColFromLongitude(double longitude, double tileSize)
		{
			return (int)System.Math.Truncate((System.Math.Abs(-180.0 - longitude) % 360) / tileSize);
		}

		/// <summary>
		/// Compute the tile number (used in file names) for given longitude and tile size.
		/// </summary>
		/// <param name="longitude">Longitude (decimal degrees)</param>
		/// <param name="tileSize">Tile size  (decimal degrees)</param>
		/// <returns>The tile number</returns>
		internal static int GetColFromLongitude(Angle longitude, double tileSize)
		{
			return (int)System.Math.Truncate((System.Math.Abs(-180.0 - longitude.Degrees) % 360) / tileSize);
		}
	}
}
