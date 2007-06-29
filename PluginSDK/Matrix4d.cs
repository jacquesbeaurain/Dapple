using System;
using System.Collections.Generic;
using System.Text;
using Mapack;

namespace WorldWind
{
   public class Matrix4d
   {
      private Matrix m_MapackMat;

      #region Constructor
      public Matrix4d(
         double _M11, double _M12, double _M13, double _M14,
         double _M21, double _M22, double _M23, double _M24,
         double _M31, double _M32, double _M33, double _M34,
         double _M41, double _M42, double _M43, double _M44
         )
      {
         m_MapackMat = new Matrix(new double[][] {
               new double[] { _M11, _M12, _M13, _M14 },
               new double[] { _M21, _M22, _M23, _M24 },
               new double[] { _M31, _M32, _M33, _M34 },
               new double[] { _M41, _M42, _M43, _M44 }
            });
      }
      private Matrix4d(Matrix mapackMat)
      {
         if (mapackMat.Rows != 4 || mapackMat.Columns != 4)
            throw new ApplicationException("Only 4x4 matrices supported in Matrix4d constructor.");
         m_MapackMat = mapackMat;
      }
      #endregion

      #region Element Accessor
      public double this[int row, int column]
      {
         set
         {
            m_MapackMat[row, column] = value;
         }

         get
         {
            return m_MapackMat[row, column];
         }
      }
      #endregion

      #region Math
       public static Matrix4d Empty
       {
           get
           {
               return new Matrix4d(new Matrix(new double[][] {
               new double[] { 0, 0, 0, 0 } ,
               new double[] { 0, 0, 0, 0 } ,
               new double[] { 0, 0, 0, 0 } ,
               new double[] { 0, 0, 0, 0 } 
               }));
           }
       }
      public static Matrix4d Identity
      {
         get
         {
            return new Matrix4d(new Matrix(new double[][] {
               new double[] { 1, 0, 0, 0 } ,
               new double[] { 0, 1, 0, 0 } ,
               new double[] { 0, 0, 1, 0 } ,
               new double[] { 0, 0, 0, 1 } 
               }));
         }
      }


      public static Matrix4d Invert(Matrix4d source)
      {
         //Matrix4d test = ConvertDX.ToMatrix4d(Microsoft.DirectX.Matrix.Invert(ConvertDX.FromMatrix4d(source)));
         Matrix rightHandSide = Matrix.Diagonal(4, 4, 1.0);
         Matrix4d solution = new Matrix4d(new LuDecomposition(source.m_MapackMat).Solve(rightHandSide));
         
         return solution;
      }

      public static Matrix4d operator *(Matrix4d left, Matrix4d right)
      {
         return Matrix4d.Multiply(left, right);
      }

      public static Matrix4d Multiply(Matrix4d left, Matrix4d right)
      {
         //Matrix4d test = ConvertDX.ToMatrix4d(Microsoft.DirectX.Matrix.Multiply(ConvertDX.FromMatrix4d(left), ConvertDX.FromMatrix4d(right)));

         double[][] x = new double[4][] {
            new double[4],
            new double[4],
            new double[4],
            new double[4]
         };
         double[] column = new double[4];
         for (int j = 0; j < 4; j++)
         {
            for (int k = 0; k < 4; k++)
            {
               column[k] = right.m_MapackMat[k, j];
            }
            for (int i = 0; i < 4; i++)
            {
               double s = 0;
               for (int k = 0; k < 4; k++)
               {
                  s += left.m_MapackMat[i, k] * column[k];
               }
               x[i][j] = s;
            }
         }
         Matrix4d solution = new Matrix4d(new Matrix(x));

         return solution;
      }

      public static Matrix4d Translation(double x, double y, double z)
      {
         //Matrix4d test = ConvertDX.ToMatrix4d(Microsoft.DirectX.Matrix.Translation((float)x, (float)y, (float)z));

         Matrix4d solution = new Matrix4d(new Matrix(new double[][] 
            {
               new double[] { 1, 0, 0, 0} ,
               new double[] { 0, 1, 0, 0} ,
               new double[] { 0, 0, 1, 0} ,
               new double[] { x, y, z, 1} 
            }));
         return solution;
      }

      public static Matrix4d Scaling(double x, double y, double z)
      {
         //Matrix4d test = ConvertDX.ToMatrix4d(Microsoft.DirectX.Matrix.Scaling((float)x, (float)y, (float)z));
         Matrix4d solution = new Matrix4d(new Matrix(new double[][] 
            {
               new double[] { x, 0, 0, 0} ,
               new double[] { 0, y, 0, 0} ,
               new double[] { 0, 0, z, 0} ,
               new double[] { 0, 0, 0, 1} 
            }));
         return solution;
      }

