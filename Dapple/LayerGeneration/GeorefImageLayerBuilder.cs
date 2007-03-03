using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using WorldWind;
using WorldWind.Renderable;
using System.Xml;
using Dapple;

namespace Dapple.LayerGeneration
{
   public class GeorefImageLayerBuilder : ImageBuilder
   {
      public static readonly string URLProtocolName = "gxtif:///";
      public static readonly string CacheSubDir = "Local Image Cache";
      ImageLayer m_Layer;
      string m_strCacheRoot;
      string m_strFileName;
      string m_strCacheFileName;
      bool m_bIsTmp;
      bool m_blnIsChanged = true;

      /// <summary>
      /// Obtain geographic extents from GeoTIF file
      /// </summary>
      /// <param name="strFile"></param>
      /// <returns>null if not WGS84 or other failure</returns>
      public static GeographicBoundingBox GetExtentsFromGeotif(string strFile)
      {
         string strTemp = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

         try
         {
            ProcessStartInfo psi = new ProcessStartInfo(Path.GetDirectoryName(Application.ExecutablePath) + @"\System\listgeo.exe");
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.CreateNoWindow = true;
            psi.Arguments = "-d \"" + strFile + "\"";

            using (Process p = Process.Start(psi))
            {
               using (StreamWriter sw = new StreamWriter(strTemp))
               {
                  sw.WriteLine(p.StandardOutput.ReadToEnd());
               }
               p.WaitForExit();
            }

            if (File.Exists(strTemp))
            {
               int iWGS84Check = 0;
               string strUL = "";
               string strLL = "";
               string strUR = "";
               string strLR = "";
               using (StreamReader sr = new StreamReader(strTemp))
               {
                  string strLine;
                  while ((strLine = sr.ReadLine()) != null)
                  {
                     if (strLine.Contains("GTModelTypeGeoKey (Short,1): ModelTypeGeographic"))
                        iWGS84Check++;

                     if (strLine.Contains("GeographicTypeGeoKey (Short,1): "))
                     {
                        foreach (string strGCS in Utility.GCSMappings.GeoTiffWGS84Equivalents)
                        {
                           if (strLine.Contains(strGCS))
                           {
                              iWGS84Check++;
                              break;
                           }
                        }
                     }
                     if (strLine.Contains("Upper Left"))
                        strUL = strLine;
                     if (strLine.Contains("Upper Right"))
                        strUR = strLine;
                     if (strLine.Contains("Lower Left"))
                        strLL = strLine;
                     if (strLine.Contains("Lower Right"))
                        strLR = strLine;
                  }
               }

               if (iWGS84Check >= 2 && strUL.Length > 0 && strLL.Length > 0 && strUR.Length > 0 && strLR.Length > 0)
               {
                  double dWest = Convert.ToDouble(strUL.Substring(strUL.IndexOf('(') + 1).Substring(0, strUL.IndexOf(',') - strUL.IndexOf('(') - 2).Trim(), System.Globalization.CultureInfo.InvariantCulture);
                  double dEast = Convert.ToDouble(strUR.Substring(strUR.IndexOf('(') + 1).Substring(0, strUR.IndexOf(',') - strUR.IndexOf('(') - 2).Trim(), System.Globalization.CultureInfo.InvariantCulture);
                  double dNorth = Convert.ToDouble(strUL.Substring(strUL.IndexOf(',') + 1).Substring(0, strUL.IndexOf(')') - strUL.IndexOf(',') - 2).Trim(), System.Globalization.CultureInfo.InvariantCulture);
                  double dSouth = Convert.ToDouble(strLL.Substring(strLL.IndexOf(',') + 1).Substring(0, strLL.IndexOf(')') - strLL.IndexOf(',') - 2).Trim(), System.Globalization.CultureInfo.InvariantCulture);

                  if (dWest < dEast && dSouth < dNorth)
                     return new GeographicBoundingBox(dNorth, dSouth, dWest, dEast);
               }
            }
         }
         catch
         {
         }
         finally
         {
            if (File.Exists(strTemp))
               File.Delete(strTemp);
         }

         return null;
      }

      /// <summary>
      /// Obtain geographic information string from GeoTIF file
      /// </summary>
      /// <param name="strFile"></param>
      /// <returns>empty null if not WGS84 or other failure</returns>
      public static string GetGeorefInfoFromGeotif(string strFile)
      {
         string strTemp = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
         string strReturn = "";
         
         try
         {
            ProcessStartInfo psi = new ProcessStartInfo(Path.GetDirectoryName(Application.ExecutablePath) + @"\System\listgeo.exe");
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.CreateNoWindow = true;
            psi.Arguments = "-d \"" + strFile + "\"";

            using (Process p = Process.Start(psi))
            {
               using (StreamWriter sw = new StreamWriter(strTemp))
               {
                  sw.WriteLine(p.StandardOutput.ReadToEnd());
               }
               p.WaitForExit();
            }

            if (File.Exists(strTemp))
            {
               string strGCS = "";
               string strProjection = "";
               using (StreamReader sr = new StreamReader(strTemp))
               {
                  string strLine;
                  while ((strLine = sr.ReadLine()) != null)
                  {
                     if (strLine.Contains("GeographicTypeGeoKey (Short,1):"))
                        strGCS = strLine.Replace("GeographicTypeGeoKey (Short,1):", "").Trim();
                     
                     if (String.Compare(strLine, 0, "Projection Method:", 0, "Projection Method:".Length) == 0)
                        strProjection = strLine;
                  }
               }
               if (strGCS.Length > 0)
                  strReturn += strGCS + "\n";
               strReturn += strProjection;
            }
         }
         catch
         {
         }
         finally
         {
            if (File.Exists(strTemp))
               File.Delete(strTemp);
         }

         return strReturn;
      }

