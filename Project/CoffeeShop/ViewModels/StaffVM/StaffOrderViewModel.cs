using CoffeeShop.Models;
using CoffeeShop.Service;
using CoffeeShop.View.Controls;
using CoffeeShop.View.Staff;
using CoffeeShop.ViewModels.AdminVM;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Data;
using static CoffeeShop.View.Controls.CustomMessageBox;

namespace CoffeeShop.ViewModels.StaffVM
{
    public partial class StaffOrderViewModel
    {
        private Staff _currentStaff;
        #region Constructor
        #region Constructor
        public StaffOrderViewModel(Staff staff = null!)
        {
            _currentStaff = staff;

            Items = new ObservableCollection<OrderItem>();
            Orders = new ObservableCollection<OrderDetailItem>();
            AvailableTables = new ObservableCollection<OrderTable>();
            FilteredItems = new ObservableCollection<OrderItem>();
            FilteredCustomers = new ObservableCollection<OrderCustomer>();
            Customers = new ObservableCollection<OrderCustomer>();

            InitializeCommands();
            Orders.CollectionChanged += Orders_CollectionChanged;

            WeakReferenceMessenger.Default.Register<ReloadMenuMessage>(this, (r, m) => {
                _ = LoadOrderItemsFromDB();
            });

            _ = InitializeApplicationAsync();
        }
        #endregion

