using System;

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

		public override bool ShowAllChildren
		{
			get { return UseShowAllChildren; }
		}

		public override String DisplayText
		{
			get { return "Image Tile Servers"; }
		}

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

		#endregion


		#region Constructors

		public ImageTileLayerModelNode(DappleModel oModel, String strName, Uri oUri, String strExtension, double dLZTS, String strDataset, int iLevels)
			: base(oModel)
		{
			m_strName = strName;
			m_oUri = oUri;
			m_strExtension = strExtension;
			m_dLZTS = dLZTS;
			m_strDataset = strDataset;
			m_iLevels = iLevels;

			MarkLoaded();
		}

		#endregion


		#region Properties

		public override bool IsLeaf
		{
			get { return true; }
		}

		public override string DisplayText
		{
			get { return m_strName; }
		}

		public override string IconKey
		{
			get { return IconKeys.TileLayer; }
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