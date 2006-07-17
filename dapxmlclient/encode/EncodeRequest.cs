using System;
using System.Collections;
using Geosoft.Dap.Common;
using Geosoft.Dap.Xml.Common;

namespace Geosoft.Dap.Xml
{
   /// <summary>
   /// Encode a request from the client application into GeosoftXML
   /// </summary>
   public class EncodeRequest
   {    
      #region Member Variables
      private Command.Version  m_eVersion;

      private string           m_strUserName = string.Empty;
      private string           m_strPassword = string.Empty;
      #endregion

      #region Properties
      /// <summary>
      /// Get/Set the version to talk to the dap server with
      /// </summary>
      public Command.Version Version
      { 
         get { return m_eVersion; }
         set { m_eVersion = value; }
      }

      /// <summary>
      /// Get/set the user name
      /// </summary>
      public string UserName
      { 
         get { return m_strUserName; }
         set { m_strUserName = value; }
      }

      /// <summary>
      /// Get/Set the password
      /// </summary>
      public string Password
      {
         get { return m_strPassword; }
         set { m_strPassword = value; }
      }
      #endregion

      #region Constructor
      /// <summary>
      /// Default constructor
      /// </summary>
      /// <param name="eVersion"></param>
      public EncodeRequest(Command.Version eVersion)
      {
         m_eVersion = eVersion;
      }

      /// <summary>
      /// Default constructor
      /// </summary>
      /// <param name="eVersion"></param>
      /// <param name="strUserName"></param>
      /// <param name="strPassword"></param>
      public EncodeRequest(Command.Version eVersion, string strUserName, string strPassword)
      {
         m_eVersion = eVersion;
         m_strUserName = strUserName;
         m_strPassword = strPassword;
      }

      #endregion

      #region Private Methods
      /// <summary>
      /// Create a new request
      /// </summary>
      /// <returns></returns>
      private System.Xml.XmlElement CreateRequest(string strHandle, string strCommand)
      {
         System.Xml.XmlDocument	oXmlRequest = new System.Xml.XmlDocument();			         
         System.Xml.XmlElement	oGeosoftNode = oXmlRequest.CreateElement(Constant.Tag.GEO_XML_TAG);
         System.Xml.XmlElement	oRequestNode = oXmlRequest.CreateElement(Constant.Tag.REQUEST_TAG);
         System.Xml.XmlElement	oCommandNode = oXmlRequest.CreateElement(strCommand);
          
         // --- set the version attribute ---

         oGeosoftNode.SetAttribute(Constant.Attribute.VERSION_ATTR, Constant.XmlVersion[Convert.ToInt32(m_eVersion)]);
         oGeosoftNode.AppendChild(oRequestNode);
         
         
         // --- add the command to the request ---
         
         oRequestNode.AppendChild(oCommandNode);
         SetHandle(oCommandNode, strHandle);

         
         // --- add the user name/password if available ---

         if (m_eVersion == Command.Version.GEOSOFT_XML_1_1)
         {
            System.Xml.XmlAttribute oAttr;

            oAttr = oXmlRequest.CreateAttribute(Constant.Attribute.USER_NAME_ATTR);
            oAttr.Value = m_strUserName;
            oCommandNode.Attributes.Append(oAttr);

            oAttr = oXmlRequest.CreateAttribute(Constant.Attribute.PASSWORD_ATTR);
            oAttr.Value = m_strPassword;
            oCommandNode.Attributes.Append(oAttr);
         }


         // --- add this request to the document ---

         oXmlRequest.AppendChild(oGeosoftNode);

         return oCommandNode;
      }      

      /// <summary>
      /// Set the handle
      /// </summary>
      /// <param name="oNode"></param>
      /// <param name="strHandle"></param>
      private void SetHandle(System.Xml.XmlElement oNode, string strHandle)
      {
         if (strHandle != null && strHandle.Length != 0)
         {
            System.Xml.XmlAttribute hAttr;
            hAttr = oNode.OwnerDocument.CreateAttribute(Constant.Attribute.HANDLE_ATTR);
            hAttr.Value = strHandle;

            oNode.SetAttributeNode(hAttr);
         }
      }

      /// <summary>
      /// Encode COORDINATE_SYSTEM element
      /// </summary>
      /// <param name="oParent">The parent node</param>
      /// <param name="oCoordinateSystem">The coordinate system to encode</param>
      /// <returns>COORDINATE_SYSTEM element</returns>
      private System.Xml.XmlNode AddCoordinateSystem( System.Xml.XmlNode   oParent, 
                                                      CoordinateSystem     oCoordinateSystem )
      {
         System.Xml.XmlElement	hNode = oParent.OwnerDocument.CreateElement(Constant.Tag.COORDINATE_SYSTEM_TAG);

         if (oCoordinateSystem.ProjectionType == CoordinateSystem.ProjectionTypes.STANDARD)
         {
            if( oCoordinateSystem.Datum != null && oCoordinateSystem.Datum.Length != 0 ) 
            {
               System.Xml.XmlAttribute dataSetAttr = oParent.OwnerDocument.CreateAttribute( Geosoft.Dap.Xml.Common.Constant.Attribute.DATUM_ATTR );
               dataSetAttr.Value = oCoordinateSystem.Datum;				
               hNode.SetAttributeNode( dataSetAttr );
            }

            if( oCoordinateSystem.Method != null && oCoordinateSystem.Method.Length != 0 ) 
            {
               System.Xml.XmlAttribute dataSetAttr = oParent.OwnerDocument.CreateAttribute( Geosoft.Dap.Xml.Common.Constant.Attribute.PROJECTION_ATTR );
               dataSetAttr.Value = oCoordinateSystem.Method;				
               hNode.SetAttributeNode( dataSetAttr );
            }

            if( oCoordinateSystem.Units != null && oCoordinateSystem.Units.Length != 0 ) 
            {
               System.Xml.XmlAttribute dataSetAttr = oParent.OwnerDocument.CreateAttribute( Geosoft.Dap.Xml.Common.Constant.Attribute.UNITS_ATTR );
               dataSetAttr.Value = oCoordinateSystem.Units;				
               hNode.SetAttributeNode( dataSetAttr );
            }

            if( oCoordinateSystem.LocalDatum != null && oCoordinateSystem.LocalDatum.Length != 0 ) 
            {
               System.Xml.XmlAttribute dataSetAttr = oParent.OwnerDocument.CreateAttribute( Geosoft.Dap.Xml.Common.Constant.Attribute.LOCAL_DATUM_ATTR );
               dataSetAttr.Value = oCoordinateSystem.LocalDatum;				
               hNode.SetAttributeNode( dataSetAttr );
            }
         } 
         else if (oCoordinateSystem.ProjectionType == CoordinateSystem.ProjectionTypes.ESRI)
         {
            if( oCoordinateSystem.Esri != null && oCoordinateSystem.Esri.Length != 0 ) 
            {
               System.Xml.XmlAttribute dataSetAttr = oParent.OwnerDocument.CreateAttribute( Geosoft.Dap.Xml.Common.Constant.Attribute.ESRI_ATTR );
               dataSetAttr.Value = oCoordinateSystem.Esri;				
               hNode.SetAttributeNode( dataSetAttr );
            }
         }
         oParent.AppendChild(hNode);

         return hNode;
      }

