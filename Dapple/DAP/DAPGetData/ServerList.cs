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
