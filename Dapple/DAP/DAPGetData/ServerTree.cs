using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;

using Geosoft.Dap;
using Geosoft.Dap.Common;
using Geosoft.DotNetTools;

using Resources = global::Dapple.Properties.Resources;

namespace Geosoft.GX.DAPGetData
{
   public class ServerTree : TreeView
   {
      #region Enums
      public enum SearchModeEnum
      {
         All,
         Name,
         Description,
         Keywords
      }

      public struct AsyncRequest
      {
         public bool m_bValid;
         public AsyncRequestType m_eType;
         public object m_oParam1;
         public object m_oParam2;
         public object m_oParam3;
         public object m_oParam4;
         public object m_oParam5;
      }

      public enum AsyncRequestType
      {
         FilterChanged,
         GetCatalogHierarchy,
         GetDatasetList,
         Stop
      }
      #endregion

      #region Members
      protected string m_strCacheDir;

      //ContextMenuStrip m_oContextMenuStrip;
      protected bool m_bSupportDatasetSelection = true;
      protected bool m_bEntireCatalogMode = false;
      protected bool m_bAOIFilter = false;
      protected bool m_bPrevAOIFilter = false;
      protected bool m_bTextFilter = false;
      protected bool m_bPrevTextFilter = false;
      protected bool m_bSelect = true;

      protected string m_strHierarchy;
      protected SortedList<string, DataSet> m_hSelectedDataSets = new SortedList<string, DataSet>();

      protected Geosoft.Dap.Common.BoundingBox m_oCatalogBoundingBox = new Geosoft.Dap.Common.BoundingBox(180, 90, -180, -90);
      protected string m_strSearchString = string.Empty;
      protected string m_strCurSearchString = string.Empty;
      protected SearchModeEnum m_eMode = SearchModeEnum.All;
      protected SearchModeEnum m_ePrevMode = SearchModeEnum.All;

      protected Server m_oCurServer = null;
      protected ServerList m_oServerList;
      protected SortedList<string, Server> m_oValidServerList = new SortedList<string, Server>();
      protected SortedList<string, Server> m_oFullServerList = new SortedList<string, Server>();

      protected TreeNode m_hCurServerTreeNode;
      protected TreeNode m_hDAPRootNode = null;

      protected System.Xml.XmlDocument m_oCatalog = null;
      internal CatalogFolder m_oCatalogHierarchyRoot = null;

      internal CatalogCacheManager m_oCacheManager;

      protected DotNetTools.Common.Queue m_oAsyncQueue = new DotNetTools.Common.Queue();
      protected System.Threading.Thread m_oAsyncThread1;
      protected System.Threading.Thread m_oAsyncThread2;

      protected string m_strSecureToken;

#if DEBUG
      private List<String> m_oAddedServerDNSNames = new List<string>();
#endif

      #endregion

      #region Construction/Destruction

      /// <summary>
      /// Only in here for designer support
      /// </summary>
      public ServerTree() : this(null, String.Empty) { }

      public ServerTree(ImageList oImageList, string strCacheDir)
      {
         m_strCacheDir = strCacheDir;
         m_oServerList = new ServerList(m_strCacheDir);
         m_oCacheManager = new CatalogCacheManager(this, m_strCacheDir);         

         try
         {
            // --- Create Secure Token ---

            /*GeoSecureClient.CGeoSecureInterfaceClass scClass = new GeoSecureClient.CGeoSecureInterfaceClass();
            scClass.CreateSecureToken(out m_strSecureToken);*/
         }
         catch
         {
            m_strSecureToken = "";
         }


         InitializeComponent();

         this.ShowLines = true;
         this.ShowRootLines = false;
         this.ShowNodeToolTips = true;
         this.ShowPlusMinus = false;
         this.HideSelection = false;
         this.Scrollable = true;
         base.ImageList = oImageList;
         this.ImageIndex = this.SelectedImageIndex = Dapple.MainForm.ImageListIndex("folder");
         //this.NodeMouseClick += new TreeNodeMouseClickEventHandler(this.OnNodeMouseClick);
         this.AfterSelect += new TreeViewEventHandler(this.OnAfterSelect);
      }

      protected override void Dispose(bool disposing)
      {
         // First gracefully
         EnqueueRequest(AsyncRequestType.Stop);
         EnqueueRequest(AsyncRequestType.Stop);

         try
         {
            // Force it in Dapple
            m_oAsyncThread1.Abort();
            m_oAsyncThread2.Abort();
         }
         catch
         {
         }

         if (disposing)
         {
            if (m_oCacheManager != null)
               m_oCacheManager.Dispose();
            m_oCacheManager = null;
         }
         base.Dispose(disposing);
      }

      #endregion

      #region Events

      /// <summary>
      /// Define the select server event
      /// </summary>
      public event ServerCachedChangedHandler ServerCacheChanged;

      /// <summary>
      /// Invoke the delegatae registered with the select server event
      /// </summary>
      protected virtual void OnServerCachedChanged(Server server)
      {
         if (ServerCacheChanged != null)
         {
            ServerCacheChanged(this, server);
         }
      }

      /// <summary>
      /// Define the select server event
      /// </summary>
      public event ServerSelectHandler ServerSelect;

      /// <summary>
      /// Invoke the delegatae registered with the select server event
      /// </summary>
      protected virtual void OnSelectServer(Server oServer)
      {
         if (ServerSelect != null)
         {
            ServerSelect(this, oServer);
         }
         else if (oServer.Enabled)
         {
            // The login will setup the catalog
            Login(null);
         }
      }

      /// <summary>
      /// Define the server login event
      /// </summary>
      public event ServerLoggedInHandler ServerLoggedIn;

