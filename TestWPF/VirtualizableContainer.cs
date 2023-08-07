using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace TestWPF
{
    public class VirtualizableContainer : ContentControl
    {
        public static readonly DependencyPropertyKey IsVirtualizedPropertyKey = DependencyProperty.RegisterReadOnly("IsVirtualized", typeof(bool), typeof(VirtualizableContainer), new FrameworkPropertyMetadata(false));
        public bool IsVirtualized
        {
            get { return (bool)GetValue(IsVirtualizedPropertyKey.DependencyProperty); }
            private set { SetValue(IsVirtualizedPropertyKey, value); }
        }

        private Size _virtualSize;

        public void Virtualize()
        {
            if (IsVirtualized) return;

            _virtualSize = new Size(ActualWidth, ActualHeight);

            IsVirtualized = true;
        }

        public void DeVirtualize()
        {
            if (!IsVirtualized) return;

            IsVirtualized = false;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            if (IsVirtualized) return _virtualSize;

            return base.MeasureOverride(constraint);
        }
    }
}