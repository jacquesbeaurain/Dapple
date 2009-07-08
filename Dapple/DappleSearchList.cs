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
using WorldWind.Net;
using System.Globalization;
using NewServerTree;

namespace Dapple.CustomControls
{
	internal partial class DappleSearchList : UserControl
	{
		#region Enums

		private enum DisplayMode
		{
			Thumbnail = 0,
			List = 1
		}

		#endregion

		#region Events

		internal event EventHandler LayerSelectionChanged;

		#endregion

		#region Statics

		private const int THUMBNAIL_SIZE = 50;
		private const int BAR_WIDTH = 5;
		private const int ICON_SIZE = 16;

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
		private DappleModel m_oModel;
		private int m_iPageSize;

		long m_lSearchIndex = 0;

		#endregion

		#region Constructors

		internal DappleSearchList()
		{
			InitializeComponent();
			c_tsTabToolstrip.SetImage(0, Dapple.Properties.Resources.tab_thumbnail);
			c_tsTabToolstrip.SetImage(1, Dapple.Properties.Resources.tab_list);
			c_tsTabToolstrip.SetToolTip(0, "Thumbnail View");
			c_tsTabToolstrip.SetToolTip(1, "List View");
			c_tsTabToolstrip.SetNameAndText(0, "ShowThumbnails");
			c_tsTabToolstrip.SetNameAndText(1, "HideThumbnails");
			c_tsTabToolstrip.ButtonPressed += new TabToolStrip.TabToolbarButtonDelegate(DisplayModeChanged);
			SetNoSearch();
			c_oPageNavigator.PageBack += BackPage;
			c_oPageNavigator.PageForward += ForwardPage;
		}

		#endregion

		#region Properties

		internal bool HasLayersSelected
		{
			get
			{
				return c_lbResults.SelectedIndices.Count > 0;
			}
		}

		internal List<LayerUri> SelectedLayers
		{
			get
			{
				List<LayerUri> result = new List<LayerUri>();

				foreach (int index in c_lbResults.SelectedIndices)
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
					double dWidthRatio = (double)oResult.Thumbnail.Width / THUMBNAIL_SIZE;
					double dHeightRatio = (double)oResult.Thumbnail.Height / THUMBNAIL_SIZE;
					double dMajorRatio = Math.Max(dWidthRatio, dHeightRatio);

					dWidthRatio /= dMajorRatio;
					dHeightRatio /= dMajorRatio;

					int iDrawWidth = (int)(THUMBNAIL_SIZE * dWidthRatio);
					if (iDrawWidth < 1) iDrawWidth = 1;
					int iDrawHeight = (int)(THUMBNAIL_SIZE * dHeightRatio);
					if (iDrawHeight < 1) iDrawHeight = 1;

					int iOffsetX = (THUMBNAIL_SIZE - iDrawWidth) / 2;
					int iOffsetY = (THUMBNAIL_SIZE - iDrawHeight) / 2;

					e.Graphics.DrawImage(oResult.Thumbnail, new Rectangle(1 + iOffsetX, 1 + iOffsetY, iDrawWidth, iDrawHeight));
				}

				// --- Title and URL ---

				e.Graphics.TranslateTransform(THUMBNAIL_SIZE + 2, 0, MatrixOrder.Append);

				using (Font oTitleFont = new Font(c_lbResults.Font, FontStyle.Bold))
				{
					if (String.IsNullOrEmpty(oResult.ArcIMSServiceName))
					{
						int iSpace = (THUMBNAIL_SIZE - 2 * c_lbResults.Font.Height) / 3;
						e.Graphics.DrawString(oResult.Title, oTitleFont, Brushes.Black, new PointF(0, iSpace));
						e.Graphics.DrawString(oResult.ServerUrl, c_lbResults.Font, Brushes.Black, new PointF(0, iSpace * 2 + c_lbResults.Font.Height));
					}
					else
					{
						int iSpace = (THUMBNAIL_SIZE - 3 * c_lbResults.Font.Height) / 4;
						e.Graphics.DrawString(oResult.Title, oTitleFont, Brushes.Black, new PointF(0, iSpace));
						e.Graphics.DrawString(oResult.ArcIMSServiceName, c_lbResults.Font, Brushes.Black, new PointF(0, iSpace * 2 + c_lbResults.Font.Height));
						e.Graphics.DrawString(oResult.ServerUrl, c_lbResults.Font, Brushes.Black, new PointF(0, iSpace * 3 + c_lbResults.Font.Height * 2));
					}
				}
			}
			else if (m_eDisplayMode == DisplayMode.List)
			{
				e.Graphics.DrawIcon(Dapple.Properties.Resources.layer, new Rectangle(0, 0, e.Bounds.Height, e.Bounds.Height));
				e.Graphics.TranslateTransform(e.Bounds.Height, 0, MatrixOrder.Append);
				e.Graphics.DrawString(String.Format(CultureInfo.CurrentCulture ,"({0:P0}) {1}", oResult.PercentageRank, oResult.Title), c_lbResults.Font, Brushes.Black, new PointF(0, 0));
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
				m_iHorizontalExtent = Math.Max(m_iHorizontalExtent, e.ItemWidth);
			}
			else if (m_eDisplayMode == DisplayMode.List)
			{
				e.ItemWidth = ICON_SIZE + (int)e.Graphics.MeasureString(String.Format(CultureInfo.CurrentCulture, "({0:P0}) {1}", oResult.PercentageRank, oResult.Title), c_lbResults.Font).Width;
				e.ItemHeight = ICON_SIZE;
				m_iHorizontalExtent = Math.Max(m_iHorizontalExtent, e.ItemWidth);

			}
		}

