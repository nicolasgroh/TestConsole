﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;

namespace TestWPF
{
    [Flags]
    public enum PopTipTrigger
    {
        None = 0b0000,
        MouseOver = 0b0001,
        Focus = 0b0010,
        MouseOverOrFocus = MouseOver | Focus,
    }

    public static class PopTipService
    {
        #region DependancyProperties
        public static readonly DependencyProperty PopTipProperty = DependencyProperty.RegisterAttached("PopTip", typeof(object), typeof(PopTipService), new FrameworkPropertyMetadata(null, PopTipPropertyChanged));

        private static void PopTipPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement placementTarget)
            {
                UnhookPlacementTarget(placementTarget);

                if (e.NewValue != null)
                {
                    HookupPlacementTargetTrigger(placementTarget, GetTrigger(placementTarget));
                }

                UpdatePopTipVisibility(placementTarget);
            }
        }

        public static object GetPopTip(DependencyObject obj)
        {
            return obj.GetValue(PopTipProperty);
        }

        public static void SetPopTip(DependencyObject obj, object value)
        {
            obj.SetValue(PopTipProperty, value);
        }

        public static readonly DependencyProperty IsOpenProperty = DependencyProperty.RegisterAttached("IsOpen", typeof(bool), typeof(PopTipService), new PropertyMetadata(false, IsOpenPropertyChanged));

        private static void IsOpenPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement placementTarget && HasPopTip(placementTarget))
            {
                UpdatePopTipVisibility(placementTarget);
            }
        }

        public static bool GetIsOpen(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsOpenProperty);
        }

        public static void SetIsOpen(DependencyObject obj, bool value)
        {
            obj.SetValue(IsOpenProperty, value);
        }

        public static readonly DependencyProperty TriggerProperty = DependencyProperty.RegisterAttached("Trigger", typeof(PopTipTrigger), typeof(PopTipService), new FrameworkPropertyMetadata(PopTipTrigger.None, TriggerPropertyChanged));

        private static void TriggerPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement placementTarget && HasPopTip(placementTarget))
            {
                UnhookPlacementTarget(placementTarget);

                HookupPlacementTargetTrigger(placementTarget, (PopTipTrigger)e.NewValue);
            }
        }

        public static PopTipTrigger GetTrigger(DependencyObject obj)
        {
            return (PopTipTrigger)obj.GetValue(TriggerProperty);
        }

        public static void SetTrigger(DependencyObject obj, PopTipTrigger value)
        {
            obj.SetValue(TriggerProperty, value);
        }

        public static readonly DependencyProperty PlacementModeProperty = DependencyProperty.RegisterAttached("PlacementMode", typeof(PopupAdornerPlacementMode), typeof(PopTipService), new FrameworkPropertyMetadata(PopupAdornerPlacementMode.Top));

        public static PopupAdornerPlacementMode GetPlacementMode(DependencyObject obj)
        {
            return (PopupAdornerPlacementMode)obj.GetValue(PlacementModeProperty);
        }

        public static void SetPlacementMode(DependencyObject obj, PopupAdornerPlacementMode value)
        {
            obj.SetValue(PlacementModeProperty, value);
        }

        public static readonly DependencyProperty HorizontalOffsetProperty = DependencyProperty.RegisterAttached("HorizontalOffset", typeof(double), typeof(PopTipService), new FrameworkPropertyMetadata(0d));

        public static double GetHorizontalOffset(DependencyObject obj)
        {
            return (double)obj.GetValue(HorizontalOffsetProperty);
        }

        public static void SetHorizontalOffset(DependencyObject obj, double value)
        {
            obj.SetValue(HorizontalOffsetProperty, value);
        }

        public static readonly DependencyProperty VerticalOffsetProperty = DependencyProperty.RegisterAttached("VerticalOffset", typeof(double), typeof(PopTipService), new FrameworkPropertyMetadata(0d));

        public static double GetVerticalOffset(DependencyObject obj)
        {
            return (double)obj.GetValue(VerticalOffsetProperty);
        }

        public static void SetVerticalOffset(DependencyObject obj, double value)
        {
            obj.SetValue(VerticalOffsetProperty, value);
        }

        public static readonly DependencyProperty UseDynamicPlacementProperty = DependencyProperty.RegisterAttached("UseDynamicPlacement", typeof(bool), typeof(PopTipService), new FrameworkPropertyMetadata(true));

        public static bool GetUseDynamicPlacement(DependencyObject obj)
        {
            return (bool)obj.GetValue(UseDynamicPlacementProperty);
        }

        public static void SetUseDynamicPlacement(DependencyObject obj, bool value)
        {
            obj.SetValue(UseDynamicPlacementProperty, value);
        }

        public static readonly DependencyProperty KeepWithinViewProperty = DependencyProperty.RegisterAttached("KeepWithinView", typeof(bool), typeof(PopTipService), new FrameworkPropertyMetadata(true));

        public static bool GetKeepWithinView(DependencyObject obj)
        {
            return (bool)obj.GetValue(KeepWithinViewProperty);
        }

        public static void SetKeepWithinView(DependencyObject obj, bool value)
        {
            obj.SetValue(KeepWithinViewProperty, value);
        }

        public static readonly DependencyProperty StyleProperty = DependencyProperty.RegisterAttached("Style", typeof(Style), typeof(PopTipService));

        public static Style GetStyle(DependencyObject obj)
        {
            return (Style)obj.GetValue(StyleProperty);
        }

        public static void SetStyle(DependencyObject obj, Style value)
        {
            obj.SetValue(StyleProperty, value);
        }
        #endregion

        private static bool HasPopTip(UIElement placementTarget)
        {
            return GetPopTip(placementTarget) != null;
        }

        private static void HookupPlacementTargetTrigger(UIElement placementTarget, PopTipTrigger trigger)
        {
            if ((trigger & PopTipTrigger.MouseOver) == PopTipTrigger.MouseOver)
            {
                placementTarget.MouseEnter += PlacementTarget_MouseEnter;
                placementTarget.MouseLeave += PlacementTarget_MouseLeave;

                if (placementTarget.IsMouseOver) SetIsOpen(placementTarget, true);
            }

            if ((trigger & PopTipTrigger.Focus) == PopTipTrigger.Focus)
            {
                placementTarget.GotFocus += PlacementTarget_GotFocus;
                placementTarget.LostFocus += PlacementTarget_LostFocus;

                if (placementTarget.IsFocused) SetIsOpen(placementTarget, true);
            }
        }

        private static void UnhookPlacementTarget(UIElement placementTarget)
        {
            placementTarget.MouseEnter -= PlacementTarget_MouseEnter;
            placementTarget.MouseLeave -= PlacementTarget_MouseLeave;
            placementTarget.GotFocus -= PlacementTarget_GotFocus;
            placementTarget.LostFocus -= PlacementTarget_LostFocus;
        }

        private static void PlacementTarget_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            UpdateIsOpen((UIElement)sender);
        }

        private static void PlacementTarget_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            UpdateIsOpen((UIElement)sender);
        }

        private static void PlacementTarget_GotFocus(object sender, RoutedEventArgs e)
        {
            UpdateIsOpen((UIElement)sender);
        }

        private static void PlacementTarget_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdateIsOpen((UIElement)sender);
        }

        private static void UpdateIsOpen(UIElement placementTarget)
        {
            var trigger = GetTrigger(placementTarget);

            bool isOpen = false;

            if ((trigger & PopTipTrigger.MouseOver) == PopTipTrigger.MouseOver && placementTarget.IsMouseOver) isOpen = true;
            if ((trigger & PopTipTrigger.Focus) == PopTipTrigger.Focus && placementTarget.IsFocused) isOpen = true;

            SetIsOpen(placementTarget, isOpen);
        }

        private static void UpdatePopTipVisibility(UIElement placementTarget)
        {
            if (GetIsOpen(placementTarget) && HasPopTip(placementTarget))
            {
                if (!TryFindPopTipPopupAdorner(placementTarget, out var adornerLayer, out _))
                {
                    if (adornerLayer != null)
                    {
                        var popTip = new PopTip();

                        popTip.SetBinding(ContentControl.ContentProperty, new Binding()
                        {
                            Path = new PropertyPath(PopTipProperty),
                            Source = placementTarget
                        });

                        popTip.SetBinding(FrameworkElement.StyleProperty, new Binding()
                        {
                            Path = new PropertyPath(StyleProperty),
                            Source = placementTarget
                        });

                        var popTipPopupAdorner = new PopupAdorner(placementTarget)
                        {
                            Child = popTip
                        };

                        popTipPopupAdorner.SetBinding(PopupAdorner.PlacementModeProperty, new Binding()
                        {
                            Path = new PropertyPath(PlacementModeProperty),
                            Source = placementTarget
                        });

                        popTipPopupAdorner.SetBinding(PopupAdorner.HorizontalOffsetProperty, new Binding()
                        {
                            Path = new PropertyPath(HorizontalOffsetProperty),
                            Source = placementTarget
                        });

                        popTipPopupAdorner.SetBinding(PopupAdorner.VerticalOffsetProperty, new Binding()
                        {
                            Path = new PropertyPath(VerticalOffsetProperty),
                            Source = placementTarget
                        });

                        popTipPopupAdorner.SetBinding(PopupAdorner.UseDynamicPlacementProperty, new Binding()
                        {
                            Path = new PropertyPath(UseDynamicPlacementProperty),
                            Source = placementTarget
                        });

                        popTipPopupAdorner.SetBinding(PopupAdorner.KeepWithinViewProperty, new Binding()
                        {
                            Path = new PropertyPath(KeepWithinViewProperty),
                            Source = placementTarget
                        });

                        popTipPopupAdorner.ComputedPlacementModeChanged += PopTipPopupAdorner_ComputedPlacementModeChanged;

                        adornerLayer.Add(popTipPopupAdorner);
                    }
                }
            }
            else
            {
                if (TryFindPopTipPopupAdorner(placementTarget, out var adornerLayer, out var popTipPopupAdorner))
                {
                    if (popTipPopupAdorner != null)
                    {
                        popTipPopupAdorner.ComputedPlacementModeChanged -= PopTipPopupAdorner_ComputedPlacementModeChanged;
                        adornerLayer.Remove(popTipPopupAdorner);
                    }
                }
            }
        }

        private static void PopTipPopupAdorner_ComputedPlacementModeChanged(PopupAdorner sender, GenericPropertyChangedEventArgs<PopupAdornerPlacementMode> eventArgs)
        {
            var popTip = (PopTip)sender.Child;

            popTip.SetComputedPlacementMode(sender.ComputedPlacementMode);
        }

        private static bool TryFindPopTipPopupAdorner(UIElement placementTarget, out AdornerLayer adornerLayer, out PopupAdorner popTipPopupAdorner)
        {
            adornerLayer = AdornerLayer.GetAdornerLayer(placementTarget);

            if (adornerLayer == null)
            {
                popTipPopupAdorner = null;
                return false;
            }

            var adorners = adornerLayer.GetAdorners(placementTarget);

            popTipPopupAdorner = null;

            if (adorners != null && adorners.Length > 0) popTipPopupAdorner = adorners.OfType<PopupAdorner>().FirstOrDefault(x => x.Child is PopTip);

            return popTipPopupAdorner != null;
        }
    }
}