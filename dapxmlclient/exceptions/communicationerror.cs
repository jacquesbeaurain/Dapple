using System;

namespace Geosoft.Dap.Xml
{
   /// <summary>
   /// Represents errors that occur during the communication phase.
   /// </summary>
   public class CommunicationException : ApplicationException
   {
      /// <summary>
      /// Default constructor
      /// </summary>
      /// <param name="szMsg"></param>
      /// <param name="e"></param>
      public CommunicationException( string szMsg, Exception e ) : base(szMsg,e)
      {         
      }
   }
}
         