using System;

namespace WorldWind
{
	/// <summary>
	/// Summary description for Point3d.
	/// </summary>
	internal class Point2d
	{
		internal double X, Y;
		// constructors

		internal Point2d()
		{

		}

		internal Point2d (double xi, double yi)	// x,y constructor
		{
			X = xi; Y = yi;
		}

		public static Point2d operator +(Point2d P1, Point2d P2)	// addition 2
		{
			return new Point2d (P1.X + P2.X, P1.Y + P2.Y);
		}

		public static Point2d operator -(Point2d P1, Point2d P2)	// subtraction 2
		{
			return new Point2d (P1.X - P2.X, P1.Y - P2.Y);
		}

		public static Point2d operator *(Point2d P, double k)	// multiply by real 2
		{
			return new Point2d (P.X * k, P.Y * k);
		}

		public static Point2d operator *(double k, Point2d P)	// and its reverse order!
		{
			return new Point2d (P.X * k, P.Y * k);
		}

		public static Point2d operator /(Point2d P, double k)	// divide by real 2
		{
			return new Point2d (P.X / k, P.Y / k);
		}

		// Override the Object.Equals(object o) method:
		public override bool Equals(object o)
		{
			try
			{
				return (bool)(this == (Point2d)o);
			}
			catch
			{
				return false;
			}
		}

		// Override the Object.GetHashCode() method:
		public override int GetHashCode()
		{
			//not the best algorithm for hashing, but whatever...
			return (int)(X * Y);
		}

		public static bool operator ==(Point2d P1, Point2d P2) // equal?
		{
			return (P1.X == P2.X && P1.Y == P2.Y);
		}

		public static bool operator !=(Point2d P1, Point2d P2) // equal?
		{
			return (P1.X != P2.X || P1.Y != P2.Y);
		}

		public static Point2d operator -(Point2d P)	// negation
		{
			return new Point2d (-P.X, -P.Y);
		}
	}
}
