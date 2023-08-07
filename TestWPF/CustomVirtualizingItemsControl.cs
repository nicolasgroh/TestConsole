using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace TestWPF
{
    public class CustomVirtualizingItemsControl : CustomItemsControl
    {
        static CustomVirtualizingItemsControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomVirtualizingItemsControl), new FrameworkPropertyMetadata(typeof(CustomVirtualizingItemsControl)));
        }

        public CustomVirtualizingItemsControl()
        {
            ContentHostChanged += CustomVirtualizingItemsControl_ContentHostChanged;
        }

        protected override ContentControl CreateItemContainer()
        {
            return new VirtualizableContainer();
        }

        private void SetItemsHostScrollContentPresenter(ScrollContentPresenter scrollContentPresenter)
        {
            if (ItemsHost != null && ItemsHost is CustomVirtualizingPanel virtualizingPanel)
            {
                virtualizingPanel.ScrollInfo = scrollContentPresenter;
            }
        }

        private void CustomVirtualizingItemsControl_ContentHostChanged(object sender, CustimItemsControlPropertyChangedEventArgs e)
        {
            ScrollContentPresenter scrollContentPresenter = null;

            if (e.OldValue != null && e.OldValue is CustomVirtualizingScrollViewer oldVirtualizingScrollViewer)
            {
                oldVirtualizingScrollViewer.ScrollContentPresenterChanged -= VirtualizingScrollViewer_ScrollContentPresenterChanged;
            }

            if (e.NewValue != null && e.NewValue is CustomVirtualizingScrollViewer newVirtualizingScrollViewer)
            {
                scrollContentPresenter = newVirtualizingScrollViewer.ScrollContentPresenter;

                newVirtualizingScrollViewer.ScrollContentPresenterChanged += VirtualizingScrollViewer_ScrollContentPresenterChanged;
            }

            SetItemsHostScrollContentPresenter(scrollContentPresenter);
        }

        private void VirtualizingScrollViewer_ScrollContentPresenterChanged(object sender, CustimItemsControlPropertyChangedEventArgs e)
        {
            SetItemsHostScrollContentPresenter((ScrollContentPresenter)e.NewValue);
        }
    }
}