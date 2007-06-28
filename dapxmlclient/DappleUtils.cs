using System;
using System.Collections.Generic;
using System.Text;

namespace Geosoft.Dap.Common
{
   /// <summary>
   /// Dapple DAP Utilities
   /// </summary>
   public class DappleUtils
   {
      /// <summary>
      /// Resolves resolution in degrees
      /// </summary>
      /// <returns>resolution in degrees</returns>
      public static double GetResolution(Command oDapCommand, DataSet oDataset)
      {
         try {
            System.Xml.XmlDocument oMeta = oDapCommand.GetMetaData(oDataset);
            System.Xml.XmlNode oNodeRes = oMeta.SelectSingleNode("//meta/CLASS/CLASS/ATTRIBUTE[@name='SpatialResolution']");
            if (oNodeRes != null) {
               int dX, dY;

               double dSpatRes = Convert.ToDouble(oNodeRes.Attributes["value"].Value, System.Globalization.CultureInfo.InvariantCulture);
               double dMinX = Convert.ToDouble(oMeta.SelectSingleNode("//meta/CLASS/CLASS/ATTRIBUTE[@name='BoundingMinX']").Attributes["value"].Value, System.Globalization.CultureInfo.InvariantCulture);
               double dMinY = Convert.ToDouble(oMeta.SelectSingleNode("//meta/CLASS/CLASS/ATTRIBUTE[@name='BoundingMinY']").Attributes["value"].Value, System.Globalization.CultureInfo.InvariantCulture);
               double dMaxX = Convert.ToDouble(oMeta.SelectSingleNode("//meta/CLASS/CLASS/ATTRIBUTE[@name='BoundingMaxX']").Attributes["value"].Value, System.Globalization.CultureInfo.InvariantCulture);
               double dMaxY = Convert.ToDouble(oMeta.SelectSingleNode("//meta/CLASS/CLASS/ATTRIBUTE[@name='BoundingMaxY']").Attributes["value"].Value, System.Globalization.CultureInfo.InvariantCulture);

               dX = (int)Math.Round((dMaxX - dMinX) / dSpatRes);
               dY = (int)Math.Round((dMaxY - dMinY) / dSpatRes);

               return Math.Min((oDataset.Boundary.MaxX - oDataset.Boundary.MinX) / dX, (oDataset.Boundary.MaxY - oDataset.Boundary.MinY) / dY);
            }
         } catch {
            return 0.0;
         }
         return 0.0;
      }


      /// <summary>
      /// Calculate the default number of levels for this dataset
      /// </summary>
      /// <returns></returns>
      public static int Levels(Command oDapCommand, DataSet oDataset)
      {
         int iLevels = 15;

         // Determine the needed levels (function of tile size and resolution if available)
         double dRes = GetResolution(oDapCommand, oDataset);
         if (dRes > 0) {
            double dTileSize = LevelZeroTileSize(oDataset);
            iLevels = 1;
            while ((double)dTileSize / Convert.ToDouble(256) > dRes / 4.0) {
               iLevels++;
               dTileSize /= 2;
            }
         }
         return iLevels;
      }

      /// <summary>
      /// Calculate the default level zero tile size
      /// </summary>
      /// <param name="oDataset"></param>
      /// <returns>default level zero tile size</returns>
      public static double LevelZeroTileSize(DataSet oDataset)
      {
         // Round to ceiling of four decimals (>~ 10 meter resolution)
         // Empirically determined as pretty good tile size choice for small data sets
         double dLevelZero = (Math.Ceiling(10000.0 * Math.Max(oDataset.Boundary.MaxY - oDataset.Boundary.MinY, oDataset.Boundary.MaxX - oDataset.Boundary.MinX)) / 10000.0);
         
         // Optimum tile alignment when this is 180/(2^n), the first value is 180/2^3
         double dRet = 22.5;
         while (dLevelZero < dRet)
            dRet /= 2;

         return dRet;
      }
   }
}
