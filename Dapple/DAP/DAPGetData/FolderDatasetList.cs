using System;
using System.Collections;
using System.Text;

namespace Geosoft.GX.DAPGetData
{
   [Serializable]
   public class FolderDatasetList
   {
      #region Member Variables
      protected string m_strKey;
      protected ArrayList m_oDatasets;
      protected int m_iTimestamp;
      #endregion

      #region Properties
      public string Key
      {
         get { return m_strKey; }
      }

      public int Timestamp
      {
         get { return m_iTimestamp; }
      }

      public ICollection Datasets
      {
         get { return m_oDatasets; }
      }
      #endregion

      #region Constructor
      public FolderDatasetList(string strKey, int iTimestamp, ArrayList oList)
      {
         m_strKey = strKey;
         m_iTimestamp = iTimestamp;
         m_oDatasets = oList;

         if (m_oDatasets != null) {
            DatasetCompare dc = new DatasetCompare();
            m_oDatasets.Sort(dc);
         }
      }
      #endregion

      public static FolderDatasetList Parse(Server oServer, string strKey, int iTimestamp, System.Xml.XmlDocument oCatalog, out string strEdition)
      {
         strEdition = string.Empty;
         ArrayList oList = new ArrayList();


         // --- get the catalog edition ---

         System.Xml.XmlNodeList oNodeList = oCatalog.SelectNodes("//" + Geosoft.Dap.Xml.Common.Constant.Tag.CONFIGURATION_TAG);
         if (oNodeList != null && oNodeList.Count != 0)
         {
            System.Xml.XmlNode oAttr = oNodeList[0].Attributes.GetNamedItem(Geosoft.Dap.Xml.Common.Constant.Attribute.VERSION_ATTR);
            if (oAttr != null)
               strEdition = oAttr.Value;
         }


         oNodeList = oCatalog.SelectNodes("//" + Geosoft.Dap.Xml.Common.Constant.Tag.ITEM_TAG);
         if (oNodeList != null)
         {
            foreach (System.Xml.XmlElement oDatasetNode in oNodeList)
            {
               Geosoft.Dap.Common.DataSet oDataSet;
               oServer.Command.Parser.DataSet(oDatasetNode, out oDataSet);
               oList.Add(oDataSet);
            }
         }
         return new FolderDatasetList(strKey, iTimestamp, oList);
      }

      private class DatasetCompare : IComparer
      {
         public int Compare(object ds1, object ds2)
         {
            return string.Compare(((Geosoft.Dap.Common.DataSet)ds1).Title, ((Geosoft.Dap.Common.DataSet)ds2).Title, true);
         }
      }
   }

   sealed class FolderDatasetListDeserializationBinder : System.Runtime.Serialization.SerializationBinder
   {
      public override Type BindToType(string assemblyName, string typeName)
      {
         if (assemblyName.ToLower().Contains("dapxmlclient")) {
            Geosoft.Dap.Common.DataSet oD = new Geosoft.Dap.Common.DataSet();
            System.Reflection.Assembly oAssembly = System.Reflection.Assembly.GetAssembly(oD.GetType());
            assemblyName = oAssembly.FullName;
         }
         
         Type oType = Type.GetType(string.Format("{0}, {1}", typeName, assemblyName));
         return oType;
      }
   }
}
