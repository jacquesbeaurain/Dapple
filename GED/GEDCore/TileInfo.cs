using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Geosoft.Dap.Common;

namespace GED.Core
{
	/// <summary>
	/// Represents the parameters for a single cache tile.
	/// </summary>
	/// <remarks>
	/// Tiles are divided into levels. At level zero, the world is divided into two square tiles
	/// covering the western and eastern hemispheres.  Each successive level divides these two tiles
	/// into four quadrants.
	/// </remarks>
	public class TileInfo
	{
		#region Constants

		/// <summary>
		/// The dimension of individual tile images.
		/// </summary>
		public const int TileSizePixels = 256;

		#endregion


		#region Member Variables

		private int m_iLevel;
		private int m_iColumn;
		private int m_iRow;

		#endregion


		#region Constructors

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="iLevel">The level of this tile.</param>
		/// <param name="iColumn">The column of this tile.</param>
		/// <param name="iRow">The row of this tile.</param>
		public TileInfo(int iLevel, int iColumn, int iRow)
		{
			if (iLevel < 0) throw new ArgumentException("Level must be >= 0", "iLevel");
			m_iLevel = iLevel;

			int iNumColumns = GetNumColumns(iLevel);
			while (iColumn >= iNumColumns) iColumn -= iNumColumns;
			while (iColumn < 0) iColumn += iNumColumns;
			m_iColumn = iColumn;

			int iNumRows = GetNumRows(iLevel);
			if (iRow < 0) throw new ArgumentException("Row must be >= 0", "iRow");
			if (iRow >= iNumRows) throw new ArgumentException("Row must be < " + iNumRows, "iRow");
			m_iRow = iRow;
		}

		#endregion


		#region Properties

		/// <summary>
		/// The level of this tile.
		/// </summary>
		public int Level
		{
			get { return m_iLevel; }
		}

		/// <summary>
		/// The column of this tile.
		/// </summary>
		public int Column
		{
			get { return m_iColumn; }
		}

		/// <summary>
		/// The row of this tile.
		/// </summary>
		public int Row
		{
			get { return m_iRow; }
		}

		/// <summary>
		/// The BoundingBox that this tile covers.
		/// </summary>
		public BoundingBox Bounds
		{
			get
			{
				double dTileSize = 180.0 / Math.Pow(2.0, m_iLevel);
				return new BoundingBox((m_iColumn + 1) * dTileSize - 180.0, (m_iRow + 1) * dTileSize - 90.0, m_iColumn * dTileSize - 180.0, m_iRow * dTileSize - 90.0);
			}
		}

		#endregion


		#region Methods

		/// <summary>
		/// Gets the size in degrees of tiles at the given tile level.
		/// </summary>
		public static double GetTileSize(int iLevel)
		{
			return 180.0 / Math.Pow(2.0, iLevel);
		}

		/// <summary>
		/// The number of columns at this tile level.
		/// </summary>
		public static int GetNumColumns(int iLevel)
		{
			return 2 * GetNumRows(iLevel);
		}

		/// <summary>
		/// The number of rows at this tile level.
		/// </summary>
		public static int GetNumRows(int iLevel)
		{
			return (int)Math.Pow(2.0, iLevel);
		}

		/// <summary>
		/// Returns a System.String that represents the current TileInfo. 
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return String.Format(CultureInfo.InvariantCulture, "l{0:d2} c{1:d4} r{2:d4}", m_iLevel, m_iColumn, m_iRow);
		}

		#endregion
	}
}
