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

		private KMLFile m_oSourceFile;
		private RenderableObject m_oRenderable;
		private GeographicBoundingBox m_oBounds;

		public KMLLayerBuilder(String strFilename, WorldWindow oWorldWindow, IBuilder oParent)
			: base(Path.GetFileNameWithoutExtension(strFilename), oWorldWindow, oParent)
		{
			m_oSourceFile = new KMLFile(strFilename);
			m_oRenderable = KMLCreation.CreateKMLLayer(m_oSourceFile, oWorldWindow.CurrentWorld, out m_oBounds);
			m_oRenderable.RenderPriority = RenderPriority.TerrainMappedImages;
		}

		private KMLLayerBuilder(KMLLayerBuilder oCopySource)
			:base (oCopySource.Title, MainForm.WorldWindowSingleton, oCopySource.Parent)
		{
			m_oSourceFile = oCopySource.m_oSourceFile;
			m_oRenderable = KMLCreation.CreateKMLLayer(m_oSourceFile, MainForm.WorldWindowSingleton.CurrentWorld, out m_oBounds);
			m_oRenderable.RenderPriority = RenderPriority.TerrainMappedImages;
		}

		public override byte Opacity
		{
			get
			{
				return m_oRenderable.Opacity;
			}
			set
			{
				m_oRenderable.Opacity = value;
			}
		}

		public override bool Visible
		{
			get
			{
				return m_oRenderable.IsOn;
			}
			set
			{
				m_oRenderable.IsOn = value;
			}
		}

		public override WorldWind.GeographicBoundingBox Extents
		{
			get
			{
				return m_oBounds;
			}
		}

		public override bool IsChanged
		{
			get { return false; }
		}

		public override string ServerTypeIconKey
		{
			get { return "georef_image"; }
		}

		public override string DisplayIconKey
		{
			get { return "georef_image"; }
		}

		public override bool bIsDownloading(out int iBytesRead, out int iTotalBytes)
		{
			iBytesRead = 0;
			iTotalBytes = 0;
			return false;
		}

		public override WorldWind.Renderable.RenderableObject GetLayer()
		{
			return m_oRenderable;
		}

		public override string GetURI()
		{
			return URLProtocolName + m_oSourceFile.Filename.Replace(Path.DirectorySeparatorChar, '/');
		}

		public override string GetCachePath()
		{
			throw new Exception("Unimplemented GetCachePath.");
		}

		protected override void CleanUpLayer(bool bFinal)
		{
			//TODO Clean up the layer somehow?
		}

		public override bool Equals(object obj)
		{
			if (!(obj is KMLLayerBuilder)) return false;
			KMLLayerBuilder oCastObj = obj as KMLLayerBuilder;

			return this.m_oSourceFile.Filename.Equals(oCastObj.m_oSourceFile.Filename);
		}

		public override int GetHashCode()
		{
			return m_oSourceFile.Filename.GetHashCode();
		}

		// Never called.
		public override void GetOMMetadata(out string szDownloadType, out string szServerURL, out string szLayerId)
		{
			throw new Exception("KMLLayerBuilder.GetOMMetadata should never get called.");
		}

		public override object CloneSpecific()
		{
			return new KMLLayerBuilder(this);
		}
	}
}
