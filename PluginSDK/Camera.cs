using System;
using System.Globalization;
using WorldWind;
using Microsoft.DirectX.Direct3D;

namespace WorldWind.Camera
{
	/// <summary>
	/// Camera base class (simple camera)
	/// </summary>
	public class CameraBase
	{
		protected short _terrainElevation;
		protected double _worldRadius;

		protected Angle _latitude;
		protected Angle _longitude;
		protected Angle _heading;
		protected Angle _tilt;
		protected Angle _bank;
		protected double _distance; // Distance from eye to target
		protected double _altitude; // Altitude above sea level
		protected Quaternion4d m_Orientation;

      public const double dEpsilonTestValue = 1e-6;

		protected Frustum _viewFrustum = new Frustum();
		protected Angle _fov = World.Settings.cameraFov;

		protected Vector3d _position;

		protected static readonly Angle minTilt  = Angle.FromDegrees(0.0);
		protected static readonly Angle maxTilt = Angle.FromDegrees(85.0);
		protected static readonly double minimumAltitude = 100;
		protected static double maximumAltitude = double.MaxValue;

		protected Matrix4d m_ProjectionMatrix; // Projection matrix used in last render.
		protected Matrix4d m_ViewMatrix; // View matrix used in last render.
		protected Matrix4d m_WorldMatrix = Matrix4d.Identity;

		protected Angle viewRange;
		protected Angle trueViewRange;
		protected Viewport viewPort;

		protected int lastStepZoomTickCount;
		static Vector3d cameraUpVector = new Vector3d(0,0,1);

		// Camera Reset variables
		static int lastResetTime; // Used by Reset() to keep track type of reset.
		const int DoubleTapDelay = 3000; // Double tap max time (ms)

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.Camera.CameraBase"/> class.
		/// </summary>
		/// <param name="targetPosition"></param>
		/// <param name="radius">Planet's radius in meters</param>
		public CameraBase( Vector3d targetPosition, double radius ) 
		{
			this._worldRadius = radius;
			this._distance = 2*_worldRadius;
			this._altitude = this._distance;
			maximumAltitude = 7 * _worldRadius;
			this.m_Orientation = Quaternion4d.EulerToQuaternion(0,0,0);
		}

		public Matrix4d ViewMatrix
		{
			get
			{
				return m_ViewMatrix;
			}
		}

		public Matrix4d ProjectionMatrix
		{
			get
			{
				return m_ProjectionMatrix;
			}
		}
		public Matrix4d WorldMatrix
		{
			get
			{
				return m_WorldMatrix;
			}
		}

		public bool IsPointGoto
		{
			get { return World.Settings.cameraIsPointGoto; }
			set { World.Settings.cameraIsPointGoto = value; }
		}

		public virtual Angle Latitude
		{
			get{ return this._latitude; }
		}

		public virtual Angle Longitude
		{
			get { return this._longitude; }
		}

		public virtual Angle Tilt
		{
			get { return _tilt; }
			set
			{
				if (value > maxTilt)
					value  = maxTilt;
				else if (value < minTilt)
					value = minTilt;

				_tilt = value;
				ComputeAltitude(_distance, _tilt);
				if(_altitude < _terrainElevation*World.Settings.VerticalExaggeration+minimumAltitude)
				{
					_altitude = _terrainElevation*World.Settings.VerticalExaggeration+minimumAltitude;
					// TODO:
					ComputeTilt(_altitude, _distance);
				}
			}
		}

		public virtual Angle Bank
		{
			get { return _bank; }
			set
			{
				if(Angle.IsNaN(value))
					return;

				_bank = value;
			}
		}

		public virtual Angle Heading
		{
			get { return this._heading; }
			set { this._heading = value; }
		}

		public virtual Quaternion4d CurrentOrientation
		{
			get { return m_Orientation; }	
			set { m_Orientation = value; }
		}

