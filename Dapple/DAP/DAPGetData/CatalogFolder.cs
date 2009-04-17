using System;
using System.Collections;
using System.Text;
using System.Globalization;

namespace Geosoft.GX.DAPGetData
{
   internal class CatalogFolder
   {
      #region Member Variables
      protected SortedList m_oSubFolders = new SortedList();
      protected string m_strName = string.Empty;
      protected string m_strHierarchy = string.Empty;
      protected int m_iTimestamp = 0;
      protected string m_strConfigurationEdition;
      #endregion

      #region Properties
      /// <summary>
      /// Get the folder name
      /// </summary>
      internal string Name
      {
         get { return m_strName; }
      }

      /// <summary>
      /// Get the folder hierarchy for this node
      /// </summary>
      internal string Hierarchy
      {
         get { return m_strHierarchy; }
      }

      /// <summary>
      /// Get the timestamp
      /// </summary>
      internal int Timestamp
      {
         get { return m_iTimestamp; }
      }

      /// <summary>
      /// Get the configuration edition
      /// </summary>
      internal string ConfigurationEdition
      {
         get { return m_strConfigurationEdition; }
      }

      /// <summary>
      /// Get a list of the subfolders
      /// </summary>
      internal ICollection Folders
      {
         get { return m_oSubFolders.Values; }
      }
      #endregion

      #region Constructor
      /// <summary>
      /// Default constructor
      /// </summary>
      /// <param name="oFolderNode"></param>
      internal CatalogFolder(System.Xml.XmlNode oFolderNode, string strHierarchy)
      {
         System.Xml.XmlNode oAttr;

         oAttr = oFolderNode.Attributes.GetNamedItem("name");
         if (oAttr == null) throw new ArgumentException();
         m_strName = oAttr.Value;
         m_strHierarchy = strHierarchy + '/' + m_strName;

         oAttr = oFolderNode.Attributes.GetNamedItem("value");
         if (oAttr == null) throw new ArgumentException();
			m_iTimestamp = Int32.Parse(oAttr.Value, NumberStyles.Any, CultureInfo.InvariantCulture);

         foreach (System.Xml.XmlNode oChildNode in oFolderNode.ChildNodes)
         {
            CatalogFolder oFolder;

            oFolder = new CatalogFolder(oChildNode, m_strHierarchy);
            m_oSubFolders.Add(oFolder.Name, oFolder);
         }
      }

      protected CatalogFolder()
      {
      }
      #endregion

      #region Public Methods
      /// <summary>
      /// Get a specific subfolder
      /// </summary>
      /// <param name="strName"></param>
      /// <returns></returns>
      internal CatalogFolder GetFolder(string strName)
      {
         return (CatalogFolder)m_oSubFolders[strName];
      }

      /// <summary>
      /// Get the hash code for this folder
      /// </summary>
      /// <returns></returns>
		public override int GetHashCode()
      {
         return m_strHierarchy.GetHashCode();
      }

      #endregion

      /// <summary>
      /// Parse the catalog hierarchy 
      /// </summary>
      /// <param name="oDocument"></param>
      /// <returns></returns>
      internal static CatalogFolder Parse(System.Xml.XmlDocument oDocument, out string strConfigurationEdition)
      {
         CatalogFolder oFolder = null;
         System.Xml.XmlNode oGeosoftXmlNode;
         System.Xml.XmlNode oCatalogHierarchyNode;
         System.Xml.XmlNode oCatalogEditionNode;
         System.Xml.XmlNode oAttr;
         System.Xml.XmlNodeList oNodeList;

         strConfigurationEdition = string.Empty;

         oGeosoftXmlNode = oDocument.DocumentElement;
         if (oGeosoftXmlNode == null || oGeosoftXmlNode.ChildNodes.Count == 0) return null;

         oNodeList = oGeosoftXmlNode.SelectNodes("//" + Geosoft.Dap.Xml.Common.Constant.Tag.CONFIGURATION_TAG);
         if (oNodeList == null || oNodeList.Count == 0) return null;

         oCatalogEditionNode = oNodeList[0];
         if (oCatalogEditionNode == null) return null;

         oAttr = oCatalogEditionNode.Attributes.GetNamedItem("version");
         if (oAttr == null) return null;
         strConfigurationEdition = oAttr.Value;

         oNodeList = oGeosoftXmlNode.SelectNodes("//" + Geosoft.Dap.Xml.Common.Constant.Tag.CATALOG_HIERARCHY_TAG);
         if (oNodeList == null || oNodeList.Count == 0) return null;

         oCatalogHierarchyNode = oNodeList[0];
         if (oCatalogHierarchyNode == null || oCatalogHierarchyNode.ChildNodes.Count == 0) return null;

         try
         {
            oFolder = new CatalogFolder(oCatalogHierarchyNode.ChildNodes[0], string.Empty);
            oFolder.m_strConfigurationEdition = strConfigurationEdition;
         }
         catch
         {
            return null;
         }
         return oFolder;
      }
   }
}
