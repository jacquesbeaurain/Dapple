using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Dapple.LayerGeneration;

namespace Geosoft.DotNetTools
{
	/// <summary>
	/// Open up interface to underlying state imagelist in win32 tree view
	/// </summary>
   public class TriStateTreeView : System.Windows.Forms.TreeView
   {
      #region Events
      /// <summary>
      /// Subscribe to TreeNodeChecked event
      /// </summary>
      public event TreeNodeCheckedEventHandler   TreeNodeChecked;
      
      /// <summary>
      /// Notify all subscribers that a tree node was checked
      /// </summary>
      /// <param name="e"></param>
      protected virtual void OnTreeNodeChecked(TreeNodeCheckedEventArgs e)
      {
         if (TreeNodeChecked != null)
            TreeNodeChecked(this, e);
      }
      #endregion

      #region Enum
      /// <summary>
      /// List of possible checkbox states
      /// </summary>
      public enum CheckBoxState
      {
         /// <summary></summary>
         None = 0,
         /// <summary></summary>
         Unchecked = 1,
         /// <summary></summary>
         Checked = 2,
         /// <summary></summary>
         Indeterminate = 3
      }
      #endregion

      #region Win32 TreeView Interface
      // --- Message defines ---
      private const UInt32 TV_FIRST = 4352;
      private const UInt32 TVM_SETIMAGELIST = TV_FIRST + 9;
      private const UInt32 TVM_GETNEXTITEM = TV_FIRST + 10;
      private const UInt32 TVM_GETITEM = TV_FIRST + 12;
      private const UInt32 TVM_SETITEM = TV_FIRST + 13;
      private const UInt32 TVM_HITTEST = TV_FIRST + 17;

      // --- TVM_SETIMAGELIST image list kind ---
      private const UInt32 TVSIL_NORMAL = 0;
      private const UInt32 TVSIL_STATE   = 2;
      
      // --- TVITEM.mask flags ---
      private const UInt32 TVIF_STATE = 8;
      private const UInt32 TVIF_HANDLE = 16;      

      // --- TVITEM.state flags ---
      private const UInt32 TVIS_STATEIMAGEMASK = 61440;

      // --- TVHITTESTINFO.flags flags ---
      private const UInt32 TVHT_ONITEMSTATEICON = 64;
      
      
      private const UInt32 TVGN_ROOT = 0;

      // --- Use a sequential structure layout to define TVITEM for the TreeView. ---
      [StructLayout(LayoutKind.Sequential, Pack=8, CharSet=CharSet.Auto)]
      private struct TVITEM
      {
         public uint     mask;
         public IntPtr   hItem;
         public uint     state;
         public uint     stateMask;
         public IntPtr   pszText;
         public int      cchTextMax;
         public int      iImage;
         public int      iSelectedImage;
         public int      cChildren;
         public IntPtr   lParam;
      }

      [StructLayout(LayoutKind.Sequential)]
      private struct POINTAPI
      {
         public int x;
         public int y;
      }
                                                      
      [StructLayout(LayoutKind.Sequential)]
      private struct TVHITTESTINFO
      {
         public POINTAPI   pt;
         public int        flags;
         public IntPtr     hItem;

      }

      // Declare two overloaded SendMessage functions. The
      // difference is in the last parameter: one is ByVal and the
      // other is ByRef.
      [DllImport("user32.dll")]
      private static extern UInt32 SendMessage(IntPtr hWnd, UInt32 Msg, UInt32 wParam, IntPtr lParam);

      [DllImport("User32", CharSet=CharSet.Auto)]
      private static extern IntPtr SendMessage(IntPtr hWnd, UInt32 msg, UInt32 wParam, ref TVITEM lParam);

      [DllImport("User32")]
      private static extern IntPtr SendMessage(IntPtr hWnd, UInt32 msg, UInt32 wParam, ref TVHITTESTINFO lParam);

      private const Int32 IMG_CHECKBOX_NONE = 0;
      private const Int32 IMG_CHECKBOX_UNCHECKED = 1;
      private const Int32 IMG_CHECKBOX_CHECKED = 2;
      private const Int32 IMG_CHECKBOX_INDETERMINATE = 3;
      #endregion

