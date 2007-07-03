using System;
using System.Collections.Generic;
using System.Text;

using System.Runtime.InteropServices;

namespace Utility
{
    //this code is Mashi's pInvoke wrapper over Proj.4
    //i didn't make any modifications to it
    //----------------------------------------------------------------------------
    // : Reproject images on the fly
    // : 1.0
    // : Reproject images on the fly using nowak's "reproject on GPU technique" 
    // : Bjorn Reppen aka "Mashi"
    // : http://www.mashiharu.com
    // : 
    //----------------------------------------------------------------------------
    // This file is in the Public Domain, and comes with no warranty. 

    /// <summary>
    /// Sorry for lack of description, but this struct is kinda difficult 
    /// to describe since it supports so many coordinate systems.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct UV
    {
        public double U;
        public double V;

        public UV(double u, double v)
        {
            this.U = u;
            this.V = v;
        }
    }

    /// <summary>
    /// C# wrapper for proj.4 projection filter
    /// http://proj.maptools.org/
    /// </summary>
    public class Projection : IDisposable
    {
        IntPtr projPJ;
        [DllImport("proj.dll")]
        static extern IntPtr pj_init(int argc, string[] args);

        [DllImport("proj.dll")]
        static extern string pj_free(IntPtr projPJ);

        [DllImport("proj.dll")]
        static extern UV pj_fwd(UV uv, IntPtr projPJ);

        /// <summary>
        /// XY -> Lat/lon
        /// </summary>
        /// <param name="uv"></param>
        /// <param name="projPJ"></param>
        /// <returns></returns>
        [DllImport("proj.dll")]
        static extern UV pj_inv(UV uv, IntPtr projPJ);

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="initParameters">Proj.4 style list of options.
        /// <sample>new string[]{ "proj=utm", "ellps=WGS84", "no.defs", "zone=32" }</sample>
        /// </param>
        public Projection(string[] initParameters)
        {
            projPJ = pj_init(initParameters.Length, initParameters);
            if (projPJ == IntPtr.Zero)
                throw new ApplicationException("Projection initialization failed.");
        }

        /// <summary>
        /// Forward (Go from specified projection to lat/lon)
        /// </summary>
        /// <param name="uv"></param>
        /// <returns></returns>
        public UV Forward(UV uv)
        {
            return pj_fwd(uv, projPJ);
        }

        /// <summary>
        /// Inverse (Go from lat/lon to specified projection)
        /// </summary>
        /// <param name="uv"></param>
        /// <returns></returns>
        public UV Inverse(UV uv)
        {
            return pj_inv(uv, projPJ);
        }

        public void Dispose()
        {
            if (projPJ != IntPtr.Zero)
            {
                pj_free(projPJ);
                projPJ = IntPtr.Zero;
            }
        }
    }
}
