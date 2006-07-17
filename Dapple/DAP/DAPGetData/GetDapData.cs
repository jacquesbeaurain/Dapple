using System;
using System.Text;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Runtime.InteropServices;

using Geosoft.Dap;
using Geosoft.GXNet;

namespace Geosoft.GX.DAPGetData
{
	/// <summary>
	/// Summary description for GetDapData.
	/// </summary>
	public class GetDapData : System.Windows.Forms.Form
	{
      #region Constants
      protected const Int32                  BROWSE_MENU_IMAGE = 0;
      protected const Int32                  SEARCH_MENU_IMAGE = 1;
      protected const Int32                  RESULTS_MENU_IMAGE = 2;
      protected const Int32                  SELECTED_MENU_IMAGE = 3;
      protected const Int32                  GET_DATA_MENU_IMAGE = 4;
      #endregion

      #region enum 
      public struct AsyncRequest
      {
         public AsyncRequestType m_eType;
         public object           m_oParam1;
         public object           m_oParam2;
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
      static GetDapData                      m_hThis = null;
      
      protected bool                         m_bInit = false;

      protected String                               m_strQueryString = String.Empty;
      protected Server                               m_oCurServer;
      protected ServerList                           m_oServerList;
      protected SortedList                           m_oValidServerList;
      protected SortedList                           m_oFullServerList;
      protected Geosoft.Dap.Common.BoundingBox       m_oOMExtents = null;
      protected Geosoft.Dap.Common.BoundingBox       m_oSearchExtents = null;
      protected Geosoft.Dap.Common.BoundingBox       m_oViewExtents = null;
            
      protected Geosoft.GXNet.CGX_NET        m_hGxNet; 
      protected int                          m_iGxHandle;
      protected bool                         m_bGeodist = false;
      protected bool                         m_bMapInfo = false;
      protected bool                         m_bArcGIS = false;

      private BrowseWindow                   m_hBrowseWindow;      

      private ResultsDocument                m_hResultsDocument;
      private SelectedDocument               m_hSelectedDocument;
      private GetDataDocument                m_hGetDataDocument;

      protected GetDapError                  m_hError;

      protected DotNetTools.Common.Queue     m_oAsyncQueue;
      protected System.Threading.Thread      m_oAsyncThread1;
      protected System.Threading.Thread      m_oAsyncThread2;
      protected System.Threading.Thread      m_oAsyncThread3;
      #endregion

      private Geosoft.WinFormsUI.DockPanel               dockPanel;      
      private Geosoft.WinFormsUI.DeserializeDockContent  m_deserializeDockContent;
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
      /// Get/Set the browser map view extents
      /// </summary>
      public Geosoft.Dap.Common.BoundingBox ViewExtents
      {
         get { return m_oViewExtents; }
         set { m_oViewExtents = value; }
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
		public GetDapData()
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
         m_oViewExtents = new Geosoft.Dap.Common.BoundingBox();
         m_oValidServerList = new SortedList();
         m_oFullServerList = new SortedList();         

         m_hThis = this;         

         m_oAsyncQueue = new DotNetTools.Common.Queue();         
		}                     

