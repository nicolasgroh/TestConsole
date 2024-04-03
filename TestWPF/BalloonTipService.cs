using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;

namespace TestWPF
{
    public static class BalloonTipService
    {
        private class AdornerLayerAdornersIterator : IEnumerable<Adorner>
        {
            private struct AdornersEnumerator : IEnumerator<Adorner>
            {
                public AdornersEnumerator(AdornerLayer adornerLayer)
                {
                    _adornerLayer = adornerLayer;
                    _index = 1;
                    _current = null;
                }

                private AdornerLayer _adornerLayer;
                private int _index;
                private Adorner _current;

                public Adorner Current { get { return _current; } }

                object IEnumerator.Current { get { return _current; } }

                public bool MoveNext()
                {
                    var count = VisualTreeHelper.GetChildrenCount(_adornerLayer);

                    if (_index < count - 1)
                    {
                        var child = VisualTreeHelper.GetChild(_adornerLayer, _index);

                        if (child is Adorner adorner)
                        {
                            _current = adorner;
                            _index++;
                            return true;
                        }

                        return false;
                    }

                    Reset();
                    return false;
                }

                public void Reset()
                {
                    _index = 1;
                    _current = null;
                }

                public void Dispose()
                {
                    _adornerLayer = null;
                    _current = null;
                }
            }

            public AdornerLayerAdornersIterator(AdornerLayer adornerLayer)
            {
                _adornerLayer = adornerLayer;
            }

            private AdornerLayer _adornerLayer;

            public IEnumerator<Adorner> GetEnumerator()
            {
                throw new NotImplementedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                throw new NotImplementedException();
            }
        }

        public interface IBalloonTipConfiguration
        {
            public object Content { get; }

            public object Header { get; }

            public Duration? ShowDuration { get; }

            public PopupAdornerPlacementMode? PlacementMode { get; }

            public double? HorizontalOffset { get; }

            public double? VerticalOffset { get; }

            public bool? UseDynamicPlacement { get; }

            public bool? KeepWithinView { get; }

            public Style Style { get; }
        }

        public class BalloonTipConfiguration
        {
            private object _content;
            public object Content => _content;

            private object _header;
            public object Header => _header;

            private Duration? _showDuration;
            public Duration? ShowDuration => _showDuration;

            private PopupAdornerPlacementMode? _placementMode;
            public PopupAdornerPlacementMode? PlacementMode => _placementMode;

            private double? _horizontalOffset;
            public double? HorizontalOffset => _horizontalOffset;

            private double? _verticalOffset;
            public double? VerticalOffset => _verticalOffset;

            private bool? _useDynamicPlacement;
            public bool? UseDynamicPlacement => _useDynamicPlacement;

            private bool? _keepWithinView;
            public bool? KeepWithinView => _keepWithinView;

            private Style _Style;
            public Style Style => _Style;

            public BalloonTipConfiguration SetContent(object content)
            {
                _content = content;
                return this;
            }

            public BalloonTipConfiguration SetHeader(object header)
            {
                _header = header;
                return this;
            }

            public BalloonTipConfiguration SetShowDuration(Duration showDuration)
            {
                _showDuration = showDuration;
                return this;
            }

            public BalloonTipConfiguration SetPlacementMode(PopupAdornerPlacementMode placementMode)
            {
                _placementMode = placementMode;
                return this;
            }

            public BalloonTipConfiguration SetHorizontalOffset(double horizontalOffset)
            {
                _horizontalOffset = horizontalOffset;
                return this;
            }

            public BalloonTipConfiguration SetVerticalOffset(double verticalOffset)
            {
                _verticalOffset = verticalOffset;
                return this;
            }

            public BalloonTipConfiguration SetUseDynamicPlacement(bool useDynamicPlacement)
            {
                _useDynamicPlacement = useDynamicPlacement;
                return this;
            }

            public BalloonTipConfiguration SetKeepWithinView(bool keepWithinView)
            {
                _keepWithinView = keepWithinView;
                return this;
            }

            public BalloonTipConfiguration SetStyle(Style style)
            {
                _Style = style;
                return this;
            }
        }

        #region AttachedProperties
        public static readonly DependencyProperty ShowDurationProperty = DependencyProperty.RegisterAttached("ShowDuration", typeof(Duration), typeof(BalloonTipService));

        public static Duration GetShowDuration(DependencyObject obj)
        {
            return (Duration)obj.GetValue(ShowDurationProperty);
        }

        public static void SetShowDuration(DependencyObject obj, Duration value)
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

