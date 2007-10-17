using System;
using System.Globalization;
using WorldWind;
using Microsoft.DirectX.Direct3D;
using WorldWind.Terrain;

namespace WorldWind.Camera
{
   /// <summary>
   /// Camera base class (simple camera)
   /// </summary>
   public class CameraBase
   {
      protected short _terrainElevation;
      protected short _terrainElevationUnderCamera;   // right under the camera
      protected double _worldRadius;

      protected Angle _latitude;
      protected Angle _longitude;
      protected Angle _heading;
      protected Angle _tilt;
      protected Angle _bank;
      protected double _distance; // Distance from eye to target
      protected double _altitude; // Altitude above sea level
      //protected Quaternion _orientation;
      protected Quaternion4d m_Orientation;

      public const double dEpsilonTestValue = 1e-6;

      protected Frustum _viewFrustum = new Frustum();
      protected Angle _fov = World.Settings.cameraFov;

      protected Point3d _position;

      protected static readonly Angle minTilt = Angle.FromDegrees(0.0);
      protected static readonly Angle maxTilt = Angle.FromDegrees(85.0);
      protected static readonly double minimumAltitude = 100;
      protected static double maximumAltitude = double.MaxValue;

      protected Matrix4d m_ProjectionMatrix = Matrix4d.Empty; // Projection matrix used in last render.
      protected Matrix4d m_ViewMatrix = Matrix4d.Empty; // View matrix used in last render.
      protected Matrix4d m_WorldMatrix = Matrix4d.Identity;

      protected Angle viewRange;
      protected Angle trueViewRange;
      protected Viewport viewPort;

      protected int lastStepZoomTickCount;
      static Point3d cameraUpVector = new Point3d(0, 0, 1);
      public Point3d ReferenceCenter = new Point3d(0, 0, 0);

      // Camera Reset variables
      static int lastResetTime; // Used by Reset() to keep track type of reset.
      const int DoubleTapDelay = 3000; // Double tap max time (ms)

      // Camera changed callback
      public event System.EventHandler CameraChanged;

      /// <summary>
      /// Initializes a new instance of the <see cref= "T:WorldWind.Camera.CameraBase"/> class.
      /// </summary>
      /// <param name="targetPosition"></param>
      /// <param name="radius">Planet's radius in meters</param>
      public CameraBase(Point3d targetPosition, double radius)
      {
         this._worldRadius = radius;
         this._distance = 2 * _worldRadius;
         this._altitude = this._distance;
         maximumAltitude = 20 * _worldRadius;
         //	this._orientation = MathEngine.EulerToQuaternion(0,0,0);
         this.m_Orientation = Quaternion4d.RotationYawPitchRoll(0, 0, 0);
      }

      public Viewport Viewport
      {
         get
         {
            return viewPort;
         }
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
         get { return this._latitude; }
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
               value = maxTilt;
            else if (value < minTilt)
               value = minTilt;

            _tilt = value;
            ComputeAltitude(_distance, _tilt);
         }
      }

