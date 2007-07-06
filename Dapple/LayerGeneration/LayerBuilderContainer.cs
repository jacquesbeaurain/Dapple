using System;
using System.Collections.Generic;
using System.Text;
using WorldWind;
using System.Windows.Forms;
using Geosoft.DotNetTools;
using WorldWind.PluginEngine;
using Dapple;
using Dapple.LayerGeneration;

namespace Dapple.LayerGeneration
{
	public class LayerBuilderContainer
	{
		private LayerBuilder m_builder;
		private LayerBuilder m_buildersource;
		private string m_name;
		private string m_uri;
		private bool m_visible;
		private bool m_temporary;
		private byte m_opacity;

		public LayerBuilderContainer(string strName, string strUri, bool visible, byte opacity)
		{
			m_name = strName;
			m_uri = strUri;
			m_visible = visible;
			m_opacity = opacity;
		}

		public LayerBuilderContainer(LayerBuilder builder)
			: this(builder, true)
		{
		}

		public LayerBuilderContainer(LayerBuilder builder, bool bClone)
		{
			m_buildersource = builder;
			if (bClone)
				m_builder = builder.Clone() as LayerBuilder;
			else
				m_builder = builder;
			m_uri = m_builder.GetURI();
			m_visible = m_builder.Visible;
			m_opacity = m_builder.Opacity;
			m_name = m_builder.Name;
		}

		public string Name
		{
			get
			{
				return m_name;
			}
			set
			{
				m_name = value;
			}
		}

		public string Uri
		{
			get
			{
				if (m_builder == null)
					return m_uri;
				else
					return m_builder.GetURI();
			}
		}

		public bool Visible
		{
			get
			{
				if (m_builder == null)
					return m_visible;
				else
					return m_builder.Visible;
			}
			set
			{
				m_visible = value;
				if (m_builder != null && m_builder.Visible != value)
					m_builder.Visible = value;
			}
		}


		public byte Opacity
		{
			get
			{
				if (m_builder == null)
					return m_opacity;
				else
					return m_builder.Opacity;
			}
			set
			{
				m_opacity = value;
				if (m_builder != null && m_builder.Opacity != value)
					m_builder.Opacity = value;
			}
		}

		public bool Temporary
		{
			get
			{
				return m_temporary;
			}
			set
			{
				m_temporary = value;
			}
		}

		public LayerBuilder Builder
		{
			get
			{
				return m_builder;
			}
		}

		public LayerBuilder SourceBuilder
		{
			get
			{
				return m_buildersource;
			}
			set
			{
				m_buildersource = value;
				if (m_buildersource != null)
				{
					m_builder = m_buildersource.Clone() as LayerBuilder;
					m_builder.Opacity = m_opacity;
					m_builder.Visible = m_visible;
				}
				else
					m_builder = null;
			}
		}
	}

	public class LayerBuilderList : List<LayerBuilderContainer>
	{
		MainForm m_mainWnd;
		TriStateTreeView m_treeList;
		WorldWindow m_worldWindow;

		public LayerBuilderList(MainForm mainWnd, TriStateTreeView tree, WorldWindow worldWindow)
		{
			m_mainWnd = mainWnd;
			m_treeList = tree;
			m_worldWindow = worldWindow;
		}

		public bool ContainsSource(LayerBuilder builder)
		{
			foreach (LayerBuilderContainer container in this)
			{
				if (container.SourceBuilder == builder)
					return true;
			}
			return false;
		}

		private void BuilderChanged(IBuilder builder, BuilderChangeType changeType)
		{
			if (m_treeList.InvokeRequired)
			{
				m_treeList.BeginInvoke(new BuilderChangedHandler(BuilderChanged), new object[] { builder, changeType });
				return;
			}

			if (builder is LayerBuilder)
			{
				LayerBuilder lbuilder = builder as LayerBuilder;
				foreach (TreeNode treenode in m_treeList.Nodes)
				{
					if (treenode.Tag is LayerBuilderContainer && (treenode.Tag as LayerBuilderContainer).Builder == lbuilder)
					{
						LayerBuilderContainer container = treenode.Tag as LayerBuilderContainer;

						if (changeType == BuilderChangeType.LoadedASync)
							RefreshLayersAndOrder();
						else if (changeType == BuilderChangeType.LoadedASyncFailed || changeType == BuilderChangeType.LoadedSyncFailed)
						{
							treenode.ImageIndex = treenode.SelectedImageIndex = m_mainWnd.ImageListIndex("error");
						}
						else if (changeType == BuilderChangeType.VisibilityChanged)
						{
							// Update container and some controls in main window if needed
							container.Visible = container.Builder.Visible;
							m_treeList.SetState(treenode, container.Visible ? TriStateTreeView.CheckBoxState.Checked : TriStateTreeView.CheckBoxState.Unchecked);
						}
						break;
					}
				}
			}
		}

