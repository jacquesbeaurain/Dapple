using System;
using System.Data;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;


namespace Geosoft.DotNetTools
{   
   /// <summary>
   /// Display a combobox inside of a datagrid cell
   /// </summary>
   public class DataGridComboBoxColumn : DataGridTextBoxColumn
   {
      #region Events
      /// <summary>
      /// Subscribe to the datagrid combobox value changed event
      /// </summary>
      public event DataGridComboBoxValueChangedHandler      DataGridComboBoxValueChanged;
      
      /// <summary>
      /// Send event that the combobox value changed
      /// </summary>
      /// <param name="iRow"></param>
      /// <param name="oNewValue"></param>
      protected virtual void OnDataGridComboBoxValueChanged(int iRow, object oNewValue)
      {
         if (DataGridComboBoxValueChanged != null)
            DataGridComboBoxValueChanged(this, iRow, oNewValue);
      }
      #endregion

      #region Member Functions
      /// <summary>
      /// Special combobox so that it does not affect datagrid cell navigation
      /// </summary>
      public      NoKeyUpCombo         m_cbColumn = null;
      private     int                  m_iRowNum;
      private     int                  m_iColNum;
      private     bool                 m_bIsEditing = false;      
      #endregion

      private System.Windows.Forms.CurrencyManager m_hSource = null;
           
      #region Constructor/Destructor
      /// <summary>
      /// 	<para>Initializes an instance of the <see cref="DataGridComboBoxColumn"/> class.</para>
      /// </summary>
      /// <param name="iColumn">
      /// </param>
      public DataGridComboBoxColumn(int iColumn) : base()
      {
         m_cbColumn = new NoKeyUpCombo();
         m_iColNum = iColumn;
		
         m_cbColumn.Leave += new EventHandler(m_cbColumn_Leave);
         m_cbColumn.SelectedIndexChanged += new System.EventHandler(m_cbColumn_SelectedIndexChanged);
         m_cbColumn.SelectionChangeCommitted += new System.EventHandler(m_cbColumn_SelectionChangeCommitted);
			
      }
      #endregion
		
      #region Event Handlers
      /// <summary>
      /// Display combo box for editing
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void m_cbColumn_SelectionChangeCommitted(object sender, EventArgs e)
      {
         m_bIsEditing = true;
         base.ColumnStartedEditing((Control) sender);
      }
		
      /// <summary>
      /// Notify that value has changed
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void m_cbColumn_SelectedIndexChanged(object sender, EventArgs e)
      {
         OnDataGridComboBoxValueChanged(m_iRowNum, m_cbColumn.Text); 	
      }

      /// <summary>
      /// Commit the value
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void m_cbColumn_Leave(object sender, EventArgs e)
      {
         if(m_bIsEditing)
         {
            SetColumnValueAtRow(m_hSource, m_iRowNum, m_cbColumn.SelectedItem.ToString());
            m_bIsEditing = false;
            Invalidate();
         }
         m_cbColumn.Hide();						
      }

      /// <summary>
      /// Edit this datacell
      /// </summary>
      /// <param name="source"></param>
      /// <param name="rowNum"></param>
      /// <param name="bounds"></param>
      /// <param name="readOnly"></param>
      /// <param name="instantText"></param>
      /// <param name="cellIsVisible"></param>
      protected override void Edit(System.Windows.Forms.CurrencyManager source, int rowNum, System.Drawing.Rectangle bounds, bool readOnly, string instantText, bool cellIsVisible)
      {
	

         base.Edit(source, rowNum, bounds, readOnly, instantText, cellIsVisible);

         m_iRowNum = rowNum;
         m_hSource = source;
		
         m_cbColumn.Parent = this.TextBox.Parent;
         m_cbColumn.Location = this.TextBox.Location;
         m_cbColumn.Size = new Size(this.TextBox.Size.Width, m_cbColumn.Size.Height);
         m_cbColumn.SelectedIndexChanged -= new System.EventHandler(m_cbColumn_SelectedIndexChanged);
         m_cbColumn.Text =  this.TextBox.Text;
         m_cbColumn.SelectedIndexChanged += new System.EventHandler(m_cbColumn_SelectedIndexChanged);

         this.TextBox.Visible = false;
         m_cbColumn.Visible = true;
         m_cbColumn.BringToFront();
         m_cbColumn.Focus();	
      }

      /// <summary>
      /// Commit the value to the datagrid
      /// </summary>
      /// <param name="dataSource"></param>
      /// <param name="rowNum"></param>
      /// <returns></returns>
      protected override bool Commit(System.Windows.Forms.CurrencyManager dataSource, int rowNum)
      {
         if(m_bIsEditing)
         {
            m_bIsEditing = false;
            SetColumnValueAtRow(dataSource, rowNum, m_cbColumn.Text);
         }
         return true;
      }	
      #endregion
   }

   /// <summary>
   /// Special combobox to hide some default behaviour so that datagrid cell browsing is not affected
   /// </summary>
   public class NoKeyUpCombo : ComboBox
   {
      const int WM_KEYUP = 0x101;

      /// <summary>
      /// Ignore keyup messages
      /// </summary>
      /// <param name="m"></param>
      protected override void WndProc(ref System.Windows.Forms.Message m)
      {
         if(m.Msg == WM_KEYUP)
         {
            //ignore keyup to avoid problem with tabbing & dropdownlist;
            return;
         }
         base.WndProc(ref m);
      }
   }

   /// <summary>
   /// Represents the method that handles combobox value changed events
   /// </summary>
   public delegate void DataGridComboBoxValueChangedHandler(object o, int iRow, object oNewValue);
}
