using WorldWind.Net;
using System;
using System.IO;
using System.Collections.Generic;
using WorldWind.Camera;
using Utility;

namespace WorldWind.Renderable
{
	public interface IGeoSpatialDownloadTileSet
	{

		int NumberRetries
		{
			get;
			set;
		}

		void AddToDownloadQueue(CameraBase camera, GeoSpatialDownloadRequest newRequest);
		void RemoveFromDownloadQueue(GeoSpatialDownloadRequest request, bool serviceQueue);
		void ServiceDownloadQueue();

		int ColorKey
		{
			get;
			set;
		}

		int ColorKeyMax
		{
			get;
			set;
		}

		TimeSpan CacheExpirationTime
		{
			get;
		}

		CameraBase Camera
		{
			get;
		}
	}

	public interface IGeoSpatialDownloadTile
	{
		void Initialize();

		Angle CenterLatitude
		{
			get;
		}

		Angle CenterLongitude
		{
			get;
		}

		int Level
		{
			get;
		}

		int Row
		{
			get;
		}

		int Col
		{
			get;
		}

		/// <summary>
		/// North bound for this Tile
		/// </summary>
		double North
		{
			get;
		}

		/// <summary>
		/// West bound for this Tile
		/// </summary>
		double West
		{
			get;
		}

		/// <summary>
		/// South bound for this Tile
		/// </summary>
		double South
		{
			get;
		}

		/// <summary>
		/// East bound for this Tile
		/// </summary>
		double East
		{
			get;
		}

		int TextureSizePixels
		{
			get;
			set;
		}

		IGeoSpatialDownloadTileSet TileSet
		{
			get;
		}

		List<GeoSpatialDownloadRequest> DownloadRequests
		{
			get;
		}

		string ImageFilePath
		{
			get;
			set;
		}

		bool IsDownloadingImage
		{
			get;
			set;
		}

		bool WaitingForDownload
		{
			get;
			set;
		}
	}

	public class GeoSpatialDownloadRequest : IDisposable
	{
		internal float ProgressPercent;
		internal int DownloadPos;
		internal int DownloadTotal;
		protected WebDownload download;
		protected string m_localFilePath;
		protected string m_url;
		protected IGeoSpatialDownloadTile m_tile;
		protected ImageStore m_imageStore;

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.Renderable.GeoSpatialDownloadRequest"/> class.
		/// </summary>
		/// <param name="tile"></param>
		public GeoSpatialDownloadRequest(IGeoSpatialDownloadTile tile, ImageStore imageStore, string localFilePath, string downloadUrl)
		{
			m_tile = tile;
			m_url = downloadUrl;
			m_localFilePath = localFilePath;
			m_imageStore = imageStore;
		}

		/// <summary>
		/// Whether the request is currently being downloaded
		/// </summary>
		internal bool IsDownloading
		{
			get
			{
				return (download != null);
			}
		}

		internal bool IsComplete
		{
			get
			{
				if (download == null)
					return true;
				return download.IsComplete;
			}
		}

		  public IGeoSpatialDownloadTile Tile
		{
			get
			{
				return m_tile;
			}
		}

		protected virtual void DownloadComplete(WebDownload downloadInfo)
		{
			try
			{
				downloadInfo.Verify();

				m_tile.TileSet.NumberRetries = 0;

				// Rename temp file to real name
				File.Delete(m_localFilePath);
				File.Move(downloadInfo.SavedFilePath, m_localFilePath);

				// Make the tile reload the new image
				m_tile.DownloadRequests.Remove(this);
				m_tile.Initialize();
			}
			catch (System.Net.WebException caught)
			{
				System.Net.HttpWebResponse response = caught.Response as System.Net.HttpWebResponse;
				if (response != null && response.StatusCode == System.Net.HttpStatusCode.NotFound)
				{
					using (File.Create(m_localFilePath + ".txt"))
					{ }
					return;
				}
				m_tile.TileSet.NumberRetries++;
			}
			catch
			{
				using (File.Create(m_localFilePath + ".txt"))
				{ }
				if (File.Exists(downloadInfo.SavedFilePath))
				{
					try
					{
						File.Delete(downloadInfo.SavedFilePath);
					}
					catch (Exception e)
					{
						Log.Write(Log.Levels.Error, "GSDR", "could not delete file " + downloadInfo.SavedFilePath + ":");
						Log.Write(e);
					}
				}
			}
			finally
			{
				if (download != null)
					download.IsComplete = true;

				// Immediately queue next download
				m_tile.TileSet.RemoveFromDownloadQueue(this, true);
			}
		}

		public virtual void StartDownload()
		{
			Tile.IsDownloadingImage = true;
			download = new WebDownload(m_url);
			download.DownloadType = DownloadType.Wms;
			download.SavedFilePath = m_localFilePath + ".tmp";
			download.ProgressCallback += new DownloadProgressHandler(UpdateProgress);
			download.CompleteCallback += new WorldWind.Net.DownloadCompleteHandler(DownloadComplete);
			download.BackgroundDownloadFile();
		}

		protected void UpdateProgress(int pos, int total)
		{
			if (total == 0)
				// When server doesn't provide content-length, use this dummy value to at least show some progress.
				total = 50 * 1024;
			pos = pos % (total + 1);
			DownloadPos = pos;
			DownloadTotal = total;
			ProgressPercent = (float)pos / total;
		}

		internal virtual void Cancel()
		{
			if (download != null)
				download.Cancel();
		}

		public override string ToString()
		{
			return m_imageStore.GetLocalPath(Tile);
		}

		#region IDisposable Members

		public virtual void Dispose()
		{
			if (download != null)
			{
				download.Dispose();
				download = null;
			}
			GC.SuppressFinalize(this);
		}

		#endregion
	}
}
