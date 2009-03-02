using System;
using WorldWind;
using System.Drawing;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace WorldWind
{
	/// <summary>
	/// Summary description for LineFeature.
	/// </summary>
	public class LineFeature : WorldWind.Renderable.RenderableObject
	{
		#region Static Members
		#endregion

		#region Private Members
		double m_distanceAboveSurface = 0;
		protected Point3d[] m_points = null;
		CustomVertex.PositionNormalTextured[] m_wallVertices = null;

		CustomVertex.PositionColored[] m_topVertices = null;
		CustomVertex.PositionColored[] m_bottomVertices = null;
		CustomVertex.PositionColored[] m_sideVertices = null;

		Color m_lineColor = Color.Black;
		Color finalLineColor = Color.Black;
		float m_verticalExaggeration = World.Settings.VerticalExaggeration;
		double m_minimumDisplayAltitude = 0;
		double m_maximumDisplayAltitude = double.MaxValue;
		string m_imageUri = null;
		Texture m_texture = null;
		Color m_polygonColor = Color.Black;
		Color finalPolygonColor = Color.Black;
		bool m_outline = true;
		float m_lineWidth = 1.0f;
		bool m_extrude = false;
		AltitudeMode m_altitudeMode = AltitudeMode.ClampedToGround;
		protected long m_numPoints = 0;
		Vector3 m_localOrigin;
		bool m_extrudeUpwards;
		double m_extrudeHeight = 1000;
		bool m_extrudeToGround = false;
		bool m_enableLighting = false;
		#endregion

		/// <summary>
		/// Boolean indicating whether or not the line needs rebuilding.
		/// </summary>
		internal bool NeedsUpdate = true;

		/// <summary>
		/// Whether line should be extruded
		/// </summary>
		internal bool Extrude
		{
			get { return m_extrude; }
			set
			{
				m_extrude = value;
				if (m_topVertices != null)
					NeedsUpdate = true;
			}
		}

		/// <summary>
		/// Whether extrusion should be upwards
		/// </summary>
		internal bool ExtrudeUpwards
		{
			get { return m_extrudeUpwards; }
			set
			{
				m_extrudeUpwards = value;
				if (m_topVertices != null)
					NeedsUpdate = true;
			}
		}

		/// <summary>
		/// Distance to extrude
		/// </summary>
		internal double ExtrudeHeight
		{
			get { return m_extrudeHeight; }
			set
			{
				m_extrudeHeight = value;
				if (m_topVertices != null)
					NeedsUpdate = true;
			}
		}

		/// <summary>
		/// Whether line should be extruded to the ground (completely overrides other extrusion options)
		/// </summary>
		internal bool ExtrudeToGround
		{
			get { return m_extrudeToGround; }
			set
			{
				m_extrude = value;
				m_extrudeToGround = value;
				if (m_topVertices != null)
					NeedsUpdate = true;
			}
		}

		internal AltitudeMode AltitudeMode
		{
			get { return m_altitudeMode; }
			set { m_altitudeMode = value; }
		}

		internal bool EnableLighting
		{
			get { return m_enableLighting; }
			set { m_enableLighting = value; }
		}

		public System.Drawing.Color LineColor
		{
			get { return m_lineColor; }
			set
			{
				m_lineColor = value;
				NeedsUpdate = true;
			}
		}

		public float LineWidth
		{
			get { return m_lineWidth; }
			set
			{
				m_lineWidth = value;
				NeedsUpdate = true;
			}
		}

		internal double DistanceAboveSurface
		{
			get { return m_distanceAboveSurface; }
			set
			{
				m_distanceAboveSurface = value;
				if (m_topVertices != null)
				{
					NeedsUpdate = true;
				}
			}
		}

		internal System.Drawing.Color PolygonColor
		{
			get { return m_polygonColor; }
			set
			{
				m_polygonColor = value;
				if (m_topVertices != null)
				{
					NeedsUpdate = true;
					//					UpdateVertices();
				}
			}
		}

		internal bool Outline
		{
			get { return m_outline; }
			set
			{
				m_outline = value;
				if (m_topVertices != null)
				{
					NeedsUpdate = true;
					//					UpdateVertices();
				}
			}
		}

		public Point3d[] Points
		{
			get
			{
				// if the array size is correct just return it
				if (m_numPoints == m_points.LongLength)
					return m_points;

				// return an array the correct size.
				Point3d[] points = new Point3d[m_numPoints];
				for (int i = 0; i < m_numPoints; i++)
				{
					points[i] = m_points[i];
				}
				return points;
			}
			set
			{
				m_points = value;
				m_numPoints = m_points.LongLength;
				NeedsUpdate = true;
			}
		}

		public long NumPoints
		{
			get { return m_numPoints; }
		}

		public double MinimumDisplayAltitude
		{
			get { return m_minimumDisplayAltitude; }
			set { m_minimumDisplayAltitude = value; }
		}

		public double MaximumDisplayAltitude
		{
			get { return m_maximumDisplayAltitude; }
			set { m_maximumDisplayAltitude = value; }
		}

		public override byte Opacity
		{
			get
			{
				return base.Opacity;
			}
			set
			{
				base.Opacity = value;
				if (m_topVertices != null)
				{
					NeedsUpdate = true;
				}
			}
		}

		public LineFeature(string name, World parentWorld, Point3d[] points, System.Drawing.Color lineColor)
			: base(name, parentWorld)
		{
			m_points = points;
			m_lineColor = lineColor;
			m_polygonColor = lineColor;
			m_numPoints = m_points.LongLength;

			RenderPriority = WorldWind.Renderable.RenderPriority.LinePaths;
		}

		internal LineFeature(string name, World parentWorld, Point3d[] points, string imageUri)
			: base(name, parentWorld)
		{
			m_points = points;
			m_imageUri = imageUri;
			m_numPoints = m_points.LongLength;

			RenderPriority = WorldWind.Renderable.RenderPriority.LinePaths;
		}

		public override void Dispose()
		{
			if (m_texture != null && !m_texture.Disposed)
			{
				m_texture.Dispose();
				m_texture = null;
			}

			if (m_lineString != null)
			{
				m_lineString.Remove = true;
				m_lineString = null;
			}
			NeedsUpdate = true;
		}

		public override void Initialize(DrawArgs drawArgs)
		{
			if (m_points == null)
			{
				isInitialized = true;
				return;
			}

			if (m_imageUri != null)
			{
				//load image
				if (m_imageUri.ToLower().StartsWith("http://"))
				{
					string savePath = string.Format("{0}\\image", ConfigurationLoader.GetRenderablePathString(this));
					System.IO.FileInfo file = new System.IO.FileInfo(savePath);
					if (!file.Exists)
					{
						WorldWind.Net.WebDownload download = new WorldWind.Net.WebDownload(m_imageUri);

						if (!file.Directory.Exists)
							file.Directory.Create();

						download.DownloadFile(file.FullName, WorldWind.Net.DownloadType.Unspecified);
					}

					m_texture = ImageHelper.LoadTexture(file.FullName);
				}
				else
				{
					m_texture = ImageHelper.LoadTexture(m_imageUri);
				}
			}

			UpdateVertices();

			isInitialized = true;
		}

		/// <summary>
		/// Adds a point to the end of the line.
		/// </summary>
		/// <param name="x">Lon</param>
		/// <param name="y">Lat</param>
		/// <param name="z">Alt (meters)</param>
		internal void AddPoint(double x, double y, double z)
		{
			Point3d point = new Point3d(x, y, z);
			//TODO:Divide into subsegments if too far
			if (m_numPoints > 0)
			{
				Angle startlon = Angle.FromDegrees(m_points[m_numPoints - 1].X);
				Angle startlat = Angle.FromDegrees(m_points[m_numPoints - 1].Y);
				double startalt = m_points[m_numPoints - 1].Z;
				Angle endlon = Angle.FromDegrees(x);
				Angle endlat = Angle.FromDegrees(y);
				double endalt = z;

				Angle dist = World.ApproxAngularDistance(startlat, startlon, endlat, endlon);
				if (dist.Degrees > 0.25)
				{

					double stepSize = 0.25;
					int samples = (int)(dist.Degrees / stepSize);

					for (int i = 0; i < samples; i++)
					{
						Angle lat, lon = Angle.Zero;
						float frac = (float)i / samples;
						World.IntermediateGCPoint(frac, startlat, startlon, endlat, endlon,
						dist, out lat, out lon);
						double alt = startalt + frac * (endalt - startalt);
						Point3d pointint = new Point3d(lon.Degrees, lat.Degrees, alt);
						AddPoint(pointint);
					}
					AddPoint(point);
				}
				else
				{
					AddPoint(point);
				}
			}
			else
			{
				AddPoint(point);
			}

		}

		/// <summary>
		/// Adds a point to the line at the end of the line.
		/// </summary>
		/// <param name="point">The Point3d object to add.</param>
		internal void AddPoint(Point3d point)
		{
			// if the array is too small grow it.
			if (m_numPoints >= m_points.LongLength)
			{
				long growSize = m_points.LongLength / 2;
				if (growSize < 10) growSize = 10;

				Point3d[] points = new Point3d[m_points.LongLength + growSize];

				for (int i = 0; i < m_numPoints; i++)
				{
					points[i] = m_points[i];
				}
				m_points = points;
			}
			m_points[m_numPoints] = point;
			m_numPoints++;
			NeedsUpdate = true;
		}

		private void UpdateVertices()
		{
			int lineAlpha = (int)(((double)m_lineColor.A / 255 * (double)base.Opacity / 255) * 255);
			finalLineColor = Color.FromArgb(lineAlpha, m_lineColor);
			int polyAlpha = (int)(((double)m_polygonColor.A / 255 * (double)base.Opacity / 255) * 255);
			finalPolygonColor = Color.FromArgb(polyAlpha, m_polygonColor);

			try
			{
				m_verticalExaggeration = World.Settings.VerticalExaggeration;

				if (m_points.Length > 0)
				{
					UpdateTexturedVertices();
				}

				if (m_lineString != null && m_outline && m_wallVertices != null && m_wallVertices.Length > m_topVertices.Length)
				{
					UpdateOutlineVertices();
				}

				NeedsUpdate = false;
			}
			catch (Exception ex)
			{
				Utility.Log.Write(ex);
			}
		}

		private void UpdateOutlineVertices()
		{
			m_bottomVertices = new CustomVertex.PositionColored[m_numPoints];
			m_sideVertices = new CustomVertex.PositionColored[m_numPoints * 2];

			for (int i = 0; i < m_numPoints; i++)
			{
				m_sideVertices[2 * i] = m_topVertices[i];

				Vector3 xyzVertex = new Vector3(
					 m_wallVertices[2 * i + 1].X,
					 m_wallVertices[2 * i + 1].Y,
					 m_wallVertices[2 * i + 1].Z);

				m_bottomVertices[i].X = xyzVertex.X;
				m_bottomVertices[i].Y = xyzVertex.Y;
				m_bottomVertices[i].Z = xyzVertex.Z;
				m_bottomVertices[i].Color = finalLineColor.ToArgb();

				m_sideVertices[2 * i + 1] = m_bottomVertices[i];
			}
		}

		LineString m_lineString = null;
		private void UpdateTexturedVertices()
		{
			if (m_altitudeMode == AltitudeMode.ClampedToGround)
			{
				if (m_lineString != null)
				{
					m_lineString.Remove = true;
					m_lineString = null;
				}

				m_lineString = new LineString();
				m_lineString.Coordinates = Points;
				m_lineString.Color = finalLineColor;
				m_lineString.LineWidth = LineWidth;
				m_lineString.ParentRenderable = this;
				this.World.ProjectedVectorRenderer.Add(m_lineString);

				if (m_wallVertices != null)
					m_wallVertices = null;

				return;
			}

			// wall replicates vertices to account for different normal vectors.
			// TODO: would be nice to automatically figure out when NOT to separate
			// triangles (smooth shading) vs. having separate vertices (and thus normals)
			// ('hard' edges). GE doesn't do that, by the way.
			// -stepman
			if (m_extrude || m_altitudeMode != AltitudeMode.ClampedToGround)
			{
				m_wallVertices = new CustomVertex.PositionNormalTextured[m_numPoints * 4 - 2];
			}

			float textureCoordIncrement = 1.0f / (float)(m_numPoints - 1);
			m_verticalExaggeration = World.Settings.VerticalExaggeration;
			int vertexColor = finalPolygonColor.ToArgb();

			m_topVertices = new CustomVertex.PositionColored[m_numPoints];

			// precalculate feature center (at zero elev)
			Point3d center = new Point3d(0, 0, 0);
			for (int i = 0; i < m_numPoints; i++)
			{
				center += MathEngine.SphericalToCartesian(
					 Angle.FromDegrees(m_points[i].Y),
					 Angle.FromDegrees(m_points[i].X),
					 World.EquatorialRadius
					 );
			}
			center = center * (1.0 / m_numPoints);
			// round off to nearest 10^5.
			center.X = ((int)(center.X / 10000.0)) * 10000.0;
			center.Y = ((int)(center.Y / 10000.0)) * 10000.0;
			center.Z = ((int)(center.Z / 10000.0)) * 10000.0;

			m_localOrigin = center.Vector3;

			for (int i = 0; i < m_numPoints; i++)
			{
				double terrainHeight = 0;
				if (m_altitudeMode == AltitudeMode.RelativeToGround)
				{
					if (World.TerrainAccessor != null)
					{
						terrainHeight = World.TerrainAccessor.GetElevationAt(
							 m_points[i].Y,
							 m_points[i].X,
							 (100.0 / DrawArgs.Camera.ViewRange.Degrees)
							 );
					}
				}

				// polygon point
				Point3d xyzPoint = MathEngine.SphericalToCartesian(
					 Angle.FromDegrees(m_points[i].Y),
					 Angle.FromDegrees(m_points[i].X),
					 m_verticalExaggeration * (m_distanceAboveSurface + terrainHeight + m_points[i].Z) + World.EquatorialRadius
					 );

				Vector3 xyzVertex = (xyzPoint - center).Vector3;

				m_topVertices[i].X = xyzVertex.X;
				m_topVertices[i].Y = xyzVertex.Y;
				m_topVertices[i].Z = xyzVertex.Z;
				m_topVertices[i].Color = finalLineColor.ToArgb();


				if (m_extrude || m_altitudeMode != AltitudeMode.ClampedToGround)
				{
					m_wallVertices[4 * i].X = xyzVertex.X;
					m_wallVertices[4 * i].Y = xyzVertex.Y;
					m_wallVertices[4 * i].Z = xyzVertex.Z;
					//m_wallVertices[2 * i].Color = vertexColor;
					m_wallVertices[4 * i].Tu = i * textureCoordIncrement;
					m_wallVertices[4 * i].Tv = 1.0f;

					m_wallVertices[4 * i].Normal = new Vector3(0, 0, 0);

					// extruded point
					if (m_extrudeToGround)
					{
						xyzPoint = MathEngine.SphericalToCartesian(
							 Angle.FromDegrees(m_points[i].Y),
							 Angle.FromDegrees(m_points[i].X),
							 m_verticalExaggeration * (m_distanceAboveSurface + terrainHeight) + World.EquatorialRadius
							 );
					}
					else
					{
						double extrudeDist = m_extrudeHeight;
						if (!m_extrudeUpwards)
							extrudeDist *= -1;

						xyzPoint = MathEngine.SphericalToCartesian(
							 Angle.FromDegrees(m_points[i].Y),
							 Angle.FromDegrees(m_points[i].X),
							 (m_points[i].Z + extrudeDist + m_distanceAboveSurface) * m_verticalExaggeration + World.EquatorialRadius
							 );
					}

					xyzVertex = (xyzPoint - center).Vector3;


					m_wallVertices[4 * i + 1].X = xyzVertex.X;
					m_wallVertices[4 * i + 1].Y = xyzVertex.Y;
					m_wallVertices[4 * i + 1].Z = xyzVertex.Z;
					//m_wallVertices[2 * i + 1].Color = vertexColor;
					m_wallVertices[4 * i + 1].Tu = i * textureCoordIncrement;
					m_wallVertices[4 * i + 1].Tv = 0.0f;

					m_wallVertices[4 * i + 1].Normal = new Vector3(0, 0, 0);

					// replicate to previous vertex as well
					if (i > 0)
					{
						m_wallVertices[4 * i - 2] = m_wallVertices[4 * i];
						m_wallVertices[4 * i - 1] = m_wallVertices[4 * i + 1];
					}
				}
			}

			// calculate normals
			// -----------------
			// first pass -- accumulate normals
			// we start with the second index because we're looking at (i-1) to calculate normals
			// we also use the same normal for top and bottom vertices...
			for (int i = 1; i < m_numPoints; i++)
			{
				Vector3 dir = m_wallVertices[4 * i - 2].Position - m_wallVertices[4 * i - 3].Position;
				Vector3 up = m_wallVertices[4 * i - 1].Position - m_wallVertices[4 * i - 2].Position;
				Vector3 norm = Vector3.Normalize(Vector3.Cross(dir, up));
				m_wallVertices[4 * i - 4].Normal += norm;
				m_wallVertices[4 * i - 3].Normal += norm;
				m_wallVertices[4 * i - 2].Normal += norm;
				m_wallVertices[4 * i - 1].Normal += norm;
			}
			// normalize normal vectors.
			if (m_numPoints * 4 <= m_wallVertices.Length)
			{
				for (int i = 0; i < m_numPoints; i++)
				{
					m_wallVertices[4 * i + 0].Normal.Normalize();
					m_wallVertices[4 * i + 1].Normal.Normalize();
					m_wallVertices[4 * i + 2].Normal.Normalize();
					m_wallVertices[4 * i + 3].Normal.Normalize();
				}
			}
		}

		public override bool PerformSelectionAction(DrawArgs drawArgs)
		{
			return false;
		}

		public override void Update(DrawArgs drawArgs)
		{
			if (drawArgs.WorldCamera.Altitude >= m_minimumDisplayAltitude && drawArgs.WorldCamera.Altitude <= m_maximumDisplayAltitude)
			{
				if (!isInitialized)
					Initialize(drawArgs);

				if (NeedsUpdate || (m_verticalExaggeration != World.Settings.VerticalExaggeration))
					UpdateVertices();
			}

		}

		public override void Render(DrawArgs drawArgs)
		{
			using (new DirectXProfilerEvent("LineFeature::Render"))
			{
				if (!isInitialized || drawArgs.WorldCamera.Altitude < m_minimumDisplayAltitude || drawArgs.WorldCamera.Altitude > m_maximumDisplayAltitude)
				{
					return;
				}

				try
				{
					if (m_lineString != null)
						return;

					Cull currentCull = drawArgs.device.RenderState.CullMode;
					drawArgs.device.RenderState.CullMode = Cull.None;

					drawArgs.device.Transform.World = Matrix.Translation(
						 (float)-drawArgs.WorldCamera.ReferenceCenter.X + m_localOrigin.X,
						 (float)-drawArgs.WorldCamera.ReferenceCenter.Y + m_localOrigin.Y,
						 (float)-drawArgs.WorldCamera.ReferenceCenter.Z + m_localOrigin.Z
						 );

					//Fix for sunshading screwing with everything
					bool lighting = drawArgs.device.RenderState.Lighting;
					drawArgs.device.RenderState.Lighting = m_enableLighting;

					if (m_wallVertices != null)
					{
						drawArgs.device.RenderState.ZBufferEnable = true;

						if (m_texture != null && !m_texture.Disposed)
						{
							drawArgs.device.SetTexture(0, m_texture);
							drawArgs.device.TextureState[0].AlphaOperation = TextureOperation.Modulate;
							drawArgs.device.TextureState[0].ColorOperation = TextureOperation.Add;
							drawArgs.device.TextureState[0].AlphaArgument1 = TextureArgument.TextureColor;
						}
						else
						{
							drawArgs.device.TextureState[0].ColorOperation = TextureOperation.Disable;
						}

						Material mat = new Material();
						mat.Diffuse = mat.Ambient = this.m_polygonColor;

						drawArgs.device.Material = mat;

						drawArgs.device.VertexFormat = CustomVertex.PositionNormalTextured.Format;

						drawArgs.device.DrawUserPrimitives(PrimitiveType.TriangleStrip, m_wallVertices.Length - 2, m_wallVertices);

						if (m_outline)
						{

							drawArgs.device.TextureState[0].ColorOperation = TextureOperation.Disable;
							drawArgs.device.VertexFormat = CustomVertex.PositionColored.Format;
							drawArgs.device.DrawUserPrimitives(PrimitiveType.LineStrip, m_topVertices.Length - 1, m_topVertices);

							if (m_bottomVertices != null)
								drawArgs.device.DrawUserPrimitives(PrimitiveType.LineStrip, m_bottomVertices.Length - 1, m_bottomVertices);

							if (m_sideVertices != null)
								drawArgs.device.DrawUserPrimitives(PrimitiveType.LineList, m_sideVertices.Length / 2, m_sideVertices);

						}
					}
					else
					{
						drawArgs.device.TextureState[0].ColorOperation = TextureOperation.Disable;
						drawArgs.device.VertexFormat = CustomVertex.PositionColored.Format;
						drawArgs.device.DrawUserPrimitives(PrimitiveType.LineStrip, m_topVertices.Length - 1, m_topVertices);
					}

					drawArgs.device.Transform.World = ConvertDX.FromMatrix4d(drawArgs.WorldCamera.WorldMatrix);
					drawArgs.device.RenderState.CullMode = currentCull;

					//put lighting back like it was (see above fix)
					drawArgs.device.RenderState.Lighting = lighting;
				}
				catch//(Exception ex)
				{
					//Utility.Log.Write(ex);
				}

			}
		}
	}
}
