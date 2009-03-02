using System;
using System.Collections.Generic;
using System.Text;
using Dapple.LayerGeneration;
using WorldWind.Renderable;
using System.IO;
using WorldWind;
using System.Xml;

namespace Dapple.KML
{
	class KMLLayerBuilder : LayerBuilder
	{
		#region Statics

		private static String URLProtocolName = "gxkml:///";

		#endregion

		private String m_strInitFilename;
		private KMLFile m_oSourceFile;
		private RenderableObject m_oRenderable;
		private GeographicBoundingBox m_oBounds;

		internal KMLLayerBuilder(String strFilename, WorldWindow oWorldWindow, IBuilder oParent)
			:this(strFilename, Path.GetFileNameWithoutExtension(strFilename), oWorldWindow, oParent)
		{
		}

		internal KMLLayerBuilder(String strFilename, String strLayerName, WorldWindow oWorldWindow, IBuilder oParent)
			: base(strLayerName, oWorldWindow, oParent)
		{
			m_strInitFilename = strFilename;
			if (File.Exists(m_strInitFilename))
			{
				m_oSourceFile = new KMLFile(strFilename);
				m_oRenderable = KMLCreation.CreateKMLLayer(m_oSourceFile, oWorldWindow.CurrentWorld, out m_oBounds);
				m_oRenderable.RenderPriority = RenderPriority.TerrainMappedImages;
			}
		}

		private KMLLayerBuilder(KMLLayerBuilder oCopySource)
			:base (oCopySource.Title, MainForm.WorldWindowSingleton, oCopySource.Parent)
		{
			m_oSourceFile = oCopySource.m_oSourceFile;
			m_oRenderable = KMLCreation.CreateKMLLayer(m_oSourceFile, MainForm.WorldWindowSingleton.CurrentWorld, out m_oBounds);
			m_oRenderable.RenderPriority = RenderPriority.TerrainMappedImages;
		}
		
		[System.ComponentModel.Category("Dapple")]
		[System.ComponentModel.Browsable(true)]
		[System.ComponentModel.Description("The opacity of the image (255 = opaque, 0 = transparent)")]
		internal override byte Opacity
		{
			get
			{
				if (m_oRenderable != null) return m_oRenderable.Opacity;
				else return 255;
			}
			set
			{
				if (m_oRenderable != null) m_oRenderable.Opacity = value;
			}
		}

		[System.ComponentModel.Category("Dapple")]
		[System.ComponentModel.Browsable(true)]
		[System.ComponentModel.Description("Whether this data layer is visible on the globe")]
		internal override bool Visible
		{
			get
			{
				if (m_oRenderable != null) return m_oRenderable.IsOn;
				else return false;
			}
			set
			{
				if (m_oRenderable != null) m_oRenderable.IsOn = value;
			}
		}

		[System.ComponentModel.Category("Common")]
		[System.ComponentModel.Browsable(true)]
		[System.ComponentModel.Description("The extents of this data layer, in WGS 84")]
		internal override WorldWind.GeographicBoundingBox Extents
		{
			get
			{
				return m_oBounds;
			}
		}

		[System.ComponentModel.Browsable(false)]
		public override bool IsChanged
		{
			get { return false; }
		}

		[System.ComponentModel.Browsable(false)]
		internal override string ServerTypeIconKey
		{
			get { return "kml"; }
		}

		[System.ComponentModel.Browsable(false)]
		public override string DisplayIconKey
		{
			get { return "kml"; }
		}

		internal override bool bIsDownloading(out int iBytesRead, out int iTotalBytes)
		{
			iBytesRead = 0;
			iTotalBytes = 0;
			return false;
		}

		internal override WorldWind.Renderable.RenderableObject GetLayer()
		{
			return m_oRenderable;
		}

		internal override string GetURI()
		{
			return URLProtocolName + m_strInitFilename.Replace(Path.DirectorySeparatorChar, '/');
		}

		internal override string GetCachePath()
		{
			// --- KML layers don't cache anything ---
			return String.Empty;
		}

		protected override void CleanUpLayer(bool bFinal)
		{
			if (File.Exists(m_strInitFilename) && Temporary)
			{
				try
				{
					File.Delete(m_strInitFilename);
				}
				catch
				{
					// --- Ignore temp file delete errors ---
				}
			}
		}

		public override bool Equals(object obj)
		{
			if (!(obj is KMLLayerBuilder)) return false;
			KMLLayerBuilder oCastObj = obj as KMLLayerBuilder;

			return this.m_strInitFilename.Equals(oCastObj.m_strInitFilename);
		}

		public override int GetHashCode()
		{
			return m_oSourceFile.Filename.GetHashCode();
		}

		// Never called.
		internal override void GetOMMetadata(out string szDownloadType, out string szServerURL, out string szLayerId)
		{
			throw new Exception("KMLLayerBuilder.GetOMMetadata should never get called.");
		}

		internal override object CloneSpecific()
		{
			return new KMLLayerBuilder(this);
		}

		internal void GoToLookAt(WorldWindow oTarget)
		{
			KMLLookAt oView = m_oSourceFile.Document.View as KMLLookAt;
			if (oView != null)
			{
				oTarget.GotoLatLonHeadingViewRange(oView.Latitude, oView.Longitude, oView.Heading, oView.Range);
			}
			else
			{
				oTarget.GotoBoundingbox(m_oBounds, false);
			}
		}
	}
}
