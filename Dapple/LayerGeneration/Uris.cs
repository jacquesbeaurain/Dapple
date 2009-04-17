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
using NewServerTree;
using System.Windows.Forms;

namespace Dapple.LayerGeneration
{
	/// <summary>
	/// Class for dealing with URIs referring to a server's major HTTP entryway.
	/// </summary>
	internal abstract class ServerUri : Uri
	{
		internal ServerUri(String strValue)
			: base(strValue)
		{ }

		internal ServerUri(UriBuilder oValue)
			: base(oValue.Uri.ToString())
		{ }

		/// <summary>
		/// Get a cache directory for this Uri (host and path, minus query and scheme.
		/// </summary>
		/// <returns></returns>
		internal virtual String ToCacheDirectory()
		{
			String result = base.Host + base.AbsolutePath + base.Query;

			// Convert invalid characters to underscores
			foreach (Char ch in System.IO.Path.GetInvalidFileNameChars())
				result = result.Replace(ch.ToString(), "_");

			// And done
			return result;
		}

		internal String ToBaseUri()
		{
			return base.ToString();
		}

		internal String ServerTreeDisplayName
		{
			get
			{
				String result = this.Host;
				if (this.Port != 80)
				{
					result += ":" + this.Port.ToString(CultureInfo.InvariantCulture);
				}
				if (!this.PathAndQuery.Trim().Equals("/servlet/com.esri.esrimap.Esrimap", StringComparison.InvariantCultureIgnoreCase))
				{
					result += this.PathAndQuery;
				}

				return result;
			}
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

	internal class WMSServerUri : ServerUri
	{
		internal WMSServerUri(String strValue)
			: base(GetFilteredUriBuilder(strValue))
		{
		}

		internal static UriBuilder GetFilteredUriBuilder(String strValue)
		{
			UriBuilder oBuilder = new UriBuilder(strValue);
			// --- Remove the service, version, and request query variables ---
			NameValueCollection oTokens = HttpUtility.ParseQueryString(oBuilder.Query);
			foreach (String szKey in oTokens.AllKeys)
			{
				if (szKey == null ||
					szKey.Equals("service", StringComparison.InvariantCultureIgnoreCase) ||
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

		internal String ToCapabilitiesUri()
		{
			return base.ToString()
				+ (base.ToString().IndexOf("?") > 0 ? "&" : "?")
				+ "request=GetCapabilities&service=WMS";
		}
	}

	internal class ArcIMSServerUri : ServerUri
	{
		internal ArcIMSServerUri(String strValue)
			: base(strValue)
		{ }

		/// <summary>
		/// Get a cache directory for this Uri (host and path, minus query and scheme.
		/// </summary>
		/// <returns></returns>
		internal override String ToCacheDirectory()
		{
			String result = base.Host;

			// Convert invalid characters to underscores
			foreach (Char ch in System.IO.Path.GetInvalidFileNameChars())
				result = result.Replace(ch.ToString(), "_");

			result += "-" + this.ToString().GetHashCode().ToString("X", CultureInfo.InvariantCulture);

			// And done
			return result;
		}

		internal String ToCatalogUri()
		{
			return base.ToString()
				+ (base.ToString().IndexOf("?") > 0 ? "&" : "?")
				+ "ServiceName=catalog&ClientVersion=9.0";
		}

		internal String ToServiceUri(String serviceName)
		{
			return base.ToString()
				+ (base.ToString().IndexOf("?") > 0 ? "&" : "?")
				+ "ServiceName=" + serviceName + "&ClientVersion=9.0";
		}

		internal ArcIMSLayerUri ToLayerUri(String strServiceName, GeographicBoundingBox oBox)
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

	internal class TileServerUri : ServerUri
	{
		internal TileServerUri(String strUri) : base(strUri) { }
	}

	internal class VEServerUri : ServerUri
	{
		internal VEServerUri(String strUri) : base(strUri) { }
	}

	internal class DapServerUri : ServerUri
	{
		internal DapServerUri(String strUri) : base(strUri) { }

		internal bool IsForPersonalDAP
		{
			get
			{
				return this.Host.Equals("localhost", StringComparison.InvariantCultureIgnoreCase)
					&& this.Port == 10205
					&& this.AbsolutePath.Equals("/");
			}
		}
	}

	internal class GeoTiffFileUri : ServerUri
	{
		internal GeoTiffFileUri(String strUri) : base(strUri) { }
	}

	internal class KeyholeFileUri : ServerUri
	{
		internal KeyholeFileUri(String strUri) : base(strUri) { }
	}

	/// <summary>
	/// Class for dealing with internal layer URIs.  Parses necessary informaion and removes it from the Server URI.
	/// </summary>
	internal abstract class LayerUri
	{
		protected ServerUri m_oServer;
		protected Hashtable m_oTokens;

		internal LayerUri(String strUri)
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

		internal abstract bool IsValid { get; }

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

		internal static LayerUri create(String strUri)
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

		internal String getAttribute(String strKey)
		{
			return m_oTokens[strKey.ToLower()] as String;
		}

		protected abstract string LayerScheme { get; }

		internal abstract String LayerType { get; }

		protected abstract List<String> AdditionalUriTokens { get; }

		protected abstract List<String> ObsoleteUriTokens { get; }

		protected abstract ServerUri getServerUri(UriBuilder oBuilder);

		internal abstract LayerBuilder getBuilder(DappleModel oModel);


	}

	internal class ArcIMSLayerUri : LayerUri
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
         "maxscale"
         });

		internal ArcIMSLayerUri(String strUri) : base(strUri) { }

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

		internal override string LayerType
		{
			get { return "ArcIMS"; }
		}

		internal override bool IsValid
		{
			get { return AllTokensPresent; }
		}

		internal override LayerBuilder getBuilder(DappleModel oModel)
		{
			GeographicBoundingBox oLayerBounds = new GeographicBoundingBox();
			double dMinScale, dMaxScale;
			if (!Double.TryParse(getAttribute("minx"), NumberStyles.Any, CultureInfo.InvariantCulture, out oLayerBounds.West)) return null;
			if (!Double.TryParse(getAttribute("miny"), NumberStyles.Any, CultureInfo.InvariantCulture, out oLayerBounds.South)) return null;
			if (!Double.TryParse(getAttribute("maxx"), NumberStyles.Any, CultureInfo.InvariantCulture, out oLayerBounds.East)) return null;
			if (!Double.TryParse(getAttribute("maxy"), NumberStyles.Any, CultureInfo.InvariantCulture, out oLayerBounds.North)) return null;
			if (!Double.TryParse(getAttribute("minscale"), NumberStyles.Any, CultureInfo.InvariantCulture, out dMinScale)) return null;
			if (!Double.TryParse(getAttribute("maxscale"), NumberStyles.Any, CultureInfo.InvariantCulture, out dMaxScale)) return null;

			return new ArcIMSQuadLayerBuilder(
				m_oServer as ArcIMSServerUri,
				getAttribute("servicename"),
				getAttribute("title"),
				getAttribute("layerid"),
				oLayerBounds,
				MainForm.WorldWindowSingleton,
				null,
				dMinScale,
				dMaxScale,
				new CultureInfo("en-US"));
		}
	}

	internal class WMSLayerUri : LayerUri
	{
		private static List<String> m_lAdditionalTokens = new List<String>(new String[] {
         "layer"
         });

		private static List<String> m_oObsoleteTokens = new List<String>(new String[] {
			"pixelsize"
		});

		internal WMSLayerUri(String strUri) : base(strUri) { }

		protected override string LayerScheme
		{
			get { return "gxwms"; }
		}

		internal override string LayerType
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

		internal override bool IsValid
		{
			get { return AllTokensPresent; }
		}

		internal override LayerBuilder getBuilder(DappleModel oModel)
		{
			WMSServerModelNode oServer = oModel.AddWMSServer(m_oServer as WMSServerUri, true, false, false) as WMSServerModelNode;
			if (oServer.Enabled == false)
				if (DialogResult.Yes == MessageBox.Show(String.Format(CultureInfo.InvariantCulture, "The WMS server {1} is in your server list, but is disabled.{2}'{0}' cannot be displayed unless the server is re-enabled.{2}{2}Do you wish to enable {1}?", getAttribute("layer"), m_oServer.ServerTreeDisplayName, Environment.NewLine),
					"Server is Disabled", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1))
					oModel.ToggleServer(oServer, true);
				else
					return null;

			oServer.WaitForLoad();
			if (oServer.LoadState == LoadState.LoadFailed) return null;

			WMSLayerModelNode oLayer = oServer.GetLayer(getAttribute("layer"));
			if (oLayer == null) throw new ArgumentException("'" + getAttribute("layer") + "' was not found. This dataset may have been deleted or moved." + Environment.NewLine + Environment.NewLine + "Please check with the publisher of " + m_oServer.ServerTreeDisplayName + " for more information.");
			WMSServerBuilder oDummyServer = new WMSServerBuilder(null, m_oServer as WMSServerUri, oServer.CapabilitiesFilename, true);
			return new WMSQuadLayerBuilder(oLayer.LayerData, MainForm.WorldWindowSingleton, oDummyServer, null);
		}
	}

	internal class TileLayerUri : LayerUri
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

		internal TileLayerUri(String strUri) : base(strUri) { }

		protected override string LayerScheme
		{
			get { return "gxtile"; }
		}

		internal override string LayerType
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

		internal override bool IsValid
		{
			get { return AllTokensPresent; }
		}

		internal override LayerBuilder getBuilder(DappleModel oModel)
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
				MainForm.WorldWindowSingleton,
				null);
		}
	}

