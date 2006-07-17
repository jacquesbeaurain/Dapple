using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using WorldWind.Net;
using System.Windows.Forms;

namespace WorldWind.Terrain
{
    /// <summary>
    /// Provides elevation data (BIL format).
    /// </summary>
    public class TerrainTileService : IDisposable
    {
        #region Private Members
        string m_serverUrl;
        string m_dataSet;
        float m_levelZeroTileSizeDegrees;
        int m_samplesPerTile;
        int m_numberLevels;
        string m_fileExtension;
        string m_terrainTileDirectory;
        #endregion

        #region Properties
        public string ServerUrl
        {
            get
            {
                return m_serverUrl;
            }
        }

        public string DataSet
        {
            get
            {
                return m_dataSet;
            }
        }

        public float LevelZeroTileSizeDegrees
        {
            get
            {
                return m_levelZeroTileSizeDegrees;
            }
        }

        public int SamplesPerTile
        {
            get
            {
                return m_samplesPerTile;
            }
        }

        public string FileExtension
        {
            get
            {
                return m_fileExtension;
            }
        }

        public string TerrainTileDirectory
        {
            get
            {
                return m_terrainTileDirectory;
            }
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref= "T:WorldWind.Terrain.TerrainTileService"/> class.
        /// </summary>
        /// <param name="serverUrl"></param>
        /// <param name="dataset"></param>
        /// <param name="levelZeroTileSizeDegrees"></param>
        /// <param name="samplesPerTile"></param>
        /// <param name="fileExtension"></param>
        /// <param name="numberLevels"></param>
        /// <param name="terrainTileDirectory"></param>
        public TerrainTileService(
            string serverUrl,
            string dataset,
            float levelZeroTileSizeDegrees,
            int samplesPerTile,
            string fileExtension,
            int numberLevels,
            string terrainTileDirectory)
        {
            m_serverUrl = serverUrl;
            m_dataSet = dataset;
            m_levelZeroTileSizeDegrees = levelZeroTileSizeDegrees;
            m_samplesPerTile = samplesPerTile;
            m_numberLevels = numberLevels;
            m_fileExtension = fileExtension.Replace(".", "");
            m_terrainTileDirectory = terrainTileDirectory;
            if (!Directory.Exists(m_terrainTileDirectory))
                Directory.CreateDirectory(m_terrainTileDirectory);
        }

        /// <summary>
        /// Builds terrain tile containing the specified coordinates.
        /// </summary>
        /// <param name="latitude">Latitude in decimal degrees.</param>
        /// <param name="longitude">Longitude in decimal degrees.</param>
        /// <param name="samplesPerDegree"></param>
        /// <returns>Uninitialized terrain tile (no elevation data)</returns>
        public TerrainTile GetTerrainTile(float latitude, float longitude, float samplesPerDegree)
        {
            TerrainTile tile = new TerrainTile(this);

            tile.TargetLevel = m_numberLevels - 1;
            for (int i = 0; i < m_numberLevels; i++)
            {
                if (samplesPerDegree <= m_samplesPerTile / (m_levelZeroTileSizeDegrees * Math.Pow(0.5, i)))
                {
                    tile.TargetLevel = i;
                    break;
                }
            }

            tile.Row = GetRowFromLatitude(latitude, m_levelZeroTileSizeDegrees * Math.Pow(0.5, tile.TargetLevel));
            tile.Col = GetColFromLongitude(longitude, m_levelZeroTileSizeDegrees * Math.Pow(0.5, tile.TargetLevel));
            tile.TerrainTileFilePath = string.Format(CultureInfo.InvariantCulture,
                @"{0}\{4}\{1:D4}\{1:D4}_{2:D4}.{3}",
                m_terrainTileDirectory, tile.Row, tile.Col, m_fileExtension, tile.TargetLevel);
            tile.SamplesPerTile = m_samplesPerTile;
            tile.TileSizeDegrees = m_levelZeroTileSizeDegrees * (float)Math.Pow(0.5f, tile.TargetLevel);
            tile.North = -90.0f + tile.Row * tile.TileSizeDegrees + tile.TileSizeDegrees;
            tile.South = -90.0f + tile.Row * tile.TileSizeDegrees;
            tile.West = -180.0f + tile.Col * tile.TileSizeDegrees;
            tile.East = -180.0f + tile.Col * tile.TileSizeDegrees + tile.TileSizeDegrees;

            return tile;
        }

        // Hack: newer methods in MathEngine class cause problems
        public static int GetColFromLongitude(double longitude, double tileSize)
        {
            return (int)System.Math.Floor((System.Math.Abs(-180.0 - longitude) % 360) / tileSize);
        }

        public static int GetRowFromLatitude(double latitude, double tileSize)
        {
            return (int)System.Math.Floor((System.Math.Abs(-90.0 - latitude) % 180) / tileSize);
        }


        #region IDisposable Members

        public void Dispose()
        {
            if (DrawArgs.DownloadQueue != null)
                DrawArgs.DownloadQueue.Clear(this);
        }

        #endregion
    }