        public static List<BalloonTip> GetBalloonTips(UIElement placementTarget)
        {
            if (placementTarget == null) throw new ArgumentNullException(nameof(placementTarget));

            var adornerLayer = AdornerLayer.GetAdornerLayer(placementTarget);

            if (adornerLayer == null) return new List<BalloonTip>();

            var adorners = adornerLayer.GetAdorners(placementTarget);

            if (adorners == null || adorners.Length == 0) return new List<BalloonTip>();

            return GetBalloonTipsFromAdorners(adorners).ToList();
        }

        public static List<BalloonTip> GetAllBalloonTips(AdornerLayer adornerLayer)
        {
            if (adornerLayer == null) throw new ArgumentNullException(nameof(adornerLayer));

            var childrenCount = VisualTreeHelper.GetChildrenCount(adornerLayer);

            // Wenn wir nicht mehr als einen Child haben, gibt es auch keine Adorner
            if (childrenCount < 2) return new List<BalloonTip>();

            return GetBalloonTipsFromAdorners(new AdornerLayerAdornersIterator(adornerLayer)).ToList();
        }

        public static BalloonTipConfiguration ConfigureBalloonTip()
        {
            return new BalloonTipConfiguration();
        }

        public static void ShowBalloonTip(this UIElement placementTarget, BalloonTipConfiguration configuration)
        {
            var apllyValues = new Action<BalloonTip>((balloonTip) =>
            {
                balloonTip.Content = configuration.Content;
                balloonTip.Header = configuration.Header;
                if (configuration.ShowDuration != null) balloonTip.ShowDuration = configuration.ShowDuration.Value;
                if (configuration.PlacementMode != null) balloonTip.PlacementMode = configuration.PlacementMode.Value;
                if (configuration.HorizontalOffset != null) balloonTip.HorizontalOffset = configuration.HorizontalOffset.Value;
                if (configuration.VerticalOffset != null) balloonTip.VerticalOffset = configuration.VerticalOffset.Value;
                if (configuration.UseDynamicPlacement != null) balloonTip.UseDynamicPlacement = configuration.UseDynamicPlacement.Value;
                if (configuration.KeepWithinView != null) balloonTip.KeepWithinView = configuration.KeepWithinView.Value;
            });

            ShowBalloonTip(placementTarget, apllyValues, configuration.Style);
        }

        public static void ShowBalloonTip(this UIElement placementTarget, object content, object header = null, Duration? showDuration = null, PopupAdornerPlacementMode? placementMode = null, double? horizontalOffset = null, double? verticalOffset = null, bool? useDynamicPlacement = null, bool? keepWithinView = null, Style style = null)
        {
            var config = ConfigureBalloonTip()
                .SetContent(content)
                .SetHeader(header);

            if (showDuration != null) config.SetShowDuration(showDuration.Value);
            if (placementMode != null) config.SetPlacementMode(placementMode.Value);
            if (horizontalOffset != null) config.SetHorizontalOffset(horizontalOffset.Value);
            if (verticalOffset != null) config.SetVerticalOffset(verticalOffset.Value);
            if (useDynamicPlacement != null) config.SetUseDynamicPlacement(useDynamicPlacement.Value);
            if (keepWithinView != null) config.SetKeepWithinView(keepWithinView.Value);
            if (style != null) config.SetStyle(style);

            ShowBalloonTip(placementTarget, config);
        }

        public static void CloseBalloonTip(BalloonTip balloonTip)
        {
            var popupAdorner = balloonTip.Parent as PopupAdorner;

            if (popupAdorner == null) return;

            var placementTarget = popupAdorner.AdornedElement;

            if (placementTarget == null) return;

            var adornerLayer = AdornerLayer.GetAdornerLayer(placementTarget);

            if (adornerLayer == null) return;

            var adorners = adornerLayer.GetAdorners(placementTarget);

            if (adorners != null && adorners.Length > 0 && adorners.Contains(popupAdorner)) adornerLayer.Remove(popupAdorner);
        }

        private static IEnumerable<BalloonTip> GetBalloonTipsFromAdorners(IEnumerable<Adorner> adorners)
        {
            return adorners.OfType<PopupAdorner>().Where(x => x.Child is BalloonTip).Select(x => x.Child as BalloonTip);
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

            if (showDuration != DependencyProperty.UnsetValue) balloonTip.ShowDuration = (Duration)showDuration;

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

        private static bool TryShowBalloonTipPopupAdorner(PopupAdorner popupAdorner, UIElement placementTarget, Duration showDuration)
        {
            var adornerLayer = AdornerLayer.GetAdornerLayer(placementTarget);

            if (adornerLayer == null) return false;

            adornerLayer.Add(popupAdorner);

            if (showDuration.HasTimeSpan)
            {
                var timer = new DispatcherTimer()
                {
                    Interval = showDuration.TimeSpan,
                    Tag = popupAdorner
                };

                timer.Tick += BalloonTipTimer_Tick;

                timer.Start();
            }

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