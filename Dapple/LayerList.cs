using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Dapple.LayerGeneration;
using System.Drawing.Drawing2D;
using dappleview;

namespace Dapple
{
   public partial class LayerList : UserControl
   {
      #region Delegates

      /// <summary>
      /// Called when this control invokes a 'go to' operation.
      /// </summary>
      public delegate void GoToHandler(LayerBuilder oBuilder);

      /// <summary>
      /// Called when this control selects a layer to tell others to load Metadata for it.
      /// </summary>
      /// <param name="?"></param>
      public delegate void ViewMetadataHandler(LayerBuilder oBuilder);

      /// <summary>
      /// Occurrs when a layer is added or removed to the list.
      /// </summary>
      public delegate void ActiveLayersChangedHandler();

      #endregion

      #region Statics

      /// <summary>
      /// Dummy value for when there is no drop location.
      /// </summary>
      static Point NO_DROP_LOCATION = new Point(-1, -1);

      #endregion

      #region Member Variables

      private int m_iLastTransparency = 255; // Needed because accessing the trackbar's value breaks the message loop

      /// <summary>
      /// The internal list of LayerBuilderContainers.
      /// </summary>
      private List<LayerBuilderContainer> m_oLayers = new List<LayerBuilderContainer>();

      /// <summary>
      /// Where drop data will be dropper.
      /// </summary>
      private Point m_oDropLocation = NO_DROP_LOCATION;

      /// <summary>
      /// Needed because RemoveSelectedDatasets changes the selected index.
      /// </summary>
      private bool m_blSupressSelectedChanged = false;

      public event GoToHandler GoTo;
      public event ViewMetadataHandler ViewMetadata;
      public event ActiveLayersChangedHandler ActiveLayersChanged;

      #endregion

      #region Constructors

      public LayerList() : this(false) { }

      public LayerList(bool blIsOMMode)
      {
         InitializeComponent();
         OMFeaturesEnabled = blIsOMMode;
      }

      #endregion

      #region Properties

      public List<LayerBuilderContainer> SelectedLayers
      {
         get
         {
            List<LayerBuilderContainer> result = new List<LayerBuilderContainer>();
            foreach (int index in cLayerList.SelectedIndices)
            {
               result.Add(m_oLayers[index]);
            }
            return result;
         }
      }

      public List<LayerBuilderContainer> AllLayers
      {
         get
         {
            return m_oLayers;
         }
      }

      public ImageList ImageList
      {
         set
         {
            cLayerList.SmallImageList = value;
            cLayerList.LargeImageList = value;
         }
      }

      public bool OMFeaturesEnabled
      {
         set
         {
            cExtractButton.Visible = value;
         }
      }

      #endregion

      #region Public Members

      public void AddLayers(List<LayerBuilder> oLayers)
      {
         AddLayers(oLayers, 0);
      }

      public void AddLayers(List<LayerBuilder> oLayers, int iInsertIndex)
      {
         for (int count = oLayers.Count - 1; count >= 0; count--)
         {
            if (this.ContainsLayerBuilder(oLayers[count])) oLayers.Remove(oLayers[count]);
         }
         if (oLayers.Count == 0) return;

         cLayerList.BeginUpdate();
         m_blSupressSelectedChanged = true;

         for (int count = 0; count < oLayers.Count; count++)
         {
            AddLayer(oLayers[count], iInsertIndex + count);
         }

         cLayerList.SelectedIndices.Clear();

         for (int count = iInsertIndex; count < iInsertIndex + oLayers.Count; count++)
         {
            cLayerList.Items[count].Selected = true;
         }

         m_blSupressSelectedChanged = false;

         cLayerList_SelectedIndexChanged(this, new EventArgs());
         cLayerList.EndUpdate();
      }

      public void AddLayer(LayerBuilder oLayer)
      {
         AddLayer(oLayer, 0);
      }