		/// <summary>
		/// Altitude above sea level (meters)
		/// </summary>
		public virtual double Altitude
		{
			get { return this._altitude; }
			set
			{
				TargetAltitude = value;
			}
		}

		/// <summary>
		/// Altitude above terrain (meters)
		/// </summary>
		public virtual double AltitudeAboveTerrain
		{
			get { return this._altitude - _terrainElevation; }
		}

      /// <summary>
      /// Slerp percentage (will just be supported in certain derivatives)
      /// </summary>
      public virtual double SlerpPercentage
      {
         set
         {
         }
      }


		/// <summary>
		/// Target altitude above sea level (meters) (after travel)
		/// </summary>
		public virtual double TargetAltitude
		{
			get { return this._altitude; }
			set
			{
				if(value < _terrainElevation*World.Settings.VerticalExaggeration + minimumAltitude)
					value = _terrainElevation*World.Settings.VerticalExaggeration + minimumAltitude;
				if(value > maximumAltitude)
					value = maximumAltitude;
				this._altitude = value;
				ComputeDistance(_altitude, _tilt);
			}
		}

		public virtual short TerrainElevation
		{
			get { return this._terrainElevation; }
			set { this._terrainElevation = value; }
		}

		public virtual Angle ViewRange
		{
			get 
			{
				return viewRange;
			}
		}

		/// <summary>
		/// Angle from horizon - center earth - horizon in opposite directon
		/// </summary>
		public virtual Angle TrueViewRange
		{
			get 
			{
				return trueViewRange;
			}
		}

		/// <summary>
		/// Camera position (World XYZ coordinates)
		/// </summary>
		public virtual Vector3d Position
		{
			get{ return this._position; }
		}

		/// <summary>
		/// The planet's radius in meters
		/// </summary>
		public virtual double WorldRadius
		{
			get { return this._worldRadius;	}
			set { this._worldRadius = value; }
		}

		public Vector3d EyeDiff = Vector3d.Empty;

		public virtual void ComputeViewMatrix()
		{
			double lat1 = _latitude.Degrees;
			double lon1 = _longitude.Degrees;
			
			EyeDiff.X = MathEngine.DegreesToRadians(_latitude.Degrees - lat1);
			EyeDiff.Y = MathEngine.DegreesToRadians(_longitude.Degrees - lon1);
			
			Vector3d defaultPosition = MathEngine.SphericalToCartesian(lat1,lon1,_worldRadius);
			
			m_ViewMatrix = Matrix4d.LookAtRH(
				defaultPosition,
				Vector3d.Empty,
				cameraUpVector );

			m_ViewMatrix *= Matrix4d.Translation(0,0, _worldRadius);
			m_ViewMatrix *= Matrix4d.RotationYawPitchRoll(-EyeDiff.Y, EyeDiff.X, 0);
			m_ViewMatrix *= Matrix4d.Translation(0,0, -_worldRadius);
			
			m_ViewMatrix *= Matrix4d.RotationYawPitchRoll(
				0,
				-_tilt.Radians,
				this._heading.Radians);
			m_ViewMatrix *= Matrix4d.Translation(0,0,(-this._distance));
			m_ViewMatrix *= Matrix4d.RotationZ(this._bank.Radians);
			
			// Extract camera position
			Matrix4d cam = Matrix4d.Invert(m_ViewMatrix);
			_position = new Vector3d(cam.M41, cam.M42, cam.M43);
		}

		/// <summary>
		/// Field of view (degrees)
		/// </summary>
		public virtual Angle Fov
		{
			get{ return this._fov; }
			set
			{
				if(value > World.Settings.cameraFovMax)
					value = World.Settings.cameraFovMax;
				if(value < World.Settings.cameraFovMin)
					value = World.Settings.cameraFovMin;
				this._fov = value;
			}
		}

		/// <summary>
		/// Distance to target position on ground.
		/// </summary>
		public virtual double Distance
		{
			get
			{
				// TODO: Accurate altitude / distance calculations
				return _distance - _terrainElevation;
			}
			set
			{
				TargetDistance = value;
			}
		}

