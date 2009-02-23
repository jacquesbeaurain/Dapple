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
			String strFilename = String.Empty;


			bool openANewMap;

			if (MainForm.MontajInterface.HostHasOpenMap())
			{
				string strSrcCoordinateSystem = MainForm.MontajInterface.GetProjection(m_oDAPLayer.ServerURL, m_oDAPLayer.DatasetName);
				if (string.IsNullOrEmpty(strSrcCoordinateSystem))
					return ExtractSaveResult.Ignore;
				double dMinX, dMinY, dMaxX, dMaxY;
				if (!MainForm.MontajInterface.GetExtents(m_oDAPLayer.ServerURL, m_oDAPLayer.DatasetName, out dMaxX, out dMinX, out dMaxY, out dMinY))
					return ExtractSaveResult.Ignore;

				openANewMap = !IntersectMap(ref dMinX, ref dMinY, ref dMaxX, ref dMaxY, strSrcCoordinateSystem);
			}
			else
			{
				openANewMap = true;
			}


			try
			{
				strFilename = m_oDAPLayer.LocalFilename;
				String strStrippedFilename = StripQualifiers(strFilename);

				blFileExists = File.Exists(strStrippedFilename);
			}
			catch (Exception)
			{
				blFileExists = false;
			}

			if (blFileExists)
			{
				oDatasetElement.SetAttribute("filename", strFilename);
				oDatasetElement.SetAttribute("type", m_oDAPLayer.DAPType);
				oDatasetElement.SetAttribute("id", m_oDAPLayer.DatasetName);
				oDatasetElement.SetAttribute("new_map", openANewMap.ToString());

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

		/// <summary>
		/// Takes a filename with Geosoft qualifications and removes the qualifications.
		/// </summary>
		/// <param name="strFilename">The filename to strip.</param>
		/// <returns>The stripped filename.</returns>
		private String StripQualifiers(String strFilename)
		{
			// --- Find last open bracket ---

			int iLastBracketPos = strFilename.LastIndexOf('(');


			// --- No open bracket means no qualifiers ---

			if (iLastBracketPos == -1)
			{
				return strFilename;
			}


			// --- Get potential qualifier string ---

			String strQualifyString = strFilename.Substring(iLastBracketPos + 1);


			// --- Valid qualifiers contain no periods or backslashes ---

			if (strQualifyString.IndexOf('.') != -1 || strQualifyString.IndexOf('\\') != -1)
			{
				return strFilename;
			}


			// --- We've got a valid qualifier, so strip it ---

			return strFilename.Substring(0, iLastBracketPos);
		}
	}
}
