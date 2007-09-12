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

namespace Dapple
{
   public partial class ServerList : UserControl
   {
      #region Constants

      private const int LAYERS_PER_PAGE = 10;
      private const int MAX_DAP_RESULTS = 1000;

      #endregion

      #region Member variables

      private String m_strSearchString;
      private GeographicBoundingBox m_oSearchBox;

      private ArrayList m_oServerList;

      private int m_iCurrPage;
      private int m_iNumPages;
      private List<LayerBuilder> m_oCurrServerLayers;

      private bool m_oDragDropReady;

      private Object m_oSelectedServer;

      private LayerList m_hLayerList;

      public event Dapple.MainForm.ViewMetadataHandler ViewMetadata;

      #endregion

      #region Constructors

      /// <summary>
      /// Constructor.
      /// </summary>
      public ServerList()
      {
         InitializeComponent();
         cPageNavigator.PageBack += new System.Threading.ThreadStart(BackPage);
         cPageNavigator.PageForward += new System.Threading.ThreadStart(ForwardPage);

         m_strSearchString = String.Empty;
         m_oSearchBox = null;

         m_oServerList = null;

         m_oSelectedServer = null;
         m_oDragDropReady = false;

         m_oServerList = new ArrayList();
         SetNoServer();
      }

      #endregion

      #region Properties

      /// <summary>
      /// Sets the ImageLists for the layer list.
      /// </summary>
      public ImageList ImageList
      {
         set
         {
            cLayersListView.SmallImageList = value;
            cLayersListView.LargeImageList = value;
         }
      }

      /// <summary>
      /// The list of servers to display in the drop-down list.
      /// </summary>
      public ArrayList Servers
      {
         set
         {
            checkArrayList(value);
            m_oServerList = value;

            if (m_oSelectedServer != null && m_oServerList.Contains(m_oSelectedServer))
            {
               cServersComboBox.SelectedIndex = m_oServerList.IndexOf(m_oSelectedServer);
            }
            else
            {
               SetNoServer();
               FillServerList();
            }
         }
      }

      /// <summary>
      /// Set the selected server
      /// </summary>
      public Object SelectedServer
      {
         get
         {
            return m_oSelectedServer;
         }
         set
         {
            int iSelectedIndex = m_oServerList.IndexOf(value);

            if (iSelectedIndex != cServersComboBox.SelectedIndex)
            {
               cServersComboBox.SelectedIndex = iSelectedIndex;
               cServersComboBox_SelectedIndexChanged(this, new EventArgs());
            }
         }
      }

      /// <summary>
      /// Whether any of the layers in the layer list are selected.
      /// </summary>
      public Boolean HasLayersSelected
      {
         get 
         {
            return cLayersListView.SelectedIndices.Count > 0;
         }
      }

      /// <summary>
      /// A List of the selected layers.
      /// </summary>
      public List<LayerBuilder> SelectedLayers
      {
         get
         {
            List<LayerBuilder> result = new List<LayerBuilder>();

            foreach (int index in cLayersListView.SelectedIndices)
            {
               result.Add(m_oCurrServerLayers[index + m_iCurrPage * LAYERS_PER_PAGE]);
            }

            return result;
         }
      }

      /// <summary>
      /// The layer list that this will add to on an 'add' action.
      /// </summary>
      public LayerList LayerList
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
      public void setSearchCriteria(String strKeywords, GeographicBoundingBox oBounds)
      {
         if (!strKeywords.Equals(m_strSearchString) || !oBounds.Equals(m_oSearchBox))
         {
            m_strSearchString = strKeywords;
            m_oSearchBox = oBounds;
            if (cServersComboBox.SelectedIndex != -1)
            {
               InitLayerList();
               FillLayerList();
               UpdatePageNavigation();
            }
         }
      }

      /// <summary>
      /// Makes the text color of those layers that have been added to the layer list green.
      /// </summary>
      public void UpdateActiveLayers()
      {
         cLayersListView.SuspendLayout();

         if (m_oCurrServerLayers != null)
         {
            for (int count = m_iCurrPage * LAYERS_PER_PAGE; count < m_iCurrPage * LAYERS_PER_PAGE + LAYERS_PER_PAGE; count++)
            {
               if (count < m_oCurrServerLayers.Count)
               {
                  if (m_hLayerList.ContainsLayerBuilder(m_oCurrServerLayers[count]))
                  {
                     cLayersListView.Items[count % LAYERS_PER_PAGE].ForeColor = Color.ForestGreen;
                  }
                  else
                  {
                     cLayersListView.Items[count % LAYERS_PER_PAGE].ForeColor = cLayersListView.ForeColor;
                  }
               }
            }
         }

         cLayersListView.ResumeLayout();
      }

