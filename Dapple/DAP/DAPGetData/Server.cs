using System;
using System.IO;
using System.Xml;
using System.Collections;
using Geosoft.Dap;

namespace Geosoft.GX.DAPGetData
{
	/// <summary>
	/// Summary description for Server.
	/// </summary>
	public class Server
	{
      #region Enum
      public enum ServerStatus
      {
         OnLine,
         OffLine,
         Disabled,
         Unsupported,
         Maintenance
      }
      #endregion

      #region Constants
      protected const string CONFIGURATION_FILE = "_configuration.xml";
      protected const string BROWSERMAP_FILE = "_browser_map.xml";
      #endregion

      #region Member Variables
      protected string                    m_strName;
      protected string                    m_strUrl;
      protected Int32                     m_iIndex;
      protected Int32                     m_iMajorVersion;
      protected Int32                     m_iMinorVersion;
      protected string                    m_strCacheVersion;
      protected bool                      m_bSecure = false;
      protected bool                      m_bLoggedIn = false;
      protected string                    m_strUserName;
      protected string                    m_strPassword;

      protected ServerStatus              m_eStatus;
      protected Int32                     m_iCount;

      protected Geosoft.Dap.Command             m_oCommand;
      protected Geosoft.Dap.Common.BoundingBox  m_oServerBoundingBox = new Geosoft.Dap.Common.BoundingBox();      
      protected Geosoft.Dap.Configuration       m_oServerConfiguration = null;
      protected CatalogCollection               m_oCatalogs;

      protected XmlDocument                     m_oBrowserMap = null;      

      protected string m_strCacheDir;
      protected string m_strCacheRoot;
      #endregion

      #region Properties
      /// <summary>
      /// Get the server name
      /// </summary>
      public string Name
      {
         get { return m_strName; }
      }

      /// <summary>
      /// Index of server in the list
      /// </summary>
      public Int32 Index
      {
         get { return m_iIndex; }
         set { m_iIndex = value; }
      }

      /// <summary>
      /// Get the major version of this server
      /// </summary>
      public Int32 MajorVersion
      {
         get { return m_iMajorVersion; }
      }

      /// <summary>
      /// Get the minor version of this server
      /// </summary>
      public Int32 MinorVersion
      {
         get { return m_iMinorVersion; }
      }

      /// <summary>
      /// Get the server status
      /// </summary>
      public ServerStatus Status
      {
         get { return m_eStatus; }
         set { m_eStatus = value; }
      }

      /// <summary>
      /// Return the dataset count for the last AOI query
      /// </summary>
      public Int32 DatasetCount
      {
         get { return m_iCount; }
      }

      /// <summary>
      /// Get the dap server connection
      /// </summary>
      public Geosoft.Dap.Command Command
      {
         get { return m_oCommand; }
      }
      
      /// <summary>
      /// Get the server configuration
      /// </summary>
      public Geosoft.Dap.Configuration ServerConfiguration
      {
         get { return m_oServerConfiguration; }
      }

      /// <summary>
      /// Get the Server extents
      /// </summary>
      public Geosoft.Dap.Common.BoundingBox ServerExtents
      {
         get { return m_oServerBoundingBox; }
      }           

      /// <summary>
      /// Get the catalog cache
      /// </summary>
      public Geosoft.GX.DAPGetData.CatalogCollection CatalogCollection
      {
         get { return m_oCatalogs; }
      }

      /// <summary>
      /// Get the server url
      /// </summary>
      public string Url
      {
         get { return m_strUrl; }
      }      

      /// <summary>
      /// Get the url to retrieve the meta data from
      /// </summary>
      public string MetaUrl
      {
         get 
         {
            if (m_oCommand.XmlVersion == Geosoft.Dap.Command.Version.GEOSOFT_XML_1_0)
               return m_strUrl + "/DCS";
            else
               return m_strUrl + "/DAP";
         }
      }

      /// <summary>
      /// Get the url to extract the data from
      /// </summary>
      public string ExtractUrl
      {
         get 
         { 
            if (m_oCommand.XmlVersion == Geosoft.Dap.Command.Version.GEOSOFT_XML_1_0)
               return m_strUrl + "/DDS";
            else
               return m_strUrl + "/DAP";
         }
      }
      
