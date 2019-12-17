using Paint.classes;
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
using System.Windows.Shapes;

namespace Paint
{
    /// <summary>
    /// Логика взаимодействия для Matrix.xaml
    /// </summary>
    public partial class Morph_Window : Window
    {
        Action<Matrix3D> matrixConverAction;
        Action saveAction;
        Action abortAction;

        public Morph_Window()
        {
            matrixConverAction = _ => {};
            saveAction = () => {};
            abortAction = () => {};
            InitializeComponent();
        }

        string defaultOffset = "    ";

        public Morph_Window(Action<Matrix3D> matrixConverAction, Action saveAction, Action abortAction) :this()
        {
            this.matrixConverAction = matrixConverAction;
            this.saveAction = saveAction;
            this.abortAction = abortAction;
        }

        //private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        //{
        //    var textbox = sender as TextBox;

        //    var available = new List<Key>()
        //    {
        //        Key.D0,
        //        Key.D1,
        //        Key.D2,
        //        Key.D3,
        //        Key.D4,
        //        Key.D5,
        //        Key.D6,
        //        Key.D7,
        //        Key.D8,
        //        Key.D9,

        //        Key.NumPad0,
        //        Key.NumPad1,
        //        Key.NumPad2,
        //        Key.NumPad3,
        //        Key.NumPad4,
        //        Key.NumPad5,
        //        Key.NumPad6,
        //        Key.NumPad7,
        //        Key.NumPad8,
        //        Key.NumPad9,

        //        Key.OemMinus,
        //        Key.Subtract,
        //        Key.OemPeriod,
        //        Key.Decimal,

        //        Key.Delete,
        //        Key.Back,
        //        Key.Left,
        //        Key.Right,
        //        Key.Home,
        //        Key.End,
        //    };


        //    if (available.Contains(e.Key))
        //    {
        //        if ((e.Key == Key.OemMinus || e.Key == Key.Subtract) && (textbox.Text.Contains('-') || textbox.CaretIndex > 0)
        //            || (e.Key == Key.OemPeriod || e.Key == Key.Decimal) && (textbox.Text.Contains(",") || textbox.Text.Contains(".")))
        //        {
        //            e.Handled = true;
        //        }
        //    } else
        //    {
        //        e.Handled = true;
        //    }
        //}

        //private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    var textbox = sender as TextBox;

        //    var index = textbox.CaretIndex;
        //    if (textbox.Text.Contains("-."))
        //    {
        //        textbox.Text = textbox.Text.Replace("-.", "-0.");
        //        textbox.CaretIndex = index + 1;
        //    }

        //    SendMatrix();
        //}

        //public void TextBox_MouseWheel(object sender, MouseWheelEventArgs e)
        //{
        //    var textbox = sender as TextBox;
        //    var scrollBar = sender as System.Windows.Controls.Primitives.ScrollBar;

        //    string text = "";

        //    if (textbox != null)
        //    {
        //        text = textbox.Text;
        //    } else if (scrollBar !=  null)
        //    {
        //        text = scrollBar.Value.ToString();
        //    }
            
        //    double val = ConvertToDouble(text);
        //    val += e.Delta > 0 ? 0.1: -0.1;

        //    if (textbox != null)
        //    {
        //        textbox.Text = val.ToString();
        //    }
        //    else if (scrollBar != null)
        //    {
        //        if (val > scrollBar.Maximum)
        //        {
        //            val -= scrollBar.Maximum - scrollBar.Minimum;
        //        } else if (val < scrollBar.Minimum)
        //        {
        //            val += scrollBar.Maximum - scrollBar.Minimum;
        //        }
        //        scrollBar.Value = val;
        //    }
        //}

        //private void SendMatrix()
        //{
        //    List<double> list = new List<double>(16);

        //    MatrixTab.Children.Cast<WrapPanel>()
        //        .Select(x => x.Children.Cast<UIElement>().Where(el => el is TextBox))
        //        .Select(x=>
        //        {
        //            list.AddRange(x.Cast<TextBox>().Select(t => ConvertToDouble(t.Text)));
        //            return true;
        //        })
        //        .ToList();

        //    if (list.Count < 16) return;


        //    Matrix3D matrix = new Matrix3D(list[0], list[1], list[2], list[3],
        //                                   list[4], list[5], list[6], list[7],
        //                                   list[8], list[9], list[10], list[11],
        //                                   list[12], list[13], list[14], list[15]);

        //    matrixConverAction.Invoke(matrix);
        //}

        //static double ConvertToDouble(string str)
        //{
        //    str = str.Replace(".", ",");
        //    if (str == "-" || str == "-0," || str == "," || str == "")
        //    {
        //        str = "0";
        //    }
        //    else
        //    {
        //        if (str.LastIndexOf(",") == str.Length - 1)
        //        {
        //            str = str.Replace(",", "");
        //        }
        //    }
        //    double.TryParse(str, out double val);
        //    return val;
        //}

        //bool isSaved = false;

        //private void Window_Closed(object sender, EventArgs e)
        //{
        //    if (isSaved) return;

        //    abortAction.Invoke();
        //}

        //private void Save_Click(object sender, RoutedEventArgs e)
        //{
        //    SendMatrix();
        //    saveAction.Invoke();
        //    isSaved = true;
        //    Close();
        //}

        //private void Abort_Click(object sender, RoutedEventArgs e)
        //{
        //    isSaved = false;
        //    Close();
        //}

