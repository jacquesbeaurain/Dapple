using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;

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

      private static object abortsync = new object();

      /// <summary>
      /// Display an error message to the user, then shut down.
      /// </summary>
      /// <param name="msg">The message (reason for shutdown) to display to the user.</param>
      public static void Abort(Exception caught, Thread curthread)
      {
         lock (abortsync)
         {
            Exception e;
            string errorMessages;

            // suspend other threads
            Process currentProcess = Process.GetCurrentProcess();
            /*
            foreach (ProcessThread thread in currentProcess.Threads)
            {
               if (thread.Id != curthread.ManagedThreadId)
                  thread.ThreadState = System.Diagnostics.ThreadState..Suspend();
            }
            */
            try
            {
               // log the error
               Utility.Log.Write(caught);
            }
            catch
            {
               // ignore errors while trying to write the error to the log
            }
            finally
            {
               //int i;
               e = caught;
               errorMessages = "The following error(s) occurred:";
               do
               {
                  errorMessages += "\r\n" + e.Message;
                  e = e.InnerException;
               }
               while (e != null);

               errorMessages += "\r\n\r\nAt:\r\n";
               if (caught.StackTrace != null)
                  errorMessages += caught.StackTrace;
               else
                  errorMessages += "Unknown";

               errorMessages += "\r\n\r\n" + (currentProcess.Threads.Count - 1).ToString() + " other threads :\r\n\r\n";
               /* i = 0;
                foreach (ProcessThread thread in currentProcess.Threads)
                {
                   if (thread != curthread)
                   {
                      errorMessages += "--------------- " + i.ToString() + " ---------------\r\n";
                      errorMessages += "\r\n\r\n";
                   }
                }
                */
               Abort(errorMessages);
            }
         }
      }

      /// <summary>
      /// Display an error message to the user, then shut down.
      /// </summary>
      /// <param name="msg">The message (reason for shutdown) to display to the user.</param>
      private static void Abort(string errorMessages)
      {
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
