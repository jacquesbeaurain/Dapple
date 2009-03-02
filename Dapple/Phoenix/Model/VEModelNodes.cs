using System;
using System.ComponentModel;

namespace NewServerTree
{
	internal class VERootModelNode : ModelNode
	{
		#region Constructors

		internal VERootModelNode(DappleModel oModel)
			: base(oModel)
		{
			AddChildSilently(new VELayerModelNode(m_oModel, VELayerModelNode.VELayerType.Map));
			AddChildSilently(new VELayerModelNode(m_oModel, VELayerModelNode.VELayerType.Hybrid));
			AddChildSilently(new VELayerModelNode(m_oModel, VELayerModelNode.VELayerType.Satelite));

			MarkLoaded();
		}

		#endregion


		#region Properties

		internal override String DisplayText
		{
			get { return "Virtual Earth"; }
		}

		[Browsable(false)]
		internal override string IconKey
		{
			get { return IconKeys.VERoot; }
		}

		#endregion


		#region Public Methods

		#region Saving and Loading old Dapple Views

		internal void SaveToView(dappleview.serversType oServers)
		{
			dappleview.builderentryType oVEEntry = oServers.Newbuilderentry();
			dappleview.virtualearthType oVEType = oVEEntry.Newvirtualearth();

			oVEType.Addname(new Altova.Types.SchemaString("Virtual Earth"));

			oVEEntry.Addvirtualearth(oVEType);
			oServers.Addbuilderentry(oVEEntry);
		}

		#endregion

		#endregion


		#region Helper Methods

		protected override ModelNode[] Load()
		{
			throw new ApplicationException(ErrLoadedBadNode);
		}

		#endregion
	}


	internal class VELayerModelNode : LayerModelNode
	{
		#region Enums

		internal enum VELayerType
		{
			Map,
			Hybrid,
			Satelite
		};

		[Obsolete("This should get removed with the rest of the LayerBuilder/ServerTree stuff")]
		private static WorldWind.VirtualEarthMapType Convert(VELayerType eType)
		{
			switch (eType)
			{
				case VELayerType.Hybrid:
					return WorldWind.VirtualEarthMapType.hybrid;
				case VELayerType.Map:
					return WorldWind.VirtualEarthMapType.road;
				case VELayerType.Satelite:
					return WorldWind.VirtualEarthMapType.aerial;
				default:
					throw new ApplicationException("Missing enum case statement");
			}
		}

		#endregion


		#region Member Variables

		private VELayerType m_eLayerType;

		#endregion


		#region Constructors

		internal VELayerModelNode(DappleModel oModel, VELayerType eLayerType)
			: base(oModel)
		{
			m_eLayerType = eLayerType;

			MarkLoaded();
		}

		#endregion


		#region Properties

		[Browsable(false)]
		internal override bool IsLeaf
		{
			get { return true; }
		}

		internal override string DisplayText
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
						throw new ApplicationException("Missing enumeration case statement");
				}
			}
		}

		[Browsable(false)]
		internal override string IconKey
		{
			get { return IconKeys.VELayer; }
		}

		#endregion


		#region Public Methods

		[Obsolete("This should get removed with the rest of the LayerBuilder/ServerTree stuff")]
		internal override Dapple.LayerGeneration.LayerBuilder ConvertToLayerBuilder()
		{
			return new Dapple.LayerGeneration.VEQuadLayerBuilder(DisplayText, Convert(m_eLayerType), Dapple.MainForm.WorldWindowSingleton, true, null);
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
