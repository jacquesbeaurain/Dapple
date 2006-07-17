using System;
using System.ComponentModel;
using System.Globalization;

namespace Utility
{
   namespace Location
   {
      /// <summary>
      /// Latitude can be North or South of the equator
      /// </summary>
      public enum LatitudeDirection
      {
         North,
         South
      }

      internal class DMSConversion
      {
         public const int RecommendedDecimals = 2;

         public static double DMS2Double()
         {
            return 0;
         }

         public static void Double2DMS(double val, out int deg, out int min, out int sec, out int ifrac)
         {
            Double2DMS(val, out deg, out min, out sec, out ifrac, RecommendedDecimals);
         }

         public static void Double2DMS(double val, out int deg, out int min, out int sec, out int ifrac, int fracdec)
         {
            int i;
            double f, frac;

            // Take out the negative
            if (val < 0.0)
               val *= -1.0;

            // Break into Fractional Components
            deg = (int)Math.Floor(val);
            f = val - Math.Floor(val);

            // --- Determine the Minutes And Seconds/Frac ---
            frac = f * 3600.0;
            i = (int)Math.Floor(frac);
            frac = frac - Math.Floor(frac);
            if (frac > 0.999999999999)
            {
               // Adjust for rounding errors 
               i++;
               frac = 0.0;
            }
            sec = i;
            min = sec / 60;
            sec %= 60;

            ifrac = (int)Math.Round(frac * Math.Pow(10.0, (double)fracdec));
         }
      }

      /// <summary>
      /// latitude consists of degrees, minutes, seconds and North or South
      /// </summary>
      [TypeConverter(typeof(LatitudeTypeConverter))]
      public class Latitude
      {
         private int _Degrees;
         private int _Minutes;
         private int _Seconds;
         private int _DecimalSeconds;
         private LatitudeDirection _Direction;

         /// <summary>
         /// degrees
         /// </summary>
         public int Degrees
         {
            get
            {
               return _Degrees;
            }
            set
            {
               _Degrees = value;
            }
         }

         /// <summary>
         /// minutes
         /// </summary>
         public int Minutes
         {
            get
            {
               return _Minutes;
            }
            set
            {
               _Minutes = value;
            }
         }

         /// <summary>
         /// seconds
         /// </summary>
         public int Seconds
         {
            get
            {
               return _Seconds;
            }
            set
            {
               _Seconds = value;
            }
         }

         /// <summary>
         /// 1/100 seconds
         /// </summary>
         public int DecimalSeconds
         {
            get
            {
               return _DecimalSeconds;
            }
            set
            {
               _DecimalSeconds = value;
            }
         }

         /// <summary>
         /// direction
         /// </summary>
         public LatitudeDirection Direction
         {
            get
            {
               return _Direction;
            }
            set
            {
               _Direction = value;
            }
         }

         /// <summary>
         /// creates Latitude and sets values
         /// </summary>
         /// <param name="Degrees">degrees</param>
         /// <param name="Minutes">minutes</param>
         /// <param name="Seconds">seconds</param>
         /// <param name="DecimalSeconds">1/100 seconds</param>
         /// <param name="Direction">direction</param>
         public Latitude(int Degrees, int Minutes, int Seconds, int DecimalSeconds, LatitudeDirection Direction)
         {
            _Degrees = Degrees;
            _Minutes = Minutes;
            _Seconds = Seconds;
            _DecimalSeconds = DecimalSeconds;
            _Direction = Direction;
         }

         /// <summary>
         /// creates Latitude from a double value
         /// </summary>
         /// <param name="Value">value</param>
         public Latitude(double Value)
         {
            if (Value >= 0)
               _Direction = LatitudeDirection.North;
            else
            {
               _Direction = LatitudeDirection.South;
               Value *= -1;
            }

            DMSConversion.Double2DMS(Value, out _Degrees, out _Minutes, out _Seconds, out _DecimalSeconds);
         }

         /// <summary>
         /// default constructor
         /// </summary>
         public Latitude()
            : this(0, 0, 0, 0, LatitudeDirection.North)
         {
         }

         /// <summary>
         /// convert this type to a string using the invariant culture
         /// </summary>
         /// <returns></returns>
         public override string ToString()
         {
            return ToString(CultureInfo.InvariantCulture);
         }

         /// <summary>
         /// convert this type to a string
         /// </summary>
         /// <param name="Culture">the culture to use</param>
         /// <returns></returns>
         public string ToString(CultureInfo Culture)
         {
            return TypeDescriptor.GetConverter(GetType()).ConvertToString(null, Culture, this);
         }
      }

      /// <summary>
      /// Longitude can be West or East of Greenwitch (Prime Meridian)
      /// </summary>
      public enum LongitudeDirection
      {
         West,
         East
      }

