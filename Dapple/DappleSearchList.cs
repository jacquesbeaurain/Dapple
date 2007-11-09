using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using WorldWind;
using System.Xml;
using System.Net;
using System.Threading;
using System.Drawing.Drawing2D;
using Dapple.LayerGeneration;
using System.Web;

namespace Dapple.CustomControls
{
   public partial class DappleSearchList : UserControl
   {
      #region Enums

      private enum DisplayMode
      {
         Thumbnail = 0,
         List = 1
      }

      #endregion

      #region Statics

      private const int THUMBNAIL_SIZE = 50;
      private const int BAR_WIDTH = 5;

      #endregion

      #region Member variables

      private GeographicBoundingBox m_oSearchBoundingBox = new GeographicBoundingBox(90, -90, -180, 180);
      private String m_szSearchString = String.Empty;

      private SearchResultSet[] m_aPages;
      private int m_iCurrentPage;
      private int m_iAccessedPages;
      private DisplayMode m_eDisplayMode = DisplayMode.Thumbnail;
      private Point m_oDragDropStartPoint = Point.Empty;
      private LayerList m_hLayerList;

      private String m_szDappleSearchURI = null;

      #endregion

      #region Constructors

      public DappleSearchList() : this(String.Empty)
      {
      }

      public DappleSearchList(String szDappleSearchURI)
      {
         InitializeComponent();
         m_szDappleSearchURI = szDappleSearchURI;
         cTabToolbar.SetImage(0, Dapple.Properties.Resources.tab_thumbnail);
         cTabToolbar.SetImage(1, Dapple.Properties.Resources.tab_list);
         cTabToolbar.SetToolTip(0, "Search results with thumbnails");
         cTabToolbar.SetToolTip(1, "Search results without thumbnails");
         cTabToolbar.ButtonPressed += new TabToolStrip.TabToolbarButtonDelegate(DisplayModeChanged);
         SetNoSearch();
         cNavigator.PageBack += new ThreadStart(BackPage);
         cNavigator.PageForward += new ThreadStart(ForwardPage);
      }

      #endregion

      #region Properties

      public LayerList LayerList
      {
         set
         {
            m_hLayerList = value;
         }
      }

      public String DappleSearchURI
      {
         set
         {
            m_szDappleSearchURI = value;
         }
      }

      public bool HasLayersSelected
      {
         get
         {
            return cResultListBox.SelectedIndices.Count > 0;
         }
      }

      public List<LayerUri> SelectedLayers
      {
         get
         {
            List<LayerUri> result = new List<LayerUri>();

            foreach (int index in cResultListBox.SelectedIndices)
            {
               result.Add(m_aPages[m_iCurrentPage].Results[index].Uri);
            }

            return result;
         }
      }

      #endregion

      #region Event handlers

