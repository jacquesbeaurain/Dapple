using System;
using System.Collections;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Xml;
using GED.Core;
using Geosoft.Dap;
using Geosoft.Dap.Common;

namespace GED.WebService
{
	public class LayerInfo
	{
		private String m_strServerType;
		private String m_strServer;
		private String m_strLayer;

		public LayerInfo(String strServerType, String strServer, String strLayer)
		{
			m_strServerType = strServerType;
			m_strServer = strServer;
			m_strLayer = strLayer;
		}

		public String Type { get { return m_strServerType; } }
		public String Server { get { return m_strServer; } }
		public String Layer { get { return m_strLayer; } }

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
		private String CacheTileImage(TileInfo oTile)
		{
			String strCacheFilename = Path.Combine(CacheUtils.CacheRoot, GetCacheFilename(oTile));

			// --- Download the file if it doesn't already exist ---

			if (!File.Exists(strCacheFilename))
			{
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
		/// <param name="strImageServer">The server serving the layer.</param>
		/// <param name="strImageTitle">The title of the layer.</param>
		/// <param name="strCacheFilename">The filename to save the layer to.</param>
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

			while (oServiceTiles.Level > 0 && !AllTilesCached(oServiceTiles))
			{
				oServiceTiles = new TileSet(oViewBoundingBox, oServiceTiles.Level - 1);
			}


			// --- Render the image for the tile level requested ---

			MemoryStream result = new MemoryStream();
			using (Bitmap oResult = new Bitmap((int)(iImageSize * dLongitude / dMajorAxis), (int)(iImageSize * dLatitude / dMajorAxis)))
			{
				using (Graphics oResultGraphics = Graphics.FromImage(oResult))
				{
					foreach (TileInfo oTile in oServiceTiles)
					{
						double dTileSize = TileInfo.GetTileSize(oTile.Level);

						BoundingBox oTileBoundingBox = oTile.Bounds;
						if (!oTileBoundingBox.Intersects(oLayerBoundingBox)) continue;

						RectangleF oTileDrawRectangle = new RectangleF(
							(float)((oTileBoundingBox.MinX - oViewBoundingBox.MinX) / dDegreesPerPixel),
							(float)((oViewBoundingBox.MaxY - oTileBoundingBox.MaxY) / dDegreesPerPixel),
							(float)(dTileSize / dDegreesPerPixel),
							(float)(dTileSize / dDegreesPerPixel)
							);

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

		private bool AllTilesCached(TileSet oTileSet)
		{
			foreach (TileInfo oTile in oTileSet)
			{
				if (!new DownloadInfo(this, oTile).IsCached)
					return false;
			}
			return true;
		}
	}
}
