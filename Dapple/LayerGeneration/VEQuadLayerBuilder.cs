using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using WorldWind;
using WorldWind.Renderable;
using System.Xml;
using Dapple;
using Dapple.Plugins;
using WorldWind.PluginEngine;

using Dapple.Plugins.VirtualEarth;

namespace Dapple.LayerGeneration
{
   public class VEQuadLayerBuilder : ImageBuilder
   {
      public static readonly string URLProtocolName = "gxve://";
      public static readonly string CacheSubDir = "Virtual Earth Cache";

      VEQuadTileSet m_oVEQTS;
      string m_strCacheRoot;
      VirtualEarthMapType m_mapType;
      bool IsOn = true;
      bool m_blnIsChanged = true;
		MainApplication m_MainApp;

      WorldWindow m_oWW;

      public VEQuadLayerBuilder(string name, VirtualEarthMapType mapType, MainApplication mainApp, bool isOn, IBuilder parent)
      {
         m_strName = name;

			m_MainApp = mainApp;
			m_oWW = m_MainApp.WorldWindow;
			m_oWorld = m_MainApp.WorldWindow.CurrentWorld;

         IsOn = isOn;
         m_strCacheRoot = MainApplication.Settings.CachePath;
			
         m_Parent = parent;
         m_mapType = mapType;
      }

      public override RenderableObject GetLayer()
      {
			if (m_blnIsChanged)
         {
				m_oVEQTS = new VEQuadTileSet(m_strName, m_mapType, m_oWorld, 0, true);
				m_oVEQTS.Opacity = m_bOpacity;
				m_oVEQTS.IsOn = m_IsOn;
            m_blnIsChanged = false;
         }
         return m_oVEQTS;
      }

      
      #region IBuilder Members

      public override byte Opacity
      {
         get
         {
				if (m_oVEQTS != null)
					return m_oVEQTS.Opacity;
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
				if (m_oVEQTS != null && m_oVEQTS.Opacity != value)
            {
					m_oVEQTS.Opacity = value;
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
            if (m_oVEQTS != null)
               return m_oVEQTS.IsOn;
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
            if (m_oVEQTS != null && m_oVEQTS.IsOn != value)
            {
               m_oVEQTS.IsOn = value;
               bChanged = true;
            }

            if (bChanged)
               SendBuilderChanged(BuilderChangeType.VisibilityChanged);
         }
      }

      public override string Type
      {
         get { return TypeName; }
      }

      public override bool IsChanged
      {
         get { return m_blnIsChanged; }
      }

      public override string LogoKey
      {
         get { return "live"; }
      }

      public override bool bIsDownloading(out int iBytesRead, out int iTotalBytes)
      {
         if (m_oVEQTS != null)
            return m_oVEQTS.bIsDownloading(out iBytesRead, out iTotalBytes);
         else
         {
            iBytesRead = 0;
            iTotalBytes = 0;
            return false;
         }
      }

      #endregion

      public override string ServiceType
      {
         get { return "Virtual Earth Layer"; }
      }

      public static string TypeName
      {
         get
         {
            return "VETileSet";
         }
      }

      public override string GetCachePath()
      {
         return Path.Combine(Path.Combine(m_strCacheRoot, CacheSubDir), m_mapType.ToString());
      }

      public override string GetURI()
      {
         return URLProtocolName + m_mapType.ToString();
      }

      public static string GetRoadURI()
      {
			return URLProtocolName + Dapple.Plugins.VirtualEarth.VirtualEarthMapType.road.ToString();
      }

      public static string GetArealURI()
      {
			return URLProtocolName + Dapple.Plugins.VirtualEarth.VirtualEarthMapType.aerial.ToString();
      }

      public static string GetHybridURI()
      {
			return URLProtocolName + Dapple.Plugins.VirtualEarth.VirtualEarthMapType.hybrid.ToString();
      }

         
      public static VEQuadLayerBuilder GetBuilderFromURI(string uri, MainApplication mainApp, IBuilder parent)
      {
         uri = uri.Trim();
         if (String.Compare(uri, GetRoadURI(), true) == 0)
				return new VEQuadLayerBuilder("Virtual Earth Map", Dapple.Plugins.VirtualEarth.VirtualEarthMapType.road, mainApp, true, parent);
         else if (String.Compare(uri, GetArealURI(), true) == 0)
				return new VEQuadLayerBuilder("Virtual Earth Satellite", Dapple.Plugins.VirtualEarth.VirtualEarthMapType.aerial, mainApp, true, parent);
         else if (String.Compare(uri, GetHybridURI(), true) == 0)
				return new VEQuadLayerBuilder("Virtual Earth Map & Satellite", Dapple.Plugins.VirtualEarth.VirtualEarthMapType.hybrid, mainApp, true, parent);
         else
            return null;
      }

      public override object Clone()
      {
         return new VEQuadLayerBuilder(m_strName, m_mapType, m_MainApp, m_IsOn, m_Parent);
      }

      protected override void CleanUpLayer(bool bFinal)
      {
         if (m_oVEQTS != null)
            m_oVEQTS.Dispose();
         m_oVEQTS = null;
         m_blnIsChanged = true;
      }

      public override GeographicBoundingBox Extents
      {
         get
         {
            return new GeographicBoundingBox(90.0,-90.0,-180.0,180);
         }
      }

		public override int TextureSizePixels
      {
         get
         {
            return 256; // TODO Not relevant
         }
         set
         {
         }
      }
   }
}
