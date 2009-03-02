using System;
using System.Xml;
using System.Collections;

using Geosoft.Dap;
using Geosoft.Dap.Common;

namespace Geosoft.GX.DAPGetData
{
	/// <summary>
	/// Hold a collection of catalogs
	/// </summary>
	internal class CatalogCollection
	{
      #region Member Variables
      protected Hashtable m_hCatalogList;
      protected Server m_oServer;
      #endregion
      
      #region Constructor
      internal CatalogCollection(Server oServer)
      {
         m_oServer = oServer;
         m_hCatalogList = new Hashtable();
      }      
      #endregion

      #region Member Functions
      /// <summary>
      /// Get the catalog at the specific edition and bounding box of this catalog
      /// </summary>
      /// <param name="hBoundingBox"></param>
      /// <param name="strCatalogEdition"></param>
      /// <returns></returns>
      internal Catalog GetCatalog(BoundingBox hBoundingBox, string strKeywords)
      {
         CatalogHash hHash = new CatalogHash(hBoundingBox, strKeywords);
         Catalog     hRetCatalog = null;
         string      strEdition = String.Empty;
         string      strConfigEdition = String.Empty;

         hRetCatalog = (Catalog)m_hCatalogList[hHash];         

         if (hRetCatalog != null)
         {
            try
            {
               m_oServer.Command.GetCatalogEdition(out strConfigEdition, out strEdition, null);
            } 
            catch
            {
               strEdition = "Default";
            }

            if (strEdition != hRetCatalog.Edition || strConfigEdition != hRetCatalog.ConfigurationEdition)
            {
               lock (m_hCatalogList) {
                  if (m_hCatalogList.ContainsKey(hHash))
                     m_hCatalogList.Remove(hHash);
               }
               hRetCatalog = null;
            }
         }

         if (hRetCatalog == null)
         {
            XmlDocument hDocument;

            try
            {
               m_oServer.Command.GetCatalogEdition(out strConfigEdition, out strEdition, null);
            }
            catch
            {
               strEdition = "Default";
            }

            try
            {
               hDocument = m_oServer.Command.GetCatalog(String.Empty, -1, 0, 0, strKeywords, hBoundingBox, null);
               m_oServer.Command.Parser.PruneCatalog(hDocument);

               hRetCatalog = new Catalog(hDocument, strEdition);

               lock (m_hCatalogList) {
                  if (!m_hCatalogList.ContainsKey(hHash))
                     m_hCatalogList.Add(hHash, hRetCatalog);
                  else
                     m_hCatalogList[hHash] = hRetCatalog;
               }
            } 
            catch (Exception e)
            {
               GetDapError.Instance.Write("GetCatalog - " + e.Message);
            }
         }
         return hRetCatalog;
      }

      /// <summary>
      /// Clear all the catalogs from memory
      /// </summary>
      internal void Clear()
      {
         m_hCatalogList.Clear();
      }
      #endregion
	}

   /// <summary>
   /// Class to calculate the hash for each catalog, based on its bounding box and keywords
   /// </summary>
   internal class CatalogHash
   {
      protected BoundingBox   m_hBox;
      protected string        m_strKeywords;

      /// <summary>
      /// Default constructor
      /// </summary>
      /// <param name="hBox"></param>
      /// <param name="strKeywords"></param>
      internal CatalogHash(BoundingBox hBox, string strKeywords)
      {
         m_hBox = hBox;
         m_strKeywords = strKeywords;
      }

      /// <summary>
      /// Get the hash code
      /// </summary>
      /// <returns></returns>
		public override int GetHashCode()
      {
         if (m_hBox == null)
            return m_strKeywords.GetHashCode();

         return m_hBox.GetHashCode() ^ m_strKeywords.GetHashCode();
      }

      /// <summary>
      /// Compare this class
      /// </summary>
      /// <param name="obj"></param>
      /// <returns></returns>
		public override bool Equals(object obj)
      {
         if (obj.GetType() != typeof(CatalogHash))
            return false;

         if (m_hBox == ((CatalogHash)obj).m_hBox && String.Compare(((CatalogHash)obj).m_strKeywords, m_strKeywords, true) == 0)
         {
            return true;
         }
         return false;
      }

   }
}
