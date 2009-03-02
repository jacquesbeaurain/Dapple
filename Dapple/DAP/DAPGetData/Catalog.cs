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
   internal class Catalog
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
      internal XmlDocument Document
      {
         get { return m_hCatalog; }
      }

      /// <summary>
      /// Get the catalog edition
      /// </summary>
      internal string Edition
      {
         get { return m_strCatalogEdition; }
      }

      /// <summary>
      /// Get the configuration edition
      /// </summary>
      internal string ConfigurationEdition
      {
         get { return m_strConfigurationEdition; }
      }
      #endregion

      #region Constructor
      internal Catalog(XmlDocument hCatalog, string strCatalogEdition)
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
   }
}
