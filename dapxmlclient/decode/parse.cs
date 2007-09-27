using System;
using System.Collections;
using Geosoft.Dap.Common;
using Geosoft.Dap.Xml.Common;

namespace Geosoft.Dap.Xml
{
   /// <summary>
   /// Translate a GeosoftXML response into client-side structures, abstracting the format of GeosoftXML from the client application
   /// </summary>
   public class Parse
   {
      #region Member Variables
      /// <summary>
      /// The url of the dap server
      /// </summary>
      protected String  m_szUrl;   
      #endregion

      #region Properties
      /// <summary>
      /// Get or set the url of the dap server
      /// </summary>
      public String Url
      {
         set { m_szUrl = value;}
         get { return m_szUrl; }
      }
      #endregion

      #region Constructor
      /// <summary>
      /// Default constructor
      /// </summary>
      public Parse()
      {
         m_szUrl = String.Empty;
      }

      /// <summary>
      /// Default constructor
      /// </summary>
      /// <param name="szUrl"></param>
      public Parse(String szUrl)
      {
         m_szUrl = szUrl;         
      }         
      #endregion

      /// <summary>
      /// Get the number of entities in this response
      /// </summary>
      /// <param name="hDocument">The GeosoftXML response</param>
      public bool AuthenticateUser( System.Xml.XmlDocument hDocument ) 
      {         
         bool                          bAuthenticate = false;
         System.Xml.XmlNodeList        hReturnList;         
         
         
         // --- find all the item elements located anywhere in the document ---

         hReturnList = hDocument.SelectNodes( "//" + Constant.Tag.AUTHENTICATE_TAG );

         if (hReturnList != null && hReturnList.Count > 0)
         {
            System.Xml.XmlNode   oAttr;
            oAttr = hReturnList[0].Attributes.GetNamedItem(Constant.Tag.VALUE_TAG);
            try
            {
               bAuthenticate = Convert.ToBoolean(oAttr.Value);
            } 
            catch {}
         }
         return bAuthenticate;
      }

      /// <summary>
      /// Parse the properties response.
      /// </summary>
      /// <param name="hDocument">The GeosoftXML response</param>
      /// <param name="hPropertiesList">The list of properties</param>
      public void Properties( System.Xml.XmlDocument hDocument, out SortedList hPropertiesList ) 
      {
         System.Xml.XmlNodeList        hItemList;
         
         try 
         {
            // --- find all the item elements located anywhere in the document ---

            hItemList = hDocument.SelectNodes( "//" + Constant.Tag.ITEM_TAG );

            if (hItemList == null) throw new DapException("No properties found");

            hPropertiesList = new System.Collections.SortedList();

            // --- loop through all the datasets adding them to the list ---

            foreach (System.Xml.XmlNode hNode in hItemList)
            {
               System.Xml.XmlNode hName;
               System.Xml.XmlNode hValue;

               hName = hNode.Attributes.GetNamedItem(Constant.Attribute.NAME_ATTR);
               hValue = hNode.Attributes.GetNamedItem(Constant.Attribute.VALUE_ATTR);

               hPropertiesList.Add(hName.Value, hValue.Value);
            }
         } 
         catch(Exception e)
         {
            throw new DapException("Error compiling list of properties", e);
         }
      }

      /// <summary>
      /// Parse the catalog response.
      /// Note: this method removes any hierarchy associated with the datasets
      /// </summary>
      /// <param name="hDocument">The GeosoftXML response</param>
      /// <param name="hDataSetList">The list of datasets</param>
      public void Catalog( System.Xml.XmlDocument hDocument, out ArrayList hDataSetList ) 
      {
         System.Xml.XmlNodeList        hItemList;
         DataSet                       hDataSet;
         BoundingBox                   hBoundingBox;
         
         try 
         {
            // --- find all the item elements located anywhere in the document ---

            hItemList = hDocument.SelectNodes( "//" + Constant.Tag.ITEM_TAG );

            if (hItemList == null) throw new DapException("No datasets found in catalog");

            hDataSetList = new System.Collections.ArrayList();


            // --- loop through all the datasets adding them to the list ---

            foreach (System.Xml.XmlNode hNode in hItemList)
            {

               // --- parse the data set element ---

               this.DataSet(hNode, out hDataSet);


               // --- parse the bounding box element ---

               this.BoundingBox(hNode.FirstChild, out hBoundingBox);

               hDataSet.Boundary = hBoundingBox;
               hDataSetList.Add(hDataSet);
            }
         } 
         catch(Exception e)
         {
            throw new DapException("Error compiling list of datasets", e);
         }
      }

