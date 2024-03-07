using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TestWPF
{
    public enum BeakDirection
    {
        None,
        Left,
        TopLeft,
        Top,
        TopRight,
        Right,
        BottomRight,
        Bottom,
        BottomLeft,
    }

    public class BeakedBorder : Decorator
    {
        private struct GeometryInfo
        {
            public double ArrowWidth;
            public double ArrowLength;
            public BeakDirection BeakDirection;
            public CornerRadius CornerRadius;
            public double DiagonalArrowInset;
            public double DiagonalArrowTipOffset;
            public Rect ChildRectangle;
            public double DiagonalBeakChildRectangleOffset;

            public double RectangleOffsetX { get { return ChildRectangle.Left; } }

            public double RectangleOffsetY { get { return ChildRectangle.Top; } }

            public double RectangleWidth { get { return ChildRectangle.Width; } }

            public double RectangleHeight {  get { return ChildRectangle.Height; } }
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

        private const int MAXIMUM_POINTS_COUNT = 11;

        static BeakedBorder()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BeakedBorder), new FrameworkPropertyMetadata(typeof(BeakedBorder)));
        }

        #region DependencyProperties
        public static readonly DependencyProperty BackgroundProperty = DependencyProperty.Register("Background", typeof(Brush), typeof(BeakedBorder), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
        public Brush Background
        {
            get { return (Brush)GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }

        public static readonly DependencyProperty BorderBrushProperty = DependencyProperty.Register("BorderBrush", typeof(Brush), typeof(BeakedBorder), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
        public Brush BorderBrush
        {
            get { return (Brush)GetValue(BorderBrushProperty); }
            set { SetValue(BorderBrushProperty, value); }
        }

        public static readonly DependencyProperty BorderThicknessProperty = DependencyProperty.Register("BorderThickness", typeof(Thickness), typeof(BeakedBorder), new FrameworkPropertyMetadata(new Thickness(0), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsRender));
        public Thickness BorderThickness
        {
            get { return (Thickness)GetValue(BorderThicknessProperty); }
            set { SetValue(BorderThicknessProperty, value); }
        }

        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(BeakedBorder), new FrameworkPropertyMetadata(new CornerRadius(0), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsRender));
        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        public static readonly DependencyProperty BeakDirectionProperty = DependencyProperty.Register("BeakDirection", typeof(BeakDirection), typeof(BeakedBorder), new FrameworkPropertyMetadata(BeakDirection.Top, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsRender));
        public BeakDirection BeakDirection
        {
            get { return (BeakDirection)GetValue(BeakDirectionProperty); }
            set { SetValue(BeakDirectionProperty, value); }
        }

        public static readonly DependencyProperty BeakLenghtProperty = DependencyProperty.Register("BeakLenght", typeof(double), typeof(BeakedBorder), new FrameworkPropertyMetadata(10.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsRender));
        public double BeakLenght
        {
            get { return (double)GetValue(BeakLenghtProperty); }
            set { SetValue(BeakLenghtProperty, value); }
        }

        public static readonly DependencyProperty BeakWidthProperty = DependencyProperty.Register("BeakWidth", typeof(double), typeof(BeakedBorder), new FrameworkPropertyMetadata(10.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsRender));
        public double BeakWidth
        {
            get { return (double)GetValue(BeakWidthProperty); }
            set { SetValue(BeakWidthProperty, value); }
        }
        #endregion

        private GeometryInfo _geometryInfo;
        private StreamGeometry _borderGeometry;
        private StreamGeometry _backgroundGreometry;

        protected override Size MeasureOverride(Size constraint)
        {
            CreateGeometryInfo(constraint);

            var child = Child;

            var childSize = new Size();

            if (child != null)
            {
                child.Measure(_geometryInfo.ChildRectangle.Size);
                childSize = child.DesiredSize;
            }

            CalculateMinimumChildSize(out var minimumChildSize);

            _geometryInfo.ChildRectangle.Size = new Size(Math.Max(minimumChildSize.Width, childSize.Width), Math.Max(minimumChildSize.Height, childSize.Height));

            return CalculateTotalSize();
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            CalculateChildRectangle(arrangeSize);

            var child = Child;

            child?.Arrange(_geometryInfo.ChildRectangle);

            var background = Background;
            var borderBrush = BorderBrush;

            if (borderBrush == null && background == null) return CalculateTotalSize();

            var borderThickness = BorderThickness;

            var innerGeometryPoints = new List<GeometryPoint>(MAXIMUM_POINTS_COUNT);

            CreateGeometry(innerGeometryPoints, borderThickness);

            List<GeometryPoint> outerGeometryPoints;

            if (HasBorder(borderThickness) && borderBrush != null)
            {
                outerGeometryPoints = new List<GeometryPoint>(MAXIMUM_POINTS_COUNT);

                CreateGeometry(outerGeometryPoints, new Thickness());

                _borderGeometry = new StreamGeometry();

                using (StreamGeometryContext borderGeometryContext = _borderGeometry.Open())
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
            }
            else _borderGeometry = null;

            if (background != null)
            {
                _backgroundGreometry = new StreamGeometry();

                using (StreamGeometryContext backgroundGeometryContext = _backgroundGreometry.Open())
                {
                    if (innerGeometryPoints.Count > 0)
                    {
                        BuildGeometry(backgroundGeometryContext, innerGeometryPoints);
                    }
                }
            }
            else _backgroundGreometry = null;

            return CalculateTotalSize();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (_borderGeometry != null) drawingContext.DrawGeometry(BorderBrush, null, _borderGeometry);

            if (_backgroundGreometry != null) drawingContext.DrawGeometry(Background, null, _backgroundGreometry);
        }

        private void CreateGeometryInfo(Size constraint)
        {
            _geometryInfo = new GeometryInfo
            {
                ArrowWidth = BeakWidth,
                ArrowLength = BeakLenght,
                BeakDirection = BeakDirection,
                CornerRadius = CornerRadius
            };

            if (IsDiagonal())
            {
                _geometryInfo.DiagonalArrowInset = Math.Sqrt(Math.Pow(_geometryInfo.ArrowWidth, 2d) / 2d);
                _geometryInfo.DiagonalArrowTipOffset = Math.Sqrt(Math.Pow(_geometryInfo.ArrowLength, 2d) / 2d);

                _geometryInfo.DiagonalBeakChildRectangleOffset = _geometryInfo.DiagonalArrowTipOffset - (_geometryInfo.DiagonalArrowInset / 2d);
            }

            CalculateChildRectangle(constraint);
        }

        private void CalculateChildRectangle(Size constraint)
        {
            var childRectangle = new Rect(constraint);

            switch (_geometryInfo.BeakDirection)
            {
                case BeakDirection.Left:
                    childRectangle.Width -= _geometryInfo.ArrowLength;
                    childRectangle.Offset(_geometryInfo.ArrowLength, 0d);
                    break;
                case BeakDirection.TopLeft:
                    childRectangle.Width -= _geometryInfo.DiagonalBeakChildRectangleOffset;
                    childRectangle.Height -= _geometryInfo.DiagonalBeakChildRectangleOffset;
                    childRectangle.Offset(_geometryInfo.DiagonalBeakChildRectangleOffset, _geometryInfo.DiagonalBeakChildRectangleOffset);
                    break;
                case BeakDirection.Top:
                    childRectangle.Height -= _geometryInfo.ArrowLength;
                    childRectangle.Offset(0d, _geometryInfo.ArrowLength);
                    break;
                case BeakDirection.TopRight:
                    childRectangle.Width -= _geometryInfo.DiagonalBeakChildRectangleOffset;
                    childRectangle.Height -= _geometryInfo.DiagonalBeakChildRectangleOffset;
                    childRectangle.Offset(0d, _geometryInfo.DiagonalBeakChildRectangleOffset);
                    break;
                case BeakDirection.Right:
                    childRectangle.Width -= _geometryInfo.ArrowLength;
                    break;
                case BeakDirection.BottomRight:
                    childRectangle.Width -= _geometryInfo.DiagonalBeakChildRectangleOffset;
                    childRectangle.Height -= _geometryInfo.DiagonalBeakChildRectangleOffset;
                    break;
                case BeakDirection.Bottom:
                    childRectangle.Height -= _geometryInfo.ArrowLength;
                    break;
                case BeakDirection.BottomLeft:
                    childRectangle.Width -= _geometryInfo.DiagonalBeakChildRectangleOffset;
                    childRectangle.Height -= _geometryInfo.DiagonalBeakChildRectangleOffset;
                    childRectangle.Offset(_geometryInfo.DiagonalBeakChildRectangleOffset, 0d);
                    break;
            }

            _geometryInfo.ChildRectangle = childRectangle;
        }

        private void CalculateMinimumChildSize(out Size minimumChildSize)
        {
            double topLeftCornerRadius = _geometryInfo.CornerRadius.TopLeft;
            double topRightCornerRadius = _geometryInfo.CornerRadius.TopRight;
            double bottomRightCornerRadius = _geometryInfo.CornerRadius.BottomRight;
            double bottomLeftCornerRadius = _geometryInfo.CornerRadius.BottomLeft;

            double leftBeakHeight = 0d;
            double topBeakWidth = 0d;
            double rightBeakHeight = 0d;
            double bottomBeakWidth = 0d;

            switch (_geometryInfo.BeakDirection)
            {
                case BeakDirection.Left:
                    leftBeakHeight = _geometryInfo.ArrowWidth;
                    break;
                case BeakDirection.TopLeft:
                    topLeftCornerRadius = _geometryInfo.DiagonalArrowInset;
                    break;
                case BeakDirection.Top:
                    topBeakWidth = _geometryInfo.ArrowWidth;
                    break;
                case BeakDirection.TopRight:
                    topRightCornerRadius = _geometryInfo.DiagonalArrowInset;
                    break;
                case BeakDirection.Right:
                    rightBeakHeight = _geometryInfo.ArrowWidth;
                    break;
                case BeakDirection.BottomRight:
                    bottomRightCornerRadius = _geometryInfo.DiagonalArrowInset;
                    break;
                case BeakDirection.Bottom:
                    bottomBeakWidth = _geometryInfo.ArrowWidth;
                    break;
                case BeakDirection.BottomLeft:
                    bottomLeftCornerRadius = _geometryInfo.DiagonalArrowInset;
                    break;
            }

            var minimumWidth = Math.Max(topLeftCornerRadius + topRightCornerRadius + topBeakWidth, bottomLeftCornerRadius + bottomRightCornerRadius + bottomBeakWidth);
            var minimumHeight = Math.Max(topLeftCornerRadius + bottomLeftCornerRadius + leftBeakHeight, topRightCornerRadius + bottomRightCornerRadius + rightBeakHeight);

            minimumChildSize = new Size(minimumWidth, minimumHeight);
        }

        private Size CalculateTotalSize()
        {
            var totalSize = _geometryInfo.ChildRectangle.Size;

            switch (_geometryInfo.BeakDirection)
            {
                case BeakDirection.Left:
                    totalSize.Width += _geometryInfo.ArrowLength;
                    break;
                case BeakDirection.TopLeft:
                    totalSize.Width += _geometryInfo.DiagonalBeakChildRectangleOffset;
                    totalSize.Height += _geometryInfo.DiagonalBeakChildRectangleOffset;
                    break;
                case BeakDirection.Top:
                    totalSize.Height += _geometryInfo.ArrowLength;
                    break;
                case BeakDirection.TopRight:
                    totalSize.Width += _geometryInfo.DiagonalBeakChildRectangleOffset;
                    totalSize.Height += _geometryInfo.DiagonalBeakChildRectangleOffset;
                    break;
                case BeakDirection.Right:
                    totalSize.Width += _geometryInfo.ArrowLength;
                    break;
                case BeakDirection.BottomRight:
                    totalSize.Width += _geometryInfo.DiagonalBeakChildRectangleOffset;
                    totalSize.Height += _geometryInfo.DiagonalBeakChildRectangleOffset;
                    break;
                case BeakDirection.Bottom:
                    totalSize.Height += _geometryInfo.ArrowLength;
                    break;
                case BeakDirection.BottomLeft:
                    totalSize.Width += _geometryInfo.DiagonalBeakChildRectangleOffset;
                    totalSize.Height += _geometryInfo.DiagonalBeakChildRectangleOffset;
                    break;
            }

            return totalSize;
        }

        private bool HasBorder(Thickness borderThickness)
        {
            return borderThickness.Left > 0 || borderThickness.Top > 0 || borderThickness.Right > 0 || borderThickness.Bottom > 0;
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
    }
}