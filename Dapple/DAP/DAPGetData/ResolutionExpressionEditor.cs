using System;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms.Design;

namespace Geosoft.GX.DAPGetData
{
	/// <summary>
	/// Summary description for MapInfoFile.
	/// </summary>
	public class ResolutionExpressionEditor : UITypeEditor
	{
      public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
      {
         IWindowsFormsEditorService   hService;
         
         if (context != null && provider != null)
         {
            hService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));

            if (hService != null) 
            {     
               DatasetOptions oOptions = (DatasetOptions)context.Instance;

               if (context.Instance is GridOptions || context.Instance is PictureOptions)
               {                  
                  int      iClassToken, iXToken, iYToken, iXSize = 0, iYSize = 0;
                  double   dXCellSize = 0, dYCellSize = 0, dXOrigin = 0, dYOrigin = 0;

                  try
                  {
                     // --- get the size and cell size of the image ---

                     iClassToken = oOptions.META.ResolveUMN("CLASS:/Geosoft/Data/Grid");
                     iXToken = oOptions.META.ResolveUMN("ATTRIB:/Geosoft/Data/Grid/X Dimension");
                     iYToken = oOptions.META.ResolveUMN("ATTRIB:/Geosoft/Data/Grid/Y Dimension");

                     oOptions.META.GetAttribInt(iClassToken, iXToken, ref iXSize);
                     oOptions.META.GetAttribInt(iClassToken, iYToken, ref iYSize);

                     iClassToken = oOptions.META.ResolveUMN("CLASS:/Geosoft/Data/Grid/Location");
                     iXToken = oOptions.META.ResolveUMN("ATTRIB:/Geosoft/Data/Grid/Location/X Origin");
                     iYToken = oOptions.META.ResolveUMN("ATTRIB:/Geosoft/Data/Grid/Location/Y Origin");

                     oOptions.META.GetAttribReal(iClassToken, iXToken, ref dXOrigin);
                     oOptions.META.GetAttribReal(iClassToken, iYToken, ref dYOrigin);

                     iXToken = oOptions.META.ResolveUMN("ATTRIB:/Geosoft/Data/Grid/Location/X Point Separation");
                     iYToken = oOptions.META.ResolveUMN("ATTRIB:/Geosoft/Data/Grid/Location/Y Point Separation");

                     oOptions.META.GetAttribReal(iClassToken, iXToken, ref dXCellSize);
                     oOptions.META.GetAttribReal(iClassToken, iYToken, ref dYCellSize);
   
                     ResolutionDialog  oResolutionDialog =  new ResolutionDialog((double)value, dXOrigin, dYOrigin, iXSize, iYSize, dXCellSize, dYCellSize, oOptions.IPJ);
                  
                     if (oResolutionDialog.ShowDialog() == DialogResult.OK)
                        value = oResolutionDialog.Resolution;
                  } 
                  catch {}
               }               
               else if (context.Instance is MapOptions)
               {
                  Geosoft.GXNet.CLTB   oLTB = null;
                  string               strTempFile = string.Empty;
                  int                  iClassToken = -1;

                  try
                  {
                     // --- get the list of resolutions possible for this map ---

                     strTempFile = System.IO.Path.GetTempFileName();
                     System.IO.File.Delete(strTempFile);
                     strTempFile = System.IO.Path.ChangeExtension(strTempFile, ".csv");

                     iClassToken = oOptions.META.ResolveUMN("CLASS:/Geosoft/Data/HMAP/Indexes");

                     if (iClassToken != -1)
                     {
                        int         iRecords;
                        int         iField1;
                        int         iField2;
                        double      dResolution;
                        int         iDataPoints;
                        SortedList  oList = new SortedList();

                        oOptions.META.ExportTableCSV(iClassToken, strTempFile);

                        oLTB = Geosoft.GXNet.CLTB.Create(strTempFile, Geosoft.GXNet.Constant.LTB_TYPE_HEADER, Geosoft.GXNet.Constant.LTB_DELIM_COMMA, string.Empty);

                        iRecords = oLTB.iRecords();
                        iField1 = oLTB.iFindField("2"); //ATTRIB:/Geosoft/Data/HMAP/Indexes/Resolution
                        iField2 = oLTB.iFindField("0"); //ATTRIB:/Geosoft/Data/HMAP/Indexes/DataPoints

                        for (int i = 0; i < iRecords; i++)
                        {
                           dResolution = oLTB.rGetReal(i, iField1);
                           iDataPoints = oLTB.iGetInt(i, iField2);
                           oList.Add(dResolution, iDataPoints);
                        }

                        ResolutionDialog  oResolutionDialog =  new ResolutionDialog((double)value, oOptions.MinX, oOptions.MinY, oOptions.MaxX, oOptions.MaxY, oList, oOptions.IPJ);
                  
                        if (oResolutionDialog.ShowDialog() == DialogResult.OK)
                           value = oResolutionDialog.Resolution;
                     }
                  }
                  catch {}
                  finally
                  {
                     if (strTempFile != string.Empty) System.IO.File.Delete(strTempFile);
                     if (oLTB != null) oLTB.Dispose();
                  }
               }
            }
         }
         return value;
      }		
      
      public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext hContext)
      {
         return UITypeEditorEditStyle.Modal;         
      }      
	}
}