      /// <summary>
      /// Parse the catalog response.
      /// Note: this method removes any hierarchy associated with the datasets
      /// </summary>
      /// <param name="hDocument">The GeosoftXML response</param>
      /// <param name="iStartIndex">The index to start adding nodes</param>
      /// <param name="iCount">The number of nodes to add</param>
      /// <param name="hDataSetList">The list of datasets</param>
      public void Catalog( System.Xml.XmlDocument hDocument, int iStartIndex, int iCount, out ArrayList hDataSetList ) 
      {         
         System.Xml.XmlNodeList        hItemList;
         System.Xml.XmlNode            hNode;
         DataSet                       hDataSet;
         BoundingBox                   hBoundingBox;
         
         try 
         {
            // --- find all the item elements located anywhere in the document ---

            hItemList = hDocument.SelectNodes( "//" + Constant.Tag.ITEM_TAG );

            if (hItemList == null) throw new DapException("No datasets found in catalog");

            hDataSetList = new System.Collections.ArrayList();

            if (iCount == 0) iCount = hItemList.Count;

            // --- loop through all the datasets adding them to the list ---

            for (int i = 0; i < iCount; i++)
            {               
               if (i + iStartIndex >= hItemList.Count) return;

               hNode = hItemList[i + iStartIndex];

               // --- parse the data set element ---

               this.DataSet(hNode, out hDataSet);


               // --- parse the bounding box element ---

               this.BoundingBox(hNode.FirstChild, out hBoundingBox);

               hDataSet.Boundary = hBoundingBox;
               hDataSetList.Add(hDataSet);
            }
         } 
         catch(Exception e)
         {
            throw new DapException("Error compiling list of datasets", e);
         }
      }

      /// <summary>
      /// Get the number of entities in this response
      /// </summary>
      /// <param name="hDocument">The GeosoftXML response</param>
      public Int32 CatalogCount( System.Xml.XmlDocument hDocument ) 
      {         
         System.Xml.XmlNodeList        hItemList;         
         
         try 
         {
            // --- find all the item elements located anywhere in the document ---

            hItemList = hDocument.SelectNodes( "//" + Constant.Tag.ITEM_TAG );

            if (hItemList == null) throw new DapException("No datasets found in catalog");

            return hItemList.Count;            
         } 
         catch(Exception e)
         {
            throw new DapException("Error counting list of datasets", e);
         }
      }

      /// <summary>
      /// Remove any folders that do not have dataset children
      /// </summary>
      /// <param name="hDoc"></param>
      public void PruneCatalog(System.Xml.XmlDocument hDoc)
      {
         System.Xml.XmlNode                          hGeosoftXml = hDoc.DocumentElement;
         System.Xml.XmlNode                          hResponse = hGeosoftXml.FirstChild;
         System.Xml.XmlNode                          hCatalog = hResponse.FirstChild;

         PruneFolder(hCatalog);
      }

      /// <summary>
      /// Recursively prune each folder
      /// </summary>
      /// <param name="hCurFolder"></param>
      /// <returns></returns>
      protected bool PruneFolder(System.Xml.XmlNode hCurFolder)
      {
         bool bFoundDataSet = false;
         for (Int32 i = 0; i < hCurFolder.ChildNodes.Count; i++)
         {
            System.Xml.XmlNode hNode = hCurFolder.ChildNodes[i];

            if (hNode.Name == Geosoft.Dap.Xml.Common.Constant.Tag.COLLECTION_TAG)
            {            
               bool bFoundChildDataSet = PruneFolder(hNode);
               
               // --- no datasets, down this path ---

               if (!bFoundChildDataSet)
               {
                  hCurFolder.RemoveChild(hNode);
                  i--;
               } 
               else 
               {
                  bFoundDataSet = true;   // only need 1 child to have it to keep parent
               }
            }
            else if (hNode.Name == Geosoft.Dap.Xml.Common.Constant.Tag.ITEM_TAG)
            {
               bFoundDataSet = true;
            }
         }
         return bFoundDataSet;
      }

