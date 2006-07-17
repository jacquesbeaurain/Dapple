using System;
using System.Drawing.Design;
using System.ComponentModel;
using Geosoft.PropertyHelpers;

namespace Geosoft.GX.DAPGetData
{
	/// <summary>
	/// List of options for all dataset types
	/// </summary>
	[TypeConverter(typeof(PropertySorter))]   
	public class DatasetOptions
	{      
      #region enum 
      public enum ClientType
      {
         OASIS_MONTAJ,
         ARCGIS,
         MAPINFO
      }
      #endregion
      
      #region Member Variables
      protected static Int32     m_iNumMapInfoFiles;
      protected ClientType       m_eClient;
      protected string           m_strName;
      protected string           m_strServerName;
      protected string           m_strMetaUrl;
      protected string           m_strExtractUrl;

      protected Geosoft.GXNet.CMETA m_oMETA;
      protected Geosoft.GXNet.CIPJ  m_oIPJ;
      protected double              m_dMinX;
      protected double              m_dMinY;
      protected double              m_dMaxX;
      protected double              m_dMaxY;
      protected bool                m_bValidIPJ = true;
      
      #endregion

      #region Properties
      [PropertyOrder(10)]
      [Category("Data"), Description("Name of the dataset"), ReadOnly(true)]
      public string Name
      {
         get { return m_strName; }
         set { m_strName = value; }
      }      

      [Browsable(false)]
      public string ServerName
      {
         get { return m_strServerName; }
         set { m_strServerName = value; }
      }

      [Browsable(false)]
      public string MetaUrl
      {
         get { return m_strMetaUrl; }         
      }

      [Browsable(false)]
      public string ExtractUrl
      {
         get { return m_strExtractUrl; }         
      }

      /// <summary>
      /// Set the client type we are talking to
      /// </summary>
      [Browsable(false)]
      public ClientType Client
      {
         get { return m_eClient; }
      }

      /// <summary>
      /// Get/Set the number of datasets that mapinfo must display
      /// </summary>
      [Browsable(false)]
      public static Int32 NumMapInfoDatasets
      {
         get { return m_iNumMapInfoFiles; }
         set { m_iNumMapInfoFiles = value; }
      }

      /// <summary>
      /// Get the meta object for this dataset
      /// </summary>
      [Browsable(false)]
      public Geosoft.GXNet.CMETA META
      {
         get { return m_oMETA; }
      }

      /// <summary>
      /// Get the meta object for this dataset
      /// </summary>
      [Browsable(false)]
      public Geosoft.GXNet.CIPJ IPJ
      {
         get { return m_oIPJ; }
      }
      
      /// <summary>
      /// Get the extents
      /// </summary>
      [Browsable(false)]
      public double MinX
      {
         get { return m_dMinX; }
      }

      /// <summary>
      /// Get the extents
      /// </summary>
      [Browsable(false)]
      public double MinY
      {
         get { return m_dMinY; }
      }

      /// <summary>
      /// Get the extents
      /// </summary>
      [Browsable(false)]
      public double MaxX
      {
         get { return m_dMaxX; }
      }

      /// <summary>
      /// Get the extents
      /// </summary>
      [Browsable(false)]
      public double MaxY
      {
         get { return m_dMaxY; }
      }
      #endregion

      /// <summary>
      /// 	<para>Initializes an instance of the <see cref="DatasetOptions"/> class.</para>
      /// </summary>
      /// <param name="strServerName">
      /// </param>
      /// <param name="strTitle">
      /// </param>
      /// <param name="strUrl">
      /// </param>
      public DatasetOptions(string strServerName, string strTitle, string strUrl)
      {
         m_eClient = ClientType.OASIS_MONTAJ;
         if (GetDapData.Instance.IsArcGIS)
         {
            m_eClient = ClientType.ARCGIS;
         }
         else if (GetDapData.Instance.IsMapInfo)
         {
            m_eClient = ClientType.MAPINFO;
         }

         m_strServerName = strServerName;
         m_strName = strTitle;

         Server oServer = (Server)GetDapData.Instance.ServerList[strUrl];

         if (oServer == null) 
         {
            System.Windows.Forms.MessageBox.Show("Missing " + strUrl + " in list of valid servers");
            return;
         }

         m_strExtractUrl = oServer.ExtractUrl;
         m_strMetaUrl = oServer.MetaUrl;

         //LoadMeta();
      }

