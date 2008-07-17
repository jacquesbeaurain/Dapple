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
				if (!oBounds.IsValid) oBounds = new GeographicBoundingBox(90.0, -90.0, -180.0, 180.0);
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
			if (oSource is KMLContainer)
			{
				KMLContainer oCastSource = oSource as KMLContainer;
				KMLRenderableObjectList result = new KMLRenderableObjectList(oCastSource.Name);
				for (int count = 0; count < oCastSource.Count; count++)
				{
					if (oCastSource[count].Visibility == true)
					{
						RenderableObject oLayer = Construct(strRelativeDirectory, oCastSource[count], oWorld, oBounds);
						if (oLayer != null)
						{
							result.Add(oLayer);
						}
					}
				}
				return result;
			}
			else if (oSource is KMLPlacemark)
			{
				KMLPlacemark oCastSource = oSource as KMLPlacemark;
				return Construct(strRelativeDirectory, oCastSource.Geometry, oWorld, oBounds);
			}
			else if (oSource is KMLMultiGeometry)
			{
				KMLMultiGeometry oCastSource = oSource as KMLMultiGeometry;
				KMLRenderableObjectList result = new KMLRenderableObjectList("MultiGeometry");
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
			else if (oSource is KMLPoint)
			{
				KMLPoint oCastSource = oSource as KMLPoint;

				KMLIcon result = new KMLIcon(oCastSource.Owner.Name, oCastSource.Coordinates.Latitude, oCastSource.Coordinates.Longitude, oCastSource.Style.NormalStyle.IconStyle.Icon != null ? oCastSource.Style.NormalStyle.IconStyle.Icon.HRef : null, oCastSource.Coordinates.Altitude);
				result.DrawGroundStick = oCastSource.Extrude;
				result.Rotation = WorldWind.Angle.FromDegrees(oCastSource.Style.NormalStyle.IconStyle.Heading);
				result.IsRotated = oCastSource.Style.NormalStyle.IconStyle.Heading != 0.0f;
				result.NormalColor = oCastSource.Style.NormalStyle.LabelStyle.Color;
				result.HotColor = oCastSource.Style.HighlightStyle.LabelStyle.Color;

				oBounds.Union(oCastSource.Coordinates.Longitude, oCastSource.Coordinates.Latitude, oCastSource.Coordinates.Altitude);
				return result;
			}
			else if (oSource is KMLPolygon)
			{
				KMLPolygon oCastSource = oSource as KMLPolygon;

				PolygonFeature result = new PolygonFeature(oCastSource.Owner.Name, oWorld, new WorldWind.LinearRing(GetPoints(oCastSource.OuterBoundary)), new WorldWind.LinearRing[] { }, oCastSource.Style.NormalStyle.PolyStyle.Color);
				result.Fill = oCastSource.Style.NormalStyle.PolyStyle.Fill;
				result.Extrude = oCastSource.Extrude;
				result.AltitudeMode = KMLAltitudeModeToWorldWind(oCastSource.AltitudeMode);
				result.Outline = oCastSource.Style.NormalStyle.PolyStyle.Outline;
				result.OutlineColor = oCastSource.Style.NormalStyle.PolyStyle.Color;

				oBounds.Union(result.GeographicBoundingBox);
				return result;
			}
			else if (oSource is KMLLineString)
			{
				KMLLineString oCastSource = oSource as KMLLineString;

				LineFeature result = new LineFeature(oCastSource.Owner.Name, oWorld, GetPoints(oCastSource), oCastSource.Style.NormalStyle.LineStyle.Color);
				result.AltitudeMode = KMLAltitudeModeToWorldWind(oCastSource.AltitudeMode);
				result.LineWidth = oCastSource.Style.NormalStyle.LineStyle.Width;
				result.Extrude = oCastSource.Extrude;

				// Update oBounds
				return result;
			}
			else if (oSource is KMLGroundOverlay)
			{
				KMLGroundOverlay oCastSource = oSource as KMLGroundOverlay;

				KMLGroundOverlayRenderable result = new KMLGroundOverlayRenderable(oCastSource, strRelativeDirectory);
				oBounds.Union(new GeographicBoundingBox(oCastSource.LatLonBox.North, oCastSource.LatLonBox.South, oCastSource.LatLonBox.West, oCastSource.LatLonBox.East));
				return result;
			}
			else
			{
				Console.WriteLine("Unknown type " + oSource.GetType().ToString());
				return null;
			}
		}

		private static Point3d[] GetPoints(KMLLinearRing oInput)
		{
			Point3d[] result = new Point3d[oInput.Count];

			for (int count = 0; count < oInput.Count; count++)
			{
				result[count] = new Point3d(oInput[count].Longitude, oInput[count].Latitude, oInput[count].Altitude);
			}

			return result;
		}

		private static Point3d[] GetPoints(KMLLineString oInput)
		{
			Point3d[] result = new Point3d[oInput.Count];

			for (int count = 0; count < oInput.Count; count++)
			{
				result[count] = new Point3d(oInput[count].Longitude, oInput[count].Latitude, oInput[count].Altitude);
			}

			return result;
		}

		private static WorldWind.AltitudeMode KMLAltitudeModeToWorldWind(KMLAltitudeMode oInput)
		{
			switch (oInput)
			{
				case KMLAltitudeMode.absolute:
					return WorldWind.AltitudeMode.Absolute;
				case KMLAltitudeMode.clampToGround:
					return WorldWind.AltitudeMode.ClampedToGround;
				case KMLAltitudeMode.relativeToGround:
					return WorldWind.AltitudeMode.RelativeToGround;
				default:
					throw new ArgumentException("Unknow AltitudeMode " + oInput.ToString());
			}
		}
	}

	class KMLRenderableObjectList : RenderableObjectList
	{
		public KMLRenderableObjectList(String strName)
			: base(strName)
		{
			m_blAllowDuplicateNames = true;
			m_blSortChildrenOnAdd = false;
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
				foreach (RenderableObject ro in m_children)
				{
					ro.Opacity = value;
				}
			}
		}
	}
}
