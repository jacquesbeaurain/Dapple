using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Geosoft.OpenGX.UtilityForms;

namespace Geosoft.OpenGX.UtilityForms
{
   /// <summary>
   /// Use to indicate required status of a "buddy" control on the parent Form. The indicator is in the form of a star icon to the left of the label.
   /// </summary>
   [ToolboxItem(true)]
   [ToolboxBitmap(typeof(LabelControl), "LabelControl.bmp")]
   public partial class LabelControl : Label
   {
      #region Members
      private PictureBox m_pictureBox;
      IRequirable m_buddy;
      #endregion

      #region Constructor/Disposal
      /// <summary>
      /// 	<para>Initializes an instance of the <see cref="LabelControl"/> class.</para>
      /// </summary>
      public LabelControl()
      {
         base.TextAlign = ContentAlignment.TopLeft;
         InitializeComponent();
      }

      /// <summary>Gets or sets the alignment of text in the label.</summary>
      /// <returns>One of the <see cref="T:System.Drawing.ContentAlignment"></see> values. The default is <see cref="F:System.Drawing.ContentAlignment.TopLeft"></see>.</returns>
      /// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException">The value assigned is not one of the <see cref="T:System.Drawing.ContentAlignment"></see> values. </exception>
      /// <filterpriority>1</filterpriority>
      /// <PermissionSet>
      /// 	<IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/>
      /// 	<IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/>
      /// 	<IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence"/>
      /// 	<IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/>
      /// </PermissionSet>
      [DefaultValue(ContentAlignment.TopLeft)]
      public new ContentAlignment TextAlign
      {
         get
         {
            return base.TextAlign;
         }
         set
         {
         }
      }

      /// <summary> 
      /// Clean up any resources being used.
      /// </summary>
      /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
      protected override void Dispose(bool disposing)
      {
         if (disposing && (components != null))
         {
            components.Dispose();
         }
         base.Dispose(disposing);
      }
      #endregion

      #region Control Overrides
      /// <summary>
      /// Handles changes to the <see cref="Control.Enabled"/> property.
      /// </summary>
      /// <param name="e"></param>
      protected override void OnEnabledChanged(System.EventArgs e)
      {
         base.OnEnabledChanged(e);

         if (m_pictureBox != null)
         {
            if (this.Enabled)
               m_pictureBox.Image = global::Utility.Properties.Resources.required;
            else
               m_pictureBox.Image = global::Utility.Properties.Resources.required_disabled;
            m_pictureBox.Refresh();
         }
      }

      /// <summary>
      /// Handles changes to the <see cref="Control.Visible"/> property.
      /// </summary>
      /// <param name="e"></param>
      protected override void OnVisibleChanged(System.EventArgs e)
      {
         base.OnVisibleChanged(e);

         if (m_pictureBox != null && m_buddy != null)
         {
            if ((m_buddy).Required)
            {
               m_pictureBox.Visible = this.Visible;
               m_pictureBox.Refresh();
            }
         }
      }

      /// <summary>Sets the specified bounds of the label.</summary>
      /// <param name="y">The new <see cref="P:System.Windows.Forms.Control.Top"></see> property value of the control. </param>
      /// <param name="specified">A bitwise combination of the <see cref="T:System.Windows.Forms.BoundsSpecified"></see> values. For any parameter not specified, the current value will be used. </param>
      /// <param name="width">The new <see cref="P:System.Windows.Forms.Control.Width"></see> property value of the control. </param>
      /// <param name="height">The new <see cref="P:System.Windows.Forms.Control.Height"></see> property value of the control. </param>
      /// <param name="x">The new <see cref="P:System.Windows.Forms.Control.Left"></see> property value of the control. </param>
      protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
      {
         base.SetBoundsCore(x, y, width, height, specified);
         LocatePictureBox();
      }


      /// <summary>Raises the <see cref="M:System.Windows.Forms.Control.CreateControl"></see> event.</summary>
      protected override void OnCreateControl()
      {
         base.OnCreateControl();
         CreatePictureBox();
      }

      void LocatePictureBox()
      {
         if (m_pictureBox != null)
         {
            Point pt = new Point(this.Location.X - 10, this.Location.Y + 2);
            if (m_pictureBox.Location != pt)
            {
               m_pictureBox.Location = pt;
               m_pictureBox.Refresh();
            }
         }
      }

      void CreatePictureBox()
      {
         if (this.Parent != null)
         {
            m_pictureBox = new PictureBox();
            if (this.Enabled)
               m_pictureBox.Image = global::Utility.Properties.Resources.required;
            else
               m_pictureBox.Image = global::Utility.Properties.Resources.required_disabled;
            m_pictureBox.Location = new Point(this.Location.X - 10, this.Location.Y + 2);
            m_pictureBox.Name = "m_pictureBox";
            m_pictureBox.Size = new Size(10, 10);
            m_pictureBox.TabStop = false;
            if (m_buddy != null)
            {
               if (m_buddy.Required)
                  m_pictureBox.Visible = true;
               else
                  m_pictureBox.Visible = false;
            }
            else
               m_pictureBox.Visible = false;
            this.Parent.Controls.Add(m_pictureBox);
         }
      }
      #endregion

      #region Properties Code

      /// <summary>
      /// Set the control that this label is for.
      /// </summary>
      [Browsable(true), Category("GX_Parameters")]
      [Description("Set the control that this label is for")]
      public IRequirable BuddyControl
      {
         get
         {
            return m_buddy;
         }
         set
         {
            if (m_buddy != null)
               m_buddy.RequiredChanged -= this.OnBuddyRequiredChanged;
            m_buddy = value;
            if (m_buddy != null)
               m_buddy.RequiredChanged += this.OnBuddyRequiredChanged;
            if (m_pictureBox != null)
            {
               if (m_buddy == null || (!m_buddy.Required))
                  m_pictureBox.Visible = false;
               else
                  m_pictureBox.Visible = true;
            }
         }
      }
      #endregion

      #region Event Handlers
      private void OnBuddyRequiredChanged(object sender, EventArgs e)
      {
         if (m_pictureBox!= null && m_buddy != null)
         {
            if (m_buddy.Required)
               m_pictureBox.Visible = true;
            else
               m_pictureBox.Visible = false;
         }
      }

      private void LabelControl_ParentChanged(object sender, EventArgs e)
      {
         if (m_pictureBox != null)
         {
            if (m_pictureBox.Container != null)
               m_pictureBox.Container.Remove(m_pictureBox);
            if (this.Parent != null)
            {
               this.Parent.Controls.Add(m_pictureBox);
               LocatePictureBox();
            }
         }
      }
      #endregion
   }
}
