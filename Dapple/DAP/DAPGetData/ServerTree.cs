using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;

using Geosoft.Dap;
using Geosoft.Dap.Common;
using Geosoft.DotNetTools;

#if !DAPPLE
using Geosoft.GXNet;
using Resources = global::Geosoft.GX.Properties.Resources;
#else
using Resources = global::Dapple.Properties.Resources;
#endif

namespace Geosoft.GX.DAPGetData
{
   public class ServerTree : TriStateTreeView
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
#if !DAPPLE
      protected bool m_bGeodist;
#endif
      protected bool m_bEntireCatalogMode = false;
      protected bool m_bAOIFilter = false;
      protected bool m_bPrevAOIFilter = false;
      protected bool m_bTextFilter = false;
      protected bool m_bPrevTextFilter = false;

      protected string m_strHierarchy;
      protected SortedList<string, DataSet> m_hSelectedDataSets = new SortedList<string, DataSet>();

      protected Geosoft.Dap.Common.BoundingBox m_oCatalogBoundingBox = null;
      protected string m_strSearchString = string.Empty;
      protected string m_strCurSearchString = string.Empty;
      protected SearchModeEnum m_eMode = SearchModeEnum.All;
      protected SearchModeEnum m_ePrevMode = SearchModeEnum.All;

      protected Server m_oCurServer = null;
      protected ServerList m_oServerList;
      protected SortedList<string, Server> m_oValidServerList = new SortedList<string, Server>();
      protected SortedList<string, Server> m_oFullServerList = new SortedList<string, Server>();

      protected TreeNode m_hCurrentNode;
      protected TreeNode m_hCurServerTreeNode;

      private ImageList m_hNodeimageList = new ImageList();
      private PictureBox m_pbUpdateCatalog = new PictureBox();

      protected System.Xml.XmlDocument m_oCatalog = null;
      private CatalogFolder m_oCatalogHierarchyRoot = null;

      private CatalogCacheManager m_oCacheManager;

      protected DotNetTools.Common.Queue m_oAsyncQueue = new DotNetTools.Common.Queue();
      protected System.Threading.Thread m_oAsyncThread1;
      protected System.Threading.Thread m_oAsyncThread2;
      #endregion

