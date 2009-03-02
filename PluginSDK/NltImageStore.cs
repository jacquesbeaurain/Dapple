using WorldWind.Renderable;
using System;
using System.Globalization;

namespace WorldWind
{
	/// <summary>
	/// Formats urls for images stored in NLT-style
	/// </summary>
	public class NltImageStore : ImageStore
    {
        #region Private Members

        string m_dataSetName;
        string m_serverUri;
        string m_formatString;

        #endregion

		  public override bool IsDownloadableLayer
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref= "T:WorldWind.ImageTileService"/> class.
        /// </summary>
        /// <param name="dataSetName"></param>
        /// <param name="serverUri"></param>
		  public NltImageStore(
            string dataSetName,
            string serverUri)
        {
            m_serverUri = serverUri;
            m_dataSetName = dataSetName;
            m_formatString = "{0}?T={1}&L={2}&X={3}&Y={4}";
        }

        internal NltImageStore(
            string dataSetName,
            string serverUri,
            string formatString)
        {
            m_serverUri = serverUri;
            m_dataSetName = dataSetName;
            m_formatString = formatString;
        }

        protected override string GetDownloadUrl(IGeoSpatialDownloadTile tile)
        {
            return string.Format(CultureInfo.InvariantCulture,
                m_formatString, m_serverUri,
					 m_dataSetName, tile.Level, tile.Col, tile.Row,
					 tile.West, tile.South, tile.East, tile.North);
        }

       internal string ServerUri
       {
          get
          {
             return m_serverUri;
          }
       }

       internal string DatasetName
       {
          get
          {
             return m_dataSetName;
          }
       }
    }
}
