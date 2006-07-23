using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace WorldWind
{
   public class ConvertDX
   {
      public static Vector3 FromVector3d(Vector3d v)
      {
         return new Vector3((float)v.X, (float)v.Y, (float) v.Z);
      }

      public static Vector3d ToVector3d(Vector3 v)
      {
         return new Vector3d(v.X, v.Y, v.Z);
      }

      public static Matrix FromMatrix4d(Matrix4d m)
      {
         Matrix ret = new Matrix();
         ret.M11 = (float)m[0, 0]; ret.M12 = (float)m[0, 1]; ret.M13 = (float)m[0, 2]; ret.M14 = (float)m[0, 3];
         ret.M21 = (float)m[1, 0]; ret.M22 = (float)m[1, 1]; ret.M23 = (float)m[1, 2]; ret.M24 = (float)m[1, 3];
         ret.M31 = (float)m[2, 0]; ret.M32 = (float)m[2, 1]; ret.M33 = (float)m[2, 2]; ret.M34 = (float)m[2, 3];
         ret.M41 = (float)m[3, 0]; ret.M42 = (float)m[3, 1]; ret.M43 = (float)m[3, 2]; ret.M44 = (float)m[3, 3];
         return ret;
      }

      public static Matrix4d ToMatrix4d(Matrix m)
      {
         return new Matrix4d(
            m.M11, m.M12, m.M13, m.M14,
            m.M21, m.M22, m.M23, m.M24,
            m.M31, m.M32, m.M33, m.M34,
            m.M41, m.M42, m.M43, m.M44
         );
      }

      public static Viewport FromViewport2d(Viewport2d v)
      {
         Viewport ret = new Viewport();
         ret.Height = v.Height;
         ret.MaxZ = (float)v.MaxZ;
         ret.MinZ = (float)v.MinZ;
         ret.Width = v.Width;
         ret.X = v.X;
         ret.Y = v.Y;
         return ret;
      }

      public static Viewport2d ToViewport2d(Viewport v) 
      {
         return new Viewport2d(v.Height, v.MaxZ, v.MinZ, v.Width, v.X, v.Y);
      }
   }
}
