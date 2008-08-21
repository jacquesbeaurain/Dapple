using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using GED.App.Properties;

namespace GED.App.UI.Controls
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

		#region Events

		public event EventHandler LayerSelectionChanged;

		public class LayerAddArgs : EventArgs
		{
			public List<SearchResult> Layers = new List<SearchResult>();
		}
		public event EventHandler<LayerAddArgs> LayerAddRequested;

		#endregion

		#region Statics

		private const int THUMBNAIL_SIZE = 50;
		private const int BAR_WIDTH = 5;
		private const int ICON_SIZE = 16;

		#endregion

		#region Member variables

		private Geosoft.Dap.Common.BoundingBox m_oSearchBoundingBox = new Geosoft.Dap.Common.BoundingBox(180, 90, -180, -90);
		private String m_strSearchString = String.Empty;

		private SearchResultSet[] m_aPages;
		private int m_iCurrentPage;
		private int m_iAccessedPages;
		private DisplayMode m_eDisplayMode = DisplayMode.Thumbnail;
		private Point m_oDragDropStartPoint = Point.Empty;

		long m_lSearchIndex = 0;

		#endregion

		#region Constructors

		public DappleSearchList()
		{
			InitializeComponent();
			c_tsTabToolstrip.SetImage(0, Resources.tab_thumbnail);
			c_tsTabToolstrip.SetImage(1, Resources.tab_list);
			c_tsTabToolstrip.SetToolTip(0, "Search results with thumbnails");
			c_tsTabToolstrip.SetToolTip(1, "Search results without thumbnails");
			c_tsTabToolstrip.SetNameAndText(0, "ShowThumbnails");
			c_tsTabToolstrip.SetNameAndText(1, "HideThumbnails");
			c_tsTabToolstrip.ButtonPressed += new TabToolStrip.TabToolBarButtonEventHandler(DisplayModeChanged);
			SetNoSearch();
			c_oPageNavigator.PageBack += new ThreadStart(BackPage);
			c_oPageNavigator.PageForward += new ThreadStart(ForwardPage);
		}

		#endregion

		#region Properties

		public bool HasLayersSelected
		{
			get
			{
				return c_lbResults.SelectedIndices.Count > 0;
			}
		}

		#endregion

		#region Event handlers

		private void cResultListBox_DrawItem(object sender, DrawItemEventArgs e)
		{
			if (e.Index == -1) return;

			SearchResult oResult = c_lbResults.Items[e.Index] as SearchResult;
			e.DrawBackground();

			Matrix oOrigTransform = e.Graphics.Transform;
			e.Graphics.TranslateTransform(e.Bounds.X, e.Bounds.Y);

			if (m_eDisplayMode == DisplayMode.Thumbnail)
			{
				// --- Rating bar ---

				e.Graphics.DrawRectangle(Pens.Black, new Rectangle(0, 0, BAR_WIDTH + 1, THUMBNAIL_SIZE + 1));

				double dPercentageRank = (double)oResult.Rank / (double)UInt16.MaxValue;
				int iBarHeight = (int)(THUMBNAIL_SIZE * dPercentageRank);
				int iStartPoint = THUMBNAIL_SIZE + 1 - iBarHeight;

				e.Graphics.FillRectangle(Brushes.LightGray, new Rectangle(1, 1, BAR_WIDTH, THUMBNAIL_SIZE));
				e.Graphics.FillRectangle(new LinearGradientBrush(new Point(BAR_WIDTH / 2, THUMBNAIL_SIZE + 2), new Point(BAR_WIDTH / 2 + 1, 0), Color.DarkGreen, Color.Lime), new Rectangle(1, iStartPoint, BAR_WIDTH, iBarHeight));

				// --- Thumbnail ---

				e.Graphics.TranslateTransform(BAR_WIDTH + 1, 0, MatrixOrder.Append);

				e.Graphics.DrawRectangle(Pens.Black, new Rectangle(0, 0, THUMBNAIL_SIZE + 1, THUMBNAIL_SIZE + 1));

				if (oResult.Thumbnail == null)
				{
					e.Graphics.FillRectangle(Brushes.Orange, new Rectangle(1, 1, THUMBNAIL_SIZE, THUMBNAIL_SIZE));
				}
				else
				{
					e.Graphics.DrawImage(oResult.Thumbnail, new Rectangle(1, 1, THUMBNAIL_SIZE, THUMBNAIL_SIZE));
				}

				// --- Title and URL ---

				e.Graphics.TranslateTransform(THUMBNAIL_SIZE + 2, 0, MatrixOrder.Append);

				Font oTitleFont = new Font(c_lbResults.Font, FontStyle.Bold);
				int iSpace = (THUMBNAIL_SIZE - 2 * c_lbResults.Font.Height) / 3;
				e.Graphics.DrawString(oResult.Title, oTitleFont, Brushes.Black, new PointF(0, iSpace));
				e.Graphics.DrawString(oResult.ServerUrl, new Font(oTitleFont, FontStyle.Regular), Brushes.Black, new PointF(0, iSpace * 2 + c_lbResults.Font.Height));
			}
			else if (m_eDisplayMode == DisplayMode.List)
			{
				e.Graphics.DrawIcon(Resources.layer, new Rectangle(0, 0, e.Bounds.Height, e.Bounds.Height));
				e.Graphics.TranslateTransform(e.Bounds.Height, 0, MatrixOrder.Append);
				e.Graphics.DrawString(String.Format(CultureInfo.CurrentCulture, "({0:P0}) {1}", oResult.PercentageRank, oResult.Title), c_lbResults.Font, Brushes.Black, new PointF(0, 0));
			}

			e.Graphics.Transform = oOrigTransform;
			e.DrawFocusRectangle();
		}

		private void cResultListBox_MeasureItem(object sender, MeasureItemEventArgs e)
		{
			SearchResult oResult = c_lbResults.Items[e.Index] as SearchResult;

			if (m_eDisplayMode == DisplayMode.Thumbnail)
			{
				e.ItemHeight = THUMBNAIL_SIZE + 3;
				e.ItemWidth = BAR_WIDTH + THUMBNAIL_SIZE + 3 + Math.Max(
					(int)e.Graphics.MeasureString(oResult.Title, c_lbResults.Font).Width,
					(int)e.Graphics.MeasureString(oResult.ServerUrl, new Font(c_lbResults.Font, FontStyle.Bold)).Width
					);
				c_lbResults.HorizontalExtent = Math.Max(c_lbResults.HorizontalExtent, e.ItemWidth);
			}
			else if (m_eDisplayMode == DisplayMode.List)
			{
				e.ItemWidth = ICON_SIZE + (int)e.Graphics.MeasureString(String.Format(CultureInfo.CurrentCulture, "({0:P0}) {1}", oResult.PercentageRank, oResult.Title), c_lbResults.Font).Width;
				e.ItemHeight = ICON_SIZE;
				c_lbResults.HorizontalExtent = Math.Max(c_lbResults.HorizontalExtent, e.ItemWidth);

			}
		}

		private void cResultListBox_MouseMove(object sender, MouseEventArgs e)
		{
			/*if (m_oDragDropStartPoint != Point.Empty && (m_oDragDropStartPoint.X != e.X || m_oDragDropStartPoint.Y != e.Y) && (e.Button & MouseButtons.Left) == MouseButtons.Left)
			{
				m_oDragDropStartPoint = Point.Empty;

				if (c_lbResults.SelectedIndices.Count > 0)
				{
					List<LayerUri> oLayerUris = new List<LayerUri>();

					foreach (int iIndex in c_lbResults.SelectedIndices)
					{
						oLayerUris.Add(m_aPages[m_iCurrentPage].Results[iIndex].Uri);
					}

					DoDragDrop(CreateLayerBuilders(oLayerUris), DragDropEffects.Copy);
				}
			}*/
		}

		private void cResultListBox_MouseDown(object sender, MouseEventArgs e)
		{
			/*if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
			{
				m_oDragDropStartPoint = new Point(e.X, e.Y);
			}*/
		}

		private void cResultListBox_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			CmdAddSelected();
		}

		private void c_lbResults_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (LayerSelectionChanged != null)
			{
				LayerSelectionChanged(this, new EventArgs());
			}
		}

		private void cContextMenu_Opening(object sender, CancelEventArgs e)
		{
			if (c_lbResults.SelectedIndices.Count < 1)
			{
				e.Cancel = true;
			}
		}

		private void addLayerToolStripMenuItem_Click(object sender, EventArgs e)
		{
			CmdAddSelected();
		}

		private void DisplayModeChanged(int iIndex)
		{
			DisplayMode eNewDisplayMode = (DisplayMode)Enum.Parse(typeof(DisplayMode), iIndex.ToString(CultureInfo.InvariantCulture));
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
				DappleSearchWebDownload oDownload = new DappleSearchWebDownload(m_oSearchBoundingBox, m_strSearchString, m_iCurrentPage, m_lSearchIndex);
				SetSearching();
				oDownload.StartDownload(new DownloadCompleteHandler(ForwardPageComplete));
			}
			else
			{
				RefreshResultList();
			}
		}
		
		private void ForwardPageComplete(Object sender, DownloadCompleteEventArgs args)
		{
			DappleSearchWebDownload oDSWebDownload = sender as DappleSearchWebDownload;
			// --- If the search indices don't match, it's because a new search was done before this one completed, so discard these results ---
			if (oDSWebDownload.SearchIndex == m_lSearchIndex)
			{
				XmlDocument oDoc = new XmlDocument();
				oDoc.Load(args.Response.GetResponseStream());
				SearchResultSet oPage = new SearchResultSet(oDoc);
				ExtendSearch(oPage, m_iCurrentPage);
			}
		}

		public void SetSearchParameters(String strKeyword, Geosoft.Dap.Common.BoundingBox oBoundingBox)
		{
			m_lSearchIndex++;
			m_oSearchBoundingBox = oBoundingBox;
			m_strSearchString = strKeyword;

			if (String.Empty.Equals(strKeyword) && oBoundingBox == null)
			{
				SetNoSearch();
			}
			else
			{
				DappleSearchWebDownload oDownload = new DappleSearchWebDownload(m_oSearchBoundingBox, m_strSearchString, 0, m_lSearchIndex);
				SetSearching();
				oDownload.StartDownload(new DownloadCompleteHandler(SetSearchParametersComplete));
			}
		}
		
		private void SetSearchParametersComplete(Object sender, DownloadCompleteEventArgs args)
		{

			try
			{
				DappleSearchWebDownload oDSWebDownload = sender as DappleSearchWebDownload;
				if (args.Error != null) throw args.Error;

				// --- If the search indices don't match, it's because a new search was done before this one completed, so discard these results ---
				if (oDSWebDownload.SearchIndex == m_lSearchIndex)
				{
					XmlDocument oDoc = new XmlDocument();
					oDoc.Load(args.Response.GetResponseStream());
					SearchResultSet oPage1 = new SearchResultSet(oDoc);
					InitSearch(oPage1);
				}
			}
			catch (WebException)
			{
				this.Invoke(new MethodInvoker(SetSearchFailed));
			}
			catch (XmlException)
			{
				this.Invoke(new MethodInvoker(SetSearchFailed));
			}
		}

		#region helper functions

		public void CmdAddSelected()
		{
			LayerAddArgs oArgs = new LayerAddArgs();

			if (c_lbResults.SelectedIndices.Count > 0)
			{
				foreach (int iIndex in c_lbResults.SelectedIndices)
				{
					oArgs.Layers.Add(m_aPages[m_iCurrentPage].Results[iIndex]);
				}

				LayerAddRequested(this, oArgs);
			}
		}

		private void RefreshResultList()
		{
			if (InvokeRequired) throw new InvalidOperationException("Tried to refresh result list off of the event thread");

			if (String.Empty.Equals(m_strSearchString) && m_oSearchBoundingBox == null) return; // Nothing to refresh, no search set.

			c_lbResults.SuspendLayout();
			c_lbResults.Items.Clear();
			c_lbResults.HorizontalExtent = 0;

			if (m_aPages == null)
			{
				c_oPageNavigator.SetState("Error contacting search server");
			}
			else
			{
				if (m_aPages.Length > 0 && m_aPages[m_iCurrentPage] != null)
				{
					foreach (SearchResult oResult in m_aPages[m_iCurrentPage].Results)
					{
						c_lbResults.Items.Add(oResult);
					}
					c_oPageNavigator.SetState(m_iCurrentPage, m_aPages[0].TotalCount);
				}
				else
				{
					c_oPageNavigator.SetState("No results");
				}

				if (m_eDisplayMode == DisplayMode.Thumbnail && m_iCurrentPage < m_aPages.Length)
				{
					m_aPages[m_iCurrentPage].QueueThumbnails(c_lbResults);
				}
			}

			c_lbResults.ResumeLayout();
		}

		private void SetNoSearch()
		{
			m_iCurrentPage = 0;
			m_iAccessedPages = 0;
			m_aPages = new SearchResultSet[0];
			c_lbResults.Items.Clear();


			c_oPageNavigator.SetState("Press Alt-S to search");
			c_lbResults.HorizontalExtent = 0;
		}

		private void SetSearching()
		{
			c_lbResults.Items.Clear();
			c_oPageNavigator.SetState("Searching...");
			c_lbResults.HorizontalExtent = 0;
		}

		private void SetSearchFailed()
		{
			m_iCurrentPage = 0;
			m_iAccessedPages = 0;
			m_aPages = new SearchResultSet[0];
			c_lbResults.Items.Clear();
			c_oPageNavigator.SetState("Error contacting DappleSearch server");
			c_lbResults.HorizontalExtent = 0;
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
				RefreshResultList();
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
					c_oPageNavigator.SetState("DappleSearch not configured");
				}
				RefreshResultList();
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

		public SearchResultSet(XmlDocument oSearchResult)
		{
			if (oSearchResult.SelectSingleNode("/geosoft_xml/error") != null) throw new ArgumentException("Server sent error message");

			XmlElement oRootElement = oSearchResult.SelectSingleNode("/geosoft_xml/search_result") as XmlElement;

			m_iOffset = Int32.Parse(oRootElement.GetAttribute("offset"), NumberStyles.Any, CultureInfo.InvariantCulture);
			m_iTotalCount = Int32.Parse(oRootElement.GetAttribute("totalcount"), NumberStyles.Any, CultureInfo.InvariantCulture);
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

		public void QueueThumbnails(ListBox oView)
		{
			lock (LOCK)
			{
				if (!m_blThumbnailsQueued)
				{
					m_blThumbnailsQueued = true;

					foreach (SearchResult oResult in m_aResults)
					{
						ThreadPool.QueueUserWorkItem(new WaitCallback(AsyncLoadThumbnail), new Object[] { oResult, oView });
					}
				}
			}
		}

		private void AsyncLoadThumbnail(Object oParams)
		{
			SearchResult oResult = ((Object[])oParams)[0] as SearchResult;
			ListBox oView = ((Object[])oParams)[1] as ListBox;

			oResult.downloadThumbnail();
			oView.Invalidate();
		}

		#endregion
	}

	public class SearchResult
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

			XmlElement oTypeElement = oLayerElement.SelectSingleNode(m_aCommonAttributes["type"].ToLowerInvariant()) as XmlElement;
			foreach (XmlAttribute oAttribute in oTypeElement.Attributes)
			{
				m_aTypeSpecificAttributes.Add(oAttribute.Name, oAttribute.Value);
			}
		}

		public void downloadThumbnail()
		{
			// --- This non-WebDownload download is permitted because this method is only called by threadpool threads ---
			WebRequest oRequest = WebRequest.Create(Settings.Default.SearchServer + "Thumbnail.aspx?layerid=" + m_aCommonAttributes["obaselayerid"]);
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

		public String GetAttribute(String strKey)
		{
			if (m_aTypeSpecificAttributes.ContainsKey(strKey))
				return m_aTypeSpecificAttributes[strKey];
			else
				throw new ArgumentException("Unknown attribute key \"" + strKey + "\"");
		}

		public String Title { get { return m_aCommonAttributes["layertitle"]; } }
		public UInt16 Rank { get { return UInt16.Parse(m_aCommonAttributes["rankingscore"], CultureInfo.InvariantCulture); } }
		public double PercentageRank { get { return (double)Rank / (double)UInt16.MaxValue; } }
		public Bitmap Thumbnail { get { return m_oBitmap; } }
		public String Description { get { return m_aCommonAttributes["description"]; } }
		public String ServerUrl { get { return "http://" + m_aCommonAttributes["url"]; } }
		public String ServerType { get { return m_aCommonAttributes["type"]; } }
		public double MinX { get { return Double.Parse(m_aCommonAttributes["minx"], CultureInfo.InvariantCulture); } }
		public double MinY { get { return Double.Parse(m_aCommonAttributes["miny"], CultureInfo.InvariantCulture); } }
		public double MaxX { get { return Double.Parse(m_aCommonAttributes["maxx"], CultureInfo.InvariantCulture); } }
		public double MaxY { get { return Double.Parse(m_aCommonAttributes["maxy"], CultureInfo.InvariantCulture); } }
		public Geosoft.Dap.Common.BoundingBox Bounds { get { return new Geosoft.Dap.Common.BoundingBox(MaxX, MaxY, MinX, MinY); } }
	}

	public class DownloadCompleteEventArgs
	{
		private HttpWebRequest m_oRequest;
		private HttpWebResponse m_oResponse;
		private Exception m_oError;

		public DownloadCompleteEventArgs(HttpWebRequest oRequest, HttpWebResponse oResponse, Exception oException)
		{
			m_oRequest = oRequest;
			m_oResponse = oResponse;
			m_oError = oException;
		}

		public HttpWebRequest Request
		{
			get { return m_oRequest; }
			set { m_oRequest = value; }
		}

		public HttpWebResponse Response
		{
			get { return m_oResponse; }
			set { m_oResponse = value; }
		}

		public Exception Error
		{
			get { return m_oError; }
			set { m_oError = value; }
		}
	}
	public delegate void DownloadCompleteHandler(Object sender, DownloadCompleteEventArgs args);
	

	public class DappleSearchWebDownload
	{
		private DownloadCompleteHandler m_oCallback = null;
		private Geosoft.Dap.Common.BoundingBox m_oBoundingBox = null;
		private String m_strKeywords = null;
		private int m_iPage = 0;
		private int m_iNumResults = PageNavigator.ResultsPerPage;
		private long m_lSearchIndex;

		public DappleSearchWebDownload(Geosoft.Dap.Common.BoundingBox oBoundingBox, String strKeywords, int iPage, long lSearchIndex)
		{
			m_oBoundingBox = oBoundingBox;
			m_strKeywords = strKeywords;
			m_iPage = iPage;
			m_lSearchIndex = lSearchIndex;
		}

		public long SearchIndex { get { return m_lSearchIndex; } }

		public void StartDownload(DownloadCompleteHandler oHandler)
		{
			m_oCallback = oHandler;
			ThreadPool.QueueUserWorkItem(new WaitCallback(BuildRequest));
		}

		protected void BuildRequest(Object unused)
		{
			HttpWebRequest oRequest = null;
			try
			{
				oRequest = (HttpWebRequest)WebRequest.Create(Settings.Default.SearchServer + "SearchInterfaceXML.aspx");
			}
			catch (Exception ex)
			{
				m_oCallback(this, new DownloadCompleteEventArgs(null, null, ex));
				return;
			}

			// --- Create the document ---
			XmlDocument query = new XmlDocument();
			XmlElement geoRoot = query.CreateElement("geosoft_xml");
			query.AppendChild(geoRoot);
			XmlElement root = query.CreateElement("search_request");
			root.SetAttribute("version", "1.0");
			root.SetAttribute("handle", "cheese");
			root.SetAttribute("maxcount", m_iNumResults.ToString(CultureInfo.InvariantCulture));
			root.SetAttribute("offset", (m_iPage * m_iNumResults).ToString(CultureInfo.InvariantCulture));
			geoRoot.AppendChild(root);

			if (m_oBoundingBox != null)
			{
				XmlElement boundingBox = query.CreateElement("bounding_box");
				boundingBox.SetAttribute("minx", m_oBoundingBox.MinX.ToString(CultureInfo.InvariantCulture));
				boundingBox.SetAttribute("miny", m_oBoundingBox.MinY.ToString(CultureInfo.InvariantCulture));
				boundingBox.SetAttribute("maxx", m_oBoundingBox.MaxX.ToString(CultureInfo.InvariantCulture));
				boundingBox.SetAttribute("maxy", m_oBoundingBox.MaxY.ToString(CultureInfo.InvariantCulture));
				boundingBox.SetAttribute("crs", "WSG84");
				root.AppendChild(boundingBox);
			}

			if (!String.IsNullOrEmpty(m_strKeywords))
			{
				XmlElement keyword = query.CreateElement("text_filter");
				keyword.InnerText = m_strKeywords;
				root.AppendChild(keyword);
			}

			// --- Add document to request ---
			oRequest.Headers["GeosoftMapSearchRequest"] = query.InnerXml;

			oRequest.BeginGetResponse(new AsyncCallback(GetResponse), oRequest);
		}

		protected void GetResponse(IAsyncResult oResult)
		{
			HttpWebRequest oRequest = oResult.AsyncState as HttpWebRequest;
			HttpWebResponse oResponse = null;

			try
			{
				try
				{
					oResponse = oRequest.EndGetResponse(oResult) as HttpWebResponse;
				}
				catch (Exception ex)
				{
					m_oCallback(this, new DownloadCompleteEventArgs(null, null, ex));
					return;
				}

				m_oCallback(this, new DownloadCompleteEventArgs(oRequest, oResponse, null));
			}
			finally
			{
				if (oResponse != null) oResponse.Close();
			}
		}
	}
}
