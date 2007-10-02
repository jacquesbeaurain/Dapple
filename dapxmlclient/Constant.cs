using System;

namespace Geosoft.Dap.Xml.Common
{
   /// <summary>
   /// GeosoftXML Constants 
   /// </summary>
   public class Constant
   {
      /// <summary>
      /// The extract status
      /// </summary>
      public enum ExtractStatus
      {
         /// <summary></summary>
         IN_PROGRESS = 0,

         /// <summary></summary>
         COMPLETED,

         /// <summary></summary>
         CANCELLED,

         /// <summary></summary>
         UNKNOWN
      }


      /// <summary>
      /// List of request types
      /// </summary>
      public enum Request
      {
         /// <summary></summary>
         CONFIGURATION,
         /// <summary></summary>
         CAPABILITIES,
         /// <summary></summary>
         PROPERTIES,
         /// <summary></summary>
         CATALOG,
         /// <summary></summary>
         CATALOG_EDITION,
         /// <summary></summary>
         DATASET_EDITION,
         /// <summary></summary>
         KEYWORDS,
         /// <summary></summary>
         META,
         /// <summary></summary>
         IMAGE,
         /// <summary></summary>
         DISCLAIMER,
         /// <summary></summary>
         TRANSLATE,
         /// <summary></summary>
         EXTRACT,
         /// <summary></summary>
         DEFAULT_RESOLUTION,
         /// <summary></summary>
         SUPPORTED_COORDINATE_SYSTEMS,
         /// <summary></summary>
         OM_EXTRACT
      };

      /// <summary>
      /// List of urls for each type
      /// </summary>
      public static string []ServerV1 = new String[]
      {
         "/DMS/ois.dll",
         "/DMS/ois.dll",
         "/DMS/ois.dll",
         "/DMS/ois.dll",
         "/DMS/ois.dll",
         "/DMS/ois.dll",
         "/DMS/ois.dll",
         "/DMS/ois.dll",
         "/DMS/ois.dll",
         "/DMS/ois.dll",
         "/DMS/ois.dll",
         "/DFS/ois.dll",
         "/DMS/ois.dll",
         "/DMS/ois.dll",
         "/DDS/ois.dll",
      };

      /// <summary>
      /// List of urls for each type
      /// </summary>
      public static string []ServerV1_1 = new String[]
      {
         "/DAP/ois.dll",
         "/DAP/ois.dll",
         "/DAP/ois.dll",
         "/DAP/ois.dll",
         "/DAP/ois.dll",
         "/DAP/ois.dll",
         "/DAP/ois.dll",
         "/DAP/ois.dll",
         "/DAP/ois.dll",
         "/DAP/ois.dll",
         "/DAP/ois.dll",
         "/DAP/ois.dll",
         "/DAP/ois.dll",
         "/DAP/ois.dll",
         "/DAP/ois.dll",
      };
      
      /// <summary>
      /// The current GeosoftXML version
      /// </summary>
      public static string []XmlVersion = new String[]
      {
         "1.0",
         "1.1"
      };

      /// <summary>
      /// The path to the secure dap server
      /// </summary>
      public static string RequestXmlSecure = "?GeosoftS_XML";

      /// <summary>
      /// The path to an unsecure dap server
      /// </summary>
      public static string RequestXmlNormal = "?Geosoft_XML";

      /// <summary>
      /// The list of GeosoftXML tags
      /// </summary>
      public class Tag 
      {
         /// <summary></summary>
         public const string REQUEST_TAG						= "request";

         /// <summary></summary>
         public const string RESPONSE_TAG					   = "response";

         /// <summary></summary>
         public const string GEO_XML_TAG						= "geosoft_xml";

         /// <summary></summary>
         public const string COLLECTION_TAG					= "collection";
        
         /// <summary></summary>
         public const string DATASET_TAG					   = "dataset";

         /// <summary></summary>
         public const string LAYERS_TAG						= "layers";

         /// <summary></summary>
         public const string COORDINATE_SYSTEM_TAG			= "coordinate_system";

         /// <summary></summary>
         public const string RESOLUTION_TAG					= "resolution";

         /// <summary></summary>
         public const string BOUNDING_BOX_TAG				= "bounding_box";
         
