using System;
using System.Diagnostics;
using Microsoft.DirectX;
using System.Runtime.InteropServices;

// original code from http://www.mdxinfo.com/tutorials/profiling.php
// by Rim van Wersch; adapted to World Wind by stephan mantler

namespace WorldWind
{
    /// <summary>
    /// A little utility class that allows you to implement profiler event
    /// blocks as Using( new ProfilerEvent() ) {} blocks, so you don't have
    /// to worry about ending an event.
    /// </summary>
	public class DirectXProfilerEvent : IDisposable
    {
        public DirectXProfilerEvent(string name)
        {
            DirectXProfiler.BeginEvent(name);
        }

		  public void Dispose()
        {
            DirectXProfiler.EndEvent();
        }
    }

    /// <summary>
    /// Helper class with satic functions to signal the beginnings and endings
    /// of userdefined event blocks to the profiler, most likely PIX. 
    /// </summary>
    internal static class DirectXProfiler
    {
        #region Profiler settings

#if DEBUG
        private static bool enabled = true;
#else
            private static bool enabled = false;
#endif
        #endregion


        #region Profiler event and marker methods

        internal static int BeginEvent(string name)
        {
            return BeginEvent(System.Drawing.Color.Black, name);
        }

        internal static int BeginEvent(System.Drawing.Color color, string name)
        {
            return BeginEvent(unchecked((uint)color.ToArgb()), name);
        }

        internal static int BeginEvent(uint col, string name)
        {
            if (enabled)
            {
                return BeginEventDirect(col, name);
            }
            else
            {
                return -1;
            }
        }

        internal static int EndEvent()
        {
            if (enabled)
            {
                return EndEventDirect();
            }
            else
            {
                return -1;
            }
        }

        #endregion


        #region Line tracing and DLL imports

        // int D3DPERF_BeginEvent( D3DCOLOR col, LPCWSTR wszName );		
        [System.Security.SuppressUnmanagedCodeSecurity] // We won't use this maliciously
        [DllImport("d3d9.dll", EntryPoint = "D3DPERF_BeginEvent", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi)]
        private static extern int BeginEventDirect(uint col, string wszName);

        // int D3DPERF_EndEvent();
        [System.Security.SuppressUnmanagedCodeSecurity] // We won't use this maliciously, really 
        [DllImport("d3d9.dll", EntryPoint = "D3DPERF_EndEvent", CallingConvention = CallingConvention.Winapi)]
        private static extern int EndEventDirect();

        #endregion
    }
}