      /// <summary>
      /// Get the cache directory
      /// </summary>
      public string CacheDir
      {
         get { return m_strCacheDir; }
      }      

      /// <summary>
      /// Get the version that we have in the cache
      /// </summary>
      public string CacheVersion
      {
         get { return m_strCacheVersion; }
      }

      /// <summary>
      /// Get whether this is a secure server or not
      /// </summary>
      public bool Secure
      {
         get { return m_bSecure; }
      }

      /// <summary>
      /// Get whether we have already logged in
      /// </summary>
      public bool LoggedIn
      {
         get { return m_bLoggedIn; }
      }      
      #endregion

      #region Constructor
      /// <summary>
      /// 	<para>Initializes an instance of the <see cref="Server"/> class.</para>
      /// </summary>
      /// <param name="strDnsAddress">
      /// </param>
#if !DAPPLE
      public Server(string strDnsAddress)
#else
      public Server(string strDnsAddress, string strCacheDir)
#endif
      {
         string strEdition, strConfigEdition;

         // --- create the connection to the server ---

#if !DAPPLE
         m_oCommand = new Geosoft.Dap.Command(strDnsAddress, true, Geosoft.Dap.Command.Version.GEOSOFT_XML_1_1);

         m_strCacheRoot = string.Empty;
         GXNet.CSYS.IGetDirectory(GXNet.Constant.SYS_DIR_USER, ref m_strCacheRoot);
#else
         m_oCommand = new Geosoft.Dap.Command(strDnsAddress, false, Geosoft.Dap.Command.Version.GEOSOFT_XML_1_1);
         m_strCacheRoot = Path.Combine(strCacheDir, "DapCache");
#endif

         m_strUrl = strDnsAddress;         
		    
#if !DAPPLE
         // --- ensure this server is trusted ---
                     
         GXNet.CDAP.SetAuthorization(m_strUrl, Geosoft.GXNet.Constant.GUI_AUTH_TRUST);
#endif
         m_oCatalogs = new CatalogCollection(this);

         ConfigureServer();

         // --- If the edition change we need to reload the configuration ---
         m_oCommand.GetCatalogEdition(out strConfigEdition, out strEdition);
         if (m_strCacheVersion != strConfigEdition)
            UpdateConfiguration();
      }

#if !DAPPLE
      /// <summary>
      /// 	<para>Initializes an instance of the <see cref="Server"/> class.</para>
      /// </summary>
      /// <param name="oServerNode">
      /// </param>
      /// <exception cref="ArgumentNullException">
      /// 	<para>The argument <paramref name="oServerNode"/> is <langword name="null"/>.</para>
      /// </exception>
      /// <exception cref="ArgumentOutOfRangeException">
      /// 	<para>The argument <paramref name="oServerNode"/> is out of range.</para>
      /// </exception>
      public Server(XmlNode oServerNode)
      {
         string strEdition, strConfigEdition;
         XmlNode oAttr;

         m_strCacheRoot = string.Empty;
         GXNet.CSYS.IGetDirectory(GXNet.Constant.SYS_DIR_USER, ref m_strCacheRoot);

         if (oServerNode == null) throw new ArgumentNullException("oServerNode");
         if (oServerNode.Name != Constant.Xml.Tag.Server) throw new ArgumentOutOfRangeException("oServerNode");

         oAttr = oServerNode.Attributes.GetNamedItem(Constant.Xml.Attr.Name);
         if (oAttr == null) throw new ApplicationException("Missing name attribute in server node");
         m_strName = oAttr.Value;

         oAttr = oServerNode.Attributes.GetNamedItem(Constant.Xml.Attr.Url);
         if (oAttr == null) throw new ApplicationException("Missing url attribute in server node");
         m_strUrl = oAttr.Value;

         oAttr = oServerNode.Attributes.GetNamedItem(Constant.Xml.Attr.Status);
         if (oAttr == null) throw new ApplicationException("Missing status attribute in server node");
         m_eStatus = ParseStatus(oAttr.Value);
         
         oAttr = oServerNode.Attributes.GetNamedItem(Constant.Xml.Attr.Major_Version);
         if (oAttr == null) throw new ApplicationException("Missing major version attribute in server node");
         m_iMajorVersion = Int32.Parse(oAttr.Value);

         oAttr = oServerNode.Attributes.GetNamedItem(Constant.Xml.Attr.Minor_Version);
         if (oAttr == null) throw new ApplicationException("Missing minor version attribute in server node");
         m_iMinorVersion = Int32.Parse(oAttr.Value);

         oAttr = oServerNode.Attributes.GetNamedItem(Constant.Xml.Attr.CacheVersion);
         if (oAttr == null) throw new ApplicationException("Missing cache version attribute in server node");
         m_strCacheVersion = oAttr.Value;         

         m_oCommand = new Geosoft.Dap.Command(m_strUrl, true, Geosoft.Dap.Command.Version.GEOSOFT_XML_1_1);

         // --- this is a 6.2 server, get decreased configuration parameters ---

         if (m_iMajorVersion < 6 || (m_iMajorVersion == 6 && m_iMinorVersion < 3))
            m_oCommand.ChangeVersion(Command.Version.GEOSOFT_XML_1_0);
         
         // --- ensure this server is trusted ---
                     
         GXNet.CDAP.SetAuthorization(m_strUrl, Geosoft.GXNet.Constant.GUI_AUTH_TRUST);

         m_oCatalogs = new CatalogCollection(this);

         ConfigureServer();

         // --- If the edition change we need to reload the configuration ---
         m_oCommand.GetCatalogEdition(out strConfigEdition, out strEdition);
         if (m_strCacheVersion != strConfigEdition)
            UpdateConfiguration();
      }
#endif

