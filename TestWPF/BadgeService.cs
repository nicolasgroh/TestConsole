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
    public enum BadgeLocation
    {
        Center,
        TopLeft,
        TopRight,
        BottomRight,
        BottomLeft
    }

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

                        foreach (var badgeAdorner in elementAdorners.Where(x => x is BadgeAdorner))
                        {
                            adornerLayer.Remove(badgeAdorner);
                        }
                    }
                }

                if (e.NewValue != null)
                {
                    CreateBadge(element);
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

        public static readonly DependencyProperty LocationProperty = DependencyProperty.RegisterAttached("Location", typeof(BadgeLocation), typeof(BadgeService), new FrameworkPropertyMetadata(BadgeLocation.TopLeft, LocationPropertyChanged));

        private static void LocationPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            OnPlacementPropertyChanged(d);
        }

        public static BadgeLocation GetLocation(DependencyObject obj)
        {
            return (BadgeLocation)obj.GetValue(LocationProperty);
        }

        public static void SetLocation(DependencyObject obj, BadgeLocation value)
        {
            obj.SetValue(LocationProperty, value);
        }

        public static readonly DependencyProperty HorizontalOffsetProperty = DependencyProperty.RegisterAttached("HorizontalOffset", typeof(double), typeof(BadgeService), new FrameworkPropertyMetadata(0d, HorizontalOffsetPropertyChanged));

        private static void HorizontalOffsetPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            OnPlacementPropertyChanged(d);
        }

        public static double GetHorizontalOffset(DependencyObject obj)
        {
            return (double)obj.GetValue(HorizontalOffsetProperty);
        }

        public static void SetHorizontalOffset(DependencyObject obj, double value)
        {
            obj.SetValue(HorizontalOffsetProperty, value);
        }

        public static readonly DependencyProperty VerticalOffsetProperty = DependencyProperty.RegisterAttached("VerticalOffset", typeof(double), typeof(BadgeService), new FrameworkPropertyMetadata(0d, VerticalOffsetPropertyChanged));

        private static void VerticalOffsetPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            OnPlacementPropertyChanged(d);
        }

        public static double GetVerticalOffset(DependencyObject obj)
        {
            return (double)obj.GetValue(VerticalOffsetProperty);
        }

        public static void SetVerticalOffset(DependencyObject obj, double value)
        {
            obj.SetValue(VerticalOffsetProperty, value);
        }

        private static void CreateBadge(UIElement element)
        {
            var adornerLayer = AdornerLayer.GetAdornerLayer(element);

            if (adornerLayer != null)
            {
                var badge = new Badge(element);

                var badgeAdorner = new BadgeAdorner(element, badge);

                adornerLayer.Add(badgeAdorner);
            }
            else
            {
                var window = element.GetParentOfType<Window>();

                if (window != null)
                {
                    
                }
            }
        }

        private static void OnPlacementPropertyChanged(DependencyObject obj)
        {
            if (obj is UIElement element && GetBadge(obj) != null)
            {
                var adornerLayer = AdornerLayer.GetAdornerLayer(element);

                adornerLayer?.Update(element);
            }
        }
    }
}