      /// <summary>
      /// Encode BOUNDING_BOX element
      /// </summary>
      /// <param name="oParent">The parent node</param>
      /// <param name="hBoundingBox">The bounding box to encode</param>
      /// <returns>BOUNDNG_BOX element</returns>
      private System.Xml.XmlNode AddBoundingBox( System.Xml.XmlElement  oParent, 
                                                 BoundingBox            hBoundingBox )
      {

         System.Xml.XmlAttribute hAttr;
         System.Xml.XmlElement	hNode = oParent.OwnerDocument.CreateElement(Constant.Tag.BOUNDING_BOX_TAG);

         hAttr = oParent.OwnerDocument.CreateAttribute( Constant.Attribute.MAX_X_ATTR );
         hAttr.Value = hBoundingBox.MaxX.ToString(System.Globalization.CultureInfo.InvariantCulture);				
         hNode.SetAttributeNode( hAttr );
						
         hAttr = oParent.OwnerDocument.CreateAttribute( Constant.Attribute.MAX_Y_ATTR );
         hAttr.Value = hBoundingBox.MaxY.ToString(System.Globalization.CultureInfo.InvariantCulture);				
         hNode.SetAttributeNode( hAttr );
			
         hAttr = oParent.OwnerDocument.CreateAttribute( Constant.Attribute.MIN_X_ATTR );
         hAttr.Value = hBoundingBox.MinX.ToString(System.Globalization.CultureInfo.InvariantCulture);				
         hNode.SetAttributeNode( hAttr );
			
         hAttr = oParent.OwnerDocument.CreateAttribute( Constant.Attribute.MIN_Y_ATTR );
         hAttr.Value = hBoundingBox.MinY.ToString(System.Globalization.CultureInfo.InvariantCulture);				
         hNode.SetAttributeNode( hAttr );
			
         oParent.AppendChild(hNode);
         return hNode;
      }
      #endregion

      /// <summary>
      /// Encode the request to see if this user/password is valid on the Dap server
      /// </summary>
      /// <param name="szHandle">The handle which uniquly identifies this request/response pair</param>
      /// <param name="strUserName">The user name</param>
      /// <param name="strPassword">The password</param>
      /// <returns>The GeosoftXML request</returns>
      public System.Xml.XmlDocument AuthenticateUser(string szHandle, string strUserName, string strPassword)
      {
         System.Xml.XmlElement	hAuthenticateNode = CreateRequest(szHandle, Constant.Tag.AUTHENTICATE_TAG);
         
         System.Xml.XmlAttribute oAttr;

         oAttr = hAuthenticateNode.OwnerDocument.CreateAttribute(Constant.Attribute.USER_NAME_ATTR);
         oAttr.Value = strUserName;
         hAuthenticateNode.Attributes.Append(oAttr);

         oAttr = hAuthenticateNode.OwnerDocument.CreateAttribute(Constant.Attribute.PASSWORD_ATTR);
         oAttr.Value = strPassword;
         hAuthenticateNode.Attributes.Append(oAttr);

         return hAuthenticateNode.OwnerDocument;
      }

      /// <summary>
      /// Encode the request to discover the configuration of a Dap server
      /// </summary>
      /// <param name="szHandle">The handle which uniquly identifies this request/response pair</param>
      /// <returns>The GeosoftXML request</returns>
      public System.Xml.XmlDocument Configuration(string szHandle)
      {
         System.Xml.XmlElement	hConfigurationNode = CreateRequest(szHandle, Constant.Tag.CONFIGURATION_TAG);
         
         return hConfigurationNode.OwnerDocument;
      }

      /// <summary>
      /// Encode the request to discover the server configuration of a Dap server
      /// </summary>
      /// <param name="szHandle">The handle which uniquly identifies this request/response pair</param>
      /// <param name="strPassword">The administrator password</param>
      /// <returns>The GeosoftXML request</returns>
      public System.Xml.XmlDocument ServerConfiguration(string szHandle, string strPassword)
      {
         System.Xml.XmlAttribute hAttr;
         System.Xml.XmlElement	hConfigurationNode = CreateRequest(szHandle, Constant.Tag.SERVER_CONFIGURATION_TAG);

         if (strPassword != null && strPassword.Length > 0)
         {
            // --- Add attributes ---
         
            hAttr = hConfigurationNode.OwnerDocument.CreateAttribute(Constant.Attribute.PASSWORD_ATTR);
            hAttr.Value = strPassword;
            hConfigurationNode.SetAttributeNode(hAttr);
         }

         return hConfigurationNode.OwnerDocument;
      }