      /// <summary>
      /// Refresh catalog response
      /// </summary>
      /// <param name="hDocument">The GeosoftXML response</param>
      public bool RefreshCatalog( System.Xml.XmlDocument hDocument ) 
      {         
         bool                          bReturn = false;
         System.Xml.XmlNodeList        hReturnList;         
         
         
         // --- find all the item elements located anywhere in the document ---

         hReturnList = hDocument.SelectNodes( "//" + Constant.Tag.REFRESH_CATALOG_TAG );

         if (hReturnList != null && hReturnList.Count > 0)
         {
            System.Xml.XmlNode   oAttr;
            oAttr = hReturnList[0].Attributes.GetNamedItem(Constant.Tag.VALUE_TAG);
            try
            {
               bReturn = Convert.ToBoolean(oAttr.Value);
            } 
            catch {}
         }
         return bReturn;
      }

      /// <summary>
      /// Parse the catalog count response.
      /// </summary>
      /// <param name="hDocument">The GeosoftXML response</param>
      /// <param name="iNumDataSets">The number of datasets</param>
      public void DataSetCount( System.Xml.XmlDocument hDocument, out Int32 iNumDataSets ) 
      {
         System.Xml.XmlNodeList        hItemList;
         System.Xml.XmlNode            hNode;
         System.Xml.XmlNode            hAttr;
         
         try 
         {
            // --- find all the item elements located anywhere in the document ---

            hItemList = hDocument.SelectNodes( "//" + Constant.Tag.COUNT_TAG );

            if (hItemList == null || hItemList.Count == 0) throw new DapException("No count node found in catalog");

            hNode = hItemList[0];
            hAttr = hNode.Attributes.GetNamedItem(Constant.Attribute.VALUE_ATTR);

            if (hAttr == null) throw new DapException("Missing value attribute in cound element");
            iNumDataSets = Int32.Parse(hAttr.Value);
         } 
         catch(Exception e)
         {
            throw new DapException("Error retrieve number of datasets in catalog request", e);
         }
      }

      /// <summary>
      /// Parse the catalog edition response.
      /// </summary>
      /// <param name="hDocument">The GeosoftXML response</param>
      /// <param name="strConfigurationEdition">The edition of the server</param>
      /// <param name="strEdition">The edition of the catalog</param>
      public void CatalogEdition( System.Xml.XmlDocument hDocument, out string strConfigurationEdition, out string strEdition) 
      {
         System.Xml.XmlNode         hEdition;         
         System.Xml.XmlNode         hAttr;

         strEdition = null;

         try 
         {
            // --- find the configuration edition element ---

            strConfigurationEdition = string.Empty;
            hEdition = hDocument.SelectSingleNode("/" + Constant.Tag.GEO_XML_TAG + "/" + Constant.Tag.RESPONSE_TAG + "/" + Constant.Tag.CONFIGURATION_TAG);

            if (hEdition != null) 
            {
               hAttr = hEdition.Attributes.GetNamedItem(Constant.Attribute.VERSION_ATTR);
               if (hAttr != null) strConfigurationEdition = hAttr.Value;
            }

            // --- find the catalog edition element ---

            hEdition = hDocument.SelectSingleNode("/" + Constant.Tag.GEO_XML_TAG + "/" + Constant.Tag.RESPONSE_TAG + "/" + Constant.Tag.CATALOG_EDITION_TAG);

            if (hEdition == null) throw new DapException("No edition found in catalog");

            hAttr = hEdition.Attributes.GetNamedItem(Constant.Attribute.EDITION_ATTR);
            if (hAttr != null) strEdition = hAttr.Value;
         } 
         catch(Exception e)
         {
            throw new DapException("Error retrieving catalog edition", e);
         }
      }

      /// <summary>
      /// Parse the dataset edition response.
      /// </summary>
      /// <param name="hDocument">The GeosoftXML response</param>
      /// <param name="szEdition">The edition of the dataset</param>
      public void DataSetEdition( System.Xml.XmlDocument hDocument, out string szEdition ) 
      {
         System.Xml.XmlNode         hEdition;         
         System.Xml.XmlNode         hAttr;

         szEdition = null;

         try 
         {
            // --- find the dataset edition element ---

            hEdition = hDocument.SelectSingleNode("/" + Constant.Tag.GEO_XML_TAG + "/" + Constant.Tag.RESPONSE_TAG + "/" + Constant.Tag.DATASET_EDITION_TAG);

            if (hEdition == null) throw new DapException("No edition found in dataset");

            hAttr = hEdition.Attributes.GetNamedItem(Constant.Attribute.EDITION_ATTR);
            if (hAttr != null) szEdition = hAttr.Value;
         } 
         catch(Exception e)
         {
            throw new DapException("Error retrieving dataset edition", e);
         }
      }

