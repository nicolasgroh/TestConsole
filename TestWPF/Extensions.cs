using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;

namespace TestWPF
{
    public static class Extensions
    {
        public static T ParentOfType<T>(this DependencyObject element) where T : DependencyObject
        {
            var parent = VisualTreeHelper.GetParent(element);

            if (parent == null) return null;

            if (parent is T tParent) return tParent;

            return ParentOfType<T>(parent);
        }

        private const double DBL_EPSILON = 2.2204460492503131e-016;

        public static bool AreClose(this double value1, double value2)
        {
            if (value1 == value2) return true;
            // This computes (|value1-value2| / (|value1| + |value2| + 10.0)) < DBL_EPSILON
            double eps = (Math.Abs(value1) + Math.Abs(value2) + 10.0) * DBL_EPSILON;
            double delta = value1 - value2;
            return (-eps < delta) && (eps > delta);
        }

        public static void ShowBalloonTip(this UIElement element, string text)
        {
            BeakedBorder balloon;

            var popupAdorner = new PopupAdorner(element)
            {
                PlacementMode = PopupAdornerPlacementMode.Top,
                Child = balloon = new BeakedBorder()
                {
                    Child = new TextBlock()
                    {
                        Text = text,
                        Margin = new Thickness(5, 2, 5, 2)
                    }
                }
            };

            balloon.SetBinding(BeakedBorder.BeakDirectionProperty, new Binding("PlacementMode") { Source = popupAdorner, Converter = new PopupPlacementToBeakDirectionConverter() });

            var adornerLayer = AdornerLayer.GetAdornerLayer(element);

            var timer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromSeconds(5)
            };

            timer.Tick += (s, e) =>
            {
                timer.Stop();

                adornerLayer?.Remove(popupAdorner);
            };

            adornerLayer?.Add(popupAdorner);

            timer.Start();
        }

        public static T GetParentOfType<T>(this DependencyObject element)
        {
            DependencyObject parent;

            do
            {
                parent = VisualTreeHelper.GetParent(element);

                if (parent is T parentOfType) return parentOfType;
            }
            while (parent != null);

            return default;
        }
    }
}
