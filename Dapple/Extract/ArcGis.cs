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
   public partial class ArcGIS : DownloadOptions
   {      
      /// <summary>
      /// Default constructor
      /// </summary>
      /// <param name="oDAPbuilder"></param>
      public ArcGIS(Dapple.LayerGeneration.DAPQuadLayerBuilder oDAPbuilder)
         : base(oDAPbuilder)
      {
         InitializeComponent();
         
         tbFilename.Text = oDAPbuilder.Title;

         cbDownload.DataSource = Options.ArcGIS.DownloadOptionStrings;
         cbDownload.SelectedIndex = 0;
      }

		public override bool OpenInMap
		{
			get { return (Options.ArcGIS.DownloadOptions)cbDownload.SelectedIndex != Options.ArcGIS.DownloadOptions.DownloadOnly; }
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
         Options.ArcGIS.DownloadOptions eOption = (Options.ArcGIS.DownloadOptions)cbDownload.SelectedIndex;
         oDownloadElement.InnerText = eOption.ToString();
         oDatasetElement.AppendChild(oDownloadElement);

         return true;
      }

		public override DownloadOptions.DuplicateFileCheckResult CheckForDuplicateFiles(String szExtractDirectory, Form hExtractForm)
		{
			String szFolderName = System.IO.Path.Combine(szExtractDirectory, tbFilename.Text);
			if (System.IO.Directory.Exists(szFolderName))
			{
				return QueryOverwriteFile("The folder \"" + szFolderName + "\" already exists.  Contents of the folder may be overwritten.  Continue with extraction?", hExtractForm);
			}
			else
			{
				return DuplicateFileCheckResult.Yes;
			}
		}
   }
}
