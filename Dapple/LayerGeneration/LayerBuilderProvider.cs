using System;
using System.Windows.Forms;
using System.Threading;
using System.Xml;
using System.Collections.Generic;
using System.Text;
using WorldWind;
using WorldWind.Renderable;
using WorldWind.PluginEngine;
using System.IO;
using System.Diagnostics;

namespace Dapple.LayerGeneration
{
	public interface IBuilder : ICloneable
	{
		string Title
		{
			get;
		}

		[System.ComponentModel.Browsable(false)]
		bool IsChanged
		{
			get;
		}

      [System.ComponentModel.Browsable(false)]
		IBuilder Parent
		{
			get;
		}

		bool SupportsMetaData
		{
			get;
		}

		[System.ComponentModel.Browsable(false)]
		string StyleSheetName
		{
			get;
		}

      [System.ComponentModel.Browsable(false)]
      string DisplayIconKey
      {
         get;
      }

		XmlNode GetMetaData(XmlDocument oDoc);

		void SubscribeToBuilderChangedEvent(BuilderChangedHandler handler);
		void UnsubscribeToBuilderChangedEvent(BuilderChangedHandler handler);

      TreeNode[] getChildTreeNodes();
	}

	public delegate void LayerLoadedCallback(IBuilder builder);
	public delegate void BuilderChangedHandler(LayerBuilder sender, BuilderChangeType changeType);

	public enum BuilderChangeType
	{
		LoadedSync,
		LoadedASync,
		LoadedSyncFailed,
		LoadedASyncFailed,
		LoadFailed,
		Removed,
		OpacityChanged,
		VisibilityChanged
	}


	public abstract class LayerBuilder : IBuilder
	{
		#region Member Variables

		protected string m_szTreeNodeText;
		protected IBuilder m_Parent;
		protected WorldWindow m_oWorldWindow;

		protected byte m_bOpacity = 255;
		private bool m_blnIsLoading = false;
		protected bool m_IsOn = true;
      protected static String m_strCacheRoot = MainApplication.Settings.CachePath;
		private int m_intRenderPriority = 0;
		protected bool m_blnFailed = false;
		private bool m_blnIsAdded = false;
		private bool m_bAsyncLoaded = false;
		private object m_lockObject = new object();
      private bool m_bTemporary = false;

		private static int intObjectsRendered = 0;

		#endregion

		#region Events

		private event BuilderChangedHandler BuilderChanged;

		#endregion

		#region Constructor

		public LayerBuilder(String szTreeNodeText, WorldWindow oWorldWindow, IBuilder oParent)
      {
         m_szTreeNodeText = szTreeNodeText;
         m_Parent = oParent;
         m_oWorldWindow = oWorldWindow;
		}

		#endregion

		#region Properties

		[System.ComponentModel.Category("Dapple")]
		[System.ComponentModel.Browsable(true)]
		[System.ComponentModel.Description("Whether Dapple can display metadata for this data layer")]
		public virtual bool SupportsMetaData
		{
			get
			{
				return false;
			}
		}

		[System.ComponentModel.Category("Dapple")]
		[System.ComponentModel.Browsable(true)]
		[System.ComponentModel.Description("Whether Dapple can display legend(s) for this data layer")]
		public virtual bool SupportsLegend
		{
			get
			{
				return false;
			}
		}

		[System.ComponentModel.Category("Dapple")]
		[System.ComponentModel.Browsable(true)]
		[System.ComponentModel.Description("The opacity of the image (255 = opaque, 0 = transparent)")]
		public abstract byte Opacity
		{
			get;
			set;
		}

		[System.ComponentModel.Category("Dapple")]
		[System.ComponentModel.Browsable(true)]
		[System.ComponentModel.Description("Whether this data layer is visible on the globe")]
		public abstract bool Visible
		{
			get;
			set;
		}

		[System.ComponentModel.Category("Common")]
		[System.ComponentModel.Browsable(true)]
		[System.ComponentModel.Description("The title of this data layer")]
		public string Title
		{
			get
			{
				return m_szTreeNodeText;
			}
		}

		[System.ComponentModel.Category("Common")]
		[System.ComponentModel.Browsable(true)]
		[System.ComponentModel.Description("The extents of this data layer, in WGS 84")]
		public abstract GeographicBoundingBox Extents
		{
			get;
		}

		[System.ComponentModel.Category("Common")]
		[System.ComponentModel.Browsable(false)] // Don't make this browsable until views have been reinstated.
		[System.ComponentModel.Description("Indicates that this data layer will not be saved to views")]
		public bool Temporary
		{
			get { return m_bTemporary; }
			set { m_bTemporary = value; }
		}

