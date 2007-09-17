using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using DM.SharedMemory;
using Utility;
using WorldWind;
using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;

namespace Dapple
{
   static class Program
   {
      /// <summary>
      /// The main entry point for the application.
      /// Mutex code fragments taken from http://www.c-sharpcorner.com/FAQ/Create1InstanceAppSC.asp
      /// </summary>
      [STAThread]
      static void Main(string[] args)
      {
#if !DEBUG
         bool aborting = false;
#endif

         MontajRemote.RemoteInterface oRemoteInterface = null;
         IpcChannel oClientChannel = null;

         try
         {
            bool bAbort = false;
            string strView = "", strGeoTiff = "", strGeoTiffName = "", strLastView = "", strDatasetLink = "";
            bool bGeotiffTmp = false;
            
            GeographicBoundingBox oAoi = null;
            string strAoiCoordinateSystem = string.Empty;
            Dapple.Extract.Options.Client.ClientType eClientType = Dapple.Extract.Options.Client.ClientType.None;

            // Command line parsing
            CommandLineArguments cmdl = new CommandLineArguments(args);

            if (cmdl["h"] != null)
            {
               PrintUsage();
               return;
            }

            if (cmdl[0] != null)
            {
               if (String.Compare(cmdl[0], "ABORT") == 0 && cmdl[1] != null)
                  bAbort = true;
               else
               {
                  strView = Path.GetFullPath(cmdl[0]);
                  if (String.Compare(Path.GetExtension(strView), MainForm.ViewExt, true) != 0 || !File.Exists(strView))
                  {
                     PrintUsage();
                     return;
                  }
               }
            }

            if (cmdl["geotiff"] != null)
            {
               strGeoTiff = Path.GetFullPath(cmdl["geotiff"]);
               if (!(String.Compare(Path.GetExtension(strGeoTiff), ".tiff", true) == 0 || String.Compare(Path.GetExtension(strGeoTiff), ".tif", true) == 0) || !File.Exists(strGeoTiff))
               {
                  PrintUsage();
                  return;
               }
            }

            if (cmdl["geotifftmp"] != null)
            {
               string strGeoTiffTmpVar = cmdl["geotifftmp"];
               int iIndex = strGeoTiffTmpVar.IndexOf(":");
               if (iIndex == -1)
               {
                  PrintUsage();
                  return;
               }

               strGeoTiff = Path.GetFullPath(strGeoTiffTmpVar.Substring(iIndex + 1));
               strGeoTiffName = strGeoTiffTmpVar.Substring(0, iIndex);
               bGeotiffTmp = true;
               if (strGeoTiffName.Length == 0 || !(String.Compare(Path.GetExtension(strGeoTiff), ".tiff", true) == 0 || String.Compare(Path.GetExtension(strGeoTiff), ".tif", true) == 0) || !File.Exists(strGeoTiff))
               {
                  PrintUsage();
                  return;
               }
            }

            if (cmdl["exitview"] != null)
               strLastView = Path.GetFullPath(cmdl["exitview"]);

            if (cmdl["datasetlink"] != null)
               strDatasetLink = Path.GetFullPath(cmdl["datasetlink"]);

            if (cmdl["montajport"] != null)
            {
               int iMontajPort = int.Parse(cmdl["montajport"]);

               if (cmdl["dummyserver"] != null)
               {
                  oClientChannel = new IpcChannel(String.Format("localhost:{0}", iMontajPort));
                  ChannelServices.RegisterChannel(oClientChannel, true);
                  RemotingConfiguration.RegisterWellKnownServiceType(typeof(MontajRemote.RemoteInterface), "MontajRemote", System.Runtime.Remoting.WellKnownObjectMode.Singleton);
               }
               else
               {
                  oClientChannel = new IpcChannel();
                  ChannelServices.RegisterChannel(oClientChannel, true);
               }

               oRemoteInterface = (MontajRemote.RemoteInterface)Activator.GetObject(typeof(MontajRemote.RemoteInterface), String.Format("ipc://localhost:{0}/MontajRemote", iMontajPort));
               try
               {
                  oRemoteInterface.StartConnection();
               }
               catch (System.Runtime.Remoting.RemotingException e)
               {
                  ErrorDisplay errorDialog = new ErrorDisplay();
                  errorDialog.errorMessages
                     (
                     "Error initializing IPC:\n" + Environment.NewLine +
                     "Exception: " + e.GetType().ToString() + Environment.NewLine +
                     "Message: " + e.Message
                     );
                  Application.Run(errorDialog);
                  
                  oRemoteInterface = null;
                  return;
               }
            }

            if (cmdl["aoi"] != null)
            {
               String[] strValues = cmdl["aoi"].Split(new char[] { ',' });
               if (strValues.Length != 4)
               {
                  MessageBox.Show("Error in AOI command-line argument: incorrect number of components");
                  return;
               }
               double dMinX = 180, dMinY = 90, dMaxX = -180, dMaxY = -90;

               bool bAoiArgument = double.TryParse(strValues[0], out dMinX);
               
               if (bAoiArgument)
                  bAoiArgument = double.TryParse(strValues[1], out dMinY);

               if (bAoiArgument)
                  bAoiArgument = double.TryParse(strValues[2], out dMaxX);

               if (bAoiArgument)
                  bAoiArgument = double.TryParse(strValues[3], out dMaxY);

               if (bAoiArgument)
               {
                  oAoi = new GeographicBoundingBox(dMaxY, dMinY, dMinX, dMaxX);
               }
               else
               {
                  MessageBox.Show("Error in AOI command-line argument: number format incorrect");
                  return;
               }

               if (oAoi.North < oAoi.South || oAoi.East < oAoi.West)
               {
                  MessageBox.Show("Error in AOI command-line argument: invalid bounding box");
                  return;
               }

               if (cmdl["aoi_cs"] != null)
               {
                  strAoiCoordinateSystem = cmdl["aoi_cs"];
               }

               if (string.IsNullOrEmpty(strAoiCoordinateSystem))
               {
                  MessageBox.Show("Error in AOI command-line argument: missing coordinate system");
                  return;
               }
            }

            if (cmdl["client"] != null)
            {
               try
               {
                  eClientType = (Dapple.Extract.Options.Client.ClientType)Enum.Parse(eClientType.GetType(), cmdl["client"], true);
               }
               catch
               {
                  MessageBox.Show("Error in client command-line argument: invalid client type");
                  return;
               }
            }
            

            // From now on in own path please and free the console
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Application.ExecutablePath));
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if (bAbort)
            {
               string strErrors = "";
#if !DEBUG
               aborting = true;
#endif
               using (StreamReader sr = new StreamReader(args[1]))
               {
                  String line;
                  // Read and display lines from the file until the end of 
                  // the file is reached.
                  while ((line = sr.ReadLine()) != null)
                  {
                     strErrors += line + "\r\n";
                  }
               }
               Application.EnableVisualStyles();
               Application.SetCompatibleTextRenderingDefault(false);
               ErrorDisplay errorDialog = new ErrorDisplay();
               errorDialog.errorMessages(strErrors);
               Application.Run(errorDialog);
            }
            else
            {
               Process instance = RunningInstance();

               if (RunningInstance() == null)
               {
                  Application.Run(new MainForm(strView, strGeoTiff, strGeoTiffName, bGeotiffTmp, strLastView, strDatasetLink, eClientType, oRemoteInterface, oAoi, strAoiCoordinateSystem));
               }
               else
               {
                  HandleRunningInstance(instance);
                  if (strView.Length > 0 || strGeoTiff.Length > 0 || strDatasetLink.Length > 0)
                  {
                     try
                     {
                        using (Segment s = new Segment("Dapple.OpenView", SharedMemoryCreationFlag.Create, 10000))
                        {
                           string[] strData = new string[6];
                           strData[0] = strView;
                           strData[1] = strGeoTiff;
                           strData[2] = strGeoTiffName;
                           strData[3] = bGeotiffTmp ? "YES" : "NO";
                           strData[4] = strLastView;
                           strData[5] = strDatasetLink;

                           s.SetData(strData);
                           SendMessage(instance.MainWindowHandle, MainForm.OpenViewMessage, IntPtr.Zero, IntPtr.Zero);
                        }
                     }
                     catch
                     {
                     }
                  }
               }
            }
         }
#if !DEBUG
         catch (Exception caught)
         {
            if (!aborting)
               Utility.AbortUtility.Abort(caught, Thread.CurrentThread);
         }
#endif
         finally
         {
            if (oRemoteInterface != null) oRemoteInterface.EndConnection();
            if (oClientChannel != null) ChannelServices.UnregisterChannel(oClientChannel);
         }
      }

      public static void PrintUsage()
      {
         MessageBox.Show(null, "Dapple command line usage:\n" +
                        "\n" +
                        "Dapple -h -geotiff=file -exitview=view view\n" +
                        "\n" +
                        "-h\t\tthis help\n" +
                        "-geotiff=file\tpath to a geotiff in WGS84 to be loaded in the current or start-up view\n" +
                        "–geotifftmp=name:tmpfilename Layer name and path to a temporary geotiff filename\n" +
                        "                             (will be deleted on Dapple exit) in WGS84 to be loaded\n" +
                        "                             in current or start-up view.\n" + 
                        "-exitview=view\tpath to a Dapple view file in which to place the last view\n" +
                        "view\t\tpath to a Dapple view file to load as start-up view\n" +
                        "\n", "Dapple", MessageBoxButtons.OK, MessageBoxIcon.Information);
      }

      public static Process RunningInstance()
      {
         Process current = Process.GetCurrentProcess();
         Process[] processes = Process.GetProcessesByName(current.ProcessName);

         //Loop through the running processes in with the same name
         foreach (Process process in processes)
         {
            //Ignore the current process
            if (process.Id != current.Id)
            {
               //Make sure that the process is running from the exe file.
               //if (Assembly.GetExecutingAssembly().Location.Replace("/", "\\") ==
               //current.MainModule.FileName)
               //{
               //Return the other process instance.
               return process;
               //}
            }
         }
         return null;
      }

      public static void HandleRunningInstance(Process instance)
      {
         //Make sure the window is not minimized or maximized
         if (IsIconic(instance.MainWindowHandle))
            ShowWindowAsync(instance.MainWindowHandle, WS_SHOWNORMAL);

         //Set the real intance to foreground window
         SetForegroundWindow(instance.MainWindowHandle);
      }

      [DllImport("User32.dll")]
      private static extern bool ShowWindowAsync(IntPtr hWnd, int cmdShow);

      [DllImport("User32.dll")]
      private static extern bool IsIconic(IntPtr hWnd);

      [DllImport("User32.dll")]
      private static extern UInt32 SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

      [DllImport("User32.dll")]
      private static extern bool SetForegroundWindow(IntPtr hWnd);

      private const int WS_SHOWNORMAL = 1;
   }


}
