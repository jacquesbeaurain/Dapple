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

namespace GeosoftPlugin.New
{
   public class DAPDownloadRequest : WorldWind.Renderable.GeoSpatialDownloadRequest
   {
      public DAPDownloadRequest(GeographicBoundingBox geoBox, string imagePath, WorldWind.Renderable.DownloadQueue queue, IImageAccessor imageAcessor)
         : base(geoBox, imagePath, queue, imageAcessor)
      {
         if (File.Exists(m_strImagePath))
         {
            // cause it to believe that the download is complete if the file exists
            m_isStarted = true;
         }
      }

      protected override WebDownload BeginDownload()
      {
         m_oDownload = new DapDownload(m_strImagePath, new Geosoft.Dap.Common.BoundingBox(Boundary.East, Boundary.North, Boundary.West, Boundary.South), (DAPImageAccessor)m_oImageAcessor);
         m_oDownload.SavedFilePath = m_strImagePath + ".tmp";
         return m_oDownload;
      }

      protected override void CompletedDownload(WorldWind.Net.WebDownload downloadInfo)
      {
         // Rename temp file to real name
         if (File.Exists(m_strImagePath))
            File.Delete(m_strImagePath);
         File.Move(downloadInfo.SavedFilePath, m_strImagePath);
      }

      protected override void DownloadFailed(WorldWind.Net.WebDownload downloadInfo)
      {
         if (File.Exists(downloadInfo.SavedFilePath))
            File.Delete(downloadInfo.SavedFilePath);
      }
   }

   public class DapDownload : WorldWind.Net.WebDownload
   {
      private string m_strImagePath;
      private Geosoft.Dap.Common.BoundingBox m_hBB;
      private DAPImageAccessor m_oImageAccessor;

      public DapDownload(string imagePath, Geosoft.Dap.Common.BoundingBox hBB, DAPImageAccessor imageAccessor)
         : base()
      {
         m_hBB = hBB;
         m_oImageAccessor = imageAccessor;
         m_strImagePath = imagePath;
      }

      protected Stream SaveImage(System.Xml.XmlDocument hDoc)
      {
         string szFileName = System.Guid.NewGuid().ToString() + m_oImageAccessor.ImageExtension;
         System.Xml.XmlNodeList hNodeList = hDoc.SelectNodes("/" + Geosoft.Dap.Xml.Common.Constant.Tag.GEO_XML_TAG + "/" + Geosoft.Dap.Xml.Common.Constant.Tag.RESPONSE_TAG + "/" + Geosoft.Dap.Xml.Common.Constant.Tag.IMAGE_TAG + "/" + Geosoft.Dap.Xml.Common.Constant.Tag.PICTURE_TAG);
         System.IO.FileStream fs = new System.IO.FileStream(SavedFilePath, System.IO.FileMode.Create);
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

      private void DownloadProgress(int iBytesRead, int iTotalBytes)
      {
         if (base.ProgressCallback != null)
            base.ProgressCallback(iBytesRead, iTotalBytes);
      }

      protected override void Download()
      {
         DownloadStartTime = DateTime.Now;
         try
         {
            try
            {
               // If a registered progress-callback, inform it of our download progress so far.
               if (base.ProgressCallback != null)
                  base.ProgressCallback(0, -1);

               Geosoft.Dap.Common.Format oFormat = new Geosoft.Dap.Common.Format();
               oFormat.Type = "image/" + m_oImageAccessor.ImageExtension;
               oFormat.Transparent = true;
               Geosoft.Dap.Common.Resolution oRes = new Geosoft.Dap.Common.Resolution();
               oRes.Height = m_oImageAccessor.TextureSizePixels;
               oRes.Width = m_oImageAccessor.TextureSizePixels;
               ArrayList oArr = new ArrayList();
               oArr.Add(m_oImageAccessor.Name);
               System.Xml.XmlDocument oDoc =
               m_oImageAccessor.Server.Command.GetImage(oFormat, m_hBB, oRes, oArr, new Geosoft.Dap.Xml.UpdateProgessCallback(DownloadProgress));

               // create content stream from memory or file
               if (isMemoryDownload && ContentStream == null)
               {
                  ContentStream = new MemoryStream();
               }
               else
               {
                  // Download to file
                  string targetDirectory = Path.GetDirectoryName(SavedFilePath);
                  if (targetDirectory.Length > 0)
                     Directory.CreateDirectory(targetDirectory);
                  ContentStream = SaveImage(oDoc);
               }
            }
            catch (System.Configuration.ConfigurationException)
            {
               // is thrown by WebRequest.Create if App.config is not in the correct format
               // TODO: don't know what to do with it
               throw;
            }
            catch
            {
               try
               {
                  // Remove broken file download
                  if (ContentStream != null)
                  {
                     ContentStream.Close();
                     ContentStream = null;
                  }
                  if (SavedFilePath != null && SavedFilePath.Length > 0)
                  {
                     File.Delete(SavedFilePath);
                  }
               }
               catch (Exception)
               {
               }
            }

            if (ContentStream is MemoryStream)
            {
               ContentStream.Seek(0, SeekOrigin.Begin);
            }
            else if (ContentStream != null)
            {
               ContentStream.Close();
               ContentStream = null;
            }

            OnDebugCallback(this);

            if (CompleteCallback == null)
            {
               Verify();
            }
            else
            {
               CompleteCallback(this);
            }
         }
         catch (ThreadAbortException)
         { }
         finally
         {
            IsComplete = true;
         }
      }
   }
}
