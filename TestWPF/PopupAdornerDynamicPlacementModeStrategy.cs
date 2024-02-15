using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestWPF
{
    public struct PopupAdornerDynamicPlacementModeStrategy
    {
        public static PopupAdornerDynamicPlacementModeStrategy Default = new PopupAdornerDynamicPlacementModeStrategy()
        {
            TargetBottomLeft = new PopupAdornerPlacementMode[]
            {
                PopupAdornerPlacementMode.Left,
                PopupAdornerPlacementMode.TopLeft,
                PopupAdornerPlacementMode.BottomRight,
                PopupAdornerPlacementMode.Right,
                PopupAdornerPlacementMode.TopRight,
                PopupAdornerPlacementMode.Bottom,
                PopupAdornerPlacementMode.Top
            },
            TargetLeft = new PopupAdornerPlacementMode[]
            {
                PopupAdornerPlacementMode.BottomLeft,
                PopupAdornerPlacementMode.TopLeft,
                PopupAdornerPlacementMode.Right,
                PopupAdornerPlacementMode.BottomRight,
                PopupAdornerPlacementMode.TopRight,
                PopupAdornerPlacementMode.Bottom,
                PopupAdornerPlacementMode.Top
            },
            TargetTopLeft = new PopupAdornerPlacementMode[]
            {
                PopupAdornerPlacementMode.Left,
                PopupAdornerPlacementMode.BottomLeft,
                PopupAdornerPlacementMode.TopRight,
                PopupAdornerPlacementMode.Right,
                PopupAdornerPlacementMode.BottomRight,
                PopupAdornerPlacementMode.Bottom,
                PopupAdornerPlacementMode.Top
            },
            TargetTop = new PopupAdornerPlacementMode[]
            {
                PopupAdornerPlacementMode.TopRight,
                PopupAdornerPlacementMode.TopLeft,
                PopupAdornerPlacementMode.Bottom,
                PopupAdornerPlacementMode.BottomRight,
                PopupAdornerPlacementMode.BottomLeft,
                PopupAdornerPlacementMode.Right,
                PopupAdornerPlacementMode.Left
            },
            TargetTopRight = new PopupAdornerPlacementMode[]
            {
                PopupAdornerPlacementMode.Right,
                PopupAdornerPlacementMode.BottomRight,
                PopupAdornerPlacementMode.TopLeft,
                PopupAdornerPlacementMode.Left,
                PopupAdornerPlacementMode.BottomLeft,
                PopupAdornerPlacementMode.Bottom,
                PopupAdornerPlacementMode.Top
            },
            TargetRight = new PopupAdornerPlacementMode[]
            {
                PopupAdornerPlacementMode.BottomRight,
                PopupAdornerPlacementMode.TopRight,
                PopupAdornerPlacementMode.Left,
                PopupAdornerPlacementMode.BottomLeft,
                PopupAdornerPlacementMode.TopLeft,
                PopupAdornerPlacementMode.Bottom,
                PopupAdornerPlacementMode.Top
            },
            TargetBottomRight = new PopupAdornerPlacementMode[]
            {
                PopupAdornerPlacementMode.Right,
                PopupAdornerPlacementMode.TopRight,
                PopupAdornerPlacementMode.BottomLeft,
                PopupAdornerPlacementMode.Left,
                PopupAdornerPlacementMode.TopLeft,
                PopupAdornerPlacementMode.Bottom,
                PopupAdornerPlacementMode.Top
            },
            TargetBottom = new PopupAdornerPlacementMode[]
            {
                PopupAdornerPlacementMode.BottomRight,
                PopupAdornerPlacementMode.BottomLeft,
                PopupAdornerPlacementMode.Top,
                PopupAdornerPlacementMode.TopRight,
                PopupAdornerPlacementMode.TopLeft,
                PopupAdornerPlacementMode.Right,
                PopupAdornerPlacementMode.Left
            }
        };

        public PopupAdornerPlacementMode[] TargetBottomLeft;
        public PopupAdornerPlacementMode[] TargetLeft;
        public PopupAdornerPlacementMode[] TargetTopLeft;
        public PopupAdornerPlacementMode[] TargetTop;
        public PopupAdornerPlacementMode[] TargetTopRight;
        public PopupAdornerPlacementMode[] TargetRight;
        public PopupAdornerPlacementMode[] TargetBottomRight;
        public PopupAdornerPlacementMode[] TargetBottom;
    }
}