using CoffeeShop.ViewModels.StaffVM;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace CoffeeShop.View.Staff
{
    /// <summary>
    /// Interaction logic for Staff_Table.xaml
    /// </summary>
    public partial class Staff_Table : Page
    {
        StaffTableViewModel _viewModel;
        public Staff_Table()
        {
            InitializeComponent();
            _viewModel = new StaffTableViewModel();
            this.DataContext = _viewModel;
        }
    }
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Nếu value là null thì hiện (Visible), ngược lại thì ẩn (Collapsed)
            // Dùng cho dòng "Vui lòng chọn bàn..."
            return value == null ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class NotNullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Nếu có dữ liệu thì hiện, null thì ẩn
            // Dùng cho bảng chi tiết bàn
            return value != null ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class IntToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue && parameter != null)
            {
                // Lấy giá trị so sánh từ ConverterParameter trong XAML
                int targetValue = int.Parse(parameter.ToString());

                // Nếu status trùng với parameter thì hiện, ngược lại thì ẩn
                return intValue == targetValue ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
