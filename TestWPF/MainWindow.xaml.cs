using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Management;
using System.Windows.Documents;
using Microsoft.Win32;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Windows.Controls.Primitives;
using System.Windows.Shapes;
using System.Windows.Media;

namespace TestWPF
{
    public partial class MainWindow : Window, System.ComponentModel.INotifyPropertyChanged
    {
        public MainWindow()
        {
            InitializeComponent();

            for (int i = 0; i < 150; i++)
            {
                TestCollection.Add($"Test{i}");
            }
        }

        string _testProperty = "Test";
        public string TestProperty
        {
            get { return _testProperty; }
            set { SetProperty(ref _testProperty, value); }
        }

        string _testPropertyTwo = "Test 2";
        public string TestPropertyTwo
        {
            get { return _testPropertyTwo; }
            set { SetProperty(ref _testPropertyTwo, value); }
        }

        ObservableCollection<string> _testCollection;
        public ObservableCollection<string> TestCollection
        {
            get
            {
                _testCollection ??= new ObservableCollection<string>();

                return _testCollection;
            }
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        bool SetProperty<T>(ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            return true;
        }

        private void WriteLog(string entry)
        {
            //LogTextBlock.Inlines.Add(new Run(entry));
            //LogTextBlock.Inlines.Add(new LineBreak());
        }

        private void AddLinebreakButton_Click(object sender, RoutedEventArgs e)
        {
            var interopHelper = new WindowInteropHelper(this);

            NativeMethods.GetWindowRect(interopHelper.Handle, out RECT windowRect);

            NativeMethods.GetClientRect(interopHelper.Handle, out RECT clientRect);

            var windowPosition = new POINT
            {
                x = windowRect.left,
                y = windowRect.top
            };

            var windowWidth = windowRect.right - windowRect.left;
            var windowHeight = windowRect.bottom - windowRect.top;

            var clientAreaPosition = new POINT
            {
                x = windowRect.left + clientRect.left,
                y = windowRect.top + clientRect.top
            };

            var clientAreaWidth = clientRect.right - clientRect.left;
            var clientAreaHeight = clientRect.bottom - clientRect.top;

            var windowOutlineWindow = new OutlineWindow(windowPosition, windowWidth, windowHeight);
            var clientAreaOutlineWindow = new OutlineWindow(clientAreaPosition, clientAreaWidth, clientAreaHeight);

            windowOutlineWindow.Show();
            clientAreaOutlineWindow.Show();
        }

        private void Rectangle_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            WriteLog("Rectangle mouse down");
        }

        private void Button_LostFocus(object sender, RoutedEventArgs e)
        {
            WriteLog("Button Lost Focus");
        }

        private void LogTextBlock_Loaded(object sender, RoutedEventArgs e)
        {
            var textBlock = (TextBlock)sender;

            textBlock.Focusable = true;

            TextEditorWrapper.RegisterCommandHandlers(typeof(TextBlock), true, true, true);

            TextEditorWrapper.CreateFor(textBlock);
        }

        private void Test_Click(object sender, RoutedEventArgs e)
        {
            TestThumb.ShowBalloonTip("Hallooooo!", "Header");
        }

        private void TestThumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            var thumb = (Thumb)sender;

            var left = Canvas.GetLeft(thumb);
            var top = Canvas.GetTop(thumb);

            if (double.IsNaN(left)) left = 0;
            if (double.IsNaN(top)) top = 0;

            Canvas.SetLeft(thumb, left + e.HorizontalChange);
            Canvas.SetTop(thumb, top + e.VerticalChange);
        }

