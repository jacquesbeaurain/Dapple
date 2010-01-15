using System;
using System.IO;
using System.Net;
using System.ServiceModel.Web;
using System.Web;
using GED.Core;
using Geosoft.Dap.Common;
using System.Drawing;

namespace GED.WebService
{
	internal class Implementation : Contract
	{
		public Stream GenerateTileKml(String strServerType, String strServer, String strLayer, int iLevel, int iCol, int iRow, double dMinX, double dMinY, double dMaxX, double dMaxY)
		{
			strServerType = HttpUtility.UrlDecode(strServerType);
			strServer = HttpUtility.UrlDecode(strServer);
			strLayer = HttpUtility.UrlDecode(strLayer);

			TileInfo oTile = new TileInfo(iLevel, iCol, iRow);
			BoundingBox oLayerBounds = new BoundingBox(dMaxX, dMaxY, dMinX, dMinY);
			System.Diagnostics.Debug.Assert(oLayerBounds.MaxX >= oLayerBounds.MinX);
			System.Diagnostics.Debug.Assert(oLayerBounds.MaxY >= oLayerBounds.MinY);

			WebOperationContext.Current.OutgoingResponse.ContentType = "text/xml";
			return TileKmlGenerator.GenerateTileKml(strServerType, strServer, strLayer, oTile, oLayerBounds);
		}

		public Stream GetCachedImageTile(string strServerType, string strServer, string strLayer, int iLevel, int iCol, int iRow)
		{
			strServerType = HttpUtility.UrlDecode(strServerType);
			strServer = HttpUtility.UrlDecode(strServer);
			strLayer = HttpUtility.UrlDecode(strLayer);
			TileInfo oTile = new TileInfo(iLevel, iCol, iRow);
			LayerInfo oLayer = new LayerInfo(strServerType, strServer, strLayer);

			#region // Input checking
			if (String.IsNullOrEmpty(strServerType) ||
				String.IsNullOrEmpty(strServer) ||
				String.IsNullOrEmpty(strLayer))
			{
				ReportStatusCode(HttpStatusCode.BadRequest);
				return null;
			}
			#endregion

			byte[] oImage = oLayer.GetTileImage(oTile);
			if (oImage != null)
			{
				return new MemoryStream(oImage);
			}
			else
			{
				ReportStatusCode(HttpStatusCode.NotFound);
				return null;
			}
		}

		public Stream GenerateSingleImage(string strServerType, string strServer, string strLayer, string strBBox, double dMinX, double dMinY, double dMaxX, double dMaxY)
		{
			strServerType = HttpUtility.UrlDecode(strServerType);
			strServer = HttpUtility.UrlDecode(strServer);
			strLayer = HttpUtility.UrlDecode(strLayer);
			BoundingBox oViewBounds;
			BoundingBox oLayerBounds = new BoundingBox(dMaxX, dMaxY, dMinX, dMinY);
			System.Diagnostics.Debug.Assert(oLayerBounds.MaxX >= oLayerBounds.MinX);
			System.Diagnostics.Debug.Assert(oLayerBounds.MaxY >= oLayerBounds.MinY);
			LayerInfo oLayer = new LayerInfo(strServerType, strServer, strLayer);

			#region // Input checking
			oViewBounds = ParseBBoxToken(strBBox);
			if (oViewBounds == null)
			{
				ReportStatusCode(HttpStatusCode.BadRequest);
				return null;
			}
			System.Diagnostics.Debug.Assert(oViewBounds.MaxX >= oViewBounds.MinX);
			System.Diagnostics.Debug.Assert(oViewBounds.MaxY >= oViewBounds.MinY);
			#endregion

			byte[] oImage = oLayer.GetCompositeImage(oLayerBounds, oViewBounds);
			if (oImage != null)
			{
				return new MemoryStream(oImage);
			}
			else
			{
				ReportStatusCode(HttpStatusCode.NotFound);
				return null;
			}
		}

		public Stream DappleSearchOverDAP(String dapUrl)
		{
			return DappleSearch.DappleSearch.OverDAP(dapUrl, System.ServiceModel.Web.WebOperationContext.Current.IncomingRequest.Headers["GeosoftMapSearchRequest"]);
		}

		public Stream ThumbnailOverDAP(string dapUrl, int layerID)
		{
			return DappleSearch.DappleSearch.Thumbnail(dapUrl, layerID);
		}

		private void ReportStatusCode(HttpStatusCode oCode)
		{
			WebOperationContext oContext = WebOperationContext.Current;
			oContext.OutgoingResponse.StatusCode = oCode;
		}

		private BoundingBox ParseBBoxToken(String strBBox)
		{
			double dMinX, dMinY, dMaxX, dMaxY;

			String[] oBBox = strBBox.Split(',');
			if (oBBox.Length != 4)
			{
				return null;
			}
			if (!Double.TryParse(oBBox[0], out dMinX))
			{
				return null;
			}
			if (!Double.TryParse(oBBox[1], out dMinY))
			{
				return null;
			}
			if (!Double.TryParse(oBBox[2], out dMaxX))
			{
				return null;
			}
			if (!Double.TryParse(oBBox[3], out dMaxY))
			{
				return null;
			}


			// --- Google Earth swaps MinX and MaxX for views which include the poles ---

			if (dMinX == 180.0 && dMaxX == -180.0)
			{
				dMinX = -180.0;
				dMaxX = 180.0;
			}

			return new BoundingBox(dMaxX, dMaxY, dMinX, dMinY);
		}
	}
}
