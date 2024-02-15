using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;

namespace TestWPF
{
    [ContentProperty("Child")]
    public class PopupAdornerDefinition : FrameworkElement
    {
        #region DependancyProperties
        public static readonly DependencyProperty PlacementTargetProperty = DependencyProperty.Register("PlacementTarget", typeof(UIElement), typeof(PopupAdornerDefinition), new FrameworkPropertyMetadata(null, PlacementTargetPropertyChanged));

        private static void PlacementTargetPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var instance = (PopupAdornerDefinition)d;

            if (e.OldValue is UIElement oldValue)
            {
                if (instance._PopupAdorner != null)
                {
                    var adornerLayer = AdornerLayer.GetAdornerLayer(oldValue);

                    if (adornerLayer != null) adornerLayer.Remove(instance._PopupAdorner);
                }
            }

            instance.OnPlacementTargetChanged();
        }

        public UIElement PlacementTarget
        {
            get { return (UIElement)GetValue(PlacementTargetProperty); }
            set { SetValue(PlacementTargetProperty, value); }
        }

        public static readonly DependencyProperty PlacementModeProperty = DependencyProperty.Register("PlacementMode", typeof(PopupAdornerPlacementMode), typeof(PopupAdornerDefinition), new FrameworkPropertyMetadata(PopupAdornerPlacementMode.Bottom));
        public PopupAdornerPlacementMode PlacementMode
        {
            get { return (PopupAdornerPlacementMode)GetValue(PlacementModeProperty); }
            set { SetValue(PlacementModeProperty, value); }
        }

        public static readonly DependencyProperty CenterOnPlacementTargetProperty = DependencyProperty.Register("CenterOnPlacementTarget", typeof(bool), typeof(PopupAdornerDefinition), new FrameworkPropertyMetadata(true));
        public bool CenterOnPlacementTarget
        {
            get { return (bool)GetValue(CenterOnPlacementTargetProperty); }
            set { SetValue(CenterOnPlacementTargetProperty, value); }
        }

        public static readonly DependencyProperty UseDynamicPlacementProperty = DependencyProperty.Register("UseDynamicPlacement", typeof(bool), typeof(PopupAdornerDefinition), new FrameworkPropertyMetadata(true));
        public bool UseDynamicPlacement
        {
            get { return (bool)GetValue(UseDynamicPlacementProperty); }
            set { SetValue(UseDynamicPlacementProperty, value); }
        }

        public static readonly DependencyProperty KeepWithinViewportProperty = DependencyProperty.Register("KeepWithinViewport", typeof(bool), typeof(PopupAdornerDefinition), new FrameworkPropertyMetadata(true));
        public bool KeepWithinViewport
        {
            get { return (bool)GetValue(KeepWithinViewportProperty); }
            set { SetValue(KeepWithinViewportProperty, value); }
        }

        public static readonly DependencyProperty VerticalOffsetProperty = DependencyProperty.Register("VerticalOffset", typeof(double), typeof(PopupAdornerDefinition), new FrameworkPropertyMetadata(0.0));
        public double VerticalOffset
        {
            get { return (double)GetValue(VerticalOffsetProperty); }
            set { SetValue(VerticalOffsetProperty, value); }
        }

        public static readonly DependencyProperty HorizontalOffsetProperty = DependencyProperty.Register("HorizontalOffset", typeof(double), typeof(PopupAdornerDefinition), new FrameworkPropertyMetadata(0.0));
        public double HorizontalOffset
        {
            get { return (double)GetValue(HorizontalOffsetProperty); }
            set { SetValue(HorizontalOffsetProperty, value); }
        }

        public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register("IsOpen", typeof(bool), typeof(PopupAdornerDefinition), new FrameworkPropertyMetadata(false, IsOpenPropertyChanged));

