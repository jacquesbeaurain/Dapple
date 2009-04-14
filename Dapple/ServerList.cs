using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using Geosoft.GX.DAPGetData;
using Dapple.LayerGeneration;
using WorldWind;
using NewServerTree;

namespace Dapple
{
	internal partial class ServerList : UserControl
	{
		#region Constants

		private const int LAYERS_PER_PAGE = 10;
		private const int MAX_DAP_RESULTS = 1000;

		#endregion


		#region Member variables

		private DappleModel m_oModel;

		private String m_strSearchString;
		private GeographicBoundingBox m_oSearchBox;

		private int m_iCurrPage;
		private int m_iNumPages;
		private List<LayerBuilder> m_oCurrServerLayers;

		private Point m_oDragDropStartPoint;

		private LayerList m_hLayerList;

		internal event Dapple.MainForm.ViewMetadataHandler ViewMetadata;
		internal event EventHandler LayerSelectionChanged;

		#endregion


		#region Constructors

		/// <summary>
		/// Constructor.
		/// </summary>
		internal ServerList()
		{
			InitializeComponent();

			c_lvLayers.SmallImageList = Dapple.MainForm.DataTypeImageList;
			c_lvLayers.LargeImageList = Dapple.MainForm.DataTypeImageList;

			c_oPageNavigator.PageBack += BackPage;
			c_oPageNavigator.PageForward += ForwardPage;

			m_strSearchString = String.Empty;
			m_oSearchBox = null;

			m_oDragDropStartPoint = Point.Empty;

			SetNoServer();
		}

		#endregion


		#region Properties

		/// <summary>
		/// Whether any of the layers in the layer list are selected.
		/// </summary>
		internal Boolean HasLayersSelected
		{
			get
			{
				return c_lvLayers.SelectedIndices.Count > 0;
			}
		}

		/// <summary>
		/// A List of the selected layers.
		/// </summary>
		internal List<LayerBuilder> SelectedLayers
		{
			get
			{
				List<LayerBuilder> result = new List<LayerBuilder>();

				foreach (int index in c_lvLayers.SelectedIndices)
				{
					result.Add(m_oCurrServerLayers[index + m_iCurrPage * LAYERS_PER_PAGE]);
				}

				return result;
			}
		}

		/// <summary>
		/// The layer list that this will add to on an 'add' action.
		/// </summary>
		internal LayerList LayerList
		{
			set
			{
				if (m_hLayerList != null)
				{
					m_hLayerList.ActiveLayersChanged -= new LayerList.ActiveLayersChangedHandler(UpdateActiveLayers);
				}

				m_hLayerList = value;
				m_hLayerList.ActiveLayersChanged += new LayerList.ActiveLayersChangedHandler(UpdateActiveLayers);
				UpdateActiveLayers();
			}
		}

		#endregion


		#region Public Methods

		/// <summary>
		/// Sets the search criteria for the search filter.  Performs a search if the values sent are different
		/// from the values that are currently posessed.
		/// </summary>
		/// <param name="strKeywords">The keywords to search for.</param>
		/// <param name="oBounds">The bounding box to search for.</param>
		internal void setSearchCriteria(String strKeywords, GeographicBoundingBox oBounds)
		{
			bool blBoundsEqual = false;
			if (oBounds == null && m_oSearchBox == null) blBoundsEqual = true;
			if (oBounds != null && m_oSearchBox != null && oBounds.Equals(m_oSearchBox)) blBoundsEqual = true;

			if (!strKeywords.Equals(m_strSearchString) || !blBoundsEqual)
			{
				m_strSearchString = strKeywords;
				m_oSearchBox = oBounds;
				if (c_cbServers.SelectedIndex != -1)
				{
					BeginInitLayerList();
				}
			}
		}

		/// <summary>
		/// Makes the text color of those layers that have been added to the layer list green.
		/// </summary>
		internal void UpdateActiveLayers()
		{
			c_lvLayers.SuspendLayout();

			if (m_oCurrServerLayers != null)
			{
				for (int count = m_iCurrPage * LAYERS_PER_PAGE; count < m_iCurrPage * LAYERS_PER_PAGE + LAYERS_PER_PAGE; count++)
				{
					if (count < m_oCurrServerLayers.Count && count % LAYERS_PER_PAGE < c_lvLayers.Items.Count)
					{
						if (m_hLayerList.AllLayers.Contains(m_oCurrServerLayers[count]))
						{
							c_lvLayers.Items[count % LAYERS_PER_PAGE].ForeColor = Color.ForestGreen;
						}
						else
						{
							c_lvLayers.Items[count % LAYERS_PER_PAGE].ForeColor = c_lvLayers.ForeColor;
						}
					}
				}
			}

			c_lvLayers.ResumeLayout();
		}

