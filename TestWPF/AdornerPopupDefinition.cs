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
    public class AdornerPopupDefinition : FrameworkElement
    {
        public static readonly DependencyProperty PlacementTargetProperty = DependencyProperty.Register("PlacementTarget", typeof(UIElement), typeof(AdornerPopupDefinition), new FrameworkPropertyMetadata(null, PlacementTargetPropertyChanged));

        private static void PlacementTargetPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var instance = (AdornerPopupDefinition)d;

            if (e.OldValue is UIElement oldValue)
            {
                if (instance._popupAdorner != null)
                {
                    var adornerLayer = AdornerLayer.GetAdornerLayer(oldValue);

                    if (adornerLayer != null) adornerLayer.Remove(instance._popupAdorner);
                }
            }

            instance.ClearPopupAdorner();
            
            instance.OnPlacementTargetChanged();
        }

        public UIElement PlacementTarget
        {
            get { return (UIElement)GetValue(PlacementTargetProperty); }
            set { SetValue(PlacementTargetProperty, value); }
        }

        public static readonly DependencyProperty PlacementModeProperty = DependencyProperty.Register("PlacementMode", typeof(AdornerPopupPlacementMode), typeof(AdornerPopupDefinition), new FrameworkPropertyMetadata(AdornerPopupPlacementMode.Bottom));
        public AdornerPopupPlacementMode PlacementMode
        {
            get { return (AdornerPopupPlacementMode)GetValue(PlacementModeProperty); }
            set { SetValue(PlacementModeProperty, value); }
        }

        public static readonly DependencyProperty CenterOnPlacementTargetProperty = DependencyProperty.Register("CenterOnPlacementTarget", typeof(bool), typeof(AdornerPopupDefinition), new FrameworkPropertyMetadata(true));
        public bool CenterOnPlacementTarget
        {
            get { return (bool)GetValue(CenterOnPlacementTargetProperty); }
            set { SetValue(CenterOnPlacementTargetProperty, value); }
        }

        public static readonly DependencyProperty UseDynamicPlacementProperty = DependencyProperty.Register("UseDynamicPlacement", typeof(bool), typeof(AdornerPopupDefinition), new FrameworkPropertyMetadata(true));
        public bool UseDynamicPlacement
        {
            get { return (bool)GetValue(UseDynamicPlacementProperty); }
            set { SetValue(UseDynamicPlacementProperty, value); }
        }

        public static readonly DependencyProperty KeepWithinViewportProperty = DependencyProperty.Register("KeepWithinViewport", typeof(bool), typeof(AdornerPopupDefinition), new FrameworkPropertyMetadata(true));
        public bool KeepWithinViewport
        {
            get { return (bool)GetValue(KeepWithinViewportProperty); }
            set { SetValue(KeepWithinViewportProperty, value); }
        }

        public static readonly DependencyProperty VerticalOffsetProperty = DependencyProperty.Register("VerticalOffset", typeof(double), typeof(AdornerPopupDefinition), new FrameworkPropertyMetadata(0.0));
        public double VerticalOffset
        {
            get { return (double)GetValue(VerticalOffsetProperty); }
            set { SetValue(VerticalOffsetProperty, value); }
        }

        public static readonly DependencyProperty HorizontalOffsetProperty = DependencyProperty.Register("HorizontalOffset", typeof(double), typeof(AdornerPopupDefinition), new FrameworkPropertyMetadata(0.0));
        public double HorizontalOffset
        {
            get { return (double)GetValue(HorizontalOffsetProperty); }
            set { SetValue(HorizontalOffsetProperty, value); }
        }

        public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register("IsOpen", typeof(bool), typeof(AdornerPopupDefinition), new FrameworkPropertyMetadata(false, IsOpenPropertyChanged));

        private static void IsOpenPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((AdornerPopupDefinition)d).OnIsOpenChanged();
        }

        public bool IsOpen
        {
            get { return (bool)GetValue(IsOpenProperty); }
            set { SetValue(IsOpenProperty, value); }
        }

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

        private AdornerPopup _popupAdorner;
        public AdornerPopup PopupAdorner
        {
            get { return _popupAdorner; }
        }

        protected void ClearPopupAdorner()
        {
            if (_popupAdorner == null) return;

            _popupAdorner.Child = null;

            BindingOperations.ClearBinding(_popupAdorner, AdornerPopup.PlacementModeProperty);
            BindingOperations.ClearBinding(_popupAdorner, AdornerPopup.CenterOnPlacementTargetProperty);
            BindingOperations.ClearBinding(_popupAdorner, AdornerPopup.UseDynamicPlacementProperty);
            BindingOperations.ClearBinding(_popupAdorner, AdornerPopup.KeepWithinViewProperty);
            BindingOperations.ClearBinding(_popupAdorner, AdornerPopup.VerticalOffsetProperty);
            BindingOperations.ClearBinding(_popupAdorner, AdornerPopup.HorizontalOffsetProperty);
            BindingOperations.ClearBinding(_popupAdorner, WidthProperty);
            BindingOperations.ClearBinding(_popupAdorner, MinWidthProperty);
            BindingOperations.ClearBinding(_popupAdorner, MaxWidthProperty);
            BindingOperations.ClearBinding(_popupAdorner, HeightProperty);
            BindingOperations.ClearBinding(_popupAdorner, MinHeightProperty);
            BindingOperations.ClearBinding(_popupAdorner, MaxHeightProperty);

            _popupAdorner = null;
        }

        protected void CreatePopupAdorner()
        {
            if (PlacementTarget == null)
            {
                ClearPopupAdorner();
                return;
            }

            _popupAdorner = new AdornerPopup(PlacementTarget);

            _popupAdorner.SetBinding(AdornerPopup.PlacementModeProperty, new Binding(nameof(PlacementMode)) { Source = this });
            _popupAdorner.SetBinding(AdornerPopup.CenterOnPlacementTargetProperty, new Binding(nameof(CenterOnPlacementTarget)) { Source = this });
            _popupAdorner.SetBinding(AdornerPopup.UseDynamicPlacementProperty, new Binding(nameof(UseDynamicPlacement)) { Source = this });
            _popupAdorner.SetBinding(AdornerPopup.KeepWithinViewProperty, new Binding(nameof(KeepWithinViewport)) { Source = this });
            _popupAdorner.SetBinding(AdornerPopup.VerticalOffsetProperty, new Binding(nameof(VerticalOffset)) { Source = this });
            _popupAdorner.SetBinding(AdornerPopup.HorizontalOffsetProperty, new Binding(nameof(HorizontalOffset)) { Source = this });
            _popupAdorner.SetBinding(WidthProperty, new Binding(nameof(Width)) { Source = this });
            _popupAdorner.SetBinding(MinWidthProperty, new Binding(nameof(MinWidth)) { Source = this });
            _popupAdorner.SetBinding(MaxWidthProperty, new Binding(nameof(MaxWidth)) { Source = this });
            _popupAdorner.SetBinding(HeightProperty, new Binding(nameof(Height)) { Source = this });
            _popupAdorner.SetBinding(MinHeightProperty, new Binding(nameof(MinHeight)) { Source = this });
            _popupAdorner.SetBinding(MaxHeightProperty, new Binding(nameof(Height)) { Source = this });

            UpdatePopupChild();

            UpdatePopupVisibility();
        }

        protected virtual void OnPlacementTargetChanged()
        {
            CreatePopupAdorner();
        }

        protected virtual void OnIsOpenChanged()
        {
            UpdatePopupVisibility();
        }

        protected void UpdatePopupChild()
        {
            if (_popupAdorner == null) return;

            _popupAdorner.Child = Child;
        }

        protected void UpdatePopupVisibility()
        {
            if (_popupAdorner == null) return;

            if (PlacementTarget == null) return;

            var placementTarget = PlacementTarget;

            var adornerLayer = AdornerLayer.GetAdornerLayer(placementTarget);

            if (IsOpen)
            {
                var placementTargetAdorners = adornerLayer.GetAdorners(placementTarget);

                if (placementTargetAdorners != null && placementTargetAdorners.Contains(_popupAdorner)) return;

                adornerLayer.Add(_popupAdorner);
            }
            else adornerLayer.Remove(_popupAdorner);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            return new Size();
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            return new Size();
        }
    }
}