		[System.ComponentModel.Browsable(false)]
		private String RenderableObjectName
		{
			get
			{
				return "3 - " + m_intRenderPriority.ToString("0000000000");
			}
		}

		[System.ComponentModel.Browsable(false)]
		public abstract bool IsChanged
		{
			get;
		}

		[System.ComponentModel.Browsable(false)]
		public abstract string ServerTypeIconKey
		{
			get;
		}

		[System.ComponentModel.Browsable(false)]
		public abstract string DisplayIconKey
		{
			get;
		}

		[System.ComponentModel.Browsable(false)]
		public IBuilder Parent
		{
			get
			{
				return m_Parent;
			}
		}

		[System.ComponentModel.Browsable(false)]
		public bool Failed
		{
			get
			{
				return m_blnFailed;
			}
		}

		[System.ComponentModel.Browsable(false)]
		public bool IsAdded
		{
			get
			{
				return m_blnIsAdded;
			}
		}

		[System.ComponentModel.Browsable(false)]
		public virtual string StyleSheetName
		{
			get
			{
				return null;
			}
		}

		/// <summary>
		/// Whether the layer's server was loaded on-demand, 
		/// </summary>
		[System.ComponentModel.Browsable(false)]
		public virtual bool ServerIsInHomeView
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// Whether the layer is from a server type that can apper in the server tree.
		/// </summary>
		[System.ComponentModel.Browsable(false)]
		public virtual bool LayerFromSupportedServer
		{
			get
			{
				return false;
			}
		}

		#endregion

		#region Public Methods

		public abstract bool bIsDownloading(out int iBytesRead, out int iTotalBytes);

		public virtual XmlNode GetMetaData(XmlDocument oDoc)
		{
			return null;
		}

		public virtual string[] GetLegendURLs()
		{
			return null;
		}

		public abstract RenderableObject GetLayer();

