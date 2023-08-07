using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace TestWPF
{
    public class CustomVirtualizingScrollViewer : ScrollViewer
    {
        private ScrollContentPresenter _scrollContentPresenter;

        public ScrollContentPresenter ScrollContentPresenter
        {
            get { return _scrollContentPresenter; }
        }

        public event EventHandler<CustimItemsControlPropertyChangedEventArgs> ScrollContentPresenterChanged;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var oldScrollContentPresenter = _scrollContentPresenter;

            _scrollContentPresenter = (ScrollContentPresenter)GetTemplateChild("PART_ScrollContentPresenter");

            ScrollContentPresenterChanged?.Invoke(this, new CustimItemsControlPropertyChangedEventArgs(oldScrollContentPresenter, _scrollContentPresenter));
        }
    }
}