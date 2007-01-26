using System;
using System.Collections.Generic;
using System.Text;

namespace Geosoft.Dap.Common
{
   /// <summary>
   /// Export a result set to kml
   /// </summary>
   public class GoogleEarthExport
   {
      /// <summary>
      /// Get the kml for this list of datasets
      /// </summary>
      /// <param name="strWmsUrl"></param>
      /// <param name="oDatasets"></param>
      /// <param name="oSelectedDatasets"></param>
      /// <returns></returns>
      public static System.Xml.XmlDocument GetKML(string strWmsUrl, System.Collections.SortedList oDatasets, System.Collections.ArrayList oSelectedDatasets)
      {
         System.Xml.XmlDocument oOutputXml = new System.Xml.XmlDocument();

         oOutputXml.AppendChild(oOutputXml.CreateXmlDeclaration("1.0", "UTF-8", string.Empty));

         System.Xml.XmlElement oKml = oOutputXml.CreateElement("kml", "http://earth.google.com/kml/2.1");
         oOutputXml.AppendChild(oKml);

         System.Xml.XmlElement oDocument = oOutputXml.CreateElement("Document", "http://earth.google.com/kml/2.1");
         System.Xml.XmlAttribute oAttr = oOutputXml.CreateAttribute("id");
         oAttr.Value = "Geosoft";
         oDocument.Attributes.Append(oAttr);
         oKml.AppendChild(oDocument);

         System.Xml.XmlElement oName = oOutputXml.CreateElement("name", "http://earth.google.com/kml/2.1");
         oName.InnerText = "Geosoft Catalog";
         oDocument.AppendChild(oName);

         System.Xml.XmlElement oVisibility = oOutputXml.CreateElement("visibility", "http://earth.google.com/kml/2.1");
         oVisibility.InnerText = "1";
         oDocument.AppendChild(oVisibility);

         System.Xml.XmlElement oOpen = oOutputXml.CreateElement("open", "http://earth.google.com/kml/2.1");
         oOpen.InnerText = "1";
         oDocument.AppendChild(oOpen);

         foreach (string strKey in oSelectedDatasets) {
            Dap.Common.DataSet oDataset = (Dap.Common.DataSet)oDatasets[strKey];

            if (oDataset != null)
               OutputDataset(strWmsUrl, oDocument, oDataset);
         }
         return oOutputXml;
      }

      /// <summary>
      /// Find the folder node 
      /// </summary>
      /// <param name="oParent"></param>
      /// <param name="strFolder"></param>
      /// <returns></returns>
      private static System.Xml.XmlNode FindFolder(System.Xml.XmlNode oParent, string strFolder)
      {
         System.Xml.XmlNode oFolder = null;

         foreach (System.Xml.XmlNode oChild in oParent.ChildNodes) {
            foreach (System.Xml.XmlNode oChildAttributes in oChild.ChildNodes) {
               if (oChildAttributes.Name == "name" && oChildAttributes.InnerText == strFolder) {
                  oFolder = oChild;
                  break;
               }
            }
            if (oFolder != null)
               break;
         }
         return oFolder;
      }

