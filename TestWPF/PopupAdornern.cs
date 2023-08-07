using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;

namespace TestWPF
{
    public class PopupAdornern : DecoratorAdorner
    {
        static PopupAdornern()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PopupAdornern), new FrameworkPropertyMetadata(typeof(PopupAdornern)));
        }

        public PopupAdornern(UIElement adornedElement) : base(adornedElement)
        {

        }

        public static readonly DependencyProperty PlacementModeProperty = DependencyProperty.Register("PlacementMode", typeof(PopupAdornerPlacementMode), typeof(PopupAdornern), new FrameworkPropertyMetadata(PopupAdornerPlacementMode.Bottom, FrameworkPropertyMetadataOptions.AffectsParentArrange));
        public PopupAdornerPlacementMode PlacementMode
        {
            get { return (PopupAdornerPlacementMode)GetValue(PlacementModeProperty); }
            set { SetValue(PlacementModeProperty, value); }
        }

        public static readonly DependencyProperty KeepWithinViewProperty = DependencyProperty.Register("KeepWithinView", typeof(bool), typeof(PopupAdornern), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsParentArrange));
        public bool KeepWithinView
        {
            get { return (bool)GetValue(KeepWithinViewProperty); }
            set { SetValue(KeepWithinViewProperty, value); }
        }

        private TransformInfo _transformInfo;

        public override GeneralTransform GetDesiredTransform(GeneralTransform generalTransform)
        {
            _transformInfo = default;

            if (generalTransform is Transform transform)
            {
                var matrix = transform.Value;

                var adornerLayer = AdornerLayer.GetAdornerLayer(AdornedElement);

                Size adornedElementSize;

                if (AdornedElement is FrameworkElement frameworkElement) adornedElementSize = new Size(frameworkElement.ActualWidth, frameworkElement.ActualHeight);
                else adornedElementSize = AdornedElement.RenderSize;

                _transformInfo = new TransformInfo
                {
                    Constraint = new Size(adornerLayer.ActualWidth, adornerLayer.ActualHeight),
                    Origin = new Point(matrix.OffsetX, matrix.OffsetY),
                    AdornedElementSize = adornedElementSize
                };

                CalculateOffset(PlacementMode, out double offsetX, out double offsetY);

                matrix.OffsetX = offsetX;
                matrix.OffsetY = offsetY;

                return new MatrixTransform(matrix);
            }

            return generalTransform;
        }

        protected bool FitsInAdornerLayer(double offsetX, double offsetY)
        {
            if (offsetX < 0) return false;
            if (offsetY < 0) return false;

            if (offsetX + ActualWidth > _transformInfo.Constraint.Width) return false;
            if (offsetY + ActualHeight > _transformInfo.Constraint.Height) return false;

            return true;
        }

        protected void CalculatePlacementModeOffset(PopupAdornerPlacementMode placementMode, out double offsetX, out double offsetY)
        {
            offsetX = _transformInfo.Origin.X;
            offsetY = _transformInfo.Origin.Y;

            switch (placementMode)
            {
                case PopupAdornerPlacementMode.Left:
                    offsetX -= ActualWidth;
                    break;
                case PopupAdornerPlacementMode.Top:
                    offsetY -= ActualHeight;
                    break;
                case PopupAdornerPlacementMode.Right:
                    offsetX += _transformInfo.AdornedElementSize.Width;
                    break;
                case PopupAdornerPlacementMode.Bottom:
                    offsetY += _transformInfo.AdornedElementSize.Height;
                    break;
                case PopupAdornerPlacementMode.Center:
                    offsetX += _transformInfo.AdornedElementSize.Width / 2 - ActualWidth / 2;
                    offsetY += _transformInfo.AdornedElementSize.Height / 2 - ActualHeight / 2;
                    break;
            }
        }

        private PopupAdornerPlacementMode? _lastFittingPlacementMode = null;

        private bool GetAlternativePlacementMode(PopupAdornerPlacementMode placementMode, out PopupAdornerPlacementMode alternativePlacementMode, out double offsetX, out double offsetY)
        {
            offsetX = _transformInfo.Origin.X;
            offsetY = _transformInfo.Origin.Y;

            alternativePlacementMode = PopupAdornerPlacementMode.Relative;

            PopupAdornerPlacementMode[] tryOrder = new PopupAdornerPlacementMode[4];

            switch (placementMode)
            {
                case PopupAdornerPlacementMode.Left:
                    tryOrder[0] = PopupAdornerPlacementMode.Right;
                    tryOrder[1] = PopupAdornerPlacementMode.Bottom;
                    tryOrder[2] = PopupAdornerPlacementMode.Top;
                    tryOrder[3] = PopupAdornerPlacementMode.Center;
                    break;
                case PopupAdornerPlacementMode.Top:
                    tryOrder[0] = PopupAdornerPlacementMode.Bottom;
                    tryOrder[1] = PopupAdornerPlacementMode.Right;
                    tryOrder[2] = PopupAdornerPlacementMode.Left;
                    tryOrder[3] = PopupAdornerPlacementMode.Center;
                    break;
                case PopupAdornerPlacementMode.Right:
                    tryOrder[0] = PopupAdornerPlacementMode.Left;
                    tryOrder[1] = PopupAdornerPlacementMode.Bottom;
                    tryOrder[2] = PopupAdornerPlacementMode.Top;
                    tryOrder[3] = PopupAdornerPlacementMode.Center;
                    break;
                case PopupAdornerPlacementMode.Bottom:
                    tryOrder[0] = PopupAdornerPlacementMode.Top;
                    tryOrder[1] = PopupAdornerPlacementMode.Right;
                    tryOrder[2] = PopupAdornerPlacementMode.Left;
                    tryOrder[3] = PopupAdornerPlacementMode.Center;
                    break;
                case PopupAdornerPlacementMode.Center:
                    tryOrder[0] = PopupAdornerPlacementMode.Bottom;
                    tryOrder[1] = PopupAdornerPlacementMode.Right;
                    tryOrder[2] = PopupAdornerPlacementMode.Left;
                    tryOrder[3] = PopupAdornerPlacementMode.Top;
                    break;
            }

            for (int i = 0; i < tryOrder.Length; i++)
            {
                var placement = tryOrder[i];

                CalculatePlacementModeOffset(placement, out offsetX, out offsetY);

                if (FitsInAdornerLayer(offsetX, offsetY))
                {
                    _lastFittingPlacementMode = placement;

                    alternativePlacementMode = placement;
                    return true;
                }
            }

            if (_lastFittingPlacementMode != null) CalculatePlacementModeOffset(_lastFittingPlacementMode.Value, out offsetX, out offsetY);

            return false;
        }

        protected virtual void CalculateOffset(PopupAdornerPlacementMode placementMode, out double offsetX, out double offsetY)
        {
            CalculatePlacementModeOffset(placementMode, out offsetX, out offsetY);

            if (FitsInAdornerLayer(offsetX, offsetY)) return;

            if (GetAlternativePlacementMode(placementMode, out _, out offsetX, out offsetY)) return;
            else if (KeepWithinView)
            {
                offsetX = Math.Max(Math.Min(_transformInfo.Origin.X, _transformInfo.Constraint.Width - ActualWidth), 0);
                offsetY = Math.Max(Math.Min(_transformInfo.Origin.Y, _transformInfo.Constraint.Height - ActualHeight), 0);
            }
        }

        private struct TransformInfo
        {
            public Size Constraint;

            public Point Origin;

            public Size AdornedElementSize;
        }
    }

    public enum PopupAdornerPlacementMode
    {
        Left,
        Top,
        Right,
        Bottom,
        Center,
        Relative
    }
}