using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using WorldWind;
using WorldWind.Renderable;
using System.Xml;
using WorldWind.Net.Wms;

namespace Dapple.LayerGeneration
{
	public class WMSQuadLayerBuilder : ImageBuilder
	{
		#region Private Members

		public WMSLayer m_wmsLayer;
		QuadTileSet m_oQuadTileSet;

		//Image Accessor
		private int m_iLevels = 15;
		private int m_iTextureSizePixels = 256;
		private string m_strCacheRoot;
		
		//QuadTileLayer
		int distAboveSurface = 0;
		bool terrainMapped = false;
		GeographicBoundingBox m_hBoundary = new GeographicBoundingBox(0, 0, 0, 0);
		private double m_dLevelZeroTileSizeDegrees = 0;

		WMSServerBuilder m_Server;

		bool m_blnIsChanged = true;
		#endregion

		#region Static
		public static readonly string URLProtocolName = "gxwms://";

		public static readonly string TypeName = "WMSQuadLayer";

		public static readonly string CacheSubDir = "WMSImages";

		public static string GetServerFileNameFromUrl(string strurl)
		{
			string serverfile = strurl;
			int iQuery = serverfile.IndexOf("?");
			if (iQuery != -1)
				serverfile = serverfile.Substring(0, iQuery);

			int iUrl = strurl.IndexOf("//") + 2;
			if (iUrl == -1)
				iUrl = strurl.IndexOf("\\") + 2;
			if (iUrl != -1)
				serverfile = serverfile.Substring(iUrl);
			foreach (Char ch in Path.GetInvalidFileNameChars())
				serverfile = serverfile.Replace(ch.ToString(), "_");
			return serverfile;
		}

		private static void ParseURI(string uri, ref string strCapURL, ref string strLayer, ref int pixelsize)
		{
			strCapURL = uri.Replace(URLProtocolName, "http://");
			int iIndex = strCapURL.LastIndexOf("&");
			if (iIndex != -1)
			{
				pixelsize = Convert.ToInt32(strCapURL.Substring(iIndex).Replace("&pixelsize=", ""));
				strCapURL = strCapURL.Substring(0, iIndex);
			}
			else
				return;
			iIndex = strCapURL.LastIndexOf("&");
			if (iIndex != -1)
			{
				strLayer = strCapURL.Substring(iIndex).Replace("&layer=", "");
				strCapURL = strCapURL.Substring(0, iIndex).Trim();
			}
			else
				return;
			WMSCatalogBuilder.TrimCapabilitiesURL(ref strCapURL);
		}

		public static string ServerURLFromURI(string uri)
		{
			string strServer = "";
			string strLayer = "";
			int pixelsize = 1024;
			try
			{
				ParseURI(uri, ref strServer, ref strLayer, ref pixelsize);
			}
			catch
			{
			}
			return strServer;
		}

		public static WMSQuadLayerBuilder GetBuilderFromURI(string uri, WMSCatalogBuilder provider, WorldWindow worldWindow, WMSServerBuilder wmsserver)
		{
			string strServer = "";
			string strLayer = "";
			int pixelsize = 1024;
			try
			{
				ParseURI(uri, ref strServer, ref strLayer, ref pixelsize);

				WMSList oServer = provider.FindServer(strServer);
				foreach (WMSLayer layer in oServer.Layers)
				{
					WMSLayer result = FindLayer(strLayer, layer);
					if (result != null)
					{
						WMSQuadLayerBuilder quadBuilder = wmsserver.FindLayerBuilder(result);
						if (quadBuilder != null)
						{
							quadBuilder.TextureSizePixels = pixelsize;
							return quadBuilder;
						}
					}
				}
			}
			catch
			{
			}
			return null;
		}

		public static WMSLayer FindLayer(string layerName, WMSLayer list)
		{
			foreach (WMSLayer layer in list.ChildLayers)
			{
				if (layer.ChildLayers != null && layer.ChildLayers.Length > 0)
				{
					WMSLayer result = FindLayer(layerName, layer);
					if (result != null)
					{
						return result;
					}
				}
				if (layer.Name == layerName)
				{
					return layer;
				}
			}
			return null;
		}

		#endregion

		public WMSQuadLayerBuilder(WMSLayer layer, World world, string cacheDirectory, WMSServerBuilder server, IBuilder parent)
		{
			terrainMapped = true;

			m_Server = server;
			m_wmsLayer = layer;
			m_strName = layer.Title;
			distAboveSurface = 0;

			m_hBoundary = new GeographicBoundingBox((double)layer.North, (double)layer.South, (double)layer.West, (double)layer.East);

			// Determine the needed levels (function of tile size and resolution, for which we just use ~5 meters because it is not available with WMS)
			double dRes = 5.0 / 100000.0;
			if (dRes > 0)
			{
				double dTileSize = LevelZeroTileSize;
				m_iLevels = 1;
				while (dTileSize / Convert.ToDouble(m_iTextureSizePixels) > dRes / 4.0)
				{
					m_iLevels++;
					dTileSize /= 2;
				}
			}


			m_strCacheRoot = cacheDirectory;
			m_oWorld = world;
			m_Parent = parent;
		}

