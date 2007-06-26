//----------------------------------------------------------------------------
// NAME: Virtual Earth Plugin
// VERSION: 1.0
// DESCRIPTION: maps provided by Microsoft at http://local.live.com/
// DEVELOPER: casey chesnut
// WEBSITE: http://www.brains-N-brawn.com/veWorldWind/
// REFERENCES: 
//----------------------------------------------------------------------------

using System;
using System.IO;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

using System.Runtime.InteropServices;
using System.Net;
using System.Threading;
using System.Text;
using System.Text.RegularExpressions;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

using WorldWind;
using WorldWind.Net;
using WorldWind.Renderable;
using WorldWind.Terrain;

namespace bNb.Plugins_GD
{
   #region VEPROJECTTILESLAYER
   public class VeReprojectTilesLayer : RenderableObject
   {
      private bool disposed = false;
      private Projection proj;
      private WorldWindow worldWindow;

      private static double earthRadius; //6378137;
      private static double earthCircum; //40075016.685578488
      private static double earthHalfCirc; //20037508.

      private const int pixelsPerTile = 256;
      private int prevRow = -1;
      private int prevCol = -1;
      private int prevLvl = -1;
      private float prevVe = -1;

      private ArrayList veTiles = new ArrayList();

      private string datasetName;
      private string imageExtension;
      private int startZoomLevel;
      private string cacheDirectory;

      public VeReprojectTilesLayer(string name, WorldWindow worldWindow, string datasetName, string imageExtension, int startZoomLevel, string cache)
         : base(name)
      {
         this.name = name;
         this.worldWindow = worldWindow;
         this.datasetName = datasetName;
         this.imageExtension = imageExtension;
         this.startZoomLevel = startZoomLevel;
         this.cacheDirectory = cache;
      }

      private Sprite sprite;
      private Texture spriteTexture;
      int iconWidth = 128;
      int iconHeight = 128;
      Rectangle spriteSize;

      /*
      private static int badTileSize = -1;
      public static int BadTileSize
      {
         get{return badTileSize;}
      }

      private static byte [] badTileBytes;
      public static bool IsBadTile(MemoryStream newTile)
      {
         byte [] newTileBuffer = new byte[badTileBytes.Length];
         newTile.Position = badTileSize / 2;
         newTile.Read(newTileBuffer, 0, newTileBuffer.Length);
         bool isBad = true;
         for(int i=0; i<badTileBytes.Length; i++)
         {
            byte badByte = badTileBytes[i];
            byte newByte = newTileBuffer[i];
            if(badByte != newByte)
            {
               isBad = false;
               break;
            }
         }
         newTile.Position = 0;
         return isBad;
      }
      */

      public override byte Opacity
      {
         get
         {
            return base.Opacity;
         }
         set
         {
            foreach (VeTile tile in veTiles.Clone() as ArrayList)
            {
               for (int i = 0; i < tile.vertices.Length; i++)
               {
                  tile.vertices[i].Color = value << 24;
               }
            }
            base._opacity = value;
         }
      }

      /// <summary>
      /// Layer initialization code
      /// </summary>
      public override void Initialize(DrawArgs drawArgs)
      {
         try
         {
            if (this.isInitialized == true)
            {
               return;
            }

            //init the sprite for PushPins
            sprite = new Sprite(drawArgs.device);
            //#if DEBUG
            string spritePath = Path.Combine(worldWindow.WorldWindSettings.WorldWindDirectory, "Plugins\\VirtualEarth\\VirtualEarthPushPin.png");
            //#else
            //				string spritePath = VirtualEarthPlugin.PluginDir + @"\VirtualEarthPushPin.png";
            //
            //#endif
            if (File.Exists(spritePath) == false)
            {
               Utility.Log.Write("spritePath not found " + spritePath);
            }
            spriteSize = new Rectangle(0, 0, iconWidth, iconHeight);
            spriteTexture = TextureLoader.FromFile(drawArgs.device, spritePath);

            earthRadius = worldWindow.CurrentWorld.EquatorialRadius;
            earthCircum = earthRadius * 2.0 * Math.PI; //40075016.685578488
            earthHalfCirc = earthCircum / 2; //20037508.

            //NOTE tiles did not line up properly with ellps=WGS84
            //string [] projectionParameters = new string[]{"proj=merc", "ellps=WGS84", "no.defs"};
            //+proj=longlat +ellps=sphere +a=6370997.0 +es=0.0
            string[] projectionParameters = new string[] { "proj=merc", "ellps=sphere", "a=" + earthRadius.ToString(), "es=0.0", "no.defs" };
            proj = new Projection(projectionParameters);

            //static
            VeTile.Init(this.proj, worldWindow.CurrentWorld.TerrainAccessor, worldWindow.CurrentWorld.EquatorialRadius);

            prevVe = World.Settings.VerticalExaggeration;

            this.isInitialized = true;
         }
         catch (Exception ex)
         {
            Utility.Log.Write(ex);
            throw;
         }
      }

      protected override void FreeResources()
      {
         RemoveAllTiles();
         ForceRefresh();
      }
      
      public string GetLocalLiveLink()
      {
         //http://local.live.com/default.aspx?v=2&cp=43.057723~-88.404224&style=r&lvl=12
         string lat = worldWindow.DrawArgs.WorldCamera.Latitude.Degrees.ToString("###.#####");
         string lon = worldWindow.DrawArgs.WorldCamera.Longitude.Degrees.ToString("###.#####");
         string link = "http://local.live.com/default.aspx?v=2&cp=" + lat + "~" + lon + "&styles=" + datasetName + "&lvl=" + prevLvl.ToString();
         return link;
      }

      public void RemoveAllTiles()
      {
         lock (veTiles.SyncRoot)
         {
            for (int i = 0; i < veTiles.Count; i++)
            {
               VeTile veTile = (VeTile)veTiles[i];
               veTile.Dispose();
               veTiles.RemoveAt(i);
            }
            veTiles.Clear();
         }
      }

      public void ForceRefresh()
      {
         prevRow = -1;
         prevCol = -1;
         prevLvl = -1;
      }

