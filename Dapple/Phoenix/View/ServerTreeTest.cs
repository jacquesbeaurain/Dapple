using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Dapple.LayerGeneration;

namespace NewServerTree.View
{
	public partial class ServerTreeTest : Form
	{
		DappleModel m_oModel;
		List<String> m_oWMSUris = new List<String>(new String[] {
			"http://gdr.ess.nrcan.gc.ca/wmsconnector/com.esri.wms.Esrimap/gdr_e",
			"http://atlas.gc.ca/cgi-bin/atlaswms_en?VERSION=1.1.1",
			"http://apps1.gdr.nrcan.gc.ca/cgi-bin/canmin_en-ca_ows",
			"http://www.ga.gov.au/bin/getmap.pl",
			"http://apps1.gdr.nrcan.gc.ca/cgi-bin/worldmin_en-ca_ows",
			"http://gisdata.usgs.net/servlet/com.esri.wms.Esrimap",
			"http://maps.customweather.com/image",
			"http://cgkn.net/cgi-bin/cgkn_wms",
			"http://wms.jpl.nasa.gov/wms.cgi"
		});

		public ServerTreeTest()
		{
			InitializeComponent();

			m_oModel = new DappleModel();
			m_oModel.Load();

			serverTree1.Attach(m_oModel);
			serverTree2.Attach(m_oModel);
		}

		public static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new ServerTreeTest());
		}

		private void c_bAddWMS_Click(object sender, EventArgs e)
		{
			String strUrl = m_oWMSUris[0];
			m_oWMSUris.RemoveAt(0);
			m_oModel.AddWMSServer(new WMSServerUri(strUrl));
		}
	}
}
