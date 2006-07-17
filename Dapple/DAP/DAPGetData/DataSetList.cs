using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

using Geosoft.Dap;
using Geosoft.DotNetTools;

namespace Geosoft.GX.DAPGetData
{
	/// <summary>
	/// Create a datagrid, specifically to hold datasets
	/// </summary>
	public class DataSetList : System.Windows.Forms.DataGrid
	{      
      #region Member Variables
      protected Int32                  m_iSelectedColumn = -1;
      protected Int32                  m_iTypeColumn = -1;
      protected Int32                  m_iTitleColumn = -1;
      protected Int32                  m_iNameColumn = -1;
      protected Int32                  m_iUrlColumn = -1;
      protected Int32                  m_iHierarchyColumn = -1;
      protected Int32                  m_iDapServerNameColumn = -1;
      protected Int32                  m_iEnableColumn = -1;

      protected Int32                  m_iCurRow = -1;
      protected System.Data.DataTable  m_hTable;      

      protected Int32                  m_iDeleteColumn = -1;
      protected bool                   m_bDeleteColumn = false;
      protected Bitmap                 m_hDeleteBitmap;

      protected Int32                  m_iPromoteColumn = -1;
      protected bool                   m_bPromoteColumn = false;
      protected Bitmap                 m_hPromoteBitmap;

      protected Int32                  m_iDemoteColumn = -1;
      protected bool                   m_bDemoteColumn = false;
      protected Bitmap                 m_hDemoteBitmap;

      protected bool                   m_bDapServerNameColumn = false;
      #endregion

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
      /// Define the row change event
      /// </summary>
      public event RowChangedHandler  RowChanged;

