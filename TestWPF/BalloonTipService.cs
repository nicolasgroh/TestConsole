using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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

                    if (_index < count)
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
                return new AdornersEnumerator(_adornerLayer);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        public interface IBalloonTipConfiguration
        {
            void ApplyConfiguration(BalloonTip balloonTip);
        }

        public class BalloonTipConfiguration : IBalloonTipConfiguration
        {
            private Action<BalloonTip> _setContent;
            private Action<BalloonTip> _setContentTemplate;
            private Action<BalloonTip> _setContentTemplateSelector;
            private Action<BalloonTip> _setHeader;
            private Action<BalloonTip> _setHeaderTemplate;
            private Action<BalloonTip> _setHeaderTemplateSelector;
            private Action<BalloonTip> _setShowDuration;
            private Action<BalloonTip> _setPlacementMode;
            private Action<BalloonTip> _setHorizontalOffset;
            private Action<BalloonTip> _setVerticalOffset;
            private Action<BalloonTip> _setUseDynamicPlacement;
            private Action<BalloonTip> _setKeepWithinView;
            private Action<BalloonTip> _setStyle;

            public BalloonTipConfiguration SetContent(object content)
            {
                _setContent = (balloonTip) => balloonTip.Content = content;
                return this;
            }

            public BalloonTipConfiguration SetContentTemplate(DataTemplate contentTemplate)
            {
                _setContentTemplate = (balloonTip) => balloonTip.ContentTemplate = contentTemplate;
                return this;
            }

            public BalloonTipConfiguration SetContentTemplateSelector(DataTemplateSelector contentTemplateSelector)
            {
                _setContentTemplateSelector = (balloonTip) => balloonTip.ContentTemplateSelector = contentTemplateSelector;
                return this;
            }

            public BalloonTipConfiguration SetHeader(object header)
            {
                _setHeader = (balloonTip) => balloonTip.Header = header;
                return this;
            }

            public BalloonTipConfiguration SetHeaderTemplate(DataTemplate headerTemplate)
            {
                _setHeaderTemplate = (balloonTip) => balloonTip.HeaderTemplate = headerTemplate;
                return this;
            }

            public BalloonTipConfiguration SetHeaderTemplateSelector(DataTemplateSelector headerTemplateSelector)
            {
                _setHeaderTemplateSelector = (balloonTip) => balloonTip.HeaderTemplateSelector = headerTemplateSelector;
                return this;
            }

            public BalloonTipConfiguration SetShowDuration(Duration showDuration)
            {
                _setShowDuration = (balloonTip) => balloonTip.ShowDuration = showDuration;
                return this;
            }

            public BalloonTipConfiguration SetPlacementMode(PopupAdornerPlacementMode placementMode)
            {
                _setPlacementMode = (popupAdorner) => popupAdorner.PlacementMode = placementMode;
                return this;
            }

            public BalloonTipConfiguration SetHorizontalOffset(double horizontalOffset)
            {
                _setHorizontalOffset = (balloonTip) => balloonTip.HorizontalOffset = horizontalOffset;
                return this;
            }

            public BalloonTipConfiguration SetVerticalOffset(double verticalOffset)
            {
                _setVerticalOffset = (balloonTip) => balloonTip.VerticalOffset = verticalOffset;
                return this;
            }

            public BalloonTipConfiguration SetUseDynamicPlacement(bool useDynamicPlacement)
            {
                _setUseDynamicPlacement = (balloonTip) => balloonTip.UseDynamicPlacement = useDynamicPlacement;
                return this;
            }

            public BalloonTipConfiguration SetKeepWithinView(bool keepWithinView)
            {
                _setKeepWithinView = (balloonTip) => balloonTip.KeepWithinView = keepWithinView;
                return this;
            }

            public BalloonTipConfiguration SetStyle(Style style)
            {
                _setStyle = (balloonTip) => balloonTip.Style = style;
                return this;
            }

            public void ApplyConfiguration(BalloonTip balloonTip)
            {
                _setStyle?.Invoke(balloonTip);
                _setContent?.Invoke(balloonTip);
                _setContentTemplate?.Invoke(balloonTip);
                _setContentTemplateSelector?.Invoke(balloonTip);
                _setHeader?.Invoke(balloonTip);
                _setHeaderTemplate?.Invoke(balloonTip);
                _setHeaderTemplateSelector?.Invoke(balloonTip);
                _setShowDuration?.Invoke(balloonTip);
                _setPlacementMode?.Invoke(balloonTip);
                _setHorizontalOffset?.Invoke(balloonTip);
                _setVerticalOffset?.Invoke(balloonTip);
                _setUseDynamicPlacement?.Invoke(balloonTip);
                _setKeepWithinView?.Invoke(balloonTip);
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

        public static void ShowBalloonTip(this UIElement placementTarget, IBalloonTipConfiguration configuration)
        {
            var balloonTip = new BalloonTip();

            ApplyPlacementTargetValues(placementTarget, balloonTip);

            configuration.ApplyConfiguration(balloonTip);

            var popupAdorner = CreateBalloonTipPopupAdorner(balloonTip, placementTarget);

            popupAdorner.ComputedPlacementModeChanged += PopupAdorner_ComputedPlacementModeChanged;

            if (!TryShowBalloonTipPopupAdorner(popupAdorner, placementTarget, balloonTip.ShowDuration))
            {
                placementTarget.Dispatcher.BeginInvoke(new Action(() => TryShowBalloonTipPopupAdorner(popupAdorner, placementTarget, balloonTip.ShowDuration)));
            }
        }

        public static void CloseBalloonTip(BalloonTip balloonTip)
        {
            var popupAdorner = balloonTip.Parent as PopupAdorner;

            if (popupAdorner == null) return;

            popupAdorner.ComputedPlacementModeChanged -= PopupAdorner_ComputedPlacementModeChanged;

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

        private static PopupAdorner CreateBalloonTipPopupAdorner(BalloonTip balloonTip, UIElement placementTarget)
        {
            var popupAdorner = new PopupAdorner(placementTarget)
            {
                Child = balloonTip
            };

            popupAdorner.SetBinding(PopupAdorner.PlacementModeProperty, new System.Windows.Data.Binding
            {
                Path = new PropertyPath(BalloonTip.PlacementModeProperty),
                Source = balloonTip
            });

            popupAdorner.SetBinding(PopupAdorner.HorizontalOffsetProperty, new System.Windows.Data.Binding
            {
                Path = new PropertyPath(BalloonTip.HorizontalOffsetProperty),
                Source = balloonTip
            });

            popupAdorner.SetBinding(PopupAdorner.VerticalOffsetProperty, new System.Windows.Data.Binding
            {
                Path = new PropertyPath(BalloonTip.VerticalOffsetProperty),
                Source = balloonTip
            });

            popupAdorner.SetBinding(PopupAdorner.UseDynamicPlacementProperty, new System.Windows.Data.Binding
            {
                Path = new PropertyPath(BalloonTip.UseDynamicPlacementProperty),
                Source = balloonTip
            });

            popupAdorner.SetBinding(PopupAdorner.KeepWithinViewProperty, new System.Windows.Data.Binding
            {
                Path = new PropertyPath(BalloonTip.KeepWithinViewProperty),
                Source = balloonTip
            });

            return popupAdorner;
        }

        private static void PopupAdorner_ComputedPlacementModeChanged(PopupAdorner sender, GenericPropertyChangedEventArgs<PopupAdornerPlacementMode> eventArgs)
        {
            var balloonTip = (BalloonTip)sender.Child;

            balloonTip.SetComputedPlacementMode(eventArgs.NewValue);
        }

        private static void ApplyPlacementTargetValues(UIElement placementTarget, BalloonTip balloonTip)
        {
            var placementTargetBalloonTipStyle = placementTarget.ReadLocalValue(StyleProperty);

            if (placementTargetBalloonTipStyle != DependencyProperty.UnsetValue) balloonTip.Style = (Style)placementTargetBalloonTipStyle;

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

            var balloonTip = (BalloonTip)popupAdorner.Child;

            CloseBalloonTip(balloonTip);
        }
    }
}