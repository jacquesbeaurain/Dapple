using System;

namespace Geosoft.Dap.Common
{
   /// <summary>
   /// Define a coordinate system
   /// </summary>
   [Serializable]
   public class CoordinateSystem 
   {
      #region Enums
      /// <summary>
      /// The list of types that can be queried by GetCoordinateSystemList request
      /// </summary>
      public enum Types 
      {
         /// <summary></summary>
         DATUM = 0,
         
         /// <summary></summary>
         PROJECTION,
         
         /// <summary></summary>
         UNITS,

         /// <summary></summary>
         LOCAL_DATUM_DESCRIPTION,

         /// <summary></summary>
         LOCAL_DATUM_NAME
      }

      /// <summary>
      /// The types of projection types currently supported.
      /// Standard = datum, projection, units, local datum
      /// Esri = esri projection string
      /// </summary>
      public enum ProjectionTypes
      {
         /// <summary></summary>
         UNKNOWN,
         
         /// <summary></summary>
         STANDARD,

         /// <summary></summary>
         ESRI
      }
      #endregion

      #region Member Variables
      /// <summary>
      /// The string names for the list of types. The entries correspond to the ordering of the Types enum. 
      /// </summary>
      static public string []TYPES = { "datum", "projection", "units", "local datum description", "local datum name" }; 

      /// <summary></summary>
      protected string	szProjection;

      /// <summary></summary>
      protected string	szDatum;

      /// <summary></summary>
      protected string	szMethod;

      /// <summary></summary>
      protected string	szUnits;

      /// <summary></summary>
      protected string	szLocalDatum;

      /// <summary></summary>
      protected string	szEsri;

      /// <summary></summary>
      protected ProjectionTypes  m_eProjectionType;
      #endregion

      #region Properties
      /// <summary>
      /// Get or set the projection
      /// </summary>
      public string Projection
      {
         set { szProjection = value; }
         get { return szProjection; }
      }

      /// <summary>
      /// Get or set the datum
      /// </summary>
      public string Datum 
      {
         set { szDatum = value; m_eProjectionType = ProjectionTypes.STANDARD; }
         get { return szDatum; }
      }

      /// <summary>
      /// Get or set the method
      /// </summary>
      public string Method
      {
         set { szMethod = value; m_eProjectionType = ProjectionTypes.STANDARD; }
         get { return szMethod; }
      }

      /// <summary>
      /// Get or set the units
      /// </summary>
      public string Units
      {
         set { szUnits = value; m_eProjectionType = ProjectionTypes.STANDARD; }
         get { return szUnits; }
      }

      /// <summary>
      /// Get or set the local daum
      /// </summary>
      public string LocalDatum
      {
         set { szLocalDatum = value; m_eProjectionType = ProjectionTypes.STANDARD; }
         get { return szLocalDatum; }
      }

      /// <summary>
      /// Get or set the esri projection string
      /// </summary>
      public string Esri
      {
         set { szEsri = value; m_eProjectionType = ProjectionTypes.ESRI; }
         get { return szEsri; }
      }

      /// <summary>
      /// Get the projection type
      /// </summary>
      public ProjectionTypes ProjectionType
      {
         get { return m_eProjectionType; }
      }
      #endregion

      #region Construtor
      /// <summary>
      /// Default constructor
      /// </summary>
      public CoordinateSystem() 
      {
         szProjection = String.Empty;
         szDatum = String.Empty;
         szMethod = String.Empty;
         szUnits = String.Empty;
         szLocalDatum = String.Empty;
         szEsri = String.Empty;
         m_eProjectionType = ProjectionTypes.UNKNOWN;
      }

      /// <summary>
      /// Copy constructor
      /// </summary>
      public CoordinateSystem(CoordinateSystem hCS) 
      {
         szProjection = hCS.Projection;
         szDatum = hCS.Datum;         
         szMethod = hCS.Method;
         szUnits = hCS.Units;
         szLocalDatum = hCS.LocalDatum;
         szEsri = hCS.Esri;
         m_eProjectionType = hCS.ProjectionType;
      }
      #endregion

      #region Overrrides
      /// <summary>
      /// Get the hash code for this object
      /// </summary>
      /// <returns></returns>
      public override int GetHashCode()
      {
         return szDatum.GetHashCode() ^ szProjection.GetHashCode() ^ szUnits.GetHashCode() ^ szLocalDatum.GetHashCode() ^ szEsri.GetHashCode();
      }

      /// <summary>
      /// Compare two coordinate system objects
      /// </summary>
      /// <param name="obj"></param>
      /// <returns></returns>
      public override bool Equals(object obj)
      {
         if (obj.GetType() == typeof(CoordinateSystem))
         {
            CoordinateSystem hCS = (CoordinateSystem)obj;

            if (hCS.m_eProjectionType == m_eProjectionType &&
               hCS.szDatum == szDatum &&
               hCS.szEsri == szEsri &&
               hCS.szLocalDatum == szLocalDatum &&
               hCS.szMethod == szMethod &&
               hCS.szUnits == szUnits)
               return true;         
         }
         return false;
      }

      /// <summary>
      /// Get the string representation of this coordinate system
      /// </summary>
      /// <returns></returns>
      public override string ToString()
      {
         string   str;

         if (szProjection != null && szProjection.Length > 0) 
         {
            str = szProjection;
            str = str.TrimEnd('\"');
            str = str.TrimStart('\"');
         } 
         else 
         {
            str = szDatum;
            if (szMethod != null && szMethod.Length > 0)
               str += " / " + szMethod;
         }

         return str;
      }

      #endregion
   }
}