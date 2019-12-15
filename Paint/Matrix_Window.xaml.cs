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
    public partial class Matrix_Window : Window
    {
        Action<Matrix3D> matrixConverAction;
        Action saveAction;
        Action abortAction;

        private Matrix_Window()
        {
            matrixConverAction = _ => {};
            saveAction = () => {};
            abortAction = () => {};
            InitializeComponent();
        }

        public Matrix_Window(Action<Matrix3D> matrixConverAction, Action saveAction, Action abortAction) :this()
        {
            this.matrixConverAction = matrixConverAction;
            this.saveAction = saveAction;
            this.abortAction = abortAction;
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
            } else
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

            SendMatrix();
        }

        public void TextBox_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var textbox = sender as TextBox;

            var text = textbox.Text;

            double val = ConvertToDouble(text);
            val += e.Delta > 0 ? 0.1: -0.1;

            textbox.Text = val.ToString();
        }

        private void SendMatrix()
        {
            List<double> list = new List<double>(16);

            MatrixPanel.Children.Cast<WrapPanel>()
                .Select(x => x.Children.Cast<UIElement>().Where(el => el is TextBox))
                .Select(x=>
                {
                    list.AddRange(x.Cast<TextBox>().Select(t => ConvertToDouble(t.Text)));
                    return true;
                })
                .ToList();

            if (list.Count < 16) return;


            Matrix3D matrix = new Matrix3D(list[0], list[1], list[2], list[3],
                                           list[4], list[5], list[6], list[7],
                                           list[8], list[9], list[10], list[11],
                                           list[12], list[13], list[14], list[15]);

            matrixConverAction.Invoke(matrix);
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

        bool isSaved = false;

        private void Window_Closed(object sender, EventArgs e)
        {
            if (isSaved) return;

            abortAction.Invoke();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            SendMatrix();
            saveAction.Invoke();
            isSaved = true;
            Close();
        }

        private void Abort_Click(object sender, RoutedEventArgs e)
        {
            isSaved = false;
            Close();
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            (sender as TextBox).Text = ConvertToDouble((sender as TextBox).Text).ToString().Replace(",", ".");
        }
    }
}
