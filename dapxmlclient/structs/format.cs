using System;

namespace Geosoft.Dap.Common
{
	/// <summary>
	/// Define a format to get an image or extract data
	/// </summary>
	public class Format
	{
		#region Member Variables
		/// <summary>
		/// The format to extract data or get image in
		/// </summary>
		protected string szType;

		/// <summary>
		/// The background colour to use when getting an image
		/// </summary>
		protected string szBackground;

		/// <summary>
		/// Transparency setting when drawing images
		/// </summary>
		protected bool bTransparent;
		#endregion

		#region Properties
		/// <summary>
		/// Get or set the extraction or image type
		/// </summary>
		/// <remarks>Valid for Image and Extraction requests</remarks>
		public string Type
		{
			set { szType = value; }
			get { return szType; }
		}

		/// <summary>
		/// Get or set the background colour for an image
		/// </summary>
		/// <remarks>Only valid for Image requests</remarks>
		public string Background
		{
			set { szBackground = value; }
			get { return szBackground; }
		}

		/// <summary>
		/// Enable or disable tranparency for an image
		/// </summary>
		/// <remarks>Only valid for Image requests</remarks>
		public bool Transparent
		{
			set { bTransparent = value; }
			get { return bTransparent; }
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Default constructor
		/// </summary>
		public Format()
		{
			szType = null;
			szBackground = null;
			bTransparent = false;
		}
		#endregion
	}
}