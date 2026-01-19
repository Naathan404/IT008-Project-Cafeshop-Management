using CoffeeShop.View.Controls;
using CoffeeShop.ViewModels.AdminVM;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using static CoffeeShop.View.Controls.CustomMessageBox;

namespace CoffeeShop.View.Admin
{
    public partial class MenuManagementPage : Page
    {
        public MenuManagementPage()
        {
            InitializeComponent();
            this.DataContext = new AdminMenuViewModel();
        }

        private void ItemsContainer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sender is ItemsControl itemsControl)
            {
                var grid = FindVisualChild<UniformGrid>(itemsControl);
                if (grid != null)
                {
                    double w = itemsControl.ActualWidth;
                    int minItemWidth = 150;

                    int columns = Math.Max(1, (int)(w / minItemWidth));
                    grid.Columns = columns;
                }
            }
        }

        private void border_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Border border)
            {
                border.Background = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#D5BCA1"));
            }
        }

        private void border_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Border border)
            {
                border.Background = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#EDE2D3"));
            }
        }

        // Event handler cho nút Edit
        private void EditItem_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is AdminMenuViewModel.MenuCoffeeItem item)
            {
                var editWindow = new ItemEditWindow(item.ItemId);
                editWindow.Owner = Window.GetWindow(this);
                editWindow.ShowDialog();
            }
        }

        // Event handler cho nút Delete
        private async void DeleteItem_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is AdminMenuViewModel.MenuCoffeeItem item)
            {
                if (DataContext is AdminMenuViewModel viewModel)
                {
                    // Set selected item để command có thể thực thi
                    viewModel.SelectedItem = item;

                    // Execute delete command
                    if (viewModel.DeleteItemCommand.CanExecute(null))
                    {
                        viewModel.DeleteItemCommand.Execute(null);
                    }
                }
            }
        }

        // Event handler cho nút Add
        private void AddNewItem_Click(object sender, MouseButtonEventArgs e)
        {
            var addWindow = new ItemEditWindow();
            addWindow.Owner = Window.GetWindow(this);
            addWindow.ShowDialog();
        }

        private T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T tChild)
                    return tChild;

                var result = FindVisualChild<T>(child);
                if (result != null)
                    return result;
            }
            return null;
        }

    }

    // Converters

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