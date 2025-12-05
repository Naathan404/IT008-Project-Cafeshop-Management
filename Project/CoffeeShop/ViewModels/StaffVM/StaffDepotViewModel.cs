using CoffeeShop.Models;
using CoffeeShop.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static CoffeeShop.ViewModels.StaffVM.StaffDepotViewModel;

namespace CoffeeShop.ViewModels.StaffVM
{
    public class StaffDepotViewModel : BaseViewModel
    {
        private readonly IDialogService _dialogService;

        public class DepotItem : BaseViewModel
        {
            // Backing field (nơi lưu giá trị thật sự) 
            private int _materialId;
            private string? _materialName;
            private decimal _quantity;
            private string _unit;
            private string? _note;

            // --- CÁC PROPERTY - Khi có sự thay đổi mới gián giá trị mới cho backing field ---
            public int MaterialId { get; set; } /// KO có sự thay đổi ID nên ko cần định nghĩa
            // 1. MaterialName
            public string? MaterialName
            {
                get => _materialName;
                set
                {
                    if (_materialName != value) // Chỉ thông báo nếu giá trị thực sự thay đổi
                    {
                        _materialName = value;
                        OnPropertyChanged(); // Gọi hàm thông báo
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
                        OnPropertyChanged(); // Gọi hàm thông báo
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

        public ObservableCollection<DepotItem> depotItems { get; set; } = new ObservableCollection<DepotItem>();

        // Dùng cho Binding SelectedItem của DataGrid (Input)
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
                    // CommandManager.InvalidateRequerySuggested(); // Cập nhật trạng thái CanExecute của các nút (Delete/Update)
                }
            }
        }

        // --- 2. INPUT VÀ BỘ LỌC (Input) ---

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

        // Thuộc tính cho ô lọc số lượng Min/Max (Dùng decimal cho an toàn)
        public decimal MinQuantity { get; set; }
        public decimal MaxQuantity { get; set; }

        // Thuộc tính cho ComboBox Đơn vị
        public string? SelectedUnit { get; set; }

        // --- 3. COMMANDS (Hành động) ---

        // Command cho các nút chức năng
        public ICommand ApplyFilterCommand { get; private set; }
        public ICommand ClearFilterCommand { get; private set; }
        public ICommand AddItemCommand { get; private set; }
        public ICommand UpdateItemCommand { get; private set; }
        public ICommand DeleteItemCommand { get; private set; }

        public StaffDepotViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;
            // 1. COMMANDS LỌC/TÌM KIẾM (Thường không cần CanExecute)
            ApplyFilterCommand = new RelayCommand<object>(ExecuteApplyFilter);
            ClearFilterCommand = new RelayCommand<object>(ExecuteClearFilter);

            // 2. COMMANDS CRUD (Cần CanExecute để kiểm tra SelectedItem)
            AddItemCommand = new RelayCommand<object>(ExecuteAddItem);
            // Delete và Update chỉ hoạt động khi có item được chọn
            DeleteItemCommand = new RelayCommand<DepotItem>(ExecuteDeleteItem, CanExecuteCrudOperation);
            UpdateItemCommand = new RelayCommand<DepotItem>(ExecuteUpdateItem, CanExecuteCrudOperation);

            // Load dữ liệu ban đầu
            LoadDepotItem();
        }

        public void LoadDepotItem()
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


        private bool CanExecuteCrudOperation(DepotItem? selectedItem)
        {
            // Nút chỉ được bật nếu SelectedItem không phải là null
            return selectedItem != null;
        }

        // 1. ÁP DỤNG BỘ LỌC
        private void ExecuteApplyFilter(object? parameter)
        {
            decimal minFilterValue = MinQuantity;
            decimal maxFilterValue = MaxQuantity;
            string currentUnit = SelectedUnit ?? "All";

            // Reset DataGrid
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
                    if (minFilterValue > 0)
                    {
                        // Kiểm tra Quantity trong DB >= MinValue
                        query = query.Where(o => o != null && o.Quantity >= minFilterValue);
                    }

                    // Lọc theo Max (Chỉ lọc nếu MaxValue > 0, hoặc MaxValue > MinValue)
                    if (maxFilterValue > 0)
                    {
                        // Kiểm tra Quantity trong DB <= MaxValue
                        query = query.Where(o => o != null && o.Quantity <= maxFilterValue);
                    }

                    // 3. LỌC THEO ĐƠN VỊ (Unit)
                    if (currentUnit.ToLower() != "all")
                    {
                        // Kiểm tra Unit trong DB khớp với SelectedUnit
                        query = query.Where(o => o != null && o.Unit != null && o.Unit.ToLower() == currentUnit.ToLower());
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

        // 2. BỎ LỌC
        private void ExecuteClearFilter(object? parameter)
        {
            // Reset các thuộc tính input về giá trị mặc định
            SearchTerm = string.Empty;
            MinQuantity = 0;
            MaxQuantity = 0;

            // Buộc SelectedUnit phải được reset thủ công
            // SelectedUnit = "All"; 

            // Chạy lại bộ lọc để hiển thị toàn bộ dữ liệu
            ExecuteApplyFilter(null);
        }

        private void ExecuteAddItem(object? parameter)
        {
            // Gọi cửa sổ InputWindow ở chế độ Thêm mới
            _dialogService.OpenInputWindow(depotItems, null);
        }

        // 4. CẬP NHẬT
        private void ExecuteUpdateItem(DepotItem? selectedItem)
        {
            if (selectedItem == null) return;

            // Gọi cửa sổ InputWindow ở chế độ Cập nhật
            _dialogService.OpenInputWindow(depotItems, selectedItem);
        }

        // 5. XOÁ
        private void ExecuteDeleteItem(DepotItem? itemToDelete)
        {
            if (itemToDelete == null) return;
            // Hỏi lại 
            if (MessageBox.Show($"Bạn có chắc muốn xóa: {itemToDelete.MaterialName}?", "Xác nhận", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                // Xóa khi người dùng click yes
                using (var db = new CoffeeShopContext())
                {
                    // Lấy item trong db (phải xóa trong db)
                    Inventory deletedItem = db.Inventories.Find(itemToDelete.MaterialId);

                    if (deletedItem != null) // Tim thay item
                    {
                        db.Inventories.Remove(deletedItem);
                        // Lưu thay đổi -> chính thức bị xóa
                        int recordsAffected = db.SaveChanges();

                        if (recordsAffected > 0) // Khi có thay đổi (bị xóa) -> xóa cả dữ liệu trong dg và thông báo
                        {
                            depotItems.Remove(itemToDelete);
                            MessageBox.Show($"Đã xóa thành công {itemToDelete.MaterialName} khỏi DB!", "Thành công");
                        }
                    }
                    depotItems.Remove(itemToDelete);
                }
            }
        }
    }
}