      private void cResultListBox_DrawItem(object sender, DrawItemEventArgs e)
      {
         if (e.Index == -1) return;

         SearchResult oResult = cResultListBox.Items[e.Index] as SearchResult;
         e.DrawBackground();

         Matrix oOrigTransform = e.Graphics.Transform;
         e.Graphics.TranslateTransform(e.Bounds.X, e.Bounds.Y);

         if (m_eDisplayMode == DisplayMode.Thumbnail)
         {
            // --- Rating bar ---

            e.Graphics.DrawRectangle(new Pen(Color.Black), new Rectangle(0, 0, BAR_WIDTH + 1, THUMBNAIL_SIZE + 1));

            double dPercentageRank = (double)oResult.Rank / (double)UInt16.MaxValue;
            int iBarHeight = (int)(THUMBNAIL_SIZE * dPercentageRank);
            int iStartPoint = THUMBNAIL_SIZE + 1 - iBarHeight;

            e.Graphics.FillRectangle(Brushes.LightGray, new Rectangle(1, 1, BAR_WIDTH, THUMBNAIL_SIZE));
            e.Graphics.FillRectangle(new LinearGradientBrush(new Point(BAR_WIDTH / 2, THUMBNAIL_SIZE + 2), new Point(BAR_WIDTH / 2 + 1, 0), Color.DarkGreen, Color.Lime), new Rectangle(1, iStartPoint, BAR_WIDTH, iBarHeight));

            // --- Thumbnail ---

            e.Graphics.TranslateTransform(BAR_WIDTH + 1, 0, MatrixOrder.Append);

            e.Graphics.DrawRectangle(new Pen(Brushes.Black), new Rectangle(0, 0, THUMBNAIL_SIZE + 1, THUMBNAIL_SIZE + 1));

            if (oResult.Thumbnail == null)
            {
               e.Graphics.FillRectangle(Brushes.Orange, new Rectangle(1, 1, THUMBNAIL_SIZE, THUMBNAIL_SIZE));
            }
            else
            {
               e.Graphics.DrawImage(oResult.Thumbnail, new Rectangle(1, 1, THUMBNAIL_SIZE, THUMBNAIL_SIZE));
            }

            // --- Title ---

            e.Graphics.TranslateTransform(THUMBNAIL_SIZE + 2, 0, MatrixOrder.Append);

            Font oTitleFont = new Font(cResultListBox.Font, FontStyle.Bold);
            e.Graphics.DrawString(oResult.Title, oTitleFont, Brushes.Black, new PointF(0, (THUMBNAIL_SIZE - cResultListBox.Font.Height) / 2));
         }
         else if (m_eDisplayMode == DisplayMode.List)
         {
            e.Graphics.DrawIcon(Dapple.Properties.Resources.layer, new Rectangle(0, 0, e.Bounds.Height, e.Bounds.Height));
            e.Graphics.TranslateTransform(e.Bounds.Height, 0, MatrixOrder.Append);
            e.Graphics.DrawString(String.Format("({0:P0}) {1}", oResult.PercentageRank, oResult.Title), cResultListBox.Font, Brushes.Black, new PointF(0, 0));
         }

         e.Graphics.Transform = oOrigTransform;
         e.DrawFocusRectangle();
      }

      private void cResultListBox_MeasureItem(object sender, MeasureItemEventArgs e)
      {
         if (m_eDisplayMode == DisplayMode.Thumbnail)
         {
            e.ItemHeight = THUMBNAIL_SIZE + 3;
         }
         else if (m_eDisplayMode == DisplayMode.List)
         {
            // Use height of icon
            e.ItemHeight = 16;
         }
      }

      private void cResultListBox_MouseMove(object sender, MouseEventArgs e)
      {
         if (m_oDragDropStartPoint != Point.Empty && (m_oDragDropStartPoint.X != e.X || m_oDragDropStartPoint.Y != e.Y) && (e.Button & MouseButtons.Left) == MouseButtons.Left)
         {
            m_oDragDropStartPoint = Point.Empty;

            if (cResultListBox.SelectedIndices.Count > 0)
            {
               List<LayerUri> oLayerUris = new List<LayerUri>();

               foreach (int iIndex in cResultListBox.SelectedIndices)
               {
                  oLayerUris.Add(m_aPages[m_iCurrentPage].Results[iIndex].Uri);
               }

               DoDragDrop(oLayerUris, DragDropEffects.Copy);
            }
         }
      }

      private void cResultListBox_MouseDown(object sender, MouseEventArgs e)
      {
         if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
         {
            m_oDragDropStartPoint = new Point(e.X, e.Y);
         }
      }

      private void cResultListBox_MouseDoubleClick(object sender, MouseEventArgs e)
      {
         CmdAddSelected();
      }

      private void cContextMenu_Opening(object sender, CancelEventArgs e)
      {
         if (cResultListBox.SelectedIndices.Count < 1)
         {
            e.Cancel = true;
         }
         else
         {
            addLayerToolStripMenuItem.Text = cResultListBox.SelectedIndices.Count > 1 ? "Add layers" : "Add layer";
         }
      }

      private void addLayerToolStripMenuItem_Click(object sender, EventArgs e)
      {
         CmdAddSelected();
      }

