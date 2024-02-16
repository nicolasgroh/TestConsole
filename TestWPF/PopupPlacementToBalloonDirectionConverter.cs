using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace TestWPF
{
    public class PopupPlacementToBeakDirectionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is PopupAdornerPlacementMode placementMode)
            {
                switch (placementMode)
                {
                    case PopupAdornerPlacementMode.BottomLeft: return BeakDirection.TopRight;
                    case PopupAdornerPlacementMode.Left: return BeakDirection.Right;
                    case PopupAdornerPlacementMode.TopLeft: return BeakDirection.BottomRight;
                    case PopupAdornerPlacementMode.Top: return BeakDirection.Bottom;
                    case PopupAdornerPlacementMode.TopRight: return BeakDirection.BottomLeft;
                    case PopupAdornerPlacementMode.Right: return BeakDirection.Left;
                    case PopupAdornerPlacementMode.BottomRight: return BeakDirection.TopLeft;
                    case PopupAdornerPlacementMode.Bottom: return BeakDirection.Top;
                }
            }

            return BeakDirection.None;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}