		/// <summary>
		/// Clear the current search and execute it again.
		/// </summary>
		internal void ReSearch()
		{
			if (c_cbServers.SelectedIndex == -1)
			{
				SetNoServer();
			}
			else
			{
				SetSearching();
				BeginInitLayerList();
			}
		}

		#endregion


		#region Event handlers

		#region Model Sourced

		void m_oModel_SelectedNodeChanged(object sender, EventArgs e)
		{
			ChangeSelectedServer(c_cbServers.SelectedItem as ServerModelNode, m_oModel.SelectedServer);
		}

		private bool ChangeSelectedServer(ServerModelNode from, ServerModelNode to)
		{
			if (from != to)
			{
				if (to == null)
				{
					c_cbServers.SelectedIndex = -1;
				}
				else
				{
					c_cbServers.SelectedItem = to;
				}
				return true;
			}

			return false;
		}

		private void RebuildAndResearchIfNecessary()
		{
			ServerModelNode oSelectedServer = c_cbServers.SelectedItem as ServerModelNode;
			MuteView();
			FillServerList();
			if (ChangeSelectedServer(oSelectedServer, m_oModel.SelectedServer))
			{
				c_cbServers_SelectedIndexChanged(this, EventArgs.Empty);
			}
			UnmuteView();
		}

		void m_oModel_SearchFilterChanged(object sender, EventArgs e)
		{
			c_cbServers_SelectedIndexChanged(this, EventArgs.Empty);
		}

		void m_oModel_ServerAdded(object sender, EventArgs e)
		{
			RebuildAndResearchIfNecessary();
		}

		void m_oModel_Loaded(object sender, EventArgs e)
		{
			RebuildAndResearchIfNecessary();
		}

		void m_oModel_NodeDisplayUpdated(object sender, NodeDisplayUpdatedEventArgs e)
		{
			c_cbServers.Invalidate();
		}

		void m_oModel_NodeLoaded(object sender, NodeLoadEventArgs e)
		{
			MethodInvoker methodBody = delegate()
			{
				if (e.Node == c_cbServers.SelectedItem)
				{
					SetSearching();
					BeginInitLayerList();
				}
			};

			if (IsHandleCreated)
				Invoke(methodBody);
		}

		#region Attach and Unattach

		internal void Attach(DappleModel oModel)
		{
			if (m_oModel != null) MuteModel();
			m_oModel = oModel;
			if (m_oModel != null) UnmuteModel();
		}

		private void UnmuteModel()
		{
			m_oModel.SearchFilterChanged += new EventHandler(m_oModel_SearchFilterChanged);
			m_oModel.SelectedNodeChanged += new EventHandler(m_oModel_SelectedNodeChanged);
			m_oModel.ServerAdded += new EventHandler(m_oModel_ServerAdded);
			m_oModel.Loaded += new EventHandler(m_oModel_Loaded);
			m_oModel.NodeDisplayUpdated += new EventHandler<NodeDisplayUpdatedEventArgs>(m_oModel_NodeDisplayUpdated);
			m_oModel.NodeLoaded += new EventHandler<NodeLoadEventArgs>(m_oModel_NodeLoaded);
		}

		private void MuteModel()
		{
			m_oModel.SearchFilterChanged -= new EventHandler(m_oModel_SearchFilterChanged);
			m_oModel.SelectedNodeChanged -= new EventHandler(m_oModel_SelectedNodeChanged);
			m_oModel.ServerAdded -= new EventHandler(m_oModel_ServerAdded);
			m_oModel.Loaded -= new EventHandler(m_oModel_Loaded);
			m_oModel.NodeDisplayUpdated -= new EventHandler<NodeDisplayUpdatedEventArgs>(m_oModel_NodeDisplayUpdated);
			m_oModel.NodeLoaded -= new EventHandler<NodeLoadEventArgs>(m_oModel_NodeLoaded);
		}

		#endregion

		#endregion

		private void UnmuteView()
		{
			c_cbServers.SelectedIndexChanged += new EventHandler(c_cbServers_SelectedIndexChanged);
		}

		private void MuteView()
		{
			c_cbServers.SelectedIndexChanged -= new EventHandler(c_cbServers_SelectedIndexChanged);
		}

		/// <summary>
		/// Event handler for picking a server from the server drop-down box.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void c_cbServers_SelectedIndexChanged(object sender, EventArgs e)
		{
			MuteModel();

			if (c_cbServers.SelectedIndex == -1)
			{
				SetNoServer();
			}
			else
			{
				m_oModel.SelectedNode = c_cbServers.SelectedItem as ModelNode;
				SetSearching();
				BeginInitLayerList();
			}

			UnmuteModel();
		}

