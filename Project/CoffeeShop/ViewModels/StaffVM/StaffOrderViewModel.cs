using CoffeeShop.Models;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;

namespace CoffeeShop.ViewModels.StaffVM
{
    public partial class StaffOrderViewModel
    {
        #region Constructor
        public StaffOrderViewModel()
        {
            Items.Clear();
            Tables.Clear();
            Customers.Clear();
            Orders.Clear();
            AvailableTables.Clear();
            FilteredItems.Clear();
            FilteredCustomers.Clear();
            CurrentCategoryId = 1; // mặc định mở tab đầu tiên

            InitializeCommands();
            LoadData();
            // Load toàn bộ customers vào FilteredCustomers ban đầu
            LoadAllCustomersToFiltered();

            // Mặc định chọn khách hàng là vãng lai (ID = 0)
            SelectedCustomer = Customers.FirstOrDefault(c => c.CustomerId == 0);
            // Mặc định chọn bàn có ID = 0 (mang về)
            SelectedTable = AvailableTables.FirstOrDefault(t => t.TableId == 0);
            // Mặc định chọn thanh toán bằng tiền mặt
            SelectedPaymentMethod = PaymentMethod.FirstOrDefault(p => p.Equals("Tiền mặt")) ?? "";
            // Mặc định không in bill
            IsCheckedPrintBill = false;
            LoadDiscountByCustomer();
            // Mặc định chọn không áp dụng mã giảm giá
            SelectedDiscount = AvailableDiscounts.FirstOrDefault(d => d.DiscountId == 0);
            Orders.CollectionChanged += Orders_CollectionChanged;

            // Khi có bất kỳ sự thay đổi availabe của items thì sẽ nhận được ReloadMenuMessage ==> load lại dữ liệu và kiểm tra giỏ hàng
            WeakReferenceMessenger.Default.Register<ReloadMenuMessage>(this, (r, m) =>
            {
                // Kiểm tra giỏ hàng
                var invalidItems = GetInvalidOrderItems();
                ReloadListItems();
                if (invalidItems.Any())
                {
                    string names = string.Join(", ", invalidItems.Select(x => x.ItemName));
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (MessageBox.Show($"Cảnh báo: Món [{names}] vừa bị tắt hoặc không còn tồn tại.\n" +
                            "Hệ thống sẽ xóa món này khỏi giỏ hàng!",
                            "Món ăn không khả dụng", MessageBoxButton.OK, MessageBoxImage.Warning) == MessageBoxResult.OK)
                        {
                            // Xóa món lỗi
                            foreach (var item in invalidItems.ToList())
                            {
                                Orders.Remove(item);
                            }
                        }
                    });
                }
            });
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
                if (c == null) return;
                SelectedCustomer = new OrderCustomer
                {
                    CustomerId = c.CustomerId,
                    CustomerName = c.CustomerName,
                    PhoneNumber = c.PhoneNumber ?? "",
                    Email = c.Email,
                    Point = c.Point,
                    Tier = c.Tier ?? ""
                };
            });
            ChooseTableCommand = new RelayCommand<OrderTable>(ChooseTable);
            CancelOrderCommand = new RelayCommand<object>(ConfirmCancelOrder);
            PayOrderCommand = new RelayCommand<object>(PayOrderWindow);
            ConfirmPayOrderCommand = new RelayCommand<object>(ConfirmPayOrder);
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
            LoadPaymentMethod();
            LoadDiscountFromDB();
        }

        private void ReloadListItems()
        {
            LoadOrderItemsFromDB();
        }

        //Load dữ liệu từ DB vào MenuPanel
        private void LoadOrderItemsFromDB()
        {
            _items.Clear();
            try
            {
                using (var context = new CoffeeShopContext())
                {
                    var items = context.Items.Where(i => i.IsDeleted == false && i.IsAvailable == true).ToList();
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
                                                .Where(ip => ip.ItemId == item.ItemId && ip.IsDeleted == false)
                                                .ToList()),
                            Info = item.Info == null ? string.Empty : item.Info.ToString(),
                            ImagePath = item.ImagePath == null ? string.Empty : item.ImagePath.ToString()
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
                PhoneNumber = "",
                Email = "",
            };
            try
            {
                using (var db = new CoffeeShopContext())
                {
                    var customers = db.Customers.Where(c => c.IsDeleted == false).ToList();

                    // Thêm khách vãng lai vào đầu danh sách KH
                    _customers.Insert(0, defaultCustomer);

                    foreach (var customer in customers)
                    {
                        _customers.Add(new OrderCustomer
                        {
                            CustomerId = customer.CustomerId,
                            CustomerName = customer.CustomerName,
                            PhoneNumber = customer.PhoneNumber ?? "",
                            Email = customer.Email,
                            Point = customer.Point,
                            Tier = customer.Tier ?? ""
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
                    var tables = db.CafeTables.Where(t => t.IsDeleted == false).ToList();
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
            var availableTables = Tables.Where(t => t.TableStatus == 0).ToList();
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
        private void LoadDiscountFromDB()
        {
            Discounts.Clear();

            // Thêm option "Không áp dụng" vào đầu danh sách
            var noDiscount = new OrderDiscount
            {
                DiscountId = 0,
                DiscountCode = "NONE",
                DiscountName = "Không áp dụng",
                DiscountType = 0,
                DiscountValue = 0,
                IsActive = true,
                IsEligible = true,
                MinimumOrderValue = null,
                MaximumDiscountAmount = null,
                UsedCount = 0
            };
            Discounts.Add(noDiscount);
            try
            {
                using (var db = new CoffeeShopContext())
                {
                    var discounts = db.Discounts.Where(t => t.IsActive == true).ToList();
                    foreach (var discount in discounts)
                    {
                        Discounts.Add(new OrderDiscount
                        {
                            DiscountId = discount.DiscountId,
                            DiscountCode = discount.DiscountCode,
                            DiscountName = discount.DiscountName,
                            DiscountType = discount.DiscountType,
                            DiscountValue = discount.DiscountValue,
                            IsActive = discount.IsActive,
                            MinimumOrderValue = discount.MinimumOrderValue,
                            MaximumDiscountAmount = discount.MaximumDiscountAmount,
                            UsedCount = discount.UsedCount,
                            IsEligible = true
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading items: {ex.Message}");
            }
        }
        private void LoadDiscountByCustomer()
        {
            AvailableDiscounts.Clear();
            SelectedDiscount = null;

            if (SelectedCustomer == null)
            {
                CalculateFinalTotal();
                return;
            }

            // thêm "Không áp dụng" vào đầu
            var noDiscount = Discounts.FirstOrDefault(d => d.DiscountId == 0);
            if (noDiscount != null)
                AvailableDiscounts.Add(noDiscount);

            // Nếu là khách vãng lai
            if (SelectedCustomer.CustomerId == 0)
            {
                // Chỉ hiển thị các discount không phải VIP và đủ điều kiện
                var publicDiscounts = Discounts
                    .Where(d => d.DiscountId != 0 &&
                                !d.DiscountCode.StartsWith("VIP", StringComparison.OrdinalIgnoreCase) &&
                                (d.MinimumOrderValue == null || TotalAmount >= d.MinimumOrderValue))
                    .ToList();

                foreach (var d in publicDiscounts)
                {
                    AvailableDiscounts.Add(d);
                }

                // Mặc định chọn "Không áp dụng"
                SelectedDiscount = noDiscount;
            }
            else // Khách hàng thành viên
            {
                string customerTier = SelectedCustomer.Tier;

                // Lọc và chỉ thêm các discount hợp lệ
                var eligibleDiscounts = Discounts
                    .Where(d => d.DiscountId != 0)
                    .Where(d =>
                    {
                        // Check nếu là discount của khách hàng thành viên
                        if (d.DiscountCode.StartsWith("VIP", StringComparison.OrdinalIgnoreCase))
                        {
                            // Check tier
                            if (d.DiscountCode.Equals(customerTier, StringComparison.OrdinalIgnoreCase))
                                return true;
                            else if (customerTier == "VIP100")
                                return true; // VIP100 dùng được tất cả
                            else if (customerTier == "VIP10" && d.DiscountCode.Equals("VIP1", StringComparison.OrdinalIgnoreCase))
                                return true; // VIP10 dùng được VIP1
                            else
                                return false;
                        }
                        // check nếu là discount công khai
                        return true;
                    })
                    .Where(d => d.MinimumOrderValue == null || TotalAmount >= d.MinimumOrderValue)
                    .ToList();

                foreach (var d in eligibleDiscounts)
                {
                    AvailableDiscounts.Add(d);
                }

                // Mặc định chọn "Không áp dụng"
                SelectedDiscount = noDiscount;
            }

            // Tính lại FinalTotal
            CalculateFinalTotal();
        }

        private void FilterItemsByCategory()
        {
            SearchItems();
        }

        private void LoadPaymentMethod()
        {
            PaymentMethod.Add("Tiền mặt");
            PaymentMethod.Add("Chuyển khoản");
        }
        #endregion

        #region Management Orders Methods 
        private void Orders_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
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

        private void OrderItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // Khi total price thay đổi --> cập nhật total amount
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
        public void AddItemToOrder(OrderItem item, string? selectedSize, string? note, decimal price)
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
                Orders.Remove(item!);
            CalculateTotalAmount();
        }
        // Tính tổng tiền đơn hàng
        private void CalculateTotalAmount()
        {
            TotalAmount = Orders.Sum(o => o.TotalPrice);
            LoadDiscountByCustomer();
        }

        private void CalculateFinalTotal()
        {
            decimal totalAfterDiscount = TotalAmount;
            decimal discountValueApplied = 0;

            if (SelectedDiscount != null && SelectedDiscount.DiscountId != 0)
            {
                if (TotalAmount >= (SelectedDiscount.MinimumOrderValue ?? 0)
                    && SelectedDiscount.IsActive == true && SelectedDiscount.IsEligible == true)
                {
                    if (SelectedDiscount.DiscountType == 1) // Fixed amount
                    {
                        discountValueApplied = SelectedDiscount.DiscountValue;
                    }
                    else if (SelectedDiscount.DiscountType == 0) // Percent
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
        }
        private void ConfirmCancelOrder(object param)
        {
            if (CanCancelOrder)
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
            else
                MessageBox.Show("Chưa chọn mặt hàng nào để hủy đơn hàng!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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
            // Mặc định chọn thanh toán bằng tiền mặt
            SelectedPaymentMethod = PaymentMethod.FirstOrDefault(p => p.Equals("Tiền mặt")) ?? PaymentMethod.First();
            // Mặc định không in bill
            IsCheckedPrintBill = false;
            // cập nhật discount sau khi reset
            LoadDiscountByCustomer();
            // Mặc định chọn không áp dụng mã giảm giá
            SelectedDiscount = AvailableDiscounts.FirstOrDefault(d => d.DiscountId == 0);
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
        private void SearchCustomer(object? parameter)
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
        public OrderCustomer AddCustomer(string customerName, string customerPhoneNumber, string? customerEmail)
        {
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
                        PhoneNumber = existingCustomer.PhoneNumber ?? "",
                        Email = existingCustomer.Email,
                        Point = existingCustomer.Point,
                        Tier = existingCustomer.Tier ?? ""
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

        #region PayOrder Methods
        private void PayOrderWindow (object param)
        {
            // Force commit edit trước khi refresh view hoặc mở window mới
            var view = CollectionViewSource.GetDefaultView(Orders);
            if (view is IEditableCollectionView editableView)
            {
                if (editableView.IsAddingNew)
                    editableView.CommitNew();

                if (editableView.IsEditingItem)
                    editableView.CommitEdit();
            }
            if (CanPayOrder)
            {
                PaymentWindow payWindow = new PaymentWindow(this);
                payWindow.Show();
            }
            else
            {
                MessageBox.Show("Chưa chọn mặt hàng nào để thanh toán!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void ConfirmPayOrder(object param)
        {
            if (GetInvalidOrderItems != null)
            {
                var invalidItems = GetInvalidOrderItems();
                if (invalidItems.Any())
                {
                    string names = string.Join(", ", invalidItems.Select(x => x.ItemName));
                    MessageBox.Show($"Cảnh báo: Món [{names}] vừa bị tắt.\n" + "Vui lòng xóa khỏi giỏ hàng trước khi thanh toán!",
                        "Món ăn không khả dụng", MessageBoxButton.OK);
                    return; // Dừng quá trình thanh toán
                }
            }
            var result = MessageBox.Show(
                    "Bạn có chắc muốn thanh toán đơn hàng này không?",
                    "Xác nhận thanh toán",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);
        }
        #endregion
    }
}