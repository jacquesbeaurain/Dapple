using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;

using System.Security.Cryptography;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

using WorldWind;
using WorldWind.Camera;
using WorldWind.Net;
using WorldWind.VisualControl;
using WorldWind.Renderable;

using Utility;

namespace Dapple.Plugins.VirtualEarth
{
	public class VEQuadTile : IGeoSpatialDownloadTile, IDisposable
	{
		private static byte[] noTileMD5Hash = { 0xc1, 0x32, 0x69, 0x48, 0x1c, 0x73, 0xde, 0x6e, 0x18, 0x58, 0x9f, 0x9f, 0xbc, 0x3b, 0xdf, 0x7e };

		private static MD5 _md5Hasher;
		private static System.Drawing.Font _font;
		private static Brush _brush;
		//private static VirtualEarthForm _veForm;

		public static void Init()//, VirtualEarthForm veForm)
		{
			if (_md5Hasher == null)
			{
				//_veForm = veForm;
				_font = new System.Drawing.Font("Verdana", 15, FontStyle.Bold);
				_brush = new SolidBrush(Color.Green);
				_md5Hasher = MD5.Create();
			}
		}

		//these are the coordinate extents for the tile
		public UV UL, UR, LL, LR;


		public double LatitudeSpan;
		public double LongitudeSpan;

		private VEQuadTileSet quadTileSet;
		private Angle centerLatitude;
		private Angle centerLongitude;
		private int level;
		private int _Row;
		private int _Col;
		public double west;
		public double east;
		public double north;
		public double south;

		public bool isInitialized;
		public BoundingBox BoundingBox;

		private List<GeoSpatialDownloadRequest> downloadRequests;

		protected Texture texture;

		/// <summary>
		/// Number of points in child flat mesh grid (times 2)
		/// </summary>
		protected static int vertexCount = 40;

		/// <summary>
		/// Number of points in child terrain mesh grid (times 2)
		/// </summary>
		protected static int vertexCountElevated = 40;

		protected VEQuadTile northWestChild;
		protected VEQuadTile southWestChild;
		protected VEQuadTile northEastChild;
		protected VEQuadTile southEastChild;

		protected CustomVertex.PositionNormalTextured[] vertices;
		protected short[] indices;
		protected Point3d localOrigin; // Add this offset to get world coordinates

		protected bool m_isResetingCache;

		/// <summary>
		/// The vertical exaggeration the tile mesh was computed for
		/// </summary>
		protected float verticalExaggeration;

		protected bool isDownloadingTerrain;

		private string key;

		/// New Cache idea
		/// 
		internal static Dictionary<string, CacheEntry> VerticeCache
			 = new Dictionary<string, CacheEntry>();

		internal static int CacheSize = 256;

		internal class CacheEntry
		{
			public CustomVertex.PositionNormalTextured[] vertices;
			public short[] indices;

			public DateTime EntryTime;
		}