		public override string ServiceType
		{
			get
			{
				return "WMS Layer";
			}
		}

		public override bool SupportsMetaData
		{
			get
			{
				return m_Server != null && m_Server.SupportsMetaData;
			}
		}

		private XmlNode FindLayer(XmlNode oParentNode)
		{
			XmlNode oRetNode = null;

			foreach (XmlNode oNode in oParentNode.ChildNodes)
			{
				if (String.Compare(oNode.Name, "WMT_MS_Capabilities", true) == 0 || String.Compare(oNode.Name, "Capability") == 0 || String.Compare(oNode.Name, "Layer", true) == 0)
				{
					if (String.Compare(oNode.Name, "Layer", true) == 0)
					{
						foreach (XmlNode oChildNode in oNode.ChildNodes)
						{
							if (String.Compare(oChildNode.Name, "Name", true) == 0 && oChildNode.InnerText == m_wmsLayer.Name)
							{
								oRetNode = oNode;
								break;
							}
						}
					}
					if (oRetNode == null)
						oRetNode = FindLayer(oNode);
					if (oRetNode != null)
						break;
				}
			}
			return oRetNode;
		}

		private void RemoveLayers(XmlNode oParentNode)
		{
			List<XmlNode> deleteList = new List<XmlNode>();
			foreach (XmlNode oNode in oParentNode.ChildNodes)
			{
				if (String.Compare(oNode.Name, "WMT_MS_Capabilities", true) == 0 || String.Compare(oNode.Name, "Capability") == 0 || String.Compare(oNode.Name, "Layer", true) == 0)
				{
					if (String.Compare(oNode.Name, "Layer", true) == 0)
						deleteList.Add(oNode);
					RemoveLayers(oNode);
				}
			}
			foreach (XmlNode oNode in deleteList)
				oParentNode.RemoveChild(oNode);
		}

		public override XmlNode GetMetaData(XmlDocument oDoc)
		{
			if (m_Server != null && m_wmsLayer != null)
			{
				XmlDocument responseDoc = new XmlDocument();
				responseDoc.Load(m_Server.CapabilitiesFilePath);
				XmlNode oNode = responseDoc.DocumentElement;
				XmlNode newNode = oDoc.CreateElement(oNode.Name);
				newNode.InnerXml = oNode.InnerXml;

				// Find the layer node that matches this one and remove the rest
				XmlNode oLayerNode = FindLayer(newNode);
				if (oLayerNode != null)
				{
					string strInner = oLayerNode.InnerXml;
					RemoveLayers(newNode);
					XmlNode layerNode = oDoc.CreateElement("Layer");
					layerNode.InnerXml = strInner;
					newNode.AppendChild(layerNode);
				}
				return newNode;
			}
			else
				return null;
		}

		public override string StyleSheetName
		{
			get
			{
				return "wms_layer_meta.xslt";
			}
		}

		public override bool SupportsLegend
		{
			get
			{
				return m_wmsLayer != null && m_wmsLayer.HasLegend;
			}
		}

		public override string[] GetLegendURLs()
		{
			if (m_wmsLayer.HasLegend)
			{
				foreach (WMSLayerStyle style in m_wmsLayer.Styles)
				{
					if (style.legendURL != null && style.legendURL.Length > 0)
					{
						int i = 0;
						string[] strArr = new string[style.legendURL.Length];
						foreach (WMSLayerStyleLegendURL legURL in style.legendURL)
						{
							strArr[i] = legURL.ToString();
							i++;
						}
						return strArr;
					}
				}
			}
			return null;
		}

		public override RenderableObject GetLayer()
		{
			return GetQuadLayer();
		}

		public QuadTileSet GetQuadLayer()
		{
			if (m_blnIsChanged)
			{
				string strExt = ".png";
				string strCachePath = Path.Combine(GetCachePath(), LevelZeroTileSize.ToString());
				System.IO.Directory.CreateDirectory(strCachePath);

				string imageFormat = "image/png";
				if (m_wmsLayer.ImageFormats != null)
				{
				   foreach (string curFormat in m_wmsLayer.ImageFormats)
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
				}
				if (string.Compare(imageFormat, "image/jpeg", true, System.Globalization.CultureInfo.InvariantCulture) == 0 ||
				   string.Compare(imageFormat, "image/jpg", true, System.Globalization.CultureInfo.InvariantCulture) == 0)
					strExt = ".jpg";

				WmsImageStoreDapple[] imageStores = new WmsImageStoreDapple[1];
				imageStores[0] = new WmsImageStoreDapple(imageFormat, m_wmsLayer.ParentWMSList.ServerGetMapUrl,
					m_wmsLayer.ParentWMSList.Version, m_wmsLayer.Name, string.Empty, m_wmsLayer.SRS, m_wmsLayer.CRS);
				imageStores[0].DataDirectory = null;
				imageStores[0].LevelZeroTileSizeDegrees = LevelZeroTileSize;
				imageStores[0].LevelCount = m_iLevels;
				imageStores[0].ImageExtension = strExt;
				imageStores[0].CacheDirectory = strCachePath;
				imageStores[0].TextureFormat = World.Settings.TextureFormat;
				imageStores[0].TextureSizePixels = m_iTextureSizePixels;
				
				m_oQuadTileSet = new QuadTileSet(m_strName, m_oWorld, distAboveSurface,
					(double)m_wmsLayer.North, (double)m_wmsLayer.South, (double)m_wmsLayer.West, (double)m_wmsLayer.East, 
					terrainMapped, imageStores);
				m_oQuadTileSet.AlwaysRenderBaseTiles = true;
				m_oQuadTileSet.IsOn = m_IsOn;
				m_oQuadTileSet.Opacity = m_bOpacity;
				m_blnIsChanged = false;
			}
			return m_oQuadTileSet;
		}