      public virtual void DownloadDataset() 
      {         
      }                  

      /// <summary>
      /// Create a new map
      /// </summary>
      /// <param name="hMapBF"></param>
      public void CreateMap(GXNet.CBF  hMapBF)
      {
         Geosoft.Dap.Common.BoundingBox   oExtractBoundingBox;
         Geosoft.Dap.Common.BoundingBox   oDatasetBoundingBox;
         Geosoft.GXNet.CIPJ               hIPJ = null;
         double                           dWidth, dHeight, dMaxX, dMinX, dMaxY, dMinY;
         
         
         // --- Generate the extraction bounding box ---

         GenerateBoundingBox(out oExtractBoundingBox);
         
         
         // --- see if we have to reproject the extents to the dataset coordinate system (map always created in dataset native coordinate sytem) ---

         oDatasetBoundingBox = Constant.SetCoordinateSystem(m_dMinX, m_dMinY, m_dMaxX, m_dMaxY, m_oIPJ);
         if (oExtractBoundingBox != oDatasetBoundingBox)
            Constant.Reproject(oExtractBoundingBox, oDatasetBoundingBox.CoordinateSystem);
         
         
         // --- set the projection of the new map ---

         hIPJ = Geosoft.GXNet.CIPJ.Create();
         hIPJ.SetGXF(string.Empty, oExtractBoundingBox.CoordinateSystem.Datum, oExtractBoundingBox.CoordinateSystem.Method, oExtractBoundingBox.CoordinateSystem.Units, oExtractBoundingBox.CoordinateSystem.LocalDatum);
         

         // --- set the extents ---

         dMaxX = oExtractBoundingBox.MaxX;
         dMaxY = oExtractBoundingBox.MaxY;
         dMinX = oExtractBoundingBox.MinX;
         dMinY = oExtractBoundingBox.MinY;         
         hIPJ.Serial(hMapBF);

         dWidth = dMaxX - dMinX;
         dHeight = dMaxY - dMinY;

         // --- expand the area by 5% ---

         dMaxX += dWidth * 0.05 / 2;
         dMinX -= dWidth * 0.05 / 2;
         dMaxY += dHeight * 0.05 / 2;
         dMinY -= dHeight * 0.05 / 2;

         Geosoft.GXNet.CSYS.SetReal("XYRANGE","MAX_X",dMaxX);
         Geosoft.GXNet.CSYS.SetReal("XYRANGE","MAX_Y",dMaxY);
         Geosoft.GXNet.CSYS.SetReal("XYRANGE","MIN_X",dMinX);
         Geosoft.GXNet.CSYS.SetReal("XYRANGE","MIN_Y",dMinY);
         if (hIPJ != null) hIPJ.Dispose();
      }      

      /// <summary>
      /// Write a file into the list for mapinfo to open
      /// </summary>
      /// <param name="strFileName"></param>
      public void WriteToMapInfoFileList(string strFileName)
      {
         string                  strWorkDir = string.Empty;
         string                  strTextFile;
         System.IO.StreamWriter  hFile;

         m_iNumMapInfoFiles++;

         Geosoft.GXNet.CSYS.GtString("MAPINFO_DAP_CLIENT", "WORKING_DIR", ref strWorkDir);
         strTextFile = System.IO.Path.Combine(strWorkDir, "_MapFiles.Txt");

         hFile = new System.IO.StreamWriter(strTextFile, true, System.Text.Encoding.UTF8);

         hFile.WriteLine(strFileName);
         hFile.Close();
      }      

