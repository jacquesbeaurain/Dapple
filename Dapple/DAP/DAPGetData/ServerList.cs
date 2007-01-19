using System;
using System.IO;
using System.Xml;
using System.Collections;

using Geosoft;
#if !DAPPLE
using Geosoft.GXNet;
#endif

namespace Geosoft.GX.DAPGetData
{
   /// <summary>
   /// Store a list of all the servers
   /// </summary>
   public class ServerList
   {
      #region Member Variables
      protected string m_strCSV;
#if DAPPLE
      protected string m_strCacheDir;
#endif
      protected ArrayList m_oServerList;
      #endregion

      #region Properties
      /// <summary>
      /// Get the list of servers
      /// </summary>
      public ArrayList Servers
      {
         get { return new ArrayList(m_oServerList); }
         set { m_oServerList = new ArrayList(value); }
      }
      #endregion

      #region Constructor
#if !DAPPLE
      /// <summary>
      /// Default constructor
      /// </summary>
      public ServerList()
      {         
         m_oServerList = new ArrayList();         
      }      
#else
      /// <summary>
      /// Default constructor
      /// </summary>
      public ServerList(string cacheDir)
      {
         m_oServerList = new ArrayList();

         m_strCacheDir = cacheDir;
      }
#endif
      #endregion

      #region Public Methods
#if !DAPPLE
      public void Load(string strSecureToken)
      {
         string strDir = string.Empty;


         CSYS.IGetDirectory(GXNet.Constant.SYS_DIR_USER, ref strDir);
         m_strCSV = System.IO.Path.Combine(strDir, "csv\\Dap_Servers.xml");

         if (!System.IO.File.Exists(m_strCSV))
         {
            string strTemp;

            CSYS.IGetDirectory(GXNet.Constant.SYS_DIR_GEOSOFT, ref strDir);
            strTemp = System.IO.Path.Combine(strDir, "csv\\Dap_Servers.xml");


            // --- Copy file to user directory ---

            if (System.IO.File.Exists(strTemp))
               System.IO.File.Copy(strTemp, m_strCSV, true);
         }

         UpgradeList();
         LoadInternal(strSecureToken);
      }
#endif
      #endregion

      #region Protected Members
#if !DAPPLE
      /// <summary>
      /// Load the server list into memory
      /// </summary>
      protected void LoadInternal(string strSecureToken)
      {
         XmlDocument oDoc;
         XmlNode oRoot;


         // --- make sure this file exists ---

         if (System.IO.File.Exists(m_strCSV))
         {
            oDoc = new XmlDocument();
            oDoc.Load(m_strCSV);

            oRoot = oDoc.DocumentElement;

            foreach (XmlNode oServerNode in oRoot.ChildNodes)
            {
               try
               {
                  Server oServer = new Server(oServerNode, strSecureToken);

                  // --- only add the server to the list if there was no errors in reading it from the xml ---

                  if (VerifyServerUnique(oServer.Url))
                     m_oServerList.Add(oServer);
               }
               catch
               {
               }
            }
         }
      }

      protected class ServerUpgradeInfo
      {
         public string Name;
         public int iMajorVersion;
         public int iMinorVersion;
         public Server.ServerStatus eStatus;

         public ServerUpgradeInfo(string _Name, int _iMajorVersion, int _iMinorVersion, Server.ServerStatus _eStatus)
         {
            Name = _Name;
            iMajorVersion = _iMajorVersion;
            iMinorVersion = _iMinorVersion;
            eStatus = _eStatus;
         }
      }

      /// <summary>
      /// Update with any required new servers on upgrade (6.4.0 first implemented)
      /// </summary>
      protected void UpgradeList()
      {
         System.Collections.Generic.Dictionary<string, ServerUpgradeInfo> URLs640 = new System.Collections.Generic.Dictionary<string, ServerUpgradeInfo>();

         URLs640.Add("http://gdrdap.agg.nrcan.gc.ca", new ServerUpgradeInfo("Geoscience Data Repository Server", 6, 3, Server.ServerStatus.Disabled));

         UpgradeList(6, 4, 0, URLs640);
      }