      /// <summary>
      /// Invoke the delegatae registered with the Click event
      /// </summary>
      protected virtual void OnRowChanged(RowChangedArgs e) 
      {
         if (RowChanged != null) 
         {
            RowChanged(this, e);
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

      /// <summary>
      /// Event arguments for enabling a datagrid row
      /// </summary>
      public class DataGridEnableEventArgs : EventArgs
      {
         private int      m_iColumnNumber;
         private int      m_iRowNumber;
         private bool     m_bEnableValue;

         /// <summary>
         /// Default constructor
         /// </summary>
         /// <param name="iRowNumber"></param>
         /// <param name="iColumnNumber"></param>
         /// <param name="bValue"></param>
         public DataGridEnableEventArgs(int iRowNumber, int iColumnNumber, bool bValue)
         {
            m_iRowNumber = iRowNumber;
            m_iColumnNumber = iColumnNumber;
            m_bEnableValue = bValue;
         } 

         public int ColumnNumber
         {
            get { return m_iColumnNumber; }
            set { m_iColumnNumber = value; }
         }

         public int RowNumber
         {
            get{ return m_iRowNumber; }
            set{ m_iRowNumber = value; }
         } 

         public bool EnableValue
         {
            get{ return m_bEnableValue; }
            set{ m_bEnableValue = value; }
         }
      }
      #endregion

      #region Properties
      /// <summary>
      /// Get the selected column number
      /// </summary>
      public Int32 SelectedColumn
      {
         get { return m_iSelectedColumn; }
      }

      /// <summary>
      /// Get the type column number
      /// </summary>
      public Int32 TypeColumn
      {
         get { return m_iTypeColumn; }
      }

      /// <summary>
      /// Get the title column number
      /// </summary>
      public Int32 TitleColumn
      {
         get { return m_iTitleColumn; }
      }

      /// <summary>
      /// Get the name column number
      /// </summary>
      public Int32 NameColumn
      {
         get { return m_iNameColumn; }
      }

      /// <summary>
      /// Get the url column number
      /// </summary>
      public Int32 UrlColumn
      {
         get { return m_iUrlColumn; }
      }

      /// <summary>
      /// Get the hierarchy column number
      /// </summary>
      public Int32 HierarchyColumn
      {
         get { return m_iHierarchyColumn; }
      }      
      

      /// <summary>
      /// Get the current row
      /// </summary>
      public Int32 CurRow
      {
         get { return m_iCurRow; }
      }
      #endregion

      #region Column
      public delegate void EnableCellEventHandler(object sender, DataGridEnableEventArgs e);

      /// <summary>
      /// Enable/Disable the checkbox column
      /// </summary>
      public class DataGridEnableBoolColumn : DataGridBoolColumn
      {
         public event EnableCellEventHandler CheckEnabled;

         private int m_iColumn;

         /// <summary>
         /// Default constructor
         /// </summary>
         /// <param name="iColumn"></param>
         public DataGridEnableBoolColumn(int iColumn)
         {
            m_iColumn = iColumn;
         }         

         /// <summary>
         /// See if this control should be enabled
         /// </summary>
         /// <param name="source"></param>
         /// <param name="rowNum"></param>
         /// <param name="value"></param>
         protected override void SetColumnValueAtRow(CurrencyManager source, int rowNum, object value)
         {
            bool bEnabled = true;

            if (CheckEnabled != null)
            {
               DataGridEnableEventArgs e = new DataGridEnableEventArgs(rowNum, m_iColumn, bEnabled);
               CheckEnabled(this, e);
               bEnabled = e.EnableValue;
            }

            if (bEnabled)
               base.SetColumnValueAtRow (source, rowNum, value);
         }
      }
      #endregion

      #region Constructor
      /// <summary>
      /// Default constructor
      /// </summary>
		public DataSetList() : this(false, false, false)
		{			                      
		}			

      /// <summary>
      /// Default constructor
      /// </summary>
      public DataSetList(bool bDapServerNameColumn, bool bDeleteColumn, bool bPromoteColumn) : base()
      {
#if !DAPPLE
         System.IO.Stream hStrm = this.GetType().Assembly.GetManifestResourceStream("Geosoft.GX.DAPGetData.images.trash.gif");
#else
         System.IO.Stream hStrm = this.GetType().Assembly.GetManifestResourceStream("Geosoft.Dapple.DAPGetData.images.trash.gif");
#endif
         m_hDeleteBitmap = new Bitmap(hStrm);            
         hStrm.Close();

#if !DAPPLE
         hStrm = this.GetType().Assembly.GetManifestResourceStream("Geosoft.GX.DAPGetData.images.up.gif");
#else
         hStrm = this.GetType().Assembly.GetManifestResourceStream("Geosoft.Dapple.DAPGetData.images.up.gif");
#endif
         m_hPromoteBitmap = new Bitmap(hStrm);            
         hStrm.Close();

#if !DAPPLE
         hStrm = this.GetType().Assembly.GetManifestResourceStream("Geosoft.GX.DAPGetData.images.down.gif");
#else
         hStrm = this.GetType().Assembly.GetManifestResourceStream("Geosoft.Dapple.DAPGetData.images.down.gif");
#endif
         m_hDemoteBitmap = new Bitmap(hStrm);            
         hStrm.Close();     

         m_bDeleteColumn = bDeleteColumn;
         m_bPromoteColumn = bPromoteColumn;
         m_bDapServerNameColumn = bDapServerNameColumn;

         CreateTable();
         CreateTableLayout();
      }	
      #endregion

      #region Protected Member Functions      
      protected void CreateTableLayout()
      {
         DataGridTableStyle      hTableStyle = new DataGridTableStyle();
         hTableStyle.MappingName = "Results";
         hTableStyle.HeaderForeColor = System.Drawing.SystemColors.ControlText;                       

         DataGridTextBoxColumn   hColumnName;
         DataGridTextBoxColumn   hColumnServerName;
         DataGridTextBoxColumn   hColumnUrl;
         DataGridTypeColumn      hColumnType;
         DataGridTextBoxColumn   hColumnHierarchy;         
         DataGridTextBoxColumn   hDapServerNameColumn;
         DataGridBoolColumn      hEnableColumn;
         DataGridEnableBoolColumn   hColumnSelected;

         m_iSelectedColumn = hTableStyle.GridColumnStyles.Count;
         hColumnSelected = new DataGridEnableBoolColumn(m_iSelectedColumn);         
         hColumnSelected.AllowNull = false;
         hColumnSelected.NullValue = null;
         hColumnSelected.ReadOnly = false; 
         hColumnSelected.FalseValue = false;
         hColumnSelected.TrueValue = true;
         hColumnSelected.HeaderText = "";
         hColumnSelected.MappingName = "Selected";
         hColumnSelected.Alignment = HorizontalAlignment.Center;
         hColumnSelected.Width = 24;
         hColumnSelected.CheckEnabled += new EnableCellEventHandler(SetEnableValues);
         hTableStyle.GridColumnStyles.Add(hColumnSelected);

         if (m_bDeleteColumn)
         {
            m_iDeleteColumn = hTableStyle.GridColumnStyles.Count;
            DataGridImageColumn  hColumnDelete = new DataGridImageColumn(m_iDeleteColumn, m_hDeleteBitmap);
            hColumnDelete.ReadOnly = true; 
            hColumnDelete.HeaderText = "";
            hColumnDelete.MappingName = "DeleteColumn";
            hColumnDelete.Alignment = HorizontalAlignment.Center;
            hColumnDelete.Width = 24;
            hTableStyle.GridColumnStyles.Add(hColumnDelete);
         }

         if (m_bPromoteColumn)
         {
            m_iPromoteColumn = hTableStyle.GridColumnStyles.Count;
            DataGridImageColumn  hColumnPromote = new DataGridImageColumn(m_iPromoteColumn, m_hPromoteBitmap);
            hColumnPromote.ReadOnly = true; 
            hColumnPromote.HeaderText = "";
            hColumnPromote.MappingName = "PromoteColumn";
            hColumnPromote.Alignment = HorizontalAlignment.Center;
            hColumnPromote.Width = 24;
            hTableStyle.GridColumnStyles.Add(hColumnPromote);

            m_iDemoteColumn = hTableStyle.GridColumnStyles.Count;
            DataGridImageColumn  hColumnDemote = new DataGridImageColumn(m_iDemoteColumn, m_hDemoteBitmap);
            hColumnDemote.ReadOnly = true; 
            hColumnDemote.HeaderText = "";
            hColumnDemote.MappingName = "DemoteColumn";
            hColumnDemote.Alignment = HorizontalAlignment.Center;
            hColumnDemote.Width = 24;
            hTableStyle.GridColumnStyles.Add(hColumnDemote);
         }
                  
         m_iTypeColumn = hTableStyle.GridColumnStyles.Count;
         hColumnType = new DataGridTypeColumn(m_iTypeColumn);
         hColumnType.Format = "";
         hColumnType.FormatInfo = null;
         hColumnType.HeaderText = "";
         hColumnType.MappingName = "Type";
         hColumnType.ReadOnly = true;
         hColumnSelected.Alignment = HorizontalAlignment.Center;
         hColumnType.Width = 24;
         hTableStyle.GridColumnStyles.Add(hColumnType);

         m_iTitleColumn = hTableStyle.GridColumnStyles.Count;
         hColumnName = new DataGridTextBoxColumn();
         hColumnName.Format = "";
         hColumnName.FormatInfo = null;
         hColumnName.HeaderText = "Name";
         hColumnName.MappingName = "Name";
         hColumnName.ReadOnly = true;
         hTableStyle.GridColumnStyles.Add(hColumnName);

         m_iNameColumn = hTableStyle.GridColumnStyles.Count;
         hColumnServerName = new DataGridTextBoxColumn();
         hColumnServerName.Format = "";
         hColumnServerName.FormatInfo = null;
         hColumnServerName.HeaderText = "Server Name";
         hColumnServerName.MappingName = "ServerName";
         hColumnServerName.ReadOnly = true;
         hColumnServerName.Width = 0;
         hTableStyle.GridColumnStyles.Add(hColumnServerName);         

         m_iUrlColumn = hTableStyle.GridColumnStyles.Count;
         hColumnUrl = new DataGridTextBoxColumn();
         hColumnUrl.Format = "";
         hColumnUrl.FormatInfo = null;
         hColumnUrl.HeaderText = "Url";
         hColumnUrl.MappingName = "Url";
         hColumnUrl.ReadOnly = true;
         hColumnUrl.Width = 0;
         hTableStyle.GridColumnStyles.Add(hColumnUrl);         

         m_iHierarchyColumn = hTableStyle.GridColumnStyles.Count;
         hColumnHierarchy = new DataGridTextBoxColumn();
         hColumnHierarchy.Format = "";
         hColumnHierarchy.FormatInfo = null;
         hColumnHierarchy.HeaderText = "Hierarchy";
         hColumnHierarchy.MappingName = "Hierarchy";
         hColumnHierarchy.ReadOnly = true;
         hTableStyle.GridColumnStyles.Add(hColumnHierarchy);                                            

         m_iDapServerNameColumn = hTableStyle.GridColumnStyles.Count;
         hDapServerNameColumn = new DataGridTextBoxColumn();
         hDapServerNameColumn.Format = "";
         hDapServerNameColumn.FormatInfo = null;
         hDapServerNameColumn.HeaderText = "Server";
         hDapServerNameColumn.MappingName = "DapServerName";
         hDapServerNameColumn.ReadOnly = true;
         hDapServerNameColumn.Width = 0;
         hTableStyle.GridColumnStyles.Add(hDapServerNameColumn);

         m_iEnableColumn = hTableStyle.GridColumnStyles.Count;
         hEnableColumn = new DataGridBoolColumn();         
         hEnableColumn.AllowNull = false;
         hEnableColumn.ReadOnly = true; 
         hEnableColumn.FalseValue = false;
         hEnableColumn.TrueValue = true;
         hEnableColumn.HeaderText = "";
         hEnableColumn.MappingName = "Enable";
         hEnableColumn.Width = 0;         
         hTableStyle.GridColumnStyles.Add(hEnableColumn);

         hTableStyle.AllowSorting = false;
         hTableStyle.AlternatingBackColor = System.Drawing.Color.LightGray;
         hTableStyle.BackColor = System.Drawing.Color.Gainsboro;
         hTableStyle.RowHeadersVisible = false;
         hTableStyle.RowHeaderWidth = 0;
         hTableStyle.BackColor = System.Drawing.Color.Silver;
         hTableStyle.ForeColor = System.Drawing.Color.Black;
         hTableStyle.GridLineColor = System.Drawing.Color.DimGray;
         hTableStyle.GridLineStyle = System.Windows.Forms.DataGridLineStyle.None;
         hTableStyle.HeaderBackColor = System.Drawing.Color.MidnightBlue;
         hTableStyle.HeaderFont = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Bold);
         hTableStyle.HeaderForeColor = System.Drawing.Color.White;
         hTableStyle.LinkColor = System.Drawing.Color.MidnightBlue;         
         hTableStyle.SelectionBackColor = System.Drawing.Color.CadetBlue;
         hTableStyle.SelectionForeColor = System.Drawing.Color.White;
        
         BorderStyle = System.Windows.Forms.BorderStyle.None;
         CaptionVisible = false;
         ParentRowsVisible = false;         
         AllowNavigation = false;
         DataMember = "";
         TableStyles.Add(hTableStyle);
      }

      protected void CreateTable()
      {
         System.Data.DataColumn  hColumn;

         m_hTable = new System.Data.DataTable("Results");
         m_hTable.DefaultView.AllowNew = false;
         m_hTable.DefaultView.AllowDelete = false;
         
         hColumn = new System.Data.DataColumn("Selected");
         hColumn.DataType = typeof(bool);
         hColumn.AllowDBNull = false;
         hColumn.DefaultValue = false;
         m_hTable.Columns.Add(hColumn);

         hColumn = new System.Data.DataColumn("Name");
         hColumn.DataType = System.Type.GetType("System.String");
         hColumn.AllowDBNull = false;
         hColumn.DefaultValue = String.Empty;
         m_hTable.Columns.Add(hColumn);

         hColumn = new System.Data.DataColumn("ServerName");
         hColumn.DataType = System.Type.GetType("System.String");
         hColumn.AllowDBNull = false;
         hColumn.DefaultValue = String.Empty;
         m_hTable.Columns.Add(hColumn);

         hColumn = new System.Data.DataColumn("Url");
         hColumn.DataType = System.Type.GetType("System.String");
         hColumn.AllowDBNull = false;
         hColumn.DefaultValue = String.Empty;
         m_hTable.Columns.Add(hColumn);

         hColumn = new System.Data.DataColumn("Type");
         hColumn.DataType = System.Type.GetType("System.String");
         hColumn.AllowDBNull = false;
         hColumn.DefaultValue = String.Empty;
         m_hTable.Columns.Add(hColumn);

         hColumn = new System.Data.DataColumn("Hierarchy");
         hColumn.DataType = System.Type.GetType("System.String");
         hColumn.AllowDBNull = false;
         hColumn.DefaultValue = String.Empty;
         m_hTable.Columns.Add(hColumn);       

         hColumn = new System.Data.DataColumn("DapServerName");
         hColumn.DataType = System.Type.GetType("System.String");
         hColumn.AllowDBNull = false;
         hColumn.DefaultValue = String.Empty;
         m_hTable.Columns.Add(hColumn);
         
         hColumn = new System.Data.DataColumn("Enable");
         hColumn.DataType = System.Type.GetType("System.Boolean");
         hColumn.AllowDBNull = false;
         hColumn.DefaultValue = true;
         m_hTable.Columns.Add(hColumn);

         // --- add unbound column ---

         m_hTable.Columns.Add("DeleteColumn");       
         m_hTable.Columns.Add("PromoteColumn");
         m_hTable.Columns.Add("DemoteColumn");

         DataSource = m_hTable;          
      }

      /// <summary>
      /// Auto-size a column 
      /// </summary>
      /// <param name="iCol"></param>
      protected void AutoSizeCol(int iCol)
      {
         float          fWidth = 0; 
         int            iNumRows = ((System.Data.DataTable)DataSource).Rows.Count; 
         Graphics       g = Graphics.FromHwnd(Handle); 
         StringFormat   strFormat = new StringFormat(StringFormat.GenericTypographic); 
         SizeF          Size; 
 
         // --- measure the table header length ---

         Size = g.MeasureString(TableStyles[0].GridColumnStyles[iCol].HeaderText, Font, 500, strFormat);
         fWidth = Size.Width;
 

         // --- measure each cell length ---

         for(int i = 0; i < iNumRows; ++ i) 
         { 
            Size = g.MeasureString(this[i, iCol].ToString(), Font, 500, strFormat); 
 
            if(Size.Width > fWidth) 
               fWidth = Size.Width; 
         } 
         g.Dispose(); 
 
         TableStyles[0].GridColumnStyles[iCol].Width = (int) fWidth + 15; // 15 is for leading and trailing padding 
      }            

      /// <summary>
      /// See if we should disable this row
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      protected void SetEnableValues(object sender, DataGridEnableEventArgs e)
      {
         e.EnableValue = (bool)m_hTable.Rows[CurrentRowIndex]["Enable"];         
      }
      #endregion

      #region Public Methods
      /// <summary>
      /// Populate the table from a list of datasets
      /// </summary>
      /// <param name="hDataSets"></param>
      public void PopulateTable(ArrayList hDataSets, SortedList hSelectedList)
      {
         PopulateTable(hDataSets, hSelectedList, null);
      }

      /// <summary>
      /// Populate the table from a list of datasets
      /// </summary>
      /// <param name="hDataSets"></param>
      public void PopulateTable(ArrayList hDataSets, SortedList hSelectedList, SortedList hEnabledList)
      {
#if DAPPLE_TODO
         System.Data.DataRow  hRow;

         m_hTable.DefaultView.AllowNew = true;
         m_hTable.DefaultView.AllowDelete = true;
         m_hTable.Clear();
                  
         foreach (Geosoft.Dap.Common.DataSet hDataSet in hDataSets)
         {
            hRow = m_hTable.NewRow();
            hRow["Selected"] = hSelectedList.Contains(hDataSet.UniqueName);
            hRow["Name"] = hDataSet.Title;
            hRow["ServerName"] = hDataSet.Name;
            hRow["Url"] = hDataSet.Url;
            hRow["Type"] = hDataSet.Type;
            hRow["Hierarchy"] = hDataSet.Hierarchy;
            hRow["DapServerName"] = ((Server)GetDapData.Instance.ServerList[hDataSet.Url]).Name;

            if (hEnabledList != null && !hEnabledList.Contains(hDataSet.UniqueName))
            {
               hRow["Selected"] = false;
               hRow["Hierarchy"] = "No extract permissions";
               hRow["DapServerName"] = string.Empty;
               hRow["Enable"] = false;
            }
            else
            {
               hRow["Enable"] = true;
            }

            m_hTable.Rows.Add(hRow);
         }                 
         m_hTable.DefaultView.AllowNew = false;
         m_hTable.DefaultView.AllowDelete = false;         

         ResizeGrid();        
 
         if (m_iCurRow > m_hTable.Rows.Count)
            m_iCurRow = -1;
#endif
      }

      /// <summary>
      /// Stretch the last column to fill up the entire datagrid area
      /// </summary>
      public void ResizeGrid()
      {         
         if (TableStyles.Count <= 0) return;

         AutoSizeCol(m_iTitleColumn);
         AutoSizeCol(m_iHierarchyColumn);

         if (m_bDapServerNameColumn)
            AutoSizeCol(m_iDapServerNameColumn);

         Int32 iTargetWidth = ClientSize.Width - SystemInformation.VerticalScrollBarWidth;
         Int32 iCurWidth = TableStyles[0].RowHeaderWidth;

         for (Int32 i = 0; i < TableStyles[0].GridColumnStyles.Count; i++)
         {
            iCurWidth += TableStyles[0].GridColumnStyles[i].Width;
         }

         if (iCurWidth < iTargetWidth)
         {
            int iColumn = TableStyles[0].GridColumnStyles.Count - 1;

            while (iColumn > 0 && TableStyles[0].GridColumnStyles[iColumn].Width == 0)
               iColumn--;

            TableStyles[0].GridColumnStyles[iColumn].Width += iTargetWidth - iCurWidth;
         }
      }

      /// <summary>
      /// Prompt to see if the user ment to delete this row
      /// </summary>
      public void DeleteCurRow()
      {
         if (m_iCurRow < 0 || m_iCurRow >= m_hTable.Rows.Count) return;

         if (MessageBox.Show("Are you sure you want to remove this dataset from the selected list?", "Confirm Dataset Removal", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
         {
            // --- send the dataset deleted event ---

            Geosoft.Dap.Common.DataSet hDataSet = this.CreateDataSet(m_iCurRow);
            OnDataSetDeleted(new DataSetArgs(hDataSet));
            m_iCurRow = -1;
         }
      }

      /// <summary>
      /// Prompt to see if the user ment to delete all the rows
      /// </summary>
      public void DeleteRows(bool bConfirm)
      {
         if (m_hTable.Rows.Count == 0) return;

         if (!bConfirm || MessageBox.Show("Are you sure you want to remove all datasets from the selected list?", "Confirm Dataset Removal", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
         {
            m_hTable.Rows.Clear();

            // --- send the dataset delete event ---

            OnDataSetsDeleted();
            m_iCurRow = -1;
         }
      }

      /// <summary>
      /// Create the dataset object at the specified row
      /// </summary>
      /// <param name="iRow"></param>
      /// <returns></returns>
      public Geosoft.Dap.Common.DataSet CreateDataSet(Int32 iRow)
      {
         if (m_hTable.Rows.Count < iRow) return null;

         Geosoft.Dap.Common.DataSet hDataSet = new Geosoft.Dap.Common.DataSet();
         
         hDataSet.Url = (string)this[iRow, m_iUrlColumn];
         hDataSet.Name = (string)this[iRow, m_iNameColumn];
         hDataSet.Title = (string)this[iRow, m_iTitleColumn];
         hDataSet.Hierarchy = (string)this[iRow, m_iHierarchyColumn];
         hDataSet.Type = (string)this[iRow, m_iTypeColumn];

         return hDataSet;
      }
      #endregion

      #region Overrides 
      /// <summary>
      /// Signal that the current row has changed and check to see if the checkbox state changed
      /// </summary>
      /// <param name="e"></param>
      protected override void OnMouseUp(MouseEventArgs e)
      {  
         DataGrid.HitTestInfo hti = HitTest(e.X, e.Y);
         
         try
         {            
            if (hti.Type == DataGrid.HitTestType.Cell)
            {
               if (m_iCurRow != hti.Row)
               {
                  OnRowChanged(new RowChangedArgs(m_iCurRow, hti.Row));
               }
               m_iCurRow = hti.Row;
               
               if (hti.Column == m_iSelectedColumn)
               {
                  this[hti.Row, hti.Column] = !(bool)this[hti.Row, hti.Column];

                  // --- send the dataset selected event ---

                  Geosoft.Dap.Common.DataSet hDataSet = this.CreateDataSet(hti.Row);                   
                  OnDataSetSelected(new DataSetSelectedArgs(hDataSet, (bool)this[hti.Row, hti.Column]));
               }
               else if (hti.Column == m_iDeleteColumn)
               {
                  // --- send the dataset delete event ---

                  DeleteCurRow();
               }
               else if (hti.Column == m_iPromoteColumn)
               {
                  // --- send the dataset promoted event ---

                  Geosoft.Dap.Common.DataSet hDataSet = this.CreateDataSet(hti.Row);
                  OnDataSetPromoted(new DataSetArgs(hDataSet));
               }
               else if (hti.Column == m_iDemoteColumn)
               {
                  // --- send the dataset promoted event ---

                  Geosoft.Dap.Common.DataSet hDataSet = this.CreateDataSet(hti.Row);
                  OnDataSetDemoted(new DataSetArgs(hDataSet));
               }
            }            
         } 
         catch
         {
         }         
      }      

      /// <summary>
      /// Select the full row on the mouse down action
      /// </summary>
      /// <param name="e"></param>
      protected override void OnMouseDown(MouseEventArgs e)
      {   
         DataGrid.HitTestInfo hti = HitTest(e.X, e.Y);

         if (hti.Type == DataGrid.HitTestType.Cell)
         {
            ResetSelection();
            Select(hti.Row);
         }         
      }

      public override bool PreProcessMessage(ref Message msg)
      {
         const Int32 WM_KEYDOWN = 0x100;

         Keys KeyCode = (Keys)(int)msg.WParam & Keys.KeyCode;

         
         // --- check to see if the delete key was pressed and that there is a visible delete icon ---

         if (msg.Msg == WM_KEYDOWN && KeyCode == Keys.Delete && m_iDeleteColumn >= 0)
         {
            DeleteCurRow();            
            return true;                                                                                         
         }
         return base.PreProcessMessage (ref msg);
      }

      #endregion
	}
}
