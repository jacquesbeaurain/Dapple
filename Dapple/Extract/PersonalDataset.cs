using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Dapple.LayerGeneration;
using System.Xml;
using System.IO;

namespace Dapple.Extract
{
	public partial class PersonalDataset : DownloadOptions
	{
		public PersonalDataset(DAPQuadLayerBuilder oDAPLayer)
			:base(oDAPLayer)
		{
			InitializeComponent();
		}

		public override DownloadOptions.ExtractSaveResult Save(XmlElement oDatasetElement, string strDestFolder, DownloadSettings.DownloadCoordinateSystem eCS)
		{
			bool blFileExists;

			try
			{
				blFileExists = File.Exists(m_oDAPLayer.LocalFilename);
			}
			catch (Exception)
			{
				blFileExists = false;
			}

			if (blFileExists)
			{
				oDatasetElement.SetAttribute("filename", m_oDAPLayer.LocalFilename);
				oDatasetElement.SetAttribute("type", m_oDAPLayer.DAPType);

				return ExtractSaveResult.Extract;
			}
			else
			{
				if (MessageBox.Show("The local file for dataset " + m_oDAPLayer.Title + " cannot be found. Ignore file and extract other datasets?", "Opening Local Dataset", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
				{
					return ExtractSaveResult.Ignore;
				}
				else
				{
					return ExtractSaveResult.Cancel;
				}
			}
		}
	}
}
