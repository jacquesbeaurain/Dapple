using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms.Design;

using Geosoft.DotNetTools.FileDialogs;

namespace Geosoft.GX.DAPGetData
{  
   /// <summary>
   /// 
   /// </summary>
   public class SaveFileEditor : UITypeEditor
   {
      public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
      {
         Geosoft.DotNetTools.FileDialogs.FileDialog   hDialog;
         FileDialogFilterAttribute                    hFilter = (FileDialogFilterAttribute)context.PropertyDescriptor.Attributes[typeof(Geosoft.DotNetTools.FileDialogs.FileDialogFilterAttribute)];                 

         if (hFilter != null) 
         {
            string         strDirectory = string.Empty;
            //DatasetOptions hOptions = (DatasetOptions)context.Instance;
            
            //if (hOptions.Client == DatasetOptions.ClientType.MAPINFO)
            //{
            //   if (hOptions.GetType() == typeof(PointOptions))
            //   {                  
            //      hFilter = new FileDialogFilterAttribute(FileDialogFilterAttribute.FilterType.MapInfo);
            //   }                              
            //}
            //else if (hOptions.Client == DatasetOptions.ClientType.ARCGIS)
            //{
            //   if (hOptions.GetType() == typeof(PointOptions))
            //   {
            //         hFilter = new FileDialogFilterAttribute(FileDialogFilterAttribute.FilterType.ArcView);
            //   }
            //}
            

            hDialog = Geosoft.DotNetTools.FileDialogs.FileDialog.Create(hFilter);
            hDialog.SaveDialog = true;
            hDialog.Title = "Save File As";
            hDialog.FileName = (string)value;
            
            //if (hOptions.Client == DatasetOptions.ClientType.MAPINFO)
            //   Geosoft.GXNet.CSYS.GtString("MAPINFO_DAP_CLIENT", "WORKING_DIR", ref strDirectory);
            //else 
            //   Geosoft.GXNet.CSYS.IGetPath(Geosoft.GXNet.Constant.SYS_PATH_LOCAL, ref strDirectory);               

            hDialog.InitialDirectory = strDirectory;
            if (hDialog.Show())
               value = hDialog.FileName;
         }

         return value;
      }		
   
      public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext hContext)
      {
         return UITypeEditorEditStyle.Modal;         
      }
   }
}
