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
   /// Set voxel download options
   /// </summary>
   public partial class Voxel : DownloadOptions
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
      public Voxel(Dapple.LayerGeneration.DAPQuadLayerBuilder oDAPbuilder)
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
            tbFilename.Text = System.IO.Path.ChangeExtension(oDAPbuilder.Title, MAP_EXT);
         }
         tbGroupName.Text = oDAPbuilder.Title;         

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
         SortedList<double, int> oX;
         SortedList<double, int> oY;
         SortedList<double, int> oZ;

         string strCoordinateSystem = MainForm.MontajInterface.GetProjection(m_oDAPLayer.ServerURL, m_oDAPLayer.DatasetName);
         MainForm.MontajInterface.GetExtents(m_oDAPLayer.ServerURL, m_oDAPLayer.DatasetName, out dMaxX, out dMinX, out dMaxY, out dMinY);
         MainForm.MontajInterface.GetVoxelInfo(m_oDAPLayer.ServerURL, m_oDAPLayer.DatasetName, out oResolutionList, out oX, out oY, out oZ);

         oResolution.Setup(strCoordinateSystem, dMinX, dMinY, dMaxX, dMaxY, oResolutionList, oX, oY, oZ);
      }

      public override void SetNativeResolution()
      {
         oResolution.SetNativeResolution();
      }

      /// <summary>
      /// Write out settings for the voxel dataset
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
         oDatasetElement.Attributes.Append(oPathAttr);

         System.Xml.XmlAttribute oResolutionAttr = oDatasetElement.OwnerDocument.CreateAttribute("resolution");
         oResolutionAttr.Value = oResolution.ResolutionValueSpecific(eCS).ToString();
         oDatasetElement.Attributes.Append(oResolutionAttr);

         System.Xml.XmlAttribute oGroupAttribute = oDatasetElement.OwnerDocument.CreateAttribute("group");
         oGroupAttribute.Value = tbGroupName.Text;
         oDatasetElement.Attributes.Append(oGroupAttribute);

         return true;
      }
   }
}
