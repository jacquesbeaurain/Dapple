using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;


namespace Geosoft.DotNetTools
{   
	/// <summary>
	/// Display an image inside of a datagrid column
	/// </summary>
	public class DataGridImageColumn : DataGridTextBoxColumn
	{      
      #region Member Variables
      /// <summary>
      /// Image to display in datagrid cell
      /// </summary>
      protected Bitmap  m_hImage;    
  
      /// <summary>
      /// Column to display image in
      /// </summary>
      protected int     m_iColumnNum;      
      #endregion

      #region Properties
      /// <summary>
      /// Get/Set the current image
      /// </summary>
      public Bitmap CurImage
      {
         get { return m_hImage; }
         set { m_hImage = value; }
      }
      #endregion

      #region Default Constructor
      /// <summary>
      /// 	<para>Initializes an instance of the <see cref="DataGridImageColumn"/> class.</para>
      /// </summary>
      /// <param name="iColNum">
      /// </param>
      /// <param name="hBitmap">
      /// </param>
		public DataGridImageColumn(int iColNum, Bitmap hBitmap) : base()
		{			
         m_iColumnNum = iColNum;         
         m_hImage = hBitmap;
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
         // --- clear the background ---

         g.FillRectangle(backBrush, bounds);

         if (m_hImage != null)
         {
            // --- draw the image ---

            if (bounds.Height > m_hImage.Height) 
            {
               bounds.Y += (bounds.Height - m_hImage.Height) / 2;
               bounds.Height = m_hImage.Height;
            }
            
            if (bounds.Width > m_hImage.Width) 
            {
               bounds.X += (bounds.Width - m_hImage.Height) / 2;
               bounds.Width = m_hImage.Width;
            }         
            g.DrawImage(m_hImage, bounds);
         }
      }
	}
}
