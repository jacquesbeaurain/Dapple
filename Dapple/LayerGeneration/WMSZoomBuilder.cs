using System;
using System.Threading;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using GeosoftWorldWindApp.LayerGeneration;
using WorldWind;
using WorldWind.Renderable;
using WorldWind.Net;
using WorldWind.Net.Wms;

namespace GeosoftWorldWindApp.LayerGeneration
{
   public class WMSZoomBuilder : ImageBuilder
   {
      private WMSZoomLayer m_imgLayer;
      public WMSLayer m_wmsLayer;
      private int m_intImagePixelSize = 1024;
      private WMSLayerStyle m_style;
      private WorldWindow m_WorldWindow;
      private string m_strCacheRoot;

      #region Static
      public static readonly string URLProtocolName = "gxwms://";

      public static readonly string CacheSubDir = "WMS Image Cache";

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
            pixelsize = Convert.ToInt32(strCapURL.Substring(iIndex).Replace("&startpixelsize=", ""));
            strCapURL = strCapURL.Substring(0, iIndex);
         } else
            return;
         iIndex = strCapURL.LastIndexOf("&");
         if (iIndex != -1)
         {
            strLayer = strCapURL.Substring(iIndex).Replace("&layer=", "");
            strCapURL = strCapURL.Substring(0, iIndex).Trim();
         } else
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

      public static WMSZoomBuilder GetBuilderFromURI(string uri, WMSCatalogBuilder provider, WorldWindow worldWindow, WMSServerBuilder wmsserver)
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
                  WMSZoomBuilder zoomBuilder = wmsserver.FindLayerBuilder(result);
                  if (zoomBuilder != null)
                  {
                     zoomBuilder.ImagePixelSize = pixelsize;
                     return zoomBuilder;
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


      public WMSZoomBuilder(WMSLayer layer, string cachePath, WorldWindow worldWindow, IBuilder parent)
      {
         m_wmsLayer = layer;
         m_WorldWindow = worldWindow;
         m_strCacheRoot = cachePath;
         m_Parent = parent;
         m_oWorld = m_WorldWindow.CurrentWorld;
         m_strName = layer.Title;
      }

      #region ImageBuilder Members

      public override WorldWind.GeographicBoundingBox Extents
      {
         get
         {
            return new GeographicBoundingBox(Convert.ToDouble(m_wmsLayer.North),
                Convert.ToDouble(m_wmsLayer.South),
                Convert.ToDouble(m_wmsLayer.West),
                Convert.ToDouble(m_wmsLayer.East));
         }
      }

      public override int ImagePixelSize
      {
         get
         {
            return m_intImagePixelSize;
         }
         set
         {
            if (value != m_intImagePixelSize)
            {
               m_imgLayer = null;
               m_intImagePixelSize = value;
            }
         }
      }

      public WMSLayerStyle StyleTag
      {
         get
         {
            return m_style;
         }
         set
         {
            foreach (WMSLayerStyle style in m_wmsLayer.Styles)
            {
               if (value == style)
               {
                  m_style = value;
                  break;
               }
            }
         }
      }

      #endregion

      public WMSLayerStyle[] Styles
      {
         get
         {
            return m_wmsLayer.Styles;
         }
      }

      public override string ServiceType
      {
         get
         {
            return "WMS Layer";
         }
      }

      public string WMSServerURL
      {
         get
         {
            return m_wmsLayer.ParentWMSList.ServerGetMapUrl;
         }
      }

      #region IBuilder Members

      public override byte Opacity
      {
         get
         {
            if (m_imgLayer != null)
               return m_imgLayer.Opacity;
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
            if (m_imgLayer != null && m_imgLayer.Opacity != value)
            {
               m_imgLayer.Opacity = value;
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
            if (m_imgLayer != null)
               return m_imgLayer.IsOn;
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
            if (m_imgLayer != null && m_imgLayer.IsOn != value)
            {
               m_imgLayer.IsOn = value;
               bChanged = true;
            }

            if (bChanged)
               SendBuilderChanged(BuilderChangeType.VisibilityChanged);
         }
      }

      public override string Type
      {
         get { return "WMSImage"; }
      }

      public override bool IsChanged
      {
         get { return m_imgLayer == null; }
      }

      public override WorldWind.Renderable.RenderableObject GetLayer()
      {
         if (m_imgLayer == null)
         {
            m_imgLayer = new WMSZoomLayer(m_strName, m_oWorld, 0, m_wmsLayer, m_wmsLayer.South,
               m_wmsLayer.North, m_wmsLayer.West, m_wmsLayer.East, 0, m_intImagePixelSize, Opacity, m_IsOn,
               GetCachePath(), m_oWorld.TerrainAccessor, new decimal(0.5));
            m_imgLayer.LoadFailed += new WMSZoomLayer.LoadFailedHandler(m_imgLayer_LoadFailed);
         }
         return m_imgLayer;
      }

      void m_imgLayer_LoadFailed(RenderableObject oRO, string message)
      {
         m_blnFailed = true;
         m_imgLayer.IsOn = false;
         SendBuilderChanged(BuilderChangeType.LoadedASyncFailed);
      }
      #endregion

      public override string GetCachePath()
      {
         string serverfile = GetServerFileNameFromUrl(m_wmsLayer.ParentWMSList.ServerGetMapUrl);
         return Path.Combine(Path.Combine(Path.Combine(m_strCacheRoot, CacheSubDir), serverfile), Utility.StringHash.GetBase64HashForPath(m_wmsLayer.Name));
      }

      public override string GetURI()
      {
         return (m_wmsLayer.ParentWMSList.ServerGetCapabilitiesUrl + "&layer=" + m_wmsLayer.Name + "&startpixelsize=" + m_intImagePixelSize.ToString()).Replace("http://", URLProtocolName);
      }

      public override object Clone()
      {
         return new WMSZoomBuilder(m_wmsLayer, m_strCacheRoot, m_WorldWindow, m_Parent);
      }

      protected override void CleanUpLayer()
      {
         if (m_imgLayer != null)
            m_imgLayer.Dispose();
         m_imgLayer = null;
      }
   }
}