      private void DisplayModeChanged(int iIndex)
      {
         DisplayMode eNewDisplayMode = (DisplayMode)Enum.Parse(typeof(DisplayMode), iIndex.ToString());
         if (eNewDisplayMode != m_eDisplayMode)
         {
            m_eDisplayMode = eNewDisplayMode;
            RefreshResultList();
         }
      }

      #endregion

      private void BackPage()
      {
         m_iCurrentPage--;
         RefreshResultList();
      }

      private void ForwardPage()
      {
         m_iCurrentPage++;
         if (m_iCurrentPage >= m_iAccessedPages)
         {
            m_iAccessedPages++;
            SearchResultSet oPage = SearchResultSet.doSearch(m_szDappleSearchURI, m_szSearchString, m_oSearchBoundingBox, m_iCurrentPage * PageNavigator.ResultsPerPage, PageNavigator.ResultsPerPage);
            ExtendSearch(oPage, m_iCurrentPage);
         }
         RefreshResultList();
      }

      public void SetSearchParameters(String szKeyword, GeographicBoundingBox oBoundingBox)
      {
         m_oSearchBoundingBox = oBoundingBox;
         m_szSearchString = szKeyword;

         if (String.Empty.Equals(szKeyword) && oBoundingBox == null)
         {
            SetNoSearch();
         }
         else
         {
            SearchResultSet oPage1 = SearchResultSet.doSearch(m_szDappleSearchURI, szKeyword, oBoundingBox, 0, PageNavigator.ResultsPerPage);
            InitSearch(oPage1);
            RefreshResultList();
         }
      }

      #region helper functions

      private void CmdAddSelected()
      {
         if (cResultListBox.SelectedIndices.Count > 0 && m_hLayerList != null)
         {
            List<LayerUri> oLayerUris = new List<LayerUri>();

            foreach (int iIndex in cResultListBox.SelectedIndices)
            {
               oLayerUris.Add(m_aPages[m_iCurrentPage].Results[iIndex].Uri);
            }

            m_hLayerList.AddLayers(oLayerUris);
         }
      }

      private void RefreshResultList()
      {
         if (InvokeRequired) throw new InvalidOperationException("Tried to refresh result list off of the event thread");

         if (String.Empty.Equals(m_szSearchString) && m_oSearchBoundingBox == null) return; // Nothing to refresh, no search set.

         cResultListBox.SuspendLayout();
         cResultListBox.Items.Clear();

         if (String.IsNullOrEmpty(m_szDappleSearchURI))
         {
            cNavigator.SetState("DappleSearch not configured");
            return;
         }

         if (m_aPages == null)
         {
            cNavigator.SetState("Error contacting search server");
         }
         else
         {
            if (m_aPages.Length > 0 && m_aPages[m_iCurrentPage] != null)
            {
               foreach (SearchResult oResult in m_aPages[m_iCurrentPage].Results)
               {
                  cResultListBox.Items.Add(oResult);
               }
               cNavigator.SetState(m_iCurrentPage, m_aPages[0].TotalCount);
            }
            else
            {
               cNavigator.SetState("No results");
            }

            if (m_eDisplayMode == DisplayMode.Thumbnail && m_iCurrentPage < m_aPages.Length)
            {
               m_aPages[m_iCurrentPage].QueueThumbnails(cResultListBox, m_szDappleSearchURI);
            }
         }

         cResultListBox.ResumeLayout();
      }

      private void SetNoSearch()
      {
         m_iCurrentPage = 0;
         m_iAccessedPages = 0;
         m_aPages = new SearchResultSet[0];
         cResultListBox.Items.Clear();

         if (String.IsNullOrEmpty(m_szDappleSearchURI))
         {
            cNavigator.SetState("DappleSearch not configured");
         }
         else
         {
            cNavigator.SetState("Press Alt-S to search");
         }
      }

