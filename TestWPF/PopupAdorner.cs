using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public class PopupAdorner : DecoratorAdorner
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
        public PopupAdorner(UIElement adornedElement) : base(adornedElement)
        {

        }
        #endregion

        #region DependancyProperties
        public static readonly DependencyProperty PlacementModeProperty = DependencyProperty.Register("PlacementMode", typeof(PopupAdornerPlacementMode), typeof(PopupAdorner), new FrameworkPropertyMetadata(PopupAdornerPlacementMode.Bottom, FrameworkPropertyMetadataOptions.AffectsParentArrange | FrameworkPropertyMetadataOptions.AffectsRender, PlacementModePropertyChanged));

        private static void PlacementModePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((PopupAdorner)d).ResetDynamicPlacement();
        }

        public PopupAdornerPlacementMode PlacementMode
        {
            get { return (PopupAdornerPlacementMode)GetValue(PlacementModeProperty); }
            set { SetValue(PlacementModeProperty, value); }
        }

        public static readonly DependencyProperty CenterOnPlacementTargetProperty = DependencyProperty.Register("CenterOnPlacementTarget", typeof(bool), typeof(PopupAdorner), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsParentArrange | FrameworkPropertyMetadataOptions.AffectsRender));
        public bool CenterOnPlacementTarget
        {
            get { return (bool)GetValue(CenterOnPlacementTargetProperty); }
            set { SetValue(CenterOnPlacementTargetProperty, value); }
        }

        public static readonly DependencyProperty UseDynamicPlacementProperty = DependencyProperty.Register("UseDynamicPlacement", typeof(bool), typeof(PopupAdorner), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsParentArrange | FrameworkPropertyMetadataOptions.AffectsRender));
        public bool UseDynamicPlacement
        {
            get { return (bool)GetValue(UseDynamicPlacementProperty); }
            set { SetValue(UseDynamicPlacementProperty, value); }
        }

        public static readonly DependencyProperty DynamicPlacementModeStrategyProperty = DependencyProperty.Register("DynamicPlacementModeStrategy", typeof(PopupAdornerDynamicPlacementModeStrategy), typeof(PopupAdorner), new FrameworkPropertyMetadata(PopupAdornerDynamicPlacementModeStrategy.Default, FrameworkPropertyMetadataOptions.AffectsParentArrange | FrameworkPropertyMetadataOptions.AffectsRender));
        public PopupAdornerDynamicPlacementModeStrategy DynamicPlacementModeStrategy
        {
            get { return (PopupAdornerDynamicPlacementModeStrategy)GetValue(DynamicPlacementModeStrategyProperty); }
            set { SetValue(DynamicPlacementModeStrategyProperty, value); }
        }

        private static readonly DependencyPropertyKey ComputedPlacementModePropertyKey = DependencyProperty.RegisterReadOnly("ComputedPlacementMode", typeof(PopupAdornerPlacementMode), typeof(PopupAdorner), new FrameworkPropertyMetadata(PopupAdornerPlacementMode.Relative, ComputedPlacementModePropertyChanged));

        public static readonly DependencyProperty ComputedPlacementModeProperty = ComputedPlacementModePropertyKey.DependencyProperty;

        private static void ComputedPlacementModePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((PopupAdorner)d).OnComputedPlacementModeChanged((PopupAdornerPlacementMode)e.OldValue, (PopupAdornerPlacementMode)e.NewValue);
        }

        public PopupAdornerPlacementMode ComputedPlacementMode
        {
            get { return (PopupAdornerPlacementMode)GetValue(ComputedPlacementModePropertyKey.DependencyProperty); }
            private set { SetValue(ComputedPlacementModePropertyKey, value); }
        }

        public static readonly DependencyProperty KeepWithinViewProperty = DependencyProperty.Register("KeepWithinView", typeof(bool), typeof(PopupAdorner), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsParentArrange | FrameworkPropertyMetadataOptions.AffectsRender));
        public bool KeepWithinView
        {
            get { return (bool)GetValue(KeepWithinViewProperty); }
            set { SetValue(KeepWithinViewProperty, value); }
        }

        public static readonly DependencyProperty VerticalOffsetProperty = DependencyProperty.Register("VerticalOffset", typeof(double), typeof(PopupAdorner), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsParentArrange | FrameworkPropertyMetadataOptions.AffectsRender));
        public double VerticalOffset
        {
            get { return (double)GetValue(VerticalOffsetProperty); }
            set { SetValue(VerticalOffsetProperty, value); }
        }

        public static readonly DependencyProperty HorizontalOffsetProperty = DependencyProperty.Register("HorizontalOffset", typeof(double), typeof(PopupAdorner), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsParentArrange | FrameworkPropertyMetadataOptions.AffectsRender));
        public double HorizontalOffset
        {
            get { return (double)GetValue(HorizontalOffsetProperty); }
            set { SetValue(HorizontalOffsetProperty, value); }
        }
        #endregion

        #region PrivateMember
        private LayoutInfo _layoutInfo;
        private PopupAdornerPlacementMode _computationPlacementMode;
        private bool _hookedDispatcherLoaded = false;
        private double _offsetX = 0;
        private double _offsetY = 0;
        #endregion

        #region Events
        public event GenericPropertyChangedEventHandler<PopupAdorner, PopupAdornerPlacementMode> ComputedPlacementModeChanged;
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

        private void CalculatePlacementModeOffset(PopupAdornerPlacementMode placementMode, out double offsetX, out double offsetY)
        {
            offsetX = _layoutInfo.AdornedElementRect.Location.X;
            offsetY = _layoutInfo.AdornedElementRect.Location.Y;

            switch (placementMode)
            {
                case PopupAdornerPlacementMode.BottomLeft:
                    offsetX -= _layoutInfo.ChildSize.Width;
                    offsetY += _layoutInfo.AdornedElementRect.Height;
                    break;
                case PopupAdornerPlacementMode.Left:
                    offsetX -= _layoutInfo.ChildSize.Width;

                    if (_layoutInfo.CenterOnPlacementTarget)
                    {
                        offsetY += _layoutInfo.AdornedElementRect.Height / 2 - _layoutInfo.ChildSize.Height / 2;
                    }
                    break;
                case PopupAdornerPlacementMode.TopLeft:
                    offsetX -= _layoutInfo.ChildSize.Width;
                    offsetY -= _layoutInfo.ChildSize.Height;
                    break;
                case PopupAdornerPlacementMode.Top:
                    if (_layoutInfo.CenterOnPlacementTarget)
                    {
                        offsetX += _layoutInfo.AdornedElementRect.Width / 2 - _layoutInfo.ChildSize.Width / 2;
                    }

                    offsetY -= _layoutInfo.ChildSize.Height;
                    break;
                case PopupAdornerPlacementMode.TopRight:
                    offsetX += _layoutInfo.AdornedElementRect.Width;
                    offsetY -= _layoutInfo.ChildSize.Height;
                    break;
                case PopupAdornerPlacementMode.Right:
                    offsetX += _layoutInfo.AdornedElementRect.Width;

                    if (_layoutInfo.CenterOnPlacementTarget)
                    {
                        offsetY += _layoutInfo.AdornedElementRect.Height / 2 - _layoutInfo.ChildSize.Height / 2;
                    }
                    break;
                case PopupAdornerPlacementMode.BottomRight:
                    offsetX += _layoutInfo.AdornedElementRect.Width;
                    offsetY += _layoutInfo.AdornedElementRect.Height;
                    break;
                case PopupAdornerPlacementMode.Bottom:
                    if (_layoutInfo.CenterOnPlacementTarget)
                    {
                        offsetX += _layoutInfo.AdornedElementRect.Width / 2 - _layoutInfo.ChildSize.Width / 2;
                    }

                    offsetY += _layoutInfo.AdornedElementRect.Height;
                    break;
                case PopupAdornerPlacementMode.Relative:
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

            var computedPlacementMode = PopupAdornerPlacementMode.Relative;

            switch (relativePosition)
            {
                case RelativePosition.Left: computedPlacementMode = PopupAdornerPlacementMode.Right; break;
                case RelativePosition.TopLeft: computedPlacementMode = PopupAdornerPlacementMode.BottomRight; break;
                case RelativePosition.Top: computedPlacementMode = PopupAdornerPlacementMode.Bottom; break;
                case RelativePosition.TopRight: computedPlacementMode = PopupAdornerPlacementMode.BottomLeft; break;
                case RelativePosition.Right: computedPlacementMode = PopupAdornerPlacementMode.Left; break;
                case RelativePosition.BottomRight: computedPlacementMode = PopupAdornerPlacementMode.TopLeft; break;
                case RelativePosition.Bottom: computedPlacementMode = PopupAdornerPlacementMode.Top; break;
                case RelativePosition.BottomLeft: computedPlacementMode = PopupAdornerPlacementMode.TopRight; break;
            }

            ComputedPlacementMode = computedPlacementMode;

            CalculatePlacementModeOffset(computedPlacementMode, out offsetX, out offsetY);

            ApplyUserOffsets(ref offsetX, ref offsetY);

            ConstrainToViewIfNeeded(ref offsetX, ref offsetY);
        }

        private bool SetNextComputationPlacementMode(PopupAdornerPlacementMode placementMode)
        {
            if (_computationPlacementMode == PopupAdornerPlacementMode.Relative) return false;

            var strategy = DynamicPlacementModeStrategy;

            PopupAdornerPlacementMode[] tryOrder;

            switch (placementMode)
            {
                case PopupAdornerPlacementMode.BottomLeft:
                    tryOrder = strategy.TargetBottomLeft;
                    break;
                case PopupAdornerPlacementMode.Left:
                    tryOrder = strategy.TargetLeft;
                    break;
                case PopupAdornerPlacementMode.TopLeft:
                    tryOrder = strategy.TargetTopLeft;
                    break;
                case PopupAdornerPlacementMode.Top:
                    tryOrder = strategy.TargetTop;
                    break;
                case PopupAdornerPlacementMode.TopRight:
                    tryOrder = strategy.TargetTopRight;
                    break;
                case PopupAdornerPlacementMode.Right:
                    tryOrder = strategy.TargetRight;
                    break;
                case PopupAdornerPlacementMode.BottomRight:
                    tryOrder = strategy.TargetBottomRight;
                    break;
                case PopupAdornerPlacementMode.Bottom:
                    tryOrder = strategy.TargetBottom;
                    break;
                default:
                    tryOrder = Array.Empty<PopupAdornerPlacementMode>();
                    break;
            }

            var currentIndex = Array.IndexOf(tryOrder, _computationPlacementMode);

            if (currentIndex + 1 >= tryOrder.Length)
            {
                _computationPlacementMode = PopupAdornerPlacementMode.Relative;
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

        private void OnComputedPlacementModeChanged(PopupAdornerPlacementMode oldValue, PopupAdornerPlacementMode newValue)
        {
            ComputedPlacementModeChanged?.Invoke(this, new GenericPropertyChangedEventArgs<PopupAdornerPlacementMode>(oldValue, newValue));
        }
        #endregion
    }
}