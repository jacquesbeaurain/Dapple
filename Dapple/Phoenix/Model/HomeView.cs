using System;
using System.IO;
using System.Xml;
using Dapple;
using Dapple.LayerGeneration;

namespace NewServerTree
{
	static class HomeView
	{
		#region Constants

		private const string HomeViewFilename = "homeview_2.0.0.dapple";

		#endregion


		#region Synchronization

		private static Object LOCK = new object();

		private delegate bool QueryDelegate(XmlDocument doc, Object data);
		private static bool ExecuteQuery(QueryDelegate query, Object data)
		{
			lock (LOCK)
			{
				XmlDocument doc = new XmlDocument();
				doc.Load(FullPath);

				return query(doc, data);
			}
		}

		private delegate void UpdateDelegate(XmlDocument doc, Object data);
		private static void ExecuteUpdate(UpdateDelegate update, Object data)
		{
			lock (LOCK)
			{
				XmlDocument doc = new XmlDocument();
				doc.Load(FullPath);

				update(doc, data);

				doc.Save(FullPath);
			}
		}

		#endregion


		#region Properties

		internal static string FullPath
		{
			get
			{
				return Path.Combine(Path.Combine(MainForm.UserPath, MainForm.Settings.ConfigPath), HomeViewFilename);
			}
		}

		private static string DefaultViewFullPath
		{
			get
			{
				return Path.Combine(Path.Combine(MainForm.DirectoryPath, "Data"), "default.dapple");
			}
		}

		#endregion


		#region Public Methods

		internal static void CreateDefault()
		{
			if (!File.Exists(HomeView.FullPath))
			{
				File.Copy(DefaultViewFullPath, HomeView.FullPath);
			}
		}

		internal static void AddServer(ServerUri oUri)
		{
			ExecuteUpdate(new UpdateDelegate(Branch_AddServer), oUri);
		}

		internal static void SetServerEnabled(ServerUri oUri, bool blEnabled)
		{
			ExecuteUpdate(new UpdateDelegate(Branch_SetServerEnabled), new Object[] { oUri, blEnabled });
		}

		internal static void RemoveServer(ServerUri oUri)
		{
			ExecuteUpdate(new UpdateDelegate(Branch_RemoveServer), oUri);
		}

		internal static void SetFavourite(ServerUri oUri)
		{
			ExecuteUpdate(new UpdateDelegate(Branch_SetFavourite), oUri);
		}

		internal static void ClearFavourite()
		{
			ExecuteUpdate(new UpdateDelegate(Branch_SetFavourite), null);
		}

		internal static bool ContainsServer(ServerUri oUri)
		{
			return ExecuteQuery(new QueryDelegate(Branch_ContainsServer), oUri);
		}

		#endregion


		#region Helper Methods

		#region Branchers

		private static void Branch_AddServer(XmlDocument doc, Object uri)
		{
			if (uri is ArcIMSServerUri)
			{
				AddArcIMSServer(doc, uri as ArcIMSServerUri);
			}
			else if (uri is DapServerUri)
			{
				AddDAPServer(doc, uri as DapServerUri);
			}
			else if (uri is WMSServerUri)
			{
				AddWMSServer(doc, uri as WMSServerUri);
			}
			else
			{
				throw new NotImplementedException();
			}
		}

		private static void Branch_SetServerEnabled(XmlDocument doc, Object parameters)
		{
			ServerUri uri = (ServerUri)(parameters as Object[])[0];
			bool blValue = (bool)(parameters as Object[])[1];

			if (uri is ArcIMSServerUri)
			{
				SetArcIMSServerEnabled(doc, uri as ArcIMSServerUri, blValue);
			}
			else if (uri is DapServerUri)
			{
				SetDAPServerEnabled(doc, uri as DapServerUri, blValue);
			}
			else if (uri is WMSServerUri)
			{
				SetWMSServerEnabled(doc, uri as WMSServerUri, blValue);
			}
			else
			{
				throw new NotImplementedException();
			}
		}

