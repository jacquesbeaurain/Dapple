using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using WorldWind.Net.Wms;

namespace WorldWind.Terrain
{
   /// <summary>
   /// Reads NLT terrain/elevation data (BIL files).
   /// </summary>
   public class NltTerrainAccessor : TerrainAccessor
   {
      public static int CacheSize = 100;
      protected TerrainTileService m_terrainTileService;
      protected TerrainAccessor[] m_higherResolutionSubsets;
      protected Hashtable m_tileCache = new Hashtable();

      #region Properties

      public TerrainAccessor this[int index]
      {
         get
         {
            return m_higherResolutionSubsets[index];
         }
      }

      #endregion

      #region Public Methods

      /// <summary>
      /// Initializes a new instance of the <see cref= "T:WorldWind.Terrain.NltTerrainAccessor"/> class.
      /// </summary>
      /// <param name="name"></param>
      /// <param name="west"></param>
      /// <param name="south"></param>
      /// <param name="east"></param>
      /// <param name="north"></param>
      /// <param name="terrainTileService"></param>
      /// <param name="higherResolutionSubsets"></param>
      public NltTerrainAccessor(string name, double west, double south, double east, double north,
         TerrainTileService terrainTileService, TerrainAccessor[] higherResolutionSubsets)
      {
         m_name = name;
         m_west = west;
         m_south = south;
         m_east = east;
         m_north = north;
         m_terrainTileService = terrainTileService;
         m_higherResolutionSubsets = higherResolutionSubsets;
      }

      /// <summary>
      /// Get terrain elevation at specified location.  
      /// </summary>
      /// <param name="latitude">Latitude in decimal degrees.</param>
      /// <param name="longitude">Longitude in decimal degrees.</param>
      /// <param name="targetSamplesPerDegree"></param>
      /// <returns>Returns 0 if the tile is not available on disk.</returns>
      public override float GetElevationAt(float latitude, float longitude, float targetSamplesPerDegree)
      {
         try
         {
            if (m_terrainTileService == null || targetSamplesPerDegree < 3.0)
               return 0;

            if (m_higherResolutionSubsets != null)
            {
               foreach (TerrainAccessor higherResSub in m_higherResolutionSubsets)
               {
                  if (latitude > higherResSub.South && latitude < higherResSub.North &&
                     longitude > higherResSub.West && longitude < higherResSub.East)
                  {
                     return higherResSub.GetElevationAt(latitude, longitude, targetSamplesPerDegree);
                  }
               }
            }

            TerrainTile tt = m_terrainTileService.GetTerrainTile(latitude, longitude, targetSamplesPerDegree);
            TerrainTileCacheEntry ttce = (TerrainTileCacheEntry)m_tileCache[tt.TerrainTileFilePath];
            if (ttce == null)
            {
               ttce = new TerrainTileCacheEntry(tt);
               AddToCache(ttce);
            }

            if (!ttce.TerrainTile.IsInitialized)
               ttce.TerrainTile.Initialize();
            ttce.LastAccess = DateTime.Now;
            return ttce.TerrainTile.GetElevationAt(latitude, longitude);
         }
         catch (Exception)
         {
         }
         return 0;
      }

      /// <summary>
      /// Get terrain elevation at specified location.  
      /// </summary>
      /// <param name="latitude">Latitude in decimal degrees.</param>
      /// <param name="longitude">Longitude in decimal degrees.</param>
      /// <returns>Returns 0 if the tile is not available on disk.</returns>
      public override float GetElevationAt(float latitude, float longitude)
      {
         return GetElevationAt(latitude, longitude, m_terrainTileService.SamplesPerTile / m_terrainTileService.LevelZeroTileSizeDegrees);
      }

