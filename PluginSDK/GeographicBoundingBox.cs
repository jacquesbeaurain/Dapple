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

		public GeographicBoundingBox()
		{
			North = 90;
			South = -90;
			West = -180;
			East = 180;
		}

      public GeographicBoundingBox(double north, double south, double west, double east)
      {
         North = north;
         South = south;
         West = west;
         East = east;
      }

		public GeographicBoundingBox(double north, double south, double west, double east, double minAltitude, double maxAltitude)
		{
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

      public override string ToString()
      {
         return String.Format("({0:F2} W,{1:F2} S) -> ({2:F2} E,{3:F2} N)", West, South, East, North);
      }

      #region ICloneable Members

      public object Clone()
      {
         return new GeographicBoundingBox(North, South, West, East, MinimumAltitude, MaximumAltitude);
      }

      #endregion
   }
}
