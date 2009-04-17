using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Globalization;

namespace Dapple.Extract
{
   /// <summary>
   /// Set the download options for the grid
   /// </summary>
   internal partial class Grid : DownloadOptions
   {
      #region Constants
      private readonly string GRD_EXT = ".grd";      
      #endregion

      /// <summary>
      /// Control where the resolution can be changed
      /// </summary>
      internal override bool ResolutionEnabled
      {
         set { oResolution.Enabled = value; }
      }

		internal override bool OpenInMap
		{
			get { return (Options.Grid.DisplayOptions)cbDisplayOptions.SelectedIndex != Options.Grid.DisplayOptions.DoNotDisplay; }
		}

      /// <summary>
      /// Default constructor
      /// </summary>
      /// <param name="oDAPbuilder"></param>
      internal Grid(Dapple.LayerGeneration.DAPQuadLayerBuilder oDAPbuilder)
         : base(oDAPbuilder)
      {
         InitializeComponent();
         tbFilename.Text = System.IO.Path.ChangeExtension(oDAPbuilder.Title, GRD_EXT);

         cbDownloadOptions.DataSource = Options.Grid.DownloadOptionStrings;
         cbDownloadOptions.SelectedIndex = 0;

         cbDisplayOptions.DataSource = Options.Grid.DisplayOptionStrings;
         cbDisplayOptions.SelectedIndex = 0;         

         oResolution.SetDownloadOptions(this);
         SetDefaultResolution();
      }

		internal override ErrorProvider ErrorProvider
		{
			get
			{
				return base.ErrorProvider;
			}
			set
			{
				base.ErrorProvider = value;
				oResolution.ErrorProvider = value;
			}
		}

      /// <summary>
      /// Set the default resolution
      /// </summary>
      internal override void SetDefaultResolution()
      {
         double dXOrigin, dYOrigin, dXCellSize, dYCellSize;
         int iSizeX, iSizeY;

         string strCoordinateSystem = MainForm.MontajInterface.GetProjection(m_oDAPLayer.ServerURL, m_oDAPLayer.DatasetName);
         MainForm.MontajInterface.GetGridInfo(m_oDAPLayer.ServerURL, m_oDAPLayer.DatasetName, out dXOrigin, out dYOrigin, out iSizeX, out iSizeY, out dXCellSize, out dYCellSize);

         oResolution.Setup(false, strCoordinateSystem, dXOrigin, dYOrigin, iSizeX, iSizeY, dXCellSize, dYCellSize);         
      }

      internal override void SetNativeResolution()
      {
         oResolution.SetNativeResolution();
      }

      /// <summary>
      /// Write out settings for the Grid dataset
      /// </summary>
      /// <param name="oDatasetElement"></param>
      /// <param name="strDestFolder"></param>
      /// <param name="bDefaultResolution"></param>
      /// <returns></returns>
		internal override ExtractSaveResult Save(System.Xml.XmlElement oDatasetElement, string strDestFolder, DownloadSettings.DownloadCoordinateSystem eCS)
      {
         ExtractSaveResult result = base.Save(oDatasetElement, strDestFolder, eCS);

         int iIndex = cbDownloadOptions.SelectedIndex;
         string strFileName = Utility.FileSystem.SanitizeFilename(tbFilename.Text);
			if (!String.IsNullOrEmpty(Options.Grid.DownloadOptionExtension[iIndex]))
			{
				strFileName = System.IO.Path.ChangeExtension(strFileName, Options.Grid.DownloadOptionExtension[iIndex]);
			}
			else
			{
				if (String.IsNullOrEmpty(System.IO.Path.GetExtension(strFileName)))
				{
					strFileName = System.IO.Path.ChangeExtension(strFileName, ".grd");
				}
			}
         strFileName = string.Format(CultureInfo.InvariantCulture, "{0}({1})", strFileName, Options.Grid.DownloadOptionQualifier[iIndex]);

         System.Xml.XmlAttribute oPathAttr = oDatasetElement.OwnerDocument.CreateAttribute("file");
			oPathAttr.Value = System.IO.Path.Combine(strDestFolder, strFileName);
         oDatasetElement.Attributes.Append(oPathAttr);

         System.Xml.XmlAttribute oResolutionAttr = oDatasetElement.OwnerDocument.CreateAttribute("resolution");
         oResolutionAttr.Value = oResolution.ResolutionValueSpecific(eCS).ToString(CultureInfo.InvariantCulture);         
         oDatasetElement.Attributes.Append(oResolutionAttr);

         System.Xml.XmlElement oDisplayElement = oDatasetElement.OwnerDocument.CreateElement("display_options");
         Options.Grid.DisplayOptions eDisplayOption = (Options.Grid.DisplayOptions)cbDisplayOptions.SelectedIndex;
         oDisplayElement.InnerText = eDisplayOption.ToString();
         oDatasetElement.AppendChild(oDisplayElement);

			return result;
      }

      private void cbDownloadOptions_SelectedIndexChanged(object sender, EventArgs e)
      {
         int iIndex = cbDownloadOptions.SelectedIndex;
         tbFilename.Text = System.IO.Path.ChangeExtension(tbFilename.Text, Options.Grid.DownloadOptionExtension[iIndex]);
      }

		internal override DownloadOptions.DuplicateFileCheckResult CheckForDuplicateFiles(String szExtractDirectory, Form hExtractForm)
		{
			String szFilename = System.IO.Path.Combine(szExtractDirectory, System.IO.Path.ChangeExtension(tbFilename.Text, Options.Grid.DownloadOptionExtension[cbDownloadOptions.SelectedIndex]));
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
