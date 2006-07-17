using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Reflection;

namespace Utility
{
   namespace Location
   {
      /// <summary>
      /// the Longitude type converter
      /// </summary>
      class LongitudeTypeConverter : TypeConverter
      {
         const string WrongType = "The type of value is not a Longitude type!";
         const string DegreesMissing = "The degrees value is missing!";
         const string DirectionMissing = "The longitude direction is missing! Can be W for West or E for East.";
         const string MinutesMissing = "The minutes value is missing!";
         const string SecondsMissing = "The seconds value is missing!";
         const string MisplacedDecimal = "Misplaced decimal!";
         const string DegreesUnit = "°";
         const string MinutesUnit = "'";
         const string SecondsUnit = "\"";
         const string DecimalUnit = ".";

         /// <summary>
         /// we can convert from a string to this type
         /// </summary>
         /// <param name="context">context descriptor</param>
         /// <param name="sourceType">source type</param>
         /// <returns></returns>
         public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
         {
            if (sourceType == typeof(string))
               return true;
            else
               return base.CanConvertFrom(context, sourceType);
         }

         /// <summary>
         /// we can convert to a string a instance descriptor
         /// </summary>
         /// <param name="context">context descriptor</param>
         /// <param name="destinationType">destination type</param>
         /// <returns></returns>
         public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
         {
            if ((destinationType == typeof(string)) |
               (destinationType == typeof(InstanceDescriptor)))
               return true;
            else
               return base.CanConvertTo(context, destinationType);
         }

         /// <summary>
         /// convert from Longitude to a string or instance descriptor
         /// </summary>
         /// <param name="context">context descriptor</param>
         /// <param name="culture">culture info</param>
         /// <param name="value">the value to convert</param>
         /// <param name="destinationType"></param>
         /// <returns></returns>
         public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
         {
            // check that the value we got passed on is of type Longitude
            if (value != null)
               if (!(value is Longitude))
                  throw new Exception(WrongType);

            // convert to a string
            if (destinationType == typeof(string))
            {
               // no value so we return an empty string
               if (value == null)
                  return string.Empty;

               // strongly typed
               Longitude LongValue = value as Longitude;

               // get the two type converters to use
               TypeConverter IntConverter = TypeDescriptor.GetConverter(typeof(int));
               TypeConverter EnumConverter = TypeDescriptor.GetConverter(typeof(LongitudeDirection));

               // convert to a string and return
               string strRet = IntConverter.ConvertToString(context, culture, LongValue.Degrees) + DegreesUnit + 
                     EnumConverter.ConvertToString(context, culture, LongValue.Direction).Substring(0, 1) +
                     IntConverter.ConvertToString(context, culture, LongValue.Minutes) + MinutesUnit +
                     IntConverter.ConvertToString(context, culture, LongValue.Seconds);
               if (LongValue.DecimalSeconds != 0)
                  strRet += DecimalUnit + IntConverter.ConvertToString(context, culture, LongValue.DecimalSeconds);
               strRet += SecondsUnit;
               return strRet;
            }

            // convert to a instance descriptor
            if (destinationType == typeof(InstanceDescriptor))
            {
               // no value so we return no instance descriptor
               if (value == null)
                  return null;

               // strongly typed
               Longitude LongValue = value as Longitude;

               // used to descripe the constructor
               MemberInfo Member = null;
               object[] Arguments = null;

               // get the constructor for the type
               Member = typeof(Longitude).GetConstructor(new Type[] { typeof(int), typeof(int), typeof(int), typeof(int), typeof(LongitudeDirection) });

               // create the arguments to pass along
               Arguments = new object[] { LongValue.Degrees, LongValue.Minutes, LongValue.Seconds, LongValue.DecimalSeconds, LongValue.Direction };

               // return the instance descriptor
               if (Member != null)
                  return new InstanceDescriptor(Member, Arguments);
               else
                  return null;
            }

            // call the base converter
            return base.ConvertTo(context, culture, value, destinationType);
         }

         /// <summary>
         /// convert from a string
         /// </summary>
         /// <param name="context">context descriptor</param>
         /// <param name="culture">culture info</param>
         /// <param name="value">value</param>
         /// <returns></returns>
         public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
         {
            // no value so we return a new Longitude instance
            if (value == null)
               return new Longitude();

            // convert from a string
            if (value is string)
            {
               // get strongly typed value
               string StringValue = value as string;

               // empty string so we return a new Longitude instance
               if (StringValue.Length <= 0)
                  return new Longitude();

               // get the position of the West longitude separator
               int DirectionPos = StringValue.IndexOf(LongitudeDirection.West.ToString().Substring(0, 1));
               LongitudeDirection Direction = LongitudeDirection.West;

               // if not found get the position of the East longitude separator
               if (DirectionPos == -1)
               {
                  DirectionPos = StringValue.IndexOf(LongitudeDirection.East.ToString().Substring(0, 1));
                  Direction = LongitudeDirection.East;
               }
               int DegreesPos = StringValue.IndexOf(DegreesUnit);
               if (DegreesPos == -1)
                  DirectionPos = Math.Min(DirectionPos, DegreesPos);

               // get the minutes and seconds characters
               int MinutesPos = StringValue.IndexOf(MinutesUnit);
               int SecondsPos = StringValue.IndexOf(SecondsUnit);
               int DecimalPos = StringValue.IndexOf(DecimalUnit);

               // no minutes present
               if (MinutesPos == -1)
                  throw new Exception(MinutesMissing);

               // no seconds present
               if (SecondsPos == -1)
                  throw new Exception(SecondsMissing);

               // decimal in wrong place
               if (DecimalPos != -1 && (DecimalPos > SecondsPos || DecimalPos < MinutesPos))
                  throw new Exception(MisplacedDecimal);

               // no minutes present
               if (DirectionPos == -1)
                  throw new Exception(DirectionMissing);

               // no degrees present
               if (DirectionPos == 0)
                  throw new Exception(DegreesMissing);

               // get the type converters we need
               TypeConverter IntConverter = TypeDescriptor.GetConverter(typeof(int));
               TypeConverter DblConverter = TypeDescriptor.GetConverter(typeof(double));

               // get the degrees, minutes and seconds value
               int Degrees = (int)IntConverter.ConvertFromString(context, culture, StringValue.Substring(0, DirectionPos));
               int Minutes = (int)IntConverter.ConvertFromString(context, culture, StringValue.Substring(DirectionPos + 1, MinutesPos - DirectionPos - 1));
               int DecimalSeconds = 0;
               int Seconds;
               if (DecimalPos != -1)
               {
                  Seconds = (int)IntConverter.ConvertFromString(context, culture, StringValue.Substring(MinutesPos + 1, DecimalPos - MinutesPos - 1));
                  DecimalSeconds = (int)Math.Round(Math.Pow(10.0, (double)DMSConversion.RecommendedDecimals) * (double)DblConverter.ConvertFromString(context, culture, "0." + StringValue.Substring(DecimalPos + 1, SecondsPos - DecimalPos - 1)));
               }
               else
                  Seconds = (int)IntConverter.ConvertFromString(context, culture, StringValue.Substring(MinutesPos + 1, SecondsPos - MinutesPos - 1));

               // create a new Longitude instance with these values and return it
               return new Longitude(Degrees, Minutes, Seconds, DecimalSeconds, Direction);
            }

            // otherwise call the base converter
            else
               return base.ConvertFrom(context, culture, value);
         }
      }
   }
}