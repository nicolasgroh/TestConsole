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
using System.Net;

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

        private const int MAXIMUM_POINTS_COUNT = 11;

        private struct GeometryInfo
        {
            public double RectangleWidth;
            public double RectangleHeight;
            public double ArrowWidth;
            public double ArrowLength;
            public BeakDirection BeakDirection;
            public CornerRadius CornerRadius;
            public double RectangleOffsetX;
            public double RectangleOffsetY;
            public double DiagonalArrowInset;
            public double DiagonalArrowTipOffset;
        }

        private enum LineType
        {
            Line,
            Arc
        }

        private struct GeometryPoint
        {
            private GeometryPoint(Point coordinates, LineType lineType, Size arcSize = default)
            {
                Coordinates = coordinates;
                LineType = lineType;
                ArcSize = arcSize;
            }

            public static GeometryPoint Line(Point coordinates)
            {
                return new GeometryPoint(coordinates, LineType.Line);
            }

            public static GeometryPoint Arc(Point coordinates, Size arcSize)
            {
                return new GeometryPoint(coordinates, LineType.Arc, arcSize);
            }

            public Point Coordinates;
            public LineType LineType;
            public Size ArcSize;
        }

        private GeometryInfo _geometryInfo;

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            var width = 80d;
            var height = 40d;

            CreateGeometryInfo(width, height);

            var borderThickness = new Thickness(2, 2, 2, 2);

            var innerGeometryPoints = new List<GeometryPoint>(MAXIMUM_POINTS_COUNT);

            CreateGeometry(innerGeometryPoints, borderThickness);

            List<GeometryPoint> outerGeometryPoints;

            if (HasBorder(borderThickness))
            {
                outerGeometryPoints = new List<GeometryPoint>(MAXIMUM_POINTS_COUNT);

                CreateGeometry(outerGeometryPoints, new Thickness());
            }
            else outerGeometryPoints = new List<GeometryPoint>();

            var borderGeometry = new StreamGeometry();

            using (StreamGeometryContext borderGeometryContext = borderGeometry.Open())
            {
                if (innerGeometryPoints.Count > 0)
                {
                    BuildGeometry(borderGeometryContext, innerGeometryPoints);
                }

                if (outerGeometryPoints.Count > 0)
                {
                    BuildGeometry(borderGeometryContext, outerGeometryPoints);
                }
            }

            var backgroundGeometry = new StreamGeometry();

            using (StreamGeometryContext backgroundGeometryContext = backgroundGeometry.Open())
            {
                if (innerGeometryPoints.Count > 0)
                {
                    BuildGeometry(backgroundGeometryContext, innerGeometryPoints);
                }
            }

            drawingContext.DrawGeometry(Brushes.Black, null, borderGeometry);

            drawingContext.DrawGeometry(Brushes.Red, null, backgroundGeometry);
        }

        private void BuildGeometry(StreamGeometryContext geometryContext, List<GeometryPoint> points)
        {
            geometryContext.BeginFigure(points[0].Coordinates, true, true);

            for (int i = 1; i < points.Count; i++)
            {
                var point = points[i];

                if (point.LineType == LineType.Line) geometryContext.LineTo(point.Coordinates, true, false);
                else if (point.LineType == LineType.Arc) geometryContext.ArcTo(point.Coordinates, point.ArcSize, 0.0, false, SweepDirection.Clockwise, true, false);
            }
        }

        private void CreateGeometryInfo(double rectangleWidth, double rectangleHeight)
        {
            var arrowWidth = 5d;
            var arrowLength = 15d;

            var cornerRadius = new CornerRadius(5);

            var direction = BeakDirection.Bottom;

            _geometryInfo = new GeometryInfo
            {
                RectangleWidth = rectangleWidth,
                RectangleHeight = rectangleHeight,
                ArrowWidth = arrowWidth,
                ArrowLength = arrowLength,
                BeakDirection = direction,
                CornerRadius = cornerRadius,
            };

            if (_geometryInfo.BeakDirection == BeakDirection.Left) _geometryInfo.RectangleOffsetX = _geometryInfo.ArrowLength;
            if (_geometryInfo.BeakDirection == BeakDirection.Top) _geometryInfo.RectangleOffsetY = _geometryInfo.ArrowLength;
            if (IsDiagonal())
            {
                _geometryInfo.DiagonalArrowInset = Math.Sqrt(Math.Pow(_geometryInfo.ArrowWidth, 2d) * 2d);

                _geometryInfo.DiagonalArrowTipOffset = Math.Sqrt(Math.Pow(_geometryInfo.ArrowLength, 2d) / 2d);

                switch (_geometryInfo.BeakDirection)
                {
                    case BeakDirection.TopLeft:
                        _geometryInfo.RectangleOffsetX = _geometryInfo.DiagonalArrowTipOffset - (_geometryInfo.DiagonalArrowInset / 2d);
                        _geometryInfo.RectangleOffsetY = _geometryInfo.DiagonalArrowTipOffset - (_geometryInfo.DiagonalArrowInset / 2d);
                        break;
                    case BeakDirection.BottomLeft:
                        _geometryInfo.RectangleOffsetX = _geometryInfo.DiagonalArrowTipOffset - (_geometryInfo.DiagonalArrowInset / 2d);
                        break;
                    case BeakDirection.TopRight:
                        _geometryInfo.RectangleOffsetY = _geometryInfo.DiagonalArrowTipOffset - (_geometryInfo.DiagonalArrowInset / 2d);
                        break;
                }
            }
        }

        private void CreateGeometry(List<GeometryPoint> points, Thickness borderThickness)
        {
            // Ecke unten links
            var bottomLeft = new Point(_geometryInfo.RectangleOffsetX, _geometryInfo.RectangleHeight + _geometryInfo.RectangleOffsetY);

            if (_geometryInfo.BeakDirection == BeakDirection.BottomLeft)
            {
                CreateDiagonalArrow(bottomLeft, borderThickness.Bottom, borderThickness.Left, out var arrowStart, out var arrowEnd, out var arrowTip);

                points.Add(GeometryPoint.Line(arrowStart));
                points.Add(GeometryPoint.Line(arrowTip));
                points.Add(GeometryPoint.Line(arrowEnd));
            }
            else
            {
                bottomLeft.Offset(borderThickness.Left, -borderThickness.Bottom);

                if (_geometryInfo.CornerRadius.BottomLeft > 0)
                {
                    var arcSize = new Size(Math.Max(_geometryInfo.CornerRadius.BottomLeft - borderThickness.Left, 0), Math.Max(_geometryInfo.CornerRadius.BottomLeft - borderThickness.Bottom, 0));

                    var arcStart = bottomLeft;
                    arcStart.Offset(arcSize.Width, 0d);

                    var arcEnd = bottomLeft;
                    arcEnd.Offset(0d, -arcSize.Height);

                    points.Add(GeometryPoint.Line(arcStart));
                    points.Add(GeometryPoint.Arc(arcEnd, arcSize));
                }
                else points.Add(GeometryPoint.Line(bottomLeft));
            }

            if (_geometryInfo.BeakDirection == BeakDirection.Left)
            {
                var leftBorderMidPoint = new Point(bottomLeft.X, VerticalMidPoint(borderThickness));
                
                CreateStraightArrow(leftBorderMidPoint, borderThickness.Bottom, out var arrowStart, out var arrowEnd, out var arrowTip);

                points.Add(GeometryPoint.Line(arrowEnd));
                points.Add(GeometryPoint.Line(arrowTip));
                points.Add(GeometryPoint.Line(arrowStart));
            }

            // Ecke oben links
            var topLeft = new Point(_geometryInfo.RectangleOffsetX, _geometryInfo.RectangleOffsetY);

            if (_geometryInfo.BeakDirection == BeakDirection.TopLeft)
            {
                CreateDiagonalArrow(topLeft, borderThickness.Left, borderThickness.Top, out var arrowStart, out var arrowEnd, out var arrowTip);

                points.Add(GeometryPoint.Line(arrowStart));
                points.Add(GeometryPoint.Line(arrowTip));
                points.Add(GeometryPoint.Line(arrowEnd));
            }
            else
            {
                topLeft.Offset(borderThickness.Left, borderThickness.Top);

                if (_geometryInfo.CornerRadius.TopLeft > 0d)
                {
                    var arcSize = new Size(Math.Max(_geometryInfo.CornerRadius.TopLeft - borderThickness.Left, 0), Math.Max(_geometryInfo.CornerRadius.TopLeft - borderThickness.Top, 0));

                    var arcStart = topLeft;
                    arcStart.Offset(0d, arcSize.Height);

                    var arcEnd = topLeft;
                    arcEnd.Offset(arcSize.Width, 0d);

                    points.Add(GeometryPoint.Line(arcStart));
                    points.Add(GeometryPoint.Arc(arcEnd, arcSize));
                }
                else points.Add(GeometryPoint.Line(topLeft));
            }

            if (_geometryInfo.BeakDirection == BeakDirection.Top)
            {
                var topBorderMidPoint = new Point(HorizontalMidPoint(borderThickness), topLeft.Y);

                CreateStraightArrow(topBorderMidPoint, borderThickness.Bottom, out var arrowStart, out var arrowEnd, out var arrowTip);

                points.Add(GeometryPoint.Line(arrowStart));
                points.Add(GeometryPoint.Line(arrowTip));
                points.Add(GeometryPoint.Line(arrowEnd));
            }

            // Ecke oben rechts
            var topRight = new Point(_geometryInfo.RectangleWidth + _geometryInfo.RectangleOffsetX, _geometryInfo.RectangleOffsetY);

            if (_geometryInfo.BeakDirection == BeakDirection.TopRight)
            {
                CreateDiagonalArrow(topRight, borderThickness.Top, borderThickness.Right, out var arrowStart, out var arrowEnd, out var arrowTip);

                points.Add(GeometryPoint.Line(arrowStart));
                points.Add(GeometryPoint.Line(arrowTip));
                points.Add(GeometryPoint.Line(arrowEnd));
            }
            else
            {
                topRight.Offset(-borderThickness.Right, borderThickness.Top);

                if (_geometryInfo.CornerRadius.TopRight > 0d)
                {
                    var arcSize = new Size(Math.Max(_geometryInfo.CornerRadius.TopRight - borderThickness.Right, 0), Math.Max(_geometryInfo.CornerRadius.TopRight - borderThickness.Top, 0));

                    var arcStart = topRight;
                    arcStart.Offset(-arcSize.Width, 0d);

                    var arcEnd = topRight;
                    arcEnd.Offset(0d, arcSize.Height);

                    points.Add(GeometryPoint.Line(arcStart));
                    points.Add(GeometryPoint.Arc(arcEnd, arcSize));
                }
                else points.Add(GeometryPoint.Line(topRight));
            }

            if (_geometryInfo.BeakDirection == BeakDirection.Right)
            {
                var rightBorderMidPoint = new Point(topRight.X, VerticalMidPoint(borderThickness));

                CreateStraightArrow(rightBorderMidPoint, borderThickness.Bottom, out var arrowStart, out var arrowEnd, out var arrowTip);

                points.Add(GeometryPoint.Line(arrowStart));
                points.Add(GeometryPoint.Line(arrowTip));
                points.Add(GeometryPoint.Line(arrowEnd));
            }

            // Ecke unten rechts
            var bottomRight = new Point(_geometryInfo.RectangleWidth + _geometryInfo.RectangleOffsetX, _geometryInfo.RectangleHeight + _geometryInfo.RectangleOffsetY);

            if (_geometryInfo.BeakDirection == BeakDirection.BottomRight)
            {
                CreateDiagonalArrow(bottomRight, borderThickness.Right, borderThickness.Bottom, out var arrowStart, out var arrowEnd, out var arrowTip);

                points.Add(GeometryPoint.Line(arrowStart));
                points.Add(GeometryPoint.Line(arrowTip));
                points.Add(GeometryPoint.Line(arrowEnd));
            }
            else
            {
                bottomRight.Offset(-borderThickness.Right, -borderThickness.Bottom);

                if (_geometryInfo.CornerRadius.BottomRight > 0d)
                {
                    var arcSize = new Size(Math.Max(_geometryInfo.CornerRadius.BottomRight - borderThickness.Right, 0), Math.Max(_geometryInfo.CornerRadius.BottomRight - borderThickness.Bottom, 0));

                    var arcStart = bottomRight;
                    arcStart.Offset(0d, -arcSize.Height);

                    var arcEnd = bottomRight;
                    arcEnd.Offset(-arcSize.Width, 0d);

                    points.Add(GeometryPoint.Line(arcStart));
                    points.Add(GeometryPoint.Arc(arcEnd, arcSize));
                }
                else points.Add(GeometryPoint.Line(bottomRight));
            }

            // Pfeil nach unten
            if (_geometryInfo.BeakDirection == BeakDirection.Bottom)
            {
                var bottomBorderMidPoint = new Point(HorizontalMidPoint(borderThickness), bottomRight.Y);

                CreateStraightArrow(bottomBorderMidPoint, borderThickness.Bottom, out var arrowStart, out var arrowEnd, out var arrowTip);

                points.Add(GeometryPoint.Line(arrowEnd));
                points.Add(GeometryPoint.Line(arrowTip));
                points.Add(GeometryPoint.Line(arrowStart));
            }
        }

        private double CalculateAlpha(double a, double b)
        {
            var c = Math.Sqrt(Math.Pow(a, 2d) + Math.Pow(b, 2d));

            return Math.Asin(a / c);
        }

        private double CalculateAngleThicknessOffset(double angle, double thickness)
        {
            return thickness * (1 - angle / (Math.PI / 2d));
        }

        private Point CalculateIntersectionPoint(Point firstLineFirstPoint, Point firstLineSecondPoint, Point secondLineFirstPoint, Point secondLineSecondPoint)
        {
            var firstMultiplicationValue = firstLineFirstPoint.X * firstLineSecondPoint.Y - firstLineFirstPoint.Y * firstLineSecondPoint.X;
            var secondMultiplicationValue = secondLineSecondPoint.X * secondLineFirstPoint.Y - secondLineSecondPoint.Y * secondLineFirstPoint.X;

            var dvisionValue = (firstLineFirstPoint.X - firstLineSecondPoint.X) * (secondLineSecondPoint.Y - secondLineFirstPoint.Y) - (firstLineFirstPoint.Y - firstLineSecondPoint.Y) * (secondLineSecondPoint.X - secondLineFirstPoint.X);

            var x = ((secondLineSecondPoint.X - secondLineFirstPoint.X) * firstMultiplicationValue - (firstLineFirstPoint.X - firstLineSecondPoint.X) * secondMultiplicationValue) / dvisionValue;

            var y = ((secondLineSecondPoint.Y - secondLineFirstPoint.Y) * firstMultiplicationValue - (firstLineFirstPoint.Y - firstLineSecondPoint.Y) * secondMultiplicationValue) / dvisionValue;

            return new Point(x, y);
        }

        private bool HasBorder(Thickness borderThickness)
        {
            return borderThickness.Left > 0 || borderThickness.Top > 0 || borderThickness.Right > 0 || borderThickness.Bottom > 0;
        }

        private bool IsDiagonal()
        {
            return _geometryInfo.BeakDirection == BeakDirection.TopLeft || _geometryInfo.BeakDirection == BeakDirection.TopRight || _geometryInfo.BeakDirection == BeakDirection.BottomRight || _geometryInfo.BeakDirection == BeakDirection.BottomLeft;
        }

        private bool IsStraightHorizontal()
        {
            return _geometryInfo.BeakDirection == BeakDirection.Left || _geometryInfo.BeakDirection == BeakDirection.Right;
        }

        #region StraightArrow
        private double HorizontalMidPoint(Thickness borderThickness)
        {
            return borderThickness.Left + (_geometryInfo.RectangleWidth - borderThickness.Left - borderThickness.Right) / 2 + _geometryInfo.RectangleOffsetX;
        }

        private double VerticalMidPoint(Thickness borderThickness)
        {
            return borderThickness.Top + (_geometryInfo.RectangleHeight - borderThickness.Top - borderThickness.Bottom) / 2 + _geometryInfo.RectangleOffsetY;
        }

        private void CreateStraightArrow(Point arrowMidPoint, double thickness, out Point arrowStart, out Point arrowEnd, out Point arrowTip)
        {
            StraightArrowFromMidPoint(arrowMidPoint, out arrowStart, out arrowEnd, out arrowTip);

            if (thickness > 0.0) OffsetStraightArrowByThickness(thickness, ref arrowStart, ref arrowEnd, ref arrowTip);
        }

        private void StraightArrowFromMidPoint(Point midPoint, out Point arrowStart, out Point arrowEnd, out Point arrowTip)
        {
            if (IsStraightHorizontal())
            {
                arrowStart = midPoint;
                arrowStart.Offset(0d, -_geometryInfo.ArrowWidth / 2);

                arrowEnd = arrowStart;
                arrowEnd.Offset(0d, _geometryInfo.ArrowWidth);

                switch (_geometryInfo.BeakDirection)
                {
                    case BeakDirection.Left:
                        arrowTip = midPoint;
                        arrowTip.Offset(-_geometryInfo.ArrowLength, 0d);
                        break;
                    case BeakDirection.Right:
                        arrowTip = midPoint;
                        arrowTip.Offset(_geometryInfo.ArrowLength, 0d);
                        break;
                }
            }
            else
            {
                arrowStart = midPoint;
                arrowStart.Offset(-_geometryInfo.ArrowWidth / 2, 0d);

                arrowEnd = arrowStart;
                arrowEnd.Offset(_geometryInfo.ArrowWidth, 0d);

                switch (_geometryInfo.BeakDirection)
                {
                    case BeakDirection.Top:
                        arrowTip = midPoint;
                        arrowTip.Offset(0d, -_geometryInfo.ArrowLength);
                        break;
                    case BeakDirection.Bottom:
                        arrowTip = midPoint;
                        arrowTip.Offset(0d, _geometryInfo.ArrowLength);
                        break;
                }
            }
        }

        private void OffsetStraightArrowByThickness(double thickness, ref Point arrowStart, ref Point arrowEnd, ref Point arrowTip)
        {
            var alpha = CalculateAlpha(_geometryInfo.ArrowWidth / 2, _geometryInfo.ArrowLength);

            var angleThicknessOffset = CalculateAngleThicknessOffset(alpha, thickness);

            var tipThicknessOffset = CalculateStraightArrowTipThicknessOffset(alpha, thickness);

            var isHorizontal = _geometryInfo.BeakDirection == BeakDirection.Left || _geometryInfo.BeakDirection == BeakDirection.Right;

            if (isHorizontal)
            {
                arrowStart.Offset(0d, angleThicknessOffset);
                arrowEnd.Offset(0d, -angleThicknessOffset);

                switch (_geometryInfo.BeakDirection)
                {
                    case BeakDirection.Left:
                        arrowTip.Offset(tipThicknessOffset - thickness, 0d);
                        break;
                    case BeakDirection.Right:
                        arrowTip.Offset(-(tipThicknessOffset - thickness), 0d);
                        break;
                }
            }
            else
            {
                arrowStart.Offset(angleThicknessOffset, 0d);
                arrowEnd.Offset(-angleThicknessOffset, 0d);

                switch (_geometryInfo.BeakDirection)
                {
                    case BeakDirection.Top:
                        arrowTip.Offset(0d, tipThicknessOffset - thickness);
                        break;
                    case BeakDirection.Bottom:
                        arrowTip.Offset(0d, -(tipThicknessOffset - thickness));
                        break;
                }
            }
        }

        private double CalculateStraightArrowTipThicknessOffset(double arrowStartAngle, double thickness)
        {
            return thickness / Math.Sin(arrowStartAngle);
        }
        #endregion

        #region DiagonalArrow
        private void CreateDiagonalArrow(Point arrowCorner, double startThickness, double endThickness, out Point arrowStart, out Point arrowEnd, out Point arrowTip)
        {
            DiagonalArrowFromCornerPoint(arrowCorner, out arrowStart, out arrowTip, out arrowEnd);

            if (startThickness > 0d || endThickness > 0d) OffsetDiagonalArrowBythickness(startThickness, endThickness, ref arrowStart, ref arrowTip, ref arrowEnd);
        }

        private void DiagonalArrowFromCornerPoint(Point arrowCorner, out Point arrowStart, out Point arrowTip, out Point arrowEnd)
        {
            arrowStart = arrowCorner;
            arrowTip = arrowCorner;
            arrowEnd = arrowCorner;

            switch (_geometryInfo.BeakDirection)
            {
                case BeakDirection.TopLeft:
                    arrowStart.Offset(0d, _geometryInfo.DiagonalArrowInset);

                    arrowTip.Offset(_geometryInfo.DiagonalArrowInset / 2d, _geometryInfo.DiagonalArrowInset / 2d);
                    arrowTip.Offset(-_geometryInfo.DiagonalArrowTipOffset, -_geometryInfo.DiagonalArrowTipOffset);

                    arrowEnd.Offset(_geometryInfo.DiagonalArrowInset, 0d);
                    break;
                case BeakDirection.TopRight:
                    arrowStart.Offset(-_geometryInfo.DiagonalArrowInset, 0d);

                    arrowTip.Offset(-_geometryInfo.DiagonalArrowInset / 2d, _geometryInfo.DiagonalArrowInset / 2d);
                    arrowTip.Offset(_geometryInfo.DiagonalArrowTipOffset, -_geometryInfo.DiagonalArrowTipOffset);

                    arrowEnd.Offset(0d, _geometryInfo.DiagonalArrowInset);
                    break;
                case BeakDirection.BottomRight:
                    arrowStart.Offset(0d, -_geometryInfo.DiagonalArrowInset);

                    arrowTip.Offset(-_geometryInfo.DiagonalArrowInset / 2d, -_geometryInfo.DiagonalArrowInset / 2d);
                    arrowTip.Offset(_geometryInfo.DiagonalArrowTipOffset, _geometryInfo.DiagonalArrowTipOffset);

                    arrowEnd.Offset(-_geometryInfo.DiagonalArrowInset, 0d);
                    break;
                case BeakDirection.BottomLeft:
                    arrowStart.Offset(_geometryInfo.DiagonalArrowInset, 0d);

                    arrowTip.Offset(_geometryInfo.DiagonalArrowInset / 2d, -_geometryInfo.DiagonalArrowInset / 2d);
                    arrowTip.Offset(-_geometryInfo.DiagonalArrowTipOffset, _geometryInfo.DiagonalArrowTipOffset);

                    arrowEnd.Offset(0d, -_geometryInfo.DiagonalArrowInset);
                    break;
            }
        }

        private void OffsetDiagonalArrowBythickness(double startThickness, double endThickness, ref Point arrowStart, ref Point arrowTip, ref Point arrowEnd)
        {
            var b = _geometryInfo.DiagonalArrowTipOffset - (_geometryInfo.DiagonalArrowInset / 2d);
            var a = b + _geometryInfo.DiagonalArrowInset;

            var alpha = CalculateAlpha(a, b);

            double startAngleThicknessOffset = 0d;

            if (startThickness > 0d) startAngleThicknessOffset = CalculateAngleThicknessOffset(alpha, startThickness);

            double endAngleThicknessOffset = 0d;

            if (endThickness > 0d) endAngleThicknessOffset = CalculateAngleThicknessOffset(alpha, endThickness);

            switch (_geometryInfo.BeakDirection)
            {
                case BeakDirection.TopLeft:
                    arrowStart.Offset(startThickness, -startAngleThicknessOffset);

                    arrowEnd.Offset(-endAngleThicknessOffset, endThickness);

                    arrowTip = CalculateIntersectionPoint(arrowStart, new Point(arrowTip.X + startThickness, arrowTip.Y - startAngleThicknessOffset), arrowEnd, new Point(arrowTip.X - endAngleThicknessOffset, arrowTip.Y + endThickness));
                    break;
                case BeakDirection.TopRight:
                    arrowStart.Offset(startAngleThicknessOffset, startThickness);

                    arrowEnd.Offset(-endThickness, -endAngleThicknessOffset);

                    arrowTip = CalculateIntersectionPoint(arrowStart, new Point(arrowTip.X + startAngleThicknessOffset, arrowTip.Y + startThickness), arrowEnd, new Point(arrowTip.X - endThickness, arrowTip.Y - endAngleThicknessOffset));
                    break;
                case BeakDirection.BottomRight:
                    arrowStart.Offset(-startThickness, startAngleThicknessOffset);

                    arrowEnd.Offset(endAngleThicknessOffset, -endThickness);

                    arrowTip = CalculateIntersectionPoint(arrowStart, new Point(arrowTip.X - startThickness, arrowTip.Y + startAngleThicknessOffset), arrowEnd, new Point(arrowTip.X + endAngleThicknessOffset, arrowTip.Y - endThickness));
                    break;
                case BeakDirection.BottomLeft:
                    arrowStart.Offset(-startAngleThicknessOffset, -startThickness);

                    arrowEnd.Offset(endThickness, endAngleThicknessOffset);

                    arrowTip = CalculateIntersectionPoint(arrowStart, new Point(arrowTip.X - startAngleThicknessOffset, arrowTip.Y - startThickness), arrowEnd, new Point(arrowTip.X + endThickness, arrowTip.Y + endAngleThicknessOffset));
                    break;
            }
        }
        #endregion
    }
}