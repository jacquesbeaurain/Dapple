using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Web;
using System.Collections;
using WorldWind.PluginEngine;
using WorldWind;
using Geosoft.Dap.Common;
using System.Collections.Specialized;
using System.Globalization;

namespace Dapple.LayerGeneration
{
	/// <summary>
	/// Class for dealing with URIs referring to a server's major HTTP entryway.
	/// </summary>
	public abstract class ServerUri : Uri
	{
		public ServerUri(String strValue)
			: base(strValue)
		{ }

		public ServerUri(UriBuilder oValue)
			: base(oValue.Uri.ToString())
		{ }

		/// <summary>
		/// Get a cache directory for this Uri (host and path, minus query and scheme.
		/// </summary>
		/// <returns></returns>
		public virtual String ToCacheDirectory()
		{
			String result = base.Host + base.AbsolutePath + base.Query;

			// Convert invalid characters to underscores
			foreach (Char ch in System.IO.Path.GetInvalidFileNameChars())
				result = result.Replace(ch.ToString(), "_");

			// And done
			return result;
		}

		public String ToBaseUri()
		{
			return base.ToString();
		}

		public override bool Equals(object comparand)
		{
			return base.Equals(comparand);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}

	public class WMSServerUri : ServerUri
	{
		public WMSServerUri(String strValue)
			: base(GetFilteredUriBuilder(strValue))
		{
		}

		public static UriBuilder GetFilteredUriBuilder(String strValue)
		{
			UriBuilder oBuilder = new UriBuilder(strValue);
			// --- Remove the service, version, and request query variables ---
			NameValueCollection oTokens = HttpUtility.ParseQueryString(oBuilder.Query);
			foreach (String szKey in oTokens.AllKeys)
			{
				if (szKey.Equals("service", StringComparison.InvariantCultureIgnoreCase) ||
					szKey.Equals("version", StringComparison.InvariantCultureIgnoreCase) ||
					szKey.Equals("request", StringComparison.InvariantCultureIgnoreCase))
				{
					oTokens.Remove(szKey);
				}
			}
			String szFilteredQuery = String.Empty;
			foreach (String szKey in oTokens.AllKeys)
			{
				if (!szFilteredQuery.Equals(String.Empty))
				{
					szFilteredQuery += "&";
				}
				szFilteredQuery += HttpUtility.UrlEncode(szKey) + "=" + HttpUtility.UrlEncode(oTokens[szKey]);
			}

			oBuilder.Query = szFilteredQuery;

			return oBuilder;
		}

		public String ToCapabilitiesUri()
		{
			return base.ToString()
				+ (base.ToString().IndexOf("?") > 0 ? "&" : "?")
				+ "request=GetCapabilities&service=WMS";
		}
	}

	public class ArcIMSServerUri : ServerUri
	{
		public ArcIMSServerUri(String strValue)
			: base(strValue)
		{ }

		/// <summary>
		/// Get a cache directory for this Uri (host and path, minus query and scheme.
		/// </summary>
		/// <returns></returns>
		public override String ToCacheDirectory()
		{
			String result = base.Host;

			// Convert invalid characters to underscores
			foreach (Char ch in System.IO.Path.GetInvalidFileNameChars())
				result = result.Replace(ch.ToString(), "_");

			result += "-" + this.ToString().GetHashCode().ToString("X");

			// And done
			return result;
		}

		public String ToCatalogUri()
		{
			return base.ToString()
				+ (base.ToString().IndexOf("?") > 0 ? "&" : "?")
				+ "ServiceName=catalog&ClientVersion=9.0";
		}

		public String ToServiceUri(String serviceName)
		{
			return base.ToString()
				+ (base.ToString().IndexOf("?") > 0 ? "&" : "?")
				+ "ServiceName=" + serviceName + "&ClientVersion=9.0";
		}

		public ArcIMSLayerUri ToLayerUri(String strServiceName, GeographicBoundingBox oBox)
		{
			String strUri = base.ToString()
				+ (base.ToString().IndexOf("?") > 0 ? "&" : "?")
				+ "ServiceName=" + strServiceName
				+ "&minx=" + oBox.West
				+ "&miny=" + oBox.South
				+ "&maxx=" + oBox.East
				+ "&maxy=" + oBox.North;

			return new ArcIMSLayerUri(strUri);
		}
	}

