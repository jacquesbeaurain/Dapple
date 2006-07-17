using System;
using System.Xml;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

using Geosoft.Dap;
using Geosoft.WinFormsUI;

namespace Geosoft.GX.DAPGetData
{
	/// <summary>
	/// Summary description for BrowseWindow.
	/// </summary>
	public class BrowseWindow : DockContent
	{
      #region Constants
      protected const Int32            LARGE_MAP = 5;
      protected const Int32            SMALL_MAP = 6;

      protected const Int32            LARGE_MAP_WIDTH = 450;
      protected const Int32            LARGE_MAP_HEIGHT = 450;
      protected const Int32            SMALL_MAP_WIDTH = 250;
      protected const Int32            SMALL_MAP_HEIGHT = 250;
      protected const Double           AOI_FACTOR = 0.05;
      #endregion

      #region Member Variables
      protected Int32                  m_iMapWidth = SMALL_MAP_WIDTH;
      protected Int32                  m_iMapHeight = SMALL_MAP_HEIGHT;

      protected Geosoft.Dap.Common.BoundingBox  m_oBitmapViewExtents = new Geosoft.Dap.Common.BoundingBox();

      protected bool                   m_bHaveMouse = false;
      protected System.Drawing.Point   m_ptOriginal = new System.Drawing.Point();
      protected System.Drawing.Point	m_ptLast = new System.Drawing.Point();

      protected double                 m_dBitmapWidth;
      protected double                 m_dBitmapHeight;
      protected Bitmap                 m_oCurBitmap;

      protected bool                   m_bUpdateRegion = false;
      protected bool                   m_bSelectIndex = false;
      #endregion

      private System.Windows.Forms.PictureBox pbImage;
      private System.Windows.Forms.ToolBar tbToolbar;
      private System.Windows.Forms.ImageList imToolbarButtons;
      private System.Windows.Forms.ToolBarButton tbbAOI;
      private System.Windows.Forms.ToolBarButton tbbZoomIn;
      private System.Windows.Forms.ToolBarButton tbbZoomOut;
      private System.Windows.Forms.ToolBarButton tbbPan;
      private System.Windows.Forms.ToolBarButton tbbFullView;
      private System.Windows.Forms.ToolBarButton tbbSeperator1;
      private System.Windows.Forms.ToolBarButton tbbSeperator2;
      private System.Windows.Forms.Label lMaxY;
      private System.Windows.Forms.Label lMinY;
      private System.Windows.Forms.Label lMaxX;
      private System.Windows.Forms.Label lMinX;
      private System.Windows.Forms.Label lCoordinateSystem;
      private System.Windows.Forms.CheckBox cbBrowseMap;
      private System.Windows.Forms.CheckBox cbSelectedDatasets;
      private System.Windows.Forms.Panel pBrowseMap;
      private System.Windows.Forms.ToolBarButton tbbSeperator3;
      private System.Windows.Forms.ToolBarButton tbbMapSize;
      private System.Windows.Forms.Panel pSearch;
      private System.Windows.Forms.Label lWithinAOI;
      private System.Windows.Forms.Button bReset;
      private System.Windows.Forms.Button bSearch;
      private System.Windows.Forms.TextBox tbMaxX;
      private System.Windows.Forms.TextBox tbMaxY;
      private System.Windows.Forms.TextBox tbMinX;
      private System.Windows.Forms.TextBox tbMinY;
      private System.Windows.Forms.ComboBox cbRegion;
      private System.Windows.Forms.Label lRegion;
      private System.Windows.Forms.Label label1;
      private System.Windows.Forms.Label label2;
      private System.Windows.Forms.Label label3;
      private System.Windows.Forms.Label label4;
      private System.Windows.Forms.Label lServer;
      private System.Windows.Forms.ComboBox cbServerList;
      private System.Windows.Forms.Label lSearch;
      private System.Windows.Forms.TextBox tbSearch;
      private System.Windows.Forms.RadioButton rbAll;
      private System.Windows.Forms.RadioButton rbDescription;
      private System.Windows.Forms.RadioButton rbKeywords;
      private System.Windows.Forms.RadioButton rbName;
      private System.Windows.Forms.Label lLoggedIn;
      private System.Windows.Forms.PictureBox pbHelp;
      private System.ComponentModel.IContainer components;

      #region Event
      /// <summary>
      /// Define the aoi select event
      /// </summary>
      public event AOISelectHandler  AOISelect;

      /// <summary>
      /// Invoke the delegatae registered with the Click event
      /// </summary>
      protected virtual void OnAOISelect(AOISelectArgs e) 
      {
         if (AOISelect != null) 
         {
            AOISelect(this, e);
         }
      }

      /// <summary>
      /// Define the select server event
      /// </summary>
      public event ServerSelectHandler  ServerSelect;

      /// <summary>
      /// Invoke the delegatae registered with the select server event
      /// </summary>
      protected virtual void OnSelectServer(ServerSelectArgs e) 
      {
         if (ServerSelect != null) 
         {
            ServerSelect(this, e);
         }
      }
      #endregion

