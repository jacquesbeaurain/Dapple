using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Geosoft.GX.DAPGetData
{
	/// <summary>
	/// Summary description for ResolutionDialog.
	/// </summary>
	public class ResolutionDialog : System.Windows.Forms.Form
	{
      #region Member Variables
      private Geosoft.GXNet.CIPJ   m_oResolutionIPJ = null;
      private Geosoft.GXNet.CIPJ   m_oIPJ = null;

      private bool   m_bLatLon = false;
      private double m_dResolutionConversionFactor = 1;
      private double m_dCellConversionFactor = 1;
      private string m_strResolutionUnits = string.Empty;
      private string m_strCellUnits = string.Empty;

      private double m_dPercentOverlap = 1.0;

      private double m_dResolution;
      private double m_dCellSizeX;
      private double m_dCellSizeY;
      private double m_dCellSize;
      private int    m_iSizeX;
      private int    m_iSizeY;
      private int    m_iSize;
      private double m_dIncrement;

      private SortedList   m_oResolutions;

      private bool   m_bMap = false;
      private bool   m_bTextChange = false;
      #endregion

      private System.Windows.Forms.TextBox tbRes;
      private System.Windows.Forms.TrackBar tbResolution;
      private System.Windows.Forms.Label lSize;
      private System.Windows.Forms.Button bOK;
      private System.Windows.Forms.Button bCancel;
      private System.Windows.Forms.Label lUnit;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

      #region Properties
      /// <summary>
      /// Get the resolution that has been set
      /// </summary>
      public double Resolution
      {
         get { return m_dResolution; }
      }
      #endregion

      #region Constructor/Destructor
      /// <summary>
      /// Default constructor
      /// </summary>
		public ResolutionDialog()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
		}

      /// <summary>
      /// Default constructor
      /// </summary>
      /// <param name="dResolution"></param>
      /// <param name="iSizeX"></param>
      /// <param name="iSizeY"></param>
      /// <param name="dXCellSize"></param>
      /// <param name="dYCellSize"></param>
      public ResolutionDialog(double dResolution, double dXOrigin, double dYOrigin, int iSizeX, int iSizeY, double dXCellSize, double dYCellSize, Geosoft.GXNet.CIPJ oIPJ) : this()
      {         
         string               strUnit = string.Empty;
         string               strTemp = string.Empty;
         string               strProjection = string.Empty;
         string               strDatum = string.Empty;
         string               strMethod = string.Empty;
         string               strU = string.Empty;
         string               strLDatum = string.Empty;
         Geosoft.GXNet.CLTB   oUNI = null;
         int                  iUnits;
         double               dConv = 1;

         try
         {            
            // --- get the projection that the resolution is in ---

            m_oResolutionIPJ = Geosoft.GXNet.CIPJ.Create();
            if (GetDapData.Instance.OMExtents != null)          
               m_oResolutionIPJ.SetGXF("", GetDapData.Instance.OMExtents.CoordinateSystem.Datum, GetDapData.Instance.OMExtents.CoordinateSystem.Method, GetDapData.Instance.OMExtents.CoordinateSystem.Units, GetDapData.Instance.OMExtents.CoordinateSystem.LocalDatum);
            else
               m_oResolutionIPJ.SetGXF("", GetDapData.Instance.SearchExtents.CoordinateSystem.Datum, GetDapData.Instance.SearchExtents.CoordinateSystem.Method, GetDapData.Instance.SearchExtents.CoordinateSystem.Units, GetDapData.Instance.SearchExtents.CoordinateSystem.LocalDatum);

            
            // --- get the unit conversion factor to meters ---

            m_oResolutionIPJ.IGetUnits(ref m_dResolutionConversionFactor, ref m_strResolutionUnits);
            m_dResolutionConversionFactor = Geosoft.GXNet.CIPJ.rUnitScale(m_strResolutionUnits, m_dResolutionConversionFactor);
            lUnit.Text = m_strResolutionUnits;

            
            // --- if we are geographic then use a constant conversion factor ---

            if (m_oResolutionIPJ.iIsGeographic() == 1)
            {
               m_bLatLon = true;
               m_dResolutionConversionFactor = 110500.0 / dConv;
               
               oUNI = Geosoft.GXNet.CLTB.Create("units", Geosoft.GXNet.Constant.LTB_TYPE_HEADER, Geosoft.GXNet.Constant.LTB_DELIM_COMMA, string.Empty);
               if (Geosoft.GXNet.CSYS.IiGlobal("MONTAJ.DEFAULT_UNIT", ref strTemp) == 0) 
                  iUnits = oUNI.iFindKey(strTemp);
               else 
                  iUnits = 0;

               oUNI.IGetString(iUnits,0, ref strUnit);
               dConv = oUNI.rGetReal(iUnits,oUNI.iFindField("Factor"));
               lUnit.Text = strTemp;
            }


            oIPJ.IGetGXF(ref strProjection, ref strDatum, ref strMethod, ref strU, ref strLDatum);

            m_oIPJ = Geosoft.GXNet.CIPJ.Create();
            m_oIPJ.SetGXF(strProjection,strDatum,strMethod,strU,strLDatum);

            m_dResolution = dResolution;
            m_dCellSizeX = dXCellSize;
            m_dCellSizeY = dYCellSize;
            m_iSizeX = iSizeX;
            m_iSizeY = iSizeY;

            
            // --- get the units that the dataset cell size is in ---

            m_oIPJ.IGetUnits(ref m_dCellConversionFactor, ref m_strCellUnits);
            m_dCellConversionFactor = Geosoft.GXNet.CIPJ.rUnitScale(m_strCellUnits, m_dCellConversionFactor);
            if (m_oIPJ.iIsGeographic() == 1)
               m_dCellConversionFactor = 110500.0 / dConv;

            
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

            if (m_bLatLon)
               tbRes.Text = DisplayInMetres();
            else
               tbRes.Text = m_dResolution.ToString();

            CalculateScrollPosition();
            CalculateSize();

            m_bTextChange = false;
         } 
         catch {}
         finally
         {
            if (oUNI != null) oUNI.Dispose();
         }        
      }

      /// <summary>
      /// Default constructor, used when we have a list of valid resolutions
      /// </summary>
      /// <param name="dResolution"></param>
      /// <param name="iSizeX"></param>
      /// <param name="iSizeY"></param>
      /// <param name="dXCellSize"></param>
      /// <param name="dYCellSize"></param>
      public ResolutionDialog(double dResolution, double dMinX, double dMinY, double dMaxX, double dMaxY, SortedList oResolutions, Geosoft.GXNet.CIPJ oIPJ) : this()
      {         
         string               strUnit = string.Empty;
         string               strTemp = string.Empty;
         string               strProjection = string.Empty;
         string               strDatum = string.Empty;
         string               strMethod = string.Empty;
         string               strU = string.Empty;
         string               strLDatum = string.Empty;
         Geosoft.GXNet.CLTB   oUNI = null;
         int                  iUnits;
         double               dConv = 1;

         m_bMap = true;
         tbRes.Enabled = false;

         try
         {  
            // --- get the projection the resolution is in ---

            m_oResolutionIPJ = Geosoft.GXNet.CIPJ.Create();
            if (GetDapData.Instance.OMExtents != null)          
               m_oResolutionIPJ.SetGXF("", GetDapData.Instance.OMExtents.CoordinateSystem.Datum, GetDapData.Instance.OMExtents.CoordinateSystem.Method, GetDapData.Instance.OMExtents.CoordinateSystem.Units, GetDapData.Instance.OMExtents.CoordinateSystem.LocalDatum);
            else
               m_oResolutionIPJ.SetGXF("", GetDapData.Instance.SearchExtents.CoordinateSystem.Datum, GetDapData.Instance.SearchExtents.CoordinateSystem.Method, GetDapData.Instance.SearchExtents.CoordinateSystem.Units, GetDapData.Instance.SearchExtents.CoordinateSystem.LocalDatum);

            
            // --- figure out the units of the resolution ---

            m_oResolutionIPJ.IGetUnits(ref m_dResolutionConversionFactor, ref m_strResolutionUnits);
            m_dResolutionConversionFactor = Geosoft.GXNet.CIPJ.rUnitScale(m_strResolutionUnits, m_dResolutionConversionFactor);
            lUnit.Text = m_strResolutionUnits;

            
            // --- calculate conversion factor if units are in degrees ---

            if (m_oResolutionIPJ.iIsGeographic() == 1)
            {
               m_bLatLon = true;
               m_dResolutionConversionFactor = 110500.0 / dConv;               

               oUNI = Geosoft.GXNet.CLTB.Create("units", Geosoft.GXNet.Constant.LTB_TYPE_HEADER, Geosoft.GXNet.Constant.LTB_DELIM_COMMA, string.Empty);
               if (Geosoft.GXNet.CSYS.IiGlobal("MONTAJ.DEFAULT_UNIT", ref strTemp) == 0) 
                  iUnits = oUNI.iFindKey(strTemp);
               else 
                  iUnits = 0;

               oUNI.IGetString(iUnits,0, ref strUnit);
               dConv = oUNI.rGetReal(iUnits,oUNI.iFindField("Factor"));
               lUnit.Text = strTemp;
            }


            oIPJ.IGetGXF(ref strProjection, ref strDatum, ref strMethod, ref strU, ref strLDatum);

            m_oIPJ = Geosoft.GXNet.CIPJ.Create();
            m_oIPJ.SetGXF(strProjection,strDatum,strMethod,strU,strLDatum);
            
            m_dResolution = dResolution;
            m_oResolutions = oResolutions;

            
            // --- calculate conversion factor for resolution list from dataset ---

            m_oIPJ.IGetUnits(ref m_dCellConversionFactor, ref m_strCellUnits);
            m_dCellConversionFactor = Geosoft.GXNet.CIPJ.rUnitScale(m_strCellUnits, m_dCellConversionFactor);
            if (m_oIPJ.iIsGeographic() == 1)
               m_dCellConversionFactor = 110500.0 / dConv;

            tbResolution.Maximum = m_oResolutions.Count - 1;
            tbResolution.Minimum = 0;
            tbResolution.TickFrequency = 1;

            
            // --- calculate the overlap extents ---

            CalculateExtents(dMinX, dMinY, dMaxX, dMaxY);


            m_bTextChange = true;

            if (m_bLatLon)
               tbRes.Text = DisplayInMetres();
            else
               tbRes.Text = m_dResolution.ToString();

            CalculateScrollPosition();
            CalculateSize();

            m_bTextChange = false;
         } 
         catch {}
         finally
         {
            if (oUNI != null) oUNI.Dispose();
         }        
      }

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}

            if (m_oResolutionIPJ != null) m_oResolutionIPJ.Dispose();
            if (m_oIPJ != null) m_oIPJ.Dispose();
			}
			base.Dispose( disposing );
		}
      #endregion

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
         this.tbRes = new System.Windows.Forms.TextBox();
         this.tbResolution = new System.Windows.Forms.TrackBar();
         this.lSize = new System.Windows.Forms.Label();
         this.bOK = new System.Windows.Forms.Button();
         this.bCancel = new System.Windows.Forms.Button();
         this.lUnit = new System.Windows.Forms.Label();
         ((System.ComponentModel.ISupportInitialize)(this.tbResolution)).BeginInit();
         this.SuspendLayout();
         // 
         // tbRes
         // 
         this.tbRes.Location = new System.Drawing.Point(0, 16);
         this.tbRes.Name = "tbRes";
         this.tbRes.Size = new System.Drawing.Size(72, 20);
         this.tbRes.TabIndex = 1;
         this.tbRes.Text = "";
         this.tbRes.TextChanged += new System.EventHandler(this.tbRes_TextChanged);
         // 
         // tbResolution
         // 
         this.tbResolution.Location = new System.Drawing.Point(112, 8);
         this.tbResolution.Maximum = 100;
         this.tbResolution.Name = "tbResolution";
         this.tbResolution.Size = new System.Drawing.Size(192, 42);
         this.tbResolution.TabIndex = 2;
         this.tbResolution.TickFrequency = 5;
         this.tbResolution.Scroll += new System.EventHandler(this.tbResolution_Scroll);
         // 
         // lSize
         // 
         this.lSize.Location = new System.Drawing.Point(304, 16);
         this.lSize.Name = "lSize";
         this.lSize.Size = new System.Drawing.Size(152, 23);
         this.lSize.TabIndex = 5;
         this.lSize.Text = "File Size:";
         this.lSize.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
         // 
         // bOK
         // 
         this.bOK.Location = new System.Drawing.Point(296, 48);
         this.bOK.Name = "bOK";
         this.bOK.TabIndex = 6;
         this.bOK.Text = "&OK";
         this.bOK.Click += new System.EventHandler(this.bOK_Click);
         // 
         // bCancel
         // 
         this.bCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
         this.bCancel.Location = new System.Drawing.Point(376, 48);
         this.bCancel.Name = "bCancel";
         this.bCancel.TabIndex = 7;
         this.bCancel.Text = "&Cancel";
         // 
         // lUnit
         // 
         this.lUnit.Location = new System.Drawing.Point(72, 16);
         this.lUnit.Name = "lUnit";
         this.lUnit.Size = new System.Drawing.Size(32, 23);
         this.lUnit.TabIndex = 8;
         this.lUnit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
         // 
         // ResolutionDialog
         // 
         this.AcceptButton = this.bOK;
         this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
         this.CancelButton = this.bCancel;
         this.ClientSize = new System.Drawing.Size(458, 77);
         this.Controls.Add(this.lUnit);
         this.Controls.Add(this.bCancel);
         this.Controls.Add(this.bOK);
         this.Controls.Add(this.lSize);
         this.Controls.Add(this.tbResolution);
         this.Controls.Add(this.tbRes);
         this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
         this.MaximizeBox = false;
         this.MinimizeBox = false;
         this.Name = "ResolutionDialog";
         this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
         this.Text = "Set Resolution";
         this.Load += new System.EventHandler(this.ResolutionDialog_Load);
         ((System.ComponentModel.ISupportInitialize)(this.tbResolution)).EndInit();
         this.ResumeLayout(false);

      }
		#endregion

      #region Protected Methods
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
         double dPercentWidth;
         double dPercentHeight;

         Geosoft.GXNet.CPJ hPJ = null;

         try
         {

            // --- get the original extents ---

            dMinX = dOriginX;
            dMaxX = dOriginX + m_iSizeX * m_dCellSizeX;
            dMinY = dOriginY;
            dMaxY = dOriginY + m_iSizeY * m_dCellSizeY;

            hPJ = Geosoft.GXNet.CPJ.CreateIPJ(m_oIPJ, m_oResolutionIPJ);
         
            dMinXDest = dMinX;
            dMinYDest = dMinY;
            dMaxXDest = dMaxX;
            dMaxYDest = dMaxY;

            // --- reproject them into the search coordinate system ---

            hPJ.ProjectBoundingRectangle(ref dMinXDest, ref dMinYDest, ref dMaxXDest, ref dMaxYDest);

         
            // --- failed to reproject, work with the entire size of the image ---

            if (dMinXDest == Geosoft.GXNet.Constant.rDUMMY || 
               dMinYDest == Geosoft.GXNet.Constant.rDUMMY || 
               dMaxXDest == Geosoft.GXNet.Constant.rDUMMY || 
               dMaxYDest == Geosoft.GXNet.Constant.rDUMMY)
            {
               return;
            }

            // --- save the dataset extents ---

            dMinX = dMinXDest;
            dMinY = dMinYDest;
            dMaxX = dMaxXDest;
            dMaxY = dMaxYDest;
            
            
            // --- calculate the overlap bounding box ---

            dMinXDest = Math.Max(Math.Max(GetDapData.Instance.SearchExtents.MinX, GetDapData.Instance.ViewExtents.MinX), dMinXDest);
            dMaxXDest = Math.Min(Math.Max(GetDapData.Instance.SearchExtents.MaxX, GetDapData.Instance.ViewExtents.MaxX), dMaxXDest);
            dMinYDest = Math.Max(Math.Max(GetDapData.Instance.SearchExtents.MinY, GetDapData.Instance.ViewExtents.MinY), dMinYDest);
            dMaxYDest = Math.Min(Math.Max(GetDapData.Instance.SearchExtents.MaxY, GetDapData.Instance.ViewExtents.MaxY), dMaxYDest);


            // --- calculate the amount of overlap ---

            dPercentWidth = (dMaxXDest - dMinXDest)/(dMaxX - dMinX);
            dPercentHeight = (dMaxYDest - dMinYDest)/(dMaxY - dMinY);

            dPercentWidth = Math.Max(0, Math.Min(dPercentWidth, 1));
            dPercentHeight = Math.Max(0, Math.Min(dPercentHeight, 1));            

            m_iSizeX = Convert.ToInt32(m_iSizeX * dPercentWidth);
            m_iSizeY = Convert.ToInt32(m_iSizeY * dPercentHeight);
         } 
         catch {}
         finally
         {
            if (hPJ != null) hPJ.Dispose();
         }
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
         
         Geosoft.GXNet.CPJ hPJ = null;

         try
         {

            hPJ = Geosoft.GXNet.CPJ.CreateIPJ(m_oIPJ, m_oResolutionIPJ);
         
            dMinXDest = dMinX;
            dMinYDest = dMinY;
            dMaxXDest = dMaxX;
            dMaxYDest = dMaxY;

            // --- reproject them into the search coordinate system ---

            hPJ.ProjectBoundingRectangle(ref dMinXDest, ref dMinYDest, ref dMaxXDest, ref dMaxYDest);

         
            // --- failed to reproject, work with the entire size of the image ---

            if (dMinXDest == Geosoft.GXNet.Constant.rDUMMY || 
               dMinYDest == Geosoft.GXNet.Constant.rDUMMY || 
               dMaxXDest == Geosoft.GXNet.Constant.rDUMMY || 
               dMaxYDest == Geosoft.GXNet.Constant.rDUMMY)
            {
               return;
            }

            
            // --- calculate the overlap bounding box ---

            dMinXDest = Math.Max(Math.Max(GetDapData.Instance.SearchExtents.MinX, GetDapData.Instance.ViewExtents.MinX), dMinXDest);
            dMaxXDest = Math.Min(Math.Max(GetDapData.Instance.SearchExtents.MaxX, GetDapData.Instance.ViewExtents.MaxX), dMaxXDest);
            dMinYDest = Math.Max(Math.Max(GetDapData.Instance.SearchExtents.MinY, GetDapData.Instance.ViewExtents.MinY), dMinYDest);
            dMaxYDest = Math.Min(Math.Max(GetDapData.Instance.SearchExtents.MaxY, GetDapData.Instance.ViewExtents.MaxY), dMaxYDest);


            m_dPercentOverlap = ((dMaxXDest - dMinXDest) * (dMaxYDest - dMinYDest)) / ((dMaxX - dMinX) * (dMaxY - dMinY));
            m_dPercentOverlap = Math.Max(0, Math.Min(m_dPercentOverlap, 1));
         } 
         catch {}
         finally
         {
            if (hPJ != null) hPJ.Dispose();
         }
      }

      /// <summary>
      /// Display resolution in meters
      /// </summary>
      /// <returns></returns>
      protected string DisplayInMetres()
      {
         double dRes = m_dResolution * m_dResolutionConversionFactor;

         dRes = Round(dRes);
         return dRes.ToString();
      }

      /// <summary>
      /// Round to 4 significant digits
      /// </summary>
      /// <param name="dRes"></param>
      /// <returns></returns>
      protected double Round(double dRes)
      {
         string strFormat = dRes.ToString("g4");         

         try
         {
            dRes = Convert.ToDouble(strFormat);
         } 
         catch {}

         return dRes;            
      }

      /// <summary>
      /// See if the resolution entered is valid
      /// </summary>
      /// <returns></returns>
      protected bool ValidResolution()
      {
         bool     bRet = true;
         double   dRes;
         
         if (m_bMap)
            return true;

         if (tbRes.Text == string.Empty)
            return false;

         try
         {            
            dRes = Convert.ToDouble(tbRes.Text);

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

         if (!m_bMap)
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
            double   dDiff = Math.Abs((m_dResolution * m_dResolutionConversionFactor) - ((double)m_oResolutions.GetKey(0) * m_dCellConversionFactor));
            int      iIndex = 0;

            for (int i = 1; i < m_oResolutions.Count; i++)
            {
               double dTemp = Math.Abs((m_dResolution * m_dResolutionConversionFactor) - ((double)m_oResolutions.GetKey(i) * m_dCellConversionFactor));

               if (dTemp < dDiff)
               {
                  dDiff = dTemp;
                  iIndex = i;
               }
            }

            tbResolution.Value = iIndex;
            m_dResolution = Round((double)m_oResolutions.GetKey(iIndex) * m_dCellConversionFactor / m_dResolutionConversionFactor);

            if (m_bLatLon)
               tbRes.Text = DisplayInMetres();
            else
               tbRes.Text = m_dResolution.ToString();
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
            iSize = Convert.ToInt64((int)m_oResolutions.GetByIndex(tbResolution.Value) * 2 * 8 * m_dPercentOverlap);
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
            lSize.Text = "File Size: " + ((double)iSize / (double)1073741824).ToString("f2") + " GB";
         else if (iSize > 1048576)
            lSize.Text = "File Size: " + ((double)iSize / (double)1048576).ToString("f2") + " MB";
         else if (iSize > 1024)
            lSize.Text = "File Size: " + ((double)iSize / (double)1024).ToString("f2") + " KB";
         else
            lSize.Text = "File Size: " + iSize.ToString() + " B";
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

         if (m_bMap)
         {
            m_dResolution = Round((double)m_oResolutions.GetKey(tbResolution.Value) * m_dCellConversionFactor / m_dResolutionConversionFactor);
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
            tbRes.Text = m_dResolution.ToString();

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
            m_dResolution = Convert.ToDouble(tbRes.Text);

            if (m_bLatLon)
               m_dResolution /= m_dResolutionConversionFactor;

            m_dResolution = Round(m_dResolution);
            
            CalculateScrollPosition();
            CalculateSize();
         } 
         catch (Exception ex)
         {
            MessageBox.Show(this, string.Format("Incorrect value entered in resolution box. Error {0}", ex.Message), "Resolution error");
            tbRes.Focus();
         }    

      }

      /// <summary>
      /// Check to ensure resolution is entered correctly
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void bOK_Click(object sender, System.EventArgs e)
      {
         if (!ValidResolution())
         {
            MessageBox.Show(this, string.Format("Incorrect value entered in resolution box.\nThe value must be between {0} and {1}", m_dCellSize * m_dCellConversionFactor, m_iSize * m_dCellSize * m_dCellConversionFactor), "Resolution error");
            tbRes.Focus();
            return;
         }
         DialogResult = DialogResult.OK;
         Close();
      }

      /// <summary>
      /// Position the dialog centered around the cursor
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void ResolutionDialog_Load(object sender, System.EventArgs e)
      {
         this.Top = Cursor.Position.Y - this.Height / 2;
         this.Left = Cursor.Position.X - this.Width / 2;
      }      
      #endregion
	}
}