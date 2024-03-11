using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace TestWPF
{
    public class PopTip : ContentControl
    {
        static PopTip()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PopTip), new FrameworkPropertyMetadata(typeof(PopTip)));
        }

        private static readonly DependencyPropertyKey ComputedPlacementModePropertyKey = DependencyProperty.RegisterReadOnly("ComputedPlacementMode", typeof(PopupAdornerPlacementMode), typeof(PopTip), new FrameworkPropertyMetadata(PopupAdornerPlacementMode.Top));

        public static readonly DependencyProperty ComputedPlacementModeProperty = ComputedPlacementModePropertyKey.DependencyProperty;

        public PopupAdornerPlacementMode ComputedPlacementMode
        {
            get { return (PopupAdornerPlacementMode)GetValue(ComputedPlacementModePropertyKey.DependencyProperty); }
            private set { SetValue(ComputedPlacementModePropertyKey, value); }
        }

        internal void SetComputedPlacementMode(PopupAdornerPlacementMode computedPlacementMode)
        {
            ComputedPlacementMode = computedPlacementMode;
        }
    }
}