      /// <summary>
      /// Transform the item element, found in the catalog response, to its corresponding structure.
      /// </summary>
      /// <param name="oCatalog">The GeosoftXML catalog</param>
      /// <param name="strDataSetName">The server name of the dataset</param>
      /// <param name="oDataSet">The Dataset structure that will be populated</param>
      public void DataSet(System.Xml.XmlDocument oCatalog, string strDataSetName, out DataSet oDataSet)
      {
         System.Xml.XmlNodeList        hItemList;
         
         try 
         {
            // --- find all the item elements located anywhere in the document ---

            string   szPath = "//" + Geosoft.Dap.Xml.Common.Constant.Tag.ITEM_TAG + "[@" + Geosoft.Dap.Xml.Common.Constant.Attribute.NAME_ATTR + "=\"" + strDataSetName + "\"]";

            hItemList = oCatalog.SelectNodes(szPath);
            
            if (hItemList == null || hItemList.Count == 0) throw new DapException("No dataset node found with name " + strDataSetName);

            DataSet(hItemList[0], out oDataSet);            
         } 
         catch(System.Xml.XmlException e)
         {
            throw new DapException("Error retrieve dataset node " + strDataSetName, e);
         }
      }

      /// <summary>
      /// Transform the item element, found in the catalog response, to its corresponding structure.
      /// </summary>
      /// <param name="hDataSetNode">The GeosoftXML item element</param>
      /// <param name="hDataSet">The dataset structure to populate</param>
      public void DataSet( System.Xml.XmlNode hDataSetNode, out DataSet hDataSet )
      {       
         hDataSet = null;

         if (hDataSetNode == null || hDataSetNode.Name != Xml.Common.Constant.Tag.ITEM_TAG) 
         {
            throw new DapException("Invalid format found in item element.");
         }

         
         System.Xml.XmlNode hName = hDataSetNode.Attributes.GetNamedItem( Constant.Attribute.NAME_ATTR );
         System.Xml.XmlNode hTitle = hDataSetNode.Attributes.GetNamedItem( Constant.Attribute.TITLE_ATTR );
         System.Xml.XmlNode hType = hDataSetNode.Attributes.GetNamedItem( Constant.Attribute.TYPE_ATTR );
         System.Xml.XmlNode hEdition = hDataSetNode.Attributes.GetNamedItem( Constant.Attribute.EDITION_ATTR );

         String               szHierarchy = "";
         System.Xml.XmlNode   hParent = hDataSetNode.ParentNode;

         while (hParent != null && hParent.Name != Constant.Tag.CATALOG_TAG)
         {
            System.Xml.XmlNode hFolderName = hParent.Attributes.GetNamedItem( Constant.Attribute.NAME_ATTR );
            szHierarchy = hFolderName.Value + "/" + szHierarchy;

            hParent = hParent.ParentNode;
         }

         hDataSet = new DataSet();   
         hDataSet.Url = m_szUrl;

         if (hDataSetNode.HasChildNodes)
         {
            BoundingBox oBB;

            BoundingBox(hDataSetNode.FirstChild, out oBB);
            hDataSet.Boundary = oBB;
         }

         if (hName != null) hDataSet.Name = hName.Value;
         if (hTitle != null) hDataSet.Title = hTitle.Value;
         if (hType != null) hDataSet.Type = hType.Value;
         if (hEdition != null) hDataSet.Edition = hEdition.Value;
         if (szHierarchy != null) hDataSet.Hierarchy = szHierarchy;
      }

