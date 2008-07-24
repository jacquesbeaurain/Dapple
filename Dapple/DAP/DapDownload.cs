using System;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.IO;
using System.Threading;
using System.Xml;
using System.Collections;
using Utility;
using WorldWind.Renderable;
using WorldWind;
using WorldWind.Net;

namespace Dapple.DAP
{
   public class DAPDownloadRequest : GeoSpatialDownloadRequest
   {
		DAPImageStore m_DapImageStore;

		public DAPDownloadRequest(IGeoSpatialDownloadTile tile, DAPImageStore imageStore, string localFilePath)
         : base(tile, imageStore, localFilePath, "")
      {
			m_DapImageStore = imageStore;
      }

		private static Stream SaveDAPImage(System.Xml.XmlDocument hDoc, string strFile)
		{
			System.Xml.XmlNodeList hNodeList = hDoc.SelectNodes("/" + Geosoft.Dap.Xml.Common.Constant.Tag.GEO_XML_TAG + "/" + Geosoft.Dap.Xml.Common.Constant.Tag.RESPONSE_TAG + "/" + Geosoft.Dap.Xml.Common.Constant.Tag.IMAGE_TAG + "/" + Geosoft.Dap.Xml.Common.Constant.Tag.PICTURE_TAG);
         if (hNodeList.Count == 0)
         {
            hNodeList = hDoc.SelectNodes("/" + Geosoft.Dap.Xml.Common.Constant.Tag.GEO_XML_TAG + "/" + Geosoft.Dap.Xml.Common.Constant.Tag.RESPONSE_TAG + "/" + Geosoft.Dap.Xml.Common.Constant.Tag.TILE_TAG);
         }
			System.IO.FileStream fs = new System.IO.FileStream(strFile, System.IO.FileMode.Create);
			System.IO.BinaryWriter bs = new System.IO.BinaryWriter(fs);

			foreach (System.Xml.XmlNode hNode in hNodeList)
			{
				System.Xml.XmlNode hN1 = hNode.FirstChild;
				string jpegImage = hN1.Value;

				if (jpegImage != null)
				{
					byte[] jpegRawImage = Convert.FromBase64String(jpegImage);

					bs.Write(jpegRawImage);
				}
			}
			bs.Flush();
			bs.Close();
			fs.Close();
			return fs;
		}

		public override void StartDownload()
      {
			Log.Write(Log.Levels.Debug, "DGDR", "Starting download for DAP server " + m_DapImageStore.Server.Url);
			Log.Write(Log.Levels.Debug, "DGDR", "Dataset Name: " + (m_DapImageStore.DataSet == null ? "Browser Map" : m_DapImageStore.DataSet.Name));
         if (m_DapImageStore.Server.MajorVersion >= 11)
            Log.Write(Log.Levels.Debug, "DGDR", "Level: " + Tile.Level + " Column: " + Tile.Col + " Row: " + Tile.Row);
         else
            Log.Write(Log.Levels.Debug, "DGDR", "West: " + Tile.West + " South: " + Tile.South + " East: " + Tile.East + " North: " + Tile.North);
			Log.Write(Log.Levels.Debug, "DGDR", "to be stored in " + m_DapImageStore.GetLocalPath(Tile));

			Tile.IsDownloadingImage = true;

			Directory.CreateDirectory(Path.GetDirectoryName(m_localFilePath));
         if (m_DapImageStore.Server.MajorVersion >= 11)
         {
            download = new DapTileDownload(m_localFilePath, Tile, m_DapImageStore);
         }
         else
         {
            download = new DapDownload(m_localFilePath, new Geosoft.Dap.Common.BoundingBox(Tile.East, Tile.North, Tile.West, Tile.South), m_DapImageStore);
         }
			download.DownloadType = DownloadType.Unspecified;
			download.SavedFilePath = m_localFilePath + ".tmp";
			download.ProgressCallback += new DownloadProgressHandler(UpdateProgress);
			download.CompleteCallback += new WorldWind.Net.DownloadCompleteHandler(DownloadComplete);
			download.BackgroundDownloadMemory();
      }

