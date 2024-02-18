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

                matrix.OffsetX = offsetX;
                matrix.OffsetY = offsetY;

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

            var location = BadgeService.GetLocation(adornedElement);

            switch (location)
            {
                case BadgeLocation.Center:
                    offsetX = adornedElementSize.Width / 2 - childSize.Width / 2;
                    offsetY = adornedElementSize.Height / 2 - childSize.Height / 2;
                    break;
                case BadgeLocation.TopLeft:
                    // Zero is what we want
                    break;
                case BadgeLocation.TopRight:
                    offsetX = adornedElementSize.Width - childSize.Width;
                    break;
                case BadgeLocation.BottomRight:
                    offsetX = adornedElementSize.Width - childSize.Width;
                    offsetY = adornedElementSize.Height - childSize.Height;
                    break;
                case BadgeLocation.BottomLeft:
                    offsetX = 0;
                    offsetY = adornedElementSize.Height - childSize.Height;
                    break;
            }

            var horizontalOffset = BadgeService.GetHorizontalOffset(adornedElement);
            var verticalOffset = BadgeService.GetVerticalOffset(adornedElement);

            offsetX += horizontalOffset;
            offsetY += verticalOffset;
        }
    }
}