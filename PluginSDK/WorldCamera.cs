using System;
using System.Drawing;
using WorldWind;
using Microsoft.DirectX.Direct3D;

namespace WorldWind.Camera
{
	/// <summary>
	/// The "normal" camera
	/// </summary>
	public class WorldCamera : CameraBase
	{
		protected Angle _targetLatitude;
		protected Angle _targetLongitude;
		protected double _targetAltitude;
		protected double _targetDistance;
      protected double _slerpPercentage = 1.0;
		protected Angle _targetHeading;
		protected Angle _targetBank;
		protected Angle _targetTilt;
		protected Angle _targetFov;
		protected Quaternion4d _targetOrientation;
      protected bool _firstUpdate = true;
		protected bool m_blForceRender = false;

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.Camera.WorldCamera"/> class.
		/// </summary>
		/// <param name="targetPosition"></param>
		/// <param name="radius"></param>
		internal WorldCamera( Point3d targetPosition,double radius ) : base( targetPosition, radius ) 
		{
			this._targetOrientation = m_Orientation;
			this._targetDistance = this._distance;
			this._targetAltitude = this._altitude;
			this._targetTilt = this._tilt;
			this._targetFov = this._fov;
		}

		public override void SetPosition(double lat, double lon, double heading, double _altitude, double tilt, double bank)
		{
			if(double.IsNaN(lat)) lat = this._latitude.Degrees;
			if(double.IsNaN(lon)) lon = this._longitude.Degrees;
			if(double.IsNaN(heading)) heading = this._heading.Degrees;
			if(double.IsNaN(bank)) bank = _targetBank.Degrees;

			this._targetOrientation = Quaternion4d.RotationYawPitchRoll(
				MathEngine.DegreesToRadians(lon),
				MathEngine.DegreesToRadians(lat),
				MathEngine.DegreesToRadians(heading));

			Point3d v = Quaternion4d.QuaternionToEuler(this._targetOrientation);
			this._targetLatitude.Radians = v.Y;
			this._targetLongitude.Radians = v.X;
			this._targetHeading.Radians = v.Z;

			if(!double.IsNaN(tilt))
				this.Tilt = Angle.FromDegrees(tilt);
			if(!double.IsNaN(_altitude)) 
				Altitude = _altitude;
			this.Bank = Angle.FromDegrees(bank);
		}

		public override void SetPositionImmediate(double lat, double lon, double heading, double alt, double tilt, double bank)
		{
			this.SetPosition(lat, lon, heading, alt, tilt, bank);
			NoSlerpToTargetOrientation();
			m_blForceRender = true;
		}

		public bool ForceRender
		{
			get
			{
				if (m_blForceRender)
				{
					m_blForceRender = false;
					return true;
				}
				else return false;
			}
		}

		public override Angle Heading
		{
			get { return _heading; }
			set
			{
				_heading = value;
				_targetHeading = value;
			}
		}

		Angle angle = new Angle();
		protected void SlerpToTargetOrientation(double percent)
		{
			double c = Quaternion4d.Dot(m_Orientation, _targetOrientation);

			if (c > 1.0)
				c = 1.0;
			else if (c < -1.0)
				c = -1.0;

			angle = Angle.FromRadians(Math.Acos(c));

			m_Orientation = Quaternion4d.Slerp(m_Orientation, _targetOrientation, percent);
		
			_tilt += (_targetTilt - _tilt)*percent;
			_bank += (_targetBank - _bank)*percent;
			_distance += (_targetDistance - _distance)*percent;
			ComputeAltitude(_distance, _tilt);
			_fov += (_targetFov - _fov)*percent;
		}

      protected void NoSlerpToTargetOrientation()
      {
         m_Orientation = this._targetOrientation;
         this._tilt = this._targetTilt;
         this._bank = this._targetBank;
         this._distance = this._targetDistance;
         ComputeAltitude(this._distance, this._tilt);
         this._fov = this._targetFov;
      }

		public bool EpsilonTest()
      {
         return (m_Orientation.EpsilonTest(this._targetOrientation) &&
            Math.Abs(this._tilt.Radians - this._targetTilt.Radians) < Camera.CameraBase.dEpsilonTestValue &&
            Math.Abs(this._bank.Radians - this._targetBank.Radians) < Camera.CameraBase.dEpsilonTestValue &&
            Math.Abs(1 - (this._distance / this._targetDistance)) < 1e-3 &&
            Math.Abs(this._fov.Radians - this._targetFov.Radians) < Camera.CameraBase.dEpsilonTestValue);
		}

      protected bool ZeroTest()
      {
         return (m_Orientation == this._targetOrientation &&
            this._tilt == this._targetTilt &&
            this._bank == this._targetBank &&
            this._distance == this._targetDistance &&
            this._fov == this._targetFov);
      }
		#region Public properties

		public override double SlerpPercentage
      {
         set
         {
            _slerpPercentage = Math.Min(1.0, value);
         }
      }

