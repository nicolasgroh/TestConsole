using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Threading;

namespace TestWPF
{
    public static class BalloonTipService
    {
        #region AttachedProperties
        public static readonly DependencyProperty ShowDurationProperty = DependencyProperty.RegisterAttached("ShowDuration", typeof(int), typeof(BalloonTipService));

        public static int GetShowDuration(DependencyObject obj)
        {
            return (int)obj.GetValue(ShowDurationProperty);
        }

        public static void SetShowDuration(DependencyObject obj, int value)
        {
            obj.SetValue(ShowDurationProperty, value);
        }

        public static readonly DependencyProperty PlacementModeProperty = DependencyProperty.RegisterAttached("PlacementMode", typeof(PopupAdornerPlacementMode), typeof(BalloonTipService));

        public static PopupAdornerPlacementMode GetPlacementMode(DependencyObject obj)
        {
            return (PopupAdornerPlacementMode)obj.GetValue(PlacementModeProperty);
        }

        public static void SetPlacementMode(DependencyObject obj, PopupAdornerPlacementMode value)
        {
            obj.SetValue(PlacementModeProperty, value);
        }

        public static readonly DependencyProperty HorizontalOffsetProperty = DependencyProperty.RegisterAttached("HorizontalOffset", typeof(double), typeof(BalloonTipService));

        public static double GetHorizontalOffset(DependencyObject obj)
        {
            return (double)obj.GetValue(HorizontalOffsetProperty);
        }

        public static void SetHorizontalOffset(DependencyObject obj, double value)
        {
            obj.SetValue(HorizontalOffsetProperty, value);
        }

        public static readonly DependencyProperty VerticalOffsetProperty = DependencyProperty.RegisterAttached("VerticalOffset", typeof(double), typeof(BalloonTipService));

        public static double GetVerticalOffset(DependencyObject obj)
        {
            return (double)obj.GetValue(VerticalOffsetProperty);
        }

        public static void SetVerticalOffset(DependencyObject obj, double value)
        {
            obj.SetValue(VerticalOffsetProperty, value);
        }

        public static readonly DependencyProperty UseDynamicPlacementProperty = DependencyProperty.RegisterAttached("UseDynamicPlacement", typeof(bool), typeof(BalloonTipService));

        public static bool GetUseDynamicPlacement(DependencyObject obj)
        {
            return (bool)obj.GetValue(UseDynamicPlacementProperty);
        }

        public static void SetUseDynamicPlacement(DependencyObject obj, bool value)
        {
            obj.SetValue(UseDynamicPlacementProperty, value);
        }

        public static readonly DependencyProperty KeepWithinViewProperty = DependencyProperty.RegisterAttached("KeepWithinView", typeof(bool), typeof(BalloonTipService));

        public static bool GetKeepWithinView(DependencyObject obj)
        {
            return (bool)obj.GetValue(KeepWithinViewProperty);
        }

        public static void SetKeepWithinView(DependencyObject obj, bool value)
        {
            obj.SetValue(KeepWithinViewProperty, value);
        }

        public static readonly DependencyProperty StyleProperty = DependencyProperty.RegisterAttached("Style", typeof(Style), typeof(BalloonTipService));

        public static Style GetStyle(DependencyObject obj)
        {
            return (Style)obj.GetValue(StyleProperty);
        }

        public static void SetStyle(DependencyObject obj, Style value)
        {
            obj.SetValue(StyleProperty, value);
        }
        #endregion

        public static void ShowBalloonTip(this UIElement placementTarget, object content, object header = null, int? showDuration = null, PopupAdornerPlacementMode? placementMode = null, double? horizontalOffset = null, double? verticalOffset = null, bool? useDynamicPlacement = null, bool? keepWithinView = null, Style style = null)
        {
            var apllyValues = new Action<BalloonTip>((balloonTip) =>
            {
                balloonTip.Content = content;
                balloonTip.Header = header;
                if (showDuration != null) balloonTip.ShowDuration = showDuration.Value;
                if (placementMode != null) balloonTip.PlacementMode = placementMode.Value;
                if (horizontalOffset != null) balloonTip.HorizontalOffset = horizontalOffset.Value;
                if (verticalOffset != null) balloonTip.VerticalOffset = verticalOffset.Value;
                if (useDynamicPlacement != null) balloonTip.UseDynamicPlacement = useDynamicPlacement.Value;
                if (keepWithinView != null) balloonTip.KeepWithinView = keepWithinView.Value;
            });

            ShowBalloonTip(placementTarget, apllyValues, style);
        }