      #region Member Variables
      /// <summary>
      /// Checkbox image list
      /// </summary>
      protected System.Windows.Forms.ImageList  m_hCheckboxImageList;
      /// <summary>
      /// Whether the user hase MouseDown'd atop a draggable item.
      /// </summary>
      protected bool m_bDragDropEnabled = false;
      /// <summary>
      /// Where the mouse position is during a drag and drop (screen 
      /// </summary>
      protected System.Drawing.Point m_oLastDragOver = new System.Drawing.Point(-1, -1);
      #endregion

      #region Constructor
      /// <summary>
      /// 	<para>Initializes an instance of the <see cref="TriStateTreeView"/> class.</para>
      /// </summary>
      public TriStateTreeView()
      {
         System.Drawing.Icon              hChecked;
         System.Drawing.Icon              hIndeterminate;
         System.Drawing.Icon              hNone;
         System.Drawing.Icon              hUnchecked;
         System.IO.Stream                 hStrm;

#if !DAPPLE
         string strNameSpace = "Geosoft.DotNetTools.";
#else
         string strNameSpace = "Dapple.";
#endif

         hStrm = this.GetType().Assembly.GetManifestResourceStream(strNameSpace + "TriStateTreeView.StateChecked16.ico");
         hChecked = new System.Drawing.Icon(hStrm);

         hStrm = this.GetType().Assembly.GetManifestResourceStream(strNameSpace + "TriStateTreeView.StateIndeterminate16.ico");
         hIndeterminate = new System.Drawing.Icon(hStrm);

         hStrm = this.GetType().Assembly.GetManifestResourceStream(strNameSpace + "TriStateTreeView.StateNone16.ico");
         hNone = new System.Drawing.Icon(hStrm);

         hStrm = this.GetType().Assembly.GetManifestResourceStream(strNameSpace + "TriStateTreeView.StateUnchecked16.ico");
         hUnchecked = new System.Drawing.Icon(hStrm);

         m_hCheckboxImageList = new System.Windows.Forms.ImageList();
         m_hCheckboxImageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth24Bit;
         m_hCheckboxImageList.ImageSize = new System.Drawing.Size(16,16);
         m_hCheckboxImageList.Images.Add(hNone);
         m_hCheckboxImageList.Images.Add(hUnchecked);
         m_hCheckboxImageList.Images.Add(hChecked);         
         m_hCheckboxImageList.Images.Add(hIndeterminate);    

         SendMessage(Handle, TVM_SETIMAGELIST, TVSIL_STATE, m_hCheckboxImageList.Handle);

         this.AllowDrop = true;
      }      
      #endregion

      #region Public Member Functions
      /// <summary>
      /// Get the checkbox state of the current node
      /// </summary>
      /// <param name="hNode"></param>
      /// <returns></returns>
      public virtual CheckBoxState GetState(System.Windows.Forms.TreeNode hNode)
      {
         TVITEM         hItem = new TVITEM();
         uint           iState;
         IntPtr         iResult;

         CheckBoxState  eState = CheckBoxState.None;

         hItem.mask = TVIF_HANDLE | TVIF_STATE;
         hItem.hItem = hNode.Handle;
         hItem.stateMask = TVIS_STATEIMAGEMASK;
         hItem.state = 0;

         iResult = SendMessage(Handle, TVM_GETITEM, 0, ref hItem);
         if (iResult != IntPtr.Zero)
         {
            iState = hItem.state;
            iState = iState / 0xFFF;

            switch (iState)
            {
               case IMG_CHECKBOX_NONE:
                  eState = CheckBoxState.None;
                  break;
               case IMG_CHECKBOX_UNCHECKED:
                  eState = CheckBoxState.Unchecked;
                  break;
               case IMG_CHECKBOX_CHECKED:
                  eState = CheckBoxState.Checked;
                  break;
               case IMG_CHECKBOX_INDETERMINATE:
                  eState = CheckBoxState.Indeterminate;
                  break;
            }
         }
         return eState;
      }