      /// <summary>
      /// Invoke the delegatae registered with the select server event
      /// </summary>
      protected virtual void OnServerLoggedIn(Server server)
      {
         if (ServerLoggedIn != null)
         {
            ServerLoggedIn(this, server);
         }
      }


      /// <summary>
      /// Define the dataset selected event
      /// </summary>
      public event DataSetSelectedHandler DataSetSelected;

      /// <summary>
      /// Invoke the delegatae registered with the Click event
      /// </summary>
      protected virtual void OnDataSetSelected(DataSetSelectedArgs e)
      {
         if (DataSetSelected != null)
         {
            DataSetSelected(this, e);
         }
      }

      #endregion

      #region Properties

      /// <summary>
      /// Context menu to display on RMB
      /// </summary>
      /*public ContextMenuStrip RMBContextMenuStrip
      {
         get { return m_oContextMenuStrip; }
         set { m_oContextMenuStrip = value; }
      }*/

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
      public SortedList<string, Server> ServerList
      {
         get { return m_oValidServerList; }
      }

      /// <summary>
      /// Get the full server list
      /// </summary>
      public SortedList<string, Server> FullServerList
      {
         get { return m_oFullServerList; }
      }

      /// <summary>
      /// The currently selected DAP dataset (or null)
      /// </summary>
      public DataSet SelectedDAPDataset
      {
         get
         {
            if (this.SelectedNode == null || !(this.SelectedNode.Tag is DataSet))
               return null;
            else
               return (DataSet)this.SelectedNode.Tag;
         }
      }

      /// <summary>
      /// The root node collection to use where DAP servers are kept
      /// </summary>
      public TreeNodeCollection DAPRootNodes
      {
         get
         {
            if (m_hDAPRootNode != null)
               return m_hDAPRootNode.Nodes;
            else
               return this.Nodes;
         }
      }
      
      /// <summary>
      /// Display checkboxes next to datasets for selection support
      /// </summary>
      public bool SupportDatasetSelection
      {
         get
         {
            return m_bSupportDatasetSelection;
         }
         set
         {
            m_bSupportDatasetSelection = value;
         }
      }
      #endregion

      #region Public Methods
      /// <summary>
      /// Handle the control creation event
      /// </summary>
      public void Load()
      {
         m_oAsyncThread1 = new System.Threading.Thread(new System.Threading.ThreadStart(SendAsyncRequest));
         m_oAsyncThread1.Start();

         m_oAsyncThread2 = new System.Threading.Thread(new System.Threading.ThreadStart(SendAsyncRequest));
         m_oAsyncThread2.Start();
      }

      /// <summary>
      /// Save server list to CSV
      /// </summary>
      public void SaveServerList()
      {
         m_oServerList.Save();
      }

