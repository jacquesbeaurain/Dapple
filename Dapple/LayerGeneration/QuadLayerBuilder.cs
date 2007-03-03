using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using WorldWind;
using WorldWind.Renderable;
using System.Xml;

namespace Dapple.LayerGeneration
{
   public class QuadLayerBuilder : ImageBuilder
   {
      #region Static

      public static readonly string URLProtocolName = "gxtile://";

      public static readonly string CacheSubDir = "Tile Server Cache";

      public static string TypeName
      {
         get
         {
            return "QuadTileSet";
         }
      }

      public static string GetServerFileNameFromUrl(string strurl)
      {
         string serverfile = strurl;
         int iUrl = strurl.IndexOf("//") + 2;
         if (iUrl == -1)
            iUrl = strurl.IndexOf("\\") + 2;
         if (iUrl != -1)
            serverfile = serverfile.Substring(iUrl);
         foreach (Char ch in Path.GetInvalidFileNameChars())
            serverfile = serverfile.Replace(ch.ToString(), "_");
         return serverfile;
      }

      #endregion


      QuadTileSet m_oQuadTileSet;
      string m_strTextureDirectory;
      string m_strCacheRoot;

      //Image Accessor
      private decimal m_decLevelZeroTileSizeDegrees = 30;
      private int m_intNumberLevels = 1;
      private int m_intTextureSizePixels = 256;
      string m_strImageFileExtension = ".png";
      ImageTileService m_oImageTileService = null;

      //Image Tile Service
      string m_strServerUrl = string.Empty;
      string m_strDataSetName = string.Empty;

      //QuadTileLayer
      int distAboveSurface = 0;
      bool terrainMapped = false;
      GeographicBoundingBox m_hBoundary = new GeographicBoundingBox(0, 0, 0, 0);
      ImageAccessor m_oImageAccessor = null;

      bool m_blnIsChanged = true;

      private QuadLayerBuilder(string name, int height, bool isTerrainMapped, GeographicBoundingBox boundary,
         ImageAccessor imageAccessor, byte opacity, World World, string textureDirectory, string cacheDirectory, IBuilder parent)
      {
         m_strName = name;
         distAboveSurface = height;
         terrainMapped = isTerrainMapped;
         m_hBoundary = boundary;
         m_oImageAccessor = imageAccessor;

         m_decLevelZeroTileSizeDegrees = imageAccessor.LevelZeroTileSizeDegrees;
         m_intNumberLevels = imageAccessor.LevelCount;
         m_intTextureSizePixels = imageAccessor.TextureSizePixels;
         m_strImageFileExtension = imageAccessor.ImageExtension;

         m_bOpacity = opacity;
         m_strTextureDirectory = textureDirectory;
         m_strCacheRoot = cacheDirectory;
         m_oWorld = World;
         m_Parent = parent;
      }

      public QuadLayerBuilder(string name, int height, bool isTerrainMapped, GeographicBoundingBox boundary,
         decimal levelZeroTileSize, int levels, int textureSize, ImageTileService tileService, string imageExtension,
         byte opacity, World world, string textureDirectory, string cacheDirectory, IBuilder parent)
         : this(name, height, isTerrainMapped, boundary,
         new ImageAccessor(textureDirectory, textureSize, levelZeroTileSize, levels, imageExtension, Path.Combine(cacheDirectory, name), tileService),
         opacity, world, textureDirectory, cacheDirectory, parent)
      {
         m_strServerUrl = tileService.ServerURI;
         m_strDataSetName = tileService.DataSetName;
         m_oImageTileService = tileService;
      }

      public QuadLayerBuilder(string name, int height, bool isTerrainMapped, GeographicBoundingBox boundary,
         decimal levelZeroTileSize, int levels, int textureSize, string serverURL, string dataSetName, string imageExtension,
         byte opacity, World world, string textureDirectory, string cacheDirectory, IBuilder parent)
         : this(name, height, isTerrainMapped, boundary, levelZeroTileSize, levels, textureSize,
         new ImageTileService(dataSetName, serverURL), imageExtension, opacity, world, textureDirectory, cacheDirectory, parent)
      {
      }

      public override RenderableObject GetLayer()
      {
         return GetQuadLayer();
      }

