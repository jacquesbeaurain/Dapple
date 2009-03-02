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
using System.Globalization;

namespace Dapple.LayerGeneration
{
	internal class ArcIMSCatalogBuilder : BuilderDirectory
	{
		#region Constants
		internal const string CATALOG_CACHE = "ArcIMS Catalog Cache";
		#endregion

		private Dictionary<ArcIMSCatalogDownload, ServerBuilder> m_oCatalogDownloadsInProgress = new Dictionary<ArcIMSCatalogDownload, ServerBuilder>();
		private System.Collections.Hashtable m_oServers = new System.Collections.Hashtable();
		private int m_iIndexGenerator = 0;

		internal MethodInvoker LoadFinished = null;

		internal ArcIMSCatalogBuilder(String strName, WorldWindow oWorldWindow, IBuilder parent)
			: base(strName, parent, false)
		{
		}

		internal BuilderDirectory AddServer(ArcIMSServerUri oUri, bool blEnabled)
		{
			// create the cache directory
			String savePath = Path.Combine(Path.Combine(MainApplication.Settings.CachePath, CATALOG_CACHE), oUri.ToCacheDirectory());
			Directory.CreateDirectory(savePath);
			String xmlPath = Path.Combine(savePath, "__catalog.xml");

			ArcIMSServerBuilder dir = new ArcIMSServerBuilder(this, oUri, xmlPath, blEnabled);
			SubList.Add(dir);

			if (blEnabled)
			{
				QueueServerDownload(dir);
			}

			return dir;
		}

		private void QueueServerDownload(ArcIMSServerBuilder oBuilder)
		{
			// create the cache directory
			String savePath = Path.Combine(Path.Combine(MainApplication.Settings.CachePath, CATALOG_CACHE), oBuilder.Uri.ToCacheDirectory());
			Directory.CreateDirectory(savePath);
			String xmlPath = Path.Combine(savePath, "__catalog.xml");

			// download the catalog
			ArcIMSCatalogDownload download = new ArcIMSCatalogDownload(((ArcIMSServerUri)oBuilder.Uri), m_iIndexGenerator);
			m_iIndexGenerator++;
			download.SavedFilePath = xmlPath;
			download.CompleteCallback += new DownloadCompleteHandler(CatalogDownloadCompleteCallback);

			lock (((System.Collections.ICollection)m_oCatalogDownloadsInProgress).SyncRoot)
			{
				m_oCatalogDownloadsInProgress.Add(download, oBuilder);
			}
			download.BackgroundDownloadFile();
		}

		internal bool ContainsServer(ArcIMSServerUri oUri)
		{
			foreach (ArcIMSServerBuilder builder in m_colSublist)
			{
				if (builder.Uri.Equals(oUri))
					return true;
			}
			return false;
		}

		internal void UncacheServer(ArcIMSServerUri oUri)
		{
			m_oServers.Remove(oUri);

			foreach (ArcIMSServerBuilder iter in m_colSublist)
			{
				if (iter.Uri.Equals(oUri))
				{
					m_colSublist.Remove(iter);
					break;
				}
			}
		}

		internal ArrayList GetServers()
		{
			ArrayList result = new ArrayList();

			foreach (ArcIMSServerBuilder iter in m_colSublist)
			{
				if (iter.IsLoadedSuccessfully)
					result.Add(iter);
			}

			return result;
		}