		// End New Cache

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.Renderable.VEQuadTile"/> class.
		/// </summary>
		/// <param name="south"></param>
		/// <param name="north"></param>
		/// <param name="west"></param>
		/// <param name="east"></param>
		/// <param name="level"></param>
		/// <param name="quadTileSet"></param>
		public VEQuadTile(int row, int col, int level, VEQuadTileSet quadTileSet)
		{
			_Row = row;
			_Col = col;

			//handle the diff projection
			double metersPerPixel = MetersPerPixel(quadTileSet.LayerRadius, level);
			double totalTilesPerEdge = Math.Pow(2, level);
			double totalMeters = totalTilesPerEdge * textureSizePixels * metersPerPixel;
			double halfMeters = totalMeters / 2;

			//do meters calculation in VE space
			//the 0,0 origin for VE is in upper left
			double N = row * (textureSizePixels * metersPerPixel);
			double W = col * (textureSizePixels * metersPerPixel);
			//now convert it to +/- meter coordinates for Proj.4
			//the 0,0 origin for Proj.4 is 0 lat, 0 lon
			//-22 to 22 million, -11 to 11 million
			N = halfMeters - N;
			W = W - halfMeters;
			double E = W + (textureSizePixels * metersPerPixel);
			double S = N - (textureSizePixels * metersPerPixel);

			UL = new UV(W, N);
			UR = new UV(E, N);
			LL = new UV(W, S);
			LR = new UV(E, S);

			// figure out latrange (for terrain detail)
			UV geoUL = quadTileSet.Proj.Inverse(UL);
			UV geoLR = quadTileSet.Proj.Inverse(LR);

			north = geoUL.V * 180 / Math.PI;
			south = geoLR.V * 180 / Math.PI;
			west = geoUL.U * 180 / Math.PI;
			east = geoLR.U * 180 / Math.PI;


			centerLatitude = Angle.FromDegrees(0.5f * (north + south));
			centerLongitude = Angle.FromDegrees(0.5f * (west + east));
			LatitudeSpan = Math.Abs(north - south);
			LongitudeSpan = Math.Abs(east - west);

			this.level = level;
			this.quadTileSet = quadTileSet;

			BoundingBox = new BoundingBox((float)south, (float)north, (float)west, (float)east,
														(float)quadTileSet.LayerRadius, (float)quadTileSet.LayerRadius + 300000f);
			//localOrigin = BoundingBox.CalculateCenter();
			localOrigin = MathEngine.SphericalToCartesian(centerLatitude, centerLongitude, quadTileSet.LayerRadius);

			// To avoid gaps between neighbouring tiles truncate the origin to 
			// a number that doesn't get rounded. (nearest 10km)
			localOrigin.X = (float)(Math.Round(localOrigin.X / 10000) * 10000);
			localOrigin.Y = (float)(Math.Round(localOrigin.Y / 10000) * 10000);
			localOrigin.Z = (float)(Math.Round(localOrigin.Z / 10000) * 10000);

			downloadRequests = new List<GeoSpatialDownloadRequest>();

			key = string.Format("{0,4}", this.level)
					+ "_"
					+ string.Format("{0,4}", this._Col)
					+ string.Format("{0,4}", this._Row)
					+ this.quadTileSet.Name
					+ this.quadTileSet.ParentList.Name;
		}

		public virtual void ResetCache()
		{
			try
			{
				m_isResetingCache = true;
				this.isInitialized = false;

				if (northEastChild != null)
				{
					northEastChild.ResetCache();
				}

				if (northWestChild != null)
				{
					northWestChild.ResetCache();
				}

				if (southEastChild != null)
				{
					southEastChild.ResetCache();
				}

				if (southWestChild != null)
				{
					southWestChild.ResetCache();
				}

				this.Dispose();

				if ((quadTileSet.ImageStore != null) && quadTileSet.ImageStore.IsDownloadableLayer)
					quadTileSet.ImageStore.DeleteLocalCopy(this);

				m_isResetingCache = false;
			}
			catch
			{
			}
		}

		/// <summary>
		/// Returns the VEQuadTile for specified location if available.
		/// Tries to queue a download if not available.
		/// </summary>
		/// <returns>Initialized VEQuadTile if available locally, else null.</returns>
		private VEQuadTile ComputeChild(int row, int col)
		{
			VEQuadTile child = new VEQuadTile(row, col, this.level + 1, quadTileSet);

			return child;
		}

		public virtual void ComputeChildren(DrawArgs drawArgs)
		{
			if (level + 1 >= quadTileSet.ImageStore.LevelCount)
				return;

			int iSubRow = Row * 2;
			int iSubCol = Col * 2;

			if (southWestChild == null)
				southWestChild = ComputeChild(iSubRow, iSubCol);

			if (southEastChild == null)
				southEastChild = ComputeChild(iSubRow, iSubCol + 1);

			if (northWestChild == null)
				northWestChild = ComputeChild(iSubRow + 1, iSubCol);

			if (northEastChild == null)
				northEastChild = ComputeChild(iSubRow + 1, iSubCol + 1);
		}

		public virtual void Dispose()
		{
			try
			{
				isInitialized = false;
				if (texture != null)
				{
					texture.Dispose();
					texture = null;
				}
			}
			catch
			{
			}
			if (northWestChild != null)
			{
				northWestChild.Dispose();
				northWestChild = null;
			}
			if (southWestChild != null)
			{
				southWestChild.Dispose();
				southWestChild = null;
			}
			if (northEastChild != null)
			{
				northEastChild.Dispose();
				northEastChild = null;
			}
			if (southEastChild != null)
			{
				southEastChild.Dispose();
				southEastChild = null;
			}
			try
			{
				if (downloadRequests != null)
				{
					foreach (GeoSpatialDownloadRequest request in downloadRequests)
					{
						quadTileSet.RemoveFromDownloadQueue(request, false);
						request.Dispose();
					}
					downloadRequests.Clear();
				}
			}
			catch
			{
			}
		}