      /// <summary>
      /// Update layer (called from worker thread)
      /// </summary>
      public override void Update(DrawArgs drawArgs)
      {
         try
         {
            if (this.isOn == false)
            {
               return;
            }

            //NOTE for some reason Initialize is not getting called from the Plugin Menu Load/Unload
            //it does get called when the plugin loads from Startup
            //not sure what is going on, so i'll just call it manually
            if (this.isInitialized == false)
            {
               this.Initialize(drawArgs);
               return;
            }

            //get lat, lon
            double lat = drawArgs.WorldCamera.Latitude.Degrees;
            double lon = drawArgs.WorldCamera.Longitude.Degrees;
            //determine zoom level
            double alt = drawArgs.WorldCamera.Altitude;
            //could go off distance, but this changes when view angle changes
            //Angle fov = drawArgs.WorldCamera.Fov; //stays at 45 degress
            //Angle viewRange = drawArgs.WorldCamera.ViewRange; //off of distance, same as TVR but changes when view angle changes
            Angle tvr = drawArgs.WorldCamera.TrueViewRange; //off of altitude
            //smallest altitude = 100m
            //tvr = .00179663198575926
            //start altitude = 12756273m
            //tvr = 180

            //WW _levelZeroTileSizeDegrees
            //180 90 45 22.5 11.25 5.625 2.8125 1.40625 .703125 .3515625 .17578125 .087890625 0.0439453125 0.02197265625 0.010986328125 0.0054931640625
            int zoomLevel = GetZoomLevelByTrueViewRange(tvr.Degrees);
            //dont start VE tiles until a certain zoom level
            if (zoomLevel < startZoomLevel)
            {
               this.RemoveAllTiles();
               return;
            }

            //WW tiles
            //double tileDegrees = GetLevelDegrees(zoomLevel);
            //int row = MathEngine.GetRowFromLatitude(lat, tileDegrees);
            //int col = MathEngine.GetColFromLongitude(lon, tileDegrees);

            //VE tiles
            double metersY;
            double yMeters;
            int yMetersPerPixel;
            int row;
            /*
            //WRONG - doesn't stay centered away from equator
            //int yMeters = LatitudeToYAtZoom(lat, zoomLevel); //1024
            double sinLat = Math.Sin(DegToRad(lat));
            metersY = earthRadius / 2 * Math.Log((1 + sinLat) / (1 - sinLat)); //0
            yMeters = earthHalfCirc - metersY; //20037508.342789244
            yMetersPerPixel = (int) Math.Round(yMeters / MetersPerPixel(zoomLevel));
            row = yMetersPerPixel / pixelsPerTile;
            */
            //CORRECT
            //int xMeters = LongitudeToXAtZoom(lon, zoomLevel); //1024
            double metersX = earthRadius * DegToRad(lon); //0
            double xMeters = earthHalfCirc + metersX; //20037508.342789244
            int xMetersPerPixel = (int)Math.Round(xMeters / MetersPerPixel(zoomLevel));
            int col = xMetersPerPixel / pixelsPerTile;

            //reproject - overrides row above
            //this correctly keeps me on the current tile that is being viewed
            UV uvCurrent = new UV(DegToRad(lon), DegToRad(lat));
            uvCurrent = proj.Forward(uvCurrent);
            metersY = uvCurrent.V;
            yMeters = earthHalfCirc - metersY;
            yMetersPerPixel = (int)Math.Round(yMeters / MetersPerPixel(zoomLevel));
            row = yMetersPerPixel / pixelsPerTile;

            //update mesh if VertEx changes
            if (prevVe != World.Settings.VerticalExaggeration)
            {
               lock (veTiles.SyncRoot)
               {
                  VeTile veTile;
                  for (int i = 0; i < veTiles.Count; i++)
                  {
                     veTile = (VeTile)veTiles[i];
                     if (veTile.VertEx != World.Settings.VerticalExaggeration)
                     {
                        veTile.CreateMesh(this.Opacity, World.Settings.VerticalExaggeration);
                     }
                  }
               }
            }
            prevVe = World.Settings.VerticalExaggeration;

            //if within previous bounds and same zoom level, then exit
            if (row == prevRow && col == prevCol && zoomLevel == prevLvl)
            {
               return;
            }

            //System.Diagnostics.Debug.WriteLine("CHANGE");

            lock (veTiles.SyncRoot)
            {
               VeTile veTile;
               for (int i = 0; i < veTiles.Count; i++)
               {
                  veTile = (VeTile)veTiles[i];
                  veTile.IsNeeded = false;
               }
            }

            //metadata
            ArrayList alMetadata = null;
            //if (veForm.IsDebug == true)
            //{
            //   alMetadata = new ArrayList();
            //   alMetadata.Add("yMeters " + yMeters.ToString());
            //   alMetadata.Add("metersY " + metersY.ToString());
            //   alMetadata.Add("yMeters2 " + yMeters.ToString());
            //   alMetadata.Add("vLat " + uvCurrent.V.ToString());
            //   //alMetadata.Add("xMeters " + xMeters.ToString());
            //   //alMetadata.Add("metersX " + metersX.ToString());
            //   //alMetadata.Add("uLon " + uvCurrent.U.ToString());
            //}

            //add current tiles first
            AddVeTile(drawArgs, row, col, zoomLevel, alMetadata);
            //then add other tiles outwards in surrounding circles
            AddNeighborTiles(drawArgs, row, col, zoomLevel, null, 1);
            AddNeighborTiles(drawArgs, row, col, zoomLevel, null, 2);
            AddNeighborTiles(drawArgs, row, col, zoomLevel, null, 3);

            //if(prevLvl > zoomLevel) //zooming out
            //{
            //}			

            lock (veTiles.SyncRoot)
            {
               VeTile veTile;
               for (int i = 0; i < veTiles.Count; i++)
               {
                  veTile = (VeTile)veTiles[i];
                  if (veTile.IsNeeded == false)
                  {
                     veTile.Dispose();
                     veTiles.RemoveAt(i);
                  }
               }
            }

            prevRow = row;
            prevCol = col;
            prevLvl = zoomLevel;
         }
         catch (Exception ex)
         {
            Utility.Log.Write(ex);
         }
      }

      public bool bIsDownloading(out int iBytesRead, out int iTotalBytes)
      {
         iBytesRead = 0;
         iTotalBytes = 0;
         bool bIsdownloading = false;

         if (veTiles != null && veTiles.Count > 0)
         {
            lock (veTiles.SyncRoot)
            {
               foreach (VeTile veTile in veTiles)
               {
                  if (veTile.Texture == null)
                  {
                     iBytesRead += veTile.iDownloadPos;
                     iTotalBytes += veTile.iDownloadTotal;
                     bIsdownloading = true;
                  }
               }
            }
         }

         return bIsdownloading;
      }

