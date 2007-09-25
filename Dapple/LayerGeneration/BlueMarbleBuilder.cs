using System;
using System.Collections.Generic;
using System.Text;
using WorldWind.Renderable;
using System.IO;

namespace Dapple.LayerGeneration
{
   /// <summary>
   /// Wraps the BMNG layer in a LayerBuilder.
   /// </summary>
   class BlueMarbleBuilder : LayerBuilder
   {
      private RenderableObject m_hObject;
      private bool m_blIsChanged = true;

      public BlueMarbleBuilder(RenderableObject hObject) :base("Blue Marble", MainForm.WorldWindowSingleton, null)
      {
         m_hObject = hObject;
      }

      public override byte Opacity
      {
         get
         {
            return m_hObject.Opacity;
         }
         set
         {
            CascadeOpacity(m_hObject, value);
         }
      }

      public override bool Visible
      {
         get
         {
            return m_hObject.IsOn;
         }
         set
         {
            m_hObject.IsOn = value;
         }
      }

      public override bool IsChanged
      {
         get { return m_blIsChanged; }
      }

      public override string ServerTypeIconKey
      {
         get { return "blue_marble"; }
      }

      public override string DisplayIconKey
      {
         get { return "blue_marble"; }
      }

      public override WorldWind.GeographicBoundingBox Extents
      {
         get { return new WorldWind.GeographicBoundingBox(90, -90, -180, 180); }
      }

      public override bool bIsDownloading(out int iBytesRead, out int iTotalBytes)
      {
         return CascadeDownloading(m_hObject, out iBytesRead, out iTotalBytes);
      }

      public override WorldWind.Renderable.RenderableObject GetLayer()
      {
         return m_hObject;
      }

      public override string GetURI()
      {
         return null;
      }

      public override string GetCachePath()
      {
         return Path.Combine(m_oWorldWindow.Cache.CacheDirectory, "BMNG");
      }

      protected override void CleanUpLayer(bool bFinal)
      {
         if (m_hObject != null)
            m_hObject.Dispose();

         m_blIsChanged = true;
      }

      public override bool Equals(object obj)
      {
         if (!(obj is BlueMarbleBuilder)) return false;

         return true; // There can be only one
      }

      public override object CloneSpecific()
      {
         return new BlueMarbleBuilder(m_hObject);
      }

      private void CascadeOpacity(RenderableObject oRObject, byte bOpacity)
      {
         oRObject.Opacity = bOpacity;

         if (oRObject is RenderableObjectList)
         {
            foreach (RenderableObject oChildRObject in ((RenderableObjectList)oRObject).ChildObjects)
            {
               CascadeOpacity(oChildRObject, bOpacity);
            }
         }
      }

      private bool CascadeDownloading(RenderableObject oObj, out int iBytesRead, out int iTotalBytes)
      {
         if (oObj.Initialized && oObj.IsOn)
         {
            if (oObj is QuadTileSet)
            {
               return ((QuadTileSet)oObj).bIsDownloading(out iBytesRead, out iTotalBytes);
            }
            else if (oObj is RenderableObjectList)
            {
               bool result = false;
               int iResultRead = 0;
               int iResultTotal = 0;

               foreach (RenderableObject oRO in ((RenderableObjectList)oObj).ChildObjects)
               {
                  int iRead, iTotal;
                  result |= CascadeDownloading(oRO, out iRead, out iTotal);
                  iResultRead += iRead;
                  iResultTotal += iTotal;
               }

               iBytesRead = iResultRead;
               iTotalBytes = iResultTotal;
               return result;
            }
         }

         iBytesRead = 0;
         iTotalBytes = 0;
         return false;
      }
   }
}
