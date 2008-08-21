using System;
using System.Globalization;
using System.IO;
using System.Xml;
using GED.App.UI.Controls;
using GED.WebService;
using GED.Core;
using GED.App.Properties;
using Geosoft.Dap.Common;
using GEDCore;

namespace GED.App.Core
{
	/// <summary>
	/// Static methods to generate root KML files, which Google Earth opens and contain links to each of the
	/// child tile KML files.
	/// </summary>
	static class RootKmlFiles
	{
		#region Public Methods

		public static String CreateKmlFile(SearchResult oResult)
		{
			switch (Settings.Default.KmlFormat)
			{
				case RootKmlFormat.Tiled:
					return CreateTileKmlFile(oResult);
				case RootKmlFormat.SingleImage:
					return CreateSingleImageKmlFile(oResult);
				default:
					throw new ApplicationException("Missing case statement for enum value '" + Settings.Default.KmlFormat.ToString() + "'");
			}
		}

		/// <summary>
		/// Create a root KML file for tiled KML for a given search result.
		/// </summary>
		/// <param name="oResult">The search result to create the root KML file for.</param>
		/// <returns>The filename of the created file.</returns>
		private static String CreateTileKmlFile(SearchResult oResult)
		{
			#region //Input checking
			if (oResult == null) throw new ArgumentNullException("oResult");
			if (!oResult.ServerType.Equals("DAP", StringComparison.InvariantCultureIgnoreCase))
				throw new ArgumentNullException("Only DAP servers are currently supported");
			#endregion

			String strCacheFilename = Path.Combine(CacheUtils.CacheRoot, GetRootKmlFilename(oResult));

			{
				Directory.CreateDirectory(Path.GetDirectoryName(strCacheFilename));
				XmlWriter oWriter = XmlWriter.Create(strCacheFilename);

				oWriter.WriteStartElement("kml", "http://www.opengis.net/kml/2.2");
				oWriter.WriteStartElement("Document");

				oWriter.WriteElementString("description", oResult.Description);
				oWriter.WriteStartElement("Camera");
				oWriter.WriteElementString("longitude", ((oResult.MinX + oResult.MaxX) / 2.0).ToString(CultureInfo.InvariantCulture));
				oWriter.WriteElementString("latitude", ((oResult.MinY + oResult.MaxY) / 2.0).ToString(CultureInfo.InvariantCulture));
				oWriter.WriteElementString("altitude", Math.Min(6500000, (oResult.MaxX - oResult.MinX) * 50 * 1000).ToString(CultureInfo.InvariantCulture));
				oWriter.WriteElementString("heading", "0");
				oWriter.WriteElementString("tilt", "0");
				oWriter.WriteElementString("roll", "0");
				oWriter.WriteEndElement(); // </Camera>

				for (int iColumn = 0; iColumn < 2; iColumn++)
				{
					TileInfo oTile = new TileInfo(0, iColumn, 0);

					Uri oTileKmlUrl = ContractHelper.BindTileKmlUriTemplate(ControlPanel.BaseAddress, oResult.ServerType, oResult.ServerUrl, oResult.GetAttribute("datasetname"), oTile, oResult.Bounds);

					oWriter.WriteStartElement("NetworkLink");
					oWriter.WriteElementString("name", "Some Hemisphere");
					oWriter.WriteElementString("open", "1");
					oWriter.WriteStartElement("Region");
					oWriter.WriteStartElement("LatLonAltBox");
					oWriter.WriteElementString("north", "90");
					oWriter.WriteElementString("south", "-90");
					oWriter.WriteElementString("east", (iColumn == 0 ? 0 : 180).ToString(CultureInfo.InvariantCulture));
					oWriter.WriteElementString("west", (iColumn == 0 ? -180 : 0).ToString(CultureInfo.InvariantCulture));
					oWriter.WriteEndElement(); // </LatLonAltBox>
					oWriter.WriteStartElement("Lod");
					oWriter.WriteElementString("minLodPixels", "128");
					oWriter.WriteElementString("maxLodPixels", "-1");
					oWriter.WriteEndElement(); // </Lod>
					oWriter.WriteEndElement(); // </Region>
					oWriter.WriteStartElement("Link");
					oWriter.WriteElementString("href", oTileKmlUrl.ToString());
					oWriter.WriteElementString("viewRefreshMode", "onRegion");
					oWriter.WriteEndElement(); // </Link>
					oWriter.WriteEndElement(); // </NetworkLink>
				}

				oWriter.WriteEndElement(); // </Document>
				oWriter.WriteEndElement(); // </kml>
				oWriter.Close();
			}

			return strCacheFilename;
		}

