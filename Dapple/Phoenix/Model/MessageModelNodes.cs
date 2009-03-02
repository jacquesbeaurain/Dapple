using System;
using System.ComponentModel;

namespace NewServerTree
{
	internal abstract class MessageModelNode : ModelNode, IAnnotationModelNode
	{
		#region Member Variables

		private String m_strMessage;

		#endregion


		#region Constructors

		internal MessageModelNode(DappleModel oModel, String strMessage)
			: base(oModel)
		{
			m_strMessage = strMessage;

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


	internal class LoadingModelNode : MessageModelNode
	{
		#region Constructors

		internal LoadingModelNode(DappleModel oModel)
			: base(oModel, "Loading...")
		{
		}

		#endregion


		#region Properties

		[Browsable(false)]
		internal override string IconKey
		{
			get { return IconKeys.LoadingMessage; }
		}

		#endregion
	}


	internal class ErrorModelNode : MessageModelNode
	{
		#region Member Variables

		private string m_strAdditionalInfo;

		#endregion


		#region Constructors

		internal ErrorModelNode(DappleModel oModel, String strMessage, String strAdditionalInfo)
			: base(oModel, strMessage)
		{
			m_strAdditionalInfo = strAdditionalInfo;
		}

		#endregion


		#region Properties

		[Browsable(false)]
		internal override string IconKey
		{
			get { return IconKeys.ErrorMessage; }
		}

		[Browsable(false)]
		internal string AdditionalInfo
		{
			get { return m_strAdditionalInfo; }
		}

		#endregion
	}


	internal class InformationModelNode : MessageModelNode
	{
		#region Constructors

		internal InformationModelNode(DappleModel oModel, String strMessage)
			:base(oModel, strMessage)
		{

		}

		#endregion


		#region Properties

		[Browsable(false)]
		internal override string IconKey
		{
			get { return IconKeys.InfoMessage; }
		}

		#endregion
	}
}