      /// <summary>
      /// Encode the request to update the server configuration of a Dap server
      /// </summary>
      /// <param name="szHandle">The handle which uniquly identifies this request/response pair</param>
      /// <param name="strPassword">The administrator password</param>
      /// <param name="oConfiguration">The configuration xml to append to this request</param>
      /// <returns>The GeosoftXML request</returns>
      public System.Xml.XmlDocument UpdateServerConfiguration(string szHandle, string strPassword, System.Xml.XmlNode oConfiguration)
      {
         System.Xml.XmlAttribute hAttr;
         System.Xml.XmlElement	hConfigurationNode = CreateRequest(szHandle, Constant.Tag.UPDATE_SERVER_CONFIGURATION_TAG);

         if (strPassword != null && strPassword.Length > 0)
         {
            // --- Add attributes ---
         
            hAttr = hConfigurationNode.OwnerDocument.CreateAttribute(Constant.Attribute.PASSWORD_ATTR);
            hAttr.Value = strPassword;
            hConfigurationNode.SetAttributeNode(hAttr);
         }

         hConfigurationNode.AppendChild(hConfigurationNode.OwnerDocument.ImportNode(oConfiguration, true));

         return hConfigurationNode.OwnerDocument;
      }

      /// <summary>
      /// Encode the request to discover the list of keywords of a Dap server
      /// </summary>
      /// <param name="szHandle">The handle which uniquly identifies this request/response pair</param>
      /// <returns>The GeosoftXML request</returns>
      public System.Xml.XmlDocument Keywords( string szHandle )
      {
         System.Xml.XmlElement	hKeywordsNode = CreateRequest(szHandle, Constant.Tag.KEYWORDS_TAG);

         return hKeywordsNode.OwnerDocument;
      }

      /// <summary>
      /// Encode the request to discover the capabilities of a Dap server
      /// </summary>
      /// <param name="szHandle">The handle which uniquly identifies this request/response pair</param>
      /// <returns>The GeosoftXML request</returns>
      public System.Xml.XmlDocument Capabilities( string szHandle )
      {
         System.Xml.XmlElement	hCapabilitiesNode = CreateRequest(szHandle, Constant.Tag.CAPABILITIES_TAG);

         return hCapabilitiesNode.OwnerDocument;
      }

      /// <summary>
      /// Encode the request to discover the properties of a Dap server
      /// </summary>
      /// <param name="szHandle">The handle which uniquly identifies this request/response pair</param>
      /// <returns>The GeosoftXML request</returns>
      public System.Xml.XmlDocument Properties( string szHandle )
      {
         System.Xml.XmlElement	hPropertiesNode = CreateRequest(szHandle, Constant.Tag.PROPERTIES_TAG);

         return hPropertiesNode.OwnerDocument;
      }

      /// <summary>
      /// Encode the request for the complete catalog from a Dap server
      /// </summary>
      /// <param name="szHandle">The handle which uniquly identifies this request/response pair</param>
      /// <returns>The GeosoftXML request</returns>
      public System.Xml.XmlDocument Catalog( string szHandle ) 
      {
         return Catalog( szHandle, false, null, 0, 0, null, null, 0 );
      }

      /// <summary>
      /// Encode the request for the catalog from a Dap server
      /// </summary>
      /// <param name="szHandle">The handle which uniquly identifies this request/response pair</param>
      /// <param name="bCount">Wether to return the catalog or just the number of items that match this query</param>
      /// <param name="szPath">An xpath string to limit the returned catalog</param>
      /// <param name="iStartIndex">The start index of the first dataset you wish returned</param>
      /// <param name="iMaxResults">The maximum number of datasets you wish returned</param>
      /// <param name="szKeywords">The keywords to filter datasets by.</param>
      /// <param name="hBoundingBox">The area that must intersect a dataset bounding box in order for it to be returned</param>
      /// <param name="iLevel">The level to recurse down to</param>
      /// <returns>The GeosoftXML request</returns>
      public System.Xml.XmlDocument Catalog( string szHandle, bool bCount, string szPath, int iStartIndex, int iMaxResults, string szKeywords, BoundingBox hBoundingBox, int iLevel)
      {
         
         // --- Create required nodes ---

         System.Xml.XmlAttribute hAttr;
         System.Xml.XmlElement	hGetCatalogNode = CreateRequest(szHandle, Constant.Tag.CATALOG_TAG);
			
         // --- Add attributes ---
         
         hAttr = hGetCatalogNode.OwnerDocument.CreateAttribute(Constant.Attribute.COUNT_ATTR);
         hAttr.Value = bCount.ToString();
         hGetCatalogNode.SetAttributeNode( hAttr );

         if (iStartIndex >= 0)
         {
            hAttr = hGetCatalogNode.OwnerDocument.CreateAttribute(Constant.Attribute.INDEX_ATTR);
            hAttr.Value = iStartIndex.ToString();
            hGetCatalogNode.SetAttributeNode( hAttr );
         }

         if (iMaxResults > 0)
         {
            hAttr = hGetCatalogNode.OwnerDocument.CreateAttribute( Constant.Attribute.MAX_RESULTS_ATTR );
            hAttr.Value = iMaxResults.ToString();
            hGetCatalogNode.SetAttributeNode( hAttr );
         }

         if (iLevel > 0)
         {
            hAttr = hGetCatalogNode.OwnerDocument.CreateAttribute( Constant.Attribute.LEVEL_ATTR );
            hAttr.Value = iLevel.ToString();
            hGetCatalogNode.SetAttributeNode( hAttr );
         }
			
         if (szKeywords != null && szKeywords.Length > 0)
         {
            hAttr = hGetCatalogNode.OwnerDocument.CreateAttribute( Constant.Attribute.KEYWORDS_ATTR );
            hAttr.Value = szKeywords;
            hGetCatalogNode.SetAttributeNode( hAttr );
         }

         if (hBoundingBox != null) 
         {           
            System.Xml.XmlNode   hBoundingBoxNode = AddBoundingBox(hGetCatalogNode, hBoundingBox);
            AddCoordinateSystem(hBoundingBoxNode, hBoundingBox.CoordinateSystem);
         }


         if (szPath != null && szPath.Length > 0) 
         {
            System.Xml.XmlElement	hCatalogNode = hGetCatalogNode.OwnerDocument.CreateElement(Constant.Tag.FILTER_TAG);
            hAttr = hGetCatalogNode.OwnerDocument.CreateAttribute( Constant.Attribute.PATH_ATTR );
            hAttr.Value = szPath;

            hGetCatalogNode.AppendChild( hCatalogNode );
            hCatalogNode.SetAttributeNode( hAttr );
         }
			
         return hGetCatalogNode.OwnerDocument;			
      }

