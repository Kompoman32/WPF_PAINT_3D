using Microsoft.Win32;
using Paint.classes;
using Paint.interfaces;
using System;
using System.Collections.Generic;
using System.IO;
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
            CustomLine.DisplayProjectionMatrix =
                new Matrix3D(1, 0, 0, 0,
                             0, 1, 0, 0,
                             0, 0, 1, 0.00,
                             0, 0, 0, 1);

            this.infoWindow = new Info();
            this.infoWindow.Show();

            InitializeComponent();
            GenerateCordSyst();
        }

        Info infoWindow;

        static Random rnd = new Random();

        public static int CanvasMargin = 10;

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
            double z1 = rnd.NextDouble() * 2;

            double x2 = rnd.Next(width - CanvasMargin) - CordCenter.X;
            double y2 = rnd.Next(height - CanvasMargin) - CordCenter.Y;
            double z2 = rnd.NextDouble()*2;

            var line = InitLine(x1, y1, x2, y2, z1, z2);

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
            foreach(var o in selectedObjects.ToList())
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
            var line = Canvas.Children.Cast<IMyObject>().FirstOrDefault(x => x is CustomLine) as CustomLine;

            MessageBox.Show($"({line.X1};{line.Y1})\n{line.X2};{line.Y2})");

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
            if (isLoading) return;

            isChoosingCordPostion = true;
        }

        private void GlobalCordSystem_checkbox_Checked(object sender, RoutedEventArgs e)
        {
            if (isLoading) return;

            isChoosingCordPostion = false;
            MoveCordSystTo(new Point(0, 0));

        }

        bool isLoading = false;

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }

        private bool? Save() {
            var saveDialog = new SaveFileDialog()
            {
                DefaultExt = "p3d",
                AddExtension = true,
                Filter = "Проект рисунка (*.p3d)|*.p3d",
                FileName = "MySaveGame.p3d",
                CheckPathExists = true,
                Title = "Сохранить проект",
            };
            var ok = saveDialog.ShowDialog();
            if (ok == true)
            {
                var path = SaveLoad.Save(saveDialog.FileName, Canvas.Children.Cast<CustomLine>(), CordCenter, CordSystem_checkbox.IsChecked == true);

                MessageBox.Show($"Путь сохранения: {System.IO.Path.GetFullPath(path)}");
            }

            return ok;
        }
        private void Load_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new OpenFileDialog()
            {
                DefaultExt = "p3d",
                AddExtension = true,
                Filter = "Проект рисунка (*.p3d)|*.p3d",
                CheckPathExists = true,
                CheckFileExists = true,
                Title = "Загрузить проект",
            };

            var ok = openDialog.ShowDialog();

            if (ok == true)
            {
                if (openDialog.FileName.Substring(openDialog.FileName.LastIndexOf('.')) != ".p3d")
                {
                    MessageBox.Show("Расширение файла не верное!");
                    return;
                }

                isLoading = true;

                var result = SaveLoad.Load(openDialog.FileName);

                List<CustomLine> objects = result.Item1;
                Point center = result.Item2.Item1;
                bool isShown = result.Item2.Item2;


                //MessageBox.Show($"Count: {objects.Count}\n" +
                //                $"Center: {{{center.X}, {center.Y}}}\n" +
                //                $"Is shown: {isShown}");

                ClearLines_Click(null, null);

                CordSystem_checkbox.IsChecked = false;
                GlobalCordSystem_checkbox.IsChecked = true;

                if (isShown)
                {
                    CordSystem_checkbox.IsChecked = true;
                }

                if (center.X != 0 || center.Y != 0)
                {
                    LocalCordSystem_checkbox.IsChecked = true;
                }

                isChoosingCordPostion = false;
                CordSyst_Checked(null, null);
                MoveCordSystTo(center);

                foreach (var o in objects)
                {
                    Canvas.Children.Add(o);

                    IMyObject parent = o;
                    while (parent.GetParent() != null)
                    {
                        parent = parent.GetParent();
                    }

                    if (parent.IsSelected())
                    {
                        if (!selectedObjects.Contains(parent))
                            selectedObjects.Add(parent);
                    }

                    AddEventsOnLine(o);
                    o.InvalidateVisual();
                }

                isLoading = false;
            }
        }

        #endregion Кнопки

        #region Lines

        Point lastMousePosition;
        List<IMyObject> selectedObjects = new List<IMyObject>();

        CustomLine onLine;

        private CustomLine InitLine(double x1, double y1, double x2, double y2, double z1 = 1, double z2 = 1)
        {
            CustomLine line = new CustomLine(new SolidColorBrush((Color)ColorPicker.SelectedColor))
            {
                X1 = x1,
                Y1 = y1,
                Z1 = z1,
                X2 = x2,
                Y2 = y2,
                Z2 = z2,
                StrokeThickness = 5,
                originalPoint1 = new Point3D(int.MaxValue, int.MaxValue, int.MaxValue),
                originalPoint2 = new Point3D(int.MaxValue, int.MaxValue, int.MaxValue)
            };

            AddEventsOnLine(line);

            return line;
        }

        private void AddEventsOnLine(CustomLine line)
        {
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
                if (line.GetParent() == null)
                {
                    ChoosePoint_handler(line, point);
                }
                SetPointsInfo(point, line);
            };

            line.PreviewMouseRightButtonUp += delegate (object s, MouseButtonEventArgs ea)
            {
                var point = ea.GetPosition(Canvas);
                point.Offset(CordCenter.X, CordCenter.Y);
            };

            line.MouseEnter += delegate (object s, MouseEventArgs ea)
            {
                if (line.IsSelected())
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
                if (ea.LeftButton != MouseButtonState.Pressed)
                {
                    line.IsPressed_Point_1 = false;
                    line.IsPressed_Point_2 = false;
                }

                onLine = line;
                SetPointsInfo(ea.GetPosition(Canvas), line);
                var position = ea.GetPosition(Canvas);
                position.Offset(-CordCenter.X, -CordCenter.Y);
                if (line.IsSelected())
                {

                    if (line.IsNearToPoint1(position) || line.IsNearToPoint2(position))
                    {
                        Cursor = Cursors.SizeAll;
                    }
                    else
                    {
                        Cursor = Cursors.Hand;
                    }
                }

            };
            line.MouseLeave += delegate (object s, MouseEventArgs ea)
            {
                onLine = null;
                Cursor = Cursors.Arrow;
            };
            line.MouseUp += delegate (object s, MouseButtonEventArgs ea)
            {
                Cursor = Cursors.Arrow;
            };
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

        private void ChoosePoint_handler(CustomLine line, Point point)
        {
            if (line.IsSelected())
            {
                line.IsPressed_Point_1 = false;
                line.IsPressed_Point_2 = false;

                if (line.IsNearToPoint1(point))
                {
                    line.IsPressed_Point_1 = true;
                }
                else if (line.IsNearToPoint2(point))
                {
                    line.IsPressed_Point_2 = true;
                }

                lastMousePosition = point;
            }
        }

        private void DivideLine_Click(CustomLine line, MouseButtonEventArgs e)
        {
            //wont work due to Z cord;
            return;
            
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

        #endregion Lines

        #region Cords

        public static Point CordCenter = new Point(0, 0);

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
                    line.InvalidateVisual();
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

            if (isLoading) return;

            foreach(var o in Canvas.Children.Cast<IMyObject>())
            {
                o.Move(-deltaX, -deltaY);
            }
        }

        #endregion Cords

        #region INFO

        CustomLine ZPointLine;

        private void SetPointsInfo(Point position, CustomLine line)
        {
            PointInfo_Point.Text = "";
            PointInfo_Line.Text = "";
            ZPoint_Panel.Visibility = Visibility.Hidden;
            ZPointLine = null;

            PointInfo_Point.Text = $"X:{position.X:###0.0#} Y:{position.Y:###0.0#}";

            if (line == null)
            {
                infoWindow.SetPointOnLine(new Point(0,0));
                return;
            }
             else
            {
                var realpoint = position.GetProjectionPointOnLine(line);

                infoWindow.SetPointOnLine(realpoint);
            }

            if (position.IsOnTheLine(line))
            {
                PointInfo_Line.Text = "line: " + line.Equation.ToString();
                //PointInfo_Line.Text = "line: " + line.Equation.ToNotRightString();
            }
            PointInfo_Point.Text = $"X:{position.X:###0.0#} Y:{position.Y:###0.0#}";

            if (line.IsPressed_Point_1 || line.IsPressed_Point_2)
            {
                ZPointLine = line;
                ZPoint.Text = (line.IsPressed_Point_1 ? line.Z1 : line.Z2).ToString("###0.0#");
                ZPoint_Panel.Visibility = Visibility.Visible;
            }
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
            PointInfo_Line.Text = "";
            PointInfo_Point.Text = "";
            ZPoint_Panel.Visibility = Visibility.Hidden;
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var textbox = sender as TextBox;

            var available = new List<Key>()
            {
                Key.D0,
                Key.D1,
                Key.D2,
                Key.D3,
                Key.D4,
                Key.D5,
                Key.D6,
                Key.D7,
                Key.D8,
                Key.D9,

                Key.NumPad0,
                Key.NumPad1,
                Key.NumPad2,
                Key.NumPad3,
                Key.NumPad4,
                Key.NumPad5,
                Key.NumPad6,
                Key.NumPad7,
                Key.NumPad8,
                Key.NumPad9,

                Key.OemMinus,
                Key.Subtract,
                Key.OemPeriod,
                Key.Decimal,

                Key.Delete,
                Key.Back,
                Key.Left,
                Key.Right,
                Key.Home,
                Key.End,
            };


            if (available.Contains(e.Key))
            {
                if ((e.Key == Key.OemMinus || e.Key == Key.Subtract) && (textbox.Text.Contains('-') || textbox.CaretIndex > 0)
                    || (e.Key == Key.OemPeriod || e.Key == Key.Decimal) && (textbox.Text.Contains(",") || textbox.Text.Contains(".")))
                {
                    e.Handled = true;
                }
            }
            else
            {
                e.Handled = true;
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textbox = sender as TextBox;

            var index = textbox.CaretIndex;
            if (textbox.Text.Contains("-."))
            {
                textbox.Text = textbox.Text.Replace("-.", "-0.");
                textbox.CaretIndex = index + 1;
            }

            if (ZPointLine != null)
            {
                var z = ConvertToDouble(textbox.Text);
                if (ZPointLine.IsPressed_Point_1)
                {
                    ZPointLine.Z1 = z;
                }

                if (ZPointLine.IsPressed_Point_2)
                {
                    ZPointLine.Z2 = z;
                }
            }
        }

        public void TextBox_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var textbox = sender as TextBox;

            var text = textbox.Text;

            double val = ConvertToDouble(text);
            val += e.Delta > 0 ? 0.1 : -0.1;

            textbox.Text = val.ToString();
        }

        static double ConvertToDouble(string str)
        {
            str = str.Replace(".", ",");
            if (str == "-" || str == "-0," || str == "," || str == "")
            {
                str = "0";
            }
            else
            {
                if (str.LastIndexOf(",") == str.Length - 1)
                {
                    str = str.Replace(",", "");
                }
            }
            double.TryParse(str, out double val);
            return val;
        }

        #endregion INFO

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            var pos = e.GetPosition(Canvas);
            pos.Offset(-CordCenter.X, -CordCenter.Y);


            infoWindow.SetPoint(pos);
            infoWindow.SetObjects(selectedObjects);
            SetPointsInfo(pos, onLine);

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (isChoosingCordPostion)
                {
                    MoveCordSystTo(e.GetPosition(Canvas));
                    e.Handled = true;
                    return;
                }

                var position = e.GetPosition(Canvas);

                var xd = position.X - lastMousePosition.X;
                var yd = position.Y - lastMousePosition.Y;

                foreach (var o in selectedObjects)
                {
                    if (o is CustomLine)
                    {
                        var line = o as CustomLine;
                        if (selectedObjects.Count == 1 && (line.IsPressed_Point_1 || line.IsPressed_Point_2))
                        {
                            //var xd = position.X - CordCenter.X - (line.IsPressed_Point_1 ? line.X1 : line.X2);
                            //var yd = position.Y - CordCenter.Y - (line.IsPressed_Point_2 ? line.Y1 : line.Y2);

                            //var xd = position.X - lastMousePosition.X;
                            //var yd = position.Y - lastMousePosition.Y;

                            if (line.IsPressed_Point_1)
                            {
                                //var linePoint = CustomLine.DisplayProjectionMatrix.Transform(new Point3D(position.X - CordCenter.X, position.Y - CordCenter.Y, line.Z1));

                                line.X1 += xd;
                                line.Y1 += yd;
                            }
                            else if (line.IsPressed_Point_2)
                            {
                                //var linePoint = CustomLine.DisplayProjectionMatrix.Transform(new Point3D(position.X - CordCenter.X, position.Y - CordCenter.Y, line.Z2));

                                line.X2 += xd;
                                line.Y2 += yd;
                            }
                        }
                        else
                        {
                            o.Move(xd, yd);
                        }
                    }
                    else
                    {
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

            PointInfo_Point.Text = $"X:{e.GetPosition(Canvas).X - CordCenter.X:###0.0#} Y:{e.GetPosition(Canvas).Y - CordCenter.Y:###0.0#}";
        }

        private void Canvas_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (string.IsNullOrEmpty(PointInfo_Point.Text))
            {
                e.Handled = true;
            }
            ShowInfo(e.GetPosition(Canvas));
        }



        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var answer = MessageBox.Show("Желаете сохранить?", "Выход", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

            bool ok;

            if (answer == MessageBoxResult.Yes)
            {
                ok = Save() == true;
            } else
            {
                if (answer == MessageBoxResult.No)
                {
                    ok = true;
                }
                else
                {
                    ok = false;
                }
            }

            e.Cancel = !ok;

            if (!ok)
            {
                return;
            }

            if (matrixWindow != null)
            {
                matrixWindow.Close();
            }
            infoWindow.CloseThis();
        }
    }
}
