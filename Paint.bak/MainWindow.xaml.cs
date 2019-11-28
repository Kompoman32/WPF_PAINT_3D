using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Paint
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        static Random rnd = new Random();

        static int canvasMargin = 10;

        private void ClearLines_Click(object sender, RoutedEventArgs e)
        {
            Canvas.Children.Clear();
        }

        private void CreateLine_Click(object sender, RoutedEventArgs e)
        {
            int width = (int)Canvas.ActualWidth;
            int height = (int)Canvas.ActualHeight;

            int x1 = rnd.Next(width - canvasMargin);
            int y1 = rnd.Next(height - canvasMargin);

            int x2 = rnd.Next(width - canvasMargin);
            int y2 = rnd.Next(height - canvasMargin);

            //int x1 = 10;
            //int y1 = 150;

            //int x2 = 10;
            //int y2 = 250;

            var line = InitLine(x1, y1, x2, y2);
            Canvas.Children.Add(line);

        }

        private CustomLine InitLine(double x1, double y1, double x2, double y2)
        {
            CustomLine line = new CustomLine()
            {
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2,
                Stroke = new SolidColorBrush((Color)ColorPicker.SelectedColor),
                StrokeThickness = 5,
            };

            line.MouseLeftButtonDown += delegate (object s, MouseButtonEventArgs ea)
            {
                ChosePoint_handler(line, ea);
                SelectLine_Handler(line, ea);
            };

            line.MouseLeftButtonUp += delegate (object s, MouseButtonEventArgs ea)
            {
                lastPosition = new Point(30, 200);
                line.IsPressed_Point_1 = false;
                line.IsPressed_Point_2 = false;
            };

            line.MouseMove += delegate (object s, MouseEventArgs ea)
            {
                MoveLine_Handler(line, ea);
            };

            line.MouseEnter += delegate (object s, MouseEventArgs ea)
            {
                line.IsNotEntered = true;
                if (ea.LeftButton == MouseButtonState.Pressed && !(line.IsPressed_Point_1 || line.IsPressed_Point_2))
                {
                    line.IsNotEntered = false;
                }
            };

            return line;
        }

        private void SelectLine_Handler(CustomLine line, MouseButtonEventArgs ea)
        {
            if (ea.ClickCount == 3)
            {
                DivideLine_Click(line, ea);
            }
            else if (ea.ClickCount == 2)
            {
                if (!line.IsSelected)
                    SelectLine(line);
                else
                    UnselectLine(line);
            }
        }

        private void ChosePoint_handler(CustomLine line, MouseButtonEventArgs ea)
        {
            if (line.IsSelected)
            {
                var position = ea.GetPosition(Canvas);

                line.IsPressed_Point_1 = false;
                line.IsPressed_Point_2 = false;

                if (Math.Abs(position.X - line.X1) < line.StrokeThickness * 4 + 8 && Math.Abs(position.Y - line.Y1) < line.StrokeThickness * 4 + 8)
                {
                    line.IsPressed_Point_1 = true;
                }
                else if (Math.Abs(position.X - line.X2) < line.StrokeThickness * 4 + 8 && Math.Abs(position.Y - line.Y2) < line.StrokeThickness * 4 + 8)
                {
                    line.IsPressed_Point_2 = true;
                }

                lastPosition = position;
            }
        }

        Point lastPosition;

        private void MoveLine_Handler(CustomLine line, MouseEventArgs ea)
        {
            if (ea.LeftButton == MouseButtonState.Pressed && line.IsSelected && line.IsNotEntered)
            {
                var position = ea.GetPosition(Canvas);

                if (line.IsPressed_Point_1)
                {
                    line.X1 = position.X;
                    line.Y1 = position.Y;
                }
                else
                if (line.IsPressed_Point_2)
                {
                    line.X2 = position.X;
                    line.Y2 = position.Y;
                }
                else
                {
                    var xd = position.X - lastPosition.X;
                    var yd = position.Y - lastPosition.Y;

                    line.X1 += xd;
                    line.Y1 += yd;
                    line.X2 += xd;
                    line.Y2 += yd;
                }

                lastPosition = position;
            }
        }


        private void ClrPcker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (selectedLine != null)
                selectedLine.Stroke = new SolidColorBrush((Color)ColorPicker.SelectedColor);
        }

        private void DivideLine_Click(CustomLine line, MouseButtonEventArgs e )
        {
            var position = e.GetPosition(Canvas);

            var x = position.X;
            var y = position.Y;

            var newLine = InitLine(x, y, line.X2, line.Y2);

            line.X2 = x;
            line.Y2 = y;

            SelectLine(line);
            Canvas.Children.Add(newLine);
        }


        CustomLine selectedLine;

        private void SelectLine(CustomLine line)
        {
            if (selectedLine != null)
            {
                selectedLine.IsSelected = false;
                selectedLine.InvalidateVisual();
            }
            selectedLine = line;
            line.IsSelected = true;
            line.InvalidateVisual();
        }

        private void UnselectLine(CustomLine line)
        {
            line.IsSelected = false;
            line.InvalidateVisual();
        }
    }
}
