using System;

namespace Geosoft.GX.DAPGetData
{
   /// <summary>
   /// Arguments for when the selected server changes
   /// </summary>
   public class ServerSelectArgs : EventArgs
   {
      #region Member Variables
      /// <summary>
      /// Server index
      /// </summary>
      protected Int32 m_iIndex;
      #endregion

      #region Properties
      /// <summary>
      /// Get the server index
      /// </summary>
      public Int32 Index
      {
         get { return m_iIndex; }
      }
      #endregion

      #region Constructor
      /// <summary>
      /// Default constructor
      /// </summary>
      /// <param name="iIndex"></param>
      public ServerSelectArgs(Int32 iIndex)
      {
         m_iIndex = iIndex;
      }      

      /// <summary>
      /// Default constructor
      /// </summary>
      public ServerSelectArgs()
      {
         m_iIndex = -1;
      }
      #endregion
   }

   /// <summary>
   /// Server select event delegate
   /// </summary>
   public delegate void ServerSelectHandler(object sender, ServerSelectArgs e);   

   /// <summary>
   /// Server remove event delegate
   /// </summary>
   public delegate void RemoveServerHandler(object sender, ServerSelectArgs e);   
}