      private void AddNeighborTiles(DrawArgs drawArgs, int row, int col, int zoomLevel, ArrayList alMetadata, int range)
      {
         int minRow = row - range;
         int maxRow = row + range;
         int minCol = col - range;
         int maxCol = col + range;
         for (int i = minRow; i <= maxRow; i++)
         {
            for (int j = minCol; j <= maxCol; j++)
            {
               //only outer edges, inner tiles should already be added
               if (i == minRow || i == maxRow || j == minCol || j == maxCol)
               {
                  AddVeTile(drawArgs, i, j, zoomLevel, alMetadata);
               }
            }
         }
      }

      private void AddVeTile(DrawArgs drawArgs, int row, int col, int zoomLevel, ArrayList alMetadata)
      {
         //TODO handle column wrap-around
         //haven't had to explicitly handle this yet

         bool tileFound = false;
         lock (veTiles.SyncRoot)
         {
            foreach (VeTile veTile in veTiles)
            {
               if (veTile.IsNeeded == true)
               {
                  continue;
               }
               if (veTile.IsEqual(row, col, zoomLevel) == true)
               {
                  veTile.IsNeeded = true;
                  tileFound = true;
                  break;
               }
            }
         }
         if (tileFound == false)
         {
            //exit if zoom level has changed
            int curZoomLevel = GetZoomLevelByTrueViewRange(drawArgs.WorldCamera.TrueViewRange.Degrees);
            if (curZoomLevel != zoomLevel)
            {
               return;
            }
            VeTile newVeTile = CreateVeTile(drawArgs, row, col, zoomLevel, alMetadata);
            newVeTile.IsNeeded = true;
            lock (veTiles.SyncRoot)
            {
               veTiles.Add(newVeTile);
            }
         }
      }

      private VeTile CreateVeTile(DrawArgs drawArgs, int row, int col, int zoomLevel, ArrayList alMetadata)
      {
         VeTile newVeTile = new VeTile(row, col, zoomLevel);

         //metadata
         if (alMetadata != null)
         {
            foreach (string metadata in alMetadata)
            {
               newVeTile.AddMetaData(metadata);
            }
         }

         //thread to download new tile(s) or just load from cache
         newVeTile.GetTexture(drawArgs, pixelsPerTile, datasetName, imageExtension, cacheDirectory);

         //handle the diff projection
         double metersPerPixel = MetersPerPixel(zoomLevel);
         double totalTilesPerEdge = Math.Pow(2, zoomLevel);
         double totalMeters = totalTilesPerEdge * pixelsPerTile * metersPerPixel;
         double halfMeters = totalMeters / 2;

         //do meters calculation in VE space
         //the 0,0 origin for VE is in upper left
         double N = row * (pixelsPerTile * metersPerPixel);
         double W = col * (pixelsPerTile * metersPerPixel);
         //now convert it to +/- meter coordinates for Proj.4
         //the 0,0 origin for Proj.4 is 0 lat, 0 lon
         //-22 to 22 million, -11 to 11 million
         N = halfMeters - N;
         W = W - halfMeters;
         double E = W + (pixelsPerTile * metersPerPixel);
         double S = N - (pixelsPerTile * metersPerPixel);

         newVeTile.UL = new UV(W, N);
         newVeTile.UR = new UV(E, N);
         newVeTile.LL = new UV(W, S);
         newVeTile.LR = new UV(E, S);

         //create mesh
         byte opacity = this.Opacity; //from RenderableObject
         float verticalExaggeration = World.Settings.VerticalExaggeration;
         newVeTile.CreateMesh(opacity, verticalExaggeration);
         newVeTile.CreateDownloadRectangle(drawArgs, World.Settings.DownloadProgressColor.ToArgb());

         return newVeTile;
      }

/*      private static double MetersPerTile(int zoom)
      {
         return MetersPerPixel(zoom) * pixelsPerTile;
      }
      */
      private static double MetersPerPixel(int zoom)
      {
         double arc;
         arc = earthCircum / ((1 << zoom) * pixelsPerTile);
         return arc;
      }

      private static double DegToRad(double d)
      {
         return d * Math.PI / 180.0;
      }

/*      private static double RadToDeg(double d)
      {
         return d * 180 / Math.PI;
      }
      */
      public double GetLevelDegrees(int level)
      {
         double metersPerPixel = MetersPerPixel(level);
         double arcDistance = metersPerPixel * pixelsPerTile;
         //double arcDistance = earthCircum * (tileRange / 360);
         double tileRange = (arcDistance / earthCircum) * 360;
         return tileRange;
      }

      public int GetZoomLevelByTrueViewRange(double trueViewRange)
      {
         int maxLevel = 3;
         int minLevel = 19;
         int numLevels = minLevel - maxLevel + 1;
         int retLevel = maxLevel;
         for (int i = 0; i < numLevels; i++)
         {
            retLevel = i + maxLevel;

            double viewAngle = 180;
            for (int j = 0; j < i; j++)
            {
               viewAngle = viewAngle / 2.0;
            }
            if (trueViewRange >= viewAngle)
            {
               break;
            }
         }
         return retLevel;
      }

      public int GetZoomLevelByArcDistance(double arcDistance)
      {
         //arcDistance in meters
         int totalLevels = 24;
         int level = 0;
         for (level = 1; level <= totalLevels; level++)
         {
            double metersPerPixel = MetersPerPixel(level);
            double totalDistance = metersPerPixel * pixelsPerTile;
            if (arcDistance > totalDistance)
            {
               break;
            }
         }
         return level - 1;
      }

      /*private int LatitudeToYAtZoom(double lat, int zoom)
      {
         int y;
         //code VE Mobile v1 - NO LONGER VALID
         //double sinLat = Math.Sin(DegToRad(lat));
         //double metersY = 6378137 / 2 * Math.Log((1 + sinLat) / (1 - sinLat));
         //y = (int)Math.Round((20971520 - metersY) / MetersPerPixel(zoom));
         //forum - SKIPS TILES THE FURTHER YOU GET FROM EQUATOR
         double arc = earthCircum / ((1 << zoom) * pixelsPerTile);
         double sinLat = Math.Sin(DegToRad(lat));
         double metersY = earthRadius / 2 * Math.Log((1 + sinLat) / (1 - sinLat));
         y = (int)Math.Round((earthHalfCirc - metersY) / arc);
         //HACK - THIS HANDLES THE SKIPPING OF TILES THE FURTHER YOU GET FROM EQUATOR
         //double arc = earthCircum / ((1 << zoom) * pixelsPerTile);
         //double metersY = earthRadius * DegToRad(lat);
         //y = (int) Math.Round((earthHalfCirc - metersY) / arc);
         return y;
      }

      private int LongitudeToXAtZoom(double lon, int zoom)
      {
         int x;
         double arc = earthCircum / ((1 << zoom) * pixelsPerTile);
         double metersX = earthRadius * DegToRad(lon);
         x = (int)Math.Round((earthHalfCirc + metersX) / arc);
         return x;
      }*/

