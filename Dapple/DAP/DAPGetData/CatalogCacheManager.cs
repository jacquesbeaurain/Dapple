using System;
using System.Threading;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Soap;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

using Geosoft.Dap;
using Geosoft.Dap.Common;

namespace Geosoft.GX.DAPGetData
{
   internal class CatalogCacheManager : IDisposable
   {
      // Make Cache upper limit fixed at 10 MB and cleanup to 9MB lower limit
      // and cleanup every 60 seconds
      public long m_lCacheUpperLimit = 10L * 1024L * 1024L;
      public long m_lCacheLowerLimit = 9L * 1024L * 1024L;
      public TimeSpan m_tsCleanupFrequency = TimeSpan.FromSeconds(60);
      Timer m_timer;
      string m_strCacheDir = String.Empty;
      ServerTree m_oServerTree;

      /// <summary>
      /// Initializes a new instance of the CatalogCacheManager class.
      /// </summary>
      /// <param name="strCacheDir">Location of the cache files.</param>
      /// <remarks>Only files with extension .gz is managed and cleaned up</remarks>
#if !DAPPLE
      public CatalogCacheManager(ServerTree oServerTree)
      {
         // --- Create the cache manager ---

         GXNet.CSYS.IGetDirectory(GXNet.Constant.SYS_DIR_USER, ref m_strCacheDir);
         m_strCacheDir = Path.Combine(m_strCacheDir, "DapCache");
#else
      public CatalogCacheManager(ServerTree oServerTree, string strCacheDir)
      {
         m_strCacheDir = strCacheDir; 
         Directory.CreateDirectory(m_strCacheDir);
#endif
        // Start the timer
         m_timer = new Timer(new TimerCallback(OnTimer), null, 0, (long)m_tsCleanupFrequency.TotalMilliseconds);

         m_oServerTree = oServerTree;
      }

      /// <summary>
      /// Get the list of datasets for a particular path
      /// </summary>
      /// <param name="oServer"></param>
      /// <param name="strHierarchy"></param>
      /// <param name="iTimestamp"></param>
      /// <param name="oBounds"></param>
      /// <param name="bAOIFilter"></param>
      /// <param name="bTextFilter"></param>
      /// <param name="strSearchString"></param>
      /// <returns>true if server's cache version changed</returns>
      public bool bGetDatasetList(Server oServer, string strHierarchy, int iTimestamp, BoundingBox oBounds, bool bAOIFilter, bool bTextFilter, string strSearchString)
      {
         string strEdition;
         System.Xml.XmlDocument oDoc = null;
         string strKey = string.Format("{0}:{1}", oServer.Url, strHierarchy);

         if (!bAOIFilter && !bTextFilter)
            oDoc = oServer.Command.GetCatalog(strHierarchy, 1, 0, 0, null, null, null);
         else if (!bAOIFilter && bTextFilter)
            oDoc = oServer.Command.GetCatalog(strHierarchy, 1, 0, 0, strSearchString, null, null);
         else if (bAOIFilter && !bTextFilter)
            oDoc = oServer.Command.GetCatalog(strHierarchy, 1, 0, 0, null, oBounds, null);
         else if (bAOIFilter && bTextFilter)
            oDoc = oServer.Command.GetCatalog(strHierarchy, 1, 0, 0, strSearchString, oBounds, null);

         if (bTextFilter)
            strKey += "_" + strSearchString;
         if (bAOIFilter)
            strKey += "_" + oBounds.GetHashCode().ToString();

         string strFile = Path.Combine(oServer.CacheDir, strKey.GetHashCode().ToString() + ".dap_datasetlist.gz");

         FolderDatasetList oDataset = FolderDatasetList.Parse(oServer, strKey, iTimestamp, oDoc, out strEdition);

         if (strEdition != oServer.CacheVersion)
         {
            oServer.UpdateConfiguration();
            return false;
         }

         if (oDataset != null)
         {
            try
            {
               if (File.Exists(strFile))
                  File.Delete(strFile);

               // --- Use SOAP formatting and GZip compression to disk  ---
               // --- This way we have a nice human readable format and ---
               // --- we may later move caching to database based.      ---
               IFormatter formatter = new SoapFormatter();

               using (Stream stream = new FileStream(strFile, FileMode.Create, FileAccess.Write, FileShare.None))
               {
                  using (GZipStream compStream = new GZipStream(stream, CompressionMode.Compress, true))
                  {
                     formatter.Serialize(compStream, oDataset);
                     compStream.Close();
                     stream.Close();
                  }
               }
            }
            catch
            {
            }
         }

         return true;
      }


      /// <summary>
      /// Get the root catalog hierarchy for a particular path
      /// </summary>
      /// <param name="oServer"></param>
      /// <param name="oBounds"></param>
      /// <param name="bAOIFilter"></param>
      /// <param name="bTextFilter"></param>
      /// <param name="strSearchString"></param>
      /// <returns>The hierarchy as a CatalogFolder</returns>
      public CatalogFolder GetCatalogHierarchyRoot(Server oServer, BoundingBox oBounds, bool bAOIFilter, bool bTextFilter, string strSearchString, out bool bEntireCatalogMode, out string strEdition)
      {
         XmlDocument oDoc = null;

         strEdition = "";
         bEntireCatalogMode = false;

         string strKey = oServer.Url;
         if (bTextFilter)
            strKey += "_" + strSearchString;
         if (bAOIFilter)
            strKey += "_" + oBounds.GetHashCode().ToString();
         string strFile = Path.Combine(oServer.CacheDir, strKey.GetHashCode().ToString() + ".dap_cataloghierarchy.gz");

         if (File.Exists(strFile))
         {
            try
            {
               // --- Use GZip compression to disk  ---
               IFormatter formatter = new SoapFormatter();
               using (Stream stream = new FileStream(strFile, FileMode.Open, FileAccess.Read, FileShare.Read))
               {
                  using (GZipStream compStream = new GZipStream(stream, CompressionMode.Decompress))
                  {
                     oDoc = new XmlDocument();
                     oDoc.Load(compStream);
                     compStream.Close();
                     stream.Close();

                     CatalogFolder oCatalogFolder = CatalogFolder.Parse(oDoc, out strEdition);
                     if (oCatalogFolder != null && strEdition == oServer.CacheVersion)
                        return oCatalogFolder;
                     oDoc = null;
                  }
               }
            }
            catch
            {
               oDoc = null;

               if (File.Exists(strFile))
                  File.Delete(strFile);
            }
         }

         if (oDoc == null)
         {
            if (!bAOIFilter && !bTextFilter)
               oDoc = oServer.Command.GetCatalogHierarchy();
            else if (!bAOIFilter && bTextFilter)
               oDoc = oServer.Command.GetCatalogHierarchy(strSearchString);
            else if (bAOIFilter && !bTextFilter)
               oDoc = oServer.Command.GetCatalogHierarchy(oBounds);
            else if (bAOIFilter && bTextFilter)
               oDoc = oServer.Command.GetCatalogHierarchy(strSearchString, oBounds);

            if (oDoc == null)
            {
               // --- do something to disable server ---

               m_oServerTree.NoResponseError();
               return null;
            }
   
            CatalogFolder oCatalogFolder = CatalogFolder.Parse(oDoc, out strEdition);

            if (oCatalogFolder == null)
            {
               // --- check to see if this is an unknown request ---

               System.Xml.XmlNodeList oNodeList;
               System.Xml.XmlNode oNode;

               oNodeList = oDoc.SelectNodes("//" + Geosoft.Dap.Xml.Common.Constant.Tag.ERROR_TAG);
               if (oNodeList != null && oNodeList.Count > 0)
               {
                  oNode = oNodeList[0];
                  if (oNode != null && oNode.InnerText.ToLower() == "unknown request.")
                  {
                     bEntireCatalogMode = true;
                  }
               }
            }
            else
            {
               if (strEdition != oServer.CacheVersion)
               {
                  oServer.UpdateConfiguration();
                  return null;
               }

               try
               {
                  if (File.Exists(strFile))
                     File.Delete(strFile);

                  // --- Use GZip compression to disk  ---
                  using (Stream stream = new FileStream(strFile, FileMode.Create, FileAccess.Write, FileShare.None))
                  {
                     using (GZipStream compStream = new GZipStream(stream, CompressionMode.Compress, true))
                     {
                        XmlWriter writer = XmlWriter.Create(compStream);
                        oDoc.Save(writer);
                        compStream.Close();
                        stream.Close();
                     }
                  }
               }
               catch
               {
               }
   
               return oCatalogFolder;
            }
         }

         return null;
      }

      public FolderDatasetList GetDatasets(Server oServer, CatalogFolder oFolder, BoundingBox oBounds, bool bAOIFilter, bool bTextFilter, string strSearchString)
      {
         FolderDatasetList oRet = null;

         string strKey = string.Format("{0}:{1}", oServer.Url, oFolder.Hierarchy);

         if (bTextFilter)
            strKey += "_" + strSearchString;
         if (bAOIFilter)
            strKey += "_" + oBounds.GetHashCode().ToString();

         string strFile = Path.Combine(oServer.CacheDir, strKey.GetHashCode().ToString() + ".dap_datasetlist.gz");

         if (File.Exists(strFile))
         {
            try
            {
               // --- Use SOAP formatting and GZip compression to disk  ---
               // --- This way we have a nice human readable format and ---
               // --- we may later move caching to database based.      ---
               IFormatter formatter = new SoapFormatter();

               using (Stream stream = new FileStream(strFile, FileMode.Open, FileAccess.Read, FileShare.Read))
               {
                  using (GZipStream compStream = new GZipStream(stream, CompressionMode.Decompress))
                  {
                     oRet = (FolderDatasetList)formatter.Deserialize(compStream);
                     compStream.Close();
                     stream.Close();
                  }
               }
            }
            catch
            {
               oRet = null;

               if (File.Exists(strFile))
                  File.Delete(strFile);
            }
         }

         if (oRet != null && oRet.Timestamp == oFolder.Timestamp && strKey == oRet.Key)
            return oRet;
         return null;
      }


      /// <summary>
      /// Monitors the cache, makes sure it stays within limits.
      /// </summary>
      private void OnTimer(object state)
      {
         try
         {
            Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;

            long dirSize = GetDirectorySize(new DirectoryInfo(m_strCacheDir));
            if (dirSize < this.m_lCacheUpperLimit)
               return;

            ArrayList fileInfoList = GetDirectoryFileInfoList(new DirectoryInfo(m_strCacheDir));
            while (dirSize > this.m_lCacheLowerLimit)
            {
               // Bail if there is only 5 files there
               if (fileInfoList.Count <= 5)
                  break;

               FileInfo oldestFile = null;
               foreach (FileInfo curFile in fileInfoList)
               {
                  if (oldestFile == null)
                  {
                     oldestFile = curFile;
                     continue;
                  }

                  if (curFile.LastAccessTimeUtc < oldestFile.LastAccessTimeUtc)
                  {
                     oldestFile = curFile;
                  }
               }

               fileInfoList.Remove(oldestFile);
               dirSize -= oldestFile.Length;
               try
               {
                  File.Delete(oldestFile.FullName);

                  // Recursively remove empty directories
                  string directory = oldestFile.Directory.FullName;
                  while (Directory.GetFileSystemEntries(directory).Length == 0)
                  {
                     Directory.Delete(directory);
                     directory = Path.GetDirectoryName(directory);
                  }
               }
               catch (IOException)
               {
                  // Ignore non-removable file - move on to next
               }
            }
         }
         catch (Exception e)
         {
            GetDapError.Instance.Write("CatalogCacheManager - " + e.Message);
         }
      }

      public static ArrayList GetDirectoryFileInfoList(DirectoryInfo inDir)
      {
         ArrayList returnList = new ArrayList();
         foreach (DirectoryInfo subDir in inDir.GetDirectories())
         {
            returnList.AddRange(GetDirectoryFileInfoList(subDir));
         }
         foreach (FileInfo fi in inDir.GetFiles())
         {
            if (String.Compare(fi.Extension, ".gz", true) == 0)
               returnList.Add(fi);
         }
         return returnList;
      }

      public static long GetDirectorySize(DirectoryInfo inDir)
      {
         long returnBytes = 0;
         foreach (DirectoryInfo subDir in inDir.GetDirectories())
         {
            returnBytes += GetDirectorySize(subDir);
         }
         foreach (FileInfo fi in inDir.GetFiles())
         {
            if (String.Compare(fi.Extension, ".gz", true) == 0)
            {
               try
               {
                  returnBytes += fi.Length;
               }
               catch (System.IO.IOException)
               {
                  // Ignore files that may have disappeared since we started scanning.
               }
            }
         }
         return returnBytes;
      }

      #region IDisposable Members

      public void Dispose()
      {
         if (m_timer != null)
         {
            m_timer.Dispose();
            m_timer = null;
         }
      }

      #endregion

   }
}
