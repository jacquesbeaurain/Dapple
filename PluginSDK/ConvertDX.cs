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
         ret.M11 = (float)m.M11; ret.M12 = (float)m.M12; ret.M13 = (float)m.M13; ret.M14 = (float)m.M14;
         ret.M21 = (float)m.M21; ret.M22 = (float)m.M22; ret.M23 = (float)m.M23; ret.M24 = (float)m.M24;
         ret.M31 = (float)m.M31; ret.M32 = (float)m.M32; ret.M33 = (float)m.M33; ret.M34 = (float)m.M34;
         ret.M41 = (float)m.M41; ret.M42 = (float)m.M42; ret.M43 = (float)m.M43; ret.M44 = (float)m.M44;
         return ret;
      }

      public static Matrix4d ToMatrix4d(Matrix m)
      {
         Matrix4d ret = new Matrix4d();
         ret.M11 = m.M11; ret.M12 = m.M12; ret.M13 = m.M13; ret.M14 = m.M14;
         ret.M21 = m.M21; ret.M22 = m.M22; ret.M23 = m.M23; ret.M24 = m.M24;
         ret.M31 = m.M31; ret.M32 = m.M32; ret.M33 = m.M33; ret.M34 = m.M34;
         ret.M41 = m.M41; ret.M42 = m.M42; ret.M43 = m.M43; ret.M44 = m.M44;
         return ret;
      }

   }
}
