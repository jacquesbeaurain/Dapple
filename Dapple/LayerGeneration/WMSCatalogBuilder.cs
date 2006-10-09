using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using WorldWind;
using WorldWind.Net;
using WorldWind.Renderable;
using WorldWind.Net.Wms;
using Geosoft.DotNetTools;

namespace Dapple.LayerGeneration
{
   public class WMSCatalogBuilder : BuilderDirectory
   {
      #region Constants
      protected const string CATALOG_CACHE = "WMS Catalog Cache";
      #endregion

      private WorldWindow m_WorldWindow;
      private System.Collections.Hashtable m_oTable = new System.Collections.Hashtable();
      private System.Collections.Hashtable m_oServers = new System.Collections.Hashtable();
      private System.Threading.Semaphore sem = new System.Threading.Semaphore(1, 1, "builderList");

      public LoadingCompletedCallbackHandler LoadingCompleted = null;
      public LoadingFailedCallbackHandler LoadingFailed = null;

      TreeView m_serverTree;
      TriStateTreeView m_layerTree;
      LayerBuilderList m_activeList;

      public static void TrimCapabilitiesURL(ref string serverUrl)
      {
         // Clean up URL first (URL is case sensitive on some servers)
         int iIndex;
         
         string strTemp = serverUrl.ToLower();
         iIndex = strTemp.IndexOf("request=getcapabilities");
         
         if (iIndex != -1)
            serverUrl = serverUrl.Substring(0, iIndex) + serverUrl.Substring(iIndex + "request=getcapabilities".Length);

         strTemp = serverUrl.ToLower();
         iIndex = strTemp.IndexOf("service=wms");
         if (iIndex != -1)
            serverUrl = serverUrl.Substring(0, iIndex) + serverUrl.Substring(iIndex + "service=wms".Length);

         while (serverUrl.IndexOf("&&") != -1)
            serverUrl = serverUrl.Replace("&&", "&");
         serverUrl = serverUrl.TrimEnd(new char[] { '?' });
         serverUrl = serverUrl.TrimEnd(new char[] { '&' });
         serverUrl = serverUrl.Trim();
      }

      public WMSCatalogBuilder(string appDir, WorldWindow worldWindow, string strName, IBuilder parent, TreeView serverTree, TriStateTreeView layerTree, LayerBuilderList activeList)
         : base(strName, parent, false)
      {
         m_serverTree = serverTree;
         m_layerTree = layerTree;
         m_activeList = activeList;
         m_WorldWindow = worldWindow;
      }

      public bool IsFinishedLoading
      {
         get
         {
            return m_oTable.Count == 0;
         }
      }

      public BuilderDirectory AddServer(string serverUrl, BuilderDirectory builderdir)
      {
         TrimCapabilitiesURL(ref serverUrl);

         // create the cache directory
         string savePath = Path.Combine(Path.Combine(m_WorldWindow.WorldWindSettings.CachePath, CATALOG_CACHE), WMSQuadLayerBuilder.GetServerFileNameFromUrl(serverUrl));
         Directory.CreateDirectory(savePath);

         // download the catalog
         string xmlPath = Path.Combine(savePath, "capabilities.xml");
         string url = serverUrl +
            (serverUrl.IndexOf("?") > 0 ? "&" : "?") + "request=GetCapabilities&service=WMS";

         WebDownload download = new WebDownload(url);
         download.SavedFilePath = xmlPath;
         download.CompleteCallback += new DownloadCompleteHandler(CatalogDownloadCompleteCallback);

         WMSServerBuilder dir = new WMSServerBuilder(this, serverUrl, xmlPath);
         SubList.Add(dir);
         
         m_oTable.Add(download, dir);
         download.BackgroundDownloadFile();

         return dir;
      }

      public WMSList FindServer(string url)
      {
         return m_oServers[url.ToLower()] as WMSList;
      }

      public void RemoveServer(string url)
      {
         WMSList oServer = m_oServers[url.ToLower()] as WMSList;
         if (oServer != null)
            m_oServers.Remove(url.ToLower());
      }

      private void CatalogDownloadCompleteCallback(WebDownload download)
      {
         WMSServerBuilder serverDir = m_oTable[download] as WMSServerBuilder;

         if (!File.Exists(serverDir.CapabilitiesFilePath))
         {
            LoadFailed(serverDir.Name, "Could not retrieve configuration");
            return;
         }

         WMSList oServer = null;
         try
         {
            oServer = new WMSList(serverDir.URL,
                 serverDir.CapabilitiesFilePath);
            serverDir.ChangeName(oServer.Name);
         }
         catch(Exception e)
         {
            LoadFailed(serverDir.Name, e.Message);
            return;
         }

         if (oServer.Layers == null)
         {
            LoadFailed(serverDir.Name, "Could not retrieve the catalog.");
            return;
         }

         foreach (WMSLayer layer in oServer.Layers)
         {
            ProcessWMSLayer(layer, serverDir);
         }

         if (!m_oServers.Contains(serverDir.URL.ToLower()))
            m_oServers.Add(serverDir.URL.ToLower(), oServer);
         else
            oServer = m_oServers[serverDir.URL.ToLower()] as WMSList;

         lock (m_oTable.SyncRoot)
         {
            m_oTable.Remove(download);
         }
         if (LoadingCompleted != null)
         {
            LoadingCompleted(serverDir, m_serverTree, m_layerTree, m_activeList);
         }
         if (LoadingCompleted != null)
         {
            LoadingCompleted(serverDir, m_serverTree, m_layerTree, m_activeList);
         }
      }

