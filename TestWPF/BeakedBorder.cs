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
            public double BeakWidth;
            public double BeakLength;
            public BeakDirection BeakDirection;
            public CornerRadius CornerRadius;
            public double DiagonalBeakInset;
            public double DiagonalBeakTipOffset;
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
                BeakWidth = BeakWidth,
                BeakLength = BeakLenght,
                BeakDirection = BeakDirection,
                CornerRadius = CornerRadius
            };

            if (IsDiagonal())
            {
                _geometryInfo.DiagonalBeakInset = Math.Sqrt(Math.Pow(_geometryInfo.BeakWidth, 2d) / 2d);
                _geometryInfo.DiagonalBeakTipOffset = Math.Sqrt(Math.Pow(_geometryInfo.BeakLength, 2d) / 2d);

                _geometryInfo.DiagonalBeakChildRectangleOffset = _geometryInfo.DiagonalBeakTipOffset - (_geometryInfo.DiagonalBeakInset / 2d);
            }

            CalculateChildRectangle(constraint);
        }

        private void CalculateChildRectangle(Size constraint)
        {
            var childRectangle = new Rect(constraint);

            switch (_geometryInfo.BeakDirection)
            {
                case BeakDirection.Left:
                    childRectangle.Width -= _geometryInfo.BeakLength;
                    childRectangle.Offset(_geometryInfo.BeakLength, 0d);
                    break;
                case BeakDirection.TopLeft:
                    childRectangle.Width -= _geometryInfo.DiagonalBeakChildRectangleOffset;
                    childRectangle.Height -= _geometryInfo.DiagonalBeakChildRectangleOffset;
                    childRectangle.Offset(_geometryInfo.DiagonalBeakChildRectangleOffset, _geometryInfo.DiagonalBeakChildRectangleOffset);
                    break;
                case BeakDirection.Top:
                    childRectangle.Height -= _geometryInfo.BeakLength;
                    childRectangle.Offset(0d, _geometryInfo.BeakLength);
                    break;
                case BeakDirection.TopRight:
                    childRectangle.Width -= _geometryInfo.DiagonalBeakChildRectangleOffset;
                    childRectangle.Height -= _geometryInfo.DiagonalBeakChildRectangleOffset;
                    childRectangle.Offset(0d, _geometryInfo.DiagonalBeakChildRectangleOffset);
                    break;
                case BeakDirection.Right:
                    childRectangle.Width -= _geometryInfo.BeakLength;
                    break;
                case BeakDirection.BottomRight:
                    childRectangle.Width -= _geometryInfo.DiagonalBeakChildRectangleOffset;
                    childRectangle.Height -= _geometryInfo.DiagonalBeakChildRectangleOffset;
                    break;
                case BeakDirection.Bottom:
                    childRectangle.Height -= _geometryInfo.BeakLength;
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
                    leftBeakHeight = _geometryInfo.BeakWidth;
                    break;
                case BeakDirection.TopLeft:
                    topLeftCornerRadius = _geometryInfo.DiagonalBeakInset;
                    break;
                case BeakDirection.Top:
                    topBeakWidth = _geometryInfo.BeakWidth;
                    break;
                case BeakDirection.TopRight:
                    topRightCornerRadius = _geometryInfo.DiagonalBeakInset;
                    break;
                case BeakDirection.Right:
                    rightBeakHeight = _geometryInfo.BeakWidth;
                    break;
                case BeakDirection.BottomRight:
                    bottomRightCornerRadius = _geometryInfo.DiagonalBeakInset;
                    break;
                case BeakDirection.Bottom:
                    bottomBeakWidth = _geometryInfo.BeakWidth;
                    break;
                case BeakDirection.BottomLeft:
                    bottomLeftCornerRadius = _geometryInfo.DiagonalBeakInset;
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
                    totalSize.Width += _geometryInfo.BeakLength;
                    break;
                case BeakDirection.TopLeft:
                    totalSize.Width += _geometryInfo.DiagonalBeakChildRectangleOffset;
                    totalSize.Height += _geometryInfo.DiagonalBeakChildRectangleOffset;
                    break;
                case BeakDirection.Top:
                    totalSize.Height += _geometryInfo.BeakLength;
                    break;
                case BeakDirection.TopRight:
                    totalSize.Width += _geometryInfo.DiagonalBeakChildRectangleOffset;
                    totalSize.Height += _geometryInfo.DiagonalBeakChildRectangleOffset;
                    break;
                case BeakDirection.Right:
                    totalSize.Width += _geometryInfo.BeakLength;
                    break;
                case BeakDirection.BottomRight:
                    totalSize.Width += _geometryInfo.DiagonalBeakChildRectangleOffset;
                    totalSize.Height += _geometryInfo.DiagonalBeakChildRectangleOffset;
                    break;
                case BeakDirection.Bottom:
                    totalSize.Height += _geometryInfo.BeakLength;
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
                CreateDiagonalBeak(bottomLeft, borderThickness.Bottom, borderThickness.Left, out var beakStart, out var beakEnd, out var beakTip);

                points.Add(GeometryPoint.Line(beakStart));
                points.Add(GeometryPoint.Line(beakTip));
                points.Add(GeometryPoint.Line(beakEnd));
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

                CreateStraightBeak(leftBorderMidPoint, borderThickness.Bottom, out var beakStart, out var beakEnd, out var beakTip);

                points.Add(GeometryPoint.Line(beakEnd));
                points.Add(GeometryPoint.Line(beakTip));
                points.Add(GeometryPoint.Line(beakStart));
            }

            // Ecke oben links
            var topLeft = new Point(_geometryInfo.RectangleOffsetX, _geometryInfo.RectangleOffsetY);

            if (_geometryInfo.BeakDirection == BeakDirection.TopLeft)
            {
                CreateDiagonalBeak(topLeft, borderThickness.Left, borderThickness.Top, out var beakStart, out var beakEnd, out var beakTip);

                points.Add(GeometryPoint.Line(beakStart));
                points.Add(GeometryPoint.Line(beakTip));
                points.Add(GeometryPoint.Line(beakEnd));
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

                CreateStraightBeak(topBorderMidPoint, borderThickness.Bottom, out var beakStart, out var beakEnd, out var beakTip);

                points.Add(GeometryPoint.Line(beakStart));
                points.Add(GeometryPoint.Line(beakTip));
                points.Add(GeometryPoint.Line(beakEnd));
            }

            // Ecke oben rechts
            var topRight = new Point(_geometryInfo.RectangleWidth + _geometryInfo.RectangleOffsetX, _geometryInfo.RectangleOffsetY);

            if (_geometryInfo.BeakDirection == BeakDirection.TopRight)
            {
                CreateDiagonalBeak(topRight, borderThickness.Top, borderThickness.Right, out var beakStart, out var beakEnd, out var beakTip);

                points.Add(GeometryPoint.Line(beakStart));
                points.Add(GeometryPoint.Line(beakTip));
                points.Add(GeometryPoint.Line(beakEnd));
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

                CreateStraightBeak(rightBorderMidPoint, borderThickness.Bottom, out var beakStart, out var beakEnd, out var beakTip);

                points.Add(GeometryPoint.Line(beakStart));
                points.Add(GeometryPoint.Line(beakTip));
                points.Add(GeometryPoint.Line(beakEnd));
            }

            // Ecke unten rechts
            var bottomRight = new Point(_geometryInfo.RectangleWidth + _geometryInfo.RectangleOffsetX, _geometryInfo.RectangleHeight + _geometryInfo.RectangleOffsetY);

            if (_geometryInfo.BeakDirection == BeakDirection.BottomRight)
            {
                CreateDiagonalBeak(bottomRight, borderThickness.Right, borderThickness.Bottom, out var beakStart, out var beakEnd, out var beakTip);

                points.Add(GeometryPoint.Line(beakStart));
                points.Add(GeometryPoint.Line(beakTip));
                points.Add(GeometryPoint.Line(beakEnd));
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

                CreateStraightBeak(bottomBorderMidPoint, borderThickness.Bottom, out var beakStart, out var beakEnd, out var beakTip);

                points.Add(GeometryPoint.Line(beakEnd));
                points.Add(GeometryPoint.Line(beakTip));
                points.Add(GeometryPoint.Line(beakStart));
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

        #region StraightBeak
        private double HorizontalMidPoint(Thickness borderThickness)
        {
            return borderThickness.Left + (_geometryInfo.RectangleWidth - borderThickness.Left - borderThickness.Right) / 2 + _geometryInfo.RectangleOffsetX;
        }

        private double VerticalMidPoint(Thickness borderThickness)
        {
            return borderThickness.Top + (_geometryInfo.RectangleHeight - borderThickness.Top - borderThickness.Bottom) / 2 + _geometryInfo.RectangleOffsetY;
        }

        private void CreateStraightBeak(Point beakMidPoint, double thickness, out Point beakStart, out Point beakEnd, out Point beakTip)
        {
            StraightBeakFromMidPoint(beakMidPoint, out beakStart, out beakEnd, out beakTip);

            if (thickness > 0.0) OffsetStraightBeakByThickness(thickness, ref beakStart, ref beakEnd, ref beakTip);
        }

        private void StraightBeakFromMidPoint(Point midPoint, out Point beakStart, out Point beakEnd, out Point beakTip)
        {
            if (IsStraightHorizontal())
            {
                beakStart = midPoint;
                beakStart.Offset(0d, -_geometryInfo.BeakWidth / 2);

                beakEnd = beakStart;
                beakEnd.Offset(0d, _geometryInfo.BeakWidth);

                switch (_geometryInfo.BeakDirection)
                {
                    case BeakDirection.Left:
                        beakTip = midPoint;
                        beakTip.Offset(-_geometryInfo.BeakLength, 0d);
                        break;
                    case BeakDirection.Right:
                        beakTip = midPoint;
                        beakTip.Offset(_geometryInfo.BeakLength, 0d);
                        break;
                }
            }
            else
            {
                beakStart = midPoint;
                beakStart.Offset(-_geometryInfo.BeakWidth / 2, 0d);

                beakEnd = beakStart;
                beakEnd.Offset(_geometryInfo.BeakWidth, 0d);

                switch (_geometryInfo.BeakDirection)
                {
                    case BeakDirection.Top:
                        beakTip = midPoint;
                        beakTip.Offset(0d, -_geometryInfo.BeakLength);
                        break;
                    case BeakDirection.Bottom:
                        beakTip = midPoint;
                        beakTip.Offset(0d, _geometryInfo.BeakLength);
                        break;
                }
            }
        }

        private void OffsetStraightBeakByThickness(double thickness, ref Point beakStart, ref Point beakEnd, ref Point beakTip)
        {
            var alpha = CalculateAlpha(_geometryInfo.BeakWidth / 2, _geometryInfo.BeakLength);

            var angleThicknessOffset = CalculateAngleThicknessOffset(alpha, thickness);

            var tipThicknessOffset = CalculateStraightBeakTipThicknessOffset(alpha, thickness);

            var isHorizontal = _geometryInfo.BeakDirection == BeakDirection.Left || _geometryInfo.BeakDirection == BeakDirection.Right;

            if (isHorizontal)
            {
                beakStart.Offset(0d, angleThicknessOffset);
                beakEnd.Offset(0d, -angleThicknessOffset);

                switch (_geometryInfo.BeakDirection)
                {
                    case BeakDirection.Left:
                        beakTip.Offset(tipThicknessOffset - thickness, 0d);
                        break;
                    case BeakDirection.Right:
                        beakTip.Offset(-(tipThicknessOffset - thickness), 0d);
                        break;
                }
            }
            else
            {
                beakStart.Offset(angleThicknessOffset, 0d);
                beakEnd.Offset(-angleThicknessOffset, 0d);

                switch (_geometryInfo.BeakDirection)
                {
                    case BeakDirection.Top:
                        beakTip.Offset(0d, tipThicknessOffset - thickness);
                        break;
                    case BeakDirection.Bottom:
                        beakTip.Offset(0d, -(tipThicknessOffset - thickness));
                        break;
                }
            }
        }

        private double CalculateStraightBeakTipThicknessOffset(double beakStartAngle, double thickness)
        {
            return thickness / Math.Sin(beakStartAngle);
        }
        #endregion

        #region DiagonalBeak
        private void CreateDiagonalBeak(Point beakCorner, double startThickness, double endThickness, out Point beakStart, out Point beakEnd, out Point beakTip)
        {
            DiagonalBeakFromCornerPoint(beakCorner, out beakStart, out beakTip, out beakEnd);

            if (startThickness > 0d || endThickness > 0d) OffsetDiagonalBeakBythickness(startThickness, endThickness, ref beakStart, ref beakTip, ref beakEnd);
        }

        private void DiagonalBeakFromCornerPoint(Point beakCorner, out Point beakStart, out Point beakTip, out Point beakEnd)
        {
            beakStart = beakCorner;
            beakTip = beakCorner;
            beakEnd = beakCorner;

            switch (_geometryInfo.BeakDirection)
            {
                case BeakDirection.TopLeft:
                    beakStart.Offset(0d, _geometryInfo.DiagonalBeakInset);

                    beakTip.Offset(_geometryInfo.DiagonalBeakInset / 2d, _geometryInfo.DiagonalBeakInset / 2d);
                    beakTip.Offset(-_geometryInfo.DiagonalBeakTipOffset, -_geometryInfo.DiagonalBeakTipOffset);

                    beakEnd.Offset(_geometryInfo.DiagonalBeakInset, 0d);
                    break;
                case BeakDirection.TopRight:
                    beakStart.Offset(-_geometryInfo.DiagonalBeakInset, 0d);

                    beakTip.Offset(-_geometryInfo.DiagonalBeakInset / 2d, _geometryInfo.DiagonalBeakInset / 2d);
                    beakTip.Offset(_geometryInfo.DiagonalBeakTipOffset, -_geometryInfo.DiagonalBeakTipOffset);

                    beakEnd.Offset(0d, _geometryInfo.DiagonalBeakInset);
                    break;
                case BeakDirection.BottomRight:
                    beakStart.Offset(0d, -_geometryInfo.DiagonalBeakInset);

                    beakTip.Offset(-_geometryInfo.DiagonalBeakInset / 2d, -_geometryInfo.DiagonalBeakInset / 2d);
                    beakTip.Offset(_geometryInfo.DiagonalBeakTipOffset, _geometryInfo.DiagonalBeakTipOffset);

                    beakEnd.Offset(-_geometryInfo.DiagonalBeakInset, 0d);
                    break;
                case BeakDirection.BottomLeft:
                    beakStart.Offset(_geometryInfo.DiagonalBeakInset, 0d);

                    beakTip.Offset(_geometryInfo.DiagonalBeakInset / 2d, -_geometryInfo.DiagonalBeakInset / 2d);
                    beakTip.Offset(-_geometryInfo.DiagonalBeakTipOffset, _geometryInfo.DiagonalBeakTipOffset);

                    beakEnd.Offset(0d, -_geometryInfo.DiagonalBeakInset);
                    break;
            }
        }

        private void OffsetDiagonalBeakBythickness(double startThickness, double endThickness, ref Point beakStart, ref Point beakTip, ref Point beakEnd)
        {
            var b = _geometryInfo.DiagonalBeakTipOffset - (_geometryInfo.DiagonalBeakInset / 2d);
            var a = b + _geometryInfo.DiagonalBeakInset;

            var alpha = CalculateAlpha(a, b);

            double startAngleThicknessOffset = 0d;

            if (startThickness > 0d) startAngleThicknessOffset = CalculateAngleThicknessOffset(alpha, startThickness);

            double endAngleThicknessOffset = 0d;

            if (endThickness > 0d) endAngleThicknessOffset = CalculateAngleThicknessOffset(alpha, endThickness);

            switch (_geometryInfo.BeakDirection)
            {
                case BeakDirection.TopLeft:
                    beakStart.Offset(startThickness, -startAngleThicknessOffset);

                    beakEnd.Offset(-endAngleThicknessOffset, endThickness);

                    beakTip = CalculateIntersectionPoint(beakStart, new Point(beakTip.X + startThickness, beakTip.Y - startAngleThicknessOffset), beakEnd, new Point(beakTip.X - endAngleThicknessOffset, beakTip.Y + endThickness));
                    break;
                case BeakDirection.TopRight:
                    beakStart.Offset(startAngleThicknessOffset, startThickness);

                    beakEnd.Offset(-endThickness, -endAngleThicknessOffset);

                    beakTip = CalculateIntersectionPoint(beakStart, new Point(beakTip.X + startAngleThicknessOffset, beakTip.Y + startThickness), beakEnd, new Point(beakTip.X - endThickness, beakTip.Y - endAngleThicknessOffset));
                    break;
                case BeakDirection.BottomRight:
                    beakStart.Offset(-startThickness, startAngleThicknessOffset);

                    beakEnd.Offset(endAngleThicknessOffset, -endThickness);

                    beakTip = CalculateIntersectionPoint(beakStart, new Point(beakTip.X - startThickness, beakTip.Y + startAngleThicknessOffset), beakEnd, new Point(beakTip.X + endAngleThicknessOffset, beakTip.Y - endThickness));
                    break;
                case BeakDirection.BottomLeft:
                    beakStart.Offset(-startAngleThicknessOffset, -startThickness);

                    beakEnd.Offset(endThickness, endAngleThicknessOffset);

                    beakTip = CalculateIntersectionPoint(beakStart, new Point(beakTip.X - startAngleThicknessOffset, beakTip.Y - startThickness), beakEnd, new Point(beakTip.X + endThickness, beakTip.Y + endAngleThicknessOffset));
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