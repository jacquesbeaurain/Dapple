using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using WorldWind;
using WorldWind.Renderable;
using Geosoft;
using Geosoft.Dap.Common;
using Geosoft.GX.DAPGetData;
using System.Xml;
using System.Xml.XPath;

namespace Dapple.LayerGeneration
{
   public class DAPQuadLayerBuilder : ImageBuilder
   {
      #region Static

      public static readonly string URISchemeName = "gxdap";

      public static readonly string TypeName = "DAPQuadLayer";

      public static readonly string CacheSubDir = "DAP Tile Cache";

      public static DAPQuadLayerBuilder GetBuilderFromURI(string strURI, Dapple.ServerTree oServerTree, string strCacheDir, WorldWindow worldWindow, ref bool bOldView)
      {
         bOldView = false;

         try
         {
            string strHost = String.Empty;
            string strPath = String.Empty;
            NameValueCollection queryColl = Utility.URI.ParseURI(URISchemeName, strURI, ref strHost, ref strPath);
            string strServerUrl = Utility.URI.CreateURI("http", strHost, strPath, null);

            Server oServer = null;
            if (!oServerTree.FullServerList.ContainsKey(strServerUrl))
               oServerTree.AddDAPServer(strServerUrl, out oServer);
            else
               oServer = oServerTree.FullServerList[strServerUrl];

            DataSet hDataSet = new DataSet();
            hDataSet.Url = strServerUrl;

            hDataSet.Name = queryColl.Get("datasetname");
            int height = Convert.ToInt32(queryColl.Get("height"));
            int size = Convert.ToInt32(queryColl.Get("size"));

            bOldView = true;
            foreach (string str in queryColl.AllKeys)
            {
               if (str == "type")
               {
                  bOldView = false;
                  break;
               }
            }
            if (bOldView)
               return null;

            hDataSet.Type = queryColl.Get("type");
            hDataSet.Title = queryColl.Get("title");
            hDataSet.Edition = queryColl.Get("edition");
            hDataSet.Hierarchy = queryColl.Get("hierarchy");
            double north = Convert.ToDouble(queryColl.Get("north"));
            double east = Convert.ToDouble(queryColl.Get("east"));
            double south = Convert.ToDouble(queryColl.Get("south"));
            double west = Convert.ToDouble(queryColl.Get("west"));
            hDataSet.Boundary = new Geosoft.Dap.Common.BoundingBox(east, north, west, south);

            int levels = Convert.ToInt32(queryColl.Get("levels"));
            decimal lvl0tilesize = Convert.ToDecimal(queryColl.Get("lvl0tilesize"));


            if (hDataSet != null)
            {
               DAPQuadLayerBuilder layerBuilder = new DAPQuadLayerBuilder(hDataSet, worldWindow.CurrentWorld, strCacheDir, oServer, null);
               if (layerBuilder != null)
               {
                  layerBuilder.m_iHeight = height;
                  layerBuilder.m_iTileImageSize = size;
                  layerBuilder.Levels = levels;
                  layerBuilder.LevelZeroTileSize = lvl0tilesize;
                  return layerBuilder;
               }
            }
         }
         catch (Exception e)
         {
            throw new ApplicationException("Error parsing DAP layer URI: " +  strURI + "\n(" + e.Message + ")");
         }
         return null;
      }

      #endregion

      private QuadTileSet m_layer;
      public int m_iHeight = 0;
      public int m_iTileImageSize = 256;
      public DataSet m_hDataSet;
      private string m_strDAPType;
      private string m_strCacheRoot;
      private Server m_oServer;
      private int m_iLevels = 15;
      private decimal m_decLevelZeroTileSizeDegrees = 0;

      public DAPQuadLayerBuilder(DataSet dataSet, World world, string cacheDirectory, Server server, IBuilder parent)
      {
         m_strName = dataSet.Title;
         m_strDAPType = dataSet.Type;
         m_hDataSet = dataSet;
         m_oWorld = world;
         m_strCacheRoot = cacheDirectory;
         m_oServer = server;
         m_Parent = parent;
      }

      public override string ServiceType
      {
         get
         {
            return "DAP Layer";
         }
      }

      public string DAPServerURL
      {
         get
         {
            return m_hDataSet.Url;
         }
      }

      public string DatasetName
      {
         get
         {
            return m_hDataSet.Name;
         }
      }

      public override bool SupportsMetaData
      {
         get
         {
            return true;
         }
      }

