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
    public sealed class StackingBadgeCollection : BadgeCollection
    {
        public StackingBadgeCollection()
        {
            var stackPanel = new StackPanel();

            stackPanel.SetBinding(StackPanel.OrientationProperty, new Binding()
            {
                Path = new PropertyPath(OrientationProperty),
                Source = this
            });

            ItemsHost = stackPanel;
        }

        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register("Orientation", typeof(Orientation), typeof(StackingBadgeCollection), new FrameworkPropertyMetadata(Orientation.Horizontal));
        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }
    }
}