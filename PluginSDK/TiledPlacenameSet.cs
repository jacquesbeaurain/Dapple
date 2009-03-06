using System;
using System.Collections;
using System.IO;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using WorldWind;
using Utility;

namespace WorldWind.Renderable
{
	internal struct WorldWindPlacename
	{
		internal string Name;
		internal float Lat;
		internal float Lon;
		internal Point3d cartesianPoint;
	}
}