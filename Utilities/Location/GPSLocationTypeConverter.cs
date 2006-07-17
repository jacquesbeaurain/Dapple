using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;

namespace Utility
{
   namespace Location
   {
      /// <summary>
      /// GPS location type converter
      /// </summary>
      class GPSLocationTypeConverter : ExpandableObjectConverter
      {
         const string WrongType = "The type passed along is not of the type GPSLocation!";
         const string WrongNumberOfArgs = "Wrong number of arguments. You must provide a latitude and longitude.";
         const char Separator = ',';

         /// <summary>
         /// we can convert from a string to this type
         /// </summary>
         /// <param name="context">context descriptor</param>
         /// <param name="sourceType">the source data type</param>
         /// <returns></returns>
         public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
         {
            if (sourceType == typeof(string))
               return true;
            else
               return base.CanConvertFrom(context, sourceType);
         }

         /// <summary>
         /// we can convert to a string; instance descriptor not needed as it is only read-only
         /// </summary>
         /// <param name="context">context descriptor</param>
         /// <param name="destinationType">the destination data type</param>
         /// <returns></returns>
         public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
         {
            if (destinationType == typeof(string))
               return true;
            else
               return base.CanConvertTo(context, destinationType);
         }

         /// <summary>
         /// returns from a string to this type
         /// </summary>
         /// <param name="context">context descriptor</param>
         /// <param name="culture">culture to use</param>
         /// <param name="value">the value to convert from</param>
         /// <returns></returns>
         public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
         {
            // if no value is provided then we return a new instance of our type
            if (value == null)
               return new GPSLocation();

            // if the value is of type string then we process it
            if (value is string)
            {
               string StringValue = value as string;

               // if we have an empty string then we return again a new instance of our type
               if (StringValue.Length <= 0)
                  return new GPSLocation();

               // get the type converter for the Latitude and Longitude types
               TypeConverter LatitudeConverter = TypeDescriptor.GetConverter(typeof(Latitude));
               TypeConverter LongitudeConverter = TypeDescriptor.GetConverter(typeof(Longitude));

               // get the latitude and longitude
               string[] Values = StringValue.Split(Separator);

               // wrong number of arguments
               if (Values.Length != 2)
                  throw new Exception(WrongNumberOfArgs);

               // create the GPS location and return it
               return new GPSLocation((Latitude)LatitudeConverter.ConvertFromString(context, culture, Values[0].Trim()),
                                 (Longitude)LongitudeConverter.ConvertFromString(context, culture, Values[1].Trim()));
            }

            // otherwise we let the base converter process it
            else
               return base.ConvertFrom(context, culture, value);
         }

         /// <summary>
         /// converts from this type to a string
         /// </summary>
         /// <param name="context">context descriptor</param>
         /// <param name="culture">the culture info to use</param>
         /// <param name="value">the source value</param>
         /// <param name="destinationType">the destination type</param>
         /// <returns></returns>
         public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
         {
            // we check that the type passed along is of our type; otherwise we throw an exception
            if (value != null)
               if (!(value is GPSLocation))
                  throw new Exception(WrongType);

            // we convert from our type to a string
            if (destinationType == typeof(string))
            {
               // we have a null value so we return an empty string
               if (value == null)
                  return String.Empty;

               // strongly typed
               GPSLocation Location = value as GPSLocation;

               // get the type converter for the Latitude and Longitude types
               TypeConverter LatitudeConverter = TypeDescriptor.GetConverter(typeof(Latitude));
               TypeConverter LongitudeConverter = TypeDescriptor.GetConverter(typeof(Longitude));

               // convert GPS location
               return LatitudeConverter.ConvertToString(context, culture, Location.GPSLatitude) + Separator +
                     LongitudeConverter.ConvertToString(context, culture, Location.GPSLongitude);
            }

            // all other conversions we let the base converter do
            return base.ConvertTo(context, culture, value, destinationType);
         }
      }
   }
}