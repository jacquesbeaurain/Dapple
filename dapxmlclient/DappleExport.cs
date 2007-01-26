using System;
using System.Collections.Generic;
using System.Text;

namespace Geosoft.Dap.Common
{
   /// <summary>
   /// Export a result set to dapple
   /// </summary>
   public class DappleExport
   {
      /// <summary>
      /// Get the xml for this list of datasets
      /// </summary>\
      /// <param name="oDapCommand"></param>
      /// <param name="oDatasets"></param>
      /// <param name="oSelectedDatasets"></param>
      /// <returns></returns>
      public static System.Xml.XmlDocument GetXml(Command oDapCommand, System.Collections.SortedList oDatasets, System.Collections.ArrayList oSelectedDatasets)
      {
         System.Xml.XmlDocument oOutputXml = new System.Xml.XmlDocument();         

         oOutputXml.AppendChild(oOutputXml.CreateXmlDeclaration("1.0", "UTF-8", string.Empty));

         System.Xml.XmlElement oGeosoftXml = oOutputXml.CreateElement("geosoft_xml");
         oOutputXml.AppendChild(oGeosoftXml);
         
         foreach (string strKey in oSelectedDatasets) {
            Dap.Common.DataSet oDataset = (Dap.Common.DataSet)oDatasets[strKey];

            if (oDataset != null)
               OutputDataset(oDapCommand, oGeosoftXml, oDataset);
         }
         return oOutputXml;
      }

      /// <summary>
      /// Add this dataset into the kml document
      /// </summary>
      /// <param name="oDapCommand"></param>
      /// <param name="oXmlNode"></param>
      /// <param name="oDataset"></param>
      private static void OutputDataset(Command oDapCommand, System.Xml.XmlNode oXmlNode, Dap.Common.DataSet oDataset)
      {
         System.Xml.XmlElement oDisplayMapNode;
         System.Xml.XmlAttribute oAttr;

         // --- write out the dataset node ---

         oDisplayMapNode = oXmlNode.OwnerDocument.CreateElement("display_map");

         oAttr = oXmlNode.OwnerDocument.CreateAttribute("type");
         oAttr.Value = "DAP";
         oDisplayMapNode.Attributes.Append(oAttr);

         string strDapUrl = oDapCommand.Url;
         if (strDapUrl.ToUpper().StartsWith("HTTP://"))
            strDapUrl = strDapUrl.Substring(7);
         oAttr = oXmlNode.OwnerDocument.CreateAttribute("server");
         oAttr.Value = strDapUrl;
         oDisplayMapNode.Attributes.Append(oAttr);

         oAttr = oXmlNode.OwnerDocument.CreateAttribute("layername");
         oAttr.Value = oDataset.Name;
         oDisplayMapNode.Attributes.Append(oAttr);

         oAttr = oXmlNode.OwnerDocument.CreateAttribute("datasetname");
         oAttr.Value = oDataset.Name;
         oDisplayMapNode.Attributes.Append(oAttr);

         oAttr = oXmlNode.OwnerDocument.CreateAttribute("layertitle");
         oAttr.Value = oDataset.Title;
         oDisplayMapNode.Attributes.Append(oAttr);

         oAttr = oXmlNode.OwnerDocument.CreateAttribute("datasettype");
         oAttr.Value = oDataset.Type;
         oDisplayMapNode.Attributes.Append(oAttr);

         oAttr = oXmlNode.OwnerDocument.CreateAttribute("edition");
         oAttr.Value = oDataset.Edition;
         oDisplayMapNode.Attributes.Append(oAttr);

         oAttr = oXmlNode.OwnerDocument.CreateAttribute("hierarchy");
         oAttr.Value = oDataset.Hierarchy;
         oDisplayMapNode.Attributes.Append(oAttr);

         oAttr = oXmlNode.OwnerDocument.CreateAttribute("minx");
         oAttr.Value = oDataset.Boundary.MinX.ToString();
         oDisplayMapNode.Attributes.Append(oAttr);

         oAttr = oXmlNode.OwnerDocument.CreateAttribute("miny");
         oAttr.Value = oDataset.Boundary.MinY.ToString();
         oDisplayMapNode.Attributes.Append(oAttr);

         oAttr = oXmlNode.OwnerDocument.CreateAttribute("maxx");
         oAttr.Value = oDataset.Boundary.MaxX.ToString();
         oDisplayMapNode.Attributes.Append(oAttr);

         oAttr = oXmlNode.OwnerDocument.CreateAttribute("maxy");
         oAttr.Value = oDataset.Boundary.MaxY.ToString();
         oDisplayMapNode.Attributes.Append(oAttr);

         oAttr = oXmlNode.OwnerDocument.CreateAttribute("height");
         oAttr.Value = "0";
         oDisplayMapNode.Attributes.Append(oAttr);

         oAttr = oXmlNode.OwnerDocument.CreateAttribute("size");
         oAttr.Value = "256";
         oDisplayMapNode.Attributes.Append(oAttr);

         oAttr = oXmlNode.OwnerDocument.CreateAttribute("levels");
         oAttr.Value = DappleUtils.Levels(oDapCommand, oDataset).ToString();
         oDisplayMapNode.Attributes.Append(oAttr);

         oAttr = oXmlNode.OwnerDocument.CreateAttribute("levelzerotilesize");
         oAttr.Value = DappleUtils.LevelZeroTileSize(oDataset).ToString();
         oDisplayMapNode.Attributes.Append(oAttr);

         oXmlNode.AppendChild(oDisplayMapNode);
      }
   }
}