      /// <summary>
      /// Set the checkbox state of the node
      /// </summary>
      /// <param name="hNode"></param>
      /// <param name="eState"></param>
      public virtual void SetState(System.Windows.Forms.TreeNode hNode, CheckBoxState eState)
      {
         TVITEM   hItem = new TVITEM();
         uint     iImageIndex = IMG_CHECKBOX_NONE;

         if (hNode != null)
         {
            switch (eState)
            {
               case CheckBoxState.None:
                  iImageIndex = IMG_CHECKBOX_NONE;
                  break;
               case CheckBoxState.Unchecked:
                  iImageIndex = IMG_CHECKBOX_UNCHECKED;
                  break;
               case CheckBoxState.Checked:
                  iImageIndex = IMG_CHECKBOX_CHECKED;
                  break;
               case CheckBoxState.Indeterminate:
                  iImageIndex = IMG_CHECKBOX_INDETERMINATE;
                  break;
            }

            hItem.mask = TVIF_HANDLE | TVIF_STATE;
            hItem.hItem = hNode.Handle;
            hItem.stateMask = TVIS_STATEIMAGEMASK;
            hItem.state = iImageIndex * 0x1000;

            SendMessage(Handle, TVM_SETITEM, 0, ref hItem);
            OnTreeNodeChecked(new TreeNodeCheckedEventArgs(hNode, eState));
         }
      }

      /// <summary>
      /// Add a new node to the tree
      /// </summary>
      /// <param name="hParent"></param>
      /// <param name="strNodeText"></param>      
      /// <returns></returns>
      public virtual System.Windows.Forms.TreeNode Add(System.Windows.Forms.TreeNode hParent, string strNodeText)
      {
         System.Windows.Forms.TreeNode hNode;

         hNode = new System.Windows.Forms.TreeNode(strNodeText);
         
         if (hParent != null)
            hParent.Nodes.Add(hNode);
         else 
            this.Nodes.Add(hNode);
         
         SetState(hNode, CheckBoxState.None);

         return hNode;
      }

      /// <summary>
      /// Add a new node to the tree
      /// </summary>
      /// <param name="hParent"></param>
      /// <param name="strNodeText"></param>
      /// <param name="iImageIndex"></param>
      /// <param name="iSelectedImageIndex"></param>
      /// <param name="eState"></param>      
      /// <returns></returns>
      public virtual System.Windows.Forms.TreeNode Add(System.Windows.Forms.TreeNode hParent, string strNodeText, int iImageIndex, int iSelectedImageIndex, CheckBoxState eState)
      {
         System.Windows.Forms.TreeNode hNode;

         hNode = new System.Windows.Forms.TreeNode(strNodeText);
         hNode.ImageIndex = iImageIndex;
         hNode.SelectedImageIndex = iSelectedImageIndex;
         
         if (hParent != null)
            hParent.Nodes.Add(hNode);
         else 
            this.Nodes.Add(hNode);
         
         SetState(hNode, eState);

         return hNode;
      }

      /// <summary>
      /// Add a new node to the tree at the top
      /// </summary>
      /// <param name="hParent"></param>
      /// <param name="strNodeText"></param>
      /// <param name="iImageIndex"></param>
      /// <param name="iSelectedImageIndex"></param>
      /// <param name="eState"></param>      
      /// <returns></returns>
      public virtual System.Windows.Forms.TreeNode AddTop(System.Windows.Forms.TreeNode hParent, string strNodeText, int iImageIndex, int iSelectedImageIndex, CheckBoxState eState)
      {
         System.Windows.Forms.TreeNode hNode;

         hNode = new System.Windows.Forms.TreeNode(strNodeText);
         hNode.ImageIndex = iImageIndex;
         hNode.SelectedImageIndex = iSelectedImageIndex;

         if (hParent != null)
            hParent.Nodes.Insert(0, hNode);
         else
            this.Nodes.Insert(0, hNode);

         SetState(hNode, eState);

         return hNode;
      }

      #endregion

      #region Protected Member Functions      
      /// <summary>
      /// Toggle checkbox state
      /// </summary>
      /// <param name="hNode"></param>
      private void ToggleState(System.Windows.Forms.TreeNode hNode)
      {
         CheckBoxState eState;

         eState = GetState(hNode);

         BeginUpdate();

         switch(eState)
         {
            case CheckBoxState.Unchecked:
            case CheckBoxState.Indeterminate:
               SetState(hNode, CheckBoxState.Checked);               
               break;
            case CheckBoxState.Checked:
               SetState(hNode, CheckBoxState.Unchecked);
               break;
         }         
         EndUpdate();
      }

