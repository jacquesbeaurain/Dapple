using System;
using System.Windows.Forms;
using Dapple.Properties;
using System.Drawing;

namespace NewServerTree
{
	internal static class IconKeys
	{
		internal static String AvailableServers = "rootnode";

		internal static String DapRoot = "rootnode-dap";
		internal static String TileRoot = "rootnode-tile";
		internal static String VERoot = "rootnode-ve";
		internal static String WMSRoot = "rootnode-wms";
		internal static String ArcIMSRoot = "rootnode-arcims";

		internal static String OnlineServer = "server-online";
		internal static String OfflineServer = "server-offline";
		internal static String DisabledServer = "server-disabled";
		internal static String PersonalDAPServer = "server-personaldap";
		internal static String ArcIMSService = "folder-arcimsservice";
		internal static String TileSet = "folder-tileset";

		internal static String OpenFolder = "folder-open";
		internal static String ClosedFolder = "folder-closed";

		internal static String ErrorMessage = "message-error";
		internal static String LoadingMessage = "message-loading";
		internal static String InfoMessage = "message-info";

		internal static String DapLayerPrefix = "layer-dap-"; // 13 subtypes
		internal static String DapBrowserMapLayer = "layer-dap-browsermap";
		internal static String TileLayer = "layer-tile";
		internal static String VELayer = "layer-ve";
		internal static String WMSLayer = "layer-wms";
		internal static String ArcIMSLayer = "layer-arcims";

		internal static String MissingImage = "no-image-set";

		internal static String AddDAPServerMenuItem = "menu-add-dap";
		internal static String AddWMSServerMenuItem = "menu-add-wms";
		internal static String AddArcIMSServerMenuItem = "menu-add-arcims";

		internal static String MakeServerFavouriteMenuItem = "menu-server-makefavourite";
		internal static String RefreshServerMenuItem = "menu-server-refresh";
		internal static String EnableServerMenuItem = "menu-server-enable";
		internal static String DisableServerMenuItem = "menu-server-disable";
		internal static String RemoveServerMenuItem = "menu-server-remove";
		internal static String ViewServerPropertiesMenuItem = "menu-server-viewproperties";

		internal static String AddLayerMenuItem = "menu-layer-add";
		internal static String ViewLegendMenuItem = "menu-layer-viewlegend";

		internal static ImageList ImageList;

		static IconKeys()
		{
			ImageList = ConstructImageList();
		}

		private static ImageList ConstructImageList()
		{
			ImageList result = new ImageList();
			result.ColorDepth = ColorDepth.Depth32Bit;

			// --- Any ModelNodes that return invalid values will have this icon ---
#if DEBUG
			result.Images.Add(MissingImage, Resources.exit);
#else
			result.Images.Add(MissingImage, new Bitmap(16, 16));
#endif

			result.Images.Add(AvailableServers, Resources.dapple);

			result.Images.Add(DapRoot, Resources.dap);
			result.Images.Add(TileRoot, Resources.tile);
			result.Images.Add(VERoot, Resources.live);
			result.Images.Add(WMSRoot, Resources.wms);
			result.Images.Add(ArcIMSRoot, Resources.arcims);

			result.Images.Add(OnlineServer, Resources.enserver);
			result.Images.Add(OfflineServer, Resources.offline);
			result.Images.Add(DisabledServer, Resources.disserver);
			result.Images.Add(PersonalDAPServer, Resources.dcat);
			result.Images.Add(ArcIMSService, Resources.layers_top);
			result.Images.Add(TileSet, Resources.tile);

			result.Images.Add(OpenFolder, Resources.folder_open);
			result.Images.Add(ClosedFolder, Resources.folder);

			result.Images.Add(ErrorMessage, Resources.error);
			result.Images.Add(LoadingMessage, Resources.time);
			result.Images.Add(InfoMessage, SystemIcons.Information);

			result.Images.Add(DapLayerPrefix + "arcgis", Resources.dap_arcgis);
			result.Images.Add(DapLayerPrefix + "database", Resources.dap_database);
			result.Images.Add(DapLayerPrefix + "document", Resources.dap_document);
			result.Images.Add(DapLayerPrefix + "generic", Resources.dap_map);
			result.Images.Add(DapLayerPrefix + "grid", Resources.dap_grid);
			result.Images.Add(DapLayerPrefix + "gridsection", Resources.dap_grid);
			result.Images.Add(DapLayerPrefix + "imageserver", Resources.arcims);
			result.Images.Add(DapLayerPrefix + "map", Resources.dap_map);
			result.Images.Add(DapLayerPrefix + "picture", Resources.dap_picture);
			result.Images.Add(DapLayerPrefix + "picturesection", Resources.dap_picture);
			result.Images.Add(DapLayerPrefix + "point", Resources.dap_point);
			result.Images.Add(DapLayerPrefix + "spf", Resources.dap_spf);
			result.Images.Add(DapLayerPrefix + "voxel", Resources.dap_voxel);
			result.Images.Add(DapBrowserMapLayer, Resources.dap_picture);

			result.Images.Add(TileLayer, Resources.layer);
			result.Images.Add(VELayer, Resources.live);
			result.Images.Add(WMSLayer, Resources.layer);
			result.Images.Add(ArcIMSLayer, Resources.layer);

			result.Images.Add(AddDAPServerMenuItem, Resources.addserver);
			result.Images.Add(AddWMSServerMenuItem, Resources.addserver);
			result.Images.Add(AddArcIMSServerMenuItem, Resources.addserver);

			result.Images.Add(MakeServerFavouriteMenuItem, Resources.server_favourite);
			result.Images.Add(RefreshServerMenuItem, Resources.server_refresh);
			result.Images.Add(EnableServerMenuItem, Resources.enserver);
			result.Images.Add(DisableServerMenuItem, Resources.disserver);
			result.Images.Add(RemoveServerMenuItem, Resources.server_remove);
			result.Images.Add(ViewServerPropertiesMenuItem, Resources.properties);

			result.Images.Add(AddLayerMenuItem, Resources.layers_add);
			result.Images.Add(ViewLegendMenuItem, Resources.legend);

			return result;
		}
	}
}