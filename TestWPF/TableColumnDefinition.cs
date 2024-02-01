using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TestWPF
{
    public class TableColumnDefinition : DependencyObject
    {
        public static readonly DependencyProperty WidthProperty = DependencyProperty.Register("Width", typeof(GridLength), typeof(TableColumnDefinition), new PropertyMetadata(new GridLength(1, GridUnitType.Star), WidthPropertyChanged));

        private static void WidthPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var columnDefinition = (TableColumnDefinition)d;

            columnDefinition.LayoutPropertyChanged?.Invoke(columnDefinition, WidthProperty);
        }

        public GridLength Width
        {
            get { return (GridLength)GetValue(WidthProperty); }
            set { SetValue(WidthProperty, value); }
        }

        public static readonly DependencyProperty MinWidthProperty = DependencyProperty.Register("MinWidth", typeof(double), typeof(TableColumnDefinition), new PropertyMetadata(0.0, MinWidthPropertyChanged));

        private static void MinWidthPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var columnDefinition = (TableColumnDefinition)d;

            columnDefinition.LayoutPropertyChanged?.Invoke(columnDefinition, MinWidthProperty);
        }

        public double MinWidth
        {
            get { return (double)GetValue(MinWidthProperty); }
            set { SetValue(MinWidthProperty, value); }
        }

        public static readonly DependencyProperty MaxWidthProperty = DependencyProperty.Register("MaxWidth", typeof(double), typeof(TableColumnDefinition), new PropertyMetadata(double.MaxValue, MaxWidthPropertyChanged));

        private static void MaxWidthPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var columnDefinition = (TableColumnDefinition)d;

            columnDefinition.LayoutPropertyChanged?.Invoke(columnDefinition, MaxWidthProperty);
        }

        public double MaxWidth
        {
            get { return (double)GetValue(MaxWidthProperty); }
            set { SetValue(MaxWidthProperty, value); }
        }

        public event TableColumnDefinitionLayoutPropertyChangedEventHandler LayoutPropertyChanged;
    }

    public delegate void TableColumnDefinitionLayoutPropertyChangedEventHandler(TableColumnDefinition columnDefinition, DependencyProperty property);
}