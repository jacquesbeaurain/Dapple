using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

namespace Utility
{
	public static class GCSMappings
	{
		private static readonly List<string> s_WMSWGS84Equivalents = new List<string>(new string[] {
         "EPSG:4019",
         "EPSG:4176",
         "EPSG:4151",
         "EPSG:4133",
         "EPSG:4180",
         "EPSG:4258",
         "EPSG:4283",
         "EPSG:4121",
         "EPSG:4173",
         "EPSG:4659",
         "EPSG:4141",
         "EPSG:4612",
         "EPSG:4319",
         "EPSG:4661",
         "EPSG:4126",
         "EPSG:4669",
         "EPSG:4269",
         "EPSG:4140",
         "EPSG:4167",
         "EPSG:4172",
         "EPSG:4190",
         "EPSG:4189",
         "EPSG:4171",
         "EPSG:4624",
         "EPSG:4627",
         "EPSG:4170",
         "EPSG:4619",
         "EPSG:4148",
         "EPSG:4670",
         "EPSG:4667",
         "EPSG:4166",
         "EPSG:4130",
         "EPSG:4318",
         "EPSG:4640",
         "EPSG:4326",
         "EPSG:4163",
         "CRS:83"
      });

		public static IList<String> WMSWGS84Equivalents
		{
			get { return new ReadOnlyCollection<String>(s_WMSWGS84Equivalents); }
		}

		private static readonly List<string> s_GeoTiffWGS84Equivalents = new List<string>(new string[] {
         "GCSE_GRS1980",
         "GCS_GGRS87",
         "GCS_RT90",
         "GCS_EST92",
         "GCS_EUREF89",
         "GCS_NAD83",
         "GCS_GDA94",
         "GCS_Dealul_Piscului_1970",
         "GCS_WGS_84"
      });

		public static IList<String> GeoTiffWGS84Equivalents
		{
			get { return new ReadOnlyCollection<String>(s_GeoTiffWGS84Equivalents); }
		}
	}
}
