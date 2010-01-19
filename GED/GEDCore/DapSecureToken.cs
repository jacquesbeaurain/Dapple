using System.Runtime.InteropServices;
using System.Text;

namespace GED.Core
{
	/// <summary>
	/// Interface to unmanaged DAP secure token generator DLL.
	/// </summary>
	public static class DapSecureToken
	{
		private static string instance;

		[DllImport("CreateSecureDAPToken.dll", CharSet = CharSet.Ansi)]
		private static extern bool CreateSecureToken(StringBuilder token, int tokenLength);

		/// <summary>
		/// Creates a DAP secure token.
		/// </summary>
		public static void Create()
		{
			var result = new StringBuilder(4096);
			CreateSecureToken(result, result.Capacity);
			instance = result.ToString();
		}

		/// <summary>
		/// Gets the DAP secure token.
		/// </summary>
		public static string Instance
		{
			get { return instance; }
		}
	}
}
