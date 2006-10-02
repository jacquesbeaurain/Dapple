using System;
using System.Collections.Generic;
using System.Text;
using Geosoft.DotNetTools.ErrorLogger;

namespace Geosoft.GX.DAPGetData
{
   /// <summary>
   /// Singleton class for logging errors
   /// </summary>
   public class GetDapError : ErrorLogger
   {
      protected static GetDapError m_hError = null;

      public GetDapError(string strFileName) : base(strFileName)
      {
         if (m_hError != null)
            throw new ApplicationException("GetDapError has already been constructed");
         m_hError = this;
      }

      #region Public Access
      public static GetDapError Instance
      {
         get
         {
            if (m_hError == null)
               throw new ApplicationException("GetDapError instance has not been constructed yet");
            return m_hError;
         }
      }
      #endregion

      #region Disposing Override

      protected override void Disposing()
      {
         m_hError = null;
      }

      #endregion
   }
}