        private static void ShowBalloonTip(UIElement placementTarget, Action<BalloonTip> applyValues, Style style = null)
        {
            var balloonTip = new BalloonTip();

            var placementTargetBalloonTipStyle = placementTarget.ReadLocalValue(StyleProperty);

            if (style != null) balloonTip.Style = style;
            else if (placementTargetBalloonTipStyle != DependencyProperty.UnsetValue) balloonTip.Style = (Style)placementTargetBalloonTipStyle;

            ApplyPlacementTargetValues(placementTarget, balloonTip);

            applyValues?.Invoke(balloonTip);

            var popupAdorner = new PopupAdorner(placementTarget)
            {
                PlacementMode = balloonTip.PlacementMode,
                HorizontalOffset = balloonTip.HorizontalOffset,
                VerticalOffset = balloonTip.VerticalOffset,
                UseDynamicPlacement = balloonTip.UseDynamicPlacement,
                KeepWithinView = balloonTip.KeepWithinView,
                Child = balloonTip
            };

            popupAdorner.ComputedPlacementModeChanged += PopupAdorner_ComputedPlacementModeChanged;

            if (!TryShowBalloonTipPopupAdorner(popupAdorner, placementTarget, balloonTip.ShowDuration))
            {
                placementTarget.Dispatcher.BeginInvoke(new Action(() => TryShowBalloonTipPopupAdorner(popupAdorner, placementTarget, balloonTip.ShowDuration)));
            }
        }

        private static void PopupAdorner_ComputedPlacementModeChanged(PopupAdorner sender, GenericPropertyChangedEventArgs<PopupAdornerPlacementMode> eventArgs)
        {
            var balloonTip = (BalloonTip)sender.Child;

            balloonTip.SetComputedPlacementMode(eventArgs.NewValue);
        }

        private static void ApplyPlacementTargetValues(UIElement placementTarget, BalloonTip balloonTip)
        {
            var showDuration = placementTarget.ReadLocalValue(ShowDurationProperty);

            if (showDuration != DependencyProperty.UnsetValue) balloonTip.ShowDuration = (int)showDuration;

            var placementMode = placementTarget.ReadLocalValue(PlacementModeProperty);

            if (placementMode != DependencyProperty.UnsetValue) balloonTip.PlacementMode = (PopupAdornerPlacementMode)placementMode;

            var horizontalOffset = placementTarget.ReadLocalValue(HorizontalOffsetProperty);

            if (horizontalOffset != DependencyProperty.UnsetValue) balloonTip.HorizontalOffset = (double)horizontalOffset;

            var verticalOffsetProperty = placementTarget.ReadLocalValue(VerticalOffsetProperty);

            if (verticalOffsetProperty != DependencyProperty.UnsetValue) balloonTip.VerticalOffset = (double)verticalOffsetProperty;

            var useDynamicPlacementProperty = placementTarget.ReadLocalValue(UseDynamicPlacementProperty);

            if (useDynamicPlacementProperty != DependencyProperty.UnsetValue) balloonTip.UseDynamicPlacement = (bool)useDynamicPlacementProperty;

            var keepWithinViewProperty = placementTarget.ReadLocalValue(KeepWithinViewProperty);

            if (keepWithinViewProperty != DependencyProperty.UnsetValue) balloonTip.KeepWithinView = (bool)keepWithinViewProperty;
        }

        private static bool TryShowBalloonTipPopupAdorner(PopupAdorner popupAdorner, UIElement placementTarget, int showDuration)
        {
            var adornerLayer = AdornerLayer.GetAdornerLayer(placementTarget);

            if (adornerLayer == null) return false;

            adornerLayer.Add(popupAdorner);

            var timer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromMilliseconds(showDuration),
                Tag = popupAdorner
            };

            timer.Tick += BalloonTipTimer_Tick;

            timer.Start();

            return true;
        }

        private static void BalloonTipTimer_Tick(object sender, EventArgs e)
        {
            var timer = (DispatcherTimer)sender;

            timer.Tick -= BalloonTipTimer_Tick;

            timer.Stop();

            var popupAdorner = (PopupAdorner)timer.Tag;

            popupAdorner.ComputedPlacementModeChanged -= PopupAdorner_ComputedPlacementModeChanged;

            var placementTarget = popupAdorner.AdornedElement;

            var adornerLayer = AdornerLayer.GetAdornerLayer(placementTarget);

            if (adornerLayer != null)
            {
                var placementTargetAdorners = adornerLayer.GetAdorners(placementTarget);

                if (placementTargetAdorners.Contains(popupAdorner)) adornerLayer.Remove(popupAdorner);
            }
        }
    }
}