      /// <summary>
      /// Encode the request to inform the dap server to refresh its catalog
      /// </summary>
      /// <param name="szHandle">The handle which uniquly identifies this request/response pair</param>
      /// <param name="strPassword">The administrator password</param>
      /// <returns>The GeosoftXML request</returns>
      public System.Xml.XmlDocument RefreshCatalog(string szHandle, string strPassword)
      {
         System.Xml.XmlAttribute hAttr;
         System.Xml.XmlElement	hCatalogNode = CreateRequest(szHandle, Constant.Tag.REFRESH_CATALOG_TAG);

         if (strPassword != null && strPassword.Length > 0)
         {
            // --- Add attributes ---
         
            hAttr = hCatalogNode.OwnerDocument.CreateAttribute(Constant.Attribute.PASSWORD_ATTR);
            hAttr.Value = strPassword;
            hCatalogNode.SetAttributeNode(hAttr);
         }

         return hCatalogNode.OwnerDocument;
      }

      /// <summary>
      /// Encode the request to get the catalog edition
      /// </summary>
      /// <param name="szHandle">The handle which uniquly identifies this request/response pair</param>
      /// <returns>The GeosoftXML request</returns>
      public System.Xml.XmlDocument CatalogEdition( string szHandle )
      {

         // --- Create required nodes ---
         
         System.Xml.XmlElement	hCatalogEditionNode = CreateRequest(szHandle, Constant.Tag.CATALOG_EDITION_TAG);
			
         return hCatalogEditionNode.OwnerDocument;
      }

      /// <summary>
      /// Encode the request to get the dataset edition
      /// </summary>
      /// <param name="szHandle">The handle which uniquly identifies this request/response pair</param>
      /// <param name="szName">The unique name of the dataset</param>
      /// <returns>The GeosoftXML request</returns>
      public System.Xml.XmlDocument DatasetEdition( string szHandle, string szName )
      {
         // --- Create required nodes ---

         System.Xml.XmlElement	hDatasetEditionNode = CreateRequest(szHandle, Constant.Tag.DATASET_EDITION_TAG);
         
         if (szName != null && szName.Length > 0)
         {
            System.Xml.XmlAttribute hAttr = hDatasetEditionNode.OwnerDocument.CreateAttribute( Constant.Attribute.NAME_ATTR );
            hAttr.Value = szName;
            hDatasetEditionNode.SetAttributeNode( hAttr );
         }
			
         return hDatasetEditionNode.OwnerDocument;			
      }

      /// <summary>
      /// Encode the request to get the meta data for a particular dataset
      /// </summary>
      /// <param name="szHandle">The handle which uniquly identifies this request/response pair</param>
      /// <param name="szDataSet">The unique dataset name</param>
      /// <returns>The GeosoftXML request</returns>
      public System.Xml.XmlDocument Metadata( string szHandle, string szDataSet )
      {
         // --- Create required nodes ---
         
         System.Xml.XmlElement	hGetMetaDataNode = CreateRequest(szHandle, Constant.Tag.METADATA_TAG);
         
         if (szDataSet != null && szDataSet.Length != 0) 
         {				
            System.Xml.XmlAttribute hAttr = hGetMetaDataNode.OwnerDocument.CreateAttribute( Constant.Attribute.NAME_ATTR );
            hAttr.Value = szDataSet;
				
            hGetMetaDataNode.SetAttributeNode( hAttr );
         }
         return hGetMetaDataNode.OwnerDocument;
      }