      /// <summary>
      /// Transform the bounding box xml element to its corresponding structure.
      /// </summary>
      /// <param name="hBoundingBoxNode">The GeosoftXML Bounding Box node</param>
      /// <param name="hBoundingBox">The bounding box structure to populate</param>
      public void BoundingBox( System.Xml.XmlNode hBoundingBoxNode, out BoundingBox hBoundingBox )
      {  
       
         // --- initialize the coordinates to the boundarys of the world in WGS 84 ---

         double dMaxX = 180;
         double dMaxY = 90;
         double dMinX = -180;
         double dMinY = -90;

         hBoundingBox = new BoundingBox(dMaxX,dMaxY,dMinX,dMinY);


         // --- if it is null, then just default to the world ---

         if (hBoundingBoxNode == null) return;
         

         // --- bad tag given, generate an error ---

         if (hBoundingBoxNode.Name != Xml.Common.Constant.Tag.BOUNDING_BOX_TAG) 
         {
            throw new DapException("Invalid format found in bounding box element.");
         }

         
         System.Xml.XmlNode hMaxX = hBoundingBoxNode.Attributes.GetNamedItem( Constant.Attribute.MAX_X_ATTR );
         System.Xml.XmlNode hMaxY = hBoundingBoxNode.Attributes.GetNamedItem( Constant.Attribute.MAX_Y_ATTR );
         System.Xml.XmlNode hMinX = hBoundingBoxNode.Attributes.GetNamedItem( Constant.Attribute.MIN_X_ATTR );
         System.Xml.XmlNode hMinY = hBoundingBoxNode.Attributes.GetNamedItem( Constant.Attribute.MIN_Y_ATTR );

            
         if (hMaxX != null) hBoundingBox.MaxX = Double.Parse(hMaxX.Value, System.Globalization.CultureInfo.InvariantCulture);
         if (hMaxY != null) hBoundingBox.MaxY = Double.Parse(hMaxY.Value, System.Globalization.CultureInfo.InvariantCulture);
         if (hMinX != null) hBoundingBox.MinX = Double.Parse(hMinX.Value, System.Globalization.CultureInfo.InvariantCulture);
         if (hMinY != null) hBoundingBox.MinY = Double.Parse(hMinY.Value, System.Globalization.CultureInfo.InvariantCulture);
      }

      /// <summary>
      /// Parse the default resolution response.
      /// </summary>
      /// <param name="hDocument">The GeosoftXML response</param>
      /// <param name="szResolution">The default resolution</param>
      public void DefaultResolution( System.Xml.XmlDocument hDocument, out string szResolution ) 
      {
         System.Xml.XmlNode         hEdition;         
         System.Xml.XmlNode         hAttr;

         szResolution = null;

         try 
         {
            
            // --- find the dataset edition element ----

            hEdition = hDocument.SelectSingleNode("/" + Constant.Tag.GEO_XML_TAG + "/" + Constant.Tag.RESPONSE_TAG + "/" + Constant.Tag.DEFAULT_RESOLUTION_TAG + "/" + Constant.Tag.RESOLUTION_TAG);

            if (hEdition == null) throw new DapException("No default resolution found");

            hAttr = hEdition.Attributes.GetNamedItem(Constant.Attribute.VALUE_ATTR);
            if (hAttr != null) szResolution = hAttr.Value;
         } 
         catch(Exception e)
         {
            throw new DapException("Error retrieving default resolution", e);
         }
      }

      /// <summary>
      /// Parse the keywords response.
      /// </summary>
      /// <param name="hDocument">The GeosoftXML response</param>
      /// <param name="hKeywords">The list of keywords</param>
      public void Keywords( System.Xml.XmlDocument hDocument, out ArrayList hKeywords ) 
      {         
         System.Xml.XmlNodeList        hItemList;
         System.Xml.XmlNode            hNode;
         SortedList                    hKeywordList = new SortedList();
         
         try 
         {
            // --- find all the item elements located anywhere in the document ---

            hItemList = hDocument.SelectNodes( "//" + Constant.Tag.ITEM_TAG );

            if (hItemList == null) throw new DapException("No keywords found");

            // --- loop through all the datasets adding them to the list ---

            for (int i = 0; i < hItemList.Count; i++)
            {  
               System.Xml.XmlNode   hAttr;

               hNode = hItemList[i];
               hAttr = hNode.Attributes.GetNamedItem(Constant.Attribute.VALUE_ATTR);

               if (hAttr != null && hAttr.Value != null)
                  hKeywordList.Add(hAttr.Value, "");
            }
         } 
         catch(Exception e)
         {
            throw new DapException("Error compiling list of keywords", e);
         }
         hKeywords = new ArrayList(hKeywordList.Keys);
      }

