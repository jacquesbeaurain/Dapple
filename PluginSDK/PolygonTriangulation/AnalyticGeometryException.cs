using System;

namespace GeometryUtility
{
	/// <summary>
	/// Summary description for NoValidReturnException.
	/// </summary>
	internal class NonValidReturnException: ApplicationException
	{
		internal NonValidReturnException():base()
		{
		
		}
		internal NonValidReturnException(string msg)
			:base(msg)
		{
			string errMsg="\nThere is no valid return value available!";
			throw new NonValidReturnException(errMsg);
		}
		internal NonValidReturnException(string msg,
			Exception inner): base(msg, inner)
		{
		
		}
	}

	internal class InvalidInputGeometryDataException: ApplicationException
	{
		internal InvalidInputGeometryDataException():base()
		{
		
		}
		internal InvalidInputGeometryDataException(string msg)
			:base(msg)
		{

		}
		internal InvalidInputGeometryDataException(string msg,
			Exception inner): base(msg, inner)
		{
		
		}
	}
}