		internal void cancelDownloads()
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
				ArcIMSServerBuilder serverDir = null;
				lock (((System.Collections.ICollection)m_oCatalogDownloadsInProgress).SyncRoot)
				{
					serverDir = m_oCatalogDownloadsInProgress[(ArcIMSCatalogDownload)download] as ArcIMSServerBuilder;
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

				serverDir.Clear();
				foreach (XmlNode nServiceNode in oNodeList)
				{
					XmlElement oLocaleNode = nServiceNode.SelectSingleNode("ENVIRONMENT/LOCALE") as XmlElement;
					CultureInfo oInfo = CultureInfo.CurrentCulture;
					if (oLocaleNode != null && oLocaleNode.HasAttribute("language") && oLocaleNode.HasAttribute("country"))
					{
						String strLanguage = oLocaleNode.GetAttribute("language");
						String strCountry = oLocaleNode.GetAttribute("country");
						try
						{
							oInfo = new CultureInfo(String.Format("{0}-{1}", strLanguage.ToLowerInvariant(), strCountry.ToUpperInvariant()));
						}
						catch (ArgumentException)
						{
							try
							{
								// --- A server returned en-IT, which Windows didn't understand.  If we can't process the
								// --- language and country code, use the neutral language instead
								oInfo = new CultureInfo(strLanguage.ToLowerInvariant());
							}
							catch (ArgumentException)
							{
								// --- If they sent back something truly bizarre, then just use invariant.  If it doesn't work,
								// --- then that's the server's problem, not ours.
								oInfo = CultureInfo.InvariantCulture;
							}
						}

					}

					serverDir.SubList.Add(new ArcIMSServiceBuilder(serverDir, ((XmlElement)nServiceNode).GetAttribute("name"), LoadFinished, oInfo));
				}

				serverDir.SetLoadSuccessful();
			}
			finally
			{
				if (LoadFinished != null) LoadFinished();

				lock (((System.Collections.ICollection)m_oCatalogDownloadsInProgress).SyncRoot)
				{
					m_oCatalogDownloadsInProgress.Remove(((ArcIMSCatalogDownload)download));
					// dispose the download to release the catalog file's lock
					download.IsComplete = true;
					download.Dispose();
				}
			}
		}

		internal ArcIMSServerBuilder GetServer(ArcIMSServerUri oUri)
		{
			foreach (ArcIMSServerBuilder iter in m_colSublist)
			{
				if (iter.Uri.Equals(oUri))
					return iter;
			}
			return null;
		}

		internal void Enable(ArcIMSServerBuilder oBuilder)
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


	internal class ArcIMSServerBuilder : ServerBuilder
	{
		string m_strCatalogPathname;
		bool m_blLoadingPending = true;

		internal ArcIMSServerBuilder(IBuilder parent, ArcIMSServerUri oUri, string strCatalogPathname, bool blEnabled)
			: base(oUri.ServerTreeDisplayName, parent, oUri, blEnabled)
		{
			m_strCatalogPathname = strCatalogPathname;
		}

		[System.ComponentModel.Browsable(false)]
		internal bool LoadingPending
		{
			get { return m_blLoadingPending; }
			set { m_blLoadingPending = value; }
		}

		[System.ComponentModel.Browsable(false)]
		internal string CatalogFilename
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
		internal override System.Drawing.Icon Icon
		{
			get { return Dapple.Properties.Resources.arcims; }
		}

		protected override void SetEnabled(bool blValue)
		{
			((ArcIMSCatalogBuilder)Parent).Enable(this);
		}		

		internal bool HasUnloadedServices
		{
			get
			{
				foreach (ArcIMSServiceBuilder oBuilder in m_colSublist)
				{
					if (oBuilder.IsLoading)
					{
						return true;
					}
				}
				return false;
			}
		}

		internal void Clear()
		{
			m_colChildren.Clear();
			m_colSublist.Clear();
		}
	}


	internal class ArcIMSServiceBuilder : AsyncBuilder
	{
		private String m_szName;
		private Object m_oLock = new Object();
		private ArcIMSServiceDownload m_hDownload;
		private MethodInvoker LoadFinished;
		private CultureInfo m_oCultureInfo;

		internal ArcIMSServiceBuilder(ArcIMSServerBuilder hServer, String szName, MethodInvoker hLoadFinished, CultureInfo oInfo)
			: base(szName, hServer, hServer.Uri, true)
		{
			m_szName = szName;
			LoadFinished = hLoadFinished;
			m_oCultureInfo = oInfo;
		}

		internal CultureInfo CultureInfo
		{
			get { return m_oCultureInfo; }
		}

