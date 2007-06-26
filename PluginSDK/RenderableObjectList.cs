using System;
using System.ComponentModel;
using System.Collections;
using System.IO;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace WorldWind.Renderable
{
   /// <summary>
   /// Represents a parent node in the layer manager tree.  Contains a list of sub-nodes.
   /// </summary>
   public class RenderableObjectList : RenderableObject
   {
      protected ArrayList m_children = new ArrayList();

      public bool ShowOnlyOneLayer;

      /// <summary>
      /// Initializes a new instance of the <see cref= "T:WorldWind.Renderable.RenderableObjectList"/> class.
      /// </summary>
      /// <param name="name"></param>
      public RenderableObjectList(string name)
         : base(name, new Point3d(0, 0, 0), new Quaternion4d())
      {
         this.isSelectable = true;
      }

      public override bool Initialized
      {
         get
         {
            foreach(RenderableObject oRO in (m_children.Clone() as ArrayList))
            {
               if (oRO.IsOn && !oRO.Initialized)
                  return false;
            }
            return true;
         }
      }

      public virtual RenderableObject GetObject(string name)
      {
         try
         {
            foreach (RenderableObject ro in this.m_children)
            {
               if (ro.Name.Equals(name))
                  return ro;
            }
         }
         catch
         {
         }
         return null;
      }

      /// <summary>
      /// Enables layer with specified name
      /// </summary>
      /// <returns>False if layer not found.</returns>
      public virtual bool Enable(string name)
      {
         if (name == null || name.Length == 0)
            return true;

         string lowerName = name.ToLower();
         foreach (RenderableObject ro in m_children)
         {
            if (ro.Name.ToLower() == lowerName)
            {
               ro.IsOn = true;
               return true;
            }

            RenderableObjectList rol = ro as RenderableObjectList;
            if (rol == null)
               continue;

            // Recurse down
            if (rol.Enable(name))
            {
               rol.isOn = true;
               return true;
            }
         }

         return false;
      }

      public virtual void TurnOffAllChildren()
      {
         foreach (RenderableObject ro in this.m_children)
            ro.IsOn = false;
      }

      /// <summary>
      /// List containing the children layers
      /// </summary>
      [Browsable(false)]
      public virtual ArrayList ChildObjects
      {
         get
         {
            return this.m_children;
         }
      }

      /// <summary>
      /// Number of child objects.
      /// </summary>
      [Browsable(false)]
      public virtual int Count
      {
         get
         {
            return this.m_children.Count;
         }
      }

      public override void Initialize(DrawArgs drawArgs)
      {
         if (!this.IsOn)
            return;

         try
         {
            foreach (RenderableObject ro in this.m_children)
            {
               try
               {
                  if (ro.IsOn)
                     ro.Initialize(drawArgs);
               }
               catch (Exception caught)
               {
                  Utility.Log.Write("ROBJ", string.Format("{0}: {1} ({2})",
                     Name, caught.Message, ro.Name));
               }
            }
         }
         catch
         { }

         this.isInitialized = true;
      }

      public override void InitExportInfo(DrawArgs drawArgs, ExportInfo info)
      {
         // clone for thread safety (using lock statements impacts performance)
         ArrayList clone = m_children.Clone() as ArrayList;
         foreach (RenderableObject ro in clone)
         {
            if (ro.IsOn)
            {
               ro.InitExportInfo(drawArgs, info);
            }
         }
      }

      public override void ExportProcess(DrawArgs drawArgs, ExportInfo expInfo)
      {
         // clone for thread safety (using lock statements impacts performance)
         ArrayList clone = m_children.Clone() as ArrayList;
         foreach (RenderableObject ro in clone)
         {
            if (ro.IsOn)
            {
               ro.ExportProcess(drawArgs, expInfo);
            }
         }
      }

      public override void Update(DrawArgs drawArgs)
      {

         if (!this.IsOn)
            return;

         if (!this.isInitialized)
            this.Initialize(drawArgs);


         // clone for thread safety (using lock statements impacts performance)
         ArrayList clone = m_children.Clone() as ArrayList;
         foreach (RenderableObject ro in clone)
         {
            if (ro.IsOn)
            {
               ro.Update(drawArgs);
            }
         }
      }

      public override bool PerformSelectionAction(DrawArgs drawArgs)
      {
         try
         {
            if (!this.IsOn)
               return false;

            foreach (RenderableObject ro in this.m_children)
            {
               if (ro.IsOn && ro.isSelectable)
               {
                  if (ro.PerformSelectionAction(drawArgs))
                     return true;
               }
            }
         }
         catch
         {
         }
         return false;
      }

      public override void Render(DrawArgs drawArgs)
      {

         if (!this.IsOn)
            return;

         ArrayList clone = m_children.Clone() as ArrayList;
         foreach (RenderableObject ro in clone)
         {
            if (ro.IsOn)
            {
               try
               {
                  ro.Render(drawArgs);
               }
               catch
               {
               }
            }
         }
      }

      public override void Dispose()
      {

         this.isInitialized = false;

         ArrayList clone = m_children.Clone() as ArrayList;
         foreach (RenderableObject ro in clone)
         {
            try
            {
               ro.Dispose();
            }
            catch
            {
            }
         }
      }

      /// <summary>
      /// Add a child object to this layer.
      /// </summary>
      public virtual void Add(RenderableObject ro)
      {
         try
         {
            lock (this.m_children.SyncRoot)
            {
               ro.ParentList = this;
               this.m_children.Add(ro);
               SortChildren();
            }
         }
         catch
         {
         }
      }

      /// <summary>
      /// Removes a layer from the child layer list
      /// </summary>
      /// <param name="objectName">Name of object to remove</param>
      public virtual void Remove(string objectName)
      {
         lock (this.m_children.SyncRoot)
         {
            for (int i = 0; i < this.m_children.Count; i++)
            {
               RenderableObject ro = (RenderableObject)this.m_children[i];
               if (ro.Name.Equals(objectName))
               {
                  this.m_children.RemoveAt(i);
                  ro.Dispose();
                  ro.ParentList = null;
                  break;
               }
            }
         }
      }

      /// <summary>
      /// Removes a layer from the child layer list
      /// </summary>
      /// <param name="layer">Layer to be removed.</param>
      public virtual void Remove(RenderableObject layer)
      {
         lock (this.m_children.SyncRoot)
         {
            this.m_children.Remove(layer);
            layer.Dispose();
            layer.ParentList = null;
         }
      }

      /// <summary>
      /// Sorts the children list according to priority
      /// TODO: Redesign the render tree to perhaps a list, to enable proper sorting 
      /// </summary>
      public virtual void SortChildren()
      {
         lock (this.m_children.SyncRoot)
         {
            int index = 0;
            while (index + 1 < m_children.Count)
            {
               RenderableObject a = (RenderableObject)m_children[index];
               RenderableObject b = (RenderableObject)m_children[index + 1];
               if (a.CompareTo(b) > 0)
               {
                  // Swap
                  m_children[index] = b;
                  m_children[index + 1] = a;
                  index = 0;
                  continue;
               }
               index++;
            }
         }
      }
   }
}
