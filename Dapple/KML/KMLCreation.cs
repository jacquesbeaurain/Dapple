using System;
using System.Collections.Generic;
using System.Text;
using WorldWind.Renderable;
using WorldWind;
using System.IO;

namespace Dapple.KML
{
	internal static class KMLCreation
	{
		internal static RenderableObject CreateKMLLayer(KMLFile oSource, World oWorld, out GeographicBoundingBox oBounds)
		{
			oBounds = GeographicBoundingBox.NullBox();
			RenderableObject result = Construct(Path.GetDirectoryName(oSource.Filename), oSource.Document, oWorld, oBounds, null, null);
			if (!oBounds.IsValid) oBounds = new GeographicBoundingBox(90.0, -90.0, -180.0, 180.0);
			return result;
		}

		private static RenderableObject Construct(String strRelativeDirectory, KMLObject oSource, World oWorld, GeographicBoundingBox oBounds, ProjectedVectorRenderer oPVR, Icons oIcons)
		{
			if (oSource is KMLContainer)
			{
				KMLContainer oCastSource = oSource as KMLContainer;
				KMLRenderableObjectList result = new KMLRenderableObjectList(oCastSource.Name);

				if (oPVR == null)
				{
					oPVR = new ProjectedVectorRenderer("Polygons and LineStrings", oWorld);
					result.Add(oPVR);
				}
				if (oIcons == null)
				{
					oIcons = new Icons("Icons");
					result.Add(oIcons);
				}

				for (int count = 0; count < oCastSource.Count; count++)
				{
					if (oCastSource[count].Visibility == true)
					{
						RenderableObject oLayer = Construct(strRelativeDirectory, oCastSource[count], oWorld, oBounds, oPVR, oIcons);
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
				return Construct(strRelativeDirectory, oCastSource.Geometry, oWorld, oBounds, oPVR, oIcons);
			}
			else if (oSource is KMLMultiGeometry)
			{
				KMLMultiGeometry oCastSource = oSource as KMLMultiGeometry;
				KMLRenderableObjectList result = new KMLRenderableObjectList("MultiGeometry");
				for (int count = 0; count < oCastSource.Count; count++)
				{
					RenderableObject oLayer = Construct(strRelativeDirectory, oCastSource[count], oWorld, oBounds, oPVR, oIcons);
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
				oIcons.Add(result);

				oBounds.Union(oCastSource.Coordinates.Longitude, oCastSource.Coordinates.Latitude, oCastSource.Coordinates.Altitude);
				return null;
			}
			else if (oSource is KMLPolygon)
			{
				KMLPolygon oCastSource = oSource as KMLPolygon;

				Polygon oTool = new Polygon();
				oTool.outerBoundary = new WorldWind.LinearRing(GetPoints(oCastSource.OuterBoundary));
				oTool.innerBoundaries = GetInnerBoundaries(oCastSource);
				oTool.PolgonColor = oCastSource.Style.NormalStyle.PolyStyle.Color;
				oTool.Fill = oCastSource.Style.NormalStyle.PolyStyle.Fill;
				oTool.LineWidth = oCastSource.Style.NormalStyle.LineStyle.Width;
				oTool.Outline = oCastSource.Style.NormalStyle.PolyStyle.Outline;
				oTool.OutlineColor = oCastSource.Style.NormalStyle.LineStyle.Color;
				oPVR.Add(oTool);

				oBounds.Union(oTool.GetGeographicBoundingBox());
				return null;
			}
			else if (oSource is KMLLineString)
			{
				KMLLineString oCastSource = oSource as KMLLineString;

				LineString oTool = new LineString();
				oTool.Coordinates = GetPoints(oCastSource);
				oTool.Color = oCastSource.Style.NormalStyle.LineStyle.Color;
				oTool.LineWidth = oCastSource.Style.NormalStyle.LineStyle.Width;
				oPVR.Add(oTool);

				oBounds.Union(oTool.GetGeographicBoundingBox());
				return null;
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
				return null;
			}
		}

		private static LinearRing[] GetInnerBoundaries(KMLPolygon oCastSource)
		{
			LinearRing[] result = new LinearRing[oCastSource.InnerBoundaries.Count];

			for (int count = 0; count < oCastSource.InnerBoundaries.Count; count++)
			{
				result[count] = new LinearRing(GetPoints(oCastSource.InnerBoundaries[count]));
			}

			return result;
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
	}

	class KMLRenderableObjectList : RenderableObjectList
	{
		internal KMLRenderableObjectList(String strName)
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
