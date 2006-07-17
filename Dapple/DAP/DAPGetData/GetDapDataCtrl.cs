using System;
using System.Text;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Runtime.InteropServices;

using Geosoft.Dap;

namespace Geosoft.GX.DAPGetData
{
    /// <summary>
    /// Summary description for GetDapData.
    /// </summary>
    public class GetDapData : UserControl
    {
        #region Constants
        protected const Int32 BROWSE_MENU_IMAGE = 0;
        protected const Int32 SEARCH_MENU_IMAGE = 1;
        protected const Int32 RESULTS_MENU_IMAGE = 2;
        protected const Int32 SELECTED_MENU_IMAGE = 3;
        protected const Int32 GET_DATA_MENU_IMAGE = 4;
        #endregion

        #region enum
        public struct AsyncRequest
        {
            public AsyncRequestType m_eType;
            public object m_oParam1;
            public object m_oParam2;
        }

        public enum AsyncRequestType
        {
            GetImage,
            GetCatalog,
            GetCatalogCount,
            SetupExtract,
            Stop
        }
        #endregion

        #region Member Variables
        // --- static instance of the main form ---
        static GetDapData m_hThis = null;

        protected bool m_bInit = false;

        protected String m_strQueryString = String.Empty;
        protected Server m_oCurServer;
        protected ServerList m_oServerList;
        protected SortedList m_oValidServerList;
        protected SortedList m_oFullServerList;
        protected Geosoft.Dap.Common.BoundingBox m_oOMExtents = null;
        protected Geosoft.Dap.Common.BoundingBox m_oSearchExtents = null;

        protected int m_iGxHandle;
        protected bool m_bMapInfo = false;
        protected bool m_bArcGIS = false;

        private ResultsDocument2 m_hResultsDocument;
        private SelectedDocument m_hSelectedDocument;
        private string m_cacheDir;

        private WorldWind.WorldWindow m_wwCtl;
        public Hashtable RenderableLayers = new Hashtable();

        protected Geosoft.DotNetTools.ErrorLogger.ErrorLogger m_hError;

        protected DotNetTools.Common.Queue m_oAsyncQueue;
        protected System.Threading.Thread m_oAsyncThread1;
        protected System.Threading.Thread m_oAsyncThread2;
        protected System.Threading.Thread m_oAsyncThread3;
        #endregion

        private Geosoft.WinFormsUI.DockPanel dockPanel;
        private Geosoft.WinFormsUI.DeserializeDockContent m_deserializeDockContent;
        private System.Windows.Forms.MainMenu mMain;
        private System.Windows.Forms.MenuItem miView;
        private System.Windows.Forms.MenuItem miMap;
        private System.Windows.Forms.MenuItem miResults;
        private System.Windows.Forms.MenuItem miSelectedDatasets;
        private System.Windows.Forms.MenuItem miGetData;
        private System.Windows.Forms.MenuItem miFile;
        private System.Windows.Forms.MenuItem miExit;
        private System.Windows.Forms.ImageList ilViewMenu;
        private System.Windows.Forms.MenuItem miServer;
        private System.Windows.Forms.MenuItem miHelp;
        private System.Windows.Forms.StatusBar sbStatus;
        private System.Windows.Forms.MenuItem miFileLogin;
        private System.Windows.Forms.MenuItem miFileLogout;
        private System.Windows.Forms.MenuItem miFileSeparator1;
        private System.Windows.Forms.StatusBarPanel sbpLoginStatus;
        private System.ComponentModel.IContainer components;

        #region Properties
        /// <summary>
        /// Return the single instance of this class
        /// </summary>
        public static GetDapData Instance
        {
            get { return m_hThis; }
        }

        /// <summary>
        /// Get the current server
        /// </summary>
        public Server CurServer
        {
            get { return m_oCurServer; }
        }

        /// <summary>
        /// Get the server list
        /// </summary>
        public SortedList ServerList
        {
            get { return m_oValidServerList; }
        }

        /// <summary>
        /// Get the full server list
        /// </summary>
        public SortedList FullServerList
        {
            get { return m_oFullServerList; }
        }

        /// <summary>
        /// Get the extents specified by an open map in OM
        /// </summary>
        public Geosoft.Dap.Common.BoundingBox OMExtents
        {
            get { return m_oOMExtents; }
            set { m_oOMExtents = value; }
        }

        /// <summary>
        /// Get/Set the search extents
        /// </summary>
        public Geosoft.Dap.Common.BoundingBox SearchExtents
        {
            get { return m_oSearchExtents; }
            set { m_oSearchExtents = value; }
        }

        /// <summary>
        /// Get/Set the catalog query string
        /// </summary>
        public string QueryString
        {
            get { return m_strQueryString; }
            set { m_strQueryString = value; }
        }

        /// <summary>
        /// Get the error file to write to
        /// </summary>
        public Geosoft.DotNetTools.ErrorLogger.ErrorLogger Error
        {
            get { return m_hError; }
        }

        /// <summary>
        /// Get whether we are running in mapinfo
        /// </summary>
        public bool IsMapInfo
        {
            get { return m_bMapInfo; }
        }

        /// <summary>
        /// Get whether we are running in arc gis
        /// </summary>
        public bool IsArcGIS
        {
            get { return m_bArcGIS; }
        }
        #endregion