      /// <summary>
      /// Encode the request to get an image of a list of datasets
      /// </summary>
      /// <param name="szHandle">The handle which uniquly identifies this request/response pair</param>
      /// <param name="hFormat">The format of the image</param>
      /// <param name="hBoundingBox">The area of the iamge</param>
      /// <param name="hResolution">The hieght and width of the image</param>
      /// <param name="bBaseMap">Draw the base map</param>
      /// <param name="bIndexMap">Draw the index map</param>
      /// <param name="hItems">The list of unique dataset names to draw</param>
      /// <returns>The GeosoftXML request</returns>
      public System.Xml.XmlDocument Image(	string szHandle, 
                                             Format hFormat, 
                                             BoundingBox hBoundingBox, 
                                             Resolution hResolution,
                                             bool bBaseMap,
                                             bool bIndexMap,
                                             ArrayList hItems )
      {			
         
         // --- Create required nodes ---

         System.Xml.XmlAttribute hAttr;
         System.Xml.XmlElement	hGetImageNode = CreateRequest(szHandle, Constant.Tag.IMAGE_TAG);
         System.Xml.XmlElement	hFormatNode = hGetImageNode.OwnerDocument.CreateElement(Constant.Tag.FORMAT_TAG );         
         System.Xml.XmlElement	hResolutionNode = hGetImageNode.OwnerDocument.CreateElement(Constant.Tag.RESOLUTION_TAG );         
         System.Xml.XmlElement	hLayersNode = hGetImageNode.OwnerDocument.CreateElement(Constant.Tag.LAYERS_TAG );
         System.Xml.XmlNode	   hBoundingBoxNode;
         
         // --- Setup hierchy ---

         hGetImageNode.AppendChild(hFormatNode);
         hGetImageNode.AppendChild(hResolutionNode);			
         hGetImageNode.AppendChild(hLayersNode);

         hBoundingBoxNode = AddBoundingBox(hGetImageNode, hBoundingBox );
         AddCoordinateSystem(hBoundingBoxNode, hBoundingBox.CoordinateSystem );		

         if (hFormat.Type != null && hFormat.Type.Length != 0) 
         {
            hAttr = hGetImageNode.OwnerDocument.CreateAttribute( Constant.Attribute.TYPE_ATTR );
            hAttr.Value = hFormat.Type;
				
            hFormatNode.SetAttributeNode( hAttr );
         }

         if (hFormat.Transparent) 
         {
            hAttr = hGetImageNode.OwnerDocument.CreateAttribute( Constant.Attribute.TRANSPARENT_ATTR );
            hAttr.Value = "TRUE";				
            hFormatNode.SetAttributeNode( hAttr );
         }

         if (hFormat.Background != null && hFormat.Background.Length != 0) 
         {
            hAttr = hGetImageNode.OwnerDocument.CreateAttribute( Constant.Attribute.BACKGROUND_ATTR );
            hAttr.Value = hFormat.Background;				
            hFormatNode.SetAttributeNode( hAttr );
         }

         hAttr = hGetImageNode.OwnerDocument.CreateAttribute( Constant.Attribute.WIDTH_ATTR );
         hAttr.Value = hResolution.Width.ToString();				
         hResolutionNode.SetAttributeNode( hAttr );
			
         hAttr = hGetImageNode.OwnerDocument.CreateAttribute( Constant.Attribute.HEIGHT_ATTR );
         hAttr.Value = hResolution.Height.ToString();				
         hResolutionNode.SetAttributeNode( hAttr );

         hAttr = hGetImageNode.OwnerDocument.CreateAttribute( Constant.Attribute.BASE_MAP_ATTR );
         hAttr.Value = bBaseMap.ToString();				
         hLayersNode.SetAttributeNode( hAttr );

         hAttr = hGetImageNode.OwnerDocument.CreateAttribute( Constant.Attribute.INDEX_MAP_ATTR );
         hAttr.Value = bIndexMap.ToString();				
         hLayersNode.SetAttributeNode( hAttr );
			
         foreach (Object item in hItems) 
         {			
            System.Xml.XmlElement	hDataSetNode = hGetImageNode.OwnerDocument.CreateElement(Constant.Tag.DATASET_TAG);
            hAttr = hGetImageNode.OwnerDocument.CreateAttribute( Constant.Attribute.NAME_ATTR );
            hAttr.Value = item.ToString();				
            hDataSetNode.SetAttributeNode( hAttr );
            hLayersNode.AppendChild( hDataSetNode );
         }
			
         return hGetImageNode.OwnerDocument;
      }

      /// <summary>
      /// Encode the request to translate a list of coordinates from one coordinate system to another
      /// </summary>
      /// <param name="szHandle">The handle which uniquly identifies this request/response pair</param>
      /// <param name="hInputCoordinateSystem">The current coordinate system the points are in</param>
      /// <param name="hOutputCoordinateSystem">The destination coordiante system you wish the points to be translated to</param>
      /// <param name="hItems">The list of points</param>
      /// <returns>The GeosoftXML request</returns>
      public System.Xml.XmlDocument TranslateCoordinates(	string szHandle, 			
                                                            CoordinateSystem hInputCoordinateSystem,
                                                            CoordinateSystem hOutputCoordinateSystem,
                                                            System.Collections.ArrayList hItems )
      {
         
         // --- Create required Nodes ---

         System.Xml.XmlAttribute hAttr;
         System.Xml.XmlElement	hTranslateCoordinatesNode = CreateRequest(szHandle, Constant.Tag.TRANSLATE_COORDINATES_TAG);
         System.Xml.XmlElement	hInputNode = hTranslateCoordinatesNode.OwnerDocument.CreateElement(Constant.Tag.INPUT_TAG );
         System.Xml.XmlElement	hOutputNode = hTranslateCoordinatesNode.OwnerDocument.CreateElement(Constant.Tag.OUTPUT_TAG );
         
         
         // --- Setup hierchy ---
         
         hTranslateCoordinatesNode.AppendChild(hInputNode);
         hTranslateCoordinatesNode.AppendChild(hOutputNode);

         AddCoordinateSystem(hInputNode, hInputCoordinateSystem );
         AddCoordinateSystem(hOutputNode, hOutputCoordinateSystem );			

                 			        
         foreach (Point item in hItems) 
         {			            
            System.Xml.XmlElement	hPointElement = hTranslateCoordinatesNode.OwnerDocument.CreateElement(Constant.Tag.POINT_TAG );
				            
            hAttr = hTranslateCoordinatesNode.OwnerDocument.CreateAttribute( Constant.Attribute.X_ATTR );
            hAttr.Value = item.X.ToString(System.Globalization.CultureInfo.InvariantCulture);
            hPointElement.SetAttributeNode(hAttr);
            
            hAttr = hTranslateCoordinatesNode.OwnerDocument.CreateAttribute( Constant.Attribute.Y_ATTR );
            hAttr.Value = item.Y.ToString(System.Globalization.CultureInfo.InvariantCulture);
            hPointElement.SetAttributeNode(hAttr);
            
            hAttr = hTranslateCoordinatesNode.OwnerDocument.CreateAttribute( Constant.Attribute.Z_ATTR );																			  
            hAttr.Value = item.Z.ToString(System.Globalization.CultureInfo.InvariantCulture);
            hPointElement.SetAttributeNode(hAttr);

            hTranslateCoordinatesNode.AppendChild( hPointElement );
         }
         return hTranslateCoordinatesNode.OwnerDocument;
      }

