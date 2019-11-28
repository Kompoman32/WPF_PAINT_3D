using Paint.classes;
using Paint.interfaces;
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

        public static int CanvasMargin = 10;

        #region Кнопки

        private void ClearLines_Click(object sender, RoutedEventArgs e)
        {
            foreach(var o in selectedObjects)
            {
                if (o is IMyGroup)
                {
                    (o as IMyGroup).UnGroup();
                }

                o.SetParent(null);

                o.UnSelect();
            }
            selectedObjects.Clear();
            Canvas.Children.Clear();
        }

        private void CreateLine_Click(object sender, RoutedEventArgs e)
        {
            int width = (int)Canvas.ActualWidth;
            int height = (int)Canvas.ActualHeight;

            int x1 = rnd.Next(width - CanvasMargin);
            int y1 = rnd.Next(height - CanvasMargin);

            int x2 = rnd.Next(width - CanvasMargin);
            int y2 = rnd.Next(height - CanvasMargin);

            //int x1 = 10;
            //int y1 = 150;

            //int x2 = 10;
            //int y2 = 250;

            var line = InitLine(x1, y1, x2, y2);
            Canvas.Children.Add(line);

        }

        private void ClrPcker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            foreach(var o in selectedObjects)
            {
                o.SetStroke(new SolidColorBrush((Color)ColorPicker.SelectedColor));
            }
        }

        private void RemoveObject_Click(object sender, RoutedEventArgs e)
        {
            foreach(var o in selectedObjects)
            {
                if (o is IMyGroup)
                {
                    var shapes = (o as IMyGroup).GetShapes();
                    foreach (var s in shapes)
                    {
                        Canvas.Children.Remove(s);
                    }
                } 
                else
                {
                    Canvas.Children.Remove(o as Shape);
                }

                selectedObjects.Remove(o);
            }
        }

        #endregion Кнопки

        #region Линия

        Point lastPosition;
        List<IMyObject> selectedObjects = new List<IMyObject>();

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
                if (line.GetParent() == null)
                {
                    ChoosePoint_handler(line, ea);
                }
                SelectLine_Handler(line, ea);
            };

            line.MouseLeftButtonUp += delegate (object s, MouseButtonEventArgs ea)
            {
                lastPosition = new Point(30, 200);
                line.IsPressed_Point_1 = false;
                line.IsPressed_Point_2 = false;
            };

            line.PreviewMouseRightButtonDown += delegate (object s, MouseButtonEventArgs ea)
            {
                SetPointsInfo(ea.GetPosition(Canvas), line);
            };

            line.PreviewMouseRightButtonUp+= delegate (object s, MouseButtonEventArgs ea)
            {
                ShowInfo(ea.GetPosition(Canvas));
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
            var parent = line.GetParent();

            if (ea.ClickCount == 3)
            {
                if (parent == null)
                {
                    DivideLine_Click(line, ea);
                }
            }
            else if (ea.ClickCount == 2)
            {
                if (parent != null)
                {
                    while (parent.GetParent() != null)
                    {
                        parent = parent.GetParent();
                    }
                }
                else
                {
                    parent = line;
                }

                if (parent.IsSelected())
                {
                    parent.UnSelect();
                    selectedObjects.Remove(parent);
                }
                else
                {
                    parent.Select();
                    selectedObjects.Add(parent);
                }
            }
        }

        private void ChoosePoint_handler(CustomLine line, MouseButtonEventArgs ea)
        {
            if (line.isSelected)
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

        private void DivideLine_Click(CustomLine line, MouseButtonEventArgs e)
        {
            if (selectedObjects.Count != 1 ) return;

            var selectedLine = selectedObjects.FirstOrDefault();

            if (!(selectedLine is IMyObject)) return;

            var position = e.GetPosition(Canvas);

            var x = position.X;
            var y = position.Y;

            var newLine = InitLine(x, y, line.X2, line.Y2);

            line.X2 = x;
            line.Y2 = y;

            if (selectedLine != null)
            {
                selectedLine.UnSelect();
                selectedObjects.Remove(selectedLine);
            }
            line.Select();
            selectedObjects.Add(line);
            Canvas.Children.Add(newLine);
        }

        #endregion Линия

        #region INFO
        private void SetPointsInfo(Point position, CustomLine line)
        {
            PointInfo_Point.Text = "";

            var realpoint = position.GetProjectionPointOnLine(line);
            if (position.IsOnTheLine(line))
            {
                PointInfo_Line.Text = "line: " + line.Equation.ToString();
            }
            PointInfo_Point.Text = $"X:{realpoint.X:###0.0#} Y:{realpoint.Y:###0.0#}";
        }

        private void ShowInfo(Point position)
        {
            double left = position.X + CanvasMargin * 2;
            double top = position.Y + CanvasMargin * 2;


            if (left + PointInfo.ActualWidth > Canvas_Grid.ActualWidth)
            {
                left -= PointInfo.ActualWidth;
            }

            if (top + PointInfo.ActualHeight > Canvas_Grid.ActualHeight)
            {
                top -= PointInfo.ActualHeight;
            }

            PointInfo.Margin = new Thickness(left, top, 0, 0);
            PointInfo.Visibility = Visibility.Visible;
        }

        private void HideInfo()
        {
            PointInfo.Visibility = Visibility.Hidden;
        }

        #endregion INFO

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var position = e.GetPosition(Canvas);

                foreach (var o in selectedObjects)
                {
                    if (o is CustomLine)
                    {
                        var line = o as CustomLine;
                        if (line.IsPressed_Point_1 && selectedObjects.Count == 1)
                        {
                            line.X1 = position.X;
                            line.Y1 = position.Y;
                        }
                        else
                        if (line.IsPressed_Point_2 && selectedObjects.Count == 1)
                        {
                            line.X2 = position.X;
                            line.Y2 = position.Y;
                        } else
                        {
                            var xd = position.X - lastPosition.X;
                            var yd = position.Y - lastPosition.Y;

                            o.Move(xd, yd);
                        }
                    }
                    else
                    {
                        var xd = position.X - lastPosition.X;
                        var yd = position.Y - lastPosition.Y;
                    
                        o.Move(xd, yd);
                    }

                }


                lastPosition = position;
            };
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            lastPosition = e.GetPosition(Canvas);
            HideInfo();
        }

        private void Canvas_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            PointInfo_Line.Text = "";
            PointInfo_Point.Text = "";

            PointInfo_Point.Text = $"X:{e.GetPosition(Canvas).X:###0.0#} Y:{e.GetPosition(Canvas).Y:###0.0#}";
        }

        private void Canvas_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (string.IsNullOrEmpty(PointInfo_Line.Text))
            {
                e.Handled = true;
                ShowInfo(e.GetPosition(Canvas));
            }
        }

        private void GroupSelected_Click(object sender, RoutedEventArgs e)
        {
            if (selectedObjects.Count <= 1) return;

            var group = new CustomGroup(selectedObjects);
            group.Select();

            selectedObjects.Clear();
            selectedObjects.Add(group);
        }
        private void UnGroupSelected_Click(object sender, RoutedEventArgs e)
        {
            foreach(var o in selectedObjects.ToArray())
            {
                if (o is IMyGroup)
                {
                    selectedObjects.AddRange((o as IMyGroup).GetObjects());
                    (o as IMyGroup).UnGroup();
                    selectedObjects.Remove(o);
                }
            }
        }
    }
}
