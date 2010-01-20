using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Xml;
using Geosoft.Dap;
using Geosoft.Dap.Common;
using System.Net;

namespace GED.Core
{
	/// <summary>
	/// Represents a layer on a server.
	/// </summary>
	public class LayerInfo
	{
		#region Member Variables

		private String m_strServerType;
		private String m_strServer;
		private String m_strLayer;

		#endregion


		#region Constructors

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="strServerType">The type of server (DAP, WMS, ARCIMS...)</param>
		/// <param name="strServer">The url of the server.</param>
		/// <param name="strLayer">The unique identifier for the layer.</param>
		public LayerInfo(String strServerType, String strServer, String strLayer)
		{
			m_strServerType = strServerType;
			m_strServer = strServer;
			m_strLayer = strLayer;
		}

		#endregion


		#region Properties

		/// <summary>
		/// The type of server (DAP, WMS, ARCIMS...)
		/// </summary>
		public String Type
		{
			get { return m_strServerType; }
		}

		/// <summary>
		/// The URL of the server.
		/// </summary>
		public String Server
		{
			get { return m_strServer; }
		}

		/// <summary>
		/// The unique identifier for the layer.
		/// </summary>
		public String Layer
		{
			get { return m_strLayer; }
		}

		#endregion

		/// <summary>
		/// Get the filename that this LayerInfo will save a given TileInfo as.
		/// </summary>
		/// <param name="oTile">The TileInfo for the tile to cache.</param>
		/// <returns>A filename relative to the cache root that the tile will be saved as.</returns>
		internal String GetCacheFilename(TileInfo oTile)
		{
			if (m_strServerType.ToUpperInvariant().Equals("DAP"))
			{
				UriBuilder oBuilder = new UriBuilder(m_strServer);
				return String.Format(CultureInfo.InvariantCulture, "DAP Images\\{0}\\{1}\\{2}\\{3:d4}-{4:d4}.png",
					CacheUtils.CreateValidFilename(oBuilder.Host),
					m_strLayer,
					oTile.Level,
					oTile.Column,
					oTile.Row
				);
			}

			throw new NotImplementedException();
		}

		/// <summary>
		/// Get the raw data for the given TileInfo.
		/// </summary>
		/// <param name="oTile">The TileInfo of the tile of this LayerInfo to get.</param>
		/// <returns>The raw data of the tile image, or null if it could not be downloaded.</returns>
		public byte[] GetTileImage(TileInfo oTile)
		{
			String strFilename = CacheTileImage(oTile);

			if (!String.IsNullOrEmpty(strFilename) && File.Exists(strFilename))
				return File.ReadAllBytes(strFilename);
			else
				return null;
		}

		/// <summary>
		/// Download the image for the given TileInfo.
		/// </summary>
		/// <param name="oTile">The TileInfo of the tile of this LayerInfo to download.</param>
		/// <returns>The filename for the image, or null if it could not be downloaded.</returns>
		public String CacheTileImage(TileInfo oTile)
		{
			String strCacheFilename = Path.Combine(CacheUtils.CacheRoot, GetCacheFilename(oTile));

			// --- Download the file if it doesn't already exist ---

			if (!File.Exists(strCacheFilename))
			{
				System.Diagnostics.Debug.WriteLine("LayerInfo: cacheing tile " + oTile.ToString());

				Directory.CreateDirectory(Path.GetDirectoryName(strCacheFilename));

				if (m_strServerType.ToUpperInvariant().Equals("DAP"))
				{
					try
					{
						DownloadDapImage(strCacheFilename, oTile);
					}
					catch (DapException)
					{
						return null;
					}
					catch (WebException)
					{
						return null;
					}
				}
				else
				{
					throw new NotImplementedException();
				}
			}

			return strCacheFilename;
		}

