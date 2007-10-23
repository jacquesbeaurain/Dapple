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
using WorldWind.Renderable;
using WorldWind;
using System.IO;
using System.Drawing.Imaging;
using WorldWind.PluginEngine;
using System.Diagnostics;
using Geosoft.GX.DAPGetData;

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
      private List<LayerBuilder> m_oLayers = new List<LayerBuilder>();

      /// <summary>
      /// Where drop data will be dropper.
      /// </summary>
      private Point m_oDropLocation = NO_DROP_LOCATION;

      /// <summary>
      /// Needed because RemoveSelectedDatasets changes the selected index.
      /// </summary>
      private bool m_blSupressSelectedChanged = false;

      private ServerTree m_hServerTree = null;
      private LayerBuilder m_hBaseLayer = null;

      private TransparencyDriver m_oTransparencyDriver = new TransparencyDriver();

      public event GoToHandler GoTo;
      public event Dapple.MainForm.ViewMetadataHandler ViewMetadata;
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

      public List<LayerBuilder> SelectedLayers
      {
         get
         {
            List<LayerBuilder> result = new List<LayerBuilder>();
            foreach (int index in cLayerList.SelectedIndices)
            {
               result.Add(m_oLayers[index]);
            }
            return result;
         }
      }

      public List<LayerBuilder> AllLayers
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

      /// <summary>
      /// Whether a delete operation should be allowed to commence (disallows deletion of the base layer).
      /// </summary>
      public bool RemoveAllowed
      {
         get
         {
            return cLayerList.SelectedIndices.Count > 1 || (cLayerList.SelectedIndices.Count == 1 && !m_oLayers[cLayerList.SelectedIndices[0]].Equals(m_hBaseLayer));
         }
      }

      public bool DownloadsInProgress
      {
         get
         {
            int temp, temp2;

            foreach (LayerBuilder oBuilder in this.SelectedLayers)
            {
               if (oBuilder.Visible && oBuilder.bIsDownloading(out temp, out temp2))
                  return true;
            }
            return false;
         }
      }

      public ServerTree ServerTree
      {
         set
         {
            m_hServerTree = value;
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
            if (m_oLayers.Contains(oLayers[count])) oLayers.Remove(oLayers[count]);
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

      public void AddLayers(List<LayerUri> oUris)
      {
         AddLayers(oUris, 0);
      }

      public void AddLayers(List<LayerUri> oUris, int iInsertIndex)
      {
         this.AddLayers(CreateLayerBuilders(oUris), iInsertIndex);
      }

      public void SetBaseLayer(LayerBuilder oBaseLayer)
      {
         // TODO: remove an existing base layer, if necessary.
         m_hBaseLayer = oBaseLayer;
         oBaseLayer.Temporary = true;
         AddLayer(oBaseLayer, 0);
      }

      public void AddLayer(LayerBuilder oLayer)
      {
         AddLayer(oLayer, 0);
      }

      private void AddLayer(LayerBuilder oNewBuilder, int iInsertIndex)
      {
         if (m_oLayers.Contains(oNewBuilder)) return;

         oNewBuilder.Reset();

         m_oLayers.Insert(iInsertIndex, oNewBuilder);
         cLayerList.Items.Insert(iInsertIndex, oNewBuilder.Name);
         cLayerList.Items[iInsertIndex].Checked = m_oLayers[iInsertIndex].Visible;
         if (oNewBuilder.Equals(m_hBaseLayer))
         {
            cLayerList.Items[iInsertIndex].ImageIndex = cLayerList.SmallImageList.Images.IndexOfKey("blue_marble");
            cLayerList.Items[iInsertIndex].ForeColor = Color.Black;
            cLayerList.Items[iInsertIndex].Font = new Font(cLayerList.Items[iInsertIndex].Font, FontStyle.Bold | FontStyle.Underline);
         }
         else
         {
            cLayerList.Items[iInsertIndex].ImageIndex = cLayerList.SmallImageList.Images.IndexOfKey(m_oLayers[iInsertIndex].DisplayIconKey);
            cLayerList.Items[iInsertIndex].ForeColor = Color.ForestGreen;
         }

         m_oLayers[iInsertIndex].SubscribeToBuilderChangedEvent(new BuilderChangedHandler(this.BuilderChanged));

         if (m_oLayers[iInsertIndex] is GeorefImageLayerBuilder)
         {
            if (m_oLayers[iInsertIndex].GetLayer() == null)
               cLayerList.Items[iInsertIndex].ImageIndex = cLayerList.SmallImageList.Images.IndexOfKey("error");
            else
               m_oLayers[iInsertIndex].SyncAddLayer(true);

            RefreshLayerRenderOrder();
         }
         else
         {
            if (oNewBuilder != m_hBaseLayer)
               m_oLayers[iInsertIndex].AsyncAddLayer();
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
            if (cTransparencySlider.Focused)
            {
               g.DrawRectangle(SystemPens.ActiveCaption, rect);
            }
            else
            {
               g.DrawRectangle(SystemPens.InactiveCaption, rect);
            }
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
            m_oTransparencyDriver.OpacityChanged((byte)cTransparencySlider.Value);
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
         CmdDownloadActiveLayers();
      }

      private void cExportButton_Click(object sender, EventArgs e)
      {
         CmdExportSelected();
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
         else if (e.Data.GetDataPresent(typeof(LayerListInternalShuffleToken)))
         {
            e.Effect = DragDropEffects.Move;
         }
         else if (e.Data.GetDataPresent(typeof(List<LayerUri>)))
         {
            e.Effect = DragDropEffects.Copy;
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
         else if (e.Data.GetDataPresent(typeof(LayerListInternalShuffleToken)))
         {
            Point oClientLocation = cLayerList.PointToClient(new Point(e.X, e.Y));
            int iInsertPoint = GetDropIndex(oClientLocation.Y);

            ShuffleSelectedLayers(iInsertPoint);

            cLayerList.Focus();
         }
         else if (e.Data.GetDataPresent(typeof(List<LayerUri>)))
         {
            List<LayerUri> oDropData = e.Data.GetData(typeof(List<LayerUri>)) as List<LayerUri>;
            Point oClientLocation = cLayerList.PointToClient(new Point(e.X, e.Y));
            int iInsertPoint = GetDropIndex(oClientLocation.Y);

            List<LayerBuilder> oListToAdd = CreateLayerBuilders(oDropData);

            AddLayers(oListToAdd, iInsertPoint);

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
         if ((e.Button & MouseButtons.Left) == MouseButtons.Left && cLayerList.SelectedIndices.Count > 0)
         {
            DoDragDrop(new LayerListInternalShuffleToken(), DragDropEffects.Move);
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

         cRemoveToolStripMenuItem.Enabled = this.RemoveAllowed;
         cGoToToolStripMenuItem.Enabled = cLayerList.SelectedIndices.Count == 1;
         cViewPropertiesToolStripMenuItem.Enabled = cLayerList.SelectedIndices.Count == 1;
         cViewLegendToolStripMenuItem.Enabled = cLayerList.SelectedIndices.Count == 1 && m_oLayers[cLayerList.SelectedIndices[0]].SupportsLegend;
      }

      private void cGoToToolStripMenuItem_Click(object sender, EventArgs e)
      {
         CmdGoTo();
      }

      private void cViewPropertiesToolStripMenuItem_Click(object sender, EventArgs e)
      {
         LayerBuilder oBuilder = m_oLayers[cLayerList.SelectedIndices[0]];
         if (oBuilder == null) return;

         frmProperties.DisplayForm(oBuilder);

         if (oBuilder.IsChanged)
         {
            CmdRefreshLayer(cLayerList.SelectedIndices[0]);
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

      private void cViewLegendToolStripMenuItem_Click(object sender, EventArgs e)
      {
         string[] aLegends = m_oLayers[cLayerList.SelectedIndices[0]].GetLegendURLs();
         foreach (string szLegend in aLegends)
         {
            if (!String.IsNullOrEmpty(szLegend)) MainForm.BrowseTo(szLegend);
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
         cTransparencySlider.Enabled = cLayerList.SelectedIndices.Count > 0;

         if (cTransparencySlider.Enabled)
         {
            List<LayerBuilder> oSelectedLayers = new List<LayerBuilder>();
            foreach (int iIndex in cLayerList.SelectedIndices)
            {
               oSelectedLayers.Add(m_oLayers[iIndex]);
            }

            m_oTransparencyDriver.SetBuilders(oSelectedLayers);
            cTransparencySlider.Value = m_oTransparencyDriver.ReferenceValue;
         }

         cGoToButton.Enabled = cLayerList.SelectedIndices.Count == 1;
         cRemoveLayerButton.Enabled = this.RemoveAllowed;
         cExtractButton.Enabled = cLayerList.SelectedIndices.Count > 0;
         cExportButton.Enabled = cLayerList.SelectedIndices.Count > 0;
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
               if (m_oLayers[iBuilderIndex] == oBuilder) break;
               iBuilderIndex++;
            } while (iBuilderIndex < m_oLayers.Count);
            if (iBuilderIndex == m_oLayers.Count) return;

            if (eChangeType == BuilderChangeType.LoadedASync)
            {
               RefreshLayerRenderOrder();
               cLayerList.Items[iBuilderIndex].ImageIndex = cLayerList.SmallImageList.Images.IndexOfKey(oBuilder.DisplayIconKey);
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
         foreach (LayerBuilder oLayer in m_oLayers)
         {
            oLayer.PushBackInRenderOrder();
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
      /// Reorder the selected layers to be continuous starting at the drop index.
      /// </summary>
      /// <remarks>
      /// The drop index is a value ranging from 0 to the number of layers in the list.  It represents the positions between layers (so drop index 2 is above layer 2, for example).
      /// A drop index equal to the number of layers in the list represents dropping the layers at the end of the list.
      /// </remarks>
      private void ShuffleSelectedLayers(int iDropIndex)
      {
         if (cLayerList.SelectedIndices.Count < 1) throw new InvalidOperationException("No layers are selected");

         // --- Check if the selection is continuous ---

         bool blContinuousSelection = cLayerList.SelectedIndices[cLayerList.SelectedIndices.Count - 1] - cLayerList.SelectedIndices[0] + 1 == cLayerList.SelectedIndices.Count;
         
         // --- Don't do anything if the selection is continuous and the drop index selected wouldn't move the items ---

         if (blContinuousSelection)
         {
            if (iDropIndex >= cLayerList.SelectedIndices[0] && iDropIndex <= cLayerList.SelectedIndices[cLayerList.SelectedIndices.Count - 1] + 1)
            {
               return;
            }
         }

         // --- If the drop index is below a selected layer, decrement it, as removing the layer from the list before the insert changes the indices ---

         int iDecrement = 0;
         foreach (int iSelectedIndex in cLayerList.SelectedIndices)
         {
            if (iDropIndex > iSelectedIndex) iDecrement++; 
         }
         iDropIndex -= iDecrement;
         
         // --- Store items to move in temporary buffers ---

         List<LayerBuilder> oMovedContainers = new List<LayerBuilder>();
         List<ListViewItem> oMovedItems = new List<ListViewItem>();

         foreach (int iSelectedIndex in cLayerList.SelectedIndices)
         {
            oMovedContainers.Add(m_oLayers[iSelectedIndex]);
            oMovedItems.Add(cLayerList.Items[iSelectedIndex]);
         }

         // --- Remove the selected layers ---

         cLayerList.SuspendLayout();
         m_blSupressSelectedChanged = true;

         while (cLayerList.SelectedIndices.Count > 0)
         {
            m_oLayers.RemoveAt(cLayerList.SelectedIndices[0]);
            cLayerList.Items.RemoveAt(cLayerList.SelectedIndices[0]);
         }

         // --- Reinsert and reselect the selected layers ---

         for (int count = 0; count < oMovedContainers.Count; count++)
         {
            m_oLayers.Insert(count + iDropIndex, oMovedContainers[count]);
            cLayerList.Items.Insert(count + iDropIndex, oMovedItems[count]);
            cLayerList.SelectedIndices.Add(count + iDropIndex);
         }

         m_blSupressSelectedChanged = false;
         cLayerList.ResumeLayout();

         // --- Update the globe ---

         RefreshLayerRenderOrder();

         CheckIsValid();
      }

      /// <summary>
      /// Turns a list of LayerUris into a list of LayerBuilders.
      /// </summary>
      /// <param name="oUris"></param>
      /// <returns></returns>
      private List<LayerBuilder> CreateLayerBuilders(List<LayerUri> oUris)
      {
         List<LayerBuilder> result = new List<LayerBuilder>();

         foreach (LayerUri oUri in oUris)
         {
            result.Add(oUri.getBuilder(MainForm.WorldWindowSingleton, m_hServerTree));
         }

         return result;
      }

      #endregion

      #region UI Commands

      /// <summary>
      /// Clear a layer's cache.
      /// </summary>
      /// <param name="iIndex"></param>
      private void CmdClearLayerCache(int iIndex)
      {
         if (m_oLayers[cLayerList.SelectedIndices[0]] != null)
         {
            string strCache = m_oLayers[iIndex].GetCachePath();
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
         m_oLayers[iIndex].RefreshLayer();
      }

      /// <summary>
      /// Go to selected layer.
      /// </summary>
      private void CmdGoTo()
      {
         if (cLayerList.SelectedIndices.Count == 1 && GoTo != null && m_oLayers[cLayerList.SelectedIndices[0]] != null)
         {
            GoTo(m_oLayers[cLayerList.SelectedIndices[0]]);
         }
      }

      /// <summary>
      /// Display the metadata for the selected layer.
      /// </summary>
      private void CmdViewMetadata()
      {
         if (cLayerList.SelectedIndices.Count == 1 && ViewMetadata != null && m_oLayers[cLayerList.SelectedIndices[0]] != null)
         {
            ViewMetadata(m_oLayers[cLayerList.SelectedIndices[0]]);
         }
      }

      /// <summary>
      /// Remove selected layers in the layer list.
      /// </summary>
      public void CmdRemoveSelectedLayers()
      {
         if (cLayerList.SelectedIndices.Count == 0) return;

         cLayerList.BeginUpdate();
         m_blSupressSelectedChanged = true;

         foreach (int iIndex in cLayerList.SelectedIndices)
         {
            if (m_oLayers[iIndex].Equals(m_hBaseLayer))
            {
               cLayerList.SelectedIndices.Remove(iIndex);
               break;
            }
         }

         int iLastIndex = 0;
         while (cLayerList.SelectedIndices.Count > 0)
         {
            int iIndexToDelete = cLayerList.SelectedIndices[0];

            if (m_oLayers[iIndexToDelete] != null)
            {
               m_oLayers[iIndexToDelete].UnsubscribeToBuilderChangedEvent(new BuilderChangedHandler(this.BuilderChanged));
               m_oLayers[iIndexToDelete].RemoveLayer();
            }

            m_oLayers.RemoveAt(iIndexToDelete);
            cLayerList.Items.RemoveAt(iIndexToDelete);
            iLastIndex = iIndexToDelete;
         }
         if (iLastIndex == cLayerList.Items.Count) iLastIndex--;
         if (iLastIndex != -1) cLayerList.SelectedIndices.Add(iLastIndex);

         m_blSupressSelectedChanged = false;
         cLayerList.EndUpdate();

         RefreshLayerRenderOrder();
         SetButtonState();
         CmdViewMetadata();

         if (ActiveLayersChanged != null) ActiveLayersChanged();
         CheckIsValid();
      }

      /// <summary>
      /// Remove a GeoTiff from the active layers.
      /// </summary>
      /// <param name="szFilename">The filename of the GeoTiff to remove.</param>
      public void CmdRemoveGeoTiff(String szFilename)
      {
         cLayerList.BeginUpdate();
         m_blSupressSelectedChanged = true;

         int iIndex = m_oLayers.Count - 1;
         while (iIndex >= 0)
         {
            if (m_oLayers[iIndex] != null && m_oLayers[iIndex] is GeorefImageLayerBuilder &&
               ((GeorefImageLayerBuilder)m_oLayers[iIndex]).FileName.Equals(szFilename))
            {
               m_oLayers[iIndex].UnsubscribeToBuilderChangedEvent(new BuilderChangedHandler(this.BuilderChanged));
               m_oLayers[iIndex].RemoveLayer();

               m_oLayers.RemoveAt(iIndex);
               cLayerList.Items.RemoveAt(iIndex);
            }

            iIndex--;
         }

         m_blSupressSelectedChanged = false;
         cLayerList.EndUpdate();

         RefreshLayerRenderOrder();
         SetButtonState();

         if (ActiveLayersChanged != null) ActiveLayersChanged();
         CheckIsValid();
      }

      /// <summary>
      /// Open the download dialog.
      /// </summary>
      public void CmdDownloadActiveLayers()
      {
         if (DownloadsInProgress)
         {
            MessageBox.Show(this, "It is not possible to extract data while there are still downloads in progress.\nPlease wait for the downloads to complete and try again.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
         }

         Extract.DownloadSettings oDownloadDialog = new Dapple.Extract.DownloadSettings(this.SelectedLayers);
         oDownloadDialog.ShowDialog(this);
      }

      /// <summary>
      /// Set the active layers from a saved view.
      /// </summary>
      /// <param name="view">The view to load from.</param>
      /// <param name="oTree">The ServerTree to save servers to.</param>
      /// <returns>True if one or more of the layers is in an old/improper format.</returns>
      public bool CmdLoadFromView(DappleView view, ServerTree oTree)
      {
         bool blIncompleteLoad = false;

         cLayerList.BeginUpdate();
         m_blSupressSelectedChanged = true;

         cLayerList.Items.Clear();
         m_oLayers.Clear();

         AddLayer(m_hBaseLayer);

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
            AddLayer(oBuilder, i);
         }

         m_blSupressSelectedChanged = false;
         cLayerList.EndUpdate();

         CheckIsValid();

         return blIncompleteLoad;
      }

      #endregion

      #region Exporting

      private class ExportEntry
      {
         public LayerBuilder Container;
         public RenderableObject RO;
         public RenderableObject.ExportInfo Info;

         public ExportEntry(LayerBuilder container, RenderableObject ro, RenderableObject.ExportInfo expInfo)
         {
            Container = container;
            RO = ro;
            Info = expInfo;
         }
      }

      /// <summary>
      /// Extracts all the currently selected datasets (including blue marble).
      /// </summary>
      public void CmdExportSelected()
      {
         string szGeoTiff = null;
         List<ExportEntry> aExportList = new List<ExportEntry>();

         // Gather info first
         foreach (LayerBuilder oBuilder in this.SelectedLayers)
         {
            if (oBuilder.Visible)
            {
               RenderableObject oRObj = oBuilder.GetLayer();
               if (oRObj != null)
               {
                  RenderableObject.ExportInfo oExportInfo = new RenderableObject.ExportInfo();
                  oRObj.InitExportInfo(MainForm.WorldWindowSingleton.DrawArgs, oExportInfo);

                  if (oExportInfo.iPixelsX > 0 && oExportInfo.iPixelsY > 0)
                     aExportList.Add(new ExportEntry(oBuilder, oRObj, oExportInfo));
               }
            }
         }

         if (aExportList.Count == 0)
         {
            MessageBox.Show(this, "There are no visible layers to export.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
         }

         // Reverse the list to do render order right
         aExportList.Reverse();

         if (DownloadsInProgress)
         {
            MessageBox.Show(this, "It is not possible to export a view while there are still downloads in progress.\nPlease wait for the downloads to complete and try again.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
         }

         WorldWind.Camera.MomentumCamera camera = MainForm.WorldWindowSingleton.DrawArgs.WorldCamera as WorldWind.Camera.MomentumCamera;
         if (camera.Tilt.Degrees > 5.0)
         {
            MessageBox.Show(this, "It is not possible to export a tilted view. Reset the tilt using the navigation buttons\nor by using Right-Mouse-Button and drag and try again.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
         }

         try
         {
            ExportView oExportDialog = new ExportView(MainApplication.Settings.ConfigPath);
            if (oExportDialog.ShowDialog(this) == DialogResult.OK)
            {
               Cursor = Cursors.WaitCursor;

               // Stop the camera
               camera.SetPosition(camera.Latitude.Degrees, camera.Longitude.Degrees, camera.Heading.Degrees, camera.Altitude, camera.Tilt.Degrees);

               // Determine output parameters
               GeographicBoundingBox oViewedArea = GeographicBoundingBox.FromQuad(MainForm.WorldWindowSingleton.GetSearchBox());
               int iResolution = oExportDialog.Resolution;
               int iExportPixelsX, iExportPixelsY;

               // Minimize the estimated extents to what is available
               double dMinX = double.MaxValue;
               double dMaxX = double.MinValue;
               double dMinY = double.MaxValue;
               double dMaxY = double.MinValue;

               foreach (ExportEntry oExportEntry in aExportList)
               {
                  dMaxY = Math.Max(dMaxY, oExportEntry.Info.dMaxLat);
                  dMinY = Math.Min(dMinY, oExportEntry.Info.dMinLat);
                  dMaxX = Math.Max(dMaxX, oExportEntry.Info.dMaxLon);
                  dMinX = Math.Min(dMinX, oExportEntry.Info.dMinLon);
               }
               GeographicBoundingBox oExtractArea = new GeographicBoundingBox(dMaxY, dMinY, dMinX, dMaxX);

               oViewedArea.East = Math.Min(oViewedArea.East, oExtractArea.East);
               oViewedArea.North = Math.Min(oViewedArea.North, oExtractArea.North);
               oViewedArea.West = Math.Max(oViewedArea.West, oExtractArea.West);
               oViewedArea.South = Math.Max(oViewedArea.South, oExtractArea.South);

               // Determine the maximum resolution based on the highest res in layers
               if (iResolution == -1)
               {
                  double dXRes, dYRes;
                  foreach (ExportEntry oExportEntry in aExportList)
                  {
                     dXRes = (double)oExportEntry.Info.iPixelsX / (oExportEntry.Info.dMaxLon - oExportEntry.Info.dMinLon);
                     dYRes = (double)oExportEntry.Info.iPixelsY / (oExportEntry.Info.dMaxLat - oExportEntry.Info.dMinLat);
                     iResolution = Math.Max(iResolution, (int)Math.Round(Math.Max(dXRes * (oViewedArea.East - oViewedArea.West), dYRes * (oViewedArea.North - oViewedArea.South))));
                  }
               }
               if (iResolution <= 0)
                  return;

               if (oViewedArea.North - oViewedArea.South > oViewedArea.East - oViewedArea.West)
               {
                  iExportPixelsY = iResolution;
                  iExportPixelsX = (int)Math.Round((double)iResolution * (oViewedArea.East - oViewedArea.West) / (oViewedArea.North - oViewedArea.South));
               }
               else
               {
                  iExportPixelsX = iResolution;
                  iExportPixelsY = (int)Math.Round((double)iResolution * (oViewedArea.North - oViewedArea.South) / (oViewedArea.East - oViewedArea.West));
               }


               // Make geotiff metadata file to use for georeferencing images
               szGeoTiff = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
               using (StreamWriter sw = new StreamWriter(szGeoTiff, false))
               {
                  sw.WriteLine("Geotiff_Information:");
                  sw.WriteLine("Version: 1");
                  sw.WriteLine("Key_Revision: 1.0");
                  sw.WriteLine("Tagged_Information:");
                  sw.WriteLine("ModelTiepointTag (2,3):");
                  sw.WriteLine("0 0 0");
                  sw.WriteLine(oViewedArea.West.ToString() + " " + oViewedArea.North.ToString() + " 0");
                  sw.WriteLine("ModelPixelScaleTag (1,3):");
                  sw.WriteLine(((oViewedArea.East - oViewedArea.West) / (double)iExportPixelsX).ToString() + " " + ((oViewedArea.North - oViewedArea.South) / (double)iExportPixelsY).ToString() + " 0");
                  sw.WriteLine("End_Of_Tags.");
                  sw.WriteLine("Keyed_Information:");
                  sw.WriteLine("GTModelTypeGeoKey (Short,1): ModelTypeGeographic");
                  sw.WriteLine("GTRasterTypeGeoKey (Short,1): RasterPixelIsArea");
                  sw.WriteLine("GeogAngularUnitsGeoKey (Short,1): Angular_Degree");
                  sw.WriteLine("GeographicTypeGeoKey (Short,1): GCS_WGS_84");
                  sw.WriteLine("End_Of_Keys.");
                  sw.WriteLine("End_Of_Geotiff.");
               }

               // Export image(s)
               using (Bitmap oExportedImage = new Bitmap(iExportPixelsX, iExportPixelsY))
               {
                  using (Graphics oEIGraphics = Graphics.FromImage(oExportedImage))
                  {
                     oEIGraphics.FillRectangle(Brushes.White, new Rectangle(0, 0, iExportPixelsX, iExportPixelsY));
                     foreach (ExportEntry oExportEntry in aExportList)
                     {
                        // Limit info for layer to area we are looking at

                        double dExpXRes = (double)oExportEntry.Info.iPixelsX / (oExportEntry.Info.dMaxLon - oExportEntry.Info.dMinLon);
                        double dExpYRes = (double)oExportEntry.Info.iPixelsY / (oExportEntry.Info.dMaxLat - oExportEntry.Info.dMinLat);
                        int iExpOffsetX = (int)Math.Round((oViewedArea.West - oExportEntry.Info.dMinLon) * dExpXRes);
                        int iExpOffsetY = (int)Math.Round((oExportEntry.Info.dMaxLat - oViewedArea.North) * dExpYRes);
                        oExportEntry.Info.iPixelsX = (int)Math.Round((oViewedArea.East - oViewedArea.West) * dExpXRes);
                        oExportEntry.Info.iPixelsY = (int)Math.Round((oViewedArea.North - oViewedArea.South) * dExpYRes);
                        oExportEntry.Info.dMinLon = oExportEntry.Info.dMinLon + (double)iExpOffsetX / dExpXRes;
                        oExportEntry.Info.dMaxLat = oExportEntry.Info.dMaxLat - (double)iExpOffsetY / dExpYRes;
                        oExportEntry.Info.dMaxLon = oExportEntry.Info.dMinLon + (double)oExportEntry.Info.iPixelsX / dExpXRes;
                        oExportEntry.Info.dMinLat = oExportEntry.Info.dMaxLat - (double)oExportEntry.Info.iPixelsY / dExpYRes;

                        using (Bitmap oLayerImage = new Bitmap(oExportEntry.Info.iPixelsX, oExportEntry.Info.iPixelsY))
                        {
                           int iOffsetX, iOffsetY;
                           int iWidth, iHeight;

                           using (oExportEntry.Info.gr = Graphics.FromImage(oLayerImage))
                              oExportEntry.RO.ExportProcess(MainForm.WorldWindowSingleton.DrawArgs, oExportEntry.Info);

                           iOffsetX = (int)Math.Round((oExportEntry.Info.dMinLon - oViewedArea.West) * (double)iExportPixelsX / (oViewedArea.East - oViewedArea.West));
                           iOffsetY = (int)Math.Round((oViewedArea.North - oExportEntry.Info.dMaxLat) * (double)iExportPixelsY / (oViewedArea.North - oViewedArea.South));
                           iWidth = (int)Math.Round((oExportEntry.Info.dMaxLon - oExportEntry.Info.dMinLon) * (double)iExportPixelsX / (oViewedArea.East - oViewedArea.West));
                           iHeight = (int)Math.Round((oExportEntry.Info.dMaxLat - oExportEntry.Info.dMinLat) * (double)iExportPixelsY / (oViewedArea.North - oViewedArea.South));

                           ImageAttributes imgAtt = new ImageAttributes();
                           float[][] fMat = { 
                                 new float[] {1.0f, 0.0f, 0.0f, 0.0f, 0.0f},
                                 new float[] {0.0f, 1.0f, 0.0f, 0.0f, 0.0f},
                                 new float[] {0.0f, 0.0f, 1.0f, 0.0f, 0.0f},
                                 new float[] {0.0f, 0.0f, 0.0f, (float)oExportEntry.Container.Opacity/255.0f, 0.0f},
                                 new float[] {0.0f, 0.0f, 0.0f, 0.0f, 1.0f}
                              };
                           ColorMatrix clrMatrix = new ColorMatrix(fMat);
                           imgAtt.SetColorMatrix(clrMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                           oEIGraphics.DrawImage(oLayerImage, new Rectangle(iOffsetX, iOffsetY, iWidth, iHeight), 0, 0, oExportEntry.Info.iPixelsX, oExportEntry.Info.iPixelsY, GraphicsUnit.Pixel, imgAtt);

                           if (oExportDialog.KeepLayers)
                           {
                              using (Bitmap oIndividualLayerImage = new Bitmap(iExportPixelsX, iExportPixelsY))
                              {
                                 using (Graphics grl = Graphics.FromImage(oIndividualLayerImage))
                                 {
                                    grl.FillRectangle(Brushes.White, new Rectangle(0, 0, iExportPixelsX, iExportPixelsY));
                                    grl.DrawImage(oLayerImage, new Rectangle(iOffsetX, iOffsetY, iWidth, iHeight), 0, 0, oExportEntry.Info.iPixelsX, oExportEntry.Info.iPixelsY, GraphicsUnit.Pixel);
                                 }

                                 SaveGeoImage(oIndividualLayerImage, oExportDialog.OutputName + "_" + oExportEntry.Container.Name, oExportDialog.Folder, oExportDialog.OutputFormat, szGeoTiff);
                              }
                           }
                        }
                     }
                  }
                  SaveGeoImage(oExportedImage, oExportDialog.OutputName, oExportDialog.Folder, oExportDialog.OutputFormat, szGeoTiff);
               }
            }
         }
         catch (Exception exc)
         {
            MessageBox.Show(this, "Export failed!\n" + exc.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            Cursor = Cursors.Default;
         }
         finally
         {
            if (szGeoTiff != null && File.Exists(szGeoTiff))
               File.Delete(szGeoTiff);

            Cursor = Cursors.Default;
         }
      }

      /// <summary>
      /// Saves a bitmap to a GeoTiff, or if another format has been requested, just saves it.
      /// </summary>
      /// <param name="oBitmap"></param>
      /// <param name="szName"></param>
      /// <param name="strFolder"></param>
      /// <param name="szFormat"></param>
      /// <param name="szGeotiff"></param>
      private void SaveGeoImage(Bitmap oBitmap, string szName, string strFolder, string szFormat, string szGeotiff)
      {
         ImageFormat eFormat = ImageFormat.Tiff;

         if (szFormat.Equals(".bmp")) eFormat = ImageFormat.Bmp;
         else if (szFormat.Equals(".png")) eFormat = ImageFormat.Png;
         else if (szFormat.Equals(".gif")) eFormat = ImageFormat.Gif;

         foreach (char c in Path.GetInvalidFileNameChars())
            szName = szName.Replace(c, '_');

         if (eFormat == ImageFormat.Tiff)
         {
            string szTempImageFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + szFormat);
            oBitmap.Save(szTempImageFile, eFormat);

            ProcessStartInfo psi = new ProcessStartInfo(Path.GetDirectoryName(Application.ExecutablePath) + @"\System\geotifcp.exe");
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            psi.Arguments = "-g \"" + szGeotiff + "\" \"" + szTempImageFile + "\" \"" + Path.Combine(strFolder, szName + szFormat) + "\"";

            using (Process p = Process.Start(psi))
               p.WaitForExit();

            try
            {
               File.Delete(szTempImageFile);
            }
            catch
            {
            }
         }
         else
         {
            oBitmap.Save(Path.Combine(strFolder, szName + szFormat), eFormat);
         }
      }

      #endregion

      #region Testing

      /// <summary>
      /// Raise an exception if the layers in the internal layer list don't match up with the UI list.
      /// </summary>
      private void CheckIsValid()
      {
         if (m_hBaseLayer != null && !m_oLayers.Contains(m_hBaseLayer))
            throw new ArgumentException("You've somehow managed to delete the base layer");

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

      /// <summary>
      /// Passed in a drag drop operation to tell the layer list that the user is doing a shuffle, not an add.
      /// </summary>
      private class LayerListInternalShuffleToken { }

      /// <summary>
      /// Handles transparency application for single and multiple selections.
      /// </summary>
      private class TransparencyDriver
      {
         private List<LayerBuilder> m_aBuilders;
         private List<byte> m_aOriginalOpacities;
         private byte m_bReferenceValue;

         public TransparencyDriver()
         {
            m_aBuilders = new List<LayerBuilder>();
            m_aOriginalOpacities = new List<byte>();
            m_bReferenceValue = 128;
         }

         public byte ReferenceValue
         {
            get
            {
               return m_bReferenceValue;
            }
         }

         public void SetBuilders(List<LayerBuilder> oBuilders)
         {
            m_aBuilders.Clear();
            m_aBuilders.AddRange(oBuilders);
            m_aOriginalOpacities.Clear();

            byte bMinOpacity = Byte.MaxValue;
            byte bMaxOpacity = Byte.MinValue;

            foreach (LayerBuilder oBuilder in m_aBuilders)
            {
               byte bOpacity = oBuilder.Opacity;

               bMinOpacity = Math.Min(bMinOpacity, bOpacity);
               bMaxOpacity = Math.Max(bMaxOpacity, bOpacity);

               m_aOriginalOpacities.Add(bOpacity);
            }

            m_bReferenceValue = (byte)(((int)bMinOpacity + (int)bMaxOpacity) / 2);
         }

         public void OpacityChanged(byte bNewValue)
         {
            ApplyTransparencies(bNewValue);
         }

         private void ApplyTransparencies(byte bSliderValue)
         {
            bool blNegative = bSliderValue < m_bReferenceValue;
            double dRatio = 0.0;

            if (blNegative)
            {
               dRatio = (double)bSliderValue / (double)m_bReferenceValue;
            }
            else
            {
               dRatio = (double)(bSliderValue - m_bReferenceValue) / (double)(Byte.MaxValue - m_bReferenceValue);
            }

            for (int count = 0; count < m_aBuilders.Count; count++)
            {
               LayerBuilder oBuilder = m_aBuilders[count];
               byte bOriginalOpacity = m_aOriginalOpacities[count];

               if (bSliderValue == m_bReferenceValue)
               {
                  oBuilder.Opacity = bOriginalOpacity;
               }
               else
               {
                  if (blNegative)
                  {
                     oBuilder.Opacity = (byte)(bOriginalOpacity * dRatio);
                  }
                  else
                  {
                     oBuilder.Opacity = (byte)((Byte.MaxValue - bOriginalOpacity) * dRatio + bOriginalOpacity);
                  }
               }
            }
         }
      }
   }
}