        #region Constructor/Destructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public GetDapData(WorldWind.WorldWindow wwCtl, string cacheDir)
        {
            m_deserializeDockContent = new Geosoft.WinFormsUI.DeserializeDockContent(GetContentFromPersistString);

            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
            m_strQueryString = String.Empty;
            m_oCurServer = null;
            m_oOMExtents = null;
            m_oSearchExtents = null;
            m_oValidServerList = new SortedList();
            m_oFullServerList = new SortedList();
            m_cacheDir = cacheDir;

            this.m_wwCtl = wwCtl; // Added for WorldWind

            m_hThis = this;

            m_oAsyncQueue = new DotNetTools.Common.Queue();


            // --- must do after we initialize the gx pointer ---

            //m_oAsyncThread1 = new System.Threading.Thread(new System.Threading.ThreadStart(SendAsyncRequest));
            //m_oAsyncThread1.Start();

            //m_oAsyncThread2 = new System.Threading.Thread(new System.Threading.ThreadStart(SendAsyncRequest));
            //m_oAsyncThread2.Start();

            //m_oAsyncThread3 = new System.Threading.Thread(new System.Threading.ThreadStart(SendAsyncRequest));
            //m_oAsyncThread3.Start();


            // --- Create the new map ---

            string strWorkDir = string.Empty;

            m_hError = new Geosoft.DotNetTools.ErrorLogger.ErrorLogger(System.IO.Path.Combine(strWorkDir, "_GetDapDataError.log"));

            m_oServerList = new ServerList(false, m_cacheDir, null);        
            SetupForm();

            m_hResultsDocument.Show(this.dockPanel);
            m_hSelectedDocument.Show(this.dockPanel);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            this.GetDapDataCtrl_Closing(null, null);
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }
        #endregion

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(GetDapData));
            this.dockPanel = new Geosoft.WinFormsUI.DockPanel();
            this.mMain = new System.Windows.Forms.MainMenu();
            this.miFile = new System.Windows.Forms.MenuItem();
            this.miFileLogin = new System.Windows.Forms.MenuItem();
            this.miFileLogout = new System.Windows.Forms.MenuItem();
            this.miFileSeparator1 = new System.Windows.Forms.MenuItem();
            this.miExit = new System.Windows.Forms.MenuItem();
            this.miServer = new System.Windows.Forms.MenuItem();
            this.miView = new System.Windows.Forms.MenuItem();
            this.miMap = new System.Windows.Forms.MenuItem();
            this.miResults = new System.Windows.Forms.MenuItem();
            this.miSelectedDatasets = new System.Windows.Forms.MenuItem();
            this.miGetData = new System.Windows.Forms.MenuItem();
            this.miHelp = new System.Windows.Forms.MenuItem();
            this.ilViewMenu = new System.Windows.Forms.ImageList(this.components);
            this.sbStatus = new System.Windows.Forms.StatusBar();
            this.sbpLoginStatus = new System.Windows.Forms.StatusBarPanel();
            ((System.ComponentModel.ISupportInitialize)(this.sbpLoginStatus)).BeginInit();
            this.SuspendLayout();
            // 
            // dockPanel
            // 
            this.dockPanel.ActiveAutoHideContent = null;
            this.dockPanel.BackColor = System.Drawing.SystemColors.Control;
            this.dockPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dockPanel.Font = new System.Drawing.Font("Tahoma", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.World);
            this.dockPanel.Location = new System.Drawing.Point(0, 0);
            this.dockPanel.Name = "dockPanel";
            this.dockPanel.Size = new System.Drawing.Size(1104, 623);
            this.dockPanel.TabIndex = 0;
            // 
            // mMain
            // 
            this.mMain.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                              this.miFile,
                                                                              this.miServer,
                                                                              this.miView,
                                                                              this.miHelp});
            // 
            // miFile
            // 
            this.miFile.Index = 0;
            this.miFile.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                               this.miFileLogin,
                                                                               this.miFileLogout,
                                                                               this.miFileSeparator1,
                                                                               this.miExit});
            this.miFile.Text = "&File";
            // 
            // miFileLogin
            // 
            this.miFileLogin.Index = 0;
            this.miFileLogin.Text = "&Login";
            this.miFileLogin.Click += new System.EventHandler(this.miFileLogin_Click);
            // 
            // miFileLogout
            // 
            this.miFileLogout.Index = 1;
            this.miFileLogout.Text = "Logou&t";
            this.miFileLogout.Visible = false;
            this.miFileLogout.Click += new System.EventHandler(this.miFileLogout_Click);
            // 
            // miFileSeparator1
            // 
            this.miFileSeparator1.Index = 2;
            this.miFileSeparator1.Text = "-";
            // 
            // miExit
            // 
            this.miExit.Index = 3;
            this.miExit.Shortcut = System.Windows.Forms.Shortcut.AltF4;
            this.miExit.Text = "E&xit";
            this.miExit.Click += new System.EventHandler(this.miExit_Click);
            // 
            // miServer
            // 
            this.miServer.Index = 1;
            this.miServer.Text = "&Servers";
            this.miServer.Click += new System.EventHandler(this.miServers_Click);
            // 
            // miView
            // 
            this.miView.Index = 2;
            this.miView.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                               this.miMap,
                                                                               this.miResults,
                                                                               this.miSelectedDatasets,
                                                                               this.miGetData});
            this.miView.Text = "&View";
            // 
            // miMap
            // 
            this.miMap.Index = 0;
            this.miMap.OwnerDraw = true;
            this.miMap.Text = "Browser Map";
            this.miMap.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.miMap_DrawItem);
            this.miMap.Click += new System.EventHandler(this.miMap_Click);
            this.miMap.MeasureItem += new System.Windows.Forms.MeasureItemEventHandler(this.miMap_MeasureItem);
            // 
            // miResults
            // 
            this.miResults.Index = 1;
            this.miResults.OwnerDraw = true;
            this.miResults.Text = "Results";
            this.miResults.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.miResults_DrawItem);
            this.miResults.Click += new System.EventHandler(this.miResults_Click);
            this.miResults.MeasureItem += new System.Windows.Forms.MeasureItemEventHandler(this.miResults_MeasureItem);
            // 
            // miSelectedDatasets
            // 
            this.miSelectedDatasets.Index = 2;
            this.miSelectedDatasets.OwnerDraw = true;
            this.miSelectedDatasets.Text = "Selected Datasets";
            this.miSelectedDatasets.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.miSelectedDatasets_DrawItem);
            this.miSelectedDatasets.Click += new System.EventHandler(this.miSelectedDatasets_Click);
            this.miSelectedDatasets.MeasureItem += new System.Windows.Forms.MeasureItemEventHandler(this.miSelectedDatasets_MeasureItem);
            // 
            // miGetData
            // 
            this.miGetData.Index = 3;
            this.miGetData.OwnerDraw = true;
            this.miGetData.Text = "Get Data";
            this.miGetData.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.miGetData_DrawItem);
            this.miGetData.Click += new System.EventHandler(this.miGetData_Click);
            this.miGetData.MeasureItem += new System.Windows.Forms.MeasureItemEventHandler(this.miGetData_MeasureItem);
            // 
            // miHelp
            // 
            this.miHelp.Index = 3;
            this.miHelp.Text = "&Help";
            this.miHelp.Click += new System.EventHandler(this.miHelp_Click);
            // 
            // ilViewMenu
            // 
            this.ilViewMenu.ImageSize = new System.Drawing.Size(16, 16);
            this.ilViewMenu.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ilViewMenu.ImageStream")));
            this.ilViewMenu.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // sbStatus
            // 
            this.sbStatus.Location = new System.Drawing.Point(0, 623);
            this.sbStatus.Name = "sbStatus";
            this.sbStatus.Panels.AddRange(new System.Windows.Forms.StatusBarPanel[] {
                                                                                    this.sbpLoginStatus});
            this.sbStatus.ShowPanels = true;
            this.sbStatus.Size = new System.Drawing.Size(1104, 22);
            this.sbStatus.TabIndex = 1;
            // 
            // sbpLoginStatus
            // 
            this.sbpLoginStatus.Text = "Not logged in";
            this.sbpLoginStatus.Width = 200;
            // 
            // GetDapData
            // 
            //this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.BackColor = System.Drawing.Color.LightSteelBlue;
            this.ClientSize = new System.Drawing.Size(1104, 645);
            this.Controls.Add(this.dockPanel);
            this.Controls.Add(this.sbStatus);
            //this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Menu = this.mMain;
            this.Name = "GetDapData";
            //this.ShowInTaskbar = false;
            //this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Get DAP Data";
            //this.OnClosing += new System.ComponentModel.CancelEventHandler(this.GetDapDataCtrl_Closing);
            this.VisibleChanged += new System.EventHandler(this.GetDapDataCtrl_VisibleChanged);
            ((System.ComponentModel.ISupportInitialize)(this.sbpLoginStatus)).EndInit();
            this.ResumeLayout(false);

        }
        #endregion

        #region Protected Member Functions
        protected Geosoft.WinFormsUI.DockContent GetContentFromPersistString(string persistString)
        {
            if (persistString == typeof(ResultsDocument2).ToString())
                return m_hResultsDocument;
            else if (persistString == typeof(SelectedDocument).ToString())
                return m_hSelectedDocument;

            return null;
        }

        protected void CreateResultsDocument()
        {
            m_hResultsDocument = new ResultsDocument2();
            m_hResultsDocument.HideOnClose = true;
            m_hResultsDocument.DockableAreas = Geosoft.WinFormsUI.DockAreas.Document;
            m_hResultsDocument.DataSetSelected += new DataSetSelectedHandler(m_hResultsDocument_DataSetSelected);
            m_hResultsDocument.ViewMeta += new ViewMetaHandler(OnViewMeta);
            m_hResultsDocument.RefreshClick += new System.Threading.ThreadStart(this.RefreshCatalog);
            m_hResultsDocument.ShowAllClick += new System.Threading.ThreadStart(this.ShowAllCatalog);
        }

        protected void CreateSelectedDocument()
        {
            m_hSelectedDocument = new SelectedDocument(this.m_wwCtl);
            m_hSelectedDocument.HideOnClose = true;
            m_hSelectedDocument.DockableAreas = Geosoft.WinFormsUI.DockAreas.Document;
            m_hSelectedDocument.DataSetSelected += new DataSetSelectedHandler(m_hSelectedDocument_DataSetSelected);
            m_hSelectedDocument.ViewMeta += new ViewMetaHandler(OnViewMeta);
            m_hSelectedDocument.DataSetDeleted += new DataSetDeletedHandler(m_hSelectedDocument_DataSetDeleted);
            m_hSelectedDocument.DataSetsDeleted += new EventHandler(m_hSelectedDocument_DataSetsDeleted);
            m_hSelectedDocument.DataSetPromoted += new DataSetPromotedHandler(m_hSelectedDocument_DataSetOrderChanged);
            m_hSelectedDocument.DataSetDemoted += new DataSetDemotedHandler(m_hSelectedDocument_DataSetOrderChanged);
        }

        /// <summary>
        /// Calculate the width of this menu item
        /// </summary>
        /// <param name="miItem"></param>
        /// <param name="ilImages"></param>
        /// <param name="iImageIndex"></param>
        protected void MeasureMenuItem(MenuItem miItem, Image iImage, Graphics hGraphics, out Int32 iWidth, out Int32 iHeight)
        {
            // --- get the standard menu font ---

            Font hFont = SystemInformation.MenuFont;
            StringFormat hStrFmt = new StringFormat();


            // --- measure the text length ---

            SizeF hSizeF = hGraphics.MeasureString(miItem.Text, hFont, 1000, hStrFmt);


            // --- measure the bitmap length ---

            iWidth = (int)Math.Ceiling(hSizeF.Width) + iImage.Width + 12;
            iHeight = Math.Max((int)Math.Ceiling(hSizeF.Height), iImage.Height) + 4;
        }

        /// <summary>
        /// Draw the menu item
        /// </summary>
        /// <param name="miItem"></param>
        /// <param name="ilImages"></param>
        /// <param name="iImageIndex"></param>
        /// <param name="bSelected"></param>
        protected void DrawMenuItem(MenuItem miItem, Image iImage, bool bSelected, Graphics hGraphics, Rectangle Rect)
        {
            Font hFont = SystemInformation.MenuFont;
            StringFormat hStrFmt = new StringFormat();
            SolidBrush hBrush = null;
            SolidBrush hImageBackground = new SolidBrush(SystemColors.Control);
            Rectangle RectImage;
            Rectangle RectText;

            if (!miItem.Enabled)
            {
                // --- disabled text ---
                hBrush = new SolidBrush(SystemColors.GrayText);
            }
            else
            {
                if (bSelected)
                    hBrush = new SolidBrush(SystemColors.HighlightText);
                else
                    hBrush = new SolidBrush(SystemColors.MenuText);
            }

            RectImage = Rect;
            RectImage.Y = Rect.Y + (Rect.Height - iImage.Height) / 2;
            RectImage.X = Rect.X + 4;
            RectImage.Width = iImage.Width;
            RectImage.Height = iImage.Height;

            hStrFmt.LineAlignment = System.Drawing.StringAlignment.Center;
            RectText = Rect;
            RectText.X += RectImage.Width + 12;


            // --- start drawing the menu ---

            if (bSelected)
            {
                hGraphics.FillRectangle(SystemBrushes.Highlight, Rect);
            }
            else
            {
                hGraphics.FillRectangle(SystemBrushes.Menu, Rect);
                hGraphics.FillRectangle(hImageBackground, Rect.X, Rect.Y, RectImage.Width + 8, Rect.Height);
            }


            // --- draw image portion ---

            hGraphics.DrawImage(iImage, RectImage);


            // --- draw text ---

            hGraphics.DrawString(miItem.Text, hFont, hBrush, RectText, hStrFmt);
        }

        /// <summary>
        /// Populate the list of servers
        /// </summary>
        protected void PopulateServerList()
        {
            foreach (Server oServer in m_oServerList.Servers)
            {
                if (oServer.Status == Server.ServerStatus.OnLine || oServer.Status == Server.ServerStatus.Maintenance)
                {
                    if (!m_oValidServerList.Contains(oServer.Url)) m_oValidServerList.Add(oServer.Url, oServer);
                }

                if (!m_oFullServerList.Contains(oServer.Url)) m_oFullServerList.Add(oServer.Url, oServer);
            }
        }

        /// <summary>
        /// Load the server that was last looked at
        /// </summary>
        protected bool LoadFirstServer()
        {
            bool bRet = false;

            // --- load the server ---

            string strServerUrl = String.Empty;
            //Constant.GetSelectedServerInSettingsMeta(out strServerUrl);

            if (strServerUrl != String.Empty)
            {
                int iIndex = m_oValidServerList.IndexOfKey(strServerUrl);

                if (iIndex != -1)
                {
                    m_hResultsDocument.UpdateCatalog();
                    bRet = true;
                }
                else
                {
                    // --- did not find our pre selected server ---

                    bRet = ChooseServer();
                }
            }
            else if (m_oValidServerList.Count == 1)
            {
                foreach (Server s in m_oValidServerList.Values)
                {
                    ActivateServer(s, true);
                }
                bRet = true;
            }
            else if (m_oValidServerList.Count > 0)
            {
                bRet = ChooseServer();
            }
            //else
            //{
            //   Server hServer;
            //   if (AddServer(out hServer))
            //   {
            //      if (hServer == null)
            //      {
            //         MessageBox.Show("Unable to display dialog without a 6.2 or later versioned dap server.", "Missing valid server", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            //      } 
            //      else 
            //      {            
            //         ActivateServer(hServer, false);
            //         bRet = true;
            //      }
            //   }
            //}
            return bRet;
        }

        /// <summary>
        /// Select a server to set as the active one
        /// </summary>
        protected bool ChooseServer()
        {
            SelectServer hServer = new SelectServer();

            hServer.RemoveServer += new RemoveServerHandler(miServers_RemoveServer);
            hServer.ServerSelect += new ServerSelectHandler(miServers_SelectServer);

            hServer.ShowDialog();

            if (CurServer == null)
                return false;
            return true;
        }

        /// <summary>
        /// Activate the specified server
        /// </summary>
        /// <param name="hServer"></param>
        public void ActivateServer(Server hServer, bool bForce)
        {
            try
            {
                // --- have already selected this server ---

                if (hServer == m_oCurServer && !bForce)
                    return;

                if (hServer.Status == Server.ServerStatus.OffLine)
                {
                    //CSYS.iClearErrAP();

                    MessageBox.Show("The dap server " + hServer.Name + " is not responding.", "Dap Server Not Responding", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                    ChooseServer();
                }
                else if (hServer.Status == Server.ServerStatus.Disabled)
                {
                    //CSYS.iClearErrAP();

                    MessageBox.Show("The dap server " + hServer.Name + " is disabled.", "Dap Server Disabled", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                    ChooseServer();
                }
                else
                {
                    m_oCurServer = hServer;

                    // --- set the search array ---

                    if (m_oSearchExtents == null)
                    {
                        // --- have not yet set any search extents ---

                        if (m_oOMExtents != null)
                            m_oSearchExtents = new Geosoft.Dap.Common.BoundingBox(m_oOMExtents);
                        else
                            m_oSearchExtents = new Geosoft.Dap.Common.BoundingBox(m_oCurServer.ServerExtents);
                    }

                    // --- reproject the search extents into the dap server coordinate system ---

                    //if (!Constant.Reproject(m_oSearchExtents, m_oCurServer.ServerExtents.CoordinateSystem))
                    //{
                    //   // --- unable to reproject search area to dap server coordinate system, taking the dap server default area as our AOI ---

                    //   m_oSearchExtents = new Geosoft.Dap.Common.BoundingBox(m_oCurServer.ServerExtents);

                    //   MessageBox.Show("Unable to reproject the current area of interest into the\n\rcoordinate system defined by the selected dap server.\n\r\n\rThe area of interest will be set to the dap server's\n\r default area of interest.", "Area of interest changed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    //}

                    // --- login to this server if it is required ---

                    Login();


                    // --- remember the currently selected server ---

                    //Constant.SetSelectedServerInSettingsMeta(m_oCurServer.Url);                      
                }
            }
            catch (Exception ex)
            {
                //CGX_Util.ShowError(ex);
            }
        }

        /// <summary>
        /// Send an async request
        /// </summary>
        protected void SendAsyncRequest()
        {
            IntPtr oAP = IntPtr.Zero;
            IntPtr oGXP = IntPtr.Zero;
            IntPtr oGXX = IntPtr.Zero;
            IntPtr oINI = IntPtr.Zero;

            AsyncRequest oRequest;

            do
            {
                oRequest = (AsyncRequest)m_oAsyncQueue.Dequeue();

                // --- Safety try/catch so that a thread will not terminate, and not clean itself up ---

                try
                {
                    if (oRequest.m_eType == AsyncRequestType.GetImage)
                    {
                        // TODO : Get Image if (m_hBrowseWindow != null) m_hBrowseWindow.GetImage();
                    }
                    else if (oRequest.m_eType == AsyncRequestType.GetCatalog)
                    {
                        GetCatalog();
                    }
                    else if (oRequest.m_eType == AsyncRequestType.GetCatalogCount)
                    {
                        GetCatalogCount();
                    }
                }
                catch (Exception e)
                {
                }

            } while (oRequest.m_eType != AsyncRequestType.Stop);
        }

        /// <summary>
        /// Get the catalog
        /// </summary>
        protected void GetCatalog()
        {
            m_oCatalog = GetDapData.Instance.CurServer.CatalogCollection.GetCatalog(GetDapData.Instance.SearchExtents, GetDapData.Instance.QueryString);

            if (m_oCatalog == null)
            {
                if (m_oValidServerList.Count > 0)
                {
                    if (MessageBox.Show(this, "An error has occurred while attempting to retrieve the catalog from the dap server.\nDo you wish to disable this server?", "Failed to update catalog", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        DisableServer(GetDapData.Instance.CurServer);
                        ActivateServer((Server)m_oValidServerList.GetByIndex(0), true);
                    }
                }
                else
                {
                    MessageBox.Show(this, "An error has occurred while attempting to retrieve the catalog from the dap server.", "Failed to update catalog", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                if (m_oCatalog.ConfigurationEdition != CurServer.CacheVersion)
                {
                    Server oServer = GetDapData.Instance.CurServer;

                    // --- have to update all of our configuration information ---

                    oServer.UpdateConfiguration();
                    ActivateServer(oServer, true);
                }
            }

            if (m_hResultsDocument != null)
                m_hResultsDocument.UpdateResults(m_oCatalog);
        }

        Catalog m_oCatalog;

        public Catalog CurrentCatalog
        {
            get
            {
                return m_oCatalog;
            }
        }

        /// <summary>
        /// Get the catalog counts, for this particular query
        /// </summary>
        protected void GetCatalogCount()
        {
            foreach (Server oServer in m_oValidServerList.Values)
            {
                oServer.GetDatasetCount(SearchExtents, QueryString);
            }
        }

        /// <summary>
        /// Verify that the AOI intersects with the open map
        /// </summary>
        /// <returns></returns>
        protected bool VerifyAOI()
        {
            bool bReproject;
            bool bValid = false;

            Geosoft.Dap.Common.BoundingBox oOMBoundingBox;

            // --- only check we have an open map ---

            if (OMExtents == null) return true;


            // --- get the open map extents ---

            oOMBoundingBox = new Geosoft.Dap.Common.BoundingBox(OMExtents);


            // --- reproject to AOI coordinate system ---

            bReproject = Constant.Reproject(oOMBoundingBox, GetDapData.Instance.SearchExtents.CoordinateSystem);


            // --- see if the two boxes intersect ---

            if (bReproject)
            {
                Geosoft.Dap.Common.BoundingBox oIntersectBox = new Geosoft.Dap.Common.BoundingBox(GetDapData.Instance.SearchExtents);

                oIntersectBox.MinX = Math.Max(oOMBoundingBox.MinX, GetDapData.Instance.SearchExtents.MinX);
                oIntersectBox.MaxX = Math.Min(oOMBoundingBox.MaxX, GetDapData.Instance.SearchExtents.MaxX);
                oIntersectBox.MinY = Math.Max(oOMBoundingBox.MinY, GetDapData.Instance.SearchExtents.MinY);
                oIntersectBox.MaxY = Math.Min(oOMBoundingBox.MaxY, GetDapData.Instance.SearchExtents.MaxY);

                if (Constant.IsValidBoundingBox(oIntersectBox))
                {
                    bValid = true;
                }
            }
            return bValid;
        }

        /// <summary>
        /// Log the user into the current server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected bool Login()
        {
            bool bRet = true;

            // --- see if this server requires us to log in ---

            if (CurServer.Secure)
            {
                // --- check to see if we have already successfully logged in to this server, if so then use that user name/password ---

                if (CurServer.LoggedIn)
                {
                    CurServer.SwitchToUser();

                    miFileLogin.Visible = false;
                    miFileLogout.Visible = true;
                    miFileSeparator1.Visible = true;
                    sbpLoginStatus.Text = "Logged in";
                    bRet = true;
                }
                else
                {
                    Login oLogin = new Login();

                    bRet = false;
                    if (CurServer.Login())
                    {
                        miFileLogin.Visible = false;
                        miFileLogout.Visible = true;
                        miFileSeparator1.Visible = true;
                        sbpLoginStatus.Text = "Logged in";
                        bRet = true;
                    }
                    else
                    {
                        miFileLogin.Visible = true;
                        miFileLogout.Visible = false;
                        miFileSeparator1.Visible = true;
                        bRet = false;
                    }
                }
            }
            else
            {
                miFileLogin.Visible = false;
                miFileLogout.Visible = false;
                miFileSeparator1.Visible = false;

                // --- pick up the default user name and password so we are not passing it to a server who doesn't need it ---

                CurServer.SwitchToUser();
            }

            // --- if we are successfully logged in, then get the data from the dap server ---

            if (bRet)
            {
                EnqueueRequest(AsyncRequestType.GetCatalogCount);

                if (m_hResultsDocument != null)
                {
                    EnqueueRequest(AsyncRequestType.GetCatalog);
                    m_hResultsDocument.Activate();
                }
            }


            // --- Enable the correct windows ---

            if (m_hResultsDocument != null) m_hResultsDocument.LoggedIn(bRet);

            return bRet;
        }
        #endregion

        #region Public Members
        /// <summary>
        /// Load the default windows
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public bool SetupForm()
        {
            bool bRet = false;

            try
            {
                m_bInit = true;

                CreateResultsDocument();
                CreateSelectedDocument();

                String szDir = "";
                szDir += "GetDapData.config";
                try
                {
                    if (System.IO.File.Exists(szDir))
                    {
                        dockPanel.LoadFromXml(szDir, m_deserializeDockContent);
                    }
                }
                catch { }

                PopulateServerList();

                bRet = LoadFirstServer();
            }
            catch (Exception)
            {
                bRet = false;
            }
            finally
            {
                m_bInit = false;
            }
            return bRet;
        }

        /// <summary>
        /// Get the list of selected datasets
        /// </summary>
        /// <returns></returns>
        public ArrayList GetSelectedDatasets()
        {
            if (m_hSelectedDocument != null) return m_hSelectedDocument.GetSelectedDatasets();
            return new ArrayList();
        }


        /// <summary>
        /// Add a new server
        /// </summary>
        public bool AddServer(out Server hRetServer)
        {
            AddServer hAdd = new AddServer();
            bool bRet = false;
            hRetServer = null;

            if (hAdd.ShowDialog() == DialogResult.OK)
            {
                bRet = AddServer(hAdd.ServerUrl, out hRetServer);
            }
            return bRet;
        }

        /// <summary>
        /// Add server through its url
        /// </summary>
        /// <param name="strUrl"></param>
        /// <returns></returns>
        public bool AddServer(string strUrl, out Server hRetServer)
        {
            bool bRet;

            bRet = true;
            hRetServer = null;

            try
            {
                Cursor = System.Windows.Forms.Cursors.WaitCursor;
                string strServerUrl = strUrl;

                if (!strServerUrl.StartsWith("http://"))
                    strServerUrl = "http://" + strServerUrl;

                Server oServer = new Server(strServerUrl,m_cacheDir );

                if (oServer.Status == Server.ServerStatus.OnLine || oServer.Status == Server.ServerStatus.Maintenance)
                {
                    m_oValidServerList.Remove(oServer.Url);
                    m_oValidServerList.Add(oServer.Url, oServer);
                    hRetServer = oServer;
                }
                m_oFullServerList.Remove(oServer.Url);
                m_oFullServerList.Add(oServer.Url, oServer);

                m_oServerList.RemoveServer(oServer);
                m_oServerList.AddServer(oServer);
            }
            catch (Exception e)
            {
                Error.Write("Error adding dap server " + strUrl + " to the list.\n\r(" + e.Message + ")");
                //CSYS.iClearErrAP();
                bRet = false;
            }
            Cursor = System.Windows.Forms.Cursors.Default;
            return bRet;
        }

        /// <summary>
        /// Enable a server
        /// </summary>
        /// <param name="oServer"></param>
        public void EnableServer(Server oServer)
        {
            if (!m_oValidServerList.Contains(oServer.Url))
            {
                oServer.Status = Server.ServerStatus.OnLine;
                m_oValidServerList.Add(oServer.Url, oServer);
            }
        }

        /// <summary>
        /// Disable a server
        /// </summary>
        /// <param name="oServer"></param>
        public void DisableServer(Server oServer)
        {
            Int32 iIndex = m_oValidServerList.IndexOfKey(oServer.Url);

            if (iIndex != -1)
            {
                oServer.Status = Server.ServerStatus.Disabled;
                m_oValidServerList.RemoveAt(iIndex);
            }
        }

        /// <summary>
        /// Enqueue a request onto the queue
        /// </summary>
        /// <param name="eRequest"></param>
        public void EnqueueRequest(AsyncRequestType eRequest)
        {
            //AsyncRequest oRequest = new AsyncRequest();
            //oRequest.m_eType = eRequest;
            //oRequest.m_oParam1 = null;
            //oRequest.m_oParam2 = null;
            //m_oAsyncQueue.Enqueue(oRequest);
            EnqueueRequest(eRequest, null, null);
        }

        /// <summary>
        /// Enqueue a request onto the queue
        /// </summary>
        /// <param name="eRequest"></param>
        public void EnqueueRequest(AsyncRequestType eRequest, object oParam1, object oParam2)
        {
            AsyncRequest oRequest = new AsyncRequest();
            oRequest.m_eType = eRequest;
            oRequest.m_oParam1 = oParam1;
            oRequest.m_oParam2 = oParam2;
            if (oRequest.m_eType == AsyncRequestType.GetImage)
            {
                // TODO : Get Image if (m_hBrowseWindow != null) m_hBrowseWindow.GetImage();
            }
            else if (oRequest.m_eType == AsyncRequestType.GetCatalog)
            {
                GetCatalog();
            }
            else if (oRequest.m_eType == AsyncRequestType.GetCatalogCount)
            {
                GetCatalogCount();
            }
        }
        #endregion

        #region Event Handlers

        private void RefreshCatalog()
        {
            double vr = this.m_wwCtl.ViewRange;
            double lat = this.m_wwCtl.Latitude;
            double lon = this.m_wwCtl.Longitude;
            this.SearchExtents.MaxX = lon + vr / 2;
            this.SearchExtents.MinX = lon - vr / 2;
            this.SearchExtents.MaxY = lat + vr / 2;
            this.SearchExtents.MinY = lat - vr / 2;

            GetCatalog();
        }

        private void ShowAllCatalog()
        {
            this.SearchExtents.MaxX = 180;
            this.SearchExtents.MinX = -180;
            this.SearchExtents.MaxY = 90;
            this.SearchExtents.MinY = -90;

            GetCatalog();
        }


        /// <summary>
        /// Save the layout
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GetDapDataCtrl_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            String szDir = "";

            try
            {
                EnqueueRequest(AsyncRequestType.Stop);
                EnqueueRequest(AsyncRequestType.Stop);
                EnqueueRequest(AsyncRequestType.Stop);

                m_oServerList.Save();
            }
            catch (Exception ex)
            {
            }
        }


        private void GetDapDataCtrl_VisibleChanged(object sender, System.EventArgs e)
        {
            if (this.Visible && this.m_oValidServerList.Count < 1)
            {
                this.ChooseServer();
            }
        }

        /// <summary>
        /// Handle change in area of interest from the browse window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void m_hBrowseWindow_AOISelect(object sender, Geosoft.Dap.AOISelectArgs e)
        {
            // --- we have browsed outside of our open map, revert to old form ---

            if (!VerifyAOI())
            {
                OMExtents = null;
            }

            if (m_hResultsDocument != null) m_hResultsDocument.UpdateCatalog();

            EnqueueRequest(AsyncRequestType.GetCatalog);
            EnqueueRequest(AsyncRequestType.GetCatalogCount);
        }

        /// <summary>
        /// Refresh documents when the selected datasets change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void m_hResultsDocument_DataSetSelected(object sender, DataSetSelectedArgs e)
        {
            if (m_hSelectedDocument != null) m_hSelectedDocument.UpdateResults(e.DataSet, e.Selected);
        }

        /// <summary>
        /// Refresh documents when the selected datasets change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void m_hSelectedDocument_DataSetSelected(object sender, DataSetSelectedArgs e)
        {
        }

        /// <summary>
        /// Update the list of selected datasets
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void m_hSelectedDocument_DataSetDeleted(object sender, DataSetArgs e)
        {
            if (m_hResultsDocument != null) m_hResultsDocument.UpdateResults(e.DataSet, false);
        }

        /// <summary>
        /// Remove all selected datasets
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void m_hSelectedDocument_DataSetsDeleted(object sender, EventArgs e)
        {
            if (m_hResultsDocument != null) m_hResultsDocument.ClearSelectedDatasets();
        }

        /// <summary>
        /// Update the browser map as the order of the selected datasets
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void m_hSelectedDocument_DataSetOrderChanged(object sender, DataSetArgs e)
        {
        }

        /// <summary>
        /// Ensure browse window is visible
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void miMap_Click(object sender, System.EventArgs e)
        {
        }

        /// <summary>
        /// Ensure results window is visible
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void miResults_Click(object sender, System.EventArgs e)
        {
            m_hResultsDocument.Show(dockPanel);
        }

        /// <summary>
        /// Ensure selected datasets window is visible
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void miSelectedDatasets_Click(object sender, System.EventArgs e)
        {
            m_hSelectedDocument.Show(dockPanel);
        }

        /// <summary>
        /// Ensure get data window is visible
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void miGetData_Click(object sender, System.EventArgs e)
        {
        }

        /// <summary>
        /// Close the application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void miExit_Click(object sender, System.EventArgs e)
        {
            //Close();
        }

        /// <summary>
        /// View the meta for the selected dataset
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnViewMeta(object sender, ViewMetaArgs e)
        {
            Server oServer = (Server)GetDapData.Instance.ServerList[e.Url];
            if (oServer == null)
                MessageBox.Show("Missing " + e.Url + " in list of valid servers");
            System.Xml.XmlDocument oDoc = oServer.Command.GetMetaData(e.Name);
            string path = System.IO.Path.Combine(oServer.CacheDirectory, "temp");
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }
            path = System.IO.Path.Combine(path, e.Name + ".xml");
            System.Xml.XmlNode oXSLNode = oDoc.CreateNode(System.Xml.XmlNodeType.ProcessingInstruction, "xml-stylesheet", "");
            oXSLNode.InnerText = @"type=""text/xsl"" href=""ViewMeta.xslt""";
            oDoc.InsertAfter(oXSLNode, oDoc.ChildNodes[0]);
            oDoc.Save(path);
            MetaDataViewer form = new MetaDataViewer(path);
            form.Show();
        }

        /// <summary>
        /// Draw this menu item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void miGetData_DrawItem(object sender, System.Windows.Forms.DrawItemEventArgs e)
        {
            bool bSelected = false;
            if ((e.State & DrawItemState.Selected) != 0)
                bSelected = true;

            DrawMenuItem((MenuItem)sender, ilViewMenu.Images[GET_DATA_MENU_IMAGE], bSelected, e.Graphics, e.Bounds);
        }

        /// <summary>
        /// Measure the width of this menu item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void miGetData_MeasureItem(object sender, System.Windows.Forms.MeasureItemEventArgs e)
        {
            Int32 iWidth;
            Int32 iHeight;

            MeasureMenuItem((MenuItem)sender, ilViewMenu.Images[GET_DATA_MENU_IMAGE], e.Graphics, out iWidth, out iHeight);

            e.ItemHeight = iHeight;
            e.ItemWidth = iWidth;
        }

        /// <summary>
        /// Draw this menu item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void miSelectedDatasets_DrawItem(object sender, System.Windows.Forms.DrawItemEventArgs e)
        {
            bool bSelected = false;
            if ((e.State & DrawItemState.Selected) != 0)
                bSelected = true;

            DrawMenuItem((MenuItem)sender, ilViewMenu.Images[SELECTED_MENU_IMAGE], bSelected, e.Graphics, e.Bounds);
        }

        /// <summary>
        /// Measure the width of this menu item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void miSelectedDatasets_MeasureItem(object sender, System.Windows.Forms.MeasureItemEventArgs e)
        {
            Int32 iWidth;
            Int32 iHeight;

            MeasureMenuItem((MenuItem)sender, ilViewMenu.Images[SELECTED_MENU_IMAGE], e.Graphics, out iWidth, out iHeight);

            e.ItemHeight = iHeight;
            e.ItemWidth = iWidth;
        }

        /// <summary>
        /// Draw this menu item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void miResults_DrawItem(object sender, System.Windows.Forms.DrawItemEventArgs e)
        {
            bool bSelected = false;
            if ((e.State & DrawItemState.Selected) != 0)
                bSelected = true;

            DrawMenuItem((MenuItem)sender, ilViewMenu.Images[RESULTS_MENU_IMAGE], bSelected, e.Graphics, e.Bounds);
        }

        /// <summary>
        /// Measure the width of this menu item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void miResults_MeasureItem(object sender, System.Windows.Forms.MeasureItemEventArgs e)
        {
            Int32 iWidth;
            Int32 iHeight;

            MeasureMenuItem((MenuItem)sender, ilViewMenu.Images[RESULTS_MENU_IMAGE], e.Graphics, out iWidth, out iHeight);

            e.ItemHeight = iHeight;
            e.ItemWidth = iWidth;
        }

        /// <summary>
        /// Draw this menu item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void miMap_DrawItem(object sender, System.Windows.Forms.DrawItemEventArgs e)
        {
            bool bSelected = false;
            if ((e.State & DrawItemState.Selected) != 0)
                bSelected = true;

            DrawMenuItem((MenuItem)sender, ilViewMenu.Images[BROWSE_MENU_IMAGE], bSelected, e.Graphics, e.Bounds);
        }

        /// <summary>
        /// Measure the width of this menu item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void miMap_MeasureItem(object sender, System.Windows.Forms.MeasureItemEventArgs e)
        {
            Int32 iWidth;
            Int32 iHeight;

            MeasureMenuItem((MenuItem)sender, ilViewMenu.Images[BROWSE_MENU_IMAGE], e.Graphics, out iWidth, out iHeight);

            e.ItemHeight = iHeight;
            e.ItemWidth = iWidth;
        }

        /// <summary>
        /// Change the current server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void m_hBrowseWindow_ServerSelect(object sender, ServerSelectArgs e)
        {
            if (e.Index < 0)
            {
                return;
            }

            Server hServer = (Server)m_oValidServerList.GetByIndex(e.Index);
            ActivateServer(hServer, false);
        }

        /// <summary>
        /// Display the servers dialog
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void miServers_Click(object sender, System.EventArgs e)
        {
            ChooseServer();
        }

        /// <summary>
        /// Select the server from the list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void miServers_SelectServer(object sender, ServerSelectArgs e)
        {
            Server hServer = (Server)m_oFullServerList.GetByIndex(e.Index);

            Int32 iIndex = m_oValidServerList.IndexOfKey(hServer.Url);

            if (iIndex != -1)
            {
                ActivateServer(hServer, false);
            }
        }

        /// <summary>
        /// Remove the server from the list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void miServers_RemoveServer(object sender, ServerSelectArgs e)
        {
            Server hServer = (Server)m_oFullServerList.GetByIndex(e.Index);

            Int32 iIndex = m_oValidServerList.IndexOfKey(hServer.Url);

            if (iIndex != -1)
            {
                m_oValidServerList.RemoveAt(iIndex);
            }
            m_oFullServerList.Remove(hServer.Url);
            m_oServerList.RemoveServer(hServer);
        }

        /// <summary>
        /// Launch the help document for the dap get data dialog
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void miHelp_Click(object sender, System.EventArgs e)
        {
        }

        /// <summary>
        /// Login into the current server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void miFileLogin_Click(object sender, System.EventArgs e)
        {
            Login();
        }

        /// <summary>
        /// Logout from the current secure server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void miFileLogout_Click(object sender, System.EventArgs e)
        {
            miFileLogin.Visible = true;
            miFileLogout.Visible = false;
            miFileSeparator1.Visible = true;

            CurServer.Logout();

            // --- Enable the correct windows ---

            if (m_hResultsDocument != null) m_hResultsDocument.LoggedIn(false);
        }
        #endregion

        private System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            AppDomain domain = (AppDomain)sender;

            foreach (System.Reflection.Assembly asm in domain.GetAssemblies())
            {
                if (asm.FullName == args.Name)
                    return asm;
            }
            return null;
        }

        // ADDED FOR CONVERSION TO USER CONTROL

        public MainMenu Menu
        {
            get
            {
                return this.mMain;
            }
            set
            {
                this.mMain = value;
            }
        }

        public void EditLayerSettings(string layerTitle)
        {
            DAPLayerSettings oForm = new DAPLayerSettings(this.m_wwCtl, layerTitle);
            oForm.Enabled = true;
            oForm.ShowDialog(this);
        }

    }
}
