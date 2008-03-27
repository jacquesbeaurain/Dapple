using System;
using Geosoft.Dap.Common;

namespace Geosoft.Dap
{
	/// <summary>
	/// Arguments for when the area of interest changes
	/// </summary>
	public class DataSetArgs : EventArgs
	{
		#region Member Variables
		/// <summary>
		/// dataset
		/// </summary>
		protected DataSet m_hDataSet;
		#endregion

		#region Properties
		/// <summary>
		/// Get/Set the dataset
		/// </summary>
		public DataSet DataSet
		{
			get { return m_hDataSet; }
			set { m_hDataSet = value; }
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Default constructor
		/// </summary>
		/// <param name="hDataSet"></param>
		public DataSetArgs(DataSet hDataSet)
		{
			DataSet = hDataSet;
		}

		/// <summary>
		/// Default constructor
		/// </summary>
		public DataSetArgs()
		{
			DataSet = null;
		}
		#endregion
	}

	/// <summary>
	/// Dataset delete event handlers
	/// </summary>
	public delegate void DataSetDeletedHandler(object sender, DataSetArgs e);

	/// <summary>
	/// Dataset promoted event handlers
	/// </summary>
	public delegate void DataSetPromotedHandler(object sender, DataSetArgs e);

	/// <summary>
	/// Dataset demoted event handlers
	/// </summary>
	public delegate void DataSetDemotedHandler(object sender, DataSetArgs e);
}
