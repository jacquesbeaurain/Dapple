using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

using Geosoft.Dap;
using Geosoft.DotNetTools;
using Geosoft.WinFormsUI;

namespace Geosoft.GX.DAPGetData
{
	/// <summary>
	/// Summary description for SearchDocument.
	/// </summary>
	public class ResultsDocument : DockContent
	{     
      #region Member Variables
      protected string                 m_strHierarchy;
      protected Int32                  m_iCurPage = 1;
      protected Int32                  m_iNumPages;
      protected Int32                  m_iNumItemsPerPage = 25;
      protected Int32                  m_iDatasetCount = 0;
      protected Int32                  m_iTotalDatasetCount = 0;
      protected SortedList             m_hSelectedDataSets = new SortedList();

      protected bool                                  m_bListView = false;
      protected DataSetList                           m_dgResults;
      protected Geosoft.DotNetTools.TriStateTreeView  m_tvResults;
      protected TreeNode                              m_hResultsRoot;
      protected TreeNode                              m_hSelectedNode;

      protected Bitmap                                m_hTreeViewImage;
      protected Bitmap                                m_hListViewImage;

      protected System.Xml.XmlDocument                m_oCatalog;
      #endregion

      private System.Windows.Forms.Panel pPage;
      private System.Windows.Forms.Label lPage;
      private System.Windows.Forms.TextBox tbPage;
      private System.Windows.Forms.Label lOf;
      private System.Windows.Forms.Label lPageNumber;
      private System.Windows.Forms.PictureBox pbFirst;
      private System.Windows.Forms.PictureBox pbPrev;
      private System.Windows.Forms.PictureBox pbNext;
      private System.Windows.Forms.PictureBox pbLast;
      private System.Windows.Forms.Panel pSelect;
      private System.Windows.Forms.PictureBox pbView;
      private System.Windows.Forms.ContextMenu cmResults;
      private System.Windows.Forms.MenuItem miViewMeta;
      private System.Windows.Forms.ImageList imTVResults;
      private System.Windows.Forms.Panel pPageCenter;
      private System.Windows.Forms.PictureBox pbUpdateCatalog;
      private System.Windows.Forms.MainMenu mResults;
      private System.Windows.Forms.MenuItem miResults;
      private System.Windows.Forms.MenuItem miResultsViewInGoogleEarth;
      private System.Windows.Forms.MenuItem miZoomDataset;
      private System.Windows.Forms.MenuItem miSeparator1;
      private System.Windows.Forms.Label lNumResults;
      private System.ComponentModel.IContainer components;

      #region Events
      /// <summary>
      /// Define the dataset selected event
      /// </summary>
      public event DataSetSelectedHandler  DataSetSelected;

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

      /// <summary>
      /// Define the view meta event
      /// </summary>
      public event ViewMetaHandler  ViewMeta;

      /// <summary>
      /// Invoke the delegatae registered with the Click event
      /// </summary>
      protected virtual void OnViewMeta(ViewMetaArgs e) 
      {
         if (ViewMeta != null) 
         {
            ViewMeta(this, e);
         }
      }      
      #endregion

      #region Constructor/Destructor
      /// <summary>
      /// Default constructor
      /// </summary>
		public ResultsDocument()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();			

         m_dgResults = new DataSetList();
         m_dgResults.Dock = DockStyle.Fill;
         m_dgResults.Visible = false;         
         m_dgResults.ContextMenu = cmResults;
         m_dgResults.TabIndex = 0;
         m_dgResults.Scroll += new EventHandler(m_dgResults_Scroll);
         m_dgResults.DataSetSelected += new DataSetSelectedHandler(m_dgResults_DataSetSelected);
         
         m_tvResults = new Geosoft.DotNetTools.TriStateTreeView();
         m_tvResults.Dock = DockStyle.Fill;
         m_tvResults.Visible = true;         
         m_tvResults.HideSelection = false;
         m_tvResults.ImageList = imTVResults;
         m_tvResults.ShowPlusMinus = false;
         m_tvResults.ShowRootLines = false;
         m_tvResults.TabIndex = 0;
         m_tvResults.AfterSelect += new TreeViewEventHandler(m_tvResults_AfterSelect);
         m_tvResults.MouseDown +=new MouseEventHandler(m_tvResults_MouseDown);
         m_tvResults.TreeNodeChecked += new TreeNodeCheckedEventHandler(m_tvResults_TreeNodeChecked);
         
         m_hResultsRoot = new TreeNode("Catalog", 1, 1);
         m_hResultsRoot.Tag = null;
         m_tvResults.Nodes.Add(m_hResultsRoot);
         
         this.Controls.Add(m_tvResults);
         this.Controls.Add(m_dgResults);

         m_tvResults.BringToFront();
         m_dgResults.BringToFront();

         System.IO.Stream hStrm = this.GetType().Assembly.GetManifestResourceStream("Geosoft.GX.DAPGetData.images.listviewselected.gif");
         m_hListViewImage = new Bitmap(hStrm);            
         hStrm.Close();            

         hStrm = this.GetType().Assembly.GetManifestResourceStream("Geosoft.GX.DAPGetData.images.treeviewselected.gif");
         m_hTreeViewImage = new Bitmap(hStrm);            
         hStrm.Close();
         
