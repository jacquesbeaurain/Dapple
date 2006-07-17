using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using Geosoft.WinFormsUI;

namespace Geosoft.GX.DAPGetData
{
	/// <summary>
	/// Summary description for SearchDocument.
	/// </summary>
	public class GetDataDocument : DockContent
	{
      #region Events
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

      #region Member Variables
      protected SortedList    m_hOptions = new SortedList();
      protected SortedList    m_hDataSets = new SortedList();
      protected SortedList    m_oRemoveQueue = new SortedList();
      protected SortedList    m_oSetupQueue = new SortedList();
      protected double        m_dRes;
      protected double        m_dImgRes;
      protected DataSetList   m_dgResults;
      #endregion

      private System.Windows.Forms.PropertyGrid pgOptions;
      private System.Windows.Forms.Label lCoordinateSystem;
      private System.Windows.Forms.Button bGetData;
      private System.Windows.Forms.ToolBarButton tbbGetData;
      private System.Windows.Forms.ToolBarButton tbbModifyCoordinateSystem;
      private System.Windows.Forms.ContextMenu cmResults;
      private System.Windows.Forms.MenuItem miViewMeta;
      private System.Windows.Forms.Label lCoord;
      private System.ComponentModel.IContainer components = null;

      #region Constructor/Destructor
      /// <summary>
      /// Default constructor
      /// </summary>
		public GetDataDocument()
		{         
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

         m_dgResults = new DataSetList(true, false, false);
         m_dgResults.AllowNavigation = false;
         m_dgResults.Dock = DockStyle.None;
         m_dgResults.Visible = true;       
         m_dgResults.Location = new System.Drawing.Point(8, 38);
         m_dgResults.Name = "dgResults";
         m_dgResults.ParentRowsVisible = false;
         m_dgResults.BorderStyle = BorderStyle.Fixed3D;
         m_dgResults.Size = new System.Drawing.Size(408, 225);
         m_dgResults.ContextMenu = cmResults;
         m_dgResults.RowChanged += new Geosoft.GX.DAPGetData.RowChangedHandler(m_dgResults_RowChanged);

         this.Controls.Add(m_dgResults);    
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
         System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(GetDataDocument));
         this.pgOptions = new System.Windows.Forms.PropertyGrid();
         this.lCoordinateSystem = new System.Windows.Forms.Label();
         this.bGetData = new System.Windows.Forms.Button();
         this.tbbGetData = new System.Windows.Forms.ToolBarButton();
         this.tbbModifyCoordinateSystem = new System.Windows.Forms.ToolBarButton();
         this.lCoord = new System.Windows.Forms.Label();
         this.cmResults = new System.Windows.Forms.ContextMenu();
         this.miViewMeta = new System.Windows.Forms.MenuItem();
         this.SuspendLayout();
         // 
         // pgOptions
         // 
         this.pgOptions.CommandsVisibleIfAvailable = true;
         this.pgOptions.LargeButtons = false;
         this.pgOptions.LineColor = System.Drawing.SystemColors.ScrollBar;
         this.pgOptions.Location = new System.Drawing.Point(8, 266);
         this.pgOptions.Name = "pgOptions";
         this.pgOptions.Size = new System.Drawing.Size(408, 248);
         this.pgOptions.TabIndex = 4;
         this.pgOptions.Text = "PropertyGrid";
         this.pgOptions.ToolbarVisible = false;
         this.pgOptions.ViewBackColor = System.Drawing.SystemColors.Window;
         this.pgOptions.ViewForeColor = System.Drawing.SystemColors.WindowText;
         // 
         // lCoordinateSystem
         // 
         this.lCoordinateSystem.BackColor = System.Drawing.SystemColors.ActiveBorder;
         this.lCoordinateSystem.Location = new System.Drawing.Point(138, 12);
         this.lCoordinateSystem.Name = "lCoordinateSystem";
         this.lCoordinateSystem.Size = new System.Drawing.Size(250, 16);
         this.lCoordinateSystem.TabIndex = 2;
         this.lCoordinateSystem.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
         this.lCoordinateSystem.Visible = false;
         // 
         // bGetData
         // 
         this.bGetData.Image = ((System.Drawing.Image)(resources.GetObject("bGetData.Image")));
         this.bGetData.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
         this.bGetData.Location = new System.Drawing.Point(8, 8);
         this.bGetData.Name = "bGetData";
         this.bGetData.Size = new System.Drawing.Size(78, 24);
         this.bGetData.TabIndex = 1;
         this.bGetData.Text = "Get Data";
         this.bGetData.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
         this.bGetData.Click += new System.EventHandler(this.bGetData_Click);
         // 
         // tbbGetData
         // 
         this.tbbGetData.ImageIndex = 1;
         this.tbbGetData.ToolTipText = "Get Data";
         // 
         // tbbModifyCoordinateSystem
         // 
         this.tbbModifyCoordinateSystem.ImageIndex = 0;
         this.tbbModifyCoordinateSystem.ToolTipText = "Modify Coordinate System";
         // 
         // lCoord
         // 
         this.lCoord.Location = new System.Drawing.Point(94, 12);
         this.lCoord.Name = "lCoord";
         this.lCoord.Size = new System.Drawing.Size(40, 16);
         this.lCoord.TabIndex = 43;
         this.lCoord.Text = "Coord";
         this.lCoord.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
         this.lCoord.Visible = false;
         // 
         // cmResults
         // 
         this.cmResults.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                  this.miViewMeta});
         // 
         // miViewMeta
         // 
         this.miViewMeta.Index = 0;
         this.miViewMeta.Text = "&View Meta";
         this.miViewMeta.Click += new System.EventHandler(this.miViewMeta_Click);
         // 
         // GetDataDocument
         // 
         this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
         this.BackColor = System.Drawing.SystemColors.Control;
         this.ClientSize = new System.Drawing.Size(424, 518);
         this.Controls.Add(this.lCoord);
         this.Controls.Add(this.lCoordinateSystem);
         this.Controls.Add(this.bGetData);
         this.Controls.Add(this.pgOptions);
         this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
         this.MaximizeBox = false;
         this.MinimizeBox = false;
         this.Name = "GetDataDocument";
         this.ShowInTaskbar = false;
         this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
         this.Text = "Get Data";
         this.ResumeLayout(false);

      }
		#endregion            

      #region Public Methods      
      /// <summary>
      /// Update the list of selected datasets
      /// </summary>
      public void UpdateGetData(Geosoft.Dap.Common.DataSet hDataSet, bool bAdded)
      {                  
         if (bAdded)
         {
            GetDapData.Instance.EnqueueRequest(GetDapData.AsyncRequestType.SetupExtract, hDataSet, null);
         } 
         else 
         {
            lock (this)
            {
               if (m_oSetupQueue.Contains(hDataSet.UniqueName)) 
               {
                  if (!m_oRemoveQueue.Contains(hDataSet.UniqueName))
                     // --- it has not been added to the results yet, put it in the remove queue ---
                     m_oRemoveQueue.Add(hDataSet.UniqueName, hDataSet.UniqueName);
               }
               else
               {               
                  DatasetOptions hOption = (DatasetOptions)m_hOptions[hDataSet.UniqueName];

                  if (hOption == pgOptions.SelectedObject) 
                  {
                     pgOptions.SelectedObject = null;
                  }
                  
                  m_hOptions.Remove(hDataSet.UniqueName);
                  m_hDataSets.Remove(hDataSet.UniqueName);

                  if (pgOptions.SelectedObject == null)
                  {
                     if (m_hOptions.Count > 0)
                     {
                        pgOptions.SelectedObject = m_hOptions.GetByIndex(0);
                     }
                     else
                     {
                        pgOptions.SelectedObject = null;
                     }
                  }
               }                
            
               // --- refresh the datagrid list ---

               UpdateTable();
            }
         }
      }

      /// <summary>
      /// Setup a dataset for extraction
      /// </summary>
      /// <param name="oDataSet"></param>
      public void SetupDataset(Geosoft.Dap.Common.DataSet hDataSet)
      {
         DatasetOptions hOption = null;

         try
         {
            lock (this)
            {
               if (!m_oSetupQueue.Contains(hDataSet.UniqueName))
               {
                  m_oSetupQueue.Add(hDataSet.UniqueName, null);
               }               
            }

            try
            {
               switch(hDataSet.Type.ToLower())
               {
                  case "map":
                     hOption = new Geosoft.GX.DAPGetData.MapOptions(hDataSet.Name, hDataSet.Title, hDataSet.Url);
                     ((MapOptions)hOption).GroupName = hDataSet.Title;
                     ((MapOptions)hOption).Resolution = hOption.CalculateDefaultResolution();
                     break;
                  case "point":
                     hOption = new Geosoft.GX.DAPGetData.PointOptions(hDataSet.Name, hDataSet.Title, hDataSet.Url);
                     ((PointOptions)hOption).Filename = System.IO.Path.ChangeExtension(hDataSet.Title, ".gdb");
                     break;
                  case "picture":                  
                     hOption = new Geosoft.GX.DAPGetData.PictureOptions(hDataSet.Name, hDataSet.Title, hDataSet.Url);
                     ((PictureOptions)hOption).Filename = hDataSet.Title;
                     ((PictureOptions)hOption).Display = PictureOptions.DisplayOptions.DownloadAndDisplay;
                     ((PictureOptions)hOption).Quality = PictureOptions.Qualities.SaveInDefaultFormat;
                     ((PictureOptions)hOption).Resolution = hOption.CalculateDefaultImageResolution();
                     break;
                  case "spf":
                     hOption = new Geosoft.GX.DAPGetData.SPFOptions(hDataSet.Name, hDataSet.Title, hDataSet.Url);
                     ((SPFOptions)hOption).GroupName = hDataSet.Title;
                     break;
                  case "database":
                     hOption = new Geosoft.GX.DAPGetData.DatabaseOptions(hDataSet.Name, hDataSet.Title, hDataSet.Url);
                     ((DatabaseOptions)hOption).Filename = System.IO.Path.ChangeExtension(hDataSet.Title, ".gdb");
                     break;
                  case "generic":
                     hOption = new Geosoft.GX.DAPGetData.GenericOptions(hDataSet.Name, hDataSet.Title, hDataSet.Url);
                     break;
                  case "document":
                  {
                     // --- get  the extension of this document from its meta ---

                     Geosoft.GXNet.CMETA  hDataSetMETA = null;
                     Int32                iAttribToken = GXNet.Constant.H_META_INVALID_TOKEN;
                     Int32                iClassToken = GXNet.Constant.H_META_INVALID_TOKEN;
                     string               strExtension = "doc";
         

                     hOption = new Geosoft.GX.DAPGetData.DocumentOptions(hDataSet.Name, hDataSet.Title, hDataSet.Url);

                     try
                     {
                        hDataSetMETA = hOption.META;

                        iClassToken = hDataSetMETA.ResolveUMN("CLASS:/Geosoft/Data/Document");
                        iAttribToken = hDataSetMETA.ResolveUMN("ATTRIB:/Geosoft/Data/Document/Extension");

                        hDataSetMETA.IGetAttribString(iClassToken, iAttribToken, ref strExtension);                  
                     } 
                     catch (Exception e)
                     {
                        GetDapError.Instance.Write("UpdateGetData (Document) - " + e.Message);
                     }
      
                              
                     ((DocumentOptions)hOption).Filename = System.IO.Path.ChangeExtension(hDataSet.Title, "." + strExtension);
                     ((DocumentOptions)hOption).Download = DocumentOptions.DownloadOptions.DownloadAndOpen;
                     break;
                  }
                  case "grid":
                     hOption = new Geosoft.GX.DAPGetData.GridOptions(hDataSet.Name, hDataSet.Title, hDataSet.Url);
                     ((GridOptions)hOption).Filename = System.IO.Path.ChangeExtension(hDataSet.Title, ".grd");
                     ((GridOptions)hOption).Download = GridOptions.DownloadOptions.ReprojectAndResample;
                     ((GridOptions)hOption).Display = GridOptions.DisplayOptions.ShadedColourImage;
                     ((GridOptions)hOption).Resolution = hOption.CalculateDefaultResolution();
                     break;
                  default:
                     System.Diagnostics.Debug.Assert(true, "Unhandled type: " + hDataSet.Type);
                     break;
               }
            } 
            catch (Exception e)
            {
               if (e.Message.ToLower().IndexOf("does not have permissions to carry out the requested operation") == -1)
               {
                  throw e;
               }
               hOption = null;
            }

            lock (this)
            {
               if (!m_oRemoveQueue.Contains(hDataSet.UniqueName))
               {
                  m_oSetupQueue.Remove(hDataSet.UniqueName);
                  m_hDataSets.Add(hDataSet.UniqueName, hDataSet);                                  
                  
                  if (hOption != null)
                     m_hOptions.Add(hDataSet.UniqueName, hOption);
                  

                  // --- Update the table ---

                  if (this.InvokeRequired)
                     this.Invoke(new MethodInvoker(this.UpdateTable));
               }
            }
         } 
         catch (Exception e)
         {            
               MessageBox.Show(this, string.Format("There was an error while trying to add \"{0}\" to the list of datasets to be extracted.\n\rPlease try again.\n\r\n\rError: {1}",hDataSet.Title, e.Message), "Error addding \"" + hDataSet.Title + "\" to extraction list", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
         }   
         finally
         {
            lock (this)
            {
               m_oRemoveQueue.Remove(hDataSet.UniqueName);
               m_oSetupQueue.Remove(hDataSet.UniqueName);
            }
         }
      }

      /// <summary>
      /// Update the datagrid table
      /// </summary>
      protected void UpdateTable()
      {
         // --- do not execute if we are cleanup up --- 
         
         if (this.IsDisposed) return;


         // --- refresh the datagrid list ---

         m_dgResults.PopulateTable(new ArrayList(m_hDataSets.Values), m_hDataSets, m_hOptions);

         if (pgOptions.SelectedObject == null && m_hOptions.Count > 0)
            pgOptions.SelectedObject = m_hOptions.GetByIndex(0);
      }

      public void SetMapState(bool bOpen)
      {
         lCoord.Visible = bOpen;
         lCoordinateSystem.Visible = bOpen;      
   
         if (bOpen && GetDapData.Instance.OMExtents != null)
         {
            lCoordinateSystem.Text = GetDapData.Instance.OMExtents.CoordinateSystem.ToString();
         }

         lock(this)
         {
            foreach (DatasetOptions oOption in m_hOptions.Values)
            {
               if (oOption == null)
               {
                  continue;
               }
               else if (oOption.GetType() == typeof(PictureOptions))
               {
                  ((PictureOptions)oOption).Resolution = oOption.CalculateDefaultImageResolution();
               }
               else if (oOption.GetType() == typeof(GridOptions))
               {
                  ((GridOptions)oOption).Resolution = oOption.CalculateDefaultResolution();
               }
               else if (oOption.GetType() == typeof(MapOptions))
               {
                  ((MapOptions)oOption).Resolution = oOption.CalculateDefaultResolution();
               }

               if (pgOptions.SelectedObject == oOption)
               {
                  pgOptions.SelectedObject = null;
                  pgOptions.SelectedObject = oOption;
               }
            }            
         }
      }

      /// <summary>
      /// Clear the list of selected datasets
      /// </summary>
      public void ClearSelectedDatasets()
      {
         lock (this)
         {
            m_hDataSets.Clear();
            m_hOptions.Clear();
            pgOptions.SelectedObject = null;

            UpdateTable();
         }
      }
      #endregion

      #region Event Handlers
      private void m_dgResults_RowChanged(object sender, RowChangedArgs e)
      {
         Geosoft.Dap.Common.DataSet hDataSet = m_dgResults.CreateDataSet(e.NewRow);
         
         pgOptions.SelectedObject = m_hOptions[hDataSet.UniqueName];
      }

      /// <summary>
      /// Extract all selected datasets
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void bGetData_Click(object sender, System.EventArgs e)
      {
         bool  bMapInfo = false;

         // --- clear any errors we have ---

         Geosoft.GXNet.CSYS.iClearErrAP();


         lock (this)
         {
            // --- handle weird mapinfo stuff ---

            if (m_hOptions.Count > 0) 
            {
               foreach (DatasetOptions hOption in m_hOptions.Values)
               {
                  if (hOption == null) continue;

                  if (hOption.Client == DatasetOptions.ClientType.MAPINFO) 
                  {
                     DatasetOptions.ClearMapInfoFileList();
                     DatasetOptions.NumMapInfoDatasets = 0;
                     bMapInfo = true;
                     break;
                  }
               }
            }

            for (Int32 iRow = 0; iRow < m_hOptions.Count; iRow++)
            {
               if ((bool)m_dgResults[iRow, m_dgResults.SelectedColumn])
               {
                  try
                  {
                     Geosoft.Dap.Common.DataSet hDataSet = m_dgResults.CreateDataSet(iRow);
                     DatasetOptions hOptions = (DatasetOptions)m_hOptions[hDataSet.UniqueName];
                  
                     if (hOptions == null) continue;


                     // --- disable interactive gx's ---

                     Geosoft.GXNet.CSYS.SetInteractive(0);
                     hOptions.DownloadDataset();
                  
                     Geosoft.GXNet.CSYS.SetInteractive(1);
                     Geosoft.GXNet.CSYS.Progress(0);

                     Int32 iNumErrors = Geosoft.GXNet.CSYS.iNumErrorsAP();

                     if (iNumErrors > 0)
                     {
                        Geosoft.GXNet.CGX_NET.ShowError();
                        Geosoft.GXNet.CSYS.iClearErrAP();
                     }
                  } 
                  catch (Exception ex)
                  {
                     Geosoft.GXNet.CSYS.SetInteractive(1);
                     Geosoft.GXNet.CSYS.Progress(0);

                     MessageBox.Show(string.Format("An error was encountered while downloading dataset \"{0}\"\n\r\n\rError: {1}", (string)m_dgResults[iRow, m_dgResults.TitleColumn], ex.Message), "Error downloading dataset", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                     Geosoft.GXNet.CSYS.iClearErrAP();
                  }
               }
            }

            if (bMapInfo) 
            {               
               Geosoft.GXNet.CSYS.SetInt("MAPINFO_DAP_CLIENT", "DATASETS2MAP", DatasetOptions.NumMapInfoDatasets);
               Geosoft.GXNet.CSYS.SetInt("MAPINFO_DAP_CLIENT", "GET_DATA", DatasetOptions.NumMapInfoDatasets);

               if (DatasetOptions.NumMapInfoDatasets != 0)
               {
                  // --- close the dialog so that in mapinfo we can load the datasets ---

                  GetDapData.Instance.Close();
               }            
            }
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

         if (m_dgResults.CurRow == -1) return;
            
         strServerName = (string)m_dgResults[m_dgResults.CurRow, m_dgResults.NameColumn];   
         strServerUrl = (string)m_dgResults[m_dgResults.CurRow, m_dgResults.UrlColumn];
         
         OnViewMeta(new ViewMetaArgs(strServerUrl, strServerName));      
      }
      #endregion
   }
}
