//
// Copyright © 2005 NASA.  Available under the NOSA License
//
// Portions copied from JHU_Icon - Copyright © 2005-2006 The Johns Hopkins University 
// Applied Physics Laboratory.  Available under the JHU/APL Open Source Agreement.
//
using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Net;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Collections.Generic;

namespace WorldWind.Renderable
{
	/// <summary>
	/// One icon in an icon layer
	/// </summary>
	public class Icon : RenderableObject
	{
		# region private variables

		protected bool m_isUpdated;
		protected bool m_newTexture;

		protected Point3d m_groundPoint;
		protected Line m_groundStick;
		protected Vector2[] m_groundStickVector;

		protected static int nameColor = Color.White.ToArgb();
		protected static int descriptionColor = Color.White.ToArgb();

		protected const int minIconZoomAltitude = 2500000;

		// used only in default render
		protected Sprite m_sprite;

		// used only in default render
		protected List<Rectangle> m_labelRectangles;

		protected Matrix4d lastView = Matrix4d.Identity;

		/// <summary>
		/// The context menu for this icon
		/// </summary>
		protected ContextMenu m_contextMenu = null;

		#endregion

		#region Properties

		/// <summary>
		/// Whether the name of this icon should always be rendered
		/// </summary>
		public bool NameAlwaysVisible
		{
			get { return m_nameAlwaysVisible; }
			set { m_nameAlwaysVisible = value; }
		}
		protected bool m_nameAlwaysVisible;

		/// <summary>
		/// Whether or not this icon should be rotated.
		/// </summary>
		public bool IsRotated
		{
			get { return m_isRotated; }
			set { m_isRotated = value; }
		}
		protected bool m_isRotated;

		/// <summary>
		/// The angle of rotation to display the icon's texture in degrees.
		/// </summary>
		public Angle Rotation
		{
			get { return m_rotation; }
			set { m_rotation = value; }
		}
		protected Angle m_rotation = Angle.Zero;

		/// <summary>
		/// Latitude (North/South) in decimal degrees
		/// </summary>
		public double Latitude
		{
			get { return m_latitude; }
			set
			{
				m_latitude = value;
				m_isUpdated = false;
			}
		}
		protected double m_latitude = 0.0;

		/// <summary>
		/// Longitude (East/West) in decimal degrees
		/// </summary>
		public double Longitude
		{
			get { return m_longitude; }
			set
			{
				m_longitude = value;
				m_isUpdated = false;
			}

		}
		protected double m_longitude = 0.0;

		/// <summary>
		/// The icon altitude above the surface 
		/// </summary>
		public double Altitude
		{
			get { return m_altitude; }
			set
			{
				m_altitude = value;
				m_isUpdated = false;
			}
		}
		protected double m_altitude;

		/// <summary>
		/// The cartesian coordinates of this icon.  
		/// Used to be settable but never actually updated the position of the icon.
		/// </summary>
		internal Point3d PositionD
		{
			get { return m_positionD; }
		}
		protected Point3d m_positionD = new Point3d();

		/// <summary>
		/// Icon bitmap path. (Overrides Image)
		/// </summary>
		internal string TextureFileName
		{
			get { return m_textureFileName; }
		}
		protected string m_textureFileName;

		protected IconTexture m_iconTexture;

		/// <summary>
		/// On-Click browse to location
		/// </summary>
		internal string ClickableActionURL
		{
			get
			{
				return m_clickableActionURL;
			}
		}
		protected string m_clickableActionURL;

		/// <summary>
		/// Whether or not a groundstick should be drawn
		/// </summary>
		public bool DrawGroundStick
		{
			get { return m_drawGroundStick; }
			set
			{
				m_drawGroundStick = value;
				m_isUpdated = false;
			}
		}
		protected bool m_drawGroundStick;

		/// <summary>
		/// Whether or not the labels should be decluttered
		/// </summary>
		public bool Declutter
		{
			get { return m_declutter; }
			set { m_declutter = value; }
		}
		private bool m_declutter;

