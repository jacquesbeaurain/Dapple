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
		private String m_szExtension;

      /// <summary>
      /// Default constructor
      /// </summary>
      /// <param name="oDAPbuilder"></param>
      public Document(Dapple.LayerGeneration.DAPQuadLayerBuilder oDAPbuilder)
         : base(oDAPbuilder)
      {
         InitializeComponent();

			MainForm.MontajInterface.GetDocumentExtension(m_oDAPLayer.ServerURL, m_oDAPLayer.DatasetName, out m_szExtension);

         tbFilename.Text = System.IO.Path.ChangeExtension(oDAPbuilder.Title, m_szExtension);

         cbDownload.DataSource = Options.Document.DownloadOptionStrings;
         cbDownload.SelectedIndex = 0;
      }

		public override bool OpenInMap
		{
			get { return (Options.Document.DownloadOptions)cbDownload.SelectedIndex != Options.Document.DownloadOptions.DownloadOnly; }
		}

      /// <summary>
      /// Write out settings for the document
      /// </summary>
      /// <param name="oDatasetElement"></param>
      /// <param name="strDestFolder"></param>
      /// <param name="bDefaultResolution"></param>
      /// <returns></returns>
		public override ExtractSaveResult Save(System.Xml.XmlElement oDatasetElement, string strDestFolder, DownloadSettings.DownloadCoordinateSystem eCS)
      {
         ExtractSaveResult result = base.Save(oDatasetElement, strDestFolder, eCS);

         System.Xml.XmlAttribute oPathAttr = oDatasetElement.OwnerDocument.CreateAttribute("file");
         oPathAttr.Value = System.IO.Path.Combine(strDestFolder, System.IO.Path.ChangeExtension(tbFilename.Text, m_szExtension));
         oDatasetElement.Attributes.Append(oPathAttr);

         System.Xml.XmlElement oDownloadElement = oDatasetElement.OwnerDocument.CreateElement("download_options");
         Options.Document.DownloadOptions eOption = (Options.Document.DownloadOptions)cbDownload.SelectedIndex;
         oDownloadElement.InnerText = eOption.ToString();
         oDatasetElement.AppendChild(oDownloadElement);

			return result;
      }

		public override DownloadOptions.DuplicateFileCheckResult CheckForDuplicateFiles(String szExtractDirectory, Form hExtractForm)
		{
			String szFilename = System.IO.Path.Combine(szExtractDirectory, System.IO.Path.ChangeExtension(Utility.FileSystem.SanitizeFilename(tbFilename.Text), m_szExtension));
			if (System.IO.File.Exists(szFilename))
			{
				return QueryOverwriteFile("The file \"" + szFilename + "\" already exists.  Overwrite?", hExtractForm);
			}
			else
			{
				return DuplicateFileCheckResult.Yes;
			}
		}

		private void tbFilename_Validating(object sender, CancelEventArgs e)
		{
			if (String.IsNullOrEmpty(tbFilename.Text))
			{
				m_oErrorProvider.SetError(tbFilename, "Field cannot be empty.");
				e.Cancel = true;
			}
			else
			{
				m_oErrorProvider.SetError(tbFilename, String.Empty);
			}
		}
   }
}
