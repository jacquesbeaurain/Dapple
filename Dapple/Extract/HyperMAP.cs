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
   /// Set the options for downloading a hypermap
   /// </summary>
   public partial class HyperMAP : DownloadOptions
   {
      #region Constants
      private readonly string MAP_EXT = ".map";
      #endregion

      /// <summary>
      /// Control where the resolution can be changed
      /// </summary>
      public override bool ResolutionEnabled
      {
         set { oResolution.Enabled = value; }
      }

      /// <summary>
      /// Default constructor
      /// </summary>
      /// <param name="oDAPbuilder"></param>
      public HyperMAP(Dapple.LayerGeneration.DAPQuadLayerBuilder oDAPbuilder)
         : base(oDAPbuilder)
      {
         InitializeComponent();

         if (MainForm.OpenMap && !string.IsNullOrEmpty(MainForm.MapFileName))
         {
            tbFilename.Text = System.IO.Path.GetFileName(MainForm.MapFileName);
            tbFilename.Enabled = false;
         }
         else
         {
            tbFilename.Text = System.IO.Path.ChangeExtension(oDAPbuilder.Name, MAP_EXT);
         }
         tbGroupName.Text = oDAPbuilder.Name;         

         oResolution.SetDownloadOptions(this);         
         SetDefaultResolution();
      }

      /// <summary>
      /// Set the default resolution
      /// </summary>
      public override void SetDefaultResolution()
      {
         double dMinX, dMaxX, dMinY, dMaxY;
         SortedList<double, int> oResolutionList;

         string strCoordinateSystem = MainForm.MontajInterface.GetProjection(m_oDAPLayer.DAPServerURL, m_oDAPLayer.DatasetName);
         MainForm.MontajInterface.GetExtents(m_oDAPLayer.DAPServerURL, m_oDAPLayer.DatasetName, out dMaxX, out dMinX, out dMaxY, out dMinY);
         MainForm.MontajInterface.GetMapInfo(m_oDAPLayer.DAPServerURL, m_oDAPLayer.DatasetName, out oResolutionList);

         oResolution.Setup(strCoordinateSystem, dMinX, dMinY, dMaxX, dMaxY, oResolutionList);
      }

      public override void SetNativeResolution()
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
      public override bool Save(System.Xml.XmlElement oDatasetElement, string strDestFolder, DownloadSettings.DownloadClip eClip, DownloadSettings.DownloadCoordinateSystem eCS)
      {
         base.Save(oDatasetElement, strDestFolder, eClip, eCS);

         System.Xml.XmlAttribute oPathAttr = oDatasetElement.OwnerDocument.CreateAttribute("file");
         oPathAttr.Value = System.IO.Path.Combine(strDestFolder, System.IO.Path.ChangeExtension(tbFilename.Text, MAP_EXT));

         System.Xml.XmlAttribute oResolutionAttr = oDatasetElement.OwnerDocument.CreateAttribute("resolution");
         oResolutionAttr.Value = oResolution.ResolutionValueSpecific(eCS).ToString();

         System.Xml.XmlAttribute oGroupElement = oDatasetElement.OwnerDocument.CreateAttribute("group");
         oGroupElement.Value = tbGroupName.Text;

         oDatasetElement.Attributes.Append(oPathAttr);
         oDatasetElement.Attributes.Append(oResolutionAttr);
         oDatasetElement.Attributes.Append(oGroupElement);

         return true;
      }
   }
}
