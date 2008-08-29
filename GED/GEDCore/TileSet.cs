using System;
using System.Collections;
using System.Collections.Generic;
using Geosoft.Dap.Common;
using System.IO;

namespace GED.Core
{
	/// <summary>
	/// Represents a continuous collection of tiles at a given level that are visible from a certain view.
	/// Allows for easy-to-read code by providing an enumerator of TileInfos instead of having to do a
	/// double for-loop on rows and columns.
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

			if (m_iMinRow < 0) m_iMinRow = 0;
			if (m_iMaxRow >= TileInfo.GetNumRows(m_iLevel)) m_iMaxRow = TileInfo.GetNumRows(m_iLevel) - 1;

			#region // Invariant Checking
			System.Diagnostics.Debug.Assert(m_iMaxCol >= m_iMinCol);
			System.Diagnostics.Debug.Assert(m_iMaxRow >= m_iMinRow);
			System.Diagnostics.Debug.Assert(m_iMinRow >= 0);
			System.Diagnostics.Debug.Assert(m_iMaxRow < TileInfo.GetNumRows(m_iLevel));
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


		#region Public Methods

		/// <summary>
		/// Determine whether all the tiles in this set are cached for the given LayerInfo.
		/// </summary>
		/// <param name="oInfo">The layer to check for cached tiles.</param>
		/// <returns>true if all the tiles in this TileSet are cached.</returns>
		public bool IsCached(LayerInfo oInfo)
		{
			foreach (TileInfo oTile in this)
			{
				if (!File.Exists(Path.Combine(CacheUtils.CacheRoot, oInfo.GetCacheFilename(oTile)))) return false;
			}
			return true;
		}

		/// <summary>
		/// Determine whether all the tiles in this set that intersect a given bounding box are cached for the given LayerInfo.
		/// </summary>
		/// <param name="oInfo">The layer to check for cached tiles.</param>
		/// <param name="oLayerBounds">The bounding box to cull tiles with</param>
		/// <returns>true if all the tiles in this TileSet that intersect the given bounds are cached.</returns>
		public bool IsCached(LayerInfo oInfo, BoundingBox oLayerBounds)
		{
			foreach (TileInfo oTile in this)
			{
				if (oLayerBounds.Intersects(oTile.Bounds) && !File.Exists(Path.Combine(CacheUtils.CacheRoot, oInfo.GetCacheFilename(oTile)))) return false;
			}
			return true;
		}

		#endregion
	}
}
