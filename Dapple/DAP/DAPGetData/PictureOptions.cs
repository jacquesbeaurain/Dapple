using System;
using System.Drawing.Design;
using System.ComponentModel;
using Geosoft.PropertyHelpers;
using Geosoft.GXNet;
using Geosoft.Dap.Common;
using Geosoft.DotNetTools.FileDialogs;

namespace Geosoft.GX.DAPGetData
{
	/// <summary>
	/// List of options for pictures
	/// </summary>
	[TypeConverter(typeof(PropertySorter))]
	public class PictureOptions : DatasetOptions
	{
      #region Enums
      public enum Qualities
      {
         [Description("High Quality")]             HighQuality,
         [Description("Medium Quality")]           MediumQuality,
         [Description("Low Quality")]              LowQuality,
         [Description("Save In Native Format")]    SaveInNativeFormat,
         [Description("Save In Default Format")]   SaveInDefaultFormat
      }

      public enum DisplayOptions
      {
         [Description("Download And Display")]           DownloadAndDisplay,
         [Description("Download Only Do Not Display")]   DownloadOnlyDoNotDisplay
      }
      #endregion

      #region Member Variables
      protected string           m_strFileName;
      protected Qualities        m_eQuality;
      protected DisplayOptions   m_eDisplayOptions;
      protected double           m_dResolution;

      #endregion

      #region Properties
      [PropertyOrder(11)]
      [Category("Data"), Description("Set the filename to save this dataset as."), EditorAttribute("Geosoft.GX.DAPGetData.SaveFileEditor",typeof(UITypeEditor)),FileDialogFilterAttribute(FileDialogFilterAttribute.FilterType.AllFiles)]
      public string Filename
      {
         get { return m_strFileName; }
         set { m_strFileName = value; }
      }

      [PropertyOrder(20)]
      [TypeConverter(typeof(EnumDescConverter))]
      [Category("Options"), Description("Set the display options.")]
      public DisplayOptions Display
      {
         get { return m_eDisplayOptions; }
         set { m_eDisplayOptions = value; }
      }

      [PropertyOrder(21)]
      [TypeConverter(typeof(EnumDescConverter))]
      [Category("Options"), Description("Specify the quality of the image to extract.")]
      public Qualities Quality
      {
         get { return m_eQuality; }
         set { m_eQuality = value; }
      }      

      [PropertyOrder(30)]
      [TypeConverter(typeof(ResolutionConverter))]
      [Category("Format"), Description("Specify the resolution to extract the picture at."), Editor("Geosoft.GX.DAPGetData.ResolutionExpressionEditor",typeof(UITypeEditor))]
      public double Resolution
      {
         get { return m_dResolution; }
         set { m_dResolution = value; }
      }
      #endregion

      #region Constructor
		public PictureOptions(string strServerName, string strTitle, string strUrl) : base(strServerName, strTitle, strUrl)
		{		
		}
      #endregion

