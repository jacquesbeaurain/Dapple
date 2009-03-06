using Microsoft.DirectX.Direct3D;
using WorldWind.Net.Wms;
using WorldWind.Renderable;
using System;
using System.IO;
using Utility;

namespace WorldWind
{
	/// <summary>
	/// Base class for calculating local image paths and remote download urls
	/// </summary>
	public class ImageStore
	{
		#region Private Members

		protected string m_dataDirectory;
		protected double m_levelZeroTileSizeDegrees = 22.5;
		protected int m_levelCount = 1;
		protected string m_imageFileExtension;
		protected string m_cacheDirectory;
		protected string m_duplicateTexturePath;
		protected string m_serverlogo;

		protected bool m_colorKeyEnabled;
		protected bool m_alphaKeyEnabled;

		protected Format m_textureFormat;
		protected int m_colorKey;
		protected int m_alphaKeyMin = -1;
		protected int m_alphaKeyMax = -1;

		#endregion

		#region Properties

		public Format TextureFormat
		{
			get
			{
				return m_textureFormat;
			}
			set
			{
				m_textureFormat = value;
			}
		}

		/// <summary>
		/// Coverage of outer level 0 bitmaps (decimal degrees)
		/// Level 1 has half the coverage, level 2 half of level 1 (1/4) etc.
		/// </summary>
		public double LevelZeroTileSizeDegrees
		{
			get
			{
				return m_levelZeroTileSizeDegrees;
			}
			set
			{
				m_levelZeroTileSizeDegrees = value;
			}
		}

		/// <summary>
		/// The size of a texture tile in the store (for Nlt servers this is later determined in QTS texture loading code once first tile is read)
		/// Used by QTS export in Dapple
		/// </summary>
		public virtual int TextureSizePixels
		{
			get
			{
				return -1;
			}
			set
			{
			}
		}

		/// <summary>
		/// If the mesh should be reprojected an image store should return a projection to use here
		/// </summary>
		internal virtual Projection Projection
		{
			get
			{
				return null;
			}
		}

		/// <summary>
		/// This can be used by mesh calculation to calculate projected meshes for tiles
		/// </summary>
		internal virtual void GetProjectionCorners(IGeoSpatialDownloadTile tile, out UV ul, out UV ur, out UV ll, out UV lr)
		{
			// For normal quad tiles this is equal to the geographic coordinates
			ul = new UV(tile.West, tile.North);
			ur = new UV(tile.East, tile.North);
			ll = new UV(tile.West, tile.South);
			lr = new UV(tile.East, tile.South);
		}
		
		/// <summary>
		/// Number of detail levels
		/// </summary>
		public int LevelCount
		{
			get
			{
				return m_levelCount;
			}
			set
			{
				m_levelCount = value;
			}
		}

		/// <summary>
		/// File extension of the source image file format
		/// </summary>
		public string ImageExtension
		{
			get
			{
				return m_imageFileExtension;
			}
			set
			{
				// Strip any leading dot
				m_imageFileExtension = value.Replace(".", "");
			}
		}

		/// <summary>
		/// Cache subdirectory for this layer
		/// </summary>
		public string CacheDirectory
		{
			get
			{
				return m_cacheDirectory;
			}
			set
			{
				m_cacheDirectory = value;
			}
		}

		/// <summary>
		/// Data directory for this layer (permanently stored images)
		/// </summary>
		public string DataDirectory
		{
			get
			{
				return m_dataDirectory;
			}
			set
			{
				m_dataDirectory = value;
			}
		}

		/// <summary>
		/// Default texture to be used (always ocean?)
		/// Can be either file or url
		/// </summary>
		internal string DuplicateTexturePath
		{
			get
			{
				return m_duplicateTexturePath;
			}
		}

		public virtual bool IsDownloadableLayer
		{
			get
			{
				return false;
			}
		}

		#endregion