		#region IBuilder Members

		public override byte Opacity
		{
			get
			{
				if (m_oQuadTileSet != null)
					return m_oQuadTileSet.Opacity;
				return m_bOpacity;
			}
			set
			{
				bool bChanged = false;
				if (m_bOpacity != value)
				{
					m_bOpacity = value;
					bChanged = true;
				}
				if (m_oQuadTileSet != null && m_oQuadTileSet.Opacity != value)
				{
					m_oQuadTileSet.Opacity = value;
					bChanged = true;
				}
				if (bChanged)
					SendBuilderChanged(BuilderChangeType.OpacityChanged);
			}
		}

		public override bool Visible
		{
			get
			{
				if (m_oQuadTileSet != null)
					return m_oQuadTileSet.IsOn;
				return m_IsOn;
			}
			set
			{
				bool bChanged = false;
				if (m_IsOn != value)
				{
					m_IsOn = value;
					bChanged = true;
				}
				if (m_oQuadTileSet != null && m_oQuadTileSet.IsOn != value)
				{
					m_oQuadTileSet.IsOn = value;
					bChanged = true;
				}

				if (bChanged)
					SendBuilderChanged(BuilderChangeType.VisibilityChanged);
			}
		}

		public override string Type
		{
			get { return WMSQuadLayerBuilder.TypeName; }
		}

		public override bool IsChanged
		{
			get { return m_blnIsChanged; }
		}

		public override string LogoKey
		{
			get { return "wms"; }
		}

		public override bool bIsDownloading(out int iBytesRead, out int iTotalBytes)
		{
			if (m_oQuadTileSet != null)
				return m_oQuadTileSet.bIsDownloading(out iBytesRead, out iTotalBytes);
			else
			{
				iBytesRead = 0;
				iTotalBytes = 0;
				return false;
			}
		}

		#endregion

		public double LevelZeroTileSize
		{
			get
			{
				if (m_dLevelZeroTileSizeDegrees == 0)
				{
					// Round to ceiling of four decimals (>~ 10 meter resolution)
					// Empirically determined as pretty good tile size choice for small data sets
					double dLevelZero = Math.Ceiling(10000.0 * Math.Max(m_hBoundary.North - m_hBoundary.South, m_hBoundary.West - m_hBoundary.East)) / 10000.0;

					// Optimum tile alignment when this is 180/(2^n), the first value is 180/2^3
					m_dLevelZeroTileSizeDegrees = 22.5;
					while (dLevelZero < m_dLevelZeroTileSizeDegrees)
						m_dLevelZeroTileSizeDegrees /= 2;
				}
				return m_dLevelZeroTileSizeDegrees;
			}
		}


		public int Levels
		{
			get 
			{ 
				return m_iLevels; 
			}
			set
			{
				if (m_iLevels != value)
				{
					m_blnIsChanged = true;
					m_iLevels = value;
				}
			}
		}

		#region ImageBuilder Members

		public override GeographicBoundingBox Extents
		{
			get { return m_hBoundary; }
		}

		public int TextureSizePixels
		{
			get { return m_iTextureSizePixels; }
			set
			{
				if (m_iTextureSizePixels != value)
				{
					m_blnIsChanged = true;
					m_iTextureSizePixels = value;
				}
			}
		}

		#endregion

		public override string GetCachePath()
		{
			string serverfile = GetServerFileNameFromUrl(m_wmsLayer.ParentWMSList.ServerGetMapUrl);
			return Path.Combine(Path.Combine(Path.Combine(m_strCacheRoot, CacheSubDir), serverfile), Utility.StringHash.GetBase64HashForPath(m_wmsLayer.Name));
		}

		public override string GetURI()
		{
			return (m_wmsLayer.ParentWMSList.ServerGetCapabilitiesUrl + "&layer=" + m_wmsLayer.Name + "&pixelsize=" + m_iTextureSizePixels.ToString()).Replace("http://", URLProtocolName);
		}

		public override object Clone()
		{
			return new WMSQuadLayerBuilder(m_wmsLayer, m_oWorld, m_strCacheRoot, m_Server, m_Parent);
		}

		protected override void CleanUpLayer(bool bFinal)
		{
			if (m_oQuadTileSet != null)
				m_oQuadTileSet.Dispose();
			m_oQuadTileSet = null;
			m_blnIsChanged = true;
		}
	}
}
