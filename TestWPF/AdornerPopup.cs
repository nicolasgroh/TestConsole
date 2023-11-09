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
            LayoutUpdated += AdornerPopup_LayoutUpdated;
        }

        #region DependancyProperties
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
        #endregion

        private TransformInfo _transformInfo;

        private bool _firstAfterLayoutUpdated = true;
        private bool _lastFitted = false;
        private bool _fittedTwice = false;

        private readonly HashSet<AdornerPopupPlacementMode> _failedPlacementModes = new HashSet<AdornerPopupPlacementMode>(8);

        private AdornerLayer GetAdornerLayer()
        {
            return AdornerLayer.GetAdornerLayer(AdornedElement);
        }

        private void AdornerPopup_LayoutUpdated(object sender, EventArgs e)
        {
            _firstAfterLayoutUpdated = true;
            _lastFitted = false;
            _fittedTwice = false;

            _failedPlacementModes.Clear();
        }

        private bool TryCreateTransformInfo(Size contentSize)
        {
            _transformInfo = default;

            var adornerLayer = GetAdornerLayer();

            var adornerLayerParent = VisualTreeHelper.GetParent(adornerLayer) as Visual;

            if (adornerLayerParent == null)
            {
                _offsetX = 0;
                _offsetY = 0;
                return false;
            }

            var transform = AdornedElement.TransformToAncestor(adornerLayerParent);

            var origin = transform.Transform(default);

            Size adornedElementSize;

            if (AdornedElement is FrameworkElement frameworkElement) adornedElementSize = new Size(frameworkElement.ActualWidth, frameworkElement.ActualHeight);
            else adornedElementSize = AdornedElement.RenderSize;

            _transformInfo = new TransformInfo
            {
                Constraint = new Size(adornerLayer.ActualWidth, adornerLayer.ActualHeight),
                AdornedElement = new Rect(origin, adornedElementSize),
                ContentSize = contentSize,
                CenterOnPlacementTarget = CenterOnPlacementTarget
            };

            return true;
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            var baseArrangeSize = base.ArrangeOverride(arrangeSize);

            if (Child == null) return baseArrangeSize;

            if (!TryCreateTransformInfo(baseArrangeSize)) return baseArrangeSize;

            double offsetX, offsetY;

            AdornerPopupPlacementMode computedPlacementMode;

            if (UseDynamicPlacement)
            {
                if (IsAdornedElementInsideAdornerLayer())
                {
                    if (_firstAfterLayoutUpdated) ComputedPlacementMode = AdornerPopupPlacementMode.Relative;
                    else
                    {
                        if (!_lastFitted)
                        {
                            _failedPlacementModes.Add(ComputedPlacementMode);
                        }
                    }

                    CalculateDynamicOffset(PlacementMode, out computedPlacementMode, out offsetX, out offsetY);

                    ApplyOffsets(ref offsetX, ref offsetY);

                    if (!_fittedTwice)
                    {
                        _lastFitted = ContentFitsInAdornerLayer(offsetX, offsetY);

                        if (_lastFitted && computedPlacementMode == ComputedPlacementMode) _fittedTwice = true;
                        else Child.InvalidateMeasure();

                        var adornerLayer = GetAdornerLayer();

                        adornerLayer?.Update(AdornedElement);
                    }
                }
                else
                {
                    var relativePosition = GetAdornedElementPositionRelativeToAdornerLayer();

                    computedPlacementMode = AdornerPopupPlacementMode.Relative;

                    switch (relativePosition)
                    {
                        case RelativePosition.Left: computedPlacementMode = AdornerPopupPlacementMode.Right; break;
                        case RelativePosition.TopLeft: computedPlacementMode = AdornerPopupPlacementMode.BottomRight; break;
                        case RelativePosition.Top: computedPlacementMode = AdornerPopupPlacementMode.Bottom; break;
                        case RelativePosition.TopRight: computedPlacementMode = AdornerPopupPlacementMode.BottomLeft; break;
                        case RelativePosition.Right: computedPlacementMode = AdornerPopupPlacementMode.Left; break;
                        case RelativePosition.BottomRight: computedPlacementMode = AdornerPopupPlacementMode.TopLeft; break;
                        case RelativePosition.Bottom: computedPlacementMode = AdornerPopupPlacementMode.Top; break;
                        case RelativePosition.BottomLeft: computedPlacementMode = AdornerPopupPlacementMode.TopRight; break;
                    }

                    CalculatePlacementModeOffset(computedPlacementMode, out offsetX, out offsetY);
                }
            }
            else
            {
                computedPlacementMode = PlacementMode;

                CalculatePlacementModeOffset(PlacementMode, out offsetX, out offsetY);

                ApplyOffsets(ref offsetX, ref offsetY);
            }

            ConstrainToViewIfNeeded(ref offsetX, ref offsetY);

            _offsetX = offsetX;
            _offsetY = offsetY;

            ComputedPlacementMode = computedPlacementMode;

            _firstAfterLayoutUpdated = false;

            return baseArrangeSize;
        }

        private double _offsetX = 0;
        private double _offsetY = 0;

        public override GeneralTransform GetDesiredTransform(GeneralTransform generalTransform)
        {
            if (generalTransform is Transform transform)
            {
                var matrix = transform.Value;

                matrix.OffsetX = _offsetX;
                matrix.OffsetY = _offsetY;

                return new MatrixTransform(matrix);
            }

            return generalTransform;
        }

        private bool ContentFitsInAdornerLayer(double offsetX, double offsetY)
        {
            return FitsInAdornerLayer(new Point(offsetX, offsetY)) && FitsInAdornerLayer(new Point(offsetX + _transformInfo.ContentSize.Width, offsetY + _transformInfo.ContentSize.Height));
        }

        private bool FitsInAdornerLayer(Point point)
        {
            if (point.X < 0) return false;
            if (point.Y < 0) return false;

            if (point.X > _transformInfo.Constraint.Width) return false;
            if (point.Y > _transformInfo.Constraint.Height) return false;

            return true;
        }

        private void CalculatePlacementModeOffset(AdornerPopupPlacementMode placementMode, out double offsetX, out double offsetY)
        {
            offsetX = _transformInfo.AdornedElement.Location.X;
            offsetY = _transformInfo.AdornedElement.Location.Y;

            switch (placementMode)
            {
                case AdornerPopupPlacementMode.BottomLeft:
                    offsetX -= _transformInfo.ContentSize.Width;
                    offsetY += _transformInfo.AdornedElement.Height;
                    break;
                case AdornerPopupPlacementMode.Left:
                    offsetX -= _transformInfo.ContentSize.Width;

                    if (_transformInfo.CenterOnPlacementTarget)
                    {
                        offsetY += _transformInfo.AdornedElement.Height / 2 - _transformInfo.ContentSize.Height / 2;
                    }
                    break;
                case AdornerPopupPlacementMode.TopLeft:
                    offsetX -= _transformInfo.ContentSize.Width;
                    offsetY -= _transformInfo.ContentSize.Height;
                    break;
                case AdornerPopupPlacementMode.Top:
                    if (_transformInfo.CenterOnPlacementTarget)
                    {
                        offsetX += _transformInfo.AdornedElement.Width / 2 - _transformInfo.ContentSize.Width / 2;
                    }

                    offsetY -= _transformInfo.ContentSize.Height;
                    break;
                case AdornerPopupPlacementMode.TopRight:
                    offsetX += _transformInfo.AdornedElement.Width;
                    offsetY -= _transformInfo.ContentSize.Height;
                    break;
                case AdornerPopupPlacementMode.Right:
                    offsetX += _transformInfo.AdornedElement.Width;

                    if (_transformInfo.CenterOnPlacementTarget)
                    {
                        offsetY += _transformInfo.AdornedElement.Height / 2 - _transformInfo.ContentSize.Height / 2;
                    }
                    break;
                case AdornerPopupPlacementMode.BottomRight:
                    offsetX += _transformInfo.AdornedElement.Width;
                    offsetY += _transformInfo.AdornedElement.Height;
                    break;
                case AdornerPopupPlacementMode.Bottom:
                    if (_transformInfo.CenterOnPlacementTarget)
                    {
                        offsetX += _transformInfo.AdornedElement.Width / 2 - _transformInfo.ContentSize.Width / 2;
                    }

                    offsetY += _transformInfo.AdornedElement.Height;
                    break;
                case AdornerPopupPlacementMode.Relative:
                    if (_transformInfo.CenterOnPlacementTarget)
                    {
                        offsetX += _transformInfo.AdornedElement.Width / 2 - _transformInfo.ContentSize.Width / 2;
                        offsetY += _transformInfo.AdornedElement.Height / 2 - _transformInfo.ContentSize.Height / 2;
                    }
                    break;

            }
        }

        private bool GetNextPlacementMode(AdornerPopupPlacementMode placementMode, out AdornerPopupPlacementMode nextPlacementMode)
        {
            nextPlacementMode = AdornerPopupPlacementMode.Relative;

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

                if (!_failedPlacementModes.Contains(placement))
                {
                    nextPlacementMode = placement;

                    return true;
                }
            }

            return false;
        }

        private void CalculateDynamicOffset(AdornerPopupPlacementMode placementMode, out AdornerPopupPlacementMode nextPlacementMode, out double offsetX, out double offsetY)
        {
            if (_failedPlacementModes.Contains(placementMode))
            {
                GetNextPlacementMode(placementMode, out nextPlacementMode);

                CalculatePlacementModeOffset(nextPlacementMode, out offsetX, out offsetY);
            }
            else
            {
                nextPlacementMode = placementMode;

                CalculatePlacementModeOffset(placementMode, out offsetX, out offsetY);
            }
        }

        private void ApplyOffsets(ref double offsetX, ref double offsetY)
        {
            offsetX += HorizontalOffset;
            offsetY += VerticalOffset;
        }

        private void ConstrainToViewIfNeeded(ref double offsetX, ref double offsetY)
        {
            if (KeepWithinView)
            {
                offsetX = Math.Max(Math.Min(offsetX, _transformInfo.Constraint.Width - _transformInfo.ContentSize.Width), 0);
                offsetY = Math.Max(Math.Min(offsetY, _transformInfo.Constraint.Height - _transformInfo.ContentSize.Height), 0);
            }
        }

        private bool IsAdornedElementInsideAdornerLayer()
        {
            if (FitsInAdornerLayer(_transformInfo.AdornedElement.TopLeft)) return true;
            if (FitsInAdornerLayer(_transformInfo.AdornedElement.TopRight)) return true;
            if (FitsInAdornerLayer(_transformInfo.AdornedElement.BottomLeft)) return true;
            if (FitsInAdornerLayer(_transformInfo.AdornedElement.BottomRight)) return true;

            return false;
        }

        private RelativePosition GetAdornedElementPositionRelativeToAdornerLayer()
        {
            if (_transformInfo.AdornedElement.BottomRight.X < 0.0 && _transformInfo.AdornedElement.BottomRight.Y < 0.0) return RelativePosition.TopLeft;
            if (_transformInfo.AdornedElement.BottomLeft.X > _transformInfo.Constraint.Width && _transformInfo.AdornedElement.BottomLeft.Y < 0.0) return RelativePosition.TopRight;
            if (_transformInfo.AdornedElement.TopLeft.X > _transformInfo.Constraint.Width && _transformInfo.AdornedElement.TopLeft.Y > _transformInfo.Constraint.Height) return RelativePosition.BottomRight;
            if (_transformInfo.AdornedElement.TopRight.X < 0.0 && _transformInfo.AdornedElement.TopRight.Y > _transformInfo.Constraint.Height) return RelativePosition.BottomLeft;

            if (_transformInfo.AdornedElement.Right < 0.0) return RelativePosition.Left;
            if (_transformInfo.AdornedElement.Bottom < 0.0) return RelativePosition.Top;
            if (_transformInfo.AdornedElement.Left > _transformInfo.Constraint.Width) return RelativePosition.Right;
            if (_transformInfo.AdornedElement.Top > _transformInfo.Constraint.Height) return RelativePosition.Bottom;

            // Sould never happen
            return default;
        }

        private struct TransformInfo
        {
            public Size Constraint;

            public Rect AdornedElement;

            public Size ContentSize;

            public bool CenterOnPlacementTarget;
        }

        private enum RelativePosition
        {
            Left,
            Top,
            Right,
            Bottom,
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight
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