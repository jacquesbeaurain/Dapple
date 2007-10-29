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
		string Name
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

		private event BuilderChangedHandler BuilderChanged;

      public LayerBuilder(String szTreeNodeText, WorldWindow oWorldWindow, IBuilder oParent)
      {
         m_szTreeNodeText = szTreeNodeText;
         m_Parent = oParent;
         m_oWorldWindow = oWorldWindow;
      }

		public string Name
		{
			get
			{
				return m_szTreeNodeText;
			}
		}

      private String RenderableObjectName
      {
         get
         {
            return "3 - " + m_intRenderPriority.ToString("0000000000");
         }
      }

		public abstract byte Opacity
		{
			get;
			set;
		}

		public abstract bool Visible
		{
			get;
			set;
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

      [System.ComponentModel.Browsable(true)]
      public abstract GeographicBoundingBox Extents
      {
         get;
      }

      [System.ComponentModel.Browsable(false)]
      public bool Temporary
      {
         get { return m_bTemporary; }
         set { m_bTemporary = value; }
      }

      [System.ComponentModel.Browsable(false)]
		public abstract bool bIsDownloading(out int iBytesRead, out int iTotalBytes);

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

      [System.ComponentModel.Browsable(true)]
		public virtual bool SupportsMetaData
		{
			get
			{
				return false;
			}
		}

		public virtual XmlNode GetMetaData(XmlDocument oDoc)
		{
			return null;
		}

      [System.ComponentModel.Browsable(true)]
		public virtual bool SupportsLegend
		{
			get
			{
				return false;
			}
		}

		public virtual string[] GetLegendURLs()
		{
			return null;
		}

      [System.ComponentModel.Browsable(false)]
		public virtual string StyleSheetName
		{
			get
			{
				return null;
			}
		}

		public abstract RenderableObject GetLayer();

      public bool exportToGeoTiff(String szFilename)
      {
         String szTempMetaFilename = String.Empty;
         String szTempImageFile = String.Empty;
         szFilename = Path.ChangeExtension(szFilename, ".tif");

         try
         {

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

      public abstract void GetOMMetadata(out String szDownloadType, out String szServerURL, out String szLayerId);

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
