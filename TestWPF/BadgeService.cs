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
        private class BadgeAdorner : DecoratorAdorner
        {
            internal BadgeAdorner(UIElement adornedElement, Badge badge) : base(adornedElement)
            {
                _badge = badge;
                Child = _badge;
            }

            private Badge _badge;
            internal Badge Badge
            {
                get { return _badge; }
            }

            public override GeneralTransform GetDesiredTransform(GeneralTransform generalTransform)
            {
                if (generalTransform is Transform transform)
                {
                    CalculateOffsets(out var offsetX, out var offsetY);

                    var matrix = transform.Value;

                    matrix.OffsetX += offsetX;
                    matrix.OffsetY += offsetY;

                    return new MatrixTransform(matrix);
                }

                return generalTransform;
            }

            private double GetHorizontalOffset(UIElement adornedElement)
            {
                var adornedElementValueSource = DependencyPropertyHelper.GetValueSource(adornedElement, HorizontalOffsetProperty);

                if (adornedElementValueSource.BaseValueSource == BaseValueSource.Default) return _badge.HorizontalOffset;

                return BadgeService.GetHorizontalOffset(adornedElement);
            }

            private double GetVerticalOffset(UIElement adornedElement)
            {
                var adornedElementValueSource = DependencyPropertyHelper.GetValueSource(adornedElement, VerticalOffsetProperty);

                if (adornedElementValueSource.BaseValueSource == BaseValueSource.Default) return _badge.VerticalOffset;

                return BadgeService.GetVerticalOffset(adornedElement);
            }

            private void CalculateOffsets(out double offsetX, out double offsetY)
            {
                var adornedElement = AdornedElement;

                var adornedElementSize = adornedElement.RenderSize;
                var childSize = Child.DesiredSize;

                var horizontalOffset = GetHorizontalOffset(adornedElement);
                var verticalOffset = GetVerticalOffset(adornedElement);

                offsetX = adornedElementSize.Width * horizontalOffset;
                offsetX -= childSize.Width * horizontalOffset;

                offsetY = adornedElementSize.Height * verticalOffset;
                offsetY -= childSize.Height * verticalOffset;

                if (offsetX < 0) offsetX = 0;
                if (offsetY < 0) offsetY = 0;
            }
        }

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

        public static readonly DependencyProperty StyleProperty = DependencyProperty.RegisterAttached("Style", typeof(Style), typeof(BadgeService), new FrameworkPropertyMetadata(null, StylePropertyChanged));

        private static void StylePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element)
            {
                if (TryGetElementBadge(element, out var badge))
                {
                    badge.Style = (Style)e.NewValue;
                }
            }
        }

        public static Style GetStyle(DependencyObject obj)
        {
            return (Style)obj.GetValue(StyleProperty);
        }

        public static void SetStyle(DependencyObject obj, Style value)
        {
            obj.SetValue(StyleProperty, value);
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
            return CoerceOffset((double)baseValue);
        }

        internal static double CoerceOffset(double offset)
        {
            if (offset > 1d) return 1d;
            if (offset < 0d) return 0;

            return offset;
        }

        private static bool TryGetElementBadge(UIElement element, out Badge badge)
        {
            badge = null;

            var adornerLayer = AdornerLayer.GetAdornerLayer(element);

            if (adornerLayer != null)
            {
                var elementAdorners = adornerLayer.GetAdorners(element);

                var badgeAdorner = (BadgeAdorner)elementAdorners.FirstOrDefault(x => x is BadgeAdorner);

                if (badgeAdorner != null)
                {
                    badge = badgeAdorner.Badge;
                    return true;
                }
            }

            return false;
        }

        private static void InitilizeBadge(UIElement element)
        {
            var adornerLayer = AdornerLayer.GetAdornerLayer(element);

            if (adornerLayer == null) HookupLoaded(element);
            else CreateBadge(element, adornerLayer);
        }

        private static void HookupLoaded(UIElement element)
        {
            element.Dispatcher.BeginInvoke(new Action(() =>
            {
                var adornerLayer = AdornerLayer.GetAdornerLayer(element);

                if (adornerLayer != null) CreateBadge(element, adornerLayer);

            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private static void CreateBadge(UIElement element, AdornerLayer adornerLayer)
        {
            var badge = new Badge(element);

            var badgeAdorner = new BadgeAdorner(element, badge);

            adornerLayer.Add(badgeAdorner);
        }

        internal static void UpdateElementBadgeAdorner(DependencyObject obj)
        {
            if (obj is UIElement element && GetBadge(obj) != null)
            {
                var adornerLayer = AdornerLayer.GetAdornerLayer(element);

                if (adornerLayer != null && adornerLayer.GetAdorners(element).Length > 0)
                {
                    adornerLayer?.Update(element);
                }
            }
        }
    }
}