      /// <summary>
      /// Encode the request to translate a bounding box from one coordinate system to another
      /// </summary>
      /// <param name="szHandle">The handle which uniquly identifies this request/response pair</param>
      /// <param name="hBoundingBox">The bounding box to translate</param>
      /// <param name="hOutputCoordinateSystem">The destination coordinate system</param>
      /// <returns>The GeosoftXML request</returns>
      public System.Xml.XmlDocument TranslateBoundingBox(	string szHandle, 			
                                                            BoundingBox hBoundingBox,
                                                            CoordinateSystem hOutputCoordinateSystem)
      {
         return TranslateBoundingBox( szHandle, hBoundingBox, hOutputCoordinateSystem, 0 );
      }

      /// <summary>
      /// Encode the request to translate a bounding box from one coordinate system to another
      /// </summary>
      /// <param name="szHandle">The handle which uniquly identifies this request/response pair</param>
      /// <param name="hBoundingBox">The bounding box to translate</param>
      /// <param name="hOutputCoordinateSystem">The destination coordinate system</param>
      /// <param name="dResolution">The current resolution that you wish to have translated to the new coordinate system</param>
      /// <returns>The GeosoftXML request</returns>
      public System.Xml.XmlDocument TranslateBoundingBox(	string szHandle, 			
                                                            BoundingBox hBoundingBox,
                                                            CoordinateSystem hOutputCoordinateSystem,
                                                            double dResolution )
      {
       
         // --- Create required Nodes ---
         
         System.Xml.XmlAttribute	hAttr;
         System.Xml.XmlElement	hTranslateBoundingBoxNode = CreateRequest(szHandle, Constant.Tag.TRANSLATE_BOUNDING_BOX_TAG);
         System.Xml.XmlNode   	hBoundingBoxNode;

         
         // --- Setup hierchy ---

         hBoundingBoxNode = AddBoundingBox(hTranslateBoundingBoxNode, hBoundingBox);
         AddCoordinateSystem(hBoundingBoxNode, hBoundingBox.CoordinateSystem );
         AddCoordinateSystem(hTranslateBoundingBoxNode, hOutputCoordinateSystem );	
         

         // --- Add attributes ---
         
         if (dResolution != 0) 
         {
            System.Xml.XmlElement	hResolutionNode = hTranslateBoundingBoxNode.OwnerDocument.CreateElement(Constant.Tag.RESOLUTION_TAG );
				
            hAttr = hTranslateBoundingBoxNode.OwnerDocument.CreateAttribute( Constant.Attribute.VALUE_ATTR );
            hAttr.Value = dResolution.ToString(System.Globalization.CultureInfo.InvariantCulture);
            hResolutionNode.SetAttributeNode( hAttr );
            hTranslateBoundingBoxNode.AppendChild( hResolutionNode );
         }

         return hTranslateBoundingBoxNode.OwnerDocument;
      }

      /// <summary>
      /// Encode the request to get the default resolution to extract a dataset at
      /// </summary>
      /// <param name="szHandle">The handle which uniquly identifies this request/response pair</param>
      /// <param name="szType">The dataset type</param>
      /// <param name="hBoundingBox">The bounding box to get the default resolution for</param>
      /// <returns>The GeosoftXML request</returns>
      public System.Xml.XmlDocument DefaultResolution(	string szHandle, string szType, BoundingBox hBoundingBox )
      {
         // --- Create required Nodes ---
         System.Xml.XmlAttribute	hAttr;
         System.Xml.XmlElement	hGetDefaultResolutionNode = CreateRequest(szHandle, Constant.Tag.DEFAULT_RESOLUTION_TAG);
         
         AddBoundingBox(hGetDefaultResolutionNode, hBoundingBox );
         
         
         // --- Add attributes ---
         
         if (szType != null && szType.Length != 0)
         {
            hAttr = hGetDefaultResolutionNode.OwnerDocument.CreateAttribute( Constant.Attribute.TYPE_ATTR );
            hAttr.Value = szType;
            hGetDefaultResolutionNode.SetAttributeNode( hAttr );
         }
         
         return hGetDefaultResolutionNode.OwnerDocument;
      }

