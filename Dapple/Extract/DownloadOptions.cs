using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Dapple.Extract
{
   public partial class DownloadOptions : UserControl
   {
      #region Member Variables
      protected Dapple.LayerGeneration.DAPQuadLayerBuilder m_oDAPLayer;
      protected WorldWind.GeographicBoundingBox m_oViewedAoi;
      protected WorldWind.GeographicBoundingBox m_oMapAoi;
      protected string m_strMapProjection;
      #endregion


      /// <summary>
      /// Get the curent viewed area of interest
      /// </summary>
      public WorldWind.GeographicBoundingBox ViewedAoi
      {
         get { return m_oViewedAoi; }
      }

      /// <summary>
      /// Get the thick client open map area of interest
      /// </summary>
      public WorldWind.GeographicBoundingBox MapAoi
      {
         get { return m_oMapAoi; }
      }

      public virtual bool ResolutionEnabled
      {
         set { }
      }

      #region Constructor
      private DownloadOptions()
      {
         InitializeComponent();
      }

      /// <summary>
      /// Default constructor
      /// </summary>
      /// <param name="oMainForm"></param>
      /// <param name="oDAPLayer"></param>
      public DownloadOptions(Dapple.LayerGeneration.DAPQuadLayerBuilder oDAPLayer) : this()
      {
         m_oDAPLayer = oDAPLayer;
         m_oViewedAoi = WorldWind.GeographicBoundingBox.FromQuad(MainForm.WorldWindowSingleton.GetViewBox(false));
         m_oMapAoi = MainForm.MapAoi;
         m_strMapProjection = MainForm.MapAoiCoordinateSystem;
      }
      #endregion

      #region Public Methods
      /// <summary>
      /// Save the current contents of these controls to an xml file
      /// </summary>
      /// <param name="oDatasetElement"></param>
      /// <param name="strDestFolder"></param>
      /// <param name="bDefaultResolution"></param>
      /// <returns></returns>
      public virtual bool Save(System.Xml.XmlElement oDatasetElement, string strDestFolder, DownloadSettings.DownloadClip eClip, DownloadSettings.DownloadCoordinateSystem eCS)
      {
         double dMaxX, dMinX, dMaxY, dMinY;
         double dProjMinX, dProjMinY, dProjMaxX, dProjMaxY;
         string strProjCoordinateSystem;


         // --- save the dataset type ---

         System.Xml.XmlAttribute oTypeAttr = oDatasetElement.OwnerDocument.CreateAttribute("type");
         oTypeAttr.Value = m_oDAPLayer.DAPType;
         oDatasetElement.Attributes.Append(oTypeAttr);

         
         // --- set the server url ---

         System.Xml.XmlAttribute oAttr = oDatasetElement.OwnerDocument.CreateAttribute("url");
         oAttr.Value = m_oDAPLayer.DAPServerURL;
         oDatasetElement.Attributes.Append(oAttr);

         oAttr = oDatasetElement.OwnerDocument.CreateAttribute("id");
         oAttr.Value = m_oDAPLayer.DatasetName;
         oDatasetElement.Attributes.Append(oAttr);

         
         // --- get the dataset coordinate system ---

         string strSrcCoordinateSystem = MainForm.MontajInterface.GetProjection(m_oDAPLayer.DAPServerURL, m_oDAPLayer.DatasetName);
         if (string.IsNullOrEmpty(strSrcCoordinateSystem))
            return true;
         

         // --- get the dataset extents ---

         if (!MainForm.MontajInterface.GetExtents(m_oDAPLayer.DAPServerURL, m_oDAPLayer.DatasetName, out dMaxX, out dMinX, out dMaxY, out dMinY))
            return true;

         
         // --- calculate the extract area ---

         dProjMaxX = dMaxX;
         dProjMaxY = dMaxY;
         dProjMinX = dMinX;
         dProjMinY = dMinY;
         strProjCoordinateSystem = strSrcCoordinateSystem;

         if (eClip == DownloadSettings.DownloadClip.None)
         {
            if (eCS == DownloadSettings.DownloadCoordinateSystem.OriginalMap)
            {
               if (!MainForm.MontajInterface.ProjectBoundingRectangle(strSrcCoordinateSystem, ref dProjMinX, ref dProjMinY, ref dProjMaxX, ref dProjMaxY, m_strMapProjection))
                  return true;

               strProjCoordinateSystem = m_strMapProjection;
            }
         }
         else if (eClip == DownloadSettings.DownloadClip.OriginalMap)
         {
            if (eCS == DownloadSettings.DownloadCoordinateSystem.OriginalMap)
            {
               if (!MainForm.MontajInterface.ProjectBoundingRectangle(strSrcCoordinateSystem, ref dProjMinX, ref dProjMinY, ref dProjMaxX, ref dProjMaxY, m_strMapProjection))
                  return true;

               dProjMaxX = Math.Min(m_oMapAoi.East, dProjMaxX);
               dProjMinX = Math.Max(m_oMapAoi.West, dProjMinX);
               dProjMaxY = Math.Min(m_oMapAoi.North, dProjMaxY);
               dProjMinY = Math.Max(m_oMapAoi.South, dProjMinY);
               strProjCoordinateSystem = m_strMapProjection;
            }
            else
            {
               dProjMaxX = m_oMapAoi.East;
               dProjMinX = m_oMapAoi.West;
               dProjMaxY = m_oMapAoi.North;
               dProjMinY = m_oMapAoi.South;
               if (!MainForm.MontajInterface.ProjectBoundingRectangle(m_strMapProjection, ref dProjMinX, ref dProjMinY, ref dProjMaxX, ref dProjMaxY, strSrcCoordinateSystem))
                  return true;

               dProjMaxX = Math.Min(dMaxX, dProjMaxX);
               dProjMinX = Math.Max(dMinX, dProjMinX);
               dProjMaxY = Math.Min(dMaxY, dProjMaxY);
               dProjMinY = Math.Max(dMinY, dProjMinY);
               strProjCoordinateSystem = strSrcCoordinateSystem;
            }
         }
         else if (eClip == DownloadSettings.DownloadClip.ViewedArea)
         {
            if (eCS == DownloadSettings.DownloadCoordinateSystem.OriginalMap)
            {
               if (!MainForm.MontajInterface.ProjectBoundingRectangle(strSrcCoordinateSystem, ref dProjMinX, ref dProjMinY, ref dProjMaxX, ref dProjMaxY, Resolution.WGS_84))
                  return true;

               dProjMaxX = Math.Min(m_oViewedAoi.East, dProjMaxX);
               dProjMinX = Math.Max(m_oViewedAoi.West, dProjMinX);
               dProjMaxY = Math.Min(m_oViewedAoi.North, dProjMaxY);
               dProjMinY = Math.Max(m_oViewedAoi.South, dProjMinY);

               if (!MainForm.MontajInterface.ProjectBoundingRectangle(Resolution.WGS_84, ref dProjMinX, ref dProjMinY, ref dProjMaxX, ref dProjMaxY, m_strMapProjection))
                  return true;
               
               strProjCoordinateSystem = m_strMapProjection;
            }
            else
            {
               if (!MainForm.MontajInterface.ProjectBoundingRectangle(strSrcCoordinateSystem, ref dProjMinX, ref dProjMinY, ref dProjMaxX, ref dProjMaxY, Resolution.WGS_84))
                  return true;

               dProjMaxX = Math.Min(m_oViewedAoi.East, dProjMaxX);
               dProjMinX = Math.Max(m_oViewedAoi.West, dProjMinX);
               dProjMaxY = Math.Min(m_oViewedAoi.North, dProjMaxY);
               dProjMinY = Math.Max(m_oViewedAoi.South, dProjMinY);

               if (!MainForm.MontajInterface.ProjectBoundingRectangle(Resolution.WGS_84, ref dProjMinX, ref dProjMinY, ref dProjMaxX, ref dProjMaxY, strSrcCoordinateSystem))
                  return true;

               strProjCoordinateSystem = strSrcCoordinateSystem;
            }
         }

         
         // --- save the extents and coordinate system ---

         oAttr = oDatasetElement.OwnerDocument.CreateAttribute("minx");
         oAttr.Value = dProjMinX.ToString();
         oDatasetElement.Attributes.Append(oAttr);

         oAttr = oDatasetElement.OwnerDocument.CreateAttribute("miny");
         oAttr.Value = dProjMinY.ToString();
         oDatasetElement.Attributes.Append(oAttr);

         oAttr = oDatasetElement.OwnerDocument.CreateAttribute("maxx");
         oAttr.Value = dProjMaxX.ToString();
         oDatasetElement.Attributes.Append(oAttr);

         oAttr = oDatasetElement.OwnerDocument.CreateAttribute("maxy");
         oAttr.Value = dProjMaxY.ToString();
         oDatasetElement.Attributes.Append(oAttr);

         oAttr = oDatasetElement.OwnerDocument.CreateAttribute("coordinate_system");
         oAttr.Value = strProjCoordinateSystem;
         oDatasetElement.Attributes.Append(oAttr);
         return true;
      }

      public virtual void SetDefaultResolution()
      {
      }

      public virtual void SetNativeResolution()
      {
      }
      #endregion
   }
}
