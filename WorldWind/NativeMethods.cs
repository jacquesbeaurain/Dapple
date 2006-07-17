using System;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace WorldWind.Interop
{
	/// <summary>
	///  Interop functionality for WorldWind namespace
	/// </summary>
	sealed internal class NativeMethods
	{
		private NativeMethods()
		{}

		/// <summary>
		/// Contains message information from a thread's message queue.
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		public struct Message
		{
			public IntPtr hWnd;
			public uint msg;
			public IntPtr wParam;
			public IntPtr lParam;
			public uint time;
			public System.Drawing.Point p;
		}

		/// <summary>
		/// The PeekMessage function dispatches incoming sent messages, 
		/// checks the thread message queue for a posted message, 
		/// and retrieves the message (if any exist).
		/// </summary>
		[System.Security.SuppressUnmanagedCodeSecurity] // We won't use this maliciously
		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		public static extern bool PeekMessage(out Message msg, IntPtr hWnd, uint messageFilterMin, uint messageFilterMax, uint flags);
	}
}
