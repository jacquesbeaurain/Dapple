using System;
using System.Threading;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Windows.Forms;
using WorldWind;
using Geosoft;
using Geosoft.GX;
using Geosoft.Dap.Common;
using Geosoft.GX.DAPGetData;
using Geosoft.DotNetTools;

namespace Dapple.LayerGeneration
{
   public class DAPCatalogBuilder : BuilderDirectory
   {
      private string m_strCacheDir;
      private World m_oWorld;
      private GetDapError m_Error;
      private ServerList m_oServers;
      private System.Collections.Hashtable m_oDirTable = new System.Collections.Hashtable();

      public LoadingCompletedCallbackHandler LoadingCompleted = null;
      public LoadingFailedCallbackHandler LoadingFailed = null;

      TreeView m_serverTree;
      TriStateTreeView m_layerTree;
      LayerBuilderList m_activeList;

      public DAPCatalogBuilder(string cacheDir, World world, string strName, IBuilder parent, TreeView serverTree, TriStateTreeView layerTree, LayerBuilderList activeList)
         : base(strName, parent, false)
      {
         m_serverTree = serverTree;
         m_layerTree = layerTree;
         m_activeList = activeList;
         m_strCacheDir = cacheDir;
         m_oWorld = world;
         
         m_oServers = new ServerList(false, cacheDir, string.Empty);

         m_Error = GetDapError.Instance;
         if (m_Error == null)
            m_Error = new GetDapError(Path.Combine(m_strCacheDir, "DapErrors.log"));
      }

      public override object Clone()
      {
         DAPCatalogBuilder clone = new DAPCatalogBuilder(m_strCacheDir, m_oWorld, Name, Parent, m_serverTree, m_layerTree, m_activeList);
         clone.m_oServers.Servers = this.m_oServers.Servers;
         clone.LoadingCompleted += this.LoadingCompleted;
         clone.LoadingFailed += this.LoadingFailed;
         return clone;
      }

      public ServerBuilder AddDapServer(Server oServer, Geosoft.Dap.Common.BoundingBox extents, IBuilder parent)
      {
         // if the server is already in the list, return it
         if (!m_oServers.AddServer(oServer))
         {
            return m_oDirTable[oServer.Url] as ServerBuilder;
         }
         // otherwise create it
         ServerBuilder dir = new DAPServerBuilder(oServer, extents, parent);
         SubList.Add(dir);
         m_oDirTable.Add(oServer.Url, dir);
         System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(PopulateBuilderList), dir);
         return dir;
      }

      public Server FindServer(string url)
      {
         return m_oServers.FindServer(url);
      }

      public void RemoveServer(string url)
      {
         Server oServer = m_oServers.FindServer(url);
         if (oServer != null)
         {
            m_oDirTable.Remove(oServer.Url);
            m_oServers.RemoveServer(oServer);
         }
      }

      public TreeView ServerTree
      {
         set
         {
            m_serverTree = value;
         }
      }

      private void PopulateBuilderList(object stateInfo)
      {
#if !DEBUG
         try
         {
#endif
            string strConfigEdition, strEdition;
            DAPServerBuilder oDir = stateInfo as DAPServerBuilder;

            //if (!strServerUrl.StartsWith("http://"))
            //   strServerUrl = "http://" + strServerUrl;


            Server oServer = oDir.Server;

            if (oServer.Status != Server.ServerStatus.OnLine &&
                oServer.Status != Server.ServerStatus.Maintenance)
            {
               LoadFailed(oServer.Url, "Server Offline");
               return;
            }

            if (oServer.Secure && !oServer.LoggedIn)
            {
               if (!oServer.Login()) //-- Login.cs uses GetDapData.Instance
               {
                  LoadFailed(oServer.Url, "Could not log in to server");
                  return;
               }
            }

            try
            {
               oServer.Command.GetCatalogEdition(out strConfigEdition, out strEdition, null);
            }
            catch
            {
               strEdition = "Default";
            }


            System.Xml.XmlDocument oDoc;
            Catalog oCatalog;
            try
            {
               oDoc = oServer.Command.GetCatalog(String.Empty, -1, 0, 0, string.Empty, oDir.FilterExtents, null);
               oServer.Command.Parser.PruneCatalog(oDoc);
               oCatalog = new Catalog(oDoc, strEdition);
            }
            catch (Exception e)
            {
               LoadFailed(oServer.Url, e.Message);
               return;
            }

            XmlNodeList list = oCatalog.Document.DocumentElement.SelectNodes("//" + Geosoft.Dap.Xml.Common.Constant.Tag.CATALOG_TAG);
            foreach (XmlNode childNode in list[0])
            {
               PopulateBuilderListHelp(childNode, oDir, oServer);
            }
            SubList.Add(oDir);

            if (LoadingCompleted != null)
               LoadingCompleted(oDir, m_serverTree, m_layerTree, m_activeList);
#if !DEBUG
         }
         catch (Exception e)
         {
            Utility.AbortUtility.Abort(e, Thread.CurrentThread);
         }
#endif
      }

