//
// Copyright © 2005 NASA. Available under the NOSA License
//
// Portions copied from JHU_Icons - Copyright © 2005-2006 The Johns Hopkins University 
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
	/// Holds a collection of icons
	/// </summary>
	public class Icons : RenderableObjectList
	{
		protected Sprite m_sprite;

        protected bool m_mouseOver;

		protected bool m_needToInitChildren = true;

		protected List<RenderableObject> m_childrenToInit;

		protected static int hotColor = Color.White.ToArgb();
		protected static int normalColor = Color.FromArgb(150,255,255,255).ToArgb();
		protected static int nameColor = Color.White.ToArgb();
		protected static int descriptionColor = Color.White.ToArgb();

        protected const int minIconZoomAltitude = 5000000;

		System.Timers.Timer refreshTimer;

        /// <summary>
        /// This list holds all the rendered labels rectangles for declutter purposes
        /// </summary>
        protected List<Rectangle> m_labelRectangles = new List<Rectangle>();

		/// <summary>
		/// The closest icon the mouse is currently over
		/// </summary>
		protected Icon mouseOverIcon;

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.Renderable.Icons"/> class 
		/// </summary>
		/// <param name="name">The name of the icons layer</param>
		public Icons(string name)
			: base(name) 
		{
            m_mouseOver = true;
            isInitialized = false;
            m_needToInitChildren = true;
            m_childrenToInit = new List<RenderableObject>();
			this.RenderPriority = RenderPriority.Icons;
		}

        /// <summary>
        /// Initializes a new instance of the <see cref= "T:WorldWind.Renderable.Icons"/> class 
        /// Sets up refresh of this layer from a data source.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="dataSource"></param>
        /// <param name="refreshInterval"></param>
        /// <param name="parentWorld"></param>
        /// <param name="cache"></param>
		internal Icons(string name, 
			string dataSource, 
			TimeSpan refreshInterval,
			World parentWorld,
			Cache cache) : base(name, dataSource, refreshInterval, parentWorld, cache) 
		{
            m_mouseOver = true;
            isInitialized = false;
            m_needToInitChildren = true;
            m_childrenToInit = new List<RenderableObject>();
		}

        /// <summary>
        /// Adds an icon to this layer.
        /// </summary>
        /// <param name="icon">The icon to add to this Icons layer.  Deprecateed.  Use Add(ro).</param>
        [Obsolete]
        internal virtual void AddIcon(Icon icon)
		{
			Add(icon);
		}

		#region RenderableObject methods


        /// <summary>
        /// Add a child object to this layer.
        /// </summary>
        /// <param name="ro">The renderable object to add to this layer</param>
		  public override void Add(RenderableObject ro)
		{
            ro.ParentList = this;

			m_children.Add(ro);
            m_childrenToInit.Add(ro);
			isInitialized = false;

            // force an initialize on all children
            m_needToInitChildren = true;
		}

		public override void Initialize(DrawArgs drawArgs)
		{
			if(!isOn)
				return;

            System.TimeSpan smallestRefreshInterval = System.TimeSpan.MaxValue;

            if (!isInitialized)
            {
                if (m_sprite != null)
                {
                    m_sprite.Dispose();
                    m_sprite = null;
                }

                // init Icons layer
                m_sprite = new Sprite(drawArgs.device);

                if (refreshTimer == null && smallestRefreshInterval != TimeSpan.MaxValue)
                {
                    refreshTimer = new System.Timers.Timer(smallestRefreshInterval.TotalMilliseconds);
                    refreshTimer.Elapsed += new System.Timers.ElapsedEventHandler(refreshTimer_Elapsed);
                    refreshTimer.Start();
                }

                // force the init of all children
                m_needToInitChildren = true;
                m_childrenToInit.Clear();
            }

            // figure out refresh period
            foreach (RenderableObject ro in m_children)
            {
                Icon icon = ro as Icon;
                if (icon != null)
                {
                    if (icon.RefreshInterval.TotalMilliseconds != 0 && icon.RefreshInterval != TimeSpan.MaxValue && icon.RefreshInterval < smallestRefreshInterval)
                        smallestRefreshInterval = icon.RefreshInterval;
                }
            }

            InitializeChildren(drawArgs);

			isInitialized = true;
		}

        protected void InitializeChildren(DrawArgs drawArgs)
        {
            if (m_needToInitChildren)
            {
                // if we stuff in the init list just do those otherwise do all children
                if (m_childrenToInit.Count != 0)
                {
                    foreach (RenderableObject ro in m_childrenToInit)
                    {
                        if (ro.IsOn)
                            ro.Initialize(drawArgs);
                    }
                    m_childrenToInit.Clear();
                }
                else
                {
                    // initialize all children
                    foreach (RenderableObject ro in m_children)
                    {
                        if (ro.IsOn)
                            ro.Initialize(drawArgs);
                        continue;
                    }
                }
                m_needToInitChildren = false;
            }
        }

		  public override void Dispose()
		{

            try
            {
                // call rol dispose to get all the children disposed first
                base.Dispose();

                // remove any textures no longer being used.
                if (DrawArgs.Textures != null)
                {
                    List<object> removeList = new List<object>();
                    foreach (object key in DrawArgs.Textures.Keys)
                    {
                        IconTexture iconTexture = (IconTexture)DrawArgs.Textures[key];
                        if (iconTexture.ReferenceCount <= 0)
                        {
                            removeList.Add(key);
                        }
                    }

                    foreach (object key in removeList)
                    {
                        IconTexture iconTexture = (IconTexture)DrawArgs.Textures[key];
                        DrawArgs.Textures.Remove(key);
                        iconTexture.Dispose();
                    }
                }

                if (m_sprite != null)
                {
                    m_sprite.Dispose();
                    m_sprite = null;
                }

                if (refreshTimer != null)
                {
                    refreshTimer.Stop();
                    refreshTimer.Dispose();
                    refreshTimer = null;
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message.ToString());
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

				foreach (RenderableObject oChild in m_children)
				{
					oChild.Opacity = value;
				}
			}
		}

		public override bool PerformSelectionAction(DrawArgs drawArgs)
        {
            int closestIconDistanceSquared = int.MaxValue;
            Icon closestIcon = null;

            foreach (RenderableObject ro in m_children)
            {
                if (!ro.IsOn)
                    continue;
                if (!ro.isSelectable)
                    continue;

                Icon icon = ro as Icon;

                // if it's not an icon just do the normal selection action
                if (icon == null)
                {
                    // Child is not an icon
                    if (ro.PerformSelectionAction(drawArgs))
                        return true;
                }
                else
                {
                    // don't check if we aren't even in view
                    if (drawArgs.WorldCamera.ViewFrustum.ContainsPoint(icon.Position))
                    {

                        // check if inside the icons bounding box
                        Point3d referenceCenter = new Point3d(
                            drawArgs.WorldCamera.ReferenceCenter.X,
                            drawArgs.WorldCamera.ReferenceCenter.Y,
                            drawArgs.WorldCamera.ReferenceCenter.Z);

                        Point3d projectedPoint = drawArgs.WorldCamera.Project(icon.Position - referenceCenter);

                        int dx = DrawArgs.LastMousePosition.X - (int)projectedPoint.X;
                        int dy = DrawArgs.LastMousePosition.Y - (int)projectedPoint.Y;
                        
                        if (icon.SelectionRectangle.Contains( dx, dy ))
                        {
                            // Mouse is over, check whether this icon is closest
                            int distanceSquared = dx * dx + dy * dy;
                            if (distanceSquared < closestIconDistanceSquared)
                            {
                                closestIconDistanceSquared = distanceSquared;
                                closestIcon = icon;
                            }
                        }
                    }
                }
            }

            // if no other object has handled the selection let the closest icon try
            if (closestIcon != null)
            {
                return closestIcon.PerformSelectionAction(drawArgs);
            }

			return false;
		}

		public override void Render(DrawArgs drawArgs)
		{
			if(!isOn)
				return;

			if(!isInitialized)
				return;

            if (m_needToInitChildren)
                this.InitializeChildren(drawArgs);

            // First render everything except icons - do we need this or can we collapse into other loop?
            foreach (RenderableObject ro in m_children)
            {
                if (ro is Icon)
                    continue;

                if (!ro.IsOn)
                    continue;

                // Child is not an icon
                ro.Render(drawArgs);
            }

            m_labelRectangles.Clear();

			int closestIconDistanceSquared = int.MaxValue;
			Icon closestIcon = null;

            m_sprite.Begin(SpriteFlags.AlphaBlend);
            try
            {
                // Now render just the icons
                foreach (RenderableObject ro in m_children)
                {
                    if (!ro.IsOn)
                        continue;

                    Icon icon = ro as Icon;
                    if (icon == null)
                        continue;

                    // render the overlay regardless of everything else
                    icon.PreRender(drawArgs);

                    // don't bother to do anything else if we aren't even in view
                    if (drawArgs.WorldCamera.ViewFrustum.ContainsPoint(icon.Position))
                    {

                        Point3d translationVector = new Point3d(
                        (icon.PositionD.X - drawArgs.WorldCamera.ReferenceCenter.X),
                        (icon.PositionD.Y - drawArgs.WorldCamera.ReferenceCenter.Y),
                        (icon.PositionD.Z - drawArgs.WorldCamera.ReferenceCenter.Z));

                        Point3d projectedPoint = drawArgs.WorldCamera.Project(translationVector);

                        // check if inside bounding box of icon
                        int dx = DrawArgs.LastMousePosition.X - (int)projectedPoint.X;
                        int dy = DrawArgs.LastMousePosition.Y - (int)projectedPoint.Y;
                        if (icon.SelectionRectangle.Contains(dx, dy))
                        {
                            // Mouse is over, check whether this icon is closest
                            int distanceSquared = dx * dx + dy * dy;
                            if (distanceSquared < closestIconDistanceSquared)
                            {
                                closestIconDistanceSquared = distanceSquared;
                                closestIcon = icon;
                            }
                        }

                        // mouseover is always one render cycle behind...we mark that an icon is the mouseover
                        // icon and render it normally.  On the NEXT pass it renders as a mouseover icon
                        if (icon != mouseOverIcon)
                            icon.FastRender(drawArgs, m_sprite, projectedPoint, false, m_labelRectangles);
                    }
                    else
                    {
                        icon.NoRender(drawArgs);
                    }

                    // do post rendering even if we don't render
                    icon.PostRender(drawArgs);
                }

                // Clear the rectangles so that mouseover label always appears
                m_labelRectangles.Clear();

                // Render the mouse over icon last (on top)
                if (mouseOverIcon != null)
                {
                    Point3d translationVector = new Point3d(
                        (mouseOverIcon.PositionD.X - drawArgs.WorldCamera.ReferenceCenter.X),
                        (mouseOverIcon.PositionD.Y - drawArgs.WorldCamera.ReferenceCenter.Y),
                        (mouseOverIcon.PositionD.Z - drawArgs.WorldCamera.ReferenceCenter.Z));

                    Point3d projectedPoint = drawArgs.WorldCamera.Project(translationVector);

                    mouseOverIcon.FastRender(drawArgs, m_sprite, projectedPoint, true, m_labelRectangles);
                }

                // set new mouseover icon
                mouseOverIcon = closestIcon;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message.ToString());
            }
            finally
            {
                m_sprite.End();
            }
		}

		#endregion

		/// <summary>
		/// Draw the icon
		/// </summary>
        [Obsolete]
		protected virtual void Render(DrawArgs drawArgs, Icon icon, Point3d projectedPoint)
		{
			if (!icon.isInitialized)
				icon.Initialize(drawArgs);

			if(!drawArgs.WorldCamera.ViewFrustum.ContainsPoint(icon.Position))
				return;

			// Check icons for within "visual" range
			double distanceToIcon = (icon.Position - drawArgs.WorldCamera.Position).Length;
			if(distanceToIcon > icon.MaximumDisplayDistance)
				return;
			if(distanceToIcon < icon.MinimumDisplayDistance)
				return;

			IconTexture iconTexture = GetTexture(icon);
			bool isMouseOver = icon == mouseOverIcon;

            //Show description for normal Icons at the bottom
            //of the page
			if(isMouseOver) // && !icon.IsKMLIcon)
			{
				// Mouse is over
				isMouseOver = true;

				if(icon.isSelectable)
					DrawArgs.MouseCursor = CursorType.Hand;

				string description = icon.Description;
				if(description==null)
					description = icon.ClickableActionURL;
				if(description!=null)
				{
					// Render description field
					DrawTextFormat format = DrawTextFormat.NoClip | DrawTextFormat.WordBreak | DrawTextFormat.Bottom;
					int left = 10;
					if(World.Settings.showLayerManager)
						left += World.Settings.layerManagerWidth;
					Rectangle rect = Rectangle.FromLTRB(left, 10, drawArgs.screenWidth - 10, drawArgs.screenHeight - 10 );

					// Draw outline
					drawArgs.defaultDrawingFont.DrawText(
						m_sprite, description,
						rect,
						format, 0xb0 << 24 );
					
					rect.Offset(2,0);
					drawArgs.defaultDrawingFont.DrawText(
						m_sprite, description,
						rect,
						format, 0xb0 << 24 );

					rect.Offset(0,2);
					drawArgs.defaultDrawingFont.DrawText(
						m_sprite, description,
						rect,
						format, 0xb0 << 24 );

					rect.Offset(-2,0);
					drawArgs.defaultDrawingFont.DrawText(
						m_sprite, description,
						rect,
						format, 0xb0 << 24 );

					// Draw description
					rect.Offset(1,-1);
					drawArgs.defaultDrawingFont.DrawText(
						m_sprite, description,
						rect, 
						format, descriptionColor );
				}
			}

			int color = isMouseOver ? hotColor : normalColor;
			if(iconTexture==null || isMouseOver || icon.NameAlwaysVisible)
			{
				// Render label
				if(icon.Name != null)
				{
					// Render name field
					const int labelWidth = 1000; // Dummy value needed for centering the text
					if(iconTexture==null)
					{
						// Center over target as we have no bitmap
						Rectangle rect = new Rectangle(
							(int)projectedPoint.X - (labelWidth>>1), 
							(int)(projectedPoint.Y - (drawArgs.defaultDrawingFont.Description.Height >> 1)),
							labelWidth, 
							drawArgs.screenHeight );

						drawArgs.defaultDrawingFont.DrawText(m_sprite, icon.Name, rect, DrawTextFormat.Center, color);
					}
					else
					{
						// Adjust text to make room for icon
						int spacing = (int)(icon.Width * 0.3f);
						if(spacing>10)
							spacing = 10;
						int offsetForIcon = (icon.Width>>1) + spacing;

						Rectangle rect = new Rectangle(
							(int)projectedPoint.X + offsetForIcon, 
							(int)(projectedPoint.Y - (drawArgs.defaultDrawingFont.Description.Height >> 1)),
							labelWidth, 
							drawArgs.screenHeight );

						drawArgs.defaultDrawingFont.DrawText(m_sprite, icon.Name, rect, DrawTextFormat.WordBreak, color);
					}
				}
			}

			if(iconTexture!=null)
			{
				// Render icon
                float factor = 1;
                //Do Altitude depedent scaling for KMLIcons
                //if(icon.IsKMLIcon)
                //    if (drawArgs.WorldCamera.Altitude > minIconZoomAltitude)
                //        factor -= (float)((drawArgs.WorldCamera.Altitude - minIconZoomAltitude) / drawArgs.WorldCamera.Altitude);

                float xscale = factor * ((float)icon.Width / iconTexture.Width);
                float yscale = factor * ((float)icon.Height / iconTexture.Height); 
                m_sprite.Transform = Matrix.Scaling(xscale, yscale, 0);

				if(icon.IsRotated)
					m_sprite.Transform *= Matrix.RotationZ((float)icon.Rotation.Radians - (float)drawArgs.WorldCamera.Heading.Radians);

            m_sprite.Transform *= Matrix.Translation((float)projectedPoint.X, (float)projectedPoint.Y, 0);
				m_sprite.Draw( iconTexture.Texture,
					new Vector3(iconTexture.Width>>1, iconTexture.Height>>1,0),
					Vector3.Empty,
					color );
				
				// Reset transform to prepare for text rendering later
				m_sprite.Transform = Matrix.Identity;
			}
		}

		/// <summary>
		/// Retrieve an icon's texture
		/// </summary>
		protected IconTexture GetTexture(Icon icon)
		{
			object key = null;
			
			if(icon.Image == null)
			{
				key = icon.TextureFileName;
			}
			else
			{
				key = icon.Image;
			}
			if(key==null)
				return null;

            IconTexture res = (IconTexture)DrawArgs.Textures[key];
			return res;
		}

		bool isUpdating;
		private void refreshTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			if(isUpdating)
				return;
			isUpdating = true;
			try
			{
				for(int i = 0; i < this.ChildObjects.Count; i++)
				{
					RenderableObject ro = (RenderableObject)this.ChildObjects[i];
					if(ro != null && ro.IsOn && ro is Icon)
					{
						Icon icon = (Icon)ro;

						if(icon.RefreshInterval == TimeSpan.MaxValue || icon.LastRefresh > System.DateTime.Now - icon.RefreshInterval)
							continue;

						object key = null;
						IconTexture iconTexture = null;

                        if (icon.TextureFileName != null && icon.TextureFileName.Length > 0)
                        {
                            iconTexture = (IconTexture)DrawArgs.Textures[icon.TextureFileName];
                            if (iconTexture != null)
                            {
                                iconTexture.UpdateTexture(DrawArgs.Device, icon.TextureFileName);
                            }
                            else
                            {
                                key = icon.TextureFileName;
                                iconTexture = new IconTexture(DrawArgs.Device, icon.TextureFileName);

                                iconTexture.ReferenceCount++;

                                // New texture, cache it
                                DrawArgs.Textures.Add(key, iconTexture);

                                // Use default dimensions if not set
                                if (icon.Width == 0)
                                    icon.Width = iconTexture.Width;
                                if (icon.Height == 0)
                                    icon.Height = iconTexture.Height;
                            }

                        }
                        else
                        {
                            // Icon image from bitmap
                            if (icon.Image != null)
                            {
                                iconTexture = (IconTexture)DrawArgs.Textures[icon.Image];
                                if (iconTexture != null)
                                {
                                    IconTexture tempTexture = iconTexture;
                                    DrawArgs.Textures[icon.SaveFilePath] = new IconTexture(DrawArgs.Device, icon.Image);
                                    tempTexture.Dispose();
                                }
                                else
                                {
                                    key = icon.SaveFilePath;
                                    iconTexture = new IconTexture(DrawArgs.Device, icon.Image);

                                    // New texture, cache it
                                    DrawArgs.Textures.Add(key, iconTexture);

                                    // Use default dimensions if not set
                                    if (icon.Width == 0)
                                        icon.Width = iconTexture.Width;
                                    if (icon.Height == 0)
                                        icon.Height = iconTexture.Height;
                                }
                            }
                        }

						icon.LastRefresh = System.DateTime.Now;
					}
				}
			}
			catch{}
			finally
			{
				isUpdating = false;
			}
		}
	}
}