      /// <summary>
      /// Draws the layer
      /// </summary>
      public override void Render(DrawArgs drawArgs)
      {
         try
         {
            if (this.isOn == false)
            {
               return;
            }

            if (this.isInitialized == false)
            {
               return;
            }

            if (drawArgs.device == null)
               return;

            if (veTiles != null && veTiles.Count > 0)
            {
               //render mesh and tile(s)
               bool disableZBuffer = false; //TODO where do i get this setting
               //foreach(VeTile veTile in veTiles)
               //{
               //	veTile.Render(drawArgs, disableZBuffer);
               //}
               VeTile.Render(drawArgs, disableZBuffer, veTiles);
            }
         }
         catch (Exception ex)
         {
            Utility.Log.Write(ex);
         }
      }

      public void GetViewPort(DrawArgs drawArgs, out double lat1, out double lon1, out double lat2, out double lon2)
      {
         double halfViewRange = drawArgs.WorldCamera.TrueViewRange.Degrees / 2;
         double lat = drawArgs.WorldCamera.Latitude.Degrees;
         double lon = drawArgs.WorldCamera.Longitude.Degrees;
         lat1 = lat + halfViewRange;
         lon1 = lon + halfViewRange;
         lat2 = lat - halfViewRange;
         lon2 = lon - halfViewRange;
      }

      #region IDisposable Implementation
      protected virtual void Dispose(bool disposing)
      {
         if (!disposed)
         {
            // Dispose of resources held by this instance.
            RemoveAllTiles();

            if (sprite != null)
            {
               sprite.Dispose();
               sprite = null;
            }

            if (proj != null)
            {
               proj.Dispose();
               proj = null;
            }

            disposed = true;
            // Suppress finalization of this disposed instance.
            if (disposing)
            {
               GC.SuppressFinalize(this);
            }
         }
      }

      public sealed override void Dispose()
      {
         Dispose(true);
      }

      ~VeReprojectTilesLayer()
      {
         Dispose(false);
      }
      #endregion


      /// <summary>
      /// Handle mouse click
      /// </summary>
      /// <returns>true if click was handled.</returns>
      public override bool PerformSelectionAction(DrawArgs drawArgs)
      {
         return false;
      }
   }
   #endregion

   #region VETILE
   public class VeTile : IDisposable
   {
      private bool disposed = false;

      //these are the coordinate extents for the tile
      UV m_ul, m_ur, m_ll, m_lr;

      /// <summary>
      /// Coordinates at upper left edge of image
      /// </summary>
      public UV UL
      {
         get { return m_ul; }
         set { m_ul = value; }
      }

      /// <summary>
      /// Coordinates at upper right edge of image
      /// </summary>
      public UV UR
      {
         get { return m_ur; }
         set { m_ur = value; }
      }

      /// <summary>
      /// Coordinates at lower left edge of image
      /// </summary>
      public UV LL
      {
         get { return m_ll; }
         set { m_ll = value; }
      }

      /// <summary>
      /// Coordinates at lower right edge of image
      /// </summary>
      public UV LR
      {
         get { return m_lr; }
         set { m_lr = value; }
      }

      //store the Vertical Exaggeration for when the mesh was created
      //so when the VerticalExaggeration setting changes, it know which meshes to recreate
      private float vertEx;
      public float VertEx
      {
         get { return vertEx; }
      }

      private static Projection _proj;
      private static double _layerRadius;
      private static TerrainAccessor _terrainAccessor;
      private static System.Drawing.Font _font;
      private static Brush _brush;
      //private static VirtualEarthForm _veForm;

      public static void Init(Projection proj, TerrainAccessor terrainAccessor, double layerRadius)//, VirtualEarthForm veForm)
      {
         _proj = proj;
         _terrainAccessor = terrainAccessor;
         _layerRadius = layerRadius;
         //_veForm = veForm;
         _font = new System.Drawing.Font("Verdana", 15, FontStyle.Bold);
         _brush = new SolidBrush(Color.Green);
      }

      //flag for if the tile should be disposed
      private bool isNeeded = true;
      public bool IsNeeded
      {
         get { return isNeeded; }
         set { isNeeded = value; }
      }

      public bool IsEqual(int row, int col, int level)
      {
         bool retVal = false;
         if (this.row == row && this.col == col && this.level == level)
         {
            retVal = true;
         }
         return retVal;
      }

      private int row;
      private int col;
      private int level;

      public VeTile(int row, int col, int level)
      {
         this.row = row;
         this.col = col;
         this.level = level;
      }

      private Texture texture = null;
      public Texture Texture
      {
         get { return texture; }
         set { texture = value; }
      }

      private ArrayList alMetaData = new ArrayList();
      private WebDownload download;
      public int iDownloadPos = 0;
      public int iDownloadTotal = 2048;
      private string textureName;
      private string datasetName;
      private string cacheDirectory;
      private DrawArgs drawArgs;