		public bool exportToGeoTiff(String szFilename)
		{
			if (!GeographicBoundingBox.FromQuad(MainForm.WorldWindowSingleton.GetSearchBox()).Intersects(this.Extents)) return false;
			String szTempMetaFilename = String.Empty;
			String szTempImageFile = String.Empty;
			szFilename = Path.ChangeExtension(szFilename, ".tif");


			RenderableObject oRObj = GetLayer();
			RenderableObject.ExportInfo oExportInfo = new RenderableObject.ExportInfo();
			oRObj.InitExportInfo(MainForm.WorldWindowSingleton.DrawArgs, oExportInfo);

			if (!(oExportInfo.iPixelsX > 0 && oExportInfo.iPixelsY > 0)) return false;

			// Stop the camera
			//camera.SetPosition(camera.Latitude.Degrees, camera.Longitude.Degrees, camera.Heading.Degrees, camera.Altitude, camera.Tilt.Degrees);

			// Minimize the estimated extents to what is available
			GeographicBoundingBox oViewedArea = GeographicBoundingBox.FromQuad(MainForm.WorldWindowSingleton.GetSearchBox());
			oViewedArea.East = Math.Min(oViewedArea.East, oExportInfo.dMaxLon);
			oViewedArea.North = Math.Min(oViewedArea.North, oExportInfo.dMaxLat);
			oViewedArea.West = Math.Max(oViewedArea.West, oExportInfo.dMinLon);
			oViewedArea.South = Math.Max(oViewedArea.South, oExportInfo.dMinLat);

			//Calculate the image dimensions
			int iImageWidth = (int)(oExportInfo.iPixelsX * (oViewedArea.East - oViewedArea.West) / (oExportInfo.dMaxLon - oExportInfo.dMinLon));
			int iImageHeight = (int)(oExportInfo.iPixelsY * (oViewedArea.North - oViewedArea.South) / (oExportInfo.dMaxLat - oExportInfo.dMinLat));

			if (iImageWidth < 0 || iImageHeight < 0) return false;

			try
			{
				// Export image
				using (System.Drawing.Bitmap oExportedImage = new System.Drawing.Bitmap(iImageWidth, iImageHeight))
				{
					using (System.Drawing.Graphics oEIGraphics = System.Drawing.Graphics.FromImage(oExportedImage))
					{
						oExportInfo.dMaxLat = oViewedArea.North;
						oExportInfo.dMaxLon = oViewedArea.East;
						oExportInfo.dMinLat = oViewedArea.South;
						oExportInfo.dMinLon = oViewedArea.West;
						oExportInfo.gr = oEIGraphics;
						oExportInfo.iPixelsX = iImageWidth;
						oExportInfo.iPixelsY = iImageHeight;

						if (MainForm.Client == Dapple.Extract.Options.Client.ClientType.ArcMAP)
						{
							oEIGraphics.FillRectangle(new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(254, 254, 254)), new System.Drawing.Rectangle(0, 0, iImageWidth, iImageHeight));
						}

						oRObj.ExportProcess(MainForm.WorldWindowSingleton.DrawArgs, oExportInfo);
					}

					szTempMetaFilename = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
					using (StreamWriter sw = new StreamWriter(szTempMetaFilename, false))
					{
						sw.WriteLine("Geotiff_Information:");
						sw.WriteLine("Version: 1");
						sw.WriteLine("Key_Revision: 1.0");
						sw.WriteLine("Tagged_Information:");
						sw.WriteLine("ModelTiepointTag (2,3):");
						sw.WriteLine("0 0 0");
						sw.WriteLine(oViewedArea.West.ToString() + " " + oViewedArea.North.ToString() + " 0");
						sw.WriteLine("ModelPixelScaleTag (1,3):");
						sw.WriteLine(((oViewedArea.East - oViewedArea.West) / (double)iImageWidth).ToString() + " " + ((oViewedArea.North - oViewedArea.South) / (double)iImageHeight).ToString() + " 0");
						sw.WriteLine("End_Of_Tags.");
						sw.WriteLine("Keyed_Information:");
						sw.WriteLine("GTModelTypeGeoKey (Short,1): ModelTypeGeographic");
						sw.WriteLine("GTRasterTypeGeoKey (Short,1): RasterPixelIsArea");
						sw.WriteLine("GeogAngularUnitsGeoKey (Short,1): Angular_Degree");
						sw.WriteLine("GeographicTypeGeoKey (Short,1): GCS_WGS_84");
						sw.WriteLine("End_Of_Keys.");
						sw.WriteLine("End_Of_Geotiff.");
					}

					szTempImageFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
					oExportedImage.Save(szTempImageFile, System.Drawing.Imaging.ImageFormat.Tiff);

					ProcessStartInfo psi = new ProcessStartInfo(Path.GetDirectoryName(Application.ExecutablePath) + @"\System\geotifcp.exe");
					psi.UseShellExecute = false;
					psi.CreateNoWindow = true;
					psi.Arguments = "-g \"" + szTempMetaFilename + "\" \"" + szTempImageFile + "\" \"" + szFilename + "\"";

					using (Process p = Process.Start(psi))
						p.WaitForExit();

					if (File.Exists(szFilename + ".gi")) File.Delete(szFilename + ".gi");
				}
			}
			catch (ArgumentException ex)
			{
				throw new ArgumentException(
					String.Format("An error occurred extracting data layer [{0}]: Viewed Area={1}, Extract area={2}, Extract image size={3}x{4}",
					this.ToString(),
					oViewedArea.ToString(),
					GeographicBoundingBox.FromQuad(MainForm.WorldWindowSingleton.GetSearchBox()),
					iImageWidth,
					iImageHeight),
					ex);
			}
			finally
			{
				if (File.Exists(szTempMetaFilename)) File.Delete(szTempMetaFilename);
				if (File.Exists(szTempImageFile)) File.Delete(szTempImageFile);
			}

			return true;
		}

		public abstract string GetURI();

		public abstract string GetCachePath();

		/// <summary>
		/// Asynchronous addition of layer
		/// </summary>
		public void AsyncAddLayer()
		{
			m_blnFailed = false;
			if (!m_blnIsLoading)
			{
				lock (m_lockObject)
				{
					if (m_intRenderPriority == 0)
					{
						intObjectsRendered++;
						m_intRenderPriority = intObjectsRendered;
					}

					m_blnIsLoading = true;
					m_bAsyncLoaded = true;
					System.Threading.ThreadPool.QueueUserWorkItem(LoadLayer);
				}
			}
		}

		/// <summary>
		/// Synchronous addition of layer
		/// </summary>
		public void SyncAddLayer(bool forceload)
		{
			if (forceload || !m_blnIsLoading)
			{
				lock (m_lockObject)
				{
					if (m_intRenderPriority == 0)
					{
						intObjectsRendered++;
						m_intRenderPriority = intObjectsRendered;
					}

					m_bAsyncLoaded = false;
					m_blnIsLoading = true;
					LoadLayer(null);
				}
			}
		}

