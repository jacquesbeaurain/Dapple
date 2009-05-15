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
   /// Set the options for downloading a hypermap
   /// </summary>
   internal partial class HyperMAP : DownloadOptions
   {
      #region Constants
      private readonly string MAP_EXT = ".map";
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
			get { return true; }
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
      /// Default constructor
      /// </summary>
      /// <param name="oDAPbuilder"></param>
      internal HyperMAP(Dapple.LayerGeneration.DAPQuadLayerBuilder oDAPbuilder)
         : base(oDAPbuilder)
      {
         InitializeComponent();

			if (MainForm.MontajInterface.HostHasOpenMap() && !string.IsNullOrEmpty(MainForm.MapFileName))
         {
            tbFilename.Text = System.IO.Path.GetFileName(MainForm.MapFileName);
            tbFilename.Enabled = false;
         }
         else
         {
            tbFilename.Text = System.IO.Path.ChangeExtension(oDAPbuilder.Title, MAP_EXT);
         }
         tbGroupName.Text = oDAPbuilder.Title;         

         oResolution.SetDownloadOptions(this);         
         SetDefaultResolution();
      }

      /// <summary>
      /// Set the default resolution
      /// </summary>
      internal override void SetDefaultResolution()
      {
         double dMinX, dMaxX, dMinY, dMaxY;
         SortedList<double, int> oResolutionList;

         string strCoordinateSystem = m_strLayerProjection;
         MainForm.MontajInterface.GetExtents(m_oDAPLayer.ServerURL, m_oDAPLayer.DatasetName, out dMaxX, out dMinX, out dMaxY, out dMinY);
         MainForm.MontajInterface.GetMapInfo(m_oDAPLayer.ServerURL, m_oDAPLayer.DatasetName, out oResolutionList);

         oResolution.Setup(strCoordinateSystem, dMinX, dMinY, dMaxX, dMaxY, oResolutionList);
      }

      internal override void SetNativeResolution()
      {
         oResolution.SetNativeResolution();
      }

      /// <summary>
      /// Write out settings for the HyperMAP dataset
      /// </summary>
      /// <param name="oDatasetElement"></param>
      /// <param name="strDestFolder"></param>
      /// <param name="bDefaultResolution"></param>
      /// <returns></returns>
		internal override ExtractSaveResult Save(System.Xml.XmlElement oDatasetElement, string strDestFolder, DownloadSettings.DownloadCoordinateSystem eCS)
      {
         ExtractSaveResult result = base.Save(oDatasetElement, strDestFolder, eCS);

         System.Xml.XmlAttribute oPathAttr = oDatasetElement.OwnerDocument.CreateAttribute("file");
         oPathAttr.Value = System.IO.Path.Combine(strDestFolder, System.IO.Path.ChangeExtension(Utility.FileSystem.SanitizeFilename(tbFilename.Text), MAP_EXT));

         System.Xml.XmlAttribute oResolutionAttr = oDatasetElement.OwnerDocument.CreateAttribute("resolution");
         oResolutionAttr.Value = oResolution.ResolutionValueSpecific(eCS).ToString(CultureInfo.InvariantCulture);

         System.Xml.XmlAttribute oGroupElement = oDatasetElement.OwnerDocument.CreateAttribute("group");
         oGroupElement.Value = tbGroupName.Text;

         oDatasetElement.Attributes.Append(oPathAttr);
         oDatasetElement.Attributes.Append(oResolutionAttr);
         oDatasetElement.Attributes.Append(oGroupElement);

			return result;
      }

		internal override DownloadOptions.DuplicateFileCheckResult CheckForDuplicateFiles(String szExtractDirectory, Form hExtractForm)
		{
			String szFilename = System.IO.Path.Combine(szExtractDirectory, System.IO.Path.ChangeExtension(tbFilename.Text, MAP_EXT));
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

		private void tbGroupName_Validating(object sender, CancelEventArgs e)
		{
			if (String.IsNullOrEmpty(tbGroupName.Text))
			{
				m_oErrorProvider.SetError(tbGroupName, "Field cannot be empty.");
				e.Cancel = true;
			}
			else
			{
				m_oErrorProvider.SetError(tbGroupName, String.Empty);
			}
		}
   }
}