      private delegate void InitResultsDelegate(SearchResultSet oResults);
      private void InitSearch(SearchResultSet oResults)
      {
         if (InvokeRequired)
         {
            this.Invoke(new InitResultsDelegate(InitSearch), new Object[] { oResults });
            return;
         }
         else
         {
            m_iCurrentPage = 0;
            m_iAccessedPages = 1;

            if (oResults != null)
            {
               int iNumPages = PageNavigator.PagesFromResults(oResults.TotalCount);

               m_aPages = new SearchResultSet[iNumPages];

               if (iNumPages > 0)
               {
                  m_aPages[0] = oResults;
               }
            }
            else
            {
               m_aPages = null;
            }
         }
      }

      private delegate void UpdateResultsDelegate(SearchResultSet aResults, int iPage);
      private void ExtendSearch(SearchResultSet oResults, int iPage)
      {
         if (InvokeRequired)
         {
            this.Invoke(new UpdateResultsDelegate(ExtendSearch), new Object[] { oResults, iPage });
            return;
         }
         else
         {
            if (oResults != null)
            {
               m_aPages[iPage] = oResults;
            }
            else
            {
               cNavigator.SetState("DappleSearch not configured");
            }
         }
      }

      #endregion
   }

   class SearchResultSet
   {
      private int m_iOffset;
      private int m_iTotalCount;
      private List<SearchResult> m_aResults;

      private Object LOCK = new Object();
      private bool m_blThumbnailsQueued = false;

      private SearchResultSet(XmlDocument oSearchResult)
      {
         if (oSearchResult.SelectSingleNode("/geosoft_xml/error") != null) throw new ArgumentException("Server sent error message");

         XmlElement oRootElement = oSearchResult.SelectSingleNode("/geosoft_xml/search_result") as XmlElement;

         m_iOffset = Int32.Parse(oRootElement.GetAttribute("offset"));
         m_iTotalCount = Int32.Parse(oRootElement.GetAttribute("totalcount"));
         m_aResults = new List<SearchResult>();

         foreach (XmlNode oResult in oRootElement.SelectNodes("/geosoft_xml/search_result/layers/layer"))
         {
            m_aResults.Add(new SearchResult(oResult as XmlElement));
         }
      }

      public int TotalCount { get { return m_iTotalCount; } }
      public List<SearchResult> Results { get { return m_aResults; } }
      public int Offset { get { return m_iOffset; } }

      #region Asynchronous Thumbnail Loading

      public void QueueThumbnails(ListBox oView, String szDappleSearchURI)
      {
         lock (LOCK)
         {
            if (!m_blThumbnailsQueued)
            {
               m_blThumbnailsQueued = true;

               foreach (SearchResult oResult in m_aResults)
               {
                  ThreadPool.QueueUserWorkItem(new WaitCallback(AsyncLoadThumbnail), new Object[] { oResult, oView, szDappleSearchURI });
               }
            }
         }
      }

      private void AsyncLoadThumbnail(Object oParams)
      {
         SearchResult oResult = ((Object[])oParams)[0] as SearchResult;
         ListBox oView = ((Object[])oParams)[1] as ListBox;
         String szDappleSearchURI = ((Object[])oParams)[2] as String;

         oResult.downloadThumbnail(szDappleSearchURI);
         oView.Invalidate();
      }

      #endregion