      public void GetTexture(DrawArgs drawArgs, int pixelsPerTile, string _datasetName, string imageExtension, string _cacheDirectory)
      {
         this.drawArgs = drawArgs;
         this.datasetName = _datasetName;
         this.cacheDirectory = _cacheDirectory;
         string _imageExtension = imageExtension;
         string _serverUri = ".ortho.tiles.virtualearth.net/tiles/";

         string quadKey = TileToQuadKey(col, row, level);

         //TODO no clue what ?g= is
         string textureUrl = String.Concat(new object[] { "http://", _datasetName, quadKey[quadKey.Length - 1], _serverUri, _datasetName, quadKey, ".", _imageExtension, "?g=", 15 });

         //if (_veForm.IsDebug == true)
         //{
         //   //generate a DEBUG tile with metadata
         //   MemoryStream ms;
         //   //debug
         //   Bitmap b = new Bitmap(pixelsPerTile, pixelsPerTile);
         //   System.Drawing.Imaging.ImageFormat imageFormat;
         //   //could download on my own from here and add metadata to the images before storing to cache
         //   //Bitmap b = DownloadImage(url);
         //   //string levelDir = CreateLevelDir(level);
         //   //string rowDir = CreateRowDir(levelDir, row);
         //   //alMetaData.Add("wwLevel : " + level.ToString());
         //   alMetaData.Add("ww rowXcol : " + row.ToString() + "x" + col.ToString());
         //   //alMetaData.Add("wwArcDist : " + arcDistance.ToString());
         //   //alMetaData.Add("tileRange : " + tileRange.ToString());
         //   //alMetaData.Add("latXlon : " + lat.ToString("###.###") + "x" + lon.ToString("###.###"));
         //   //alMetaData.Add("lat : " + lat.ToString());
         //   //alMetaData.Add("lon : " + lon.ToString());
         //   alMetaData.Add("veLevel : " + level.ToString());
         //   //alMetaData.Add("ve rowXcol : " + t_x.ToString() + "x" + t_y.ToString());
         //   //alMetaData.Add("veArcDist : " + tileDistance.ToString());
         //   //alMetaData.Add("sinLat : " + sinLat.ToString());
         //   alMetaData.Add("quadKey " + quadKey.ToString());
         //   imageFormat = System.Drawing.Imaging.ImageFormat.Jpeg;
         //   b = DecorateBitmap(b, _font, _brush, alMetaData);
         //   //SaveBitmap(b, rowDir, row, col, _imageExtension, b.RawFormat); //, System.Drawing.Imaging.ImageFormat.Jpeg
         //   //url = String.Empty;
         //   ms = new MemoryStream();
         //   b.Save(ms, imageFormat);
         //   ms.Position = 0;
         //   this.texture = TextureLoader.FromStream(drawArgs.device, ms);
         //   ms.Close();
         //   ms = null;
         //   b.Dispose();
         //   b = null;
         //}
         //else
         //{
         //load a tile from file OR download it if not cached
         string levelDir = CreateLevelDir(level, cacheDirectory);
         string rowDir = CreateRowDir(levelDir, row);
         textureName = String.Empty; //= GetTextureName(rowDir, row, col, "dds");
         if (_datasetName == "r")
         {
            textureName = GetTextureName(rowDir, row, col, "png");
         }
         else
         {
            textureName = GetTextureName(rowDir, row, col, "jpeg");
         }
         if (File.Exists(textureName) == true)
         {
            this.texture = TextureLoader.FromFile(drawArgs.device, textureName);
         }
         else //download it
         {
            //"http://h1.ortho.tiles.virtualearth.net/tiles/h03022221001.jpeg?g=2"
            //"http://h0.ortho.tiles.virtualearth.net/tiles/h300..jpeg?g=15"
            download = new WebDownload(textureUrl, false);
            download.DownloadType = DownloadType.Unspecified;
            download.SavedFilePath = textureName + ".tmp"; //?
            download.ProgressCallback += new DownloadProgressHandler(UpdateProgress);
            download.CompleteCallback += new DownloadCompleteHandler(DownloadComplete);
            download.BackgroundDownloadMemory();
         }
      }

      void UpdateProgress(int pos, int total)
      {
         iDownloadPos = pos;

         if (total < pos)
         {
            // Provide rolling progress (starts with 2048 estimate)
            while (iDownloadTotal < pos)
               iDownloadTotal *= 2;
         }
         else
            iDownloadTotal = total;
      }

      private void DownloadComplete(WebDownload downloadInfo)
      {
         try
         {
            downloadInfo.Verify();

            //m_quadTile.QuadTileArgs.NumberRetries = 0;

            //TODO add back in logic to check for the no data tile?
            //the logic was to not display that tile to at least show some data for the layers beneath VE
            //or show the no data tile so they know that VE has not covered that area yet
            //then just let those tiles get periodically deleted from cache for when VE updates

            // Rename temp file to real name
            File.Delete(textureName);

            if (download.ContentStream.Length == 1033)
               /*{
                  int parentlevel = this.level - 1;
                  int parentrow = (int) Math.Round((double)this.row / 2.0);
                  int parentcol = (int) Math.Round((double)this.col / 2.0);
                  //parentrow = this.row / 2;
                  //parentcol = this.col / 2;
               
                  string levelDir;
                  string mapTypeDir;
                  string rowDir;

                  textureName = String.Empty;
                  while (parentlevel >= 0 && (textureName == String.Empty || !File.Exists(textureName)))
                  {
                     levelDir = CreateLevelDir(parentlevel, this.cacheDirectory);
                     mapTypeDir = CreateMapTypeDir(levelDir, this.datasetName);
                     rowDir = CreateRowDir(mapTypeDir, parentrow);

                     if (datasetName == "r")
                     {
                        textureName = GetTextureName(rowDir, parentrow, parentcol, "png");
                     }
                     else
                     {
                        textureName = GetTextureName(rowDir, parentrow, parentcol, "jpeg");
                     }
                     parentlevel = parentlevel - 1;
                     parentrow = (int)Math.Round((double)parentrow / 2.0);
                     parentcol = (int)Math.Round((double)parentcol / 2.0);
                     //parentrow = parentrow / 2;
                     //parentcol = parentcol / 2;
                  }
                  if (textureName != String.Empty && File.Exists(textureName))
                  {
                     using (Image parentImage = Image.FromFile(textureName))
                     {
                        int ndiv, childwidth, childheight;
                     
                        ndiv = (int) Math.Pow(2.0, (this.level - parentlevel));
                        childwidth = parentImage.Width / ndiv;
                        childheight = parentImage.Height / ndiv;

                        if (childwidth > 0 && childheight > 0)
                        {
                           int childcol, childrow;

                           childcol = ndiv/2 - (parentcol * ndiv - this.col);
                           childrow = ndiv/2 - (parentrow * ndiv - this.row);
                           //childcol = ndiv / 2 - (this.col - parentcol * ndiv);
                           //childrow = ndiv / 2 - (this.row - parentrow * ndiv);


                           Bitmap constBitmap = new Bitmap(parentImage.Width / ndiv, parentImage.Height / ndiv);
                           using (Graphics gr = Graphics.FromImage(constBitmap))
                           {
                              Rectangle rc = new Rectangle(childcol * constBitmap.Width, childrow * constBitmap.Height, constBitmap.Width, constBitmap.Height);
                              gr.DrawImage(parentImage, 0, 0, rc, GraphicsUnit.Pixel);
                           }

                           using (MemoryStream constStream = new MemoryStream())
                           {
                              constBitmap.Save(constStream, System.Drawing.Imaging.ImageFormat.Png);
                              constStream.Seek(0, SeekOrigin.Begin);
                              this.texture = TextureLoader.FromStream(drawArgs.device, constStream);
                           }
                        }
                     }
                  }

                  if (this.texture == null)*/
               this.texture = TextureLoader.FromFile(drawArgs.device, "Plugins\\VirtualEarth\\nodata.png");
            //}
            else
            {
               downloadInfo.SaveMemoryDownloadToFile(textureName);
               this.texture = TextureLoader.FromFile(drawArgs.device, textureName);
            }

            //File.Move(downloadInfo.SavedFilePath, textureName);

            // Make the quad tile reload the new image
            //m_quadTile.DownloadRequest = null;
            //m_quadTile.isInitialized = false;

         }
         catch (System.Net.WebException caught)
         {
            System.Net.HttpWebResponse response = caught.Response as System.Net.HttpWebResponse;
            if (response != null && response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
               using (File.Create(textureName + ".txt"))
               { }
               return;
            }
            //m_quadTile.QuadTileArgs.NumberRetries++;
         }
         catch
         {
            using (File.Create(textureName + ".txt"))
            { }
            if (File.Exists(downloadInfo.SavedFilePath))
               File.Delete(downloadInfo.SavedFilePath);
         }
         finally
         {
            if (download != null)
               download.IsComplete = true;
            //m_quadTile.QuadTileArgs.RemoveFromDownloadQueue(this);
            //Immediately queue next download
            //m_quadTile.QuadTileArgs.ServiceDownloadQueue();
         }
      }