		public void AsyncAddExisting(LayerBuilderContainer container)
		{
			if (container.Builder == null)
				return;

			container.Builder.SubscribeToBuilderChangedEvent(new BuilderChangedHandler(this.BuilderChanged));
			container.Builder.AsyncAddLayer();
		}

		public void Add(string strName, LayerBuilder builder, bool front, byte opacity, bool visible)
		{
			Add(strName, builder, front, opacity, visible, false);
		}

		public void Add(string strName, LayerBuilder builder, bool front, byte opacity, bool visible, bool temporary)
		{
			int iImageIndex;

			if (ContainsSource(builder))
				return;

			LayerBuilderContainer container = new LayerBuilderContainer(builder);
			container.Visible = visible;
			container.Opacity = opacity;
			container.Temporary = temporary;
			container.Name = strName;

			if (front)
				Insert(0, container);
			else
				Add(container);

			if (container.Builder is DAPQuadLayerBuilder)
			{
				DAPQuadLayerBuilder dapbuilder = (DAPQuadLayerBuilder)container.Builder;

				iImageIndex = m_mainWnd.ImageIndex(dapbuilder.DAPType.ToLower());
				if (iImageIndex == -1)
					m_mainWnd.ImageListIndex("layer");
			}
			else if (container.Builder is VEQuadLayerBuilder)
				iImageIndex = m_mainWnd.ImageListIndex("live");
			else if (container.Builder is GeorefImageLayerBuilder)
				iImageIndex = m_mainWnd.ImageListIndex("georef_image");
			else
				iImageIndex = m_mainWnd.ImageListIndex("layer");

			TreeNode treeNode;

			if (front)
				treeNode = m_treeList.AddTop(null, strName, iImageIndex, iImageIndex, container.Visible ? TriStateTreeView.CheckBoxState.Checked : TriStateTreeView.CheckBoxState.Unchecked);
			else
				treeNode = m_treeList.Add(null, strName, iImageIndex, iImageIndex, container.Visible ? TriStateTreeView.CheckBoxState.Checked : TriStateTreeView.CheckBoxState.Unchecked);
			treeNode.Tag = container;

			container.Builder.SubscribeToBuilderChangedEvent(new BuilderChangedHandler(this.BuilderChanged));

			if (container.Builder is GeorefImageLayerBuilder)
			{
				if (container.Builder.GetLayer() == null)
					treeNode.ImageIndex = treeNode.SelectedImageIndex = m_mainWnd.ImageListIndex("error");
				else
					container.Builder.SyncAddLayer(true);
				treeNode.ToolTipText = (container.Builder as GeorefImageLayerBuilder).FileName;
				RefreshLayersAndOrder();
			}
			else
				container.Builder.AsyncAddLayer();
		}

		public void RemoveAll()
		{
			List<LayerBuilderContainer> clonelist = new List<LayerBuilderContainer>(this);
			foreach (LayerBuilderContainer container in clonelist)
			{
				if (container.Builder != null)
				{
					container.Builder.RemoveLayer();
					container.Builder.UnsubscribeToBuilderChangedEvent(new BuilderChangedHandler(this.BuilderChanged));
				}
				base.Remove(container);
				m_treeList.Nodes.Clear();
			}
		}

		public void RemoveContainer(LayerBuilderContainer remove)
		{
			foreach (LayerBuilderContainer container in this)
			{
				if (container == remove)
				{
					if (container.Builder != null)
					{
						container.Builder.RemoveLayer();
						container.Builder.UnsubscribeToBuilderChangedEvent(new BuilderChangedHandler(this.BuilderChanged));
					}
					base.Remove(container);
					foreach (TreeNode node in m_treeList.Nodes)
					{
						if (node.Tag as LayerBuilderContainer == container)
						{
							node.Remove();
							break;
						}
					}
					break;
				}
			}
		}

