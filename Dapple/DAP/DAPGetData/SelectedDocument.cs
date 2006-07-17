using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

using Geosoft.WinFormsUI;
using Geosoft.Dap;
using WorldWind.Renderable;

namespace Geosoft.GX.DAPGetData
{
	/// <summary>
	/// Summary description for SearchDocument.
	/// </summary>
	public class SelectedDocument : DockContent
	{
      #region Events
      /// <summary>
      /// Define the dataset selected event
      /// </summary>
      public event DataSetSelectedHandler  DataSetSelected;

      /// <summary>
      /// Invoke the delegate registered with the Click event
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
      /// Invoke the delegate registered with the view meta event
      /// </summary>
      protected virtual void OnViewMeta(ViewMetaArgs e) 
      {
         if (ViewMeta != null) 
         {
            ViewMeta(this, e);
         }
      }

      /// <summary>
      /// Define the dataset deleted event
      /// </summary>
      public event DataSetDeletedHandler  DataSetDeleted;

      /// <summary>
      /// Invoke the delegate registered with the delete event
      /// </summary>
      protected virtual void OnDataSetDeleted(DataSetArgs e) 
      {
         if (DataSetDeleted != null) 
         {
            DataSetDeleted(this, e);
         }
      }

      /// <summary>
      /// Define the datasets deleted event
      /// </summary>
      public event EventHandler  DataSetsDeleted;

      /// <summary>
      /// Invoke the delegate registered with the delete event
      /// </summary>
      protected virtual void OnDataSetsDeleted() 
      {
         if (DataSetsDeleted != null) 
         {
            DataSetsDeleted(this, new EventArgs());
         }
      }
      
      /// <summary>
      /// Define the dataset promote event
      /// </summary>
      public event DataSetPromotedHandler  DataSetPromoted;

      /// <summary>
      /// Invoke the delegate registered with the promote event
      /// </summary>
      protected virtual void OnDataSetPromoted(DataSetArgs e) 
      {
         if (DataSetPromoted != null) 
         {
            DataSetPromoted(this, e);
         }
      }

      /// <summary>
      /// Define the dataset promote event
      /// </summary>
      public event DataSetDemotedHandler  DataSetDemoted;

      /// <summary>
      /// Invoke the delegate registered with the demote event
      /// </summary>
      protected virtual void OnDataSetDemoted(DataSetArgs e) 
      {
         if (DataSetDemoted != null) 
         {
            DataSetDemoted(this, e);
         }
      }
      #endregion

      #region Member Variables
      protected SortedList          m_hSelectedList = new SortedList();
      protected ArrayList           m_hDataSetList = new ArrayList();
      protected DataSetList         m_dgResults;
       private WorldWind.WorldWindow m_wwCtl;
      #endregion

      private System.Windows.Forms.ContextMenu cmSelected;
      private System.Windows.Forms.MenuItem miViewMeta;
      private System.Windows.Forms.ToolBar tbbSelected;
      private System.Windows.Forms.ToolBarButton tbbDelete;
      private System.Windows.Forms.ToolBarButton tbbDeleteAll;
      private System.Windows.Forms.ImageList ilTools;
       private MenuItem miSettings;
      private System.ComponentModel.IContainer components;

      #region Constructor/Destructor
      /// <summary>
      /// Default constructor
      /// </summary>
		public SelectedDocument(WorldWind.WorldWindow wwCtl)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

         this.m_wwCtl = wwCtl;

         m_dgResults = new DataSetList(true, true, true);
         m_dgResults.AllowNavigation = false;
         m_dgResults.Dock = DockStyle.Fill;
         m_dgResults.Visible = true;       
         m_dgResults.ContextMenu = cmSelected;
         m_dgResults.DataSetSelected += new DataSetSelectedHandler(m_dgResults_DataSetSelected);
         m_dgResults.DataSetDeleted += new DataSetDeletedHandler(m_dgResults_DataSetDeleted);
         m_dgResults.DataSetsDeleted +=new EventHandler(m_dgResults_DataSetsDeleted);
         m_dgResults.DataSetPromoted += new DataSetPromotedHandler(m_dgResults_DataSetPromoted);
         m_dgResults.DataSetDemoted += new DataSetDemotedHandler(m_dgResults_DataSetDemoted);

