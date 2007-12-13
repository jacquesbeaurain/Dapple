using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Dapple.Extract
{
   /// <summary>
   /// Gather the extract parameters for downloading the selected datasets
   /// </summary>
   public partial class DownloadSettings : Form
   {
      public enum DownloadCoordinateSystem
      {
         Native,
         OriginalMap
      }

      public enum DownloadClip
      {
         None,
         ViewedArea,
         OriginalMap
      }

      #region Member Variables
      List<Dapple.LayerGeneration.LayerBuilder> m_oLayersToDownload;
      private List<DownloadOptions> m_oDownloadSettings = new List<DownloadOptions>();
      private DownloadOptions m_oCurUserControl = null;
		private Form m_oParentForm = null;
      #endregion

      /// <summary>
      /// Default constructor
      /// </summary>
      /// <param name="oLayersToDownload"></param>
      public DownloadSettings(List<Dapple.LayerGeneration.LayerBuilder> oLayersToDownload, Form oParentForm)
      {
         InitializeComponent();

			m_oParentForm = oParentForm;

         m_oLayersToDownload = oLayersToDownload;

         if (!MainForm.OpenMap)
         {
            rbReproject.Enabled = false;
         }

			cFolderControl.Value = System.IO.Path.GetDirectoryName(MainForm.MontajInterface.BaseDirectory());

         lvDatasets.SmallImageList = MainForm.DataTypeImageList;
         lvDatasets.LargeImageList = MainForm.DataTypeImageList;

			bDownload.Enabled = false;

         foreach (Dapple.LayerGeneration.LayerBuilder oBuilder in oLayersToDownload)
         {
            int iImageIndex = 0;

            if (oBuilder is Dapple.LayerGeneration.DAPQuadLayerBuilder)
            {
               Dapple.LayerGeneration.DAPQuadLayerBuilder oDAPbuilder = (Dapple.LayerGeneration.DAPQuadLayerBuilder)oBuilder;

               iImageIndex = MainForm.ImageIndex(oDAPbuilder.DAPType.ToLower());
               if (iImageIndex == -1)
                  MainForm.ImageListIndex("layer");
            }
            else if (oBuilder is Dapple.LayerGeneration.VEQuadLayerBuilder)
               iImageIndex = MainForm.ImageListIndex("live");
            else if (oBuilder is Dapple.LayerGeneration.GeorefImageLayerBuilder)
               iImageIndex = MainForm.ImageListIndex("georef_image");
            else
               iImageIndex = MainForm.ImageListIndex("layer");

				if (!WorldWind.GeographicBoundingBox.FromQuad(MainForm.WorldWindowSingleton.GetSearchBox()).Intersects(oBuilder.Extents))
					iImageIndex = MainForm.ImageListIndex("error");
				else
					bDownload.Enabled = true;

            ListViewItem oItem = new ListViewItem(oBuilder.Title);
            oItem.ImageIndex = iImageIndex;
            oItem.Tag = oBuilder;

            
            // --- create the user control ---

            DownloadOptions oControl = CreateUserControl(oBuilder);
            if (oControl != null)
            {
               // --- no errors, add this dataset to the list ---

               lvDatasets.Items.Add(oItem);
               m_oDownloadSettings.Add(oControl);
               
               oControl.Visible = false;
               oControl.Dock = DockStyle.Fill;
               pSettings.Controls.Add(oControl);
            }
         }

         if (lvDatasets.Items.Count > 0)
         {
            lvDatasets.Items[0].Selected = true;
         }
      }

      #region Private Methods
      /// <summary>
      /// Create the correct control for this dataset
      /// </summary>
      /// <param name="oContainer"></param>
      /// <returns></returns>
      private DownloadOptions CreateUserControl(Dapple.LayerGeneration.LayerBuilder oBuilder)
      {
			if (!WorldWind.GeographicBoundingBox.FromQuad(MainForm.WorldWindowSingleton.GetSearchBox()).Intersects(oBuilder.Extents))
			{
				return new Disabled();
			}

         DownloadOptions oControl = null;
         if (oBuilder is Dapple.LayerGeneration.DAPQuadLayerBuilder)
         {
            Dapple.LayerGeneration.DAPQuadLayerBuilder oDAPbuilder = (Dapple.LayerGeneration.DAPQuadLayerBuilder)oBuilder;
            
            if (oDAPbuilder.DAPType.ToLower() == "map") {
               oControl = new HyperMAP(oDAPbuilder);
            }
            else if (oDAPbuilder.DAPType.ToLower() == "grid")
            {
               oControl = new Grid(oDAPbuilder);
            }
            else if (oDAPbuilder.DAPType.ToLower() == "picture")
            {
               oControl = new Picture(oDAPbuilder);
            }               
            else if (oDAPbuilder.DAPType.ToLower() == "point")
            {
               oControl = new HyperXYZ(oDAPbuilder);
            }
            else if (oDAPbuilder.DAPType.ToLower() == "database")
            {
               oControl = new Database(oDAPbuilder);
            }
            else if (oDAPbuilder.DAPType.ToLower() == "document")
            {
               oControl = new Document(oDAPbuilder);
            }
            else if (oDAPbuilder.DAPType.ToLower() == "spf")
            {
               oControl = new GIS(oDAPbuilder);
            }
            else if (oDAPbuilder.DAPType.ToLower() == "generic")
            {
               oControl = new Generic(oDAPbuilder);
            }
            else if (oDAPbuilder.DAPType.ToLower() == "voxel")
            {
               oControl = new Voxel(oDAPbuilder);
            }
            else if (oDAPbuilder.DAPType.ToLower() == "arcgis")
            {
               oControl = new ArcGIS(oDAPbuilder);
            }
            else if (oDAPbuilder.DAPType.ToLower() == "imageserver")
            {
               oControl = new PictureWithoutResolution(oDAPbuilder);
            }
            else if (oDAPbuilder.DAPType.ToLower() == "picturesection")
            {
               oControl = new SectionPicture(oDAPbuilder);
            }
            else if (oDAPbuilder.DAPType.ToLower() == "gridsection")
            {
               oControl = new SectionGrid(oDAPbuilder);
            }
         }
         else 
         {
            oControl = new PictureWithoutResolution(oBuilder);
         }         
         return oControl;
      }
      #endregion

      #region Event Handler
      /// <summary>
      /// The selected dataset has changed, display its extract parameters
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void lvDatasets_SelectedIndexChanged(object sender, EventArgs e)
      {
         DownloadOptions oControl = null;

         if (m_oCurUserControl != null)
            m_oCurUserControl.Visible = false;

         if (lvDatasets.SelectedIndices.Count == 1)
         {
            oControl = m_oDownloadSettings[lvDatasets.SelectedIndices[0]];
            oControl.Visible = true;
            m_oCurUserControl = oControl;
         }
      }
      
      /// <summary>
      /// Download all the datasets
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void bDownload_Click(object sender, EventArgs e)
      {
			if (!DoFilenamePrompt())
			{
				this.DialogResult = DialogResult.None;
				return;
			}

         try
         {
            DownloadClip eClip = DownloadClip.ViewedArea;
            DownloadCoordinateSystem eCS = DownloadCoordinateSystem.Native;

            if (rbReproject.Checked)
               eCS = DownloadCoordinateSystem.OriginalMap;


            System.Xml.XmlDocument oExtractDoc = new System.Xml.XmlDocument();
            System.Xml.XmlElement oRootElement = oExtractDoc.CreateElement("geosoft_xml");
            System.Xml.XmlElement oExtractElement = oExtractDoc.CreateElement("extract");

            oExtractDoc.AppendChild(oRootElement);
            oRootElement.AppendChild(oExtractElement);

				// --- create xml document for each dataset ---

            foreach (DownloadOptions oDataset in m_oDownloadSettings)
            {
               System.Xml.XmlElement oDatasetElement = oExtractDoc.CreateElement("dataset");

               if (oDataset.Save(oDatasetElement, cFolderControl.Value, eClip, eCS))
               {
                  oExtractElement.AppendChild(oDatasetElement);
               }
            }
#if DEBUG
            System.Xml.XmlElement oDebugElement = oExtractDoc.CreateElement("debug");
            WorldWind.GeographicBoundingBox oViewAOI = WorldWind.GeographicBoundingBox.FromQuad(MainForm.WorldWindowSingleton.GetSearchBox());
            oDebugElement.SetAttribute("wgs84_west", oViewAOI.West.ToString("f2"));
            oDebugElement.SetAttribute("wgs84_south", oViewAOI.South.ToString("f2"));
            oDebugElement.SetAttribute("wgs84_east", oViewAOI.East.ToString("f2"));
            oDebugElement.SetAttribute("wgs84_north", oViewAOI.North.ToString("f2"));
            oDebugElement.SetAttribute("clip_setting", eClip.ToString());
            oExtractElement.AppendChild(oDebugElement);
#endif

            DatasetDisclaimer oDisclaimers = new DatasetDisclaimer(m_oLayersToDownload, oExtractDoc);
				oDisclaimers.ShowInTaskbar = false;

				if (oDisclaimers.HasDisclaimer)
				{
					if (oDisclaimers.ShowDialog(this) == DialogResult.OK)
					{
						DoDownload(oExtractDoc);
					}
					else
					{
						this.DialogResult = DialogResult.Cancel;
					}
				}
				else
				{
					DoDownload(oExtractDoc);
				}
         }
         catch (System.Runtime.Remoting.RemotingException)
         {
				this.DialogResult = DialogResult.Abort;
				return;
         }
      }

		/// <summary>
		/// Checks each dataset to download if its file already exists
		/// </summary>
		/// <returns></returns>
		private bool DoFilenamePrompt()
		{
			for (int count = 0; count < m_oDownloadSettings.Count; count++)
			{
				DownloadOptions.DuplicateFileCheckResult oResult = m_oDownloadSettings[count].CheckForDuplicateFiles(cFolderControl.Value, this);

				if (oResult == DownloadOptions.DuplicateFileCheckResult.YesAndStopAsking) return true;
				if (oResult == DownloadOptions.DuplicateFileCheckResult.No)
				{
					this.SuspendLayout();
					lvDatasets.SelectedIndices.Clear();
					lvDatasets.SelectedIndices.Add(count);
					this.ResumeLayout(true);
					return false;
				}
			}

			return true;
		}

		private void DoDownload(System.Xml.XmlDocument oExtractDoc)
		{
			this.Hide();
			m_oParentForm.Activate();
			Application.DoEvents();
			MainForm.MontajInterface.Download(oExtractDoc.OuterXml);
		}

      #endregion

      private void DownloadSettings_Shown(object sender, EventArgs e)
      {
         lvDatasets.Columns[0].Width = lvDatasets.ClientSize.Width;
      }

		private void cFolderControl_Validating(object sender, CancelEventArgs e)
		{
			if (cFolderControl.Value.Trim().Equals(String.Empty))
			{
				cErrorProvider.SetError(cFolderControl, "A directory is required.");
				e.Cancel = true;
			}
			else
			{
				String szError = String.Empty;
				e.Cancel = !cFolderControl.bIsValid(ref szError);
				cErrorProvider.SetError(cFolderControl, szError);
			}
		}
   }
}