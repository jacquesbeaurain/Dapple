using System;
using System.Windows.Forms;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using MWA.Progress;

namespace Utility
{
   public class FileSystem
   {
      class ProgressInfo
      {
         public ProgressWindow progress;
         public string strOrigFolder;
         public string strFolder;
         public long lTotalSize;
         public long lCounter;

         public ProgressInfo()
         {
         }
      }

      public static void DeleteFolderGUI(IWin32Window parent, string strFolder, string strTitle)
      {
         ProgressInfo pi = new ProgressInfo();
         pi.progress = new ProgressWindow();
         pi.strOrigFolder = pi.strFolder = strFolder;
         pi.lCounter = 0;
         pi.lTotalSize = 0;
         pi.progress.Text = strTitle;
         ThreadPool.QueueUserWorkItem(new WaitCallback(DeleteFolderRecursive), pi);
         pi.progress.ShowDialog(parent);
      }

      private static void DeleteFolderRecursive(object info)
      {
         bool bMain = false;
         ProgressInfo pi = (ProgressInfo) info;
         try
         {
            if (pi.lCounter == 0)
            {
               if (Directory.Exists(pi.strFolder))
               {
                  Cursor.Current = Cursors.WaitCursor;
                  SizeFolderContentsRecursive(pi.strFolder, ref pi.lTotalSize);
                  Cursor.Current = Cursors.Default;

                  pi.progress.Begin(0, (int) (pi.lTotalSize / 1024));
                  pi.progress.SetText("Deleting folder " + pi.strOrigFolder);
                  pi.lCounter = 1;
               }
               bMain = true;
            }

            if (Directory.Exists(pi.strFolder))
            {
               DirectoryInfo di = new DirectoryInfo(pi.strFolder);
               FileInfo[] fis = di.GetFiles();
               foreach (FileInfo fi in fis)
               {
                  try
                  {
                     pi.progress.StepTo((int)((pi.lCounter - 1) / 1024));
                     if (pi.progress.IsAborting)
                     {
                        return;
                     }
                     pi.lCounter += fi.Length;
                     fi.Delete();
                     if (pi.progress.IsAborting)
                     {
                        return;
                     }
                  }
                  catch
                  {
                  }
               }
               DirectoryInfo[] dis = di.GetDirectories();
               foreach (DirectoryInfo disub in dis)
               {
                  try
                  {
                     if (pi.progress.IsAborting)
                     {
                        return;
                     }
                     pi.strFolder = disub.FullName;
                     DeleteFolderRecursive(pi);
                     if (pi.progress.IsAborting)
                     {
                        return;
                     }
                  }
                  catch
                  {
                  }
               }
            }
         }
         catch (ThreadAbortException)
         {
         }
         catch (ThreadInterruptedException)
         {
         }
         catch
         {
         }
         
         if (pi.progress != null && bMain)
         {
            pi.progress.End();
            try
            {
               Directory.Delete(pi.strOrigFolder, true);
            }
            catch
            {
            }
         }
      }

      public static void SizeFolderContentsRecursive(string strFolder, ref long lSize)
      {
         DirectoryInfo di = new DirectoryInfo(strFolder);
         FileInfo[] fis = di.GetFiles();
         foreach (FileInfo fi in fis)
            lSize += fi.Length;
         DirectoryInfo[] dis = di.GetDirectories();
         foreach (DirectoryInfo disub in dis)
            SizeFolderContentsRecursive(disub.FullName, ref lSize);
      }
   }
}
