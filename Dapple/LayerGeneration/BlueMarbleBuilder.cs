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
         if (m_hObject.IsOn)
         {
            //CMTODO: Implement this actually
            iBytesRead = 5;
            iTotalBytes = 50;
            return true;
         }
         else
         {
            iBytesRead = 0;
            iTotalBytes = 0;
            return false;
         }
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
   }
}
