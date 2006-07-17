using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;


namespace Geosoft.DotNetTools
{   
	/// <summary>
	/// Hold a button inside of a datagrid column
	/// </summary>
	public class DataGridButtonColumn : DataGridTextBoxColumn
	{
      #region Events
      /// <summary>
      /// Subscribe to a datagrid button clicked event
      /// </summary>
      public event DataGridCellButtonClickEventHandler   DataGridButtonClicked;
      
      /// <summary>
      /// Send event that the datagrid button was clicked
      /// </summary>
      /// <param name="e"></param>
      protected virtual void OnDataGridButtonClicked(DataGridCellButtonClickEventArgs e)
      {
         if (DataGridButtonClicked != null)
            DataGridButtonClicked(this, e);
      }
      #endregion

      #region Member Variables
      /// <summary>
      /// Bitmap of the button
      /// </summary>
      protected Bitmap  m_buttonFace;

      /// <summary>
      /// Bitmap of the button when pressed
      /// </summary>
      protected Bitmap  m_buttonFacePressed;

      /// <summary>
      /// Column to display button in
      /// </summary>
      protected int     m_iColumnNum;

      /// <summary>
      /// Row for this button
      /// </summary>
      protected int     m_iPressedRow;
      #endregion

      #region Default Constructor
      /// <summary>
      /// 	<para>Initializes an instance of the <see cref="DataGridButtonColumn"/> class.</para>
      /// </summary>
      /// <param name="iColNum">
      /// </param>
		public DataGridButtonColumn(int iColNum) : base()
		{			
         m_iColumnNum = iColNum;
         m_iPressedRow = -1;

         try
         {
#if !DAPPLE
            string strNameSpace = "Geosoft.DotNetTools.";
#else
            string strNameSpace = "Geosoft.Dapple.";
#endif

            System.IO.Stream hStrm = this.GetType().Assembly.GetManifestResourceStream(strNameSpace + "DataGridButtonColumn.res.fullbuttonface.bmp");
            m_buttonFace = new Bitmap(hStrm);

            hStrm = this.GetType().Assembly.GetManifestResourceStream(strNameSpace + "DataGridButtonColumn.res.fullbuttonfacepressed.bmp");
            m_buttonFacePressed = new Bitmap(hStrm);
         } 
         catch{}
		}      
      #endregion

      /// <summary>
      /// Handle the cell being edited
      /// </summary>
      /// <param name="source"></param>
      /// <param name="rowNum"></param>
      /// <param name="bounds"></param>
      /// <param name="readOnly"></param>
      /// <param name="instantText"></param>
      /// <param name="cellIsVisible"></param>
      protected override void Edit(System.Windows.Forms.CurrencyManager source, 
                                   int rowNum, 
                                   System.Drawing.Rectangle bounds, 
                                   bool readOnly, 
                                   string instantText, 
                                   bool cellIsVisible) 
      {          
         // no editing for this column
      } 

      /// <summary>
      /// Draw the button
      /// </summary>
      /// <param name="g"></param>
      /// <param name="bm"></param>
      /// <param name="bounds"></param>
      /// <param name="row"></param>
      protected virtual void DrawButton(Graphics g, Bitmap bm, Rectangle bounds, int row)
      {

         DataGrid hDataGrid = DataGridTableStyle.DataGrid;
         string sz = hDataGrid[row, m_iColumnNum].ToString();

         SizeF szF = g.MeasureString(sz, hDataGrid.Font, bounds.Width - 4, StringFormat.GenericTypographic);

         int x = bounds.Left + Math.Max(0, (bounds.Width - (int)szF.Width)/2);
         g.DrawImage(bm, bounds, 0, 0, bm.Width, bm.Height,GraphicsUnit.Pixel);
			
         if(szF.Height < bounds.Height)
         {
            int y = bounds.Top + (bounds.Height - (int) szF.Height) / 2;
            if(m_buttonFacePressed == bm)
            {
               x++;
            }

            g.DrawString(sz, hDataGrid.Font, new SolidBrush(hDataGrid.ForeColor), x, y);
         }

      }


