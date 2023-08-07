using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace TestWPF
{
    /// <summary>
    /// Interaktionslogik für OutlineWindow.xaml
    /// </summary>
    public partial class OutlineWindow : Window
    {
        public OutlineWindow(POINT position, int width, int height)
        {
            InitializeComponent();

            _position = position;
            _width = width;
            _height = height;
        }

        POINT _position;
        int _width;
        int _height;

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            var interopHelper = new WindowInteropHelper(this);

            NativeMethods.SetWindowPos(interopHelper.Handle, 0, _position.x, _position.y, _width, _height, (int)SetWindowPosFlags.SWP_SHOWWINDOW | (int)SetWindowPosFlags.SWP_NOZORDER);
            NativeMethods.SetWindowPos(interopHelper.Handle, 0, _position.x, _position.y, _width, _height, (int)SetWindowPosFlags.SWP_SHOWWINDOW | (int)SetWindowPosFlags.SWP_NOZORDER);
        }
    }
}