	public class TileServerUri : ServerUri
	{
		public TileServerUri(String strUri) : base(strUri) { }
	}

	public class VEServerUri : ServerUri
	{
		public VEServerUri(String strUri) : base(strUri) { }
	}

	public class DapServerUri : ServerUri
	{
		public DapServerUri(String strUri) : base(strUri) { }
	}

	public class GeoTiffFileUri : ServerUri
	{
		public GeoTiffFileUri(String strUri) : base(strUri) { }
	}

	public class KeyholeFileUri : ServerUri
	{
		public KeyholeFileUri(String strUri) : base(strUri) { }
	}

	/// <summary>
	/// Class for dealing with internal layer URIs.  Parses necessary informaion and removes it from the Server URI.
	/// </summary>
	public abstract class LayerUri
	{
		protected ServerUri m_oServer;
		protected Hashtable m_oTokens;

		public LayerUri(String strUri)
		{
			// This hack is needed because old .dapple views had WMS URIs that were missing the ?
			if (strUri.Contains("&") && !strUri.Contains("?"))
			{
				int index = strUri.IndexOf('&');
				strUri = strUri.Substring(0, index) + "?" + strUri.Substring(index + 1);
			}
			// End hack

			UriBuilder oTemp = new UriBuilder(strUri);
			if (!oTemp.Scheme.Equals(LayerScheme))
				throw new ArgumentException(oTemp.Scheme + " is not a valid LayerUri for " + LayerType + " layers");

			if (strUri.StartsWith("gxtif") || strUri.StartsWith("gxkml"))
			{
				oTemp.Scheme = "file";
			}
			else
			{
				oTemp.Scheme = "http";
			}

			parseQuery(oTemp);

			String reducedQuery = String.Empty;

			foreach (String variable in m_oTokens.Keys)
			{
				if (variable.Trim().Equals(String.Empty)) continue;

				if (!AdditionalUriTokens.Contains(variable) && !ObsoleteUriTokens.Contains(variable))
					reducedQuery += variable + "=" + m_oTokens[variable] + "&";
			}

			if (reducedQuery.EndsWith("&")) reducedQuery = reducedQuery.Substring(0, reducedQuery.Length - 1);

			oTemp.Query = reducedQuery;

			m_oServer = getServerUri(oTemp);
		}

		private void parseQuery(UriBuilder oTemp)
		{
			m_oTokens = new Hashtable();

			if (oTemp.Query.Length == 0) return;

			String[] aTokens = oTemp.Query.Substring(1).Split(new char[] { '&' });

			foreach (String strToken in aTokens)
			{
				if (strToken.Length == 0) continue;

				if (strToken.Contains("="))
				{
					String[] strParts = strToken.Split(new char[] { '=' });
					strParts[0] = strParts[0].ToLower();
					m_oTokens[strParts[0]] = HttpUtility.UrlDecode(strParts[1]);
					// --- Fix for views that contain ArcIMS layers with maxscale = Double.MaxValue; set it to 1 ---
					if (m_oTokens[strParts[0]].Equals("1.79769313486232E 308"))
						m_oTokens[strParts[0]] = "1";
				}
				else
				{
					m_oTokens[strToken] = null;
				}
			}
		}

		public abstract bool IsValid { get; }

		protected bool AllTokensPresent
		{
			get
			{
				foreach (String strKey in AdditionalUriTokens)
				{
					if (!m_oTokens.ContainsKey(strKey)) return false;
				}
				return true;
			}
		}

		public static LayerUri create(String strUri)
		{
			if (strUri.StartsWith("gxarcims")) return new ArcIMSLayerUri(strUri);
			if (strUri.StartsWith("gxwms")) return new WMSLayerUri(strUri);
			if (strUri.StartsWith("gxdapbm")) return new BrowserMapLayerUri(strUri);
			if (strUri.StartsWith("gxdap")) return new DapLayerUri(strUri);
			if (strUri.StartsWith("gxtile")) return new TileLayerUri(strUri);
			if (strUri.StartsWith("gxve")) return new VELayerUri(strUri);
			if (strUri.StartsWith("gxtif")) return new GeoTifLayerUri(strUri);
			if (strUri.StartsWith("gxkml")) return new KeyholeLayerURI(strUri);
			throw new ArgumentException("Unknown layer uri " + strUri);
		}

