using System;
using System.Drawing.Design;
using System.ComponentModel;
using Geosoft.PropertyHelpers;
using Geosoft.GXNet;
using Geosoft.Dap.Common;

namespace Geosoft.GX.DAPGetData
{
	/// <summary>
	/// List of options for maps
	/// </summary>
	[TypeConverter(typeof(PropertySorter))]
	public class MapOptions : DatasetOptions
	{      
      #region Member Variables
      protected string           m_strGroupName;
      protected double           m_dResolution;
      #endregion

      #region Properties
      [PropertyOrder(20)]
      [Category("Format"), Description("Name of the map group to add this dataset into.")]
      public string GroupName
      {
         get { return m_strGroupName; }
         set { m_strGroupName = value; }
      }

      [PropertyOrder(21)]
      [TypeConverter(typeof(ResolutionConverter))]
      [Category("Format"), Description("Resolution to extract dataset at."), Editor("Geosoft.GX.DAPGetData.ResolutionExpressionEditor",typeof(UITypeEditor))]
      public double Resolution
      {
         get { return m_dResolution; }
         set { m_dResolution = value; }
      }
      #endregion

      #region Constructor
		public MapOptions(string strServerName, string strTitle, string strUrl) : base(strServerName, strTitle, strUrl)
		{			
		}
      #endregion

      /// <summary>
      /// Download map dataset
      /// </summary>
      public override void DownloadDataset()
      {
         CDSEL          hDSEL = null;
         CIPJ           hIPJ = null;
         CDAP           hDAP = null;
         CMAP           hMAP = null;
         CEMAP          hEMAP = null;
         
         String         szMapFile = "";         


         try 
         {
         
            base.DownloadDataset();

         
            // --- disable interactive gx's ---

            CSYS.SetInteractive(0);

            CSYS.Progress(1);
            CSYS.ProgName("Displaying map " + Name + " ...", 0);

            hDSEL = SetExtractionBoundingBox(false);

            if (Resolution >= 0)
               hDSEL.SelectResolution(Resolution, Geosoft.GXNet.Constant.GS_FALSE);


            // --- Create a new map ---
         
            if (GetDapData.Instance.OMExtents == null || m_eClient == ClientType.ARCGIS || m_eClient == ClientType.MAPINFO)
            {                    
               CSBF     hMapSBF = null;
               CBF      hMapBF = null;

               // --- format the map name ---

               string   strMapName = GroupName.Trim();            

               strMapName = System.Text.RegularExpressions.Regex.Replace(strMapName, @"[^\w]", @"_");


               // --- Create the new map ---

               if (m_eClient == ClientType.ARCGIS)
                  CSYS.ITempFileExt("map", ref szMapFile);
               else if (m_eClient == ClientType.MAPINFO)
               {
                  string strWorkDir  = string.Empty;

                  CSYS.GtString("MAPINFO_DAP_CLIENT", "WORKING_DIR", ref strWorkDir);

                  szMapFile = System.IO.Path.Combine(strWorkDir, strMapName + ".map");
               }
               else 
               {
                  CSYS.IGetDirectory(Geosoft.GXNet.Constant.SYS_DIR_LOCAL, ref szMapFile);
                  szMapFile += strMapName;
                  szMapFile = System.IO.Path.ChangeExtension(szMapFile, ".map");
               }

               CSYS.SetString("DEFMAP", "MAP", szMapFile);
            
             
               // --- Set the projection and range (make view about 5% larger in each directio to prevent clipping) ---
   
               hMapSBF = CSBF.hGetSYS();
               hMapBF = CBF.CreateSBF(hMapSBF,"_xyrange.ipj",Geosoft.GXNet.Constant.BF_READWRITE_NEW);
            
               CreateMap(hMapBF);
            
               hMapBF.Dispose();
               hMapSBF.Dispose();
            
               // --- Create the new map ---
             
               if (CSYS.iRunGX("defmap.gx") != 0)
                  CSYS.Cancel();                           
            } 
                  
            // --- Lock the map ---

            hEMAP = CEMAP.Current();
            hMAP = hEMAP.Lock();


            // --- Open a connection to DDS ---

            hDAP = CDAP.Create(ExtractUrl,"Retrieve requested data.");


            // --- Get the data ---

            hDAP.RequestMapData(ServerName,hDSEL,hMAP,GroupName);
      

            if (m_eClient == ClientType.MAPINFO)
            {
               hMAP.ResizeAll();
            }

            // --- Unlock the map ---

            hEMAP.UnLock();
         
            if (m_eClient == ClientType.OASIS_MONTAJ)
            {
               // --- Force redraw of map ---
         
               if (CEMAP.iHaveCurrent() == 1) 
               {
                  hEMAP = CEMAP.Current();
                  hEMAP.Redraw();
               }
            } 
            else if (m_eClient == ClientType.ARCGIS)
            {
               CLST                       hFileList = null;
               string                     strSHP;
               string                     strGroupName;
               String                     szSHPFile = "";

               CEMAP.UnLoad(szMapFile);

               strGroupName = System.Text.RegularExpressions.Regex.Replace(GroupName, @"[^\w]", "_");
            

               CARCSYS.IGetBrowseLoc(ref szSHPFile);
               szSHPFile += strGroupName;
               strSHP = System.IO.Path.ChangeExtension(szSHPFile, ".shp");
            
               if (System.IO.File.Exists(strSHP)) 
               {
                  if (System.Windows.Forms.DialogResult.Yes == System.Windows.Forms.MessageBox.Show("Shapefile exists", "\"" + strSHP + "\" exists. Overwrite?", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Question))
                  {
                     System.IO.File.Delete(strSHP);
                  } 
                  else 
                  {
                     Geosoft.DotNetTools.FileDialogs.ArcViewFileDialog hDialog = new Geosoft.DotNetTools.FileDialogs.ArcViewFileDialog(true, strSHP);
                     if (!hDialog.Show())
                        CSYS.Cancel();
                     strSHP = hDialog.FileName;
                  }
               }

               hFileList = CLST.Create(Geosoft.GXNet.Constant.STR_FILE);

               CARCMAP.MapViewToShape(szMapFile, "Data", strSHP, hFileList);
               System.IO.File.Delete(szMapFile);
               szMapFile += ".gm";
               System.IO.File.Delete(szMapFile);


               // --- Iterate over the list of names ---
	
               for (Int32 i = 0; i < hFileList.iSize(); i++)
               {
                  hFileList.GtItem(0, i, ref szSHPFile);
                  CARCMAP.LoadShape(szSHPFile, "", "");            
               }

               if (hFileList != null) hFileList.Dispose();
            } 
            else if (m_eClient == ClientType.MAPINFO)
            {
               hEMAP.Redraw();
               CEMAP.UnLoad(szMapFile);

               WriteToMapInfoFileList(szMapFile);
            }

            CSYS.Progress(0);
         }
         finally 
         {
            if (hDSEL != null) hDSEL.Dispose();
            if (hDAP != null) hDAP.Dispose();
            if (hIPJ != null) hIPJ.Dispose();
            if (hEMAP != null)   hEMAP.Dispose();
            if (hMAP != null)    hMAP.Dispose();
            if (m_eClient == ClientType.ARCGIS && szMapFile.Length > 0)
               CSYS.iDeleteFile(szMapFile);
         }
      }
	}
}
