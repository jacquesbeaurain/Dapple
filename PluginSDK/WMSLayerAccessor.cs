using System;
using System.Collections.Generic;
using System.Globalization;
using Utility;
using WorldWind.Renderable;

namespace WorldWind.Net.Wms
{
	/// <summary>
	/// Calculates URLs for WMS layers.
	/// </summary>
	public class WmsImageStoreDapple : ImageStore
	{
		#region Private Members

		string m_serverGetMapUrl;
		string m_wmsLayerName;
		string m_wmsLayerStyle;
		string m_imageFormat;
		string m_version;
		string m_srs;
		string m_crs;
		int m_textureSizePixels = 512;
		TimeSpan m_cacheExpirationTime = TimeSpan.MaxValue;

		#endregion

		public WmsImageStoreDapple(string imageFormat, string serverGetMapUrl, string version, string layerName, string wmsLayerStyle, string srs, string crs)
		{
			m_imageFormat = imageFormat;
			m_serverGetMapUrl = serverGetMapUrl;
			m_version = version;
			m_wmsLayerName = layerName;
			m_wmsLayerStyle = wmsLayerStyle;
			m_srs = srs;
			m_crs = crs;
		}


		#region ImageStore Overrides

		public override bool IsDownloadableLayer
		{
			get
			{
				return true;
			}
		}

		protected override string GetDownloadUrl(IGeoSpatialDownloadTile tile)
		{

			string projectionRequest = "";
			bool reverseXY = false;

			if (m_version == "1.3.0")
			{
				if (m_crs.Equals("EPSG:4326"))
				{
					reverseXY = true;
					projectionRequest = "crs=EPSG:4326";
				}
				if (GCSMappings.WMSWGS84Equivalents.Contains(m_crs))
					projectionRequest = "crs=" + m_crs;
				else
					projectionRequest = "crs=CRS:84";
			}
			else
			{
				if (GCSMappings.WMSWGS84Equivalents.Contains(m_srs))
					projectionRequest = "srs=" + m_srs;
				else
					projectionRequest = "srs=EPSG:4326";
			}

			string wmsQuery = string.Format(
				 CultureInfo.InvariantCulture,
				 "{0}" + (m_serverGetMapUrl.IndexOf("?") == -1 ? "?" : "") +
				 "service=WMS&version={1}&request=GetMap&layers={2}&format={3}&width={4}&height={5}&{6}&bbox={7},{8},{9},{10}&styles={11}&transparent=TRUE",
				 m_serverGetMapUrl,
				 m_version,
				 m_wmsLayerName,
				 m_imageFormat,
				 m_textureSizePixels,
				 m_textureSizePixels,
				 projectionRequest,
				 reverseXY ? tile.South : tile.West,
				 reverseXY ? tile.West : tile.South,
				 reverseXY ? tile.North : tile.East,
				 reverseXY ? tile.East : tile.North,
				 m_wmsLayerStyle);

			// Cleanup
			while (wmsQuery.IndexOf("??") != -1)
				wmsQuery = wmsQuery.Replace("??", "?");

			return wmsQuery;
		}
		#endregion

		#region Properties

		internal virtual TimeSpan CacheExpirationTime
		{
			get
			{
				return m_cacheExpirationTime;
			}
			set
			{
				m_cacheExpirationTime = value;
			}
		}

		internal virtual string ServerGetMapUrl
		{
			get
			{
				return m_serverGetMapUrl;
			}
			set
			{
				m_serverGetMapUrl = value;
			}
		}

		internal virtual string SRS
		{
			get
			{
				return m_srs;
			}
			set
			{
				m_srs = value;
			}
		}

		internal virtual string CRS
		{
			get
			{
				return m_crs;
			}
			set
			{
				m_crs = value;
			}
		}


		internal virtual string WMSLayerName
		{
			get
			{
				return m_wmsLayerName;
			}
			set
			{
				m_wmsLayerName = value;
			}
		}

		internal virtual string WMSLayerStyle
		{
			get
			{
				return m_wmsLayerStyle;
			}
			set
			{
				m_wmsLayerStyle = value;
			}
		}

		internal virtual string ImageFormat
		{
			get
			{
				return m_imageFormat;
			}
			set
			{
				m_imageFormat = value;
			}
		}

		internal virtual string Version
		{
			get
			{
				return m_version;
			}
			set
			{
				m_version = value;
			}
		}

		/// <summary>
		/// Bitmap width/height
		/// </summary>
		public override int TextureSizePixels
		{
			get
			{
				return m_textureSizePixels;
			}
			set
			{
				m_textureSizePixels = value;
			}
		}
		#endregion
	}
}