      private void PopulateBuilderListHelp(System.Xml.XmlNode xmlNode, BuilderDirectory parentDir, Server oServer)
      {
         System.Xml.XmlNode hAttr = xmlNode.Attributes.GetNamedItem(Geosoft.Dap.Xml.Common.Constant.Attribute.NAME_ATTR);

         if (hAttr == null) return;

         if (xmlNode.Name == Geosoft.Dap.Xml.Common.Constant.Tag.COLLECTION_TAG)
         {
            BuilderDirectory dir = new BuilderDirectory(hAttr.Value, parentDir, false);
            foreach (System.Xml.XmlNode hChildNode in xmlNode.ChildNodes)
            {
               PopulateBuilderListHelp(hChildNode, dir, oServer);
            }
            parentDir.SubList.Add(dir);
         }
         else
         {
            Geosoft.Dap.Common.DataSet hDataSet;
            oServer.Command.Parser.DataSet(xmlNode, out hDataSet);
            parentDir.LayerBuilders.Add(new DAPQuadLayerBuilder(hDataSet,
                m_oWorld, m_strCacheDir, oServer, parentDir));
         }
      }

      private void LoadFailed(string serverURL, string message)
      {
         if (LoadingFailed != null)
         {
            LoadingFailed(m_oDirTable[serverURL] as BuilderDirectory, message, m_serverTree, m_layerTree, m_activeList);
         }
      }

      public string CacheDir
      {
         get
         {
            return m_strCacheDir;
         }
      }
   }

   public class DAPServerBuilder : ServerBuilder 
   {
      Server m_oServer;
      Geosoft.Dap.Common.BoundingBox m_oFilterExtents;

      public DAPServerBuilder(Server server, Geosoft.Dap.Common.BoundingBox extents, IBuilder parent)
         : base(server.Name, parent, server.Url)
      {
         m_oServer = server;
         m_oFilterExtents = extents;
      }

      public Server Server
      {
         get
         {
            return m_oServer;
         }
      }

      public Geosoft.Dap.Common.BoundingBox FilterExtents
      {
         get
         {
            return m_oFilterExtents;
         }
         set
         {
            m_oFilterExtents = value;
         }
      }

      public DAPQuadLayerBuilder FindLayerBuilder(DataSet hDataSet, BuilderDirectory dir)
      {
         foreach (DAPQuadLayerBuilder layerBuilder in dir.LayerBuilders)
         {
            if (layerBuilder.m_hDataSet == hDataSet)
               return layerBuilder;
         }

         foreach (BuilderDirectory subdir in dir.SubList)
         {
            DAPQuadLayerBuilder layerBuilderSub = FindLayerBuilder(hDataSet, subdir);
            if (layerBuilderSub != null)
               return layerBuilderSub;
         }

         return null;
      }

      public DAPQuadLayerBuilder FindLayerBuilder(DataSet hDataSet)
      {
         return FindLayerBuilder(hDataSet, this);
      }
   }
}