      public static SearchResultSet doSearch(String szDappleSearchServerURL, String szSearchString, GeographicBoundingBox oSearchBoundingBox, int iOffset, int iNumResults)
      {
         if (String.IsNullOrEmpty(szDappleSearchServerURL)) return null;

         XmlDocument query = new XmlDocument();
         XmlElement geoRoot = query.CreateElement("geosoft_xml");
         query.AppendChild(geoRoot);
         XmlElement root = query.CreateElement("search_request");
         root.SetAttribute("version", "1.0");
         root.SetAttribute("handle", "cheese");
         root.SetAttribute("maxcount", iNumResults.ToString());
         root.SetAttribute("offset", iOffset.ToString());
         geoRoot.AppendChild(root);

         if (oSearchBoundingBox != null)
         {
            XmlElement boundingBox = query.CreateElement("bounding_box");
            boundingBox.SetAttribute("minx", oSearchBoundingBox.West.ToString());
            boundingBox.SetAttribute("miny", oSearchBoundingBox.South.ToString());
            boundingBox.SetAttribute("maxx", oSearchBoundingBox.East.ToString());
            boundingBox.SetAttribute("maxy", oSearchBoundingBox.North.ToString());
            boundingBox.SetAttribute("crs", "WSG84");
            root.AppendChild(boundingBox);
         }

         if (!String.IsNullOrEmpty(szSearchString))
         {
            XmlElement keyword = query.CreateElement("text_filter");
            keyword.InnerText = szSearchString;
            root.AppendChild(keyword);
         }

         // --- Do the request ---

         WebResponse response = null;
         try
         {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(szDappleSearchServerURL + MainForm.SEARCH_XML_GATEWAY);
            request.Headers["GeosoftMapSearchRequest"] = query.InnerXml;
            response = request.GetResponse();

            XmlDocument responseXML = new XmlDocument();
            responseXML.Load(response.GetResponseStream());

            return new SearchResultSet(responseXML);
         }
         catch
         {
            return null;
         }
         finally
         {
            if (response != null) response.Close();
         }
      }
   }

   class SearchResult
   {
      private Bitmap m_oBitmap;
      private Dictionary<String, String> m_aCommonAttributes = new Dictionary<string, string>();
      private Dictionary<String, String> m_aTypeSpecificAttributes = new Dictionary<string, string>();

      public SearchResult(XmlElement oLayerElement)
      {

         XmlElement oCommonElement = oLayerElement.SelectSingleNode("common") as XmlElement;
         foreach (XmlAttribute oAttribute in oCommonElement.Attributes)
         {
            m_aCommonAttributes.Add(oAttribute.Name, oAttribute.Value);
         }
         m_aCommonAttributes.Add("rankingscore", oLayerElement.GetAttribute("rankingscore"));

         XmlElement oTypeElement = oLayerElement.SelectSingleNode(m_aCommonAttributes["type"].ToLower()) as XmlElement;
         foreach (XmlAttribute oAttribute in oTypeElement.Attributes)
         {
            m_aTypeSpecificAttributes.Add(oAttribute.Name, oAttribute.Value);
         }
      }

      public void downloadThumbnail(String szDappleSearchURI)
      {
         if (String.IsNullOrEmpty(szDappleSearchURI)) return;

         WebRequest oRequest = WebRequest.Create(szDappleSearchURI + "Thumbnail.aspx?layerid=" + m_aCommonAttributes["obaselayerid"]);
         WebResponse oResponse = null;
         try
         {
            oResponse = oRequest.GetResponse();
            m_oBitmap = new Bitmap(oResponse.GetResponseStream());
         }
         finally
         {
            if (oResponse != null) oResponse.Close();
         }
      }

      public String Title { get { return m_aCommonAttributes["layertitle"]; } }
      public UInt16 Rank { get { return UInt16.Parse(m_aCommonAttributes["rankingscore"]); } }
      public double PercentageRank { get { return (double)Rank / (double)UInt16.MaxValue; } }
      public Bitmap Thumbnail { get { return m_oBitmap; } }
      public String Description { get { return m_aCommonAttributes["description"]; } }

