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
   internal partial class Resolution : UserControl
   {
      //internal static string WGS_84 = "GEOGCS[\"GCS_WGS_1984\",DATUM[\"D_WGS_1984\",SPHEROID[\"WGS_1984\",6378137,298.257223563]],PRIMEM[\"Greenwich\",0],UNIT[\"Degree\",0.017453292519943295]]";
		internal static string WGS_84 = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><projection type=\"GEOGRAPHIC\" name=\"WGS 84\" ellipsoid=\"WGS 84\" datum=\"WGS 84\" datumtrf=\"WGS 84\" datumtrf_description=\"[WGS 84] World\" radius=\"6378137\" eccentricity=\"0.081819190842621486\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:geo=\"http://www.geosoft.com/schema/geo\"><geo:units name=\"dega\" unit_scale=\"1\"></geo:units></projection>";
      private DownloadOptions m_oDownloadOptions;
      
      private string m_strDatasetProjection;

      private bool   m_bLatLon = false;
      private double m_dResolutionConversionFactor = 1;
      private double m_dCellConversionFactor = 1;
      private string m_strResolutionUnits = string.Empty;
      private string m_strCellUnits = string.Empty;
		private double m_dMinResolution;
		private double m_dMaxResolution;
		private ErrorProvider m_oErrorProvider;

      private double m_dPercentOverlap = 1.0;

      private double m_dResolution;
      private double m_dCellSizeX;
      private double m_dCellSizeY;
      private double m_dCellSize;
      private int    m_iSizeX;
      private int    m_iSizeY;
      private int    m_iSize;
      private double m_dIncrement;

      private SortedList<double, int>   m_oResolutions;

      private SortedList<double, int> m_oXDimension = null;
      private SortedList<double, int> m_oYDimension = null;
      private SortedList<double, int> m_oZDimension = null;

      private bool m_bPicture = false;
      private bool   m_bVoxel = false;
      private bool   m_bMap = false;
      private bool   m_bTextChange = false;

      #region Properties
      /// <summary>
      /// Get the resolution
      /// </summary>
      private double ResolutionValue
      {
         get { return m_dResolution; }
      }

      internal double ResolutionValueSpecific(DownloadSettings.DownloadCoordinateSystem eCS)
      {
         if (eCS == DownloadSettings.DownloadCoordinateSystem.OriginalMap)
         {
            return ResolutionValue;
         }
         else
         {
            return (ResolutionValue * m_dResolutionConversionFactor) / m_dCellConversionFactor;
         }
      }

		[DefaultValue(null)]
		[Description("The ErrorProvider used to notify users of errors.")]
		[Browsable(true)]
		[Category("Behavior")]
		internal ErrorProvider ErrorProvider
		{
			get { return m_oErrorProvider; }
			set { m_oErrorProvider = value; }
		}
      #endregion

      internal Resolution()
      {
         InitializeComponent();
      }

      internal void SetDownloadOptions(DownloadOptions oOptions)
      {
         m_oDownloadOptions = oOptions;
      }

      /// <summary>
      /// Default constructor
      /// </summary>
      /// <param name="dResolution"></param>
      /// <param name="iSizeX"></param>
      /// <param name="iSizeY"></param>
      /// <param name="dXCellSize"></param>
      /// <param name="dYCellSize"></param>
      internal void Setup(bool bPicture, string strCoordinateSystem, double dXOrigin, double dYOrigin, int iSizeX, int iSizeY, double dXCellSize, double dYCellSize)
      {         
         double               dConv = 1;
         bool bLatLong;

         try
         {
            m_bPicture = bPicture;
            m_strDatasetProjection = strCoordinateSystem;

            if (!string.IsNullOrEmpty(MainForm.MapAoiCoordinateSystem))
            {
               MainForm.MontajInterface.GetResolutionUnit(MainForm.MapAoiCoordinateSystem, out m_dResolutionConversionFactor, out m_strResolutionUnits, out m_bLatLon, ref dConv);
               lUnit.Text = m_strResolutionUnits;
            }

            MainForm.MontajInterface.GetResolutionUnit(m_strDatasetProjection, out m_dCellConversionFactor, out m_strCellUnits, out bLatLong, ref dConv);
            if (string.IsNullOrEmpty(MainForm.MapAoiCoordinateSystem))
            {
               m_dResolutionConversionFactor = m_dCellConversionFactor;
               m_bLatLon = bLatLong;
               lUnit.Text = m_strCellUnits;
            }

            m_dCellSizeX = dXCellSize;
            m_dCellSizeY = dYCellSize;
            m_iSizeX = iSizeX;
            m_iSizeY = iSizeY;

            
            CalculateExtents(dXOrigin, dYOrigin);

            // --- use the max size as the basis to calculate resolution from ---

            if (iSizeX > iSizeY)
            {
               m_iSize = iSizeX;
               m_dCellSize = dXCellSize;            
            } 
            else 
            {
               m_iSize = iSizeY;
               m_dCellSize = dYCellSize;            
            }           

            m_bTextChange = true;


            // --- see if the resolution is valid ---

            if (!ValidResolution())
            {
               m_dResolution = Round(m_dResolution);
               if (m_dResolution < Round((m_dCellSize * m_dCellConversionFactor) / m_dResolutionConversionFactor))
                  m_dResolution = Round((m_dCellSize * m_dCellConversionFactor) / m_dResolutionConversionFactor);
               else if  (m_dResolution > Round((m_iSize * m_dCellSize * m_dCellConversionFactor) / m_dResolutionConversionFactor))
                  m_dResolution = Round((m_iSize * m_dCellSize * m_dCellConversionFactor) / m_dResolutionConversionFactor);
            }
            
            // --- figure out a log scale for the slider bar ---

            m_dIncrement = Math.Log(m_iSize, Math.E) / 100;

            if (m_bLatLon) {
               tbRes.Text = DisplayInMetres();
					m_dMinResolution = Round(m_dCellSize * m_dCellConversionFactor);
					lMinResolution.Text = m_dMinResolution.ToString(CultureInfo.CurrentCulture);
					m_dMaxResolution = Round(m_iSize * m_dCellSize * m_dCellConversionFactor);
					lMaxResolution.Text = m_dMaxResolution.ToString(CultureInfo.CurrentCulture);
            } else {
					tbRes.Text = m_dResolution.ToString(CultureInfo.CurrentCulture);
					m_dMinResolution = Round((m_dCellSize * m_dCellConversionFactor) / m_dResolutionConversionFactor);
					lMinResolution.Text = m_dMinResolution.ToString(CultureInfo.CurrentCulture);
					m_dMaxResolution = Round((m_iSize * m_dCellSize * m_dCellConversionFactor) / m_dResolutionConversionFactor);
					lMaxResolution.Text = m_dMaxResolution.ToString(CultureInfo.CurrentCulture);
            }

            CalculateScrollPosition();
            CalculateSize();

            m_bTextChange = false;
         } 
         catch {}         
      }

      /// <summary>
      /// Default constructor, used when we have a list of valid resolutions
      /// </summary>
      /// <param name="dResolution"></param>
      /// <param name="iSizeX"></param>
      /// <param name="iSizeY"></param>
      /// <param name="dXCellSize"></param>
      /// <param name="dYCellSize"></param>
      internal void Setup(string strCoordinateSystem, double dMinX, double dMinY, double dMaxX, double dMaxY, SortedList<double, int> oResolutions, SortedList<double, int> oXDimension, SortedList<double, int> oYDimension, SortedList<double, int> oZDimension)
      {
         m_bVoxel = true;
         m_strDatasetProjection = strCoordinateSystem;
         m_oXDimension = oXDimension;
         m_oYDimension = oYDimension;
         m_oZDimension = oZDimension;

         Init(dMinX, dMinY, dMaxX, dMaxY, oResolutions);
      }

      /// <summary>
      /// Default constructor, used when we have a list of valid resolutions
      /// </summary>
      /// <param name="dResolution"></param>
      /// <param name="iSizeX"></param>
      /// <param name="iSizeY"></param>
      /// <param name="dXCellSize"></param>
      /// <param name="dYCellSize"></param>
      internal void Setup(string strCoordinateSystem, double dMinX, double dMinY, double dMaxX, double dMaxY, SortedList<double, int> oResolutions)
      {         
         m_bMap = true;
         m_strDatasetProjection = strCoordinateSystem;
         Init(dMinX, dMinY, dMaxX, dMaxY, oResolutions);
      }

      /// <summary>
      /// Set the native resolution for this dataset
      /// </summary>
      internal void SetNativeResolution()
      {
         tbResolution.Value = 0;
         tbResolution_Scroll(this, new EventArgs());
      }

      /// <summary>
      /// Setup the dialog
      /// </summary>
      /// <param name="dResolution"></param>
      /// <param name="iSizeX"></param>
      /// <param name="iSizeY"></param>
      /// <param name="dXCellSize"></param>
      /// <param name="dYCellSize"></param>
      private void Init(double dMinX, double dMinY, double dMaxX, double dMaxY, SortedList<double, int> oResolutions)
      {         
         double               dConv = 1;
         bool bLatLon;

         tbRes.Enabled = false;

         try
         {
            if (!string.IsNullOrEmpty(MainForm.MapAoiCoordinateSystem))
            {
               MainForm.MontajInterface.GetResolutionUnit(MainForm.MapAoiCoordinateSystem, out m_dResolutionConversionFactor, out m_strResolutionUnits, out m_bLatLon, ref dConv);
               lUnit.Text = m_strResolutionUnits;
            }

            MainForm.MontajInterface.GetResolutionUnit(m_strDatasetProjection, out m_dCellConversionFactor, out m_strCellUnits, out bLatLon, ref dConv);
            if (string.IsNullOrEmpty(MainForm.MapAoiCoordinateSystem))
            {
               m_bLatLon = bLatLon;
               m_dResolutionConversionFactor = m_dCellConversionFactor;
               lUnit.Text = m_strCellUnits;
            }
                        
            m_oResolutions = oResolutions;
            
            tbResolution.Maximum = m_oResolutions.Count - 1;
            tbResolution.Minimum = 0;
            tbResolution.TickFrequency = 1;

				m_dMinResolution = Round((double)m_oResolutions.Keys[0] * m_dCellConversionFactor);
				lMinResolution.Text = m_dMinResolution.ToString(CultureInfo.CurrentCulture);
				m_dMaxResolution = Round((double)m_oResolutions.Keys[m_oResolutions.Count - 1] * m_dCellConversionFactor);
            lMaxResolution.Text = m_dMaxResolution.ToString(CultureInfo.CurrentCulture);

            
            // --- calculate the overlap extents ---

            CalculateExtents(dMinX, dMinY, dMaxX, dMaxY);


            m_bTextChange = true;

            if (m_bLatLon)
               tbRes.Text = DisplayInMetres();
            else
					tbRes.Text = m_dResolution.ToString(CultureInfo.CurrentCulture);

            CalculateScrollPosition();
            CalculateSize();

            m_bTextChange = false;
         } 
         catch {}      
      }

      #region Protected Methods
      /// <summary>
      /// Calculate the resolution
      /// </summary>
      /// <param name="dMinX"></param>
      /// <param name="dMinY"></param>
      /// <param name="dMaxX"></param>
      /// <param name="dMaxY"></param>
      protected void CalculateResolution(double dMinX, double dMinY, double dMaxX, double dMaxY)
      {
         double dRes = 250.0;

         if (m_bPicture)
            dRes /= 3.0;
         else if (m_bVoxel)
            dRes /= 2.0;

         if ((dMaxX - dMinX) > (dMaxY - dMinY))
            dRes = (dMaxX - dMinX) / dRes;
         else
            dRes = (dMaxY - dMinY) / dRes;

         // ---- calculate the resolution to 4 significant decimal places ---

			m_dResolution = Math.Floor(dRes * 10000.0) / 10000.0;

         if (m_bLatLon)
            tbRes.Text = DisplayInMetres();
         else
				tbRes.Text = m_dResolution.ToString(CultureInfo.CurrentCulture);
      }

      /// <summary>
      /// Calculate the actual size of the that will be downloaded in this area
      /// </summary>
      protected void CalculateExtents(double dOriginX, double dOriginY)
      {
         double dMaxX;
         double dMinX;
         double dMinY;
         double dMaxY;
         double dMaxXDest;
         double dMinXDest;
         double dMinYDest;
         double dMaxYDest;
         double dExtractMinX;
         double dExtractMinY;
         double dExtractMaxX;
         double dExtractMaxY;
         double dPercentWidth;
         double dPercentHeight;

         try
         {

            // --- get the original extents ---

            dMinX = dOriginX;
            dMaxX = dOriginX + m_iSizeX * m_dCellSizeX;
            dMinY = dOriginY;
            dMaxY = dOriginY + m_iSizeY * m_dCellSizeY;
            
            dMinXDest = dMinX;
            dMinYDest = dMinY;
            dMaxXDest = dMaxX;
            dMaxYDest = dMaxY;

            dExtractMinX = m_oDownloadOptions.ViewedAoi.West;
            dExtractMaxX = m_oDownloadOptions.ViewedAoi.East;
            dExtractMinY = m_oDownloadOptions.ViewedAoi.South;
            dExtractMaxY = m_oDownloadOptions.ViewedAoi.North;

            if (!string.IsNullOrEmpty(MainForm.MapAoiCoordinateSystem))
            {
               if (!MainForm.MontajInterface.ProjectBoundingRectangle(m_strDatasetProjection, ref dMinXDest, ref dMinYDest, ref dMaxXDest, ref dMaxYDest, MainForm.MapAoiCoordinateSystem))
               {
                  CalculateResolution(dOriginX, dOriginY, dOriginX + m_iSizeX * m_dCellSizeX, dOriginY + m_iSizeY * m_dCellSizeY);
                  return;
               }

               if (!MainForm.MontajInterface.ProjectBoundingRectangle(WGS_84, ref dExtractMinX, ref dExtractMinY, ref dExtractMaxX, ref dExtractMaxY, MainForm.MapAoiCoordinateSystem))
               {
                  CalculateResolution(dOriginX, dOriginY, dOriginX + m_iSizeX * m_dCellSizeX, dOriginY + m_iSizeY * m_dCellSizeY);
                  return;
               }

               dExtractMinX = Math.Max(m_oDownloadOptions.MapAoi.West, dExtractMinX);
               dExtractMaxX = Math.Min(m_oDownloadOptions.MapAoi.East, dExtractMaxX);
               dExtractMinY = Math.Max(m_oDownloadOptions.MapAoi.South, dExtractMinY);
               dExtractMaxY = Math.Min(m_oDownloadOptions.MapAoi.North, dExtractMaxY);
            }
            else
            {
               if (!MainForm.MontajInterface.ProjectBoundingRectangle(WGS_84, ref dExtractMinX, ref dExtractMinY, ref dExtractMaxX, ref dExtractMaxY, m_strDatasetProjection))
               {
                  CalculateResolution(dOriginX, dOriginY, dOriginX + m_iSizeX * m_dCellSizeX, dOriginY + m_iSizeY * m_dCellSizeY);
                  return;
               }
            }

            // --- save the dataset extents ---

            dMinX = dMinXDest;
            dMinY = dMinYDest;
            dMaxX = dMaxXDest;
            dMaxY = dMaxYDest;
                       

            // --- calculate the overlap bounding box ---

            dMinXDest = Math.Max(dExtractMinX, dMinXDest);
            dMaxXDest = Math.Min(dExtractMaxX, dMaxXDest);
            dMinYDest = Math.Max(dExtractMinY, dMinYDest);
            dMaxYDest = Math.Min(dExtractMaxY, dMaxYDest);


            CalculateResolution(dMinXDest, dMinYDest, dMaxXDest, dMaxYDest);


            // --- calculate the amount of overlap ---

            dPercentWidth = (dMaxXDest - dMinXDest)/(dMaxX - dMinX);
            dPercentHeight = (dMaxYDest - dMinYDest)/(dMaxY - dMinY);

            dPercentWidth = Math.Max(0, Math.Min(dPercentWidth, 1));
            dPercentHeight = Math.Max(0, Math.Min(dPercentHeight, 1));            

            m_iSizeX = Convert.ToInt32(m_iSizeX * dPercentWidth);
            m_iSizeY = Convert.ToInt32(m_iSizeY * dPercentHeight);
         } 
         catch {}         
      }

      /// <summary>
      /// Calculate the actual size of the that will be downloaded in this area
      /// </summary>
      protected void CalculateExtents(double dMinX, double dMinY, double dMaxX, double dMaxY)
      {
         double dMaxXDest;
         double dMinXDest;
         double dMinYDest;
         double dMaxYDest;
         double dExtractMinX;
         double dExtractMinY;
         double dExtractMaxX;
         double dExtractMaxY;
         
         try
         {
            dMinXDest = dMinX;
            dMinYDest = dMinY;
            dMaxXDest = dMaxX;
            dMaxYDest = dMaxY;

            dExtractMinX = m_oDownloadOptions.ViewedAoi.West;
            dExtractMaxX = m_oDownloadOptions.ViewedAoi.East;
            dExtractMinY = m_oDownloadOptions.ViewedAoi.South;
            dExtractMaxY = m_oDownloadOptions.ViewedAoi.North;

            if (!string.IsNullOrEmpty(MainForm.MapAoiCoordinateSystem))
            {
               if (!MainForm.MontajInterface.ProjectBoundingRectangle(m_strDatasetProjection, ref dMinXDest, ref dMinYDest, ref dMaxXDest, ref dMaxYDest, MainForm.MapAoiCoordinateSystem))
               {
                  CalculateResolution(dMinX, dMinY, dMaxX, dMaxY);
                  return;
               }


               if (!MainForm.MontajInterface.ProjectBoundingRectangle(WGS_84, ref dExtractMinX, ref dExtractMinY, ref dExtractMaxX, ref dExtractMaxY, MainForm.MapAoiCoordinateSystem))
               {
                  CalculateResolution(dMinX, dMinY, dMaxX, dMaxY);
                  return;
               }

               dExtractMinX = Math.Max(m_oDownloadOptions.MapAoi.West, dExtractMinX);
               dExtractMaxX = Math.Min(m_oDownloadOptions.MapAoi.East, dExtractMaxX);
               dExtractMinY = Math.Max(m_oDownloadOptions.MapAoi.South, dExtractMinY);
               dExtractMaxY = Math.Min(m_oDownloadOptions.MapAoi.North, dExtractMaxY);
            }
            else
            {
               if (!MainForm.MontajInterface.ProjectBoundingRectangle(WGS_84, ref dExtractMinX, ref dExtractMinY, ref dExtractMaxX, ref dExtractMaxY, m_strDatasetProjection))
               {
                  CalculateResolution(dMinX, dMinY, dMaxX, dMaxY);
                  return;
               }
            }

            // --- save the dataset extents ---

            dMinX = dMinXDest;
            dMinY = dMinYDest;
            dMaxX = dMaxXDest;
            dMaxY = dMaxYDest;


            // --- calculate the overlap bounding box ---

            dMinXDest = Math.Max(dExtractMinX, dMinXDest);
            dMaxXDest = Math.Min(dExtractMaxX, dMaxXDest);
            dMinYDest = Math.Max(dExtractMinY, dMinYDest);
            dMaxYDest = Math.Min(dExtractMaxY, dMaxYDest);

            
            CalculateResolution(dMinXDest, dMinYDest, dMaxXDest, dMaxYDest);

            
            m_dPercentOverlap = ((dMaxXDest - dMinXDest) * (dMaxYDest - dMinYDest)) / ((dMaxX - dMinX) * (dMaxY - dMinY));
            m_dPercentOverlap = Math.Max(0, Math.Min(m_dPercentOverlap, 1));
         } 
         catch {}         
      }

      /// <summary>
      /// Display resolution in meters
      /// </summary>
      /// <returns></returns>
      protected string DisplayInMetres()
      {
         double dRes = m_dResolution * m_dResolutionConversionFactor;

         dRes = Round(dRes);
			return dRes.ToString(CultureInfo.CurrentCulture);
      }

      /// <summary>
      /// Round to 4 significant digits
      /// </summary>
      /// <param name="dRes"></param>
      /// <returns></returns>
      protected double Round(double dRes)
      {
			return Math.Floor(dRes * 10000.0) / 10000.0;
      }

      /// <summary>
      /// See if the resolution entered is valid
      /// </summary>
      /// <returns></returns>
      protected bool ValidResolution()
      {
         bool     bRet = true;
         double   dRes;
         
         if (m_bMap || m_bVoxel)
            return true;

         if (tbRes.Text == string.Empty)
            return false;

         try
         {            
            dRes = Convert.ToDouble(tbRes.Text, CultureInfo.InvariantCulture);

            if (m_bLatLon)
               dRes /= m_dResolutionConversionFactor;
            
            dRes = Round(dRes);
            if (dRes < Round((m_dCellSize * m_dCellConversionFactor) / m_dResolutionConversionFactor))
               bRet = false;
            else if  (dRes > Round((m_iSize * m_dCellSize * m_dCellConversionFactor) / m_dResolutionConversionFactor))
               bRet = false;
         } 
         catch 
         {
            bRet = false;
         }
         return bRet;
      }            

      /// <summary>
      /// Calculate the scroll position given a particular resolution
      /// </summary>
      protected void CalculateScrollPosition()
      {
         int      iPercent;
         int      iCells;

         if (!m_bMap && !m_bVoxel)
         {
            iCells = Convert.ToInt32((m_dResolution * m_dResolutionConversionFactor) / (m_dCellSize * m_dCellConversionFactor));

            if (iCells <= 0)
               iPercent = 0;
            else if (iCells >= m_iSize)
               iPercent = 100;
            else
               iPercent = Convert.ToInt32(Math.Log(iCells, Math.E) / m_dIncrement);
         
            if (iPercent < 0) iPercent = 0;
            if (iPercent > 100) iPercent = 100;

            tbResolution.Value = iPercent;
         } 
         else 
         {
            double   dDiff = Math.Abs((m_dResolution * m_dResolutionConversionFactor) - ((double)m_oResolutions.Keys[0] * m_dCellConversionFactor));
            int      iIndex = 0;

            for (int i = 1; i < m_oResolutions.Count; i++)
            {
               double dTemp = Math.Abs((m_dResolution * m_dResolutionConversionFactor) - ((double)m_oResolutions.Keys[i] * m_dCellConversionFactor));

               if (dTemp < dDiff)
               {
                  dDiff = dTemp;
                  iIndex = i;
               }
            }

            tbResolution.Value = iIndex;
            m_dResolution = Round((double)m_oResolutions.Keys[iIndex] * m_dCellConversionFactor / m_dResolutionConversionFactor);

            if (m_bLatLon)
               tbRes.Text = DisplayInMetres();
            else
					tbRes.Text = m_dResolution.ToString(CultureInfo.CurrentCulture);
         }
      }

      /// <summary>
      /// Calculate the size of the extraction
      /// </summary>
      protected void CalculateSize()
      {
         Int64 iCellsX;
         Int64 iCellsY;
         int   iDecimationX;
         int   iDecimationY;
         Int64 iSize;

         if (m_bMap)
         {
            iSize = Convert.ToInt64((int)m_oResolutions.Values[tbResolution.Value]) * 2 * 8;
            iSize = Convert.ToInt64(iSize * m_dPercentOverlap);
         }
         else if (m_bVoxel)
         {
            iSize = Convert.ToInt64((int)m_oXDimension.Values[tbResolution.Value]) * Convert.ToInt64((int)m_oYDimension.Values[tbResolution.Value]) * Convert.ToInt64((int)m_oZDimension.Values[tbResolution.Value]) * 8;
            iSize = Convert.ToInt64(iSize * m_dPercentOverlap);
         }
         else
         {
            iDecimationX = Convert.ToInt32((m_dResolution * m_dResolutionConversionFactor) / (m_dCellSizeX * m_dCellConversionFactor));
            if (iDecimationX == 0)
               iCellsX = m_iSizeX;
            else
               iCellsX = m_iSizeX / iDecimationX;

            iDecimationY = Convert.ToInt32((m_dResolution * m_dResolutionConversionFactor) / (m_dCellSizeY * m_dCellConversionFactor));
            if (iDecimationY == 0)
               iCellsY = m_iSizeY;
            else
               iCellsY = m_iSizeY / iDecimationY;


            iSize = iCellsX * iCellsY;
         }

         if (iSize > 1073741824)
				lSize.Text = "File Size: " + ((double)iSize / (double)1073741824).ToString("f2", CultureInfo.CurrentCulture) + " GB";
         else if (iSize > 1048576)
				lSize.Text = "File Size: " + ((double)iSize / (double)1048576).ToString("f2", CultureInfo.CurrentCulture) + " MB";
         else if (iSize > 1024)
            lSize.Text = "File Size: " + ((double)iSize / (double)1024).ToString("f2", CultureInfo.CurrentCulture) + " KB";
         else
				lSize.Text = "File Size: " + iSize.ToString(CultureInfo.CurrentCulture) + " B";
      }
      #endregion

      #region Event Handlers
      /// <summary>
      /// Handle the scroll event
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void tbResolution_Scroll(object sender, System.EventArgs e)
      {
         int iCells;

         if (m_bMap || m_bVoxel)
         {
            m_dResolution = Round((double)m_oResolutions.Keys[tbResolution.Value] * m_dCellConversionFactor / m_dResolutionConversionFactor);
         }
         else
         {
            iCells = Convert.ToInt32(Math.Exp(tbResolution.Value * m_dIncrement));
            m_dResolution =  (iCells * m_dCellSize * m_dCellConversionFactor) / m_dResolutionConversionFactor;
            m_dResolution = Round(m_dResolution);
         }
         
         m_bTextChange = true;

         if (m_bLatLon)
            tbRes.Text = DisplayInMetres();
         else
				tbRes.Text = m_dResolution.ToString(CultureInfo.CurrentCulture);

         CalculateSize();
         m_bTextChange = false;
      }

      /// <summary>
      /// Handle the text changed event
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void tbRes_TextChanged(object sender, System.EventArgs e)
      {
         if (m_bTextChange || tbRes.Text == string.Empty || !ValidResolution())
            return;

         try
         {
				m_dResolution = Convert.ToDouble(tbRes.Text, CultureInfo.CurrentCulture);

            if (m_bLatLon)
               m_dResolution /= m_dResolutionConversionFactor;

            m_dResolution = Round(m_dResolution);
            
            CalculateScrollPosition();
            CalculateSize();
         } 
         catch (Exception ex)
         {
            MessageBox.Show(this, string.Format(CultureInfo.InvariantCulture, "Incorrect value entered in resolution box. Error {0}", ex.Message), "Resolution error");
            tbRes.Focus();
         }    

      }
      #endregion

		private void tbRes_Validating(object sender, CancelEventArgs e)
		{
			double dRes;
			if (!Double.TryParse(tbRes.Text, out dRes))
			{
				if (m_oErrorProvider != null) m_oErrorProvider.SetError(lUnit, "Invalid numeric value.");
				e.Cancel = true;
			}
			else if (dRes < m_dMinResolution)
			{
				if (m_oErrorProvider != null) m_oErrorProvider.SetError(lUnit, "Resolution is too small.");
				e.Cancel = true;
			}
			else if (dRes > m_dMaxResolution)
			{
				if (m_oErrorProvider != null) m_oErrorProvider.SetError(lUnit, "Resolution is too large.");
				e.Cancel = true;
			}
			else
			{
				m_oErrorProvider.SetError(lUnit, String.Empty);
			}
		}
   }
}
