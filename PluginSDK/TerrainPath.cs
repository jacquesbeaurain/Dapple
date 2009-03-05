using System;
using System.Collections;
using System.IO;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using WorldWind;
using WorldWind.Terrain;
using Utility;
using System.Collections.Generic;

namespace WorldWind.Renderable
{
	/// <summary>
	/// Class used to create and render a terrain following path
	/// TODO: Re-Implement terrain mapping based on new TerrainAccessor functionality
	/// </summary>
	internal class TerrainPath : RenderableObject
	{
		float north;
		float south;
		float east;
		float west;
		BoundingBox boundingBox;
		World _parentWorld;
		BinaryReader _dataArchiveReader;
		long _fileOffset;
		TerrainAccessor _terrainAccessor;
		float heightAboveSurface;
		string terrainFileName;
		//bool terrainMapped;
		internal bool isLoaded;
		Point3d lastUpdatedPosition;
		float verticalExaggeration = 1.0f;

		double _minDisplayAltitude, _maxDisplayAltitude;
		bool m_enableLighting = false;

		int lineColor;
		internal CustomVertex.PositionColored[] linePoints;

		List <Vector3> sphericalCoordinates = new List <Vector3>(); // x = lat, y = lon, z = height
        bool m_needsUpdate = false;

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.Renderable.TerrainPath"/> class.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="parentWorld"></param>
		/// <param name="minDisplayAltitude"></param>
		/// <param name="maxDisplayAltitude"></param>
		/// <param name="terrainFileName"></param>
		/// <param name="heightAboveSurface"></param>
		/// <param name="lineColor"></param>
		/// <param name="terrainAccessor"></param>
		internal TerrainPath(
			string name,
			World parentWorld,
			double minDisplayAltitude,
			double maxDisplayAltitude,
			string terrainFileName,
			float heightAboveSurface,
			System.Drawing.Color lineColor,
			TerrainAccessor terrainAccessor)
			: base(name, parentWorld.Position, Quaternion4d.RotationYawPitchRoll(0, 0, 0))
		{
			this._parentWorld = parentWorld;
			this._minDisplayAltitude = minDisplayAltitude;
			this._maxDisplayAltitude = maxDisplayAltitude;
			this.terrainFileName = terrainFileName;
			this.heightAboveSurface = heightAboveSurface;
			//this.terrainMapped = terrainMapped;
			this.lineColor = lineColor.ToArgb();
			this._terrainAccessor = terrainAccessor;
			this.RenderPriority = RenderPriority.LinePaths;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.Renderable.TerrainPath"/> class.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="parentWorld"></param>
		/// <param name="minDisplayAltitude"></param>
		/// <param name="maxDisplayAltitude"></param>
		/// <param name="dataArchiveReader"></param>
		/// <param name="fileOffset"></param>
		/// <param name="fileSize"></param>
		/// <param name="north"></param>
		/// <param name="south"></param>
		/// <param name="east"></param>
		/// <param name="west"></param>
		/// <param name="heightAboveSurface"></param>
		/// <param name="lineColor"></param>
		/// <param name="terrainAccessor"></param>
		internal TerrainPath(
			string name,
			World parentWorld, 
			double minDisplayAltitude, 
			double maxDisplayAltitude, 
			BinaryReader dataArchiveReader,
			long fileOffset,
			long fileSize,
			double north,
			double south,
			double east, 
			double west,
			float heightAboveSurface,
			System.Drawing.Color lineColor,
			TerrainAccessor terrainAccessor)
			: base (name, parentWorld.Position, Quaternion4d .RotationYawPitchRoll(0, 0, 0))
		{
			this._parentWorld = parentWorld;
			this._minDisplayAltitude = minDisplayAltitude;
			this._maxDisplayAltitude = maxDisplayAltitude;
			this._dataArchiveReader = dataArchiveReader;
			this._fileOffset = fileOffset;
			this.heightAboveSurface = heightAboveSurface;
			//this.terrainMapped = terrainMapped;
			this.lineColor = lineColor.ToArgb();
			this._terrainAccessor = terrainAccessor;

			this.north = (float)north;
			this.south = (float)south;
			this.west = (float)west;
			this.east = (float)east;

			this.RenderPriority = RenderPriority.LinePaths;

			this.boundingBox = new BoundingBox(this.south, this.north, this.west, this.east,
				(float)this._parentWorld.EquatorialRadius,
				(float)(this._parentWorld.EquatorialRadius + this.verticalExaggeration * heightAboveSurface));
		}

		public override void Initialize(DrawArgs drawArgs)
		{
			this.verticalExaggeration = World.Settings.VerticalExaggeration;
			this.isInitialized = true;
		}

		internal List<Vector3> GetSphericalCoordinates(ref float northExtent, ref float southExtent, ref float westExtent, ref float eastExtent)
		{
			if (this._dataArchiveReader == null)
			{
				FileInfo file = new FileInfo(this.terrainFileName);

				if (!file.Exists)
				{
					northExtent = 0;
					southExtent = 0;
					westExtent = 0;
					eastExtent = 0;
					return null;
				}
				using (BufferedStream fs = new BufferedStream(file.OpenRead()))
				using (BinaryReader br = new BinaryReader(fs))
				{
					int numCoords = br.ReadInt32();

                    Vector3 value = new Vector3(); 

					List<Vector3> coordinates = new List<Vector3>(numCoords);
                    value.X = br.ReadSingle();
                    value.Y = br.ReadSingle();

                    coordinates.Add(value);

					northExtent = coordinates[0].X;
					southExtent = coordinates[0].X;
					westExtent = coordinates[0].Y;
					eastExtent = coordinates[0].Y;

					for (int i = 1; i < numCoords; i++)
					{
						value.X = br.ReadSingle();
						value.Y = br.ReadSingle();

                        coordinates.Add(value);

						if (northExtent < coordinates[i].X)
							northExtent = coordinates[i].X;
						if (eastExtent < coordinates[i].Y)
							eastExtent = coordinates[i].Y;
						if (southExtent > coordinates[i].X)
							southExtent = coordinates[i].X;
						if (westExtent > coordinates[i].Y)
							westExtent = coordinates[i].Y;
					}

					return coordinates;
				}
			}
			else
			{
				this._dataArchiveReader.BaseStream.Seek(this._fileOffset, SeekOrigin.Begin);

				int numCoords = this._dataArchiveReader.ReadInt32();

				byte numElements = this._dataArchiveReader.ReadByte();
                List<Vector3> coordinates = new List<Vector3>(numCoords);

                Vector3 value = new Vector3();

                value.X = (float)this._dataArchiveReader.ReadDouble();
                value.Y = (float)this._dataArchiveReader.ReadDouble();
				if(numElements == 3)
                    value.Z = this._dataArchiveReader.ReadInt16();

                coordinates.Add(value);

				northExtent = coordinates[0].X;
				southExtent = coordinates[0].X;
				westExtent = coordinates[0].Y;
				eastExtent = coordinates[0].Y;

				for(int i = 1; i < numCoords; i++)
				{
                    value.X = (float)this._dataArchiveReader.ReadDouble();
                    value.Y = (float)this._dataArchiveReader.ReadDouble();
                    if (numElements == 3)
                        value.Z = this._dataArchiveReader.ReadInt16();

                    coordinates.Add(value);

					if (northExtent < coordinates[i].X)
						northExtent = coordinates[i].X;
					if (eastExtent < coordinates[i].Y)
						eastExtent = coordinates[i].Y;
					if (southExtent > coordinates[i].X)
						southExtent = coordinates[i].X;
					if (westExtent > coordinates[i].Y)
						westExtent = coordinates[i].Y;
				}

				return coordinates;
			}
		}

		internal void Load()
		{
			try
			{
				if (this.terrainFileName == null && this._dataArchiveReader == null)
				{
					this.isInitialized = true;
					return;
				}

				sphericalCoordinates = GetSphericalCoordinates(ref north, ref south, ref west, ref east);

				this.boundingBox = new BoundingBox(this.south, this.north, this.west, this.east,
					(float)this._parentWorld.EquatorialRadius,
					(float)(this._parentWorld.EquatorialRadius + this.verticalExaggeration * heightAboveSurface));

			}
			catch (Exception caught)
			{
				Log.Write(caught);
			}

			this.isLoaded = true;
		}

		public override void Dispose()
		{
			this.isLoaded = false;
			this.linePoints = null;
		}

		internal void SaveToFile(string fileName)
		{
			using (BinaryWriter output = new BinaryWriter(new FileStream(fileName, FileMode.Create)))
			{
				output.Write(this.sphericalCoordinates.Count);
				for (int i = 0; i < this.sphericalCoordinates.Count; i++)
				{
					output.Write(this.sphericalCoordinates[i].X);
					output.Write(this.sphericalCoordinates[i].Y);
				}
			}
		}

		public override void Update(DrawArgs drawArgs)
		{
			try
			{
				if (!drawArgs.WorldCamera.ViewFrustum.Intersects(boundingBox))
				{
					Dispose();
					return;
				}

				if (!isLoaded)
					Load();

				if (linePoints != null)
					if((lastUpdatedPosition - drawArgs.WorldCamera.Position).LengthSq < 10*10) // Update if camera moved more than 10 meters
						if (Math.Abs(this.verticalExaggeration - World.Settings.VerticalExaggeration) < 0.01)
							// Already loaded and up-to-date
							return;

				verticalExaggeration = World.Settings.VerticalExaggeration;

				ArrayList renderablePoints = new ArrayList();
				Point3d lastPointProjected = Point3d.Empty;
				Point3d currentPointProjected;
				Point3d currentPointXyz = Point3d.Empty;

				Point3d rc = new Point3d(
						  drawArgs.WorldCamera.ReferenceCenter.X,
						  drawArgs.WorldCamera.ReferenceCenter.Y,
						  drawArgs.WorldCamera.ReferenceCenter.Z
						  );

				for (int i = 0; i < sphericalCoordinates.Count; i++)
				{
					double altitude = 0;
					if (_parentWorld.TerrainAccessor != null && drawArgs.WorldCamera.Altitude < 3000000)
						altitude = _terrainAccessor.GetElevationAt(
							sphericalCoordinates[i].X,
							sphericalCoordinates[i].Y,
							(100.0 / drawArgs.WorldCamera.ViewRange.Degrees));

					currentPointXyz = MathEngine.SphericalToCartesian(
						this.sphericalCoordinates[i].X,
						this.sphericalCoordinates[i].Y,
						this._parentWorld.EquatorialRadius + this.heightAboveSurface +
						this.verticalExaggeration * altitude);

					currentPointProjected = drawArgs.WorldCamera.Project(currentPointXyz - rc);

					double dx = lastPointProjected.X - currentPointProjected.X;
					double dy = lastPointProjected.Y - currentPointProjected.Y;
					double distanceSquared = dx * dx + dy * dy;
					const double minimumPointSpacingSquaredPixels = 2 * 2;
					if (distanceSquared > minimumPointSpacingSquaredPixels)
					{
						renderablePoints.Add(currentPointXyz);
						lastPointProjected = currentPointProjected;
					}
				}

				// Add the last point if it's not already in there
				int pointCount = renderablePoints.Count;
				if (pointCount > 0 && (Point3d)renderablePoints[pointCount - 1] != currentPointXyz)
				{
					renderablePoints.Add(currentPointXyz);
					pointCount++;
				}

				CustomVertex.PositionColored[] newLinePoints = new CustomVertex.PositionColored[pointCount];
				for (int i = 0; i < pointCount; i++)
				{
					currentPointXyz = (Point3d)renderablePoints[i];
					newLinePoints[i].X = (float)currentPointXyz.X;
					newLinePoints[i].Y = (float)currentPointXyz.Y;
					newLinePoints[i].Z = (float)currentPointXyz.Z;

					newLinePoints[i].Color = this.lineColor;
				}

				this.linePoints = newLinePoints;

				lastUpdatedPosition = drawArgs.WorldCamera.Position;
				System.Threading.Thread.Sleep(1);
			}
			catch
			{
			}
		}

		public override bool PerformSelectionAction(DrawArgs drawArgs)
		{
			return false;
		}

		public override void Render(DrawArgs drawArgs)
		{
			try
			{
				if (!this.isLoaded)
					return;

				if (drawArgs.WorldCamera.Altitude > _maxDisplayAltitude)
					return;
				if (drawArgs.WorldCamera.Altitude < _minDisplayAltitude)
					return;

                if (m_needsUpdate)
                    this.Update(drawArgs);
				if (this.linePoints == null)
					return;

				if (!drawArgs.WorldCamera.ViewFrustum.Intersects(this.boundingBox))
					return;

				drawArgs.numBoundaryPointsRendered += this.linePoints.Length;
				drawArgs.numBoundaryPointsTotal += this.sphericalCoordinates.Count;
				drawArgs.numBoundariesDrawn++;

				drawArgs.device.VertexFormat = CustomVertex.PositionColored.Format;
				drawArgs.device.TextureState[0].ColorOperation = TextureOperation.Disable;

				drawArgs.device.Transform.World = Matrix.Translation(
					(float)-drawArgs.WorldCamera.ReferenceCenter.X,
					(float)-drawArgs.WorldCamera.ReferenceCenter.Y,
					(float)-drawArgs.WorldCamera.ReferenceCenter.Z
					);


				//Fix for sunshading screwing with everything
				bool lighting = drawArgs.device.RenderState.Lighting;
				drawArgs.device.RenderState.Lighting = m_enableLighting;

				drawArgs.device.DrawUserPrimitives(PrimitiveType.LineStrip, this.linePoints.Length - 1, this.linePoints);
				drawArgs.device.Transform.World = ConvertDX.FromMatrix4d(drawArgs.WorldCamera.WorldMatrix);

				//put lighting back like it was (see above fix)
				drawArgs.device.RenderState.Lighting = lighting;
			}
			catch
			{
			}
		}
	}
}
