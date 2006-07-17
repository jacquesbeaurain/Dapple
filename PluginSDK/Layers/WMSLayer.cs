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
      private decimal _north;
      private decimal _south;
      private decimal _east;
      private decimal _west;
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

      private static List<string> wgs84Equivalents = new List<string>(new string[] {
         "EPSG:4019",
         "EPSG:4176",
         "EPSG:4151",
         "EPSG:4133",
         "EPSG:4180",
         "EPSG:4258",
         "EPSG:4283",
         "EPSG:4121",
         "EPSG:4173",
         "EPSG:4659",
         "EPSG:4141",
         "EPSG:4612",
         "EPSG:4319",
         "EPSG:4661",
         "EPSG:4126",
         "EPSG:4669",
         "EPSG:4269",
         "EPSG:4140",
         "EPSG:4167",
         "EPSG:4172",
         "EPSG:4190",
         "EPSG:4189",
         "EPSG:4171",
         "EPSG:4624",
         "EPSG:4627",
         "EPSG:4170",
         "EPSG:4619",
         "EPSG:4148",
         "EPSG:4670",
         "EPSG:4667",
         "EPSG:4166",
         "EPSG:4130",
         "EPSG:4318",
         "EPSG:4640",
         "EPSG:4326",
         "EPSG:4163",
         "CRS:83"
      });
      #endregion

      #region Public Methods
      public WMSLayer()
      {
      }

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

      public string GetWMSDownloadFile(string date,
         WMSLayerStyle style,
         decimal north,
         decimal south,
         decimal west,
         decimal east,
            uint size)
      {
         // WMS Download paths are unique to the server and the URL used to download the image by using hashcode
         
         string url = GetWMSRequestUrl(date, (style != null ? style.name : null), north, south, west, east, size);
#if DEBUG
         System.Diagnostics.Debug.WriteLine("WMS Image request for filename : " + url);
#endif
         string strPath = Utility.StringHash.GetBase64HashForPath(url);

         foreach (string curFormat in this._imageFormats)
         {
            if (string.Compare(curFormat, "image/png", true, CultureInfo.InvariantCulture) == 0)
            {
               strPath += ".png";
               break;
            }
            if (string.Compare(curFormat, "image/jpeg", true, CultureInfo.InvariantCulture) == 0 ||
               String.Compare(curFormat, "image/jpg", true, CultureInfo.InvariantCulture) == 0)
            {
               strPath += ".jpg";
               break;
            }
         }
         return strPath;
      }

      public WMSDownload GetWmsRequest(string dateString,
         WMSLayerStyle curStyle,
         decimal north,
         decimal south,
         decimal west,
         decimal east,
            uint size,
         string cacheDirectory)
      {
         string url = GetWMSRequestUrl(dateString,
            (curStyle != null ? curStyle.name : null),
            north,
            south,
            west,
            east,
                size);
         WMSDownload wmsDownload = new WMSDownload(url);

         wmsDownload.North = north;
         wmsDownload.South = south;
         wmsDownload.West = west;
         wmsDownload.East = east;

         wmsDownload.Title = this._title;
         if (curStyle != null)
            wmsDownload.Title += " (" + curStyle.title + ")";

         wmsDownload.SavedFilePath = Path.Combine(cacheDirectory, GetWMSDownloadFile(dateString, curStyle, north, south, west, east, size));
         if (dateString != null && dateString.Length > 0)
            wmsDownload.Title += "\n" + dateString;
         return wmsDownload;
      }

      public string GetWMSRequestUrl(string date, string style, decimal north, decimal south, decimal west, decimal east, uint size)
      {
         if (this._name == null)
         {
            //Utility.Log.Write("WMSB", "No Name");
            return null;
         }
         string projectionRequest = "";


         if (this.ParentWMSList.Version == "1.1.1")
         {
            if (wgs84Equivalents.Contains(_srs))
               projectionRequest = "srs=" + _srs;
            else
               projectionRequest = "srs=EPSG:4326";
         }
         else
         {
            if (wgs84Equivalents.Contains(_crs))
               projectionRequest = "crs=" + _crs;
            else
               projectionRequest = "crs=CRS:84";
         }
         string imageFormat = null;

         if (this._imageFormats == null)
         {
            //Utility.Log.Write("WMSB", "No formats");
            return null;
         }

         foreach (string curFormat in this._imageFormats)
         {
            if (string.Compare(curFormat, "image/png", true, CultureInfo.InvariantCulture) == 0)
            {
               imageFormat = curFormat;
               break;
            }
            if (string.Compare(curFormat, "image/jpeg", true, CultureInfo.InvariantCulture) == 0 ||
               String.Compare(curFormat, "image/jpg", true, CultureInfo.InvariantCulture) == 0)
            {
               imageFormat = curFormat;
               break;
            }
         }

         if (imageFormat == null)
            return null;

         uint rWidth = size;
         uint rHeight = size;
         if (east - west > 0 && north - south > 0)
         {
            if (east - west > north - south)
               rWidth = (uint)Math.Round((east - west) * (decimal)rWidth / (north - south));
            else
               rHeight = (uint)Math.Round((north - south) * (decimal)rHeight / (east - west));
         }
         string wmsQuery = string.Format(
            CultureInfo.InvariantCulture,
            "{0}" + (this.ParentWMSList.ServerGetMapUrl.IndexOf("?") == -1 ? "?" : "") + 
            "service=WMS&version={1}&request=GetMap&layers={2}&format={3}&width={4}&height={5}&time={6}&{7}&bbox={8},{9},{10},{11}&styles={12}&transparent=TRUE",
            this.ParentWMSList.ServerGetMapUrl,
            this.ParentWMSList.Version,
            this._name,
            imageFormat,
            (this._width != 0 ? this._width : rWidth),
            (this._height != 0 ? this._height : rHeight),
            (date != null ? date : ""),
            projectionRequest,
            west, south, east, north,
            (style != null ? style : ""));

         // Cleanup
         wmsQuery = wmsQuery.Replace("??", "?");
         wmsQuery = wmsQuery.Replace("time=&", "");

         return wmsQuery;
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

      public string DefaultDate
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

      public uint Width
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

      public uint Height
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

      public string[] Dates
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
      public string description;
      public string title;
      public string name;
      public WMSLayerStyleLegendURL[] legendURL;

      public override string ToString()
      {
         return this.title;
      }
   }

   public class WMSLayerStyleLegendURL
   {
      public string format;
      public string href;
      public int width;
      public int height;

      public override string ToString()
      {
         return this.href;
      }
   }

   public class WMSImageLayerInfo
   {
      #region Private Members
      private float _north;
      private float _south;
      private float _west;
      private float _east;
      private string _imageFilePath;
      private string _id;
      private string _description;
      #endregion

      #region Public Methods
      public WMSImageLayerInfo()
      {
      }

      public WMSImageLayerInfo(WMSDownload dl)
      {
         this.Id = dl.Name + "-" + Path.GetFileName(dl.SavedFilePath);
         this.Description = dl.Title;
         this.ImageFilePath = dl.SavedFilePath;
         this.North = (float)dl.North;
         this.South = (float)dl.South;
         this.West = (float)dl.West;
         this.East = (float)dl.East;
      }

      public static WMSImageLayerInfo FromFile(string filePath)
      {
         using (FileStream stream = File.OpenRead(filePath))
         using (BinaryReader reader = new BinaryReader(stream, System.Text.Encoding.Unicode))
         {
            WMSImageLayerInfo imageLayerInfo = new WMSImageLayerInfo();
            imageLayerInfo.Id = reader.ReadString();
            string imageFileName = Path.GetFileName(reader.ReadString());
            imageLayerInfo.ImageFilePath = Path.Combine(Path.GetDirectoryName(filePath), imageFileName);
            if (!File.Exists(imageLayerInfo.ImageFilePath))
               throw new IOException("Cached image '" + imageLayerInfo.ImageFilePath + "' not found.");
            imageLayerInfo.Description = reader.ReadString();
            imageLayerInfo.South = reader.ReadSingle();
            imageLayerInfo.West = reader.ReadSingle();
            imageLayerInfo.North = reader.ReadSingle();
            imageLayerInfo.East = reader.ReadSingle();
            return imageLayerInfo;
         }
      }

      public void Save(string filepath)
      {
         using (FileStream stream = File.Open(filepath, FileMode.Create))
         using (BinaryWriter writer = new BinaryWriter(stream, System.Text.Encoding.Unicode))
         {
            writer.Write(this.Id);
            writer.Write(Path.GetFileName(this.ImageFilePath));
            writer.Write(this.Description);
            writer.Write(this.South);
            writer.Write(this.West);
            writer.Write(this.North);
            writer.Write(this.East);
         }
      }
      #endregion

      #region Properties
      public float North
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

      public float South
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

      public float East
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

      public float West
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

      public string Id
      {
         get
         {
            return this._id;
         }
         set
         {
            this._id = value;
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

      public string ImageFilePath
      {
         get
         {
            return this._imageFilePath;
         }
         set
         {
            this._imageFilePath = value;
         }
      }
      #endregion
   }
}
