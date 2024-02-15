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
        #region PrivateObjects
        private struct LayoutInfo
        {
            public Size Constraint;

            public Size ChildSize;

            public Rect AdornedElementRect;

            public bool CenterOnPlacementTarget;

            public AdornerLayer AdornerLayer;
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
        #endregion

        #region Constructors
        static AdornerPopup()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AdornerPopup), new FrameworkPropertyMetadata(typeof(AdornerPopup)));
        }

        public AdornerPopup(UIElement adornedElement) : base(adornedElement)
        {

        }
        #endregion

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

        #region PrivateMember
        private LayoutInfo _layoutInfo;
        private AdornerPopupPlacementMode _computationPlacementMode;
        private bool _hookedDispatcherLoaded = false;
        private double _offsetX = 0;
        private double _offsetY = 0;
        #endregion

        #region Overrides
        protected override Size ArrangeOverride(Size arrangeSize)
        {
            var baseArrangeSize = base.ArrangeOverride(arrangeSize);

            if (Child == null) return baseArrangeSize;

            if (!TryCreateLayoutInfo()) return baseArrangeSize;

            double offsetX, offsetY;

            if (UseDynamicPlacement)
            {
                if (IsAdornedElementInsideAdornerLayer()) CalculateDynamicPlacementInsideAdornerLayer(out offsetX, out offsetY);
                else CalculateDynamicPlacementOutsideAdornerLayer(out offsetX, out offsetY);
            }
            else CalculateStaticPlacement(out offsetX, out offsetY);

            _offsetX = offsetX;
            _offsetY = offsetY;

            return baseArrangeSize;
        }

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
        #endregion

        #region GeneralPlacement
        private bool TryCreateLayoutInfo()
        {
            _layoutInfo = default;

            var adornedElement = AdornedElement;

            var adornerLayer = AdornerLayer.GetAdornerLayer(adornedElement);

            var adornerLayerParent = VisualTreeHelper.GetParent(adornerLayer) as Visual;

            if (adornerLayerParent == null)
            {
                _offsetX = 0;
                _offsetY = 0;
                return false;
            }

            var transform = adornedElement.TransformToAncestor(adornerLayerParent);

            var adornedElementPosition = transform.Transform(default);

            _layoutInfo = new LayoutInfo
            {
                AdornerLayer = adornerLayer,
                Constraint = adornerLayer.RenderSize,
                ChildSize = Child.DesiredSize,
                AdornedElementRect = new Rect(adornedElementPosition, adornedElement.RenderSize),
                CenterOnPlacementTarget = CenterOnPlacementTarget
            };

            return true;
        }

        private void CalculatePlacementModeOffset(AdornerPopupPlacementMode placementMode, out double offsetX, out double offsetY)
        {
            offsetX = _layoutInfo.AdornedElementRect.Location.X;
            offsetY = _layoutInfo.AdornedElementRect.Location.Y;

            switch (placementMode)
            {
                case AdornerPopupPlacementMode.BottomLeft:
                    offsetX -= _layoutInfo.ChildSize.Width;
                    offsetY += _layoutInfo.AdornedElementRect.Height;
                    break;
                case AdornerPopupPlacementMode.Left:
                    offsetX -= _layoutInfo.ChildSize.Width;

                    if (_layoutInfo.CenterOnPlacementTarget)
                    {
                        offsetY += _layoutInfo.AdornedElementRect.Height / 2 - _layoutInfo.ChildSize.Height / 2;
                    }
                    break;
                case AdornerPopupPlacementMode.TopLeft:
                    offsetX -= _layoutInfo.ChildSize.Width;
                    offsetY -= _layoutInfo.ChildSize.Height;
                    break;
                case AdornerPopupPlacementMode.Top:
                    if (_layoutInfo.CenterOnPlacementTarget)
                    {
                        offsetX += _layoutInfo.AdornedElementRect.Width / 2 - _layoutInfo.ChildSize.Width / 2;
                    }

                    offsetY -= _layoutInfo.ChildSize.Height;
                    break;
                case AdornerPopupPlacementMode.TopRight:
                    offsetX += _layoutInfo.AdornedElementRect.Width;
                    offsetY -= _layoutInfo.ChildSize.Height;
                    break;
                case AdornerPopupPlacementMode.Right:
                    offsetX += _layoutInfo.AdornedElementRect.Width;

                    if (_layoutInfo.CenterOnPlacementTarget)
                    {
                        offsetY += _layoutInfo.AdornedElementRect.Height / 2 - _layoutInfo.ChildSize.Height / 2;
                    }
                    break;
                case AdornerPopupPlacementMode.BottomRight:
                    offsetX += _layoutInfo.AdornedElementRect.Width;
                    offsetY += _layoutInfo.AdornedElementRect.Height;
                    break;
                case AdornerPopupPlacementMode.Bottom:
                    if (_layoutInfo.CenterOnPlacementTarget)
                    {
                        offsetX += _layoutInfo.AdornedElementRect.Width / 2 - _layoutInfo.ChildSize.Width / 2;
                    }

                    offsetY += _layoutInfo.AdornedElementRect.Height;
                    break;
                case AdornerPopupPlacementMode.Relative:
                    if (_layoutInfo.CenterOnPlacementTarget)
                    {
                        offsetX += _layoutInfo.AdornedElementRect.Width / 2 - _layoutInfo.ChildSize.Width / 2;
                        offsetY += _layoutInfo.AdornedElementRect.Height / 2 - _layoutInfo.ChildSize.Height / 2;
                    }
                    break;
            }
        }

        private void ApplyUserOffsets(ref double offsetX, ref double offsetY)
        {
            offsetX += HorizontalOffset;
            offsetY += VerticalOffset;
        }

        private void ConstrainToViewIfNeeded(ref double offsetX, ref double offsetY)
        {
            if (KeepWithinView)
            {
                offsetX = Math.Max(Math.Min(offsetX, _layoutInfo.Constraint.Width - _layoutInfo.ChildSize.Width), 0);
                offsetY = Math.Max(Math.Min(offsetY, _layoutInfo.Constraint.Height - _layoutInfo.ChildSize.Height), 0);
            }
        }

        private void ResetDynamicPlacement()
        {
            _computationPlacementMode = PlacementMode;
        }
        #endregion

        #region StaticPlacement
        private void CalculateStaticPlacement(out double offsetX, out double offsetY)
        {
            ResetDynamicPlacement();

            ComputedPlacementMode = PlacementMode;

            CalculatePlacementModeOffset(PlacementMode, out offsetX, out offsetY);

            ApplyUserOffsets(ref offsetX, ref offsetY);

            ConstrainToViewIfNeeded(ref offsetX, ref offsetY);
        }
        #endregion

        #region DynamicPlacement
        private bool IsAdornedElementInsideAdornerLayer()
        {
            if (FitsInAdornerLayer(_layoutInfo.AdornedElementRect.TopLeft)) return true;
            if (FitsInAdornerLayer(_layoutInfo.AdornedElementRect.TopRight)) return true;
            if (FitsInAdornerLayer(_layoutInfo.AdornedElementRect.BottomLeft)) return true;
            if (FitsInAdornerLayer(_layoutInfo.AdornedElementRect.BottomRight)) return true;

            return false;
        }

        private void CalculateDynamicPlacementInsideAdornerLayer(out double offsetX, out double offsetY)
        {
            CalculatePlacementModeOffset(_computationPlacementMode, out offsetX, out offsetY);

            if (!WouldContentFitInAdornerLayer(offsetX, offsetY))
            {
                var shouldUpdateLayout = SetNextComputationPlacementMode(PlacementMode);

                ComputedPlacementMode = _computationPlacementMode;

                if (shouldUpdateLayout)
                {
                    _layoutInfo.AdornerLayer.Update(AdornedElement);
                }
            }

            ComputedPlacementMode = _computationPlacementMode;

            HookDispatcherLoaded();
        }

        private void CalculateDynamicPlacementOutsideAdornerLayer(out double offsetX, out double offsetY)
        {
            ResetDynamicPlacement();

            var relativePosition = GetAdornedElementPositionRelativeToAdornerLayer();

            var computedPlacementMode = AdornerPopupPlacementMode.Relative;

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

            CalculatePlacementModeOffset(computedPlacementMode, out offsetX, out offsetY);

            ApplyUserOffsets(ref offsetX, ref offsetY);

            ConstrainToViewIfNeeded(ref offsetX, ref offsetY);
        }

        private bool SetNextComputationPlacementMode(AdornerPopupPlacementMode placementMode)
        {
            if (_computationPlacementMode == AdornerPopupPlacementMode.Relative) return false;

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

            if (currentIndex + 1 >= tryOrder.Length)
            {
                _computationPlacementMode = AdornerPopupPlacementMode.Relative;
                return false;
            }

            _computationPlacementMode = tryOrder[currentIndex + 1];

            return true;
        }

        private bool FitsInAdornerLayer(Point point)
        {
            if (point.X < 0) return false;
            if (point.Y < 0) return false;

            if (point.X > _layoutInfo.Constraint.Width) return false;
            if (point.Y > _layoutInfo.Constraint.Height) return false;

            return true;
        }

        private bool WouldContentFitInAdornerLayer(double offsetX, double offsetY)
        {
            return FitsInAdornerLayer(new Point(offsetX, offsetY)) && FitsInAdornerLayer(new Point(offsetX + _layoutInfo.ChildSize.Width, offsetY + _layoutInfo.ChildSize.Height));
        }

        private void HookDispatcherLoaded()
        {
            if (!_hookedDispatcherLoaded)
            {
                _hookedDispatcherLoaded = true;

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    _hookedDispatcherLoaded = false;

                    ResetDynamicPlacement();

                }), System.Windows.Threading.DispatcherPriority.Loaded);
            }
        }

        private RelativePosition GetAdornedElementPositionRelativeToAdornerLayer()
        {
            if (_layoutInfo.AdornedElementRect.BottomRight.X < 0.0 && _layoutInfo.AdornedElementRect.BottomRight.Y < 0.0) return RelativePosition.TopLeft;
            if (_layoutInfo.AdornedElementRect.BottomLeft.X > _layoutInfo.Constraint.Width && _layoutInfo.AdornedElementRect.BottomLeft.Y < 0.0) return RelativePosition.TopRight;
            if (_layoutInfo.AdornedElementRect.TopLeft.X > _layoutInfo.Constraint.Width && _layoutInfo.AdornedElementRect.TopLeft.Y > _layoutInfo.Constraint.Height) return RelativePosition.BottomRight;
            if (_layoutInfo.AdornedElementRect.TopRight.X < 0.0 && _layoutInfo.AdornedElementRect.TopRight.Y > _layoutInfo.Constraint.Height) return RelativePosition.BottomLeft;

            if (_layoutInfo.AdornedElementRect.Right < 0.0) return RelativePosition.Left;
            if (_layoutInfo.AdornedElementRect.Bottom < 0.0) return RelativePosition.Top;
            if (_layoutInfo.AdornedElementRect.Left > _layoutInfo.Constraint.Width) return RelativePosition.Right;
            if (_layoutInfo.AdornedElementRect.Top > _layoutInfo.Constraint.Height) return RelativePosition.Bottom;

            // Sould never happen
            return default;
        }
        #endregion
    }
}