using System;
using System.Collections.Generic;
using System.Text;
using WorldWind.Renderable;
using WorldWind;
using System.IO;

namespace Dapple.KML
{
	class KMLGroundOverlayRenderable : RenderableObject
	{
		private String m_strKMLDirectory;
		private String m_strImageFilename = Path.ChangeExtension(Path.Combine(KMLFile.KMLTempDirectory, Guid.NewGuid().ToString()), ".png");
		private ImageLayer m_oLayer;
		private KMLGroundOverlay m_oGroundOverlay;
		private GeographicBoundingBox m_oLastAoI, m_oLayerAoI;
		private DateTime m_oLastAoIChangeTime;

		internal KMLGroundOverlayRenderable(KMLGroundOverlay oSource, String strKMLDirectory)
			: base(oSource.Name)
		{
			m_oGroundOverlay = oSource;
			m_strKMLDirectory = strKMLDirectory;
		}

		~KMLGroundOverlayRenderable()
		{
			// --- If this is an internet file, delete the temporary file we saved to ---
			if (!m_oGroundOverlay.Icon.IsLocalFile)
			{
				try
				{
					File.Delete(m_strImageFilename);
				}
				catch (Exception)
				{
					// --- Not a huge deal if we can't delete a temp file ---
				}
			}
		}

		public override void Initialize(WorldWind.DrawArgs drawArgs)
		{
			lock (this)
			{
				m_oLastAoI = drawArgs.CurrentRoI;
				m_oLastAoIChangeTime = DateTime.Now;

				if (m_oGroundOverlay.Icon.ViewRefreshMode == KMLViewRefreshMode.onStop)
				{
					GeographicBoundingBox oRenderBox = GetNewBox(drawArgs.CurrentRoI, m_oGroundOverlay.Icon.ViewBoundScale);

					m_oLayer = new ImageLayer(
						m_oGroundOverlay.Name,
						drawArgs.CurrentWorld,
						m_oGroundOverlay.Altitude,
						m_strImageFilename,
						oRenderBox.South,
						oRenderBox.North,
						oRenderBox.West,
						oRenderBox.East,
						(byte)(m_oGroundOverlay.Color.A * ((double)m_opacity / (double)(byte.MaxValue))),
						drawArgs.CurrentWorld.TerrainAccessor);

					if (m_oGroundOverlay.Icon.IsLocalFile)
					{
						m_oLayer.ImagePath = Path.Combine(m_strKMLDirectory, m_oGroundOverlay.Icon.HRef);
					}
					else
					{
						m_oLayer.ImageUrl = m_oGroundOverlay.Icon.GetUri(oRenderBox.West, oRenderBox.South, oRenderBox.East, oRenderBox.North);
					}
					m_oLayerAoI = drawArgs.CurrentRoI.Clone() as GeographicBoundingBox;
				}
				else
				{
					m_oLayer = new ImageLayer(
						m_oGroundOverlay.Name,
						drawArgs.CurrentWorld,
						m_oGroundOverlay.Altitude,
						m_strImageFilename,
						m_oGroundOverlay.LatLonBox.South,
						m_oGroundOverlay.LatLonBox.North,
						m_oGroundOverlay.LatLonBox.West,
						m_oGroundOverlay.LatLonBox.East,
						(byte)(m_oGroundOverlay.Color.A * ((double)m_opacity / (double)(byte.MaxValue))),
						drawArgs.CurrentWorld.TerrainAccessor);

					if (m_oGroundOverlay.Icon.IsLocalFile)
					{
						m_oLayer.ImagePath = Path.Combine(m_strKMLDirectory, m_oGroundOverlay.Icon.HRef);
					}
					else
					{
						m_oLayer.ImageUrl = m_oGroundOverlay.GetUri();
					}
				}
				m_oLayer.Initialize(drawArgs);
			}
		}

