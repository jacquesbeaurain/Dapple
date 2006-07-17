using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Dapple
{
   /// <summary>
   /// Represents a standard Windows track bar but with paint event handling (based on http://www.codeproject.com/cs/miscctrl/genmissingpaintevent.asp).
   /// </summary>
   //Use the same attibutes as TrackBar
   [DefaultProperty("Value")]
   [DefaultEvent("Scroll")]
   [DefaultBindingProperty("Value")]
   [Designer("System.Windows.Forms.Design.TrackBarDesigner, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
   public class TrackBarWithPaint : TrackBar
   {

      Bitmap internalBitmap = null;
      Graphics internalGraphics = null;

      private void DisposeInternal()
      {
         if (internalGraphics != null)
            internalGraphics.Dispose();
         if (internalBitmap != null)
            internalBitmap.Dispose();

      }
      /// <summary>Releases resources.</summary>
      /// <param name="disposing">true = Both managed and unmanaged, false = Unmanaged only.</param>
      protected override void Dispose(bool disposing)
      {
         if (disposing)
            DisposeInternal();
         base.Dispose(disposing);
      }

      /// <summary>Occurs when window is resized.</summary>
      /// <param name="e">A System.EventArgs.Empty.</param>
      /// <remarks>Recreates internal Graphics object. </remarks>
      protected override void OnResize(System.EventArgs e)
      {
         if (internalBitmap == null ||
             internalBitmap.Width != Width || internalBitmap.Height != Height)
         {

            if (Width != 0 && Height != 0)
            {
               DisposeInternal();
               internalBitmap = new Bitmap(Width, Height);
               internalGraphics = Graphics.FromImage(internalBitmap);
            }
         }
      }

      /// <summary>Occurs when a Windows message is dispatched.</summary>
      /// <param name="message">Message to process.</param>
      /// <remarks>Overrides WM_PAINT, WM_ERASEBKGND.</remarks>
      protected override void WndProc(ref Message message)
      {
         const int WM_PAINT = 0x000F;
         const int WM_PRINTCLIENT = 0x0318;
         const int WM_ERASEBKGND = 0x0014;

         switch (message.Msg)
         {

            case WM_ERASEBKGND:
               //removes flicker
               return;

            case WM_PAINT:
               // The designer host does not call OnResize()                    
               if (internalGraphics == null)
                  OnResize(EventArgs.Empty);

               //Set up 
               Win32.RECT updateRect = new Win32.RECT();
               if (Win32.GetUpdateRect(message.HWnd, ref updateRect, false) == 0)
                  break;

               Win32.PAINTSTRUCT paintStruct = new Win32.PAINTSTRUCT();
               IntPtr screenHdc = Win32.BeginPaint(message.HWnd, ref paintStruct);
               using (Graphics screenGraphics = Graphics.FromHdc(screenHdc))
               {

                  //Draw Internal Graphics
                  IntPtr hdc = internalGraphics.GetHdc();
                  Message printClientMessage = Message.Create(Handle, WM_PRINTCLIENT, hdc, IntPtr.Zero);
                  DefWndProc(ref printClientMessage);
                  internalGraphics.ReleaseHdc(hdc);

                  //Add the missing OnPaint() call
                  OnPaint(new PaintEventArgs(internalGraphics, Rectangle.FromLTRB(
                      updateRect.left,
                      updateRect.top,
                      updateRect.right,
                      updateRect.bottom)));

                  //Draw Screen Graphics
                  screenGraphics.DrawImage(internalBitmap, 0, 0);
               }

               //Tear down
               Win32.EndPaint(message.HWnd, ref paintStruct);
               return;
         }
         base.WndProc(ref message);
      }

      /// <summary>Occurs when the control is redrawn.</summary>
      /// <remarks>Re enable browsing attributes for the Paint Event.</remarks>
      [
      EditorBrowsableAttribute(EditorBrowsableState.Always),
      BrowsableAttribute(true)
      ]
      public new event PaintEventHandler Paint
      {
         add { base.Paint += value; }
         remove { base.Paint -= value; }
      }
   }

   /// <summary>
   /// Win32 PInvoke declarations
   /// </summary>
   internal class Win32
   {
      private Win32()
      {
      }

      [DllImport("User32.dll")]
      public static extern int GetUpdateRect(IntPtr hwnd, ref RECT rect, bool erase);

      [DllImport("User32.dll", SetLastError = true)]
      public static extern bool GetWindowRect(IntPtr handle, ref RECT rect);

      [DllImport("User32.dll")]
      public static extern IntPtr BeginPaint(IntPtr hWnd, ref PAINTSTRUCT paintStruct);

      [DllImport("User32.dll")]
      public static extern bool EndPaint(IntPtr hWnd, ref PAINTSTRUCT paintStruct);

      [StructLayout(LayoutKind.Sequential)]
      public struct RECT
      {
         public int left;
         public int top;
         public int right;
         public int bottom;
      }

      [StructLayout(LayoutKind.Sequential)]
      public struct PAINTSTRUCT
      {
         public IntPtr hdc;
         public int fErase;
         public RECT rcPaint;
         public int fRestore;
         public int fIncUpdate;
         public int Reserved1;
         public int Reserved2;
         public int Reserved3;
         public int Reserved4;
         public int Reserved5;
         public int Reserved6;
         public int Reserved7;
         public int Reserved8;
      }
   }
}

