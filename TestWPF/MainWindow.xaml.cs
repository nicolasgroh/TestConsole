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
        }

        private enum LineType
        {
            Line,
            Arc
        }

        private struct GeometryPoint
        {
            public GeometryPoint(Point coordinates, LineType lineType, Size arcSize = new Size())
            {
                Coordinates = coordinates;
                LineType = lineType;
                ArcSize = arcSize;
            }

            public Point Coordinates;
            public LineType LineType;
            public Size ArcSize;
        }

        private GeometryInfo _geometryInfo;

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            var width = 60d;
            var height = 30d;

            var arrowWidth = 10d;
            var arrowLength = 15d;

            var borderThickness = new Thickness(2);

            var cornerRadius = new CornerRadius(5);

            var direction = BeakDirection.Top;

            _geometryInfo = new GeometryInfo
            {
                RectangleWidth = width,
                RectangleHeight = height,
                ArrowWidth = arrowWidth,
                ArrowLength = arrowLength,
                BeakDirection = direction,
                CornerRadius = cornerRadius,
            };

            CalculateRectangleOffsets(out _geometryInfo.RectangleOffsetX, out _geometryInfo.RectangleOffsetY);

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

        private void CalculateRectangleOffsets(out double offsetX, out double offsetY)
        {
            offsetX = 0;
            offsetY = 0;

            if (_geometryInfo.BeakDirection == BeakDirection.Left) offsetX = _geometryInfo.ArrowLength;
            if (_geometryInfo.BeakDirection == BeakDirection.Top) offsetY = _geometryInfo.ArrowLength;
        }

        private void CreateGeometry(ref GeometryPoint[] points, Thickness borderThickness)
        {
            var pointIndex = 0;

            // Ecke unten links
            var bottomLeft = new Point(borderThickness.Left + _geometryInfo.RectangleOffsetX, _geometryInfo.RectangleHeight - borderThickness.Bottom + _geometryInfo.RectangleOffsetY);

            if (_geometryInfo.CornerRadius.BottomLeft > 0)
            {
                var arcSize = new Size(Math.Max(_geometryInfo.CornerRadius.BottomLeft - borderThickness.Left, 0), Math.Max(_geometryInfo.CornerRadius.BottomLeft - borderThickness.Bottom, 0));

                var arcStart = bottomLeft;
                arcStart.Offset(arcSize.Width, 0d);

                var arcEnd = bottomLeft;
                arcEnd.Offset(0d, -arcSize.Height);

                points[pointIndex++] = new GeometryPoint(arcStart, LineType.Line);
                points[pointIndex++] = new GeometryPoint(arcEnd, LineType.Arc, arcSize);
            }
            else points[pointIndex++] = new GeometryPoint(bottomLeft, LineType.Line);

            if (_geometryInfo.BeakDirection == BeakDirection.Left)
            {
                var leftBorderMidPoint = new Point(bottomLeft.X, VerticalMidPoint(_geometryInfo.RectangleHeight, borderThickness));

                CreateStraightArrow(ref points, ref pointIndex, _geometryInfo.ArrowWidth, _geometryInfo.ArrowLength, _geometryInfo.BeakDirection, leftBorderMidPoint, borderThickness.Bottom);
            }

            // Ecke oben links
            var topLeft = new Point(borderThickness.Left + _geometryInfo.RectangleOffsetX, borderThickness.Top + _geometryInfo.RectangleOffsetY);

            if (_geometryInfo.BeakDirection == BeakDirection.TopLeft)
            {
                var diagonalArrowInset = Math.Sqrt(Math.Pow(_geometryInfo.ArrowWidth, 2) * 2);

                var arrowStart = topLeft;
                arrowStart.Offset(0d, diagonalArrowInset);

                double alpha = 0;

                if (borderThickness.Left > 0 || borderThickness.Top > 0) alpha = CalculateAlpha(_geometryInfo.ArrowWidth / 2, _geometryInfo.ArrowLength);

                if (borderThickness.Left > 0)
                {
                    var leftAngleThicknessOffset = CalculateAngleThicknessOffset(alpha, borderThickness.Left);

                    arrowStart.Offset(0d, -leftAngleThicknessOffset);
                }

                var arrowTip = topLeft;
                arrowTip.Offset(diagonalArrowInset, diagonalArrowInset);



                var arrowEnd = topLeft;
                arrowEnd.Offset(diagonalArrowInset, 0d);

                if (borderThickness.Top > 0)
                {
                    var topAngleThicknessOffset = CalculateAngleThicknessOffset(alpha, borderThickness.Top);

                    arrowEnd.Offset(-topAngleThicknessOffset, 0d);
                }

                points[pointIndex++] = new GeometryPoint(arrowStart, LineType.Line);
                points[pointIndex++] = new GeometryPoint(arrowTip, LineType.Line);

                points[pointIndex++] = new GeometryPoint(arrowEnd, LineType.Line);
            }
            else
            {
                if (_geometryInfo.CornerRadius.TopLeft > 0)
                {
                    var arcSize = new Size(Math.Max(_geometryInfo.CornerRadius.TopLeft - borderThickness.Left, 0), Math.Max(_geometryInfo.CornerRadius.TopLeft - borderThickness.Top, 0));

                    var arcStart = topLeft;
                    arcStart.Offset(0d, arcSize.Height);

                    var arcEnd = topLeft;
                    arcEnd.Offset(arcSize.Width, 0d);

                    points[pointIndex++] = new GeometryPoint(arcStart, LineType.Line);
                    points[pointIndex++] = new GeometryPoint(arcEnd, LineType.Arc, arcSize);
                }
                else points[pointIndex++] = new GeometryPoint(topLeft, LineType.Line);
            }

            if (_geometryInfo.BeakDirection == BeakDirection.Top)
            {
                var topBorderMidPoint = new Point(HorizontalMidPoint(_geometryInfo.RectangleWidth, borderThickness), topLeft.Y);

                CreateStraightArrow(ref points, ref pointIndex, _geometryInfo.ArrowWidth, _geometryInfo.ArrowLength, _geometryInfo.BeakDirection, topBorderMidPoint, borderThickness.Bottom);
            }

            // Ecke oben rechts
            var topRight = new Point(_geometryInfo.RectangleWidth - borderThickness.Right + _geometryInfo.RectangleOffsetX, borderThickness.Top + _geometryInfo.RectangleOffsetY);

            if (_geometryInfo.CornerRadius.TopRight > 0)
            {
                var arcSize = new Size(Math.Max(_geometryInfo.CornerRadius.TopRight - borderThickness.Right, 0), Math.Max(_geometryInfo.CornerRadius.TopRight - borderThickness.Top, 0));

                var arcStart = topRight;
                arcStart.Offset(-arcSize.Width, 0d);

                var arcEnd = topRight;
                arcEnd.Offset(0d, arcSize.Height);

                points[pointIndex++] = new GeometryPoint(arcStart, LineType.Line);
                points[pointIndex++] = new GeometryPoint(arcEnd, LineType.Arc, arcSize);
            }
            else points[pointIndex++] = new GeometryPoint(topRight, LineType.Line);

            if (_geometryInfo.BeakDirection == BeakDirection.Right)
            {
                var rightBorderMidPoint = new Point(topRight.X, VerticalMidPoint(_geometryInfo.RectangleHeight, borderThickness));

                CreateStraightArrow(ref points, ref pointIndex, _geometryInfo.ArrowWidth, _geometryInfo.ArrowLength, _geometryInfo.BeakDirection, rightBorderMidPoint, borderThickness.Bottom);
            }

            // Ecke unten rechts
            var bottomRight = new Point(_geometryInfo.RectangleWidth - borderThickness.Right + _geometryInfo.RectangleOffsetX, _geometryInfo.RectangleHeight - borderThickness.Bottom + _geometryInfo.RectangleOffsetY);

            if (_geometryInfo.CornerRadius.BottomRight > 0)
            {
                var arcSize = new Size(Math.Max(_geometryInfo.CornerRadius.BottomRight - borderThickness.Right, 0), Math.Max(_geometryInfo.CornerRadius.BottomRight - borderThickness.Bottom, 0));

                var arcStart = bottomRight;
                arcStart.Offset(0d, -arcSize.Height);

                var arcEnd = bottomRight;
                arcEnd.Offset(-arcSize.Width, 0d);

                points[pointIndex++] = new GeometryPoint(arcStart, LineType.Line);
                points[pointIndex++] = new GeometryPoint(arcEnd, LineType.Arc, arcSize);
            }
            else points[pointIndex++] = new GeometryPoint(bottomRight, LineType.Line);

            // Pfeil nach unten
            if (_geometryInfo.BeakDirection == BeakDirection.Bottom)
            {
                var bottomBorderMidPoint = new Point(HorizontalMidPoint(_geometryInfo.RectangleWidth, borderThickness), bottomRight.Y);

                CreateStraightArrow(ref points, ref pointIndex, _geometryInfo.ArrowWidth, _geometryInfo.ArrowLength, _geometryInfo.BeakDirection, bottomBorderMidPoint, borderThickness.Bottom);
            }
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

        #region StraightArrow
        private double HorizontalMidPoint(double width, Thickness borderThickness)
        {
            return borderThickness.Left + (width - borderThickness.Left - borderThickness.Right) / 2 + _geometryInfo.RectangleOffsetX;
        }

        private double VerticalMidPoint(double height, Thickness borderThickness)
        {
            return borderThickness.Top + (height - borderThickness.Top - borderThickness.Bottom) / 2 + _geometryInfo.RectangleOffsetY;
        }

        private void CreateStraightArrow(ref GeometryPoint[] points, ref int pointIndex, double arrowWidth, double arrowLength, BeakDirection direction, Point arrowMidPoint, double thickness)
        {
            StraightArrowFromMidPoint(arrowMidPoint, direction, arrowWidth, arrowLength, out var arrowStart, out var arrowEnd, out var arrowTip);

            if (thickness > 0.0) OffsetStraightArrowByThickness(direction, arrowWidth, arrowLength, thickness, ref arrowStart, ref arrowEnd, ref arrowTip);

            points[pointIndex++] = new GeometryPoint(arrowEnd, LineType.Line);
            points[pointIndex++] = new GeometryPoint(arrowTip, LineType.Line);
            points[pointIndex++] = new GeometryPoint(arrowStart, LineType.Line);
        }

        private void StraightArrowFromMidPoint(Point midPoint, BeakDirection direction, double arrowWidth, double arrowLength, out Point arrowStart, out Point arrowEnd, out Point arrowTip)
        {
            var isHorizontal = direction == BeakDirection.Left || direction == BeakDirection.Right;

            if (isHorizontal)
            {
                arrowStart = midPoint;
                arrowStart.Offset(0d, -arrowWidth / 2);

                arrowEnd = arrowStart;
                arrowEnd.Offset(0d, arrowWidth);

                switch (direction)
                {
                    case BeakDirection.Left:
                        arrowTip = midPoint;
                        arrowTip.Offset(-arrowLength, 0d);
                        break;
                    case BeakDirection.Right:
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
                    case BeakDirection.Top:
                        arrowTip = midPoint;
                        arrowTip.Offset(0d, -arrowLength);
                        break;
                    case BeakDirection.Bottom:
                        arrowTip = midPoint;
                        arrowTip.Offset(0d, arrowLength);
                        break;
                }
            }
        }

        private void OffsetStraightArrowByThickness(BeakDirection direction, double arrowWidth, double arrowLength, double thickness, ref Point arrowStart, ref Point arrowEnd, ref Point arrowTip)
        {
            var alpha = CalculateAlpha(arrowWidth / 2, arrowLength);

            var angleThicknessOffset = CalculateAngleThicknessOffset(alpha, thickness);

            var tipThicknessOffset = CalculateStraightArrowTipThicknessOffset(alpha, thickness);

            var isHorizontal = direction == BeakDirection.Left || direction == BeakDirection.Right;

            if (isHorizontal)
            {
                arrowStart.Offset(0d, angleThicknessOffset);
                arrowEnd.Offset(0d, -angleThicknessOffset);

                switch (direction)
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

                switch (direction)
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
    }
}