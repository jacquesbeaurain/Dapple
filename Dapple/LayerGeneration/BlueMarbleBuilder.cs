using System;
using System.Collections.Generic;
using System.Text;
using WorldWind.Renderable;
using System.IO;
using System.Windows.Forms;
using System.Globalization;

namespace Dapple.LayerGeneration
{
   /// <summary>
   /// Wraps the BMNG layer in a LayerBuilder.
   /// </summary>
   class BlueMarbleBuilder : LayerBuilder
	{
		#region Member variables

		private RenderableObject m_hObject;
		private bool m_blIsChanged = true;

		#endregion

		#region Constructors

		internal BlueMarbleBuilder()
         : base("Blue Marble", MainForm.WorldWindowSingleton, null)
      {
         ImageLayer oBaseLayer = new WorldWind.Renderable.ImageLayer(
            "Blue Marble ImageLayer",
            MainForm.WorldWindowSingleton.CurrentWorld,
            0,
            String.Format(CultureInfo.InvariantCulture, "{0}\\Data\\Earth\\BmngBathy\\world.topo.bathy.2004{1:D2}.jpg", Path.GetDirectoryName(Application.ExecutablePath), 7),
            -90, 90, -180, 180, 1.0f, null);

         WorldWind.NltImageStore imageStore = new WorldWind.NltImageStore(String.Format(CultureInfo.InvariantCulture, "bmng.topo.bathy.2004{0:D2}", 7), "http://worldwind25.arc.nasa.gov/tile/tile.aspx");
         imageStore.DataDirectory = null;
         imageStore.LevelZeroTileSizeDegrees = 36.0;
         imageStore.LevelCount = 5;
         imageStore.ImageExtension = "jpg";
         imageStore.CacheDirectory = MainForm.WorldWindowSingleton.Cache.CacheDirectory + "\\Earth\\BMNG\\";

         WorldWind.ImageStore[] ias = new WorldWind.ImageStore[1];
         ias[0] = imageStore;

         QuadTileSet oTiledBaseLayer = new WorldWind.Renderable.QuadTileSet(
                 "Blue Marble QuadTileSet",
                 MainForm.WorldWindowSingleton.CurrentWorld,
                 0,
                 90, -90, -180, 180, true, ias);

         RenderableObjectList oRenderableList = new RenderableObjectList("This name doesn't matter, it gets rewritten");
         oRenderableList.Add(oBaseLayer);
         oRenderableList.Add(oTiledBaseLayer);
         oRenderableList.RenderPriority = RenderPriority.TerrainMappedImages;

         m_hObject = oRenderableList;
      }

      internal BlueMarbleBuilder(RenderableObject hObject) :base("Blue Marble", MainForm.WorldWindowSingleton, null)
      {
         m_hObject = hObject;
		}

		#endregion

		#region Properties

		[System.ComponentModel.Category("Dapple")]
		[System.ComponentModel.Browsable(true)]
		[System.ComponentModel.Description("The opacity of the image (255 = opaque, 0 = transparent)")]
		public override byte Opacity
      {
         get
         {
            return m_hObject.Opacity;
         }
         set
         {
            CascadeOpacity(m_hObject, value);
         }
      }

		[System.ComponentModel.Category("Dapple")]
		[System.ComponentModel.Browsable(true)]
		[System.ComponentModel.Description("Whether this data layer is visible on the globe")]
		public override bool Visible
      {
         get
         {
            return m_hObject.IsOn;
         }
         set
         {
            m_hObject.IsOn = value;
         }
      }

		[System.ComponentModel.Category("Common")]
		[System.ComponentModel.Browsable(true)]
		[System.ComponentModel.Description("The extents of this data layer, in WGS 84")]
		public override WorldWind.GeographicBoundingBox Extents
		{
			get { return new WorldWind.GeographicBoundingBox(90, -90, -180, 180); }
		}

		[System.ComponentModel.Browsable(false)]
		public override bool IsChanged
      {
         get { return m_blIsChanged; }
      }

		[System.ComponentModel.Browsable(false)]
      internal override string ServerTypeIconKey
      {
         get { return "blue_marble"; }
      }

		[System.ComponentModel.Browsable(false)]
		public override string DisplayIconKey
      {
         get { return "blue_marble"; }
      }

		#endregion

		internal override bool bIsDownloading(out int iBytesRead, out int iTotalBytes)
      {
         return CascadeDownloading(m_hObject, out iBytesRead, out iTotalBytes);
      }

      internal override WorldWind.Renderable.RenderableObject GetLayer()
      {
         return m_hObject;
      }

      internal override string GetURI()
      {
         return null;
      }

      internal override string GetCachePath()
      {
         return Path.Combine(m_oWorldWindow.Cache.CacheDirectory, @"Earth\BMNG");
      }

      protected override void CleanUpLayer(bool bFinal)
      {
         if (m_hObject != null)
            m_hObject.Dispose();

         m_blIsChanged = true;
      }

		public override bool Equals(object obj)
      {
         if (!(obj is BlueMarbleBuilder)) return false;

         return true; // There can be only one
      }

		public override int GetHashCode()
		{
			return "BLUEMARBLE".GetHashCode();
		}

      internal override object CloneSpecific()
      {
         return new BlueMarbleBuilder(m_hObject);
      }

      private void CascadeOpacity(RenderableObject oRObject, byte bOpacity)
      {
         oRObject.Opacity = bOpacity;

         if (oRObject is RenderableObjectList)
         {
            foreach (RenderableObject oChildRObject in ((RenderableObjectList)oRObject).ChildObjects)
            {
               CascadeOpacity(oChildRObject, bOpacity);
            }
         }
      }

      private bool CascadeDownloading(RenderableObject oObj, out int iBytesRead, out int iTotalBytes)
      {
         if (oObj.Initialized && oObj.IsOn)
         {
            if (oObj is QuadTileSet)
            {
               return ((QuadTileSet)oObj).bIsDownloading(out iBytesRead, out iTotalBytes);
            }
            else if (oObj is RenderableObjectList)
            {
               bool result = false;
               int iResultRead = 0;
               int iResultTotal = 0;

               foreach (RenderableObject oRO in ((RenderableObjectList)oObj).ChildObjects)
               {
                  int iRead, iTotal;
                  result |= CascadeDownloading(oRO, out iRead, out iTotal);
                  iResultRead += iRead;
                  iResultTotal += iTotal;
               }

               iBytesRead = iResultRead;
               iTotalBytes = iResultTotal;
               return result;
            }
         }

         iBytesRead = 0;
         iTotalBytes = 0;
         return false;
      }

      internal override void GetOMMetadata(out String szDownloadType, out String szServerURL, out String szLayerId)
      {
         szDownloadType = "tile";
         szServerURL = "http://worldwind25.arc.nasa.gov/tile/tile.aspx";
         szLayerId = "bmng.topo.bathy.200407";
      }
   }
}
