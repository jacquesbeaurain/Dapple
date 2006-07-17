using System;
using System.Reflection;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;

namespace Geosoft.GX.DAPGetData
{
	/// <summary>
	/// Summary description for ResolutionConverter.
	/// </summary>
	public class ResolutionConverter : System.ComponentModel.DoubleConverter
	{      
      
      public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
      {
         return false;
      }

      public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
      {
         double               dResolution = 0;
         Geosoft.GXNet.CIPJ   oIPJ  = null;
         Geosoft.GXNet.CLTB   oUNI = null;

         double               dFactor = 0;
         string               strUnits = string.Empty;

         if (destinationType == typeof(string) && value is double)
         {
            try
            {
               if (context.Instance is GridOptions)
               {                  
                  GridOptions oOptions = (GridOptions)context.Instance;

                  dResolution = oOptions.Resolution;
               } 
               else if (context.Instance is PictureOptions)
               {
                  PictureOptions oOptions = (PictureOptions)context.Instance;

                  dResolution = oOptions.Resolution;
               }
               else if (context.Instance is MapOptions)
               {
                  MapOptions oOptions = (MapOptions)context.Instance;

                  dResolution = oOptions.Resolution;
               }

               // --- convert the resolution to the default units if we are in degrees ---

               oIPJ = Geosoft.GXNet.CIPJ.Create();
               if (GetDapData.Instance.OMExtents != null)          
                  oIPJ.SetGXF("", GetDapData.Instance.OMExtents.CoordinateSystem.Datum, GetDapData.Instance.OMExtents.CoordinateSystem.Method, GetDapData.Instance.OMExtents.CoordinateSystem.Units, GetDapData.Instance.OMExtents.CoordinateSystem.LocalDatum);
               else
                  oIPJ.SetGXF("", GetDapData.Instance.SearchExtents.CoordinateSystem.Datum, GetDapData.Instance.SearchExtents.CoordinateSystem.Method, GetDapData.Instance.SearchExtents.CoordinateSystem.Units, GetDapData.Instance.SearchExtents.CoordinateSystem.LocalDatum);

               oIPJ.IGetUnits(ref dFactor, ref strUnits);         
               if (oIPJ.iIsGeographic() == 1)
               {            
                  string               strTemp = string.Empty;                  
                  int                  iUnits;
                  double               dConv = 1;
                  
                  oUNI = Geosoft.GXNet.CLTB.Create("units", Geosoft.GXNet.Constant.LTB_TYPE_HEADER, Geosoft.GXNet.Constant.LTB_DELIM_COMMA, string.Empty);
                  if (Geosoft.GXNet.CSYS.IiGlobal("MONTAJ.DEFAULT_UNIT", ref strTemp) == 0) 
                     iUnits = oUNI.iFindKey(strTemp);
                  else 
                     iUnits = 0;

                  oUNI.IGetString(iUnits,0, ref strUnits);
                  dConv = oUNI.rGetReal(iUnits,oUNI.iFindField("Factor"));

                  dResolution *= 110500.0 / dConv;

                  string strFormat = dResolution.ToString("g4");
                  try
                  {
                     dResolution = Convert.ToDouble(strFormat);
                  } 
                  catch {}
               }         
               return dResolution.ToString() + " " + strUnits;
            } 
            catch
            {
            }
            finally
            {
               if (oIPJ != null) oIPJ.Dispose();
               if (oUNI != null) oUNI.Dispose();
            }
         }         
         return base.ConvertTo (context, culture, value, destinationType);
      }
	}
}
