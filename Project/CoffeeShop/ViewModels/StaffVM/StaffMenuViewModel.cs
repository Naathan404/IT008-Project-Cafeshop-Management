using CoffeeShop.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;
using CommunityToolkit.Mvvm.Messaging;
using static CoffeeShop.ViewModels.StaffVM.StaffOrderViewModel;

namespace CoffeeShop.ViewModels.StaffVM
{
    public partial class StaffMenuViewModel : INotifyPropertyChanged
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
        private ObservableCollection<MenuCoffeeItem> _items = new ObservableCollection<MenuCoffeeItem>();
        public ObservableCollection<MenuCoffeeItem> Items
        {
            get { return _items; }
            set
            {
                _items = value;
                OnPropertyChanged(nameof(Items));
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
        private ObservableCollection<MenuCoffeeItem> _filteredItems = new ObservableCollection<MenuCoffeeItem>();
        public ObservableCollection<MenuCoffeeItem> FilteredItems
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

        // Item đang chọn để hiển thị chi tiết
        private MenuCoffeeItem? _selectedItem;
        public MenuCoffeeItem? SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                OnPropertyChanged(nameof(SelectedItem));
                OnPropertyChanged(nameof(TextButtonSetAvailable));
                LoadAvailableSizes();
            }
        }
        public string TextButtonSetAvailable
        {
            get
            {
                if (SelectedItem == null)
                    return string.Empty;

                return SelectedItem.IsAvailable ? "Tắt món" : "Hủy tắt món";
            }
        }
        private string? _selectedSize;
        public string? SelectedSize
        {
            get => _selectedSize;
            set
            {
                if (_selectedSize != value)
                {
                    _selectedSize = value;
                    OnPropertyChanged(nameof(SelectedSize));
                    UpdateSelectedPrice();
                }
            }
        }

        private decimal _selectedPrice;
        public decimal SelectedPrice
        {
            get => _selectedPrice;
            set
            {
                if (_selectedPrice != value)
                {
                    _selectedPrice = value;
                    OnPropertyChanged(nameof(SelectedPrice));
                    OnPropertyChanged(nameof(SelectedPriceFormatted));
                }
            }
        }

        public string SelectedPriceFormatted => $"{SelectedPrice:N0} VND";

        // Danh sách size có sẵn cho item hiện tại
        private ObservableCollection<SizeViewModel> _availableSizes = new();
        public ObservableCollection<SizeViewModel> AvailableSizes
        {
            get => _availableSizes;
            set
            {
                _availableSizes = value;
                OnPropertyChanged(nameof(AvailableSizes));
            }
        }

        #endregion

        #region Commands
        public ICommand SetIsAvailableForItem { get; set; } = null!;
        // Command để chọn size
        public ICommand SelectSizeCommand { get; set; } = null!;
        #endregion

        #region Constructor
        public StaffMenuViewModel()
        {
            Items = new ObservableCollection<MenuCoffeeItem>();
            FilteredItems = new ObservableCollection<MenuCoffeeItem>();
            InitializeCommands();
            LoadData();

            // Mặc định chọn item đầu tiên
            if (Items.Count > 0)
            {
                SelectedItem = Items.First();
            }
        }
        #endregion

        #region Command Initialization
        private void InitializeCommands()
        {
            SetIsAvailableForItem = new RelayCommand<MenuCoffeeItem>(SetIsavailable);
            SelectSizeCommand = new RelayCommand<SizeViewModel>(size =>
            {
                if (size == null) return;

                // Reset tất cả sizes
                foreach (var s in AvailableSizes)
                {
                    s.IsSelected = false;
                }

                // Set size được chọn
                size.IsSelected = true;
                SelectedSize = size.SizeName;
            });
        }
        #endregion

        #region Load Data Methods
        private void LoadData()
        {
            LoadOrderItemsFromDB();
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
                    var items = context.Items
                        .Where(i => i.IsDeleted == false)
                        .OrderByDescending(i => i.IsAvailable)
                        .ToList();

                    foreach (var item in items)
                    {
                        string displayImagePath;

                        // Xử lý ảnh mặc định
                        if (string.IsNullOrEmpty(item.ImagePath) ||
                            item.ImagePath.Contains("imgItemExample.jpg") ||
                            item.ImagePath == "Assets/Images/imgItemExample.jpg" ||
                            item.ImagePath == "Assets\\Images\\imgItemExample.jpg")
                        {
                            // Dùng Pack URI cho ảnh mặc định
                            displayImagePath = "/Assets/Images/imgItemExample.jpg";
                        }
                        else
                        {
                            // Convert đường dẫn tương đối sang tuyệt đối cho ảnh user upload
                            displayImagePath = Path.Combine(
                                AppDomain.CurrentDomain.BaseDirectory,
                                item.ImagePath
                            );
                        }

                        _items.Add(new MenuCoffeeItem
                        {
                            ItemId = item.ItemId,
                            ItemName = item.ItemName,
                            CategoryId = item.CategoryId,
                            IsAvailable = item.IsAvailable,
                            ItemPrices = new ObservableCollection<ItemPrice>(
                                context.ItemPrices
                                    .Include(ip => ip.Size)
                                    .Where(ip => ip.ItemId == item.ItemId && ip.IsDeleted == false)
                                    .ToList()
                            ),
                            Info = item.Info ?? string.Empty,
                            ImagePath = displayImagePath
                        });
                    }
                }

