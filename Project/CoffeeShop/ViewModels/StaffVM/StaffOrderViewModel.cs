using CoffeeShop.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
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
            FilteredCustomers = new ObservableCollection<OrderCustomer>();
            CurrentCategoryId = 1; // mặc định mở tab đầu tiên

            InitializeCommands();
            LoadData();
            // Load toàn bộ customers vào FilteredCustomers ban đầu
            LoadAllCustomersToFiltered();

            // Mặc định chọn khách hàng là vãng lai (ID = 0)
            SelectedCustomer = Customers.FirstOrDefault(c => c.CustomerId == 0);
            // Mặc định chọn bàn có ID = 0 (mang về)
            SelectedTable = AvailableTables.FirstOrDefault(t => t.TableId == 0);
            LoadDiscountByCustomer();
            Orders.CollectionChanged += Orders_CollectionChanged;
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
            CancelOrderCommand = new RelayCommand<object>(ConfirmCancelOrder);
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
                // Sau khi load, filtered = toàn bộ danh sách
                FilteredItems = new ObservableCollection<OrderItem>(_items);
                OnPropertyChanged(nameof(FilteredItems));
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

        private void LoadDiscountByCustomer()
        {
            if (SelectedCustomer == null)
            {
                SelectedDiscount = null;
                CalculateFinalTotal();
                return;
            }
            _discounts.Clear();

            // Load các discount đang activated
            using (var context = new CoffeeShopContext())
            {
                var allDiscounts = context.Discounts
                    .Where(d => d.IsActive == true)
                    .ToList();

                foreach (var d in allDiscounts)
                {
                    _discounts.Add(new OrderDiscount
                    {
                        DiscountId = d.DiscountId,
                        DiscountCode = d.DiscountCode,
                        DiscountName = d.DiscountName,
                        DiscountType = d.DiscountType,
                        DiscountValue = d.DiscountValue,
                        MinimumOrderValue = d.MinimumOrderValue,
                        MaximumDiscountAmount = d.MaximumDiscountAmount,
                        IsActive = d.IsActive,
                        UsedCount = d.UsedCount
                    });
                }
            }

            // Tự động gán SelectedDiscount theo tier của khách hàng
            if (SelectedCustomer.CustomerId == 0 || SelectedCustomer == null) // Khách vãng lai hoặc null
            {
                SelectedDiscount = null;
            }
            else
            {
                string customerTier = SelectedCustomer.Tier;

                // Tìm discount theo tier (VIP1, VIP10, VIP100)
                var tierDiscount = Discounts.FirstOrDefault(d => d.DiscountCode.Equals(customerTier, StringComparison.OrdinalIgnoreCase));

                SelectedDiscount = tierDiscount;
            }
            // Tính lại FinalTotal
            CalculateFinalTotal();
        }

        private void FilterItemsByCategory()
        {
            SearchItems();
        }

        #endregion

        #region Management Orders Methods 
        private void Orders_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (OrderDetailItem item in e.NewItems)
                {
                    item.PropertyChanged += OrderItem_PropertyChanged;
                }
            }

            if (e.OldItems != null)
            {
                foreach (OrderDetailItem item in e.OldItems)
                {
                    item.PropertyChanged -= OrderItem_PropertyChanged;
                }
            }
            CalculateTotalAmount();
        }

        private void OrderItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Khi total price thay đổi → cập nhật total amount
            if (e.PropertyName == nameof(OrderDetailItem.TotalPrice))
            {
                CalculateTotalAmount();
            }
        }


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
            CalculateFinalTotal();
        }

        private void CalculateFinalTotal()
        {
            decimal totalAfterDiscount = TotalAmount;
            decimal discountValueApplied = 0;

            if (SelectedDiscount != null)
            {
                if (TotalAmount >= (SelectedDiscount.MinimumOrderValue ?? 0)
                    && SelectedDiscount.IsActive == true)
                {
                    if (SelectedDiscount.DiscountType == 0) // Fixed amount
                    {
                        discountValueApplied = SelectedDiscount.DiscountValue;
                    }
                    else if (SelectedDiscount.DiscountType == 1) // Percent
                    {
                        discountValueApplied = TotalAmount * SelectedDiscount.DiscountValue / 100m;
                    }

                    // Giới hạn theo MaximumDiscountAmount
                    if (SelectedDiscount.MaximumDiscountAmount.HasValue)
                    {
                        discountValueApplied = Math.Min(discountValueApplied, SelectedDiscount.MaximumDiscountAmount.Value);
                    }

                    // Giới hạn discount không được vượt quá tổng tiền
                    discountValueApplied = Math.Min(discountValueApplied, TotalAmount);
                }
            }

            // Cập nhật thuộc tính Discount (Số tiền giảm thực tế)
            FinalDiscount = discountValueApplied;

            // Tính toán tổng tiền cuối cùng
            FinalTotal = TotalAmount - FinalDiscount;

            OnPropertyChanged(nameof(FinalDiscount));
            OnPropertyChanged(nameof(FinalTotal));
        }
        private void ConfirmCancelOrder(object param)
        {
            var result = MessageBox.Show(
                "Bạn có chắc muốn hủy đơn này không?",
                "Xác nhận hủy đơn",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                CancelOrder(param);
            }
        }
        private void CancelOrder(object param)
        {
            Orders.Clear();
            CalculateTotalAmount();
            SelectedCustomer = null;
            SearchCustomerKeyword = "";

            // Mặc định chọn bàn có ID = 0 (mang về)
            SelectedTable = _availableTables.FirstOrDefault(t => t.TableId == 0);
            // Mặc định chọn khách hàng là vãng lai (ID = 0)
            SelectedCustomer = _customers.FirstOrDefault(c => c.CustomerId == 0);
            // cập nhật discount sau khi reset
            LoadDiscountByCustomer();
        }
        #endregion

        #region Search Methods
        // Tìm kiếm món trong MenuPanel
        private void SearchItems()
        {
            IEnumerable<OrderItem> source;

            // Tab "Tất cả"
            if (CurrentCategoryId == 0)
                source = Items;
            else
                source = Items.Where(i => i.CategoryId == CurrentCategoryId);

            // Không có keyword --> trả danh sách theo tab
            if (string.IsNullOrWhiteSpace(SearchItemKeyword))
            {
                FilteredItems = new ObservableCollection<OrderItem>(source);
                return;
            }

            var keyword = SearchItemKeyword.Trim();

            var result = source
                .Where(i => i.ItemName.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();

            FilteredItems = new ObservableCollection<OrderItem>(result);
        }


        // Tìm kiếm khách hàng
        private void SearchCustomer(object parameter)
        {
            if (FilteredCustomers == null)
                FilteredCustomers = new ObservableCollection<OrderCustomer>();

            FilteredCustomers.Clear();

            // Nếu ko nhập gì --> hiển thị toàn bộ customer trong DB
            if (string.IsNullOrWhiteSpace(SearchCustomerKeyword))
            {
                foreach (var c in _customers)
                    FilteredCustomers.Add(c);
            }
            else
            {
                var filtered = _customers.Where(c =>
                    !string.IsNullOrEmpty(c.PhoneNumber) &&
                    c.PhoneNumber.Contains(SearchCustomerKeyword, StringComparison.OrdinalIgnoreCase)
                );

                foreach (var c in filtered)
                    FilteredCustomers.Add(c);
            }

            OnPropertyChanged(nameof(FilteredCustomers));
            OnPropertyChanged(nameof(HasSearchResults));
        }

        // Chỉ load toàn bộ khách hàng vào FilteredCustomers
        public void LoadAllCustomersToFiltered()
        {
            if (_customers == null || _customers.Count == 0) return;

            if (FilteredCustomers == null)
                FilteredCustomers = new ObservableCollection<OrderCustomer>();

            FilteredCustomers.Clear();

            foreach (var c in _customers)
                FilteredCustomers.Add(c);

            OnPropertyChanged(nameof(FilteredCustomers));
            OnPropertyChanged(nameof(HasSearchResults));
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
                // cập nhật khuyến mãi & tổng tiền sau khi chọn khách mới
                LoadDiscountByCustomer();
                CalculateTotalAmount();
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