using CoffeeShop.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Windows;

namespace CoffeeShop.ViewModels.StaffVM
{
    public partial class StaffOrderViewModel
    {
        #region Constructor
        public StaffOrderViewModel()
        {
            Items = new ObservableCollection<OrderItem>();
            Tables = new ObservableCollection<OrderTable>();
            Customers = new ObservableCollection<OrderCustomer>();
            Orders = new ObservableCollection<OrderDetailItem>();
            AvailableTables = new ObservableCollection<OrderTable>();
            FilteredItems = new ObservableCollection<OrderItem>();
            CurrentCategoryId = 1; // mặc định mở tab đầu tiên

            InitializeCommands();
            LoadData();

            // Mặc định chọn khách hàng là vãng lai (ID = 0)
            SelectedCustomer = _customers.FirstOrDefault(c => c.CustomerId == 0);
            // Mặc định chọn bàn có ID = 0 (mang về)
            SelectedTable = _availableTables.FirstOrDefault(t => t.TableId == 0);

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
            AddCustomerCommand = new RelayCommand<object>(param =>
            {
                if (param is Tuple<string, string, string> data)
                    AddCustomer(data.Item1, data.Item2, data.Item3);
            });
            ChooseCustomerCommand = new RelayCommand<Customer>(c =>
            {
                SelectedCustomer = new OrderCustomer
                {
                    CustomerId = c.CustomerId,
                    CustomerName = c.CustomerName,
                    PhoneNumber = c.PhoneNumber,
                    Email = c.Email,
                    Point = c.Point,
                    Tier = c.Tier
                };
            });
            ChooseTableCommand = new RelayCommand<OrderTable>(ChooseTable);
            CancelOrderCommand = new RelayCommand<object>(CancelOrder);
        }
        #endregion