      public void AddLayer(LayerBuilder oLayer, int iInsertIndex)
      {
         if (ContainsLayerBuilder(oLayer)) return;

         m_oLayers.Insert(iInsertIndex, new LayerBuilderContainer(oLayer, true));
         cLayerList.Items.Insert(iInsertIndex, oLayer.Name);
         cLayerList.Items[iInsertIndex].Checked = m_oLayers[iInsertIndex].Visible;
         cLayerList.Items[iInsertIndex].ImageIndex = cLayerList.SmallImageList.Images.IndexOfKey(m_oLayers[iInsertIndex].Builder.LayerTypeIconKey);
         cLayerList.Items[iInsertIndex].ForeColor = Color.ForestGreen;

         m_oLayers[iInsertIndex].Builder.SubscribeToBuilderChangedEvent(new BuilderChangedHandler(this.BuilderChanged));

         if (m_oLayers[iInsertIndex].Builder is GeorefImageLayerBuilder)
         {
            if (m_oLayers[iInsertIndex].Builder.GetLayer() == null)
               cLayerList.Items[iInsertIndex].ImageIndex = cLayerList.SmallImageList.Images.IndexOfKey("error");
            else
               m_oLayers[iInsertIndex].Builder.SyncAddLayer(true);

            RefreshLayerRenderOrder();
         }
         else
         {
            m_oLayers[iInsertIndex].Builder.AsyncAddLayer();
         }

         cLayerList.Items[iInsertIndex].Selected = true;

         if (ActiveLayersChanged != null) ActiveLayersChanged();
         CheckIsValid();
      }

      #endregion

      #region Event Handlers

      #region The Control

      private void LayerList_Load(object sender, EventArgs e)
      {
         ResizeColumn();
         SetButtonState();
      }

      #endregion

      #region Transparency Slider

      private void cTransparencySlider_Paint(object sender, PaintEventArgs e)
      {
         // Be careful accessing any properties in here (e.g. Value) as it may cause native messages to be sent in which case the loop may be broken

         Graphics g = e.Graphics;
         int iPos = 8 + m_iLastTransparency * (cTransparencySlider.Bounds.Width - 26) / 255;

         // Attempt to match the toolstrip next to it by drawing with same colors, bottom line and separator
         g.FillRectangle(SystemBrushes.ButtonFace, cTransparencySlider.Bounds.X, cTransparencySlider.Bounds.Y, cTransparencySlider.Bounds.Width, cTransparencySlider.Bounds.Height);
         g.DrawLine(Pens.LightGray, cTransparencySlider.Bounds.X, cTransparencySlider.Bounds.Y + cTransparencySlider.Bounds.Height - 2, cTransparencySlider.Bounds.X + cTransparencySlider.Bounds.Width, cTransparencySlider.Bounds.Y + cTransparencySlider.Bounds.Height - 2);
         g.DrawLine(Pens.White, cTransparencySlider.Bounds.X, cTransparencySlider.Bounds.Y + cTransparencySlider.Bounds.Height - 1, cTransparencySlider.Bounds.X + cTransparencySlider.Bounds.Width, cTransparencySlider.Bounds.Y + cTransparencySlider.Bounds.Height - 1);
         Rectangle rect = new Rectangle(cTransparencySlider.Bounds.X + 3, cTransparencySlider.Bounds.Y + cTransparencySlider.Bounds.Height / 2 - 8, cTransparencySlider.Bounds.Width - 7, 16);
         if (cTransparencySlider.Enabled)
         {
            using (LinearGradientBrush lgb = new LinearGradientBrush(rect, SystemColors.ButtonFace, SystemColors.ControlDarkDark, LinearGradientMode.Horizontal))
               g.FillRectangle(lgb, rect);
            g.DrawRectangle(SystemPens.ActiveCaption, rect);
            g.FillRectangle(SystemBrushes.ControlDarkDark, cTransparencySlider.Bounds.X + 9, cTransparencySlider.Bounds.Y + cTransparencySlider.Bounds.Height / 2 - 1, cTransparencySlider.Bounds.Width - 17, 2);
            g.DrawImage(global::Dapple.Properties.Resources.trackbutton, cTransparencySlider.Bounds.X + iPos, cTransparencySlider.Bounds.Y + 1, global::Dapple.Properties.Resources.trackbutton.Width, cTransparencySlider.Bounds.Height - 3);
         }
         else
         {
            using (LinearGradientBrush lgb = new LinearGradientBrush(rect, SystemColors.ButtonFace, SystemColors.ControlDark, LinearGradientMode.Horizontal))
               g.FillRectangle(lgb, rect);
            g.DrawRectangle(SystemPens.ControlDark, rect);
            g.FillRectangle(SystemBrushes.ControlDark, cTransparencySlider.Bounds.X + 9, cTransparencySlider.Bounds.Y + cTransparencySlider.Bounds.Height / 2 - 1, cTransparencySlider.Bounds.Width - 17, 2);
            g.DrawImage(global::Dapple.Properties.Resources.trackbutton_disable, cTransparencySlider.Bounds.X + iPos, cTransparencySlider.Bounds.Y + 1, global::Dapple.Properties.Resources.trackbutton.Width, cTransparencySlider.Bounds.Height - 3);
         }
         if (e.ClipRectangle.X > 0)
            cTransparencySlider.Invalidate();
      }

