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
   /// Set the hyxz options
   /// </summary>
   public partial class HyperXYZ : DownloadOptions
   {
      #region Constants
      private readonly string DATABASE_EXT = ".gdb";
      #endregion

      /// <summary>
      /// Default constructor
      /// </summary>
      /// <param name="oDAPbuilder"></param>
      public HyperXYZ(Dapple.LayerGeneration.DAPQuadLayerBuilder oDAPbuilder)
         : base(oDAPbuilder)
      {
         InitializeComponent();
         tbFilename.Text = System.IO.Path.ChangeExtension(oDAPbuilder.Title, DATABASE_EXT);
      }

		public override bool OpenInMap
		{
			get { return true; }
		}

      /// <summary>
      /// Write out settings for the HyperXYZ dataset
      /// </summary>
      /// <param name="oDatasetElement"></param>
      /// <param name="strDestFolder"></param>
      /// <param name="bDefaultResolution"></param>
      /// <returns></returns>
      public override bool Save(System.Xml.XmlElement oDatasetElement, string strDestFolder, DownloadSettings.DownloadClip eClip, DownloadSettings.DownloadCoordinateSystem eCS)
      {
         base.Save(oDatasetElement, strDestFolder, eClip, eCS);

         System.Xml.XmlAttribute oPathAttr = oDatasetElement.OwnerDocument.CreateAttribute("file");
         oPathAttr.Value = System.IO.Path.Combine(strDestFolder, System.IO.Path.ChangeExtension(tbFilename.Text, DATABASE_EXT));

         oDatasetElement.Attributes.Append(oPathAttr);

         return true;
      }
   }
}
