using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using MWA.Progress;
using System.Xml;

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

      #region Member Variables
   	private readonly List<LayerGeneration.LayerBuilder> m_oLayersToDownload;
      private readonly List<DownloadOptions> m_oDownloadSettings = new List<DownloadOptions>();
      private DownloadOptions m_oCurUserControl;
		private bool m_blLayersDownloaded = true;
      #endregion

      /// <summary>
      /// Default constructor
      /// </summary>
      /// <param name="oLayersToDownload"></param>
      public DownloadSettings(List<Dapple.LayerGeneration.LayerBuilder> oLayersToDownload)
      {
         InitializeComponent();

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

			String strBaseDirectory = MainForm.MontajInterface.BaseDirectory();
			if (String.IsNullOrEmpty(strBaseDirectory))
			{
				cFolderControl.Value = String.Empty;
			}
			else
			{
				cFolderControl.Value = System.IO.Path.GetDirectoryName(strBaseDirectory);
			}


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

		private static string GetExtractionVerb(Dapple.LayerGeneration.LayerBuilder oBuilder)
		{
			if (oBuilder is Dapple.LayerGeneration.DAPQuadLayerBuilder &&
				(oBuilder as Dapple.LayerGeneration.DAPQuadLayerBuilder).IsFromPersonalDapServer)
			{
				return "open";
			}

			return "extract";
		}

      /// <summary>
      /// Create the correct control for this dataset
      /// </summary>
      /// <param name="oContainer"></param>
      /// <returns></returns>
      private DownloadOptions CreateUserControl(Dapple.LayerGeneration.LayerBuilder oBuilder)
      {
			if (!WorldWind.GeographicBoundingBox.FromQuad(MainForm.WorldWindowSingleton.GetSearchBox()).Intersects(oBuilder.Extents))
			{
				return new Disabled("This data layer cannot be " + GetExtractionVerb(oBuilder) + "ed. View the data layer extents within the currently viewed area and try again.");
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

				if (oDAPbuilder.IsFromPersonalDapServer)
				{
					if (oDAPbuilder.DAPType.Equals("spf", StringComparison.OrdinalIgnoreCase)
						&& oDAPbuilder.LocalFilename.EndsWith("(TAB)", StringComparison.OrdinalIgnoreCase))
					{
						return new Disabled("This data layer will not be opened because TAB files are not supported in ArcMap.");
					}
					else
					{
						oControl = new PersonalDataset(oDAPbuilder);
						oControl.ErrorProvider = cErrorProvider;
						return oControl;
					}
				}

            if (oDAPbuilder.DAPType.ToLower() == "map")
				{
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
					if (MainForm.Client != Options.Client.ClientType.MapInfo)
					{
						oControl = new Generic(oDAPbuilder);
					}
					else
					{
						oControl = new Disabled("This data layer will not be extracted as acQuire connections are not a supported format in MapInfo.");
					}
            }
            else if (oDAPbuilder.DAPType.ToLower() == "voxel")
            {
					if (MainForm.Client != Options.Client.ClientType.MapInfo)
					{
						oControl = new Voxel(oDAPbuilder);
					}
					else
					{
						oControl = new Disabled("This data layer will not be extracted as voxel is not a supported format in MapInfo.");
					}
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

			if (oControl != null) oControl.ErrorProvider = cErrorProvider;
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
			if (!this.ValidateChildren())
			{
				this.DialogResult = DialogResult.None;
				return;
			}

			if (!DoFilenamePrompt())
			{
				this.DialogResult = DialogResult.None;
				return;
			}

         try
         {
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
					System.Xml.XmlElement oDatasetElement;
					if (m_oDownloadSettings[count] is PersonalDataset)
					{
						oDatasetElement = oExtractDoc.CreateElement("personal_dataset");
					}
					else
					{
						oDatasetElement = oExtractDoc.CreateElement("dataset");
					}

					switch (m_oDownloadSettings[count].Save(oDatasetElement, cFolderControl.Value, eCS))
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

#if DEBUG
				ExtractDebug oDebug = new ExtractDebug(oExtractDoc);
				oDebug.ShowDialog(this);
#endif

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

		public bool DoDownload(XmlDocument oExtractDoc)
		{
			ProgressWindow oProgress = new ProgressWindow(false, false);
			oProgress.Text = "Extraction in Progress";
			oProgress.SetText(Utility.EnumUtils.GetDescription(MainForm.Client) + " is extracting your datasets.");
			oProgress.Height = 77;
			ThreadPool.QueueUserWorkItem(new WaitCallback(DoDownloadThreadMain), new Object[] { oExtractDoc, oProgress});
			oProgress.ShowDialog();
			if (oProgress.Exception != null) throw oProgress.Exception;
			return (bool)oProgress.ReturnValue;
		}

		private void DoDownloadThreadMain(Object args)
		{
			XmlDocument oExtractDoc = ((Object[])args)[0] as XmlDocument;
			ProgressWindow oProgress = ((Object[])args)[1] as ProgressWindow;
			try
			{
				Program.FocusOnCaller();
				oProgress.ReturnValue = MainForm.MontajInterface.Download(oExtractDoc.OuterXml) > 0;
			}
			catch (Exception ex)
			{
				oProgress.Exception = ex;
			}
			oProgress.End();
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