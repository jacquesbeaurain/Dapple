using System;
using System.Windows.Forms;

namespace Geosoft.DotNetTools.ErrorLogger
{
   /// <summary>
	/// Log errors to a file
	/// </summary>
	public class ErrorLogger : IDisposable
	{
      /// <summary>
      /// Name of log file
      /// </summary>
      protected string                    m_strFileName;

      /// <summary>
      /// Error stream
      /// </summary>
      protected System.IO.StreamWriter    m_hStream;      

      #region Properties
      /// <summary>
      /// Get or set the filename 
      /// </summary>
      public string FileName
      {
         get { return m_strFileName; }
         set { m_strFileName = value; }
      }
      #endregion

      #region Constructor
      /// <summary>
      /// 	<para>Initializes an instance of the <see cref="ErrorLogger"/> class.</para>
      /// </summary>
      /// <param name="strFileName">
      /// </param>
      public ErrorLogger(string strFileName)
      {
         m_strFileName = strFileName;

         try
         {
            m_hStream = new System.IO.StreamWriter(m_strFileName, false);
         }
         catch
         {
            m_hStream = null;
         }
      }      
      #endregion      

      #region Public Methods
      /// <summary>
      /// Write out log message
      /// </summary>
      /// <param name="strMessage"></param>
      public void Write(string strMessage)
      {
         try
         {
            if (m_hStream != null) 
            {
               m_hStream.WriteLine(DateTime.Now + ": " + strMessage);
               m_hStream.Flush();
            }
         } 
         catch{}
      }
      #endregion

      #region IDisposable Members

      /// <summary>
      /// Dispose of any resources
      /// </summary>
      public void Dispose()
      {
         Disposing();
         if (m_hStream != null) 
         {
            m_hStream.Flush();
            m_hStream.Close();
         }
      }

      protected virtual void Disposing()
      {
      }

      #endregion
   }
}