		/// <summary>
		/// Distance to target position on ground (after traveling to target)
		/// </summary>
		public virtual double TargetDistance
		{
			get
			{
				return _distance;
			}
			set
			{
				if(value < minimumAltitude + _terrainElevation*World.Settings.VerticalExaggeration)
					value = minimumAltitude+TerrainElevation*World.Settings.VerticalExaggeration;
				if(value > maximumAltitude)
					value = maximumAltitude;
				_distance = value;
				ComputeAltitude(_distance, _tilt);
			}
		}

		public virtual Frustum ViewFrustum
		{
			get { return this._viewFrustum; }
		}

		public virtual void Update(Device device)
		{
		   this.viewPort = device.Viewport;

			Vector3d p = Quaternion4d.QuaternionToEuler(m_Orientation);

			if(!double.IsNaN(p.Y))
				this._latitude.Radians = p.Y;
			if(!double.IsNaN(p.X))
				this._longitude.Radians = p.X;
			if(!double.IsNaN(p.Z))
				this._heading.Radians = p.Z;

			m_Orientation = Quaternion4d.EulerToQuaternion(_longitude.Radians, _latitude.Radians, _heading.Radians);

		   ComputeProjectionMatrix(viewPort);
			ComputeViewMatrix();
         device.Transform.Projection = ConvertDX.FromMatrix4d(m_ProjectionMatrix);
         device.Transform.View = ConvertDX.FromMatrix4d(m_ViewMatrix);
         device.Transform.World = ConvertDX.FromMatrix4d(m_WorldMatrix);
	
			ViewFrustum.Update(
				Matrix4d.Multiply(m_WorldMatrix,
				Matrix4d.Multiply(m_ViewMatrix, m_ProjectionMatrix)));

			// Old view range (used in quadtile logic)
			double factor = (this._altitude) / this._worldRadius;
			if(factor > 1)
				viewRange = Angle.FromRadians(Math.PI);
			else
				viewRange = Angle.FromRadians(Math.Abs(Math.Asin((this._altitude) / this._worldRadius))*2);
			

			// True view range 
			if(factor < 1)
				trueViewRange = Angle.FromRadians(Math.Abs(Math.Asin((this._distance) / this._worldRadius))*2);
			else
				trueViewRange = Angle.FromRadians(Math.PI);		
				
		}

		/// <summary>
		/// Resets the camera settings
		/// Two consecutive resets closer than DoubleTapDelay ms apart performs a full reset.
		/// </summary>
		public virtual void Reset()
		{
			Fov = World.Settings.cameraFov;

			int curTime = Environment.TickCount;
			if(curTime-lastResetTime < DoubleTapDelay)
			{
				// Was already reset (step 1) - do a full reset
				if (Angle.IsNaN(_tilt))
					_tilt.Radians = 0;
				if (Angle.IsNaN(_heading))
					_heading.Radians = 0;
				if (Angle.IsNaN(_bank))
					_bank.Radians = 0;
				this.SetPosition(double.NaN,double.NaN,0,2*this._worldRadius,0,0);
			}
			else
			{
				// Reset direction, tilt & bank
				this.SetPosition(double.NaN,double.NaN,0,double.NaN,0,0);
			}
			lastResetTime = curTime;
		}

		/// <summary>
		/// Sets camera position.
		/// </summary>
		/// <param name="lat">Latitude in decimal degrees</param>
		/// <param name="lon">Longitude in decimal degrees</param>
		/// <param name="heading">Heading in decimal degrees</param>
		/// <param name="_altitude">Altitude in meters</param>
		/// <param name="tilt">Tilt in decimal degrees</param>
		public virtual void PointGoto(double lat, double lon )
		{
			if(!World.Settings.cameraIsPointGoto)
				return;

			SetPosition( lat,lon,double.NaN,double.NaN,double.NaN,double.NaN );
		}

