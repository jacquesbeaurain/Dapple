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
	public class VEQuadLayerBuilder : ImageBuilder
	{
		public static readonly string URLProtocolName = "gxve://";
		public static readonly string CacheSubDir = "VEImages";

#if USE_NEW_VE_QTS
		QuadTileSet m_oVEQTS;
#else
		VeReprojectTilesLayer m_oVEQTS;
#endif

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
				double distanceAboveSurface = 0.0;

#if USE_NEW_VE_QTS
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

				m_oVEQTS = new VeReprojectTilesLayer(m_strName, m_MainApp, dataset, fileExt, 0, GetCachePath());
#endif
				m_oVEQTS.IsOn = m_IsOn;
				m_oVEQTS.Opacity = m_bOpacity;
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
			return URLProtocolName + VirtualEarthMapType.road.ToString();
		}

		public static string GetArealURI()
		{
			return URLProtocolName + VirtualEarthMapType.aerial.ToString();
		}

		public static string GetHybridURI()
		{
			return URLProtocolName + VirtualEarthMapType.hybrid.ToString();
		}


		public static VEQuadLayerBuilder GetBuilderFromURI(string uri, MainApplication mainApp, IBuilder parent)
		{
			uri = uri.Trim();
			if (String.Compare(uri, GetRoadURI(), true) == 0)
				return new VEQuadLayerBuilder("Virtual Earth Map", VirtualEarthMapType.road, mainApp, true, parent);
			else if (String.Compare(uri, GetArealURI(), true) == 0)
				return new VEQuadLayerBuilder("Virtual Earth Satellite", VirtualEarthMapType.aerial, mainApp, true, parent);
			else if (String.Compare(uri, GetHybridURI(), true) == 0)
				return new VEQuadLayerBuilder("Virtual Earth Map & Satellite", VirtualEarthMapType.hybrid, mainApp, true, parent);
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
				return new GeographicBoundingBox(90.0, -90.0, -180.0, 180);
			}
		}
	}
}
