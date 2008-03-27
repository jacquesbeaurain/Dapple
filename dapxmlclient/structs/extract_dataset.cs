using System;

namespace Geosoft.Dap.Common
{
	/// <summary>
	/// Define a dataset to extract
	/// </summary>
	[Serializable]
	public class ExtractDataSet
	{
		#region Member Variables
		/// <summary>
		/// Unique name of the dataset to extract
		/// </summary>
		protected DataSet m_hDataSet;

		/// <summary>
		/// The format to extract the data to
		/// </summary>
		protected string m_szFormat;

		/// <summary>
		/// The resolution
		/// </summary>
		protected double m_dResolution;
		#endregion

		#region Properties
		/// <summary>
		/// Get or set the dataset name
		/// </summary>
		public DataSet DataSet
		{
			set { m_hDataSet = value; }
			get { return m_hDataSet; }
		}

		/// <summary>
		/// Get or set the format
		/// </summary>
		public string Format
		{
			set { m_szFormat = value; }
			get { return m_szFormat; }
		}

		/// <summary>
		/// Get or set the resolution
		/// </summary>
		public double Resolution
		{
			set { m_dResolution = value; }
			get { return m_dResolution; }
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Default constructor
		/// </summary>
		public ExtractDataSet()
		{
			m_hDataSet = null;
			m_szFormat = null;
			m_dResolution = 0;
		}

		/// <summary>
		/// Default constructor
		/// </summary>
		/// <param name="hDataSet">dataset</param>
		/// <param name="szFormat">Format to extract the dataset to</param>
		/// <param name="dResolution">Resolution of the dataset</param>
		public ExtractDataSet(DataSet hDataSet, string szFormat, double dResolution)
		{
			m_hDataSet = hDataSet;
			m_szFormat = szFormat;
			m_dResolution = dResolution;
		}
		#endregion
	}
}