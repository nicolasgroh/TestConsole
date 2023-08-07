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
    public class FlexPanel : CustomVirtualizingPanel
    {
        public static readonly DependencyProperty MinItemWidthProperty = DependencyProperty.Register("MinItemWidth", typeof(double), typeof(FlexPanel), new FrameworkPropertyMetadata(10.0, FrameworkPropertyMetadataOptions.AffectsMeasure));
        public double MinItemWidth
        {
            get { return (double)GetValue(MinItemWidthProperty); }
            set { SetValue(MinItemWidthProperty, value); }
        }

        public static readonly DependencyProperty MaximumItemsPerRowProperty = DependencyProperty.Register("MaximumItemsPerRow", typeof(int), typeof(FlexPanel), new FrameworkPropertyMetadata(int.MaxValue, FrameworkPropertyMetadataOptions.AffectsMeasure));
        public int MaximumItemsPerRow
        {
            get { return (int)GetValue(MaximumItemsPerRowProperty); }
            set { SetValue(MaximumItemsPerRowProperty, value); }
        }

        private static readonly DependencyPropertyKey ItemsPerRowPropertyKey = DependencyProperty.RegisterReadOnly("ItemsPerRow", typeof(int), typeof(FlexPanel), new FrameworkPropertyMetadata(1, ItemsPerRowPropertyChanged));

        private static void ItemsPerRowPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((FlexPanel)d).OnItemsPerRowChanged();
        }

        public int ItemsPerRow
        {
            get { return (int)GetValue(ItemsPerRowPropertyKey.DependencyProperty); }
            private set { SetValue(ItemsPerRowPropertyKey, value); }
        }

        public EventHandler ItemsPerRowChanged;

        protected virtual void OnItemsPerRowChanged()
        {
            ItemsPerRowChanged?.Invoke(this, EventArgs.Empty);
        }

        private int CalculateItemsPerRow(double viewportWidth)
        {
            var itemsPerRow = (int)(viewportWidth / MinItemWidth);

            if (itemsPerRow == 0) itemsPerRow = 1;

            if (ItemsPerRow > MaximumItemsPerRow) itemsPerRow = MaximumItemsPerRow;

            if (itemsPerRow != ItemsPerRow) ItemsPerRow = itemsPerRow;

            return itemsPerRow;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var width = ScrollInfo == null ? availableSize.Width : ScrollInfo.ViewportWidth;
            System.Diagnostics.Debug.WriteLine($"Measure: {width}\n{Environment.StackTrace}");
            var itemsPerRow = CalculateItemsPerRow(width);

            var itemWidth = width / itemsPerRow;

            var rowItemCounter = 0;
            var currentRowPosition = 0.0;
            UIElement[] currentRow = new UIElement[itemsPerRow];

            for (int i = 0; i < InternalChildren.Count; i++)
            {
                var child = InternalChildren[i];

                currentRow[rowItemCounter] = child;

                rowItemCounter++;

                if (rowItemCounter == itemsPerRow || i == InternalChildren.Count - 1)
                {
                    currentRowPosition += MeasureRow(currentRow, itemWidth, currentRowPosition);

                    rowItemCounter = 0;

                    currentRow = new UIElement[itemsPerRow];
                }
            }

            availableSize = new Size(double.IsInfinity(width) ? double.MaxValue : width, currentRowPosition);

            return availableSize;
        }

        private double MeasureRow(UIElement[] currentRow, double itemWidth, double rowPosition)
        {
            double rowHeight = 0.0;

            bool shouldDeVirtualizeIfNeeded = false;

            for (int i = 0; i < currentRow.Length; i++)
            {
                var child = currentRow[i];

                if (child == null) continue;

                child.Measure(new Size(itemWidth, double.MaxValue));

                if (child is VirtualizableContainer virtualizableContainer && virtualizableContainer.IsVirtualized) shouldDeVirtualizeIfNeeded = true;

                if (child.DesiredSize.Height > rowHeight) rowHeight = child.DesiredSize.Height;
            }

            var newRowPosition = rowPosition + rowHeight;

            if (ScrollInfo != null && shouldDeVirtualizeIfNeeded && (newRowPosition > ScrollInfo.VerticalOffset || rowPosition < ScrollInfo.VerticalOffset + ScrollInfo.ViewportHeight))
            {
                for (int i = 0; i < currentRow.Length; i++)
                {
                    var child = currentRow[i];

                    if (child == null) continue;

                    if (child is VirtualizableContainer virtualizableContainer)
                    {
                        virtualizableContainer.DeVirtualize();

                        virtualizableContainer.Measure(new Size(itemWidth, double.MaxValue));

                        if (child.DesiredSize.Height > rowHeight) rowHeight = child.DesiredSize.Height;
                    }
                }
            }

            return rowHeight;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var width = ScrollInfo == null ? finalSize.Width : ScrollInfo.ViewportWidth;
            System.Diagnostics.Debug.WriteLine($"Arrange Width {width}");
            var itemsPerRow = CalculateItemsPerRow(width);

            var itemWidth = width / itemsPerRow;

            var rowItemCounter = 0;
            var currentRowPosition = 0.0;
            UIElement[] currentRow = new UIElement[itemsPerRow];
            
            for (int i = 0; i < InternalChildren.Count; i++)
            {
                var child = InternalChildren[i];

                currentRow[rowItemCounter] = child;

                rowItemCounter++;

                if (rowItemCounter == itemsPerRow || i == InternalChildren.Count - 1)
                {
                    currentRowPosition += ArrangeRow(currentRow, itemWidth, currentRowPosition);

                    rowItemCounter = 0;

                    currentRow = new UIElement[itemsPerRow];
                }
            }

            return new Size(finalSize.Width, currentRowPosition);
        }

        private double ArrangeRow(UIElement[] currentRow, double itemWidth, double rowPosition)
        {
            var rowHeight = currentRow.Max(i => i?.DesiredSize.Height).GetValueOrDefault();

            var newRowPosition = rowPosition + rowHeight;

            var virtualizeRow = ScrollInfo != null && (newRowPosition < ScrollInfo.VerticalOffset || rowPosition > ScrollInfo.VerticalOffset + ScrollInfo.ViewportHeight);

            for (int i = 0; i < currentRow.Length; i++)
            {
                var child = currentRow[i];

                if (child == null) continue;

                child.Arrange(new Rect(new Point(i * itemWidth, rowPosition), new Size(itemWidth, rowHeight)));

                if (virtualizeRow && child is VirtualizableContainer virtualizableContainer && !virtualizableContainer.IsVirtualized) virtualizableContainer.Virtualize();
            }

            return rowHeight;
        }
    }
}