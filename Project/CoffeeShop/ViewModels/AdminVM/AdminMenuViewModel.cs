using CoffeeShop.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CoffeeShop.ViewModels.AdminVM
{
    public class AdminMenuViewModel : BaseViewModel
    {
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
            SubscribeToMessages();
            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            await LoadOrderItemsFromDB();
            FilterItemsByCategory();
        }
        public AdminMenuViewModel(int itemId)
        {
            InitializeViewModel();
            _ = InitializeEditModeAsync(itemId);
        }

        private async Task InitializeEditModeAsync(int itemId)
        {
            IsLoading = true;
            await Task.Run(() => LoadSelectedItem(itemId));
            IsLoading = false;
        }

        private void InitializeViewModel()
        {
            Items = new ObservableCollection<MenuCoffeeItem>();
            FilteredItems = new ObservableCollection<MenuCoffeeItem>();
            AvailableSizes = new ObservableCollection<SizeViewModel>(); 
            InitializeCommands();   
            SubscribeToMessages();  
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

        //private void OnItemsChanged(ItemsChangedMessage message)
        //{
        //    Application.Current.Dispatcher.Invoke(() =>
        //    {
        //        var currentSelectedId = SelectedItem?.ItemId;

        //        LoadOrderItemsFromDB();
        //        FilterItemsByCategory();

        //        // Giữ lại selection và refresh sizes + image
        //        if (currentSelectedId.HasValue)
        //        {
        //            var updatedItem = FilteredItems.FirstOrDefault(i => i.ItemId == currentSelectedId.Value);
        //            if (updatedItem != null)
        //            {
        //                SelectedItem = updatedItem;

        //                // Force reload image
        //                if (!string.IsNullOrEmpty(updatedItem.ImagePath))
        //                {
        //                    ImagePath = updatedItem.ImagePath;
        //                }
        //            }
        //            else
        //            {
        //                SelectedItem = null;
        //            }
        //        }
        //        else
        //        {
        //            SelectedItem = null;
        //        }
        //    });
        //}

        private async void OnItemsChanged(ItemsChangedMessage message)
        {
            var currentSelectedId = SelectedItem?.ItemId;

            // Chỉ cần await Load, mọi thứ sẽ được làm mới sạch sẽ
            await LoadOrderItemsFromDB();

            // Khôi phục lại item đang chọn nếu nó vẫn tồn tại
            if (currentSelectedId.HasValue)
            {
                SelectedItem = FilteredItems.FirstOrDefault(i => i.ItemId == currentSelectedId.Value);
            }
        }

        #endregion

        #region Load, Search, Edit, Delete Items
        private void LoadSelectedItem(int itemId)
        {
            try
            {
                using (var context = new CoffeeShopContext())
                {
                    var item = context.Items.FirstOrDefault(i => i.ItemId == itemId && !i.IsDeleted);
                    if (item != null)
                    {
                        var editItem = new MenuCoffeeItem
                        {
                            ItemId = item.ItemId,
                            ItemName = item.ItemName,
                            CategoryId = item.CategoryId,
                            IsAvailable = item.IsAvailable,
                            Info = item.Info ?? string.Empty,
                            ImagePath = item.ImagePath ?? string.Empty, // Đường dẫn gốc DB
                            ItemPrices = new ObservableCollection<ItemPrice>(
                                context.ItemPrices.Include(ip => ip.Size)
                                       .Where(ip => ip.ItemId == itemId && !ip.IsDeleted).ToList())
                        };

                        SelectedItem = editItem;

                        // QUAN TRỌNG: Phải gán SelectedImagePath bằng đường dẫn cũ trong DB
                        // để nếu người dùng không chọn ảnh mới, giá trị này vẫn tồn tại để lưu lại.
                        this.SelectedImagePath = item.ImagePath;

                        // Hiển thị ảnh lên UI
                        this.ImagePath = GetDisplayPath(item.ImagePath);

                        LoadAvailableSizes();
                    }
                }
            }
            catch (Exception ex) { Debug.WriteLine(ex.Message); }
        }


        //public async Task LoadOrderItemsFromDB()
        //{
        //    if (IsLoading) return;
        //    IsLoading = true;

        //    // Cho UI một khoảng nghỉ 50ms để kịp hiện icon loading lên
        //    await Task.Delay(50);

        //    try
        //    {
        //        var dbItems = await Task.Run(() =>
        //        {
        //            using var context = new CoffeeShopContext();
        //            return context.Items
        //                .Where(i => !i.IsDeleted)
        //                .OrderByDescending(i => i.IsAvailable)
        //                .ToList();
        //        });

        //        _items.Clear();
        //        foreach (var item in dbItems)
        //        {
        //            var newItem = new MenuCoffeeItem
        //            {
        //                ItemId = item.ItemId,
        //                ItemName = item.ItemName,
        //                CategoryId = item.CategoryId,
        //                IsAvailable = item.IsAvailable,
        //                Info = item.Info ?? string.Empty,
        //                ImagePath = GetDisplayPath(item.ImagePath),
        //                ItemPrices = new ObservableCollection<ItemPrice>(
        //                    item.ItemPrices
        //                        .Where(ip => ip.ItemId == item.ItemId && ip.IsDeleted == false)
        //                        .ToList()
        //                ),
        //            };

        //            await Application.Current.Dispatcher.InvokeAsync(() =>
        //            {
        //                _items.Add(newItem);
        //                FilteredItems.Add(newItem);
        //            }, System.Windows.Threading.DispatcherPriority.Background);
        //        }
        //    }
        //    catch (Exception ex) { Debug.WriteLine(ex.Message); }
        //    finally { IsLoading = false; }
        //}

        public async Task LoadOrderItemsFromDB()
        {
            if (IsLoading) return;
            IsLoading = true;
            await Task.Delay(50); // Nhường UI vẽ icon loading

            try
            {
                // 1. CHUẨN BỊ DỮ LIỆU SẠCH Ở LUỒNG PHỤ
                var cleanList = await Task.Run(() =>
                {
                    using var context = new CoffeeShopContext();
                    var dbItems = context.Items
                        .Include(i => i.ItemPrices).ThenInclude(ip => ip.Size)
                        .Where(i => !i.IsDeleted)
                        .OrderByDescending(i => i.IsAvailable).ToList();

                    // Chuyển đổi sang MenuCoffeeItem ngay tại luồng phụ
                    return dbItems.Select(item => new MenuCoffeeItem
                    {
                        ItemId = item.ItemId,
                        ItemName = item.ItemName,
                        CategoryId = item.CategoryId,
                        IsAvailable = item.IsAvailable,
                        Info = item.Info ?? string.Empty,
                        ImagePath = GetDisplayPath(item.ImagePath), // Đường dẫn hiển thị UI
                        ItemPrices = new ObservableCollection<ItemPrice>(item.ItemPrices.Where(ip => !ip.IsDeleted).ToList())
                    }).ToList();
                });

                // 2. GÁN 1 LẦN DUY NHẤT VÀO UI (Triệt tiêu lỗi nhân đôi)
                Application.Current.Dispatcher.Invoke(() =>
                {
                    // Gán mới hoàn toàn thay vì Clear/Add
                    Items = new ObservableCollection<MenuCoffeeItem>(cleanList);
                    FilterItemsByCategory();
                });
            }
            catch (Exception ex) { Debug.WriteLine($"Error: {ex.Message}"); }
            finally { IsLoading = false; }
        }
        private string GetDisplayPath(string? imagePath)
        {
            string displayImagePath;
            if (string.IsNullOrEmpty(imagePath) ||
                imagePath.Contains("imgItemExample.jpg") ||
                imagePath == "Assets/Images/imgItemExample.jpg" ||
                imagePath == "Assets\\Images\\imgItemExample.jpg")
            {
                displayImagePath = "/Assets/Images/imgItemExample.jpg";
            }
            // Ảnh thật từ user upload
            else
            {
                string cleanRelativePath = imagePath?.TrimStart('/', '\\') ?? "";
                displayImagePath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    imagePath
                );
            }
            return displayImagePath;
        }

        //public async Task LoadOrderItemsFromDB()
        //{
        //    if (IsLoading) return;
        //    IsLoading = true;
        //    await Task.Delay(100);

        //    try
        //    {
        //        var processedItems = await Task.Run(() =>
        //        {
        //            using (var context = new CoffeeShopContext())
        //            {
        //                var dbItems = context.Items
        //                    .Include(i => i.ItemPrices)
        //                    .ThenInclude(ip => ip.Size)
        //                    .Where(i => !i.IsDeleted)
        //                    .OrderByDescending(i => i.IsAvailable)
        //                    .ToList();

        //                var tempList = new List<MenuCoffeeItem>();
        //                foreach (var item in dbItems)
        //                {
        //                    string displayImagePath;
        //                    if (string.IsNullOrEmpty(item.ImagePath) ||
        //                        item.ImagePath.Contains("imgItemExample.jpg") ||
        //                        item.ImagePath == "Assets/Images/imgItemExample.jpg" ||
        //                        item.ImagePath == "Assets\\Images\\imgItemExample.jpg")
        //                    {
        //                        displayImagePath = "/Assets/Images/imgItemExample.jpg";
        //                    }
        //                    // Ảnh thật từ user upload
        //                    else
        //                    {
        //                        string cleanRelativePath = item.ImagePath?.TrimStart('/', '\\') ?? "";
        //                        displayImagePath = Path.Combine(
        //                            AppDomain.CurrentDomain.BaseDirectory,
        //                            item.ImagePath
        //                        );
        //                    }

        //                    tempList.Add(new MenuCoffeeItem
        //                    {
        //                        ItemId = item.ItemId,
        //                        ItemName = item.ItemName,
        //                        CategoryId = item.CategoryId,
        //                        IsAvailable = item.IsAvailable,
        //                        Info = item.Info ?? string.Empty,
        //                        ImagePath = displayImagePath,
        //                        ItemPrices = new ObservableCollection<ItemPrice>(item.ItemPrices.Where(ip => !ip.IsDeleted))
        //                    });
        //                }
        //                return tempList;
        //            }
        //        });

        //        _items.Clear();
        //        FilteredItems.Clear();

        //        foreach (var item in processedItems)
        //        {
        //            _items.Add(item);
        //            FilteredItems.Add(item);

        //            if (_items.Count % 5 == 0) await Task.Delay(1);
        //        }

        //        OnPropertyChanged(nameof(FilteredItems));
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine($"Error: {ex.Message}");
        //    }
        //    finally
        //    {
        //        // 4. Tắt loading
        //        IsLoading = false;
        //    }
        //}
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

            IsLoading = true;
            try
            {
                string fileName = $"{Guid.NewGuid()}{Path.GetExtension(dialog.FileName)}";
                string relativePath = Path.Combine("Assets", "Images", "Items", fileName);
                string absolutePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);

                Directory.CreateDirectory(Path.GetDirectoryName(absolutePath)!);
                File.Copy(dialog.FileName, absolutePath, true);

                // Cập nhật đường dẫn TUYỆT ĐỐI để hiển thị lên UI ngay lập tức
                this.ImagePath = absolutePath;

                // Cập nhật đường dẫn để chuẩn bị lưu vào DB (dùng đường dẫn tương đối)
                this.SelectedImagePath = relativePath;

                if (SelectedItem != null)
                {
                    SelectedItem.ImagePath = relativePath;
                }
            }
            finally { IsLoading = false; }
        }


        private async Task AddItemAsync()
        {
            IsLoading = true;
            try
            {
                int newId = 0;
                await Task.Run(() =>
                {
                    using var context = new CoffeeShopContext();
                    var item = new Item { /* gán giá trị... */ };
                    context.Items.Add(item);
                    context.SaveChanges();
                    newId = item.ItemId;
                });

                // Không cần Items.Add(newMenu) ở đây nữa
                // Chỉ cần bắn tin nhắn, danh sách sẽ tự làm mới từ DB
                EventAggregator.Instance.Publish(new ItemsChangedMessage { Action = "Added", ItemId = newId });
            }
            finally { IsLoading = false; }
        }

        private async Task EditItemAsync()
        {
            if (SelectedItem == null) return;
            IsLoading = true;
            await Task.Delay(100);

            try
            {
                var itemId = SelectedItem.ItemId;
                var itemName = SelectedItem.ItemName;
                var categoryId = SelectedItem.CategoryId;
                var isAvailable = SelectedItem.IsAvailable;
                var info = SelectedItem.Info;

                // Lấy đường dẫn đã được chuẩn bị (có thể là đường dẫn cũ hoặc mới từ Upload)
                var pathToSave = SelectedImagePath;

                await Task.Run(() =>
                {
                    using var context = new CoffeeShopContext();
                    var item = context.Items.Find(itemId);
                    if (item != null)
                    {
                        item.ItemName = itemName;
                        item.CategoryId = categoryId;
                        item.IsAvailable = isAvailable;
                        item.Info = info;

                        if (!string.IsNullOrEmpty(pathToSave))
                        {
                            item.ImagePath = GetDisplayPath(pathToSave);
                        }
                        context.SaveChanges();
                    }
                });

                EventAggregator.Instance.Publish(new ItemsChangedMessage { Action = "Updated", ItemId = itemId });
            }
            finally { IsLoading = false; }
        }

        private async Task DeleteItemAsync()
        {
            if (SelectedItem == null) return;

            if (MessageBox.Show("Bạn có chắc muốn xóa món này?", "Xác nhận", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;

            IsLoading = true;
            await Task.Delay(50);

            try
            {
                int itemId = SelectedItem.ItemId;

                // 1. Xóa ngầm trong Database
                await Task.Run(() =>
                {
                    using var context = new CoffeeShopContext();
                    var item = context.Items.Find(itemId);
                    if (item != null)
                    {
                        item.IsDeleted = true; // Soft delete
                        context.SaveChanges();
                    }
                });

                // 2. Bắn tin nhắn để refresh lại toàn bộ danh sách
                EventAggregator.Instance.Publish(new ItemsChangedMessage { Action = "Deleted", ItemId = itemId });

                // Reset lựa chọn về null
                SelectedItem = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
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
            private ImageSource? _imageSource;
            public ImageSource? ImageSource
            {
                get => _imageSource;
                set { _imageSource = value; OnPropertyChanged(nameof(ImageSource)); }
            }
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