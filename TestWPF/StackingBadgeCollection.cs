using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Navigation;

namespace TestWPF
{
    public sealed class StackingBadgeCollection : BadgeCollection, INotifyPropertyChanged
    {
        public StackingBadgeCollection()
        {
            var stackPanel = new StackPanel();

            stackPanel.SetBinding(StackPanel.OrientationProperty, new Binding(nameof(Orientation))
            {
                Source = this
            });

            ItemsHost = stackPanel;
        }

        private Orientation _orientation;
        public Orientation Orientation
        {
            get { return _orientation; }
            set
            {
                _orientation = value;
                OnPropertyChanged(nameof(Orientation));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}