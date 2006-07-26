using System;
using System.Windows.Forms;
using System.Threading;
using System.Xml;
using System.Collections.Generic;
using System.Text;
using WorldWind;
using WorldWind.Renderable;

namespace Dapple.LayerGeneration
{
   public interface IBuilder : ICloneable
   {
      string Name
      {
         get;
      }

      byte Opacity
      {
         get;
         set;
      }

      string Type
      {
         get;
      }

      bool IsChanged
      {
         get;
      }

      IBuilder Parent
      {
         get;
      }

      bool IsParent
      {
         get;
      }

      [System.ComponentModel.Browsable(false)]
      bool SupportsOpacity
      {
         get;
      }

      [System.ComponentModel.Browsable(false)]
      bool SupportsMetaData
      {
         get;
      }

      [System.ComponentModel.Browsable(false)]
      string StyleSheetPath
      {
         get;
      }

      XmlNode GetMetaData(XmlDocument oDoc);

      void SubscribeToBuilderChangedEvent(BuilderChangedHandler handler);
      void UnsubscribeToBuilderChangedEvent(BuilderChangedHandler handler);
   }

   public delegate void LayerLoadedCallback(IBuilder builder);
   public delegate void BuilderChangedHandler(IBuilder sender, BuilderChangeType changeType);

   public enum BuilderChangeType
   {
      LoadedSync,
      LoadedASync,
      LoadedSyncFailed,
      LoadedASyncFailed,
      LoadFailed,
      Removed,
      OpacityChanged,
      VisibilityChanged
   }

   public abstract class LayerBuilder : IBuilder
   {
      protected string m_strName;
      protected IBuilder m_Parent;
      protected World m_oWorld;
      protected byte m_bOpacity = 255;
      private bool m_blnIsLoading = false;
      protected bool m_IsOn = true;
      private int m_intRenderPriority = 0;
      protected bool m_blnFailed = false;
      private bool m_blnIsAdded = false;
      private bool m_bAsyncLoaded = false;
      private object m_lockObject = new object();

      private static int intObjectsRendered = 0;

      private event BuilderChangedHandler BuilderChanged;

      public string Name
      {
         get
         {
            return m_strName;
         }
      }

      public abstract byte Opacity
      {
         get;
         set;
      }

      public abstract bool Visible
      {
         get;
         set;
      }

      public abstract string Type
      {
         get;
      }

      public abstract bool IsChanged
      {
         get;
      }

      public abstract string LogoKey
      {
         get;
      }

      public abstract bool bIsDownloading(out int iBytesRead, out int iTotalBytes);

      /// <summary>
      /// Used to display the type of service being provided
      /// DAP, WMS, File etc..
      /// </summary>
      public abstract string ServiceType
      {
         get;
      }

      public IBuilder Parent
      {
         get
         {
            return m_Parent;
         }
      }

      public bool IsParent
      {
         get
         {
            return false;
         }
      }

      public bool Failed
      {
         get
         {
            return m_blnFailed;
         }
      }

      public bool IsAdded
      {
         get
         {
            return m_blnIsAdded;
         }
      }

      [System.ComponentModel.Browsable(false)]
      public virtual bool SupportsOpacity
      {
         get
         {
            return true;
         }
      }

      [System.ComponentModel.Browsable(false)]
      public virtual bool SupportsMetaData
      {
         get
         {
            return false;
         }
      }

      public virtual XmlNode GetMetaData(XmlDocument oDoc)
      {
         return null;
      }

      [System.ComponentModel.Browsable(false)]
      public virtual bool SupportsLegend
      {
         get
         {
            return false;
         }
      }

      public virtual string[] GetLegendURLs()
      {
         return null;
      }


      public virtual string StyleSheetPath
      {
         get
         {
            return null;
         }
      }

      public abstract RenderableObject GetLayer();

      public abstract string GetURI();

      public abstract string GetCachePath();
      
      /// <summary>
      /// Asynchronous addition of layer
      /// </summary>
      public void AsyncAddLayer()
      {
         m_blnFailed = false;
         if (!m_blnIsLoading)
         {
            lock (m_lockObject)
            {
               if (m_intRenderPriority == 0)
               {
                  intObjectsRendered++;
                  m_intRenderPriority = intObjectsRendered;
               }

               m_blnIsLoading = true;
               m_bAsyncLoaded = true;
               System.Threading.ThreadPool.QueueUserWorkItem(LoadLayer);
            }
         }
      }

