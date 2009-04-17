using System;
using System.Collections.Generic;
using System.Text;
using Geosoft.DotNetTools.ErrorLogger;

namespace Geosoft.GX.DAPGetData
{
   /// <summary>
   /// Singleton class for logging errors
   /// </summary>
   internal class GetDapError : ErrorLogger
   {
      protected static GetDapError s_hError = null;

      internal GetDapError(string strFileName) : base(strFileName)
      {
         if (s_hError != null)
            throw new ApplicationException("GetDapError has already been constructed");
         s_hError = this;
      }

      #region Public Access
      internal static GetDapError Instance
      {
         get
         {
            if (s_hError == null)
               throw new ApplicationException("GetDapError instance has not been constructed yet");
            return s_hError;
         }
      }
      #endregion

      #region Disposing Override

      protected override void Disposing()
      {
         s_hError = null;
      }

      #endregion
   }
}
