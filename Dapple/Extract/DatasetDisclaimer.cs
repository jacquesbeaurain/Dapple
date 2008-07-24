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
   public partial class DatasetDisclaimer : Form
   {      
      #region Properties
      /// <summary>
      /// Return a value indicating if we have any disclaimers
      /// </summary>
      public bool HasDisclaimer
      {
         get { return (lvDatasets.Items.Count > 0); }
      }
      #endregion

      /// <summary>
      /// Default constructor
      /// </summary>
      /// <param name="oLayersToDownload"></param>
      public DatasetDisclaimer(List<Dapple.LayerGeneration.LayerBuilder> oLayersToDownload, System.Xml.XmlDocument oDownload)
      {
         InitializeComponent();

         string strTempFile = System.IO.Path.GetTempFileName();
         System.Xml.XmlReader oReader = null;

         foreach (Dapple.LayerGeneration.LayerBuilder oBuilder in oLayersToDownload)
         {
				if (oBuilder is Dapple.LayerGeneration.DAPQuadLayerBuilder && ((Dapple.LayerGeneration.DAPQuadLayerBuilder)oBuilder).ServerMajorVersion >= 11)
            {
               Dapple.LayerGeneration.DAPQuadLayerBuilder oDAPbuilder = (Dapple.LayerGeneration.DAPQuadLayerBuilder)oBuilder;

               Geosoft.Dap.Command oCommand = new Geosoft.Dap.Command(oDAPbuilder.ServerURL, false, Geosoft.Dap.Command.Version.GEOSOFT_XML_1_1, Dapple.MainForm.DOWNLOAD_TIMEOUT);
               Geosoft.Dap.Common.DataSet oDataset = new Geosoft.Dap.Common.DataSet();
               oDataset.Name = oDAPbuilder.DatasetName;
               oDataset.Url = oDAPbuilder.ServerURL;

					System.Xml.XmlDocument oDoc = null;
					try
					{
						oDoc = oCommand.GetDisclaimer(oDataset);
					}
					catch (System.Net.WebException ex)
					{
						ex.Data["dataset"] = oBuilder.Title;
						throw;
					}
					oDoc.Save(strTempFile);
					oReader = System.Xml.XmlReader.Create(strTempFile);

               if (oReader.ReadToFollowing("disclaimer"))
               {
                  if (string.Compare(oReader.GetAttribute("value"), "true", true) == 0)
                  {
                     // --- read the base 64 encoded text into a temporary file ---

                     string strTempHtmFile = System.IO.Path.GetTempFileName();
                     System.IO.FileStream oOutputStream = new System.IO.FileStream(strTempHtmFile, System.IO.FileMode.Create);

                     byte[] bBuffer = new byte[65536];
                     int iCount = 0;

                     do
                     {
                        iCount = oReader.ReadElementContentAsBase64(bBuffer, 0, 65536);
                        oOutputStream.Write(bBuffer, 0, iCount);
                     } while (iCount != 0);


                     // --- close the output stream ---

                     if (oOutputStream != null)
                        oOutputStream.Close();
                     oOutputStream = null;

                     ListViewItem oItem = new ListViewItem();
                     oItem.Name = oDAPbuilder.DatasetName;
							oItem.Text = oDAPbuilder.Title;
                     oItem.Tag = strTempHtmFile;
                     lvDatasets.Items.Add(oItem);
                  }
               }
               oReader.Close();
            }
         }
         System.IO.File.Delete(strTempFile);

         if (lvDatasets.Items.Count > 0)
         {
				lvDatasets.Items[0].Selected = true;
				wbDisclaimer.Navigate(lvDatasets.Items[0].Tag as string);
         }
      }

      #region Event Handler
      /// <summary>
      /// The selected dataset has changed, display its extract parameters
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void lvDatasets_SelectedIndexChanged(object sender, EventArgs e)
      {
         if (lvDatasets.SelectedIndices.Count == 1)
         {
            try
            {
               wbDisclaimer.Navigate(lvDatasets.SelectedItems[0].Tag as string);
            }
            catch { }
         }
      }      
      
      /// <summary>
      /// Download all the datasets
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void bAccept_Click(object sender, EventArgs e)
      {
			DeleteTempDisclaimerFiles();  
      }

		private void bCancel_Click(object sender, EventArgs e)
		{
			DeleteTempDisclaimerFiles();  
		}


		private void DeleteTempDisclaimerFiles()
		{
			foreach (ListViewItem oItem in lvDatasets.Items)
			{
				try
				{
					string strFile = oItem.Tag as string;
					System.IO.File.Delete(strFile);
				}
				catch { }
			}
		}

      #endregion      
   }
}