      public override XmlNode GetMetaData(XmlDocument oDoc)
      {
         XmlDocument responseDoc = m_oServer.Command.GetMetaData(m_hDataSet, null);
         XmlNode oNode = responseDoc.DocumentElement.FirstChild.FirstChild.FirstChild;
         XmlNode metaNode = oDoc.CreateElement("dapmeta");
         XmlNode nameNode = oDoc.CreateElement("name");
         nameNode.InnerText = Name;
         metaNode.AppendChild(nameNode);
         XmlNode geoMetaNode = oDoc.CreateElement(oNode.Name);
         geoMetaNode.InnerXml = oNode.InnerXml;
         metaNode.AppendChild(geoMetaNode);
         return metaNode;
      }

      public override string StyleSheetName
      {
         get
         {
            return "dap_dataset.xsl"; 
         }
      }

      #region Public Properties

      public decimal LevelZeroTileSize
      {
         get
         {
            if (m_decLevelZeroTileSizeDegrees == 0)
               // Round to ceiling of four decimals (>~ 10 meter resolution)
               m_decLevelZeroTileSizeDegrees = Math.Min(30, Math.Ceiling(10000 * (decimal)Math.Max(m_hDataSet.Boundary.MaxY - m_hDataSet.Boundary.MinY, m_hDataSet.Boundary.MaxX - m_hDataSet.Boundary.MinX)) / 10000);
            return m_decLevelZeroTileSizeDegrees;
         }
         set
         {
            m_decLevelZeroTileSizeDegrees = value;
         }
      }

      public int Levels
      {
         get
         {
            return m_iLevels;
         }
         set
         {
            m_iLevels = value;
         }
      }

      #endregion

      #region ImageBuilder Members

      public override WorldWind.GeographicBoundingBox Extents
      {
         get
         {
            if (m_layer != null)
            {
               return m_layer.QuadTileArgs.Boundary;
            }
            return new GeographicBoundingBox(m_hDataSet.Boundary.MaxY,
                m_hDataSet.Boundary.MinY,
                m_hDataSet.Boundary.MinX,
                m_hDataSet.Boundary.MaxX);
         }
      }

      public override int ImagePixelSize
      {
         get
         {
            return m_iTileImageSize;
         }
         set
         {
            if (m_iTileImageSize != value)
            {
               if (m_layer != null)
                  m_layer.Dispose();
               m_layer = null;
               m_iTileImageSize = value;
            }
         }
      }

      #endregion

      #region IBuilder Members

      public override byte Opacity
      {
         get
         {
            if (m_layer != null)
               return m_layer.Opacity;
            return m_bOpacity;
         }
         set
         {
            bool bChanged = false;
            if (m_bOpacity != value)
            {
               m_bOpacity = value;
               bChanged = true;
            }
            if (m_layer != null && m_layer.Opacity != value)
            {
               m_layer.Opacity = value;
               bChanged = true;
            }
            if (bChanged)
               SendBuilderChanged(BuilderChangeType.OpacityChanged);
         }
      }

      public override bool Visible
      {
         get
         {
            if (m_layer != null)
               return m_layer.IsOn;
            return m_IsOn;
         }
         set
         {
            bool bChanged = false;
            if (m_IsOn != value)
            {
               m_IsOn = value;
               bChanged = true;
            }
            if (m_layer != null && m_layer.IsOn != value)
            {
               m_layer.IsOn = value;
               bChanged = true;
            }

            if (bChanged)
               SendBuilderChanged(BuilderChangeType.VisibilityChanged);
         }
      }

      public override string Type
      {
         get { return DAPQuadLayerBuilder.TypeName; }
      }

      public override bool IsChanged
      {
         get { return m_layer == null; }
      }

      public override string LogoKey
      {
         get { return "dap"; }
      }

      public override bool bIsDownloading(out int iBytesRead, out int iTotalBytes)
      {
         if (m_layer != null)
            return m_layer.bIsDownloading(out iBytesRead, out iTotalBytes);
         else
         {
            iBytesRead = 0;
            iTotalBytes = 0;
            return false;
         }
      }
      
      public override WorldWind.Renderable.RenderableObject GetLayer()
      {
         return GetQuadTileSet();
      }

      #endregion

      public QuadTileSet GetQuadTileSet()
      {
         if (m_layer == null)
         {
            string strCachePath = Path.Combine(GetCachePath(), LevelZeroTileSize.ToString());
            System.IO.Directory.CreateDirectory(strCachePath);

            // Determine the needed levels (function of tile size and resolution if available)
            double dRes = GetResolution();
            if (dRes > 0)
            {
               decimal dTileSize = LevelZeroTileSize;
               m_iLevels = 1;
               while ((double) dTileSize / Convert.ToDouble(m_iTileImageSize) > dRes / 4.0)
               {
                  m_iLevels++;
                  dTileSize /= 2;
               }
            }

            GeosoftPlugin.New.DAPImageAccessor imgAccessor = new GeosoftPlugin.New.DAPImageAccessor(
                m_hDataSet.Name, m_oServer,
                 strCachePath,
                 m_iTileImageSize,
                 LevelZeroTileSize,
                 m_iLevels, ".png", strCachePath);

            GeographicBoundingBox box = new WorldWind.GeographicBoundingBox(m_hDataSet.Boundary.MaxY,
                m_hDataSet.Boundary.MinY, m_hDataSet.Boundary.MinX, m_hDataSet.Boundary.MaxX);

            m_layer = new QuadTileSet(m_hDataSet.Title, box, m_oWorld, 0,
                m_oWorld.TerrainAccessor,
                imgAccessor, m_bOpacity, true);
         }
         return m_layer;
      }

