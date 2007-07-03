using System;
using System.Globalization;
using System.Collections.Generic;
using System.Text;

using WorldWind;
using WorldWind.Renderable;

namespace Dapple.Plugins.VirtualEarth
{
	/// <summary>
	/// Formats urls for images stored in NLT-style
	/// </summary>
	public class VEImageStore : ImageStore
	{
		string m_formatString;

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
		public VEImageStore(VirtualEarthMapType mapType)
		{
			string fileExtension;
			string dataset;

			if (mapType == VirtualEarthMapType.road)
			{
				fileExtension = "png";
				dataset = "r";
			}
			else
			{
				fileExtension = "jpeg";
				if (mapType == VirtualEarthMapType.aerial)
				{
					dataset = "a";
				}
				else
				{
					dataset = "h";
				}
			}

			//TODO no clue what ?g= is
			m_formatString = "http://" + dataset + "{0}.ortho.tiles.virtualearth.net/tiles/" + dataset + "{1}." + fileExtension + "?g=15";

			//The VE QuadTileSet is always at level 0 (3 (180/2^3 level zero) in equations from previous plugin)

			LevelZeroTileSizeDegrees = 22.5;
			LevelCount = 16;
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
			VEQuadTile qt = (VEQuadTile)tile;

			string quadKey = TileToQuadKey(qt.Col, qt.Row, qt.Level);

			return String.Format(CultureInfo.InvariantCulture, m_formatString, quadKey[quadKey.Length - 1], quadKey);
		}
	}
}