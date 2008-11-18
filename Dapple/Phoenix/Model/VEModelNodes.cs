using System;

namespace NewServerTree
{
	public class VERootModelNode : ModelNode
	{
		public VERootModelNode(DappleModel oModel)
			: base(oModel)
		{
			AddChildSilently(new VELayerModelNode(m_oModel, VELayerModelNode.VELayerType.Map));
			AddChildSilently(new VELayerModelNode(m_oModel, VELayerModelNode.VELayerType.Hybrid));
			AddChildSilently(new VELayerModelNode(m_oModel, VELayerModelNode.VELayerType.Satelite));

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
				return "Virtual Earth";
			}
		}

		public override string IconKey
		{
			get { return IconKeys.VERoot; }
		}
	}

	public class VELayerModelNode : LayerModelNode
	{
		public enum VELayerType
		{
			Map,
			Hybrid,
			Satelite
		};

		private VELayerType m_eLayerType;

		public VELayerModelNode(DappleModel oModel, VELayerType eLayerType)
			: base(oModel)
		{
			m_eLayerType = eLayerType;

			MarkLoaded();
		}

		public override ModelNode[] Load()
		{
			throw new NotImplementedException(ErrLoadedLeafNode);
		}

		public override string DisplayText
		{
			get
			{
				switch (m_eLayerType)
				{
					case VELayerType.Map:
						return "Virtual Earth Map";
					case VELayerType.Hybrid:
						return "Virtual Earth Map & Satellite";
					case VELayerType.Satelite:
						return "Virtual Earth Satellite";
					default:
						throw new NotImplementedException("Missing enumeration case statement");
				}
			}
		}

		public override bool IsLeaf
		{
			get
			{
				return true;
			}
		}

		public override string IconKey
		{
			get { return IconKeys.VELayer; }
		}
	}
}