      private void cTransparencySlider_ValueChanged(object sender, EventArgs e)
      {
         m_iLastTransparency = cTransparencySlider.Value;
         if (cTransparencySlider.Enabled)
         {
            foreach (LayerBuilderContainer oContainer in this.SelectedLayers)
            {
               oContainer.Opacity = Convert.ToByte(cTransparencySlider.Value);
            }
         }

         // Invalidate to make sure our custom paintjob is fresh
         cTransparencySlider.Invalidate();
      }

      #endregion

      #region Menu Strip

      private void cGoToButton_Click(object sender, EventArgs e)
      {
         CmdGoTo();
      }

      private void cRemoveLayerButton_Click(object sender, EventArgs e)
      {
         CmdRemoveSelectedLayers();
      }

      private void cExtractButton_Click(object sender, EventArgs e)
      {
         Extract.DownloadSettings oDownloadDialog = new Dapple.Extract.DownloadSettings(m_oLayers);
         oDownloadDialog.ShowDialog(this);
      }

      #endregion

      #region Layer List

      private void cLayerList_DragEnter(object sender, DragEventArgs e)
      {
         SetDropLocation(e.X, e.Y);
      }

      private void cLayerList_DragOver(object sender, DragEventArgs e)
      {
         SetDropLocation(e.X, e.Y);

         if (e.Data.GetDataPresent(typeof(List<LayerBuilder>)))
         {
            e.Effect = DragDropEffects.Copy;
         }
         else if (e.Data.GetDataPresent(typeof(ListViewItem)))
         {
            e.Effect = DragDropEffects.Move;
         }
         else
         {
            e.Effect = DragDropEffects.None;
         }
      }

      private void cLayerList_DragLeave(object sender, EventArgs e)
      {
         ClearDropLocation();
      }

      private void cLayerList_DragDrop(object sender, DragEventArgs e)
      {
         ClearDropLocation();

         if (e.Data.GetDataPresent(typeof(List<LayerBuilder>)))
         {
            List<LayerBuilder> oDropData = e.Data.GetData(typeof(List<LayerBuilder>)) as List<LayerBuilder>;
            Point oClientLocation = cLayerList.PointToClient(new Point(e.X, e.Y));
            int iInsertPoint = GetDropIndex(oClientLocation.Y);

            AddLayers(oDropData, iInsertPoint);

            cLayerList.Focus();
         }
         else if (e.Data.GetDataPresent(typeof(ListViewItem)))
         {
            ListViewItem oDropData = e.Data.GetData(typeof(ListViewItem)) as ListViewItem;
            Point oClientLocation = cLayerList.PointToClient(new Point(e.X, e.Y));
            int iInsertPoint = GetDropIndex(oClientLocation.Y);

            ShuffleLayer(oDropData.Index, iInsertPoint);

            cLayerList.Focus();
         }
      }

