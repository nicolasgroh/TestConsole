using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace TestWPF
{
    public class PopupPlacementToBalloonDirectionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is PopupAdornerPlacementMode placementMode)
            {
                switch (placementMode)
                {
                    case PopupAdornerPlacementMode.BottomLeft: return BalloonDirection.TopRight;
                    case PopupAdornerPlacementMode.Left: return BalloonDirection.Right;
                    case PopupAdornerPlacementMode.TopLeft: return BalloonDirection.BottomRight;
                    case PopupAdornerPlacementMode.Top: return BalloonDirection.Bottom;
                    case PopupAdornerPlacementMode.TopRight: return BalloonDirection.BottomLeft;
                    case PopupAdornerPlacementMode.Right: return BalloonDirection.Left;
                    case PopupAdornerPlacementMode.BottomRight: return BalloonDirection.TopLeft;
                    case PopupAdornerPlacementMode.Bottom: return BalloonDirection.Top;
                }
            }

            return BalloonDirection.None;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}