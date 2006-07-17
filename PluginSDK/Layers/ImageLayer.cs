using System;
using System.Threading;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.IO;
using System.Diagnostics;
using WorldWind.Net;
using WorldWind.Terrain;
using WorldWind.Menu;
using WorldWind.VisualControl;

namespace WorldWind.Renderable
{
	/// <summary>
	/// Use this class to map a single image to the planet at a desired altitude.
	/// Source image must be in Plate Carree (geographic) map projection:
	/// http://en.wikipedia.org/wiki/Plate_Carr%E9e_Projection
	/// TODO: Update this code to take advantage of the Texture Manager
	/// </summary>
	public class ImageLayer : RenderableObject
	{
		#region Private Members

		protected double layerRadius;
		protected double minLat;
		protected double maxLat;
		protected double minLon;
		protected double maxLon;
		World m_ParentWorld;
		Stream m_TextureStream = null;

		protected bool _disableZbuffer;
		protected CustomVertex.PositionColoredTextured[] vertices;
		protected short[] indices;

		protected Texture texture;
		protected Device device;
		protected string _imageUrl;
		protected string _imagePath;

		// The triangular mesh density for the rendered sector
		protected int meshPointCount = 64;
		protected TerrainAccessor _terrainAccessor;

		protected float downloadPercent;
		protected Thread downloadThread;
		protected float verticalExaggeration;

		int m_TransparentColor = 0;
		#endregion

		#region Properties
		
		/// <summary>
		/// Gets or sets the color used for transparent areas.
		/// </summary>
		/// <value></value>
		public int TransparentColor
		{
			get
			{
				return m_TransparentColor;
			}
			set
			{
				m_TransparentColor = value;
			}
		}
		/// <summary>
		/// The url of the image (when image is on network)
		/// </summary>
		public string ImageUrl
		{
			get
			{
				return this._imageUrl;
			}
			set
			{
				this._imageUrl = value;
			}
		}

		/// <summary>
		/// The Path of the image (local file)
		/// </summary>
		public string ImagePath
		{
			get
			{
				return this._imagePath;
			}
			set
			{
				if(value!=null)
					value = value.Trim();
				if(value==null || value.Trim().Length<=0)
					_imagePath = null;
				else
					_imagePath = value.Trim();
			}
		}

		public bool DisableZBuffer
		{
			get
			{
				return this._disableZbuffer;
			}
			set
			{
				this._disableZbuffer = value;
			}
		}

		/// <summary>
		/// Longitude at left edge of image
		/// </summary>
		public double MinLon
		{
			get
			{
				return this.minLon;
			}
			set
			{
				this.minLon= value;
			}
		}

		/// <summary>
		/// Latitude at lower edge of image
		/// </summary>
		public double MinLat
		{
			get
			{
				return this.minLat;
			}
			set
			{
				this.minLat= value;
			}
		}

		/// <summary>
		/// Longitude at upper edge of image
		/// </summary>
		public double MaxLon
		{
			get
			{
				return this.maxLon;
			}
			set
			{
				this.maxLon= value;
			}
		}

		/// <summary>
		/// Latitude at upper edge of image
		/// </summary>
		public double MaxLat
		{
			get
			{
				return this.maxLat;
			}
			set
			{
				this.maxLat= value;
			}
		}


        public double LayerRadius
        {
            get
            {
                return this.layerRadius;
            }
        }

		#endregion

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.Renderable.ImageLayer"/> class.
		/// </summary>
		public ImageLayer( string name, float layerRadius ) : base (name)
		{
			this.layerRadius = layerRadius;
		}

