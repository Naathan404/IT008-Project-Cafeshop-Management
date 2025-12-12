using CoffeeShop.ViewModels.StaffVM;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CoffeeShop.View.Staff
{
    public partial class AddCustomerWindow : Window
    {
        AddCustomerViewModel _viewModel;
        public AddCustomerWindow(StaffOrderViewModel parent_viewModel)
        {
            InitializeComponent();
            _viewModel = new AddCustomerViewModel(parent_viewModel);
            DataContext = _viewModel;
            _viewModel.CloseWindowAction = new Action(() => this.Close());
        }
        public AddCustomerWindow(StaffOrderViewModel parent_viewModel, string phonenumber)
        {
            InitializeComponent();
            _viewModel = new AddCustomerViewModel(parent_viewModel);
            this.DataContext = _viewModel;
            // Gán phone number đã nhập cho window
            _viewModel.CustomerPhoneNumber = phonenumber;
            _viewModel.CloseWindowAction = new Action(() => this.Close());
        }
        #region Button Events
        private void btn_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (sender is Border border)
            {
                border.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D4BA98"));
                if (border.Child is TextBlock txtb)
                {
                    txtb.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#340D05"));
                }
            }
        }
        private void btn_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (sender is Border border)
            {
                border.Background = Brushes.Transparent; // trả về nền mặc định
                if (border.Child is TextBlock txtb)
                {
                    txtb.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#766839"));
                }
            }
        }
        private void bdrExit_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.Close();
        }
        #endregion
    }
}
