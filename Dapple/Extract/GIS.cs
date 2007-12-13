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
   /// Set the options for this gis dataset
   /// </summary>
   public partial class GIS : DownloadOptions
   {
      #region Constants
      private readonly string MAP_EXT = ".map";
      private readonly string SHP_EXT = ".shp";
      private readonly string TAB_EXT = ".tab";      
      private readonly int SAVE_AS_MAP = 0;
      private readonly int SAVE_AS_SHP_IMPORT = 1;
      private readonly int SAVE_AS_SHP_NOIMPORT = 3;
      private readonly int SAVE_AS_TAB_IMPORT = 2;
      private readonly int SAVE_AS_TAB_NOIMPORT = 4;
      #endregion

      /// <summary>
      /// Default constructor
      /// </summary>
      /// <param name="oDAPbuilder"></param>
      public GIS(Dapple.LayerGeneration.DAPQuadLayerBuilder oDAPbuilder)
         : base(oDAPbuilder)
      {
         InitializeComponent();

         cbOptions.DataSource = Options.GIS.OMDownloadOptionStrings;
         cbOptions.SelectedIndex = 0;
         tbGroupName.Text = oDAPbuilder.Title;
         tbFilename.Text = System.IO.Path.ChangeExtension(oDAPbuilder.Title, MAP_EXT);

         ConfigureDialog();
      }

		public override bool OpenInMap
		{
			get { return cbOptions.SelectedIndex == SAVE_AS_MAP || cbOptions.SelectedIndex == SAVE_AS_SHP_IMPORT || cbOptions.SelectedIndex == SAVE_AS_TAB_IMPORT; }
		}

      /// <summary>
      /// Write out settings for the GIS dataset
      /// </summary>
      /// <param name="oDatasetElement"></param>
      /// <param name="strDestFolder"></param>
      /// <param name="bDefaultResolution"></param>
      /// <returns></returns>
      public override bool Save(System.Xml.XmlElement oDatasetElement, string strDestFolder, DownloadSettings.DownloadClip eClip, DownloadSettings.DownloadCoordinateSystem eCS)
      {
         base.Save(oDatasetElement, strDestFolder, eClip, eCS);

         System.Xml.XmlAttribute oPathAttr = oDatasetElement.OwnerDocument.CreateAttribute("file");
         if (cbOptions.SelectedIndex == SAVE_AS_MAP)
         {
            oPathAttr.Value = System.IO.Path.Combine(strDestFolder, System.IO.Path.ChangeExtension(tbFilename.Text, MAP_EXT));
         }
         else if (cbOptions.SelectedIndex == SAVE_AS_SHP_IMPORT || cbOptions.SelectedIndex == SAVE_AS_SHP_NOIMPORT)
         {
            // Shape file uses a namespace name, not a file name (produces oodles of files)
            oPathAttr.Value = System.IO.Path.Combine(strDestFolder, System.IO.Path.GetFileNameWithoutExtension(tbFilename.Text));
         }
         else if (cbOptions.SelectedIndex == SAVE_AS_TAB_IMPORT || cbOptions.SelectedIndex == SAVE_AS_TAB_NOIMPORT)
         {
            oPathAttr.Value = System.IO.Path.Combine(strDestFolder, System.IO.Path.ChangeExtension(tbFilename.Text, TAB_EXT));
         }
         oDatasetElement.Attributes.Append(oPathAttr);

         System.Xml.XmlAttribute oGroupAttribute = oDatasetElement.OwnerDocument.CreateAttribute("group");
         oGroupAttribute.Value = tbGroupName.Text;
         oDatasetElement.Attributes.Append(oGroupAttribute);

         System.Xml.XmlElement oDownloadElement = oDatasetElement.OwnerDocument.CreateElement("download_options");
         Options.GIS.OMDownloadOptions eOption = (Options.GIS.OMDownloadOptions)cbOptions.SelectedIndex;
         oDownloadElement.InnerXml = eOption.ToString();
         oDatasetElement.AppendChild(oDownloadElement);

         return true;
      }

      /// <summary>
      /// Set the filename extension correctly
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void cbOptions_SelectedIndexChanged(object sender, EventArgs e)
      {
         ConfigureDialog();
      }

      private void ConfigureDialog()
      {
			// --- Set up the filename box ---
         if (cbOptions.SelectedIndex == SAVE_AS_MAP)
         {
            tbFilename.Text = System.IO.Path.ChangeExtension(tbFilename.Text, MAP_EXT);
            lFileName.Visible = true;
            lFileName.Text = "Map name:";
            tbFilename.Visible = true;
         }
         else if (cbOptions.SelectedIndex == SAVE_AS_SHP_IMPORT || cbOptions.SelectedIndex == SAVE_AS_SHP_NOIMPORT)
         {
            tbFilename.Text = System.IO.Path.ChangeExtension(tbFilename.Text, null);
            lFileName.Visible = true;
            lFileName.Text = "Namespace:";
            tbFilename.Visible = true;
         }
         else if (cbOptions.SelectedIndex == SAVE_AS_TAB_IMPORT || cbOptions.SelectedIndex == SAVE_AS_TAB_NOIMPORT)
         {
            tbFilename.Text = System.IO.Path.ChangeExtension(tbFilename.Text, TAB_EXT);
            lFileName.Visible = true;
            lFileName.Text = "File name:";
            tbFilename.Visible = true;
         }

			// --- Set up the group box ---

			if (cbOptions.SelectedIndex == SAVE_AS_MAP || cbOptions.SelectedIndex == SAVE_AS_SHP_IMPORT || cbOptions.SelectedIndex == SAVE_AS_TAB_IMPORT)
			{
				lGroupName.Enabled = true;
				tbGroupName.Enabled = true;
			}
			else
			{
				lGroupName.Enabled = false;
				tbGroupName.Enabled = false;
				if (tbGroupName.Text.Trim().Equals(String.Empty))
				{
					tbGroupName.Text = m_oDAPLayer.Title;
				}
			}
      }

		public override DownloadOptions.DuplicateFileCheckResult CheckForDuplicateFiles(string szExtractDirectory, Form hExtractForm)
		{
			if (cbOptions.SelectedIndex == SAVE_AS_MAP)
			{
				// Only potential collision is the map name, but those are uniquely generated by OM.
				return DuplicateFileCheckResult.Yes;
			}
			else if (cbOptions.SelectedIndex == SAVE_AS_SHP_IMPORT || cbOptions.SelectedIndex == SAVE_AS_SHP_NOIMPORT)
			{
				String szFilename = System.IO.Path.Combine(szExtractDirectory, System.IO.Path.ChangeExtension(tbFilename.Text, SHP_EXT));
				if (System.IO.File.Exists(szFilename))
				{
					return QueryOverwriteFile("The file \"" + szFilename + "\" already exists.  Overwrite?", hExtractForm);
				}
				else
				{
					return DuplicateFileCheckResult.Yes;
				}
			}
			else if (cbOptions.SelectedIndex == SAVE_AS_TAB_IMPORT || cbOptions.SelectedIndex == SAVE_AS_TAB_NOIMPORT)
			{
				String szFilename = System.IO.Path.Combine(szExtractDirectory, System.IO.Path.ChangeExtension(tbFilename.Text, TAB_EXT));
				if (System.IO.File.Exists(szFilename))
				{
					return QueryOverwriteFile("The file \"" + szFilename + "\" already exists.  Overwrite?", hExtractForm);
				}
				else
				{
					return DuplicateFileCheckResult.Yes;
				}
			}
			else
			{
				throw new ApplicationException("Unknown SPF download option");
			}
		}
   }
}
