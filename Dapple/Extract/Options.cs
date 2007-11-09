using System;
using System.Collections.Generic;
using System.Text;

namespace Dapple.Extract
{
   public class Options
   {
      public class Client
      {
         public enum ClientType
         {
            OasisMontaj,
            ArcMAP,
            MapInfo,
            None
         }
      }

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

      public class ArcGIS
      {
         public enum DownloadOptions
         {
            DownloadAndOpen,
            DownloadOnly
         }

         public static string[] DownloadOptionStrings = new string[] { "Download And Open", "Download Only" };
      }

      public class Grid
      {      
         public enum DisplayOptions
         {
            ShadedColourImage,
            ColourImage,
            DoNotDisplay
         }

         public static string[] DisplayOptionStrings = new string[] { "Shaded colour image", "Colour image", "Do not display" };

         public static string[] DownloadOptionStrings = new string[] {"Geosoft default (*.grd)",
                                                                      "Geosoft DOS grid (*.grd)",
                                                                      "Geosoft BYTE grid (*.grd)",
                                                                      "Geosoft SHORT grid (*.grd)",
                                                                      "Geosoft LONG grid (*.grd)",
                                                                      "Geosoft FLOAT grid (*.grd)",
                                                                      "Geosoft Colour grid (*.grd)",
                                                                      "Geosoft Comp default grid (*.grd)",
                                                                      "Geosoft Comp colour grid (*.grd)",
                                                                      "Geosoft Comp BYTE grid (*.grd)",
                                                                      "Geosoft Comp SHORT grid (*.grd)",
                                                                      "Geosoft Comp LONG grid (*.grd)",
                                                                      "Geosoft Comp FLOAT grid (*.grd)",
                                                                      "Geosoft Comp 2 sig fig grid (*.grd)",
                                                                      "Geosoft Comp 3 sig fig grid (*.grd)",
                                                                      "Geosoft Comp 4 sig fig grid (*.grd)",
                                                                      "Geosoft Comp 5 sig fig grid (*.grd)",
                                                                      "Geosoft Comp 6 sig fig grid (*.grd)",
                                                                      "ArcView Binary Raster Grid (*.flt)",
                                                                      "BIL with INI header (*.*)",
                                                                      "ER Mapper (*.ers)",
                                                                      "ER Mapper ECW Compressor (*.ecw)",
                                                                      "ER Mapper RGB Colour (*.ers)",
                                                                      "Geopak (*.grd)",
                                                                      "GXF Compressed (*.gxf)",
                                                                      "GXF Text (*.gxf)",
                                                                      "Landmark Zmap (*.dat)",
                                                                      "ODDF PC (*.*)",
                                                                      "ODDF UNIX (*.*)",
                                                                      "PCIDSK (*.pix)",
                                                                      "Surfer V6 (*.grd)",
                                                                      "Surfer V7 (*.grd)",
                                                                      "Texaco Startrax (*.grd)",
                                                                      "USGS PC (*.*)",
                                                                      "USGS UNIX (*.*)",
                                                                      "World Geoscience (*.h)"};

         public static string[] DownloadOptionExtension = new string[] {".grd",
                                                                        ".grd",
                                                                        ".grd",
                                                                        ".grd",
                                                                        ".grd",
                                                                        ".grd",
                                                                        ".grd",
                                                                        ".grd",
                                                                        ".grd",
                                                                        ".grd",
                                                                        ".grd",
                                                                        ".grd",
                                                                        ".grd",
                                                                        ".grd",
                                                                        ".grd",
                                                                        ".grd",
                                                                        ".grd",
                                                                        ".grd",
                                                                        ".flt",
                                                                        string.Empty,
                                                                        ".ers",
                                                                        ".ecw",
                                                                        ".ers",
                                                                        ".grd",
                                                                        ".gxf",
                                                                        ".dat",
                                                                        string.Empty,
                                                                        string.Empty,
                                                                        ".pix",
                                                                        ".grd",
                                                                        ".grd",
                                                                        ".grd",
                                                                        string.Empty,
                                                                        string.Empty,
                                                                        ".h"};

         public static string[] DownloadOptionQualifier = new string[] { "GRD",
                                                                         "GRD;Type=SHORT;Comp=none",
                                                                         "GRD;Type=Byte",
                                                                         "GRD;Type=Short",
                                                                         "GRD;Type=Long",
                                                                         "GRD;Type=Float",
                                                                         "GRD;Type=Color",
                                                                         "GRD;Comp=Size",
                                                                         "GRD;Type=color;Comp=Size",
                                                                         "GRD;Type=Byte;Comp=Size",
                                                                         "GRD;Type=Short;Comp=Size",
                                                                         "GRD;Type=Long;Comp=Size",
                                                                         "GRD;Type=Float;Comp=Size",
                                                                         "GRD;Comp=Size7",
                                                                         "GRD;Comp=Size10",
                                                                         "GRD;Comp=Size14",
                                                                         "GRD;Comp=Size17",
                                                                         "GRD;Comp=Size20",
                                                                         "ARC",
                                                                         "SAT,T=0",
                                                                         "ERM",
                                                                         "ERM;Type=COMP",
                                                                         "ERM;Type=COLOR",
                                                                         "GPK",
                                                                         "GXF",
                                                                         "GXF;Comp=0",
                                                                         "ZMP",
                                                                         "ODF;Type=PC",
                                                                         "ODF;Type=UNIX",
                                                                         "PCI",
                                                                         "SRF;VER=V6",
                                                                         "SRF;VER=V7",
                                                                         "TXC",
                                                                         "USG;Type=PC",
                                                                         "USG;Type=UNIX",
                                                                         "WGC"};
      }

      public class SectionGrid
      {
         public enum DisplayOptions
         {
            ColourImage,
            DoNotDisplay
         }

         public static string[] DisplayOptionStrings = new string[] { "Colour image", "Do not display" };
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
         
         public static string[] DownloadOptionStrings = new string[] { "PNG", "JPG", "TIFF", "Native", "Default" };
         public static string[] DisplayOptionStrings = new string[] { "Download and display", "Do not display" };
      }

      public class SectionPicture
      {
         public enum DisplayOptions
         {
            ColourImage,
            DoNotDisplay
         }

         public static string[] DisplayOptionStrings = new string[] { "Colour image", "Do not display" };
      }
   }
}