      /// <summary>
      /// Builds a terrain array with specified boundaries
      /// </summary>
      /// <param name="north">North edge in decimal degrees.</param>
      /// <param name="south">South edge in decimal degrees.</param>
      /// <param name="west">West edge in decimal degrees.</param>
      /// <param name="east">East edge in decimal degrees.</param>
      /// <param name="samples"></param>
      public override TerrainTile GetElevationArray(float north, float south, float west, float east,
         int samples)
      {
         TerrainTile res = null;

         if (m_higherResolutionSubsets != null)
         {
            // TODO: Support more than 1 level of higher resolution sets and allow user selections
            foreach (TerrainAccessor higherResSub in m_higherResolutionSubsets)
            {
               if (north <= higherResSub.North && south >= higherResSub.South &&
                  west >= higherResSub.West && east <= higherResSub.East)
               {
                  res = higherResSub.GetElevationArray(north, south, west, east, samples);
                  return res;
               }
            }
         }

         res = new TerrainTile(m_terrainTileService);
         res.North = north;
         res.South = south;
         res.West = west;
         res.East = east;
         res.SamplesPerTile = samples;
         res.IsInitialized = true;
         res.IsValid = true;

         float samplesPerDegree = (samples / (north - south));
         float latrange = Math.Abs(north - south);
         float lonrange = Math.Abs(east - west);
         TerrainTileCacheEntry ttce = null;

         res.ElevationData = new List<float>(samples * samples);

         if (samplesPerDegree < 3.0)
            return null;

         float scaleFactor = 1.0f / (samples - 1);
         float curLat, curLon;
         for (int y = 0; y < samples; y++)
         {
            for (int x = 0; x < samples; x++)
            {
               curLat = north - scaleFactor * latrange * x;
               curLon = west + scaleFactor * lonrange * y;

               if (ttce == null ||
                  curLat < ttce.TerrainTile.South ||
                  curLat > ttce.TerrainTile.North ||
                  curLon < ttce.TerrainTile.West ||
                  curLon > ttce.TerrainTile.East)
               {
                  TerrainTile tt = m_terrainTileService.GetTerrainTile(curLat, curLon, samplesPerDegree);
                  ttce = (TerrainTileCacheEntry)m_tileCache[tt.TerrainTileFilePath];
                  if (ttce == null)
                  {
                     ttce = new TerrainTileCacheEntry(tt);
                     AddToCache(ttce);
                  }
                  if (!ttce.TerrainTile.IsInitialized)
                     ttce.TerrainTile.Initialize();
                  ttce.LastAccess = DateTime.Now;
                  if (!tt.IsValid)
                     res.IsValid = false;
               }

               res.ElevationData.Add(ttce.TerrainTile.GetElevationAt(curLat, curLon));
            }
         }
         return res;
      }

      #endregion

      protected void AddToCache(TerrainTileCacheEntry ttce)
      {
         if (!m_tileCache.ContainsKey(ttce.TerrainTile.TerrainTileFilePath))
         {
            if (m_tileCache.Count >= CacheSize)
            {
               // Remove least recently used tile
               TerrainTileCacheEntry oldestTile = null;
               foreach (TerrainTileCacheEntry curEntry in m_tileCache.Values)
               {
                  if (oldestTile == null)
                     oldestTile = curEntry;
                  else
                  {
                     if (curEntry.LastAccess < oldestTile.LastAccess)
                        oldestTile = curEntry;
                  }
               }

               m_tileCache.Remove(oldestTile);
            }

            m_tileCache.Add(ttce.TerrainTile.TerrainTileFilePath, ttce);
         }
      }

      public class TerrainTileCacheEntry
      {
         DateTime m_lastAccess = DateTime.Now;
         TerrainTile m_terrainTile;

         /// <summary>
         /// Constructor.
         /// </summary>
         /// <param name="tile">TerrainTile to be cached.</param>
         public TerrainTileCacheEntry(TerrainTile tile)
         {
            m_terrainTile = tile;
         }

         public TerrainTile TerrainTile
         {
            get
            {
               return m_terrainTile;
            }
            set
            {
               m_terrainTile = value;
            }
         }

         public DateTime LastAccess
         {
            get
            {
               return m_lastAccess;
            }
            set
            {
               m_lastAccess = value;
            }
         }
      }

      public override void Dispose()
      {
         if (m_terrainTileService != null)
         {
            m_terrainTileService.Dispose();
            m_terrainTileService = null;
         }
      }
   }
}