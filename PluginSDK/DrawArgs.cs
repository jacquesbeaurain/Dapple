using System;
using System.Diagnostics;
using System.Collections;
using System.IO;
using System.Net;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using WorldWind.Camera;
using WorldWind;
using WorldWind.Net;
using Utility;

namespace WorldWind
{
	/// <summary>
	/// 
	/// </summary>
	public class DrawArgs : IDisposable
	{
		public Device device;
		internal static System.Windows.Forms.Control ParentControl = null;
		public int numBoundaryPointsTotal;
		public int numBoundaryPointsRendered;
		public int numBoundariesDrawn;
		public Font defaultDrawingFont;
		internal System.Drawing.Font defaultSubTitleFont;
		internal Font defaultSubTitleDrawingFont;
		public int screenWidth;
		public int screenHeight;
		public static System.Drawing.Point LastMousePosition;
		public int numberTilesDrawn;
		public string UpperLeftCornerText = "";
		CameraBase m_WorldCamera;
		internal World m_CurrentWorld = null;
		public static bool IsLeftMouseButtonDown;
		public static bool IsRightMouseButtonDown;
		internal static DownloadQueue DownloadQueue = new DownloadQueue();
		public static WorldWind.Widgets.RootWidget RootWidget = null;
		  public static WorldWind.NewWidgets.RootWidget NewRootWidget = null;

		public bool RenderWireFrame;

        /// <summary>
        /// Table of all icon textures
        /// </summary>
        protected static Hashtable m_textures= new Hashtable();
        internal static Hashtable Textures
        {
            get { return m_textures; }
        }

		  public static CameraBase Camera = null;
		public CameraBase WorldCamera
		{
			get
			{
				return m_WorldCamera;
			}	
			set
			{
				m_WorldCamera = value;
				Camera = value;
			}
		}

		  public World CurrentWorld
		{
			get
			{
				return m_CurrentWorld;
			}
			set
			{
				m_CurrentWorld = value;
			}
		}

		/// <summary>
		/// Absolute time of current frame render start (ticks)
		/// </summary>
		public static long CurrentFrameStartTicks;

		static CursorType mouseCursor;
		static CursorType lastCursor;
		Hashtable fontList = new Hashtable();

		public static Device Device = null;
		System.Windows.Forms.Cursor measureCursor;
		
		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.DrawArgs"/> class.
		/// </summary>
		/// <param name="device"></param>
		/// <param name="parentForm"></param>
		public DrawArgs(Device device, System.Windows.Forms.Control parentForm)
		{
			DrawArgs.ParentControl = parentForm;
			DrawArgs.Device = device;
			this.device = device;
			defaultDrawingFont = CreateFont( World.Settings.defaultFontName, World.Settings.defaultFontSize );
			if(defaultDrawingFont==null)
				defaultDrawingFont = CreateFont( "", 10 );

			defaultSubTitleFont = new System.Drawing.Font("Ariel", 8.0f);
			defaultSubTitleDrawingFont = new Font(device, defaultSubTitleFont);
			if(defaultSubTitleDrawingFont==null)
				defaultSubTitleDrawingFont = CreateFont( "", 8 );
		}

		public void BeginRender()
		{
			// Development variable to see the number of tiles drawn - Added for frustum culling testing
			this.numberTilesDrawn = 0;

			this.UpperLeftCornerText = "";
			this.numBoundaryPointsRendered = 0;
			this.numBoundaryPointsTotal = 0;
			this.numBoundariesDrawn = 0;
		}

		public void EndRender()
		{
		}

		/// <summary>
		/// Displays the rendered image (call after EndRender)
		/// </summary>
		/// <remarks>
		/// Calls to this method should have the WorldWindow's m_oSceneLock locked to prevent DirectX InvalidCallExceptions.
		/// </remarks>
		public void Present()
		{
			// Display the render
			device.Present();
		}

		/// <summary>
		/// Creates a font.
		/// </summary>
		public Font CreateFont(string familyName, float emSize)
		{
			return CreateFont( familyName, emSize, System.Drawing.FontStyle.Regular );
		}

