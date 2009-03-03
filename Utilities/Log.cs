using System;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;

namespace Utility
{
	/// <summary>
	/// Debug log functionality
	/// </summary>
	public sealed class Log
	{
		static StreamWriter logWriter;
		static string logPath;
		static string logFilePath;

        // a few standard values to facilitate logging
        public static class Levels
        {
            public const int Error = 0;
				public const int Warning = 2;
				public const int Debug = 5;
				public const int Verbose = 7;
        };
        internal static int Level;

		/// <summary>
		/// Static class (Only static members)
		/// </summary>
		private Log()
		{}

		static Log()
		{
			try
			{
#if DEBUG
                Level = 6;
#else
                Level = 4;
#endif
				logPath = DefaultSettingsDirectory();
				Directory.CreateDirectory(logPath);

				// TODO: do not hardcode logfile name?
				logFilePath = Path.Combine( logPath, "WorldWind.log" );

				logWriter = new StreamWriter(logFilePath, true);
				logWriter.AutoFlush = true;
			}
			catch (Exception caught)
			{
				throw new System.ApplicationException(String.Format("Unexpected logfile error: {0}", logFilePath), caught);
			}
		}

		// Return the full default directory path to be used for storing settings files,
		// which is also where logfiles will be stored.
		public static string DefaultSettingsDirectory() 
		{
			// Example for Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData):
			// @"C:\Documents and Settings\<user>\Application Data"
			return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + DefaultSettingsDirectorySuffix();
		}

		// Return the program-specifc part of the full default directory path to be used
		// for storing settings files, which is also where logfiles will be stored.
		internal static string DefaultSettingsDirectorySuffix() 
		{
			// Application.ProductName is set by AssemblyProduct in \WorldWind\AssembyInfo.cs
			Version ver = new Version(Application.ProductVersion);
			return string.Format(@"\{0}\{1}\{2}.{3}.{4}.{5}", Application.CompanyName, Application.ProductName, ver.Major, ver.Minor, ver.Build, ver.Revision);
		}

		public static void Write(string category, string message)
        {
            Write(Levels.Error, category, message);
        }

		/// <summary>
		/// Logs a message to the system log. All messages trigger LogEvents, but only messages with level <= Log.Level are recorded to the log file.
		/// </summary>
        /// <param name="level">Log level. 0 (highest) .. 9 (lowest). See Log.Level for filtering</param>
		/// <param name="category">1 to 4 character long tag for categorizing the log entries.
		/// If the category is longer than 4 characters it will be clipped.</param>
		/// <param name="message">The actual log messages to be written.</param>
        public static void Write(int level, string category, string message)
		{
            if (level <= Log.Level)
            {
                try
                {
                    lock (logWriter)
                    {
                        string logLine = string.Format("{0} {1} {2} {3}",
                            DateTime.Now.ToString("u"),
                            level,
                            category.PadRight(4, ' ').Substring(0, 4),
                            message);

#if DEBUG
                        System.Diagnostics.Debug.WriteLine(
                            string.Format("World Wind"+
                            //".{0} [{1}]"+
                            ": {2} {3} {4}", 
                                System.Threading.Thread.CurrentThread.Name,
                                System.Threading.Thread.CurrentThread.ManagedThreadId,
                                level,
                                category.PadRight(4, ' ').Substring(0, 4),
                                message));
#endif
                        logWriter.WriteLine(logLine);
                    }
                }
                catch (Exception caught)
                {
                    throw new System.ApplicationException(String.Format("Unexpected logging error on write(1)"), caught);
                }
            }
		}

		/// <summary>
		/// Logs a message to the system log only in debug builds.
		/// </summary>
		/// <param name="category">1 to 4 character long tag for categorizing the log entries.
		/// If the category is longer than 4 characters it will be clipped.</param>
		/// <param name="message">The actual log messages to be written.</param>
		[Conditional("DEBUG")]
		internal static void DebugWrite( string category, string message )
		{
			Debug.Write( category, message );
		}

		/// <summary>
		/// Logs a message to the system log
		/// </summary>
        public static void Write(int level, string message)
		{
            Write(level, "", message);
		}

		/// <summary>
		/// Logs a message to the system log only in debug builds.
		/// </summary>
		[Conditional("DEBUG")]
		internal static void DebugWrite( int level, string message )
		{
            Write(level, "", message);
		}

		/// <summary>
		/// Writes a log of an exception.
		/// </summary>
		/// <param name="caught"></param>
		public static void Write(Exception caught)
      {
         try
         {
            if (caught is System.Threading.ThreadAbortException)
               return;

            lock (logWriter)
            {
               string[] stacktrace = new string[0];
               if (caught.StackTrace != null)
               {
                  stacktrace = caught.StackTrace.Split('\n');
               }

               string logFileName = string.Format("DEBUG_{0}.txt", DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-ffff"));
               string logFullPath = Path.Combine(logPath, logFileName);
               using (StreamWriter sw = new StreamWriter(logFullPath, false))
               {
                  sw.WriteLine(caught.ToString());
               }

               Write(Log.Levels.Error, "caught exception: ");
               Write(Log.Levels.Error, caught.ToString());
               foreach (string line in stacktrace)
                  Write(Log.Levels.Debug, line);
            }
         }
         catch (Exception caught2)
         {
            throw new System.ApplicationException(String.Format("{0}\nUnexpected logging error on write(2)", caught.Message), caught2);
         }
      }

		/// <summary>
		/// Writes a debug log of an exception.
		/// Only executed in debug builds.
		/// </summary>
		[Conditional("DEBUG")]
		public static void DebugWrite(Exception caught)
		{
			try
			{
				if (caught is System.Threading.ThreadAbortException)
					return;

				string logFileName = string.Format("DEBUG_{0}.txt", DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") );
				string logFullPath = Path.Combine(logPath, logFileName);
				using (StreamWriter sw = new StreamWriter(logFullPath, false))
				{
					sw.WriteLine(caught.ToString());
				}
			}
			catch (Exception caught2)
			{
				throw new System.ApplicationException(String.Format("{0}\nUnexpected logging error on write(3)", caught.Message), caught2);
			}
		}
	}
}