		public String getAttribute(String strKey)
		{
			return m_oTokens[strKey.ToLower()] as String;
		}

		protected abstract string LayerScheme { get; }

		public abstract String LayerType { get; }

		protected abstract List<String> AdditionalUriTokens { get; }

		protected abstract List<String> ObsoleteUriTokens { get; }

		protected abstract ServerUri getServerUri(UriBuilder oBuilder);

		public abstract LayerBuilder getBuilder(WorldWindow oWindow, ServerTree oTree);


	}

	public class ArcIMSLayerUri : LayerUri
	{
		private static List<String> m_lAdditionalTokens = new List<String>(new String[] {
         "servicename",
         "layerid",
         "title",
         "minx",
         "miny",
         "maxx",
         "maxy",
         "minscale",
         "maxscale",
			"culture"
         });

		public ArcIMSLayerUri(String strUri) : base(strUri) { }

		protected override string LayerScheme
		{
			get { return "gxarcims"; }
		}

		protected override ServerUri getServerUri(UriBuilder oBuilder)
		{
			return new ArcIMSServerUri(oBuilder.Uri.ToString());
		}

		protected override List<string> AdditionalUriTokens
		{
			get { return m_lAdditionalTokens; }
		}

		protected override List<String> ObsoleteUriTokens
		{
			get
			{
				return new List<string>();
			}
		}

		public override string LayerType
		{
			get { return "ArcIMS"; }
		}

		public override bool IsValid
		{
			get { return AllTokensPresent; }
		}

		public override LayerBuilder getBuilder(WorldWindow oWindow, ServerTree oTree)
		{
			GeographicBoundingBox oLayerBounds = new GeographicBoundingBox();
			double dMinScale, dMaxScale;
			if (!Double.TryParse(getAttribute("minx"), NumberStyles.Any, CultureInfo.InvariantCulture, out oLayerBounds.West)) return null;
			if (!Double.TryParse(getAttribute("miny"), NumberStyles.Any, CultureInfo.InvariantCulture, out oLayerBounds.South)) return null;
			if (!Double.TryParse(getAttribute("maxx"), NumberStyles.Any, CultureInfo.InvariantCulture, out oLayerBounds.East)) return null;
			if (!Double.TryParse(getAttribute("maxy"), NumberStyles.Any, CultureInfo.InvariantCulture, out oLayerBounds.North)) return null;
			if (!Double.TryParse(getAttribute("minscale"), NumberStyles.Any, CultureInfo.InvariantCulture, out dMinScale)) return null;
			if (!Double.TryParse(getAttribute("maxscale"), NumberStyles.Any, CultureInfo.InvariantCulture, out dMaxScale)) return null;
			CultureInfo oServiceCultureInfo = CultureInfo.GetCultureInfoByIetfLanguageTag(getAttribute("culture"));

			return new ArcIMSQuadLayerBuilder(
				m_oServer as ArcIMSServerUri,
				getAttribute("servicename"),
				getAttribute("title"),
				getAttribute("layerid"),
				oLayerBounds,
				oWindow,
				null,
				dMinScale,
				dMaxScale,
				oServiceCultureInfo);
		}
	}

	public class WMSLayerUri : LayerUri
	{
		private static List<String> m_lAdditionalTokens = new List<String>(new String[] {
         "layer"
         });

		private static List<String> m_oObsoleteTokens = new List<String>(new String[] {
			"pixelsize"
		});

		public WMSLayerUri(String strUri) : base(strUri) { }

		protected override string LayerScheme
		{
			get { return "gxwms"; }
		}

		public override string LayerType
		{
			get { return "WMS"; }
		}

		protected override List<string> AdditionalUriTokens
		{
			get { return m_lAdditionalTokens; }
		}

		protected override List<String> ObsoleteUriTokens
		{
			get
			{
				return m_oObsoleteTokens;
			}
		}

		protected override ServerUri getServerUri(UriBuilder oBuilder)
		{
			return new WMSServerUri(oBuilder.Uri.ToString());
		}

		public override bool IsValid
		{
			get { return AllTokensPresent; }
		}

