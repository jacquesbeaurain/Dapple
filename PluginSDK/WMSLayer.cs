using System.Collections.Specialized;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System;
using WorldWind;
using WorldWind.Renderable;
using WorldWind.VisualControl;
using WorldWind.Net;
using WorldWind.Net.Wms;

namespace WorldWind
{
	public class WMSLayer
   {
      #region Private Members
      WMSList _parentWMSList;
      private string[] _imageFormats;
      private decimal _north = 90;
      private decimal _south = -90;
      private decimal _east = 180;
      private decimal _west = -180;
      private string _title;
      private string _description;
      private string _name;
      private uint _width;
      private uint _height;
      private string _crs;
      private string _srs;
      private string _defaultDate;
      private string[] _dates;
      private WMSLayer[] _childLayers;
      private WMSLayerStyle[] _styles;

      #endregion

      #region Constructor
      internal WMSLayer()
      {
      }
      #endregion

      #region internal Methods
		public bool HasLegend
      {
         get
         {
            if (this._styles == null)
               return false;
            foreach (WMSLayerStyle style in this._styles)
               if (style.legendURL != null && style.legendURL.Length > 0)
                  return true;
            return false;
         }
      }
      #endregion

      #region Properties

		public WMSList ParentWMSList
      {
         get
         {
            return this._parentWMSList;
         }
         set
         {
            this._parentWMSList = value;
         }
      }

		public string[] ImageFormats
      {
         get
         {
            return this._imageFormats;
         }
         set
         {
            this._imageFormats = value;
         }
      }

		public decimal North
      {
         get
         {
            return this._north;
         }
         set
         {
            this._north = value;
         }
      }

		public decimal South
      {
         get
         {
            return this._south;
         }
         set
         {
            this._south = value;
         }
      }

		public decimal West
      {
         get
         {
            return this._west;
         }
         set
         {
            this._west = value;
         }
      }

		public decimal East
      {
         get
         {
            return this._east;
         }
         set
         {
            this._east = value;
         }
      }

		public string CRS
      {
         get
         {
            return this._crs;
         }
         set
         {
            this._crs = value;
         }
      }

		public string SRS
      {
         get
         {
            return this._srs;
         }
         set
         {
            this._srs = value;
         }
      }

		public string Name
      {
         get
         {
            return this._name;
         }
         set
         {
            this._name = value;
         }
      }

		public string Title
      {
         get
         {
            return this._title;
         }
         set
         {
            this._title = value;
         }
      }

		public string Description
      {
         get
         {
            return this._description;
         }
         set
         {
            this._description = value;
         }
      }

      internal string DefaultDate
      {
         get
         {
            return this._defaultDate;
         }
         set
         {
            this._defaultDate = value;
         }
      }

      internal uint Width
      {
         get
         {
            return this._width;
         }
         set
         {
            this._width = value;
         }
      }

      internal uint Height
      {
         get
         {
            return this._height;
         }
         set
         {
            this._height = value;
         }
      }

      internal string[] Dates
      {
         get
         {
            return this._dates;
         }
         set
         {
            this._dates = value;
         }
      }

		public WMSLayer[] ChildLayers
      {
         get
         {
            return this._childLayers;
         }
         set
         {
            this._childLayers = value;
         }
      }

		public WMSLayerStyle[] Styles
      {
         get
         {
            return this._styles;
         }
         set
         {
            this._styles = value;
         }
      }

      #endregion
   }

	public class WMSLayerStyle
   {
      internal string title;
		public WMSLayerStyleLegendURL[] legendURL;

		public override string ToString()
      {
         return this.title;
      }
   }

	public class WMSLayerStyleLegendURL
   {
		public string href;

		public override string ToString()
      {
         return this.href;
      }
   }
}
