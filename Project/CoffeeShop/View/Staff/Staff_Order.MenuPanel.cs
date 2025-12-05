using CoffeeShop.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace CoffeeShop.View.Staff
{
    public partial class Staff_Order : Page
    {
        #region class OrderItem
        // class để lưu dữ liệu trong MenuPanel
        public class OrderItem : NotificationBase
        {
            // Backing field (nơi lưu giá trị thật sự) 
            private int _itemId;
            private string _itemName;
            private int _categoryId;
            private int _quantity;
            private bool _isAvailable;
            private ObservableCollection<ItemPrice> _itemPrices;
            private string _imagePath;

            public int ItemId
            {
                get => _itemId;
                set
                {
                    _itemId = value;
                    OnPropertyChanged();
                }
            }

            public string ItemName
            {
                get => _itemName;
                set
                {
                    _itemName = value;
                    OnPropertyChanged();
                }
            }

            public int CategoryId
            {
                get => _categoryId;
                set
                {
                    _categoryId = value;
                    OnPropertyChanged();
                }
            }

            public int Quantity
            {
                get => _quantity;
                set
                {
                    _quantity = value;
                    OnPropertyChanged();
                }
            }

            public bool IsAvailable
            {
                get => _isAvailable;
                set
                {
                    _isAvailable = value;
                    OnPropertyChanged();
                }
            }

            public ObservableCollection<ItemPrice> ItemPrices
            {
                get => _itemPrices;
                set
                {
                    _itemPrices = value;
                    OnPropertyChanged();
                }
            }

            public string ImagePath
            {
                get => _imagePath;
                set
                {
                    _imagePath = value;
                    OnPropertyChanged();
                }
            }

            // Constructor để khởi tạo giá trị mặc định
            public OrderItem()
            {
                _itemPrices = new ObservableCollection<ItemPrice>();
                _imagePath = "/Assets/Images/imgItemExample.jpg";
            }
        }
        #endregion

        #region tabItem Loading
        //Load items vào từng tabItem theo categoryId
        private void LoadItem(ObservableCollection<OrderItem> itemsList)
        {
            if (itemsList == null || itemsList.Count == 0)
                return;
            for (int i = 1; i <= itemsList.Max(x => x.CategoryId); i++)
            {
                // Lấy tên TabItem và UniformGrid
                var ic = tabMain.FindName("icCategory" + i) as ItemsControl;
                if (ic != null)
                {
                    ic.ItemsSource = null; // Xóa dữ liệu cũ trước khi nạp dữ liệu mới
                    ic.ItemsSource = new ObservableCollection<OrderItem>(itemsList.Where(x => x.CategoryId == i));
                }
            }
        }

        //Load dữ liệu từ DB vào MenuPanel
        private void LoadOrderItem()
        {
            _items.Clear();
            using (var context = new CoffeeShopContext())
            {
                var items = context.Items.ToList();
                foreach(var item in items)
                {
                    _items.Add(new OrderItem
                    {
                        ItemId = item.ItemId,
                        ItemName = item.ItemName,
                        CategoryId = item.CategoryId,
                        IsAvailable = item.IsAvailable,
                        ItemPrices = new ObservableCollection<ItemPrice>(context.ItemPrices
                                            .Include(ip => ip.Size)
                                            .Where(ip => ip.ItemId == item.ItemId)
                                            .ToList()),
                    });
                }
                LoadItem(_items);
            }
        }

        #endregion

        #region ItemCard Events
        //private void ImgItem_Loaded(object sender, RoutedEventArgs e)
        //{
        //    var img = sender as Image;
        //    if (img == null) return;
        //    img.SizeChanged += (s, e) =>
        //    {
        //        img.Clip = new RectangleGeometry()
        //        {
        //            Rect = new Rect(0, 0, img.ActualWidth, img.ActualHeight),
        //            RadiusX = 15,
        //            RadiusY = 15
        //        };
        //    };
        //}
        private void DisplayPrice(string txtblName, Border bdrItem, decimal price) // Hiển thị giá lên TextBlock
        {
            var txtbPrice = bdrItem.FindName(txtblName) as TextBlock;
            if (txtbPrice != null)
                txtbPrice.Text = String.Format("{0:N0} VND", price);
        }
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
                var item = txtblItemPrice.DataContext as OrderItem;
                if (item == null || item.ItemPrices == null || item.ItemPrices.Count == 0)
                    return;

                // Hiển thị giá mặc định (size đầu tiên)
                var defaultPrice = item.ItemPrices.First();
                txtblItemPrice.Text = string.Format("{0:N0} VND", defaultPrice.Price);
            }
        }
        private void Item_MouseDown(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            var element = sender as FrameworkElement;
            var data = element.DataContext;

            if (data == null)
                return;
            var thisitem = data as OrderItem;
            var detailWindow = new ItemWindow(thisitem);
            string note = null; // Ghi chú mặc định là null
            detailWindow.OnAddItem += (itemSelected, size, note, price) =>
            {
                AddItemToDataGrid(itemSelected, size, note, price);
            };
            detailWindow.ShowDialog();
        }

        private void ItemsContainer_SizeChanged(object sender, SizeChangedEventArgs e) //Căn chỉnh items
        {
            if (sender is not ScrollViewer sv) return;
            if (sv.Content is not UniformGrid ug) return;

            double w = sv.ActualWidth;
            if (w <= 50) return;

            int minItemWidth = 150;
            int columns = Math.Max(1, (int)(w / minItemWidth));
            ug.Columns = columns;
        }

        private void bdrItemSize_MouseEnter(object sender, MouseEventArgs e) // Đổi màu background khi rê chuột vào
        {
            if (sender is Border border && !(border.Tag as bool? ?? false))
            {
                border.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#D4BA98"));
                var txtb = border.Child as TextBlock;
                txtb.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#340D05"));
            }
        }

        private void bdrItemSize_MouseLeave(object sender, MouseEventArgs e) //Trả lại màu ban đầu khi không trò con chuột vào
        {
            if (sender is Border border && !(border.Tag as bool? ?? false))
            {
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
                var stackPanel = FindParent<StackPanel>(bdrSize);
                if (stackPanel == null) return;

                foreach (var bdr in stackPanel.Children.OfType<Border>()) // Reset tất cả border về trạng thái ban đầu
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

                // Thêm vào DataGrid
                AddItemToDataGrid(item, selectedSize, null, price); // Mặc định note là null khi click size
            }
        }


        // Tìm kiếm item với từ khóa
        private void SearchItems(string keyword)
        {
            var result = _items.Where(i => i.ItemName.Contains(keyword, StringComparison.OrdinalIgnoreCase));
            LoadItem(new ObservableCollection<OrderItem>(result));
        }

        private void txblSearchItem_TextChanged(object sender, TextChangedEventArgs e)
        {
            SearchItems(txblSearchItem.Text.Trim());
        }

        #endregion
    }
}
