using Microsoft.IdentityModel.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace TestWPF
{
    public static class BadgeService
    {
        public static readonly DependencyProperty BadgeProperty = DependencyProperty.RegisterAttached("Badge", typeof(object), typeof(BadgeService), new FrameworkPropertyMetadata(null, BadgePropertyChanged));

        private static void BadgePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element)
            {
                if (e.OldValue != null)
                {
                    var adornerLayer = AdornerLayer.GetAdornerLayer(element);

                    if (adornerLayer != null)
                    {
                        var elementAdorners = adornerLayer.GetAdorners(element);

                        if (elementAdorners != null)
                        {
                            foreach (var badgeAdorner in elementAdorners.Where(x => x is BadgeAdorner))
                            {
                                adornerLayer.Remove(badgeAdorner);
                            }
                        }
                    }
                }

                if (e.NewValue != null)
                {
                    InitilizeBadge(element);
                }
            }
        }

        public static object GetBadge(DependencyObject obj)
        {
            return obj.GetValue(BadgeProperty);
        }

        public static void SetBadge(DependencyObject obj, object value)
        {
            obj.SetValue(BadgeProperty, value);
        }

        public static readonly DependencyProperty HorizontalOffsetProperty = DependencyProperty.RegisterAttached("HorizontalOffset", typeof(double), typeof(BadgeService), new FrameworkPropertyMetadata(0d, OffsetPropertyChanged, CoerceOffsetProperty));

        public static double GetHorizontalOffset(DependencyObject obj)
        {
            return (double)obj.GetValue(HorizontalOffsetProperty);
        }

        public static void SetHorizontalOffset(DependencyObject obj, double value)
        {
            obj.SetValue(HorizontalOffsetProperty, value);
        }

        public static readonly DependencyProperty VerticalOffsetProperty = DependencyProperty.RegisterAttached("VerticalOffset", typeof(double), typeof(BadgeService), new FrameworkPropertyMetadata(0d, OffsetPropertyChanged, CoerceOffsetProperty));

        public static double GetVerticalOffset(DependencyObject obj)
        {
            return (double)obj.GetValue(VerticalOffsetProperty);
        }

        public static void SetVerticalOffset(DependencyObject obj, double value)
        {
            obj.SetValue(VerticalOffsetProperty, value);
        }

        private static void OffsetPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UpdateElementBadgeAdorner(d);
        }

        private static object CoerceOffsetProperty(DependencyObject d, object baseValue)
        {
            var offset = (double)baseValue;

            if (offset > 1d) return 1d;
            if (offset < 0d) return 0;

            return offset;
        }

        private static void InitilizeBadge(UIElement element)
        {
            var adornerLayer = AdornerLayer.GetAdornerLayer(element);

            if (adornerLayer == null) element.Dispatcher.BeginInvoke(new Action(() =>
            {
                var adornerLayer = AdornerLayer.GetAdornerLayer(element);

                if (adornerLayer != null) CreateBadge(element, adornerLayer);

            }), System.Windows.Threading.DispatcherPriority.Loaded);
            else CreateBadge(element, adornerLayer);
        }

        private static void CreateBadge(UIElement element, AdornerLayer adornerLayer)
        {
            var badge = new Badge(element);

            var badgeAdorner = new BadgeAdorner(element, badge);

            adornerLayer.Add(badgeAdorner);
        }

        private static void UpdateElementBadgeAdorner(DependencyObject obj)
        {
            if (obj is UIElement element && GetBadge(obj) != null)
            {
                var adornerLayer = AdornerLayer.GetAdornerLayer(element);

                adornerLayer?.Update(element);
            }
        }
    }
}