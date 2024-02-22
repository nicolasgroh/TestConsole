using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace TestWPF
{
    public class BadgeAdorner : DecoratorAdorner
    {
        internal BadgeAdorner(UIElement adornedElement, Badge badge) : base(adornedElement)
        {
            Child = badge;
        }

        public override GeneralTransform GetDesiredTransform(GeneralTransform generalTransform)
        {
            if (generalTransform is Transform transform)
            {
                CalculateOffsets(out var offsetX, out var offsetY);

                var matrix = transform.Value;

                matrix.OffsetX += offsetX;
                matrix.OffsetY += offsetY;

                return new MatrixTransform(matrix);
            }

            return generalTransform;
        }

        private void CalculateOffsets(out double offsetX, out double offsetY)
        {
            offsetX = 0;
            offsetY = 0;

            var adornedElement = AdornedElement;

            var adornedElementSize = adornedElement.RenderSize;
            var childSize = Child.DesiredSize;

            var horizontalOffset = BadgeService.GetHorizontalOffset(adornedElement);
            var verticalOffset = BadgeService.GetVerticalOffset(adornedElement);

            offsetX = adornedElementSize.Width * horizontalOffset;
            offsetX -= childSize.Width * horizontalOffset;

            offsetY = adornedElementSize.Height * verticalOffset;
            offsetY -= childSize.Height * verticalOffset;

            if (offsetX < 0) offsetX = 0;
            if (offsetY < 0) offsetY = 0;
        }
    }
}