         /// <summary></summary>
         public const string FORMAT_TAG						= "format";

         /// <summary></summary>
         public const string IMAGE_TAG						   = "image";

         /// <summary></summary>
         public const string GET_TILE_TAG                = "get_tile";

         /// <summary></summary>
         public const string LEGEND_TAG                  = "legend";

         /// <summary></summary>
         public const string TRANSLATE_COORDINATES_TAG	= "translate_coordinates";

         /// <summary></summary>
         public const string TRANSLATE_BOUNDING_BOX_TAG	= "translate_bounding_box";

         /// <summary></summary>
         public const string DEFAULT_RESOLUTION_TAG		= "default_resolution";

         /// <summary></summary>
         public const string INPUT_TAG						   = "input";

         /// <summary></summary>
         public const string OUTPUT_TAG						= "output";

         /// <summary></summary>
         public const string POINT_TAG						   = "point";

         /// <summary></summary>
         public const string COORDINATE_SYSTEM_LIST_TAG	= "coordinate_system_list";

         /// <summary></summary>
         public const string ATTRIBUTE_TAG					= "attribute";

         /// <summary></summary>
         public const string EXTRACT_TAG						= "extract";

         /// <summary></summary>
         public const string EXTRACT_STATUS_TAG				= "extract_status";

         /// <summary></summary>
         public const string EXTRACT_DATA_TAG				= "extract_data";

         /// <summary></summary>
         public const string EXTRACT_CANCEL_TAG				= "extract_cancel";

         /// <summary></summary>
         public const string CATALOG_TAG						= "catalog";

         /// <summary></summary>
         public const string CATALOG_HIERARCHY_TAG       = "catalog_hierarchy";

         /// <summary></summary>
         public const string ITEM_TAG						   = "item";

         /// <summary></summary>
         public const string METADATA_TAG					   = "metadata";

         /// <summary></summary>
         public const string VALUE_TAG						   = "value";

         /// <summary></summary>
         public const string FILTER_TAG						= "filter";

         /// <summary></summary>
         public const string CATALOG_EDITION_TAG			= "catalog_edition";

         /// <summary></summary>
         public const string DATASET_EDITION_TAG			= "dataset_edition";

         /// <summary></summary>
         public const string CAPABILITIES_TAG				= "capabilities";

         /// <summary></summary>
         public const string PROPERTIES_TAG			   	= "properties";

         /// <summary></summary>
         public const string COUNT_TAG				         = "count";

         /// <summary></summary>
         public const string KEY_TAG				         = "key";

         /// <summary></summary>
         public const string STATUS_TAG			         = "status";

         /// <summary></summary>
         public const string PICTURE_TAG			         = "picture";

         /// <summary></summary>
         public const string DATASET_TYPE_TAG			   = "dataset_type";

         /// <summary></summary>
         public const string COMMANDS_TAG			         = "commands";

         /// <summary></summary>
         public const string COMMAND_TAG			         = "command";

         /// <summary></summary>
         public const string NATIVE_TAG			         = "native";

         /// <summary></summary>
         public const string DATASETS_TAG	   	         = "datasets";

         /// <summary></summary>
         public const string CONFIGURATION_TAG 	         = "configuration";

         /// <summary></summary>
         public const string SERVER_CONFIGURATION_TAG 	= "server_configuration";

         /// <summary></summary>
         public const string UPDATE_SERVER_CONFIGURATION_TAG 	= "update_server_configuration";

         /// <summary></summary>
         public const string CLASS_TAG 	               = "class";

         /// <summary></summary>
         public const string TABLE_TAG 	               = "table";      
   
         /// <summary></summary>
         public const string META_TAG  	               = "meta";               

         /// <summary></summary>
         public const string ERROR_TAG  	               = "error";               

         /// <summary></summary>
         public const string KEYWORDS_TAG  	            = "keywords";

         /// <summary></summary>
         public const string LOG_TAG  	                  = "log";

         /// <summary></summary>
         public const string CLEAR_LOG_TAG  	            = "clear_log";

         /// <summary></summary>
         public const string LIST_LOG_TAG  	            = "list_log";

         /// <summary></summary>
         public const string AUTHENTICATE_TAG            = "authenticate";

