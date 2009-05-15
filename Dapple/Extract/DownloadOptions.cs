using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Globalization;

namespace Dapple.Extract
{
	internal partial class DownloadOptions : UserControl
	{
		#region Enums

		/// <summary>
		/// The result of attempting to extract a dataset.
		/// </summary>
		internal enum ExtractSaveResult
		{
			/// <summary>
			/// Save was successful, tell DappleGetData to download the item.
			/// </summary>
			Extract,
			/// <summary>
			/// Don't donwload the item.
			/// </summary>
			Ignore,
			/// <summary>
			/// Error during save, abort extract command and switch to that dataset to correct problems.
			/// </summary>
			Cancel
		};

		internal enum DuplicateFileCheckResult
		{
			Yes,
			YesAndStopAsking,
			No
		};

		#endregion

		#region Member Variables
		protected Dapple.LayerGeneration.DAPQuadLayerBuilder m_oDAPLayer;
      protected WorldWind.GeographicBoundingBox m_oViewedAoi;
      protected WorldWind.GeographicBoundingBox m_oMapAoi;
      protected string m_strMapProjection;
		protected string m_strLayerProjection;
		protected ErrorProvider m_oErrorProvider;
      #endregion


      /// <summary>
      /// Get the curent viewed area of interest
      /// </summary>
      internal WorldWind.GeographicBoundingBox ViewedAoi
      {
         get { return m_oViewedAoi; }
      }

      /// <summary>
      /// Get the thick client open map area of interest
      /// </summary>
      internal WorldWind.GeographicBoundingBox MapAoi
      {
         get { return m_oMapAoi; }
      }

      internal virtual bool ResolutionEnabled
      {
         set { }
      }

		/// <summary>
		/// If this is false, the download XML will set blNewMap to false always.
		/// </summary>
		internal virtual bool OpenInMap
		{
			get { return true; }
		}

		[DefaultValue(null)]
		[Description("The ErrorProvider used to notify users of errors.")]
		[Browsable(true)]
		[Category("Behavior")]
		internal virtual ErrorProvider ErrorProvider
		{
			get { return m_oErrorProvider; }
			set { m_oErrorProvider = value; }
		}

		internal virtual DuplicateFileCheckResult CheckForDuplicateFiles(String szExtractDirectory, Form hExtractForm)
		{
			return DuplicateFileCheckResult.Yes;
		}

