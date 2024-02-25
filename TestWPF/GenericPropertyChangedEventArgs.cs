using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestWPF
{
    public delegate void GenericPropertyChangedEventHandler<TSender, TProperty>(TSender sender, GenericPropertyChangedEventArgs<TProperty> eventArgs);

    public class GenericPropertyChangedEventArgs<T>
    {
        public GenericPropertyChangedEventArgs(T oldValue, T newValue)
        {
            _oldValue = oldValue;
            _newValue = newValue;
        }

        private T _oldValue;
        public T OldValue { get { return _oldValue; } }

        private T _newValue;

        public T NewValue { get { return _newValue; } }
    }
}