		/// <summary>
		/// Creates a font.
		/// </summary>
		internal Font CreateFont( string familyName, float emSize, System.Drawing.FontStyle style )
		{
			try
			{
				FontDescription description = new FontDescription();
				description.FaceName = familyName;
				description.Height = (int)(1.9*emSize);

				if(style == System.Drawing.FontStyle.Regular)
					return CreateFont( description );
				if((style & System.Drawing.FontStyle.Italic) != 0)
					description.IsItalic = true;
				if((style & System.Drawing.FontStyle.Bold) != 0)
					description.Weight = FontWeight.Heavy;
                description.Quality = FontQuality.AntiAliased;
				return CreateFont( description );
			}
			catch
			{
				Log.Write(Log.Levels.Error, "FONT", string.Format("Unable to load '{0}' {2} ({1}em)", 
					familyName, emSize, style ) );
				return defaultDrawingFont;
			}
		}

		/// <summary>
		/// Creates a font.
		/// </summary>
		internal Font CreateFont( FontDescription description )
		{
			try
			{
				if (World.Settings.AntiAliasedText)
					description.Quality = FontQuality.ClearTypeNatural;
				else
					description.Quality = FontQuality.Default;

				// TODO: Improve font cache
				string hash = description.ToString();//.GetHashCode(); returned hash codes are not correct

				Font font = (Font)fontList[ hash ];
				if(font!=null)
					return font;

				font = new Font( this.device, description );
				//newDrawingFont.PreloadText("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXRZ");
				fontList.Add( hash, font );
				return font;
			}
			catch
			{
				Log.Write(Log.Levels.Error, "FONT", string.Format("Unable to load '{0}' (Height: {1})", description.FaceName, description.Height) );
				return defaultDrawingFont;
			}
		}

		/// <summary>
		/// Active mouse cursor
		/// </summary>
		public static CursorType MouseCursor
		{
			get 
			{ 
				return mouseCursor; 
			}
			set 
			{ 
				mouseCursor = value; 
			}
		}

		public void UpdateMouseCursor(System.Windows.Forms.Control parent)
		{
			if(lastCursor == mouseCursor)
				return;

			switch( mouseCursor )
			{
				case CursorType.Hand:
					parent.Cursor = System.Windows.Forms.Cursors.Hand;
					break;
				case CursorType.Cross:
					parent.Cursor = System.Windows.Forms.Cursors.Cross;
					break;
				case CursorType.Measure:
					if(measureCursor == null)
						measureCursor = ImageHelper.LoadCursor("measure.cur");
					parent.Cursor = measureCursor;
					break;
				case CursorType.SizeWE:
					parent.Cursor = System.Windows.Forms.Cursors.SizeWE;
					break;
				case CursorType.SizeNS:
					parent.Cursor = System.Windows.Forms.Cursors.SizeNS;
					break;
				case CursorType.SizeNESW:
					parent.Cursor = System.Windows.Forms.Cursors.SizeNESW;
					break;
				case CursorType.SizeNWSE:
					parent.Cursor = System.Windows.Forms.Cursors.SizeNWSE;
					break;
				default:
					parent.Cursor = System.Windows.Forms.Cursors.Arrow;
					break;
			}
			lastCursor = mouseCursor;
		}

		#region IDisposable Members

		public void Dispose()
		{
			foreach(IDisposable font in fontList.Values)
			{
				if(font!=null)
				{
					font.Dispose();
				}
			}
			fontList.Clear();

			if(measureCursor!=null)
			{
				measureCursor.Dispose();
				measureCursor = null;
			}

			if(DownloadQueue != null)
			{
				DownloadQueue.Dispose();
				DownloadQueue = null;
			}

			GC.SuppressFinalize(this);
		}

		#endregion

		private GeographicBoundingBox m_oCurrentRoI;
		public GeographicBoundingBox CurrentRoI
		{
			get { return m_oCurrentRoI; }
			set { m_oCurrentRoI = value; }
		}
	}

	/// <summary>
	/// Mouse cursor
	/// </summary>
	public enum CursorType
	{
		Arrow = 0,
		Hand,
		Cross,
		Measure,
		SizeWE,
		SizeNS,
		SizeNESW,
		SizeNWSE
	}
}
