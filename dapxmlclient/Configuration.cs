using System;
using System.Xml;
using System.Collections;

namespace Geosoft.Dap
{
   /// <summary>
   /// Handle the retrieving of anything to do with coordinate system from the Dap server configuration meta
   /// </summary>
   public class Configuration
   {
      #region Member Functions
      /// <summary>
      /// Index Area Element
      /// </summary>
      protected XmlNode    m_oIndexArea;
      /// <summary>
      /// AOI List Element
      /// </summary>
      protected XmlNode    m_oAOIList;
      /// <summary>
      /// Identification Element
      /// </summary>
      protected XmlNode    m_oIdentification;
      #endregion

      #region Constructor
      /// <summary>
      /// Default constructor
      /// </summary>
      /// <param name="hMeta"></param>
      public Configuration(XmlNode hMeta)
      {
         XmlNode  oGeosoftNode;
         XmlNode  oCoreNode;
         XmlNode  oDapNode;
         XmlNode  oConfigurationNode;
         XmlNode  oResourcesNode;

         if (hMeta == null) return;

         oGeosoftNode = hMeta.FirstChild;
         if (oGeosoftNode == null) throw new ArgumentException("Invalid configuration xml");

         oCoreNode = oGeosoftNode.FirstChild;
         if (oCoreNode == null) throw new ArgumentException("Invalid configuration xml");

         oDapNode = oCoreNode.FirstChild;
         if (oDapNode == null) throw new ArgumentException("Invalid configuration xml");

         oConfigurationNode = oDapNode.FirstChild;
         if (oConfigurationNode == null) throw new ArgumentException("Invalid configuration xml");

         oResourcesNode = oConfigurationNode.FirstChild;
         if (oResourcesNode == null) throw new ArgumentException("Invalid configuration xml");

         m_oIndexArea = oResourcesNode.FirstChild;
         if (m_oIndexArea == null) throw new ArgumentException("Invalid configuration xml");
         
         m_oAOIList = m_oIndexArea.NextSibling;
         if (m_oAOIList == null) throw new ArgumentException("Invalid configuration xml");

         m_oIdentification = oResourcesNode.NextSibling;
         if (m_oIdentification == null) throw new ArgumentException("Invalid configuration xml");
      }
      #endregion

      #region Member Functions
      /// <summary>
      /// Get the name of the server
      /// </summary>
      /// <returns></returns>
      public string  GetServerName()
      {     
         XmlNode  oAttr;

         if (m_oIdentification == null) return String.Empty;

         foreach (XmlNode oAttrNode in m_oIdentification.ChildNodes)
         {
            oAttr = oAttrNode.Attributes.GetNamedItem(Geosoft.Dap.Xml.Common.Constant.Attribute.NAME_ATTR);
            if (oAttr == null || oAttr.Value != "ShortName") continue;
               
            oAttr = oAttrNode.Attributes.GetNamedItem(Geosoft.Dap.Xml.Common.Constant.Attribute.VALUE_ATTR);
            if (oAttr == null) break;
            return oAttr.Value;            
         }
         
         return string.Empty;
      }

      /// <summary>
      /// Get the descriptive name of the server
      /// </summary>
      /// <returns></returns>
      public string  GetServerDescriptiveName()
      {         
         XmlNode  oAttr;

         if (m_oIdentification == null) return String.Empty;

         foreach (XmlNode oAttrNode in m_oIdentification.ChildNodes)
         {
            oAttr = oAttrNode.Attributes.GetNamedItem(Geosoft.Dap.Xml.Common.Constant.Attribute.NAME_ATTR);
            if (oAttr == null || oAttr.Value != "DescriptiveName") continue;
               
            oAttr = oAttrNode.Attributes.GetNamedItem(Geosoft.Dap.Xml.Common.Constant.Attribute.VALUE_ATTR);
            if (oAttr == null) break;
            return oAttr.Value;            
         }
         
         return string.Empty;
      }

