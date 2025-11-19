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
using System.Windows.Media.Animation;

namespace CoffeeShop.View.Staff
{
    public partial class StaffWindow : Window
    {
        private bool isExpanded = false;
        public StaffWindow()
        {
            InitializeComponent();
            StaffFrame.Navigate(new Staff_Order());
        }
        private void bdrOrder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            StaffFrame.Visibility = Visibility.Visible;
            StaffFrame.Navigate(new Staff_Order());
        }

        private void bdrMenu_MouseDown(object sender, MouseButtonEventArgs e)
        {
            StaffFrame.Visibility = Visibility.Visible;
            StaffFrame.Navigate(new Staff_Menu());
        }

        private void bdrDepot_MouseDown(object sender, MouseButtonEventArgs e)
        {
            StaffFrame.Visibility = Visibility.Visible;
            StaffFrame.Navigate(new Staff_Depot());
        }

        private void bdrStatistics_MouseDown(object sender, MouseButtonEventArgs e)
        {
            StaffFrame.Visibility = Visibility.Visible;
            StaffFrame.Navigate(new Staff_Statistics());
        }

        private void bdrTable_After_MouseDown(object sender, MouseButtonEventArgs e)
        {
            StaffFrame.Visibility = Visibility.Visible;
            StaffFrame.Navigate(new Staff_Table());
        }

        private void bdrTable_Before_MouseDown(object sender, MouseButtonEventArgs e)
        {
            StaffFrame.Visibility = Visibility.Visible;
            StaffFrame.Navigate(new Staff_Table());
        }

        private void bdrStaffWindowFunction_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Visibility visibility = Visibility.Visible;
            double dwidth = isExpanded ? 80 : 200;

            // animation xuat hien
            var vShowBdr = new DoubleAnimation
            {
                To = dwidth,
                Duration = TimeSpan.FromMilliseconds(250),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            bdrStaffWindowFunction.BeginAnimation(Border.WidthProperty, vShowBdr);
            
            //animation thu gon
            var vReduceBdr = new DoubleAnimation
            {
                To = dwidth,
                Duration = TimeSpan.FromMilliseconds(250),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };
            bdrStaffWindowFunction.BeginAnimation(Border.WidthProperty, vReduceBdr);

            if (! isExpanded) // đang thu gon tab bar
            {
                //cho cac element before an di
                bdrAccount_Before.Visibility = Visibility.Collapsed;
                bdrOrder_Before.Visibility = Visibility.Collapsed;
                bdrMenu_Before.Visibility = Visibility.Collapsed;
                bdrDepot_Before.Visibility = Visibility.Collapsed;
                bdrStatistics_Before.Visibility = Visibility.Collapsed;
                bdrTable_Before.Visibility= Visibility.Collapsed;
                

                //cho cac element after hien ra
                bdrAccount_After.Visibility = Visibility.Visible;
                bdrOrder_After.Visibility = Visibility.Visible;
                bdrMenu_After.Visibility = Visibility.Visible;
                bdrDepot_After.Visibility = Visibility.Visible;
                bdrStatistics_After.Visibility = Visibility.Visible;
                bdrTable_After.Visibility = Visibility.Visible;
            }
            else // đang hien thi tab bar 
            {
                //cho cac element after an di
                bdrAccount_After.Visibility = Visibility.Collapsed;
                bdrOrder_After.Visibility = Visibility.Collapsed;
                bdrMenu_After.Visibility = Visibility.Collapsed;
                bdrDepot_After.Visibility = Visibility.Collapsed;
                bdrStatistics_After.Visibility = Visibility.Collapsed;
                bdrTable_After.Visibility = Visibility.Collapsed;

                //cho cac element before hien ra
                bdrAccount_Before.Visibility = Visibility.Visible;
                bdrOrder_Before.Visibility = Visibility.Visible;
                bdrMenu_Before.Visibility = Visibility.Visible;
                bdrDepot_Before.Visibility = Visibility.Visible;
                bdrStatistics_Before.Visibility = Visibility.Visible;
                bdrTable_Before.Visibility = Visibility.Visible;
            }
            isExpanded = !isExpanded;
        }
    }
}
