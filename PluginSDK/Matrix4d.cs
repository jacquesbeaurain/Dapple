using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX;

namespace WorldWind
{
   public struct Matrix4d
   {
      public double M11, M12, M13, M14;
      public double M21, M22, M23, M24;
      public double M31, M32, M33, M34;
      public double M41, M42, M43, M44;

      public static Matrix4d Identity
      {
         get
         {
            return ConvertDX.ToMatrix4d(Matrix.Identity);
         }
      }

      public static Matrix4d Invert(Matrix4d source)
      {
         return ConvertDX.ToMatrix4d(Matrix.Invert(ConvertDX.FromMatrix4d(source)));
      }

      public static Matrix4d operator *(Matrix4d left, Matrix4d right)
      {
         return Matrix4d.Multiply(left, right);
      }

      public static Matrix4d Multiply(Matrix4d left, Matrix4d right)
      {
         return ConvertDX.ToMatrix4d(Matrix.Multiply(ConvertDX.FromMatrix4d(left), ConvertDX.FromMatrix4d(right)));
      }

      public static Matrix4d Translation(double x, double y, double z)
      {
         return ConvertDX.ToMatrix4d(Matrix.Translation((float)x, (float)y, (float)z));
      }

      public static Matrix4d RotationZ(double angle)
      {
         return ConvertDX.ToMatrix4d(Matrix.RotationZ((float)angle));
      }

      public static Matrix4d RotationYawPitchRoll(double yaw, double pitch, double roll)
      {
         return ConvertDX.ToMatrix4d(Matrix.RotationYawPitchRoll((float)yaw, (float)pitch, (float)roll));
      }

      public static Matrix4d LookAtRH(Vector3d cameraPosition, Vector3d cameraTarget, Vector3d cameraUpVector)
      {
         return ConvertDX.ToMatrix4d(Matrix.LookAtRH(ConvertDX.FromVector3d(cameraPosition), ConvertDX.FromVector3d(cameraTarget), ConvertDX.FromVector3d(cameraUpVector)));
      }

      public static Matrix4d PerspectiveFovRH(double fieldOfViewY, double aspectRatio, double znearPlane, double zfarPlane)
      {
         return ConvertDX.ToMatrix4d(Matrix.PerspectiveFovRH((float)fieldOfViewY, (float)aspectRatio, (float)znearPlane, (float)zfarPlane));
      }
   }
}
