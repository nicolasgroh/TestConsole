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
    public class Badge : ContentControl
    {
        static Badge()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Badge), new FrameworkPropertyMetadata(typeof(Badge)));
        }

        internal Badge(UIElement placementTarget)
        {
            _placementTarget = placementTarget;

            SetBinding(ContentProperty, new Binding()
            {
                Path = new PropertyPath(BadgeService.BadgeProperty),
                Source = _placementTarget
            });
        }

        private UIElement _placementTarget;

        public static readonly DependencyProperty HorizontalOffsetProperty = BadgeService.HorizontalOffsetProperty.AddOwner(typeof(Badge), new FrameworkPropertyMetadata(0d, HorizontalAlignmentPropertyChanged, CoerceHorizontalOffsetProperty));

        private static void HorizontalAlignmentPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Badge)d).PlacementPropertyChanged();
        }

        private static object CoerceHorizontalOffsetProperty(DependencyObject d, object value)
        {
            return CoerceOffsetProperty(d, HorizontalOffsetProperty, (double)value);
        }

        public double HorizontalOffset
        {
            get { return (double)GetValue(HorizontalOffsetProperty); }
            set { SetValue(HorizontalOffsetProperty, value); }
        }

        public static readonly DependencyProperty VerticalOffsetProperty = BadgeService.VerticalOffsetProperty.AddOwner(typeof(Badge), new FrameworkPropertyMetadata(0d, VerticalOffsetPropertyChanged, CoerceVerticalOffsetProperty));

        private static void VerticalOffsetPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Badge)d).PlacementPropertyChanged();
        }

        private static object CoerceVerticalOffsetProperty(DependencyObject d, object value)
        {
            return CoerceOffsetProperty(d, VerticalOffsetProperty, (double)value);
        }

        public double VerticalOffset
        {
            get { return (double)GetValue(VerticalOffsetProperty); }
            set { SetValue(VerticalOffsetProperty, value); }
        }

        private static object CoerceOffsetProperty(DependencyObject d, DependencyProperty property, double value)
        {
            var placementTargetValueSource = DependencyPropertyHelper.GetValueSource(d, property);

            if (placementTargetValueSource.BaseValueSource == BaseValueSource.Default) return BadgeService.CoerceOffset(value);
            else return d.GetValue(property);
        }

        private void PlacementPropertyChanged()
        {
            BadgeService.UpdateElementBadgeAdorner(_placementTarget);
        }
    }
}