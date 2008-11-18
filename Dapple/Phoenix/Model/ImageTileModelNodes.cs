using System;

namespace NewServerTree
{
	public class ImageTileSetRootModelNode : ModelNode
	{
		public ImageTileSetRootModelNode(DappleModel oModel)
			: base(oModel)
		{
			MarkLoaded();
		}

		public override ModelNode[] Load()
		{
			throw new ApplicationException(ErrLoadedBadNode);
		}

		public override String DisplayText
		{
			get
			{
				return "Image Tile Servers";
			}
		}

		public override string IconKey
		{
			get { return IconKeys.TileRoot; }
		}

		public ImageTileSetModelNode GetImageTileSet(String strName)
		{
			foreach (ImageTileSetModelNode oTileSet in this.Children)
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
	}

	public class ImageTileSetModelNode : ModelNode
	{
		String m_strName;

		public ImageTileSetModelNode(DappleModel oModel, String strName)
			: base(oModel)
		{
			m_strName = strName;

			MarkLoaded();
		}

		public override ModelNode[] Load()
		{
			throw new ApplicationException(ErrLoadedBadNode);
		}

		public override string DisplayText
		{
			get { return m_strName; }
		}

		public override string IconKey
		{
			get { return IconKeys.TileSet; }
		}

		public String Name
		{
			get { return m_strName; }
		}

		public void AddLayer(ImageTileLayerModelNode oNewLayer)
		{
			AddChild(oNewLayer);
		}
	}

	public class ImageTileLayerModelNode : ModelNode
	{
		private String m_strName;
		private Uri m_oUri;
		private String m_strExtension;
		private double m_dLZTS;
		private String m_strDataset;
		private int m_iLevels;

		public ImageTileLayerModelNode(DappleModel oModel, String strName, Uri oUri, String strExtension, double dLZTS, String strDataset, int iLevels)
			:base(oModel)
		{
			m_strName = strName;
			m_oUri = oUri;
			m_strExtension = strExtension;
			m_dLZTS = dLZTS;
			m_strDataset = strDataset;
			m_iLevels = iLevels;

			MarkLoaded();
		}

		public override ModelNode[] Load()
		{
			throw new ApplicationException(ErrLoadedLeafNode);
		}

		public override string DisplayText
		{
			get { return m_strName; }
		}

		public override string IconKey
		{
			get { return IconKeys.TileLayer; }
		}
	}
}