         /// <summary></summary>
         public const string REFRESH_CATALOG_TAG         = "refresh_catalog";       
  
         /// <summary></summary>
         public const string CREATE_STATE_TAG            = "create_state";

         /// <summary></summary>
         public const string DESTROY_STATE_TAG            = "destroy_state";

         /// <summary></summary>
         public const string DISCLAIMERS_TAG = "disclaimers";
      }


      /// <summary>
      /// The list of GeosoftXML attributes
      /// </summary>
      public class Attribute 
      {
         /// <summary></summary>
         public const string VERSION_ATTR		   = "version";

         /// <summary></summary>
         public const string HANDLE_ATTR			= "handle";

         /// <summary></summary>
         public const string NAME_ATTR			   = "name";

         /// <summary></summary>
         public const string PATH_ATTR			   = "path";

         /// <summary></summary>
         public const string TYPE_ATTR			   = "type";

         /// <summary></summary>
         public const string VALUE_ATTR			= "value";

         /// <summary></summary>
         public const string MAX_DEPTH_ATTR		= "max_depth";

         /// <summary></summary>
         public const string MAX_RESULTS_ATTR	= "max_results";

         /// <summary></summary>
         public const string DEPTH_ATTR 	      = "depth";

         /// <summary></summary>
         public const string ITEMS_RETURNED_ATTR	= "items_returned";

         /// <summary></summary>
         public const string MAX_X_ATTR			= "maxX";

         /// <summary></summary>
         public const string MAX_Y_ATTR			= "maxY";

         /// <summary></summary>
         public const string MIN_X_ATTR			= "minX";

         /// <summary></summary>
         public const string MIN_Y_ATTR			= "minY";

         /// <summary></summary>
         public const string HEIGHT_ATTR			= "height";

         /// <summary></summary>
         public const string WIDTH_ATTR			= "width";

         /// <summary></summary>
         public const string DATUM_ATTR			= "datum";

         /// <summary></summary>
         public const string PROJECTION_ATTR		= "projection";

         /// <summary></summary>
         public const string UNITS_ATTR			= "units";

         /// <summary></summary>
         public const string LOCAL_DATUM_ATTR	= "local_datum";

         /// <summary></summary>
         public const string ESRI_ATTR          = "esri";

         /// <summary></summary>
         public const string BACKGROUND_ATTR		= "background";

         /// <summary></summary>
         public const string TRANSPARENT_ATTR	= "transparent";

         /// <summary></summary>
         public const string X_ATTR				   = "x";

         /// <summary></summary>
         public const string Y_ATTR				   = "y";

         /// <summary></summary>
         public const string Z_ATTR				   = "z";

         /// <summary></summary>
         public const string LIST_TYPE_ATTR		= "list_type";

         /// <summary></summary>
         public const string KEY_ATTR			   = "key";

         /// <summary></summary>
         public const string INDEX_ATTR	      = "index";

         /// <summary></summary>
         public const string KEYWORDS_ATTR		= "keywords";

         /// <summary></summary>
         public const string TITLE_ATTR         = "title";

         /// <summary></summary>
         public const string EDITION_ATTR       = "edition";

         /// <summary></summary>
         public const string PROGRESS_ATTR      = "progress";

         /// <summary></summary>
         public const string STATUS_ATTR        = "status";

         /// <summary></summary>
         public const string COUNT_ATTR         = "count";

         /// <summary></summary>
         public const string FORMAT_ATTR        = "format";

         /// <summary></summary>
         public const string RESOLUTION_ATTR    = "resolution";

         /// <summary></summary>
         public const string BASE_MAP_ATTR  	   = "base_map";      

         /// <summary></summary>
         public const string INDEX_MAP_ATTR     = "index_map";

         /// <summary></summary>
         public const string TOKEN_ATTR     = "secure_token";

         /// <summary></summary>
         public const string PASSWORD_ATTR      = "password";         

         /// <summary></summary>
         public const string DATE_ATTR          = "date";

         /// <summary></summary>
         public const string LAYER_ATTR = "layer";

         /// <summary></summary>
         public const string ROW_ATTR = "row";

         /// <summary></summary>
         public const string COLUMN_ATTR = "column";

         /// <summary></summary>
         public const string LEVEL_ATTR = "level";
      }
   }
}
