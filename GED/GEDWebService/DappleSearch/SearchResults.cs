using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Geosoft.Dap.Common;
using System.IO;
using System.Xml;

namespace GED.WebService.DappleSearch
{
	public class SearchResults
	{
		public string Version { get; private set; }
		public string Handle { get; private set; }
		public int TotalResults { get; private set; }
		public int Count { get { return results.Count; } }
		public int Offset { get; private set; }
		public Layer this[int index] { get { return results[index]; } }

		private readonly List<Layer> results = new List<Layer>();

		internal SearchResults(string version, string handle, int offset, int totalResults)
		{
			Version = version;
			Handle = handle;
			Offset = offset;
			TotalResults = totalResults;
		}

		internal void AddResult(Layer result)
		{
			results.Add(result);
		}
	}

	static partial class Encoder
	{
		public static Stream SearchResults(SearchResults input)
		{
			MemoryStream buffer = new MemoryStream();
			XmlWriter output = XmlWriter.Create(buffer);

			output.WriteStartElement("geosoft_xml");
			output.WriteStartElement("search_result");
			output.WriteAttributeString("version", input.Version);
			output.WriteAttributeString("handle", input.Handle);
			output.WriteAttributeString("offset", input.Offset.ToString());
			output.WriteAttributeString("totalcount", input.TotalResults.ToString());

			if (input.Count > 0)
			{
				output.WriteAttributeString("count", input.Count.ToString());

				output.WriteStartElement("layers");
				for (int count = 0; count < input.Count; count++)
					Encoder.Layer(input[count], output);
				output.WriteEndElement();
			}

			output.WriteEndElement();

			output.Close();

			buffer.Seek(0, SeekOrigin.Begin);
			return buffer;
		}

		private static void Layer(Layer l, XmlWriter output)
		{
			output.WriteStartElement("layer");
			output.WriteAttributeString("rankingscore", ((UInt16)(l.Relevance * UInt16.MaxValue)).ToString());

			output.WriteStartElement("common");
			output.WriteAttributeString("servertitle", l.ServerTitle);
			output.WriteAttributeString("layertitle", l.LayerTitle);
			output.WriteAttributeString("description", l.ServerDescription);
			output.WriteAttributeString("obaselayerid", l.DappleSearchLayerID.ToString());
			output.WriteAttributeString("url", l.Url);
			output.WriteAttributeString("type", l.Type);
			output.WriteAttributeString("minx", l.BoundingBox.MinX.ToString());
			output.WriteAttributeString("miny", l.BoundingBox.MinY.ToString());
			output.WriteAttributeString("maxx", l.BoundingBox.MaxX.ToString());
			output.WriteAttributeString("maxy", l.BoundingBox.MaxY.ToString());
			output.WriteEndElement();

			if (l is DAPLayer)
				Layer(l as DAPLayer, output);
			else
				throw new NotImplementedException();

			output.WriteEndElement();
		}

		private static void Layer(DAPLayer l, XmlWriter output)
		{
			output.WriteStartElement("dap");
			output.WriteAttributeString("datasetname", l.DAPDatasetName);
			output.WriteAttributeString("height", l.DAPHeight.ToString());
			output.WriteAttributeString("size", l.DAPSize.ToString());
			output.WriteAttributeString("type", l.DAPType);
			output.WriteAttributeString("edition", l.DAPEdition);
			output.WriteAttributeString("hierarchy", l.DAPHierarchy);
			output.WriteAttributeString("levels", l.DAPLevels.ToString());
			output.WriteAttributeString("lvlzerotilesize", l.DAPLevelZeroTilesize.ToString());
			output.WriteAttributeString("layerdescription", l.DAPLayerDescription);
			output.WriteEndElement();
		}
	}
}
