using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Geosoft.Dap.Common;

namespace GEDCore
{
	/// <summary>
	/// Class representing a single cached tile.
	/// </summary>
	public class TileInfo
	{
		#region Member Variables

		private int m_iLevel;
		private int m_iColumn;
		private int m_iRow;

		#endregion


		#region Constructors

		public TileInfo(int iLevel, int iColumn, int iRow)
		{
			if (iLevel < 0) throw new ArgumentException("Level must be >= 0", "iLevel");
			m_iLevel = iLevel;

			while (iColumn > NumColumns) iColumn -= NumColumns;
			while (iColumn < 0) iColumn += NumColumns;
			m_iColumn = iColumn;

			if (iRow < 0) throw new ArgumentException("Row must be >= 0", "iRow");
			if (iRow >= NumRows) throw new ArgumentException("Row must be < " + NumRows, "iRow");
			m_iRow = iRow;
		}

		#endregion


		#region Properties

		public int Level
		{
			get { return m_iLevel; }
		}

		public int Column
		{
			get { return m_iColumn; }
		}

		public int Row
		{
			get { return m_iRow; }
		}

		public BoundingBox Bounds
		{
			get
			{
				double dTileSize = 180.0 / Math.Pow(2.0, m_iLevel);
				return new BoundingBox((m_iColumn + 1) * dTileSize - 180.0, (m_iRow + 1) * dTileSize - 90.0, m_iColumn * dTileSize - 180.0, m_iRow * dTileSize - 90.0);
			}
		}

		public double TileSize
		{
			get { return 180.0 / Math.Pow(2.0, m_iLevel); }
		}

		public int NumRows
		{
			get { return (int)Math.Pow(2.0, m_iLevel); }
		}

		public int NumColumns
		{
			get { return (int)(2.0 * (int)Math.Pow(2.0, m_iLevel)); }
		}

		#endregion
	}
}
