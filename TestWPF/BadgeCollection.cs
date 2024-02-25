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
    public class BadgeCollection : Collection<Badge>
    {
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

                for (int i = 0; i < Items.Count; i++)
                {
                    newValue.Children.Add(Items[i]);
                }
            }

            ItemsHostChanged?.Invoke(this, new BadgeCollectionItemsHostChangedEventArgs(oldValue, newValue));
        }

        internal Panel GetItemsHost()
        {
            return _itemsHost;
        }

        internal event EventHandler<BadgeCollectionItemsHostChangedEventArgs> ItemsHostChanged;

        protected override void InsertItem(int index, Badge item)
        {
            base.InsertItem(index, item);

            _itemsHost?.Children.Insert(index, item);

            if (_placementTarget != null) BadgeService.UpdateElementBadges(_placementTarget);
        }

        protected override void RemoveItem(int index)
        {
            base.RemoveItem(index);

            _itemsHost?.Children.RemoveAt(index);

            if (_placementTarget != null) BadgeService.UpdateElementBadges(_placementTarget);
        }

        protected override void SetItem(int index, Badge item)
        {
            base.SetItem(index, item);

            _itemsHost?.Children.RemoveAt(index);

            _itemsHost?.Children.Insert(index, item);
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