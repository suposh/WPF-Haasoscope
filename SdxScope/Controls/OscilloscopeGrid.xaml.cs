using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SdxScope.Controls
{
    public partial class OscilloscopeGrid : UserControl
    {
        public OscilloscopeGrid()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            DrawGrid();
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawGrid();
        }

        private void DrawGrid()
        {
            HorizontalLinesCanvas.Children.Clear();
            VerticalLinesCanvas.Children.Clear();

            int rows = 10;
            int cols = 10;

            double width = RootGrid.ActualWidth;
            double height = RootGrid.ActualHeight;

            double rowHeight = height / rows;
            double colWidth = width / cols;

            for (int i = 0; i <= rows; i++)
            {
                double y = i * rowHeight;
                HorizontalLinesCanvas.Children.Add(new Line
                {
                    X1 = 0,
                    Y1 = y,
                    X2 = width,
                    Y2 = y,
                    Stroke = Brushes.Lime,
                    StrokeThickness = 1,
                    StrokeDashArray = new DoubleCollection { 2, 2 }
                });
            }

            for (int i = 0; i <= cols; i++)
            {
                double x = i * colWidth;
                VerticalLinesCanvas.Children.Add(new Line
                {
                    X1 = x,
                    Y1 = 0,
                    X2 = x,
                    Y2 = height,
                    Stroke = Brushes.Lime,
                    StrokeThickness = 1,
                    StrokeDashArray = new DoubleCollection { 2, 2 }
                });
            }

            // Center crosshairs
            VerticalCenterLine.X1 = VerticalCenterLine.X2 = width / 2;
            VerticalCenterLine.Y1 = 0;
            VerticalCenterLine.Y2 = height;

            HorizontalCenterLine.Y1 = HorizontalCenterLine.Y2 = height / 2;
            HorizontalCenterLine.X1 = 0;
            HorizontalCenterLine.X2 = width;
        }
    }
}
