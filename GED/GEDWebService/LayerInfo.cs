using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Geosoft.Dap;
using Geosoft.Dap.Common;
using System.Collections;
using System.Xml;
using System.Globalization;
using GED.Core;
using System.Drawing;
using GEDCore;

namespace GED.WebService
{
	internal class LayerInfo
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

		public String GetCacheFilename(TileInfo oTile)
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

		internal byte[] GetTileImage(TileInfo oTile)
		{
			if (m_strServerType.ToUpperInvariant().Equals("DAP"))
			{
				return GetDapTileImage(oTile);
			}
			else
			{
				return null;
			}
		}

		private byte[] GetDapTileImage(TileInfo oTile)
		{
			String strCacheFilename = Path.Combine(CacheUtils.CacheRoot, GetCacheFilename(oTile));

			// --- Download the file if it doesn't already exist ---

			if (!File.Exists(strCacheFilename))
			{
				Directory.CreateDirectory(Path.GetDirectoryName(strCacheFilename));

				try
				{
					DownloadDapImage(strCacheFilename, oTile);
				}
				catch (DapException)
				{
					// --- Do nothing.  There just won't be a file. ---
				}
			}

			// --- Read and deliver the file ---

			if (File.Exists(strCacheFilename))
			{
				return File.ReadAllBytes(strCacheFilename);
			}
			else
			{
				return null;
			}
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


		internal byte[] GetCompositeImage(BoundingBox oLayerBoundingBox, BoundingBox oViewBoundingBox)
		{
			const int iImageSize = 512;

			double dLatitude = oViewBoundingBox.MaxY - oViewBoundingBox.MinY;
			double dLongitude = oViewBoundingBox.MaxX - oViewBoundingBox.MinX;
			double dMajorAxis = Math.Max(dLongitude, dLatitude);
			double dDegreesPerPixel = dMajorAxis / iImageSize;


			// --- Calculate tile level to use ---

			int iLevel = 0;
			while (dMajorAxis < 180.0 / Math.Pow(2.0, iLevel))
			{
				iLevel++;
			}


			// --- Calculate the number of tiles to use ---

			double dTileSize = 180.0 / Math.Pow(2.0, iLevel);
			int iMinCol = (int)Math.Floor((oViewBoundingBox.MinX + 180.0) / dTileSize);
			int iMaxCol = (int)Math.Floor((oViewBoundingBox.MaxX + 180.0) / dTileSize);
			int iMinRow = (int)Math.Floor((oViewBoundingBox.MinY + 90.0) / dTileSize);
			int iMaxRow = (int)Math.Floor((oViewBoundingBox.MaxY + 90.0) / dTileSize);
			System.Diagnostics.Debug.Assert(iMaxCol >= iMinCol);
			System.Diagnostics.Debug.Assert(iMaxRow >= iMinRow);


			// --- Render the image ---

			MemoryStream result = new MemoryStream();
			using (Bitmap oResult = new Bitmap((int)(iImageSize * dLongitude / dMajorAxis), (int)(iImageSize * dLatitude / dMajorAxis)))
			{
				using (Graphics oResultGraphics = Graphics.FromImage(oResult))
				{
					for (int iRow = iMinRow; iRow <= iMaxRow; iRow++)
						for (int iCol = iMinCol; iCol <= iMaxCol; iCol++)
						{
							TileInfo oTile = new TileInfo(iLevel, iCol, iRow);
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
	}
}
