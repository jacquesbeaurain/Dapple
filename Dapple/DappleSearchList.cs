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

namespace Dapple
{
   public partial class DappleSearchList : UserControl
   {
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

      #endregion

      #region Constructors

      public DappleSearchList()
      {
         InitializeComponent();
         SetNoSearch();
         cNavigator.PageBack += new ThreadStart(BackPage);
         cNavigator.PageForward += new ThreadStart(ForwardPage);
      }

      #endregion

      #region Event handlers

      private void cResultListBox_DrawItem(object sender, DrawItemEventArgs e)
      {
         if (e.Index == -1) return;

         SearchResult oResult = cResultListBox.Items[e.Index] as SearchResult;

         e.DrawBackground();
         Matrix oOrigTransform = e.Graphics.Transform;

         // --- Rating bar ---

         e.Graphics.TranslateTransform(e.Bounds.X, e.Bounds.Y);

         e.Graphics.DrawRectangle(new Pen(Color.Black), new Rectangle(0, 0, BAR_WIDTH + 1, THUMBNAIL_SIZE + 1));

         double dPercentageRank = (double)oResult.Rank / (double)UInt16.MaxValue;
         int iBarHeight = (int)(THUMBNAIL_SIZE * dPercentageRank);
         int iStartPoint = THUMBNAIL_SIZE + 1 - iBarHeight;

         e.Graphics.FillRectangle(Brushes.LightGray, new Rectangle(1, 1, BAR_WIDTH, THUMBNAIL_SIZE));
         e.Graphics.FillRectangle(new LinearGradientBrush(new Point(BAR_WIDTH/2, THUMBNAIL_SIZE + 2), new Point(BAR_WIDTH / 2 + 1, 0), Color.DarkGreen, Color.Lime), new Rectangle(1, iStartPoint, BAR_WIDTH, iBarHeight));

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
         e.Graphics.DrawString(oResult.Title, oTitleFont, Brushes.Black, new PointF(0, (THUMBNAIL_SIZE / 2 - cResultListBox.Font.Height) / 2));

         e.Graphics.Transform = oOrigTransform;

         e.DrawFocusRectangle();
      }

      private void cResultListBox_MeasureItem(object sender, MeasureItemEventArgs e)
      {
         e.ItemHeight = THUMBNAIL_SIZE + 2;
      }

      #endregion

      #region PagedList Implementation

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
            SearchResultSet oPage = SearchResultSet.doSearch("http://dapplesearch.geosoft.com/", m_szSearchString, m_oSearchBoundingBox, m_iCurrentPage * PageNavigator.ResultsPerPage, PageNavigator.ResultsPerPage);
            ExtendSearch(oPage, m_iCurrentPage);
         }
         RefreshResultList();
      }

      public void SetSearchParameters(String szKeyword, GeographicBoundingBox oBoundingBox)
      {
         m_oSearchBoundingBox = oBoundingBox;
         m_szSearchString = szKeyword;

         SearchResultSet oPage1 = SearchResultSet.doSearch("http://dapplesearch.geosoft.com/", szKeyword, oBoundingBox, 0, PageNavigator.ResultsPerPage);

         InitSearch(oPage1);

         RefreshResultList();
      }

      #endregion

      #region helper functions

      private void RefreshResultList()
      {
         if (InvokeRequired) throw new InvalidOperationException("Tried to refresh result list off of the event thread");

         cResultListBox.SuspendLayout();
         cResultListBox.Items.Clear();

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

         cResultListBox.ResumeLayout();
      }

      private void SetNoSearch()
      {
         m_iCurrentPage = 0;
         m_iAccessedPages = 0;
         m_aPages = new SearchResultSet[0];

         cNavigator.SetState("Press Alt-S to search");
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

            int iNumPages = PageNavigator.PagesFromResults(oResults.TotalCount);

            m_aPages = new SearchResultSet[iNumPages];

            if (iNumPages > 0)
            {
               m_aPages[0] = oResults;
               QueueThumbnails(oResults);
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
            m_aPages[iPage] = oResults;
            QueueThumbnails(oResults);
         }
      }

      #region Asynchronous Thumbnail Loading

      private void QueueThumbnails(SearchResultSet oResults)
      {
         foreach (SearchResult oResult in oResults.Results)
         {
            ThreadPool.QueueUserWorkItem(new WaitCallback(AsyncLoadThumbnail), new Object[] { oResult });
         }
      }

      private void AsyncLoadThumbnail(Object oParams)
      {
         SearchResult oResult = ((Object[])oParams)[0] as SearchResult;

         oResult.downloadThumbnail();
         cResultListBox.Invalidate();
      }

      #endregion

      #endregion
   }

   class SearchResultSet
   {
      private int m_iOffset;
      private int m_iTotalCount;
      private List<SearchResult> m_aResults;

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

      public static SearchResultSet doSearch(String szDappleSearchServerURL, String szSearchString, GeographicBoundingBox oSearchBoundingBox, int iOffset, int iNumResults)
      {
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
      private String m_szTitle;
      private UInt16 m_uiRank;
      private Bitmap m_oBitmap;
      private String m_szDescription;
      private Int32 m_iID;

      public SearchResult(XmlElement oLayerElement)
      {
         m_uiRank = UInt16.Parse(oLayerElement.GetAttribute("rankingscore"));
         XmlElement oCommonElement = oLayerElement.SelectSingleNode("common") as XmlElement;
         m_szTitle = oCommonElement.GetAttribute("layertitle");
         m_iID = Int32.Parse(oCommonElement.GetAttribute("obaselayerid"));
         m_szDescription = oCommonElement.GetAttribute("description");
      }

      public void downloadThumbnail()
      {
         WebRequest oRequest = WebRequest.Create("http://dapplesearch.geosoft.com/Thumbnail.aspx?layerid=" + m_iID.ToString());
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

      public String Title { get { return m_szTitle; } }
      public UInt16 Rank { get { return m_uiRank; } }
      public Bitmap Thumbnail { get { return m_oBitmap; } }
      public String Description { get { return m_szDescription; } }
   }
}