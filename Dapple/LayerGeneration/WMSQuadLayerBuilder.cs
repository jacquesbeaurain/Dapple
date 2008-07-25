using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using WorldWind;
using WorldWind.Renderable;
using System.Xml;
using WorldWind.Net.Wms;
using WorldWind.PluginEngine;

namespace Dapple.LayerGeneration
{
   public class WMSQuadLayerBuilder : LayerBuilder 
	{
		#region Private Members

		private WMSLayer m_wmsLayer;
		private QuadTileSet m_oQuadTileSet;
		private int m_iLevels = 15;
		private const int TextureSizePixels = 256;
		private int distAboveSurface = 0;
		private GeographicBoundingBox m_hBoundary = new GeographicBoundingBox(90, -90, -180, 180);
		private WMSServerBuilder m_Server;
		private bool m_blnIsChanged = true;

		#endregion

		#region Static

		public static readonly string URLProtocolName = "gxwms://";

		public static readonly string TypeName = "WMSQuadLayer";

		public static readonly string CacheSubDir = "WMSImages";

		#endregion

      #region Constructor

      public WMSQuadLayerBuilder(WMSLayer layer, WorldWindow worldWindow, WMSServerBuilder server, IBuilder parent)
         :base(layer.Title, worldWindow, parent)
		{
			m_Server = server;
			m_wmsLayer = layer;
			distAboveSurface = 0;

			m_hBoundary = new GeographicBoundingBox((double)layer.North, (double)layer.South, (double)layer.West, (double)layer.East);

			// Determine the needed levels (function of tile size and resolution, for which we just use ~5 meters because it is not available with WMS)
			double dRes = 5.0 / 100000.0;
			if (dRes > 0)
			{
				double dTileSize = LevelZeroTileSize;
				m_iLevels = 1;
				while (dTileSize / Convert.ToDouble(TextureSizePixels) > dRes / 4.0)
				{
					m_iLevels++;
					dTileSize /= 2;
				}
			}
      }

      #endregion

		#region Properties

		[System.ComponentModel.Category("Dapple")]
		[System.ComponentModel.Browsable(true)]
		[System.ComponentModel.Description("Whether Dapple can display metadata for this data layer")]
		public override bool SupportsMetaData
		{
			get
			{
				return m_Server != null && m_Server.SupportsMetaData;
			}
		}

		[System.ComponentModel.Category("Dapple")]
		[System.ComponentModel.Browsable(true)]
		[System.ComponentModel.Description("Whether Dapple can display legend(s) for this data layer")]
		public override bool SupportsLegend
		{
			get
			{
				return m_wmsLayer != null && m_wmsLayer.HasLegend;
			}
		}

		[System.ComponentModel.Category("Dapple")]
		[System.ComponentModel.Browsable(true)]
		[System.ComponentModel.Description("The opacity of the image (255 = opaque, 0 = transparent)")]
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

		[System.ComponentModel.Category("Dapple")]
		[System.ComponentModel.Browsable(true)]
		[System.ComponentModel.Description("Whether this data layer is visible on the globe")]
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

		[System.ComponentModel.Category("Dapple")]
		[System.ComponentModel.Browsable(true)]
		[System.ComponentModel.ReadOnly(true)]
		[System.ComponentModel.Description("The tile size, in degrees, of the topmost level")]
		private double LevelZeroTileSize
		{
			get
			{
				// Round to ceiling of four decimals (>~ 10 meter resolution)
				// Empirically determined as pretty good tile size choice for small data sets
				double dLevelZero = Math.Ceiling(10000.0 * Math.Max(m_hBoundary.North - m_hBoundary.South, m_hBoundary.West - m_hBoundary.East)) / 10000.0;

				// Optimum tile alignment when this is 180/(2^n), the first value is 180/2^3
				double result = 22.5;

				while (dLevelZero < result)
					result /= 2;

				return result;
			}
		}

		[System.ComponentModel.Category("Common")]
		[System.ComponentModel.Browsable(true)]
		[System.ComponentModel.Description("The extents of this data layer, in WGS 84")]
		public override GeographicBoundingBox Extents
		{
			get { return m_hBoundary; }
		}

		[System.ComponentModel.Category("Common")]
		[System.ComponentModel.Browsable(true)]
		[System.ComponentModel.Description("The server providing this data layer")]
		public string ServerURL
		{
			get { return Layer.ParentWMSList.ServerGetCapabilitiesUrl; }
		}

		[System.ComponentModel.Category("WMS")]
		[System.ComponentModel.Browsable(true)]
		[System.ComponentModel.Description("The name used to access this data layer on the WMS server")]
		public String Name
		{
			get { return m_wmsLayer.Name; }
		}

