using CoffeeShop.ViewModels.AdminVM;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CoffeeShop.View.Admin
{
    public partial class ItemEditWindow : Window
    {
        public ItemEditWindow(int? itemId = null)
        {
            InitializeComponent();

            // Khởi tạo ViewModel với itemId (có thể null)
            DataContext = new ItemEditViewModel(itemId);

            SubscribeToViewModel();
        }

        private void SubscribeToViewModel()
        {
            if (DataContext is ItemEditViewModel vm)
            {
                vm.PropertyChanged += ViewModel_PropertyChanged;
            }
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ItemEditViewModel.DialogResult))
            {
                if (DataContext is ItemEditViewModel vm && vm.DialogResult.HasValue)
                {
                    this.DialogResult = vm.DialogResult;
                    this.Close();
                }
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            // Unsubscribe để tránh memory leak
            if (DataContext is ItemEditViewModel vm)
            {
                vm.PropertyChanged -= ViewModel_PropertyChanged;
            }
            base.OnClosing(e);
        }
    }
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Nếu value là null thì Ẩn (Collapsed), ngược lại thì Hiện (Visible)
            return value == null ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}