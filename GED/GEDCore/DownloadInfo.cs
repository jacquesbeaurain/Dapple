using System.IO;
using GED.Core;

namespace GED.WebService
{
	class DownloadInfo
	{
		private LayerInfo m_oLayer;
		private TileInfo m_oTile;

		public DownloadInfo(LayerInfo oLayer, TileInfo oTile)
		{
			m_oLayer = oLayer;
			m_oTile = oTile;
		}

		public LayerInfo Layer
		{
			get { return m_oLayer; }
		}

		public TileInfo Tile
		{
			get { return m_oTile; }
		}

		public bool IsCached
		{
			get { return File.Exists(m_oLayer.GetCacheFilename(m_oTile)); }
		}

		public void Download()
		{
		}

		public override string ToString()
		{
			return m_oLayer.ToString() + " " + m_oTile.ToString();
		}
	}
}