	internal class VELayerUri : LayerUri
	{
		private static List<String> m_lAdditionalTokens = new List<String>(new String[] { });

		internal VELayerUri(String strUri) : base(strUri) { }

		protected override string LayerScheme
		{
			get { return "gxve"; }
		}

		internal override string LayerType
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

		internal override bool IsValid
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

		internal override LayerBuilder getBuilder(DappleModel oModel)
		{
			if (String.Compare(m_oServer.Host, VirtualEarthMapType.road.ToString(), true) == 0)
				return new VEQuadLayerBuilder("Virtual Earth Map", VirtualEarthMapType.road, MainForm.WorldWindowSingleton, true, null);
			else if (String.Compare(m_oServer.Host, VirtualEarthMapType.aerial.ToString(), true) == 0)
				return new VEQuadLayerBuilder("Virtual Earth Satellite", VirtualEarthMapType.aerial, MainForm.WorldWindowSingleton, true, null);
			else if (String.Compare(m_oServer.Host, VirtualEarthMapType.hybrid.ToString(), true) == 0)
				return new VEQuadLayerBuilder("Virtual Earth Map & Satellite", VirtualEarthMapType.hybrid, MainForm.WorldWindowSingleton, true, null);
			else
				return null;
		}
	}

	internal class DapLayerUri : LayerUri
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

