using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace TestWPF
{
    public class Badge : ContentControl
    {
        static Badge()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Badge), new FrameworkPropertyMetadata(typeof(Badge)));
        }

        internal Badge(UIElement placementTarget)
        {
            _placementTarget = placementTarget;

            SetBinding(ContentProperty, new Binding()
            {
                Path = new PropertyPath(BadgeService.BadgeProperty),
                Source = _placementTarget
            });
        }

        private UIElement _placementTarget;
        public UIElement PlacementTarget
        {
            get { return _placementTarget; }
        }

        public static readonly DependencyProperty LocationProperty = BadgeService.LocationProperty.AddOwner(typeof(Badge));
        public BadgeLocation Location
        {
            get { return BadgeService.GetLocation(PlacementTarget); }
            set { BadgeService.SetLocation(PlacementTarget, value); }
        }

        public static readonly DependencyProperty HorizontalOffsetProperty = BadgeService.HorizontalOffsetProperty.AddOwner(typeof(Badge));
        public double HorizontalOffset
        {
            get { return BadgeService.GetHorizontalOffset(PlacementTarget); }
            set { BadgeService.SetHorizontalOffset(PlacementTarget, value); }
        }

        public static readonly DependencyProperty VerticalOffsetProperty = BadgeService.VerticalOffsetProperty.AddOwner(typeof(Badge));
        public double VerticalOffset
        {
            get { return BadgeService.GetVerticalOffset(PlacementTarget); }
            set { BadgeService.SetVerticalOffset(PlacementTarget, value); }
        }
    }
}