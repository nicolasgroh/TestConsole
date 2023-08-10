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
    public class Balloon : Decorator
    {
        private struct GeometryCash
        {
            public Thickness BorderThickness;

            public CornerRadius CornerRadius;

            public BalloonDirection BalloonDirection;

            public double BalloonLenght;

            public double BalloonWidth;
        }

        public static readonly DependencyProperty BackgroundProperty = DependencyProperty.Register("Background", typeof(Brush), typeof(Balloon), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
        public Brush Background
        {
            get { return (Brush)GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }

        public static readonly DependencyProperty BorderBrushProperty = DependencyProperty.Register("BorderBrush", typeof(Brush), typeof(Balloon), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
        public Brush BorderBrush
        {
            get { return (Brush)GetValue(BorderBrushProperty); }
            set { SetValue(BorderBrushProperty, value); }
        }

        public static readonly DependencyProperty BorderThicknessProperty = DependencyProperty.Register("BorderThickness", typeof(Thickness), typeof(Balloon), new FrameworkPropertyMetadata(new Thickness(0), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsRender));
        public Thickness BorderThickness
        {
            get { return (Thickness)GetValue(BorderThicknessProperty); }
            set { SetValue(BorderThicknessProperty, value); }
        }

        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(Balloon), new FrameworkPropertyMetadata(new CornerRadius(0), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsRender));
        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        public static readonly DependencyProperty BalloonDirectionProperty = DependencyProperty.Register("BalloonDirection", typeof(BalloonDirection), typeof(Balloon), new FrameworkPropertyMetadata(BalloonDirection.Top, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsRender));
        public BalloonDirection BalloonDirection
        {
            get { return (BalloonDirection)GetValue(BalloonDirectionProperty); }
            set { SetValue(BalloonDirectionProperty, value); }
        }

        public static readonly DependencyProperty BalloonLenghtProperty = DependencyProperty.Register("BalloonLenght", typeof(double), typeof(Balloon), new FrameworkPropertyMetadata(10.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsRender));
        public double BalloonLenght
        {
            get { return (double)GetValue(BalloonLenghtProperty); }
            set { SetValue(BalloonLenghtProperty, value); }
        }

        public static readonly DependencyProperty BalloonWidthProperty = DependencyProperty.Register("BalloonWidth", typeof(double), typeof(Balloon), new FrameworkPropertyMetadata(10.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsRender));
        public double BalloonWidth
        {
            get { return (double)GetValue(BalloonWidthProperty); }
            set { SetValue(BalloonWidthProperty, value); }
        }

        private GeometryCash _geometryCash;

        private StreamGeometry _borderGeometry;
        private StreamGeometry _backgroundGreometry;

        private void CalculateBalloonOffset(out double balloonWidthOffset, out double balloonHeightOffset)
        {
            var diagonalBalloonLenght = Math.Sqrt(Math.Pow(_geometryCash.BalloonLenght, 2) / 2);

            var diagonalBalloonWidthOffset = Math.Sqrt(_geometryCash.BalloonWidth * _geometryCash.BalloonWidth / 2) / 2;

            balloonWidthOffset = 0.0;
            balloonHeightOffset = 0.0;

            switch (_geometryCash.BalloonDirection)
            {
                case BalloonDirection.Left:
                case BalloonDirection.Right:
                    balloonWidthOffset = _geometryCash.BalloonLenght;
                    break;

                case BalloonDirection.Top:
                case BalloonDirection.Bottom:
                    balloonHeightOffset = _geometryCash.BalloonLenght;
                    break;
                case BalloonDirection.TopLeft:
                case BalloonDirection.TopRight:
                case BalloonDirection.BottomRight:
                case BalloonDirection.BottomLeft:
                    balloonWidthOffset = diagonalBalloonLenght - diagonalBalloonWidthOffset;
                    balloonHeightOffset = diagonalBalloonLenght - diagonalBalloonWidthOffset;
                    break;
            }
        }

        private void ApplyBalloonOffset(ref Point topLeft, ref Point bottomRight)
        {
            var diagonalBalloonLenght = Math.Sqrt(Math.Pow(_geometryCash.BalloonLenght, 2) / 2);

            var diagonalBalloonWidthOffset = Math.Sqrt(_geometryCash.BalloonWidth * _geometryCash.BalloonWidth / 2) / 2;

            switch (_geometryCash.BalloonDirection)
            {
                case BalloonDirection.Left:
                    topLeft.X += _geometryCash.BalloonLenght;
                    break;
                case BalloonDirection.TopLeft:
                    topLeft.Y += diagonalBalloonLenght - diagonalBalloonWidthOffset;
                    topLeft.X += diagonalBalloonLenght - diagonalBalloonWidthOffset;
                    break;
                case BalloonDirection.Top:
                    topLeft.Y += _geometryCash.BalloonLenght;
                    break;
                case BalloonDirection.TopRight:
                    topLeft.Y += diagonalBalloonLenght - diagonalBalloonWidthOffset;
                    bottomRight.X -= diagonalBalloonLenght - diagonalBalloonWidthOffset;
                    break;
                case BalloonDirection.Right:
                    bottomRight.X -= _geometryCash.BalloonLenght;
                    break;
                case BalloonDirection.BottomRight:
                    bottomRight.X -= diagonalBalloonLenght - diagonalBalloonWidthOffset;
                    bottomRight.Y -= diagonalBalloonLenght - diagonalBalloonWidthOffset;
                    break;
                case BalloonDirection.Bottom:
                    bottomRight.Y -= _geometryCash.BalloonLenght;
                    break;
                case BalloonDirection.BottomLeft:
                    topLeft.X += diagonalBalloonLenght - diagonalBalloonWidthOffset;
                    bottomRight.Y -= diagonalBalloonLenght - diagonalBalloonWidthOffset;
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
                BalloonDirection = BalloonDirection,
                BalloonLenght = BalloonLenght,
                BalloonWidth = BalloonWidth
            };

            CalculateBalloonOffset(out double balloonWidthOffset, out double balloonHeightOffset);

            CalculcateBorderOffset(out double borderWidthOffset, out double borderHeightOffset);

            Size childSize;

            var child = Child;

            if (child == null) childSize = new Size();
            else
            {
                var childConstraint = new Size(Math.Max(constraint.Width - balloonWidthOffset - borderWidthOffset, 0), Math.Max(constraint.Height - balloonHeightOffset - borderHeightOffset, 0));

                child.Measure(childConstraint);

                childSize = child.DesiredSize;
            }

            return new Size(balloonWidthOffset + borderWidthOffset + childSize.Width, balloonHeightOffset + borderHeightOffset + childSize.Height);
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

            var diagonalBalloonWidth = Math.Sqrt(_geometryCash.BalloonWidth * _geometryCash.BalloonWidth / 2);
            var diagonalBalloonWidthOffset = diagonalBalloonWidth / 2;
            var diagonalBalloonLenght = Math.Sqrt(Math.Pow(_geometryCash.BalloonLenght, 2) / 2);

            var xMidpoint = (bottomRight.X + borderThickness.Right - topLeft.X - borderThickness.Left) / 2 + topLeft.X;
            var yMidpoint = (bottomRight.Y + borderThickness.Bottom - topLeft.Y - borderThickness.Top) / 2 + topLeft.Y;

            // Left
            geometryContext.BeginFigure(leftBorderStart, true, true);

            if (_geometryCash.BalloonDirection == BalloonDirection.TopLeft)
            {
                var firstAngleOffset = borderThickness.Left * (1 - (topLeft.X - borderThickness.Left) / (topLeft.X + diagonalBalloonLenght));

                var thirdAngleOffset = borderThickness.Top * (1 - diagonalBalloonLenght / (diagonalBalloonLenght + (topLeft.X - borderThickness.Left)));

                var balloonStartPoint = new Point(topLeft.X, topLeft.Y - borderThickness.Top + diagonalBalloonWidth - firstAngleOffset);
                var balloonEndPoint = new Point(topLeft.X - borderThickness.Left + diagonalBalloonWidth - thirdAngleOffset, topLeft.Y);

                geometryContext.LineTo(balloonStartPoint, true, false);

                var actualTargetCorner = new Point(topLeft.X - (diagonalBalloonLenght - diagonalBalloonWidthOffset) - borderThickness.Left, topLeft.Y - (diagonalBalloonLenght - diagonalBalloonWidthOffset) - borderThickness.Top);

                var firstCalculationPoint = new Point(actualTargetCorner.X, actualTargetCorner.Y - borderThickness.Left - firstAngleOffset);
                var secondCalculationPoint = new Point(actualTargetCorner.X - borderThickness.Top - thirdAngleOffset, actualTargetCorner.Y);

                var firstDistance = Math.Sqrt(Math.Pow(firstCalculationPoint.X - secondCalculationPoint.X, 2) + Math.Pow(secondCalculationPoint.Y - firstCalculationPoint.Y, 2));
                var secondDistance = Math.Sqrt(Math.Pow(balloonEndPoint.X - balloonStartPoint.X, 2) + Math.Pow(balloonStartPoint.Y - balloonEndPoint.Y, 2));

                var total = firstDistance + secondDistance;

                var ratio = firstDistance / total;

                var medianCalculationDistance = Math.Sqrt(Math.Pow(balloonEndPoint.X - firstCalculationPoint.X, 2) + Math.Pow(balloonEndPoint.Y - firstCalculationPoint.Y, 2)) * ratio;

                var medianOffset = Math.Sqrt(medianCalculationDistance * medianCalculationDistance / 2);

                var firstMedian = new Point(firstCalculationPoint.X + medianOffset, firstCalculationPoint.Y + medianOffset);
                var secondMedian = new Point(secondCalculationPoint.X + medianOffset, secondCalculationPoint.Y + medianOffset);

                var median = new Point(secondMedian.X + (firstMedian.X - secondMedian.X) / 2, firstMedian.Y + (secondMedian.Y - firstMedian.Y) / 2);
                
                geometryContext.LineTo(median, true, false);

                geometryContext.LineTo(balloonEndPoint, true, false);
            }
            else
            {
                if (_geometryCash.BalloonDirection == BalloonDirection.Left)
                {
                    var yAngleOffset = borderThickness.Left * (1 - _geometryCash.BalloonWidth / (_geometryCash.BalloonWidth + _geometryCash.BalloonLenght));
                    var xAngleOffset = borderThickness.Left * _geometryCash.BalloonLenght / _geometryCash.BalloonWidth;

                    geometryContext.LineTo(new Point(leftBorderEnd.X, yMidpoint + _geometryCash.BalloonWidth / 2 - yAngleOffset), true, false);

                    geometryContext.LineTo(new Point(leftBorderEnd.X - _geometryCash.BalloonLenght + xAngleOffset, yMidpoint), true, false);

                    geometryContext.LineTo(new Point(leftBorderEnd.X, yMidpoint - _geometryCash.BalloonWidth / 2 + yAngleOffset), true, false);
                }

                geometryContext.LineTo(leftBorderEnd, true, false);

                if (_geometryCash.CornerRadius.TopLeft > 0)
                {
                    geometryContext.ArcTo(topBorderStart,
                        new Size(Math.Max(_geometryCash.CornerRadius.TopLeft - borderThickness.Left, 0), Math.Max(_geometryCash.CornerRadius.TopLeft - borderThickness.Top, 0)),
                        0.0, false, SweepDirection.Clockwise, true, false);
                }
            }

            // Top
            if (_geometryCash.BalloonDirection == BalloonDirection.Top)
            {
                var yAngleOffset = borderThickness.Top * _geometryCash.BalloonLenght / _geometryCash.BalloonWidth;
                var xAngleOffset = borderThickness.Top * (1 - _geometryCash.BalloonWidth / (_geometryCash.BalloonWidth + _geometryCash.BalloonLenght));

                geometryContext.LineTo(new Point(xMidpoint - _geometryCash.BalloonWidth / 2 + xAngleOffset, topBorderEnd.Y), true, false);

                geometryContext.LineTo(new Point(xMidpoint, topBorderEnd.Y - _geometryCash.BalloonLenght + yAngleOffset), true, false);

                geometryContext.LineTo(new Point(xMidpoint + _geometryCash.BalloonWidth / 2 - xAngleOffset, topBorderEnd.Y), true, false);
            }

            if (_geometryCash.BalloonDirection == BalloonDirection.TopRight)
            {
                var firstAngleOffset = borderThickness.Top * (1 - diagonalBalloonLenght / (diagonalBalloonLenght + (bottomRight.X + borderThickness.Right)));

                var thirdAngleOffset = borderThickness.Right * (1 - (bottomRight.X + borderThickness.Right) / (bottomRight.X + diagonalBalloonLenght));

                var balloonStartPoint = new Point(bottomRight.X + borderThickness.Right - diagonalBalloonWidth + firstAngleOffset, topLeft.Y);
                var balloonEndPoint = new Point(bottomRight.X, topLeft.Y - borderThickness.Top + diagonalBalloonWidth - thirdAngleOffset);

                geometryContext.LineTo(balloonStartPoint, true, false);

                var actualTargetCornern = new Point(bottomRight.X + (diagonalBalloonLenght - diagonalBalloonWidthOffset) + borderThickness.Right, topLeft.Y - (diagonalBalloonLenght - diagonalBalloonWidthOffset) - borderThickness.Top);

                var firstCalculationPoint = new Point(actualTargetCornern.X + borderThickness.Top + firstAngleOffset, actualTargetCornern.Y);
                var secondCalculationPoint = new Point(actualTargetCornern.X, actualTargetCornern.Y - borderThickness.Right - thirdAngleOffset);

                var firstDistance = Math.Sqrt(Math.Pow(firstCalculationPoint.X - secondCalculationPoint.X, 2) + Math.Pow(firstCalculationPoint.Y - secondCalculationPoint.Y, 2));
                var secondDistance = Math.Sqrt(Math.Pow(balloonEndPoint.X - balloonStartPoint.X, 2) + Math.Pow(balloonEndPoint.Y - balloonStartPoint.Y, 2));

                var total = firstDistance + secondDistance;

                var ratio = firstDistance / total;

                var medianCalculationDistance = Math.Sqrt(Math.Pow(firstCalculationPoint.X - balloonEndPoint.X, 2) + Math.Pow(balloonEndPoint.Y - firstCalculationPoint.Y, 2)) * ratio;

                var medianOffset = Math.Sqrt(medianCalculationDistance * medianCalculationDistance / 2);

                var firstMedian = new Point(firstCalculationPoint.X - medianOffset, firstCalculationPoint.Y + medianOffset);
                var secondMedian = new Point(secondCalculationPoint.X - medianOffset, secondCalculationPoint.Y + medianOffset);

                var median = new Point(secondMedian.X + (firstMedian.X - secondMedian.X) / 2, firstMedian.Y + (secondMedian.Y - firstMedian.Y) / 2);

                geometryContext.LineTo(median, true, false);

                geometryContext.LineTo(balloonEndPoint, true, false);
            }
            else
            {
                geometryContext.LineTo(topBorderEnd, true, false);

                if (_geometryCash.CornerRadius.TopRight > 0 && _geometryCash.BalloonDirection != BalloonDirection.TopRight)
                {
                    geometryContext.ArcTo(rightBorderStart,
                        new Size(Math.Max(_geometryCash.CornerRadius.TopRight - borderThickness.Right, 0), Math.Max(_geometryCash.CornerRadius.TopRight - borderThickness.Top, 0)),
                        0.0, false, SweepDirection.Clockwise, true, false);
                }
            }

            // Right
            if (_geometryCash.BalloonDirection == BalloonDirection.Right)
            {
                var yAngleOffset = borderThickness.Right * (1 - _geometryCash.BalloonWidth / (_geometryCash.BalloonWidth + _geometryCash.BalloonLenght));
                var xAngleOffset = borderThickness.Right * _geometryCash.BalloonLenght / _geometryCash.BalloonWidth;

                geometryContext.LineTo(new Point(rightBorderEnd.X, yMidpoint - _geometryCash.BalloonWidth / 2 + yAngleOffset), true, false);

                geometryContext.LineTo(new Point(rightBorderEnd.X + _geometryCash.BalloonLenght - xAngleOffset, yMidpoint), true, false);

                geometryContext.LineTo(new Point(rightBorderEnd.X, yMidpoint + _geometryCash.BalloonWidth / 2 - yAngleOffset), true, false);
            }

            geometryContext.LineTo(rightBorderEnd, true, false);

            if (_geometryCash.CornerRadius.BottomRight > 0 && _geometryCash.BalloonDirection != BalloonDirection.BottomRight)
            {
                geometryContext.ArcTo(bottomBorderStart,
                    new Size(Math.Max(_geometryCash.CornerRadius.BottomRight - borderThickness.Right, 0), Math.Max(_geometryCash.CornerRadius.BottomRight - borderThickness.Bottom, 0)),
                    0.0, false, SweepDirection.Clockwise, true, false);
            }

            // Bottom
            if (_geometryCash.BalloonDirection == BalloonDirection.Bottom)
            {
                var yAngleOffset = borderThickness.Bottom * _geometryCash.BalloonLenght / _geometryCash.BalloonWidth;
                var xAngleOffset = borderThickness.Bottom * (1 - _geometryCash.BalloonWidth / (_geometryCash.BalloonWidth + _geometryCash.BalloonLenght));

                geometryContext.LineTo(new Point(xMidpoint + _geometryCash.BalloonWidth / 2 - xAngleOffset, bottomBorderEnd.Y), true, false);

                geometryContext.LineTo(new Point(xMidpoint, bottomBorderEnd.Y + _geometryCash.BalloonLenght - yAngleOffset), true, false);

                geometryContext.LineTo(new Point(xMidpoint - _geometryCash.BalloonWidth / 2 + xAngleOffset, bottomBorderEnd.Y), true, false);
            }

            geometryContext.LineTo(bottomBorderEnd, true, false);

            if (_geometryCash.CornerRadius.BottomLeft > 0 && _geometryCash.BalloonDirection != BalloonDirection.BottomLeft)
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
                BalloonDirection = BalloonDirection,
                BalloonLenght = BalloonLenght,
                BalloonWidth = BalloonWidth
            };

            Point borderTopLeft = new Point(0, 0), borderBottomRight = new Point(arrangeSize.Width, arrangeSize.Height);

            ApplyBalloonOffset(ref borderTopLeft, ref borderBottomRight);

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

    public enum BalloonDirection
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