using System;

namespace Geosoft.Dap.Common
{
   /// <summary>
   /// Define an rectangular area within a particular coordinate system
   /// </summary>
   [Serializable]
   public class BoundingBox 
   {
      #region Member Variables
      /// <summary>
      /// Maximum x coordinate
      /// </summary>
      protected double	         dMaxX;

      /// <summary>
      /// Maximum y coordinate
      /// </summary>
      protected double	         dMaxY;

      /// <summary>
      /// Minimum x coordiante
      /// </summary>
      protected double	         dMinX;

      /// <summary>
      /// Minimum y coordinate
      /// </summary>
      protected double	         dMinY;

      /// <summary>
      /// The coordinate system of the bounding box
      /// </summary>
      protected CoordinateSystem hCoordinateSystem;
      #endregion

      #region Properties
      /// <summary>
      /// Get or set the maximum x coordinate
      /// </summary>
      public double MaxX 
      {
         set { dMaxX = value; }
         get { return dMaxX; }
      }

      /// <summary>
      /// Get or set the maximum y coordinate
      /// </summary>
      public double MaxY
      {
         set { dMaxY = value; }
         get { return dMaxY; }
      }

      /// <summary>
      /// Get or set the minimum x coordinate
      /// </summary>
      public double MinX
      {
         set { dMinX = value; }
         get { return dMinX; }
      }

      /// <summary>
      /// Get or set the minimum y coordinate
      /// </summary>
      public double MinY
      {
         set { dMinY = value; }
         get { return dMinY; }
      }

      /// <summary>
      /// Get or set the coordinate system
      /// </summary>
      public CoordinateSystem CoordinateSystem
      {
         set { hCoordinateSystem = value; }
         get { return hCoordinateSystem; }
      }
      #endregion

      #region Constructor
      /// <summary>
      /// Default construtor
      /// </summary>
      public BoundingBox() 
      {
         dMaxX = 0;
         dMaxY = 0;
         dMinX = 0;
         dMinY = 0;

         // --- set the default coordinate system to WGS 84 ---
         hCoordinateSystem = new CoordinateSystem();
         hCoordinateSystem.Datum = "WGS 84";
      }

      /// <summary>
      /// Extended constructor
      /// </summary>
      /// <param name="maxX">The maximum x coordinate</param>
      /// <param name="maxY">The maximum y coordinate</param>
      /// <param name="minX">The minimum x coordiante</param>
      /// <param name="minY">The minimum y coordinate</param>
      public BoundingBox( double maxX, double maxY, double minX, double minY )
      {
         dMaxX = maxX;
         dMaxY = maxY;
         dMinX = minX;
         dMinY = minY;

         // --- set the default coordinate system to WGS 84 ---
         hCoordinateSystem = new CoordinateSystem();
         hCoordinateSystem.Datum = "WGS 84";
      }

      /// <summary>
      /// Extended constructor
      /// </summary>
      /// <param name="hBox">Bounding Box to copy</param>
      public BoundingBox(BoundingBox hBox)
      {
         dMaxX = hBox.dMaxX;
         dMaxY = hBox.dMaxY;
         dMinX = hBox.dMinX;
         dMinY = hBox.dMinY;

         // --- set the default coordinate system to WGS 84 ---
         hCoordinateSystem = new CoordinateSystem(hBox.CoordinateSystem);
      }
      #endregion

      #region Overrides
      /// <summary>
      /// Get the hash code for this object
      /// </summary>
      /// <returns></returns>
      public override int GetHashCode()
      {
         return dMaxX.GetHashCode() ^ dMaxY.GetHashCode() ^ dMinX.GetHashCode() ^ dMinY.GetHashCode();
      }

      /// <summary>
      /// Compare two bounding box objects
      /// </summary>
      /// <param name="obj"></param>
      /// <returns></returns>
      public override bool Equals(object obj)
      {
         if (obj == null) return false;

         if (obj.GetType() == typeof(BoundingBox))
         {
            BoundingBox hBB = (BoundingBox)obj;

            if (hBB.dMaxX == dMaxX && hBB.dMaxY == dMaxY && hBB.dMinX == dMinX && hBB.dMinY == dMinY && hCoordinateSystem == hBB.hCoordinateSystem)
               return true;
         }
         return false;
      }

      /// <summary>
      /// Overload == operator
      /// </summary>
      /// <param name="c1"></param>
      /// <param name="c2"></param>
      /// <returns></returns>
      public static bool operator == (BoundingBox c1, BoundingBox c2)
      {         
         if ((object)c1 == null && (object)c2 == null) return true;
         if ((object)c1 == null) return false;

         return c1.Equals(c2);
      }

      /// <summary>
      /// Overload != operator
      /// </summary>
      /// <param name="c1"></param>
      /// <param name="c2"></param>
      /// <returns></returns>
      public static bool operator != (BoundingBox c1, BoundingBox c2)
      {
         if ((object)c1 == null && (object)c2 == null) return false;
         if ((object)c1 == null) return true;

         return !c1.Equals(c2);
      }
      #endregion
   }     
}