      public void AddMetaData(string metadata)
      {
         alMetaData.Add(metadata);
      }

      //for generating the debug bitmap
      public Bitmap DecorateBitmap(Bitmap b, System.Drawing.Font font, Brush brush, ArrayList alMetadata)
      {
         if (alMetadata.Count > 0)
         {
            //if(b.RawFormat == System.Drawing.Imaging.ImageFormat.Png)
            if (b.PixelFormat == System.Drawing.Imaging.PixelFormat.Format8bppIndexed)
            {
               MemoryStream ms = new MemoryStream();
               b.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
               b.Dispose();
               b = null;
               b = new Bitmap(256, 256);
               b = (Bitmap)Bitmap.FromStream(ms);
               ms.Close();
               ms = null;
            }
            Graphics g = Graphics.FromImage(b); //fails for png files
            g.Clear(Color.White);
            g.DrawLine(Pens.Red, 0, 0, b.Width, 0);
            g.DrawLine(Pens.Red, 0, 0, 0, b.Height);
            string s = (string)alMetadata[0];
            SizeF sizeF = g.MeasureString(s, font);
            for (int i = 0; i < alMetadata.Count; i++)
            {
               s = (string)alMetadata[i];
               int x = 0;
               int y = (int)(sizeF.Height * (i + 0));
               g.DrawString(s, font, brush, x, y);
            }
            g.Dispose();
         }
         return b;
      }

      //convert VE row, col, level into key for URL
      private static string TileToQuadKey(int tx, int ty, int zl)
      {
         string quad;
         quad = "";
         for (int i = zl; i > 0; i--)
         {
            int mask = 1 << (i - 1);
            int cell = 0;
            if ((tx & mask) != 0)
            {
               cell++;
            }
            if ((ty & mask) != 0)
            {
               cell += 2;
            }
            quad += cell;
         }
         return quad;
      }

      public string CreateLevelDir(int level, string cacheDirectoryRoot)
      {
         string levelDir = null;
         if (Directory.Exists(cacheDirectory) == false)
         {
            Directory.CreateDirectory(cacheDirectory);
         }
         levelDir = cacheDirectory + @"\" + level.ToString();
         if (Directory.Exists(levelDir) == false)
         {
            Directory.CreateDirectory(levelDir);
         }
         return levelDir;
      }

      public string CreateRowDir(string levelDir, int row)
      {
         string rowDir = levelDir + @"\" + row.ToString("0000");
         if (Directory.Exists(rowDir) == false)
         {
            Directory.CreateDirectory(rowDir);
         }
         return rowDir;
      }

      public string GetTextureName(string rowDir, int row, int col, string textureExtension)
      {
         string textureName = rowDir + @"\" + row.ToString("0000") + "_" + col.ToString("0000") + "." + textureExtension;
         return textureName;
      }

      public void SaveBitmap(Bitmap b, string rowDir, int row, int col, string imageExtension, System.Drawing.Imaging.ImageFormat format)
      {
         string bmpName = rowDir + @"\" + row.ToString("0000") + "_" + col.ToString("0000") + "." + imageExtension;
         b.Save(bmpName, format);
         //b.Save(bmpName); //, format
      }

      public void Reproject()
      {
         //TODO refactor from the VeLayer class
      }

      public CustomVertex.PositionColoredTextured[] vertices;
      public short[] indices;
      protected int meshPointCount = 64;

      private double North;
      private double South;
      private double West;
      private double East;

