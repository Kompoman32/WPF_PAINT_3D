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
using System.Windows.Shapes;

namespace Paint
{
    /// <summary>
    /// Логика взаимодействия для Info.xaml
    /// </summary>
    public partial class Info : Window
    {
        public Info()
        {
            InitializeComponent();
        }
        public Info(Point point, IEnumerable<IMyObject> objects)
        {
            SetPoint(point);
            SetObjects(objects);
            InitializeComponent();
        }

        string defaultOffset = "    ";

        public void SetPoint(Point point)
        {
            this.point.Content = $"( {point.X:00.00} | {point.Y:00.00} )";
        }

        public void SetPointOnLine(Point point)
        {
            this.pointOnLine.Content = $"( {point.X:00.00} | {point.Y:00.00} )";
        }

        public void SetObjects(IEnumerable<IMyObject> objects)
        {
            var str = GetStringOnObjects("", objects);
            LinesBlock.Text = str;
        }

        private string GetStringOnLine(string offset, CustomLine line)
        {
            return $"{offset}({line.X1};{line.Y1};{line.Z1}) " +
                   $"({line.X2};{line.Y2};{line.Z2})\n" +
                   $"{offset}  {line.Equation}\n";
        }

        private string GetStringOnObjects(string offset, IEnumerable<IMyObject> objects)
        {
            string str = $"{offset}---\n";
            foreach (var o in objects)
            {
                if (o is CustomLine)
                {
                    str += GetStringOnLine(offset, o as CustomLine);
                }

                if (o is CustomGroup)
                {
                    str += GetStringOnObjects(offset + defaultOffset, (o as CustomGroup).GetObjects());
                }
            }

            str += $"{offset}---\n";

            return str;
        }

        bool isClosing = false;

        public void CloseThis()
        {
            isClosing = true;
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = !isClosing;
        }
}
}
