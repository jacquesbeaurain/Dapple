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
	/// List of options for database
	/// </summary>
	[TypeConverter(typeof(PropertySorter))]
	public class DatabaseOptions : DatasetOptions
	{      
      #region Member Variables
      protected string           m_strFileName;      
      #endregion

      #region Properties
      [PropertyOrder(11)]
      [Category("Data"), Description("Filename to save dataset as."), Editor("Geosoft.GX.DAPGetData.SaveFileEditor",typeof(UITypeEditor)), FileDialogFilter(FileDialogFilterAttribute.FilterType.Database)]
      public string Filename
      {
         get { return m_strFileName; }
         set { m_strFileName = value; }
      }
      #endregion

      #region Constructor
      /// <summary>
      /// Default constructor
      /// </summary>
		public DatabaseOptions(string strServerName, string strTitle, string strUrl) : base(strServerName, strTitle, strUrl)
		{			
		}
      #endregion
      
      /// <summary>
      /// Download the database
      /// </summary>
      public override void DownloadDataset()
      {
         CDSEL hDSEL = null;
         CIPJ  hIPJ = null;
         CEDB  hEDB = null;
         CDAP  hDAP = null;
         CDB   hDB = null;
         
         Int32 iDatabaseClassToken = GXNet.Constant.H_META_INVALID_TOKEN;
         Int32 iLinesAttribToken = GXNet.Constant.H_META_INVALID_TOKEN;
         Int32 iChannelsAttribToken = GXNet.Constant.H_META_INVALID_TOKEN;

         Int32    iLines = 0;
         Int32    iChannels = 0;
         Int32    iBlobs = 0;

         base.DownloadDataset();
         
         
         // --- disable interactive gx's ---

         CSYS.SetInteractive(0);


         CSYS.Progress(1);
         CSYS.ProgName("Displaying database " + Name + " ...", 0);         

         hDSEL = SetExtractionBoundingBox(false);

         if (CEDB.iLoaded(Filename) == 1) CEDB.UnLoad(Filename);

         
         iDatabaseClassToken = m_oMETA.ResolveUMN("CLASS:/Geosoft/Data/Database");
         iLinesAttribToken = m_oMETA.ResolveUMN("ATTRIB:/Geosoft/Data/Database/Lines");
         iChannelsAttribToken = m_oMETA.ResolveUMN("ATTRIB:/Geosoft/Data/Database/Channels");


         if (iDatabaseClassToken != GXNet.Constant.H_META_INVALID_TOKEN && iLinesAttribToken != GXNet.Constant.H_META_INVALID_TOKEN)
            m_oMETA.GetAttribInt(iDatabaseClassToken, iLinesAttribToken, ref iLines);

         if (iDatabaseClassToken != GXNet.Constant.H_META_INVALID_TOKEN && iChannelsAttribToken != GXNet.Constant.H_META_INVALID_TOKEN)
            m_oMETA.GetAttribInt(iDatabaseClassToken, iChannelsAttribToken, ref iChannels);

         
         if (iLines <= 0) iLines = 200;
         if (iChannels <= 0) iChannels = 50;
         iBlobs = iLines + iChannels + 20;

         CDB.Create(Filename, iLines + 5, iChannels + 5, iBlobs, 10, 100, "SUPER", "");

         if (m_eClient != ClientType.ARCGIS)
         {
            hEDB = CEDB.Load(Filename);
            hDB = hEDB.Lock();
         } 
         else 
         {
            hDB = CDB.Open(Filename, "SUPER", "");
         }

         hDAP = CDAP.Create(ExtractUrl, "Retrieve requested data.");
         hDAP.RequestDatabaseData(ServerName, hDSEL, hDB);

         if (m_eClient != ClientType.ARCGIS)
         {
            hEDB.UnLock();
            hEDB.DelLine0();
            hEDB.LoadAllChans();
         } 
         else 
         {
            Int32          iLine = GXNet.Constant.NULLSYMB;
            CLST           hLST = null;
            CLST           hFileLST = null;
            String         hStrSelLine = "";
            String         hStrSHP = "";
            string         strSHPFile;
            
            // --- This gets rid of L0 ---
            
            iLine = hDB.FindSymb("L0",Geosoft.GXNet.Constant.DB_SYMB_LINE);
            if (hDB.iIsLineEmpty(iLine) == 0) 
            {
               hDB.LockSymb(iLine, Geosoft.GXNet.Constant.DB_LOCK_READWRITE, Geosoft.GXNet.Constant.DB_WAIT_INFINITY);
               hDB.DeleteSymb(iLine);
            }
            
            // --- Export all to shape ---
                
            hDB.Select("",Geosoft.GXNet.Constant.DB_LINE_SELECT_INCLUDE);
            hLST = CLST.Create(Geosoft.GXNet.Constant.STR_FILE);
            hDB.SelectedLineLST(hLST);
            hLST.GtItem(0, 0, ref hStrSelLine);

            hFileLST = CLST.Create(Geosoft.GXNet.Constant.STR_FILE);
            
            strSHPFile = System.IO.Path.ChangeExtension(Filename, ".shp");           

            CDU.ExportSHP(hDB, hStrSelLine.ToString(), null, Geosoft.GXNet.Constant.DU_CHANNELS_ALL, 0, strSHPFile, hFileLST);

            // --- Display shape file(s) ---

            for (Int32 i = 0; i < hFileLST.iSize(); i++)
            {
               hFileLST.GtItem(0, i, ref hStrSHP);
               CARCMAP.LoadShape(hStrSHP.ToString(), "", "");
            }

            if (hLST != null)       hLST.Dispose();
            if (hFileLST != null)   hFileLST.Dispose();
         }

         CSYS.Progress(0);

         if (hDSEL != null)   hDSEL.Dispose();
         if (hEDB != null)    hEDB.Dispose();
         if (hDB != null)     hDB.Dispose();
         if (hDAP != null)    hDAP.Dispose();
         if (hIPJ != null)    hIPJ.Dispose();
      }
	}
}