      /// <summary>
      /// Parse the coordinate system list.
      /// </summary>
      /// <param name="hDocument">The GeosoftXML response</param>
      /// <param name="hCoordinateSystemList">The list of items</param>
      public void SupportedCoordinateSystem( System.Xml.XmlDocument hDocument, out ArrayList hCoordinateSystemList ) 
      {
         System.Xml.XmlNodeList        hItemList;
         System.Xml.XmlNode            hAttr;
         
         hCoordinateSystemList = null;
         
         try 
         {

            // --- find all the item elements located anywhere in the document ---

            hItemList = hDocument.SelectNodes("//" + Constant.Tag.ITEM_TAG);

            if (hItemList == null) throw new DapException("No items found in supported coordinate system list");

            hCoordinateSystemList = new System.Collections.ArrayList();


            // --- loop through all the datasets adding them to the list ---

            foreach (System.Xml.XmlNode hNode in hItemList)
            {
               hAttr = hNode.Attributes.GetNamedItem(Constant.Attribute.NAME_ATTR);

               if (hAttr != null) hCoordinateSystemList.Add(hAttr.Value);               
            }
         } 
         catch(Exception e)
         {
            throw new DapException("Error compiling list of datasets", e);
         }
      }

      /// <summary>
      /// Parse the extract response.
      /// </summary>
      /// <param name="hDocument">The GeosoftXML response</param>
      /// <param name="szKey">The key that represents this extraction</param>
      public void ExtractKey( System.Xml.XmlDocument hDocument, out string szKey ) 
      {
         System.Xml.XmlNode         hKey;         
         System.Xml.XmlNode         hAttr;

         szKey = null;
         try 
         {
            
            // --- find the extract key element ---

            hKey = hDocument.SelectSingleNode("/" + Constant.Tag.GEO_XML_TAG + "/" + Constant.Tag.RESPONSE_TAG + "/" + Constant.Tag.EXTRACT_TAG + "/" + Constant.Tag.KEY_TAG);

            if (hKey == null) throw new DapException("No key found in extract");

            hAttr = hKey.Attributes.GetNamedItem(Constant.Attribute.NAME_ATTR);
            if (hAttr != null) szKey = hAttr.Value;
         } 
         catch(Exception e)
         {
            throw new DapException("Error retrieving key in extract element", e);
         }
      }

      /// <summary>
      /// Parse the extract progress response.
      /// </summary>
      /// <param name="hDocument">The GeosoftXML response</param>
      /// <param name="eStatus">The status of the extraction</param>
      /// <param name="iProgress">The percent completion of the extraction</param>
      /// <param name="szStatus">The current extraction task</param>
      public void ExtractProgress( System.Xml.XmlDocument hDocument, out Constant.ExtractStatus eStatus, out Int32 iProgress, out String szStatus ) 
      {
         System.Xml.XmlNode         hKey;         
         System.Xml.XmlNode         hAttr;
         
         eStatus = Constant.ExtractStatus.UNKNOWN;
         iProgress = 0;
         szStatus = null;

         try 
         {
            
            // --- find the extract key element ---

            hKey = hDocument.SelectSingleNode("/" + Constant.Tag.GEO_XML_TAG + "/" + Constant.Tag.RESPONSE_TAG + "/" + Constant.Tag.EXTRACT_STATUS_TAG + "/" + Constant.Tag.STATUS_TAG);

            if (hKey == null) throw new DapException("No status information found in extract progress");

            hAttr = hKey.Attributes.GetNamedItem(Constant.Attribute.VALUE_ATTR);
            if (hAttr == null) 
            {
               eStatus = Constant.ExtractStatus.UNKNOWN;               
            } 
            else 
            {
               if (hAttr.Value == "IN PROGRESS")
                  eStatus = Constant.ExtractStatus.IN_PROGRESS;
               else if (hAttr.Value == "COMPLETED")
                  eStatus = Constant.ExtractStatus.COMPLETED;
               else if (hAttr.Value == "CANCELLED")
                  eStatus = Constant.ExtractStatus.CANCELLED;
            }

            hAttr = hKey.Attributes.GetNamedItem(Constant.Attribute.PROGRESS_ATTR);
            if (hAttr != null) iProgress = Convert.ToInt32(hAttr.Value);

            hAttr = hKey.Attributes.GetNamedItem(Constant.Attribute.STATUS_ATTR);
            if (hAttr != null) szStatus = hAttr.Value;
         } 
         catch(Exception e)
         {
            throw new DapException("Error retrieving status information in extract progress", e);
         }
      }