		public override LayerBuilder getBuilder(WorldWindow oWindow, ServerTree oTree)
		{
			// Get the ServerBuilder (need its WMSLayers to make the QuadLayer
			WMSServerBuilder oServerBuilder = oTree.WMSCatalog.GetServer(m_oServer as WMSServerUri);
			if (oServerBuilder == null)
			{
				oTree.AddWMSServer(((WMSServerUri)m_oServer).ToCapabilitiesUri(), true, true, false);
				oServerBuilder = oTree.WMSCatalog.GetServer(m_oServer as WMSServerUri);
			}

			oServerBuilder.WaitUntilLoaded();

			// Throw the loading error up, if there was one
			if (oServerBuilder.LoadingErrorOccurred)
				return null;

			// Otherwise, make a layer and send it up now
			WMSLayer oLayer = WMSQuadLayerBuilder.GetLayer(getAttribute("layer"), oServerBuilder.List.Layers);
			if (oLayer == null)
				return null;

			return new WMSQuadLayerBuilder(oLayer, oWindow, oServerBuilder, null);
		}
	}

	public class TileLayerUri : LayerUri
	{
		private static List<String> m_lAdditionalTokens = new List<String>(new String[] {
         "datasetname",
         "name",
         "height",
         "north",
         "south",
         "east",
         "west",
         "size",
         "levels",
         "lvl0tilesize",
         "terrainmapped",
         "imgfileext"
         });

		public TileLayerUri(String strUri) : base(strUri) { }

		protected override string LayerScheme
		{
			get { return "gxtile"; }
		}

		public override string LayerType
		{
			get { return "Tile"; }
		}

		protected override List<string> AdditionalUriTokens
		{
			get { return m_lAdditionalTokens; }
		}

		protected override List<String> ObsoleteUriTokens
		{
			get
			{
				return new List<string>();
			}
		}

		protected override ServerUri getServerUri(UriBuilder oBuilder)
		{
			return new TileServerUri(oBuilder.Uri.ToString());
		}

		public override bool IsValid
		{
			get { return AllTokensPresent; }
		}

		public override LayerBuilder getBuilder(WorldWindow oWindow, ServerTree oTree)
		{
			GeographicBoundingBox oLayerBounds = new GeographicBoundingBox();
			bool blTerrainMapped;
			int iHeight, iLevels, iSize;
			double dLvl0Tilesie;

			if (!Double.TryParse(getAttribute("west"), NumberStyles.Any, CultureInfo.InvariantCulture, out oLayerBounds.West)) return null;
			if (!Double.TryParse(getAttribute("south"), NumberStyles.Any, CultureInfo.InvariantCulture, out oLayerBounds.South)) return null;
			if (!Double.TryParse(getAttribute("east"), NumberStyles.Any, CultureInfo.InvariantCulture, out oLayerBounds.East)) return null;
			if (!Double.TryParse(getAttribute("north"), NumberStyles.Any, CultureInfo.InvariantCulture, out oLayerBounds.North)) return null;
			if (!Int32.TryParse(getAttribute("height"), NumberStyles.Any, CultureInfo.InvariantCulture, out iHeight)) return null;
			if (!Int32.TryParse(getAttribute("levels"), NumberStyles.Any, CultureInfo.InvariantCulture, out iLevels)) return null;
			if (!Int32.TryParse(getAttribute("size"), NumberStyles.Any, CultureInfo.InvariantCulture, out iSize)) return null;
			if (!Boolean.TryParse(getAttribute("terrainmapped"), out blTerrainMapped)) return null;
			if (!Double.TryParse(getAttribute("lvl0tilesize"), NumberStyles.Any, CultureInfo.InvariantCulture, out dLvl0Tilesie)) return null;

			return new NltQuadLayerBuilder(
				getAttribute("name"),
				iHeight,
				blTerrainMapped,
				oLayerBounds,
				dLvl0Tilesie,
				iLevels,
				iSize,
				m_oServer.ToBaseUri(),
				getAttribute("datasetname"),
				getAttribute("imgfileext"),
				255,
				oWindow,
				null);
		}
	}

	public class VELayerUri : LayerUri
	{
		private static List<String> m_lAdditionalTokens = new List<String>(new String[] { });

		public VELayerUri(String strUri) : base(strUri) { }

