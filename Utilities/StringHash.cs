using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Utility
{
   public class StringHash : SHA1Managed
   {
      static readonly StringHash instance = new StringHash();
      private UnicodeEncoding UE = new UnicodeEncoding();

      // Explicit static constructor to tell C# compiler
      // not to mark type as beforefieldinit
      static StringHash()
      {
      }

      StringHash()
      {
      }

      /// <summary>
      /// Function that is safe for paths (convert some Base64 characters to more path friendly characters)
      /// </summary>
      /// <param name="input"></param>
      /// <returns></returns>
      public static string GetBase64HashForPath(string input)
      {
         //Convert the string into an array of bytes.
         byte[] stringBytes = instance.UE.GetBytes(input);
         return Convert.ToBase64String(instance.ComputeHash(stringBytes)).Replace('/', '_').Replace('+', '-').Trim('=');
      }
   }
}