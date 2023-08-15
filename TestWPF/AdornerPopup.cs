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
    public class AdornerPopup : AdornerDecorator
    {
        static AdornerPopup()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AdornerPopup), new FrameworkPropertyMetadata(typeof(AdornerPopup)));
        }

        public AdornerPopup(UIElement adornedElement) : base(adornedElement)
        {

        }

        public static readonly DependencyProperty PlacementModeProperty = DependencyProperty.Register("PlacementMode", typeof(AdornerPopupPlacementMode), typeof(AdornerPopup), new FrameworkPropertyMetadata(AdornerPopupPlacementMode.Bottom, FrameworkPropertyMetadataOptions.AffectsParentArrange | FrameworkPropertyMetadataOptions.AffectsRender));
        public AdornerPopupPlacementMode PlacementMode
        {
            get { return (AdornerPopupPlacementMode)GetValue(PlacementModeProperty); }
            set { SetValue(PlacementModeProperty, value); }
        }

        public static readonly DependencyProperty CenterOnPlacementTargetProperty = DependencyProperty.Register("CenterOnPlacementTarget", typeof(bool), typeof(AdornerPopup), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsParentArrange | FrameworkPropertyMetadataOptions.AffectsRender));
        public bool CenterOnPlacementTarget
        {
            get { return (bool)GetValue(CenterOnPlacementTargetProperty); }
            set { SetValue(CenterOnPlacementTargetProperty, value); }
        }

        public static readonly DependencyProperty UseDynamicPlacementProperty = DependencyProperty.Register("UseDynamicPlacement", typeof(bool), typeof(AdornerPopup), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsParentArrange | FrameworkPropertyMetadataOptions.AffectsRender));
        public bool UseDynamicPlacement
        {
            get { return (bool)GetValue(UseDynamicPlacementProperty); }
            set { SetValue(UseDynamicPlacementProperty, value); }
        }

        private static readonly DependencyPropertyKey ComputedPlacementModePropertyKey = DependencyProperty.RegisterReadOnly("ComputedPlacementMode", typeof(AdornerPopupPlacementMode), typeof(AdornerPopup), new PropertyMetadata(AdornerPopupPlacementMode.Relative));
        public AdornerPopupPlacementMode ComputedPlacementMode
        {
            get { return (AdornerPopupPlacementMode)GetValue(ComputedPlacementModePropertyKey.DependencyProperty); }
            private set { SetValue(ComputedPlacementModePropertyKey, value); }
        }

        public static readonly DependencyProperty KeepWithinViewProperty = DependencyProperty.Register("KeepWithinView", typeof(bool), typeof(AdornerPopup), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsParentArrange | FrameworkPropertyMetadataOptions.AffectsRender));
        public bool KeepWithinView
        {
            get { return (bool)GetValue(KeepWithinViewProperty); }
            set { SetValue(KeepWithinViewProperty, value); }
        }

        public static readonly DependencyProperty VerticalOffsetProperty = DependencyProperty.Register("VerticalOffset", typeof(double), typeof(AdornerPopup), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsParentArrange | FrameworkPropertyMetadataOptions.AffectsRender));
        public double VerticalOffset
        {
            get { return (double)GetValue(VerticalOffsetProperty); }
            set { SetValue(VerticalOffsetProperty, value); }
        }

        public static readonly DependencyProperty HorizontalOffsetProperty = DependencyProperty.Register("HorizontalOffset", typeof(double), typeof(AdornerPopup), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsParentArrange | FrameworkPropertyMetadataOptions.AffectsRender));
        public double HorizontalOffset
        {
            get { return (double)GetValue(HorizontalOffsetProperty); }
            set { SetValue(HorizontalOffsetProperty, value); }
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
                    ContentSize = new Size(ActualWidth, ActualHeight),
                    AdornedElementSize = adornedElementSize,
                    CenterOnPlacementTarget = CenterOnPlacementTarget
                };

                CalculateOffset(PlacementMode, out AdornerPopupPlacementMode computedPlacementMode, out double offsetX, out double offsetY);

                ComputedPlacementMode = computedPlacementMode;

                offsetX += HorizontalOffset;
                offsetY += VerticalOffset;

                if (KeepWithinView)
                {
                    offsetX = Math.Max(Math.Min(offsetX, _transformInfo.Constraint.Width - _transformInfo.ContentSize.Width), 0);
                    offsetY = Math.Max(Math.Min(offsetY, _transformInfo.Constraint.Height - _transformInfo.ContentSize.Height), 0);
                }

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

            if (offsetX + _transformInfo.ContentSize.Width > _transformInfo.Constraint.Width) return false;
            if (offsetY + _transformInfo.ContentSize.Height > _transformInfo.Constraint.Height) return false;

            return true;
        }

        protected void CalculatePlacementModeOffset(AdornerPopupPlacementMode placementMode, out double offsetX, out double offsetY)
        {
            offsetX = _transformInfo.Origin.X;
            offsetY = _transformInfo.Origin.Y;

            switch (placementMode)
            {
                case AdornerPopupPlacementMode.BottomLeft:
                    offsetX -= _transformInfo.ContentSize.Width;
                    offsetY += _transformInfo.AdornedElementSize.Height;
                    break;
                case AdornerPopupPlacementMode.Left:
                    offsetX -= _transformInfo.ContentSize.Width;

                    if (_transformInfo.CenterOnPlacementTarget)
                    {
                        offsetY += _transformInfo.AdornedElementSize.Height / 2 - _transformInfo.ContentSize.Height / 2;
                    }
                    break;
                case AdornerPopupPlacementMode.TopLeft:
                    offsetX -= _transformInfo.ContentSize.Width;
                    offsetY -= _transformInfo.ContentSize.Height;
                    break;
                case AdornerPopupPlacementMode.Top:
                    if (_transformInfo.CenterOnPlacementTarget)
                    {
                        offsetX += _transformInfo.AdornedElementSize.Width / 2 - _transformInfo.ContentSize.Width / 2;
                    }

                    offsetY -= _transformInfo.ContentSize.Height;
                    break;
                case AdornerPopupPlacementMode.TopRight:
                    offsetX += _transformInfo.AdornedElementSize.Width;
                    offsetY -= _transformInfo.ContentSize.Height;
                    break;
                case AdornerPopupPlacementMode.Right:
                    offsetX += _transformInfo.AdornedElementSize.Width;

                    if (_transformInfo.CenterOnPlacementTarget)
                    {
                        offsetY += _transformInfo.AdornedElementSize.Height / 2 - _transformInfo.ContentSize.Height / 2;
                    }
                    break;
                case AdornerPopupPlacementMode.BottomRight:
                    offsetX += _transformInfo.AdornedElementSize.Width;
                    offsetY += _transformInfo.AdornedElementSize.Height;
                    break;
                case AdornerPopupPlacementMode.Bottom:
                    if (_transformInfo.CenterOnPlacementTarget)
                    {
                        offsetX += _transformInfo.AdornedElementSize.Width / 2 - _transformInfo.ContentSize.Width / 2;
                    }

                    offsetY += _transformInfo.AdornedElementSize.Height;
                    break;
                case AdornerPopupPlacementMode.Relative:
                    if (_transformInfo.CenterOnPlacementTarget)
                    {
                        offsetX += _transformInfo.AdornedElementSize.Width / 2 - _transformInfo.ContentSize.Width / 2;
                        offsetY += _transformInfo.AdornedElementSize.Height / 2 - _transformInfo.ContentSize.Height / 2;
                    }
                    break;

            }
        }

        private AdornerPopupPlacementMode? _lastFittingPlacementMode = null;

        private bool GetAlternativePlacementMode(AdornerPopupPlacementMode placementMode, out AdornerPopupPlacementMode alternativePlacementMode, out double offsetX, out double offsetY)
        {
            offsetX = _transformInfo.Origin.X;
            offsetY = _transformInfo.Origin.Y;

            alternativePlacementMode = AdornerPopupPlacementMode.Relative;

            AdornerPopupPlacementMode[] tryOrder = new AdornerPopupPlacementMode[7];

            switch (placementMode)
            {
                case AdornerPopupPlacementMode.BottomLeft:
                    tryOrder[0] = AdornerPopupPlacementMode.Left;
                    tryOrder[1] = AdornerPopupPlacementMode.TopLeft;
                    tryOrder[2] = AdornerPopupPlacementMode.BottomRight;
                    tryOrder[3] = AdornerPopupPlacementMode.Right;
                    tryOrder[4] = AdornerPopupPlacementMode.TopRight;
                    tryOrder[5] = AdornerPopupPlacementMode.Bottom;
                    tryOrder[6] = AdornerPopupPlacementMode.Top;
                    break;
                case AdornerPopupPlacementMode.Left:
                    tryOrder[0] = AdornerPopupPlacementMode.BottomLeft;
                    tryOrder[1] = AdornerPopupPlacementMode.TopLeft;
                    tryOrder[2] = AdornerPopupPlacementMode.Right;
                    tryOrder[3] = AdornerPopupPlacementMode.BottomRight;
                    tryOrder[4] = AdornerPopupPlacementMode.TopRight;
                    tryOrder[5] = AdornerPopupPlacementMode.Bottom;
                    tryOrder[6] = AdornerPopupPlacementMode.Top;
                    break;
                case AdornerPopupPlacementMode.TopLeft:
                    tryOrder[0] = AdornerPopupPlacementMode.Left;
                    tryOrder[1] = AdornerPopupPlacementMode.BottomLeft;
                    tryOrder[2] = AdornerPopupPlacementMode.TopRight;
                    tryOrder[3] = AdornerPopupPlacementMode.Right;
                    tryOrder[4] = AdornerPopupPlacementMode.BottomRight;
                    tryOrder[5] = AdornerPopupPlacementMode.Bottom;
                    tryOrder[6] = AdornerPopupPlacementMode.Top;
                    break;
                case AdornerPopupPlacementMode.Top:
                    tryOrder[0] = AdornerPopupPlacementMode.TopRight;
                    tryOrder[1] = AdornerPopupPlacementMode.TopLeft;
                    tryOrder[2] = AdornerPopupPlacementMode.Bottom;
                    tryOrder[3] = AdornerPopupPlacementMode.BottomRight;
                    tryOrder[4] = AdornerPopupPlacementMode.BottomLeft;
                    tryOrder[5] = AdornerPopupPlacementMode.Right;
                    tryOrder[6] = AdornerPopupPlacementMode.Left;
                    break;
                case AdornerPopupPlacementMode.TopRight:
                    tryOrder[0] = AdornerPopupPlacementMode.Right;
                    tryOrder[1] = AdornerPopupPlacementMode.BottomRight;
                    tryOrder[2] = AdornerPopupPlacementMode.TopLeft;
                    tryOrder[3] = AdornerPopupPlacementMode.Left;
                    tryOrder[4] = AdornerPopupPlacementMode.BottomLeft;
                    tryOrder[5] = AdornerPopupPlacementMode.Bottom;
                    tryOrder[6] = AdornerPopupPlacementMode.Top;
                    break;
                case AdornerPopupPlacementMode.Right:
                    tryOrder[0] = AdornerPopupPlacementMode.BottomRight;
                    tryOrder[1] = AdornerPopupPlacementMode.TopRight;
                    tryOrder[2] = AdornerPopupPlacementMode.Left;
                    tryOrder[3] = AdornerPopupPlacementMode.BottomLeft;
                    tryOrder[4] = AdornerPopupPlacementMode.TopLeft;
                    tryOrder[5] = AdornerPopupPlacementMode.Bottom;
                    tryOrder[6] = AdornerPopupPlacementMode.Top;
                    break;
                case AdornerPopupPlacementMode.BottomRight:
                    tryOrder[0] = AdornerPopupPlacementMode.Right;
                    tryOrder[1] = AdornerPopupPlacementMode.TopRight;
                    tryOrder[2] = AdornerPopupPlacementMode.BottomLeft;
                    tryOrder[3] = AdornerPopupPlacementMode.Left;
                    tryOrder[4] = AdornerPopupPlacementMode.TopLeft;
                    tryOrder[5] = AdornerPopupPlacementMode.Bottom;
                    tryOrder[6] = AdornerPopupPlacementMode.Top;
                    break;
                case AdornerPopupPlacementMode.Bottom:
                    tryOrder[0] = AdornerPopupPlacementMode.BottomRight;
                    tryOrder[1] = AdornerPopupPlacementMode.BottomLeft;
                    tryOrder[2] = AdornerPopupPlacementMode.Top;
                    tryOrder[3] = AdornerPopupPlacementMode.TopRight;
                    tryOrder[4] = AdornerPopupPlacementMode.TopLeft;
                    tryOrder[5] = AdornerPopupPlacementMode.Right;
                    tryOrder[6] = AdornerPopupPlacementMode.Left;
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

            if (_lastFittingPlacementMode != null)
            {
                CalculatePlacementModeOffset(_lastFittingPlacementMode.Value, out offsetX, out offsetY);
                alternativePlacementMode = _lastFittingPlacementMode.Value;
            }

            return false;
        }

        protected virtual void CalculateOffset(AdornerPopupPlacementMode placementMode, out AdornerPopupPlacementMode computedPlacementMode, out double offsetX, out double offsetY)
        {
            computedPlacementMode = placementMode;

            CalculatePlacementModeOffset(placementMode, out offsetX, out offsetY);

            if (FitsInAdornerLayer(offsetX, offsetY))
            {
                _lastFittingPlacementMode = placementMode;
                return;
            }

            if (UseDynamicPlacement)
            {
                GetAlternativePlacementMode(placementMode, out computedPlacementMode, out offsetX, out offsetY);
            }
        }

        private struct TransformInfo
        {
            public Size Constraint;

            public Point Origin;

            public Size ContentSize;

            public Size AdornedElementSize;

            public bool CenterOnPlacementTarget;
        }
    }

    public enum AdornerPopupPlacementMode
    {
        BottomLeft,
        Left,
        TopLeft,
        Top,
        TopRight,
        Right,
        BottomRight,
        Bottom,
        Relative
    }
}