		private bool waitingForDownload = false;
		private bool isDownloadingImage = false;

		public virtual void Initialize()
		{
			if (m_isResetingCache)
				return;

			try
			{
				if (downloadRequests.Count > 0)
				{
					// Waiting for download
					return;
				}

				// assume we're finished.
				waitingForDownload = false;

				// not entirely sure if this is a good idea...
				if (texture != null)
					texture.Dispose();

				// check for missing texture.
				texture = quadTileSet.ImageStore.LoadFile(this);
				if (texture == null)
				{
					// texture missing, wait for download
					waitingForDownload = true;
				}


				if (waitingForDownload)
					return;

				isDownloadingImage = false;
				CreateTileMesh();
			}
			//catch (Microsoft.DirectX.Direct3D.Direct3DXException)
			catch (Exception)
			{
				//Log.Write(ex);
				// Texture load failed.
			}
			finally
			{
				isInitialized = true;
			}
		}

		/// <summary>
		/// Updates this layer (background)
		/// </summary>
		public virtual void Update(DrawArgs drawArgs)
		{
			if (m_isResetingCache)
				return;

			try
			{
				double tileSize = north - south;

				if (!isInitialized)
				{
					if (DrawArgs.Camera.ViewRange * 0.5f < Angle.FromDegrees(quadTileSet.TileDrawDistance * tileSize)
				 && MathEngine.SphericalDistance(centerLatitude, centerLongitude,
																	DrawArgs.Camera.Latitude, DrawArgs.Camera.Longitude) <
							 Angle.FromDegrees(quadTileSet.TileDrawSpread * tileSize * 1.25f)
						 && DrawArgs.Camera.ViewFrustum.Intersects(BoundingBox)
						 )
						Initialize();
				}

				if (isInitialized && World.Settings.VerticalExaggeration != verticalExaggeration ||
					 m_CurrentOpacity != quadTileSet.Opacity ||
					 quadTileSet.RenderStruts != renderStruts)
				{
					CreateTileMesh();
				}

				if (isInitialized)
				{
					if (DrawArgs.Camera.ViewRange < Angle.FromDegrees(quadTileSet.TileDrawDistance * tileSize)
				 && MathEngine.SphericalDistance(centerLatitude, centerLongitude,
																	DrawArgs.Camera.Latitude, DrawArgs.Camera.Longitude) <
							 Angle.FromDegrees(quadTileSet.TileDrawSpread * tileSize)
						 && DrawArgs.Camera.ViewFrustum.Intersects(BoundingBox)
				 )
					{
						if (northEastChild == null || northWestChild == null || southEastChild == null ||
							 southWestChild == null)
						{
							ComputeChildren(drawArgs);
						}

						if (northEastChild != null)
						{
							northEastChild.Update(drawArgs);
						}

						if (northWestChild != null)
						{
							northWestChild.Update(drawArgs);
						}

						if (southEastChild != null)
						{
							southEastChild.Update(drawArgs);
						}

						if (southWestChild != null)
						{
							southWestChild.Update(drawArgs);
						}
					}
					else
					{
						if (northWestChild != null)
						{
							northWestChild.Dispose();
							northWestChild = null;
						}

						if (northEastChild != null)
						{
							northEastChild.Dispose();
							northEastChild = null;
						}

						if (southEastChild != null)
						{
							southEastChild.Dispose();
							southEastChild = null;
						}

						if (southWestChild != null)
						{
							southWestChild.Dispose();
							southWestChild = null;
						}
					}
				}

				if (isInitialized)
				{
					if (DrawArgs.Camera.ViewRange / 2 > Angle.FromDegrees(quadTileSet.TileDrawDistance * tileSize * 1.5f)
						 ||
						 MathEngine.SphericalDistance(centerLatitude, centerLongitude, DrawArgs.Camera.Latitude,
																DrawArgs.Camera.Longitude) >
						 Angle.FromDegrees(quadTileSet.TileDrawSpread * tileSize * 1.5f))
					{
						if (level != 0 || (level == 0 && !quadTileSet.AlwaysRenderBaseTiles))
							this.Dispose();
					}
				}
			}
			catch
			{
			}
		}

