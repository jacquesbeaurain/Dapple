using System;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
using System.Text;

using System.Security.Cryptography;

using WorldWind.Renderable;

using Utility;

namespace WorldWind
{
	/// <summary>
	/// The types of maps supported by VE
	/// </summary>
	public enum VirtualEarthMapType
	{
		aerial = 0,
		road,
		hybrid
	}

	/// <summary>
	/// Formats urls for images stored in NLT-style
	/// </summary>
	internal class VEImageStore : ImageStore
	{
		private static byte[] noTileMD5Hash = { 0xc1, 0x32, 0x69, 0x48, 0x1c, 0x73, 0xde, 0x6e, 0x18, 0x58, 0x9f, 0x9f, 0xbc, 0x3b, 0xdf, 0x7e };

		private MD5 m_md5Hasher;
		string m_formatString;
		Projection m_proj;
		double m_dLayerRadius;

		public override bool IsDownloadableLayer
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:Dapple.Plugins.VirtualEarth"/> class.
		/// </summary>
		/// <param name="mapType"></param>
		internal VEImageStore(VirtualEarthMapType mapType, double dLayerRadius)
		{
			string dataset;

			if (mapType == VirtualEarthMapType.road)
			{
				m_imageFileExtension = "png";
				dataset = "r";
			}
			else
			{
				m_imageFileExtension = "jpeg";
				if (mapType == VirtualEarthMapType.aerial)
				{
					dataset = "a";
				}
				else
				{
					dataset = "h";
				}
			}

			//TODO no clue what ?g=72&n=z is (This is what VE 3D currently tack on)
			m_formatString = "http://" + dataset + "{0}.ortho.tiles.virtualearth.net/tiles/" + dataset + "{1}." + m_imageFileExtension + "?g=72&n=z";

			//We start the VE QuadTileSet at level 3 (8x8 tiles at 45x22.5 degrees) 
			LevelZeroTileSizeDegrees = 22.5;
			LevelCount = 16;

			//NOTE tiles did not line up properly with ellps=WGS84
			
			//+proj=longlat +ellps=sphere +a=6370997.0 +es=0.0
			m_dLayerRadius = dLayerRadius;

			string[] projectionParameters = new string[] { "proj=merc", "ellps=sphere", "a=" + m_dLayerRadius.ToString(), "es=0.0", "no.defs" };
			//projectionParameters = new string[] { "proj=merc", "ellps=WGS84", "no.defs" };
			m_proj = new Projection(projectionParameters);

			// To check for no more tiles images
			m_md5Hasher = MD5.Create();
		}

		/// <summary>
		/// convert VE row, col, level into key for URL
		/// </summary>
		/// <param name="tx"></param>
		/// <param name="ty"></param>
		/// <param name="zl"></param>
		/// <returns></returns>
		private static string TileToQuadKey(int tx, int ty, int zl)
		{
			string quad;
			quad = "";
			for (int i = zl; i > 0; i--)
			{
				int mask = 1 << (i - 1);
				int cell = 0;
				if ((tx & mask) != 0)
				{
					cell++;
				}
				if ((ty & mask) != 0)
				{
					cell += 2;
				}
				quad += cell;
			}
			return quad;
		}


		protected override string GetDownloadUrl(IGeoSpatialDownloadTile tile)
		{
			//We start the VE QuadTileSet at level 3 (8x8 tiles at 45x22.5 degrees) 
			string quadKey = TileToQuadKey(tile.Col, tile.Row, tile.Level + 3);

			return String.Format(CultureInfo.InvariantCulture, m_formatString, quadKey[quadKey.Length - 1], quadKey);
		}

		/// <summary>
		/// If the mesh should be reprojected an image store should return a projection to use here
		/// </summary>
		internal override Projection Projection
		{
			get
			{
				return m_proj;
			}
		}

		public override int TextureSizePixels
		{
			get
			{
				return 256;
			}
			set
			{
			}
		}

		double MetersPerPixel(double layerRadius, int zoom)
		{
			double arc;
			arc = layerRadius * 2.0 * Math.PI / ((1 << zoom) * TextureSizePixels);
			return arc;
		}


		/// <summary>
		/// This can be used by mesh calculation to calculate projected meshes for tiles
		/// </summary>
		internal override void GetProjectionCorners(IGeoSpatialDownloadTile tile, out UV ul, out UV ur, out UV ll, out UV lr)
		{
			//handle the diff projection
			//We start the VE QuadTileSet at level 3 (8x8 tiles at 45x22.5 degrees) 
			double metersPerPixel = MetersPerPixel(m_dLayerRadius, tile.Level + 3);
			double totalTilesPerEdge = Math.Pow(2, tile.Level + 3);
			double totalMeters = totalTilesPerEdge * TextureSizePixels * metersPerPixel;
			double halfMeters = totalMeters / 2;

			//do meters calculation in VE space
			//the 0,0 origin for VE is in upper left
			double N = tile.Row * (TextureSizePixels * metersPerPixel);
			double W = tile.Col * (TextureSizePixels * metersPerPixel);
			//now convert it to +/- meter coordinates for Proj.4
			//the 0,0 origin for Proj.4 is 0 lat, 0 lon
			//-22 to 22 million, -11 to 11 million
			N = halfMeters - N;
			W = W - halfMeters;
			double E = W + (TextureSizePixels * metersPerPixel);
			double S = N - (TextureSizePixels * metersPerPixel);

			ul = new UV(W, N);
			ur = new UV(E, N);
			ll = new UV(W, S);
			lr = new UV(E, S);
		}


		internal bool IsValidTile(string strFile)
		{
			bool bBadTile = false;

			// VE's no data tiles are exactly 1033 bytes, but we compute the MD5 Hash too just to be sure 
			// If the tile in the cache is more than a week old we will get rid of it, perhaps they get new data,
			// otherwise no texture will be created and the textures above will show instead 

			FileInfo fi = new FileInfo(strFile);
			if (fi.Length == 1033)
			{
				using (FileStream fs = new FileStream(strFile, FileMode.Open))
				{
					byte[] md5Hash = m_md5Hasher.ComputeHash(fs);
					if (noTileMD5Hash.Length == md5Hash.Length)
					{
						bBadTile = true;
						for (int i = 0; i < noTileMD5Hash.Length; i++)
						{
							if (noTileMD5Hash[i] != md5Hash[i])
							{
								bBadTile = false;
								break;
							}
						}
					}
				}
			}

			return !bBadTile;
		}
	}
}