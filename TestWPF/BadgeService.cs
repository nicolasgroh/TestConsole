using Microsoft.IdentityModel.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace TestWPF
{
    public static class BadgeService
    {
        private class BadgeAdorner : DecoratorAdorner
        {
            public BadgeAdorner(UIElement adornedElement, BadgeCollection badges) : base(adornedElement)
            {
                HookupBadges(badges);
            }

            BadgeCollection _badges;
            public BadgeCollection Badges
            {
                get { return _badges; }
            }

            public void HookupBadges(BadgeCollection badges)
            {
                _badges = badges;

                Child = badges.GetItemsHost();

                _badges.ItemsHostChanged += Badges_ItemsHostChanged;
            }

            public void UnhookBadges()
            {
                Child = null;
                _badges.ItemsHostChanged -= Badges_ItemsHostChanged;
                _badges = null;
            }

            private void Badges_ItemsHostChanged(BadgeCollection sender, GenericPropertyChangedEventArgs<Panel> e)
            {
                Child = e.NewValue;
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

        public static readonly DependencyProperty BadgesProperty = DependencyProperty.RegisterAttached("Badges", typeof(BadgeCollection), typeof(BadgeCollection), new FrameworkPropertyMetadata(new StackingBadgeCollection(), BadgesPropertyChanged));

        private static void BadgesPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element)
            {
                if (e.OldValue != null)
                {
                    var oldValue = (BadgeCollection)e.OldValue;

                    oldValue.UnhookPlacementTarget();
                }

                if (e.NewValue != null)
                {
                    var newValue = (BadgeCollection)e.NewValue;

                    newValue.HookupPlacementTarget(element);
                }

                UpdateElementBadges(element);
            }
        }

        public static BadgeCollection GetBadges(DependencyObject obj)
        {
            return (BadgeCollection)obj.GetValue(BadgesProperty);
        }

        public static void SetBadges(DependencyObject obj, BadgeCollection value)
        {
            obj.SetValue(BadgesProperty, value);
        }

        public static readonly DependencyProperty HorizontalOffsetProperty = DependencyProperty.RegisterAttached("HorizontalOffset", typeof(double), typeof(BadgeService), new FrameworkPropertyMetadata(1d, OffsetPropertyChanged, CoerceOffsetProperty));

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
            if (d is UIElement element) UpdateElementBadges(element);
        }

        private static object CoerceOffsetProperty(DependencyObject d, object baseValue)
        {
            return CoerceOffset((double)baseValue);
        }

        private static double CoerceOffset(double offset)
        {
            if (offset > 1d) return 1d;
            if (offset < 0d) return 0;

            return offset;
        }

        private static void InitilizeBadgeAdorner(UIElement element, BadgeCollection badges)
        {
            var adornerLayer = AdornerLayer.GetAdornerLayer(element);

            if (adornerLayer == null) HookupDispatcherLoaded(element, badges);
            else CreateBadgeAdorner(element, badges, adornerLayer);
        }

        private static void HookupDispatcherLoaded(UIElement element, BadgeCollection badges)
        {
            element.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (!TryGetBadgeAdorner(element, out var badgeAdorner, out var adornerLayer))
                {
                    if (adornerLayer != null) CreateBadgeAdorner(element, badges, adornerLayer);
                }

            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private static void CreateBadgeAdorner(UIElement element, BadgeCollection badges, AdornerLayer adornerLayer)
        {
            var badgeAdorner = new BadgeAdorner(element, badges);

            adornerLayer.Add(badgeAdorner);
        }

        private static bool TryGetBadgeAdorner(UIElement element, out BadgeAdorner badgeAdorner, out AdornerLayer adornerLayer)
        {
            badgeAdorner = null;

            adornerLayer = AdornerLayer.GetAdornerLayer(element);

            if (adornerLayer != null)
            {
                var adorners = adornerLayer.GetAdorners(element);

                if (adorners != null && adorners.Length > 0)
                {
                    badgeAdorner = adorners.OfType<BadgeAdorner>().FirstOrDefault();

                    return badgeAdorner != null;
                }
            }

            return false;
        }

        private static BadgeCollection GetBadges(UIElement element, out bool isDefault)
        {
            var valueSource = DependencyPropertyHelper.GetValueSource(element, BadgesProperty);

            isDefault = valueSource.BaseValueSource == BaseValueSource.Default;

            return GetBadges(element);
        }

        internal static void UpdateElementBadges(UIElement element)
        {
            var badges = GetBadges(element, out var isDefault);

            if (isDefault) badges.HookupPlacementTarget(element);

            var hasBadges = badges != null && badges.Count > 0;

            if (TryGetBadgeAdorner(element, out var badgeAdorner, out var adornerLayer))
            {
                if (hasBadges)
                {
                    if (badgeAdorner.Badges == badges)
                    {
                        adornerLayer.Update(element);
                    }
                    else
                    {
                        badgeAdorner.UnhookBadges();
                        badgeAdorner.HookupBadges(badges);
                        adornerLayer.Update(element);
                    }
                }
                else
                {
                    badgeAdorner.UnhookBadges();
                    adornerLayer.Remove(badgeAdorner);
                }
            }
            else
            {
                if (hasBadges) InitilizeBadgeAdorner(element, badges);
            }
        }
    }
}