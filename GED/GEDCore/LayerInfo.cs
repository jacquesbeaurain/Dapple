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
			oRes.Height = 256;
			oRes.Width = 256;

			ArrayList oArr = new ArrayList();
			oArr.Add(m_strLayer);

			Command oCommand = new Command(m_strServer, false, Command.Version.GEOSOFT_XML_1_1);
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
			const int iImageSize = 768;
			double dLongitude = oViewBoundingBox.MaxX - oViewBoundingBox.MinX;
			double dLatitude = oViewBoundingBox.MaxY - oViewBoundingBox.MinY;
			double dMajorAxis = Math.Max(dLongitude, dLatitude);
			double dDegreesPerPixel = dMajorAxis / iImageSize;
			TileSet oRequiredTiles = new TileSet(oViewBoundingBox);
			TileSet oServiceTiles = oRequiredTiles;


			// --- Keep going to lower-levelled tiles until we can service the request ---

			System.Diagnostics.Debug.WriteLine("Requesting composite image at level " + oServiceTiles.Level);
			while (oServiceTiles.Level > 0 && !oServiceTiles.IsCached(this, oLayerBoundingBox))
			{
				oServiceTiles = new TileSet(oViewBoundingBox, oServiceTiles.Level - 1);
				System.Diagnostics.Debug.WriteLine("Decrementing level to " + oServiceTiles.Level);
			}

			
			// --- Queue up the other images for the request ---

			if (!oRequiredTiles.IsCached(this, oLayerBoundingBox))
			{
				DownloadSet oRequiredDownloads = new DownloadSet(this, oRequiredTiles, oLayerBoundingBox);
				oRequiredDownloads.DownloadAsync(new EventHandler(oRequiredDownloads_DownloadComplete));
			}


			// --- Render the image for the tile level requested ---

			MemoryStream result = new MemoryStream();
			int iResultWidth = (int)(iImageSize * dLongitude / dMajorAxis);
			int iResultHeight = (int)(iImageSize * dLatitude / dMajorAxis);
			using (Bitmap oResult = new Bitmap(iResultWidth, iResultHeight))
			{
				using (Graphics oResultGraphics = Graphics.FromImage(oResult))
				{
					oResultGraphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

					float fTileSizePixels = (float)(TileInfo.GetTileSize(oServiceTiles.Level) / dDegreesPerPixel);

					foreach (TileInfo oTile in oServiceTiles)
					{
						BoundingBox oTileBoundingBox = oTile.Bounds;
						if (!oTileBoundingBox.Intersects(oLayerBoundingBox)) continue;

						RectangleF oTileDrawRectangle = new RectangleF(
							(float)((oTileBoundingBox.MinX - oViewBoundingBox.MinX) / dDegreesPerPixel),
							(float)((oViewBoundingBox.MaxY - oTileBoundingBox.MaxY) / dDegreesPerPixel),
							fTileSizePixels,
							fTileSizePixels
							);

						System.Diagnostics.Debug.WriteLine("Drawing " + oTile.ToString() + " at " + oTileDrawRectangle.ToString());

						byte[] oTileData = GetTileImage(oTile);
						if (oTileData != null)
						{
							using (Bitmap oTileImage = new Bitmap(new MemoryStream(oTileData)))
							{
								oResultGraphics.DrawImage(oTileImage, oTileDrawRectangle);
							}
						}
					}	
				}
				oResult.Save(result, System.Drawing.Imaging.ImageFormat.Png);
			}
			return result.ToArray();
		}

		void oRequiredDownloads_DownloadComplete(object sender, EventArgs e)
		{
			System.Diagnostics.Debug.WriteLine("Download complete callback!");
			GoogleEarth.TriggerRefresh();
		}
	}
}