		public override void Update(WorldWind.DrawArgs drawArgs)
		{
			lock (this)
			{
				if (m_oLayer != null)
				{
					if (m_oGroundOverlay.Icon.ViewRefreshMode == KMLViewRefreshMode.onStop)
					{
						double dTimeStopped;
						if (drawArgs.CurrentRoI.Equals(m_oLastAoI))
						{
							dTimeStopped = (DateTime.Now - m_oLastAoIChangeTime).Seconds;
						}
						else
						{
							dTimeStopped = 0.0;
							m_oLastAoI = drawArgs.CurrentRoI;
							m_oLastAoIChangeTime = DateTime.Now;
						}

						if (dTimeStopped > m_oGroundOverlay.Icon.ViewRefreshTime && !m_oLayerAoI.Equivalent(drawArgs.CurrentRoI, 1e-6))
						{
							try
							{
								File.Delete(m_strImageFilename);
							}
							catch (IOException) { return; }
							GeographicBoundingBox oRenderBox = GetNewBox(drawArgs.CurrentRoI, m_oGroundOverlay.Icon.ViewBoundScale);

							m_oLayer = new ImageLayer(
								m_oGroundOverlay.Name,
								drawArgs.CurrentWorld,
								m_oGroundOverlay.Altitude,
								m_strImageFilename,
								oRenderBox.South,
								oRenderBox.North,
								oRenderBox.West,
								oRenderBox.East,
								(byte)(m_oGroundOverlay.Color.A * ((double)m_opacity / (double)(byte.MaxValue))),
								drawArgs.CurrentWorld.TerrainAccessor);

							if (m_oGroundOverlay.Icon.IsLocalFile)
							{
								m_oLayer.ImagePath = Path.Combine(m_strKMLDirectory, m_oGroundOverlay.Icon.HRef);
							}
							else
							{
								m_oLayer.ImageUrl = m_oGroundOverlay.Icon.GetUri(oRenderBox.West, oRenderBox.South, oRenderBox.East, oRenderBox.North);
							}
							m_oLayer.Initialize(drawArgs);
							m_oLayerAoI = drawArgs.CurrentRoI.Clone() as GeographicBoundingBox;
						}
					}

					m_oLayer.Update(drawArgs);
				}
			}
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

				if (m_oLayer != null)
				{
					m_oLayer.Opacity = (byte)(m_oGroundOverlay.Color.A * ((double)value / (double)(byte.MaxValue)));
				}
			}
		}

		public override void Render(WorldWind.DrawArgs drawArgs)
		{
			lock (this)
			{
				if (m_oLayer != null)
				{
					m_oLayer.Render(drawArgs);
				}
			}
		}

		public override void Dispose()
		{
			//TODO: Something here?
		}

		/// <summary>
		/// True if the layer covers the entire world longitudinally.
		/// </summary>
		private bool WraparoundLayer
		{
			get { return m_oGroundOverlay.LatLonBox.East == 180.0 && m_oGroundOverlay.LatLonBox.West == -180.0; }
		}

		private GeographicBoundingBox GetNewBox(GeographicBoundingBox input, double dScale)
		{
			GeographicBoundingBox result = new GeographicBoundingBox(
				input.CenterLatitude + (input.North - input.CenterLatitude) * dScale,
				input.CenterLatitude + (input.South - input.CenterLatitude) * dScale,
				input.CenterLongitude + (input.West - input.CenterLongitude) * dScale,
				input.CenterLongitude + (input.East - input.CenterLongitude) * dScale
				);

			//result.North = Math.Min(m_oGroundOverlay.LatLonBox.North, Math.Min(90.0, result.North));
			//result.South = Math.Max(m_oGroundOverlay.LatLonBox.South, Math.Max(-90.0, result.South));
			if (!WraparoundLayer)
			{
				//result.East = Math.Min(m_oGroundOverlay.LatLonBox.East, Math.Min(180.0, result.East));
				//result.West = Math.Max(m_oGroundOverlay.LatLonBox.West, Math.Max(-180.0, result.West));
			}
			else
			{
				if (result.Longitude > 360.0)
				{
					result.East = 180.0;
					result.West = -180.0;
				}
			}

			return result;
		}
	}
}