		/// <summary>
		/// Constructor
		/// </summary>
		public ImageLayer(
			string name,
			World parentWorld,
			double distanceAboveSurface,
			string imagePath,
			double minLatitude,
			double maxLatitude,
			double minLongitude,
			double maxLongitude,
			byte opacity,
			TerrainAccessor terrainAccessor)
			: base(name, parentWorld.Position, parentWorld.Orientation) 
		{
			this.m_ParentWorld = parentWorld;
			this.layerRadius = (float)parentWorld.EquatorialRadius + distanceAboveSurface;
			this._imagePath = imagePath;
			minLat = minLatitude;
			maxLat = maxLatitude;
			minLon = minLongitude;
			maxLon = maxLongitude;
			this._opacity = opacity;
			this._terrainAccessor = terrainAccessor;
			this._imagePath = imagePath;
		}

		/// <summary>
		/// Layer initialization code
		/// </summary>
		public override void Initialize(DrawArgs drawArgs)
		{
			try
			{
				this.device = drawArgs.device;

				if(_imagePath == null && _imageUrl != null && _imageUrl.ToLower().StartsWith("http://"))
				{
					_imagePath = getFilePathFromUrl(_imageUrl);
					
				}
				
				if(downloadThread != null && downloadThread.IsAlive)
					return;

				if(m_TextureStream != null)
				{
					UpdateTexture(m_TextureStream, m_TransparentColor);
					verticalExaggeration = World.Settings.VerticalExaggeration;
					CreateMesh();
					isInitialized = true;
					return;
				}
				else if(File.Exists(_imagePath) )
				{
                if ((new FileInfo(_imagePath)).Length > 0)
                {
                    UpdateTexture(_imagePath);
                    verticalExaggeration = World.Settings.VerticalExaggeration;
                    CreateMesh();
                    isInitialized = true;
                    return;
                }
                else
                {
                    File.Delete(_imagePath);
                }
				}

				if(_imageUrl != null && _imageUrl.ToLower().StartsWith("http://"))
				{
					//download it...
					downloadThread = new Thread(new ThreadStart(DownloadImage));
					downloadThread.Name = "ImageLayer.DownloadImage";
					
					downloadThread.IsBackground = true;
					downloadThread.Start();

					return;
				}

				// No image available
				Dispose();
				isOn = false;
				return;
			}
			catch
			{
			}
		}

      protected override void FreeResources()
      {
      }

		/// <summary>
		/// Downloads image from web
		/// </summary>
		protected virtual void DownloadImage()
		{
			try
			{
				if(_imagePath!=null)
					Directory.CreateDirectory(Path.GetDirectoryName(this._imagePath));

				using(WebDownload downloadReq = new WebDownload(this._imageUrl))
				{
					downloadReq.ProgressCallback += new DownloadProgressHandler(UpdateDownloadProgress);
					string filePath = getFilePathFromUrl(_imageUrl);
					
					if(_imagePath==null)
					{
						// Download to RAM
						downloadReq.DownloadMemory();
						texture = ImageHelper.LoadTexture(device, downloadReq.ContentStream);
					}
					else
					{
						downloadReq.DownloadFile(_imagePath);
						UpdateTexture(_imagePath);
					}
					CreateMesh();
					isInitialized = true;
				}
			}
			catch(ThreadAbortException)
			{}
			catch(Exception caught)
			{
				string msg = string.Format("Image download of file\n\n{1}\n\nfor layer '{0}' failed:\n\n{2}",
					name, _imageUrl, caught.Message );
				System.Windows.Forms.MessageBox.Show(msg, "Image download failed.", 
					System.Windows.Forms.MessageBoxButtons.OK, 
					System.Windows.Forms.MessageBoxIcon.Error ); 
				isOn = false;
			}
		}

		/// <summary>
		/// Download progress callback 
		/// </summary>
		protected void UpdateDownloadProgress(int bytesRead, int bytesTotal)
		{
			if(bytesRead < bytesTotal)
				downloadPercent = (float)bytesRead / bytesTotal;
		}