		private static void Branch_RemoveServer(XmlDocument doc, Object uri)
		{
			if (uri is ArcIMSServerUri)
			{
				RemoveArcIMSServer(doc, uri as ArcIMSServerUri);
			}
			else if (uri is DapServerUri)
			{
				RemoveDAPServer(doc, uri as DapServerUri);
			}
			else if (uri is WMSServerUri)
			{
				RemoveWMSServer(doc, uri as WMSServerUri);
			}
			else
			{
				throw new NotImplementedException();
			}
		}

		private static void Branch_SetFavourite(XmlDocument doc, Object uri)
		{
			if (uri == null)
			{
				ClearFavourite(doc);
			}
			if (uri is ArcIMSServerUri || uri is DapServerUri || uri is WMSServerUri)
			{
				SetFavourite(doc, uri.ToString());
			}
			else
			{
				throw new NotImplementedException();
			}
		}

		private static bool Branch_ContainsServer(XmlDocument doc, Object uri)
		{
			if (uri is ArcIMSServerUri)
			{
				return ContainsArcIMSServer(doc, uri as ArcIMSServerUri);
			}
			else if (uri is DapServerUri)
			{
				return ContainsDAPServer(doc, uri as DapServerUri);
			}
			else if (uri is WMSServerUri)
			{
				return ContainsWMSServer(doc, uri as WMSServerUri);
			}
			else
			{
				throw new NotImplementedException();
			}
		}

		#endregion


		#region AddServer

		private static void AddArcIMSServer(XmlDocument doc, ArcIMSServerUri uri)
		{
			XmlElement oWMSRoot = doc.SelectSingleNode("/dappleview/servers/builderentry/builderdirectory[@specialcontainer=\"WMSServers\"]") as XmlElement;
			XmlElement oBuilderEntry = doc.CreateElement("builderentry");
			oWMSRoot.AppendChild(oBuilderEntry);
			XmlElement oDapCatalog = doc.CreateElement("arcimscatalog");
			oDapCatalog.SetAttribute("capabilitiesurl", uri.ToString());
			oDapCatalog.SetAttribute("enabled", "true");
			oBuilderEntry.AppendChild(oDapCatalog);
		}

		private static void AddDAPServer(XmlDocument doc, DapServerUri uri)
		{
			XmlElement oDAPRoot = doc.SelectSingleNode("/dappleview/servers/builderentry/builderdirectory[@specialcontainer=\"DAPServers\"]") as XmlElement;
			XmlElement oBuilderEntry = doc.CreateElement("builderentry");
			oDAPRoot.AppendChild(oBuilderEntry);
			XmlElement oDapCatalog = doc.CreateElement("dapcatalog");
			oDapCatalog.SetAttribute("url", uri.ToString());
			oDapCatalog.SetAttribute("enabled", "true");
			oBuilderEntry.AppendChild(oDapCatalog);
		}

		private static void AddWMSServer(XmlDocument doc, WMSServerUri uri)
		{
			XmlElement oWMSRoot = doc.SelectSingleNode("/dappleview/servers/builderentry/builderdirectory[@specialcontainer=\"WMSServers\"]") as XmlElement;
			XmlElement oBuilderEntry = doc.CreateElement("builderentry");
			oWMSRoot.AppendChild(oBuilderEntry);
			XmlElement oDapCatalog = doc.CreateElement("wmscatalog");
			oDapCatalog.SetAttribute("capabilitiesurl", uri.ToString());
			oDapCatalog.SetAttribute("enabled", "true");
			oBuilderEntry.AppendChild(oDapCatalog);
		}

		#endregion


		#region SetServerEnabled

		private static void SetArcIMSServerEnabled(XmlDocument doc, ArcIMSServerUri uri, bool blValue)
		{
			foreach (XmlElement oArcIMSCatalog in doc.SelectNodes("/dappleview/servers/builderentry/builderdirectory/builderentry/arcimscatalog"))
			{
				if (new ArcIMSServerUri(oArcIMSCatalog.GetAttribute("capabilitiesurl")).ToString().Equals(uri.ToString()))
				{
					oArcIMSCatalog.SetAttribute("enabled", blValue.ToString());
				}
			}
		}

