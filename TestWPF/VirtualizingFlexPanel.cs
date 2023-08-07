using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace TestWPF
{
    public class VirtualizingFlexPanel : VirtualizingPanel, IScrollInfo
    {
        public VirtualizingFlexPanel()
        {
            IsVisibleChanged += OnIsVisibleChanged;
        }

        private const double _scrollLineDelta = 16.0;

        public static readonly DependencyProperty MinItemWidthProperty = DependencyProperty.Register("MinItemWidth", typeof(double), typeof(VirtualizingFlexPanel), new PropertyMetadata(10.0));
        public double MinItemWidth
        {
            get { return (double)GetValue(MinItemWidthProperty); }
            set { SetValue(MinItemWidthProperty, value); }
        }

        protected override bool CanHierarchicallyScrollAndVirtualizeCore
        {
            get { return true; }
        }

        private bool _canHorizontallyScroll;
        public bool CanHorizontallyScroll
        {
            get { return _canHorizontallyScroll; }
            set { _canHorizontallyScroll = value; }
        }

        private bool _canVerticallyScroll;
        public bool CanVerticallyScroll
        {
            get { return _canVerticallyScroll; }
            set { _canVerticallyScroll = value; }
        }

        private double _extentHeight;
        public double ExtentHeight
        {
            get { return _extentHeight; }
        }

        private double _extentWidth;
        public double ExtentWidth
        {
            get { return _extentWidth; }
        }

        private double _horizontalCalculationOffset;

        private double _horizontalOffset;
        public double HorizontalOffset
        {
            get { return _horizontalOffset; }
        }

        private ScrollViewer _scrollOwner;
        public ScrollViewer ScrollOwner
        {
            get { return _scrollOwner; }
            set { _scrollOwner = value; }
        }

        private bool IsScrollingPanel
        {
            get { return ScrollOwner != null; }
        }

        private double _verticalCalculationOffset;

        private double _verticalOffset;
        public double VerticalOffset
        {
            get { return _verticalOffset; }
        }

        private double _viewPortHeight;
        public double ViewportHeight
        {
            get { return _viewPortHeight; }
        }

        private double _viewPortWidth;
        public double ViewportWidth
        {
            get { return _viewPortWidth; }
        }

        private static bool CanMouseWheelVerticallyScroll
        {
            get { return SystemParameters.WheelScrollLines > 0; }
        }

        private double? _unitformOrAverageContainerSize;
        private double? UnitformOrAverageContainerSize
        {
            get { return _unitformOrAverageContainerSize; }
            set { _unitformOrAverageContainerSize = value; }
        }

        private ItemsControl _itemsControl;

        private GroupItem _groupItem;

        private bool IsVirtualising
        {
            get
            {
                EnsureOwners();

                bool isVirstualizing = false;

                if (_itemsControl != null) isVirstualizing = GetIsVirtualizing(_itemsControl);

                if (!isVirstualizing) isVirstualizing = GetIsVirtualizing(this);

                return isVirstualizing;
            }
        }

        private VirtualizationCacheLength VirtualizationCacheLength
        {
            get
            {
                EnsureOwners();

                return GetCacheLength(_itemsControl);
            }
        }

        private VirtualizationCacheLengthUnit VirtualizationCacheLengthUnit
        {
            get
            {
                EnsureOwners();

                return GetCacheLengthUnit(_itemsControl);
            }
        }

        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue) InvalidateMeasure();
        }

        private void EnsureOwners()
        {
            if (IsItemsHost && _itemsControl == null) GetItemsOwnerInternal(out _itemsControl, out _groupItem);
            else
            {
                _itemsControl = null;
                _groupItem = null;
            }
        }

        protected override void OnIsItemsHostChanged(bool oldIsItemsHost, bool newIsItemsHost)
        {
            base.OnIsItemsHostChanged(oldIsItemsHost, newIsItemsHost);

            if (!newIsItemsHost)
            {
                _itemsControl = null;
                _groupItem = null;
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            EnsureOwners();

            if (_itemsControl == null || !IsItemsHost || !IsVirtualising) return MeasureNonItemsHost(availableSize);

            var cacheSize = VirtualizationCacheLength;
            var cacheUnit = VirtualizationCacheLengthUnit;

            Rect viewPort;

            var virtualizationInfoProvider = _groupItem as IHierarchicalVirtualizationAndScrollInfo;

            if (IsScrollingPanel)
            {
                viewPort = new Rect(_horizontalCalculationOffset, _verticalCalculationOffset, availableSize.Width, availableSize.Height);

                // Coerce Viewport to Extent
                if (viewPort.X > _extentWidth - _viewPortWidth) viewPort.X = _extentWidth - _viewPortWidth;
                if (viewPort.Y > _extentHeight - _viewPortHeight) viewPort.X = _extentHeight - _viewPortHeight;
            }
            else if (virtualizationInfoProvider != null)
            {
                var virtualizationConstraints = virtualizationInfoProvider.Constraints;

                viewPort = virtualizationConstraints.Viewport;
                cacheSize = virtualizationConstraints.CacheLength;
                cacheUnit = virtualizationConstraints.CacheLengthUnit;
            }

            NormalizeCacheLength(viewPort, ref cacheSize, ref cacheUnit);

            var generator = ItemContainerGenerator as ItemContainerGenerator;

            var items = generator.Items;

            if (items.Count == 0)
            {
                // Clear everything up

                return new Size(0, 0);
            }

            IContainItemStorage itemStorageProvider = _groupItem ?? _itemsControl as IContainItemStorage;

            var uniformOrAverageContainerSize = UnitformOrAverageContainerSize ?? 1.0;

            // For now. Assuming the Children are uniformly sized
            var firstItemInViewportIndex = (int)Math.Floor(viewPort.Y / uniformOrAverageContainerSize);
            var firstItemInViewportOffset = firstItemInViewportIndex * uniformOrAverageContainerSize;

            return new Size();
        }

        private Size MeasureNonItemsHost(Size availableSize)
        {
            var rowItemCount = (int)(availableSize.Width / MinItemWidth);

            if (rowItemCount == 0) rowItemCount = 1;

            var itemWidth = availableSize.Width / rowItemCount;

            var rowItemCounter = 0;
            var currentRowPosition = 0.0;
            UIElement[] currentRow = new UIElement[rowItemCount];

            for (int i = 0; i < InternalChildren.Count; i++)
            {
                var child = InternalChildren[i];

                child.Measure(new Size(itemWidth, availableSize.Height));

                currentRow[rowItemCounter] = child;

                rowItemCounter++;

                if (rowItemCounter == rowItemCount || i == InternalChildren.Count - 1)
                {
                    currentRowPosition += currentRow.Max(i => i?.DesiredSize.Height).GetValueOrDefault();

                    rowItemCounter = 0;

                    currentRow = new UIElement[rowItemCount];
                }
            }

            availableSize = new Size(double.IsInfinity(availableSize.Width) ? double.MaxValue : availableSize.Width, currentRowPosition);

            return availableSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var rowItemCount = (int)(finalSize.Width / MinItemWidth);

            if (rowItemCount == 0) rowItemCount = 1;

            System.Diagnostics.Debug.WriteLine(rowItemCount);

            var itemWidth = finalSize.Width / rowItemCount;

            var rowItemCounter = 0;
            var currentRowPosition = 0.0;
            UIElement[] currentRow = new UIElement[rowItemCount];

            for (int i = 0; i < InternalChildren.Count; i++)
            {
                var child = InternalChildren[i];

                currentRow[rowItemCounter] = child;

                rowItemCounter++;

                if (rowItemCounter == rowItemCount || i == InternalChildren.Count - 1)
                {
                    currentRowPosition += ArrangeRow(currentRow, itemWidth, currentRowPosition);

                    rowItemCounter = 0;

                    currentRow = new UIElement[rowItemCount];
                }
            }

            return new Size(finalSize.Width, currentRowPosition);
        }

        private double ArrangeRow(UIElement[] row, double itemWidth, double rowPosition)
        {
            var rowHeight = row.Max(i => i?.DesiredSize.Height).GetValueOrDefault();

            for (int i = 0; i < row.Length; i++)
            {
                var rowChild = row[i];

                if (rowChild == null) continue;

                rowChild.Arrange(new Rect(new Point(i * itemWidth, rowPosition), new Size(itemWidth, rowHeight)));
            }

            return rowHeight;
        }

        #region IScrollInfoImplementation
        public Rect MakeVisible(Visual visual, Rect rectangle)
        {
            throw new NotImplementedException();
        }

        public void LineDown()
        {
            SetInternalVerticalOffset(VerticalOffset + _scrollLineDelta);
        }

        public void LineLeft()
        {
            SetInternalHorizontalOffset(HorizontalOffset - _scrollLineDelta);
        }

        public void LineRight()
        {
            SetInternalHorizontalOffset(HorizontalOffset + _scrollLineDelta);
        }

        public void LineUp()
        {
            SetInternalVerticalOffset(VerticalOffset - _scrollLineDelta);
        }

        public void MouseWheelDown()
        {
            if (CanMouseWheelVerticallyScroll)
            {
                SetInternalVerticalOffset(VerticalOffset + SystemParameters.WheelScrollLines * _scrollLineDelta);
            }
            else PageDown();
        }

        public void MouseWheelLeft()
        {
            SetInternalHorizontalOffset(HorizontalOffset - 3.0 * _scrollLineDelta);
        }

        public void MouseWheelRight()
        {
            SetInternalHorizontalOffset(HorizontalOffset + 3.0 * _scrollLineDelta);
        }

        public void MouseWheelUp()
        {
            if (CanMouseWheelVerticallyScroll)
            {
                SetInternalVerticalOffset(VerticalOffset - SystemParameters.WheelScrollLines * _scrollLineDelta);
            }
            else PageUp();
        }

        public void PageDown()
        {
            SetInternalVerticalOffset(VerticalOffset + ViewportHeight);
        }

        public void PageLeft()
        {
            SetInternalHorizontalOffset(HorizontalOffset - ViewportWidth);
        }

        public void PageRight()
        {
            SetInternalHorizontalOffset(HorizontalOffset + ViewportWidth);
        }

        public void PageUp()
        {
            SetInternalVerticalOffset(VerticalOffset - ViewportHeight);
        }

        public void SetHorizontalOffset(double offset)
        {
            SetInternalHorizontalOffset(offset);
        }

        private void SetInternalHorizontalOffset(double offset)
        {
            _horizontalCalculationOffset = Math.Max(0, offset);

            InvalidateArrange();
        }

        public void SetVerticalOffset(double offset)
        {
            SetInternalVerticalOffset(offset);
        }

        private void SetInternalVerticalOffset(double offset)
        {
            _verticalCalculationOffset = Math.Max(0, offset);

            if (IsVirtualising) InvalidateMeasure();
            else InvalidateArrange();
        }
        #endregion

        private void GetItemsOwnerInternal(out ItemsControl itemsControl, out GroupItem groupItem)
        {
            itemsControl = null;
            groupItem = null;

            if (IsItemsHost)
            {
                // see if element was generated for an ItemsPresenter
                if (TemplatedParent is ItemsPresenter itemsPresenter)
                {
                    if (itemsPresenter.TemplatedParent is ItemsControl itemsPresenterOwner)
                    {
                        itemsControl = itemsPresenterOwner;
                    }
                    else if (itemsPresenter.TemplatedParent is GroupItem itemsPresenterGroupItem)
                    {
                        groupItem = itemsPresenterGroupItem;

                        // Rekursiv ItemsControl finden
                        itemsControl = TryFindItemsControl(itemsPresenter);
                    }
                }
                else
                {
                    // otherwise use element's templated parent
                    itemsControl = TemplatedParent as ItemsControl;
                    groupItem = TemplatedParent as GroupItem;
                }
            }
        }

        private ItemsControl TryFindItemsControl(ItemsPresenter itemsPresenter)
        {
            if (itemsPresenter == null) return null;

            if (itemsPresenter.TemplatedParent is ItemsControl itemsControl) return itemsControl;

            if (itemsPresenter.TemplatedParent is GroupItem groupItem)
            {
                // Get the grouping Panel
                var parent = VisualTreeHelper.GetParent(groupItem);

                if (parent == null) return null;

                // Get the grouping Panel's ItemsPresenter
                var groupingItemsPresenter = VisualTreeHelper.GetParent(parent) as ItemsPresenter;

                return TryFindItemsControl(groupingItemsPresenter);
            }

            return null;
        }

        private void NormalizeCacheLength(Rect viewport, ref VirtualizationCacheLength cacheLength, ref VirtualizationCacheLengthUnit cacheUnit)
        {
            if (cacheUnit == VirtualizationCacheLengthUnit.Page)
            {
                double factor = viewport.Height;

                if (double.IsPositiveInfinity(factor)) cacheLength = new VirtualizationCacheLength(0, 0);
                else cacheLength = new VirtualizationCacheLength(cacheLength.CacheBeforeViewport * factor, cacheLength.CacheAfterViewport * factor);

                cacheUnit = VirtualizationCacheLengthUnit.Pixel;
            }

            // if the viewport is empty in the scrolling direction, force the
            // cache to be empty also.   This avoids an infinite loop re- and
            // de-virtualizing the last item
            if (viewport.Height.AreClose(0.0)) cacheLength = new VirtualizationCacheLength(0, 0);
        }
    }
}