      /// <summary>
      /// Constructor called from within OM
      /// </summary>
      /// <param name="p"></param>
      public GetDapData(int p) : this()
      {
         m_iGxHandle = p;
         m_hGxNet = new CGX_NET((IntPtr)p);

         // --- See if we are outside montaj ---

         m_bGeodist = CSYS.iGetGeodist() != 0;
         m_bMapInfo = DatasetOptions.bIsMapInfo();
         m_bArcGIS = DatasetOptions.bIsArcGIS();


         // --- must do after we initialize the gx pointer ---

         m_oAsyncThread1 = new System.Threading.Thread(new System.Threading.ThreadStart(SendAsyncRequest));
         m_oAsyncThread1.Start();

         m_oAsyncThread2 = new System.Threading.Thread(new System.Threading.ThreadStart(SendAsyncRequest));
         m_oAsyncThread2.Start();

         m_oAsyncThread3 = new System.Threading.Thread(new System.Threading.ThreadStart(SendAsyncRequest));
         m_oAsyncThread3.Start();


         // --- Create the new map ---

         string strWorkDir = string.Empty;
         if (m_bMapInfo)
         {
            CSYS.GtString("MAPINFO_DAP_CLIENT", "WORKING_DIR", ref strWorkDir);
         }
         else 
         {
            CSYS.IGetDirectory(Geosoft.GXNet.Constant.SYS_DIR_LOCAL, ref strWorkDir);            
         }
         m_hError = new GetDapError(System.IO.Path.Combine(strWorkDir, "_GetDapDataError.log"));

         m_oServerList = new ServerList();         
      }

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
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
         this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
         this.BackColor = System.Drawing.Color.LightSteelBlue;
         this.ClientSize = new System.Drawing.Size(1104, 645);
         this.Controls.Add(this.dockPanel);
         this.Controls.Add(this.sbStatus);
         this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
         this.Menu = this.mMain;
         this.Name = "GetDapData";
         this.ShowInTaskbar = false;
         this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
         this.Text = "Get DAP Data";
         this.Closing += new System.ComponentModel.CancelEventHandler(this.GetDapData_Closing);
         ((System.ComponentModel.ISupportInitialize)(this.sbpLoginStatus)).EndInit();
         this.ResumeLayout(false);

      }
		#endregion

      #region Entry Method
      /// <summary>
      /// Create the legend explorer
      /// </summary>
      [CGXAttribute("")]
      public void Run()
      {         
         Geosoft.GXNet.CSYS.SetServerMessagesAP(0);
         if (SetupForm())
            this.ShowDialog();                         
         Geosoft.GXNet.CSYS.SetServerMessagesAP(1);
         Geosoft.GXNet.CSYS.iClearErrAP();
      }
      #endregion

      #region Protected Member Functions
      protected Geosoft.WinFormsUI.DockContent GetContentFromPersistString(string persistString)
      {
         if (persistString == typeof(BrowseWindow).ToString())
            return m_hBrowseWindow;
         else if (persistString == typeof(GetDataDocument).ToString())
            return m_hGetDataDocument;
         else if (persistString == typeof(ResultsDocument).ToString())
            return m_hResultsDocument;
         else if (persistString == typeof(SelectedDocument).ToString())
            return m_hSelectedDocument;         

         return null;
      }

      /// <summary>
      /// Create the browse window pane
      /// </summary>
      protected void CreateBrowseWindow()
      {
         m_hBrowseWindow = new BrowseWindow();
         m_hBrowseWindow.HideOnClose = true;
         m_hBrowseWindow.DockableAreas = Geosoft.WinFormsUI.DockAreas.DockLeft | Geosoft.WinFormsUI.DockAreas.DockRight | Geosoft.WinFormsUI.DockAreas.DockTop | Geosoft.WinFormsUI.DockAreas.DockBottom;
         m_hBrowseWindow.AOISelect += new Geosoft.Dap.AOISelectHandler(this.m_hBrowseWindow_AOISelect); 
         m_hBrowseWindow.ServerSelect += new ServerSelectHandler(m_hBrowseWindow_ServerSelect);
      }      

      protected void CreateResultsDocument()
      {
         m_hResultsDocument = new ResultsDocument();
         m_hResultsDocument.HideOnClose = true;
         m_hResultsDocument.DockableAreas = Geosoft.WinFormsUI.DockAreas.Document;
         m_hResultsDocument.DataSetSelected += new DataSetSelectedHandler(m_hResultsDocument_DataSetSelected);    
         m_hResultsDocument.ViewMeta += new ViewMetaHandler(OnViewMeta);
      }

      protected void CreateSelectedDocument()
      {
         m_hSelectedDocument = new SelectedDocument();
         m_hSelectedDocument.HideOnClose = true;
         m_hSelectedDocument.DockableAreas = Geosoft.WinFormsUI.DockAreas.Document;
         m_hSelectedDocument.DataSetSelected += new DataSetSelectedHandler(m_hSelectedDocument_DataSetSelected);         
         m_hSelectedDocument.ViewMeta += new ViewMetaHandler(OnViewMeta);
         m_hSelectedDocument.DataSetDeleted += new DataSetDeletedHandler(m_hSelectedDocument_DataSetDeleted);
         m_hSelectedDocument.DataSetsDeleted += new EventHandler(m_hSelectedDocument_DataSetsDeleted);
         m_hSelectedDocument.DataSetPromoted += new DataSetPromotedHandler(m_hSelectedDocument_DataSetOrderChanged);
         m_hSelectedDocument.DataSetDemoted += new DataSetDemotedHandler(m_hSelectedDocument_DataSetOrderChanged);
      }

