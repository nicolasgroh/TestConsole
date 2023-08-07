using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace TestWPF
{
    public class GeometryTest : Decorator
    {
        protected override void OnRender(DrawingContext drawingContext)
        {
            var geometry = new StreamGeometry();

            var firstWidth = 1.0;

            var firstOffset = Math.Sqrt(Math.Sqrt(firstWidth) / 2);



            using (var stream = geometry.Open())
            {
                stream.BeginFigure(new Point(0, 0), true, true);

                stream.LineTo(new Point(10, 0), false, false);
                stream.LineTo(new Point(10, 2), false, false);
                stream.LineTo(new Point(1 + 1 + 1 + 1 - firstOffset, 2), false, false);
                stream.LineTo(new Point(10 + firstOffset, 10 - firstOffset), false, false);
                stream.LineTo(new Point(10, 10), false, false);
            }

            drawingContext.DrawGeometry(Brushes.Black, null, geometry);
        }
    }
}