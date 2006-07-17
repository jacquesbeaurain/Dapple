using System;
using System.Text;
using System.Drawing.Design;
using System.ComponentModel;
using Geosoft.PropertyHelpers;
using Geosoft.GXNet;
using Geosoft.Dap.Common;

namespace Geosoft.GX.DAPGetData
{
	/// <summary>
	/// List of options for SPF data type
	/// </summary>
	[TypeConverter(typeof(PropertySorter))]
	public class SPFOptions : DatasetOptions
	{      
      #region Member Variables
      protected string           m_strGroupName;
      #endregion

      #region Properties
      [PropertyOrder(20)]
      [Category("Format"), Description("Name of the map group to add this dataset into.")]
      public string GroupName
      {
         get { return m_strGroupName; }
         set { m_strGroupName = value; }
      }
      #endregion

      #region Constructor
		public SPFOptions(string strServerName, string strTitle, string strUrl) : base(strServerName, strTitle, strUrl)
		{			
		}
      #endregion

      /// <summary>
      /// Download spf dataset
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
            // --- format the map name ---

            string   strMapName = GroupName.Trim();            

            strMapName = System.Text.RegularExpressions.Regex.Replace(strMapName, @"[^\w]", @"_");


            base.DownloadDataset();
         
         
            // --- disable interactive gx's ---

            CSYS.SetInteractive(0);

            CSYS.Progress(1);
            CSYS.ProgName("Displaying map " + Name + " ...", 0);

            hDSEL = SetExtractionBoundingBox(false);

            if (m_eClient != ClientType.MAPINFO)
            {         
               // --- Create a new map ---
         
               if (GetDapData.Instance.OMExtents == null || m_eClient == ClientType.ARCGIS)
               {                    
                  CSBF  hMapSBF = null;
                  CBF   hMapBF = null;               


                  // --- Create the new map ---

                  if (m_eClient == ClientType.ARCGIS)
                     CSYS.ITempFileExt("map", ref szMapFile);
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
            }

            // --- Open a connection to DDS ---

            hDAP = CDAP.Create(ExtractUrl,"Retrieve requested data.");


            // --- Get the data ---

            if (m_eClient == ClientType.ARCGIS) 
            {  
               String         strSHPList = string.Empty;
               Int32          iNumSHPFiles = 0;

               hDAP.RequestSPFDataAsSHP(ServerName, hDSEL, GroupName, ref iNumSHPFiles, ref strSHPList);

               // --- Load the shapefiles if there is one ---         

               CARCMAP.iLoadSPF(strSHPList, iNumSHPFiles);
            }  
            else if (m_eClient == ClientType.OASIS_MONTAJ)
            {
               String         szGDBList = string.Empty;
               String         []szGDB;
               CEDB           hEDB;
               Int32          iIndex;
               Int32          iNumDatabases = 0;

               hDAP.RequestSPFData(ServerName, hDSEL, hMAP, GroupName, ref iNumDatabases, ref szGDBList);

   
               // --- Unlock the map ---

               hEMAP.UnLock();

         
               // --- Force redraw of map ---
         
               if (CEMAP.iHaveCurrent() == 1) 
               {
                  hEMAP = CEMAP.Current();
                  hEMAP.Redraw();
               }


               szGDB = szGDBList.Split('|');

               // --- Load the database if there is one ---         

               for (iIndex = 0; iIndex < szGDB.Length; iIndex++) 
               {
                  if (System.IO.File.Exists(szGDB[iIndex])) 
                  {
                     hEDB = CEDB.Load(szGDB[iIndex]);

                     hEDB.DelLine0();
                     hEDB.LoadAllChans();
                  }
               }
            }
            else if (m_eClient == ClientType.MAPINFO)
            {
               string   strTAB;
               string   strWorkDir = string.Empty;

               // --- Create a new database name ---

               CSYS.GtString("MAPINFO_DAP_CLIENT","WORKING_DIR", ref strWorkDir);
            
               strTAB = System.IO.Path.Combine(strWorkDir, strMapName + ".tab");            
               hDAP.RequestSPFDataAsTAB(ServerName, hDSEL, strTAB);

               WriteToMapInfoFileList(strTAB);
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