      /// <summary>
      /// Calculate the default resolution
      /// </summary>
      public double CalculateDefaultResolution()
      {
         double dMaxX, dMinX, dMaxY, dMinY, dRes;
         Geosoft.Dap.Common.BoundingBox   oExtractBoundingBox;
         
         GenerateBoundingBox(out oExtractBoundingBox);         

         dMaxX = oExtractBoundingBox.MaxX;
         dMaxY = oExtractBoundingBox.MaxY;
         dMinX = oExtractBoundingBox.MinX;
         dMinY = oExtractBoundingBox.MinY;         
         
         dRes = 250.0;
         if ((dMaxX - dMinX) > (dMaxY - dMinY))
            dRes = (dMaxX - dMinX) / dRes;
         else   
            dRes = (dMaxY - dMinY) / dRes;
         
         // ---- calculate the resolution to 4 significant decimal places ---

         try
         {
            string strPercision = dRes.ToString("g4");
            dRes = Convert.ToDouble(strPercision);
         } 
         catch {}

         return dRes;
      }

      /// <summary>
      /// Calculate the default image resolution
      /// </summary>
      /// <param name="hDataSet"></param>
      /// <returns></returns>
      public double CalculateDefaultImageResolution()
      {
         double dImgRes;
         double dRes;

         dRes = CalculateDefaultResolution();

         dImgRes = dRes / 3.0;

         // ---- calculate the resolution to 4 significant decimal places ---

         try
         {
            string strPercision = dImgRes.ToString("g4");
            dImgRes = Convert.ToDouble(strPercision);
         } 
         catch {}

         return dImgRes;
      }

      /// <summary>
      /// 
      /// </summary>
      protected void LoadMeta()
      {
         Geosoft.GXNet.CDAP   hDAP = null;
         Geosoft.GXNet.CMETA  hMETA = null;

         Int32                iAttribToken = GXNet.Constant.H_META_INVALID_TOKEN;
         Int32                iClassToken = GXNet.Constant.H_META_INVALID_TOKEN; 
         Int32                iDataClassToken = GXNet.Constant.H_META_INVALID_TOKEN;
         Int32                iIPJAttribToken = GXNet.Constant.H_META_INVALID_TOKEN;
         Int32                iMaxXAttribToken = GXNet.Constant.H_META_INVALID_TOKEN;
         Int32                iMaxYAttribToken = GXNet.Constant.H_META_INVALID_TOKEN;
         Int32                iMinXAttribToken = GXNet.Constant.H_META_INVALID_TOKEN;
         Int32                iMinYAttribToken = GXNet.Constant.H_META_INVALID_TOKEN;
         
         double                     dMaxX = Geosoft.GXNet.Constant.rDUMMY;
         double                     dMaxY = Geosoft.GXNet.Constant.rDUMMY;
         double                     dMinX = Geosoft.GXNet.Constant.rDUMMY;
         double                     dMinY = Geosoft.GXNet.Constant.rDUMMY;         

         hDAP = Geosoft.GXNet.CDAP.Create(MetaUrl, "");

         hMETA = hDAP.DescribeDataSet(m_strServerName);         

         iClassToken = hMETA.ResolveUMN("CLASS:/Geosoft/Core/DAP/Data/DatasetInfo");
         iAttribToken = hMETA.ResolveUMN("ATTRIB:/Geosoft/Core/DAP/Data/DatasetInfo/Information");

         // --- Get the META to display ---

         m_oMETA = Geosoft.GXNet.CMETA.Create();
      
         hMETA.GetAttribOBJ(iClassToken, iAttribToken, m_oMETA);            
         iDataClassToken = m_oMETA.ResolveUMN("CLASS:/Geosoft/Data");
         iIPJAttribToken = m_oMETA.ResolveUMN("ATTRIB:/Geosoft/Data/CoordinateSystem");
         iMaxXAttribToken = m_oMETA.ResolveUMN("ATTRIB:/Geosoft/Data/BoundingMaxX");
         iMaxYAttribToken = m_oMETA.ResolveUMN("ATTRIB:/Geosoft/Data/BoundingMaxY");
         iMinXAttribToken = m_oMETA.ResolveUMN("ATTRIB:/Geosoft/Data/BoundingMinX");
         iMinYAttribToken = m_oMETA.ResolveUMN("ATTRIB:/Geosoft/Data/BoundingMinY");
         
         try
         {
            m_oIPJ = Geosoft.GXNet.CIPJ.Create();
            m_oMETA.GetAttribOBJ(iDataClassToken, iIPJAttribToken, m_oIPJ);
         } 
         catch
         {
            m_bValidIPJ = false;
         }
         
         m_oMETA.GetAttribReal(iDataClassToken, iMaxXAttribToken, ref m_dMaxX);
         m_oMETA.GetAttribReal(iDataClassToken, iMaxYAttribToken, ref m_dMaxY);         
         m_oMETA.GetAttribReal(iDataClassToken, iMinXAttribToken, ref m_dMinX);
         m_oMETA.GetAttribReal(iDataClassToken, iMinYAttribToken, ref m_dMinY);
         
         if (hMETA != null) hMETA.Dispose();
         if (hDAP != null) hDAP.Dispose();         
      }