      /// <summary>
      /// Parse the translate coordinates response.
      /// </summary>
      /// <param name="hDocument">The GeosoftXML response</param>
      /// <param name="hTranslateCoordinatesList">The list of translated points</param>
      public void TranslateCoordinates( System.Xml.XmlDocument hDocument, out ArrayList hTranslateCoordinatesList ) 
      {
         System.Xml.XmlNodeList        hItemList;
         System.Xml.XmlNode            hAttr;
         
         hTranslateCoordinatesList = null;
         
         try 
         {
            
            // --- find all the item elements located anywhere in the document ---

            hItemList = hDocument.SelectNodes("//" + Constant.Tag.POINT_TAG);

            if (hItemList == null) throw new DapException("No points found in translate coordinates");

            hTranslateCoordinatesList = new System.Collections.ArrayList();

            
            // --- loop through all the datasets adding them to the list ---
            
            foreach (System.Xml.XmlNode hNode in hItemList)
            {
               Point hPoint = new Point();

               hAttr = hNode.Attributes.GetNamedItem(Constant.Attribute.X_ATTR);
               if (hAttr != null) hPoint.X = Double.Parse(hAttr.Value, System.Globalization.CultureInfo.InvariantCulture);               

               hAttr = hNode.Attributes.GetNamedItem(Constant.Attribute.Y_ATTR);
               if (hAttr != null) hPoint.Y = Double.Parse(hAttr.Value, System.Globalization.CultureInfo.InvariantCulture);               

               hAttr = hNode.Attributes.GetNamedItem(Constant.Attribute.Z_ATTR);
               if (hAttr != null) hPoint.Z = Double.Parse(hAttr.Value, System.Globalization.CultureInfo.InvariantCulture);     
          
               hTranslateCoordinatesList.Add(hPoint);
            }
         } 
         catch(Exception e)
         {
            throw new DapException("Error compiling list of datasets", e);
         }
      }

      /// <summary>
      /// Parse the translate bounding box response.
      /// </summary>
      /// <param name="hDocument">The GeosoftXML response</param>
      /// <param name="hBoundingBox">The translated bounding box coordinates</param>
      /// <param name="dResolution">The translated resolution</param>
      public void TranslateBoundingBox( System.Xml.XmlDocument hDocument, out BoundingBox hBoundingBox, out Double dResolution ) 
      {
         System.Xml.XmlNode            hNode;
         System.Xml.XmlNode            hAttr;

         hBoundingBox = null;
         dResolution = 0;

         try 
         {
            
            // --- find the boundingbox element located anywhere in the document ---

            hNode = hDocument.SelectSingleNode("//" + Constant.Tag.BOUNDING_BOX_TAG);

            if (hNode == null) throw new DapException("No bounding box found in translate bounding box");            

            this.BoundingBox( hNode, out hBoundingBox );               
            

            // --- find the resolution element located anywhere in the document ----

            hNode = hDocument.SelectSingleNode("//" + Constant.Tag.RESOLUTION_TAG);

            if (hNode != null) {
               hAttr = hNode.Attributes.GetNamedItem(Constant.Attribute.VALUE_ATTR);
               if (hAttr != null) dResolution = Double.Parse(hAttr.Value, System.Globalization.CultureInfo.InvariantCulture);
            }
         } 
         catch(Exception e)
         {
            throw new DapException("Error translating bounding box", e);
         }
      }

      /// <summary>
      /// Parse the image response.
      /// </summary>
      /// <param name="hDocument">The GeosoftXML response</param>
      /// <param name="hMemStream">The image data in a memory stream</param>
      public void Image( System.Xml.XmlDocument hDocument, out System.IO.MemoryStream hMemStream ) 
      {
         System.Xml.XmlNode         hNode;         
         Byte                       []bImage;

         hMemStream = new System.IO.MemoryStream();


         try 
         {
            
            // --- find the dataset edition element ---

            hNode = hDocument.SelectSingleNode("/" + Constant.Tag.GEO_XML_TAG + "/" + Constant.Tag.RESPONSE_TAG + "/" + Constant.Tag.IMAGE_TAG + "/" + Constant.Tag.PICTURE_TAG);

            if (hNode == null) throw new DapException("No picture found");

            String s = hNode.InnerText;
            bImage = Convert.FromBase64String(s);

            hMemStream.Write(bImage,0,bImage.Length);
         } 
         catch(Exception e)
         {
            throw new DapException("Error retrieving image", e);
         }
      }

