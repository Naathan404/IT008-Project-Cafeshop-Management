using CoffeeShop.Models;
using CoffeeShop.Service.Interfaces;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace CoffeeShop.ViewModels.StaffVM
{
    public class StaffDepotViewModel : BaseViewModel
    {
        private readonly IDialogService _dialogService; // Dùng để mở cửa sổ InsertMaterial
        // Commands
        public ICommand ApplyFilterCommand { get; private set; } = null!;
        public ICommand ClearFilterCommand { get; private set; } = null!;
        public ICommand AddItemCommand { get; private set; } = null!;
        public ICommand UpdateItemCommand { get; private set; } = null!;
        public ICommand DeleteItemCommand { get; private set; } = null!;
        public ICommand TogglePopupCommand { get; private set; } = null!;
        public ICommand ShowDepotHistory { get; private set; } = null!;

        // Class DepotItem
        public class DepotItem : BaseViewModel
        {
            // Backing field (nơi lưu giá trị thật sự) 
            private int _materialId;
            private string? _materialName;
            private decimal _quantity;
            private string _unit;
            private string? _note;

            // --- CÁC PROPERTY - Khi có sự thay đổi mới gián giá trị mới cho backing field ---
            public int MaterialId { get; set; } /// Ko có sự thay đổi ID nên ko cần định nghĩa
            // 1. MaterialName
            public string? MaterialName
            {
                get => _materialName;
                set
                {
                    if (_materialName != value)
                    {
                        _materialName = value;
                        OnPropertyChanged();
                    }
                }
            }

            // 2. Quantity
            public decimal Quantity
            {
                get => _quantity;
                set
                {
                    if (_quantity != value)
                    {
                        _quantity = value;
                        OnPropertyChanged();
                    }
                }
            }

            // 3. Unit
            public string Unit
            {
                get => _unit;
                set
                {
                    if (_unit != value)
                    {
                        _unit = value;
                        OnPropertyChanged();
                    }
                }
            }

            // 4. Note
            public string? Note
            {
                get => _note;
                set
                {
                    if (_note != value)
                    {
                        _note = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

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
                    Inventory deletedItem = db.Inventories.Find(itemToDelete.MaterialId);

                    if (deletedItem != null) // Tim thay item
                    {
                        const int ACTION_TYPE_NHAP = 4; // 4 =  Hủy
                        const int STAFF_ID = 1; // ID nhân viên đang đăng nhập
                        using (var transaction = db.Database.BeginTransaction()) // Transaction - All or nothing
                        {
                            try
                            {
                                db.Inventories.Remove(deletedItem);
                                // Lưu thay đổi -> chính thức bị xóa
                                int recordsAffected = db.SaveChanges();
                                
                                // Ghi lại hành động cập nhật vào lịch sử kho
                                InventoryHistory newHistory = new InventoryHistory
                                {
                                    MaterialId = deletedItem.MaterialId,
                                    ActionTypeId = ACTION_TYPE_NHAP,
                                    Quantity = deletedItem.Quantity,
                                    //InputPrice = InputPrice,
                                    Date = DateTime.Now,
                                    StaffId = STAFF_ID
                                };

                                db.InventoryHistories.Add(newHistory);
                                db.SaveChanges(); // Lưu bản ghi lịch sử

                                transaction.Commit(); // Hoàn tất cả hai

                                if (recordsAffected > 0) // Khi có thay đổi (bị xóa) -> xóa cả dữ liệu trong dg và thông báo
                                {
                                    depotItems.Remove(itemToDelete);
                                    MessageBox.Show($"Đã xóa thành công {itemToDelete.MaterialName} khỏi DB!", "Thành công");
                                }

                                depotItems.Remove(itemToDelete);
                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback(); // Hủy bỏ cả hai
                                MessageBox.Show($"Lỗi: {ex.Message}");
                            }
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

        private void ExecuteShowHistory(object? parameter)
        {
            _dialogService.OpenDepotHistoryWindow();
        }
        #endregion

        // Load dữ liệu cho dg
        private void LoadDepotItem()
        {
            depotItems.Clear();
            using (var db = new CoffeeShopContext())
            {
                var items = db.Inventories.ToList();
                foreach (var item in items)
                {
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
        }
    }
}
