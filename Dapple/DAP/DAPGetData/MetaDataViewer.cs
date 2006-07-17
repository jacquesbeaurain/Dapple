using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Geosoft.GX.DAPGetData
{
    public partial class MetaDataViewer : Form
    {
        public MetaDataViewer(string url)
        {
            InitializeComponent();
            this.Icon = new System.Drawing.Icon(@"app.ico");
            this.webBrowser1.Url = new Uri(url);
        }

    }
}