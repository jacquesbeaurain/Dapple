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
using System.Xml.Serialization;

using System.Globalization;

using System.Security.Cryptography;

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
using WorldWind.PluginEngine;

using Utility;

namespace bNb.Plugins_GD
{
	#region VEPROJECTTILESLAYER
	public class VeReprojectTilesLayer : RenderableObject
	{
		private Projection proj;
		private WorldWindow parentApplication;

		private MD5 md5Hasher = MD5.Create();

		private static double earthRadius; //6378137;
		private static double earthCircum; //40075016.685578488
		private static double earthHalfCirc; //20037508.

		public const int pixelsPerTile = 256;
		private int prevRow = -1;
		private int prevCol = -1;
		private int prevLvl = -1;
		private float prevVe = -1;
		private double preTilt = 0;

		private ArrayList veTiles = new ArrayList();

		private string datasetName;
		private string imageExtension;
		private string cacheDirectory;

		public VeReprojectTilesLayer(string name, WorldWindow parentApplication, string datasetName, string imageExtension, int startZoomLevel, string cache)
			: base(name)
		{
			this.name = name;
			this.parentApplication = parentApplication;
			this.datasetName = datasetName;
			this.imageExtension = imageExtension;
			this.cacheDirectory = cache;
		}

		private Sprite pushpinsprite, logosprite;


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

				pushpinsprite = new Sprite(drawArgs.device);

				string spritePath = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + "\\Plugins\\VirtualEarth\\VirtualEarthPushPin.png";

				if (File.Exists(spritePath) == false)
				{
					Utility.Log.Write(new Exception("spritePath not found " + spritePath));
				}

				earthRadius = parentApplication.CurrentWorld.EquatorialRadius;
				earthCircum = earthRadius * 2.0 * Math.PI; //40075016.685578488
				earthHalfCirc = earthCircum / 2; //20037508.

				string[] projectionParameters = new string[] { "proj=merc", "ellps=sphere", "a=" + earthRadius.ToString(), "es=0.0", "no.defs" };
				proj = new Projection(projectionParameters);

				//static
				VeTile.Init(this.proj, parentApplication.CurrentWorld.TerrainAccessor, parentApplication.CurrentWorld.EquatorialRadius);

				prevVe = World.Settings.VerticalExaggeration;