		private static void SetDAPServerEnabled(XmlDocument doc, DapServerUri uri, bool blValue)
		{
			foreach (XmlElement oDapCatalog in doc.SelectNodes("/dappleview/servers/builderentry/builderdirectory/builderentry/dapcatalog"))
			{
				if (new DapServerUri(oDapCatalog.GetAttribute("url")).ToString().Equals(uri.ToString()))
				{
					oDapCatalog.SetAttribute("enabled", blValue.ToString());
				}
			}
		}

		private static void SetWMSServerEnabled(XmlDocument doc, WMSServerUri uri, bool blValue)
		{
			foreach (XmlElement oWmsCatalog in doc.SelectNodes("/dappleview/servers/builderentry/builderdirectory/builderentry/wmscatalog"))
			{
				if (new WMSServerUri(oWmsCatalog.GetAttribute("capabilitiesurl")).ToString().Equals(uri.ToString()))
				{
					oWmsCatalog.SetAttribute("enabled", blValue.ToString());
				}
			}
		}

		#endregion


		#region RemoveServer

		private static void RemoveArcIMSServer(XmlDocument doc, ArcIMSServerUri uri)
		{
			foreach (XmlElement oDapCatalog in doc.SelectNodes("/dappleview/servers/builderentry/builderdirectory/builderentry/arcimscatalog"))
			{
				if (new ArcIMSServerUri(oDapCatalog.GetAttribute("capabilitiesurl")).ToString().Equals(uri.ToString()))
				{
					oDapCatalog.ParentNode.ParentNode.RemoveChild(oDapCatalog.ParentNode);
				}
			}
		}

		private static void RemoveDAPServer(XmlDocument doc, DapServerUri uri)
		{
			foreach (XmlElement oDapCatalog in doc.SelectNodes("/dappleview/servers/builderentry/builderdirectory/builderentry/dapcatalog"))
			{
				if (new DapServerUri(oDapCatalog.GetAttribute("url")).ToString().Equals(uri.ToString()))
				{
					oDapCatalog.ParentNode.ParentNode.RemoveChild(oDapCatalog.ParentNode);
				}
			}
		}

		private static void RemoveWMSServer(XmlDocument doc, WMSServerUri uri)
		{
			foreach (XmlElement oDapCatalog in doc.SelectNodes("/dappleview/servers/builderentry/builderdirectory/builderentry/wmscatalog"))
			{
				if (new WMSServerUri(oDapCatalog.GetAttribute("capabilitiesurl")).ToString().Equals(uri.ToString()))
				{
					oDapCatalog.ParentNode.ParentNode.RemoveChild(oDapCatalog.ParentNode);
				}
			}
		}

		#endregion


		#region SetFavourite

		private static void ClearFavourite(XmlDocument doc)
		{
			SetFavourite(doc, String.Empty);
		}

		private static void SetFavourite(XmlDocument doc, String uri)
		{
			XmlElement oDocRoot = doc.SelectSingleNode("/dappleview") as XmlElement;
			oDocRoot.SetAttribute("favouriteserverurl", uri);
		}

		#endregion


		#region Contains

		private static bool ContainsArcIMSServer(XmlDocument doc, ArcIMSServerUri uri)
		{
			foreach (XmlAttribute oAttr in doc.SelectNodes("//arcimscatalog/@capabilitiesurl"))
			{
				if (new ArcIMSServerUri(oAttr.Value).ToString().Equals(uri.ToString()))
				{
					return true;
				}
			}

			return false;
		}

		private static bool ContainsDAPServer(XmlDocument doc, DapServerUri uri)
		{
			foreach (XmlAttribute oAttr in doc.SelectNodes("//dapcatalog/@url"))
			{
				if (new DapServerUri(oAttr.Value).ToString().Equals(uri.ToString()))
				{
					return true;
				}
			}

			return false;
		}

		private static bool ContainsWMSServer(XmlDocument doc, WMSServerUri uri)
		{
			foreach (XmlAttribute oAttr in doc.SelectNodes("//wmscatalog/@capabilitiesurl"))
			{
				if (new WMSServerUri(oAttr.Value).ToString().Equals(uri.ToString()))
				{
					return true;
				}
			}

			return false;
		}

		#endregion

		#endregion
	}
}
