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
   /// Set the download options for the grid
   /// </summary>
   public partial class SectionPicture : DownloadOptions
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
			get { return ((Options.SectionPicture.DisplayOptions)cbDisplayOptions.SelectedIndex) != Options.SectionPicture.DisplayOptions.DoNotDisplay; }
		}

      /// <summary>
      /// Default constructor
      /// </summary>
      /// <param name="oDAPbuilder"></param>
      public SectionPicture(Dapple.LayerGeneration.DAPQuadLayerBuilder oDAPbuilder)
         : base(oDAPbuilder)
      {
         InitializeComponent();
         tbFilename.Text = System.IO.Path.ChangeExtension(oDAPbuilder.Title, TIF_EXT);

         cbDisplayOptions.DataSource = Options.SectionPicture.DisplayOptionStrings;
         cbDisplayOptions.SelectedIndex = 0;         

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

         oResolution.Setup(false, strCoordinateSystem, dXOrigin, dYOrigin, iSizeX, iSizeY, dXCellSize, dYCellSize);         
      }

      public override void SetNativeResolution()
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
      public override bool Save(System.Xml.XmlElement oDatasetElement, string strDestFolder, DownloadSettings.DownloadClip eClip, DownloadSettings.DownloadCoordinateSystem eCS)
      {
         // --- cannot reproject section data --- 

         base.Save(oDatasetElement, strDestFolder, eClip, DownloadSettings.DownloadCoordinateSystem.Native);
         
         string strFileName = System.IO.Path.ChangeExtension(tbFilename.Text, TIF_EXT);         
         System.Xml.XmlAttribute oPathAttr = oDatasetElement.OwnerDocument.CreateAttribute("file");
         oPathAttr.Value = System.IO.Path.Combine(strDestFolder, System.IO.Path.ChangeExtension(tbFilename.Text, TIF_EXT));
         oDatasetElement.Attributes.Append(oPathAttr);

         System.Xml.XmlAttribute oResolutionAttr = oDatasetElement.OwnerDocument.CreateAttribute("resolution");
         oResolutionAttr.Value = oResolution.ResolutionValueSpecific(eCS).ToString();         
         oDatasetElement.Attributes.Append(oResolutionAttr);

         System.Xml.XmlElement oDisplayElement = oDatasetElement.OwnerDocument.CreateElement("display_options");
         Options.SectionPicture.DisplayOptions eDisplayOption = (Options.SectionPicture.DisplayOptions)cbDisplayOptions.SelectedIndex;
         oDisplayElement.InnerText = eDisplayOption.ToString();
         oDatasetElement.AppendChild(oDisplayElement);

         return true;
      }
   }
}
