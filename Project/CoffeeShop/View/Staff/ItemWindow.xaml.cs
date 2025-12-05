using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static CoffeeShop.View.Staff.Staff_Order;

namespace CoffeeShop.View.Staff
{
    public partial class ItemWindow : Window
    {
        public event Action<OrderItem, string, string, decimal> OnAddItem; // Event để trả dữ liệu về Page Order
        public ItemWindow(OrderItem item)
        {
            InitializeComponent();
            this.DataContext = item; // Gán DataContext cho cả Window
        }

        #region Item Events
        private void ItemPrice_Changed(TextBlock txbl, decimal price)
        {
            if (txbl == null)
                return;

            txbl.Text = string.Format("{0:N0} VND", price);
        }
        private decimal GetPriceFromBorder(Border bdrItemSize) // Lấy giá theo size được click
        {
            if (bdrItemSize == null) return 0;

            // Lấy tên size từ TextBlock bên trong
            if (!(bdrItemSize.Child is TextBlock txtSize)) return 0;
            string sizeName = txtSize.Text;

            // Lấy item từ DataContext của StackPanel cha
            var stackPanel = FindParent<StackPanel>(bdrItemSize);
            if (stackPanel == null) return 0;

            var item = stackPanel.DataContext as OrderItem;
            if (item == null || item.ItemPrices == null) return 0;

            // Tìm price tương ứng size
            var selectedPrice = item.ItemPrices.FirstOrDefault(p => p.Size.SizeName == sizeName)?.Price ?? 0;

            return selectedPrice;
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
                    var stkSizeName = stackPanel.FindName("stpnItemSize") as StackPanel;
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

        private void ItemPrice_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is TextBlock txtblItemPrice)
            {
                var stackPanel = FindParent<StackPanel>(txtblItemPrice);
                if (stackPanel == null) return;
                var item = stackPanel.DataContext as OrderItem;
                if (item == null || item.ItemPrices == null) return;

                // Hiển thị giá mặc định (giá size đầu tiên)
                var defaultPrice = item.ItemPrices.FirstOrDefault();
                if (defaultPrice != null)
                    ItemPrice_Changed(txtblItemPrice, defaultPrice.Price);
            }
        }

        private void bdrItemSize_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Border border)
            {
                // Nếu border đang selected thì KHÔNG đổi màu khi hover
                if (border.Tag is bool isSelected && isSelected == true)
                    return;
                border.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#D4BA98"));
                var txtb = border.Child as TextBlock;
                txtb.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#340D05"));
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
                var txtb = border.Child as TextBlock;
                txtb.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#766839"));
            }
        }
        private void bdrItemSize_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true; // Ngăn event bubble lên parent

            if (sender is Border bdrSize)
            {
                // Cập nhật giá
                decimal price = GetPriceFromBorder(bdrSize);
                txblItemPrice.Text = string.Format("{0:N0} VND", price);

                // Tìm StackPanel chứa các size
                var stkSizePanel = FindParent<StackPanel>(bdrSize);
                if (stkSizePanel == null) return;

                // Bỏ chọn tất cả size khác
                foreach (var b in stkSizePanel.Children.OfType<Border>())
                {
                    b.Tag = false;
                    b.Background = Brushes.Transparent;
                    if (b.Child is TextBlock t)
                        t.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#766839"));
                }

                // Chọn Border được click
                bdrSize.Tag = true;
                bdrSize.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#766839"));
                if (bdrSize.Child is TextBlock txt)
                    txt.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#EDE2D3"));
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
                border.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#D4BA98"));
                var txtb = border.Child as TextBlock;
                txtb.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#340D05"));
            }
        }

        private void bdrExit_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Border border)
            {
                border.Background = Brushes.Transparent; // trả về nền mặc định
                var txtb = border.Child as TextBlock;
                txtb.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#766839"));
            }
        }

        private void bdrAddToOrder_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Border border)
            {
                border.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#D4BA98"));
                var txtb = border.Child as TextBlock;
                txtb.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#340D05"));
            }
        }

        private void bdrAddToOrder_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Border border)
            {
                border.Background = Brushes.Transparent; // trả về nền mặc định
                var txtb = border.Child as TextBlock;
                txtb.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#766839"));
            }
        }

        private void bdrAddToOrder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Lấy item hiện tại
            var item = this.DataContext as OrderItem;
            if (item == null) return;

            // Lấy size được chọn
            var sizeBorder = stpnItemSize.Children
                .OfType<Border>()
                .FirstOrDefault(b => (bool)(b.Tag ?? false));

            string selectedSize = sizeBorder != null? (sizeBorder.Child as TextBlock)?.Text: item.ItemPrices.First().Size.SizeName; // nếu không chọn thì lấy size đầu
            string note = txblItemNote.Text;

            // Lấy giá
            decimal price = 0;
            if (sizeBorder != null)
                price = GetPriceFromBorder(sizeBorder);
            else
                price = item.ItemPrices.First().Price;

            // Gọi event trả dữ liệu về Page
            OnAddItem?.Invoke(item, selectedSize, note, price);

            this.Close();
        }
        #endregion
        
    }
}
