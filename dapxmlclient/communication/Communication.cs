using System;
using System.IO;
using System.Reflection;

namespace Geosoft.Dap.Xml
{
   /// <summary>
   /// Delegate for providing download progress feedback
   /// </summary>
   /// <param name="iBytesRead"></param>
   /// <param name="iTotalBytes"></param>
   public delegate void UpdateProgessCallback(int iBytesRead, int iTotalBytes);

   /// <summary>
   /// Transmit and recieve xml over an http connection
   /// </summary>
   public class Communication
   {
      #region Constants
      const int BUFFER_SIZE = 2048;
      #endregion
      
      #region Member Variables
#if !DAPPLE 
      private bool m_bTask;
#endif
      private bool m_bSecure;
      #endregion

      #region Properties
      /// <summary>
      /// Get/Set wether to send this encypted or not
      /// </summary>
      public bool Secure
      {
         get { return m_bSecure; }
         set { m_bSecure = value; }
      }

#if !DAPPLE 
      /// <summary>
      /// Get/Set wether to send this through task
      /// </summary>
      public bool Task
      {
         get { return m_bTask; }
         set { m_bTask = value; }
      }
#endif

      #endregion

      /// <summary>
      /// Default constructor
      /// </summary>
      /// <param name="bTask"></param>
      /// <param name="bSecure"></param>
      public Communication(bool bTask, bool bSecure)
      {
#if !DAPPLE 
         m_bTask = bTask;
#else
         if (bTask)
            throw new ApplicationException("Task based communication not supported in this version of dapxmlclient");
#endif
         m_bSecure = bSecure;
      }

      /// <summary>
      /// Send an xml request on the wire, get the response and parse it into an XML document
      /// </summary>
      /// <param name="hRequestDocument"></param>
      /// <param name="szUrl"></param>
      /// <param name="progressCallBack">Progress handler (may be null)</param>
      /// <returns></returns>
      public System.Xml.XmlDocument Send(string szUrl, System.Xml.XmlDocument hRequestDocument, UpdateProgessCallback progressCallBack)
      {
#if !DAPPLE 
         if (m_bTask)
         {
            return SendTask(szUrl, hRequestDocument);
         } 
#endif
         return SendHttp(szUrl, hRequestDocument, progressCallBack);
      }

#if !DAPPLE 
      /// <summary>
      /// Send an xml request through task
      /// </summary>
      /// <param name="szUrl"></param>
      /// <param name="hRequestDocument"></param>
      /// <returns></returns>
      protected System.Xml.XmlDocument SendTask(string szUrl, System.Xml.XmlDocument hRequestDocument)
      {
         string   strXMLResponseFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

         try
         {
            System.Xml.XmlDocument  hResponseDocument;
            Geosoft.GXNet.CDAP      hDAP;
            string                  strXML;               

            // --- remove the /ois.dll?geosoft_xml from the url ---

            int iIndex = szUrl.LastIndexOf("/");
            szUrl = szUrl.Substring(0, iIndex);

            hDAP = Geosoft.GXNet.CDAP.Create(szUrl, "Send XML Request");
            strXML = hRequestDocument.InnerXml;

            hDAP.sExecuteGeosoftXML(strXML, strXML.Length, strXMLResponseFile);

            hDAP.Dispose();

            hResponseDocument = new System.Xml.XmlDocument();
            hResponseDocument.Load(strXMLResponseFile);               

            return hResponseDocument;
         } 
         finally
         {
            System.IO.File.Delete(strXMLResponseFile);
         }         
      }
#endif

