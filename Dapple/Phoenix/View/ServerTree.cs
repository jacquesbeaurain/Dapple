using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace NewServerTree.View
{
	internal partial class ServerTree : UserControl
	{
		#region Interop

		#region Structs

		/// <summary>
		/// Wrapper for Windows SCROLLINFO structure.
		/// </summary>
		/// <seealso cref="http://msdn.microsoft.com/en-us/library/bb787537(VS.85).aspx"/>
		private struct SCROLLINFO
		{
			#region Member Variables
			/// <summary>
			/// Specifies the size, in bytes, of this structure.
			/// </summary>
			internal uint cbSize;

			/// <summary>
			/// Specifies the scroll bar parameters to set or retrieve.
			/// </summary>
			internal SIF fMask;

			/// <summary>
			/// Specifies the minimum scrolling position.
			/// </summary>
			internal int nMin;

			/// <summary>
			/// Specifies the maximum scrolling position.
			/// </summary>
			internal int nMax;

			/// <summary>
			/// Specifies the page size.
			/// </summary>
			internal uint nPage;

			/// <summary>
			/// Specifies the position of the scroll box.
			/// </summary>
			internal int nPos;

			/// <summary>
			/// Specifies the immediate position of a scroll box that the user is dragging.
			/// </summary>
			internal int nTrackPos;

			#endregion

			#region Constructors

			internal SCROLLINFO(SIF mask, int min, int max, uint page, int pos, int trackPos)
			{
				cbSize = 28;
				fMask = mask;
				nMin = min;
				nMax = max;
				nPage = page;
				nPos = pos;
				nTrackPos = trackPos;
			}

			#endregion
		}

		#endregion


		#region Enums

		/// <summary>
		/// Enum for the Windows SB_* defines.
		/// </summary>
		private enum SB : int
		{
			HORZ = 0,
			VERT = 1,
			CTL = 2
		}

		/// <summary>
		/// Enum for the Win32 SIF_* defines.
		/// </summary>
		[System.Flags]
		private enum SIF : uint
		{
			RANGE = 0x0001,
			PAGE = 0x0002,
			POS = 0x0004,
			DISABLENOSCROLL = 0x0008,
			TRACKPOS = 0x0010,
			ALL = (RANGE | PAGE | POS | TRACKPOS)
		}

		#endregion


		#region Methods

		/// <summary>
		/// Import for the Win32 SetScrollInfo function. 
		/// </summary>
		/// <seealso cref="http://msdn.microsoft.com/en-us/library/bb787595(VS.85).aspx"/>
		[System.Runtime.InteropServices.DllImport("user32.dll")]
		private static extern int SetScrollInfo(IntPtr hwnd, SB fnBar, IntPtr lpsi, int fRedraw);


		/// <summary>
		/// Coder-friendly version of SetScrollInfo.
		/// </summary>
		/// <param name="hwnd">The window handle of the window to set scroll information for.</param>
		/// <param name="fnBar">Which scroll bar to set information for.</param>
		/// <param name="lpsi">The information to set.</param>
		/// <param name="fRedraw">Whether to repaint the window after changing the information.</param>
		private static void SetScrollPos(IntPtr hwnd, SB fnBar, SCROLLINFO lpsi, bool fRedraw)
		{
			int iSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(SCROLLINFO));
			IntPtr buffer = System.Runtime.InteropServices.Marshal.AllocHGlobal(iSize);
			System.Runtime.InteropServices.Marshal.StructureToPtr(
				lpsi, buffer, false);

			SetScrollInfo(hwnd, fnBar, buffer, 0);

			System.Runtime.InteropServices.Marshal.DestroyStructure(buffer, typeof(SCROLLINFO));
		}

		/// <summary>
		/// Import for the Windows GetScrollInfo function.
		/// </summary>
		/// <seealso cref="http://msdn.microsoft.com/en-us/library/bb787583(VS.85).aspx"/>
		[System.Runtime.InteropServices.DllImport("user32.dll", SetLastError=true)]
		private static extern int GetScrollInfo(IntPtr hwnd, SB fnBar, ref SCROLLINFO lpsi);

		/// <summary>
		/// Coder-friendly version of GetScrollInfo.
		/// </summary>
		/// <param name="hwnd">The window handle of the window to get scroll information for.</param>
		/// <param name="axis">Which axis to get information for.</param>
		/// <returns>A SCROLLINFO structure containing the requested information.</returns>
		private static SCROLLINFO? GetScrollInfo(IntPtr hwnd, SB fnBar)
		{
			SCROLLINFO hScrollInfo = new SCROLLINFO();
			hScrollInfo.cbSize = 28;
			hScrollInfo.fMask = SIF.POS | SIF.RANGE | SIF.PAGE;
			
			int result = GetScrollInfo(hwnd, fnBar, ref hScrollInfo);
			if (result == 0)
			{
				int error = System.Runtime.InteropServices.Marshal.GetLastWin32Error();

				if (error == 1447)
				{
					return null;
				}
				else
				{
					throw new ApplicationException("Call to GetScrollInfo failed with error code " + error);
				}
			}

			return hScrollInfo;
		}

		#endregion

		#endregion


		#region Constants

		/// <summary>
		/// Whether to color-code tree nodes based on their loadstate.
		/// </summary>
		private static readonly bool ColorTreeNodeText = false;

		#endregion


		#region Member Variables

		private DappleModel m_oModel;
		private Point? m_oDragStartPoint = null;

		#endregion


		#region Constructors

		internal ServerTree()
		{
			InitializeComponent();

			c_tvView.ImageList = IconKeys.ImageList;
		}

		#endregion


		#region Event Handlers

		#region Model Sourced

		void m_oModel_SelectedNodeChanged(object sender, EventArgs e)
		{
			Repopulate();
		}

		void m_oModel_NodeLoaded(object sender, NodeLoadEventArgs e)
		{
			MethodInvoker MethodBody = delegate()
			{
				// --- We only need to populate children if the loaded node is the selected node ---
				if (c_tvView.SelectedNode.Tag == e.Node)
				{
					c_tvView.BeginUpdate();

					RemoveMessageNodes(c_tvView.SelectedNode);
					AddChildTreeNodes(c_tvView.SelectedNode);

					c_tvView.ExpandAll();
					TreeIntegrityCheck();
					c_tvView.EndUpdate();

					c_tvView.BeginUpdate();
					RepositionScrollBars(c_tvView.SelectedNode);
					c_tvView.EndUpdate();
					c_tvView.Invalidate();
				}
			};

			if (c_tvView.IsHandleCreated)
				c_tvView.Invoke(MethodBody);
		}

		void m_oModel_NodeAdded(object sender, NodeAddedEventArgs e)
		{
			MethodInvoker MethodBody = delegate()
			{
				TreeNode oParentNode = c_tvView.SelectedNode;
				TreeNode oChildNode = null;

				while (oParentNode != null && oParentNode.Tag != e.Parent)
				{
					oChildNode = oParentNode;
					oParentNode = oParentNode.Parent;
				}

				if (oParentNode == null) throw new ApplicationException("I'm lost");

				if (oChildNode == null || e.Parent.ShowAllChildren)
				{
					// --- The node that got added was a child of the selected node ---
					if (oParentNode != c_tvView.SelectedNode) throw new ApplicationException("I'm lost");

					c_tvView.BeginUpdate();

					oParentNode.Nodes.Insert(e.Parent.GetIndex(e.Child), WrapModelNode(e.Child));

					c_tvView.ExpandAll();
					TreeIntegrityCheck();
					c_tvView.EndUpdate();
				}
			};

			if (c_tvView.IsHandleCreated)
				c_tvView.Invoke(MethodBody);
		}

		void m_oModel_NodeDisplayUpdated(object sender, NodeDisplayUpdatedEventArgs e)
		{
			MethodInvoker MethodBody = delegate()
			{
				TreeNode oUpdatedNode = FindNodeWithTag(e.Node);

				if (oUpdatedNode != null)
				{
					ConfigureTreeNodeDisplay(oUpdatedNode);
				}
			};

			if (c_tvView.IsHandleCreated)
				c_tvView.Invoke(MethodBody);
		}

		void m_oModel_NodeUnloaded(object sender, NodeUnloadedEventArgs e)
		{
			MethodInvoker MethodBody = delegate()
			{
				TreeNode oUnloadedNode = FindNodeWithTag(e.Node);

				if (oUnloadedNode != null)
				{
					ConfigureTreeNodeDisplay(oUnloadedNode);
					AddChildTreeNodes(oUnloadedNode);
				}
			};

			if (c_tvView.IsHandleCreated)
				c_tvView.Invoke(MethodBody);
		}

		void m_oModel_Loaded(object sender, EventArgs e)
		{
			Repopulate();
		}

		void m_oModel_SearchFilterChanged(object sender, EventArgs e)
		{
			Repopulate();
		}

		void m_oModel_FavouriteServerChanged(object sender, EventArgs e)
		{
			Repopulate();
		}

		void m_oModel_ServerToggled(object sender, EventArgs e)
		{
			Repopulate();
		}

		void ViewedDatasets_LayersRemoved(object sender, EventArgs e)
		{
			ConfigureEntireTreeDisplay();
		}

		void ViewedDatasets_LayersAdded(object sender, EventArgs e)
		{
			ConfigureEntireTreeDisplay();
		}

		private void UnmuteModel()
		{
			m_oModel.SelectedNodeChanged += new EventHandler(m_oModel_SelectedNodeChanged);
			m_oModel.NodeLoaded += new EventHandler<NodeLoadEventArgs>(m_oModel_NodeLoaded);
			m_oModel.NodeUnloaded += new EventHandler<NodeUnloadedEventArgs>(m_oModel_NodeUnloaded);
			m_oModel.NodeAdded += new EventHandler<NodeAddedEventArgs>(m_oModel_NodeAdded);
			m_oModel.NodeDisplayUpdated += new EventHandler<NodeDisplayUpdatedEventArgs>(m_oModel_NodeDisplayUpdated);
			m_oModel.Loaded += new EventHandler(m_oModel_Loaded);
			m_oModel.SearchFilterChanged += new EventHandler(m_oModel_SearchFilterChanged);
			m_oModel.FavouriteServerChanged += new EventHandler(m_oModel_FavouriteServerChanged);
			m_oModel.ServerToggled += new EventHandler(m_oModel_ServerToggled);

			m_oModel.ViewedDatasets.LayersRemoved += new EventHandler(ViewedDatasets_LayersRemoved);
			m_oModel.ViewedDatasets.LayersAdded += new EventHandler(ViewedDatasets_LayersAdded);
		}

		private void MuteModel()
		{
			m_oModel.SelectedNodeChanged -= new EventHandler(m_oModel_SelectedNodeChanged);
			m_oModel.NodeLoaded -= new EventHandler<NodeLoadEventArgs>(m_oModel_NodeLoaded);
			m_oModel.NodeUnloaded -= new EventHandler<NodeUnloadedEventArgs>(m_oModel_NodeUnloaded);
			m_oModel.NodeAdded -= new EventHandler<NodeAddedEventArgs>(m_oModel_NodeAdded);
			m_oModel.NodeDisplayUpdated -= new EventHandler<NodeDisplayUpdatedEventArgs>(m_oModel_NodeDisplayUpdated);
			m_oModel.Loaded -= new EventHandler(m_oModel_Loaded);
			m_oModel.SearchFilterChanged -= new EventHandler(m_oModel_SearchFilterChanged);
			m_oModel.FavouriteServerChanged -= new EventHandler(m_oModel_FavouriteServerChanged);
			m_oModel.ServerToggled -= new EventHandler(m_oModel_ServerToggled);

			m_oModel.ViewedDatasets.LayersRemoved -= new EventHandler(ViewedDatasets_LayersRemoved);
			m_oModel.ViewedDatasets.LayersAdded -= new EventHandler(ViewedDatasets_LayersAdded);
		}

		#endregion

		#region Controller Sourced

		TreeNode before;
		private void c_tvView_BeforeSelect(object sender, TreeViewCancelEventArgs e)
		{
			if (e.Node.Tag is IAnnotationModelNode)
			{
				e.Cancel = true;
				return;
			}

			before = c_tvView.SelectedNode;
		}

		TreeNode after;
		private void c_tvView_AfterSelect(object sender, TreeViewEventArgs e)
		{
			after = c_tvView.SelectedNode;

			Reconfigure();
		}

		private void c_tvView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
		{
			if ((e.Button & MouseButtons.Right) == MouseButtons.Right && e.Node.Tag is ErrorModelNode)
			{
				ErrorModelNode emn = e.Node.Tag as ErrorModelNode;

				if (!String.IsNullOrEmpty(emn.AdditionalInfo))
				{
					MessageBox.Show(emn.AdditionalInfo, "Additional Error Information", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
				}

				return;
			}

			if ((e.Button & MouseButtons.Right) == MouseButtons.Right)
			{
				c_tvView.SelectedNode = e.Node;

				if (c_tvView.SelectedNode.Tag is IContextModelNode)
				{
					System.Windows.Forms.ContextMenuStrip oStrip = new ContextMenuStrip();

					foreach (ToolStripMenuItem oItem in (c_tvView.SelectedNode.Tag as IContextModelNode).MenuItems)
					{
						oStrip.Items.Add(oItem);
					}

					if (oStrip.Items.Count > 0)
					{
						oStrip.Show(this, e.Location.X, e.Node.Bounds.Y + e.Node.Bounds.Height / 2);
					}
				}
			}
		}

		private void c_tvView_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
		{
			if (e.Node.Tag is LayerModelNode)
			{
				(e.Node.Tag as LayerModelNode).AddToVisibleLayers();
			}
		}

		private void c_tvView_BeforeCollapse(object sender, TreeViewCancelEventArgs e)
		{
			// --- Never collapse a treenode in the server tree---

			e.Cancel = true;
		}

		private void c_tvView_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				TreeNode oMouseNode = c_tvView.GetNodeAt(e.Location);

				if (oMouseNode != null && oMouseNode.Tag is LayerModelNode)
				{
					m_oDragStartPoint = e.Location;
				}
			}
		}

		private void c_tvView_MouseMove(object sender, MouseEventArgs e)
		{
			if (m_oDragStartPoint != null)
			{
				if (e.Button == MouseButtons.Left && m_oDragStartPoint.Value != e.Location)
				{
					List<Dapple.LayerGeneration.LayerBuilder> oDragData = new List<Dapple.LayerGeneration.LayerBuilder>();
					TreeNode oMouseNode = c_tvView.GetNodeAt(m_oDragStartPoint.Value);

					if (oMouseNode != null && oMouseNode.Tag is LayerModelNode)
					{
#pragma warning disable 618
						oDragData.Add((oMouseNode.Tag as LayerModelNode).ConvertToLayerBuilder());
#pragma warning restore 618
					}

					if (oDragData.Count > 0 && oMouseNode != null)
					{
						m_oModel.SelectedNode = oMouseNode.Tag as ModelNode;
					}
				}

				m_oDragStartPoint = null;
			}
		}

		private void UnmuteController()
		{
			c_tvView.BeforeSelect += new TreeViewCancelEventHandler(c_tvView_BeforeSelect);
			c_tvView.AfterSelect += new TreeViewEventHandler(c_tvView_AfterSelect);
		}

		private void MuteController()
		{
			c_tvView.BeforeSelect -= new TreeViewCancelEventHandler(c_tvView_BeforeSelect);
			c_tvView.AfterSelect -= new TreeViewEventHandler(c_tvView_AfterSelect);
		}

		#endregion

		#endregion


		#region Public Methods

		internal void Attach(DappleModel oModel)
		{
			if (m_oModel != null) MuteModel();
			m_oModel = oModel;
			if (m_oModel != null) UnmuteModel();

			m_oModel_SelectedNodeChanged(this, EventArgs.Empty);
		}

		#endregion


		#region Helper Methods

		#region Tree Manipulation

		private TreeNode WrapModelNode(ModelNode oNode)
		{
			TreeNode result = new TreeNode();
			result.Tag = oNode;
			ConfigureTreeNodeDisplay(result);
			return result;
		}

		private void ConfigureTreeNodeDisplay(TreeNode oNodeToConfigure)
		{
			ModelNode oNode = oNodeToConfigure.Tag as ModelNode;

			oNodeToConfigure.Text = oNode.DisplayText + (String.IsNullOrEmpty(oNode.Annotation) ? String.Empty : " " + oNode.Annotation);
			oNodeToConfigure.ImageKey = oNode.IconKey;
			oNodeToConfigure.SelectedImageKey = oNode.IconKey;
			oNodeToConfigure.ForeColor = SystemColors.ControlText;

			if (ColorTreeNodeText)
			{
				switch (oNode.LoadState)
				{
					case LoadState.Unloaded:
						oNodeToConfigure.ForeColor = Color.Orange;
						break;
					case LoadState.Loading:
						oNodeToConfigure.ForeColor = Color.RosyBrown;
						break;
					case LoadState.LoadSuccessful:
						oNodeToConfigure.ForeColor = Color.Black;
						break;
					case LoadState.LoadFailed:
						oNodeToConfigure.ForeColor = Color.Red;
						break;
					default:
						throw new ApplicationException("Missing enumeration case statement");
				}
			}

			if (oNode is ServerModelNode && (oNode as ServerModelNode).Favourite) oNodeToConfigure.NodeFont = new Font(c_tvView.Font, FontStyle.Bold);
			if (oNode is ServerModelNode && !(oNode as ServerModelNode).Enabled) oNodeToConfigure.ForeColor = Color.Gray;

			if (oNode is LayerModelNode && (oNode as LayerModelNode).Visible) oNodeToConfigure.ForeColor = Color.ForestGreen;
		}

		private void ConfigureEntireTreeDisplay()
		{
			_ConfigureEntireTreeDisplayRecursive(c_tvView.Nodes[0]);
		}

		private void _ConfigureEntireTreeDisplayRecursive(TreeNode oNode)
		{
			ConfigureTreeNodeDisplay(oNode);

			foreach (TreeNode oChildNode in oNode.Nodes)
			{
				_ConfigureEntireTreeDisplayRecursive(oChildNode);
			}
		}

		/// <summary>
		/// Add all child TreeNodes to a node.
		/// </summary>
		/// <param name="oNode">The node to populate.</param>
		private void AddChildTreeNodes(TreeNode oNode)
		{
			oNode.Nodes.Clear();

			foreach (ModelNode oChild in (oNode.Tag as ModelNode).FilteredChildren)
			{
				oNode.Nodes.Add(WrapModelNode(oChild));
			}
		}

		/// <summary>
		/// Removes all child TreeNodes from a node, sparing a single node.
		/// </summary>
		/// <param name="oNode">The node to depopulate.</param>
		/// <param name="oSpared">The node to leave in oNode.</param>
		private static void RemoveAllChildrenExcept(TreeNode oNode, TreeNode oSpared)
		{
			if ((oNode.Tag as ModelNode).ShowAllChildren) return;

			for (int count = oNode.Nodes.Count - 1; count >= 0; count--)
			{
				if (oNode.Nodes[count] != oSpared)
				{
					oNode.Nodes.RemoveAt(count);
				}
			}
		}

		private static void RemoveMessageNodes(TreeNode oNode)
		{
			for (int count = oNode.Nodes.Count - 1; count >= 0; count--)
			{
				if (oNode.Nodes[count].Tag is MessageModelNode)
				{
					oNode.Nodes.RemoveAt(count);
				}
			}
		}

		private void ReconfigureTree(TreeNode oBefore, TreeNode oAfter)
		{
			if (oBefore == null) return;
			if (oAfter == null) throw new ArgumentException("Need to handle this case");

			if (oBefore == oAfter) throw new ApplicationException("How did we manage this?");

			TreeNode iter;
			List<TreeNode> oBeforeList = new List<TreeNode>();
			List<TreeNode> oAfterList = new List<TreeNode>();
			for (iter = oBefore; iter != null; iter = iter.Parent)
			{
				oBeforeList.Add(iter);
			}
			for (iter = oAfter; iter != null; iter = iter.Parent)
			{
				oAfterList.Add(iter);
			}
			oBeforeList.Reverse();
			oAfterList.Reverse();

			bool blClickedAncestor = oBeforeList.Contains(oAfter);
			bool blClickedDescendant = oAfterList.Contains(oBefore);

			Debug.Assert(!(blClickedAncestor && blClickedDescendant));

			TreeNode oCommonAncestor = null;
			while (oBeforeList.Count > 0 && oAfterList.Count > 0 && oBeforeList[0] == oAfterList[0])
			{
				oCommonAncestor = oBeforeList[0];
				oBeforeList.RemoveAt(0);
				oAfterList.RemoveAt(0);
			}

			oBeforeList.Reverse();


			// --- Close the tree up backwards ---

			foreach (TreeNode oNode in oBeforeList)
			{
				oNode.Nodes.Clear();
			}

			if (blClickedAncestor)
			{
				// --- Clear the common ancestor so it doesn't get duplicate nodes
				// --- When we fill at the next step

				oCommonAncestor.Nodes.Clear();
			}
			else
			{
				// --- Trim back all the nodes in this node ---

				if (!(oAfter.Tag as ModelNode).IsLeaf)
				{
					RemoveAllChildrenExcept(oCommonAncestor, oAfter);
				}
			}

			// --- Fill in nodes for selected node ---

			AddChildTreeNodes(oAfter);
		}

		/// <summary>
		/// Moves the scroll bars of the TreeView so that the given node is as near the upper-left as possible.
		/// </summary>
		/// <remarks>
		/// Should be called between c_tvView.BeginUpdate() and c_tvView.BeginUpdate().
		/// </remarks>
		/// <param name="oNode"></param>
		private void RepositionScrollBars(TreeNode oNode)
		{
			// --- Get the current scroll bar positions ---

			SCROLLINFO? hScrollInfo = GetScrollInfo(c_tvView.Handle, SB.HORZ);
			SCROLLINFO? vScrollInfo = GetScrollInfo(c_tvView.Handle, SB.VERT);

			if (hScrollInfo == null || vScrollInfo == null) return;

			// --- Get the selected node's bounding box in client coordinates, ignoring the scroll position --

			Rectangle oSelectedBounds = oNode.Bounds;
			oSelectedBounds.X += hScrollInfo.Value.nPos;
			oSelectedBounds.Y += vScrollInfo.Value.nPos * c_tvView.ItemHeight;

			// --- Calculate desired horizontal scroll position (measured in pixels) ---

			int iDesiredHScroll = oSelectedBounds.Left - 2 * c_tvView.Indent;
			int iMaxHScroll = hScrollInfo.Value.nMax - (int)hScrollInfo.Value.nPage + 1;
			if (iDesiredHScroll < 0) iDesiredHScroll = 0;
			if (iDesiredHScroll > iMaxHScroll) iDesiredHScroll = iMaxHScroll;

			// --- Calculate desired vertical scroll position (measured in TreeNodes) ---

			int iDesiredVScroll = oSelectedBounds.Y / c_tvView.ItemHeight - 1;
			if (iDesiredVScroll < 0) iDesiredVScroll = 0;
			if (iDesiredVScroll > vScrollInfo.Value.nMax) iDesiredVScroll = vScrollInfo.Value.nMax;

			// --- Update the scroll bar positions ---

			SetScrollPos(c_tvView.Handle, SB.HORZ, new SCROLLINFO(SIF.POS, 0, 0, 0, iDesiredHScroll, 0), false);
			SetScrollPos(c_tvView.Handle, SB.VERT, new SCROLLINFO(SIF.POS, 0, 0, 0, iDesiredVScroll, 0), false);
		}

		#endregion

		/// <summary>
		/// Completely tear down the tree and rebuild from scratch.
		/// </summary>
		private void Repopulate()
		{
			m_oModel.DoWithLock(new MethodInvoker(_RepopulateBody));
		}

		private void _RepopulateBody()
		{
			MuteController();

			c_tvView.BeginUpdate();
			c_tvView.Nodes.Clear();

			TreeNode oSelectedNode = new TreeNode();
			oSelectedNode.Tag = m_oModel.SelectedNode;
			AddChildTreeNodes(oSelectedNode);
			ConfigureTreeNodeDisplay(oSelectedNode);

			TreeNode oRootNode = CreateParentTreeNodes(m_oModel.SelectedNode.Parent, oSelectedNode);

			c_tvView.Nodes.Add(oRootNode);
			c_tvView.ExpandAll();
			c_tvView.SelectedNode = oSelectedNode;
			TreeIntegrityCheck();
			c_tvView.EndUpdate();

			UnmuteController();
		}

		private TreeNode CreateParentTreeNodes(ModelNode oModel, TreeNode oConstructedChild)
		{
			if (oModel == null)
			{
				return oConstructedChild;
			}

			TreeNode oThisNode = WrapModelNode(oModel);

			if (oModel.ShowAllChildren || (oConstructedChild.Tag as ModelNode).IsLeaf)
			{
				foreach (ModelNode oChild in oModel.FilteredChildren)
				{
					if (oConstructedChild != null && oChild == oConstructedChild.Tag)
					{
						oThisNode.Nodes.Add(oConstructedChild);
					}
					else
					{
						oThisNode.Nodes.Add(WrapModelNode(oChild));
					}
				}
			}
			else
			{
				oThisNode.Nodes.Add(oConstructedChild);
			}

			return CreateParentTreeNodes(oModel.Parent, oThisNode);
		}

		/// <summary>
		/// Selectively remove and add tree nodes based on the before and after TreeNodes set.
		/// </summary>
		private void Reconfigure()
		{
			m_oModel.DoWithLock(new MethodInvoker(_ReconfigureBody));
		}

		private void _ReconfigureBody()
		{
			MuteModel();

			m_oModel.SelectedNode = after.Tag as ModelNode;

			c_tvView.BeginUpdate();

			ConfigureTreeNodeDisplay(after);
			ReconfigureTree(before, after);

			c_tvView.ExpandAll();
			TreeIntegrityCheck();
			c_tvView.EndUpdate();

			if (!(c_tvView.SelectedNode.Tag as ModelNode).IsLeaf)
			{
				c_tvView.BeginUpdate();
				RepositionScrollBars(c_tvView.SelectedNode);
				c_tvView.EndUpdate();
				c_tvView.Invalidate();
			}

			UnmuteModel();
		}

		private TreeNode FindNodeWithTag(ModelNode oTag)
		{
			return _FindNodeWithTag(c_tvView.Nodes[0], oTag);
		}

		private TreeNode _FindNodeWithTag(TreeNode oNode, ModelNode oTag)
		{
			if (oNode.Tag == oTag) return oNode;

			foreach (TreeNode oChidNode in oNode.Nodes)
			{
				TreeNode result = _FindNodeWithTag(oChidNode, oTag);
				if (result != null) return result;
			}

			return null;
		}

		#endregion


		#region Debugging Methods

		/// <summary>
		/// Checks that all the tree nodes have a Tag set.
		/// </summary>
		private void TreeIntegrityCheck()
		{
			__TreeIntegrityCheck(c_tvView.Nodes[0]);
		}

		private void __TreeIntegrityCheck(TreeNode oNode)
		{
			if (oNode.Tag == null) Debugger.Break();

			foreach (TreeNode oChild in oNode.Nodes)
				__TreeIntegrityCheck(oChild);
		}

		#endregion
	}
}
