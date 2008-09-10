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
using System.Collections;

namespace Dapple.LayerGeneration
{
   public class WMSCatalogBuilder : BuilderDirectory
   {
      #region Constants
      protected const string CATALOG_CACHE = "WMS Catalog Cache";
      #endregion

      private WorldWindow m_WorldWindow;
      private Dictionary<IndexedWebDownload, ServerBuilder> m_oCatalogDownloadsInProgress = new Dictionary<IndexedWebDownload,ServerBuilder>();
      private System.Collections.Hashtable m_oWMSListCache = new System.Collections.Hashtable(); //<WMSUri, WMSList>>
      private int m_iDownloadIndex = 0;

      public ServerTree.LoadFinishedCallbackHandler LoadFinished = null;

      public WMSCatalogBuilder(String strName, WorldWindow worldWindow, IBuilder parent)
         : base(strName, parent, false)
      {
         m_WorldWindow = worldWindow;
      }

      /// <summary>
      /// Add a WMS server to this catalog builder.
      /// </summary>
      /// <param name="oUri">The URI of the server.</param>
      /// <returns>A WMSServerBuilder for the server added.</returns>
      public BuilderDirectory AddServer(WMSServerUri oUri, bool blEnabled)
      {
         // create the cache directory
         string savePath = Path.Combine(Path.Combine(MainApplication.Settings.CachePath, CATALOG_CACHE), oUri.ToCacheDirectory());
         Directory.CreateDirectory(savePath);
         string xmlPath = Path.Combine(savePath, "capabilities.xml");

         // add a child node
         WMSServerBuilder oBuilder = new WMSServerBuilder(this, oUri, xmlPath, blEnabled);
         SubList.Add(oBuilder);

         if (blEnabled == true)
         {
            QueueServerDownload(oBuilder);
         }

         return oBuilder;
      }

      private void QueueServerDownload(WMSServerBuilder oBuilder)
      {
         // create the cache directory
         string savePath = Path.Combine(Path.Combine(MainApplication.Settings.CachePath, CATALOG_CACHE), ((WMSServerUri)oBuilder.Uri).ToCacheDirectory());
         Directory.CreateDirectory(savePath);
         string xmlPath = Path.Combine(savePath, "capabilities.xml");

         IndexedWebDownload download = new IndexedWebDownload(((WMSServerUri)oBuilder.Uri).ToCapabilitiesUri(), true, m_iDownloadIndex);
         m_iDownloadIndex++;
         download.SavedFilePath = xmlPath;
         download.CompleteCallback += new DownloadCompleteHandler(CatalogDownloadCompleteCallback);

			lock (((System.Collections.ICollection)m_oCatalogDownloadsInProgress).SyncRoot)
			{
				m_oCatalogDownloadsInProgress.Add(download, oBuilder);
			}
         download.BackgroundDownloadFile();
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

      public ArrayList GetServers()
      {
         ArrayList result = new ArrayList();

         foreach (WMSServerBuilder iter in m_colSublist)
         {
            if (iter.IsLoadedSuccessfully)
               result.Add(iter);
         }

         return result;
      }

      public void UncacheServer(WMSServerUri oUri)
      {
         m_oWMSListCache.Remove(oUri);

         foreach (WMSServerBuilder iter in m_colSublist)
         {
            if (iter.Uri.Equals(oUri))
            {
               m_colSublist.Remove(iter);
               break;
            }
         }
      }

      public void cancelDownloads()
      {
         lock (((System.Collections.ICollection)m_oCatalogDownloadsInProgress).SyncRoot)
         {
            foreach (WebDownload oDownload in m_oCatalogDownloadsInProgress.Keys)
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
				WMSServerBuilder serverDir = null;
				lock (((System.Collections.ICollection)m_oCatalogDownloadsInProgress).SyncRoot)
				{
					serverDir = m_oCatalogDownloadsInProgress[(IndexedWebDownload)download] as WMSServerBuilder;
				}

            try
            {
               download.Verify();
            }
            catch (Exception e)
            {
               serverDir.SetLoadFailed("Error accessing catalog: " + e.Message);
               return;
            }

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
            catch (Exception)
            {
               serverDir.SetLoadFailed("Unable to parse server capabilities");
               return;
            }

            if (oServer.Layers == null)
            {
               serverDir.SetLoadFailed("Could not retrieve the catalog.");
               return;
            }

				serverDir.Clear();
            foreach (WMSLayer layer in oServer.Layers)
            {
               // Each server's layers are compacted into one root parent layer; don't add that root to the tree
               foreach (WMSLayer actualLayer in layer.ChildLayers)
                  ProcessWMSLayer(actualLayer, serverDir);
            }
            if (!m_oWMSListCache.Contains(serverDir.Uri as WMSServerUri))
               m_oWMSListCache.Add(serverDir.Uri, oServer);

            serverDir.List = oServer;

            serverDir.SetLoadSuccessful();
         }
         finally
         {
            if (LoadFinished != null) LoadFinished();

				lock (((System.Collections.ICollection)m_oCatalogDownloadsInProgress).SyncRoot)
				{
               m_oCatalogDownloadsInProgress.Remove((IndexedWebDownload)download);
               // dispose the download to release the catalog file's lock
               download.IsComplete = true;
               download.Dispose();
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
            IBuilder parentServer = directory;
            while (parentServer != null && !(parentServer is WMSServerBuilder))
               parentServer = parentServer.Parent;

            WMSQuadLayerBuilder builder = new WMSQuadLayerBuilder(layer, m_WorldWindow, parentServer as WMSServerBuilder, directory);
            directory.LayerBuilders.Add(builder);
         }
      }

      internal void Enable(WMSServerBuilder oBuilder)
      {
         if (oBuilder.LoadingPending)
         {
            oBuilder.LoadingPending = false;
            QueueServerDownload(oBuilder);
         }
         else
         {
            if (LoadFinished != null) LoadFinished();
         }
      }
   }

   public class WMSServerBuilder : ServerBuilder
   {
      string m_strCapabilitiesFilePath;
      WMSList m_oList;
      bool m_blLoadingPending = true;

      public WMSServerBuilder(IBuilder parent, WMSServerUri oUri, string CapabilitiesFilePath, bool blEnabled)
         : base(oUri.ToBaseUri(), parent, oUri, blEnabled)
      {
         m_strCapabilitiesFilePath = CapabilitiesFilePath;
      }

      [System.ComponentModel.Browsable(false)]
      public bool LoadingPending
      {
         get { return m_blLoadingPending; }
         set { m_blLoadingPending = value; }
      }

      [System.ComponentModel.Browsable(false)]
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

      [System.ComponentModel.Browsable(false)]
      public override string StyleSheetName
      {
         get
         {
            return "wms_cap_meta.xslt";
         }
      }

      [System.ComponentModel.Browsable(false)]
      public WMSList List
      {
         get { return m_oList; }
         set { m_oList = value; }
      }

      [System.ComponentModel.Browsable(false)]
      public override System.Drawing.Icon Icon
      {
         get { return Dapple.Properties.Resources.wms; }
      }

      protected override void SetEnabled(bool blValue)
      {
         ((WMSCatalogBuilder)Parent).Enable(this);
      }

		internal void Clear()
		{
			m_colChildren.Clear();
			m_colSublist.Clear();
		}
	}
}
