using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;

namespace Dapple
{
   // Invert the gradient and use square instead of rounded toolstrips to improve appearances better for all themes

   public class DappleToolStripColorTable : ProfessionalColorTable
   {
      public override Color ToolStripGradientBegin
      {
         get
         {
            return base.ToolStripGradientEnd;
         }
      }

      public override Color ToolStripGradientEnd
      {
         get
         {
            return base.ToolStripGradientBegin;
         }
      }
   }

   public class DappleToolStripRenderer : ToolStripProfessionalRenderer
   {
      public DappleToolStripRenderer()
         : base(new DappleToolStripColorTable())
      {
         // This class will make things look weird with rounded edges
         this.RoundedEdges = false;
      }

      // Border on other side to match the gradient
      protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
      {
         ToolStrip strip1 = e.ToolStrip;
         Graphics graphics1 = e.Graphics;
         if (!(strip1 is MenuStrip) && !(strip1 is StatusStrip))
         {
            Rectangle rectangle1 = new Rectangle(Point.Empty, strip1.Size);
            using (Pen pen1 = new Pen(this.ColorTable.ToolStripBorder))
            {
               if (strip1.Orientation == Orientation.Horizontal)
                  graphics1.DrawLine(pen1, rectangle1.Left, 0, rectangle1.Right, 0);
               else
                  graphics1.DrawLine(pen1, 0, 0, 0, rectangle1.Height - 1);
            }
         }
         else
            base.OnRenderToolStripBorder(e);
      }
   }

   public class BorderlessToolStripRenderer : ToolStripSystemRenderer
   {
      public BorderlessToolStripRenderer() : base()
      {
      }

      protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
      {
         // Don't
      }
   }
}

