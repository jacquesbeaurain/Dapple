using System;
using System.Windows.Forms;
using Dapple.Properties;
using System.Drawing;

namespace NewServerTree
{
	public static class IconKeys
	{
		public static String AvailableServers = "rootnode";

		public static String DapRoot = "rootnode-dap";
		public static String TileRoot = "rootnode-tile";
		public static String VERoot = "rootnode-ve";
		public static String WMSRoot = "rootnode-wms";
		public static String ArcIMSRoot = "rootnode-arcims";

		public static String OnlineServer = "server-online";
		public static String OfflineServer = "server-offline";
		public static String DisabledServer = "server-disabled";
		public static String PersonalDAPServer = "server-personaldap";
		public static String ArcIMSService = "folder-arcimsservice";
		public static String TileSet = "folder-tileset";

		public static String OpenFolder = "folder-open";
		public static String ClosedFolder = "folder-closed";

		public static String ErrorMessage = "message-error";
		public static String LoadingMessage = "message-loading";
		public static String InfoMessage = "message-info";

		public static String DapLayerPrefix = "layer-dap-"; // 13 subtypes
		public static String DapBrowserMapLayer = "layer-dap-browsermap";
		public static String TileLayer = "layer-tile";
		public static String VELayer = "layer-ve";
		public static String WMSLayer = "layer-wms";
		public static String ArcIMSLayer = "layer-arcims";

		public static String MissingImage = "no-image-set";

		public static String AddDAPServerMenuItem = "menu-add-dap";
		public static String AddWMSServerMenuItem = "menu-add-wms";
		public static String AddArcIMSServerMenuItem = "menu-add-arcims";

		public static String MakeServerFavouriteMenuItem = "menu-server-makefavourite";
		public static String RefreshServerMenuItem = "menu-server-refresh";
		public static String EnableServerMenuItem = "menu-server-enable";
		public static String DisableServerMenuItem = "menu-server-disable";
		public static String RemoveServerMenuItem = "menu-server-remove";
		public static String ViewServerPropertiesMenuItem = "menu-server-viewproperties";

		public static String AddLayerMenuItem = "menu-layer-add";
		public static String ViewLegendMenuItem = "menu-layer-viewlegend";

		public static ImageList ImageList;

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
			result.Images.Add(PersonalDAPServer, Resources.dap);
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