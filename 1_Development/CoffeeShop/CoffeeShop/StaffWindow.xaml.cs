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

namespace CoffeeShop.View
{
    /// <summary>
    /// Interaction logic for StaffWindow.xaml
    /// </summary>
    public partial class StaffWindow : Window
    {
        public StaffWindow()
        {
            InitializeComponent();
        }
        private void bdrOrder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            StaffFrame.Visibility = Visibility.Visible;
            StaffFrame.Navigate(new CoffeeShop.Staff_Order());
        }

        private void bdrMenu_MouseDown(object sender, MouseButtonEventArgs e)
        {
            StaffFrame.Visibility = Visibility.Visible;
            StaffFrame.Navigate(new CoffeeShop.Staff_Menu());
        }

        private void bdrDepot_MouseDown(object sender, MouseButtonEventArgs e)
        {
            StaffFrame.Visibility = Visibility.Visible;
            StaffFrame.Navigate(new CoffeeShop.Staff_Depot());
        }

        private void bdrStatistics_MouseDown(object sender, MouseButtonEventArgs e)
        {
            StaffFrame.Visibility = Visibility.Visible;
            StaffFrame.Navigate(new CoffeeShop.Staff_Statistics());
        }
    }
}
