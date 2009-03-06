using System;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace WorldWind.Renderable
{
	/// <summary>
	/// Draws a latitude/longitude grid
	/// </summary>
	public class LatLongGrid : RenderableObject
	{
		/// <summary>
		/// Planet radius (constant)
		/// </summary>
		internal double WorldRadius;

		/// <summary>
		/// Grid line radius (varies, >= world radius
		/// </summary>
		protected double radius;

		/// <summary>
		/// Current planet == Earth?
		/// </summary>
		internal bool IsEarth;

		/// <summary>
		/// Lowest visible longitude
		/// </summary>
		internal int MinVisibleLongitude;

		/// <summary>
		/// Highest visible longitude
		/// </summary>
		internal int MaxVisibleLongitude;

		/// <summary>
		/// Lowest visible Latitude
		/// </summary>
		internal int MinVisibleLatitude;

		/// <summary>
		/// Highest visible Latitude
		/// </summary>
		internal int MaxVisibleLatitude;

		/// <summary>
		/// Interval in degrees between visible latitudes
		/// </summary>
		internal int LongitudeInterval;

		/// <summary>
		/// Interval in degrees between visible longitudes
		/// </summary>
		internal int LatitudeInterval;

		/// <summary>
		/// The number of visible longitude lines
		/// </summary>
		internal int LongitudePointCount;

		/// <summary>
		/// The number of visible latitude lines
		/// </summary>
		internal int LatitudePointCount;

		/// <summary>
		/// Temporary buffer used for rendering  lines
		/// </summary>
		protected CustomVertex.PositionColored[] lineVertices;

		/// <summary>
		/// Z Buffer enabled (depending on distance)
		/// </summary>
		protected bool useZBuffer;

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.Renderable.LatLongGrid"/> class.
		/// </summary>
		public LatLongGrid(World world)
			: base("1 - Grid Lines", world)
		{
			WorldRadius = world.EquatorialRadius;

			IsEarth = world.Name == "Earth";

			// Render grid lines on top of imagery
			m_renderPriority = RenderPriority.LinePaths;
		}

		#region RenderableObject

		/// <summary>
		/// Render the grid lines
		/// </summary>
		public override void Render(DrawArgs drawArgs)
		{
			if (!World.Settings.showLatLonLines)
				return;

			ComputeGridValues(drawArgs);

			float offsetDegrees = (float)drawArgs.WorldCamera.TrueViewRange.Degrees / 6;

			drawArgs.device.RenderState.ZBufferEnable = useZBuffer;

			drawArgs.device.TextureState[0].ColorOperation = TextureOperation.Disable;
			drawArgs.device.VertexFormat = CustomVertex.PositionColored.Format;
			drawArgs.device.Transform.World = Matrix.Translation(
					  (float)-drawArgs.WorldCamera.ReferenceCenter.X,
					  (float)-drawArgs.WorldCamera.ReferenceCenter.Y,
					  (float)-drawArgs.WorldCamera.ReferenceCenter.Z
					  );

			Point3d referenceCenter = new Point3d(
					  drawArgs.WorldCamera.ReferenceCenter.X,
					  drawArgs.WorldCamera.ReferenceCenter.Y,
					  drawArgs.WorldCamera.ReferenceCenter.Z);

			// Turn off light
			if (World.Settings.EnableSunShading) drawArgs.device.RenderState.Lighting = false;

			// Draw longitudes
			for (float longitude = MinVisibleLongitude; longitude < MaxVisibleLongitude; longitude += LongitudeInterval)
			{
				// Draw longitude lines
				int vertexIndex = 0;
				for (float latitude = MinVisibleLatitude; latitude <= MaxVisibleLatitude; latitude += LatitudeInterval)
				{
					Point3d pointXyz = MathEngine.SphericalToCartesian(latitude, longitude, radius);
					lineVertices[vertexIndex].X = (float)pointXyz.X;
					lineVertices[vertexIndex].Y = (float)pointXyz.Y;
					lineVertices[vertexIndex].Z = (float)pointXyz.Z;
					lineVertices[vertexIndex].Color = World.Settings.latLonLinesColor;
					vertexIndex++;
				}
				drawArgs.device.DrawUserPrimitives(PrimitiveType.LineStrip, LatitudePointCount - 1, lineVertices);

				// Draw longitude label
				float lat = (float)(drawArgs.WorldCamera.Latitude).Degrees;
				if (lat > 70)
					lat = 70;
				Point3d v = MathEngine.SphericalToCartesian(lat, (float)longitude, radius);
				if (drawArgs.WorldCamera.ViewFrustum.ContainsPoint(v))
				{
					// Make sure longitude is in -180 .. 180 range
					int longitudeRanged = (int)longitude;
					if (longitudeRanged <= -180)
						longitudeRanged += 360;
					else if (longitudeRanged > 180)
						longitudeRanged -= 360;

					string s = Math.Abs(longitudeRanged).ToString();
					if (longitudeRanged < 0)
						s += "W";
					else if (longitudeRanged > 0 && longitudeRanged < 180)
						s += "E";

					v = drawArgs.WorldCamera.Project(v - referenceCenter);
					System.Drawing.Rectangle rect = new System.Drawing.Rectangle((int)v.X + 2, (int)v.Y, 10, 10);
					drawArgs.defaultDrawingFont.DrawText(null, s, rect.Left, rect.Top, World.Settings.latLonLinesColor);
				}
			}

			// Draw latitudes
			for (float latitude = MinVisibleLatitude; latitude <= MaxVisibleLatitude; latitude += LatitudeInterval)
			{
				// Draw latitude label
				float longitude = (float)(drawArgs.WorldCamera.Longitude).Degrees + offsetDegrees;

				Point3d v = MathEngine.SphericalToCartesian(latitude, longitude, radius);
				if (drawArgs.WorldCamera.ViewFrustum.ContainsPoint(v))
				{
					v = drawArgs.WorldCamera.Project(v - referenceCenter);
					float latLabel = latitude;
					if (latLabel > 90)
						latLabel = 180 - latLabel;
					else if (latLabel < -90)
						latLabel = -180 - latLabel;
					string s = ((int)Math.Abs(latLabel)).ToString();
					if (latLabel > 0)
						s += "N";
					else if (latLabel < 0)
						s += "S";
					System.Drawing.Rectangle rect = new System.Drawing.Rectangle((int)v.X, (int)v.Y, 10, 10);
					drawArgs.defaultDrawingFont.DrawText(null, s, rect.Left, rect.Top, World.Settings.latLonLinesColor);
				}

				// Draw latitude line
				int vertexIndex = 0;
				for (longitude = MinVisibleLongitude; longitude <= MaxVisibleLongitude; longitude += LongitudeInterval)
				{
					Point3d pointXyz = MathEngine.SphericalToCartesian(latitude, longitude, radius);
					lineVertices[vertexIndex].X = (float)pointXyz.X;
					lineVertices[vertexIndex].Y = (float)pointXyz.Y;
					lineVertices[vertexIndex].Z = (float)pointXyz.Z;

					if (latitude == 0)
						lineVertices[vertexIndex].Color = World.Settings.equatorLineColor;
					else
						lineVertices[vertexIndex].Color = World.Settings.latLonLinesColor;

					vertexIndex++;
				}
				drawArgs.device.DrawUserPrimitives(PrimitiveType.LineStrip, LongitudePointCount - 1, lineVertices);
			}

			if (World.Settings.showTropicLines && IsEarth)
				RenderTropicLines(drawArgs);

			// Restore state
			drawArgs.device.Transform.World = ConvertDX.FromMatrix4d(drawArgs.WorldCamera.WorldMatrix);
			if (!useZBuffer)
				// Reset Z buffer setting
				drawArgs.device.RenderState.ZBufferEnable = true;
			if (World.Settings.EnableSunShading) drawArgs.device.RenderState.Lighting = true;
		}

		public override void Initialize(DrawArgs drawArgs)
		{
			this.isInitialized = true;
		}

		public override void Dispose()
		{
		}

		public override void Update(DrawArgs drawArgs)
		{
		}

		public override bool IsOn
		{
			get
			{
				return World.Settings.showLatLonLines;
			}
			set
			{
				World.Settings.showLatLonLines = value;
			}
		}

		#endregion

		/// <summary>
		/// Draw Tropic of Cancer, Tropic of Capricorn, Arctic and Antarctic lines 
		/// </summary>
		void RenderTropicLines(DrawArgs drawArgs)
		{
			RenderTropicLine(drawArgs, 23.439444f, "Tropic Of Cancer");
			RenderTropicLine(drawArgs, -23.439444f, "Tropic Of Capricorn");
			RenderTropicLine(drawArgs, 66.560556f, "Arctic Circle");
			RenderTropicLine(drawArgs, -66.560556f, "Antarctic Circle");
		}

		/// <summary>
		/// Draws a tropic line at specified latitude with specified label
		/// </summary>
		/// <param name="latitude">Latitude in degrees</param>
		void RenderTropicLine(DrawArgs drawArgs, float latitude, string label)
		{
			int vertexIndex = 0;
			Point3d referenceCenter = new Point3d(
					  drawArgs.WorldCamera.ReferenceCenter.X,
					  drawArgs.WorldCamera.ReferenceCenter.Y,
					  drawArgs.WorldCamera.ReferenceCenter.Z);

			for (float longitude = MinVisibleLongitude; longitude <= MaxVisibleLongitude; longitude = longitude + LongitudeInterval)
			{
				Point3d pointXyz = MathEngine.SphericalToCartesian(latitude, longitude, radius);

				lineVertices[vertexIndex].X = (float)pointXyz.X;
				lineVertices[vertexIndex].Y = (float)pointXyz.Y;
				lineVertices[vertexIndex].Z = (float)pointXyz.Z;
				lineVertices[vertexIndex].Color = World.Settings.tropicLinesColor;
				vertexIndex++;
			}
			drawArgs.device.DrawUserPrimitives(PrimitiveType.LineStrip, LongitudePointCount - 1, lineVertices);

			Point3d t1 = MathEngine.SphericalToCartesian(Angle.FromDegrees(latitude),
				drawArgs.WorldCamera.Longitude - drawArgs.WorldCamera.TrueViewRange * 0.3f * 0.5f, radius);
			if (drawArgs.WorldCamera.ViewFrustum.ContainsPoint(t1))
			{
				t1 = drawArgs.WorldCamera.Project(t1 - referenceCenter);
				drawArgs.defaultDrawingFont.DrawText(null, label, new System.Drawing.Rectangle((int)t1.X, (int)t1.Y, drawArgs.screenWidth, drawArgs.screenHeight), DrawTextFormat.NoClip, World.Settings.tropicLinesColor);
			}
		}

		/// <summary>
		/// Recalculates the grid bounds + interval values
		/// </summary>
		internal void ComputeGridValues(DrawArgs drawArgs)
		{
			double vr = drawArgs.WorldCamera.TrueViewRange.Radians;

			// Compensate for closer grid towards poles
			vr *= 1 + Math.Abs(Math.Sin(drawArgs.WorldCamera.Latitude.Radians));

			if (vr < 0.17)
				LatitudeInterval = 1;
			else if (vr < 0.6)
				LatitudeInterval = 2;
			else if (vr < 1.0)
				LatitudeInterval = 5;
			else
				LatitudeInterval = 10;

			LongitudeInterval = LatitudeInterval;

			if (drawArgs.WorldCamera.ViewFrustum.ContainsPoint(MathEngine.SphericalToCartesian(90, 0, radius)) ||
				drawArgs.WorldCamera.ViewFrustum.ContainsPoint(MathEngine.SphericalToCartesian(-90, 0, radius)))
			{
				// Pole visible, 10 degree longitude spacing forced
				LongitudeInterval = 10;
			}

			MinVisibleLongitude = LongitudeInterval >= 10 ? -180 : (int)drawArgs.WorldCamera.Longitude.Degrees / LongitudeInterval * LongitudeInterval - 18 * LongitudeInterval;
			MaxVisibleLongitude = LongitudeInterval >= 10 ? 180 : (int)drawArgs.WorldCamera.Longitude.Degrees / LongitudeInterval * LongitudeInterval + 18 * LongitudeInterval;
			MinVisibleLatitude = (int)drawArgs.WorldCamera.Latitude.Degrees / LatitudeInterval * LatitudeInterval - 9 * LatitudeInterval;
			MaxVisibleLatitude = (int)drawArgs.WorldCamera.Latitude.Degrees / LatitudeInterval * LatitudeInterval + 9 * LatitudeInterval;

			if (MaxVisibleLatitude - MinVisibleLatitude >= 180 || LongitudeInterval == 10)
			{
				MinVisibleLatitude = -90;
				MaxVisibleLatitude = 90;
			}
			LongitudePointCount = (MaxVisibleLongitude - MinVisibleLongitude) / LongitudeInterval + 1;
			LatitudePointCount = (MaxVisibleLatitude - MinVisibleLatitude) / LatitudeInterval + 1;
			int vertexPointCount = Math.Max(LatitudePointCount, LongitudePointCount);
			if (lineVertices == null || vertexPointCount > lineVertices.Length)
				lineVertices = new CustomVertex.PositionColored[Math.Max(LatitudePointCount, LongitudePointCount)];

			radius = WorldRadius;
			if (drawArgs.WorldCamera.Altitude < 0.10f * WorldRadius)
				useZBuffer = false;
			else
			{
				useZBuffer = true;
				double bRadius = WorldRadius * 1.01f;
				double nRadius = WorldRadius + 0.015f * drawArgs.WorldCamera.Altitude;

				radius = Math.Min(nRadius, bRadius);
			}
		}
	}
}
