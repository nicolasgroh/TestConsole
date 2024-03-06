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
            public GeometryPoint(Point coordinates, LineType lineType, Size arcSize = default)
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

            var arrowWidth = 5d;
            var arrowLength = 15d;

            var borderThickness = new Thickness(2, 2, 2, 2);

            var cornerRadius = new CornerRadius(5);

            var direction = BeakDirection.BottomLeft;

            _geometryInfo = new GeometryInfo
            {
                RectangleWidth = width,
                RectangleHeight = height,
                ArrowWidth = arrowWidth,
                ArrowLength = arrowLength,
                BeakDirection = direction,
                CornerRadius = cornerRadius,
            };

            CalculateRectangleOffsets(out _geometryInfo.RectangleOffsetX, out _geometryInfo.RectangleOffsetY, out _geometryInfo.DiagonalArrowInset, out _geometryInfo.DiagonalArrowTipOffset);

            var pointsCount = CalculatePointsCount();

            var innerGeometryPoints = new GeometryPoint[pointsCount];

            CreateGeometry(ref innerGeometryPoints, borderThickness);

            GeometryPoint[] outerGeometryPoints;

            if (borderThickness.Left > 0 || borderThickness.Top > 0 || borderThickness.Right > 0 || borderThickness.Bottom > 0)
            {
                outerGeometryPoints = new GeometryPoint[pointsCount];

                CreateGeometry(ref outerGeometryPoints, new Thickness());
            }
            else outerGeometryPoints = Array.Empty<GeometryPoint>();

            var borderGeometry = new StreamGeometry();

            using (StreamGeometryContext borderGeometryContext = borderGeometry.Open())
            {
                if (innerGeometryPoints.Length > 0)
                {
                    BuildGeometry(borderGeometryContext, innerGeometryPoints);
                }

                if (outerGeometryPoints.Length > 0)
                {
                    BuildGeometry(borderGeometryContext, outerGeometryPoints);
                }
            }

            drawingContext.DrawGeometry(Brushes.Black, null, borderGeometry);

            var backgroundGeometry = new StreamGeometry();

            using (StreamGeometryContext backgroundGeometryContext = backgroundGeometry.Open())
            {
                if (innerGeometryPoints.Length > 0)
                {
                    BuildGeometry(backgroundGeometryContext, innerGeometryPoints);
                }
            }

            drawingContext.DrawGeometry(Brushes.Red, null, backgroundGeometry);
        }

        private void BuildGeometry(StreamGeometryContext geometryContext, GeometryPoint[] points)
        {
            geometryContext.BeginFigure(points[0].Coordinates, true, true);

            for (int i = 1; i < points.Length; i++)
            {
                var point = points[i];

                if (point.LineType == LineType.Line) geometryContext.LineTo(point.Coordinates, true, false);
                else if (point.LineType == LineType.Arc) geometryContext.ArcTo(point.Coordinates, point.ArcSize, 0.0, false, SweepDirection.Clockwise, true, false);
            }
        }

        private int CalculatePointsCount()
        {
            var pointsCount = 4;

            if ((_geometryInfo.BeakDirection & BeakDirection.Diagonal) > 0)
            {
                pointsCount += 2;

                switch (_geometryInfo.BeakDirection)
                {
                    case BeakDirection.TopLeft:
                        if (_geometryInfo.CornerRadius.TopLeft > 0) pointsCount -= 1;
                        break;
                    case BeakDirection.TopRight:
                        if (_geometryInfo.CornerRadius.TopRight > 0) pointsCount -= 1;
                        break;
                    case BeakDirection.BottomRight:
                        if (_geometryInfo.CornerRadius.BottomRight > 0) pointsCount -= 1;
                        break;
                    case BeakDirection.BottomLeft:
                        if (_geometryInfo.CornerRadius.BottomLeft > 0) pointsCount -= 1;
                        break;
                }
            }
            else if ((_geometryInfo.BeakDirection & BeakDirection.Straight) > 0) pointsCount += 3;

            var cornerRadii = new double[4];

            cornerRadii[0] = _geometryInfo.CornerRadius.TopLeft;
            cornerRadii[1] = _geometryInfo.CornerRadius.TopRight;
            cornerRadii[2] = _geometryInfo.CornerRadius.BottomRight;
            cornerRadii[3] = _geometryInfo.CornerRadius.BottomLeft;

            for (int i = 0; i < cornerRadii.Length; i++)
            {
                if (cornerRadii[i] > 0) pointsCount += 1;
            }

            return pointsCount;
        }

        private void CalculateRectangleOffsets(out double offsetX, out double offsetY, out double diagonalArrowInset, out double diagonalArrowTipOffset)
        {
            offsetX = 0d;
            offsetY = 0d;
            diagonalArrowInset = 0d;
            diagonalArrowTipOffset = 0d;

            if (_geometryInfo.BeakDirection == BeakDirection.Left) offsetX = _geometryInfo.ArrowLength;
            if (_geometryInfo.BeakDirection == BeakDirection.Top) offsetY = _geometryInfo.ArrowLength;
            if ((_geometryInfo.BeakDirection & BeakDirection.Diagonal) > 0)
            {
                diagonalArrowInset = Math.Sqrt(Math.Pow(_geometryInfo.ArrowWidth, 2d) * 2d);

                diagonalArrowTipOffset = Math.Sqrt(Math.Pow(_geometryInfo.ArrowLength, 2d) / 2d);

                switch (_geometryInfo.BeakDirection)
                {
                    case BeakDirection.TopLeft:
                        offsetX = diagonalArrowTipOffset - (diagonalArrowInset / 2d);
                        offsetY = diagonalArrowTipOffset - (diagonalArrowInset / 2d);
                        break;
                    case BeakDirection.BottomLeft:
                        offsetX = diagonalArrowTipOffset - (diagonalArrowInset / 2d);
                        break;
                    case BeakDirection.TopRight:
                        offsetY = diagonalArrowTipOffset - (diagonalArrowInset / 2d);
                        break;
                }
            }
        }

        private void CreateGeometry(ref GeometryPoint[] points, Thickness borderThickness)
        {
            var pointIndex = 0;

            // Ecke unten links
            var bottomLeft = new Point(_geometryInfo.RectangleOffsetX, _geometryInfo.RectangleHeight + _geometryInfo.RectangleOffsetY);

            if (_geometryInfo.BeakDirection == BeakDirection.BottomLeft)
            {
                CreateDiagonalArrow(ref points, ref pointIndex, bottomLeft, borderThickness.Bottom, borderThickness.Left);
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

                    points[pointIndex++] = GeometryPoint.Line(arcStart);
                    points[pointIndex++] = GeometryPoint.Arc(arcEnd, arcSize);
                }
                else points[pointIndex++] = GeometryPoint.Line(bottomLeft);
            }

            if (_geometryInfo.BeakDirection == BeakDirection.Left)
            {
                var leftBorderMidPoint = new Point(bottomLeft.X, VerticalMidPoint(borderThickness));
                
                CreateStraightArrow(ref points, ref pointIndex, leftBorderMidPoint, borderThickness.Bottom);
            }

            // Ecke oben links
            var topLeft = new Point(_geometryInfo.RectangleOffsetX, _geometryInfo.RectangleOffsetY);

            if (_geometryInfo.BeakDirection == BeakDirection.TopLeft)
            {
                CreateDiagonalArrow(ref points, ref pointIndex, topLeft, borderThickness.Left, borderThickness.Top);
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

                    points[pointIndex++] = GeometryPoint.Line(arcStart);
                    points[pointIndex++] = GeometryPoint.Arc(arcEnd, arcSize);
                }
                else points[pointIndex++] = GeometryPoint.Line(topLeft);
            }

            if (_geometryInfo.BeakDirection == BeakDirection.Top)
            {
                var topBorderMidPoint = new Point(HorizontalMidPoint(borderThickness), topLeft.Y);

                CreateStraightArrow(ref points, ref pointIndex, topBorderMidPoint, borderThickness.Bottom);
            }

            // Ecke oben rechts
            var topRight = new Point(_geometryInfo.RectangleWidth + _geometryInfo.RectangleOffsetX, _geometryInfo.RectangleOffsetY);

            if (_geometryInfo.BeakDirection == BeakDirection.TopRight)
            {
                CreateDiagonalArrow(ref points, ref pointIndex, topRight, borderThickness.Top, borderThickness.Right);
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

                    points[pointIndex++] = GeometryPoint.Line(arcStart);
                    points[pointIndex++] = GeometryPoint.Arc(arcEnd, arcSize);
                }
                else points[pointIndex++] = GeometryPoint.Line(topRight);
            }

            if (_geometryInfo.BeakDirection == BeakDirection.Right)
            {
                var rightBorderMidPoint = new Point(topRight.X, VerticalMidPoint(borderThickness));

                CreateStraightArrow(ref points, ref pointIndex, rightBorderMidPoint, borderThickness.Bottom);
            }

            // Ecke unten rechts
            var bottomRight = new Point(_geometryInfo.RectangleWidth + _geometryInfo.RectangleOffsetX, _geometryInfo.RectangleHeight + _geometryInfo.RectangleOffsetY);

            if (_geometryInfo.BeakDirection == BeakDirection.BottomRight)
            {
                CreateDiagonalArrow(ref points, ref pointIndex, bottomRight, borderThickness.Right, borderThickness.Bottom);
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

                    points[pointIndex++] = GeometryPoint.Line(arcStart);
                    points[pointIndex++] = GeometryPoint.Arc(arcEnd, arcSize);
                }
                else points[pointIndex++] = GeometryPoint.Line(bottomRight);
            }

            // Pfeil nach unten
            if (_geometryInfo.BeakDirection == BeakDirection.Bottom)
            {
                var bottomBorderMidPoint = new Point(HorizontalMidPoint(borderThickness), bottomRight.Y);

                CreateStraightArrow(ref points, ref pointIndex, bottomBorderMidPoint, borderThickness.Bottom);
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

        #region StraightArrow
        private double HorizontalMidPoint(Thickness borderThickness)
        {
            return borderThickness.Left + (_geometryInfo.RectangleWidth - borderThickness.Left - borderThickness.Right) / 2 + _geometryInfo.RectangleOffsetX;
        }

        private double VerticalMidPoint(Thickness borderThickness)
        {
            return borderThickness.Top + (_geometryInfo.RectangleHeight - borderThickness.Top - borderThickness.Bottom) / 2 + _geometryInfo.RectangleOffsetY;
        }

        private void CreateStraightArrow(ref GeometryPoint[] points, ref int pointIndex, Point arrowMidPoint, double thickness)
        {
            StraightArrowFromMidPoint(arrowMidPoint, out var arrowStart, out var arrowEnd, out var arrowTip);

            if (thickness > 0.0) OffsetStraightArrowByThickness(thickness, ref arrowStart, ref arrowEnd, ref arrowTip);

            points[pointIndex++] = GeometryPoint.Line(arrowEnd);
            points[pointIndex++] = GeometryPoint.Line(arrowTip);
            points[pointIndex++] = GeometryPoint.Line(arrowStart);
        }

        private void StraightArrowFromMidPoint(Point midPoint, out Point arrowStart, out Point arrowEnd, out Point arrowTip)
        {
            if ((_geometryInfo.BeakDirection & BeakDirection.StraightHorizontal) > 0)
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
        private void CreateDiagonalArrow(ref GeometryPoint[] points, ref int pointIndex, Point arrowCorner, double startThickness, double endThickness)
        {
            DiagonalArrowFromCornerPoint(arrowCorner, out var arrowStart, out var arrowTip, out var arrowEnd);

            if (startThickness > 0d || endThickness > 0d) OffsetDiagonalArrowBythickness(startThickness, endThickness, ref arrowStart, ref arrowTip, ref arrowEnd);

            points[pointIndex++] = GeometryPoint.Line(arrowStart);
            points[pointIndex++] = GeometryPoint.Line(arrowTip);
            points[pointIndex++] = GeometryPoint.Line(arrowEnd);
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