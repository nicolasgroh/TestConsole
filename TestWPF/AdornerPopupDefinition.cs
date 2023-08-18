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
                if (instance._adornerPopup != null)
                {
                    var adornerLayer = AdornerLayer.GetAdornerLayer(oldValue);

                    if (adornerLayer != null) adornerLayer.Remove(instance._adornerPopup);
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

        private AdornerPopup _adornerPopup;
        public AdornerPopup AdornerPopup
        {
            get { return _adornerPopup; }
        }

        protected void ClearPopupAdorner()
        {
            if (_adornerPopup == null) return;

            _adornerPopup.Child = null;

            BindingOperations.ClearBinding(_adornerPopup, AdornerPopup.PlacementModeProperty);
            BindingOperations.ClearBinding(_adornerPopup, AdornerPopup.CenterOnPlacementTargetProperty);
            BindingOperations.ClearBinding(_adornerPopup, AdornerPopup.UseDynamicPlacementProperty);
            BindingOperations.ClearBinding(_adornerPopup, AdornerPopup.KeepWithinViewProperty);
            BindingOperations.ClearBinding(_adornerPopup, AdornerPopup.VerticalOffsetProperty);
            BindingOperations.ClearBinding(_adornerPopup, AdornerPopup.HorizontalOffsetProperty);
            BindingOperations.ClearBinding(_adornerPopup, WidthProperty);
            BindingOperations.ClearBinding(_adornerPopup, MinWidthProperty);
            BindingOperations.ClearBinding(_adornerPopup, MaxWidthProperty);
            BindingOperations.ClearBinding(_adornerPopup, HeightProperty);
            BindingOperations.ClearBinding(_adornerPopup, MinHeightProperty);
            BindingOperations.ClearBinding(_adornerPopup, MaxHeightProperty);

            _adornerPopup = null;
        }

        protected void CreatePopupAdorner()
        {
            if (PlacementTarget == null)
            {
                ClearPopupAdorner();
                return;
            }

            _adornerPopup = new AdornerPopup(PlacementTarget);

            _adornerPopup.SetBinding(AdornerPopup.PlacementModeProperty, new Binding(nameof(PlacementMode)) { Source = this });
            _adornerPopup.SetBinding(AdornerPopup.CenterOnPlacementTargetProperty, new Binding(nameof(CenterOnPlacementTarget)) { Source = this });
            _adornerPopup.SetBinding(AdornerPopup.UseDynamicPlacementProperty, new Binding(nameof(UseDynamicPlacement)) { Source = this });
            _adornerPopup.SetBinding(AdornerPopup.KeepWithinViewProperty, new Binding(nameof(KeepWithinViewport)) { Source = this });
            _adornerPopup.SetBinding(AdornerPopup.VerticalOffsetProperty, new Binding(nameof(VerticalOffset)) { Source = this });
            _adornerPopup.SetBinding(AdornerPopup.HorizontalOffsetProperty, new Binding(nameof(HorizontalOffset)) { Source = this });
            _adornerPopup.SetBinding(WidthProperty, new Binding(nameof(Width)) { Source = this });
            _adornerPopup.SetBinding(MinWidthProperty, new Binding(nameof(MinWidth)) { Source = this });
            _adornerPopup.SetBinding(MaxWidthProperty, new Binding(nameof(MaxWidth)) { Source = this });
            _adornerPopup.SetBinding(HeightProperty, new Binding(nameof(Height)) { Source = this });
            _adornerPopup.SetBinding(MinHeightProperty, new Binding(nameof(MinHeight)) { Source = this });
            _adornerPopup.SetBinding(MaxHeightProperty, new Binding(nameof(Height)) { Source = this });

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
            if (_adornerPopup == null) return;

            _adornerPopup.Child = Child;
        }

        protected void UpdatePopupVisibility()
        {
            if (_adornerPopup == null) return;

            var placementTarget = PlacementTarget;

            if (placementTarget == null) return;

            var adornerLayer = AdornerLayer.GetAdornerLayer(placementTarget);

            if (IsOpen)
            {
                var placementTargetAdorners = adornerLayer.GetAdorners(placementTarget);

                if (placementTargetAdorners != null && placementTargetAdorners.Contains(_adornerPopup)) return;

                adornerLayer.Add(_adornerPopup);
            }
            else adornerLayer.Remove(_adornerPopup);
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