using CoffeeShop.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace CoffeeShop.ViewModels.StaffVM
{
    public class InsertMaterialViewModel : BaseViewModel
    {
        // Commands
        public ICommand SaveCommand { get; set; } = null!;
        public ICommand CloseWindowCommand { get; set; } = null!;


        private string? _materialName;
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

        private decimal _quantity;
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

        public ObservableCollection<string> Units { get; set; } = new ObservableCollection<string>()
        {
            "Kg", "Lon", "Chai", "Hộp", "Hộp 1L", "Hũ"
        };

        private string _selectedUnit = string.Empty;
        public string SelectedUnit
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

        private string? _note;
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

        public string WindowTitle { get; private set; } // Title của cửa sổ Insert (Thêm mới/Sửa)
        public string SaveButtonContent { get; private set; } // Nội dung nút Lưu/Cập nhật

        private readonly ObservableCollection<StaffDepotViewModel.DepotItem> _depotItems;
        private StaffDepotViewModel.DepotItem? _itemToEdit; // Chỉ dùng cho chế độ sửa

        // Constructor Add
        public InsertMaterialViewModel(ObservableCollection<StaffDepotViewModel.DepotItem> itemsCollection)
        {
            _depotItems = itemsCollection;
            InitializeData();
            LoadCommands();

            // Setup cho chế độ Thêm mới
            this.WindowTitle = "Thêm vật tư mới";
            this.SaveButtonContent = "Thêm mới";
        }

        // Constructor Update
        public InsertMaterialViewModel(StaffDepotViewModel.DepotItem selectedItem, ObservableCollection<StaffDepotViewModel.DepotItem> itemsCollection)
            : this(itemsCollection) // Gọi constructor Thêm mới để khởi tạo chung
        {
            // Setup cho chế độ Cập nhật
            _itemToEdit = selectedItem;
            this.WindowTitle = $"Sửa vật tư: {selectedItem.MaterialName}";
            this.SaveButtonContent = "Cập nhật";

            // Gán dữ liệu của item được chọn vào các ô dữ liệu của cửa sổ
            this.MaterialName = selectedItem.MaterialName;
            this.Quantity = selectedItem.Quantity;
            this.SelectedUnit = selectedItem.Unit;
            this.Note = selectedItem.Note;
        }

        private void InitializeData()
        {
            // Chọn giá trị mặc định cho đơn vị (Giá trị đầu tiên)
            if (Units.Any())
            {
                SelectedUnit = Units.First();
            }
        }

        private void LoadCommands()
        {
            SaveCommand = new RelayCommand<Window>(ExecuteSave);
            CloseWindowCommand = new RelayCommand<Window>(p => { p?.Close(); });
        }

        // Kiểm tra dữ liệu nhập vào
        private bool CanExecuteSave(Window p)
        {
            if (string.IsNullOrWhiteSpace(MaterialName) || Quantity <= 0 || string.IsNullOrWhiteSpace(SelectedUnit))
                return false;

            return true;
        }

        private void ExecuteSave(Window window)
        {
            // Nếu dữ liệu ko hợp lệ
            if (!CanExecuteSave(window))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ Tên, Số lượng (> 0) và Đơn vị hợp lệ.", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Thao tác Thêm/Cập nhật dữ liệu
            using (var db = new CoffeeShopContext())
            {
                if (_itemToEdit != null)
                {
                    // Update
                    Inventory? itemToUpdate = db.Inventories.Find(_itemToEdit.MaterialId);

                    if (itemToUpdate != null)
                    {
                        // Cập nhật giá trị mới vào DB
                        itemToUpdate.MaterialName = MaterialName;
                        itemToUpdate.Quantity = Quantity;
                        itemToUpdate.Unit = SelectedUnit;
                        itemToUpdate.Note = Note;
                        db.SaveChanges();

                        // Cập nhật lại đối tượng trong ObservableCollection
                        _itemToEdit.MaterialName = MaterialName;
                        _itemToEdit.Quantity = Quantity;
                        _itemToEdit.Unit = SelectedUnit;
                        _itemToEdit.Note = Note;
                    }
                }
                else
                {
                    // Add
                    Inventory newItem = new Inventory
                    {
                        MaterialName = MaterialName,
                        Quantity = Quantity,
                        Unit = SelectedUnit,
                        Note = Note
                    };

                    db.Inventories.Add(newItem);
                    db.SaveChanges(); // Thêm item vào csdl

                    // Ánh xạ ngược và thêm vào ObservableCollection
                    StaffDepotViewModel.DepotItem newDepotItem = new StaffDepotViewModel.DepotItem
                    {
                        MaterialId = newItem.MaterialId, // Lấy ID từ DB
                        MaterialName = newItem.MaterialName,
                        Quantity = newItem.Quantity,
                        Unit = newItem.Unit,
                        Note = newItem.Note
                    };
                    // Cập nhật item vào dg
                    _depotItems.Add(newDepotItem);
                }
                // Đóng cửa sổ sau khi thao tác xong
                window?.Close();
            }
        }
    }
}