        //private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        //{
        //    (sender as TextBox).Text = ConvertToDouble((sender as TextBox).Text).ToString().Replace(",", ".");
        //}

        //TextBlock lastTab;

        //private void Tab_Textbox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        //{
        //    var textBlock = sender as TextBlock;

        //    if (lastTab != textBlock)
        //    {
        //        lastTab = textBlock;

        //        if (lastTab == Matrix_Tab_Textblock)
        //        {
        //            MatrixTab.Visibility = Visibility.Visible;
        //            Matrix_Tab_Textblock.Background = new SolidColorBrush(Colors.Purple);
        //            ParametersTab.Visibility = Visibility.Collapsed;
        //            Paramters_Tab_Textblock.Background = new SolidColorBrush(Colors.Transparent);
        //        }
        //        else
        //        {
        //            MatrixTab.Visibility = Visibility.Collapsed;
        //            Matrix_Tab_Textblock.Background = new SolidColorBrush(Colors.Transparent);
        //            ParametersTab.Visibility = Visibility.Visible;
        //            Paramters_Tab_Textblock.Background = new SolidColorBrush(Colors.Purple);
        //        }
        //    }

        //}

        //private void ConvertFromParametersToMatrix(Matrix3D matrix)
        //{
        //    List<TextBox> list = new List<TextBox>(16);

        //    MatrixTab.Children.Cast<WrapPanel>()
        //        .Select(x => x.Children.Cast<UIElement>().Where(el => el is TextBox))
        //        .Select(x =>
        //        {
        //            list.AddRange(x.Cast<TextBox>());
        //            return true;
        //        })
        //        .ToList();

        //    if (list.Count < 16) return;

        //    list[0].Text = matrix.M11.ToString(); 
        //    list[1].Text = matrix.M12.ToString(); 
        //    list[2].Text = matrix.M13.ToString(); 
        //    list[3].Text = matrix.M14.ToString();
                                           
        //    list[4].Text = matrix.M21.ToString(); 
        //    list[5].Text = matrix.M22.ToString(); 
        //    list[6].Text = matrix.M23.ToString(); 
        //    list[7].Text = matrix.M24.ToString();

        //    list[8].Text = matrix.M31.ToString();
        //    list[9].Text = matrix.M32.ToString(); 
        //    list[10].Text = matrix.M33.ToString(); 
        //    list[11].Text = matrix.M34.ToString();
                                           
        //    list[12].Text = matrix.OffsetX.ToString(); 
        //    list[13].Text = matrix.OffsetY.ToString(); 
        //    list[14].Text = matrix.OffsetZ.ToString();
        //    list[15].Text = matrix.M44.ToString();

        //}

        //private void ScrollBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        //{
        //    double a_x = Math.Sin(ox_Scroll.Value * Math.PI);
        //    double b_x = Math.Cos(ox_Scroll.Value * Math.PI);
        //    Matrix3D rotateToOx = new Matrix3D(1, 0, 0, 0,
        //                                       0, b_x, a_x, 0,
        //                                       0, -a_x, b_x, 0,
        //                                       0, 0, 0, 1);

        //    ox_Textblock.Text = ox_Scroll.Value.ToString();

        //    double a_y = Math.Sin(oy_Scroll.Value * Math.PI);
        //    double b_y = Math.Cos(oy_Scroll.Value * Math.PI);
        //    Matrix3D rotateToOy = new Matrix3D(b_y, 0, -a_y, 0,
        //                                       0, 1, 0, 0,
        //                                       a_y, 0, b_y, 0,
        //                                       0, 0, 0, 1);

        //    oy_Textblock.Text = oy_Scroll.Value.ToString();

        //    double a_z = Math.Sin(oz_Scroll.Value * Math.PI);
        //    double b_z = Math.Cos(oz_Scroll.Value * Math.PI);
        //    Matrix3D rotateToOz = new Matrix3D(b_z, a_z, 0, 0,
        //                                       -a_z, b_z, 0, 0,
        //                                       0, 0, 1, 0,
        //                                       0, 0, 0, 1);

        //    oz_Textblock.Text = oz_Scroll.Value.ToString();

        //    ConvertFromParametersToMatrix(rotateToOx * rotateToOy * rotateToOz);

        //    matrixConverAction.Invoke(rotateToOx * rotateToOy * rotateToOz);
        //}

        //private string GetStringOnLine(string offset, CustomLine line)
        //{
        //    return $"{offset}({line.X1:00.00};{line.Y1:00.00};{line.Z1:00.00}) " +
        //           $"({line.X2:00.00};{line.Y2:00.00};{line.Z2:00.00})\n" +
        //           $"{offset}  {line.Equation}\n";
        //}

        //private string GetStringOnObjects(string offset, IEnumerable<IMyObject> objects)
        //{
        //    string str = $"{offset}---\n";
        //    foreach (var o in objects)
        //    {
        //        if (o is CustomLine)
        //        {
        //            str += GetStringOnLine(offset, o as CustomLine);
        //        }

        //        if (o is CustomGroup)
        //        {
        //            str += GetStringOnObjects(offset + defaultOffset, (o as CustomGroup).GetObjects());
        //        }
        //    }

        //    str += $"{offset}---\n";

        //    return str;
        //}

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Topmost = (sender as CheckBox).IsChecked == true;
        }
    }
}