      public virtual Angle Bank
      {
         get { return _bank; }
         set
         {
            if (Angle.IsNaN(value))
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
         //	get { return this._orientation; }	
         //	set { this._orientation = value; }
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
            if (value < _terrainElevationUnderCamera * World.Settings.VerticalExaggeration + minimumAltitude)
               value = _terrainElevationUnderCamera * World.Settings.VerticalExaggeration + minimumAltitude;
            if (value > maximumAltitude)
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
      public virtual short TerrainElevationUnderCamera
      {
         get { return this._terrainElevationUnderCamera; }
         set { this._terrainElevationUnderCamera = value; }
      }

      private DateTime lastElevationUpdate = System.DateTime.Now;

      public void UpdateTerrainElevation(TerrainAccessor terrainAccessor)
      {
         // Update camera terrain elevation
         if (terrainAccessor != null)
         {
            if (Altitude < 300000)
            {
               if (System.DateTime.Now - this.lastElevationUpdate > TimeSpan.FromMilliseconds(500))
               {
                  float elevation;
                  // Under camera target
                  elevation = terrainAccessor.GetCachedElevationAt(Latitude.Degrees, Longitude.Degrees);
                  TerrainElevation = float.IsNaN(elevation) ? (short)0 : (short)elevation;
                  // Under the camera itself
                  Point3d cameraPos = Position;
                  Point3d cameraCoord = MathEngine.CartesianToSpherical(cameraPos.X, cameraPos.Y, cameraPos.Z);
                  double camLat = MathEngine.RadiansToDegrees(cameraCoord.Y);
                  double camLon = MathEngine.RadiansToDegrees(cameraCoord.Z);
                  elevation = terrainAccessor.GetCachedElevationAt(camLat, camLon);
                  TerrainElevationUnderCamera = float.IsNaN(elevation) ? (short)0 : (short)elevation;
                  if (TerrainElevationUnderCamera < 0 && !World.Settings.AllowNegativeAltitude)
                     TerrainElevationUnderCamera = 0;
                  // reset timer
                  this.lastElevationUpdate = System.DateTime.Now;
               }
            }
            else
            {
               TerrainElevation = 0;
               TerrainElevationUnderCamera = 0;
            }
         }
         else
         {
            TerrainElevation = 0;
            TerrainElevationUnderCamera = 0;
         }
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
      public virtual Point3d Position
      {
         get { return this._position; }
      }

      /// <summary>
      /// The planet's radius in meters
      /// </summary>
      public virtual double WorldRadius
      {
         get { return this._worldRadius; }
         set { this._worldRadius = value; }
      }

      public Point3d EyeDiff = Point3d.Empty;

      public float curCameraElevation = 0;
      float targetCameraElevation = 0;

      public static Point3d LookFrom = new Point3d();
      public static Point3d relCameraPos = new Point3d();
      public virtual void ComputeAbsoluteMatrices()
      {
         m_absoluteWorldMatrix = Matrix4d.Identity;

			double aspectRatio = (double)viewPort.Width / viewPort.Height;
         double zNear = Math.Max(this._altitude - TerrainElevationUnderCamera, minimumAltitude) * 0.1f;
         double distToCenterOfPlanet = (this._altitude + this.WorldRadius);
         double tangentalDistance = Math.Sqrt(distToCenterOfPlanet * distToCenterOfPlanet - _worldRadius * _worldRadius);
         if (tangentalDistance < 1000000 || double.IsNaN(tangentalDistance))
            tangentalDistance = 1000000;

         m_absoluteProjectionMatrix = Matrix4d.PerspectiveFovRH(_fov.Radians, aspectRatio, zNear, /*tangentalDistance*/ _worldRadius * 0.9 + _altitude);

         m_absoluteViewMatrix = Matrix4d.LookAtRH(
            MathEngine.SphericalToCartesian(
            _latitude.Degrees,
            _longitude.Degrees,
                WorldRadius + curCameraElevation),
            Point3d.Empty,
            new Point3d(0, 0, 1));

         /*m_absoluteViewMatrix *= Matrix4d.RotationYawPitchRoll(
            0,
            -_tilt.Radians,
            this._heading.Radians);*/
         //m_absoluteViewMatrix *= Matrix4d.Translation(0, 0, (-this._distance + curCameraElevation));
         m_absoluteViewMatrix *= Matrix4d.Translation(0, 0, (-this._distance));
         m_absoluteViewMatrix *= Matrix4d.RotationZ(this._bank.Radians);
      }

      public virtual void ComputeViewMatrix()
      {
         // Compute camera elevation
         if (World.Settings.ElevateCameraLookatPoint)
         {
            int minStep = 10;
            targetCameraElevation = TerrainElevation * World.Settings.VerticalExaggeration;
            float stepToTarget = targetCameraElevation - curCameraElevation;
            if (Math.Abs(stepToTarget) > minStep)
            {
               float step = 0.05f * stepToTarget;
               if (Math.Abs(step) < minStep) step = step > 0 ? minStep : -minStep;
               curCameraElevation = curCameraElevation + step;
            }
            else curCameraElevation = targetCameraElevation;
         }
         else
         {
            curCameraElevation = 0;
         }

         // Absolute matrices
         ComputeAbsoluteMatrices();

         // needs to be double precsion
         double radius = WorldRadius + curCameraElevation;
         double radCosLat = radius * Math.Cos(_latitude.Radians);
         LookFrom = new Point3d(radCosLat * Math.Cos(_longitude.Radians),
                             radCosLat * Math.Sin(_longitude.Radians),
                             radius * Math.Sin(_latitude.Radians));

         // this constitutes a local tri-frame hovering above the sphere		
         Point3d zAxis = LookFrom; // on sphere the normal vector and position vector are the same
         zAxis.normalize();
         Point3d xAxis = Point3d.cross(cameraUpVector, zAxis);
         xAxis.normalize();
         Point3d yAxis = Point3d.cross(zAxis, xAxis);

         ReferenceCenter = MathEngine.SphericalToCartesian(
         Angle.FromRadians(Convert.ToSingle(_latitude.Radians)),
         Angle.FromRadians(Convert.ToSingle(_longitude.Radians)),
         WorldRadius);

         // Important step !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
         // In order to use single precsion rendering, we need to define a local frame (i.e. center of center tile, etc.)
         // Vector3d LocalCenter should be defined & initialized in the CameraBase class
         // Each time the camera moves, a new local center could be defined
         // The local center also has to be subtracted from all the terrain vertices!!!!
         relCameraPos = LookFrom - ReferenceCenter;

         // Important step: construct the single precision m_ViewMatrix by hand

         // We can build the m_ViewMatrix by hand
         m_ViewMatrix[0, 0] = xAxis.X;
         m_ViewMatrix[1, 0] = xAxis.Y;
         m_ViewMatrix[2, 0] = xAxis.Z;

         m_ViewMatrix[0, 1] = yAxis.X;
         m_ViewMatrix[1, 1] = yAxis.Y;
         m_ViewMatrix[2, 1] = yAxis.Z;

         m_ViewMatrix[0, 2] = zAxis.X;
         m_ViewMatrix[1, 2] = zAxis.Y;
         m_ViewMatrix[2, 2] = zAxis.Z;

         m_ViewMatrix[3, 0] = -(xAxis.X * relCameraPos.X + xAxis.Y * relCameraPos.Y + xAxis.Z * relCameraPos.Z);
         m_ViewMatrix[3, 1] = -(yAxis.X * relCameraPos.X + yAxis.Y * relCameraPos.Y + yAxis.Z * relCameraPos.Z);
         m_ViewMatrix[3, 2] = -(zAxis.X * relCameraPos.X + zAxis.Y * relCameraPos.Y + zAxis.Z * relCameraPos.Z);

         m_ViewMatrix[0, 3] = 0.0;
         m_ViewMatrix[1, 3] = 0.0;
         m_ViewMatrix[2, 3] = 0.0;
         m_ViewMatrix[3, 3] = 1.0;

         double cameraDisplacement = _distance;
         //if(cameraDisplacement < targetCameraElevation + minimumAltitude)
         //	cameraDisplacement = targetCameraElevation + minimumAltitude;

         m_ViewMatrix *= Matrix4d.RotationYawPitchRoll(
          0,
          -_tilt.Radians,
          _heading.Radians);
         //m_ViewMatrix *= Matrix4d.Translation(0, 0, (-cameraDisplacement + curCameraElevation));
         m_ViewMatrix *= Matrix4d.Translation(0, 0, (-cameraDisplacement));
         m_ViewMatrix *= Matrix4d.RotationZ(_bank.Radians);

         // Extract camera position
         Matrix4d cam = Matrix4d.Invert(m_absoluteViewMatrix);

         _position = new Point3d(cam[3, 0], cam[3, 1], cam[3, 2]);
      }

      /// <summary>
      /// Field of view (degrees)
      /// </summary>
      public virtual Angle Fov
      {
         get { return this._fov; }
         set
         {
            if (value > World.Settings.cameraFovMax)
               value = World.Settings.cameraFovMax;
            if (value < World.Settings.cameraFovMin)
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
            //return _distance - _terrainElevation;
            return _distance;
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
            if (value < minimumAltitude)
               value = minimumAltitude;
            if (value > maximumAltitude)
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
         viewPort = device.Viewport;

         Point3d p = Quaternion4d.QuaternionToEuler(m_Orientation);

         if (!double.IsNaN(p.Y))
            this._latitude.Radians = p.Y;
         if (!double.IsNaN(p.X))
            this._longitude.Radians = p.X;
         if (!double.IsNaN(p.Z))
            this._heading.Radians = p.Z;

         // Compute matrices
         ComputeProjectionMatrix(viewPort);
         ComputeViewMatrix();
         device.Transform.Projection = ConvertDX.FromMatrix4d(m_ProjectionMatrix);
         device.Transform.View = ConvertDX.FromMatrix4d(m_ViewMatrix);
         device.Transform.World = ConvertDX.FromMatrix4d(m_WorldMatrix);

         ViewFrustum.Update(
            Matrix4d.Multiply(m_absoluteWorldMatrix,
            Matrix4d.Multiply(m_absoluteViewMatrix, m_absoluteProjectionMatrix)));

         // Old view range (used in quadtile logic)
         double factor = (this._altitude) / this._worldRadius;
         if (factor > 1)
            viewRange = Angle.FromRadians(Math.PI);
         else
            viewRange = Angle.FromRadians(Math.Abs(Math.Asin((this._altitude) / this._worldRadius)) * 2);

         // True view range 
         if (factor < 1)
            trueViewRange = Angle.FromRadians(Math.Abs(Math.Asin((this._distance) / this._worldRadius)) * 2);
         else
            trueViewRange = Angle.FromRadians(Math.PI);

         if (CameraChanged != null)
         {
            if (World.Settings.cameraAltitudeMeters != Altitude ||
               World.Settings.cameraLatitude != _latitude ||
               World.Settings.cameraLongitude != _longitude ||
               World.Settings.cameraHeading != _heading ||
               World.Settings.cameraTilt != _tilt)
            {
               CameraChanged(this, new EventArgs());
            }
         }

         World.Settings.cameraAltitudeMeters = Altitude;
         World.Settings.cameraLatitude = _latitude;
         World.Settings.cameraLongitude = _longitude;
         World.Settings.cameraHeading = _heading;
         World.Settings.cameraTilt = _tilt;
      }

      /// <summary>
      /// Resets the camera settings
      /// Two consecutive resets closer than DoubleTapDelay ms apart performs a full reset.
      /// </summary>
      public virtual void Reset()
      {
         Fov = World.Settings.cameraFov;

         int curTime = Environment.TickCount;
         if (true)//(curTime - lastResetTime < DoubleTapDelay)
         {
            // Was already reset (step 1) - do a full reset
            if (Angle.IsNaN(_tilt))
               _tilt.Radians = 0;
            if (Angle.IsNaN(_heading))
               _heading.Radians = 0;
            if (Angle.IsNaN(_bank))
               _bank.Radians = 0;
            this.SetPosition(double.NaN, double.NaN, 0, 2 * this._worldRadius, 0, 0);
         }
         else
         {
            // Reset direction, tilt & bank
            this.SetPosition(double.NaN, double.NaN, 0, double.NaN, 0, 0);
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
      public virtual void PointGoto(double lat, double lon)
      {
         if (!World.Settings.cameraIsPointGoto)
            return;

         SetPosition(lat, lon, double.NaN, double.NaN, double.NaN, double.NaN);
      }

      /// <summary>
      /// Sets camera position.
      /// </summary>
      /// <param name="lat">Latitude in decimal degrees</param>
      /// <param name="lon">Longitude in decimal degrees</param>
      /// <param name="heading">Heading in decimal degrees</param>
      /// <param name="_altitude">Altitude in meters</param>
      /// <param name="tilt">Tilt in decimal degrees</param>
      public virtual void PointGoto(Angle lat, Angle lon)
      {
         if (!World.Settings.cameraIsPointGoto)
            return;

         SetPosition(lat.Degrees, lon.Degrees, double.NaN, double.NaN, double.NaN, double.NaN);
      }

      /// <summary>
      /// Sets camera position.
      /// </summary>
      /// <param name="lat">Latitude in decimal degrees</param>
      /// <param name="lon">Longitude in decimal degrees</param>
      /// <param name="heading">Heading in decimal degrees</param>
      /// <param name="_altitude">Altitude in meters</param>
      /// <param name="tilt">Tilt in decimal degrees</param>
      public virtual void SetPosition(double lat, double lon)
      {
         SetPosition(lat, lon, 0, double.NaN, 0, 0);
      }

      /// <summary>
      /// Sets camera position.
      /// </summary>
      /// <param name="lat">Latitude in decimal degrees</param>
      /// <param name="lon">Longitude in decimal degrees</param>
      /// <param name="heading">Heading in decimal degrees</param>
      /// <param name="_altitude">Altitude above ground level in meters</param>
      /// <param name="tilt">Tilt in decimal degrees</param>
      public virtual void SetPosition(double lat, double lon, double heading, double _altitude, double tilt)
      {
         SetPosition(lat, lon, heading, _altitude, tilt, 0);
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
         if (double.IsNaN(lat)) lat = this._latitude.Degrees;
         if (double.IsNaN(lon)) lon = this._longitude.Degrees;
         if (double.IsNaN(heading)) heading = this._heading.Degrees;
         if (double.IsNaN(bank)) bank = this._bank.Degrees;

         m_Orientation = Quaternion4d.RotationYawPitchRoll(
            MathEngine.DegreesToRadians(lon),
            MathEngine.DegreesToRadians(lat),
            MathEngine.DegreesToRadians(heading));

         Point3d p = Quaternion4d.QuaternionToEuler(m_Orientation);

         _latitude.Radians = p.Y;
         _longitude.Radians = p.X;
         _heading.Radians = p.Z;

         if (!double.IsNaN(tilt))
            Tilt = Angle.FromDegrees(tilt);
         if (!double.IsNaN(_altitude))
            this.Altitude = _altitude;
         this.Bank = Angle.FromDegrees(bank);
      }

      Matrix4d m_absoluteViewMatrix = Matrix4d.Identity;
      Matrix4d m_absoluteWorldMatrix = Matrix4d.Identity;
      Matrix4d m_absoluteProjectionMatrix = Matrix4d.Identity;

      public Matrix4d AbsoluteViewMatrix
      {
         get { return m_absoluteViewMatrix; }
      }

      public Matrix4d AbsoluteWorldMatrix
      {
         get { return m_absoluteWorldMatrix; }
      }

      public Matrix4d AbsoluteProjectionMatrix
      {
         get { return m_absoluteProjectionMatrix; }
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
         if (m_ProjectionMatrix == null || m_ViewMatrix == null)
         {
            latitude = Angle.FromDegrees(0);
            longitude = Angle.FromDegrees(0);
            return;
         }

         Point3d v1 = Point3d.Empty;
         v1.X = screenX;
         v1.Y = screenY;
         v1.Z = viewPort.MinZ;
         v1.Unproject(viewPort, m_absoluteProjectionMatrix, m_absoluteViewMatrix, m_absoluteWorldMatrix);

         Point3d v2 = Point3d.Empty;
         v2.X = screenX;
         v2.Y = screenY;
         v2.Z = viewPort.MaxZ;
         v2.Unproject(viewPort, m_absoluteProjectionMatrix, m_absoluteViewMatrix, m_absoluteWorldMatrix);

         Point3d p1 = new Point3d(v1.X, v1.Y, v1.Z);
         Point3d p2 = new Point3d(v2.X, v2.Y, v2.Z);

         double a = (p2.X - p1.X) * (p2.X - p1.X) + (p2.Y - p1.Y) * (p2.Y - p1.Y) + (p2.Z - p1.Z) * (p2.Z - p1.Z);
         double b = 2.0 * ((p2.X - p1.X) * (p1.X) + (p2.Y - p1.Y) * (p1.Y) + (p2.Z - p1.Z) * (p1.Z));
         double c = p1.X * p1.X + p1.Y * p1.Y + p1.Z * p1.Z - _worldRadius * _worldRadius;

         double discriminant = b * b - 4 * a * c;
         if (discriminant <= 0)
         {
            latitude = Angle.NaN;
            longitude = Angle.NaN;
            return;
         }

         //	float t0 = ((-1.0f) * b + (float)Math.Sqrt(b*b - 4 * a * c)) / (2*a);
         double t1 = ((-1.0) * b - Math.Sqrt(b * b - 4 * a * c)) / (2 * a);

         //	Vector3 i0 = new Vector3(p1.X + t0*(p2.X - p1.X), p1.Y + t0*(p2.Y - p1.Y), p1.Z + t0 *(p2.Z - p1.Z));
         Point3d i1 = new Point3d(p1.X + t1 * (p2.X - p1.X), p1.Y + t1 * (p2.Y - p1.Y), p1.Z + t1 * (p2.Z - p1.Z));

         //	Vector3 i0t = MathEngine.CartesianToSpherical(i0.X, i0.Y, i0.Z);
         Point3d i1t = MathEngine.CartesianToSpherical(i1.X, i1.Y, i1.Z);
         Point3d mousePointer = i1t;

         latitude = Angle.FromRadians(mousePointer.Y);
         longitude = Angle.FromRadians(mousePointer.Z);
      }

      /// <summary>
      /// Calculates latitude/longitude for given screen coordinate.
      /// Cast a ray to the terrain geometry (Patrick Murris - march 2007)
      /// </summary>
      public virtual void PickingRayIntersectionWithTerrain(
          int screenX,
          int screenY,
            out Angle latitude,
            out Angle longitude,
            World world)
      {
         // Get near and far points on the ray
         Point3d v1 = new Point3d(screenX, screenY, viewPort.MinZ);
         v1.Unproject(viewPort, m_absoluteProjectionMatrix, m_absoluteViewMatrix, m_absoluteWorldMatrix);
         Point3d v2 = new Point3d(screenX, screenY, viewPort.MaxZ);
         v2.Unproject(viewPort, m_absoluteProjectionMatrix, m_absoluteViewMatrix, m_absoluteWorldMatrix);
         Point3d p1 = new Point3d(v1.X, v1.Y, v1.Z);
         Point3d p2 = new Point3d(v2.X, v2.Y, v2.Z);
         // Find intersection
         RayCasting.RayIntersectionWithTerrain(p1, p2, 100, 1, out latitude, out longitude, world);
      }

      /// <summary>
      ///  Calculates the projection transformation matrix, which transforms 3-D camera or 
      ///  view space coordinates into 2-D screen coordinates.
      /// </summary>
      protected virtual void ComputeProjectionMatrix(Viewport viewport)
      {
         double aspectRatio = (double)viewport.Width / (double)viewport.Height;
         double zNear = Math.Max(this._altitude - TerrainElevationUnderCamera, minimumAltitude) * 0.1f;
         double distToCenterOfPlanet = (this._altitude + this.WorldRadius);
         double tangentalDistance = Math.Sqrt(distToCenterOfPlanet * distToCenterOfPlanet - _worldRadius * _worldRadius);
         if (tangentalDistance < 1000000 || double.IsNaN(tangentalDistance))
            tangentalDistance = 1000000;
         m_ProjectionMatrix = Matrix4d.PerspectiveFovRH(_fov.Radians, aspectRatio, zNear, tangentalDistance);
      }

      public virtual void RotationYawPitchRoll(Angle yaw, Angle pitch, Angle roll)
      {
         //	this._orientation *= MathEngine.EulerToQuaternion(yaw.Radians, pitch.Radians, roll.Radians);
         //	Vector3 v = MathEngine.QuaternionToEuler(this._orientation);

         //	if(!double.IsNaN(v.Y))
         //		this._latitude.Radians = v.Y;
         //	if(!double.IsNaN(v.X))
         //		this._longitude.Radians = v.X;
         //	if(Math.Abs(roll.Radians)>Single.Epsilon)
         //		this._heading.Radians = v.Z;


         m_Orientation = Quaternion4d.RotationYawPitchRoll(yaw.Radians, pitch.Radians, roll.Radians) * m_Orientation;

         Point3d p = Quaternion4d.QuaternionToEuler(m_Orientation);
         if (!double.IsNaN(p.Y))
            _latitude.Radians = p.Y;
         if (!double.IsNaN(p.X))
            _longitude.Radians = p.X;
         if (Math.Abs(roll.Radians) > double.Epsilon)
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
         if (factor < 0)
            factor = 0;
         if (factor > 1)
            factor = 1;

         double minTime = 50;  // <= 50ms: fastest
         double maxTime = 250; // >=250ms: slowest
         double time = currentTickCount - lastStepZoomTickCount;
         if (time < minTime)
            time = minTime;
         double multiplier = 1 - Math.Abs((time - minTime) / maxTime); // Range: 1 .. 2
         if (multiplier < 0)
            multiplier = 0;

         multiplier = multiplier * World.Settings.cameraZoomAcceleration;
         double mulfac = Math.Pow(1 - factor, multiplier + 1);
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
         if (percent > 0)
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
         if (Angle.IsNaN(lat)) lat = this._latitude;
         if (Angle.IsNaN(lon)) lon = this._longitude;
         lat += _latitude;
         lon += _longitude;

         //	this._orientation = MathEngine.EulerToQuaternion(
         //		lon.Radians,
         //		lat.Radians,
         //		_heading.Radians);

         m_Orientation = Quaternion4d.RotationYawPitchRoll(
            lon.Radians, lat.Radians, _heading.Radians);

         Point3d p = Quaternion4d.QuaternionToEuler(m_Orientation);

         //	Vector3d v = MathEngine.QuaternionToEuler(this._orientation);
         //	if(!double.IsNaN(v.Y))
         //	{
         //		this._latitude.Radians = v.Y;
         //		this._longitude.Radians = v.X;
         //	}

         if (!double.IsNaN(p.Y))
         {
            _latitude.Radians = p.Y;
            _longitude.Radians = p.X;
         }
      }

      protected void ComputeDistanceOld(double altitude, Angle tilt)
      {
         double cos = Math.Cos(Math.PI - tilt.Radians);
         double x = _worldRadius * cos;
         double hyp = _worldRadius + altitude;
         double y = Math.Sqrt(_worldRadius * _worldRadius * cos * cos + hyp * hyp - _worldRadius * _worldRadius);
         double res = x - y;
         if (res < 0)
            res = x + y;
         _distance = res;
      }

      protected void ComputeDistance(double altitude, Angle tilt)
      {
         double hyp = _worldRadius + altitude;
         double a = (_worldRadius + curCameraElevation) * Math.Sin(tilt.Radians);
         double b = Math.Sqrt(hyp * hyp - a * a);
         double c = (_worldRadius + curCameraElevation) * Math.Cos(tilt.Radians);
         _distance = b - c;
      }

      protected void ComputeAltitude(double distance, Angle tilt)
      {
         double radius = _worldRadius + this.curCameraElevation;
         double dfromeq = Math.Sqrt(radius * radius + distance * distance -
             2 * radius * distance * Math.Cos(Math.PI - tilt.Radians));
         double alt = dfromeq - _worldRadius;
         _altitude = alt;
      }

      protected void ComputeTilt(double alt, double distance)
      {
         double a = _worldRadius + alt;
         double b = distance;
         double c = _worldRadius + curCameraElevation;
         //_tilt.Radians = Math.Acos((a * a + b * b - c * c) / (2 * a * b)); // Wrong angle (PM 2007-05)
         _tilt.Radians = Math.PI - Math.Acos((c * c + b * b - a * a) / (2 * c * b));
      }

      /// <summary>
      /// Projects a point from world to screen coordinates.
      /// </summary>
      /// <param name="point">Point in world space</param>
      /// <returns>Point in screen space</returns>
      public Point3d Project(Point3d point)
      {
         point.Project(viewPort, m_ProjectionMatrix, m_ViewMatrix, m_WorldMatrix);
         return point;
      }

      public override string ToString()
      {
         string res = string.Format(CultureInfo.InvariantCulture,
            "Altitude: {6:f0}m\nView Range: {0}\nHeading: {1}\nTilt: {2}\nFOV: {7}\nPosition: ({3}, {4} @ {5:f0}m)",
            ViewRange, _heading, _tilt,
            _latitude, _longitude, _distance, _altitude, _fov);
         return res;
      }

      /// <summary>
      /// Gets the visible bounding box for the application in degrees.
      /// </summary>
      /// <returns>An array of Angles in minx.miny,maxx, maxy order</returns>
      public static Angle[] getViewBoundingBox()
      {
         /// TODO: Correct the ViewRange for non-square windows. Is is accurate horizontally
         /// but not vertically.
         Angle[] bbox = new Angle[4];

         /// HACK: need to deal with startup of World (nothing is instantiated yet)
         if (DrawArgs.Camera != null)
         {
            Angle lat = DrawArgs.Camera.Latitude;
            Angle lon = DrawArgs.Camera.Longitude;
            Angle vr = DrawArgs.Camera.ViewRange;

            Angle North = lat + (0.5 * vr);
            Angle South = lat - (0.5 * vr);
            Angle East = lon + (0.5 * vr);
            Angle West = lon - (0.5 * vr);

            //minX(West), minY(South), maxX(East), MaxY(North)
            bbox[0] = West; bbox[1] = South;
            bbox[2] = East; bbox[3] = North;
         }
         else
         {
            bbox[0] = Angle.FromDegrees(-180.0); bbox[1] = Angle.FromDegrees(-90.0);
            bbox[2] = Angle.FromDegrees(180.0); bbox[3] = Angle.FromDegrees(90.0);
         }
         return bbox;
      }
      
   }
}
