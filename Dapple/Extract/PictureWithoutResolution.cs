using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

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

      /// <summary>
      /// Default constructor
      /// </summary>
      /// <param name="oDAPbuilder"></param>
      public PictureWithoutResolution(Dapple.LayerGeneration.LayerBuilder oBuilder)
         : base(oBuilder as Dapple.LayerGeneration.DAPQuadLayerBuilder)
      {
         InitializeComponent();

         cbDisplayOptions.DataSource = Options.Picture.DisplayOptionStrings;
         cbDisplayOptions.SelectedIndex = 0;

         cbDownloadOptions.DataSource = Options.Picture.DownloadOptionStrings;
         cbDownloadOptions.SelectedIndex = 2;

         cbSize.DataSource = Options.Picture.SizeOptionStrings;
         cbSize.SelectedIndex = 3;

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
         base.Save(oDatasetElement, strDestFolder, eClip, eCS);

         System.Xml.XmlAttribute oPathAttr = oDatasetElement.OwnerDocument.CreateAttribute("file");
         oPathAttr.Value = System.IO.Path.Combine(strDestFolder, tbFilename.Text);

         oDatasetElement.Attributes.Append(oPathAttr);

         System.Xml.XmlElement oDownloadElement = oDatasetElement.OwnerDocument.CreateElement("download_options");
         Options.Picture.DownloadOptions eOption = (Options.Picture.DownloadOptions)cbDownloadOptions.SelectedIndex;
         oDownloadElement.Value = eOption.ToString();
         oDatasetElement.AppendChild(oDownloadElement);

         System.Xml.XmlElement oDisplayElement = oDatasetElement.OwnerDocument.CreateElement("display_options");
         Options.Picture.DisplayOptions eDisplayOption = (Options.Picture.DisplayOptions)cbDisplayOptions.SelectedIndex;
         oDisplayElement.Value = eDisplayOption.ToString();
         oDatasetElement.AppendChild(oDisplayElement);

         System.Xml.XmlElement oSizeElement = oDatasetElement.OwnerDocument.CreateElement("size");
         Options.Picture.SizeOptions eSizeOption = (Options.Picture.SizeOptions)cbSize.SelectedIndex;
         oSizeElement.Value = eSizeOption.ToString();
         oDatasetElement.AppendChild(oSizeElement);

         return true;
      }

      /// <summary>
      /// Set the extension correctly
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void cbDownloadOptions_SelectedIndexChanged(object sender, EventArgs e)
      {
         string strOption = cbDownloadOptions.SelectedItem.ToString().ToLower();
         if (strOption == Options.Picture.DisplayOptionStrings[3].ToLower())
            tbFilename.Text = System.IO.Path.GetFileNameWithoutExtension(tbFilename.Text);
         else if (strOption == Options.Picture.DisplayOptionStrings[4].ToLower())
            tbFilename.Text = System.IO.Path.GetFileNameWithoutExtension(tbFilename.Text);
         else
            tbFilename.Text = System.IO.Path.ChangeExtension(tbFilename.Text, "." + strOption.ToLower());
      }      
   }
}
