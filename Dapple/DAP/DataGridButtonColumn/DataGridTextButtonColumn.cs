using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;


namespace Geosoft.DotNetTools
{   
	/// <summary>
	/// Hold a button inside of a datagrid column
	/// </summary>
	public class DataGridTextButtonColumn : DataGridButtonColumn
	{      
      #region Default Constructor
      /// <summary>
      /// Default constructor
      /// </summary>
      /// <param name="iColNum"></param>
		public DataGridTextButtonColumn(int iColNum) : base(iColNum)
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

            System.IO.Stream hStrm = this.GetType().Assembly.GetManifestResourceStream(strNameSpace + "DataGridButtonColumn.res.buttonface.bmp");
            m_buttonFace = new Bitmap(hStrm);

            hStrm = this.GetType().Assembly.GetManifestResourceStream(strNameSpace + "DataGridButtonColumn.res.buttonfacepressed.bmp");
            m_buttonFacePressed = new Bitmap(hStrm);
         } 
         catch{}
		}      
      #endregion

      /// <summary>
      /// Draw the button
      /// </summary>
      /// <param name="g"></param>
      /// <param name="bm"></param>
      /// <param name="bounds"></param>
      /// <param name="row"></param>
      protected override void DrawButton(Graphics g, Bitmap bm, Rectangle bounds, int row)
      {         
         g.DrawImage(bm, bounds.Right - bm.Width, bounds.Y);
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

         
         // --- set the colors to draw ---
         
         Color BackColor = bCurrent ? hDataGrid.SelectionBackColor : hDataGrid.BackColor;
         Color ForeColor = bCurrent ? hDataGrid.SelectionForeColor : hDataGrid.ForeColor;
			

         // --- clear the cell ---

         g.FillRectangle(new SolidBrush(BackColor), bounds);


         // --- draw the value ---

         string sz = GetColumnValueAtRow(source, rowNum).ToString();
         g.DrawString(sz, hDataGrid.Font, new SolidBrush(ForeColor), bounds);

         
         // --- draw the button ---

         Bitmap hBitmap = m_iPressedRow == rowNum ? m_buttonFacePressed : m_buttonFace;
         g.DrawImage(hBitmap, bounds.Right - hBitmap.Width, bounds.Y);		
      }
	}
}
