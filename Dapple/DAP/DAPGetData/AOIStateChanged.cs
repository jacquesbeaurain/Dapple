using System;

namespace Geosoft.GX.DAPGetData
{
   /// <summary>
   /// Arguments for when we want to view the meta for a particular dataset
   /// </summary>
   public class AOIStateChangedArgs : EventArgs
   {
      #region Member Variables
      /// <summary>
      /// Whether we area of interest should be enabled or disabled
      /// </summary>
      protected bool m_bEnabled;      
      #endregion

      #region Properties
      /// <summary>
      /// Get/Set the enabled flag
      /// </summary>
      public bool Enabled
      {
         get { return m_bEnabled; }
         set { m_bEnabled = value; }
      }      
      #endregion

      #region Constructor
      /// <summary>
      /// Default constructor
      /// </summary>
      /// <param name="bEnabled"></param>      
      public AOIStateChangedArgs(bool bEnabled)
      {
         Enabled = bEnabled;
      }      

      /// <summary>
      /// Default constructor
      /// </summary>
      public AOIStateChangedArgs()
      {
         Enabled = true;
      }
      #endregion
   }

   /// <summary>
   /// Server select event handler
   /// </summary>
   public delegate void AOIStateChangedHandler(object sender, AOIStateChangedArgs e);   
}