		/// <summary>
		/// Sets camera position.
		/// </summary>
		/// <param name="lat">Latitude in decimal degrees</param>
		/// <param name="lon">Longitude in decimal degrees</param>
		/// <param name="heading">Heading in decimal degrees</param>
		/// <param name="_altitude">Altitude in meters</param>
		/// <param name="tilt">Tilt in decimal degrees</param>
		public virtual void PointGoto(Angle lat, Angle lon )
		{
			if(!World.Settings.cameraIsPointGoto)
				return;

			SetPosition( lat.Degrees,lon.Degrees,double.NaN,double.NaN,double.NaN,double.NaN );
		}

		/// <summary>
		/// Sets camera position.
		/// </summary>
		/// <param name="lat">Latitude in decimal degrees</param>
		/// <param name="lon">Longitude in decimal degrees</param>
		/// <param name="heading">Heading in decimal degrees</param>
		/// <param name="_altitude">Altitude in meters</param>
		/// <param name="tilt">Tilt in decimal degrees</param>
		public virtual void SetPosition(double lat, double lon )
		{
			SetPosition( lat,lon,0,double.NaN,0,0 );
		}

		/// <summary>
		/// Sets camera position.
		/// </summary>
		/// <param name="lat">Latitude in decimal degrees</param>
		/// <param name="lon">Longitude in decimal degrees</param>
		/// <param name="heading">Heading in decimal degrees</param>
		/// <param name="_altitude">Altitude above ground level in meters</param>
		/// <param name="tilt">Tilt in decimal degrees</param>
		public virtual void SetPosition(double lat, double lon, double heading, double _altitude, double tilt )
		{
			SetPosition( lat,lon,heading,_altitude,tilt,0);
		}

		/// <summary>
		/// Sets camera position.
		/// </summary>
		/// <param name="lat">Latitude in decimal degrees</param>
		/// <param name="lon">Longitude in decimal degrees</param>
		/// <param name="heading">Heading in decimal degrees</param>
		/// <param name="_altitude">Altitude above ground level in meters</param>
		/// <param name="tilt">Tilt in decimal degrees</param>
		/// <param name="bank">Camera bank (roll) in decimal degrees</param>
		public virtual void SetPosition(double lat, double lon, double heading, double _altitude, double tilt, double bank)
		{
			if(double.IsNaN(lat)) lat = this._latitude.Degrees;
			if(double.IsNaN(lon)) lon = this._longitude.Degrees;
			if(double.IsNaN(heading)) heading = this._heading.Degrees;
			if(double.IsNaN(bank))  bank = this._bank.Degrees;

			m_Orientation = Quaternion4d.EulerToQuaternion(
				MathEngine.DegreesToRadians(lon),
				MathEngine.DegreesToRadians(lat),
				MathEngine.DegreesToRadians(heading));

			Vector3d p = Quaternion4d.QuaternionToEuler(m_Orientation);

			_latitude.Radians = p.Y;
			_longitude.Radians = p.X;
			_heading.Radians = p.Z;

			if(!double.IsNaN(tilt)) 
				Tilt = Angle.FromDegrees(tilt);
			if(!double.IsNaN(_altitude)) 
				this.Altitude = _altitude;
			this.Bank = Angle.FromDegrees(bank);
		}
			
