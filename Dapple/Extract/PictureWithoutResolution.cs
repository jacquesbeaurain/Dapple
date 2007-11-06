using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using WorldWind.Renderable;
using WorldWind;
using Dapple.LayerGeneration;

namespace Dapple.Extract
{
   /// <summary>
   /// Set the download options for a picture that does not have a known resolution
   /// </summary>
   public partial class PictureWithoutResolution : DownloadOptions
   {
      #region Constants
      private readonly string TIF_EXT = ".tif";            
      #endregion

      #region Member Variables
      private Dapple.LayerGeneration.LayerBuilder m_oNonDapBuilder;
      #endregion

      /// <summary>
      /// Default constructor
      /// </summary>
      /// <param name="oDAPbuilder"></param>
      public PictureWithoutResolution(Dapple.LayerGeneration.LayerBuilder oBuilder)
         : base(oBuilder as Dapple.LayerGeneration.DAPQuadLayerBuilder)
      {
         InitializeComponent();

         m_oNonDapBuilder = oBuilder;

         cbDisplayOptions.DataSource = Options.Picture.DisplayOptionStrings;
         cbDisplayOptions.SelectedIndex = 0;

         tbFilename.Text = System.IO.Path.ChangeExtension(oBuilder.Name, TIF_EXT);
      }

      /// <summary>
      /// Default constructor
      /// </summary>
      /// <param name="oDAPbuilder"></param>
      public PictureWithoutResolution(Dapple.LayerGeneration.DAPQuadLayerBuilder oDAPbuilder)
         : this(oDAPbuilder as Dapple.LayerGeneration.LayerBuilder)
      {         
      }

      /// <summary>
      /// Write out settings for the picture dataset
      /// </summary>
      /// <param name="oDatasetElement"></param>
      /// <param name="strDestFolder"></param>
      /// <param name="bDefaultResolution"></param>
      /// <returns></returns>
      public override bool Save(System.Xml.XmlElement oDatasetElement, string strDestFolder, DownloadSettings.DownloadClip eClip, DownloadSettings.DownloadCoordinateSystem eCS)
      {
         System.Xml.XmlAttribute oAttr = oDatasetElement.OwnerDocument.CreateAttribute("type");
         oAttr.Value = "geotiff";
         oDatasetElement.Attributes.Append(oAttr);

         oAttr = oDatasetElement.OwnerDocument.CreateAttribute("title");
         oAttr.Value = m_oNonDapBuilder.Name;
         oDatasetElement.Attributes.Append(oAttr);

         oAttr = oDatasetElement.OwnerDocument.CreateAttribute("file");
         String szFileName = tbFilename.Text;
         foreach (Char ch in System.IO.Path.GetInvalidFileNameChars())
            szFileName = szFileName.Replace(ch, '_');
         oAttr.Value = System.IO.Path.Combine(strDestFolder, System.IO.Path.ChangeExtension(szFileName, TIF_EXT));
         oDatasetElement.Attributes.Append(oAttr);

         if (m_oNonDapBuilder is GeorefImageLayerBuilder)
         {
            System.IO.File.Copy(((GeorefImageLayerBuilder)m_oNonDapBuilder).FileName, oAttr.Value);
         }
         else
         {
            m_oNonDapBuilder.exportToGeoTiff(oAttr.Value);
         }

         System.Xml.XmlElement oDisplayElement = oDatasetElement.OwnerDocument.CreateElement("display_options");
         Options.Picture.DisplayOptions eDisplayOption = (Options.Picture.DisplayOptions)cbDisplayOptions.SelectedIndex;
         oDisplayElement.InnerText = eDisplayOption.ToString();
         oDatasetElement.AppendChild(oDisplayElement);

         WorldWind.GeographicBoundingBox oViewBox = WorldWind.GeographicBoundingBox.FromQuad(MainForm.WorldWindowSingleton.GetSearchBox());
         String szViewCRS = Dapple.Extract.Resolution.WGS_84;

         WorldWind.GeographicBoundingBox oMapBox = MainForm.MapAoi;
         String szMapCRS = MainForm.MapAoiCoordinateSystem;

         bool blNewMap;

         if (oMapBox == null)
         {
            blNewMap = true;
         }
         else
         {
            if (MainForm.MontajInterface.ProjectBoundingRectangle(szMapCRS, ref oMapBox.West, ref oMapBox.South, ref oMapBox.East, ref oMapBox.North, szViewCRS))
            {
               blNewMap = (!oViewBox.Intersects(oMapBox));
            }
            else
            {
               blNewMap = true;
            }
         }

         oAttr = oDatasetElement.OwnerDocument.CreateAttribute("new_map");
         oAttr.Value = blNewMap.ToString();
         oDatasetElement.Attributes.Append(oAttr);


         GeographicBoundingBox oGeoTiffBox = oViewBox.Clone() as GeographicBoundingBox;

         if (m_oNonDapBuilder is GeorefImageLayerBuilder)
         {
            oGeoTiffBox = GeorefImageLayerBuilder.GetExtentsFromGeotif(((GeorefImageLayerBuilder)m_oNonDapBuilder).FileName);
         }

         oAttr = oDatasetElement.OwnerDocument.CreateAttribute("minx");
         oAttr.Value = oGeoTiffBox.West.ToString();
         oDatasetElement.Attributes.Append(oAttr);

         oAttr = oDatasetElement.OwnerDocument.CreateAttribute("miny");
         oAttr.Value = oGeoTiffBox.South.ToString();
         oDatasetElement.Attributes.Append(oAttr);

         oAttr = oDatasetElement.OwnerDocument.CreateAttribute("maxx");
         oAttr.Value = oGeoTiffBox.East.ToString();
         oDatasetElement.Attributes.Append(oAttr);

         oAttr = oDatasetElement.OwnerDocument.CreateAttribute("maxy");
         oAttr.Value = oGeoTiffBox.North.ToString();
         oDatasetElement.Attributes.Append(oAttr);

         oAttr = oDatasetElement.OwnerDocument.CreateAttribute("coordinate_system");
         oAttr.Value = szViewCRS;
         oDatasetElement.Attributes.Append(oAttr);

         String szDownloadType = String.Empty;
         String szDownloadUrl = String.Empty;
         String szLayerId = String.Empty;
         m_oNonDapBuilder.GetOMMetadata(out szDownloadType, out szDownloadUrl, out szLayerId);

         oAttr = oDatasetElement.OwnerDocument.CreateAttribute("download_type");
         oAttr.Value = szDownloadType;
         oDatasetElement.Attributes.Append(oAttr);

         oAttr = oDatasetElement.OwnerDocument.CreateAttribute("url");
         oAttr.Value = szDownloadUrl;
         oDatasetElement.Attributes.Append(oAttr);

         oAttr = oDatasetElement.OwnerDocument.CreateAttribute("id");
         oAttr.Value = szLayerId;
         oDatasetElement.Attributes.Append(oAttr);

         return true;
      }
   }
}