         bool bListView;         
         Constant.GetResultViewInSettingsMeta(out bListView);

         if (bListView)
         {
            m_bListView = true;
            m_dgResults.Visible = true;
            m_tvResults.Visible = false;
            pPage.Visible = true;
         }
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
         System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(ResultsDocument));
         this.pPage = new System.Windows.Forms.Panel();
         this.pPageCenter = new System.Windows.Forms.Panel();
         this.pbLast = new System.Windows.Forms.PictureBox();
         this.pbNext = new System.Windows.Forms.PictureBox();
         this.pbPrev = new System.Windows.Forms.PictureBox();
         this.pbFirst = new System.Windows.Forms.PictureBox();
         this.lPageNumber = new System.Windows.Forms.Label();
         this.lOf = new System.Windows.Forms.Label();
         this.tbPage = new System.Windows.Forms.TextBox();
         this.lPage = new System.Windows.Forms.Label();
         this.pSelect = new System.Windows.Forms.Panel();
         this.pbView = new System.Windows.Forms.PictureBox();
         this.cmResults = new System.Windows.Forms.ContextMenu();
         this.miViewMeta = new System.Windows.Forms.MenuItem();
         this.miSeparator1 = new System.Windows.Forms.MenuItem();
         this.miZoomDataset = new System.Windows.Forms.MenuItem();
         this.imTVResults = new System.Windows.Forms.ImageList(this.components);
         this.pbUpdateCatalog = new System.Windows.Forms.PictureBox();
         this.mResults = new System.Windows.Forms.MainMenu();
         this.miResults = new System.Windows.Forms.MenuItem();
         this.miResultsViewInGoogleEarth = new System.Windows.Forms.MenuItem();
         this.lNumResults = new System.Windows.Forms.Label();
         this.pPage.SuspendLayout();
         this.pPageCenter.SuspendLayout();
         this.pSelect.SuspendLayout();
         this.SuspendLayout();
         // 
         // pPage
         // 
         this.pPage.Controls.Add(this.pPageCenter);
         this.pPage.Dock = System.Windows.Forms.DockStyle.Bottom;
         this.pPage.Location = new System.Drawing.Point(0, 478);
         this.pPage.Name = "pPage";
         this.pPage.Size = new System.Drawing.Size(560, 56);
         this.pPage.TabIndex = 1;
         this.pPage.Visible = false;
         // 
         // pPageCenter
         // 
         this.pPageCenter.Controls.Add(this.pbLast);
         this.pPageCenter.Controls.Add(this.pbNext);
         this.pPageCenter.Controls.Add(this.pbPrev);
         this.pPageCenter.Controls.Add(this.pbFirst);
         this.pPageCenter.Controls.Add(this.lPageNumber);
         this.pPageCenter.Controls.Add(this.lOf);
         this.pPageCenter.Controls.Add(this.tbPage);
         this.pPageCenter.Controls.Add(this.lPage);
         this.pPageCenter.Location = new System.Drawing.Point(168, 8);
         this.pPageCenter.Name = "pPageCenter";
         this.pPageCenter.Size = new System.Drawing.Size(200, 40);
         this.pPageCenter.TabIndex = 8;
         // 
         // pbLast
         // 
         this.pbLast.Image = ((System.Drawing.Image)(resources.GetObject("pbLast.Image")));
         this.pbLast.Location = new System.Drawing.Point(112, 24);
         this.pbLast.Name = "pbLast";
         this.pbLast.Size = new System.Drawing.Size(16, 16);
         this.pbLast.TabIndex = 7;
         this.pbLast.TabStop = false;
         this.pbLast.Click += new System.EventHandler(this.pbLast_Click);
         // 
         // pbNext
         // 
         this.pbNext.Image = ((System.Drawing.Image)(resources.GetObject("pbNext.Image")));
         this.pbNext.Location = new System.Drawing.Point(96, 24);
         this.pbNext.Name = "pbNext";
         this.pbNext.Size = new System.Drawing.Size(16, 16);
         this.pbNext.TabIndex = 6;
         this.pbNext.TabStop = false;
         this.pbNext.Click += new System.EventHandler(this.pbNext_Click);
         // 
         // pbPrev
         // 
         this.pbPrev.Image = ((System.Drawing.Image)(resources.GetObject("pbPrev.Image")));
         this.pbPrev.Location = new System.Drawing.Point(80, 24);
         this.pbPrev.Name = "pbPrev";
         this.pbPrev.Size = new System.Drawing.Size(16, 16);
         this.pbPrev.TabIndex = 5;
         this.pbPrev.TabStop = false;
         this.pbPrev.Click += new System.EventHandler(this.pbPrev_Click);
         // 
         // pbFirst
         // 
         this.pbFirst.Image = ((System.Drawing.Image)(resources.GetObject("pbFirst.Image")));
         this.pbFirst.Location = new System.Drawing.Point(64, 24);
         this.pbFirst.Name = "pbFirst";
         this.pbFirst.Size = new System.Drawing.Size(16, 16);
         this.pbFirst.TabIndex = 4;
         this.pbFirst.TabStop = false;
         this.pbFirst.Click += new System.EventHandler(this.pbFirst_Click);
         // 
         // lPageNumber
         // 
         this.lPageNumber.Location = new System.Drawing.Point(144, 0);
         this.lPageNumber.Name = "lPageNumber";
         this.lPageNumber.Size = new System.Drawing.Size(24, 23);
         this.lPageNumber.TabIndex = 3;
         this.lPageNumber.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
         // 
         // lOf
         // 
         this.lOf.Location = new System.Drawing.Point(120, 0);
         this.lOf.Name = "lOf";
         this.lOf.Size = new System.Drawing.Size(24, 23);
         this.lOf.TabIndex = 2;
         this.lOf.Text = "of";
         this.lOf.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
         // 
         // tbPage
         // 
         this.tbPage.Location = new System.Drawing.Point(72, 0);
         this.tbPage.Name = "tbPage";
         this.tbPage.Size = new System.Drawing.Size(48, 20);
         this.tbPage.TabIndex = 2;
         this.tbPage.Text = "";
         this.tbPage.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
         this.tbPage.TextChanged += new System.EventHandler(this.tbPage_TextChanged);
         // 
         // lPage
         // 
         this.lPage.Location = new System.Drawing.Point(40, 0);
         this.lPage.Name = "lPage";
         this.lPage.Size = new System.Drawing.Size(32, 23);
         this.lPage.TabIndex = 0;
         this.lPage.Text = "Page ";
         this.lPage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
         // 
         // pSelect
         // 
         this.pSelect.BackColor = System.Drawing.SystemColors.Control;
         this.pSelect.Controls.Add(this.lNumResults);
         this.pSelect.Controls.Add(this.pbView);
         this.pSelect.Dock = System.Windows.Forms.DockStyle.Top;
         this.pSelect.Location = new System.Drawing.Point(0, 0);
         this.pSelect.Name = "pSelect";
         this.pSelect.Size = new System.Drawing.Size(560, 40);
         this.pSelect.TabIndex = 0;
         // 
         // pbView
         // 
         this.pbView.Location = new System.Drawing.Point(8, 8);
         this.pbView.Name = "pbView";
         this.pbView.Size = new System.Drawing.Size(56, 24);
         this.pbView.TabIndex = 0;
         this.pbView.TabStop = false;
         this.pbView.Click += new System.EventHandler(this.pbView_Click);
         // 
         // cmResults
         // 
         this.cmResults.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                  this.miViewMeta,
                                                                                  this.miSeparator1,
                                                                                  this.miZoomDataset});
         // 
         // miViewMeta
         // 
         this.miViewMeta.Index = 0;
         this.miViewMeta.Text = "View Meta";
         this.miViewMeta.Click += new System.EventHandler(this.miViewMeta_Click);
         // 
         // miSeparator1
         // 
         this.miSeparator1.Index = 1;
         this.miSeparator1.Text = "-";
         // 
         // miZoomDataset
         // 
         this.miZoomDataset.Index = 2;
         this.miZoomDataset.Text = "&Zoom to Dataset Extents";
         this.miZoomDataset.Click += new System.EventHandler(this.miZoomDataset_Click);
         // 
         // imTVResults
         // 
         this.imTVResults.ImageSize = new System.Drawing.Size(16, 16);
         this.imTVResults.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imTVResults.ImageStream")));
         this.imTVResults.TransparentColor = System.Drawing.Color.Transparent;
         // 
         // pbUpdateCatalog
         // 
         this.pbUpdateCatalog.BackColor = System.Drawing.Color.White;
         this.pbUpdateCatalog.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
         this.pbUpdateCatalog.Image = ((System.Drawing.Image)(resources.GetObject("pbUpdateCatalog.Image")));
         this.pbUpdateCatalog.Location = new System.Drawing.Point(8, 48);
         this.pbUpdateCatalog.Name = "pbUpdateCatalog";
         this.pbUpdateCatalog.Size = new System.Drawing.Size(200, 96);
         this.pbUpdateCatalog.TabIndex = 2;
         this.pbUpdateCatalog.TabStop = false;
         // 
         // mResults
         // 
         this.mResults.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                 this.miResults});
         // 
         // miResults
         // 
         this.miResults.Index = 0;
         this.miResults.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                  this.miResultsViewInGoogleEarth});
         this.miResults.Text = "Results";
         // 
         // miResultsViewInGoogleEarth
         // 
         this.miResultsViewInGoogleEarth.Index = 0;
         this.miResultsViewInGoogleEarth.Text = "View in Google Earth";
         this.miResultsViewInGoogleEarth.Click += new System.EventHandler(this.miResultsViewInGoogleEarth_Click);
         // 
         // lNumResults
         // 
         this.lNumResults.Location = new System.Drawing.Point(64, 8);
         this.lNumResults.Name = "lNumResults";
         this.lNumResults.Size = new System.Drawing.Size(320, 23);
         this.lNumResults.TabIndex = 1;
         this.lNumResults.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
         // 
         // ResultsDocument
         // 
         this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
         this.BackColor = System.Drawing.SystemColors.Control;
         this.ClientSize = new System.Drawing.Size(560, 534);
         this.Controls.Add(this.pbUpdateCatalog);
         this.Controls.Add(this.pSelect);
         this.Controls.Add(this.pPage);
         this.DockableAreas = Geosoft.WinFormsUI.DockAreas.Document;
         this.HideOnClose = true;
         this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
         this.MaximizeBox = false;
         this.Menu = this.mResults;
         this.MinimizeBox = false;
         this.Name = "ResultsDocument";
         this.ShowInTaskbar = false;
         this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
         this.Text = "Results";
         this.Resize += new System.EventHandler(this.ResultsDocument_Resize);
         this.pPage.ResumeLayout(false);
         this.pPageCenter.ResumeLayout(false);
         this.pSelect.ResumeLayout(false);
         this.ResumeLayout(false);

      }
		#endregion            

      #region Public Methods
      /// <summary>
      /// Invalid drawing area until we get the results back
      /// </summary>
      public void UpdateCatalog()
      {
         m_oCatalog = null;
         pbUpdateCatalog.Visible = true;
         pbUpdateCatalog.Top = (this.ClientSize.Height - pbUpdateCatalog.Height) / 2;
         pbUpdateCatalog.Left = (this.ClientSize.Width - pbUpdateCatalog.Width) / 2;
         pbUpdateCatalog.BringToFront();

         if (m_bListView) 
         {
            m_dgResults.DeleteRows(false);
         }
         else 
         {
            m_tvResults.SelectedNode = null;
            m_hResultsRoot.Nodes.Clear();
         }
      }

      /// <summary>
      /// Update the contents of the datagrid
      /// </summary>
      public void UpdateResults(Catalog oCatalog)      
      {                  
         double                  dCount;
         
         m_iCurPage = 1;
         m_strHierarchy = String.Empty;
         pbUpdateCatalog.Visible = false;
         
         if (oCatalog != null) 
         {
            m_oCatalog = oCatalog.Document;
            m_iDatasetCount = GetDapData.Instance.CurServer.Command.Parser.CatalogCount(m_oCatalog);
            dCount = (double)m_iDatasetCount / (double)m_iNumItemsPerPage;
         } 
         else 
         {
            m_oCatalog = null;
            m_iDatasetCount = 0;
            dCount = 0;
         }

         m_iNumPages = Convert.ToInt32(System.Math.Ceiling(dCount));

         if (this.InvokeRequired)
         {
            this.BeginInvoke(new MethodInvoker(this.RefreshResults));
         } 
         else 
         {
            RefreshResults();
         }
      }
      
      /// <summary>
      /// Update the contents of the datagrid, as a result has been (de)selected from a different screen
      /// </summary>
      public void UpdateResults(Geosoft.Dap.Common.DataSet hDataSet, bool bAdded)      
      {
         if (bAdded)
         {
            if (!m_hSelectedDataSets.Contains(hDataSet.UniqueName)) m_hSelectedDataSets.Add(hDataSet.UniqueName, hDataSet);
         }
         else
         {
            m_hSelectedDataSets.Remove(hDataSet.UniqueName);
         }
         RefreshResults();
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
      /// Clear the list of selected datasets
      /// </summary>
      public void ClearSelectedDatasets()
      {
         m_hSelectedDataSets.Clear();
         RefreshResults();
      }      

      /// <summary>
      /// Enable/Disable whether we can see anything in this dialog
      /// </summary>
      /// <param name="bValid"></param>
      public void LoggedIn(bool bValid)
      {
         pSelect.Visible = bValid; 
         pbUpdateCatalog.Visible = bValid;            
         
         if (m_bListView) 
         {
            m_dgResults.Visible = bValid;
            pPage.Visible = bValid;
         }
         else 
         {
            m_tvResults.Visible = bValid;
         }
      }
      #endregion      

      #region Protected Methods
      /// <summary>
      /// Get the image index
      /// </summary>
      /// <param name="strType"></param>
      /// <returns></returns>
      protected Int32 iImageIndex(string strType)
      {
         Int32 iRet = 3;

         switch(strType.ToLower())
         {
            case "database":
               iRet = 2;
               break;
            case "document":
               iRet = 3;
               break;
            case "generic":
               iRet = 5;
               break;
            case "grid":
               iRet = 4;
               break;
            case "map":
               iRet = 5;
               break;
            case "picture":
               iRet = 6;
               break;
            case "point":
               iRet = 7;
               break;
            case "spf":
               iRet = 8;
               break;
         }
         return iRet;
      }

      /// <summary>
      /// Display the catalog, after it has been modified
      /// </summary>
      protected void RefreshResults()
      {     
         lPageNumber.Text = m_iNumPages.ToString();
         lNumResults.Text = string.Format("Displaying {0} of {1} datasets", m_iDatasetCount, m_iTotalDatasetCount);

         // --- do not execute if we are cleanup up --- 
         
         if (this.IsDisposed) return;

         if (m_bListView)
         {
            // --- display the catalog in list view form ---
            
            ArrayList               hDataSetList;
         
            pbView.Image = m_hListViewImage;

            tbPage.Text = m_iCurPage.ToString();

            if (m_oCatalog != null) 
            {
               GetDapData.Instance.CurServer.Command.Parser.Catalog(m_oCatalog, (m_iCurPage - 1) * m_iNumItemsPerPage, m_iNumItemsPerPage, out hDataSetList);
               m_dgResults.PopulateTable(hDataSetList, m_hSelectedDataSets);
            } 
            else 
            {
               m_dgResults.DeleteRows(false);
            }
         } 
         else 
         {
            pbView.Image = m_hTreeViewImage;

            m_tvResults.AfterSelect -= new TreeViewEventHandler(m_tvResults_AfterSelect);


            System.Xml.XmlNodeList  hNodeList;
            System.Xml.XmlNode      hCurNode;
            TreeNode                hTreeNode = null;
            string                  []strLevels;

            // --- display the catalog in tree view form ---

            m_hResultsRoot.Text = GetDapData.Instance.CurServer.Name;
            if (m_oCatalog == null) 
            {
               m_hResultsRoot.Nodes.Clear();
               return;
            }


            // --- get the catalog node --

            hCurNode = m_oCatalog.DocumentElement;
            hNodeList = hCurNode.SelectNodes("//" + Geosoft.Dap.Xml.Common.Constant.Tag.CATALOG_TAG);

            if (hNodeList == null || hNodeList.Count == 0) return;

            hCurNode = hNodeList[0];


            hTreeNode = m_hResultsRoot;            

            m_tvResults.BeginUpdate();            
            
            strLevels = m_strHierarchy.Split('/');
            for (int i = 0; i < strLevels.Length - 1; i++)
            {
               string                  str = strLevels[i];
               
               string   szPath = Geosoft.Dap.Xml.Common.Constant.Tag.COLLECTION_TAG + "[@" + Geosoft.Dap.Xml.Common.Constant.Attribute.NAME_ATTR + "=\"" + str + "\"]";

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
                     hTreeNode.ImageIndex = 1;
                     hTreeNode.SelectedImageIndex = 1;
                  } 
                  else 
                  {                  
                     hTreeNode.Nodes.Clear();                  
                     hTreeNode = new TreeNode(str, 1, 1); 
                     hTreeNode.Tag = null;
                  }
                  hCurNode = hNodeList[0];                  
               }
            }

            hTreeNode.Nodes.Clear();
            foreach (System.Xml.XmlNode hChildNode in hCurNode.ChildNodes)
            {
               System.Xml.XmlNode  hAttr = hChildNode.Attributes.GetNamedItem(Geosoft.Dap.Xml.Common.Constant.Attribute.NAME_ATTR);
               TreeNode            hChildTreeNode;

               if (hAttr == null) continue;

               if (hChildNode.Name == Geosoft.Dap.Xml.Common.Constant.Tag.COLLECTION_TAG)
               {
                  hChildTreeNode = new TreeNode(hAttr.Value, 0, 0);
                  hChildTreeNode.Tag = null;
                  hTreeNode.Nodes.Add(hChildTreeNode);                  
               }
            }

            foreach (System.Xml.XmlNode hChildNode in hCurNode.ChildNodes)
            {
               System.Xml.XmlNode  hAttr = hChildNode.Attributes.GetNamedItem(Geosoft.Dap.Xml.Common.Constant.Attribute.NAME_ATTR);
               TreeNode            hChildTreeNode;

               if (hAttr == null) continue;

               if (hChildNode.Name != Geosoft.Dap.Xml.Common.Constant.Tag.COLLECTION_TAG)
               {
                  Int32                      iType;
                  Geosoft.Dap.Common.DataSet hDataSet;

                  GetDapData.Instance.CurServer.Command.Parser.DataSet(hChildNode, out hDataSet);

                  iType = iImageIndex(hDataSet.Type);
                  if (m_hSelectedDataSets.Contains(hDataSet.UniqueName))
                     hChildTreeNode = m_tvResults.Add(hTreeNode, hDataSet.Title, iType, iType, TriStateTreeView.CheckBoxState.Checked);                
                  else
                     hChildTreeNode = m_tvResults.Add(hTreeNode, hDataSet.Title, iType, iType, TriStateTreeView.CheckBoxState.Unchecked);                

                  hChildTreeNode.Tag = hDataSet;
               }
            }
            
            m_tvResults.EndUpdate();
            m_tvResults.SelectedNode = hTreeNode;
            m_tvResults.ExpandAll();
            m_tvResults.AfterSelect += new TreeViewEventHandler(m_tvResults_AfterSelect);
         }
      }      

      /// <summary>
      /// Update the counts for each server
      /// </summary>
      protected void RefreshCounts()
      {
         // --- do not execute if we are cleanup up --- 
         
         if (this.IsDisposed) return;

         m_iTotalDatasetCount = GetDapData.Instance.CurServer.DatasetCount;
         lNumResults.Text = string.Format("Displaying {0} of {1} datasets", m_iDatasetCount, m_iTotalDatasetCount);

      }
      #endregion

      #region Event Handlers
      private void pbLast_Click(object sender, System.EventArgs e)
      {
         m_iCurPage = m_iNumPages;
         RefreshResults();
      }

      private void pbNext_Click(object sender, System.EventArgs e)
      {
         if (m_iCurPage < m_iNumPages)
         {
            m_iCurPage++;
            RefreshResults();
         }      
      }

      private void pbPrev_Click(object sender, System.EventArgs e)
      {
         if (m_iCurPage > 1)
         {
            m_iCurPage--;
            RefreshResults();
         }      
      }

      private void pbFirst_Click(object sender, System.EventArgs e)
      {
         m_iCurPage = 1;
         RefreshResults();      
      }

      private void tbPage_TextChanged(object sender, System.EventArgs e)
      {
         Int32 iPage;

         try
         {
            iPage = Int32.Parse(tbPage.Text);

            if (iPage >= 1 && iPage <= m_iNumPages)
            {
               m_iCurPage = iPage;
               RefreshResults();
            }
         } 
         catch (Exception ex)
         {
            GetDapError.Instance.Write("Refresh Results - " + ex.Message);
         }
      }

      /// <summary>
      /// Handle when a new dataset has been selected
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void m_dgResults_DataSetSelected(object sender, Geosoft.Dap.DataSetSelectedArgs e)
      {
         if (e.Selected)
         {
            if (!m_hSelectedDataSets.Contains(e.DataSet.UniqueName)) m_hSelectedDataSets.Add(e.DataSet.UniqueName, e.DataSet);               
         } 
         else
         {
            m_hSelectedDataSets.Remove(e.DataSet.UniqueName);                        
         }
         OnDataSetSelected(e);
      }

      /// <summary>
      /// Redraw the datagrid
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void m_dgResults_Scroll(object sender, EventArgs e)
      {
         m_dgResults.Invalidate();
      }

      /// <summary>
      /// Handle when a new dataset has been selected
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void m_tvResults_TreeNodeChecked(object sender, Geosoft.DotNetTools.TreeNodeCheckedEventArgs e)
      {
         DataSetSelectedArgs  ex = new DataSetSelectedArgs();         
         if (e.Node != null && e.Node.Tag != null)
         {
            ex.DataSet = (Geosoft.Dap.Common.DataSet)e.Node.Tag;

            if (e.State == Geosoft.DotNetTools.TriStateTreeView.CheckBoxState.Checked) 
            {
               if (!m_hSelectedDataSets.Contains(ex.DataSet.UniqueName)) m_hSelectedDataSets.Add(ex.DataSet.UniqueName, ex.DataSet);
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
      /// Toggle the view
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void pbView_Click(object sender, System.EventArgs e)
      {
         m_bListView = !m_bListView;

         if (m_bListView) 
         {
            m_dgResults.Visible = true;
            m_tvResults.Visible = false;
            pPage.Visible = true;

            Constant.SetResultViewInSettingsMeta(true);
         } 
         else
         {
            m_dgResults.Visible = false;
            m_tvResults.Visible = true;
            pPage.Visible = false;

            Constant.SetResultViewInSettingsMeta(false);
         }

         RefreshResults();
      }

      /// <summary>
      /// Modify catalog browsing tree
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void m_tvResults_AfterSelect(object sender, TreeViewEventArgs e)
      {
         TreeNode                      hCurNode = e.Node;
         Geosoft.Dap.Common.DataSet    hDataSet = (Geosoft.Dap.Common.DataSet)hCurNode.Tag;

         if (hDataSet == null)
         {
            m_strHierarchy = String.Empty;

            while (hCurNode != m_hResultsRoot)
            {
               m_strHierarchy = hCurNode.Text + "/" + m_strHierarchy;
               hCurNode = hCurNode.Parent;
            }         

            RefreshResults();
         }
      }      

      /// <summary>
      /// View the meta for the selected dataset
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void miViewMeta_Click(object sender, System.EventArgs e)
      {     
         string   strServerName = null;
         string   strServerUrl = null;

         if (m_bListView) 
         {
            if (m_dgResults.CurRow == -1) return;
            
            strServerName = (string)m_dgResults[m_dgResults.CurRow, m_dgResults.NameColumn];   
            strServerUrl = (string)m_dgResults[m_dgResults.CurRow, m_dgResults.UrlColumn];
         }
         else
         {
            if (m_hSelectedNode == null) return;

            Geosoft.Dap.Common.DataSet    hDataSet = (Geosoft.Dap.Common.DataSet)m_hSelectedNode.Tag;
            
            
            // --- make sure we have a dataset and not a folder ---

            if (hDataSet == null) return;

            strServerName = hDataSet.Name;
            strServerUrl = hDataSet.Url;
         }

         OnViewMeta(new ViewMetaArgs(strServerUrl, strServerName));   
      }

      private void miZoomDataset_Click(object sender, System.EventArgs e)
      {
         Geosoft.Dap.Common.BoundingBox   oBoundingBox;
         string                           strServerName = null;
         Geosoft.Dap.Common.DataSet       hDataSet = null;

         if (m_bListView) 
         {
            ArrayList               hDataSetList;

            if (m_dgResults.CurRow == -1) return;
            
            strServerName = (string)m_dgResults[m_dgResults.CurRow, m_dgResults.NameColumn];   
            
            if (m_oCatalog != null) 
            {
               GetDapData.Instance.CurServer.Command.Parser.Catalog(m_oCatalog, (m_iCurPage - 1) * m_iNumItemsPerPage, m_iNumItemsPerPage, out hDataSetList);

               foreach (Geosoft.Dap.Common.DataSet oDS in hDataSetList)
               {
                  if (oDS.Name == strServerName)
                  {
                     hDataSet = oDS;
                     break;
                  }
               }
            }             
         } 
         else 
         {            
            if (m_hSelectedNode == null) return;

            hDataSet = (Geosoft.Dap.Common.DataSet)m_hSelectedNode.Tag;
         }
            
         // --- make sure we have a dataset and not a folder ---

         if (hDataSet == null) return;
                     
         oBoundingBox = new Geosoft.Dap.Common.BoundingBox(hDataSet.Boundary);

         if (Constant.Reproject(hDataSet.Boundary, GetDapData.Instance.SearchExtents.CoordinateSystem))
         {
            GetDapData.Instance.ZoomTo(oBoundingBox);
         } 
         else 
         {
            MessageBox.Show(this, "Unable to zoom to the selected dataset extents. Could not reproject into coordinate system.",  "Unable to zoom to the selected dataset extents.");
         }
      }      

      /// <summary>
      /// Set the selected node
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void m_tvResults_MouseDown(object sender, MouseEventArgs e)
      {
         m_hSelectedNode = m_tvResults.GetNodeAt(e.X, e.Y);

         if (m_hSelectedNode != null && m_hSelectedNode.Tag != null)
            m_tvResults.ContextMenu = cmResults;
         else
            m_tvResults.ContextMenu = null;
      }      

      /// <summary>
      /// Move the paging controls to the middle of the form
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void ResultsDocument_Resize(object sender, System.EventArgs e)
      {
         pPageCenter.Left = pPage.Width / 2 - pPageCenter.Width / 2;      

         if (m_bListView) 
         {
            m_dgResults.ResizeGrid();
         }
      }
      #endregion      

      protected void IterateFolder(System.Xml.XmlNode oCurFolder, System.Xml.XmlNode oParentNode)
      {
         foreach (System.Xml.XmlNode oNode in oParentNode.ChildNodes)
         {
            if (String.Compare(oNode.Name, "collection", true) == 0)
            {
               System.Xml.XmlNode      oAttr;
               System.Xml.XmlElement   oFolder = oCurFolder.OwnerDocument.CreateElement("Folder");
               System.Xml.XmlElement   oNameNode;
               System.Xml.XmlElement   oOpenNode;
               System.Xml.XmlElement   oVisibilityNode;

               oCurFolder.AppendChild(oFolder);
               oAttr = oNode.Attributes.GetNamedItem("name");

               oNameNode = oCurFolder.OwnerDocument.CreateElement("name");
               oNameNode.InnerText = oAttr.Value;
               oFolder.AppendChild(oNameNode);

               oOpenNode = oCurFolder.OwnerDocument.CreateElement("open");
               oOpenNode.InnerText = "1";
               oFolder.AppendChild(oOpenNode);

               oVisibilityNode = oCurFolder.OwnerDocument.CreateElement("visibility");
               oVisibilityNode.InnerText = "1";
               oFolder.AppendChild(oVisibilityNode);
               
               IterateFolder(oFolder, oNode);
            }
            else if (String.Compare(oNode.Name, "item", true) == 0)
            {
               Geosoft.Dap.Common.DataSet       oDataset;
               Geosoft.Dap.Common.BoundingBox   oBoundingBox;

               GetDapData.Instance.CurServer.Command.Parser.DataSet(oNode, out oDataset);
               GetDapData.Instance.CurServer.Command.Parser.BoundingBox(oNode.FirstChild, out oBoundingBox);
               oDataset.Boundary = oBoundingBox;

               WriteDatasetToKML(oCurFolder, oDataset);
            }
         }
      }

      protected void WriteDatasetToKML(System.Xml.XmlNode oFolderNode, Geosoft.Dap.Common.DataSet oDataset)
      {
         System.Xml.XmlElement   oPlaceMarkNode;
         System.Xml.XmlElement   oNameNode;
         System.Xml.XmlElement   oStyleNode;
         System.Xml.XmlElement   oPolyStyleNode;
         System.Xml.XmlElement   oIconStyleNode;
         System.Xml.XmlElement   oStyleAttrNode;
         System.Xml.XmlElement   oCollectionNode;
         System.Xml.XmlElement   oPolygonNode;
         System.Xml.XmlElement   oOuterBoundaryNode;
         System.Xml.XmlElement   oLinearRingNode;
         System.Xml.XmlElement   oPointNode;
         System.Xml.XmlElement   oAltitudeModeNode;
         System.Xml.XmlElement   oCoordinatesNode;

         oPlaceMarkNode = oFolderNode.OwnerDocument.CreateElement("Placemark");
         oFolderNode.AppendChild(oPlaceMarkNode);

         oNameNode = oFolderNode.OwnerDocument.CreateElement("name");
         oNameNode.InnerText = oDataset.Title;
         oPlaceMarkNode.AppendChild(oNameNode);

         oStyleNode = oFolderNode.OwnerDocument.CreateElement("Style");
         oPlaceMarkNode.AppendChild(oStyleNode);

         oPolyStyleNode = oFolderNode.OwnerDocument.CreateElement("PolyStyle");
         oStyleNode.AppendChild(oPolyStyleNode);

         oStyleAttrNode = oFolderNode.OwnerDocument.CreateElement("color");
         oStyleAttrNode.InnerText = "500000FF";
         oPolyStyleNode.AppendChild(oStyleAttrNode);

         oStyleAttrNode = oFolderNode.OwnerDocument.CreateElement("filled");
         oStyleAttrNode.InnerText = "1";
         oPolyStyleNode.AppendChild(oStyleAttrNode);
            
         oStyleNode = oFolderNode.OwnerDocument.CreateElement("Style");
         oPlaceMarkNode.AppendChild(oStyleNode);

         oIconStyleNode = oFolderNode.OwnerDocument.CreateElement("IconStyle");
         oStyleNode.AppendChild(oIconStyleNode);

         oStyleAttrNode = oFolderNode.OwnerDocument.CreateElement("color");
         oStyleAttrNode.InnerText = "FF0000FF";
         oIconStyleNode.AppendChild(oStyleAttrNode);

         oStyleAttrNode = oFolderNode.OwnerDocument.CreateElement("scale");
         oStyleAttrNode.InnerText = "2";
         oIconStyleNode.AppendChild(oStyleAttrNode);

         oCollectionNode = oFolderNode.OwnerDocument.CreateElement("GeometryCollection");
         oPlaceMarkNode.AppendChild(oCollectionNode);

         oPolygonNode = oFolderNode.OwnerDocument.CreateElement("Polygon");
         oCollectionNode.AppendChild(oPolygonNode);

         oAltitudeModeNode = oFolderNode.OwnerDocument.CreateElement("altitudeMode");
         oAltitudeModeNode.InnerText = "clampedToGround";
         oPolygonNode.AppendChild(oAltitudeModeNode);

         oOuterBoundaryNode = oFolderNode.OwnerDocument.CreateElement("outerBoundaryIs");
         oPolygonNode.AppendChild(oOuterBoundaryNode);

         oLinearRingNode = oFolderNode.OwnerDocument.CreateElement("LinearRing");
         oOuterBoundaryNode.AppendChild(oLinearRingNode);

         oCoordinatesNode = oFolderNode.OwnerDocument.CreateElement("coordinates");
         oCoordinatesNode.InnerText = oDataset.Boundary.MaxX + "," + oDataset.Boundary.MaxY + ",0 " + oDataset.Boundary.MaxX + "," + oDataset.Boundary.MinY + ",0 " + oDataset.Boundary.MinX + "," + oDataset.Boundary.MinY + ",0 " + oDataset.Boundary.MinX + "," + oDataset.Boundary.MaxY + ",0 " + oDataset.Boundary.MaxX + "," + oDataset.Boundary.MaxY + ",0 ";
         oLinearRingNode.AppendChild(oCoordinatesNode);

         // --- make point ---
         
         oPointNode = oFolderNode.OwnerDocument.CreateElement("Point");
         oCollectionNode.AppendChild(oPointNode);

         oAltitudeModeNode = oFolderNode.OwnerDocument.CreateElement("altitudeMode");
         oAltitudeModeNode.InnerText = "clampedToGround";
         oPointNode.AppendChild(oAltitudeModeNode);

         double dX = (oDataset.Boundary.MinX + oDataset.Boundary.MaxX) / 2;
         double dY = (oDataset.Boundary.MinY + oDataset.Boundary.MaxY) / 2;

         oCoordinatesNode = oFolderNode.OwnerDocument.CreateElement("coordinates");
         oCoordinatesNode.InnerText = dX + "," + dY + ",0";
         oPointNode.AppendChild(oCoordinatesNode);            
      }

      private void miResultsViewInGoogleEarth_Click(object sender, System.EventArgs e)
      {
         System.Xml.XmlDocument  oGoogleXml;
         System.Xml.XmlElement   oKmlNode;
         System.Xml.XmlElement   oFolderNode;
         System.Xml.XmlElement   oNameNode;
         System.Xml.XmlElement   oVisibilityNode;
         System.Xml.XmlElement   oOpenNode;
         
         System.Xml.XmlAttribute oAttr;

         System.Xml.XmlNode      oGeosoftXml;
         System.Xml.XmlNode      oResponseNode;
         System.Xml.XmlNode      oCatalogNode;
         
         oGoogleXml = new System.Xml.XmlDocument();
         
         oKmlNode = oGoogleXml.CreateElement("kml");
         oAttr = oGoogleXml.CreateAttribute("xmlns");
         oAttr.Value = "http://earth.google.com/kml/2.0";
         oKmlNode.Attributes.Append(oAttr);
         oGoogleXml.AppendChild(oKmlNode);

         oFolderNode = oGoogleXml.CreateElement("Folder");
         oKmlNode.AppendChild(oFolderNode);

         oNameNode = oGoogleXml.CreateElement("name");
         oNameNode.InnerText = "Geosoft DAP Result Set";
         oFolderNode.AppendChild(oNameNode);

         oOpenNode = oGoogleXml.CreateElement("open");
         oOpenNode.InnerText = "1";
         oFolderNode.AppendChild(oOpenNode);

         oVisibilityNode = oGoogleXml.CreateElement("visibility");
         oVisibilityNode.InnerText = "0";
         oFolderNode.AppendChild(oVisibilityNode);

         oGeosoftXml = m_oCatalog.DocumentElement;
         oResponseNode = oGeosoftXml.FirstChild;
         oCatalogNode = oResponseNode.FirstChild;
         IterateFolder(oFolderNode, oCatalogNode);

         string strTemp = "C:\\temp.kml";

         oGoogleXml.Save(strTemp);
      }

      private void ResultsDocument_Enter(object sender, System.EventArgs e)
      {
         GetDapData.Instance.Menu.MenuItems.Add(3, miResults);      
      }

      private void ResultsDocument_Leave(object sender, System.EventArgs e)
      {
         GetDapData.Instance.Menu.MenuItems.RemoveAt(3);
      }      
   }
}
