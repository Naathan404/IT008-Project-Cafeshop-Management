using CoffeeShop.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace CoffeeShop.ViewModels.StaffVM
{
    public class StaffOrderViewModel : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Properties
        // Món trong MenuPanel
        private ObservableCollection<OrderItem> _items = new ObservableCollection<OrderItem>();
        public ObservableCollection<OrderItem> Items
        {
            get { return _items; }
            set
            {
                _items = value;
                OnPropertyChanged(nameof(Items));
            }
        }
        // Bàn
        private ObservableCollection<OrderTable> _tables = new ObservableCollection<OrderTable>();
        public ObservableCollection<OrderTable> Tables
        {
            get { return _tables; }
            set
            {
                _tables = value;
                OnPropertyChanged(nameof(Tables));
            }
        }
        // Khách hàng
        private ObservableCollection<OrderCustomer> _customers = new ObservableCollection<OrderCustomer>();
        public ObservableCollection<OrderCustomer> Customers
        {
            get { return _customers; }
            set
            {
                _customers = value;
                OnPropertyChanged(nameof(Customers));
            }
        }
        // Món trong đơn hàng
        private ObservableCollection<OrderDetailItem> _orders = new ObservableCollection<OrderDetailItem>();
        public ObservableCollection<OrderDetailItem> Orders
        {
            get { return _orders; }
            set
            {
                _orders = value;
                OnPropertyChanged(nameof(Orders));
            }
        }
        // Bàn trống có thể chọn để đặt món
        private ObservableCollection<OrderTable> _availableTables = new ObservableCollection<OrderTable>();
        public ObservableCollection<OrderTable> AvailableTables
        {
            get { return _availableTables; }
            set
            {
                _availableTables = value;
                OnPropertyChanged(nameof(AvailableTables));
            }
        }
        // Bàn được chọn để đặt món
        private ObservableCollection<OrderTable> _selectedTable = new ObservableCollection<OrderTable>();
        public ObservableCollection<OrderTable> SelectedTable
        {
            get { return _selectedTable; }
            set
            {
                _selectedTable = value;
                OnPropertyChanged(nameof(SelectedTable));
            }
        }
        // Tìm kiếm món trong MenuPanel
        private string _seachItemKeyword;
        public string SearchItemKeyword
        {
            get { return _seachItemKeyword; }
            set
            {
                _seachItemKeyword = value;
                OnPropertyChanged(nameof(SearchItemKeyword));
                SearchItems();
            }
        }
        // Tìm kiếm khách hàng
        private string _seachCustomerKeyword;
        public string SearchCustomerKeyword
        {
            get { return _seachCustomerKeyword; }
            set
            {
                _seachCustomerKeyword = value;
                OnPropertyChanged(nameof(SearchCustomerKeyword));
                SearchCustomer(null);
                OnPropertyChanged(nameof(HasSearchResults)); // Notify để mở popup
            }
        }
        // Kiểm tra có kết quả tìm kiếm khách hàng hay không
        public bool HasSearchResults => Customers.Count > 0;

        // Khách hàng được chọn
        private OrderCustomer _selectedCustomer;
        public OrderCustomer SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                _selectedCustomer = value;
                OnPropertyChanged(nameof(SelectedCustomer));
            }
        }
        // Tổng tiền đơn hàng
        private decimal _totalAmount;
        public decimal TotalAmount
        {
            get => _totalAmount;
            set
            {
                _totalAmount = value;
                OnPropertyChanged(nameof(TotalAmount));
            }
        }
        // Giảm giá
        public decimal _discount;
        public decimal Discount
        {
            get => _discount;
            set
            {
                _discount = value;
                OnPropertyChanged(nameof(Discount));
            }
        }
        #endregion

        #region Commands
        public ICommand AddItemCommand { get; set; }
        public ICommand RemoveItemCommand { get; set; }
        public ICommand IncreaseQuantityCommand { get; set; }
        public ICommand DecreaseQuantityCommand { get; set; }
        public ICommand SearchCustomerCommand { get; set; }
        public ICommand AddCustomerCommand { get; set; }
        public ICommand PlaceOrderCommand { get; set; }
        #endregion

        #region Constructor
        public StaffOrderViewModel()
        {
            Items = new ObservableCollection<OrderItem>();
            Tables = new ObservableCollection<OrderTable>();
            Customers = new ObservableCollection<OrderCustomer>();
            Orders = new ObservableCollection<OrderDetailItem>();
            AvailableTables = new ObservableCollection<OrderTable>();
            InitializeCommands();
            LoadData();
            Orders.CollectionChanged += (s, e) => CalculateTotalAmount();
        }
        #endregion

        #region Command Initialization
        private void InitializeCommands()
        {
            AddItemCommand = new RelayCommand<object>(param =>
            {
                if (param is Tuple<OrderItem, string, string, decimal> data)
                    AddItemToOrder(data.Item1, data.Item2, data.Item3, data.Item4);
            });
            RemoveItemCommand = new RelayCommand<OrderDetailItem>(RemoveItemFromOrder);
            IncreaseQuantityCommand = new RelayCommand<OrderDetailItem>(IncreaseQuantity);
            DecreaseQuantityCommand = new RelayCommand<OrderDetailItem>(DecreaseQuantity);
            SearchCustomerCommand = new RelayCommand<object>(SearchCustomer);
            AddCustomerCommand = new RelayCommand<object>(AddCustomer);
            //PlaceOrderCommand = new RelayCommand<object>(PlaceOrder, CanPlaceOrder);
        }
        #endregion

        #region Load Data Methods
        private void LoadData()
        {
            LoadOrderItemsFromDB();
            LoadCustomerFromDB();
            LoadTableFromDB();
            LoadAvailableTable();
        }

        //Load dữ liệu từ DB vào MenuPanel
        private void LoadOrderItemsFromDB()
        {
            _items.Clear();
            try
            {
                using (var context = new CoffeeShopContext())
                {
                    var items = context.Items.ToList();
                    foreach (var item in items)
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
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading items: {ex.Message}");
            }
        }

        // Load danh sách khách hàng từ DB
        private void LoadCustomerFromDB()
        {
            _customers.Clear();
            try
            {
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
                            Tier = customer.Tier,
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading customers: {ex.Message}");
            }
        }
        // Load danh sách bàn từ DB
        private void LoadTableFromDB()
        {
            _tables.Clear();
            try
            {
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
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading items: {ex.Message}");
            }
        }
        // Load danh sách bàn trống
        private void LoadAvailableTable()
        {
            AvailableTables.Clear();
            var availableTables = _tables.Where(t => t.TableStatus == 0).ToList();
            foreach (var table in availableTables)
            {
                AvailableTables.Add(table);
            }
        }
        #endregion

        #region Management Methods
        // Thêm món vào đơn hàng  
        public void AddItemToOrder(OrderItem item, string selectedSize, string note, decimal price)
        {
            if (item == null) return;
            selectedSize = selectedSize ?? string.Empty;
            note = string.IsNullOrEmpty(note) ? null : note.Trim();

            var existingItem = Orders.FirstOrDefault(i => i.ItemId == item.ItemId && i.SizeName == selectedSize && i.Note == note);

            if (existingItem != null)
            {
                existingItem.Quantity++;
            }
            else
            {
                Orders.Add(new OrderDetailItem
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
            CalculateTotalAmount();
        }
        // Xóa món khỏi đơn hàng
        private void RemoveItemFromOrder(OrderDetailItem item)
        {
            if (item != null && Orders.Contains(item))
            {
                Orders.Remove(item);
                CalculateTotalAmount();
            }
        }// Tăng số lượng món trong đơn hàng
        private void IncreaseQuantity(OrderDetailItem item)
        {
            if (item != null)
            {
                item.Quantity++;
                CalculateTotalAmount();
            }
        }
        // Giảm số lượng món trong đơn hàng
        private void DecreaseQuantity(OrderDetailItem item)
        {
            if (item == null) return;
            if (item != null && item.Quantity > 1)
                item.Quantity--;
            else
                Orders.Remove(item);
            CalculateTotalAmount();
        }
        // Tính tổng tiền đơn hàng
        private void CalculateTotalAmount()
        {
            TotalAmount = Orders.Sum(o => o.TotalPrice);
        }

        #endregion

        #region Search Methods
        // Tìm kiếm món trong MenuPanel
        private void SearchItems()
        {
            if (string.IsNullOrWhiteSpace(SearchItemKeyword))
            {
                LoadOrderItemsFromDB();
                return;
            }
            var filteredItems = Items.Where(i => i.ItemName.IndexOf(SearchItemKeyword, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            Items.Clear();
            foreach (var item in filteredItems)
            {
                Items.Add(item);
            }
        }
        // Tìm kiếm khách hàng
        private void SearchCustomer(object parameter)
        {
            if (string.IsNullOrWhiteSpace(SearchCustomerKeyword))
            {
                // Reset về danh sách gốc
                Customers.Clear();
                LoadCustomerFromDB();
                return;
            }
            // Lấy tất cả khách hàng từ DB
            var allCustomers = _customers.ToList();
            // Lọc theo PhoneNumber trên danh sách gốc
            var filteredCustomers = allCustomers.Where(c => !string.IsNullOrEmpty(c.PhoneNumber) &&
                            c.PhoneNumber.Contains(SearchCustomerKeyword)).ToList();

            Customers.Clear();
            foreach (var c in filteredCustomers)
                Customers.Add(c);
        }
        // Thêm khách hàng mới
        private void AddCustomer(object parameter)
        {

        }
        #endregion

        #region Order Placement
        //private bool CanPlaceOrder(object parameter)
        //{
        //    return Orders.Count > 0 && SelectedTable != null;
        //}

        //private void PlaceOrder(object parameter)
        //{
        //    if (!CanPlaceOrder(parameter))
        //        return;

        //    try
        //    {
        //        using (var db = new CoffeeShopContext())
        //        {
        //            // Create new order
        //            var newOrder = new Order
        //            {
        //                TableId = SelectedTable.TableId,
        //                OrderDate = DateTime.Now,
        //                OrderStatus = 0, // Pending
        //                TotalAmount = TotalAmount
        //            };

        //            db.Orders.Add(newOrder);
        //            db.SaveChanges();

        //            // Add order details
        //            foreach (var orderDetail in Orders)
        //            {
        //                // Find the correct size ID from ItemPrices
        //                int? sizeId = null;
        //                using (var tempDb = new CoffeeShopContext())
        //                {
        //                    var itemPrice = tempDb.ItemPrices
        //                        .Include(ip => ip.Size)
        //                        .FirstOrDefault(ip => ip.ItemId == orderDetail.ItemId &&
        //                                             ip.Size.SizeName == orderDetail.SizeName);
        //                    sizeId = itemPrice?.SizeId;
        //                }

        //                db.OrderDetails.Add(new OrderDetail
        //                {
        //                    OrderId = newOrder.OrderId,
        //                    ItemId = orderDetail.ItemId,
        //                    SizeId = sizeId,
        //                    Quantity = orderDetail.Quantity,
        //                    Price = orderDetail.Price,
        //                    Note = orderDetail.Note
        //                });
        //            }

        //            // Update table status
        //            var table = db.CafeTables.Find(SelectedTable.TableId);
        //            if (table != null)
        //            {
        //                table.TableStatus = 1; // Occupied
        //            }

        //            db.SaveChanges();

        //            // Clear order after successful placement
        //            Orders.Clear();
        //            SelectedTable = null;
        //            LoadTableFromDB();
        //            LoadAvailableTable();
        //            CalculateTotalAmount();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Error placing order: {ex.Message}");
        //        // You can add error notification to user here
        //    }
        //}
        #endregion

        #region Helper Methods
        public class OrderItem : NotificationBase
        {
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
                set { _itemId = value; OnPropertyChanged(); }
            }

            public string ItemName
            {
                get => _itemName;
                set { _itemName = value; OnPropertyChanged(); }
            }

            public int CategoryId
            {
                get => _categoryId;
                set { _categoryId = value; OnPropertyChanged(); }
            }

            public int Quantity
            {
                get => _quantity;
                set { _quantity = value; OnPropertyChanged(); }
            }

            public bool IsAvailable
            {
                get => _isAvailable;
                set { _isAvailable = value; OnPropertyChanged(); }
            }

            public ObservableCollection<ItemPrice> ItemPrices
            {
                get => _itemPrices;
                set { _itemPrices = value; OnPropertyChanged(); }
            }

            public string ImagePath
            {
                get => _imagePath;
                set { _imagePath = value; OnPropertyChanged(); }
            }

            public OrderItem()
            {
                _itemPrices = new ObservableCollection<ItemPrice>();
                _imagePath = "/Assets/Images/imgItemExample.jpg";
            }
        }

        public class OrderCustomer : NotificationBase
        {
            private int _customerId;
            private string _customerName = string.Empty;
            private string _phoneNumber;
            private string _email;
            private int _point;
            private string _tier;

            public int CustomerId
            {
                get => _customerId;
                set { _customerId = value; OnPropertyChanged(); }
            }

            public string CustomerName
            {
                get => _customerName;
                set { _customerName = value; OnPropertyChanged(); }
            }

            public string PhoneNumber
            {
                get => _phoneNumber;
                set { _phoneNumber = value; OnPropertyChanged(); }
            }

            public string Email
            {
                get => _email;
                set { _email = value; OnPropertyChanged(); }
            }

            public int Point
            {
                get => _point;
                set { _point = value; OnPropertyChanged(); }
            }

            public string Tier
            {
                get => _tier;
                set { _tier = value; OnPropertyChanged(); }
            }
        }
        public class OrderTable : NotificationBase
        {
            private int _tableId;
            private string _tableName = string.Empty;
            private int _tableStatus;
            private string _note;

            public int TableId
            {
                get => _tableId;
                set { _tableId = value; OnPropertyChanged(); }
            }

            public string TableName
            {
                get => _tableName;
                set { _tableName = value ?? string.Empty; OnPropertyChanged(); }
            }

            public int TableStatus
            {
                get => _tableStatus;
                set { _tableStatus = value; OnPropertyChanged(); }
            }

            public string Note
            {
                get => _note;
                set { _note = value; OnPropertyChanged(); }
            }
        }
        public class OrderDetailItem : NotificationBase
        {
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
                set { _itemId = value; OnPropertyChanged(); }
            }

            public string ItemName
            {
                get => _itemName;
                set { _itemName = value; OnPropertyChanged(); }
            }

            public string SizeName
            {
                get => _sizeName;
                set { _sizeName = value; OnPropertyChanged(); }
            }

            public int Quantity
            {
                get => _quantity;
                set
                {
                    _quantity = value;
                    OnPropertyChanged();
                    TotalPrice = _quantity * _price;
                }
            }

            public decimal Price
            {
                get => _price;
                set
                {
                    _price = value;
                    OnPropertyChanged();
                    TotalPrice = _quantity * _price;
                }
            }

            public decimal TotalPrice
            {
                get => _totalPrice;
                set { _totalPrice = value; OnPropertyChanged(); }
            }

            public string Note
            {
                get => _note;
                set { _note = value; OnPropertyChanged(); }
            }
        }
        public class NotificationBase : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}