		/// <summary>
		/// Calculates latitude/longitude for given screen coordinate.
		/// </summary>
		public virtual void PickingRayIntersection(
			int screenX, 
			int screenY,
			out Angle latitude,
			out Angle longitude)
		{
			Vector3d v1 = Vector3d.Empty;
			v1.X = screenX;
			v1.Y = screenY;
			v1.Z = viewPort.MinZ;
			v1.Unproject(viewPort, m_ProjectionMatrix, m_ViewMatrix, m_WorldMatrix);

			Vector3d v2 = Vector3d.Empty;
			v2.X = screenX;
			v2.Y = screenY;
			v2.Z = viewPort.MaxZ;
			v2.Unproject(viewPort, m_ProjectionMatrix, m_ViewMatrix, m_WorldMatrix);

			Vector3d p1 = new Vector3d(v1.X, v1.Y, v1.Z);
			Vector3d p2 = new Vector3d(v2.X, v2.Y, v2.Z);

			double a = (p2.X - p1.X) * (p2.X - p1.X) + (p2.Y - p1.Y) * (p2.Y - p1.Y) + (p2.Z - p1.Z) * (p2.Z - p1.Z);
			double b = 2.0*((p2.X - p1.X)*(p1.X) + (p2.Y - p1.Y)*(p1.Y) + (p2.Z - p1.Z)*(p1.Z));
			double c = p1.X*p1.X + p1.Y*p1.Y + p1.Z*p1.Z - _worldRadius * _worldRadius;

			double discriminant = b*b - 4 * a * c;
			if(discriminant <= 0)
			{
				latitude = Angle.NaN;
				longitude = Angle.NaN;
				return;
			}

			double t1 = ((-1.0) * b - Math.Sqrt(b*b - 4 * a * c)) / (2*a);

			Vector3d i1 = new Vector3d(p1.X + t1*(p2.X - p1.X), p1.Y + t1*(p2.Y - p1.Y), p1.Z + t1 *(p2.Z - p1.Z));

			Vector3d i1t = MathEngine.CartesianToSpherical(i1.X, i1.Y, i1.Z);
			Vector3d mousePointer = i1t;

			latitude = Angle.FromRadians(mousePointer.Y);
			longitude = Angle.FromRadians(mousePointer.Z);
		}

		/// <summary>
		///  Calculates the projection transformation matrix, which transforms 3-D camera or 
		///  view space coordinates into 2-D screen coordinates.
		/// </summary>
		protected virtual void ComputeProjectionMatrix(Viewport viewport)
		{
         double aspectRatio = (double)viewport.Width / (double)viewport.Height;
			double zNear = this._altitude*0.1f;
			double distToCenterOfPlanet = (this._altitude + this.WorldRadius);
			double tangentalDistance  = Math.Sqrt( distToCenterOfPlanet*distToCenterOfPlanet - _worldRadius*_worldRadius);
			m_ProjectionMatrix = Matrix4d.PerspectiveFovRH(_fov.Radians, aspectRatio, zNear, tangentalDistance );
		}

		public virtual void RotationYawPitchRoll(Angle yaw, Angle pitch, Angle roll)
		{
			m_Orientation = Quaternion4d.EulerToQuaternion(yaw.Radians, pitch.Radians, roll.Radians) * m_Orientation;

			Vector3d p = Quaternion4d.QuaternionToEuler(m_Orientation);
			if(!double.IsNaN(p.Y))
				_latitude.Radians = p.Y;
			if(!double.IsNaN(p.X))
				_longitude.Radians = p.X;
			if(Math.Abs(roll.Radians) > double.Epsilon)
				_heading.Radians = p.Z;
		}

		/// <summary>
		/// Digital zoom (keyboard/mouse wheel style)
		/// </summary>
		/// <param name="ticks">Positive value for zoom in, negative for zoom out.</param>
		public virtual void ZoomStepped(double ticks)
		{
			int currentTickCount = Environment.TickCount;

			double factor = World.Settings.cameraZoomStepFactor;
			if(factor<0)
				factor = 0;
			if (factor > 1)
				factor = 1;

			double minTime = 50;  // <= 50ms: fastest
			double maxTime = 250; // >=250ms: slowest
			double time = currentTickCount - lastStepZoomTickCount;
			if (time<minTime)
				time = minTime;
			double multiplier = 1-Math.Abs( (time-minTime)/maxTime ); // Range: 1 .. 2
			if(multiplier<0)
				multiplier=0;

			multiplier= multiplier * World.Settings.cameraZoomAcceleration;
			double mulfac = Math.Pow(1 - factor, multiplier+1 );
			mulfac = Math.Pow(mulfac, Math.Abs(ticks));

			if (ticks > 0)
				TargetDistance *= mulfac;
			else
				TargetDistance /= mulfac;

			lastStepZoomTickCount = currentTickCount;
		}

