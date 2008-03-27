using System;

namespace Geosoft.Dap
{
	/// <summary>
	/// Arguments for when the area of interest changes
	/// </summary>
	public class AOISelectArgs : EventArgs
	{
		#region Member Variables
		/// <summary>
		/// Max X
		/// </summary>
		protected Double m_dMaxX;

		/// <summary>
		/// Max Y
		/// </summary>
		protected Double m_dMaxY;

		/// <summary>
		/// Min X
		/// </summary>
		protected Double m_dMinX;

		/// <summary>
		/// Min Y
		/// </summary>
		protected Double m_dMinY;

		/// <summary>
		/// List of keywords
		/// </summary>
		protected string m_szKeywords;
		#endregion

		#region Properties
		/// <summary>
		/// Get/Set maximum x
		/// </summary>
		public Double MaxX
		{
			get { return m_dMaxX; }
			set { m_dMaxX = value; }
		}

		/// <summary>
		/// Get/Set maximum y
		/// </summary>
		public Double MaxY
		{
			get { return m_dMaxY; }
			set { m_dMaxY = value; }
		}

		/// <summary>
		/// Get/Set minimum x
		/// </summary>
		public Double MinX
		{
			get { return m_dMinX; }
			set { m_dMinX = value; }
		}

		/// <summary>
		/// Get/Set minimum y
		/// </summary>
		public Double MinY
		{
			get { return m_dMinY; }
			set { m_dMinY = value; }
		}

		/// <summary>
		/// Get/Set the keywords
		/// </summary>
		public string Keywords
		{
			get { return m_szKeywords; }
			set { m_szKeywords = value; }
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Default constructor
		/// </summary>
		/// <param name="dMaxX"></param>
		/// <param name="dMinX"></param>
		/// <param name="dMaxY"></param>
		/// <param name="dMinY"></param>
		/// <param name="pcKeywords"></param>
		public AOISelectArgs(double dMaxX, double dMinX, double dMaxY, double dMinY, string pcKeywords)
		{
			MaxX = dMaxX;
			MaxY = dMaxY;
			MinX = dMinX;
			MinY = dMinY;
			Keywords = pcKeywords;
		}

		/// <summary>
		/// Default constructor
		/// </summary>
		/// <param name="dMaxX"></param>
		/// <param name="dMinX"></param>
		/// <param name="dMaxY"></param>
		/// <param name="dMinY"></param>
		public AOISelectArgs(double dMaxX, double dMinX, double dMaxY, double dMinY)
		{
			MaxX = dMaxX;
			MaxY = dMaxY;
			MinX = dMinX;
			MinY = dMinY;
			Keywords = null;
		}

		/// <summary>
		/// Default constructor
		/// </summary>
		public AOISelectArgs()
		{
			MaxX = Double.MaxValue;
			MaxY = Double.MaxValue;
			MinY = Double.MinValue;
			MinX = Double.MinValue;
			Keywords = null;
		}
		#endregion
	}

	/// <summary>
	/// Area of interest event handler
	/// </summary>
	public delegate void AOISelectHandler(object sender, AOISelectArgs e);
}
