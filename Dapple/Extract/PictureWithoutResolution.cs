using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using WorldWind.Renderable;
using WorldWind;

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
         //base.Save(oDatasetElement, strDestFolder, eClip, eCS);

         System.Xml.XmlAttribute oTypeAttr = oDatasetElement.OwnerDocument.CreateAttribute("type");
         oTypeAttr.Value = "geotiff";
         oDatasetElement.Attributes.Append(oTypeAttr);

         System.Xml.XmlAttribute oTitleAttr = oDatasetElement.OwnerDocument.CreateAttribute("title");
         oTitleAttr.Value = m_oNonDapBuilder.Name;
         oDatasetElement.Attributes.Append(oTitleAttr);

         System.Xml.XmlAttribute oPathAttr = oDatasetElement.OwnerDocument.CreateAttribute("file");
         String szFileName = tbFilename.Text;
         foreach (Char ch in System.IO.Path.GetInvalidFileNameChars())
            szFileName = szFileName.Replace(ch, '_');
         oPathAttr.Value = System.IO.Path.Combine(strDestFolder, szFileName);
         oDatasetElement.Attributes.Append(oPathAttr);

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

         System.Xml.XmlAttribute oNewMapAttr = oDatasetElement.OwnerDocument.CreateAttribute("new_map");
         oNewMapAttr.Value = blNewMap.ToString();
         oDatasetElement.Attributes.Append(oNewMapAttr);

         m_oNonDapBuilder.exportToGeoTiff(oPathAttr.Value);

         return true;
      }
   }
}