		/// <summary>
		/// Download an image for a layer.
		/// </summary>
		/// <param name="strCacheFilename">The filename to save the layer to.</param>
		/// <param name="oTile">The tile of this layer to download.</param>
		private void DownloadDapImage(string strCacheFilename, TileInfo oTile)
		{
			Format oFormat = new Format();
			oFormat.Type = "image/png";
			oFormat.Transparent = true;

			Resolution oRes = new Resolution();
			oRes.Height = TileInfo.TileSizePixels;
			oRes.Width = TileInfo.TileSizePixels;

			ArrayList oArr = new ArrayList();
			oArr.Add(m_strLayer);

			Command oCommand = new Command(m_strServer, false, Command.Version.GEOSOFT_XML_1_1, false, DapSecureToken.Instance);
			XmlDocument result = oCommand.GetImage(oFormat, oTile.Bounds, oRes, oArr);

			// --- Save the resulting image ---

			XmlNodeList hNodeList = result.SelectNodes("/" + Geosoft.Dap.Xml.Common.Constant.Tag.GEO_XML_TAG + "/" + Geosoft.Dap.Xml.Common.Constant.Tag.RESPONSE_TAG + "/" + Geosoft.Dap.Xml.Common.Constant.Tag.IMAGE_TAG + "/" + Geosoft.Dap.Xml.Common.Constant.Tag.PICTURE_TAG);
			if (hNodeList.Count == 0)
			{
				hNodeList = result.SelectNodes("/" + Geosoft.Dap.Xml.Common.Constant.Tag.GEO_XML_TAG + "/" + Geosoft.Dap.Xml.Common.Constant.Tag.RESPONSE_TAG + "/" + Geosoft.Dap.Xml.Common.Constant.Tag.TILE_TAG);
			}
			FileStream fs = new FileStream(strCacheFilename, System.IO.FileMode.Create);
			BinaryWriter bs = new BinaryWriter(fs);

			foreach (XmlNode hNode in hNodeList)
			{
				XmlNode hN1 = hNode.FirstChild;
				string jpegImage = hN1.Value;

				if (jpegImage != null)
				{
					byte[] jpegRawImage = Convert.FromBase64String(jpegImage);

					bs.Write(jpegRawImage);
				}
			}
			bs.Flush();
			bs.Close();
			fs.Close();
		}

