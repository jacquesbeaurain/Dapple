using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;

using WorldWind;
using WorldWind.Net;
using WorldWind.Renderable;
using WorldWind.Net.Wms;

using WorldWind.PluginEngine;

using Geosoft.DotNetTools;
using System.Collections;

namespace Dapple.LayerGeneration
{	internal class WMSServerBuilder : ServerBuilder
	{
		string m_strCapabilitiesFilePath;
		WMSList m_oList;
		bool m_blLoadingPending = true;

		internal WMSServerBuilder(IBuilder parent, WMSServerUri oUri, string CapabilitiesFilePath, bool blEnabled)
			: base(oUri.ToBaseUri(), parent, oUri, blEnabled)
		{
			m_strCapabilitiesFilePath = CapabilitiesFilePath;
		}

		[System.ComponentModel.Browsable(false)]
		internal bool LoadingPending
		{
			get { return m_blLoadingPending; }
			set { m_blLoadingPending = value; }
		}

		[System.ComponentModel.Browsable(false)]
		internal string CapabilitiesFilePath
		{
			get
			{
				return m_strCapabilitiesFilePath;
			}
			set
			{
				m_strCapabilitiesFilePath = value;
			}
		}

		public override bool SupportsMetaData
		{
			get
			{
				return File.Exists(m_strCapabilitiesFilePath);
			}
		}

		public override XmlNode GetMetaData(XmlDocument oDoc)
		{
			XmlDocument responseDoc = new XmlDocument();
			XmlReaderSettings oSettings = new System.Xml.XmlReaderSettings();
			oSettings.IgnoreWhitespace = true;
			oSettings.ProhibitDtd = false;
			oSettings.XmlResolver = null;
			oSettings.ValidationType = ValidationType.None;
			using (XmlReader oResponseXmlStream = XmlReader.Create(m_strCapabilitiesFilePath, oSettings))
			{
				responseDoc.Load(oResponseXmlStream);
			}
			XmlNode oNode = responseDoc.DocumentElement;
			XmlNode newNode = oDoc.CreateElement(oNode.Name);
			newNode.InnerXml = oNode.InnerXml;
			return newNode;
		}

		[System.ComponentModel.Browsable(false)]
		public override string StyleSheetName
		{
			get
			{
				return "wms_cap_meta.xslt";
			}
		}

		[System.ComponentModel.Browsable(false)]
		internal WMSList List
		{
			get { return m_oList; }
			set { m_oList = value; }
		}

		[System.ComponentModel.Browsable(false)]
		internal override System.Drawing.Icon Icon
		{
			get { return Dapple.Properties.Resources.wms; }
		}

		internal void Clear()
		{
			m_colChildren.Clear();
			m_colSublist.Clear();
		}
	}
}