		/// <summary>
		/// The color of the icon in its highlighted state.
		/// </summary>
		public Color HotColor
		{
			get { return Color.FromArgb(hotColor); }
			set { hotColor = value.ToArgb(); }
		}
		protected int hotColor = Color.White.ToArgb();

		/// <summary>
		/// The color of the icon in its normal state.
		/// </summary>
		public Color NormalColor
		{
			get { return Color.FromArgb(normalColor); }
			set { normalColor = value.ToArgb(); }
		}
		protected int normalColor = Color.FromArgb(150, 255, 255, 255).ToArgb();

		protected object m_id;
		protected object m_tag;

		/// <summary>
		/// If true the icon will autoscale based on altitude
		/// </summary>
		public bool AutoScaleIcon
		{
			get { return m_autoScaleIcon; }
			set { m_autoScaleIcon = value; }
		}
		protected bool m_autoScaleIcon;

        /// <summary>
        /// True if altitude is in AGL, False if ASL.  Default is AGL.
        /// </summary>
        internal bool IsAGL
        {
            get { return m_isAGL; }
        }
        protected bool m_isAGL = true;

        /// <summary>
        /// True if Vertical Exaggeration should be used in computing altitude.  
        /// Default is true.
        /// </summary>
        internal bool UseVE
        {
            get { return m_useVE; }
        }
        protected bool m_useVE = true;        
        
        /// <summary>
        /// True if a zero Vertical Exaggeration should be used in computing altitude.  
        /// If set then a VE of 0 forces altitude to 0 since its multiplied.  Ignored if UseVE is false.
        /// Default is true.
        /// </summary>
        internal bool UseZeroVE
        {
            get { return m_useZeroVE; }
        }
        protected bool m_useZeroVE = true;

        protected bool m_alwaysHighlight;

        protected bool m_disableMouseoverHighlight;

        protected bool m_onClickZoomTo = true;
		#endregion

		  public double OnClickZoomAltitude = double.NaN;
		  public double OnClickZoomHeading = double.NaN;
		  public double OnClickZoomTilt = double.NaN;
		internal string SaveFilePath = null;
		internal System.DateTime LastRefresh = System.DateTime.MinValue;
		internal System.TimeSpan RefreshInterval = System.TimeSpan.MaxValue;

		/// <summary>
		/// Longer description of icon (addition to name)
		/// </summary>
		//internal string Description;

		/// <summary>
		/// Icon image.  Leave TextureFileName=null if using Image.  
		/// Caller is responsible for disposing the Bitmap when the layer is removed, 
		/// either by calling Dispose on Icon or on the Image directly.
		/// </summary>
		public Bitmap Image
		{
			get { return m_image; }
			set
			{
				m_image = value;
				m_newTexture = true;
				m_isUpdated = false;
			}
		}
		private Bitmap m_image;

		/// <summary>
		/// Icon on-screen rendered width (pixels).  Defaults to icon image width.  
		/// If source image file is not a valid GDI+ image format, width may be increased to closest power of 2.
		/// </summary>
		public int Width;

		/// <summary>
		/// Icon on-screen rendered height (pixels).  Defaults to icon image height.  
		/// If source image file is not a valid GDI+ image format, height may be increased to closest power of 2.
		/// </summary>
		public int Height;

		/// <summary>
		///  Icon X scaling computed by dividing icon width by texture width
		/// </summary>
		internal float XScale;

		/// <summary>
		///  Icon Y scaling computed by dividing icon height by texture height 
		/// </summary>
		internal float YScale;

		/// <summary>
		/// The maximum distance (meters) the icon will be visible from
		/// </summary>
		internal double MaximumDisplayDistance = double.MaxValue;

