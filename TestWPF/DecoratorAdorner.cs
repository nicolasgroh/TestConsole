using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;

namespace TestWPF
{
    public class DecoratorAdorner : Adorner, IAddChild
    {
        private class EmptyEnumerator : IEnumerator
        {
            public void Reset() { }

            public bool MoveNext() { return false; }

            public object Current
            {
                get
                {
                    throw new InvalidOperationException();
                }
            }
        }

        private class SingleChildEnumerator : IEnumerator
        {
            internal SingleChildEnumerator(object Child)
            {
                _child = Child;
                _count = Child == null ? 0 : 1;
            }

            object IEnumerator.Current
            {
                get { return (_index == 0) ? _child : null; }
            }

            bool IEnumerator.MoveNext()
            {
                _index++;
                return _index < _count;
            }

            void IEnumerator.Reset()
            {
                _index = -1;
            }

            private int _index = -1;
            private int _count = 0;
            private object _child;
        }

        public DecoratorAdorner(UIElement adornedElement) : base(adornedElement)
        {

        }

        public void AddChild(object value)
        {
            if (!(value is UIElement))
            {
                throw new ArgumentException("Value has to by of Type UIElement", nameof(value));
            }

            if (Child != null)
            {
                throw new ArgumentException("Can only have one child");
            }

            Child = (UIElement)value;
        }

        public void AddText(string text)
        {
            AddChild(new TextBlock() { Text = text });
        }

        UIElement _child;
        public virtual UIElement Child
        {
            get
            {
                return _child;
            }
            set
            {
                if (_child != value)
                {
                    RemoveVisualChild(_child);

                    RemoveLogicalChild(_child);

                    _child = value;

                    AddLogicalChild(_child);
                    
                    AddVisualChild(_child);

                    InvalidateMeasure();
                }
            }
        }

        protected override IEnumerator LogicalChildren
        {
            get
            {
                if (_child == null)
                {
                    return new EmptyEnumerator();
                }

                return new SingleChildEnumerator(_child);
            }
        }

        protected override int VisualChildrenCount
        {
            get { return _child == null ? 0 : 1; }
        }

        protected override Visual GetVisualChild(int index)
        {
            if (_child == null || index != 0) throw new ArgumentOutOfRangeException(nameof(index));

            return _child;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            if (Child != null)
            {
                Child.Measure(constraint);

                return Child.DesiredSize;
            }

            return new Size();
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            if (Child != null)
            {
                Child.Arrange(new Rect(arrangeSize));
            }

            return arrangeSize;
        }
    }
}