      private void cLayerList_Resize(object sender, EventArgs e)
      {
         ResizeColumn();
      }

      private void cLayerList_SelectedIndexChanged(object sender, EventArgs e)
      {
         if (!m_blSupressSelectedChanged)
         {
            SetButtonState();
            CmdViewMetadata();
         }
      }

      private void cLayerList_ItemCheck(object sender, ItemCheckEventArgs e)
      {
         m_oLayers[e.Index].Visible = e.NewValue == CheckState.Checked;
      }

      private void cLayerList_MouseMove(object sender, MouseEventArgs e)
      {
         if ((e.Button & MouseButtons.Left) == MouseButtons.Left && cLayerList.SelectedIndices.Count == 1)
         {
            DoDragDrop(cLayerList.SelectedItems[0], DragDropEffects.Move);
         }
      }

      private void cLayerList_KeyUp(object sender, KeyEventArgs e)
      {
         if (e.KeyCode == Keys.Delete)
         {
            CmdRemoveSelectedLayers();
         }
      }

      #endregion

      #region Context Menu

      private void cLayerListContextMenu_Opening(object sender, CancelEventArgs e)
      {
         if (cLayerList.SelectedIndices.Count == 0)
         {
            e.Cancel = true;
            return;
         }

         cGoToToolStripMenuItem.Enabled = cLayerList.SelectedIndices.Count == 1;
         cViewPropertiesToolStripMenuItem.Enabled = cLayerList.SelectedIndices.Count == 1;
      }

      private void cGoToToolStripMenuItem_Click(object sender, EventArgs e)
      {
         CmdGoTo();
      }

      private void cViewPropertiesToolStripMenuItem_Click(object sender, EventArgs e)
      {
         LayerBuilder oBuilder = m_oLayers[cLayerList.SelectedIndices[0]].Builder;

         if (oBuilder == null) return;

         frmProperties form = new frmProperties();
         form.SetObject = oBuilder;
         form.ShowDialog(this);

         if (oBuilder.IsChanged)
         {
            CmdRefreshLayer(cLayerList.SelectedIndices[0]);
            SetButtonState();
         }
      }

      private void cRemoveToolStripMenuItem_Click(object sender, EventArgs e)
      {
         CmdRemoveSelectedLayers();
      }

      private void cRefreshToolStripMenuItem_Click(object sender, EventArgs e)
      {
         foreach (int iIndex in cLayerList.SelectedIndices)
         {
            CmdRefreshLayer(iIndex);
         }
      }

      private void cClearCacheToolStripMenuItem_Click(object sender, EventArgs e)
      {
         foreach (int iIndex in cLayerList.SelectedIndices)
         {
            CmdClearLayerCache(iIndex);
         }
      }

      #endregion

      #endregion

      #region Helper Methods

      #region UI Modification

      /// <summary>
      /// Sets the width of the (invisible) layer list column equal to the width of the layer list.
      /// </summary>
      private void ResizeColumn()
      {
         cLayerList.Columns[0].Width = cLayerList.ClientSize.Width;
      }

      /// <summary>
      /// Set the enabled and value settings of the widgets in this control.
      /// </summary>
      private void SetButtonState()
      {
         cTransparencySlider.Enabled = cLayerList.SelectedIndices.Count == 1;
         if (cLayerList.SelectedIndices.Count == 1)
         {
            cTransparencySlider.Value = m_oLayers[cLayerList.SelectedIndices[0]].Opacity;
         }

         cGoToButton.Enabled = cLayerList.SelectedIndices.Count == 1;
         cRemoveLayerButton.Enabled = cLayerList.SelectedIndices.Count > 0;
         cExtractButton.Enabled = cLayerList.SelectedIndices.Count > 0;
      }

