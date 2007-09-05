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
   /// Set the options for downloading the selected document
   /// </summary>
   public partial class Document : DownloadOptions
   {      
      /// <summary>
      /// Default constructor
      /// </summary>
      /// <param name="oDAPbuilder"></param>
      public Document(Dapple.LayerGeneration.DAPQuadLayerBuilder oDAPbuilder)
         : base(oDAPbuilder)
      {
         InitializeComponent();
         
         tbFilename.Text = oDAPbuilder.Name;

         cbDownload.DataSource = Options.Document.DownloadOptionStrings;
         cbDownload.SelectedIndex = 0;
      }

      /// <summary>
      /// Write out settings for the document
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
         Options.Document.DownloadOptions eOption = (Options.Document.DownloadOptions)cbDownload.SelectedIndex;
         oDownloadElement.InnerText = eOption.ToString();
         oDatasetElement.AppendChild(oDownloadElement);

         return true;
      }
   }
}
