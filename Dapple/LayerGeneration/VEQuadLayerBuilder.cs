using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using WorldWind;
using WorldWind.Renderable;
using System.Xml;

using Dapple;

//#define USE_NEW_VE_QTS

#if !USE_NEW_VE_QTS
using bNb.Plugins_GD;
#endif

using WorldWind.PluginEngine;

namespace Dapple.LayerGeneration
{
   public class VEQuadLayerBuilder : LayerBuilder
   {
      #region Statics

      public static readonly string URLProtocolName = "gxve://";
		public static readonly string CacheSubDir = "VEImages";

      #endregion

      #region Member Variables
#if USE_NEW_VE_QTS
		QuadTileSet m_oVEQTS;
#else
      VeReprojectTilesLayer m_oVEQTS;
#endif

		VirtualEarthMapType m_mapType;
		bool IsOn = true;
		bool m_blnIsChanged = true;

      #endregion

      #region Constructor

      public VEQuadLayerBuilder(string name, VirtualEarthMapType mapType, WorldWindow oWorldWindow, bool isOn, IBuilder parent)
         :base(name, oWorldWindow, parent)
		{
			IsOn = isOn;
			m_mapType = mapType;
      }

      #endregion

		#region Properties

		[System.ComponentModel.Category("Dapple")]
		[System.ComponentModel.Browsable(true)]
		[System.ComponentModel.Description("The opacity of the image (255 = opaque, 0 = transparent)")]
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

		[System.ComponentModel.Category("Dapple")]
		[System.ComponentModel.Browsable(true)]
		[System.ComponentModel.Description("Whether this data layer is visible on the globe")]
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
					if (value == true)
					{
						m_oVEQTS.ForceRefresh();
					}
					bChanged = true;
				}

				if (bChanged)
					SendBuilderChanged(BuilderChangeType.VisibilityChanged);
			}
		}

		[System.ComponentModel.Category("Common")]
		[System.ComponentModel.Browsable(true)]
		[System.ComponentModel.Description("The extents of this data layer, in WGS 84")]
		public override GeographicBoundingBox Extents
		{
			get
			{
				return new GeographicBoundingBox(85.0, -85.0, -180.0, 180);
			}
		}

		[System.ComponentModel.Category("VE")]
		[System.ComponentModel.Browsable(true)]
		[System.ComponentModel.Description("Which virtual earth data layer this is")]
		public String Type
		{
			get { return m_mapType.ToString(); }
		}

		[System.ComponentModel.Browsable(false)]
		public override bool IsChanged
		{
			get { return m_blnIsChanged; }
		}

		[System.ComponentModel.Browsable(false)]
		public override string ServerTypeIconKey
		{
			get { return "live"; }
		}

		[System.ComponentModel.Browsable(false)]
		public override string DisplayIconKey
		{
			get { return "live"; }
		}

		#endregion

		#region ImageBuilder Implementations

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

      public override RenderableObject GetLayer()
      {
         if (m_blnIsChanged)
         {
#if USE_NEW_VE_QTS
				double distanceAboveSurface = 0.0;

				VEImageStore[] imageStores = null;
				imageStores = new VEImageStore[1];
				imageStores[0] = new VEImageStore(m_mapType, m_oWorld.EquatorialRadius + distanceAboveSurface);
				imageStores[0].CacheDirectory = GetCachePath();
				imageStores[0].TextureFormat = World.Settings.TextureFormat;

				m_oVEQTS = new QuadTileSet(m_strName, m_oWorld, distanceAboveSurface, 90, -90, -180, 180, true, imageStores);
				m_oVEQTS.AlwaysRenderBaseTiles = true;
#else
            string fileExt, dataset;
            if (m_mapType == VirtualEarthMapType.road)
            {
               fileExt = "png";
               dataset = "r";
            }
            else
            {
               fileExt = "jpeg";
               if (m_mapType == VirtualEarthMapType.aerial)
               {
                  dataset = "a";
               }
               else
               {
                  dataset = "h";
               }
            }

            m_oVEQTS = new VeReprojectTilesLayer(m_szTreeNodeText, m_oWorldWindow, dataset, fileExt, 0, GetCachePath());
#endif
            m_oVEQTS.IsOn = m_IsOn;
            m_oVEQTS.Opacity = m_bOpacity;
            m_oVEQTS.RenderPriority = RenderPriority.TerrainMappedImages;
            m_blnIsChanged = false;
         }
         return m_oVEQTS;
      }

      public override string GetURI()
      {
         return URLProtocolName + m_mapType.ToString();
      }

      public override string GetCachePath()
      {
         return Path.Combine(Path.Combine(m_strCacheRoot, CacheSubDir), m_mapType.ToString());
      }

      protected override void CleanUpLayer(bool bFinal)
      {
         if (m_oVEQTS != null)
            m_oVEQTS.Dispose();
         m_oVEQTS = null;
         m_blnIsChanged = true;
      }

      public override object CloneSpecific()
      {
         return new VEQuadLayerBuilder(m_szTreeNodeText, m_mapType, m_oWorldWindow, m_IsOn, m_Parent);
      }

      public override bool Equals(object obj)
      {
         if (!(obj is VEQuadLayerBuilder)) return false;
         VEQuadLayerBuilder castObj = obj as VEQuadLayerBuilder;

         // -- Equal if they're the same VE map type --
         return m_mapType == castObj.m_mapType;
      }

      public override void GetOMMetadata(out String szDownloadType, out String szServerURL, out String szLayerId)
      {
         szDownloadType = "ve";
         szServerURL = m_mapType.ToString()[0] + "0.ortho.tiles.virtualearth.net/tiles/";
         szLayerId = m_mapType.ToString();
      }

      #endregion
	}
}
