using System;
using System.Security.Permissions;
using System.Runtime.InteropServices;

namespace WorldWind.Interop
{
	/// <summary>
	/// Interop methods for WorldWindow namespace
	/// </summary>
	internal sealed class NativeMethods
	{
		private NativeMethods()
		{
		}

		/// <summary>
		/// Contains message information from a thread's message queue.
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		internal struct Message
		{
			internal IntPtr hWnd;
			internal uint msg;
			internal IntPtr wParam;
			internal IntPtr lParam;
			internal uint time;
			internal System.Drawing.Point p;
		}

		/// <summary>
		/// The PeekMessage function dispatches incoming sent messages, 
		/// checks the thread message queue for a posted message, 
		/// and retrieves the message (if any exist).
		/// </summary>
		[System.Security.SuppressUnmanagedCodeSecurity] // We won't use this maliciously
		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		internal static extern bool PeekMessage(out Message msg, IntPtr hWnd, uint messageFilterMin, uint messageFilterMax, uint flags);
	}
}
