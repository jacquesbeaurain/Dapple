using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using Flobbster.Windows.Forms;
using System.Collections;

namespace Dapple
{
   public partial class frmProperties : Form
   {
      object m_Object = null;
      Hashtable m_propetyTable = new Hashtable();
      Hashtable m_isChangableTable = new Hashtable();
      PropertyBag bag;

      public frmProperties()
      {
         InitializeComponent();
      }

      public object SetObject
      {
         get
         {
            return m_Object;
         }
         set
         {
            m_Object = value;
            bag = new PropertyBag();
            m_propetyTable = new Hashtable();
            m_isChangableTable = new Hashtable();

            bag.GetValue += new PropertySpecEventHandler(bag_GetValue);
            bag.SetValue += new PropertySpecEventHandler(bag_SetValue);

            PropertyInfo[] props = m_Object.GetType().GetProperties();
            
            foreach (PropertyInfo p in props)
            {
               if (!p.CanRead)
                  continue;
               
               object[] attributes = p.GetCustomAttributes(true);
               bool exit = false;
               string description = string.Empty;
               string category = null;
               bool readOnly = false;
               foreach (object attr in attributes)
               {
                  if (attr is BrowsableAttribute)
                  {
                     exit = !(attr as BrowsableAttribute).Browsable;
                     if (exit) break;
                  }
                  else if (attr is DescriptionAttribute)
                  {
                     description = (attr as DescriptionAttribute).Description;
                  }
                  else if (attr is CategoryAttribute)
                  {
                     category = (attr as CategoryAttribute).Category;
                  }
                  else if (attr is ReadOnlyAttribute)
                  {
                     readOnly = (attr as ReadOnlyAttribute).IsReadOnly;
                  }
               }

               if (exit)
                  continue;

               object val = p.GetValue(m_Object, BindingFlags.Default, null, null, System.Threading.Thread.CurrentThread.CurrentCulture);
               PropertySpec pSpec = new PropertySpec(p.Name, p.PropertyType, category, description, val);
               pSpec.Attributes = new Attribute[1];
               pSpec.Attributes[0] = new ReadOnlyAttribute(!p.CanWrite || readOnly); 

               bag.Properties.Add(pSpec);
               
               m_propetyTable.Add(p.Name, val);
               if (p.CanWrite)
                  m_isChangableTable.Add(p.Name, false);
               
            }
            c_pgProperties.SelectedObject = bag;
         }
      }

      void bag_SetValue(object sender, PropertySpecEventArgs e)
      {
         if ( (e.Property.Attributes[0]  as ReadOnlyAttribute).IsReadOnly)
            return;

         m_isChangableTable.Remove(e.Property.Name);
         m_isChangableTable.Add(e.Property.Name, true);
         m_propetyTable.Remove(e.Property.Name);
         m_propetyTable.Add(e.Property.Name, e.Value);
      }

      void bag_GetValue(object sender, PropertySpecEventArgs e)
      {
         e.Value = m_propetyTable[e.Property.Name];
      }

      private void btnCancel_Click(object sender, EventArgs e)
      {
         Close();
      }

      private void btnOk_Click(object sender, EventArgs e)
      {
         PropertyInfo[] props = m_Object.GetType().GetProperties();

         foreach (PropertyInfo p in props)
         {
            if (!m_propetyTable.ContainsKey(p.Name))
               continue;

            object val = m_propetyTable[p.Name];
            if (val != null && p.GetSetMethod() != null)
            {
               p.SetValue(m_Object, val, BindingFlags.Default, null, null, System.Threading.Thread.CurrentThread.CurrentCulture);
            }
         }
         Close();
      }

      /// <summary>
      /// Display a frmProperties for an Object.
      /// </summary>
      /// <param name="oTarget"></param>
      public static void DisplayForm(Object oTarget)
      {
         frmProperties oBob = new frmProperties();
         oBob.SetObject = oTarget;
         oBob.ShowDialog();
      }
   }
}