		/// <summary>
		/// Bounding box centered at (0,0) used to calculate whether mouse is over icon/label
		/// </summary>
		internal Rectangle SelectionRectangle;

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.Renderable.Icon"/> class 
		/// </summary>
		/// <param name="name">Name of the icon</param>
		/// <param name="latitude">Latitude in decimal degrees.</param>
		/// <param name="longitude">Longitude in decimal degrees.</param>
		internal Icon(string name,
			double latitude,
			double longitude)
			: base(name)
		{
			m_latitude = latitude;
			m_longitude = longitude;
			this.RenderPriority = RenderPriority.Icons;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.Renderable.Icon"/> class 
		/// </summary>
		/// <param name="name">Name of the icon</param>
		/// <param name="latitude">Latitude in decimal degrees.</param>
		/// <param name="longitude">Longitude in decimal degrees.</param>
		/// <param name="heightAboveSurface">Icon height (meters) above sea level.</param>
		internal Icon(string name,
			double latitude, 
			double longitude,
            double heightAboveSurface)
			: this(name, latitude, longitude)
		{
			m_altitude = heightAboveSurface;
		}

		/// <summary>
		/// Sets the geographic position of the icon.
		/// </summary>
		/// <param name="latitude">Latitude in decimal degrees.</param>
		/// <param name="longitude">Longitude in decimal degrees.</param>
		internal virtual void SetPosition(double latitude, double longitude)
		{
			m_latitude = latitude;
			m_longitude = longitude;

			// Recalculate XYZ coordinates
			m_isUpdated = false;
		}

		/// <summary>
		/// Sets the geographic position of the icon.
		/// </summary>
		/// <param name="latitude">Latitude in decimal degrees.</param>
		/// <param name="longitude">Longitude in decimal degrees.</param>
		/// <param name="altitude">The icon altitude above sea level.</param>
		internal virtual void SetPosition(double latitude, double longitude, double altitude)
		{
			m_latitude = latitude;
			m_longitude = longitude;
			m_altitude = altitude;

			// Recalculate XYZ coordinates
			m_isUpdated = false;
		}

		#region RenderableObject methods

		public override void Initialize(DrawArgs drawArgs)
		{
			// get icon texture
			m_iconTexture = null;

			BuildIconTexture(drawArgs);

			if (m_drawGroundStick)
			{
				if (m_groundStick == null)
					m_groundStick = new Line(drawArgs.device);

				if (m_groundStickVector == null)
					m_groundStickVector = new Vector2[2];
			}

			m_isUpdated = false;
			Update(drawArgs);

			isInitialized = true;
		}

		/// <summary>
		/// Disposes the icon (when disabled)
		/// </summary>
		public override void Dispose()
		{
			try
			{
				IconTexture iconTexture = null;
				// decrement our count from textures - the icons class will clean up
				if ((TextureFileName != null) && (TextureFileName.Trim() != String.Empty))
					iconTexture = (IconTexture)DrawArgs.Textures[TextureFileName];
				else if (m_image != null)
					iconTexture = (IconTexture)DrawArgs.Textures[m_image];

				if (iconTexture != null)
				{
					iconTexture.ReferenceCount--;
				}

				if (m_sprite != null)
				{
					m_sprite.Dispose();
					m_sprite = null;
				}

				isInitialized = false;

			}
			finally
			{
				// base.Dispose();
			}

		}

		/// <summary>
		/// Updates where we are if the camera has changed position (and thereby might be using higher resolution terrain
		/// </summary>
		/// <param name="drawArgs"></param>
		public override void Update(DrawArgs drawArgs)
		{
			if (!m_isUpdated || (drawArgs.WorldCamera.ViewMatrix != lastView))
			{
				double elevation = drawArgs.WorldCamera.WorldRadius;
                double altitude;

                // altitude = World.Settings.VerticalExaggeration * Altitude;

                // Added this because if VE is set to zero then all floating icons fall to the earth.
                if (UseVE && (UseZeroVE || World.Settings.VerticalExaggeration > 0.1))
                    altitude = World.Settings.VerticalExaggeration * Altitude;
                else
                    altitude = Altitude;

				if (drawArgs.CurrentWorld.TerrainAccessor != null && drawArgs.WorldCamera.Altitude < 300000)
				{
					double samplesPerDegree = 50.0 / drawArgs.WorldCamera.ViewRange.Degrees;
					elevation += drawArgs.CurrentWorld.TerrainAccessor.GetElevationAt(m_latitude, m_longitude, samplesPerDegree) * World.Settings.VerticalExaggeration;
				}

                // we do this rather than zero out elevation because ground stick needs elevation if it exists.
                if (IsAGL)
                {
                    Position = MathEngine.SphericalToCartesian(m_latitude, m_longitude, altitude + elevation);

                    m_positionD = MathEngine.SphericalToCartesian(
                        Angle.FromDegrees(m_latitude),
                        Angle.FromDegrees(m_longitude),
                        altitude + elevation);
                }
                else
                {
                    Position = MathEngine.SphericalToCartesian(m_latitude, m_longitude, altitude + drawArgs.WorldCamera.WorldRadius);

                    m_positionD = MathEngine.SphericalToCartesian(
                        Angle.FromDegrees(m_latitude),
                        Angle.FromDegrees(m_longitude),
                        altitude + drawArgs.WorldCamera.WorldRadius);
                }

				if (m_drawGroundStick)
				{
					if (m_groundStick == null)
						m_groundStick = new Line(drawArgs.device);

					if (m_groundStickVector == null)
						m_groundStickVector = new Vector2[2];

					m_groundPoint = MathEngine.SphericalToCartesian(Latitude, Longitude, elevation);
				}

				lastView = drawArgs.WorldCamera.ViewMatrix;
			}

			if (m_newTexture)
			{
				BuildIconTexture(drawArgs);
			}

			m_isUpdated = true;
		}


		/// <summary>
		/// Builds the icon texture based on the saved texturefile name
		/// </summary>
		/// <param name="drawArgs"></param>
		protected virtual void BuildIconTexture(DrawArgs drawArgs)
		{
			try
			{
				object key = null;

				if (m_iconTexture != null)
				{
					m_iconTexture.ReferenceCount--;
				}

				if ((TextureFileName != null) && (TextureFileName.Trim() != String.Empty))
				{
					// Icon image from file
					m_iconTexture = (IconTexture)DrawArgs.Textures[TextureFileName];
					if (m_iconTexture == null)
					{
						key = TextureFileName;
						m_iconTexture = new IconTexture(drawArgs.device, TextureFileName);
					}
				}
				else
				{
					// Icon image from bitmap
					if (this.m_image != null)
					{
						m_iconTexture = (IconTexture)DrawArgs.Textures[this.m_image];
						if (m_iconTexture == null)
						{
							// Create new texture from image
							key = this.m_image;
							m_iconTexture = new IconTexture(drawArgs.device, this.m_image);
						}
					}
				}

				if (m_iconTexture != null)
				{
					m_iconTexture.ReferenceCount++;

					if (key != null)
					{
						// New texture, cache it
						DrawArgs.Textures.Add(key, m_iconTexture);
					}

					// Use default dimensions if not set
					if (this.Width == 0)
						this.Width = m_iconTexture.Width;
					if (this.Height == 0)
						this.Height = m_iconTexture.Height;
				}

				// Compute mouse over bounding boxes
				if (m_iconTexture == null)
				{
					// Label only 
					this.SelectionRectangle = drawArgs.defaultDrawingFont.MeasureString(null, this.Name, DrawTextFormat.None, 0);
				}
				else
				{
					// Icon only
					this.SelectionRectangle = new Rectangle(0, 0, this.Width, this.Height);
				}

				// Center the box at (0,0)
				this.SelectionRectangle.Offset(-this.SelectionRectangle.Width / 2, -this.SelectionRectangle.Height / 2);

				if (m_iconTexture != null)
				{
					this.XScale = (float)this.Width / m_iconTexture.Width;
					this.YScale = (float)this.Height / m_iconTexture.Height;

				}
				else
				{
					this.XScale = 1.0f;
					this.YScale = 1.0f;
				}
			}
			catch (Exception ex)
			{
				System.Console.WriteLine(ex.Message.ToString());
			}

			m_newTexture = false;

		}

		/// <summary>
		/// Render the icon.  This can be pretty slow so you should only stick an Icon on an Icons layer.
		/// </summary>
		/// <param name="drawArgs"></param>
		public override void Render(DrawArgs drawArgs)
		{
			// Do whatever pre-rendering we have to do
			PreRender(drawArgs);

			// If we're in view render
			if (drawArgs.WorldCamera.ViewFrustum.ContainsPoint(this.Position))
			{
				Point3d translationVector = new Point3d(
					  (PositionD.X - drawArgs.WorldCamera.ReferenceCenter.X),
					  (PositionD.Y - drawArgs.WorldCamera.ReferenceCenter.Y),
					  (PositionD.Z - drawArgs.WorldCamera.ReferenceCenter.Z));

				Point3d projectedPoint = drawArgs.WorldCamera.Project(translationVector);

				if (m_sprite == null)
					m_sprite = new Sprite(drawArgs.device);

				if (m_labelRectangles == null)
					m_labelRectangles = new List<Rectangle>();

				// Clear or we never redraw our label
				m_labelRectangles.Clear();

				m_sprite.Begin(SpriteFlags.AlphaBlend);

				FastRender(drawArgs, m_sprite, projectedPoint, false, m_labelRectangles);

				m_sprite.End();
			}

			// do whatever post rendering stuff we have to do
			PostRender(drawArgs);
		}

		/// <summary>
		/// Fast render is used to batch the renders of all icons on a layer into a single Sprite.Begin and End block.
		/// </summary>
		/// <param name="drawArgs">The drawing arguments</param>
		/// <param name="sprite">The sprite to use for drawing</param>
		/// <param name="projectedPoint">Where we are</param>
		/// <param name="isMouseOver">Whether we should render as a mouseover icon</param>
		internal virtual void FastRender(DrawArgs drawArgs, Sprite sprite, Point3d projectedPoint, bool isMouseOver, List<Rectangle> labelRectangles)
		{
			// Check icons for within "visual" range
			double distanceToIcon = (this.Position - drawArgs.WorldCamera.Position).Length;
			if (distanceToIcon > this.MaximumDisplayDistance)
				return;
			if (distanceToIcon < 0.0)
				return;

			if (!this.isInitialized)
				this.Initialize(drawArgs);

			if ((!this.m_isUpdated) || (drawArgs.WorldCamera.Altitude < 300000))
			{
				this.Update(drawArgs);
			}

			int color = normalColor;

			if ((!m_disableMouseoverHighlight && isMouseOver) || m_alwaysHighlight)
				color = hotColor;

			color = Color.FromArgb(this.Opacity, Color.FromArgb(color)).ToArgb();

			// Render the label if necessary
			if (m_iconTexture == null || isMouseOver || NameAlwaysVisible)
			{
				RenderLabel(drawArgs, sprite, projectedPoint, color, labelRectangles);
			}

			// render the icon image
			if (m_iconTexture != null)
			{
				RenderTexture(drawArgs, sprite, projectedPoint, color);
			}

			// Depending on JIT to correctly inline (or not) these calls
			RenderGroundStick(drawArgs, sprite, projectedPoint, color);

			if (isMouseOver)
				RenderDescription(drawArgs, sprite, projectedPoint, color);

		}

		/// <summary>
		/// Renders the overlays for this icon
		/// </summary>
		/// <param name="drawArgs"></param>
		internal virtual void RenderOverlay(DrawArgs drawArgs)
		{
		}

		/// <summary>
		/// Helper function to render icon label.  Broken out so that child classes can override this behavior.
		/// </summary>
		/// <param name="drawArgs"></param>
		/// <param name="sprite"></param>
		/// <param name="projectedPoint"></param>
		/// <param name="color"></param>
		protected virtual void RenderLabel(DrawArgs drawArgs, Sprite sprite, Point3d projectedPoint, int color, List<Rectangle> labelRectangles)
		{
			if (this.Name != null)
			{
				if (m_iconTexture == null)
				{
					// Original Icon Label Render code

					// KML Label Render Code with Declutter

					// Center over target as we have no bitmap
					Rectangle realrect = drawArgs.defaultDrawingFont.MeasureString(m_sprite, Name, DrawTextFormat.WordBreak, color);
					realrect.X = (int)projectedPoint.X - (realrect.Width >> 1);
					realrect.Y = (int)(projectedPoint.Y - (drawArgs.defaultDrawingFont.Description.Height >> 1));

					bool bDraw = true;

					// Only not show if declutter is turned on and we aren't always supposed to be seen
					if (Declutter && !NameAlwaysVisible)
					{
						foreach (Rectangle drawnrect in labelRectangles)
						{
							if (realrect.IntersectsWith(drawnrect))
							{
								bDraw = false;
								break;
							}
						}
					}

					if (bDraw)
					{
						labelRectangles.Add(realrect);

						drawArgs.defaultDrawingFont.DrawText(m_sprite, Name, realrect, DrawTextFormat.WordBreak, color);
					}
				}
				else
				{
					// KML Label Render Code with Declutter

					// Adjust text to make room for icon
					int spacing = (int)(Width * 0.3f);
					if (spacing > 5)
						spacing = 5;
					int offsetForIcon = (Width >> 1) + spacing;

					// Text to the right
					Rectangle rightrect = drawArgs.defaultDrawingFont.MeasureString(m_sprite, Name, DrawTextFormat.WordBreak, color);
					rightrect.X = (int)projectedPoint.X + offsetForIcon;
					rightrect.Y = (int)(projectedPoint.Y - (drawArgs.defaultDrawingFont.Description.Height >> 1));

					// Text to the left
					Rectangle leftrect = drawArgs.defaultDrawingFont.MeasureString(m_sprite, Name, DrawTextFormat.WordBreak, color);
					leftrect.X = (int)projectedPoint.X - offsetForIcon - rightrect.Width;
					leftrect.Y = (int)(projectedPoint.Y - (drawArgs.defaultDrawingFont.Description.Height >> 1));

					bool bDrawRight = true;
					bool bDrawLeft = true;

					// Only not show if declutter is turned on and we aren't always supposed to be seen
					if (Declutter && !NameAlwaysVisible)
					{
						foreach (Rectangle drawnrect in labelRectangles)
						{
							if (rightrect.IntersectsWith(drawnrect))
							{
								bDrawRight = false;
							}
							if (leftrect.IntersectsWith(drawnrect))
							{
								bDrawLeft = false;
							}
							if (!bDrawRight && !bDrawLeft)
							{
								break;
							}
						}
					}

					// draw either right or left if we have space.  If we don't too bad.
					if (bDrawRight)
					{
						labelRectangles.Add(rightrect);
						drawArgs.defaultDrawingFont.DrawText(m_sprite, Name, rightrect, DrawTextFormat.WordBreak, color);
					}
					else if (bDrawLeft)
					{
						labelRectangles.Add(leftrect);
						drawArgs.defaultDrawingFont.DrawText(m_sprite, Name, leftrect, DrawTextFormat.WordBreak, color);
					}
				}
			}
		}

		/// <summary>
		/// Helper function to render icon texture.  Broken out so that child classes can override this behavior.
		/// </summary>
		/// <param name="drawArgs"></param>
		/// <param name="sprite"></param>
		/// <param name="projectedPoint"></param>
		/// <param name="color">the color to render the icon</param>
		protected virtual void RenderTexture(DrawArgs drawArgs, Sprite sprite, Point3d projectedPoint, int color)
		{
			Matrix4d scaleTransform;
			Matrix4d rotationTransform;

			//Do Altitude depedent scaling for KMLIcons
			if (AutoScaleIcon)
			{
				float factor = 1;
				if (drawArgs.WorldCamera.Altitude > minIconZoomAltitude)
					factor -= (float)((drawArgs.WorldCamera.Altitude - minIconZoomAltitude) / drawArgs.WorldCamera.Altitude);
				if (factor < 0.20) factor = 0.20F;

				XScale = factor * ((float)Width / m_iconTexture.Width);
				YScale = factor * ((float)Height / m_iconTexture.Height);
			}

			//scale and rotate image
			scaleTransform = Matrix4d.Scaling(this.XScale, this.YScale, 0);

			if (m_isRotated)
				rotationTransform = Matrix4d.RotationZ((float)m_rotation.Radians - (float)drawArgs.WorldCamera.Heading.Radians);
			else
				rotationTransform = Matrix4d.Identity;

			sprite.Transform = ConvertDX.FromMatrix4d(scaleTransform * rotationTransform * Matrix4d.Translation(projectedPoint.X, projectedPoint.Y, 0));
			sprite.Draw(m_iconTexture.Texture,
				 new Vector3(m_iconTexture.Width >> 1, m_iconTexture.Height >> 1, 0),
				 Vector3.Empty,
				 color);

			// Reset transform to prepare for text rendering later
			sprite.Transform = Matrix.Identity;
		}

		/// <summary>
		/// Helper function to render the groundstick
		/// </summary>
		/// <param name="drawArgs"></param>
		/// <param name="projectedPoint"></param>
		protected virtual void RenderGroundStick(DrawArgs drawArgs, Sprite sprite, Point3d projectedPoint, int color)
		{
			if (m_drawGroundStick)
			{
				Point3d referenceCenter = new Point3d(
					 drawArgs.WorldCamera.ReferenceCenter.X,
					 drawArgs.WorldCamera.ReferenceCenter.Y,
					 drawArgs.WorldCamera.ReferenceCenter.Z);

				Point3d projectedGroundPoint = drawArgs.WorldCamera.Project(m_groundPoint - referenceCenter);

				m_groundStick.Begin();
				m_groundStickVector[0].X = (float)projectedPoint.X;
				m_groundStickVector[0].Y = (float)projectedPoint.Y;
				m_groundStickVector[1].X = (float)projectedGroundPoint.X;
				m_groundStickVector[1].Y = (float)projectedGroundPoint.Y;

				m_groundStick.Draw(m_groundStickVector, color);
				m_groundStick.End();
			}
		}

		/// <summary>
		/// Helper function to render icon description.  Broken out so that child classes can override this behavior.
		/// </summary>
		/// <param name="drawArgs"></param>
		protected virtual void RenderDescription(DrawArgs drawArgs, Sprite sprite, Point3d projectedPoint, int color)
		{
			string description = this.Description;

			if (description == null)
				description = ClickableActionURL;

			if (description != null)
			{
				// Render description field
				DrawTextFormat format = DrawTextFormat.NoClip | DrawTextFormat.WordBreak | DrawTextFormat.Bottom;
				int left = 10;
				Rectangle rect = Rectangle.FromLTRB(left, 10, drawArgs.screenWidth - 10, drawArgs.screenHeight - 10);

				// Draw outline
				drawArgs.defaultDrawingFont.DrawText(
					 sprite, description,
					 rect,
					 format, 0xb0 << 24);

				rect.Offset(2, 0);
				drawArgs.defaultDrawingFont.DrawText(
					 sprite, description,
					 rect,
					 format, 0xb0 << 24);

				rect.Offset(0, 2);
				drawArgs.defaultDrawingFont.DrawText(
					 sprite, description,
					 rect,
					 format, 0xb0 << 24);

				rect.Offset(-2, 0);
				drawArgs.defaultDrawingFont.DrawText(
					 sprite, description,
					 rect,
					 format, 0xb0 << 24);

				// Draw description
				rect.Offset(1, -1);
				drawArgs.defaultDrawingFont.DrawText(
					 sprite, description,
					 rect,
					 format, descriptionColor);
			}
		}

		/// <summary>
		/// Does whatever you need to do before you render the icon.  Occurs even if the icon isn't visible!
		/// </summary>
		/// <param name="drawArgs"></param>
		/// <param name="sprite"></param>
		/// <param name="projectedPoint"></param>
		/// <param name="color"></param>
		internal virtual void PreRender(DrawArgs drawArgs)
		{
		}

		/// <summary>
		/// Does whatever you need to do after you render the icon.  Occurs even if the icon isn't visible!
		/// </summary>
		/// <param name="drawArgs"></param>
		/// <param name="sprite"></param>
		/// <param name="projectedPoint"></param>
		/// <param name="color"></param>
		internal virtual void PostRender(DrawArgs drawArgs)
		{
		}

		/// <summary>
		/// Do this if we don't actually get rendered (not in view, too far, etc)
		/// </summary>
		/// <param name="drawArgs"></param>
		internal virtual void NoRender(DrawArgs drawArgs)
		{
		}

		#endregion

		protected void RefreshTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
		}
	}
}