      public GeorefImageLayerBuilder(string strCacheRoot, string strFileName, bool bTmp, World World, IBuilder parent)
      {
         m_strCacheRoot = strCacheRoot;
         m_strName = Path.GetFileName(strFileName);
         m_strFileName = strFileName;
         m_bIsTmp = bTmp;
         m_strCacheFileName = Path.Combine(GetCachePath(), Path.GetFileNameWithoutExtension(strFileName) + ".png");
         m_oWorld = World;
         m_Parent = parent;
      }

      public override RenderableObject GetLayer()
      {
         if (m_blnIsChanged)
         {
            try
            {
               GeographicBoundingBox extents = GeorefImageLayerBuilder.GetExtentsFromGeotif(m_strFileName);
               if (extents != null)
               {
                  // Convert Geotif right here to save lockup in update thread due to slow GDI+ to DirectX texture stream
                  if (!File.Exists(m_strCacheFileName) || File.GetLastWriteTime(m_strCacheFileName) < File.GetLastWriteTime(m_strFileName))
                  {
                     using (Image img = Image.FromFile(m_strFileName))
                     {
                        Directory.CreateDirectory(Path.GetDirectoryName(m_strCacheFileName));
                        img.Save(m_strCacheFileName, System.Drawing.Imaging.ImageFormat.Png);
                     }
                  }

                  m_Layer = new ImageLayer(m_strName, m_oWorld, 0.0, m_strCacheFileName, extents.South, extents.North, extents.West, extents.East, m_bOpacity, m_oWorld.TerrainAccessor);
                  m_Layer.IsOn = m_IsOn;
                  m_Layer.Opacity = m_bOpacity;
               }
            }
            catch
            {
               if (File.Exists(m_strCacheFileName))
                  File.Delete(m_strCacheFileName);
               m_Layer = null;
            }
            m_blnIsChanged = false;
         }
         return m_Layer;
      }

      #region IBuilder Members

      public override byte Opacity
      {
         get
         {
            if (m_Layer != null)
               return m_Layer.Opacity;
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
            if (m_Layer != null && m_Layer.Opacity != value)
            {
               m_Layer.Opacity = value;
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
            if (m_Layer != null)
               return m_Layer.IsOn;
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
            if (m_Layer != null && m_Layer.IsOn != value)
            {
               m_Layer.IsOn = value;
               bChanged = true;
            }

            if (bChanged)
               SendBuilderChanged(BuilderChangeType.VisibilityChanged);
         }
      }

      public override string Type
      {
         get { return TypeName; }
      }

      public override bool IsChanged
      {
         get { return m_blnIsChanged; }
      }

      public override bool bIsDownloading(out int iBytesRead, out int iTotalBytes)
      {
         iBytesRead = 0;
         iTotalBytes = 0;
         return false;
      }

      public override string LogoKey
      {
         get { return "georef_image"; }
      }

      #endregion

      public string FileName
      {
         get { return m_strFileName; }
      }

      public override string ServiceType
      {
         get { return "GeoTif Image Layer"; }
      }

      public static string TypeName
      {
         get
         {
            return "GeoTif";
         }
      }

      public override string GetCachePath()
      {
         return Path.Combine(Path.Combine(m_strCacheRoot, CacheSubDir), m_strFileName.GetHashCode().ToString());
      }


      public override string GetURI()
      {
         return URLProtocolName + m_strFileName.Replace(Path.DirectorySeparatorChar, '/');
      }

      public static GeorefImageLayerBuilder GetBuilderFromURI(string uri, string strCacheRoot, World world, IBuilder parent)
      {
         try
         {
            uri = uri.Trim();
            string strFile = uri.Replace(URLProtocolName, "").Replace('/', Path.DirectorySeparatorChar);

            // See if we have a valid geotif first
            GeographicBoundingBox extents = GeorefImageLayerBuilder.GetExtentsFromGeotif(strFile);

            if (extents != null)
               return new GeorefImageLayerBuilder(strCacheRoot, strFile, false, world, parent);
         }
         catch
         {
         }
         return null;
      }

      public override object Clone()
      {
         return new GeorefImageLayerBuilder(m_strCacheRoot, m_strFileName, m_bIsTmp, m_oWorld, m_Parent);
      }

      protected override void CleanUpLayer(bool bFinal)
      {
         if (m_Layer != null)
            m_Layer.Dispose();
         if (File.Exists(m_strCacheFileName))
            File.Delete(m_strCacheFileName);
         if (bFinal && m_bIsTmp && File.Exists(m_strFileName))
            File.Delete(m_strFileName);
         m_Layer = null;
         m_blnIsChanged = true;
      }

      public override GeographicBoundingBox Extents
      {
         get
         {
            if (m_Layer == null)
               GetLayer();
            return new GeographicBoundingBox(m_Layer.MaxLat, m_Layer.MinLat, m_Layer.MinLon, m_Layer.MaxLon);
         }
      }

      public override int ImagePixelSize
      {
         get
         {
            return 256; // TODO Not relevant
         }
         set
         {
         }
      }
   }
}
