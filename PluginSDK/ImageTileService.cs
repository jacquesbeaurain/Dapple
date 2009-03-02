using System;
using System.Globalization;

namespace WorldWind
{
	/// <summary>
	/// Summary description for ImageTileService.
	/// </summary>
	internal class ImageTileService
	{
		#region Private Members
		TimeSpan _cacheExpirationTime = TimeSpan.MaxValue;
		string _datasetName;
		string _serverUri;
		string _serverLogoPath;
		#endregion

		#region Properties

		internal TimeSpan CacheExpirationTime
		{
			get
			{
				return this._cacheExpirationTime;
			}								   
		}

      internal string DataSetName
      {
         get
         {
            return this._datasetName;
         }
      }

      internal string ServerURI
      {
         get
         {
            return this._serverUri;
         }
      }
		internal string ServerLogoPath
		{
			get
			{
				return this._serverLogoPath;
			}
			set
			{
				this._serverLogoPath = value;
			}								   
		}
		#endregion

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.ImageTileService"/> class.
		/// </summary>
		/// <param name="datasetName"></param>
		/// <param name="serverUri"></param>
		/// <param name="serverLogoPath"></param>
		/// <param name="cacheExpirationTime"></param>
		internal ImageTileService(
			string datasetName,
			string serverUri,
			string serverLogoPath,
			TimeSpan cacheExpirationTime)
		{
			this._serverUri = serverUri;
			this._datasetName = datasetName;
			this._serverLogoPath = serverLogoPath;
			this._cacheExpirationTime = cacheExpirationTime;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.ImageTileService"/> class.
		/// </summary>
		/// <param name="datasetName"></param>
		/// <param name="serverUri"></param>
		internal ImageTileService(
			string datasetName,
			string serverUri,
			string serverLogoPath)
		{
			this._serverUri = serverUri;
			this._datasetName = datasetName;
			this._serverLogoPath = serverLogoPath;
		}

		internal virtual string GetImageTileServiceUri(int level, int row, int col)
		{
			return String.Format(CultureInfo.InvariantCulture, "{0}?T={1}&L={2}&X={3}&Y={4}", this._serverUri, this._datasetName, level, col, row);
		}
	}
}