      /// <summary>
      /// Resolves resolution in degrees
      /// </summary>
      /// <returns></returns>
      public double GetResolution()
      {
         try
         {
            XmlDocument doc = m_oServer.Command.GetMetaData(m_hDataSet, null);
            XmlNode nodeRes = doc.SelectSingleNode("//meta/CLASS/CLASS/ATTRIBUTE[@name='SpatialResolution']");
            if (nodeRes != null)
            {
               int dX, dY;

               double dSpatRes = Convert.ToDouble(nodeRes.Attributes["value"].Value);
               double dMinX = Convert.ToDouble(doc.SelectSingleNode("//meta/CLASS/CLASS/ATTRIBUTE[@name='BoundingMinX']").Attributes["value"].Value);
               double dMinY = Convert.ToDouble(doc.SelectSingleNode("//meta/CLASS/CLASS/ATTRIBUTE[@name='BoundingMinY']").Attributes["value"].Value);
               double dMaxX = Convert.ToDouble(doc.SelectSingleNode("//meta/CLASS/CLASS/ATTRIBUTE[@name='BoundingMaxX']").Attributes["value"].Value);
               double dMaxY = Convert.ToDouble(doc.SelectSingleNode("//meta/CLASS/CLASS/ATTRIBUTE[@name='BoundingMaxY']").Attributes["value"].Value);

               dX = (int)Math.Round((dMaxX - dMinX) / dSpatRes);
               dY = (int)Math.Round((dMaxY - dMinY) / dSpatRes);

               return Math.Min((m_hDataSet.Boundary.MaxX - m_hDataSet.Boundary.MinX) / dX, (m_hDataSet.Boundary.MaxY - m_hDataSet.Boundary.MinY) / dY);
            }
         }
         catch
         {
            return 0.0;
         }
         return 0.0;
      }

      public override string GetCachePath()
      {
         return Path.Combine(Path.Combine(Path.Combine(m_strCacheRoot, CacheSubDir), m_oServer.Url.Replace("http://", "")), m_hDataSet.GetHashCode().ToString());
      }

      public override string GetURI()
      {
         NameValueCollection queryColl = new NameValueCollection();

         queryColl.Add("datasetname", m_hDataSet.Name);
         queryColl.Add("height", m_iHeight.ToString());
         queryColl.Add("size", m_iTileImageSize.ToString());
         queryColl.Add("type", m_hDataSet.Type);
         queryColl.Add("title", m_hDataSet.Title);
         queryColl.Add("edition", m_hDataSet.Edition);
         queryColl.Add("hierarchy", m_hDataSet.Hierarchy);
         queryColl.Add("north", m_hDataSet.Boundary.MaxY.ToString());
         queryColl.Add("east", m_hDataSet.Boundary.MaxX.ToString());
         queryColl.Add("south", m_hDataSet.Boundary.MinY.ToString());
         queryColl.Add("west", m_hDataSet.Boundary.MinX.ToString());
         queryColl.Add("levels", m_iLevels.ToString());
         queryColl.Add("lvl0tilesize", m_decLevelZeroTileSizeDegrees.ToString());

         string strHost = "";
         string strPath = "";
         Utility.URI.ParseURI("http", m_oServer.Url, ref strHost, ref strPath);
         
         return Utility.URI.CreateURI(URISchemeName, strHost, strPath, queryColl);
      }

      public override object Clone()
      {
         DataSet hDSCopy = new DataSet();
         hDSCopy.Edition = m_hDataSet.Edition;
         hDSCopy.Boundary = m_hDataSet.Boundary;
         hDSCopy.Hierarchy = m_hDataSet.Hierarchy;
         hDSCopy.Name = m_hDataSet.Name;
         hDSCopy.Title = m_hDataSet.Title;
         hDSCopy.Type = m_hDataSet.Type;
         hDSCopy.Url = m_hDataSet.Url;

         return new DAPQuadLayerBuilder(hDSCopy, m_oWorld, m_strCacheRoot, m_oServer, m_Parent);
      }

      protected override void CleanUpLayer(bool bFinal)
      {
         if (m_layer != null)
            m_layer.Dispose();
         m_layer = null;
      }

      public string DAPType
      {
         get
         {
            return m_strDAPType;
         }
      }
   }
}