		public void RefreshFromSource(LayerBuilderContainer refresh, LayerBuilder source)
		{
			foreach (LayerBuilderContainer container in this)
			{
				if (container == refresh)
				{
					if (container.Builder != null)
					{
						container.Builder.RemoveLayer();
						container.Builder.UnsubscribeToBuilderChangedEvent(new BuilderChangedHandler(this.BuilderChanged));
					}
					container.SourceBuilder = source;
					AsyncAddExisting(container);
					break;
				}
			}
		}

		public void RemoveOthers(LayerBuilderContainer remove)
		{
			List<LayerBuilderContainer> clonelist = new List<LayerBuilderContainer>(this);
			foreach (LayerBuilderContainer container in clonelist)
			{
				if (container != remove)
				{
					if (container.Builder != null)
					{
						container.Builder.RemoveLayer();
						container.Builder.UnsubscribeToBuilderChangedEvent(new BuilderChangedHandler(this.BuilderChanged));
					}
					base.Remove(container);
					foreach (TreeNode node in m_treeList.Nodes)
					{
						if (node.Tag as LayerBuilderContainer == container)
						{
							node.Remove();
							break;
						}
					}
				}
			}
		}

		public bool IsTop(LayerBuilderContainer container)
		{
			return (this.IndexOf(container) == 0);
		}

		public bool IsBottom(LayerBuilderContainer container)
		{
			return (this.IndexOf(container) == this.Count - 1);
		}

		public void MoveUp(LayerBuilderContainer container)
		{
			int iIndex = IndexOf(container);
			if (iIndex == 0)
				return;

			LayerBuilderContainer prevnode = this[iIndex - 1];
			this[iIndex - 1] = container;
			this[iIndex] = prevnode;
			foreach (TreeNode treenode in m_treeList.Nodes)
			{
				if (treenode.Tag as LayerBuilderContainer == container)
				{
					int iNewIndex = m_treeList.Nodes.IndexOf(treenode) - 1;
					treenode.Remove();
					m_treeList.Nodes.Insert(iNewIndex, treenode);
					m_treeList.SetState(treenode, container.Visible ? TriStateTreeView.CheckBoxState.Checked : TriStateTreeView.CheckBoxState.Unchecked);
					m_treeList.SelectedNode = treenode;
					break;
				}
			}
			RefreshLayersAndOrder();
		}

		public void MoveDown(LayerBuilderContainer container)
		{
			int iIndex = IndexOf(container);
			if (iIndex == this.Count - 1)
				return;

			LayerBuilderContainer nextnode = this[iIndex + 1];
			this[iIndex + 1] = container;
			this[iIndex] = nextnode;
			foreach (TreeNode treenode in m_treeList.Nodes)
			{
				if (treenode.Tag as LayerBuilderContainer == container)
				{
					int iNewIndex = m_treeList.Nodes.IndexOf(treenode) + 1;
					treenode.Remove();
					m_treeList.Nodes.Insert(iNewIndex, treenode);
					m_treeList.SetState(treenode, container.Visible ? TriStateTreeView.CheckBoxState.Checked : TriStateTreeView.CheckBoxState.Unchecked);
					m_treeList.SelectedNode = treenode;
					break;
				}
			}
			RefreshLayersAndOrder();
		}

		public void MoveTop(LayerBuilderContainer container)
		{
			int iIndex = IndexOf(container);
			if (iIndex == 0)
				return;

			Remove(container);
			Insert(0, container);
			foreach (TreeNode treenode in m_treeList.Nodes)
			{
				if (treenode.Tag as LayerBuilderContainer == container)
				{
					treenode.Remove();
					m_treeList.Nodes.Insert(0, treenode);
					m_treeList.SetState(treenode, container.Visible ? TriStateTreeView.CheckBoxState.Checked : TriStateTreeView.CheckBoxState.Unchecked);
					m_treeList.SelectedNode = treenode;
					break;
				}
			}
			RefreshLayersAndOrder();
		}

		public void MoveBottom(LayerBuilderContainer container)
		{
			int iIndex = IndexOf(container);
			if (iIndex == this.Count - 1)
				return;

			Remove(container);
			Add(container);
			foreach (TreeNode treenode in m_treeList.Nodes)
			{
				if (treenode.Tag as LayerBuilderContainer == container)
				{
					treenode.Remove();
					m_treeList.Nodes.Add(treenode);
					m_treeList.SetState(treenode, container.Visible ? TriStateTreeView.CheckBoxState.Checked : TriStateTreeView.CheckBoxState.Unchecked);
					m_treeList.SelectedNode = treenode;
					break;
				}
			}
			RefreshLayersAndOrder();
		}