      /// <summary>
      /// Add server through its url
      /// </summary>
      /// <param name="strUrl"></param>
      /// <returns></returns>
      public virtual bool AddDAPServer(string strUrl, out Server hRetServer, bool blEnabled)
      {
#if DEBUG
			try
			{
				System.Net.IPHostEntry oNewServer = System.Net.Dns.GetHostEntry(new Uri(strUrl).Host);
				if (m_oAddedServerDNSNames.Contains(oNewServer.HostName))
				{
					MessageBox.Show("Newly added server " + strUrl + " has host name matching existing server in tree.  Check for duplicates", "Possible duplicated DAP server detected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				}
				else
				{
					m_oAddedServerDNSNames.Add(oNewServer.HostName);
				}
			}
			catch (System.Net.Sockets.SocketException) { }
#endif
         bool bRet;

         bRet = true;
         hRetServer = null;

         try
         {
            Cursor = System.Windows.Forms.Cursors.WaitCursor;
            string strServerUrl = strUrl;

            if (!strServerUrl.StartsWith("http://"))
               strServerUrl = "http://" + strServerUrl;

            // Throw the string through a Uri parse/unparse so that server list lookups are right more often.
            // Fixes the problem that "http://dap.geosoft.com" and "http://dap.geosoft.com/" aren't recognized as the same server.
            String strActualServerUrl = new Uri(strServerUrl).ToString();

            Server oServer = new Server(strActualServerUrl, m_strCacheDir, m_strSecureToken, blEnabled);

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

            if (blEnabled) GetDatasetCount(oServer);
            PopulateServerList();
         }
         catch (Exception e)
         {
            GetDapError.Instance.Write("Error adding dap server " + strUrl + " to the list.\n\r(" + e.Message + ")");
            bRet = false;
         }
         Cursor = System.Windows.Forms.Cursors.Default;
         return bRet;
      }

      /// <summary>
      /// Remove the server from the list
      /// </summary>
      /// <param name="oServer">Server to remove</param>
      public void RemoveServer(Server oServer)
      {
         if (m_oValidServerList.ContainsKey(oServer.Url))
            m_oValidServerList.Remove(oServer.Url);

         m_oFullServerList.Remove(oServer.Url);
         m_oServerList.RemoveServer(oServer);

         if (oServer == m_oCurServer)
            m_oCurServer = null;

         PopulateServerList();
      }

#if !DAPPLE
      /// <summary>
      /// Enable a server
      /// </summary>
      /// <param name="oServer"></param>
      public void EnableServer(Server oServer)
      {
         if (!m_oValidServerList.ContainsKey(oServer.Url))
         {
            oServer.Status = Server.ServerStatus.OnLine;
            m_oValidServerList.Add(oServer.Url, oServer);
            PopulateServerList();
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
            PopulateServerList();
         }
      }
#endif

      /// <summary>
      /// Update the dataset count list
      /// </summary>
      public void UpdateCounts()
      {
         if (this.InvokeRequired)
            this.BeginInvoke(new MethodInvoker(this.RefreshTreeNodeText));
         else
            RefreshTreeNodeText();
      }

      /// <summary>
      /// Populate the list of servers
      /// </summary>
      public void PopulateServerList()
      {
         string strServerUrl = string.Empty;
         Server oSelectServer;

         // --- do not execute if we are cleanup up --- 

         if (this.IsDisposed) return;

         m_bSelect = false;

         oSelectServer = m_oCurServer;

         foreach (Server oServer in m_oServerList.Servers)
         {
            if (oServer.Status == Server.ServerStatus.OnLine || oServer.Status == Server.ServerStatus.Maintenance)
            {
               if (!m_oValidServerList.ContainsKey(oServer.Url)) m_oValidServerList.Add(oServer.Url, oServer);
            }

            if (!m_oFullServerList.ContainsKey(oServer.Url)) m_oFullServerList.Add(oServer.Url, oServer);
         }

         // Show all servers in Dapple, there is no other way to manage the list of servers
         if (m_oCurServer != null && m_oFullServerList.ContainsValue(m_oCurServer))
            strServerUrl = m_oCurServer.Url;
         else
            m_oCurServer = null;

         this.BeginUpdate();

         // --- Remove all but current server ---
         List<TreeNode> nodeList = new List<TreeNode>();
         foreach (TreeNode oTreeNode in this.DAPRootNodes)
            nodeList.Add(oTreeNode);
         foreach (TreeNode oTreeNode in nodeList)
         {
            if (m_oCurServer != oTreeNode.Tag as Server)
               oTreeNode.Remove();
         }

         int iInsert = 0;
         // Show all servers in Dapple, there is no other way to manage the list of servers
         for (int i = 0; i < m_oFullServerList.Count; i++)
         {
            Server oServer = m_oFullServerList.Values[i];

            if (m_oCurServer == oServer)
               iInsert++;
            else
            {
               if (oServer.Url == strServerUrl)
                  oSelectServer = oServer;

               TreeNode oTreeNode = new TreeNode(oServer.Name);
               oTreeNode.Tag = oServer;

               this.DAPRootNodes.Insert(iInsert, oTreeNode);
            }

            iInsert++;
         }

         this.RefreshTreeNodeText();
         this.EndUpdate();

         m_bSelect = true;

         if (oSelectServer != null)
            SelectServer(oSelectServer);
      }

      /// <summary>
      /// Set the selected server
      /// </summary>
      /// <param name="oServer"></param>
      public void SelectServer(Server oServer)
      {
         if (m_oCurServer != oServer)
         {
            foreach (TreeNode oTreeNode in this.DAPRootNodes)
            {
               if (oServer == oTreeNode.Tag as Server)
               {
                  this.SelectedNode = oTreeNode;
#if DEBUG
                  System.Diagnostics.Debug.WriteLine("SelectedNode Changed (SelectServer): " + (this.SelectedNode != null ? this.SelectedNode.Text : "(none)"));
#endif
               }
            }
            m_oCurServer = oServer;
            OnSelectServer(m_oCurServer);
         }
      }

      /// <summary>
      /// Update the contents of the tree, as a result has been (de)selected from a different screen
      /// </summary>
      public void UpdateResults(DataSet hDataSet, bool bAdded)
      {
         if (bAdded)
         {
            if (!m_hSelectedDataSets.ContainsKey(hDataSet.UniqueName)) m_hSelectedDataSets.Add(hDataSet.UniqueName, hDataSet);
         }
         else
         {
            m_hSelectedDataSets.Remove(hDataSet.UniqueName);
         }
         RefreshResults();
      }

      /// <summary>
      /// Clear the list of selected datasets
      /// </summary>
      public void ClearSelectedDatasets()
      {
         m_hSelectedDataSets.Clear();
         RefreshResults();
      }

      /// <summary>
      /// Invalidate drawing area until we get the results back
      /// </summary>
      public void SetupCatalog(BoundingBox searchExtents)
      {
         if (m_oCurServer.MajorVersion < 6 || (m_oCurServer.MajorVersion == 6 && m_oCurServer.MinorVersion < 3))
            m_bEntireCatalogMode = true;
         else
            m_bEntireCatalogMode = false;

         ClearCatalog();
         if (searchExtents != null)
            m_oCatalogBoundingBox = new Geosoft.Dap.Common.BoundingBox(searchExtents);
         EnqueueRequest(AsyncRequestType.GetCatalogHierarchy);
      }

      /// <summary>
      /// Refresh the catalog based on the new aoi
      /// </summary>
      public virtual void AsyncFilterChanged(SearchModeEnum eMode, BoundingBox searchExtents, string strSearch, bool bAOIFilter, bool bTextFilter)
      {
         m_eMode = eMode;
         m_bAOIFilter = bAOIFilter;
         m_bTextFilter = bTextFilter;
         CreateSearchString(strSearch);
         EnqueueRequest(ServerTree.AsyncRequestType.FilterChanged, (object)ServerTree.SearchModeEnum.All, (object)searchExtents, (object)strSearch, (object)bAOIFilter, (object)bTextFilter);
      }

      /// <summary>
      /// Refresh the catalog based on the new aoi
      /// </summary>
      public void FilterChanged(SearchModeEnum eMode, BoundingBox searchExtents, string strSearch, bool bAOIFilter, bool bTextFilter)
      {
         bool bChanged = false;


         // --- the catalog filter changed, do we need to do something ---

         if (m_bAOIFilter && m_oCatalogBoundingBox != searchExtents)
         {
            m_oCatalogBoundingBox = new Geosoft.Dap.Common.BoundingBox(searchExtents);
            bChanged = true;
         }

         if (m_bTextFilter && (m_strSearchString != m_strCurSearchString || m_eMode != m_ePrevMode))
         {
            m_strSearchString = m_strCurSearchString;
            m_ePrevMode = m_eMode;
            bChanged = true;
         }

         if (bChanged || m_bAOIFilter != m_bPrevAOIFilter || m_bTextFilter != m_bPrevTextFilter)
         {
            ClearCatalog();
            GetCatalogHierarchy();

            m_bPrevAOIFilter = m_bAOIFilter;
            m_bPrevTextFilter = m_bTextFilter;
         }
      }

      public void ReenableServer(Server oServer)
      {
         oServer.Status = Server.ServerStatus.OnLine;
         if (!m_oValidServerList.ContainsKey(oServer.Url))
            m_oValidServerList.Add(oServer.Url, oServer);
      }
      public void NoResponseError(Server oServer)
      {
         Int32 iIndex = m_oValidServerList.IndexOfKey(oServer.Url);

         if (iIndex != -1)
            m_oValidServerList.RemoveAt(iIndex);
         oServer.Status = Server.ServerStatus.OffLine;
         if (this.InvokeRequired)
            this.BeginInvoke(new MethodInvoker(this.PopulateServerList));
         else
            PopulateServerList();
      }

      /// <summary>
      /// Get the catalog hierarchy
      /// </summary>
      public void GetCatalogHierarchy()
      {
         string strConfigurationEdition = string.Empty;

         if (m_oCurServer == null)
            return;

         if (!(m_oCurServer.Enabled))
         {
            m_oCatalogHierarchyRoot = null;

            if (this.InvokeRequired)
            {
               try
               {
                  this.Invoke(new MethodInvoker(this.RefreshResults));
               }
               catch
               {
               }
            }
            else
               RefreshResults();

            return;
         }

         if (m_bEntireCatalogMode)
         {
            Catalog oCatalog = null;

            try
            {
               if (!m_bAOIFilter && !m_bTextFilter)
                  oCatalog = m_oCurServer.CatalogCollection.GetCatalog(null, string.Empty);
               else if (!m_bAOIFilter && m_bTextFilter)
                  oCatalog = m_oCurServer.CatalogCollection.GetCatalog(null, m_strSearchString);
               else if (m_bAOIFilter && !m_bTextFilter)
                  oCatalog = m_oCurServer.CatalogCollection.GetCatalog(m_oCatalogBoundingBox, string.Empty);
               else if (m_bAOIFilter && m_bTextFilter)
                  oCatalog = m_oCurServer.CatalogCollection.GetCatalog(m_oCatalogBoundingBox, m_strSearchString);
            }
            catch (DapException)
            {
               oCatalog = null;
            }

            if (oCatalog == null)
            {
               // --- do something to disable server ---
               NoResponseError(m_oCurServer);
               return;
            }
            else
            {
               // --- Looks like the server is online now ---
               ReenableServer(m_oCurServer);
               m_oCatalog = oCatalog.Document;
               strConfigurationEdition = oCatalog.ConfigurationEdition;
            }
         }
         else
         {
            bool bEntireCatalog;
            m_oCatalogHierarchyRoot = null;

            try
            {
               m_oCatalogHierarchyRoot = m_oCacheManager.GetCatalogHierarchyRoot(m_oCurServer, m_oCatalogBoundingBox, m_bAOIFilter, m_bTextFilter, m_strSearchString, out bEntireCatalog, out strConfigurationEdition);
            }
            catch
            {
               // Assumed that the server does not support this request, revert to old method
               bEntireCatalog = true;
            }

            if (m_oCatalogHierarchyRoot == null && bEntireCatalog)
            {
               m_bEntireCatalogMode = true;
               GetCatalogHierarchy();
               return;
            }
         }

         if (strConfigurationEdition != m_oCurServer.CacheVersion)
         {
            OnServerCachedChanged(m_oCurServer);
            return;
         }

         if (this.InvokeRequired)
         {
            try
            {
               this.Invoke(new MethodInvoker(this.RefreshResults));
            }
            catch
            {
            }
         }
         else
            RefreshResults();
      }

      /// <summary>
      /// Get the list of datasets for a particular path
      /// </summary>
      /// <param name="strHierarchy"></param>
      public void GetDatasetList(string strHierarchy, int iTimestamp)
      {
         if (!m_oCacheManager.bGetDatasetList(m_oCurServer, strHierarchy, iTimestamp, m_oCatalogBoundingBox, m_bAOIFilter, m_bTextFilter, m_strSearchString))
         {
            OnServerCachedChanged(m_oCurServer);
            return;
         }

         if (this.InvokeRequired)
         {
            try
            {
               this.Invoke(new MethodInvoker(this.RefreshResults));
            }
            catch
            {
            }
         }
         else
            RefreshResults();
      }

      /// <summary>
      /// Get the dataset counts for this server
      /// </summary>
      /// <param name="oServer"></param>
      public void GetDatasetCount(Server oServer)
      {
         if (!m_bAOIFilter && !m_bTextFilter)
            oServer.GetDatasetCount(null, null);
         else if (!m_bAOIFilter && m_bTextFilter)
            oServer.GetDatasetCount(null, m_strSearchString);
         else if (m_bAOIFilter && !m_bTextFilter)
            oServer.GetDatasetCount(m_oCatalogBoundingBox, null);
         else if (m_bAOIFilter && m_bTextFilter)
            oServer.GetDatasetCount(m_oCatalogBoundingBox, m_strSearchString);
      }

      /// <summary>
      /// Log the user into the current server
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      public bool Login(BoundingBox searchExtents)
      {
         bool bRet = true;

         if (m_oCurServer == null)
            return false;


         // --- ensure we only attempt to login 1 at a time ---

         lock (this)
         {
            // --- Set the Server Token ---

            m_oCurServer.SetServerToken();


            // --- if we are successfully logged in, then get the data from the dap server ---

            if (bRet)
            {
               // Show all servers in Dapple, there is no other way to manage the list of servers
               SelectServer(m_oFullServerList[m_oCurServer.Url]);
               SetupCatalog(searchExtents);
            }

            OnServerLoggedIn(m_oCurServer);
         }

         return bRet;
      }

      #endregion

      #region Protected Methods

      /// <summary>
      /// Enqueue a request onto the queue
      /// </summary>
      /// <param name="eRequest"></param>
      public void EnqueueRequest(AsyncRequestType eRequest)
      {
         AsyncRequest oRequest = new AsyncRequest();
         oRequest.m_bValid = true;
         oRequest.m_eType = eRequest;
         oRequest.m_oParam1 = null;
         oRequest.m_oParam2 = null;
         oRequest.m_oParam3 = null;
         oRequest.m_oParam4 = null;
         oRequest.m_oParam5 = null;
         m_oAsyncQueue.Enqueue(oRequest);
      }

      /// <summary>
      /// Enqueue a request onto the queue
      /// </summary>
      /// <param name="eRequest"></param>
      /// <param name="oParam1"></param>
      /// <param name="oParam2"></param>
      public void EnqueueRequest(AsyncRequestType eRequest, object oParam1, object oParam2)
      {
         AsyncRequest oRequest = new AsyncRequest();
         oRequest.m_bValid = true;
         oRequest.m_eType = eRequest;
         oRequest.m_oParam1 = oParam1;
         oRequest.m_oParam2 = oParam2;
         oRequest.m_oParam3 = null;
         oRequest.m_oParam4 = null;
         oRequest.m_oParam5 = null;
         m_oAsyncQueue.Enqueue(oRequest);
      }

      /// <summary>
      /// Enqueue a request onto the queue
      /// </summary>
      /// <param name="eRequest"></param>
      /// <param name="oParam1"></param>
      /// <param name="oParam2"></param>
      /// <param name="oParam3"></param>
      /// <param name="oParam4"></param>
      /// <param name="oParam5"></param>
      public void EnqueueRequest(AsyncRequestType eRequest, object oParam1, object oParam2, object oParam3, object oParam4, object oParam5)
      {
         AsyncRequest oRequest = new AsyncRequest();
         oRequest.m_bValid = true;
         oRequest.m_eType = eRequest;
         oRequest.m_oParam1 = oParam1;
         oRequest.m_oParam2 = oParam2;
         oRequest.m_oParam3 = oParam3;
         oRequest.m_oParam4 = oParam4;
         oRequest.m_oParam5 = oParam5;
         m_oAsyncQueue.Enqueue(oRequest);
      }

      /// <summary>
      /// Send an async request
      /// </summary>
      protected void SendAsyncRequest()
      {
         AsyncRequest oRequest;
         do
         {
            oRequest = new AsyncRequest();
            oRequest.m_bValid = false;

            // --- Safety try/catch so that a thread will not terminate, and not clean itself up ---

            try
            {
               oRequest = (AsyncRequest)m_oAsyncQueue.Dequeue();

               if (oRequest.m_eType == AsyncRequestType.GetCatalogHierarchy)
               {
                  GetCatalogHierarchy();
                  GetCatalogCount();
               }
               else if (oRequest.m_eType == AsyncRequestType.GetDatasetList)
               {
                  GetDatasetList((string)oRequest.m_oParam1, (int)oRequest.m_oParam2);
               }
               else if (oRequest.m_eType == AsyncRequestType.FilterChanged)
               {
                  FilterChanged((SearchModeEnum)oRequest.m_oParam1, (BoundingBox)oRequest.m_oParam2, (string)oRequest.m_oParam3, (bool)oRequest.m_oParam4, (bool)oRequest.m_oParam5);
                  GetCatalogCount();
               }
            }
            catch (Exception e)
            {
               if (oRequest.m_bValid)
                  GetDapError.Instance.Write(string.Format("Failed to send request {0} - {1}", oRequest.m_eType.ToString(), e.Message));
            }

         } while (!oRequest.m_bValid || oRequest.m_eType != AsyncRequestType.Stop);
      }

      /// <summary>
      /// Create the search string
      /// </summary>
      protected void CreateSearchString(string strSearch)
      {
         m_strCurSearchString = strSearch;
         if (m_eMode == SearchModeEnum.Name)
            m_strCurSearchString = string.Format("Name contains({0})", m_strCurSearchString);
         else if (m_eMode == SearchModeEnum.Description)
            m_strCurSearchString = string.Format("Description contains({0})", m_strCurSearchString);
         else if (m_eMode == SearchModeEnum.Keywords)
            m_strCurSearchString = string.Format("Keywords contains({0})", m_strCurSearchString);
      }

      /// <summary>
      /// Get the catalog counts, for this particular query
      /// </summary>
      protected void GetCatalogCount()
      {
         foreach (Server oServer in m_oValidServerList.Values)
            GetDatasetCount(oServer);
         UpdateCounts();
      }

      /// <summary>
      /// Update the treenode text for each server
      /// </summary>
      protected virtual void RefreshTreeNodeText()
      {
         // --- do not execute if we are cleanup up --- 

         if (this.IsDisposed)
            return;

         foreach (TreeNode oTreeNode in this.DAPRootNodes)
         {
            string str;
            int iImage;
            Server oServer = oTreeNode.Tag as Server;

            if (string.IsNullOrEmpty(oServer.Name))
               str = oServer.Url;
            else
               str = oServer.Name;

            if (!oServer.Enabled)
            {
               str += " (disabled)";
               iImage = Dapple.MainForm.ImageListIndex("disserver");
            }
            else
            {
               switch (oServer.Status)
               {
                  case Server.ServerStatus.OnLine:
                     str += " (" + oServer.DatasetCount.ToString() + ")";
                     iImage = Dapple.MainForm.ImageListIndex("enserver");
                     break;
                  case Server.ServerStatus.Maintenance:
                     str += " (undergoing maintenance)";
                     iImage = Dapple.MainForm.ImageListIndex("disserver");
                     break;
                  case Server.ServerStatus.OffLine:
                     str += " (offline)";
                     iImage = Dapple.MainForm.ImageListIndex("offline");
                     break;
                  case Server.ServerStatus.Disabled:
                     str += " (disabled)";
                     iImage = Dapple.MainForm.ImageListIndex("disserver");
                     break;
                  default:
                     str += " (unsupported)";
                     iImage = Dapple.MainForm.ImageListIndex("offline");
                     break;
               }
            }


            oTreeNode.Text = str;
            oTreeNode.ImageIndex = oTreeNode.SelectedImageIndex = iImage;
         }
      }

      internal FolderDatasetList GetDatasets(CatalogFolder oFolder)
      {
         return m_oCacheManager.GetDatasets(m_oCurServer, oFolder, m_oCatalogBoundingBox, m_bAOIFilter, m_bTextFilter, m_strSearchString);
      }      

      /// <summary>
      /// Display the catalog, after it has been modified
      /// </summary>
      protected void RefreshResults()
      {
         TreeNode oSelectedNode = null;

         // --- do not execute if we are cleanup up --- 

         if (this.IsDisposed) return;

         this.AfterSelect -= new TreeViewEventHandler(this.OnAfterSelect);

         // --- Clear all server nodes ---
         this.BeginUpdate();

         foreach (TreeNode oTreeNode in this.DAPRootNodes)
            oTreeNode.Nodes.Clear();

         if (m_oCurServer.Enabled)
         {
            if (m_bEntireCatalogMode)
               oSelectedNode = DisplayEntireCatalog();
            else
               oSelectedNode = DisplayCatalog();
         }
         else
         {
            oSelectedNode = m_hCurServerTreeNode;
         }

         this.SelectedNode = oSelectedNode;
#if DEBUG
         System.Diagnostics.Debug.WriteLine("SelectedNode Changed (RefreshResults): " + (this.SelectedNode != null ? this.SelectedNode.Text : "(none)"));
#endif
         this.RefreshTreeNodeText();
         if (m_hDAPRootNode != null)
            m_hDAPRootNode.ExpandAll();
         else
            this.ExpandAll();
         this.EndUpdate();
         this.AfterSelect += new TreeViewEventHandler(this.OnAfterSelect);
      }

      /// <summary>
      /// Display the catalog in chunks
      /// </summary>
      /// <returns></returns>
      protected TreeNode DisplayCatalog()
      {
         CatalogFolder oCurFolder = null;
         TreeNode hTreeNode = null;
         string[] strLevels;

         // --- display the catalog in tree view form ---

         if (m_oCatalogHierarchyRoot == null)
         {
            m_hCurServerTreeNode.Nodes.Clear();
            return null;
         }

         oCurFolder = m_oCatalogHierarchyRoot;
         hTreeNode = m_hCurServerTreeNode;

         strLevels = m_strHierarchy.Split('/');
         for (int i = 0; i < strLevels.Length - 1; i++)
         {
            string str = strLevels[i];
            CatalogFolder oChildFolder;

            oChildFolder = oCurFolder.GetFolder(strLevels[i]);
            if (oChildFolder != null)
            {
               Int32 iIndex = -1;

               // --- find where this node is in the children list ---

               for (Int32 b = 0; b < hTreeNode.Nodes.Count; b++)
               {
                  if (hTreeNode.Nodes[b].Text == str)
                  {
                     iIndex = b;
                     break;
                  }
               }

               // --- remove all nodes that are not this one ---

               if (iIndex != -1)
               {
                  Int32 iCount = hTreeNode.Nodes.Count;

                  for (Int32 a = 0; a < iCount; a++)
                  {
                     if (a < iIndex)
                        hTreeNode.Nodes.RemoveAt(0);
                     else if (a > iIndex)
                        hTreeNode.Nodes.RemoveAt(1);
                  }
                  hTreeNode = hTreeNode.Nodes[0];
                  hTreeNode.ImageIndex = hTreeNode.SelectedImageIndex = Dapple.MainForm.ImageListIndex("folder_open");
               }
               else
               {
                  TreeNode oTempNode = new TreeNode(str, Dapple.MainForm.ImageListIndex("folder_open"), Dapple.MainForm.ImageListIndex("folder_open"));
                  oTempNode.Tag = null;

                  hTreeNode.Nodes.Clear();
                  hTreeNode.Nodes.Add(oTempNode);
                  hTreeNode = oTempNode;
               }
               oCurFolder = oChildFolder;
            }
         }

         hTreeNode.Nodes.Clear();
         foreach (CatalogFolder oFolder in oCurFolder.Folders)
         {
            TreeNode hChildTreeNode;

            hChildTreeNode = new TreeNode(oFolder.Name, Dapple.MainForm.ImageListIndex("folder"), Dapple.MainForm.ImageListIndex("folder"));
            hChildTreeNode.Tag = null;
            hTreeNode.Nodes.Add(hChildTreeNode);
         }

         FolderDatasetList oDatasetList = GetDatasets(oCurFolder);
         if (oDatasetList == null)
         {
            // --- updating in progress ---

            TreeNode hTempNode;
            hTempNode = new TreeNode("Retrieving Datasets...", Dapple.MainForm.ImageListIndex("loading"), Dapple.MainForm.ImageListIndex("loading"));
            hTempNode.Tag = null;
            hTreeNode.Nodes.Add(hTempNode);
            this.Refresh();

            EnqueueRequest(AsyncRequestType.GetDatasetList, oCurFolder.Hierarchy, oCurFolder.Timestamp);
         }
         else
         {
            foreach (DataSet oDataset in oDatasetList.Datasets)
            {
               Int32 iType;
               TreeNode hChildTreeNode;

               iType = Dapple.MainForm.ImageIndex(oDataset.Type);
               if (m_bSupportDatasetSelection)
               {
                  if (m_hSelectedDataSets.ContainsKey(oDataset.UniqueName))
                     hChildTreeNode = this.Add(hTreeNode, oDataset.Title, iType, iType);
                  else
                     hChildTreeNode = this.Add(hTreeNode, oDataset.Title, iType, iType);
               }
               else
               {
                  hChildTreeNode = new TreeNode(oDataset.Title, iType, iType);
                  hTreeNode.Nodes.Add(hChildTreeNode);
               }

               hChildTreeNode.Tag = oDataset;
            }
         }
         return hTreeNode;
      }

      /// <summary>
      /// Display the catalog in the old fashioned way, with the server returning the entire catalog 
      /// </summary>
      protected TreeNode DisplayEntireCatalog()
      {
         System.Xml.XmlNodeList hNodeList;
         System.Xml.XmlNode hCurNode = null;
         TreeNode hTreeNode = null;
         string[] strLevels;

         // --- display the catalog in tree view form ---

         if (m_oCatalog == null)
         {
            m_hCurServerTreeNode.Nodes.Clear();
            return null;
         }

         // --- get the catalog node --

         hCurNode = m_oCatalog.DocumentElement;
         hNodeList = hCurNode.SelectNodes("//" + Geosoft.Dap.Xml.Common.Constant.Tag.CATALOG_TAG);

         if (hNodeList == null || hNodeList.Count == 0) return null;

         hCurNode = hNodeList[0];

         hTreeNode = m_hCurServerTreeNode;

         strLevels = m_strHierarchy.Split('/');
         for (int i = 0; i < strLevels.Length - 1; i++)
         {
            string str = strLevels[i];

            string szPath = Geosoft.Dap.Xml.Common.Constant.Tag.COLLECTION_TAG + "[@" + Geosoft.Dap.Xml.Common.Constant.Attribute.NAME_ATTR + "=\"" + str + "\"]";

            hNodeList = hCurNode.SelectNodes(szPath);

            if (hNodeList != null && hNodeList.Count == 1)
            {
               Int32 iIndex = -1;

               // --- find where this node is in the children list ---

               for (Int32 b = 0; b < hTreeNode.Nodes.Count; b++)
               {
                  if (hTreeNode.Nodes[b].Text == str)
                  {
                     iIndex = b;
                     break;
                  }
               }

               // --- remove all nodes that are not this one ---

               if (iIndex != -1)
               {
                  Int32 iCount = hTreeNode.Nodes.Count;

                  for (Int32 a = 0; a < iCount; a++)
                  {
                     if (a < iIndex)
                        hTreeNode.Nodes.RemoveAt(0);
                     else if (a > iIndex)
                        hTreeNode.Nodes.RemoveAt(1);
                  }
                  hTreeNode = hTreeNode.Nodes[0];
                  hTreeNode.ImageIndex = hTreeNode.SelectedImageIndex = Dapple.MainForm.ImageListIndex("folder_open");
               }
               else
               {
                  hTreeNode.Nodes.Clear();

                  TreeNode oTempNode = new TreeNode(str, Dapple.MainForm.ImageListIndex("folder_open"), Dapple.MainForm.ImageListIndex("folder_open"));
                  oTempNode.Tag = null;
                  hTreeNode.Nodes.Add(oTempNode);
                  hTreeNode = oTempNode;
               }
               hCurNode = hNodeList[0];
            }
         }

         hTreeNode.Nodes.Clear();
         foreach (System.Xml.XmlNode hChildNode in hCurNode.ChildNodes)
         {
            System.Xml.XmlNode hAttr = hChildNode.Attributes.GetNamedItem(Geosoft.Dap.Xml.Common.Constant.Attribute.NAME_ATTR);
            TreeNode hChildTreeNode;

            if (hAttr == null) continue;

            if (hChildNode.Name == Geosoft.Dap.Xml.Common.Constant.Tag.COLLECTION_TAG)
            {
               hChildTreeNode = new TreeNode(hAttr.Value, Dapple.MainForm.ImageListIndex("folder"), Dapple.MainForm.ImageListIndex("folder"));
               hChildTreeNode.Tag = null;
               hTreeNode.Nodes.Add(hChildTreeNode);
            }
         }

         foreach (System.Xml.XmlNode hChildNode in hCurNode.ChildNodes)
         {
            System.Xml.XmlNode hAttr = hChildNode.Attributes.GetNamedItem(Geosoft.Dap.Xml.Common.Constant.Attribute.NAME_ATTR);
            TreeNode hChildTreeNode;

            if (hAttr == null) continue;

            if (hChildNode.Name != Geosoft.Dap.Xml.Common.Constant.Tag.COLLECTION_TAG)
            {
               Int32 iType;
               DataSet oDataSet;

               m_oCurServer.Command.Parser.DataSet(hChildNode, out oDataSet);

               iType = Dapple.MainForm.ImageIndex(oDataSet.Type);

               if (m_bSupportDatasetSelection)
               {
                  if (m_hSelectedDataSets.ContainsKey(oDataSet.UniqueName))
                     hChildTreeNode = this.Add(hTreeNode, oDataSet.Title, iType, iType);
                  else
                     hChildTreeNode = this.Add(hTreeNode, oDataSet.Title, iType, iType);
               }
               else
               {
                  hChildTreeNode = new TreeNode(oDataSet.Title, iType, iType);
                  hTreeNode.Nodes.Add(hChildTreeNode);
               }
               hChildTreeNode.Tag = oDataSet;
            }
         }
         return hTreeNode;
      }

      /// <summary>
      /// Add a new node to the tree
      /// </summary>
      /// <param name="hParent"></param>
      /// <param name="strNodeText"></param>
      /// <param name="iImageIndex"></param>
      /// <param name="iSelectedImageIndex"></param>
      /// <param name="eState"></param>      
      /// <returns></returns>
      public virtual System.Windows.Forms.TreeNode Add(System.Windows.Forms.TreeNode hParent, string strNodeText, int iImageIndex, int iSelectedImageIndex)
      {
         System.Windows.Forms.TreeNode hNode;

         hNode = new System.Windows.Forms.TreeNode(strNodeText);
         hNode.ImageIndex = iImageIndex;
         hNode.SelectedImageIndex = iSelectedImageIndex;

         if (hParent != null)
            hParent.Nodes.Add(hNode);
         else
            this.Nodes.Add(hNode);

         return hNode;
      }

      /// <summary>
      /// Clear the current catalog that we have displayed
      /// </summary>
      protected void ClearCatalog()
      {
         m_oCatalog = null;
         m_oCatalogHierarchyRoot = null;
         m_oAsyncQueue.Clear();

         if (this.InvokeRequired)
            this.Invoke(new MethodInvoker(ClearTree));
         else
            ClearTree();
      }

      protected void ClearTree()
      {
         if (m_hCurServerTreeNode != null)
         {
            m_hCurServerTreeNode.Nodes.Clear();

            // --- updating in progress ---

            TreeNode hTempNode;
            hTempNode = new TreeNode("Retrieving Datasets...", Dapple.MainForm.ImageListIndex("loading"), Dapple.MainForm.ImageListIndex("loading"));
            hTempNode.Tag = null;
            m_hCurServerTreeNode.Nodes.Add(hTempNode);
            this.Refresh();
         }
      }

      #endregion

      #region Event Handlers

      /*protected void OnNodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
      {
         if (m_oContextMenuStrip != null && e.Button == MouseButtons.Right)
         {
            // Select first on right click, then display context menu
            this.SelectedNode = e.Node;

            m_oContextMenuStrip.Show(this, e.Location.X, e.Node.Bounds.Y + e.Node.Bounds.Height/2);
         }
      }*/

      protected virtual void UpdateTreeNodeColors()
      {
      }
      protected virtual void AfterSelected(TreeNode node)
      {
      }
      protected virtual void FireViewMetadataEvent()
      {
      }

      /// <summary>
      /// Modify catalog browsing tree
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      protected void OnAfterSelect(object sender, TreeViewEventArgs e)
      {
         if (m_bSelect)
         {
            TreeNode hCurrentNode;
            
            m_bSelect = false;

            hCurrentNode = e.Node;

            if (!(hCurrentNode.Tag is DataSet))
            {
               string strNewHierarchy = String.Empty;

               while (hCurrentNode != null && !(hCurrentNode.Tag is Server))
               {
                  strNewHierarchy = hCurrentNode.Text + "/" + strNewHierarchy;
                  hCurrentNode = hCurrentNode.Parent;
               }

					if (hCurrentNode != null)
					{
						m_strHierarchy = strNewHierarchy;
						if (hCurrentNode.Tag is Server)
						{
							m_hCurServerTreeNode = hCurrentNode;
							if (m_hCurServerTreeNode.Tag as Server != m_oCurServer)
								SelectServer(m_hCurServerTreeNode.Tag as Server);
							else
								RefreshResults();
						}
						UpdateTreeNodeColors();
					}
					else
					{
						// We're not in the DAP branch, so let the subclass take care of it.
						m_oCurServer = null;
						m_hCurServerTreeNode = null;
						AfterSelected(e.Node);
					}
            }
            else
            {
               FireViewMetadataEvent();
            }

            // --- This is needed because all the docking window activations tends to change the tree selection on us ---
            /*if (!this.SelectedNode.Equals(e.Node))
               this.SelectedNode = e.Node;*/
#if DEBUG
            System.Diagnostics.Debug.WriteLine("SelectedNode Changed (AfterSelect): " + (this.SelectedNode != null ? this.SelectedNode.Text : "(none)"));
#endif
            m_bSelect = true;
         }
      }

      /// <summary>
      /// Handle when a new dataset has been selected
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      protected virtual void OnTreeNodeChecked(object sender, Geosoft.DotNetTools.TreeNodeCheckedEventArgs e)
      {
         DataSetSelectedArgs ex = new DataSetSelectedArgs();
         if (e.Node != null && e.Node.Tag != null)
         {
            ex.DataSet = (DataSet)e.Node.Tag;

            if (e.State == Geosoft.DotNetTools.TriStateTreeView.CheckBoxState.Checked)
            {
               if (!m_hSelectedDataSets.ContainsKey(ex.DataSet.UniqueName)) m_hSelectedDataSets.Add(ex.DataSet.UniqueName, ex.DataSet);
               ex.Selected = true;
            }
            else
            {
               m_hSelectedDataSets.Remove(ex.DataSet.UniqueName);
               ex.Selected = false;
            }

            OnDataSetSelected(ex);
         }
      }      
      #endregion

      private void InitializeComponent()
      {
         this.SuspendLayout();
         // 
         // ServerTree
         // 
         this.ResumeLayout(false);

      }      
   }
}
