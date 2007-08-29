using System;
using System.Collections.Generic;
using System.Text;
using WorldWind;
using Utility;
using WorldWind.Net;
using System.IO;
using WorldWind.PluginEngine;
using System.Xml;
using System.Collections;

namespace Dapple.LayerGeneration
{
   public class ArcIMSCatalogBuilder : BuilderDirectory
   {
      #region Constants
      protected const string CATALOG_CACHE = "ArcIMS Catalog Cache";
      #endregion

      private WorldWindow m_oWorldWindow;
      private System.Collections.Hashtable m_oCatalogDownloadsInProgress = new System.Collections.Hashtable();
      private System.Collections.Hashtable m_oServers = new System.Collections.Hashtable();
      private int m_iIndexGenerator = 0;

      public ServerTree.LoadFinishedCallbackHandler LoadFinished = null;

      private int m_iServerLayerImageIndex, m_iServerDirImageIndex;

      public ArcIMSCatalogBuilder(String strName, WorldWindow oWorldWindow, IBuilder parent, int iLayerImageIndex, int iDirectoryImageIndex, int iSLI, int iSDI)
         : base(strName, parent, false, iLayerImageIndex, iDirectoryImageIndex)
      {
         m_iServerDirImageIndex = iSDI;
         m_iServerLayerImageIndex = iSLI;
         m_oWorldWindow = oWorldWindow;
      }

      public BuilderDirectory AddServer(ArcIMSServerUri oUri)
      {
         // create the cache directory
         String savePath = Path.Combine(Path.Combine(MainApplication.Settings.CachePath, CATALOG_CACHE), oUri.ToCacheDirectory());
         Directory.CreateDirectory(savePath);

         // download the catalog
         String xmlPath = Path.Combine(savePath, "__catalog.xml");

         ArcIMSCatalogDownload download = new ArcIMSCatalogDownload(oUri, m_iIndexGenerator);
         m_iIndexGenerator++;
         download.SavedFilePath = xmlPath;
         download.CompleteCallback += new DownloadCompleteHandler(CatalogDownloadCompleteCallback);

         BuilderDirectory dir = new ArcIMSServerBuilder(this, oUri, xmlPath, m_iServerLayerImageIndex, m_iServerDirImageIndex);
         SubList.Add(dir);

         m_oCatalogDownloadsInProgress.Add(download.IndexNumber, dir);
         download.BackgroundDownloadFile();

         return dir;
      }

      public bool ContainsServer(ArcIMSServerUri oUri)
      {
         foreach (ArcIMSServerBuilder builder in m_colSublist)
         {
            if (builder.Uri.Equals(oUri))
               return true;
         }
         return false;
      }

      public void UncacheServer(ArcIMSServerUri oUri)
      {
         m_oServers.Remove(oUri);
      }

      public ArrayList GetServers()
      {
         ArrayList result = new ArrayList();

         foreach (ArcIMSServerBuilder iter in m_colSublist)
         {
            if (iter.IsLoadedSuccessfully)
               result.Add(iter);
         }

         return result;
      }

      public void cancelDownloads()
      {
         lock (m_oCatalogDownloadsInProgress.SyncRoot)
         {
            foreach (WebDownload oDownload in m_oCatalogDownloadsInProgress.Values)
            {
               oDownload.Cancel();
               oDownload.Dispose();
            }
         }
      }

      private void CatalogDownloadCompleteCallback(WebDownload download)
      {
         try
         {
            ArcIMSServerBuilder serverDir = m_oCatalogDownloadsInProgress[((ArcIMSCatalogDownload)download).IndexNumber] as ArcIMSServerBuilder;

            if (!File.Exists(serverDir.CatalogFilename))
            {
               serverDir.SetLoadFailed("Could not retrieve catalog");
               return;
            }

            if (new FileInfo(serverDir.CatalogFilename).Length == 0)
            {
               serverDir.SetLoadFailed("Could not retrieve catalog");
               File.Delete(serverDir.CatalogFilename);
               return;
            }

            XmlDocument hCatalog = new XmlDocument();
            try
            {
               hCatalog.Load(serverDir.CatalogFilename);
            }
            catch (XmlException)
            {
               serverDir.SetLoadFailed("Could not retrieve catalog");
               return;
            }

            XmlNodeList oNodeList = hCatalog.SelectNodes("/ARCXML/RESPONSE/SERVICES/SERVICE[@type=\"ImageServer\" and @access=\"PUBLIC\" and @status=\"ENABLED\"]");
            if (oNodeList.Count == 0)
            {
               serverDir.SetLoadFailed("Server has no public, enabled image services.");
               return;
            }

            foreach (XmlNode nServiceNode in oNodeList)
            {
               ProcessArcIMSService(serverDir.Uri as ArcIMSServerUri, nServiceNode as XmlElement, serverDir);
            }

            serverDir.SetLoadSuccessful();
         }
         finally
         {
            if (LoadFinished != null) LoadFinished();

            lock (m_oCatalogDownloadsInProgress.SyncRoot)
            {
               m_oCatalogDownloadsInProgress.Remove(((ArcIMSCatalogDownload)download).IndexNumber);
               // dispose the download to release the catalog file's lock
               download.IsComplete = true;
               download.Dispose();
            }
         }
      }

      private void ProcessArcIMSService(ArcIMSServerUri serviceUri, XmlElement nServiceNode, BuilderDirectory directory)
      {
         //TODO: Get acutal layer bounds, not placeholder ones
         ArcIMSQuadLayerBuilder builder = new ArcIMSQuadLayerBuilder(serviceUri, nServiceNode.GetAttribute("name"), new GeographicBoundingBox(90, -90, -180, 180), m_oWorldWindow, directory);
         directory.LayerBuilders.Add(builder);
      }
   }

   public class ArcIMSServerBuilder : AsyncServerBuilder
   {
      string m_strCatalogPathname;

      public ArcIMSServerBuilder(IBuilder parent, ArcIMSServerUri oUri, string strCatalogPathname, int iLayerImageIndex, int iDirectoryImageIndex)
         : base(oUri.ToBaseUri(), parent, oUri, iLayerImageIndex, iDirectoryImageIndex)
      {
         m_strCatalogPathname = strCatalogPathname;
      }

      public string CatalogFilename
      {
         get
         {
            return m_strCatalogPathname;
         }
         set
         {
            m_strCatalogPathname = value;
         }
      }

      public override bool SupportsMetaData
      {
         get
         {
            return false;
         }
      }

      public override string StyleSheetName
      {
         get
         {
            return String.Empty;
         }
      }

      public override string Type
      {
         get { return "ArcIMS"; }
      }
   }
}
