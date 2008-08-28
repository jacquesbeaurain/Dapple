using System;
using System.Globalization;
using System.IO;
using System.Xml;
using GED.Core;
using Geosoft.Dap.Common;

namespace GED.WebService
{
	class TileKmlGenerator
	{
		public static MemoryStream GenerateTileKml(String strServerType, String strServer, String strLayer, TileInfo oTile, BoundingBox oLayerBounds)
		{
			BoundingBox oBoundsForThisFile = oTile.Bounds;

			MemoryStream result = new MemoryStream();
			XmlWriter oOutputWriter = XmlWriter.Create(result);
			oOutputWriter.WriteStartElement("kml", "http://www.opengis.net/kml/2.2");
			oOutputWriter.WriteStartElement("Document");

			oOutputWriter.WriteElementString("name", String.Format(CultureInfo.CurrentCulture, "Level {0}, Column {1}, Row {2}", oTile.Level, oTile.Column, oTile.Row));
			oOutputWriter.WriteStartElement("Region");
			oOutputWriter.WriteStartElement("LatLonAltBox");
			oOutputWriter.WriteElementString("north", oBoundsForThisFile.MaxY.ToString(CultureInfo.InvariantCulture));
			oOutputWriter.WriteElementString("south", oBoundsForThisFile.MinY.ToString(CultureInfo.InvariantCulture));
			oOutputWriter.WriteElementString("east", oBoundsForThisFile.MaxX.ToString(CultureInfo.InvariantCulture));
			oOutputWriter.WriteElementString("west", oBoundsForThisFile.MinX.ToString(CultureInfo.InvariantCulture));
			oOutputWriter.WriteEndElement(); //LatLonAltBox
			oOutputWriter.WriteStartElement("Lod");
			oOutputWriter.WriteElementString("minLodPixels", "256");
			oOutputWriter.WriteElementString("maxLodPixels", "-1");
			oOutputWriter.WriteEndElement(); //Lod
			oOutputWriter.WriteEndElement(); //Region

			bool blIntersecion = false;
			for (int iDRow = 0; iDRow <= 1; iDRow++)
			{
				for (int iDCol = 0; iDCol <= 1; iDCol++)
				{
					TileInfo oSubTile = new TileInfo(oTile.Level + 1, oTile.Column * 2 + iDCol, oTile.Row * 2 + iDRow);
					BoundingBox oBoundsForSubTile = oSubTile.Bounds;

					if (oBoundsForSubTile.Intersects(oLayerBounds))
					{
						blIntersecion = true;

						oOutputWriter.WriteStartElement("NetworkLink");
						oOutputWriter.WriteElementString("name", String.Format(CultureInfo.InvariantCulture, "{0}{1} SubTile", iDRow == 0 ? "S" : "N", iDCol == 0 ? "W" : "E"));
						oOutputWriter.WriteElementString("open", "1");
						oOutputWriter.WriteStartElement("Region");
						oOutputWriter.WriteStartElement("LatLonAltBox");
						oOutputWriter.WriteElementString("north", oBoundsForSubTile.MaxY.ToString(CultureInfo.InvariantCulture));
						oOutputWriter.WriteElementString("south", oBoundsForSubTile.MinY.ToString(CultureInfo.InvariantCulture));
						oOutputWriter.WriteElementString("east", oBoundsForSubTile.MaxX.ToString(CultureInfo.InvariantCulture));
						oOutputWriter.WriteElementString("west", oBoundsForSubTile.MinX.ToString(CultureInfo.InvariantCulture));
						oOutputWriter.WriteEndElement(); //LatLonAltBox
						oOutputWriter.WriteStartElement("Lod");
						oOutputWriter.WriteElementString("minLodPixels", "256");
						oOutputWriter.WriteElementString("maxLodPixels", "-1");
						oOutputWriter.WriteEndElement(); //Lod
						oOutputWriter.WriteEndElement(); //Region
						oOutputWriter.WriteStartElement("Link");
						oOutputWriter.WriteElementString("href", ContractHelper.BindTileKmlUriTemplate(ControlPanel.BaseAddress, strServerType, strServer, strLayer, oSubTile, oLayerBounds).ToString());
						oOutputWriter.WriteElementString("viewRefreshMode", "onRegion");
						oOutputWriter.WriteEndElement(); //Link
						oOutputWriter.WriteEndElement(); //NetworkLink
					}
				}
			}

			if (blIntersecion)
			{
				oOutputWriter.WriteStartElement("GroundOverlay");
				oOutputWriter.WriteElementString("drawOrder", (oTile.Level + 1).ToString(CultureInfo.InvariantCulture));
				oOutputWriter.WriteStartElement("Icon");
				oOutputWriter.WriteElementString("href", ContractHelper.BindImageCacheUriTemplate(ControlPanel.BaseAddress, strServerType, strServer, strLayer, oTile).ToString());
				oOutputWriter.WriteEndElement(); //Icon
				oOutputWriter.WriteStartElement("LatLonAltBox");
				oOutputWriter.WriteElementString("north", oBoundsForThisFile.MaxY.ToString(CultureInfo.InvariantCulture));
				oOutputWriter.WriteElementString("south", oBoundsForThisFile.MinY.ToString(CultureInfo.InvariantCulture));
				oOutputWriter.WriteElementString("east", oBoundsForThisFile.MaxX.ToString(CultureInfo.InvariantCulture));
				oOutputWriter.WriteElementString("west", oBoundsForThisFile.MinX.ToString(CultureInfo.InvariantCulture));
				oOutputWriter.WriteEndElement(); //LatLonAltBox
				oOutputWriter.WriteEndElement(); //GroundOverlay
			}

			oOutputWriter.WriteEndElement(); //Document
			oOutputWriter.WriteEndElement(); //kml
			oOutputWriter.Close();

			result.Seek(0, SeekOrigin.Begin);
			return result;
		}
	}
}