      /// <summary>
      /// Get the version of the server
      /// </summary>
      /// <returns></returns>
      public void  GetVersion(out Int32 iMajorVersion, out Int32 iMinorVersion)
      {         
         XmlNode  oAttr;

         iMajorVersion = 6;
         iMinorVersion = 2;

         if (m_oIdentification == null) return;

         foreach (XmlNode oAttrNode in m_oIdentification.ChildNodes)
         {
            oAttr = oAttrNode.Attributes.GetNamedItem(Geosoft.Dap.Xml.Common.Constant.Attribute.NAME_ATTR);
            if (oAttr == null || oAttr.Value != "Version") continue;
               
            oAttr = oAttrNode.Attributes.GetNamedItem(Geosoft.Dap.Xml.Common.Constant.Attribute.VALUE_ATTR);
            if (oAttr == null) break;
            
            Int32 iIndex = oAttr.Value.IndexOf('.');

            if (iIndex != -1)
            {
               string szMajor = oAttr.Value.Substring(0, iIndex);
               string szMinor = oAttr.Value.Substring(iIndex + 1);
            
               iMajorVersion = Int32.Parse(szMajor);
               iMinorVersion = Int32.Parse(szMinor);
            }
         }
      }
                

      /// <summary>
      /// Get the edition of the server
      /// </summary>
      /// <returns></returns>
      public void  GetEdition(out string strEdition)
      {         
         strEdition = string.Empty;

         XmlNode  oAttr;

         if (m_oIdentification == null) return;

         foreach (XmlNode oAttrNode in m_oIdentification.ChildNodes)
         {
            oAttr = oAttrNode.Attributes.GetNamedItem(Geosoft.Dap.Xml.Common.Constant.Attribute.NAME_ATTR);
            if (oAttr == null || oAttr.Value != "Edition") continue;
               
            oAttr = oAttrNode.Attributes.GetNamedItem(Geosoft.Dap.Xml.Common.Constant.Attribute.VALUE_ATTR);
            if (oAttr == null) break;
            strEdition = oAttr.Value;            
         }
      }       

      /// <summary>
      /// Get the list of areas' of interest
      /// </summary>
      /// <returns></returns>
      public ArrayList  GetAreaList()
      {         
         ArrayList   oAOIList = new ArrayList();
         XmlNode     oTableNode;
         XmlNode     oAttr;

         if (m_oAOIList == null) return oAOIList;

         oTableNode = m_oAOIList.FirstChild;
         if (oTableNode == null) return oAOIList;

         foreach (XmlNode oItemNode in oTableNode.ChildNodes)
         {
            oAttr = oItemNode.Attributes.GetNamedItem(Geosoft.Dap.Xml.Common.Constant.Attribute.NAME_ATTR);
            oAOIList.Add(oAttr.Value);
         }

         oAOIList.Sort();
         return oAOIList;
      }