      /// <summary>
      /// Parse the extract response.
      /// </summary>
      /// <param name="hDocument">The GeosoftXML response</param>
      /// <param name="hMemStream">The extracted data in a memory stream</param>
      public void ExtractData( System.Xml.XmlDocument hDocument, out System.IO.MemoryStream hMemStream ) 
      {
         hMemStream = new System.IO.MemoryStream();
         ExtractData(hDocument, hMemStream);
      }

      /// <summary>
      /// Parse the extract response.
      /// </summary>
      /// <param name="oDocument">The GeosoftXML response</param>
      /// <param name="oStream">The extracted data in a stream</param>
      public void ExtractData(System.Xml.XmlDocument oDocument, System.IO.Stream oStream)
      {
         System.Xml.XmlNode         hNode;         
         Byte                       []bExtract;         

         try 
         {
            
            // --- find the dataset edition element ---

            hNode = oDocument.SelectSingleNode("/" + Constant.Tag.GEO_XML_TAG + "/" + Constant.Tag.RESPONSE_TAG + "/" + Constant.Tag.EXTRACT_DATA_TAG);

            if (hNode == null) throw new DapException("No data found in extract");

            bExtract = Convert.FromBase64String(hNode.InnerText);

            oStream.Write(bExtract,0,bExtract.Length);
         } 
         catch(Exception e)
         {
            throw new DapException("Error retrieving extract data", e);
         }
      }

      /// <summary>
      /// Parse the extract response.
      /// </summary>
      /// <param name="oReader">The GeosoftXML response</param>
      /// <param name="oStream">The extracted data in a stream</param>
      public void ExtractData(System.Xml.XmlReader oReader, System.IO.Stream oStream)
      {
         byte  []bExtract = new byte[4096];
         if (oReader.ReadToFollowing(Constant.Tag.EXTRACT_DATA_TAG))
         {
            int iCount = 0;

            do
            {
               iCount = oReader.ReadElementContentAsBase64(bExtract, 0, 4096);
               oStream.Write(bExtract, 0, iCount);
            } while (iCount != 0);
         }
      }

      /// <summary>
      /// Get the state key
      /// </summary>
      /// <param name="hDocument">The GeosoftXML response</param>
      public string CreateClientState(System.Xml.XmlDocument hDocument) 
      {         
         System.Xml.XmlNodeList        hReturnList;         
         
         
         // --- find all the item elements located anywhere in the document ---

         hReturnList = hDocument.SelectNodes( "//" + Constant.Tag.CREATE_STATE_TAG );

         if (hReturnList != null && hReturnList.Count > 0)
         {
            System.Xml.XmlNode   oAttr;
            oAttr = hReturnList[0].Attributes.GetNamedItem(Constant.Tag.VALUE_TAG);
            return oAttr.Value;
         }
         return String.Empty;
      }

      /// <summary>
      /// Parse the list logs response.
      /// </summary>
      /// <param name="hDocument">The GeosoftXML response</param>
      /// <param name="oLogs">The list of logs</param>
      public void ListLogs( System.Xml.XmlDocument hDocument, out ArrayList oLogs ) 
      {         
         System.Xml.XmlNodeList        hItemList;
         System.Xml.XmlNode            hNode;
         
         oLogs = new ArrayList();
         
         try 
         {
            // --- find all the item elements located anywhere in the document ---

            hItemList = hDocument.SelectNodes( "//" + Constant.Tag.LOG_TAG );

            if (hItemList == null) throw new DapException("No logs found");

            // --- loop through all the datasets adding them to the list ---

            for (int i = 0; i < hItemList.Count; i++)
            {  
               System.Xml.XmlNode   hAttr;

               hNode = hItemList[i];
               hAttr = hNode.Attributes.GetNamedItem(Constant.Attribute.NAME_ATTR);

               if (hAttr != null && hAttr.Value != null)
                  oLogs.Add(hAttr.Value);
            }
         } 
         catch(Exception e)
         {
            throw new DapException("Error compiling list of logs", e);
         }         
      }
   }
}