        #region Load Data Methods
        private void LoadData()
        {
            LoadOrderItemsFromDB();
            LoadCustomerFromDB();
            LoadTableFromDB();
            LoadAvailableTable();
            FilterItemsByCategory();
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
            // Khởi tạo đối tượng khách hàng vãng lai
            var defaultCustomer = new OrderCustomer
            {
                CustomerId = 0, // ID = 0 không được tồn tại trong DB
                CustomerName = "Khách vãng lai",
                PhoneNumber = null,
                Email = null,
            };
            try
            {
                using (var db = new CoffeeShopContext())
                {
                    var customers = db.Customers.ToList();

                    // Thêm khách vãng lai vào đầu danh sách KH
                    _customers.Insert(0, defaultCustomer);

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
            // Thêm bàn mặc định
            var placeholderTable = new OrderTable
            {
                TableId = 0, // ID = 0 không được tồn tại trong DB
                TableName = "Không",
                TableStatus = 0,
                Note = null
            };

            // Thêm bàn mặc định vào đầu danh sách
            AvailableTables.Insert(0, placeholderTable);
        }

        private void FilterItemsByCategory()
        {
            // Đảm bảo FilteredItems được khởi tạo
            if (_filteredItems == null)
            {
                _filteredItems = new ObservableCollection<OrderItem>();
            }
            _filteredItems.Clear();

            var itemsToDisplay = _items.Where(i => i.CategoryId == CurrentCategoryId);

            foreach (var item in itemsToDisplay)
            {
                _filteredItems.Add(item);
            }

            // Thông báo cho View rằng danh sách đã thay đổi
            OnPropertyChanged(nameof(FilteredItems));
        }
        #endregion

        #region Management Orders Methods 

        // Gộp Item trong orders khi trùng note
        private void MergeItemOnNoteChange(OrderDetailItem modifiedItem)
        {
            // Tìm kiếm mục khác có ItemId và Note trùng với modifiedItem
            var existingItem = Orders.FirstOrDefault(o =>
                o != modifiedItem &&
                o.ItemId == modifiedItem.ItemId &&
                o.Note == modifiedItem.Note
            );

            if (existingItem != null)
            {
                existingItem.Quantity += modifiedItem.Quantity;
                Orders.Remove(modifiedItem);
                CalculateTotalAmount();
            }
        }
        // Thêm món vào đơn hàng  
        public void AddItemToOrder(OrderItem item, string selectedSize, string note, decimal price)
        {
            if (item == null) return;
            selectedSize = selectedSize ?? string.Empty;
            note = string.IsNullOrEmpty(note) ? null : note.Trim();

            var newItem = new OrderDetailItem
            {
                ItemId = item.ItemId,
                ItemName = item.ItemName,
                SizeName = selectedSize,
                Quantity = 1,
                Price = price,
                Note = note
            };
            newItem.NoteChangedCallback = MergeItemOnNoteChange;

            var existingItem = Orders.FirstOrDefault(i => i.ItemId == item.ItemId && i.SizeName == selectedSize && i.Note == note);

            if (existingItem != null)
                existingItem.Quantity++;
            else
                Orders.Add(newItem);
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
        }

        // Tăng số lượng món trong đơn hàng
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

        // Xóa order đã chọn, khôi phục các biến về trạng thái ban đầu 
        private void CancelOrder(object param)
        {
            Orders.Clear();
            CalculateTotalAmount();
            SelectedCustomer = null;

            // Mặc định chọn bàn có ID = 0 (mang về)
            SelectedTable = _availableTables.FirstOrDefault(t => t.TableId == 0);
            // Mặc định chọn khách hàng là vãng lai (ID = 0)
            SelectedCustomer = _customers.FirstOrDefault(c => c.CustomerId == 0);
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
        #endregion

        #region Management Customers Methods
        // Thêm khách hàng mới
        public OrderCustomer AddCustomer(string customerName, string customerPhoneNumber, string customerEmail)
        {
            if (string.IsNullOrWhiteSpace(customerName) || string.IsNullOrWhiteSpace(customerPhoneNumber))
            {
                MessageBox.Show("Vui lòng nhập tên và số điện thoại khách hàng.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
            using (var db = new CoffeeShopContext())
            {
                // Kiểm tra khách hàng đã tồn tại chưa theo số điện thoại
                var existingCustomer = db.Customers.FirstOrDefault(c => c.PhoneNumber == customerPhoneNumber);
                if (existingCustomer != null)
                {
                    MessageBox.Show("Khách hàng với số điện thoại này đã tồn tại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    // Trả về khách hàng đã tồn tại (dùng để chọn vào SelectedCustomer)
                    var exist = new OrderCustomer
                    {
                        CustomerName = existingCustomer.CustomerName,
                        PhoneNumber = existingCustomer.PhoneNumber,
                        Email = existingCustomer.Email,
                        Point = existingCustomer.Point,
                        Tier = existingCustomer.Tier
                    };
                    SelectedCustomer = exist;
                    return exist;
                }

                // Thêm khách hàng mới
                var newcustomer = new Customer
                {
                    CustomerName = customerName,
                    PhoneNumber = customerPhoneNumber,
                    Email = customerEmail,
                };
                db.Customers.Add(newcustomer);
                db.SaveChanges();
                MessageBox.Show("Thêm khách hàng thành công.", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                // Reload danh sách khách hàng
                LoadCustomerFromDB();

                var oc = new OrderCustomer
                {
                    CustomerId = newcustomer.CustomerId,
                    CustomerName = newcustomer.CustomerName,
                    PhoneNumber = newcustomer.PhoneNumber,
                    Email = newcustomer.Email
                };

                SelectedCustomer = oc;
                return oc;
            }
        }
        #endregion

        #region Management Tables Methods
        private void ChooseTable(OrderTable table)
        {
            if (table == null) return;

            if (SelectedTable != null && SelectedTable.TableId != table.TableId)
            {
                if (MessageBox.Show($"Bạn đang chọn {SelectedTable.TableName}. Đổi sang {table.TableName}?",
                            "Xác nhận đổi bàn", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                    return;
            }
            SelectedTable = table;
        }
        #endregion
    }
}