      /// <summary>
      /// Synchronous addition of layer
      /// </summary>
      public void SyncAddLayer(bool forceload)
      {
         if (forceload || !m_blnIsLoading)
         {
            lock (m_lockObject)
            {
               if (m_intRenderPriority == 0)
               {
                  intObjectsRendered++;
                  m_intRenderPriority = intObjectsRendered;
               }

               m_bAsyncLoaded = false;
               m_blnIsLoading = true;
               LoadLayer(null);
            }
         }
      }

      public void RefreshLayer()
      {
         lock (m_lockObject)
         {
            RenderableObject oRO = m_oWorld.RenderableObjects.GetObject("3 - " + m_intRenderPriority.ToString());
            m_blnIsLoading = false;
            if (oRO != null)
            {
               m_oWorld.RenderableObjects.Remove(oRO);
               m_blnIsLoading = false;
            }
            CleanUpLayer(false);
            m_blnIsAdded = false;
         }
         AsyncAddLayer();
      }

      public void PushBackInRenderOrder()
      {
         lock (m_lockObject)
         {
            RenderableObject oRO = m_oWorld.RenderableObjects.GetObject("3 - " + m_intRenderPriority.ToString());
            if (oRO != null)
            {
               intObjectsRendered++;
               m_intRenderPriority = intObjectsRendered;
               oRO.Name = "3 - " + m_intRenderPriority.ToString();
               m_oWorld.RenderableObjects.SortChildren();
            }
         }
      }

      public bool RemoveLayer()
      {
         bool bRemoved = false;
         bool bReturn = false;
         lock (m_lockObject)
         {
            RenderableObject oRO = m_oWorld.RenderableObjects.GetObject("3 - " + m_intRenderPriority.ToString());
            m_blnIsLoading = false;
            if (oRO != null)
            {
               m_oWorld.RenderableObjects.Remove(oRO);
               m_blnIsLoading = false;
               bRemoved = true;
               bReturn = true;
            }
            CleanUpLayer(true);
            m_blnIsAdded = false;
         }
         if (bRemoved)
            SendBuilderChanged(BuilderChangeType.Removed);
         return bReturn;
      }

      private void LoadLayer(object stateInfo)
      {
#if !DEBUG
         try
         {
#endif
            lock (m_lockObject)
            {
               RenderableObject layer = null;
               try
               {
                  layer = GetLayer();
               }
               catch
               {
                  m_blnIsLoading = false;
                  m_blnFailed = true;
               }
               if (!m_blnFailed)
               {
                  layer.Name = "3 - " + m_intRenderPriority.ToString();
                  m_oWorld.RenderableObjects.Remove("3 - " + m_intRenderPriority.ToString());
                  if (m_blnIsLoading)
                  {
                     m_oWorld.RenderableObjects.Add(layer);
                     m_blnIsAdded = true;
                  }
                  m_blnIsLoading = false;
               }
            }
            if (!m_blnFailed)
            {
               if (m_bAsyncLoaded)
                  SendBuilderChanged(BuilderChangeType.LoadedASync);
               else
                  SendBuilderChanged(BuilderChangeType.LoadedSync);
            }
            else
            {
               if (m_bAsyncLoaded)
                  SendBuilderChanged(BuilderChangeType.LoadedASyncFailed);
               else
                  SendBuilderChanged(BuilderChangeType.LoadedSyncFailed);
            }
#if !DEBUG
         }
         catch (Exception e)
         {
            Utility.AbortUtility.Abort(e, Thread.CurrentThread);
         }
#endif
      }

      public void SubscribeToBuilderChangedEvent(BuilderChangedHandler handler)
      {
         BuilderChanged += handler;
      }

      public void UnsubscribeToBuilderChangedEvent(BuilderChangedHandler handler)
      {
         BuilderChanged -= handler;
      }

      protected void SendBuilderChanged(BuilderChangeType type)
      {
         if (BuilderChanged != null)
         {
            BuilderChanged.Invoke(this, type);
         }
      }

      protected abstract void CleanUpLayer(bool bFinal);
      
      #region ICloneable Members

      public abstract object Clone();

      #endregion
   }


   public abstract class ImageBuilder : LayerBuilder
   {
      public abstract GeographicBoundingBox Extents
      {
         get;
      }

      public abstract int ImagePixelSize
      {
         get;
         set;
      }
   }
}
