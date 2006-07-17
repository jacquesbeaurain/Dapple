using System;

namespace Geosoft.GX.DAPGetData
{
   /// <summary>
   /// Arguments for when the area of interest changes
   /// </summary>
   public class RowChangedArgs : EventArgs
   {
      #region Member Variables
      /// <summary>
      /// Old row
      /// </summary>
      protected Int32 m_iOldRow;

      /// <summary>
      /// New row
      /// </summary>
      protected Int32 m_iNewRow;

      #endregion

      #region Properties
      /// <summary>
      /// Get/Set the old selected row
      /// </summary>
      public Int32 OldRow
      {
         get { return m_iOldRow; }
         set { m_iOldRow = value; }
      }      

      /// <summary>
      /// Get/Set the new row
      /// </summary>
      public Int32 NewRow
      {
         get { return m_iNewRow; }
         set { m_iNewRow = value; }
      }
      #endregion

      #region Constructor
      /// <summary>
      /// Default constructor
      /// </summary>
      /// <param name="iOldRow"></param>
      /// <param name="iNewRow"></param>
      public RowChangedArgs(Int32 iOldRow, Int32 iNewRow)
      {
         NewRow = iNewRow;
         OldRow = iOldRow;
      }      
      #endregion
   }

   /// <summary>
   /// Dataset selected event handler
   /// </summary>
   public delegate void RowChangedHandler(object sender, RowChangedArgs e);  
}