        private void RootBorder_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

        }
    }

    public class GeometryTest : FrameworkElement
    {
        public GeometryTest()
        {
            Margin = new Thickness(50, 50, 0, 0);

            Width = 100;
            Height = 100;
        }

        private enum StraightArrowDirection
        {
            Left,
            Up,
            Right,
            Down
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            var geometry = new StreamGeometry();

            using (StreamGeometryContext geometryContext = geometry.Open())
            {
                var width = 60d;
                var height = 30d;

                var arrowWidth = 10d;
                var arrowLength = 15d;

                var borderThickness = new Thickness(2);

                var direction = StraightArrowDirection.Down;

                //BuildStraightArrowGeometry(geometryContext, 0, 0, width, height, arrowWidth, arrowLength);

                BuildStraightArrowGeometryNew(geometryContext, width, height, arrowWidth, arrowLength, new Thickness(), direction);
                BuildStraightArrowGeometryNew(geometryContext, width, height, arrowWidth, arrowLength, borderThickness, direction);
            }

            drawingContext.DrawGeometry(Brushes.Black, null, geometry);
        }

        private void BuildStraightArrowGeometryNew(StreamGeometryContext geometryContext, double width, double height, double arrowWidth, double arrowLength, Thickness borderThickness, StraightArrowDirection direction)
        {
            // Ecke oben links
            geometryContext.BeginFigure(new Point(borderThickness.Left, borderThickness.Top), true, true);

            if (direction == StraightArrowDirection.Up)
            {
                var topBorderMidPoint = new Point(HorizontalMidPoint(width, borderThickness), borderThickness.Top);

                DrawStraightArrow(geometryContext, arrowWidth, arrowLength, direction, topBorderMidPoint, borderThickness.Bottom);
            }

            // Ecke oben rechts
            geometryContext.LineTo(new Point(width - borderThickness.Right, borderThickness.Top), true, false);

            if (direction == StraightArrowDirection.Right)
            {
                var rightBorderMidPoint = new Point(width - borderThickness.Right, VerticalMidPoint(height, borderThickness));

                DrawStraightArrow(geometryContext, arrowWidth, arrowLength, direction, rightBorderMidPoint, borderThickness.Bottom);
            }

            // Ecke unten rechts
            geometryContext.LineTo(new Point(width - borderThickness.Right, height - borderThickness.Bottom), true, false);

            // Pfeil nach unten
            if (direction == StraightArrowDirection.Down)
            {
                var bottomBorderMidPoint = new Point(HorizontalMidPoint(width, borderThickness), height - borderThickness.Bottom);

                DrawStraightArrow(geometryContext, arrowWidth, arrowLength, direction, bottomBorderMidPoint, borderThickness.Bottom);
            }

            // Ecke unten links
            geometryContext.LineTo(new Point(borderThickness.Left, height - borderThickness.Bottom), true, false);

            if (direction == StraightArrowDirection.Left)
            {
                var leftBorderMidPoint = new Point(borderThickness.Left, VerticalMidPoint(height, borderThickness));

                DrawStraightArrow(geometryContext, arrowWidth, arrowLength, direction, leftBorderMidPoint, borderThickness.Bottom);
            }
        }

        private double HorizontalMidPoint(double width, Thickness borderThickness)
        {
            return borderThickness.Left + (width - borderThickness.Left - borderThickness.Right) / 2;
        }

        private double VerticalMidPoint(double height, Thickness borderThickness)
        {
            return borderThickness.Top + (height - borderThickness.Top - borderThickness.Bottom) / 2;
        }

        private void OffsetStraightArrowByThickness(StraightArrowDirection direction, double arrowWidth, double arrowLength, double thickness, ref Point arrowStart, ref Point arrowEnd, ref Point arrowTip)
        {
            var alpha = CalculateAlpha(arrowWidth / 2, arrowLength);

            var angleThicknessOffset = CalculateAngleThicknessOffset(alpha, thickness);

            var tipThicknessOffset = CalculateStraightArrowTipThicknessOffset(alpha, thickness);

            var isHorizontal = direction == StraightArrowDirection.Left || direction == StraightArrowDirection.Right;

            if (isHorizontal)
            {
                arrowStart.Offset(0d, angleThicknessOffset);
                arrowEnd.Offset(0d, -angleThicknessOffset);

                switch (direction)
                {
                    case StraightArrowDirection.Left:
                        arrowTip.Offset(tipThicknessOffset - thickness, 0d);
                        break;
                    case StraightArrowDirection.Right:
                        arrowTip.Offset(-(tipThicknessOffset - thickness), 0d);
                        break;
                }
            }
            else
            {
                arrowStart.Offset(angleThicknessOffset, 0d);
                arrowEnd.Offset(-angleThicknessOffset, 0d);

                switch (direction)
                {
                    case StraightArrowDirection.Up:
                        arrowTip.Offset(0d, tipThicknessOffset - thickness);
                        break;
                    case StraightArrowDirection.Down:
                        arrowTip.Offset(0d, -(tipThicknessOffset - thickness));
                        break;
                }
            }
        }

        private void StraightArrowFromMidPoint(Point midPoint, StraightArrowDirection direction, double arrowWidth, double arrowLength, out Point arrowStart, out Point arrowEnd, out Point arrowTip)
        {
            var isHorizontal = direction == StraightArrowDirection.Left || direction == StraightArrowDirection.Right;

            if (isHorizontal)
            {
                arrowStart = midPoint;
                arrowStart.Offset(0d, -arrowWidth / 2);

                arrowEnd = arrowStart;
                arrowEnd.Offset(0d, arrowWidth);

                switch (direction)
                {
                    case StraightArrowDirection.Left:
                        arrowTip = midPoint;
                        arrowTip.Offset(-arrowLength, 0d);
                        break;
                    case StraightArrowDirection.Right:
                        arrowTip = midPoint;
                        arrowTip.Offset(arrowLength, 0d);
                        break;
                }
            }
            else
            {
                arrowStart = midPoint;
                arrowStart.Offset(-arrowWidth / 2, 0d);

                arrowEnd = arrowStart;
                arrowEnd.Offset(arrowWidth, 0d);

                switch (direction)
                {
                    case StraightArrowDirection.Up:
                        arrowTip = midPoint;
                        arrowTip.Offset(0d, -arrowLength);
                        break;
                    case StraightArrowDirection.Down:
                        arrowTip = midPoint;
                        arrowTip.Offset(0d, arrowLength);
                        break;
                }
            }
        }

        private void DrawStraightArrow(StreamGeometryContext geometryContext, double arrowWidth, double arrowLength, StraightArrowDirection direction, Point arrowMidPoint, double thickness)
        {
            StraightArrowFromMidPoint(arrowMidPoint, direction, arrowWidth, arrowLength, out var arrowStart, out var arrowEnd, out var arrowTip);

            if (thickness > 0.0) OffsetStraightArrowByThickness(direction, arrowWidth, arrowLength, thickness, ref arrowStart, ref arrowEnd, ref arrowTip);

            geometryContext.LineTo(arrowEnd, true, false);
            geometryContext.LineTo(arrowTip, true, false);
            geometryContext.LineTo(arrowStart, true, false);
        }

        private double CalculateAlpha(double a, double b)
        {
            var c = Math.Sqrt(Math.Pow(a, 2) + Math.Pow(b, 2));

            return Math.Asin(a / c);
        }

        private double CalculateAngleThicknessOffset(double angle, double thickness)
        {
            return thickness * (1 - angle / (Math.PI / 2));
        }

        private double CalculateStraightArrowTipThicknessOffset(double arrowStartAngle, double thickness)
        {
            return thickness / Math.Sin(arrowStartAngle);
        }
    }
}