      /// <summary>
      /// Add this dataset into the kml document
      /// </summary>
      /// <param name="strWmsUrl"></param>
      /// <param name="oKmlNode"></param>
      /// <param name="oDataset"></param>
      private static void OutputDataset(string strWmsUrl, System.Xml.XmlNode oKmlNode, Dap.Common.DataSet oDataset)
      {
         string strHierarchy = oDataset.Hierarchy.Trim('/');
         System.Xml.XmlNode oCurFolder = oKmlNode;
         System.Xml.XmlNode oNextFolder;
         System.Xml.XmlElement oGroundOverlayNode;
         System.Xml.XmlElement oNameNode;
         System.Xml.XmlElement oOpenNode;
         System.Xml.XmlElement oIconNode;
         System.Xml.XmlElement oHRefNode;
         System.Xml.XmlElement oViewRefreshTimeNode;
         System.Xml.XmlElement oViewBoundScaleNode;
         System.Xml.XmlElement oViewFormatNode;
         System.Xml.XmlElement oViewRefreshModeNode;
         System.Xml.XmlElement oWGS84BoundingBoxNode;
         System.Xml.XmlElement oCNode;
         System.Xml.XmlElement oTitleNode;
         System.Xml.XmlElement oVisibleNode;
         System.Xml.XmlAttribute oOutputAttr;

         // --- create the folder structure ---

         string []oFolders = strHierarchy.Split('/');
         foreach (string strFolderName in oFolders) {
            oNextFolder = FindFolder(oCurFolder, strFolderName);

            if (oNextFolder == null) {
               
               oNextFolder = oKmlNode.OwnerDocument.CreateElement("Folder", "http://earth.google.com/kml/2.1");

               oTitleNode = oKmlNode.OwnerDocument.CreateElement("name", "http://earth.google.com/kml/2.1");
               oTitleNode.InnerText = strFolderName;
               oNextFolder.AppendChild(oTitleNode);

               oVisibleNode = oKmlNode.OwnerDocument.CreateElement("visibility", "http://earth.google.com/kml/2.1");
               oVisibleNode.InnerText = "1";
               oNextFolder.AppendChild(oVisibleNode);

               oOpenNode = oKmlNode.OwnerDocument.CreateElement("open", "http://earth.google.com/kml/2.1");
               oOpenNode.InnerText = "0";
               oNextFolder.AppendChild(oOpenNode);
               oCurFolder.AppendChild(oNextFolder);
            }
            oCurFolder = oNextFolder;
         }         

         // --- write out the dataset node ---

         oGroundOverlayNode = oKmlNode.OwnerDocument.CreateElement("GroundOverlay", "http://earth.google.com/kml/2.1");
         oOutputAttr = oKmlNode.OwnerDocument.CreateAttribute("id");
         oOutputAttr.Value = oDataset.Name;
         oGroundOverlayNode.Attributes.Append(oOutputAttr);

         oNameNode = oKmlNode.OwnerDocument.CreateElement("name", "http://earth.google.com/kml/2.1");
         oNameNode.InnerText = oDataset.Title;
         oGroundOverlayNode.AppendChild(oNameNode);

         oOpenNode = oKmlNode.OwnerDocument.CreateElement("open", "http://earth.google.com/kml/2.1");
         oOpenNode.InnerText = "0";
         oGroundOverlayNode.AppendChild(oOpenNode);

         oIconNode = oKmlNode.OwnerDocument.CreateElement("Icon", "http://earth.google.com/kml/2.1");
         oGroundOverlayNode.AppendChild(oIconNode);

         oHRefNode = oKmlNode.OwnerDocument.CreateElement("href", "http://earth.google.com/kml/2.1");
         oHRefNode.InnerText = strWmsUrl + "?Request=googlemap&LAYERID=" + oDataset.Name;
         oIconNode.AppendChild(oHRefNode);

         oViewRefreshModeNode = oKmlNode.OwnerDocument.CreateElement("viewRefreshMode", "http://earth.google.com/kml/2.1");
         oViewRefreshModeNode.InnerText = "onStop";
         oIconNode.AppendChild(oViewRefreshModeNode);

         oViewRefreshTimeNode = oKmlNode.OwnerDocument.CreateElement("viewRefreshTime", "http://earth.google.com/kml/2.1");
         oViewRefreshTimeNode.InnerText = "0.5";
         oIconNode.AppendChild(oViewRefreshTimeNode);

         oViewBoundScaleNode = oKmlNode.OwnerDocument.CreateElement("viewBoundScale", "http://earth.google.com/kml/2.1");
         oViewBoundScaleNode.InnerText = "1.2";
         oIconNode.AppendChild(oViewBoundScaleNode);

         oViewFormatNode = oKmlNode.OwnerDocument.CreateElement("viewFormat", "http://earth.google.com/kml/2.1");
         oViewFormatNode.InnerText = "BBOX=[bboxWest],[bboxSouth],[bboxEast],[bboxNorth]";
         oIconNode.AppendChild(oViewFormatNode);

         oWGS84BoundingBoxNode = oKmlNode.OwnerDocument.CreateElement("LatLonBox", "http://earth.google.com/kml/2.1");
         oGroundOverlayNode.AppendChild(oWGS84BoundingBoxNode);

         oCNode = oKmlNode.OwnerDocument.CreateElement("north", "http://earth.google.com/kml/2.1");
         oCNode.InnerText = oDataset.Boundary.MaxY.ToString();
         oWGS84BoundingBoxNode.AppendChild(oCNode);

         oCNode = oKmlNode.OwnerDocument.CreateElement("south", "http://earth.google.com/kml/2.1");
         oCNode.InnerText = oDataset.Boundary.MinY.ToString();
         oWGS84BoundingBoxNode.AppendChild(oCNode);

         oCNode = oKmlNode.OwnerDocument.CreateElement("east", "http://earth.google.com/kml/2.1");
         oCNode.InnerText = oDataset.Boundary.MaxX.ToString();
         oWGS84BoundingBoxNode.AppendChild(oCNode);

         oCNode = oKmlNode.OwnerDocument.CreateElement("west", "http://earth.google.com/kml/2.1");
         oCNode.InnerText = oDataset.Boundary.MinX.ToString();
         oWGS84BoundingBoxNode.AppendChild(oCNode);

         oCurFolder.AppendChild(oGroundOverlayNode);

      }
   }
}