		/// <summary>
		/// Update layer (called from worker thread)
		/// </summary>
		public override void Update(DrawArgs drawArgs)
		{
			try
			{
				if(!this.isInitialized)
				{
					this.Initialize(drawArgs);
					if(!this.isInitialized)
						return;
				}

				if(isInitialized && Math.Abs(this.verticalExaggeration - World.Settings.VerticalExaggeration)>0.01f)
				{
					// Vertical exaggeration changed - rebuild mesh
					this.verticalExaggeration = World.Settings.VerticalExaggeration;
					this.isInitialized = false;
					CreateMesh();
					this.isInitialized = true;
				}
			}
			catch
			{
			}
		}

		/// <summary>
		/// Handle mouse click
		/// </summary>
		/// <returns>true if click was handled.</returns>
		public override bool PerformSelectionAction(DrawArgs drawArgs)
		{
			return false;
		}

		public override byte Opacity
		{
			get
			{
				return _opacity;
			}
			set
			{
				_opacity = value;

				if(vertices==null)
					return;

				// Update mesh opacity
				int opacityColor = _opacity << 24;
				for(int index = 0; index < vertices.Length; index++)
					vertices[index].Color = opacityColor;
			}
		}

      /// <summary>
		/// Builds the image's mesh 
		/// </summary>
		protected virtual void CreateMesh()
		{
			int upperBound = meshPointCount - 1;
			float scaleFactor = (float)1/upperBound;
			double latrange = Math.Abs(maxLat - minLat);
			double lonrange;
			if(minLon < maxLon)
				lonrange = maxLon - minLon;
			else
				lonrange = 360.0f + maxLon - minLon;

			int opacityColor = System.Drawing.Color.FromArgb(this._opacity,0,0,0).ToArgb();
			vertices = new CustomVertex.PositionColoredTextured[meshPointCount * meshPointCount];
			for(int i = 0; i < meshPointCount; i++)
			{
				for(int j = 0; j < meshPointCount; j++)
				{	
					double height = 0;
					if(this._terrainAccessor != null)
						height = this.verticalExaggeration * this._terrainAccessor.GetElevationAt(
                     (float)(maxLat - scaleFactor * latrange * i),
                     (float)(minLon + scaleFactor * lonrange * j),
                     (float)(upperBound / latrange));

					Vector3d pos = MathEngine.SphericalToCartesian( 
						maxLat - scaleFactor*latrange*i,
						minLon + scaleFactor*lonrange*j, 
						layerRadius + height);

               vertices[i * meshPointCount + j].X = (float)pos.X;
               vertices[i * meshPointCount + j].Y = (float)pos.Y;
               vertices[i * meshPointCount + j].Z = (float)pos.Z;
					
					vertices[i*meshPointCount + j].Tu = j*scaleFactor;
					vertices[i*meshPointCount + j].Tv = i*scaleFactor;
					vertices[i*meshPointCount + j].Color = opacityColor;
				}
			}

			indices = new short[2 * upperBound * upperBound * 3];
			for(int i = 0; i < upperBound; i++)
			{
				for(int j = 0; j < upperBound; j++)
				{
					indices[(2*3*i*upperBound) + 6*j] = (short)(i*meshPointCount + j);
					indices[(2*3*i*upperBound) + 6*j + 1] = (short)((i+1)*meshPointCount + j);
					indices[(2*3*i*upperBound) + 6*j + 2] = (short)(i*meshPointCount + j+1);
	
					indices[(2*3*i*upperBound) + 6*j + 3] = (short)(i*meshPointCount + j+1);
					indices[(2*3*i*upperBound) + 6*j + 4] = (short)((i+1)*meshPointCount + j);
					indices[(2*3*i*upperBound) + 6*j + 5] = (short)((i+1)*meshPointCount + j+1);
				}
			}
		}

