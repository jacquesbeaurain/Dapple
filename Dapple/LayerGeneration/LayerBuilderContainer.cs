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
      ServerTree m_oServerTree;

		public LayerBuilderList(MainForm mainWnd, TriStateTreeView tree, WorldWindow worldWindow)
		{
			m_mainWnd = mainWnd;
         m_oServerTree = null;
			m_treeList = tree;
			m_worldWindow = worldWindow;
		}

		public bool ContainsSource(LayerBuilder builder)
		{
			foreach (LayerBuilderContainer container in this)
			{
				if (container.SourceBuilder.Equals(builder))
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

      /// <summary>
      /// Move an LBC to a new position in the list.
      /// </summary>
      /// <param name="iOldIndex">The old position in the list.</param>
      /// <param name="iNewIndex">The new position in the list.</param>
      public void Reorder(int iOldIndex, int iNewIndex)
      {
         if (iOldIndex < 0 || iOldIndex > this.Count) throw new ArgumentOutOfRangeException("iOldIndex");
         if (iNewIndex < 0 || iNewIndex > this.Count) throw new ArgumentOutOfRangeException("iNewIndex");
         if (iOldIndex == iNewIndex) return;

         int iInsertShim = iNewIndex < iOldIndex ? 1 : 0;

         // --- Reorder the layer in the layer list ---

         LayerBuilderContainer oLBCToMove = this[iOldIndex];

         this.Insert(iNewIndex, oLBCToMove);
         this.RemoveAt(iOldIndex + iInsertShim);

         // --- Reorder the layer in the view ---

         m_treeList.BeginUpdate();
         
         TreeNode oTNToMove = m_treeList.Nodes[iOldIndex];
         
         m_treeList.Nodes.Insert(iNewIndex, oTNToMove.Text);
         m_treeList.Nodes[iNewIndex].Tag = oTNToMove.Tag;
         m_treeList.SetState(m_treeList.Nodes[iNewIndex], oLBCToMove.Visible ? TriStateTreeView.CheckBoxState.Checked : TriStateTreeView.CheckBoxState.Unchecked);
         m_treeList.SelectedNode = m_treeList.Nodes[iNewIndex];
         m_treeList.Nodes.RemoveAt(iOldIndex + iInsertShim);

         m_treeList.EndUpdate();

         // --- Reorder the layer on the globe ---

         RefreshLayersAndOrder();
      }

      public void AddMultiple(int iNewIndex, List<LayerBuilder> oLayers)
      {
         if (iNewIndex < 0 || iNewIndex > this.Count) throw new ArgumentOutOfRangeException("iNewIndex");
         if (oLayers == null) throw new ArgumentNullException("oLayers");
         if (oLayers.Count == 0) return;

         // --- Add the layers to the list ---

         for (int count = 0; count < oLayers.Count; count++)
         {
            this.Insert(iNewIndex + count, new LayerBuilderContainer(oLayers[count], true));
         }

         // --- Add the layers to the tree ---

         m_treeList.BeginUpdate();

         for (int count = 0; count < oLayers.Count; count++)
         {
            m_treeList.Nodes.Insert(iNewIndex + count, this[iNewIndex + count].Name);
            m_treeList.Nodes[iNewIndex + count].Tag = this[iNewIndex + count];
            m_treeList.SetState(m_treeList.Nodes[iNewIndex + count], this[iNewIndex + count].Visible ? TriStateTreeView.CheckBoxState.Checked : TriStateTreeView.CheckBoxState.Unchecked);
         }

         m_treeList.EndUpdate();

         for (int count = 0; count < oLayers.Count; count++)
         {
            this[count].Builder.SubscribeToBuilderChangedEvent(new BuilderChangedHandler(this.BuilderChanged));

            if (this[count].Builder is GeorefImageLayerBuilder)
            {
               if (this[count].Builder.GetLayer() == null)
                  m_treeList.Nodes[iNewIndex + count].ImageIndex = m_treeList.Nodes[iNewIndex + count].SelectedImageIndex = m_mainWnd.ImageListIndex("error");
               else
                  this[count].Builder.SyncAddLayer(true);
               m_treeList.Nodes[iNewIndex + count].ToolTipText = (this[count].Builder as GeorefImageLayerBuilder).FileName;
               RefreshLayersAndOrder();
            }
            else
               this[count].Builder.AsyncAddLayer();
         }

         // --- Reorder the layers on the globe ---

         RefreshLayersAndOrder();
      }

      public void AddUsingUri(string strName, string strUri, bool visible, byte opacity, bool front, ServerTree serverTree, ref bool bOldView)
      {
         string strError = string.Empty;
         LayerBuilder builder = null;

         try
         {
            LayerUri oUri = LayerUri.create(strUri);
            if (!oUri.IsValid)
               throw new Exception("Invalid layer URI format");

            builder = oUri.getBuilder( m_mainWnd, serverTree);
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
                  treeNode = m_treeList.AddTop(null, strName + "[" + strError + "]", iImage, iImage, container.Visible ? TriStateTreeView.CheckBoxState.Checked : TriStateTreeView.CheckBoxState.Unchecked);
               }
               else
               {
                  Add(container);
                  treeNode = m_treeList.Add(null, strName + "[" + strError + "]", iImage, iImage, container.Visible ? TriStateTreeView.CheckBoxState.Checked : TriStateTreeView.CheckBoxState.Unchecked);
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

      public ServerTree ServerTree
      {
         set { m_oServerTree = value; }
      }

      public void HandleDragOver(Object oSender, DragEventArgs oArgs)
      {
         if (oArgs.Data.GetDataPresent(typeof(List<LayerBuilder>)))
         {
            oArgs.Effect = DragDropEffects.Copy;
         }
         else if (oArgs.Data.GetDataPresent(typeof(TreeNode)))
         {
            oArgs.Effect = DragDropEffects.Move;
         }
         else
         {
            oArgs.Effect = DragDropEffects.None;
         }
      }

      public void HandleDragDrop(Object oSender, DragEventArgs oArgs)
      {
         if (oArgs.Data.GetDataPresent(typeof(List<LayerBuilder>)))
         {
            // --- Figure out where in the tree the user dropped the layer ---

            int newIndex;
            TreeNode oDropTarget = this.m_treeList.GetNodeAt(m_treeList.PointToClient(new System.Drawing.Point(oArgs.X, oArgs.Y)));
            if (oDropTarget == null)
            {
               newIndex = this.Count;
            }
            else
            {
               newIndex = this.IndexOf(oDropTarget.Tag as LayerBuilderContainer);
            }

            // --- Get the layer list, remove layers already in the tree ---

            List<LayerBuilder> oLayers = oArgs.Data.GetData(typeof(List<LayerBuilder>)) as List<LayerBuilder>;
            for (int count = oLayers.Count - 1; count >= 0; count--)
            {
               if (this.ContainsSource(oLayers[count])) oLayers.RemoveAt(count);
            }

            AddMultiple(newIndex, oLayers);

            /*int iOffset = 0;
            m_treeList.BeginUpdate();
            foreach (LayerBuilder oLayer in oLayers)
            {
               // --- Check that the layer isn't already present ---

               if (this.ContainsSource(oLayer)) return;

               // --- Add to top or bottom, depending on which requires more moves afterwards to position ---
               if (this.Count == 0)
               {
                  this.Add(oLayer.Name, oLayer, false, 255, true);
               }
               else if (newIndex < Count - newIndex)
               {
                  this.Add(oLayer.Name, oLayer, true, 255, true);
                  Reorder(0, newIndex + iOffset);
               }
               else
               {
                  this.Add(oLayer.Name, oLayer, false, 255, true);
                  Reorder(this.Count - 1, newIndex + iOffset);
               }
               iOffset++;
            }
            m_treeList.EndUpdate();*/
         }

         if (oArgs.Data.GetDataPresent(typeof(TreeNode)))
         {
            TreeNode oNodeToMove = oArgs.Data.GetData(typeof(TreeNode)) as TreeNode;
            if (!this.Contains(oNodeToMove.Tag as LayerBuilderContainer)) return;

            int oldIndex = this.IndexOf(oNodeToMove.Tag as LayerBuilderContainer);

            int newIndex;
            TreeNode oDropTarget = this.m_treeList.GetNodeAt(m_treeList.PointToClient(new System.Drawing.Point(oArgs.X, oArgs.Y)));
            if (oDropTarget == null)
            {
               newIndex = this.Count;
            }
            else
            {
               newIndex = this.IndexOf(oDropTarget.Tag as LayerBuilderContainer);
            }

            Reorder(oldIndex, newIndex);
         }
      }
	}
}