		internal DapLayerUri(String strUri) : base(strUri) { }

		protected override string LayerScheme
		{
			get { return "gxdap"; }
		}

		internal override string LayerType
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

		internal override bool IsValid
		{
			get { return AllTokensPresent; }
		}

		internal override LayerBuilder getBuilder(DappleModel oModel)
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

			DapServerModelNode oServerMN = oModel.AddDAPServer(m_oServer as DapServerUri, true, false, false) as DapServerModelNode;
			oServerMN.WaitForLoad();
			if (oServerMN.LoadState == LoadState.LoadFailed) return null;

			Geosoft.GX.DAPGetData.Server oServer = oServerMN.Server;
			return new DAPQuadLayerBuilder(hDataSet, MainForm.WorldWindowSingleton, oServer, null, height, size, lvl0tilesize, levels);
		}
	}

	internal class BrowserMapLayerUri : LayerUri
	{
		private static List<String> m_lAdditionalTokens = new List<String>(new String[] {
         });

		internal BrowserMapLayerUri(String strUri) : base(strUri) { }

		protected override string LayerScheme
		{
			get { return "gxdapbm"; }
		}

		internal override string LayerType
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

		internal override bool IsValid
		{
			get { return AllTokensPresent; }
		}

		internal override LayerBuilder getBuilder(DappleModel oModel)
		{
			DapServerModelNode oServerMN = oModel.AddDAPServer(m_oServer as DapServerUri, true, false, false) as DapServerModelNode;
			oServerMN.WaitForLoad();
			if (oServerMN.LoadState == LoadState.LoadFailed) return null;

			Geosoft.GX.DAPGetData.Server oServer = oServerMN.Server;
			return new DAPBrowserMapBuilder(MainForm.WorldWindowSingleton, oServer, null);
		}
	}

	internal class GeoTifLayerUri : LayerUri
	{
		private static List<String> m_lAdditionalTokens = new List<String>(new String[] { });

		internal GeoTifLayerUri(String strUri) : base(strUri) { }

		protected override string LayerScheme
		{
			get { return "gxtif"; }
		}

		internal override string LayerType
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

		internal override bool IsValid
		{
			get
			{
				return true;
			}
		}

		internal override LayerBuilder getBuilder(DappleModel oModel)
		{
			return new GeorefImageLayerBuilder(m_oServer.LocalPath, false, MainForm.WorldWindowSingleton, null);
		}
	}

	internal class KeyholeLayerURI : LayerUri
	{
		private static List<String> m_lAdditionalTokens = new List<String>(new String[] { });

		internal KeyholeLayerURI(String strUri) : base(strUri) { }

		internal override bool IsValid
		{
			get { return true; }
		}

		protected override string LayerScheme
		{
			get { return "gxkml"; }
		}

		internal override string LayerType
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

		internal override LayerBuilder getBuilder(DappleModel oModel)
		{
			return new KML.KMLLayerBuilder(m_oServer.LocalPath, MainForm.WorldWindowSingleton, null);
		}
	}
}
