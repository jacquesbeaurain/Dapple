using System.IO;
using System.Xml;
using Geosoft.Dap.Common;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Drawing;
using GED.Core;

namespace GED.WebService.DappleSearch
{
	static class DappleSearch
	{
		static Dictionary<int, DataSet> cachedData = new Dictionary<int, DataSet>();

		internal static Stream OverDAP(string dapUrl, string requestXML)
		{
			SearchRequest request = Decoder.SearchRequest(requestXML);

			Geosoft.Dap.Command c = new Geosoft.Dap.Command(dapUrl, false, Geosoft.Dap.Command.Version.GEOSOFT_XML_1_1, false, DapSecureToken.Instance, 30000);
			ArrayList datasets; c.GetCatalog(null, 0, request.Offset, request.MaxCount, request.TextFilter, request.AoIFilter, out datasets);
			int totalResults; c.GetDataSetCount(null, 0, request.Offset, request.MaxCount, request.TextFilter, request.AoIFilter, out totalResults);

			SearchResults result = new SearchResults(request.Version, request.Handle, request.Offset, totalResults);
			cachedData.Clear();
			foreach (DataSet d in datasets)
			{
				result.AddResult(new DAPLayer(d));
				cachedData.Add(int.Parse(d.Name), d);
			}

			return Encoder.SearchResults(result);
		}

		internal static Stream Thumbnail(string dapUrl, int layerID)
		{
			Geosoft.Dap.Command c = new Geosoft.Dap.Command(dapUrl, false, Geosoft.Dap.Command.Version.GEOSOFT_XML_1_1, false, DapSecureToken.Instance, 30000);

			DataSet data = cachedData[layerID];

			ArrayList datasets = new ArrayList();
			datasets.Add(data.Name);

			double width = data.Boundary.MaxX - data.Boundary.MinX;
			double height = data.Boundary.MaxY - data.Boundary.MinY;

			Format format = new Format() { Transparent = true, Type = "image/png" };

			const int TileSize = 100;
			const int Upscale = 4;

			Resolution rez;
			if (width > height)
				rez = new Resolution() { Width = TileSize * Upscale, Height = (int)Math.Max(1.0, (double)TileSize * Upscale * height / width) };
			else if (height > width)
				rez = new Resolution() { Width = (int)Math.Max(1.0, (double)TileSize * Upscale * width / height), Height = TileSize * Upscale };
			else
				rez = new Resolution() { Width = TileSize * Upscale, Height = TileSize * Upscale };

			XmlDocument response = c.GetImage(format, data.Boundary, rez, false, false, datasets);
			byte[] pictureData = Convert.FromBase64String(response.SelectSingleNode("/geosoft_xml/response/image/picture").InnerText);
			MemoryStream serverResponseImage = new MemoryStream(pictureData);
			MemoryStream result = new MemoryStream();
			using (Bitmap src = new Bitmap(serverResponseImage))
			{
				using (Bitmap dst = new Bitmap(rez.Width / Upscale, rez.Height / Upscale))
				{
					using (Graphics g = Graphics.FromImage(dst))
					{
						g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
						g.DrawImage(src, 0, 0, rez.Width / Upscale, rez.Height / Upscale);
					}
					dst.Save(result, System.Drawing.Imaging.ImageFormat.Png);
				}
			}
			result.Seek(0, SeekOrigin.Begin);
			return result;
		}
	}
}