      #endregion

      #region Public Methods
      /// <summary>
      /// Save the server node to xml
      /// </summary>
      /// <param name="oNode">
      /// </param>
      /// <exception cref="ArgumentNullException">
      /// 	<para>The argument <paramref name="oNode"/> is <langword name="null"/>.</para>
      /// </exception>
      public void Save(XmlNode oNode)
      {
         XmlElement     oServerNode;
         XmlAttribute   oAttr;

         if (oNode == null) throw new ArgumentNullException("oNode");

         oServerNode = oNode.OwnerDocument.CreateElement(Constant.Xml.Tag.Server);
            
         oAttr = oNode.OwnerDocument.CreateAttribute(Constant.Xml.Attr.Name);
         oAttr.Value = m_strName;
         oServerNode.Attributes.Append(oAttr);

         oAttr = oNode.OwnerDocument.CreateAttribute(Constant.Xml.Attr.Url);
         oAttr.Value = m_strUrl;
         oServerNode.Attributes.Append(oAttr);

         oAttr = oNode.OwnerDocument.CreateAttribute(Constant.Xml.Attr.Status);
         oAttr.Value = m_eStatus.ToString();
         oServerNode.Attributes.Append(oAttr);

         oAttr = oNode.OwnerDocument.CreateAttribute(Constant.Xml.Attr.Major_Version);
         oAttr.Value = m_iMajorVersion.ToString();
         oServerNode.Attributes.Append(oAttr);

         oAttr = oNode.OwnerDocument.CreateAttribute(Constant.Xml.Attr.Minor_Version);
         oAttr.Value = m_iMinorVersion.ToString();
         oServerNode.Attributes.Append(oAttr);

         oAttr = oNode.OwnerDocument.CreateAttribute(Constant.Xml.Attr.CacheVersion);
         oAttr.Value = m_strCacheVersion;
         oServerNode.Attributes.Append(oAttr); 

         oNode.AppendChild(oServerNode);
      }

      /// <summary>
      /// Get the browser map
      /// </summary>
      /// <returns></returns>
      public XmlDocument GetBrowserMap()
      {
         return m_oBrowserMap;
      }