      /// <summary>
      /// Get the bounding box for a particular area of interest
      /// </summary>
      /// <param name="szName"></param>
      /// <param name="dMaxX"></param>
      /// <param name="dMaxY"></param>
      /// <param name="dMinX"></param>
      /// <param name="dMinY"></param>
      /// <param name="szCoordinateSystem"></param>
      /// <returns></returns>
      public bool GetBoundingBox(string szName, out double dMaxX, out double dMaxY, out double dMinX, out double dMinY, out string szCoordinateSystem)
      {         
         ArrayList   oAOIList = new ArrayList();
         XmlNode     oTableNode;
         XmlNode     oAttr;
         
         szCoordinateSystem = "";
         dMaxX = Double.MinValue;
         dMaxY = Double.MinValue;
         dMinX = Double.MinValue;
         dMinY = Double.MinValue;

         

         if (m_oAOIList == null) return false;

         oTableNode = m_oAOIList.FirstChild;
         if (oTableNode == null) return false;

         foreach (XmlNode oItemNode in oTableNode.ChildNodes)
         {
            oAttr = oItemNode.Attributes.GetNamedItem(Geosoft.Dap.Xml.Common.Constant.Attribute.NAME_ATTR);
            if (oAttr == null || oAttr.Value != szName) continue;

            foreach (XmlNode oAttrNode in oItemNode.ChildNodes)
            {
               XmlNode hNameAttr = oAttrNode.Attributes.GetNamedItem(Geosoft.Dap.Xml.Common.Constant.Attribute.NAME_ATTR);
               XmlNode hValueAttr = oAttrNode.Attributes.GetNamedItem(Geosoft.Dap.Xml.Common.Constant.Attribute.VALUE_ATTR);

               if (hNameAttr == null || hNameAttr.Value == null || hValueAttr == null || hValueAttr.Value == null) return false;

               switch (hNameAttr.Value)
               {
                  case "Coordinate System":
                     szCoordinateSystem = hValueAttr.Value;
                     break;
                  case "Maximum X":
                     dMaxX = Double.Parse(hValueAttr.Value, System.Globalization.CultureInfo.InvariantCulture);
                     break;
                  case "Maximum Y":
                     dMaxY = Double.Parse(hValueAttr.Value, System.Globalization.CultureInfo.InvariantCulture);
                     break;
                  case "Minimum X":
                     dMinX = Double.Parse(hValueAttr.Value, System.Globalization.CultureInfo.InvariantCulture);
                     break;
                  case "Minimum Y":
                     dMinY = Double.Parse(hValueAttr.Value, System.Globalization.CultureInfo.InvariantCulture);
                     break;
               }
            }
         }         
         return true;
      }

      /// <summary>
      /// Get the bounding box for the index area
      /// </summary>
      /// <param name="dMaxX"></param>
      /// <param name="dMaxY"></param>
      /// <param name="dMinX"></param>
      /// <param name="dMinY"></param>
      /// <param name="szProjection"></param>
      /// <param name="szDatum"></param>
      /// <param name="szMethod"></param>
      /// <param name="szUnit"></param>
      /// <param name="szLocalDatum"></param>
      /// <returns></returns>
      public bool GetDefaultAOI(out double dMaxX, out double dMaxY, out double dMinX, out double dMinY, out string szProjection, out string szDatum, out string szMethod, out string szUnit, out string szLocalDatum)
      {         
         szProjection = string.Empty;
         szDatum = string.Empty;
         szMethod = string.Empty;
         szUnit = string.Empty;
         szLocalDatum = string.Empty;
         dMaxX = Double.MinValue;
         dMaxY = Double.MinValue;
         dMinX = Double.MinValue;
         dMinY = Double.MinValue;

         if (m_oIndexArea == null) return false;         


         foreach (XmlNode hAttr in m_oIndexArea.ChildNodes)
         {
            XmlNode hValueAttr;
            XmlNode hNameAttr;

            if (String.Compare(hAttr.Name,Geosoft.Dap.Xml.Common.Constant.Tag.CLASS_TAG, true) == 0)
            {
               foreach (XmlNode hChild in hAttr.ChildNodes)
               {
                  hNameAttr = hChild.Attributes.GetNamedItem(Geosoft.Dap.Xml.Common.Constant.Attribute.NAME_ATTR);
                  hValueAttr = hChild.Attributes.GetNamedItem(Geosoft.Dap.Xml.Common.Constant.Attribute.VALUE_ATTR);
                  if (hNameAttr == null || hNameAttr.Value == null || hValueAttr == null || hValueAttr.Value == null) return false;
                  
                  switch (hNameAttr.Value)
                  {
                     case "Projection":
                        szProjection= hValueAttr.Value;
                        break;
                     case "Datum":
                        szDatum= hValueAttr.Value;
                        break;
                     case "Method":
                        szMethod= hValueAttr.Value;
                        break;
                     case "Unit":
                        szUnit= hValueAttr.Value;
                        break;
                     case "Local Datum":
                        szLocalDatum= hValueAttr.Value;
                        break;
                  }
               }
            } 
            else 
            {
               hNameAttr = hAttr.Attributes.GetNamedItem(Geosoft.Dap.Xml.Common.Constant.Attribute.NAME_ATTR);
               hValueAttr = hAttr.Attributes.GetNamedItem(Geosoft.Dap.Xml.Common.Constant.Attribute.VALUE_ATTR);
               if (hNameAttr == null || hNameAttr.Value == null || hValueAttr == null || hValueAttr.Value == null) return false;

               switch (hNameAttr.Value)
               {
                  case "Maximum X":
                     dMaxX = Double.Parse(hValueAttr.Value, System.Globalization.CultureInfo.InvariantCulture);
                     break;
                  case "Maximum Y":
                     dMaxY = Double.Parse(hValueAttr.Value, System.Globalization.CultureInfo.InvariantCulture);
                     break;
                  case "Minimum X":
                     dMinX = Double.Parse(hValueAttr.Value, System.Globalization.CultureInfo.InvariantCulture);
                     break;
                  case "Minimum Y":
                     dMinY = Double.Parse(hValueAttr.Value, System.Globalization.CultureInfo.InvariantCulture);
                     break;
               }
            }
         }
         return true;
      }

