using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Web;
using System.Collections;
using WorldWind.PluginEngine;
using WorldWind;
using Geosoft.Dap.Common;

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
         String result = base.Host + base.AbsolutePath;

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
         : base(TrimCapabilitiesUri(strValue))
      {
      }

      public String ToCapabilitiesUri()
      {
         return base.ToString()
            + (base.ToString().IndexOf("?") > 0 ? "&" : "?")
            + "request=GetCapabilities&service=WMS";
      }

      private static String TrimCapabilitiesUri(String serverUrl)
      {
         int iIndex;

         // Remove request=getcapabilities query token
         string strTemp = serverUrl.ToLower();
         iIndex = strTemp.IndexOf("request=getcapabilities");

         if (iIndex != -1)
            serverUrl = serverUrl.Substring(0, iIndex) + serverUrl.Substring(iIndex + "request=getcapabilities".Length);

         // Remove service=wms query token
         strTemp = serverUrl.ToLower();
         iIndex = strTemp.IndexOf("service=wms");
         if (iIndex != -1)
            serverUrl = serverUrl.Substring(0, iIndex) + serverUrl.Substring(iIndex + "service=wms".Length);

         // Remove duplicate amphersands
         while (serverUrl.IndexOf("&&") != -1)
            serverUrl = serverUrl.Replace("&&", "&");

         // Remove '&'s and '?'s from the end of the string
         serverUrl = serverUrl.TrimEnd(new char[] { '?', '&' });
         //serverUrl = serverUrl.TrimEnd(new char[] { '&' });

         // Remove whitespace on either end
         serverUrl = serverUrl.Trim();

         return serverUrl;
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
         String strUri =  base.ToString()
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

         if (strUri.StartsWith("gxtif"))
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

            if (!AdditionalUriTokens.Contains(variable))
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
               m_oTokens[strParts[0].ToLower()] = HttpUtility.UrlDecode(strParts[1]);
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
         throw new ArgumentException("Unknown layer uri " + strUri);
      }

      public ServerUri Server { get { return m_oServer; } }

      public String getAttribute(String strKey)
      {
         return m_oTokens[strKey.ToLower()] as String;
      }

      protected abstract string LayerScheme { get; }

      public abstract String LayerType { get; }

      protected abstract List<String> AdditionalUriTokens { get; }

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
         "maxscale"
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
         return new ArcIMSQuadLayerBuilder(
            m_oServer as ArcIMSServerUri,
            getAttribute("servicename"),
            getAttribute("title"),
            getAttribute("layerid"),
            new GeographicBoundingBox(double.Parse(getAttribute("maxy")), double.Parse(getAttribute("miny")), double.Parse(getAttribute("minx")), double.Parse(getAttribute("maxx"))),
            oWindow,
            null,
            Double.Parse(getAttribute("minscale")),
            Double.Parse(getAttribute("maxscale")));
      }
   }

   public class WMSLayerUri : LayerUri
   {
      private static List<String> m_lAdditionalTokens = new List<String>(new String[] {
         "layer"
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
            oTree.AddWMSServer(((WMSServerUri)m_oServer).ToCapabilitiesUri(), true, true);
            oServerBuilder = oTree.WMSCatalog.GetServer(m_oServer as WMSServerUri);
         }

         oServerBuilder.WaitUntilLoaded();

         // Throw the loading error up, if there was one
         if (oServerBuilder.LoadingErrorOccurred)
            throw new Exception("Could not access server " + m_oServer.ToBaseUri());

         // Otherwise, make a layer and send it up now
         WMSLayer oLayer = WMSQuadLayerBuilder.GetLayer(getAttribute("layer"), oServerBuilder.List.Layers);
         if (oLayer == null)
            throw new Exception("Server doesn't have a layer named " + getAttribute("layer"));

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
         return new NltQuadLayerBuilder(
            getAttribute("name"),
            int.Parse(getAttribute("height")),
            bool.Parse(getAttribute("terrainmapped")),
            new GeographicBoundingBox(double.Parse(getAttribute("north")), double.Parse(getAttribute("south")), double.Parse(getAttribute("west")), double.Parse(getAttribute("east"))),
            double.Parse(getAttribute("lvl0tilesize")),
            int.Parse(getAttribute("levels")),
            int.Parse(getAttribute("size")),
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
      private static List<String> m_lAdditionalTokens = new List<String>(new String[] {});

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
         hDataSet.Boundary = new Geosoft.Dap.Common.BoundingBox(
            double.Parse(getAttribute("east")),
            double.Parse(getAttribute("north")),
            double.Parse(getAttribute("west")),
            double.Parse(getAttribute("south")));

         int height = Convert.ToInt32(getAttribute("height"));
         int size = Convert.ToInt32(getAttribute("size"));
         int levels = int.Parse(getAttribute("levels"));
         double lvl0tilesize = double.Parse(getAttribute("lvl0tilesize"));

         Geosoft.GX.DAPGetData.Server oServer = null;
         if (!oTree.FullServerList.ContainsKey(m_oServer.ToBaseUri()))
            oTree.AddDAPServer(m_oServer.ToBaseUri(), out oServer, true);
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
            oTree.AddDAPServer(m_oServer.ToBaseUri(), out oServer, true);
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
}
