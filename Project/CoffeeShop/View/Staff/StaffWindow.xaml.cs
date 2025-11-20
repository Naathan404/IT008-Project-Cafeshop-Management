using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace CoffeeShop.View.Staff
{
    public partial class StaffWindow : Window
    {
        private bool _isExpanded = false;
        private float _minimumNavigationBarWidth = 80;
        private float _maximumNavigationBarWidth = 200;
        public StaffWindow()
        {
            InitializeComponent();
            StaffFrame.Navigate(new Staff_Order());
            bdrStaffWindowFunction.Width = _minimumNavigationBarWidth;
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

        private void bdrTable_MouseDown(object sender, MouseButtonEventArgs e)
        {
            StaffFrame.Visibility = Visibility.Visible;
            StaffFrame.Navigate(new Staff_Table());
        }

        private void bdrStaffWindowFunction_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!_isExpanded)    // Nếu Navigation bar đang được thu gọn
            {
                MaximizeNavigationBar();
            }
            else                // Nếu Navigation bar đang được mở rộng 
            {
                MinimizeNavigationBar();
            }
            _isExpanded = !_isExpanded;
        }

        // Mở rộng Navigation bar
        private void MaximizeNavigationBar()
        {
            double dwidth = _maximumNavigationBarWidth;
            // animation xuat hien
            var animMaximizeNavigationBar = new DoubleAnimation
            {
                To = dwidth,
                Duration = TimeSpan.FromMilliseconds(250),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            //cho cac element before an di
            bdrAccount_Before.Visibility = Visibility.Collapsed;
            bdrOrder_Before.Visibility = Visibility.Collapsed;
            bdrMenu_Before.Visibility = Visibility.Collapsed;
            bdrDepot_Before.Visibility = Visibility.Collapsed;
            bdrStatistics_Before.Visibility = Visibility.Collapsed;
            bdrTable_Before.Visibility = Visibility.Collapsed;

            //cho cac element after hien ra
            bdrAccount_After.Visibility = Visibility.Visible;
            bdrOrder_After.Visibility = Visibility.Visible;
            bdrMenu_After.Visibility = Visibility.Visible;
            bdrDepot_After.Visibility = Visibility.Visible;
            bdrStatistics_After.Visibility = Visibility.Visible;
            bdrTable_After.Visibility = Visibility.Visible;

            bdrStaffWindowFunction.BeginAnimation(Border.WidthProperty, animMaximizeNavigationBar);
        }

        // Thu nhỏ Navigation bar
        private void MinimizeNavigationBar()
        {
            double dwidth = _minimumNavigationBarWidth;
            //animation thu gon
            var animMinimizeNavigationBar = new DoubleAnimation
            {
                To = dwidth,
                Duration = TimeSpan.FromMilliseconds(250),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };

            // Chạy animation
            animMinimizeNavigationBar.Completed += (s, e) => // Đảm bảo vReduceBdr hoàn thành thì mới chạy đoạn code bên tronng
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
            };
            bdrStaffWindowFunction.BeginAnimation(Border.WidthProperty, animMinimizeNavigationBar);
        }

        private void PreviewMouseDownEvt(object sender, MouseButtonEventArgs e)
        {
            if (_isExpanded)
            {
                if (bdrStaffWindowFunction.IsMouseOver == false)
                {
                    MinimizeNavigationBar();
                    _isExpanded = false;
                }
            }
        }
    }
}