		private bool renderStruts = false;

		/// <summary>
		/// Builds flat or terrain mesh for current tile
		/// </summary>
		public virtual void CreateTileMesh()
		{
			if (VerticeCache.ContainsKey(key) && World.Settings.VerticalExaggeration == verticalExaggeration)
			{
				this.vertices = VerticeCache[key].vertices;
				this.indices = VerticeCache[key].indices;

				VerticeCache[key].EntryTime = DateTime.Now;

				return;
			}

			verticalExaggeration = World.Settings.VerticalExaggeration;
			m_CurrentOpacity = quadTileSet.Opacity;
			renderStruts = quadTileSet.RenderStruts;

			//if (quadTileSet.TerrainMapped && Math.Abs(verticalExaggeration) > 1e-3)
			CreateElevatedMesh();
			//else
			// JBTODO	CreateFlatMesh();

			AddToCache();
		}

		private void AddToCache()
		{
			if (!VerticeCache.ContainsKey(key))
			{
				if (VerticeCache.Count >= CacheSize)
				{
					for (int i = 0; i < 10; i++)
					{
						// Remove least recently used tile
						CacheEntry oldestTile = null;
						string k = "";
						foreach (KeyValuePair<string, CacheEntry> curEntry in VerticeCache)
						{
							if (oldestTile == null)
								oldestTile = curEntry.Value;
							else
							{
								if (curEntry.Value.EntryTime < oldestTile.EntryTime)
								{
									oldestTile = curEntry.Value;
									k = curEntry.Key;
								}
							}
						}
						VerticeCache.Remove(k);
					}
				}

				CacheEntry c = new CacheEntry();
				c.EntryTime = DateTime.Now;
				c.indices = this.indices;
				c.vertices = this.vertices;

				VerticeCache.Add(key, c);
			}
		}

		protected byte m_CurrentOpacity = 255;

		/// <summary>
		/// Create child tile terrain mesh
		/// Build the mesh with one extra vertice all around for proper normals calculations later on.
		/// Use the struts vertices to that effect. Struts are properly folded after normals calculations.
		/// </summary>
		protected int meshPointCount = 64;