      /// <summary>
      /// Count the number of datasets that match this aoi
      /// </summary>
      /// <param name="hBox"></param>
      /// <returns></returns>
      public Int32 GetDatasetCount(Geosoft.Dap.Common.BoundingBox hBox, string szKeywords)
      {
         m_iCount = 0;
         try
         {
            XmlDocument oDocument = m_oCommand.GetDataSetCount(String.Empty, -1, 0, 0, szKeywords, hBox, null);
            m_oCommand.Parser.DataSetCount(oDocument, out m_iCount);

            // --- check to see if we have a status node ---

            m_eStatus = ServerStatus.OnLine;

            XmlNodeList oNodeList = oDocument.SelectNodes("//" + Geosoft.Dap.Xml.Common.Constant.Tag.CONFIGURATION_TAG);
            if (oNodeList.Count > 0)
            {
               XmlNode  oNode = oNodeList[0];
               XmlNode  oAttr;

               // --- if status attribute exists then we are in maintenance ---
               // --- this server is currently in maintenance mode ---

               oAttr = oNode.Attributes.GetNamedItem(Geosoft.Dap.Xml.Common.Constant.Attribute.STATUS_ATTR);
               if (oAttr != null && oAttr.Value == "maintenance")
                  m_eStatus = ServerStatus.Maintenance;
            } 
            else 
            {
               
            }
         } 
         catch (Exception e)
         {
            GetDapError.Instance.Write("Get Dataset Count (" + m_strUrl + ") - " + e.Message);
         }
         return m_iCount;
      }

      /// <summary>
      /// The configuratin has become out of date, clear the cache and get new configuration information from the server
      /// </summary>
      public void UpdateConfiguration()
      {
         // --- delete the browser map and configuration ---

         string strBrowserMapFile = Path.Combine(m_strCacheDir, BROWSERMAP_FILE);
         string strConfigurationFile = Path.Combine(m_strCacheDir, CONFIGURATION_FILE);
         if (File.Exists(strBrowserMapFile))
            File.Delete(strBrowserMapFile);
         if (File.Exists(strConfigurationFile))
            File.Delete(strConfigurationFile);

         ConfigureServer();
      }

      /// <summary>
      /// Authenticate the user
      /// </summary>
      /// <param name="strUserName"></param>
      /// <param name="strPassword"></param>
      /// <returns></returns>
      public bool Login()
      {
         Login oLogin = new Login(this);

         m_bLoggedIn = false;
         if (oLogin.ShowDialog() == System.Windows.Forms.DialogResult.OK)
         {         
            m_bLoggedIn = true;
            m_strUserName = oLogin.UserName;
            m_strPassword = oLogin.Password;
            Command.ChangeLogin(m_strUserName, m_strPassword);
#if !DAPPLE
            Geosoft.GXNet.CDAP.SetUserNamePassword(m_strUserName, m_strPassword);
#endif
            oLogin.Dispose();
            return true;
         }

         CatalogCollection.Clear();
         oLogin.Dispose();
         return false;
      }

      /// <summary>
      /// Logout from this server
      /// </summary>
      /// <returns></returns>
      public bool Logout()
      {
         m_bLoggedIn = false;
         m_strUserName = string.Empty;
         m_strPassword = string.Empty;
         Command.ChangeLogin(m_strUserName, m_strPassword);

         try
         {
#if !DAPPLE
            Geosoft.GXNet.CDAP.SetUserNamePassword(m_strUserName, m_strPassword);
#endif
         } 
         catch {}

    
         CatalogCollection.Clear();
         return true;
      }

      /// <summary>
      /// Switch to the already authenticated user
      /// </summary>
      public void SwitchToUser()
      {
         if (m_bLoggedIn)
         {
            Command.ChangeLogin(m_strUserName, m_strPassword);
#if !DAPPLE
            Geosoft.GXNet.CDAP.SetUserNamePassword(m_strUserName, m_strPassword);
#endif
         }
      }
      #endregion

