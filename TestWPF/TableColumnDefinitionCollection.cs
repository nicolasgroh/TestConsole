using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TestWPF
{
    public class TableColumnDefinitionCollection : Collection<TableColumnDefinition>
    {
        public TableColumnDefinitionCollection(TablePanel owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        private readonly TablePanel _owner;

        private void ResetMeasure()
        {
            _owner.InvalidateMeasure();
        }

        private void ColumnDefinition_LayoutPropertyChanged(TableColumnDefinition columnDefinition, DependencyProperty property)
        {
            ResetMeasure();
        }

        private void HookupColumnDefinition(TableColumnDefinition columnDefinition)
        {
            columnDefinition.LayoutPropertyChanged += ColumnDefinition_LayoutPropertyChanged;
        }

        private void UnhookColumnDefinition(TableColumnDefinition columnDefinition)
        {
            columnDefinition.LayoutPropertyChanged -= ColumnDefinition_LayoutPropertyChanged;
        }

        protected override void ClearItems()
        {
            for (int i = 0; i < Count; i++)
            {
                UnhookColumnDefinition(this[i]);
            }

            base.ClearItems();

            ResetMeasure();
        }

        protected override void InsertItem(int index, TableColumnDefinition item)
        {
            base.InsertItem(index, item);

            HookupColumnDefinition(item);

            ResetMeasure();
        }

        protected override void RemoveItem(int index)
        {
            UnhookColumnDefinition(Items[index]);

            base.RemoveItem(index);

            ResetMeasure();
        }

        protected override void SetItem(int index, TableColumnDefinition item)
        {
            UnhookColumnDefinition(Items[index]);

            base.SetItem(index, item);

            HookupColumnDefinition(item);

            ResetMeasure();
        }
    }
}