using System;
using System.IO;

namespace GED.Core
{
	/// <summary>
	/// Class for managing how DappleEarth stores files to the hard drive.
	/// </summary>
	public static class CacheUtils
	{
		private static readonly string s_strDefaultCacheRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GEDCache");
		private static string s_strCacheRoot = s_strDefaultCacheRoot;

		/// <summary>
		/// The root directory into which all cached files are saved. DappleEarth should NEVER save or load
		/// files from anywhere except in some subdirectory of this directory.
		/// </summary>
		public static String CacheRoot
		{
			get { return s_strCacheRoot; }
			set { s_strCacheRoot = value; }
		}

		/// <summary>
		/// Checks if the current cache root is the default cache root.
		/// </summary>
		public static bool IsCacheRootDefault
		{
			get { return s_strDefaultCacheRoot.Equals(s_strCacheRoot); }
		}

		/// <summary>
		/// Remove invalid filename characters from a given filename.
		/// </summary>
		/// <param name="strFilename">The file name to fix.</param>
		/// <returns>The fixed file name.</returns>
		public static String CreateValidFilename(String strFilename)
		{
			foreach (Char c in Path.GetInvalidFileNameChars())
			{
				strFilename = strFilename.Replace(c, '_');
			}

			return strFilename;
		}
	}
}