      #endregion

      #region Event handlers

      /// <summary>
      /// Event handler for picking a server from the server drop-down box.
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void cServersComboBox_SelectedIndexChanged(object sender, EventArgs e)
      {
         if (cServersComboBox.SelectedIndex == -1)
         {
            SetNoServer();
            m_oSelectedServer = null;
         }
         else
         {
            Object oNewSelectedServer = m_oServerList[cServersComboBox.SelectedIndex];
            if (!oNewSelectedServer.Equals(m_oSelectedServer))
            {
               SetSearching();
               InitLayerList();
               DrawCurrentPage();
               m_oSelectedServer = oNewSelectedServer;
            }
         }
      }

      /// <summary>
      /// User mouses down on the layer list view.  They may be wanting to start a drag & drop.
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void cLayersListView_MouseDown(object sender, MouseEventArgs e)
      {
         m_oDragDropReady = true;
      }

      /// <summary>
      /// User moves the mouse.  If the drag flag is set, initiate the drag & drop.
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void cLayersListView_MouseMove(object sender, MouseEventArgs e)
      {
         if (m_oDragDropReady && HasLayersSelected && (e.Button & MouseButtons.Left) == MouseButtons.Left)
         {
            DoDragDrop(SelectedLayers, DragDropEffects.Copy);
         }
         m_oDragDropReady = false;
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
         if (cLayersListView.SelectedIndices.Count == 0) e.Cancel = true;
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
      /// View the metadata if there is one layer selected.
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void cLayersListView_SelectedIndexChanged(object sender, EventArgs e)
      {
         if (cLayersListView.SelectedIndices.Count == 1)
         {
            if (ViewMetadata != null) ViewMetadata(m_oCurrServerLayers[cLayersListView.SelectedIndices[0]]);
         }
      }

      #endregion

      #region Private helper methods

      /// <summary>
      /// User presses the "back a page" button.
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void BackPage()
      {
         m_iCurrPage--;
         DrawCurrentPage();
      }

      /// <summary>
      /// User presses the "forward a page" button.
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void ForwardPage()
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
         cServersComboBox.SelectedIndex = -1;
         cLayersListView.Items.Clear();
         cPageNavigator.SetState(String.Empty);

         m_oCurrServerLayers = new List<LayerBuilder>();
      }

      /// <summary>
      /// Call when a search is underway.
      /// </summary>
      private void SetSearching()
      {
         cLayersListView.Items.Clear();
         cPageNavigator.SetState("Searching...");

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
            cPageNavigator.SetState(m_iCurrPage, m_oCurrServerLayers.Count);
         }
         else
         {
            cPageNavigator.SetState("No results");
         }
      }

      /// <summary>
      /// Call to fill in the layer list.  Takes into account the current page.
      /// </summary>
      private void FillLayerList()
      {
         cLayersListView.SuspendLayout();
         cLayersListView.Items.Clear();

         if (m_oCurrServerLayers != null)
         {
            for (int count = m_iCurrPage * LAYERS_PER_PAGE; count < m_iCurrPage * LAYERS_PER_PAGE + LAYERS_PER_PAGE; count++)
            {
               if (count < m_oCurrServerLayers.Count) cLayersListView.Items.Add(getLayerTitle(m_oCurrServerLayers[count]), cLayersListView.SmallImageList.Images.IndexOfKey(m_oCurrServerLayers[count].LayerTypeIconKey));
            }
         }

         cLayersListView.ResumeLayout();

         UpdateActiveLayers();
      }

      /// <summary>
      /// Sets the width of the (invisible) layer list column equal to the width of the layer list.
      /// </summary>
      private void ResizeColumn()
      {
         cLayersListView.Columns[0].Width = cLayersListView.ClientSize.Width;
      }

      /// <summary>
      /// Call to fill in the server list.
      /// </summary>
      private void FillServerList()
      {
         cServersComboBox.SuspendLayout();
         cServersComboBox.Items.Clear();

         foreach (Object obj in m_oServerList)
         {
            cServersComboBox.Items.Add(getServerName(obj));
         }

         cServersComboBox.ResumeLayout();
      }

      #endregion

      #region Smelly code