      /// <summary>
      /// Get the node at the specified position
      /// </summary>
      /// <param name="iX"></param>
      /// <param name="iY"></param>
      /// <returns></returns>
      private System.Windows.Forms.TreeNode GetNodeAtCheckBox(int iX, int iY)
      {
         System.Windows.Forms.TreeNode hNode = null;
         TVHITTESTINFO                 hInfo = new TVHITTESTINFO();
         IntPtr                        iResult;

         hInfo.pt.x = iX;
         hInfo.pt.y = iY;

         iResult = SendMessage(Handle, TVM_HITTEST, 0, ref hInfo);
         if (iResult != IntPtr.Zero)
         {
            if ((hInfo.flags & TVHT_ONITEMSTATEICON) != 0)
               hNode = System.Windows.Forms.TreeNode.FromHandle(this, iResult);
         }
         return hNode;
      }

      /// <summary>
      /// Set the tree control to display checkboxes
      /// </summary>
      /// <param name="e"></param>
      protected override void OnInvalidated(System.Windows.Forms.InvalidateEventArgs e)
      {
         base.OnInvalidated (e);
         SendMessage(Handle, TVM_SETIMAGELIST, TVSIL_STATE, m_hCheckboxImageList.Handle);
      }
      #endregion

      #region Event Handlers
      /// <summary>
      /// Toggle the checkbox state when checkbox is clicked
      /// </summary>
      /// <param name="e"></param>
      protected override void OnMouseDown(System.Windows.Forms.MouseEventArgs e)
      {
         System.Windows.Forms.TreeNode hNode;

         base.OnMouseDown (e);

         hNode = GetNodeAtCheckBox(e.X, e.Y);
         if (hNode != null)
            ToggleState(hNode);
         else
         {
            hNode = GetNodeAt(e.X, e.Y);
            if (hNode != null)
            {
               m_bDragDropEnabled = true;
            }
         }
      }

      /// <summary>
      /// Start a drag and drop when the selected node moves.
      /// </summary>
      /// <param name="e"></param>
      protected override void OnMouseMove(MouseEventArgs e)
      {
         TreeNode hNode;

         base.OnMouseMove(e);

         if (m_bDragDropEnabled)
         {
            hNode = GetNodeAt(e.X, e.Y);
            this.Nodes.IndexOf(hNode);
            if (hNode != null)
            {
               DragDropEffects oEffects = DoDragDrop(hNode, DragDropEffects.Move);
            }
            m_bDragDropEnabled = false;
         }
      }

      /// <summary>
      /// Sets the insert location when the user drags something onto this control.
      /// </summary>
      /// <param name="drgevent"></param>
      protected override void OnDragEnter(DragEventArgs drgevent)
      {
         base.OnDragEnter(drgevent);

         setInsertLocation(drgevent.X, drgevent.Y);
      }

      /// <summary>
      /// Resets the insert location when the user keeps dragging something onto this control.
      /// </summary>
      /// <param name="drgevent"></param>
      protected override void OnDragOver(DragEventArgs drgevent)
      {
         base.OnDragOver(drgevent);

         setInsertLocation(drgevent.X, drgevent.Y);
      }

      /// <summary>
      /// Clears the insert location when the user stops dragging something onto this control.
      /// </summary>
      /// <param name="e"></param>
      protected override void OnDragLeave(EventArgs e)
      {
         base.OnDragLeave(e);

         clearInsertLocation();
      }

      /// <summary>
      /// Clears the insert location when the user drops something onto this control.
      /// </summary>
      /// <param name="drgevent"></param>
      protected override void OnDragDrop(DragEventArgs drgevent)
      {
         base.OnDragDrop(drgevent);

         clearInsertLocation();
      }

      /// <summary>
      /// Store the last known mouse position (for a drag/drop operation).
      /// </summary>
      /// <param name="iX"></param>
      /// <param name="iY"></param>
      private void setInsertLocation(int iX, int iY)
      {
         m_oLastDragOver.X = iX;
         m_oLastDragOver.Y = iY;
         this.Refresh();
         drawInsertLocationHint();
      }

      /// <summary>
      /// Clear the stored last known drag/drop mouse position.
      /// </summary>
      private void clearInsertLocation()
      {
         m_oLastDragOver.X = -1;
         m_oLastDragOver.Y = -1;
         this.Refresh();
      }

