using System;

namespace Geosoft.Dap.Common
{
   /// <summary>
   /// Define a point
   /// </summary>
   [Serializable]
   public class Point 
   {
      #region Member Variables
      /// <summary>
      /// x coordinate
      /// </summary>
      protected double	         dX;

      /// <summary>
      /// y coordinate 
      /// </summary>
      protected double	         dY;

      /// <summary>
      /// z coordinate
      /// </summary>
      protected double	         dZ;
      #endregion
      
      #region Properties
      /// <summary>
      /// Get or set the x coordinate
      /// </summary>
      public double X 
      {
         set { dX = value; }
         get { return dX; }
      }

      /// <summary>
      /// Get or set the y coordinate
      /// </summary>
      public double Y
      {
         set { dY = value; }
         get { return dY; }
      }

      /// <summary>
      /// Get or set the z coordiante
      /// </summary>
      public double Z
      {
         set { dZ = value; }
         get { return dZ; }
      }
      #endregion

      #region Constructor
      /// <summary>
      /// Default constructor
      /// </summary>
      public Point() 
      {
         dX = 0;
         dY = 0;        
         dZ = 0;
      }

      /// <summary>
      /// 1 Dimensional constructor
      /// </summary>
      /// <param name="x">x coordinate</param>
      public Point( double x )
      {
         dX = x;
         dY = 0;
         dZ = 0;
      }

      /// <summary>
      /// 2 Dimensional constructor
      /// </summary>
      /// <param name="x">x coordinate</param>
      /// <param name="y">y coordinate</param>
      public Point( double x, double y )
      {
         dX = x;
         dY = y;
         dZ = 0;
      }

      /// <summary>
      /// 3 Dimensional constructor
      /// </summary>
      /// <param name="x">x coordinate</param>
      /// <param name="y">y coordinate</param>
      /// <param name="z">z coordinate</param>
      public Point( double x, double y, double z )
      {
         dX = x;
         dY = y;         
         dZ = z;
      }
      #endregion
   }     
}