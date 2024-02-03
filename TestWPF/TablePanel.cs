using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace TestWPF
{
    public class TablePanel : Panel
    {
        public static readonly DependencyProperty VerticalGapProperty = DependencyProperty.Register("VerticalGap", typeof(double), typeof(TablePanel), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));
        public double VerticalGap
        {
            get { return (double)GetValue(VerticalGapProperty); }
            set { SetValue(VerticalGapProperty, value); }
        }

        public static readonly DependencyProperty HorizontalGapProperty = DependencyProperty.Register("HorizontalGap", typeof(double), typeof(TablePanel), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));
        public double HorizontalGap
        {
            get { return (double)GetValue(HorizontalGapProperty); }
            set { SetValue(HorizontalGapProperty, value); }
        }

        private TableColumnDefinitionCollection _columnDefinitions;
        public TableColumnDefinitionCollection ColumnDefinitions
        {
            get
            {
                _columnDefinitions ??= new TableColumnDefinitionCollection(this);

                return _columnDefinitions;
            }
        }

        private double[] _columnWidths = new double[0];

        private bool CoerceColumnWidth(TableColumnDefinition columnDefinition, ref double width)
        {
            if (width < columnDefinition.MinWidth)
            {
                width = columnDefinition.MinWidth;
                return true;
            }
            if (width > columnDefinition.MaxWidth)
            {
                width = columnDefinition.MaxWidth;
                return true;
            }

            return false;
        }

        private void MeasureAbsoluteColumns(TableColumnDefinitionCollection columnDefinitions, UIElementCollection children, ref double variableWidth)
        {
            foreach (var absolueWidthColumn in columnDefinitions.Where(x => x.Width.IsAbsolute))
            {
                var width = absolueWidthColumn.Width.Value;

                CoerceColumnWidth(absolueWidthColumn, ref width);

                variableWidth -= width;

                var columnIndex = columnDefinitions.IndexOf(absolueWidthColumn);

                _columnWidths[columnIndex] = width;

                for (int i = columnIndex; i < children.Count; i += columnDefinitions.Count)
                {
                    var child = children[i];

                    if (child == null) continue;

                    child.Measure(new Size(width, double.MaxValue));
                }
            }
        }

        private void MeasureAutoColumns(TableColumnDefinitionCollection columnDefinitions, UIElementCollection children, ref double variableWidth)
        {
            foreach (var autoWidthColumn in columnDefinitions.Where(x => x.Width.IsAuto))
            {
                var columnIndex = columnDefinitions.IndexOf(autoWidthColumn);

                var width = 0.0;

                for (int i = columnIndex; i < children.Count; i += columnDefinitions.Count)
                {
                    var child = children[i];

                    if (child != null)
                    {
                        child.Measure(new Size(autoWidthColumn.MaxWidth, double.MaxValue));

                        if (child.DesiredSize.Width > width) width = child.DesiredSize.Width;
                    }
                }

                CoerceColumnWidth(autoWidthColumn, ref width);

                variableWidth -= width;

                if (variableWidth < 0.0) variableWidth = 0.0;

                _columnWidths[columnIndex] = width;
            }
        }

        private void MeasureStarColumns(TableColumnDefinitionCollection columnDefinitions, ref double variableWidth)
        {
            var starColumns = columnDefinitions.Where(x => x.Width.IsStar).ToList();

            var starColumnValueTotal = starColumns.Sum(x => x.Width.Value);

            IterateStarColumns(starColumns, (column) => columnDefinitions.IndexOf(column), ref starColumnValueTotal, ref variableWidth);
        }

        private void IterateStarColumns(List<TableColumnDefinition> starColumns, Func<TableColumnDefinition, int> getColumnIndex, ref double starColumnValueTotal, ref double variableWidth)
        {
            for (int i = 0; i < starColumns.Count; i++)
            {
                var starWidthColumn = starColumns[i];

                double calculatedWidth;

                if (starColumnValueTotal == 0.0) calculatedWidth = 0.0;
                else calculatedWidth = variableWidth / starColumnValueTotal * starWidthColumn.Width.Value;

                var width = calculatedWidth;

                var widthCoerced = CoerceColumnWidth(starWidthColumn, ref width);

                var columnIndex = getColumnIndex.Invoke(starWidthColumn);

                _columnWidths[columnIndex] = width;

                if (widthCoerced)
                {
                    starColumnValueTotal -= starWidthColumn.Width.Value;

                    if (starColumnValueTotal < 0.0) starColumnValueTotal = 0.0;

                    variableWidth -= width;

                    if (variableWidth < 0.0) variableWidth = 0.0;

                    starColumns.Remove(starWidthColumn);
                    IterateStarColumns(starColumns, getColumnIndex, ref starColumnValueTotal, ref variableWidth);
                    break;
                }
            }
        }

        private double MeasureTotalHeight(UIElementCollection children, double horizontalGap)
        {
            double height = 0.0;

            var columnIndex = 0;
            var rowHeight = 0.0;

            for (int i = 0; i < children.Count; i++)
            {
                var child = children[i];

                if (child != null)
                {
                    double childHeight;

                    if (child.IsMeasureValid) childHeight = child.DesiredSize.Height;
                    else
                    {
                        var columnWidth = _columnWidths[columnIndex];

                        child.Measure(new Size(columnWidth, double.MaxValue));

                        childHeight = child.DesiredSize.Height;
                    }

                    if (childHeight > rowHeight) rowHeight = childHeight;
                }

                columnIndex++;

                var rowFinished = columnIndex == _columnWidths.Length;
                var isLastChild = i == children.Count - 1;

                if (rowFinished || isLastChild)
                {
                    height += rowHeight;

                    if (!isLastChild) height += horizontalGap;

                    columnIndex = 0;
                    rowHeight = 0.0;
                }
            }

            return height;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (double.IsInfinity(availableSize.Width) || double.IsInfinity(availableSize.Height)) throw new InvalidOperationException("TablePanel does not support scrolling.");

            var children = InternalChildren;

            var columnDefinitions = ColumnDefinitions;

            if (columnDefinitions.Count == 0)
            {
                _columnWidths = Array.Empty<double>();

                for (int i = 0; i < children.Count; i++)
                {
                    var child = children[i];

                    child.Measure(availableSize);
                }

                return availableSize;
            }
            else
            {
                _columnWidths = new double[columnDefinitions.Count];

                var variableWidth = availableSize.Width - Math.Max(columnDefinitions.Count - 1, 0) * VerticalGap;

                MeasureAbsoluteColumns(columnDefinitions, children, ref variableWidth);

                MeasureAutoColumns(columnDefinitions, children, ref variableWidth);

                MeasureStarColumns(columnDefinitions, ref variableWidth);

                var height = MeasureTotalHeight(children, HorizontalGap);

                return new Size(availableSize.Width, height);
            }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (double.IsInfinity(finalSize.Width) || double.IsInfinity(finalSize.Height)) throw new InvalidOperationException("TablePanel does not support scrolling.");

            var children = InternalChildren;

            if (_columnWidths.Length == 0)
            {
                for (int i = 0; i < children.Count; i++)
                {
                    var child = children[i];

                    if (child == null) continue;

                    child.Arrange(new Rect(finalSize));
                }

                return finalSize;
            }
            else
            {
                var columnIndex = 0;
                var rowChildren = new UIElement[_columnWidths.Length];
                var vericalGap = VerticalGap;

                var rowStart = 0.0;

                for (int i = 0; i < children.Count; i++)
                {
                    var child = children[i];

                    rowChildren[columnIndex] = child;

                    columnIndex++;

                    var rowFinished = columnIndex == _columnWidths.Length;
                    var isLastChild = i == children.Count - 1;

                    if (rowFinished || isLastChild)
                    {
                        rowStart += ArrangeRow(rowChildren, rowStart, vericalGap);

                        if (!isLastChild) rowStart += HorizontalGap;

                        columnIndex = 0;
                        rowChildren = new UIElement[_columnWidths.Length];
                    }
                }

                return new Size(finalSize.Width, rowStart);
            }
        }

        private double ArrangeRow(UIElement[] children, double rowStart, double vericalGap)
        {
            var rowHeight = children.Where(x => x != null).Max(x => x.DesiredSize.Height);

            var columnStart = 0.0;

            for (int i = 0; i < _columnWidths.Length; i++)
            {
                var columnWidth = _columnWidths[i];

                if (i >= children.Length) break;

                var child = children[i];

                if (child == null) continue;

                child.Arrange(new Rect(new Point(columnStart, rowStart), new Size(columnWidth, rowHeight)));

                columnStart += columnWidth + vericalGap;
            }

            return rowHeight;
        }
    }
}