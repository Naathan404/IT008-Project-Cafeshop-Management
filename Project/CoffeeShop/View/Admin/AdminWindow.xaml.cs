using CoffeeShop.View.General;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace CoffeeShop.View.Admin
{
    /// <summary>
    /// Interaction logic for AdminWindow.xaml
    /// </summary>
    public partial class AdminWindow : Window
    {
        private bool _isExpanded = false;
        private float _minimumNavigationBarWidth = 80;
        private float _maximumNavigationBarWidth = 200;
        private float _animationDuration = 200;
        private bool _isAnimating = false;

        /// Account informations
        /// Name, Role, Phonenumber, Email, BaseSalary
        private CoffeeShop.Models.Staff _account;

        /// Tạo các trang
        private AdminCustomerManagementPage _customerManagementPage = new AdminCustomerManagementPage();
        private DepotManagementPage _depotManagementPage = new DepotManagementPage();
        private StaffManagementPage _staffManagementPage = new StaffManagementPage();
        private HistoryManagementPage _historyManagementPage = new HistoryManagementPage();
        private MenuManagementPage _menuManagementPage = new MenuManagementPage();
        private StatisticPage _statisticPage = new StatisticPage();
        private DiscountManagementPage _discountManagementPage = new DiscountManagementPage();

        private readonly Brush _colorActive = (Brush)(new BrushConverter().ConvertFrom("#c7a57a") ?? Brushes.Transparent);
        private readonly Brush _colorNormal = Brushes.Transparent;

        // Constructor
        public AdminWindow(CoffeeShop.Models.Staff account)
        {
            InitializeComponent();
            StaffFrame.Navigate(_statisticPage);
            bdrStaffWindowFunction.Width = _minimumNavigationBarWidth;
            _account = account;
            SetActiveTabUI("Statistics");
        }

        #region Button Events
        private void SetActiveTabUI(string tabName)
        {
            // 1. Gom nhóm các Border để xử lý hàng loạt
            Border[] beforeBorders = { bdrAccount_Before, bdrCustomer_Before, bdrHistory_Before, bdrDepot_Before, bdrEmployee_Before, bdrStatistics_Before, bdrMenu_Before, bdrDiscount_Before };
            Border[] afterBorders = { bdrAccount_After, bdrCustomer_After, bdrHistory_After, bdrDepot_After, bdrEmployee_After, bdrStatistics_After, bdrMenu_After, bdrDiscount_After };

            // 2. Reset tất cả về trong suốt (Xóa dấu vết cũ)
            foreach (var b in beforeBorders) if (b != null) b.Background = _colorNormal;
            foreach (var b in afterBorders) if (b != null) b.Background = _colorNormal;

            // 3. Highlight tab được chọn (Tô màu cho cả Before và After để đồng bộ khi thu phóng)
            switch (tabName)
            {
                case "Account":
                    bdrAccount_Before.Background = bdrAccount_After.Background = _colorActive;
                    break;
                case "Customer":
                    bdrCustomer_Before.Background = bdrCustomer_After.Background = _colorActive;
                    break;
                case "History":
                    bdrHistory_Before.Background = bdrHistory_After.Background = _colorActive;
                    break;
                case "Depot":
                    bdrDepot_Before.Background = bdrDepot_After.Background = _colorActive;
                    break;
                case "Staff":
                    bdrEmployee_After.Background = bdrEmployee_Before.Background = _colorActive;
                    break;
                case "Statistics":
                    bdrStatistics_After.Background = bdrStatistics_Before.Background = _colorActive;
                    break;
                case "Menu":
                    bdrMenu_After.Background = bdrMenu_Before.Background = _colorActive;
                    break;
                case "Discount":
                    bdrDiscount_After.Background = bdrDiscount_Before.Background = _colorActive;
                    break;
            }
        }
        private void bdrAccount_MouseDown(object sender, MouseButtonEventArgs e)
        {
            StaffFrame.Visibility = Visibility.Visible;
            StaffFrame.Navigate(new AccountPage(_account, this));
            SetActiveTabUI("Account");
        }
        private void bdrCustomer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            StaffFrame.Visibility = Visibility.Visible;
            StaffFrame.Navigate(_customerManagementPage);
            SetActiveTabUI("Customer");
        }

        private void bdrDepot_MouseDown(object sender, MouseButtonEventArgs e)
        {
            StaffFrame.Visibility = Visibility.Visible;
            StaffFrame.Navigate(_depotManagementPage);
            SetActiveTabUI("Depot");
        }

        private void bdrEmployee_MouseDown(object sender, MouseButtonEventArgs e)
        {
            StaffFrame.Visibility = Visibility.Visible;
            StaffFrame.Navigate(_staffManagementPage);
            SetActiveTabUI("Staff");
        }

        private void bdrStatistics_MouseDown(object sender, MouseButtonEventArgs e)
        {
            StaffFrame.Visibility = Visibility.Visible;
            StaffFrame.Navigate(_statisticPage);
            SetActiveTabUI("Statistics");
        }
        private void bdrDiscount_MouseDown(object sender, MouseButtonEventArgs e)
        {
            StaffFrame.Visibility = Visibility.Visible;
            StaffFrame.Navigate(_discountManagementPage);
            SetActiveTabUI("Discount");
        }

        private void bdrHistory_MouseDown(object sender, MouseButtonEventArgs e)
        {
            StaffFrame.Visibility = Visibility.Visible;
            StaffFrame.Navigate(_historyManagementPage);
            SetActiveTabUI("History");
        }

        private void bdrMenu_MouseDown(object sender, MouseButtonEventArgs e)
        {
            StaffFrame.Visibility = Visibility.Visible;
            StaffFrame.Navigate(_menuManagementPage);
            SetActiveTabUI("Menu");
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
            bdrEmployee_Before.Visibility = Visibility.Collapsed;
            bdrMenu_Before.Visibility = Visibility.Collapsed;
            bdrDepot_Before.Visibility = Visibility.Collapsed;
            bdrStatistics_Before.Visibility = Visibility.Collapsed;
            bdrDiscount_Before.Visibility = Visibility.Collapsed;
            bdrHistory_Before.Visibility = Visibility.Collapsed;
            bdrCustomer_Before.Visibility = Visibility.Collapsed;

            //cho cac element after hien ra
            bdrAccount_After.Visibility = Visibility.Visible;
            bdrEmployee_After.Visibility = Visibility.Visible;
            bdrMenu_After.Visibility = Visibility.Visible;
            bdrDepot_After.Visibility = Visibility.Visible;
            bdrStatistics_After.Visibility = Visibility.Visible;
            bdrDiscount_After.Visibility = Visibility.Visible;
            bdrHistory_After.Visibility = Visibility.Visible;
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
                bdrEmployee_After.Visibility = Visibility.Collapsed;
                bdrMenu_After.Visibility = Visibility.Collapsed;
                bdrDepot_After.Visibility = Visibility.Collapsed;
                bdrStatistics_After.Visibility = Visibility.Collapsed;
                bdrDiscount_After.Visibility = Visibility.Collapsed;
                bdrHistory_After.Visibility = Visibility.Collapsed;
                bdrCustomer_After.Visibility = Visibility.Collapsed;

                //cho cac element before hien ra
                bdrAccount_Before.Visibility = Visibility.Visible;
                bdrEmployee_Before.Visibility = Visibility.Visible;
                bdrMenu_Before.Visibility = Visibility.Visible;
                bdrDepot_Before.Visibility = Visibility.Visible;
                bdrStatistics_Before.Visibility = Visibility.Visible;
                bdrDiscount_Before.Visibility = Visibility.Visible;
                bdrHistory_Before.Visibility = Visibility.Visible;
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