      public QuadTileSet GetQuadLayer()
      {
         if (m_blnIsChanged)
         {
            string strCachePath = GetCachePath();
            if (m_oImageTileService == null && m_strDataSetName != string.Empty && m_strServerUrl != string.Empty)
            {
               m_oImageTileService = new ImageTileService(m_strDataSetName, m_strServerUrl);
            }
            m_oImageAccessor = new ImageAccessor(strCachePath,
                m_intTextureSizePixels,
                m_decLevelZeroTileSizeDegrees,
                m_intNumberLevels,
                m_strImageFileExtension,
                strCachePath,
                m_oImageTileService);

            m_oQuadTileSet = new QuadTileSet(m_strName,
                m_hBoundary,
                m_oWorld,
                distAboveSurface,
                (terrainMapped ? m_oWorld.TerrainAccessor : null),
                m_oImageAccessor, m_bOpacity, false);
            m_blnIsChanged = false;
         }
         return m_oQuadTileSet;
      }

      #region IBuilder Members

      public override byte Opacity
      {
         get
         {
            if (m_oQuadTileSet != null)
               return m_oQuadTileSet.Opacity;
            return m_bOpacity;
         }
         set
         {
            bool bChanged = false;
            if (m_bOpacity != value)
            {
               m_bOpacity = value;
               bChanged = true;
            }
            if (m_oQuadTileSet != null && m_oQuadTileSet.Opacity != value)
            {
               m_oQuadTileSet.Opacity = value;
               bChanged = true;
            }
            if (bChanged)
               SendBuilderChanged(BuilderChangeType.OpacityChanged);
         }
      }

      public override bool Visible
      {
         get
         {
            if (m_oQuadTileSet != null)
               return m_oQuadTileSet.IsOn;

            return m_IsOn;
         }
         set
         {
            bool bChanged = false;
            if (m_IsOn != value)
            {
               m_IsOn = value;
               bChanged = true;
            }
            if (m_oQuadTileSet != null && m_oQuadTileSet.IsOn != value)
            {
               m_oQuadTileSet.IsOn = value;
               bChanged = true;
            }

            if (bChanged)
               SendBuilderChanged(BuilderChangeType.VisibilityChanged);
         }
      }

      public override string Type
      {
         get { return TypeName; }
      }

      public override bool IsChanged
      {
         get { return m_blnIsChanged; }
      }

      public override string LogoKey
      {
         get { return "tile"; }
      }

      public override bool bIsDownloading(out int iBytesRead, out int iTotalBytes)
      {
         if (m_oQuadTileSet != null)
            return m_oQuadTileSet.bIsDownloading(out iBytesRead, out iTotalBytes);
         else
         {
            iBytesRead = 0;
            iTotalBytes = 0;
            return false;
         }
      }

      #endregion

      #region Public Properties

      public decimal LevelZeroTileSize
      {
         get { return m_decLevelZeroTileSizeDegrees; }
         set
         {
            if (value != m_decLevelZeroTileSizeDegrees)
            {
               m_blnIsChanged = true;
               m_decLevelZeroTileSizeDegrees = value;
            }
         }
      }

      public int Levels
      {
         get { return m_intNumberLevels; }
         set
         {
            if (m_intNumberLevels != value)
            {
               m_blnIsChanged = true;
               m_intNumberLevels = value;
            }
         }
      }

      #endregion

      #region ImageBuilder Members

      public override GeographicBoundingBox Extents
      {
         get { return m_hBoundary; }
      }

      public override int ImagePixelSize
      {
         get { return m_intTextureSizePixels; }
         set
         {
            if (m_intTextureSizePixels != value)
            {
               m_blnIsChanged = true;
               m_intTextureSizePixels = value;
            }
         }
      }

      #endregion

      public override string ServiceType
      {
         get { return "Tile Server"; }
      }
      
      public string TileServerURL
      {
         get
         {
            return m_strServerUrl;
         }
      }

      public string ImageFileExtension
      {
         get
         {
            return m_strImageFileExtension;
         }
      }

      public string TileServerDatasetName
      {
         get
         {
            return m_strDataSetName;
         }
      }

