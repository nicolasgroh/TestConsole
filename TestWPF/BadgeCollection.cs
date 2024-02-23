using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace TestWPF
{
    public class BadgeCollection : DependencyObject, ICollection<Badge>
    {
        public BadgeCollection()
        {
            items = new List<Badge>();
        }

        private UIElement _placementTarget;

        private Panel _itemsHost;
        protected Panel ItemsHost
        {
            get { return _itemsHost; }
            set
            {
                var oldValue = _itemsHost;

                _itemsHost = value;

                OnItemsHostChanged(oldValue, _itemsHost);
            }
        }

        private void OnItemsHostChanged(Panel oldValue, Panel newValue)
        {
            if (oldValue != null) oldValue.Children.Clear();

            if (newValue != null)
            {
                newValue.Children.Clear();

                for (int i = 0; i < items.Count; i++)
                {
                    newValue.Children.Add(items[i]);
                }
            }

            ItemsHostChanged?.Invoke(this, new BadgeCollectionItemsHostChangedEventArgs(oldValue, newValue));
        }

        internal Panel GetItemsHost()
        {
            return _itemsHost;
        }

        internal event EventHandler<BadgeCollectionItemsHostChangedEventArgs> ItemsHostChanged;

        #region ICollection
        private readonly List<Badge> items;

        public int Count => items.Count;

        public bool IsReadOnly => false;

        public void Add(Badge item)
        {
            int index = items.Count;

            InsertItem(index, item);
        }

        public void Clear()
        {
            ClearItems();
        }

        public void CopyTo(Badge[] array, int index)
        {
            items.CopyTo(array, index);
        }

        public bool Contains(Badge item)
        {
            return items.Contains(item);
        }

        public IEnumerator<Badge> GetEnumerator()
        {
            return items.GetEnumerator();
        }

        public void Insert(int index, Badge item)
        {
            if ((uint)index > (uint)items.Count) throw new ArgumentOutOfRangeException(nameof(index));

            InsertItem(index, item);
        }

        public bool Remove(Badge item)
        {
            int index = items.IndexOf(item);

            if (index < 0) return false;

            RemoveItem(index);
            return true;
        }

        public void RemoveAt(int index)
        {
            if ((uint)index >= (uint)items.Count) throw new ArgumentOutOfRangeException(nameof(index));

            RemoveItem(index);
        }
        #endregion

        private void ClearItems()
        {
            items.Clear();

            _itemsHost?.Children.Clear();

            if (_placementTarget != null) BadgeService.UpdateElementBadgeAdorner(_placementTarget);
        }

        private void InsertItem(int index, Badge item)
        {
            items.Insert(index, item);

            _itemsHost?.Children.Insert(index, item);

            if (_placementTarget != null) BadgeService.UpdateElementBadgeAdorner(_placementTarget);
        }

        private void RemoveItem(int index)
        {
            items.RemoveAt(index);

            _itemsHost?.Children.RemoveAt(index);

            if (_placementTarget != null) BadgeService.UpdateElementBadgeAdorner(_placementTarget);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)items).GetEnumerator();
        }

        internal void HookupPlacementTarget(UIElement placementTarget)
        {
            _placementTarget = placementTarget;
        }

        internal void UnhookPlacementTarget()
        {
            _placementTarget = null;
        }
    }

    public class BadgeCollectionItemsHostChangedEventArgs : EventArgs
    {
        public BadgeCollectionItemsHostChangedEventArgs(Panel oldValue,  Panel newValue)
        {
            _oldValue = oldValue;
            _newValue = newValue;
        }

        private readonly Panel _oldValue;
        private readonly Panel _newValue;

        public Panel OldValue { get { return _oldValue; } }

        public Panel NewValue { get { return _newValue; } }
    }
}