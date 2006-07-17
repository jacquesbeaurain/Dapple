using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using WorldWind;
using WorldWind.Renderable;
using System.Xml;
using Dapple;

namespace Dapple.LayerGeneration
{
   public class VEQuadLayerBuilder : ImageBuilder
   {
      public static readonly string URLProtocolName = "gxve://";
      public static readonly string CacheSubDir = "Virtual Earth Cache";
      public enum VirtualEarthMapType
      {
         aerial = 0,
         road,
         hybrid
      }

      bNb.Plugins_GD.VeReprojectTilesLayer m_oVELayer;
      string m_strCacheRoot;
      VirtualEarthMapType m_mapType;
      bool IsOn = true;
      bool m_blnIsChanged = true;

      WorldWindow m_oWW;

      public VEQuadLayerBuilder(string name, VirtualEarthMapType mapType, WorldWindow window, bool isOn, World World, string cacheDirectory, IBuilder parent)
      {
         m_strName = name;

         m_oWW = window;

         IsOn = isOn;
         m_strCacheRoot = cacheDirectory;
         m_oWorld = World;
         m_Parent = parent;
         m_mapType = mapType;
      }

      public override RenderableObject GetLayer()
      {
         return GetVELayer();
      }

      public bNb.Plugins_GD.VeReprojectTilesLayer GetVELayer()
      {
         if (m_blnIsChanged)
         {
            string fileExtension;
            string dataset;
            if (m_mapType == VirtualEarthMapType.road)
            {
               fileExtension = "png";
               dataset = "r";
            }
            else
            {
               fileExtension = "jpeg";
               if (m_mapType == VirtualEarthMapType.aerial)
               {
                  dataset = "a";
               }
               else
               {
                  dataset = "h";
               }
            }

            m_oVELayer = new bNb.Plugins_GD.VeReprojectTilesLayer("Virtual Earth", m_oWW, dataset, fileExtension, 0, GetCachePath());
            m_oVELayer.Opacity = m_bOpacity;
            m_oVELayer.IsOn = m_IsOn;

            //m_oQuadTileSet = new QuadTileSet("Virtual Earth",
            //   new GeographicBoundingBox(90, -90, -180, 180),
            //   m_oWorld,
            //   0,
            //   m_oWorld.TerrainAccessor,
            //   new VEImageAccessor(m_oWW));

            m_blnIsChanged = false;
         }
         return m_oVELayer;
      }

      #region IBuilder Members

      public override byte Opacity
      {
         get
         {
            if (m_oVELayer != null)
               return m_oVELayer.Opacity;
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
            if (m_oVELayer != null && m_oVELayer.Opacity != value)
            {
               m_oVELayer.Opacity = value;
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
            if (m_oVELayer != null)
               return m_oVELayer.IsOn;
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
            if (m_oVELayer != null && m_oVELayer.IsOn != value)
            {
               m_oVELayer.IsOn = value;
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
         if (m_oVELayer != null)
            return m_oVELayer.bIsDownloading(out iBytesRead, out iTotalBytes);
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
         return URLProtocolName + VEQuadLayerBuilder.VirtualEarthMapType.road.ToString();
      }

      public static string GetArealURI()
      {
         return URLProtocolName + VEQuadLayerBuilder.VirtualEarthMapType.aerial.ToString();
      }

      public static string GetHybridURI()
      {
         return URLProtocolName + VEQuadLayerBuilder.VirtualEarthMapType.hybrid.ToString();
      }

         
      public static VEQuadLayerBuilder GetBuilderFromURI(string uri, WorldWindow worldWindow, IBuilder parent)
      {
         uri = uri.Trim();
         if (String.Compare(uri, GetRoadURI(), true) == 0)
            return new VEQuadLayerBuilder("Virtual Earth Map", VEQuadLayerBuilder.VirtualEarthMapType.road, worldWindow, true, worldWindow.CurrentWorld,
                                          worldWindow.WorldWindSettings.CachePath, parent);
         else if (String.Compare(uri, GetArealURI(), true) == 0)
            return new VEQuadLayerBuilder("Virtual Earth Satellite", VEQuadLayerBuilder.VirtualEarthMapType.aerial, worldWindow, true, worldWindow.CurrentWorld,
                                          worldWindow.WorldWindSettings.CachePath, parent);
         else if (String.Compare(uri, GetHybridURI(), true) == 0)
            return new VEQuadLayerBuilder("Virtual Earth Map & Satellite", VEQuadLayerBuilder.VirtualEarthMapType.hybrid, worldWindow, true, worldWindow.CurrentWorld,
                                          worldWindow.WorldWindSettings.CachePath, parent);
         else
            return null;
      }

      public override object Clone()
      {
         return new VEQuadLayerBuilder(m_strName, m_mapType, m_oWW, m_IsOn, m_oWorld, m_strCacheRoot, m_Parent);
      }

      protected override void CleanUpLayer()
      {
         if (m_oVELayer != null)
            m_oVELayer.Dispose();
         m_oVELayer = null;
         m_blnIsChanged = true;
      }

      public override GeographicBoundingBox Extents
      {
         get
         {
            return new GeographicBoundingBox(90.0,-90.0,-180.0,180);
         }
      }

      public override int ImagePixelSize
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
