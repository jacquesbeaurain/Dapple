using System;

namespace Geosoft.Dap.Common
{
   /// <summary>
   /// Define a dataset
   /// </summary>
   [Serializable]
   public class DataSet : IComparable
   {
      #region Member Variables
      /// <summary>
      /// The unique name
      /// </summary>
      protected string        m_szName;

      /// <summary>
      /// The user friendly name
      /// </summary>
      protected string        m_szTitle;

      /// <summary>
      /// The dataset type
      /// </summary>
      protected string        m_szType;

      /// <summary></summary>
      protected string        m_szEdition;

      /// <summary></summary>
      protected string        m_szHierarchy;

      /// <summary></summary>
      protected BoundingBox   m_hBoundingBox;

      /// <summary>
      /// Server Url that this dataset belongs to
      /// </summary>
      protected string        m_szUrl;
      #endregion

      #region Properties
      /// <summary>
      /// Get or set the unique name
      /// </summary>
      public string Name 
      {
         set { m_szName = value; }
         get { return m_szName; }
      }

      /// <summary>
      /// Get or set the user friendly name
      /// </summary>
      public string Title
      {
         set { m_szTitle = value; }
         get { return m_szTitle; }
      }

      /// <summary>
      /// Get or set the dataset type
      /// </summary>
      public string Type
      {
         set { m_szType = value; }
         get { return m_szType; }
      }

      /// <summary>
      /// Get or set the edition
      /// </summary>
      public string Edition
      {
         set { m_szEdition = value; }
         get { return m_szEdition; }
      }

      /// <summary>
      /// Get or set the hierarchy
      /// </summary>
      public string Hierarchy
      {
         set { m_szHierarchy = value; }
         get { return m_szHierarchy; }
      }

      /// <summary>
      /// Get or set the bounding box
      /// </summary>
      public BoundingBox   Boundary
      {
         set { m_hBoundingBox = value; }
         get { return m_hBoundingBox; }
      }

      /// <summary>
      /// Get or set the server this dataset came from
      /// </summary>
      public string Url
      {
         set { m_szUrl = value; }
         get { return m_szUrl; }
      }

      /// <summary>
      /// Get a unique name for this dataset
      /// </summary>
      public string UniqueName
      {
         get { 
            string strUniqueName = Url + "/" + Hierarchy + Name + "/" + Type;
            if (Edition != null)
               strUniqueName += "/" + Edition;
            return strUniqueName;
         }
      }
      #endregion

      #region Constructor
      /// <summary>
      /// Default construct
      /// </summary>
      public DataSet()
      {
         m_hBoundingBox = new BoundingBox();
         m_szUrl = String.Empty;
      }
      #endregion

      #region Member Functions
      /// <summary>
      /// Return the string reprenstation of this object
      /// </summary>
      /// <returns></returns>
      public override string ToString()
      {
         return Title;
      }

      /// <summary>
      /// Compare this object to another
      /// </summary>
      /// <param name="obj"></param>
      /// <returns></returns>
      public override bool Equals(object obj)
      {
         int i = CompareTo(obj);

         if (i == 0)
            return true;
         return false;
      }

      /// <summary>
      /// Get the hash code
      /// </summary>
      /// <returns></returns>
      public override int GetHashCode()
      {
         return UniqueName.GetHashCode() ^ Boundary.GetHashCode();
      }

      /// <summary>
      /// Overload == operator
      /// </summary>
      /// <param name="c1"></param>
      /// <param name="c2"></param>
      /// <returns></returns>
      public static bool operator == (DataSet c1, DataSet c2)
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
      public static bool operator != (DataSet c1, DataSet c2)
      {
         if ((object)c1 == null && (object)c2 == null) return false;
         if ((object)c1 == null) return true;

         return !c1.Equals(c2);
      }
      #endregion

      #region IComparable Members

      /// <summary>
      /// Compare object to this one to see if it is identical
      /// </summary>
      /// <param name="obj"></param>
      /// <returns></returns>
      public int CompareTo(object obj)
      {
         if (obj == null) return -1;

         if (obj.GetType() != typeof(DataSet))
            return -1;
      
         DataSet hDataSet = (DataSet)obj;      
   

         Int32 iRet = String.Compare(m_szUrl, hDataSet.m_szUrl);
         
         if (iRet == 0)
            iRet = String.Compare(m_szName, hDataSet.Name);            
         return iRet;
      }

      #endregion
   }     
}