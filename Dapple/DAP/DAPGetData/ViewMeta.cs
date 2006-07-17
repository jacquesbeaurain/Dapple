using System;

namespace Geosoft.GX.DAPGetData
{
   /// <summary>
   /// Arguments for when we want to view the meta for a particular dataset
   /// </summary>
   public class ViewMetaArgs : EventArgs
   {
      #region Member Variables
      /// <summary>
      /// Server Name
      /// </summary>
      protected string m_strServerName;

      /// <summary>
      /// Server Url
      /// </summary>
      protected string m_strServerUrl;
      #endregion

      #region Properties
      /// <summary>
      /// Get/Set the server name
      /// </summary>
      public string Name
      {
         get { return m_strServerName; }
         set { m_strServerName = value; }
      }

      /// <summary>
      /// Get/Set the server url
      /// </summary>
      public string Url
      {
         get { return m_strServerUrl; }
         set { m_strServerUrl = value; }
      }
      #endregion

      #region Constructor
      /// <summary>
      /// Default constructor
      /// </summary>
      /// <param name="strServerUrl"></param>
      /// <param name="strServerName"></param>      
      public ViewMetaArgs(string strServerUrl, string strServerName)
      {
         Url = strServerUrl;
         Name = strServerName;
      }      

      /// <summary>
      /// Default constructor
      /// </summary>
      public ViewMetaArgs()
      {
         Url = string.Empty;
         Name = string.Empty;
      }
      #endregion
   }

   /// <summary>
   /// Server select event handler
   /// </summary>
   public delegate void ViewMetaHandler(object sender, ViewMetaArgs e);   
}
