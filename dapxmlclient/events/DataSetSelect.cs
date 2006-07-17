using System;
using Geosoft.Dap.Common;

namespace Geosoft.Dap
{
   /// <summary>
   /// Arguments for when the area of interest changes
   /// </summary>
   public class DataSetSelectedArgs : EventArgs
   {
      #region Member Variables
      /// <summary>
      /// Selected dataset
      /// </summary>
      protected DataSet m_hDataSet;

      /// <summary>
      /// Hold whether this dataset has just been selected or deselected
      /// </summary>
      protected bool    m_bSelected;
      #endregion

      #region Properties
      /// <summary>
      /// Get/Set the selected dataset
      /// </summary>
      public DataSet DataSet
      {
         get { return m_hDataSet; }
         set { m_hDataSet = value; }
      }      

      /// <summary>
      /// Get/Set whether this datset is selected or deselected
      /// </summary>
      public bool Selected
      {
         get { return m_bSelected; }
         set { m_bSelected = value; }
      }
      #endregion

      #region Constructor
      /// <summary>
      /// Default constructor
      /// </summary>
      /// <param name="hDataSet"></param>
      /// <param name="bSelected"></param>
      public DataSetSelectedArgs(DataSet hDataSet, bool bSelected)
      {
         DataSet = hDataSet;         
         Selected = bSelected;
      }

      /// <summary>
      /// Default constructor
      /// </summary>
      public DataSetSelectedArgs()
      {
         DataSet = null;
         Selected = false;
      }
      #endregion
   }

   /// <summary>
   /// Dataset selected event handler
   /// </summary>
   public delegate void DataSetSelectedHandler(object sender, DataSetSelectedArgs e);   
}
