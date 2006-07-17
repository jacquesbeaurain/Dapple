using System;
using System.Text;
using System.Drawing.Design;
using System.ComponentModel;
using Geosoft.PropertyHelpers;
using Geosoft.GXNet;
using Geosoft.Dap.Common;
using Geosoft.DotNetTools.FileDialogs;

namespace Geosoft.GX.DAPGetData
{
	/// <summary>
	/// List of options for documents
	/// </summary>
	[TypeConverter(typeof(PropertySorter))]
	public class DocumentOptions : DatasetOptions
	{
      #region Enums
      public enum DownloadOptions
      {
         [Description("Download And Open")]     DownloadAndOpen,
         [Description("Download Only")]         DownloadOnly
      }
      #endregion

      #region Member Variables
      protected string           m_strFileName;
      protected DownloadOptions  m_eDownloadOptions;
      #endregion

      #region Properties
      [PropertyOrder(11)]
      [Category("Data"), Description("Filename to save dataset as."), Editor("Geosoft.GX.DAPGetData.SaveFileEditor",typeof(UITypeEditor)),FileDialogFilter(FileDialogFilterAttribute.FilterType.Document)]
      public string Filename
      {
         get { return m_strFileName; }
         set { m_strFileName = value; }
      }

      [PropertyOrder(20)]
      [TypeConverter(typeof(EnumDescConverter))]
      [Category("Options"), Description("Download Options")]      
      public DownloadOptions Download
      {
         get { return m_eDownloadOptions; }
         set { m_eDownloadOptions = value; }
      }
      #endregion

      #region Constructor
		public DocumentOptions(string strServerName, string strTitle, string strUrl) : base(strServerName, strTitle, strUrl)
		{			
		}
      #endregion

      /// <summary>
      /// Download document dataset
      /// </summary>
      public override void DownloadDataset()
      {
         String         hStrDoc = Filename;
         
         CDSEL hDSEL = null;
         CIPJ  hIPJ = null;
         CDAP  hDAP = null;
         
         base.DownloadDataset();

         
         // --- disable interactive gx's ---

         CSYS.SetInteractive(0);


         CSYS.Progress(1);
         CSYS.ProgName("Displaying database " + Name + " ...", 0);

         hDSEL = SetExtractionBoundingBox(false);
         
         hDAP = CDAP.Create(ExtractUrl, "");
         hDAP.RequestDocumentData(ServerName, hDSEL, ref hStrDoc);

         if (Download == DownloadOptions.DownloadAndOpen)
         {
            // --- Open as Geosoft map if the file extension is MAP ---
             
            switch(System.IO.Path.GetExtension(hStrDoc.ToLower()))
            {
               case ".map":
                  if (m_eClient == ClientType.OASIS_MONTAJ)
                     CEMAP.Load(hStrDoc.ToString());
                  else if (m_eClient == ClientType.ARCGIS)
                     CARCMAP.iLoadMAP(hStrDoc.ToString(),"","",Geosoft.GXNet.Constant.ARCMAP_LOAD_EXISTFRAME + Geosoft.GXNet.Constant.ARCMAP_LOAD_COPYLAYER + Geosoft.GXNet.Constant.ARCMAP_LOAD_INTOCURRENTFRAME + Geosoft.GXNet.Constant.ARCMAP_LOAD_MERGETOSINGLEVIEW);
                  else if (m_eClient == ClientType.MAPINFO)
                  {
                     WriteToMapInfoFileList(hStrDoc);
                  }
                  break;
               case ".gdb":
                  if (m_eClient == ClientType.OASIS_MONTAJ)
                     CEDB.Load(hStrDoc.ToString());
                  else if (m_eClient == ClientType.ARCGIS)
                  {               
                     CSYS.SetString("EXPSHP","FILE",hStrDoc.ToString());
                     CSYS.SetString("EXPSHP","CHAN","A");
                     CSYS.SetString("EXPSHP","LINE","A");
                     CSYS.SetString("EXPSHP","SINGLE","S");
                     CSYS.iRunGX("expshp");
                  }              
                  else if (m_eClient == ClientType.MAPINFO)
                  {
                     CDOCU hDOCU;
                     // --- Open document in generic way ---
   
                     hDOCU = CDOCU.Create();
                     hDOCU.SetFile("","",hStrDoc);
                     hDOCU.Open(Geosoft.GXNet.Constant.DOCU_OPEN_VIEW);
                     hDOCU.Dispose();
                  }
                  break;
               default:           
               {
                  CDOCU hDOCU;
                  // --- Open document in generic way ---
   
                  hDOCU = CDOCU.Create();
                  hDOCU.SetFile("","",hStrDoc);
                  hDOCU.Open(Geosoft.GXNet.Constant.DOCU_OPEN_VIEW);
                  hDOCU.Dispose();
                  break;
               }
            }
         }

         CSYS.Progress(0);

         if (hDSEL != null) hDSEL.Dispose();
         if (hIPJ != null)  hIPJ.Dispose();
         if (hDAP != null) hDAP.Dispose();
      }
	}
}