      #region Protected Methods
      /// <summary>
      /// Translate the server status from a string to enum
      /// </summary>
      /// <param name="strStatus"></param>
      /// <returns></returns>
      protected ServerStatus ParseStatus(string strStatus)
      {
         ServerStatus   eStatus = ServerStatus.OffLine;

         if (String.Compare(strStatus, ServerStatus.OnLine.ToString(), true) == 0)         
            eStatus = ServerStatus.OnLine;
         else if (String.Compare(strStatus, ServerStatus.OffLine.ToString(), true) == 0)
            eStatus = ServerStatus.OffLine;
         else if (String.Compare(strStatus, ServerStatus.Disabled.ToString(), true) == 0)
            eStatus = ServerStatus.Disabled;
         else if (String.Compare(strStatus, ServerStatus.Unsupported.ToString(), true) == 0)
            eStatus = ServerStatus.Unsupported;
         else if (String.Compare(strStatus, ServerStatus.Maintenance.ToString(), true) == 0)
            eStatus = ServerStatus.Maintenance;
              
         return eStatus;
      }

      /// <summary>
      /// Load the configuration for this server
      /// </summary>
      protected void ConfigureServer()
      {  
         string strDir = string.Empty;

         // --- only load configuration if we could actually talk to this server ---

         if (m_eStatus == ServerStatus.Unsupported) return;         

         m_strCacheDir = Path.Combine(m_strCacheRoot, "DapCache");

         // --- remove the http:// from the directory name ---

         strDir = m_strUrl.Substring(7);
         foreach (char c in System.IO.Path.GetInvalidPathChars())
         {
            strDir.Replace(c, '_');
         }

         m_strCacheDir = System.IO.Path.Combine(m_strCacheDir, strDir);

         if (!System.IO.Directory.Exists(m_strCacheDir))
         {
            System.IO.Directory.CreateDirectory(m_strCacheDir);
         } 

         if (m_eStatus == ServerStatus.Unsupported || m_eStatus == ServerStatus.OffLine) return;

         LoadConfiguration();         
         LoadBrowserMap();
      }

      /// <summary>
      /// Load the configuration
      /// </summary>
      protected void LoadConfiguration()
      {
         // --- Check to see if this is in the cache ---

         string         strConfigurationFile = Path.Combine(m_strCacheDir, CONFIGURATION_FILE);
         XmlDocument    oDoc;
         XmlNodeList    oList;
         double         dMaxX, dMaxY, dMinX, dMinY;
         string         strMethod = string.Empty;
         string         strDatum = string.Empty;
         string         strProjection = string.Empty;
         string         strLocalDatum = string.Empty;
         string         strUnits = string.Empty;

         
         if (File.Exists(strConfigurationFile))
         {
            oDoc = new XmlDocument();
            oDoc.Load(strConfigurationFile);
         } 
         else 
         {
            try
            {
               oDoc = m_oCommand.GetConfiguration(null);
               oDoc.Save(strConfigurationFile);
            }  
            catch (Exception)
            {
               // --- this server against a 6.2 server ---

               try
               {
                  m_oCommand.ChangeVersion(Geosoft.Dap.Command.Version.GEOSOFT_XML_1_0);
                  oDoc = m_oCommand.GetConfiguration(null);
                  oDoc.Save(strConfigurationFile);
               }
               catch (Exception ex)
               {
                  // --- verify this server is on-line, because if it is then it is an unsupported version ---

                  m_eStatus = ServerStatus.OffLine;     
                  GetDapError.Instance.Write("Configure Server, Get Configuration (" + m_strUrl + ") - " + ex.Message);
                  return;
               }
            }
         }

         oList = oDoc.SelectNodes("/" + Geosoft.Dap.Xml.Common.Constant.Tag.GEO_XML_TAG + "/" + Geosoft.Dap.Xml.Common.Constant.Tag.RESPONSE_TAG + "/" + Geosoft.Dap.Xml.Common.Constant.Tag.CONFIGURATION_TAG + "/" + Geosoft.Dap.Xml.Common.Constant.Tag.META_TAG);
         if (oList.Count == 0) 
         {
            m_iMajorVersion = 6;
            m_iMinorVersion = 1;
            m_eStatus = ServerStatus.Unsupported;

            GetDapError.Instance.Write("Configure Server, Default AOI (" + m_strUrl + ")");
            return;
         }
                  
         m_oServerConfiguration = new Configuration(oList[0]);
         
         
         // --- get the server configuration edition ---

         m_oServerConfiguration.GetEdition(out m_strCacheVersion);

         
         // --- get the default aoi ---

         if (m_oServerConfiguration.GetDefaultAOI(out dMaxX, out dMaxY, out dMinX, out dMinY, out strProjection, out strDatum, out strMethod, out strUnits, out strLocalDatum))
         {            
            m_oServerBoundingBox.MaxX = dMaxX;
            m_oServerBoundingBox.MaxY = dMaxY;
            m_oServerBoundingBox.MinX = dMinX;
            m_oServerBoundingBox.MinY = dMinY;

            m_oServerBoundingBox.CoordinateSystem.Projection = strProjection;
            m_oServerBoundingBox.CoordinateSystem.Datum = strDatum;
            m_oServerBoundingBox.CoordinateSystem.Method = strMethod;
            m_oServerBoundingBox.CoordinateSystem.LocalDatum = strLocalDatum;
            m_oServerBoundingBox.CoordinateSystem.Units = strUnits;
         }
         m_strName =  m_oServerConfiguration.GetServerName();  
         
         m_oServerConfiguration.GetVersion(out m_iMajorVersion, out m_iMinorVersion);     
    
         // --- this is a 6.3 server, get increased configuration parameters ---

         if (m_iMajorVersion <= 6 && m_iMinorVersion <= 2)
         {
            m_oCommand.ChangeVersion(Geosoft.Dap.Command.Version.GEOSOFT_XML_1_0);
         }
         else if (m_iMajorVersion > 6 || (m_iMajorVersion == 6 && m_iMinorVersion >= 3))
         {
            if (m_oServerConfiguration.bSecure())
               m_oCommand.ChangeSecureConnection(true);

            if (m_oServerConfiguration.bLogin())
               m_bSecure = true;
         }
      }