      /// <summary>
      /// Check to see if the button was pressed, if so then launch the event
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      public void HandleMouseUp(object sender, MouseEventArgs e)
      {
         DataGrid             hDataGrid;
         DataGrid.HitTestInfo hHitTest;
         bool                 bIsClickInCell;
         Rectangle            hRect = new Rectangle(0,0,0,0);

         hDataGrid = this.DataGridTableStyle.DataGrid;
         hHitTest = hDataGrid.HitTest(new Point(e.X, e.Y));
         bIsClickInCell = hHitTest.Column == m_iColumnNum && hHitTest.Row > -1;

         m_iPressedRow = -1;         

         if(bIsClickInCell)
         {
            hRect = hDataGrid.GetCellBounds(hHitTest.Row, hHitTest.Column);
            bIsClickInCell = (e.X > hRect.Right - m_buttonFace.Width);
         }
         if(bIsClickInCell)
         {
            Graphics g = Graphics.FromHwnd(hDataGrid.Handle);
            DrawButton(g, m_buttonFace, hRect, hHitTest.Row);
            		
            g.Dispose();
            OnDataGridButtonClicked(new DataGridCellButtonClickEventArgs(hHitTest.Row, hHitTest.Column));
         }
      }

      /// <summary>
      /// Draw the button as pressed down
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      public void HandleMouseDown(object sender, MouseEventArgs e)
      {
         DataGrid             hDataGrid;
         DataGrid.HitTestInfo hHitTest;
         bool                 bIsClickInCell;
         Rectangle            hRect = new Rectangle(0,0,0,0);

         hDataGrid = this.DataGridTableStyle.DataGrid;
         hHitTest = hDataGrid.HitTest(new Point(e.X, e.Y));
         bIsClickInCell = hHitTest.Column == m_iColumnNum && hHitTest.Row > -1;

         if(bIsClickInCell)
         {
            hRect = hDataGrid.GetCellBounds(hHitTest.Row, hHitTest.Column);
            bIsClickInCell = (e.X > hRect.Right - m_buttonFace.Width);
         }

         if(bIsClickInCell)
         {
            Graphics g = Graphics.FromHwnd(hDataGrid.Handle);
            DrawButton(g, m_buttonFacePressed, hRect, hHitTest.Row);

            g.Dispose();
            m_iPressedRow = hHitTest.Row;
         }
      }

      /// <summary>
      /// Draw the button in the grid cell
      /// </summary>
      /// <param name="g"></param>
      /// <param name="bounds"></param>
      /// <param name="source"></param>
      /// <param name="rowNum"></param>
      /// <param name="backBrush"></param>
      /// <param name="foreBrush"></param>
      /// <param name="alignToRight"></param>
      protected override void Paint(System.Drawing.Graphics g, System.Drawing.Rectangle bounds, System.Windows.Forms.CurrencyManager source, int rowNum, System.Drawing.Brush backBrush, System.Drawing.Brush foreBrush, bool alignToRight)
      {
         DataGrid hDataGrid = DataGridTableStyle.DataGrid;

         
         // --- see if this cell is selected ----

         bool     bCurrent = hDataGrid.IsSelected(rowNum) || ( hDataGrid.CurrentRowIndex == rowNum && hDataGrid.CurrentCell.ColumnNumber == m_iColumnNum);                 

         
         // --- draw the button ---

         Bitmap hBitmap = m_iPressedRow == rowNum ? m_buttonFacePressed : m_buttonFace;
         g.DrawImage(hBitmap, bounds.Right - hBitmap.Width, bounds.Y);		
      }
	}

   #region Events
   /// <summary>
   /// Event arguments for when a button is clicked within a datagrid
   /// </summary>
   public class DataGridCellButtonClickEventArgs : EventArgs
   {
      #region Member Variables
      private int m_iRow;
      private int m_iCol;
      #endregion

      #region Constructor
      /// <summary>
      /// Default constructor
      /// </summary>
      /// <param name="iRow"></param>
      /// <param name="iCol"></param>
      public DataGridCellButtonClickEventArgs(int iRow, int iCol)
      {
         m_iRow = iRow;
         m_iCol = iCol;
      }
      #endregion

      #region Properties
      /// <summary>
      /// Get the row index
      /// </summary>
      public int RowIndex	
      {
         get { return m_iRow; }
      }

      /// <summary>
      /// Get the column index
      /// </summary>
      public int ColIndex	
      {
         get { return m_iCol; } 
      }
      #endregion
   }

   /// <summary>
   /// Represent the method that handles a datagrid button clicked event
   /// </summary>
   public delegate void DataGridCellButtonClickEventHandler(object sender, DataGridCellButtonClickEventArgs e);   
   #endregion

}
