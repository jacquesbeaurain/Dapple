using System.IO;

namespace GED.Core
{
	/// <summary>
	/// Represents a combination of a LayerInfo and a TileInfo, providing all the information needed
	/// to download a tile image.
	/// </summary>
	public class DownloadInfo
	{
		#region Member Variables

		private LayerInfo m_oLayer;
		private TileInfo m_oTile;

		#endregion


		#region Constructors

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="oLayer">The layer of this download.</param>
		/// <param name="oTile">The tile to download.</param>
		public DownloadInfo(LayerInfo oLayer, TileInfo oTile)
		{
			m_oLayer = oLayer;
			m_oTile = oTile;
		}

		#endregion


		#region Properties

		/// <summary>
		/// The layer to download a tile for.
		/// </summary>
		public LayerInfo Layer
		{
			get { return m_oLayer; }
		}

		/// <summary>
		/// The tile to download.
		/// </summary>
		public TileInfo Tile
		{
			get { return m_oTile; }
		}

		/// <summary>
		/// Whether the tile image of this DownloadInfo is already downloaded.
		/// </summary>
		public bool IsCached
		{
			get { return File.Exists(Path.Combine(CacheUtils.CacheRoot, m_oLayer.GetCacheFilename(m_oTile))); }
		}

		#endregion


		#region Public Methods

		/// <summary>
		/// Download this tile to disk. Blocks until the download is completed.
		/// </summary>
		public void Download()
		{
			m_oLayer.CacheTileImage(m_oTile);
		}

		/// <summary>
		/// Returns a System.String that represents the current System.Object.
		/// </summary>
		/// <returns>A System.String that represents the current System.Object.</returns>
		public override string ToString()
		{
			return m_oLayer.ToString() + " " + m_oTile.ToString();
		}

		#endregion
	}
}
