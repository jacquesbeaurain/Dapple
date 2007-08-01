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

using WorldWind.PluginEngine;

using Geosoft.DotNetTools;

namespace Dapple.LayerGeneration
{
   public class WMSCatalogBuilder : BuilderDirectory
   {
      #region Constants
      protected const string CATALOG_CACHE = "WMS Catalog Cache";
      #endregion

      private WorldWindow m_WorldWindow;
      private System.Collections.Hashtable m_oCatalogDownloadsInProgress = new System.Collections.Hashtable(); //<WebDownload, WMSServerBuilder>>
      private System.Collections.Hashtable m_oWMSListCache = new System.Collections.Hashtable(); //<WMSUri, WMSList>>

      public ServerTree.LoadFinishedCallbackHandler LoadFinished = null;

      private int m_iServerLayerImageIndex, m_iServerDirImageIndex;

      public WMSCatalogBuilder(String strName, WorldWindow worldWindow, IBuilder parent, int iLayerImageIndex, int iDirectoryImageIndex, int iSLI, int iSDI)
         : base(strName, parent, false, iLayerImageIndex, iDirectoryImageIndex)
      {
         m_WorldWindow = worldWindow;
         m_iServerDirImageIndex = iSDI;
         m_iServerLayerImageIndex = iSLI;
      }

      /// <summary>
      /// Add a WMS server to this catalog builder.
      /// </summary>
      /// <param name="oUri">The URI of the server.</param>
      /// <returns>A WMSServerBuilder for the server added.</returns>
      public BuilderDirectory AddServer(WMSServerUri oUri)
      {
         // create the cache directory
         string savePath = Path.Combine(Path.Combine(MainApplication.Settings.CachePath, CATALOG_CACHE), oUri.ToCacheDirectory());
         Directory.CreateDirectory(savePath);

         // queue up capabilities download
         string xmlPath = Path.Combine(savePath, "capabilities.xml");

         WebDownload download = new WebDownload(oUri.ToCapabilitiesUri(), true);
         download.SavedFilePath = xmlPath;
         download.CompleteCallback += new DownloadCompleteHandler(CatalogDownloadCompleteCallback);

         // add a child node
         WMSServerBuilder dir = new WMSServerBuilder(this, oUri, xmlPath, m_iServerLayerImageIndex, m_iServerDirImageIndex);
         SubList.Add(dir);
         
         m_oCatalogDownloadsInProgress.Add(download, dir);
         download.BackgroundDownloadFile();

         return dir;
      }

      public bool ContainsServer(WMSServerUri oUri)
      {
         foreach (WMSServerBuilder iter in m_colSublist)
         {
            if (iter.Uri.Equals(oUri))
               return true;
         }
         return false;
      }

      public WMSServerBuilder GetServer(WMSServerUri oUri)
      {
         foreach (WMSServerBuilder iter in m_colSublist)
         {
            if (iter.Uri.Equals(oUri))
               return iter;
         }
         return null;
      }

      public void UncacheServer(WMSServerUri oUri)
      {
         m_oWMSListCache.Remove(oUri);
      }

      private void CatalogDownloadCompleteCallback(WebDownload download)
      {
         try
         {
            WMSServerBuilder serverDir = m_oCatalogDownloadsInProgress[download] as WMSServerBuilder;

            if (!File.Exists(serverDir.CapabilitiesFilePath))
            {
               serverDir.SetLoadFailed("Could not retrieve configuration");
               return;
            }

            WMSList oServer = null;
            try
            {
               oServer = new WMSList(serverDir.Uri.ToBaseUri(), serverDir.CapabilitiesFilePath);
               serverDir.ChangeName(oServer.Name);
            }
            catch (Exception e)
            {
               serverDir.SetLoadFailed(e.Message);
               return;
            }

            if (oServer.Layers == null)
            {
               serverDir.SetLoadFailed("Could not retrieve the catalog.");
               return;
            }

            foreach (WMSLayer layer in oServer.Layers)
            {
               // Each server's layers are compacted into one root parent layer; don't add that root to the tree
               foreach (WMSLayer actualLayer in layer.ChildLayers)
                  ProcessWMSLayer(actualLayer, serverDir);
            }
            if (!m_oWMSListCache.Contains(serverDir.Uri as WMSServerUri))
               m_oWMSListCache.Add(serverDir.Uri, oServer);

            serverDir.List = oServer;

            lock (m_oCatalogDownloadsInProgress.SyncRoot)
            {
               m_oCatalogDownloadsInProgress.Remove(download);
            }

            serverDir.SetLoadSuccessful();
         }
         finally
         {
            if (LoadFinished != null)
               LoadFinished();
         }
      }

      private void ProcessWMSLayer(WMSLayer layer, BuilderDirectory directory)
      {
         if (layer.ChildLayers != null)
         {
            BuilderDirectory childDir = new BuilderDirectory(layer.Title, directory, false, m_iServerLayerImageIndex, m_iServerDirImageIndex);
            directory.SubList.Add(childDir);

            foreach (WMSLayer childLayer in layer.ChildLayers)
            {
               ProcessWMSLayer(childLayer, childDir);
            }
         }
         else
         {
            IBuilder parentServer = directory;
            while (parentServer != null && !(parentServer is WMSServerBuilder))
               parentServer = parentServer.Parent;

            WMSQuadLayerBuilder builder = new WMSQuadLayerBuilder(layer, m_WorldWindow.CurrentWorld, parentServer as WMSServerBuilder, directory);
            directory.LayerBuilders.Add(builder);
         }
      }
   }

   public class WMSServerBuilder : ServerBuilder
   {
      string m_strCapabilitiesFilePath;
      WMSList m_oList;

      public WMSServerBuilder(IBuilder parent, WMSServerUri oUri, string CapabilitiesFilePath, int iLayerImageIndex, int iDirectoryImageIndex)
         : base(oUri.ToBaseUri(), parent, oUri, iLayerImageIndex, iDirectoryImageIndex)
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
         XmlReaderSettings oSettings = new System.Xml.XmlReaderSettings();
         oSettings.IgnoreWhitespace = true;
         oSettings.ProhibitDtd = false;
         oSettings.XmlResolver = null;
         oSettings.ValidationType = ValidationType.None;
         using (XmlReader oResponseXmlStream = XmlReader.Create(m_strCapabilitiesFilePath, oSettings))
         {
            responseDoc.Load(oResponseXmlStream);
         }
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

      public WMSList List
      {
         get { return m_oList; }
         set { m_oList = value; }
      }
   }
}