      public override string GetURI()
      {
         return m_strServerUrl.Replace("http://", URLProtocolName) + "?" +
         "datasetname=" + m_strDataSetName + "&" +
         "name=" + System.Web.HttpUtility.UrlEncode(m_strName) + "&" +
         "height=" + distAboveSurface.ToString() + "&" +
         "north=" + m_hBoundary.North.ToString(System.Globalization.CultureInfo.InvariantCulture) + "&" +
         "east=" + m_hBoundary.East.ToString(System.Globalization.CultureInfo.InvariantCulture) + "&" +
         "south=" + m_hBoundary.South.ToString(System.Globalization.CultureInfo.InvariantCulture) + "&" +
         "west=" + m_hBoundary.West.ToString(System.Globalization.CultureInfo.InvariantCulture) + "&" +
         "size=" + m_intTextureSizePixels.ToString() + "&" +
         "levels=" + m_intNumberLevels.ToString() + "&" +
         "lvl0tilesize=" + m_decLevelZeroTileSizeDegrees.ToString(System.Globalization.CultureInfo.InvariantCulture) + "&" +
         "terrainMapped=" + terrainMapped.ToString() + "&" +
         "imgfileext=" + m_strImageFileExtension;
      }

      public override string GetCachePath()
      {
         return Path.Combine(Path.Combine(Path.Combine(Path.Combine(m_strCacheRoot, CacheSubDir), GetServerFileNameFromUrl(m_strServerUrl)), Utility.StringHash.GetBase64HashForPath(m_strDataSetName)), m_decLevelZeroTileSizeDegrees.GetHashCode().ToString());
      }

      public static QuadLayerBuilder GetQuadLayerBuilderFromURI(string uri, string textureDir, string cacheDir, World world, IBuilder parent)
      {
         try
         {
            string serverUrl = uri.Substring(0, uri.IndexOf("?")).Replace(URLProtocolName, "http://");
            string rest = uri.Substring(uri.IndexOf("?") + 1);
            string[] pairs = rest.Split('&');
            string datasetname = pairs[0].Substring(pairs[0].IndexOf('=') + 1);
            string name = pairs[1].Substring(pairs[1].IndexOf('=') + 1);
            int height = Convert.ToInt32(pairs[2].Substring(pairs[2].IndexOf('=') + 1));
            double north = Convert.ToDouble(pairs[3].Substring(pairs[3].IndexOf('=') + 1), System.Globalization.CultureInfo.InvariantCulture);
            double east = Convert.ToDouble(pairs[4].Substring(pairs[4].IndexOf('=') + 1), System.Globalization.CultureInfo.InvariantCulture);
            double south = Convert.ToDouble(pairs[5].Substring(pairs[5].IndexOf('=') + 1), System.Globalization.CultureInfo.InvariantCulture);
            double west = Convert.ToDouble(pairs[6].Substring(pairs[6].IndexOf('=') + 1), System.Globalization.CultureInfo.InvariantCulture);
            int size = Convert.ToInt32(pairs[7].Substring(pairs[7].IndexOf('=') + 1));
            int levels = Convert.ToInt32(pairs[8].Substring(pairs[8].IndexOf('=') + 1));
            decimal lvl0tilesize = Convert.ToDecimal(pairs[9].Substring(pairs[9].IndexOf('=') + 1), System.Globalization.CultureInfo.InvariantCulture);
            bool terrainMapped = Convert.ToBoolean(pairs[10].Substring(pairs[10].IndexOf('=') + 1));
            string fileExt = pairs[11].Substring(pairs[11].IndexOf('=') + 1);

            return new QuadLayerBuilder(name.Replace('+', ' '), height, terrainMapped, new GeographicBoundingBox(north, south, west, east),
               lvl0tilesize, levels, size, new ImageTileService(datasetname, serverUrl), fileExt, 255,
               world, textureDir, cacheDir, parent);
         }
         catch { }

         return null;
      }

      public override object Clone()
      {
         return new QuadLayerBuilder(m_strName, distAboveSurface, terrainMapped, m_hBoundary, m_decLevelZeroTileSizeDegrees,
            m_intNumberLevels, m_intTextureSizePixels, m_strServerUrl, m_strDataSetName,
            m_strImageFileExtension, m_bOpacity, m_oWorld, m_strTextureDirectory,
            m_strCacheRoot, m_Parent);
      }

      protected override void CleanUpLayer(bool bFinal)
      {
         if (m_oQuadTileSet != null)
            m_oQuadTileSet.Dispose();
         if (m_oImageAccessor != null)
            m_oImageAccessor.Dispose();
         m_oImageTileService = null;
         m_oImageAccessor = null;
         m_oQuadTileSet = null;
         m_blnIsChanged = true;
      }
   }
}