      /// <summary>
      /// Encode the request to retrieve a coordinate system list from a Dap server.
      /// </summary>
      /// <param name="szHandle">The handle which uniquly identifies this request/response pair</param>
      /// <param name="szListType">The type of list you wish to retrieve. Must be DATUM, PROJECTION, UNITS, LOCAL_DATUM_NAME, LOCAL_DATUM_DESCRIPTION</param>
      /// <param name="szDatum">The datum to filter the returned results. It is required if szListType is LOCAL_DATUM_NAME or LOCAL_DATUM_DESCRIPTION</param>
      /// <returns>The GeosoftXML request</returns>
      public System.Xml.XmlDocument CoordinateSystemList( string szHandle, string szListType, string szDatum )
      {
         System.Xml.XmlAttribute	hAttr;
         System.Xml.XmlElement	hGetCoordinateNode = CreateRequest(szHandle, Constant.Tag.COORDINATE_SYSTEM_LIST_TAG);
			         
         if (szListType != null && szListType.Length > 0)
         {
            hAttr = hGetCoordinateNode.OwnerDocument.CreateAttribute( Constant.Attribute.LIST_TYPE_ATTR );
            hAttr.Value = szListType;

            hGetCoordinateNode.SetAttributeNode( hAttr );
         }

         if (szDatum != null && szDatum.Length > 0)
         {
            hAttr = hGetCoordinateNode.OwnerDocument.CreateAttribute( Constant.Attribute.DATUM_ATTR );
            hAttr.Value = szDatum;

            hGetCoordinateNode.SetAttributeNode( hAttr );
         }			
         return hGetCoordinateNode.OwnerDocument;
      }      

      /// <summary>
      /// Encode the request to extract a dataset from a Dap server
      /// </summary>
      /// <param name="szHandle">The handle which uniquly identifies this request/response pair</param>
      /// <param name="hDataSetList">List of datasets to extract</param>
      /// <param name="hBoundingBox">The bounding region to limit the size of the extraction</param>
      /// <param name="bNative">Save datasets in native coordinate system or that of the bounding box</param>
      /// <returns>The GeosoftXML request</returns>
      public System.Xml.XmlDocument Extract(string szHandle, ArrayList hDataSetList, BoundingBox hBoundingBox, bool bNative)         
      {
         System.Xml.XmlAttribute	hAttr;
         System.Xml.XmlElement	hExtractNode = CreateRequest(szHandle, Constant.Tag.EXTRACT_TAG);
         System.Xml.XmlElement	hNativeNode = hExtractNode.OwnerDocument.CreateElement(Constant.Tag.NATIVE_TAG);
         System.Xml.XmlElement	hDataSetsNode = hExtractNode.OwnerDocument.CreateElement(Constant.Tag.DATASETS_TAG );
         System.Xml.XmlNode   	hBoundingBoxNode;
         
         hExtractNode.AppendChild(hDataSetsNode);
         hBoundingBoxNode = AddBoundingBox(hExtractNode, hBoundingBox );
         AddCoordinateSystem(hBoundingBoxNode, hBoundingBox.CoordinateSystem );	
                 

         // --- Add attributes ---

         
         foreach (ExtractDataSet hDataSet  in hDataSetList)
         {
            System.Xml.XmlElement hDataSetNode = hExtractNode.OwnerDocument.CreateElement(Constant.Tag.DATASET_TAG );

            hAttr = hExtractNode.OwnerDocument.CreateAttribute( Constant.Attribute.NAME_ATTR );
            hAttr.Value = hDataSet.DataSet.Name;				
            hDataSetNode.SetAttributeNode( hAttr );

            hAttr = hExtractNode.OwnerDocument.CreateAttribute( Constant.Attribute.FORMAT_ATTR );
            hAttr.Value = hDataSet.Format;				
            hDataSetNode.SetAttributeNode( hAttr );

            hAttr = hExtractNode.OwnerDocument.CreateAttribute( Constant.Attribute.RESOLUTION_ATTR );
            hAttr.Value = hDataSet.Resolution.ToString(System.Globalization.CultureInfo.InvariantCulture);				
            hDataSetNode.SetAttributeNode( hAttr );

            hDataSetsNode.AppendChild(hDataSetNode);
         }
         
         hAttr = hExtractNode.OwnerDocument.CreateAttribute( Constant.Attribute.VALUE_ATTR );
         hAttr.Value = bNative.ToString();
         hNativeNode.SetAttributeNode( hAttr );		         		
         
         return hExtractNode.OwnerDocument;			
      }

      /// <summary>
      /// Encode the request to find out the progress of an extraction
      /// </summary>
      /// <param name="szHandle">The handle which uniquly identifies this request/response pair</param>
      /// <param name="szKey">The extraction key</param>
      /// <returns>The GeosoftXML request</returns>
      public System.Xml.XmlDocument ExtractProgress( string szHandle, string szKey )
      {
         System.Xml.XmlAttribute	hAttr;
         System.Xml.XmlElement	hExtractStatusNode = CreateRequest(szHandle, Constant.Tag.EXTRACT_STATUS_TAG);         

         if (szKey != null && szKey.Length != 0)
         {
            hAttr = hExtractStatusNode.OwnerDocument.CreateAttribute( Constant.Attribute.KEY_ATTR );
            hAttr.Value = szKey;

            hExtractStatusNode.SetAttributeNode( hAttr );
         }
         return hExtractStatusNode.OwnerDocument;	
      }

      /// <summary>
      /// Encode the request to cancel an extraction
      /// </summary>
      /// <param name="szHandle">The handle which uniquly identifies this request/response pair</param>
      /// <param name="szKey">The extraction key</param>
      /// <returns>The GeosoftXML request</returns>
      public System.Xml.XmlDocument ExtractCancel( string szHandle, string szKey )
      {
         System.Xml.XmlAttribute	hAttr;
         System.Xml.XmlElement	hExtractCancelNode = CreateRequest(szHandle, Constant.Tag.EXTRACT_CANCEL_TAG);
         
         if( szKey != null && szKey.Length != 0 )
         {
            hAttr = hExtractCancelNode.OwnerDocument.CreateAttribute( Constant.Attribute.KEY_ATTR );
            hAttr.Value = szKey;

            hExtractCancelNode.SetAttributeNode( hAttr );
         }
         return hExtractCancelNode.OwnerDocument;
      }

