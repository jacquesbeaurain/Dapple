using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using WorldWind.VisualControl;

namespace WorldWind.Renderable
{
	/// <summary>
	/// The base class for objects to be rendered as part of the scene.
	/// </summary>
	public abstract class RenderableObject : IRenderable, IComparable
	{
		/// <summary>
		/// True when object is ready to be rendered.
		/// </summary>
		public bool isInitialized;

		/// <summary>
		/// True for objects the user can interact with.
		/// </summary>
		public bool isSelectable;

		public RenderableObjectList ParentList;

		protected string name;
		protected Hashtable _metaData = new Hashtable();
		protected Vector3d position;
		protected Quaternion4d orientation;
		protected bool isOn = true;
		protected byte _opacity = 255;
		protected Form m_propertyBrowser;

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.Renderable.RenderableObject"/> class.
		/// </summary>
		/// <param name="name">Object description</param>
		protected RenderableObject(string name)
		{
			this.name = name;
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="name">Object description</param>
		/// <param name="position">Object position (XYZ world coordinates)</param>
		/// <param name="orientation">Object rotation (Quaternion)</param>
		protected RenderableObject(string name, Vector3d position, Quaternion4d orientation)
		{
			this.name = name;
			this.position = position;
			this.orientation = orientation;
		}
		
		public abstract void Initialize(DrawArgs drawArgs);

		public abstract void Update(DrawArgs drawArgs);

      public class ExportInfo
      {
         public double dMinLat = double.MaxValue;
         public double dMaxLat = double.MinValue;
         public double dMinLon = double.MaxValue;
         public double dMaxLon = double.MinValue;

         public int iPixelsX;
         public int iPixelsY;

         public Graphics gr;

         public ExportInfo()
         {
         }
      }
      
      public virtual void InitExportInfo(DrawArgs drawArgs, ExportInfo info)
      {
      }
      
      public virtual void ExportProcess(DrawArgs drawArgs, ExportInfo expInfo)
      {
      }

		public abstract void Render(DrawArgs drawArgs);

      public virtual bool Initialized
		{	
			get
			{
				return isInitialized;
			}	
		}

		/// <summary>
		/// Called when object is disabled.
		/// </summary>
		public abstract void Dispose();

		/// <summary>
		/// User interaction (mouse click)
		/// </summary>
		public abstract bool PerformSelectionAction(DrawArgs drawArgs);

		public int CompareTo(object obj)
		{
			RenderableObject robj = obj as RenderableObject;
			if(obj == null)
				return 1;

         return String.Compare(robj.Name, this.name);
		}

		/// <summary>
		/// Permanently delete the layer
		/// </summary>
		public virtual void Delete()
		{
			string namestring = name;
			//TODO: Make the string absolute from the XML root so that we don't delete stuff we aren't supposed to...
			string xmlConfigFile = (string)MetaData["XML Configuration File"];
			if(xmlConfigFile == null)
				xmlConfigFile = (string)ParentList.MetaData["XML Configuration File"];																					   
				
			if(xmlConfigFile == null || !File.Exists(xmlConfigFile))
				throw new ApplicationException("Error deleting layer.");
				
			XmlDocument doc = new XmlDocument();
			doc.Load(xmlConfigFile);

			XmlNodeList list;
			XmlElement root = doc.DocumentElement;
			list = root.SelectNodes("//*[Name='" + namestring + "']");

			ParentList.Remove(Name);

			list[0].ParentNode.RemoveChild(list[0]);
			if(File.Exists(xmlConfigFile.Replace(".xml", ".bak")))
				File.Delete(xmlConfigFile.Replace(".xml", ".bak"));
			File.Move(xmlConfigFile, xmlConfigFile.Replace(".xml",".bak"));
			doc.Save(xmlConfigFile);
		}

		/// <summary>
		/// Fills the context menu with menu items specific to the layer.
		/// </summary>
		/// <param name="menu">Pre-initialized context menu.</param>
		public virtual void BuildContextMenu( ContextMenu menu )
		{
			menu.MenuItems.Add("Properties", new System.EventHandler(OnPropertiesClick));
			menu.MenuItems.Add("Info", new System.EventHandler(OnInfoClick));
			menu.MenuItems.Add("Delete", new System.EventHandler(OnDeleteClick));
		}

		/// <summary>
		/// Returns a String that represents the current SelectedObject.
		/// </summary>
		public override string ToString()
		{
			return name;
		}

		#region Properties

		/// <summary>
		/// How transparent this object should appear (0=invisible, 255=opaque)
		/// </summary>
		[Description("Controls the amount of light allowed to pass through this object. (0=invisible, 255=opaque).")]
		public virtual byte Opacity
		{
			get
			{
				return this._opacity;
			}
			set
			{
				this._opacity = value;
			}
		}

		[Browsable(false)]
		public virtual System.Collections.Hashtable MetaData
		{
			get
			{
				return this._metaData;
			}
		}

      /// <summary>
      /// Friendly request to release some resources if the object is turned off
      /// </summary>
      protected abstract void FreeResources();
		/// <summary>
		/// Hide/Show this object.
		/// </summary>
		[Description("This layer's enabled status.")]
		public virtual bool IsOn
		{
			get
			{
				return this.isOn;
			}
			set
			{
				this.isOn = value;
            if (!this.isOn)
               FreeResources();
			}
		}

		/// <summary>
		/// Describes this object
		/// </summary>
		[Description("This layer's name.")]
		public virtual string Name 
		{
			get
			{
				return this.name;
			}
			set
			{
				this.name = value;
			}
		}

		/// <summary>
		/// Object position (XYZ world coordinates)
		/// </summary>
		[Browsable(false)]
		public virtual Vector3d Position
		{
			get
			{
				return this.position;
			}
			set
			{
				this.position = value;
			}
		}

		/// <summary>
		/// Object rotation (Quaternion)
		/// </summary>
		[Browsable(false)]
		public virtual Quaternion4d Orientation
		{
			get
			{
				return this.orientation;
			}
			set
			{
				this.orientation = value;
			}
		}

		#endregion

		#region Menu items

		/// <summary>
		/// Layer info context menu item
		/// </summary>
		protected virtual void OnInfoClick(object sender, System.EventArgs e)
		{
			WorldWind.Menu.LayerManagerItemInfo lmii = new WorldWind.Menu.LayerManagerItemInfo(MetaData);
			lmii.ShowDialog();
		}

		/// <summary>
		/// Layer properties context menu item
		/// </summary>
		protected virtual void OnPropertiesClick(object sender, System.EventArgs e)
		{
			if(m_propertyBrowser!=null)
				m_propertyBrowser.Dispose();

			m_propertyBrowser = new PropertyBrowser(this);
			m_propertyBrowser.Show();
		}

		/// <summary>
		/// Delete layer context menu item
		/// </summary>
		protected virtual void OnDeleteClick(object sender, System.EventArgs e)
		{
			if(ParentList == null || ParentList.ParentList == null)
			{
				MessageBox.Show("Unable to delete root layer list.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
			}

			string message = "Permanently Delete Layer '" + name + "'?";
			if(DialogResult.Yes != MessageBox.Show( message, "Delete layer", MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
				MessageBoxDefaultButton.Button2 ))
				return;

			try
			{
				Delete();
			}
			catch(Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		#endregion

	}
}
