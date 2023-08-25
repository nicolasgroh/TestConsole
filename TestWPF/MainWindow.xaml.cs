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

        private void ThumnButton_Click(object sender, EventArgs e)
        {

        }

        int _testClickCount = 0;

        private void Test_Click(object sender, RoutedEventArgs e)
        {
            var firstLineFirstPoint = new Point(10, 12);
            var firstLineSecondPoint = new Point(60, 50);
            
            var secondLineFirstPoint = new Point(20, 42);
            var secondLineSecondPoint = new Point(36, 16);

            if (_testClickCount == 0)
            {
                var firstPath = new Path()
                {
                    Stroke = Brushes.Black
                };

                firstPath.Data = new LineGeometry(firstLineFirstPoint, firstLineSecondPoint);

                TestCanvas.Children.Add(firstPath);

                var secondPath = new Path()
                {
                    Stroke = Brushes.Black
                };

                secondPath.Data = new LineGeometry(secondLineFirstPoint, secondLineSecondPoint);

                TestCanvas.Children.Add(secondPath);
            }
            else if ( _testClickCount == 1)
            {
                var point = new Point();

                var firstMagicMultiplicationValue = firstLineFirstPoint.X * firstLineSecondPoint.Y - firstLineFirstPoint.Y * firstLineSecondPoint.X;
                var secondMagicMultiplicationValue = secondLineSecondPoint.X * secondLineFirstPoint.Y - secondLineSecondPoint.Y * secondLineFirstPoint.X;

                var magicDvisionValue = (firstLineFirstPoint.X - firstLineSecondPoint.X) * (secondLineSecondPoint.Y - secondLineFirstPoint.Y) - (firstLineFirstPoint.Y - firstLineSecondPoint.Y) * (secondLineSecondPoint.X - secondLineFirstPoint.X);

                point.X = ((secondLineSecondPoint.X - secondLineFirstPoint.X) * firstMagicMultiplicationValue - (firstLineFirstPoint.X - firstLineSecondPoint.X) * secondMagicMultiplicationValue) / magicDvisionValue;

                point.Y = ((secondLineSecondPoint.Y - secondLineFirstPoint.Y) * firstMagicMultiplicationValue - (firstLineFirstPoint.Y - firstLineSecondPoint.Y) * secondMagicMultiplicationValue) / magicDvisionValue;

                var thirdPath = new Path()
                {
                    Fill = Brushes.Red
                };

                thirdPath.Data = new EllipseGeometry(point, 3, 3);

                TestCanvas.Children.Add(thirdPath);
            }

            _testClickCount++;
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
}