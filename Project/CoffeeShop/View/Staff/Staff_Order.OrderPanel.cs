using CoffeeShop.Models;
using MaterialDesignThemes.Wpf;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace CoffeeShop.View.Staff
{
    public partial class Staff_Order : Page
    {
        #region Class Customer, Table and OrderDetail
        // class để lưu dữ liệu khách hàng
        public partial class OrderCustomer : NotificationBase
        {
            // Backing fields
            private int _customerId;
            private string _customerName = string.Empty;
            private string? _phoneNumber;
            private string? _email;
            private int _point;
            private string? _tier;
            private ObservableCollection<Order> _orders;

            public int CustomerId { get; set; }
            public string CustomerName
            {
                get => _customerName;
                set
                {
                    if (_customerName != value)
                    {
                        _customerName = value;
                        OnPropertyChanged();
                    }
                }
            }

            public string? PhoneNumber
            {
                get => _phoneNumber;
                set
                {
                    if (_phoneNumber != value)
                    {
                        _phoneNumber = value;
                        OnPropertyChanged();
                    }
                }
            }

            public string? Email
            {
                get => _email;
                set
                {
                    if (_email != value)
                    {
                        _email = value;
                        OnPropertyChanged();
                    }
                }
            }

            public int Point
            {
                get => _point;
                set
                {
                    if (_point != value)
                    {
                        _point = value;
                        OnPropertyChanged();
                    }
                }
            }

            public string? Tier
            {
                get => _tier;
                set
                {
                    if (_tier != value)
                    {
                        _tier = value;
                        OnPropertyChanged();
                    }
                }
            }

            public ObservableCollection<Order> Orders
            {
                get => _orders;
                set
                {
                    if (_orders != value)
                    {
                        _orders = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        // class để lưu dữ liệu bàn
        public partial class OrderTable : NotificationBase
        {
            // Backing fields
            private int _tableId;
            private string _tableName = string.Empty;
            private int _tableStatus;
            private string? _note;
            private ObservableCollection<Order> _orders;

            public int TableId
            {
                get => _tableId;
                set
                {
                    if (_tableId != value)
                    {
                        _tableId = value;
                        OnPropertyChanged();
                    }
                }
            }

            public string TableName
            {
                get => _tableName;
                set
                {
                    if (_tableName != value)
                    {
                        _tableName = value ?? string.Empty;
                        OnPropertyChanged();
                    }
                }
            }

            public int TableStatus
            {
                get => _tableStatus;
                set
                {
                    if (_tableStatus != value)
                    {
                        _tableStatus = value;
                        OnPropertyChanged();
                    }
                }
            }

            public string? Note
            {
                get => _note;
                set
                {
                    if (_note != value)
                    {
                        _note = value;
                        OnPropertyChanged();
                    }
                }
            }

            public ObservableCollection<Order> Orders
            {
                get => _orders;
                set
                {
                    if (_orders != value)
                    {
                        _orders = value;
                        OnPropertyChanged();
                    }
                }
            }
        }
        // class để lưu dữ liệu hiển thị trong DataGrid
        public class OrderDetailDisplay : NotificationBase
        {
            // Backing fields
            private int _itemId;
            private string _itemName = string.Empty;
            private string _sizeName = string.Empty;
            private int _quantity;
            private decimal _price;
            private decimal _totalPrice;
            private string? _note;

            public int ItemId
            {
                get => _itemId;
                set
                {
                    if (_itemId != value)
                    {
                        _itemId = value;
                        OnPropertyChanged();
                    }
                }
            }

            public string ItemName
            {
                get => _itemName;
                set
                {
                    if (_itemName != value)
                    {
                        _itemName = value;
                        OnPropertyChanged();
                    }
                }
            }

            public string SizeName
            {
                get => _sizeName;
                set
                {
                    if (_sizeName != value)
                    {
                        _sizeName = value;
                        OnPropertyChanged();
                    }
                }
            }

            public int Quantity
            {
                get => _quantity;
                set
                {
                    if (_quantity != value)
                    {
                        _quantity = value;
                        OnPropertyChanged();
                        // Tự động cập nhật TotalPrice khi Quantity thay đổi
                        TotalPrice = _quantity * _price;
                    }
                }
            }

            public decimal Price
            {
                get => _price;
                set
                {
                    if (_price != value)
                    {
                        _price = value;
                        OnPropertyChanged();
                        // Tự động cập nhật TotalPrice khi Price thay đổi
                        TotalPrice = _quantity * _price;
                    }
                }
            }

            public decimal TotalPrice
            {
                get => _totalPrice;
                set
                {
                    if (_totalPrice != value)
                    {
                        _totalPrice = value;
                        OnPropertyChanged();
                    }
                }
            }

            public string? Note
            {
                get => _note;
                set
                {
                    if (_note != value)
                    {
                        _note = value;
                        OnPropertyChanged();
                    }
                }
            }
        }
        #endregion

        #region Datagrid Events
        private void AddItemToDataGrid(OrderItem item, string selectedSize, string note, decimal price)
        {
            if (item == null) return;

            selectedSize = selectedSize.Trim();

            var existingItem = _orders.FirstOrDefault(x => x.ItemId == item.ItemId && x.SizeName == selectedSize && note == x.Note);

            if (existingItem != null)
            {
                existingItem.Quantity++;
                existingItem.TotalPrice = existingItem.Price * existingItem.Quantity;
            }
            else
            {
                _orders.Add(new OrderDetailDisplay
                {
                    ItemId = item.ItemId,
                    ItemName = item.ItemName,
                    SizeName = selectedSize,
                    Quantity = 1,
                    Price = price,
                    Note = note,
                    TotalPrice = price
                });
            }
        }
        private void DeleteOrderItem_MouseDown(object sender, RoutedEventArgs e)
        {
            if (sender is PackIcon ic && ic.DataContext is OrderDetailDisplay item)
            {
                var list = dtgListOrder.ItemsSource as ObservableCollection<OrderDetailDisplay>;
                if (list != null)
                    list.Remove(item); // xóa item khỏi danh sách
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
                if (icon == null) return;
                icon.Kind = PackIconKind.PlusCircle;
            }
        }
        private void bdrPlusQuantity_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Border border && !(border.Tag as bool? ?? false))
            {
                var icon = border.Child as PackIcon;
                if (icon == null) return;
                icon.Kind = PackIconKind.PlusCircleOutline;
            }
        }
        private void bdrMinusQuantity_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Border border)
            {
                var icon = border.Child as PackIcon;
                if (icon == null) return;
                icon.Kind = PackIconKind.MinusCircle;
            }
        }
        private void bdrMinusQuantity_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Border border && !(border.Tag as bool? ?? false))
            {
                var icon = border.Child as PackIcon;
                if (icon == null) return;
                icon.Kind = PackIconKind.MinusCircleOutline;
            }
        }
        private void icPlusQuantity_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border bdr && bdr.DataContext is OrderDetailDisplay item)
                item.Quantity++;
        }
        private void icMinusQuantity_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border bdr && bdr.DataContext is OrderDetailDisplay item)
            {
                var list = dtgListOrder.ItemsSource as ObservableCollection<OrderDetailDisplay>;
                if (list == null) return;

                if (item.Quantity > 1)
                    item.Quantity--;
                else
                    list.Remove(item);
            }
        }
        private void tbItemOrderedQuantity_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (sender is TextBox tb)
                {
                    // Force update binding
                    tb.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();

                    if (tb.DataContext is OrderDetailDisplay item)
                    {
                        if (item.Quantity <= 0)
                        {
                            var list = dtgListOrder.ItemsSource as ObservableCollection<OrderDetailDisplay>;
                            list?.Remove(item);
                        }
                    }
                    Keyboard.ClearFocus();
                }
            }
        }
        private void tbItemOrderedNote_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && sender is TextBox tb && tb.DataContext is OrderDetailDisplay item)
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
            // Chỉ cho phép nhập số
            e.Handled = !IsTextNumeric(e.Text);
        }

        #endregion

        #region Customer

        // Load danh sách khách hàng từ DB
        private void LoadCustomer()
        {
            _customers.Clear();
            using (var db = new CoffeeShopContext())
            {
                var customers = db.Customers.ToList();
                foreach (var customer in customers)
                {
                    _customers.Add(new OrderCustomer
                    {
                        CustomerId = customer.CustomerId,
                        CustomerName = customer.CustomerName,
                        PhoneNumber = customer.PhoneNumber,
                        Email = customer.Email,
                        Point = customer.Point,
                    });
                }
            }
        }
        private void icAddCustomer_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void icSearchCustomer_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }


        #endregion

        #region Table
        private void LoadTable()
        {
            _tables.Clear();
            using (var db = new CoffeeShopContext())
            {
                var tables = db.CafeTables.ToList();
                foreach (var table in tables)
                {
                    _tables.Add(new OrderTable
                    {
                        TableId = table.TableId,
                        TableName = table.TableName,
                        TableStatus = table.TableStatus,
                        Note = table.Note,
                    });
                }
            }
        }
        #endregion
    }
    // Converter để hiển thị số thứ tự trong DataGrid
    public class IndexToNumberConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int index)
            {
                return (index + 1).ToString();
            }
            return "0";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
