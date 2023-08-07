using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace TestWPF
{
    public class CustomItemsControl : Control
    {
        static CustomItemsControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomItemsControl), new FrameworkPropertyMetadata(typeof(CustomItemsControl)));
        }

        #region ItemsSourceProperty
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(CustomItemsControl), new FrameworkPropertyMetadata(ItemsSourcePropertyChanged));

        private static void ItemsSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var customItemsControl = (CustomItemsControl)d;

            if (e.OldValue is INotifyCollectionChanged oldNotifyCollectionChanged)
            {
                customItemsControl.UnhookItemsSource(oldNotifyCollectionChanged);
            }

            if (e.NewValue is INotifyCollectionChanged newNotifyCollectionChanged)
            {
                customItemsControl.HookupItemsSource(newNotifyCollectionChanged);
            }

            customItemsControl.OnItemsSourceChanged();
        }

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        private void HookupItemsSource(INotifyCollectionChanged notifyCollectionChanged)
        {
            notifyCollectionChanged.CollectionChanged += ItemsSource_CollectionChanged;
        }

        private void UnhookItemsSource(INotifyCollectionChanged notifyCollectionChanged)
        {
            notifyCollectionChanged.CollectionChanged -= ItemsSource_CollectionChanged;
        }
        #endregion

        #region ItemsContainerStyleProperty
        public static readonly DependencyProperty ItemContainerStyleProperty = DependencyProperty.Register("ItemContainerStyle", typeof(Style), typeof(CustomItemsControl), new FrameworkPropertyMetadata(ItemContainerStylePropertyChanged));

        private static void ItemContainerStylePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CustomItemsControl)d).OnItemContainerStyleChanged();
        }

        public Style ItemContainerStyle
        {
            get { return (Style)GetValue(ItemContainerStyleProperty); }
            set { SetValue(ItemContainerStyleProperty, value); }
        }
        #endregion

        #region IsGeneratedContainerAttachedProperty
        private static readonly DependencyPropertyKey IsGeneratedContainerPropertyKey = DependencyProperty.RegisterAttachedReadOnly("IsGeneratedContainer", typeof(bool), typeof(CustomItemsControl), new FrameworkPropertyMetadata(false));

        public static bool GetIsGeneratedContainer(DependencyObject container)
        {
            return (bool)container.GetValue(IsGeneratedContainerPropertyKey.DependencyProperty);
        }

        private static void SetIsGeneratedContainer(DependencyObject container, bool value)
        {
            container.SetValue(IsGeneratedContainerPropertyKey, value);
        }
        #endregion

        #region ItemTemplateProperty
        public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register("ItemTemplate", typeof(DataTemplate), typeof(CustomItemsControl), new FrameworkPropertyMetadata(null, ItemTemplatePropertyChanged));

        private static void ItemTemplatePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CustomItemsControl)d).OnItemTemplateChanged();
        }

        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }
        #endregion

        public bool IsCollectionChanging
        {
            get => _isCollectionChanging;
        }

        public ContentControl ContentHost
        {
            get => _contentHost;
        }

        protected Panel ItemsHost
        {
            get => _itemsHost;
        }

        public event EventHandler<CustimItemsControlPropertyChangedEventArgs> ContentHostChanged;

        #region Private Members
        ContentControl _contentHost;
        Panel _itemsHost;
        private bool _isCollectionChanging = false;
        #endregion

        public override void OnApplyTemplate()
        {
            var oldContentHost = _contentHost;

            _contentHost = (ContentControl)GetTemplateChild("PART_ContentHost");

            if (_contentHost == null) throw new ArgumentNullException("PART_ContentHost");

            ContentHostChanged?.Invoke(this, new CustimItemsControlPropertyChangedEventArgs(oldContentHost, _contentHost));

            ClearItemsHost();
            
            CreateAndPrepareItemsHost();

            GenerateItemsContainer();
        }

        protected virtual void OnItemsSourceChanged()
        {
            ClearItemsHost();

            GenerateItemsContainer();
        }

        protected virtual void OnItemsSourceCollectionChanged(NotifyCollectionChangedEventArgs e)
        {

        }

        protected virtual Panel CreateItemsHost()
        {
            return new StackPanel();
        }

        protected virtual void OnItemContainerStyleChanged()
        {
            ClearItemsHost();

            GenerateItemsContainer();
        }

        protected virtual void PrepareItemContainer(ContentControl container, object item)
        {
            container.Style = ItemContainerStyle;
            container.Content = item;

            if (item is UIElement) container.ContentTemplate = null;
            else container.ContentTemplate = ItemTemplate;
        }

        protected virtual void ContainerAdded(UIElement container, int index)
        {
            
        }

        protected virtual void OnItemTemplateChanged()
        {
            if (!HasItemsHost()) return;

            for (int i = 0; i < _itemsHost.Children.Count; i++)
            {
                var child = _itemsHost.Children[i];

                if (GetIsGeneratedContainer(child) && child is ContentControl container)
                {
                    PrepareItemContainer(container, container.Content);
                }
            }
        }

        protected bool HasItemsHost()
        {
            return _itemsHost != null;
        }

        private void ItemsSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _isCollectionChanging = true;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add: InsertItem(e.NewStartingIndex, e.NewItems[0]); break;
                case NotifyCollectionChangedAction.Remove: RemoveItem(e.OldItems[0]); break;
                case NotifyCollectionChangedAction.Replace:
                    RemoveItemAt(e.OldStartingIndex);
                    InsertItem(e.NewStartingIndex, e.NewItems[0]);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    ClearItemsHost();
                    GenerateItemsContainer();
                    break;
            }

            _isCollectionChanging = false;

            OnItemsSourceCollectionChanged(e);
        }

        private void ClearItemsHost()
        {
            if (_itemsHost != null)
            {
                _itemsHost.Children.Clear();
            }
        }

        private void CreateAndPrepareItemsHost()
        {
            _itemsHost = CreateItemsHost();

            if (_itemsHost == null) throw new Exception("Unable to Create ItemsPanel");

            if (_contentHost != null) _contentHost.Content = _itemsHost;
        }

        protected virtual ContentControl CreateItemContainer()
        {
            return new ContentControl();
        }

        private UIElement CreateAndPrepareItemContainer(object item)
        {
            var container = CreateItemContainer();

            SetIsGeneratedContainer(container, true);

            PrepareItemContainer(container, item);

            return container;
        }

        private int CollectionIndexToChildIndex(int index)
        {
            var childIndex = 0;
            var collectionIndex = 0;

            for (int i = 0; i < _itemsHost.Children.Count; i++)
            {
                if (collectionIndex == index) break;

                var child = _itemsHost.Children[i];

                if (GetIsGeneratedContainer(child)) collectionIndex++;

                childIndex++;
            }

            return childIndex;
        }

        private void Additem(object item)
        {
            if (!HasItemsHost()) return;

            var container = CreateAndPrepareItemContainer(item);

            var index = _itemsHost.Children.Add(container);

            ContainerAdded(container, index);
        }

        private void InsertItem(int index, object item)
        {
            if (!HasItemsHost()) return;

            var container = CreateAndPrepareItemContainer(item);

            _itemsHost.Children.Insert(index, container);

            ContainerAdded(container, index);
        }

        private void RemoveItem(object item)
        {
            if (!HasItemsHost()) return;

            if (item is UIElement ItemUIElement)
            {
                _itemsHost.Children.Remove(ItemUIElement);
            }
            else
            {
                int collectionIndex = -1;

                if (ItemsSource is IList itemsSourceList) collectionIndex = itemsSourceList.IndexOf(item);
                else
                {
                    var index = 0;

                    foreach (var sourceItem in ItemsSource)
                    {
                        if (sourceItem == item)
                        {
                            collectionIndex = index;
                            break;
                        }

                        index++;
                    }
                }

                if (collectionIndex == -1) return;

                RemoveItemAt(collectionIndex);
            }
        }

        private void RemoveItemAt(int index)
        {
            if (!HasItemsHost()) return;

            var childIndex = CollectionIndexToChildIndex(index);

            _itemsHost.Children.RemoveAt(childIndex);
        }

        private void GenerateItemsContainer()
        {
            if (ItemsSource == null)
            {
                ClearItemsHost();
                return;
            }

            if (!HasItemsHost()) return;

            if (ItemsSource is IList listItemsSource)
            {
                for (int i = 0; i < listItemsSource.Count; i++)
                {
                    var item = listItemsSource[i];

                    Additem(item);
                }
            }
            else if (ItemsSource is IEnumerable enumerableItemsSource)
            {
                foreach (var item in enumerableItemsSource)
                {
                    Additem(item);
                }
            }
        }
    }

    public class CustimItemsControlPropertyChangedEventArgs : EventArgs
    {
        public CustimItemsControlPropertyChangedEventArgs(object oldValue, object newValue)
        {
            _oldValue = oldValue;
            _newValue = newValue;
        }

        private object _oldValue;
        public object OldValue
        {
            get { return _oldValue; }
        }

        private object _newValue;
        public object NewValue
        {
            get { return _newValue; }
        }
    }
}