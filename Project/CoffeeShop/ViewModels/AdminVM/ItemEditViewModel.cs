using CoffeeShop.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CoffeeShop.ViewModels.AdminVM
{
    public class ItemEditViewModel : INotifyPropertyChanged
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

        private int? _itemId;
        public int? ItemId
        {
            get => _itemId;
            set { _itemId = value; OnPropertyChanged(nameof(ItemId)); }
        }

        private string _windowTitle = "Thêm món mới";
        public string WindowTitle
        {
            get => _windowTitle;
            set { _windowTitle = value; OnPropertyChanged(nameof(WindowTitle)); }
        }

        private string _itemName = "";
        public string ItemName
        {
            get => _itemName;
            set { _itemName = value; OnPropertyChanged(nameof(ItemName)); }
        }

        private bool _isAvailable = true;
        public bool IsAvailable
        {
            get => _isAvailable;
            set { _isAvailable = value; OnPropertyChanged(nameof(IsAvailable)); }
        }

        private string _info = "";
        public string Info
        {
            get => _info;
            set { _info = value; OnPropertyChanged(nameof(Info)); }
        }

        private string _imagePath = "/Assets/Images/imgItemExample.jpg";
        public string ImagePath
        {
            get => _imagePath;
            set
            {
                _imagePath = value;
                OnPropertyChanged();
                LoadImageFromPath(_imagePath); // 🔥 mấu chốt
            }
        }

        private BitmapImage _image;
        public BitmapImage Image
        {
            get => _image;
            set
            {
                _image = value;
                OnPropertyChanged();
            }
        }
        private int _categoryId = 1;
        public int CategoryId
        {
            get => _categoryId;
            set
            {
                if (_categoryId != value)
                {
                    _categoryId = value;
                    OnPropertyChanged(nameof(CategoryId));

                    // Khi chuyển sang Food, tự động xóa hết size (chỉ giữ 1)
                    if (value == 7 && SizePrices.Count > 1)
                    {
                        var firstPrice = SizePrices.FirstOrDefault();
                        SizePrices.Clear();

                        if (firstPrice != null)
                        {
                            // Giữ lại giá đầu tiên, xóa SizeId
                            firstPrice.SizeId = null;
                            SizePrices.Add(firstPrice);
                        }
                        else
                        {
                            // Nếu không có giá nào, thêm giá mặc định
                            SizePrices.Add(new SizePriceViewModel
                            {
                                SizeId = null,
                                Price = 0
                            });
                        }

                        MessageBox.Show("Đã chuyển sang Food. Chỉ giữ lại 1 giá duy nhất.",
                            "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    // Khi chuyển từ Food sang Drinks, nếu chưa có size thì thêm
                    else if (_categoryId == 7 && value != 7 && SizePrices.Count == 1 && SizePrices[0].SizeId == null)
                    {
                        var currentPrice = SizePrices[0].Price;
                        SizePrices.Clear();

                        // Thêm size mặc định (ví dụ size đầu tiên)
                        if (AvailableSizes.Count > 0)
                        {
                            SizePrices.Add(new SizePriceViewModel
                            {
                                SizeId = AvailableSizes[0].SizeId,
                                Price = currentPrice
                            });
                        }
                    }
                }
            }
        }


        private ObservableCollection<CategoryViewModel> _categories = new();
        public ObservableCollection<CategoryViewModel> Categories
        {
            get => _categories;
            set { _categories = value; OnPropertyChanged(nameof(Categories)); }
        }

        private ObservableCollection<SizeViewModel> _availableSizes = new();
        public ObservableCollection<SizeViewModel> AvailableSizes
        {
            get => _availableSizes;
            set { _availableSizes = value; OnPropertyChanged(nameof(AvailableSizes)); }
        }

        private ObservableCollection<SizePriceViewModel> _sizePrices = new();
        public ObservableCollection<SizePriceViewModel> SizePrices
        {
            get => _sizePrices;
            set { _sizePrices = value; OnPropertyChanged(nameof(SizePrices)); }
        }

        private bool? _dialogResult;
        public bool? DialogResult
        {
            get => _dialogResult;
            set { _dialogResult = value; OnPropertyChanged(nameof(DialogResult)); }
        }
        #endregion

        #region Commands
        public ICommand UploadImageCommand { get; private set; }
        public ICommand AddSizeCommand { get; private set; }
        public ICommand RemoveSizeCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }
        #endregion

        #region Constructor
        public ItemEditViewModel()
        {
            InitializeCommands();
            LoadCategories();
            LoadAvailableSizes();
        }

        public ItemEditViewModel(int itemId) : this()
        {
            ItemId = itemId;
            WindowTitle = "Chỉnh sửa món";
            LoadItemData(itemId);
        }
        #endregion

        #region Command Initialization
        private void InitializeCommands()
        {
            UploadImageCommand = new RelayCommand<object>(async p => await UploadImageAsync());
            AddSizeCommand = new RelayCommand<object>(p => AddSize());
            RemoveSizeCommand = new RelayCommand<SizePriceViewModel>(p => RemoveSize(p));
            SaveCommand = new RelayCommand<object>(async p => await SaveAsync());
            CancelCommand = new RelayCommand<object>(p => Cancel());
        }
        #endregion

        #region Load Data
        private void LoadCategories()
        {
            Categories.Clear();
            try
            {
                using var context = new CoffeeShopContext();
                var categories = context.Categories
                    .Where(c => !c.IsDeleted)
                    .Select(c => new CategoryViewModel
                    {
                        Id = c.CategoryId,
                        Name = c.CategoryName
                    })
                    .ToList();

                foreach (var cat in categories)
                    Categories.Add(cat);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh mục: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadAvailableSizes()
        {
            AvailableSizes.Clear();
            try
            {
                using var context = new CoffeeShopContext();
                var sizes = context.Sizes
                    .Where(s => !s.IsDeleted)
                    .Select(s => new SizeViewModel
                    {
                        SizeId = s.SizeId,
                        SizeName = s.SizeName
                    })
                    .ToList();

                foreach (var size in sizes)
                    AvailableSizes.Add(size);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải size: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadItemData(int itemId)
        {
            try
            {
                using var context = new CoffeeShopContext();
                var item = context.Items
                    .Include(i => i.ItemPrices)
                    .ThenInclude(ip => ip.Size)
                    .FirstOrDefault(i => i.ItemId == itemId && !i.IsDeleted);

                if (item != null)
                {
                    ItemName = item.ItemName;
                    CategoryId = item.CategoryId;
                    IsAvailable = item.IsAvailable;
                    Info = item.Info ?? "";

                    // Chuẩn hóa ImagePath
                    if (item != null)
                    {
                        string dbPath = item.ImagePath;

                        if (string.IsNullOrWhiteSpace(dbPath) || dbPath.Contains("imgItemExample.jpg"))
                        {
                            ImagePath = "pack://application:,,,/Assets/Images/imgItemExample.jpg";
                        }
                        else
                        {
                            // 1. Xóa dấu gạch ở đầu VÀ chuẩn hóa dấu gạch chéo của Windows
                            string cleanPath = dbPath.TrimStart('/', '\\').Replace('/', '\\');

                            // 2. Kết hợp với thư mục gốc của App
                            string absolutePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, cleanPath);

                            if (System.IO.File.Exists(absolutePath))
                            {
                                // Dùng UriKind.Absolute để WPF biết đây là đường dẫn ngoài ổ cứng
                                ImagePath = absolutePath;
                            }
                            else
                            {
                                // Nếu vẫn không thấy, thử dùng Pack URI (nếu bạn lỡ để Build Action là Resource)
                                ImagePath = "pack://application:,,,/" + dbPath.TrimStart('/');
                            }
                        }
                    }

                    SizePrices.Clear();
                    foreach (var price in item.ItemPrices.Where(ip => !ip.IsDeleted))
                    {
                        SizePrices.Add(new SizePriceViewModel
                        {
                            PriceId = price.PriceId,
                            SizeId = price.SizeId,
                            Price = price.Price
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải thông tin món: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region Commands Implementation

        private void LoadImageFromPath(string path)
        {
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;

                // 1. Chỉ dùng ảnh mặc định khi đường dẫn thực sự trống hoặc chứa tên ảnh mặc định
                if (string.IsNullOrWhiteSpace(path) || path.Contains("imgItemExample.jpg"))
                {
                    bitmap.UriSource = new Uri("pack://application:,,,/Assets/Images/imgItemExample.jpg", UriKind.Absolute);
                }
                // 2. Nếu là Pack URI (đã chuẩn hóa sẵn)
                else if (path.StartsWith("pack://"))
                {
                    bitmap.UriSource = new Uri(path, UriKind.Absolute);
                }
                // 3. Nếu là đường dẫn tuyệt đối (C:\...)
                else if (Path.IsPathRooted(path))
                {
                    if (File.Exists(path))
                        bitmap.UriSource = new Uri(path, UriKind.Absolute);
                    else
                        bitmap.UriSource = new Uri("pack://application:,,,/Assets/Images/imgItemExample.jpg", UriKind.Absolute);
                }
                // 4. Case quan trọng nhất: Đường dẫn từ DB (/Assets/Images/...)
                else
                {
                    // Xóa dấu / ở đầu để Path.Combine không hiểu nhầm là gốc ổ đĩa
                    string cleanPath = path.TrimStart('/', '\\').Replace('/', '\\');
                    string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, cleanPath);

                    if (File.Exists(fullPath))
                    {
                        bitmap.UriSource = new Uri(fullPath, UriKind.Absolute);
                    }
                    else
                    {
                        // Thử tìm trong Resource nếu không thấy file vật lý (Dành cho ảnh cũ)
                        System.Diagnostics.Debug.WriteLine($"[Warning] Khong tim thay file: {fullPath}");
                        bitmap.UriSource = new Uri("pack://application:,,,/Assets/Images/imgItemExample.jpg", UriKind.Absolute);
                    }
                }

                bitmap.EndInit();
                if (bitmap.CanFreeze) bitmap.Freeze();
                Image = bitmap;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Lỗi Image] {ex.Message}");
                // Fallback an toàn nhất
                Image = new BitmapImage(new Uri("pack://application:,,,/Assets/Images/imgItemExample.jpg", UriKind.Absolute));
            }
        }


        private async Task UploadImageAsync()
        {
            var dialog = new OpenFileDialog { Filter = "Image files (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png" };
            if (dialog.ShowDialog() != true) return;

            IsLoading = true;
            try
            {
                string fileName = $"{Guid.NewGuid()}{Path.GetExtension(dialog.FileName)}";
                string relativeFolder = Path.Combine("Assets", "Images", "Menu");
                string absoluteFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativeFolder);

                if (!Directory.Exists(absoluteFolder))
                    Directory.CreateDirectory(absoluteFolder);

                string destPath = Path.Combine(absoluteFolder, fileName);

                // Copy file
                await Task.Run(() => File.Copy(dialog.FileName, destPath, true));

                // Cập nhật ImagePath
                ImagePath = destPath;
            }
            catch (Exception ex) { MessageBox.Show($"Lỗi: {ex.Message}"); }
            finally { IsLoading = false; }
        }


        private void AddSize()
        {
            if (CategoryId == 7) // Food - không cần size
            {
                if (SizePrices.Count > 0)
                {
                    MessageBox.Show("Món Food chỉ có một giá duy nhất!",
                        "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                SizePrices.Add(new SizePriceViewModel
                {
                    SizeId = null, // Food không có SizeId
                    Price = 0
                });
            }
            else // Drinks - cần chọn size
            {
                if (AvailableSizes.Count == 0)
                {
                    MessageBox.Show("Không có size nào khả dụng!",
                        "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Kiểm tra xem size đã được thêm chưa
                var usedSizeIds = SizePrices.Where(sp => sp.SizeId.HasValue)
                                            .Select(sp => sp.SizeId.Value)
                                            .ToHashSet();

                var availableSize = AvailableSizes.FirstOrDefault(s => !usedSizeIds.Contains(s.SizeId));

                if (availableSize == null)
                {
                    MessageBox.Show("Đã thêm đủ tất cả các size!",
                        "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                SizePrices.Add(new SizePriceViewModel
                {
                    SizeId = availableSize.SizeId,
                    Price = 0
                });
            }
        }

        private void RemoveSize(SizePriceViewModel? sizePrice)
        {
            if (MessageBox.Show("Bạn có chắc muốn xóa size/giá này?", "Xác nhận xóa Size",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }
            if (sizePrice != null)
                SizePrices.Remove(sizePrice);
        }

        private async Task SaveAsync()
        {
            IsLoading = true;
            LoadingMessage = "Đang lưu...";

            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(ItemName))
                {
                    MessageBox.Show("Vui lòng nhập tên món!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (SizePrices.Count == 0)
                {
                    MessageBox.Show("Vui lòng thêm ít nhất 1 size/giá!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Validation cho Food
                if (CategoryId == 7 && SizePrices.Count > 1)
                {
                    MessageBox.Show("Món Food chỉ được có 1 giá duy nhất!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Validation cho Drinks
                if (CategoryId != 7)
                {
                    foreach (var sp in SizePrices)
                    {
                        if (!sp.SizeId.HasValue)
                        {
                            MessageBox.Show("Vui lòng chọn size cho tất cả các giá!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                    }

                    // Kiểm tra trùng size
                    var sizeIds = SizePrices.Where(sp => sp.SizeId.HasValue)
                                           .Select(sp => sp.SizeId.Value)
                                           .ToList();

                    if (sizeIds.Count != sizeIds.Distinct().Count())
                    {
                        MessageBox.Show("Không được chọn trùng size!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                await Task.Run(() =>
                {
                    using var context = new CoffeeShopContext();
                    Item? item;

                    if (ItemId.HasValue)
                    {
                        item = context.Items.FirstOrDefault(i => i.ItemId == ItemId.Value);
                        if (item == null) throw new Exception("Không tìm thấy món.");
                    }
                    else
                    {
                        item = new Item { IsDeleted = false };
                        context.Items.Add(item);
                    }

                    // --- XỬ LÝ ẢNH ---
                    string pathForDb;

                    if (!string.IsNullOrEmpty(ImagePath) && Path.IsPathRooted(ImagePath))
                    {
                        pathForDb = Path.GetRelativePath(AppDomain.CurrentDomain.BaseDirectory, ImagePath);
                    }
                    else if (!string.IsNullOrEmpty(ImagePath) &&
                             (ImagePath.StartsWith("/Assets") || ImagePath.StartsWith("pack://")))
                    {
                        pathForDb = "Assets/Images/imgItemExample.jpg";
                    }
                    else if (string.IsNullOrEmpty(ImagePath))
                    {
                        pathForDb = item.ImagePath ?? "Assets/Images/imgItemExample.jpg";
                    }
                    else
                    {
                        pathForDb = ImagePath;
                    }

                    item.ItemName = ItemName;
                    item.CategoryId = CategoryId;
                    item.IsAvailable = IsAvailable;
                    item.Info = Info;
                    item.ImagePath = pathForDb;

                    context.SaveChanges();

                    // Xử lý ItemPrices - XÓA HẾT rồi thêm lại
                    var existingPrices = context.ItemPrices
                        .Where(ip => ip.ItemId == item.ItemId)
                        .ToList();

                    // Đánh dấu xóa tất cả prices cũ
                    foreach (var price in existingPrices)
                    {
                        price.IsDeleted = true;
                    }

                    // Thêm/Cập nhật prices mới
                    foreach (var sp in SizePrices)
                    {
                        ItemPrice? priceToUpdate = null;

                        if (sp.PriceId.HasValue)
                        {
                            priceToUpdate = existingPrices.FirstOrDefault(p => p.PriceId == sp.PriceId.Value);
                        }

                        if (priceToUpdate != null)
                        {
                            priceToUpdate.SizeId = sp.SizeId;
                            priceToUpdate.Price = sp.Price;
                            priceToUpdate.IsDeleted = false;
                        }
                        else
                        {
                            context.ItemPrices.Add(new ItemPrice
                            {
                                ItemId = item.ItemId,
                                SizeId = sp.SizeId,
                                Price = sp.Price,
                                IsDeleted = false
                            });
                        }
                    }

                    context.SaveChanges();
                    EventAggregator.Instance.Publish(new ItemsChangedMessage());
                });

                MessageBox.Show("Lưu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lưu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }


        private void Cancel()
        {
            DialogResult = false;
        }
        #endregion

        #region Helper Classes
        public class CategoryViewModel
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
        }

        public class SizeViewModel : INotifyPropertyChanged
        {
            private int _sizeId;
            private string _sizeName = string.Empty;

            public int SizeId
            {
                get => _sizeId;
                set
                {
                    _sizeId = value;
                    OnPropertyChanged(nameof(SizeId));
                }
            }

            public string SizeName
            {
                get => _sizeName;
                set
                {
                    _sizeName = value;
                    OnPropertyChanged(nameof(SizeName));
                }
            }

            public event PropertyChangedEventHandler? PropertyChanged;
            protected virtual void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public class SizePriceViewModel : INotifyPropertyChanged
        {
            private int? _priceId;
            private int? _sizeId; // Nullable cho Food
            private decimal _price;

            public int? PriceId
            {
                get => _priceId;
                set { _priceId = value; OnPropertyChanged(nameof(PriceId)); }
            }

            public int? SizeId
            {
                get => _sizeId;
                set { _sizeId = value; OnPropertyChanged(nameof(SizeId)); }
            }

            public decimal Price
            {
                get => _price;
                set { _price = value; OnPropertyChanged(nameof(Price)); }
            }

            public event PropertyChangedEventHandler? PropertyChanged;
            protected void OnPropertyChanged(string name) =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion
    }
}