		protected static DuplicateFileCheckResult QueryOverwriteFile(String szMessage, Form hExtractForm)
		{
			OverwriteDialog oChecker = new OverwriteDialog(szMessage, hExtractForm);
			switch (oChecker.ShowDialog())
			{
				case DialogResult.OK:
					return DuplicateFileCheckResult.Yes;
				case DialogResult.Cancel:
					return DuplicateFileCheckResult.No;
				case DialogResult.Ignore:
					return DuplicateFileCheckResult.YesAndStopAsking;
			}
			return DuplicateFileCheckResult.No;
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
      internal DownloadOptions(Dapple.LayerGeneration.DAPQuadLayerBuilder oDAPLayer) : this()
      {
         m_oDAPLayer = oDAPLayer;
         m_oViewedAoi = WorldWind.GeographicBoundingBox.FromQuad(MainForm.WorldWindowSingleton.CurrentAreaOfInterest);
         m_oMapAoi = MainForm.MapAoi;
         m_strMapProjection = MainForm.MapAoiCoordinateSystem;
			m_strLayerProjection = MainForm.MontajInterface.GetProjection(m_oDAPLayer.ServerURL, m_oDAPLayer.DatasetName);
      }
      #endregion

		#region Properties

		public string Projection
		{
			get { return m_strLayerProjection; }
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
      internal virtual ExtractSaveResult Save(System.Xml.XmlElement oDatasetElement, string strDestFolder, DownloadSettings.DownloadCoordinateSystem eCS)
      {
         double dMaxX, dMinX, dMaxY, dMinY;
         double dProjMinX, dProjMinY, dProjMaxX, dProjMaxY;
         string strProjCoordinateSystem;
         bool bNewMap = true;
         bool bInvalidReprojection = false;


         // --- save the dataset type ---

         System.Xml.XmlAttribute oTypeAttr = oDatasetElement.OwnerDocument.CreateAttribute("type");
         oTypeAttr.Value = m_oDAPLayer.DAPType;
         oDatasetElement.Attributes.Append(oTypeAttr);

         
         // --- set the server url ---

         System.Xml.XmlAttribute oAttr = oDatasetElement.OwnerDocument.CreateAttribute("title");
         oAttr.Value = m_oDAPLayer.Title;
         oDatasetElement.Attributes.Append(oAttr);

         oAttr = oDatasetElement.OwnerDocument.CreateAttribute("url");
         oAttr.Value = m_oDAPLayer.ServerURL;
         oDatasetElement.Attributes.Append(oAttr);

         oAttr = oDatasetElement.OwnerDocument.CreateAttribute("id");
         oAttr.Value = m_oDAPLayer.DatasetName;
         oDatasetElement.Attributes.Append(oAttr);

         
         // --- get the dataset coordinate system ---

         string strSrcCoordinateSystem = m_strLayerProjection;
         if (string.IsNullOrEmpty(strSrcCoordinateSystem))
            return ExtractSaveResult.Ignore;

         // --- get the dataset extents ---

         if (!MainForm.MontajInterface.GetExtents(m_oDAPLayer.ServerURL, m_oDAPLayer.DatasetName, out dMaxX, out dMinX, out dMaxY, out dMinY))
				return ExtractSaveResult.Ignore;

         // --- Sanity check on the data ---

         double dMapInWGS84_MinX = dMinX;
         double dMapInWGS84_MinY = dMinY;
         double dMapInWGS84_MaxX = dMaxX;
         double dMapInWGS84_MaxY = dMaxY;
         if (MainForm.MontajInterface.ProjectBoundingRectangle(strSrcCoordinateSystem, ref dMapInWGS84_MinX, ref dMapInWGS84_MinY, ref dMapInWGS84_MaxX, ref dMapInWGS84_MaxY, Resolution.WGS_84))
         {
            if (Math.Abs(m_oDAPLayer.m_hDataSet.Boundary.MinX - dMapInWGS84_MinX) > 0.01 ||
					Math.Abs(m_oDAPLayer.m_hDataSet.Boundary.MinY - dMapInWGS84_MinY) > 0.01 ||
					Math.Abs(m_oDAPLayer.m_hDataSet.Boundary.MaxX - dMapInWGS84_MaxX) > 0.01 ||
					Math.Abs(m_oDAPLayer.m_hDataSet.Boundary.MaxY - dMapInWGS84_MaxY) > 0.01)
            {
					Geosoft.Dap.Common.BoundingBox oReprojectedBox = new Geosoft.Dap.Common.BoundingBox(dMapInWGS84_MaxX, dMapInWGS84_MaxY, dMapInWGS84_MinX, dMapInWGS84_MinY);

					Program.ShowMessageBox(
						"A problem was encountered while preparing to download dataset " + m_oDAPLayer.Title + "\n" +
						"The WGS 84 bounding box advertised by the server:\n" +
						m_oDAPLayer.m_hDataSet.Boundary.ToString(2) + "\n" +
						"does not match up with the reprojected extents of the layer's metadata:\n" +
						oReprojectedBox.ToString(2) + "\n" +
						"The dataset will not be downloaded.  Contact the server administrator.",
						"Extract Datasets",
						MessageBoxButtons.OK,
						MessageBoxDefaultButton.Button1,
						MessageBoxIcon.Error);
					return ExtractSaveResult.Ignore;
            }
         }

         // End sanity check.  Insanity may resume.

         
         // --- calculate the extract area ---

         dProjMaxX = dMaxX;
         dProjMaxY = dMaxY;
         dProjMinX = dMinX;
         dProjMinY = dMinY;
         strProjCoordinateSystem = strSrcCoordinateSystem;

         if (MainForm.MontajInterface.ProjectBoundingRectangle(strSrcCoordinateSystem, ref dProjMinX, ref dProjMinY, ref dProjMaxX, ref dProjMaxY, Resolution.WGS_84))
         {
            dProjMaxX = Math.Min(m_oViewedAoi.East, dProjMaxX);
            dProjMinX = Math.Max(m_oViewedAoi.West, dProjMinX);
            dProjMaxY = Math.Min(m_oViewedAoi.North, dProjMaxY);
            dProjMinY = Math.Max(m_oViewedAoi.South, dProjMinY);

            if (eCS == DownloadSettings.DownloadCoordinateSystem.OriginalMap)
            {
               if (MainForm.MontajInterface.ProjectBoundingRectangle(Resolution.WGS_84, ref dProjMinX, ref dProjMinY, ref dProjMaxX, ref dProjMaxY, m_strMapProjection))
                  strProjCoordinateSystem = m_strMapProjection;
               else
                  bInvalidReprojection = true;
            }
            else
            {
               if (MainForm.MontajInterface.ProjectBoundingRectangle(Resolution.WGS_84, ref dProjMinX, ref dProjMinY, ref dProjMaxX, ref dProjMaxY, strSrcCoordinateSystem))
                  strProjCoordinateSystem = strSrcCoordinateSystem;
               else
                  bInvalidReprojection = true;
            }
         }
         else
         {
            bInvalidReprojection = true;
         }

         // --- check to see if we require a new ---

         if (!bInvalidReprojection && MainForm.MontajInterface.HostHasOpenMap())
         {
            bNewMap = !IntersectMap(ref dProjMinX, ref dProjMinY, ref dProjMaxX, ref dProjMaxY, strProjCoordinateSystem);
         }


         // --- check to see if this is a valid bounding box ---

         if (bInvalidReprojection || !(dProjMaxX > dProjMinX && dProjMaxY > dProjMinY))
         {
            // --- invalid box ---

            dProjMaxX = dMaxX;
            dProjMaxY = dMaxY;
            dProjMinX = dMinX;
            dProjMinY = dMinY;
            strProjCoordinateSystem = strSrcCoordinateSystem;
            bNewMap = true;
         }
         
         // --- save the extents and coordinate system ---

         oAttr = oDatasetElement.OwnerDocument.CreateAttribute("new_map");
         oAttr.Value = (bNewMap && OpenInMap).ToString();
         oDatasetElement.Attributes.Append(oAttr);

         oAttr = oDatasetElement.OwnerDocument.CreateAttribute("minx");
			oAttr.Value = dProjMinX.ToString("R", CultureInfo.InvariantCulture);
         oDatasetElement.Attributes.Append(oAttr);

         oAttr = oDatasetElement.OwnerDocument.CreateAttribute("miny");
			oAttr.Value = dProjMinY.ToString("R", CultureInfo.InvariantCulture);
         oDatasetElement.Attributes.Append(oAttr);

         oAttr = oDatasetElement.OwnerDocument.CreateAttribute("maxx");
			oAttr.Value = dProjMaxX.ToString("R", CultureInfo.InvariantCulture);
         oDatasetElement.Attributes.Append(oAttr);

         oAttr = oDatasetElement.OwnerDocument.CreateAttribute("maxy");
			oAttr.Value = dProjMaxY.ToString("R", CultureInfo.InvariantCulture);
         oDatasetElement.Attributes.Append(oAttr);

         oAttr = oDatasetElement.OwnerDocument.CreateAttribute("coordinate_system");
         oAttr.Value = strProjCoordinateSystem;
         oDatasetElement.Attributes.Append(oAttr);

			if (m_oDAPLayer != null)
			{
				oAttr = oDatasetElement.OwnerDocument.CreateAttribute("meta_stylesheet_name");
				oAttr.Value = m_oDAPLayer.StyleSheetID;
				oDatasetElement.Attributes.Append(oAttr);
			}

#if DEBUG
         double dMapBoundMinX_WGS84 = dMinX;
         double dMapBoundMaxX_WGS84 = dMaxX;
         double dMapBoundMinY_WGS84 = dMinY;
         double dMapBoundMaxY_WGS84 = dMaxY;

         double dClipBoundMinX_WGS84 = dProjMinX;
         double dClipBoundMaxX_WGS84 = dProjMaxX;
         double dClipBoundMinY_WGS84 = dProjMinY;
         double dClipBoundMaxY_WGS84 = dProjMaxY;

         MainForm.MontajInterface.ProjectBoundingRectangle(strSrcCoordinateSystem, ref dMapBoundMinX_WGS84, ref dMapBoundMinY_WGS84, ref dMapBoundMaxX_WGS84, ref dMapBoundMaxY_WGS84, Resolution.WGS_84);
         MainForm.MontajInterface.ProjectBoundingRectangle(strProjCoordinateSystem, ref dClipBoundMinX_WGS84, ref dClipBoundMinY_WGS84, ref dClipBoundMaxX_WGS84, ref dClipBoundMaxY_WGS84, Resolution.WGS_84);

         oDatasetElement.SetAttribute("map_wgs84_west", dMapBoundMinX_WGS84.ToString("f5", CultureInfo.InvariantCulture));
			oDatasetElement.SetAttribute("map_wgs84_south", dMapBoundMinY_WGS84.ToString("f5", CultureInfo.InvariantCulture));
			oDatasetElement.SetAttribute("map_wgs84_east", dMapBoundMaxX_WGS84.ToString("f5", CultureInfo.InvariantCulture));
			oDatasetElement.SetAttribute("map_wgs84_north", dMapBoundMaxY_WGS84.ToString("f5", CultureInfo.InvariantCulture));

			oDatasetElement.SetAttribute("clip_wgs84_west", dClipBoundMinX_WGS84.ToString("f5", CultureInfo.InvariantCulture));
			oDatasetElement.SetAttribute("clip_wgs84_south", dClipBoundMinY_WGS84.ToString("f5", CultureInfo.InvariantCulture));
			oDatasetElement.SetAttribute("clip_wgs84_east", dClipBoundMaxX_WGS84.ToString("f5", CultureInfo.InvariantCulture));
			oDatasetElement.SetAttribute("clip_wgs84_north", dClipBoundMaxY_WGS84.ToString("f5", CultureInfo.InvariantCulture));
#endif

			return ExtractSaveResult.Extract;
      }

      internal virtual void SetDefaultResolution()
      {
      }

      internal virtual void SetNativeResolution()
      {
      }
      #endregion

      #region Private Methods
      /// <summary>
      /// Get the intersection bounding box of this area with the open map
      /// </summary>
      /// <param name="dMinX"></param>
      /// <param name="dMinY"></param>
      /// <param name="dMaxX"></param>
      /// <param name="dMaxY"></param>
      /// <returns></returns>
      protected bool IntersectMap(ref double dMinX, ref double dMinY, ref double dMaxX, ref double dMaxY, string strCoordinateSystem)
      {
         double dTempMinX = dMinX;
         double dTempMinY = dMinY;
         double dTempMaxX = dMaxX;
         double dTempMaxY = dMaxY;

         if (MainForm.MontajInterface.ProjectBoundingRectangle(strCoordinateSystem, ref dTempMinX, ref dTempMinY, ref dTempMaxX, ref dTempMaxY, m_strMapProjection))
         {
            dTempMaxX = Math.Min(m_oMapAoi.East, dTempMaxX);
            dTempMinX = Math.Max(m_oMapAoi.West, dTempMinX);
            dTempMaxY = Math.Min(m_oMapAoi.North, dTempMaxY);
            dTempMinY = Math.Max(m_oMapAoi.South, dTempMinY);

            if (dTempMaxX >= dTempMinX && dTempMaxY >= dTempMinY)
            {
               if (MainForm.MontajInterface.ProjectBoundingRectangle(m_strMapProjection, ref dTempMinX, ref dTempMinY, ref dTempMaxX, ref dTempMaxY, strCoordinateSystem))
               {
                  dMinX = dTempMinX;
                  dMaxX = dTempMaxX;
                  dMinY = dTempMinY;
                  dMaxY = dTempMaxY;
                  return true;
               }
            }
         }
         return false;
      }
      #endregion
   }
}