      public override void DownloadDataset()
      {
         CDSEL          hDSEL = null;
         CIPJ           hIPJ = null;
         CDAP           hDAP = null;
         CEMAP          hEMAP = null;
         
         Int32          iQuality;
         String         szGridFileName = "";
         
         // --- get the client we are running ---

         base.DownloadDataset();

         
         // --- disable interactive gx's ---

         CSYS.SetInteractive(0);
         

         CSYS.Progress(1);
         CSYS.ProgName("Displaying picture " + Name + " ...", 0);

         hDSEL = SetExtractionBoundingBox(false);

         if (Resolution >= 0)
            hDSEL.SelectResolution(Resolution, Geosoft.GXNet.Constant.GS_FALSE);

         switch (Quality)
         {
            case Qualities.HighQuality:
               iQuality = 1;
               break;
            case Qualities.MediumQuality:
               iQuality = 2;
               break;
            case Qualities.LowQuality:
               iQuality = 3;
               break;
            case Qualities.SaveInDefaultFormat:
               iQuality = 0;
               break;
            case Qualities.SaveInNativeFormat:
               iQuality = 4;
               break;
            default:
               iQuality = 4;
               break;
         }
         hDSEL.PictureQuality(iQuality);
                  
         
         // --- create dap connection ---

         hDAP = CDAP.Create(ExtractUrl, "");

         szGridFileName += Filename;
         hDAP.RequestPictureData(ServerName, hDSEL, ref szGridFileName); 

         if (Display == DisplayOptions.DownloadAndDisplay)
         {
            if (m_eClient == ClientType.ARCGIS)
               CARCMAP.LoadRaster(szGridFileName);
            else 
            {
               CIPJ  hObjIPJ = CIPJ.Create();
               CIMG  hOrigIMG = CIMG.CreateFile(GXNet.Constant.GS_DOUBLE, szGridFileName, GXNet.Constant.IMG_FILE_READONLY);
               hOrigIMG.GetIPJ(hObjIPJ);
               hOrigIMG.Dispose();

               if (GetDapData.Instance.OMExtents != null) 
               {
                  CIPJ hMapIPJ = CIPJ.Create();
                  hMapIPJ.SetGXF(String.Empty, GetDapData.Instance.OMExtents.CoordinateSystem.Datum, GetDapData.Instance.OMExtents.CoordinateSystem.Method, GetDapData.Instance.OMExtents.CoordinateSystem.Units, GetDapData.Instance.OMExtents.CoordinateSystem.LocalDatum);

                  if (hMapIPJ.iSame(hObjIPJ) == 0)
                  {
                     if (System.Windows.Forms.MessageBox.Show("The raster dataset is in a different projection than the current map. \n\rIn order for the data to be displayed correctly the dataset must be reprojected. \n\r\n\rDo you wish to reproject the data?","Raster coordinate system different than current map.", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                     {
                        string   strNativeGrid = string.Empty;

                        if (System.IO.Path.GetDirectoryName(szGridFileName) == string.Empty) 
                        {
                           if (m_eClient == ClientType.MAPINFO)
                              Geosoft.GXNet.CSYS.GtString("MAPINFO_DAP_CLIENT", "WORKING_DIR", ref strNativeGrid);
                           else
                              Geosoft.GXNet.CSYS.IGetPath(Geosoft.GXNet.Constant.SYS_PATH_LOCAL, ref strNativeGrid);

                           szGridFileName = System.IO.Path.Combine(strNativeGrid, szGridFileName);
                        } 
                        strNativeGrid = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(szGridFileName), "orig_" + System.IO.Path.GetFileName(szGridFileName));
                  
                        System.IO.File.Delete(strNativeGrid);
                        System.IO.File.Copy(Constant.StripQualifiers(szGridFileName), Constant.StripQualifiers(strNativeGrid), true);
                        System.IO.File.Delete(Constant.StripQualifiers(szGridFileName));
                        
                        if (System.IO.File.Exists(Constant.StripQualifiers(szGridFileName) + ".gi")) 
                        {
                           System.IO.File.Copy(Constant.StripQualifiers(szGridFileName) + ".gi", Constant.StripQualifiers(strNativeGrid) + ".gi", true);
                           System.IO.File.Delete(Constant.StripQualifiers(szGridFileName) + ".gi");
                        }


                        // --- special output qualifier for er mapper files ---

                        string   strQual = string.Empty;
                        Int32    iIndex = strNativeGrid.LastIndexOf("(");

                        if (iIndex != -1)
                        {
                           strQual = strNativeGrid.Substring(iIndex + 1);

                           iIndex = strQual.LastIndexOf(")");
                           strQual = strQual.Substring(0, iIndex);
                        }
                  
                        if (String.Compare(strQual, "ecw", true) == 0)
                        {
                           iIndex = szGridFileName.LastIndexOf("(");
                           szGridFileName = szGridFileName.Substring(0, iIndex + 1);
                           szGridFileName += "ERM;type=COMP;T=2)";

                           iIndex = strNativeGrid.LastIndexOf("(");
                           strNativeGrid = strNativeGrid.Substring(0, iIndex + 1);
                           strNativeGrid += "ECW)";
                        } 
                        else
                        {
                           szGridFileName = System.IO.Path.ChangeExtension(szGridFileName, "TIF");
                           szGridFileName += "(TIF)";                                                
                        }

                        hOrigIMG = CIMG.CreateFile(GXNet.Constant.GS_DOUBLE, strNativeGrid, GXNet.Constant.IMG_FILE_READONLY);

                        hOrigIMG.CreateProjected(hMapIPJ);

                        CIMG hNewIMG = CIMG.CreateOutFile(GXNet.Constant.GS_DOUBLE, szGridFileName, hOrigIMG);

                        hOrigIMG.Copy(hNewIMG);
                        hNewIMG.SetIPJ(hMapIPJ);

                        hOrigIMG.Dispose();
                        hNewIMG.Dispose();
                        hObjIPJ.Dispose();
                     }
                  }
                  hMapIPJ.Dispose();
               }
            }

            if (m_eClient == ClientType.OASIS_MONTAJ)
            {
               
               if (GetDapData.Instance.OMExtents != null)
               {
                  CSYS.SetInt("GRIDIMG","NEW",0);
               }
               else          
               {
                  CSYS.SetString("DEFMAP", "NEWMAP", System.IO.Path.ChangeExtension(szGridFileName, ".map"));
                  CSYS.SetInt("GRIDIMG","NEW",1);
               }
               CSYS.SetString("GRIDIMG","IMAGE",szGridFileName);
               CSYS.iRunGX("gridimg");
      

               // --- Force redraw of map ---
         
               if (CEMAP.iHaveCurrent() == 1) 
               {
                  hEMAP = CEMAP.Current();
                  hEMAP.Redraw();
               }
            }            
            else if (m_eClient == ClientType.MAPINFO)
            {                       
               CSYS.SetString("MIDISPGRID", "GRID", szGridFileName);
               CSYS.SetString("MIDISPGRID", "ZONE", "0");
               CSYS.iRunGX("MIDISPGRID");

               CEMAP.UnLoad(System.IO.Path.ChangeExtension(szGridFileName, ".map"));
               WriteToMapInfoFileList(System.IO.Path.ChangeExtension(szGridFileName, ".map"));
            }               
         }
         CSYS.Progress(0);

         if (hDSEL != null) hDSEL.Dispose();
         if (hDAP != null) hDAP.Dispose();
         if (hIPJ != null) hIPJ.Dispose();
         if (hEMAP != null)   hEMAP.Dispose();
      }
	}
}
