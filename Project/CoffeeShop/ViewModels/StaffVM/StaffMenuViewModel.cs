using CoffeeShop.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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


        #endregion

        #region Commands
        public ICommand SetIsAvailableForItem { get; set; } = null!;
        #endregion

        #region Constructor
        public StaffMenuViewModel()
        {
            Items = new ObservableCollection<MenuCoffeeItem>();
            FilteredItems = new ObservableCollection<MenuCoffeeItem>();
            InitializeCommands();
            LoadData();

            // Mặc định chọn item đầu tiên
            SelectedItem = Items.First();
        }
        #endregion

        #region Command Initialization
        private void InitializeCommands()
        {
            SetIsAvailableForItem = new RelayCommand<MenuCoffeeItem>(SetIsavailable);
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
                    var items = context.Items.Where(i => i.IsDeleted == false).OrderByDescending(i => i.IsAvailable).ToList();
                    foreach (var item in items)
                    {
                        _items.Add(new MenuCoffeeItem
                        {
                            ItemId = item.ItemId,
                            ItemName = item.ItemName,
                            CategoryId = item.CategoryId,
                            IsAvailable = item.IsAvailable,
                            ItemPrices = new ObservableCollection<ItemPrice>(context.ItemPrices
                                                .Include(ip => ip.Size)
                                                .Where(ip => ip.ItemId == item.ItemId && ip.IsDeleted == false)
                                                .ToList()),
                        });
                    }
                }
                // Sau khi load, filtered = toàn bộ danh sách
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
                        LoadData();
                    }
                }
            }
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

            public MenuCoffeeItem()
            {
                _itemName = string.Empty;
                _itemPrices = new ObservableCollection<ItemPrice>();
                _imagePath = "/Assets/Images/imgItemExample.jpg"; // Ví dụ hình ảnh
            }
        }
        #endregion
    }
}
