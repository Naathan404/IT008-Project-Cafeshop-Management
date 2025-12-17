using CoffeeShop.ViewModels.StaffVM;
using MaterialDesignThemes.Wpf;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using static CoffeeShop.ViewModels.StaffVM.StaffOrderViewModel;

namespace CoffeeShop.View.Staff
{
    public partial class Staff_Order : Page
    {
        private StaffOrderViewModel _viewModel;
        public Staff_Order()
        {
            InitializeComponent();
            _viewModel = new StaffOrderViewModel();
            this.DataContext = _viewModel;
        }

        #region Helper Methods
        // Hàm tìm kiếm phần tử cha trong Visual Tree
        public static T? FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null) return null;
            if (parentObject is T parent)
                return parent;
            return FindParent<T>(parentObject);
        }
        // Hàm tìm kiếm phần tử con trong Visual Tree
        public static T? FindChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;

            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is T tChild)
                    return tChild;

                var result = FindChild<T>(child);
                if (result != null)
                    return result;
            }

            return null;
        }

        #endregion

        #region TabControl Methods
        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is TabControl tabControl && tabControl.SelectedItem is TabItem tab)
            {
                if (DataContext is StaffOrderViewModel vm)
                {
                    if (tab.Tag != null && int.TryParse(tab.Tag.ToString(), out int categoryId))
                    {
                        vm.CurrentCategoryId = categoryId;
                    }
                }
            }
        }

        #endregion

        #region Item Events
        private void ItemsContainer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sender is not ScrollViewer sv) return;

            // Tìm UniformGrid bên trong ScrollViewer
            UniformGrid? ug = FindChild<UniformGrid>(sv);
            if (ug == null) return;

            double w = sv.ActualWidth;
            int minItemWidth = 150;

            int columns = Math.Max(1, (int)(w / minItemWidth));
            ug.Columns = columns;
        }
        private void Item_MouseDown(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            var element = sender as FrameworkElement;
            var data = element?.DataContext;

            if (data == null) return;
            var thisItem = data as OrderItem;
            if (thisItem == null) return;
            bool isFoodWithoutDetailWindow = thisItem.CategoryId == 7 &&
                                             (thisItem.ItemPrices?.Count == 1 || thisItem.ItemPrices?.Count == 0);

            if (isFoodWithoutDetailWindow)
            {
                // Thêm trực tiếp Food vào Order
                decimal price = thisItem.ItemPrices?.FirstOrDefault()?.Price ?? 0;
                _viewModel.AddItemToOrder(thisItem, string.Empty, null, price);
            }
            else
            {
                var detailWindow = new ItemWindow(_viewModel, thisItem);
                detailWindow.ShowDialog();
            }
        }
        private void ItemSize_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is StackPanel stackPanel &&
                stackPanel.DataContext is OrderItem item &&
                item.ItemPrices != null)
            {
                var sizeList = item.ItemPrices
                    .Where(p => p.Size != null)
                    .Select(p => p.Size)
                    .ToList();
                if (sizeList.Count() == 0)
                    return;
                if (sizeList.Count == 1 && item.CategoryId == 7)
                    return;

                var stkSizeName = stackPanel.FindName("stpnItemSize") as StackPanel;
                if (stkSizeName == null) return;

                stkSizeName.Visibility = Visibility.Visible;

                for (int i = 0; i < sizeList.Count; i++)
                {
                    if (sizeList[i] is { SizeName: not null } sizeItem)
                    {
                        if (stackPanel.FindName($"bdrItemSize{i}") is Border bdr &&
                            bdr.Child is TextBlock txt)
                        {
                            txt.Text = sizeItem.SizeName;
                            bdr.Visibility = Visibility.Visible;
                        }
                    }
                }
            }
        }

        private void ItemPrice_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is TextBlock txtblItemPrice)
            {
                var item = txtblItemPrice.DataContext as OrderItem;
                if (item == null || item.ItemPrices == null || item.ItemPrices.Count == 0)
                    return;

                var defaultPrice = item.ItemPrices.First();
                txtblItemPrice.Text = string.Format("{0:N0} VND", defaultPrice.Price);
            }
        }
        private void bdrItemSize_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            if (sender is Border bdrSize)
            {
                var stackPanel = FindParent<StackPanel>(bdrSize);
                if (stackPanel == null) return;

                // Reset tất cả border về trạng thái ban đầu
                foreach (var bdr in stackPanel.Children.OfType<Border>())
                {
                    bdr.Background = Brushes.Transparent;
                    if (bdr.Child is TextBlock t)
                        t.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#766839"));
                }

                // Tô màu border đang click
                bdrSize.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#766839"));
                if (bdrSize.Child is TextBlock txt)
                    txt.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EDE2D3"));

                // Lấy size được chọn
                string selectedSize = (bdrSize.Child as TextBlock)?.Text.Trim() ?? "";

                // Lấy giá
                decimal price = GetPriceFromBorder(bdrSize);

                // Lấy item từ DataContext cha
                var card = FindParent<Border>(bdrSize);
                var item = card?.DataContext as OrderItem;
                if (item == null) return;

                // Thêm vào DataGrid qua ViewModel
                _viewModel.AddItemToOrder(item, selectedSize, null, price);
            }
        }
        private decimal GetPriceFromBorder(Border bdrItemSize)
        {
            if (bdrItemSize?.Child is not TextBlock txtSize)
                return 0;

            string sizeName = txtSize.Text;
            if (string.IsNullOrWhiteSpace(sizeName))
                return 0;

            var stackPanel = FindParent<StackPanel>(bdrItemSize);
            if (stackPanel?.DataContext is not OrderItem item)
                return 0;

            if (item.ItemPrices == null || item.ItemPrices.Count == 0)
                return 0;
            var selectedPrice = item.ItemPrices
                .Where(p => p?.Size?.SizeName != null)
                .FirstOrDefault(p => p!.Size!.SizeName == sizeName)
                ?.Price ?? 0;

            return selectedPrice;
        }

        #endregion

        #region Search Item
        private void txblSearchItem_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                _viewModel.SearchItemKeyword = tb.Text;
            }
        }
        
        #endregion

        #region DataGrid Events
        private void DeleteOrderItem_MouseDown(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is OrderDetailItem item)
            {
                _viewModel.RemoveItemCommand?.Execute(item);
            }
        }
        private void icPlusQuantity_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border bdr && bdr.DataContext is OrderDetailItem item)
            {
                _viewModel.IncreaseQuantityCommand?.Execute(item);
            }
        }

        private void icMinusQuantity_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border bdr && bdr.DataContext is OrderDetailItem item)
            {
                _viewModel.DecreaseQuantityCommand?.Execute(item);
            }
        }
        private void tbItemOrderedQuantity_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (sender is TextBox tb)
                {
                    tb.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();

                    if (tb.DataContext is OrderDetailItem item)
                    {
                        if (item.Quantity <= 0)
                        {
                            _viewModel.RemoveItemCommand?.Execute(item);
                        }
                    }
                    Keyboard.ClearFocus();
                }
            }
        }
        private void tbItemOrderedNote_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && sender is TextBox tb && tb.DataContext is OrderDetailItem item)
            {
                item.Note = tb.Text.Trim();
            }
        }
        private void tbNote_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && textBox.DataContext is OrderDetailItem modifiedItem)
            {
                modifiedItem.NoteChangedCallback?.Invoke(modifiedItem);
            }
        }
        private static bool IsTextNumeric(string text)
        {
            return int.TryParse(text, out _);
        }
        private void tbItemOrderedQuantity_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextNumeric(e.Text);
        }
        #endregion

        #region Customer Events
        private void icSearchCustomer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is PackIcon ic)
            {
                _viewModel.SearchCustomerCommand?.Execute(tbCustomerPhone.Text);
            }
        }
        private void icAddCustomer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            AddCustomerWindow addCustomer;
            if (string.IsNullOrEmpty(tbCustomerPhone.Text))
                addCustomer = new AddCustomerWindow(_viewModel);
            else
                addCustomer = new AddCustomerWindow(_viewModel, tbCustomerPhone.Text);

            addCustomer.Show();
        }
        private void CustomerItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is OrderCustomer customer)
            {
                _viewModel.SelectedCustomer = customer;
                tbCustomerPhone.Text = customer.PhoneNumber;
                popupCustomers.IsOpen = false;
            }
        }
        // Hiển thị popup khi có thay đổi text
        private void tbCustomerPhone_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                _viewModel.SearchCustomerKeyword = tb.Text;
                popupCustomers.IsOpen = _viewModel.HasSearchResults;
            }
        }
        #endregion

        #region Button Events
        private void bdrItemSize_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Border border && !(border.Tag as bool? ?? false))
            {
                border.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D4BA98"));
                var txtb = border.Child as TextBlock;
                if (txtb != null)
                    txtb.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#340D05"));
            }
        }

        private void bdrItemSize_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Border border && !(border.Tag as bool? ?? false))
            {
                border.Background = Brushes.Transparent;
                var txtb = border.Child as TextBlock;
                if (txtb != null)
                    txtb.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#766839"));
            }
        }
        private void icDeleteItem_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is PackIcon icon)
                icon.Kind = PackIconKind.Delete;
        }

        private void icDeleteItem_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is PackIcon icon)
                icon.Kind = PackIconKind.DeleteOutline;
        }

        private void bdrPlusQuantity_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Border border)
            {
                var icon = border.Child as PackIcon;
                if (icon != null)
                    icon.Kind = PackIconKind.PlusCircle;
            }
        }

        private void bdrPlusQuantity_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Border border && !(border.Tag as bool? ?? false))
            {
                var icon = border.Child as PackIcon;
                if (icon != null)
                    icon.Kind = PackIconKind.PlusCircleOutline;
            }
        }

        private void bdrMinusQuantity_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Border border)
            {
                var icon = border.Child as PackIcon;
                if (icon != null)
                    icon.Kind = PackIconKind.MinusCircle;
            }
        }

        private void bdrMinusQuantity_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Border border && !(border.Tag as bool? ?? false))
            {
                var icon = border.Child as PackIcon;
                if (icon != null)
                    icon.Kind = PackIconKind.MinusCircleOutline;
            }
        }
        private void icAddCustomer_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Border bdr && bdr.Child is PackIcon icon)
            {
                icon.Kind = PackIconKind.PlusCircleOutline;
            }
        }

        private void icAddCustomer_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Border bdr && bdr.Child is PackIcon icon)
            {
                icon.Kind = PackIconKind.PlusCircle;
            }
        }
        private void CustomerItem_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Border border)
            {
                border.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D4BA98"));
            }
        }

        private void CustomerItem_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Border border)
            {
                border.Background = Brushes.Transparent;
            }
        }
        private void icSearchCustomer_MouseEnter(object sender, MouseEventArgs e)
        {
            if(sender is PackIcon icon)
            {
                icon.Kind = PackIconKind.AccountSearch;
            }
        }

        private void icSearchCustomer_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is PackIcon icon)
            {
                icon.Kind = PackIconKind.AccountSearchOutline;
            }
        }
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
    // Converter AND hai biến boolean
    public class AndBooleanConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values.All(v => v is bool b && b);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