      #region Public Static Methods
      /// <summary>
      /// Clear the map info file list
      /// </summary>
      public static void ClearMapInfoFileList()
      {
         string   strWorkDir = string.Empty;
         string   strTextFile;

         Geosoft.GXNet.CSYS.GtString("MAPINFO_DAP_CLIENT", "WORKING_DIR", ref strWorkDir);
         strTextFile = System.IO.Path.Combine(strWorkDir, "_MapFiles.Txt");

         System.IO.File.Delete(strTextFile);
      }

      public static bool bIsArcGIS()
      {
         String                     strLicClass = "";

         Geosoft.GXNet.CSYS.IGetLicenseClass(ref strLicClass);

         if (String.Compare(strLicClass.ToString(), "arcgis", true) == 0)
            return true;
         return false;
      }

      public static bool bIsMapInfo()
      {
         Int32 iMapInfo = 0;

         iMapInfo = Geosoft.GXNet.CSYS.iGetInt("DAPGETDATA", "MAPINFO_CLIENT");

         if (iMapInfo > 0)
            return true;
         return false;
      }
      #endregion

      #region Protected Methods
      /// <summary>
      /// Set the extraction bounding box
      /// </summary>
      /// <returns></returns>
      protected Geosoft.GXNet.CDSEL SetExtractionBoundingBox(bool bReproject)
      {
         Geosoft.GXNet.CIPJ               oIPJ = null;
         Geosoft.GXNet.CDSEL              oDSEL = Geosoft.GXNet.CDSEL.Create();
         Geosoft.Dap.Common.BoundingBox   oExtractBoundingBox;
         

         GenerateBoundingBox(out oExtractBoundingBox);
                  
         oIPJ = Geosoft.GXNet.CIPJ.Create();
         oIPJ.SetGXF(String.Empty, oExtractBoundingBox.CoordinateSystem.Datum, oExtractBoundingBox.CoordinateSystem.Method, oExtractBoundingBox.CoordinateSystem.Units, oExtractBoundingBox.CoordinateSystem.LocalDatum);
         oDSEL.SetIPJ(oIPJ, Convert.ToInt32(bReproject));
         oDSEL.SelectRect(oExtractBoundingBox.MinX, oExtractBoundingBox.MinY, oExtractBoundingBox.MaxX, oExtractBoundingBox.MaxY);             
         
         if (oIPJ != null) oIPJ.Dispose();

         return oDSEL;
      }