		internal virtual string GetLocalPath(IGeoSpatialDownloadTile tile)
		{
			if (tile.Level >= m_levelCount)
				throw new ArgumentException(string.Format("Level {0} not available.",
				   tile.Level));

			string relativePath = String.Format(@"{0}\{1:D4}\{1:D4}_{2:D4}.{3}",
			   tile.Level, tile.Row, tile.Col, m_imageFileExtension);

			if (m_dataDirectory != null)
			{
				// Search data directory first
				string rawFullPath = Path.Combine(m_dataDirectory, relativePath);
				if (File.Exists(rawFullPath))
					return rawFullPath;
			}

			// If cache doesn't exist, fall back to duplicate texture path.
			if (m_cacheDirectory == null)
				return m_duplicateTexturePath;

			// Try cache with default file extension
			string cacheFullPath = Path.Combine(m_cacheDirectory, relativePath);
			if (File.Exists(cacheFullPath))
				return cacheFullPath;

			// Try cache but accept any valid image file extension
			const string ValidExtensions = ".bmp.dds.dib.hdr.jpg.jpeg.pfm.png.ppm.tga.gif.tif";

			string cacheSearchPath = Path.GetDirectoryName(cacheFullPath);
			if (Directory.Exists(cacheSearchPath))
			{
				foreach (string imageFile in Directory.GetFiles(
				   cacheSearchPath,
				   Path.GetFileNameWithoutExtension(cacheFullPath) + ".*"))
				{
					string extension = Path.GetExtension(imageFile).ToLower();
					if (ValidExtensions.IndexOf(extension) < 0)
						continue;

					return imageFile;
				}
			}

			return cacheFullPath;
		}

		/// <summary>
		/// Figure out how to download the image.
		/// TODO: Allow subclasses to have control over how images are downloaded, 
		/// not just the download url.
		/// </summary>
		protected virtual string GetDownloadUrl(IGeoSpatialDownloadTile tile)
		{
			// No local image, return our "duplicate" tile if any
			if (m_duplicateTexturePath != null && File.Exists(m_duplicateTexturePath))
				return m_duplicateTexturePath;

			// No image available anywhere, give up
			return "";
		}

		/// <summary>
		/// Deletes the cached copy of the tile.
		/// </summary>
		/// <param name="tile"></param>
		internal virtual void DeleteLocalCopy(IGeoSpatialDownloadTile tile)
		{
			string filename = GetLocalPath(tile);
			if (File.Exists(filename))
				File.Delete(filename);
		}

		internal Texture LoadFile(IGeoSpatialDownloadTile tile)
		{
			string filePath = GetLocalPath(tile);
			tile.ImageFilePath = filePath;
            // remove broken files
            if (File.Exists(filePath))
            {
                FileInfo fi = new FileInfo(filePath);
                if (fi.Length == 0)
                    File.Delete(filePath);
            }
			if (!File.Exists(filePath))
			{
				string badFlag = filePath + ".txt";
				if (File.Exists(badFlag))
				{
					FileInfo fi = new FileInfo(badFlag);
					if (DateTime.Now - fi.LastWriteTime < TimeSpan.FromDays(1))
					{
						return null;
					}
					// Timeout period elapsed, retry
					File.Delete(badFlag);
				}

				if (IsDownloadableLayer)
				{
					QueueDownload(tile, filePath);
					return null;
				}

				if (DuplicateTexturePath == null)
					// No image available, neither local nor online.
					return null;

				filePath = DuplicateTexturePath;
			}

			// Use color key
			Texture texture = null;
			if (tile.TileSet.ColorKeyMax != 0)
			{
				texture = ImageHelper.LoadTexture(filePath, tile.TileSet.ColorKey,
				   tile.TileSet.ColorKeyMax);
			}
			else
			{
				texture = ImageHelper.LoadTexture(filePath, tile.TileSet.ColorKey,
				   TextureFormat);
			}
			if (texture == null) return null;

			SurfaceDescription sd = texture.GetLevelDescription(0);
			if (sd.Width != sd.Height)
				Log.Write(Log.Levels.Error, "ISTOR", "non-square texture in file " + filePath + "may cause export issues :");
			tile.TextureSizePixels = sd.Width;

			if (tile.TileSet.CacheExpirationTime != TimeSpan.MaxValue)
			{
				FileInfo fi = new FileInfo(filePath);
				DateTime expiry = fi.LastWriteTimeUtc.Add(tile.TileSet.CacheExpirationTime);
				if (DateTime.UtcNow > expiry)
					QueueDownload(tile, filePath);
			}

			return texture;
		}

		protected virtual void QueueDownload(IGeoSpatialDownloadTile tile, string filePath)
		{
			string url = GetDownloadUrl(tile);
			tile.TileSet.AddToDownloadQueue(tile.TileSet.Camera,
			   new GeoSpatialDownloadRequest(tile, this, filePath, url));
		}
	}
}
