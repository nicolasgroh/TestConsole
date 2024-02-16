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
    public class BeakedBorder : Decorator
    {
        private struct GeometryCache
        {
            public Thickness BorderThickness;

            public CornerRadius CornerRadius;

            public BeakDirection BeakDirection;

            public double BeakLenght;

            public double BeakWidth;
        }

        private struct DiagonalBeakInfo
        {
            public double DiagonalBeakWidth;

            public double DiagonalBeakThicknessAngleOffsetMultiplier;

            public double DiagonalBeakOffset;
        }

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

        private GeometryCache _geometryCash;
        private DiagonalBeakInfo _diagonalBeakInfo;

        private StreamGeometry _borderGeometry;
        private StreamGeometry _backgroundGreometry;

        private void CreateGeometryCash()
        {
            _geometryCash = new GeometryCache
            {
                BorderThickness = BorderThickness,
                CornerRadius = CornerRadius,
                BeakDirection = BeakDirection,
                BeakLenght = BeakLenght,
                BeakWidth = BeakWidth
            };
        }

        private void CalculateDiagonalBeakInfo()
        {
            var diagonalBeakWidth = Math.Sqrt(Math.Pow(_geometryCash.BeakWidth, 2) / 2);
            var diagonalBeakWidthOffset = diagonalBeakWidth / 2;
            var diagonalBeakLenght = Math.Sqrt(Math.Pow(_geometryCash.BeakLenght, 2) / 2);
            var diagonalBeakOffset1 = diagonalBeakLenght - diagonalBeakWidthOffset;
            var diagonalBeakOffset2 = diagonalBeakLenght - diagonalBeakWidthOffset + diagonalBeakWidth;

            var diagonalBeakThicknessOffsetMultiplier = 1 - diagonalBeakOffset2 / (diagonalBeakOffset1 + diagonalBeakOffset2);

            _diagonalBeakInfo = new DiagonalBeakInfo
            {
                DiagonalBeakWidth = diagonalBeakWidth,
                DiagonalBeakThicknessAngleOffsetMultiplier = diagonalBeakThicknessOffsetMultiplier,
                DiagonalBeakOffset = diagonalBeakOffset1
            };
        }

        private void CalculateBeakOffset(out double BeakWidthOffset, out double BeakHeightOffset)
        {
            var diagonalBeakLenght = Math.Sqrt(Math.Pow(_geometryCash.BeakLenght, 2) / 2);

            var diagonalBeakWidthOffset = Math.Sqrt(_geometryCash.BeakWidth * _geometryCash.BeakWidth / 2) / 2;

            BeakWidthOffset = 0.0;
            BeakHeightOffset = 0.0;

            switch (_geometryCash.BeakDirection)
            {
                case BeakDirection.Left:
                case BeakDirection.Right:
                    BeakWidthOffset = _geometryCash.BeakLenght;
                    break;

                case BeakDirection.Top:
                case BeakDirection.Bottom:
                    BeakHeightOffset = _geometryCash.BeakLenght;
                    break;
                case BeakDirection.TopLeft:
                case BeakDirection.TopRight:
                case BeakDirection.BottomRight:
                case BeakDirection.BottomLeft:
                    BeakWidthOffset = diagonalBeakLenght - diagonalBeakWidthOffset;
                    BeakHeightOffset = diagonalBeakLenght - diagonalBeakWidthOffset;
                    break;
            }
        }

        private void ApplyBeakOffset(ref Point topLeft, ref Point bottomRight)
        {
            var diagonalBeakLenght = Math.Sqrt(Math.Pow(_geometryCash.BeakLenght, 2) / 2);

            var diagonalBeakWidthOffset = Math.Sqrt(_geometryCash.BeakWidth * _geometryCash.BeakWidth / 2) / 2;

            switch (_geometryCash.BeakDirection)
            {
                case BeakDirection.Left:
                    topLeft.X += _geometryCash.BeakLenght;
                    break;
                case BeakDirection.TopLeft:
                    topLeft.Y += diagonalBeakLenght - diagonalBeakWidthOffset;
                    topLeft.X += diagonalBeakLenght - diagonalBeakWidthOffset;
                    break;
                case BeakDirection.Top:
                    topLeft.Y += _geometryCash.BeakLenght;
                    break;
                case BeakDirection.TopRight:
                    topLeft.Y += diagonalBeakLenght - diagonalBeakWidthOffset;
                    bottomRight.X -= diagonalBeakLenght - diagonalBeakWidthOffset;
                    break;
                case BeakDirection.Right:
                    bottomRight.X -= _geometryCash.BeakLenght;
                    break;
                case BeakDirection.BottomRight:
                    bottomRight.X -= diagonalBeakLenght - diagonalBeakWidthOffset;
                    bottomRight.Y -= diagonalBeakLenght - diagonalBeakWidthOffset;
                    break;
                case BeakDirection.Bottom:
                    bottomRight.Y -= _geometryCash.BeakLenght;
                    break;
                case BeakDirection.BottomLeft:
                    topLeft.X += diagonalBeakLenght - diagonalBeakWidthOffset;
                    bottomRight.Y -= diagonalBeakLenght - diagonalBeakWidthOffset;
                    break;
            }
        }

        private void CalculcateBorderOffset(out double borderWidthOffset, out double borderHeightOffset)
        {
            borderWidthOffset = _geometryCash.BorderThickness.Left + _geometryCash.BorderThickness.Right;
            borderHeightOffset = _geometryCash.BorderThickness.Top + _geometryCash.BorderThickness.Bottom;
        }

        private Size CalculateMinimumContentSize()
        {
            var width = Math.Max(CornerRadius.TopLeft + CornerRadius.TopRight, CornerRadius.BottomLeft + CornerRadius.BottomRight);
            var height = Math.Max(CornerRadius.TopLeft + CornerRadius.BottomLeft, CornerRadius.TopRight + CornerRadius.BottomRight);

            switch (_geometryCash.BeakDirection)
            {
                case BeakDirection.Left:
                case BeakDirection.Right:
                    height += _geometryCash.BeakWidth;
                    break;
                case BeakDirection.Top:
                case BeakDirection.Bottom:
                    width += _geometryCash.BeakWidth;
                    break;
                case BeakDirection.TopLeft:
                    width = Math.Max(width, _diagonalBeakInfo.DiagonalBeakWidth + CornerRadius.TopRight);
                    height = Math.Max(height, _diagonalBeakInfo.DiagonalBeakWidth + CornerRadius.BottomLeft);
                    break;
                case BeakDirection.TopRight:
                    width = Math.Max(width, _diagonalBeakInfo.DiagonalBeakWidth + CornerRadius.TopLeft);
                    height = Math.Max(height, _diagonalBeakInfo.DiagonalBeakWidth + CornerRadius.BottomRight);
                    break;
                case BeakDirection.BottomRight:
                    width = Math.Max(width, _diagonalBeakInfo.DiagonalBeakWidth + CornerRadius.BottomLeft);
                    height = Math.Max(height, _diagonalBeakInfo.DiagonalBeakWidth + CornerRadius.TopRight);
                    break;
                case BeakDirection.BottomLeft:
                    width = Math.Max(width, _diagonalBeakInfo.DiagonalBeakWidth + CornerRadius.BottomRight);
                    height = Math.Max(height, _diagonalBeakInfo.DiagonalBeakWidth + CornerRadius.TopLeft);
                    break;
            }

            return new Size(width, height);
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
            CreateGeometryCash();

            CalculateDiagonalBeakInfo();

            CalculateBeakOffset(out double BeakWidthOffset, out double BeakHeightOffset);

            CalculcateBorderOffset(out double borderWidthOffset, out double borderHeightOffset);

            var contentSize = CalculateMinimumContentSize();

            var child = Child;

            if (child != null)
            {
                var childConstraint = new Size(Math.Max(constraint.Width - BeakWidthOffset - borderWidthOffset, 0), Math.Max(constraint.Height - BeakHeightOffset - borderHeightOffset, 0));

                child.Measure(childConstraint);

                contentSize = new Size(Math.Max(child.DesiredSize.Width, contentSize.Width), Math.Max(child.DesiredSize.Height, contentSize.Height));
            }

            return new Size(BeakWidthOffset + borderWidthOffset + contentSize.Width, BeakHeightOffset + borderHeightOffset + contentSize.Height);
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

            var xMidpoint = (bottomRight.X + borderThickness.Right - topLeft.X - borderThickness.Left) / 2 + topLeft.X;
            var yMidpoint = (bottomRight.Y + borderThickness.Bottom - topLeft.Y - borderThickness.Top) / 2 + topLeft.Y;

            // Left
            geometryContext.BeginFigure(leftBorderStart, true, true);

            if (_geometryCash.BeakDirection == BeakDirection.TopLeft)
            {
                var firstAngleOffset = borderThickness.Left * _diagonalBeakInfo.DiagonalBeakThicknessAngleOffsetMultiplier;
                var thirdAngleOffset = borderThickness.Top * _diagonalBeakInfo.DiagonalBeakThicknessAngleOffsetMultiplier;

                var BeakStartPoint = new Point(topLeft.X, Math.Max(topLeft.Y - borderThickness.Top + _diagonalBeakInfo.DiagonalBeakWidth - firstAngleOffset, topLeft.Y));
                var BeakEndPoint = new Point(Math.Max(topLeft.X - borderThickness.Left + _diagonalBeakInfo.DiagonalBeakWidth - thirdAngleOffset, topLeft.X), topLeft.Y);

                geometryContext.LineTo(BeakStartPoint, true, false);

                var actualTargetCorner = new Point(topLeft.X - _diagonalBeakInfo.DiagonalBeakOffset - borderThickness.Left, topLeft.Y - _diagonalBeakInfo.DiagonalBeakOffset - borderThickness.Top);

                var firstCalculationPoint = new Point(actualTargetCorner.X, actualTargetCorner.Y - borderThickness.Left - firstAngleOffset);
                var secondCalculationPoint = new Point(actualTargetCorner.X - borderThickness.Top - thirdAngleOffset, actualTargetCorner.Y);

                var middlePoint = CalculateIntersectionPoint(BeakStartPoint, firstCalculationPoint, BeakEndPoint, secondCalculationPoint);

                geometryContext.LineTo(middlePoint, true, false);

                geometryContext.LineTo(BeakEndPoint, true, false);
            }
            else
            {
                if (_geometryCash.BeakDirection == BeakDirection.Left)
                {
                    var yAngleOffset = borderThickness.Left * (1 - _geometryCash.BeakWidth / (_geometryCash.BeakWidth + _geometryCash.BeakLenght));
                    var xAngleOffset = borderThickness.Left * _geometryCash.BeakLenght / _geometryCash.BeakWidth;

                    geometryContext.LineTo(new Point(leftBorderEnd.X, yMidpoint + _geometryCash.BeakWidth / 2 - yAngleOffset), true, false);

                    geometryContext.LineTo(new Point(leftBorderEnd.X - _geometryCash.BeakLenght + xAngleOffset, yMidpoint), true, false);

                    geometryContext.LineTo(new Point(leftBorderEnd.X, yMidpoint - _geometryCash.BeakWidth / 2 + yAngleOffset), true, false);
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
            if (_geometryCash.BeakDirection == BeakDirection.Top)
            {
                var yAngleOffset = borderThickness.Top * _geometryCash.BeakLenght / _geometryCash.BeakWidth;
                var xAngleOffset = borderThickness.Top * (1 - _geometryCash.BeakWidth / (_geometryCash.BeakWidth + _geometryCash.BeakLenght));

                geometryContext.LineTo(new Point(xMidpoint - _geometryCash.BeakWidth / 2 + xAngleOffset, topBorderEnd.Y), true, false);

                geometryContext.LineTo(new Point(xMidpoint, topBorderEnd.Y - _geometryCash.BeakLenght + yAngleOffset), true, false);

                geometryContext.LineTo(new Point(xMidpoint + _geometryCash.BeakWidth / 2 - xAngleOffset, topBorderEnd.Y), true, false);
            }

            if (_geometryCash.BeakDirection == BeakDirection.TopRight)
            {
                var firstAngleOffset = borderThickness.Top * _diagonalBeakInfo.DiagonalBeakThicknessAngleOffsetMultiplier;
                var thirdAngleOffset = borderThickness.Right * _diagonalBeakInfo.DiagonalBeakThicknessAngleOffsetMultiplier;

                var BeakStartPoint = new Point(bottomRight.X + borderThickness.Right - _diagonalBeakInfo.DiagonalBeakWidth + firstAngleOffset, topLeft.Y);
                var BeakEndPoint = new Point(bottomRight.X, topLeft.Y - borderThickness.Top + _diagonalBeakInfo.DiagonalBeakWidth - thirdAngleOffset);

                geometryContext.LineTo(BeakStartPoint, true, false);

                var actualTargetCornern = new Point(bottomRight.X + _diagonalBeakInfo.DiagonalBeakOffset + borderThickness.Right, topLeft.Y - _diagonalBeakInfo.DiagonalBeakOffset - borderThickness.Top);

                var firstCalculationPoint = new Point(actualTargetCornern.X + borderThickness.Top + firstAngleOffset, actualTargetCornern.Y);
                var secondCalculationPoint = new Point(actualTargetCornern.X, actualTargetCornern.Y - borderThickness.Right - thirdAngleOffset);

                var middlePoint = CalculateIntersectionPoint(BeakStartPoint, firstCalculationPoint, BeakEndPoint, secondCalculationPoint);

                geometryContext.LineTo(middlePoint, true, false);

                geometryContext.LineTo(BeakEndPoint, true, false);
            }
            else
            {
                geometryContext.LineTo(topBorderEnd, true, false);

                if (_geometryCash.CornerRadius.TopRight > 0)
                {
                    geometryContext.ArcTo(rightBorderStart,
                        new Size(Math.Max(_geometryCash.CornerRadius.TopRight - borderThickness.Right, 0), Math.Max(_geometryCash.CornerRadius.TopRight - borderThickness.Top, 0)),
                        0.0, false, SweepDirection.Clockwise, true, false);
                }
            }

            // Right
            if (_geometryCash.BeakDirection == BeakDirection.Right)
            {
                var yAngleOffset = borderThickness.Right * (1 - _geometryCash.BeakWidth / (_geometryCash.BeakWidth + _geometryCash.BeakLenght));
                var xAngleOffset = borderThickness.Right * _geometryCash.BeakLenght / _geometryCash.BeakWidth;

                geometryContext.LineTo(new Point(rightBorderEnd.X, yMidpoint - _geometryCash.BeakWidth / 2 + yAngleOffset), true, false);

                geometryContext.LineTo(new Point(rightBorderEnd.X + _geometryCash.BeakLenght - xAngleOffset, yMidpoint), true, false);

                geometryContext.LineTo(new Point(rightBorderEnd.X, yMidpoint + _geometryCash.BeakWidth / 2 - yAngleOffset), true, false);
            }

            if (_geometryCash.BeakDirection == BeakDirection.BottomRight)
            {
                var firstAngleOffset = borderThickness.Right * _diagonalBeakInfo.DiagonalBeakThicknessAngleOffsetMultiplier;

                var thirdAngleOffset = borderThickness.Bottom * _diagonalBeakInfo.DiagonalBeakThicknessAngleOffsetMultiplier;

                var BeakStartPoint = new Point(bottomRight.X, bottomRight.Y + borderThickness.Bottom - _diagonalBeakInfo.DiagonalBeakWidth + firstAngleOffset);
                var BeakEndPoint = new Point(bottomRight.X + borderThickness.Right - _diagonalBeakInfo.DiagonalBeakWidth + thirdAngleOffset, bottomRight.Y);

                geometryContext.LineTo(BeakStartPoint, true, false);

                var actualTargetCornern = new Point(bottomRight.X + _diagonalBeakInfo.DiagonalBeakOffset + borderThickness.Right, bottomRight.Y + _diagonalBeakInfo.DiagonalBeakOffset + borderThickness.Bottom);

                var firstCalculationPoint = new Point(actualTargetCornern.X, actualTargetCornern.Y + borderThickness.Right + firstAngleOffset);
                var secondCalculationPoint = new Point(actualTargetCornern.X + borderThickness.Bottom + thirdAngleOffset, actualTargetCornern.Y);

                var middlePoint = CalculateIntersectionPoint(BeakStartPoint, firstCalculationPoint, BeakEndPoint, secondCalculationPoint);

                geometryContext.LineTo(middlePoint, true, false);

                geometryContext.LineTo(BeakEndPoint, true, false);
            }
            else
            {
                geometryContext.LineTo(rightBorderEnd, true, false);

                if (_geometryCash.CornerRadius.BottomRight > 0 && _geometryCash.BeakDirection != BeakDirection.BottomRight)
                {
                    geometryContext.ArcTo(bottomBorderStart,
                        new Size(Math.Max(_geometryCash.CornerRadius.BottomRight - borderThickness.Right, 0), Math.Max(_geometryCash.CornerRadius.BottomRight - borderThickness.Bottom, 0)),
                        0.0, false, SweepDirection.Clockwise, true, false);
                }
            }

            // Bottom
            if (_geometryCash.BeakDirection == BeakDirection.Bottom)
            {
                var yAngleOffset = borderThickness.Bottom * _geometryCash.BeakLenght / _geometryCash.BeakWidth;
                var xAngleOffset = borderThickness.Bottom * (1 - _geometryCash.BeakWidth / (_geometryCash.BeakWidth + _geometryCash.BeakLenght));

                geometryContext.LineTo(new Point(xMidpoint + _geometryCash.BeakWidth / 2 - xAngleOffset, bottomBorderEnd.Y), true, false);

                geometryContext.LineTo(new Point(xMidpoint, bottomBorderEnd.Y + _geometryCash.BeakLenght - yAngleOffset), true, false);

                geometryContext.LineTo(new Point(xMidpoint - _geometryCash.BeakWidth / 2 + xAngleOffset, bottomBorderEnd.Y), true, false);
            }

            if (_geometryCash.BeakDirection == BeakDirection.BottomLeft)
            {
                var firstAngleOffset = borderThickness.Bottom * _diagonalBeakInfo.DiagonalBeakThicknessAngleOffsetMultiplier;

                var thirdAngleOffset = borderThickness.Left * _diagonalBeakInfo.DiagonalBeakThicknessAngleOffsetMultiplier;

                var BeakStartPoint = new Point(topLeft.X - borderThickness.Left + _diagonalBeakInfo.DiagonalBeakWidth - thirdAngleOffset, bottomRight.Y);
                var BeakEndPoint = new Point(topLeft.X, bottomRight.Y + borderThickness.Top - _diagonalBeakInfo.DiagonalBeakWidth + firstAngleOffset);

                geometryContext.LineTo(BeakStartPoint, true, false);

                var actualTargetCornern = new Point(topLeft.X - _diagonalBeakInfo.DiagonalBeakOffset - borderThickness.Left, bottomRight.Y + _diagonalBeakInfo.DiagonalBeakOffset + borderThickness.Bottom);

                var firstCalculationPoint = new Point(actualTargetCornern.X - borderThickness.Bottom - firstAngleOffset, actualTargetCornern.Y);
                var secondCalculationPoint = new Point(actualTargetCornern.X, actualTargetCornern.Y + borderThickness.Left + thirdAngleOffset);

                var middlePoint = CalculateIntersectionPoint(BeakStartPoint, firstCalculationPoint, BeakEndPoint, secondCalculationPoint);

                geometryContext.LineTo(middlePoint, true, false);

                geometryContext.LineTo(BeakEndPoint, true, false);
            }
            else
            {
                geometryContext.LineTo(bottomBorderEnd, true, false);

                if (_geometryCash.CornerRadius.BottomLeft > 0 && _geometryCash.BeakDirection != BeakDirection.BottomLeft)
                {
                    geometryContext.ArcTo(new Point(topLeft.X, bottomRight.Y - Math.Max(_geometryCash.CornerRadius.BottomLeft - borderThickness.Bottom, 0)),
                        new Size(Math.Max(_geometryCash.CornerRadius.BottomLeft - borderThickness.Left, 0), Math.Max(_geometryCash.CornerRadius.BottomLeft - borderThickness.Bottom, 0)),
                        0.0, false, SweepDirection.Clockwise, true, false);
                }
            }
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            Point borderTopLeft = new Point(0, 0), borderBottomRight = new Point(arrangeSize.Width, arrangeSize.Height);

            ApplyBeakOffset(ref borderTopLeft, ref borderBottomRight);

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
        BottomLeft
    }
}