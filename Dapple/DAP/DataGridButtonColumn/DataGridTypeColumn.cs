using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;


namespace Geosoft.DotNetTools
{   
	/// <summary>
	/// Display an image in a cell, based on the contents of the cell. Used to display geosoft dataset types
	/// </summary>
	public class DataGridTypeColumn : DataGridImageColumn
	{      
      #region Member Variables
      /// <summary>
      /// Database image
      /// </summary>
      protected Bitmap  m_hDatabase;

      /// <summary>
      /// Document image
      /// </summary>
      protected Bitmap  m_hDocument;

      /// <summary>
      /// Generic image
      /// </summary>
      protected Bitmap  m_hGeneric;

      /// <summary>
      /// Grid image
      /// </summary>
      protected Bitmap  m_hGrid;

      /// <summary>
      /// Map image
      /// </summary>
      protected Bitmap  m_hMap;

      /// <summary>
      /// Picture image
      /// </summary>
      protected Bitmap  m_hPicture;

      /// <summary>
      /// Point image
      /// </summary>
      protected Bitmap  m_hPoint;

      /// <summary>
      /// SPF image
      /// </summary>
      protected Bitmap  m_hSPF;      
      #endregion

      #region Default Constructor
      /// <summary>
      /// Default constructor
      /// </summary>
      /// <param name="iColNum"></param>
		public DataGridTypeColumn(int iColNum) : base(iColNum, null)
		{			
         m_iColumnNum = iColNum;         

         try
         {
#if !DAPPLE
            string strNameSpace = "Geosoft.DotNetTools.";
#else
            string strNameSpace = "Geosoft.Dapple.";
#endif
            System.IO.Stream hStrm = this.GetType().Assembly.GetManifestResourceStream(strNameSpace + "DataGridButtonColumn.res.database.gif");
            m_hDatabase = new Bitmap(hStrm);            
            hStrm.Close();            

            hStrm = this.GetType().Assembly.GetManifestResourceStream(strNameSpace + "DataGridButtonColumn.res.document.gif");
            m_hDocument = new Bitmap(hStrm);            
            hStrm.Close();

            hStrm = this.GetType().Assembly.GetManifestResourceStream(strNameSpace + "DataGridButtonColumn.res.map.gif");
            m_hGeneric = new Bitmap(hStrm);            
            hStrm.Close();

            hStrm = this.GetType().Assembly.GetManifestResourceStream(strNameSpace + "DataGridButtonColumn.res.grid.gif");
            m_hGrid = new Bitmap(hStrm);            
            hStrm.Close();

            hStrm = this.GetType().Assembly.GetManifestResourceStream(strNameSpace + "DataGridButtonColumn.res.map.gif");
            m_hMap = new Bitmap(hStrm);            
            hStrm.Close();

            hStrm = this.GetType().Assembly.GetManifestResourceStream(strNameSpace + "DataGridButtonColumn.res.picture.gif");
            m_hPicture = new Bitmap(hStrm);            
            hStrm.Close();

            hStrm = this.GetType().Assembly.GetManifestResourceStream(strNameSpace + "DataGridButtonColumn.res.point.gif");
            m_hPoint = new Bitmap(hStrm);     
            hStrm.Close();

            hStrm = this.GetType().Assembly.GetManifestResourceStream(strNameSpace + "DataGridButtonColumn.res.spf.gif");
            m_hSPF = new Bitmap(hStrm);       
            hStrm.Close();
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
         m_hImage = m_hDocument;
         string   str = hDataGrid[rowNum, m_iColumnNum].ToString();
         
         switch (str.ToLower())
         {
            case "database":
               m_hImage = m_hDatabase;
               break;
            case "document":
               m_hImage = m_hDocument;
               break;
            case "generic":
               m_hImage = m_hGeneric;
               break;
            case "grid":
               m_hImage = m_hGrid;
               break;
            case "map":
               m_hImage = m_hMap;
               break;
            case "picture":
               m_hImage = m_hPicture;
               break;
            case "point":
               m_hImage = m_hPoint;
               break;
            case "spf":
               m_hImage = m_hSPF;
               break;        
         }

         base.Paint(g, bounds, source, rowNum, backBrush, foreBrush, alignToRight);         
      }
	}
}