		internal override double TargetAltitude
		{
			get
			{
				return this._targetAltitude;
			}
			set
			{
				if(value < _terrainElevationUnderCamera * World.Settings.VerticalExaggeration + minimumAltitude)
                    value = _terrainElevationUnderCamera * World.Settings.VerticalExaggeration + minimumAltitude;
				if(value > maximumAltitude)
                    value = maximumAltitude;
				this._targetAltitude = value;
				ComputeTargetDistance(this._targetAltitude, this._targetTilt);
			}
		}

		public override Angle Fov
		{
			get { return this._targetFov; }
			set { 
				if(value > World.Settings.cameraFovMax)
					value = World.Settings.cameraFovMax;
				if(value < World.Settings.cameraFovMin)
					value = World.Settings.cameraFovMin;
				this._targetFov = value; 
			}
		}

		#endregion


		#region ICamera interface

		public override void Update(Device device)
		{
            // Move camera
			if (EpsilonTest())
            NoSlerpToTargetOrientation();
         else
            SlerpToTargetOrientation(World.Settings.cameraSlerpPercentage);
            // Check for terrain collision
            if (_altitude < _terrainElevationUnderCamera * World.Settings.VerticalExaggeration + minimumAltitude)
         {
                _targetAltitude = _terrainElevationUnderCamera * World.Settings.VerticalExaggeration + minimumAltitude;
                _altitude = _targetAltitude;
				//ComputeTargetDistance( _targetAltitude, _targetTilt );
                ComputeTargetTilt(_targetAltitude, _distance);
                if (Angle.IsNaN(_targetTilt))
                {
                    _targetTilt.Degrees = 0;
                    ComputeTargetDistance(_targetAltitude, _targetTilt);
                    _distance = _targetDistance;
                }
                _tilt = _targetTilt;
			}
            // Update camera base
         base.Update(device);
      }

		#endregion

		public override string ToString()
		{
			string res = base.ToString() + 
				string.Format(
				"\nTarget: ({0}, {1} @ {2:f0}m)\nTarget Altitude: {3:f0}m",
				_targetLatitude, _targetLongitude, _targetDistance, _targetAltitude ) + "\nAngle: " + angle;
			return res;
		}

		public override void RotationYawPitchRoll(Angle yaw, Angle pitch, Angle roll)
		{
			_targetOrientation = Quaternion4d.RotationYawPitchRoll(yaw.Radians, pitch.Radians, roll.Radians) * _targetOrientation;
			
			Point3d v = Quaternion4d.QuaternionToEuler(_targetOrientation);
			if(!double.IsNaN(v.Y))
				this._targetLatitude.Radians = v.Y;
			if(!double.IsNaN(v.X))
				this._targetLongitude.Radians = v.X;
			if(Math.Abs(roll.Radians)>double.Epsilon)
				this._targetHeading.Radians = v.Z;
		}

		public override Angle Bank
		{
			get { return _targetBank; }
			set
			{
				if(Angle.IsNaN(value))
					return;

				_targetBank = value;
				if(!World.Settings.cameraSmooth)
					_bank = value;
			}
		}

		public override Angle Tilt
		{
			get { return _targetTilt; }
			set
			{
				if (value > maxTilt)
					value  = maxTilt;
				else if (value < minTilt)
					value = minTilt;

				_targetTilt = value;
				ComputeTargetAltitude(_targetDistance, _targetTilt);
				if(!World.Settings.cameraSmooth)
					_tilt = value;
			}
		}

		public override double TargetDistance
		{
			get
			{
				return _targetDistance;
			}
			set
			{
				if(value < minimumAltitude)
					value = minimumAltitude;
				if(value>maximumAltitude)
					value = maximumAltitude;
				_targetDistance = value;
				ComputeTargetAltitude(_targetDistance, _targetTilt );
				if(!World.Settings.cameraSmooth)
				{
					base._distance =  _targetDistance;
 					base._altitude =  _targetAltitude;
				}
			}
		}
	
		/// <summary>
		/// Zoom camera in/out (distance) 
		/// </summary>
		/// <param name="percent">Positive value = zoom in, negative=out</param>
		public override void Zoom(double percent)
		{
			if(percent>0)
			{
				// In
				double factor = 1.0 + percent;
				TargetDistance /= factor;
			}
			else
			{
				// Out
				double factor = 1.0 - percent;
                TargetDistance *= factor;
			}
		}

		protected void ComputeTargetDistanceOld( double altitude, Angle tilt )
		{
			double cos = Math.Cos(Math.PI-tilt.Radians);
			double x = _worldRadius*cos;
			double hyp = _worldRadius+altitude;
			double y = Math.Sqrt(_worldRadius*_worldRadius*cos*cos+hyp*hyp-_worldRadius*_worldRadius);
			double res = x-y;
			if(res<0)
				res = x+y;
			_targetDistance = res;
		}