      #region Constructor/Destructor
		public BrowseWindow()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
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
         this.components = new System.ComponentModel.Container();
         System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(BrowseWindow));
         this.pbImage = new System.Windows.Forms.PictureBox();
         this.tbToolbar = new System.Windows.Forms.ToolBar();
         this.tbbZoomIn = new System.Windows.Forms.ToolBarButton();
         this.tbbZoomOut = new System.Windows.Forms.ToolBarButton();
         this.tbbPan = new System.Windows.Forms.ToolBarButton();
         this.tbbSeperator1 = new System.Windows.Forms.ToolBarButton();
         this.tbbFullView = new System.Windows.Forms.ToolBarButton();
         this.tbbSeperator2 = new System.Windows.Forms.ToolBarButton();
         this.tbbAOI = new System.Windows.Forms.ToolBarButton();
         this.tbbSeperator3 = new System.Windows.Forms.ToolBarButton();
         this.tbbMapSize = new System.Windows.Forms.ToolBarButton();
         this.imToolbarButtons = new System.Windows.Forms.ImageList(this.components);
         this.lMaxY = new System.Windows.Forms.Label();
         this.lMinY = new System.Windows.Forms.Label();
         this.lMaxX = new System.Windows.Forms.Label();
         this.lMinX = new System.Windows.Forms.Label();
         this.lCoordinateSystem = new System.Windows.Forms.Label();
         this.cbBrowseMap = new System.Windows.Forms.CheckBox();
         this.cbSelectedDatasets = new System.Windows.Forms.CheckBox();
         this.pBrowseMap = new System.Windows.Forms.Panel();
         this.pSearch = new System.Windows.Forms.Panel();
         this.tbSearch = new System.Windows.Forms.TextBox();
         this.bReset = new System.Windows.Forms.Button();
         this.bSearch = new System.Windows.Forms.Button();
         this.tbMaxX = new System.Windows.Forms.TextBox();
         this.tbMaxY = new System.Windows.Forms.TextBox();
         this.tbMinX = new System.Windows.Forms.TextBox();
         this.tbMinY = new System.Windows.Forms.TextBox();
         this.lSearch = new System.Windows.Forms.Label();
         this.cbRegion = new System.Windows.Forms.ComboBox();
         this.lRegion = new System.Windows.Forms.Label();
         this.label1 = new System.Windows.Forms.Label();
         this.label2 = new System.Windows.Forms.Label();
         this.label3 = new System.Windows.Forms.Label();
         this.label4 = new System.Windows.Forms.Label();
         this.lWithinAOI = new System.Windows.Forms.Label();
         this.rbKeywords = new System.Windows.Forms.RadioButton();
         this.rbDescription = new System.Windows.Forms.RadioButton();
         this.rbName = new System.Windows.Forms.RadioButton();
         this.rbAll = new System.Windows.Forms.RadioButton();
         this.lServer = new System.Windows.Forms.Label();
         this.cbServerList = new System.Windows.Forms.ComboBox();
         this.lLoggedIn = new System.Windows.Forms.Label();
         this.pbHelp = new System.Windows.Forms.PictureBox();
         this.pBrowseMap.SuspendLayout();
         this.pSearch.SuspendLayout();
         this.SuspendLayout();
         // 
         // pbImage
         // 
         this.pbImage.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
         this.pbImage.Location = new System.Drawing.Point(5, 26);
         this.pbImage.Name = "pbImage";
         this.pbImage.Size = new System.Drawing.Size(272, 250);
         this.pbImage.TabIndex = 0;
         this.pbImage.TabStop = false;
         this.pbImage.Paint += new System.Windows.Forms.PaintEventHandler(this.pbImage_Paint);
         this.pbImage.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pbImage_MouseUp);
         this.pbImage.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pbImage_MouseMove);
         this.pbImage.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pbImage_MouseDown);
         // 
         // tbToolbar
         // 
         this.tbToolbar.Appearance = System.Windows.Forms.ToolBarAppearance.Flat;
         this.tbToolbar.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
                                                                                     this.tbbZoomIn,
                                                                                     this.tbbZoomOut,
                                                                                     this.tbbPan,
                                                                                     this.tbbSeperator1,
                                                                                     this.tbbFullView,
                                                                                     this.tbbSeperator2,
                                                                                     this.tbbAOI,
                                                                                     this.tbbSeperator3,
                                                                                     this.tbbMapSize});
         this.tbToolbar.ButtonSize = new System.Drawing.Size(16, 16);
         this.tbToolbar.Dock = System.Windows.Forms.DockStyle.None;
         this.tbToolbar.DropDownArrows = true;
         this.tbToolbar.ImageList = this.imToolbarButtons;
         this.tbToolbar.Location = new System.Drawing.Point(8, 32);
         this.tbToolbar.Name = "tbToolbar";
         this.tbToolbar.ShowToolTips = true;
         this.tbToolbar.Size = new System.Drawing.Size(336, 28);
         this.tbToolbar.TabIndex = 1;
         this.tbToolbar.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.tbToolbar_ButtonClick);
         // 
         // tbbZoomIn
         // 
         this.tbbZoomIn.ImageIndex = 1;
         this.tbbZoomIn.Pushed = true;
         this.tbbZoomIn.ToolTipText = "Zoom In";
         // 
         // tbbZoomOut
         // 
         this.tbbZoomOut.ImageIndex = 2;
         this.tbbZoomOut.ToolTipText = "Zoom Out";
         // 
         // tbbPan
         // 
         this.tbbPan.ImageIndex = 3;
         this.tbbPan.ToolTipText = "Recentre";
         // 
         // tbbSeperator1
         // 
         this.tbbSeperator1.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
         // 
         // tbbFullView
         // 
         this.tbbFullView.ImageIndex = 4;
         this.tbbFullView.ToolTipText = "Full View";
         // 
         // tbbSeperator2
         // 
         this.tbbSeperator2.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
         // 
         // tbbAOI
         // 
         this.tbbAOI.ImageIndex = 0;
         this.tbbAOI.ToolTipText = "Select Area Of Interest";
         // 
         // tbbSeperator3
         // 
         this.tbbSeperator3.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
         // 
         // tbbMapSize
         // 
         this.tbbMapSize.ImageIndex = 6;
         this.tbbMapSize.ToolTipText = "Toggle Map Size";
         // 
         // imToolbarButtons
         // 
         this.imToolbarButtons.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
         this.imToolbarButtons.ImageSize = new System.Drawing.Size(16, 16);
         this.imToolbarButtons.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imToolbarButtons.ImageStream")));
         this.imToolbarButtons.TransparentColor = System.Drawing.Color.Transparent;
         // 
         // lMaxY
         // 
         this.lMaxY.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
         this.lMaxY.Location = new System.Drawing.Point(280, 26);
         this.lMaxY.Name = "lMaxY";
         this.lMaxY.Size = new System.Drawing.Size(48, 38);
         this.lMaxY.TabIndex = 2;
         // 
         // lMinY
         // 
         this.lMinY.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
         this.lMinY.Location = new System.Drawing.Point(280, 250);
         this.lMinY.Name = "lMinY";
         this.lMinY.Size = new System.Drawing.Size(48, 46);
         this.lMinY.TabIndex = 3;
         // 
         // lMaxX
         // 
         this.lMaxX.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
         this.lMaxX.Location = new System.Drawing.Point(200, 278);
         this.lMaxX.Name = "lMaxX";
         this.lMaxX.Size = new System.Drawing.Size(72, 34);
         this.lMaxX.TabIndex = 4;
         this.lMaxX.TextAlign = System.Drawing.ContentAlignment.TopRight;
         // 
         // lMinX
         // 
         this.lMinX.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
         this.lMinX.Location = new System.Drawing.Point(8, 278);
         this.lMinX.Name = "lMinX";
         this.lMinX.Size = new System.Drawing.Size(72, 34);
         this.lMinX.TabIndex = 5;
         // 
         // lCoordinateSystem
         // 
         this.lCoordinateSystem.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
         this.lCoordinateSystem.Location = new System.Drawing.Point(80, 278);
         this.lCoordinateSystem.Name = "lCoordinateSystem";
         this.lCoordinateSystem.Size = new System.Drawing.Size(120, 23);
         this.lCoordinateSystem.TabIndex = 6;
         this.lCoordinateSystem.TextAlign = System.Drawing.ContentAlignment.TopCenter;
         // 
         // cbBrowseMap
         // 
         this.cbBrowseMap.Checked = true;
         this.cbBrowseMap.CheckState = System.Windows.Forms.CheckState.Checked;
         this.cbBrowseMap.Location = new System.Drawing.Point(8, 6);
         this.cbBrowseMap.Name = "cbBrowseMap";
         this.cbBrowseMap.Size = new System.Drawing.Size(96, 16);
         this.cbBrowseMap.TabIndex = 7;
         this.cbBrowseMap.Text = "Browser Map";
         this.cbBrowseMap.CheckedChanged += new System.EventHandler(this.cbLayer_CheckedChanged);
         // 
         // cbSelectedDatasets
         // 
         this.cbSelectedDatasets.Location = new System.Drawing.Point(112, 6);
         this.cbSelectedDatasets.Name = "cbSelectedDatasets";
         this.cbSelectedDatasets.Size = new System.Drawing.Size(120, 16);
         this.cbSelectedDatasets.TabIndex = 9;
         this.cbSelectedDatasets.Text = "Selected Datasets";
         this.cbSelectedDatasets.CheckedChanged += new System.EventHandler(this.cbLayer_CheckedChanged);
         // 
         // pBrowseMap
         // 
         this.pBrowseMap.BackColor = System.Drawing.SystemColors.Window;
         this.pBrowseMap.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
         this.pBrowseMap.Controls.Add(this.cbSelectedDatasets);
         this.pBrowseMap.Controls.Add(this.lCoordinateSystem);
         this.pBrowseMap.Controls.Add(this.lMinX);
         this.pBrowseMap.Controls.Add(this.lMaxX);
         this.pBrowseMap.Controls.Add(this.lMinY);
         this.pBrowseMap.Controls.Add(this.lMaxY);
         this.pBrowseMap.Controls.Add(this.pbImage);
         this.pBrowseMap.Controls.Add(this.cbBrowseMap);
         this.pBrowseMap.Location = new System.Drawing.Point(4, 64);
         this.pBrowseMap.Name = "pBrowseMap";
         this.pBrowseMap.Size = new System.Drawing.Size(336, 312);
         this.pBrowseMap.TabIndex = 11;
         // 
         // pSearch
         // 
         this.pSearch.BackColor = System.Drawing.SystemColors.Window;
         this.pSearch.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
         this.pSearch.Controls.Add(this.pbHelp);
         this.pSearch.Controls.Add(this.tbSearch);
         this.pSearch.Controls.Add(this.bReset);
         this.pSearch.Controls.Add(this.bSearch);
         this.pSearch.Controls.Add(this.tbMaxX);
         this.pSearch.Controls.Add(this.tbMaxY);
         this.pSearch.Controls.Add(this.tbMinX);
         this.pSearch.Controls.Add(this.tbMinY);
         this.pSearch.Controls.Add(this.lSearch);
         this.pSearch.Controls.Add(this.cbRegion);
         this.pSearch.Controls.Add(this.lRegion);
         this.pSearch.Controls.Add(this.label1);
         this.pSearch.Controls.Add(this.label2);
         this.pSearch.Controls.Add(this.label3);
         this.pSearch.Controls.Add(this.label4);
         this.pSearch.Controls.Add(this.lWithinAOI);
         this.pSearch.Controls.Add(this.rbKeywords);
         this.pSearch.Controls.Add(this.rbDescription);
         this.pSearch.Controls.Add(this.rbName);
         this.pSearch.Controls.Add(this.rbAll);
         this.pSearch.Location = new System.Drawing.Point(8, 384);
         this.pSearch.Name = "pSearch";
         this.pSearch.Size = new System.Drawing.Size(328, 200);
         this.pSearch.TabIndex = 20;
         // 
         // tbSearch
         // 
         this.tbSearch.Location = new System.Drawing.Point(8, 126);
         this.tbSearch.Name = "tbSearch";
         this.tbSearch.Size = new System.Drawing.Size(272, 20);
         this.tbSearch.TabIndex = 17;
         this.tbSearch.Text = "";
         this.tbSearch.TextChanged += new System.EventHandler(this.tbSearch_TextChanged);
         // 
         // bReset
         // 
         this.bReset.BackColor = System.Drawing.SystemColors.Control;
         this.bReset.Location = new System.Drawing.Point(240, 168);
         this.bReset.Name = "bReset";
         this.bReset.Size = new System.Drawing.Size(78, 23);
         this.bReset.TabIndex = 15;
         this.bReset.Text = "Reset";
         this.bReset.Click += new System.EventHandler(this.bReset_Click);
         // 
         // bSearch
         // 
         this.bSearch.BackColor = System.Drawing.SystemColors.Control;
         this.bSearch.Enabled = false;
         this.bSearch.Location = new System.Drawing.Point(160, 168);
         this.bSearch.Name = "bSearch";
         this.bSearch.Size = new System.Drawing.Size(78, 23);
         this.bSearch.TabIndex = 14;
         this.bSearch.Text = "Find";
         this.bSearch.Click += new System.EventHandler(this.bSearch_Click);
         // 
         // tbMaxX
         // 
         this.tbMaxX.Location = new System.Drawing.Point(7, 80);
         this.tbMaxX.Name = "tbMaxX";
         this.tbMaxX.Size = new System.Drawing.Size(72, 20);
         this.tbMaxX.TabIndex = 8;
         this.tbMaxX.Text = "";
         this.tbMaxX.TextChanged += new System.EventHandler(this.tbCoordinate_TextChanged);
         // 
         // tbMaxY
         // 
         this.tbMaxY.Location = new System.Drawing.Point(82, 80);
         this.tbMaxY.Name = "tbMaxY";
         this.tbMaxY.Size = new System.Drawing.Size(72, 20);
         this.tbMaxY.TabIndex = 9;
         this.tbMaxY.Text = "";
         this.tbMaxY.TextChanged += new System.EventHandler(this.tbCoordinate_TextChanged);
         // 
         // tbMinX
         // 
         this.tbMinX.Location = new System.Drawing.Point(7, 40);
         this.tbMinX.Name = "tbMinX";
         this.tbMinX.Size = new System.Drawing.Size(72, 20);
         this.tbMinX.TabIndex = 6;
         this.tbMinX.Text = "";
         this.tbMinX.TextChanged += new System.EventHandler(this.tbCoordinate_TextChanged);
         // 
         // tbMinY
         // 
         this.tbMinY.Location = new System.Drawing.Point(82, 40);
         this.tbMinY.Name = "tbMinY";
         this.tbMinY.Size = new System.Drawing.Size(72, 20);
         this.tbMinY.TabIndex = 7;
         this.tbMinY.Text = "";
         this.tbMinY.TextChanged += new System.EventHandler(this.tbCoordinate_TextChanged);
         // 
         // lSearch
         // 
         this.lSearch.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
         this.lSearch.Location = new System.Drawing.Point(8, 106);
         this.lSearch.Name = "lSearch";
         this.lSearch.TabIndex = 11;
         this.lSearch.Text = "Search";
         // 
         // cbRegion
         // 
         this.cbRegion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
         this.cbRegion.Location = new System.Drawing.Point(160, 40);
         this.cbRegion.Name = "cbRegion";
         this.cbRegion.Size = new System.Drawing.Size(160, 21);
         this.cbRegion.TabIndex = 10;
         this.cbRegion.SelectedIndexChanged += new System.EventHandler(this.cbRegion_SelectedIndexChanged);
         // 
         // lRegion
         // 
         this.lRegion.Location = new System.Drawing.Point(160, 24);
         this.lRegion.Name = "lRegion";
         this.lRegion.Size = new System.Drawing.Size(64, 16);
         this.lRegion.TabIndex = 9;
         this.lRegion.Text = "Region";
         this.lRegion.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
         // 
         // label1
         // 
         this.label1.Location = new System.Drawing.Point(7, 64);
         this.label1.Name = "label1";
         this.label1.Size = new System.Drawing.Size(64, 16);
         this.label1.TabIndex = 4;
         this.label1.Text = "Maximum X";
         this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
         // 
         // label2
         // 
         this.label2.Location = new System.Drawing.Point(82, 64);
         this.label2.Name = "label2";
         this.label2.Size = new System.Drawing.Size(64, 16);
         this.label2.TabIndex = 3;
         this.label2.Text = "Y";
         this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
         // 
         // label3
         // 
         this.label3.Location = new System.Drawing.Point(7, 24);
         this.label3.Name = "label3";
         this.label3.Size = new System.Drawing.Size(64, 16);
         this.label3.TabIndex = 2;
         this.label3.Text = "Minimum X";
         this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
         // 
         // label4
         // 
         this.label4.Location = new System.Drawing.Point(82, 24);
         this.label4.Name = "label4";
         this.label4.Size = new System.Drawing.Size(64, 16);
         this.label4.TabIndex = 1;
         this.label4.Text = "Y";
         this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
         // 
         // lWithinAOI
         // 
         this.lWithinAOI.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
         this.lWithinAOI.Location = new System.Drawing.Point(8, 4);
         this.lWithinAOI.Name = "lWithinAOI";
         this.lWithinAOI.Size = new System.Drawing.Size(144, 24);
         this.lWithinAOI.TabIndex = 16;
         this.lWithinAOI.Text = "Area of Interest";
         // 
         // rbKeywords
         // 
         this.rbKeywords.Location = new System.Drawing.Point(208, 144);
         this.rbKeywords.Name = "rbKeywords";
         this.rbKeywords.Size = new System.Drawing.Size(72, 24);
         this.rbKeywords.TabIndex = 21;
         this.rbKeywords.Text = "Keywords";
         this.rbKeywords.CheckedChanged += new System.EventHandler(this.tbSearch_TextChanged);
         // 
         // rbDescription
         // 
         this.rbDescription.Location = new System.Drawing.Point(120, 144);
         this.rbDescription.Name = "rbDescription";
         this.rbDescription.Size = new System.Drawing.Size(80, 24);
         this.rbDescription.TabIndex = 20;
         this.rbDescription.Text = "Description";
         this.rbDescription.CheckedChanged += new System.EventHandler(this.tbSearch_TextChanged);
         // 
         // rbName
         // 
         this.rbName.Location = new System.Drawing.Point(56, 144);
         this.rbName.Name = "rbName";
         this.rbName.Size = new System.Drawing.Size(56, 24);
         this.rbName.TabIndex = 19;
         this.rbName.Text = "Name";
         this.rbName.CheckedChanged += new System.EventHandler(this.tbSearch_TextChanged);
         // 
         // rbAll
         // 
         this.rbAll.Checked = true;
         this.rbAll.Location = new System.Drawing.Point(8, 144);
         this.rbAll.Name = "rbAll";
         this.rbAll.Size = new System.Drawing.Size(40, 24);
         this.rbAll.TabIndex = 18;
         this.rbAll.TabStop = true;
         this.rbAll.Text = "All";
         this.rbAll.CheckedChanged += new System.EventHandler(this.tbSearch_TextChanged);
         // 
         // lServer
         // 
         this.lServer.Location = new System.Drawing.Point(8, 8);
         this.lServer.Name = "lServer";
         this.lServer.Size = new System.Drawing.Size(40, 23);
         this.lServer.TabIndex = 22;
         this.lServer.Text = "Server";
         this.lServer.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
         // 
         // cbServerList
         // 
         this.cbServerList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
         this.cbServerList.Location = new System.Drawing.Point(48, 8);
         this.cbServerList.Name = "cbServerList";
         this.cbServerList.Size = new System.Drawing.Size(296, 21);
         this.cbServerList.TabIndex = 21;
         this.cbServerList.SelectedIndexChanged += new System.EventHandler(this.cbServerList_SelectedIndexChanged);
         // 
         // lLoggedIn
         // 
         this.lLoggedIn.Location = new System.Drawing.Point(8, 32);
         this.lLoggedIn.Name = "lLoggedIn";
         this.lLoggedIn.Size = new System.Drawing.Size(256, 56);
         this.lLoggedIn.TabIndex = 23;
         this.lLoggedIn.Text = "Unable to view Browser Map or Search for datasets as you are not currently logged" +
            " in.";
         this.lLoggedIn.Visible = false;
         // 
         // pbHelp
         // 
         this.pbHelp.Image = ((System.Drawing.Image)(resources.GetObject("pbHelp.Image")));
         this.pbHelp.Location = new System.Drawing.Point(283, 128);
         this.pbHelp.Name = "pbHelp";
         this.pbHelp.Size = new System.Drawing.Size(16, 16);
         this.pbHelp.TabIndex = 22;
         this.pbHelp.TabStop = false;
         this.pbHelp.Click += new System.EventHandler(this.pbHelp_Click);
         // 
         // BrowseWindow
         // 
         this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
         this.AutoScroll = true;
         this.BackColor = System.Drawing.SystemColors.Control;
         this.ClientSize = new System.Drawing.Size(512, 591);
         this.Controls.Add(this.lServer);
         this.Controls.Add(this.cbServerList);
         this.Controls.Add(this.pSearch);
         this.Controls.Add(this.pBrowseMap);
         this.Controls.Add(this.tbToolbar);
         this.Controls.Add(this.lLoggedIn);
         this.DockableAreas = ((Geosoft.WinFormsUI.DockAreas)((Geosoft.WinFormsUI.DockAreas.DockLeft | Geosoft.WinFormsUI.DockAreas.DockRight)));
         this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
         this.HideOnClose = true;
         this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
         this.MaximizeBox = false;
         this.MinimizeBox = false;
         this.MinimumSize = new System.Drawing.Size(518, 550);
         this.Name = "BrowseWindow";
         this.ShowInTaskbar = false;
         this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
         this.Text = "Area Of Interest";
         this.EnabledChanged += new System.EventHandler(this.BrowseWindow_EnabledChanged);
         this.pBrowseMap.ResumeLayout(false);
         this.pSearch.ResumeLayout(false);
         this.ResumeLayout(false);

      }
		#endregion

      #region Public Member Functions
      /// <summary>
      /// Initalize the control 
      /// </summary>
      public void Init()
      {
         Geosoft.Dap.Common.BoundingBox hSearchBoundingBox = GetDapData.Instance.SearchExtents;

         if (GetDapData.Instance.CurServer != null)
            this.Text = GetDapData.Instance.CurServer.Name;

         // --- Setup the browser map ---

         GetDapData.Instance.ViewExtents = new Geosoft.Dap.Common.BoundingBox(hSearchBoundingBox);
         
         // --- get the browser image ---
         
         SaveImage(GetDapData.Instance.CurServer.GetBrowserMap(), GetDapData.Instance.CurServer.ServerExtents);         

         
         // --- set up the search keywords ---

         tbMaxX.Text = Constant.FormatCoordinate(hSearchBoundingBox.MaxX);
         tbMaxY.Text = Constant.FormatCoordinate(hSearchBoundingBox.MaxY);
         tbMinX.Text = Constant.FormatCoordinate(hSearchBoundingBox.MinX);
         tbMinY.Text = Constant.FormatCoordinate(hSearchBoundingBox.MinY);
         cbRegion.SelectedIndex = -1;

         // --- populate the list of keywords and regions ---

         PopulateLists();


         // --- upate the aoi ---

         UpdateAOI();         
      }

      /// <summary>
      /// Update the contents of the browser map
      /// </summary>
      public void UpdateAOI()
      {  
         Geosoft.Dap.Common.BoundingBox hSearchBoundingBox = GetDapData.Instance.SearchExtents;

         // --- redraw the image with the new aoi ---

         double   dWidth = hSearchBoundingBox.MaxX - hSearchBoundingBox.MinX;
         double   dHeight = hSearchBoundingBox.MaxY - hSearchBoundingBox.MinY;

         dWidth *= AOI_FACTOR;
         dHeight *= AOI_FACTOR;

         GetDapData.Instance.ViewExtents.MaxX = hSearchBoundingBox.MaxX + dWidth;
         GetDapData.Instance.ViewExtents.MaxY = hSearchBoundingBox.MaxY + dHeight;
         GetDapData.Instance.ViewExtents.MinX = hSearchBoundingBox.MinX - dWidth;
         GetDapData.Instance.ViewExtents.MinY = hSearchBoundingBox.MinY - dHeight;

         DrawImage();

         
         // --- reset the search extents ---

         tbMaxX.Text = Constant.FormatCoordinate(hSearchBoundingBox.MaxX);
         tbMaxY.Text = Constant.FormatCoordinate(hSearchBoundingBox.MaxY);
         tbMinX.Text = Constant.FormatCoordinate(hSearchBoundingBox.MinX);
         tbMinY.Text = Constant.FormatCoordinate(hSearchBoundingBox.MinY);
         cbRegion.SelectedIndex = -1;

         bSearch.Enabled = false;
      }

      /// <summary>
      /// Disable/Enable aoi selection
      /// </summary>
      public void UpdateAOIState(bool bEnabled)
      {
         tbbAOI.Enabled = bEnabled;

         pbImage.Invalidate();
      }

      /// <summary>
      /// The selected datasets have changed, if we are drawing them then refresh the image
      /// </summary>
      public void SelectedDatasetChanged()
      {
         if (cbSelectedDatasets.Checked)
            DrawImage();
      }
      
      /// <summary>
      /// Get the image from the server
      /// </summary>
      public void GetImage()
      {
         Geosoft.Dap.Common.Format        hFormat = new Geosoft.Dap.Common.Format();
         Geosoft.Dap.Command              hDapCommand = GetDapData.Instance.CurServer.Command;
         Geosoft.Dap.Common.Resolution    hResolution = new Geosoft.Dap.Common.Resolution();
         Geosoft.Dap.Common.BoundingBox   hBBox = new Geosoft.Dap.Common.BoundingBox(GetDapData.Instance.ViewExtents);
         ArrayList                        hList = new ArrayList();
         ArrayList                        hSelectedList = new ArrayList();
         XmlDocument                      hDoc;         
         
         hFormat.Type = "image/png24";

         hResolution.Width = pbImage.Width;
         hResolution.Height = pbImage.Height;                 

         try
         {
            if (cbSelectedDatasets.Checked) 
            {
               // --- only add the datasets that are from the currently selected server ---

               hSelectedList = GetDapData.Instance.GetSelectedDatasets();

               foreach (Geosoft.Dap.Common.DataSet hDataSet in hSelectedList)
               {
                  if (hDataSet.Url == GetDapData.Instance.CurServer.Url)
                     hList.Add(hDataSet);
               }
            }

            if (cbBrowseMap.Checked || hList.Count > 0)
            {
               hDoc = hDapCommand.GetImageEx(hFormat, hBBox, hResolution, cbBrowseMap.Checked, false, hList);
               SaveImage(hDoc, hBBox);
            } 
            else 
            {
               SaveBlankImage(hBBox);
            }
         } 
         catch (Exception e)
         {
            GetDapError.Instance.Write("GetImage - " + e.Message);
         }
      }

      /// <summary>
      /// The keywords list or aoi list might have changed, repopulate them
      /// </summary>
      public void UpdateConfiguration()
      {
         if (this.InvokeRequired)         
            this.BeginInvoke(new MethodInvoker(this.PopulateLists));
         else 
            PopulateLists();
      }

      /// <summary>
      /// Update the dataset count list
      /// </summary>
      public void UpdateCounts()
      {
         if (this.InvokeRequired)
            this.BeginInvoke(new MethodInvoker(this.RefreshCounts));
         else
            RefreshCounts();
      }

      /// <summary>
      /// Populate the list of servers
      /// </summary>
      public void PopulateServerList()
      {
         Int32    iSelect = -1;   
         string   strServerUrl = string.Empty;
         Server   hServer;

         
         // --- do not execute if we are cleanup up --- 
         
         if (this.IsDisposed) return;


         m_bSelectIndex = true;
         
         if (cbServerList.SelectedIndex != -1 && cbServerList.SelectedIndex < GetDapData.Instance.ServerList.Count)
         {
            hServer = (Server)GetDapData.Instance.ServerList.GetByIndex(cbServerList.SelectedIndex);
            strServerUrl = hServer.Url;
         }

         if (strServerUrl == string.Empty)
            Constant.GetSelectedServerInSettingsMeta(out strServerUrl);

         cbServerList.Items.Clear();

         for (int i = 0; i < GetDapData.Instance.ServerList.Count; i++)
         {
            hServer = (Server)GetDapData.Instance.ServerList.GetByIndex(i);

            if (hServer.Status == Server.ServerStatus.OnLine)
               cbServerList.Items.Add(hServer.Name + " (" + hServer.DatasetCount.ToString() + ")");
            else if (hServer.Status == Server.ServerStatus.Maintenance)
               cbServerList.Items.Add(hServer.Name + " (under going maintenance)");

            if (hServer.Url == strServerUrl)
               iSelect = i;
         }

         m_bSelectIndex = false;

         if (iSelect != -1)
            cbServerList.SelectedIndex = iSelect;                 
      }

      /// <summary>
      /// Set the selected server
      /// </summary>
      /// <param name="iIndex"></param>
      public void SelectServer(Int32 iIndex)
      {
         cbServerList.SelectedIndex = iIndex;
      }         

      /// <summary>
      /// Enable/Disable whether we can see anything in this dialog
      /// </summary>
      /// <param name="bValid"></param>
      public void LoggedIn(bool bValid)
      {
         tbToolbar.Visible = bValid;
         pBrowseMap.Visible = bValid;
         pSearch.Visible = bValid;
         lLoggedIn.Visible = !bValid;
      }

      /// <summary>
      /// Zoom to a specifed bounding box
      /// </summary>
      /// <param name="oBox"></param>
      public void ZoomTo(Geosoft.Dap.Common.BoundingBox oBox)
      {
         GetDapData.Instance.ViewExtents.MaxX = oBox.MaxX;
         GetDapData.Instance.ViewExtents.MinX = oBox.MinX;
         GetDapData.Instance.ViewExtents.MaxY = oBox.MaxY;
         GetDapData.Instance.ViewExtents.MinY = oBox.MinY;

         DrawImage();
      }
      #endregion

      #region Proected Methods
      #region Browser Map
      /// <summary>
      /// Calculate the new extents for this image
      /// </summary>
      protected void CalculateExtents()
      {
         Geosoft.Dap.Common.Resolution    hResolution = new Geosoft.Dap.Common.Resolution();         
         
         hResolution.Width = pbImage.Width;
         hResolution.Height = pbImage.Height;                 


         // --- Calculate the best bounding box for this image, so that it is not distorted ---

         double dWidth = GetDapData.Instance.ViewExtents.MaxX - GetDapData.Instance.ViewExtents.MinX;
         double dHeight = GetDapData.Instance.ViewExtents.MaxY - GetDapData.Instance.ViewExtents.MinY;

         if (dWidth == 0 || dHeight == 0) 
         {
            return;
         }

         double dPictureRatio = (double)hResolution.Height / (double)hResolution.Width;
         double dBoxRatio = dHeight / dWidth;

         if (dBoxRatio > dPictureRatio)
         {
            // --- width is smaller than it should be ---

            dWidth = dHeight / dPictureRatio;
            
            double dCenter = (GetDapData.Instance.ViewExtents.MaxX + GetDapData.Instance.ViewExtents.MinX) / 2;
            GetDapData.Instance.ViewExtents.MaxX = dCenter + dWidth / 2;
            GetDapData.Instance.ViewExtents.MinX = dCenter - dWidth / 2;
         } 
         else if (dBoxRatio < dPictureRatio)
         {
            // --- height is smaller than it should be ---

            dHeight = dWidth * dPictureRatio;

            double dCenter = (GetDapData.Instance.ViewExtents.MaxY + GetDapData.Instance.ViewExtents.MinY) / 2;
            GetDapData.Instance.ViewExtents.MaxY = dCenter + dHeight / 2;
            GetDapData.Instance.ViewExtents.MinY = dCenter - dHeight / 2;
         }         
         
            
         try
         {
            // --- convert the area to WGS 84 ---

            Geosoft.Dap.Common.BoundingBox      hBox = new Geosoft.Dap.Common.BoundingBox(GetDapData.Instance.ViewExtents);
            Geosoft.Dap.Common.CoordinateSystem hWGS84 = new Geosoft.Dap.Common.CoordinateSystem();
            hWGS84.Datum = "WGS 84";

            Constant.Reproject(hBox, hWGS84);

            // --- save the new bounding box ---
            
            lMaxX.Text = Constant.FormatCoordinate(hBox.MaxX);
            lMaxY.Text = Constant.FormatCoordinate(hBox.MaxY);
            lMinX.Text = Constant.FormatCoordinate(hBox.MinX);
            lMinY.Text = Constant.FormatCoordinate(hBox.MinY);
            lCoordinateSystem.Text = GetDapData.Instance.ViewExtents.CoordinateSystem.ToString();
         } 
         catch (Exception e)
         {
            GetDapError.Instance.Write("CalculateExtents - " + e.Message);
         }
      }

      /// <summary>
      /// Draw the image
      /// </summary>
      protected void DrawImage()
      {
         CalculateExtents();
         pbImage.Invalidate();

         GetDapData.Instance.EnqueueRequest(GetDapData.AsyncRequestType.GetImage);
      }

      /// <summary>
      /// Save the image into our current bitmap
      /// </summary>
      /// <param name="hDoc"></param>
      protected void SaveImage(XmlDocument hDoc, Geosoft.Dap.Common.BoundingBox hBox)
      {
         XmlNodeList	hNodeList =  hDoc.SelectNodes("/" + Geosoft.Dap.Xml.Common.Constant.Tag.GEO_XML_TAG + "/" + Geosoft.Dap.Xml.Common.Constant.Tag.RESPONSE_TAG + "/" + Geosoft.Dap.Xml.Common.Constant.Tag.IMAGE_TAG + "/" + Geosoft.Dap.Xml.Common.Constant.Tag.PICTURE_TAG);
         System.IO.MemoryStream	ms = new System.IO.MemoryStream();
         
         if (hNodeList.Count > 0)
         {
            foreach( System.Xml.XmlNode hNode in hNodeList ) 
            {
               XmlNode  hN1 = hNode.FirstChild;
               string   jpegImage = hN1.Value;
					
               if( jpegImage != null ) 
               {                              
                  byte[] jpegRawImage = Convert.FromBase64String( jpegImage );
                  ms.Write(jpegRawImage, 0, jpegRawImage.Length);
               }
            }		
            ms.Flush();
            ms.Seek(0, System.IO.SeekOrigin.Begin);
         
            m_oCurBitmap = new Bitmap(ms);
            m_oBitmapViewExtents.MaxX = hBox.MaxX;
            m_oBitmapViewExtents.MaxY = hBox.MaxY;
            m_oBitmapViewExtents.MinX = hBox.MinX;
            m_oBitmapViewExtents.MinY = hBox.MinY;
            pbImage.Invalidate();
         } 
         else 
         {
            SaveBlankImage(hBox);
         }
      }

      /// <summary>
      /// Create a blank image 
      /// </summary>
      protected void SaveBlankImage(Geosoft.Dap.Common.BoundingBox hBox)
      {
         m_oCurBitmap = new Bitmap(pbImage.Width, pbImage.Height);
         m_oBitmapViewExtents.MaxX = hBox.MaxX;
         m_oBitmapViewExtents.MaxY = hBox.MaxY;
         m_oBitmapViewExtents.MinX = hBox.MinX;
         m_oBitmapViewExtents.MinY = hBox.MinY;
         pbImage.Invalidate();
      }

      /// <summary>
      /// Draw the current bitmap that we have saved onto the new extents
      /// </summary>
      /// <param name="hGraphics"></param>
      protected void DrawImage(System.Drawing.Graphics   hGraphics)
      {
         float   fTop = Convert.ToSingle(GetDapData.Instance.ViewExtents.MaxY);
         float   fLeft = Convert.ToSingle(GetDapData.Instance.ViewExtents.MinX);
         float   fBottom = Convert.ToSingle(GetDapData.Instance.ViewExtents.MinY);
         float   fRight = Convert.ToSingle(GetDapData.Instance.ViewExtents.MaxX);
         
         float   fWidth = Convert.ToSingle(GetDapData.Instance.ViewExtents.MaxX - GetDapData.Instance.ViewExtents.MinX);
         float   fHeight = Convert.ToSingle(GetDapData.Instance.ViewExtents.MaxY - GetDapData.Instance.ViewExtents.MinY);
   
         float   fBitmapTop = Convert.ToSingle(m_oBitmapViewExtents.MaxY);
         float   fBitmapLeft = Convert.ToSingle(m_oBitmapViewExtents.MinX);
         float   fBitmapBottom = Convert.ToSingle(m_oBitmapViewExtents.MinY);
         float   fBitmapRight = Convert.ToSingle(m_oBitmapViewExtents.MaxX);
         
         float   fBitmapWidth = Convert.ToSingle(m_oBitmapViewExtents.MaxX - m_oBitmapViewExtents.MinX);
         float   fBitmapHeight = Convert.ToSingle(m_oBitmapViewExtents.MaxY - m_oBitmapViewExtents.MinY);
         

         // --- paint background white ---
         
         System.Drawing.Brush hBrush = System.Drawing.Brushes.White;
         hGraphics.FillRectangle(hBrush, 0, 0, pbImage.Width, pbImage.Height);         

         if (fBitmapBottom > fTop || fBitmapTop < fBottom || fBitmapLeft > fRight || fBitmapRight < fLeft)
            return;

         if (fWidth == 0 || fHeight == 0 || fBitmapWidth == 0 || fBitmapHeight == 0)
            return;

         // --- calculate destination rectangle ---
         
         float       fPercentLeft = (fLeft - fBitmapLeft) / fWidth;
         float       fPercentTop = (fTop - fBitmapTop) / fHeight;
         float       fPercentRight = (fRight - fBitmapRight) / fWidth;
         float       fPercentBottom = (fBottom - fBitmapBottom) / fHeight;
         RectangleF  oDestRect = new RectangleF();
                  
         if (fPercentLeft >= 0) // --- we are to the left of the area we wish to draw ---
            oDestRect.X = 0;            
         else  // --- we are to the right of the area we wish to draw ---
            oDestRect.X = Math.Abs(fPercentLeft) * pbImage.Width;
         
         if (fPercentRight >= 0) // --- we have the end within the area we within the area we wish to draw ---
            oDestRect.Width = (1 - fPercentRight) * pbImage.Width - oDestRect.X;
         else  // --- we have the end outside the area we within the area we wish to draw (to the right) ---
            oDestRect.Width = pbImage.Width - oDestRect.X;           

         if (fPercentTop >= 0) // --- we are below the top of the area we width to draw ---
            oDestRect.Y = fPercentTop * pbImage.Height;
         else // --- we are above the area we wish to draw ---
            oDestRect.Y = 0;

         if (fPercentBottom >= 0) // --- we are below the bottom of the area we wish to draw --
            oDestRect.Height = pbImage.Height - oDestRect.Y;
         else // --- we are within the area
            oDestRect.Height = (1 - Math.Abs(fPercentBottom)) * pbImage.Height - oDestRect.Y;



         // --- calculate source rectangle ---
         
         fPercentLeft = (fBitmapLeft - fLeft) / fBitmapWidth;
         fPercentTop = (fBitmapTop - fTop) / fBitmapHeight;
         fPercentRight = (fBitmapRight - fRight) /fBitmapWidth;
         fPercentBottom = (fBitmapBottom - fBottom) / fBitmapHeight;
         RectangleF  oSrcRect = new RectangleF();
                  
         if (fPercentLeft >= 0) // --- we are to the left the save bitmap ---
            oSrcRect.X = 0;            
         else  // --- we are to the right of the saved bitmap ---
            oSrcRect.X = Math.Abs(fPercentLeft) * m_oCurBitmap.Width;
         
         if (fPercentRight >= 0) // we do not want the entire width
            oSrcRect.Width = (1 - fPercentRight) * m_oCurBitmap.Width - oSrcRect.X;
         else
            oSrcRect.Width = m_oCurBitmap.Width - oSrcRect.X;

         if (fPercentTop >= 0) // --- we are below the saved bitmap ---
            oSrcRect.Y = fPercentTop * m_oCurBitmap.Height;
         else // --- we are above the saved bitmap ---
            oSrcRect.Y = 0;

         if (fPercentBottom >= 0) // we are below what we can give
            oSrcRect.Height = m_oCurBitmap.Height - oSrcRect.Y;
         else // --- we want all the remaining information
            oSrcRect.Height = (1 - Math.Abs(fPercentBottom)) * m_oCurBitmap.Height - oSrcRect.Y;

         
         // --- do not resize the image if it is going to be really really small ---

         if (oDestRect.Width < 10 && oDestRect.Height < 10)
            return;

         hGraphics.DrawImage(m_oCurBitmap, oDestRect, oSrcRect, GraphicsUnit.Pixel);         
      }

      /// <summary>
      /// Lightly paint over all areas that are not in the selected AOI
      /// </summary>
      protected void DrawRegion(System.Drawing.Graphics hGraphics)
      {
         // --- make sure we have a server connection ---
         
         if (GetDapData.Instance.CurServer == null) return;
         
         
         System.Drawing.Drawing2D.HatchBrush hBrush;         
         System.Drawing.Pen hPen;

         Geosoft.Dap.Common.BoundingBox   hSearchBoundingBox = GetDapData.Instance.SearchExtents;         

         double   dTop = hSearchBoundingBox.MaxY;
         double   dLeft = hSearchBoundingBox.MinX;
         double   dRight = hSearchBoundingBox.MaxX;
         double   dBottom = hSearchBoundingBox.MinY;

         double dWidth = GetDapData.Instance.ViewExtents.MaxX - GetDapData.Instance.ViewExtents.MinX;
         double dHeight = GetDapData.Instance.ViewExtents.MaxY - GetDapData.Instance.ViewExtents.MinY;
   
         double dPercentLeft = (hSearchBoundingBox.MinX - GetDapData.Instance.ViewExtents.MinX) / dWidth;
         double dPercentTop = (GetDapData.Instance.ViewExtents.MaxY - hSearchBoundingBox.MaxY) / dHeight;
         double dPercentWidth = (hSearchBoundingBox.MaxX - hSearchBoundingBox.MinX) / dWidth;
         double dPercentHeight = (hSearchBoundingBox.MaxY - hSearchBoundingBox.MinY) / dHeight;
   
         hPen = new Pen(System.Drawing.Color.Black, 1);
         hBrush = new System.Drawing.Drawing2D.HatchBrush(System.Drawing.Drawing2D.HatchStyle.BackwardDiagonal, System.Drawing.Color.LightBlue, System.Drawing.Color.Transparent);
         if (dWidth == 0) 
         {
            dPercentLeft = 0;
            dPercentWidth = 0;
         }
   
         if (dHeight == 0) 
         {
            dPercentTop = 0;
            dPercentHeight = 0;
         }
   
   
         // --- left border out of boundary area ---
   
         if (dLeft < GetDapData.Instance.ViewExtents.MinX) 
         {
            dPercentLeft = 0;
            dLeft = GetDapData.Instance.ViewExtents.MinX;
            dPercentWidth = (dRight - dLeft) / dWidth;
         }
   
         // --- top border out of boundary area ---
   
         if (dTop > GetDapData.Instance.ViewExtents.MaxY) 
         {
            dPercentTop = 0;
            dTop = GetDapData.Instance.ViewExtents.MaxY;
            dPercentHeight = (dTop - dBottom) / dHeight;
         }
   
         // --- right border out of boundary area ---
   
         if (dRight > GetDapData.Instance.ViewExtents.MaxX) 
         {
            dRight = GetDapData.Instance.ViewExtents.MaxX;
            dPercentWidth = (dRight - dLeft) / dWidth;
         }
   
         // --- bottom border out of boundary area ---
   
         if (dBottom < GetDapData.Instance.ViewExtents.MinY) 
         {
            dBottom = GetDapData.Instance.ViewExtents.MinY;
            dPercentHeight = (dTop - dBottom) / dHeight;
         }
   
     
         if (dPercentHeight < 0 || dPercentWidth < 0)
         {            
            hGraphics.FillRectangle(hBrush, 0, 0, pbImage.Width, pbImage.Height);
         } 
         else 
         {            
            if (dPercentLeft > 0)
            {
               hGraphics.FillRectangle(hBrush, 0, 0, Convert.ToSingle(pbImage.Width * dPercentLeft), Convert.ToSingle(pbImage.Height));
            }
      
            if (dPercentLeft + dPercentWidth < 1)
            {
               hGraphics.FillRectangle(hBrush, Convert.ToSingle(pbImage.Width * (dPercentLeft + dPercentWidth)), 0, Convert.ToSingle(pbImage.Width * (1 - dPercentLeft - dPercentWidth)), Convert.ToSingle(pbImage.Height));
            }
      
            if (dPercentTop > 0)
            {
               hGraphics.FillRectangle(hBrush, Convert.ToSingle(pbImage.Width * dPercentLeft), 0, Convert.ToSingle(pbImage.Width * dPercentWidth), Convert.ToSingle(pbImage.Height * dPercentTop));               
            }
      
            if (dPercentTop + dPercentHeight < 1)
            {
               hGraphics.FillRectangle(hBrush, Convert.ToSingle(pbImage.Width * dPercentLeft), Convert.ToSingle(pbImage.Height * (dPercentTop + dPercentHeight)), Convert.ToSingle(pbImage.Width * dPercentWidth), Convert.ToSingle(pbImage.Height * (1 - dPercentTop - dPercentHeight)));
            }            
            hGraphics.DrawRectangle(hPen, Convert.ToSingle(pbImage.Width * dPercentLeft), Convert.ToSingle(pbImage.Height * dPercentTop), Convert.ToSingle(pbImage.Width * dPercentWidth), Convert.ToSingle(pbImage.Height * dPercentHeight));
         }
      }

      /// <summary>
      /// Draw rubber band box
      /// </summary>
      /// <param name="p1"></param>
      /// <param name="p2"></param>
      private void DrawReversibleRectangle( System.Drawing.Point p1, System.Drawing.Point p2 )
      {
         Rectangle rc = new Rectangle();

         
         // --- Convert the points to screen coordinates. ---

         p1 = pbImage.PointToScreen( p1 );
         p2 = pbImage.PointToScreen( p2 );


         // --- Normalize the rectangle. ---

         if( p1.X < p2.X )
         {
            rc.X = p1.X;
            rc.Width = p2.X - p1.X;
         }
         else
         {
            rc.X = p2.X;
            rc.Width = p1.X - p2.X;
         }
         if( p1.Y < p2.Y )
         {
            rc.Y = p1.Y;
            rc.Height = p2.Y - p1.Y;
         }
         else
         {
            rc.Y = p2.Y;
            rc.Height = p1.Y - p2.Y;
         }

         // --- Draw the reversible frame. ---

         ControlPaint.DrawReversibleFrame(rc, System.Drawing.Color.FromArgb(0,255,255), FrameStyle.Thick);         
      }

      /// <summary>
      /// Modify all the controls to work with the larger map
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      protected void ToggleMapSize()
      {
         Int32 iWidthDiff = LARGE_MAP_WIDTH - SMALL_MAP_WIDTH;
         Int32 iHeightDiff = LARGE_MAP_HEIGHT - SMALL_MAP_HEIGHT;

         if (tbbMapSize.ImageIndex == SMALL_MAP)
         {
            // --- make into large map ---

            pBrowseMap.Width += iWidthDiff;
            pBrowseMap.Height += iHeightDiff;

            lMaxY.Left += iWidthDiff;

            lMinY.Left += iWidthDiff;
            lMinY.Top += iHeightDiff;

            lMaxX.Left += iWidthDiff;
            lMaxX.Top += iHeightDiff;

            lMinX.Top += iHeightDiff;

            pbImage.Width += iWidthDiff;
            pbImage.Height += iHeightDiff;

            lCoordinateSystem.Top += iHeightDiff;
            lCoordinateSystem.Left = pbImage.Left + pbImage.Width / 2 - lCoordinateSystem.Width / 2;

            tbbMapSize.ImageIndex = LARGE_MAP;

            pSearch.Top = pBrowseMap.Bottom + 8;
         } 
         else
         {
            // --- make into small map ---

            // --- make into large map ---

            pBrowseMap.Width -= iWidthDiff;
            pBrowseMap.Height -= iHeightDiff;

            lMaxY.Left -= iWidthDiff;

            lMinY.Left -= iWidthDiff;
            lMinY.Top -= iHeightDiff;

            lMaxX.Left -= iWidthDiff;
            lMaxX.Top -= iHeightDiff;

            lMinX.Top -= iHeightDiff;

            pbImage.Width -= iWidthDiff;
            pbImage.Height -= iHeightDiff;

            lCoordinateSystem.Top -= iHeightDiff;
            lCoordinateSystem.Left = pbImage.Left + pbImage.Width / 2 - lCoordinateSystem.Width / 2;

            tbbMapSize.ImageIndex = SMALL_MAP;

            pSearch.Top = pBrowseMap.Bottom + 8;
         }

         DrawImage();
      }
      #endregion

      #region Search
      /// <summary>
      /// Populate the list of controls
      /// </summary>
      protected void PopulateLists()
      {
         // --- populate the aoi drop down list ---

         ArrayList   hAOIList;
         if (GetDapData.Instance.CurServer.ServerConfiguration == null)
            hAOIList = new ArrayList();
         else
            hAOIList = GetDapData.Instance.CurServer.ServerConfiguration.GetAreaList();
         hAOIList.Insert(0, "[None]");
         cbRegion.DataSource = hAOIList;        
 
         bSearch.Enabled = false;     
      }
      #endregion

      /// <summary>
      /// Update the counts for each server
      /// </summary>
      protected void RefreshCounts()
      {
         // --- do not execute if we are cleanup up --- 
         
         if (this.IsDisposed) return;


         m_bSelectIndex = true;
         for (int i = 0; i < GetDapData.Instance.ServerList.Count; i++)
         {
            Server oServer = (Server)GetDapData.Instance.ServerList.GetByIndex(i);

            if (oServer.Secure && !oServer.LoggedIn)
               cbServerList.Items[i] = oServer.Name + " (unauthorized)";
            else if (oServer.Status == Server.ServerStatus.OnLine)
               cbServerList.Items[i] = oServer.Name + " (" + oServer.DatasetCount.ToString() + ")";
            else if (oServer.Status == Server.ServerStatus.Maintenance)
               cbServerList.Items[i] = oServer.Name + " (undergoing maintenance)";
         }  
         m_bSelectIndex = false;
      }
      #endregion

      #region Event Handlers
      #region Browser Map
      /// <summary>
      /// Override the paint function for the picture so that we can draw in the selected area
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void pbImage_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
      {
         DrawImage(e.Graphics);
         DrawRegion(e.Graphics);           
      }
      
      /// <summary>
      /// Start the aoi selection box
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void pbImage_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
      {
         // --- Make a note that we "have the mouse". ---

         m_bHaveMouse = true;


         // --- Store the "starting point" for this rubber-band rectangle. ---

         m_ptOriginal = new Point(e.X, e.Y);
         

         // --- Special value lets us know that no previous rectangle needs to be erased. ---

         m_ptLast.X = -1;
         m_ptLast.Y = -1;  
      }

      /// <summary>
      /// Increase size of aoi selection box
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void pbImage_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
      {
         int iX = e.X;
         int iY = e.Y;

         if (iX < 0) iX = 0;
         if (iX > pbImage.Width) iX = pbImage.Width;
         if (iY < 0) iY = 0;
         if (iY > pbImage.Height) iY = pbImage.Height;

         System.Drawing.Point ptCurrent = new Point(iX, iY);

         
         // --- If we "have the mouse", then we draw our lines ---

         if( m_bHaveMouse )
         {
            
            // --- If we have drawn previously, draw again in that spot to remove the lines. ---

            if( m_ptLast.X != -1 )
            {
               DrawReversibleRectangle( m_ptOriginal, m_ptLast );
            }
         
            m_ptLast = ptCurrent;
            DrawReversibleRectangle( m_ptOriginal, ptCurrent );
         }    
      }

      /// <summary>
      /// Carry out selected text for aoi region
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void pbImage_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
      {
         Geosoft.Dap.Common.BoundingBox   hSearchBoundingBox = GetDapData.Instance.SearchExtents;
         
         double dMaxX, dMinX, dMaxY, dMinY;
         double dWidth, dHeight, dNewCenterX, dNewCenterY, dClickWidthPercent, dClickHeightPercent;


         // --- Set internal flag to know we no longer "have the mouse". ---

         m_bHaveMouse = false;

         
         // --- If we have drawn previously, draw again in that spot to remove the lines. ---
         
         if( m_ptLast.X != -1 )
         {
            DrawReversibleRectangle( m_ptOriginal, m_ptLast );
         } 
         else 
         {
            m_ptLast.X = m_ptOriginal.X;
            m_ptLast.Y = m_ptOriginal.Y;
         }

         // --- Calculate the new bounding box ---

         dMaxX = Math.Max(m_ptOriginal.X, m_ptLast.X);
         dMinX = Math.Min(m_ptOriginal.X, m_ptLast.X);
         dMaxY = Math.Max(m_ptOriginal.Y, m_ptLast.Y);
         dMinY = Math.Min(m_ptOriginal.Y, m_ptLast.Y);

         // --- calculate the where the click was as a percentage of the height and width of the image

         dWidth = GetDapData.Instance.ViewExtents.MaxX - GetDapData.Instance.ViewExtents.MinX;
         dHeight = GetDapData.Instance.ViewExtents.MaxY - GetDapData.Instance.ViewExtents.MinY;

         if (tbbAOI.Pushed)
         {
            dClickWidthPercent = dMinX / Convert.ToDouble(pbImage.Width);
            dClickHeightPercent = dMinY / Convert.ToDouble(pbImage.Height);

            if (dMaxX != dMinX && dMaxY != dMinY)
            {

               // --- Zooming based on a dragged rectangle ---

               dNewCenterX = GetDapData.Instance.ViewExtents.MinX + dWidth * dClickWidthPercent;
               dNewCenterY = GetDapData.Instance.ViewExtents.MaxY - dHeight * dClickHeightPercent;

               dClickWidthPercent = dMaxX / Convert.ToDouble(pbImage.Width);
               dClickHeightPercent = dMaxY / Convert.ToDouble(pbImage.Height);

               hSearchBoundingBox.MaxX = GetDapData.Instance.ViewExtents.MinX + dWidth * dClickWidthPercent;
               hSearchBoundingBox.MinY = GetDapData.Instance.ViewExtents.MaxY - dHeight * dClickHeightPercent;
               hSearchBoundingBox.MinX = dNewCenterX;
               hSearchBoundingBox.MaxY = dNewCenterY;

               OnAOISelect(new AOISelectArgs(hSearchBoundingBox.MaxX, hSearchBoundingBox.MinX, hSearchBoundingBox.MaxY, hSearchBoundingBox.MinY));

               UpdateAOI();
            } 
         }
         else if (tbbZoomIn.Pushed)
         {
            dClickWidthPercent = dMinX / Convert.ToDouble(pbImage.Width);
            dClickHeightPercent = dMinY / Convert.ToDouble(pbImage.Height);

            if (dMaxX != dMinX && dMaxY != dMinY)
            {

               // --- Zooming based on a dragged rectangle ---

               dNewCenterX = GetDapData.Instance.ViewExtents.MinX + dWidth * dClickWidthPercent;
               dNewCenterY = GetDapData.Instance.ViewExtents.MaxY - dHeight * dClickHeightPercent;

               dClickWidthPercent = dMaxX / Convert.ToDouble(pbImage.Width);
               dClickHeightPercent = dMaxY / Convert.ToDouble(pbImage.Height);

               GetDapData.Instance.ViewExtents.MaxX = GetDapData.Instance.ViewExtents.MinX + dWidth * dClickWidthPercent;
               GetDapData.Instance.ViewExtents.MinY = GetDapData.Instance.ViewExtents.MaxY - dHeight * dClickHeightPercent;
               GetDapData.Instance.ViewExtents.MinX = dNewCenterX;
               GetDapData.Instance.ViewExtents.MaxY = dNewCenterY;
            } 
            else
            {
               
               // --- move the top left hand corner to the new center

               dNewCenterX = GetDapData.Instance.ViewExtents.MinX + dWidth * dClickWidthPercent;
               dNewCenterY = GetDapData.Instance.ViewExtents.MaxY - dHeight * dClickHeightPercent;
               GetDapData.Instance.ViewExtents.MaxX = dNewCenterX + dWidth / 2 / 2;
               GetDapData.Instance.ViewExtents.MinX = dNewCenterX - dWidth / 2 / 2;
               GetDapData.Instance.ViewExtents.MaxY = dNewCenterY + dHeight / 2 / 2;
               GetDapData.Instance.ViewExtents.MinY = dNewCenterY - dHeight / 2 / 2;
            }
            DrawImage();
         } 
         else if (tbbPan.Pushed)
         {
            dClickWidthPercent = (dMaxX + dMinX) / 2 / Convert.ToDouble(pbImage.Width);
            dClickHeightPercent = (dMaxY + dMinY) / 2 / Convert.ToDouble(pbImage.Height);

            dNewCenterX = GetDapData.Instance.ViewExtents.MinX + dWidth * dClickWidthPercent;
            dNewCenterY = GetDapData.Instance.ViewExtents.MaxY - dHeight * dClickHeightPercent;
            GetDapData.Instance.ViewExtents.MaxX = dNewCenterX + dWidth / 2;
            GetDapData.Instance.ViewExtents.MinX = dNewCenterX - dWidth / 2;
            GetDapData.Instance.ViewExtents.MaxY = dNewCenterY + dHeight / 2;
            GetDapData.Instance.ViewExtents.MinY = dNewCenterY - dHeight / 2;
            DrawImage();
         }
         else if (tbbZoomOut.Pushed)
         {
            dClickWidthPercent = (dMaxX + dMinX) / 2 / Convert.ToDouble(pbImage.Width);
            dClickHeightPercent = (dMaxY + dMinY) / 2 / Convert.ToDouble(pbImage.Height);

            dNewCenterX = GetDapData.Instance.ViewExtents.MinX + dWidth * dClickWidthPercent;
            dNewCenterY = GetDapData.Instance.ViewExtents.MaxY - dHeight * dClickHeightPercent;
            GetDapData.Instance.ViewExtents.MaxX = dNewCenterX + dWidth / 2 * 2;
            GetDapData.Instance.ViewExtents.MinX = dNewCenterX - dWidth / 2 * 2;
            GetDapData.Instance.ViewExtents.MaxY = dNewCenterY + dHeight / 2 * 2;
            GetDapData.Instance.ViewExtents.MinY = dNewCenterY - dHeight / 2 * 2;
            DrawImage();
         }         


         // --- Set flags to know that there is no "previous" line to reverse. ---

         m_ptLast.X = -1;
         m_ptLast.Y = -1;
         m_ptOriginal.X = -1;
         m_ptOriginal.Y = -1;               
      }

      /// <summary>
      /// Toggle the tool selected
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void tbToolbar_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
      {         
         if (e.Button == tbbFullView)
         {            
            // --- redraw the image with the new aoi ---

            double   dWidth = GetDapData.Instance.CurServer.ServerExtents.MaxX - GetDapData.Instance.CurServer.ServerExtents.MinX;
            double   dHeight = GetDapData.Instance.CurServer.ServerExtents.MaxY - GetDapData.Instance.CurServer.ServerExtents.MinY;

            dWidth *= AOI_FACTOR;
            dHeight *= AOI_FACTOR;

            GetDapData.Instance.ViewExtents.MaxX = GetDapData.Instance.CurServer.ServerExtents.MaxX + dWidth;
            GetDapData.Instance.ViewExtents.MaxY = GetDapData.Instance.CurServer.ServerExtents.MaxY + dHeight;
            GetDapData.Instance.ViewExtents.MinX = GetDapData.Instance.CurServer.ServerExtents.MinX - dWidth;
            GetDapData.Instance.ViewExtents.MinY = GetDapData.Instance.CurServer.ServerExtents.MinY;

            DrawImage();
         } 
         else if (e.Button == tbbMapSize)
         {
            ToggleMapSize();
         }
         else 
         {
            tbbAOI.Pushed = false;
            tbbZoomIn.Pushed = false;
            tbbZoomOut.Pushed = false;
            tbbPan.Pushed = false;
            tbbFullView.Pushed = false;
            e.Button.Pushed = true;
         }      
      }      

      /// <summary>
      /// A layer checkbox has changed redraw the image
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void cbLayer_CheckedChanged(object sender, System.EventArgs e)
      {
         DrawImage();
      }

      /// <summary>
      /// Hide/Show the image
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void BrowseWindow_EnabledChanged(object sender, System.EventArgs e)
      {
         // --- do nothing ---
      }      
      #endregion

      #region Search
      /// <summary>
      /// Update the search extents
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void bSearch_Click(object sender, System.EventArgs e)
      {
         try
         {
            double   dMaxX, dMaxY, dMinX, dMinY;
            int      iIndex;
            string   strSearchString = tbSearch.Text;

            dMaxX = Double.Parse(tbMaxX.Text);
            dMaxY = Double.Parse(tbMaxY.Text);
            dMinX = Double.Parse(tbMinX.Text);
            dMinY = Double.Parse(tbMinY.Text);

            if (dMinX >= dMaxX || dMinY >= dMaxY) 
            {
               MessageBox.Show("Invalid coordinates specified in bounding box", "Invalid bounding box", MessageBoxButtons.OK, MessageBoxIcon.Warning);
               return;
            }

            GetDapData.Instance.SearchExtents.MaxX = dMaxX;
            GetDapData.Instance.SearchExtents.MaxY = dMaxY;
            GetDapData.Instance.SearchExtents.MinX = dMinX;
            GetDapData.Instance.SearchExtents.MinY = dMinY;

            if (strSearchString.Length > 0)
            {
               if (rbName.Checked)
                  strSearchString = string.Format("Name contains({0})", tbSearch.Text);
               else if (rbDescription.Checked)
                  strSearchString = string.Format("Description contains({0})", tbSearch.Text);
               else if (rbKeywords.Checked)
                  strSearchString = string.Format("Keywords contains({0})", tbSearch.Text);
            }
               
            GetDapData.Instance.QueryString = strSearchString;

            iIndex = cbRegion.SelectedIndex;

            UpdateAOI();
            OnAOISelect(new AOISelectArgs());

            bSearch.Enabled = false;
            cbRegion.SelectedIndex = iIndex;
         } 
         catch (Exception ex)
         {
            MessageBox.Show("Invalid coordinate specified in bounding box", "Invalid bounding box", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            GetDapError.Instance.Write("Search - " + ex.Message);
         }
      }

      /// <summary>
      /// Reset the search extents
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void bReset_Click(object sender, System.EventArgs e)
      {
         // --- reset the search extents to that of the server ---

         
         GetDapData.Instance.SearchExtents.MaxX = GetDapData.Instance.CurServer.ServerExtents.MaxX;
         GetDapData.Instance.SearchExtents.MaxY = GetDapData.Instance.CurServer.ServerExtents.MaxY;
         GetDapData.Instance.SearchExtents.MinX = GetDapData.Instance.CurServer.ServerExtents.MinX;
         GetDapData.Instance.SearchExtents.MinY = GetDapData.Instance.CurServer.ServerExtents.MinY;
    

         GetDapData.Instance.QueryString = String.Empty;
         cbRegion.SelectedIndex = -1;
         tbSearch.Text = String.Empty;

         UpdateAOI();      
         OnAOISelect(new AOISelectArgs());

         bSearch.Enabled = false;
      }

      /// <summary>
      /// Set the region of interest based on the area of interest drop down list
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void cbRegion_SelectedIndexChanged(object sender, System.EventArgs e)
      {
         Geosoft.Dap.Common.BoundingBox  hBoundingBox = new Geosoft.Dap.Common.BoundingBox();

         double dMaxX, dMaxY, dMinX, dMinY;
         string strCS;

         if (cbRegion.SelectedIndex > 0)
         {
            GetDapData.Instance.CurServer.ServerConfiguration.GetBoundingBox((string)cbRegion.SelectedItem,out dMaxX, out dMaxY, out dMinX, out dMinY, out strCS);
            
            hBoundingBox.MaxX = dMaxX;
            hBoundingBox.MaxY = dMaxY;
            hBoundingBox.MinX = dMinX;
            hBoundingBox.MinY = dMinY;
            hBoundingBox.CoordinateSystem.Projection = strCS;

            if (Constant.Reproject(hBoundingBox, GetDapData.Instance.CurServer.ServerExtents.CoordinateSystem))
            {
               m_bUpdateRegion = true;

               tbMaxX.Text = Constant.FormatCoordinate(hBoundingBox.MaxX);
               tbMaxY.Text = Constant.FormatCoordinate(hBoundingBox.MaxY);
               tbMinX.Text = Constant.FormatCoordinate(hBoundingBox.MinX);
               tbMinY.Text = Constant.FormatCoordinate(hBoundingBox.MinY);

               m_bUpdateRegion = false;
               bSearch.Enabled = true;
            } 
            else 
            {
               MessageBox.Show(this, "Unable to convert the region \"" + (string)cbRegion.SelectedItem + "\" into the browser map coordinate system", "Cannot jump to region \"" + (string)cbRegion.SelectedItem + "\"", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
         }
      }

      /// <summary>
      /// Clear the selection in the drop down list as soon as any text is entered
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void tbCoordinate_TextChanged(object sender, System.EventArgs e)
      {
         if (m_bUpdateRegion) return;
         cbRegion.SelectedIndex = -1;
         bSearch.Enabled = true;
      }           

      /// <summary>
      /// Enable the search button
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void tbSearch_TextChanged(object sender, System.EventArgs e)
      {  
         bSearch.Enabled = true;
      }      
      #endregion

      /// <summary>
      /// Change the server we are currently looking at
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void cbServerList_SelectedIndexChanged(object sender, System.EventArgs e)
      {
         if (!m_bSelectIndex)
            OnSelectServer(new ServerSelectArgs(cbServerList.SelectedIndex));
      }

      private void pbHelp_Click(object sender, System.EventArgs e)
      {
         SearchHelp oDialog = new SearchHelp();
         oDialog.ShowDialog();
      
      }
      #endregion                       
	}
}