      /// <summary>
      /// longitude consists of degrees, minutes, seconds and West or East
      /// </summary>
      [TypeConverter(typeof(LongitudeTypeConverter))]
      public class Longitude
      {
         private int _Degrees;
         private int _Minutes;
         private int _Seconds;
         private int _DecimalSeconds;
         private LongitudeDirection _Direction;

         /// <summary>
         /// degrees
         /// </summary>
         public int Degrees
         {
            get
            {
               return _Degrees;
            }
            set
            {
               _Degrees = value;
            }
         }

         /// <summary>
         /// minutes
         /// </summary>
         public int Minutes
         {
            get
            {
               return _Minutes;
            }
            set
            {
               _Minutes = value;
            }
         }

         /// <summary>
         /// seconds
         /// </summary>
         public int Seconds
         {
            get
            {
               return _Seconds;
            }
            set
            {
               _Seconds = value;
            }
         }

         /// <summary>
         /// 1/100 seconds
         /// </summary>
         public int DecimalSeconds
         {
            get
            {
               return _DecimalSeconds;
            }
            set
            {
               _DecimalSeconds = value;
            }
         }

         /// <summary>
         /// direction
         /// </summary>
         public LongitudeDirection Direction
         {
            get
            {
               return _Direction;
            }
            set
            {
               _Direction = value;
            }
         }

         /// <summary>
         /// creates Longitude and sets values
         /// </summary>
         /// <param name="Degrees">degrees</param>
         /// <param name="Minutes">minutes</param>
         /// <param name="Seconds">seconds</param>
         /// <param name="DecimalSeconds">1/100 seconds</param>
         /// <param name="Direction">direction</param>
         public Longitude(int Degrees, int Minutes, int Seconds, int DecimalSeconds, LongitudeDirection Direction)
         {
            _Degrees = Degrees;
            _Minutes = Minutes;
            _Seconds = Seconds;
            _DecimalSeconds = DecimalSeconds;
            _Direction = Direction;
         }

         /// <summary>
         /// creates Longitude from a double value
         /// </summary>
         /// <param name="Value">value</param>
         public Longitude(double Value)
         {
            if (Value >= 0)
               _Direction = LongitudeDirection.East;
            else
            {
               _Direction = LongitudeDirection.West;
               Value *= -1;
            }

            DMSConversion.Double2DMS(Value, out _Degrees, out _Minutes, out _Seconds, out _DecimalSeconds);
         }

         /// <summary>
         /// default constructor
         /// </summary>
         public Longitude()
            : this(0, 0, 0, 0, LongitudeDirection.West)
         {
         }

         /// <summary>
         /// convert this type to a string using the invariant culture
         /// </summary>
         /// <returns></returns>
         public override string ToString()
         {
            return ToString(CultureInfo.InvariantCulture);
         }

         /// <summary>
         /// convert this type to a string
         /// </summary>
         /// <param name="Culture">the culture to use</param>
         /// <returns></returns>
         public string ToString(CultureInfo Culture)
         {
            return TypeDescriptor.GetConverter(GetType()).ConvertToString(null, Culture, this);
         }
      }

      /// <summary>
      /// a GPS location consists of a latitude and longitude
      /// </summary>
      [TypeConverter(typeof(GPSLocationTypeConverter))]
      public class GPSLocation
      {
         Longitude _Longitude;
         Latitude _Latitude;

         /// <summary>
         /// longitude
         /// </summary>
         [Browsable(true)]
         [NotifyParentProperty(true)]
         public Longitude GPSLongitude
         {
            get
            {
               return _Longitude;
            }
            set
            {
               _Longitude = value;
            }
         }

         /// <summary>
         /// latitude
         /// </summary>
         [Browsable(true)]
         [NotifyParentProperty(true)]
         public Latitude GPSLatitude
         {
            get
            {
               return _Latitude;
            }
            set
            {
               _Latitude = value;
            }
         }

         /// <summary>
         /// instantiate a GPS location
         /// </summary>
         /// <param name="GPSLatitue">latitude</param>
         /// <param name="GPSLongitude">longitude</param>
         public GPSLocation(Latitude GPSLatitue, Longitude GPSLongitude)
         {
            _Latitude = GPSLatitue;
            _Longitude = GPSLongitude;
         }

         /// <summary>
         /// def instantiation
         /// </summary>
         public GPSLocation()
            : this(new Latitude(), new Longitude())
         {
         }

         /// <summary>
         /// convert this type to a string using the invariant culture
         /// </summary>
         /// <returns></returns>
         public override string ToString()
         {
            return ToString(CultureInfo.InvariantCulture);
         }

         /// <summary>
         /// convert this type to a string
         /// </summary>
         /// <param name="Culture">the culture to use</param>
         /// <returns></returns>
         public string ToString(CultureInfo Culture)
         {
            return TypeDescriptor.GetConverter(GetType()).ConvertToString(null, Culture, this);
         }
      }
   }
}