		/// <summary>
		/// Draws the layer
		/// </summary>
		public override void Render(DrawArgs drawArgs)
		{
			if(!this.isInitialized)
				return;

			try
			{
				if(texture == null)
					return;

				drawArgs.device.SetTexture(0, this.texture);

				if(this._disableZbuffer)
				{
					if(drawArgs.device.RenderState.ZBufferEnable)
						drawArgs.device.RenderState.ZBufferEnable = false;
				}
				else
				{
					if(!drawArgs.device.RenderState.ZBufferEnable)
						drawArgs.device.RenderState.ZBufferEnable = true;
				}

				drawArgs.device.Clear(ClearFlags.ZBuffer, 0, 1.0f, 0);

				drawArgs.device.VertexFormat = CustomVertex.PositionColoredTextured.Format;
				drawArgs.device.TextureState[0].AlphaOperation = TextureOperation.Modulate;
				drawArgs.device.TextureState[0].ColorOperation = TextureOperation.Add;
				drawArgs.device.TextureState[0].AlphaArgument1 = TextureArgument.TextureColor;
				drawArgs.device.DrawIndexedUserPrimitives(PrimitiveType.TriangleList, 0, 
					vertices.Length, indices.Length / 3, indices, true, vertices);
			}
			finally
			{
				if(this._disableZbuffer)
					drawArgs.device.RenderState.ZBufferEnable = true;
			}
		}

		private static string getFilePathFromUrl(string url)
		{
			if(url.ToLower().StartsWith("http://"))
			{
				url = url.Substring(7);
			}

			// ShockFire: Remove any illegal characters from the path
			foreach (char invalidChar in Path.GetInvalidPathChars())
			{
				url = url.Replace(invalidChar.ToString(), "");
			}

			// ShockFire: Also remove other illegal chars that are not included in InvalidPathChars for no good reason
			url = url.Replace(":", "").Replace("*", "").Replace("?", "");
          url = url.Replace(@"/", @"\" );

			return Path.Combine(
				Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath), Path.Combine("Cache\\ImageUrls", url));
		}


		/// <summary>
		/// Switch to a different image
		/// </summary>
		public void UpdateTexture(string fileName)
		{
			try
			{
				if(this.device != null)
				{
					Texture oldTexture = this.texture;

					this._imagePath = fileName;
					Texture newTexture = ImageHelper.LoadTexture(device, fileName);
					this.texture = newTexture;

					if(oldTexture != null)
						oldTexture.Dispose();
				}
			}
			catch
			{
			}
		}

		/// <summary>
		/// Switch to a different image
		/// </summary>
		public void UpdateTexture(Stream textureStream)
		{
			UpdateTexture(textureStream, 0);
		}

		/// <summary>
		/// Switch to a different image
		/// </summary>
		public void UpdateTexture(Stream textureStream, int transparentColor)
		{
			try
			{
				if(this.device != null)
				{
					Texture oldTexture = this.texture;

					Texture newTexture = ImageHelper.LoadTexture(device, textureStream);
					this.texture = newTexture;

					if(oldTexture != null)
						oldTexture.Dispose();
				}
			}
			catch
			{
			}
		}


		/// <summary>
		/// Cleanup when layer is disabled
		/// </summary>
      public override void Dispose()
		{
			this.isInitialized = false;

			if(downloadThread != null)
			{
				if(downloadThread.IsAlive)
				{
					downloadThread.Abort();
				}
				downloadThread = null;
			}

			if (this.texture!=null)
			{
				this.texture.Dispose();
				this.texture = null;
			}
		}

		/// <summary>
		/// Change opacity
		/// </summary>
		/// <param name="percent">0=transparent, 1=opaque</param>
		public void UpdateOpacity(float percent)
		{
			Debug.Assert(percent <= 1);
			Debug.Assert(percent >= 0);

			this._opacity = (byte)(255 * percent);
			
			this.isInitialized = false;
			CreateMesh();
			this.isInitialized = true;
		}

		/// <summary>
		/// Change radius
		/// </summary>
		/// <param name="layerRadius">Sphere radius (meters)</param>
		public void UpdateLayerRadius(float layerRadius)
		{
			this.layerRadius = layerRadius;

			this.isInitialized = false;
			CreateMesh();
			this.isInitialized = true;
		}
	}
}