		/// <summary>
		/// Zoom camera in/out (distance) 
		/// </summary>
		/// <param name="percent">Positive value = zoom in, negative=out</param>
		public virtual void Zoom(double percent)
		{
			if(percent>0)
				TargetDistance /= 1.0f + percent;
			else
				TargetDistance *= 1.0f - percent;
		}

		/// <summary>
		/// Pan the camera using delta values
		/// </summary>
		/// <param name="lat">Latitude offset</param>
		/// <param name="lon">Longitude offset</param>
		public virtual void Pan(Angle lat, Angle lon)
		{
			if(Angle.IsNaN(lat)) lat = this._latitude;
			if(Angle.IsNaN(lon)) lon = this._longitude;
			lat += _latitude;
			lon += _longitude;

		//	this._orientation = MathEngine.EulerToQuaternion(
		//		lon.Radians,
		//		lat.Radians,
		//		_heading.Radians);

			m_Orientation = Quaternion4d.EulerToQuaternion(
				lon.Radians, lat.Radians, _heading.Radians);

			Vector3d p = Quaternion4d.QuaternionToEuler(m_Orientation);

		//	Vector3d v = MathEngine.QuaternionToEuler(this._orientation);
		//	if(!double.IsNaN(v.Y))
		//	{
		//		this._latitude.Radians = v.Y;
		//		this._longitude.Radians = v.X;
		//	}

			if(!double.IsNaN(p.Y))
			{
				_latitude.Radians = p.Y;
				_longitude.Radians = p.X;
			}
		}
	
		protected void ComputeDistance( double altitude, Angle tilt )
		{
			double cos = Math.Cos(Math.PI-tilt.Radians);
			double x = _worldRadius*cos;
			double hyp = _worldRadius+altitude;
			double y = Math.Sqrt(_worldRadius*_worldRadius*cos*cos+hyp*hyp-_worldRadius*_worldRadius);
			double res = x-y;
			if(res<0)
				res = x+y;
			_distance = res;
		}

		protected void ComputeAltitude( double distance, Angle tilt )
		{
			double dfromeq = Math.Sqrt(_worldRadius*_worldRadius + distance*distance - 
				2 * _worldRadius*distance*Math.Cos(Math.PI-tilt.Radians));
			double alt = dfromeq - _worldRadius;
			if(alt<minimumAltitude + _terrainElevation*World.Settings.VerticalExaggeration)
				alt = minimumAltitude + _terrainElevation*World.Settings.VerticalExaggeration;
			else if(alt>maximumAltitude)
				alt = maximumAltitude;
			_altitude = alt;
		}

		protected void ComputeTilt(double alt, double distance )
		{
			double a = _worldRadius+alt;
			double b = distance;
			double c = _worldRadius;
			_tilt.Radians = Math.Acos((a*a+b*b-c*c)/(2*a*b));
		}
      
		/// <summary>
		/// Projects a point from world to screen coordinates.
		/// </summary>
		/// <param name="point">Point in world space</param>
		/// <returns>Point in screen space</returns>
		public Vector3d Project(Vector3d point)
		{
			point.Project(viewPort, m_ProjectionMatrix, m_ViewMatrix, m_WorldMatrix);
			return point;
		}

		public override string ToString()
		{
			string res = string.Format(CultureInfo.InvariantCulture,
				"Altitude: {6:f0}m\nView Range: {0}\nHeading: {1}\nTilt: {2}\nFOV: {7}\nPosition: ({3}, {4} @ {5:f0}m)",
				ViewRange, _heading, _tilt,
				_latitude,_longitude, _distance, _altitude, _fov);
			return res;
		}
	}
}
