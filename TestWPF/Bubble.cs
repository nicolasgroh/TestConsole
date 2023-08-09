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
    public class Bubble : Decorator
    {
        private struct GeometryCash
        {
            public Thickness BorderThickness;

            public CornerRadius CornerRadius;

            public BubbleDirection BubbleDirection;

            public double BubbleLenght;

            public double BubbleWidth;
        }

        public static readonly DependencyProperty BackgroundProperty = DependencyProperty.Register("Background", typeof(Brush), typeof(Bubble), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
        public Brush Background
        {
            get { return (Brush)GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }

        public static readonly DependencyProperty BorderBrushProperty = DependencyProperty.Register("BorderBrush", typeof(Brush), typeof(Bubble), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
        public Brush BorderBrush
        {
            get { return (Brush)GetValue(BorderBrushProperty); }
            set { SetValue(BorderBrushProperty, value); }
        }

        public static readonly DependencyProperty BorderThicknessProperty = DependencyProperty.Register("BorderThickness", typeof(Thickness), typeof(Bubble), new FrameworkPropertyMetadata(new Thickness(0), FrameworkPropertyMetadataOptions.AffectsMeasure));
        public Thickness BorderThickness
        {
            get { return (Thickness)GetValue(BorderThicknessProperty); }
            set { SetValue(BorderThicknessProperty, value); }
        }

        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(Bubble), new FrameworkPropertyMetadata(new CornerRadius(0), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));
        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        public static readonly DependencyProperty BubbleDirectionProperty = DependencyProperty.Register("BubbleDirection", typeof(BubbleDirection), typeof(Bubble), new FrameworkPropertyMetadata(BubbleDirection.Top, FrameworkPropertyMetadataOptions.AffectsMeasure));
        public BubbleDirection BubbleDirection
        {
            get { return (BubbleDirection)GetValue(BubbleDirectionProperty); }
            set { SetValue(BubbleDirectionProperty, value); }
        }

        public static readonly DependencyProperty BubbleLenghtProperty = DependencyProperty.Register("BubbleLenght", typeof(double), typeof(Bubble), new FrameworkPropertyMetadata(10.0, FrameworkPropertyMetadataOptions.AffectsMeasure));
        public double BubbleLenght
        {
            get { return (double)GetValue(BubbleLenghtProperty); }
            set { SetValue(BubbleLenghtProperty, value); }
        }

        public static readonly DependencyProperty BubbleWidthProperty = DependencyProperty.Register("BubbleWidth", typeof(double), typeof(Bubble), new FrameworkPropertyMetadata(10.0, FrameworkPropertyMetadataOptions.AffectsMeasure));
        public double BubbleWidth
        {
            get { return (double)GetValue(BubbleWidthProperty); }
            set { SetValue(BubbleWidthProperty, value); }
        }

        private GeometryCash _geometryCash;

        private StreamGeometry _borderGeometry;
        private StreamGeometry _backgroundGreometry;

        private void CalculateBubbleOffset(out double bubbleWidthOffset, out double bubbleHeightOffset)
        {
            var diagonalBubbleLenght = Math.Sqrt(Math.Pow(_geometryCash.BubbleLenght, 2) / 2);

            var diagonalBubbleWidthOffset = Math.Sqrt(_geometryCash.BubbleWidth * _geometryCash.BubbleWidth / 2) / 2;

            bubbleWidthOffset = 0.0;
            bubbleHeightOffset = 0.0;

            switch (_geometryCash.BubbleDirection)
            {
                case BubbleDirection.Left:
                case BubbleDirection.Right:
                    bubbleWidthOffset = _geometryCash.BubbleLenght;
                    break;

                case BubbleDirection.Top:
                case BubbleDirection.Bottom:
                    bubbleHeightOffset = _geometryCash.BubbleLenght;
                    break;
                case BubbleDirection.TopLeft:
                case BubbleDirection.TopRight:
                case BubbleDirection.BottomRight:
                case BubbleDirection.BottomLeft:
                    bubbleWidthOffset = diagonalBubbleLenght - diagonalBubbleWidthOffset;
                    bubbleHeightOffset = diagonalBubbleLenght - diagonalBubbleWidthOffset;
                    break;
            }
        }

        private void ApplyBubbleOffset(ref Point topLeft, ref Point bottomRight)
        {
            var diagonalBubbleLenght = Math.Sqrt(Math.Pow(_geometryCash.BubbleLenght, 2) / 2);

            var diagonalBubbleWidthOffset = Math.Sqrt(_geometryCash.BubbleWidth * _geometryCash.BubbleWidth / 2) / 2;

            switch (_geometryCash.BubbleDirection)
            {
                case BubbleDirection.Left:
                    topLeft.X += _geometryCash.BubbleLenght;
                    break;
                case BubbleDirection.TopLeft:
                    topLeft.Y += diagonalBubbleLenght - diagonalBubbleWidthOffset;
                    topLeft.X += diagonalBubbleLenght - diagonalBubbleWidthOffset;
                    break;
                case BubbleDirection.Top:
                    topLeft.Y += _geometryCash.BubbleLenght;
                    break;
                case BubbleDirection.TopRight:
                    topLeft.Y += diagonalBubbleLenght - diagonalBubbleWidthOffset;
                    bottomRight.X -= diagonalBubbleLenght - diagonalBubbleWidthOffset;
                    break;
                case BubbleDirection.Right:
                    bottomRight.X -= _geometryCash.BubbleLenght;
                    break;
                case BubbleDirection.BottomRight:
                    bottomRight.X -= diagonalBubbleLenght - diagonalBubbleWidthOffset;
                    bottomRight.Y -= diagonalBubbleLenght - diagonalBubbleWidthOffset;
                    break;
                case BubbleDirection.Bottom:
                    bottomRight.Y -= _geometryCash.BubbleLenght;
                    break;
                case BubbleDirection.BottomLeft:
                    topLeft.X += diagonalBubbleLenght - diagonalBubbleWidthOffset;
                    bottomRight.Y -= diagonalBubbleLenght - diagonalBubbleWidthOffset;
                    break;
            }
        }

        private void CalculcateBorderOffset(out double borderWidthOffset, out double borderHeightOffset)
        {
            borderWidthOffset = _geometryCash.BorderThickness.Left + _geometryCash.BorderThickness.Right;
            borderHeightOffset = _geometryCash.BorderThickness.Top + _geometryCash.BorderThickness.Bottom;
        }

        private void ApplyBorderOffset(ref Point topLeft, ref Point bottomRight)
        {
            topLeft.X += _geometryCash.BorderThickness.Left;
            topLeft.Y += _geometryCash.BorderThickness.Top;
            bottomRight.X -= _geometryCash.BorderThickness.Right;
            bottomRight.Y -= _geometryCash.BorderThickness.Bottom;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            _geometryCash = new GeometryCash
            {
                BorderThickness = BorderThickness,
                CornerRadius = CornerRadius,
                BubbleDirection = BubbleDirection,
                BubbleLenght = BubbleLenght,
                BubbleWidth = BubbleWidth
            };

            CalculateBubbleOffset(out double bubbleWidthOffset, out double bubbleHeightOffset);

            CalculcateBorderOffset(out double borderWidthOffset, out double borderHeightOffset);

            Size childSize;

            var child = Child;

            if (child == null) childSize = new Size();
            else
            {
                var childConstraint = new Size(Math.Max(constraint.Width - bubbleWidthOffset - borderWidthOffset, 0), Math.Max(constraint.Height - bubbleHeightOffset - borderHeightOffset, 0));

                child.Measure(childConstraint);

                childSize = child.DesiredSize;
            }

            return new Size(bubbleWidthOffset + borderWidthOffset + childSize.Width, bubbleHeightOffset + borderHeightOffset + childSize.Height);
        }

        private void CreateGeometry(StreamGeometryContext geometryContext, Point topLeft, Point bottomRight, Thickness borderThickness)
        {
            var leftBorderStart = new Point(topLeft.X, bottomRight.Y - Math.Max(_geometryCash.CornerRadius.BottomLeft - borderThickness.Bottom, 0));
            var leftBorderEnd = new Point(topLeft.X, topLeft.Y + Math.Max(_geometryCash.CornerRadius.TopLeft - borderThickness.Top, 0));

            var topBorderStart = new Point(topLeft.X + Math.Max(_geometryCash.CornerRadius.TopLeft - borderThickness.Left, 0), topLeft.Y);
            var topBorderEnd = new Point(bottomRight.X - Math.Max(_geometryCash.CornerRadius.TopRight - borderThickness.Right, 0), topLeft.Y);

            var rightBorderStart = new Point(bottomRight.X, topLeft.Y + Math.Max(_geometryCash.CornerRadius.TopRight - borderThickness.Top, 0));
            var rightBorderEnd = new Point(bottomRight.X, bottomRight.Y - Math.Max(_geometryCash.CornerRadius.BottomRight - borderThickness.Bottom, 0));

            var bottomBorderStart = new Point(bottomRight.X - Math.Max(_geometryCash.CornerRadius.BottomRight - borderThickness.Right, 0), bottomRight.Y);
            var bottomBorderEnd = new Point(topLeft.X + Math.Max(_geometryCash.CornerRadius.BottomLeft - borderThickness.Left, 0), bottomRight.Y);

            var diagonalBubbleWidth = Math.Sqrt(_geometryCash.BubbleWidth * _geometryCash.BubbleWidth / 2);
            var diagonalBubbleWidthOffset = diagonalBubbleWidth / 2;
            var diagonalBubbleLenght = Math.Sqrt(Math.Pow(_geometryCash.BubbleLenght, 2) / 2);

            var xMidpoint = (bottomRight.X + borderThickness.Right - topLeft.X - borderThickness.Left) / 2 + topLeft.X;
            var yMidpoint = (bottomRight.Y + borderThickness.Bottom - topLeft.Y - borderThickness.Top) / 2 + topLeft.Y;

            // Left
            geometryContext.BeginFigure(leftBorderStart, true, true);

            if (_geometryCash.BubbleDirection == BubbleDirection.TopLeft)
            {
                var firstAngleOffset = borderThickness.Left * (1 - (topLeft.X - borderThickness.Left) / (topLeft.X + diagonalBubbleLenght));

                var thirdAngleOffset = borderThickness.Top * (1 - diagonalBubbleLenght / (diagonalBubbleLenght + (topLeft.X - borderThickness.Left)));

                var bubbleStartPoint = new Point(topLeft.X, topLeft.Y - borderThickness.Top + diagonalBubbleWidth - firstAngleOffset);
                var bubbleEndPoint = new Point(topLeft.X - borderThickness.Left + diagonalBubbleWidth - thirdAngleOffset, topLeft.Y);

                geometryContext.LineTo(bubbleStartPoint, true, false);

                var actualTopLeft = new Point(topLeft.X - (diagonalBubbleLenght - diagonalBubbleWidthOffset) - borderThickness.Left, topLeft.Y - (diagonalBubbleLenght - diagonalBubbleWidthOffset) - borderThickness.Top);

                var firstCalculationPoint = new Point(actualTopLeft.X, actualTopLeft.Y - borderThickness.Left - firstAngleOffset);
                var secondCalculationPoint = new Point(actualTopLeft.X - borderThickness.Top - thirdAngleOffset, actualTopLeft.Y);

                var firstDistance = Math.Sqrt(Math.Pow(firstCalculationPoint.X - secondCalculationPoint.X, 2) + Math.Pow(secondCalculationPoint.Y - firstCalculationPoint.Y, 2));
                var secondDistance = Math.Sqrt(Math.Pow(bubbleEndPoint.X - bubbleStartPoint.X, 2) + Math.Pow(bubbleStartPoint.Y - bubbleEndPoint.Y, 2));

                var ratio = 0.0;

                if (firstDistance > 0)
                {
                    var total = firstDistance + secondDistance;

                    ratio = firstDistance / total;
                }

                var medianCalculationDistance = Math.Sqrt(Math.Pow(bubbleEndPoint.X - firstCalculationPoint.X, 2) + Math.Pow(bubbleEndPoint.Y - firstCalculationPoint.Y, 2)) * ratio;

                var medianOffset = Math.Sqrt(medianCalculationDistance * medianCalculationDistance / 2);

                var firstMedian = new Point(firstCalculationPoint.X + medianOffset, firstCalculationPoint.Y + medianOffset);
                var secondMedian = new Point(secondCalculationPoint.X + medianOffset, secondCalculationPoint.Y + medianOffset);

                var centerMedianOffset = Math.Sqrt(Math.Pow(firstMedian.X - secondMedian.X, 2) + Math.Pow(secondMedian.Y - firstMedian.Y, 2)) * 0.5;

                var median = new Point(secondMedian.X + (firstMedian.X - secondMedian.X) / 2, firstMedian.Y + (secondMedian.Y - firstMedian.Y) / 2);

                geometryContext.LineTo(median, true, false);
                
                geometryContext.LineTo(bubbleEndPoint, true, false);
            }
            else
            {
                if (_geometryCash.BubbleDirection == BubbleDirection.Left)
                {
                    var yAngleOffset = borderThickness.Left * (1 - _geometryCash.BubbleWidth / (_geometryCash.BubbleWidth + _geometryCash.BubbleLenght));
                    var xAngleOffset = borderThickness.Left * _geometryCash.BubbleLenght / _geometryCash.BubbleWidth;

                    geometryContext.LineTo(new Point(leftBorderEnd.X, yMidpoint + _geometryCash.BubbleWidth / 2 - yAngleOffset), true, false);

                    geometryContext.LineTo(new Point(leftBorderEnd.X - _geometryCash.BubbleLenght + xAngleOffset, yMidpoint), true, false);

                    geometryContext.LineTo(new Point(leftBorderEnd.X, yMidpoint - _geometryCash.BubbleWidth / 2 + yAngleOffset), true, false);
                }

                geometryContext.LineTo(leftBorderEnd, true, false);

                if (_geometryCash.CornerRadius.TopLeft > 0 && _geometryCash.BubbleDirection != BubbleDirection.TopLeft)
                {
                    geometryContext.ArcTo(topBorderStart,
                        new Size(Math.Max(_geometryCash.CornerRadius.TopLeft - borderThickness.Left, 0), Math.Max(_geometryCash.CornerRadius.TopLeft - borderThickness.Top, 0)),
                        0.0, false, SweepDirection.Clockwise, true, false);
                }
            }

            // Top
            if (_geometryCash.BubbleDirection == BubbleDirection.Top)
            {
                var yAngleOffset = borderThickness.Top * _geometryCash.BubbleLenght / _geometryCash.BubbleWidth;
                var xAngleOffset = borderThickness.Top * (1 - _geometryCash.BubbleWidth / (_geometryCash.BubbleWidth + _geometryCash.BubbleLenght));

                geometryContext.LineTo(new Point(xMidpoint - _geometryCash.BubbleWidth / 2 + xAngleOffset, topBorderEnd.Y), true, false);

                geometryContext.LineTo(new Point(xMidpoint, topBorderEnd.Y - _geometryCash.BubbleLenght + yAngleOffset), true, false);

                geometryContext.LineTo(new Point(xMidpoint + _geometryCash.BubbleWidth / 2 - xAngleOffset, topBorderEnd.Y), true, false);
            }

            geometryContext.LineTo(topBorderEnd, true, false);

            if (_geometryCash.CornerRadius.TopRight > 0 && _geometryCash.BubbleDirection != BubbleDirection.TopRight)
            {
                geometryContext.ArcTo(rightBorderStart,
                    new Size(Math.Max(_geometryCash.CornerRadius.TopRight - borderThickness.Right, 0), Math.Max(_geometryCash.CornerRadius.TopRight - borderThickness.Top, 0)),
                    0.0, false, SweepDirection.Clockwise, true, false);
            }

            // Right
            if (_geometryCash.BubbleDirection == BubbleDirection.Right)
            {
                var yAngleOffset = borderThickness.Right * (1 - _geometryCash.BubbleWidth / (_geometryCash.BubbleWidth + _geometryCash.BubbleLenght));
                var xAngleOffset = borderThickness.Right * _geometryCash.BubbleLenght / _geometryCash.BubbleWidth;

                geometryContext.LineTo(new Point(rightBorderEnd.X, yMidpoint - _geometryCash.BubbleWidth / 2 + yAngleOffset), true, false);

                geometryContext.LineTo(new Point(rightBorderEnd.X + _geometryCash.BubbleLenght - xAngleOffset, yMidpoint), true, false);

                geometryContext.LineTo(new Point(rightBorderEnd.X, yMidpoint + _geometryCash.BubbleWidth / 2 - yAngleOffset), true, false);
            }

            geometryContext.LineTo(rightBorderEnd, true, false);

            if (_geometryCash.CornerRadius.BottomRight > 0 && _geometryCash.BubbleDirection != BubbleDirection.BottomRight)
            {
                geometryContext.ArcTo(bottomBorderStart,
                    new Size(Math.Max(_geometryCash.CornerRadius.BottomRight - borderThickness.Right, 0), Math.Max(_geometryCash.CornerRadius.BottomRight - borderThickness.Bottom, 0)),
                    0.0, false, SweepDirection.Clockwise, true, false);
            }

            // Bottom
            if (_geometryCash.BubbleDirection == BubbleDirection.Bottom)
            {
                var yAngleOffset = borderThickness.Bottom * _geometryCash.BubbleLenght / _geometryCash.BubbleWidth;
                var xAngleOffset = borderThickness.Bottom * (1 - _geometryCash.BubbleWidth / (_geometryCash.BubbleWidth + _geometryCash.BubbleLenght));

                geometryContext.LineTo(new Point(xMidpoint + _geometryCash.BubbleWidth / 2 - xAngleOffset, bottomBorderEnd.Y), true, false);

                geometryContext.LineTo(new Point(xMidpoint, bottomBorderEnd.Y + _geometryCash.BubbleLenght - yAngleOffset), true, false);

                geometryContext.LineTo(new Point(xMidpoint - _geometryCash.BubbleWidth / 2 + xAngleOffset, bottomBorderEnd.Y), true, false);
            }

            geometryContext.LineTo(bottomBorderEnd, true, false);

            if (_geometryCash.CornerRadius.BottomLeft > 0 && _geometryCash.BubbleDirection != BubbleDirection.BottomLeft)
            {
                geometryContext.ArcTo(new Point(topLeft.X, bottomRight.Y - Math.Max(_geometryCash.CornerRadius.BottomLeft - borderThickness.Bottom, 0)),
                    new Size(Math.Max(_geometryCash.CornerRadius.BottomLeft - borderThickness.Left, 0), Math.Max(_geometryCash.CornerRadius.BottomLeft - borderThickness.Bottom, 0)),
                    0.0, false, SweepDirection.Clockwise, true, false);
            }
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            _geometryCash = new GeometryCash
            {
                BorderThickness = BorderThickness,
                CornerRadius = CornerRadius,
                BubbleDirection = BubbleDirection,
                BubbleLenght = BubbleLenght,
                BubbleWidth = BubbleWidth
            };

            Point borderTopLeft = new Point(0, 0), borderBottomRight = new Point(arrangeSize.Width, arrangeSize.Height);

            ApplyBubbleOffset(ref borderTopLeft, ref borderBottomRight);

            Point backgroundTopLeft = borderTopLeft, backgroundBottomRight = borderBottomRight;

            ApplyBorderOffset(ref backgroundTopLeft, ref backgroundBottomRight);

            var child = Child;

            if (child != null)
            {
                child.Arrange(new Rect(backgroundTopLeft, backgroundBottomRight));
            }

            _borderGeometry = null;

            if (_geometryCash.BorderThickness.Left > 0 || _geometryCash.BorderThickness.Top > 0 || _geometryCash.BorderThickness.Right > 0 || _geometryCash.BorderThickness.Bottom > 0)
            {
                var geometry = new StreamGeometry();

                using (StreamGeometryContext geometryContext = geometry.Open())
                {
                    CreateGeometry(geometryContext, borderTopLeft, borderBottomRight, new Thickness());

                    CreateGeometry(geometryContext, backgroundTopLeft, backgroundBottomRight, _geometryCash.BorderThickness);
                }

                _borderGeometry = geometry;
            }

            _backgroundGreometry = null;

            if (Background != null)
            {
                var geometry = new StreamGeometry();

                using (StreamGeometryContext geometryContext = geometry.Open())
                {
                    CreateGeometry(geometryContext, backgroundTopLeft, backgroundBottomRight, _geometryCash.BorderThickness);
                }

                _backgroundGreometry = geometry;
            }

            return arrangeSize;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (_borderGeometry != null) drawingContext.DrawGeometry(BorderBrush, null, _borderGeometry);

            if (_backgroundGreometry != null) drawingContext.DrawGeometry(Background, null, _backgroundGreometry);
        }
    }

    public enum BubbleDirection
    {
        None,
        Left,
        TopLeft,
        Top,
        TopRight,
        Right,
        BottomRight,
        Bottom,
        BottomLeft
    }
}