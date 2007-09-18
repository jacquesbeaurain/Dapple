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
      private readonly int SAVE_AS_SHP = 1;
      private readonly int SAVE_AS_SHP2 = 3;
      private readonly int SAVE_AS_TAB = 2;
      private readonly int SAVE_AS_TAB2 = 4;
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
         tbGroupName.Text = oDAPbuilder.Name;

         if (MainForm.OpenMap && !string.IsNullOrEmpty(MainForm.MapFileName))
         {
            tbFilename.Text = System.IO.Path.GetFileName(MainForm.MapFileName);
            tbFilename.Enabled = false;
         }
         else
         {
            tbFilename.Text = System.IO.Path.ChangeExtension(oDAPbuilder.Name, MAP_EXT);
         }
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
         oPathAttr.Value = System.IO.Path.Combine(strDestFolder, tbFilename.Text);

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
         if (cbOptions.SelectedIndex == SAVE_AS_MAP)
         {
            tbFilename.Text = System.IO.Path.ChangeExtension(tbFilename.Text, MAP_EXT);
         }
         else if (cbOptions.SelectedIndex == SAVE_AS_SHP || cbOptions.SelectedIndex == SAVE_AS_SHP2)
         {
            tbFilename.Text = System.IO.Path.ChangeExtension(tbFilename.Text, SHP_EXT);
         }
         else if (cbOptions.SelectedIndex == SAVE_AS_TAB || cbOptions.SelectedIndex == SAVE_AS_TAB2)
         {
            tbFilename.Text = System.IO.Path.ChangeExtension(tbFilename.Text, TAB_EXT);
         }
      }
   }
}
