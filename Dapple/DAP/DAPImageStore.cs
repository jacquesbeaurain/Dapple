using Microsoft.DirectX.Direct3D;
using System;
using System.Drawing;
using System.IO;
using WorldWind;
using WorldWind.Net.Wms;
using WorldWind.Renderable;
using Geosoft.Dap.Common;

namespace Dapple.DAP
{
	/// <summary>
	/// Summary description for DAPImageStore.
	/// </summary>
	internal class DAPImageStore : ImageStore
	{
		#region Private Members

      protected DataSet m_oDataSet;
      protected Geosoft.GX.DAPGetData.Server m_oServer;
		protected int m_TextureSizePixels;

		#endregion

		#region Properties

       internal Geosoft.GX.DAPGetData.Server Server
       {
           get
           {
               return m_oServer;
           }
       }

      internal DataSet DataSet
      {
         get
         {
            return m_oDataSet;
         }
      }

		/// <summary>
		/// The size of a texture tile in the store (for Nlt servers this is later determined in QTS texture loading code once first tile is read)
		/// </summary>
		public override int TextureSizePixels
		{
			get
			{
				return m_TextureSizePixels;
			}
			set
			{
				m_TextureSizePixels = value;
			}
		}

      

		#endregion

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:DAPImageStore"/> class.
		/// </summary>
      internal DAPImageStore(DataSet oDataSet,
         Geosoft.GX.DAPGetData.Server server)
      {
         m_oDataSet = oDataSet;
         m_oServer = server;
      }

		public override bool IsDownloadableLayer
		{
			get
			{
				return true;
			}
		}

      protected override void QueueDownload(IGeoSpatialDownloadTile tile, string filePath)
		{
			tile.TileSet.AddToDownloadQueue(tile.TileSet.Camera,
				new DAPDownloadRequest(tile, this, filePath));
		}
  }
}
