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
	public partial class ServerTree : UserControl
	{
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

		public ServerTree()
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
				}
			};

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

					if (oDragData.Count > 0)
					{
						m_oModel.SelectedNode = oMouseNode.Tag as ModelNode;
						DragDropEffects dropEffect = this.DoDragDrop(oDragData, DragDropEffects.All);
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

		public void Attach(DappleModel oModel)
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

			oNodeToConfigure.Text = oNode.DisplayText;
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
			if (oBefore == null || oAfter == null) throw new ArgumentException("Need to handle this case");

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
			c_tvView.SelectedNode = oSelectedNode;

			c_tvView.ExpandAll();
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