      /// <summary>
      /// Update with any required new servers on upgrades
      /// </summary>
      protected void UpgradeList(int iMajorVersion, int iMinorVersion, int iSPVersion, System.Collections.Generic.Dictionary<string, ServerUpgradeInfo> URLs)
      {
         XmlDocument oDoc;
         XmlNode oNode, oRoot;
         XmlAttribute oAttr;
         
         try
         {
            string strDir = string.Empty;
            CSYS.IGetDirectory(GXNet.Constant.SYS_DIR_USER, ref strDir);
            string strUpdated = System.IO.Path.Combine(strDir, string.Format("csv\\Dap_Servers{0}{1}{2}.updated", iMajorVersion, iMinorVersion, iSPVersion));

            // --- Only update once per version ---
            if (!System.IO.File.Exists(strUpdated))
            {
               bool bFound;

               oDoc = new XmlDocument();
               oDoc.Load(m_strCSV);
               oRoot = oDoc.DocumentElement;


               // --- See if the URL has not been added manually already ---
               foreach (string strURL in URLs.Keys)
               {
                  bFound = false;
                  foreach (XmlNode oServerNode in oRoot.ChildNodes)
                  {
                     oNode = oServerNode.Attributes.GetNamedItem(Constant.Xml.Attr.Url);
                     if (oNode == null) throw new ApplicationException("Missing url attribute in server node");
                     if (String.Compare(oNode.Value, strURL, true) == 0)
                     {
                        bFound = true;
                        break;
                     }
                  }
                  if (!bFound)
                  {
                     // --- Add the server ---

                     XmlNode oServerNode = oRoot.OwnerDocument.CreateElement(Constant.Xml.Tag.Server);

                     oAttr = oRoot.OwnerDocument.CreateAttribute(Constant.Xml.Attr.Name);
                     oAttr.Value = URLs[strURL].Name;
                     oServerNode.Attributes.Append(oAttr);

                     oAttr = oRoot.OwnerDocument.CreateAttribute(Constant.Xml.Attr.Url);
                     oAttr.Value = strURL;
                     oServerNode.Attributes.Append(oAttr);

                     oAttr = oRoot.OwnerDocument.CreateAttribute(Constant.Xml.Attr.Status);
                     oAttr.Value = URLs[strURL].eStatus.ToString();
                     oServerNode.Attributes.Append(oAttr);

                     oAttr = oRoot.OwnerDocument.CreateAttribute(Constant.Xml.Attr.Major_Version);
                     oAttr.Value = URLs[strURL].iMajorVersion.ToString();
                     oServerNode.Attributes.Append(oAttr);

                     oAttr = oRoot.OwnerDocument.CreateAttribute(Constant.Xml.Attr.Minor_Version);
                     oAttr.Value = URLs[strURL].iMinorVersion.ToString();
                     oServerNode.Attributes.Append(oAttr);

                     oAttr = oRoot.OwnerDocument.CreateAttribute(Constant.Xml.Attr.CacheVersion);
                     oAttr.Value = "";
                     oServerNode.Attributes.Append(oAttr);

                     oRoot.AppendChild(oServerNode);
                  }
               }

               oDoc.Save(m_strCSV);

               // --- Only update once per version ---
               using (StreamWriter sw = new StreamWriter(strUpdated))
                  sw.WriteLine();
            }
         }
         catch
         {
         }
      }
#endif

      /// <summary>
      /// Verify that this server url is not already in the list
      /// </summary>
      /// <param name="strUrl"></param>
      /// <returns></returns>
      protected bool VerifyServerUnique(string strUrl)
      {
         if (strUrl == null) throw new ArgumentNullException("strUrl");

         foreach (Server oServer in m_oServerList)
         {
            if (oServer.Url == strUrl)
               return false;
         }
         return true;
      }
      #endregion

      #region Public Members
      /// <summary>
      /// Save the list of servers to xml
      /// </summary>
      /// <returns></returns>
      public void Save()
      {
         XmlDocument oDoc;
         XmlElement oRoot;

         oDoc = new XmlDocument();

         oRoot = oDoc.CreateElement(Constant.Xml.Tag.Geosoft_Xml);
         oDoc.AppendChild(oRoot);

         foreach (Server oServer in m_oServerList)
         {
            oServer.Save(oRoot);
         }
         oDoc.Save(m_strCSV);
      }

      /// <summary>
      /// Add a new server to the list
      /// </summary>
      /// <param name="oServer">
      /// </param>
      /// <returns>
      /// </returns>
      /// <exception cref="ArgumentNullException">
      /// 	<para>The argument <paramref name="oServer"/> is <langword name="null"/>.</para>
      /// </exception>
      public bool AddServer(Server oServer)
      {
         if (oServer == null) throw new ArgumentNullException("oServer");

         if (VerifyServerUnique(oServer.Url))
         {
            oServer.Index = m_oServerList.Count;
            m_oServerList.Add(oServer);
            return true;
         }

         return false;
      }

      /// <summary>
      /// Remove a server from the list
      /// </summary>
      /// <param name="hServer">
      /// </param>
      /// <returns>
      /// </returns>
      /// <param name="oServer">
      /// </param>
      /// <exception cref="ArgumentNullException">
      /// 	<para>The argument <paramref name="oServer"/> is <langword name="null"/>.</para>
      /// </exception>
      public bool RemoveServer(Server oServer)
      {
         bool bRemoved = false;

         if (oServer == null) throw new ArgumentNullException("oServer");

         for (int i = 0; i < m_oServerList.Count; i++)
         {
            Server oS = (Server)m_oServerList[i];

            if (oS.Url == oServer.Url)
            {
               m_oServerList.RemoveAt(i);
               bRemoved = true;
               break;
            }
         }
         return bRemoved;
      }

      /// <summary>
      /// Clear the list
      /// </summary>
      public void Clear()
      {
         m_oServerList.Clear();
      }

      /// <summary>
      /// Find a server based on its url
      /// </summary>
      /// <param name="strUrl">
      /// </param>
      /// <returns>
      /// </returns>
      /// <exception cref="ArgumentNullException">
      /// 	<para>The argument <paramref name="strUrl"/> is <langword name="null"/>.</para>
      /// </exception>
      public Server FindServer(string strUrl)
      {
         if (strUrl == null) throw new ArgumentNullException("strUrl");

         for (int i = 0; i < m_oServerList.Count; i++)
         {
            Server oS = (Server)m_oServerList[i];

            if (oS.Url == strUrl)
            {
               return oS;
            }
         }
         return null;
      }
      #endregion
   }
}
