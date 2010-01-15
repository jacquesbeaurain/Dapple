using Geosoft.Dap.Common;
using System.Xml;

namespace GED.WebService.DappleSearch
{
	class SearchRequest
	{
		public string Version { get; private set; }
		public string Handle { get; private set; }
		public int Offset { get; private set; }
		public int MaxCount { get; private set; }
		public BoundingBox AoIFilter { get; private set; }
		public string TextFilter { get; private set; }

		public SearchRequest(string version, string handle, int offset, int maxCount, BoundingBox aoiFilter, string textFilter)
		{
			Version = version;
			Handle = handle;
			Offset = offset;
			MaxCount = maxCount;
			AoIFilter = aoiFilter;
			TextFilter = textFilter;
		}
	}

	static partial class Decoder
	{
		public static SearchRequest SearchRequest(string requestXML)
		{
			XmlDocument requestDocument = new XmlDocument();
			requestDocument.LoadXml(requestXML);

			string version = requestDocument.SelectSingleNode("/geosoft_xml/search_request/@version").Value;
			string handle = requestDocument.SelectSingleNode("/geosoft_xml/search_request/@handle").Value;
			int maxCount = int.Parse(requestDocument.SelectSingleNode("/geosoft_xml/search_request/@maxcount").Value);
			int offset = int.Parse(requestDocument.SelectSingleNode("/geosoft_xml/search_request/@offset").Value);

			BoundingBox aoiFilter = null;
			XmlElement boundingBoxElement = requestDocument.SelectSingleNode("/geosoft_xml/search_request/bounding_box") as XmlElement;
			if (boundingBoxElement != null)
			{
				aoiFilter = new BoundingBox(double.Parse(boundingBoxElement.GetAttribute("maxx")),
					double.Parse(boundingBoxElement.GetAttribute("maxy")),
					double.Parse(boundingBoxElement.GetAttribute("minx")),
					double.Parse(boundingBoxElement.GetAttribute("miny")));
			}
			string textFilter = null;
			XmlElement textFilterElement = requestDocument.SelectSingleNode("/geosoft_xml/search_request/text_filter") as XmlElement;
			if (textFilterElement != null)
			{
				textFilter = textFilterElement.InnerText;
			}

			return new SearchRequest(version, handle, offset, maxCount, aoiFilter, textFilter);
		}
	}
}
