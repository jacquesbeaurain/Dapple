using System;
using System.Collections.Specialized;
using System.Text.RegularExpressions;

/// <summary>
/// Command line parser.  
/// </summary>
/// <remarks>
/// Create an object from this class with the command line array
/// from the <c>Main</c> method. It will automatically parse the
/// command line arguments, find all parameters starting with -, -- or
/// / and all the values linked. Any value could be separated from the
/// parameter with a space, a : or a =. The parser also look for
/// enclosing characters like ' or " and remove them. Of course if you
/// have a value like 'Mike's house', only the first and last ' will be
/// removed.
/// </remarks>
/// <example>
/// Here is an example on how to retrieve the command line arguments.
/// <code>
///        static void Main(string[] Args)
///        {
///            CommandLineArguments cml = new CommandLineArguments(Args);
///
///            if (cml["Height"]!=null)
///                Console.WriteLine( "Height: "+CommandLine["Height"] );
///            else
///                Console.WriteLine( "Height is not defined !" );
///
///            if (cml["Width"]!=null)
///                Console.WriteLine( "Width: "+CommandLine["Width"] );
///            else
///                Console.WriteLine( "Width is not defined !" );
///        }
/// </code>
/// If a file extension has been associated with the program, and
/// the user double-clicks on the file, the filename will be added
/// as a command line argument by Windows. This is an unnamed argument
/// and must therefore be retrieved by its index value (of unnamed
/// arguments):
/// <code>
///        static void Main(string[] Args)
///        {
///            CommandLineArguments cml = new CommandLineArguments(Args);
///
///            if (cml[0]!=null)
///                Console.WriteLine( "Open file: " +CommandLine[0] );
///            else
///                Console.WriteLine( "No file to open..." );
///        }
/// </code>
/// </example>
internal class CommandLineArguments
{
   private StringDictionary namedParameters =
       new StringDictionary();
   private System.Collections.ArrayList unnamedParameters =
       new System.Collections.ArrayList();

   /// <summary>
   /// Creates a <see cref="CommandLineArguments"/> object to parse
   /// command lines.
   /// </summary>
   /// <param name="args">The command line to parse.</param>
   internal CommandLineArguments(string[] args)
   {
      Regex splitter = new Regex(@"^-{1,2}|^/|=|:",
          RegexOptions.IgnoreCase | RegexOptions.Compiled);
      Regex remover = new Regex(@"^['""]?(.*?)['""]?$",
          RegexOptions.IgnoreCase | RegexOptions.Compiled);
      string parameter = null;
      string[] parts;

      // Valid parameters forms:
      // {-,/,--}param{ ,=,:}((",')value(",'))
      // Examples: -param1 value1 --param2 /param3:"Test-:-work"
      //           /param4=happy -param5 '--=nice=--'
      foreach (string str in args)
      {
         // Do we have a parameter (starting with -, /, or --)?
         if (str.StartsWith("-") || str.StartsWith("/"))
         {
            // Look for new parameters (-,/ or --) and a possible
            // enclosed value (=,:)
            parts = splitter.Split(str, 3);
            switch (parts.Length)
            {
               // Found a value (for the last parameter found
               // (space separator))
               case 1:
                  if (parameter != null)
                  {
                     if (!namedParameters.ContainsKey(parameter))
                     {
                        parts[0] =
                            remover.Replace(parts[0], "$1");
                        namedParameters.Add(
                            parameter, parts[0]);
                     }
                     parameter = null;
                  }
                  // else Error: no parameter waiting for a value
                  // (skipped)
                  break;
               // Found just a parameter
               case 2:
                  // The last parameter is still waiting. With no
                  // value, set it to true.
                  if (parameter != null)
                  {
                     if (!namedParameters.ContainsKey(parameter))
                        namedParameters.Add(parameter, "true");
                  }
                  parameter = parts[1];
                  break;
               // parameter with enclosed value
               case 3:
                  // The last parameter is still waiting. With no
                  // value, set it to true.
                  if (parameter != null)
                  {
                     if (!namedParameters.ContainsKey(parameter))
                        namedParameters.Add(parameter, "true");
                  }
                  parameter = parts[1];
                  // Remove possible enclosing characters (",')
                  if (!namedParameters.ContainsKey(parameter))
                  {
                     parts[2] = remover.Replace(parts[2], "$1");
                     namedParameters.Add(parameter, parts[2]);
                  }
                  parameter = null;
                  break;
            }
         }
         else
         {
            unnamedParameters.Add(str);
         }
      }
      // In case a parameter is still waiting
      if (parameter != null)
      {
         if (!namedParameters.ContainsKey(parameter))
            namedParameters.Add(parameter, "true");
      }
   }



   /// <summary>
   /// Retrieves the parameter with the specified name.
   /// </summary>
   /// <param name="name">
   /// The name of the parameter. The name is case insensitive.
   /// </param>
   /// <returns>
   /// The parameter or <c>null</c> if it can not be found.
   /// </returns>
   internal string this[string name]
   {
      get { return (namedParameters[name]); }
   }




   /// <summary>
   /// Retrieves an unnamed parameter (that did not start with '-'
   /// or '/').
   /// </summary>
   /// <param name="name">The index of the unnamed parameter.</param>
   /// <returns>The unnamed parameter or <c>null</c> if it does not
   /// exist.</returns>
   /// <remarks>
   /// Primarily used to retrieve filenames which extension has been
   /// associated to the application.
   /// </remarks>
   internal string this[int index]
   {
      get
      {
         return (string)(index < unnamedParameters.Count ?
             unnamedParameters[index] :
         null);
      }
   }
}