				this.isInitialized = true;
			}
			catch (Exception ex)
			{
				Utility.Log.Write(ex);
				throw;
			}
		}


		public string GetLocalLiveLink()
		{
			//http://local.live.com/default.aspx?v=2&cp=43.057723~-88.404224&style=r&lvl=12
			string lat = parentApplication.DrawArgs.WorldCamera.Latitude.Degrees.ToString("###.#####");
			string lon = parentApplication.DrawArgs.WorldCamera.Longitude.Degrees.ToString("###.#####");
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
					//veTiles.RemoveAt(i);
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
				double tilt = drawArgs.WorldCamera.Tilt.Degrees;
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
				//if (zoomLevel < veForm.StartZoomLevel)
				//{
				//	this.RemoveAllTiles();
				// this.ForceRefresh();
				//	return;
				//}

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
				if (row == prevRow && col == prevCol && zoomLevel == prevLvl && tilt == preTilt)
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
				// Extend tile grid if camera tilt above some values
				if (tilt > 45) AddNeighborTiles(drawArgs, row, col, zoomLevel, null, 4);
				if (tilt > 60) AddNeighborTiles(drawArgs, row, col, zoomLevel, null, 5);


				//if(prevLvl > zoomLevel) //zooming out
				//{
				//}			

				lock (veTiles.SyncRoot)
				{
					VeTile veTile;
					for (int i = 0; i < veTiles.Count; i++)
					{
						veTile = (VeTile)veTiles[i];
						if (veTile.IsNeeded == false && veTile.DownloadInProgress == false)
						{
							veTile.Dispose();
							veTiles.RemoveAt(i);
							i--;
						}
					}
				}
				
				prevRow = row;
				prevCol = col;
				prevLvl = zoomLevel;
				preTilt = tilt;
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
			VeTile newVeTile = new VeTile(row, col, zoomLevel, this.md5Hasher);

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

		private static double MetersPerTile(int zoom)
		{
			return MetersPerPixel(zoom) * pixelsPerTile;
		}

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

		private static double RadToDeg(double d)
		{
			return d * 180 / Math.PI;
		}

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

		private int LatitudeToYAtZoom(double lat, int zoom)
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
		}

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

					// camera jitter fix
					drawArgs.device.Transform.World = Matrix.Translation(
							 (float)-drawArgs.WorldCamera.ReferenceCenter.X,
						 (float)-drawArgs.WorldCamera.ReferenceCenter.Y,
						 (float)-drawArgs.WorldCamera.ReferenceCenter.Z
					);

					// Clear ZBuffer between layers (as in WW)
					drawArgs.device.Clear(ClearFlags.ZBuffer, 0, 1.0f, 0);

					// Render tiles
					int zoomLevel = GetZoomLevelByTrueViewRange(drawArgs.WorldCamera.TrueViewRange.Degrees);
					int tileDrawn = VeTile.Render(drawArgs, disableZBuffer, veTiles, zoomLevel, m_opacity); // Try current level first
					while (zoomLevel > 1 && tileDrawn == 0)
					{
						// If nothing drawn, try to render previous level tiles if they are still around, TODO: force refresh for that level on update???
						zoomLevel--;
						tileDrawn = VeTile.Render(drawArgs, disableZBuffer, veTiles, zoomLevel, m_opacity);
					}

					//camera jitter fix
					drawArgs.device.Transform.World = ConvertDX.FromMatrix4d(drawArgs.WorldCamera.WorldMatrix);

					//Render logo
					//RenderDownloadProgress(drawArgs, null, 0);

				}

				//else pushpins only
				//render PushPins
				if (pushPins != null && pushPins.Count > 0)
				{

					//RenderPushPins(drawArgs);

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

		private ArrayList pushPins = null;
		public ArrayList PushPins
		{
			get { return pushPins; }
			set { pushPins = value; }
		}
		
		/// <summary>
		/// Cleanup when layer is disabled
		/// </summary>
		public override void Dispose()
		{
			RemoveAllTiles();

			if (pushpinsprite != null)
			{
				pushpinsprite.Dispose();
				pushpinsprite = null;
			}

			if (logosprite != null)
			{
				logosprite.Dispose();
				logosprite = null;
			}
		}

		/// <summary>
		/// Handle mouse click
		/// </summary>
		/// <returns>true if click was handled.</returns>
		public override bool PerformSelectionAction(DrawArgs drawArgs)
		{
			return false;
		}

      public override void InitExportInfo(DrawArgs drawArgs, RenderableObject.ExportInfo info)
      {
         lock (veTiles)
         {
            int iMinCol = int.MaxValue;
            int iMaxCol = int.MinValue;
            int iMinRow = int.MaxValue;
            int iMaxRow = int.MinValue;

            int iLevel = ((VeTile)veTiles[0]).Level;

            foreach (VeTile oTile in veTiles)
            {
               iMinRow = Math.Min(oTile.Row, iMinRow);
               iMaxRow = Math.Max(oTile.Row, iMaxRow);

               iMinCol = Math.Min(oTile.Col, iMinCol);
               iMaxCol = Math.Max(oTile.Col, iMaxCol);

               info.dMinLat = Math.Min(info.dMinLat, oTile.MinY);
               info.dMaxLat = Math.Max(info.dMaxLat, oTile.MaxY);

               info.dMinLon = Math.Min(info.dMinLon, oTile.MinX);
               info.dMaxLon = Math.Max(info.dMaxLon, oTile.MaxX);
            }

            info.iPixelsX = (iMaxCol - iMinCol + 1) * pixelsPerTile;
            info.iPixelsY = (iMaxRow - iMinRow + 1) * pixelsPerTile;
         }
      }

      public override void ExportProcess(DrawArgs drawArgs, RenderableObject.ExportInfo expInfo)
      {
         foreach (VeTile oTile in veTiles)
         {
            double dNorth = (oTile.MaxY - expInfo.dMaxLat) / (expInfo.dMinLat - expInfo.dMaxLat) * expInfo.iPixelsY;
            double dSouth = (oTile.MinY - expInfo.dMaxLat) / (expInfo.dMinLat - expInfo.dMaxLat) * expInfo.iPixelsY;

            double dEast = (oTile.MaxX - expInfo.dMinLon) / (expInfo.dMaxLon - expInfo.dMinLon) * expInfo.iPixelsX;
            double dWest = (oTile.MinX - expInfo.dMinLon) / (expInfo.dMaxLon - expInfo.dMinLon) * expInfo.iPixelsX;

            using (Image oMap = oTile.getBitmap(cacheDirectory, datasetName))
            {
               expInfo.gr.DrawImage(oMap, new Rectangle((int)dWest, (int)dNorth, (int)(dEast - dWest), (int)(dSouth - dNorth)));
            }
         }
      }
	}
	#endregion

	#region VESETTINGS
	/// <summary>
	/// This class stores virtual earth settings
	/// </summary>
	[Serializable]
	public class VESettings
	{
		/// <summary>
		/// Layer Zoom Level
		/// </summary>
		private int zoomlevel = 8;

		/// <summary>
		/// Turn layer on
		/// </summary>
		private bool layeron = true;
		/// <summary>
		/// Turn terrain on
		/// </summary>
		private bool terrain = true;
		/// <summary>
		/// Layer types
		/// </summary>
		private bool road = true;
		private bool aerial = false;
		private bool hybrid = false;
		private bool debug = false;


		public int ZoomLevel
		{
			get
			{
				return zoomlevel;
			}
			set
			{
				zoomlevel = value;
			}
		}

		public bool LayerOn
		{
			get
			{
				return layeron;
			}
			set
			{
				layeron = value;
			}
		}

		public bool Terrain
		{
			get
			{
				return terrain;
			}
			set
			{
				terrain = value;
			}
		}

		public bool Road
		{
			get
			{
				return road;
			}
			set
			{
				road = value;
			}
		}

		public bool Aerial
		{
			get
			{
				return aerial;
			}
			set
			{
				aerial = value;
			}
		}

		public bool Hybrid
		{
			get
			{
				return hybrid;
			}
			set
			{
				hybrid = value;
			}
		}

		public bool Debug
		{
			get
			{
				return debug;
			}
			set
			{
				debug = value;
			}
		}
		/// <summary>
		/// Loads a serialized instance of the settings from the specified file
		/// returns default values if the file doesn't exist or an error occurs
		/// </summary>
		/// <returns>The persisted settings from the file</returns>
		public static VESettings LoadSettingsFromFile(string filename)
		{
			VESettings settings;
			XmlSerializer xs = new XmlSerializer(typeof(VESettings));

			if (File.Exists(filename))
			{
				FileStream fs = null;

				try
				{
					fs = File.Open(filename, FileMode.Open, FileAccess.Read);
				}
				catch
				{
					return new VESettings();
				}

				try
				{
					settings = (VESettings)xs.Deserialize(fs);
				}
				catch
				{
					settings = new VESettings();
				}
				finally
				{
					fs.Close();
				}
			}
			else
			{
				settings = new VESettings();
			}

			return settings;
		}

		/// <summary>
		/// Persists the settings to the specified filename
		/// </summary>
		/// <param name="file">The filename to use for saving</param>
		/// <param name="settings">The instance of the Settings class to persist</param>
		public static void SaveSettingsToFile(string file, VESettings settings)
		{
			FileStream fs = null;
			XmlSerializer xs = new XmlSerializer(typeof(VESettings));

			fs = File.Open(file, FileMode.Create, FileAccess.Write);

			try
			{
				xs.Serialize(fs, settings);
			}
			finally
			{
				fs.Close();
			}
		}
	}
	#endregion

	#region VETILE
	public class VeTile : IDisposable
	{
		//Whether a download is currently in progress
		private bool m_blDownloading = false;

      //these are the coordinate extents for the tile
      UV m_ul, m_ur, m_ll, m_lr;

		private MD5 md5Hasher;
		private static byte[] noTileMD5Hash = { 0xc1, 0x32, 0x69, 0x48, 0x1c, 0x73, 0xde, 0x6e, 0x18, 0x58, 0x9f, 0x9f, 0xbc, 0x3b, 0xdf, 0x7e };

		public Boolean DownloadInProgress
		{
			get
			{
				return m_blDownloading;
			}
		}

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

		public static void Init(Projection proj, TerrainAccessor terrainAccessor, double layerRadius)
		{
			_proj = proj;
			_terrainAccessor = terrainAccessor;
			_layerRadius = layerRadius;
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

      public int Row { get { return row; } }
      public int Col { get { return col; } }
      public int Level { get { return level; } }

		public VeTile(int row, int col, int level, MD5 _md5Hasher)
		{
			this.md5Hasher = _md5Hasher;
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
		private DrawArgs drawArgs;

		public void GetTexture(DrawArgs drawArgs, int pixelsPerTile, string _datasetName, string imageExtension, string _cacheDirectory)
		{
			this.drawArgs = drawArgs;
			string _imageExtension = imageExtension;
			string _serverUri = ".ortho.tiles.virtualearth.net/tiles/";

			string quadKey = TileToQuadKey(col, row, level);

			//TODO no clue what ?g= is
			string textureUrl = String.Concat(new object[] { "http://", _datasetName, quadKey[quadKey.Length - 1], _serverUri, _datasetName, quadKey, ".", _imageExtension, "?g=", 15 });

			//load a tile from file OR download it if not cached
			string levelDir = CreateLevelDir(level, _cacheDirectory);
			string mapTypeDir = CreateMapTypeDir(levelDir, _datasetName);
			string rowDir = CreateRowDir(mapTypeDir, row);
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
				if (!IsBadTile())
					this.texture = TextureLoader.FromFile(drawArgs.device, textureName);
				else
				{
					// If older than 7 days download it again
					FileInfo fi = new FileInfo(textureName);
					TimeSpan ts = DateTime.Now - fi.LastWriteTime;
					if (ts.Days > 7)
						File.Delete(textureName);
					else
						return;
				}
			}
			
			
			if (this.texture == null)
			{
				m_blDownloading = true;
				download = new WebDownload(textureUrl);
				download.DownloadType = DownloadType.Unspecified;
				download.SavedFilePath = textureName + ".tmp"; //?
				download.ProgressCallback += new DownloadProgressHandler(UpdateProgress);
				download.CompleteCallback += new DownloadCompleteHandler(DownloadComplete);
				download.BackgroundDownloadFile();
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

		bool IsBadTile()
		{
			bool bBadTile = false;
			if (File.Exists(textureName))
			{

				// VE's no data tiles are exactly 1033 bytes, but we compute the MD5 Hash too just to be sure 
				// If the tile in the cache is more than a week old we will get rid of it, perhaps they get new data,
				// otherwise no texture will be created and the textures above will show instead 

				FileInfo fi = new FileInfo(textureName);
				if (fi.Length == 1033)
				{
					using (FileStream fs = new FileStream(textureName, FileMode.Open))
					{
						byte[] md5Hash = md5Hasher.ComputeHash(fs);
						if (noTileMD5Hash.Length == md5Hash.Length)
						{
							bBadTile = true;
							for (int i = 0; i < noTileMD5Hash.Length; i++)
							{
								if (noTileMD5Hash[i] != md5Hash[i])
								{
									bBadTile = false;
									break;
								}
							}
						}
					}
				}
			}
			return bBadTile;
		}

		private void DownloadComplete(WebDownload downloadInfo)
		{
			try
			{
				downloadInfo.Verify();

				// Rename temp file to real name
				File.Delete(textureName);
				File.Move(downloadInfo.SavedFilePath, textureName);

				if (!IsBadTile())
				{
					// Make the quad tile reload the new image
					//m_quadTile.DownloadRequest = null;
					//m_quadTile.isInitialized = false;
					this.texture = TextureLoader.FromFile(drawArgs.device, textureName);
				}
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
				m_blDownloading = false;
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
			//VirtualEarth.m_WorldWindow.Cache.CacheDirectory
			string cacheDirectory = String.Format("{0}\\Virtual Earth", cacheDirectoryRoot);
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

		public string CreateMapTypeDir(string levelDir, string mapType)
		{
			string mapTypeDir = levelDir + @"\" + mapType;
			if (Directory.Exists(mapTypeDir) == false)
			{
				Directory.CreateDirectory(mapTypeDir);
			}
			return mapTypeDir;
		}

		public string CreateRowDir(string mapTypeDir, int row)
		{
			string rowDir = mapTypeDir + @"\" + row.ToString("0000");
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

		protected CustomVertex.PositionNormalTextured[] vertices;
		public CustomVertex.PositionNormalTextured[] Vertices
		{
			get { return vertices; }
		}
		protected short[] indices;
		public short[] Indices
		{
			get { return indices; }
		}
		protected int meshPointCount = 64;

		private double North;
		private double South;
		private double West;
		private double East;

      public double MinX { get { return West; } }
      public double MaxX { get { return East; } }
      public double MinY { get { return South; } }
      public double MaxY { get { return North; } }

		//NOTE this is a mix from Mashi's Reproject and WW for terrain
		public void CreateMesh(byte opacity, float verticalExaggeration)
		{
			this.vertEx = verticalExaggeration;

			int opacityColor = System.Drawing.Color.FromArgb(opacity, 0, 0, 0).ToArgb();

			meshPointCount = 32; //64; //96 // How many vertices for each direction in mesh (total: n^2)
			//vertices = new CustomVertex.PositionColoredTextured[meshPointCount * meshPointCount];

			// Build mesh with one extra row and col around the terrain for normal computation and struts
			vertices = new CustomVertex.PositionNormalTextured[(meshPointCount + 2) * (meshPointCount + 2)];

			int upperBound = meshPointCount - 1;
			float scaleFactor = (float)1 / upperBound;
			//using(Projection proj = new Projection(m_projectionParameters))
			//{
			double uStep = (UR.U - UL.U) / upperBound;
			double vStep = (UL.V - LL.V) / upperBound;
			UV curUnprojected = new UV(UL.U - uStep, UL.V + vStep);

			// figure out latrange (for terrain detail)
			UV geoUL = _proj.Inverse(m_ul);
			UV geoLR = _proj.Inverse(m_lr);
			double latRange = (geoUL.U - geoLR.U) * 180 / Math.PI;

			North = geoUL.V * 180 / Math.PI;
			South = geoLR.V * 180 / Math.PI;
			West = geoUL.U * 180 / Math.PI;
			East = geoLR.U * 180 / Math.PI;

			float meshBaseRadius = (float)_layerRadius;
			
			UV geo;
			Point3d pos;
			double height = 0;
			for (int i = 0; i < meshPointCount + 2; i++)
			{
				for (int j = 0; j < meshPointCount + 2; j++)
				{
					geo = _proj.Inverse(curUnprojected);

					// Radians -> Degrees
					geo.U *= 180 / Math.PI;
					geo.V *= 180 / Math.PI;

					if (_terrainAccessor != null)
					{
							height = verticalExaggeration * _terrainAccessor.GetElevationAt(geo.V, geo.U, Math.Abs(upperBound / latRange));
					}

					pos = MathEngine.SphericalToCartesian(
						 geo.V,
						 geo.U,
						 _layerRadius + height);
					int idx = i * (meshPointCount + 2) + j;
					vertices[idx].X = (float)pos.X;
					vertices[idx].Y = (float)pos.Y;
					vertices[idx].Z = (float)pos.Z;

					vertices[idx].Tu = (j - 1) * scaleFactor;
					vertices[idx].Tv = (i - 1) * scaleFactor;
					curUnprojected.U += uStep;
				}
				curUnprojected.U = UL.U - uStep;
				curUnprojected.V -= vStep;
			}

			int slices = meshPointCount + 1;
			indices = new short[2 * slices * slices * 3];
			for (int i = 0; i < slices; i++)
			{
				for (int j = 0; j < slices; j++)
				{
					indices[(2 * 3 * i * slices) + 6 * j] = (short)(i * (meshPointCount + 2) + j);
					indices[(2 * 3 * i * slices) + 6 * j + 1] = (short)((i + 1) * (meshPointCount + 2) + j);
					indices[(2 * 3 * i * slices) + 6 * j + 2] = (short)(i * (meshPointCount + 2) + j + 1);

					indices[(2 * 3 * i * slices) + 6 * j + 3] = (short)(i * (meshPointCount + 2) + j + 1);
					indices[(2 * 3 * i * slices) + 6 * j + 4] = (short)((i + 1) * (meshPointCount + 2) + j);
					indices[(2 * 3 * i * slices) + 6 * j + 5] = (short)((i + 1) * (meshPointCount + 2) + j + 1);
				}
			}

			// Compute normals and fold struts
			calculate_normals();
			fold_struts(false, meshBaseRadius);
		}

		// Compute mesh normals and fold struts
		private void calculate_normals()
		{
			System.Collections.ArrayList[] normal_buffer = new System.Collections.ArrayList[vertices.Length];
			for (int i = 0; i < vertices.Length; i++)
			{
				normal_buffer[i] = new System.Collections.ArrayList();
			}
			for (int i = 0; i < indices.Length; i += 3)
			{
				Vector3 p1 = vertices[indices[i + 0]].Position;
				Vector3 p2 = vertices[indices[i + 1]].Position;
				Vector3 p3 = vertices[indices[i + 2]].Position;

				Vector3 v1 = p2 - p1;
				Vector3 v2 = p3 - p1;
				Vector3 normal = Vector3.Cross(v1, v2);

				normal.Normalize();

				// Store the face's normal for each of the vertices that make up the face.
				normal_buffer[indices[i + 0]].Add(normal);
				normal_buffer[indices[i + 1]].Add(normal);
				normal_buffer[indices[i + 2]].Add(normal);
			}

			// Now loop through each vertex vector, and avarage out all the normals stored.
			for (int i = 0; i < vertices.Length; ++i)
			{
				for (int j = 0; j < normal_buffer[i].Count; ++j)
				{
					Vector3 curNormal = (Vector3)normal_buffer[i][j];

					if (vertices[i].Normal == Vector3.Empty)
						vertices[i].Normal = curNormal;
					else
						vertices[i].Normal += curNormal;
				}

				vertices[i].Normal.Multiply(1.0f / normal_buffer[i].Count);
			}
		}

		// Adjust/Fold struts vertices using terrain border vertices positions
		private void fold_struts(bool renderStruts, float meshBaseRadius)
		{
			short vertexDensity = (short)Math.Sqrt(vertices.Length);
			for (int i = 0; i < vertexDensity; i++)
			{
				if (i == 0 || i == vertexDensity - 1)
				{
					for (int j = 0; j < vertexDensity; j++)
					{
						int offset = (i == 0) ? vertexDensity : -vertexDensity;
						if (j == 0) offset++;
						if (j == vertexDensity - 1) offset--;
						Point3d p = new Point3d(vertices[i * vertexDensity + j + offset].Position.X, vertices[i * vertexDensity + j + offset].Position.Y, vertices[i * vertexDensity + j + offset].Position.Z);
						if (renderStruts) p = ProjectOnMeshBase(p, meshBaseRadius);
						vertices[i * vertexDensity + j].Position = new Vector3((float)p.X, (float)p.Y, (float)p.Z);
					}
				}
				else
				{
					Point3d p = new Point3d(vertices[i * vertexDensity + 1].Position.X, vertices[i * vertexDensity + 1].Position.Y, vertices[i * vertexDensity + 1].Position.Z);
					if (renderStruts) p = ProjectOnMeshBase(p, meshBaseRadius);
					vertices[i * vertexDensity].Position = new Vector3((float)p.X, (float)p.Y, (float)p.Z);

					p = new Point3d(vertices[i * vertexDensity + vertexDensity - 2].Position.X, vertices[i * vertexDensity + vertexDensity - 2].Position.Y, vertices[i * vertexDensity + vertexDensity - 2].Position.Z);
					if (renderStruts) p = ProjectOnMeshBase(p, meshBaseRadius);
					vertices[i * vertexDensity + vertexDensity - 1].Position = new Vector3((float)p.X, (float)p.Y, (float)p.Z);
				}
			}
		}

		// Project an elevated mesh point to the mesh base
		private Point3d ProjectOnMeshBase(Point3d p, float meshBaseRadius)
		{
			p.normalize();
			p = p * meshBaseRadius;
			return p;
		}

		public void Dispose()
		{
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
			GC.SuppressFinalize(this);
		}

		CustomVertex.PositionColored[] downloadRectangle = new CustomVertex.PositionColored[5];

		public static int Render(DrawArgs drawArgs, bool disableZbuffer, ArrayList alVeTiles, int zoomLevel, ushort usOpacity)
		{
			int tileDrawn = 0;
         int iOldTextureFactor = 0;
			try
			{
            iOldTextureFactor = drawArgs.device.RenderState.TextureFactor;
				if (alVeTiles.Count <= 0)
					return 0;

				lock (alVeTiles.SyncRoot)
				{
               // Turn back light on if needed
               if (World.Settings.EnableSunShading)
               {
                  drawArgs.device.RenderState.Lighting = false;
               }

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
					drawArgs.device.VertexFormat = CustomVertex.PositionNormalTextured.Format;

               drawArgs.device.TextureState[0].ColorOperation = TextureOperation.BlendCurrentAlpha;//TextureOperation.SelectArg1;
					drawArgs.device.TextureState[0].ColorArgument1 = TextureArgument.TextureColor;
					drawArgs.device.TextureState[0].AlphaOperation = TextureOperation.Modulate;//TextureOperation.SelectArg1;
					drawArgs.device.TextureState[0].AlphaArgument1 = TextureArgument.TextureColor;
               drawArgs.device.RenderState.TextureFactor = Color.FromArgb(usOpacity, 255, 255, 255).ToArgb();

					// Set up for shading 
					if (World.Settings.EnableSunShading)
					{
						drawArgs.device.TextureState[0].ColorOperation = TextureOperation.Modulate;
						drawArgs.device.TextureState[0].ColorArgument1 = TextureArgument.Diffuse;
						drawArgs.device.TextureState[0].ColorArgument2 = TextureArgument.TextureColor;
					}

					//save index to tiles not downloaded yet
					int notDownloadedIter = 0;
					int[] notDownloaded = new int[alVeTiles.Count];

					//render tiles that are downloaded
					VeTile veTile;
					for (int i = 0; i < alVeTiles.Count; i++)
					{
						veTile = (VeTile)alVeTiles[i];
						// Only current level
						if (veTile.level == zoomLevel)
						{
							if (veTile.Texture == null) //not downloaded yet
							{
								notDownloaded[notDownloadedIter] = i;
								notDownloadedIter++;
								continue;
							}
							else
							{
								drawArgs.device.SetTexture(0, veTile.Texture);

								drawArgs.device.DrawIndexedUserPrimitives(PrimitiveType.TriangleList, 0,
									 veTile.Vertices.Length, veTile.Indices.Length / 3, veTile.Indices, true, veTile.Vertices);

								tileDrawn++;
							}
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
						if (World.Settings.ShowDownloadIndicator)
							veTile.RenderDownloadRectangle(drawArgs);
					}

					drawArgs.device.TextureState[0].ColorOperation = TextureOperation.SelectArg1;
					drawArgs.device.VertexFormat = CustomVertex.PositionTextured.Format;
					drawArgs.device.RenderState.ZBufferEnable = true;

					// Turn back light on if needed
					if (World.Settings.EnableSunShading)
					{
						drawArgs.device.RenderState.Lighting = true;
					}

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

            drawArgs.device.RenderState.TextureFactor = iOldTextureFactor;
			}
			return tileDrawn;
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

			// camera jitter fix
			drawArgs.device.Transform.World = Matrix.Translation(
					 (float)-drawArgs.WorldCamera.ReferenceCenter.X,
				 (float)-drawArgs.WorldCamera.ReferenceCenter.Y,
				 (float)-drawArgs.WorldCamera.ReferenceCenter.Z
			);

			drawArgs.device.DrawUserPrimitives(PrimitiveType.LineStrip, 4, downloadRectangle);

			// camera jitter fix
			drawArgs.device.Transform.World = ConvertDX.FromMatrix4d(drawArgs.WorldCamera.WorldMatrix);
		}

      internal Image getBitmap(String _cacheDirectory, String _datasetName)
      {
         string levelDir = CreateLevelDir(level, _cacheDirectory);
         string mapTypeDir = CreateMapTypeDir(levelDir, _datasetName);
         string rowDir = CreateRowDir(mapTypeDir, row);
         string textureName = String.Empty;
         if (_datasetName == "r")
         {
            textureName = GetTextureName(rowDir, row, col, "png");
         }
         else
         {
            textureName = GetTextureName(rowDir, row, col, "jpeg");
         }

         if (File.Exists(textureName))
         {
            Image result = Bitmap.FromFile(textureName);
            return result;
         }
         else
         {
            Image oPlaceHolder = new Bitmap(VeReprojectTilesLayer.pixelsPerTile, VeReprojectTilesLayer.pixelsPerTile);
            return oPlaceHolder;
         }
      }
   }
	#endregion

}
