using System;
using System.Collections.Generic;

namespace WorldWind.Terrain
{
    /// <summary>
    /// Terrain (elevation) interface
    /// </summary>
    public abstract class TerrainAccessor : IDisposable
    {
        protected string m_name;
        protected double m_north;
        protected double m_south;
        protected double m_east;
        protected double m_west;

        /// <summary>
        /// Terrain model name
        /// </summary>
        public string Name
        {
            get
            {
                return m_name;
            }
            set
            {
                m_name = value;
            }
        }

        /// <summary>
        /// North boundary
        /// </summary>
        public double North
        {
            get
            {
                return m_north;
            }
            set
            {
                m_north = value;
            }
        }

        /// <summary>
        /// South boundary
        /// </summary>
        public double South
        {
            get
            {
                return m_south;
            }
            set
            {
                m_south = value;
            }
        }

        /// <summary>
        /// West boundary
        /// </summary>
        public double West
        {
            get
            {
                return m_west;
            }
            set
            {
                m_west = value;
            }
        }

        /// <summary>
        /// East boundary
        /// </summary>
        public double East
        {
            get
            {
                return m_east;
            }
            set
            {
                m_east = value;
            }
        }

        /// <summary>
        /// Gets the terrain elevation at a given Latitude, Longitude, 
        /// and resolution accuracy in the latitude/longitude geographic frame of reference.
        /// </summary>
        /// <param name="latitude">Latitude in decimal degrees.</param>
        /// <param name="longitude">Longitude in decimal degrees.</param>
        /// <param name="targetSamplesPerDegree"></param>
        /// <returns>Returns 0 if the tile is not available on disk.</returns>
        public abstract float GetElevationAt(float latitude, float longitude, float targetSamplesPerDegree);

        /// <summary>
        /// Get terrain elevation at specified location.  
        /// </summary>
        /// <param name="latitude">Latitude in decimal degrees.</param>
        /// <param name="longitude">Longitude in decimal degrees.</param>
        /// <returns>Returns 0 if the tile is not available on disk.</returns>
        public virtual float GetElevationAt(float latitude, float longitude)
        {
            return GetElevationAt(latitude, longitude, 0);
        }

        /// <summary>
        /// Gets the elevation array for given geographic bounding box and resolution.
        /// </summary>
        /// <param name="north">North edge in decimal degrees.</param>
        /// <param name="south">South edge in decimal degrees.</param>
        /// <param name="west">West edge in decimal degrees.</param>
        /// <param name="east">East edge in decimal degrees.</param>
        /// <param name="samples"></param>
        public virtual TerrainTile GetElevationArray(float north, float south, float west, float east, int samples)
        {
            TerrainTile res = null;
            res = new TerrainTile(null);
            res.North = north;
            res.South = south;
            res.West = west;
            res.East = east;
            res.SamplesPerTile = samples;
            res.IsInitialized = true;
            res.IsValid = true;

            float samplesPerDegree = (float)(samples / (north - south));
            float latrange = Math.Abs(north - south);
            float lonrange = Math.Abs(east - west);

            res.ElevationData = new List<float>(samples * samples);
            float scaleFactor = (float)1 / (samples - 1);
            float curLat, curLon;
            for (int y = 0; y < samples; y++)
            {
                for (int x = 0; x < samples; x++)
                {
                    curLat = north - scaleFactor * latrange * x;
                    curLon = west + scaleFactor * lonrange * y;

                    res.ElevationData.Add(GetElevationAt(curLat, curLon, 0));
                }
            }
            
            return res;
        }

        public virtual void Dispose()
        {
        }
    }
}
