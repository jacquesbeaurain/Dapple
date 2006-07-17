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
	/// List of options for point data types
	/// </summary>
	[TypeConverter(typeof(PropertySorter))]
	public class PointOptions : DatasetOptions
	{      
      #region Member Variables
      protected string           m_strFileName;      
      #endregion

      #region Properties
      [PropertyOrder(11)]
      [Category("Data"), Description("Set the filename to save this dataset as."),EditorAttribute("Geosoft.GX.DAPGetData.SaveFileEditor",typeof(UITypeEditor)),FileDialogFilterAttribute(FileDialogFilterAttribute.FilterType.Point)]
      public string Filename
      {
         get { return m_strFileName; }
         set 
         { 
            m_strFileName = value; 

            if (m_eClient == ClientType.MAPINFO)
               m_strFileName = System.IO.Path.ChangeExtension(m_strFileName, ".tab");
            else if (m_eClient == ClientType.ARCGIS)
               m_strFileName = System.IO.Path.ChangeExtension(m_strFileName, ".shp");
         }
      }
      #endregion

      #region Constructor
		public PointOptions(string strServerName, string strTitle, string strUrl) : base(strServerName, strTitle, strUrl)
		{			
		}
      #endregion

      /// <summary>
      /// Download a point dataset
      /// </summary>
      public override void DownloadDataset()
      {
         CDSEL          hDSEL = null;
         CIPJ           hIPJ = null;
         CEDB           hEDB = null;
         CDAP           hDAP = null;
         CDB            hDB = null;
         Int32          iSymb;
         string         strGroupName;
         
         // --- get the client we are running ---

         base.DownloadDataset();


         // --- disable interactive gx's ---

         CSYS.SetInteractive(0);

         CSYS.Progress(1);
         CSYS.ProgName("Displaying database " + Name + " ...", 0);

         hDSEL = SetExtractionBoundingBox(false);
        
 
         // --- create dap connection ---

         hDAP = CDAP.Create(ExtractUrl, "");

         if (m_eClient == ClientType.OASIS_MONTAJ)
         {
            int   iChannels = 0;
            int   iBlobs  = 0;
            int   iLines = 0;

            // --- Create a new database ---

            if (CEDB.iLoaded(Filename) == 1) CEDB.UnLoad(Filename);
            
            
            int iHXYZClassToken = m_oMETA.ResolveUMN("CLASS:/Geosoft/Data/HXYZ");
            int iFieldsAttribToken = m_oMETA.ResolveUMN("ATTRIB:/Geosoft/Data/HXYZ/Fields");
            int iBlobsAttribToken = m_oMETA.ResolveUMN("ATTRIB:/Geosoft/Data/HXYZ/Blobs");


            if (iHXYZClassToken != GXNet.Constant.H_META_INVALID_TOKEN && iFieldsAttribToken != GXNet.Constant.H_META_INVALID_TOKEN)
               m_oMETA.GetAttribInt(iHXYZClassToken, iFieldsAttribToken, ref iChannels);

            if (iHXYZClassToken != GXNet.Constant.H_META_INVALID_TOKEN && iBlobsAttribToken != GXNet.Constant.H_META_INVALID_TOKEN)
               m_oMETA.GetAttribInt(iHXYZClassToken, iBlobsAttribToken, ref iBlobs);

         
            if (iLines <= 0) iLines = 200;
            if (iChannels <= 0) iChannels = 50;
            if (iBlobs <= iLines + iChannels + 20)
               iBlobs = Math.Max(iBlobs, iLines + iChannels);            

            CDB.CreateComp(Filename, iLines + 5, iChannels + 5, iBlobs + 20, 10, 100, "SUPER", "", 8192, 0);
                                                       
                                                       
            // --- Load this database ---

            hEDB = CEDB.Load(Filename);
            hDB  = hEDB.Lock();

         
            // --- format the group name ---

            strGroupName = "L0";         


            // --- Create group ---

            iSymb = hDB.CreateSymb(strGroupName,Geosoft.GXNet.Constant.DB_SYMB_LINE,Geosoft.GXNet.Constant.DB_OWN_SHARED,Geosoft.GXNet.Constant.DB_CATEGORY_LINE_GROUP);


            // --- Get the point data ---
            
            hDAP.RequestPointData(ServerName,hDSEL,hDB,iSymb);
   

            // --- Unlock the database ---
         
            hEDB.UnLock();
            hEDB.DelLine0();
            hEDB.GotoLine(iSymb);
            hEDB.LoadAllChans();

         } 
         else if (m_eClient == ClientType.ARCGIS)
         {
            hDAP.RequestPointDataAsSHP(ServerName,hDSEL,Filename);

            // --- Load to current data frame ---
          
            CARCMAP.LoadShape(Filename,"","");
         }
         else if (m_eClient == ClientType.MAPINFO)
         {
            string   strWorkDir = string.Empty;
            string   strTextFile;

            Geosoft.GXNet.CSYS.GtString("MAPINFO_DAP_CLIENT", "WORKING_DIR", ref strWorkDir);
            strTextFile = System.IO.Path.Combine(strWorkDir, Filename);

            hDAP.RequestPointDataAsTAB(ServerName, hDSEL, strTextFile);

            WriteToMapInfoFileList(strTextFile);
         }
         
         CSYS.Progress(0);

         if (hDSEL != null) hDSEL.Dispose();
         if (hEDB != null) hEDB.Dispose();
         if (hDB != null)  hDB.Dispose();
         if (hDAP != null) hDAP.Dispose();
         if (hIPJ != null) hIPJ.Dispose();
      }
	}
}