      #region Construction/Destruction
#if !DAPPLE
      public ServerTree()
      {
         m_oServerList = new ServerList();
         m_bGeodist = CSYS.iGetGeodist() != 0;
         m_oCacheManager = new CatalogCacheManager(this);
#else
      public ServerTree(string strCacheDir)
      {
         m_oServerList = new ServerList(strCacheDir);
         m_oCacheManager = new CatalogCacheManager(this, strCacheDir);
#endif
         
         m_hNodeimageList.ColorDepth = ColorDepth.Depth32Bit;
         m_hNodeimageList.ImageSize = new Size(16, 16);
         m_hNodeimageList.TransparentColor = Color.Transparent;

         m_hNodeimageList.Images.Add("enserver", Resources.enserver);
         m_hNodeimageList.Images.Add("disserver", Resources.disserver);
         m_hNodeimageList.Images.Add("offline", Resources.offline);
         m_hNodeimageList.Images.Add("dap", Resources.dap);
         m_hNodeimageList.Images.Add("dap_database", Resources.dap_database);
         m_hNodeimageList.Images.Add("dap_document", Resources.dap_document);
         m_hNodeimageList.Images.Add("dap_grid", Resources.dap_grid);
         m_hNodeimageList.Images.Add("dap_map", Resources.dap_map);
         m_hNodeimageList.Images.Add("dap_picture", Resources.dap_picture);
         m_hNodeimageList.Images.Add("dap_point", Resources.dap_point);
         m_hNodeimageList.Images.Add("dap_spf", Resources.dap_spf);
         m_hNodeimageList.Images.Add("dap_voxel", Resources.dap_voxel);
         m_hNodeimageList.Images.Add("folder", Resources.folder);
         m_hNodeimageList.Images.Add("folder_open", Resources.folder_open);
         m_hNodeimageList.Images.Add("loading", Resources.loading);

         m_pbUpdateCatalog.Visible = false;
         m_pbUpdateCatalog.BackColor = System.Drawing.Color.White;
         m_pbUpdateCatalog.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
         m_pbUpdateCatalog.Image = Resources.dap_update;
         m_pbUpdateCatalog.Location = new System.Drawing.Point((this.ClientSize.Width - m_pbUpdateCatalog.Width) / 2, (this.ClientSize.Height - m_pbUpdateCatalog.Height) / 2);
         m_pbUpdateCatalog.Name = "m_pbUpdateCatalog";
         m_pbUpdateCatalog.Size = new Size(200, 96);
         m_pbUpdateCatalog.TabIndex = 2;
         m_pbUpdateCatalog.TabStop = false;
         this.Controls.Add(m_pbUpdateCatalog);
         m_pbUpdateCatalog.BringToFront();

         this.ImageList = m_hNodeimageList;
         this.ImageIndex = this.SelectedImageIndex = iImageListIndex("folder");
         this.SizeChanged += new EventHandler(this.ServerTree_SizeChanged);
         this.AfterSelect += new TreeViewEventHandler(this.ServerTree_AfterSelect);

         m_oAsyncThread1 = new System.Threading.Thread(new System.Threading.ThreadStart(SendAsyncRequest));
         m_oAsyncThread1.Start();

         m_oAsyncThread2 = new System.Threading.Thread(new System.Threading.ThreadStart(SendAsyncRequest));
         m_oAsyncThread2.Start();
      }

      protected override void Dispose(bool disposing)
      {
         EnqueueRequest(AsyncRequestType.Stop);
         EnqueueRequest(AsyncRequestType.Stop);

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
      protected virtual void OnSelectServer(Server server)
      {
         if (ServerSelect != null)
         {
            ServerSelect(this, server);
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

      public DataSet SelectedDataset
      {
         get
         {
            if (this.SelectedNode == null || !(this.SelectedNode.Tag is DataSet))
               return null;
            else
               return (DataSet)this.SelectedNode.Tag;
         }
      }

      #endregion

      #region Public Methods

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
#if !DAPPLE
      public bool AddServer(string strUrl, out Server hRetServer)
#else
      public bool AddServer(string strUrl, out Server hRetServer, string strCacheDir)
#endif
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

#if !DAPPLE
            Server oServer = new Server(strServerUrl);
#else
            Server oServer = new Server(strServerUrl, strCacheDir);
#endif
            if (oServer.Status == Server.ServerStatus.OnLine || oServer.Status == Server.ServerStatus.Maintenance)
            {
               m_oValidServerList.Remove(oServer.Url);
               m_oValidServerList.Add(oServer.Url, oServer);
               PopulateServerList();
               hRetServer = oServer;
            }
            m_oFullServerList.Remove(oServer.Url);
            m_oFullServerList.Add(oServer.Url, oServer);

            m_oServerList.RemoveServer(oServer);
            m_oServerList.AddServer(oServer);
         }
         catch (Exception e)
         {
            GetDapError.Instance.Write("Error adding dap server " + strUrl + " to the list.\n\r(" + e.Message + ")");
#if !DAPPLE
            CSYS.iClearErrAP();
#endif
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
         {
            m_oValidServerList.Remove(oServer.Url);
            PopulateServerList();
         }

         m_oFullServerList.Remove(oServer.Url);
         m_oServerList.RemoveServer(oServer);
      }

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

      /// <summary>
      /// Update the dataset count list
      /// </summary>
      public void UpdateCounts()
      {
         if (this.InvokeRequired)
            this.BeginInvoke(new MethodInvoker(this.RefreshCounts));
         else
            RefreshCounts();
      }

      /// <summary>
      /// Populate the list of servers
      /// </summary>
      bool m_bSelect = true;
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

         if (m_oCurServer != null && m_oValidServerList.ContainsValue(m_oCurServer))
            strServerUrl = m_oCurServer.Url;
         else
            m_oCurServer = null;

         if (strServerUrl == string.Empty)
#if !DAPPLE
            Constant.GetSelectedServerInSettingsMeta(out strServerUrl);
#else
            strServerUrl = "http://dap.geosoft.com";
#endif

         this.BeginUpdate();

         // --- Remove all but current server ---
         List<TreeNode> nodeList = new List<TreeNode>();
         foreach (TreeNode oTreeNode in this.Nodes)
            nodeList.Add(oTreeNode);
         foreach (TreeNode oTreeNode in nodeList)
         {
            if (m_oCurServer != oTreeNode.Tag as Server)
               oTreeNode.Remove();
         }

         int iInsert = 0;
         for (int i = 0; i < m_oValidServerList.Count; i++)
         {
            Server oServer = m_oValidServerList.Values[i];

            if (m_oCurServer == oServer)
               iInsert++;
            else
            {
               string str;
               int iImage;

               if (oServer.Url == strServerUrl)
                  oSelectServer = oServer;

               if (oServer.Secure && !oServer.LoggedIn)
               {
                  str = oServer.Name + " (unauthorized)";
                  iImage = iImageListIndex("offline");
               }
               else
               {
                  switch (oServer.Status)
                  {
                     case Server.ServerStatus.OnLine:
                        str = oServer.Name + " (" + oServer.DatasetCount.ToString() + ")";
                        iImage = iImageListIndex("enserver");
                        break;
                     case Server.ServerStatus.Maintenance:
                        str = oServer.Name + " (undergoing maintenance)";
                        iImage = iImageListIndex("disserver");
                        break;
                     case Server.ServerStatus.OffLine:
                        str = oServer.Name + " (offline)";
                        iImage = iImageListIndex("offline");
                        break;
                     case Server.ServerStatus.Disabled:
                        str = oServer.Name + " (disabled)";
                        iImage = iImageListIndex("disserver");
                        break;
                     default:
                        str = oServer.Name + " (unsupported)";
                        iImage = iImageListIndex("offline");
                        break;
                  }
               }
               TreeNode oTreeNode = new TreeNode(str, iImage, iImage);
               oTreeNode.Tag = oServer;

               this.Nodes.Insert(iInsert, oTreeNode);
            }

            iInsert++;
         }

         this.EndUpdate();

         m_bSelect = true;

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
            foreach (TreeNode oTreeNode in this.Nodes)
            {
               if (oServer == oTreeNode.Tag as Server)
               {
                  this.SelectedNode = oTreeNode;
#if DEBUG
                  System.Diagnostics.Debug.WriteLine("SelectedNode Changed (SelectServer): " + (this.SelectedNode != null ? this.SelectedNode.Text : "(none)"));
#endif
                  // --- The ServerTree_AfterSelect method will call OnSelectServer ---
                  return;
               }
            }
         }
         OnSelectServer(m_oCurServer);
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
         m_oCatalogBoundingBox = new Geosoft.Dap.Common.BoundingBox(searchExtents);
         EnqueueRequest(AsyncRequestType.GetCatalogHierarchy);
      }

      /// <summary>
      /// Refresh the catalog based on the new aoi
      /// </summary>
      public void FilterChanged(SearchModeEnum eMode, BoundingBox searchExtents, string strSearch, bool bAOIFilter, bool bTextFilter)
      {
         bool bChanged = false;

         m_eMode = eMode;
         m_bAOIFilter = bAOIFilter;
         m_bTextFilter = bTextFilter;

         // --- the catalog filter changed, do we need to do something ---

         if (m_bAOIFilter && m_oCatalogBoundingBox != searchExtents)
         {
            m_oCatalogBoundingBox = new Geosoft.Dap.Common.BoundingBox(searchExtents);
            bChanged = true;
         }

         CreateSearchString(strSearch);
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

      /// <summary>
      /// Request to the server failed to respond
      /// </summary>
      public void NoResponseError()
      {
         if (m_oValidServerList.Count > 0)
         {
            if (MessageBox.Show(this, "An error has occurred while attempting to retrieve the catalog from the dap server.\nDo you wish to disable this server?", "Failed to update catalog", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
               DisableServer(m_oCurServer);
               SelectServer(m_oValidServerList.Values[0]);
            }
         }
         else
         {
            MessageBox.Show(this, "An error has occurred while attempting to retrieve the catalog from the dap server.", "Failed to update catalog", MessageBoxButtons.OK, MessageBoxIcon.Information);
         }
      }


      /// <summary>
      /// Get the catalog hierarchy
      /// </summary>
      public void GetCatalogHierarchy()
      {
         string strConfigurationEdition = string.Empty;

         if (m_bEntireCatalogMode)
         {
            Catalog oCatalog = null;

            if (!m_bAOIFilter && !m_bTextFilter)
               oCatalog = m_oCurServer.CatalogCollection.GetCatalog(null, string.Empty);
            else if (!m_bAOIFilter && m_bTextFilter)
               oCatalog = m_oCurServer.CatalogCollection.GetCatalog(null, m_strSearchString);
            else if (m_bAOIFilter && !m_bTextFilter)
               oCatalog = m_oCurServer.CatalogCollection.GetCatalog(m_oCatalogBoundingBox, string.Empty);
            else if (m_bAOIFilter && m_bTextFilter)
               oCatalog = m_oCurServer.CatalogCollection.GetCatalog(m_oCatalogBoundingBox, m_strSearchString);

            if (oCatalog == null)
            {
               NoResponseError();
               return;
            }
            else
            {
               m_oCatalog = oCatalog.Document;
               strConfigurationEdition = oCatalog.ConfigurationEdition;
            }
         }
         else
         {
            bool bEntireCatalog;
            m_oCatalogHierarchyRoot = m_oCacheManager.GetCatalogHierarchyRoot(m_oCurServer, m_oCatalogBoundingBox, m_bAOIFilter, m_bTextFilter, m_strSearchString, out bEntireCatalog, out strConfigurationEdition);

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
            this.BeginInvoke(new MethodInvoker(this.RefreshResults));
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
            this.BeginInvoke(new MethodInvoker(this.RefreshResults));
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

         // --- ensure we only attempt to login 1 at a time ---

         lock (this)
         {
            // --- see if this server requires us to log in ---

            if (m_oCurServer.Secure)
            {
               // --- check to see if we have already successfully logged in to this server, if so then use that user name/password ---

               if (m_oCurServer.LoggedIn)
               {
                  m_oCurServer.SwitchToUser();
                  bRet = true;
               }
               else
               {
                  if (m_oCurServer.Login())
                     bRet = true;
               }
            }
            else
            {
               // --- pick up the default user name and password so we are not passing it to a server who doesn't need it ---

               m_oCurServer.SwitchToUser();
            }

            // --- if we are successfully logged in, then get the data from the dap server ---

            if (bRet)
            {
               SelectServer(m_oValidServerList[m_oCurServer.Url]);
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
         oRequest.m_eType = eRequest;
         oRequest.m_oParam1 = oParam1;
         oRequest.m_oParam2 = oParam2;
         oRequest.m_oParam3 = oParam3;
         oRequest.m_oParam4 = oParam4;
         oRequest.m_oParam5 = oParam5;
         m_oAsyncQueue.Enqueue(oRequest);
      }
#if DEBUG
      const string GEOLIB = "geolibd.dll";
#else 
      const string GEOLIB = "geolib.dll";
#endif
      [DllImport(GEOLIB, EntryPoint = "?hCreat_AP@@YAPAUh_ap@@PBD0@Z", CallingConvention = CallingConvention.Cdecl)]
      extern static IntPtr hCreat_AP(string strApplication, string strVersion);

      [DllImport(GEOLIB, EntryPoint = "?Destr_AP@@YAXPAUh_ap@@@Z", CallingConvention = CallingConvention.Cdecl)]
      extern static void Destr_AP(IntPtr oAP);

      [DllImport(GEOLIB, EntryPoint = "?hCreat_INI@@YAPAUh_ini@@PAUh_ap@@@Z", CallingConvention = CallingConvention.Cdecl)]
      extern static IntPtr hCreat_INI(IntPtr oAP);

      [DllImport(GEOLIB, EntryPoint = "?Destr_INI@@YAXPAUh_ini@@@Z", CallingConvention = CallingConvention.Cdecl)]
      extern static void Destr_INI(IntPtr oINI);

      [DllImport(GEOLIB, EntryPoint = "?hCreat_GXP@@YAPAUh_gxp@@PAUh_ap@@PAUh_ini@@@Z", CallingConvention = CallingConvention.Cdecl)]
      extern static IntPtr hCreat_GXP(IntPtr oAP, IntPtr oINI);

      [DllImport(GEOLIB, EntryPoint = "?Destr_GXP@@YAXPAUh_gxp@@@Z", CallingConvention = CallingConvention.Cdecl)]
      extern static IntPtr Destr_GXP(IntPtr oGXP);

      [DllImport(GEOLIB, EntryPoint = "?hCreatExtern_GXX@@YAPAUh_gxx@@PAUh_gxp@@@Z", CallingConvention = CallingConvention.Cdecl)]
      extern static IntPtr hCreatExtern_GXX(IntPtr oGXP);

      [DllImport(GEOLIB, EntryPoint = "?Destr_GXX@@YAXPAUh_gxx@@@Z", CallingConvention = CallingConvention.Cdecl)]
      extern static IntPtr Destr_GXX(IntPtr oGXX);

      /// <summary>
      /// Send an async request
      /// </summary>
      protected void SendAsyncRequest()
      {
         AsyncRequest oRequest;
#if !DAPPLE
         CGX_NET oGxNet;
         IntPtr oAP = IntPtr.Zero;
         IntPtr oGXP = IntPtr.Zero;
         IntPtr oGXX = IntPtr.Zero;
         IntPtr oINI = IntPtr.Zero;

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
#endif
         do
         {
            oRequest = (AsyncRequest)m_oAsyncQueue.Dequeue();

            // --- Safety try/catch so that a thread will not terminate, and not clean itself up ---

            try
            {
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
               GetDapError.Instance.Write(string.Format("Failed to send request {0} - {1}", oRequest.m_eType.ToString(), e.Message));
            }

         } while (oRequest.m_eType != AsyncRequestType.Stop);

#if !DAPPLE
         oGxNet.Dispose();
         if (oGXX != IntPtr.Zero) Destr_GXX(oGXX);
         if (oGXP != IntPtr.Zero) Destr_GXP(oGXP);
         if (oINI != IntPtr.Zero) Destr_INI(oINI);
         if (oAP != IntPtr.Zero) Destr_AP(oAP);
#endif
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
      /// Update the counts for each server
      /// </summary>
      protected void RefreshCounts()
      {
         // --- do not execute if we are cleanup up --- 

         if (this.IsDisposed)
            return;

         foreach (TreeNode oTreeNode in this.Nodes)
         {
            Server oServer = oTreeNode.Tag as Server;

            if (oServer.Secure && !oServer.LoggedIn)
               oTreeNode.Text = oServer.Name + " (unauthorized)";
            else if (oServer.Status == Server.ServerStatus.OnLine)
               oTreeNode.Text = oServer.Name + " (" + oServer.DatasetCount.ToString() + ")";
            else if (oServer.Status == Server.ServerStatus.Maintenance)
               oTreeNode.Text = oServer.Name + " (undergoing maintenance)";
         }
      }

      private FolderDatasetList GetDatasets(CatalogFolder oFolder)
      {
         return m_oCacheManager.GetDatasets(m_oCurServer, oFolder, m_oCatalogBoundingBox, m_bAOIFilter, m_bTextFilter, m_strSearchString);
      }

      /// <summary>
      /// Returns imagelist index from key name
      /// </summary>
      /// <param name="strKey"></param>
      /// <returns></returns>
      protected int iImageListIndex(string strKey)
      {
         return m_hNodeimageList.Images.IndexOfKey(strKey);
      }

      /// <summary>
      /// Get the image index based on the DAP data type
      /// </summary>
      /// <param name="strType"></param>
      /// <returns></returns>
      protected Int32 iImageIndex(string strType)
      {
         Int32 iRet = 3;

         switch (strType.ToLower())
         {
            case "database":
               iRet = iImageListIndex("dap_database");
               break;
            case "document":
               iRet = iImageListIndex("dap_document");
               break;
            case "generic":
               iRet = iImageListIndex("dap_map");
               break;
            case "grid":
               iRet = iImageListIndex("dap_grid");
               break;
            case "map":
               iRet = iImageListIndex("dap_map");
               break;
            case "picture":
               iRet = iImageListIndex("dap_picture");
               break;
            case "point":
               iRet = iImageListIndex("dap_point");
               break;
            case "spf":
               iRet = iImageListIndex("dap_spf");
               break;
            case "voxel":
               iRet = iImageListIndex("dap_voxel");
               break;
         }
         return iRet;
      }


      /// <summary>
      /// Display the catalog, after it has been modified
      /// </summary>
      protected void RefreshResults()
      {
         TreeNode oSelectedNode = null;

         // --- do not execute if we are cleanup up --- 

         if (this.IsDisposed) return;

         this.AfterSelect -= new TreeViewEventHandler(ServerTree_AfterSelect);
         this.TreeNodeChecked -= new TreeNodeCheckedEventHandler(ServerTree_TreeNodeChecked);
         m_pbUpdateCatalog.Visible = false;

         // --- Clear all server nodes ---
         this.BeginUpdate();

         foreach (TreeNode oTreeNode in this.Nodes)
            oTreeNode.Nodes.Clear();

         if (m_bEntireCatalogMode)
            oSelectedNode = DisplayEntireCatalog();
         else
            oSelectedNode = DisplayCatalog();

         this.SelectedNode = oSelectedNode;
#if DEBUG
         System.Diagnostics.Debug.WriteLine("SelectedNode Changed (RefreshResults): " + (this.SelectedNode != null ? this.SelectedNode.Text : "(none)"));
#endif
         this.ExpandAll();
         this.EndUpdate();
         this.AfterSelect += new TreeViewEventHandler(ServerTree_AfterSelect);
         this.TreeNodeChecked += new TreeNodeCheckedEventHandler(ServerTree_TreeNodeChecked);
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
                  hTreeNode.ImageIndex = hTreeNode.SelectedImageIndex = iImageListIndex("folder_open");
               }
               else
               {
                  TreeNode oTempNode = new TreeNode(str, iImageListIndex("folder_open"), iImageListIndex("folder_open"));
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

            hChildTreeNode = new TreeNode(oFolder.Name, iImageListIndex("folder"), iImageListIndex("folder"));
            hChildTreeNode.Tag = null;
            hTreeNode.Nodes.Add(hChildTreeNode);
         }

         FolderDatasetList oDatasetList = GetDatasets(oCurFolder);
         if (oDatasetList == null)
         {
            // --- updating in progress ---

            TreeNode hTempNode;
            hTempNode = new TreeNode("Retrieving Datasets...", iImageListIndex("loading"), iImageListIndex("loading"));
            hTempNode.Tag = null;
            hTreeNode.Nodes.Add(hTempNode);

            EnqueueRequest(AsyncRequestType.GetDatasetList, oCurFolder.Hierarchy, oCurFolder.Timestamp);
         }
         else
         {
            foreach (DataSet oDataset in oDatasetList.Datasets)
            {
               Int32 iType;
               TreeNode hChildTreeNode;

               iType = iImageIndex(oDataset.Type);
               if (m_hSelectedDataSets.ContainsKey(oDataset.UniqueName))
                  hChildTreeNode = this.Add(hTreeNode, oDataset.Title, iType, iType, TriStateTreeView.CheckBoxState.Checked);
               else
                  hChildTreeNode = this.Add(hTreeNode, oDataset.Title, iType, iType, TriStateTreeView.CheckBoxState.Unchecked);
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
                  hTreeNode.ImageIndex = hTreeNode.SelectedImageIndex = iImageListIndex("folder_open");
               }
               else
               {
                  hTreeNode.Nodes.Clear();

                  TreeNode oTempNode = new TreeNode(str, iImageListIndex("folder_open"), iImageListIndex("folder_open"));
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
               hChildTreeNode = new TreeNode(hAttr.Value, iImageListIndex("folder"), iImageListIndex("folder"));
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
               DataSet hDataSet;

               m_oCurServer.Command.Parser.DataSet(hChildNode, out hDataSet);

               iType = iImageIndex(hDataSet.Type);
               if (m_hSelectedDataSets.ContainsKey(hDataSet.UniqueName))
                  hChildTreeNode = this.Add(hTreeNode, hDataSet.Title, iType, iType, TriStateTreeView.CheckBoxState.Checked);
               else
                  hChildTreeNode = this.Add(hTreeNode, hDataSet.Title, iType, iType, TriStateTreeView.CheckBoxState.Unchecked);

               hChildTreeNode.Tag = hDataSet;
            }
         }
         return hTreeNode;
      }

      /// <summary>
      /// Clear the current catalog that we have displayed
      /// </summary>
      protected void ClearCatalog()
      {
         m_oCatalog = null;
         m_oCatalogHierarchyRoot = null;

         if (this.InvokeRequired)
         {
            this.Invoke(new MethodInvoker(ShowPictureBox));
            this.Invoke(new MethodInvoker(ClearTree));
         }
         else
         {
            ShowPictureBox();
            ClearTree();
         }
      }

      protected void ShowPictureBox()
      {
         m_pbUpdateCatalog.Visible = true;
      }

      protected void HidePictureBox()
      {
         m_pbUpdateCatalog.Visible = false;
      }

      protected void ClearTree()
      {
         this.SelectedNode = null;
#if DEBUG
         System.Diagnostics.Debug.WriteLine("SelectedNode Changed (ClearTree): " + (this.SelectedNode != null ? this.SelectedNode.Text : "(none)"));
#endif
         if (m_hCurServerTreeNode != null)
            m_hCurServerTreeNode.Nodes.Clear();
      }

      #endregion

      #region TriStateTreeView Overrides

      protected override void OnMouseDown(System.Windows.Forms.MouseEventArgs e)
      {
         base.OnMouseDown(e);

         if (e.Button == MouseButtons.Right)
         {
            this.SelectedNode = GetNodeAt(e.X, e.Y);
#if DEBUG
            System.Diagnostics.Debug.WriteLine("SelectedNode Changed (OnMouseDown): " + (this.SelectedNode != null ? this.SelectedNode.Text : "(none)"));
#endif
         }
         else
            base.OnMouseDown(e);
      }

      #endregion

      #region Event Handlers

      /// <summary>
      /// Modify catalog browsing tree
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void ServerTree_AfterSelect(object sender, TreeViewEventArgs e)
      {
         if (m_bSelect)
         {
            m_bSelect = false;

            m_hCurrentNode = e.Node;

            if (!(m_hCurrentNode.Tag is DataSet))
            {
               m_strHierarchy = String.Empty;

               while (!(m_hCurrentNode.Tag is Server))
               {
                  m_strHierarchy = m_hCurrentNode.Text + "/" + m_strHierarchy;
                  m_hCurrentNode = m_hCurrentNode.Parent;
               }

               if (m_hCurrentNode.Tag is Server && m_hCurrentNode.Tag as Server != m_oCurServer)
               {
                  m_oCurServer = m_hCurrentNode.Tag as Server;
                  m_hCurServerTreeNode = m_hCurrentNode;
                  OnSelectServer(m_oCurServer);
               }
               else
                  RefreshResults();
            }

            // --- This is needed because all the docking window activations tends to change the tree selection on us ---
            this.SelectedNode = e.Node;
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
      private void ServerTree_TreeNodeChecked(object sender, Geosoft.DotNetTools.TreeNodeCheckedEventArgs e)
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

      /// <summary>
      /// Move Picture box during sizing
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void ServerTree_SizeChanged(object sender, EventArgs e)
      {
         m_pbUpdateCatalog.Top = (this.ClientSize.Height - m_pbUpdateCatalog.Height) / 2;
         m_pbUpdateCatalog.Left = (this.ClientSize.Width - m_pbUpdateCatalog.Width) / 2;
      }

      #endregion
   }
}
