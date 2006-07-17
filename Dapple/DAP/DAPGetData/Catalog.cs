using System;
using System.Xml;
using System.Collections;

using Geosoft.Dap;
using Geosoft.Dap.Common;

namespace Geosoft.GX.DAPGetData
{
	/// <summary>
	/// Hold a specific catalog
	/// </summary>
   public class Catalog
   {
      #region Member Variables
      protected XmlDocument   m_hCatalog;
      protected string        m_strCatalogEdition;

      protected string        m_strConfigurationEdition;
      #endregion

      #region Properties
      /// <summary>
      /// Get the catalog
      /// </summary>
      public XmlDocument Document
      {
         get { return m_hCatalog; }
      }

      /// <summary>
      /// Get the catalog edition
      /// </summary>
      public string Edition
      {
         get { return m_strCatalogEdition; }
      }

      /// <summary>
      /// Get the configuration edition
      /// </summary>
      public string ConfigurationEdition
      {
         get { return m_strConfigurationEdition; }
      }
      #endregion

      #region Constructor
      public Catalog(XmlDocument hCatalog, string strCatalogEdition)
      {
         m_hCatalog = hCatalog;
         m_strCatalogEdition = strCatalogEdition;
         m_strConfigurationEdition = string.Empty;

         // --- look for the configuration edition ---
         try
         {
            XmlNodeList oNodeList = hCatalog.SelectNodes("//" + Geosoft.Dap.Xml.Common.Constant.Tag.CONFIGURATION_TAG);
            if (oNodeList == null || oNodeList.Count == 0) return;

            XmlNode oAttr = oNodeList[0].Attributes.GetNamedItem(Geosoft.Dap.Xml.Common.Constant.Attribute.VERSION_ATTR);
            if (oAttr != null)
               m_strConfigurationEdition = oAttr.Value;
         }
         catch
         {
         }
      }
      #endregion

      #region Methods

      public DataSet FindDataset(string name, Server oServer)
      {
         try
         {
            DataSet hDataset = new DataSet();
            oServer.Command.Parser.DataSet(FindDatasetHelper(name, m_hCatalog["geosoft_xml"]["response"]["catalog"]),
               out hDataset);
            return hDataset;
         }
         catch { }
         return null;
      }

      private XmlNode FindDatasetHelper(string name, XmlNode oNode)
      {
         foreach(XmlNode oChild in oNode.ChildNodes)
         {
            if (oChild.Name == Geosoft.Dap.Xml.Common.Constant.Tag.COLLECTION_TAG)
            {
               XmlNode oTemp = FindDatasetHelper(name, oChild);
               if (oTemp != null)
               {
                  return oTemp;
               }
            }
            else if ( oChild.Attributes["name"] != null && oChild.Attributes["name"].Value == name)
            {
               return oChild;
            }
         }
         return null;
      }

      #endregion
   }
}
