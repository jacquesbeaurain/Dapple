using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace WorldWind.PluginEngine
{
	/// <summary>
	/// Interop functionality for Plugin namespace.
	/// </summary>
	internal sealed class NativeMethods
	{
		private NativeMethods() 
		{
		}

		[DllImport("User32.dll",CharSet = CharSet.Auto)]
		internal static extern long SetWindowLong(IntPtr hwnd, int nIndex, long dwNewLong);

		[StructLayout(LayoutKind.Sequential)]
		internal struct RECT
		{
			internal int left;
			internal int top;
			internal int right;
			internal int bottom;
		}

		[StructLayout(LayoutKind.Sequential,CharSet=CharSet.Auto)]
		internal struct DRAWITEMSTRUCT
		{
			internal int ctrlType;
			internal int ctrlID;
			internal int itemID;
			internal int itemAction;
			internal int itemState;
			internal IntPtr hwnd;
			internal IntPtr hdc;
			internal RECT rcItem;
			internal IntPtr itemData;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct LVHITTESTINFO 
		{ 
			internal Point pt; 
			internal int flags; 
			internal int iItem; 
			internal int iSubItem;
		}
	}
}
