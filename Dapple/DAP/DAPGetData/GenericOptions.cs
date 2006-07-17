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
   /// List of options for generic datasets
   /// </summary>
   [TypeConverter(typeof(PropertySorter))]
   public class GenericOptions : DatasetOptions
   {      
      #region Constructor
      /// <summary>
      /// Default constructor
      /// </summary>
      public GenericOptions(string strServerName, string strTitle, string strUrl) : base(strServerName, strTitle, strUrl)
      {			
      }
      #endregion
      
      /// <summary>
      /// Download the dataset
      /// </summary>
      public override void DownloadDataset()
      {
         CDAP hDDSDAP = null;
         CDAP hDCSDAP = null;
         Int32 iAttribToken = GXNet.Constant.H_META_INVALID_TOKEN;
         Int32 iClassToken = GXNet.Constant.H_META_INVALID_TOKEN;
         CMETA hMeta = null;
         CMETA hDataSetMeta = null;
         string strGenType = string.Empty;
         string strName = string.Empty;
         string strDatum = string.Empty;
         string strMethod = string.Empty;
         string strUnit = string.Empty;
         string strTransform = string.Empty;


         // -- Call base class first ---

         base.DownloadDataset();


         // --- We do not support acQuire downloads from MapInfo ---

         if (m_eClient == ClientType.MAPINFO)
         {
            System.Windows.Forms.MessageBox.Show("Unable to download \"" + Name + " \" because acQuire is not a supported format.", "Unable to retrieve \"" + Name + "\"", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
            return;
         }


         try
         {
            // --- Enable interactive GXs ---

            CSYS.SetInteractive(1);
         
            
            // --- Update progress ---

            CSYS.Progress(1);
            CSYS.ProgName("Displaying dataset " + Name + " ...", 0);         
         
            
            // --- Initiate DAP communication ---

            hDCSDAP = CDAP.Create(MetaUrl, "Retrieve dataset information.");
            hDDSDAP = CDAP.Create(ExtractUrl, "Retrieve requested data.");                     
            hMeta = hDCSDAP.DescribeDataSet(ServerName);
            iClassToken = hMeta.ResolveUMN("CLASS:/Geosoft/Core/DAP/Data/DatasetInfo");
            iAttribToken = hMeta.ResolveUMN("ATTRIB:/Geosoft/Core/DAP/Data/DatasetInfo/Information");


            // --- Get the META to display ---

            hDataSetMeta = Geosoft.GXNet.CMETA.Create();
            hMeta.GetAttribOBJ(iClassToken, iAttribToken, hDataSetMeta);
         
            // --- Define the actual user AOI ---

            Geosoft.Dap.Common.BoundingBox oBBox = null;
            GenerateBoundingBox(out oBBox);

            
            // --- Set current bounding box information for acQuireDirect dialog ---

            CSYS.SetReal("DAP_ACQUIRE","BOUNDING_BOX_MINX",oBBox.MinX);
            CSYS.SetReal("DAP_ACQUIRE","BOUNDING_BOX_MINY",oBBox.MinY);
            CSYS.SetReal("DAP_ACQUIRE","BOUNDING_BOX_MAXX",oBBox.MaxX);
            CSYS.SetReal("DAP_ACQUIRE","BOUNDING_BOX_MAXY",oBBox.MaxY);
            
            
            // --- Set current projection information for acQuireDirect dialog ---
            
            CSYS.SetString("DAP_ACQUIRE","PROJECTION_NAME",oBBox.CoordinateSystem.Projection);
            CSYS.SetString("DAP_ACQUIRE","PROJECTION_METHOD",oBBox.CoordinateSystem.Method);
            CSYS.SetString("DAP_ACQUIRE","PROJECTION_DATUM",oBBox.CoordinateSystem.Datum);
            CSYS.SetString("DAP_ACQUIRE","PROJECTION_LOCALTRANSFORM",oBBox.CoordinateSystem.LocalDatum);

            
            // --- Disable the DAP dialog ---

            GetDapData.Instance.Enabled = false;
            
            
            // --- Run (in this case) the acQuire GXs to extract the data from acQuire ---
            
            hDDSDAP.RequestGenericData(ServerName, hDataSetMeta);
         } 
         catch (Exception e)
         {
            GetDapError.Instance.Write("Download generic - " + e.Message);
         }
         finally
         {
            // --- Reset current bounding box and projection information ---

            CSYS.ClearGroup("DAP_ACQUIRE");

   
            // --- Re-enable the DAP dialog ---

            GetDapData.Instance.Enabled = true;
            
            
            // --- Reset progress ---

            CSYS.Progress(0);

            
            // --- Clean up resources ---

            if (hDCSDAP != null)    hDCSDAP.Dispose();
            if (hDDSDAP != null)    hDDSDAP.Dispose();
         }
      }
   }
}