      //NOTE this is a mix from Mashi's Reproject and WW for terrain
      public void CreateMesh(byte opacity, float verticalExaggeration)
      {
         this.vertEx = verticalExaggeration;

         int opacityColor = System.Drawing.Color.FromArgb(opacity, 0, 0, 0).ToArgb();

         meshPointCount = 32; //64; //96 // How many vertices for each direction in mesh (total: n^2)
         vertices = new CustomVertex.PositionColoredTextured[meshPointCount * meshPointCount];

         int upperBound = meshPointCount - 1;
         float scaleFactor = (float)1 / upperBound;
         //using(Projection proj = new Projection(m_projectionParameters))
         //{
         double uStep = (UR.U - UL.U) / upperBound;
         double vStep = (UL.V - LL.V) / upperBound;
         UV curUnprojected = new UV(UL.U, UL.V);

         // figure out latrange (for terrain detail)
         UV geoUL = _proj.Inverse(m_ul);
         UV geoLR = _proj.Inverse(m_lr);
         double latRange = (geoUL.U - geoLR.U) * 180 / Math.PI;

         North = geoUL.V * 180 / Math.PI;
         South = geoLR.V * 180 / Math.PI;
         West = geoUL.U * 180 / Math.PI;
         East = geoLR.U * 180 / Math.PI;

         float meshBaseRadius = (float)_layerRadius;
         TerrainTile tile = null;
         if (_terrainAccessor != null)//&& _veForm.IsTerrainOn == true)
         {
            //does the +1 to help against tears between elevated tiles? - made it worse
            //TODO not sure how to fix the tear between tiles caused by elevation?
            tile = _terrainAccessor.GetElevationArray((float)North, (float)South, (float)West, (float)East, meshPointCount);

            /*
            // Calculate mesh base radius (bottom vertices)
            float minimumElevation = float.MaxValue;
            float maximumElevation = float.MinValue;
			
            // Find minimum elevation to account for possible bathymetry
            foreach(float _height in heightData)
            {
               if(_height < minimumElevation)
                  minimumElevation = _height;
               if(_height > maximumElevation)
                  maximumElevation = _height;
            }
            minimumElevation *= verticalExaggeration;
            maximumElevation *= verticalExaggeration;

            if(minimumElevation > maximumElevation)
            {
               // Compensate for negative vertical exaggeration
               float tmp = minimumElevation;
               minimumElevation = maximumElevation;
               maximumElevation = minimumElevation;
            }

            float overlap = 500 * verticalExaggeration; // 500m high tiles
			
            // Radius of mesh bottom grid
            meshBaseRadius = (float) _layerRadius + minimumElevation - overlap;
            */
         }

         UV geo;
         Point3d pos;
         double height = 0;
         for (int i = 0; i < meshPointCount; i++)
         {
            for (int j = 0; j < meshPointCount; j++)
            {
               geo = _proj.Inverse(curUnprojected);

               // Radians -> Degrees
               geo.U *= 180 / Math.PI;
               geo.V *= 180 / Math.PI;

               if (tile != null)
               {
                  //if (_veForm.IsTerrainOn == true)
                  //{
                  height = tile.ElevationData[i + j * meshPointCount] * verticalExaggeration;
                  //}
                  //else
                  //{
                  //   //original
                  //   height = verticalExaggeration * _terrainAccessor.GetElevationAt(geo.V, geo.U, upperBound / latRange);
                  //}
               }

               pos = MathEngine.SphericalToCartesian(
                  geo.V,
                  geo.U,
                  _layerRadius + height);

               vertices[i * meshPointCount + j].X = (float)pos.X;
               vertices[i * meshPointCount + j].Y = (float)pos.Y;
               vertices[i * meshPointCount + j].Z = (float)pos.Z;
               //double sinLat = Math.Sin(geo.V);
               //vertices[i*meshPointCount + j].Z = (float) (pos.Z * sinLat);

               vertices[i * meshPointCount + j].Tu = j * scaleFactor;
               vertices[i * meshPointCount + j].Tv = i * scaleFactor;
               vertices[i * meshPointCount + j].Color = opacityColor;
               curUnprojected.U += uStep;
            }
            curUnprojected.U = UL.U;
            curUnprojected.V -= vStep;
         }
         //}

         indices = new short[2 * upperBound * upperBound * 3];
         for (int i = 0; i < upperBound; i++)
         {
            for (int j = 0; j < upperBound; j++)
            {
               indices[(2 * 3 * i * upperBound) + 6 * j] = (short)(i * meshPointCount + j);
               indices[(2 * 3 * i * upperBound) + 6 * j + 1] = (short)((i + 1) * meshPointCount + j);
               indices[(2 * 3 * i * upperBound) + 6 * j + 2] = (short)(i * meshPointCount + j + 1);

               indices[(2 * 3 * i * upperBound) + 6 * j + 3] = (short)(i * meshPointCount + j + 1);
               indices[(2 * 3 * i * upperBound) + 6 * j + 4] = (short)((i + 1) * meshPointCount + j);
               indices[(2 * 3 * i * upperBound) + 6 * j + 5] = (short)((i + 1) * meshPointCount + j + 1);
            }
         }
      }

      #region IDisposable Implementation
      protected virtual void Dispose(bool disposing)
      {
         if (!disposed)
         {
            // Dispose of resources held by this instance.
            if (texture != null)
            {
               texture.Dispose();
               texture = null;
            }
            if (download != null)
            {
               download.Dispose();
               download = null;
            }
            if (vertices != null)
            {
               vertices = null;
            }
            if (indices != null)
            {
               indices = null;
            }
            if (downloadRectangle != null)
            {
               downloadRectangle = null;
            }
            disposed = true;

            // Suppress finalization of this disposed instance.
            if (disposing)
            {
               GC.SuppressFinalize(this);
            }
         }
      }

      public void Dispose()
      {
         Dispose(true);
      }

      ~VeTile()
      {
         Dispose(false);
      }
      #endregion

      CustomVertex.PositionColored[] downloadRectangle = new CustomVertex.PositionColored[5];

      public static void Render(DrawArgs drawArgs, bool disableZbuffer, ArrayList alVeTiles)
      {
         try
         {
            if (alVeTiles.Count <= 0)
               return;

            lock (alVeTiles.SyncRoot)
            {
               //setup device to render textures
               if (disableZbuffer)
               {
                  if (drawArgs.device.RenderState.ZBufferEnable)
                     drawArgs.device.RenderState.ZBufferEnable = false;
               }
               else
               {
                  if (!drawArgs.device.RenderState.ZBufferEnable)
                     drawArgs.device.RenderState.ZBufferEnable = true;
               }
               drawArgs.device.VertexFormat = CustomVertex.PositionColoredTextured.Format;
               drawArgs.device.TextureState[0].AlphaOperation = TextureOperation.Modulate;
               drawArgs.device.TextureState[0].ColorOperation = TextureOperation.Add;
               drawArgs.device.TextureState[0].AlphaArgument1 = TextureArgument.TextureColor;

               //save index to tiles not downloaded yet
               int notDownloadedIter = 0;
               int[] notDownloaded = new int[alVeTiles.Count];

               int zoomLevel = GetZoomLevelByTrueViewRange(drawArgs.WorldCamera.TrueViewRange.Degrees);

               //render tiles that are downloaded
               VeTile veTile;
               for (int i = 0; i < alVeTiles.Count; i++)
               {
                  veTile = (VeTile)alVeTiles[i];
                  if (veTile.Texture == null) //not downloaded yet
                  {
                     notDownloaded[notDownloadedIter] = i;
                     notDownloadedIter++;
                     continue;
                  }
                  else if (veTile.level == zoomLevel)
                  {
                     //NOTE to stop ripping?
                     drawArgs.device.Clear(ClearFlags.ZBuffer, 0, 1.0f, 0);

                     drawArgs.device.SetTexture(0, veTile.Texture);

                     drawArgs.device.DrawIndexedUserPrimitives(PrimitiveType.TriangleList, 0,
                        veTile.vertices.Length, veTile.indices.Length / 3, veTile.indices, true, veTile.vertices);
                  }
               }

               //now render the downloading tiles
               drawArgs.device.RenderState.ZBufferEnable = false;
               drawArgs.device.VertexFormat = CustomVertex.PositionColored.Format;
               drawArgs.device.TextureState[0].ColorOperation = TextureOperation.Disable;

               int tileIndex;
               for (int i = 0; i < notDownloadedIter; i++)
               {
                  tileIndex = notDownloaded[i];
                  veTile = (VeTile)alVeTiles[tileIndex];
                  if (World.Settings.ShowDownloadRectangles)
                     veTile.RenderDownloadRectangle(drawArgs);
               }

               drawArgs.device.TextureState[0].ColorOperation = TextureOperation.SelectArg1;
               drawArgs.device.VertexFormat = CustomVertex.PositionTextured.Format;
               drawArgs.device.RenderState.ZBufferEnable = true;
            }
         }
         catch (Exception ex)
         {
            string sex = ex.ToString();
            Utility.Log.Write(ex);
         }
         finally
         {
            if (disableZbuffer)
               drawArgs.device.RenderState.ZBufferEnable = true;
         }
      }

