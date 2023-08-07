using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace TestWPF
{
    //
    // Zusammenfassung:
    //     Positions child elements in sequential position from left to right, breaking
    //     content to the next line at the edge of the containing box. Subsequent ordering
    //     happens sequentially from top to bottom or from right to left, depending on the
    //     value of the Orientation property.
    public class VirtualizingWrapPanel : VirtualizingPanel, IScrollInfo
    {
        private struct ScrollData
        {
            public Point Offset;

            public Size Viewport;

            public Size Extent;
        }

        private struct Position
        {
            public readonly int Row;

            public readonly int Column;

            public Position(int row, int column)
            {
                Row = row;
                Column = column;
            }
        }

        private class LayoutData
        {
            public int FirstVisibleIndex { get; set; }

            public int LastVisibleIndex { get; set; }

            public Size AvailableSize { get; set; }
        }

        private List<UIElement> generatedContainers = new List<UIElement>();

        private Dictionary<GeneratorPosition, UIElement> generatedContainersByPosition = new Dictionary<GeneratorPosition, UIElement>();

        private HashSet<UIElement> recycledElements = new HashSet<UIElement>();

        //
        // Zusammenfassung:
        //     Identifies the ItemHeight dependency property.
        public static readonly DependencyProperty ItemHeightProperty = DependencyProperty.Register("ItemHeight", typeof(double), typeof(VirtualizingWrapPanel), new PropertyMetadata(100.0, OnAppearancePropertyChanged));

        //
        // Zusammenfassung:
        //     Identifies the Orientation dependency property.
        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register("Orientation", typeof(Orientation), typeof(VirtualizingWrapPanel), new PropertyMetadata(Orientation.Horizontal, OnAppearancePropertyChanged));

        //
        // Zusammenfassung:
        //     Identifies the ItemWidth dependency property.
        public static readonly DependencyProperty ItemWidthProperty = DependencyProperty.Register("ItemWidth", typeof(double), typeof(VirtualizingWrapPanel), new PropertyMetadata(100.0, OnAppearancePropertyChanged));

        //
        // Zusammenfassung:
        //     Identifies the ScrollStep dependency property.
        public static readonly DependencyProperty ScrollStepProperty = DependencyProperty.Register("ScrollStep", typeof(double), typeof(VirtualizingWrapPanel), new PropertyMetadata(10.0, OnAppearancePropertyChanged));

        private bool canHorizontallyScroll;

        private bool canVerticallyScroll;

        private ScrollData scrollData;

        private Size itemSize;

        private int focusedIndex = -1;

        private int bringIntoViewIndex = -1;

        private LayoutData layoutData;

        //
        // Zusammenfassung:
        //     Gets or sets a value that specifies the height of all items that are contained
        //     within a VirtualizingWrapPanel. This is a dependency property.
        public double ItemHeight
        {
            get
            {
                return (double)GetValue(ItemHeightProperty);
            }
            set
            {
                SetValue(ItemHeightProperty, value);
            }
        }

        //
        // Zusammenfassung:
        //     Gets or sets a value that specifies the width of all items that are contained
        //     within a VirtualizingWrapPanel. This is a dependency property.
        public double ItemWidth
        {
            get
            {
                return (double)GetValue(ItemWidthProperty);
            }
            set
            {
                SetValue(ItemWidthProperty, value);
            }
        }

        //
        // Zusammenfassung:
        //     Gets or sets a value that specifies the dimension in which child content is arranged.
        //     This is a dependency property.
        public Orientation Orientation
        {
            get
            {
                return (Orientation)GetValue(OrientationProperty);
            }
            set
            {
                SetValue(OrientationProperty, value);
            }
        }

        //
        // Zusammenfassung:
        //     Gets or sets a value that indicates whether scrolling on the horizontal axis
        //     is possible.
        public bool CanHorizontallyScroll
        {
            get
            {
                return canHorizontallyScroll;
            }
            set
            {
                if (canHorizontallyScroll != value)
                {
                    canHorizontallyScroll = value;
                    InvalidateMeasure();
                }
            }
        }

        //
        // Zusammenfassung:
        //     Gets or sets a value that indicates whether scrolling on the vertical axis is
        //     possible.
        public bool CanVerticallyScroll
        {
            get
            {
                return canVerticallyScroll;
            }
            set
            {
                if (canVerticallyScroll != value)
                {
                    canVerticallyScroll = value;
                    InvalidateMeasure();
                }
            }
        }

        //
        // Zusammenfassung:
        //     Gets or sets a ScrollViewer element that controls scrolling behavior.
        public ScrollViewer ScrollOwner { get; set; }

        //
        // Zusammenfassung:
        //     Gets the horizontal offset of the scrolled content.
        public double HorizontalOffset => scrollData.Offset.X;

        //
        // Zusammenfassung:
        //     Gets the vertical offset of the scrolled content.
        public double VerticalOffset => scrollData.Offset.Y;

        //
        // Zusammenfassung:
        //     Gets the horizontal size of the viewport for this content.
        public double ViewportWidth => scrollData.Viewport.Width;

        //
        // Zusammenfassung:
        //     Gets the vertical size of the viewport for this content.
        public double ViewportHeight => scrollData.Viewport.Height;

        //
        // Zusammenfassung:
        //     Gets the horizontal size of the extent.
        public double ExtentWidth => scrollData.Extent.Width;

        //
        // Zusammenfassung:
        //     Gets the vertical size of the extent.
        public double ExtentHeight => scrollData.Extent.Height;

        //
        // Zusammenfassung:
        //     Gets or sets a value for mouse wheel scroll step.
        public double ScrollStep
        {
            get
            {
                return (double)GetValue(ScrollStepProperty);
            }
            set
            {
                SetValue(ScrollStepProperty, value);
            }
        }

        private ItemContainerGenerator Generator
        {
            get
            {
                if (base.ItemContainerGenerator == null)
                {
                    return null;
                }

                return base.ItemContainerGenerator.GetItemContainerGeneratorForPanel(this);
            }
        }

        private int ItemCount
        {
            get
            {
                UIElementCollection internalChildren = base.InternalChildren;
                if (internalChildren == null)
                {
                    return 0;
                }

                if (Generator == null)
                {
                    return 0;
                }

                return Generator.Items.Count;
            }
        }

        private double VerticalStep
        {
            get
            {
                if (!IsPixelBased)
                {
                    return 1.0;
                }

                return ScrollStep;
            }
        }

        private double HorizontalStep
        {
            get
            {
                if (!IsPixelBased)
                {
                    return 1.0;
                }

                return ScrollStep;
            }
        }

        private bool IsPixelBased
        {
            get
            {
                DependencyObject element = (DependencyObject)(((object)ItemsControl.GetItemsOwner(this)) ?? ((object)this));
                return VirtualizingPanel.GetScrollUnit(element) == ScrollUnit.Pixel;
            }
        }

        protected override bool CanHierarchicallyScrollAndVirtualizeCore => true;

        private bool IsRecycling
        {
            get
            {
                ItemsControl itemsOwner = ItemsControl.GetItemsOwner(this);
                return GetVirtualizationMode(itemsOwner) == VirtualizationMode.Recycling;
            }
        }

        //
        // Zusammenfassung:
        //     Initializes a new instance of the Telerik.Windows.Controls.VirtualizingWrapPanel
        //     class.
        public VirtualizingWrapPanel()
        {
            itemSize = new Size(ItemWidth, ItemHeight);
            layoutData = new LayoutData();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            ItemsControl itemsOwner = ItemsControl.GetItemsOwner(this);
            if (itemsOwner != null && itemsOwner.IsGrouping && !IsPixelBased)
            {
                throw new NotSupportedException("Grouping is not supported with VirtualizingPanel.ScrollUnit set to Item. You can set the VirtualizingPanel.ScrollUnit property to Pixel instead.");
            }
        }

        //
        // Zusammenfassung:
        //     Scrolls down within content by one logical unit.
        public void LineDown()
        {
            SetVerticalOffset(VerticalOffset + VerticalStep);
        }

        //
        // Zusammenfassung:
        //     Scrolls left within content by one logical unit.
        public void LineLeft()
        {
            SetHorizontalOffset(HorizontalOffset - HorizontalStep);
        }

        //
        // Zusammenfassung:
        //     Scrolls right within content by one logical unit.
        public void LineRight()
        {
            SetHorizontalOffset(HorizontalOffset + HorizontalStep);
        }

        //
        // Zusammenfassung:
        //     Scrolls up within content by one logical unit.
        public void LineUp()
        {
            SetVerticalOffset(VerticalOffset - VerticalStep);
        }

        //
        // Zusammenfassung:
        //     Forces content to scroll until the coordinate space of a Visual object is visible.
        public Rect MakeVisible(Visual visual, Rect rectangle)
        {
            MakeVisible(visual as UIElement);
            return rectangle;
        }

        //
        // Zusammenfassung:
        //     Scrolls down within content after a user clicks the wheel button on a mouse.
        public void MouseWheelDown()
        {
            SetVerticalOffset(VerticalOffset + ScrollStep);
        }

        //
        // Zusammenfassung:
        //     Scrolls left within content after a user clicks the wheel button on a mouse.
        public void MouseWheelLeft()
        {
            SetHorizontalOffset(HorizontalOffset - ScrollStep);
        }

        //
        // Zusammenfassung:
        //     Scrolls right within content after a user clicks the wheel button on a mouse.
        public void MouseWheelRight()
        {
            SetHorizontalOffset(HorizontalOffset + ScrollStep);
        }

        //
        // Zusammenfassung:
        //     Scrolls up within content after a user clicks the wheel button on a mouse.
        public void MouseWheelUp()
        {
            SetVerticalOffset(VerticalOffset - ScrollStep);
        }

        //
        // Zusammenfassung:
        //     Scrolls up within content by one page.
        public void PageUp()
        {
            double pageOffset = GetPageOffset();
            SetVerticalOffset(VerticalOffset - pageOffset);
        }

        //
        // Zusammenfassung:
        //     Scrolls down within content by one page.
        public void PageDown()
        {
            double pageOffset = GetPageOffset();
            SetVerticalOffset(VerticalOffset + pageOffset);
        }

        //
        // Zusammenfassung:
        //     Scrolls left within content by one page.
        public void PageLeft()
        {
            double pageOffset = GetPageOffset();
            SetHorizontalOffset(HorizontalOffset - pageOffset);
        }

        //
        // Zusammenfassung:
        //     Scrolls right within content by one page.
        public void PageRight()
        {
            double pageOffset = GetPageOffset();
            SetHorizontalOffset(HorizontalOffset + pageOffset);
        }

        //
        // Zusammenfassung:
        //     Sets the amount of vertical offset.
        public void SetVerticalOffset(double offset)
        {
            double val = Math.Min(offset, ExtentHeight - ViewportHeight);
            val = Math.Max(0.0, val);
            if (!IsPixelBased)
            {
                val = Math.Round(val);
            }

            if (scrollData.Offset.Y != val)
            {
                scrollData.Offset.Y = val;
                InvalidateScrollInfo();
                InvalidateMeasure();
            }
        }

        //
        // Zusammenfassung:
        //     Sets the amount of horizontal offset.
        public void SetHorizontalOffset(double offset)
        {
            double val = Math.Min(offset, ExtentWidth - ViewportWidth);
            val = Math.Max(0.0, val);
            if (!IsPixelBased)
            {
                val = Math.Round(val);
            }

            if (scrollData.Offset.X != val)
            {
                scrollData.Offset.X = val;
                InvalidateScrollInfo();
                InvalidateMeasure();
            }
        }

        //
        // Zusammenfassung:
        //     Note: Works only for vertical.
        internal void PageLast()
        {
            SetVerticalOffset(double.PositiveInfinity);
        }

        //
        // Zusammenfassung:
        //     Note: Works only for vertical.
        internal void PageFirst()
        {
            SetVerticalOffset(0.0);
        }

        //
        // Zusammenfassung:
        //     Generates the item at the specified index location and makes it visible.
        //
        // Parameter:
        //   index:
        //     The index position of the item that is generated and made visible.
        protected override void BringIndexIntoView(int index)
        {
            if (index < ItemCount)
            {
                GetRowsAndColumns(layoutData.AvailableSize, out var _, out var itemsInDirection);
                Position position = GetPosition(index, itemsInDirection);
                BringIndexIntoView(index, position);
            }
        }

        //
        // Zusammenfassung:
        //     When items are removed, remove the corresponding UI if necessary.
        //
        // Parameter:
        //   sender:
        //
        //   args:
        protected override void OnItemsChanged(object sender, ItemsChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                {
                    RemoveChildRange(args.Position, args.ItemUICount);
                    int num = base.ItemContainerGenerator.IndexFromGeneratorPosition(args.Position);
                    if (num <= layoutData.FirstVisibleIndex)
                    {
                        layoutData.FirstVisibleIndex -= args.ItemCount;
                    }

                    if (num <= layoutData.LastVisibleIndex)
                    {
                        layoutData.LastVisibleIndex -= args.ItemCount;
                    }

                    break;
                }
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Move:
                    if (args.ItemUICount > 0 && args.Position.Index >= 0 && args.Position.Index < base.InternalChildren.Count)
                    {
                        RemoveInternalChildRange(args.Position.Index, args.ItemUICount);
                    }

                    break;
                case NotifyCollectionChangedAction.Reset:
                    SetVerticalOffset(0.0);
                    SetHorizontalOffset(0.0);
                    generatedContainers.Clear();
                    generatedContainersByPosition.Clear();
                    recycledElements.Clear();
                    layoutData.FirstVisibleIndex = 0;
                    layoutData.LastVisibleIndex = 0;
                    break;
            }
        }

        //
        // Zusammenfassung:
        //     Measure the children.
        //
        // Parameter:
        //   availableSize:
        //     The available size.
        //
        // Rückgabewerte:
        //     The desired size.
        protected override Size MeasureOverride(Size availableSize)
        {
            ScrollData scrollData = this.scrollData;
            IHierarchicalVirtualizationAndScrollInfo hierarchicalVirtualizationAndScrollInfo = null;
            bool flag = double.IsInfinity(availableSize.Width);
            bool flag2 = double.IsInfinity(availableSize.Height);
            bool isPixelBased = IsPixelBased;
            Size size = default(Size);
            Size size2 = default(Size);
            Point point = default(Point);
            if (ScrollOwner == null)
            {
                GroupItem groupItem = this.ParentOfType<GroupItem>();
                hierarchicalVirtualizationAndScrollInfo = GetHierarchyScrollInfo(groupItem);
                if (hierarchicalVirtualizationAndScrollInfo != null)
                {
                    Rect viewport = hierarchicalVirtualizationAndScrollInfo.Constraints.Viewport;
                    HierarchicalVirtualizationHeaderDesiredSizes headerDesiredSizes = hierarchicalVirtualizationAndScrollInfo.HeaderDesiredSizes;
                    Size logicalSize = headerDesiredSizes.LogicalSize;
                    size2 = headerDesiredSizes.PixelSize;
                    size = (isPixelBased ? size2 : logicalSize);
                    Point offset = new Point(Math.Max(0.0, viewport.Location.X), Math.Max(0.0, viewport.Location.Y));
                    point = GetOffset(groupItem);
                    Size size3 = viewport.Size;
                    if (flag)
                    {
                        availableSize.Width = size3.Width;
                    }

                    if (flag2)
                    {
                        availableSize.Height = size3.Height;
                    }

                    double val = availableSize.Height - Math.Max(0.0, size.Height - offset.Y);
                    availableSize.Height = Math.Max(0.0, val);
                    if (point.X > offset.X)
                    {
                        availableSize.Width = Math.Max(0.0, availableSize.Width - (point.X - offset.X));
                    }

                    this.scrollData = new ScrollData
                    {
                        Offset = offset,
                        Viewport = availableSize,
                        Extent = scrollData.Extent
                    };
                }
            }

            GetRowsAndColumns(availableSize, out var rowsOrColumns, out var itemsInDirection);
            bool flag3 = ItemsControl.GetItemsOwner(this)?.IsGrouping ?? false;
            Size size4;
            Size size5;

            size4 = new Size((double)rowsOrColumns * itemSize.Width, (double)itemsInDirection * itemSize.Height);
            if (ScrollOwner != null && !flag3 && !double.IsInfinity(availableSize.Width) && availableSize.Width % itemSize.Width != 0.0)
            {
                rowsOrColumns++;
            }

            size5 = new Size(rowsOrColumns, itemsInDirection);

            double num = Math.Max(1.0, itemSize.Width);
            double num2 = Math.Max(1.0, itemSize.Height);
            Size size7;
            Size size6;

            int num4 = (double.IsInfinity(availableSize.Height) ? rowsOrColumns : ((int)Math.Ceiling(availableSize.Width / num)));
            size6 = new Size(num4, itemsInDirection);
            size7 = new Size(Math.Min(availableSize.Width, (double)num4 * num), Math.Min(availableSize.Height, (double)itemsInDirection * num2));

            this.scrollData = new ScrollData
            {
                Offset = this.scrollData.Offset,
                Viewport = (isPixelBased ? size7 : size6),
                Extent = (isPixelBased ? size4 : size5)
            };
            bool flag4 = ScrollOwner != null;
            if (flag4 && this.scrollData.Offset.X + this.scrollData.Viewport.Width > this.scrollData.Extent.Width)
            {
                this.scrollData.Offset.X = Math.Max(0.0, this.scrollData.Extent.Width - this.scrollData.Viewport.Width);
            }

            if (flag4 && this.scrollData.Offset.Y + this.scrollData.Viewport.Height > this.scrollData.Extent.Height)
            {
                this.scrollData.Offset.Y = Math.Max(0.0, this.scrollData.Extent.Height - this.scrollData.Viewport.Height);
            }

            int num5 = -1;
            int num6 = -1;
            int itemCount = ItemCount;
            bool flag5 = ShouldVirtualize();
            if (itemCount > 0)
            {
                double num7 = Math.Max(0.0, HorizontalOffset - point.X);
                double num8 = num;
                double num9 = 0.0;
                double num10 = availableSize.Width;
                if (flag5)
                {
                    num5 = (isPixelBased ? ((int)Math.Floor(Math.Max(0.0, num7 - num9) / num8) * itemsInDirection) : ((int)(num7 * (double)itemsInDirection)));
                    if (double.IsInfinity(num10))
                    {
                        num6 = itemCount - 1;
                    }
                    else
                    {
                        num6 = (isPixelBased ? ((int)Math.Ceiling((Math.Max(0.0, num7 - num9) + num10) / num8) * itemsInDirection - 1) : (num5 + (int)Math.Ceiling(num10 / num8) * itemsInDirection - 1));
                        num6 = Math.Min(itemCount - 1, num6);
                    }
                }
                else
                {
                    num5 = 0;
                    num6 = itemCount - 1;
                }

                CleanUpChildren(num5, num6);
                if (num5 >= 0 && num5 < itemCount && num6 >= 0)
                {
                    GenerateContainers(num5, num6);
                }

                if (hierarchicalVirtualizationAndScrollInfo != null)
                {
                    Position position = GetPosition(num5, itemsInDirection);
                    Position position2 = GetPosition(num6, itemsInDirection);
                    int num11 = position2.Column - position.Column;
                    num11++;
                    num11 = Math.Max(0, num11);
                    size6 = new Size(num11, (num11 > 0) ? 1 : 0);
                    if (isPixelBased)
                    {
                        size5 = size6;
                        if (size5 == default(Size))
                        {
                            size5 = new Size(1.0, 1.0);
                        }
                    }

                    int num12 = 0;
                    Size logicalSizeBeforeViewport = new Size(num12, size6.Height);
                    int num13 = 0;
                    Size logicalSizeAfterViewport = new Size(num13, size6.Height);
                    if (size6 == default(Size))
                    {
                        size7 = default(Size);
                    }

                    double num14 = position.Column * itemSize.Width;
                    num14 = Math.Max(0.0, HorizontalOffset - num14);
                    if (HorizontalOffset > point.X)
                    {
                        num14 = Math.Max(0.0, num14 - point.X);
                    }

                    Size pixelSizeBeforeViewport = new Size(num14, size7.Height);
                    double num15 = position2.Column * itemSize.Width;
                    num15 = ((num6 < 0 || !isPixelBased) ? 0.0 : (itemSize.Width - (hierarchicalVirtualizationAndScrollInfo.Constraints.Viewport.Size.Width - point.X - num15 + HorizontalOffset)));
                    num15 = Math.Max(0.0, num15);
                    Size pixelSizeAfterViewport = new Size(num15, size7.Height);
                    hierarchicalVirtualizationAndScrollInfo.ItemDesiredSizes = new HierarchicalVirtualizationItemDesiredSizes(pixelSizeInViewport: (!isPixelBased) ? new Size(size6.Width * num8, size7.Height) : new Size(Math.Min(size7.Width, Math.Max(0.0, ExtentWidth - HorizontalOffset)), size7.Height), logicalSize: size5, logicalSizeInViewport: size6, logicalSizeBeforeViewport: logicalSizeBeforeViewport, logicalSizeAfterViewport: logicalSizeAfterViewport, pixelSize: size4, pixelSizeBeforeViewport: pixelSizeBeforeViewport, pixelSizeAfterViewport: pixelSizeAfterViewport);
                }
            }
            else
            {
                CleanUpChildren(0, base.InternalChildren.Count);
                if (hierarchicalVirtualizationAndScrollInfo != null)
                {
                    hierarchicalVirtualizationAndScrollInfo.ItemDesiredSizes = default(HierarchicalVirtualizationItemDesiredSizes);
                }
            }

            if (scrollData.Offset != this.scrollData.Offset || scrollData.Viewport != this.scrollData.Viewport || scrollData.Extent != this.scrollData.Extent)
            {
                InvalidateScrollInfo();
            }

            layoutData.FirstVisibleIndex = num5;
            layoutData.LastVisibleIndex = num6;
            layoutData.AvailableSize = availableSize;
            CollapseRecycledElements();
            if (ScrollOwner == null)
            {
                return size4;
            }

            return new Size(Math.Min(availableSize.Width, size4.Width), Math.Min(availableSize.Height, size4.Height));
        }

        private void CollapseRecycledElements()
        {
            foreach (UIElement recycledElement in recycledElements)
            {
                recycledElement.Visibility = Visibility.Collapsed;
            }
        }

        //
        // Zusammenfassung:
        //     Arranges the children.
        //
        // Parameter:
        //   finalSize:
        //     The available size.
        //
        // Rückgabewerte:
        //     The used size.
        protected override Size ArrangeOverride(Size finalSize)
        {
            bringIntoViewIndex = -1;
            if (ItemCount == 0)
            {
                return finalSize;
            }

            GetRowsAndColumns(finalSize, out var _, out var itemsInDirection);
            double arrangeOffset = GetArrangeOffset(itemsInDirection);
            foreach (UIElement generatedContainer in generatedContainers)
            {
                int index = Generator.IndexFromContainer(generatedContainer);
                Position position = GetPosition(index, itemsInDirection);
                Rect rect = new Rect((double)position.Column * itemSize.Width, (double)position.Row * itemSize.Height, itemSize.Width, itemSize.Height);
                ArrangeContainer(generatedContainer, rect, arrangeOffset);
            }

            return finalSize;
        }

        protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnGotKeyboardFocus(e);
            UpdateFocusedIndex(e.NewFocus as DependencyObject);
        }

        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnLostKeyboardFocus(e);
            focusedIndex = -1;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            ItemsControl itemsOwner = ItemsControl.GetItemsOwner(this);
            if (itemsOwner != null && itemsOwner.IsGrouping && (e.Key == Key.Prior || e.Key == Key.Next))
            {
                e.Handled = true;
            }
            else if ((e.KeyboardDevice.Modifiers & ModifierKeys.Alt) == 0)
            {
                bool flag = base.FlowDirection == FlowDirection.RightToLeft;
                bool flag2 = (e.KeyboardDevice.Modifiers & ModifierKeys.Shift) != 0;
                if (e.Key == Key.Down)
                {
                    e.Handled = MoveFocus(FocusNavigationDirection.Down);
                }
                else if (e.Key == Key.Up)
                {
                    e.Handled = MoveFocus(FocusNavigationDirection.Up);
                }
                else if (e.Key == Key.Left)
                {
                    e.Handled = MoveFocus(flag ? FocusNavigationDirection.Right : FocusNavigationDirection.Left);
                }
                else if (e.Key == Key.Right)
                {
                    e.Handled = MoveFocus(FocusNavigationDirection.Right);
                }
                else if (e.Key == Key.Home)
                {
                    e.Handled = MoveFocus(FocusNavigationDirection.First);
                }
                else if (e.Key == Key.End)
                {
                    e.Handled = MoveFocus(FocusNavigationDirection.Last);
                }
                else if (e.Key == Key.Tab)
                {
                    FocusNavigationDirection direction = (flag2 ? FocusNavigationDirection.Previous : FocusNavigationDirection.Next);
                    e.Handled = MoveFocus(direction);
                }
                else if (e.Key == Key.Prior)
                {
                    e.Handled = NavigateByPage(FocusNavigationDirection.Up, focusedIndex);
                }
                else if (e.Key == Key.Next)
                {
                    e.Handled = NavigateByPage(FocusNavigationDirection.Down, focusedIndex);
                }
            }
        }

        private static void OnAppearancePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            VirtualizingWrapPanel virtualizingWrapPanel = d as VirtualizingWrapPanel;
            if (virtualizingWrapPanel != null)
            {
                if (e.Property == ItemWidthProperty)
                {
                    virtualizingWrapPanel.itemSize = new Size((double)e.NewValue, virtualizingWrapPanel.ItemHeight);
                }
                else if (e.Property == ItemHeightProperty)
                {
                    virtualizingWrapPanel.itemSize = new Size(virtualizingWrapPanel.ItemWidth, (double)e.NewValue);
                }

                virtualizingWrapPanel.bringIntoViewIndex = -1;
                virtualizingWrapPanel.InvalidateMeasure();
            }
        }

        private static Point GetOffset(GroupItem child)
        {
            Panel panel = ((IHierarchicalVirtualizationAndScrollInfo)child)?.ItemsHost;
            if (panel == null || !panel.IsVisible)
            {
                return default(Point);
            }

            GeneralTransform generalTransform = child.TransformToDescendant(panel);
            if (generalTransform == null)
            {
                return default(Point);
            }

            Thickness thickness = (child as FrameworkElement)?.Margin ?? default(Thickness);
            Rect rect = new Rect(default(Point), ((IHierarchicalVirtualizationAndScrollInfo)child).HeaderDesiredSizes.PixelSize);
            rect.Offset(0.0 - thickness.Left, 0.0 - thickness.Top);
            Rect rect2 = generalTransform.TransformBounds(rect);
            double num = (rect2.Left.AreClose(0.0) ? 0.0 : (0.0 - rect2.Left));
            double y = (rect2.Top.AreClose(0.0) ? 0.0 : (0.0 - rect2.Top));
            if (num.AreClose(0.0))
            {
                FrameworkElement frameworkElement = VisualTreeHelper.GetParent(((IHierarchicalVirtualizationAndScrollInfo)child).ItemsHost) as FrameworkElement;
                if (frameworkElement != null)
                {
                    num = frameworkElement.Margin.Left;
                }
            }

            return new Point(num, y);
        }

        private double GetArrangeOffset(int itemsInDirection)
        {
            if (IsPixelBased)
            {
                return HorizontalOffset;
            }

            Position position = GetPosition(layoutData.FirstVisibleIndex, itemsInDirection);
            return position.Column * itemSize.Width;
        }

        private double GetPageOffset()
        {
            double num = ViewportWidth;
            bool flag = IsFullyVisible(layoutData.LastVisibleIndex);
            if (!IsPixelBased && !flag)
            {
                num = Math.Max(0.0, num - 1.0);
            }

            return num;
        }

        private bool NavigateByPage(FocusNavigationDirection direction, int focusIndex)
        {
            int itemsInDirection;
            int pageNavigationFocusIndex = GetPageNavigationFocusIndex(direction, focusIndex, out itemsInDirection);
            if (pageNavigationFocusIndex != focusedIndex)
            {
                Position position = GetPosition(pageNavigationFocusIndex, itemsInDirection);
                FocusItemAt(pageNavigationFocusIndex, position);
                return true;
            }

            return false;
        }

        private int GetPageNavigationFocusIndex(FocusNavigationDirection direction, int focusIndex, out int itemsInDirection)
        {
            GetRowsAndColumns(layoutData.AvailableSize, out var _, out itemsInDirection);
            Position position = GetPosition(focusIndex, itemsInDirection);
            Position position2 = GetPosition(layoutData.FirstVisibleIndex, itemsInDirection);
            Position position3 = GetPosition(layoutData.LastVisibleIndex, itemsInDirection);
            if (direction == FocusNavigationDirection.Up)
            {
                return CalculatePageUpFocusIndex(itemsInDirection, focusIndex, position, position2, position3);
            }

            return CalculatePageDownFocusIndex(itemsInDirection, focusIndex, position, position2, position3);
        }

        private int CalculatePageUpFocusIndex(int itemsInDirection, int focusIndex, Position focused, Position firstVisible, Position lastVisible)
        {
            bool flag = layoutData.FirstVisibleIndex <= focusIndex && focusIndex <= layoutData.LastVisibleIndex;
            int val;

            if (flag && focused.Column > firstVisible.Column)
            {
                val = firstVisible.Column * itemsInDirection + focused.Row;
            }
            else
            {
                int num3 = LastFullyVisibleRowOrColumn(lastVisible.Column);
                int num4 = Math.Max(1, num3 - firstVisible.Column);
                val = focusIndex - num4 * itemsInDirection;
            }

            return Math.Max(0, val);
        }

        private int CalculatePageDownFocusIndex(int itemsInDirection, int focusIndex, Position focused, Position firstVisible, Position lastVisible)
        {
            bool flag = layoutData.FirstVisibleIndex <= focusIndex && focusIndex <= layoutData.LastVisibleIndex;
            int val;

            int num3 = LastFullyVisibleRowOrColumn(lastVisible.Column);
            if (flag && focused.Column < num3)
            {
                val = num3 * itemsInDirection + focused.Row;
            }
            else
            {
                int num4 = Math.Max(1, num3 - firstVisible.Column);
                val = focusIndex + num4 * itemsInDirection;
            }

            return Math.Min(ItemCount - 1, val);
        }

        private int LastFullyVisibleRowOrColumn(int lastGeneratedRowOrColumn)
        {
            if (!IsFullyVisible(layoutData.LastVisibleIndex))
            {
                lastGeneratedRowOrColumn = Math.Max(0, lastGeneratedRowOrColumn - 1);
            }

            return lastGeneratedRowOrColumn;
        }

        private bool IsFullyVisible(int generatedIndex)
        {
            UIElement uIElement = Generator.ContainerFromIndex(generatedIndex) as UIElement;
            if (uIElement == null)
            {
                return false;
            }

            ScrollViewer scrollViewer = uIElement.ParentOfType<ScrollViewer>();
            if (scrollViewer == null)
            {
                return false;
            }

            Rect rect = uIElement.TransformToVisual(scrollViewer).TransformBounds(new Rect(uIElement.RenderSize));

            return rect.Right < base.RenderSize.Width;
        }

        private void GetRowsAndColumns(Size availableSize, out int rowsOrColumns, out int itemsInDirection)
        {
            double num = availableSize.Height;
            int itemCount = ItemCount;
            if (itemCount == 0)
            {
                rowsOrColumns = 0;
                itemsInDirection = 0;
                return;
            }

            if (double.IsInfinity(num))
            {
                rowsOrColumns = 1;
                itemsInDirection = itemCount;
                return;
            }

            double num2 = itemSize.Height;
            itemsInDirection = (int)Math.Floor(num / num2);
            if (itemsInDirection <= 0)
            {
                itemsInDirection = 1;
            }

            if (itemsInDirection >= itemCount)
            {
                rowsOrColumns = 1;
            }
            else
            {
                rowsOrColumns = (int)Math.Ceiling((double)itemCount / (double)itemsInDirection);
                _ = num % num2;
                _ = 0.0;
            }

            itemsInDirection = Math.Min(itemsInDirection, itemCount);
        }

        private void GenerateContainers(int firstVisibleIndex, int lastVisibleIndex)
        {
            UIElementCollection internalChildren = base.InternalChildren;
            IItemContainerGenerator itemContainerGenerator = base.ItemContainerGenerator;
            if (itemContainerGenerator == null)
            {
                return;
            }

            GeneratorPosition position = itemContainerGenerator.GeneratorPositionFromIndex(firstVisibleIndex);
            int num = ((position.Offset == 0) ? position.Index : (position.Index + 1));
            using (itemContainerGenerator.StartAt(position, GeneratorDirection.Forward, allowStartAtRealizedItem: true))
            {
                int num2 = firstVisibleIndex;
                while (num2 <= lastVisibleIndex)
                {
                    bool isNewlyRealized;
                    UIElement uIElement = itemContainerGenerator.GenerateNext(out isNewlyRealized) as UIElement;
                    if (uIElement != null)
                    {
                        if (!isNewlyRealized)
                        {
                            if (!uIElement.IsVisible)
                            {
                                uIElement.ClearValue(UIElement.VisibilityProperty);
                            }

                            recycledElements.Remove(uIElement);
                        }
                        else if (num >= internalChildren.Count)
                        {
                            AddInternalChild(uIElement);
                        }
                        else
                        {
                            InsertInternalChild(num, uIElement);
                        }

                        itemContainerGenerator.PrepareItemContainer(uIElement);
                        generatedContainers.Add(uIElement);
                        GeneratorPosition key = itemContainerGenerator.GeneratorPositionFromIndex(num2);
                        if (!generatedContainersByPosition.ContainsKey(key))
                        {
                            generatedContainersByPosition.Add(key, uIElement);
                        }

                        uIElement.Measure(itemSize);
                    }

                    num2++;
                    num++;
                }
            }
        }

        private void RemoveChildRange(GeneratorPosition position, int itemUICount)
        {
            if (!base.IsItemsHost || itemUICount <= 0)
            {
                return;
            }

            for (int i = 0; i < itemUICount; i++)
            {
                GeneratorPosition key = new GeneratorPosition(position.Index + i, position.Offset);
                if (generatedContainersByPosition.ContainsKey(key))
                {
                    UIElement element = generatedContainersByPosition[key];
                    int num = base.InternalChildren.IndexOf(element);
                    if (num != -1)
                    {
                        RemoveInternalChildRange(num, 1);
                    }
                }
            }
        }

        private void ArrangeContainer(UIElement child, Rect rect, double offset)
        {
            if (ScrollOwner != null || !IsPixelBased)
            {
                rect.X -= offset;
            }

            child.Arrange(rect);
        }

        private void UpdateFocusedIndex(DependencyObject focused)
        {
            if (focused == null)
            {
                focusedIndex = -1;
                return;
            }

            DependencyObject dependencyObject = focused;
            ItemContainerGenerator generator = Generator;
            while (dependencyObject != null && generator != null)
            {
                int num = generator.IndexFromContainer(dependencyObject);
                if (num == -1)
                {
                    dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
                    continue;
                }

                focusedIndex = num;
                break;
            }
        }

        private Position GetPosition(int index, int itemsInDirection)
        {
            if (itemsInDirection < 1)
            {
                return default(Position);
            }

            int num = index / itemsInDirection;
            int num2 = index % itemsInDirection;

            return new Position(num2, num);
        }

        private int ComputeFocusIndexFromDirection(FocusNavigationDirection direction, int focusIndex, int itemsInDirection)
        {
            int num = focusIndex;
            int itemCount = ItemCount;
            switch (direction)
            {
                case FocusNavigationDirection.Next:
                    num++;
                    break;
                case FocusNavigationDirection.Previous:
                    num--;
                    break;
                case FocusNavigationDirection.First:
                    num = 0;
                    break;
                case FocusNavigationDirection.Last:
                    num = itemCount - 1;
                    break;
                case FocusNavigationDirection.Left:
                    num -= itemsInDirection;
                    break;
                case FocusNavigationDirection.Right:
                    num += itemsInDirection;
                    break;
                case FocusNavigationDirection.Up:
                    num -= itemsInDirection;
                    break;
                case FocusNavigationDirection.Down:
                    num += itemsInDirection;
                    break;
            }

            num = Math.Min(itemCount - 1, num);
            return Math.Max(0, num);
        }

        private int GetNewFocusedIndex(FocusNavigationDirection direction, int focusIndex)
        {
            GetRowsAndColumns(layoutData.AvailableSize, out var _, out var itemsInDirection);
            return ComputeFocusIndexFromDirection(direction, focusIndex, itemsInDirection);
        }

        private bool MoveFocus(FocusNavigationDirection direction)
        {
            GetRowsAndColumns(layoutData.AvailableSize, out var _, out var itemsInDirection);
            int newFocusedIndex = GetNewFocusedIndex(direction, focusedIndex);
            if (focusedIndex == newFocusedIndex)
            {
                return false;
            }

            Position position = GetPosition(focusedIndex, itemsInDirection);
            Position position2 = GetPosition(newFocusedIndex, itemsInDirection);
            if (CanMoveFocus(position, position2, direction))
            {
                FocusItemAt(newFocusedIndex, position2);
                return true;
            }

            return false;
        }

        private UIElement BringIndexIntoView(int nextFocusIndex, Position nextFocusedPosition)
        {
            bringIntoViewIndex = nextFocusIndex;
            FrameworkElement frameworkElement = Generator.ContainerFromIndex(nextFocusIndex) as FrameworkElement;
            Rect rect = new Rect((double)nextFocusedPosition.Column * itemSize.Width, (double)nextFocusedPosition.Row * itemSize.Height, itemSize.Width, itemSize.Height);
            if (frameworkElement == null)
            {
                GenerateContainers(nextFocusIndex, nextFocusIndex);
                frameworkElement = Generator.ContainerFromIndex(nextFocusIndex) as FrameworkElement;
                double offset = (!IsPixelBased) ? 0.0 : HorizontalOffset;
                ArrangeContainer(frameworkElement, rect, offset);
            }

            ItemsControl itemsOwner = ItemsControl.GetItemsOwner(this);
            if (frameworkElement != null)
            {
                if (itemsOwner != null && itemsOwner.IsGrouping)
                {
                    GroupItem groupItem = this.ParentOfType<GroupItem>();
                    if (groupItem != null)
                    {
                        (VisualTreeHelper.GetParent(groupItem) as VirtualizingStackPanel)?.MakeVisible(groupItem, rect);
                    }
                }
                else
                {
                    frameworkElement.BringIntoView(rect);
                    if (IsElementInView(frameworkElement, this))
                    {
                        bringIntoViewIndex = -1;
                    }
                }
            }

            if (itemsOwner != null && itemsOwner.ItemsSource != null)
            {
                object item = Generator.ItemFromContainer(frameworkElement);
                ICollectionView defaultView = CollectionViewSource.GetDefaultView(itemsOwner.ItemsSource);
                if (defaultView != null && IsSingleSelection(itemsOwner))
                {
                    defaultView.MoveCurrentTo(item);
                }
            }

            return frameworkElement;
        }

        private void FocusItemAt(int nextFocusIndex, Position nextFocusedPosition)
        {
            BringIndexIntoView(nextFocusIndex, nextFocusedPosition)?.Focus();
            focusedIndex = nextFocusIndex;
        }

        private static bool IsSingleSelection(ItemsControl owner)
        {
            if (owner is ListBox ownerlistBox) return ownerlistBox.SelectionMode == SelectionMode.Single;

            return true;
        }

        private static bool IsElementInView(FrameworkElement element, FrameworkElement container)
        {
            Rect rect = element.TransformToAncestor(container).TransformBounds(new Rect(0.0, 0.0, element.ActualWidth, element.ActualHeight));
            return new Rect(0.0, 0.0, container.ActualWidth, container.ActualHeight).Contains(rect);
        }

        private static bool CanMoveFocus(Position focused, Position next, FocusNavigationDirection direction)
        {
            switch (direction)
            {
                case FocusNavigationDirection.Next:
                case FocusNavigationDirection.Previous:
                case FocusNavigationDirection.First:
                case FocusNavigationDirection.Last:
                    if (focused.Column == next.Column)
                    {
                        return focused.Row != next.Row;
                    }

                    return true;
                case FocusNavigationDirection.Left:
                case FocusNavigationDirection.Right:
                    return focused.Column != next.Column;
                case FocusNavigationDirection.Up:
                case FocusNavigationDirection.Down:
                    return focused.Row != next.Row;
                default:
                    return false;
            }
        }

        private void InvalidateScrollInfo()
        {
            if (ScrollOwner != null)
            {
                ScrollOwner.InvalidateScrollInfo();
            }
        }

        private void MakeVisible(UIElement element)
        {
            ItemContainerGenerator generator = Generator;
            if (element == null || generator == null)
            {
                return;
            }

            for (int num = generator.IndexFromContainer(element); num == -1; num = generator.IndexFromContainer(element))
            {
                element = element.ParentOfType<UIElement>();
                if (element == null)
                {
                    return;
                }
            }

            ScrollViewer scrollViewer = element.ParentOfType<ScrollViewer>();
            if (scrollViewer != null)
            {
                GeneralTransform generalTransform = element.TransformToVisual(scrollViewer);
                Rect elementRectangle = generalTransform.TransformBounds(new Rect(new Point(0.0, 0.0), element.RenderSize));
                Point point = CalculateScrollOffset(elementRectangle);
                SetHorizontalOffset(point.X);
                SetVerticalOffset(point.Y);
            }
        }

        private Point CalculateScrollOffset(Rect elementRectangle)
        {
            double x = scrollData.Offset.X;
            double y = scrollData.Offset.Y;
            bool isPixelBased = IsPixelBased;
            if (elementRectangle.Bottom > base.RenderSize.Height)
            {
                double num = elementRectangle.Bottom - base.RenderSize.Height;
                double num2 = (isPixelBased ? num : ((double)Math.Sign(num) * Math.Ceiling(Math.Abs(num) / itemSize.Height)));
                y = scrollData.Offset.Y + num2;
            }

            if (elementRectangle.Top < 0.0)
            {
                double top = elementRectangle.Top;
                double num3 = (isPixelBased ? top : ((double)Math.Sign(top) * Math.Ceiling(Math.Abs(top) / itemSize.Height)));
                y = scrollData.Offset.Y + num3;
            }

            if (elementRectangle.Right > base.RenderSize.Width)
            {
                double num4 = elementRectangle.Right - base.RenderSize.Width;
                double num5 = (isPixelBased ? num4 : ((double)Math.Sign(num4) * Math.Ceiling(Math.Abs(num4) / itemSize.Width)));
                x = scrollData.Offset.X + num5;
            }

            if (elementRectangle.Left < 0.0)
            {
                double left = elementRectangle.Left;
                double num6 = (isPixelBased ? left : ((double)Math.Sign(left) * Math.Ceiling(Math.Abs(left) / itemSize.Width)));
                x = scrollData.Offset.X + num6;
            }

            return new Point(x, y);
        }

        private void CleanUpChildren(int minIndex, int maxIndex)
        {
            IRecyclingItemContainerGenerator recyclingItemContainerGenerator = base.ItemContainerGenerator as IRecyclingItemContainerGenerator;
            for (int num = generatedContainers.Count - 1; num >= 0; num--)
            {
                UIElement uIElement = generatedContainers[num];
                int num2 = Generator.IndexFromContainer(uIElement);
                if (num2 != bringIntoViewIndex && (num2 < minIndex || num2 > maxIndex))
                {
                    GeneratorPosition position = recyclingItemContainerGenerator.GeneratorPositionFromIndex(num2);
                    if (position.Index == -1)
                    {
                        int num3 = base.InternalChildren.IndexOf(uIElement);
                        if (num3 != -1)
                        {
                            RemoveInternalChildRange(num3, 1);
                        }
                    }
                    else if (IsRecycling)
                    {
                        recyclingItemContainerGenerator.Recycle(position, 1);
                        recycledElements.Add(uIElement);
                    }
                    else
                    {
                        recyclingItemContainerGenerator.Remove(position, 1);
                        int num4 = base.InternalChildren.IndexOf(uIElement);
                        if (num4 != -1)
                        {
                            RemoveInternalChildRange(num4, 1);
                        }
                    }
                }
            }

            generatedContainers.Clear();
            generatedContainersByPosition.Clear();
        }

        private IHierarchicalVirtualizationAndScrollInfo GetHierarchyScrollInfo(GroupItem groupItem)
        {
            VirtualizingStackPanel virtualizingStackPanel = ((groupItem != null) ? (VisualTreeHelper.GetParent(groupItem) as VirtualizingStackPanel) : null);
            if (groupItem != null && !((IHierarchicalVirtualizationAndScrollInfo)groupItem).MustDisableVirtualization && virtualizingStackPanel != null && virtualizingStackPanel.LogicalOrientationPublic == base.LogicalOrientationPublic && ShouldVirtualize())
            {
                return groupItem;
            }

            return null;
        }

        private bool ShouldVirtualize()
        {
            ItemsControl itemsOwner = ItemsControl.GetItemsOwner(this);
            if (itemsOwner == null)
            {
                return false;
            }

            bool isVirtualizing = VirtualizingPanel.GetIsVirtualizing(itemsOwner);
            bool isVirtualizingWhenGrouping = VirtualizingPanel.GetIsVirtualizingWhenGrouping(itemsOwner);
            bool isGrouping = itemsOwner.IsGrouping;
            return (!isGrouping && isVirtualizing) || (isGrouping && isVirtualizing && isVirtualizingWhenGrouping);
        }
    }
}