		private void cResultListBox_MouseMove(object sender, MouseEventArgs e)
		{
			if (m_oDragDropStartPoint != Point.Empty && (m_oDragDropStartPoint.X != e.X || m_oDragDropStartPoint.Y != e.Y) && (e.Button & MouseButtons.Left) == MouseButtons.Left)
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


		#region Public Methods

		internal void Attach(DappleModel oModel, LayerList oList)
		{
			m_oModel = oModel;
			m_hLayerList = oList;
		}

		#endregion


		private void BackPage(object sender, EventArgs e)
		{
			m_iCurrentPage--;
			RefreshResultList();
		}

		private void ForwardPage(object sender, EventArgs e)
		{
			m_iCurrentPage++;
			if (m_iCurrentPage >= m_iAccessedPages)
			{
				m_iAccessedPages++;
				DappleSearchWebDownload oDownload = new DappleSearchWebDownload(m_oSearchBoundingBox, m_szSearchString, m_iCurrentPage, m_lSearchIndex, m_iPageSize);
				SetSearching();
				oDownload.BackgroundDownloadMemory(new DownloadCompleteHandler(ForwardPageComplete));
			}
			else
			{
				RefreshResultList();
			}
		}

		private void ForwardPageComplete(WebDownload oWebDownload)
		{
			DappleSearchWebDownload oDSWebDownload = oWebDownload as DappleSearchWebDownload;

			// --- If the search indices don't match, it's because a new search was done before this one completed, so discard these results ---
			if (oDSWebDownload.SearchIndex == m_lSearchIndex)
			{
				XmlDocument oDoc = new XmlDocument();
				oDoc.Load(oWebDownload.ContentStream);
				SearchResultSet oPage = new SearchResultSet(oDoc);
				ExtendSearch(oPage, m_iCurrentPage);
			}
		}

		internal void SetSearchParameters(String szKeyword, GeographicBoundingBox oBoundingBox)
		{
			if (!MainForm.Settings.UseDappleSearch) return;

			m_lSearchIndex++;
			m_oSearchBoundingBox = oBoundingBox;
			m_szSearchString = szKeyword;

			if (String.Empty.Equals(szKeyword) && oBoundingBox == null)
			{
				SetNoSearch();
			}
			else
			{
				UpdatePageSize();
				DappleSearchWebDownload oDownload = new DappleSearchWebDownload(m_oSearchBoundingBox, m_szSearchString, 0, m_lSearchIndex, m_iPageSize);
				SetSearching();
				oDownload.BackgroundDownloadMemory(new DownloadCompleteHandler(SetSearchParametersComplete));
			}
		}

		private void UpdatePageSize()
		{
			if (m_eDisplayMode == DisplayMode.Thumbnail)
				m_iPageSize = Math.Max(c_lbResults.ClientSize.Height / (THUMBNAIL_SIZE + 1), 1);
			else
				m_iPageSize = Math.Max(c_lbResults.ClientSize.Height / (ICON_SIZE) - 1, 1);

			c_oPageNavigator.SetPageSize(m_iPageSize);
		}

		private void SetSearchParametersComplete(WebDownload oWebDownload)
		{
			try
			{
				DappleSearchWebDownload oDSWebDownload = oWebDownload as DappleSearchWebDownload;
				oWebDownload.Verify();

				// --- If the search indices don't match, it's because a new search was done before this one completed, so discard these results ---
				if (oDSWebDownload.SearchIndex == m_lSearchIndex)
				{
					XmlDocument oDoc = new XmlDocument();
					oDoc.Load(oWebDownload.ContentStream);
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
				LayerBuilder oLayerToAdd = null;
				try
				{
					oLayerToAdd = oUri.getBuilder(m_oModel);
					if (oLayerToAdd != null)
					{
						result.Add(oLayerToAdd);
					}
				}
				catch (Exception ex)
				{
					Program.ShowMessageBox(ex.Message, "Dataset Could Not Be Added", MessageBoxButtons.OK, MessageBoxDefaultButton.Button1, MessageBoxIcon.Error);
				}
			}

			return result;
		}

		internal void CmdAddSelected()
		{
			if (c_lbResults.SelectedIndices.Count > 0 && m_hLayerList != null)
			{
				List<LayerUri> oLayerUris = new List<LayerUri>();

				foreach (int iIndex in c_lbResults.SelectedIndices)
				{
					oLayerUris.Add(m_aPages[m_iCurrentPage].Results[iIndex].Uri);
				}

				m_hLayerList.AddLayers(CreateLayerBuilders(oLayerUris));
			}
		}

		private int m_iHorizontalExtent;
		private void RefreshResultList()
		{
			if (InvokeRequired) throw new InvalidOperationException("Tried to refresh result list off of the event thread");

			if (String.Empty.Equals(m_szSearchString) && m_oSearchBoundingBox == null) return; // Nothing to refresh, no search set.

			c_lbResults.SuspendLayout();
			c_lbResults.Items.Clear();

			if (!MainForm.Settings.UseDappleSearch)
			{
				c_oPageNavigator.SetState("DappleSearch is disabled");
				return;
			}

			if (m_aPages == null)
			{
				c_oPageNavigator.SetState("Error contacting search server");
			}
			else
			{
				if (m_aPages.Length > 0 && m_aPages[m_iCurrentPage] != null)
				{
					m_iHorizontalExtent = 0;
					foreach (SearchResult oResult in m_aPages[m_iCurrentPage].Results)
					{
						c_lbResults.Items.Add(oResult);
					}
					c_oPageNavigator.SetState(m_iCurrentPage, m_aPages[0].TotalCount);
					c_lbResults.HorizontalExtent = m_iHorizontalExtent;
				}
				else
				{
					c_oPageNavigator.SetState("No results");
					c_lbResults.HorizontalExtent = 0;
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

			if (!MainForm.Settings.UseDappleSearch)
			{
				c_oPageNavigator.SetState("DappleSearch is disabled");
			}
			else
			{
				c_oPageNavigator.SetState("Press Alt-S to search");
			}
			c_lbResults.HorizontalExtent = 0;
		}

		private void SetSearching()
		{
			c_lbResults.Items.Clear();
			c_oPageNavigator.SetState("Searching...");
		}

		private void SetSearchFailed()
		{
			m_iCurrentPage = 0;
			m_iAccessedPages = 0;
			m_aPages = new SearchResultSet[0];
			c_lbResults.Items.Clear();
			c_oPageNavigator.SetState("Error contacting DappleSearch server");
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
					int iNumPages = c_oPageNavigator.PagesFromResults(oResults.TotalCount);

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
		private readonly int m_iTotalCount;
		private readonly List<SearchResult> m_aResults;

		private readonly Object LOCK = new Object();
		private bool m_blThumbnailsQueued;

		internal SearchResultSet(XmlNode oSearchResult)
		{
			if (oSearchResult.SelectSingleNode("/geosoft_xml/error") != null) 
				throw new ArgumentException("Server sent error message");

			m_aResults = new List<SearchResult>();

			var oRootElement = oSearchResult.SelectSingleNode("/geosoft_xml/search_result") as XmlElement;

			if (oRootElement != null)
			{
				m_iTotalCount = Int32.Parse(oRootElement.GetAttribute("totalcount"), NumberStyles.Any, CultureInfo.InvariantCulture);

				var layerElements = oRootElement.SelectNodes("/geosoft_xml/search_result/layers/layer");
				if (layerElements != null)
				{
					foreach (XmlNode oResult in layerElements)
						m_aResults.Add(new SearchResult(oResult as XmlElement));
				}
			}
		}

		internal int TotalCount { get { return m_iTotalCount; } }
		internal List<SearchResult> Results { get { return m_aResults; } }

		#region Asynchronous Thumbnail Loading

		internal void QueueThumbnails(ListBox oView)
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

	class SearchResult
	{
		private Bitmap m_oBitmap;
		private Dictionary<String, String> m_aCommonAttributes = new Dictionary<string, string>();
		private Dictionary<String, String> m_aTypeSpecificAttributes = new Dictionary<string, string>();

		internal SearchResult(XmlElement oLayerElement)
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

		internal void downloadThumbnail()
		{
			if (!MainForm.Settings.UseDappleSearch) return;

			WebDownload oThumbnailDownload = new WebDownload(MainForm.Settings.DappleSearchURL + "Thumbnail.aspx?layerid=" + m_aCommonAttributes["obaselayerid"], false);
			try
			{
				oThumbnailDownload.DownloadMemory();
				m_oBitmap = new Bitmap(oThumbnailDownload.ContentStream);
			}
			catch (Exception)
			{
				m_oBitmap = Dapple.Properties.Resources.delete;
			}
		}

		internal String Title { get { return m_aCommonAttributes["layertitle"]; } }
		internal UInt16 Rank { get { return UInt16.Parse(m_aCommonAttributes["rankingscore"], CultureInfo.InvariantCulture); } }
		internal double PercentageRank { get { return (double)Rank / (double)UInt16.MaxValue; } }
		internal Bitmap Thumbnail { get { return m_oBitmap; } }
		internal String ServerUrl { get { return "http://" + m_aCommonAttributes["url"]; } }
		internal String ArcIMSServiceName
		{
			get
			{
				if (m_aCommonAttributes["type"].Equals("ArcIMS"))
					return m_aTypeSpecificAttributes["servicename"];
				else
					return null;
			}
		}

		internal LayerUri Uri
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
					//szUri += "&title=" + HttpUtility.UrlEncode(m_aCommonAttributes["layertitle"]);
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

	class DappleSearchWebDownload : WebDownload
	{
		private GeographicBoundingBox m_oBoundingBox = null;
		private String m_szKeywords = null;
		private int m_iPage = 0;
		private int m_iNumResults;
		private long m_lSearchIndex;

		internal DappleSearchWebDownload(GeographicBoundingBox oBoundingBox, String szKeywords, int iPage, long lSearchIndex, int resultsPerPage)
			: base(MainForm.Settings.DappleSearchURL, true)
		{
			m_oBoundingBox = oBoundingBox;
			m_szKeywords = szKeywords;
			m_iPage = iPage;
			m_lSearchIndex = lSearchIndex;
			m_iNumResults = resultsPerPage;
		}

		internal long SearchIndex
		{
			get { return m_lSearchIndex; }
		}

		protected override HttpWebRequest BuildRequest()
		{
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
				boundingBox.SetAttribute("minx", m_oBoundingBox.West.ToString(CultureInfo.InvariantCulture));
				boundingBox.SetAttribute("miny", m_oBoundingBox.South.ToString(CultureInfo.InvariantCulture));
				boundingBox.SetAttribute("maxx", m_oBoundingBox.East.ToString(CultureInfo.InvariantCulture));
				boundingBox.SetAttribute("maxy", m_oBoundingBox.North.ToString(CultureInfo.InvariantCulture));
				boundingBox.SetAttribute("crs", "WSG84");
				root.AppendChild(boundingBox);
			}

			if (!String.IsNullOrEmpty(m_szKeywords))
			{
				XmlElement keyword = query.CreateElement("text_filter");
				keyword.InnerText = m_szKeywords;
				root.AppendChild(keyword);
			}

			// --- Create the request ---

			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(MainForm.Settings.DappleSearchURL + MainForm.SEARCH_XML_GATEWAY);
			request.Headers["GeosoftMapSearchRequest"] = query.InnerXml;
			request.UserAgent = UserAgent;

			if (this.Compressed)
			{
				request.Headers.Add("Accept-Encoding", "gzip,deflate");
			}

			request.Proxy = ProxyHelper.DetermineProxyForUrl(
				Url,
				useWindowsDefaultProxy,
				useDynamicProxy,
				proxyUrl,
				proxyUserName,
				proxyPassword);

			request.ProtocolVersion = HttpVersion.Version11;
			request.Timeout = WebDownload.DownloadTimeout;

			return request;
		}
	}
}
