//
// Copyright © 2005 NASA.  Available under the NOSA License
//
// Portions copied from Icon - Copyright © 2005-2006 The Johns Hopkins University 
// Applied Physics Laboratory.  Available under the JHU/APL Open Source Agreement.
//
using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Net;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Text;
using WorldWind.NewWidgets;

namespace WorldWind.Renderable
{
	/// <summary>
	/// One icon in an icon layer
	/// </summary>
	public class KMLIcon : Icon
    {
        /// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.Renderable.Icon"/> class  
        /// </summary>
		/// <param name="name">Name of the icon</param>
		/// <param name="latitude">Latitude in decimal degrees.</param>
		/// <param name="longitude">Longitude in decimal degrees.</param>
		  /// 
        /// <param name="heightAboveSurface">Altitude</param>
		  public KMLIcon(string name, double latitude, double longitude, double heightAboveSurface)
            : base(name, latitude, longitude, heightAboveSurface)
		{
            AutoScaleIcon = true;
            Declutter = true;
		}
	}
}
