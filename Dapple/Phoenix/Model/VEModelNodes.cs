using System;

namespace NewServerTree
{
	public class VERootModelNode : ModelNode
	{
		#region Constructors

		public VERootModelNode(DappleModel oModel)
			: base(oModel)
		{
			AddChildSilently(new VELayerModelNode(m_oModel, VELayerModelNode.VELayerType.Map));
			AddChildSilently(new VELayerModelNode(m_oModel, VELayerModelNode.VELayerType.Hybrid));
			AddChildSilently(new VELayerModelNode(m_oModel, VELayerModelNode.VELayerType.Satelite));

			MarkLoaded();
		}

		#endregion


		#region Properties

		public override String DisplayText
		{
			get { return "Virtual Earth"; }
		}

		public override string IconKey
		{
			get { return IconKeys.VERoot; }
		}

		#endregion


		#region Helper Methods

		protected override ModelNode[] Load()
		{
			throw new ApplicationException(ErrLoadedBadNode);
		}

		#endregion
	}


	public class VELayerModelNode : LayerModelNode
	{
		#region Enums

		public enum VELayerType
		{
			Map,
			Hybrid,
			Satelite
		};

		#endregion


		#region Member Variables

		private VELayerType m_eLayerType;

		#endregion


		#region Constructors

		public VELayerModelNode(DappleModel oModel, VELayerType eLayerType)
			: base(oModel)
		{
			m_eLayerType = eLayerType;

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

		public override string IconKey
		{
			get { return IconKeys.VELayer; }
		}

		#endregion


		#region Helper Methods

		protected override ModelNode[] Load()
		{
			throw new NotImplementedException(ErrLoadedLeafNode);
		}

		#endregion
	}
}
