using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using GeosoftWorldWindApp.LayerGeneration;

namespace GeosoftWorldWindApp
{
   public partial class PropertiesForm : Form
   {
      public delegate void ApplyChangesClickedHandler(object sender, EventArgs e, object item);
      public event ApplyChangesClickedHandler ApplyChangesClicked;

      public PropertiesForm(object item)
      {
         InitializeComponent();
         propertyGrid1.SelectedObject = item;
      }

      public object Item
      {
         get
         {
            return propertyGrid1.SelectedObject;
         }
      }

      private void btnApplyChanges_Click(object sender, EventArgs e)
      {
         if (ApplyChangesClicked != null)
            ApplyChangesClicked(sender, e, Item);
      }
   }
}