      /// <summary>
      /// Gets the list of layers from the server (according to the search criteria) and populates the
      /// local list of layers, and resets the currently viewed page to the first page.
      /// </summary>
      private void InitLayerList()
      {
         Object obj = m_oServerList[cServersComboBox.SelectedIndex];

         try
         {
            if (obj is Server)
            {
               ArrayList oDapLayers = new ArrayList();
               m_oCurrServerLayers = new List<LayerBuilder>();
               Geosoft.Dap.Common.BoundingBox oConvertedBox = m_oSearchBox == null ? null : new Geosoft.Dap.Common.BoundingBox(m_oSearchBox.East, m_oSearchBox.North, m_oSearchBox.West, m_oSearchBox.South);
               ((Server)obj).Command.GetCatalog(null, 0, 0, MAX_DAP_RESULTS, m_strSearchString, oConvertedBox, out oDapLayers);

               foreach (Geosoft.Dap.Common.DataSet oDataSet in oDapLayers)
               {
                  m_oCurrServerLayers.Add(new DAPQuadLayerBuilder(oDataSet, MainForm.WorldWindowSingleton, obj as Server, null));
               }
            }
            else if (obj is AsyncServerBuilder)
            {
               m_oCurrServerLayers = new List<LayerBuilder>();
               ((AsyncServerBuilder)obj).getLayerBuilders(ref m_oCurrServerLayers);
            }
            else
               throw new ArgumentException("obj is unknown type " + obj.GetType());

            m_iCurrPage = 0;
            m_iNumPages = m_oCurrServerLayers.Count / LAYERS_PER_PAGE;
            if (m_oCurrServerLayers.Count % LAYERS_PER_PAGE != 0) m_iNumPages++;
         }
         catch (Exception)
         {
            m_oCurrServerLayers = null;
            m_iCurrPage = 0;
            m_iNumPages = -1;
            cPageNavigator.SetState("Error occurred");
         }
      }

      /// <summary>
      /// Get the server name for the Server/ServerBuilder.
      /// </summary>
      /// Breaking polymorphic design principles since August 2007!
      /// <remarks>
      /// </remarks>
      /// <param name="obj">The server to get the name of.</param>
      /// <returns>The server name.</returns>
      private String getServerName(Object obj)
      {
         if (obj is Server)
            return ((Server)obj).Name;
         else if (obj is ServerBuilder)
            return ((ServerBuilder)obj).Name;
         else
            throw new ArgumentException("obj is unknown type " + obj.GetType());
      }

      private Icon getServerIcon(Object obj)
      {
         if (obj is Server)
            return Dapple.Properties.Resources.dap;
         else if (obj is ServerBuilder)
            return ((ServerBuilder)obj).Icon;
         else
            throw new ArgumentException("obj is unknown type " + obj.GetType());
      }

      /// <summary>
      /// Get the layer title for the LayerBuilder/DataSet.
      /// </summary>
      /// <param name="obj">The layer to get the title of.</param>
      /// <returns></returns>
      private String getLayerTitle(Object obj)
      {
         if (obj is Geosoft.Dap.Common.DataSet)
            return ((Geosoft.Dap.Common.DataSet)obj).Title;
         else if (obj is LayerBuilder)
            return ((LayerBuilder)obj).Name;
         else
            throw new ArgumentException("obj is unknown type " + obj.GetType());
      }

      /// <summary>
      /// Get the image of the icon to display in the list for this layer.
      /// </summary>
      /// <param name="obj">The layer to get the icon of.</param>
      /// <returns></returns>
      private int getImageIndex(Object obj)
      {
         if (obj is Geosoft.Dap.Common.DataSet)
            return cLayersListView.SmallImageList.Images.IndexOfKey("dap_" + ((Geosoft.Dap.Common.DataSet)obj).Type.ToLower());
         else if (obj is LayerBuilder)
            return cLayersListView.SmallImageList.Images.IndexOfKey("layer");
         else
            throw new ArgumentException("obj is unknown type " + obj.GetType());
      }
      
      /// <summary>
      /// Throws an ArgumentException if an ArrayList contains something other than ServerBuilders and Servers.
      /// </summary>
      /// <param name="oList"></param>
      private void checkArrayList(ArrayList oList)
      {
         foreach (Object obj in oList)
         {
            if (!(obj is Server || obj is ServerBuilder)) throw new ArgumentException("oServers contains an invalid object (Type " + obj.GetType().ToString() + ")");
         }
      }

      #endregion

      private void cServersComboBox_DrawItem(object sender, DrawItemEventArgs e)
      {
         if (e.Index >= 0)
         {
            e.Graphics.DrawIcon(getServerIcon(m_oServerList[e.Index]), new Rectangle(e.Bounds.X, e.Bounds.Y, e.Bounds.Height, e.Bounds.Height));
            e.Graphics.DrawString(getServerName(m_oServerList[e.Index]), e.Font, Brushes.Black, new PointF(e.Bounds.X + e.Bounds.Height, e.Bounds.Y));
         }
         else
         {
            e.Graphics.DrawString("Select a server", e.Font, Brushes.Black, e.Bounds.Location);
         }
      }

      #endregion
   }
}