      public LayerUri Uri
      {
         get
         {
            String szType = m_aCommonAttributes["type"];

            if (szType.Equals("dap", StringComparison.InvariantCultureIgnoreCase))
            {
               String szUri = "gxdap://" + m_aCommonAttributes["url"];
               if (!szUri.Contains("?")) szUri += "?";
               szUri += "&datasetname=" + HttpUtility.UrlEncode(m_aTypeSpecificAttributes["datasetname"]);
               szUri += "&height=" + HttpUtility.UrlEncode(m_aTypeSpecificAttributes["height"]);
               szUri += "&size=" + HttpUtility.UrlEncode(m_aTypeSpecificAttributes["size"]);
               szUri += "&type=" + HttpUtility.UrlEncode(m_aTypeSpecificAttributes["type"]);
               szUri += "&title=" + HttpUtility.UrlEncode(m_aCommonAttributes["layertitle"]);
               szUri += "&edition=" + HttpUtility.UrlEncode(m_aTypeSpecificAttributes["edition"]);
               szUri += "&hierarchy=" + HttpUtility.UrlEncode(m_aTypeSpecificAttributes["hierarchy"]);
               szUri += "&levels=" + HttpUtility.UrlEncode(m_aTypeSpecificAttributes["levels"]);
               szUri += "&lvl0tilesize=" + HttpUtility.UrlEncode(m_aTypeSpecificAttributes["lvlzerotilesize"]);
               szUri += "&north=" + HttpUtility.UrlEncode(m_aCommonAttributes["maxy"]);
               szUri += "&east=" + HttpUtility.UrlEncode(m_aCommonAttributes["maxx"]);
               szUri += "&south=" + HttpUtility.UrlEncode(m_aCommonAttributes["miny"]);
               szUri += "&west=" + HttpUtility.UrlEncode(m_aCommonAttributes["minx"]);

               DapLayerUri oUri = new DapLayerUri(szUri);

               return oUri;
            }
            else if (szType.Equals("wms", StringComparison.InvariantCultureIgnoreCase))
            {
               String szUri = "gxwms://" + m_aCommonAttributes["url"];
               if (!szUri.Contains("?")) szUri += "?";
               //szUri += "&version=" + HttpUtility.UrlEncode(m_aTypeSpecificAttributes["serverversion"]);
               szUri += "&layer=" + HttpUtility.UrlEncode(m_aTypeSpecificAttributes["layername"]);
               szUri += "&title=" + HttpUtility.UrlEncode(m_aCommonAttributes["layertitle"]);
               szUri += "&" + HttpUtility.UrlEncode(m_aTypeSpecificAttributes["cgitokens"]);

               /*if (String.Compare(m_aTypeSpecificAttributes["serverversion"], "1.3.0") == -1)
               {
                  szUri += "&srs=";
               }
               else
               {
                  szUri += "&crs=";
               }

               if (m_aTypeSpecificAttributes.ContainsKey("specialcoord"))
               {
                  szUri += m_aTypeSpecificAttributes["specialcoord"];
               }
               else
               {
                  szUri += "EPSG:4326";
                  szUri += "&bbox=" + String.Format("{0},{1},{2},{3}", m_aCommonAttributes["minx"], m_aCommonAttributes["miny"], m_aCommonAttributes["maxx"], m_aCommonAttributes["maxy"]);
               }*/

               WMSLayerUri oUri = new WMSLayerUri(szUri);

               return oUri;
            }
            else if (szType.Equals("arcims", StringComparison.InvariantCultureIgnoreCase))
            {
               String szUri = "gxarcims://" + m_aCommonAttributes["url"];
               if (!szUri.Contains("?")) szUri += "?";
               szUri += "&servicename=" + HttpUtility.UrlEncode(m_aTypeSpecificAttributes["servicename"]);
               szUri += "&layerid=" + HttpUtility.UrlEncode(m_aTypeSpecificAttributes["layerid"]);
               szUri += "&title=" + HttpUtility.UrlEncode(m_aCommonAttributes["layertitle"]);
               szUri += "&minx=" + HttpUtility.UrlEncode(m_aCommonAttributes["minx"]);
               szUri += "&miny=" + HttpUtility.UrlEncode(m_aCommonAttributes["miny"]);
               szUri += "&maxx=" + HttpUtility.UrlEncode(m_aCommonAttributes["maxx"]);
               szUri += "&maxy=" + HttpUtility.UrlEncode(m_aCommonAttributes["maxy"]);
               szUri += "&minscale=" + HttpUtility.UrlEncode(m_aTypeSpecificAttributes["minscale"]);
               szUri += "&maxscale=" + HttpUtility.UrlEncode(m_aTypeSpecificAttributes["maxscale"]);

               ArcIMSLayerUri oUri = new ArcIMSLayerUri(szUri);

               return oUri;
            }
            return null;
         }
      }
   }
}
