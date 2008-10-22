using System;
using System.Collections.Generic;
using System.Text;

namespace WorldWind
{
   public class GeographicQuad
   {
      public double X1, Y1; // lower left
      public double X2, Y2; // lower right
      public double X3, Y3; // upper right
      public double X4, Y4; // upper left

      public GeographicQuad(double _X1, double _Y1, double _X2, double _Y2, double _X3, double _Y3, double _X4, double _Y4)
      {
         X1 = _X1; Y1 = _Y1;
         X2 = _X2; Y2 = _Y2;
         X3 = _X3; Y3 = _Y3;
         X4 = _X4; Y4 = _Y4;
      }
   }

   public class GeographicBoundingBox : ICloneable
   {
      public double North;
      public double South;
      public double West;
      public double East;
		public double MinimumAltitude;
		public double MaximumAltitude;

		public GeographicBoundingBox() : this(90.0, -90.0, -180.0, 180.0, 0.0 , 0.0)
		{
		}

      public GeographicBoundingBox(double north, double south, double west, double east) : this(north, south, west, east, 0.0, 0.0)
      {
      }

		public GeographicBoundingBox(double north, double south, double west, double east, double minAltitude, double maxAltitude)
		{
			if (north < south) throw new ArgumentOutOfRangeException("Invalid bounding box parameters: north is less than south");
			if (east < west) throw new ArgumentOutOfRangeException("Invalid bounding box parameters: east is less than west");
			if (maxAltitude < minAltitude) throw new ArgumentOutOfRangeException("Invalid bounding box parameters: max altitude is less than min altitude");

			// --- Normalize longitude coordinates ---

			while ((east + west) / 2.0 < -180.0)
			{
				east += 360.0;
				west += 360.0;
			}
			while ((east + west) / 2.0 > 180.0)
			{
				east -= 360.0;
				west -= 360.0;
			}

			North = north;
			South = south;
			West = west;
			East = east;
			MinimumAltitude = minAltitude;
			MaximumAltitude = maxAltitude;
		}

      public static GeographicBoundingBox FromQuad(GeographicQuad quad)
      {
         return new GeographicBoundingBox(Math.Max(Math.Max(Math.Max(quad.Y1, quad.Y2), quad.Y3), quad.Y4),
            Math.Min(Math.Min(Math.Min(quad.Y1, quad.Y2), quad.Y3), quad.Y4),
            Math.Min(Math.Min(Math.Min(quad.X1, quad.X2), quad.X3), quad.X4),
            Math.Max(Math.Max(Math.Max(quad.X1, quad.X2), quad.X3), quad.X4));
      }

		public static GeographicBoundingBox NullBox()
		{
			GeographicBoundingBox result = new GeographicBoundingBox();
			result.North = double.MinValue;
			result.South = double.MaxValue;
			result.East = double.MinValue;
			result.West = double.MaxValue;
			result.MaximumAltitude = double.MinValue;
			result.MinimumAltitude = double.MaxValue;

			return result;
		}

		public bool IsValid
		{
			get
			{
				return North >= South && East >= West && MaximumAltitude >= MinimumAltitude;
			}
		}

		public void Union(GeographicBoundingBox other)
		{
			this.North = Math.Max(this.North, other.North);
			this.South = Math.Min(this.South, other.South);
			this.East = Math.Max(this.East, other.East);
			this.West = Math.Min(this.West, other.West);
			this.MaximumAltitude = Math.Max(this.MaximumAltitude, other.MaximumAltitude);
			this.MinimumAltitude = Math.Min(this.MinimumAltitude, other.MinimumAltitude);
		}

		public void Union(double dLongitude, double dLatitude, double dAltitude)
		{
			this.North = Math.Max(this.North, dLatitude);
			this.South = Math.Min(this.South, dLatitude);
			this.East = Math.Max(this.East, dLongitude);
			this.West = Math.Min(this.West, dLongitude);
			this.MaximumAltitude = Math.Max(this.MaximumAltitude, dAltitude);
			this.MinimumAltitude = Math.Min(this.MinimumAltitude, dAltitude);
		}

		public double Longitude
		{
			get { return East - West; }
		}

		public double Latitude
		{
			get { return North - South; }
		}

		public double Height
		{
			get { return MaximumAltitude - MinimumAltitude; }
		}

		public double CenterLatitude
		{
			get { return (North + South) / 2.0; }
		}

		public double CenterLongitude
		{
			get { return (East + West) / 2.0; }
		}

		public bool Intersects(GeographicBoundingBox boundingBox)
		{
			if(North <= boundingBox.South ||
				South >= boundingBox.North ||
				West >= boundingBox.East ||
				East <= boundingBox.West)
			{
				return false;
			}
			else
			{
				return true;
			}
		}

      public bool Contains(GeographicBoundingBox test)
      {
         return (test.West >= this.West && test.East <= this.East && test.South >= this.South && test.North < this.North);
      }

      public override bool Equals(object obj)
      {
         if (!(obj is GeographicBoundingBox)) return false;
         GeographicBoundingBox castObj = obj as GeographicBoundingBox;

         return North == castObj.North && East == castObj.East && South == castObj.South && West == castObj.West;
      }

		public bool Equivalent(GeographicBoundingBox other, double tolerance)
		{
			return this.North - other.North < tolerance && this.East - other.East < tolerance && this.South - other.South < tolerance && this.West - other.West < tolerance;
		}

		public override int GetHashCode()
		{
			return North.GetHashCode() ^ East.GetHashCode() ^ South.GetHashCode() ^ West.GetHashCode() ^ MinimumAltitude.GetHashCode() ^ MaximumAltitude.GetHashCode();
		}

      public override string ToString()
      {
         return String.Format("({0:F2} {4},{1:F2} {5}) -> ({2:F2} {6},{3:F2} {7})", Math.Abs(West), Math.Abs(South), Math.Abs(East), Math.Abs(North),
            West < 0 ? "W" : "E", South < 0 ? "S" : "N", East < 0 ? "W" : "E", North < 0 ? "S" : "N");
      }

      #region ICloneable Members

      public object Clone()
      {
         return new GeographicBoundingBox(North, South, West, East, MinimumAltitude, MaximumAltitude);
      }

      #endregion
        public bool Contains(Point3d p)
        {
            if (North < p.Y ||
                South > p.Y ||
                West > p.X ||
                East < p.X ||
                MaximumAltitude < p.Z ||
                MinimumAltitude > p.Z)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
	}
}