		protected override string LayerScheme
		{
			get { return "gxve"; }
		}

		public override string LayerType
		{
			get { return "Virtual Earth"; }
		}

		protected override List<string> AdditionalUriTokens
		{
			get { return m_lAdditionalTokens; }
		}

		protected override List<String> ObsoleteUriTokens
		{
			get
			{
				return new List<string>();
			}
		}

		protected override ServerUri getServerUri(UriBuilder oBuilder)
		{
			return new VEServerUri(oBuilder.Uri.ToString());
		}

		public override bool IsValid
		{
			get
			{
				return (m_oTokens.Keys.Count == 0 &&
					(m_oServer.Host.Equals(VirtualEarthMapType.road.ToString())) ||
					(m_oServer.Host.Equals(VirtualEarthMapType.aerial.ToString())) ||
					(m_oServer.Host.Equals(VirtualEarthMapType.hybrid.ToString()))
					);
			}
		}

		public override LayerBuilder getBuilder(WorldWindow oWindow, ServerTree oTree)
		{
			if (String.Compare(m_oServer.Host, VirtualEarthMapType.road.ToString(), true) == 0)
				return new VEQuadLayerBuilder("Virtual Earth Map", VirtualEarthMapType.road, oWindow, true, null);
			else if (String.Compare(m_oServer.Host, VirtualEarthMapType.aerial.ToString(), true) == 0)
				return new VEQuadLayerBuilder("Virtual Earth Satellite", VirtualEarthMapType.aerial, oWindow, true, null);
			else if (String.Compare(m_oServer.Host, VirtualEarthMapType.hybrid.ToString(), true) == 0)
				return new VEQuadLayerBuilder("Virtual Earth Map & Satellite", VirtualEarthMapType.hybrid, oWindow, true, null);
			else
				return null;
		}
	}

	public class DapLayerUri : LayerUri
	{
		private static List<String> m_lAdditionalTokens = new List<String>(new String[] {
         "datasetname",
         "height",
         "size",
         "type",
         "title",
         "edition",
         "hierarchy",
         "north",
         "south",
         "east",
         "west",
         "levels",
         "lvl0tilesize"
         });

		public DapLayerUri(String strUri) : base(strUri) { }

		protected override string LayerScheme
		{
			get { return "gxdap"; }
		}

		public override string LayerType
		{
			get { return "Dap"; }
		}

		protected override List<string> AdditionalUriTokens
		{
			get { return m_lAdditionalTokens; }
		}

		protected override List<String> ObsoleteUriTokens
		{
			get
			{
				return new List<string>();
			}
		}

		protected override ServerUri getServerUri(UriBuilder oBuilder)
		{
			return new DapServerUri(oBuilder.Uri.ToString());
		}

		public override bool IsValid
		{
			get { return AllTokensPresent; }
		}

		public override LayerBuilder getBuilder(WorldWindow oWindow, ServerTree oTree)
		{
			DataSet hDataSet = new DataSet();
			hDataSet.Name = getAttribute("datasetname");
			hDataSet.Url = m_oServer.ToBaseUri();
			hDataSet.Type = getAttribute("type");
			hDataSet.Title = getAttribute("title");
			hDataSet.Edition = getAttribute("edition");
			hDataSet.Hierarchy = getAttribute("hierarchy");

			double minX, minY, maxX, maxY;
			if (!Double.TryParse(getAttribute("west"), NumberStyles.Any, CultureInfo.InvariantCulture, out minX)) return null;
			if (!Double.TryParse(getAttribute("south"), NumberStyles.Any, CultureInfo.InvariantCulture, out minY)) return null;
			if (!Double.TryParse(getAttribute("east"), NumberStyles.Any, CultureInfo.InvariantCulture, out maxX)) return null;
			if (!Double.TryParse(getAttribute("north"), NumberStyles.Any, CultureInfo.InvariantCulture, out maxY)) return null;
			hDataSet.Boundary = new Geosoft.Dap.Common.BoundingBox(maxX, maxY, minX, minY);

			int height, size, levels;
			double lvl0tilesize;
			if (!Int32.TryParse(getAttribute("height"), NumberStyles.Any, CultureInfo.InvariantCulture, out height)) return null;
			if (!Int32.TryParse(getAttribute("size"), NumberStyles.Any, CultureInfo.InvariantCulture, out size)) return null;
			if (!Int32.TryParse(getAttribute("levels"), NumberStyles.Any, CultureInfo.InvariantCulture, out levels)) return null;
			if (!Double.TryParse(getAttribute("lvl0tilesize"), NumberStyles.Any, CultureInfo.InvariantCulture, out lvl0tilesize)) return null;

			Geosoft.GX.DAPGetData.Server oServer = null;
			if (!oTree.FullServerList.ContainsKey(m_oServer.ToBaseUri()))
				oTree.AddDAPServer(m_oServer.ToBaseUri(), out oServer, true, false);
			else
				oServer = oTree.FullServerList[m_oServer.ToBaseUri()];

			return new DAPQuadLayerBuilder(hDataSet, oWindow, oServer, null, height, size, lvl0tilesize, levels);
		}
	}