      /// <summary>
      /// Draw a line where an item dropped into this control will appear.
      /// </summary>
      private void drawInsertLocationHint()
      {
         System.Drawing.Graphics G = this.CreateGraphics();

         System.Drawing.Point oMouseInClientCoords = PointToClient(m_oLastDragOver);
         if (!G.VisibleClipBounds.Contains(oMouseInClientCoords)) return;
         int iLineWidth = (int)G.VisibleClipBounds.Width;
         int iLineY = 0;

         if (Nodes.Count == 0)
         {
            iLineY = 0;
         }
         else
         {
            TreeNode oNodeMouseIsOver = GetNodeAt(oMouseInClientCoords);

            if (oNodeMouseIsOver != null)
            {
               // Draw above current node
               iLineY = oNodeMouseIsOver.Bounds.Top;
            }
            else
            {
               // Draw below last node
               oNodeMouseIsOver = Nodes[Nodes.Count - 1];
               while (oNodeMouseIsOver.Nodes.Count != 0)
               {
                  oNodeMouseIsOver = oNodeMouseIsOver.Nodes[oNodeMouseIsOver.Nodes.Count - 1];
               }
               iLineY = oNodeMouseIsOver.Bounds.Bottom;
            }
         }

         G.DrawLine(new System.Drawing.Pen(System.Drawing.Brushes.Black, 3.0f), new System.Drawing.Point(0, iLineY), new System.Drawing.Point(iLineWidth, iLineY));

         G.Dispose();
      }

      /// <summary>
      /// Toggle the checkbox state when the spacebar is pressed
      /// </summary>
      /// <param name="e"></param>
      protected override void OnKeyUp(System.Windows.Forms.KeyEventArgs e)
      {
         base.OnKeyUp (e);

         if (e.KeyCode == System.Windows.Forms.Keys.Space)
         {
            if (SelectedNode != null)
               ToggleState(SelectedNode);
         }
      }

      /// <summary>
      /// Prevent the node from being expanded on a double click within the checkbox
      /// </summary>
      /// <param name="e"></param>
      protected override void OnBeforeExpand(System.Windows.Forms.TreeViewCancelEventArgs e)
      {
         System.Drawing.Point pt;
 
         pt = PointToClient(new System.Drawing.Point(MousePosition.X, MousePosition.Y));

         if (GetNodeAtCheckBox(pt.X, pt.Y) != null)
            e.Cancel = true;
      }

      /// <summary>
      /// Prevent node from being collapsed on a double click within the checkbox
      /// </summary>
      /// <param name="e"></param>
      protected override void OnBeforeCollapse(System.Windows.Forms.TreeViewCancelEventArgs e)
      {         
         System.Drawing.Point pt;
 
         pt = PointToClient(new System.Drawing.Point(MousePosition.X, MousePosition.Y));

         if (GetNodeAtCheckBox(pt.X, pt.Y) != null)
            e.Cancel = true;
      }

      #endregion
	}

   #region Events
   /// <summary>
   /// Event arguments for when a button is clicked within a datagrid
   /// </summary>
   public class TreeNodeCheckedEventArgs : EventArgs
   {
      #region Member Variables
      private System.Windows.Forms.TreeNode  m_hNode;
      private TriStateTreeView.CheckBoxState m_eState;
      #endregion

      #region Constructor
      /// <summary>
      /// Default constructor
      /// </summary>
      /// <param name="hNode"></param>
      public TreeNodeCheckedEventArgs(System.Windows.Forms.TreeNode hNode, TriStateTreeView.CheckBoxState eState)
      {
         m_hNode = hNode;
         m_eState = eState;
      }
      #endregion

      #region Properties
      /// <summary>
      /// Get the node
      /// </summary>
      public System.Windows.Forms.TreeNode Node
      {
         get { return m_hNode; }
      }      

      /// <summary>
      /// Get the state of this node
      /// </summary>
      public TriStateTreeView.CheckBoxState State
      {
         get { return m_eState; }
      }
      #endregion
   }

   /// <summary>
   /// Represent TreeNodeChecked event
   /// </summary>
   public delegate void TreeNodeCheckedEventHandler(object sender, TreeNodeCheckedEventArgs e);   
   #endregion
}
