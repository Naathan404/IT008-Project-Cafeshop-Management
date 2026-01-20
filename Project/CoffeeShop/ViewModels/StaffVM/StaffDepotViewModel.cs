using CoffeeShop.Helper;
using CoffeeShop.Models;
using CoffeeShop.Service;
using CoffeeShop.Service.DTOs;
using CoffeeShop.Service.Interfaces;
using CoffeeShop.View.Controls;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace CoffeeShop.ViewModels.StaffVM
{
    public class StaffDepotViewModel : BaseViewModel
    {
        private readonly IDialogService _dialogService;

        public ICommand ReloadCommand { get; private set; } = null!;
        public ICommand AddItemCommand { get; private set; } = null!;
        public ICommand UpdateItemCommand { get; private set; } = null!;
        //public ICommand DeleteItemCommand { get; private set; } = null!;
        public ICommand ReportCommand { get; private set; } = null!;

        #region Properties
        private List<DepotItemDTO> _allDepotItems = new List<DepotItemDTO>();
        private ObservableCollection<DepotItemDTO> _depotItems = new ObservableCollection<DepotItemDTO>();
        public ObservableCollection<DepotItemDTO> DepotItems
        {
            get => _depotItems;
            set { _depotItems = value; OnPropertyChanged(); }
        }

        private List<DepotHistoryItemDTO> _allHistoryRecord = new List<DepotHistoryItemDTO>();
        private ObservableCollection<DepotHistoryItemDTO> _depotHistoryItems = new ObservableCollection<DepotHistoryItemDTO>();
        public ObservableCollection<DepotHistoryItemDTO> DepotHistoryItems
        {
            get => _depotHistoryItems;
            set { _depotHistoryItems = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> Units { get; set; } = new ObservableCollection<string>()
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
                    ApplyFilter();
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

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }
        #endregion

        // Constructor
        public StaffDepotViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;
            LoadCommands();
            _ = LoadData();
            _ = LoadHistory();
        }

        #region Logic
        public async Task LoadData()
        {
            MinQuantity = null;
            MaxQuantity = null;
            SelectedUnit = Units.First();
            SearchTerm = string.Empty;
            IsLoading = true;

            try
            {
                using (var db = new CoffeeShopContext())
                {
                    var dataFromDb = await db.Inventories.AsNoTracking().Where(i => !i.IsDeleted).ToListAsync();
                    var result = dataFromDb.Select(item => new DepotItemDTO
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
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", CustomMessageBox.MessageButtons.OK, CustomMessageBox.MessageType.Error);
            }
            finally
            {
                IsLoading = false;
            }
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
            catch (Exception ex)
            { 
                CustomMessageBox.Show(ex.Message, "Lỗi", CustomMessageBox.MessageButtons.OK, CustomMessageBox.MessageType.Error);
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

            DepotItems = new ObservableCollection<DepotItemDTO>(filtered);
        }
        #endregion

        #region Execute Methods
        private async void ExecuteAddItem(object? parameter)
        {
            if (_dialogService.OpenInsertMaterialWindow(DepotItems, null) == true)
            {
                await LoadData();
                await LoadHistory();
            }
        }

        private async void ExecuteUpdateItem(DepotItemDTO? selectedItem)
        {
            if (selectedItem == null) return;
            int savedId = selectedItem.MaterialId;
            if (_dialogService.OpenInsertMaterialWindow(DepotItems, selectedItem) == true)
            {
                await LoadData();
                await LoadHistory(savedId);
                SelectedItem = DepotItems.FirstOrDefault(x => x.MaterialId == savedId);
                OnPropertyChanged(nameof(SelectedItem));
            }
        }
        #endregion
        private async void SendReport(string filePath)
        {
            // Email Subject
            string senderName = UserSession.Instance.StaffName ?? "Nhân viên";
            string senderId = UserSession.Instance.StaffId.ToString();
            string sendTime = DateTime.Now.ToString("HH:mm:ss - dd/MM/yyyy");
            string subject = $"BAO_CAO_KHO_HANG_{DateTime.Now:dd-MM-yyyy}";
            string body = $@"THÔNG BÁO GỬI BÁO CÁO KHO HÀNG
            ----------------------------------------------
            Người gửi: {senderName} (ID: {senderId})
            Thời gian gửi: {sendTime}
            Nội dung: Vui lòng xem file báo cáo kho hàng đính kèm để biết chi tiết tình trạng nguyên vật liệu trong kho.

            Đây là email tự động từ hệ thống quản lý CoffeeShop 2G1G.";
            string toEmail;
            try
            {
                using (var db = new CoffeeShopContext())
                {
                    var admins = db.Staff.Where(p => p != null && p.StaffRole == "Admin");
                    foreach (var admin in admins)
                    {
                        if (admin == null || string.IsNullOrEmpty(admin.Email))
                        {
                            CustomMessageBox.Show("Không tìm thấy email người dùng.", "Lỗi", CustomMessageBox.MessageButtons.OK, CustomMessageBox.MessageType.Error);
                            return;
                        }
                        else
                        {
                            toEmail = admin.Email;
                            await MailUtils.SendEmailAsync(toEmail, subject, body, filePath);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show($"Gửi báo cáo thất bại. Lỗi: {ex.Message}", "Lỗi", CustomMessageBox.MessageButtons.OK, CustomMessageBox.MessageType.Error);
                return;
            }
            finally
            {
                // Xóa file sau khi gửi
                if (File.Exists(filePath))
                {
                    try
                    {
                        File.Delete(filePath);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Không thể xóa file tạm: {ex.Message}");
                    }
                }
            }

        }
        private void LoadCommands()
        {
            ReloadCommand = new RelayCommand<object>(p => _ = LoadData());
            AddItemCommand = new RelayCommand<object>(ExecuteAddItem);
            //DeleteItemCommand = new RelayCommand<DepotItemDTO>(ExecuteDeleteItem, p => p != null);
            UpdateItemCommand = new RelayCommand<DepotItemDTO>(ExecuteUpdateItem, p => p != null);
            ReportCommand = new RelayCommand<object>(async (p) =>
            {
                var result = CustomMessageBox.Show("Xác nhận gửi báo cáo?", "Xác nhận", CustomMessageBox.MessageButtons.YesNo, CustomMessageBox.MessageType.Question);
                if (result != CustomMessageBox.MessageBoxResult.Yes) return;

                if (DepotItems.Count == 0 || DepotItems == null)
                {
                    CustomMessageBox.Show("Không có dữ liệu.", "Thông báo", CustomMessageBox.MessageButtons.OK, CustomMessageBox.MessageType.Info);
                }
                else
                {
                    // Khởi tạo hóa đơn
                    string fileName = $"BAO_CAO_LICH_SU_BAN_HANG_{DateTime.Now:dd-MM-yyyy_HH-mm}.xlsx";
                    string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Exports");
                    if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                    string fullPath = Path.Combine(folderPath, fileName);

                    var exporter = new ReportExporter(DepotItems);
                    await exporter.ExportDepotItem(fullPath);

                    // Gửi file qua mail và xóa file ở máy
                    SendReport(fullPath);
                }
            });
        }
    }
}