      /// <summary>
      /// Event handler for a builder change event.
      /// </summary>
      /// <param name="oBuilder"></param>
      /// <param name="eChangeType"></param>
      private void BuilderChanged(LayerBuilder oBuilder, BuilderChangeType eChangeType)
      {
         if (InvokeRequired)
         {
            Invoke(new BuilderChangedHandler(BuilderChanged), new Object[] { oBuilder, eChangeType });
            return;
         }
         else
         {
            // --- Get the updated builder's index ---
            int iBuilderIndex = 0;
            do
            {
               if (m_oLayers[iBuilderIndex].Builder == oBuilder) break;
               iBuilderIndex++;
            } while (iBuilderIndex < m_oLayers.Count);
            if (iBuilderIndex == m_oLayers.Count) return;

            if (eChangeType == BuilderChangeType.LoadedASync)
            {
               // refresh layers and order
            }
            else if (eChangeType == BuilderChangeType.LoadedASyncFailed || eChangeType == BuilderChangeType.LoadedSyncFailed)
            {
               cLayerList.Items[iBuilderIndex].ImageIndex = cLayerList.SmallImageList.Images.IndexOfKey("error");
            }
            else if (eChangeType == BuilderChangeType.VisibilityChanged)
            {
               cLayerList.Items[iBuilderIndex].Checked = m_oLayers[iBuilderIndex].Visible;
            }
         }
      }

      /// <summary>
      /// Refresh the order of the layers on the globe.
      /// </summary>
      private void RefreshLayerRenderOrder()
      {
         foreach (LayerBuilderContainer oLayer in m_oLayers)
         {
            oLayer.Builder.PushBackInRenderOrder();
         }
      }

      #endregion

      #region Drag and Drop

      /// <summary>
      /// Sets the point the mouse is over.
      /// </summary>
      /// <param name="iX"></param>
      /// <param name="iY"></param>
      private void SetDropLocation(int iX, int iY)
      {
         m_oDropLocation = new Point(iX, iY);
         cLayerList.Refresh();
         DrawDropHint();
      }

      /// <summary>
      /// Clears the point the mouse is over.
      /// </summary>
      private void ClearDropLocation()
      {
         m_oDropLocation = NO_DROP_LOCATION;
         cLayerList.Refresh();
      }

      /// <summary>
      /// Gets the index of the in-between-layers point that a drop operation will drop to.
      /// </summary>
      /// <param name="iClientY"></param>
      /// <returns></returns>
      private int GetDropIndex(int iClientY)
      {
         int result = cLayerList.Items.IndexOf(cLayerList.GetItemAt(35, iClientY));
         if (result == -1) return cLayerList.Items.Count;
         Rectangle oItemBounds = cLayerList.GetItemRect(result);
         // --- Offset to the next number if we're below halfway ---
         if (iClientY - oItemBounds.Top > oItemBounds.Height / 2) result++;
         return result;
      }

      /// <summary>
      /// Draw a line on the layer list where dropped data will be insterted.
      /// </summary>
      private void DrawDropHint()
      {
         if (m_oDropLocation == NO_DROP_LOCATION) return;
         Point oClientDropLocation = cLayerList.PointToClient(m_oDropLocation);
         Graphics oGraphics = cLayerList.CreateGraphics();
         if (!oGraphics.VisibleClipBounds.Contains(oClientDropLocation)) return;
         
         int iLineWidth = (int)oGraphics.VisibleClipBounds.Width;
         int iLineY = 0;

         if (cLayerList.Items.Count != 0)
         {
            int iInsertPoint = GetDropIndex(oClientDropLocation.Y);

            if (iInsertPoint == cLayerList.Items.Count)
            {
               iLineY = cLayerList.GetItemRect(iInsertPoint - 1).Bottom;
            }
            else
            {
               iLineY = cLayerList.GetItemRect(iInsertPoint).Top;
            }
         }
         oGraphics.DrawLine(new System.Drawing.Pen(System.Drawing.Brushes.Black, 3.0f), new System.Drawing.Point(0, iLineY), new System.Drawing.Point(iLineWidth, iLineY));
      }

