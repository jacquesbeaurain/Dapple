using System;
using WorldWind;
using System.ComponentModel;

namespace NewServerTree
{
	public class ImageTileSetRootModelNode : ModelNode
	{
		#region Constructors

		public ImageTileSetRootModelNode(DappleModel oModel)
			: base(oModel)
		{
			MarkLoaded();
		}

		#endregion


		#region Properties

		[Browsable(false)]
		public override bool ShowAllChildren
		{
			get { return UseShowAllChildren; }
		}

		public override String DisplayText
		{
			get { return "Image Tile Servers"; }
		}

		[Browsable(false)]
		public override string IconKey
		{
			get { return IconKeys.TileRoot; }
		}

		#endregion


		#region Public Methods

		public ImageTileSetModelNode GetImageTileSet(String strName)
		{
			foreach (ImageTileSetModelNode oTileSet in this.UnfilteredChildren)
			{
				if (oTileSet.Name.Equals(strName))
				{
					return oTileSet;
				}
			}

			ImageTileSetModelNode oNewSet = new ImageTileSetModelNode(m_oModel, strName);
			AddChild(oNewSet);
			return oNewSet;
		}

		public void SaveToView(dappleview.serversType oServers)
		{
			dappleview.builderentryType oImageEntry = oServers.Newbuilderentry();
			dappleview.builderdirectoryType oImageDir = oImageEntry.Newbuilderdirectory();
			oImageDir.Addname(new Altova.Types.SchemaString("Image Tile Servers"));
			oImageDir.Addspecialcontainer(new dappleview.SpecialDirectoryType("ImageServers"));

			foreach (ImageTileSetModelNode oTileSets in UnfilteredChildren)
			{
				oTileSets.SaveToView(oImageDir);
			}

			oImageEntry.Addbuilderdirectory(oImageDir);
			oServers.Addbuilderentry(oImageEntry);
		}

		#endregion


		#region Helper Methods

		protected override ModelNode[] Load()
		{
			throw new ApplicationException(ErrLoadedBadNode);
		}

		#endregion
	}


	public class ImageTileSetModelNode : ModelNode
	{
		#region Member Variables

		String m_strName;

		#endregion


		#region Constructors

		public ImageTileSetModelNode(DappleModel oModel, String strName)
			: base(oModel)
		{
			m_strName = strName;

			MarkLoaded();
		}

		#endregion


		#region Properties

		public override string DisplayText
		{
			get { return m_strName; }
		}

		[Browsable(false)]
		public override string IconKey
		{
			get { return IconKeys.TileSet; }
		}

		public String Name
		{
			get { return m_strName; }
		}

		#endregion


		#region Public Methods

		public void AddLayer(ImageTileLayerModelNode oNewLayer)
		{
			AddChild(oNewLayer);
		}

		public void SaveToView(dappleview.builderdirectoryType oDir)
		{
			dappleview.builderentryType oSetEntry = oDir.Newbuilderentry();
			dappleview.tileserversetType oSet = oSetEntry.Newtileserverset();
			oSet.Addname(new Altova.Types.SchemaString(m_strName));
			dappleview.tilelayersType oLayers = oSet.Newtilelayers();

			foreach (ImageTileLayerModelNode oTileLayer in UnfilteredChildren)
			{
				oTileLayer.SaveToView(oLayers);
			}

			oSet.Addtilelayers(oLayers);
			oSetEntry.Addtileserverset(oSet);
			oDir.Addbuilderentry(oSetEntry);
		}

		#endregion


		#region Helper Methods

		protected override ModelNode[] Load()
		{
			throw new ApplicationException(ErrLoadedBadNode);
		}

		#endregion
	}


	public class ImageTileLayerModelNode : LayerModelNode
	{
		#region Memeber Variables

		private String m_strName;
		private Uri m_oUri;
		private String m_strExtension;
		private double m_dLZTS;
		private String m_strDataset;
		private int m_iLevels;
		private GeographicBoundingBox m_oBounds;
		private int m_iDistanceAboveSurface;
		private int m_iTextureSize;

		#endregion


		#region Constructors

		public ImageTileLayerModelNode(DappleModel oModel, String strName, Uri oUri, String strExtension, double dLZTS, String strDataset, int iLevels, GeographicBoundingBox oBounds, int iDistanceAboveSurface, int iTextureSize)
			: base(oModel)
		{
			m_strName = strName;
			m_oUri = oUri;
			m_strExtension = strExtension;
			m_dLZTS = dLZTS;
			m_strDataset = strDataset;
			m_iLevels = iLevels;
			m_oBounds = oBounds;
			m_iDistanceAboveSurface = iDistanceAboveSurface;
			m_iTextureSize = iTextureSize;

			MarkLoaded();
		}

		#endregion


		#region Properties

		[Browsable(false)]
		public override bool IsLeaf
		{
			get { return true; }
		}

		public override string DisplayText
		{
			get { return m_strName; }
		}

		[Browsable(false)]
		public override string IconKey
		{
			get { return IconKeys.TileLayer; }
		}

		#endregion


		#region Public Methods

		#region Saving and Loading old Dapple Views

		public void SaveToView(dappleview.tilelayersType oSet)
		{
			dappleview.tilelayerType oData = oSet.Newtilelayer();

			oData.Addname(new Altova.Types.SchemaString(m_strName));
			oData.Addurl(new Altova.Types.SchemaString(m_oUri.ToString()));
			oData.Addimageextension(new Altova.Types.SchemaString(m_strExtension));
			oData.Addlevelzerotilesize(new Altova.Types.SchemaDouble(m_dLZTS));
			oData.Adddataset(new Altova.Types.SchemaString(m_strDataset));
			oData.Addlevels(new Altova.Types.SchemaInt(m_iLevels));

			dappleview.boundingboxType oBounds = oData.Newboundingbox();

			oBounds.Addminlon(new Altova.Types.SchemaDouble(m_oBounds.West));
			oBounds.Addmaxlon(new Altova.Types.SchemaDouble(m_oBounds.East));
			oBounds.Addminlat(new Altova.Types.SchemaDouble(m_oBounds.South));
			oBounds.Addmaxlat(new Altova.Types.SchemaDouble(m_oBounds.North));

			oData.Addboundingbox(oBounds);

			oSet.Addtilelayer(oData);
		}

		#endregion

		[Obsolete("This should get removed with the rest of the LayerBuilder/ServerTree stuff")]
		public override Dapple.LayerGeneration.LayerBuilder ConvertToLayerBuilder()
		{
			return new Dapple.LayerGeneration.NltQuadLayerBuilder(
				m_strName,
				m_iDistanceAboveSurface,
				true,
				m_oBounds,
				m_dLZTS,
				m_iLevels,
				m_iTextureSize,
				m_oUri.ToString(),
				m_strDataset,
				m_strExtension,
				Byte.MaxValue,
				Dapple.MainForm.WorldWindowSingleton,
				null);
		}

		#endregion


		#region Helper Methods

		protected override ModelNode[] Load()
		{
			throw new ApplicationException(ErrLoadedLeafNode);
		}

		#endregion
	}
}