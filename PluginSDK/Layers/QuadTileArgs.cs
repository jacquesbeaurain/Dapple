using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Utility;
using WorldWind.Terrain;

namespace WorldWind.Renderable
{

   public class QuadTileArgs : IDisposable
   {

      #region Private Members

      QuadTileSet m_ParentQuadTileSet;
      double _layerRadius;
      bool _alwaysRenderBaseTiles;
      float _tileDrawSpread;
      float _tileDrawDistance;

      //int m_iTextureSize;

      int m_TransparentColor = 0;
      bool m_RenderTileFileNames = false;
      byte m_opacity = 255;
      bool _isDownloadingElevation;

      TerrainAccessor _terrainAccessor;
      IImageAccessor _imageAccessor;

      #endregion

      public GeographicBoundingBox Boundary;

      #region Properties

      public int TransparentColor
      {
         get
         {
            return m_TransparentColor;
         }
         set
         {
            m_TransparentColor = value;
         }
      }
      public QuadTileSet ParentQuadTileSet
      {
         get
         {
            return m_ParentQuadTileSet;
         }
      }
      public byte Opacity
      {
         get
         {
            return m_opacity;
         }
         set
         {
            m_opacity = value;
         }
      }

      public double LayerRadius
      {
         get
         {
            return this._layerRadius;
         }
         set
         {
            this._layerRadius = value;
         }
      }

      public bool AlwaysRenderBaseTiles
      {
         get
         {
            return this._alwaysRenderBaseTiles;
         }
         set
         {
            this._alwaysRenderBaseTiles = value;
         }
      }

      public float TileDrawSpread
      {
         get
         {
            return this._tileDrawSpread;
         }
         set
         {
            this._tileDrawSpread = value;
         }
      }

      public float TileDrawDistance
      {
         get
         {
            return this._tileDrawDistance;
         }
         set
         {
            this._tileDrawDistance = value;
         }
      }

      public bool IsDownloadingElevation
      {
         get
         {
            return this._isDownloadingElevation;
         }
         set
         {
            this._isDownloadingElevation = value;
         }
      }

      public bool RenderTileFileNames
      {
         get
         {
            return m_RenderTileFileNames;
         }
         set
         {
            m_RenderTileFileNames = value;
         }
      }

      public TerrainAccessor TerrainAccessor
      {
         get
         {
            return this._terrainAccessor;
         }
         set
         {
            this._terrainAccessor = value;
         }
      }

      public IImageAccessor ImageAccessor
      {
         get
         {
            return this._imageAccessor;
         }
      }


      #endregion

      /// <summary>
      /// Initializes a new instance of the <see cref= "T:WorldWind.Renderable.QuadTileArgs"/> class.
      /// </summary>
      /// <param name="layerRadius"></param>
      /// <param name="terrainAccessor"></param>
      /// <param name="imageAccessor"></param>
      public QuadTileArgs(
         double layerRadius,
        QuadTileSet parentQuadTileSet,
         TerrainAccessor terrainAccessor,
         IImageAccessor imageAccessor,
         bool alwaysRenderBaseTiles)
      {
         this._layerRadius = layerRadius;
         m_ParentQuadTileSet = parentQuadTileSet;
         this._tileDrawDistance = 3.5f;
         this._tileDrawSpread = 2.9f;
         this._imageAccessor = imageAccessor;
         this._terrainAccessor = terrainAccessor;
         this._alwaysRenderBaseTiles = alwaysRenderBaseTiles;
      }

      public void Dispose()
      {
         _imageAccessor.DownloadQueue.ClearDownloadRequests();
      }
   }
}