      public static Matrix4d RotationX(double angle)
      {
         //Matrix4d test = ConvertDX.ToMatrix4d(Microsoft.DirectX.Matrix.RotationX((float)angle));

         double dSin = Math.Sin(angle);
         double dCos = Math.Cos(angle);

         Matrix4d solution = new Matrix4d(new Matrix(new double[][] 
            {
               new double[] { 1, 0, 0, 0 },
               new double[] { 0, dCos, dSin, 0},
               new double[] { 0, -dSin, dCos, 0 },
               new double[] { 0, 0, 0, 1 }
            }));
         return solution;
      }

      public static Matrix4d RotationY(double angle)
      {
         //Matrix4d test = ConvertDX.ToMatrix4d(Microsoft.DirectX.Matrix.RotationY((float)angle));

         double dSin = Math.Sin(angle);
         double dCos = Math.Cos(angle);

         Matrix4d solution = new Matrix4d(new Matrix(new double[][] 
            {
               new double[] { dCos, 0, -dSin, 0 },
               new double[] { 0, 1, 0, 0},
               new double[] { dSin, 0, dCos, 0 },
               new double[] { 0, 0, 0, 1 }
            }));
         return solution;
      }


      public static Matrix4d RotationZ(double angle)
      {
         //Matrix4d test = ConvertDX.ToMatrix4d(Microsoft.DirectX.Matrix.RotationZ((float)angle));

         double dSin = Math.Sin(angle);
         double dCos = Math.Cos(angle);

         Matrix4d solution = new Matrix4d(new Matrix(new double[][] 
            {
               new double[] { dCos, dSin, 0, 0 },
               new double[] { -dSin, dCos, 0, 0},
               new double[] { 0, 0, 1, 0 },
               new double[] { 0, 0, 0, 1 }
            }));
         return solution;
      }

      public static Matrix4d RotationYawPitchRoll(double yaw, double pitch, double roll)
      {
         //Matrix4d test = ConvertDX.ToMatrix4d(Microsoft.DirectX.Matrix.RotationYawPitchRoll((float)yaw, (float)pitch, (float)roll));

         // This can be simplified to a single matrix constructor by hand if it becomes a performance issue

         Matrix4d rollMat = Matrix4d.RotationZ(roll);
         Matrix4d pitchMat = Matrix4d.RotationX(pitch);
         Matrix4d yawMat = Matrix4d.RotationY(yaw);

         Matrix4d solution = rollMat * pitchMat * yawMat;

         return solution;
      }

      public static Matrix4d LookAtRH(Point3d cameraPosition, Point3d cameraTarget, Point3d cameraUpVector)
      {
         //Matrix4d test = ConvertDX.ToMatrix4d(Microsoft.DirectX.Matrix.LookAtRH(ConvertDX.FromVector3d(cameraPosition), ConvertDX.FromVector3d(cameraTarget), ConvertDX.FromVector3d(cameraUpVector)));

         Point3d z = Point3d.normalize(cameraPosition - cameraTarget);
         Point3d x = Point3d.normalize(Point3d.cross(cameraUpVector, z));
         Point3d y = Point3d.cross(z, x);
         
         Matrix4d solution = new Matrix4d(new Matrix(new double[][] 
            {
               new double[] { x.X, y.X, z.X, 0 },
               new double[] { x.Y, y.Y, z.Y, 0},
               new double[] { x.Z, y.Z, z.Z, 0 },
               new double[] { -Point3d.dot(x, cameraPosition), -Point3d.dot(y, cameraPosition), -Point3d.dot(z, cameraPosition), 1 }
            }));
         return solution;
      }

      public static Matrix4d PerspectiveFovRH(double fieldOfViewY, double aspectRatio, double znearPlane, double zfarPlane)
      {
         //Matrix4d test = ConvertDX.ToMatrix4d(Microsoft.DirectX.Matrix.PerspectiveFovRH((float)fieldOfViewY, (float)aspectRatio, (float)znearPlane, (float)zfarPlane));

         double dYScale = Math.Tan(Math.PI / 2.0 - fieldOfViewY / 2.0);
         double dXScale = dYScale / aspectRatio;

         Matrix4d solution = new Matrix4d(new Matrix(new double[][] 
            {
               new double[] { dXScale , 0, 0, 0 },
               new double[] { 0, dYScale , 0, 0},
               new double[] { 0, 0, zfarPlane / (znearPlane - zfarPlane), -1 },
               new double[] { 0, 0, znearPlane * zfarPlane / (znearPlane - zfarPlane), 0 }
            }));
         return solution;
      }
      #endregion
   }
}
