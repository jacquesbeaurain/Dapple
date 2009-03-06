using System;
using System.Drawing;
using System.Globalization;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace WorldWind
{
	/// <summary>
	/// Summary description for OverviewControl.
	/// </summary>
	public class OverviewControl : System.Windows.Forms.Control
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		private WorldWindow m_WorldWindow = null;
      Image m_Image;
      Bitmap m_BackBm;
      string m_strMinimapFilePath;

		System.Timers.Timer m_RenderTimer = new System.Timers.Timer(150);

		internal OverviewControl(System.ComponentModel.IContainer container)
		{
			///
			/// Required for Windows.Forms Class Composition Designer support
			///
			container.Add(this);
			InitializeComponent();
		}

		public OverviewControl(string miniMapFilePath, WorldWindow ww, Control parent)
		{
			InitializeComponent();

			m_WorldWindow = ww;
         m_strMinimapFilePath = miniMapFilePath;
			
			this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque, true);

			// The m_Device3d can't be created unless the control is at least 1 x 1 pixels in size
			this.Size = new Size(600,300);
			
			// Now perform the rendering m_Device3d initialization
			// Skip DirectX initialization in design mode
			if(!IsInDesignMode())
				InitializeGraphics();

         this.Parent = parent;
		}

		public void StartTimer()
		{
			m_RenderTimer.Elapsed += new System.Timers.ElapsedEventHandler(m_RenderTimer_Elapsed);
			m_RenderTimer.Start();
		}

		protected override void OnMove(EventArgs e)
		{
			base.OnMove (e);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
         if (m_Image == null)
            e.Graphics.Clear(SystemColors.Control);
         else 
         {
            Render(e.Graphics);
         }
		}

		protected override void OnMouseWheel(MouseEventArgs e)
		{
			if(m_WorldWindow != null)
			{
				m_WorldWindow.DrawArgs.WorldCamera.ZoomStepped(e.Delta/120.0f);
			}
			base.OnMouseWheel (e);
		}

		/// <summary>
		/// Render the scene.
		/// </summary>
      internal void Render(Graphics g)
      {
         if (m_BackBm == null || m_BackBm.Width != Width || m_BackBm.Height != Height)
         {
            if (m_BackBm != null)
               m_BackBm.Dispose();
            m_BackBm = new Bitmap(Width, Height);
         }

         using (Graphics grb = Graphics.FromImage(m_BackBm))
         {
            grb.DrawImage(m_Image, 0, 0, Width, Height);
            RenderViewBox(grb);
         }

         g.DrawImageUnscaled(m_BackBm, 0, 0);

         if (m_FireMouseUpEvent)
         {
            m_FireMouseUpEvent = false;
         }
      }

		internal Point GetPointFromCoord(double latitude, double longitude)
		{
			Point p = new Point();

			if(latitude == 90.0)
				p.Y = 0;
			else if(latitude == -90.0)
				p.Y = Height;
			else
			{
				p.Y = (int)(Height - ((latitude + 90.0) / 180.0) * Height);
			}

			if(longitude == -180.0)
				p.X = 0;
			else if(longitude == 180.0)
				p.X = Width;
			else
			{
				p.X = (int)(((longitude + 180.0) / 360.0) * Width);
			}

			return p;
		}


		static int hotColor = Color.White.ToArgb();
		static int normalColor = Color.FromArgb(150,255,255,255).ToArgb();
		static int nameColor = Color.White.ToArgb();
		static int descriptionColor = Color.White.ToArgb();

		bool m_FireMouseUpEvent = false;

      SolidBrush m_ViewBoxBrush = new SolidBrush(Color.FromArgb(150, Color.Red.R, Color.Red.G, Color.Red.B));
		Pen m_ViewBoxPen = Pens.Red;

		SolidBrush m_TargetViewBoxBrush = new SolidBrush(Color.FromArgb(150, Color.Purple.R, Color.Purple.G, Color.Purple.B));
      Pen m_TargetViewBoxPen = Pens.Purple;

		bool m_MouseIsDragging = false;
		bool m_IsPanning = false;
		bool m_RenderTargetViewBox = false;
		
		Point m_MouseDownStartPosition = Point.Empty;
		Point m_LastMousePosition = new Point();
		Point m_TargetRenderBoxPosition = Point.Empty;
		float m_PanSpeed = 0.2f;
		
		System.DateTime m_LastResize = System.DateTime.Now;
		System.DateTime m_LeftMouseDownTime = System.DateTime.MaxValue;
		System.TimeSpan m_MousePanHoldInterval = TimeSpan.FromSeconds(1);
		
		private void RenderViewBox(Graphics g)
		{
         Point[] pts = new Point[4];

         GeographicQuad viewBox = m_WorldWindow.CurrentAreaOfInterest;
         
         Point center = GetPointFromCoord(m_WorldWindow.DrawArgs.WorldCamera.Latitude.Degrees, m_WorldWindow.DrawArgs.WorldCamera.Longitude.Degrees);
         
         pts[0] = GetPointFromCoord(viewBox.Y1, viewBox.X1); // lower left
         pts[1] = GetPointFromCoord(viewBox.Y2, viewBox.X2); // lower right
         pts[2] = GetPointFromCoord(viewBox.Y3, viewBox.X3); // upper left
         pts[3] = GetPointFromCoord(viewBox.Y4, viewBox.X4); // upper right

         g.FillPolygon(m_ViewBoxBrush, pts);
         g.DrawPolygon(m_ViewBoxPen, pts);
         g.DrawEllipse(m_ViewBoxPen, center.X - 1, center.Y - 1, 3, 3);
         g.FillEllipse(m_ViewBoxBrush, center.X - 1, center.Y - 1, 3, 3);

         if (m_RenderTargetViewBox)
				RenderTargetViewBox(g);
		}

      private void RenderTargetViewBox(Graphics g)
		{
         Point[] pts = new Point[4];

			pts[0] = new Point(
				(m_MouseDownStartPosition.X < m_LastMousePosition.X ? m_MouseDownStartPosition.X : m_LastMousePosition.X),
				(m_MouseDownStartPosition.Y >= m_LastMousePosition.Y ? m_MouseDownStartPosition.Y : m_LastMousePosition.Y)
				);

         pts[1] = new Point(
				pts[0].X,
				(m_MouseDownStartPosition.Y < m_LastMousePosition.Y ? m_MouseDownStartPosition.Y : m_LastMousePosition.Y)
				);

         pts[2] = new Point(
				(m_MouseDownStartPosition.X >= m_LastMousePosition.X ? m_MouseDownStartPosition.X : m_LastMousePosition.X),
				pts[1].Y
				);

         pts[3] = new Point(
				pts[2].X,
				pts[0].Y);

         g.FillPolygon(m_TargetViewBoxBrush, pts);
         g.DrawPolygon(m_TargetViewBoxPen, pts);
		}

      private PointF GetGeoCoordFromScreenPoint(Point p)
		{
         return new PointF(
				(float)p.X / (float)Width * 360.0f - 180.0f,
				(float)(Height - p.Y) / (float)Height * 180.0f - 90.0f);
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			bool isMouseLeftButtonDown = ((int)e.Button & (int)MouseButtons.Left) != 0;
			bool isMouseRightButtonDown = ((int)e.Button & (int)MouseButtons.Right) != 0;
			if (!m_IsPanning && isMouseLeftButtonDown || isMouseRightButtonDown)
			{
				int dx = m_MouseDownStartPosition.X - e.X;
				int dy = m_MouseDownStartPosition.Y - e.Y;
				int distanceSquared = dx*dx + dy*dy;
				if (distanceSquared > 3*3)
					// Distance > 3 = drag
					m_MouseIsDragging = true;
			}

			if(isMouseLeftButtonDown && m_MouseIsDragging)
			{
				if(!m_RenderTargetViewBox)
				{
					m_RenderTargetViewBox = true;
				}
			}
			else if (!isMouseLeftButtonDown && isMouseRightButtonDown && m_MouseIsDragging)
			{
				float gX = (float)Width / 360.0f;
				float gY = (float)Height / 180.0f;

				float dX = m_LastMousePosition.X - e.X;
				float dY = e.Y - m_LastMousePosition.Y;

				m_WorldWindow.DrawArgs.WorldCamera.Tilt += Angle.FromDegrees(dY / gY);
				//m_WorldWindow.DrawArgs.WorldCamera.Heading += Angle.FromDegrees(dX);
				m_WorldWindow.DrawArgs.WorldCamera.RotationYawPitchRoll( Angle.Zero, Angle.Zero, Angle.FromDegrees(dX / gX) );

			}

			m_LastMousePosition = new Point(e.X, e.Y);

			base.OnMouseMove (e);
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			m_LeftMouseDownTime = System.DateTime.MaxValue;

			if(m_IsPanning)
			{
				m_IsPanning = false;	
			}
			else if(m_MouseIsDragging)
			{
				m_MouseIsDragging = false;
				if(m_RenderTargetViewBox)
				{
					m_RenderTargetViewBox = false;
					Point centerPoint = new Point(
						(m_MouseDownStartPosition.X + m_LastMousePosition.X) / 2,
						(m_MouseDownStartPosition.Y + m_LastMousePosition.Y) / 2);

					PointF gotoPoint = GetGeoCoordFromScreenPoint(centerPoint);

					double dLongitudeAngle = Math.Abs(m_MouseDownStartPosition.X - m_LastMousePosition.X);
					double dLatitudeAngle = Math.Abs(m_MouseDownStartPosition.Y - m_LastMousePosition.Y);

					double degreesPerPixelX = 360.0 / Width;
					double degreesPerPixelY = 180.0 / Height;

					dLongitudeAngle *= degreesPerPixelX;
					dLatitudeAngle *= degreesPerPixelY;

               m_WorldWindow.GoToBoundingBox(
						new GeographicBoundingBox(
                  gotoPoint.Y + dLatitudeAngle / 2.0,
						gotoPoint.Y - dLatitudeAngle / 2.0,
						gotoPoint.X - dLongitudeAngle / 2.0,
                  gotoPoint.X + dLongitudeAngle / 2.0
                  ),
						false);
            }
			}
			else if(e.Button == MouseButtons.Left)
			{
				PointF gotoPoint = GetGeoCoordFromScreenPoint(new Point(e.X, e.Y));
				m_WorldWindow.GoToLatLon(gotoPoint.Y, gotoPoint.X);

				m_FireMouseUpEvent = true;
			}
			else if(e.Button == MouseButtons.Right)
			{
			}
			m_MouseDownStartPosition = Point.Empty;

         Invalidate();

			base.OnMouseUp (e);
		}
		
		protected override void OnMouseDown(MouseEventArgs e)
		{
			m_MouseDownStartPosition = new Point(e.X, e.Y);
			if(e.Button == MouseButtons.Left)
			{
				m_LeftMouseDownTime = System.DateTime.Now;
			}
			base.OnMouseDown (e);
		}

		/// <summary>
		/// Returns true if executing in Design mode (inside IDE)
		/// </summary>
		/// <returns></returns>
		private static bool IsInDesignMode()
		{
			return Application.ExecutablePath.ToUpper(CultureInfo.InvariantCulture).EndsWith("DEVENV.EXE");
		}

		private void InitializeGraphics()
		{
         m_Image = Image.FromFile(m_strMinimapFilePath);
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}

         if (m_Image != null)
			{
            m_Image.Dispose();
            m_Image = null;
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			// 
			// OverviewForm
			// 
			this.Name = "OverviewControl";
			this.Text = "OverviewControl";

		}
		#endregion

		protected override void OnMouseLeave(EventArgs e)
		{
			m_LeftMouseDownTime = System.DateTime.MaxValue;

			base.OnMouseLeave (e);
		}


		private void m_RenderTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			if (m_LeftMouseDownTime != System.DateTime.MaxValue || m_IsPanning || (m_WorldWindow != null && m_WorldWindow.DrawArgs != null && (!((WorldWind.Camera.WorldCamera)m_WorldWindow.DrawArgs.WorldCamera).EpsilonTest() || ((WorldWind.Camera.WorldCamera)m_WorldWindow.DrawArgs.WorldCamera).ForceRender)))
            Invalidate();

         if(m_LeftMouseDownTime != System.DateTime.MaxValue)
			{
				System.DateTime currentTime = System.DateTime.Now;
				if(!m_MouseIsDragging && currentTime.Subtract(m_LeftMouseDownTime) >= m_MousePanHoldInterval)
				{
					m_IsPanning = true;
				}
			}
			else
			{
				m_IsPanning = false;
			}
				

			if(m_IsPanning)
			{
				if(m_WorldWindow != null)
				{
					Point srcP = this.GetPointFromCoord(
						m_WorldWindow.DrawArgs.WorldCamera.Latitude.Degrees, m_WorldWindow.DrawArgs.WorldCamera.Longitude.Degrees);
               Point dstP = new Point((int)Math.Round((float)srcP.X + m_PanSpeed * (float)(m_LastMousePosition.X - srcP.X)), (int)Math.Round((float)srcP.Y + m_PanSpeed * (m_LastMousePosition.Y - srcP.Y)));

					PointF dest = this.GetGeoCoordFromScreenPoint(dstP);
					m_WorldWindow.GoToLatLon(dest.Y, dest.X);
				}
			}
		}
	}
}
