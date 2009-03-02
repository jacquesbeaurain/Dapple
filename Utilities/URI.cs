using System;
using System.Collections.Specialized;
using System.Text;
using System.Web;

namespace Utility
{
	// TODO: Delete this class.  System.Uri provides the same functionality.
	public static class URI
   {
      static string StripSchemeFromURI(string strScheme, string strURI)
      {
         string strPre = strScheme + "://";
         if (!strURI.StartsWith(strPre))
            throw new ApplicationException("The URI: \"" + strURI + "\" is not a valid URI for the " + strScheme + " protocol.");

         return strURI.Replace(strPre, "");
      }

      internal static string HostFromURI(string strScheme, string strURI)
      {
         string strHost = StripSchemeFromURI(strScheme, strURI);

         if (strHost.IndexOf("?") != -1)
            strHost = strHost.Substring(0, strHost.IndexOf("?"));

         if (strHost.IndexOf("/") != -1)
            strHost = strHost.Substring(0, strHost.IndexOf("/"));

         return strHost;
      }

      internal static string PathFromURI(string strScheme, string strURI)
      {
         string strPath = StripSchemeFromURI(strScheme, strURI);

         if (strPath.IndexOf("?") != -1)
            strPath = strPath.Substring(0, strPath.IndexOf("?"));

         if (strPath.IndexOf("/") != -1)
            strPath = strPath.Substring(strPath.IndexOf("/") + 1);
         else
            return String.Empty;

         return strPath;
      }

      internal static string QueryFromURI(string strScheme, string strURI)
      {
         string strQuery = StripSchemeFromURI(strScheme, strURI);

         if (strQuery.IndexOf("?") != -1)
            return strQuery.Substring(strQuery.IndexOf("?") + 1);
         else
            return String.Empty;
      }

      public static NameValueCollection ParseURI(string strScheme, string strURI, ref string strHost, ref string strPath)
      {
         strHost = HostFromURI(strScheme, strURI);
         strPath = PathFromURI(strScheme, strURI);
         return HttpUtility.ParseQueryString(QueryFromURI(strScheme, strURI));
      }

		public static string CreateURI(string strScheme, string strHost, string strPath, NameValueCollection queryColl)
      {
         bool bFirst = true;
         string strURI = strScheme + "://" + strHost + "/";
         if (!String.IsNullOrEmpty(strPath))
            strURI += strPath;

         string strQuery = String.Empty;
         if (queryColl != null)
         {
            for (int i = 0; i < queryColl.Count; i++)
            {
               if (!bFirst)
                  strQuery += "&";
               else
                  bFirst = false;

               strQuery += queryColl.GetKey(i) + "=" + HttpUtility.UrlEncode(queryColl.Get(i));
            }
         }

         if (strQuery != String.Empty)
            strURI += "?" + strQuery;

         return strURI;
      }
   }
}
