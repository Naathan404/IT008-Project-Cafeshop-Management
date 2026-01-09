using CoffeeShop.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace CoffeeShop.ViewModels.AdminVM
{
    public class AdminMenuViewModel : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Properties
        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(nameof(IsLoading)); }
        }

        private string _loadingMessage = "Loading...";
        public string LoadingMessage
        {
            get => _loadingMessage;
            set { _loadingMessage = value; OnPropertyChanged(nameof(LoadingMessage)); }
        }

        private ObservableCollection<MenuCoffeeItem> _items = new ObservableCollection<MenuCoffeeItem>();
        public ObservableCollection<MenuCoffeeItem> Items
        {
            get => _items;
            set { _items = value; OnPropertyChanged(nameof(Items)); }
        }

        private TabItem? _selectedTabItem;
        public TabItem? SelectedTabItem
        {
            get => _selectedTabItem;
            set
            {
                _selectedTabItem = value;
                OnPropertyChanged(nameof(SelectedTabItem));
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
            set { _filteredItems = value; OnPropertyChanged(nameof(FilteredItems)); }
        }

        private int _currentCategoryId;
        public int CurrentCategoryId
        {
            get => _currentCategoryId;
            set
            {
                _currentCategoryId = value;
                OnPropertyChanged(nameof(CurrentCategoryId));
                FilterItemsByCategory();
            }
        }

        private string? _searchItemKeyword;
        public string? SearchItemKeyword
        {
            get => _searchItemKeyword;
            set
            {
                _searchItemKeyword = value;
                OnPropertyChanged(nameof(SearchItemKeyword));
                SearchItems();
            }
        }

        private MenuCoffeeItem? _selectedItem;
        public MenuCoffeeItem? SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                OnPropertyChanged(nameof(SelectedItem));
                LoadAvailableSizes();
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

        private ObservableCollection<SizeViewModel> _availableSizes = new();
        public ObservableCollection<SizeViewModel> AvailableSizes
        {
            get => _availableSizes;
            set { _availableSizes = value; OnPropertyChanged(nameof(AvailableSizes)); }
        }

        private string? _selectedImagePath;
        public string? SelectedImagePath
        {
            get => _selectedImagePath;
            set { _selectedImagePath = value; OnPropertyChanged(nameof(SelectedImagePath)); }
        }

        private bool? _dialogResult;
        public bool? DialogResult
        {
            get => _dialogResult;
            set
            {
                _dialogResult = value;
                OnPropertyChanged(nameof(DialogResult));
            }
        }
        private BitmapImage? _image;
        public BitmapImage? Image
        {
            get => _image;
            set { _image = value; OnPropertyChanged(nameof(Image)); }
        }

        private BitmapSource? _tempCroppedBitmap; // Lưu ảnh đã cắt nhưng chưa lưu file

        private string? _imagePath;
        public string? ImagePath
        {
            get => _imagePath;
            set
            {
                _imagePath = value;
                OnPropertyChanged(nameof(ImagePath));
                LoadImage();
            }
        }

        #endregion

        #region Commands
        public ICommand AddItemCommand { get; private set; } = null!;
        public ICommand EditItemCommand { get; private set; } = null!;
        public ICommand DeleteItemCommand { get; private set; } = null!;
        public ICommand UploadImageCommand { get; private set; } = null!;
        #endregion

        #region Constructor & Destructor
        public AdminMenuViewModel()
        {
            Items = new ObservableCollection<MenuCoffeeItem>();
            FilteredItems = new ObservableCollection<MenuCoffeeItem>();
            InitializeCommands();
            LoadData();
            SubscribeToMessages();
        }
        public AdminMenuViewModel(int itemId)
        {
            InitializeViewModel();
            LoadSelectedItem(itemId); // Chỉ load món được chọn cho màn hình Edit
        }

        private void InitializeViewModel()
        {
            Items = new ObservableCollection<MenuCoffeeItem>();
            FilteredItems = new ObservableCollection<MenuCoffeeItem>();
            AvailableSizes = new ObservableCollection<SizeViewModel>(); // Khởi tạo danh sách size
            InitializeCommands();    // Khởi tạo các nút bấm (Add, Edit, Delete)
            SubscribeToMessages();   // Đăng ký nhận thông báo thay đổi
        }

        ~AdminMenuViewModel()
        {
            UnsubscribeFromMessages();
        }
        #endregion

        #region Command Initialization
        private void InitializeCommands()
        {
            AddItemCommand = new RelayCommand<object>(async p => await AddItemAsync(), p => !IsLoading);
            EditItemCommand = new RelayCommand<object>(async p => await EditItemAsync(), p => CanExecuteEditOrDeleteItem(p) && !IsLoading);
            DeleteItemCommand = new RelayCommand<object>(async p => await DeleteItemAsync(), p => CanExecuteEditOrDeleteItem(p) && !IsLoading);
            UploadImageCommand = new RelayCommand<object>(async p => await UploadImageAsync(), p => !IsLoading);
        }

        private bool CanExecuteEditOrDeleteItem(object? parameter) => SelectedItem != null;
        #endregion

        #region Event Aggregator Methods
        private void SubscribeToMessages()
        {
            EventAggregator.Instance.Subscribe<ItemsChangedMessage>(OnItemsChanged);
        }

        private void UnsubscribeFromMessages()
        {
            EventAggregator.Instance.Unsubscribe<ItemsChangedMessage>(OnItemsChanged);
        }

        private void OnItemsChanged(ItemsChangedMessage message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var currentSelectedId = SelectedItem?.ItemId;

                LoadOrderItemsFromDB();
                FilterItemsByCategory();

                // Giữ lại selection và refresh sizes + image
                if (currentSelectedId.HasValue)
                {
                    var updatedItem = FilteredItems.FirstOrDefault(i => i.ItemId == currentSelectedId.Value);
                    if (updatedItem != null)
                    {
                        SelectedItem = updatedItem;

                        // Force reload image
                        if (!string.IsNullOrEmpty(updatedItem.ImagePath))
                        {
                            ImagePath = updatedItem.ImagePath;
                        }
                    }
                    else
                    {
                        SelectedItem = null;
                    }
                }
                else
                {
                    SelectedItem = null;
                }
            });
        }

        #endregion

        #region Load, Search, Edit, Delete Items
        private void LoadData()
        {
            LoadOrderItemsFromDB();
            FilterItemsByCategory();
        }

        private void LoadSelectedItem(int itemId)
        {
            try
            {
                using (var context = new CoffeeShopContext())
                {
                    // Lấy thông tin cơ bản của Item
                    var item = context.Items
                        .FirstOrDefault(i => i.ItemId == itemId && !i.IsDeleted);

                    if (item != null)
                    {
                        // Chuyển đổi sang MenuCoffeeItem
                        var editItem = new MenuCoffeeItem
                        {
                            ItemId = item.ItemId,
                            ItemName = item.ItemName,
                            CategoryId = item.CategoryId,
                            IsAvailable = item.IsAvailable,
                            Info = item.Info ?? string.Empty,
                            ImagePath = item.ImagePath ?? string.Empty,
                            // Load danh sách giá kèm theo Size
                            ItemPrices = new ObservableCollection<ItemPrice>(
                                context.ItemPrices
                                    .Include(ip => ip.Size)
                                    .Where(ip => ip.ItemId == itemId && !ip.IsDeleted)
                                    .ToList()
                            )
                        };

                        // Gán vào SelectedItem để giao diện Binding dữ liệu lên các TextBox/Image
                        SelectedItem = editItem;
                        this.ImagePath = item.ImagePath;

                        // Load các Size hiện có của món này vào ComboBox/List
                        LoadAvailableSizes();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải thông tin món: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

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

                        // Nếu là ảnh mặc định, giữ nguyên dạng Pack URI
                        if (string.IsNullOrEmpty(item.ImagePath) ||
                            item.ImagePath.Contains("imgItemExample.jpg") ||
                            item.ImagePath == "Assets/Images/imgItemExample.jpg" ||
                            item.ImagePath == "Assets\\Images\\imgItemExample.jpg")
                        {
                            displayImagePath = "/Assets/Images/imgItemExample.jpg";
                        }
                        // Ảnh thật từ user upload
                        else
                        {
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
                MessageBox.Show($"Error loading items: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadImage()
        {
            if (string.IsNullOrWhiteSpace(ImagePath))
            {
                Image = null;
                return;
            }

            try
            {
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.CreateOptions = BitmapCreateOptions.IgnoreImageCache;

                // Nếu là Pack URI (ảnh mặc định)
                if (ImagePath.StartsWith("/Assets") || ImagePath.StartsWith("pack://"))
                {
                    bmp.UriSource = new Uri("pack://application:,,,/Assets/Images/imgItemExample.jpg", UriKind.Absolute);
                }
                // Nếu là đường dẫn tuyệt đối
                else if (Path.IsPathRooted(ImagePath))
                {
                    if (!File.Exists(ImagePath))
                    {
                        Image = null;
                        return;
                    }
                    bmp.UriSource = new Uri(ImagePath, UriKind.Absolute);
                }
                // Nếu là đường dẫn tương đối
                else
                {
                    string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ImagePath);
                    if (!File.Exists(fullPath))
                    {
                        Image = null;
                        return;
                    }
                    bmp.UriSource = new Uri(fullPath, UriKind.Absolute);
                }

                bmp.EndInit();
                bmp.Freeze();
                Image = bmp;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi load ảnh: {ex.Message}");
                Image = null;
            }
        }


        private void FilterItemsByCategory() => SearchItems();

        private void SearchItems()
        {
            IEnumerable<MenuCoffeeItem> source = (CurrentCategoryId == 0)
                ? Items
                : Items.Where(i => i.CategoryId == CurrentCategoryId);

            if (string.IsNullOrWhiteSpace(SearchItemKeyword))
            {
                FilteredItems = new ObservableCollection<MenuCoffeeItem>(source);
                return;
            }

            var keyword = SearchItemKeyword.Trim();
            var result = source.Where(i => i.ItemName.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            FilteredItems = new ObservableCollection<MenuCoffeeItem>(result);
        }

        private void LoadAvailableSizes()
        {
            AvailableSizes.Clear();
            SelectedSize = null;
            SelectedPrice = 0;

            if (SelectedItem?.ItemPrices == null || SelectedItem.ItemPrices.Count == 0) return;

            if (SelectedItem.CategoryId == 7) // Food
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
                .Select(p => new SizeViewModel { SizeName = p.Size!.SizeName!, Price = p.Price, IsSelected = false })
                .ToList();

            foreach (var size in sizeList) AvailableSizes.Add(size);

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

        private async Task UploadImageAsync()
        {
            var dialog = new OpenFileDialog { Filter = "Image files (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp" };
            if (dialog.ShowDialog() != true) return;

            // Mở cửa sổ cắt ảnh
            var cropper = new CoffeeShop.View.Admin.ImageCropperWindow(dialog.FileName);
            cropper.Owner = Application.Current.MainWindow;

            if (cropper.ShowDialog() == true)
            {
                // Lưu kết quả vào biến tạm
                _tempCroppedBitmap = cropper.CroppedImage;

                // Hiển thị tạm lên UI để người dùng thấy
                Image = _tempCroppedBitmap as BitmapImage ?? new BitmapImage(new Uri(dialog.FileName));
            }
        }


        private async Task AddItemAsync()
        {
            IsLoading = true;
            try
            {
                var result = await Task.Run(() =>
                {
                    using var context = new CoffeeShopContext();

                    // Tạo món mới với đường dẫn mặc định trước
                    var item = new Item
                    {
                        ItemName = "Món mới",
                        CategoryId = CurrentCategoryId == 0 ? 1 : CurrentCategoryId,
                        IsAvailable = true,
                        ImagePath = "/Assets/Images/imgItemExample.jpg" // Mặc định
                    };

                    context.Items.Add(item);
                    context.SaveChanges(); // DB sẽ sinh ra ItemId ở đây

                    // 2. Nếu có ảnh đã cắt, thực hiện lưu file với tên là ItemId
                    if (_tempCroppedBitmap != null)
                    {
                        string fileName = $"{item.ItemId}.jpg";
                        string relativePath = Path.Combine("Assets", "Images", "Items", fileName);
                        string absolutePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);

                        // Tạo thư mục nếu chưa có
                        Directory.CreateDirectory(Path.GetDirectoryName(absolutePath)!);

                        // Lưu file vật lý
                        using (var stream = new FileStream(absolutePath, FileMode.Create))
                        {
                            var encoder = new JpegBitmapEncoder();
                            encoder.Frames.Add(BitmapFrame.Create(_tempCroppedBitmap));
                            encoder.Save(stream);
                        }

                        // Cập nhật lại ImagePath chính xác vào DB
                        item.ImagePath = relativePath.Replace("\\", "/");
                        context.SaveChanges();
                    }

                    return item;
                });

                // Làm mới danh sách và xóa ảnh tạm
                _tempCroppedBitmap = null;
                LoadData();
                MessageBox.Show("Thêm món thành công!");
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
            finally { IsLoading = false; }
        }

        private async Task EditItemAsync()
        {
            if (SelectedItem == null) return;
            IsLoading = true;
            LoadingMessage = "Đang cập nhật món ăn...";

            try
            {
                await Task.Run(() =>
                {
                    using var context = new CoffeeShopContext();
                    var item = context.Items.Find(SelectedItem.ItemId);

                    if (item != null)
                    {
                        // Cập nhật các thông tin văn bản
                        item.ItemName = SelectedItem.ItemName;
                        item.CategoryId = SelectedItem.CategoryId;
                        item.IsAvailable = SelectedItem.IsAvailable;
                        item.Info = SelectedItem.Info;

                        // ử lý ảnh nếu có ảnh mới từ biến tạm (_tempCroppedBitmap)
                        if (_tempCroppedBitmap != null)
                        {
                            string fileName = $"{item.ItemId}.jpg";
                            string relativePath = Path.Combine("Assets", "Images", "Items", fileName).Replace("\\", "/");
                            string absolutePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);

                            // Đảm bảo thư mục tồn tại
                            Directory.CreateDirectory(Path.GetDirectoryName(absolutePath)!);

                            // Lưu file mới (Sẽ tự động ghi đè nếu trùng tên)
                            using (var stream = new FileStream(absolutePath, FileMode.Create))
                            {
                                var encoder = new JpegBitmapEncoder { QualityLevel = 90 };
                                encoder.Frames.Add(BitmapFrame.Create(_tempCroppedBitmap));
                                encoder.Save(stream);
                            }

                            // Cập nhật đường dẫn vào DB nếu trước đó là ảnh mặc định hoặc đường dẫn khác
                            item.ImagePath = relativePath;
                        }

                        context.SaveChanges();
                    }
                });

                // Xóa ảnh tạm và làm mới UI
                _tempCroppedBitmap = null;

                // Cần Invoke để cập nhật UI Collection
                Application.Current.Dispatcher.Invoke(() => {
                    LoadOrderItemsFromDB();
                    FilterItemsByCategory();
                });

                MessageBox.Show("Cập nhật món thành công!", "Thông báo");

                EventAggregator.Instance.Publish(new ItemsChangedMessage
                {
                    Action = "Updated",
                    ItemId = SelectedItem.ItemId
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi sửa món: {ex.Message}");
            }
            finally { IsLoading = false; }
        }

        private async Task DeleteItemAsync()
        {
            if (SelectedItem == null) return;
            if (MessageBox.Show("Bạn có chắc muốn xóa món này?", "Xác nhận xóa món", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                IsLoading = true;
                try
                {
                    await Task.Run(() => {
                        Application.Current.Dispatcher.Invoke(() => {
                            using var context = new CoffeeShopContext();
                            var item = context.Items.Find(SelectedItem.ItemId);
                            if (item != null)
                            {
                                item.IsDeleted = true;
                                context.SaveChanges();
                                if (!string.IsNullOrEmpty(item.ImagePath) && !item.ImagePath.Contains("imgItemExample.jpg"))
                                {
                                    string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, item.ImagePath);
                                    if (File.Exists(fullPath))
                                    {
                                        try { File.Delete(fullPath); } catch { /* File có thể đang bị dùng, bỏ qua */ }
                                    }
                                }
                                Items.Remove(SelectedItem);
                                FilterItemsByCategory();
                            }
                        });
                    });
                }
                finally { IsLoading = false; }
            }
        }
        #endregion

        #region Helper Classes
        public class MenuCoffeeItem : INotifyPropertyChanged
        {
            private int _itemId;
            private string _itemName = string.Empty;
            private int _categoryId;
            private bool _isAvailable;
            private string? _imagePath;
            private string _info = string.Empty;
            private ObservableCollection<ItemPrice> _itemPrices = new();

            public int ItemId { get => _itemId; set { _itemId = value; OnPropertyChanged(nameof(ItemId)); } }
            public string ItemName { get => _itemName; set { _itemName = value; OnPropertyChanged(nameof(ItemName)); } }
            public int CategoryId { get => _categoryId; set { _categoryId = value; OnPropertyChanged(nameof(CategoryId)); } }
            public bool IsAvailable { get => _isAvailable; set { _isAvailable = value; OnPropertyChanged(nameof(IsAvailable)); } }
            public string ImagePath { get => _imagePath; set { _imagePath = value; OnPropertyChanged(nameof(ImagePath)); } }
            public string Info { get => _info; set { _info = value; OnPropertyChanged(nameof(Info)); } }
            public ObservableCollection<ItemPrice> ItemPrices { get => _itemPrices; set { _itemPrices = value; OnPropertyChanged(nameof(ItemPrices)); } }

            public event PropertyChangedEventHandler? PropertyChanged;
            protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public class SizeViewModel : INotifyPropertyChanged
        {
            private string _sizeName = string.Empty;
            private decimal _price;
            private bool _isSelected;

            public string SizeName { get => _sizeName; set { _sizeName = value; OnPropertyChanged(nameof(SizeName)); } }
            public decimal Price { get => _price; set { _price = value; OnPropertyChanged(nameof(Price)); } }
            public bool IsSelected { get => _isSelected; set { _isSelected = value; OnPropertyChanged(nameof(IsSelected)); } }

            public event PropertyChangedEventHandler? PropertyChanged;
            protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion
    }

    #region RelayCommand
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T?>? _execute;
        private readonly Func<T?, Task>? _executeAsync;
        private readonly Predicate<T?>? _canExecute;

        public RelayCommand(Action<T?> execute, Predicate<T?>? canExecute = null) { _execute = execute; _canExecute = canExecute; }
        public RelayCommand(Func<T?, Task> executeAsync, Predicate<T?>? canExecute = null) { _executeAsync = executeAsync; _canExecute = canExecute; }
        public bool CanExecute(object? parameter) => _canExecute == null || _canExecute((T?)parameter);
        public async void Execute(object? parameter)
        {
            if (_executeAsync != null) await _executeAsync((T?)parameter);
            else _execute?.Invoke((T?)parameter);
        }
        public event EventHandler? CanExecuteChanged { add => CommandManager.RequerySuggested += value; remove => CommandManager.RequerySuggested -= value; }
    }
    #endregion
}