using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Threading;

namespace TestWPF
{
    public class BalloonTip : HeaderedContentControl
    {
        static BalloonTip()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BalloonTip), new FrameworkPropertyMetadata(typeof(BalloonTip)));
        }

        public static readonly DependencyProperty ShowDurationProperty = BalloonTipService.ShowDurationProperty.AddOwner(typeof(BalloonTip), new FrameworkPropertyMetadata(new Duration(TimeSpan.FromSeconds(3))));
        public Duration ShowDuration
        {
            get { return (Duration)GetValue(ShowDurationProperty); }
            set { SetValue(ShowDurationProperty, value); }
        }

        public static readonly DependencyProperty PlacementModeProperty = BalloonTipService.PlacementModeProperty.AddOwner(typeof(BalloonTip), new FrameworkPropertyMetadata(PopupAdornerPlacementMode.Top));
        public PopupAdornerPlacementMode PlacementMode
        {
            get { return (PopupAdornerPlacementMode)GetValue(PlacementModeProperty); }
            set { SetValue(PlacementModeProperty, value); }
        }

        private static readonly DependencyPropertyKey ComputedPlacementModePropertyKey = DependencyProperty.RegisterReadOnly("ComputedPlacementMode", typeof(PopupAdornerPlacementMode), typeof(BalloonTip), new FrameworkPropertyMetadata(PopupAdornerPlacementMode.Top));

        public static readonly DependencyProperty ComputedPlacementModeProperty = ComputedPlacementModePropertyKey.DependencyProperty;

        public PopupAdornerPlacementMode ComputedPlacementMode
        {
            get { return (PopupAdornerPlacementMode)GetValue(ComputedPlacementModePropertyKey.DependencyProperty); }
            private set { SetValue(ComputedPlacementModePropertyKey, value); }
        }

        public static readonly DependencyProperty HorizontalOffsetProperty = BalloonTipService.HorizontalOffsetProperty.AddOwner(typeof(BalloonTip), new FrameworkPropertyMetadata(0d));
        public double HorizontalOffset
        {
            get { return (double)GetValue(HorizontalOffsetProperty); }
            set { SetValue(HorizontalOffsetProperty, value); }
        }

        public static readonly DependencyProperty VerticalOffsetProperty = BalloonTipService.VerticalOffsetProperty.AddOwner(typeof(BalloonTip), new FrameworkPropertyMetadata(0d));
        public double VerticalOffset
        {
            get { return (double)GetValue(VerticalOffsetProperty); }
            set { SetValue(VerticalOffsetProperty, value); }
        }

        public static readonly DependencyProperty UseDynamicPlacementProperty = BalloonTipService.UseDynamicPlacementProperty.AddOwner(typeof(BalloonTip), new FrameworkPropertyMetadata(true));
        public bool UseDynamicPlacement
        {
            get { return (bool)GetValue(UseDynamicPlacementProperty); }
            set { SetValue(UseDynamicPlacementProperty, value); }
        }

        public static readonly DependencyProperty KeepWithinViewProperty = BalloonTipService.KeepWithinViewProperty.AddOwner(typeof(BalloonTip), new FrameworkPropertyMetadata(true));
        public bool KeepWithinView
        {
            get { return (bool)GetValue(KeepWithinViewProperty); }
            set { SetValue(KeepWithinViewProperty, value); }
        }

        internal void SetComputedPlacementMode(PopupAdornerPlacementMode computedPlacementMode)
        {
            ComputedPlacementMode = computedPlacementMode;
        }
    }
}