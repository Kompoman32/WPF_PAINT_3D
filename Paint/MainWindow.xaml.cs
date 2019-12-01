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
using System.Windows.Media.Media3D;
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
            GenerateCordSyst();
        }

        static Random rnd = new Random();

        public static int CanvasMargin = 10;
        public static Point CordCenter = new Point(0,0);

        Matrix_Window matrixWindow;

        bool isMatrixWindowOpened = false;

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

            double x1 = rnd.Next(width - CanvasMargin) - CordCenter.X;
            double y1 = rnd.Next(height - CanvasMargin) - CordCenter.Y;

            double x2 = rnd.Next(width - CanvasMargin) - CordCenter.X;
            double y2 = rnd.Next(height - CanvasMargin) - CordCenter.Y;

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
            foreach (var o in selectedObjects.ToArray())
            {
                if (o is IMyGroup)
                {
                    selectedObjects.AddRange((o as IMyGroup).GetObjects());
                    (o as IMyGroup).UnGroup();
                    selectedObjects.Remove(o);
                }
            }
        }

        private void OpenMatrix_Click(object sender, RoutedEventArgs e)
        {
            if (matrixWindow == null)
            {
                matrixWindow = new Matrix_Window(
                                    (matrix) => ConvertObjectsByMatrixHandler(matrix),
                                    () => SaveCordOfObjects(),
                                    () => ResetCordOfObjects()
                                    );
                matrixWindow.Closed += (object s, EventArgs ev) =>
                {
                    isMatrixWindowOpened = false;
                    matrixWindow = null;
                };
            }

            if (!isMatrixWindowOpened)
            {
                matrixWindow.Show();
                isMatrixWindowOpened = true;

            }
            else
            {
                matrixWindow.Focus();
            }
        }


        bool isChoosingCordPostion = false;

        private void CordSyst_Checked(object sender, RoutedEventArgs e)
        {
            if (CordSystem_checkbox.IsChecked == true)
            {
                CordCanvas.Visibility = Visibility.Visible;
                LocalCordSystem_checkbox.IsEnabled = true;
                GlobalCordSystem_checkbox.IsEnabled = true;
            }
            else
            {
                isChoosingCordPostion = false;

                CordCanvas.Visibility = Visibility.Hidden;
                LocalCordSystem_checkbox.IsEnabled = false;
                GlobalCordSystem_checkbox.IsEnabled = false;
            }
        }

        private void LocalCordSystem_checkbox_Click(object sender, RoutedEventArgs e)
        {
            isChoosingCordPostion = true;
        }

        private void GlobalCordSystem_checkbox_Checked(object sender, RoutedEventArgs e)
        {
            isChoosingCordPostion = false;
            MoveCordSystTo(new Point(0, 0));

        }

        #endregion Кнопки

        #region Объекты

        Point lastMousePosition;
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
                originalPoint1 = new Point3D(int.MaxValue, int.MaxValue, int.MaxValue),
                originalPoint2 = new Point3D(int.MaxValue, int.MaxValue, int.MaxValue)
            };

            line.MouseLeftButtonDown += delegate (object s, MouseButtonEventArgs ea)
            {
                var point = ea.GetPosition(Canvas);
                point.Offset(-CordCenter.X, -CordCenter.Y);

                if (line.GetParent() == null)
                {
                    ChoosePoint_handler(line, point);
                }
                SelectLine_Handler(line, ea);
            };

            line.MouseLeftButtonUp += delegate (object s, MouseButtonEventArgs ea)
            {
                line.IsPressed_Point_1 = false;
                line.IsPressed_Point_2 = false;
            };

            line.PreviewMouseRightButtonDown += delegate (object s, MouseButtonEventArgs ea)
            {
                var point = ea.GetPosition(Canvas);
                point.Offset(-CordCenter.X, -CordCenter.Y);
                SetPointsInfo(point, line);
            };

            line.PreviewMouseRightButtonUp+= delegate (object s, MouseButtonEventArgs ea)
            {
                var point = ea.GetPosition(Canvas);
                point.Offset(CordCenter.X, CordCenter.Y);
                ShowInfo(ea.GetPosition(Canvas));
            };

            line.MouseEnter += delegate (object s, MouseEventArgs ea)
            {
                if (line.isSelected)
                {
                    Cursor = Cursors.Hand;
                }
                //line.IsNotEntered = true;
                //if (ea.LeftButton == MouseButtonState.Pressed && !(line.IsPressed_Point_1 || line.IsPressed_Point_2))
                //{
                //    line.IsNotEntered = false;
                //}
            };
            line.MouseMove += delegate (object s, MouseEventArgs ea)
            {
                var position = ea.GetPosition(Canvas);
                position.Offset(-CordCenter.X, -CordCenter.Y);
                if (line.isSelected)
                {

                    if (Math.Abs(position.X - line.X1) < line.StrokeThickness * 4 + 8 && Math.Abs(position.Y - line.Y1) < line.StrokeThickness * 4 + 8
                    || Math.Abs(position.X - line.X2) < line.StrokeThickness * 4 + 8 && Math.Abs(position.Y - line.Y2) < line.StrokeThickness * 4 + 8)
                    {
                        Cursor = Cursors.SizeAll;
                    } else
                    {
                        Cursor = Cursors.Hand;
                    }
                }

            };
            line.MouseLeave += delegate (object s, MouseEventArgs ea)
            {
                Cursor = Cursors.Arrow;
            };
            line.MouseUp += delegate(object s, MouseButtonEventArgs ea)
            {
                Cursor = Cursors.Arrow;
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

        private void ChoosePoint_handler(CustomLine line, Point position)
        {
            if (line.isSelected)
            {
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

                lastMousePosition = position;
            }
        }

        private void DivideLine_Click(CustomLine line, MouseButtonEventArgs e)
        {
            if (selectedObjects.Count != 1 ) return;

            var selectedLine = selectedObjects.FirstOrDefault();

            if (!(selectedLine is IMyObject)) return;

            var position = e.GetPosition(Canvas);
            position.Offset(-CordCenter.X, -CordCenter.Y);
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

        private void ConvertObjectsByMatrixHandler(Matrix3D matrix)
        {
            foreach(var o in selectedObjects)
            {
                o.ConvertByMatrix(matrix);
            }
        }

        private void SaveCordOfObjects()
        {
            foreach (var o in selectedObjects)
            {
                o.SaveNewCords();
            }
        }

        private void ResetCordOfObjects()
        {
            foreach (var o in selectedObjects)
            {
                o.ResetOriginalCords();
            }
        }

        private void GenerateCordSyst()
        {
            var border = 2000;

            var lineOX = new Line() { X1 = -border, X2 = border, Y1 = 0, Y2 = 0, Stroke = new SolidColorBrush(Colors.Black), StrokeThickness = 1, Tag="LineOX" };
            CordCanvas.Children.Add(lineOX);

            var lineOY = new Line() { Y1 = -border, Y2 = border, X1 = 0, X2 = 0, Stroke = new SolidColorBrush(Colors.Black), StrokeThickness = 1, Tag = "LineOY" };
            CordCanvas.Children.Add(lineOY);

            for(var i = -border; i < border; i+=10)
            {
                var line = new Line() { X1 = i, X2 = i, Y1 = i % 100 == 0 ? -7 : -5, Y2 = i % 100 == 0 ? 7 : 5, Stroke = new SolidColorBrush(i % 100 == 0 ? Colors.Red : Colors.Black), StrokeThickness = 1 };
                CordCanvas.Children.Add(line);

                line = new Line() { Y1 = i, Y2 = i, X1 = i % 100 == 0 ? -7 : -5, X2 = i % 100 == 0 ? 7 : 5, Stroke = new SolidColorBrush(i % 100 == 0 ? Colors.Red : Colors.Black), StrokeThickness = 1 };
                CordCanvas.Children.Add(line);

                if (i % 100 == 0 && (i >= 100 || i <= -100))
                {
                    var label = new Label() { Margin = new Thickness(i - 16, 2, 0,0), Content=$"{i}", FontSize=12, Foreground=new SolidColorBrush(Colors.Black), Tag="OX" };
                    CordCanvas.Children.Add(label);

                    label = new Label() { Margin = new Thickness(3, -i - 14, 0, 0), Content = $"{-i}", FontSize = 12, Foreground = new SolidColorBrush(Colors.Black), Tag = "OY" };
                    CordCanvas.Children.Add(label);
                }
            }
        }

        private void MoveCordSystTo(Point point)
        {
            var deltaX = point.X - CordCenter.X;
            var deltaY = point.Y - CordCenter.Y;

            foreach (var c in CordCanvas.Children)
            {
                if (c is Line)
                {
                    var line = c as Line;

                    line.Move(new Point(line.X1 + deltaX, line.Y1 + deltaY), new Point(line.X2 + deltaX, line.Y2 + deltaY));
                }
                else
                {
                    var label = c as Label;

                    if (label.Tag.ToString() == "OX")
                    {
                        label.Margin = new Thickness(label.Margin.Left + deltaX, label.Margin.Top + deltaY, 0, 0);
                    }
                    else
                    {
                        label.Margin = new Thickness(label.Margin.Left + deltaX, label.Margin.Top + deltaY, 0, 0);
                    }

                }
            }
            CordCenter = point;

            foreach(var o in Canvas.Children.Cast<IMyObject>())
            {
                o.Move(-deltaX, -deltaY);
            }
        }

        #endregion Объекты

        #region INFO
        private void SetPointsInfo(Point position, CustomLine line)
        {
            PointInfo_Point.Text = "";

            //var realpoint = position.GetProjectionPointOnLine(line);
            //if (position.IsOnTheLine(line))
            {
                PointInfo_Line.Text = "line: " + line.Equation.ToString();
                PointInfo_Line.Text = "line: " + line.Equation.ToNotRightString();
            }
            PointInfo_Point.Text = $"X:{position.X:###0.0#} Y:{position.Y:###0.0#}";
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
                if (isChoosingCordPostion)
                {
                    MoveCordSystTo(e.GetPosition(Canvas));
                    e.Handled = true;
                    return;
                }

                var position = e.GetPosition(Canvas);

                foreach (var o in selectedObjects)
                {
                    if (o is CustomLine)
                    {
                        var line = o as CustomLine;
                        if (line.IsPressed_Point_1 && selectedObjects.Count == 1)
                        {
                            line.X1 = position.X - CordCenter.X;
                            line.Y1 = position.Y - CordCenter.Y;
                        }
                        else
                        if (line.IsPressed_Point_2 && selectedObjects.Count == 1)
                        {
                            line.X2 = position.X- CordCenter.X;
                            line.Y2 = position.Y - CordCenter.Y;
                        } else
                        {
                            var xd = position.X - lastMousePosition.X;
                            var yd = position.Y - lastMousePosition.Y;

                            o.Move(xd, yd);
                        }
                    }
                    else
                    {
                        var xd = position.X - lastMousePosition.X;
                        var yd = position.Y - lastMousePosition.Y;
                    
                        o.Move(xd, yd);
                    }
                }

                lastMousePosition = position;
            };
        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isChoosingCordPostion = false;
        }


        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (isChoosingCordPostion)
            {
                MoveCordSystTo(e.GetPosition(Canvas));
                e.Handled = true;
            }

            lastMousePosition = e.GetPosition(Canvas);
            HideInfo();
        }

        private void Canvas_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            HideInfo();
            PointInfo_Line.Text = "";
            PointInfo_Point.Text = "";

            PointInfo_Point.Text = $"X:{e.GetPosition(Canvas).X - CordCenter.X:###0.0#} Y:{e.GetPosition(Canvas).Y - CordCenter.Y:###0.0#}";
        }

        private void Canvas_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (string.IsNullOrEmpty(PointInfo_Line.Text))
            {
                e.Handled = true;
            }
            ShowInfo(e.GetPosition(Canvas));
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (matrixWindow != null)
            {
                matrixWindow.Close();
            }
        }
    }
}
