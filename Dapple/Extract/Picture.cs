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
   /// Set picture download options
   /// </summary>
   public partial class Picture : DownloadOptions
   {
      #region Constants
      private readonly string TIF_EXT = ".tif";      
      #endregion

      /// <summary>
      /// Control where the resolution can be changed
      /// </summary>
      public override bool ResolutionEnabled
      {
         set { oResolution.Enabled = value; }
      }

		public override bool OpenInMap
		{
			get { return (Options.Picture.DisplayOptions)cbDisplayOptions.SelectedIndex != Options.Picture.DisplayOptions.DoNotDisplay; }
		}

      /// <summary>
      /// Default constructor
      /// </summary>
      /// <param name="oDAPbuilder"></param>
      public Picture(Dapple.LayerGeneration.DAPQuadLayerBuilder oDAPbuilder)
         : base(oDAPbuilder)
      {
         InitializeComponent();

         cbDisplayOptions.DataSource = Options.Picture.DisplayOptionStrings;
         cbDisplayOptions.SelectedIndex = 0;

         cbDownloadOptions.DataSource = Options.Picture.DownloadOptionStrings;
         cbDownloadOptions.SelectedIndex = 2;

         tbFilename.Text = System.IO.Path.ChangeExtension(oDAPbuilder.Title, TIF_EXT);

         oResolution.SetDownloadOptions(this);
         SetDefaultResolution();
      }

      /// <summary>
      /// Set the default resolution
      /// </summary>
      public override void SetDefaultResolution()
      {
         double dXOrigin, dYOrigin, dXCellSize, dYCellSize;
         int iSizeX, iSizeY;

         string strCoordinateSystem = MainForm.MontajInterface.GetProjection(m_oDAPLayer.ServerURL, m_oDAPLayer.DatasetName);
         MainForm.MontajInterface.GetGridInfo(m_oDAPLayer.ServerURL, m_oDAPLayer.DatasetName, out dXOrigin, out dYOrigin, out iSizeX, out iSizeY, out dXCellSize, out dYCellSize);

         oResolution.Setup(true, strCoordinateSystem, dXOrigin, dYOrigin, iSizeX, iSizeY, dXCellSize, dYCellSize);
      }

      public override void SetNativeResolution()
      {
         oResolution.SetNativeResolution();
      }

      /// <summary>
      /// Write out settings for the Picture dataset
      /// </summary>
      /// <param name="oDatasetElement"></param>
      /// <param name="strDestFolder"></param>
      /// <param name="bDefaultResolution"></param>
      /// <returns></returns>
      public override bool Save(System.Xml.XmlElement oDatasetElement, string strDestFolder, DownloadSettings.DownloadClip eClip, DownloadSettings.DownloadCoordinateSystem eCS)
      {
         base.Save(oDatasetElement, strDestFolder, eClip, eCS);

			SetExtension();
         System.Xml.XmlAttribute oPathAttr = oDatasetElement.OwnerDocument.CreateAttribute("file");
         oPathAttr.Value = System.IO.Path.Combine(strDestFolder, tbFilename.Text);
         oDatasetElement.Attributes.Append(oPathAttr);

         System.Xml.XmlAttribute oResolutionAttr = oDatasetElement.OwnerDocument.CreateAttribute("resolution");
         oResolutionAttr.Value = oResolution.ResolutionValueSpecific(eCS).ToString();
         oDatasetElement.Attributes.Append(oResolutionAttr);

         System.Xml.XmlElement oDownloadElement = oDatasetElement.OwnerDocument.CreateElement("download_options");
         Options.Picture.DownloadOptions eOption = (Options.Picture.DownloadOptions)cbDownloadOptions.SelectedIndex;
         oDownloadElement.InnerText = eOption.ToString();
         oDatasetElement.AppendChild(oDownloadElement);

         System.Xml.XmlElement oDisplayElement = oDatasetElement.OwnerDocument.CreateElement("display_options");
         Options.Picture.DisplayOptions eDisplayOption = (Options.Picture.DisplayOptions)cbDisplayOptions.SelectedIndex;
         oDisplayElement.InnerText = eDisplayOption.ToString();
         oDatasetElement.AppendChild(oDisplayElement);
         return true;
      }

      /// <summary>
      /// Set the extension correctly
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void cbDownloadOptions_SelectedIndexChanged(object sender, EventArgs e)
      {
			SetExtension();
      }

		private void SetExtension()
		{
			string strOption = cbDownloadOptions.SelectedItem.ToString().ToLower();
			if (strOption == Options.Picture.DownloadOptionStrings[3].ToLower())
				tbFilename.Text = System.IO.Path.GetFileNameWithoutExtension(tbFilename.Text);
			else if (strOption == Options.Picture.DownloadOptionStrings[4].ToLower())
				tbFilename.Text = System.IO.Path.GetFileNameWithoutExtension(tbFilename.Text);
			else
				tbFilename.Text = System.IO.Path.ChangeExtension(tbFilename.Text, "." + strOption.ToLower());
		}

		public override DownloadOptions.DuplicateFileCheckResult CheckForDuplicateFiles(String szExtractDirectory, Form hExtractForm)
		{
			SetExtension();
			String szFilename = System.IO.Path.Combine(szExtractDirectory, tbFilename.Text);
			if (System.IO.File.Exists(szFilename))
			{
				return QueryOverwriteFile("The file \"" + szFilename + "\" already exists.  Overwrite?", hExtractForm);
			}
			else
			{
				return DuplicateFileCheckResult.Yes;
			}
		}
   }
}