		private void c_cbServers_DrawItem(object sender, DrawItemEventArgs e)
		{
			e.DrawBackground();

			if (e.Index >= 0)
			{
				e.Graphics.DrawImage(IconKeys.ImageList.Images[(c_cbServers.Items[e.Index] as ServerModelNode).ServerTypeIconKey], new Rectangle(e.Bounds.X, e.Bounds.Y, e.Bounds.Height, e.Bounds.Height));
				e.Graphics.DrawString((c_cbServers.Items[e.Index] as ServerModelNode).DisplayText, e.Font, Brushes.Black, new PointF(e.Bounds.X + e.Bounds.Height, e.Bounds.Y));
			}
			else
			{
				e.Graphics.DrawString("Select a server", e.Font, Brushes.Black, e.Bounds.Location);
			}
		}

		private void cLayersListView_DoubleClick(object sender, EventArgs e)
		{
			if (m_hLayerList != null)
			{
				m_hLayerList.AddLayers(this.SelectedLayers);
			}
		}

		/// <summary>
		/// User mouses down on the layer list view.  They may be wanting to start a drag & drop.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void cLayersListView_MouseDown(object sender, MouseEventArgs e)
		{
			m_oDragDropStartPoint = e.Location;
		}

		/// <summary>
		/// User moves the mouse.  If the drag flag is set, initiate the drag & drop.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void cLayersListView_MouseMove(object sender, MouseEventArgs e)
		{
			if (m_oDragDropStartPoint != Point.Empty && e.Location.Equals(m_oDragDropStartPoint)) return; // The mouse didn't really move

			if (m_oDragDropStartPoint != Point.Empty && HasLayersSelected && (e.Button & MouseButtons.Left) == MouseButtons.Left)
			{
				DoDragDrop(SelectedLayers, DragDropEffects.Copy);
				m_oDragDropStartPoint = Point.Empty;
			}
		}

		/// <summary>
		/// The layer list has resized.  Resize the (invisible) column.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void cLayersListView_Resize(object sender, EventArgs e)
		{
			ResizeColumn();
		}

		/// <summary>
		/// The control loads.  Set the size of the (invisible) layer list column.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ServerList_Load(object sender, EventArgs e)
		{
			ResizeColumn();
		}

		/// <summary>
		/// Cancel the context menu if no layers are selected.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void cLayerContextMenu_Opening(object sender, CancelEventArgs e)
		{
			if (c_lvLayers.SelectedIndices.Count == 0) e.Cancel = true;

			c_miViewLegend.Enabled = (c_lvLayers.SelectedIndices.Count == 1 && m_oCurrServerLayers[c_lvLayers.SelectedIndices[0]].SupportsLegend);
		}

		/// <summary>
		/// Add the selected layers to the visible layers set.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void addToLayersToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (m_hLayerList != null)
			{
				m_hLayerList.AddLayers(this.SelectedLayers);
			}
		}

		/// <summary>
		/// View the legend for the selected layer.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void viewLegendToolStripMenuItem_Click(object sender, EventArgs e)
		{
			LayerBuilder oBuilder = m_oCurrServerLayers[c_lvLayers.SelectedIndices[0]];

			string[] aLegends = oBuilder.GetLegendURLs();
			foreach (string szLegend in aLegends)
			{
				if (!String.IsNullOrEmpty(szLegend)) MainForm.BrowseTo(szLegend);
			}
		}

		/// <summary>
		/// View the metadata if there is one layer selected.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void cLayersListView_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (c_lvLayers.SelectedIndices.Count == 1)
			{
				if (ViewMetadata != null) ViewMetadata(m_oCurrServerLayers[c_lvLayers.SelectedIndices[0] + LAYERS_PER_PAGE * m_iCurrPage]);
			}
			else
			{
				if (ViewMetadata != null) ViewMetadata(null);
			}