		/// <summary>
		/// Create an image for this layer by stitching together tile images.
		/// </summary>
		/// <param name="oLayerBoundingBox">
		/// The bounding box for the layer. Tiles outside this bounding box will not be drawn.
		/// </param>
		/// <param name="oViewBoundingBox">
		/// The bounding box of the view. The resulting image will be scaled and
		/// cropped to fill this view.
		/// </param>
		/// <returns>The raw data of the composed image.</returns>
		public byte[] GetCompositeImage(BoundingBox oLayerBoundingBox, BoundingBox oViewBoundingBox)
		{
			if (!oLayerBoundingBox.Intersects(oViewBoundingBox))
				return BlankImageBytes();

			BoundingBox visibleDataBounds = oLayerBoundingBox.IntersectWith(oViewBoundingBox);

			//bool moreDataComing = false;
			TileSet oRequiredTiles = new TileSet(visibleDataBounds, TileSet.GetLevel(oViewBoundingBox));
			TileSet oServiceTiles = oRequiredTiles;


			// --- Keep going to lower-levelled tiles until we can service the request ---

			System.Diagnostics.Debug.WriteLine("Requesting composite image at level " + oServiceTiles.Level);
			while (oServiceTiles.Level > 0 && !oServiceTiles.IsCached(this, oLayerBoundingBox))
			{
				oRequiredTiles = oServiceTiles;
				oServiceTiles = new TileSet(visibleDataBounds, oServiceTiles.Level - 1);
				//moreDataComing = true;
				System.Diagnostics.Debug.WriteLine("Decrementing level to " + oServiceTiles.Level);
			}


			// --- Queue up the other images for the request ---

			if (!oRequiredTiles.IsCached(this, oLayerBoundingBox))
			{
				DownloadSet oRequiredDownloads = new DownloadSet(this, oRequiredTiles, oLayerBoundingBox);
				oRequiredDownloads.DownloadAsync(new EventHandler(oRequiredDownloads_DownloadComplete));
			}

			// --- Compose image tiles to master tile ---

			Bitmap composite = ComposeImageFromTileCache(oServiceTiles);
			BoundingBox compositeBounds = oServiceTiles.Bounds;
			double degreesPerPixel = TileInfo.GetTileSize(oServiceTiles.Level) / TileInfo.TileSizePixels;

			// --- Render the image for the tile level requested ---

			Bitmap result = new Bitmap((int)((oViewBoundingBox.MaxX - oViewBoundingBox.MinX) / degreesPerPixel), (int)((oViewBoundingBox.MaxY - oViewBoundingBox.MinY) / degreesPerPixel));
			BoundingBox resultBounds = new BoundingBox(oViewBoundingBox);

			using (Graphics g = Graphics.FromImage(result))
			{
				g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

				g.DrawImage(composite, new PointF((float)((compositeBounds.MinX - resultBounds.MinX) / degreesPerPixel), (float)((resultBounds.MaxY - compositeBounds.MaxY) / degreesPerPixel)));

				//if (moreDataComing)
				//   g.FillRectangle(new System.Drawing.Drawing2D.HatchBrush(System.Drawing.Drawing2D.HatchStyle.Percent05, Color.Green, Color.Transparent), 0, 0, result.Width, result.Height);
			}

			result.Save("C:/documents and settings/chrismac/desktop/sent.png", System.Drawing.Imaging.ImageFormat.Png);

			MemoryStream buffer = new MemoryStream();
			result.Save(buffer, System.Drawing.Imaging.ImageFormat.Png);
			return buffer.ToArray();
		}

		private Bitmap ComposeImageFromTileCache(TileSet oServiceTiles)
		{
			Bitmap result = new Bitmap(oServiceTiles.CompositeWidth, oServiceTiles.CompositeHeight);
			using (Graphics g = Graphics.FromImage(result))
				foreach (TileInfo tile in oServiceTiles)
				{
					Console.WriteLine(tile);
					byte[] tileBytes = GetTileImage(tile);
					if (tileBytes != null)
						using (Bitmap tileImage = new Bitmap(new MemoryStream(tileBytes)))
							g.DrawImage(tileImage, oServiceTiles.GetCompositePoint(tile));
				}
			result.Save("C:/documents and settings/chrismac/desktop/out.png", System.Drawing.Imaging.ImageFormat.Png);
			return result;
		}

		private byte[] BlankImageBytes()
		{
			MemoryStream buffer = new MemoryStream();
			using (Bitmap b = new Bitmap(1, 1))
				b.Save(buffer, System.Drawing.Imaging.ImageFormat.Png);
			return buffer.ToArray();
		}

		void oRequiredDownloads_DownloadComplete(object sender, EventArgs e)
		{
			System.Diagnostics.Debug.WriteLine("Download complete callback!");
			GoogleEarth.TriggerRefresh();
		}
	}

	static class BoundingBoxExtension
	{
		/// <summary>
		/// Find the intersection of two bounding boxes.
		/// </summary>
		/// <param name="bbox1"></param>
		/// <param name="bbox2"></param>
		/// <returns>A bounding box covering the region of intersection.</returns>
		public static BoundingBox IntersectWith(this BoundingBox bbox1, BoundingBox bbox2)
		{
			if (!bbox1.Intersects(bbox2))
				throw new InvalidOperationException();

			return new BoundingBox(
				Math.Min(bbox1.MaxX, bbox2.MaxX),
				Math.Min(bbox1.MaxY, bbox2.MaxY),
				Math.Max(bbox1.MinX, bbox2.MinX),
				Math.Max(bbox1.MinY, bbox2.MinY));
		}
	}
}
