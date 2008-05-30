using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Utility
{
#if !DEBUG
   public class AbortUtility
   {
      [DllImport("clrdump.dll", CharSet = CharSet.Unicode, SetLastError = true)]
      static extern Int32 CreateDump(Int32 ProcessId, string FileName,
          Int32 DumpType, Int32 ExcThreadId, IntPtr ExtPtrs);

      [DllImport("clrdump.dll", CharSet = CharSet.Unicode, SetLastError = true)]
      static extern Int32 RegisterFilter(string FileName, Int32 DumpType);

      [DllImport("clrdump.dll", SetLastError = true)]
      static extern Int32 UnregisterFilter();

      [DllImport("clrdump.dll")]
      static extern Int32 SetFilterOptions(Int32 Options);

      private static object ABORT_LOCK = new object();
		private static bool blAbortCreated = false;

      /// <summary>
      /// Display an error message to the user, then shut down.
      /// </summary>
      /// <param name="msg">The message (reason for shutdown) to display to the user.</param>
      public static void Abort(Exception caught, Thread curthread)
      {
			if (caught is ThreadAbortException)
			{
				return;
			}


         lock (ABORT_LOCK)
         {
				if (blAbortCreated)
				{
					Thread.CurrentThread.Abort();
				}
				else
				{
					blAbortCreated = true;
				}

            Exception e;

            try
            {
               // log the error
               Utility.Log.Write(caught);
            }
            catch
            {
               // ignore errors while trying to write the error to the log
            }

            //int i;
            e = caught;
				String errorMessages = FormatException(e) + Environment.NewLine;

            if (e.InnerException != null)
            {
               e = e.InnerException;
               do
               {
						errorMessages += FormatException(e) + Environment.NewLine;
                  e = e.InnerException;
               } while (e != null);
            }

				errorMessages += "Current thread: ";
				if (Thread.CurrentThread.IsThreadPoolThread)
				{
					errorMessages += "background worker thread" + Environment.NewLine;
				}
				else if (String.IsNullOrEmpty(Thread.CurrentThread.Name))
				{
					errorMessages += "<unnmaed thread>" + Environment.NewLine;
				}
				else
				{
					errorMessages += Thread.CurrentThread.Name + Environment.NewLine;
				}
				errorMessages += "Thread count (including this one): " + Process.GetCurrentProcess().Threads.Count;


            Abort(errorMessages);
         }
      }

      private static String FormatException(Exception e)
      {
         StringBuilder result = new StringBuilder();

			String szExceptionName = e.GetType().ToString();

			result.Append(e.GetType().ToString());
         result.Append(": ");
			result.Append(e.Message.Replace(Environment.NewLine, "; "));
         result.Append(Environment.NewLine);

         if (e.StackTrace != null)
         {
            result.Append(e.StackTrace);
            result.Append(Environment.NewLine);
         }

			if (e.Data.Count > 0)
			{
				result.Append("Additional data:");
				foreach (String szKey in e.Data.Keys)
				{
					result.Append(Environment.NewLine);
					result.Append("   ");
					result.Append(szKey);
					result.Append(": ");
					result.Append(e.Data[szKey]);
				}
				result.Append(Environment.NewLine);
			}

         return result.ToString();
      }

		public static event MethodInvoker ProgramAborting;

      /// <summary>
      /// Display an error message to the user, then shut down.
      /// </summary>
      /// <param name="msg">The message (reason for shutdown) to display to the user.</param>
      private static void Abort(string errorMessages)
      {
			Thread.CurrentThread.Priority = ThreadPriority.Highest;
			if (ProgramAborting != null)
			{
				ProgramAborting();
			}

         string strAbortLog = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
         //CreateDump(Process.GetCurrentProcess().Id, strMiniDump, 0, 0, IntPtr.Zero);
         //System.Diagnostics.Process.Start(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "clrdump.exe"), Process.GetCurrentProcess().Id.ToString() + " " + @"f:\out.dmp mid");
         
         using (StreamWriter sw = new StreamWriter(strAbortLog))
         {
            sw.Write(errorMessages);
         }
         System.Diagnostics.Process.Start(Application.ExecutablePath, "ABORT \"" + strAbortLog + "\"");
			Application.Exit();
      }
   }
#endif
}