                FilteredItems = new ObservableCollection<MenuCoffeeItem>(_items);
                OnPropertyChanged(nameof(FilteredItems));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading items: {ex.Message}");
            }
        }

        private void FilterItemsByCategory()
        {
            SearchItems();
        }
        #endregion

        #region Search Methods
        // Tìm kiếm món trong MenuPanel
        private void SearchItems()
        {
            IEnumerable<MenuCoffeeItem> source;

            // Tab "Tất cả"
            if (CurrentCategoryId == 0)
                source = Items;
            else
                source = Items.Where(i => i.CategoryId == CurrentCategoryId);

            // Không có keyword --> trả danh sách theo tab
            if (string.IsNullOrWhiteSpace(SearchItemKeyword))
            {
                FilteredItems = new ObservableCollection<MenuCoffeeItem>(source);
                return;
            }

            var keyword = SearchItemKeyword.Trim();

            var result = source
                .Where(i => i.ItemName.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();

            FilteredItems = new ObservableCollection<MenuCoffeeItem>(result);
        }
        #endregion

        #region Set IsAvailable for Item
        private void SetIsavailable(MenuCoffeeItem thisItem)
        {
            if (thisItem == null) return;
            string txt = thisItem.IsAvailable ? "Bạn có chắc muốn tắt món này?" : "Bạn có chắc muốn hủy tắt món này?";

            if (MessageBox.Show(txt, "Thông báo", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                // Đảo trạng thái
                thisItem.IsAvailable = !thisItem.IsAvailable;

                OnPropertyChanged(nameof(TextButtonSetAvailable));
                using (var db = new CoffeeShopContext())
                {
                    var item = db.Items.FirstOrDefault(i => i.ItemId == thisItem.ItemId);
                    if (item != null)
                    {
                        item.IsAvailable = thisItem.IsAvailable;
                        db.SaveChanges(); // Lưu vô DB
                        LoadData(); // reload lại dữ liệu ở page Menu

                        // Gửi tin nhắn thông báo cho page order để reload lại dữ liệu
                        WeakReferenceMessenger.Default.Send(new ReloadMenuMessage());
                    }
                }
            }
        }
        #endregion

        #region Selected size & price
        private void LoadAvailableSizes()
        {
            AvailableSizes.Clear();
            SelectedSize = null;
            SelectedPrice = 0;

            if (SelectedItem?.ItemPrices == null || SelectedItem.ItemPrices.Count == 0)
                return;

            // Nếu chỉ có 1 size và là category Food (7) thì không hiển thị size
            if (SelectedItem.CategoryId == 7)
            {
                var firstPrice = SelectedItem.ItemPrices.FirstOrDefault();
                if (firstPrice != null)
                {
                    SelectedSize = firstPrice.Size?.SizeName;
                    SelectedPrice = firstPrice.Price;
                    return;
                }
            }

            var sizeList = SelectedItem.ItemPrices
                .Where(p => p.Size != null && !string.IsNullOrEmpty(p.Size.SizeName))
                .Select(p => new SizeViewModel
                {
                    SizeName = p.Size!.SizeName!,
                    Price = p.Price,
                    IsSelected = false
                })
                .ToList();

            // Load sizes vào collection
            foreach (var size in sizeList)
            {
                AvailableSizes.Add(size);
            }

            // Tự động chọn size đầu tiên
            if (AvailableSizes.Count > 0)
            {
                AvailableSizes[0].IsSelected = true;
                SelectedSize = AvailableSizes[0].SizeName;
                SelectedPrice = AvailableSizes[0].Price;
            }
        }

        private void UpdateSelectedPrice()
        {
            if (SelectedItem?.ItemPrices == null || string.IsNullOrEmpty(SelectedSize))
            {
                SelectedPrice = 0;
                return;
            }

            var priceInfo = SelectedItem.ItemPrices.FirstOrDefault(p =>
                p?.Size?.SizeName != null &&
                p.Size.SizeName.Equals(SelectedSize, StringComparison.OrdinalIgnoreCase));

            SelectedPrice = priceInfo?.Price ?? 0;
        }
        #endregion

        #region Helper Classes
        public class MenuCoffeeItem : NotificationBase
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


            public MenuCoffeeItem()
            {
                _itemName = string.Empty;
                _itemPrices = new ObservableCollection<ItemPrice>();
            }
        }
        // Model cho Size
        public class SizeViewModel : INotifyPropertyChanged
        {
            private string _sizeName = string.Empty;
            public string SizeName
            {
                get => _sizeName;
                set
                {
                    _sizeName = value;
                    OnPropertyChanged(nameof(SizeName));
                }
            }

            private decimal _price;
            public decimal Price
            {
                get => _price;
                set
                {
                    _price = value;
                    OnPropertyChanged(nameof(Price));
                }
            }

            private bool _isSelected;
            public bool IsSelected
            {
                get => _isSelected;
                set
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }

            public event PropertyChangedEventHandler? PropertyChanged;
            protected virtual void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}
