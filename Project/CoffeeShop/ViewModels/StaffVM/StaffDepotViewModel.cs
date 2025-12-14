using CoffeeShop.Models;
using CoffeeShop.Service;
using CoffeeShop.Service.Interfaces;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
// Thư viện EPPlus
using OfficeOpenXml;
using OfficeOpenXml.Style;
using OfficeOpenXml.Table;

namespace CoffeeShop.ViewModels.StaffVM
{
    public class StaffDepotViewModel : BaseViewModel
    {
        private readonly IDialogService _dialogService; // Dùng để mở các cửa sổ phụ
        // Commands
        public ICommand ApplyFilterCommand { get; private set; } = null!;
        public ICommand ClearFilterCommand { get; private set; } = null!;
        public ICommand AddItemCommand { get; private set; } = null!;
        public ICommand UpdateItemCommand { get; private set; } = null!;
        public ICommand DeleteItemCommand { get; private set; } = null!;
        public ICommand TogglePopupCommand { get; private set; } = null!;
        public ICommand ShowDepotHistory { get; private set; } = null!;
        public ICommand ReportCommand { get; private set; } = null!;


        // Data Collection
        public ObservableCollection<DepotItem> depotItems { get; set; } = new ObservableCollection<DepotItem>();
        public ObservableCollection<string> units { get; set; } = new ObservableCollection<string>()
        {
            "Tất cả", "Kg", "Lon", "Chai", "Hộp", "Hộp 1L", "Hũ"
        };

