using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.IO;
using System.Collections.Specialized;
using System.Web;
using System.Globalization;
using Geosoft.Dap.Common;
using GEDCore;

namespace GED.WebService
{
	[ServiceContract]
	internal interface Contract
	{
		[OperationContract]
		[WebGet(UriTemplate = ContractHelper.ImageCacheUriTemplate)]
		Stream GetCachedImageTile(String serverType, String server, String layer, int level, int col, int row);

		[OperationContract]
		[WebGet(UriTemplate = ContractHelper.TileKmlUriTemplate)]
		Stream GenerateTileKml(String serverType, String server, String layer, int level, int col, int row, double minx, double miny, double maxx, double maxy);		

		[OperationContract]
		[WebGet(UriTemplate = ContractHelper.SingleImageUriTemplate)]
		Stream GenerateSingleImage(String serverType, String server, String layer, String bbox, double minx, double miny, double maxx, double maxy);
	}


	public static class ContractHelper
	{
		internal const string ImageCacheUriTemplate = "/ImageCache/?servertype={serverType}&server={server}&layer={layer}&level={level}&col={col}&row={row}";
		public static Uri BindImageCacheUriTemplate(Uri oBaseAddress, String strServerType, String strServer, String strLayer, TileInfo oTile)
		{
			UriTemplate oTemplate = new UriTemplate(ImageCacheUriTemplate);

			NameValueCollection oParameters = new NameValueCollection();
			oParameters.Add("serverType", HttpUtility.UrlEncode(strServerType));
			oParameters.Add("server", HttpUtility.UrlEncode(strServer));
			oParameters.Add("layer", HttpUtility.UrlEncode(strLayer));
			oParameters.Add("level", oTile.Level.ToString(CultureInfo.InvariantCulture));
			oParameters.Add("col", oTile.Column.ToString(CultureInfo.InvariantCulture));
			oParameters.Add("row", oTile.Row.ToString(CultureInfo.InvariantCulture));

			return oTemplate.BindByName(oBaseAddress, oParameters);
		}

		internal const string TileKmlUriTemplate = "/TileKml/?servertype={serverType}&server={server}&layer={layer}&level={level}&col={col}&row={row}&minx={minx}&miny={miny}&maxx={maxx}&maxy={maxy}";
		public static Uri BindTileKmlUriTemplate(Uri oBaseAddress, String strServerType, String strServer, String strLayer, TileInfo oTile, BoundingBox oLayerBounds)
		{
			UriTemplate oTemplate = new UriTemplate(TileKmlUriTemplate);

			NameValueCollection oParameters = new NameValueCollection();
			oParameters.Add("serverType", HttpUtility.UrlEncode(strServerType));
			oParameters.Add("server", HttpUtility.UrlEncode(strServer));
			oParameters.Add("layer", HttpUtility.UrlEncode(strLayer));
			oParameters.Add("level", oTile.Level.ToString(CultureInfo.InvariantCulture));
			oParameters.Add("col", oTile.Column.ToString(CultureInfo.InvariantCulture));
			oParameters.Add("row", oTile.Row.ToString(CultureInfo.InvariantCulture));
			oParameters.Add("minx", oLayerBounds.MinX.ToString(CultureInfo.InvariantCulture));
			oParameters.Add("miny", oLayerBounds.MinY.ToString(CultureInfo.InvariantCulture));
			oParameters.Add("maxx", oLayerBounds.MaxX.ToString(CultureInfo.InvariantCulture));
			oParameters.Add("maxy", oLayerBounds.MaxY.ToString(CultureInfo.InvariantCulture));

			return oTemplate.BindByName(oBaseAddress, oParameters);
		}

		internal const string PartialSingleImageUriTemplate = "/ConstructImage/?servertype={serverType}&server={server}&layer={layer}&minx={minx}&miny={miny}&maxx={maxx}&maxy={maxy}";
		internal const string SingleImageUriTemplate = PartialSingleImageUriTemplate + "&bbox={bbox}";
		public static Uri PartialBindSingleImageUriTemplate(Uri oBaseAddress, String strServerType, String strServer, String strLayer, BoundingBox oLayerBounds)
		{
			UriTemplate oTemplate = new UriTemplate(SingleImageUriTemplate);

			NameValueCollection oParameters = new NameValueCollection();
			oParameters.Add("serverType", HttpUtility.UrlEncode(strServerType));
			oParameters.Add("server", HttpUtility.UrlEncode(strServer));
			oParameters.Add("layer", HttpUtility.UrlEncode(strLayer));
			oParameters.Add("minx", oLayerBounds.MinX.ToString(CultureInfo.InvariantCulture));
			oParameters.Add("miny", oLayerBounds.MinY.ToString(CultureInfo.InvariantCulture));
			oParameters.Add("maxx", oLayerBounds.MaxX.ToString(CultureInfo.InvariantCulture));
			oParameters.Add("maxy", oLayerBounds.MaxY.ToString(CultureInfo.InvariantCulture));

			return oTemplate.BindByName(oBaseAddress, oParameters);
		}
	}
}
