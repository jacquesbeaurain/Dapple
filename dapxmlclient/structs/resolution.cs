using System;

namespace Geosoft.Dap.Common
{
   /// <summary>
   /// Define the resolution of an image
   /// </summary>
   [Serializable]
   public class Resolution 
   {
      #region Member Variables
      /// <summary></summary>
      
      protected int	iWidth;

      /// <summary></summary>      
      protected int	iHeight;
      #endregion

      #region Properties
      /// <summary>
      /// Get or set the width
      /// </summary>
      public int Width 
      {
         set { iWidth = value; }
         get { return iWidth; }
      }

      /// <summary>
      /// Get or set the height
      /// </summary>
      public int Height
      {
         set { iHeight = value; }
         get { return iHeight; }
      }
      #endregion

      #region Constructor
      /// <summary>
      /// Default constructor
      /// </summary>
      /// <remarks>Sets the width and height to 500</remarks>
      public Resolution() 
      {
         iWidth = 500;
         iHeight = 500;
      }
      #endregion
   }
}