		protected override void DownloadComplete(WebDownload downloadInfo)
		{
         Log.Write(Log.Levels.Debug, "DGDR", "Finishing download for DAP server " + m_DapImageStore.Server.Url);
         Log.Write(Log.Levels.Debug, "DGDR", "Dataset Name: " + (m_DapImageStore.DataSet == null ? "Browser Map" : m_DapImageStore.DataSet.Name));
         if (m_DapImageStore.Server.MajorVersion >= 11)
            Log.Write(Log.Levels.Debug, "DGDR", "Level: " + Tile.Level + " Column: " + Tile.Col + " Row: " + Tile.Row);
         else
            Log.Write(Log.Levels.Debug, "DGDR", "West: " + Tile.West + " South: " + Tile.South + " East: " + Tile.East + " North: " + Tile.North);
         Log.Write(Log.Levels.Debug, "DGDR", "to be stored in " + m_DapImageStore.GetLocalPath(Tile));
			try
			{
				downloadInfo.Verify();

				// --- Load Response into XML Document and convert to real image ---

				System.Xml.XmlDocument hResponseDocument = new System.Xml.XmlDocument();
				System.Xml.XmlReaderSettings oSettings = new System.Xml.XmlReaderSettings();
				oSettings.IgnoreWhitespace = true;
				System.Xml.XmlReader oResponseXmlStream = System.Xml.XmlReader.Create(downloadInfo.ContentStream, oSettings);
				hResponseDocument.Load(oResponseXmlStream);
				SaveDAPImage(hResponseDocument, downloadInfo.SavedFilePath);

				// --- search for an error ---

				System.Xml.XmlNodeList hNodeList = hResponseDocument.SelectNodes("//" + Geosoft.Dap.Xml.Common.Constant.Tag.ERROR_TAG);
				if (hNodeList.Count >= 1)
				{
					System.Xml.XmlNode hNode = hNodeList[0];

					throw new Geosoft.Dap.DapException(hNode.InnerText);
				}

				m_tile.TileSet.NumberRetries = 0;
				if (m_tile.IsValidTile(downloadInfo.SavedFilePath))
				{
					// Rename temp file to real name
					File.Delete(m_localFilePath);
					File.Move(downloadInfo.SavedFilePath, m_localFilePath);

					// Make the tile reload the new image
					m_tile.DownloadRequests.Remove(this);
					m_tile.Initialize();
				}
				else
				{
					using (File.Create(m_localFilePath + ".txt"))
					{ }
					if (File.Exists(downloadInfo.SavedFilePath))
					{
						try
						{
							File.Delete(downloadInfo.SavedFilePath);
						}
						catch (Exception e)
						{
							Log.Write(Log.Levels.Error, "DGDR", "could not delete file " + downloadInfo.SavedFilePath + ":");
							Log.Write(e);
						}
					}
				}
			}
			catch (System.Net.WebException caught)
			{
				System.Net.HttpWebResponse response = caught.Response as System.Net.HttpWebResponse;
				if (response != null && response.StatusCode == System.Net.HttpStatusCode.NotFound)
				{
					using (File.Create(m_localFilePath + ".txt"))
					{ }
					return;
				}
            Log.Write(Log.Levels.Error, "DGDR", "web exception occurred");
				Log.Write(Log.Levels.Error, "DGDR", "Dataset Name: " + (m_DapImageStore.DataSet == null ? "Browser Map" : m_DapImageStore.DataSet.Name));
            Log.Write(Log.Levels.Error, "DGDR", "West: " + Tile.West + " South: " + Tile.South + " East: " + Tile.East + " North: " + Tile.North);
				m_tile.TileSet.NumberRetries++;
			}
			catch
			{
				using (File.Create(m_localFilePath + ".txt"))
				{ }
				if (File.Exists(downloadInfo.SavedFilePath))
				{
					try
					{
						File.Delete(downloadInfo.SavedFilePath);
					}
					catch (Exception e)
					{
						Log.Write(Log.Levels.Error, "GSDR", "could not delete file " + downloadInfo.SavedFilePath + ":");
						Log.Write(e);
					}
				}
			}
			finally
			{
				if (download != null)
					download.IsComplete = true;

				// Immediately queue next download
				m_tile.TileSet.RemoveFromDownloadQueue(this, true);
			}
		}
   }

   public class DapDownload : WorldWind.Net.WebDownload
   {
      private Geosoft.Dap.Common.BoundingBox m_hBB;
		private DAPImageStore m_oImageStore;

		public DapDownload(string imagePath, Geosoft.Dap.Common.BoundingBox hBB, DAPImageStore imageStore)
         : base()
      {
         m_hBB = hBB;
			m_oImageStore = imageStore;
			Url = imageStore.Server.Url;
      }

