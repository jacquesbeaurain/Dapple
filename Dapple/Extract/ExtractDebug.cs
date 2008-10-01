using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.IO;

namespace Dapple.Extract
{
	public partial class ExtractDebug : Form
	{
		private XmlDocument m_oExtractDoc;
		private readonly string m_strFilename = Path.Combine(Path.GetTempPath(), Path.ChangeExtension(Path.GetRandomFileName(), ".xml"));

		public ExtractDebug() : this(null)
		{
		}

		public ExtractDebug(XmlDocument oExtractDoc)
		{
			InitializeComponent();
			m_oExtractDoc = oExtractDoc;
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			if (m_oExtractDoc != null)
			{
				m_oExtractDoc.Save(m_strFilename);
				c_wbExtract.Url = new Uri(m_strFilename);
			}
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);

			if (File.Exists(m_strFilename))
			{
				try
				{
					File.Delete(m_strFilename);
				}
				catch { } 
			}
		}

		private void c_bExecute_Click(object sender, EventArgs e)
		{
			int result = MainForm.MontajInterface.Download(m_oExtractDoc.OuterXml);

			MessageBox.Show("Extraction Execution Complete", "Extract operation returned " + result + ".");
		}

		private void c_bDone_Click(object sender, EventArgs e)
		{
			this.Close();
		}
	}
}
