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

        public static readonly DependencyProperty PlacementModeProperty = DependencyProperty.Register("PlacementMode", typeof(PopupAdornerPlacementMode), typeof(AdornerPopupDefinition), new FrameworkPropertyMetadata(PopupAdornerPlacementMode.Bottom));
        public PopupAdornerPlacementMode PlacementMode
        {
            get { return (PopupAdornerPlacementMode)GetValue(PlacementModeProperty); }
            set { SetValue(PlacementModeProperty, value); }
        }

        public static readonly DependencyProperty KeepWithinViewportProperty = DependencyProperty.Register("KeepWithinViewport", typeof(bool), typeof(AdornerPopupDefinition), new FrameworkPropertyMetadata(true));
        public bool KeepWithinViewport
        {
            get { return (bool)GetValue(KeepWithinViewportProperty); }
            set { SetValue(KeepWithinViewportProperty, value); }
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

        private PopupAdornern _popupAdorner;
        public PopupAdornern PopupAdorner
        {
            get { return _popupAdorner; }
        }

        protected void ClearPopupAdorner()
        {
            if (_popupAdorner == null) return;

            _popupAdorner.Child = null;

            BindingOperations.ClearBinding(_popupAdorner, PopupAdornern.PlacementModeProperty);

            _popupAdorner = null;
        }

        protected void CreatePopupAdorner()
        {
            if (PlacementTarget == null)
            {
                ClearPopupAdorner();
                return;
            }

            _popupAdorner = new PopupAdornern(PlacementTarget);

            _popupAdorner.SetBinding(PopupAdornern.PlacementModeProperty, new Binding(nameof(PlacementMode)) { Source = this });

            UpdatePopupChild();

            UpdatePopupVisibility();
        }

        protected virtual void OnPlacementTargetChanged()
        {
            CreatePopupAdorner();
        }

        protected virtual void OnIsOpenChanged()
        {
            if (PopupAdorner == null) return;

            if (PlacementTarget == null) return;

            UpdatePopupVisibility();
        }

        protected void UpdatePopupChild()
        {
            if (PopupAdorner == null) return;

            PopupAdorner.Child = Child;
        }

        protected void UpdatePopupVisibility()
        {
            var adornerLayer = AdornerLayer.GetAdornerLayer(PlacementTarget);

            if (IsOpen) adornerLayer.Add(PopupAdorner);
            else adornerLayer.Remove(PopupAdorner);
        }
    }
}