      /// <summary>
      /// Generate the bounding box for this dataset
      /// </summary>
      /// <param name="dMaxX"></param>
      /// <param name="dMinX"></param>
      /// <param name="dMaxY"></param>
      /// <param name="dMinY"></param>
      /// <returns></returns>
      protected void GenerateBoundingBox(out Geosoft.Dap.Common.BoundingBox oBoundingBox)
      {
         Geosoft.Dap.Common.BoundingBox   oDatasetBoundingBox;
         Geosoft.Dap.Common.BoundingBox   oIntersectBox;
         Geosoft.Dap.Common.BoundingBox   oMapBoundingBox;
         bool bReproject = true;

         
         oBoundingBox = new Geosoft.Dap.Common.BoundingBox();
         oDatasetBoundingBox = Constant.SetCoordinateSystem(m_dMinX, m_dMinY, m_dMaxX, m_dMaxY, m_oIPJ);

         
         // --- see if we should be using the AOI or the current view extents ---

         oIntersectBox = new Geosoft.Dap.Common.BoundingBox(GetDapData.Instance.SearchExtents);
         oMapBoundingBox = new Geosoft.Dap.Common.BoundingBox(GetDapData.Instance.SearchExtents);
            
         oIntersectBox.MinX = Math.Max(GetDapData.Instance.ViewExtents.MinX, GetDapData.Instance.SearchExtents.MinX);
         oIntersectBox.MaxX = Math.Min(GetDapData.Instance.ViewExtents.MaxX, GetDapData.Instance.SearchExtents.MaxX);
         oIntersectBox.MinY = Math.Max(GetDapData.Instance.ViewExtents.MinY, GetDapData.Instance.SearchExtents.MinY);
         oIntersectBox.MaxY = Math.Min(GetDapData.Instance.ViewExtents.MaxY, GetDapData.Instance.SearchExtents.MaxY);

         if (Constant.IsValidBoundingBox(oIntersectBox))
         {
            oMapBoundingBox = new Geosoft.Dap.Common.BoundingBox(oIntersectBox);
         }


         // --- do not have a valid projection, use the aoi ---

         if (!m_bValidIPJ || !Constant.IsValidBoundingBox(oDatasetBoundingBox))
         {
            oBoundingBox = new Geosoft.Dap.Common.BoundingBox(oMapBoundingBox);

            if (GetDapData.Instance.OMExtents != null) 
            {
               Constant.Reproject(oBoundingBox, GetDapData.Instance.OMExtents.CoordinateSystem);
            }

            return;
         }

         bReproject = Constant.Reproject(oDatasetBoundingBox, GetDapData.Instance.SearchExtents.CoordinateSystem);

         if (bReproject)
         {
            oIntersectBox = new Geosoft.Dap.Common.BoundingBox(oMapBoundingBox);
            
            oIntersectBox.MinX = Math.Max(oDatasetBoundingBox.MinX, oMapBoundingBox.MinX);
            oIntersectBox.MaxX = Math.Min(oDatasetBoundingBox.MaxX, oMapBoundingBox.MaxX);
            oIntersectBox.MinY = Math.Max(oDatasetBoundingBox.MinY, oMapBoundingBox.MinY);
            oIntersectBox.MaxY = Math.Min(oDatasetBoundingBox.MaxY, oMapBoundingBox.MaxY);

            if (Constant.IsValidBoundingBox(oIntersectBox))
            {
               oBoundingBox = oIntersectBox;               
            } 
            else 
            {
               // --- area of interest does not overlap ---

               oBoundingBox = Constant.SetCoordinateSystem(m_dMinX, m_dMinY, m_dMaxX, m_dMaxY, m_oIPJ);
            }
         } 
         else 
         {
            // --- cannot reproject area to bounding box, assume area of interest does not overlap ---

            oBoundingBox = Constant.SetCoordinateSystem(m_dMinX, m_dMinY, m_dMaxX, m_dMaxY, m_oIPJ);
         }

         if (GetDapData.Instance.OMExtents != null) 
         {
            Constant.Reproject(oBoundingBox, GetDapData.Instance.OMExtents.CoordinateSystem);
         }
      }      
      #endregion
	}
}
