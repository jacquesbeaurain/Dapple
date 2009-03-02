using System;

namespace GeometryUtility
{
	/// <summary>
	///To define the common types used in 
	///Analytical Geometry calculations.
	/// </summary>
	
	//To define some constant Values 
	//used for local judgment 
	internal struct ConstantValue
	{
		internal const  double SmallValue=double.Epsilon;
		internal const double BigValue=double.MaxValue;
	}
	
	internal enum VertexType
	{
		ErrorPoint,
		ConvexPoint,
		ConcavePoint		
	}

	internal enum PolygonType
	{
		Unknown,
		Convex, 
		Concave	
	}

	internal enum PolygonDirection
	{
		Unknown,
		Clockwise,
		Count_Clockwise
	}
}
