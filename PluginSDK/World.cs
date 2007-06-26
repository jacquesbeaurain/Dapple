using System;
using System.IO;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using WorldWind.Renderable;
using WorldWind;
using WorldWind.Configuration;
using WorldWind.Terrain;

namespace WorldWind
{
	/// <summary>
	///
	/// </summary>
	public class World : RenderableObject
	{
		/// <summary>
		/// Persisted user adjustable settings.
		/// </summary>
		public static WorldSettings Settings = new WorldSettings();

		#region Private Members

		double equatorialRadius;
		// TODO: Add ellipsoid parameters to world.
		const double flattening = 6378.135;
		const double SemiMajorAxis = 6378137.0;
		const double SemiMinorAxis = 6356752.31425;
		TerrainAccessor _terrainAccessor;
		RenderableObjectList _renderableObjects;
		private System.Collections.IList onScreenMessages;
		private DateTime lastElevationUpdate = System.DateTime.Now;

		public System.Collections.IList OnScreenMessages
		{
			get 
			{
				return this.onScreenMessages;
			}
			set 
			{
				this.onScreenMessages = value;
			}
		}

		#endregion

		#region Properties
/*		public string DataDirectory
		{
			get
			{
				return this._dataDirectory;
			}
			set
			{
				this._dataDirectory = value;
			}
		} */

		/// <summary>
		/// Whether this world is planet Earth.
		/// </summary>
		public bool IsEarth
		{
			get
			{
				// HACK
				return this.Name=="Earth";
			}
		}
		#endregion

