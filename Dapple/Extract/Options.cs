using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace Dapple.Extract
{
   internal class Options
   {
      internal class Client
      {
         internal enum ClientType
         {
				[Description("Oasis montaj")]
            OasisMontaj,
				[Description("ArcMap")]
            ArcMAP,
				[Description("MapInfo")]
            MapInfo,
				[Description("the host application")]
            None
         }
      }

      internal class Document
      {
         internal enum DownloadOptions
         {
            DownloadAndOpen,
            DownloadOnly
         }

         internal static string[] DownloadOptionStrings = new string[] { "Download And Open", "Download Only" };
      }

      internal class GIS
      {
         internal enum OMDownloadOptions
         {
            ImportIntoMap,
            SaveAsSHPImportIntoMap,
            SaveAsTABImportIntoMap,
            SaveAsSHP,
            SaveAsTAB
         }

         internal enum ArcDownloadOptions
         {
            SaveAsSHP
         }

         internal enum MIDownloadOptions
         {
            SaveAsTAB
         }

         internal static string[] OMDownloadOptionStrings = new string[] { "Import into map", "Save as ArcGIS Shape file and import into map", "Save as MapInfo Tab file and import into map", "Save as ArcGIS Shape file", "Save as MapInfo tab file" };
         internal static string[] ArcDownloadOptionStrings = new string[] { "Save as ArcGIS Shape file" };
         internal static string[] MIDownloadOptionStrings = new string[] { "Save as MapInfo Tab file" };
      }

      internal class ArcGIS
      {
         internal enum DownloadOptions
         {
            DownloadAndOpen,
            DownloadOnly
         }

         internal static string[] DownloadOptionStrings = new string[] { "Download And Open", "Download Only" };
      }

      internal class Grid
      {      
         internal enum DisplayOptions
         {
            ShadedColourImage,
            ColourImage,
            DoNotDisplay
         }

         internal static string[] DisplayOptionStrings = new string[] { "Shaded colour image", "Colour image", "Do not display" };

         internal static string[] DownloadOptionStrings = new string[] {"Geosoft default (*.grd)",
                                                                      "Geosoft DOS grid (*.grd)",
                                                                      "Geosoft BYTE grid (*.grd)",
                                                                      "Geosoft SHORT grid (*.grd)",
                                                                      "Geosoft LONG grid (*.grd)",//5
                                                                      "Geosoft FLOAT grid (*.grd)",
                                                                      "Geosoft Colour grid (*.grd)",
                                                                      "Geosoft Comp default grid (*.grd)",
                                                                      "Geosoft Comp colour grid (*.grd)",
                                                                      "Geosoft Comp BYTE grid (*.grd)",//10
                                                                      "Geosoft Comp SHORT grid (*.grd)",
                                                                      "Geosoft Comp LONG grid (*.grd)",
                                                                      "Geosoft Comp FLOAT grid (*.grd)",
                                                                      "Geosoft Comp 2 sig fig grid (*.grd)",
                                                                      "Geosoft Comp 3 sig fig grid (*.grd)",//15
                                                                      "Geosoft Comp 4 sig fig grid (*.grd)",
                                                                      "Geosoft Comp 5 sig fig grid (*.grd)",
                                                                      "Geosoft Comp 6 sig fig grid (*.grd)",
                                                                      "ArcView Binary Raster Grid (*.flt)",
                                                                      "BIL with INI header (*.*)",//20
                                                                      "ER Mapper (*.ers)",
                                                                      "ER Mapper ECW Compressor (*.ecw)",
                                                                      "ER Mapper RGB Colour (*.ers)",
                                                                      "Geopak (*.grd)",
                                                                      "GXF Compressed (*.gxf)",//25
                                                                      "GXF Text (*.gxf)",
                                                                      "Landmark Zmap (*.dat)",
                                                                      "ODDF PC (*.*)",
                                                                      "ODDF UNIX (*.*)",
                                                                      "PCIDSK (*.pix)",//30
                                                                      "Surfer V6 (*.grd)",
                                                                      "Surfer V7 (*.grd)",
                                                                      "Texaco Startrax (*.grd)",
                                                                      "USGS PC (*.*)",
                                                                      "USGS UNIX (*.*)",//35
                                                                      "World Geoscience (*.h)"};

         internal static string[] DownloadOptionExtension = new string[] {".grd",
                                                                        ".grd",
                                                                        ".grd",
                                                                        ".grd",
                                                                        ".grd",//5
                                                                        ".grd",
                                                                        ".grd",
                                                                        ".grd",
                                                                        ".grd",
                                                                        ".grd",//10
                                                                        ".grd",
                                                                        ".grd",
                                                                        ".grd",
                                                                        ".grd",
                                                                        ".grd",//15
                                                                        ".grd",
                                                                        ".grd",
                                                                        ".grd",
                                                                        ".flt",
                                                                        null,//20
                                                                        ".ers",
                                                                        ".ecw",
                                                                        ".ers",
                                                                        ".grd",
                                                                        ".gxf",//25
				                                                            ".gxf",
                                                                        ".dat",
                                                                        null,
                                                                        null,
                                                                        ".pix",
                                                                        ".grd",//30
                                                                        ".grd",
                                                                        ".grd",
                                                                        null,
                                                                        null,//35
                                                                        ".h"};

         internal static string[] DownloadOptionQualifier = new string[] { "GRD",
                                                                         "GRD;Type=SHORT;Comp=none",
                                                                         "GRD;Type=Byte",
                                                                         "GRD;Type=Short",
                                                                         "GRD;Type=Long",//5
                                                                         "GRD;Type=Float",
                                                                         "GRD;Type=Color",
                                                                         "GRD;Comp=Size",
                                                                         "GRD;Type=color;Comp=Size",
                                                                         "GRD;Type=Byte;Comp=Size",//10
                                                                         "GRD;Type=Short;Comp=Size",
                                                                         "GRD;Type=Long;Comp=Size",
                                                                         "GRD;Type=Float;Comp=Size",
                                                                         "GRD;Comp=Size7",
                                                                         "GRD;Comp=Size10",//15
                                                                         "GRD;Comp=Size14",
                                                                         "GRD;Comp=Size17",
                                                                         "GRD;Comp=Size20",
                                                                         "ARC",
                                                                         "SAT,T=0",//20
                                                                         "ERM",
                                                                         "ERM;Type=COMP",
                                                                         "ERM;Type=COLOR",
                                                                         "GPK",
                                                                         "GXF",//25
                                                                         "GXF;Comp=0",
                                                                         "ZMP",
                                                                         "ODF;Type=PC",
                                                                         "ODF;Type=UNIX",
                                                                         "PCI",//30
                                                                         "SRF;VER=V6",
                                                                         "SRF;VER=V7",
                                                                         "TXC",
                                                                         "USG;Type=PC",
                                                                         "USG;Type=UNIX",//35
                                                                         "WGC"};
      }

      internal class SectionGrid
      {
         internal enum DisplayOptions
         {
            ColourImage,
            DoNotDisplay
         }

         internal static string[] DisplayOptionStrings = new string[] { "Colour image", "Do not display" };
      }

      internal class Picture
      {
         internal enum DownloadOptions
         {
            PNG,
            JPG,
            TIFF,
				ECW,
            Native,
            Default
         }

         internal enum DisplayOptions
         {
            DownloadAndDisplay,
            DoNotDisplay
         }
         
         internal static string[] DownloadOptionStrings = new string[] { "PNG", "JPG", "TIFF", "ECW", "Native", "Default" };
         internal static string[] DisplayOptionStrings = new string[] { "Download and display", "Do not display" };
      }

      internal class SectionPicture
      {
         internal enum DisplayOptions
         {
            ColourImage,
            DoNotDisplay
         }

         internal static string[] DisplayOptionStrings = new string[] { "Colour image", "Do not display" };
      }
   }
}
