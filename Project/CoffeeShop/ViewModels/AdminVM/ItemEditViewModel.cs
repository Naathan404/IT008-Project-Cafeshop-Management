using CoffeeShop.Models;
using CoffeeShop.View.Admin;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
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

        private const string DEFAULT_IMAGE_PATH = "/Assets/Images/imgItemExample.jpg";
        private const int FOOD_CATEGORY_ID = 7;

        #region Properties
        private bool _isLoading;
        public bool IsLoading { get => _isLoading; set { _isLoading = value; OnPropertyChanged(nameof(IsLoading)); } }

        private string _loadingMessage = "Loading...";
        public string LoadingMessage { get => _loadingMessage; set { _loadingMessage = value; OnPropertyChanged(nameof(LoadingMessage)); } }

        public int? ItemId { get; set; }
        public string WindowTitle { get; set; } = "Thêm món mới";

        private string _itemName = "";
        public string ItemName { get => _itemName; set { _itemName = value; OnPropertyChanged(nameof(ItemName)); } }

        private bool _isAvailable = true;
        public bool IsAvailable { get => _isAvailable; set { _isAvailable = value; OnPropertyChanged(nameof(IsAvailable)); } }

        private string _info = "";
        public string Info { get => _info; set { _info = value; OnPropertyChanged(nameof(Info)); } }

        private string _imagePath = DEFAULT_IMAGE_PATH;
        public string ImagePath
        {
            get => _imagePath;
            set { _imagePath = value; OnPropertyChanged(); LoadImageFromPath(_imagePath); }
        }

        private BitmapImage? _image;
        public BitmapImage? Image { get => _image; set { _image = value; OnPropertyChanged(); } }

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
                    HandleCategoryChange(value);
                }
            }
        }

        public ObservableCollection<CategoryViewModel> Categories { get; set; } = new();
        public ObservableCollection<SizeViewModel> AvailableSizes { get; set; } = new();
        public ObservableCollection<SizePriceViewModel> SizePrices { get; set; } = new();

        private bool? _dialogResult;
        public bool? DialogResult { get => _dialogResult; set { _dialogResult = value; OnPropertyChanged(nameof(DialogResult)); } }
        #endregion

        #region Commands
        public ICommand UploadImageCommand { get; private set; }
        public ICommand AddSizeCommand { get; private set; }
        public ICommand RemoveSizeCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }
        #endregion

        public ItemEditViewModel()
        {
            InitializeCommands();
            LoadInitialData();
        }

        public ItemEditViewModel(int? itemId = null) : this()
        {
            if (itemId.HasValue)
            {
                ItemId = itemId;
                WindowTitle = "Chỉnh sửa món";
                LoadItemData(itemId.Value);
            }
            else
            {
                ItemId = null;
                WindowTitle = "Thêm món mới";
                AddSize();
            }
        }

        private void InitializeCommands()
        {
            UploadImageCommand = new RelayCommand<object>(async _ => await UploadImageAsync());
            AddSizeCommand = new RelayCommand<object>(_ => AddSize());
            RemoveSizeCommand = new RelayCommand<SizePriceViewModel>(p => RemoveSize(p));
            SaveCommand = new RelayCommand<object>(async _ => await SaveAsync());
            CancelCommand = new RelayCommand<object>(_ => Cancel());
        }

        #region Logic Xử lý Category
        private void HandleCategoryChange(int newId)
        {
            if (newId == FOOD_CATEGORY_ID)
            {
                var currentPrice = SizePrices.FirstOrDefault()?.Price ?? 0;
                SizePrices.Clear();
                SizePrices.Add(new SizePriceViewModel { SizeId = null, Price = currentPrice });
            }
            else if (SizePrices.Count > 0 && SizePrices[0].SizeId == null)
            {
                if (AvailableSizes.Count > 0)
                {
                    SizePrices[0].SizeId = AvailableSizes[0].SizeId;
                }
            }
        }
        #endregion

        #region Data Loading
        private async void LoadInitialData()
        {
            try
            {
                using var context = new CoffeeShopContext();
                var cats = await context.Categories.Where(c => !c.IsDeleted).ToListAsync();
                var sizes = await context.Sizes.Where(s => !s.IsDeleted).ToListAsync();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Categories.Clear();
                    cats.ForEach(c => Categories.Add(new CategoryViewModel { Id = c.CategoryId, Name = c.CategoryName }));

                    AvailableSizes.Clear();
                    sizes.ForEach(s => AvailableSizes.Add(new SizeViewModel { SizeId = s.SizeId, SizeName = s.SizeName }));
                });
            }
            catch (Exception ex) { MessageBox.Show($"Lỗi tải dữ liệu: {ex.Message}"); }
        }

        private void LoadItemData(int itemId)
        {
            using var context = new CoffeeShopContext();
            var item = context.Items.Include(i => i.ItemPrices).FirstOrDefault(i => i.ItemId == itemId && !i.IsDeleted);
            if (item == null) return;

            ItemName = item.ItemName;
            CategoryId = item.CategoryId;
            IsAvailable = item.IsAvailable;
            Info = item.Info ?? "";
            ImagePath = string.IsNullOrWhiteSpace(item.ImagePath) ? DEFAULT_IMAGE_PATH : item.ImagePath;

            SizePrices.Clear();
            foreach (var p in item.ItemPrices.Where(ip => !ip.IsDeleted))
            {
                SizePrices.Add(new SizePriceViewModel { PriceId = p.PriceId, SizeId = p.SizeId, Price = p.Price });
            }
        }
        #endregion

        #region Image Handling
        private void LoadImageFromPath(string path)
        {
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;

                if (path.StartsWith("/") || path.StartsWith("pack://"))
                {
                    bitmap.UriSource = new Uri($"pack://application:,,,{path}");
                }
                else
                {
                    string fullPath = Path.IsPathRooted(path) ? path : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
                    if (File.Exists(fullPath))
                    {
                        bitmap.UriSource = new Uri(fullPath);
                    }
                    else
                    {
                        bitmap.UriSource = new Uri($"pack://application:,,,{DEFAULT_IMAGE_PATH}");
                    }
                }

                bitmap.EndInit();
                bitmap.Freeze();
                Image = bitmap;
            }
            catch { /* Log error */ }
        }

        private async Task UploadImageAsync()
        {
            var dialog = new OpenFileDialog { Filter = "Images|*.jpg;*.jpeg;*.png" };
            if (dialog.ShowDialog() != true) return;

            var cropper = new ImageCropperWindow(dialog.FileName);
            if (cropper.ShowDialog() == true)
            {
                await ProcessAndSaveImage(cropper.CroppedImage);
            }
        }

        private async Task ProcessAndSaveImage(BitmapSource croppedResult)
        {
            IsLoading = true;
            LoadingMessage = "Đang xử lý ảnh...";
            try
            {
                croppedResult.Freeze();
                string fileName = $"{Guid.NewGuid()}.jpg";
                string relativePath = Path.Combine("Assets", "Images", "Menu", fileName);
                string absolutePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);

                Directory.CreateDirectory(Path.GetDirectoryName(absolutePath)!);

                await Task.Run(() =>
                {
                    using var stream = new FileStream(absolutePath, FileMode.Create);
                    var encoder = new JpegBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(croppedResult));
                    encoder.Save(stream);
                });

                ImagePath = relativePath;
            }
            catch (Exception ex) { MessageBox.Show($"Lỗi lưu ảnh: {ex.Message}"); }
            finally { IsLoading = false; }
        }
        #endregion

        #region Save Logic
        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(ItemName)) { MessageBox.Show("Tên món không được để trống!"); return; }
            if (SizePrices.Count == 0) { MessageBox.Show("Cần ít nhất một mức giá!"); return; }

            IsLoading = true;
            try
            {
                await Task.Run(() =>
                {
                    using var context = new CoffeeShopContext();
                    Item? item;

                    if (ItemId.HasValue)
                        item = context.Items.Find(ItemId.Value);
                    else
                    {
                        item = new Item();
                        context.Items.Add(item);
                    }

                    if (item == null) return;

                    item.ItemName = ItemName;
                    item.CategoryId = CategoryId;
                    item.IsAvailable = IsAvailable;
                    item.Info = Info;
                    item.ImagePath = ImagePath.Replace("\\", "/");

                    context.SaveChanges();

                    // Cập nhật giá (Soft Delete logic)
                    var oldPrices = context.ItemPrices.Where(p => p.ItemId == item.ItemId).ToList();
                    oldPrices.ForEach(p => p.IsDeleted = true);

                    foreach (var sp in SizePrices)
                    {
                        var existing = oldPrices.FirstOrDefault(p => p.PriceId == sp.PriceId);
                        if (existing != null)
                        {
                            existing.SizeId = sp.SizeId;
                            existing.Price = sp.Price;
                            existing.IsDeleted = false;
                        }
                        else
                        {
                            context.ItemPrices.Add(new ItemPrice { ItemId = item.ItemId, SizeId = sp.SizeId, Price = sp.Price });
                        }
                    }
                    context.SaveChanges();
                    EventAggregator.Instance.Publish(new ItemsChangedMessage());
                });

                DialogResult = true;
            }
            catch (Exception ex) { MessageBox.Show($"Lỗi khi lưu: {ex.Message}"); }
            finally { IsLoading = false; }
        }

        private void AddSize()
        {
            if (CategoryId == FOOD_CATEGORY_ID && SizePrices.Count >= 1) return;

            var usedIds = SizePrices.Select(s => s.SizeId).ToList();
            var nextSize = AvailableSizes.FirstOrDefault(s => !usedIds.Contains(s.SizeId));

            SizePrices.Add(new SizePriceViewModel
            {
                SizeId = CategoryId == FOOD_CATEGORY_ID ? null : nextSize?.SizeId,
                Price = 0
            });
        }

        private void RemoveSize(SizePriceViewModel? item)
        {
            if (item != null) SizePrices.Remove(item);
        }

        private void Cancel() => DialogResult = false;
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
            private int? _sizeId;
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