      /// <summary>
      /// Move a layer up or down in the list.
      /// </summary>
      /// <param name="iOldIndex"></param>
      /// <param name="iNewIndex"></param>
      private void ShuffleLayer(int iOldIndex, int iNewIndex)
      {
         if (iOldIndex == iNewIndex || iOldIndex == iNewIndex - 1) return;
         int iUpShim = iNewIndex < iOldIndex ? 1 : 0;
         int iDownShim = iNewIndex > iOldIndex ? 1 : 0;

         // --- Update the internal list ---

         m_oLayers.Insert(iNewIndex, m_oLayers[iOldIndex]);
         m_oLayers.RemoveAt(iOldIndex + iUpShim);

         // --- Update the UI list ---

         ListViewItem oItemToMove = cLayerList.Items[iOldIndex];
         cLayerList.Items.RemoveAt(iOldIndex);
         cLayerList.Items.Insert(System.Math.Max(iNewIndex - iDownShim, 0), oItemToMove);

         // --- Update the globe ---

         RefreshLayerRenderOrder();
         CheckIsValid();
      }

      #endregion

      #region UI Commands

      /// <summary>
      /// Clear a layer's cache.
      /// </summary>
      /// <param name="iIndex"></param>
      private void CmdClearLayerCache(int iIndex)
      {
         if (m_oLayers[cLayerList.SelectedIndices[0]].Builder != null)
         {
            string strCache = m_oLayers[iIndex].Builder.GetCachePath();
            if (!string.IsNullOrEmpty(strCache))
            {
               Utility.FileSystem.DeleteFolderGUI(this, strCache);
               CmdRefreshLayer(iIndex);
            }
         }
      }

      /// <summary>
      /// Refresh a layer.
      /// </summary>
      /// <param name="iIndex"></param>
      private void CmdRefreshLayer(int iIndex)
      {
         cLayerList.Items[iIndex].ImageIndex = cLayerList.SmallImageList.Images.IndexOfKey("time");
         m_oLayers[iIndex].Builder.RefreshLayer();
      }

      /// <summary>
      /// Go to selected layer.
      /// </summary>
      private void CmdGoTo()
      {
         if (cLayerList.SelectedIndices.Count == 1 && GoTo != null && m_oLayers[cLayerList.SelectedIndices[0]].Builder != null)
         {
            GoTo(m_oLayers[cLayerList.SelectedIndices[0]].Builder);
         }
      }

      /// <summary>
      /// Display the metadata for the selected layer.
      /// </summary>
      private void CmdViewMetadata()
      {
         if (cLayerList.SelectedIndices.Count == 1 && ViewMetadata != null && m_oLayers[cLayerList.SelectedIndices[0]].Builder != null)
         {
            ViewMetadata(m_oLayers[cLayerList.SelectedIndices[0]].Builder);
         }
      }

      public void CmdRemoveSelectedLayers()
      {
         if (cLayerList.SelectedIndices.Count == 0) return;

         cLayerList.BeginUpdate();
         m_blSupressSelectedChanged = true;
         while (cLayerList.SelectedIndices.Count > 0)
         {
            int iIndexToDelete = cLayerList.SelectedIndices[0];

            if (m_oLayers[iIndexToDelete].Builder != null)
            {
               m_oLayers[iIndexToDelete].Builder.UnsubscribeToBuilderChangedEvent(new BuilderChangedHandler(this.BuilderChanged));
               m_oLayers[iIndexToDelete].Builder.RemoveLayer();
            }

            m_oLayers.RemoveAt(iIndexToDelete);
            cLayerList.Items.RemoveAt(iIndexToDelete);
         }
         m_blSupressSelectedChanged = false;
         cLayerList.EndUpdate();

         RefreshLayerRenderOrder();
         SetButtonState();

         if (ActiveLayersChanged != null) ActiveLayersChanged();
         CheckIsValid();
      }

