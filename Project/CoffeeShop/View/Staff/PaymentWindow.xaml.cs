using CoffeeShop.View.Controls;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace CoffeeShop.ViewModels.StaffVM
{
    public partial class PaymentWindow : Window
    {
        StaffOrderViewModel _viewModel;
        public PaymentWindow(StaffOrderViewModel _parentViewModel)
        {
            InitializeComponent();
            _viewModel = _parentViewModel;
            this.DataContext = _viewModel;
        }
        #region Button Events
        private void border_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Border bdr)
            {
                bdr.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#766839"));
                var tb = bdr.Child as TextBlock;
                if (tb != null)
                    tb.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EDE2D3"));

                bdr.Effect = new DropShadowEffect
                {
                    Color = (Color)ColorConverter.ConvertFromString("#766839"),
                    Direction = 315,
                    ShadowDepth = 4,
                    BlurRadius = 10,
                    Opacity = 0.6
                };
            }
        }

        private void border_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Border bdr)
            {
                bdr.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EDE2D3"));
                var tb = bdr.Child as TextBlock;
                if (tb != null)
                    tb.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#766839"));

                bdr.Effect = null;
            }
        }
        private void bdrCancelPayOrder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var result = CustomMessageBox.Show("Bạn có chắc muốn hủy thanh toán đơn hàng này không?", 
                "Xác nhận hủy thanh toán", CustomMessageBox.MessageButtons.YesNo, CustomMessageBox.MessageType.Info);
            if (result == CustomMessageBox.MessageBoxResult.Yes)
            {
                _viewModel.IsCheckedPrintBill = false;
                _viewModel.SelectedPaymentMethod = _viewModel.PaymentMethod.FirstOrDefault(p => p.Equals("Tiền mặt")) ?? "";
                this.Close();
            }
        }
        #endregion

    }
    // Converter để hiển thị số thứ tự trong DataGrid
    public class IndexToNumberConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is int index)
            {
                return (index + 1).ToString();
            }
            return "0";
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (string.IsNullOrWhiteSpace(value as string))
                return Visibility.Collapsed;

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }

}