      private static int GetZoomLevelByTrueViewRange(double trueViewRange)
      {
         int maxLevel = 3;
         int minLevel = 19;
         int numLevels = minLevel - maxLevel + 1;
         int retLevel = maxLevel;
         for (int i = 0; i < numLevels; i++)
         {
            retLevel = i + maxLevel;

            double viewAngle = 180;
            for (int j = 0; j < i; j++)
            {
               viewAngle = viewAngle / 2.0;
            }
            if (trueViewRange >= viewAngle)
            {
               break;
            }
         }
         return retLevel;
      }

      public void CreateDownloadRectangle(DrawArgs drawArgs, int color)
      {
         // Render terrain download rectangle
         Point3d northWestV = MathEngine.SphericalToCartesian((float)North, (float)West, _layerRadius);
         Point3d southWestV = MathEngine.SphericalToCartesian((float)South, (float)West, _layerRadius);
         Point3d northEastV = MathEngine.SphericalToCartesian((float)North, (float)East, _layerRadius);
         Point3d southEastV = MathEngine.SphericalToCartesian((float)South, (float)East, _layerRadius);

         downloadRectangle[0].X = (float)northWestV.X;
         downloadRectangle[0].Y = (float)northWestV.Y;
         downloadRectangle[0].Z = (float)northWestV.Z;
         downloadRectangle[0].Color = color;

         downloadRectangle[1].X = (float)southWestV.X;
         downloadRectangle[1].Y = (float)southWestV.Y;
         downloadRectangle[1].Z = (float)southWestV.Z;
         downloadRectangle[1].Color = color;

         downloadRectangle[2].X = (float)southEastV.X;
         downloadRectangle[2].Y = (float)southEastV.Y;
         downloadRectangle[2].Z = (float)southEastV.Z;
         downloadRectangle[2].Color = color;

         downloadRectangle[3].X = (float)northEastV.X;
         downloadRectangle[3].Y = (float)northEastV.Y;
         downloadRectangle[3].Z = (float)northEastV.Z;
         downloadRectangle[3].Color = color;

         downloadRectangle[4].X = downloadRectangle[0].X;
         downloadRectangle[4].Y = downloadRectangle[0].Y;
         downloadRectangle[4].Z = downloadRectangle[0].Z;
         downloadRectangle[4].Color = color;
      }

      public void RenderDownloadRectangle(DrawArgs drawArgs)
      {
         drawArgs.device.DrawUserPrimitives(PrimitiveType.LineStrip, 4, downloadRectangle);
      }
   }
   #endregion

   #region PROJ.4
   //this code is Mashi's pInvoke wrapper over Proj.4
   //i didn't make any modifications to it
   //----------------------------------------------------------------------------
   // : Reproject images on the fly
   // : 1.0
   // : Reproject images on the fly using nowak's "reproject on GPU technique" 
   // : Bjorn Reppen aka "Mashi"
   // : http://www.mashiharu.com
   // : 
   //----------------------------------------------------------------------------
   // This file is in the Public Domain, and comes with no warranty. 

   /// <summary>
   /// Sorry for lack of description, but this struct is kinda difficult 
   /// to describe since it supports so many coordinate systems.
   /// </summary>
   [StructLayout(LayoutKind.Sequential)]
   public struct UV
   {
      public double U;
      public double V;

      public UV(double u, double v)
      {
         this.U = u;
         this.V = v;
      }
   }

   /// <summary>
   /// C# wrapper for proj.4 projection filter
   /// http://proj.maptools.org/
   /// </summary>
   public class Projection : IDisposable
   {
      IntPtr projPJ;
      [DllImport("\\Plugins\\VirtualEarth\\proj.dll")]
      static extern IntPtr pj_init(int argc, string[] args);

      [DllImport("\\Plugins\\VirtualEarth\\proj.dll")]
      static extern string pj_free(IntPtr projPJ);

      [DllImport("\\Plugins\\VirtualEarth\\proj.dll")]
      static extern UV pj_fwd(UV uv, IntPtr projPJ);

      /// <summary>
      /// XY -> Lat/lon
      /// </summary>
      /// <param name="uv"></param>
      /// <param name="projPJ"></param>
      /// <returns></returns>
      [DllImport("Plugins\\VirtualEarth\\proj.dll")]
      static extern UV pj_inv(UV uv, IntPtr projPJ);

      /// <summary>
      /// Constructor.
      /// </summary>
      /// <param name="initParameters">Proj.4 style list of options.
      /// <sample>new string[]{ "proj=utm", "ellps=WGS84", "no.defs", "zone=32" }</sample>
      /// </param>
      public Projection(string[] initParameters)
      {
         projPJ = pj_init(initParameters.Length, initParameters);
         if (projPJ == IntPtr.Zero)
            throw new ApplicationException("Projection initialization failed.");
      }

      /// <summary>
      /// Forward (Go from specified projection to lat/lon)
      /// </summary>
      /// <param name="uv"></param>
      /// <returns></returns>
      public UV Forward(UV uv)
      {
         return pj_fwd(uv, projPJ);
      }

      /// <summary>
      /// Inverse (Go from lat/lon to specified projection)
      /// </summary>
      /// <param name="uv"></param>
      /// <returns></returns>
      public UV Inverse(UV uv)
      {
         return pj_inv(uv, projPJ);
      }

      #region IDisposable Implementation
      protected virtual void Dispose(bool disposing)
      {
         if (projPJ != IntPtr.Zero)
         {
            // Dispose of resources held by this instance.
            pj_free(projPJ);
            projPJ = IntPtr.Zero;

            // Suppress finalization of this disposed instance.
            if (disposing)
            {
               GC.SuppressFinalize(this);
            }
         }
      }

      public void Dispose()
      {
         Dispose(true);
      }

      ~Projection()
      {
         Dispose(false);
      }
      #endregion
   }
   #endregion
}
