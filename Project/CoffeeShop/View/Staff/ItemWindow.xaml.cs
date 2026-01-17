using CoffeeShop.ViewModels.StaffVM;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static CoffeeShop.ViewModels.StaffVM.StaffOrderViewModel;

namespace CoffeeShop.View.Staff
{
    public partial class ItemWindow : Window
    {
        private readonly StaffOrderViewModel _viewModel;
        private OrderItem _currentItem;
        private string? _selectedSize;
        private decimal _selectedPrice;
        private int _selectedPriceID;
        public ItemWindow(StaffOrderViewModel viewModel, OrderItem item)
        {
            InitializeComponent();
            _viewModel = viewModel;
            _currentItem = item;
            this.DataContext = item;

            // Tự động chọn size đầu tiên nếu có
            if (item.ItemPrices != null && item.ItemPrices.Count > 0)
            {
                var firstSize = item.ItemPrices.First();

                if (_currentItem.CategoryId == 7)
                {
                    _selectedSize = null; // food không có size
                    _selectedPrice = firstSize.Price;
                    _selectedPriceID = firstSize.PriceId;
                }
                else if (firstSize.Size != null)
                {
                    _selectedSize = firstSize.Size.SizeName;
                    _selectedPrice = firstSize.Price;
                    _selectedPriceID = firstSize.PriceId;
                    bdrItemSize0.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D4BA98"));
                    txtSize0.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#340D05"));
                }
            }

        }

        #region Item Events
        private void ItemPrice_Changed(decimal price)
        {
            // Cập nhật giá bằng FindName
            var priceTextBlock = this.FindName("txblItemPrice") as TextBlock;
            if (priceTextBlock != null)
            {
                priceTextBlock.Text = string.Format("{0:N0} VND", _selectedPrice);
            }
        }
        private void ItemSize_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is StackPanel stackPanel)
            {
                var item = stackPanel.DataContext as OrderItem;
                if (item == null || item.ItemPrices == null) return;

                //Load size button tương ứng với item
                var sizeList = item.ItemPrices.Select(p => p.Size).ToList(); // list size cua item
                if (sizeList.Count() == 0)
                    return;
                if (sizeList.Count == 1 && item.CategoryId == 7) // categoryId == 7 --> food không có size
                    return;

                int numOfSize = sizeList.Count;
                if (numOfSize > 0)
                {
                    if (stackPanel.FindName("stpnItemSize") is StackPanel stkSizeName)
                        stkSizeName.Visibility = Visibility.Visible;
                    for (int i = 0; i < numOfSize; i++)
                    {
                        string bdrName = "bdrItemSize" + i;

                        if (stackPanel.FindName($"bdrItemSize{i}") is Border bdr && bdr.Child is TextBlock txt &&
                                sizeList != null && i < sizeList.Count && sizeList[i] is { } sizeItem)
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

                // Hiển thị giá mặc định (size đầu tiên)
                var defaultPrice = item.ItemPrices.First();
                txtblItemPrice.Text = string.Format("{0:N0} VND", defaultPrice.Price);
            }
        }

        private void bdrItemSize_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Border border)
            {
                // Nếu border đang selected thì KHÔNG đổi màu khi hover
                if (border.Tag is bool isSelected && isSelected == true)
                    return;
                border.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D4BA98"));
                if (border.Child is TextBlock txtb)
                {
                    txtb.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#340D05"));
                }
            }
        }

        private void bdrItemSize_MouseLeave(object sender, MouseEventArgs e) //Trả lại màu ban đầu khi không trò con chuột vào
        {
            if (sender is Border border)
            {
                // Nếu border đang selected thì KHÔNG reset màu
                if (border.Tag is bool isSelected && isSelected == true)
                    return;
                border.Background = Brushes.Transparent; // trả về nền mặc định
                if (border.Child is TextBlock txtb)
                {
                    txtb.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#766839"));
                }
            }
        }
        private void bdrItemSize_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border bdrSize)
            {
                var stackPanel = FindParent<StackPanel>(bdrSize);
                if (stackPanel == null) return;

                // Reset tất cả border
                foreach (var bdr in stackPanel.Children.OfType<Border>())
                {
                    bdr.Tag = false;
                    bdr.Background = Brushes.Transparent;
                    if (bdr.Child is TextBlock t)
                        t.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#766839"));
                }

                // Tô màu border đang click
                bdrSize.Tag = true;
                bdrSize.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#766839"));
                if (bdrSize.Child is TextBlock txt)
                {
                    txt.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EDE2D3"));
                    _selectedSize = txt.Text.Trim();
                }
                // Lấy giá
                _selectedPrice = GetPriceFromBorder(bdrSize);
                _selectedPriceID = _currentItem.ItemPrices
                    .First(p => p is { Size: { SizeName: not null } s } && s.SizeName == _selectedSize).PriceId;

                // Cập nhật hiển thị giá
                ItemPrice_Changed(_selectedPrice);
            }
        }
        #endregion

        #region Button Events
        private void bdrExit_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void bdrExit_MouseEnter(object sender, MouseEventArgs e)
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

        private void bdrExit_MouseLeave(object sender, MouseEventArgs e)
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

        private void bdrAddToOrder_MouseEnter(object sender, MouseEventArgs e)
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

        private void bdrAddToOrder_MouseLeave(object sender, MouseEventArgs e)
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
        private void bdrAddToOrder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Lấy note từ TextBox (nếu có)
            string? note = txblItemNote.Text;
            if (string.IsNullOrEmpty(note))
                note = null;

            // Kiểm tra đã chọn size chưa
            if (string.IsNullOrEmpty(_selectedSize) && _currentItem.CategoryId != 7)
            {
                MessageBox.Show("Vui lòng chọn size!", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Thêm vào order thông qua ViewModel
            _viewModel.AddItemToOrder(_currentItem, _selectedSize, note, _selectedPrice, _selectedPriceID);

            // Đóng window
            this.Close();
        }
        #endregion

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
        private decimal GetPriceFromBorder(Border bdrItemSize)
        {
            if (bdrItemSize?.Child is not TextBlock txtSize)
                return 0;

            string sizeName = txtSize.Text;

            var stackPanel = FindParent<StackPanel>(bdrItemSize);
            if (stackPanel?.DataContext is not OrderItem item)
                return 0;

            if (item.ItemPrices == null)
                return 0;
            var selected = item.ItemPrices.FirstOrDefault(p => p is { Size: { SizeName: not null } s } && s.SizeName == sizeName);

            return selected?.Price ?? 0;
        }

        #endregion
    }
}