      protected void CreateGetDataDocument()
      {
         m_hGetDataDocument = new GetDataDocument();      
         m_hGetDataDocument.HideOnClose = true;
         m_hGetDataDocument.DockableAreas = Geosoft.WinFormsUI.DockAreas.Document;
         m_hGetDataDocument.ViewMeta += new ViewMetaHandler(OnViewMeta);
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

         Font           hFont = SystemInformation.MenuFont;
         StringFormat   hStrFmt = new StringFormat();

         
         // --- measure the text length ---

         SizeF          hSizeF = hGraphics.MeasureString(miItem.Text, hFont, 1000, hStrFmt);

         
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
         Font           hFont = SystemInformation.MenuFont;
         StringFormat   hStrFmt = new StringFormat();
         SolidBrush     hBrush = null;
         SolidBrush     hImageBackground = new SolidBrush(SystemColors.Control);
         Rectangle      RectImage;
         Rectangle      RectText;

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
         RectImage.Y = Rect.Y + (Rect.Height - iImage.Height)/2;
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

         m_hBrowseWindow.PopulateServerList();
      }

      /// <summary>
      /// Load the server that was last looked at
      /// </summary>
      protected bool LoadFirstServer()
      {
         bool  bRet = false;

         // --- load the server ---

         string strServerUrl = String.Empty;
         Constant.GetSelectedServerInSettingsMeta(out strServerUrl);

         if (strServerUrl != String.Empty)
         {
            int   iIndex = m_oValidServerList.IndexOfKey(strServerUrl);
            
            if (iIndex != -1)
            {
               m_hBrowseWindow.SelectServer(iIndex);
               m_hResultsDocument.UpdateCatalog();
               bRet = true;
            } 
            else 
            {            
               // --- did not find our pre selected server ---
            
               bRet = ChooseServer();            
            }
         } 
         else if (m_oValidServerList.Count > 0)
         {
            bRet = ChooseServer();
         }
         else
         {
            Server hServer;
            if (AddServer(out hServer))
            {
               if (hServer == null)
               {
                  MessageBox.Show("Unable to display dialog without a 6.2 or later versioned dap server.", "Missing valid server", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
               } 
               else 
               {            
                  ActivateServer(hServer, false);
                  bRet = true;
               }
            }
         }
         return bRet;
      }

      /// <summary>
      /// Select a server to set as the active one
      /// </summary>
      protected bool ChooseServer()
      {
         SelectServer   hServer = new SelectServer();

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
      protected void ActivateServer(Server hServer, bool bForce)
      {
         try
         {            
            // --- have already selected this server ---

            if (hServer == m_oCurServer && !bForce)
               return;

            if (hServer.Status == Server.ServerStatus.OffLine)
            {
               CSYS.iClearErrAP();

               MessageBox.Show("The dap server " + hServer.Name + " is not responding.", "Dap Server Not Responding", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

               ChooseServer();
            } 
            else if (hServer.Status == Server.ServerStatus.Disabled)
            {
               CSYS.iClearErrAP();

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

               if (!Constant.Reproject(m_oSearchExtents, m_oCurServer.ServerExtents.CoordinateSystem))
               {
                  // --- unable to reproject search area to dap server coordinate system, taking the dap server default area as our AOI ---

                  m_oSearchExtents = new Geosoft.Dap.Common.BoundingBox(m_oCurServer.ServerExtents);

                  MessageBox.Show("Unable to reproject the current area of interest into the\n\rcoordinate system defined by the selected dap server.\n\r\n\rThe area of interest will be set to the dap server's\n\r default area of interest.", "Area of interest changed", MessageBoxButtons.OK, MessageBoxIcon.Information);
               }

               // --- login to this server if it is required ---

               Login();                                                                  
            }
         }
         catch (Exception ex)
         {
            CGX_Util.ShowError(ex);
         }
      }

#if DEBUG
      const string GEOLIB = "geolibd.dll";
#else 
      const string GEOLIB = "geolib.dll";
#endif
      [DllImport(GEOLIB, EntryPoint="hCreat_AP",CallingConvention=CallingConvention.Cdecl)]      
      extern static IntPtr hCreat_AP(string strApplication, string strVersion);

      [DllImport(GEOLIB, EntryPoint="Destr_AP",CallingConvention=CallingConvention.Cdecl)]      
      extern static void Destr_AP(IntPtr oAP);

      [DllImport(GEOLIB, EntryPoint="hCreat_INI",CallingConvention=CallingConvention.Cdecl)]      
      extern static IntPtr hCreat_INI(IntPtr oAP);

      [DllImport(GEOLIB, EntryPoint="Destr_INI",CallingConvention=CallingConvention.Cdecl)]      
      extern static void Destr_INI(IntPtr oINI);

      [DllImport(GEOLIB, EntryPoint="hCreat_GXP",CallingConvention=CallingConvention.Cdecl)]      
      extern static IntPtr hCreat_GXP(IntPtr oAP, IntPtr oINI);

      [DllImport(GEOLIB, EntryPoint="Destr_GXP",CallingConvention=CallingConvention.Cdecl)]      
      extern static IntPtr Destr_GXP(IntPtr oGXP);

      [DllImport(GEOLIB, EntryPoint="hCreatExtern_GXX",CallingConvention=CallingConvention.Cdecl)]      
      extern static IntPtr hCreatExtern_GXX(IntPtr oGXP);

      [DllImport(GEOLIB, EntryPoint="Destr_GXX",CallingConvention=CallingConvention.Cdecl)]      
      extern static IntPtr Destr_GXX(IntPtr oGXX);

      /// <summary>
      /// Send an async request
      /// </summary>
      protected void SendAsyncRequest()
      {
         IntPtr   oAP = IntPtr.Zero;
         IntPtr   oGXP = IntPtr.Zero;
         IntPtr   oGXX = IntPtr.Zero;
         IntPtr   oINI = IntPtr.Zero;
         CGX_NET  oGxNet;

         AsyncRequest oRequest;

         lock (this)
         {
            if (m_bGeodist)
            {
               oGxNet = new CGX_NET("GetDapData", "1.0", 0, 0, 0);
            } 
            else 
            {
               oAP = hCreat_AP(System.Reflection.Assembly.GetExecutingAssembly().GetName().Name, System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
               if (oAP == IntPtr.Zero) return;

               oINI = hCreat_INI(oAP);
               if (oINI == IntPtr.Zero) return;

               oGXP = hCreat_GXP(oAP, oINI);
               if (oGXP == IntPtr.Zero) return;

               oGXX = hCreatExtern_GXX(oGXP);
               if (oGXX == IntPtr.Zero) return;

               oGxNet = new CGX_NET(oGXX);
            }
         }

         // --- do not display server error messages ---

         CSYS.SetServerMessagesAP(0);

         do
         {
            oRequest = (AsyncRequest)m_oAsyncQueue.Dequeue();
            
            // --- Safety try/catch so that a thread will not terminate, and not clean itself up ---

            try
            {               
               if (oRequest.m_eType == AsyncRequestType.GetImage) 
               {
                  if (m_hBrowseWindow != null) m_hBrowseWindow.GetImage();
               }
               else if (oRequest.m_eType == AsyncRequestType.GetCatalog)
               {
                  GetCatalog();
               }
               else if (oRequest.m_eType == AsyncRequestType.GetCatalogCount)
               {
                  GetCatalogCount();
               }
               else if (oRequest.m_eType == AsyncRequestType.SetupExtract)
               {
                  if (m_hGetDataDocument != null) m_hGetDataDocument.SetupDataset((Geosoft.Dap.Common.DataSet)oRequest.m_oParam1);
               }
            } 
            catch (Exception e)
            {
               m_hError.Write(string.Format("Failed to send request {0} - {1}", oRequest.m_eType.ToString(), e.Message));
            }

         } while (oRequest.m_eType != AsyncRequestType.Stop);

         oGxNet.Dispose();
         if (oGXX != IntPtr.Zero) Destr_GXX(oGXX);
         if (oGXP != IntPtr.Zero) Destr_GXP(oGXP);
         if (oINI != IntPtr.Zero) Destr_INI(oINI);
         if (oAP != IntPtr.Zero) Destr_AP(oAP);
      }

      /// <summary>
      /// Get the catalog
      /// </summary>
      protected void GetCatalog()
      {         
         Catalog oCatalog = GetDapData.Instance.CurServer.CatalogCollection.GetCatalog(GetDapData.Instance.SearchExtents, GetDapData.Instance.QueryString);

         if (oCatalog == null)
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
            if (oCatalog.ConfigurationEdition != CurServer.CacheVersion)
            {
               Server oServer = GetDapData.Instance.CurServer;

               // --- have to update all of our configuration information ---

               oServer.UpdateConfiguration();
               ActivateServer(oServer, true);
            }
         }

         if (m_hResultsDocument != null) m_hResultsDocument.UpdateResults(oCatalog);
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

         if (m_hBrowseWindow != null) m_hBrowseWindow.UpdateCounts();
         if (m_hResultsDocument != null) m_hResultsDocument.UpdateCounts();
      }

      /// <summary>
      /// Verify that the AOI intersects with the open map
      /// </summary>
      /// <returns></returns>
      protected bool VerifyAOI()
      {
         bool  bReproject;
         bool  bValid = false;

         Geosoft.Dap.Common.BoundingBox   oOMBoundingBox;

         // --- only check we have an open map ---

         if (OMExtents == null) return true;


         // --- get the open map extents ---

         oOMBoundingBox = new Geosoft.Dap.Common.BoundingBox(OMExtents);

         
         // --- reproject to AOI coordinate system ---

         bReproject = Constant.Reproject(oOMBoundingBox, GetDapData.Instance.SearchExtents.CoordinateSystem);

         
         // --- see if the two boxes intersect ---

         if (bReproject)
         {
            Geosoft.Dap.Common.BoundingBox   oIntersectBox = new Geosoft.Dap.Common.BoundingBox(GetDapData.Instance.SearchExtents);
            
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

         // --- ensure we only attempt to login 1 at a time ---

         lock (this)
         {
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
               if (m_hBrowseWindow != null) 
               {
                  m_hBrowseWindow.Init();
                  m_hBrowseWindow.SelectServer(m_oValidServerList.IndexOfKey(m_oCurServer.Url));
                  EnqueueRequest(AsyncRequestType.GetCatalogCount);
               }

               if (m_hResultsDocument != null) 
               { 
                  EnqueueRequest(AsyncRequestType.GetCatalog);                  
                  m_hResultsDocument.Activate();
               }
               
               if (m_hBrowseWindow != null) m_hBrowseWindow.Activate();
            }


            // --- Enable the correct windows ---

            if (m_hResultsDocument != null) m_hResultsDocument.LoggedIn(bRet);
            if (m_hBrowseWindow != null) m_hBrowseWindow.LoggedIn(bRet);
         }

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
         Int32 iHeight;
         Int32 iWidth;
         bool  bRet = true;
         try
         {
            m_bInit = true;

            CreateBrowseWindow();         
            CreateResultsDocument();
            CreateSelectedDocument();
            CreateGetDataDocument();                
                        
            String                     szDir = "";
            bool                       bOpenExisting = false;

            Geosoft.GXNet.CSYS.IGetDirectory(Geosoft.GXNet.Constant.SYS_DIR_LOCAL, ref szDir);
            szDir += "GetDapData.config";         


            try
            {
               if (System.IO.File.Exists(szDir))
               {
                  dockPanel.LoadFromXml(szDir, m_deserializeDockContent);
                  bOpenExisting = true;
               }
            } 
            catch {}
            
            if (!bOpenExisting)
            {
               m_hBrowseWindow.Show(dockPanel, Geosoft.WinFormsUI.DockState.DockLeft);
               m_hResultsDocument.Show(dockPanel);
               m_hSelectedDocument.Show(dockPanel);
               m_hGetDataDocument.Show(dockPanel);
               dockPanel.DockLeftPortion = 0.45;
            }
            
            
            // --- clear mapinfo file ---

            if (m_bMapInfo)
            {
               DatasetOptions.ClearMapInfoFileList();
            }
            
            // --- get the area of interest from oasis ---

            double dMinX = Geosoft.GXNet.Constant.rDUMMY;
            double dMinY = Geosoft.GXNet.Constant.rDUMMY;
            double dMaxX = Geosoft.GXNet.Constant.rDUMMY;
            double dMaxY = Geosoft.GXNet.Constant.rDUMMY;

            String strProjectionName = String.Empty;
            String strDatum = String.Empty;
            String strProjection = String.Empty;
            String strUnits = String.Empty;
            String strLocalDatum = String.Empty;

            Int32 iAOISet = CSYS.iGetInt("DAPGETDATA", "AOI_SET");

            if (iAOISet == 1)
            {               
               dMinX = CSYS.rGetReal("DAPGETDATA", "AOI_MINX");
               dMinY = CSYS.rGetReal("DAPGETDATA", "AOI_MINY");
               dMaxX = CSYS.rGetReal("DAPGETDATA", "AOI_MAXX");
               dMaxY = CSYS.rGetReal("DAPGETDATA", "AOI_MAXY");

               CSYS.GtString("DAPGETDATA", "AOI_IPJ_PROJ", ref strProjectionName);
               CSYS.GtString("DAPGETDATA", "AOI_IPJ_DATUM", ref strDatum);
               CSYS.GtString("DAPGETDATA", "AOI_IPJ_METHOD", ref strProjection);
               CSYS.GtString("DAPGETDATA", "AOI_IPJ_LOCAL_DATUM", ref strLocalDatum);
               CSYS.GtString("DAPGETDATA", "AOI_IPJ_UNITS", ref strUnits);

               CIPJ hIPJ = CIPJ.Create();
               hIPJ.SetGXF(strProjectionName, strDatum, strProjection, strUnits, strLocalDatum);               
               hIPJ.Dispose();

               if (dMinX != Geosoft.GXNet.Constant.rDUMMY && 
                  dMinY != Geosoft.GXNet.Constant.rDUMMY && 
                  dMaxX != Geosoft.GXNet.Constant.rDUMMY &&
                  dMaxY != Geosoft.GXNet.Constant.rDUMMY)
               {
                  m_oOMExtents = new Geosoft.Dap.Common.BoundingBox(dMaxX, dMaxY, dMinX, dMinY);
                  m_oOMExtents.CoordinateSystem = new Geosoft.Dap.Common.CoordinateSystem();
                  m_oOMExtents.CoordinateSystem.Projection = strProjectionName;
                  m_oOMExtents.CoordinateSystem.Datum = strDatum;
                  m_oOMExtents.CoordinateSystem.Method = strProjection;
                  m_oOMExtents.CoordinateSystem.Units = strUnits;
                  m_oOMExtents.CoordinateSystem.LocalDatum = strLocalDatum;                          
               }
            }
            else
            {
               CIPJ hIPJ = CIPJ.Create();
               CPLY hPLY = CPLY.Create();

               Int32 iRet = CGUI.iGetAreaOfInterest(this, ref dMinX, ref dMinY, ref dMaxX, ref dMaxY, hPLY, hIPJ);
               if (iRet == Geosoft.GXNet.Constant.AOI_RETURN_DEFINE)
                  m_oOMExtents = Constant.SetCoordinateSystem(dMinX, dMinY, dMaxX, dMaxY, hIPJ);                  
               else if (iRet == Geosoft.GXNet.Constant.AOI_RETURN_CANCEL)
                  bRet = false;

               hIPJ.Dispose();
               hPLY.Dispose();
            }
         
            if (bRet)
            {
               Constant.GetSizeInSettingsMeta(out iWidth, out iHeight);
            
               if (iHeight > 0) Height = iHeight;
               if (iWidth > 0) Width = iWidth;

               if (m_oOMExtents != null) m_hGetDataDocument.SetMapState(true);
                     
               PopulateServerList();

               bRet = LoadFirstServer();            
            }
         } 
         catch (Exception ex)
         {
            CGX_Util.ShowError(ex);            
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
         AddServer   hAdd = new AddServer();
         bool        bRet = false;
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
         bool  bRet;

         bRet = true;
         hRetServer = null;

         try
         {
            Cursor = System.Windows.Forms.Cursors.WaitCursor;
            string   strServerUrl = strUrl;
               
            if (!strServerUrl.StartsWith("http://"))
               strServerUrl = "http://" + strServerUrl;

            Server oServer = new Server(strServerUrl);               

            if (oServer.Status == Server.ServerStatus.OnLine || oServer.Status == Server.ServerStatus.Maintenance)
            {
               m_oValidServerList.Remove(oServer.Url);
               m_oValidServerList.Add(oServer.Url, oServer);
               m_hBrowseWindow.PopulateServerList();
               hRetServer = oServer;                     
            }
            m_oFullServerList.Remove(oServer.Url);
            m_oFullServerList.Add(oServer.Url, oServer);

            m_oServerList.RemoveServer(oServer);
            m_oServerList.AddServer(oServer);   
         } 
         catch (Exception e)
         {
            m_hError.Write("Error adding dap server " + strUrl + " to the list.\n\r(" + e.Message + ")");
            CSYS.iClearErrAP();
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
            m_hBrowseWindow.PopulateServerList();
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
            m_hBrowseWindow.PopulateServerList();
         }
      }

      /// <summary>
      /// Enqueue a request onto the queue
      /// </summary>
      /// <param name="eRequest"></param>
      public void EnqueueRequest(AsyncRequestType eRequest)
      {
         AsyncRequest oRequest = new AsyncRequest();
         oRequest.m_eType = eRequest;
         oRequest.m_oParam1 = null;
         oRequest.m_oParam2 = null;
         m_oAsyncQueue.Enqueue(oRequest);
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
         m_oAsyncQueue.Enqueue(oRequest);
      }      

      /// <summary>
      /// Zoom to a specified box
      /// </summary>
      /// <param name="oBox"></param>
      public void ZoomTo(Geosoft.Dap.Common.BoundingBox oBox)
      {
         if (m_hBrowseWindow != null)
            m_hBrowseWindow.ZoomTo(oBox);
      }
      #endregion

      #region Event Handlers
      /// <summary>
      /// Save the layout
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void GetDapData_Closing(object sender, System.ComponentModel.CancelEventArgs e)
      {
         String                     szDir = "";

         try
         {
            EnqueueRequest(AsyncRequestType.Stop);
            EnqueueRequest(AsyncRequestType.Stop);
            EnqueueRequest(AsyncRequestType.Stop);

            Geosoft.GXNet.CSYS.IGetDirectory(Geosoft.GXNet.Constant.SYS_DIR_LOCAL, ref szDir);
            szDir += "GetDapData.config";         
            dockPanel.SaveAsXml(szDir);

            Constant.SetSizeInSettingsMeta(Width, Height);            
            
            m_oServerList.Save();            

            m_hError.Dispose();
         }
         catch (Exception ex)
         {
            CGX_Util.ShowError(ex);
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
            if (m_hGetDataDocument != null) m_hGetDataDocument.SetMapState(false);
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
         if (m_hGetDataDocument != null) m_hGetDataDocument.UpdateGetData(e.DataSet, e.Selected);     
         if (m_hBrowseWindow != null) m_hBrowseWindow.SelectedDatasetChanged();
      }

      /// <summary>
      /// Refresh documents when the selected datasets change
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void m_hSelectedDocument_DataSetSelected(object sender, DataSetSelectedArgs e)
      {
         if (m_hBrowseWindow != null) m_hBrowseWindow.SelectedDatasetChanged();
      }      

      /// <summary>
      /// Update the list of selected datasets
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void m_hSelectedDocument_DataSetDeleted(object sender, DataSetArgs e)
      {
         if (m_hResultsDocument != null) m_hResultsDocument.UpdateResults(e.DataSet, false);
         if (m_hGetDataDocument != null) m_hGetDataDocument.UpdateGetData(e.DataSet, false);     
         if (m_hBrowseWindow != null) m_hBrowseWindow.SelectedDatasetChanged();
      }

      /// <summary>
      /// Remove all selected datasets
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void m_hSelectedDocument_DataSetsDeleted(object sender, EventArgs e)
      {
         if (m_hResultsDocument != null) m_hResultsDocument.ClearSelectedDatasets();
         if (m_hGetDataDocument != null) m_hGetDataDocument.ClearSelectedDatasets();     
         if (m_hBrowseWindow != null) m_hBrowseWindow.SelectedDatasetChanged();
      }

      /// <summary>
      /// Update the browser map as the order of the selected datasets
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void m_hSelectedDocument_DataSetOrderChanged(object sender, DataSetArgs e)
      {
         if (m_hBrowseWindow != null) m_hBrowseWindow.SelectedDatasetChanged();
      }

      /// <summary>
      /// Ensure browse window is visible
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void miMap_Click(object sender, System.EventArgs e)
      {
         m_hBrowseWindow.Show(dockPanel);
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
         m_hGetDataDocument.Show(dockPanel);
      }

      /// <summary>
      /// Close the application
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void miExit_Click(object sender, System.EventArgs e)
      {
         Close();
      }      

      /// <summary>
      /// View the meta for the selected dataset
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void OnViewMeta(object sender, ViewMetaArgs e)
      {     
         Geosoft.GXNet.CDAP   hDAP = null;
         Geosoft.GXNet.CMETA  hMETA = null;
         Geosoft.GXNet.CMETA  hDataSetMETA = null;
         Int32                iToken = GXNet.Constant.H_META_INVALID_TOKEN;
         Int32                iAttribToken = GXNet.Constant.H_META_INVALID_TOKEN;
         Int32                iClassToken = GXNet.Constant.H_META_INVALID_TOKEN;
         

         try
         {
            Server oServer = (Server)GetDapData.Instance.ServerList[e.Url];

            if (oServer == null) 
               MessageBox.Show("Missing " + e.Url + " in list of valid servers");

            hDAP = Geosoft.GXNet.CDAP.Create(oServer.MetaUrl, "");

            hMETA = hDAP.DescribeDataSet(e.Name);         

            iClassToken = hMETA.ResolveUMN("CLASS:/Geosoft/Core/DAP/Data/DatasetInfo");
            iAttribToken = hMETA.ResolveUMN("ATTRIB:/Geosoft/Core/DAP/Data/DatasetInfo/Information");

            // --- Get the META to display ---

            hDataSetMETA = Geosoft.GXNet.CMETA.Create();
         
            try
            {
               hMETA.GetAttribOBJ(iClassToken, iAttribToken, hDataSetMETA);            
               Geosoft.GXNet.CGUI.MetaDataViewer(this, hDataSetMETA, iToken, Geosoft.GXNet.Constant.GS_FALSE);  
            } 
            catch
            {
               Geosoft.GXNet.CGUI.MetaDataViewer(this, hMETA, iToken, Geosoft.GXNet.Constant.GS_FALSE);  
            }
         }
         catch (Exception ex)
         {
            CGX_Util.ShowError(ex);
         }

       
         if (hDataSetMETA != null) hDataSetMETA.Dispose();
         if (hMETA != null) hMETA.Dispose();
         if (hDAP != null) hDAP.Dispose();         
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
            // --- remember the currently selected server ---

            Constant.SetSelectedServerInSettingsMeta(hServer.Url);

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
            m_hBrowseWindow.PopulateServerList();
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
         try
         {
            Geosoft.GXNet.CSYS.DisplayHelp("geogxnet","Geosoft_GX_DAPGetData_GetDapData_Run");
         }
         catch(Exception ex)
         {
            MessageBox.Show("Error launching help topic for get dap data. (" + ex.Message + ")");
         }
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
         if (m_hBrowseWindow != null) m_hBrowseWindow.LoggedIn(false);
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
   }
}
