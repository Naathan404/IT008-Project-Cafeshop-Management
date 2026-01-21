using CoffeeShop.ViewModels.StaffVM;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using static CoffeeShop.ViewModels.StaffVM.StaffMenuViewModel;

namespace CoffeeShop.View.Staff
{
    public partial class Staff_Menu : Page
    {
        StaffMenuViewModel _viewModel = new StaffMenuViewModel();

        public Staff_Menu()
        {
            InitializeComponent();
            this.DataContext = _viewModel;
        }
        #region Item Events
        private void ItemsContainer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sender is ItemsControl itemsControl)
            {
                var grid = FindChild<UniformGrid>(itemsControl);
                if (grid != null)
                {
                    double w = itemsControl.ActualWidth;
                    int minItemWidth = 150;

                    int columns = Math.Max(1, (int)(w / minItemWidth));
                    grid.Columns = columns;
                }
            }
        }

        private void Item_MouseDown(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            var element = sender as FrameworkElement;
            var data = element?.DataContext;

            if (data == null) return;
            var thisItem = data as MenuCoffeeItem;
            if (thisItem == null) return;
            _viewModel.SelectedItem = thisItem;
        }
        #endregion

        #region TabControl Methods
        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is TabControl tabControl && tabControl.SelectedItem is TabItem tab)
            {
                if (DataContext is StaffMenuViewModel vm)
                {
                    if (tab.Tag != null && int.TryParse(tab.Tag.ToString(), out int categoryId))
                    {
                        vm.CurrentCategoryId = categoryId;
                    }
                }
            }
        }

        #endregion

        #region Search item methods
        private void txblSearchItem_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                _viewModel.SearchItemKeyword = tb.Text;
            }
        }
        #endregion

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
                    Color = (Color)ColorConverter.ConvertFromString("#766839"), // Màu bóng tối
                    Direction = 315, // Góc đổ bóng
                    ShadowDepth = 4, // Độ sâu/khoảng cách của bóng
                    BlurRadius = 10, // Độ mờ của bóng
                    Opacity = 0.6 // Độ trong suốt của bóng
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
        #endregion

        #region Helper methods
        public static T? FindChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null)
                return null;
            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (child is T foundChild)
                    return foundChild;
                T? result = FindChild<T>(child);
                if (result != null)
                    return result;
            }
            return null;
        }
        #endregion
    }
    public class ObjectEqualsConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2)
                return false;

            return ReferenceEquals(values[0], values[1]);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
    // Converter để chuyển Count thành Visibility
    public class CountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count)
            {
                return count > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Converter để kiểm tra Count > 1
    public class GreaterThanOneConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count)
            {
                return count > 1;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}