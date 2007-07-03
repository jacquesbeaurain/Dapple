using System;
using System.Globalization;
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

      public static readonly string CacheSubDir = "Images";

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
      string m_strCacheRoot;

      //QuadTileLayer
      int distAboveSurface = 0;
      bool terrainMapped = false;
      GeographicBoundingBox m_hBoundary = new GeographicBoundingBox(0, 0, 0, 0);
      ImageStore[] m_oImageStores = null;

      bool m_blnIsChanged = true;

      private QuadLayerBuilder(string name, int height, bool isTerrainMapped, GeographicBoundingBox boundary,
         ImageStore[] imageStores, byte opacity, World world, string cacheRoot, IBuilder parent)
      {
         m_strName = name;
         distAboveSurface = height;
         terrainMapped = isTerrainMapped;
         m_hBoundary = boundary;
         m_oImageStores = imageStores;
         m_bOpacity = opacity;
			m_strCacheRoot = String.Format("{0}{1}{2}", cacheRoot, Path.DirectorySeparatorChar, world.Name);
         m_oWorld = world;
         m_Parent = parent;
      }


		private static string getRenderablePathString(RenderableObject renderable)
		{
			if (renderable.ParentList == null)
			{
				return renderable.Name;
			}
			else
			{
				return getRenderablePathString(renderable.ParentList) + Path.DirectorySeparatorChar + renderable.Name;
			}
		}

      public QuadLayerBuilder(string name, int height, bool isTerrainMapped, GeographicBoundingBox boundary,
         double levelZeroTileSize, int levels, int textureSize, string serverURL, string dataSetName, string imageExtension,
         byte opacity, World world, string cacheRoot, IBuilder parent)
         : this(name, height, isTerrainMapped, boundary, null, opacity, world, cacheRoot, parent)
      {
         m_oImageStores = new ImageStore[1];
         m_oImageStores[0] = new NltImageStore(dataSetName, serverURL);
         m_oImageStores[0].DataDirectory = null;
         m_oImageStores[0].LevelZeroTileSizeDegrees = levelZeroTileSize;
         m_oImageStores[0].LevelCount = levels;
         m_oImageStores[0].ImageExtension = imageExtension;
         m_oImageStores[0].CacheDirectory = GetCachePath();
         m_oImageStores[0].TextureFormat = World.Settings.TextureFormat;
         m_oImageStores[0].TextureSizePixels = textureSize;
      }

      public override RenderableObject GetLayer()
      {
         return GetQuadLayer();
      }

      public QuadTileSet GetQuadLayer()
      {
         if (m_blnIsChanged)
         {
            m_oQuadTileSet = new QuadTileSet(m_strName,
                m_oWorld,
                distAboveSurface,
                m_hBoundary.North, m_hBoundary.South, m_hBoundary.West, m_hBoundary.East, terrainMapped, m_oImageStores);
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

      public virtual double LevelZeroTileSizeDegrees
      {
         get 
         {
            if (m_oImageStores != null && m_oImageStores.Length > 0 && m_oImageStores[0] != null)
               return m_oImageStores[0].LevelZeroTileSizeDegrees;
            else
               return 22.5;
         }
         set
         {
            // JBTODO: Shouldn't we just allow this for DAP and WMS?
            if (m_oImageStores != null && m_oImageStores.Length > 0 && m_oImageStores[0] != null)
            {
               m_blnIsChanged = true;
               m_oImageStores[0].LevelZeroTileSizeDegrees = value;
            }
         }
      }

      public virtual int LevelCount
      {
         get 
         {
            if (m_oImageStores != null && m_oImageStores.Length > 0 && m_oImageStores[0] != null)
               return m_oImageStores[0].LevelCount;
            else
               return 15;
         }
         set
         {
            // JBTODO: Shouldn't we just allow this for DAP and WMS?
            if (m_oImageStores != null && m_oImageStores.Length > 0 && m_oImageStores[0] != null)
            {
               m_blnIsChanged = true;
               m_oImageStores[0].LevelCount = value;
            }
         }
      }

      #endregion

      #region ImageBuilder Members

      public override GeographicBoundingBox Extents
      {
         get { return m_hBoundary; }
      }

      public override int TextureSizePixels
      {
         get 
         {
            if (m_oImageStores != null && m_oImageStores.Length > 0 && m_oImageStores[0] != null)
               return m_oImageStores[0].TextureSizePixels;
            else
               return 15;
         }
         set
         {
            // JBTODO: Shouldn't we just allow this for DAP and WMS?
            if (m_oImageStores != null && m_oImageStores.Length > 0 && m_oImageStores[0] != null)
            {
               m_blnIsChanged = true;
               m_oImageStores[0].TextureSizePixels = value;
            }
         }
      }

      #endregion

      public override string ServiceType
      {
         get { return "Tile Server"; }
      }

      public override string GetURI()
      {
         if (m_oImageStores != null && m_oImageStores.Length > 0 && m_oImageStores[0] != null && m_oImageStores[0] is NltImageStore)
         {
            NltImageStore nltStore = m_oImageStores[0] as NltImageStore;
            string formatString = "{0}?datasetname={1}&name={2}&height={3}&north={4}&east={5}&south={6}&west={7}&size={8}&levels={9}&lvl0tilesize={10}&terrainMapped={11}&imgfileext={12}";

            return string.Format(CultureInfo.InvariantCulture, formatString,
               nltStore.ServerUri.Replace("http://", URLProtocolName),
               nltStore.DatasetName,
               System.Web.HttpUtility.UrlEncode(m_strName),
               distAboveSurface.ToString(),
               m_hBoundary.North,
               m_hBoundary.East,
               m_hBoundary.South,
               m_hBoundary.West,
               nltStore.TextureSizePixels,
               nltStore.LevelCount,
               nltStore.LevelZeroTileSizeDegrees,
               terrainMapped,
               nltStore.ImageExtension);
         }
         else
            return "";
      }

		private static string GetBuilderPathString(IBuilder builder)
		{
			if(builder.Parent == null)
			{
				string strRet = builder.Name;
				foreach (char cInvalid in Path.GetInvalidPathChars())
					strRet.Replace(cInvalid, '_');
				return strRet;
			}
			else
			{
				return GetBuilderPathString(builder.Parent) + Path.DirectorySeparatorChar + builder.Name;
			}
		}

      public override string GetCachePath()
      {
			return String.Format("{0}{1}{2}{1}{3}", m_strCacheRoot, Path.DirectorySeparatorChar, CacheSubDir, GetBuilderPathString(this));
      }

      public static QuadLayerBuilder GetQuadLayerBuilderFromURI(string uri, string cacheRoot, World world, IBuilder parent)
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
            double lvl0tilesize = Convert.ToDouble(pairs[9].Substring(pairs[9].IndexOf('=') + 1), System.Globalization.CultureInfo.InvariantCulture);
            bool terrainMapped = Convert.ToBoolean(pairs[10].Substring(pairs[10].IndexOf('=') + 1));
            string fileExt = pairs[11].Substring(pairs[11].IndexOf('=') + 1);

            return new QuadLayerBuilder(name.Replace('+', ' '), height, terrainMapped, new GeographicBoundingBox(north, south, west, east),
               lvl0tilesize, levels, size, serverUrl, datasetname, fileExt, 255,
               world, cacheRoot, parent);
         }
         catch { }

         return null;
      }

      public override object Clone()
      {
         return new QuadLayerBuilder(m_strName, distAboveSurface, terrainMapped, m_hBoundary, m_oImageStores, Opacity, m_oWorld, m_strCacheRoot, m_Parent);
      }

      protected override void CleanUpLayer(bool bFinal)
      {
         if (m_oQuadTileSet != null)
            m_oQuadTileSet.Dispose();
         m_oImageStores = null;
         m_oQuadTileSet = null;
         m_blnIsChanged = true;
      }
   }
}