        private async Task InitializeApplicationAsync()
        {
            IsLoading = true;

            await Task.Delay(50);

            try
            {
                await Task.Run(() => {
                    LoadCustomerFromDB();
                    LoadTableFromDB();
                    LoadDiscountFromDB();
                    LoadPaymentMethod();
                });

                await LoadOrderItemsFromDB();
                LoadAvailableTable();
                LoadAllCustomersToFiltered();
                FilterItemsByCategory();

                // Thiết lập mặc định
                SelectedCustomer = Customers.FirstOrDefault(c => c.CustomerId == 0);
                SelectedTable = AvailableTables.FirstOrDefault(t => t.TableId == 0);
                SelectedPaymentMethod = PaymentMethod.FirstOrDefault(p => p.Equals("Tiền mặt")) ?? "";
                SelectedDiscount = AvailableDiscounts.FirstOrDefault(d => d.DiscountId == 0);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi khởi tạo: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
        #endregion

        #region Command Initialization
        private void InitializeCommands()
        {
            AddItemCommand = new RelayCommand<object>(param =>
            {
                if (param is Tuple<OrderItem, string, string, decimal, int> data)
                    AddItemToOrder(data.Item1, data.Item2, data.Item3, data.Item4, data.Item5);
            });
            RemoveItemCommand = new RelayCommand<OrderDetailItem>(item => RemoveItemFromOrder(item));
            IncreaseQuantityCommand = new RelayCommand<OrderDetailItem>(IncreaseQuantity);
            DecreaseQuantityCommand = new RelayCommand<OrderDetailItem>(DecreaseQuantity);
            SearchCustomerCommand = new RelayCommand<object>(SearchCustomer);
            AddCustomerCommand = new RelayCommand<object>(param =>
            {
                var window = string.IsNullOrWhiteSpace(SearchCustomerKeyword)
                    ? new AddCustomerWindow(this)
                    : new AddCustomerWindow(this, SearchCustomerKeyword);

                window.ShowDialog();
                SearchCustomerKeyword = string.Empty;
                FilteredCustomers.Clear();
                OnPropertyChanged(nameof(HasSearchResults));
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
            CommitQuantityCommand = new RelayCommand<OrderDetailItem>(CommitQuantity);
        }
        #endregion

        private async Task LoadOrderItemsFromDB()
        {
            IsLoading = true;
            await Task.Delay(100);

            try
            {
                var dbItems = await Task.Run(() =>
                {
                    using (var context = new CoffeeShopContext())
                    {
                        return context.Items
                            .Include(i => i.ItemPrices).ThenInclude(ip => ip.Size)
                            .Where(i => i.IsDeleted == false)
                            .Where(i => i.IsAvailable == true)
                            .OrderByDescending(i => i.IsAvailable)
                            .ToList();
                    }
                });

                _items.Clear();
                FilteredItems.Clear();

                foreach (var item in dbItems)
                {
                    string displayImagePath;
                    string rawPath = item.ImagePath?.TrimStart('/', '\\').Replace('/', '\\') ?? "";
                    string absolutePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, rawPath);

                    if (!string.IsNullOrEmpty(rawPath) && File.Exists(absolutePath))
                    {
                        displayImagePath = absolutePath;
                    }
                    else if (!string.IsNullOrEmpty(rawPath) && !rawPath.Contains("imgItemExample.jpg"))
                    {
                        displayImagePath = "pack://application:,,,/" + rawPath.Replace('\\', '/');
                    }
                    else
                    {
                        displayImagePath = "pack://application:,,,/Assets/Images/imgItemExample.jpg";
                    }

                    var newItem = new OrderItem
                    {
                        ItemId = item.ItemId,
                        ItemName = item.ItemName,
                        CategoryId = item.CategoryId,
                        IsAvailable = item.IsAvailable,
                        ItemPrices = new ObservableCollection<ItemPrice>(item.ItemPrices.Where(ip => !ip.IsDeleted)),
                        Info = item.Info ?? string.Empty,
                        ImagePath = displayImagePath
                    };

                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        _items.Add(newItem);
                        FilteredItems.Add(newItem);
                    }, System.Windows.Threading.DispatcherPriority.Background);
                }
            }
            catch (Exception ex) { Debug.WriteLine(ex.Message); }
            finally
            {
                IsLoading = false;
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
                    var discounts = db.Discounts.Where(t => (t.IsActive == true && t.IsDeleted == false)).ToList();
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

        private void CommitQuantity(OrderDetailItem item)
        {
            if (item == null) return;

            // Kiểm tra quantity
            if (!int.TryParse(item.Quantity.ToString(), out int qty) || qty < 0)
            {
                CustomMessageBox.Show("Số lượng không hợp lệ. Vui lòng nhập số nguyên dương!", "Lỗi", MessageButtons.OK, MessageType.Error);
                return;
            }
            if (item.Quantity <= 0)
            {
                RemoveItemCommand?.Execute(item);
            }
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
        public void AddItemToOrder(OrderItem item, string? selectedSize, string? note, decimal price, int priceId)
        {
            if (item == null) return;
            selectedSize = selectedSize ?? string.Empty;
            note = string.IsNullOrEmpty(note) ? null : note.Trim();

            // Tìm xem món này với Size này đã có trong giỏ hàng chưa
            var existingItem = Orders.FirstOrDefault(i => i.PriceId == priceId && i.Note == note);

            if (existingItem != null)
            {
                existingItem.Quantity++;
            }
            else
            {
                var newItem = new OrderDetailItem
                {
                    ItemId = item.ItemId,
                    PriceId = priceId,
                    ItemName = item.ItemName,
                    SizeName = selectedSize,
                    Quantity = 1,
                    Price = price,
                    Note = note
                };
                newItem.NoteChangedCallback = MergeItemOnNoteChange;
                Orders.Add(newItem);
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
                var result = CustomMessageBox.Show(
                    "Bạn có chắc muốn hủy đơn này không?",
                    "Xác nhận hủy đơn", MessageButtons.YesNo, MessageType.Question);

                if (result == CustomMessageBox.MessageBoxResult.Yes)
                {
                    CancelOrder(param);
                }
            }
            else
                CustomMessageBox.Show("Chưa chọn mặt hàng nào để hủy đơn hàng!", "Lỗi", MessageButtons.OK, MessageType.Warning);
        }
        private void CancelOrder(object param)
        {
            Orders.Clear();
            CalculateTotalAmount();
            SelectedCustomer = null;
            SearchCustomerKeyword = string.Empty;
            FilteredCustomers?.Clear();
            OnPropertyChanged(nameof(HasSearchResults));

            // Mặc định chọn bàn có ID = 0 (mang về)
            SelectedTable = _availableTables.FirstOrDefault(t => t.TableId == 0);
            // Mặc định chọn khách hàng là vãng lai (ID = 0)
            SelectedCustomer = _customers.FirstOrDefault(c => c.CustomerId == 0);
            // Mặc định chọn thanh toán bằng tiền mặt
            SelectedPaymentMethod = PaymentMethod.FirstOrDefault(p => p.Equals("Tiền mặt")) ?? PaymentMethod.First();
            // Mặc định không in bill
            IsCheckedPrintBill = true;
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
                    (!string.IsNullOrEmpty(c.PhoneNumber) && c.PhoneNumber.Contains(SearchCustomerKeyword, StringComparison.OrdinalIgnoreCase))
                    || (!string.IsNullOrEmpty(c.CustomerName) && c.CustomerName.Contains(SearchCustomerKeyword, StringComparison.OrdinalIgnoreCase))
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
                    CustomMessageBox.Show("Khách hàng với số điện thoại này đã tồn tại.", "Lỗi", MessageButtons.OK, MessageType.Error);
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
                    JoinDate = DateTime.Now,
                    Point = 0,
                    Tier = "VIP1"
                };
                db.Customers.Add(newcustomer);
                db.SaveChanges();
                CustomMessageBox.Show("Thêm khách hàng thành công.", "Thành công", MessageButtons.OK, MessageType.Success);
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
                if (CustomMessageBox.Show($"Bạn đang chọn {SelectedTable.TableName}. Đổi sang {table.TableName}?", "Xác nhận đổi bàn",
                             MessageButtons.YesNo, MessageType.Info) == CustomMessageBox.MessageBoxResult.No)
                    return;
            }
            SelectedTable = table;
        }
        #endregion

        #region PayOrder Methods
        PaymentWindow payWindow;
        private void PayOrderWindow(object param)
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
                payWindow = new PaymentWindow(this);
                payWindow.Show();
            }
            else
            {
                CustomMessageBox.Show("Chưa chọn mặt hàng nào để thanh toán!", "Lỗi", MessageButtons.OK, MessageType.Warning);
            }
        }
        private async void ConfirmPayOrder(object param)
        {
            if (GetInvalidOrderItems != null)
            {
                var invalidItems = GetInvalidOrderItems();
                if (invalidItems.Any())
                {
                    string names = string.Join(", ", invalidItems.Select(x => x.ItemName));
                    CustomMessageBox.Show($"Cảnh báo: Món [{names}] vừa bị tắt.\n" + "Vui lòng xóa khỏi giỏ hàng trước khi thanh toán!", "Món ăn không khả dụng",
                        MessageButtons.OK, MessageType.Warning);
                    return; // Dừng quá trình thanh toán
                }
            }
            // Xác nhận thanh toán đơn hàng
            var confirm = CustomMessageBox.Show("Xác nhận thanh toán đơn hàng?", "Xác nhận",
                MessageButtons.YesNo, MessageType.Info);
            if (confirm != CustomMessageBox.MessageBoxResult.Yes) return;

            IsLoading = true;
            try
            {
                await Task.Run(async () =>
                {
                    using (var db = new CoffeeShopContext())
                    {
                        using (var transaction = db.Database.BeginTransaction())
                        {
                            try
                            {
                                var newOrder = new Order
                                {
                                    TableId = SelectedTable?.TableId != 0 ? SelectedTable?.TableId : null,
                                    CustomerId = SelectedCustomer?.CustomerId != 0 ? SelectedCustomer?.CustomerId : null,
                                    StaffId = _currentStaff.StaffId,
                                    OrderDate = DateTime.Now,
                                    SubTotal = TotalAmount,
                                    DiscountId = SelectedDiscount?.DiscountId != 0 ? SelectedDiscount?.DiscountId : null,
                                    DiscountMoney = FinalDiscount,
                                    TotalAmount = FinalTotal,
                                    PaymentMethod = SelectedPaymentMethod ?? "Tiền mặt"
                                };

                                db.Orders.Add(newOrder);
                                db.SaveChanges();

                                foreach (var item in Orders)
                                {
                                    var detail = new OrderDetail
                                    {
                                        OrderId = newOrder.OrderId,
                                        PriceId = item.PriceId,
                                        Quantity = item.Quantity,
                                        UnitPrice = item.Price,
                                        TotalPrice = item.TotalPrice,
                                        Note = item.Note
                                    };
                                    db.OrderDetails.Add(detail);
                                }

                                // Cập nhật bàn đang sử dụng
                                if (newOrder.TableId != null)
                                {
                                    var table = db.CafeTables.Find(newOrder.TableId);
                                    if (table != null)
                                    {
                                        table.TableStatus = 1;
                                    }
                                }

                                // Cập nhật lượt dùng ddiiss cao
                                if (newOrder.DiscountId != null)
                                {
                                    var disc = db.Discounts.Find(newOrder.DiscountId);
                                    if (disc != null) disc.UsedCount += 1;
                                }

                                // Cập nhật điểm cho khách hàng, cứ mỗi 2k giá trị đơn <=> 1 điểm
                                if (newOrder.CustomerId != null)
                                {
                                    var customer = db.Customers.Find(newOrder.CustomerId);
                                    if (customer != null) customer.Point += (int)(newOrder.TotalAmount / 2000);
                                    switch (customer!.Point)
                                    {
                                        case int points when points >= 3000:
                                            customer.Tier = "VIP100";
                                            break;
                                        case int points when points >= 1500:
                                            customer.Tier = "VIP10";
                                            break;
                                        case int points when points >= 500:
                                            customer.Tier = "VIP1";
                                            break;
                                        default:
                                            customer.Tier = "MEMBER";
                                            break;
                                    }
                                }

                                db.SaveChanges();
                                transaction.Commit();
                                EventAggregator.Instance.Publish(new OrderCompletedMessage { TableId = newOrder.TableId });
                                /// Xử lý in hóa đơn nếu được chọn
                                if (IsCheckedPrintBill)
                                {
                                    // Khởi tạo hóa đơn
                                    string fileName = $"Bill_{newOrder.OrderId}_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                                    string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Exports");
                                    if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                                    string fullPath = Path.Combine(folderPath, fileName);

                                    var exporter = new BillExporter(newOrder.OrderId);
                                    await exporter.ExportToExcel(fullPath);

                                    // Tự động mở file Excel
                                    Process.Start(new ProcessStartInfo(fullPath) { UseShellExecute = true });
                                }
                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback();
                                throw new Exception("Lỗi dữ liệu: " + ex.Message);
                            }
                        }
                    }
                });

                CustomMessageBox.Show("Thanh toán và lưu hóa đơn thành công!", "Thành công", MessageButtons.OK, MessageType.Success);


                // Reset giao diện
                CancelOrder(null);
                SearchCustomerKeyword = string.Empty;
                FilteredCustomers.Clear();
                OnPropertyChanged(nameof(HasSearchResults));
                // Đóng cửa sổ thanh toán
                payWindow.Close();
                //
                SelectedCustomer = Customers.FirstOrDefault(c => c.CustomerId == 0);
                SelectedTable = AvailableTables.FirstOrDefault(t => t.TableId == 0);
                SelectedDiscount = AvailableDiscounts.FirstOrDefault(d => d.DiscountId == 0);
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Lỗi thanh toán: {ex.Message}", "Lỗi", MessageButtons.OK, MessageType.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }
        #endregion
    }
}