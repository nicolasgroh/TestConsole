using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace TestWPF
{
    public class FlexCustomItemsControl : CustomVirtualizingItemsControl
    {
        public static readonly DependencyProperty MinItemWidthProperty = DependencyProperty.Register("MinItemWidth", typeof(double), typeof(FlexCustomItemsControl), new FrameworkPropertyMetadata(200.0));
        public double MinItemWidth
        {
            get { return (double)GetValue(MinItemWidthProperty); }
            set { SetValue(MinItemWidthProperty, value); }
        }

        private FlexPanel _itemsHost;

        protected override Panel CreateItemsHost()
        {
            if (_itemsHost != null) _itemsHost.ItemsPerRowChanged -= ItemsHost_ItemsPerRowChanged;

            _itemsHost = new FlexPanel();

            _itemsHost.SetBinding(FlexPanel.MinItemWidthProperty, new Binding() { Source = this, Path = new PropertyPath(MinItemWidthProperty) });

            _itemsHost.ItemsPerRowChanged += ItemsHost_ItemsPerRowChanged;

            return _itemsHost;
        }

        private void ItemsHost_ItemsPerRowChanged(object sender, EventArgs e)
        {
            UpdateBorders(0);
        }

        protected override void ContainerAdded(UIElement container, int index)
        {
            base.ContainerAdded(container, index);

            if (!IsCollectionChanging) return;

            UpdateBorders(index);
        }

        private void UpdateBorders(int startIndex)
        {
            var lastRowStartIndex = _itemsHost.Children.Count / _itemsHost.ItemsPerRow * _itemsHost.ItemsPerRow;
            var secondToLastRowStartIndex = Math.Max(0, lastRowStartIndex - _itemsHost.ItemsPerRow);

            for (int i = Math.Min(startIndex, secondToLastRowStartIndex); i < _itemsHost.Children.Count; i++)
            {
                var child = _itemsHost.Children[i];

                if (GetIsGeneratedContainer(child))
                {
                    int right = 0;
                    int bottom = 0;

                    var container = child as ContentControl;

                    // Just one Item per Row
                    if (_itemsHost.ItemsPerRow == 1)
                    {
                        // As long as it's not the last index, set the bottom Border
                        if (i < lastRowStartIndex) bottom = 1;
                    }
                    else
                    {
                        // If i + 1 ist divisable by ItemsPerRow, we have the last Item within that row
                        if ((i + 1) % _itemsHost.ItemsPerRow == 0 && i < lastRowStartIndex) bottom = 1;
                        else
                        {
                            right = 1;
                            if (i < lastRowStartIndex) bottom = 1;
                        }
                    }

                    container.BorderThickness = new Thickness(0, 0, right, bottom);
                }
            }
        }
    }
}