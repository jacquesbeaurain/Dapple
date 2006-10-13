using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Windows.Forms;
using WorldWind.Renderable;
using Geosoft.DotNetTools;

namespace Dapple.LayerGeneration
{
   public class BuilderDirectory : IBuilder
   {
      private string m_strName;
      private IBuilder m_Parent;
      private bool m_Removable;
      private List<LayerBuilder> m_colChildren;
      private List<BuilderDirectory> m_colSublist;
      protected byte m_bOpacity = 255;

      public BuilderDirectory(string name, IBuilder parent, bool removable)
      {
         m_strName = name;
         m_Parent = parent;
         m_Removable = removable;
         m_colChildren = new List<LayerBuilder>();
         m_colSublist = new List<BuilderDirectory>();
      }

      [System.ComponentModel.Browsable(false)]
      public List<LayerBuilder> LayerBuilders
      {
         get
         {
            return m_colChildren;
         }
      }

      [System.ComponentModel.Browsable(false)]
      public bool Removable
      {
         get
         {
            return m_Removable;
         }
      }
      
      [System.ComponentModel.Browsable(false)]
      public List<BuilderDirectory> SubList
      {
         get
         {
            return m_colSublist;
         }
      }

      #region IBuilder Members

      public string Name
      {
         get { return m_strName; }
      }

      public void ChangeName(string strNewName)
      {
         m_strName = strNewName;
      }

      public byte Opacity
      {
         get
         {
            return m_bOpacity;
         }
         set
         {
            foreach (IBuilder builder in SubList)
            {
               builder.Opacity = value;
            }
            foreach (LayerBuilder builder in LayerBuilders)
            {
               if (builder.IsAdded)
                  builder.Opacity = value;
            }
            m_bOpacity = value;
         }
      }

      public string Type
      {
         get { return TypeName; }
      }

      public bool IsChanged
      {
         get { return false; }
      }

      public IBuilder Parent
      {
         get { return m_Parent; }
      }

      public bool IsParent
      {
         get { return true; }
      }

      public bool IsChildAdded()
      {
         foreach (LayerBuilder builder in LayerBuilders)
         {
            if (builder.IsAdded)
            {
               return true;
            }
         }
         foreach (BuilderDirectory dir in SubList)
         {
            if (dir.IsChildAdded())
            {
               return true;
            }
         }
         return false;
      }

      /// <summary>
      /// Indicates whether a directory supports opacity
      /// by querying it's sublist and layers
      /// </summary>
      [System.ComponentModel.Browsable(false)]
      public virtual bool SupportsOpacity
      {
         get 
         {
            foreach (LayerBuilder builder in LayerBuilders)
            {
               if (builder.SupportsOpacity)
               {
                  return true;
               }
            }
            foreach (BuilderDirectory dir in SubList)
            {
               if (dir.SupportsOpacity)
               {
                  return true;
               }
            }
            return false;
         }
      }

      public event BuilderChangedHandler BuilderChanged; 

      public void SubscribeToBuilderChangedEvent(BuilderChangedHandler handler)
      {
         BuilderChanged += handler;
      }

      [System.ComponentModel.Browsable(false)]
      public virtual bool SupportsMetaData
      {
         get { return false; }
      }

      [System.ComponentModel.Browsable(false)]
      public virtual string StyleSheetName
      {
         get { return null; }
      }

      public virtual XmlNode GetMetaData(XmlDocument oDoc)
      {
         return null;
      }

      #endregion


      #region Search

      public void GetLayerCount(bool bInterSect, WorldWind.GeographicBoundingBox extents, string strText, ref int iCount)
      {
         foreach (IBuilder builder in SubList)
            (builder as BuilderDirectory).GetLayerCount(bInterSect, extents, strText, ref iCount);
         foreach (IBuilder builder in LayerBuilders)
         {
            if (builder is ImageBuilder)
            {
               ImageBuilder imgBuilder = builder as ImageBuilder;

               if ((strText != string.Empty && imgBuilder.Name.IndexOf(strText, 0, StringComparison.InvariantCultureIgnoreCase) == -1) ||
                   (extents != null && bInterSect && !extents.IntersectsWith(imgBuilder.Extents) && !extents.Contains(imgBuilder.Extents)))
                  continue;

               iCount++;
            }
         }
      }

      public int iGetLayerCount(bool bInterSect, WorldWind.GeographicBoundingBox extents, string strText)
      {
         int iCount = 0;

         GetLayerCount(bInterSect, extents, strText, ref iCount);

         return iCount;
      }

      #endregion

      public static string TypeName
      {
         get
         {
            return "BuilderDirectory";
         }
      }

      public void UnsubscribeToBuilderChangedEvent(BuilderChangedHandler handler)
      {
         BuilderChanged -= handler;
      }

      #region ICloneable Members

      public virtual object Clone()
      {
         BuilderDirectory dir = new BuilderDirectory(m_strName, m_Parent, m_Removable);
         foreach (IBuilder builder in SubList)
         {
            dir.SubList.Add(builder.Clone() as BuilderDirectory);
         }
         foreach (IBuilder builder in LayerBuilders)
         {
            dir.LayerBuilders.Add(builder.Clone() as LayerBuilder);
         }
         return dir;
      }

      #endregion
   }

   public abstract class ServerBuilder : BuilderDirectory
   {
      protected string m_strUrl;
      
      public ServerBuilder(string name, IBuilder parent, string url)
         : base(name, parent, true)
      {
         m_strUrl = url;
      }

      public string URL
      {
         get
         {
            return m_strUrl;
         }
      }
   }
}