        private static void IsOpenPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((PopupAdornerDefinition)d).OnIsOpenChanged();
        }

        public bool IsOpen
        {
            get { return (bool)GetValue(IsOpenProperty); }
            set { SetValue(IsOpenProperty, value); }
        }
        #endregion

        #region Properties
        UIElement _child;
        public UIElement Child
        {
            get { return _child; }
            set
            {
                _child = value;

                UpdatePopupChild();
            }
        }

        private PopupAdorner _PopupAdorner;
        public PopupAdorner PopupAdorner
        {
            get { return _PopupAdorner; }
        }
        #endregion

        #region PrivateMethods
        private PopupAdorner CreatePopupAdorner(UIElement placementTarget)
        {
            return new PopupAdorner(placementTarget);
        }

        private void ClearPopupAdorner()
        {
            if (_PopupAdorner == null) return;

            _PopupAdorner.Child = null;

            BindingOperations.ClearBinding(_PopupAdorner, PopupAdorner.PlacementModeProperty);
            BindingOperations.ClearBinding(_PopupAdorner, PopupAdorner.CenterOnPlacementTargetProperty);
            BindingOperations.ClearBinding(_PopupAdorner, PopupAdorner.UseDynamicPlacementProperty);
            BindingOperations.ClearBinding(_PopupAdorner, PopupAdorner.KeepWithinViewProperty);
            BindingOperations.ClearBinding(_PopupAdorner, PopupAdorner.VerticalOffsetProperty);
            BindingOperations.ClearBinding(_PopupAdorner, PopupAdorner.HorizontalOffsetProperty);
            BindingOperations.ClearBinding(_PopupAdorner, WidthProperty);
            BindingOperations.ClearBinding(_PopupAdorner, MinWidthProperty);
            BindingOperations.ClearBinding(_PopupAdorner, MaxWidthProperty);
            BindingOperations.ClearBinding(_PopupAdorner, HeightProperty);
            BindingOperations.ClearBinding(_PopupAdorner, MinHeightProperty);
            BindingOperations.ClearBinding(_PopupAdorner, MaxHeightProperty);

            _PopupAdorner = null;
        }

        private void ResetPopupAdorner()
        {
            ClearPopupAdorner();

            _PopupAdorner = CreatePopupAdorner(PlacementTarget);

            if (_PopupAdorner == null) throw new ArgumentNullException(nameof(_PopupAdorner));

            _PopupAdorner.SetBinding(PopupAdorner.PlacementModeProperty, new Binding(nameof(PlacementMode)) { Source = this });
            _PopupAdorner.SetBinding(PopupAdorner.CenterOnPlacementTargetProperty, new Binding(nameof(CenterOnPlacementTarget)) { Source = this });
            _PopupAdorner.SetBinding(PopupAdorner.UseDynamicPlacementProperty, new Binding(nameof(UseDynamicPlacement)) { Source = this });
            _PopupAdorner.SetBinding(PopupAdorner.KeepWithinViewProperty, new Binding(nameof(KeepWithinViewport)) { Source = this });
            _PopupAdorner.SetBinding(PopupAdorner.VerticalOffsetProperty, new Binding(nameof(VerticalOffset)) { Source = this });
            _PopupAdorner.SetBinding(PopupAdorner.HorizontalOffsetProperty, new Binding(nameof(HorizontalOffset)) { Source = this });
            _PopupAdorner.SetBinding(WidthProperty, new Binding(nameof(Width)) { Source = this });
            _PopupAdorner.SetBinding(MinWidthProperty, new Binding(nameof(MinWidth)) { Source = this });
            _PopupAdorner.SetBinding(MaxWidthProperty, new Binding(nameof(MaxWidth)) { Source = this });
            _PopupAdorner.SetBinding(HeightProperty, new Binding(nameof(Height)) { Source = this });
            _PopupAdorner.SetBinding(MinHeightProperty, new Binding(nameof(MinHeight)) { Source = this });
            _PopupAdorner.SetBinding(MaxHeightProperty, new Binding(nameof(Height)) { Source = this });

            UpdatePopupChild();

            UpdatePopupVisibility();
        }

        private void OnPlacementTargetChanged()
        {
            ResetPopupAdorner();
        }

        private void OnIsOpenChanged()
        {
            UpdatePopupVisibility();
        }

        private void UpdatePopupChild()
        {
            if (_PopupAdorner == null) return;

            _PopupAdorner.Child = Child;
        }

        private void UpdatePopupVisibility()
        {
            if (_PopupAdorner == null) return;

            var placementTarget = PlacementTarget;

            if (placementTarget == null) return;

            var adornerLayer = AdornerLayer.GetAdornerLayer(placementTarget);

            if (IsOpen)
            {
                var placementTargetAdorners = adornerLayer.GetAdorners(placementTarget);

                if (placementTargetAdorners != null && placementTargetAdorners.Contains(_PopupAdorner)) return;

                adornerLayer.Add(_PopupAdorner);
            }
            else adornerLayer.Remove(_PopupAdorner);
        }
        #endregion

        #region Overrides
        protected override Size MeasureOverride(Size availableSize)
        {
            return new Size();
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            return new Size();
        }
        #endregion
    }
}