        protected void ComputeTargetDistance(double altitude, Angle tilt)
        {
            double hyp = _worldRadius + altitude;
            double a = (_worldRadius + curCameraElevation) * Math.Sin(tilt.Radians);
            double b = Math.Sqrt(hyp * hyp - a * a);
            double c = (_worldRadius + curCameraElevation) * Math.Cos(tilt.Radians);
            _targetDistance = b - c;
        }

		protected void ComputeTargetAltitude( double distance, Angle tilt )
		{
            double radius = _worldRadius + this.curCameraElevation;
            double dfromeq = Math.Sqrt(radius * radius + distance * distance -
                2 * radius * distance * Math.Cos(Math.PI - tilt.Radians));
			double alt = dfromeq - _worldRadius;
			_targetAltitude = alt;
		}

        protected void ComputeTargetTilt(double alt, double distance)
        {
            double a = _worldRadius + alt;
            double b = distance;
            double c = _worldRadius + curCameraElevation;
            _targetTilt.Radians = Math.PI - Math.Acos((c * c + b * b - a * a) / (2 * c * b));
        }

	}

	/// <summary>
	/// Normal camera with MomentumCamera. (perhaps merge with the normal camera)
	/// </summary>
	public class MomentumCamera : WorldCamera
	{
		protected Angle _latitudeMomentum;
		protected Angle _longitudeMomentum;
		protected Angle _headingMomentum;
		//protected Angle _bankMomentum;

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.Camera.MomentumCamera"/> class.
		/// </summary>
		/// <param name="targetPosition"></param>
		/// <param name="radius"></param>
		public MomentumCamera(Point3d targetPosition, double radius)
			: base(targetPosition, radius) 
		{
			this._targetOrientation = m_Orientation;
			this._targetDistance = this._distance;
			this._targetAltitude = this._altitude;
			this._targetTilt = this._tilt;
		}

		public override void RotationYawPitchRoll(Angle yaw, Angle pitch, Angle roll)
		{
			if(World.Settings.cameraHasMomentum)
			{
				_latitudeMomentum += pitch/100;
				_longitudeMomentum += yaw/100;
				_headingMomentum += roll/100;
			}

			this._targetOrientation = Quaternion4d.RotationYawPitchRoll( yaw.Radians, pitch.Radians, roll.Radians ) * _targetOrientation;
			Point3d v = Quaternion4d.QuaternionToEuler(_targetOrientation);
			if(!double.IsNaN(v.Y))
			{
				this._targetLatitude.Radians = v.Y;
				this._targetLongitude.Radians = v.X;
				if(!World.Settings.cameraTwistLock)
					_targetHeading.Radians = v.Z;
			}

			base.RotationYawPitchRoll(yaw,pitch,roll);
		}

		/// <summary>
		/// Pan the camera using delta values
		/// </summary>
		/// <param name="lat">Latitude offset</param>
		/// <param name="lon">Longitude offset</param>
		public override void Pan(Angle lat, Angle lon)
		{
			if(World.Settings.cameraHasMomentum)
			{
				_latitudeMomentum += lat/100;
				_longitudeMomentum += lon/100;
			}

			if(Angle.IsNaN(lat)) lat = this._targetLatitude;
			if(Angle.IsNaN(lon)) lon = this._targetLongitude;
			lat += _targetLatitude;
			lon += _targetLongitude;

			if(Math.Abs(lat.Radians)>Math.PI/2-1e-3)
			{
				lat.Radians = Math.Sign(lat.Radians)*(Math.PI/2 - 1e-3);
			}

			this._targetOrientation = Quaternion4d.RotationYawPitchRoll(
				lon.Radians,
				lat.Radians,
				_targetHeading.Radians);

			Point3d v = Quaternion4d.QuaternionToEuler(this._targetOrientation);
			if(!double.IsNaN(v.Y))
			{
				_targetLatitude.Radians = v.Y;
				_targetLongitude.Radians = v.X;
				_targetHeading.Radians = v.Z;

				if(!World.Settings.cameraSmooth)
				{
					_latitude = _targetLatitude;
					_longitude = _targetLongitude;
					_heading = _targetHeading;
					m_Orientation = _targetOrientation;
				}
			}
		}

		public override void Update(Device device)
		{
			if(World.Settings.cameraHasMomentum)
			{
				base.RotationYawPitchRoll(
					_longitudeMomentum,
					_latitudeMomentum,
					_headingMomentum );
			}

			base.Update(device);
		}

		public override void SetPosition(double lat, double lon, double heading, double _altitude, double tilt, double bank)
		{
			_latitudeMomentum.Radians = 0;
			_longitudeMomentum.Radians = 0;
			_headingMomentum.Radians = 0;

			base.SetPosition(lat,lon,heading, _altitude, tilt, bank );
		}

		public override string ToString()
		{
			string res = base.ToString() + 
				string.Format(
				"\nMomentum: {0}, {1}, {2}",
				_latitudeMomentum, _longitudeMomentum, _headingMomentum );
			return res;
		}
	}
}