		protected override HttpWebRequest BuildRequest()
		{
			string strURL;
			System.Xml.XmlDocument oRequestDoc = null;

			try
			{
				System.IO.StringWriter hRequest = new StringWriter();

				Geosoft.Dap.Common.Format oFormat = new Geosoft.Dap.Common.Format();
				oFormat.Type = "image/" + m_oImageStore.ImageExtension;
				oFormat.Transparent = true;
				Geosoft.Dap.Common.Resolution oRes = new Geosoft.Dap.Common.Resolution();
				oRes.Height = m_oImageStore.TextureSizePixels;
				oRes.Width = m_oImageStore.TextureSizePixels;
				ArrayList oArr = new ArrayList();
            if (m_oImageStore.DataSet == null)
            {
               oRequestDoc = m_oImageStore.Server.Command.GetImageRequestDocument(oFormat, m_hBB, oRes, true, false, oArr, out strURL);
            }
            else
            {
               oArr.Add(m_oImageStore.DataSet.Name);
               oRequestDoc = m_oImageStore.Server.Command.GetImageRequestDocument(oFormat, m_hBB, oRes, false, false, oArr, out strURL);
            }

				// Create the request object.
				request = (HttpWebRequest)WebRequest.Create(strURL);
				request.UserAgent = UserAgent;
				request.Pipelined = false;

				request.Proxy = ProxyHelper.DetermineProxyForUrl(
								  strURL,
								  WorldWind.Net.WebDownload.useWindowsDefaultProxy,
								  WorldWind.Net.WebDownload.useDynamicProxy,
								  WorldWind.Net.WebDownload.proxyUrl,
								  WorldWind.Net.WebDownload.proxyUserName,
								  WorldWind.Net.WebDownload.proxyPassword);


				// --- Encode the document into ascii ---

				System.Text.UTF8Encoding hRequestEncoding = new System.Text.UTF8Encoding();

				oRequestDoc.Save(hRequest);
				byte[] byte1 = hRequestEncoding.GetBytes(hRequest.GetStringBuilder().ToString());


				// --- Setup the HTTP Request ---

				request.Method = "POST";

				if (WorldWind.Net.WebDownload.useProto == WorldWind.Net.WebDownload.HttpProtoVersion.HTTP1_1)
					request.ProtocolVersion = HttpVersion.Version11;
				else
					request.ProtocolVersion = HttpVersion.Version10;

				request.KeepAlive = false;
				request.ContentType = "application/x-www-form-urlencoded";
				request.ContentLength = byte1.Length;
				request.Timeout = MainForm.DOWNLOAD_TIMEOUT;

				// --- Serialize the XML document request onto the wire ---

				System.IO.Stream hRequestStream = request.GetRequestStream();
				hRequestStream.Write(byte1, 0, byte1.Length);
				hRequestStream.Close();

				// --- Turn off connection keep-alives. ---

				request.KeepAlive = false;
			}
			finally
			{
				m_oImageStore.Server.Command.ReleaseImageRequestDocument(oRequestDoc);
			}

			return request;
		}
   }

   public class DapTileDownload : WorldWind.Net.WebDownload
   {
      private IGeoSpatialDownloadTile m_oTile;
      private DAPImageStore m_oImageStore;

      public DapTileDownload(string imagePath, IGeoSpatialDownloadTile oTile, DAPImageStore imageStore)
         : base()
      {
         m_oTile = oTile;
         m_oImageStore = imageStore;
         Url = imageStore.Server.Url;
      }

      protected override HttpWebRequest BuildRequest()
      {
         string strURL;
         System.Xml.XmlDocument oRequestDoc = null;

         try
         {
            System.IO.StringWriter hRequest = new StringWriter();

            Geosoft.Dap.Common.Format oFormat = new Geosoft.Dap.Common.Format();
            oFormat.Type = "image/" + m_oImageStore.ImageExtension;
            oFormat.Transparent = true;
            Geosoft.Dap.Common.Resolution oRes = new Geosoft.Dap.Common.Resolution();
            oRes.Height = m_oImageStore.TextureSizePixels;
            oRes.Width = m_oImageStore.TextureSizePixels;
            ArrayList oArr = new ArrayList();
            if (m_oImageStore.DataSet == null) // Base layers don't have tiling enabled
            {
               oRequestDoc = m_oImageStore.Server.Command.GetImageRequestDocument(oFormat, new Geosoft.Dap.Common.BoundingBox(m_oTile.East, m_oTile.North, m_oTile.West, m_oTile.South), oRes, true, false, oArr, out strURL);
            }
            else
            {
               oRequestDoc = m_oImageStore.Server.Command.GetTileRequestDocument(m_oImageStore.DataSet, m_oTile.Level, m_oTile.Row, m_oTile.Col, out strURL);
            }

            // Create the request object.
            request = (HttpWebRequest)WebRequest.Create(strURL);
            request.UserAgent = UserAgent;
            request.Pipelined = false;

            request.Proxy = ProxyHelper.DetermineProxyForUrl(
                          strURL,
                          WorldWind.Net.WebDownload.useWindowsDefaultProxy,
                          WorldWind.Net.WebDownload.useDynamicProxy,
                          WorldWind.Net.WebDownload.proxyUrl,
                          WorldWind.Net.WebDownload.proxyUserName,
                          WorldWind.Net.WebDownload.proxyPassword);


            // --- Encode the document into ascii ---

            System.Text.UTF8Encoding hRequestEncoding = new System.Text.UTF8Encoding();

            oRequestDoc.Save(hRequest);
            byte[] byte1 = hRequestEncoding.GetBytes(hRequest.GetStringBuilder().ToString());


            // --- Setup the HTTP Request ---

            request.Method = "POST";

            if (WorldWind.Net.WebDownload.useProto == WorldWind.Net.WebDownload.HttpProtoVersion.HTTP1_1)
               request.ProtocolVersion = HttpVersion.Version11;
            else
               request.ProtocolVersion = HttpVersion.Version10;

            request.KeepAlive = false;
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = byte1.Length;
            request.Timeout = MainForm.DOWNLOAD_TIMEOUT;

            // --- Serialize the XML document request onto the wire ---

            System.IO.Stream hRequestStream = request.GetRequestStream();
            hRequestStream.Write(byte1, 0, byte1.Length);
            hRequestStream.Close();

            // --- Turn off connection keep-alives. ---

            request.KeepAlive = false;
         }
         finally
         {
            m_oImageStore.Server.Command.ReleaseImageRequestDocument(oRequestDoc);
         }

         return request;
      }
   }
}
