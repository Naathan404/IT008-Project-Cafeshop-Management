using CoffeeShop.ViewModels.AdminVM;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace CoffeeShop.View.Admin
{
    public partial class MenuManagementPage : Page
    {
        public MenuManagementPage()
        {
            InitializeComponent();
            DataContext = new AdminMenuViewModel();
        }

        private void ItemsContainer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sender is ScrollViewer scrollViewer)
            {
                var itemsControl = scrollViewer.Content as ItemsControl;
                if (itemsControl?.ItemsPanel != null)
                {
                    var panel = itemsControl.ItemsPanel.LoadContent() as UniformGrid;
                    if (panel != null)
                    {
                        double width = scrollViewer.ActualWidth - 20;
                        int columns = (int)(width / 210);
                        panel.Columns = columns > 0 ? columns : 1;
                    }
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