      public void CmdRemoveAllLayers()
      {
         cLayerList.Items.Clear();
         m_oLayers.Clear();

         SetButtonState();

         if (ActiveLayersChanged != null) ActiveLayersChanged();
         CheckIsValid();
      }

      public void CmdRemoveGeoTiff(String szFilename)
      {
         cLayerList.BeginUpdate();
         m_blSupressSelectedChanged = true;

         int iIndex = m_oLayers.Count - 1;
         while (iIndex >= 0)
         {
            if (m_oLayers[iIndex].Builder != null && m_oLayers[iIndex].Builder is GeorefImageLayerBuilder &&
               ((GeorefImageLayerBuilder)m_oLayers[iIndex].Builder).FileName.Equals(szFilename))
            {
               m_oLayers[iIndex].Builder.UnsubscribeToBuilderChangedEvent(new BuilderChangedHandler(this.BuilderChanged));
               m_oLayers[iIndex].Builder.RemoveLayer();

               m_oLayers.RemoveAt(iIndex);
               cLayerList.Items.RemoveAt(iIndex);
            }
         }

         m_blSupressSelectedChanged = false;
         cLayerList.EndUpdate();

         RefreshLayerRenderOrder();
         SetButtonState();

         if (ActiveLayersChanged != null) ActiveLayersChanged();
         CheckIsValid();
      }

      public bool LoadFromView(DappleView view, ServerTree oTree)
      {
         CmdRemoveAllLayers();

         bool blIncompleteLoad = false;

         cLayerList.BeginUpdate();
         m_blSupressSelectedChanged = true;
         for (int i = 0; i < view.View.activelayers.datasetCount; i++)
         {
            datasetType dataset = view.View.activelayers.GetdatasetAt(i);

            LayerUri oUri = LayerUri.create(dataset.uri.Value);
            if (!oUri.IsValid)
            {
               blIncompleteLoad = true;
               continue;
            }

            LayerBuilder oBuilder = oUri.getBuilder(MainForm.WorldWindowSingleton, oTree);
            oBuilder.Visible = dataset.Hasinvisible() ? !dataset.invisible.Value : true;
            oBuilder.Opacity = (byte)dataset.opacity.Value;
            AddLayer(oBuilder);
         }
         m_blSupressSelectedChanged = false;
         cLayerList.EndUpdate();

         return blIncompleteLoad;
      }

      #endregion

      /// <summary>
      /// Check if a LayerBuilder is already in the layer list.
      /// </summary>
      /// <param name="oBuilder"></param>
      /// <returns></returns>
      public bool ContainsLayerBuilder(LayerBuilder oBuilder)
      {
         foreach (LayerBuilderContainer oContainer in m_oLayers)
         {
            if (oContainer.Builder.Equals(oBuilder)) return true;
         }
         return false;
      }

      #region Testing

      /// <summary>
      /// Raise an exception if the layers in the internal layer list don't match up with the UI list.
      /// </summary>
      private void CheckIsValid()
      {
         if (m_oLayers.Count != cLayerList.Items.Count)
            throw new ArgumentException("Data no longer syncs");

         for (int count = 0; count < m_oLayers.Count; count++)
         {
            if (!m_oLayers[count].Name.Equals(cLayerList.Items[count].Text))
               throw new ArgumentException("Data no longer syncs");
         }
      }

      #endregion

      #endregion
   }
}
