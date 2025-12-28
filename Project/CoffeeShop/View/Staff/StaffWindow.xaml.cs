using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using CoffeeShop.View.General;
using CoffeeShop.ViewModels.StaffVM;

namespace CoffeeShop.View.Staff
{
    public partial class StaffWindow : Window
    {
        private bool _isExpanded = false;
        private float _minimumNavigationBarWidth = 80;
        private float _maximumNavigationBarWidth = 200;
        private float _animationDuration = 200;
        private bool _isAnimating = false;

        /// Inial all pages
        private Staff_Order _orderPage = new Staff_Order();
        private Staff_Menu _menuPage = new Staff_Menu();
        private Staff_Table _tablePage = new Staff_Table();
        private Staff_Customer _customerPage = new Staff_Customer();
        private Staff_History _historyPage = new Staff_History();
        private Staff_Depot _depotPage = new Staff_Depot();

        /// Account informations
        /// Name, Role, Phonenumber, Email, BaseSalary
        private CoffeeShop.Models.Staff _account;
        
        // Constructor
        public StaffWindow(CoffeeShop.Models.Staff account)
        {
            InitializeComponent();
            Debug.WriteLine($"Width = {this.ActualWidth}, Height = {this.ActualHeight}");
            StaffFrame.Navigate(new Staff_Order());
            bdrStaffWindowFunction.Width = _minimumNavigationBarWidth;
            _account = account;
        }

        #region Button Events
        private void bdrAccount_MouseDown(object sender, MouseButtonEventArgs e)
        {
            StaffFrame.Visibility = Visibility.Visible;
            StaffFrame.Navigate(new AccountPage(_account, this));
        }
        private void bdrOrder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            StaffFrame.Visibility = Visibility.Visible;
            StaffFrame.Navigate(_orderPage);
        }

        private void bdrMenu_MouseDown(object sender, MouseButtonEventArgs e)
        {
            StaffFrame.Visibility = Visibility.Visible;
            StaffFrame.Navigate(_menuPage);
        }

        private void bdrDepot_MouseDown(object sender, MouseButtonEventArgs e)
        {
            StaffFrame.Visibility = Visibility.Visible;
            StaffFrame.Navigate(_depotPage);
        }

        private void bdrTable_MouseDown(object sender, MouseButtonEventArgs e)
        {
            StaffFrame.Visibility = Visibility.Visible;
            StaffFrame.Navigate(_tablePage);
        }

        private void bdrHistory_MouseDown(object sender, MouseButtonEventArgs e)
        {
            StaffFrame.Visibility = Visibility.Visible;
            StaffFrame.Navigate(_historyPage);
        }

        private void bdrCustomer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            StaffFrame.Visibility = Visibility.Visible;
            StaffFrame.Navigate(_customerPage);
        }

        private void bdrStaffWindowFunction_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_isAnimating) return;       // Nếu có 1 animation đang hoạt động thì không làm gì cả

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
        #endregion

        /// <summary>
        /// Expands the navigation bar to its maximum width and updates the visibility of its elements.
        /// </summary>
        /// <remarks>This method animates the expansion of the navigation bar to the width specified by
        /// the  maximum navigation bar width. It also updates the visibility of the navigation bar's  elements, hiding
        /// the "before" elements and displaying the "after" elements.</remarks>
        private void MaximizeNavigationBar()
        {
            _isAnimating = true;            // Bật cờ đang chạy animation

            double dwidth = _maximumNavigationBarWidth;
            // animation xuat hien
            var animMaximizeNavigationBar = new DoubleAnimation
            {
                To = dwidth,
                Duration = TimeSpan.FromMilliseconds(_animationDuration),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            animMaximizeNavigationBar.Completed += (s, e) =>
            {
                _isAnimating = false;       // Đã hoàn thành animation
            };

            //cho cac element before an di
            bdrAccount_Before.Visibility = Visibility.Collapsed;
            bdrOrder_Before.Visibility = Visibility.Collapsed;
            bdrMenu_Before.Visibility = Visibility.Collapsed;
            bdrDepot_Before.Visibility = Visibility.Collapsed;
            bdrHistory_Before.Visibility = Visibility.Collapsed;
            bdrTable_Before.Visibility = Visibility.Collapsed;
            bdrCustomer_Before.Visibility = Visibility.Collapsed;

            //cho cac element after hien ra
            bdrAccount_After.Visibility = Visibility.Visible;
            bdrOrder_After.Visibility = Visibility.Visible;
            bdrMenu_After.Visibility = Visibility.Visible;
            bdrDepot_After.Visibility = Visibility.Visible;
            bdrHistory_After.Visibility = Visibility.Visible;
            bdrTable_After.Visibility = Visibility.Visible;
            bdrCustomer_After.Visibility = Visibility.Visible;

            bdrStaffWindowFunction.BeginAnimation(Border.WidthProperty, animMaximizeNavigationBar);
        }

        /// <summary>
        /// Minimizes the navigation bar by animating its width and updating the visibility of related elements.
        /// </summary>
        /// <remarks>This method reduces the width of the navigation bar to a predefined minimum value
        /// using an animation.  Once the animation completes, it updates the visibility of specific UI elements to
        /// reflect the minimized state.</remarks>
        private void MinimizeNavigationBar()
        {
            _isAnimating = true;            // Bật cờ đang chạy animation

            double dwidth = _minimumNavigationBarWidth;
            //animation thu gon
            var animMinimizeNavigationBar = new DoubleAnimation
            {
                To = dwidth,
                Duration = TimeSpan.FromMilliseconds(_animationDuration),
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
                bdrHistory_After.Visibility = Visibility.Collapsed;
                bdrTable_After.Visibility = Visibility.Collapsed;
                bdrCustomer_After.Visibility = Visibility.Collapsed;

                //cho cac element before hien ra
                bdrAccount_Before.Visibility = Visibility.Visible;
                bdrOrder_Before.Visibility = Visibility.Visible;
                bdrMenu_Before.Visibility = Visibility.Visible;
                bdrDepot_Before.Visibility = Visibility.Visible;
                bdrHistory_Before.Visibility = Visibility.Visible;
                bdrTable_Before.Visibility = Visibility.Visible;
                bdrCustomer_Before.Visibility = Visibility.Visible;

                _isAnimating = false;       // Kết thúc animation
            };

            bdrStaffWindowFunction.BeginAnimation(Border.WidthProperty, animMinimizeNavigationBar);
        }

        /// <summary>
        /// Handles the PreviewMouseDown event to minimize the navigation bar when certain conditions are met.
        /// </summary>
        /// <remarks>This method minimizes the navigation bar if it is currently expanded and the mouse
        /// click occurs outside the specified area. The event is marked as handled to prevent further
        /// processing.</remarks>
        /// <param name="sender">The source of the event, typically the control that was clicked.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        private void PreviewMouseDownEvt(object sender, MouseButtonEventArgs e)
        {
            if (_isAnimating) return;       // Nếu có 1 animation đang hoạt động thì không làm gì cả

            if (_isExpanded)
            {
                if (bdrStaffWindowFunction.IsMouseOver == false)
                {
                    MinimizeNavigationBar();
                    _isExpanded = false;
                    e.Handled = true;
                }
            }
        }
    }
}