		static World()
		{
			// Don't load settings here - use LoadSettings explicitly
			//LoadSettings();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.World"/> class.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="position"></param>
		/// <param name="orientation"></param>
		/// <param name="equatorialRadius"></param>
		/// <param name="cacheDirectory"></param>
		/// <param name="terrainAccessor"></param>
		public World(string name, Point3d position, Quaternion4d orientation, double equatorialRadius,
			string cacheDirectory,
			TerrainAccessor terrainAccessor)
			: base(name, position, orientation)
		{
			this.equatorialRadius = equatorialRadius;
			this._terrainAccessor = terrainAccessor;
			this._renderableObjects = new RenderableObjectList(this.Name);
		//	this.m_WorldSurfaceRenderer = new WorldSurfaceRenderer(32, 0, this);
		}

		public void SetLayerOpacity(string category, string name, float opacity)
		{
			this.setLayerOpacity(this._renderableObjects, category, name, opacity);
		}

		private void setLayerOpacity(RenderableObject ro, string category, string name, float opacity)
		{
			foreach(string key in ro.MetaData.Keys)
			{
				if(String.Compare(key, category, true) == 0)
				{
					if(ro.MetaData[key].GetType() == typeof(String))
					{
						string curValue = ro.MetaData[key] as string;
						if(String.Compare(curValue, name, true) == 0)
						{
							ro.Opacity = (byte)(255 * opacity);
						}
					}
					break;
				}
			}

			RenderableObjectList rol = ro as RenderableObjectList;
			if (rol != null)
			{
				foreach (RenderableObject childRo in rol.ChildObjects)
					setLayerOpacity(childRo, category, name, opacity);
			}
		}

		/// <summary>
		/// Deserializes settings from specified location
		/// </summary>
		public static void LoadSettings(string directory)
		{
			try
			{
				Settings = (WorldSettings) SettingsBase.LoadFromPath(Settings, directory);
			}
			catch(Exception caught)
			{
				Utility.Log.Write(caught);
			}
		}

		public TerrainAccessor TerrainAccessor
		{
			get
			{
				return this._terrainAccessor;
			}
			set
			{
				this._terrainAccessor = value;
			}
		}

		public double EquatorialRadius
		{
			get
			{
				return this.equatorialRadius;
			}
		}

		public RenderableObjectList RenderableObjects
		{
			get
			{
				return this._renderableObjects;
			}
			set
			{
				this._renderableObjects = value;
			}
		}

		public override void Initialize(DrawArgs drawArgs)
		{
			try
			{
				if(this.isInitialized)
					return;

				this.RenderableObjects.Initialize(drawArgs);
			}
			catch(Exception caught)
			{
				Utility.Log.DebugWrite( caught );
			}
			finally
			{
				this.isInitialized = true;
			}
		}

		private void DrawAxis(DrawArgs drawArgs)
		{
			CustomVertex.PositionColored[] axis = new CustomVertex.PositionColored[2];
			Point3d topV = MathEngine.SphericalToCartesian(90,0,this.EquatorialRadius + 0.15f*this.EquatorialRadius);
         axis[0].X = (float)topV.X;
         axis[0].Y = (float)topV.Y;
         axis[0].Z = (float)topV.Z;

			axis[0].Color = System.Drawing.Color.Pink.ToArgb();

			Point3d botV = MathEngine.SphericalToCartesian(-90,0,this.EquatorialRadius + 0.15f*this.EquatorialRadius);
         axis[1].X = (float)botV.X;
         axis[1].Y = (float)botV.Y;
         axis[1].Z = (float)botV.Z;
			axis[1].Color = System.Drawing.Color.Pink.ToArgb();

			drawArgs.device.VertexFormat = CustomVertex.PositionColored.Format;
			drawArgs.device.TextureState[0].ColorOperation = TextureOperation.Disable;
			drawArgs.device.DrawUserPrimitives(PrimitiveType.LineStrip, 1, axis);
		}

		public override void Update(DrawArgs drawArgs)
		{
			if(!this.isInitialized)
			{
				this.Initialize(drawArgs);
			}

			if(this.RenderableObjects != null)
			{
				this.RenderableObjects.Update(drawArgs);
			}

			if(this.TerrainAccessor != null)
			{
				if(drawArgs.WorldCamera.Altitude < 300000)
				{
					if(System.DateTime.Now - this.lastElevationUpdate > TimeSpan.FromMilliseconds(500))
					{
                  drawArgs.WorldCamera.TerrainElevation = (short)this.TerrainAccessor.GetElevationAt((float)drawArgs.WorldCamera.Latitude.Degrees, (float)drawArgs.WorldCamera.Longitude.Degrees, (float)(100.0 / drawArgs.WorldCamera.ViewRange.Degrees));
						this.lastElevationUpdate = System.DateTime.Now;
					}
				}
				else
					drawArgs.WorldCamera.TerrainElevation = 0;
			}
			else
			{
				drawArgs.WorldCamera.TerrainElevation = 0;
			}
		}

      public override bool PerformSelectionAction(DrawArgs drawArgs)
		{
			return this._renderableObjects.PerformSelectionAction(drawArgs);
		}

		public override void Render(DrawArgs drawArgs)
		{
			RenderableObjects.Render(drawArgs);

			if(Settings.showPlanetAxis)
				this.DrawAxis(drawArgs);
		}

      public override void Dispose()
		{
			if(this.RenderableObjects!=null)
			{
            this.RenderableObjects.Dispose();
				this.RenderableObjects=null;
			}
		}

		/// <summary>
		/// Computes the great circle distance between two pairs of lat/longs.
		/// TODO: Compute distance using ellipsoid.
		/// </summary>
		public static Angle ApproxAngularDistance(Angle latA, Angle lonA, Angle latB, Angle lonB )
		{
			Angle dlon = lonB - lonA;
			Angle dlat = latB - latA;
			double k = Math.Sin(dlat.Radians*0.5);
			double l = Math.Sin(dlon.Radians*0.5);
			double a = k*k + Math.Cos(latA.Radians) * Math.Cos(latB.Radians) * l*l;
			double c = 2 * Math.Asin(Math.Min(1,Math.Sqrt(a)));
			return Angle.FromRadians(c);
		}

		/// <summary>
		/// Computes the distance between two pairs of lat/longs in meters.
		/// </summary>
		public double ApproxDistance(Angle latA, Angle lonA, Angle latB, Angle lonB )
		{
			double distance = equatorialRadius * ApproxAngularDistance(latA,lonA,latB,lonB).Radians;
			return distance;
		}

		/// <summary>
		/// Intermediate points on a great circle
		/// In previous sections we have found intermediate points on a great circle given either
		/// the crossing latitude or longitude. Here we find points (lat,lon) a given fraction of the
		/// distance (d) between them. Suppose the starting point is (lat1,lon1) and the final point
		/// (lat2,lon2) and we want the point a fraction f along the great circle route. f=0 is
		/// point 1. f=1 is point 2. The two points cannot be antipodal ( i.e. lat1+lat2=0 and
		/// abs(lon1-lon2)=pi) because then the route is undefined.
		/// </summary>
		/// <param name="f">Fraction of the distance for intermediate point (0..1)</param>
		public static void IntermediateGCPoint( float f, Angle lat1, Angle lon1, Angle lat2, Angle lon2, Angle d,
			out Angle lat, out Angle lon )
		{
			double sind = Math.Sin(d.Radians);
			double cosLat1 = Math.Cos(lat1.Radians);
			double cosLat2 = Math.Cos(lat2.Radians);
			double A=Math.Sin((1-f)*d.Radians)/sind;
			double B=Math.Sin(f*d.Radians)/sind;
			double x = A*cosLat1*Math.Cos(lon1.Radians) +  B*cosLat2*Math.Cos(lon2.Radians);
			double y = A*cosLat1*Math.Sin(lon1.Radians) +  B*cosLat2*Math.Sin(lon2.Radians);
			double z = A*Math.Sin(lat1.Radians) +  B*Math.Sin(lat2.Radians);
			lat = Angle.FromRadians(Math.Atan2(z,Math.Sqrt(x*x+y*y)));
			lon = Angle.FromRadians(Math.Atan2(y,x));
		}

		/// <summary>
		/// Intermediate points on a great circle
		/// In previous sections we have found intermediate points on a great circle given either
		/// the crossing latitude or longitude. Here we find points (lat,lon) a given fraction of the
		/// distance (d) between them. Suppose the starting point is (lat1,lon1) and the final point
		/// (lat2,lon2) and we want the point a fraction f along the great circle route. f=0 is
		/// point 1. f=1 is point 2. The two points cannot be antipodal ( i.e. lat1+lat2=0 and
		/// abs(lon1-lon2)=pi) because then the route is undefined.
		/// </summary>
		/// <param name="f">Fraction of the distance for intermediate point (0..1)</param>
		public Point3d IntermediateGCPoint(double f, Angle lat1, Angle lon1, Angle lat2, Angle lon2, Angle d)
		{
			double sind = Math.Sin(d.Radians);
			double cosLat1 = Math.Cos(lat1.Radians);
			double cosLat2 = Math.Cos(lat2.Radians);
			double A=Math.Sin((1-f)*d.Radians)/sind;
			double B=Math.Sin(f*d.Radians)/sind;
			double x = A*cosLat1*Math.Cos(lon1.Radians) +  B*cosLat2*Math.Cos(lon2.Radians);
			double y = A*cosLat1*Math.Sin(lon1.Radians) +  B*cosLat2*Math.Sin(lon2.Radians);
			double z = A*Math.Sin(lat1.Radians) +  B*Math.Sin(lat2.Radians);
			Angle lat=Angle.FromRadians(Math.Atan2(z,Math.Sqrt(x*x+y*y)));
			Angle lon=Angle.FromRadians(Math.Atan2(y,x));

			Point3d v = MathEngine.SphericalToCartesian(lat,lon,equatorialRadius);
			return v;
		}
	}
}