		//NOTE this is a mix from Mashi's Reproject and WW for terrain
		public void CreateElevatedMesh()
		{
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

			float meshBaseRadius = (float)quadTileSet.LayerRadius;
			/*          float[,] heightData = null;
							if (_terrainAccessor != null && _veForm.IsTerrainOn == true)
							{
								 //does the +1 to help against tears between elevated tiles? - made it worse
								 //TODO not sure how to fix the tear between tiles caused by elevation?

					  // Get elevation data with one extra row and col all around the terrain
					  double degreePerSample = Math.Abs(latRange / (meshPointCount - 1));
								 TerrainTile tile = _terrainAccessor.GetElevationArray(North + degreePerSample, South - degreePerSample, West - degreePerSample, East + degreePerSample, meshPointCount + 2);
								 heightData = tile.ElevationData;
								 tile.Dispose();
								 tile = null;

                
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
			meshBaseRadius = (float) quadTileSet.LayerRadius + minimumElevation - overlap;
                
							}
			*/
			UV geo;
			Point3d pos;
			double height = 0;
			for (int i = 0; i < meshPointCount + 2; i++)
			{
				for (int j = 0; j < meshPointCount + 2; j++)
				{
					geo = quadTileSet.Proj.Inverse(curUnprojected);

					// Radians -> Degrees
					geo.U *= 180 / Math.PI;
					geo.V *= 180 / Math.PI;

					if (quadTileSet.World.TerrainAccessor != null)
					{
						//if (_veForm.IsTerrainOn == true)
						{
							//height = heightData[i, j] * verticalExaggeration;
							//original : need to fetch altitude on a per vertex basis (in VE space) to have matching tile borders (note PM)
							height = verticalExaggeration * quadTileSet.World.TerrainAccessor.GetElevationAt(geo.V, geo.U, Math.Abs(upperBound / LatitudeSpan));
						}
						//else
						//{
						//	height = 0;
						//}
					}

					pos = MathEngine.SphericalToCartesian(
						 geo.V,
						 geo.U,
						 quadTileSet.LayerRadius + height);
					int idx = i * (meshPointCount + 2) + j;
					vertices[idx].X = (float)pos.X;
					vertices[idx].Y = (float)pos.Y;
					vertices[idx].Z = (float)pos.Z;
					//double sinLat = Math.Sin(geo.V);
					//vertices[idx].Z = (float) (pos.Z * sinLat);

					vertices[idx].Tu = (j - 1) * scaleFactor;
					vertices[idx].Tv = (i - 1) * scaleFactor;
					//vertices[idx].Color = opacityColor;
					curUnprojected.U += uStep;
				}
				curUnprojected.U = UL.U - uStep;
				curUnprojected.V -= vStep;
			}
			//}

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

		private string imageFilePath = null;
		public const int textureSizePixels = 256;

		public static double MetersPerPixel(double layerRadius, int zoom)
		{
			double arc;
			arc = layerRadius / ((1 << zoom) * textureSizePixels);
			return arc;
		}

		public virtual bool Render(DrawArgs drawArgs)
		{
			m_CurrentOpacity = quadTileSet.Opacity;
			try
			{
				if (!isInitialized ||
					 this.vertices == null)
					return false;

				if (!DrawArgs.Camera.ViewFrustum.Intersects(BoundingBox))
					return false;

				bool northWestChildRendered = false;
				bool northEastChildRendered = false;
				bool southWestChildRendered = false;
				bool southEastChildRendered = false;

				if (northWestChild != null)
					if (northWestChild.Render(drawArgs))
						northWestChildRendered = true;

				if (southWestChild != null)
					if (southWestChild.Render(drawArgs))
						southWestChildRendered = true;

				if (northEastChild != null)
					if (northEastChild.Render(drawArgs))
						northEastChildRendered = true;

				if (southEastChild != null)
					if (southEastChild.Render(drawArgs))
						southEastChildRendered = true;

				if (quadTileSet.RenderFileNames &&
					 (!northWestChildRendered || !northEastChildRendered || !southWestChildRendered ||
					  !southEastChildRendered))
				{
					Point3d referenceCenter = new Point3d(
						 drawArgs.WorldCamera.ReferenceCenter.X,
						 drawArgs.WorldCamera.ReferenceCenter.Y,
						 drawArgs.WorldCamera.ReferenceCenter.Z);

					RenderDownloadRectangle(drawArgs, Color.FromArgb(255, 0, 0).ToArgb(), referenceCenter);

					Point3d cartesianPoint = MathEngine.SphericalToCartesian(
						 centerLatitude.Degrees,
						 centerLongitude.Degrees,
						 drawArgs.WorldCamera.WorldRadius + drawArgs.WorldCamera.TerrainElevation);

					if (imageFilePath != null && drawArgs.WorldCamera.ViewFrustum.ContainsPoint(cartesianPoint))
					{
						Point3d projectedPoint = drawArgs.WorldCamera.Project(cartesianPoint - referenceCenter);

						Rectangle rect = new Rectangle(
							 (int)projectedPoint.X - 100,
							 (int)projectedPoint.Y,
							 200,
							 200);

						drawArgs.defaultDrawingFont.DrawText(
							 null,
							 imageFilePath,
							 rect,
							 DrawTextFormat.WordBreak,
							 Color.Red);
					}
				}

				if (northWestChildRendered && northEastChildRendered && southWestChildRendered && southEastChildRendered)
				{
					return true;
				}

				Device device = DrawArgs.Device;

				if (texture == null || texture.Disposed)
					return false;

				device.SetTexture(0, texture);

				drawArgs.numberTilesDrawn++;


				DrawArgs.Device.Transform.World = Matrix.Translation(
					 (float)(localOrigin.X - drawArgs.WorldCamera.ReferenceCenter.X),
					 (float)(localOrigin.Y - drawArgs.WorldCamera.ReferenceCenter.Y),
					 (float)(localOrigin.Z - drawArgs.WorldCamera.ReferenceCenter.Z)
					 );

				if (!northWestChildRendered && !southWestChildRendered && !northEastChildRendered && !southEastChildRendered)
				{
					DrawArgs.Device.SetTexture(0, texture);
					DrawArgs.Device.DrawIndexedUserPrimitives(PrimitiveType.TriangleList, 0, vertices.Length, indices.Length / 3, indices, true, vertices);
				}

				DrawArgs.Device.Transform.World = ConvertDX.FromMatrix4d(DrawArgs.Camera.WorldMatrix);

				return true;
			}
			catch (DirectXException)
			{
			}
			return false;
		}

		protected CustomVertex.PositionColored[] downloadRectangle = new CustomVertex.PositionColored[5];

		/// <summary>
		/// Render a rectangle around an image tile in the specified color
		/// </summary>
		public void RenderDownloadRectangle(DrawArgs drawArgs, int color, Point3d referenceCenter)
		{
			// Render terrain download rectangle
			Point3d northWestV = MathEngine.SphericalToCartesian(north, west, quadTileSet.LayerRadius) -
										referenceCenter;
			Point3d southWestV = MathEngine.SphericalToCartesian(south, west, quadTileSet.LayerRadius) -
										referenceCenter;
			Point3d northEastV = MathEngine.SphericalToCartesian(north, east, quadTileSet.LayerRadius) -
										referenceCenter;
			Point3d southEastV = MathEngine.SphericalToCartesian(south, east, quadTileSet.LayerRadius) -
										referenceCenter;

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

			drawArgs.device.RenderState.ZBufferEnable = false;
			drawArgs.device.VertexFormat = CustomVertex.PositionColored.Format;
			drawArgs.device.TextureState[0].ColorOperation = TextureOperation.Disable;
			drawArgs.device.DrawUserPrimitives(PrimitiveType.LineStrip, 4, downloadRectangle);
			drawArgs.device.TextureState[0].ColorOperation = TextureOperation.SelectArg1;
			drawArgs.device.VertexFormat = CustomVertex.PositionNormalTextured.Format;
			drawArgs.device.RenderState.ZBufferEnable = true;
		}

		private static Effect grayscaleEffect = null;

		private void device_DeviceReset(object sender, EventArgs e)
		{
			Device device = (Device)sender;

			string outerrors = "";

			try
			{
				Assembly assembly = Assembly.GetExecutingAssembly();
				Stream stream = assembly.GetManifestResourceStream("WorldWind.Shaders.grayscale.fx");

				grayscaleEffect =
					 Effect.FromStream(
						  device,
						  stream,
						  null,
						  null,
						  ShaderFlags.None,
						  null,
						  out outerrors);

				if (outerrors != null && outerrors.Length > 0)
					Log.Write(Log.Levels.Error, outerrors);
			}
			catch (Exception ex)
			{
				Log.Write(ex);
			}
		}
		public void InitExportInfo(DrawArgs drawArgs, RenderableObject.ExportInfo info)
		{
			if (isInitialized)
			{
				info.dMaxLat = Math.Max(info.dMaxLat, this.north);
				info.dMinLat = Math.Min(info.dMinLat, this.south);
				info.dMaxLon = Math.Max(info.dMaxLon, this.east);
				info.dMinLon = Math.Min(info.dMinLon, this.west);

				info.iPixelsY = Math.Max(info.iPixelsY, (int)Math.Round((info.dMaxLat - info.dMinLat) / (this.north - this.south)) * textureSizePixels);
				info.iPixelsX = Math.Max(info.iPixelsX, (int)Math.Round((info.dMaxLon - info.dMinLon) / (this.east - this.west)) * textureSizePixels);
			}

			if (northWestChild != null && northWestChild.isInitialized)
				northWestChild.InitExportInfo(drawArgs, info);
			if (northEastChild != null && northEastChild.isInitialized)
				northEastChild.InitExportInfo(drawArgs, info);
			if (southWestChild != null && southWestChild.isInitialized)
				southWestChild.InitExportInfo(drawArgs, info);
			if (southEastChild != null && southEastChild.isInitialized)
				southEastChild.InitExportInfo(drawArgs, info);
		}

		public void ExportProcess(DrawArgs drawArgs, RenderableObject.ExportInfo expInfo)
		{
			try
			{
				bool bChildren = false;

				if (!isInitialized || texture == null)
					return;
				if (!drawArgs.WorldCamera.ViewFrustum.Intersects(BoundingBox))
					return;

				if (northWestChild != null && northWestChild.isInitialized)
				{
					northWestChild.ExportProcess(drawArgs, expInfo);
					bChildren = true;
				}

				if (northEastChild != null && northEastChild.isInitialized)
				{
					northEastChild.ExportProcess(drawArgs, expInfo);
					bChildren = true;
				}

				if (southWestChild != null && southWestChild.isInitialized)
				{
					southWestChild.ExportProcess(drawArgs, expInfo);
					bChildren = true;
				}

				if (southEastChild != null && southEastChild.isInitialized)
				{
					southEastChild.ExportProcess(drawArgs, expInfo);
					bChildren = true;
				}

				if (!bChildren && texture == null)
				{
					Image img = null;

					try
					{
						int iWidth, iHeight, iX, iY;

						GeographicBoundingBox geoBox = new GeographicBoundingBox(this.north, this.south, this.west, this.east);
						img = Image.FromFile(imageFilePath);

						iWidth = (int)Math.Round((this.east - this.west) * (double)expInfo.iPixelsX / (expInfo.dMaxLon - expInfo.dMinLon));
						iHeight = (int)Math.Round((this.north - this.south) * (double)expInfo.iPixelsY / (expInfo.dMaxLat - expInfo.dMinLat));
						iX = (int)Math.Round((this.west - expInfo.dMinLon) * (double)expInfo.iPixelsX / (expInfo.dMaxLon - expInfo.dMinLon));
						iY = (int)Math.Round((expInfo.dMaxLat - this.north) * (double)expInfo.iPixelsY / (expInfo.dMaxLat - expInfo.dMinLat));
						expInfo.gr.DrawImage(img, new Rectangle(iX, iY, iWidth, iHeight));
					}
					catch
					{
#if DEBUG
						System.Diagnostics.Debug.WriteLine("Thrown in image export");
#endif
					}
					finally
					{
						if (img != null)
							img.Dispose();
					}
				}
			}
			catch
			{
			}
		}

		#region IGeoSpatialDownloadTile Implementation
		public Angle CenterLatitude
		{
			get
			{
				return centerLatitude;
			}
		}

		public Angle CenterLongitude
		{
			get
			{
				return centerLongitude;
			}
		}

		public int Level
		{
			get
			{
				return level;
			}
		}

		public int Row
		{
			get
			{
				return _Row;
			}
		}

		public int Col
		{
			get
			{
				return _Col;
			}
		}

		/// <summary>
		/// North bound for this Tile
		/// </summary>
		public double North
		{
			get
			{
				return north;
			}
		}

		/// <summary>
		/// West bound for this Tile
		/// </summary>
		public double West
		{
			get
			{
				return west;
			}
		}

		/// <summary>
		/// South bound for this Tile
		/// </summary>
		public double South
		{
			get
			{
				return south;
			}
		}

		/// <summary>
		/// East bound for this Tile
		/// </summary>
		public double East
		{
			get
			{
				return east;
			}
		}

		public int TextureSizePixels
		{
			get
			{
				return textureSizePixels;
			}
			set
			{
			}
		}

		public IGeoSpatialDownloadTileSet TileSet
		{
			get
			{
				return quadTileSet;
			}
		}

		public List<GeoSpatialDownloadRequest> DownloadRequests
		{
			get
			{
				return downloadRequests;
			}
		}

		public string ImageFilePath
		{
			get
			{
				return imageFilePath;
			}
			set
			{
				imageFilePath = value;
			}
		}

		public bool IsDownloadingImage
		{
			get
			{
				return isDownloadingImage;
			}
			set
			{
				isDownloadingImage = value;
			}
		}

		public bool WaitingForDownload
		{
			get
			{
				return waitingForDownload;
			}
			set
			{
				waitingForDownload = value;
			}

		}

		public bool IsValidTile(string strFile)
		{
			bool bBadTile = false;

			// VE's no data tiles are exactly 1033 bytes, but we compute the MD5 Hash too just to be sure 
			// If the tile in the cache is more than a week old we will get rid of it, perhaps they get new data,
			// otherwise no texture will be created and the textures above will show instead 

			FileInfo fi = new FileInfo(strFile);
			if (fi.Length == 1033)
			{
				using (FileStream fs = new FileStream(strFile, FileMode.Open))
				{
					byte[] md5Hash = _md5Hasher.ComputeHash(fs);
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

			return bBadTile;
		}
		#endregion
	}
}