        #region Properties
        private DepotItem? _selectedItem;
        public DepotItem? SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (_selectedItem != value)
                {
                    _selectedItem = value;
                    OnPropertyChanged();
                    CommandManager.InvalidateRequerySuggested(); // Cập nhật trạng thái CanExecute của các nút (Delete/Update)
                }
            }
        }

        // Thuộc tính binding với TextBox Search
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
                    ApplyFilterCommand.Execute(null);
                }
            }
        }

        // Thuộc tính cho ô lọc số lượng Min/Max
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

        //Constructor
        public StaffDepotViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;
            LoadCommands();
            // Load dữ liệu ban đầu
            LoadDepotItem();
        }

        #region Command Logic
        private bool CanExecuteCrudOperation(DepotItem? selectedItem)
        {
            // Chỉ được thực hiện khi có item được chọn
            return selectedItem != null;
        }

        // Hàm lọc dữ liệu
        private void ExecuteApplyFilter(object? parameter)
        {
            depotItems.Clear();
            try
            {
                using (var db = new CoffeeShopContext())
                {
                    var query = db.Inventories.AsQueryable();

                    // 1. LỌC THEO TÊN (Search Term)
                    if (!string.IsNullOrEmpty(SearchTerm))
                    {
                        string searchLower = SearchTerm.ToLower();
                        query = query.Where(o => o != null && o.MaterialName.ToLower().Contains(searchLower));
                    }

                    // 2. LỌC THEO SỐ LƯỢNG (Min/Max)

                    // Lọc theo Min (Chỉ lọc nếu MinValue > 0)
                    if (MinQuantity.HasValue && MinQuantity > 0)
                    {
                        // Kiểm tra Quantity trong DB >= MinValue
                        query = query.Where(o => o != null && o.Quantity >= MinQuantity);
                    }

                    // Lọc theo Max (Chỉ lọc nếu MaxValue > 0, hoặc MaxValue > MinValue)
                    if (MaxQuantity.HasValue && MaxQuantity > 0)
                    {
                        // Kiểm tra Quantity trong DB <= MaxValue
                        query = query.Where(o => o != null && o.Quantity <= MaxQuantity);
                    }

                    // 3. LỌC THEO ĐƠN VỊ (Unit)
                    if (SelectedUnit.ToLower() != "tất cả")
                    {
                        // Kiểm tra Unit trong DB khớp với SelectedUnit
                        query = query.Where(o => o != null && o.Unit != null && o.Unit.ToLower() == SelectedUnit.ToLower());
                    }

                    // 4. THỰC THI TRUY VẤN VÀ CẬP NHẬT COLLECTION
                    var items = query.ToList();

                    foreach (var item in items)
                    {
                        // Ánh xạ an toàn
                        if (item.IsDeleted) continue; // Bỏ qua các mục đã bị xóa
                        depotItems.Add(new DepotItem
                        {
                            MaterialId = item.MaterialId,
                            MaterialName = item.MaterialName ?? string.Empty,
                            Quantity = item.Quantity,
                            Unit = item.Unit ?? string.Empty,
                            Note = item.Note ?? string.Empty
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                // Hiển thị lỗi nếu có vấn đề DB
                MessageBox.Show($"Lỗi lọc dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Hàm bỏ lọc
        private void ExecuteClearFilter(object? parameter)
        {
            // Reset các thuộc tính input về giá trị mặc định
            SearchTerm = string.Empty;
            MinQuantity = null;
            MaxQuantity = null;

            SelectedUnit = "tất cả"; 

            // Chạy lại bộ lọc để hiển thị toàn bộ dữ liệu
            ExecuteApplyFilter(null);
        }

        // Hàm thêm item mới
        private void ExecuteAddItem(object? parameter)
        {
            // Gọi cửa sổ InsertMaterial ở chế độ Thêm mới
            _dialogService.OpenInsertMaterialWindow(depotItems, null);
        }

        // Hàm cập nhật item
        private void ExecuteUpdateItem(DepotItem? selectedItem)
        {
            // Gọi cửa sổ InsertMaterial ở chế độ Cập nhật
            _dialogService.OpenInsertMaterialWindow(depotItems, selectedItem);
        }

        // Hàm xóa item
        private void ExecuteDeleteItem(DepotItem? itemToDelete)
        {
            if (itemToDelete == null) return;
            // Xác nhận hành động xóa
            if (MessageBox.Show($"Bạn có chắc muốn xóa: {itemToDelete.MaterialName}?", "Xác nhận", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                // Xóa khi người dùng click yes
                using (var db = new CoffeeShopContext())
                {
                    // Lấy item trong db (phải xóa trong db)
                    Inventory itemToDeactivate = db.Inventories.Find(itemToDelete.MaterialId);

                    if (itemToDeactivate != null) // Tim thay item
                    {
                        int actionType = 4; // 4 =  Hủy
                        int staffId = UserSession.Instance.StaffId; // ID nhân viên đang đăng nhập
                        decimal quantityToLog = itemToDeactivate.Quantity; // Luu lai so luong truoc khi xoa
                        try
                        {
                            // Ghi lại hành động vào lịch sử kho
                            InventoryHistory newHistory = new InventoryHistory
                            {
                                MaterialId = itemToDeactivate.MaterialId,
                                ActionTypeId = actionType,
                                Quantity = itemToDeactivate.Quantity,
                                //InputPrice = InputPrice,
                                Date = DateTime.Now,
                                StaffId = staffId
                            };

                            db.InventoryHistories.Add(newHistory);
                            // Soft deleted
                            itemToDeactivate.IsDeleted = true;
                            itemToDeactivate.Quantity = 0;

                            int recordsAffected = db.SaveChanges();

                            if (recordsAffected > 0) // Khi có thay đổi (bị xóa) -> xóa cả dữ liệu trong dg và thông báo
                            {
                                depotItems.Remove(itemToDelete);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Lỗi: {ex.Message}");
                        }
                    }
                }
            }
        }

        // Hàm hỗ trợ bật tắt Popup Filter
        private void ExecuteTogglePopup(object? parameter)
        {
            if (PopupVisibleState == "Collapsed")
            {
                PopupVisibleState = "Visible";
            }
            else
            {
                PopupVisibleState = "Collapsed";
            }
        }
        // Hiển thị lịch sử kho
        private void ExecuteShowHistory(object? parameter)
        {
            _dialogService.OpenDepotHistoryWindow();
        }
        #endregion
        private void ExecuteReport(object? parameter)
        {
            List<DepotItem> reportData;
            string reportPath = string.Empty;

            try
            {
                using (var db = new CoffeeShopContext())
                {
                    var data = db.Inventories
                                .Where(i => i.IsDeleted == false)
                                .Select(i => new DepotItem
                                {
                                    MaterialId = i.MaterialId,
                                    MaterialName = i.MaterialName,
                                    Quantity = i.Quantity,
                                    Unit = i.Unit,
                                    Note = i.Note
                                })
                                .ToList();
                    reportData = data;
                }

                if (reportData.Count == 0)
                {
                    MessageBox.Show("Không có dữ liệu báo cáo.", "Thông báo");
                    return;
                }

                reportPath = CreateExcelReport(reportData); // Tạo file và lấy đường dẫn

                // reportData: Hiển thị dữ liệu preview
                // reportPath: Hỗ trợ gửi file báo cáo
                _dialogService.OpenReportDepotWindow(reportData, reportPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tạo báo cáo: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string CreateExcelReport(List<DepotItem> data)
        {
            ExcelPackage.License.SetNonCommercialPersonal("2G1G Café");

            string fileName = $"BaoCaoKho_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            string filePath = Path.Combine(Path.GetTempPath(), fileName);

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                // Tao work sheet
                var workSheet = package.Workbook.Worksheets.Add("Báo Cáo Kho");
                // true = co header
                workSheet.Cells["A1"].LoadFromCollection(data, true, TableStyles.Medium1);

                
                workSheet.Cells[workSheet.Dimension.Address].AutoFitColumns();
                workSheet.Cells[1, 1, 1, 6].Style.Font.Bold = true;

                // Ví dụ định dạng cột số lượng (giả sử cột số 4 là Quantity)
                workSheet.Column(4).Style.Numberformat.Format = "#,##0.00";

                package.Save();
                return filePath;
            }
        }

        // Load dữ liệu cho dg
        private void LoadDepotItem()
        {
            depotItems.Clear();
            using (var db = new CoffeeShopContext())
            {
                var items = db.Inventories.ToList();
                foreach (var item in items)
                {
                    if (item.IsDeleted) continue; // Bỏ qua các mục đã bị xóa
                    depotItems.Add(new DepotItem
                    {
                        MaterialId = item.MaterialId,
                        MaterialName = item.MaterialName ?? string.Empty,
                        Quantity = item.Quantity,
                        Unit = item.Unit ?? string.Empty,
                        Note = item.Note ?? string.Empty
                    });
                }
            }
        }

        // Load Commands
        private void LoadCommands()
        {
            // 1. COMMANDS LỌC/TÌM KIẾM
            ApplyFilterCommand = new RelayCommand<object>(ExecuteApplyFilter);
            ClearFilterCommand = new RelayCommand<object>(ExecuteClearFilter);

            // 2. COMMANDS CRUD
            AddItemCommand = new RelayCommand<object>(ExecuteAddItem);
            DeleteItemCommand = new RelayCommand<DepotItem>(ExecuteDeleteItem, CanExecuteCrudOperation);
            UpdateItemCommand = new RelayCommand<DepotItem>(ExecuteUpdateItem, CanExecuteCrudOperation);

            TogglePopupCommand = new RelayCommand<object>(ExecuteTogglePopup);
            ShowDepotHistory = new RelayCommand<object>(ExecuteShowHistory);
            ReportCommand = new RelayCommand<object>(ExecuteReport);
        }
    }
}
