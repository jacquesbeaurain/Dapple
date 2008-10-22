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

		#region Events

		public event EventHandler LayerSelectionChanged;

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

      private bool m_blAllowExtract = true;
		private bool m_blLoadingFromView = false;

      private LayerBuilder m_hBaseLayer = null;

      private TransparencyDriver m_oTransparencyDriver = new TransparencyDriver();

      public event GoToHandler GoTo;
      public event Dapple.MainForm.ViewMetadataHandler ViewMetadata;
      public event ActiveLayersChangedHandler ActiveLayersChanged;
		private ServerTree m_hServerTree;

		bool m_bClearDropHint = false;
		Point m_ptDropHint1, m_ptDropHint2;
		static System.Drawing.Pen m_oDragPen = new System.Drawing.Pen(System.Drawing.Brushes.Black, 2.0f);
		static System.Drawing.Pen m_oDragNoPen = new System.Drawing.Pen(System.Drawing.Brushes.White, 2.0f);

		private bool m_blDragAllowed = false;

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
            foreach (int index in c_lvLayers.SelectedIndices)
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
            List<LayerBuilder> oResult = new List<LayerBuilder>(m_oLayers);
            if (m_hBaseLayer != null) oResult.Add(m_hBaseLayer);
            return oResult;
         }
      }

      public List<LayerBuilder> ExtractLayers
      {
         get
         {
            List<LayerBuilder> oResult = new List<LayerBuilder>();

				foreach (LayerBuilder oBuilder in m_oLayers)
				{
					if (oBuilder.Visible) oResult.Add(oBuilder);
				}
            return oResult;
         }
      }

      public ImageList ImageList
      {
         set
         {
            c_lvLayers.SmallImageList = value;
            c_lvLayers.LargeImageList = value;
         }
      }

      public bool OMFeaturesEnabled
      {
         set
         {
            c_bExtract.Visible = value;
         }
      }

      /// <summary>
      /// Whether a delete operation should be allowed to commence (disallows deletion of the base layer).
      /// </summary>
      public bool RemoveAllowed
      {
         get
         {
            return c_lvLayers.SelectedIndices.Count > 1 || (c_lvLayers.SelectedIndices.Count == 1 && !m_oLayers[c_lvLayers.SelectedIndices[0]].Equals(m_hBaseLayer));
         }
      }

      public bool DownloadsInProgress
      {
         get
         {
            int temp, temp2;

            foreach (LayerBuilder oBuilder in this.AllLayers)
            {
               if (oBuilder.Visible && !oBuilder.Equals(m_hBaseLayer) && oBuilder.bIsDownloading(out temp, out temp2))
                  return true;
            }
            return false;
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

         c_lvLayers.BeginUpdate();
         m_blSupressSelectedChanged = true;

         for (int count = 0; count < oLayers.Count; count++)
         {
            AddLayer(oLayers[count], iInsertIndex + count);
         }

         c_lvLayers.SelectedIndices.Clear();

         for (int count = iInsertIndex; count < iInsertIndex + oLayers.Count; count++)
         {
            c_lvLayers.Items[count].Selected = true;
         }

         m_blSupressSelectedChanged = false;

         cLayerList_SelectedIndexChanged(this, new EventArgs());
         c_lvLayers.EndUpdate();
      }

      public void SetBaseLayer(LayerBuilder oBaseLayer)
      {
         // TODO: remove an existing base layer, if necessary.
         m_hBaseLayer = oBaseLayer;
         m_hBaseLayer.Temporary = true;
         AddLayerToGlobe(oBaseLayer);
      }

      public void AddLayer(LayerBuilder oLayer)
      {
         AddLayer(oLayer, 0);
      }

      private bool AddLayer(LayerBuilder oNewBuilder, int iInsertIndex)
      {
         if (m_oLayers.Contains(oNewBuilder)) return false;

			if (!m_blLoadingFromView)
				oNewBuilder.Reset();

         m_oLayers.Insert(iInsertIndex, oNewBuilder);
         c_lvLayers.Items.Insert(iInsertIndex, oNewBuilder.Title);
         c_lvLayers.Items[iInsertIndex].Checked = m_oLayers[iInsertIndex].Visible;
         {
            c_lvLayers.Items[iInsertIndex].ImageIndex = c_lvLayers.SmallImageList.Images.IndexOfKey(m_oLayers[iInsertIndex].DisplayIconKey);
         }

         if (!AddLayerToGlobe(m_oLayers[iInsertIndex]))
         {
            c_lvLayers.Items[iInsertIndex].ImageIndex = c_lvLayers.SmallImageList.Images.IndexOfKey("error");
         }

         bool blSupressed = m_blSupressSelectedChanged;
         m_blSupressSelectedChanged = true;

         c_lvLayers.SelectedIndices.Clear();
         c_lvLayers.Items[iInsertIndex].Selected = true;

         m_blSupressSelectedChanged = blSupressed;
         cLayerList_SelectedIndexChanged(this, new EventArgs());

         if (ActiveLayersChanged != null) ActiveLayersChanged();

			ResizeColumn();
         CheckIsValid();

			return true;
      }

      private bool AddLayerToGlobe(LayerBuilder oBuilder)
      {
         return AddLayerToGlobe(oBuilder, oBuilder is GeorefImageLayerBuilder || oBuilder is KML.KMLLayerBuilder);
      }

      private bool AddLayerToGlobe(LayerBuilder oBuilder, bool blSync)
      {
         oBuilder.SubscribeToBuilderChangedEvent(new BuilderChangedHandler(this.BuilderChanged));

         if ((oBuilder is GeorefImageLayerBuilder || oBuilder is KML.KMLLayerBuilder)&& oBuilder.GetLayer() == null) return false;
         if (blSync)
         {
            oBuilder.SyncAddLayer(true);
            RefreshLayerRenderOrder();
         }
         else
         {
            oBuilder.AsyncAddLayer();
         }
         return true;
      }

		public ServerTree ServerTree
		{
			set { m_hServerTree = value; }
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
         int iPos = 8 + m_iLastTransparency * (c_tbTransparency.Bounds.Width - 26) / 255;

         // Attempt to match the toolstrip next to it by drawing with same colors, bottom line and separator
         g.FillRectangle(SystemBrushes.ButtonFace, c_tbTransparency.Bounds.X, c_tbTransparency.Bounds.Y, c_tbTransparency.Bounds.Width, c_tbTransparency.Bounds.Height);
         g.DrawLine(Pens.LightGray, c_tbTransparency.Bounds.X, c_tbTransparency.Bounds.Y + c_tbTransparency.Bounds.Height - 2, c_tbTransparency.Bounds.X + c_tbTransparency.Bounds.Width, c_tbTransparency.Bounds.Y + c_tbTransparency.Bounds.Height - 2);
         g.DrawLine(Pens.White, c_tbTransparency.Bounds.X, c_tbTransparency.Bounds.Y + c_tbTransparency.Bounds.Height - 1, c_tbTransparency.Bounds.X + c_tbTransparency.Bounds.Width, c_tbTransparency.Bounds.Y + c_tbTransparency.Bounds.Height - 1);
         Rectangle rect = new Rectangle(c_tbTransparency.Bounds.X + 3, c_tbTransparency.Bounds.Y + c_tbTransparency.Bounds.Height / 2 - 8, c_tbTransparency.Bounds.Width - 7, 16);
         if (c_tbTransparency.Enabled)
         {
            using (LinearGradientBrush lgb = new LinearGradientBrush(rect, SystemColors.ButtonFace, SystemColors.ControlDarkDark, LinearGradientMode.Horizontal))
               g.FillRectangle(lgb, rect);
            if (c_tbTransparency.Focused)
            {
               g.DrawRectangle(SystemPens.ActiveCaption, rect);
            }
            else
            {
               g.DrawRectangle(SystemPens.InactiveCaption, rect);
            }
            g.FillRectangle(SystemBrushes.ControlDarkDark, c_tbTransparency.Bounds.X + 9, c_tbTransparency.Bounds.Y + c_tbTransparency.Bounds.Height / 2 - 1, c_tbTransparency.Bounds.Width - 17, 2);
            g.DrawImage(global::Dapple.Properties.Resources.trackbutton, c_tbTransparency.Bounds.X + iPos, c_tbTransparency.Bounds.Y + 1, global::Dapple.Properties.Resources.trackbutton.Width, c_tbTransparency.Bounds.Height - 3);
         }
         else
         {
            using (LinearGradientBrush lgb = new LinearGradientBrush(rect, SystemColors.ButtonFace, SystemColors.ControlDark, LinearGradientMode.Horizontal))
               g.FillRectangle(lgb, rect);
            g.DrawRectangle(SystemPens.ControlDark, rect);
            g.FillRectangle(SystemBrushes.ControlDark, c_tbTransparency.Bounds.X + 9, c_tbTransparency.Bounds.Y + c_tbTransparency.Bounds.Height / 2 - 1, c_tbTransparency.Bounds.Width - 17, 2);
            g.DrawImage(global::Dapple.Properties.Resources.trackbutton_disable, c_tbTransparency.Bounds.X + iPos, c_tbTransparency.Bounds.Y + 1, global::Dapple.Properties.Resources.trackbutton.Width, c_tbTransparency.Bounds.Height - 3);
         }
         if (e.ClipRectangle.X > 0)
            c_tbTransparency.Invalidate();
      }

      private void cTransparencySlider_ValueChanged(object sender, EventArgs e)
      {
         m_iLastTransparency = c_tbTransparency.Value;
         if (c_tbTransparency.Enabled)
         {
            m_oTransparencyDriver.OpacityChanged((byte)c_tbTransparency.Value);
         }

         // Invalidate to make sure our custom paintjob is fresh
         c_tbTransparency.Invalidate();
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
         CmdExtractVisibleLayers();
      }

      private void cExportButton_Click(object sender, EventArgs e)
      {
         CmdTakeSnapshot();
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
            Point oClientLocation = c_lvLayers.PointToClient(new Point(e.X, e.Y));
            int iInsertPoint = GetDropIndex(oClientLocation.Y);

            AddLayers(oDropData, iInsertPoint);

            c_lvLayers.Focus();
         }
         else if (e.Data.GetDataPresent(typeof(LayerListInternalShuffleToken)))
         {
            Point oClientLocation = c_lvLayers.PointToClient(new Point(e.X, e.Y));
            int iInsertPoint = GetDropIndex(oClientLocation.Y);

            ShuffleSelectedLayers(iInsertPoint);

            c_lvLayers.Focus();
         }
      }

      private void cLayerList_SelectedIndexChanged(object sender, EventArgs e)
      {
         if (!m_blSupressSelectedChanged)
         {
            SetButtonState();
            CmdViewMetadata();
				if (LayerSelectionChanged != null) LayerSelectionChanged(this, new EventArgs());
         }
      }

      private void cLayerList_ItemCheck(object sender, ItemCheckEventArgs e)
      {
         m_oLayers[e.Index].Visible = e.NewValue == CheckState.Checked;
			SetButtonState();
      }

      private void cLayerList_MouseMove(object sender, MouseEventArgs e)
      {
         if ((e.Button & MouseButtons.Left) == MouseButtons.Left && c_lvLayers.SelectedIndices.Count > 0 && m_blDragAllowed)
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

		private void c_lvLayers_MouseDown(object sender, MouseEventArgs e)
		{
			m_blDragAllowed = e.X >= 23;
		}

      #endregion

      #region Context Menu

      private void cLayerListContextMenu_Opening(object sender, CancelEventArgs e)
      {
         if (c_lvLayers.SelectedIndices.Count == 0)
         {
            e.Cancel = true;
            return;
         }

			bool blOneSelected = c_lvLayers.SelectedIndices.Count == 1;

         c_miRemoveLayer.Enabled = this.RemoveAllowed;
			c_miGoToLayer.Enabled = blOneSelected;
			c_miProperties.Enabled = blOneSelected;
			c_miViewLegend.Enabled = blOneSelected && m_oLayers[c_lvLayers.SelectedIndices[0]].SupportsLegend;

			if (blOneSelected && m_oLayers[c_lvLayers.SelectedIndices[0]].LayerFromSupportedServer)
			{
				c_miAddOrGoToServer.Visible = true;
				c_miAddOrGoToServer.Enabled = true;

				if (m_oLayers[c_lvLayers.SelectedIndices[0]].ServerIsInHomeView)
				{
					c_miAddOrGoToServer.Text = "Open Server in Server Tree";
				}
				else
				{
					c_miAddOrGoToServer.Text = "Add Server to Home View";
				}
			}
			else
			{
				c_miAddOrGoToServer.Enabled = false;
				c_miAddOrGoToServer.Visible = false;
			}
      }

      private void cGoToToolStripMenuItem_Click(object sender, EventArgs e)
      {
         CmdGoTo();
      }

      private void cViewPropertiesToolStripMenuItem_Click(object sender, EventArgs e)
      {
         LayerBuilder oBuilder = m_oLayers[c_lvLayers.SelectedIndices[0]];
         if (oBuilder == null) return;

         frmProperties.DisplayForm(oBuilder);

         if (oBuilder.IsChanged)
         {
            CmdRefreshLayer(c_lvLayers.SelectedIndices[0]);
         }
      }

      private void cRemoveToolStripMenuItem_Click(object sender, EventArgs e)
      {
         CmdRemoveSelectedLayers();
      }

      private void cRefreshToolStripMenuItem_Click(object sender, EventArgs e)
      {
         foreach (int iIndex in c_lvLayers.SelectedIndices)
         {
            CmdRefreshLayer(iIndex);
         }
      }

      private void cClearCacheToolStripMenuItem_Click(object sender, EventArgs e)
      {
         foreach (int iIndex in c_lvLayers.SelectedIndices)
         {
            CmdClearLayerCache(iIndex);
         }
      }

      private void cViewLegendToolStripMenuItem_Click(object sender, EventArgs e)
      {
         string[] aLegends = m_oLayers[c_lvLayers.SelectedIndices[0]].GetLegendURLs();
         foreach (string szLegend in aLegends)
         {
            if (!String.IsNullOrEmpty(szLegend)) MainForm.BrowseTo(szLegend);
         }
      }

		private void c_miAddOrGoToServer_Click(object sender, EventArgs e)
		{
			LayerBuilder oBuilder = m_oLayers[c_lvLayers.SelectedIndices[0]];

			if (!oBuilder.ServerIsInHomeView)
			{
				oBuilder.AddServerToHomeView(this.FindForm() as MainForm);
			}

			oBuilder.SelectServer(m_hServerTree);
			(this.FindForm() as MainForm).CmdShowServerTree();
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
			c_lvLayers.Columns[0].AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
      }

      /// <summary>
      /// Set the enabled and value settings of the widgets in this control.
      /// </summary>
      private void SetButtonState()
      {
         c_tbTransparency.Enabled = c_lvLayers.SelectedIndices.Count > 0;

         if (c_tbTransparency.Enabled)
         {
            List<LayerBuilder> oSelectedLayers = new List<LayerBuilder>();
            foreach (int iIndex in c_lvLayers.SelectedIndices)
            {
               oSelectedLayers.Add(m_oLayers[iIndex]);
            }

            m_oTransparencyDriver.SetBuilders(oSelectedLayers);
            c_tbTransparency.Value = m_oTransparencyDriver.ReferenceValue;
         }

         c_bGoToLayer.Enabled = c_lvLayers.SelectedIndices.Count == 1;
         c_RemoveLayer.Enabled = this.RemoveAllowed;
         c_bExtract.Enabled = (ExtractLayers.Count > 0) && m_blAllowExtract;
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
            if (oBuilder.Equals(m_hBaseLayer))
            {
               if (eChangeType == BuilderChangeType.LoadedASync)
               {
                  RefreshLayerRenderOrder();
               }
            }
            else
            {
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
                  c_lvLayers.Items[iBuilderIndex].ImageIndex = c_lvLayers.SmallImageList.Images.IndexOfKey(oBuilder.DisplayIconKey);
               }
               else if (eChangeType == BuilderChangeType.LoadedASyncFailed || eChangeType == BuilderChangeType.LoadedSyncFailed)
               {
                  c_lvLayers.Items[iBuilderIndex].ImageIndex = c_lvLayers.SmallImageList.Images.IndexOfKey("error");
               }
               else if (eChangeType == BuilderChangeType.VisibilityChanged)
               {
                  c_lvLayers.Items[iBuilderIndex].Checked = m_oLayers[iBuilderIndex].Visible;
               }
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
         if (m_hBaseLayer != null) m_hBaseLayer.PushBackInRenderOrder();
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
			clearDropHint();
         DrawDropHint();
      }

      /// <summary>
      /// Clears the point the mouse is over.
      /// </summary>
      private void ClearDropLocation()
      {
         m_oDropLocation = NO_DROP_LOCATION;
			clearDropHint();
      }

		/// <summary>
		/// Clear previously drawn drop hint line
		/// </summary>
		private void clearDropHint()
		{
			if (m_bClearDropHint)
			{
				using (Graphics oGraphics = c_lvLayers.CreateGraphics())
				{
					oGraphics.DrawLine(m_oDragNoPen, m_ptDropHint1, m_ptDropHint2);
				}
				m_bClearDropHint = false;
			}
		}

      /// <summary>
      /// Gets the index of the in-between-layers point that a drop operation will drop to.
      /// </summary>
      /// <param name="iClientY"></param>
      /// <returns></returns>
      private int GetDropIndex(int iClientY)
      {
         int result = c_lvLayers.Items.IndexOf(c_lvLayers.GetItemAt(35, iClientY));
         if (result == -1) return c_lvLayers.Items.Count;
         Rectangle oItemBounds = c_lvLayers.GetItemRect(result);
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
         Point oClientDropLocation = c_lvLayers.PointToClient(m_oDropLocation);
			using (Graphics oGraphics = c_lvLayers.CreateGraphics())
			{
				if (!oGraphics.VisibleClipBounds.Contains(oClientDropLocation)) return;

				int iLineWidth = (int)oGraphics.VisibleClipBounds.Width;
				int iLineY = 0;

				if (c_lvLayers.Items.Count != 0)
				{
					int iInsertPoint = GetDropIndex(oClientDropLocation.Y);

					if (iInsertPoint == c_lvLayers.Items.Count)
					{
						iLineY = c_lvLayers.GetItemRect(iInsertPoint - 1).Bottom;
					}
					else
					{
						iLineY = c_lvLayers.GetItemRect(iInsertPoint).Top - 1;
					}
				}
				m_ptDropHint1.X = 0;
				m_ptDropHint1.Y = iLineY;
				m_ptDropHint2.X = iLineWidth;
				m_ptDropHint2.Y = iLineY;
				oGraphics.DrawLine(m_oDragPen, m_ptDropHint1, m_ptDropHint2);
				m_bClearDropHint = true;
			}
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
         if (c_lvLayers.SelectedIndices.Count < 1) throw new InvalidOperationException("No layers are selected");

         // --- Check if the selection is continuous ---

         bool blContinuousSelection = c_lvLayers.SelectedIndices[c_lvLayers.SelectedIndices.Count - 1] - c_lvLayers.SelectedIndices[0] + 1 == c_lvLayers.SelectedIndices.Count;
         
         // --- Don't do anything if the selection is continuous and the drop index selected wouldn't move the items ---

         if (blContinuousSelection)
         {
            if (iDropIndex >= c_lvLayers.SelectedIndices[0] && iDropIndex <= c_lvLayers.SelectedIndices[c_lvLayers.SelectedIndices.Count - 1] + 1)
            {
               return;
            }
         }

         // --- If the drop index is below a selected layer, decrement it, as removing the layer from the list before the insert changes the indices ---

         int iDecrement = 0;
         foreach (int iSelectedIndex in c_lvLayers.SelectedIndices)
         {
            if (iDropIndex > iSelectedIndex) iDecrement++; 
         }
         iDropIndex -= iDecrement;
         
         // --- Store items to move in temporary buffers ---

         List<LayerBuilder> oMovedContainers = new List<LayerBuilder>();
         List<ListViewItem> oMovedItems = new List<ListViewItem>();

         foreach (int iSelectedIndex in c_lvLayers.SelectedIndices)
         {
            oMovedContainers.Add(m_oLayers[iSelectedIndex]);
            oMovedItems.Add(c_lvLayers.Items[iSelectedIndex]);
         }

         // --- Remove the selected layers ---

         c_lvLayers.SuspendLayout();
         m_blSupressSelectedChanged = true;

         while (c_lvLayers.SelectedIndices.Count > 0)
         {
            m_oLayers.RemoveAt(c_lvLayers.SelectedIndices[0]);
            c_lvLayers.Items.RemoveAt(c_lvLayers.SelectedIndices[0]);
         }

         // --- Reinsert and reselect the selected layers ---

         for (int count = 0; count < oMovedContainers.Count; count++)
         {
            m_oLayers.Insert(count + iDropIndex, oMovedContainers[count]);
            c_lvLayers.Items.Insert(count + iDropIndex, oMovedItems[count]);
            c_lvLayers.SelectedIndices.Add(count + iDropIndex);
         }

         m_blSupressSelectedChanged = false;
         c_lvLayers.ResumeLayout();

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
         if (m_oLayers[c_lvLayers.SelectedIndices[0]] != null)
         {
            string strCache = m_oLayers[iIndex].GetCachePath();
            if (!string.IsNullOrEmpty(strCache))
            {
               Utility.FileSystem.DeleteFolderGUI(this, strCache, "Clearing Data Layer Cache");
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
         c_lvLayers.Items[iIndex].ImageIndex = c_lvLayers.SmallImageList.Images.IndexOfKey("time");
         m_oLayers[iIndex].RefreshLayer();
      }

      /// <summary>
      /// Go to selected layer.
      /// </summary>
      private void CmdGoTo()
      {
         if (c_lvLayers.SelectedIndices.Count == 1 && GoTo != null && m_oLayers[c_lvLayers.SelectedIndices[0]] != null)
         {
            GoTo(m_oLayers[c_lvLayers.SelectedIndices[0]]);
         }
      }

      /// <summary>
      /// Display the metadata for the selected layer.
      /// </summary>
      private void CmdViewMetadata()
      {
			if (c_lvLayers.SelectedIndices.Count == 1 && ViewMetadata != null && m_oLayers[c_lvLayers.SelectedIndices[0]] != null)
			{
				ViewMetadata(m_oLayers[c_lvLayers.SelectedIndices[0]]);
			}
			else
			{
				ViewMetadata(null);
			}
      }

      /// <summary>
      /// Remove selected layers in the layer list.
      /// </summary>
      public void CmdRemoveSelectedLayers()
      {
         if (c_lvLayers.SelectedIndices.Count == 0) return;

			c_lvLayers.BeginUpdate();
         m_blSupressSelectedChanged = true;

			// --- Get the list of layers to delete, and ListViewItems to not delete.

			List<int> oIndices = new List<int>();
			List<ListViewItem> oLVIs = new List<ListViewItem>();

         foreach (int iIndex in c_lvLayers.SelectedIndices)
         {
				if (!m_oLayers[iIndex].Equals(m_hBaseLayer))
				{
					oIndices.Add(iIndex);
				}
         }
			for (int count = 0; count < c_lvLayers.Items.Count; count++)
			{
				if (!oIndices.Contains(count))
				{
					oLVIs.Add(c_lvLayers.Items[count]);
				}
			}

			// --- Delete the layers bottom-up, because top-down will damage the indices ---

			oIndices.Sort();
			oIndices.Reverse();

         int iLastIndex = 0;
			foreach (int iIndexToDelete in oIndices)
			{
            if (m_oLayers[iIndexToDelete] != null)
            {
               m_oLayers[iIndexToDelete].UnsubscribeToBuilderChangedEvent(new BuilderChangedHandler(this.BuilderChanged));
               m_oLayers[iIndexToDelete].RemoveLayer();
            }

            m_oLayers.RemoveAt(iIndexToDelete);
            iLastIndex = iIndexToDelete;
         }

			// --- Populate the ListView with those ListViewItems not deleted ---

			c_lvLayers.SelectedIndices.Clear();
			c_lvLayers.Items.Clear();

			foreach (ListViewItem oItem in oLVIs)
			{
				c_lvLayers.Items.Add(oItem);
			}

			// --- Restore the "next item" selection if any items remain ---

         if (iLastIndex == c_lvLayers.Items.Count) iLastIndex--;
         if (iLastIndex != -1) c_lvLayers.SelectedIndices.Add(iLastIndex);

			m_blSupressSelectedChanged = false;
			c_lvLayers.EndUpdate();

         RefreshLayerRenderOrder();
			ResizeColumn();
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
         c_lvLayers.BeginUpdate();
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
               c_lvLayers.Items.RemoveAt(iIndex);
            }

            iIndex--;
         }

         m_blSupressSelectedChanged = false;
         c_lvLayers.EndUpdate();

         RefreshLayerRenderOrder();
			ResizeColumn();
         SetButtonState();

         if (ActiveLayersChanged != null) ActiveLayersChanged();
         CheckIsValid();
      }

      /// <summary>
      /// Open the download dialog.
      /// </summary>
      public void CmdExtractVisibleLayers()
      {
			// --- Dataset extraction will operate in testing mode, but ensure that you don't try to extract
			// --- a dataset that needs to be fully downloaded
         if (DownloadsInProgress && !Program.g_blTestingMode)
         {
				Program.ShowMessageBox(
					"It is not possible to extract data while Dapple is downloading tiles for visible data layers.\nPlease wait for tile downloading to complete and try again.",
					"Extract Layers",
					MessageBoxButtons.OK,
					MessageBoxDefaultButton.Button1,
					MessageBoxIcon.Warning);
            return;
         }
			List<LayerBuilder> aExtractLayers = this.ExtractLayers;
			if (aExtractLayers.Count == 0)
			{
				Program.ShowMessageBox(
					"None of the enabled layers intersect the current view.\nEither enable some disabled data layers, or move the view.",
					"Extract Layers",
					MessageBoxButtons.OK,
					MessageBoxDefaultButton.Button1,
					MessageBoxIcon.Warning);
				return;
			}

         Extract.DownloadSettings oDownloadDialog = new Dapple.Extract.DownloadSettings(aExtractLayers, this.TopLevelControl as Form);
         oDownloadDialog.ShowInTaskbar = false;
         DialogResult oResult = oDownloadDialog.ShowDialog(this);
			if (oResult == DialogResult.OK)
			{
				if (MainForm.Client == Dapple.Extract.Options.Client.ClientType.MapInfo)
				{
					// Assumption: the layer list is on the main form of dapple.  If it's anywhere else, this'll need to be updated.
					if (oDownloadDialog.LayersDownloaded)
						this.ParentForm.Close();
				}
				else
				{
					Program.ShowMessageBox(
						"Extraction complete.",
						"Extract Layers",
						MessageBoxButtons.OK,
						MessageBoxDefaultButton.Button1,
						MessageBoxIcon.Information);
				}
			}
			else if (oResult == DialogResult.Abort)
			{
				Program.ShowMessageBox(
					"Connection to " + Utility.EnumUtils.GetDescription(MainForm.Client) + " lost, unable to extract datasets.",
					"Extract Layers",
					MessageBoxButtons.OK,
					MessageBoxDefaultButton.Button1,
					MessageBoxIcon.Error);
				m_blAllowExtract = false;
				c_bExtract.Enabled = false;
			}
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

         c_lvLayers.BeginUpdate();
         m_blSupressSelectedChanged = true;
			m_blLoadingFromView = true;

         c_lvLayers.Items.Clear();
			foreach (LayerBuilder oBuilder in m_oLayers)
				oBuilder.RemoveLayer();
         m_oLayers.Clear();

			int iInsertIndex = 0;
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
				if (oBuilder != null)
				{
					oBuilder.Visible = dataset.Hasinvisible() ? !dataset.invisible.Value : true;
					oBuilder.Opacity = (byte)dataset.opacity.Value;
					if (AddLayer(oBuilder, iInsertIndex))
					{
						iInsertIndex++;
					}
				}
         }

			m_blLoadingFromView = false;
         m_blSupressSelectedChanged = false;
         c_lvLayers.EndUpdate();

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
      public void CmdTakeSnapshot()
      {
         string szGeoTiff = null;
         List<ExportEntry> aExportList = new List<ExportEntry>();

         // Gather info first
         foreach (LayerBuilder oBuilder in this.AllLayers)
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
            Program.ShowMessageBox(
					"There are no visible layers to export.",
					"Create GeoTIFF Snapshot",
					MessageBoxButtons.OK,
					MessageBoxDefaultButton.Button1,
					MessageBoxIcon.Warning);
            return;
         }

         // Reverse the list to do render order right
         aExportList.Reverse();

         if (DownloadsInProgress)
         {
            Program.ShowMessageBox(
					"It is not possible to create a snapshot while Dapple is downloading tiles for visible data layers.\nPlease wait for tile downloading to complete and try again.",
					"Create GeoTIFF Snapshot",
					MessageBoxButtons.OK,
					MessageBoxDefaultButton.Button1,
					MessageBoxIcon.Warning);
            return;
         }

         WorldWind.Camera.MomentumCamera camera = MainForm.WorldWindowSingleton.DrawArgs.WorldCamera as WorldWind.Camera.MomentumCamera;
         if (camera.Tilt.Degrees > 5.0)
         {
				Program.ShowMessageBox(
					"It is not possible to create a snapshot of a tilted view.\nPlease reset the tilt using the navigation buttons and try again.",
					"Create GeoTIFF Snapshot",
					MessageBoxButtons.OK,
					MessageBoxDefaultButton.Button1,
					MessageBoxIcon.Warning);
            return;
         }

         try
         {
            ExportView oExportDialog = null;
            if (MainForm.MontajInterface != null)
            {
               try
               {
                  oExportDialog = new ExportView(Path.Combine(MainForm.UserPath, MainApplication.Settings.ConfigPath), MainForm.MontajInterface.BaseDirectory());
               }
               catch (System.Runtime.Remoting.RemotingException)
               {
						oExportDialog = new ExportView(Path.Combine(MainForm.UserPath, MainApplication.Settings.ConfigPath));
               }
            }
            else
            {
					oExportDialog = new ExportView(Path.Combine(MainForm.UserPath, MainApplication.Settings.ConfigPath));
            }

            if (oExportDialog.ShowDialog(this) == DialogResult.OK)
            {
					String szFilename = Path.ChangeExtension(oExportDialog.FullFileName, ".tif");

					// --- Delete all the files that OM generates, so we don't get invalid projections ---
					if (System.IO.File.Exists(szFilename))
						System.IO.File.Delete(szFilename);
					if (System.IO.File.Exists(System.IO.Path.ChangeExtension(szFilename, ".ipj")))
						System.IO.File.Delete(System.IO.Path.ChangeExtension(szFilename, ".ipj"));
					if (System.IO.File.Exists(System.IO.Path.ChangeExtension(szFilename, ".gi")))
						System.IO.File.Delete(System.IO.Path.ChangeExtension(szFilename, ".gi"));
					if (System.IO.File.Exists(System.IO.Path.ChangeExtension(szFilename, ".tif.xml")))
						System.IO.File.Delete(System.IO.Path.ChangeExtension(szFilename, ".tif.xml"));

               Cursor = Cursors.WaitCursor;

               // Stop the camera
               camera.SetPosition(camera.Latitude.Degrees, camera.Longitude.Degrees, camera.Heading.Degrees, camera.Altitude, camera.Tilt.Degrees);

               // Determine output parameters
               GeographicBoundingBox oViewedArea = GeographicBoundingBox.FromQuad(MainForm.WorldWindowSingleton.GetSearchBox());
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
					double dPixelsPerDegree = -1;
               foreach (ExportEntry oExportEntry in aExportList)
               {
                  double dXRes = (double)oExportEntry.Info.iPixelsX / (oExportEntry.Info.dMaxLon - oExportEntry.Info.dMinLon);
                  double dYRes = (double)oExportEntry.Info.iPixelsY / (oExportEntry.Info.dMaxLat - oExportEntry.Info.dMinLat);
						dPixelsPerDegree = Math.Max(dPixelsPerDegree, Math.Max(dXRes, dYRes));
               }

					double dMaxPixelsPerDegree = Math.Sqrt((5120 * 2560) / (oViewedArea.Latitude * oViewedArea.Longitude));
					if (dPixelsPerDegree > dMaxPixelsPerDegree)
						dPixelsPerDegree = dMaxPixelsPerDegree;

					iExportPixelsX = (int)(oViewedArea.Longitude * dPixelsPerDegree);
					iExportPixelsY = (int)(oViewedArea.Latitude * dPixelsPerDegree);

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
						sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture,
							"{0} {1} {2}",
							oViewedArea.West,
							oViewedArea.North,
							0));
                  sw.WriteLine("ModelPixelScaleTag (1,3):");
						sw.WriteLine(String.Format(System.Globalization.CultureInfo.InvariantCulture,
							"{0} {1} {2}",
							(oViewedArea.East - oViewedArea.West) / (double)iExportPixelsX,
							(oViewedArea.North - oViewedArea.South) / (double)iExportPixelsY,
							0));
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
					try
					{
						using (Bitmap oExportedImage = new Bitmap(iExportPixelsX, iExportPixelsY))
						{
							using (Graphics oEIGraphics = Graphics.FromImage(oExportedImage))
							{
								oEIGraphics.FillRectangle(Brushes.White, new Rectangle(0, 0, iExportPixelsX, iExportPixelsY));
								foreach (ExportEntry oExportEntry in aExportList)
								{
									// Clip layer export to viewed area
									if (oExportEntry.Info.dMinLon < oViewedArea.West) oExportEntry.Info.dMinLon = oViewedArea.West;
									if (oExportEntry.Info.dMaxLon > oViewedArea.East) oExportEntry.Info.dMaxLon = oViewedArea.East;
									if (oExportEntry.Info.dMinLat < oViewedArea.South) oExportEntry.Info.dMinLat = oViewedArea.South;
									if (oExportEntry.Info.dMaxLat > oViewedArea.North) oExportEntry.Info.dMaxLat = oViewedArea.North;

									// Re-scale pixels
									oExportEntry.Info.iPixelsX = (int)((oExportEntry.Info.dMaxLon - oExportEntry.Info.dMinLon) * dPixelsPerDegree);
									oExportEntry.Info.iPixelsY = (int)((oExportEntry.Info.dMaxLat - oExportEntry.Info.dMinLat) * dPixelsPerDegree);

									// Cancel if the layer doesn't intersect the viewed area
									if (oExportEntry.Info.iPixelsX < 0 || oExportEntry.Info.iPixelsY < 0) continue;

									try
									{
										using (Bitmap oLayerImage = new Bitmap(oExportEntry.Info.iPixelsX, oExportEntry.Info.iPixelsY))
										{
											int iOffsetX, iOffsetY;
											int iWidth, iHeight;

											using (oExportEntry.Info.gr = Graphics.FromImage(oLayerImage))
												oExportEntry.RO.ExportProcess(MainForm.WorldWindowSingleton.DrawArgs, oExportEntry.Info);

											iOffsetX = (int)Math.Round((oExportEntry.Info.dMinLon - oViewedArea.West) * (double)iExportPixelsX / (oViewedArea.East - oViewedArea.West));
											iOffsetY = (int)Math.Round((oViewedArea.North - oExportEntry.Info.dMaxLat) * (double)iExportPixelsY / (oViewedArea.North - oViewedArea.South));
											iWidth = oExportEntry.Info.iPixelsX;
											iHeight = oExportEntry.Info.iPixelsY;

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
										}
									}
									catch (System.ArgumentException)
									{
										String szLayers = String.Empty;
										foreach (LayerBuilder oBuilder in this.AllLayers)
										{
											szLayers += "\"" + oBuilder.Title + "\"" + ", ";
										}
										throw new ArgumentException(String.Format("Error creating layer image for snapshot. Width[{0}] Height[{1}] Bounds[{2}] Layers[{3}]", oExportEntry.Info.iPixelsX, oExportEntry.Info.iPixelsY, oViewedArea.ToString(), szLayers));
									}
								}
							}
							SaveGeoImage(oExportedImage, szFilename, szGeoTiff);
						}
					}
					catch (System.ArgumentException)
					{
						String szLayers = String.Empty;
						foreach (LayerBuilder oBuilder in this.AllLayers)
						{
							szLayers += "\"" + oBuilder.Title + "\"" + ", ";
						}
						throw new ArgumentException(String.Format("Error creating layer image for snapshot. Width[{0}] Height[{1}] Bounds[{2}] Layers[{3}]", iExportPixelsX, iExportPixelsY, oViewedArea.ToString(), szLayers));
					}

					Program.ShowMessageBox(
						"GeoTIFF snapshot created.",
						"Create GeoTIFF Snapshot",
						MessageBoxButtons.OK,
						MessageBoxDefaultButton.Button1,
						MessageBoxIcon.Information);
            }
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
      private void SaveGeoImage(Bitmap oBitmap, string szResultFilename, string szGeotiff)
      {
			if (!File.Exists(szGeotiff)) throw new ArgumentException("Could not find tiff to georefrerence");

			ImageFormat eFormat = ImageFormat.Tiff;

			String szExtension = Path.GetExtension(szResultFilename);

         if (szExtension.Equals("bmp", StringComparison.InvariantCultureIgnoreCase)) eFormat = ImageFormat.Bmp;
			else if (szExtension.Equals("png", StringComparison.InvariantCultureIgnoreCase)) eFormat = ImageFormat.Png;
			else if (szExtension.Equals("gif", StringComparison.InvariantCultureIgnoreCase)) eFormat = ImageFormat.Gif;


         if (eFormat == ImageFormat.Tiff)
         {
            string szTempImageFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + szExtension);
            oBitmap.Save(szTempImageFile, eFormat);

            ProcessStartInfo psi = new ProcessStartInfo(Path.GetDirectoryName(Application.ExecutablePath) + @"\System\geotifcp.exe");
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            psi.Arguments = "-g \"" + szGeotiff + "\" \"" + szTempImageFile + "\" \"" + szResultFilename + "\"";

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
            oBitmap.Save(szResultFilename, eFormat);
         }
      }

      #endregion

      #region Testing

      /// <summary>
      /// Raise an exception if the layers in the internal layer list don't match up with the UI list.
      /// </summary>
      private void CheckIsValid()
      {
         if (m_oLayers.Count != c_lvLayers.Items.Count)
            throw new ArgumentException("Data no longer syncs");

         for (int count = 0; count < m_oLayers.Count; count++)
         {
            if (!m_oLayers[count].Title.Equals(c_lvLayers.Items[count].Text))
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
