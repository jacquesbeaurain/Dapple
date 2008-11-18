using System;

namespace NewServerTree
{
	public class ImageTileServerRootModelNode : ModelNode
	{
		public ImageTileServerRootModelNode(DappleModel oModel)
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
	}
}