      /// <summary>
      /// Parse coordinate system into its components
      /// </summary>
      /// <param name="szCoordinateSystem"></param>
      /// <param name="strDatum"></param>
      /// <param name="strProjection"></param>
      /// <param name="strUnits"></param>
      /// <param name="strLocalDatum"></param>      
      public static void ParseCoordinateSystem(string szCoordinateSystem, out string strDatum, out string strProjection, out string strUnits, out string strLocalDatum)
      {
         string []strParts = szCoordinateSystem.Split('/');

         strDatum = strParts[0].Trim();
         strProjection = string.Empty;
         strLocalDatum = string.Empty;
         strUnits = string.Empty;

         if (strParts.Length > 1)
            strProjection = strParts[1].Trim();

         if (strParts.Length > 2)
            strUnits = strParts[2].Trim();

         if (strParts.Length > 3)
            strLocalDatum = strParts[3].Trim();
      }

      /// <summary>
      /// Get wether this server requires an encrypted stream
      /// </summary>
      /// <returns></returns>
      public bool  bSecure()
      {         
         XmlNode  oAttr;

         if (m_oIdentification == null) return false;

         foreach (XmlNode oAttrNode in m_oIdentification.ChildNodes)
         {
            oAttr = oAttrNode.Attributes.GetNamedItem(Geosoft.Dap.Xml.Common.Constant.Attribute.NAME_ATTR);
            if (oAttr == null || oAttr.Value != "Secure") continue;
               
            oAttr = oAttrNode.Attributes.GetNamedItem(Geosoft.Dap.Xml.Common.Constant.Attribute.VALUE_ATTR);
            if (oAttr == null) break;
            return Convert.ToBoolean(oAttr.Value);
         }
         return false;
      }

      /// <summary>
      /// Get wether this server requires login credentials
      /// </summary>
      /// <returns></returns>
      public bool  bLogin()
      {         
         XmlNode  oAttr;

         if (m_oIdentification == null) return false;

         foreach (XmlNode oAttrNode in m_oIdentification.ChildNodes)
         {
            oAttr = oAttrNode.Attributes.GetNamedItem(Geosoft.Dap.Xml.Common.Constant.Attribute.NAME_ATTR);
            if (oAttr == null || oAttr.Value != "Verify") continue;
               
            oAttr = oAttrNode.Attributes.GetNamedItem(Geosoft.Dap.Xml.Common.Constant.Attribute.VALUE_ATTR);
            if (oAttr == null) break;
            return Convert.ToBoolean(oAttr.Value);
         }         
         return false;
      }
      #endregion
   }
}
