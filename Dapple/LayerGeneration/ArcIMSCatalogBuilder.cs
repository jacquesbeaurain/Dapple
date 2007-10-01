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
using System.Windows.Forms;

namespace Dapple.LayerGeneration
{
   public class ArcIMSCatalogBuilder : BuilderDirectory
   {
      #region Constants
      public const string CATALOG_CACHE = "ArcIMS Catalog Cache";
      #endregion

      private WorldWindow m_oWorldWindow;
      private System.Collections.Hashtable m_oCatalogDownloadsInProgress = new System.Collections.Hashtable();
      private System.Collections.Hashtable m_oServers = new System.Collections.Hashtable();
      private int m_iIndexGenerator = 0;

      public ServerTree.LoadFinishedCallbackHandler LoadFinished = null;

      public ArcIMSCatalogBuilder(String strName, WorldWindow oWorldWindow, IBuilder parent)
         : base(strName, parent, false)
      {
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

         BuilderDirectory dir = new ArcIMSServerBuilder(this, oUri, xmlPath);
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
               serverDir.SubList.Add(new ArcIMSServiceBuilder(serverDir, ((XmlElement)nServiceNode).GetAttribute("name"), LoadFinished));
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
   }

   public class ArcIMSServerBuilder : AsyncBuilder
   {
      string m_strCatalogPathname;

      public ArcIMSServerBuilder(IBuilder parent, ArcIMSServerUri oUri, string strCatalogPathname)
         : base(oUri.ToBaseUri(), parent, oUri)
      {
         m_strCatalogPathname = strCatalogPathname;
      }

      [System.ComponentModel.Browsable(false)]
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

      [System.ComponentModel.Browsable(false)]
      public override string StyleSheetName
      {
         get
         {
            return String.Empty;
         }
      }

      [System.ComponentModel.Browsable(false)]
      public override System.Drawing.Icon Icon
      {
         get { return Dapple.Properties.Resources.arcims; }
      }
   }


   public class ArcIMSServiceBuilder : AsyncBuilder
   {
      private String m_szName;
      private Object m_oLock = new Object();
      private ArcIMSServiceDownload m_hDownload;
      private ServerTree.LoadFinishedCallbackHandler LoadFinished;

      public ArcIMSServiceBuilder(ArcIMSServerBuilder hServer, String szName, ServerTree.LoadFinishedCallbackHandler hLoadFinished) : base(szName, hServer, hServer.Uri)
      {
         m_szName = szName;
         LoadFinished = hLoadFinished;
      }

      public override System.Windows.Forms.TreeNode[] getChildTreeNodes()
      {
         lock (m_oLock)
         {
            if (m_hDownload == null)
            {
               // create the cache directory
               String szSavePath = Path.Combine(Path.Combine(MainApplication.Settings.CachePath, ArcIMSCatalogBuilder.CATALOG_CACHE), this.Uri.ToCacheDirectory());
               Directory.CreateDirectory(szSavePath);

               // download the catalog
               String szXmlPath = Path.Combine(Path.Combine(szSavePath, m_szName), "__catalog.xml");

               m_hDownload = new ArcIMSServiceDownload(this.Uri as ArcIMSServerUri, m_szName, 0);
               m_hDownload.SavedFilePath = szXmlPath;
               m_hDownload.CompleteCallback += new DownloadCompleteHandler(CatalogDownloadCompleteCallback);
               m_hDownload.BackgroundDownloadFile();
            }
         }

         return base.getChildTreeNodes();
      }

