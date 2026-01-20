using CoffeeShop.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CoffeeShop.ViewModels.StaffVM
{
    public partial class StaffOrderViewModel : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string? propertyName = null)
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
        // List món trong đơn hàng bị lỗi (không còn trong Menu hoặc không còn phục vụ)
        public List<OrderDetailItem> GetInvalidOrderItems()
        {
            using (var context = new CoffeeShopContext())
            {
                var orderItemIds = Orders.Select(o => o.ItemId).Distinct().ToList();
                var disabledIds = context.Items
                    .Where(i => orderItemIds.Contains(i.ItemId) && (i.IsAvailable == false || i.IsDeleted == true))
                    .Select(i => i.ItemId)
                    .ToList();

                // Trả về những món trong Orders có ItemId nằm trong danh sách bị tắt
                return Orders.Where(o => disabledIds.Contains(o.ItemId)).ToList();
            }
        }
        // Tab được chọn
        private TabItem? _selectedTabItem;
        public TabItem? SelectedTabItem
        {
            get => _selectedTabItem;
            set
            {
                _selectedTabItem = value;
                OnPropertyChanged();

                // Lấy CategoryId từ Tag của TabItem
                if (value?.Tag is string tag && int.TryParse(tag, out int id))
                {
                    CurrentCategoryId = id;
                }
            }
        }
        private ObservableCollection<OrderItem> _filteredItems = new ObservableCollection<OrderItem>();
        public ObservableCollection<OrderItem> FilteredItems
        {
            get => _filteredItems;
            set
            {
                _filteredItems = value;
                OnPropertyChanged(nameof(FilteredItems));
            }
        }

        // Category hiện tại
        private int _currentCategoryId;
        public int CurrentCategoryId
        {
            get => _currentCategoryId;
            set
            {
                _currentCategoryId = value;
                OnPropertyChanged();
                FilterItemsByCategory();
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
        // List khách hàng tìm kiếm theo keyword
        private ObservableCollection<OrderCustomer> _filteredCustomers = new ObservableCollection<OrderCustomer>();
        public ObservableCollection<OrderCustomer> FilteredCustomers
        {
            get => _filteredCustomers;
            set
            {
                _filteredCustomers = value;
                OnPropertyChanged(nameof(FilteredCustomers));
                OnPropertyChanged(nameof(HasSearchResults));
            }
        }
        // Kiểm tra có kết quả tìm kiếm khách hàng hay không
        public bool HasSearchResults => FilteredCustomers != null && FilteredCustomers.Count > 0;
        // Khách hàng được chọn
        private OrderCustomer? _selectedCustomer;
        public OrderCustomer? SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                _selectedCustomer = value;
                OnPropertyChanged(nameof(SelectedCustomer));
                LoadDiscountByCustomer();
                OnPropertyChanged(nameof(AvailableDiscounts));
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
        private OrderTable? _selectedTable = null; // Mặc định là không chọn bàn
        public OrderTable? SelectedTable
        {
            get { return _selectedTable; }
            set
            {
                _selectedTable = value;
                OnPropertyChanged(nameof(SelectedTable));
                OnPropertyChanged(nameof(SelectedTableDisplay)); // Hiển thị text
            }
        }

        // Property hiển thị text cho bàn trống
        public string SelectedTableDisplay => SelectedTable?.TableName ?? "Không";

        // Tìm kiếm món trong MenuPanel
        private string? _searchItemKeyword;
        public string? SearchItemKeyword
        {
            get { return _searchItemKeyword; }
            set
            {
                _searchItemKeyword = value;
                OnPropertyChanged(nameof(SearchItemKeyword));
                SearchItems();
            }
        }

        // Tìm kiếm khách hàng
        private string _seachCustomerKeyword = "";
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

        // Tổng tiền đơn hàng
        private decimal _totalAmount;
        public decimal TotalAmount
        {
            get => _totalAmount;
            set
            {
                _totalAmount = value;
                OnPropertyChanged(nameof(TotalAmount));
                CalculateFinalTotal();
            }
        }

        // Giảm giá
        private ObservableCollection<OrderDiscount> _discounts = new ObservableCollection<OrderDiscount>();
        public ObservableCollection<OrderDiscount> Discounts
        {
            get { return _discounts; }
            set
            {
                _discounts = value;
                OnPropertyChanged(nameof(Discounts));
            }
        }

        // List mã giảm giá hợp lệ cho selected customer
        private ObservableCollection<OrderDiscount> _availableDiscounts = new ObservableCollection<OrderDiscount>();
        public ObservableCollection<OrderDiscount> AvailableDiscounts
        {
            get => _availableDiscounts;
            set
            {
                _availableDiscounts = value;
                OnPropertyChanged(nameof(AvailableDiscounts));
            }
        }
        // Giảm giá được áp dụng cho order
        private OrderDiscount? _selectedDiscount;
        public OrderDiscount? SelectedDiscount
        {
            get => _selectedDiscount;
            set
            {
                _selectedDiscount = value;
                OnPropertyChanged(nameof(SelectedDiscount));
                CalculateFinalTotal();
            }
        }
        // Số tiền giảm giá thực tế
        private decimal _finalDiscount = 0;
        public decimal FinalDiscount
        {
            get => _finalDiscount;
            set
            {
                _finalDiscount = value;
                OnPropertyChanged(nameof(FinalDiscount));
            }
        }
        // Tổng tiền cuối cùng (sau khi áp dụng giảm giá)
        private decimal _finalTotal;
        public decimal FinalTotal
        {
            get => _finalTotal;
            set
            {
                _finalTotal = value;
                OnPropertyChanged(nameof(FinalTotal));
            }
        }
        public bool CanCancelOrder => Orders.Count > 0;
        public bool CanPayOrder => Orders.Count > 0 && SelectedCustomer != null && SelectedTable != null;

        private List<string> _paymentMethod = new List<string>(); // Tiền mặt - Chuyển khoản
        public List<string> PaymentMethod
        {
            get => _paymentMethod;
            set
            {
                _paymentMethod = value;
                OnPropertyChanged(nameof(PaymentMethod));
            }
        }
        private string _selectedPaymentMethod = "";
        public string SelectedPaymentMethod
        {
            get => _selectedPaymentMethod;
            set
            {
                _selectedPaymentMethod = value;
                OnPropertyChanged(SelectedPaymentMethod);
            }
        }

        // Yêu cầu in Bill
        private bool _isCheckedPrintBill = true;
        public bool IsCheckedPrintBill
        {
            get => _isCheckedPrintBill;
            set
            {
                _isCheckedPrintBill = value;
                OnPropertyChanged(nameof(IsCheckedPrintBill));
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }
        #endregion

        #region Commands
        public ICommand AddItemCommand { get; set; } = null!;
        public ICommand RemoveItemCommand { get; set; } = null!;
        public ICommand IncreaseQuantityCommand { get; set; } = null!;
        public ICommand DecreaseQuantityCommand { get; set; } = null!;
        public ICommand SearchCustomerCommand { get; set; } = null!;
        public ICommand AddCustomerCommand { get; set; } = null!;
        public ICommand ChooseCustomerCommand { get; set; } = null!;
        public ICommand ChooseTableCommand { get; set; } = null!;
        public ICommand CancelOrderCommand { get; set; } = null!;
        public ICommand PayOrderCommand { get; set; } = null!;
        public ICommand ChoosePaymentMethodCommand { get; set; } = null!;
        public ICommand ConfirmPayOrderCommand { get; set; } = null!;
        public ICommand ChooseDiscountCommand { get; set; } = null!;
        public ICommand CommitQuantityCommand { get; set; } = null!;
        #endregion

        #region Helper Classes
        public class OrderItem : NotificationBase
        {
            private int _itemId;
            private string _itemName;
            private int _categoryId;
            private int _quantity;
            private bool _isAvailable;
            private ObservableCollection<ItemPrice> _itemPrices;
            private string _imagePath;
            private string _info;

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
            public string Info
            {
                get => _info;
                set { _info = value; OnPropertyChanged(nameof(Info)); }
            }

            public OrderItem()
            {
                _itemName = string.Empty;
                _itemPrices = new ObservableCollection<ItemPrice>();
            }
        }

        public class OrderCustomer : NotificationBase
        {
            private int _customerId;
            private string _customerName = string.Empty;
            private string _phoneNumber = string.Empty;
            private string? _email;
            private int _point;
            private string _tier = string.Empty;

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

            public string? Email
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
            private string? _note;

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

            public string? Note
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
            private string? _note;
            public Action<OrderDetailItem>? NoteChangedCallback { get; set; }
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
                    OnPropertyChanged(nameof(Quantity));
                    OnPropertyChanged(nameof(TotalPrice));
                }
            }

            public decimal Price
            {
                get => _price;
                set
                {
                    _price = value;
                    OnPropertyChanged(nameof(Price));
                    OnPropertyChanged(nameof(TotalPrice));
                }
            }

            public decimal TotalPrice
            {
                get
                {
                    return _quantity * _price;
                }
            }

            public string? Note
            {
                get => _note;
                set 
                { 
                    _note = value; 
                    OnPropertyChanged(nameof(Note));
                }
            }

            private int _priceId;
            public int PriceId
            {
                get => _priceId;
                set { _priceId = value; OnPropertyChanged(nameof(PriceId)); }
            }
        }

        public class NotificationBase : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler? PropertyChanged;

            protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public class OrderDiscount
        {
            public int DiscountId { get; set; }

            public string DiscountCode { get; set; } = null!;

            public string DiscountName { get; set; } = null!;

            public int DiscountType { get; set; }

            public decimal DiscountValue { get; set; }

            public decimal? MinimumOrderValue { get; set; }

            public decimal? MaximumDiscountAmount { get; set; }

            public bool IsActive { get; set; }

            public int? UsedCount { get; set; }
            // Property mới để check điều kiện
            public bool IsEligible { get; set; } = true;
        }
        #endregion
    }
}