		public void RefreshLayer()
		{
			lock (m_lockObject)
			{
				RenderableObject oRO = m_oWorldWindow.CurrentWorld.RenderableObjects.GetObject(this.RenderableObjectName);
				m_blnIsLoading = false;
				if (oRO != null)
				{
               m_oWorldWindow.CurrentWorld.RenderableObjects.Remove(oRO);
					m_blnIsLoading = false;
				}
				CleanUpLayer(false);
				m_blnIsAdded = false;
			}
			AsyncAddLayer();
		}

		public void PushBackInRenderOrder()
		{
			lock (m_lockObject)
			{
            RenderableObject oRO = m_oWorldWindow.CurrentWorld.RenderableObjects.GetObject(this.RenderableObjectName);
				if (oRO != null)
				{
					intObjectsRendered++;
					m_intRenderPriority = intObjectsRendered;
               oRO.Name = this.RenderableObjectName;
               m_oWorldWindow.CurrentWorld.RenderableObjects.SortChildren();
				}
			}
		}

		public bool RemoveLayer()
		{
			bool bRemoved = false;
			bool bReturn = false;
			lock (m_lockObject)
			{
            RenderableObject oRO = m_oWorldWindow.CurrentWorld.RenderableObjects.GetObject(this.RenderableObjectName);
				m_blnIsLoading = false;
				if (oRO != null)
				{
               m_oWorldWindow.CurrentWorld.RenderableObjects.Remove(oRO);
					m_blnIsLoading = false;
					bRemoved = true;
					bReturn = true;
				}
				CleanUpLayer(true);
				m_blnIsAdded = false;
			}
			if (bRemoved)
				SendBuilderChanged(BuilderChangeType.Removed);
			return bReturn;
		}

		public void SubscribeToBuilderChangedEvent(BuilderChangedHandler handler)
		{
			BuilderChanged += handler;
		}

		public void UnsubscribeToBuilderChangedEvent(BuilderChangedHandler handler)
		{
			BuilderChanged -= handler;
		}

		protected void SendBuilderChanged(BuilderChangeType type)
		{
			if (BuilderChanged != null)
			{
				BuilderChanged.Invoke(this, type);
			}
		}

		protected abstract void CleanUpLayer(bool bFinal);

      public abstract override bool Equals(object obj);

		public abstract override int GetHashCode();

      public abstract void GetOMMetadata(out String szDownloadType, out String szServerURL, out String szLayerId);

		/// <summary>
		/// Add this server's URL to the home view.
		/// </summary>
		public virtual void AddServerToHomeView(MainForm oMainForm)
		{
		}

		#endregion

		private void LoadLayer(object stateInfo)
		{
#if !DEBUG
         try
         {
#endif
			lock (m_lockObject)
			{
				RenderableObject layer = null;
				try
				{
					layer = GetLayer();
				}
				catch
				{
					m_blnIsLoading = false;
					m_blnFailed = true;
				}
				if (!m_blnFailed)
				{
					layer.Name = this.RenderableObjectName;
					m_oWorldWindow.CurrentWorld.RenderableObjects.Remove(this.RenderableObjectName);
					if (m_blnIsLoading)
					{
						m_oWorldWindow.CurrentWorld.RenderableObjects.Add(layer);
						m_blnIsAdded = true;
					}
					m_blnIsLoading = false;
				}
			}
			if (!m_blnFailed)
			{
				if (m_bAsyncLoaded)
					SendBuilderChanged(BuilderChangeType.LoadedASync);
				else
					SendBuilderChanged(BuilderChangeType.LoadedSync);
			}
			else
			{
				if (m_bAsyncLoaded)
					SendBuilderChanged(BuilderChangeType.LoadedASyncFailed);
				else
					SendBuilderChanged(BuilderChangeType.LoadedSyncFailed);
			}
#if !DEBUG
         }
         catch (Exception e)
         {
            Utility.AbortUtility.Abort(e, Thread.CurrentThread);
         }
#endif
		}

		#region ICloneable Members

      public object Clone()
      {
         LayerBuilder result = CloneSpecific() as LayerBuilder;
         result.m_bOpacity = this.m_bOpacity;
         result.m_IsOn = this.m_IsOn;
         result.m_bTemporary = this.m_bTemporary;

         return result;
      }

		public abstract object CloneSpecific();

		#endregion

      #region IBuilder Members

      public TreeNode[] getChildTreeNodes()
      {
         return new TreeNode[0];
      }

      #endregion

      internal void Reset()
      {
         m_bOpacity = Byte.MaxValue;
         m_IsOn = true;
      }
   }
}