		public override System.Windows.Forms.TreeNode[] getChildTreeNodes()
		{
			lock (m_oLock)
			{
				if (m_hDownload == null)
				{
					// create the cache directory
					String szSavePath = Path.Combine(Path.Combine(MainApplication.Settings.CachePath, ArcIMSCatalogBuilder.CATALOG_CACHE), ((ArcIMSServerBuilder)Parent).Uri.ToCacheDirectory());
					Directory.CreateDirectory(szSavePath);

					// download the catalog
					String szXmlPath = Path.Combine(Path.Combine(szSavePath, m_szName), "__catalog.xml");

					m_hDownload = new ArcIMSServiceDownload(((ArcIMSServerBuilder)Parent).Uri as ArcIMSServerUri, m_szName, 0);
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
						GeographicBoundingBox oRealServiceBounds = new GeographicBoundingBox();
						bool blValid = true;
						blValid &= Double.TryParse(oServiceEnvelope.GetAttribute("minx"), NumberStyles.Any, m_oCultureInfo, out oRealServiceBounds.West);
						blValid &= Double.TryParse(oServiceEnvelope.GetAttribute("miny"), NumberStyles.Any, m_oCultureInfo, out oRealServiceBounds.South);
						blValid &= Double.TryParse(oServiceEnvelope.GetAttribute("maxx"), NumberStyles.Any, m_oCultureInfo, out oRealServiceBounds.East);
						blValid &= Double.TryParse(oServiceEnvelope.GetAttribute("maxy"), NumberStyles.Any, m_oCultureInfo, out oRealServiceBounds.North);

						if (blValid)
							oServiceBounds = oRealServiceBounds;
					}
				}

				lock (m_oLock)
				{
					foreach (XmlElement nLayerElement in oNodeList)
					{
						String szID = nLayerElement.GetAttribute("id");
						String szTitle = nLayerElement.GetAttribute("name");
						if (String.IsNullOrEmpty(szTitle)) szTitle = "LayerID " + szID;

						String szMinScale = nLayerElement.GetAttribute("minscale");
						double dMinScale = ArcIMSQuadLayerBuilder.DefaultMinScale;
						if (!String.IsNullOrEmpty(szMinScale))
						{
							Double.TryParse(szMinScale, NumberStyles.Any, CultureInfo.InvariantCulture, out dMinScale);
						}

						String szMaxScale = nLayerElement.GetAttribute("maxscale");
						double dMaxScale = ArcIMSQuadLayerBuilder.DefaultMaxScale;
						if (!String.IsNullOrEmpty(szMaxScale))
						{
							Double.TryParse(szMaxScale, NumberStyles.Any, CultureInfo.InvariantCulture, out dMaxScale);
						}

						if (dMaxScale > dMinScale || dMinScale > 1.0)
						{
							// --- If the server sends back weird values, ignore them ---
							dMaxScale = ArcIMSQuadLayerBuilder.DefaultMaxScale;
							dMinScale = ArcIMSQuadLayerBuilder.DefaultMinScale;
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
								GeographicBoundingBox oRealLayerBounds = new GeographicBoundingBox();
								bool blValid = true;
								blValid &= Double.TryParse(oLayerEnvelope.GetAttribute("minx"), NumberStyles.Any, m_oCultureInfo, out oRealLayerBounds.West);
								blValid &= Double.TryParse(oLayerEnvelope.GetAttribute("miny"), NumberStyles.Any, m_oCultureInfo, out oRealLayerBounds.South);
								blValid &= Double.TryParse(oLayerEnvelope.GetAttribute("maxx"), NumberStyles.Any, m_oCultureInfo, out oRealLayerBounds.East);
								blValid &= Double.TryParse(oLayerEnvelope.GetAttribute("maxy"), NumberStyles.Any, m_oCultureInfo, out oRealLayerBounds.North);
								if (blValid)
									oLayerBounds = oRealLayerBounds;
							}
						}

						LayerBuilders.Add(new ArcIMSQuadLayerBuilder(((ArcIMSServerBuilder)Parent).Uri as ArcIMSServerUri, m_szName, szTitle, szID, oLayerBounds, MainForm.WorldWindowSingleton, this, dMinScale, dMaxScale, m_oCultureInfo));
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
			throw new NotImplementedException();
			/*TreeNode result = new TreeNode("Getting service layers...", MainForm.ImageListIndex("loading"), MainForm.ImageListIndex("loading"));
			result.Tag = new ServerTree.TempNodeTag();
			return result;*/
		}

		internal override System.Drawing.Icon Icon
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

		internal override void updateTreeNode(TreeNode oParent, bool blnAOIFilter, GeographicBoundingBox oAOI, string strSearch)
		{
			if (IsLoading)
			{
				oParent.Text = Title + " (Click to load layers)";
			}
			else if (LoadingErrorOccurred)
			{
				oParent.Text = Title + " (" + ErrorMessage + ")";
			}
			else
			{
				oParent.Text = Title + " (" + iGetLayerCount(blnAOIFilter, oAOI, strSearch).ToString() + ")";
			}
		}
	}
}
