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

        protected override Size MeasureOverride(Size availableSize)
        {
            if (double.IsInfinity(availableSize.Width) || double.IsInfinity(availableSize.Height)) throw new InvalidOperationException("TablePanel does not support scrolling.");

            var children = InternalChildren;

            var columnDefinitions = ColumnDefinitions;

            if (columnDefinitions.Count == 0)
            {
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

                var variableWidth = availableSize.Width;

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

                        child.Measure(new Size(width, availableSize.Height));
                    }
                }

                foreach (var autoWidthColumn in columnDefinitions.Where(x => x.Width.IsAuto))
                {
                    var columnIndex = columnDefinitions.IndexOf(autoWidthColumn);

                    var width = 0.0;

                    for (int i = columnIndex; i < children.Count; i += columnDefinitions.Count)
                    {
                        var child = children[i];

                        if (child != null)
                        {
                            child.Measure(new Size(autoWidthColumn.MaxWidth, availableSize.Height));

                            if (child.DesiredSize.Width > width) width = child.DesiredSize.Width;
                        }
                    }

                    CoerceColumnWidth(autoWidthColumn, ref width);

                    variableWidth -= width;

                    if (variableWidth < 0.0) variableWidth = 0.0;

                    _columnWidths[columnIndex] = width;
                }

                var starColumns = columnDefinitions.Where(x => x.Width.IsStar).ToList();

                var starColumnValueTotal = starColumns.Sum(x => x.Width.Value);

                foreach (var starWidthColumn in starColumns)
                {
                    double calculatedWidth;

                    if (starWidthColumn.Width.Value == 0.0) calculatedWidth = 0.0;
                    else calculatedWidth = variableWidth * starColumnValueTotal / starWidthColumn.Width.Value;

                    var width = calculatedWidth;

                    if (CoerceColumnWidth(starWidthColumn, ref width))
                    {
                        variableWidth += width - calculatedWidth;

                        if (variableWidth < 0.0) variableWidth = 0.0;
                    }

                    var columnIndex = columnDefinitions.IndexOf(starWidthColumn);

                    _columnWidths[columnIndex] = width;

                    for (int i = columnIndex; i < children.Count; i += columnDefinitions.Count)
                    {
                        var child = children[i];

                        if (child == null) continue;

                        child.Measure(new Size(width, availableSize.Height));
                    }
                }

                var height = 0.0;

                var finalColumnIndex = 0;
                var rowHeight = 0.0;

                for (int i = 0; i < children.Count; i++)
                {
                    var child = children[i];

                    var columnWidth = _columnWidths[finalColumnIndex];

                    if (child != null)
                    {
                        double childHeight;

                        if (child.IsMeasureValid) childHeight = child.DesiredSize.Height;
                        else
                        {
                            child.Measure(new Size(columnWidth, availableSize.Height));

                            childHeight = child.DesiredSize.Height;
                        }

                        if (childHeight > rowHeight) rowHeight = childHeight;
                    }

                    finalColumnIndex++;

                    if (finalColumnIndex == columnDefinitions.Count || i == children.Count - 1)
                    {
                        height += rowHeight;
                        finalColumnIndex = 0;
                        rowHeight = 0.0;
                    }
                }

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

                var rowStart = 0.0;

                for (int i = 0; i < children.Count; i++)
                {
                    var child = children[i];

                    rowChildren[columnIndex] = child;

                    columnIndex++;

                    if (columnIndex == _columnWidths.Length || i == children.Count - 1)
                    {
                        rowStart += ArrangeRow(rowChildren, rowStart);

                        columnIndex = 0;
                        rowChildren = new UIElement[_columnWidths.Length];
                    }
                }

                return new Size(finalSize.Width, rowStart);
            }
        }

        private double ArrangeRow(UIElement[] children, double rowStart)
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

                columnStart += columnWidth;
            }

            return rowHeight;
        }
    }
}