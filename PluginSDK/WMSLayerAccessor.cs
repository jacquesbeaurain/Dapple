using System;
using System.Collections.Generic;
using System.Globalization;
using Utility;

namespace WorldWind.Net.Wms
{
	/// <summary>
	/// Calculates URLs for WMS layers.
	/// </summary>
	public class WMSLayerAccessor
	{
		#region Private Members
		
		string m_username;
		string m_password;
		string m_serverGetMapUrl;
		string m_wmsLayerName;
		string m_wmsLayerStyle;
		string m_imageFormat;
		string m_version;
      string m_srs;
      string m_crs;
		bool m_isTransparent;
		double m_boundingBoxOverlap;
		TimeSpan m_cacheExpirationTime = TimeSpan.MaxValue;

		#endregion	

      public WMSLayerAccessor(string imageFormat, bool istransparent, string serverGetMapUrl, string version, string layerName, string wmsLayerStyle, string srs, string crs)
      {
         m_imageFormat = imageFormat;
         m_isTransparent = istransparent;
         m_serverGetMapUrl = serverGetMapUrl;
         m_version = version;
         m_wmsLayerName = layerName;
         m_wmsLayerStyle = wmsLayerStyle;
         m_srs = srs;
         m_crs = crs;
      }


		#region Public Methods

      public string GetWMSRequestUrl(decimal north, decimal south, decimal west, decimal east, int size)
      {
         
         string projectionRequest = "";

         if (m_version == "1.1.1")
         {
            if (GCSMappings.WMSWGS84Equivalents.Contains(m_srs))
               projectionRequest = "srs=" + m_srs;
            else
               projectionRequest = "srs=EPSG:4326";
         }
         else
         {
            if (GCSMappings.WMSWGS84Equivalents.Contains(m_crs))
               projectionRequest = "crs=" + m_crs;
            else
               projectionRequest = "crs=CRS:84";
         }

         string wmsQuery = string.Format(
            CultureInfo.InvariantCulture,
            "{0}" + (m_serverGetMapUrl.IndexOf("?") == -1 ? "?" : "") +
            "service=WMS&version={1}&request=GetMap&layers={2}&format={3}&width={4}&height={5}&{6}&bbox={7},{8},{9},{10}&styles={11}&transparent={12}",
            m_serverGetMapUrl,
            m_version,
            m_wmsLayerName,
            m_imageFormat,
            size,
            size,
            projectionRequest,
            west, south, east, north,
            m_wmsLayerStyle,
            m_isTransparent.ToString().ToUpper());

         // Cleanup
         while (wmsQuery.IndexOf("??") != -1)
            wmsQuery = wmsQuery.Replace("??", "?");

         return wmsQuery;
      }
		#endregion

		#region Properties
		
		public virtual double BoundingBoxOverlap
		{
			get
			{
				return this.m_boundingBoxOverlap;
			}
			set
			{
				this.m_boundingBoxOverlap = value;
			}
		}

		public virtual string Username
		{
			get
			{
				return m_username;
			}
			set
			{
				m_username = value;
			}
		}

		public virtual string Password
		{
			get
			{
				return m_password;
			}
			set
			{
				m_password = value;
			}
		}

		public virtual TimeSpan CacheExpirationTime
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

		public virtual string ServerGetMapUrl
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

      public virtual string SRS
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

      public virtual string CRS
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


		public virtual string WMSLayerName
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

		public virtual string WMSLayerStyle
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

		public virtual string ImageFormat
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

		public virtual string Version
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

		public virtual bool IsTransparent
		{
			get
			{
				return m_isTransparent;
			}
			set
			{
				m_isTransparent = value;
			}
		}
		#endregion
	}
}
