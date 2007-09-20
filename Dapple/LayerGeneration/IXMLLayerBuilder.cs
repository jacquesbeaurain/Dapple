using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Windows.Forms;
using WorldWind.Renderable;
using Geosoft.DotNetTools;
using WorldWind;
using System.Threading;

namespace Dapple.LayerGeneration
{
   public class BuilderDirectory : IBuilder
   {
      private string m_strName;
      private IBuilder m_Parent;
      private bool m_Removable;
      protected List<LayerBuilder> m_colChildren;
      protected List<BuilderDirectory> m_colSublist;
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

      [System.ComponentModel.Browsable(false)]
      public bool IsChanged
      {
         get { return false; }
      }

      [System.ComponentModel.Browsable(false)]
      public IBuilder Parent
      {
         get { return m_Parent; }
      }

      [System.ComponentModel.Browsable(false)]
      public virtual string DisplayIconKey
      {
         get { return "folder"; }
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

      private void GetLayerCount(bool bInterSect, WorldWind.GeographicBoundingBox extents, string strText, ref int iCount)
      {
         if (strText == null) throw new ArgumentNullException("strText");

         foreach (IBuilder builder in SubList)
            (builder as BuilderDirectory).GetLayerCount(bInterSect, extents, strText, ref iCount);
         foreach (LayerBuilder builder in LayerBuilders)
         {
            if ((strText != string.Empty && builder.Name.IndexOf(strText, 0, StringComparison.InvariantCultureIgnoreCase) == -1) ||
                (extents != null && bInterSect && !extents.Intersects(builder.Extents) && !extents.Contains(builder.Extents)))
               continue;

            iCount++;
         }
      }

      public int iGetLayerCount(bool bInterSect, WorldWind.GeographicBoundingBox extents, string strText)
      {
         int iCount = 0;
         if (strText == null) strText = String.Empty;

         GetLayerCount(bInterSect, extents, strText, ref iCount);

         return iCount;
      }

      #endregion

      public void UnsubscribeToBuilderChangedEvent(BuilderChangedHandler handler)
      {
         BuilderChanged -= handler;
      }

      public LayerBuilder GetLayerBuilderByName(String layerName)
      {
         foreach (LayerBuilder b in m_colChildren)
         {
            if (layerName.Equals(b.Name)) return b;
         }
         foreach (BuilderDirectory bd in m_colSublist)
         {
            LayerBuilder result = bd.GetLayerBuilderByName(layerName);
            if (result != null) return result;
         }
         return null;
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

      #region IBuilder Members


      public virtual TreeNode[] getChildTreeNodes()
      {
         TreeNode[] result = new TreeNode[m_colChildren.Count + m_colSublist.Count];
         int index = 0;
         foreach(LayerBuilder childNode in m_colChildren)
         {
            result[index] = new TreeNode(childNode.Name, MainForm.ImageListIndex(childNode.DisplayIconKey), MainForm.ImageListIndex(childNode.DisplayIconKey));
            result[index].Tag = childNode;
            index++;
         }
         foreach (BuilderDirectory childDir in m_colSublist)
         {
            result[index] = new TreeNode(childDir.Name, MainForm.ImageListIndex(childDir.DisplayIconKey), MainForm.ImageListIndex(childDir.DisplayIconKey));
            result[index].Tag = childDir;
            index++;
         }
         return result;
      }

      #endregion

      /// <summary>
      /// Get all of the LayerBuilders throughout the tree.
      /// </summary>
      /// <param name="result">The ArrayList to fill with the LayerBuilders.</param>
      public void getLayerBuilders(ref List<LayerBuilder> result)
      {
         foreach (LayerBuilder oBuilder in m_colChildren)
            result.Add(oBuilder);
         foreach (BuilderDirectory oDirectory in m_colSublist)
            oDirectory.getLayerBuilders(ref result);
      }
   }

   public abstract class ServerBuilder : BuilderDirectory
   {
      protected ServerUri m_oUri;

      public ServerBuilder(string name, IBuilder parent, ServerUri oUri)
         : base(name, parent, true)
      {
         m_oUri = oUri;
      }

      /// <summary>
      /// The URL of this server.
      /// </summary>
      public ServerUri Uri
      {
         get { return m_oUri; }
      }

      [System.ComponentModel.Browsable(false)]
      public abstract System.Drawing.Icon Icon
      {
         get;
      }
   }

   /// <summary>
   /// A BuilderDirectory representing a builder that has asynchronous loading and thus may
   /// not be loaded immediately upon addition to a ServerTree.
   /// </summary>
   public abstract class AsyncBuilder : ServerBuilder
   {
      protected string m_strErrorMessage = String.Empty;
      protected bool m_blnIsLoading = true;
      private ManualResetEvent m_oLoadBlock = new ManualResetEvent(false);
      
      public AsyncBuilder(string name, IBuilder parent, ServerUri oUri)
         : base(name, parent, oUri)
      {
      }

      #region Properties
      /// <summary>
      /// Description of the error that occurred while loading the server.
      /// </summary>
      [System.ComponentModel.Browsable(false)]
      public String ErrorMessage
      {
         get { return m_strErrorMessage; }
      }

      /// <summary>
      /// Whether an error occurred while accessing this server for its service information.
      /// </summary>
      [System.ComponentModel.Browsable(false)]
      public bool LoadingErrorOccurred
      {
         get { return !m_strErrorMessage.Equals(String.Empty); }
      }

      /// <summary>
      /// Whether the server loaded successfully.  Eqivalent to saying !IsLoading && !LoadingErrorOccurred.
      /// </summary>
      [System.ComponentModel.Browsable(false)]
      public bool IsLoadedSuccessfully
      {
         get { return !IsLoading && !LoadingErrorOccurred; }
      }

      /// <summary>
      /// Whether the server is loading, or loading has been completed.
      /// </summary>
      [System.ComponentModel.Browsable(false)]
      public bool IsLoading
      {
         get { return m_blnIsLoading; }
      }

      [System.ComponentModel.Browsable(false)]
      public override string DisplayIconKey
      {
         get
         {
            if (LoadingErrorOccurred)
            {
               return "offline";
            }
            return "enserver";
         }
      }

      /// <summary>
      /// Get child nodes.
      /// </summary>
      /// <remarks>
      /// If this server is still loading, the only child is a temporary node.
      /// </remarks>
      /// <returns></returns>
      public override TreeNode[] getChildTreeNodes()
      {
         if (IsLoading)
         {
            TreeNode[] result = new TreeNode[1];
            result[0] = getLoadingNode();

            return result;
         }

         return base.getChildTreeNodes();
      }

      /// <summary>
      /// Get the TreeNode to display as the child of this server's TreeNode if it's not loaded.
      /// </summary>
      /// <returns></returns>
      protected virtual TreeNode getLoadingNode()
      {
         return new TreeNode("Retrieving Datasets...", MainForm.ImageListIndex("loading"), MainForm.ImageListIndex("loading"));
      }

      /// <summary>
      /// Updates a TreeNode to be loaded, loading, or broken.
      /// </summary>
      /// <param name="oParent">The TreeNode whose tag is this ServerBuilder.</param>
      /// <param name="oTree">The ServerTree which contains oParent.</param>
      public void updateTreeNode(TreeNode oParent, ServerTree oTree, bool blnAOIFilter, GeographicBoundingBox oAOI, String strSearch)
      {
         if (IsLoading)
         {
            oParent.ImageIndex = MainForm.ImageListIndex("enserver");
            oParent.SelectedImageIndex = MainForm.ImageListIndex("enserver");
            oParent.Text = Name;
         }
         else if (LoadingErrorOccurred)
         {
            oParent.ImageIndex = MainForm.ImageListIndex("offline");
            oParent.SelectedImageIndex = MainForm.ImageListIndex("offline");
            oParent.Text = Name + " (" + ErrorMessage + ")";
         }
         else
         {
            oParent.Text = Name + " (" + iGetLayerCount(blnAOIFilter, oAOI, strSearch).ToString() + ")";
         }
      }

      [System.ComponentModel.Browsable(false)]
      public abstract override System.Drawing.Icon Icon
      {
         get;
      }

      #endregion

      #region Assigning state
      /// <summary>
      /// Call when this server has been loaded successfully.
      /// </summary>
      public void SetLoadSuccessful()
      {
         m_blnIsLoading = false;
         m_strErrorMessage = String.Empty;
         m_oLoadBlock.Set();
      }

      /// <summary>
      /// Call when loading this server fails.
      /// </summary>
      /// <param name="strErrorMessage"></param>
      public void SetLoadFailed(String strErrorMessage)
      {
         m_blnIsLoading = false;
         m_strErrorMessage = strErrorMessage;
         m_oLoadBlock.Set();
      }

      /// <summary>
      /// Call to reset the loaded state of this server.
      /// </summary>
      public void SetUnloaded()
      {
         m_blnIsLoading = true;
         m_strErrorMessage = String.Empty;
         m_oLoadBlock.Set();
      }
      #endregion


      #region Waiting for load

      /// <summary>
      /// Block until this server has loaded (successfully or not).
      /// </summary>
      public void WaitUntilLoaded()
      {
         m_oLoadBlock.WaitOne();
      }

      #endregion
   }
}
