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
        private List<DepotItemDTO> _allDepotItems = new List<DepotItemDTO>();

        private ObservableCollection<DepotItemDTO> _depotItems = new ObservableCollection<DepotItemDTO>();
        public ObservableCollection<DepotItemDTO> DepotItems
        {
            get => _depotItems;
            set { _depotItems = value; OnPropertyChanged(); }
        }

        private List<DepotHistoryItemDTO> _allHistoryRecord = new List<DepotHistoryItemDTO>(); // Danh sách gốc từ DB

        private ObservableCollection<DepotHistoryItemDTO> _depotHistoryItems = new ObservableCollection<DepotHistoryItemDTO>();
        public ObservableCollection<DepotHistoryItemDTO> DepotHistoryItems
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
                        var filtered = _allHistoryRecord
                            .Where(h => h.MaterialId == _selectedItem.MaterialId)
                            .ToList();

                        DepotHistoryItems = new ObservableCollection<DepotHistoryItemDTO>(filtered);
                    }
                    else
                    {
                        DepotHistoryItems = new ObservableCollection<DepotHistoryItemDTO>();
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
            _ = LoadHistory();
        }

        #region Core Logic
        public async Task LoadItem()
        {
            try
            {
                using (var db = new CoffeeShopContext())
                {
                    var items = await db.Inventories.AsNoTracking()
                                        .Where(i => !i.IsDeleted).ToListAsync();

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
                        ApplyFilter();
                        OnPropertyChanged(nameof(DepotItems));
                    });
                }
            }
            catch (Exception ex) { MessageBox.Show($"Lỗi: {ex.Message}"); }
        }

        public async Task LoadHistory(int? forcedId = null)
        {
            int? idToFilter = forcedId ?? SelectedItem?.MaterialId;

            try
            {
                using (var db = new CoffeeShopContext())
                {
                    var historyItems = await db.InventoryHistories
                        .AsNoTracking()
                        .Include(h => h.ActionType)
                        .Include(h => h.Staff)
                        .OrderByDescending(h => h.Date)
                        .ToListAsync();

                    var result = historyItems.Select(h => new DepotHistoryItemDTO
                    {
                        MaterialId = h.MaterialId,
                        MaterialName = h.Material?.MaterialName ?? "N/A",
                        ActionName = h.ActionType?.ActionName ?? "Không xác định",
                        Quantity = h.Quantity,
                        Date = h.Date,
                        StaffName = h.Staff?.StaffName ?? "Ẩn danh"
                    }).ToList();

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        _allHistoryRecord = result;

                        if (idToFilter != null)
                        {
                            var filtered = _allHistoryRecord.Where(h => h.MaterialId == idToFilter).ToList();
                            DepotHistoryItems = new ObservableCollection<DepotHistoryItemDTO>(filtered);
                        }
                        OnPropertyChanged(nameof(DepotHistoryItems));
                    });
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
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

            DepotItems = new ObservableCollection<DepotItemDTO>(filtered);
        }
        #endregion

        #region Commands Execution
        private async void ExecuteAddItem(object? parameter)
        {
            if (_dialogService.OpenInsertMaterialWindow(DepotItems, null) == true)
            {
                await LoadItem();
                await LoadHistory();
            }
        }

        private async void ExecuteUpdateItem(DepotItemDTO? selectedItem)
        {
            if (selectedItem == null) return;
            int savedId = selectedItem.MaterialId;

            if (_dialogService.OpenInsertMaterialWindow(DepotItems, selectedItem) == true)
            {
                await LoadItem();
                SelectedItem = null;
                var newItem = DepotItems.FirstOrDefault(x => x.MaterialId == savedId);
                SelectedItem = newItem;
                await LoadHistory(savedId);

                OnPropertyChanged(nameof(SelectedItem));
            }
        }

        private async void ExecuteDeleteItem(DepotItemDTO? itemToDelete)
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
                        await db.SaveChangesAsync();
                        await LoadItem();
                        await LoadHistory();
                    }
                }
            }
        }

        private void ExecuteReport(object? parameter)
        {
            if (DepotItems.Count == 0) { MessageBox.Show("Không có dữ liệu."); return; }

            _dialogService.OpenReportDepotWindow();
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