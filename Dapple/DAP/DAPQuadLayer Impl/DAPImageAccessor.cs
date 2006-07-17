using Microsoft.DirectX.Direct3D;
using System;
using System.Drawing;
using System.IO;
using WorldWind;
using WorldWind.Net.Wms;
using WorldWind.Renderable;


namespace GeosoftPlugin.New
{
	/// <summary>
	/// Summary description for ImageAccessor.
	/// </summary>
	public class DAPImageAccessor : ImageAccessor , IDisposable
	{
		#region Private Members

      protected string m_strName;
      protected string m_appDirectory;
      protected Geosoft.GX.DAPGetData.Server m_oServer;

		#endregion

		#region Properties

       public Geosoft.GX.DAPGetData.Server Server
       {
           get
           {
               return m_oServer;
           }
       }

      public string Name
      {
         get
         {
            return m_strName;
         }
      }

		#endregion

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.ImageAccessor"/> class.
		/// </summary>
		/// <param name="permanentTextureDirectory"></param>
		/// <param name="textureSizePixels"></param>
		/// <param name="levelZeroTileSizeDegrees"></param>
		/// <param name="numberLevels"></param>
		/// <param name="imageFileExtension"></param>
		/// <param name="cacheDirectory"></param>
      public DAPImageAccessor(
         string name,
          Geosoft.GX.DAPGetData.Server server,
         string permanentTextureDirectory,
         int textureSizePixels,
         decimal levelZeroTileSizeDegrees,
         int numberLevels,
         string imageFileExtension,
         string cacheDirectory,
          string appDir)
         : base(permanentTextureDirectory,
         textureSizePixels, levelZeroTileSizeDegrees,
         numberLevels, imageFileExtension, cacheDirectory)
      {
         m_strName = name;
         m_oServer = server;
         m_dataDirectory = permanentTextureDirectory;
         m_appDirectory = appDir;
      }
		
		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.ImageAccessor"/> class.
		/// </summary>
		/// <param name="permanentTextureDirectory"></param>
		/// <param name="textureSizePixels"></param>
		/// <param name="levelZeroTileSizeDegrees"></param>
		/// <param name="numberLevels"></param>
		/// <param name="imageFileExtension"></param>
		/// <param name="cacheDirectory"></param>
		/// <param name="duplicateTextureFilePath"></param>
		public DAPImageAccessor(
         string name,
          Geosoft.GX.DAPGetData.Server server,
			string permanentTextureDirectory, 
			int textureSizePixels, 
			decimal levelZeroTileSizeDegrees,
			int numberLevels,
			string imageFileExtension,
			string cacheDirectory,
         string duplicateTextureFilePath,
          string appDir
			):this(name, server, permanentTextureDirectory,
         textureSizePixels, levelZeroTileSizeDegrees, numberLevels ,
         imageFileExtension, cacheDirectory, appDir)

		{
			m_duplicateTexturePath = duplicateTextureFilePath;

		}

		public override ImageTileInfo GetImageTileInfo(int level, int row, int col)
		{
			if(level >= m_numberLevels)
				throw new ArgumentException("Level " + level.ToString() + " not available.");

         string relativePath = String.Format(@"{0}\{1:D4}\{1:D4}_{2:D4}_{3:D6}.{4}", level, row, col, m_textureSizePixels, m_imageFileExtension);
			
			if(m_dataDirectory != null)
			{
				// Search data directory first
				string rawFullPath = Path.Combine( m_dataDirectory, relativePath );
				if(File.Exists(rawFullPath))
					return new ImageTileInfo(rawFullPath);
			}
	
			// Try cache with default file extension
			string cacheFullPath = Path.Combine( m_cacheDirectory, relativePath );
			if(File.Exists(cacheFullPath))
				return new ImageTileInfo(cacheFullPath);

			// Try cache but accept any valid image file extension
			const string ValidExtensions = ".bmp.dds.dib.hdr.jpg.jpeg.pfm.png.ppm.tga.gif.tif";
			
			string cacheSearchPath = Path.GetDirectoryName(cacheFullPath);
			if(Directory.Exists(cacheSearchPath))
			{
				foreach( string imageFile in Directory.GetFiles(
					cacheSearchPath, 
					Path.GetFileNameWithoutExtension(cacheFullPath) + ".*") )
				{
					string extension = Path.GetExtension(imageFile).ToLower();
					if(ValidExtensions.IndexOf(extension)<0)
						continue;

					return new ImageTileInfo(imageFile);
				}
			}

			return new ImageTileInfo(Path.Combine(m_cacheDirectory, relativePath), "Geosoft");

		}

      protected override GeoSpatialDownloadRequest GetDownloadRequest(GeographicBoundingBox geoBox, ImageTileInfo info)
      {
         return new DAPDownloadRequest(geoBox, info.ImagePath, m_downloadQueue, this);
      }

		#region IDisposable Members

		public override void Dispose()
		{

         this.m_imageTileService = null;
         this.m_oServer = null;

         base.Dispose();
		}

		#endregion

  }
}
