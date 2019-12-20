using Paint.classes;
using Paint.interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;

namespace Paint
{
    /// <summary>
    /// Логика взаимодействия для Matrix.xaml
    /// </summary>
    public partial class AdditionalLines_Window : Window
    {
        Func<IEnumerable<IMyObject>> getSelectedObjects;
        public AdditionalLines_Window(Func<IEnumerable<IMyObject>> getSelectedObjects)
        {
            this.getSelectedObjects = getSelectedObjects;

            InitializeComponent();
        }

        public int GetValue()
        {
            if (perpendicular.IsChecked == true)
            {
                return 1;
            }

            if (median.IsChecked == true)
            {
                return 2;
            }

            if (bisector.IsChecked == true)
            {
                return 3;
            }

            return 0;
        }

        public void Reset()
        {
            perpendicular.IsChecked = false;
            median.IsChecked = false;
            bisector.IsChecked = false;
        }

        private void bisector_Checked(object sender, RoutedEventArgs e)
        {
            var selected = getSelectedObjects.Invoke();
            if (selected.Count() != 2 || selected.Where(x => x is CustomLine).Count() != 2)
            {
                MessageBox.Show("Для биссектрисы необходимо выбрать только две линии");
                bisector.IsChecked = false;
                return;
            }
        }

        private void perpendicular_Checked(object sender, RoutedEventArgs e)
        {
            var selected = getSelectedObjects.Invoke();
            if (selected.Count() != 1 || selected.Where(x => x is CustomLine).Count() != 1)
            {
                MessageBox.Show("Для перпендикуляра необходимо выбрать только одну линию");
                perpendicular.IsChecked = false;
                return;
            }
        }

        private void median_Checked(object sender, RoutedEventArgs e)
        {
            var selected = getSelectedObjects.Invoke();
            if (selected.Count() != 1 || selected.Where(x => x is CustomLine).Count() != 1)
            {
                MessageBox.Show("Для медиана необходимо выбрать только одну линию");
                median.IsChecked = false;
                return;
            }
        }
    }

}
