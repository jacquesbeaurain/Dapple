using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;


namespace Geosoft.DotNetTools
{   
	/// <summary>
	/// Hold a button inside of a datagrid column
	/// </summary>
	public class DataGridCheckBoxColumn : DataGridBoolColumn
	{
      #region Member Variables
      /// <summary>
      /// Are we editing this cell
      /// </summary>
      protected bool m_bIsEditing = false;
      #endregion

      #region Default Constructor
      /// <summary>
      /// 	<para>Initializes an instance of the <see cref="DataGridCheckBoxColumn"/> class.</para>
      /// </summary>
		public DataGridCheckBoxColumn() : base()
		{			         
		}      
      #endregion

      /// <summary>
      /// Edit the current cell
      /// </summary>
      /// <param name="source"></param>
      /// <param name="rowNum"></param>
      protected void Edit(CurrencyManager source, int rowNum)
      {
         object oValue = GetColumnValueAtRow(source, rowNum);

         if (oValue == null)
            SetColumnValueAtRow(source, rowNum, true);
         else if (Convert.ToBoolean(oValue))
            SetColumnValueAtRow(source, rowNum, false);
         else
            SetColumnValueAtRow(source, rowNum, true);
      }

      /// <summary>
      /// Handle the cell being edited
      /// </summary>
      /// <param name="source"></param>
      /// <param name="rowNum"></param>
      /// <param name="bounds"></param>
      /// <param name="readOnly"></param>
      /// <param name="instantText"></param>
      protected override void Edit(CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly, string instantText)
      {
         m_bIsEditing = true;
         
         Edit(source, rowNum);
         base.Edit (source, rowNum, bounds, readOnly, instantText);
         this.Invalidate();
         
         m_bIsEditing = false;
      }

      /// <summary>
      /// Handle the cell being edited
      /// </summary>
      /// <param name="source"></param>
      /// <param name="rowNum"></param>
      /// <param name="bounds"></param>
      /// <param name="readOnly"></param>
      protected override void Edit(CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly)
      {
         m_bIsEditing = true;

         Edit(source, rowNum);
         base.Edit (source, rowNum, bounds, readOnly);
         this.Invalidate();
         
         m_bIsEditing = false;
      }

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
         m_bIsEditing = true;

         Edit(source, rowNum);
         base.Edit(source, rowNum, bounds, readOnly, instantText, cellIsVisible);
         this.Invalidate();

         m_bIsEditing = false;
      }       

      /// <summary>
      /// Enter null value into cell
      /// </summary>
      protected override void EnterNullValue()
      {
         base.EnterNullValue ();
      }

	}   
}