      private void LoadFailed(string serverName, string message)
      {
         if (LoadingFailed != null)
         {
            foreach (WMSServerBuilder dir in SubList)
            {
               if (dir.Name == serverName)
               {
                  LoadingFailed(dir, message, m_serverTree, m_layerTree, m_activeList);
                  break;
               }
            }            
         }
      }

      private void ProcessWMSLayer(WMSLayer layer, BuilderDirectory directory)
      {
         if (layer.ChildLayers != null)
         {
            BuilderDirectory childDir = new BuilderDirectory(layer.Title, directory, false);
            directory.SubList.Add(childDir);

            foreach (WMSLayer childLayer in layer.ChildLayers)
            {
               ProcessWMSLayer(childLayer, childDir);
            }
         }
         else
         {
            string imageFormat = "image/png";
            foreach (string curFormat in layer.ImageFormats)
            {
               if (string.Compare(curFormat, "image/png", true, System.Globalization.CultureInfo.InvariantCulture) == 0)
               {
                  imageFormat = curFormat;
                  break;
               }
               if (string.Compare(curFormat, "image/jpeg", true, System.Globalization.CultureInfo.InvariantCulture) == 0 ||
                  String.Compare(curFormat, "image/jpg", true, System.Globalization.CultureInfo.InvariantCulture) == 0)
               {
                  imageFormat = curFormat;
               }
            }

            WMSLayerAccessor accessor = new WMSLayerAccessor(imageFormat, true, layer.ParentWMSList.ServerGetMapUrl,
               layer.ParentWMSList.Version, layer.Name, string.Empty, layer.SRS, layer.CRS);

            IBuilder parentServer = directory;
            while (parentServer != null && !(parentServer is WMSServerBuilder))
               parentServer = parentServer.Parent;

            WMSQuadLayerBuilder builder = new WMSQuadLayerBuilder(layer,
               0, true, new GeographicBoundingBox(Convert.ToDouble(layer.North), Convert.ToDouble(layer.South)
               , Convert.ToDouble(layer.West), Convert.ToDouble(layer.East)), accessor, true, m_WorldWindow.CurrentWorld,
               m_WorldWindow.WorldWindSettings.CachePath, parentServer as WMSServerBuilder, directory);
            //WMSZoomBuilder builder = new WMSZoomBuilder(layer, m_WorldWindow.WorldWindSettings.CachePath, m_WorldWindow, directory);
            sem.WaitOne();
            directory.LayerBuilders.Add(builder);
            sem.Release();
         }

      }
   }

   public class WMSServerBuilder : ServerBuilder
   {
      string m_strCapabilitiesFilePath;

      public WMSServerBuilder(IBuilder parent, string url, string CapabilitiesFilePath)
         : base(url, parent, url)
      {
         m_strCapabilitiesFilePath = CapabilitiesFilePath;
      }

      public string CapabilitiesFilePath
      {
         get
         {
            return m_strCapabilitiesFilePath;
         }
         set
         {
            m_strCapabilitiesFilePath = value;
         }
      }

      public override bool SupportsMetaData
      {
         get
         {
            return File.Exists(m_strCapabilitiesFilePath);
         }
      }

      public override XmlNode GetMetaData(XmlDocument oDoc)
      {
         XmlDocument responseDoc = new XmlDocument();
         responseDoc.Load(m_strCapabilitiesFilePath);
         XmlNode oNode = responseDoc.DocumentElement;
         XmlNode newNode = oDoc.CreateElement(oNode.Name);
         newNode.InnerXml = oNode.InnerXml;
         return newNode;
      }

      public override string StyleSheetName
      {
         get
         {
            return "wms_cap_meta.xslt";
         }
      }

      public WMSQuadLayerBuilder FindLayerBuilder(WMSLayer layer, BuilderDirectory dir)
      {
         foreach (WMSQuadLayerBuilder layerBuilder in dir.LayerBuilders)
         {
            if (layerBuilder.m_wmsLayer == layer)
               return layerBuilder;
         }

         foreach (BuilderDirectory subdir in dir.SubList)
         {
            WMSQuadLayerBuilder layerBuilderSub = FindLayerBuilder(layer, subdir);
            if (layerBuilderSub != null)
               return layerBuilderSub;
         }

         return null;
      }

      public WMSQuadLayerBuilder FindLayerBuilder(WMSLayer layer)
      {
         return FindLayerBuilder(layer, this);
      }
   }
}