      /// <summary>
      /// Load the default browser map for this server, from either the cache or the server
      /// </summary>
      protected void LoadBrowserMap()
      {
         string         strBrowserMapFile = Path.Combine(m_strCacheDir, BROWSERMAP_FILE);
         
         try
         {
            if (File.Exists(strBrowserMapFile))
            {
               m_oBrowserMap = new XmlDocument();
               m_oBrowserMap.Load(strBrowserMapFile);
            } 
            else 
            {
               Geosoft.Dap.Common.Format        oFormat = new Geosoft.Dap.Common.Format();
               Geosoft.Dap.Common.Resolution    oResolution = new Geosoft.Dap.Common.Resolution();     
               Geosoft.Dap.Common.BoundingBox   oBoundingBox = new Geosoft.Dap.Common.BoundingBox(m_oServerBoundingBox);
               
               oFormat.Type = "image/png24";

               oResolution.Width = 600;
               oResolution.Height = 600;

               
               // --- Calculate the best resolution for this image, so that it is not distorted ---

               double dWidth = oBoundingBox.MaxX - oBoundingBox.MinX;
               double dHeight = oBoundingBox.MaxY - oBoundingBox.MinY;

               if (dWidth == 0 || dHeight == 0) 
               {
                  throw new Exception("Fix this");
               }

               double dPictureRatio = oResolution.Height / oResolution.Width;
               double dBoxRatio = dHeight / dWidth;

               if (dBoxRatio > dPictureRatio)
               {
                  // --- width is smaller than it should be ---

                  oResolution.Width = Convert.ToInt32(oResolution.Height / dBoxRatio);
               } 
               else if (dBoxRatio < dPictureRatio)
               {
                  // --- height is smaller than it should be ---

                  oResolution.Height = Convert.ToInt32(oResolution.Width * dBoxRatio);
               }         
               
               m_oBrowserMap = m_oCommand.GetImageEx(oFormat, oBoundingBox, oResolution, true, false, new ArrayList(), null);  
               m_oBrowserMap.Save(strBrowserMapFile);
            } 
         }
         catch (Exception e)
         {
            m_oBrowserMap = new XmlDocument();
            GetDapError.Instance.Write("Get Server Browser Map (" + m_strUrl + ") - " + e.Message );
         }
      }
      #endregion
	}
}
