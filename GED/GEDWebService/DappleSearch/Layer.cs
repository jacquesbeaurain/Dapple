using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Geosoft.Dap.Common;

namespace GED.WebService.DappleSearch
{
	public abstract class Layer
	{
		public float Relevance { get; set; }
		public string ServerTitle { get; set; }
		public string LayerTitle { get; set; }
		public string ServerDescription { get; set; }
		public int DappleSearchLayerID { get; set; }
		public abstract String Type { get; }
		public string Url { get; set; }
		public BoundingBox BoundingBox { get; set; }

		public Layer(float releavance, string serverTitle, string layerTitle, string serverDescription, int dappleSearchLayerID, string url, BoundingBox bounds)
		{
			Relevance = releavance;
			ServerTitle = serverTitle;
			LayerTitle = layerTitle;
			ServerDescription = serverDescription;
			DappleSearchLayerID = dappleSearchLayerID;
			Url = url;
			BoundingBox = bounds;
		}
	}

	public class DAPLayer : Layer
	{
		public string DAPDatasetName { get; set; }
		public double DAPHeight { get; set; }
		public int DAPSize { get; set; }
		public string DAPType { get; set; }
		public string DAPEdition { get; set; }
		public string DAPHierarchy { get; set; }
		public int DAPLevels { get; set; }
		public double DAPLevelZeroTilesize { get; set; }
		public string DAPLayerDescription { get; set; }
		public override string Type { get { return "DAP"; } }

		public DAPLayer(DataSet data)
			: base(1.0f, "[Servertitle]", data.Title, "[ServerDescription]", int.Parse(data.Name), new Uri(data.Url).Host, data.Boundary)
		{
			DAPDatasetName = data.Name;
			DAPHeight = 0;
			DAPSize = 256;
			DAPType = data.Type;
			DAPEdition = data.Edition;
			DAPHierarchy = data.Hierarchy;
			DAPLevels = 15;
			DAPLevelZeroTilesize = 22.5;
			DAPLayerDescription = "[LayerDescription]";
		}
	}
}
