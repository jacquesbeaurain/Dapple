using System;
using System.Globalization;

namespace WorldWind
{
	/// <summary>
	/// Summary description for ImageTileService.
	/// </summary>
	public class ImageTileService
	{
		#region Private Members
		TimeSpan _cacheExpirationTime = TimeSpan.MaxValue;
		string _datasetName;
		string _serverUri;
		#endregion

		#region Properties

		public TimeSpan CacheExpirationTime
		{
			get
			{
				return this._cacheExpirationTime;
			}								   
		}

      public string DataSetName
      {
         get
         {
            return this._datasetName;
         }
      }

      public string ServerURI
      {
         get
         {
            return this._serverUri;
         }
      }
		#endregion

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.ImageTileService"/> class.
		/// </summary>
		/// <param name="datasetName"></param>
		/// <param name="serverUri"></param>
		/// <param name="cacheExpirationTime"></param>
		public ImageTileService(
			string datasetName,
			string serverUri,
			TimeSpan cacheExpirationTime)
		{
			this._serverUri = serverUri;
			this._datasetName = datasetName;
			this._cacheExpirationTime = cacheExpirationTime;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.ImageTileService"/> class.
		/// </summary>
		/// <param name="datasetName"></param>
		/// <param name="serverUri"></param>
		public ImageTileService(
			string datasetName,
			string serverUri)
		{
			this._serverUri = serverUri;
			this._datasetName = datasetName;
		}

		public virtual string GetImageTileServiceUri(int level, int row, int col)
		{
			return String.Format(CultureInfo.InvariantCulture, "{0}?T={1}&L={2}&X={3}&Y={4}", this._serverUri, this._datasetName, level, col, row);
		}
	}
}
