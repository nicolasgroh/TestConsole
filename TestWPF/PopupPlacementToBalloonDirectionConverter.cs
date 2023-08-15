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
            if (value is AdornerPopupPlacementMode placementMode)
            {
                switch (placementMode)
                {
                    case AdornerPopupPlacementMode.BottomLeft: return BalloonDirection.TopRight;
                    case AdornerPopupPlacementMode.Left: return BalloonDirection.Right;
                    case AdornerPopupPlacementMode.TopLeft: return BalloonDirection.BottomRight;
                    case AdornerPopupPlacementMode.Top: return BalloonDirection.Bottom;
                    case AdornerPopupPlacementMode.TopRight: return BalloonDirection.BottomLeft;
                    case AdornerPopupPlacementMode.Right: return BalloonDirection.Left;
                    case AdornerPopupPlacementMode.BottomRight: return BalloonDirection.TopLeft;
                    case AdornerPopupPlacementMode.Bottom: return BalloonDirection.Top;
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