		[System.ComponentModel.Browsable(false)]
		public override bool IsChanged
		{
			get { return m_blnIsChanged; }
		}

		[System.ComponentModel.Browsable(false)]
		public override string ServerTypeIconKey
		{
			get { return "wms"; }
		}

		[System.ComponentModel.Browsable(false)]
		public override string DisplayIconKey
		{
			get { return "layer"; }
		}

		[System.ComponentModel.Browsable(false)]
		public WMSLayer Layer
		{
			get
			{
				return m_wmsLayer;
			}
		}

		[System.ComponentModel.Browsable(false)]
		public override string StyleSheetName
		{
			get
			{
				return "wms_layer_meta.xslt";
			}
		}

		[System.ComponentModel.Browsable(false)]
		public override bool CanAddServerToHomeView
		{
			get
			{
				return !MainForm.HomeViewContains(m_Server.Uri.ToString(), MainForm.ServerType.WMS);
			}
		}

		#endregion

		#region ImageBuilder Implementations

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

      public override RenderableObject GetLayer()
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
            imageStores[0].TextureSizePixels = TextureSizePixels;

            m_oQuadTileSet = new QuadTileSet(m_szTreeNodeText, m_oWorldWindow.CurrentWorld, distAboveSurface,
               (double)m_wmsLayer.North, (double)m_wmsLayer.South, (double)m_wmsLayer.West, (double)m_wmsLayer.East,
               true, imageStores);
            m_oQuadTileSet.AlwaysRenderBaseTiles = true;
            m_oQuadTileSet.IsOn = m_IsOn;
            m_oQuadTileSet.Opacity = m_bOpacity;
            m_blnIsChanged = false;
         }
         return m_oQuadTileSet;
      }

      public override string GetURI()
      {
         String result = m_wmsLayer.ParentWMSList.ServerGetCapabilitiesUrl;
         if (!result.Contains("?")) result += "?";
         result += "&layer=" + m_wmsLayer.Name;
         result = result.Replace("http://", URLProtocolName);
         return result;
      }

      public override string GetCachePath()
      {
         WMSServerUri temp = new WMSServerUri(m_wmsLayer.ParentWMSList.ServerGetCapabilitiesUrl);
         return Path.Combine(Path.Combine(Path.Combine(m_strCacheRoot, CacheSubDir), temp.ToCacheDirectory()), Utility.StringHash.GetBase64HashForPath(m_wmsLayer.Name));
      }

      protected override void CleanUpLayer(bool bFinal)
      {
         if (m_oQuadTileSet != null)
            m_oQuadTileSet.Dispose();
         m_oQuadTileSet = null;
         m_blnIsChanged = true;
      }

      public override object CloneSpecific()
      {
         return new WMSQuadLayerBuilder(m_wmsLayer, m_oWorldWindow, m_Server, m_Parent);
      }

      public override bool Equals(object obj)
      {
         if (!(obj is WMSQuadLayerBuilder)) return false;
         WMSQuadLayerBuilder castObj = obj as WMSQuadLayerBuilder;

         // -- Equal if the same server serves the same layer --
         return this.m_wmsLayer.ParentWMSList.ServerGetCapabilitiesUrl.Equals(castObj.m_wmsLayer.ParentWMSList.ServerGetCapabilitiesUrl) &&
            this.m_wmsLayer.Name.Equals(castObj.m_wmsLayer.Name);
      }

		public override int GetHashCode()
		{
			return m_Server.Uri.ToString().GetHashCode() ^ m_wmsLayer.Name.GetHashCode();
		}

		public override void AddServerToHomeView(MainForm bob)
		{
			bob.CmdUpdateHomeView(MainForm.UpdateHomeViewType.AddServer, new String[] { m_Server.Uri.ToString(), "WMS" });
		}

      #endregion

      #region ImageBuilder Overrides

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

      #endregion

      #region Private Members

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

      public override void GetOMMetadata(out String szDownloadType, out String szServerURL, out String szLayerId)
      {
         szDownloadType = "wms";
         szServerURL = m_Server.Uri.ToBaseUri();
         szLayerId = m_wmsLayer.Name;
      }

      #endregion

      #region Other Public Methods

      public static WMSLayer GetLayer(String strDesiredLayerName, WMSLayer[] oSearchRoot)
      {
         if (oSearchRoot == null) return null;

         foreach (WMSLayer iter in oSearchRoot)
         {
            if (iter.Name != null && iter.Name.Equals(strDesiredLayerName)) return iter;

            WMSLayer result = GetLayer(strDesiredLayerName, iter.ChildLayers);
            if (result != null) return result;
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
   }
}
