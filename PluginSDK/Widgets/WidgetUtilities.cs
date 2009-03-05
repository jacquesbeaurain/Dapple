//========================= (UNCLASSIFIED) ==============================
// Copyright © 2005-2006 The Johns Hopkins University /
// Applied Physics Laboratory.  All rights reserved.
//
// WorldWind Source Code - Copyright 2005 NASA World Wind 
// Modified under the NOSA License
//
//========================= (UNCLASSIFIED) ==============================
//
// LICENSE AND DISCLAIMER 
//
// Copyright (c) 2005 The Johns Hopkins University. 
//
// This software was developed at The Johns Hopkins University/Applied 
// Physics Laboratory (“JHU/APL”) that is the author thereof under the 
// “work made for hire” provisions of the copyright law.  Permission is 
// hereby granted, free of charge, to any person obtaining a copy of this 
// software and associated documentation (the “Software”), to use the 
// Software without restriction, including without limitation the rights 
// to copy, modify, merge, publish, distribute, sublicense, and/or sell 
// copies of the Software, and to permit others to do so, subject to the 
// following conditions: 
//
// 1.  This LICENSE AND DISCLAIMER, including the copyright notice, shall 
//     be included in all copies of the Software, including copies of 
//     substantial portions of the Software; 
//
// 2.  JHU/APL assumes no obligation to provide support of any kind with 
//     regard to the Software.  This includes no obligation to provide 
//     assistance in using the Software nor to provide updated versions of 
//     the Software; and 
//
// 3.  THE SOFTWARE AND ITS DOCUMENTATION ARE PROVIDED AS IS AND WITHOUT 
//     ANY EXPRESS OR IMPLIED WARRANTIES WHATSOEVER.  ALL WARRANTIES 
//     INCLUDING, BUT NOT LIMITED TO, PERFORMANCE, MERCHANTABILITY, FITNESS
//     FOR A PARTICULAR PURPOSE, AND NONINFRINGEMENT ARE HEREBY DISCLAIMED.  
//     USERS ASSUME THE ENTIRE RISK AND LIABILITY OF USING THE SOFTWARE.  
//     USERS ARE ADVISED TO TEST THE SOFTWARE THOROUGHLY BEFORE RELYING ON 
//     IT.  IN NO EVENT SHALL THE JOHNS HOPKINS UNIVERSITY BE LIABLE FOR 
//     ANY DAMAGES WHATSOEVER, INCLUDING, WITHOUT LIMITATION, ANY LOST 
//     PROFITS, LOST SAVINGS OR OTHER INCIDENTAL OR CONSEQUENTIAL DAMAGES, 
//     ARISING OUT OF THE USE OR INABILITY TO USE THE SOFTWARE. 
//
using System;
using System.Text;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace WorldWind.NewWidgets
{
	internal sealed class WidgetUtilities
	{
		private WidgetUtilities(){}

		internal static void DrawLine(Vector2[] linePoints, int color, Device device)
		{
			CustomVertex.TransformedColored[] lineVerts = new CustomVertex.TransformedColored[linePoints.Length];

			for(int i = 0; i < linePoints.Length; i++)
			{
				lineVerts[i].X = linePoints[i].X;
				lineVerts[i].Y = linePoints[i].Y;
				lineVerts[i].Z = 0.0f;

				lineVerts[i].Color = color;
			}

			device.TextureState[0].ColorOperation = TextureOperation.Disable;
			device.VertexFormat = CustomVertex.TransformedColored.Format;

			device.DrawUserPrimitives(PrimitiveType.LineStrip, lineVerts.Length - 1, lineVerts);
		}

		internal static void DrawBox(int ulx, int uly, int width, int height, float z, int color, Device device)
		{
			CustomVertex.TransformedColored[] verts = new CustomVertex.TransformedColored[4];
			verts[0].X = (float)ulx;
			verts[0].Y = (float)uly;
			verts[0].Z = z;
			verts[0].Color = color;

			verts[1].X = (float)ulx;
			verts[1].Y = (float)uly + height;
			verts[1].Z = z;
			verts[1].Color = color;

			verts[2].X = (float)ulx + width;
			verts[2].Y = (float)uly;
			verts[2].Z = z;
			verts[2].Color = color;

			verts[3].X = (float)ulx + width;
			verts[3].Y = (float)uly + height;
			verts[3].Z = z;
			verts[3].Color = color;

			device.VertexFormat = CustomVertex.TransformedColored.Format;
			device.TextureState[0].ColorOperation = TextureOperation.Disable;
			device.DrawUserPrimitives(PrimitiveType.TriangleStrip, verts.Length - 2, verts);
		}
	}
}
