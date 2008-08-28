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

			while (iColumn >= NumColumns) iColumn -= NumColumns;
			while (iColumn < 0) iColumn += NumColumns;
			m_iColumn = iColumn;

			if (iRow < 0) throw new ArgumentException("Row must be >= 0", "iRow");
			if (iRow >= NumRows) throw new ArgumentException("Row must be < " + NumRows, "iRow");
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

		/// <summary>
		/// The number of rows at this tile level.
		/// </summary>
		public int NumRows
		{
			get { return (int)Math.Pow(2.0, m_iLevel); }
		}

		/// <summary>
		/// The number of columns at this tile level.
		/// </summary>
		public int NumColumns
		{
			get { return (int)(2.0 * (int)Math.Pow(2.0, m_iLevel)); }
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
		/// Returns a System.String that represents the current TileInfo. 
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return String.Format(CultureInfo.InvariantCulture, "l{0:d2} c{1:d4} r{2:d4}", m_iLevel, m_iColumn, m_iRow);
		}

		#endregion
	}


	/// <summary>
	/// Represents a continuous collection of tiles at a given level that are visible from a certain view.
	/// </summary>
	public class TileSet : IEnumerable<TileInfo>
	{
		#region Member Variables

		private int m_iLevel;
		private int m_iMinCol, m_iMaxCol;
		private int m_iMinRow, m_iMaxRow;

		#endregion


		#region Constructors

		/// <summary>
		/// Creates a new TileSet for a given view extent at the optimal tile level.
		/// </summary>
		/// <param name="oView">The view to construct a TileSet for.</param>
		public TileSet(BoundingBox oView)
		{
			#region // Input checking
			if (oView == null) throw new ArgumentNullException("oView");
			#endregion

			// --- Calculate tile level to use ---
			double dLatitude = oView.MaxY - oView.MinY;
			double dLongitude = oView.MaxX - oView.MinX;
			m_iLevel = 0;
			while (Math.Min(dLongitude, dLatitude) < TileInfo.GetTileSize(m_iLevel))
			{
				m_iLevel++;
			}

			CalculateMinMax(oView);
		}

		/// <summary>
		/// Creates a new TileSet for a give view extent at a given tile level.
		/// </summary>
		/// <param name="oView">The view to construct a TileSet for.</param>
		/// <param name="iLevel">The level to create the tiles at.</param>
		public TileSet(BoundingBox oView, int iLevel)
		{
			#region // Input checking
			if (oView == null) throw new ArgumentNullException("oView");
			if (iLevel < 0) throw new ArgumentException("iLevel must be non-negative", "iLevel");
			#endregion

			m_iLevel = iLevel;
			CalculateMinMax(oView);
		}

		/// <summary>
		/// Calculates the minimum and maximum row/column for this tileset based on the given view extent.
		/// </summary>
		/// <param name="oView">The view to calculate this TileSet for.</param>
		private void CalculateMinMax(BoundingBox oView)
		{
			double dTileSize = TileInfo.GetTileSize(m_iLevel);
			m_iMinCol = (int)Math.Floor((oView.MinX + 180.0) / dTileSize);
			m_iMaxCol = (int)Math.Floor((oView.MaxX + 180.0) / dTileSize);
			m_iMinRow = (int)Math.Floor((oView.MinY + 90.0) / dTileSize);
			m_iMaxRow = (int)Math.Floor((oView.MaxY + 90.0) / dTileSize);

			#region // Invariant Checking
			System.Diagnostics.Debug.Assert(m_iMaxCol >= m_iMinCol);
			System.Diagnostics.Debug.Assert(m_iMaxRow >= m_iMinRow);
			#endregion
		}

		#endregion


		#region Properties

		/// <summary>
		/// The level of the TileInfos in this TileSet.
		/// </summary>
		public int Level
		{
			get { return m_iLevel; }
		}

		#endregion


		#region Enumeration

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>A System.Collections.Generic.IEnumerator&lt;TileInfo&gt; that can be used
		/// to iterate through the collection.</returns>
		public IEnumerator<TileInfo> GetEnumerator()
		{
			return new TileSetEnumerator(this);
		}

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>A System.Collections.Generic.IEnumerator&lt;TileInfo&gt; that can be used
		/// to iterate through the collection.</returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		/// <summary>
		/// Enumerates the TileInfo objects in a TileSet.
		/// </summary>
		public class TileSetEnumerator : IEnumerator<TileInfo>
		{
			#region Member Variables

			private TileSet m_oTarget;
			private int m_iCurrentCol, m_iCurrentRow;

			#endregion


			#region Constructors and Destructors

			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="oTarget">The TileSet to enumerate over.</param>
			public TileSetEnumerator(TileSet oTarget)
			{
				m_oTarget = oTarget;
				Reset();
			}

			/// <summary>
			/// Performs application-defined tasks associated with freeing, releasing,
			/// or resetting unmanaged resources.
			/// </summary>
			public void Dispose()
			{
				// --- Do nothing ---
			}

			#endregion


			#region Properties

			/// <summary>
			/// Gets the element in the collection at the current position of the enumerator.
			/// </summary>
			public TileInfo Current
			{
				get { return new TileInfo(m_oTarget.m_iLevel, m_iCurrentCol, m_iCurrentRow); }
			}

			/// <summary>
			/// Gets the element in the collection at the current position of the enumerator.
			/// </summary>
			object IEnumerator.Current
			{
				get { return this.Current; }
			}

			#endregion


			#region Methods

			/// <summary>
			/// Advances the enumerator to the next element of the collection.
			/// </summary>
			/// <returns>
			/// true if the enumerator was successfully advanced to the next element;
			/// false if the enumerator has passed the end of the collection.
			/// </returns>
			public bool MoveNext()
			{
				m_iCurrentCol++;
				if (m_iCurrentCol > m_oTarget.m_iMaxCol)
				{
					m_iCurrentCol = m_oTarget.m_iMinCol;
					m_iCurrentRow++;
				}
				return m_iCurrentRow <= m_oTarget.m_iMaxRow;
			}

			/// <summary>
			/// Sets the enumerator to its initial position, which is before the first element
			/// in the collection.
			/// </summary>
			public void Reset()
			{
				m_iCurrentCol = m_oTarget.m_iMinCol - 1;
				m_iCurrentRow = m_oTarget.m_iMinRow;
			}

			#endregion
		}

		#endregion
	}
}