         this.Controls.Add(m_dgResults);
         m_dgResults.BringToFront();
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
          System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SelectedDocument));
          this.cmSelected = new System.Windows.Forms.ContextMenu();
          this.miViewMeta = new System.Windows.Forms.MenuItem();
          this.miSettings = new System.Windows.Forms.MenuItem();
          this.tbbSelected = new System.Windows.Forms.ToolBar();
          this.tbbDelete = new System.Windows.Forms.ToolBarButton();
          this.tbbDeleteAll = new System.Windows.Forms.ToolBarButton();
          this.ilTools = new System.Windows.Forms.ImageList(this.components);
          this.SuspendLayout();
          // 
          // cmSelected
          // 
          this.cmSelected.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.miViewMeta,
            this.miSettings});
          // 
          // miViewMeta
          // 
          this.miViewMeta.Index = 0;
          this.miViewMeta.Text = "View Meta";
          this.miViewMeta.Click += new System.EventHandler(this.miViewMeta_Click);
          // 
          // miSettings
          // 
          this.miSettings.Index = 1;
          this.miSettings.Text = "Settings";
          this.miSettings.Click += new System.EventHandler(this.miSettings_Click);
          // 
          // tbbSelected
          // 
          this.tbbSelected.Appearance = System.Windows.Forms.ToolBarAppearance.Flat;
          this.tbbSelected.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
            this.tbbDelete,
            this.tbbDeleteAll});
          this.tbbSelected.DropDownArrows = true;
          this.tbbSelected.ImageList = this.ilTools;
          this.tbbSelected.Location = new System.Drawing.Point(0, 0);
          this.tbbSelected.Name = "tbbSelected";
          this.tbbSelected.ShowToolTips = true;
          this.tbbSelected.Size = new System.Drawing.Size(560, 28);
          this.tbbSelected.TabIndex = 0;
          this.tbbSelected.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.tbbSelected_ButtonClick);
          // 
          // tbbDelete
          // 
          this.tbbDelete.ImageIndex = 0;
          this.tbbDelete.Name = "tbbDelete";
          this.tbbDelete.ToolTipText = "Remove highlighted datasets from selected list";
          // 
          // tbbDeleteAll
          // 
          this.tbbDeleteAll.ImageIndex = 1;
          this.tbbDeleteAll.Name = "tbbDeleteAll";
          this.tbbDeleteAll.ToolTipText = "Remove all datasets from selected list";
          // 
          // ilTools
          // 
          this.ilTools.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ilTools.ImageStream")));
          this.ilTools.TransparentColor = System.Drawing.Color.Transparent;
          this.ilTools.Images.SetKeyName(0, "");
          this.ilTools.Images.SetKeyName(1, "");
          // 
          // SelectedDocument
          // 
          this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
          this.ClientSize = new System.Drawing.Size(560, 486);
          this.Controls.Add(this.tbbSelected);
          this.DockableAreas = Geosoft.WinFormsUI.DockAreas.Document;
          this.HideOnClose = true;
          this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
          this.MaximizeBox = false;
          this.MinimizeBox = false;
          this.Name = "SelectedDocument";
          this.ShowInTaskbar = false;
          this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
          this.Text = "Selected";
          this.ResumeLayout(false);
          this.PerformLayout();

      }
		#endregion      

      #region Public Members
       
       /// <summary>
      /// Update the list of selected datasets
      /// </summary>
      public void UpdateResults(Geosoft.Dap.Common.DataSet hDataSet, bool bAdded)
      {
         if (bAdded)
         {
             Geosoft.Dap.Common.DataSet hDataSet2 = null;
             GetDapData.Instance.CurServer.Command.Parser.DataSet(GetDapData.Instance.CurrentCatalog.Document, hDataSet.Name, out hDataSet2);
            m_hDataSetList.Add(hDataSet);
            m_hSelectedList.Add(hDataSet.UniqueName, hDataSet);

             DownloadQueue.MaxConcurrentDownloads = 1;

             QuadLayerSettings settings = DAPLayerSettings.GetSettings(hDataSet.Title);

             RenderableObject oRO = this.m_wwCtl.CurrentWorld.RenderableObjects.GetObject(hDataSet.Title);
             if (oRO == null)
             {
                 string path = System.IO.Path.Combine(GetDapData.Instance.CurServer.CacheDirectory, hDataSet.Title);
                 GeosoftPlugin.New.DAPImageAccessor imgAccessor = new GeosoftPlugin.New.DAPImageAccessor(hDataSet.Name, 
                     GetDapData.Instance.CurServer,
                     path, Convert.ToInt32(settings.TileImageSize), Convert.ToDouble(settings.LevelZeroTileSize), Convert.ToInt32(settings.Levels), ".png", path,
                     /*new GeosoftPlugin.DAPLayerAccessor(),*/ System.IO.Path.GetDirectoryName(Application.ExecutablePath));

                 hDataSet.Boundary = hDataSet2.Boundary;

                 WorldWind.GeographicBoundingBox box = new WorldWind.GeographicBoundingBox(hDataSet.Boundary.MaxY,
                     hDataSet.Boundary.MinY, hDataSet.Boundary.MinX, hDataSet.Boundary.MaxX);

                 oRO = new QuadTileSet(hDataSet.Title, box, this.m_wwCtl.CurrentWorld, 0,
                     //null,
                     this.m_wwCtl.CurrentWorld.TerrainAccessor,
                     imgAccessor);
                 GetDapData.Instance.RenderableLayers.Add(oRO, hDataSet.Name);
                 this.m_wwCtl.CurrentWorld.RenderableObjects.Add(oRO);
             }
             else
             {
                 // remove and re-add to keep layer on top
                 this.m_wwCtl.CurrentWorld.RenderableObjects.Remove(oRO);
                 oRO.IsOn = true;
                 this.m_wwCtl.CurrentWorld.RenderableObjects.Add(oRO);
             }

             // goto location
             double lat = 0.5 * (hDataSet2.Boundary.MaxY + hDataSet2.Boundary.MinY);
             double lon = 0.5 * (hDataSet2.Boundary.MinX + hDataSet2.Boundary.MaxX);

             if (hDataSet2.Boundary.MinX > hDataSet2.Boundary.MaxX)
                lon += 180;

            this.m_wwCtl.GotoLatLon(lat, lon,
                0,
                double.NaN,
                180.0f,
                0);
             
         } 
         else 
         {
            for (Int32 i = 0; i < m_hDataSetList.Count; i++)
            {
               Geosoft.Dap.Common.DataSet hDS = (Geosoft.Dap.Common.DataSet)m_hDataSetList[i];

               if (hDS == hDataSet)
               {
                  m_hDataSetList.RemoveAt(i);
                  this.m_wwCtl.CurrentWorld.RenderableObjects.GetObject(hDS.Title).IsOn = false;
                  break;
               }
            }
            m_hSelectedList.Remove(hDataSet.UniqueName);
         }

         m_dgResults.PopulateTable(m_hDataSetList, m_hSelectedList);
      }

      /// <summary>
      /// Get the list of selected datasets
      /// </summary>
      /// <returns></returns>
      public ArrayList GetSelectedDatasets()
      {
         ArrayList   hList = new ArrayList();

         foreach (Geosoft.Dap.Common.DataSet hDataset in m_hDataSetList)
         {
            if (m_hSelectedList.Contains(hDataset.UniqueName))
               hList.Add(hDataset);
         }
         
         return hList;
      }

       public void RefreshResults()
       {
           
           //Catalog catalog =  GetDapData.Instance.CurServer.CatalogCollection.GetCatalog();
       }
      #endregion

      #region Event Handlers
      /// <summary>
      /// A dataset was selected or removed
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void m_dgResults_DataSetSelected(object sender, DataSetSelectedArgs e)
      {
         if (e.Selected)
         {
            if (!m_hSelectedList.Contains(e.DataSet.UniqueName)) m_hSelectedList.Add(e.DataSet.UniqueName, e.DataSet);         
         }
         else
         {
            m_hSelectedList.Remove(e.DataSet.UniqueName);
         }

         OnDataSetSelected(e);
      }      

      /// <summary>
      /// View the meta for the selected dataset
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void miViewMeta_Click(object sender, System.EventArgs e)
      {
         string strServerName;
         string strServerUrl;

         if (m_dgResults.CurRow == -1) return;
            
         strServerName = (string)m_dgResults[m_dgResults.CurRow, m_dgResults.NameColumn]; 
         strServerUrl = (string)m_dgResults[m_dgResults.CurRow, m_dgResults.UrlColumn];

         OnViewMeta(new ViewMetaArgs(strServerUrl, strServerName));
      }
      
      /// <summary>
      /// Delete the dataset from the list
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void m_dgResults_DataSetDeleted(object sender, DataSetArgs e)
      {
         UpdateResults(e.DataSet, false);

         
         // --- notify listeners that a dataset had been deleted from the selected list ---

         OnDataSetDeleted(e);
      }

      /// <summary>
      /// Delete all the datasets
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void m_dgResults_DataSetsDeleted(object sender, EventArgs e)
      {
         m_hDataSetList.Clear();
         m_hSelectedList.Clear();
         m_dgResults.PopulateTable(m_hDataSetList, m_hSelectedList);

         OnDataSetsDeleted();
      }

      /// <summary>
      /// Promote the current dataset 
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void m_dgResults_DataSetPromoted(object sender, DataSetArgs e)
      {
         for (Int32 i = 0; i < m_hDataSetList.Count; i++)
         {
            Geosoft.Dap.Common.DataSet hDS = (Geosoft.Dap.Common.DataSet)m_hDataSetList[i];

            if (hDS == e.DataSet)
            {
               if (i > 0) 
               {
                  object hTemp = m_hDataSetList[i - 1];
                  m_hDataSetList[i - 1] = hDS;
                  m_hDataSetList[i] = hTemp;
               }
               break;
            }
         }
         m_dgResults.PopulateTable(m_hDataSetList, m_hSelectedList);

         // --- notify listeners that a dataset had been promoted from the selected list ---

         OnDataSetPromoted(e);
      }

      /// <summary>
      /// Demote the current dataset
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void m_dgResults_DataSetDemoted(object sender, DataSetArgs e)
      {
         for (Int32 i = 0; i < m_hDataSetList.Count; i++)
         {
            Geosoft.Dap.Common.DataSet hDS = (Geosoft.Dap.Common.DataSet)m_hDataSetList[i];

            if (hDS == e.DataSet)
            {
               if (i < m_hDataSetList.Count - 1) 
               {
                  object hTemp = m_hDataSetList[i + 1];
                  m_hDataSetList[i + 1] = hDS;
                  m_hDataSetList[i] = hTemp;
               }
               break;
            }
         }
         m_dgResults.PopulateTable(m_hDataSetList, m_hSelectedList);


         // --- notify listeners that a dataset had been demoted from the selected list ---

         OnDataSetDemoted(e);
      }      

      /// <summary>
      /// Remove the selected datasets
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void tbbSelected_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
      {
         if (e.Button == tbbDelete)
         {
            m_dgResults.DeleteCurRow();
         }
         else if (e.Button == tbbDeleteAll)
         {
            m_dgResults.DeleteRows(true);
         }
      }
      #endregion      

       private void miSettings_Click(object sender, EventArgs e)
       {
           GetDapData.Instance.EditLayerSettings( (string)m_dgResults[m_dgResults.CurRow, m_dgResults.TitleColumn] );
       }
   }
}
