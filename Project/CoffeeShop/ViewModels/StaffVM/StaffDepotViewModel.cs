using CoffeeShop.Models;
using CoffeeShop.Service;
using CoffeeShop.Service.DTOs;
using CoffeeShop.Service.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace CoffeeShop.ViewModels.StaffVM
{
    public class StaffDepotViewModel : BaseViewModel
    {
        private readonly IDialogService _dialogService;

        // Commands
        public ICommand ReloadCommand { get; private set; } = null!;
        public ICommand AddItemCommand { get; private set; } = null!;
        public ICommand UpdateItemCommand { get; private set; } = null!;
        public ICommand DeleteItemCommand { get; private set; } = null!;
        public ICommand ShowDepotHistory { get; private set; } = null!;
        public ICommand ReportCommand { get; private set; } = null!;

        // Data Collection
        // 1. Danh sách gốc lưu trên RAM (Giống AllCoupons)
        private List<DepotItemDTO> _allDepotItems = new List<DepotItemDTO>();

        // 2. Danh sách hiển thị (Binding với DataGrid)
        private ObservableCollection<DepotItemDTO> _depotItems = new ObservableCollection<DepotItemDTO>();
        public ObservableCollection<DepotItemDTO> depotItems
        {
            get => _depotItems;
            set { _depotItems = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> Units { get; set; } = new ObservableCollection<string>()
        {
            "Tất cả", "Kg", "Lon", "Chai", "Hộp", "Hộp 1L", "Hũ"
        };

        #region Properties
        private DepotItemDTO? _selectedItem;
        public DepotItemDTO? SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (_selectedItem != value)
                {
                    _selectedItem = value;
                    OnPropertyChanged();
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        private string _searchTerm = string.Empty;
        public string SearchTerm
        {
            get => _searchTerm;
            set
            {
                if (_searchTerm != value)
                {
                    _searchTerm = value;
                    OnPropertyChanged();
                    ApplyFilter(); // Gọi lọc trực tiếp trên RAM, không gọi DB
                }
            }
        }

        private decimal? _minQuantity;
        public decimal? MinQuantity
        {
            get => _minQuantity;
            set
            {
                if (_minQuantity != value)
                {
                    _minQuantity = value;
                    OnPropertyChanged();
                    ApplyFilter();
                }
            }
        }

        private decimal? _maxQuantity;
        public decimal? MaxQuantity
        {
            get => _maxQuantity;
            set
            {
                if (_maxQuantity != value)
                {
                    _maxQuantity = value;
                    OnPropertyChanged();
                    ApplyFilter();
                }
            }
        }

        private string? _selectedUnit = "Tất cả";
        public string? SelectedUnit
        {
            get => _selectedUnit;
            set
            {
                if (_selectedUnit != value)
                {
                    _selectedUnit = value;
                    OnPropertyChanged();
                    ApplyFilter();
                }
            }
        }

        private string _popupVisibleState = "Collapsed";
        public string PopupVisibleState
        {
            get => _popupVisibleState;
            set
            {
                if (_popupVisibleState != value)
                {
                    _popupVisibleState = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        public StaffDepotViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;
            LoadCommands();
            _ = LoadData();
        }

        #region Command Logic
        public async Task LoadData()
        {
            SearchTerm = string.Empty;
            MinQuantity = null;
            MaxQuantity = null;
            SelectedUnit = Units.First();
            try
            {
                using (var db = new CoffeeShopContext())
                {
                    // Lấy dữ liệu bất đồng bộ (giống Admin)
                    var dataFromDb = await db.Inventories.Where(i => !i.IsDeleted).ToListAsync();

                    var result = dataFromDb.Select(item => new DepotItemDTO
                    {
                        MaterialId = item.MaterialId,
                        MaterialName = item.MaterialName ?? string.Empty,
                        Quantity = item.Quantity,
                        Unit = item.Unit ?? string.Empty,
                        Note = item.Note ?? string.Empty
                    }).ToList();

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        _allDepotItems = result; // Cất vào RAM
                        ApplyFilter(); // Đổ ra UI
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi load data: {ex.Message}");
            }
        }

        public void ApplyFilter()
        {
            var filtered = _allDepotItems.AsEnumerable();

            if (!string.IsNullOrEmpty(SearchTerm))
                filtered = filtered.Where(x => x.MaterialName.ToLower().Contains(SearchTerm.ToLower()));

            if (SelectedUnit != null && SelectedUnit != "Tất cả")
                filtered = filtered.Where(x => x.Unit == SelectedUnit);

            if (MinQuantity.HasValue && MinQuantity > 0)
                filtered = filtered.Where(x => x.Quantity >= MinQuantity.Value);

            if (MaxQuantity.HasValue && MaxQuantity > 0)
                filtered = filtered.Where(x => x.Quantity <= MaxQuantity.Value);

            // Bỏ .ToList(), gán thẳng y chang bên Discount
            depotItems = new ObservableCollection<DepotItemDTO>(filtered);
        }

        private void ExecuteAddItem(object? parameter)
        {
            if (_dialogService.OpenInsertMaterialWindow(depotItems, null) == true)
            {
                _ = LoadData();
            }
        }

        private void ExecuteUpdateItem(DepotItemDTO? selectedItem)
        {
            if (_dialogService.OpenInsertMaterialWindow(depotItems, selectedItem) == true)
            {
                _ = LoadData();
            }
        }

        private void ExecuteDeleteItem(DepotItemDTO? itemToDelete)
        {
            if (itemToDelete == null) return;
            if (MessageBox.Show($"Bạn có chắc muốn xóa: {itemToDelete.MaterialName}?", "Xác nhận", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                using (var db = new CoffeeShopContext())
                {
                    var itemInDb = db.Inventories.Find(itemToDelete.MaterialId);
                    if (itemInDb != null)
                    {
                        try
                        {
                            db.InventoryHistories.Add(new InventoryHistory
                            {
                                MaterialId = itemInDb.MaterialId,
                                ActionTypeId = 4,
                                Quantity = itemInDb.Quantity,
                                Date = DateTime.Now,
                                StaffId = UserSession.Instance.StaffId
                            });
                            itemInDb.IsDeleted = true;
                            itemInDb.Quantity = 0;
                            db.SaveChanges();
                            _ = LoadData();
                        }
                        catch (Exception ex) { MessageBox.Show($"Lỗi: {ex.Message}"); }
                    }
                }
            }
        }

        private void ExecuteShowHistory(object? parameter) => _dialogService.OpenDepotHistoryWindow();
        private void ExecuteReport(object? parameter) => _dialogService.OpenReportDepotWindow();
        #endregion

        private void LoadCommands()
        {
            ReloadCommand = new RelayCommand<object>(p => _ = LoadData());
            AddItemCommand = new RelayCommand<object>(ExecuteAddItem);
            DeleteItemCommand = new RelayCommand<DepotItemDTO>(ExecuteDeleteItem, p => p != null);
            UpdateItemCommand = new RelayCommand<DepotItemDTO>(ExecuteUpdateItem, p => p != null);
            ShowDepotHistory = new RelayCommand<object>(ExecuteShowHistory);
            ReportCommand = new RelayCommand<object>(ExecuteReport);
        }
    }
}