      /// <summary>
      /// Send an xml request on the wire, get the response and parse it into an XML document
      /// </summary>
      /// <param name="hRequestDocument"></param>
      /// <param name="szUrl"></param>
      /// <param name="progressCallBack"></param>
      /// <returns></returns>
      protected System.Xml.XmlDocument SendHttp(string szUrl, System.Xml.XmlDocument hRequestDocument, UpdateProgessCallback progressCallBack)
      {
         byte[]						   byte1;
         System.Xml.XmlDocument		hResponseDocument = null;

         System.IO.StringWriter		hRequest = null;
         System.IO.Stream			   hRequestStream = null;
         System.Xml.XmlTextReader	hResponseXmlStream = null;

         System.Net.HttpWebRequest	cHttpWReq = null;
         System.Net.HttpWebResponse	cHttpWResp = null;

         try 
         {            
            // --- Initialize all the required streams to write out the xml request ---

            hRequest = new System.IO.StringWriter();           


            // --- Create a HTTP Request to the Dap Server and HTTP Response Objects---

            cHttpWReq = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(szUrl);
            cHttpWReq.Pipelined = false;

            // --- Encode the document into ascii ---

            System.Text.UTF8Encoding	hRequestEncoding = new System.Text.UTF8Encoding();

            hRequestDocument.Save(hRequest);
            byte1 = hRequestEncoding.GetBytes(hRequest.GetStringBuilder().ToString());


            // --- Setup the HTTP Request ---

            cHttpWReq.Method = "POST";
            cHttpWReq.ContentType = "application/x-www-form-urlencoded";
            cHttpWReq.ContentLength = hRequest.GetStringBuilder().Length;


            // --- Serialize the XML document onto the wire ---
            
            hRequestStream = cHttpWReq.GetRequestStream();
            hRequestStream.Write( byte1, 0, byte1.Length );
            hRequestStream.Close();

            
            // --- Turn off connection keep-alives. ---
            
            cHttpWReq.KeepAlive = false;


            // --- Get the response ---
            
            cHttpWResp = (System.Net.HttpWebResponse)cHttpWReq.GetResponse();
            
            Stream strmResponse = cHttpWResp.GetResponseStream();
            MemoryStream strmMem = new MemoryStream(BUFFER_SIZE);
            // --- Ask the server for the file size and store it (this does not return a valid value at this stage) ---
            int iFileSize = (int)cHttpWResp.ContentLength;
            // --- For no total size known provide rolling feedback estimate ---
            bool bRolling = iFileSize == -1;
            if (bRolling)
               iFileSize = BUFFER_SIZE*2;

            // --- It will store the current number of bytes we retrieved from the server ---
            int bytesSize = 0;
            // --- A buffer for storing and writing the data retrieved from the server ---
            byte[] downBuffer = new byte[BUFFER_SIZE];
            // --- Loop through the buffer until the buffer is empty ---
            while ((bytesSize = strmResponse.Read(downBuffer, 0, downBuffer.Length)) > 0)
            {
               // --- Write the data from the buffer to the local hard drive ---
               strmMem.Write(downBuffer, 0, bytesSize);
               
               // --- Was progress requested? ---
               if (progressCallBack != null)
               {
                  if (strmMem.Length >= iFileSize && bRolling)
                     iFileSize = iFileSize*2;
                  progressCallBack((int) strmMem.Length, iFileSize);
               }
            }
            if (progressCallBack != null)
               progressCallBack((int)strmMem.Length, (int)strmMem.Length);
               
            strmMem.Seek(0, SeekOrigin.Begin);
            hResponseXmlStream = new System.Xml.XmlTextReader(strmMem);				
            hResponseXmlStream.WhitespaceHandling = System.Xml.WhitespaceHandling.None;                       
            

            // --- Load Response into XML Document ---
            
            hResponseDocument = new System.Xml.XmlDocument();
            hResponseDocument.Load(hResponseXmlStream);


            // --- search for an error ---

            System.Xml.XmlNodeList hNodeList = hResponseDocument.SelectNodes("//" + Geosoft.Dap.Xml.Common.Constant.Tag.ERROR_TAG);
            if (hNodeList.Count >= 1)
            {               
               System.Xml.XmlNode  hNode = hNodeList[0];
               
               throw new DapException(hNode.InnerText);
            }
         } 
         catch( Exception e ) 
         {
            hResponseDocument = null;
            throw e;
         } 
         finally
         {
            if (hResponseXmlStream != null) hResponseXmlStream.Close();
            if (cHttpWResp != null) cHttpWResp.Close();
         }
         return hResponseDocument;
      } 

      /// <summary>
      /// Send an xml request onto the wire and return a pointer to the actual response stream. Used when you have a long response
      /// that you do not want to load into an XML document and store all in memory. 
      /// Note: The response stream is not closed. That is left up to the caller to dispose of
      /// </summary>
      /// <param name="hRequestDocument"></param>
      /// <param name="szUrl"></param>
      /// <returns></returns>
      public System.Xml.XmlTextReader Send2(string szUrl, System.Xml.XmlDocument hRequestDocument)
      {
         System.IO.Stream			   hRequestStream = null;
         System.Xml.XmlTextReader	hResponseXmlStream = null;;
         System.Net.HttpWebRequest	cHttpWReq = null;
         System.Net.HttpWebResponse	cHttpWResp = null;
         byte[]						   byte1;

         try 
         {
            // --- Initialize all the required streams to write out the xml request ---

            System.IO.StringWriter		hRequest = new System.IO.StringWriter();
            

            // --- Create a HTTP Request to the Dap Server and HTTP Response Objects---

            cHttpWReq = (System.Net.HttpWebRequest)System.Net.WebRequest.Create( szUrl );


            // --- Encode the document into ascii ---

            System.Text.ASCIIEncoding	hRequestEncoding = new System.Text.ASCIIEncoding();

            hRequestDocument.Save( hRequest );
            byte1 = hRequestEncoding.GetBytes(hRequest.GetStringBuilder().ToString());


            // --- Setup the HTTP Request ---

            cHttpWReq.Method = "POST";
            cHttpWReq.ContentType = "application/x-www-form-urlencoded";
            cHttpWReq.ContentLength = hRequest.GetStringBuilder().Length;


            // --- Serialize the XML document onto the wire ---

            hRequestStream = cHttpWReq.GetRequestStream();
            hRequestStream.Write( byte1, 0, byte1.Length );
            hRequestStream.Close();


            // --- Turn off connection keep-alives. ---

            cHttpWReq.KeepAlive = false;


            // --- Get the response ---

            cHttpWResp = (System.Net.HttpWebResponse)cHttpWReq.GetResponse();

            hResponseXmlStream = new System.Xml.XmlTextReader( cHttpWResp.GetResponseStream() );				
         } 
         catch( Exception e ) 
         {
            hResponseXmlStream = null;
            throw e;
         }         
         return hResponseXmlStream;
      } 
   }
}
