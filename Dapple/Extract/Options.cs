using System;
using System.Collections.Generic;
using System.Text;

namespace Dapple.Extract
{
   class Options
   {
      public class Document
      {
         public enum DownloadOptions
         {
            DownloadAndOpen,
            DownloadOnly
         }

         public static string[] DownloadOptionStrings = new string[] { "Download And Open", "Download Only" };
      }

      public class GIS
      {
         public enum OMDownloadOptions
         {
            ImportIntoMap,
            SaveAsSHPImportIntoMap,
            SaveAsTABImportIntoMap,
            SaveAsSHP,
            SaveAsTAB
         }

         public enum ArcDownloadOptions
         {
            SaveAsSHP
         }

         public enum MIDownloadOptions
         {
            SaveAsTAB
         }

         public static string[] OMDownloadOptionStrings = new string[] { "Import into map", "Save as shape file and import into map", "Save as TAB file and import into map", "Save as shape file", "Save as TAB file" };
         public static string[] ArcDownloadOptionStrings = new string[] { "Save as shape file" };
         public static string[] MIDownloadOptionStrings = new string[] { "Save as TAB file" };
      }

      public class Grid
      {
         public enum DownloadOptions
         {
            Reproject,
            ReprojectResample,
            NativeProjection
         }

         public enum SectionDownloadOptions
         {
            NativeProjection
         }

         public enum DisplayOptions
         {
            ShadedColourImage,
            ColourImage,
            DoNotDisplay
         }

         public static string[] DownloadOptionStrings = new string[] { "Reproject", "Reproject and resample", "Save in native projection" };
         public static string[] SectionDownloadOptionStrings = new string[] { "Save in native projection" };
         public static string[] DisplayOptionStrings = new string[] { "Shaded colour image", "Colour image", "Do not display" };
      }

      public class Picture
      {
         public enum DownloadOptions
         {
            PNG,
            JPG,
            TIFF,
            Native,
            Default
         }

         public enum DisplayOptions
         {
            DownloadAndDisplay,
            DoNotDisplay
         }

         public enum SizeOptions
         {
            TwoHundred,
            FourHundred,
            SixHundred,
            EightHundred,
            OneThousand,
            OneThousandTwoHundred,
            OneThousandSixHundred
         }

         public static string[] DownloadOptionStrings = new string[] { "PNG", "JPG", "TIFF", "Native", "Default" };
         public static string[] DisplayOptionStrings = new string[] { "Download and display", "Do not display" };
         public static string[] SizeOptionStrings = new string[] { "200", "400", "600", "800", "1000", "1200", "1600" };
      }
   }
}
