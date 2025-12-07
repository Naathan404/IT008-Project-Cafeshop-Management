using CoffeeShop.ViewModels.StaffVM;
using MaterialDesignThemes.Wpf;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using static CoffeeShop.ViewModels.StaffVM.StaffOrderViewModel;

namespace CoffeeShop.View.Staff
{
    public partial class Staff_Order : Page
    {
        private ICollectionView _itemsView;
        private StaffOrderViewModel _viewModel;
        public Staff_Order()
        {
            InitializeComponent();
            _viewModel = new StaffOrderViewModel();
            this.DataContext = _viewModel;
            LoadTablesToComboBox();
        }
        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_viewModel.Items))
            {
                LoadItemsByCategory();
            }
        }

        #region Page Events
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadItemsByCategory();
        }
        #endregion

        #region Helper Methods
        // Hàm tìm kiếm phần tử cha trong Visual Tree
        public static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null) return null;
            if (parentObject is T parent)
                return parent;
            return FindParent<T>(parentObject);
        }
        #endregion

        #region TabItem Loading
        private void LoadItemsByCategory()
        {
            if (_viewModel == null || _viewModel.Items.Count == 0)
                return;
            for (int i = 1; i <= _viewModel.Items.Max(x => x.CategoryId); i++)
            {
                var ic = tabMain.FindName($"icCategory{i}") as ItemsControl;
                if (ic != null)
                {
                    ic.ItemsSource = null;
                    ic.ItemsSource = _viewModel.Items.Where(x => x.CategoryId == i).ToList();
                }
            }
        }
        #endregion

        #region Item Events
        private void ItemsContainer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sender is not ScrollViewer sv) return;
            if (sv.Content is not UniformGrid ug) return;

            double w = sv.ActualWidth;
            if (w <= 50) return;

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

            var detailWindow = new ItemWindow(_viewModel, thisItem);
            detailWindow.ShowDialog();
        }
        private void ItemSize_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is StackPanel stackPanel)
            {
                var item = stackPanel.DataContext as OrderItem;
                if (item == null || item.ItemPrices == null) return;

                var sizeList = item.ItemPrices.Select(p => p.Size).ToList();
                if (sizeList.Count() == 0)
                    return;
                if (sizeList.Count == 1 && item.CategoryId == 7)
                    return;

                int numOfSize = sizeList.Count;
                if (numOfSize > 0)
                {
                    var stkSizeName = stackPanel.FindName("stpnItemSize") as StackPanel;
                    if (stkSizeName != null)
                    {
                        stkSizeName.Visibility = Visibility.Visible;
                        for (int i = 0; i < numOfSize; i++)
                        {
                            string bdrName = "bdrItemSize" + i.ToString();
                            var bdr = stackPanel.FindName(bdrName) as Border;
                            if (bdr != null)
                            {
                                var textBlock = bdr.Child as TextBlock;
                                if (textBlock != null)
                                {
                                    textBlock.Text = sizeList[i].SizeName;
                                    bdr.Visibility = Visibility.Visible;
                                }
                            }
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
        private void bdrItemSize_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Border border && !(border.Tag as bool? ?? false))
            {
                border.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#D4BA98"));
                var txtb = border.Child as TextBlock;
                if (txtb != null)
                    txtb.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#340D05"));
            }
        }

        private void bdrItemSize_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Border border && !(border.Tag as bool? ?? false))
            {
                border.Background = Brushes.Transparent;
                var txtb = border.Child as TextBlock;
                if (txtb != null)
                    txtb.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#766839"));
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
                        t.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#766839");
                }

                // Tô màu border đang click
                bdrSize.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#766839");
                if (bdrSize.Child is TextBlock txt)
                    txt.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#EDE2D3");

                // Lấy size được chọn
                string selectedSize = (bdrSize.Child as TextBlock)?.Text.Trim();

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
            if (bdrItemSize == null) return 0;

            if (!(bdrItemSize.Child is TextBlock txtSize)) return 0;
            string sizeName = txtSize.Text;

            var stackPanel = FindParent<StackPanel>(bdrItemSize);
            if (stackPanel == null) return 0;

            var item = stackPanel.DataContext as OrderItem;
            if (item == null || item.ItemPrices == null) return 0;

            var selectedPrice = item.ItemPrices.FirstOrDefault(p => p.Size.SizeName == sizeName)?.Price ?? 0;

            return selectedPrice;
        }
        #endregion

        #region Search Item
        private void txblSearchItem_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                _viewModel.SearchItemKeyword = tb.Text;
                LoadItemsByCategory();
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
        private void icSearchCustomer_MouseEnter(object sender, MouseEventArgs e)
        {
            var icon = sender as PackIcon;
            icon.Kind = PackIconKind.AccountSearch;
        }

        private void icSearchCustomer_MouseLeave(object sender, MouseEventArgs e)
        {
            var icon = sender as PackIcon;
            icon.Kind = PackIconKind.AccountSearchOutline;
        }
        //private void tbCustomerPhone_LostFocus(object sender, RoutedEventArgs e)
        //{
        //    // Delay để cho phép click vào popup item
        //    System.Threading.Tasks.Task.Delay(200).ContinueWith(_ =>
        //    {
        //        Dispatcher.Invoke(() => popupCustomers.IsOpen = false);
        //    });
        //}
        private void icAddCustomer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _viewModel.AddCustomerCommand?.Execute(null);
        }
        private void icAddCustomer_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Border bdr)
            {
                var icon = bdr.Child as PackIcon;
                icon.Kind = PackIconKind.PlusCircleOutline;
            }
        }

        private void icAddCustomer_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Border bdr)
            {
                var icon = bdr.Child as PackIcon;
                icon.Kind = PackIconKind.PlusCircle;
            }
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

        private void CustomerItem_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Border border)
            {
                border.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#D4BA98");
            }
        }

        private void CustomerItem_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Border border)
            {
                border.Background = Brushes.Transparent;
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

        #region Table Events
        private void LoadTablesToComboBox()
        {
            // Thêm các bàn trống vào ComboBox
            cbTable.Items.Clear();
            cbTable.Items.Add(new ComboBoxItem
            {
                Content = "Không",
                Tag = null,
                IsSelected = true
            });
            foreach (var table in _viewModel.AvailableTables)
            {
                ComboBoxItem comboBoxItem = new ComboBoxItem
                {
                    Content = table.TableName,
                    Tag = table.TableId
                };
                cbTable.Items.Add(comboBoxItem);
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
}