      private void CatalogDownloadCompleteCallback(WebDownload hDownload)
      {
         try
         {

            if (!File.Exists(hDownload.SavedFilePath))
            {
               SetLoadFailed("Could not retrieve layer list");
               return;
            }

            if (new FileInfo(hDownload.SavedFilePath).Length == 0)
            {
               SetLoadFailed("Could not retrieve layer list");
               File.Delete(hDownload.SavedFilePath);
               return;
            }

            XmlDocument hCatalog = new XmlDocument();
            try
            {
               hCatalog.Load(hDownload.SavedFilePath);
            }
            catch (XmlException)
            {
               SetLoadFailed("Could not retrieve layer list");
               return;
            }

            XmlNodeList oNodeList = hCatalog.SelectNodes("/ARCXML/RESPONSE/SERVICEINFO/LAYERINFO");
            if (oNodeList.Count == 0)
            {
               SetLoadFailed("Service has no layers");
               return;
            }

            bool blRecognizedCRS = false;

            XmlElement oFeatureCoordSys = hCatalog.SelectSingleNode("/ARCXML/RESPONSE/SERVICEINFO/PROPERTIES/FEATURECOORDSYS") as XmlElement;
            if (oFeatureCoordSys != null)
            {
               String szCRSID = "EPSG:" + oFeatureCoordSys.GetAttribute("id");
               blRecognizedCRS = Utility.GCSMappings.WMSWGS84Equivalents.Contains(szCRSID);
            }

            GeographicBoundingBox oServiceBounds = new GeographicBoundingBox();

            if (blRecognizedCRS)
            {
               XmlElement oServiceEnvelope = hCatalog.SelectSingleNode("/ARCXML/RESPONSE/SERVICEINFO/PROPERTIES/ENVELOPE") as XmlElement;
               if (oServiceEnvelope != null)
               {
                  oServiceBounds.North = Double.Parse(oServiceEnvelope.GetAttribute("maxy"));
                  oServiceBounds.East = Double.Parse(oServiceEnvelope.GetAttribute("maxx"));
                  oServiceBounds.South = Double.Parse(oServiceEnvelope.GetAttribute("miny"));
                  oServiceBounds.West = Double.Parse(oServiceEnvelope.GetAttribute("minx"));
               }
            }

            lock (m_oLock)
            {
               foreach (XmlElement nLayerElement in oNodeList) //CMTODO: get maxscale and minscale here?
               {
                  String szID = nLayerElement.GetAttribute("id");
                  String szTitle = nLayerElement.GetAttribute("name");
                  if (String.IsNullOrEmpty(szTitle)) szTitle = "LayerID " + szID;

                  String szMinScale = nLayerElement.GetAttribute("minscale");
                  double dMinScale = 0.0;
                  if (!String.IsNullOrEmpty(szMinScale))
                  {
                     dMinScale = Double.Parse(szMinScale);
                  }

                  String szMaxScale = nLayerElement.GetAttribute("maxscale");
                  double dMaxScale = double.MaxValue;
                  if (!String.IsNullOrEmpty(szMaxScale))
                  {
                     dMaxScale = Double.Parse(szMaxScale);
                  }

                  GeographicBoundingBox oLayerBounds = oServiceBounds.Clone() as GeographicBoundingBox;

                  if (blRecognizedCRS)
                  {
                     XmlElement oLayerEnvelope = null;
                     if (nLayerElement.GetAttribute("type").Equals("image"))
                        oLayerEnvelope = nLayerElement.SelectSingleNode("ENVELOPE") as XmlElement;
                     if (nLayerElement.GetAttribute("type").Equals("featureclass"))
                        oLayerEnvelope = nLayerElement.SelectSingleNode("FCLASS/ENVELOPE") as XmlElement;
                     if (oLayerEnvelope != null)
                     {
                        oLayerBounds.North = Double.Parse(oLayerEnvelope.GetAttribute("maxy"));
                        oLayerBounds.East = Double.Parse(oLayerEnvelope.GetAttribute("maxx"));
                        oLayerBounds.South = Double.Parse(oLayerEnvelope.GetAttribute("miny"));
                        oLayerBounds.West = Double.Parse(oLayerEnvelope.GetAttribute("minx"));
                     }
                  }

                  LayerBuilders.Add(new ArcIMSQuadLayerBuilder(this.Uri as ArcIMSServerUri, m_szName, szTitle, szID, oLayerBounds, MainForm.WorldWindowSingleton, this, dMinScale, dMaxScale));
               }
            }

            SetLoadSuccessful();
         }
         catch (Exception e)
         {
            SetLoadFailed(e.Message);
         }
         finally
         {
            if (LoadFinished != null) LoadFinished();

            hDownload.IsComplete = true;
            hDownload.Dispose();
         }
      }

      protected override System.Windows.Forms.TreeNode getLoadingNode()
      {
         TreeNode result =  new TreeNode("Getting service layers...", MainForm.ImageListIndex("loading"), MainForm.ImageListIndex("loading"));
         result.Tag = new ServerTree.TempNodeTag();
         return result;
      }

      public override System.Drawing.Icon Icon
      {
         get { return Dapple.Properties.Resources.nasa; }
      }

      public override string DisplayIconKey
      {
         get
         {
            return "imageservice";
         }
      }
   }
}