			if (LayerSelectionChanged != null) LayerSelectionChanged(this, new EventArgs());
		}

		#endregion


		#region Private helper methods

		/// <summary>
		/// User presses the "back a page" button.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void BackPage(object sender, EventArgs e)
		{
			m_iCurrPage--;
			DrawCurrentPage();
		}

		/// <summary>
		/// User presses the "forward a page" button.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ForwardPage(object sender, EventArgs e)
		{
			m_iCurrPage++;
			DrawCurrentPage();
		}

		#region Enabling/disabling controls

		/// <summary>
		/// Call to deselect servers.
		/// </summary>
		private void SetNoServer()
		{
			c_cbServers.SelectedIndex = -1;
			c_lvLayers.Items.Clear();
			c_oPageNavigator.SetState(String.Empty);
			m_oCurrServerLayers = new List<LayerBuilder>();
		}

		/// <summary>
		/// Call when a search is underway.
		/// </summary>
		private void SetSearching()
		{
			c_lvLayers.Items.Clear();
			c_oPageNavigator.SetState("Searching...");
			m_oCurrServerLayers = new List<LayerBuilder>();

			Refresh();
		}

		/// <summary>
		/// Resets the layer list and the navigation buttons and the label.
		/// </summary>
		private void DrawCurrentPage()
		{
			FillLayerList();
			UpdatePageNavigation();
		}

		/// <summary>
		/// Call to update the forward, back buttons, page label, and layers in list when the current page changes.
		/// </summary>
		private void UpdatePageNavigation()
		{
			if (m_oCurrServerLayers.Count > 0)
			{
				c_oPageNavigator.SetState(m_iCurrPage, m_oCurrServerLayers.Count);
			}
			else
			{
				c_oPageNavigator.SetState("No results");
			}
		}

		/// <summary>
		/// Call to fill in the layer list.  Takes into account the current page.
		/// </summary>
		private void FillLayerList()
		{
			c_lvLayers.SuspendLayout();
			c_lvLayers.Items.Clear();

			if (m_oCurrServerLayers != null)
			{
				for (int count = m_iCurrPage * LAYERS_PER_PAGE; count < m_iCurrPage * LAYERS_PER_PAGE + LAYERS_PER_PAGE; count++)
				{
					if (count < m_oCurrServerLayers.Count) c_lvLayers.Items.Add(m_oCurrServerLayers[count].Title, m_oCurrServerLayers[count].DisplayIconKey);
				}
			}

			c_lvLayers.ResumeLayout();

			UpdateActiveLayers();
		}

		/// <summary>
		/// Sets the width of the (invisible) layer list column equal to the width of the layer list.
		/// </summary>
		private void ResizeColumn()
		{
			c_lvLayers.Columns[0].Width = c_lvLayers.ClientSize.Width;
		}

		/// <summary>
		/// Call to fill in the server list.
		/// </summary>
		private void FillServerList()
		{
			c_cbServers.SuspendLayout();
			c_cbServers.Items.Clear();

			foreach (ServerModelNode oServer in m_oModel.ListableServers)
			{
				c_cbServers.Items.Add(oServer);
			}

			c_cbServers.ResumeLayout();
		}

		#endregion

		#region Async code

		private Object m_oAsyncLock = new object();
		private int m_iAsyncCookie = 0;

		/// <summary>
		/// Gets the list of layers from the server (according to the search criteria) and populates the
		/// local list of layers, and resets the currently viewed page to the first page.
		/// Finally, updates the UI to show the first page.
		/// </summary>
		private void BeginInitLayerList()
		{
			ServerModelNode oCurrServer = c_cbServers.SelectedItem as ServerModelNode;
#pragma warning disable 0618
			GetBuildersDelegate oDelegate = new GetBuildersDelegate(oCurrServer.GetBuilders);
#pragma warning restore 0618
			lock (m_oAsyncLock)
			{
				oDelegate.BeginInvoke(new AsyncCallback(EndInitLayerList), new InitLayerListDatapack(++m_iAsyncCookie, oDelegate));
			}
		}

		private void EndInitLayerList(IAsyncResult result)
		{
			try
			{
				InitLayerListDatapack pack = result.AsyncState as InitLayerListDatapack;

				lock (m_oAsyncLock)
				{
					if (pack.cookie != m_iAsyncCookie)
					{
						return;
					}
				}

				m_oCurrServerLayers = pack.caller.EndInvoke(result);

				m_iCurrPage = 0;
				m_iNumPages = m_oCurrServerLayers.Count / LAYERS_PER_PAGE;
				if (m_oCurrServerLayers.Count % LAYERS_PER_PAGE != 0) m_iNumPages++;
				Invoke(new MethodInvoker(DrawCurrentPage));
			}
			catch (Exception)
			{
				m_oCurrServerLayers = null;
				m_iCurrPage = 0;
				m_iNumPages = -1;
				c_oPageNavigator.SetState("Error occurred");
				FillLayerList();
			}
		}

		private class InitLayerListDatapack
		{
			internal int cookie;
			internal GetBuildersDelegate caller;

			internal InitLayerListDatapack(int c, GetBuildersDelegate m)
			{
				cookie = c;
				caller = m;
			}
		}

		private delegate List<LayerBuilder> GetBuildersDelegate();

		#endregion

		#endregion
	}
}