		/// <summary>
		/// Create a root KML file for tiled KML for a given search result.
		/// </summary>
		/// <param name="oResult">The search result to create the root KML file for.</param>
		/// <returns>The filename of the created file.</returns>
		private static String CreateSingleImageKmlFile(SearchResult oResult)
		{
			#region //Input checking
			if (oResult == null) throw new ArgumentNullException("oResult");
			if (!oResult.ServerType.Equals("DAP", StringComparison.InvariantCultureIgnoreCase))
				throw new ArgumentNullException("Only DAP servers are currently supported");
			#endregion

			String strCacheFilename = Path.Combine(CacheUtils.CacheRoot, GetRootKmlFilename(oResult));

			{
				Directory.CreateDirectory(Path.GetDirectoryName(strCacheFilename));
				XmlWriter oWriter = XmlWriter.Create(strCacheFilename);

				oWriter.WriteStartElement("kml", "http://www.opengis.net/kml/2.2");
				oWriter.WriteStartElement("Document");

				oWriter.WriteElementString("description", oResult.Description);
				oWriter.WriteStartElement("Camera");
				oWriter.WriteElementString("longitude", ((oResult.MinX + oResult.MaxX) / 2.0).ToString(CultureInfo.InvariantCulture));
				oWriter.WriteElementString("latitude", ((oResult.MinY + oResult.MaxY) / 2.0).ToString(CultureInfo.InvariantCulture));
				oWriter.WriteElementString("altitude", Math.Min(6500000, (oResult.MaxX - oResult.MinX) * 50 * 1000).ToString(CultureInfo.InvariantCulture));
				oWriter.WriteElementString("heading", "0");
				oWriter.WriteElementString("tilt", "0");
				oWriter.WriteElementString("roll", "0");
				oWriter.WriteEndElement(); // </Camera>

				oWriter.WriteStartElement("GroundOverlay");
				oWriter.WriteElementString("name", oResult.Title);
				oWriter.WriteStartElement("Icon");
				oWriter.WriteElementString("href", ContractHelper.PartialBindSingleImageUriTemplate(ControlPanel.BaseAddress, oResult.ServerType, oResult.ServerUrl, oResult.GetAttribute("datasetname"), oResult.Bounds).ToString());
				oWriter.WriteElementString("viewRefreshMode", "onStop");
				oWriter.WriteElementString("viewRefreshTime", "0.5");
				oWriter.WriteElementString("viewBoundScale", "1.2");
				oWriter.WriteElementString("viewFormat", "BBOX=[bboxWest],[bboxSouth],[bboxEast],[bboxNorth]");
				oWriter.WriteEndElement(); // </Icon>
				oWriter.WriteStartElement("LatLonBox");
				oWriter.WriteElementString("north", Math.Min(90.0, oResult.MaxY).ToString(CultureInfo.InvariantCulture));
				oWriter.WriteElementString("south", Math.Max(-90.0, oResult.MinY).ToString(CultureInfo.InvariantCulture));
				oWriter.WriteElementString("east", Math.Min(180.0, oResult.MaxX).ToString(CultureInfo.InvariantCulture));
				oWriter.WriteElementString("west", Math.Max(-180.0, oResult.MinX).ToString(CultureInfo.InvariantCulture));
				oWriter.WriteEndElement(); // </LatLonBox>
				oWriter.WriteEndElement(); // </GroundOverlay>

				oWriter.WriteEndElement(); // </Document>
				oWriter.WriteEndElement(); // </kml>
				oWriter.Close();
			}

			return strCacheFilename;
		}

		#endregion

		#region Helper Methods

		private static String GetRootKmlFilename(SearchResult oResult)
		{
			return String.Format(CultureInfo.InvariantCulture, "Root KML Files/{0}.kml",
				CacheUtils.CreateValidFilename(oResult.Title)
			);
		}

		#endregion
	}

	public enum RootKmlFormat
	{
		Tiled,
		SingleImage
	}
}
