using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Geosoft.Dap.Common;

namespace GED.Core
{
	/// <summary>
	/// Represents a set of DownloadInfos.
	/// </summary>
	public class DownloadSet
	{
		#region Member Variables

		private List<DownloadInfo> m_oInfos;

		#endregion


		#region Events

		/// <summary>
		/// Occurs when all the DownloadInfos in this DownloadSet have finished downloading.
		/// </summary>
		protected event EventHandler DownloadComplete;

		#endregion


		#region Public Methods

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="oLayer">The layer to download.</param>
		/// <param name="oTiles">The set of tiles to download for the layer.</param>
		public DownloadSet(LayerInfo oLayer, TileSet oTiles)
			:this(oLayer, oTiles, new BoundingBox(180, 90, -180, -90))
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="oLayer">The layer to download.</param>
		/// <param name="oTiles">The set of tiles to download for the layer.</param>
		/// <param name="oLayerBounds">The bounding box of the layer.</param>
		public DownloadSet(LayerInfo oLayer, TileSet oTiles, BoundingBox oLayerBounds)
		{
			m_oInfos = new List<DownloadInfo>();

			foreach (TileInfo oTile in oTiles)
			{
				if (oLayerBounds.Intersects(oTile.Bounds))
				{
					DownloadInfo oInfo = new DownloadInfo(oLayer, oTile);
					if (!oInfo.IsCached)
					{
						m_oInfos.Add(oInfo);
					}
				}
			}
		}

		#endregion


		#region Properties

		/// <summary>
		/// Whether all the DownloadInfos in this DownloadSet are already cached.
		/// </summary>
		public bool IsCached
		{
			get
			{
				foreach (DownloadInfo oInfo in m_oInfos)
				{
					if (!oInfo.IsCached) return false;
				}

				return true;
			}
		}

		#endregion


		#region Public Methods

		/// <summary>
		/// Download all the DownloadInfos in this DownloadSet. If this DownloadSet is empty, this
		/// method is a no-op, and the DownloadComplete handler will not be called.
		/// </summary>
		public void DownloadAsync(EventHandler callback)
		{
			if (m_oInfos.Count > 0)
			{
				DownloadComplete += callback;
				Thread oDownloadThread = new Thread(new ThreadStart(DownloadSync));
				oDownloadThread.IsBackground = true;
				oDownloadThread.Start();
			}
		}

		/// <summary>
		/// Download all the DownloadInfos in this DownloadSet. If this DownloadSet is empty, this
		/// method is a no-op, and the DownloadComplete handler will not be called.
		/// </summary>
		public void DownloadSync()
		{
			if (m_oInfos.Count > 0)
			{
				foreach (DownloadInfo oInfo in m_oInfos)
				{
					oInfo.Download();
				}

				OnDownloadCompelte();
			}
		}

		#endregion


		#region Helper Methods

		/// <summary>
		/// Raises the DownloadComplete event.
		/// </summary>
		protected void OnDownloadCompelte()
		{
			if (DownloadComplete != null)
			{
				DownloadComplete(this, EventArgs.Empty);
			}
		}

		#endregion
	}
}