		public void AddUsingUri(string strName, string strUri, bool visible, byte opacity, bool front, ServerTree serverTree, ref bool bOldView)
      {
         string strError = string.Empty;
         LayerBuilder builder = null;

         try
         {
            if (strUri.StartsWith(GeorefImageLayerBuilder.URLProtocolName))
               builder = GeorefImageLayerBuilder.GetBuilderFromURI(strUri, MainApplication.Settings.CachePath, m_worldWindow.CurrentWorld, null);
            else if (strUri.StartsWith(VEQuadLayerBuilder.URLProtocolName))
               builder = VEQuadLayerBuilder.GetBuilderFromURI(strUri, m_mainWnd, null);
            else if (strUri.StartsWith(NltQuadLayerBuilder.URLProtocolName))
               builder = NltQuadLayerBuilder.GetQuadLayerBuilderFromURI(strUri, MainApplication.Settings.CachePath, m_worldWindow.CurrentWorld, null);
            else if (strUri.StartsWith(DAPQuadLayerBuilder.URISchemeName))
               builder = DAPQuadLayerBuilder.GetBuilderFromURI(strUri, serverTree, MainApplication.Settings.CachePath, m_worldWindow, ref bOldView);
         }
         catch (Exception e)
         {
            strError = e.Message;
         }

         if (builder != null)
            Add(strName, builder, front, opacity, visible);
         else
         {
            int iImage;

            if (!bOldView)
            {
               // --- These layers should be populated in catalog loading delegates (or they are errors) ---
               LayerBuilderContainer container = new LayerBuilderContainer(strName, strUri, visible, opacity);

               if (strError != string.Empty)
                  iImage = m_mainWnd.ImageListIndex("error");
               else
                  iImage = m_mainWnd.ImageListIndex("time");

               TreeNode treeNode;
               if (front)
               {
                  Insert(0, container);
                  treeNode = m_treeList.AddTop(null, strName, iImage, iImage, container.Visible ? TriStateTreeView.CheckBoxState.Checked : TriStateTreeView.CheckBoxState.Unchecked);
               }
               else
               {
                  Add(container);
                  treeNode = m_treeList.Add(null, strName, iImage, iImage, container.Visible ? TriStateTreeView.CheckBoxState.Checked : TriStateTreeView.CheckBoxState.Unchecked);
               }
               treeNode.Tag = container;
            }
         }
      }

		public void RefreshBuilder(LayerBuilder builder)
		{
			foreach (TreeNode treenode in m_treeList.Nodes)
			{
				if (treenode.Tag is LayerBuilderContainer && (treenode.Tag as LayerBuilderContainer).Builder == builder)
				{
					treenode.ImageIndex = treenode.SelectedImageIndex = m_mainWnd.ImageListIndex("time");
					builder.RefreshLayer();
				}
			}
		}

		public void RefreshLayersAndOrder()
		{
			// Add in reverse to do render order right
			// Only do layers already loaded and no failed ones
			foreach (LayerBuilderContainer container in this)
			{
				if (container.Builder != null)
				{
					container.Builder.PushBackInRenderOrder();

					if (!container.Builder.Failed)
					{
						foreach (TreeNode node in m_treeList.Nodes)
						{
							if (node.Tag as LayerBuilderContainer == container)
							{
								// --- Also refresh icons ---
								if (container.Builder is DAPQuadLayerBuilder)
								{
									DAPQuadLayerBuilder dapbuilder = (DAPQuadLayerBuilder)container.Builder;

									node.ImageIndex = node.SelectedImageIndex = m_mainWnd.ImageIndex(dapbuilder.DAPType.ToLower());
									if (node.ImageIndex == -1)
										node.ImageIndex = node.SelectedImageIndex = m_mainWnd.ImageListIndex("layer");
								}
								else if (container.Builder is VEQuadLayerBuilder)
									node.ImageIndex = node.SelectedImageIndex = m_mainWnd.ImageListIndex("live");
								else if (container.Builder is GeorefImageLayerBuilder)
									node.ImageIndex = node.SelectedImageIndex = m_mainWnd.ImageListIndex("georef_image");
								else
									node.ImageIndex = node.SelectedImageIndex = m_mainWnd.ImageListIndex("layer");


								break;
							}
						}
					}
				}
			}
		}
	}
}
