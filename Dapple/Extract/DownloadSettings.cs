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
		private bool m_blLayersDownloaded = true;
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

			if (MainForm.Client == Options.Client.ClientType.OasisMontaj)
			{
				rbCSNative.Enabled = true;
				rbReproject.Enabled = MainForm.MontajInterface.HostHasOpenMap();
				rbCSNative.Checked = true;
			}
			else
			{
				rbReproject.Enabled = MainForm.MontajInterface.HostHasOpenMap();
				rbCSNative.Enabled = !rbReproject.Enabled;
				if (rbReproject.Enabled)
					rbReproject.Checked = true;
				else
					rbCSNative.Checked = true;
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
            
            // --- create the user control ---
				DownloadOptions oControl = CreateUserControl(oBuilder);

				if (oControl is Disabled)
				{
					iImageIndex = MainForm.ImageListIndex("error");
				}
				else
				{
					bDownload.Enabled = true;
				}

				ListViewItem oItem = new ListViewItem(oBuilder.Title);
				oItem.ImageIndex = iImageIndex;
				oItem.Tag = oBuilder;

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
				return new Disabled("This data layer will not be extracted because it does not intersect with the viewed area.");
			}

         DownloadOptions oControl = null;
         if (oBuilder is Dapple.LayerGeneration.DAPQuadLayerBuilder)
         {
            Dapple.LayerGeneration.DAPQuadLayerBuilder oDAPbuilder = (Dapple.LayerGeneration.DAPQuadLayerBuilder)oBuilder;

				double dummy1 = 0, dummy2 = 0, dummy3 = 0, dummy4 = 0;
				if (MainForm.MontajInterface.GetExtents(oDAPbuilder.ServerURL, oDAPbuilder.DatasetName, out dummy1, out dummy2, out dummy3, out dummy4) == false)
				{
					return new Disabled("This data layer will not be extracted because its metadata could not be accessed.  This usually indicates that you do not have the required permissions to access it.");
				}
            
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
					if (MainForm.Client != Options.Client.ClientType.MapInfo)
					{
						oControl = new ArcGIS(oDAPbuilder);
					}
					else
					{
						oControl = new Disabled("This data layer will not be extracted as LYR is not a supported format in MapInfo.");
					}
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
			else if (oBuilder is KML.KMLLayerBuilder)
			{
				oControl = new Disabled("This data layer will not be extracted as KML extraction is not currently supported.");
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

				int iCount = 0;
				for (int count = 0; count < m_oDownloadSettings.Count; count++)
				{
					System.Xml.XmlElement oDatasetElement = oExtractDoc.CreateElement("dataset");

					switch (m_oDownloadSettings[count].Save(oDatasetElement, cFolderControl.Value, eClip, eCS))
					{
						case DownloadOptions.ExtractSaveResult.Cancel:
							SetActivePage(count);
							DialogResult = DialogResult.None;
							return;
						case DownloadOptions.ExtractSaveResult.Extract:
							iCount++;
							oExtractElement.AppendChild(oDatasetElement);
							break;
						case DownloadOptions.ExtractSaveResult.Ignore:
							continue;

					}
				}

				if (iCount == 0)
				{
					this.DialogResult = DialogResult.None;
					return;
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
				DatasetDisclaimer oDisclaimers = null;
				try
				{
					oDisclaimers = new DatasetDisclaimer(m_oLayersToDownload, oExtractDoc);
				}
				catch (System.Net.WebException ex)
				{
					String strErrorMessage = "Could not access disclaimer information for ";
					if (ex.Data["dataset"] != null)
					{
						strErrorMessage += "data layer " + ex.Data["dataset"];
					}
					else
					{
						strErrorMessage += "one or more data layers";
					}

					if (ex.Message != null)
					{
						strErrorMessage += ":" + Environment.NewLine + ex.Message;
					}
					else
					{
						strErrorMessage += ".";
					}

					Program.ShowMessageBox(
						strErrorMessage,
						"Extract Layers",
						MessageBoxButtons.OK,
						MessageBoxDefaultButton.Button1,
						MessageBoxIcon.Error);
					this.DialogResult = DialogResult.None;
					return;
				}
				oDisclaimers.ShowInTaskbar = false;

				if (oDisclaimers.HasDisclaimer)
				{
					if (oDisclaimers.ShowDialog(this) == DialogResult.OK)
					{
						m_blLayersDownloaded = DoDownload(oExtractDoc);
					}
					else
					{
						this.DialogResult = DialogResult.Cancel;
					}
				}
				else
				{
					m_blLayersDownloaded = DoDownload(oExtractDoc);
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
					SetActivePage(count);
					return false;
				}
			}

			return true;
		}

		private void SetActivePage(int count)
		{
			this.SuspendLayout();
			lvDatasets.SelectedIndices.Clear();
			lvDatasets.SelectedIndices.Add(count);
			this.ResumeLayout(true);
		}

		private bool DoDownload(System.Xml.XmlDocument oExtractDoc)
		{
			this.Hide();
			m_oParentForm.Activate();
			Application.DoEvents();
			return MainForm.MontajInterface.Download(oExtractDoc.OuterXml) > 0;
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

		public bool LayersDownloaded
		{
			get { return m_blLayersDownloaded; }
		}
   }
}