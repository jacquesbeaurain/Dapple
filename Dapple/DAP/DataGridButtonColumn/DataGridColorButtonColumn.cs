using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace Geosoft.DotNetTools
{
	/// <summary>
	/// Display a color button inside of a datagrid cell
	/// </summary>
	public class DataGridColorButtonColumn : DataGridTextButtonColumn
	{
      /// <summary>
      /// 	<para>Initializes an instance of the <see cref="DataGridColorButtonColumn"/> class.</para>
      /// </summary>
      /// <param name="iColNum">
      /// </param>
		public DataGridColorButtonColumn(int iColNum) : base(iColNum)
		{			
		}

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
         
         Color BackColor;
         Color ForeColor = bCurrent ? hDataGrid.SelectionForeColor : hDataGrid.ForeColor;
			
         
         int      iRed = 0;
         int      iGreen = 0;
         int      iBlue = 0;
         string   sz = GetColumnValueAtRow(source, rowNum).ToString();        
         string   szNum = "";
         int      iIndex;

         sz = sz.ToLower();

         iIndex = sz.IndexOf('r');
         if (iIndex != -1) 
         {
            iIndex++;
            while (iIndex < sz.Length && Char.IsDigit(sz[iIndex]) )
            {
               szNum += sz[iIndex++];
            }

            try { iRed = Convert.ToInt32(szNum); } 
            catch{}
         }
         szNum = "";

         iIndex = sz.IndexOf('g');
         if (iIndex != -1) 
         {
            iIndex++;
            while (iIndex < sz.Length && Char.IsDigit(sz[iIndex]))
            {
               szNum += sz[iIndex++];
            }

            try { iGreen = Convert.ToInt32(szNum); } 
            catch{}
         }
         szNum = "";

         iIndex = sz.IndexOf('b');
         if (iIndex != -1) 
         {
            iIndex++;
            while (iIndex < sz.Length && Char.IsDigit(sz[iIndex]))
            {
               szNum += sz[iIndex++];
            }

            try { iBlue = Convert.ToInt32(szNum); } 
            catch{}
         }
         
         BackColor = Color.FromArgb(iRed, iGreen, iBlue);

         // --- clear the cell ---

         g.FillRectangle(new SolidBrush(BackColor), bounds);


         // --- draw the button ---

         Bitmap hBitmap = m_iPressedRow == rowNum ? m_buttonFacePressed : m_buttonFace;
         g.DrawImage(hBitmap, bounds.Right - hBitmap.Width, bounds.Y);		
      }
	}
}
