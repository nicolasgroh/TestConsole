using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TestWPF
{
    public class InnerDropshadowBorder : Decorator
    {
        public InnerDropshadowBorder()
        {
            _innerDropShadowRenderer = new InnerDropshadowRenderer();
            AddVisualChild(_innerDropShadowRenderer);
        }

        private InnerDropshadowRenderer _innerDropShadowRenderer;

        protected override Visual GetVisualChild(int index)
        {
            if (index == base.VisualChildrenCount)
            {
                if (_innerDropShadowRenderer == null)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }
                return _innerDropShadowRenderer;
            }
            else return base.GetVisualChild(index);
        }

        protected override int VisualChildrenCount
        {
            get { return base.VisualChildrenCount + (_innerDropShadowRenderer != null ? 1 : 0); }
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            var childArrangeSize = base.ArrangeOverride(arrangeSize);

            _innerDropShadowRenderer.DrawDropShadows(childArrangeSize, 8);

            return childArrangeSize;
        }
    }
}