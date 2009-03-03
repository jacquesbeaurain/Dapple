using System;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
using System.Text;

using System.Security.Cryptography;

using WorldWind.Renderable;

using Utility;

namespace WorldWind
{
	/// <summary>
	/// The types of maps supported by VE
	/// </summary>
	public enum VirtualEarthMapType
	{
		aerial = 0,
		road,
		hybrid
	}
}