	public class BrowserMapLayerUri : LayerUri
	{
		private static List<String> m_lAdditionalTokens = new List<String>(new String[] {
         });

		public BrowserMapLayerUri(String strUri) : base(strUri) { }

		protected override string LayerScheme
		{
			get { return "gxdapbm"; }
		}

		public override string LayerType
		{
			get { return "DAP browser map"; }
		}

		protected override List<string> AdditionalUriTokens
		{
			get { return m_lAdditionalTokens; }
		}

		protected override List<String> ObsoleteUriTokens
		{
			get
			{
				return new List<string>();
			}
		}

		protected override ServerUri getServerUri(UriBuilder oBuilder)
		{
			return new DapServerUri(oBuilder.Uri.ToString());
		}

		public override bool IsValid
		{
			get { return AllTokensPresent; }
		}

		public override LayerBuilder getBuilder(WorldWindow oWindow, ServerTree oTree)
		{
			Geosoft.GX.DAPGetData.Server oServer = null;
			if (!oTree.FullServerList.ContainsKey(m_oServer.ToBaseUri()))
				oTree.AddDAPServer(m_oServer.ToBaseUri(), out oServer, true, false);
			else
				oServer = oTree.FullServerList[m_oServer.ToBaseUri()];

			return new DAPBrowserMapBuilder(oWindow, oServer, null);
		}
	}

	public class GeoTifLayerUri : LayerUri
	{
		private static List<String> m_lAdditionalTokens = new List<String>(new String[] { });

		public GeoTifLayerUri(String strUri) : base(strUri) { }

		protected override string LayerScheme
		{
			get { return "gxtif"; }
		}

		public override string LayerType
		{
			get { return "GeoTiff"; }
		}

		protected override List<string> AdditionalUriTokens
		{
			get { return m_lAdditionalTokens; }
		}

		protected override List<String> ObsoleteUriTokens
		{
			get
			{
				return new List<string>();
			}
		}

		protected override ServerUri getServerUri(UriBuilder oBuilder)
		{
			return new GeoTiffFileUri(oBuilder.Uri.ToString());
		}

		public override bool IsValid
		{
			get
			{
				return true;
			}
		}

		public override LayerBuilder getBuilder(WorldWindow oWindow, ServerTree oTree)
		{
			return new GeorefImageLayerBuilder(m_oServer.LocalPath, false, oWindow, null);
		}
	}

	public class KeyholeLayerURI : LayerUri
	{
		private static List<String> m_lAdditionalTokens = new List<String>(new String[] { });

		public KeyholeLayerURI(String strUri) : base(strUri) { }

		public override bool IsValid
		{
			get { return true; }
		}

		protected override string LayerScheme
		{
			get { return "gxkml"; }
		}

		public override string LayerType
		{
			get { return "KML"; }
		}

		protected override List<string> AdditionalUriTokens
		{
			get { return m_lAdditionalTokens; }
		}

		protected override List<string> ObsoleteUriTokens
		{
			get { return new List<String>(); }
		}

		protected override ServerUri getServerUri(UriBuilder oBuilder)
		{
			return new KeyholeFileUri(oBuilder.Uri.ToString());
		}

		public override LayerBuilder getBuilder(WorldWindow oWindow, ServerTree oTree)
		{
			return new KML.KMLLayerBuilder(m_oServer.LocalPath, oWindow, null);
		}
	}
}
