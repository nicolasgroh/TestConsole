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
    public class AdornerPopup : DecoratorAdorner
    {
        static AdornerPopup()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AdornerPopup), new FrameworkPropertyMetadata(typeof(AdornerPopup)));
        }

        public AdornerPopup(UIElement adornedElement) : base(adornedElement)
        {
            
        }

        #region DependancyProperties
        public static readonly DependencyProperty PlacementModeProperty = DependencyProperty.Register("PlacementMode", typeof(AdornerPopupPlacementMode), typeof(AdornerPopup), new FrameworkPropertyMetadata(AdornerPopupPlacementMode.Bottom, FrameworkPropertyMetadataOptions.AffectsParentArrange | FrameworkPropertyMetadataOptions.AffectsRender, PlacementModePropertyChanged));

        private static void PlacementModePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((AdornerPopup)d)._computationPlacementMode = (AdornerPopupPlacementMode)e.NewValue;
        }

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

        private AdornerLayer GetAdornerLayer()
        {
            return AdornerLayer.GetAdornerLayer(AdornedElement);
        }

        private bool TryCreateTransformInfo()
        {
            _transformInfo = default;
            _transformInfo.IsValid = false;

            var adornerLayer = GetAdornerLayer();

            var adornerLayerParent = VisualTreeHelper.GetParent(adornerLayer) as Visual;

            if (adornerLayerParent == null)
            {
                _offsetX = 0;
                _offsetY = 0;
                return _transformInfo.IsValid;
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
                CenterOnPlacementTarget = CenterOnPlacementTarget,
                IsValid = true
            };

            return _transformInfo.IsValid;
        }

        protected virtual Size GetPlacementModeSize(AdornerPopupPlacementMode placementMode)
        {
            return Child.DesiredSize;
        }

        private AdornerPopupPlacementMode _computationPlacementMode;
        private bool _hookedLoaded = false;
        private bool _invalidateArrange = false;

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            var baseArrangeSize = base.ArrangeOverride(arrangeSize);

            if (Child == null) return baseArrangeSize;

            if (!TryCreateTransformInfo()) return baseArrangeSize;

            double offsetX, offsetY, width, height;
            
            AdornerPopupPlacementMode computedPlacementMode;

            if (UseDynamicPlacement)
            {
                if (IsAdornedElementInsideAdornerLayer())
                {
                    //GetComputedPlacementMode(PlacementMode, out computedPlacementMode, out offsetX, out offsetY, out width, out height);

                    System.Diagnostics.Debug.WriteLine($"_computationPlacementMode: {_computationPlacementMode}");

                    CalculatePlacementModeOffset(_computationPlacementMode, out offsetX, out offsetY, out width, out height);

                    var fits = ContentFitsInAdornerLayer(offsetX, offsetY, width, height);

                    System.Diagnostics.Debug.WriteLine($"fits: {fits}");

                    if (!fits)
                    {
                        SetNextPlacementMode(PlacementMode);

                        ComputedPlacementMode = _computationPlacementMode;

                        InvalidateArrange();
                        return baseArrangeSize;
                    }

                    ComputedPlacementMode = _computationPlacementMode;
                    InvalidateArrange();

                    ApplyOffsets(ref offsetX, ref offsetY);
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

                    ComputedPlacementMode = computedPlacementMode;

                    CalculatePlacementModeOffset(computedPlacementMode, out offsetX, out offsetY, out width, out height);
                }
            }
            else
            {
                computedPlacementMode = PlacementMode;

                CalculatePlacementModeOffset(PlacementMode, out offsetX, out offsetY, out width, out height);

                ApplyOffsets(ref offsetX, ref offsetY);
            }

            ConstrainToViewIfNeeded(width, height, ref offsetX, ref offsetY);

            _offsetX = offsetX;
            _offsetY = offsetY;

            _appliedDesiredTransform = false;

            if (!_hookedLoaded)
            {
                _hookedLoaded = true;

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    System.Diagnostics.Debug.WriteLine($"loaded");

                    _computationPlacementMode = PlacementMode;
                    _hookedLoaded = false;

                    if (!_appliedDesiredTransform)
                    {
                        //var adornerLayer = GetAdornerLayer();

                        //adornerLayer.Update(AdornedElement);
                    }

                }), System.Windows.Threading.DispatcherPriority.Loaded);
            }

            return baseArrangeSize;
        }

        private double _offsetX = 0;
        private double _offsetY = 0;

        private bool _appliedDesiredTransform = false;

        public override GeneralTransform GetDesiredTransform(GeneralTransform generalTransform)
        {
            _appliedDesiredTransform = true;

            System.Diagnostics.Debug.WriteLine($"GetDesiredTransform");
            if (generalTransform is Transform transform)
            {
                var matrix = transform.Value;

                matrix.OffsetX = _offsetX;
                matrix.OffsetY = _offsetY;

                return new MatrixTransform(matrix);
            }

            return generalTransform;
        }

        private bool ContentFitsInAdornerLayer(double offsetX, double offsetY, double width, double height)
        {
            return FitsInAdornerLayer(new Point(offsetX, offsetY)) && FitsInAdornerLayer(new Point(offsetX + width, offsetY + height));
        }

        protected bool FitsInAdornerLayer(Point point)
        {
            if (point.X < 0) return false;
            if (point.Y < 0) return false;

            if (point.X > _transformInfo.Constraint.Width) return false;
            if (point.Y > _transformInfo.Constraint.Height) return false;

            return true;
        }

        protected void CalculatePlacementModeOffset(AdornerPopupPlacementMode placementMode, out double offsetX, out double offsetY, out double width, out double height)
        {
            offsetX = _transformInfo.AdornedElement.Location.X;
            offsetY = _transformInfo.AdornedElement.Location.Y;

            var contentSize = GetPlacementModeSize(placementMode);

            width = contentSize.Width;
            height = contentSize.Height;

            switch (placementMode)
            {
                case AdornerPopupPlacementMode.BottomLeft:
                    offsetX -= contentSize.Width;
                    offsetY += _transformInfo.AdornedElement.Height;
                    break;
                case AdornerPopupPlacementMode.Left:
                    offsetX -= contentSize.Width;

                    if (_transformInfo.CenterOnPlacementTarget)
                    {
                        offsetY += _transformInfo.AdornedElement.Height / 2 - contentSize.Height / 2;
                    }
                    break;
                case AdornerPopupPlacementMode.TopLeft:
                    offsetX -= contentSize.Width;
                    offsetY -= contentSize.Height;
                    break;
                case AdornerPopupPlacementMode.Top:
                    if (_transformInfo.CenterOnPlacementTarget)
                    {
                        offsetX += _transformInfo.AdornedElement.Width / 2 - contentSize.Width / 2;
                    }

                    offsetY -= contentSize.Height;
                    break;
                case AdornerPopupPlacementMode.TopRight:
                    offsetX += _transformInfo.AdornedElement.Width;
                    offsetY -= contentSize.Height;
                    break;
                case AdornerPopupPlacementMode.Right:
                    offsetX += _transformInfo.AdornedElement.Width;

                    if (_transformInfo.CenterOnPlacementTarget)
                    {
                        offsetY += _transformInfo.AdornedElement.Height / 2 - contentSize.Height / 2;
                    }
                    break;
                case AdornerPopupPlacementMode.BottomRight:
                    offsetX += _transformInfo.AdornedElement.Width;
                    offsetY += _transformInfo.AdornedElement.Height;
                    break;
                case AdornerPopupPlacementMode.Bottom:
                    if (_transformInfo.CenterOnPlacementTarget)
                    {
                        offsetX += _transformInfo.AdornedElement.Width / 2 - contentSize.Width / 2;
                    }

                    offsetY += _transformInfo.AdornedElement.Height;
                    break;
                case AdornerPopupPlacementMode.Relative:
                    if (_transformInfo.CenterOnPlacementTarget)
                    {
                        offsetX += _transformInfo.AdornedElement.Width / 2 - contentSize.Width / 2;
                        offsetY += _transformInfo.AdornedElement.Height / 2 - contentSize.Height / 2;
                    }
                    break;
            }
        }

        private void GetComputedPlacementMode(AdornerPopupPlacementMode placementMode, out AdornerPopupPlacementMode computedPlacementMode, out double offsetX, out double offsetY, out double width, out double height)
        {
            computedPlacementMode = AdornerPopupPlacementMode.Relative;

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

                CalculatePlacementModeOffset(placement, out offsetX, out offsetY, out width, out height);

                if (ContentFitsInAdornerLayer(offsetX, offsetY, width, height)) computedPlacementMode = placement;
            }

            CalculatePlacementModeOffset(computedPlacementMode, out offsetX, out offsetY, out width, out height);
        }

        private void SetNextPlacementMode(AdornerPopupPlacementMode placementMode)
        {
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

            var currentIndex = Array.IndexOf(tryOrder, _computationPlacementMode);

            _computationPlacementMode = tryOrder[currentIndex + 1];
        }

        private void ApplyOffsets(ref double offsetX, ref double offsetY)
        {
            offsetX += HorizontalOffset;
            offsetY += VerticalOffset;
        }

        private void ConstrainToViewIfNeeded(double width, double height, ref double offsetX, ref double offsetY)
        {
            if (KeepWithinView)
            {
                offsetX = Math.Max(Math.Min(offsetX, _transformInfo.Constraint.Width - width), 0);
                offsetY = Math.Max(Math.Min(offsetY, _transformInfo.Constraint.Height - height), 0);
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

            public bool CenterOnPlacementTarget;

            public bool IsValid;
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