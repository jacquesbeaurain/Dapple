using System;
using WorldWind;
using System.Collections.Generic;
using System.Text;

namespace WorldWind.Renderable
{
   public interface IImageAccessor
   {
      /// <summary>
      /// Returns a texture to be applied vertices on a tile or layer
      /// Specify an argument as null or 0 if it is not required
      /// </summary>
      /// <param name="boundingBox">the region to which the texture will be applied</param>
      /// <param name="imageSize">the size of the image to be returned</param>
      /// <param name="key">a key object associated with the desired image</param>
      /// <returns></returns>

      GeoSpatialDownloadRequest RequestTexture(WorldWind.DrawArgs drawArgs, /*BoundingBox boundingBox,*/ GeographicBoundingBox geoBox, int level);

      string GetImagePath(GeographicBoundingBox geoBox, int level);

      Microsoft.DirectX.Direct3D.Texture GetTexture(WorldWind.DrawArgs drawArgs, /*BoundingBox boundingBox,*/ GeographicBoundingBox geoBox, int level);

      string ImageExtension
      {
         get;
      }

      decimal LevelZeroTileSizeDegrees
      {
         get;
      }

      int LevelCount
      {
         get;
      }

      int TextureSizePixels
      {
         get;
      }

      int TransparentColor
      {
         get;
         set;
      }

      WorldWind.Camera.CameraBase Camera
      {
         get;
         set;
      }

      DownloadQueue DownloadQueue
      {
         get;
      }

      bool IsDownloadableLayer
      {
         get;
      }
   }
}
