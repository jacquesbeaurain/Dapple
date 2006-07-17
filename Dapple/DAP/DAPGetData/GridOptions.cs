using System;
using System.Text;
using System.Drawing.Design;
using System.ComponentModel;
using Geosoft.PropertyHelpers;
using Geosoft.GXNet;
using Geosoft.Dap.Common;
using Geosoft.DotNetTools.FileDialogs;

namespace Geosoft.GX.DAPGetData
{
	/// <summary>
	/// List of options for grids
	/// </summary>
	[TypeConverter(typeof(PropertySorter))]
	public class GridOptions : DatasetOptions
	{
      #region Enums
      public enum DownloadOptions
      {
         [Description("Reproject")]                   Reproject,
         [Description("Reproject And Resample")]      ReprojectAndResample,
         [Description("Save In Native Projection")]   SaveInNativeProjection
      }

      public enum DisplayOptions
      {
         [Description("Colour Image")]                   ColourImage,
         [Description("Shaded Colour Image")]            ShadedColourImage,
         [Description("Download Only Do Not Display")]   DownloadOnlyDoNotDisplay
      }
      #endregion

      #region Member Variables
      protected string           m_strFileName;
      protected DownloadOptions  m_eDownloadOptions;
      protected DisplayOptions   m_eDisplayOptions;
      protected double           m_dResolution;

      #endregion

      #region Properties
      [PropertyOrder(11)]
      [Category("Data"), Description("Filename to save dataset as."), EditorAttribute("Geosoft.GX.DAPGetData.SaveFileEditor",typeof(UITypeEditor)),FileDialogFilterAttribute(FileDialogFilterAttribute.FilterType.Grid)]
      public string Filename
      {
         get { return m_strFileName; }
         set { m_strFileName = value; }
      }

      [PropertyOrder(20)]
      [TypeConverter(typeof(EnumDescConverter))]
      [Category("Options"), Description("Download options")]
      public DownloadOptions Download
      {
         get { return m_eDownloadOptions; }
         set { m_eDownloadOptions = value; }
      }

      [PropertyOrder(21)]
      [TypeConverter(typeof(EnumDescConverter))]
      [Category("Options"), Description("Display options")] 
      public DisplayOptions Display
      {
         get { return m_eDisplayOptions; }
         set { m_eDisplayOptions = value; }
      }

      [PropertyOrder(30)]
      [TypeConverter(typeof(ResolutionConverter))]
      [Category("Format"), Description("Resolution to extract dataset at."), Editor("Geosoft.GX.DAPGetData.ResolutionExpressionEditor",typeof(UITypeEditor))]
      public double Resolution
      {
         get { return m_dResolution; }
         set { m_dResolution = value; }
      }
      #endregion

      #region Constructor
		public GridOptions(string strServerName, string strTitle, string strUrl) : base(strServerName, strTitle, strUrl)
		{			
		}
      #endregion

      /// <summary>
      /// Download a grid dataset
      /// </summary>
      public override void DownloadDataset()
      {
         CDSEL          hDSEL = null;
         CIPJ           hIPJ = null;
         CDAP           hDAP = null;
         CEMAP          hEMAP = null;
         bool           bReproject = false;

         // --- get the client we are running ---

         base.DownloadDataset();

         
         // --- disable interactive gx's ---

         CSYS.SetInteractive(0);


         CSYS.Progress(1);
         CSYS.ProgName("Displaying grid " + Name + " ...", 0);

         bReproject = (Download == DownloadOptions.ReprojectAndResample || Download == DownloadOptions.Reproject);
         hDSEL = SetExtractionBoundingBox(bReproject);            

         if (Resolution > 0)
         {
            if (Download == DownloadOptions.ReprojectAndResample)            
               hDSEL.SelectResolution(Resolution, Geosoft.GXNet.Constant.GS_TRUE);
            else
               hDSEL.SelectResolution(Resolution, Geosoft.GXNet.Constant.GS_FALSE);
         }

         hDAP = CDAP.Create(ExtractUrl, "");
         hDAP.RequestGridData(ServerName, hDSEL, Filename);

         if (Display == DisplayOptions.ColourImage || Display == DisplayOptions.ShadedColourImage)
         {
            if (m_eClient == ClientType.OASIS_MONTAJ)
            {
               string sGX;
               if (Display == DisplayOptions.ColourImage)
               {
                  sGX = "gridimg1";
               } 
               else 
               {
                  sGX = "gridimgs";
               }
                  
               if (GetDapData.Instance.OMExtents != null) 
               {
                  CSYS.SetInt(sGX,"NEW",0);
               }
               else          
               {
                  // --- create a new map ---

                  CSYS.SetInt(sGX,"NEW",1);
                  CSYS.SetString("DEFMAP","NEWMAP",System.IO.Path.ChangeExtension(Filename, ".map"));
               }  
               CSYS.SetString(sGX,"GRID",Filename);
               CSYS.SetString(sGX,"ZONE","6");
               CSYS.iRunGX(sGX);               
               

               // --- Force redraw of map ---
            
               if (CEMAP.iHaveCurrent() == 1) 
               {
                  hEMAP = CEMAP.Current();
                  hEMAP.Redraw();
               }

               CPROJ.iAddDocument(Filename, "Grid", 0);
            } 
            else if (m_eClient == ClientType.ARCGIS)
            {
               string strGrid;

               strGrid = System.IO.Path.ChangeExtension(Filename, ".grd");               
               CARCMAP.LoadRaster(strGrid);
            }
            else if (m_eClient == ClientType.MAPINFO)
            {               
               if (Display != DisplayOptions.DownloadOnlyDoNotDisplay)
               {
                  string sGX;
                  if (Display == DisplayOptions.ColourImage)
                  {
                     sGX = "MIDISPGRID";
                  } 
                  else 
                  {
                     sGX = "MIDISPSHADE";
                  }
                  
                  CSYS.SetString(sGX,"GRID",Filename);
                  CSYS.SetString(sGX,"ZONE","0");
                  CSYS.iRunGX(sGX);     
                  
                  string strMap = string.Empty;

                  CSTR.IFileNamePart(Filename, ref strMap, GXNet.Constant.STR_FILE_PART_NAME);
                  CSTR.IFileExt(strMap, "map", ref strMap, GXNet.Constant.FILE_EXT_FORCE);

                  CEMAP.UnLoad(strMap);

                  WriteToMapInfoFileList(strMap);
               }
            }
         }
         CSYS.Progress(0);

         if (hDSEL != null)   hDSEL.Dispose();
         if (hDAP != null)    hDAP.Dispose();
         if (hIPJ != null)    hIPJ.Dispose();
         if (hEMAP != null)   hEMAP.Dispose();
      }
	}
}
