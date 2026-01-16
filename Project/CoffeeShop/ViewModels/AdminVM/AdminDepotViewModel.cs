using CoffeeShop.Models;
using CoffeeShop.Service;
using CoffeeShop.Service.DTOs;
using CoffeeShop.Service.Interfaces;
using CoffeeShop.View.Staff;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Table;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace CoffeeShop.ViewModels.AdminVM
{
    public class AdminDepotViewModel : BaseViewModel
    {
        private readonly IDialogService _dialogService;

        // Commands
        public ICommand ReloadCommand { get; private set; } = null!;
        public ICommand AddItemCommand { get; private set; } = null!;
        public ICommand UpdateItemCommand { get; private set; } = null!;
        public ICommand DeleteItemCommand { get; private set; } = null!;
        public ICommand ShowDepotHistory { get; private set; } = null!;
        public ICommand ExportFileCommand { get; private set; } = null!;

        #region Properties
        // --- QUAN TRỌNG: Danh sách gốc lưu trên RAM để lọc không lag ---
        private List<DepotItemDTO> _allDepotItems = new List<DepotItemDTO>();

        // Danh sách hiển thị trên DataGrid
        private ObservableCollection<DepotItemDTO> _depotItems = new ObservableCollection<DepotItemDTO>();
        public ObservableCollection<DepotItemDTO> DepotItems
        {
            get => _depotItems;
            set { _depotItems = value; OnPropertyChanged(); }
        }

        private List<DepotHistoryItemDTO> _depotHistoryItems = new List<DepotHistoryItemDTO>();
        public List<DepotHistoryItemDTO> DepotHistoryItems
        {
            get => _depotHistoryItems;
            set { _depotHistoryItems = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> units { get; set; } = new ObservableCollection<string>()
        {
            "Tất cả", "Kg", "Lon", "Chai", "Hộp", "Hộp 1L", "Hũ"
        };

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
                    if (_selectedItem != null)
                    {
                        DepotHistoryItems = _depotHistoryItems
                            .Where(h => h.MaterialId == value.MaterialId)
                            .ToList();
                    }
                    else
                    {
                        DepotHistoryItems = new List<DepotHistoryItemDTO>();
                    }
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
                    ApplyFilter(); // Lọc trực tiếp trên RAM
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
        #endregion

        // Constructor
        public AdminDepotViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;
            LoadCommands();
            _ = LoadItem();
        }

        #region Core Logic
        public async Task LoadItem()
        {
            // Reset các field (dùng field trực tiếp để tránh kích hoạt ApplyFilter 4 lần)
            _searchTerm = string.Empty;
            _selectedUnit = "Tất cả";
            _minQuantity = null;
            _maxQuantity = null;

            OnPropertyChanged(nameof(SearchTerm));
            OnPropertyChanged(nameof(SelectedUnit));
            OnPropertyChanged(nameof(MinQuantity));
            OnPropertyChanged(nameof(MaxQuantity));

            // Load dữ liệu cho nguyên liệu trong kho
            try
            {
                using (var db = new CoffeeShopContext())
                {
                    var items = await db.Inventories.Where(i => !i.IsDeleted).ToListAsync();

                    var result = items.Select(item => new DepotItemDTO
                    {
                        MaterialId = item.MaterialId,
                        MaterialName = item.MaterialName ?? string.Empty,
                        Quantity = item.Quantity,
                        Unit = item.Unit ?? string.Empty,
                        Note = item.Note ?? string.Empty,
                        Threshold = item.Threshold
                    }).ToList();

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        _allDepotItems = result;
                        ApplyFilter(); // Đổ dữ liệu ra bảng
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải nguyên liệu: {ex.Message}");
            }
        }

        public async Task LoadHistory()
        {
            try
            {
                using (var db = new CoffeeShopContext())
                {
                    var historyItems = await db.InventoryHistories
                        .Include(h => h.ActionType)
                        .Include(h => h.Staff)
                        .OrderByDescending(h => h.Date)
                        .ToListAsync();

                    _depotHistoryItems = historyItems.Select(h => new DepotHistoryItemDTO
                    {
                        MaterialId = h.MaterialId,
                        ActionName = h.ActionType?.ActionName ?? "Không xác định",
                        Quantity = h.Quantity,
                        Date = h.Date,
                        StaffName = h.Staff?.StaffName ?? "Ẩn danh"
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải lịch sử: {ex.Message}");
            }
        }

        public void ApplyFilter()
        {
            var filtered = _allDepotItems.AsEnumerable();

            // Viết gọn y chang AdminDiscount
            if (!string.IsNullOrEmpty(SearchTerm))
                filtered = filtered.Where(x => x.MaterialName.ToLower().Contains(SearchTerm.ToLower()));

            if (SelectedUnit != null && SelectedUnit != "Tất cả")
                filtered = filtered.Where(x => x.Unit == SelectedUnit);

            if (MinQuantity.HasValue && MinQuantity > 0)
                filtered = filtered.Where(x => x.Quantity >= MinQuantity.Value);

            if (MaxQuantity.HasValue && MaxQuantity > 0)
                filtered = filtered.Where(x => x.Quantity <= MaxQuantity.Value);

            DepotItems = new ObservableCollection<DepotItemDTO>(filtered);
        }
        #endregion

        #region Commands Execution
        private void ExecuteAddItem(object? parameter)
        {
            if (_dialogService.OpenInsertMaterialWindow(DepotItems, null) == true)
                _ = LoadItem();
        }

        private void ExecuteUpdateItem(DepotItemDTO? selectedItem)
        {
            if (selectedItem == null) return;
            if (_dialogService.OpenInsertMaterialWindow(DepotItems, selectedItem) == true)
                _ = LoadItem();
        }

        private void ExecuteDeleteItem(DepotItemDTO? itemToDelete)
        {
            if (itemToDelete == null) return;
            if (MessageBox.Show($"Xóa: {itemToDelete.MaterialName}?", "Xác nhận", MessageBoxButton.YesNoCancel) == MessageBoxResult.Yes)
            {
                using (var db = new CoffeeShopContext())
                {
                    var itemInDb = db.Inventories.Find(itemToDelete.MaterialId);
                    if (itemInDb != null)
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
                        db.SaveChanges();
                        _ = LoadItem();
                    }
                }
            }
        }

        private void ExecuteReport(object? parameter)
        {
            if (DepotItems.Count == 0) { MessageBox.Show("Không có dữ liệu."); return; }

            string path = CreateExcelReport(DepotItems.ToList());
            _dialogService.OpenReportDepotWindow();
        }

        private string CreateExcelReport(List<DepotItemDTO> data)
        {
            ExcelPackage.License.SetNonCommercialPersonal("2G1G Café");
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"BaoCaoKho_{DateTime.Now:yyyyMMdd}.xlsx");

            using (var package = new ExcelPackage(new FileInfo(path)))
            {
                var ws = package.Workbook.Worksheets.Add("Báo Cáo");
                ws.Cells["A1"].LoadFromCollection(data, true, TableStyles.Medium1);
                ws.Cells.AutoFitColumns();
                package.Save();
                return path;
            }
        }
        #endregion

        private void LoadCommands()
        {
            ReloadCommand = new RelayCommand<object>(p => _ = LoadItem());
            AddItemCommand = new RelayCommand<object>(ExecuteAddItem);
            DeleteItemCommand = new RelayCommand<DepotItemDTO>(ExecuteDeleteItem);
            UpdateItemCommand = new RelayCommand<DepotItemDTO>(ExecuteUpdateItem);
            ShowDepotHistory = new RelayCommand<object>(p => _dialogService.OpenDepotHistoryWindow());
            ExportFileCommand = new RelayCommand<object>(ExecuteReport);
        }
    }
}