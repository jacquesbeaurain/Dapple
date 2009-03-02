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
		internal Point2d (Point2d P) // copy constructor
		{
			X = P.X;
			Y = P.Y;
		}

		// other operators
		internal double norm()	// L2 norm
		{
			return Math.Sqrt(norm2());
		}

		internal double norm2() // squared L2 norm
		{
			return X*X + Y*Y;
		}

		internal Point2d normalize() // normalization
		{
			double n = norm();
			return new Point2d(X / n, Y / n);
		}

		internal double Length
		{
			get
			{
				return Math.Sqrt(X * X + Y * Y);
			}
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

		internal static double dot(Point2d P1, Point2d P2) // inner product 2
		{
			return (P1.X * P2.X + P1.Y * P2.Y);
		}
//	TODO: implement this...
	//	internal static Point2d operator *(Point2d P1, Point2d P2)
	//	{
	//		return new Point2d (P1.Y * P2.Z - P1.Z * P2.Y,
	//			P1.Z * P2.X - P1.X * P2.Z, P1.X * P2.Y - P1.Y * P2.X);
	//	}

		public static Point2d operator -(Point2d P)	// negation
		{
			return new Point2d (-P.X, -P.Y);
		}

	//	internal static Point3d cross(Point2d P1, Point2d P2) // cross product
	//	{
	//		return P1 * P2;
	//	}

		// Normal direction corresponds to a right handed traverse of ordered points.
	//	internal Point2d unit_normal (Point2d P0, Point2d P1, Point2d P2)
	//	{
	//		Point2d p = (P1 - P0) * (P2 - P0);
	//		double l = p.norm ();
	//		return new Point2d (p.X / l, p.Y / l);
	//	}
	}
}
