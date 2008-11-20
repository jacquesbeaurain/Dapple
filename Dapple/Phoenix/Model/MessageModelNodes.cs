using System;

namespace NewServerTree
{
	public abstract class MessageModelNode : ModelNode, IAnnotationModelNode
	{
		#region Member Variables

		private String m_strMessage;

		#endregion


		#region Constructors

		public MessageModelNode(DappleModel oModel, String strMessage)
			: base(oModel)
		{
			m_strMessage = strMessage;

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
			get { return m_strMessage; }
		}

		#endregion


		#region Helper Methods

		protected override ModelNode[] Load()
		{
			throw new ApplicationException(ErrLoadedLeafNode);
		}

		#endregion
	}


	public class LoadingModelNode : MessageModelNode
	{
		#region Constructors

		public LoadingModelNode(DappleModel oModel)
			: base(oModel, "Loading...")
		{
		}

		#endregion


		#region Properties

		public override string IconKey
		{
			get { return IconKeys.LoadingMessage; }
		}

		#endregion
	}


	public class ErrorModelNode : MessageModelNode
	{
		#region Constructors

		public ErrorModelNode(DappleModel oModel, String strMessage)
			: base(oModel, strMessage)
		{
		}

		#endregion


		#region Properties

		public override string IconKey
		{
			get { return IconKeys.ErrorMessage; }
		}

		#endregion
	}


	public class InformationModelNode : MessageModelNode
	{
		#region Constructors

		public InformationModelNode(DappleModel oModel, String strMessage)
			:base(oModel, strMessage)
		{

		}

		#endregion


		#region Properties

		public override string IconKey
		{
			get { return IconKeys.InfoMessage; }
		}

		#endregion
	}
}