using System;
using Geosoft.Dap.Xml.Common;

namespace Geosoft.Dap.Common
{
   #region Enums
   /// <summary>
   /// The list of supported commands in GeosoftXML
   /// </summary>
   public enum Commands
   {
      /// <summary>
      /// Dataset edition
      /// </summary>
      DATASET_EDITION = 0,
      
      /// <summary>
      /// Metadata
      /// </summary>
      METADATA,

      /// <summary>
      /// Image
      /// </summary>
      IMAGE,

      /// <summary>
      /// Extract
      /// </summary>
      EXTRACT,

      /// <summary>
      /// Extract cancel
      /// </summary>
      EXTRACT_CANCEL,

      /// <summary>
      /// Extract status
      /// </summary>
      EXTRACT_STATUS,

      /// <summary>
      /// Extract data
      /// </summary>
      EXTRACT_DATA,

      /// <summary>
      /// Default resolution
      /// </summary>
      DEFAULT_RESOLUTION
   }
   #endregion

   /// <summary>
   /// Store the capabilites response and carry out any functions required on the capabilities
   /// </summary>
   public class Capabilities 
   {
      #region Member Variables
      private System.Xml.XmlDocument  m_hCapabilities;
      private static String           []m_szCommandNames = { "dataset_edition", "metadata", "image", "extract", "extract_cancel", "extract_status", "extract_data", "default_resolution" };
      #endregion

      #region Properties
      /// <summary>
      /// Get or set the capabilities response
      /// </summary>
      public System.Xml.XmlDocument Document
      {
         set { m_hCapabilities = value; }
         get { return m_hCapabilities; }
      }
      #endregion

      #region Constructor
      /// <summary>
      /// Default constructor
      /// </summary>
      /// <param name="hCapabilities">The capabilities response</param>
      public Capabilities (System.Xml.XmlDocument hCapabilities)
      {
         m_hCapabilities = hCapabilities;
      }
      #endregion

      #region Member Functions
      /// <summary>
      /// Inform the caller whether this command is support by this type
      /// </summary>
      /// <param name="szType">The dataset type</param>
      /// <param name="eCommand">The command to see if it is supported</param>
      /// <returns>true if supported, false otherwise</returns>
      public bool CommandSupported( string szType, Commands eCommand ) 
      {
         System.Xml.XmlNode   hNode;

         hNode = FindCommand( szType, eCommand );

         if (hNode == null) 
         {
            return false;
         }
         return true;
      }			

      /// <summary>
      /// Get the list of possible values for a particular enum parameter
      /// </summary>
      /// <param name="szType">The dataset type</param>
      /// <param name="eCommand">The command to get the list of values for</param>
      /// <returns>The list of values</returns>
      public System.Collections.ArrayList CommandFormat( string szType, Commands eCommand ) 
      {
         System.Collections.ArrayList	hArrayList = new System.Collections.ArrayList();
         System.Xml.XmlNode			   hNode;
         System.Xml.XmlNode            hAttr;

         hNode = FindCommand( szType, eCommand );
         if (hNode != null) 
         {  
            System.Xml.XmlNode hFormat = hNode.FirstChild;
            System.Xml.XmlNode hParameter = hFormat.FirstChild;
            foreach (System.Xml.XmlNode hValueNode in hParameter.ChildNodes) 
            {
               hAttr = hValueNode.Attributes.GetNamedItem( "name" );
               if (hAttr != null) 
               {
                  hArrayList.Add(hAttr.Value);
               }
            }          
         }
         return hArrayList;
      }

      /// <summary>
      /// Search through the capabilities document to find the command
      /// </summary>
      /// <param name="szType">The dataset type</param>
      /// <param name="eCommand">The command to find</param>
      /// <returns>The node in the capabilities document representing this dataset type and command; null if not found</returns>
      protected System.Xml.XmlNode  FindCommand( string szType, Commands eCommand )
      {
         System.Xml.XmlNodeList			hNodeList;
         System.Xml.XmlNode            hFoundNode = null;

         hNodeList =  m_hCapabilities.SelectNodes("/" + Constant.Tag.GEO_XML_TAG + "/" + Constant.Tag.RESPONSE_TAG + "/" + Constant.Tag.CAPABILITIES_TAG + "/" + Constant.Tag.DATASET_TYPE_TAG);
         foreach (System.Xml.XmlNode hNode in hNodeList) 
         {
            System.Xml.XmlNode hAttr = hNode.Attributes.GetNamedItem( "name" );
            if (hAttr != null && String.Compare(hAttr.Value, szType, true) == 0) 
            {
               System.Xml.XmlNodeList	hCommandList = hNode.SelectNodes(Constant.Tag.COMMANDS_TAG + "/" + Constant.Tag.COMMAND_TAG);
               foreach (System.Xml.XmlNode hCommandNode in hCommandList) 
               {
                  hAttr = hCommandNode.Attributes.GetNamedItem( "name" );
                  if (hAttr != null && String.Compare(hAttr.Value, m_szCommandNames[Convert.ToInt32(eCommand)]) == 0) 
                  {
                     hFoundNode = hCommandNode;
                     break;                     
                  }
               }
               break;	
            }
         }
         return hFoundNode;
      }
      #endregion
   }
}