    public class TerrainTile : IDisposable
    {
        public string TerrainTileFilePath;
        public float TileSizeDegrees;
        public int SamplesPerTile;
        public float South;
        public float North;
        public float West;
        public float East;
        public int Row;
        public int Col;
        public int TargetLevel;
        public TerrainTileService m_owner;
        public bool IsInitialized;
        public bool IsValid;

        public List<float> ElevationData;
        protected TerrainDownloadRequest request;

        /// <summary>
        /// How long a bad terrain tile should be cached before retrying download.
        /// </summary>
        public static TimeSpan BadTileRetryInterval = TimeSpan.FromMinutes(30);

        public TerrainTile(TerrainTileService owner)
        {
            m_owner = owner;
        }

        public void Initialize()
        {
            if (IsInitialized)
                return;

            if (!File.Exists(TerrainTileFilePath))
            {
                // Download elevation
                if (request == null)
                {
                    using (request = new TerrainDownloadRequest(this, m_owner, Row, Col, TargetLevel))
                    {
                        request.SaveFilePath = TerrainTileFilePath;
                        request.DownloadInForeGround();
                    }
                }
            }

            if (ElevationData == null)
               ElevationData = new List<float>(SamplesPerTile * SamplesPerTile);

            if (File.Exists(TerrainTileFilePath))
            {
                // Load elevation file
                try
                {
                    using (Stream s = File.OpenRead(TerrainTileFilePath))
                    {
                        byte[] tfBuffer = new byte[SamplesPerTile * SamplesPerTile * 2];
                        if (s.Read(tfBuffer, 0, tfBuffer.Length) == tfBuffer.Length)
                        {
                            int offset = 0;

                            for (int y = 0; y < SamplesPerTile; y++)
                                for (int x = 0; x < SamplesPerTile; x++)
                                    ElevationData.Add(tfBuffer[offset++] + (short)(tfBuffer[offset++] << 8));
                            IsInitialized = true;
                            IsValid = true;
                        }
                    }
                }
                catch
                {
                }

                if (!IsValid)
                {
                    try
                    {
                        // Remove corrupt/failed elevation files after preset time.
                        FileInfo badFileInfo = new FileInfo(TerrainTileFilePath);
                        TimeSpan age = DateTime.Now.Subtract(badFileInfo.LastWriteTime);
                        if (age < BadTileRetryInterval)
                        {
                            // This tile is still flagged bad
                            IsInitialized = true;
                            return;
                        }

                        File.Delete(TerrainTileFilePath);
                    }
                    catch
                    {
                    }
                }
            }
        }

        public float GetElevationAt(float lat, float lon)
        {
           if (lat >= 90 || ElevationData.Count == 0)
                return 0;
            try
            {
                float tileUpperLeftLatitude = this.North;
                float tileUpperLeftLongitude = this.West;

                float deltaLat = tileUpperLeftLatitude - lat;
                float deltaLon = lon - tileUpperLeftLongitude;

                float lat_pixel = (deltaLat / TileSizeDegrees * SamplesPerTile);
                float lon_pixel = (deltaLon / TileSizeDegrees * SamplesPerTile);

                int lat_pixel_min = (int)lat_pixel;
                int lat_pixel_max = lat_pixel_min + 1;

                if (lat_pixel_min >= SamplesPerTile)
                    lat_pixel_min = SamplesPerTile - 1;

                int lon_pixel_min = (int)lon_pixel;
                int lon_pixel_max = lon_pixel_min + 1;

                if (lat_pixel_max >= SamplesPerTile)
                    lat_pixel_max = SamplesPerTile - 1;

                if (lon_pixel_min >= SamplesPerTile)
                    lon_pixel_min = SamplesPerTile - 1;

                if (lon_pixel_max >= SamplesPerTile)
                    lon_pixel_max = SamplesPerTile - 1;

                float x1y1 = ElevationData[lon_pixel_min + lat_pixel_min * SamplesPerTile];
                float x1y2 = ElevationData[lon_pixel_min + lat_pixel_max * SamplesPerTile];
                float x2y1 = ElevationData[lon_pixel_max + lat_pixel_min * SamplesPerTile];
                float x2y2 = ElevationData[lon_pixel_max + lat_pixel_max * SamplesPerTile];
                float x1_avg = x1y1 * (1 - (lat_pixel - lat_pixel_min)) + x1y2 * (lat_pixel - lat_pixel_min);
                float x2_avg = x2y1 * (1 - (lat_pixel - lat_pixel_min)) + x2y2 * (lat_pixel - lat_pixel_min);
                float avg_h = x1_avg * (1 - (lon_pixel - lon_pixel_min)) + x2_avg * (lon_pixel - lon_pixel_min);

                return avg_h;
            }
            catch
            {
            }
            return 0;
        }
        #region IDisposable Members

        public void Dispose()
        {
            if (request != null)
            {
                request.Dispose();
                request = null;
            }

            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
