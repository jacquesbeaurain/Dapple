using System;
using Utility;
using System.Collections.Generic;

namespace WorldWind
{
	/// <summary>
	/// Summary description for ProjectedVectorRenderer.
	/// </summary>
	public class ProjectedVectorRenderer
	{
		ProjectedVectorTile[] m_rootTiles = null;
		double m_lzts = 36.0;
		public World World = null;
		List<Polygon> m_polygons = new List<Polygon>();
		List<LineString> m_lineStrings = new List<LineString>();
		public System.DateTime LastUpdate = System.DateTime.Now;
		private GeographicBoundingBox m_oBounds = GeographicBoundingBox.NullBox();

		public Polygon[] Polygons
		{
			get { return m_polygons.ToArray(); }
		}

		public LineString[] LineStrings
		{
			get { return m_lineStrings.ToArray(); }
		}

		public GeographicBoundingBox Bounds
		{
			get { return m_oBounds; }
		}

		public ProjectedVectorRenderer(World parentWorld)
		{
			World = parentWorld;
			CreateRootTiles();
		}

		private void CreateRootTiles()
		{
			int numberRows = (int)(180.0f / m_lzts);

			System.Collections.ArrayList tileList = new System.Collections.ArrayList();

			int istart = 0;
			int iend = numberRows;
			int jstart = 0;
			int jend = numberRows * 2;

			for (int i = istart; i < iend; i++)
			{
				for (int j = jstart; j < jend; j++)
				{
					double north = (i + 1) * m_lzts - 90.0f;
					double south = i * m_lzts - 90.0f;
					double west = j * m_lzts - 180.0f;
					double east = (j + 1) * m_lzts - 180.0f;

					ProjectedVectorTile newTile = new ProjectedVectorTile(
						new GeographicBoundingBox(
						north,
						south,
						west,
						east),
						this);

					newTile.Level = 0;
					newTile.Row = i;
					newTile.Col = j;
					tileList.Add(newTile);
				}
			}

			m_rootTiles = (ProjectedVectorTile[])tileList.ToArray(typeof(ProjectedVectorTile));
		}


		public void Add(Polygon polygon)
		{
			if (polygon.ParentRenderable == null)
				Log.Write(Log.Levels.Error, "null null null");
			polygon.Visible = IsRenderableVisible(polygon.ParentRenderable);
			m_polygons.Add(polygon);
			LastUpdate = System.DateTime.Now;

			m_oBounds.Union(polygon.GetGeographicBoundingBox());
		}

		public void Add(LineString lineString)
		{
			lineString.Visible = IsRenderableVisible(lineString.ParentRenderable);
			m_lineStrings.Add(lineString);
			LastUpdate = System.DateTime.Now;

			m_oBounds.Union(lineString.GetGeographicBoundingBox());
		}

		public void Update(DrawArgs drawArgs)
		{
			for (int i = 0; i < m_lineStrings.Count; i++)
			{
				LineString lineString = (LineString)m_lineStrings[i];
				if (lineString.Remove)
				{
					m_lineStrings.RemoveAt(i);
					RecalculateBoundingBox();
					LastUpdate = System.DateTime.Now;
					i--;
				}
				else if (lineString.ParentRenderable != null)
				{
					bool visibility = IsRenderableVisible(lineString.ParentRenderable);
					if (visibility != lineString.Visible)
					{
						lineString.Visible = visibility;
						LastUpdate = System.DateTime.Now;
					}
				}
			}

			for (int i = 0; i < m_polygons.Count; i++)
			{
				Polygon polygon = (Polygon)m_polygons[i];
				if (polygon.Remove)
				{
					m_polygons.RemoveAt(i);
					 RecalculateBoundingBox();
					LastUpdate = System.DateTime.Now;
					i--;
				}

				else if (polygon.ParentRenderable != null)
				{
					bool visibility = IsRenderableVisible(polygon.ParentRenderable);
					if (visibility != polygon.Visible)
					{
						polygon.Visible = visibility;
						LastUpdate = System.DateTime.Now;
					}
				}
			}

			foreach (ProjectedVectorTile tile in m_rootTiles)
				tile.Update(drawArgs);
		}

		private void RecalculateBoundingBox()
		{
			m_oBounds = GeographicBoundingBox.NullBox();

			foreach (Polygon oPoly in m_polygons)
			{
				m_oBounds.Union(oPoly.GetGeographicBoundingBox());
			}
			foreach (LineString oLine in m_lineStrings)
			{
				m_oBounds.Union(oLine.GetGeographicBoundingBox());
			}
		}

		private static bool IsRenderableVisible(WorldWind.Renderable.RenderableObject renderable)
		{
			if (!renderable.IsOn)
			{
				return false;
			}
			else if (renderable.ParentList != null)
			{
				return IsRenderableVisible(renderable.ParentList);
			}
			else
			{
				return true;
			}
		}

		public void Render(DrawArgs drawArgs)
		{
			drawArgs.device.Clear(Microsoft.DirectX.Direct3D.ClearFlags.ZBuffer, 0, 1.0f, 0);

			foreach (ProjectedVectorTile tile in m_rootTiles)
				tile.Render(drawArgs);
		}
	}
}
