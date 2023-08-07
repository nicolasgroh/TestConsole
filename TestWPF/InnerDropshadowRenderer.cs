using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace TestWPF
{
    public class InnerDropshadowRenderer : DrawingVisual
    {
        public void DrawDropShadows(Size renderSize, double dropshadowSize)
        {
            using (var drawingContext = RenderOpen())
            {
                drawingContext.DrawRectangle(GetLinearGradientBrush(new Point(0, 0.5), new Point(1, 0.5)), new Pen(), new Rect(new Point(0, 0), new Size(dropshadowSize, renderSize.Height)));
                drawingContext.DrawRectangle(GetLinearGradientBrush(new Point(0.5, 0), new Point(0.5, 1)), new Pen(), new Rect(new Point(0, 0), new Size(renderSize.Width, dropshadowSize)));
                drawingContext.DrawRectangle(GetLinearGradientBrush(new Point(1, 0.5), new Point(0, 0.5)), new Pen(), new Rect(new Point(renderSize.Width - dropshadowSize, 0), new Size(dropshadowSize, renderSize.Height)));
                drawingContext.DrawRectangle(GetLinearGradientBrush(new Point(0.5, 1), new Point(0.5, 0)), new Pen(), new Rect(new Point(0, renderSize.Height - dropshadowSize), new Size(renderSize.Width, dropshadowSize)));
            }
        }

        private static Brush GetLinearGradientBrush(Point startPoint, Point endPoint)
        {
            return new LinearGradientBrush(Colors.Black, Colors.Transparent, startPoint, endPoint);
        }
    }
}