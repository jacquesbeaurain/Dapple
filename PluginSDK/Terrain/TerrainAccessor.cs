using System;

namespace WorldWind.Terrain
{
   /// <summary>
   /// Terrain (elevation) interface
   /// </summary>
	public abstract class TerrainAccessor : IDisposable
   {
        private bool isOn = true;
      protected string m_name;
      protected double m_north;
      protected double m_south;
      protected double m_east;
      protected double m_west;
      protected TerrainAccessor[] m_higherResolutionSubsets;

      /// <summary>
      /// North boundary
      /// </summary>
      internal double North
      {
         get
         {
            return m_north;
         }
      }

      /// <summary>
      /// South boundary
      /// </summary>
      internal double South
      {
         get
         {
            return m_south;
         }
      }

      /// <summary>
      /// West boundary
      /// </summary>
      internal double West
      {
         get
         {
            return m_west;
         }
      }

      /// <summary>
      /// East boundary
      /// </summary>
      internal double East
      {
         get
         {
            return m_east;
         }
      }

        /// <summary>
        /// Hide/Show this object.
        /// </summary>
        [System.ComponentModel.Description("This layer's enabled status.")]
        internal virtual bool IsOn
        {
            get { return this.isOn; }
            set
            {
                this.isOn = value;
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
		  public abstract float GetElevationAt(double latitude, double longitude, double targetSamplesPerDegree);

      /// <summary>
      /// Get terrain elevation at specified location.  
      /// </summary>
      /// <param name="latitude">Latitude in decimal degrees.</param>
      /// <param name="longitude">Longitude in decimal degrees.</param>
      /// <returns>Returns 0 if the tile is not available on disk.</returns>
		  public virtual float GetElevationAt(double latitude, double longitude)
      {
         return GetElevationAt(latitude, longitude, 0);
      }

      /// <summary>
      /// Get fast terrain elevation at specified location from cached data. 
      /// Will not trigger any download or file loading from cache - just memory.
      /// </summary>
      /// <param name="latitude">Latitude in decimal degrees.</param>
      /// <param name="longitude">Longitude in decimal degrees.</param>
      /// <returns>Returns 0 if the tile is not available in cache.</returns>
      internal virtual float GetCachedElevationAt(double latitude, double longitude)
      {
         return 0f;
      }

      /// <summary>
      /// Gets the elevation array for given geographic bounding box and resolution.
      /// </summary>
      /// <param name="north">North edge in decimal degrees.</param>
      /// <param name="south">South edge in decimal degrees.</param>
      /// <param name="west">West edge in decimal degrees.</param>
      /// <param name="east">East edge in decimal degrees.</param>
      /// <param name="samples"></param>
      internal virtual TerrainTile GetElevationArray(double north, double south, double west, double east, int samples)
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

         double latrange = Math.Abs(north - south);
         double lonrange = Math.Abs(east - west);

         float[,] data = new float[samples, samples];
         float scaleFactor = 1.0f / (samples - 1);
         for (int x = 0; x < samples; x++)
         {
            for (int y = 0; y < samples; y++)
            {
               double curLat = north - scaleFactor * latrange * x;
               double curLon = west + scaleFactor * lonrange * y;

               data[x, y] = GetElevationAt(curLat, curLon, 0);
            }
         }
         res.ElevationData = data;

         return res;
      }

		public virtual void Dispose()
      {
      }
   }
}
