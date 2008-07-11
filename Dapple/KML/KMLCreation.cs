using System;
using System.Collections.Generic;
using System.Text;
using WorldWind.Renderable;
using WorldWind;
using System.IO;

namespace Dapple.KML
{
	public static class KMLCreation
	{
		public static RenderableObject CreateKMLLayer(KMLFile oSource, World oWorld, out GeographicBoundingBox oBounds)
		{
			try
			{
				oBounds = GeographicBoundingBox.NullBox();
				RenderableObject result = Construct(Path.GetDirectoryName(oSource.Filename), oSource.Document, oWorld, oBounds);
				if (!oBounds.IsValid)oBounds = new GeographicBoundingBox(90.0, -90.0, -180.0, 180.0);
				return result;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message.ToString());
				throw;
			}
		}

		private static RenderableObject Construct(String strRelativeDirectory, KMLObject oSource, World oWorld, GeographicBoundingBox oBounds)
		{
			if (oSource is Container)
			{
				Container oCastSource = oSource as Container;
				RenderableObjectList result = new RenderableObjectList(oCastSource.Name);
				result.SortChildrenOnAdd = false;
				for (int count = 0; count < oCastSource.Count; count++)
				{
					RenderableObject oLayer = Construct(strRelativeDirectory, oCastSource[count], oWorld, oBounds);
					if (oLayer != null)
					{
						result.Add(oLayer);
					}
				}
				return result;
			}
			else if (oSource is Placemark)
			{
				Placemark oCastSource = oSource as Placemark;
				return Construct(strRelativeDirectory, oCastSource.Geometry, oWorld, oBounds);
			}
			else if (oSource is MultiGeometry)
			{
				MultiGeometry oCastSource = oSource as MultiGeometry;
				RenderableObjectList result = new RenderableObjectList("MultiGeometry");
				result.SortChildrenOnAdd = false;
				for (int count = 0; count < oCastSource.Count; count++)
				{
					RenderableObject oLayer = Construct(strRelativeDirectory, oCastSource[count], oWorld, oBounds);
					if (oLayer != null)
					{
						result.Add(oLayer);
					}
				}
				return result;
			}
			else if (oSource is Point)
			{
				Point oCastSource = oSource as Point;

				KMLIcon result = new KMLIcon(oCastSource.Owner.Name, oCastSource.Coordinates.Latitude, oCastSource.Coordinates.Longitude, oCastSource.Style.NormalStyle.IconStyle.Icon.HRef, oCastSource.Coordinates.Altitude);
				result.DrawGroundStick = oCastSource.Extrude;
				result.Rotation = WorldWind.Angle.FromDegrees(oCastSource.Style.NormalStyle.IconStyle.Heading);
				result.IsRotated = oCastSource.Style.NormalStyle.IconStyle.Heading != 0.0f;

				// update oBounds
				return result;
			}
			else if (oSource is Polygon)
			{
				Polygon oCastSource = oSource as Polygon;

				PolygonFeature result = new PolygonFeature(oCastSource.Owner.Name, oWorld, new WorldWind.LinearRing(GetPoints(oCastSource.OuterBoundary)), new WorldWind.LinearRing[] { }, oCastSource.Style.NormalStyle.PolyStyle.Color);
				result.Fill = oCastSource.Style.NormalStyle.PolyStyle.Fill;
				result.Extrude = oCastSource.Extrude;
				result.AltitudeMode = KMLAltitudeModeToWorldWind(oCastSource.AltitudeMode);
				result.Outline = oCastSource.Style.NormalStyle.PolyStyle.Outline;
				result.OutlineColor = oCastSource.Style.NormalStyle.PolyStyle.Color;

				oBounds.Union(result.GeographicBoundingBox);
				return result;
			}
			else if (oSource is LineString)
			{
				LineString oCastSource = oSource as LineString;

				LineFeature result = new LineFeature(oCastSource.Owner.Name, oWorld, GetPoints(oCastSource), oCastSource.Style.NormalStyle.LineStyle.Color);
				result.AltitudeMode = KMLAltitudeModeToWorldWind(oCastSource.AltitudeMode);
				result.LineWidth = oCastSource.Style.NormalStyle.LineStyle.Width;
				result.Extrude = oCastSource.Extrude;

				// Update oBounds
				return result;
			}
			else if (oSource is GroundOverlay)
			{
				GroundOverlay oCastSource = oSource as GroundOverlay;

				ImageLayer result = new ImageLayer(
					oCastSource.Name,
					oWorld,
					oCastSource.Altitude,
					null,
					oCastSource.LatLonBox.South,
					oCastSource.LatLonBox.North,
					oCastSource.LatLonBox.West,
					oCastSource.LatLonBox.East,
					255,
					oWorld.TerrainAccessor);

				String strFilePath = oCastSource.Icon.HRef;

				if (strFilePath.StartsWith("http://"))
				{
					result.ImageUrl = strFilePath;
					result.ImagePath = Path.Combine(Path.Combine(MainForm.Settings.CachePath, "kml"), strFilePath.GetHashCode() + ".png");
				}
				else
				{
					strFilePath = strFilePath.Replace('\\', Path.DirectorySeparatorChar);
					strFilePath = strFilePath.Replace('/', Path.DirectorySeparatorChar);
					
					if (Path.IsPathRooted(strFilePath))
					{
						result.ImagePath = strFilePath;
					}
					else
					{
						result.ImagePath = Path.Combine(strRelativeDirectory, strFilePath);
					}
				}

				result.Opacity = oCastSource.Color.A;

				oBounds.Union(new GeographicBoundingBox(oCastSource.LatLonBox.North, oCastSource.LatLonBox.South, oCastSource.LatLonBox.West, oCastSource.LatLonBox.East));
				return result;
			}
			else
			{
				Console.WriteLine("Unknown type " + oSource.GetType().ToString());
				return null;
			}
		}

		private static Point3d[] GetPoints(LinearRing oInput)
		{
			Point3d[] result = new Point3d[oInput.Count];

			for (int count = 0; count < oInput.Count; count++)
			{
				result[count] = new Point3d(oInput[count].Longitude, oInput[count].Latitude, oInput[count].Altitude);
			}

			return result;
		}

		private static Point3d[] GetPoints(LineString oInput)
		{
			Point3d[] result = new Point3d[oInput.Count];

			for (int count = 0; count < oInput.Count; count++)
			{
				result[count] = new Point3d(oInput[count].Longitude, oInput[count].Latitude, oInput[count].Altitude);
			}

			return result;
		}

		private static WorldWind.AltitudeMode KMLAltitudeModeToWorldWind(AltitudeMode oInput)
		{
			switch (oInput)
			{
				case AltitudeMode.absolute:
					return WorldWind.AltitudeMode.Absolute;
				case AltitudeMode.clampToGround:
					return WorldWind.AltitudeMode.ClampedToGround;
				case AltitudeMode.relativeToGround:
					return WorldWind.AltitudeMode.RelativeToGround;
				default:
					throw new ArgumentException("Unknow AltitudeMode " + oInput.ToString());
			}
		}
	}
}
