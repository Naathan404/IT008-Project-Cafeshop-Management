using CoffeeShop.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;

namespace CoffeeShop.ViewModels.StaffVM
{
    public class InsertMaterialViewModel : BaseViewModel
    {
        // Thuộc tính để Binding với các Control trong View (InputWindow)
        private string _materialName;
        public string MaterialName
        {
            get => _materialName;
            set { _materialName = value; OnPropertyChanged(); }
        }

        private decimal _quantity;
        public decimal Quantity
        {
            get => _quantity;
            set { _quantity = value; OnPropertyChanged(); }
        }

        // Dùng List/ObservableCollection cho các đơn vị
        public ObservableCollection<string> Units { get; set; } = new ObservableCollection<string>()
        {
            "Kg", "Lon", "Chai", "Hộp", "Hộp 1L", "Hũ"
        };

        private string _selectedUnit;
        public string SelectedUnit
        {
            get => _selectedUnit;
            set { _selectedUnit = value; OnPropertyChanged(); }
        }

        private string _note;
        public string Note
        {
            get => _note;
            set { _note = value; OnPropertyChanged(); }
        }

        public string WindowTitle { get; private set; } // Title cửa sổ
        public string SaveButtonContent { get; private set; } // Nội dung nút Lưu/Cập nhật

        // Commands
        public ICommand SaveCommand { get; set; }
        public ICommand CloseWindowCommand { get; set; } // Để đóng cửa sổ

        // Dữ liệu tham chiếu: Dùng để cập nhật lên DataGrid chính
        private readonly ObservableCollection<StaffDepotViewModel.DepotItem> _depotItems;
        private StaffDepotViewModel.DepotItem _itemToEdit; // Chỉ dùng cho chế độ SỬA

        // Constructor dùng cho chế độ THÊM MỚI (ADD)
        public InsertMaterialViewModel(ObservableCollection<StaffDepotViewModel.DepotItem> itemsCollection)
        {
            _depotItems = itemsCollection;
            InitializeData();
            LoadCommands();

            // Setup cho chế độ Thêm mới
            this.WindowTitle = "Thêm vật tư mới";
            this.SaveButtonContent = "Thêm vật tư";
        }

        // Constructor dùng cho chế độ CẬP NHẬT (UPDATE)
        public InsertMaterialViewModel(StaffDepotViewModel.DepotItem selectedItem, ObservableCollection<StaffDepotViewModel.DepotItem> itemsCollection)
            : this(itemsCollection) // Gọi constructor Thêm mới để khởi tạo chung
        {
            // Setup cho chế độ Cập nhật
            _itemToEdit = selectedItem;
            this.WindowTitle = $"Sửa vật tư: {selectedItem.MaterialName}";
            this.SaveButtonContent = "Cập nhật";

            // Đổ dữ liệu cũ vào các Property để Binding
            this.MaterialName = selectedItem.MaterialName;
            this.Quantity = selectedItem.Quantity;
            this.SelectedUnit = selectedItem.Unit;
            this.Note = selectedItem.Note;
        }

        private void InitializeData()
        {
            // Chọn giá trị mặc định cho đơn vị
            if (Units.Any())
            {
                SelectedUnit = Units.First();
            }
        }

        private void LoadCommands()
        {
            // Tạm thời dùng RelayCommand/BaseCommand (m cần định nghĩa lớp này)
            SaveCommand = new RelayCommand<Window>(ExecuteSave, CanExecuteSave);
            CloseWindowCommand = new RelayCommand<Window>(p => { p?.Close(); });
        }

        // Logic kiểm tra xem có thể thực hiện Save/Update không (Validation)
        private bool CanExecuteSave(Window p)
        {
            // Cần kiểm tra Validation, không chỉ đơn thuần là Quantity > 0
            if (string.IsNullOrWhiteSpace(MaterialName) || Quantity <= 0 || string.IsNullOrWhiteSpace(SelectedUnit))
                return false;

            return true;
        }

        // Logic chính: Thêm hoặc Cập nhật vào DB và ObservableCollection
        private void ExecuteSave(Window window)
        {
            // 1. Validation (Tạm thời là check rỗng/zero, chi tiết m nên dùng IDataErrorInfo)
            if (!CanExecuteSave(window))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ Tên, Số lượng (> 0) và Đơn vị hợp lệ.", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // 2. Thao tác với DB
            using (var db = new CoffeeShopContext())
            {
                if (_itemToEdit != null)
                {
                    // CHẾ ĐỘ UPDATE
                    Inventory itemToUpdate = db.Inventories.Find(_itemToEdit.MaterialId);

                    if (itemToUpdate != null)
                    {
                        // Cập nhật giá trị mới vào DB
                        itemToUpdate.MaterialName = MaterialName;
                        itemToUpdate.Quantity = Quantity;
                        itemToUpdate.Unit = SelectedUnit;
                        itemToUpdate.Note = Note;
                        db.SaveChanges();

                        // Cập nhật lại đối tượng trong ObservableCollection (DepotItem đã có INotifyPropertyChanged)
                        _itemToEdit.MaterialName = MaterialName;
                        _itemToEdit.Quantity = Quantity;
                        _itemToEdit.Unit = SelectedUnit;
                        _itemToEdit.Note = Note;
                    }
                }
                else
                {
                    // CHẾ ĐỘ THÊM MỚI (ADD)
                    Inventory newEntity = new Inventory
                    {
                        MaterialName = MaterialName,
                        Quantity = Quantity,
                        Unit = SelectedUnit,
                        Note = Note
                    };

                    db.Inventories.Add(newEntity);
                    db.SaveChanges();

                    // Ánh xạ ngược và thêm vào ObservableCollection
                    StaffDepotViewModel.DepotItem newDepotItem = new StaffDepotViewModel.DepotItem
                    {
                        MaterialId = newEntity.MaterialId, // Lấy ID từ DB
                        MaterialName = newEntity.MaterialName,
                        Quantity = newEntity.Quantity,
                        Unit = newEntity.Unit,
                        Note = newEntity.Note
                    };

                    _depotItems.Add(newDepotItem);
                }
            }

            // 3. Đóng cửa sổ sau khi thao tác xong
            window?.Close();
        }
    }
}
