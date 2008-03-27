using System;

namespace Geosoft.Dap
{
	/// <summary>
	/// Represent errors that occur during encoding or decoding of GeosoftXML requests/responses.
	/// </summary>
	public class DapException : ApplicationException
	{
		/// <summary>
		/// Default constructor
		/// </summary>
		/// <param name="szMsg"></param>
		/// <param name="e"></param>
		public DapException(string szMsg, Exception e)
			: base(szMsg, e)
		{
		}

		/// <summary>
		/// Default constructor
		/// </summary>
		/// <param name="szMsg"></param>
		public DapException(string szMsg)
			: base(szMsg)
		{
		}
	}
}