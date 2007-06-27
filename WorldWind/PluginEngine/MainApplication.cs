using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace WorldWind.PluginEngine
{
   public abstract class MainApplication : Form
   {
      public static WorldWindSettings Settings = new WorldWindSettings();
      public static readonly string DirectoryPath = Path.GetDirectoryName(Application.ExecutablePath);

      /// <summary>
      /// MainApplication's System.Windows.Forms.Form
      /// </summary>
      public abstract System.Windows.Forms.Form Form
      {
         get;
      }

      /// <summary>
      /// MainApplication's globe window
      /// </summary>
      public abstract WorldWindow WorldWindow
      {
         get;
      }

      /// <summary>
      /// The splash screen dialog.
      /// </summary>
      public abstract Splash SplashScreen
      {
         get;
      }

      /// <summary>
      /// MainApplication's main menu (drop-down)
      /// </summary>
      public abstract MainMenu MainMenu
      {
         get;
      }

      /// <summary>
      /// MainApplication's Tools menu (drop-down)
      /// </summary>
      public abstract MenuItem ToolsMenu
      {
         get;
      }

      /// <summary>
      /// MainApplication's View menu (drop-down)
      /// </summary>
      public abstract MenuItem ViewMenu
      {
         get;
      }

      /// <summary>
      /// MainApplication's Plugins menu (drop-down)
      /// </summary>
      public abstract MenuItem PluginsMenu
      {
         get;
      }


   }
}
