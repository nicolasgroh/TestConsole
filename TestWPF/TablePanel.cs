using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

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

        private double[] _columnWidths;

        private double CoerceColumnWidth(TableColumnDefinition columnDefinition, double width)
        {
            if (width < columnDefinition.MinWidth) return columnDefinition.MinWidth;
            if (width > columnDefinition.MaxWidth) return columnDefinition.MaxWidth;

            return width;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var children = InternalChildren;

            var columnDefinitions = ColumnDefinitions;

            _columnWidths = new double[columnDefinitions.Count];

            var variableWidth = availableSize.Width;

            foreach (var absolueWidthColumn in columnDefinitions.Where(x => x.Width.IsAbsolute))
            {
                var width = CoerceColumnWidth(absolueWidthColumn, absolueWidthColumn.Width.Value);

                variableWidth -= width;

                var columnIndex = columnDefinitions.IndexOf(absolueWidthColumn);

                _columnWidths[columnIndex] = width;

                for (int i = columnIndex; i < children.Count; i += columnDefinitions.Count)
                {
                    var child = children[i];

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

                    child.Measure(new Size(autoWidthColumn.MaxWidth, availableSize.Height));

                    if (child.DesiredSize.Width > width) width = child.DesiredSize.Width;
                }

                width = CoerceColumnWidth(autoWidthColumn, width);

                variableWidth -= width;

                _columnWidths[columnIndex] = width;
            }

            var starColumns = columnDefinitions.Where(x => x.Width.IsStar).ToList();

            var starColumnValueTotal = starColumns.Sum(x => x.Width.Value);

            foreach (var starWidthColumn in starColumns)
            {
                var calculatedWidth = variableWidth * starColumnValueTotal / starWidthColumn.Width.Value;

                var width = CoerceColumnWidth(starWidthColumn, calculatedWidth);

                if (width < calculatedWidth)
            }
        }
    }
}