      /// <summary>
      /// Encode the request to retrieve the data once an extraction has completed
      /// </summary>
      /// <param name="szHandle">The handle which uniquly identifies this request/response pair</param>
      /// <param name="szKey">The extraction key</param>
      /// <returns>The GeosoftXML request</returns>
      public System.Xml.XmlDocument ExtractData( string szHandle, string szKey )
      {
         System.Xml.XmlAttribute	hAttr;
         System.Xml.XmlElement	hExtractDataNode = CreateRequest(szHandle, Constant.Tag.EXTRACT_DATA_TAG);
         
         if (szKey != null && szKey.Length != 0)
         {
            hAttr = hExtractDataNode.OwnerDocument.CreateAttribute( Constant.Attribute.KEY_ATTR );
            hAttr.Value = szKey;

            hExtractDataNode.SetAttributeNode( hAttr );
         }
         return hExtractDataNode.OwnerDocument;
      }      

      /// <summary>
      /// Encode the request to get the log of a Dap server
      /// </summary>
      /// <param name="strHandle">The handle which uniquly identifies this request/response pair</param>
      /// <param name="strPassword">The administrator password</param>
      /// <param name="oDate">The date of the log to retrieve</param>
      /// <returns>The GeosoftXML request</returns>
      public System.Xml.XmlDocument GetLog(string strHandle, string strPassword, DateTime oDate)
      {
         System.Xml.XmlAttribute hAttr;
         System.Xml.XmlElement	hGetLogNode = CreateRequest(strHandle, Constant.Tag.LOG_TAG);

         if (strPassword != null && strPassword.Length > 0)
         {
            // --- Add attributes ---
         
            hAttr = hGetLogNode.OwnerDocument.CreateAttribute(Constant.Attribute.PASSWORD_ATTR);
            hAttr.Value = strPassword;
            hGetLogNode.SetAttributeNode(hAttr);

            hAttr = hGetLogNode.OwnerDocument.CreateAttribute(Constant.Attribute.DATE_ATTR);
            hAttr.Value = oDate.ToString("dddd, MMMM dd, yyyy");
            hGetLogNode.SetAttributeNode(hAttr);
         }

         return hGetLogNode.OwnerDocument;
      }

      /// <summary>
      /// Encode the request to clear the log of a Dap server
      /// </summary>
      /// <param name="strHandle">The handle which uniquly identifies this request/response pair</param>
      /// <param name="strPassword">The administrator password</param>
      /// <param name="oDate">The date of the log to clear</param>
      /// <returns>The GeosoftXML request</returns>
      public System.Xml.XmlDocument ClearLog(string strHandle, string strPassword, DateTime oDate)
      {
         System.Xml.XmlAttribute hAttr;
         System.Xml.XmlElement	hClearLogNode = CreateRequest(strHandle, Constant.Tag.CLEAR_LOG_TAG);

         if (strPassword != null && strPassword.Length > 0)
         {
            // --- Add attributes ---
         
            hAttr = hClearLogNode.OwnerDocument.CreateAttribute(Constant.Attribute.PASSWORD_ATTR);
            hAttr.Value = strPassword;
            hClearLogNode.SetAttributeNode(hAttr);

            hAttr = hClearLogNode.OwnerDocument.CreateAttribute(Constant.Attribute.DATE_ATTR);
            hAttr.Value = oDate.ToString("dddd, MMMM dd, yyyy");
            hClearLogNode.SetAttributeNode(hAttr);
         }

         return hClearLogNode.OwnerDocument;
      }

      /// <summary>
      /// Encode the request to list all the logs of a Dap server
      /// </summary>
      /// <param name="strHandle">The handle which uniquly identifies this request/response pair</param>
      /// <param name="strPassword">The administrator password</param>
      /// <returns>The GeosoftXML request</returns>
      public System.Xml.XmlDocument ListLogs(string strHandle, string strPassword)
      {
         System.Xml.XmlAttribute hAttr;
         System.Xml.XmlElement	hListLogNode = CreateRequest(strHandle, Constant.Tag.LIST_LOG_TAG);

         if (strPassword != null && strPassword.Length > 0)
         {
            // --- Add attributes ---
         
            hAttr = hListLogNode.OwnerDocument.CreateAttribute(Constant.Attribute.PASSWORD_ATTR);
            hAttr.Value = strPassword;
            hListLogNode.SetAttributeNode(hAttr);
         }

         return hListLogNode.OwnerDocument;
      }

      /// <summary>
      /// Create a client state object on the server
      /// </summary>
      /// <param name="szHandle">The handle which uniquly identifies this request/response pair</param>
      /// <returns>The GeosoftXML request</returns>
      public System.Xml.XmlDocument CreateClientState(string szHandle)
      {
         System.Xml.XmlElement	hStateNode = CreateRequest(szHandle, Constant.Tag.CREATE_STATE_TAG);

         return hStateNode.OwnerDocument;
      }

      /// <summary>
      /// Destroy a client state object on the server
      /// </summary>
      /// <param name="szHandle">The handle which uniquly identifies this request/response pair</param>
      /// <param name="strKey">State object key</param>
      /// <returns>The GeosoftXML request</returns>
      public System.Xml.XmlDocument DestroyClientState(string szHandle, string strKey)
      {
         System.Xml.XmlAttribute hAttr;
         System.Xml.XmlElement	hStateNode = CreateRequest(szHandle, Constant.Tag.DESTROY_STATE_TAG);

         if (strKey != null && strKey.Length > 0)
         {
            // --- Add attributes ---
         
            hAttr = hStateNode.OwnerDocument.CreateAttribute(Constant.Attribute.VALUE_ATTR);
            hAttr.Value = strKey;
            hStateNode.SetAttributeNode(hAttr);
         }

         return hStateNode.OwnerDocument;
      }
   }
}
